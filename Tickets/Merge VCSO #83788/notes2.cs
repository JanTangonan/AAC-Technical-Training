using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PLCCONTROLS;
using PLCGlobals;
using System.Drawing;

namespace BEASTiLIMS
{
    public partial class LabWork : PageBase
    {
        PLCCommon PLCCommonClass = new PLCCommon();

        private const string PrintSRHistoryText = "Print SR History";
        private const string PrintReportText = "Print Lab Report";
        private const string RelatedAssignmentsText = "Related Assignments";

        private enum LabWorkView
        {
            ServiceRequest,
            Section,
            Assignment
        }

        private LabWorkView CurrentView
        {
            get
            {
                return radServiceRequest.Checked
                    ? LabWorkView.ServiceRequest
                    : radSection.Checked
                        ? LabWorkView.Section
                        : LabWorkView.Assignment;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Set page title.
            ((MasterPage)Master).SetCaseTitle(""); //AACI 10/22/2010

            lblMessage.Text = "";

            if (!IsPostBack)
            {
                if (string.IsNullOrEmpty(PLCSession.PLCGlobalCaseKey))
                    Response.Redirect("~/Dashboard.aspx");

                string previousPage = PLCSession.GetProperty<string>("PreviousPage", string.Empty);
                PLCSession.SetProperty<string>("PreviousPage", null);
                if (previousPage.ToString().ToUpper().Contains("SERVICEREQUEST.ASPX") ||
                    previousPage.ToString().ToUpper().Contains("SERVICEREQUESTWIZARD.ASPX"))
                {
                    PLCCommonClass.SetSelectedMenuItem(MainMenuTab.ServiceRequestTab,
                                                       (Menu)Master.FindControl("menu_main"));
                }
                else
                {
                    //This page is accessible only thru ServiceRequest.aspx.
                    Response.Redirect("~/Dashboard.aspx");
                    //PLCCommonClass.SetSelectedMenuItem(MainMenuTab.AssignmentsTab,
                    //                                   (Menu)Master.FindControl("menu_main"));
                }

                divTV.Attributes["onscroll"] = String.Format("OnContainerScroll(this,'{0}');", hdnScrollPos.ClientID);

                if (!string.IsNullOrEmpty(PLCSession.PLCGlobalSRMasterKey))
                    fbSR.SelectedValue = PLCSession.PLCGlobalSRMasterKey;

                PopulateTreeView();              
                InitCaseLinkButton();
            }

            hdnDBPanelClientID.Value = dbpManageSR.ClientID;
            btnManageSR.Visible = PLCSession.CheckUserOption("MANAGESRLABWORK") && (CurrentView == LabWorkView.ServiceRequest);
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "_addToList", dbpManageSR.GetAddToListScript(), true);
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "_fixCalendarPosition", "fixCalendarPosition();", true);
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            ScriptManager.RegisterStartupScript(this, typeof(Page), "scrollpos",
                    String.Format("SetPostbackScrollPos('{0}',{1});", divTV.ClientID, hdnScrollPos.Value),
                    true);
        }

        protected void Page_Init()
        {
            //fix/work-around for error 
            //"Two components with the same id 'someId' can’t be added to the application"
            ScriptManager scriptManager = (ScriptManager)this.Master.FindControl("ScriptManager1");
            if (scriptManager != null)
            {
                scriptManager.ScriptMode = ScriptMode.Release;
            }
        }

        protected void View_CheckedChanged(object sender, EventArgs e)
        {
            fbSR.Enabled = CurrentView == LabWorkView.ServiceRequest;
            
            PopulateTreeView();
        }

        protected void fbSR_ValueChanged(object sender, EventArgs e)
        {
            PopulateTreeView();
        }

        protected void trvSR_SelectedNodeChanged(object sender, EventArgs e)
        {
            ShowDetails();
        }

        protected void btnPrint_Click(object sender, EventArgs e)
        {
            if (btnPrint.Text == PrintSRHistoryText)
            {
                string srMasterKey = btnPrint.CommandArgument;

                PLCSession.PLCCrystalReportName = PLCSession.FindCrystalReport("SRHistory.rpt");
                PLCSession.PLCCrystalSelectionFormula = "{TV_SRMASTER.SR_MASTER_KEY} = " + srMasterKey;
                PLCSession.PLCCrystalReportTitle = "Service Request History";
                PLCSession.PrintCRWReport(true);
            }
            else
            {
                string assignmentKey = btnPrint.CommandArgument;

                PLCSession.PLCCrystalReportName = "";
                Session["PDFViewer_Regenerate"] = false;
                Session["PDFViewer_FileSourceKey"] = assignmentKey;
                Session["PDFViewer_FileSource"] = "REPORT";

                PLCSession.PLCGlobalAssignmentKey = assignmentKey;
                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), "uniqueKey" + DateTime.Now, "window.open('./ShowReportPDF.aspx','_blank','titlebar=no,status=no,toolbar=no,location=no,resizable=yes');", true);

                // Write audit log for printing this lab report.
                PLCQuery qryLabRept = new PLCQuery();
                qryLabRept.SQL = string.Format("SELECT * FROM TV_LABREPT WHERE CASE_KEY = {0} AND EXAM_KEY = {1}", PLCSession.PLCGlobalCaseKey, assignmentKey);
                qryLabRept.Open();
                if (qryLabRept.HasData())
                    PLCDBGlobal.instance.WriteLabRptPrintAuditLog(qryLabRept.FieldByName("CASE_KEY"), qryLabRept.FieldByName("EXAM_KEY"), qryLabRept.FieldByName("SECTION"), qryLabRept.FieldByName("ANALYST_ASSIGNED"), qryLabRept.FieldByName("DATE_COMPLETED"), "LabWork.aspx");
            }
        }

        protected void btnBackToSR_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/ServiceRequest.aspx");
        }

        protected void PopulateTreeView()
        {
            hdnScrollPos.Value = "0";
            btnPrint.Visible = false;
            trvSR.Nodes.Clear();

            switch (CurrentView)
            {
                case LabWorkView.ServiceRequest:
                    GroupByServiceRequest();
                    break;
                case LabWorkView.Section:
                    GroupBySection();
                    break;
                case LabWorkView.Assignment:
                    GroupByAssignment(); 
                    break;
                default:
                    lblMessage.Text = "Please select a service request.";
                    break;
            }
        }

        private void GroupByServiceRequest()
        {
            PLCQuery qrySRMaster = new PLCQuery(PLCSession.FormatSpecialFunctions(@"SELECT SRM.SR_MASTER_KEY, A.NAME, SRM.REQUESTED_DATE, SRD.SR_DETAIL_KEY, SRD.EXAM_KEY, 
E.DESCRIPTION AS SECTIONDESC, ES.DESCRIPTION AS STATUS, I.LAB_ITEM_NUMBER, IT.DESCRIPTION AS ITEM_TYPE, I.ITEM_DESCRIPTION, S.STATUS_TYPE_DESCRIPTION, SRD.STATUS_CODE FROM TV_SRMASTER SRM
INNER JOIN TV_SRDETAIL SRD ON SRM.SR_MASTER_KEY = SRD.SR_MASTER_KEY
LEFT OUTER JOIN TV_LABASSIGN ASG ON SRD.EXAM_KEY = ASG.EXAM_KEY
LEFT OUTER JOIN TV_EXAMSTAT ES ON ASG.STATUS = ES.EXAM_STATUS
LEFT OUTER JOIN TV_ANALYST A ON SRM.REQUESTED_BY = A.ANALYST
LEFT OUTER JOIN TV_EXAMCODE E ON SRD.SECTION = E.EXAM_CODE
LEFT OUTER JOIN TV_LABITEM I ON SRD.EVIDENCE_CONTROL_NUMBER = I.EVIDENCE_CONTROL_NUMBER
LEFT OUTER JOIN TV_ITEMTYPE IT ON I.ITEM_TYPE = IT.ITEM_TYPE
LEFT OUTER JOIN TV_SRSTATUS S ON SRM.STATUS = S.STATUS_TYPE
WHERE SRM.CASE_KEY = " + PLCSession.PLCGlobalCaseKey + (fbSR.SelectedValue != "" ? " AND SRM.SR_MASTER_KEY = " + fbSR.SelectedValue.ToString() : "") + " ORDER BY SRM.REQUESTED_DATE " ));
            qrySRMaster.Open();
            if (!qrySRMaster.IsEmpty())
            {

                //get master records
                var masters = from a in qrySRMaster.PLCDataTable.AsEnumerable()
                              group a by new
                              {
                                  //MasterKey = a.Field<decimal>("SR_MASTER_KEY"),
                                  //Analyst = a.Field<string>("NAME"),
                                  //RequestDate = a.Field<DateTime>("REQUESTED_DATE")
                                  
                                  MasterKey = Convert.ToInt32(a["SR_MASTER_KEY"]),
                                  Analyst = Convert.ToString(a["NAME"]),
                                  RequestDate = string.IsNullOrEmpty(a["REQUESTED_DATE"].ToString()) ? "" : Convert.ToDateTime(a["REQUESTED_DATE"]).ToString(PLCSession.GetDateFormat()),
                                  StatusDesc = Convert.ToString(a["STATUS_TYPE_DESCRIPTION"])
                              } into g
                              select g;


                
                foreach (var master in masters)
                {
                    //add parent nodes

                    TreeNode node = new TreeNode("Requested on " + master.Key.RequestDate.ToString() + " by " + master.Key.Analyst + "- Request Status (" + master.Key.StatusDesc.ToString() + ")", master.Key.MasterKey.ToString());
                    node.SelectAction = TreeNodeSelectAction.SelectExpand;
                    trvSR.Nodes.Add(node);

                    //get detail records
                    var details = from a in qrySRMaster.PLCDataTable.AsEnumerable()
                                  //where a.Field<decimal>("SR_MASTER_KEY") == master.Key.MasterKey
                                  where Convert.ToInt32(a["SR_MASTER_KEY"]) == master.Key.MasterKey
                                  //orderby a.Field<decimal>("SR_DETAIL_KEY")
                                  orderby a["SR_DETAIL_KEY"]
                                  select new
                                  {
                                      //DetailKey = a.Field<decimal>("SR_DETAIL_KEY"),
                                      //ExamKey = a.Field<decimal?>("EXAM_KEY") == null ? 0 : a.Field<decimal?>("EXAM_KEY"),
                                      //Section = a.Field<string>("SECTIONDESC"),
                                      //Status = string.IsNullOrEmpty(a.Field<string>("STATUS")) ? "No assignment" : a.Field<string>("STATUS")
                                      DetailKey = Convert.ToInt32(a["SR_DETAIL_KEY"]),
                                      ExamKey = ((a["EXAM_KEY"] == null) ? 0 : Convert.ToInt32(a["EXAM_KEY"])),
                                      Section = Convert.ToString(a["SECTIONDESC"]),
                                      Status = ((string.IsNullOrEmpty(Convert.ToString(a["STATUS"]))) ? "No assignment" : "Assignment status(" + Convert.ToString(a["STATUS"]) + ")"),
                                      LabItemNumber = Convert.ToString(a["LAB_ITEM_NUMBER"]),
                                      ItemType = Convert.ToString(a["ITEM_TYPE"]),
                                      ItemDescription = Convert.ToString(a["ITEM_DESCRIPTION"]),
                                      StatusDesc = " Request status(" + GetStatusDesc(Convert.ToString(a["STATUS_CODE"])) + ")"
                                  };

                    foreach (var detail in details)
                    {
                        string itemInfo = "Item #" + detail.LabItemNumber +
                            (!string.IsNullOrEmpty(detail.ItemType) ? " - " + detail.ItemType : "") +
                            (!string.IsNullOrEmpty(detail.ItemDescription) ? " - " + detail.ItemDescription : "") +
                            " - ";
                        //add child nodes
                        TreeNode childNode = new TreeNode(itemInfo + detail.Section + ", " + detail.Status + "," + detail.StatusDesc, Convert.ToString(detail.DetailKey) + "_" + Convert.ToString(detail.ExamKey));
                        childNode.SelectAction = TreeNodeSelectAction.Select;
                        node.ChildNodes.Add(childNode);
                    }
                }

                //show details of first node
                trvSR.Nodes[0].Select();
                ShowDetails();
            }
        }

        private void GroupBySection()
        {
            PLCQuery qrySRMaster = new PLCQuery(PLCSession.FormatSpecialFunctions(@"SELECT SRM.SR_MASTER_KEY, A.NAME, SRM.REQUESTED_DATE, SRD.SR_DETAIL_KEY, SRD.EXAM_KEY, 
ASG.DATE_ASSIGNED, SRD.SECTION, E.DESCRIPTION AS SECTIONDESC, ES.DESCRIPTION AS STATUS, I.ITEM_DESCRIPTION FROM TV_SRMASTER SRM
INNER JOIN TV_SRDETAIL SRD ON SRM.SR_MASTER_KEY = SRD.SR_MASTER_KEY
INNER JOIN TV_LABASSIGN ASG ON SRD.EXAM_KEY = ASG.EXAM_KEY
INNER JOIN TV_LABITEM I ON SRD.EVIDENCE_CONTROL_NUMBER = I.EVIDENCE_CONTROL_NUMBER
LEFT OUTER JOIN TV_EXAMSTAT ES ON ASG.STATUS = ES.EXAM_STATUS
LEFT OUTER JOIN TV_ANALYST A ON SRM.REQUESTED_BY = A.ANALYST
LEFT OUTER JOIN TV_EXAMCODE E ON SRD.SECTION = E.EXAM_CODE
WHERE SRM.CASE_KEY = " + PLCSession.PLCGlobalCaseKey));
            qrySRMaster.Open();
            if (!qrySRMaster.IsEmpty())
            {
                //get sections
                var sections = from a in qrySRMaster.PLCDataTable.AsEnumerable()
                               group a by new
                               {
                                   //Code = a.Field<string>("SECTION"),
                                   //Description = a.Field<string>("SECTIONDESC")
                                   Code = Convert.ToString(a["SECTION"]),
                                   Description = Convert.ToString(a["SECTIONDESC"])
                               } into g
                               orderby g.Key.Description
                               select g;

                foreach (var section in sections)
                {
                    //add parent nodes
                    TreeNode node = new TreeNode(section.Key.Description, section.Key.Code);
                    node.SelectAction = TreeNodeSelectAction.SelectExpand;
                    trvSR.Nodes.Add(node);

                    //get assignments for each section
                    var assignments = from a in qrySRMaster.PLCDataTable.AsEnumerable()
                                      group a by new
                                      {
                                          ExamKey = Convert.ToInt32(a["EXAM_KEY"]),
                                          Section = Convert.ToString(a["SECTION"]),
                                          Status = string.IsNullOrEmpty(Convert.ToString(a["STATUS"])) ? "-" : Convert.ToString(a["STATUS"]),
                                          DateAssigned = Convert.ToDateTime(a["DATE_ASSIGNED"])
                                      } into g
                                  //where a.Field<string>("SECTION") == section.Key.Code
                                  where g.Key.Section == section.Key.Code
                                  //orderby a.Field<DateTime>("DATE_ASSIGNED")
                                  orderby g.Key.DateAssigned
                                  select new
                                  {
                                      //DetailKey = a.Field<decimal>("SR_DETAIL_KEY"),
                                      //ExamKey = a.Field<decimal>("EXAM_KEY"),
                                      //Section = a.Field<string>("SECTION"),
                                      //Status = string.IsNullOrEmpty(a.Field<string>("STATUS")) ? "-" : a.Field<string>("STATUS")
                                      ExamKey = g.Key.ExamKey,
                                      Section = g.Key.Section,
                                      Status = g.Key.Status
                                  };

                    foreach (var assignment in assignments)
                    {
                        //add child nodes
                        TreeNode childNode = new TreeNode(assignment.Status, Convert.ToString(assignment.ExamKey));
                        childNode.SelectAction = TreeNodeSelectAction.SelectExpand;
                        node.ChildNodes.Add(childNode);

                        //get service requests of each assignment
                        var srdetails = from a in qrySRMaster.PLCDataTable.AsEnumerable()
                                        //where a.Field<string>("SECTION") == section.Key.Code
                                        //&& a.Field<decimal>("EXAM_KEY") == assignment.ExamKey
                                        where Convert.ToString(a["SECTION"]) == section.Key.Code
                                        && Convert.ToInt32(a["EXAM_KEY"]) == assignment.ExamKey
                                        //orderby a.Field<decimal>("SR_DETAIL_KEY")
                                        orderby Convert.ToInt32(a["SR_DETAIL_KEY"])
                                        select new
                                        {
                                            //DetailKey = a.Field<decimal>("SR_DETAIL_KEY"),
                                            //Analyst = a.Field<string>("NAME"),
                                            //RequestDate = a.Field<DateTime>("REQUESTED_DATE"),
                                            //Item = string.IsNullOrEmpty(a.Field<string>("ITEM_DESCRIPTION")) ? "-" : a.Field<string>("ITEM_DESCRIPTION")
                                            DetailKey = Convert.ToInt32(a["SR_DETAIL_KEY"]),
                                            Analyst = Convert.ToString(a["NAME"]),
                                            RequestDate = string.IsNullOrEmpty(a["REQUESTED_DATE"].ToString()) ? "" : Convert.ToDateTime(a["REQUESTED_DATE"]).ToString(PLCSession.GetDateFormat()),
                                            Item = string.IsNullOrEmpty(Convert.ToString(a["ITEM_DESCRIPTION"])) ? "-" : Convert.ToString(a["ITEM_DESCRIPTION"])
                                        };

                        foreach (var srdetail in srdetails)
                        {
                            //add srdetail node
                            TreeNode srdetailNode = new TreeNode("Requested on " + srdetail.RequestDate.ToString() + " by " + srdetail.Analyst + ", " + srdetail.Item, srdetail.DetailKey.ToString());
                            srdetailNode.SelectAction = TreeNodeSelectAction.None;
                            childNode.ChildNodes.Add(srdetailNode);
                        }
                    }
                }

                //show details of first node
                trvSR.Nodes[0].Select();
                ShowDetails();
            }
        }

        private void GroupByAssignment()
        {
            string sql = "";
            if (PLCSession.PLCDatabaseServer == "ORACLE")
                sql = @"SELECT SRM.SR_MASTER_KEY, A.NAME, SRM.REQUESTED_DATE, SRD.SR_DETAIL_KEY, SRD.EXAM_KEY, 
ASG.DATE_ASSIGNED, SRD.SECTION, E.DESCRIPTION AS SECTIONDESC, ES.DESCRIPTION AS STATUS, I.ITEM_DESCRIPTION, SECTION_NUMBER,
(SELECT COUNT(EXAM_KEY) FROM TV_LABASSIGN WHERE SECTION_NUMBER = ASG.SECTION_NUMBER AND EXAM_KEY <> ASG.EXAM_KEY AND SECTION = ASG.SECTION AND SECTION_NUMBER IS NOT NULL) AS RELATEDASSIGNMENTS 
FROM TV_SRMASTER SRM
INNER JOIN TV_SRDETAIL SRD ON SRM.SR_MASTER_KEY = SRD.SR_MASTER_KEY
INNER JOIN TV_LABASSIGN ASG ON SRD.EXAM_KEY = ASG.EXAM_KEY
INNER JOIN TV_LABITEM I ON SRD.EVIDENCE_CONTROL_NUMBER = I.EVIDENCE_CONTROL_NUMBER
LEFT OUTER JOIN TV_EXAMSTAT ES ON ASG.STATUS = ES.EXAM_STATUS
LEFT OUTER JOIN TV_ANALYST A ON SRM.REQUESTED_BY = A.ANALYST
LEFT OUTER JOIN TV_EXAMCODE E ON SRD.SECTION = E.EXAM_CODE
WHERE SRM.CASE_KEY = " + PLCSession.PLCGlobalCaseKey;
            else
                sql = @"SELECT SRM.SR_MASTER_KEY, A.NAME, SRM.REQUESTED_DATE, SRD.SR_DETAIL_KEY, SRD.EXAM_KEY, 
ASG.DATE_ASSIGNED, SRD.SECTION, E.DESCRIPTION AS SECTIONDESC, ES.DESCRIPTION AS STATUS, I.ITEM_DESCRIPTION, SECTION_NUMBER,
(SELECT COUNT(EXAM_KEY) FROM TV_LABASSIGN WHERE SECTION_NUMBER = ASG.SECTION_NUMBER AND EXAM_KEY <> ASG.EXAM_KEY AND SECTION = ASG.SECTION AND SECTION_NUMBER IS NOT NULL AND SECTION_NUMBER != '') AS RELATEDASSIGNMENTS 
FROM TV_SRMASTER SRM
INNER JOIN TV_SRDETAIL SRD ON SRM.SR_MASTER_KEY = SRD.SR_MASTER_KEY
INNER JOIN TV_LABASSIGN ASG ON SRD.EXAM_KEY = ASG.EXAM_KEY
INNER JOIN TV_LABITEM I ON SRD.EVIDENCE_CONTROL_NUMBER = I.EVIDENCE_CONTROL_NUMBER
LEFT OUTER JOIN TV_EXAMSTAT ES ON ASG.STATUS = ES.EXAM_STATUS
LEFT OUTER JOIN TV_ANALYST A ON SRM.REQUESTED_BY = A.ANALYST
LEFT OUTER JOIN TV_EXAMCODE E ON SRD.SECTION = E.EXAM_CODE
WHERE SRM.CASE_KEY = " + PLCSession.PLCGlobalCaseKey;

            PLCQuery qrySRMaster = new PLCQuery(PLCSession.FormatSpecialFunctions(sql));
            qrySRMaster.Open();
            if (!qrySRMaster.IsEmpty())
            {
                //get assignments
                var assignments = from a in qrySRMaster.PLCDataTable.AsEnumerable()
                                  group a by new
                                  {
                                      //ExamKey = a.Field<decimal>("EXAM_KEY"),
                                      //Section = a.Field<string>("SECTIONDESC"),
                                      //Status = string.IsNullOrEmpty(a.Field<string>("STATUS")) ? "-" : a.Field<string>("STATUS"),
                                      //DateAssigned = a.Field<DateTime>("DATE_ASSIGNED"),
                                      //SectionNumber = a.Field<string>("SECTION_NUMBER"),
                                      //RelatedAssignments = a.Field<decimal>("RELATEDASSIGNMENTS")
                                      ExamKey = Convert.ToInt32(a["EXAM_KEY"]),
                                      Section = Convert.ToString(a["SECTIONDESC"]),
                                      Status = string.IsNullOrEmpty(Convert.ToString(a["STATUS"])) ? "-" : Convert.ToString(a["STATUS"]),
                                      DateAssigned = Convert.ToDateTime(a["DATE_ASSIGNED"]),
                                      SectionNumber = Convert.ToString(a["SECTION_NUMBER"]),
                                      RelatedAssignments = Convert.ToInt32(a["RELATEDASSIGNMENTS"])
                                  } into g
                                  orderby g.Key.DateAssigned
                                  select g;

                foreach (var assignment in assignments)
                {
                    //add parent nodes
                    TreeNode node = new TreeNode(assignment.Key.Section + ", " + assignment.Key.Status, Convert.ToString(assignment.Key.ExamKey));
                    node.SelectAction = TreeNodeSelectAction.SelectExpand;
                    trvSR.Nodes.Add(node);

                    //get service requests of each assignment
                    var srdetails = from a in qrySRMaster.PLCDataTable.AsEnumerable()
                                    //where a.Field<decimal>("EXAM_KEY") == assignment.Key.ExamKey
                                    where Convert.ToInt32(a["EXAM_KEY"]) == assignment.Key.ExamKey
                                    //orderby a.Field<decimal>("SR_DETAIL_KEY")
                                    orderby Convert.ToInt32(a["SR_DETAIL_KEY"])
                                    select new
                                    {
                                        //DetailKey = a.Field<decimal>("SR_DETAIL_KEY"),
                                        //Analyst = a.Field<string>("NAME"),
                                        //RequestDate = a.Field<DateTime>("REQUESTED_DATE"),
                                        //Item = string.IsNullOrEmpty(a.Field<string>("ITEM_DESCRIPTION")) ? "-" : a.Field<string>("ITEM_DESCRIPTION")
                                        DetailKey = Convert.ToInt32(a["SR_DETAIL_KEY"]),
                                        Analyst = Convert.ToString(a["NAME"]),
                                        RequestDate = string.IsNullOrEmpty(a["REQUESTED_DATE"].ToString()) ? "" : Convert.ToDateTime(a["REQUESTED_DATE"]).ToString(PLCSession.GetDateFormat()),
                                        Item = string.IsNullOrEmpty(Convert.ToString(a["ITEM_DESCRIPTION"])) ? "-" : Convert.ToString(a["ITEM_DESCRIPTION"])
                                    };

                    foreach (var srdetail in srdetails)
                    {
                        //add child nodes
                        TreeNode childNode = new TreeNode("Requested on " + srdetail.RequestDate.ToString() + " by " + srdetail.Analyst + ", " + srdetail.Item, Convert.ToString(srdetail.DetailKey));
                        childNode.SelectAction = TreeNodeSelectAction.None;
                        node.ChildNodes.Add(childNode);
                    }

                    //get related assignments
                    if (assignment.Key.RelatedAssignments > 0)
                    {
                        //related assignments label node
                        TreeNode labelNode = new TreeNode(RelatedAssignmentsText);
                        labelNode.SelectAction = TreeNodeSelectAction.SelectExpand;
                        node.ChildNodes.Add(labelNode);

                        PLCQuery qryRelated = new PLCQuery(@"SELECT EXAM_KEY, E.DESCRIPTION AS SECTION, ES.DESCRIPTION AS STATUS FROM TV_LABASSIGN A
LEFT OUTER JOIN TV_EXAMCODE E ON A.SECTION = E.EXAM_CODE
LEFT OUTER JOIN TV_EXAMSTAT ES ON A.STATUS = ES.EXAM_STATUS 
WHERE SECTION_NUMBER = '" + assignment.Key.SectionNumber + "' AND EXAM_KEY <> " + assignment.Key.ExamKey);
                        qryRelated.Open();
                        if (!qryRelated.IsEmpty())
                        {
                            while (!qryRelated.EOF())
                            {
                                TreeNode relatedNode = new TreeNode(qryRelated.FieldByName("SECTION") + ", " + qryRelated.FieldByName("STATUS"), qryRelated.FieldByName("EXAM_KEY"));
                                relatedNode.SelectAction = TreeNodeSelectAction.Select;
                                labelNode.ChildNodes.Add(relatedNode);

                                qryRelated.Next();
                            }
                        }
                    }
                }

                //show details of first node
                trvSR.Nodes[0].Select();
                ShowDetails();
            }
        }

        /// <summary>
        /// Validates if the selected assignment has pdf data stored
        /// </summary>
        /// <param name="assignmentKey">The assignment whose pdf data will be verified</param>
        /// <returns>Returns true if the selected assignment as pdf data stored</returns>
        private bool CheckReportPdfRecord(string assignmentKey)
        {
            bool hasPdfRecord = false;
            string queryString = string.Empty;
            PLCQuery queryObject = null;

            // set the query
            queryString = string.Format("SELECT DATA_KEY FROM TV_PDFDATA WHERE FILE_SOURCE_KEY = {0} AND FILE_SOURCE = 'REPORT'", assignmentKey);
            queryObject = new PLCQuery(queryString);
            queryObject.Open();

            if (queryObject.HasData())
                hasPdfRecord = true;

            return hasPdfRecord;
        }

                /// <summary>
        /// Display the Assignment detail panel and set the print button properties for it
        /// </summary>
        /// <param name="assignmentKey">The labassign key whose details will be displayed</param>
        /// <returns>Returns true if no errors are found</returns>
        private bool DisplayAssignmentDetailsPanel(string assignmentKey)
        {
            bool hasPdfRecord = false;

            // set the print button configuration
            SetPrintButtonDefaultConfiguration();
            btnPrint.Text = PrintReportText;

            // load panel record
            dbpAssignmentDetails.PLCWhereClause = string.Format(" WHERE EXAM_KEY = {0}", assignmentKey);
            dbpAssignmentDetails.DoLoadRecord();
            dbpAssignmentDetails.SetBrowseMode();

            // check the pdf data of the assignment 
            hasPdfRecord = CheckReportPdfRecord(assignmentKey);

            if (assignmentKey != string.Empty && hasPdfRecord)
            {
                // set the print button configurations
                btnPrint.Enabled = true;
                btnPrint.CommandArgument = assignmentKey;
            }

            // display the panel
            divAssignmentDetails.Visible = true;

            return true;
        }

        /// <summary>
        /// Displays the SR Master Detail panel and set the print button properties for it
        /// </summary>
        /// <param name="srMasterKey">The srmaster key whose details will be displayed</param>
        /// <returns>Returns true if no errors are found</returns>
        private bool DisplaySRMasterDetailPanel(string srMasterKey)
        {
            // set the print button configuration
            SetPrintButtonDefaultConfiguration();

            // load panel record
            dbpSRMasterDetails.PLCWhereClause = string.Format(" WHERE SR_MASTER_KEY = {0}", srMasterKey);
            dbpSRMasterDetails.DoLoadRecord();
            dbpSRMasterDetails.SetBrowseMode();

            if (srMasterKey != string.Empty)
            {
                // set the print button configurations
                btnPrint.Enabled = true;
                btnPrint.CommandArgument = srMasterKey;
            }

            // display the panel 
            divSRMasterDetails.Visible = true;

            return true;
        }

        private void ShowDetails()
        {
            string panelToDisplay = string.Empty;

            // hide all the details panel
            HideDetailsPanel();
            litDetails.Text = string.Empty;

            // retrieve the panel to display
            panelToDisplay = GetPanelToDisplay();

            // check if the panel is available
            if (IsDBPanelAvailable(panelToDisplay))
            {
                // display appropriate details according to the current view
                switch (CurrentView)
                {
                    case LabWorkView.ServiceRequest:
                        if (trvSR.SelectedNode.Depth == 0) // srmaster
                            DisplaySRMasterDetailPanel(trvSR.SelectedNode.Value);
                        else if (trvSR.SelectedNode.Depth == 1) //srdetail-assignment
                            DisplayAssignmentDetailsPanel(trvSR.SelectedNode.Value.Substring(trvSR.SelectedNode.Value.IndexOf("_") + 1));
                        break;
                    case LabWorkView.Section:
                        if (trvSR.SelectedNode.Depth == 1) //assignment
                            DisplayAssignmentDetailsPanel(trvSR.SelectedNode.Value);
                        break;
                    case LabWorkView.Assignment:
                        if (trvSR.SelectedNode.Depth == 0) //assignment
                            DisplayAssignmentDetailsPanel(trvSR.SelectedNode.Value);
                        else if (trvSR.SelectedNode.Depth == 2) //related assignment
                            DisplayAssignmentDetailsPanel(trvSR.SelectedNode.Value);
                        break;
                }
            }
            else
            {
                ShowDetailsText();
            }
        }

        private void ShowDetailsText()
        {
            string srDetails = "";

            switch (CurrentView)
            {
                case LabWorkView.ServiceRequest:
                    if (trvSR.SelectedNode.Depth == 0) //srmaster
                        srDetails = SRMasterDetails(trvSR.SelectedNode.Value);
                    else if (trvSR.SelectedNode.Depth == 1) //srdetail-assignment
                        srDetails = AssignmentDetails(trvSR.SelectedNode.Value.Substring(trvSR.SelectedNode.Value.IndexOf("_") + 1));
                    break;
                case LabWorkView.Section:
                    if (trvSR.SelectedNode.Depth == 1) //assignment
                        srDetails = AssignmentDetails(trvSR.SelectedNode.Value);
                    break;
                case LabWorkView.Assignment:
                    if (trvSR.SelectedNode.Depth == 0) //assignment
                        srDetails = AssignmentDetails(trvSR.SelectedNode.Value);
                    else if (trvSR.SelectedNode.Depth == 2) //related assignment
                        srDetails = AssignmentDetails(trvSR.SelectedNode.Value);
                    break;
            }

            litDetails.Text = srDetails;
        }

        private string SRMasterDetails(string srMasterKey)
        {
            string srDetails = "";

            btnPrint.Visible = true;
            btnPrint.Text = PrintSRHistoryText;
            btnPrint.Enabled = false;
            btnPrint.CommandArgument = "0";

            PLCQuery qrySR = new PLCQuery(@"SELECT REQUESTED_DATE, A.NAME AS REQUESTED_BY, REQUEST_COMMENTS FROM TV_SRMASTER SRM
LEFT OUTER JOIN TV_ANALYST A ON SRM.REQUESTED_BY = A.ANALYST WHERE SR_MASTER_KEY = " + srMasterKey);
            qrySR.Open();
            if (!qrySR.IsEmpty())
            {
                srDetails = "<div style='margin: 10px; padding: 8px; border: solid 1px #c0c0c0;'>" +
                    "<span style='font-weight: bold; text-decoration: underline;'>Service Request Details</span><br /><br />" +
                    "<b>Request Date:</b> " + DetailsText(qrySR.FieldByName("REQUESTED_DATE"), true) + "<br />" +
                    "<b>Requested By:</b> " + DetailsText(qrySR.FieldByName("REQUESTED_BY")) + "<br />" +
                    "<b>Comments:</b> " + DetailsText(qrySR.FieldByName("REQUEST_COMMENTS")) + "</div>";

                btnPrint.Enabled = true;
                btnPrint.CommandArgument = srMasterKey;
            }

            return srDetails;
        }

        private string AssignmentDetails(string assignmentKey)
        {
            string srDetails = "";

            btnPrint.Visible = true;
            btnPrint.Text = PrintReportText;
            btnPrint.Enabled = false;
            btnPrint.CommandArgument = "0";

            if (assignmentKey != "0")
            {
                PLCQuery qryReport = new PLCQuery(@"SELECT REPORT_NUMBER, E.DESCRIPTION AS SECTION, A.NAME AS ANALYST_ASSIGNED, DATE_ASSIGNED, DATE_COMPLETED, DATA_KEY AS PDF FROM TV_LABASSIGN LA
LEFT OUTER JOIN TV_EXAMCODE E ON LA.SECTION = E.EXAM_CODE
LEFT OUTER JOIN TV_ANALYST A ON LA.ANALYST_ASSIGNED = A.ANALYST
LEFT OUTER JOIN TV_PDFDATA P ON LA.EXAM_KEY = P.FILE_SOURCE_KEY AND FILE_SOURCE = 'REPORT'
WHERE EXAM_KEY = " + assignmentKey);
                qryReport.Open();
                if (!qryReport.IsEmpty())
                {
                    srDetails = "<div style='margin: 10px; padding: 8px; border: solid 1px #c0c0c0;'>" +
                        "<span style='font-weight: bold; text-decoration: underline;'>Report Details</span><br /><br />" +
                        "<b>Report Number:</b> " + DetailsText(qryReport.FieldByName("REPORT_NUMBER")) + "<br />" +
                        "<b>Section:</b> " + DetailsText(qryReport.FieldByName("SECTION")) + "<br />" +
                        "<b>Analyst:</b> " + DetailsText(qryReport.FieldByName("ANALYST_ASSIGNED")) + "<br />" +
                        "<b>Date Assigned:</b> " + DetailsText(qryReport.FieldByName("DATE_ASSIGNED"), true) + "<br />" +
                        "<b>Date Completed:</b> " + DetailsText(qryReport.FieldByName("DATE_COMPLETED"), true) + "<br />" + "</div>";

                    if (!string.IsNullOrEmpty(qryReport.FieldByName("PDF")))
                    {
                        btnPrint.Enabled = true;
                        btnPrint.CommandArgument = assignmentKey;
                    }
                }
            }
            else
            {
                srDetails = "<div style='margin: 10px; padding: 8px; border: solid 1px #c0c0c0; color: Red;'>No assignment found for this service request.</div>";
            }

            return srDetails;
        }

        private string DetailsText(string text)
        {
            return DetailsText(text, false);
        }

        private string DetailsText(string text, bool isDate)
        {
            return string.IsNullOrEmpty(text) 
                ? "-" 
                : isDate 
                    ? Convert.ToDateTime(text).ToString(PLCSession.GetDateFormat())
                    : text;
        }


        private string GetStatusDesc(string statusCode)
        { 
            string statDesc = string.Empty;
            PLCQuery qryStatus = new PLCQuery();
            qryStatus.SQL = string.Format("SELECT STATUS_TYPE_DESCRIPTION FROM TV_SRSTATUS WHERE STATUS_TYPE = '{0}'", statusCode);
            qryStatus.Open();
            if (!qryStatus.IsEmpty())
                statDesc = qryStatus.FieldByName("STATUS_TYPE_DESCRIPTION");

            return statDesc;
                
        }

        protected void btnManageSR_Click(object sender, EventArgs e)
        {
            bool assignmentsCompleted = false;
            string assignmentKeys = "";
            if (CurrentView == LabWorkView.ServiceRequest)
            {
                if (trvSR.SelectedNode.Depth == 0)
                {
                    string masterKey = trvSR.SelectedNode.Value;
                    if (!string.IsNullOrEmpty(masterKey))
                    {
                        List<string> keys = new List<string>();
                        PLCQuery qryAssignment = new PLCQuery();
                        qryAssignment.SQL = "SELECT * FROM TV_SRDETAIL WHERE SR_MASTER_KEY = '" + masterKey + "'  AND EXAM_KEY > 0";
                        qryAssignment.Open();
                        while (!qryAssignment.EOF())
                        {
                            string examKey = qryAssignment.FieldByName("EXAM_KEY");
                            if (!keys.Contains(examKey))
                                keys.Add(examKey);

                            qryAssignment.Next();
                        }

                        if (keys.Count > 0)
                            assignmentKeys = string.Join(",", keys);
                    }

                }
                else if (trvSR.SelectedNode.Depth == 1)
                {
                    assignmentKeys = trvSR.SelectedNode.Value.Substring(trvSR.SelectedNode.Value.IndexOf("_") + 1);
                    assignmentKeys = assignmentKeys == "0" ? "" : assignmentKeys;
                }
            }

           
            if (!string.IsNullOrEmpty(assignmentKeys))
            {
                assignmentsCompleted = AssignmentCompleted(assignmentKeys);

                if (assignmentsCompleted)                
                    dlgManage.ShowAlert("Manage SR", "Assignments linked to this service request are complete. Please contact the lab for assistance");               
                else                
                    ScriptManager.RegisterStartupScript(Page, Page.GetType(), "_showManageSRPopup", "showManageSRPopup();", true);              
            }
            else
                dlgManage.ShowAlert("Manage SR", "No assignment/s");

        }

        /// <summary>
        /// Retrieve the panel details to display
        /// </summary>
        /// <returns>Returns the name of the panel to display</returns>
        private string GetPanelToDisplay()
        {
            string panelToDisplay = string.Empty;

            if (CurrentView == LabWorkView.ServiceRequest && trvSR.SelectedNode.Depth == 0)
                panelToDisplay = "LABWORK_SRMASTER_DETAILS";
            else
                panelToDisplay = "LABWORK_ASSIGNMENT_DETAILS";

            return panelToDisplay;
        }

        /// <summary>
        /// Hides all the details panel
        /// </summary>
        /// <returns>Returns true if no errors are found</returns>
        private bool HideDetailsPanel()
        {
            divAssignmentDetails.Visible = false;
            divSRMasterDetails.Visible = false;
            return true;
        }
        private bool AssignmentCompleted(string assignmentKeys)
        {
            PLCQuery qryLabAssign = new PLCQuery();
            qryLabAssign.SQL = "SELECT * FROM TV_LABASSIGN WHERE EXAM_KEY IN (" + assignmentKeys + ") AND (COMPLETED <> 'T' OR COMPLETED IS NULL) AND ((APPROVED <> 'A' AND APPROVED <> 'Y') OR APPROVED IS NULL)";
            qryLabAssign.Open();
            return qryLabAssign.IsEmpty();
        }

        /// <summary>
        /// Check if a DBPanel Configuration is available
        /// </summary>
        /// <param name="panelName">The panel name to check</param>
        /// <returns>Returns true if the panel is available</returns>
        private bool IsDBPanelAvailable(string panelName)
        {
            bool panelAvailable = false;
            PLCQuery queryObject = null;
            string queryString = string.Empty;

            // set the query
            queryString = string.Format("SELECT PANEL_NAME FROM TV_DBPANEL WHERE PANEL_NAME = '{0}'", panelName);
            queryObject = new PLCQuery(queryString);
            queryObject.Open();

            // check if a panel configuration is found
            if (queryObject.HasData())
                panelAvailable = true;

            return panelAvailable;
        }

        /// <summary>
        /// Sets the default configuration for the print button
        /// </summary>
        /// <returns>Returns true if no errors are found</returns>
        private bool SetPrintButtonDefaultConfiguration()
        {
            // set the print button configuration
            btnPrint.Visible = true;
            btnPrint.Text = PrintSRHistoryText;
            btnPrint.Enabled = false;
            btnPrint.CommandArgument = "0";

            return true;
        }

        protected void btnOKManage_Click(object sender, EventArgs e)
        {
            if (!dbpManageSR.CanSave())
            {
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "_fixCalendarPosition", "fixCalendarPosition();", true); 
                return;
            }
     

           if(CurrentView == LabWorkView.ServiceRequest)
           {
                if (trvSR.SelectedNode.Depth == 0)
                {
                    string masterKey = trvSR.SelectedNode.Value;
                    if (!string.IsNullOrEmpty(masterKey))
                    {
                        PLCQuery qryAssignment = new PLCQuery();
                        qryAssignment.SQL = "SELECT DISTINCT EXAM_KEY FROM TV_SRDETAIL WHERE SR_MASTER_KEY = '" + masterKey + "'  AND EXAM_KEY > 0";
                        qryAssignment.Open();
                        while (!qryAssignment.EOF())
                        {
                            string examKey = qryAssignment.FieldByName("EXAM_KEY");
                            UpdateAssignmentDetails(examKey);                           
                            qryAssignment.Next();
                        }
                    }

                }
                else if (trvSR.SelectedNode.Depth == 1)
                { //srdetail-assignment
                    string examKey = trvSR.SelectedNode.Value.Substring(trvSR.SelectedNode.Value.IndexOf("_") + 1);
                    if(!string.IsNullOrEmpty(examKey))
                        UpdateAssignmentDetails(examKey);
                }
           }

            dbpManageSR.ClearErrors();
            dbpManageSR.ClearFields();
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "_closeLabCaseDialog", "closeManageSRDialog('mdialog-manage');", true);
        }

        private void UpdateAssignmentDetails(string examKey)
        {
            string[] fieldNames = dbpManageSR.GetPanelFieldNames();

            //limit updating records to assignments that is not yet approved/closed
            PLCQuery qryLabExam = new PLCQuery();
            qryLabExam.SQL = "SELECT * FROM TV_LABASSIGN WHERE EXAM_KEY = '" + examKey +
                              "' AND (COMPLETED <> 'T' OR COMPLETED IS NULL) AND ((APPROVED <> 'A' AND APPROVED <> 'Y') OR APPROVED IS NULL)"; 
            qryLabExam.Open();
            if (!qryLabExam.IsEmpty())
            {
                string comments = qryLabExam.FieldByName("COMMENTS");
                qryLabExam.Edit();


                for (int fieldIndex = 0; fieldIndex < fieldNames.Length; fieldIndex++)
                {
                    string fieldName = fieldNames[fieldIndex].ToUpper();
                    string fieldLabel = dbpManageSR.GetFieldPrompt(fieldName);
                    string tableName = dbpManageSR.GetFieldTableName(fieldName);
                    string value = dbpManageSR.GetFieldValue(tableName, fieldName, fieldLabel).Replace("'", "''").Trim();

                    if (!string.IsNullOrEmpty(value))
                    {
                        if (fieldName == "COMMENTS")
                            value = (!string.IsNullOrEmpty(comments) ? comments + " " + value : value);

                        if (qryLabExam.FieldExist(fieldName))
                            qryLabExam.SetFieldValue(fieldName, value);
                    }
                }

                qryLabExam.Post("TV_LABASSIGN", -1, -1);
            }

        }
        protected void btnCancelManage_Click(object sender, EventArgs e)
        {
            dbpManageSR.ClearErrors();
            dbpManageSR.ClearFields();
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "_closeLabCaseDialog", "closeManageSRDialog('mdialog-manage');", true);
        }

        #region Case Link
        #region Events
        protected void btnLinkedReports_Click(object sender, EventArgs e)
        {
            dbgCaseLink.InitializePLCDBGrid();
            
            if (dbgCaseLink.Rows.Count > 0)
                dbgCaseLink.SelectedIndex = 0;

            ShowCaseLinkPopup();
        }

        protected void btnCaseLinkPrint_Click(object sender, EventArgs e)
        {
            string caseKey = dbgCaseLink.SelectedDataKey["CASE_KEY"].ToString();
            string examKey = dbgCaseLink.SelectedDataKey["EXAM_KEY"].ToString();
            PrintLinkedReport(examKey, caseKey);
        }

        #endregion Events

        #region Methods
        private void InitCaseLinkButton()
        {
            if (PLCSession.GetLabCtrlFlag("USE_ASSIGNMENT_CASE_LINK") == "T")
            {
                btnLinkedReports.Visible = true;
                if (PLCSession.GetLabCtrl("ASSIGNMENT_CASE_LINK_TEXT").Trim() != string.Empty)
                    btnLinkedReports.Text = PLCSession.GetSysPrompt("LABCTRL.ASSIGNMENT_CASE_LINK_TEXT", PLCSession.GetLabCtrl("ASSIGNMENT_CASE_LINK_TEXT"));

                dbgCaseLink.InitializePLCDBGrid();

                if (dbgCaseLink.Rows.Count == 0)
                {
                    btnLinkedReports.Enabled = false;
                    btnLinkedReports.ForeColor = Color.Black;
                }
                else
                {
                    btnLinkedReports.Enabled = true;
                    btnLinkedReports.ForeColor = Color.Red;
                }
            }
        }

        private void ShowCaseLinkPopup()
        {
            ScriptManager.RegisterStartupScript(btnLinkedReports, btnLinkedReports.GetType(), "_showCaseLinkDialog" + DateTime.Now, "showCaseLinkDialog();", true);
        }

        private void PrintLinkedReport(string examKey, string caseKey)
        {
            if (CanViewReportFromPDFData(examKey, caseKey))
                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), "reportPDF" + DateTime.Now, "window.open('" + PLCSession.GetApplicationURL(Request) + "/ShowReportPDF.aspx?reportkey=" + examKey + "','_blank','resizable=yes, height=500,width=750');", true);
            else
                dlgCaseLink.ShowAlert("Case Link", "Report not found.");
        }

        private bool CanViewReportFromPDFData(string examKey, string caseKey)
        {
            PLCQuery qryPDFData = new PLCQuery();
            qryPDFData.SQL = "SELECT * FROM TV_PDFDATA WHERE FILE_SOURCE = 'REPORT' and FILE_SOURCE_KEY= ?";
            qryPDFData.AddSQLParameter("FILE_SOURCE_KEY", examKey);
            qryPDFData.OpenReadOnly();
            if (qryPDFData.HasData())
            {

                // Write audit log for printing this lab report.
                PLCQuery qryLabRept = new PLCQuery("SELECT * FROM TV_LABREPT WHERE CASE_KEY = ? AND EXAM_KEY = ?");
                qryLabRept.AddSQLParameter("CASE_KEY", caseKey);
                qryLabRept.AddSQLParameter("EXAM_KEY", examKey);
                qryLabRept.OpenReadOnly();

                string reportCaseKey = qryLabRept.FieldByName("CASE_KEY");
                string reportExamKey = qryLabRept.FieldByName("EXAM_KEY");
                string reportSection = qryLabRept.FieldByName("SECTION");
                string reportAnalyst = qryLabRept.FieldByName("ANALYST_ASSIGNED");
                string reportDateCompleted = qryLabRept.FieldByName("DATE_COMPLETED");

                if (qryLabRept.HasData())
                    PLCDBGlobal.instance.WriteLabRptPrintAuditLog(reportCaseKey, reportExamKey, reportSection, reportAnalyst, reportDateCompleted, "LabWork.aspx");

                return true;
            }

            return false;
        }

        #endregion Methods

        #endregion Case Link
    }
}
