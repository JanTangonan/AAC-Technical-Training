#region Revision History
/*
 * 04/19/2013   Manny       #1003/40998     Need to honor the "No Item Assignments" Labctrl flag - implement LABCTRL flag.
 * 04/16/2013   Manny       #1030/38307     Add tasks command barcode in the web - added SaveAssignTasksGrid(), ClearAssignTasksGrid(), and InputAssignTaskType();
*/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.OracleClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml.Linq;
using PLCCONTROLS;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using PLCGlobals;
using PLCWebCommon;
//using wsDOCGEN;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;


namespace BEASTiLIMS
{
    public partial class TAB6Assignments : PLCGlobals.PageBase, ITab6Assignment
    {
        #region Properties and Declarations

        PLCDBGlobal PLCDBGlobalClass = new PLCDBGlobal();
        PLCCommon PLCCommonClass = new PLCCommon();
        PLCMsgBox messageBox = new PLCMsgBox(); //*AAC* 06262009
        PLCMsgComment mInput = new PLCMsgComment();
        UC_ApproveReport LoadApproveReport = new UC_ApproveReport();
        Assignment ObjAssignment = new Assignment();
        CodeHead chCompletionCode;
        CodeHead chUserID;
        CodeHead chRoutingCodes;

        [Serializable()]
        private struct Task
        {
            public string CaseKey, ECN, ItemType, TaskType, Description, Priority, Status, Analyst;
        }

        [Serializable]
        private struct CommentChange
        {
            public string examKey, newComment, analyst;
            public DateTime changeDate;

            public CommentChange(string _examKey, string _newComment, string _analyst, DateTime _changeDate)
            {
                examKey = _examKey;
                newComment = _newComment;
                analyst = _analyst;
                changeDate = _changeDate;
            }
        }

        public Boolean BigViewEdit
        {
            get
            {
                if (ViewState["bve"] == null)
                    return false;
                return Convert.ToBoolean(ViewState["bve"]);
            }
            set
            {
                ViewState["bve"] = value;
            }
        }

        public class ItemRec
        {
            public int GridID;
            public string ItemNumber;
            public string DeptItemNumber;
            public string PackageType;
            public string ItemType;
            public int Quantity;
            public string ServiceRequest;
            public string Sections;
            public string ItemDesc;
            public int ECN;
            public string SELECTED;

            public ItemRec(int GridID, string ItemNumber, string DeptItemNumber, string PackageType, string ItemType, int Quantity, string ServiceRequest, string ItemDesc, int ECN, string ThirdPartyBarcode, string SeizedForBiologicalPurposes, string Selected)
            {
                this.GridID = GridID;
                this.ItemNumber = ItemNumber;
                this.DeptItemNumber = DeptItemNumber;
                this.PackageType = PackageType;
                this.ItemType = ItemType;
                this.Quantity = Quantity;
                this.ServiceRequest = ServiceRequest;
                this.ItemDesc = ItemDesc;
                this.ECN = ECN;
                this.SELECTED = Selected;
            }
        }
        public List<ItemRec> ItemList;

        private bool IsItemARDeleteConfirmed
        {
            get
            {
                if (ViewState["IsItemARDeleteConfirmed"] == null)
                    return false;
                return Convert.ToBoolean(ViewState["IsItemARDeleteConfirmed"]);
            }
            set
            {
                ViewState["IsItemARDeleteConfirmed"] = value;
            }
        }

        private bool IsItemCancelTaskConfirmed
        {
            get
            {
                if (ViewState["IsItemCancelTaskConfirmed"] == null)
                    return false;
                return Convert.ToBoolean(ViewState["IsItemCancelTaskConfirmed"]);
            }
            set
            {
                ViewState["IsItemCancelTaskConfirmed"] = value;
            }
        }

        private bool SaveFromTaskPopup
        {
            get
            {
                if (ViewState["SaveFromTaskPopup"] == null)
                    return false;
                return Convert.ToBoolean(ViewState["SaveFromTaskPopup"]);
            }
            set
            {
                ViewState["SaveFromTaskPopup"] = value;
            }
        }

        private bool IsAssignAddMode
        {
            get
            {
                if (ViewState["IsAssignAddMode"] == null)
                    return false;
                return Convert.ToBoolean(ViewState["IsAssignAddMode"]);
            }
            set
            {
                ViewState["IsAssignAddMode"] = value;
            }
        }

        private Boolean NO_ITEM_ASSIGNMENT
        {
            get
            {
                if (ViewState["NO_ITEM_ASSIGNMENT"] != null)
                    return (Boolean)ViewState["NO_ITEM_ASSIGNMENT"];
                return false;
            }
            set
            {
                ViewState["NO_ITEM_ASSIGNMENT"] = value;
            }
        }

        public string LinkedCases
        {
            get
            {
                if (ViewState["LinkedCases"] == null)
                {
                    ViewState.Add("LinkedCases", string.Empty);
                    return string.Empty;
                }
                else
                {
                    return ViewState["LinkedCases"].ToString();
                }
            }
            set
            {
                ViewState["LinkedCases"] = value;
            }
        }

        private bool VerifySectionNumber
        {
            get
            {
                if (ViewState["VerifySectionNumber"] == null)
                {
                    ViewState["VerifySectionNumber"] = false;
                    return false;
                }
                else
                    return Convert.ToBoolean(ViewState["VerifySectionNumber"]);
            }
            set
            {
                ViewState["VerifySectionNumber"] = value;
            }
        }

        public List<string> AttachmentEntryIDs
        {
            get
            {
                if (ViewState["AttachmentEntryIDs"] == null)
                {
                    ViewState["AttachmentEntryIDs"] = new List<string>();
                    return (List<string>)ViewState["AttachmentEntryIDs"];
                }
                else
                {
                    return (List<string>)ViewState["AttachmentEntryIDs"];
                }
            }
            set
            {
                ViewState["AttachmentEntryIDs"] = value;
            }
        }

        private string AssignmentNextPage
        {
            get
            {
                if (ViewState["AssignmentNextPage"] == null)
                    return string.Empty;

                return (string)ViewState["AssignmentNextPage"];
            }
            set
            {
                ViewState["AssignmentNextPage"] = value;
            }
        }

        private int NewAssignSequence
        {
            get {

                if (ViewState["NewAssignSequence"] == null)
                    return 0;

                return (int)ViewState["NewAssignSequence"];
            }
            set {

                ViewState["NewAssignSequence"] = value;
            }
        }

        private List<string> assignedItems
        {
            get
            {
                if (ViewState["AssignedItems"] == null)
                {
                    ViewState["AssignedItems"] = new List<string>();
                    return (List<string>)ViewState["AssignedItems"];
                }
                else
                {
                    return (List<string>)ViewState["AssignedItems"];
                }
            }
            set
            {
                ViewState["AssignedItems"] = value;
            }
        }

        private bool IsAssignTaskCancelled = false;
        private int numLabExam = -1;
        private const int VIEW_ITEMS = 0;
        private const int VIEW_NAMES = 1;
        private const string IMAGES_DIR = "C:\\Labsave\\Images\\"; // TODO: (if used) replace labsave path with AppSetting LABSAVEDIR (See PLCPath.cs)

        #region Dialog
        enum RoutingDialogKey
        {
            None = 0,
            ResetAssignment,
        }

        private RoutingDialogKey LastRoutingDialog
        {
            get
            {
                if (ViewState["LastRoutingDialog"] != null)
                    return (RoutingDialogKey)Enum.Parse(typeof(RoutingDialogKey), ViewState["LastRoutingDialog"].ToString());
                return RoutingDialogKey.None;
            }
            set
            {
                ViewState["LastRoutingDialog"] = value;
            }
        }
        #endregion Dialog

        #endregion

        #region Events

        protected void Page_Init()
        {
            if (PLCDBGlobal.instance.GetCaseHitMode().Equals("C"))
            {
                PLCDBPanel1.PLCPanelName = "HIT_ASSIGNMENTSTAB";
                PLCDBPanel1.ReDrawControls();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            ScriptManager.RegisterStartupScript(this, this.GetType(), "SetGridHandlers", "SetGridHandlers();", true);
            ScriptManager.RegisterStartupScript(this, this.GetType(), "init", "Init();", true);
            ScriptManager.GetCurrent(this.Page).Services.Add(new ServiceReference("~/MatrixControl/WSMatrix.asmx"));

            string initdbpanelScript = "(function() {" + this.PLCDBPanel1.GetAddToListScript() + "})();";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "initdbpanel", initdbpanelScript, true);

            PLCCommon.instance.SetTechReviewGroupParam("");

            Control CtrlmInput = LoadControl("~/PLCWebCommon/PLCMsgComment.ascx");
            mInput = (PLCMsgComment)CtrlmInput;
            plhMsgBoxComments.Controls.Add(mInput);

            MasterPage mp = (MasterPage)Master;
            mp.RegisterPostbackTrigger(btnDataSaveOK);
            mp.RegisterPostbackTrigger(bnDataOpen);

            Control cApproveReport = LoadControl("~/UC_ApproveReport.ascx");
            LoadApproveReport = (UC_ApproveReport)cApproveReport;
            phPLCWebOCX.Controls.Add(LoadApproveReport);

            PLCCommon_PLCCopyRights UC_Status;
            Label ErrorMessage;

            messageBox = (PLCMsgBox)LoadControl("~/PLCWebCommon/PLCMsgBox.ascx");
            plhMessageBox.Controls.Add(messageBox);
            messageBox.OnOkScript = string.Empty;

            if ((PLCSession.GetLabCtrl("USES_QMS") == "T") && (PLCSession.GetLabCtrl("HIDE_SOP_BUTTON") != "T"))
                bnSOP.Visible = true;
            else
                bnSOP.Visible = false;

            // Notes button
            if (PLCCommonClass.IsReadOnlyAccess("WEBINQ,RONLYASTAB") || PLCSession.GetLabCtrl("HIDE_NOTES_BUTTON") == "T")
                bnNotes.Visible = false;

            // Set the default notes button text
            if (PLCSession.GetSysPrompt("LABCTRL.NOTES_BUTTON_TEXT", PLCSession.GetLabCtrl("NOTES_BUTTON_TEXT")) != string.Empty)
            {
                bnNotes.Text = PLCSession.GetSysPrompt("LABCTRL.NOTES_BUTTON_TEXT", PLCSession.GetLabCtrl("NOTES_BUTTON_TEXT"));
            }
            else
            {
                bnNotes.Text = PLCSession.GetSysPrompt("TAB6Assignments.bnNotes", "Exam Notes");
            }


            SetCloseCodeControls();

            if (PLCSession.PLCGlobalCaseKey == "")
            {
                Response.Redirect("~/DashBoard.aspx");
                return;
            }
            PLCCommonClass.SetSelectedMenuItem(MainMenuTab.AssignmentsTab, (Menu)Master.FindControl("menu_main"));

            // Set page title.
            ((MasterPage)Master).SetCaseTitle(""); //AACI 07/27/2010

            UC_Status = (PLCCommon_PLCCopyRights)Master.FindControl("UC_PLCCopyRights_Master2");
            ErrorMessage = (Label)UC_Status.FindControl("lbError");

            // Don't show Service Request button for SDS or If HIDESRBTN user option is ON
            if ((PLCSession.GetLabCtrl("SD_QUICK_CREATE") == "T") || PLCSession.CheckUserOption("HIDESRBTN") || PLCSession.GetLabCtrl("USES_SERVICE_REQUESTS") == "" || PLCSession.GetLabCtrl("USES_SERVICE_REQUESTS") == "F")
                this.bnSRPrint.Attributes["style"] = "display: none;";

            PLCSession.PLCGlobalECN = string.Empty;

            ScriptManager.RegisterStartupScript(this, this.GetType(), "_clearNPIntervals", "ClearNPIntervals();", true);

            if (!IsPostBack)
            {
                ((MasterPage)this.Master).SetCanRecordLock(string.Empty);
                PLCDBGlobal.instance.RemoveRecordLocks("TV_LABEXAM", PLCSession.PLCGlobalCaseKey, null, null, PLCSession.PLCGlobalAnalyst);

                PLCSession.AddToRecentCases(PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalAnalyst);
                PLCSession.SetProperty<bool>("CanViewInProcessReports", false);

                ViewState["CommentChange"] = null;
                ViewState["AssignNameChanges"] = null;
                ViewState["AssignItemChanges"] = null;

                NewAssignSequence = 0;

                hdnUOTaskAssign.Value = PLCSession.CheckUserOption("TASKASSIGN") ? "T" : "F";

                string qmsButtonCaption = PLCSession.GetLabCtrl("QMS_BUTTON_TEXT").Trim();
                if (qmsButtonCaption != "")
                    this.bnQMS.Text = qmsButtonCaption;

                // If QC Flag set for this user, disable certain action buttons and display alert message.
                this.hdnQCFlag.Value = PLCDBGlobal.instance.GetAnalystQCFlag(PLCSession.PLCGlobalAnalyst);
                if (this.hdnQCFlag.Value != "")
                    this.messageBox.ShowMsg("Alert", "<p>The QA Manager has placed a flag on your account. Please contact the Quality Assurance Manager.</p> Message: " + this.hdnQCFlag.Value, 0);

                ViewState["AssignTasks"] = null;

                bnRollback.Visible = PLCSession.CheckUserOption("ROLLASSIGN");
                bnRouting.Visible = PLCSession.GetLabCtrl("USES_ASSIGNMENT_ROUTING") == "T" && !PLCSession.CheckUserOption("HIDEROUTE");
                if (bnRouting.Visible)
                    InitializeRouteMemoEvent();
                bnHistory.Visible = PLCSession.GetLabCtrlFlag("USES_STATUS_HISTORY") == "T";

                PLCButtonPanel1.SetCustomButtonVisible("Manage", false);
                PLCButtonPanel1.SetCustomButtonVisible("Split", false);
                if (PLCSession.GetLabCtrl("USE_SPLIT_MERGE_ASSIGNMENT") != "F" && PLCSession.GetLabCtrl("USE_SPLIT_MERGE_ASSIGNMENT") != string.Empty)
                {
                    PLCButtonPanel1.SetCustomButtonVisible("Manage", PLCSession.CheckUserOption("MNGASSIGN"));

                    if (PLCSession.GetLabCtrl("USE_SPLIT_MERGE_ASSIGNMENT") == "L")
                    {
                        PLCButtonPanel1.SetCustomButtonVisible("Split", PLCSession.CheckUserOption("MNGASSIGN"));
                        PLCButtonPanel1.SetCustomButtonText("Manage", "Merge");
                    }
                }

                if (PLCSession.GetLabCtrl("DOC_REVISION_BTN_TEXT").Trim().Length > 0)
                    PLCButtonPanel1.SetCustomButtonText("Revisions", PLCSession.GetLabCtrl("DOC_REVISION_BTN_TEXT"));

                if (PLCSession.CheckUserOption("HIDERVN"))
                    PLCButtonPanel1.SetCustomButtonVisible("Revisions", false);

                if (PLCSession.GetLabCtrl("SHOW_NOTE_REVISIONS") != "T")
                    PLCButtonPanel1.SetCustomButtonVisible("Rev Note", false);

                if (!PLCSession.CheckUserOption("DELLOCKS"))
                    PLCButtonPanel1.SetCustomButtonVisible("RecordUnlock", false);

                bnData.Visible = PLCSession.GetLabCtrl("USES_RAW_DATA") == "T";
                CheckSubmExtraInfoHighlight();

                if (PLCSession.GetSysPrompt("LABCTRL.NOTES_BUTTON_TEXT", PLCSession.GetLabCtrl("NOTES_BUTTON_TEXT")) != "")
                    bnNotes.Text = PLCSession.GetSysPrompt("LABCTRL.NOTES_BUTTON_TEXT", PLCSession.GetLabCtrl("NOTES_BUTTON_TEXT"));
                if (PLCSession.GetSysPrompt("LABCTRL.REVIEW_BUTTON_TEXT", PLCSession.GetLabCtrl("REVIEW_BUTTON_TEXT")) != "")
                    bnReview.Text = PLCSession.GetSysPrompt("LABCTRL.REVIEW_BUTTON_TEXT", PLCSession.GetLabCtrl("REVIEW_BUTTON_TEXT"));
                if (PLCSession.GetSysPrompt("LABCTRL.APPROVE_BUTTON_TEXT", PLCSession.GetLabCtrl("APPROVE_BUTTON_TEXT")) != "")
                    bnApprove.Text = PLCSession.GetSysPrompt("LABCTRL.APPROVE_BUTTON_TEXT", PLCSession.GetLabCtrl("APPROVE_BUTTON_TEXT"));
                if (PLCSession.GetSysPrompt("LABCTRL.ADMIN_BUTTON_TEXT", PLCSession.GetLabCtrl("ADMIN_BUTTON_TEXT")) != "")
                    bnAdmClose.Text = PLCSession.GetSysPrompt("LABCTRL.ADMIN_BUTTON_TEXT", PLCSession.GetLabCtrl("ADMIN_BUTTON_TEXT"));
                if (PLCSession.GetSysPrompt("LABCTRL.REPORT_BUTTON_TEXT", PLCSession.GetLabCtrl("REPORT_BUTTON_TEXT")) != "")
                    bnReports.Text = PLCSession.GetSysPrompt("LABCTRL.REPORT_BUTTON_TEXT", PLCSession.GetLabCtrl("REPORT_BUTTON_TEXT"));
                if (PLCSession.GetSysPrompt("LABCTRL.SIGN_BUTTON_TEXT", PLCSession.GetLabCtrl("SIGN_BUTTON_TEXT")) != "")
                    bnSign.Text = PLCSession.GetSysPrompt("LABCTRL.SIGN_BUTTON_TEXT", PLCSession.GetLabCtrl("SIGN_BUTTON_TEXT"));

                if (!string.IsNullOrEmpty(PLCSession.GetSysPrompt("LABCTRL.SERVICE_REQUEST_TEXT", PLCSession.GetLabCtrl("SERVICE_REQUEST_TEXT")).Trim()))
                    bnSRPrint.Text = PLCSession.GetSysPrompt("LABCTRL.SERVICE_REQUEST_TEXT", PLCSession.GetLabCtrl("SERVICE_REQUEST_TEXT")).Trim();

                bnAssignRoute.Visible = PLCSession.CheckUserOption("ASSIGNROUT") && PLCSession.GetLabCtrl("ASSIGNMENT_ROUTING_CODE") != "";
                if (PLCSession.GetSysPrompt("LABCTRL.ASSIGNMENT_ROUTING_TEXT", PLCSession.GetLabCtrl("ASSIGNMENT_ROUTING_TEXT")) != "")
                    bnAssignRoute.Text = PLCSession.GetSysPrompt("LABCTRL.ASSIGNMENT_ROUTING_TEXT", PLCSession.GetLabCtrl("ASSIGNMENT_ROUTING_TEXT"));

                btnWorklist.Enabled = PLCSession.CheckUserOption("IBATCHRES");

                GridView1.PLCGridName = "CASE_ASSIGNMENTS";
                string sql = "Select EXAM_KEY, CASE_KEY, SEQUENCE, SECTION, ANALYST_ASSIGNED, DATE_ASSIGNED, STATUS, DRAFT_DATE " +
                             "FROM TV_LABEXAM Where CASE_KEY = " + PLCSession.PLCGlobalCaseKey +
                             " Order by EXAM_KEY";

                GridView1.PLCSQLString = PLCSession.FormatSpecialFunctions(sql);
                GridView1.PLCErrorMessageLabel = ErrorMessage;
                GridView1.InitializePLCDBGrid();

                if ((string)Session["ApproveOCXcalled"] == "T" || (string)Session["AdminApproved"] == "T" || (string)Session["OCXDNAProcessApprove"] == "T")
                {
                    Session["ApproveOCXcalled"] = "";
                    Session["AdminApproved"] = "";
                    Session["OCXDNAProcessApprove"] = "";

                    if (PLCSession.PLCGlobalAssignmentKey != "")
                    {
                        PLCQuery qryLabExam = QuerySelectedLabAssign();
                        if (qryLabExam.HasData() && qryLabExam.FieldByName("COMPLETED") == "T")
                        {
                            PLCDBGlobalClass.SetAssignmentTurnAround(PLCSession.PLCGlobalAssignmentKey);
                            PLCDBGlobal.instance.CompleteServiceRequest(Convert.ToInt32(PLCSession.PLCGlobalAssignmentKey));

                            PLCSession.WriteAuditLog("81", "1", "", "User approved a report");
                            messageBox.ShowMsg("Approve", "Approve Complete", 0);
                        }
                        else
                            messageBox.ShowMsg("Approve", "Approve Failed", 0);
                    }
                }

                // empty tab
                if (GridView1.Rows.Count == 0)
                {
                    ResetControls();
                }
                else
                {
                    SetAttachmentsButtonMode(true);
                }
                MultiView1.ActiveViewIndex = 0;

                DataSet DS;
                if (PLCSession.PLCGeneralDataSet == null)
                {
                    DS = new DataSet();
                    DataTable DTT = new DataTable("ASSIGNMENTITEMS");
                    AddItemColsToDataSet(DTT);
                    DS.Tables.Add(DTT);
                    PLCSession.PLCGeneralDataSet = DS;
                }
                else
                {
                    DS = PLCSession.PLCGeneralDataSet;
                    if (DS.Tables.IndexOf("ASSIGNMENTITEMS") < 0)
                    {
                        DataTable DTT = new DataTable("ASSIGNMENTITEMS");
                        AddItemColsToDataSet(DTT);
                        DS.Tables.Add(DTT);
                        PLCSession.PLCGeneralDataSet = DS;
                    }
                }

                gItems.DataSource = PLCSession.PLCGeneralDataSet.Tables["ASSIGNMENTITEMS"];
                gItems.DataBind();

                BigViewEdit = false;
                SelectTab(VIEW_ITEMS);

                ConfigureResultsGrid();
                string IsReportOpened = (string)Session["ReportOpened"];
                if (IsReportOpened == "T")
                {
                    // PopulateBigViewAfterPrintClick();
                }

                pnlItems.Attributes["onscroll"] = String.Format("OnContainerScroll(this,'{0}');", hdnItemScrollPos.ClientID);

                if ((string)Session["OCXDNAProcess"] == "T" || (string)Session["PeerReviewed"] == "T")
                {
                    Session["OCXDNAProcess"] = "";
                    Session["PeerReviewed"] = "";
                    messageBox.ShowMsg("Review", "Review Complete", 0);
                }

                FindAssignment();

                if (GridView1.SelectedIndex < 0 && GridView1.Rows.Count > 0)
                {
                    GridView1.SelectedIndex = 0;
                    GrabGridRecord();
                }

                if ((string)Session["SignOCXcalled"] == "T")
                {
                    Session["SignOCXcalled"] = "";
                    if (PLCSession.PLCGlobalAssignmentKey != "" && !PLCDBGlobal.instance.AssignmentSignedAlready(PLCSession.PLCGlobalAssignmentKey, "SIGNED"))
                    {
                        if (PLCDBGlobal.instance.IsNotesPacketAccepted(PLCSession.PLCGlobalAssignmentKey) || PLCDBGlobal.instance.IsAssignmentNoDocument(PLCSession.PLCGlobalAssignmentKey))
                        {
                            bnSign.Enabled = true;
                            ProcessReview("SIGNED");
                        }
                    }
                }

                if ((string)Session["SignOCXcalled"] == "standby")
                    Session["SignOCXcalled"] = "";

                if (PLCSession.GetLabCtrl("DISPLAY_CV_IN_CASE") != "T")
                    this.bnQMS.Attributes["style"] = "display:none;";

                SetQCFlagButtonState();
                IsAssignAddMode = false;

                hdnReportSigned.Value = PLCDBGlobal.instance.AssignmentSignedAlready(PLCSession.PLCGlobalAssignmentKey, "SIGNED") ? "T" : "F";
                hdnReportReconciled.Value = PLCDBGlobal.instance.AssignmentReconciledAlready(PLCSession.PLCGlobalAssignmentKey) ? "T" : "F";
            }
            else
            {
                GridView1.SetPLCGridProperties();
                if (!string.IsNullOrEmpty(hdnItemScrollPos.Value))
                {
                    ScriptManager.RegisterStartupScript(this, typeof(Page), "restoreItemsGridScroll",
                        string.Format("SetPostbackScrollPos('{0}', {1});", pnlItems.ClientID, hdnItemScrollPos.Value), true);
                }
            }

            SetPreviewReportEvent();

            if (bnSign.Text == "Waiting For NP")
                ScriptManager.RegisterStartupScript(this, this.GetType(), "_waitForNotesPacketCompletion", "WaitForNotesPacketCompletion('" + bnSign.ClientID + "', '" + PLCSession.PLCGlobalAssignmentKey + "');", true);
        }

        protected void Page_LoadComplete(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                MasterPage master = (MasterPage)this.Master;
                if (PLCSession.CheckUserOption("IVVIEWE") && PLCSession.GetLabCtrlFlag("USES_IMAGE_VAULT_WINDOW").Equals("T"))
                {
                    master.SetAttachModelessPopup();
                    if (GridView1.Rows.Count <= 0)
                        EnablePaperClipModeless(false);
                }
            }
        }

        protected void MultiView1_ActiveViewChanged(object sender, EventArgs e)
        {
            if (MultiView1.ActiveViewIndex == 1)
            {
                LoadItems();
                LoadNames(GridView1.SelectedDataKey["EXAM_KEY"].ToString());
                PLCDBCheckList1.SetBrowseMode();
                PLCButtonPanel2.SetBrowseMode();
            }

            SetReadOnlyAccess();
        }

        protected void chCompletionCode_ValueChanged(object sender, EventArgs e)
        {
            spRequired.Visible = false;
            if (chCompletionCode.GetValue() != "")
            {
                PLCQuery qryCompletion = new PLCQuery("SELECT COMMENT_REQUIRED FROM TV_CLOSECODE WHERE COMPLETION_CODE = '" + chCompletionCode.GetValue() + "'");
                if (qryCompletion.Open() && qryCompletion.HasData())
                    spRequired.Visible = qryCompletion.FieldByName("COMMENT_REQUIRED") == "T";
            }
            mpeAdminClose.Show();
        }

        protected void bnDetails_Click(object sender, EventArgs e)
        {
            MultiView1.ActiveViewIndex = 0;
        }

        protected void bnItems_Click(object sender, EventArgs e)
        {
            if (GridView1.Rows.Count == 0)
                return;

            MultiView1.ActiveViewIndex = 1;
        }

        protected void bnTasks_Click(object sender, EventArgs e)
        {
            if (GridView1.Rows.Count == 0)
                return;

            if (NO_ITEM_ASSIGNMENT)
                messageBox.ShowMsg("Edit Tasks", "No items associated to this assignment.", 1);
            else
                Response.Redirect("EditTasks.aspx");
        }

        protected void bnNotes_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(PLCSession.PLCGlobalAssignmentKey))
            {
                messageBox.ShowMsg("Assignment", "Please select an assignment.", 1);
                return;
            }

            Response.Redirect("~/AssignmentNotes.aspx?p=1");
        }

        protected void bnReports_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(PLCSession.PLCGlobalAssignmentKey))
            {
                messageBox.ShowMsg("Assignment", "Please select an assignment.", 1);
                return;
            }

            // Make sure the labexam exists and is not completed before taking action on it.
            PLCQuery qryLabExam = QuerySelectedLabExam();
            if (qryLabExam.HasData())
            {
                bool isAnalystRequired = false;
                bool showMatrixItemsTab = false;
                bool showTaskListPage = false;
                bool assignmentSignedAlready = false;
                bool canViewInProcessReports = false;
                bool showRevisionsTab = false;
                bool hideMatrixReportsTab = false;
                bool hideMatrixGeneralWorksheets = false;
                bool enableReportsOnSign = false;
                bool hasNoAnalystAssigned = string.IsNullOrEmpty(PLCDBPanel1.getpanelfield("ANALYST_ASSIGNED"));
                bool isAnalystAssigned = PLCDBPanel1.getpanelfield("ANALYST_ASSIGNED") == PLCSession.PLCGlobalAnalyst;
                bool canWriteReports = PLCSession.GetUserAnalSect(PLCDBPanel1.getpanelfield("SECTION"), "WRITE_REPORTS").ToString() == "T";
                bool canCreateWorksheets = PLCSession.GetUserAnalSect(PLCDBPanel1.getpanelfield("SECTION").Trim(), "CREATE_WORKSHEETS").ToString() == "T";
                bool canCreateSupervisorWorksheet = PLCSession.GetUserAnalSect(PLCDBPanel1.getpanelfield("SECTION").Trim(), "CREATE_SUPERVISOR_WORKSHT").ToString() == "T";
                string matrixDefaultTab = string.Empty;

                string examCode = qryLabExam.FieldByName("SECTION");
                PLCQuery qryExamCode = new PLCQuery("SELECT * FROM TV_EXAMCODE where EXAM_CODE = '" + examCode + "'");
                if (qryExamCode.Open() && qryExamCode.HasData())
                {
                    isAnalystRequired = qryExamCode.FieldByName("ANALYST_REQUIRED") != "F";
                    showMatrixItemsTab = qryExamCode.FieldByName("SHOW_MATRIX_ITEMS_TAB") == "T";
                    showTaskListPage = qryExamCode.FieldByName("USES_TOX_RESULTS") == "T";
                    showRevisionsTab = qryExamCode.FieldByName("SHOW_REVISIONS") == "T";
                    hideMatrixReportsTab = qryExamCode.FieldByName("HIDE_MATRIX_REPORT_WRITING") == "T";
                    hideMatrixGeneralWorksheets = qryExamCode.FieldByName("HIDE_MATRIX_GENERAL_WORKSHEETS") == "T";
                    matrixDefaultTab = qryExamCode.FieldByName("MATRIX_DEFAULT_TAB").Trim().ToUpper();
                }
                else
                {
                    messageBox.ShowMsg("Assignment", "The section code is not valid.", 0);
                    return;
                }

                if (!canCreateSupervisorWorksheet && (isAnalystRequired && string.IsNullOrEmpty(PLCDBPanel1.getpanelfield("ANALYST_ASSIGNED")) ||
                    (!canWriteReports && !canCreateWorksheets)))
                {
                    messageBox.ShowMsg("Assignment", "An analyst is required for this assignment", 1);
                    return;
                }

                PLCSession.PLCGlobalMatrixEditable = PLCCommonClass.IsReadOnlyAccess("WEBINQ,RONLYASTAB") ? "F" : "T";
                if (isAnalystRequired || !string.IsNullOrEmpty(PLCDBPanel1.getpanelfield("ANALYST_ASSIGNED")))
                {
                    string sBatchKey = "";
                    if (PLCDBGlobal.instance.IsAssignmentInBatch(PLCSession.PLCGlobalAssignmentKey, ref sBatchKey)
                        && PLCDBGlobal.instance.BatchSignedAlready(sBatchKey, "SIGNED")) // batch assignment
                    {
                        PLCSession.PLCGlobalMatrixEditable = "F";
                    }
                    else // single assignment
                    {
                        assignmentSignedAlready = PLCDBGlobal.instance.AssignmentSignedAlready(PLCSession.PLCGlobalAssignmentKey, "SIGNED");
                        canViewInProcessReports = PLCSession.GetUserAnalSect(PLCDBPanel1.getpanelfield("SECTION").Trim(), "VIEW_IN_PROCESS_REPORTS").ToString() == "T";
                        enableReportsOnSign = PLCSession.GetLabCtrl("ENABLE_REPORTS_ON_SIGN") == "T";

                        if (assignmentSignedAlready)
                        {
                            if (!HasAssignmentReportsAccess())
                                PLCSession.PLCGlobalMatrixEditable = "F";
                            else if (!enableReportsOnSign)
                                PLCSession.PLCGlobalMatrixEditable = "F";

                            // check if reports button in matrix should be visible
                            if (canViewInProcessReports && !enableReportsOnSign)
                                PLCSession.SetProperty<bool>("CanViewInProcessReports", true);
                        }
                        else
                        {
                            if (!HasAssignmentReportsAccess() && canViewInProcessReports)
                                PLCSession.PLCGlobalMatrixEditable = "F";

                            // check if reports button in matrix should be visible
                            if (canViewInProcessReports)
                                PLCSession.SetProperty<bool>("CanViewInProcessReports", true);
                        }
                    }
                }

                PLCQuery qryItemsAssigned = new PLCQuery();
                qryItemsAssigned.SQL = "SELECT * FROM TV_ITASSIGN WHERE EXAM_KEY = " + PLCSession.PLCGlobalAssignmentKey;
                qryItemsAssigned.Open();
                if (qryItemsAssigned.IsEmpty())
                {
                    if (PLCSession.GetLabCtrl("NO_ITEM_ASSIGNMENTS") == "T") //if assignments with no items are allowed
                    {
                        if (PLCDBGlobal.instance.IsAssignmentHasItemBasedPanels(Convert.ToInt32(PLCSession.PLCGlobalAssignmentKey)))
                        {
                            messageBox.ShowMsg("Assignment", "This Assignment contains Item Based Panels. It is required that you select at least 1 Item in order for Matrix Panels to render properly.", 1);
                            return;
                        }
                    }
                    else
                    {
                        messageBox.ShowMsg("Assignment", "Please select items for this assignment.", 1);
                        return;
                    }
                }

                //if (isAnalystRequired && PLCSession.SectionRequiresAnalyst(examCode))
                //{
                //    messageBox.ShowMsg("Assignment", "The Analyst is required.", 0);
                //    return;
                //}

                //Ticket#23656
                if (ItemSourceWithoutValue())
                {
                    messageBox.ShowMsg("Assignment", "Please select item source for the selected items.", 0);
                    return;
                }

                if (!string.IsNullOrEmpty(matrixDefaultTab))
                {
                    AssignmentNextPage = RedirectToMatrixDefaultTab(matrixDefaultTab, showMatrixItemsTab, showTaskListPage, showRevisionsTab, hideMatrixReportsTab, hideMatrixGeneralWorksheets);
                }

                if (string.IsNullOrEmpty(AssignmentNextPage))
                {
                    //allow user to create worksheets and redirect to AssignmentWorksheets tab 
                    //if ANALSECT.CREATE_SUPERVISOR_WORKSHT == 'T' AND
                    //if there is no analyst assigned to the assignment OR(all other matrix tabs are disabled)
                    //if user has no WRITE_REPORTS section permission OR (all other matrix tabs are disabled)
                    //if user has no CREATE_WORKSHEETS section permission (if analyst assigned = global analyst, redirect to default tab)
                    if (canCreateSupervisorWorksheet && (hasNoAnalystAssigned || !canWriteReports || (!isAnalystAssigned && !canCreateWorksheets)))
                    {
                        if (!hideMatrixGeneralWorksheets)
                        {
                            SetNotesSequence();
                            AssignmentNextPage = "~/AssignmentWorksheet.aspx";
                        }
                        else
                        {
                            AssignmentNextPage = null;
                            dlgMessage.ShowAlert("Assignment", "Assignment Worksheet page is hidden. Please contact LIMS Admin.");
                            return;
                        }

                    }
                    else if (showTaskListPage)
                    {
                        SetNotesSequence();
                        AssignmentNextPage = "~/AssignmentTaskList.aspx";
                    }
                    else
                    {
                        if (showMatrixItemsTab && PLCSession.PLCGlobalMatrixEditable != "F") // If Analyst has no access, redirect to Assignment Report
                        {
                            SetNotesSequence();

                            if (PLCSession.GetLabCtrl("USES_MATRIX_ITEM_PREVIEW") == "T")
                                AssignmentNextPage = "~/AssignmentItemM.aspx";
                            else
                                AssignmentNextPage = "~/AssignmentItem.aspx";

                        }
                        else if (!hideMatrixReportsTab)
                        {
                            SetNotesSequence();
                            AssignmentNextPage = "~/AssignmentReport.aspx";
                        }
                        else if (!hideMatrixGeneralWorksheets)
                        {
                            SetNotesSequence();
                            AssignmentNextPage = "~/AssignmentWorksheet.aspx";
                        }
                        else if (showRevisionsTab)
                        {
                            SetNotesSequence();
                            AssignmentNextPage = "~/AssignmentReport_Revisions.aspx";
                        }
                        else
                        {
                            AssignmentNextPage = null;
                            dlgMessage.ShowAlert("Assignment", "All Matrix Tabs are hidden. Please contact LIMS Admin.");
                            return;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(AssignmentNextPage)
                    && !ValidateItemInAnalystCustody())
                    return;

                Response.Redirect(AssignmentNextPage);
            }
            else
            {
                ShowAssignmentChangedError();
            }
        }

        protected void bnVerifyReports_Click(object sender, EventArgs e)
        {
            bool hasNoAnalystAssigned = string.IsNullOrEmpty(PLCDBPanel1.getpanelfield("ANALYST_ASSIGNED"));
            bool isAnalystAssigned = PLCDBPanel1.getpanelfield("ANALYST_ASSIGNED") .ToUpper() == PLCSession.PLCGlobalAnalyst;
            bool canWriteReports = PLCSession.GetUserAnalSect(PLCDBPanel1.getpanelfield("SECTION"), "WRITE_REPORTS").ToString() == "T";
            bool canReviewReports = PLCSession.GetUserAnalSect(PLCDBPanel1.getpanelfield("SECTION"), "REVIEW_REPORTS").ToString() == "T";
            bool canCreateWorksheets = PLCSession.GetUserAnalSect(PLCDBPanel1.getpanelfield("SECTION").Trim(), "CREATE_WORKSHEETS").ToString() == "T";
            bool canCreateSupervisorWorksheet = PLCSession.GetUserAnalSect(PLCDBPanel1.getpanelfield("SECTION").Trim(), "CREATE_SUPERVISOR_WORKSHT").ToString() == "T";

            if (string.IsNullOrEmpty(PLCSession.PLCGlobalAssignmentKey))
            {
                messageBox.ShowMsg("Assignment", "Please select an assignment.", 1);
                return;
            }

            // Make sure the labexam exists and is not completed before taking action on it.
            PLCQuery qryLabExam = QuerySelectedLabExam();
            if (qryLabExam.HasData())
            {
                bool isAnalystRequired = false;
                bool showMatrixItemsTab = false;

                string examCode = qryLabExam.FieldByName("SECTION");
                PLCQuery qryExamCode = new PLCQuery("SELECT * FROM TV_EXAMCODE where EXAM_CODE = '" + examCode + "'");
                if (qryExamCode.Open() && qryExamCode.HasData())
                {
                    isAnalystRequired = qryExamCode.FieldByName("ANALYST_REQUIRED") != "F";
                    showMatrixItemsTab = qryExamCode.FieldByName("SHOW_MATRIX_ITEMS_TAB") == "T";
                }
                else
                {
                    messageBox.ShowMsg("Assignment", "The section code is not valid.", 0);
                    return;
                }

                if (isAnalystRequired && string.IsNullOrEmpty(PLCDBPanel1.getpanelfield("ANALYST_ASSIGNED")))
                {
                    messageBox.ShowMsg("Assignment", "An analyst is required for this assignemnt", 1);
                    return;
                }

                PLCSession.PLCGlobalMatrixEditable = PLCCommonClass.IsReadOnlyAccess("WEBINQ,RONLYASTAB") ? "F" : "T";

                //if (isAnalystRequired || !string.IsNullOrEmpty(PLCDBPanel1.getpanelfield("ANALYST_ASSIGNED")))
                //{
                //    if (HasAssignmentReportsAccess())
                //    {
                //        string sBatchKey = "";
                //        if (PLCDBGlobal.instance.IsAssignmentInBatch(PLCSession.PLCGlobalAssignmentKey, ref sBatchKey) &&
                //            PLCDBGlobal.instance.BatchSignedAlready(sBatchKey, "SIGNED"))
                //            PLCSession.PLCGlobalMatrixEditable = "F";
                //    }
                //    else
                //    {
                //        if (PLCDBGlobal.instance.AssignmentSignedAlready(PLCSession.PLCGlobalAssignmentKey, "SIGNED"))
                //            PLCSession.PLCGlobalMatrixEditable = "F";
                //        else
                //        {
                //            messageBox.ShowMsg("Assignment", "You are not authorized to access reports for this assignment.", 1);
                //            return;
                //        }
                //    }
                //}

                PLCQuery qryItemsAssigned = new PLCQuery("SELECT * FROM TV_ITASSIGN WHERE EXAM_KEY = " + PLCSession.PLCGlobalAssignmentKey);
                qryItemsAssigned.Open();
                if (qryItemsAssigned.IsEmpty())
                {
                    if (PLCSession.GetLabCtrl("NO_ITEM_ASSIGNMENTS") == "T")
                    {
                        if (PLCDBGlobal.instance.IsAssignmentHasItemBasedPanels(Convert.ToInt32(PLCSession.PLCGlobalAssignmentKey), true))
                        {
                            messageBox.ShowMsg("Assignment", "This Assignment contains Item Based Panels. It is required that you select at least 1 Item in order for Matrix Panels to render properly.", 1);
                            return;
                        }
                    }
                    else
                    {
                        messageBox.ShowMsg("Assignment", "Please select items for this assignment.", 1);
                        return;
                    }
                }

                //if (isAnalystRequired && PLCSession.SectionRequiresAnalyst(examCode))
                //{
                //    messageBox.ShowMsg("Assignment", "The Analyst is required.", 0);
                //    return;
                //}

                //Ticket#23656
                //if (ItemSourceWithoutValue())
                //{
                //    messageBox.ShowMsg("Assignment", "Please select item source for the selected items.", 0);
                //    return;
                //}

                //if (showMatrixItemsTab)
                //{
                //    Response.Redirect("~/AssignmentItem.aspx");
                //}
                //else
                //{
                //    Response.Redirect("~/AssignmentReport.aspx");
                //}
                //allow user to create worksheets and redirect to AssignmentWorksheets_Verify tab 
                //if ANALSECT.CREATE_SUPERVISOR_WORKSHT == 'T' AND
                //if there is no analyst assigned to the assignment OR(all other matrix tabs are disabled)
                //if user has no REVIEW_REPORTS section permission OR (all other matrix tabs are disabled)
                //if user (has REVIEW_REPORTS section permission and has no WRITE_REPORTS section permission) OR (all other matrix tabs are disabled)
                //if user has no CREATE_WORKSHEETS section permission
                if (canCreateSupervisorWorksheet && (hasNoAnalystAssigned || !canReviewReports || (canReviewReports && !canWriteReports) || (!isAnalystAssigned && !canCreateWorksheets)))
                {
                    SetNotesSequence();
                    Response.Redirect("~/AssignmentWorksheet_Verify.aspx");
                }
                else
                {
                    SetNotesSequence();
                    Response.Redirect("~/AssignmentReport_Verify.aspx");
                }

            }
            //else
            //{
            //    ShowAssignmentChangedError();
            //}
        }

        protected void bnRemoveBlind_Click(object sender, EventArgs e)
        {
            if (PLCDBGlobal.RemoveBlindVerification(PLCSession.PLCGlobalAssignmentKey))
                messageBox.ShowMsg("Verification", "Blind Verification has been removed.", 1);
        }

        protected void bnSign_Click(object sender, EventArgs e)
        {
            if (ValidateCanSignAssignment(sender))
                ProcessReview("SIGNED");
        }

        protected void bnReview_Click(object sender, EventArgs e)
        {
            if (ValidateCanReviewAssignment(sender))
            {
                if (PLCDBGlobal.instance.HasReportPreview(PLCDBPanel1.getpanelfield("SECTION"), "REVIEW")
                    && !PLCDBGlobalClass.IsAssignmentTechReviewed(PLCSession.PLCGlobalAssignmentKey))
                {
                    PLCApproveReportPreview approvePreview = (PLCApproveReportPreview)((UserControl)this.Page.Master.FindControl("UC_CustomerTitle_Master2")).FindControl("ReportPreview");
                    approvePreview.ExamKey = PLCSession.PLCGlobalAssignmentKey;
                    approvePreview.ReviewType = "REVIEW";
                    approvePreview.Show();
                }
                else
                {
                    ProcessReview("REVIEW");
                }
            }
        }

        protected void bnCodisReview_Click(object sender, EventArgs e)
        {
            if (ValidateCanReviewAssignment(sender))
                ProcessReview("CODISREV");
        }

        protected void bnApprove_Click(object sender, EventArgs e)
        {
            ApproveAssignment(sender);
        }

        protected void bnAdmClose_Click(object sender, EventArgs e)
        {
            // Make sure the labexam exists and is not completed before taking action on it.
            PLCQuery qyLabExam = QuerySelectedLabExam();
            if (!qyLabExam.IsEmpty())
            {
                string LockedInfo = string.Empty, LockedTypeRes = string.Empty;
                if (PLCDBGlobal.instance.IsRecordLocked("TV_LABEXAM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalAssignmentKey, "-1", out LockedInfo, out LockedTypeRes))
                {
                    dlgMessage.ShowAlert("Record Lock", string.Format("Assignment locked by another user for {0}.<br/>{1}", ObjAssignment.GetLockedTypeResMessage(LockedTypeRes), LockedInfo));
                    lPLCLockStatus.Text = LockedInfo;
                    dvPLCLockStatus.Visible = true;
                    UpdateRecLockLblBtnState(true);
                    return;
                }
                else
                {
                    UpdateRecLockLblBtnState();
                    PLCDBGlobal.instance.LockUnlockRecord("TV_LABEXAM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalAssignmentKey, "-1", true, "CLOSE");
                    ((MasterPage)this.Master).SetCanRecordLock("T");
                }

                hdnExamKey.Value = PLCSession.PLCGlobalAssignmentKey;
                lblSequence.Text = GridView1.SelectedDataKey["SEQUENCE"].ToString();
                hdnSection.Value = GridView1.SelectedDataKey["SECTION"].ToString();
                lblURN.Text = PLCSession.PLCGlobalDepartmentCaseNumber;

                if (chCompletionCode != null)
                    chCompletionCode.ClearValues();
                txtComments.Text = "";
                spRequired.Visible = false;

                mpeAdminClose.Show();
            }
            else
            {
                ShowAssignmentChangedError();
            }
        }

        protected void bnUnlock_Click(object sender, EventArgs e)
        {
            //
        }

        protected void btnRouteItems_Click(object sender, EventArgs e)
        {
            //PLCSession.PLCGlobalAssignmentKey = GridView1.DataKeys[GridView1.SelectedRow.RowIndex].Values["EXAM_KEY"].ToString().Trim();
            //PLCSession.SetProperty<string>("PreviousPage", Page.Request.Url.ToString());
            //if (Session["ACNITEMCODE"] != null)   Session["ACNITEMCODE"] = null;
            //Response.Redirect("~/RouteItems.aspx");
        }

        protected void bnSOP_Click(object sender, EventArgs e)
        {
            Response.Redirect("SOPVIEW.ASPX");

            string TheOraDate = "";
            string tempstr = "";

            tempstr = "select TV_QMSDVER.VERSION_NUMBER, TV_QMSDVER.STATUS , TV_QMSDOCS.DOCUMENT_NAME, TV_QMSDVER.START_DATE, TV_QMSDVER.END_DATE ,TV_QMSDVER.VERSION_KEY, TV_QMSDVER.DOCUMENT_KEY, TV_QMSDVER.STATUS, TV_QMSDOCS.DOCUMENT_KEY ";
            tempstr += " from TV_QMSDOCSECT, TV_QMSDOCS, TV_QMSDVER ";
            tempstr += " where  (TV_QMSDOCS.DOCUMENT_KEY = TV_QMSDOCSECT.DOCUMENT_KEY) and (TV_QMSDVER.DOCUMENT_KEY = TV_QMSDOCSECT.DOCUMENT_KEY) ";
            tempstr += " and (TV_QMSDOCSECT.SECTION = :THESECTION ) and ( :THEDATE between TV_QMSDVER.START_DATE and TV_QMSDVER.END_DATE) ";


            PLCQuery qryFindAssign = new PLCQuery();
            qryFindAssign.SQL = "SELECT DATE_ASSIGNED, SECTION FROM TV_LABASSIGN WHERE EXAM_KEY = :EK";
            qryFindAssign.SetParam("EK", PLCSession.PLCGlobalAssignmentKey.ToString());
            qryFindAssign.Open();
            if (qryFindAssign.HasData())
            {

                DateTime dt = Convert.ToDateTime(qryFindAssign.FieldByName("DATE_ASSIGNED"));
                TheOraDate = dt.ToString("MM/dd/yyyy");

                if (PLCSession.PLCDatabaseServer == "MSSQL")
                {
                    TheOraDate = dt.ToString("MM/dd/yyyy");
                    TheOraDate = "CONVERT(DATETIME,'" + TheOraDate + "',101) ";

                }
                else
                {
                    TheOraDate = dt.ToString("dd-mmm-yyyy");
                    TheOraDate = "TO_DATE('" + TheOraDate + "','dd-mmm-yyyy') ";
                }

                PLCQuery qry = new PLCQuery();
                qry.SQL = tempstr;
                qry.SetParam("THESECTION", "'" + qryFindAssign.FieldByName("SECTION") + "'");
                qry.SetParam("THEDATE", TheOraDate);
                qry.Open();
                tempstr = "";
                if (qry.HasData())
                {
                    while (!qry.EOF())
                    {
                        tempstr += "/n" + qry.FieldByName("DOCUMENT_NAME");
                        qry.Next();
                    }

                    messageBox.ShowMsg("Documents", tempstr, 1);
                }
            }
        }

        protected void bnSRPrint_Click(object sender, EventArgs e)
        {
            // Make sure the labexam exists and is not completed before taking action on it.
            PLCQuery qyLabExam = QuerySelectedLabExam();
            if (!qyLabExam.IsEmpty())
            {
                string examkey = PLCSession.PLCGlobalAssignmentKey;

                if (examkey != "")
                {
                    List<int> lstMasterKeys = null;
                    PLCQuery qry = new PLCQuery();
                    qry.SQL = "SELECT DISTINCT(SR_MASTER_KEY) from TV_SRDETAIL where TV_SRDETAIL.EXAM_KEY = :EK";
                    qry.SetParam("EK", examkey);
                    qry.Open();
                    if (qry.HasData())
                    {
                        lstMasterKeys = new List<int>();

                        while (!qry.EOF())
                        {
                            lstMasterKeys.Add(Convert.ToInt32(qry.FieldByName("SR_MASTER_KEY")));
                            qry.Next();
                        }
                    }

                    if (lstMasterKeys != null)
                    {
                        string reportName = "SREQUEST.RPT";
                        string reportPath = PLCSession.FindCrystalReport(reportName);

                        if (PLCSession.GetLabCtrlFlag("ASSIGN_SR_BTN_RED_BASED_ON_RPT") != "T" &&
                            string.IsNullOrEmpty(reportPath))
                        {
                            ServerAlert("Service Request", "Report " + reportName + " not found.");
                            return;
                        }

                        PLCSession.PLCCrystalReportName = reportPath;
                        PLCSession.PLCCrystalSelectionFormula = "{TV_SRMASTER.SR_MASTER_KEY} IN [" + string.Join(",", lstMasterKeys) + "]";
                        PLCSession.PLCCrystalReportTitle = "Service Request";
                        PLCSession.PrintCRWReport(true);
                    }
                    else
                    {
                        messageBox.ShowMsg("Service Request", "No Service Request for this Assignment", 1);
                        return;
                    }
                }
            }
            else
            {
                ShowAssignmentChangedError();
            }
        }

        protected void bnPullList_Click(object sender, EventArgs e)
        {
            PLCSession.PLCCrystalReportName = GetPullListCrystalReportName(PLCDBPanel1.getpanelfield("Section"));

            // If report found, print the report
            if (PLCSession.PLCCrystalReportName != "")
            {
                PLCSession.PLCCrystalSelectionFormula = "{TV_LABASSIGN.EXAM_KEY} = " + PLCSession.PLCGlobalAssignmentKey;
                PLCSession.PLCCrystalReportTitle = "Exam Items";
                Session["ReportOpened"] = "T";
                string sectionOne = PLCDBPanel1.getpanelfield("Section");
                Session["SectionForPrint"] = sectionOne;
                PLCSession.PrintCRWReport(true);
            }
        }

        protected void bnDNADates_Click(object sender, EventArgs e)
        {
            MultiView1.ActiveViewIndex = 5;
        }

        protected void btnSaveAssignTask_Click(object sender, EventArgs e)
        {
            List<Task> tasksList = new List<Task>();
            string strTask = ",";
            bool permissionPassed = true;

            foreach (GridViewRow row in gvAssignTask.Rows)
            {
                CodeMultiPick cmpAssignTaskType = (CodeMultiPick)FindControlWithin(row, "cmpAssignTaskType_" + row.RowIndex.ToString());
                List<string> taskTypeList = cmpAssignTaskType.GetSelectedSections();
                if (taskTypeList.Count > 0)
                {
                    foreach (string taskType in taskTypeList)
                    {
                        Task task = new Task();
                        task.ECN = ((HiddenField)row.FindControl("hdnAssignTaskECN")).Value;
                        task.CaseKey = ((HiddenField)row.FindControl("hdnAssignTaskCaseKey")).Value;
                        task.ItemType = ((HiddenField)row.FindControl("hdnAssignTaskItemType")).Value;
                        task.TaskType = taskType;
                        task.Description = ((TextBox)row.FindControl("txtAssignTaskItemDesc")).Text;
                        task.Priority = ((FlexBox)row.FindControl("fbAssignTaskPriority")).GetValue();
                        task.Status = string.IsNullOrEmpty(PLCSession.GetLabCtrl("INITIAL_TASK_STATUS")) == true ? "O" : PLCSession.GetLabCtrl("INITIAL_TASK_STATUS");
                        if (hdnAutoSyncAnalyst.Value == "T")
                        {
                            task.Analyst = PLCDBPanel1.getpanelfield("ANALYST_ASSIGNED").Trim();
                            if (!string.IsNullOrEmpty(task.Analyst))
                                task.Status = "A";
                        }
                        tasksList.Add(task);

                        if (!QMSTaskFilterAllow(taskType, PLCSession.PLCGlobalAnalyst))
                        {
                            permissionPassed = false;
                            if (!strTask.Contains("," + taskType + ","))
                            {
                                strTask += taskType + ",";
                            }
                        }
                    }
                }
            }

            if (!permissionPassed)
            {
                messageBox.OnOkScript = "javascript:document.getElementById('" + btnLoadAssignTaskGrid.ClientID + "').click();";
                messageBox.ShowMsg("Assign Tasks", "The analyst (" + PLCSession.PLCGlobalAnalyst +
                    ") can not work on selected Task/s (" + strTask.TrimStart(',').TrimEnd(',') +
                    ")<br /><br />Please contact the Quality Assurance Manager", 1);
                return;
            }

            if (tasksList.Count > 0)
            {
                if (!PLCDBPanel1.IsBrowseMode)
                {
                    hdnTaskListFocused.Value = string.Empty;
                    ViewState["AssignTasks"] = tasksList;
                    SaveFromTaskPopup = true;
                    PLCButtonPanel1.ClickSaveButton();
                }
                else
                    SaveAssignmentTasks(tasksList, PLCDBPanel1.ReasonChangeKey);
            }
            else
            {
                messageBox.OnOkScript = "javascript:document.getElementById('" + btnLoadAssignTaskGrid.ClientID + "').click();";
                messageBox.ShowMsg("Assign Tasks", "Please specify at least one Task Type", 1);
            }
        }

        protected void btnCancelSaveAssignTask_Click(object sender, EventArgs e)
        {
            hdnTaskListFocused.Value = string.Empty;
        }

        protected void btnSkipAssignTask_Click(object sender, EventArgs e)
        {
            hdnTaskListFocused.Value = string.Empty;
            IsAssignTaskCancelled = true;
            ViewState["AssignTasks"] = null;
            PLCButtonPanel1.ClickSaveButton();
        }

        protected void btnLoadAssignTaskGrid_Click(object sender, EventArgs e)
        {
            LoadAssignTasksGrid();
        }

        protected void btnConfirmUpdate_Click(object sender, EventArgs e)
        {
            // save text in txtConfirmUpdate
            if (txtConfirmUpdate.Text.Trim().Length > 0)
            {
                hdnConfirmUpdate.Value = txtConfirmUpdate.Text;
                if (((Button)sender).ID == "btnConfirmDelete")
                    PLCButtonPanel1.ClickDeleteButton();
                else
                    PLCButtonPanel1.ClickSaveButton();
            }
            else
            {
                PLCButtonPanel1.ClickCancelButton();
            }
        }

        public byte[] GETCRYSTALPDF(string rptname, string selectionformula, string formulalist)
        {
            //string formulalist = "";

            PLCSession.WriteDebug("tab6Assignment.GETCRYSTALPDF IN:" + rptname);

            string ThisURL = HttpContext.Current.Request.Url.AbsoluteUri.ToString();
            int idx = ThisURL.ToUpper().IndexOf("Tab6Assignments".ToUpper());
            try
            {
                ThisURL = ThisURL.Remove(idx);
            }
            catch
            {

            }

            PLCSession.WriteDebug("tab6Assignment.GETCRYSTALPDF 1:" + rptname);

            //PLCSession.SetupConnectionFromCINFO(PLCSession.Pl);
            //PLCSession.SetupConnectionFromCINFO(PLCSession.PLCDBDatabase);
            PLCSession.SetupConnectionFromCINFO(PLCSession.PLCDBName);

            string websvcurl = "";
            byte[] crystalresponse;
            PLCWebCommon.PLCWebMethodsRef.PLCWebMethods webmethods = new PLCWebCommon.PLCWebMethodsRef.PLCWebMethods();

            PLCSession.WriteDebug("tab6Assignment.GETCRYSTALPDF 2:" + rptname);

            try
            {
                websvcurl = PLCSession.GetWebConfiguration("RPTSVCURL");
            }
            catch (Exception ex)
            {
                websvcurl = "";
            }

            PLCSession.WriteDebug("tab6Assignment.GETCRYSTALPDF RPTSVCURL:" + websvcurl);

            crystalresponse = null;

            if (websvcurl == "*LOCAL")
            {
                PLCSession.WriteDebug("tab6Assignment GETCRYSTALPDF before GetCrystalReportResponse:");
                try
                {
                    crystalresponse = PLCDBGlobal.instance.GetCrystalReportResponse(rptname, selectionformula, "", formulalist, PLCSession.PLCDBName, "", "", PLCSession.PLCGlobalAnalyst, PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalAssignmentKey, PLCSession.PLCGlobalLabCode);
                }
                catch (Exception e)
                {
                    PLCSession.WriteDebug("tab6Assignment exceptionin GetCrystalReportResponse:" + e.Message);
                }
                return crystalresponse;
            }

            if (websvcurl == "")
                websvcurl = ThisURL;

            if (!websvcurl.EndsWith("/"))
                websvcurl += "/";

            webmethods.Url = websvcurl + "PLCWebCommon/PLCWebMethods.asmx";

            try
            {
                crystalresponse = webmethods.LoadCrystalReport(rptname, selectionformula, "", formulalist, PLCSession.PLCDBName, "", "", "", PLCSession.PLCGlobalAnalyst, "", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalAssignmentKey, PLCSession.PLCGlobalLabCode);
                return crystalresponse;
            }
            catch (Exception ex)
            {
                PLCSession.WriteDebug("Exception in tab6Assignment calling webmethods.LoadCrystalReport:" + ex.Message);
                return null;
            }
        }

        private Boolean saveReportToPDFData(String examKey, String fileSource, Byte[] data)
        {
            PLCSession.WriteDebug("SaveReportToPDFData IN, examkey:" + examKey, true);
            PLCSession.WriteDebug("SaveReportToPDFData IN, filesource:" + fileSource, true);
            if (data != null)
                PLCSession.WriteDebug("SaveReportToPDFData IN, data.Length:" + data.Length, true);

            if (fileSource == "PENDING")
            {
                if (File.Exists(PLCPath.PdfStamp + "PENDING.PDF"))
                {
                    try
                    {
                        File.Delete(PLCPath.PdfStamp + "PENDING.PDF");
                        File.WriteAllBytes(PLCPath.PdfStamp + "PENDING.PDF", data);
                    }
                    catch { }
                }
            }

            PLCQuery qryPDFDATA = new PLCQuery("DELETE FROM TV_PDFDATA WHERE FILE_SOURCE = ? and FILE_SOURCE_KEY = ? ");
            qryPDFDATA.AddSQLParameter("FILE_SOURCE", fileSource);
            qryPDFDATA.AddSQLParameter("FILE_SOURCE_KEY", examKey);
            qryPDFDATA.ExecSQL();

            OleDbConnection dbConn = new OleDbConnection(PLCSession.GetConnectionString());
            OleDbDataAdapter dbImageAdapt = new OleDbDataAdapter("SELECT * FROM TV_PDFDATA where 0 = 1", dbConn);
            OleDbCommandBuilder dbImageCB = new OleDbCommandBuilder(dbImageAdapt);
            DataSet dbImageSet = new DataSet();
            dbImageAdapt.Fill(dbImageSet, "TV_PDFDATA");
            DataTable dbImageTable = dbImageSet.Tables["TV_PDFDATA"];
            DataRow dbImageRow = dbImageTable.NewRow();

            try
            {
                //      WriteDebug("Uploading Workist Notespacket to PDFDATA", true);
                dbImageRow["DATA_KEY"] = PLCSession.GetNextSequence("PDFDATA_SEQ").ToString();
                dbImageRow["FILE_SOURCE_KEY"] = examKey;
                dbImageRow["FILE_SOURCE"] = fileSource;
                dbImageRow["FORMAT"] = "PDF";
                dbImageRow["STATUS"] = "S";
                dbImageRow["DATE_PRINTED"] = DateTime.Today;
                dbImageRow["DATA"] = data;
                dbImageRow["FILE_SIZE"] = data.Length;
                dbImageRow["PRINTED_BY"] = PLCSession.PLCGlobalAnalyst;

                dbImageTable.Rows.Add(dbImageRow);
                dbImageAdapt.Update(dbImageSet, "TV_PDFDATA");
                dbConn.Close();
            }
            catch (Exception e)
            {
                PLCSession.WriteDebug("Exception in saveReportToPDFData:" + e.Message, true);
                return false;
            }

            //        WriteDebug("Upload Complete", true);

            return true;
        }


        protected void btnOKSign_Click(object sender, EventArgs e)
        {
            string groupRes = hdnGroupCode.Value;
            int sigID = PLCDBGlobal.instance.GetAnalystSignatureID();
            if (sigID != -1)
                PLCDBGlobal.instance.CreateAssignmentDigitalSignature(sigID, PLCSession.PLCGlobalAssignmentKey, groupRes);

            String assignSection = PLCDBPanel1.getpanelfield("SECTION");
            String reportFormat = PLCDBPanel1.getpanelfield("REPORT_FORMAT");
            String reportName = reportFormat;
            if (String.IsNullOrWhiteSpace(reportName)) reportName = assignSection;

            bool usesReviewRotation = PLCSession.GetLabCtrlFlag("USES_REVIEW_ROTATION") == "T";

            switch (groupRes)
            {
                case "SIGNED":
                    PLCDBGlobal.instance.SetAssignmentTechReviewed(PLCSession.PLCGlobalAssignmentKey, groupRes);

                    if (PLCSession.CheckSectionFlag(assignSection, "PRINT_CRYSTAL_REPORT_ONLY")
                        && !string.IsNullOrEmpty(PLCSession.FindCrystalReport(reportName, false)))
                    {
                        if (PLCSession.CheckSectionFlag(assignSection, "USES_CRYSTAL_STAMPING"))
                        {
                            // Save a copy of the final report (without signatures, inthe PDF data table)
                            //Signatures will go on at approval.

                            PLCSession.WriteDebug("tab6Assignment 1.USES_CRYSTAL_STAMPING 1");

                            PLCQuery qry = new PLCQuery(String.Format("SELECT * FROM TV_TASKLIST WHERE CASE_KEY = {0} AND EXAM_KEY = {1} AND STATUS <> 'C'",
                            PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalAssignmentKey));

                            Byte[] rptBytes = GETCRYSTALPDF(reportName, "{TV_LABASSIGN.EXAM_KEY} = " + PLCSession.PLCGlobalAssignmentKey, "REPORTMODE='FINAL'");

                            PLCSession.WriteDebug("tab6Assignment 1.USES_CRYSTAL_STAMPING 2");

                            if (rptBytes != null)
                                saveReportToPDFData(PLCSession.PLCGlobalAssignmentKey, "PENDING", rptBytes);

                            PLCSession.WriteDebug("tab6Assignment 1.USES_CRYSTAL_STAMPING 3");

                            Byte[] draftBytes = GETCRYSTALPDF(reportName, "{TV_LABASSIGN.EXAM_KEY} = " + PLCSession.PLCGlobalAssignmentKey, "REPORTMODE='DRAFT'");

                            PLCSession.WriteDebug("tab6Assignment 1.USES_CRYSTAL_STAMPING 4");

                            if (draftBytes != null)
                                saveReportToPDFData(PLCSession.PLCGlobalAssignmentKey, "DRAFT", draftBytes);

                            PLCSession.WriteDebug("tab6Assignment 1.USES_CRYSTAL_STAMPING 5");
                        }

                        PLCSession.WriteDebug("tab6Assignment 1.USES_CRYSTAL_STAMPING 6");
                    }
                    
                    if (usesReviewRotation && PLCDBGlobal.instance.NeedtoAssignReviewer(PLCSession.PLCGlobalAssignmentKey, usesReviewRotation))
                    {
                        string reviewer = PLCDBGlobal.instance.GetNextReviewerInRotation(PLCSession.PLCGlobalAssignmentKey);
                        PLCDBGlobal.instance.AssignReviewer(PLCSession.PLCGlobalAssignmentKey, reviewer);

                        string reviewerName = PLCSession.GetCodeDesc("TV_ANALYST", reviewer);
                        dlgAlertMessage.ShowAlert("Sign", "This report has been routed for review.<br>Review will be done by: " + reviewerName);
                    }
                    else
                        messageBox.ShowMsg("Sign", "Sign Complete", 0);

                    break;

                case "REVIEW":
                case "CODISREV":
                    PLCDBGlobal.instance.SetAssignmentTechReviewed(PLCSession.PLCGlobalAssignmentKey, groupRes);
                    if (PLCSession.CheckSectionFlag(PLCDBPanel1.getpanelfield("SECTION"), "CREATE_DNA_WS_AFTER_REVIEW"))
                    {
                        Session["OCXDNAProcess"] = "T";
                        string returnPage = "Dashboard.aspx";
                        if (Request.UrlReferrer != null)
                            returnPage = Request.UrlReferrer.AbsolutePath.Substring(Request.UrlReferrer.AbsolutePath.LastIndexOf("/") + 1);
                        WebOCX.OnSuccessScript = "CheckBrowserWindowID(function () { window.location = '" + returnPage + "'; });";
                        WebOCX.StartDNAProcess(PLCSession.PLCGlobalAssignmentKey, groupRes);
                    }
                    else if (PLCSession.CheckSectionFlag(PLCDBPanel1.getpanelfield("SECTION"), "PRINT_CRYSTAL_REPORT_ONLY") &&
                        PLCSession.CheckSectionFlag(PLCDBPanel1.getpanelfield("SECTION"), "VIEW_REPORT_REVIEW"))
                    {
                        PLCSession.PLCCrystalReportName = PLCDBGlobal.instance.GetWordReportName(PLCSession.PLCGlobalAssignmentKey);
                        PLCSession.PLCCrystalSelectionFormula = "{TV_LABASSIGN.EXAM_KEY} = " + PLCSession.PLCGlobalAssignmentKey;
                        PLCSession.PrintCRWReport(true);
                        messageBox.ShowMsg("Review", "Review Complete", 0);
                    }
                    else
                        messageBox.ShowMsg("Review", "Review Complete", 0);

                    string analystAssigned = PLCDBPanel1.getpanelfield("ANALYST_ASSIGNED");
                    if (!string.IsNullOrEmpty(analystAssigned))
                        PLCDBGlobal.instance.NeedtoAssignReviewer(PLCSession.PLCGlobalAssignmentKey, true);

                    break;

                case "APPROVE":
                    if (PLCSession.CheckSectionFlag(PLCDBPanel1.getpanelfield("SECTION"), "PRINT_CRYSTAL_REPORT_ONLY"))
                    {
                        PLCDBGlobal.instance.ApproveAssignment(PLCSession.PLCGlobalAssignmentKey);
                        PLCDBGlobal.instance.CompleteServiceRequest(Convert.ToInt32(PLCSession.PLCGlobalAssignmentKey));
                        CloseImageVaultWindow();

                        if (PLCSession.CheckSectionFlag(PLCDBPanel1.getpanelfield("SECTION"), "CREATE_DNA_WS_AFTER_APPROVAL"))
                        {
                            Session["OCXDNAProcessApprove"] = "T";
                            WebOCX.OnSuccessScript = "CheckBrowserWindowID(function () { document.getElementById('" + btnDNAProcessApprove.ClientID + "').click(); });";
                            WebOCX.StartDNAProcess(PLCSession.PLCGlobalAssignmentKey, groupRes);
                            return;
                        }
                        else
                        {
                            PrintReportOnApprove(PLCDBPanel1.getpanelfield("SECTION"));
                            GridView1.SelectedIndex = 0;

                            if(PLCSession.GetLabCtrl("FORAY_EXPORT_ALL_ASSETS") == "T")
                            {
                                OpenForayLink();
                            }
                            else
                            {
                                messageBox.ShowMsg("Approve", "Approve Complete", 0);
                            }
                        }
                    }
                    else
                        CallUCApproveReport();

                    // increment only if USES_REVIEW_ROTATION = F and the users skipped review
                    PLCQuery qryAssignReviewed = new PLCQuery("SELECT REVIEWED FROM TV_LABASSIGN WHERE EXAM_KEY = " + PLCSession.PLCGlobalAssignmentKey);
                    qryAssignReviewed.Open();
                    if (qryAssignReviewed.HasData())
                    {
                        bool isAssignReviewed = qryAssignReviewed.FieldByName("REVIEWED") == "Y";
                        PLCDBGlobal.instance.NeedtoAssignReviewer(PLCSession.PLCGlobalAssignmentKey, !usesReviewRotation && !isAssignReviewed);
                    }
                    break;
            }

            if (groupRes != "APPROVE" || PLCSession.CheckSectionFlag(PLCDBPanel1.getpanelfield("SECTION"), "PRINT_CRYSTAL_REPORT_ONLY"))
            {
                UpdateRecLockLblBtnState();
                int selIndex = GridView1.SelectedIndex;
                GridView1.InitializePLCDBGrid();
                if (GridView1.Rows.Count > 0)
                {
                    GridView1.SelectedIndex = selIndex;
                    GrabGridRecord();
                    GridView1.SetControlMode(true);
                }
                else
                    ResetControls();
            }
        }

        private bool OpenForayLink()
        {
            GenerateAdamsWebURL();
            SetForayOcxParametersToHiddenField();

            ScriptManager.RegisterStartupScript(this, this.GetType(), DateTime.Now.ToString(),
                            String.Format("OpenForayLink('{0}')", PLCSession.PLCGlobalAssignmentKey), true);
            return true;
        }

        private void CloseImageVaultWindow()
        {
            ScriptManager.RegisterStartupScript(this, this.GetType(), DateTime.Now.ToString(),
                            "OnPageUnload();", true);
        }

        protected void btnYesAdminClose_Click(object sender, EventArgs e)
        {
            string assignmentKey = hdnExamKey.Value;

            //check assignmentkey
            if (assignmentKey == "")
            {
                ShowErrorMessage(PLCSession.GetSysPrompt("TAB6Assignments.ACCESS_ERROR", "There is an error accessing the assignment."));
                return;
            }

            string completionCode = null;
            string comments = null;
            string assignmentAction = null;
            string srStatus = string.Empty;
            string closeComments = string.Empty;

            if (PLCSession.GetLabCtrl("USES_CLOSE_CODES") == "T")
            {
                completionCode = chCompletionCode.GetValue();
                comments = txtComments.Text.Trim();

                //check if completion code is entered
                if (completionCode == "")
                {
                    ShowErrorMessage(PLCSession.GetSysPrompt("TAB6Assignments.COMPLETE_CODE_REQ", "Completion Code is required."));
                    return;
                }

                //check if completion code is valid
                PLCQuery qryCompletion = new PLCQuery("SELECT COMMENT_REQUIRED, ASSIGNMENT_ACTION, SR_STATUS, DESCRIPTION FROM TV_CLOSECODE WHERE COMPLETION_CODE = '" + completionCode + "'");
                if (qryCompletion.Open() && qryCompletion.HasData())
                {
                    if (qryCompletion.FieldByName("COMMENT_REQUIRED") == "T" && comments == "")
                    {
                        ShowErrorMessage(PLCSession.GetSysPrompt("TAB6Assignments.COMMENTS_REQ", "Comments are required."));
                        return;
                    }

                    assignmentAction = qryCompletion.FieldByName("ASSIGNMENT_ACTION");
                    srStatus = qryCompletion.FieldByName("SR_STATUS").Trim();
                    closeComments = string.Format("{0} {1}",
                        qryCompletion.FieldByName("DESCRIPTION"),
                        comments).Trim();
                }
                else
                {
                    ShowErrorMessage(PLCSession.GetSysPrompt("TAB6Assignments.COMPLETE_CODE_INVALID", "Completion Code is invalid."));
                    return;
                }
            }

            string statusMessage = "";
            PLCDBGlobal DBGlobal = new PLCDBGlobal();

            if (assignmentAction == "R") // Reset the assignment
            {
                if (DBGlobal.AdminResetAssignment(assignmentKey, completionCode, comments, ref statusMessage))
                {
                    if (!string.IsNullOrEmpty(completionCode))
                    {
                        PLCDBGlobal.instance.ResetServiceRequest(Convert.ToInt32(assignmentKey), srStatus, "Lab Assignment was reset: " + closeComments);
                    }

                    GridView1.InitializePLCDBGrid();
                }
            }
            else if (assignmentAction == "W") // Close related tasks
            {
                PLCQuery qryTasks = new PLCQuery(string.Format("UPDATE TV_TASKLIST SET STATUS = 'C' WHERE EXAM_KEY = {0} and STATUS <> 'C' and STATUS <> 'X'", assignmentKey));
                if (qryTasks.ExecSQL())
                    statusMessage = "Assignment Tasks Closed";
            }
            else // Close the assignment; if successful, rebind assignments grid
            {
                if (DBGlobal.AdminCloseAssignment(assignmentKey, completionCode, comments, ref statusMessage))
                {
                    if (!string.IsNullOrEmpty(completionCode))
                    {
                        PLCDBGlobal.instance.CancelServiceRequest(Convert.ToInt32(assignmentKey), srStatus, "Lab Assignment was closed: " + closeComments);
                    }

                    GridView1.InitializePLCDBGrid();

                    // Ticket#36927 Clear completion code and comments
                    if (chCompletionCode != null)
                        chCompletionCode.ClearValues();
                    txtComments.Text = "";
                    spRequired.Visible = false;
                }
            }

            UnlockRecord();
            CloseImageVaultWindow();

            if (GridView1.Rows.Count > 0)
            {
                GridView1.SelectedIndex = 0;
                GrabGridRecord();
            }
            else
            {
                ResetControls();
            }

            if (statusMessage != "")
                messageBox.ShowMsg("Admin Close", statusMessage, 0);
        }

        protected void btnConfirmUnlock_Click(object sender, EventArgs e)
        {
            PLCQuery qyLabExam = QuerySelectedLabExam();
            if (qyLabExam.IsEmpty())
            {
                ShowAssignmentChangedError();
                return;
            }

            if (txtConfirmUnlock.Text.Trim() != "")
                UnlockRecord();
        }

        protected void bnRollback_Click(object sender, EventArgs e)
        {
            mbGenericConfirm.Caption = bnRollback.Text;
            mbGenericConfirm.Message = string.Format("The assignment status will be rolled back to '{0}'. Continue?",
                PLCDBGlobal.instance.IsAssignmentTechReviewed(PLCSession.PLCGlobalAssignmentKey) ? "Ready For Tech Review" : "Draft Printed");
            mbGenericConfirm.Show();
            return;
        }

        protected void bnRollbackOK_Click(object sender, EventArgs e)
        {
            if (PLCDBGlobal.instance.IsAssignmentTechReviewed(PLCSession.PLCGlobalAssignmentKey))
            {
                PLCDBGlobal.instance.RemoveAssignmentSignature(PLCSession.PLCGlobalAssignmentKey, false, "APPROVE");
                PLCDBGlobal.instance.RemoveAssignmentSignature(PLCSession.PLCGlobalAssignmentKey, false, "REVIEW");
                PLCDBGlobal.instance.ResetChecklists(PLCSession.PLCGlobalAssignmentKey, false, "APPROVE");
                PLCDBGlobal.instance.ResetChecklists(PLCSession.PLCGlobalAssignmentKey, false, "REVIEW");
                PLCDBGlobal.instance.ResetAssignmentReviewFlags(PLCSession.PLCGlobalAssignmentKey);
                PLCDBGlobal.instance.SetAssignmentTechReviewed(PLCSession.PLCGlobalAssignmentKey, "SIGNED");
            }
            else
            {
                PLCDBGlobal.instance.RemoveAssignmentSignature(PLCSession.PLCGlobalAssignmentKey);
                //PLCDBGlobal.instance.DeleteNotesPacket(PLCSession.PLCGlobalAssignmentKey);
                PLCDBGlobal.instance.ResetChecklists(PLCSession.PLCGlobalAssignmentKey, false, "APPROVE");
                PLCDBGlobal.instance.ResetChecklists(PLCSession.PLCGlobalAssignmentKey, false, "REVIEW");
                PLCDBGlobal.instance.ResetChecklists(PLCSession.PLCGlobalAssignmentKey, false, "SIGNED");
                PLCDBGlobal.instance.ResetAssignmentReviewFlags(PLCSession.PLCGlobalAssignmentKey);
                PLCDBGlobal.instance.UpdateAssignmentToDraftPrinted(PLCSession.PLCGlobalAssignmentKey);

                if (PLCDBGlobal.instance.AssignmentReconciledAlready(PLCSession.PLCGlobalAssignmentKey))
                    PLCDBGlobal.instance.UpdateAssignmentToUnVerified(PLCSession.PLCGlobalAssignmentKey);
            }

            GridView1.InitializePLCDBGrid();
            GrabGridRecord();
        }

        protected void ReviewPlanSave_Click(object sender, EventArgs e)
        {
            if (ReviewPlan1.ReviewEditMode)
            {
                int selIndex = GridView1.SelectedIndex;
                GridView1.InitializePLCDBGrid();
                GridView1.SelectedIndex = selIndex;
                GrabGridRecord();

                if (MultiView1.ActiveViewIndex == 3)
                    LoadRoutingInfo(false);
            }
            else
                GetSignature("SIGNED");
        }

        protected void bnReviewPlan_Click(object sender, EventArgs e)
        {
            ReviewPlan1.ReviewEditMode = true;
            ReviewPlan1.ReviewSection = PLCDBPanel1.getpanelfield("SECTION");
            ReviewPlan1.Show();
        }

        protected void btnDNAProcessApprove_Click(object sender, EventArgs e)
        {
            PrintReportOnApprove(PLCDBPanel1.getpanelfield("SECTION"));
            ScriptManager.RegisterClientScriptBlock(btnDNAProcessApprove, btnDNAProcessApprove.GetType(), btnDNAProcessApprove.ClientID + "_Redirect", "window.location = 'TAB6Assignments.aspx'", true);
        }

        protected void mbGenericConfirm_OkClick(object sender, EventArgs e)
        {
            if (mbGenericConfirm.Caption == bnSign.Text)
                bnSign_Click(mbGenericConfirm, null);
            else if (mbGenericConfirm.Caption == bnReview.Text)
                bnReview_Click(mbGenericConfirm, null);
            else if (mbGenericConfirm.Caption == bnApprove.Text)
                bnApprove_Click(mbGenericConfirm, null);
            else if (mbGenericConfirm.Caption == bnAdmClose.Text)
                bnAdmClose_Click(mbGenericConfirm, null);
            else if (mbGenericConfirm.Caption == bnRollback.Text)
                bnRollbackOK_Click(mbGenericConfirm, null);
        }

        protected void mbSigInvalidate_CancelClick(object sender, EventArgs e)
        {
            PLCButtonPanel1.ClickCancelButton();
        }

        protected void dlgMessage_ConfirmClick(object sender, DialogEventArgs e)
        {
            switch (e.DialogKey)
            {
                case "ITEMS_NOT_IN_CUSTODY":
                    if (!string.IsNullOrEmpty(AssignmentNextPage))
                        Response.Redirect(AssignmentNextPage);
                    break;
                default:
                    break;
            }

            // Routing
            if (e.DialogKey == RoutingDialogKey.ResetAssignment.ToString())
            {
                LastRoutingDialog = RoutingDialogKey.ResetAssignment;
                btnRouteOK_Click(sender, EventArgs.Empty);
            }
        }

        protected void dlgMessage_CancelClick(object sender, DialogEventArgs e)
        {
            // Routing
            if (e.DialogKey == RoutingDialogKey.ResetAssignment.ToString())
            {
                ShowRoutingPopup();
            }
        }

        protected void dlgMessageResetAssign_Click(object sender, DialogEventArgs e)
        {
            PLCButtonPanel1.ClickCancelButton();
        }

        protected void mbARCheck_OkClick(object sender, EventArgs e)
        {
            IsItemARDeleteConfirmed = true;

            if (BigViewEdit)
                //                SaveBigViewItems();
                BigViewEdit.ToString(); // To suppress empty statement compiler warning.
            else
                PLCButtonPanel1.ClickSaveButton();
        }

        protected void mbNotesPacketRequired_OKClick(object sender, EventArgs e)
        {
            GenerateNotes();
        }

        protected void mbNotesPacketRequired_CancelClick(object sender, EventArgs e)
        {
            ProcessReview("SIGNED");
        }

        protected void mbCancelTasks_OkClick(object sender, EventArgs e)
        {
            IsItemCancelTaskConfirmed = true;
            PLCButtonPanel1.ClickSaveButton();
        }

        protected void mbCancelTasks_CancelClick(object sender, EventArgs e)
        {
            IsItemARDeleteConfirmed = false;
            IsItemCancelTaskConfirmed = false;
        }

        protected void PLCDBPanel1_PLCDBPanelCodeHeadChanged(object sender, PLCDBPanelCodeHeadChangedEventArgs e)
        {
            if (e.CodeTable == "TV_ANALYST")
            {
                //PLCDBPanel1.setpanelfield("ANALYST_DATE", DateTime.Now.ToString("MM/dd/yyyy"));

                // Set focus to the control that initiated the postback. If we don't do this, the focus reverts back to the first control after auto-postback.
                string nextFieldClientID = PLCDBPanel1.GetNextFieldClientID("ANALYST_ASSIGNED");
                if (nextFieldClientID != "" && PLCDBPanel1.IsPostbackPanelField("ANALYST_ASSIGNED"))
                {
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "nextfieldfocus",
                        String.Format("SetFocusWithDelay('{0}', 1000)", nextFieldClientID), true);
                }
            }
        }

        protected void PLCDBPanel1_PLCDBPanelButtonClick(object sender, PLCButtonClickEventArgs e)
        {
        }

        protected void PLCDBPanel1_SetDefaultRecord(object sender, PLCDBPanelSetDefaultRecordEventArgs e)
        {
            int seq = NewAssignSequence = PLCDBGlobal.instance.GetNextAssignmentSequence(PLCSession.PLCGlobalCaseKey);
            //set all default record values (TV_LABASSIGN)
            e.NewRecordValues.Add("CASE_KEY", PLCSession.PLCGlobalCaseKey);
            e.NewRecordValues.Add("SEQUENCE", seq);
            e.NewRecordValues.Add("STATUS", PLCDBGlobalClass.GetExamStatusCode("S", "0"));
            e.NewRecordValues.Add("DATE_ASSIGNED", DateTime.Today.ToShortDateString());
            e.NewRecordValues.Add("PRIORITY", string.IsNullOrEmpty(PLCDBPanel1.getpanelfield("PRIORITY")) ? PLCCommon.instance.LabAssignPriority() : PLCDBPanel1.getpanelfield("PRIORITY"));
            e.NewRecordValues.Add("ENTRY_ANALYST", PLCSession.PLCGlobalAnalyst);
            e.NewRecordValues.Add("ENTRY_TIME_STAMP", DateTime.Now);

            PLCSession.PLCGlobalAssignmentKey = "0";
        }

        protected void PLCDBPanel1_PLCDBPanelGetNewRecord(object sender, PLCDBPanelGetNewRecordEventArgs e)
        {
            //get next exam key and add to record values
            string examKey = PLCSession.GetNextSequence("LABEXAM_SEQ").ToString();
            e.NewRecordValues.Add("EXAM_KEY", examKey);

            //set newwhereclause
            e.newWhereClause = dbpDNADates.PLCWhereClause = " where EXAM_KEY = " + examKey;

            //set global exam key
            PLCSession.PLCGlobalAssignmentKey = examKey;
        }

        protected void PLCDBPanel1_PreRender(object sender, EventArgs e)
        {
            if (PLCSession.CheckUserOption("NOEDITAA"))
                PLCDBPanel1.SetMyFieldMode("ANALYST_ASSIGNED", true);

            if (GridView1.Rows.Count > 0)
            {
                string verificationType;
                bool showVerification = ShowReportVerificationButton(out verificationType);

                bnVerifyReports.Visible = showVerification;
                bnReports.Visible = !showVerification;

                // Disable controls in the page for read only access
                if (PLCCommonClass.IsReadOnlyAccess("WEBINQ,RONLYASTAB"))
                    bnRemoveBlind.Visible = false;
                else
                    bnRemoveBlind.Visible = PLCSession.CheckUserOption("REMBLIND") && verificationType == "B";
            }
        }

        protected void PLCButtonPanel1_PLCButtonClick(object sender, PLCButtonClickEventArgs e)
        {
            Session["IsEditClicked"] = null;

            if (e.button_name == "ADD")
            {
                if (e.button_action == "BEFORE")
                {
                    if (PLCSession.GetLabCtrlFlag("USES_FOLLOW_ON_TASKS").Equals("T"))
                        Response.Redirect("NewAssignment.aspx");

                    SelectTab(VIEW_ITEMS);
                }
                else
                {
                    lPLCLockStatus.Text = string.Empty;
                    dvPLCLockStatus.Visible = false;

                    GridView1.SetControlMode(false);
                    EnableButtonControls(false, false);
                    DisableLink("bnDetails");

                    LoadNames("0");
                    if (gvNameItem.Rows.Count > 0)
                    {
                        if (PLCSession.GetLabCtrl("CHECK_ASSIGNMENT_NAME_ITEM") != "T")
                        {
                            gvNameItem.CheckAllRows();
                        }
                        else
                        {
                            foreach (GridViewRow row in gvNameItem.Rows)
                                ((CheckBox)row.FindControl("cbSelect")).Checked = false;
                        }
                    }

                    LoadAssignmentItems("0");
                    SetAssignmentItemControlsMode(true, true);
                    PLCDBPanel1.setpanelfield("LAB_CODE", PLCSession.PLCGlobalLabCode);

                    bnBigView.Enabled = false;
                    CloseImageVaultWindow();
                }

                IsAssignAddMode = true;
                assignedItems.Clear();
            }

            if ((e.button_name == "EDIT") & (e.button_action == "BEFORE"))
            {
                SelectTab(VIEW_ITEMS);
                string LockedInfo = string.Empty, LockedTypeRes = string.Empty;
                if (PLCDBGlobal.instance.IsRecordLocked("TV_LABEXAM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalAssignmentKey, "-1", out LockedInfo, out LockedTypeRes))
                {
                    e.button_canceled = true;
                    dlgMessage.ShowAlert("Record Lock", string.Format("Assignment locked by another user for {0}.<br/>{1}", ObjAssignment.GetLockedTypeResMessage(LockedTypeRes), LockedInfo));
                    lPLCLockStatus.Text = LockedInfo;
                    dvPLCLockStatus.Visible = true;
                    UpdateRecLockLblBtnState(true);
                    return;
                }
                else
                {
                    PLCDBGlobal.instance.LockUnlockRecord("TV_LABEXAM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalAssignmentKey, "-1", true, "EDIT");
                    ((MasterPage)this.Master).SetCanRecordLock("T");
                }

                if (!PLCCommonClass.EditAssignmentInfoAllowed(PLCDBPanel1.getpanelfield("ANALYST_ASSIGNED")))
                {
                    e.button_canceled = true;
                    messageBox.ShowMsg("Not Authorized", "You are not allowed to change this assignment.", 1);
                    return;
                }

                if (PLCDBGlobal.instance.AssignmentSignedAlready(PLCSession.PLCGlobalAssignmentKey, "SIGNED"))
                {
                    string confirmMsg = "The report had already been signed. Saving changes will remove the signature from the assignment. Do you want to continue?";

                    if (PLCDBGlobal.instance.AssignmentReconciledAlready(PLCSession.PLCGlobalAssignmentKey))
                        confirmMsg = "The report had already been signed and verified. Saving changes will remove the signature from the assignment and remove the verification. Do you want to continue?";

                    dlgMessageResetAssign.ShowYesNo("Confirm", confirmMsg);
                    return;
                }
            }

            if ((e.button_name == "EDIT") & (e.button_action == "AFTER"))
            {
                GridView1.SetControlMode(false);
                EnableButtonControls(false, false);
                DisableLink("bnDetails");
                SetAssignmentItemControlsMode(true, true);

                Session["IsEditClicked"] = "T";
                bnBigView.Enabled = false;
                IsAssignAddMode = false;

                if (hdnIsAssignmentInBatch.Value == "1")
                {
                    PLCDBPanel1.SetBrowseMode();
                    PLCDBPanel1.IsBrowseMode = false;
                    PLCDBPanel1.EnablePanelField("REPORT_TYPE", true);
                    PLCDBPanel1.EnablePanelField("REPORT_FORMAT", true);
                    PLCDBPanel1.EnablePanelField("COMMENTS", true);
                    PLCDBPanel1.EnablePanelField("SPECIAL_ID", true);
                }
            }

            if ((e.button_name == "DELETE") & (e.button_action == "BEFORE"))
            {
                SelectTab(VIEW_ITEMS);
                string LockedInfo = string.Empty, LockedTypeRes = string.Empty;
                if (PLCDBGlobal.instance.IsRecordLocked("TV_LABEXAM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalAssignmentKey, "-1", out LockedInfo, out LockedTypeRes))
                {
                    e.button_canceled = true;
                    dlgMessage.ShowAlert("Record Lock", string.Format("Assignment locked by another user for {0}.<br/>{1}", ObjAssignment.GetLockedTypeResMessage(LockedTypeRes), LockedInfo));
                    lPLCLockStatus.Text = LockedInfo;
                    dvPLCLockStatus.Visible = true;
                    UpdateRecLockLblBtnState(true);
                    return;
                }
                else
                {
                    PLCDBGlobal.instance.LockUnlockRecord("TV_LABEXAM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalAssignmentKey, "-1", true, "EDIT");
                    ((MasterPage)this.Master).SetCanRecordLock("T");
                }

                if (!PLCSession.CanDeleteAssignment(PLCSession.PLCGlobalAssignmentKey))
                {
                    e.button_canceled = true;
                    messageBox.ShowMsg("Not Authorized", "You are not authorized to delete this assignment", 0);
                    return;
                }
                else
                {
                    if (PLCSession.GetLabCtrl("USES_IMAGE_VAULT_SERVICE").Equals("T"))
                        GetAssignAttachEntryIDs();
                    if (PLCSession.GetLabCtrl("LOG_EDITS_TO_NARRATIVE") == "T")
                    {
                        if (hdnConfirmUpdate.Value.Trim().Length > 0)
                        {
                            StringBuilder auditText = new StringBuilder();
                            auditText.AppendLine("ASSIGNMENT DELETED");
                            auditText.AppendLine(string.Format("EXAM KEY: {0} - SECTION: {1}", PLCSession.PLCGlobalAssignmentKey, PLCDBPanel1.GetFieldDesc("SECTION")));
                            SaveConfirmUpdate(auditText.ToString());
                            hdnConfirmUpdate.Value = "";
                        }
                        else
                        {
                            mInput.ShowMsg("Case update reason", PLCSession.GetSysPrompt("TAB6Assignments.UPDATE_REASON", "Please enter the reason for your changes"), 
                                0, txtConfirmUpdate.ClientID, btnConfirmDelete.ClientID, PLCSession.GetSysPrompt("TAB6Assignments.mInput.SAVE", "Save"),
                                PLCSession.GetSysPrompt("TAB6Assignments.mInput.CANCEL", "Cancel"));
                            e.button_canceled = true;
                            return;
                        }
                    }

                    if (PLCSession.PLCDatabaseServer == "MSSQL")
                    {
                        PLCQuery qryDeleteCase = new PLCQuery();
                        qryDeleteCase.AddProcedureParameter("examKey", Convert.ToInt32(PLCSession.PLCGlobalAssignmentKey), 10, OleDbType.Integer, ParameterDirection.Input);
                        qryDeleteCase.AddProcedureParameter("caseKey", Convert.ToInt32(PLCSession.PLCGlobalCaseKey), 10, OleDbType.Integer, ParameterDirection.Input);
                        qryDeleteCase.AddProcedureParameter("labCase", PLCSession.PLCGlobalLabCase, 8, OleDbType.VarChar, ParameterDirection.Input);
                        qryDeleteCase.AddProcedureParameter("deleteReason", hdnConfirmUpdate.Value, hdnConfirmUpdate.Value.Length, OleDbType.VarChar, ParameterDirection.Input);
                        qryDeleteCase.AddProcedureParameter("userId", PLCSession.PLCGlobalAnalyst, 15, OleDbType.VarChar, ParameterDirection.Input);
                        qryDeleteCase.AddProcedureParameter("program", "iLIMS" + PLCSession.PLCBEASTiLIMSVersion, 8, OleDbType.VarChar, ParameterDirection.Input);
                        qryDeleteCase.AddProcedureParameter("OSComputerName", PLCSession.GetOSComputerName(), 50, OleDbType.VarChar, ParameterDirection.Input);
                        qryDeleteCase.AddProcedureParameter("OSUserName", PLCSession.GetOSUserName(), 50, OleDbType.VarChar, ParameterDirection.Input);
                        qryDeleteCase.AddProcedureParameter("OSAddress", PLCSession.GetOSAddress(), 50, OleDbType.VarChar, ParameterDirection.Input);
                        qryDeleteCase.AddProcedureParameter("BuildNumber", PLCSession.PLCBEASTiLIMSVersion, 100, OleDbType.VarChar, ParameterDirection.Input);
                        qryDeleteCase.AddProcedureParameter("labCode", PLCSession.PLCGlobalLabCode, 3, OleDbType.VarChar, ParameterDirection.Input);
                        qryDeleteCase.ExecuteProcedure("DELETE_CASE_ASSIGNMENT");
                    }
                    else
                    {
                        PLCQuery qryDeleteCase = new PLCQuery();
                        List<OracleParameter> parameters = new List<OracleParameter>();
                        parameters.Add(new OracleParameter("examKey", Convert.ToInt32(PLCSession.PLCGlobalAssignmentKey)));
                        parameters.Add(new OracleParameter("caseKey", Convert.ToInt32(PLCSession.PLCGlobalCaseKey)));
                        parameters.Add(new OracleParameter("labCase", PLCSession.PLCGlobalLabCase));
                        parameters.Add(new OracleParameter("deleteReason", hdnConfirmUpdate.Value));
                        parameters.Add(new OracleParameter("userId", PLCSession.PLCGlobalAnalyst));
                        parameters.Add(new OracleParameter("program", ("iLIMS" + PLCSession.PLCBEASTiLIMSVersion).Substring(0, 8)));
                        parameters.Add(new OracleParameter("OSComputerName", PLCSession.GetOSComputerName()));
                        parameters.Add(new OracleParameter("OSUserName", PLCSession.GetOSUserName()));
                        parameters.Add(new OracleParameter("OSAddress", PLCSession.GetOSAddress()));
                        parameters.Add(new OracleParameter("BuildNumber", PLCSession.PLCBEASTiLIMSVersion));
                        parameters.Add(new OracleParameter("labCode", PLCSession.PLCGlobalLabCode));
                        qryDeleteCase.ExecuteOracleProcedure("DELETE_CASE_ASSIGNMENT", parameters);
                    }
                }
            }

            if ((e.button_name == "DELETE") & (e.button_action == "AFTER"))
            {
                PLCDBGlobal.instance.LockUnlockRecord("TV_LABEXAM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalAssignmentKey, "-1", false);
                ((MasterPage)this.Master).SetCanRecordLock(string.Empty);

                PLCDBGlobal.instance.UpdateCaseStatus();

                if (PLCSession.GetLabCtrl("USES_IMAGE_VAULT_SERVICE").Equals("T"))
                    DeleteAttachmentsOnServer();
                IsItemARDeleteConfirmed = false;
                IsAssignAddMode = false;
                assignedItems.Clear();

                GridView1.InitializePLCDBGrid();
                if (GridView1.Rows.Count > 0)
                {
                    GridView1.SelectedIndex = 0;
                    GridView1.SetClientSideScrollToSelectedRow();
                    GrabGridRecord();
                }
                else
                {
                    ResetControls();
                }
                SetLabCaseHeader();
                CloseImageVaultWindow();
            }

            if ((e.button_name == "SAVE") & (e.button_action == "BEFORE"))
            {
                if (PLCDBPanel1.HasPanelRec("TV_LABEXAM", "SEQUENCE"))
                {
                    string currentSequence = PLCDBPanel1.getpanelfield("SEQUENCE");
                    if (AssignSequenceExists(currentSequence, e.row_added))
                    {
                        if (e.row_added)
                        {
                            int seq = NewAssignSequence = PLCDBGlobal.instance.GetNextAssignmentSequence(PLCSession.PLCGlobalCaseKey);
                            PLCDBPanel1.setpanelfield("SEQUENCE", seq.ToString());
                            messageBox.ShowMsg("Assignment Sequence", "The Assignment Sequence #" + currentSequence + " already exists. The next sequence will be assigned.", 1);
                        }
                        else
                        {
                            messageBox.ShowMsg("Assignment Sequence", "The Assignment Sequence #" + currentSequence + " already exists.", 1);
                            e.button_canceled = true;
                            return;
                        }
                    }
                }

                if (!PLCDBGlobal.instance.ValidateDueDate(PLCDBPanel1.getpanelfield("DUE_DATE"), PLCDBPanel1.GetOriginalValue("DUE_DATE")))
                {
                    e.button_canceled = true;
                    messageBox.ShowMsg("Not Authorized", "Due Date entered should be a current or future date.", 1);
                    return;
                }

                if (!ValidateItems(PLCSession.PLCGlobalAssignmentKey))
                {
                    e.button_canceled = true;
                    return;
                }

                if (!(DoesAssignmentHaveItemsChecked() || PLCSession.GetLabCtrl("NO_ITEM_ASSIGNMENTS") == "T"))
                {
                    messageBox.ShowMsg("Assign Items", "Please select items for this Assignment", 0);
                    e.button_canceled = true;
                    return;
                }

                // Add tasks to new assignments
                if (e.row_added && ViewState["AssignTasks"] == null && !IsAssignTaskCancelled && PLCSession.GetLabCtrl("ASSIGNMENT_ALWAYS_HAS_TASK") == "T" &&
                    (!(PLCSession.GetLabCtrl("NO_ITEM_ASSIGNMENTS") == "T") || DoesAssignmentHaveItemsChecked()))
                {
                    LoadAssignTasksGrid();
                    e.button_canceled = true;
                    return;
                }

                if (!PLCDBPanel1.IsNewRecord && !IsItemARDeleteConfirmed)
                {
                    DataSet DS = PLCSession.PLCGeneralDataSet;
                    if (DS.Tables.IndexOf("ASSIGNMENTITEMS") < 0)
                        return;

                    string itemsWithARResults = ItemsWithARResults(gItems);
                    if (!string.IsNullOrEmpty(itemsWithARResults))
                    {
                        mbARCheck.Message = "Data has been entered for the following item(s): <ul class='scrollable-popup-content'>" + itemsWithARResults + "</ul><br /><br />Are you sure you want to remove the item(s) listed from this assignment? The related data will be permanently deleted if you remove these items.";
                        mbARCheck.Show();
                        e.button_canceled = true;
                        return;
                    }
                }

                if (!PLCDBPanel1.IsNewRecord && !IsItemCancelTaskConfirmed)
                {
                    string itemsWithOpenTasks = GetItemsWithOpenTasks(gItems);
                    string itemsWithWorklist = GetItemWithWorklist(gItems);

                    if (!string.IsNullOrEmpty(itemsWithOpenTasks) && !string.IsNullOrEmpty(itemsWithWorklist))
                    {
                        mbCancelTasks.Message = "Open tasks for following item(s) will be cancelled: " + itemsWithOpenTasks + "<br /><br />" +
                            "Assigned tasks for following item(s) will be cancelled and removed from worklist: " + itemsWithWorklist + "<br /><br />Continue?";
                        mbCancelTasks.Show();
                        e.button_canceled = true;
                        return;
                    }
                    else if (!string.IsNullOrEmpty(itemsWithOpenTasks))
                    {
                        mbCancelTasks.Message = "Open tasks for following item(s) will be cancelled: " + itemsWithOpenTasks + "<br /><br />Continue?";
                        mbCancelTasks.Show();
                        e.button_canceled = true;
                        return;
                    }
                    else if (!string.IsNullOrEmpty(itemsWithWorklist))
                    {
                        mbCancelTasks.Message = "Assigned tasks for following item(s) will be cancelled and removed from worklist: " + itemsWithWorklist + "<br /><br />Continue?";
                        mbCancelTasks.Show();
                        e.button_canceled = true;
                        return;
                    }

                }

                if (PLCSession.GetLabCtrl("TRACK_ASSIGNMENT_COMMENTS") == "T" && !PLCDBPanel1.IsNewRecord &&
                    (PLCDBPanel1.GetFieldNames().Contains("COMMENTS") && PLCDBPanel1.GetOriginalValue("COMMENTS").Trim() != PLCDBPanel1.getpanelfield("COMMENTS").Trim()))
                {
                    CommentChange commChg = new CommentChange(PLCSession.PLCGlobalAssignmentKey,
                        PLCDBPanel1.getpanelfield("COMMENTS"), PLCSession.PLCGlobalAnalyst, DateTime.Now);
                    ViewState["CommentChange"] = commChg;
                }

                // Check if user needs to state reason for updating the case record
                bool hasChanges = CheckForChanges();
                string auditText = PrepareCaseNarrative();
                string reasonText = "";
                if (PLCSession.GetLabCtrl("LOG_EDITS_TO_NARRATIVE") == "T" && !PLCDBPanel1.IsNewRecord)
                {
                    if (hdnConfirmUpdate.Value.Trim().Length > 0)
                    {
                        SaveConfirmUpdate(auditText);
                        reasonText = hdnConfirmUpdate.Value;
                        hdnConfirmUpdate.Value = ""; // Clear flag to save case update changes
                    }
                    else if (hasChanges) // Changes in main dbpanel AND all details tabs
                    {
                        mInput.ShowMsg("Case update reason", PLCSession.GetSysPrompt("TAB6Assignments.UPDATE_REASON", "Please enter the reason for your changes"), 
                            0, txtConfirmUpdate.ClientID, btnConfirmUpdate.ClientID, PLCSession.GetSysPrompt("TAB6Assignments.mInput.SAVE", "Save"),
                            PLCSession.GetSysPrompt("TAB6Assignments.mInput.CANCEL", "Cancel"));
                        e.button_canceled = true;
                        return;
                    }
                }

                //if analyst has not been assigned for this new assignment 
                //and AUTO_ASSIGN_ADDTNL_SUBS flag is ON for selected section, 
                //get latest report analyst in this case and section
                if (PLCDBPanel1.IsNewRecord && PLCDBPanel1.getpanelfield("ANALYST_ASSIGNED").Trim() == "")
                {
                    string sectionCode = PLCDBPanel1.getpanelfield("SECTION");
                    string autoAssign = PLCSession.GetSectionFlag(sectionCode, "AUTO_ASSIGN_ADDTNL_SUBS").Trim().ToUpper();
                    if (sectionCode != "")
                    {
                        string reportAnalyst = "";
                        if (autoAssign == "T")
                            reportAnalyst = PLCDBGlobal.instance.LatestReportAnalyst(sectionCode, PLCSession.PLCGlobalCaseKey);
                        else if (autoAssign == "S")
                            reportAnalyst = PLCDBGlobal.instance.FirstReportAnalyst(sectionCode, PLCSession.PLCGlobalCaseKey);

                        if (reportAnalyst != "")
                        {
                            PLCDBPanel1.setpanelfield("ANALYST_ASSIGNED", reportAnalyst);
                            PLCDBPanel1.setpanelfield("STATUS", PLCDBGlobalClass.GetExamStatusCode("L", "1"));
                        }
                    }
                }

                if (hasChanges)
                {
                    PLCDBGlobalClass.MarkAssignmentsForRegeneration(PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalAssignmentKey, "LABEXAM", auditText);
                }

                if (PLCDBPanel1.HasChanges() || ItemHasChanges() || NameHasChanges())
                    PLCDBPanel1.ReasonChangeKey = PLCDBGlobalClass.GenerateReasonChange("ASSIGNMENTS TAB SAVE", reasonText);

                //if AUTO_GEN_SECTION_NUM = 'S', Add SECTION_NUMBER
                if (PLCDBPanel1.IsNewRecord && PLCSession.GetLabCtrl("AUTO_GEN_SECTION_NUM").Trim() == "S")
                {
                    if (PLCDBPanel1.HasPanelRec("TV_LABEXAM", "SECTION"))
                    {
                        string section = PLCDBPanel1.getpanelfield("SECTION").Trim();
                        if (!string.IsNullOrEmpty(section) && PLCDBPanel1.HasPanelRec("TV_LABEXAM", "SECTION_NUMBER"))
                        {
                            string sectionNumber = (new PLCHelperFunctions()).GetNextSectionNumber(section);
                            PLCDBPanel1.setpanelfield("SECTION_NUMBER", sectionNumber);
                            VerifySectionNumber = true;
                        }
                    }
                }

                UpdateAssignmentInfo();

            }

            if ((e.button_name == "SAVE") & (e.button_action == "AFTER"))
            {
                UpdateAssignedBy(PLCDBPanel1.ReasonChangeKey);
                SaveAssignmentNames(PLCSession.PLCGlobalAssignmentKey, PLCDBPanel1.ReasonChangeKey);
                SaveAssignmentItems(PLCSession.PLCGlobalAssignmentKey, PLCDBPanel1.ReasonChangeKey);
                GenerateSectionFileItem();
                if (PLCSession.GetLabCtrlFlag("USES_STATUS_HISTORY") == "T")
                    AddAssignReptStatRecord();

                if (e.row_added)
                {
                    if (PLCSession.GetLabCtrl("USES_SERVICE_REQUESTS") == "L")
                        CreateServiceRequest();

                    PLCDBGlobal.instance.UpdateAssignmentSubmissionDateUsingECNs(PLCSession.PLCGlobalAssignmentKey, GetSelectedItems());

                    PLCDBGlobal.instance.UpdateCaseStatus();

                }

                bool usesCheckPoint = PLCDBGlobal.instance.UsesCheckPoint();
                if (usesCheckPoint)
                {
                    if (PLCDBPanel1.GetOriginalValue("ANALYST_ASSIGNED").Trim() == ""
                        && PLCDBPanel1.getpanelfield("ANALYST_ASSIGNED").Trim() != "")
                    {
                        PLCDBGlobal.instance.AddAssignmentSAKHist(PLCSession.PLCGlobalAssignmentKey, CheckPointConstants.ACT_QUEUED);
                    }
                }

                int seq = e.row_added ? NewAssignSequence : PLCSession.SafeInt(GridView1.SelectedDataKey["SEQUENCE"].ToString());
                PLCDBGlobal.instance.PopulateLabCaseRelatedFields(seq, PLCSession.SafeInt(PLCSession.PLCGlobalAssignmentKey), PLCSession.PLCGlobalCaseKey);

                string caseTitleCacheKey = "CASETITLE@@@" + PLCSessionVars1.PLCGlobalCaseKey;
                CacheHelper.DeleteItem(caseTitleCacheKey);

                ((MasterPage)Master).SetCaseTitle("");
                ((MasterPage)Master).SetLabCaseHeader();

                //moved here so that autosync will not override
                if (!SaveFromTaskPopup)
                {
                    PLCDBGlobal.instance.AutoSyncTask(PLCSession.PLCGlobalAssignmentKey);
                }
                SaveFromTaskPopup = false;

                if (ViewState["AssignTasks"] != null)
                {
                    SaveAssignmentTasks((List<Task>)ViewState["AssignTasks"], PLCDBPanel1.ReasonChangeKey);
                    ViewState["AssignTasks"] = null;
                }

                if (PLCSession.GetLabCtrl("TRACK_ASSIGNMENT_COMMENTS") == "T" && ViewState["CommentChange"] != null)
                {
                    RecordCommentChange((CommentChange)ViewState["CommentChange"], PLCDBPanel1.ReasonChangeKey);
                    ViewState["CommentChange"] = null;
                }

                if (VerifySectionNumber)
                {
                    (new PLCHelperFunctions()).RecheckSectionNumber(PLCSession.PLCGlobalAssignmentKey);
                    VerifySectionNumber = false;
                }

                if (!e.row_added)
                {
                    PLCDBGlobal.instance.LockUnlockRecord("TV_LABEXAM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalAssignmentKey, "-1", false);
                    ((MasterPage)this.Master).SetCanRecordLock(string.Empty);
                }

                PLCButtonPanel1.SetCustomButtonEnabled("RecordUnlock", false);

                IsAssignAddMode = false;

                GridView1.InitializePLCDBGrid();
                GridView1.SelectRowByDataKey(PLCSession.PLCGlobalAssignmentKey);
                GridView1.SetControlMode(true);
                SetAttachmentsButton("ASSIGNMENT");
                EnableButtonControls(true, false);
                GetAnalystSectionInfo();
                EnableLink();
                SetAssignmentItemControlsMode(false, false);

                // Moved from top of control stack to bottom
                IsItemARDeleteConfirmed = false;
                IsItemCancelTaskConfirmed = false;
                //ADD_NEW_ASSIGNMENT = false;
                ReloadDBPanels();
                bpDNADates.SetBrowseMode();

                PLCDBPanel1.ReasonChangeKey = 0;
                GrabGridRecord();

                //Sync report format of TV_LABEXAM with TV_TOXREPORT.REPORT_FORMAT
                PLCQuery qryReportFormat = new PLCQuery("SELECT REPORT_FORMAT FROM TV_TOXREPORT WHERE EXAM_KEY = " + PLCSession.PLCGlobalAssignmentKey);
                qryReportFormat.Open();
                if (!qryReportFormat.IsEmpty())
                {
                    qryReportFormat.Edit();
                    qryReportFormat.SetFieldValue("REPORT_FORMAT", PLCDBPanel1.getpanelfield("REPORT_FORMAT"));
                    qryReportFormat.Post("TV_TOXREPORT", 52, 1);
                }

                //when changes are made to the assignment
                //and the assignment has already been codis reviewed (status='A'), change PROFUPLD status back to R
                if (!e.row_added)
                    PLCDBGlobal.UpdateCodisProfileStatus(PLCSession.PLCGlobalAssignmentKey, "R", "A");
                else
                {
                    string newAssignSP = PLCSession.GetSectionFlag(PLCDBPanel1.getpanelfield("SECTION").Trim(), "NEW_ASSIGNMENT_SP");
                    if (!string.IsNullOrEmpty(newAssignSP))
                    {
                        PLCDBGlobal.instance.AddDefaultPanels(PLCSession.PLCGlobalAssignmentKey, PLCDBPanel1.getpanelfield("SECTION").Trim());
                        PLCDBGlobal.instance.PrepopulatePanels(newAssignSP, PLCSession.PLCGlobalAssignmentKey, PLCDBPanel1.getpanelfield("SEQUENCE").Trim());
                    }
                }

                SetLabCaseHeader();
            }

            if ((e.button_name == "CANCEL") & (e.button_action == "AFTER"))
            {
                if (!e.row_added)
                {
                    PLCDBGlobal.instance.LockUnlockRecord("TV_LABEXAM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalAssignmentKey, "-1", false);
                    ((MasterPage)this.Master).SetCanRecordLock(string.Empty);
                }

                PLCButtonPanel1.SetCustomButtonEnabled("RecordUnlock", false);

                if (GridView1.Rows.Count > 0)
                {
                    GridView1.InitializePLCDBGrid();
                    GridView1.SelectRowByDataKey(PLCSession.PLCGlobalAssignmentKey);
                    GridView1.SetControlMode(true);
                    EnableButtonControls(true, false);
                    GrabGridRecord();
                }
                else
                    ResetControls();

                //GetAnalystSectionInfo();
                SetAssignmentItemControlsMode(false, false);

                ViewState["AssignTasks"] = null;
                ViewState["AssignNameChanges"] = null;
                ViewState["AssignItemChanges"] = null;
                IsAssignAddMode = false;
            }

            if (e.button_name == "Search Results...")
            {
                gvAssignSearch.InitializePLCDBGrid();
                gvAssignSearch.SelectedIndex = -1;
                ScriptManager.RegisterStartupScript(PLCDBPanel1, PLCDBPanel1.GetType(), "OpenDialogAssignSearchResults", "OpenDialogAssignSearchResults();", true);
            }

            ToggleResultsButton();

            if (e.button_name.ToUpper().Trim() == "MANAGE" || e.button_name.ToUpper().Trim() == "MERGE")
                Response.Redirect("AssignmentManage.aspx");

            if (e.button_name.ToUpper().Trim() == "SPLIT")
                Response.Redirect("AssignmentSplit.aspx");

            if (e.button_name.ToUpper().Trim() == "BACK TO BATCH ASSIGN")
                Response.Redirect("~/BatchAssign.aspx");

            if (e.button_name == "Revisions" || e.button_name == PLCSession.GetLabCtrl("DOC_REVISION_BTN_TEXT"))
            {
                if (!PLCSession.CheckUserOption("DOCREVIEW"))
                {
                    messageBox.ShowMsg("View Revisions", "You cannot view revisions for this assignment.", 1);
                    return;
                }

                string section = PLCDBPanel1.getpanelfield("SECTION");
                if (PLCSession.PLCGlobalAnalyst != PLCDBPanel1.getpanelfield("ANALYST_ASSIGNED") &&
                    PLCSession.GetUserAnalSect(section, "REVIEW_REPORTS").ToString() != "T" &&
                    PLCSession.GetUserAnalSect(section, "APPROVE_REPORTS").ToString() != "T")
                {
                    messageBox.ShowMsg("View Revisions", "You cannot view revisions for this assignment.", 1);
                    return;
                }

                KeepSessionAlive();
                WebOCX.OnSuccessScript = "CheckBrowserWindowID();";
                WebOCX.ShowDocumentRevisions(PLCSession.PLCGlobalAssignmentKey, "-1");
            }

            if (e.button_name == "Rev Note")
            {
                string rptName = PLCSession.FindCrystalReport("REVNOTE");
                if (!string.IsNullOrEmpty(rptName))
                {
                    PLCSession.PLCCrystalReportName = rptName;
                    PLCSession.PLCCrystalSelectionFormula = "{TV_LABEXAM.EXAM_KEY} = " + PLCSession.PLCGlobalAssignmentKey;
                    PLCSession.PLCCrystalReportTitle = "Note Revisions";
                    PLCSession.PrintCRWReport(true);
                }
                else
                {
                    messageBox.ShowMsg("Note Revisions", "REVNOTE.RPT not found.", 1);
                    return;
                }
            }

            if (e.button_name == "RecordUnlock")
            {
                mInput.ShowMsg("Unlock record", PLCSession.GetSysPrompt("TAB6Assignments.UNLOCK_REASON", "Please enter why you have to unlock the record ?"), 0, txtConfirmUnlock.ClientID, btnConfirmUnlock.ClientID);
                return;
            }

            SetBigViewButtonColor();
        }

        protected void PLCButtonPanel2_PLCButtonClick(object sender, PLCButtonClickEventArgs e)
        {
            if ((e.button_name == "SAVE") & (e.button_action == "AFTER"))
            {
                string myKeys = PLCDBCheckList1.GetSelectedKeys();
                SaveItemsToITAssign(myKeys);
            }

            if ((e.button_name == "CANCEL") & (e.button_action == "AFTER"))
            {
                LoadItems();
                LoadNames(GridView1.SelectedDataKey["EXAM_KEY"].ToString());
                GridView1.Enabled = true;
            }
        }

        protected void bpDNADates_PLCButtonClick(object sender, PLCButtonClickEventArgs e)
        {
            if ((e.button_name == "EDIT") & (e.button_action == "AFTER"))
            {
                GridView1.SetControlMode(false);
                EnableButtonControls(false, false);
                DisableLink("bnDNADates");
            }

            if ((e.button_name == "SAVE") & (e.button_action == "AFTER"))
            {
                GridView1.SetControlMode(true);
                EnableButtonControls(true, false);
                EnableLink();
                GetAnalystSectionInfo();
            }

            if ((e.button_name == "CANCEL") & (e.button_action == "AFTER"))
            {
                GridView1.SetControlMode(true);
                EnableButtonControls(true, false);
                EnableLink();
                GetAnalystSectionInfo();
            }
        }

        protected void GridView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            GrabGridRecord();

            MultiView1.ActiveViewIndex = 0;
            SetAssignmentItemControlsMode(false, false);
        }

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            HideGridViewColumns();

            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                for (int i = 0; i < e.Row.Cells.Count - 1; i++)
                {
                    try
                    {
                        //DateTime dDateAssigned = Convert.ToDateTime(DataBinder.Eval(e.Row.DataItem, "DATE_ASSIGNED").ToString());
                        DateTime dDate = Convert.ToDateTime(e.Row.Cells[i].Text);
                        e.Row.Cells[i].Text = dDate.ToShortDateString();
                    }
                    catch
                    {
                    }
                }

                // Comment out the code below. It causes problems in IE9. And slows down the page as GetInitialNumlabExam() is called for every gridview row.
                //                if (GetInitialNumLabExam() >= 20)
                //                    e.Row.Attributes.Add("onclick", "ShowPageLoading(); " + ClientScript.GetPostBackEventReference(GridView1, "Select$" + e.Row.RowIndex.ToString()) + "; return false;");
            }
        }

        protected void GridView1_Sorted(object sender, EventArgs e)
        {
            if (GridView1.SelectedIndex != 0)
            {
                GridView1.SelectedIndex = 0;
                GridView1.SetClientSideScrollToSelectedRow();
            }

            GrabGridRecord();
            MultiView1.ActiveViewIndex = 0;
            SetAssignmentItemControlsMode(false, false);
        }

        protected void gItems_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                GetSubmExtraInfoHighlight(e, gItems.DataKeys[e.Row.RowIndex].Value.ToString());
            }
        }

        protected void gvNameItem_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DataRowView row = (DataRowView)e.Row.DataItem;
                if (Convert.ToInt32(row["CHECKED"]) > 0)
                {
                    // update cbSelect_state; create a new function in dbgrid to CheckRowByIndex
                    var hdnSelectState = (HiddenField)gvNameItem.HeaderRow.FindControl("cbSelect_state");
                    var rowIndexes = hdnSelectState.Value;
                    var index = e.Row.RowIndex;
                    ((CheckBox)e.Row.FindControl("cbSelect")).Checked = true;
                    rowIndexes = rowIndexes.Remove(index, 1);
                    rowIndexes = rowIndexes.Insert(index, "1");
                    hdnSelectState.Value = rowIndexes;
                }
            }
        }

        protected void gvNameItem_Sorted(object sender, EventArgs e)
        {
            if (PLCDBPanel1.IsBrowseMode)
            {
                SetAssignmentItemControlsMode(false, false);
                SelectTab(VIEW_NAMES);
            }
        }

        protected void gvAssignTask_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                e.Row.Cells[1].Text = PLCSession.GetLabCtrl("LAB_CASE_TEXT");
            }
            else if (e.Row.RowType == DataControlRowType.DataRow)
            {
                CodeMultiPick cmpAssignTaskType = new CodeMultiPick("cmpAssignTaskType_" + e.Row.RowIndex.ToString(), "TV_TASKTYPE",
                    "TASK_TYPE", "DESCRIPTION", string.Format("SECTION = '{0}' AND (HIDE_IN_TASK_SCREEN IS NULL OR HIDE_IN_TASK_SCREEN <> 'T') AND (ACTIVE <> 'F' OR ACTIVE IS NULL)", PLCDBPanel1.getpanelfield("SECTION")), "");

                cmpAssignTaskType.UsesSearchBar = true;
                cmpAssignTaskType.HideDescriptionText = true;
                cmpAssignTaskType.HeaderPrompt = "Select Tasks";
                cmpAssignTaskType.PopupWidth = "340px";
                cmpAssignTaskType.PopupHeight = "175px";
                cmpAssignTaskType.PopupX = "195px";
                cmpAssignTaskType.PopupY = "15px";
                cmpAssignTaskType.Attributes["modal-parent"] = ".modalPopup";

                ((PlaceHolder)e.Row.FindControl("phTaskType")).Controls.Add(cmpAssignTaskType);

                if (e.Row.FindControl("fbAssignTaskPriority") != null)
                    ((FlexBox)e.Row.FindControl("fbAssignTaskPriority")).SetValue("2");
            }
        }

        protected void gvAssignTask_DataBound(object sender, EventArgs e)
        {
            CodeMultiPick cmp;
            TextBox txt;

            foreach (GridViewRow row in gvAssignTask.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    cmp = (CodeMultiPick)row.FindControl("cmpAssignTaskType_" + row.RowIndex);
                    if (cmp != null)
                    {
                        if (row.RowIndex == 0)
                        {
                            cmp.Focus();
                            hdnTaskListFocused.Value = cmp.ID;
                        }

                        txt = cmp.GetTextBox();
                        txt.Attributes.Add("onfocus", "javascript:saveFocusedTaskList('" + cmp.ID + "','" + hdnTaskListFocused.ClientID + "');");

                    }
                }
            }
        }

        protected void gvAssignSearch_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                int chkboxIndex = -1;
                int indexCounter = 1; // Start with '1' to consider hidden select field
                string dataField = string.Empty;

                foreach (DataControlField field in gvAssignSearch.Columns)
                {
                    if (field.GetType() == typeof(CheckBoxField))
                    {
                        chkboxIndex = indexCounter;
                    }
                    indexCounter++;
                }

                if (chkboxIndex > -1)
                {
                    e.Row.Cells[chkboxIndex].Style.Value = "display: none";
                    ViewState["ChkboxIndex"] = chkboxIndex;
                }
            }
            else if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (ViewState["ChkboxIndex"] != null)
                {
                    e.Row.Cells[Convert.ToInt32(ViewState["ChkboxIndex"])].Style.Value = "display: none";
                }
            }
        }

        protected void gvAssignSearch_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (UserTabRestricted())
            {
                return;
            }

            string caseKey = gvAssignSearch.SelectedDataKey["CASE_KEY"].ToString();

            // check if user has access to case
            string msg = string.Empty;
            if (!PLCDBGlobalClass.IsUserHasAccessToCase(caseKey, PLCSession.PLCGlobalAnalyst, out msg))
            {
                dlgMessageSearchResults.ShowAlert("Case Search", msg);
                return;
            }

            if (!PLCDBGlobalClass.CheckCaseLock(caseKey, PLCSession.PLCGlobalAnalyst, PLCSession.PLCGlobalAnalystDepartmentCode))
            {
                string examKey = gvAssignSearch.SelectedDataKey["EXAM_KEY"].ToString();
                LoadCaseByEK(examKey);
            }
            else
            {
                string message = PLCDBGlobalClass.GetCaseLockMessage(caseKey);
                if (string.IsNullOrEmpty(message))
                    message = "Case is locked.";

                dlgMessageSearchResults.ShowAlert("Assignment Search", message);
                return;
            }
        }

        protected void gvAssignSearch_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName.ToUpper() == "GOTOASSIGNMENT")
            {
                if (UserTabRestricted())
                {
                    return;
                }

                int index = Convert.ToInt32(e.CommandArgument);
                string caseKey = gvAssignSearch.DataKeys[index].Values["CASE_KEY"].ToString();

                // check if user has access to case
                string msg = string.Empty;
                if (!PLCDBGlobalClass.IsUserHasAccessToCase(caseKey, PLCSession.PLCGlobalAnalyst, out msg))
                {
                    dlgMessageSearchResults.ShowAlert("Case Search", msg);
                    return;
                }

                if (!PLCDBGlobalClass.CheckCaseLock(caseKey, PLCSession.PLCGlobalAnalyst, PLCSession.PLCGlobalAnalystDepartmentCode))
                {
                    string examKey = gvAssignSearch.DataKeys[index].Values["EXAM_KEY"].ToString();
                    LoadCaseByEK(examKey);
                }
                else
                {
                    string message = PLCDBGlobalClass.GetCaseLockMessage(caseKey);
                    if (string.IsNullOrEmpty(message))
                        message = "Case is locked.";

                    dlgMessageSearchResults.ShowAlert("Assignment Search", message);
                    return;
                }
            }
        }

        protected void ReportPreview_AcceptClick(object sender, EventArgs e)
        {
            string reviewType = string.Empty;
            SavePreviewOnAccept(out reviewType);
            ProcessReview(reviewType);
        }

        #region QMS

        protected void bnQMS_Click(object sender, EventArgs e)
        {
            string section = PLCDBGlobal.instance.GetLabAssignField(PLCSession.PLCGlobalAssignmentKey, "SECTION");
            string dateAssigned = PLCDBGlobal.instance.GetLabAssignField(PLCSession.PLCGlobalAssignmentKey, "DATE_ASSIGNED");

            DateTime dtAssigned = dateAssigned == "" ? DateTime.Now : Convert.ToDateTime(dateAssigned);

            this.lblQMSSection.Text = section;
            this.lblQMSAsOf.Text = DateTime.Now.ToString(PLCSession.GetDateFormat());

            BindQMSExamDocsGrid(section, dtAssigned);

            if (PLCDBGlobal.instance.IsResetAssignment(PLCSession.PLCGlobalAssignmentKey))
            {
                this.qmsrptdocssection.Attributes["style"] = "display:block;";
                BindQMSRptDocsGrid(PLCSession.PLCGlobalAssignmentKey, section);
            }
            else
            {
                this.qmsrptdocssection.Attributes["style"] = "display:none;";
            }

            this.hdnSelectedQMSGrid.Value = "";
            this.dbgQMSExamDocs.SelectedIndex = -1;
            this.dbgQMSRptDocs.SelectedIndex = -1;
            SetBtnQMSDocOpenState(false);
            ShowQMSDocsPopup();
        }

        protected void btnQMSDocOpen_Click(object sender, EventArgs e)
        {
            // Open the QMS doc selected from the active grid.
            PLCDBGrid activegrid;
            if (this.hdnSelectedQMSGrid.Value == "examdocs")
                activegrid = this.dbgQMSExamDocs;
            else if (this.hdnSelectedQMSGrid.Value == "rptdocs")
                activegrid = this.dbgQMSRptDocs;
            else
                activegrid = null;

            if (activegrid != null)
            {
                WebOCX.OnSuccessScript = "CheckBrowserWindowID();";
                WebOCX.OpenVersionWM(activegrid.SelectedDataKey["DOCUMENT_KEY"].ToString(), activegrid.SelectedDataKey["VERSION_KEY"].ToString());
            }
        }

        protected void dbgQMSExamDocs_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Set examdocs as the active grid and unselect the other grid.
            this.hdnSelectedQMSGrid.Value = "examdocs";
            this.dbgQMSRptDocs.SelectedIndex = -1;
            this.dbgQMSExamDocs.SetClientSideScrollToSelectedRow();
            SetBtnQMSDocOpenState(true);
        }

        protected void dbgQMSExamDocs_Sorting(object sender, EventArgs e)
        {
            if (dbgQMSExamDocs.SelectedIndex > -1)
            {
                this.dbgQMSExamDocs.SelectedIndex = 0;
                this.dbgQMSExamDocs.SetClientSideScrollToSelectedRow();
                SetBtnQMSDocOpenState(true);
            }
        }

        protected void dbgQMSRptDocs_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Set rptdocs as the active grid and unselect the other grid.
            this.hdnSelectedQMSGrid.Value = "rptdocs";
            this.dbgQMSExamDocs.SelectedIndex = -1;
            this.dbgQMSRptDocs.SetClientSideScrollToSelectedRow();
            SetBtnQMSDocOpenState(true);
        }

        protected void dbgQMSRptDocs_Sorting(object sender, EventArgs e)
        {
            if (dbgQMSRptDocs.SelectedIndex > -1)
            {
                this.dbgQMSRptDocs.SelectedIndex = 0;
                this.dbgQMSRptDocs.SetClientSideScrollToSelectedRow();
                SetBtnQMSDocOpenState(true);
            }
        }

        /// <summary>
        /// Opens the QMS popup
        /// </summary>
        private void ShowQMSDocsPopup()
        {
            ScriptManager.RegisterStartupScript(bnQMS, bnQMS.GetType(), bnQMS.ClientID + "_OpenDialog", "OpenDialogQMSDocuments();", true);
        }

        /// <summary>
        /// Sets the QMS Open Document button state to Enable or Disable
        /// </summary>
        /// <param name="enable">Set to 'true' if Open Document button should be enabled</param>
        private void SetBtnQMSDocOpenState(bool enable)
        {
            ScriptManager.RegisterStartupScript(btnQMSDocOpen, btnQMSDocOpen.GetType(), btnQMSDocOpen.ClientID + "_Enable", "setTimeout(function () { SetBtnQMSDocOpenState(" + enable.ToString().ToLower() + "); }, 0);", true);
        }

        #endregion

        #region LocalFileDirectory

        protected void btnOpenFileLink_Click(object sender, EventArgs e)
        {
            PLCQuery qryLinkPath = new PLCQuery();
            string[] linkdIDS = lbRawData.SelectedValue.Split('|');

            qryLinkPath.SQL = string.Format("SELECT DATA_FILE_PATH FROM TV_EXAMIRAW WHERE SEQUENCE = '{0}' AND IMAGE_ID = '{1}'", linkdIDS[0], linkdIDS[1]);
            qryLinkPath.Open();

            if (!qryLinkPath.IsEmpty())
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "key:" + DateTime.Now.ToString(), "openFile(\"" + qryLinkPath.FieldByName("DATA_FILE_PATH").Trim().Replace('\\', '/') + "\");", true);
            }
        }

        protected void btnCancelSaveLink_Click(object sender, EventArgs e)
        {
            mpeBrowseFile.Hide();
        }

        #endregion

        #region Routing

        protected void bnRouting_Click(object sender, EventArgs e)
        {
            if (GridView1.Rows.Count == 0)
                return;

            LoadRoutingInfo(true);
        }

        protected void btnRoutingReply_Click(object sender, EventArgs e)
        {
            ClearRoutingValues(true);
            //mpeRouting.OnOkScript = "javascript:document.getElementById('" + btnRoutingOK.ClientID + "').click();";
            ShowRoutingPopup();
        }

        protected void btnRoute_Click(object sender, EventArgs e)
        {
            ClearRoutingValues(false);
            ShowRoutingPopup();
        }

        protected void btnRouteOK_Click(object sender, EventArgs e)
        {
            string RouteTo = chUserID.GetValue();
            string RoutingCode = chRoutingCodes.GetValue();
            string RouteMessage = tbxRouteMessage.Text;

            lblRouteRequired.Visible = string.IsNullOrEmpty(RouteTo) || string.IsNullOrEmpty(RoutingCode);
            if (lblRouteRequired.Visible)
            {
                ShowRoutingPopup();
                return;
            }

            if (IsResetRoutedAssignment(RoutingCode)
                && LastRoutingDialog < RoutingDialogKey.ResetAssignment)
            {
                dlgMessage.DialogKey = RoutingDialogKey.ResetAssignment.ToString();
                dlgMessage.IconType = "alert";
                dlgMessage.ShowYesNoOnly("Route This Assignment", "Using this route code will reset the assignment and the associated service request. Are you sure you want to continue?");
                return;
            }

            PLCQuery qryRouteTo = new PLCQuery();
            qryRouteTo.SQL = "SELECT ROUTE_TO, ROUTE_CODE, ROUTE_DATE, ROUTE_MESSAGE, ROUTE_BY FROM TV_LABEXAM WHERE EXAM_KEY=" +
                GridView1.SelectedDataKey.Values["EXAM_KEY"].ToString();
            qryRouteTo.Open();
            if (!qryRouteTo.IsEmpty())
            {
                qryRouteTo.Edit();
                qryRouteTo.SetFieldValue("ROUTE_TO", RouteTo);
                qryRouteTo.SetFieldValue("ROUTE_CODE", RoutingCode);
                qryRouteTo.SetFieldValue("ROUTE_MESSAGE", RouteMessage);
                qryRouteTo.SetFieldValue("ROUTE_DATE", DateTime.Now);
                qryRouteTo.SetFieldValue("ROUTE_BY", PLCSession.PLCGlobalAnalyst);
                qryRouteTo.Post("TV_LABEXAM", 17, 1);
            }

            int RouteKey = PLCSession.GetNextSequence("ROUTE_SEQ");
            qryRouteTo = new PLCQuery();
            qryRouteTo.SQL = "SELECT * FROM TV_EXAMROUT WHERE 0=1";
            qryRouteTo.Open();
            if (qryRouteTo.IsEmpty())
            {
                qryRouteTo.Append();
                qryRouteTo.SetFieldValue("ROUTE_KEY", RouteKey);
                qryRouteTo.SetFieldValue("EXAM_KEY", GridView1.SelectedDataKey.Values["EXAM_KEY"].ToString());
                qryRouteTo.SetFieldValue("ROUTE_TO", RouteTo);
                qryRouteTo.SetFieldValue("ROUTED_BY", PLCSession.PLCGlobalAnalyst);
                qryRouteTo.SetFieldValue("ROUTE_DATE", DateTime.Now);
                qryRouteTo.SetFieldValue("ROUTE_CODE", RoutingCode);
                qryRouteTo.SetFieldValue("ROUTE_MESSAGE", RouteMessage);
                qryRouteTo.Post("TV_EXAMROUT", -1, -1);
            }

            UpdateRoutedAssignmentInfo(RoutingCode);
            PLCDBPanel1.DoLoadRecord();
            LoadRoutingInfo(false);

            // reset dialog confirmations
            LastRoutingDialog = RoutingDialogKey.None;
        }


        private bool IsResetRoutedAssignment(string routingCode)
        {
            var qry = new PLCQuery();
            qry.SQL = "SELECT RESET_ROUTED_ASSIGNMENT FROM TV_ROUTECOD WHERE ROUTING_CODE = ?";
            qry.AddSQLParameter("ROUTING_CODE", routingCode);
            qry.OpenReadOnly();
            return qry.HasData() && qry.FieldByName("RESET_ROUTED_ASSIGNMENT").ToUpper() == "T";
        }

        private void UpdateRoutedAssignmentInfo(string routingCode)
        {
            PLCQuery qryRoutecod = new PLCQuery();
            qryRoutecod.SQL = String.Format("SELECT RESET_ROUTED_ASSIGNMENT FROM TV_ROUTECOD WHERE ROUTING_CODE = '{0}'", routingCode);
            qryRoutecod.Open();
            if ((!qryRoutecod.IsEmpty()) && (qryRoutecod.FieldByName("RESET_ROUTED_ASSIGNMENT").ToUpper() == "T"))
            {
                PLCQuery qryLabexam = new PLCQuery();
                qryLabexam.SQL = String.Format("SELECT STATUS, SIGNED_BY, DRAFT_DATE, DOCUMENT_PATH, ANALYST_ASSIGNED, EXAM_KEY,REVIEWED_BY, REVIEWED_DATE, REVIEWED FROM TV_LABEXAM WHERE EXAM_KEY = '{0}'", PLCSessionVars1.PLCGlobalAssignmentKey);
                qryLabexam.Open();
                if (!qryLabexam.IsEmpty())
                {
                    qryLabexam.Edit();
                    qryLabexam.SetFieldValue("STATUS", PLCDBGlobalClass.GetExamStatusCode("S", "0"));
                    qryLabexam.SetFieldValue("ANALYST_ASSIGNED", "");
                    qryLabexam.SetFieldValue("SIGNED_BY", "");
                    qryLabexam.SetFieldValue("DOCUMENT_PATH", "");
                    qryLabexam.SetFieldValue("DRAFT_DATE", "");
                    qryLabexam.SetFieldValue("REVIEWED_BY", "");
                    qryLabexam.SetFieldValue("REVIEWED_DATE", "");
                    qryLabexam.SetFieldValue("REVIEWED", "");
                    qryLabexam.Post("TV_LABEXAM", 17, 1);

                    PLCDBGlobal.instance.DeleteWordData(PLCSessionVars1.PLCGlobalAssignmentKey);
                    PLCDBGlobal.instance.RemoveAssignmentSignature(PLCSession.PLCGlobalAssignmentKey);
                    PLCDBGlobal.instance.ResetChecklists(PLCSession.PLCGlobalAssignmentKey, false, "APPROVE");
                    PLCDBGlobal.instance.ResetChecklists(PLCSession.PLCGlobalAssignmentKey, false, "REVIEW");
                    PLCDBGlobal.instance.ResetChecklists(PLCSession.PLCGlobalAssignmentKey, false, "SIGNED");
                }

                PLCQuery qryTaskList = new PLCQuery();
                qryTaskList.SQL = String.Format("SELECT STATUS FROM TV_TASKLIST WHERE EXAM_KEY = '{0}'", PLCSessionVars1.PLCGlobalAssignmentKey);
                qryTaskList.Open();
                if (!qryTaskList.IsEmpty())
                {
                    while (!qryTaskList.EOF())
                    {
                        qryTaskList.Edit();
                        qryTaskList.SetFieldValue("STATUS", "O");
                        qryTaskList.Post("TV_TASKLIST", 52, 1);
                        qryTaskList.Next();
                    }
                }

                PLCQuery qrySRDetail = new PLCQuery();
                qrySRDetail.SQL = String.Format("SELECT SR_MASTER_KEY, SR_DETAIL_KEY, STATUS_DATE, STATUS_CODE, EXAM_KEY FROM TV_SRDETAIL WHERE EXAM_KEY = '{0}' ", PLCSessionVars1.PLCGlobalAssignmentKey);
                qrySRDetail.Open();
                if (!qrySRDetail.IsEmpty())
                {
                    string masterKey = qrySRDetail.FieldByName("SR_MASTER_KEY");
                    while (!qrySRDetail.EOF())
                    {
                        qrySRDetail.Edit();
                        qrySRDetail.SetFieldValue("STATUS_CODE", "1");
                        qrySRDetail.Post("TV_SRDETAIL", 3000, 11);


                        int historyKey = PLCSession.GetNextSequence("SRHIST_SEQ");
                        PLCQuery qrySRHistory = new PLCQuery();
                        qrySRHistory.SQL = "SELECT * FROM TV_SRHIST WHERE 0 = 1";
                        qrySRHistory.Open();
                        qrySRHistory.Append();
                        qrySRHistory.SetFieldValue("SR_HISTORY_KEY", historyKey);
                        qrySRHistory.SetFieldValue("SR_MASTER_KEY", qrySRDetail.FieldByName("SR_MASTER_KEY"));
                        qrySRHistory.SetFieldValue("SR_DETAIL_KEY", qrySRDetail.FieldByName("SR_DETAIL_KEY"));
                        qrySRHistory.SetFieldValue("EXAM_KEY", PLCSessionVars1.PLCGlobalAssignmentKey);
                        qrySRHistory.SetFieldValue("STATUS_CODE", "1");
                        qrySRHistory.SetFieldValue("STATUS_DATE", qrySRDetail.FieldByName("STATUS_DATE"));
                        qrySRHistory.SetFieldValue("STATUS_BY", PLCSessionVars1.PLCGlobalAnalyst);
                        qrySRHistory.SetFieldValue("COMMENTS", "Reset routed assignment");
                        qrySRHistory.Post("TV_SRHIST", 7000, 62);

                        qrySRDetail.Next();
                    }

                    UpdateSRMasterStatus(masterKey); 
                }

                GridView1.InitializePLCDBGrid();
                GridView1.SelectRowByDataKey(PLCSessionVars1.PLCGlobalAssignmentKey);
                GrabGridRecord();
            }   
        }

        private void UpdateSRMasterStatus(string masterKey)
        {
            PLCQuery qrySRDetailRecord = new PLCQuery();
            qrySRDetailRecord.SQL = String.Format("SELECT COUNT(*) AS DETAIL_COUNT FROM TV_SRDETAIL WHERE SR_MASTER_KEY ='{0}'", masterKey);
            qrySRDetailRecord.Open();
            if (!qrySRDetailRecord.IsEmpty())
            {
                int detailCount = qrySRDetailRecord.iFieldByName("DETAIL_COUNT");
                PLCQuery qrySRDetailStat = new PLCQuery();
                qrySRDetailStat.SQL = String.Format("SELECT COUNT(*) AS STAT_COUNT FROM TV_SRDETAIL WHERE SR_MASTER_KEY ='{0}' AND STATUS_CODE = '{1}'", masterKey, "1");
                qrySRDetailStat.Open();
                if (!qrySRDetailStat.IsEmpty())
                {         
                    int statCount = qrySRDetailStat.iFieldByName("STAT_COUNT");
                    if (detailCount == statCount)
                    {
                        PLCQuery qrySRMaster = new PLCQuery();
                        qrySRMaster.SQL = String.Format("SELECT SR_MASTER_KEY, STATUS, STATUS_BY FROM TV_SRMASTER WHERE SR_MASTER_KEY = '{0}'", masterKey);
                        qrySRMaster.Open();
                        if (!qrySRMaster.IsEmpty())
                        {
                            qrySRMaster.Edit();
                            qrySRMaster.SetFieldValue("STATUS", "1");
                            qrySRMaster.Post("TV_SRMASTER", 3000, 11);
                        }
                    }
                }
            }
        }

        protected void btnRoutingCancel_Click(object sender, EventArgs e)
        {
            PLCQuery qryClearRouting = new PLCQuery();
            qryClearRouting.SQL = "SELECT ROUTE_TO, ROUTE_CODE, ROUTE_DATE, ROUTE_MESSAGE, ROUTE_BY FROM TV_LABEXAM WHERE EXAM_KEY=" +
                GridView1.SelectedDataKey.Values["EXAM_KEY"].ToString();
            qryClearRouting.Open();
            if (!qryClearRouting.IsEmpty())
            {
                qryClearRouting.Edit();
                qryClearRouting.SetFieldValue("ROUTE_TO", "");
                qryClearRouting.SetFieldValue("ROUTE_CODE", "");
                qryClearRouting.SetFieldValue("ROUTE_MESSAGE", "");
                qryClearRouting.SetFieldValue("ROUTE_DATE", "");
                qryClearRouting.SetFieldValue("ROUTE_BY", "");
                qryClearRouting.Post("TV_LABEXAM", 17, 1);

                int examKey = PLCSession.SafeInt(PLCSession.PLCGlobalAssignmentKey);
                var qry = new PLCQuery();
                qry.SQL = "SELECT ROUTE_KEY, ROUTE_CANCELED FROM TV_EXAMROUT "
                    + "WHERE EXAM_KEY = " + examKey + " ORDER BY ROUTE_KEY DESC";
                qry.Open();
                if (qry.HasData())
                {
                    qry.Edit();
                    qry.SetFieldValue("ROUTE_CANCELED", "T");
                    qry.Post("TV_EXAMROUT", -1, -1);
                }
            }

            LoadRoutingInfo(false);
        }

        protected void btnRoutingHistory_Click(object sender, EventArgs e)
        {
            //string selectionFormula = "{ALL_ASSIGNMENTS.SEQUENCER} = ";
            string selectionFormula = "{TV_EXAMROUT.EXAM_KEY} = " + GridView1.SelectedDataKey.Values["EXAM_KEY"].ToString();
            PLCSession.WriteDebug("Report: Routing History; FileName: EXAMROUT; Parameters: " + selectionFormula, true);

            PLCSession.PLCCrystalReportName = PLCSession.FindCrystalReport("EXAMROUT.rpt");
            PLCSession.PLCCrystalReportTitle = "Routing History";
            PLCSession.PLCCrystalSelectionFormula = selectionFormula;
            PLCSession.PrintCRWReport(false);
        }

        protected void bnAssignRoute_Click(object sender, EventArgs e)
        {
            if (!PLCDBGlobal.instance.AssignmentSignedAlready(PLCSession.PLCGlobalAssignmentKey, "SIGNED"))
            {
                List<int> lstAssign = new List<int>();
                lstAssign.Add(Convert.ToInt32(GridView1.SelectedDataKey.Values["EXAM_KEY"].ToString()));

                AssignRoute1.RouteCode = PLCSession.GetLabCtrl("ASSIGNMENT_ROUTING_CODE");
                AssignRoute1.RouteAssignments = lstAssign;
                AssignRoute1.Show();
            }
            else
                messageBox.ShowMsg("Assign and Route", "This assignment already has report signed.", 1);
        }

        protected void AssignRouteSave_Click(object sender, EventArgs e)
        {
            int selIndex = GridView1.SelectedIndex;
            GridView1.InitializePLCDBGrid();
            GridView1.SelectedIndex = selIndex;
            GrabGridRecord();

            LoadRoutingInfo(false);
        }

        protected void dbgRoutingHistory_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (!dbgRoutingHistory.HasHDConfig())
            {
                if (e.Row.RowType == DataControlRowType.Header)
                {
                    // Update header text
                    e.Row.Cells[1].Text = "Route Date";
                    e.Row.Cells[2].Text = "Route To";
                    e.Row.Cells[3].Text = "Route Code";
                    e.Row.Cells[4].Text = "Message";
                    e.Row.Cells[5].Text = "Routed By";
                }
            }
        }

        protected void bnSRPrint_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            SetServiceRequestButtonColor(checkRpt: PLCSession.GetLabCtrlFlag("ASSIGN_SR_BTN_RED_BASED_ON_RPT") == "T", checkData: false);
        }

        #endregion

        #region Additional Analysts

        protected void fbSection_ValueChanged(object sender, EventArgs e)
        {
            txtFindAnalyst.Text = "";
            BindSectionAnalysts(fbSection.SelectedValue, null);
        }

        protected void txtFindAnalyst_TextChanged(object sender, EventArgs e)
        {
            BindSectionAnalysts(fbSection.SelectedValue, txtFindAnalyst.Text);
        }

        protected void bnAnalysts_Click(object sender, EventArgs e)
        {
            MultiView1.ActiveViewIndex = 6;

            string section = PLCDBPanel1.getpanelfield("SECTION");
            fbSection.SelectedValue = section;
            BindSectionAnalysts(section, null);

            BindAssignmentAnalysts();
        }

        protected void btnAddAnalyst_Click(object sender, EventArgs e)
        {
            //check if primary analyst is assigned
            if (PLCDBPanel1.getpanelfield("ANALYST_ASSIGNED") == string.Empty)
            {
                mbNoAnalyst.Show();
                return;
            }

            //allow edit if current user has edit-assignment permissions or current user is the primary analyst
            if (!PLCSession.CheckUserOption("EDITASSGN") && !PLCDBPanel1.getpanelfield("ANALYST_ASSIGNED").Trim().Equals(PLCSession.PLCGlobalAnalyst))
            {
                mbNotAllowed.Show();
                return;
            }

            //open analyst popup
            mpeSA.Show();
        }

        protected void btnRemoveAnalyst_Click(object sender, EventArgs e)
        {
            //allow edit if current user has edit-assignment permissions or current user is the primary analyst
            if (!PLCSession.CheckUserOption("EDITASSGN") && !PLCDBPanel1.getpanelfield("ANALYST_ASSIGNED").Trim().Equals(PLCSession.PLCGlobalAnalyst))
            {
                mbEAnalysts.ShowMessage("Remove Analyst", "You are not allowed to change this assignment.", MessageBoxType.Error);
                return;
            }

            //check if there is a selected item
            if (lstAnalysts.SelectedItem == null)
            {
                mbEAnalysts.ShowMessage("Remove Analyst", "Please select an analyst to remove.", MessageBoxType.Error);
                return;
            }

            //confirm
            mbCAnalysts.ShowMessage("Confirm Remove Analyst", "Would you like to remove the analyst?", MessageBoxType.Confirmation);
        }

        protected void btnOKSA_Click(object sender, EventArgs e)
        {
            if (grdSA.SelectedIndex == -1)
            {
                mbEAnalysts.ShowMessage("Add Analyst", "Please select an analyst.", MessageBoxType.Error);
                mpeSA.Show();
                return;
            }

            string selectedName = grdSA.SelectedRow.Cells[0].Text;
            string selectedCode = grdSA.SelectedRow.Cells[1].Text;
            string examKey = PLCSession.PLCGlobalAssignmentKey;
            string primaryVar = (PLCSession.PLCDatabaseServer.Equals("MSSQL") ? "'PRIMARY'" : "PRIMARY");

            string sql = string.Format(@"SELECT * FROM (SELECT ANALYST_ASSIGNED AS ANALYST, 1 AS {2} FROM TV_LABEXAM WHERE EXAM_KEY = {0}
    UNION SELECT ANALYST, 0 AS {2} FROM TV_EXAMANAL WHERE EXAM_KEY = {0}) A
WHERE ANALYST = '{1}'", examKey, selectedCode, primaryVar);
            PLCQuery query = new PLCQuery(sql);
            query.Open();
            if (query.IsEmpty())
            {
                PLCQuery qryAddAnalyst = new PLCQuery(string.Format("SELECT * FROM TV_EXAMANAL WHERE EXAM_KEY = {0}", examKey));
                qryAddAnalyst.Open();
                int count = qryAddAnalyst.PLCDataTable.Rows.Count;
                qryAddAnalyst.Append();
                qryAddAnalyst.SetFieldValue("EXAM_KEY", examKey);
                qryAddAnalyst.SetFieldValue("ANALYST", selectedCode);
                qryAddAnalyst.SetFieldValue("ORDER_RES", count + 1);
                qryAddAnalyst.Post("TV_EXAMANAL", 309, 1);
            }
            else
            {
                bool isPrimary = query.FieldByName("PRIMARY") == "1";
                string message = isPrimary
                    ? "Analyst: {0} is the primary analyst for this report."
                    : "Analyst: {0} is already in the Additional Analyst list.";
                message = string.Format(message, selectedName);
                mbEAnalysts.ShowMessage("Add Analyst", message, MessageBoxType.Error);
                BindAnalystsWithNoFilterByName();
                return;
            }

            mpeSA.Hide();
            BindAnalystsWithNoFilterByName();
            BindAssignmentAnalysts();
        }

        protected void mbCAnalysts_OkClick(object sender, EventArgs e)
        {
            MessageBox messageBox = (MessageBox)sender;
            if (messageBox.Caption == "Confirm Remove Analyst")
            {
                //delete record
                string analystCode = lstAnalysts.SelectedValue;
                string examKey = PLCSession.PLCGlobalAssignmentKey;
                PLCQuery qryDelete = new PLCQuery();
                qryDelete.Delete("TV_EXAMANAL", string.Format("WHERE EXAM_KEY = {0} AND ANALYST = '{1}'", examKey, analystCode), 309, 3);

                //rebind list
                BindAssignmentAnalysts();
            }
        }

        protected void btnCancelSA_OkClick(object sender, EventArgs e)
        {
            BindAnalystsWithNoFilterByName();
        }

        private void BindAnalystsWithNoFilterByName()
        {
            txtFindAnalyst.Text = "";
            BindSectionAnalysts(fbSection.SelectedValue, txtFindAnalyst.Text);
        }

        #endregion

        #region Data

        protected void bnData_Click(object sender, EventArgs e)
        {
            if (GridView1.SelectedDataKey != null)
            {
                LoadRawData();
            }
            else
            {
                bnDataAdd.Enabled = false;
                bnDataOpen.Enabled = false;
                bnDataEdit.Enabled = false;
                bnDataRemove.Enabled = false;
                MultiView1.ActiveViewIndex = 4;
            }
        }

        protected void lbRawData_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetAttachmentKey();
        }

        protected void bnDataAdd_Click(object sender, EventArgs e)
        {
            //txtDataGetFileDesc.Text = "";
            txtLinkDescription.Text = "";
            txtLinkPath.Text = "";
            btnDataSaveOK.CommandName = "ADD";
            //btnDataSave.Text = "Add Link";
            //lblDataGetFileTitle.Text = "Select a File or Folder to link";
            //lblDataGetFilePath.Visible = true;
            //fupDataGetFilePath.Visible = true;
            ////mpeDataGetFile.OnOkScript = "javascript:document.getElementById('" + btnDataSaveOK.ClientID + "').click();";
            //mpeDataGetFile.Show();

            ScriptManager.RegisterStartupScript(this, this.GetType(), DateTime.Now.ToString(), "LoadDrive();", true);
            mpeBrowseFile.Show();
        }

        protected void bnDataRemove_Click(object sender, EventArgs e)
        {
            if (lbRawData.SelectedIndex > -1)
            {
                PLCQuery qryRawData = new PLCQuery();

                //qryRawData.SQL = "DELETE FROM TV_EXAMIRAW WHERE EXAM_KEY = " + GridView1.SelectedDataKey.Values["EXAM_KEY"].ToString() +
                //    " AND SEQUENCE = " + lbRawData.SelectedItem.Value.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries)[0];
                //qryRawData.ExecSQL("");

                // Changed to Delete to insert code and sub_code for auditlog
                qryRawData.Delete("TV_EXAMIRAW", "WHERE EXAM_KEY = " + GridView1.SelectedDataKey.Values["EXAM_KEY"].ToString() +
                    " AND SEQUENCE = " + lbRawData.SelectedItem.Value.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries)[0], 103, 3);

                SetAttachmentKey();
                DeleteAttachment();
                LoadRawData();
            }
        }

        protected void bnDataEdit_Click(object sender, EventArgs e)
        {
            if (lbRawData.SelectedIndex > -1)
            {
                PLCQuery qryRawData = new PLCQuery();
                qryRawData.SQL = "SELECT DESCRIPTION FROM TV_EXAMIRAW WHERE EXAM_KEY = " + GridView1.SelectedDataKey.Values["EXAM_KEY"].ToString() +
                    " AND SEQUENCE = " + lbRawData.SelectedItem.Value.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries)[0];
                if (qryRawData.Open() && qryRawData.HasData())
                {
                    txtDataGetFileDesc.Text = qryRawData.FieldByName("DESCRIPTION");
                    btnDataSaveOK.CommandName = "EDIT";
                    btnDataSave.Text = PLCSession.GetSysPrompt("TAB6Assignments.btnDataSave.SAVE", "Save");
                    lblDataGetFileTitle.Text = PLCSession.GetSysPrompt("TAB6Assignments.lblDataGetFileTitle.NEW_DESC", "Enter a new description");
                    lblDataGetFilePath.Visible = false;
                    fupDataGetFilePath.Visible = false;
                    mpeDataGetFile.Show();
                }
            }
        }

        protected void bnDataSave_Click(object sender, EventArgs e)
        {
            bool validSave = true;

            if (btnDataSaveOK.CommandName == "ADD")
            {
                string filePath = txtLinkPath.Text; //this.hdnPath.Value;
                //string filePath = fupDataGetFilePath.FileName;

                //if (!string.IsNullOrEmpty(filePath.Trim()))
                if (!string.IsNullOrEmpty(filePath.Trim()) && !filePath.Equals("0") && !filePath.Equals(""))
                {
                    foreach (ListItem item in lbRawData.Items)
                    {
                        string value = GetProcessedValue(item.Text);
                        if (filePath.ToUpper().Trim().Equals(value.ToUpper().Trim()))
                        {
                            validSave = false;
                            messageBox.ShowMsg("Duplicate Data", "The file is already present in the list.", 1);
                            break;
                        }
                    }
                }
                else
                {
                    validSave = false;
                    messageBox.ShowMsg("No Data", "File Path must have a value.", 1);
                }

                if (validSave)
                {
                    PLCQuery qryLabExam = QuerySelectedLabExam();
                    if (qryLabExam.HasData())
                    {
                        //if (PLCSession.CheckSectionFlag(qryLabExam.FieldByName("SECTION"), "RAW_DATA_TO_DB"))
                        //{
                        //    if (UploadRawDataFile())
                        //    {
                        //        SaveExamRawData(SaveRawDataToDatabase(), filePath, txtDataGetFileDesc.Text);
                        //    }
                        //}
                        //else
                        //{
                        //    SaveExamRawData(0, fupDataGetFilePath.PostedFile.FileName, txtDataGetFileDesc.Text);
                        //}
                        SaveExamRawData(0, filePath, txtLinkDescription.Text);
                    }
                }
            }
            else if (btnDataSaveOK.CommandName == "EDIT")
            {
                PLCQuery qryRawData = new PLCQuery();
                qryRawData.SQL = "SELECT * FROM TV_EXAMIRAW WHERE EXAM_KEY = " + GridView1.SelectedDataKey.Values["EXAM_KEY"].ToString() +
                    " AND SEQUENCE = " + lbRawData.SelectedItem.Value.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries)[0];
                if (qryRawData.Open() && qryRawData.HasData())
                {
                    qryRawData.Edit();
                    qryRawData.SetFieldValue("DESCRIPTION", txtDataGetFileDesc.Text);
                    qryRawData.Post("TV_EXAMIRAW", 103, 2);
                }
            }

            txtDataGetFileDesc.Text = "";
            LoadRawData();
            mpeDataGetFile.Hide();
        }

        #endregion

        #region Big View Items

        protected void bnBigViewCancel_Click(object sender, EventArgs e)
        {
            SetControlEdit(true);
            LinkedCases = string.Empty;
        }

        protected void btnCancelprint_Click(object sender, EventArgs e)
        {
            string isEditClicked = (string)Session["IsEditClicked"];
            if (isEditClicked == null)
            {
                //                btnPullLIst.Enabled = true;
                //                btnLinkCases.Enabled = true;
            }
            else
            {
                PLCButtonPanel1.DisableButton("EDIT");
                PLCButtonPanel1.DisableButton("ADD");
                PLCButtonPanel1.DisableButton("DELETE");
                PLCButtonPanel1.EnableButton("SAVE");
                PLCButtonPanel1.EnableButton("CANCEL");
            }
        }

        protected void gvBigViewItems_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //                GetSubmExtraInfoHighlight(e, gvBigViewItems.DataKeys[e.Row.RowIndex].Value.ToString());
            }
        }

        protected void grdBigViewLink_DataBound(object sender, EventArgs e)
        {
            GridView grid = (GridView)sender;
            if (grid.HeaderRow != null)
                grid.HeaderRow.Visible = false;
        }

        protected void repBigViewLink_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            string caseKey = ((HiddenField)e.Item.FindControl("hdnBigViewLinkCaseKey")).Value;
            string currentExamKey = PLCSession.PLCGlobalAssignmentKey;

            string qry = @"SELECT I.CASE_KEY, ITA.EVIDENCE_CONTROL_NUMBER AS SELECTED, I.EVIDENCE_CONTROL_NUMBER, I.LAB_ITEM_NUMBER, I.DEPARTMENT_ITEM_NUMBER, I.THIRD_PARTY_BARCODE,
IT.DESCRIPTION AS ITEM_TYPE, I.ITEM_DESCRIPTION AS ITEM_DESC, I.QUANTITY, ITA.ITEM_SOURCE FROM TV_LABITEM I 
LEFT OUTER JOIN TV_ITEMTYPE IT ON I.ITEM_TYPE = IT.ITEM_TYPE
LEFT OUTER JOIN TV_ITASSIGN ITA ON (I.EVIDENCE_CONTROL_NUMBER = ITA.EVIDENCE_CONTROL_NUMBER AND ITA.EXAM_KEY = " + currentExamKey + @") 
WHERE I.LAB_ITEM_NUMBER <> '0' AND I.CASE_KEY = " + caseKey + @" 
ORDER BY I.ITEM_SORT";

            PLCQuery qrySubGrid = new PLCQuery(PLCSession.FormatSpecialFunctions(qry));
            qrySubGrid.Open();

            GridView grid = ((GridView)e.Item.FindControl("grdBigViewLink"));
            grid.DataSource = qrySubGrid.PLCDataTable;
            grid.DataBind();

            //third party barcode
            grid.Columns[2].Visible = PLCSession.GetLabCtrl("THIRD_PARTY_BC_IN_MATRIX") == "T";

            //department item number
            grid.Columns[3].Visible = PLCSession.GetLabCtrl("DEPT_ITEM_NUMBER_IN_MATRIX") == "T";
        }

        #endregion

        #region Worklists
        protected void btnWorklist_Click(object sender, EventArgs e)
        {
            assignWorklist.OpenWorklistOcxFunction = PLCDBGlobalClass.GetOpenWorklistOcxFunction(PLCDBPanel1.getpanelfield("SECTION").ToUpper());
            assignWorklist.Show();
        }
        #endregion Worklists

        protected void dlgAssignmentMessage_ConfirmClick(object sender, DialogEventArgs e)
        {
            switch (e.DialogKey)
            {
                case "Approve_NotLabAnalyst":
                    var groupRes = hdnGroupCode.Value;
                    GetSignature(groupRes);
                    break;

                default:
                    break;
            }
        }

        protected void anpAssignment_Continue(object sender, AutoNotesPacketEventArgs args)
        {
            string groupRes = PLCCommon.instance.GetReviewProcess(args.Process);

            PLCQuery qryProduceNP = QuerySelectedLabAssign();
            string usesPrintManager = PLCDBGlobalClass.GetExamCodeValue(Int32.Parse(PLCSession.PLCGlobalAssignmentKey), "USES_PRINT_MANAGER");
            if (anpAssignment.Status == AutoNotesPacketStatus.ViewObsolete)
            {
                txtAuditReason.Text = "";
                txtAuditReason.Focus();
                ScriptManager.RegisterStartupScript(this, this.GetType(), "_openAuditReasonPopup" + DateTime.Now.ToString(), "openAuditReasonPopup();", true);
            }
            else if (usesPrintManager == "S" && qryProduceNP.FieldByName("PRODUCE_NOTES_PACKET") == "D" && !PLCDBGlobal.instance.AssignmentSignedAlready(PLCSession.PLCGlobalAssignmentKey, "SIGNED"))
                ShowMessage("Cannot sign the assignment with drafted notes packet.");
            else
                ProcessReview(groupRes);
        }

        protected void anpAssignment_Requeue(object sender, AutoNotesPacketEventArgs args)
        {
            PLCQuery qryAnalystSigned = QuerySelectedLabAssign();
            if (qryAnalystSigned.FieldByName("ANALYST_SIGNED") == "T")
            {
                txtRegenReason.Text = hdnRegenReason.Value = "";
                fbRegenReason.Focus();
                ScriptManager.RegisterStartupScript(this, this.GetType(), "_openRegenReasonPopup" + DateTime.Now.ToString(), "openRegenReasonPopup();", true);
            }
            else
                GenerateNotes(false);
            anpAssignment.OverrideRequeue = false;
        }

        protected void btnRegenReasonDummy_Click(object sender, EventArgs e)
        {
            string reasonForRegen = hdnRegenReason.Value.ToString();
            reasonForRegen = reasonForRegen.Replace("\n", "\\n").Replace("\\", "\\\\").Replace("'", "\\'");

            if ((string)Session["SignOCXcalled"] == "standby")
                Session["SignOCXcalled"] = "T";

            string returnPage = "Dashboard.aspx";
            if (Request.UrlReferrer != null)
                returnPage = Request.UrlReferrer.AbsolutePath.Substring(Request.UrlReferrer.AbsolutePath.LastIndexOf("/") + 1);

            WebOCX.OnSuccessScript = "CheckBrowserWindowID(setTimeout(function () { ShowLoading();  window.location = '" + returnPage + "'; }));";
            WebOCX.CreateNotesPacket(PLCSession.PLCGlobalAssignmentKey, reasonForRegen);
        }

        protected void btnAuditReasonDummy_Click(object sender, EventArgs e)
        {
            string reason = hdnAuditReason.Value.ToString();
            string logInfo = "EXAM_KEY: " + PLCSession.PLCGlobalAssignmentKey +
                "\nANALYST: " + PLCSession.PLCGlobalAnalyst +
                "\nREASON FOR CHANGE: " + reason;
            PLCSession.WriteAuditLog("11100", "1", "", logInfo);

            string groupRes = PLCCommon.instance.GetReviewProcess(anpAssignment.Process);
            ProcessReview(groupRes);
        }

        protected void btnCancelSignOCX_Click(object sender, EventArgs e)
        {
            Session["SignOCXcalled"] = "F";

            Response.Redirect("~/TAB6Assignments.aspx");
        }

        #endregion

        #region Methods

        private void AddItemColsToDataSet(DataTable DT)
        {
            DT.Columns.Add("ID", typeof(String));
            DT.Columns.Add("SELECTED", typeof(String));
            DT.Columns.Add("ITEMNUM", typeof(String));
            DT.Columns.Add("TRACKINGNUM", typeof(String));
            DT.Columns.Add("DEPTITEMNUM", typeof(String));
            DT.Columns.Add("THIRDPARTYBC", typeof(String));
            DT.Columns.Add("PACKTYPE", typeof(String));
            DT.Columns.Add("ITEMTYPE", typeof(String));
            DT.Columns.Add("QUANTITY", typeof(String));
            DT.Columns.Add("ITEMDESC", typeof(String));
            DT.Columns.Add("SERVICEREQUEST", typeof(String));
            DT.Columns.Add("ECN", typeof(String));
            DT.Columns.Add("EXAM_KEY", typeof(String));
            DT.Columns.Add("ITEM_TYPE", typeof(String));
            DT.Columns.Add("PACKAGING_CODE", typeof(String));
            DT.Columns.Add("VOUCHERNUM", typeof(string));
            DT.Columns.Add("ITEMSOURCE", typeof(string));
        }

        private void LoadNames(string examKey)
        {
            gvNameItem.PLCSQLString_AdditionalFrom = "LEFT OUTER JOIN TV_REPTNAME RN ON LN.NAME_KEY = RN.NAME_KEY AND RN.EXAM_KEY = " + examKey;
            gvNameItem.PLCSQLString_AdditionalCriteria = "WHERE LN.CASE_KEY = " + PLCSession.PLCGlobalCaseKey;
            gvNameItem.InitializePLCDBGrid();
            PopulateItemNameKeysList();
        }

        private void SelectTab(int tabView)
        {
            tbItems.Attributes["class"] = tbNames.Attributes["class"] = "";
            divTabItems.Style["display"] = divTabNames.Style["display"] = "none";
            switch (tabView)
            {
                case VIEW_ITEMS:
                    tbItems.Attributes["class"] = "active";
                    divTabItems.Style["display"] = "";
                    break;
                case VIEW_NAMES:
                    tbNames.Attributes["class"] = "active";
                    divTabNames.Style["display"] = "";
                    break;
            }
        }

        private void EnableButtonControls(bool Enable, bool UnLock)
        {
            string sBatchKey = String.Empty;

            bnNotes.Enabled = Enable;
            bnReports.Enabled = Enable;
            bnVerifyReports.Enabled = Enable;
            bnRemoveBlind.Enabled = Enable;
            bnSign.Enabled = Enable;
            bnReview.Enabled = Enable;
            bnCodisReview.Enabled = Enable;
            bnApprove.Enabled = Enable;
            bnAdmClose.Enabled = Enable;
            bnRollback.Enabled = Enable;
            bnUnlock.Enabled = Enable;
            bnSRPrint.Enabled = Enable;
            bnBigView.Enabled = Enable;
            bnQMS.Enabled = Enable;
            bnSOP.Enabled = Enable;
            btnWorklist.Enabled = Enable && PLCSession.CheckUserOption("IBATCHRES");
            btnDNAWorksheets.Enabled = Enable;

            if (!Enable)
                bnReviewPlan.Visible = Enable;

            CheckDisableEditDeleteBtns();
            SetQCFlagButtonState();
            SetAttachmentsButtonMode(!(GridView1.Rows.Count == 0));
            EnablePaperClipModeless(Enable);
        }

        private void SetQCFlagButtonState()
        {
            if (this.hdnQCFlag.Value != "")
            {
                this.PLCButtonPanel1.DisableAllButtons();
                bpDNADates.DisableButton("EDIT");

                bnNotes.Enabled = false;
                bnReports.Enabled = false;
                bnVerifyReports.Enabled = false;
                bnRemoveBlind.Enabled = false;
                bnSign.Enabled = false;
                bnReview.Enabled = false;
                bnApprove.Enabled = false;
                bnAdmClose.Enabled = false;
                bnRollback.Enabled = false;
                bnUnlock.Enabled = false;
                //  btnRouteItems.Enabled = false;
                bnSRPrint.Enabled = false;
                bnSOP.Enabled = false;
                bnQMS.Enabled = false;
                bnReviewPlan.Visible = false;
            }
        }

        private int GetInitialNumLabExam()
        {
            if (this.numLabExam == -1)
            {
                PLCQuery qry = new PLCQuery("SELECT COUNT(*) AS NUMLABEXAM FROM TV_LABEXAM Where CASE_KEY = ?");
                qry.AddParameter("CASE_KEY", PLCSession.PLCGlobalCaseKey);
                qry.Open();
                if (qry.HasData())
                    this.numLabExam = qry.iFieldByName("NUMLABEXAM");
                else
                    this.numLabExam = 0;
            }

            return this.numLabExam;
        }

        private void SetReadOnlyAccess()
        {
            // Disable controls in the page for read only access
            if (PLCCommonClass.IsReadOnlyAccess("WEBINQ,RONLYASTAB"))
            {
                if (MultiView1.ActiveViewIndex == 3) // Routing
                {
                    btnRoute.Enabled = false;
                    btnRoutingReply.Enabled = false;
                    btnRoutingCancel.Enabled = false;
                    bnAssignRoute.Enabled = false;
                }
                else if (MultiView1.ActiveViewIndex == 4) // Data
                {
                    bnDataAdd.Enabled = false;
                    bnDataRemove.Enabled = false;
                    bnDataEdit.Enabled = false;
                }
                else if (MultiView1.ActiveViewIndex == 5) // DNA Dates
                {
                    PLCCommonClass.SetReadOnlyAccess(bpDNADates);
                }
                else if (MultiView1.ActiveViewIndex == 6) // Additional Analyst(s) 
                {
                    btnAddAnalyst.Enabled = false;
                    btnRemoveAnalyst.Enabled = false;
                }
                else // Details
                {
                    // Disable plcbuttonpanel and other plcbuttonpanel custom button for read only access
                    PLCCommonClass.SetReadOnlyAccess(PLCButtonPanel1, "RecordUnlock,Manage,Split,Revisions");

                    // Show button if its already signed
                    if (!string.IsNullOrEmpty(PLCSession.PLCGlobalAssignmentKey) && PLCDBGlobal.instance.AssignmentSignedAlready(PLCSession.PLCGlobalAssignmentKey, "SIGNED"))
                        bnSign.Visible = true;
                    else
                        bnSign.Visible = false;

                    // Show button if its already reviewed
                    if (!string.IsNullOrEmpty(PLCSession.PLCGlobalAssignmentKey) && PLCDBGlobal.instance.IsAssignmentTechReviewed(PLCSession.PLCGlobalAssignmentKey))
                        bnReview.Visible = true;
                    else
                        bnReview.Visible = false;

                    bnApprove.Visible = false;
                    bnAdmClose.Visible = false;
                    bnRollback.Visible = false;

                    hdnIsROnlyAccess.Value = "T";
                }
            }
        }

        private void UpdateHiddenControlValues()
        {
            // Save current case key, exam key, and section for Big View popup.
            this.hdnCurrentExamKey.Value = PLCSession.PLCGlobalAssignmentKey;
            this.hdnCurrentCaseKey.Value = PLCSession.PLCGlobalCaseKey;
            this.hdnCurrentSection.Value = PLCDBPanel1.getpanelfield("SECTION");
            this.hdnCurrentAnalyst.Value = PLCSession.PLCGlobalAnalyst;

            // Save state of PullList Print button.
            if (GetPullListCrystalReportName(PLCDBPanel1.getpanelfield("Section")) != "")
                this.hdnPullListEnabledState.Value = "true";
            else
                this.hdnPullListEnabledState.Value = "false";
        }

        private void GrabGridRecord()
        {
            assignedItems.Clear();

            if (GridView1.Rows.Count > 0)
            {
                PLCDBPanel1.PLCWhereClause = dbpDNADates.PLCWhereClause = " where EXAM_KEY = " + GridView1.SelectedDataKey.Values["EXAM_KEY"].ToString();
                PLCSession.PLCGlobalAssignmentKey = hdnAssignKey.Value = GridView1.SelectedDataKey.Values["EXAM_KEY"].ToString();

                ReloadDBPanels();
                UpdateHiddenControlValues();

                PLCButtonPanel1.SetBrowseMode();
                bpDNADates.SetBrowseMode();

                string assignSection = PLCDBPanel1.getpanelfield("SECTION");

                SetAttachmentsButton("ASSIGNMENT");
                MasterPage mp = (MasterPage)this.Master;
                mp.SetFilterDescription(GridView1.SelectedDataKey["SEQUENCE"].ToString());

                if (PLCDBGlobal.instance.IsAssignAttachRestricted(PLCSession.PLCGlobalAssignmentKey))
                    SetAttachmentsButtonMode(false);
                else
                    SetAttachmentsButtonMode(true);

                GetAnalystSectionInfo();

                if (!PLCDBGlobal.instance.AssignmentSignedAlready(PLCSession.PLCGlobalAssignmentKey, "SIGNED") || !PLCDBGlobal.instance.HasNotesRevision())
                    PLCButtonPanel1.DisableButton("Rev Note");

                hdnReportSigned.Value = PLCDBGlobal.instance.AssignmentSignedAlready(PLCSession.PLCGlobalAssignmentKey, "SIGNED") ? "T" : "F";
                hdnReportReconciled.Value = PLCDBGlobal.instance.AssignmentReconciledAlready(PLCSession.PLCGlobalAssignmentKey) ? "T" : "F";

                // Set state of TechReview button: 
                string sBatchKey = String.Empty;
                EnableLink();
                if (!PLCDBGlobal.instance.IsAssignmentInBatch(PLCSession.PLCGlobalAssignmentKey, ref sBatchKey))
                {
                    this.hdnIsAssignmentInBatch.Value = "0";
                    bnReviewPlan.Visible = PLCSession.CheckSectionFlag(PLCDBPanel1.getpanelfield("SECTION"), "USES_REVIEW_PLAN")
                        && PLCDBGlobal.instance.IsCheckListExists(PLCSessionVars1.PLCGlobalAssignmentKey, "REVIEW", PLCDBPanel1.getpanelfield("SECTION"), AssignmentReviewType.SingleAssignment)
                        && (PLCDBPanel1.getpanelfield("ANALYST_ASSIGNED") == PLCSession.PLCGlobalAnalyst || PLCSession.GetUserAnalSect(PLCDBPanel1.getpanelfield("SECTION"), "EDIT_REVIEW_PLAN_FOR_OTHERS").ToString() == "T")
                        && PLCDBGlobal.instance.AssignmentSignedAlready(PLCSession.PLCGlobalAssignmentKey, "SIGNED")
                        && !PLCDBGlobal.instance.IsAssignmentTechReviewed(PLCSession.PLCGlobalAssignmentKey);
                }
                else
                {
                    this.hdnIsAssignmentInBatch.Value = "1";
                    bnReviewPlan.Visible = false;
                    PLCButtonPanel1.DisableButton("Split");
                }

                if (!PLCCommonClass.IsReadOnlyAccess("WEBINQ,RONLYASTAB"))
                {
                    // Codis Revew button
                    bnCodisReview.Visible = PLCSession.GetLabCtrlFlag("USES_CODIS_REVIEW") == "T" && PLCDBGlobal.instance.IsCodisReviewAssignment(PLCSession.PLCGlobalAssignmentKey);

                    string LockedInfo = "";
                    bool isLocked = PLCDBGlobal.instance.IsRecordLocked("TV_LABEXAM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalAssignmentKey, "-1", out LockedInfo);

                    if (isLocked)
                        PLCButtonPanel1.SetButtonsForLock(true);
                    else
                        PLCButtonPanel1.DisableLock();

                    lPLCLockStatus.Text = LockedInfo;
                    dvPLCLockStatus.Visible = isLocked;
                }

                if (PLCCommon.instance.IsReadOnlyAccess("WEBINQ," + PLCCommonClass.GetUOROnlyAccess(PLCSession.PLCGlobalAttachmentSource)))
                    btnDNAWorksheets.Visible = false;
                else
                    btnDNAWorksheets.Visible = PLCSession.CheckSectionFlag(PLCDBPanel1.getpanelfield("SECTION"), "SHOW_MATRIX_WORKSHEETS_TAB");

                mp.SetBatchAssignLabel(sBatchKey);
                Session["BATCH_ASSIGN_KEY"] = sBatchKey == "" ? null : sBatchKey;

                LoadNames(GridView1.SelectedDataKey["EXAM_KEY"].ToString());
                LoadAssignmentItems(PLCSession.PLCGlobalAssignmentKey);
                //
                PLCSession.SetDefault("TASKEDIT_SECTION", PLCDBPanel1.getpanelfield("SECTION"));
                SetBigViewButtonColor();

                btnBigViewResults.Visible = PLCSession.GetSectionFlag(PLCDBPanel1.getpanelfield("SECTION"), "SHOW_RESULTS_OF_ITASSIGN") == "L";

                // set notes button
                bnNotes.Enabled = CanUseNotesFeature();

                //this.bnQMS.Enabled = true;
                this.hdnLastRowSel.Value = GridView1.SelectedIndex.ToString();

                // revisions button
                if (!PLCSession.CheckUserOption("HIDERVN"))
                    PLCButtonPanel1.SetCustomButtonMode("Revisions", PLCDBGlobal.instance.AssignmentHasRevList(PLCSession.PLCGlobalAssignmentKey));

                SetServiceRequestButtonColor(checkRpt: false, checkData: PLCSession.GetLabCtrlFlag("ASSIGN_SR_BTN_RED_BASED_ON_RPT") != "T");
                ToggleResultsButton();
                CheckDisableEditDeleteBtns();
                SetQCFlagButtonState();
                SetReadOnlyAccess();
            }
        }

        private void ReloadDBPanels()
        {
            PLCDBPanel1.DoLoadRecord();
            dbpDNADates.DoLoadRecord();
            bnDNADates.Visible = PLCSession.CheckSectionFlag(PLCDBPanel1.getpanelfield("SECTION"), "SHOW_DNA_DATES");
            bnAnalysts.Visible = PLCSession.CheckSectionFlag(PLCDBPanel1.getpanelfield("SECTION"), "USES_ADDITIONAL_ANALYST");
        }

        private void GetAnalystSectionInfo()
        {
            string sBatchKey = String.Empty;
            Dictionary<string, object> AnalystSection = GetAnalystSection();
            string sectionCode = PLCDBPanel1.getpanelfield("SECTION").Trim();
            bool canViewInProcessReports = false;
            bool canCreateSupervisorWorksheet = PLCSession.GetUserAnalSect(sectionCode, "CREATE_SUPERVISOR_WORKSHT").ToString() == "T";
            bool skipTechReview = PLCSession.CheckSectionFlag(sectionCode, "SKIP_TECH_REVIEW");

            if (AnalystSection.Count() > 0)
            {
                if (AnalystSection.ContainsKey(PLCDBPanel1.getpanelfield("SECTION").Trim()))
                {
                    Dictionary<string, string> section = (Dictionary<string, string>)AnalystSection[sectionCode];
                    if (section.ContainsKey("WRITE"))
                    {
                        bnReports.Enabled = canCreateSupervisorWorksheet ? true : (!IsLockedSignedReportsButton(PLCSession.PLCGlobalAssignmentKey, PLCDBPanel1.getpanelfield("STATUS"))) && (section["WRITE"] == "T" ? true : false);
                        bnSign.Enabled = (section["WRITE"] == "T" ? true : false);
                    }

                    bnApprove.Enabled = (section.ContainsKey("APPROVE")) && (section["APPROVE"] == "T");
                    bnReview.Enabled = (section.ContainsKey("REVIEW")) && (section["REVIEW"] == "T");
                    bnCodisReview.Enabled = (section.ContainsKey("REVIEW")) && (section["REVIEW"] == "T");

                    if (section.ContainsKey("ADMINCLOSE"))
                        bnAdmClose.Enabled = (section["ADMINCLOSE"] == "T" ? true : false);
                }
            }
            else
            {
                bnReports.Enabled = false;
                bnApprove.Enabled = false;
                bnReview.Enabled = false;
                bnAdmClose.Enabled = false;
            }

            canViewInProcessReports = PLCSession.GetUserAnalSect(PLCDBPanel1.getpanelfield("SECTION").Trim(), "VIEW_IN_PROCESS_REPORTS").ToString() == "T";

            bnVerifyReports.Enabled = true;
            bnRemoveBlind.Enabled = true;

            // if Assignment is in a Batch, change Sign button text to 'Notes Packet'
            if (PLCDBGlobal.instance.IsAssignmentInBatch(PLCSession.PLCGlobalAssignmentKey, ref sBatchKey))
                bnSign.Text = PLCSession.GetSysPrompt("TAB6Assignments.bnSign.NOTES_PACKET", "Notes Packet");
            else if (PLCDBGlobal.instance.AssignmentSignedAlready(PLCSession.PLCGlobalAssignmentKey, "SIGNED"))
            {
                bnSign.Text = PLCSession.GetSysPrompt("TAB6Assignments.bnSign.SIGNED", "Signed");
                bnSign.ForeColor = Color.Red;
            }
            else
            {
                bnSign.Text = PLCSession.GetSysPrompt("LABCTRL.SIGN_BUTTON_TEXT", PLCSession.GetLabCtrl("SIGN_BUTTON_TEXT"));
                bnSign.ForeColor = Color.Black;
            }

            // update sign button if using service side notes packet
            string usesPrintManager = PLCDBGlobalClass.GetExamCodeValue(Int32.Parse(PLCSession.PLCGlobalAssignmentKey), "USES_PRINT_MANAGER");
            if (usesPrintManager == "S" && !PLCDBGlobal.instance.AssignmentSignedAlready(PLCSession.PLCGlobalAssignmentKey, "SIGNED"))
            {
                PLCQuery qryProduceNP = QuerySelectedLabExam();
                if (qryProduceNP.HasData())
                {
                    string produceNP = qryProduceNP.FieldByName("PRODUCE_NOTES_PACKET");
                    if (produceNP == "C")
                        bnSign.Text = "NP Ready";
                    else if (produceNP == "E")
                        bnSign.Text = "NP ERROR";
                    else if (!string.IsNullOrEmpty(produceNP) && produceNP != "D" && produceNP != "R")
                        bnSign.Text = "Waiting For NP";
                }
            }

            string reviewStatus = PLCDBGlobal.instance.GetExamStatusCode("R", "4");
            string approveStatus = PLCDBGlobal.instance.GetExamStatusCode("C", "5");
            string currentAssignStatus = PLCDBGlobal.instance.GetLabAssignField(PLCSession.PLCGlobalAssignmentKey, "STATUS");
            string assignStatus = "'" + reviewStatus + "'," + "'" + approveStatus + "'";

            //this is to include REVLIST.STATUS_CODE set for SIGNED, REVIEW and APPROVE
            if (currentAssignStatus != reviewStatus && currentAssignStatus != approveStatus)
                assignStatus += ",'" + currentAssignStatus + "'";

            bool codisReviewed = false;
            PLCQuery qryCodisRev = new PLCQuery(String.Format("SELECT * FROM TV_LABASSIGN WHERE EXAM_KEY = {0} AND STATUS IN ({1})  AND STATUS <> " + PLCDBGlobal.instance.GetExamStatusCode("W", "10"), PLCSession.PLCGlobalAssignmentKey, assignStatus));
            qryCodisRev.Open();
            if (!qryCodisRev.IsEmpty())
                codisReviewed = true;

            // If assignment has been tech reviewed, check if checklist(s) exists then Tech Review button is enabled else button is disabled.
            bool enableReviewButtonWithChecklist = !PLCDBGlobal.instance.IsAssignmentTechReviewed(PLCSession.PLCGlobalAssignmentKey)
                    ? true
                    : (PLCDBGlobal.instance.IsCheckListExists(PLCSession.PLCGlobalAssignmentKey, "REVIEW", PLCDBPanel1.getpanelfield("SECTION"), AssignmentReviewType.SingleAssignment))
                      ? true
                      : false;


            bnReports.Enabled = bnReports.Enabled
                && (PLCDBGlobal.instance.AssignmentSignedAlready(PLCSession.PLCGlobalAssignmentKey, "SIGNED")
                    ? (PLCSession.GetLabCtrl("ENABLE_REPORTS_ON_SIGN") == "T"
                        ? true
                        : canViewInProcessReports) // enable button to allow access to matrix in read-only even after signing, and/or even if user has no assignment access
                    : HasAssignmentReportsAccess() || canViewInProcessReports);

            bool assignmentSigned = PLCDBGlobal.instance.AssignmentSignedAlready(PLCSession.PLCGlobalAssignmentKey, "SIGNED");
            bool hasExistingCheckList = PLCDBGlobal.instance.IsCheckListExists(PLCSessionVars1.PLCGlobalAssignmentKey, "SIGNED", PLCDBPanel1.getpanelfield("SECTION"), AssignmentReviewType.SingleAssignment);
            bnSign.Enabled = bnSign.Enabled
                && (assignmentSigned
                    ? (PLCSession.GetLabCtrl("ENABLE_REPORTS_ON_SIGN") == "T")
                    : true);

            bool isTechReviewed = PLCDBGlobal.instance.IsAssignmentTechReviewed(PLCSession.PLCGlobalAssignmentKey);
            bnReview.Enabled = bnReview.Enabled && !PLCDBGlobal.instance.IsAssignmentInBatch(PLCSession.PLCGlobalAssignmentKey, ref sBatchKey) &&
                PLCDBGlobal.instance.AssignmentSignedAlready(PLCSession.PLCGlobalAssignmentKey, "SIGNED") &&
                (PLCDBGlobal.instance.IsAssignmentTechReviewed(PLCSession.PLCGlobalAssignmentKey) ? 
                    PLCSession.GetLabCtrl("ENABLE_REPORTS_ON_SIGN") == "T"
                    : true);

            if(bnReview.Enabled && isTechReviewed)
                bnReview.ForeColor = Color.Red;
            else
                bnReview.ForeColor = Color.Black;

            bnCodisReview.Enabled = bnCodisReview.Enabled && !PLCDBGlobal.instance.IsAssignmentInBatch(PLCSession.PLCGlobalAssignmentKey, ref sBatchKey) &&
                PLCDBGlobal.instance.AssignmentSignedAlready(PLCSession.PLCGlobalAssignmentKey, "SIGNED") &&
                (PLCDBGlobal.instance.IsAssignmentTechReviewed(PLCSession.PLCGlobalAssignmentKey) || (!PLCSession.CheckSectionFlag(PLCDBPanel1.getpanelfield("SECTION"), "REQUIRES_PEER_REVIEW"))) &&
                PLCDBGlobal.instance.IsAssignmentReadyForCodisReview(PLCSession.PLCGlobalAssignmentKey);
            bnApprove.Enabled = bnApprove.Enabled && PLCDBGlobal.instance.AssignmentSignedAlready(PLCSession.PLCGlobalAssignmentKey, "SIGNED") &&
                (PLCDBGlobal.instance.IsAssignmentTechReviewed(PLCSession.PLCGlobalAssignmentKey) && (PLCSession.GetLabCtrlFlag("USES_CODIS_REVIEW") == "T" ? codisReviewed : true) ||
                ((!PLCSession.CheckSectionFlag(PLCDBPanel1.getpanelfield("SECTION"), "REQUIRES_PEER_REVIEW") && !PLCDBGlobal.instance.NeedtoAssignReviewer(PLCSession.PLCGlobalAssignmentKey)) || skipTechReview)) &&
                !PLCDBGlobal.instance.IsAssignmentInBatch(PLCSession.PLCGlobalAssignmentKey, ref sBatchKey);
            bnAdmClose.Enabled = bnAdmClose.Enabled && !PLCDBGlobal.instance.IsAssignmentInBatch(PLCSession.PLCGlobalAssignmentKey, ref sBatchKey);
            bnRollback.Enabled = !PLCDBGlobal.instance.IsAssignmentInBatch(PLCSession.PLCGlobalAssignmentKey, ref sBatchKey) && PLCDBGlobal.instance.AssignmentSignedAlready(PLCSession.PLCGlobalAssignmentKey, "SIGNED");

            if (PLCSession.CheckSectionFlag(sectionCode, "USES_ANALYST_TASK_PERMISSIONS") //if section uses analyst task permissions
                && (bnReview.Enabled || bnApprove.Enabled)) //only check tasks permissions if the buttons are not disabled from previous logic
            {
                //if assignment has tasks, check if current analyst has TECH_REVIEW and ADMIN_REVIEW permissions for all task list types of the assignment (ANALTASK)
                //if assignment has no tasks, check if current analyst has REVIEW and APPROVE permissions for selected section (ANALSECT)

                // assignments with tasks and section.USES_ANALYST_TASK_PERMISSIONS is true
                // UNION
                // assignments with no tasks and section.USES_ANALYST_TASK_PERMISSIONS is true
                string sql = string.Format(@"SELECT TECH_REVIEW AS CANREVIEW, ADMIN_REVIEW AS CANAPPROVE FROM TV_LABEXAM E
INNER JOIN TV_EXAMCODE EC ON E.SECTION = EC.EXAM_CODE
INNER JOIN TV_TASKLIST TL ON E.EXAM_KEY = TL.EXAM_KEY
LEFT OUTER JOIN TV_ANALTASK A ON UPPER(TL.TASK_TYPE) = UPPER(A.TASK_TYPE) 
WHERE EC.USES_ANALYST_TASK_PERMISSIONS = 'T' AND E.EXAM_KEY = {0} AND A.ANALYST = '{1}'
UNION
SELECT REVIEW_REPORTS AS CANREVIEW, APPROVE_REPORTS AS CANAPPROVE FROM TV_LABEXAM E
LEFT OUTER JOIN TV_TASKLIST T ON E.EXAM_KEY = T.EXAM_KEY
INNER JOIN TV_EXAMCODE EC ON E.SECTION = EC.EXAM_CODE
INNER JOIN TV_ANALSECT ASEC ON EC.EXAM_CODE = ASEC.SECTION
WHERE USES_ANALYST_TASK_PERMISSIONS = 'T' AND E.EXAM_KEY =  {0} AND ASEC.ANALYST = '{1}'
GROUP BY ASEC.ANALYST, REVIEW_REPORTS, APPROVE_REPORTS
HAVING COUNT(T.EXAM_KEY) = 0", PLCSession.PLCGlobalAssignmentKey, PLCSession.PLCGlobalAnalyst);
                PLCQuery qryCanReview = new PLCQuery(sql);
                qryCanReview.Open();
                if (!qryCanReview.IsEmpty())
                {
                    bool canReview = qryCanReview.PLCDataTable.AsEnumerable().All(a => Convert.ToString(a["CANREVIEW"]).Trim().ToUpper() == "T"); //analyst can review all task list types in the assignment
                    bool canApprove = qryCanReview.PLCDataTable.AsEnumerable().All(a => Convert.ToString(a["CANAPPROVE"]).Trim().ToUpper() == "T"); //analyst can approve all task list types in the assignment

                    bnReview.Enabled = bnReview.Enabled ? canReview : false;
                    bnApprove.Enabled = bnApprove.Enabled ? canApprove : false;
                }
                else
                {
                    bnReview.Enabled = false;
                    bnApprove.Enabled = false;
                }
            }

            if (skipTechReview && bnReview.Enabled)
            {
                bnReview.Enabled = false;
            }

            CheckDisableEditDeleteBtns();

            SetQCFlagButtonState();
        }

        private string GetReviewType()
        {
            return "REVIEW";
        }

        private void LoadItems()
        {
            string ShowFileItem = string.Empty;

            if (PLCSession.GetLabCtrl("SHOW_FILE_ITEM") != "T")
            {
                ShowFileItem = "AND I.LAB_ITEM_NUMBER != '0' ";
            }

            string sqlstr = "select EVIDENCE_CONTROL_NUMBER, LAB_ITEM_NUMBER || ' - ' || P.DESCRIPTION || ' : ' || IT.DESCRIPTION AS Description, " +
                            "(select count(*) from TV_ITASSIGN Where EVIDENCE_CONTROL_NUMBER = I.EVIDENCE_CONTROL_NUMBER and EXAM_KEY = " + GridView1.SelectedDataKey.Values["EXAM_KEY"].ToString() + ") AS Count " +
                            "from TV_LABITEM I LEFT OUTER JOIN TV_ITEMTYPE IT ON IT.ITEM_TYPE = I.ITEM_TYPE " +
                            "LEFT OUTER JOIN TV_PACKTYPE P ON P.PACKAGING_CODE = I.PACKAGING_CODE " +
                            "where I.CASE_KEY = " + PLCSession.PLCGlobalCaseKey + " " + ShowFileItem +
                            "order by I.LAB_ITEM_NUMBER";

            //Label1.Text = ">" + sqlstr + "<";

            PLCQuery qryItems = new PLCQuery();
            qryItems.SQL = PLCSession.FormatSpecialFunctions(sqlstr);
            qryItems.Open();

            PLCDBCheckList1.ClearItems();

            while (!qryItems.EOF())
            {
                if (qryItems.FieldByName("COUNT") == "0")
                    PLCDBCheckList1.AddItem(qryItems.FieldByName("EVIDENCE_CONTROL_NUMBER"), qryItems.FieldByName("DESCRIPTION"), false);
                else
                    PLCDBCheckList1.AddItem(qryItems.FieldByName("EVIDENCE_CONTROL_NUMBER"), qryItems.FieldByName("DESCRIPTION"), true);

                qryItems.Next();
            }
        }

        private void SaveItemsToITAssign(string keystring)
        {
            string theExamKey = GridView1.SelectedDataKey.Values["EXAM_KEY"].ToString();

            string sqlstr = "DELETE from TV_ITASSIGN where EXAM_KEY = " + theExamKey;
            PLCQuery qryDel = new PLCQuery(sqlstr);
            qryDel.ExecSQL();

            char[] DelimChars = { ',' };
            string[] keylist = keystring.Split(DelimChars);
            sqlstr = "";
            foreach (string s in keylist)
            {
                InsertBigViewItem(s, null, null, null);
            }

        }

        private void UpdateAssignmentInfo()
        {
            string status = PLCDBPanel1.GetOriginalValue("STATUS");
            string assignStatus = string.Empty;
            if (!string.IsNullOrEmpty(PLCDBPanel1.getpanelfield("ANALYST_ASSIGNED")))
            {
                if (status == PLCDBGlobalClass.GetExamStatusCode("S", "0"))
                    PLCDBPanel1.setpanelfield("STATUS", PLCDBGlobalClass.GetExamStatusCode("L", "1"));
                else if (status == PLCDBGlobalClass.GetExamStatusCode("V", "3") || status == PLCDBGlobalClass.GetExamStatusCode("R", "4") || status == PLCDBGlobal.instance.GetExamStatusCode("W", "10"))
                {
                    assignStatus = !string.IsNullOrEmpty(PLCDBGlobalClass.GetExamStatusCode("L", "1")) ?
                        PLCDBGlobalClass.GetExamStatusCode("L", "1") : PLCSession.GetLabCtrl("SD_ALLOW_ASSIGN_CHANGE") == "T" ? "1" : "2"; //Merged from SD Rev#10494
                    PLCDBPanel1.setpanelfield("STATUS", assignStatus);
                }

                DateTime analystDate = DateTime.TryParse(PLCDBPanel1.getpanelfield("ANALYST_DATE").ToString(), out analystDate) ? analystDate : DateTime.Now;
                //PLCDBPanel1.setpanelfield("ANALYST_DATE", analystDate.ToString("MM/dd/yyyy"));
                PLCDBPanel1.setpanelfield("ANALYST_DATE", PLCDBPanel1.ConvertToFieldShortDateFormat("ANALYST_DATE",analystDate));

                if ((status == PLCDBGlobalClass.GetExamStatusCode("D", "2") && PLCDBGlobal.instance.AssignmentSignedAlready(PLCSessionVars1.PLCGlobalAssignmentKey, "SIGNED")) || status == PLCDBGlobalClass.GetExamStatusCode("V", "3") || status == PLCDBGlobalClass.GetExamStatusCode("R", "4") || status == PLCDBGlobal.instance.GetExamStatusCode("W", "10"))
                {
                    bool canSignWithObsoleteNP = PLCSession.GetSectionFlag(PLCDBPanel1.getpanelfield("SECTION"), "CAN_SIGN_WITH_OBSOLETE_NP") == "T";
                    PLCDBGlobal.instance.RemoveAssignmentSignature(PLCSession.PLCGlobalAssignmentKey, (status == PLCDBGlobalClass.GetExamStatusCode("V", "3") || status == PLCDBGlobalClass.GetExamStatusCode("R", "4") && canSignWithObsoleteNP ? 3 : 0));
                    //PLCDBGlobal.instance.DeleteNotesPacket(PLCSession.PLCGlobalAssignmentKey);
                    PLCDBGlobal.instance.ResetChecklists(PLCSession.PLCGlobalAssignmentKey, false, "SIGNED");
                    PLCDBGlobal.instance.ResetChecklists(PLCSession.PLCGlobalAssignmentKey, false, "REVIEW");
                    PLCDBGlobalClass.ResetChecklists(PLCSessionVars1.PLCGlobalAssignmentKey, false, "CODISREV");
                    PLCDBGlobal.instance.ResetChecklists(PLCSession.PLCGlobalAssignmentKey, false, "APPROVE");
                    PLCDBGlobal.instance.ResetAssignmentReviewFlags(PLCSession.PLCGlobalAssignmentKey);

                    if (PLCDBGlobal.instance.AssignmentReconciledAlready(PLCSession.PLCGlobalAssignmentKey))
                        PLCDBGlobal.instance.UpdateAssignmentToUnVerified(PLCSession.PLCGlobalAssignmentKey);
                }
            }
            else
            {
                PLCQuery qryStatus = new PLCQuery("SELECT USAGE_RES FROM TV_EXAMSTAT WHERE EXAM_STATUS = ?");
                qryStatus.AddSQLParameter("EXAM_STATUS", status);
                qryStatus.Open();

                if (qryStatus.FieldByName("USAGE_RES") != "S")
                {
                    PLCDBPanel1.setpanelfield("STATUS", PLCDBGlobalClass.GetExamStatusCode("S", "0"));
                    PLCDBPanel1.setpanelfield("ANALYST_DATE", "");

                    if (status == PLCDBGlobalClass.GetExamStatusCode("V", "3") || status == PLCDBGlobalClass.GetExamStatusCode("R", "4"))
                    {
                        PLCDBGlobal.instance.RemoveAssignmentSignature(PLCSession.PLCGlobalAssignmentKey);
                        //PLCDBGlobal.instance.DeleteNotesPacket(PLCSession.PLCGlobalAssignmentKey);
                        PLCDBGlobal.instance.DeleteChecklists(PLCSession.PLCGlobalAssignmentKey, false);
                        PLCDBGlobal.instance.ResetAssignmentReviewFlags(PLCSession.PLCGlobalAssignmentKey);
                    }
                }
            }
        }

        private void UpdateAssignedBy(int changeKey)
        {
            if (!string.IsNullOrEmpty(PLCDBPanel1.getpanelfield("ANALYST_ASSIGNED")) &&
                PLCDBPanel1.getpanelfield("ANALYST_ASSIGNED") != PLCDBPanel1.GetOriginalValue("ANALYST_ASSIGNED"))
            {
                // update assigned_by
                PLCQuery qryLabExam = new PLCQuery();
                qryLabExam.SQL = "SELECT * FROM TV_LABEXAM WHERE EXAM_KEY = " + PLCSession.PLCGlobalAssignmentKey;
                qryLabExam.Open();
                if (!qryLabExam.IsEmpty())
                {
                    qryLabExam.Edit();
                    qryLabExam.SetFieldValue("ASSIGNED_BY", PLCSession.PLCGlobalAnalyst);
                    if (string.IsNullOrEmpty(qryLabExam.FieldByName("DATE_ASSIGNED")))
                        qryLabExam.SetFieldValue("DATE_ASSIGNED", DateTime.Today);
                    qryLabExam.Post("TV_LABEXAM", 17, 1, changeKey);
                }
            }
        }

        private void RecordCommentChange(CommentChange commentChange, int changeKey)
        {
            PLCQuery qryCommentChange = new PLCQuery("SELECT * FROM TV_COMMENTCHG WHERE 0 = 1");
            qryCommentChange.Open();
            qryCommentChange.Append();
            qryCommentChange.SetFieldValue("COMMENTCHG_KEY", PLCSession.GetNextSequence("COMMENTCHG_SEQ"));
            qryCommentChange.SetFieldValue("EXAM_KEY", commentChange.examKey);
            qryCommentChange.SetFieldValue("COMMENTS", commentChange.newComment);
            qryCommentChange.SetFieldValue("ANALYST", commentChange.analyst);
            qryCommentChange.SetFieldValue("CHANGE_DATE", commentChange.changeDate);
            qryCommentChange.Post("TV_COMMENTCHG", 38, 1, changeKey);
        }

        private bool CheckForChanges()
        {
            bool changesInTabs = false;

            // Check Main DBPanel
            if (PLCSession.GetLabCtrl("LOG_REPORT_FORMAT_CHANGE") == "T")
                changesInTabs = PLCDBPanel1.HasChanges(PLCSession.GetLabCtrl("LOG_BLANK_TO_DATA_CHANGES") == "T", "REPORT_FORMAT");
            else
                changesInTabs = PLCDBPanel1.HasChanges(PLCSession.GetLabCtrl("LOG_BLANK_TO_DATA_CHANGES") == "T");

            // Check Assign Items Tab Changes
            List<string> assignItemChanges;
            ViewState["AssignItemChanges"] = null;
            if (CheckItemChanges(out assignItemChanges))
            {
                ViewState["AssignItemChanges"] = assignItemChanges;
                changesInTabs = true;
            }

            // Check Assign Names Tab Changes
            List<string> assignNameChanges;
            ViewState["AssignNameChanges"] = null;
            if (CheckNameChanges(out assignNameChanges))
            {
                ViewState["AssignNameChanges"] = assignNameChanges;
                changesInTabs = true;
            }

            return changesInTabs;
        }

        private bool CheckNameChanges(out List<string> assignNameChanges)
        {
            List<string> listOfChanges = new List<string>();
            List<string> origNameList = new List<string>();

            PLCQuery qryNames = new PLCQuery("SELECT NAME_KEY FROM TV_REPTNAME WHERE EXAM_KEY = " + PLCSession.PLCGlobalAssignmentKey);
            if (qryNames.Open() && qryNames.HasData())
            {
                while (!qryNames.EOF())
                {
                    origNameList.Add(qryNames.FieldByName("NAME_KEY"));
                    qryNames.Next();
                }
            }

            foreach (GridViewRow row in gvNameItem.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    string nameKey = gvNameItem.DataKeys[row.RowIndex].Value.ToString();
                    string nameDesc = PLCDBGlobal.instance.StripHTMLTags(row.Cells[2].Text);

                    // Check for existing list to determine what should be deleted and added (and to leave alone)
                    if (((CheckBox)row.FindControl("cbSelect")).Checked)
                    {
                        //if (!origNameList.Contains(nameKey))
                        //{
                        //	listOfChanges.Add(string.Format("ADDED ASSIGNMENT NAME LINK --- NAME: {0}", nameDesc));
                        //}
                    }
                    else
                    {
                        if (origNameList.Contains(nameKey))
                            listOfChanges.Add(string.Format("DELETED ASSIGNMENT NAME LINK --- NAME: {0}", nameDesc));
                    }
                }
            }

            assignNameChanges = listOfChanges;
            return listOfChanges.Count > 0;
        }

        private bool NameHasChanges()
        {
            List<string> origNameList = new List<string>();
            PLCQuery qryNames = new PLCQuery("SELECT NAME_KEY FROM TV_REPTNAME WHERE EXAM_KEY = " + PLCSession.PLCGlobalAssignmentKey);
            if (qryNames.Open() && qryNames.HasData())
            {
                while (!qryNames.EOF())
                {
                    origNameList.Add(qryNames.FieldByName("NAME_KEY"));
                    qryNames.Next();
                }
            }

            foreach (GridViewRow row in gvNameItem.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    string nameKey = gvNameItem.DataKeys[row.RowIndex].Value.ToString();
                    if (((CheckBox)row.FindControl("cbSelect")).Checked)
                    {
                        if (!origNameList.Contains(nameKey))
                            return true;
                    }
                    else
                    {
                        if (origNameList.Contains(nameKey))
                            return true;
                    }
                }
            }
            return false;
        }

        private bool CheckItemChanges(out List<string> itemChanges)
        {
            List<string> listOfChanges = new List<string>();
            List<string> origItemList = new List<string>();

            PLCQuery qryItemAssign = new PLCQuery("SELECT EVIDENCE_CONTROL_NUMBER FROM TV_ITASSIGN WHERE EXAM_KEY = " + PLCSession.PLCGlobalAssignmentKey);

            if (qryItemAssign.Open() && qryItemAssign.HasData())
            {
                while (!qryItemAssign.EOF())
                {
                    origItemList.Add(qryItemAssign.FieldByName("EVIDENCE_CONTROL_NUMBER"));
                    qryItemAssign.Next();
                }
            }

            foreach (GridViewRow row in gItems.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    string ecn = ((HiddenField)row.FindControl("hdnItemNameKey")).Value;
                    string itemnumber = ((TextBox)row.FindControl("ITEMNUM")).Text;

                    if (((CheckBox)row.FindControl("cbSELECT")).Checked)
                    {
                        //if (!origItemList.Contains(ecn))
                        //{
                        //	listOfChanges.Add(string.Format("ADDED ASSIGNMENT ITEM LINK --- LAB ITEM NUMBER: {0}", itemnumber));
                        //}
                    }
                    else
                    {
                        if (origItemList.Contains(ecn))
                        {
                            listOfChanges.Add(string.Format("DELETED ASSIGNMENT ITEM LINK --- LAB ITEM NUMBER: {0}", itemnumber));
                        }
                    }
                }
            }

            itemChanges = listOfChanges;

            return listOfChanges.Count > 0;
        }

        private bool ItemHasChanges()
        {
            List<string> origItemList = new List<string>();
            PLCQuery qryItemAssign = new PLCQuery("SELECT EVIDENCE_CONTROL_NUMBER FROM TV_ITASSIGN WHERE EXAM_KEY = " + PLCSession.PLCGlobalAssignmentKey);

            if (qryItemAssign.Open() && qryItemAssign.HasData())
            {
                while (!qryItemAssign.EOF())
                {
                    origItemList.Add(qryItemAssign.FieldByName("EVIDENCE_CONTROL_NUMBER"));
                    qryItemAssign.Next();
                }
            }

            foreach (GridViewRow row in gItems.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    string ecn = ((HiddenField)row.FindControl("hdnItemNameKey")).Value;
                    if (((CheckBox)row.FindControl("cbSELECT")).Checked)
                    {
                        if (!origItemList.Contains(ecn))
                            return true;
                    }
                    else
                    {
                        if (origItemList.Contains(ecn))
                            return true;
                    }
                }
            }
            return false;
        }

        private string PrepareCaseNarrative()
        {
            StringBuilder auditText = new StringBuilder();

            auditText.AppendLine(string.Format("EXAM KEY: {0} - SECTION: {1}",
                PLCSession.PLCGlobalAssignmentKey, PLCDBPanel1.GetFieldDesc("SECTION")));

            // Main Panel Changes
            string[] excludeField = {};
            if (PLCSession.GetLabCtrl("LOG_REPORT_FORMAT_CHANGE") == "T")
                excludeField = new string[] { "REPORT_FORMAT" };

            foreach (string fieldName in PLCDBPanel1.GetFieldNames())
            {
                PLCDBPanel.PanelRec panel = PLCDBPanel1.GetPanelRecByFieldName(fieldName);
                string fieldType = panel.fldtype;
                string mask = panel.editmask;
                string originalValue = PLCDBPanel1.GetOriginalValue(fieldName);
                string newValue = PLCDBPanel1.getpanelfield(fieldName);

                if (fieldType == "D")
                {
                    if (!string.IsNullOrEmpty(originalValue))
                        if (mask.ToUpper().StartsWith("DD/"))
                            originalValue = DateTime.Parse(originalValue, System.Globalization.CultureInfo.CreateSpecificCulture("en-GB")).ToString(
                                System.Globalization.CultureInfo.CreateSpecificCulture("en-GB"));
                        else
                            originalValue = DateTime.Parse(originalValue).ToShortDateString();
                    if (!string.IsNullOrEmpty(newValue))
                        if (mask.ToUpper().StartsWith("DD/"))
                            newValue = DateTime.Parse(newValue, System.Globalization.CultureInfo.CreateSpecificCulture("en-GB")).ToString(
                                System.Globalization.CultureInfo.CreateSpecificCulture("en-GB"));
                        else
                            newValue = DateTime.Parse(newValue).ToShortDateString();
                }
                else if (fieldType == "T")
                {
                    if (!string.IsNullOrEmpty(originalValue))
                        originalValue = DateTime.Parse(originalValue).ToShortTimeString();
                    if (!string.IsNullOrEmpty(newValue))
                        newValue = DateTime.Parse(newValue).ToShortTimeString();
                }

                if (((excludeField.Contains(fieldName) || !string.IsNullOrEmpty(originalValue)) && originalValue != newValue) || (excludeField.Contains(fieldName) || PLCSession.GetLabCtrl("LOG_BLANK_TO_DATA_CHANGES") == "T" && originalValue != newValue))
                {
                    auditText.AppendLine(PLCDBPanel1.GetPanelRecByFieldName(fieldName).prompt + " (" + fieldName + ") CHANGED");
                    if (fieldType == "D")
                        auditText.AppendLine("FROM: " + (originalValue == "" ? "-" : originalValue) +
                        " TO: " + (newValue == "" ? "-" : newValue));
                    else
                        auditText.AppendLine("FROM: " + (originalValue == "" ? "-" : PLCDBPanel1.GetOriginalDesc(fieldName)) +
                        " TO: " + (newValue == "" ? "-" : PLCDBPanel1.GetFieldDesc(fieldName)));
                }
            }

            // Assignment Item Changes
            if (ViewState["AssignItemChanges"] != null && ((List<string>)ViewState["AssignItemChanges"]).Count > 0)
            {
                auditText.AppendLine("ASSIGNMENT ITEM LINK CHANGES");

                foreach (string assignItemChange in (List<string>)ViewState["AssignItemChanges"])
                {
                    auditText.AppendLine(assignItemChange);
                }

                ViewState["AssignItemChanges"] = null;
            }

            // Assignment Name Changes
            if (ViewState["AssignNameChanges"] != null && ((List<string>)ViewState["AssignNameChanges"]).Count > 0)
            {
                auditText.AppendLine("ASSIGNMENT NAME LINK CHANGES");

                foreach (string assignNameChange in (List<string>)ViewState["AssignNameChanges"])
                {
                    auditText.AppendLine(assignNameChange);
                }

                ViewState["AssignNameChanges"] = null;
            }

            return auditText.ToString();
        }

        private void SaveConfirmUpdate(string auditLogText)
        {
            StringBuilder caseUpdateComments = new StringBuilder();
            caseUpdateComments.AppendLine("CASE UPDATED");
            caseUpdateComments.AppendLine("AUDIT LOG:");
            caseUpdateComments.Append(auditLogText);
            caseUpdateComments.Append("REASON FOR CHANGE: " + hdnConfirmUpdate.Value);
            AddSchedule("CE", caseUpdateComments.ToString()); // Used CE (Case event) for saving case updates
        }

        private void AddSchedule(string type, string scheduleComments)
        {
            int scheduleKey = PLCSession.GetNextSequence("LABSUPP_SEQ");

            PLCQuery qrySchedule = new PLCQuery();
            qrySchedule.SQL = "SELECT SCHEDULE_KEY, CASE_KEY, DATE_RES, TIME, TYPE_RES, COMMENTS, ANALYST FROM TV_SCHEDULE WHERE 0 = 1";
            qrySchedule.Open();
            qrySchedule.Append();
            qrySchedule.SetFieldValue("SCHEDULE_KEY", scheduleKey);
            qrySchedule.SetFieldValue("CASE_KEY", PLCSession.PLCGlobalCaseKey);
            qrySchedule.SetFieldValue("DATE_RES", DateTime.Now);
            qrySchedule.SetFieldValue("TIME", DateTime.Now);
            qrySchedule.SetFieldValue("TYPE_RES", type);
            qrySchedule.SetFieldValue("COMMENTS", scheduleComments);
            qrySchedule.SetFieldValue("ANALYST", PLCSession.PLCGlobalAnalyst);
            qrySchedule.Post("TV_SCHEDULE", 41, 3);
        }

        private void SyncLabAssignDueDate()
        {
            //PLCDBGlobal dBGlobal = new PLCDBGlobal();

            PLCQuery qryLabAssign = new PLCQuery();
            qryLabAssign.SQL = "SELECT * FROM TV_LABEXAM where EXAM_KEY = " + PLCSession.PLCGlobalAssignmentKey;
            if (qryLabAssign.Open() && qryLabAssign.HasData())
            {
                DateTime dtNewDueDate;
                if (string.IsNullOrEmpty(qryLabAssign.FieldByName("DUE_DATE")))
                {
                    dtNewDueDate = Convert.ToDateTime("01-01-1900");
                }
                else
                {
                    dtNewDueDate = Convert.ToDateTime(qryLabAssign.FieldByName("DUE_DATE"));
                }

                if (PLCDBGlobalClass.GetUpdatedDueDate(null, ref dtNewDueDate))
                {
                    qryLabAssign.Edit();
                    qryLabAssign.SetFieldValue("DUE_DATE", dtNewDueDate);
                    qryLabAssign.Post("TV_LABEXAM", 17, 1);
                    PLCDBPanel1.setpanelfield("DUE_DATE", dtNewDueDate.ToShortDateString());
                }
            }
        }

        private void SetAssignmentItemControlsMode(bool Enable, bool isAddEditMode)
        {
            if (gItems.HeaderRow != null)
                ((CheckBox)gItems.HeaderRow.FindControl("cbSELECT_All")).Enabled = Enable;

            foreach (GridViewRow GVR in gItems.Rows)
                if (GVR.RowType == DataControlRowType.DataRow)
                {
                    ((CheckBox)GVR.FindControl("cbSELECT")).Enabled = Enable;

                    if (PLCCommonClass.IsReadOnlyAccess("WEBINQ,RONLYASTAB"))
                        ((Button)GVR.FindControl("ADDTASK")).Enabled = false;
                }

            if (gvNameItem.HeaderRow != null)
                ((CheckBox)gvNameItem.HeaderRow.FindControl("cbSelect_All")).Enabled = Enable;

            foreach (GridViewRow row in gvNameItem.Rows)
                if (row.RowType == DataControlRowType.DataRow)
                    ((CheckBox)row.FindControl("cbSelect")).Enabled = Enable;

            if (isAddEditMode)
                Enable = !Enable;

            btnItems.Enabled = btnNames.Enabled = !Enable;

            //if (!PLCSession.CheckUserOption("DELASSGN"))
            //    PLCButtonPanel1.DisableButton("DELETE");
        }

        private void LoadAssignmentItems(string examKey)
        {
            assignedItems.Clear();
            NO_ITEM_ASSIGNMENT = true;
            // item_source, Section_Item_Number are for later (DNA)

            PLCQuery qryItems = new PLCQuery();
            qryItems.SQL = PLCSession.FormatSpecialFunctions(PLCDBGlobal.instance.GetItemSql(PLCSession.PLCGlobalCaseKey, examKey, false));
            qryItems.Open();

            DataSet DS = PLCSession.PLCGeneralDataSet;
            DataTable DT = new DataTable();

            if (DS.Tables.IndexOf("ASSIGNMENTITEMS") < 0)
            {
                DT = new DataTable("ASSIGNMENTITEMS");
                AddItemColsToDataSet(DT);
                DS.Tables.Add(DT);
                PLCSession.PLCGeneralDataSet = DS;
            }
            else
                DT = PLCSession.PLCGeneralDataSet.Tables["ASSIGNMENTITEMS"];
            DT.Rows.Clear();

            string itemqty = string.Empty;
            if (examKey == "0")
                itemqty = PLCSession.GetSectionFlag(PLCDBPanel1.getpanelfield("SECTION"), "DEFAULT_ITEM_QUANTITY");

            int I = 0;
            while (!qryItems.EOF())
            {
                DataRow DR = DT.Rows.Add();
                DR["ID"] = I.ToString();
                DR["ITEMNUM"] = qryItems.FieldByName("LAB_ITEM_NUMBER");
                DR["TRACKINGNUM"] = qryItems.FieldByName("TRACKING_NUMBER");
                DR["DEPTITEMNUM"] = qryItems.FieldByName("DEPARTMENT_ITEM_NUMBER");
                DR["THIRDPARTYBC"] = qryItems.FieldByName("THIRD_PARTY_BARCODE");
                DR["PACKTYPE"] = qryItems.FieldByName("PACKAGE_DESC");
                DR["ITEMTYPE"] = qryItems.FieldByName("ITEM_TYPE_DESC");
                DR["SERVICEREQUEST"] = "";
                DR["ECN"] = qryItems.FieldByName("EVIDENCE_CONTROL_NUMBER");
                DR["ITEMDESC"] = qryItems.FieldByName("Description");
                DR["QUANTITY"] = !string.IsNullOrEmpty(itemqty) && (DR["ITEMNUM"].ToString() != "0") ? itemqty : qryItems.FieldByName("QUANTITY");
                DR["SERVICEREQUEST"] = qryItems.FieldByName("Service_Request");
                DR["EXAM_KEY"] = qryItems.FieldByName("EXAM_KEY");
                DR["ITEM_TYPE"] = qryItems.FieldByName("ITEM_TYPE");
                DR["PACKAGING_CODE"] = qryItems.FieldByName("PACKAGING_CODE");
                DR["VOUCHERNUM"] = qryItems.FieldByName("VOUCHERNUM");
                DR["ITEMSOURCE"] = qryItems.FieldByName("ITEM_SOURCE");
                //
                if ((examKey == "0") || (qryItems.FieldByName("EXAM_KEY") != examKey))
                {
                    DR["SELECTED"] = "F";
                }
                else
                {
                    DR["SELECTED"] = "T";
                    if (qryItems.FieldByName("ITA_ITEM_DESCRIPTION").Trim() != "")
                        DR["ITEMDESC"] = qryItems.FieldByName("ITA_ITEM_DESCRIPTION");
                    DR["QUANTITY"] = !string.IsNullOrEmpty(itemqty) && (DR["ITEMNUM"].ToString() != "0") ? itemqty : qryItems.FieldByName("QUANTITY");
                    if (qryItems.FieldByName("REPTQTY").Trim() != "")
                        DR["QUANTITY"] = !string.IsNullOrEmpty(itemqty) && (DR["ITEMNUM"].ToString() != "0") ? itemqty : qryItems.FieldByName("REPTQTY");
                    //
                    if (NO_ITEM_ASSIGNMENT)
                        NO_ITEM_ASSIGNMENT = false;
                    if (!assignedItems.Contains(DR["ECN"].ToString()))
                        assignedItems.Add(DR["ECN"].ToString());
                }

                if ((PLCSession.GetLabCtrl("ITEM_TYPE_DESC_AS_REPT_DESC") == "T") && (DR["ITEMDESC"].ToString().Trim() == ""))
                    DR["ITEMDESC"] = qryItems.FieldByName("ITEM_TYPE_DESC");

                I++;
                qryItems.Next();
            }

            //third party barcode
            //            gvBigViewItems.Columns[2].Visible = PLCSession.GetLabCtrl("THIRD_PARTY_BC_IN_MATRIX") == "T";

            //department item number
            //            gvBigViewItems.Columns[3].Visible = PLCSession.GetLabCtrl("DEPT_ITEM_NUMBER_IN_MATRIX") == "T";

            if ((PLCSession.GetLabCtrl("RMS_EXTERNAL_IS_VOUCHER") != "T"))
            {
                gItems.Columns[8].Visible = false;
            }
            else
            {
                gItems.Columns[8].Visible = true;

            }


            gItems.Columns[2].Visible = PLCSession.GetLabCtrlFlag("SHOW_TRACKING_NUMBER") == "T";

            gItems.Columns[9].Visible = PLCSession.CheckUserOption("TASKASSIGN");

            gItems.DataSource = DT;
            gItems.DataBind();

            SetAssignmentItemControlsMode(false, false);

            bnBigView.Enabled = true;
        }

        private bool ValidateItems(string ExamKey)
        {
            if (PLCSession.PLCGeneralDataSet == null)
            {
                messageBox.ShowMsg("Error", "Assignment Item Data set not found ", 0);
                return false;
            }

            DataSet DS = PLCSession.PLCGeneralDataSet;

            if (DS.Tables.IndexOf("ASSIGNMENTITEMS") < 0)
            {
                messageBox.ShowMsg("Error", "Assignment Item Data set not found ", 0);
                return false;
            }

            //DataTable DT = PLCSession.PLCGeneralDataSet.Tables["ASSIGNMENTITEMS"];

            string Qty = "";

            foreach (GridViewRow GVR in gItems.Rows)
            {
                if (GVR.RowType == DataControlRowType.DataRow)
                {
                    if (((CheckBox)GVR.FindControl("cbSELECT")).Checked)
                    {
                        Qty = ((TextBox)GVR.FindControl("QUANTITY")).Text;
                        if (Qty.Trim() != "")
                        {
                            int iQty = PLCSession.SafeInt(Qty, -100);
                            if (iQty == -100)
                            {
                                messageBox.ShowMsg("Error", "Please enter the Quantity in numbers", 0);
                                ((TextBox)GVR.FindControl("QUANTITY")).Focus();
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        private void SaveAssignmentNames(string examKey, int changeKey)
        {
            string nameKey = string.Empty;
            bool selected = false;

            List<string> lstNameKeysOrig = new List<string>();
            List<string> lstNameKeysToDelete = new List<string>();
            List<string> lstNameKeysToAdd = new List<string>();

            PLCQuery qryNames = new PLCQuery("SELECT * FROM TV_REPTNAME WHERE EXAM_KEY = " + examKey);
            if (qryNames.Open() && qryNames.HasData())
            {
                while (!qryNames.EOF())
                {
                    lstNameKeysOrig.Add(qryNames.FieldByName("NAME_KEY"));
                    qryNames.Next();
                }
            }

            foreach (GridViewRow row in gvNameItem.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    nameKey = gvNameItem.DataKeys[row.RowIndex].Value.ToString();
                    selected = ((CheckBox)row.FindControl("cbSelect")).Checked;

                    // Check for existing list to determine what should be deleted and added (and to leave alone)
                    if (lstNameKeysOrig.Contains(nameKey))
                    {
                        if (!selected)
                            lstNameKeysToDelete.Add(nameKey);
                    }
                    else
                    {
                        if (selected)
                            lstNameKeysToAdd.Add(nameKey);
                    }
                }
            }

            // Delete name-assignment relationship (TV_REPTNAME)
            if (lstNameKeysToDelete.Count > 0)
            {
                string deletedNameKeys = string.Join(",", lstNameKeysToDelete.ToArray());

                PLCQuery qryDeleteNames = new PLCQuery(string.Format("DELETE FROM TV_REPTNAME WHERE EXAM_KEY = {0} AND NAME_KEY IN ({1})", examKey, deletedNameKeys));
                qryDeleteNames.ExecSQL();
                PLCDBGlobalClass.WriteAuditLog("37", "1", "", string.Format("Deleted in TV_REPTNAME: EXAM_KEY = {0}; NAME_KEY = {1}", examKey, deletedNameKeys), changeKey);
            }

            // Add new name-assignment relationship
            if (lstNameKeysToAdd.Count > 0)
            {
                foreach (string key in lstNameKeysToAdd)
                {
                    qryNames.Append();
                    qryNames.SetFieldValue("EXAM_KEY", examKey);
                    qryNames.SetFieldValue("NAME_KEY", key);
                    qryNames.Post("TV_REPTNAME", 7, 10);
                }
                PLCDBGlobalClass.WriteAuditLog("37", "2", "", string.Format("Added in TV_REPTNAME: EXAM_KEY = {0}; NAME_KEY = {1}",
                    examKey, string.Join(",", lstNameKeysToAdd.ToArray())), changeKey);
            }
        }

        private void SaveAssignmentItems(string ExamKey, int changeKey)
        {
            NO_ITEM_ASSIGNMENT = true;
            //
            if (PLCSession.PLCGeneralDataSet == null)
                return;

            DataSet DS = PLCSession.PLCGeneralDataSet;


            if (DS.Tables.IndexOf("ASSIGNMENTITEMS") < 0)
                return;
            DataTable DT = PLCSession.PLCGeneralDataSet.Tables["ASSIGNMENTITEMS"];

            bool uncheckedEcns = false;
            bool Selected = false;
            string ItemDesc = "";
            string Qty = "";
            int srMasterKey = 0;

            foreach (GridViewRow GVR in gItems.Rows)
            {
                if (GVR.RowType == DataControlRowType.DataRow)
                {
                    Selected = ((CheckBox)GVR.FindControl("cbSELECT")).Checked;
                    ItemDesc = ((TextBox)GVR.FindControl("ITEMDESC")).Text;
                    Qty = ((TextBox)GVR.FindControl("QUANTITY")).Text;
                    int ID = GVR.DataItemIndex;
                    for (int i = 0; i < DT.Rows.Count; i++)
                    {
                        DataRow DR = DT.Rows[i];
                        if (ID == Convert.ToInt32(DR["ID"]))
                        {
                            DR["SELECTED"] = "F";
                            if (Selected)
                            {
                                DR["SELECTED"] = "T";
                                DR["EXAM_KEY"] = ExamKey;
                                //
                                NO_ITEM_ASSIGNMENT = false;
                            }
                            DR["ITEMDESC"] = ItemDesc;
                            DR["QUANTITY"] = Qty;
                        }
                    }
                }
            }

            for (int i = 0; i < DT.Rows.Count; i++)
            {
                DataRow DR = DT.Rows[i];
                if (DR["SELECTED"].ToString() == "T") //gridrow is selected -> add record if it does not yet exist in ITASSIGN
                {
                    if (PLCDBGlobal.instance.IsItemNotInAssignment(Convert.ToInt32(ExamKey), Convert.ToInt32(DR["ECN"])))
                    {
                        string itemqty = PLCSession.GetSectionFlag(PLCDBPanel1.getpanelfield("SECTION"), "DEFAULT_ITEM_QUANTITY");

                        PLCDBGlobal.instance.AddItemToAssignment(new ItemAssignment
                        {
                            ExamKey = Convert.ToInt32(ExamKey),
                            ECN = Convert.ToInt32(DR["ECN"]),
                            ItemNumber = DR["ITEMNUM"].ToString(),
                            ItemType = DR["ITEM_TYPE"].ToString(),
                            PackagingCode = DR["PACKAGING_CODE"].ToString(),
                            ItemQuantity = !string.IsNullOrEmpty(itemqty) && (DR["ITEMNUM"].ToString() != "0") ? itemqty : DR["QUANTITY"].ToString(),
                            ItemDescription = DR["ITEMDESC"].ToString(),
                            ReportDescription = DR["ITEMDESC"].ToString()
                        });

                        PLCDBGlobal.instance.AddOrUpdateSRDetail(
                            Convert.ToInt32(ExamKey),
                            Convert.ToInt32(DR["ECN"]),
                            PLCDBPanel1.getpanelfield("SECTION"),
                            ref srMasterKey,
                            changeKey: changeKey);
                    }
                }
                else //gridrow is not selected -> delete record if it exists in ITASSIGN; delete existing AR results linked to the item
                {
                    if (!IsAssignAddMode && assignedItems.Contains(DR["ECN"].ToString()))
                    {
                        if (PLCSession.GetLabCtrl("USES_IMAGE_VAULT_SERVICE").Equals("T"))
                        {
                            GetItemAttachEntryIDs(new List<string> { DR["ECN"].ToString() });
                            DeleteAttachmentsOnServer();
                        }

                        uncheckedEcns = true;
                        CancelItemOpenTask(DR["ECN"].ToString(), changeKey);
                        DeleteItemAssignmentRecords(ExamKey, DR["ECN"].ToString(), changeKey);

                        PLCDBGlobal.instance.CancelLinkedSRDetail(
                            Convert.ToInt32(ExamKey),
                            Convert.ToInt32(DR["ECN"]),
                            ref srMasterKey,
                            changeKey: changeKey);
                    }
                }
            }

            // update TV_ARTBLREC sequences of unlinked items (after the DeleteItemAssignmentRecords)
            if (uncheckedEcns)
                PLCDBGlobal.instance.UpdateAssignItemsArTblRec();

            // Update service request
            if (srMasterKey > 0)
            {
                ServiceRequestMaster srMaster = new ServiceRequestMaster()
                {
                    SRMasterKey = srMasterKey,
                    Status = PLCDBGlobal.instance.GetSRMasterStatus(srMasterKey)
                };

                PLCDBGlobal.instance.UpdateSRMaster(srMaster, changeKey: changeKey);
            }
        }

        private void FindAssignment()
        {
            if (!Page.IsPostBack)
            {
                // call from Quick Create; Additional Submission; set selection
                string assignmentkey = PLCSession.PLCGlobalAssignmentKey;
                if ((assignmentkey != "") && ((assignmentkey != "0")) && (GridView1.Rows.Count > 0))
                {
                    for (int intPage = 0; intPage < GridView1.PageCount; intPage++)
                    {
                        GridView1.PageIndex = intPage;
                        GridView1.DataBind();
                        for (int i = 0; i < GridView1.DataKeys.Count; i++)
                        {
                            if (Convert.ToString(GridView1.DataKeys[i].Value) == assignmentkey)
                            {
                                // If it is a match set the variables and exit
                                GridView1.SelectedIndex = i;
                                GridView1.PageIndex = intPage;
                                GridView1.SetClientSideScrollToSelectedRow();
                                GrabGridRecord();
                                return;
                            }
                        }
                    }
                }
            }
        }

        private Control FindControlWithin(Control ctrl, string id)
        {
            if (ctrl.ID == id)
                return ctrl;
            else
            {
                // Look for control within heirarchy of each child.
                foreach (Control childControl in ctrl.Controls)
                {
                    Control foundCtrl = FindControlWithin(childControl, id);
                    if (foundCtrl != null)
                        return foundCtrl;
                }

                return null;
            }
        }

        private void SaveAssignmentTasks(List<Task> tasksList, int changeKey)
        {
            foreach (Task task in tasksList)
            {
                int CurrentNum = 1;
                PLCQuery qryTaskList = new PLCQuery(
                    string.Format("SELECT TASK_NUM FROM TV_TASKLIST WHERE CASE_KEY = {0} " +
                    "AND EVIDENCE_CONTROL_NUMBER = {1} AND EXAM_KEY = {2} ORDER BY TASK_NUM DESC",
                    task.CaseKey, task.ECN, PLCSession.PLCGlobalAssignmentKey));

                if (qryTaskList.Open())
                {
                    if (qryTaskList.HasData())
                        CurrentNum = Convert.ToInt32(qryTaskList.FieldByName("TASK_NUM")) + 1;

                    string NewTaskID = PLCSession.GetNextSequence("TASKLIST_SEQ").ToString();

                    PLCQuery qryTaskListAppend = new PLCQuery("SELECT * FROM TV_TASKLIST WHERE 0=1");
                    qryTaskListAppend.Open();
                    qryTaskListAppend.Append();
                    qryTaskListAppend.SetFieldValue("TASK_ID", NewTaskID);
                    qryTaskListAppend.SetFieldValue("TASK_NUM", CurrentNum);
                    qryTaskListAppend.SetFieldValue("ITEM_TYPE", task.ItemType);
                    qryTaskListAppend.SetFieldValue("CASE_KEY", task.CaseKey);
                    qryTaskListAppend.SetFieldValue("DESCRIPTION", task.Description);
                    qryTaskListAppend.SetFieldValue("EVIDENCE_CONTROL_NUMBER", task.ECN);
                    qryTaskListAppend.SetFieldValue("EXAM_KEY", PLCSession.PLCGlobalAssignmentKey);
                    qryTaskListAppend.SetFieldValue("PRIORITY", task.Priority);
                    qryTaskListAppend.SetFieldValue("REQUESTED_DATE", DateTime.Today);
                    qryTaskListAppend.SetFieldValue("REQUESTED_BY", PLCSession.PLCGlobalAnalyst);
                    qryTaskListAppend.SetFieldValue("SECTION", PLCDBPanel1.getpanelfield("SECTION"));
                    qryTaskListAppend.SetFieldValue("STATUS", task.Status);
                    qryTaskListAppend.SetFieldValue("TASK_TYPE", task.TaskType);
                    qryTaskListAppend.SetFieldValue("ITEM_SOURCE", PLCDBGlobal.instance.GetItemSource(Convert.ToInt32(PLCSession.PLCGlobalAssignmentKey), Convert.ToInt32(task.ECN)));
                    if (hdnAutoSyncAnalyst.Value == "T")
                    {
                        qryTaskListAppend.SetFieldValue("ANALYST", task.Analyst);
                    }
                    qryTaskListAppend.SetFieldValue("DATE_ASSIGNED", DateTime.Now);
                    qryTaskListAppend.SetFieldValue("TASK_CREATED_DATE", DateTime.Now);
                    qryTaskListAppend.Post("TV_TASKLIST", 52, 3, changeKey);
                }
            }
        }

        private void DisplayAssignTaskPopup()
        {
            string section = PLCDBPanel1.getpanelfield("SECTION").Trim();
            if (!string.IsNullOrEmpty(section))
            {
                //Check for auto sync
                PLCQuery qryAuto = new PLCQuery("SELECT AUTO_SYNC_TASK_ANALYST, AUTO_SYNC_TASK_PRIORITY FROM TV_EXAMCODE WHERE EXAM_CODE = '" + section + "'");
                qryAuto.Open();
                if (qryAuto.HasData())
                {
                    qryAuto.First();
                    hdnAutoSyncAnalyst.Value = qryAuto.FieldByName("AUTO_SYNC_TASK_ANALYST").Trim();
                    if (qryAuto.FieldByName("AUTO_SYNC_TASK_PRIORITY").Trim() == "T")
                    {
                        string priority = PLCDBPanel1.getpanelfield("PRIORITY").Trim();
                        FlexBox fbAssignTaskPriority;

                        //Set dropdown for priority
                        foreach (GridViewRow row in gvAssignTask.Rows)
                        {
                            fbAssignTaskPriority = (FlexBox)row.FindControl("fbAssignTaskPriority");
                            if (fbAssignTaskPriority != null && priority != string.Empty)
                            {
                                fbAssignTaskPriority.SetValue(priority);
                            }
                        }
                    }
                }
                else
                {
                    //clear hidden field in case it has a previous value
                    hdnAutoSyncAnalyst.Value = string.Empty;
                }
            }

            mpeAssignTask.Show();
        }

        private bool QMSTaskFilterAllow(string TaskType, string Analyst)
        {
            if (PLCSession.GetLabCtrl("USES_QMS_TASK_FILTER") != "T")
            {
                return true;
            }

            string sql = string.Empty;

            if (PLCSession.PLCDatabaseServer == "MSSQL")
            {
                sql =
                    @"SELECT DISTINCT T.TASK_CODE AS TASKTYPE
FROM TV_QMSSECTT S, TV_QMSSTASK T
WHERE T.SECTION = S.SECTION
AND T.TASK_CODE = '" + TaskType + @"'
AND T.TEST_CODE = S.TEST_CODE
AND S.TEST_CODE IN (
  SELECT A.TEST_CODE FROM TV_QMSANALT A, TV_QMSRSLT R
  WHERE A.SECTION = S.SECTION
  AND A.ANALYST = '" + Analyst + @"'
  AND R.CODE = A.FINAL_RESULT AND R.PASSED = 'T'
  AND CONVERT(DATETIME, FLOOR(CONVERT(FLOAT, (A.TEST_COMPLETED + ISNULL(S.DAYS_VALID, 0))))) 
	>=  CONVERT(DATETIME, FLOOR(CONVERT(FLOAT, GETDATE())))
)";
            }
            else
            {
                sql =
                    @"SELECT DISTINCT T.TASK_CODE AS TASKTYPE 
FROM TV_QMSSECTT S, TV_QMSSTASK T
WHERE T.SECTION = S.SECTION
AND T.TASK_CODE = '" + TaskType + @"'
AND T.TEST_CODE = S.TEST_CODE
AND S.TEST_CODE IN (
  SELECT AN.TEST_CODE FROM TV_QMSANALT AN, TV_QMSRSLT R
  WHERE AN.SECTION = S.SECTION
  AND AN.ANALYST = '" + Analyst +  @"'
  AND R.CODE = AN.FINAL_RESULT AND R.PASSED = 'T'
  AND (to_date(to_char(AN.TEST_COMPLETED, 'DD-MON-YY')) + NVL(S.DAYS_VALID, 0)) 
  >=  to_date(to_char(SYSDATE, 'DD-MON-YY'))
)";
            }

            PLCQuery qryQMS = new PLCQuery(sql);
            qryQMS.Open();

            if (qryQMS.HasData())
            {
                if (qryQMS.FieldByName("TASKTYPE") == TaskType)
                    return true;
            }

            return false;
        }

        // Error message displayed when the assignment has been changed and may not be processed.
        // This can occur when an assignment has been set to 'Complete' in another session and the current page grid hasn't refreshed yet.
        private void ShowAssignmentChangedError()
        {
            Button btnOK = (Button)FindControlWithin(this.plhMessageBox, "OkButton");
            if (btnOK != null)
                btnOK.Attributes["onclick"] = "OnOKAssignmentChangedError();";

            messageBox.ShowMsg("Assignment", "Currently selected assignment has been changed.", 0);
        }

        private bool HasAssignmentReportsAccess()
        {
            //true if user is the assigned analyst or an additional analyst or if user has EDIT_REPORTS_FOR_OTHERS permission for current section

            return PLCSession.PLCGlobalAnalyst == PLCDBPanel1.getpanelfield("ANALYST_ASSIGNED").Trim() ||
                PLCDBGlobal.instance.IsAdditionalAnalyst(PLCSession.PLCGlobalAssignmentKey, PLCSession.PLCGlobalAnalyst) ||
                PLCSession.GetUserAnalSect(PLCDBPanel1.getpanelfield("SECTION").Trim(), "EDIT_REPORTS_FOR_OTHERS").ToString() == "T";
        }

        private bool CanWriteAssignment()
        {
            Dictionary<string, object> AnalystSection = GetAnalystSection();

            if (AnalystSection.Count() > 0)
            {
                if (AnalystSection.ContainsKey(PLCDBPanel1.getpanelfield("SECTION").Trim()))
                {
                    Dictionary<string, string> section = (Dictionary<string, string>)AnalystSection[PLCDBPanel1.getpanelfield("SECTION").Trim()];
                    if (section.ContainsKey("WRITE"))
                    {
                        if (section["WRITE"].ToString().Trim() != "T")
                            return false;
                    }
                }
            }

            return true;
        }

        private bool IsTrueSectionUsesPrintManagerList()
        {
            int selectedIndex = GridView1.SelectedIndex;
            //idViewRow  row = GridView1.Rows[1];
            //ring a = row["a"].tostring();
            int cellIndex = GetColumnIndexOf(GridView1, "Section");
            string section = GridView1.Rows[selectedIndex].Cells[cellIndex + 1].Text;
            section = StripTagsRegex(section);
            //GridView1.Columns[1].HeaderText
            PLCQuery qryPrintManager = new PLCQuery();
            qryPrintManager.SQL = "select USES_PRINT_MANAGER from tv_examcode where EXAM_CODE = '" + section + "'";
            qryPrintManager.Open();
            string usesPrintManger = "";
            if (!qryPrintManager.IsEmpty())
            {
                usesPrintManger = qryPrintManager.FieldByName("USES_PRINT_MANAGER").ToString();
                if (usesPrintManger == "L")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        //To get the cell number when column name is given
        private int GetColumnIndexOf(GridView gv, string ColumnName)
        {
            try
            {
                int columnID = 0;
                // Loop all the columns   
                var DataControlFieldsList = gv.Columns.OfType<DataControlField>().ToList().
                                             Where(t => t.HeaderText.ToUpper().Trim() == ColumnName.ToUpper().Trim() ||
                                             t.AccessibleHeaderText.ToUpper().Trim() == ColumnName.ToUpper().Trim()).ToList();
                if (DataControlFieldsList != null)
                {
                    if (DataControlFieldsList.Count > 0)
                    {
                        columnID = gv.Columns.IndexOf(DataControlFieldsList.First());
                    }
                }
                return columnID;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private string StripTagsRegex(string source)
        {
            return Regex.Replace(source, "<.*?>", string.Empty);
        }

        private bool IsProcessApproved()
        {
            int selectedIndex = GridView1.SelectedIndex;
            //string status = GridView1.Rows[selectedIndex].Cells[6].Text;
            //status = StripTagsRegex(status);
            //string status = "F";
            string examKey = GridView1.SelectedDataKey.Values["EXAM_KEY"].ToString();


            PLCQuery qryCount = new PLCQuery();
            //qryCount.SQL = "select count(PRINTLOG_KEY) as COUNT from tv_printlog where " +
            //"FILE_SOURCE_KEY1 = '" + examKey + "' and IMAGE_SOURCE = 'SIGN' and CASE_NOTES = 'T' and STATUS = '" + status + "'";
            //qryCount.SQL = "select count(*) as COUNT from TV_PDFDATA where " +
            // "FILE_SOURCE_KEY = '" + examKey + "' and FILE_SOURCE = 'NOTES' and STATUS = '" + status + "'";

            qryCount.SQL = "select status from TV_PDFDATA where " +
              "FILE_SOURCE_KEY = '" + examKey + "' and FILE_SOURCE = 'NOTES'";

            qryCount.Open();

            string countStr = "";
            if (!qryCount.IsEmpty())
            {
                countStr = qryCount.FieldByName("STATUS").ToString();

            }
            if (countStr == "T")
                return true;
            else
            {
                qryCount.SQL = "select * from TV_PRTWODOC WHERE EXAM_KEY = " + examKey;
                qryCount.Open();
                if (!qryCount.IsEmpty())
                    return qryCount.FieldByName("NO_DOCUMENT") == "T";
                else
                    return false;
            }
        }

        private bool UserCanRoute(string currentUser, string section)
        {
            if (PLCSession.GetLabCtrl("USES_ITEM_ROUTING") != "T")
                return false;

            if (PLCDBPanel1.getpanelfield("ANALYST_ASSIGNED") == currentUser)
                return true;

            PLCQuery qrySectionPermissions = new PLCQuery();
            qrySectionPermissions.SQL = "SELECT CAN_ROUTE_ITEMS FROM TV_ANALSECT WHERE ANALYST = '" + currentUser + "' AND SECTION = '" + section + "'";
            qrySectionPermissions.Open();
            if (!qrySectionPermissions.IsEmpty())
                return qrySectionPermissions.FieldByName("CAN_ROUTE_ITEMS") == "T";

            return false;
        }

        private bool IsLockedSignedReportsButton(string sExamKey, string sStatus)
        {
            if ((PLCSession.GetLabCtrl("LOCKED_SIGNED_REPORTS") == "T") &&
                ((sStatus == PLCDBGlobalClass.GetExamStatusCode("V", "3")) || (sStatus == PLCDBGlobalClass.GetExamStatusCode("R", "4")) || (sStatus == PLCDBGlobalClass.GetExamStatusCode("C", "5"))))
            {
                PLCQuery qryReptSig = new PLCQuery("SELECT * FROM TV_REPTSIG WHERE EXAM_KEY = ?");
                qryReptSig.AddParameter("EXAM_KEY", sExamKey);
                qryReptSig.Open();
                if (!qryReptSig.IsEmpty())
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        private void EnableLink()
        {
            foreach (Control ctrl in pnlLinkButtons.Controls)
            {
                if (ctrl is LinkButton)
                {
                    LinkButton lbtn = (LinkButton)ctrl;
                    lbtn.Enabled = true;
                }
            }
        }

        private void DisableLink(string linkname)
        {
            foreach (Control ctrl in pnlLinkButtons.Controls)
            {
                if (ctrl is LinkButton)
                {
                    LinkButton lbtn = (LinkButton)ctrl;
                    if (lbtn.ClientID.IndexOf(linkname) > -1)
                    {
                        lbtn.Enabled = true;
                    }
                    else
                    {
                        lbtn.Enabled = false;
                    }
                }
            }
        }

        private void SetCloseCodeControls()
        {
            //completion code popup
            lblURNText.Text = PLCSession.GetSysPrompt("LABCTRL.DEPT_CASE_TEXT", PLCSession.GetLabCtrl("DEPT_CASE_TEXT"));

            if (PLCSession.GetLabCtrl("USES_CLOSE_CODES") == "T")
            {
                plhCloseCode.Visible = true;
                lblError.Text = "";
                chCompletionCode = new CodeHead();
                chCompletionCode = (CodeHead)LoadControl("~/PLCWebCommon/CodeHead.ascx");
                chCompletionCode.TableName = "TV_CLOSECODE";
                chCompletionCode.RequiredValidationGroup = "AdminClose";
                chCompletionCode.PopupX = 20;
                chCompletionCode.PopupY = 30;
                chCompletionCode.ControlWidth = 255;
                chCompletionCode.ValueChanged += new ValueChangedEventHandler(chCompletionCode_ValueChanged);
                if (PLCSession.PLCDatabaseServer == "ORACLE")
                    chCompletionCode.CodeCondition = "(' ' || REPLACE(SECTION, ',', ' ') || ' ' LIKE '% " + PLCDBPanel1.getpanelfield("SECTION") + " %' OR SECTION IS NULL OR SECTION = '') AND COALESCE(UPPER(ACTIVE), 'T') <> 'F'";
                else
                    chCompletionCode.CodeCondition = "(' ' + REPLACE(SECTION, ',', ' ') + ' ' LIKE '% " + PLCDBPanel1.getpanelfield("SECTION") + " %' OR SECTION IS NULL OR SECTION = '') AND COALESCE(ACTIVE, 'T') <> 'F'";

                plhCompletionCode.Controls.Add(chCompletionCode);
            }

            //
            chUserID = new CodeHead();
            chUserID = (CodeHead)LoadControl("~/PLCWebCommon/CodeHead.ascx");
            chUserID.TableName = "TV_ANALYST";
            chUserID.PopupX = 420;
            chUserID.PopupY = 10;
            phUserID.Controls.Add(chUserID);
            //
            chRoutingCodes = new CodeHead();
            chRoutingCodes = (CodeHead)LoadControl("~/PLCWebCommon/CodeHead.ascx");
            chRoutingCodes.TableName = "TV_ROUTECOD";
            chRoutingCodes.PopupX = 25;
            chRoutingCodes.PopupY = 0;

            AjaxControlToolkit.ModalPopupExtender mpeCodeHead = (AjaxControlToolkit.ModalPopupExtender)chRoutingCodes.FindControl("mpeCodeHead");
            if (mpeCodeHead != null)
            {
                mpeCodeHead.PopupDragHandleControlID = "";
                mpeCodeHead.RepositionMode = AjaxControlToolkit.ModalPopupRepositionMode.None;
                mpeCodeHead.BackgroundCssClass = "modalBackground mpeBackgroundElem";
            }

            phRoutingCodes.Controls.Add(chRoutingCodes);
        }

        private void ShowErrorMessage(string message)
        {
            mpeAdminClose.Show();
            lblError.Visible = true;
            lblError.Text = message;
        }

        private Dictionary<string, object> GetAnalystSection()
        {
            //PLCDBGlobal plcdbglobal = new PLCDBGlobal();
            Dictionary<string, object> AnalystSection = new Dictionary<string, object>();

            if (ViewState["TAB6Assignments_AnalystSection"] != null)
            {
                AnalystSection = (Dictionary<string, object>)ViewState["TAB6Assignments_AnalystSection"];
            }
            else
            {
                AnalystSection = (Dictionary<string, object>)PLCDBGlobalClass.GetAnalystSectionInfo(PLCSession.PLCGlobalAnalyst);
                ViewState["TAB6Assignments_AnalystSection"] = AnalystSection;
            }

            return AnalystSection;
        }

        private string GenerateReportDigitalSig(string sReportKey, string sSigID, byte[] baSig)
        {
            string sDigitalSig = string.Empty;

            if (PLCSession.GetLabCtrl("PRIVATE_KEY_LOADED") == "T")
            {
                // Do streaming logic?
            }

            return sDigitalSig;
        }

        private void GetSubmExtraInfoHighlight(GridViewRowEventArgs Item, String ItemDataKey)
        {
            if (PLCSession.GetLabCtrl("USES_EXTRA_INFO_HIGHLIGHT") == "T")
            {
                if (ViewState["SUBM_EXTRA_INFO_TABLE"] != null)
                {
                    string[] ItemCollection = ((string)ViewState["SUBM_EXTRA_INFO_TABLE"]).Split(',');
                    if (ItemCollection.Length > 0)
                    {
                        foreach (String item in ItemCollection)
                        {
                            if (item == ItemDataKey)
                            {
                                foreach (Control ctrl in Item.Row.Controls)
                                {
                                    foreach (Control ItemCtrl in ctrl.Controls)
                                    {
                                        if (ItemCtrl is TextBox)
                                        {
                                            TextBox tbx = ((TextBox)ItemCtrl);
                                            tbx.BackColor = System.Drawing.Color.Transparent;
                                        }
                                    }
                                }
                                //                            
                                Item.Row.Style["background-color"] = null;
                                Item.Row.CssClass = "submextrainfoHL";
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void CheckSubmExtraInfoHighlight()
        {
            if (PLCSession.GetLabCtrl("USES_EXTRA_INFO_HIGHLIGHT") == "T")
            {
                // get subm extra info table
                if (!String.IsNullOrEmpty(PLCSession.GetLabCtrl("SUBM_EXTRA_INFO_TABLE")))
                {
                    PLCQuery qryHighlight = new PLCQuery();
                    //check if the table exists
                    qryHighlight.SQL = "SELECT X.SUBMISSION_KEY FROM " + PLCSession.GetLabCtrl("SUBM_EXTRA_INFO_TABLE") +
                        " X INNER JOIN TV_LABSUB S ON S.SUBMISSION_KEY = X.SUBMISSION_KEY WHERE S.CASE_KEY = " + PLCSession.PLCGlobalCaseKey;
                    qryHighlight.Open();
                    if (qryHighlight.PLCDataTable != null)
                    {
                        string SUBMISSION_KEYS = string.Empty;
                        foreach (DataRow row in qryHighlight.PLCDataTable.Rows)
                        {
                            if (SUBMISSION_KEYS.Length > 0)
                                SUBMISSION_KEYS += ",";
                            //
                            SUBMISSION_KEYS += row["SUBMISSION_KEY"].ToString();
                        }
                        //
                        if (SUBMISSION_KEYS.Length > 0)
                        {
                            //get all items given the submission keys
                            qryHighlight = new PLCQuery();
                            qryHighlight.SQL = "SELECT EVIDENCE_CONTROL_NUMBER AS ECN FROM TV_SUBLINK WHERE SUBMISSION_KEY IN (" + SUBMISSION_KEYS + ")";
                            qryHighlight.Open();
                            if (!qryHighlight.IsEmpty())
                            {
                                string ECN = string.Empty;
                                foreach (DataRow row in qryHighlight.PLCDataTable.Rows)
                                {
                                    if (ECN.Length > 0)
                                        ECN += ",";
                                    //
                                    ECN += row["ECN"].ToString();
                                }
                                //
                                //PLCSession.SetDefault("SUBM_EXTRA_INFO_HIGHLIGHT", ECN);
                                ViewState["SUBM_EXTRA_INFO_TABLE"] = ECN;
                            }
                        }
                    }
                }
            }
        }

        private void DeleteAssignmentTaskList(string ExamKey)
        {
            PLCQuery qryTaskList = new PLCQuery();
            PLCQuery qrytaskDelete = new PLCQuery();
            //PLCDBGlobal dbg = new PLCDBGlobal();

            // auditlog before deleting
            qryTaskList.SQL = "SELECT * FROM TV_TASKLIST WHERE EXAM_KEY = " + ExamKey;
            qryTaskList.Open();
            while (!qryTaskList.EOF())
            {
                string AudtiInfo = "TASK_ID: " + qryTaskList.FieldByName("TASK_ID") + "\r\n";
                AudtiInfo += "TASK_TYPE " + qryTaskList.FieldByName("TASK_TYPE") + "\r\n";
                AudtiInfo += "ITEM_TYPE: " + qryTaskList.FieldByName("ITEM_TYPE") + "\r\n";
                AudtiInfo += "DESCRIPTION: " + qryTaskList.FieldByName("DESCRIPTION") + "\r\n";
                AudtiInfo += "STATUS: " + qryTaskList.FieldByName("STATUS") + "\r\n";
                AudtiInfo += "ANALYST: " + qryTaskList.FieldByName("ANALYST") + "\r\n";
                AudtiInfo += "SECTION: " + qryTaskList.FieldByName("SECTION") + "\r\n";
                AudtiInfo += "DATE_ASSIGNED: " + qryTaskList.FieldByName("DATE_ASSIGNED") + "\r\n";
                AudtiInfo += "CASE_KEY: " + qryTaskList.FieldByName("CASE_KEY") + "\r\n";
                AudtiInfo += "ECN: " + qryTaskList.FieldByName("EVIDENCE_CONTROL_NUMBER") + "\r\n";
                AudtiInfo += "TASK_NUM: " + qryTaskList.FieldByName("TASK_NUM") + "\r\n";
                AudtiInfo += "PRIORITY: " + qryTaskList.FieldByName("PRIORITY") + "\r\n";
                AudtiInfo += "EXAM_KEY: " + qryTaskList.FieldByName("EXAM_KEY") + "\r\n";
                AudtiInfo += "REQUESTED_DATE: " + qryTaskList.FieldByName("REQUESTED_DATE") + "\r\n";
                AudtiInfo += "REQUESTED_BY: " + qryTaskList.FieldByName("REQUESTED_BY");
                PLCDBGlobalClass.WriteAuditLog("52", "2", "", AudtiInfo);
                AudtiInfo = "";
                qryTaskList.Next();
            }
            qrytaskDelete.SQL = "DELETE FROM TV_TASKLIST WHERE EXAM_KEY = " + ExamKey;
            qrytaskDelete.ExecSQL("");
        }

        private void SetLinkedCasesDropDown(bool isEnabled)
        {
            /*
                        //Get the selected value from the ItemSource DropDownList
                        string itemSourceValue = "";
                        if (ddItemSource.SelectedItem != null)
                        {
                            itemSourceValue = ddItemSource.SelectedItem.Value;
                        }

                        //Add the remaining values which should be put in the GridView to a List
                        List<string> itemSourceList = new List<string>();
                        foreach (ListItem lItem in ddItemSource.Items)
                        {
                            if (!itemSourceList.Contains(lItem.Value))
                            {
                                itemSourceList.Add(lItem.Value.Trim());
                            }
                        }

                        //cbSELECT
                        foreach (RepeaterItem item in repBigViewLink.Items)
                        {
                            if (item.FindControl("grdBigViewLink") != null)
                            {
                                GridView grid = (GridView)item.FindControl("grdBigViewLink");
                                foreach (GridViewRow row in grid.Rows)
                                {
                                    if (row.FindControl("cbSELECT") != null)
                                    {
                                        CheckBox cbox = (CheckBox)row.FindControl("cbSELECT");
                                        cbox.Enabled = isEnabled;

                                        DropDownList ddown = (DropDownList)row.FindControl("ddItemSourceField");
                                        ddown.Enabled = (isEnabled && cbox.Checked);
                                        if (ddown.Enabled)
                                        {
                                            if (itemSourceList.Count == 0)
                                            {
                                                ddown.Enabled = false;
                                            }
                                            else
                                            {
                                                foreach (string liValue in itemSourceList)
                                                {
                                                    if (ddown.Items.FindByValue(liValue.Trim()) == null)
                                                    {
                                                        if (liValue == "Please Select")
                                                        {
                                                            ddown.Items.Add(liValue);
                                                        }
                                                        else
                                                        {
                                                            ddown.Items.Add(new ListItem(liValue.Trim(), liValue.Trim()));
                                                        }
                                                    }
                                                }

                                                string itemSource = ((HiddenField)row.FindControl("hdnITEM_SOURCE")).Value.Trim();
                                                if (itemSource != string.Empty && ddown.Items.FindByValue(itemSource) != null)
                                                    ddown.SelectedValue = itemSource;
                                                else
                                                    ddown.SelectedValue = itemSourceValue;
                                            }
                                        }
                                        else
                                        {
                                            string itemSource = ((HiddenField)row.FindControl("hdnITEM_SOURCE")).Value.Trim();
                                            if (itemSource != string.Empty)
                                            {
                                                foreach (string liValue in itemSourceList)
                                                {
                                                    if (ddown.Items.FindByValue(liValue.Trim()) == null)
                                                    {
                                                        if (liValue == "Please Select")
                                                        {
                                                            ddown.Items.Add(liValue);
                                                        }
                                                        else
                                                        {
                                                            ddown.Items.Add(new ListItem(liValue.Trim(), liValue.Trim()));
                                                        }
                                                    }
                                                }
                                                ddown.SelectedValue = itemSource;
                                            }
                                            else
                                            {
                                                ddown.SelectedValue = itemSourceValue;
                                            }
                                        }
                                    }
                                }
                            }
                        }
*/
                    }

                    private void HideGridViewColumns()
                    {
                        List<string> GridViewColumnsToHide = new List<string>();
                        GridViewColumnsToHide.Add("CASE_KEY");
                        GridViewColumnsToHide.Add("EXAM_KEY");
                        //
                        foreach (String ColumnToHide in GridViewColumnsToHide)
                        {
                            foreach (BoundField col in GridView1.Columns)
                            {
                                if (col.DataField == ColumnToHide)
                                {
                                    col.Visible = false;
                                    break;
                                }
                            }
                        }
                    }

                    private bool DoesAssignmentHaveItemsChecked()
                    {
                        bool bItemSelected = false;
                        foreach (GridViewRow GVR in gItems.Rows)
                        {
                            if (GVR.RowType == DataControlRowType.DataRow)
                            {
                                if (((CheckBox)GVR.FindControl("cbSELECT")).Checked)
                                {
                                    bItemSelected = true;
                                    break;
                                }
                            }
                        }
                        return bItemSelected;
                    }

                    private string ItemsWithARResults(GridView gridView)
                    {
                        string conflictItemDesc = "";
                        List<string> uncheckedECNs = new List<string>();

                        foreach (GridViewRow GVR in gridView.Rows)
                        {
                            if (!((CheckBox)GVR.FindControl("cbSELECT")).Checked)
                                uncheckedECNs.Add(gridView.DataKeys[GVR.DataItemIndex].Value.ToString());
                        }

                        //query fails if all the items are selected...
                        if (uncheckedECNs.Count() == 0)
                            return "";

                        //check if there are AR (Assignment Report) records related to the item excluded from the assignment
                        //and if there is a description entered for the AssignmentItem through a Matrix Description panel
                        string sql = string.Format(@"SELECT IA.EVIDENCE_CONTROL_NUMBER, LAB_ITEM_NUMBER, ITEM_DESCRIPTION FROM TV_ITASSIGN IA
            INNER JOIN TV_LABITEM I ON IA.EVIDENCE_CONTROL_NUMBER = I.EVIDENCE_CONTROL_NUMBER
            WHERE IA.EXAM_KEY = {0} AND IA.EVIDENCE_CONTROL_NUMBER IN ({1})
            AND (SELECT COUNT(RESULT_KEY) AS RESULTCOUNT FROM TV_ARRESULT 
                WHERE EXAM_KEY = IA.EXAM_KEY
                AND EVIDENCE_CONTROL_NUMBER = IA.EVIDENCE_CONTROL_NUMBER) > 0", PLCSession.PLCGlobalAssignmentKey, string.Join(",", uncheckedECNs.ToArray()));
                        PLCQuery qryARItem = new PLCQuery(sql);
                        qryARItem.Open();
                        if (!qryARItem.IsEmpty())
                        {
                            while (!qryARItem.EOF())
                            {
                                string itemNumber = qryARItem.FieldByName("LAB_ITEM_NUMBER");
                                string description = qryARItem.FieldByName("ITEM_DESCRIPTION");
                                conflictItemDesc += "<li class='blt'>" + itemNumber + " - " + description + "</li>";

                                qryARItem.Next();
                            }
                        }

                        return conflictItemDesc;
                    }

                    private void DeleteItemAssignmentRecords(string examKey, string ecn, int changeKey)
                    {
                        PLCSession.WriteDebug("@assignmentsTab: Procedure call for DELETE_ASSIGNMENT_ITEM for assigned item ECN# " + ecn + ".", true);

                        try
                        {
                            if (PLCSession.PLCDatabaseServer == "MSSQL")
                            {
                                PLCQuery qryDeleteCase = new PLCQuery();
                                qryDeleteCase.AddProcedureParameter("examKey", Convert.ToInt32(PLCSession.PLCGlobalAssignmentKey), 10, OleDbType.Integer, ParameterDirection.Input);
                                qryDeleteCase.AddProcedureParameter("caseKey", Convert.ToInt32(PLCSession.PLCGlobalCaseKey), 10, OleDbType.Integer, ParameterDirection.Input);
                                qryDeleteCase.AddProcedureParameter("deleteReason", hdnConfirmUpdate.Value, hdnConfirmUpdate.Value.Length, OleDbType.VarChar, ParameterDirection.Input);
                                qryDeleteCase.AddProcedureParameter("userId", PLCSession.PLCGlobalAnalyst, 15, OleDbType.VarChar, ParameterDirection.Input);
                                qryDeleteCase.AddProcedureParameter("program", "iLIMS" + PLCSession.PLCBEASTiLIMSVersion, 8, OleDbType.VarChar, ParameterDirection.Input);
                                qryDeleteCase.AddProcedureParameter("OSComputerName", PLCSession.GetOSComputerName(), 50, OleDbType.VarChar, ParameterDirection.Input);
                                qryDeleteCase.AddProcedureParameter("OSUserName", PLCSession.GetOSUserName(), 50, OleDbType.VarChar, ParameterDirection.Input);
                                qryDeleteCase.AddProcedureParameter("OSAddress", PLCSession.GetOSAddress(), 50, OleDbType.VarChar, ParameterDirection.Input);
                                qryDeleteCase.AddProcedureParameter("BuildNumber", PLCSession.PLCBEASTiLIMSVersion, 100, OleDbType.VarChar, ParameterDirection.Input);
                                qryDeleteCase.AddProcedureParameter("labCode", PLCSession.PLCGlobalLabCode, 3, OleDbType.VarChar, ParameterDirection.Input);
                                qryDeleteCase.AddProcedureParameter("ecn", Convert.ToInt32(ecn), 10, OleDbType.Integer, ParameterDirection.Input);
                                qryDeleteCase.ExecuteProcedure("DELETE_ASSIGNMENT_ITEM");
                            }
                            else
                            {
                                PLCQuery qryDeleteCase = new PLCQuery();
                                List<OracleParameter> parameters = new List<OracleParameter>();
                                parameters.Add(new OracleParameter("examKey", Convert.ToInt32(PLCSession.PLCGlobalAssignmentKey)));
                                parameters.Add(new OracleParameter("caseKey", Convert.ToInt32(PLCSession.PLCGlobalCaseKey)));
                                parameters.Add(new OracleParameter("deleteReason", hdnConfirmUpdate.Value));
                                parameters.Add(new OracleParameter("userId", PLCSession.PLCGlobalAnalyst));
                                parameters.Add(new OracleParameter("program", ("iLIMS" + PLCSession.PLCBEASTiLIMSVersion).Substring(0, 8)));
                                parameters.Add(new OracleParameter("OSComputerName", PLCSession.GetOSComputerName()));
                                parameters.Add(new OracleParameter("OSUserName", PLCSession.GetOSUserName()));
                                parameters.Add(new OracleParameter("OSAddress", PLCSession.GetOSAddress()));
                                parameters.Add(new OracleParameter("BuildNumber", PLCSession.PLCBEASTiLIMSVersion));
                                parameters.Add(new OracleParameter("labCode", PLCSession.PLCGlobalLabCode));
                                parameters.Add(new OracleParameter("ecn", Convert.ToInt32(ecn)));
                                qryDeleteCase.ExecuteOracleProcedure("DELETE_ASSIGNMENT_ITEM", parameters);
                            }
                        }
                        catch (Exception e)
                        {
                            PLCSession.WriteDebug("@assignmentsTab DELETE_ASSIGNMENT_ITEM exception: " + e.Message, true);
                        }
                    }

                    private void CancelItemOpenTask(string ecn, int changeKey)
                    {
                        if (IsItemCancelTaskConfirmed)
                        {
                            PLCQuery qry = new PLCQuery(String.Format("SELECT * FROM TV_TASKLIST WHERE CASE_KEY = {0} AND EXAM_KEY = {1} AND EVIDENCE_CONTROL_NUMBER = {2} AND STATUS = 'O'",
                                PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalAssignmentKey, ecn));
                            if (qry.Open() && qry.HasData())
                                while (!qry.EOF())
                                {
                                    qry.Edit();
                                    qry.SetFieldValue("STATUS", "X");
                                    qry.Post("TV_TASKLIST", 52, 1, changeKey);
                                    qry.Next();
                                }

                            qry = new PLCQuery(String.Format("SELECT * FROM TV_TASKLIST WHERE CASE_KEY = {0} AND EXAM_KEY = {1} AND EVIDENCE_CONTROL_NUMBER = {2} AND STATUS = 'A'",
                                PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalAssignmentKey, ecn));
                            if (qry.Open() && qry.HasData())
                                while (!qry.EOF())
                                {
                                    PLCQuery qryWorkTask = new PLCQuery(string.Format("SELECT WL.WORKLIST_ID, WT.SEQUENCE, WT.TASK_ID " +
                                        "FROM TV_WORKTASK WT JOIN TV_WORKLIST WL ON WT.WORKLIST_ID = WL.WORKLIST_ID " +
                                        "WHERE WT.TASK_ID = {0} AND WL.STATUS IS NULL", qry.FieldByName("TASK_ID")));
                                    if (qryWorkTask.Open() && qryWorkTask.HasData())
                                    {
                                        string taskID = qryWorkTask.FieldByName("TASK_ID");
                                        string worklistID = qryWorkTask.FieldByName("WORKLIST_ID");
                                        int taskSequence = (string.IsNullOrEmpty(qryWorkTask.FieldByName("SEQUENCE"))) ? 1 : Convert.ToInt32(qryWorkTask.FieldByName("SEQUENCE"));

                                        // Delete WORKTASK record
                                        qryWorkTask = new PLCQuery();
                                        qryWorkTask.SQL = string.Format("DELETE FROM TV_WORKTASK WHERE WORKLIST_ID = {0} AND TASK_ID = {1}", worklistID, taskID);
                                        qryWorkTask.ExecSQL();

                                        // Update WORKTASK sequences
                                        qryWorkTask = new PLCQuery();
                                        qryWorkTask.SQL = string.Format("UPDATE TV_WORKTASK SET SEQUENCE = SEQUENCE - 1 WHERE WORKLIST_ID = {0} AND SEQUENCE > {1}", worklistID, taskSequence.ToString());
                                        qryWorkTask.ExecSQL();

                                        // Update WORKLIST TASK_TYPE
                                        List<string> tasks = new List<string>();
                                        qryWorkTask = new PLCQuery("SELECT DISTINCT TL.TASK_TYPE FROM TV_WORKTASK WT INNER JOIN TV_TASKLIST TL ON WT.TASK_ID = TL.TASK_ID WHERE WT.WORKLIST_ID = " + worklistID);
                                        qryWorkTask.Open();
                                        while (!qryWorkTask.EOF())
                                        {
                                            tasks.Add(qryWorkTask.FieldByName("TASK_TYPE"));
                                            qryWorkTask.Next();
                                        }

                                        qryWorkTask = new PLCQuery("SELECT * FROM TV_WORKLIST WHERE WORKLIST_ID = " + worklistID);
                                        qryWorkTask.Open();
                                        qryWorkTask.Edit();
                                        qryWorkTask.SetFieldValue("TASK_TYPE", string.Join(", ", tasks));
                                        qryWorkTask.Post("TV_WORKLIST", 32, 1);

                                        qry.Edit();
                                        qry.SetFieldValue("STATUS", "X");
                                        qry.Post("TV_TASKLIST", 52, 1, changeKey);
                                    }

                                    qry.Next();
                                }
                        }
                    }

                    private void ToggleResultsButton()
                    {
                        if (PLCSession.GetLabCtrl("REMEMBER_SEARCH_RESULTS") == "T")
                        {
                            if (Session["RedirectFromAssignSearch"] != null &&
                                Convert.ToBoolean(Session["RedirectFromAssignSearch"]) == true)
                            {
                                PLCButtonPanel1.SetCustomButtonVisible("Search Results...", true);
                            }
                            else
                            {
                                PLCButtonPanel1.SetCustomButtonVisible("Search Results...", false);
                            }
                        }
                        else
                            PLCButtonPanel1.SetCustomButtonVisible("Search Results...", false);
                    }

                    private void ConfigureResultsGrid()
                    {
                        gvAssignSearch.PLCSQLString = PLCSession.PLCAssignSearchSelect;
                        gvAssignSearch.PLCSQLString_AdditionalFrom = PLCSession.PLCAssignSearchFrom;
                        gvAssignSearch.PLCSQLString_AdditionalCriteria = PLCSession.PLCAssignSearchWhere;
                        ToggleResultsButton();
                    }

                    private Boolean UserTabRestricted()
                    {
                        string Title = string.Empty;
                        string Message = string.Empty;
                        if (PLCCommonClass.UserTabRestricted(MainMenuTab.AssignmentsTab, ref Title, ref Message))
                        {
                            messageBox.ShowMsg(Title, Message, 1);
                            return true;
                        }
                        //
                        return false;
                    }

                    private void LoadCaseByEK(string sEK)
                    {
                        PLCQuery qryFindCase = new PLCQuery();

                        string sqlstr = "SELECT E.CASE_KEY, C.DEPARTMENT_CASE_NUMBER, C.LAB_CASE FROM TV_LABEXAM E, TV_LABCASE C " +
                            "WHERE E.EXAM_KEY = " + sEK + " AND C.CASE_KEY = E.CASE_KEY";
                        qryFindCase.SQL = sqlstr;
                        qryFindCase.Open();

                        if (!qryFindCase.IsEmpty())
                        {
                            PLCSession.SetCaseVariables(qryFindCase.FieldByName("CASE_KEY"), qryFindCase.FieldByName("DEPARTMENT_CASE_NUMBER"), qryFindCase.FieldByName("LAB_CASE"));
                            PLCDBGlobalClass.AuditLogCaseAccess(PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalLabCase, PLCSession.PLCGlobalDepartmentCaseNumber);
                            PLCSession.PLCGlobalAssignmentKey = sEK;
                            Response.Redirect("~/TAB6Assignments.aspx");
                        }
                    }

                    private string GetPullListCrystalReportName(string panelSection)
                    {
                        //HideMessagePopUp();
                        //Get the section
                        string section = panelSection + "_ITEMLIST";

                        //Get the Report Name
                        string crystalReportName = PLCSession.FindCrystalReport(section);

                        //If No Report exists, pass "EXAM_ITEMLIST" and get the name
                        if (crystalReportName == "")
                        {
                            crystalReportName = PLCSession.FindCrystalReport("EXAM_ITEMLIST");
                        }

                        return crystalReportName;
                    }

                    private void CheckDisableEditDeleteBtns()
                    {
                        string sBatchKey = String.Empty;
                        if (PLCDBGlobal.instance.IsAssignmentInBatch(PLCSession.PLCGlobalAssignmentKey, ref sBatchKey))
                        {       
                            PLCButtonPanel1.DisableButton("DELETE");
                            if (PLCDBGlobal.instance.AssignmentSignedAlready(PLCSession.PLCGlobalAssignmentKey, "SIGNED"))
                                PLCButtonPanel1.DisableButton("EDIT");
                            else if (PLCDBPanel1.IsBrowseMode && !dvPLCLockStatus.Visible)
                                PLCButtonPanel1.EnableButton("EDIT");
                        }
                        else if (PLCSession.GetLabCtrl("DISABLE_EDIT_AFTER_SIGN") == "T" || PLCSession.CheckUserOption("NOEDITSIGN"))
                        {
                            if (PLCDBGlobal.instance.AssignmentSignedAlready(PLCSession.PLCGlobalAssignmentKey, "SIGNED"))
                                PLCButtonPanel1.DisableButton("EDIT");
                            else if (PLCDBPanel1.IsBrowseMode && !dvPLCLockStatus.Visible)
                                PLCButtonPanel1.EnableButton("EDIT");
                        }
                        else if (PLCDBPanel1.IsBrowseMode)
                        {
                            if (!dvPLCLockStatus.Visible)
                                PLCButtonPanel1.EnableButton("EDIT");

                            if (!PLCSession.CheckUserOption("DELASSGN"))
                                PLCButtonPanel1.DisableButton("DELETE");
                            else if (!dvPLCLockStatus.Visible)
                                PLCButtonPanel1.EnableButton("DELETE");
                        }
                    }

                    private void PopulateItemNameKeysList()
                    {
                        // List of related NAME_KEYs per ECN
                        string ItemNamesSql = null;
                        if (PLCSession.PLCDatabaseServer == "MSSQL")
                        {
                            if (PLCSession.GetLabCtrl("CHECK_ASSIGNMENT_NAME_ITEM") == "T" && hfItemKeys.Value == "")
                                ItemNamesSql = "SELECT DISTINCT ECN = I.EVIDENCE_CONTROL_NUMBER, NAME_KEY_LIST = STUFF((SELECT ',' + CONVERT(varchar, ITN.NAME_KEY) FROM TV_ITEMNAME ITN " +
                                    "WHERE ITN.EVIDENCE_CONTROL_NUMBER = I.EVIDENCE_CONTROL_NUMBER ORDER BY ITN.NAME_KEY FOR XML PATH('')), 1, 1, '') " +
                                    "FROM TV_LABITEM I INNER JOIN TV_ITEMNAME ITN2 ON ITN2.EVIDENCE_CONTROL_NUMBER = I.EVIDENCE_CONTROL_NUMBER WHERE I.CASE_KEY = " +
                                    PLCSession.PLCGlobalCaseKey;
                        }
                        else
                        {
                            if (PLCSession.GetLabCtrl("CHECK_ASSIGNMENT_NAME_ITEM") == "T" && hfItemKeys.Value == "")
                                ItemNamesSql = "SELECT DISTINCT I.EVIDENCE_CONTROL_NUMBER AS ECN, RTRIM(XMLAGG(XMLELEMENT(e,ITN.NAME_KEY || ',')).EXTRACT('//text()'),',') NAME_KEY_LIST " +
                                    "FROM TV_LABITEM I INNER JOIN TV_ITEMNAME ITN ON ITN.EVIDENCE_CONTROL_NUMBER = I.EVIDENCE_CONTROL_NUMBER WHERE I.CASE_KEY = " +
                                    PLCSession.PLCGlobalCaseKey + "GROUP BY I.EVIDENCE_CONTROL_NUMBER";
                        }

                        if (!string.IsNullOrEmpty(ItemNamesSql))
                        {
                            PLCQuery qryItemNamesList = new PLCQuery(ItemNamesSql);
                            if (qryItemNamesList.Open() && qryItemNamesList.HasData())
                            {
                                List<String> itemKeys = new List<String>();
                                List<String> itemValues = new List<String>();
                                while (!qryItemNamesList.EOF())
                                {
                                    itemKeys.Add(qryItemNamesList.FieldByName("ECN"));
                                    itemValues.Add(qryItemNamesList.FieldByName("NAME_KEY_LIST"));
                                    qryItemNamesList.Next();
                                }
                                hfItemKeys.Value = String.Join("|", itemKeys);
                                hfItemValues.Value = String.Join("|", itemValues);
                            }
                        }

                        // Assign css class to checkbox using client id and store the IDs with corresponding NAME_KEY
                        if (PLCSession.GetLabCtrl("CHECK_ASSIGNMENT_NAME_ITEM") == "T" && hfNameKeys.Value == "")
                        {
                            List<String> nameKeys = new List<String>();
                            List<String> nameValues = new List<String>();
                            foreach (GridViewRow row in gvNameItem.Rows)
                            {
                                nameKeys.Add(gvNameItem.DataKeys[row.RowIndex]["NAME_KEY"].ToString());
                                nameValues.Add(((CheckBox)row.FindControl("cbSelect")).ClientID);
                            }
                            hfNameKeys.Value = String.Join(",", nameKeys);
                            hfNameValues.Value = String.Join(",", nameValues);
                        }
                    }

                    private bool ItemSourceWithoutValue()
                    {
                        string section = GridView1.DataKeys[GridView1.SelectedRow.RowIndex].Values["SECTION"].ToString().Trim();

                        //if (PLCSession.GetLabCtrl("USES_CALCULATION_TYPE") == "T")
                        //{
                        string sql = @"SELECT LE.CASE_KEY, LE.EXAM_KEY, LE.SECTION, EC.FORCE_ITEM_SOURCE, ITA.ITEM_SOURCE 
            FROM TV_LABEXAM LE JOIN TV_EXAMCODE EC ON LE.SECTION = EC.EXAM_CODE JOIN
            TV_ITASSIGN ITA ON LE.EXAM_KEY = ITA.EXAM_KEY JOIN TV_LABITEM LI ON ITA.EVIDENCE_CONTROL_NUMBER = LI.EVIDENCE_CONTROL_NUMBER
            WHERE LE.EXAM_KEY = " + PLCSession.PLCGlobalAssignmentKey + " AND LE.SECTION  = '" + section + "' AND LI.LAB_ITEM_NUMBER <> '0'";
                        PLCQuery qryItemSource = new PLCQuery(PLCSession.FormatSpecialFunctions(sql));
                        qryItemSource.Open();
                        if (qryItemSource.HasData())
                        {
                            bool hasItemSource = true;

                            if (qryItemSource.FieldByName("FORCE_ITEM_SOURCE") != "T")
                                return false;

                            while (!qryItemSource.EOF())
                            {
                                if (qryItemSource.FieldByName("ITEM_SOURCE").Trim() == string.Empty)
                                {
                                    hasItemSource = false;
                                    break;
                                }

                                qryItemSource.Next();
                            }

                            if (!hasItemSource)
                                return true;
                        }
                        //}

                        return false;
                    }

                    private bool ItemSourceWithoutValue(string section)
                    {
                        bool forceItemSource = false;

                        PLCQuery qryForceItemSource = new PLCQuery("SELECT EC.EXAM_CODE, EC.FORCE_ITEM_SOURCE FROM TV_EXAMCODE EC WHERE EC.EXAM_CODE = '" +
                                     section + "'");
                        qryForceItemSource.Open();
                        if (qryForceItemSource.HasData())
                            forceItemSource = (qryForceItemSource.FieldByName("FORCE_ITEM_SOURCE").Trim() == "T");

                        if (forceItemSource)
                        {
                            string ECNs = string.Empty;

                            foreach (GridViewRow GVR in gItems.Rows)
                            {
                                if (GVR.RowType == DataControlRowType.DataRow)
                                {
                                    if (((CheckBox)GVR.FindControl("cbSELECT")).Checked)
                                    {
                                        ECNs += gItems.DataKeys[GVR.RowIndex].Values["ECN"].ToString().Trim() + ",";
                                    }
                                }
                            }
                            ECNs = ECNs.TrimEnd(',');

                            if (ECNs.Trim() == string.Empty)
                                return false; //Checking empty items done by a different function

                            string sql =
                                @"SELECT LI.EVIDENCE_CONTROL_NUMBER, ITA.ITEM_SOURCE FROM
                TV_LABITEM LI LEFT JOIN TV_ITASSIGN ITA ON LI.EVIDENCE_CONTROL_NUMBER = ITA.EVIDENCE_CONTROL_NUMBER
                AND ITA.EXAM_KEY = " + PLCSession.PLCGlobalAssignmentKey + @"
                WHERE LI.EVIDENCE_CONTROL_NUMBER IN (" + ECNs + ") AND LI.LAB_ITEM_NUMBER <> '0'";
                            PLCQuery qryItemSource = new PLCQuery(PLCSession.FormatSpecialFunctions(sql));
                            qryItemSource.Open();
                            if (qryItemSource.HasData())
                            {
                                bool hasItemSource = true;

                                while (!qryItemSource.EOF())
                                {
                                    if (qryItemSource.FieldByName("ITEM_SOURCE").Trim() == string.Empty)
                                    {
                                        hasItemSource = false;
                                        break;
                                    }

                                    qryItemSource.Next();
                                }

                                if (!hasItemSource)
                                    return true;
                            }
                        }
                        return false;
                    }

                    private void BindQMSExamDocsGrid(string section, DateTime dtAssigned)
                    {
                        string currentDate = DateTime.Now.ToShortDateString();
                        string dateAssigned = dtAssigned.ToShortDateString();

                        this.dbgQMSExamDocs.PLCGridName = "QMSEXAM";
                        /*this.dbgQMSExamDocs.PLCSQLString = String.Format(
            @"select TV_QMSDVER.VERSION_NUMBER, TV_QMSDVER.STATUS , TV_QMSDOCS.DOCUMENT_NAME, TV_QMSDVER.START_DATE, TV_QMSDVER.END_DATE, 
            TV_QMSDVER.STATUS, TV_QMSDVER.VERSION_KEY, TV_QMSDVER.DOCUMENT_KEY 
            from TV_QMSDOCSECT, TV_QMSDOCS, TV_QMSDVER 
            where  (TV_QMSDOCS.DOCUMENT_KEY = TV_QMSDOCSECT.DOCUMENT_KEY) and (TV_QMSDVER.DOCUMENT_KEY = TV_QMSDOCSECT.DOCUMENT_KEY)  and (TV_QMSDOCSECT.SECTION = '{0}') 
            and (TV_QMSDVER.START_DATE <= CONVERTTODATE('{1}')) and (TV_QMSDVER.END_DATE >= CONVERTTODATE('{1}')) ORDER BY TV_QMSDOCS.DOCUMENT_NAME",
            section, currentDate);*/

            this.dbgQMSExamDocs.PLCSQLString_AdditionalCriteria = PLCSession.FormatSpecialFunctions(String.Format(
@"(TV_QMSDOCSECT.SECTION = '{0}') and (TV_QMSDVER.START_DATE <= CONVERTTODATE('{1}')) and (TV_QMSDVER.END_DATE >= CONVERTTODATE('{2}')) and (TV_QMSDVER.STATUS = '{3}') ORDER BY TV_QMSDOCS.DOCUMENT_NAME",
section, currentDate, dateAssigned, "CV"));
            this.dbgQMSExamDocs.ClearSortExpression();
            this.dbgQMSExamDocs.ScrollPosition = 0;
            this.dbgQMSExamDocs.InitializePLCDBGrid();
            this.dbgQMSExamDocs.DataBind();

            PLCSession.WriteDebug("QMS Assignments Tab:", true);
            PLCSession.WriteDebug(this.dbgQMSExamDocs.PLCSQLString, true);
        }

        private void BindQMSRptDocsGrid(string examkey, string section)
        {
            string dateCompleted = string.Empty;
            string dateAssigned = string.Empty;

            // Get original approved report from REPTLINK
            PLCQuery qryReptLink = new PLCQuery(@"SELECT LABASSIGN.EXAM_KEY, LABASSIGN.SECTION, LABASSIGN.DATE_COMPLETED, LABASSIGN.DATE_ASSIGNED FROM TV_REPTLINK REPTLINK  
INNER JOIN TV_LABASSIGN LABASSIGN ON LABASSIGN.EXAM_KEY = REPTLINK.REPORT_KEY 
WHERE REPTLINK.EXAM_KEY = " + examkey);
            qryReptLink.Open();
            if (!qryReptLink.IsEmpty())
            {
                dateAssigned = qryReptLink.FieldByName("DATE_ASSIGNED");
                DateTime dtAssigned = dateAssigned == "" ? DateTime.Now : Convert.ToDateTime(dateAssigned);
                dateAssigned = dtAssigned.ToShortDateString();

                dateCompleted = qryReptLink.FieldByName("DATE_COMPLETED");
                DateTime dtCompleted = dateCompleted == "" ? DateTime.Now : Convert.ToDateTime(dateCompleted);
                dateCompleted = dtCompleted.ToShortDateString();

                this.dbgQMSRptDocs.PLCGridName = "QMSAPPROVAL";
                /*this.dbgQMSRptDocs.PLCSQLString = String.Format(
    @"select TV_QMSDVER.VERSION_NUMBER, TV_QMSDVER.STATUS , TV_QMSDOCS.DOCUMENT_NAME, TV_QMSDVER.START_DATE, TV_QMSDVER.END_DATE, TV_QMSDVER.STATUS, 
    TV_QMSDVER.VERSION_KEY, TV_QMSDVER.DOCUMENT_KEY, TV_QMSEXAML.EXAM_KEY
    from TV_QMSEXAML, TV_QMSDOCSECT, TV_QMSDOCS, TV_QMSDVER 
    where (TV_QMSDVER.VERSION_KEY = TV_QMSEXAML.VERSION_KEY) and (TV_QMSDOCS.DOCUMENT_KEY = TV_QMSDOCSECT.DOCUMENT_KEY) and (TV_QMSDVER.DOCUMENT_KEY = TV_QMSDOCSECT.DOCUMENT_KEY) and (TV_QMSEXAML.EXAM_KEY = {0}) and (TV_QMSDOCSECT.SECTION = '{1}') ORDER BY TV_QMSDOCS.DOCUMENT_NAME",
    examkey, section);*/

                this.dbgQMSRptDocs.PLCSQLString_AdditionalCriteria = PLCSession.FormatSpecialFunctions(String.Format(
    @"(TV_QMSDOCSECT.SECTION = '{0}') and (TV_QMSDVER.START_DATE <= CONVERTTODATE('{1}')) and (TV_QMSDVER.END_DATE >= CONVERTTODATE('{2}')) and (TV_QMSDVER.STATUS = '{3}') ORDER BY TV_QMSDOCS.DOCUMENT_NAME",
    section, dateCompleted, dateAssigned, "CV"));
                this.dbgQMSRptDocs.ClearSortExpression();
                this.dbgQMSRptDocs.ScrollPosition = 0;
                this.dbgQMSRptDocs.InitializePLCDBGrid();
                this.dbgQMSRptDocs.DataBind();
            }
        }

        private bool IsDNAStartDateRequiredButBlank(string sReptFmt, string section)
        {
            PLCQuery qryReptFmt = new PLCQuery("SELECT DNA_START_DATE_REQUIRED FROM TV_REPTFMT WHERE FORMAT = ?");

            if (string.IsNullOrWhiteSpace(sReptFmt)) // look for default
            {
                qryReptFmt.AddParameter("FORMAT", "*" + section);
            }
            else
            {
                qryReptFmt.AddParameter("FORMAT", sReptFmt);
            }
            
            qryReptFmt.Open();
            if ((!qryReptFmt.IsEmpty()) && qryReptFmt.FieldByName("DNA_START_DATE_REQUIRED").Trim() == "T")
            {
                PLCQuery qryLabExam = new PLCQuery("SELECT DNA_START_DATE FROM TV_LABEXAM WHERE EXAM_KEY = ?");
                qryLabExam.AddParameter("EXAM_KEY", PLCSession.PLCGlobalAssignmentKey);
                qryLabExam.Open();
                if (!qryLabExam.IsEmpty())
                {
                    if (qryLabExam.FieldByName("DNA_START_DATE").Trim() == "")
                        return true;
                }
            }

            return false;
        }

		private void GenerateSectionFileItem()
		{
			//check if Auto Create File Item
			PLCQuery qryLabExam = new PLCQuery("SELECT LE.SECTION, EC.DEFAULT_CASE_FILE, EC.CASE_FILE_CUSTODY, EC.CASE_FILE_LOCATION " +
				"FROM TV_LABEXAM LE " +
				"INNER JOIN TV_EXAMCODE EC ON LE.SECTION = EC.EXAM_CODE " +
				"WHERE EXAM_KEY = " + PLCSession.PLCGlobalAssignmentKey);
			if (qryLabExam.Open() && qryLabExam.HasData() && qryLabExam.FieldByName("DEFAULT_CASE_FILE") != "")
			{
                string labItemNumber = PLCDBGlobalClass.SectionCaseFileLabItemNumber(qryLabExam.FieldByName("SECTION"));
                //check if File Item for the section does not exist
				PLCQuery qryLabItem = new PLCQuery("SELECT EVIDENCE_CONTROL_NUMBER FROM TV_LABITEM " +
					" WHERE CASE_KEY = " + PLCSession.PLCGlobalCaseKey + " " +
                    " AND LAB_ITEM_NUMBER = '" + labItemNumber + "'");
				if (qryLabItem.Open() && qryLabItem.IsEmpty())
				{
                    int nLabCaseSubNum = PLCDBGlobalClass.GetLastSubmission();
					PLCQuery qryLabSub = new PLCQuery("SELECT SUBMISSION_KEY, COMMAND_VOUCHER FROM TV_LABSUB " +
						"WHERE CASE_KEY = " + PLCSession.PLCGlobalCaseKey + " " +
						"AND SUBMISSION_NUMBER = " + nLabCaseSubNum.ToString());
					if (qryLabSub.Open() && qryLabSub.HasData())
					{
                        int nECN = PLCSession.GetNextSequence("LABITEM_SEQ");
						qryLabItem = new PLCQuery("SELECT * FROM TV_LABITEM WHERE 0 = 1");
						qryLabItem.Open();
						qryLabItem.Append();
						qryLabItem.SetFieldValue("EVIDENCE_CONTROL_NUMBER", nECN);
						qryLabItem.SetFieldValue("CASE_KEY", PLCSession.PLCGlobalCaseKey);
                        qryLabItem.SetFieldValue("LAB_ITEM_NUMBER", labItemNumber);
						qryLabItem.SetFieldValue("LAB_CASE_SUBMISSION", nLabCaseSubNum);
						qryLabItem.SetFieldValue("DEPARTMENT_ITEM_NUMBER", "0");
						qryLabItem.SetFieldValue("ITEM_TYPE", qryLabExam.FieldByName("DEFAULT_CASE_FILE"));
						qryLabItem.SetFieldValue("PACKAGING_CODE", "FILE");
						qryLabItem.SetFieldValue("ITEM_SORT", PLCCommonClass.GetItemSort("0" + qryLabExam.FieldByName("SECTION")));
						qryLabItem.SetFieldValue("CUSTODY_OF", qryLabExam.FieldByName("CASE_FILE_CUSTODY"));
						qryLabItem.SetFieldValue("LOCATION", qryLabExam.FieldByName("CASE_FILE_LOCATION"));
                        qryLabItem.SetFieldValue("CUSTODY_BY", PLCSession.PLCGlobalAnalyst);
                        qryLabItem.SetFieldValue("CUSTODY_DATE", DateTime.Now);

                        if (PLCSession.GetLabCtrl("DUPE_DEPT_ITEM_NUM_ON_SAMPLE") == "T")
                            qryLabItem.SetFieldValue("DEPT_ITEM_SORT", PLCCommon.instance.GetItemSort("0"));

						if (PLCSession.GetLabCtrl("RMS_EXTERNAL_IS_VOUCHER") == "T")
							qryLabItem.SetFieldValue("RMS_EXTERNAL", qryLabSub.FieldByName("COMMAND_VOUCHER"));
						else
							qryLabItem.SetFieldValue("RMS_EXTERNAL", "");
                        qryLabItem.SetFieldValue("QUANTITY", 1);
                        qryLabItem.Post("TV_LABITEM", 7, 10);

                        // add to sublink
                        PLCQuery qrySubLink = new PLCQuery();
                        qrySubLink.SQL = "Select * from TV_SUBLINK where 1 = 0";
                        qrySubLink.Open();
                        qrySubLink.Append();
                        qrySubLink.SetFieldValue("SUBMISSION_KEY", qryLabSub.FieldByName("SUBMISSION_KEY"));
                        qrySubLink.SetFieldValue("EVIDENCE_CONTROL_NUMBER", nECN);
                        qrySubLink.SetFieldValue("RESUBMISSION", "F");
                        qrySubLink.Post("TV_SUBLINK", 7000, 17);
					}
				}	
			}
		}
      
        private void ResetControls()
        {
            PLCSession.PLCGlobalAssignmentKey = "";
            if (PLCSession.PLCGeneralDataSet != null && PLCSession.PLCGeneralDataSet.Tables.IndexOf("ASSIGNMENTITEMS") >= 0)
            {
                PLCSession.PLCGeneralDataSet.Tables["ASSIGNMENTITEMS"].Rows.Clear();
            }

            PLCDBPanel1.IsBrowseMode = false;
            PLCDBPanel1.EmptyMode();
            PLCButtonPanel1.SetEmptyMode();
            dbpDNADates.EmptyMode();
            bpDNADates.SetEmptyMode();

            EnableButtonControls(false, false);
            bnDNADates.Visible = false;
            bnAnalysts.Visible = false;

            gItems.DataBind();
            bnBigView.Enabled = false;

            gvNameItem.DataBind();

            SetAttachmentsButton("CASE");
            SetAttachmentsButtonMode(false);
        }

        private void SetAttachmentsButton(string source)
        {
            MasterPage master = (MasterPage)this.Master;
            if (source == "CASE" && PLCSession.GetLabCtrl("NOTIFY_ATTACH_FOR_TABS") == "T")
            {
                PLCQuery qryLabCase = new PLCQuery("SELECT DEPARTMENT_CODE, DEPARTMENT_CASE_NUMBER FROM TV_LABCASE WHERE CASE_KEY = " + PLCSession.PLCGlobalCaseKey);
                qryLabCase.Open();

                PLCSession.PLCGlobalAttachmentSource = source;
                PLCSession.PLCGlobalAttachmentSourceDesc = "Agency: " + qryLabCase.FieldByName("DEPARTMENT_CODE") + ", " + PLCSession.GetLabCtrl("DEPT_CASE_TEXT") + ":" + qryLabCase.FieldByName("DEPARTMENT_CASE_NUMBER");
                master.ApplyImageAttachedClip("LABCASE", PLCSession.PLCGlobalCaseKey);
            }
            else if (source == "ASSIGNMENT")
            {
                PLCSession.PLCGlobalAttachmentSource = source;
                PLCSession.PLCGlobalAttachmentSourceDesc = "Assignment " + GridView1.SelectedDataKey["SEQUENCE"].ToString() + " Section : " + PLCDBPanel1.getpanelfield("SECTION");
                master.ApplyImageAttachedClip("LABEXAM", PLCSession.PLCGlobalAssignmentKey);
            }
        }

        private bool ShowReportVerificationButton(out string verificationType)
        {
            //hide/show matrix buttons
            //if user is not the assigned analyst, has TV_ANALSECT.REVIEW_REPORTS permissions, and EXAM_CODE.VERIFICATION_QUERY returns results, show Verify button
            //else, show Report button

            bool isVerified = false;
            string section = PLCDBPanel1.getpanelfield("SECTION");
            bool isAnalystAssigned = PLCDBPanel1.getpanelfield("ANALYST_ASSIGNED").ToUpper() == PLCSession.PLCGlobalAnalyst.ToUpper();
            bool canReviewReport = string.IsNullOrWhiteSpace(section) ? false : Convert.ToString(PLCSession.GetUserAnalSect(section, "REVIEW_REPORTS")) == "T";
            verificationType = PLCDBGlobal.instance.GetAssignmentVerificationType(PLCSession.PLCGlobalAssignmentKey, out isVerified);
            bool hasVerificationResults = (!string.IsNullOrWhiteSpace(verificationType));

            bool showVerification = !isAnalystAssigned && canReviewReport && hasVerificationResults;

            return showVerification;
        }

        private void SetNotesSequence()
        {
            bool recordAllMatrixRevisions = PLCDBGlobal.instance.CheckExamCode(Convert.ToInt32(PLCSession.PLCGlobalAssignmentKey), "RECORD_ALL_MATRIX_REVISIONS");// If set to T, log add/remove panel

            if (recordAllMatrixRevisions || !PLCDBGlobal.instance.CheckExamCode(Convert.ToInt32(PLCSession.PLCGlobalAssignmentKey), "REVISION_ON_SIGN") ||
                PLCDBGlobal.instance.IsAssignmentAnalystSigned(PLCSession.PLCGlobalAssignmentKey))
            {
                PLCSession.PLCNotesSequence = Convert.ToString(PLCSession.GetNextSequence("NOTESCH_SEQ"));
                PLCSession.SetDefault("MATRIX_NOTEREV_COMMENTS", null);
            }
        }

        private void SetAttachmentsButtonMode(bool enable)
        {
            MasterPage mp = (MasterPage)this.Master;
            mp.EnablePaperClip(enable);
            if (GridView1.Rows.Count <= 0) mp.SetPaperClipImageManually(false);
        }

        private string GetItemsWithOpenTasks(GridView gv)
        {
            string itemDesc = "";
            List<string> uncheckedECNs = new List<string>();
            foreach (GridViewRow row in gv.Rows)
            {
                if (!((CheckBox)row.FindControl("cbSELECT")).Checked)
                    uncheckedECNs.Add(gv.DataKeys[row.DataItemIndex].Value.ToString());
            }

            if (uncheckedECNs.Count > 0)
            {
                PLCQuery qry = new PLCQuery(String.Format("SELECT DISTINCT LI.LAB_ITEM_NUMBER, LI.ITEM_DESCRIPTION, IT.DESCRIPTION FROM TV_TASKLIST TL " +
                    "INNER JOIN TV_LABITEM LI ON TL.EVIDENCE_CONTROL_NUMBER = LI.EVIDENCE_CONTROL_NUMBER " +
                    "INNER JOIN TV_ITEMTYPE IT ON LI.ITEM_TYPE = IT.ITEM_TYPE " +
                    "WHERE TL.CASE_KEY = {0} AND TL.EXAM_KEY = {1} AND TL.EVIDENCE_CONTROL_NUMBER IN ({2}) AND TL.STATUS = 'O'",
                    PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalAssignmentKey, string.Join(",", uncheckedECNs)));
                if (qry.Open() && qry.HasData())
                    while (!qry.EOF())
                    {
                        itemDesc += "<li class='blt'>" + qry.FieldByName("LAB_ITEM_NUMBER") + " - " + qry.FieldByName("DESCRIPTION") + " - " + qry.FieldByName("ITEM_DESCRIPTION") + "</li>";
                        qry.Next();
                    }
            }

            return itemDesc;
        }

        private string GetItemWithWorklist(GridView gv)
        {
            string itemDesc = "";
            List<string> uncheckedECNs = new List<string>();
            foreach (GridViewRow row in gv.Rows)
            {
                if (!((CheckBox)row.FindControl("cbSELECT")).Checked)
                    uncheckedECNs.Add(gv.DataKeys[row.DataItemIndex].Value.ToString());
            }

            if (uncheckedECNs.Count > 0)
            {
                PLCQuery qry = new PLCQuery(String.Format("SELECT DISTINCT LI.LAB_ITEM_NUMBER, LI.ITEM_DESCRIPTION, IT.DESCRIPTION FROM TV_TASKLIST TL " +
                    "INNER JOIN TV_LABITEM LI ON TL.EVIDENCE_CONTROL_NUMBER = LI.EVIDENCE_CONTROL_NUMBER " +
                    "INNER JOIN TV_ITEMTYPE IT ON LI.ITEM_TYPE = IT.ITEM_TYPE " +
                    "INNER JOIN TV_WORKTASK WT ON TL.TASK_ID = WT.TASK_ID " +
                    "INNER JOIN TV_WORKLIST WL ON WT.WORKLIST_ID = WL.WORKLIST_ID " +
                    "WHERE TL.CASE_KEY = {0} AND TL.EXAM_KEY = {1} AND TL.EVIDENCE_CONTROL_NUMBER IN ({2}) AND WL.STATUS IS NULL",
                    PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalAssignmentKey, string.Join(",", uncheckedECNs)));
                if (qry.Open() && qry.HasData())
                    while (!qry.EOF())
                    {
                        itemDesc += "<li class='blt'>" + qry.FieldByName("LAB_ITEM_NUMBER") + " - " + qry.FieldByName("DESCRIPTION") + " - " + qry.FieldByName("ITEM_DESCRIPTION") + "</li>";
                        qry.Next();
                    }
            }

            return itemDesc;
        }

        private bool AssignSequenceExists(string sequence, bool rowAdded)
        {
            PLCQuery qryAssign = new PLCQuery();
            if (rowAdded)
                qryAssign.SQL = String.Format("SELECT * FROM TV_LABEXAM WHERE CASE_KEY = {0} AND SEQUENCE = '{1}'", PLCSession.PLCGlobalCaseKey, sequence);
            else
                qryAssign.SQL = String.Format("SELECT * FROM TV_LABEXAM WHERE CASE_KEY = {0} AND SEQUENCE = '{1}' AND EXAM_KEY <> {2}", PLCSession.PLCGlobalCaseKey, sequence, PLCSession.PLCGlobalAssignmentKey);

            return qryAssign.Open() && qryAssign.HasData();
        }

        private void GetAssignAttachEntryIDs()
        {
            string dataType = string.Empty;
            PLCQuery qryImages = new PLCQuery();
            qryImages.SQL = @"SELECT I.IMAGE_ID, I.ENTRY_ID, P.DATA_TYPE, I.FORMAT, I.IMAGE_ID 
FROM TV_IMAGES I 
LEFT OUTER JOIN TV_PRINTLOG P ON P.IMAGE_ID = I.IMAGE_ID 
WHERE P.FILE_SOURCE = 'LABEXAM' AND P.FILE_SOURCE_KEY1 = " + PLCSession.PLCGlobalAssignmentKey +
@" UNION 
SELECT I.IMAGE_ID, I.ENTRY_ID, E.DATA_TYPE, I.FORMAT, I.IMAGE_ID
FROM TV_IMAGES I 
LEFT OUTER JOIN TV_EXAMIMAG E ON E.IMAGES_TABLE_ID = I.IMAGE_ID 
WHERE E.EXAM_KEY = " + PLCSession.PLCGlobalAssignmentKey +
@" UNION 
SELECT I.IMAGE_ID, I.ENTRY_ID, E.DATA_TYPE, I.FORMAT, I.IMAGE_ID
FROM TV_IMAGES I 
INNER JOIN TV_EXAMIMAG E ON E.ANNOTATED_IMAGES_TABLE_ID = I.IMAGE_ID 
WHERE E.EXAM_KEY = " + PLCSession.PLCGlobalAssignmentKey;
            qryImages.Open();

            if (qryImages.IsEmpty())
                return;
            PLCQuery qryPrintlog = new PLCQuery();
            while (!qryImages.EOF())
            {
                qryPrintlog = new PLCQuery();
                qryPrintlog.ClearParameters();
                qryPrintlog.SQL = "SELECT IMAGE_ID FROM TV_PRINTLOG WHERE IMAGE_ID = ? AND FILE_SOURCE_KEY1 <> ?";
                qryPrintlog.AddSQLParameter("IMAGE_ID", qryImages.FieldByName("IMAGE_ID"));
                qryPrintlog.AddSQLParameter("FILE_SOURCE_KEY1", PLCSession.PLCGlobalAssignmentKey);
                qryPrintlog.Open();

                if (qryPrintlog.IsEmpty())
                {
                    dataType = !string.IsNullOrEmpty(qryImages.FieldByName("DATA_TYPE")) ? qryImages.FieldByName("DATA_TYPE") : qryImages.FieldByName("FORMAT");

                    if(!IsImageReferencedByOthers(qryImages.FieldByName("IMAGE_ID")))
                        AttachmentEntryIDs.Add(qryImages.FieldByName("ENTRY_ID") + "." + dataType);
                }
                qryImages.Next();
            }
        }

        private void GetItemAttachEntryIDs(List<string> ecns)
        {
            if (!PLCSession.GetLabCtrl("USES_IMAGE_VAULT_SERVICE").Equals("T"))
                return;

            AttachmentEntryIDs.Clear();

            PLCQuery qryImages = new PLCQuery();
            string dataType = string.Empty;
            string fileName = string.Empty;

            foreach (string ecn in ecns)
            {
                qryImages.SQL = @"SELECT I.ENTRY_ID, E.DATA_TYPE, I.FORMAT, I.IMAGE_ID
FROM TV_IMAGES I 
LEFT OUTER JOIN TV_EXAMIMAG E ON E.IMAGES_TABLE_ID = I.IMAGE_ID
WHERE E.EVIDENCE_CONTROL_NUMBER = " + ecn + " AND E.EXAM_KEY = " + PLCSession.PLCGlobalAssignmentKey +
@" UNION 
SELECT I.ENTRY_ID, E.DATA_TYPE, I.FORMAT, I.IMAGE_ID
FROM TV_IMAGES I 
INNER JOIN TV_EXAMIMAG E ON E.ANNOTATED_IMAGES_TABLE_ID = I.IMAGE_ID
WHERE E.EVIDENCE_CONTROL_NUMBER = " + ecn + " AND E.EXAM_KEY = " + PLCSession.PLCGlobalAssignmentKey;
                qryImages.Open();

                while (!qryImages.EOF())
                {
                    dataType = !string.IsNullOrEmpty(qryImages.FieldByName("DATA_TYPE")) ? qryImages.FieldByName("DATA_TYPE") : qryImages.FieldByName("FORMAT");
                    fileName = qryImages.FieldByName("ENTRY_ID") + "." + dataType;

                    if (!AttachmentEntryIDs.Contains(fileName) && !IsImageReferencedByOthers(qryImages.FieldByName("IMAGE_ID")))
                        AttachmentEntryIDs.Add(fileName);

                    qryImages.Next();
                }
            }
        }

        private void DeleteAttachmentsOnServer()
        {
            if (AttachmentEntryIDs.Count <= 0)
                return;

            PLCHelperFunctions PLCHelper = new PLCHelperFunctions();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "_deleteAssignAttachments" + DateTime.Now.ToString(), "DeleteAttachmentsOnServer('" + PLCHelper.GetDeleteIVFromServerURL("") + "', ['" + string.Join("','", AttachmentEntryIDs) + "']);", true);

            AttachmentEntryIDs.Clear();
        }

        /// <summary>
        /// Checks if the image is referenced by others. 
        /// </summary>
        private bool IsImageReferencedByOthers(string imageId)
        {
            var query = new PLCQuery();
            query.SQL = "SELECT " +
                            "COUNT(EXAM_LOG_KEY) AS REFERENCE_COUNT " +
                        "FROM " +
                            "TV_EXAMIMAG " +
                        "WHERE " +
                            "IMAGES_TABLE_ID = ? OR ANNOTATED_IMAGES_TABLE_ID = ?";

            query.AddParameter("IMAGES_TABLE_ID", imageId);
            query.AddParameter("ANNOTATED_IMAGES_TABLE_ID", imageId);
            query.Open();
            if (query.HasData())
                return query.iFieldByName("REFERENCE_COUNT") > 1;

            return false;
        }

        private void SetNotesDefaultValuesForSign(string section)
        {
            PLCDBGlobal plcDbGlobal = new PLCDBGlobal();
            List<string> lstDistinctGroups = null;
            List<string> lstIgnoreGroups = null;
            List<string> lstNoteSetup = null;

            // retrieve the list of groups to ignore
            lstIgnoreGroups = plcDbGlobal.GetIgnoreGroups();

            // retrieves the a list of distinct note groups
            lstDistinctGroups = plcDbGlobal.GetDistinctCustomGroups(lstIgnoreGroups, section);

            // retrieve the list of tabs indicated in the notes setup table
            lstNoteSetup = plcDbGlobal.GetNotesSetupRecords(section);

            foreach (string strNotesTab in lstNoteSetup)
            {
                if (lstDistinctGroups != null)
                {
                    if (lstDistinctGroups.Contains(strNotesTab, StringComparer.OrdinalIgnoreCase))
                    {
                        CreateDefaultStats(strNotesTab, section);
                        continue;
                    }
                }

                if (strNotesTab.ToLower() == "statistics")
                {
                    CreateDefaultStats(strNotesTab, section);
                }
            }
        }

        private void CheckNotesStatsRecords(string examKey, string section, out string msg)
        {
            PLCDBGlobal plcDbGlobal = new PLCDBGlobal();
            List<string> lstDistinctGroups = null;
            List<string> lstIgnoreGroups = null;
            List<string> lstNoteSetup = null;
            List<string> lstAllStatGroupRes = new List<string>();

            // retrieve the list of groups to ignore
            lstIgnoreGroups = plcDbGlobal.GetIgnoreGroups();

            // retrieves the a list of distinct note groups
            lstDistinctGroups = plcDbGlobal.GetDistinctCustomGroups(lstIgnoreGroups, section);

            // retrieve the list of tabs indicated in the notes setup table
            lstNoteSetup = plcDbGlobal.GetNotesSetupRecords(section);

            foreach (string strNotesTab in lstNoteSetup)
            {
                if (lstDistinctGroups != null)
                {
                    if (lstDistinctGroups.Contains(strNotesTab, StringComparer.OrdinalIgnoreCase))
                    {
                        lstAllStatGroupRes.Add(strNotesTab.ToUpper());
                        continue;
                    }
                }

                if (strNotesTab.ToLower() == "statistics")
                {
                    lstAllStatGroupRes.Add(strNotesTab.ToUpper());
                }
            }

            msg = string.Empty;
            bool hasRecord = false;
            bool hasRequiredBlankRecord = false;
            PLCQuery qryStats = new PLCQuery();
            qryStats.SQL = string.Format(@"SELECT A.STAT_CODE, B.EXAM_KEY, B.TEXT, A.GROUP_RES, A.NOT_REQUIRED 
FROM TV_STATLIST A
LEFT OUTER JOIN TV_STATS B ON B.STAT_CODE = A.STAT_CODE AND UPPER(A.GROUP_RES) = UPPER(B.GROUP_RES) AND B.EXAM_KEY = {0}
WHERE UPPER(A.GROUP_RES) IN ('{1}') AND A.SECTION = '{2}' AND (UPPER(A.ACTIVE) <> 'F' OR A.ACTIVE IS NULL)", examKey, string.Join("', '", lstAllStatGroupRes), section);
            qryStats.Open();

            if (qryStats.IsEmpty())
            {
                return;
            }

            while (!qryStats.EOF())
            {
                if (!string.IsNullOrEmpty(qryStats.FieldByName("EXAM_KEY")))
                {
                    if (!string.IsNullOrEmpty(qryStats.FieldByName("TEXT")))
                        hasRecord = true;
                    else if (!qryStats.FieldByName("NOT_REQUIRED").Equals("T"))
                        hasRequiredBlankRecord = true;
                }
                else if (!qryStats.FieldByName("NOT_REQUIRED").Equals("T"))
                {
                    hasRequiredBlankRecord = true;
                }
                qryStats.Next();
            }

            if (!hasRecord)
                msg = "Statistics must be entered before signing. Click the \"" + bnNotes.Text + "\" button to enter the statistics.";
            else if (hasRequiredBlankRecord)
                msg = "Please fill in required fields in Statistics before signing. Click the \"" + bnNotes.Text + "\" button to enter the statistics.";
        }

        private void CreateDefaultStats(string groupRes, string section)
        {
            PLCQuery qryStatList = QueryStatList(groupRes, section);
            if (qryStatList.IsEmpty())
                return;

            while (!qryStatList.EOF())
            {
                string defaultValue = qryStatList.FieldByName("DEFAULT_VALUE").Trim();
                if (!string.IsNullOrEmpty(defaultValue))
                {
                    PLCQuery qryStats = QueryStatsLine(PLCSession.PLCGlobalAssignmentKey, qryStatList.FieldByName("STAT_CODE"), qryStatList.FieldByName("GROUP_RES"));
                    qryStats.Open();

                    if (qryStats.IsEmpty())
                    {
                        qryStats.Append();

                        qryStats.SetFieldValue("EXAM_KEY", PLCSession.PLCGlobalAssignmentKey);
                        qryStats.SetFieldValue("STAT_CODE", qryStatList.FieldByName("STAT_CODE"));
                        qryStats.SetFieldValue("CASE_KEY", PLCSession.PLCGlobalCaseKey);
                        qryStats.SetFieldValue("DESCRIPTION", qryStatList.FieldByName("DESCRIPTION"));
                        qryStats.SetFieldValue("ENTRYDATE", DateTime.Now);
                        qryStats.SetFieldValue("TEXT", defaultValue);
                        qryStats.SetFieldValue("ANALYST", PLCSession.PLCGlobalAnalyst);
                        qryStats.SetFieldValue("GROUP_RES", groupRes);

                        try
                        {
                            double dblDefaultValue = Convert.ToDouble(defaultValue.Trim());
                            qryStats.SetFieldValue("COUNT_RES", dblDefaultValue);
                        }
                        catch { }

                        qryStats.Post("TV_STATS", 7000, 22);

                        // TV_AUDITLOG
                        string auditInfo = "";
                        auditInfo += "Lab Case #: " + PLCSession.PLCGlobalLabCase + "\r\n";
                        auditInfo += "Exam Key: " + PLCSession.PLCGlobalAssignmentKey + "\r\n";
                        auditInfo += "Description: Statistics Added -- ON SIGN \r\n";
                        auditInfo += "Description: " + qryStatList.FieldByName("DESCRIPTION") + "\r\n";
                        auditInfo += "Stat Code: " + qryStatList.FieldByName("STAT_CODE") + "\r\n";
                        auditInfo += "Text: " + defaultValue + "\r\n";

                        PLCSession.WriteAuditLog("7000", "22", "0", auditInfo);
                    }
                }
                qryStatList.Next();
            }
        }

        // Query for StatList checklist line item.
        private PLCQuery QueryStatList(string groupRes, string section)
        {
            PLCQuery qryStatList = new PLCQuery();
            qryStatList.SQL = String.Format("SELECT * FROM TV_STATLIST SL WHERE UPPER(SL.GROUP_RES) = UPPER('{0}') AND (SL.SECTION = '{1}') AND (UPPER(ACTIVE) <> 'F' OR ACTIVE IS NULL) ORDER BY SL.USER_RES", groupRes, section);
            qryStatList.Open();

            return qryStatList;
        }

        // Query for StatsLine answer line item.
        private PLCQuery QueryStatsLine(string examkey, string statcode, string groupRes)
        {
            PLCQuery qryStats = new PLCQuery();
            qryStats.SQL = String.Format("select * from tv_stats where EXAM_KEY = {0} and STAT_CODE = '{1}' " +
                                         " and GROUP_RES = UPPER('{2}')", examkey, statcode, groupRes);

            qryStats.Open();

            return qryStats;
        }

        private void InitializeRouteMemoEvent()
        {
            tbxRouteMessage.Attributes.Add("onblur", "limitTextBoxLengthOnBlur(event, this, 4000);");
            tbxRouteMessage.Attributes.Add("onkeydown", "return limitTextBoxLength(event, this, 4000);");
            tbxRouteMessage.Attributes.Add("ondblclick", "return expandTextBox(event, this, 'Route Message', 11000);");
        }

        /// <summary>
        /// Unlocks the assignment record. Remove assignment (EXAM_KEY) in TV_PLCLOCK
        /// </summary>
        private void UnlockRecord()
        {
            PLCDBGlobal.instance.LockUnlockRecord("TV_LABEXAM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalAssignmentKey, "-1", false);
            ((MasterPage)this.Master).SetCanRecordLock(string.Empty);
            if (Page.IsPostBack) 
                UpdateRecLockLblBtnState();
        }

        /// <summary>
        /// Updates the record lock button and label state. Clears the label, and reset button panel state with respect to the user options
        /// </summary>
        /// <param name="skipLabels">True if should skip label and update buttons only</param>
        private void UpdateRecLockLblBtnState(bool skipLabels = false)
        {
            if (!skipLabels)
            {
                lPLCLockStatus.Text = string.Empty;
                dvPLCLockStatus.Visible = false;
            }

            PLCButtonPanel1.SetButtonsForLock(skipLabels);
            CheckDisableEditDeleteBtns();
            SetReadOnlyAccess();
        }

        /// <summary>
        /// Checks the items in the assignment if in custody
        /// </summary>
        /// <returns>False if some items are not in custody, and display warning popup</returns>
        private bool ValidateItemInAnalystCustody()
        {
            bool checkCustodyForReports = false;
            bool isAssignedAnalyst = false;
            string message = string.Empty;

            checkCustodyForReports = PLCSession.GetLabCtrlFlag("CHECK_CUSTODY_FOR_REPORTS") == "T";
            isAssignedAnalyst = PLCDBPanel1.getpanelfield("ANALYST_ASSIGNED") == PLCSession.PLCGlobalAnalyst;

            if (checkCustodyForReports
                && isAssignedAnalyst)
            {
                if (!HasCustodyForAllLinkedItems(out message))
                {
                    dlgMessage.Width = "450px";
                    dlgMessage.DialogKey = "ITEMS_NOT_IN_CUSTODY";
                    dlgMessage.ShowConfirm("Warning", message);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks and lists the items not in custody
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool HasCustodyForAllLinkedItems(out string message)
        {
            bool hasCustodyForAll = true;
            var items = new Dictionary<int, string>();

            items = GetItemsNotInCustody();
            if (items.Count > 0)
            {
                hasCustodyForAll = false;
                message = GetItemsNotInCustodyMessage(items);
            }
            else
                message = string.Empty;

            return hasCustodyForAll;
        }

        /// <summary>
        /// Gets the items not in custody
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, string> GetItemsNotInCustody()
        {
            var qryItaassign = new PLCQuery();
            var items = new Dictionary<int, string>();
            var description = string.Empty;

            qryItaassign.SQL = string.Format(@"
SELECT
    IA.EXAM_KEY, IA.EVIDENCE_CONTROL_NUMBER,
    LI.LAB_ITEM_NUMBER,
    CL.DESCRIPTION AS LOCATION,
    CC.DESCRIPTION AS CUSTODY_OF
FROM TV_ITASSIGN IA
    INNER JOIN TV_LABITEM LI ON IA.EVIDENCE_CONTROL_NUMBER = LI.EVIDENCE_CONTROL_NUMBER
    LEFT JOIN {0} CL ON LI.CUSTODY_OF = CL.CUSTODY_CODE AND LI.LOCATION = CL.LOCATION
    LEFT JOIN TV_CUSTCODE CC ON LI.CUSTODY_OF = CC.CUSTODY_TYPE
WHERE
    IA.EXAM_KEY = ? 
    AND LI.LOCATION <> ?
ORDER BY
    LI.ITEM_SORT", PLCDBGlobal.instance.GetTableNameView("TV_CUSTLOC"));
            qryItaassign.AddSQLParameter("EXAM_KEY", PLCSession.PLCGlobalAssignmentKey);
            qryItaassign.AddSQLParameter("LOCATION", PLCSession.PLCGlobalAnalyst);
            qryItaassign.Open();
            if (qryItaassign.HasData())
            {
                while (!qryItaassign.EOF())
                {
                    description = string.Format("{0} {1}-{2}",
                        qryItaassign.FieldByName("LAB_ITEM_NUMBER"),
                        qryItaassign.FieldByName("CUSTODY_OF"),
                        qryItaassign.FieldByName("LOCATION"));

                    items.Add(qryItaassign.iFieldByName("EVIDENCE_CONTROL_NUMBER"), description);

                    qryItaassign.Next();
                }
            }

            return items;
        }

        /// <summary>
        /// Gets the list of items not in custody message
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        private string GetItemsNotInCustodyMessage(Dictionary<int, string> items)
        {
            var sbMessage = new StringBuilder();

            sbMessage.Append("The following Items are not in your custody. Would you like to continue?");
            sbMessage.Append("<div class=\"item-list\">");
            sbMessage.Append("<ul>");

            foreach (var item in items)
            {
                sbMessage.Append("<li>");
                sbMessage.Append(item.Value);
                sbMessage.Append("</li>");
            }

            sbMessage.Append("</ul>");
            sbMessage.Append("</div>");

            return sbMessage.ToString();
        }

        #region Interface ITab6Assignment

        public void LoadAssignTasksGrid()
        {
            if ((GridView1.Rows.Count > 0) && (GridView1.SelectedIndex == -1))
            {
                messageBox.ShowMsg("Task List", "Please select an assignment.", 1);
                return;
            }

            if (!DoesAssignmentHaveItemsChecked())
            {
                messageBox.ShowMsg("Assign Items", "Please select items for this Assignment", 0);
                return;
            }

            DataTable dt = PLCSession.PLCGeneralDataSet.Tables["ASSIGNMENTITEMS"];
            Dictionary<string, string> selectedECNs = new Dictionary<string, string>();

            foreach (GridViewRow gr in gItems.Rows)
            {
                CheckBox cbSELECT = (CheckBox)gr.FindControl("cbSELECT");

                if (cbSELECT.Checked)
                    selectedECNs.Add(((HiddenField)gr.FindControl("hdnItemNameKey")).Value, "");
            }

            PLCQuery qryLabCase = new PLCQuery(string.Format("SELECT C.CASE_KEY, C.LAB_CASE, LI.EVIDENCE_CONTROL_NUMBER FROM TV_LABCASE C INNER JOIN " +
                "TV_LABITEM LI ON C.CASE_KEY = LI.CASE_KEY WHERE LI.EVIDENCE_CONTROL_NUMBER IN ({0})", string.Join(",", selectedECNs.Keys.ToArray())));

            if (qryLabCase.Open() && qryLabCase.HasData())
            {
                while (!qryLabCase.EOF())
                {
                    if (selectedECNs.ContainsKey(qryLabCase.FieldByName("EVIDENCE_CONTROL_NUMBER")))
                        selectedECNs[qryLabCase.FieldByName("EVIDENCE_CONTROL_NUMBER")] = qryLabCase.FieldByName("CASE_KEY") + "|" + qryLabCase.FieldByName("LAB_CASE");
                    qryLabCase.Next();
                }

                var selectedItems = from row in dt.AsEnumerable()
                                    where selectedECNs.ContainsKey(row.Field<string>("ECN"))
                                    select new
                                    {
                                        ECN = row.Field<string>("ECN"),
                                        CASE_KEY = selectedECNs[row.Field<string>("ECN")].Split(new char[] { '|' })[0],
                                        LAB_CASE = selectedECNs[row.Field<string>("ECN")].Split(new char[] { '|' })[1],
                                        ITEM_NUMBER = row.Field<string>("ITEMNUM"),
                                        ITEM_TYPE = row.Field<string>("ITEM_TYPE"),
                                        ITEM_TYPE_DESC = row.Field<string>("ITEMTYPE")
                                    };

                gvAssignTask.DataSource = selectedItems;
                gvAssignTask.DataBind();
                //gvAssignTask.Controls[0].Focus();
            }

            TextBox txtBarCode = (TextBox)((UserControl)this.Page.Master.FindControl("UC_CustomerTitle_Master2")).FindControl("txtBarCode");
            if (txtBarCode != null)
                txtBarCode.ReadOnly = false;

            DisplayAssignTaskPopup();
        }

        public void SaveAssignTasksGrid()
        {
            btnSaveAssignTask_Click(null, null);
        }

        public void ClearAssignTasksGrid()
        {
            CodeMultiPick cmpAssignTaskType;

            foreach (GridViewRow row in gvAssignTask.Rows)
            {
                cmpAssignTaskType = (CodeMultiPick)FindControlWithin(row, "cmpAssignTaskType_" + row.RowIndex.ToString());
                if (cmpAssignTaskType != null)
                    cmpAssignTaskType.Clear();

                if (row.FindControl("fbAssignTaskPriority") != null)
                    ((FlexBox)row.FindControl("fbAssignTaskPriority")).SetValue("2");
            }
            DisplayAssignTaskPopup();
        }

        public void InputAssignTaskType(string code)
        {
            if (string.IsNullOrEmpty(hdnTaskListFocused.Value.Trim()))
                return;
            string cmpname = hdnTaskListFocused.Value.Trim();
            string selected = string.Empty;
            int rowIndex = Convert.ToInt32(cmpname.Substring(18, cmpname.Length - 18));

            CodeMultiPick cmp = (CodeMultiPick)gvAssignTask.Rows[rowIndex].FindControl(cmpname);
            if (cmp != null)
            {
                selected = cmp.GetText().Replace(" ", string.Empty);
                if (!("," + selected + ",").Contains("," + code + ","))
                {
                    cmp.SetText(selected + "," + code);
                }
            }

            mpeAssignTask.Show();
        }

        public void SignAssignment()
        {
            if (bnSign.Visible && bnSign.Enabled)
            {
                bnSign_Click(bnSign, null);
            }
            else
                messageBox.ShowMsg("Sign", "Signing this assignment is not allowed.", 1);
        }

        #endregion

        #region PLCQuery

        private PLCQuery QueryAnalyst(string analystKey)
        {
            PLCQuery qryAnalyst = new PLCQuery();
            qryAnalyst.SQL = String.Format("select NAME from tv_analyst where ANALYST = '{0}'", analystKey);
            qryAnalyst.Open();

            return qryAnalyst;
        }

        private PLCQuery QuerySelectedLabExam()
        {
            PLCQuery qyLabExam = new PLCQuery();
            qyLabExam.SQL = "Select * FROM TV_LABEXAM where EXAM_KEY = " + PLCSession.PLCGlobalAssignmentKey;
            qyLabExam.Open();

            return qyLabExam;
        }

        private PLCQuery QuerySelectedLabAssign()
        {
            PLCQuery qyLabExam = new PLCQuery();
            qyLabExam.SQL = "SELECT * FROM TV_LABASSIGN WHERE EXAM_KEY = " + PLCSession.PLCGlobalAssignmentKey;
            qyLabExam.Open();

            return qyLabExam;
        }

        private PLCQuery QueryExamRev(int examkey, string reviewtype, string reviewcode, int order)
        {
            PLCQuery qryExamRev = new PLCQuery();
            qryExamRev.SQL = String.Format("select * from TV_EXAMREV " +
                "where EXAM_KEY = {0} and REVIEW_CODE = '{1}' and GROUP_RES = '{2}' and ORDER_RES = {3}",
                examkey, reviewcode, reviewtype, order);
            qryExamRev.Open();

            return qryExamRev;
        }

        #endregion

        #region Review

        private bool ValidateCanSignAssignment(object sender)
        {
            bool isNoteStatsValidated = false;
            if (string.IsNullOrEmpty(PLCSession.PLCGlobalAssignmentKey))
            {
                messageBox.ShowMsg("Sign", "Please select an assignment.", 1);
                return false;
            }

            // Make sure the labexam exists and is not completed before taking action on it.
            PLCQuery qyLabExam = QuerySelectedLabExam();
            if (qyLabExam.HasData())
            {
                ((MasterPage)this.Master).SetCanRecordLock(string.Empty);

                var analystReportGenerated = PLCDBGlobal.instance.AnalystReportGenerated(PLCSession.PLCGlobalAssignmentKey);

                //this means the event was called by a confirmation box
                if (sender.GetType() != typeof(MessageBox) && analystReportGenerated &&
                    PLCDBGlobalClass.IsAssignmentRegenerationRequired(PLCSession.PLCGlobalAssignmentKey))
                {
                    mbGenericConfirm.Caption = bnSign.Text;
                    mbGenericConfirm.Message = "The following information is changed:<br/><br/>" + PLCDBGlobalClass.GetAssignmentRegenerationReason(PLCSession.PLCGlobalAssignmentKey).Replace("\r\n", "<br/>") +
                        "<br/>The report may require regeneration. Continue without regeneration?";
                    mbGenericConfirm.Show();
                    return false;
                }

                //check if report has been written
                var printCrystalReportOnly = PLCSession.CheckSectionFlag(qyLabExam.FieldByName("SECTION"), "PRINT_CRYSTAL_REPORT_ONLY");
                if ((!printCrystalReportOnly && !analystReportGenerated) ||
                    (printCrystalReportOnly && string.IsNullOrEmpty(qyLabExam.FieldByName("DRAFT_DATE"))))
                {
                    messageBox.ShowMsg("Sign", "This report has not been written.", 0);
                    return false;
                }

                bool isVerified = false;
                string verificationType = PLCDBGlobal.instance.GetAssignmentVerificationType(PLCSession.PLCGlobalAssignmentKey, out isVerified);
                bool needsVerification = (!string.IsNullOrWhiteSpace(verificationType));
                if (needsVerification)
                {
                    if (!isVerified)
                    {
                        messageBox.ShowMsg("Sign", "Verification is required.", 0); 
                        return false;
                    }
                }

                //check if assignment is assigned to current user
                if (!PLCDBGlobal.instance.AssignmentSignedAlready(PLCSession.PLCGlobalAssignmentKey, "SIGNED"))
                {
                    if (qyLabExam.FieldByName("ANALYST_ASSIGNED") != PLCSession.PLCGlobalAnalyst)
                    {
                        messageBox.ShowMsg("Sign", "This report is not assigned to you.", 0);
                        return false;
                    }

                    string assignVerifyMessage = string.Empty;
                    string panelVerifyMessage = string.Empty;
                    string testTypeVerifyMessage = string.Empty;
                    bool hasUnverifiedPanelTestType = false;

                    if (!PLCDBGlobal.instance.IsAssignmentPanelVerificationComplete(PLCSession.PLCGlobalAssignmentKey, out panelVerifyMessage))
                    {
                        if (!string.IsNullOrEmpty(panelVerifyMessage))
                            assignVerifyMessage = "Please verify the following panels: <ul>" + panelVerifyMessage + "</ul><br /><br />";
                        hasUnverifiedPanelTestType = true;
                    }

                    if (!PLCDBGlobal.instance.IsAssignmentTesTypeVerificationComplete(PLCSession.PLCGlobalAssignmentKey, out testTypeVerifyMessage))
                    {
                        if (!string.IsNullOrEmpty(testTypeVerifyMessage))
                            assignVerifyMessage = assignVerifyMessage + "Please verify the following test types: <ul>" + testTypeVerifyMessage + "</ul><br /><br />";
                        hasUnverifiedPanelTestType = true;
                    }

                    if (hasUnverifiedPanelTestType)
                    {   
                        string verifyMsg = "Verification needs to be completed before signing this report.<br /><br />";
                        assignVerifyMessage = assignVerifyMessage.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("'", "\\'").Replace("\r", " ").Replace("\n", " ").Replace("\t", " ");
                        dlgAlertMessage.ShowAlert("Sign", verifyMsg + assignVerifyMessage);
                        return false;
                    }

                    //check if there are open tasks
                    if (!PLCDBGlobal.instance.CheckForOpenTasks(PLCSession.PLCGlobalAssignmentKey, qyLabExam.FieldByName("SECTION")))
                    {
                        messageBox.ShowMsg("Sign", "There are still open tasks for this assignment.", 0);
                        return false;
                    }

                    // check TV_REPTFMT.DNA_START_DATE_REQUIRED
                    if (IsDNAStartDateRequiredButBlank(PLCDBPanel1.getpanelfield("REPORT_FORMAT"), PLCDBPanel1.getpanelfield("SECTION")))
                    {
                        messageBox.ShowMsg("Sign", "The DNA Start Date is required.", 0);
                        return false;
                    }

                    isNoteStatsValidated = true;
                    if (!ValidateNoteStatsExistence(PLCDBPanel1.getpanelfield("SECTION")))
                    {
                        return false;
                    }

                    PLCQuery qryExamCode = new PLCQuery("SELECT * FROM TV_EXAMCODE WHERE EXAM_CODE = ?");
                    qryExamCode.AddSQLParameter("EXAM_CODE", PLCDBPanel1.getpanelfield("SECTION"));
                    qryExamCode.Open();
                    if (qryExamCode.HasData() && !string.IsNullOrEmpty(qryExamCode.FieldByName("SP_ON_SIGN")))
                    {
                        string storedProcedure = qryExamCode.FieldByName("SP_ON_SIGN");

                        PLCQuery qryProc = new PLCQuery();
                        qryProc.AddProcedureParameter("P_EXAMKEY", PLCSession.PLCGlobalAssignmentKey, 10, OleDbType.Numeric, ParameterDirection.Input);
                        qryProc.AddProcedureParameter("P_MESSAGE", 0, 4000, OleDbType.VarChar, ParameterDirection.Output);
                        qryProc.AddProcedureParameter("P_RESULT", 0, 1, OleDbType.VarChar, ParameterDirection.Output);
                        Dictionary<string, object> spOutput = qryProc.ExecuteProcedure(storedProcedure);

                        string spMessage = Convert.ToString(spOutput["P_MESSAGE"]);
                        string spResult = Convert.ToString(spOutput["P_RESULT"]);

                        if (spResult != "T")
                        {
                            if (!string.IsNullOrEmpty(spMessage))
                                messageBox.ShowMsg("Sign", spMessage, 0);
                            return false;
                        }
                    }

                    string usesPrintManager = PLCDBGlobalClass.GetExamCodeValue(Int32.Parse(PLCSession.PLCGlobalAssignmentKey), "USES_PRINT_MANAGER");
                    if ((usesPrintManager == "A") && (qyLabExam.FieldExist("PRODUCE_NOTES_PACKET")))
                    {
                        qyLabExam.Edit();
                        qyLabExam.SetFieldValue("PRODUCE_NOTES_PACKET", "I");
                        qyLabExam.Post("TV_LABEXAM");
                    }
                    else if (usesPrintManager == "S" && (qyLabExam.FieldExist("PRODUCE_NOTES_PACKET")))
                    {
                        string produceNP = qyLabExam.FieldByName("PRODUCE_NOTES_PACKET");
                        if (produceNP == "C")
                        {
                            bnSign.Text = "NP Ready";

                            anpAssignment.Key = PLCSession.SafeInt(PLCSession.PLCGlobalAssignmentKey);
                            anpAssignment.Status = AutoNotesPacketStatus.Signing;
                            anpAssignment.Process = ReviewProcess.Signed;
                            anpAssignment.OverrideRequeue = true;
                            anpAssignment.ResetButtonsState();

                            anpAssignment.LoadGrid();
                            anpAssignment.Show();
                        }
                        else if (produceNP == "E")
                        {
                            bnSign.Text = "NP ERROR";

                            anpAssignment.Key = PLCSession.SafeInt(PLCSession.PLCGlobalAssignmentKey);
                            anpAssignment.Status = AutoNotesPacketStatus.Error;
                            anpAssignment.Process = ReviewProcess.Signed;
                            anpAssignment.OverrideRequeue = true;
                            anpAssignment.ResetButtonsState();

                            anpAssignment.LoadGrid();
                            anpAssignment.Show();
                        }
                        else if (string.IsNullOrEmpty(produceNP) || produceNP == "D" || produceNP == "R")
                        {
                            bool canSignWithObsoleteNP = PLCSession.GetSectionFlag(qyLabExam.FieldByName("SECTION"), "CAN_SIGN_WITH_OBSOLETE_NP") == "T";

                            PLCQuery qryObsoleteNP = new PLCQuery();
                            qryObsoleteNP.SQL = "SELECT DATA_KEY FROM TV_PDFDATA WHERE OBSOLETE = 'O' AND FILE_SOURCE = 'NOTES' AND FILE_SOURCE_KEY = '" + PLCSession.PLCGlobalAssignmentKey + "'";
                            qryObsoleteNP.Open();
                            bool hasObsoleteNP = qryObsoleteNP.HasData();

                            if (produceNP == "D")
                                bnSign.Text = PLCSession.GetSysPrompt("LABCTRL.SIGN_BUTTON_TEXT", PLCSession.GetLabCtrl("SIGN_BUTTON_TEXT"));

                            if (canSignWithObsoleteNP && hasObsoleteNP && produceNP != "D")
                            {
                                anpAssignment.Key = PLCSession.SafeInt(PLCSession.PLCGlobalAssignmentKey);
                                anpAssignment.Status = AutoNotesPacketStatus.ViewObsolete;
                                anpAssignment.Process = ReviewProcess.Signed;
                                anpAssignment.OverrideRequeue = true;

                                anpAssignment.ResetButtonsState();
                                anpAssignment.ContinueEnabled = produceNP != "R";

                                anpAssignment.LoadGrid();
                                anpAssignment.Show();
                            }
                            else if (qyLabExam.FieldByName("ANALYST_SIGNED") == "T")
                            {
                                Session["SignOCXcalled"] = "standby";

                                txtRegenReason.Text = hdnRegenReason.Value = "";
                                fbRegenReason.Focus();
                                ScriptManager.RegisterStartupScript(this, this.GetType(), "_openRegenReasonPopup" + DateTime.Now.ToString(), "openRegenReasonPopup();", true);
                            }
                            else
                                GenerateNotes();
                        }
                        else
                        {
                            ShowMessage("Application is waiting for the service to produce the notes packet, if you have questions please call your LIMS administrator.");
                        }
                        return false;
                    }
                    else
                    {
                        //If Notes Packet needed                
                        NotesPacketState nps = PLCDBGlobalClass.GetNotesPacketRequired(PLCDBPanel1.getpanelfield("SECTION"));
                        if (nps == NotesPacketState.Optional)
                        {
                            mbNotesPacketRequired.Show();
                            return false;
                        }
                        else if (nps == NotesPacketState.Required)
                        {
                            GenerateNotes();
                            return false;
                        }
                    }
                }

                // check for proficiency
                if (PLCSession.GetLabCtrl("CHECK_PROFICIENCY_TASKS").Equals("T"))
                {
                    string errMsg = CheckProficiencyTasks();
                    if (!string.IsNullOrEmpty(errMsg))
                    {
                        messageBox.ShowMsg("Sign", errMsg, 0);
                        return false;
                    }
                }

                if (!isNoteStatsValidated)
                {
                    if (!ValidateNoteStatsExistence(PLCDBPanel1.getpanelfield("SECTION")))
                    {
                        return false;
                    }
                }

                bool usesReviewRotation = PLCSession.GetLabCtrlFlag("USES_REVIEW_ROTATION") == "T";
                if (usesReviewRotation && PLCDBGlobal.instance.NeedtoAssignReviewer(PLCSession.PLCGlobalAssignmentKey))
                {
                    string reviewer = PLCDBGlobal.instance.GetNextReviewerInRotation(PLCSession.PLCGlobalAssignmentKey);
                    if (string.IsNullOrEmpty(reviewer))
                    {
                        dlgAlertMessage.ShowAlert("Sign", "There was an error assigning a reviewer. Please make sure you have analysts set up to accept assignments in rotation.");
                        return false;
                    }
                }
            }
            else
            {
                ShowAssignmentChangedError();
                return false;
            }
            return true;
        }

        private bool ValidateNoteStatsExistence(string section)
        {
            if (PLCSession.GetSectionFlag(section, "REQUIRES_STATS").Equals("T") &&
                        PLCSession.GetSectionFlag(section, "USES_NOTES").Equals("T"))
            {

                if (PLCSession.GetLabCtrlFlag("STATS_DEFAULT_VALUES_ON_SIGN").Equals("T"))
                    SetNotesDefaultValuesForSign(section);

                string msg = string.Empty;
                CheckNotesStatsRecords(PLCSession.PLCGlobalAssignmentKey, section, out msg);
                if (!string.IsNullOrEmpty(msg))
                {
                    messageBox.ShowMsg("Sign", msg, 0);
                    return false;
                }
            }

            return true;
        }

        private bool ValidateCanReviewAssignment(object sender)
        {
            if (string.IsNullOrEmpty(PLCSession.PLCGlobalAssignmentKey))
            {
                messageBox.ShowMsg("Review", "Please select an assignment.", 1);
                return false;
            }

            // Make sure the labexam exists and is not completed before taking action on it.
            PLCQuery qyLabExam = QuerySelectedLabExam();
            if (qyLabExam.HasData())
            {
                var analystReportGenerated = PLCDBGlobal.instance.AnalystReportGenerated(PLCSession.PLCGlobalAssignmentKey);

                //this means the event was called by a confirmation box
                if (sender.GetType() != typeof(MessageBox) && analystReportGenerated &&
                    PLCDBGlobalClass.IsAssignmentRegenerationRequired(PLCSession.PLCGlobalAssignmentKey))
                {
                    mbGenericConfirm.Caption = bnReview.Text;
                    mbGenericConfirm.Message = "The following information is changed:<br/><br/>" + PLCDBGlobalClass.GetAssignmentRegenerationReason(PLCSession.PLCGlobalAssignmentKey).Replace("\r\n", "<br/>") +
                        "<br/>The report may require regeneration. Continue without regeneration?";
                    mbGenericConfirm.Show();
                    return false;
                }

                if (!PLCDBGlobal.instance.IsAssignmentTechReviewed(PLCSession.PLCGlobalAssignmentKey))
                {
                    if (qyLabExam.FieldByName("ANALYST_ASSIGNED") == "" && PLCSession.SectionRequiresAnalyst(qyLabExam.FieldByName("SECTION")))
                    {
                        messageBox.ShowMsg("Review", "This assignment requires an analyst", 0);
                        return false;
                    }

                    if (qyLabExam.FieldByName("ANALYST_ASSIGNED") == PLCSession.PLCGlobalAnalyst && PLCSession.GetLabCtrl("ALLOW_SELF_REVIEW") != "T")
                    {
                        messageBox.ShowMsg("Review", "Analysts cannot peer review their own reports", 0);
                        return false;
                    }

                    var printCrystalReportOnly = PLCSession.CheckSectionFlag(qyLabExam.FieldByName("SECTION"), "PRINT_CRYSTAL_REPORT_ONLY");
                    if ((!printCrystalReportOnly && !analystReportGenerated) ||
                        (printCrystalReportOnly && string.IsNullOrEmpty(qyLabExam.FieldByName("DRAFT_DATE"))))
                    {
                        messageBox.ShowMsg("Review", "This report has not been written", 0);
                        return false;
                    }

                    // check if signed
                    if (!PLCDBGlobal.instance.AssignmentSignedAlready(PLCSession.PLCGlobalAssignmentKey, "SIGNED"))
                    {
                        messageBox.ShowMsg("Review", "This Assignment has not yet been Signed.", 0);
                        return false;
                    }

                    // check for open tasks
                    if (!PLCDBGlobal.instance.CheckForOpenTasks(PLCSession.PLCGlobalAssignmentKey, qyLabExam.FieldByName("SECTION")))
                    {
                        messageBox.ShowMsg("Review", "There are still open tasks for this assignment", 0);
                        return false;
                    }

                    // check if record is locked
                    string LockedInfo = string.Empty, LockedTypeRes = string.Empty;
                    if (PLCDBGlobal.instance.IsRecordLocked("TV_LABEXAM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalAssignmentKey, "-1", out LockedInfo, out LockedTypeRes))
                    {
                        dlgMessage.ShowAlert("Record Lock", string.Format("Assignment locked by another user for {0}.<br/>{1}", ObjAssignment.GetLockedTypeResMessage(LockedTypeRes), LockedInfo));
                        lPLCLockStatus.Text = LockedInfo;
                        dvPLCLockStatus.Visible = true;
                        UpdateRecLockLblBtnState(true);
                        return false;
                    }
                    else
                    {
                        UpdateRecLockLblBtnState();
                        PLCDBGlobal.instance.LockUnlockRecord("TV_LABEXAM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalAssignmentKey, "-1", true, "REVIEW");
                        ((MasterPage)this.Master).SetCanRecordLock("T");
                    }

                    string produceNotesPacket = qyLabExam.FieldByName("PRODUCE_NOTES_PACKET");
                    bool usesPrintManager = PLCSession.GetSectionFlag(qyLabExam.FieldByName("SECTION"), "USES_PRINT_MANAGER").Trim().ToUpper() == "A";

                    if (usesPrintManager 
                        && !string.IsNullOrEmpty(produceNotesPacket)
                        && !produceNotesPacket.Equals("C"))
                    {
                        anpAssignment.Key = PLCSession.SafeInt(PLCSession.PLCGlobalAssignmentKey);
                        anpAssignment.Process = (sender.GetType() != typeof(MessageBox) && ((Button)sender).ID == "bnCodisReview")
                            ? ReviewProcess.CodisReview
                            : ReviewProcess.Review;
                        anpAssignment.ResetButtonsState();

                        if (produceNotesPacket.Equals("E"))
                        {
                            anpAssignment.Status = AutoNotesPacketStatus.Error;
                            anpAssignment.LoadGrid();
                        }
                        else if (produceNotesPacket.Equals("I"))
                        {
                            anpAssignment.Status = AutoNotesPacketStatus.Incomplete;
                        }

                        anpAssignment.Show();
                        return false;
                    }

                    // tech review sp validation
                    string section = qyLabExam.FieldByName("SECTION");
                    string storedProcedure = PLCSession.GetSectionFlag(section, "TECH_REVIEW_SP_VALIDATION");
                    if (!string.IsNullOrEmpty(storedProcedure))
                    {
                        try
                        {
                            PLCQuery qrySpValidation = new PLCQuery();
                            qrySpValidation.AddProcedureParameter("P_EXAMKEY", PLCSession.PLCGlobalAssignmentKey, 10, OleDbType.Numeric, ParameterDirection.Input);
                            qrySpValidation.AddProcedureParameter("P_ANALYST", PLCSession.PLCGlobalAnalyst, 10, OleDbType.VarChar, ParameterDirection.Input);
                            qrySpValidation.AddProcedureParameter("P_MESSAGE", 0, 4000, OleDbType.VarChar, ParameterDirection.Output);
                            Dictionary<string, object> spOutput = qrySpValidation.ExecuteProcedure(storedProcedure);
                            string spMessage = Convert.ToString(spOutput["P_MESSAGE"]);

                            if (!string.IsNullOrEmpty(spMessage))
                            {
                                dlgMessage.ShowAlert("Review", spMessage);
                                return false;
                            }
                        }
                        catch (Exception ex)
                        {
                            PLCSession.WriteDebug("TECH_REVIEW_SP_VALIDATION: Error - Unable to execute stored procedure " + storedProcedure + ": " + ex.Message, true);
                        }
                    }
                }
            }
            else
            {
                ShowAssignmentChangedError();
                return false;
            }
            return true;
        }

        private bool ValidateCanApproveAssignment(object sender)
        {

            string sBatchKey = String.Empty;

            if (string.IsNullOrEmpty(PLCSession.PLCGlobalAssignmentKey))
            {
                messageBox.ShowMsg("Approve", "Please select an Assignment.", 1);
                return false;
            }

            var analystReportGenerated = PLCDBGlobal.instance.AnalystReportGenerated(PLCSession.PLCGlobalAssignmentKey);

            //this means the event was called by a confirmation box
            if (sender.GetType() != typeof(MessageBox) && analystReportGenerated &&
                PLCDBGlobalClass.IsAssignmentRegenerationRequired(PLCSession.PLCGlobalAssignmentKey))
            {
                mbGenericConfirm.Caption = bnApprove.Text;
                mbGenericConfirm.Message = "The following information is changed:<br/><br/>" + PLCDBGlobalClass.GetAssignmentRegenerationReason(PLCSession.PLCGlobalAssignmentKey).Replace("\r\n", "<br/>") +
                    "<br/>The report may require regeneration. Continue without regeneration?";
                mbGenericConfirm.Show();
                return false;
            }

            PLCQuery qryLabExam = QuerySelectedLabExam();

            // Make sure the labexam exists and is not completed before taking action on it.
            if (!qryLabExam.IsEmpty())
            {
                if ((!PLCSession.CheckSectionFlag(qryLabExam.FieldByName("SECTION"), "PRINT_CRYSTAL_REPORT_ONLY")) && (!PLCSession.CheckUserOption("PLCWEBOCX")))
                {
                    messageBox.ShowMsg("Approve", "PLCWEB OCX authorization is required. Please contact your LIMS Administrator.", 1);
                    return false;
                }

                if (qryLabExam.FieldByName("COMPLETED") == "T")
                {
                    messageBox.ShowMsg("Approve", "This report has been completed already", 0);
                    return false;
                }

                if (qryLabExam.FieldByName("APPROVED") == "Y")
                {
                    messageBox.ShowMsg("Approve", "This report has been approved already", 0);
                    return false;
                }

                var printCrystalReportOnly = PLCSession.CheckSectionFlag(qryLabExam.FieldByName("SECTION"), "PRINT_CRYSTAL_REPORT_ONLY");
                if ((!printCrystalReportOnly && !analystReportGenerated) ||
                    (printCrystalReportOnly && string.IsNullOrEmpty(qryLabExam.FieldByName("DRAFT_DATE"))))
                {
                    messageBox.ShowMsg("Approve", "This report has not been written", 0);
                    return false;
                }

                // check if signed
                if (!PLCDBGlobal.instance.AssignmentSignedAlready(PLCSession.PLCGlobalAssignmentKey, "SIGNED"))
                {
                    messageBox.ShowMsg("Approve", "This Assignment has not yet been Signed.", 0);
                    return false;
                }

                if ((qryLabExam.FieldByName("ANALYST_ASSIGNED") == "") && (PLCSession.SectionRequiresAnalyst(qryLabExam.FieldByName("SECTION"))))
                {
                    messageBox.ShowMsg("Approve", "This Assignment requires an analyst", 0);
                    return false;
                }

                // Analysts cannot approve their own report if labctrl PREVENT_ANALYST_ADMIN_APPROVE is set.
                if ((PLCSession.GetLabCtrl("PREVENT_ANALYST_ADMIN_APPROVE") == "T") && (qryLabExam.FieldByName("ANALYST_ASSIGNED") == PLCSession.PLCGlobalAnalyst))
                {
                    messageBox.ShowMsg("Approve", "The analyst may not approve their own report", 0);
                    return false;
                }

                // Analyst cannot approve their own report if the RESTRICT_SELF_ADMIN_REVIEW for the section is set.
                bool preventOwnApprove = PLCSession.CheckSectionFlag(qryLabExam.FieldByName("SECTION"), "RESTRICT_SELF_ADMIN_REVIEW");
                if (preventOwnApprove && (qryLabExam.FieldByName("ANALYST_ASSIGNED") == PLCSession.PLCGlobalAnalyst))
                {
                    messageBox.ShowMsg("Approve", "Analyst cannot " + bnApprove.Text + " his own report", 0);
                    return false;
                }

                if (!PLCDBGlobal.instance.CheckForOpenTasks(PLCSession.PLCGlobalAssignmentKey, qryLabExam.FieldByName("SECTION")))
                {
                    messageBox.ShowMsg("Approve", "There are still open tasks for this Assignment", 0);
                    return false;
                }

                if ((PLCSession.GetLabCtrl("USES_RECEIVED_IN_SECTION") == "T") && (qryLabExam.FieldByName("RECEIVED_IN_SECTION") == ""))
                {
                    messageBox.ShowMsg("Approve", "This Assignment requires the date when it was received in this section", 0);
                    return false;
                }

                if ((qryLabExam.FieldByName("REVIEWED") != "Y") && (PLCSession.GetLabCtrl("HIDE_REVIEW_BUTTON") != "T") && (PLCSession.CheckSectionFlag(qryLabExam.FieldByName("SECTION"), "REQUIRES_PEER_REVIEW")))
                {
                    if (PLCDBGlobal.instance.IsCheckListExists(PLCSessionVars1.PLCGlobalAssignmentKey, "REVIEW", PLCDBPanel1.getpanelfield("SECTION"), AssignmentReviewType.SingleAssignment, ChecklistRequiredOption.RequiredOnly, ChecklistReviewedOption.UnreviewedOnly))
                    {
                        string sRequiredCheckLists = PLCDBGlobal.instance.GetUnreviewedRequiredChecklists(PLCSessionVars1.PLCGlobalAssignmentKey, "REVIEW", PLCDBPanel1.getpanelfield("SECTION"), AssignmentReviewType.SingleAssignment);
                        messageBox.ShowMsg("Approve", "This Assignment requires the Review of the following Checklists: " + sRequiredCheckLists, 0);
                        return false;
                    }

                    if (qryLabExam.FieldByName("ASSIGNED_REVIEWER") != "")
                    {
                        messageBox.ShowMsg("Approve", "This Assignment requires peer review. The review is currently assigned to " + PLCSession.GetCodeDesc("ANALYST", qryLabExam.FieldByName("ASSIGNED_REVIEWER")), 0);
                        return false;
                    }
                    else
                    {
                        messageBox.ShowMsg("Approve", PLCSession.GetSectionFlag(qryLabExam.FieldByName("SECTION"), "DESCRIPTION") + " Assignments must first be peer reviewed", 0);
                        return false;
                    }
                }

                if ((qryLabExam.FieldByName("REVIEWED") != "Y") && (PLCSession.GetLabCtrl("HIDE_REVIEW_BUTTON") != "T") && PLCDBGlobal.instance.NeedtoAssignReviewer(PLCSession.PLCGlobalAssignmentKey))
                {
                    dlgAlertMessage.ShowAlert("Approve", PLCSession.GetSectionFlag(qryLabExam.FieldByName("SECTION"), "DESCRIPTION") + " Assignments must first be peer reviewed.");
                    return false;
                }

                bool bIsSigValid = false;
                string sErrorMsg = string.Empty;

                PLCQuery qryExamCode = new PLCQuery();
                qryExamCode.SQL = "SELECT * FROM TV_EXAMCODE WHERE EXAM_CODE = '" + qryLabExam.FieldByName("SECTION") + "'";
                qryExamCode.Open();

                if (PLCSession.GetLabCtrl("USES_ANALYST_SIGNATURES") == "T" && qryExamCode.FieldByName("ANALYST_SIGNATURE_REQUIRED") == "T")
                {
                    if (PLCSession.GetLabCtrl("USES_MULTI_SIGN") == "T")
                    {
                        if (!PLCDBGlobal.instance.IsMultiSignatureComplete(PLCSession.PLCGlobalAssignmentKey, ref sErrorMsg))
                        {
                            messageBox.ShowMsg("Approve", "This report must be signed before it is reviewed.", 0);
                            return false;
                        }
                    }
                    else
                    {
                        int sigID = PLCDBGlobal.instance.GetAnalystSignatureID(qryLabExam.FieldByName("ANALYST_ASSIGNED"));
                        if (sigID != -1)
                        {
                            bool bSignatureExists = PLCDBGlobal.instance.IsReportSignatureExists(PLCSession.PLCGlobalAssignmentKey,
                                                                            qryLabExam.FieldByName("ANALYST_ASSIGNED"),
                                                                            "SIGNED", ref bIsSigValid, ref sErrorMsg);
                            if (!bSignatureExists)
                            {
                                messageBox.ShowMsg("Approve", "This report requires a signature.", 0);
                                return false;
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(sErrorMsg))
                                {
                                    messageBox.ShowMsg("Approve", sErrorMsg, 0);
                                    return false;
                                }
                                else if (!bIsSigValid)
                                {
                                    messageBox.ShowMsg("Approve", "Signature is not valid.", 0);
                                    return false;
                                }
                            }
                        }
                    }
                }

                if (!PLCSession.CheckSectionFlag(qryLabExam.FieldByName("SECTION"), "PRINT_CRYSTAL_REPORT_ONLY"))
                {
                    PLCQuery qyWordData = new PLCQuery();
                    qyWordData.SQL = "SELECT * FROM TV_WORDDATA WHERE EXAM_KEY=" + PLCSession.PLCGlobalAssignmentKey;
                    qyWordData.Open();

                    if (qyWordData.IsEmpty())
                    {
                        messageBox.ShowMsg("Approve", "Word report not found", 0);
                        return false;
                    }
                }

                // Check if record is locked
                string LockedInfo = string.Empty, LockedTypeRes = string.Empty;
                if (PLCDBGlobal.instance.IsRecordLocked("TV_LABEXAM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalAssignmentKey, "-1", out LockedInfo, out LockedTypeRes))
                {
                    dlgMessage.ShowAlert("Record Lock", string.Format("Assignment locked by another user for {0}.<br/>{1}", ObjAssignment.GetLockedTypeResMessage(LockedTypeRes), LockedInfo));
                    lPLCLockStatus.Text = LockedInfo;
                    dvPLCLockStatus.Visible = true;
                    UpdateRecLockLblBtnState(true);
                    return false;
                }
                else
                {
                    UpdateRecLockLblBtnState();
                    PLCDBGlobal.instance.LockUnlockRecord("TV_LABEXAM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalAssignmentKey, "-1", true, "APPROVE");
                    ((MasterPage)this.Master).SetCanRecordLock("T");
                }

                string produceNotesPacket = qryLabExam.FieldByName("PRODUCE_NOTES_PACKET");
                bool usesPrintManager = PLCSession.GetSectionFlag(qryLabExam.FieldByName("SECTION"), "USES_PRINT_MANAGER").Trim().ToUpper() == "A";

                if (usesPrintManager
                    && !string.IsNullOrEmpty(produceNotesPacket)
                    && !produceNotesPacket.Equals("C"))
                {
                    anpAssignment.Key = PLCSession.SafeInt(PLCSession.PLCGlobalAssignmentKey);
                    anpAssignment.Process = ReviewProcess.Approve;
                    anpAssignment.ResetButtonsState();

                    if (produceNotesPacket.Equals("E"))
                    {
                        anpAssignment.Status = AutoNotesPacketStatus.Error;
                        anpAssignment.LoadGrid();
                    }
                    else if (produceNotesPacket.Equals("I"))
                    {
                        anpAssignment.Status = AutoNotesPacketStatus.Incomplete;
                    }

                    anpAssignment.Show();
                    return false;
                }
            }
            else
            {
                messageBox.ShowMsg("Approve", "The assignment no longer exists. Please select another assignment.", 0);
                return false;
            }

            // All validation went through.
            return true;
        }

        private void ApproveAssignment()
        {
            if (PLCDBGlobal.instance.IsCheckListExists(PLCSessionVars1.PLCGlobalAssignmentKey, "APPROVE", PLCDBPanel1.getpanelfield("SECTION"), AssignmentReviewType.SingleAssignment, ChecklistRequiredOption.All, ChecklistReviewedOption.UnreviewedOnly))
            {
                PLCCommon.instance.SetTechReviewModeParam(TechReviewMode.Single);
                PLCCommon.instance.SetTechReviewGroupParam("approve");
                Response.Redirect("TechReview2.aspx");
            }
            else
            {
                CallUCApproveReport();
            }
        }

        private void ApproveAssignment(object sender)
        {
            if (ValidateCanApproveAssignment(sender))
            {
                if (PLCDBGlobal.instance.HasReportPreview(PLCDBPanel1.getpanelfield("SECTION")))
                {
                    PLCApproveReportPreview approvePreview = (PLCApproveReportPreview)((UserControl)this.Page.Master.FindControl("UC_CustomerTitle_Master2")).FindControl("ReportPreview");
                    approvePreview.ExamKey = PLCSession.PLCGlobalAssignmentKey;
                    approvePreview.ReviewType = "APPROVE";
                    approvePreview.Show();
                }
                else
                {
                    ProcessReview("APPROVE");
                }
            }
        }

        private void CallUCApproveReport()
        {
            Session["ApproveOCXcalled"] = "T";
            LoadApproveReport.ApproveAssignment(PLCSession.PLCGlobalAssignmentKey);
        }

        private void PrintReportOnApprove(string section)
        {
            string reportName = PLCDBGlobal.instance.GetWordReportName(PLCSession.PLCGlobalAssignmentKey);
            PLCSession.PLCCrystalReportName = reportName;
            PLCSession.PLCCrystalSelectionFormula = "{TV_LABASSIGN.EXAM_KEY} = " + PLCSession.PLCGlobalAssignmentKey;

            string pdfName = PLCDBGlobal.instance.SaveCurrentReportAsPDF(reportName + ".pdf");
            PLCDBGlobal.instance.SaveFileToPDFData(PLCSession.PLCGlobalAssignmentKey, "REPORT", pdfName);

            if (PLCSession.CheckSectionFlag(section, "VIEW_REPORT_APPROVE"))
                PLCSession.PrintCRWReport(true);
        }

        /// <summary>
        /// Call OCX CreateNotesPacket method and then reload page.
        /// </summary>
        private void GenerateNotes(bool openSignAfterOcx = true)
        {
            if (openSignAfterOcx)
                Session["SignOCXcalled"] = "T";
            bnSign.Enabled = false;

            string returnPage = "Dashboard.aspx";
            if (Request.UrlReferrer != null)
                returnPage = Request.UrlReferrer.AbsolutePath.Substring(Request.UrlReferrer.AbsolutePath.LastIndexOf("/") + 1);

            WebOCX.OnSuccessScript = "CheckBrowserWindowID(setTimeout(function () { ShowLoading(); " +
                    "if (result.toLowerCase() != 'false') " + 
                        "window.location = '" + returnPage + "'; " + 
                    "else " +
                        "document.getElementById('" + btnCancelSignOCX.ClientID + "').click(); " + 
                        " }));";
            WebOCX.CreateNotesPacket(PLCSession.PLCGlobalAssignmentKey);
        }

        private void ProcessReview(string groupRes)
        {
            string section = PLCDBPanel1.getpanelfield("SECTION");
            if (section.Trim() == "")
            {
                PLCQuery qryLabExam = new PLCQuery("SELECT SECTION FROM TV_LABASSIGN WHERE EXAM_KEY = " + PLCSession.PLCGlobalAssignmentKey);
                if (qryLabExam.Open() && qryLabExam.HasData())
                    section = qryLabExam.FieldByName("SECTION");
            }
            if (PLCDBGlobal.instance.IsAssignmentInBatch(PLCSession.PLCGlobalAssignmentKey))
            {
                // if Batch Assignment and Notes Packet button clicked, redirect to ChecklistCaseNotes
                if (groupRes == "SIGNED")
                {
                    PLCCommon.instance.SetTechReviewModeParam(TechReviewMode.Batch);
                    PLCCommon.instance.SetTechReviewGroupParam("signed");
                    Response.Redirect("CheckListCaseNotes.aspx");
                }
            }
            else if (PLCDBGlobal.instance.AssignmentSignedAlready(PLCSessionVars1.PLCGlobalAssignmentKey, groupRes))
            {
                // if assignment has been signed, and if checklist exists, redirect to Tech Review Page, else show prompt
                if (PLCDBGlobal.instance.IsCheckListExists(PLCSessionVars1.PLCGlobalAssignmentKey, groupRes, PLCDBPanel1.getpanelfield("SECTION"), AssignmentReviewType.SingleAssignment))
                {
                    PLCCommon.instance.SetTechReviewModeParam(TechReviewMode.Single);
                    PLCCommon.instance.SetTechReviewGroupParam(groupRes.ToLower());
                    Response.Redirect("TechReview2.aspx");
                }
                else
                    messageBox.ShowMsg(groupRes, GetSignedByText(groupRes), 0);
            }
            else
            {
                // if assignment is not yet signed, and if unreviewed checklist exists, redirect to Tech Review Page, else review assignment
                if (PLCDBGlobal.instance.IsCheckListExists(PLCSessionVars1.PLCGlobalAssignmentKey, groupRes, PLCDBPanel1.getpanelfield("SECTION"), AssignmentReviewType.SingleAssignment, ChecklistRequiredOption.All, ChecklistReviewedOption.UnreviewedOnly))
                {
                    PLCCommon.instance.SetTechReviewModeParam(TechReviewMode.Single);
                    PLCCommon.instance.SetTechReviewGroupParam(groupRes.ToLower());
                    Response.Redirect("TechReview2.aspx");
                }
                else if (groupRes == "SIGNED"
                    && PLCSession.CheckSectionFlag(section, "USES_REVIEW_PLAN")
                    && PLCDBGlobal.instance.IsCheckListExists(PLCSessionVars1.PLCGlobalAssignmentKey, "REVIEW", PLCDBPanel1.getpanelfield("SECTION"), AssignmentReviewType.SingleAssignment))
                {
                    ReviewPlan1.ReviewEditMode = false;
                    ReviewPlan1.ReviewSection = section;
                    ReviewPlan1.Show();
                }
                else
                {
                    string confirmAnalystLabCodeMsg;
                    
                    // get user confirmation about tasks that will be closed/canceled
                    if (PLCSession.GetSectionFlag(section, "NOTIFY_TASK_CLOSE_ON_SIGN") == "T" && assignmentHasOpenTasks())
                    {
                        hdnGroupCode.Value = groupRes;
                        mbConfirmCloseTasksOnSign.Message = ConfirmCloseTaskMessage();
                        mbConfirmCloseTasksOnSign.Show();
                    }
                    else if (groupRes == "APPROVE" && PLCDBGlobal.instance.ConfirmAnalystLabCodeOnApproval(out confirmAnalystLabCodeMsg))
                    {
                        hdnGroupCode.Value = groupRes;
                        dlgAssignmentMessage.DialogKey = "Approve_NotLabAnalyst";
                        dlgAssignmentMessage.ShowYesNo("Approve", confirmAnalystLabCodeMsg);
                    }
                    else
                        GetSignature(groupRes);
                }                
            }
        }

        /// <summary>
        /// Override Report Preview popup event on UC Customer Title
        /// </summary>
        private void SetPreviewReportEvent()
        {
            PLCCommon_CustomerTitle ct = (PLCCommon_CustomerTitle)Master.FindControl("UC_CustomerTitle_Master2");
            ct.AcceptPreviewOverrideEvent += ReportPreview_AcceptClick;
            ct.SetPreviewReportEvent();
        }

        private bool assignmentHasOpenTasks()
        {
            PLCQuery qryTasks = new PLCQuery();
            qryTasks.SQL = string.Format("SELECT STATUS FROM TV_TASKLIST WHERE EXAM_KEY = {0} AND STATUS <> 'C' AND STATUS <> 'X'", PLCSession.SafeInt(PLCSession.PLCGlobalAssignmentKey));
            return qryTasks.Open() && qryTasks.HasData();
        }

        private string ConfirmCloseTaskMessage()
        {
            string html = @"<div id=""open-task-msg-container"">This assignment has open tasks, and the following actions will be taken:<br /><br />
    <table class=""task-table"">
        <thead>
            <tr>
                <th>Task Type</th>
                <th>Task ID</th>
                <th>Status</th>
                <th>Worklist ID</th>
                <th>Action</th>
            </tr>
        </thead>
        <tbody>{0}</tbody>
    </table><br /> Do you wish to proceed?</div>";

            List<string> taskRows = new List<string>();
            PLCQuery qryTasks = new PLCQuery();
            qryTasks.SQL = string.Format(@"
                SELECT DISTINCT tl.TASK_ID, tt.DESCRIPTION, tl.STATUS, wt.WORKLIST_ID
                FROM TV_TASKLIST tl
                LEFT OUTER JOIN TV_WORKTASK wt on wt.TASK_ID = tl.TASK_ID
                LEFT OUTER JOIN TV_TASKTYPE tt on tt.TASK_TYPE = tl.TASK_TYPE
                WHERE EXAM_KEY = {0} AND STATUS not in ('C','X')",
                PLCSession.SafeInt(PLCSession.PLCGlobalAssignmentKey)
            );

            qryTasks.OpenReadOnly();
            while (!qryTasks.EOF())
            {
                string taskAction = qryTasks.FieldByName("STATUS") == "A" ? "Close" : "Cancel";
                string worklistLink = "";
                if (!String.IsNullOrEmpty(qryTasks.FieldByName("WORKLIST_ID")))
                {
                    worklistLink = String.Format(
                        @"<a href=""#"" title=""Open worklist in Batch Results"" onclick=""OpenBatch({0})"" />{0}</a>",
                        qryTasks.FieldByName("WORKLIST_ID")
                    );
                }
                string tr = String.Format(
                    @"<tr>
                        <td>{0}</td>
                        <td>{1}</td>
                        <td>{2}</td>
                        <td>{3}</td>
                        <td>{4}</td>
                    </tr>",
                    qryTasks.FieldByName("DESCRIPTION"),
                    qryTasks.FieldByName("TASK_ID"),
                    qryTasks.FieldByName("STATUS"),
                    worklistLink,
                    taskAction
                );

                taskRows.Add(tr);
                qryTasks.Next();
            }

            return String.Format(html, String.Join("", taskRows));
        }

        protected void mbConfirmCloseTasksOnSign_OkClick(object sender, EventArgs e)
        {
            var groupRes = hdnGroupCode.Value;
            GetSignature(groupRes);
        }


        private void GetSignature(string groupRes)
        {
            int sigID = PLCDBGlobal.instance.GetAnalystSignatureID();
            if (sigID != -1 && !PLCDBGlobal.instance.IsValidDigitalSignature(sigID))
                messageBox.ShowMsg("Sign", "Invalid Signature. Please re-capture your signature.", 0);
            else
            {
                hdnGroupCode.Value = groupRes;
                VerifySignature1.Analyst = PLCSession.PLCGlobalAnalyst;
                VerifySignature1.Show();
            }
        }

        private void KeepSessionAlive()
        {
            int interval = (Session.Timeout * 60000) - 60000;
            string script = @"
<script type='text/javascript'>
function Reconnect(){
    var img = new Image(1,1);
    img.src = '/PLCWebCommon/Reconnect.aspx?id=' + Math.random();
}
window.setInterval('Reconnect()'," + interval.ToString() + @");
</script>";

            ClientScript.RegisterClientScriptBlock(this.GetType(), "reconnect", script, false);
        }

        private string GetSignedByText(string groupRes)
        {
            string signText = "";
            PLCQuery qrySign = new PLCQuery();
            switch (groupRes)
            {
                case "SIGNED":
                    signText = "The Report has already been signed.";
                    string sBatchKey = "";
                    qrySign = new PLCQuery();
                    if (PLCDBGlobal.instance.IsAssignmentInBatch(PLCSession.PLCGlobalAssignmentKey, ref sBatchKey))
                    {
                        qrySign.SQL = string.Format("SELECT A.NAME, RS.ENTRY_DATE FROM TV_REPTSIG RS " +
                                "INNER JOIN TV_ANALYST A ON RS.ANALYST = A.ANALYST " +
                                "LEFT JOIN TV_REVLIST RL ON RL.SECTION = '{0}' AND RS.TYPE_RES = RL.REVIEW_CODE " +
                                "WHERE RS.BATCH_ASSIGN_KEY = {1} AND (RS.TYPE_RES = 'ANALYST' OR RL.GROUP_RES = 'SIGNED')", PLCDBPanel1.getpanelfield("SECTION"), sBatchKey);
                    }
                    else
                    {
                        qrySign.SQL = string.Format("SELECT A.NAME, RS.ENTRY_DATE FROM TV_REPTSIG RS " +
                                "INNER JOIN TV_ANALYST A ON RS.ANALYST = A.ANALYST " +
                                "INNER JOIN TV_LABEXAM LE ON RS.EXAM_KEY = LE.EXAM_KEY " +
                                "LEFT JOIN TV_REVLIST RL ON RS.TYPE_RES = RL.REVIEW_CODE AND LE.SECTION = RL.SECTION " +
                                "WHERE RS.EXAM_KEY = {0} AND (RS.TYPE_RES = 'ANALYST' OR RL.GROUP_RES = 'SIGNED')", PLCSession.PLCGlobalAssignmentKey);
                    }

                    if (qrySign.Open() && qrySign.HasData())
                        signText = String.Format("The Report has already been signed by {0} on {1}.", qrySign.FieldByName("NAME"), Convert.ToDateTime(qrySign.FieldByName("ENTRY_DATE")).ToShortDateString());
                    break;

                case "REVIEW":
                    signText = "The Report has already been reviewed.";
                    qrySign = QuerySelectedLabExam();
                    if (qrySign.HasData() && qrySign.FieldByName("REVIEWED") == "Y")
                    {
                        PLCQuery qryAnalyst = QueryAnalyst(qrySign.FieldByName("REVIEWED_BY"));
                        if (qryAnalyst.HasData())
                        {
                            signText = String.Format("The Report has already been reviewed by {0} on {1}.", qryAnalyst.FieldByName("NAME"), Convert.ToDateTime(qrySign.FieldByName("REVIEWED_DATE")).ToShortDateString());
                        }
                    }
                    break;

                case "CODISREV":
                    signText = "The Report has already been codis reviewed.";
                    break;

                case "APPROVE":
                    break;

                default:
                    break;
            }

            return signText;
        }

        private string GetSelectedItems()
        {
            string ECNs = string.Empty;
            foreach (GridViewRow GVR in gItems.Rows)
            {
                if (GVR.RowType == DataControlRowType.DataRow)
                {
                    if (((CheckBox)GVR.FindControl("cbSELECT")).Checked)
                    {
                        ECNs += gItems.DataKeys[GVR.RowIndex].Values["ECN"].ToString().Trim() + ",";
                    }
                }
            }
            ECNs = ECNs.TrimEnd(',');

            return ECNs;
        }

        private string CheckProficiencyTasks()
        {
            string msg = string.Empty;
            
            // check if assignment is proficiency
            PLCQuery qryQMSCALSK = new PLCQuery();
            qryQMSCALSK.SQL = "SELECT * FROM TV_QMSCASLK WHERE CASE_KEY = " + PLCSession.PLCGlobalCaseKey + " AND EXAM_KEY = " + PLCSession.PLCGlobalAssignmentKey;
            qryQMSCALSK.Open();

            if (qryQMSCALSK.IsEmpty())
                return string.Empty;

            //get exam code
            PLCQuery qryLabExam = QuerySelectedLabExam();
            string examCode = qryLabExam.HasData() ? qryLabExam.FieldByName("SECTION") : "";

            if (string.IsNullOrEmpty(examCode))
                return string.Empty;

            // get parent tasks
            List<string> liParentTasks = ParentTasks();
            if (liParentTasks.Count <= 0)
                return string.Empty;

            // get subtasks
            List<string> liSubTasks = new List<string>();
            liSubTasks = SubTasks(liParentTasks);
            if (liSubTasks.Count <= 0)
                return string.Empty;

            //get matrix test types based on subtasks
            List<string> liTestTypes = MatrixTestTypesBasedOnSubTasks(examCode, liSubTasks);
            if (liTestTypes.Count <= 0)
                return string.Empty;

            // get arresult records
            List<string> liArresults = new List<string>();
            PLCQuery qryArresult = new PLCQuery();
            qryArresult.SQL = string.Format("SELECT DISTINCT(TEST_TYPE_CODE) FROM TV_ARRESULT WHERE EXAM_KEY = {0} AND (COALESCE(RESULT, '') <> '' OR RESULT IS NOT NULL)", PLCSession.PLCGlobalAssignmentKey);
            qryArresult.Open();

            if (qryArresult.HasData())
            {
                while (!qryArresult.EOF())
                {
                    liArresults.Add(qryArresult.FieldByName("TEST_TYPE_CODE"));
                    qryArresult.Next();
                }
            }

            // compare arresults from matrix test types
            foreach (string testTypeCode in liTestTypes)
            {
                if(!liArresults.Contains(testTypeCode))
                    msg = msg + "<br/> * " + PLCSession.GetCodeDesc("TV_ARTEST", testTypeCode) + " (" + testTypeCode + ")";
            }

            if (!string.IsNullOrEmpty(msg))
                msg = "<div style='max-height:200px;overflow-y:auto;'>Please fill out the following Test Types for this Proficiency Test: " + msg + "</div>";

            return msg;
        }

        private List<string> ParentTasks()
        {
            List<string> liParentTasks = new List<string>();
            PLCQuery qryParentTasks = new PLCQuery();
            qryParentTasks.SQL = string.Format("SELECT DISTINCT(B.PARENT_TASK) " +
                                        "FROM TV_TASKLIST A " +
                                        "LEFT OUTER JOIN TV_TASKTYPE B ON B.TASK_TYPE = A.TASK_TYPE " +
                                        "WHERE EXAM_KEY = {0} AND (COALESCE(B.PARENT_TASK, '') <> '' OR B.PARENT_TASK IS NOT NULL)", PLCSession.PLCGlobalAssignmentKey);
            qryParentTasks.Open();
            if (qryParentTasks.IsEmpty())
                return liParentTasks;

            while (!qryParentTasks.EOF())
            {
                liParentTasks.Add(qryParentTasks.FieldByName("PARENT_TASK"));
                qryParentTasks.Next();
            }

            return liParentTasks;
        }

        private List<string> SubTasks(List<string> liParentTasks)
        {
            List<string> liSubTasks = new List<string>();
            PLCQuery qrySubTasks = new PLCQuery();
            qrySubTasks.SQL = string.Format("SELECT A.TASK_TYPE, B.WRITE " +
                                                "FROM TV_TASKTYPE A " +
                                                "LEFT OUTER JOIN TV_ANALTASK B ON A.TASK_TYPE = B.TASK_TYPE " +
                                                "WHERE PARENT_TASK IN ('{0}') AND B.WRITE = 'T' AND ANALYST = '{1}'", string.Join("','", liParentTasks), PLCSession.PLCGlobalAnalyst);
            qrySubTasks.Open();

            if (qrySubTasks.IsEmpty())
                return liSubTasks;

            while (!qrySubTasks.EOF())
            {
                liSubTasks.Add(qrySubTasks.FieldByName("TASK_TYPE"));
                qrySubTasks.Next();
            }

            return liSubTasks;
        }

        private List<string> MatrixTestTypesBasedOnSubTasks(string examCode, List<string> liSubTasks)
        {
            List<string> liTestTypes = new List<string>();
            PLCQuery qryArtest = new PLCQuery();
            qryArtest.SQL = string.Format("SELECT A.TEST_TYPE_CODE " +
                                    "FROM TV_ARTEST A " +
                                    "LEFT OUTER JOIN TV_ARLINK B ON B.TEST_TYPE_CODE = A.TEST_TYPE_CODE " +
                                    "LEFT OUTER JOIN TV_ARSECTLK C ON C.ANALYSIS_TYPE_CODE = B.ANALYSIS_TYPE_CODE " +
                                    "WHERE A.METHOD_TASK IN ('{0}') AND B.INACTIVE <> 'T' AND C.SECTION = '{1}'", string.Join("','", liSubTasks), examCode);
            qryArtest.Open();
            if (qryArtest.IsEmpty())
                return liTestTypes;

            while (!qryArtest.EOF())
            {
                liTestTypes.Add(qryArtest.FieldByName("TEST_TYPE_CODE"));
                qryArtest.Next();
            }

            return liTestTypes;
        }

        #endregion

        #region Routing

        private void LoadRoutingInfo(bool NeedToValidateUser)
        {
            hdnROUTETO.Value = "";
            hdnROUTEBY.Value = "";
            lblRouteTo.Text = PLCSession.GetSysPrompt("TAB6Assignments.lblRouteTo", "Not Routed");
            lblRouteBy.Text = "";
            lblRouteCode.Text = "";
            lblRouteDate.Text = "";
            tbxRoutingMessage.Text = "";

            PLCQuery qryRoutingInfo = new PLCQuery();
            qryRoutingInfo.SQL = PLCSession.FormatSpecialFunctions(@"SELECT E.ANALYST_ASSIGNED, A.NAME AS ROUTE_TO_NAME, E.ROUTE_TO, R.DESCRIPTION AS ROUTE_CODE, 
E.ROUTE_BY, AR.NAME AS ROUTE_BY_NAME, FORMATDATETIME(E.ROUTE_DATE) AS ROUTE_DATE, E.ROUTE_MESSAGE FROM TV_LABEXAM E 
LEFT OUTER JOIN TV_ANALYST A ON E.ROUTE_TO = A.ANALYST 
LEFT OUTER JOIN TV_ROUTECOD R ON E.ROUTE_CODE = R.ROUTING_CODE
LEFT OUTER JOIN TV_ANALYST AR ON E.ROUTE_BY = AR.ANALYST
WHERE E.CASE_KEY = " + PLCSession.PLCGlobalCaseKey + " AND E.EXAM_KEY = " + GridView1.SelectedDataKey.Values["EXAM_KEY"].ToString());
            qryRoutingInfo.Open();
            if (!qryRoutingInfo.IsEmpty())
            {
                if (NeedToValidateUser)
                {
                    bool CanRouteAny = CanRouteAnyAssignment();
                    if (!CanRouteAny && qryRoutingInfo.FieldByName("ROUTE_TO") != PLCSession.PLCGlobalAnalyst && qryRoutingInfo.FieldByName("ANALYST_ASSIGNED") != PLCSession.PLCGlobalAnalyst)
                    {
                        string strMsg = "This assignment is not currently routed to you.<br />";
                        strMsg += "The routing can only be changed by the person it is routed to or by an authorized user.";
                        messageBox.ShowMsg("Route This Assigment", strMsg, 0);
                        return;
                    }
                }
                //
                MultiView1.ActiveViewIndex = 3;
                if (!String.IsNullOrEmpty(qryRoutingInfo.FieldByName("ROUTE_TO")))
                {
                    hdnROUTETO.Value = qryRoutingInfo.FieldByName("ROUTE_TO");
                    hdnROUTEBY.Value = qryRoutingInfo.FieldByName("ROUTE_BY");
                    lblRouteTo.Text = qryRoutingInfo.FieldByName("ROUTE_TO_NAME");
                    lblRouteBy.Text = qryRoutingInfo.FieldByName("ROUTE_BY_NAME");
                    lblRouteCode.Text = qryRoutingInfo.FieldByName("ROUTE_CODE");
                    lblRouteDate.Text = qryRoutingInfo.FieldByName("ROUTE_DATE");
                    tbxRoutingMessage.Text = qryRoutingInfo.FieldByName("ROUTE_MESSAGE");
                }
            }

            string routingHistorySQL = "SELECT E.ROUTE_DATE, A.NAME, "
                + "R.DESCRIPTION, E.ROUTE_MESSAGE, AR.NAME "
                + "FROM TV_EXAMROUT E "
                + "LEFT OUTER JOIN TV_ANALYST A ON E.ROUTE_TO = A.ANALYST "
                + "LEFT OUTER JOIN TV_ANALYST AR ON E.ROUTED_BY = AR.ANALYST "
                + "LEFT OUTER JOIN TV_ROUTECOD R ON E.ROUTE_CODE = R.ROUTING_CODE "
                + "ORDER BY E.ROUTE_DATE DESC";
            dbgRoutingHistory.PLCSQLString = PLCSession.FormatSpecialFunctions(routingHistorySQL);
            dbgRoutingHistory.PLCSQLString_AdditionalCriteria = "WHERE E.EXAM_KEY = " + PLCSession.PLCGlobalAssignmentKey;
            dbgRoutingHistory.ClearSortExpression();
            dbgRoutingHistory.InitializePLCDBGrid();
        }

        private bool CanRouteAnyAssignment()
        {
            Dictionary<string, object> AnalystSection = GetAnalystSection();

            if (AnalystSection.Count() > 0)
            {
                if (AnalystSection.ContainsKey(PLCDBPanel1.getpanelfield("SECTION").Trim()))
                {
                    Dictionary<string, string> section = (Dictionary<string, string>)AnalystSection[PLCDBPanel1.getpanelfield("SECTION").Trim()];
                    if (section.ContainsKey("ROUTEANYASSIGNMENT"))
                    {
                        if (section["ROUTEANYASSIGNMENT"].ToString().Trim() != "T")
                            return false;
                    }
                }
            }

            return true;
        }

        private void ClearRoutingValues(bool SetUserID)
        {
            if (SetUserID)
                chUserID.SetValue(hdnROUTEBY.Value);
            else
                chUserID.SetValue("");

            chRoutingCodes.SetValue("");
            tbxRouteMessage.Text = "";
            lblRouteRequired.Visible = false;
        }

        private void ShowRoutingPopup()
        {
            mpeRouting.Show();

            SetOnTopOfModal(".dbgrid-routing-history");
        }

        #endregion

        #region Data

        private void SetAttachmentKey()
        {
            if (lbRawData.SelectedIndex > -1)
            {
                //bnDataOpen.Enabled = false;
                if (lbRawData.SelectedItem.Value.Contains('|') && !string.IsNullOrEmpty(lbRawData.SelectedItem.Value.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries)[1]))
                {
                    PLCQuery qryAttachKey = new PLCQuery();
                    qryAttachKey.SQL = string.Format("SELECT PRINTLOG_KEY FROM TV_PRINTLOG WHERE IMAGE_ID = {0}",
                        lbRawData.SelectedItem.Value.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries)[1]);
                    if (qryAttachKey.Open() && qryAttachKey.HasData())
                    {
                        PLCSession.PLCGlobalAttachmentKey = qryAttachKey.FieldByName("PRINTLOG_KEY");
                        //bnDataOpen.Enabled = true;
                    }
                }
            }
        }

        private void SaveExamRawData(int imageKey, string filePath, string description)
        {
            PLCQuery qryRawData = new PLCQuery("SELECT * FROM TV_EXAMIRAW WHERE 0=1");
            qryRawData.Open();
            if (qryRawData.IsEmpty())
            {
                qryRawData.Append();
                qryRawData.SetFieldValue("SEQUENCE", PLCSession.GetNextSequence("EXAMIRAW_SEQ"));
                qryRawData.SetFieldValue("EXAM_KEY", GridView1.SelectedDataKey.Values["EXAM_KEY"].ToString());
                qryRawData.SetFieldValue("DATA_FILE_PATH", filePath);
                qryRawData.SetFieldValue("DESCRIPTION", description);
                qryRawData.SetFieldValue("IMAGE_ID", imageKey);
                qryRawData.Post("TV_EXAMIRAW", 103, 1);
            }
        }

        private bool UploadRawDataFile()
        {

            if (!Directory.Exists(IMAGES_DIR))
            {
                try
                {
                    Directory.CreateDirectory(IMAGES_DIR);
                }
                catch (Exception ex)
                {
                    messageBox.ShowMsg(ex.Message.ToString(), "Please have your Administrator create the " + IMAGES_DIR + " directory on the Web Server.", 1);
                    return false;
                }
            }

            if (fupDataGetFilePath.HasFile)
            {
                fupDataGetFilePath.SaveAs(IMAGES_DIR + fupDataGetFilePath.FileName);
                return true;
            }
            else
            {
                return false;
            }
        }

        private string GetProcessedValue(string value)
        {
            return value.Contains('-') ? value.Substring(value.IndexOf('-') + 1,
                value.Length - (value.IndexOf('-') + 1)).Trim() : value.Trim();
        }

        private void SaveDataType(string type, string desc)
        {
            PLCQuery qryDataType = new PLCQuery();
            qryDataType.SQL = "SELECT * FROM TV_ADTYPE where DATA_TYPE = '" + type + "'";
            qryDataType.Open();
            if (qryDataType.EOF())
            {
                qryDataType.Append();
                qryDataType.SetFieldValue("DATA_TYPE", type);
                qryDataType.SetFieldValue("DESCRIPTION", desc);
                qryDataType.Post("TV_ADTYPE", 7, 10);
            }
        }

        private int SaveRawDataToDatabase()
        {
            string strCaseKey = PLCSession.PLCGlobalCaseKey;
            string printLogKey = PLCSession.GetNextSequence("PRINTLOG_SEQ").ToString();
            int imageKey = PLCSession.GetNextSequence("IMAGES_SEQ");

            int startIdx = fupDataGetFilePath.FileName.LastIndexOf(".") + 1;
            string theDataType = fupDataGetFilePath.FileName.Substring(startIdx).ToUpper();

            //This needs to be here so the LIMS can develop a database of content types.
            SaveDataType(theDataType, fupDataGetFilePath.PostedFile.ContentType);

            PLCQuery qry = new PLCQuery("SELECT * FROM TV_PRINTLOG");
            qry.Open();
            qry.Append();

            // Store data in the row            
            qry.SetFieldValue("PRINTLOG_KEY", printLogKey);
            qry.SetFieldValue("IMAGE_ID", imageKey);
            qry.SetFieldValue("CASE_KEY", strCaseKey);
            qry.SetFieldValue("DOCUMENT_TYPE", "ATTACH");
            qry.SetFieldValue("DOCUMENT_DESCRIPTION", PLCSession.PLCGlobalAttachmentSourceDesc);
            qry.SetFieldValue("DATA_FILE_NAME", fupDataGetFilePath.FileName);
            qry.SetFieldValue("DATA_TYPE", theDataType);
            qry.SetFieldValue("FILE_SOURCE", "EXAMIRAW");
            qry.SetFieldValue("FILE_SOURCE_KEY1", PLCSession.PLCGlobalAssignmentKey);
            qry.Post("TV_PRINTLOG", 31, 3);

            string AuditInfo = "";
            AuditInfo += "PRINTLOG_KEY: " + printLogKey + "\r\n";
            AuditInfo += "IMAGE_ID: " + imageKey + "\r\n";
            AuditInfo += "CASE_KEY: " + strCaseKey + "\r\n";
            AuditInfo += "DOCUMENT_TYPE: ATTACH" + "\r\n";
            AuditInfo += "DOCUMENT_DESCRIPTION: " + PLCSession.PLCGlobalAttachmentSourceDesc + "\r\n";
            AuditInfo += "DATA_FILE_NAME: " + fupDataGetFilePath.FileName + "\r\n";
            AuditInfo += "DATA_TYPE: " + theDataType + "\r\n";
            AuditInfo += "FILE_SOURCE: " + "EXAMIRAW" + "\r\n";
            AuditInfo += "FILE_SOURCE_KEY1: " + PLCSession.PLCGlobalAssignmentKey + "\r\n";

            byte[] blob;
            int fileSize = PLCCommon.instance.ConvertFileToBlob(IMAGES_DIR + fupDataGetFilePath.FileName, out blob, true);

            string entryID = PLCSession.PLCDBUserID + "W" + PLCSession.GetNextSequence("LIMSDOC_SEQ").ToString();

            PLCCommon.instance.SaveRawDataToDatabase(imageKey, null, entryID, theDataType.Substring(0, theDataType.Length >= 3 ? 3 : theDataType.Length),
                null, null, PLCSession.PLCGlobalAnalyst, DateTime.Now, fileSize, null, fupDataGetFilePath.PostedFile.ContentType, blob);

            // Auditlog
            string nextVal = PLCSession.GetNextSequence("AUDITLOG_SEQ").ToString();
            PLCQuery qryAudit = new PLCQuery();
            qryAudit.SQL = "SELECT * FROM TV_AUDITLOG where 0 = 1";
            qryAudit.Open();
            qryAudit.Append();

            DateTime TheDateTime = DateTime.Now;
            qryAudit.SetFieldValue("LOG_STAMP", nextVal);
            qryAudit.SetFieldValue("TIME_STAMP", TheDateTime);
            qryAudit.SetFieldValue("USER_ID", PLCSession.PLCGlobalAnalyst);
            qryAudit.SetFieldValue("PROGRAM", ("iLIMS" + PLCSession.PLCBEASTiLIMSVersion).Substring(0, 8));
            if (PLCSession.PLCGlobalCaseKey != "")
                qryAudit.SetFieldValue("CASE_KEY", PLCSession.PLCGlobalCaseKey);
            if (PLCSession.PLCGlobalECN != "")
                qryAudit.SetFieldValue("EVIDENCE_CONTROL_NUMBER", PLCSession.PLCGlobalECN);
            qryAudit.SetFieldValue("CODE", 31);
            qryAudit.SetFieldValue("SUB_CODE", 3);
            qryAudit.SetFieldValue("ERROR_LEVEL", 0);
            qryAudit.SetFieldValue("ADDITIONAL_INFORMATION", AuditInfo + PLCSession.PLCGlobalAnalystName);
            qryAudit.SetFieldValue("OS_COMPUTER_NAME", PLCSession.GetOSComputerName());
            qryAudit.SetFieldValue("OS_USER_NAME", PLCSession.GetOSUserName());
            qryAudit.SetFieldValue("OS_ADDRESS", PLCSession.GetOSAddress());
            qryAudit.SetFieldValue("BUILD_NUMBER", PLCSession.PLCBEASTiLIMSVersion);
            qryAudit.Post("TV_AUDITLOG", -1, -1);

            return imageKey;
        }

        private void LoadRawData()
        {
            lbRawData.Items.Clear();

            PLCQuery qryRawData = new PLCQuery();
            qryRawData.SQL = "SELECT T.EXAM_KEY, T.SEQUENCE, T.DATA_FILE_PATH, T.DESCRIPTION, T.IMAGE_ID FROM TV_EXAMIRAW T " +
                "WHERE T.EXAM_KEY = :EK ORDER BY T.SEQUENCE";
            qryRawData.SetParam("EK", GridView1.SelectedDataKey.Values["EXAM_KEY"].ToString());

            if (qryRawData.Open() && !qryRawData.IsEmpty())
            {
                foreach (DataRow dr in qryRawData.PLCDataTable.Rows)
                {
                    ListItem item = new ListItem((dr["DESCRIPTION"] == null || string.IsNullOrEmpty(dr["DESCRIPTION"].ToString()) ? "" :
                        dr["DESCRIPTION"].ToString() + " - ") + dr["DATA_FILE_PATH"].ToString(), dr["SEQUENCE"].ToString() +
                        (dr["IMAGE_ID"] == null || string.IsNullOrEmpty(dr["IMAGE_ID"].ToString()) ? "" : "|" + dr["IMAGE_ID"].ToString()));
                    lbRawData.Items.Add(item);
                }
            }

            if (lbRawData.Items.Count == 0)
            {
                bnDataAdd.Enabled = true;
                bnDataOpen.Enabled = false;
                bnDataEdit.Enabled = false;
                bnDataRemove.Enabled = false;
            }
            else if (lbRawData.Items.Count > 0 && lbRawData.SelectedIndex == -1)
            {
                lbRawData.SelectedIndex = 0;
                bnDataAdd.Enabled = true;
                bnDataOpen.Enabled = true;
                bnDataEdit.Enabled = true;
                bnDataRemove.Enabled = true;
            }
            MultiView1.ActiveViewIndex = 4;

            SetAttachmentKey();
        }

        private void DeleteAttachment()
        {
            PLCQuery qryLabExam = QuerySelectedLabExam();
            string AuditInfo = "";

            if (qryLabExam.HasData() && PLCSession.CheckSectionFlag(qryLabExam.FieldByName("SECTION"), "RAW_DATA_TO_DB"))
            {
                string sCondition = " FILE_SOURCE = 'EXAMIRAW' AND FILE_SOURCE_KEY1 = " + PLCSession.PLCGlobalAssignmentKey;

                PLCQuery qryPrintLog = new PLCQuery();
                qryPrintLog.SQL = "Select * FROM TV_PRINTLOG WHERE PRINTLOG_KEY = " + PLCSession.PLCGlobalAttachmentKey;
                qryPrintLog.Open();
                if (!qryPrintLog.EOF())
                {
                    AuditInfo += sCondition + "\r\n";
                    AuditInfo += "PRINTLOG_KEY: " + qryPrintLog.FieldByName("PRINTLOG_KEY") + "\r\n";
                    AuditInfo += "DOCUMENT_TYPE: " + qryPrintLog.FieldByName("DOCUMENT_TYPE") + "\r\n";
                    AuditInfo += "DOCUMENT_DESCRIPTION: " + qryPrintLog.FieldByName("DOCUMENT_DESCRIPTION") + "\r\n";
                    AuditInfo += "DATA_FILE_NAME: " + qryPrintLog.FieldByName("DATA_FILE_NAME") + "\r\n";
                    AuditInfo += "DATA_TYPE: " + qryPrintLog.FieldByName("DATA_TYPE") + "\r\n";

                    string sImageID = qryPrintLog.FieldByName("IMAGE_ID");
                    PLCQuery qryDeleteImage = new PLCQuery();
                    //IMAGES
                    qryDeleteImage.SQL = "DELETE FROM TV_IMAGES where IMAGE_ID = " + sImageID;
                    qryDeleteImage.ExecSQL(PLCSession.GetConnectionString());
                    //PRINTLOG
                    qryDeleteImage.SQL = "DELETE FROM TV_PRINTLOG WHERE PRINTLOG_KEY = " + PLCSession.PLCGlobalAttachmentKey;
                    qryDeleteImage.ExecSQL(PLCSession.GetConnectionString());
                }
            }

            // Auditlog
            string nextVal = PLCSession.GetNextSequence("AUDITLOG_SEQ").ToString();
            PLCQuery qryAudit = new PLCQuery();
            qryAudit.SQL = "SELECT * FROM TV_AUDITLOG where 0 = 1";
            qryAudit.Open();
            qryAudit.Append();

            DateTime TheDateTime = DateTime.Now;
            qryAudit.SetFieldValue("LOG_STAMP", nextVal);
            qryAudit.SetFieldValue("TIME_STAMP", TheDateTime);
            qryAudit.SetFieldValue("USER_ID", PLCSession.PLCGlobalAnalyst);
            qryAudit.SetFieldValue("PROGRAM", ("iLIMS" + PLCSession.PLCBEASTiLIMSVersion).Substring(0, 8));
            if (PLCSession.PLCGlobalCaseKey != "")
                qryAudit.SetFieldValue("CASE_KEY", PLCSession.PLCGlobalCaseKey);
            if (PLCSession.PLCGlobalECN != "")
                qryAudit.SetFieldValue("EVIDENCE_CONTROL_NUMBER", PLCSession.PLCGlobalECN);
            qryAudit.SetFieldValue("CODE", 31);
            qryAudit.SetFieldValue("SUB_CODE", 1);
            qryAudit.SetFieldValue("ERROR_LEVEL", 0);
            qryAudit.SetFieldValue("ADDITIONAL_INFORMATION", AuditInfo + PLCSession.PLCGlobalAnalystName);
            qryAudit.SetFieldValue("OS_COMPUTER_NAME", PLCSession.GetOSComputerName());
            qryAudit.SetFieldValue("OS_USER_NAME", PLCSession.GetOSUserName());
            qryAudit.SetFieldValue("OS_ADDRESS", PLCSession.GetOSAddress());
            qryAudit.SetFieldValue("BUILD_NUMBER", PLCSession.PLCBEASTiLIMSVersion);
            qryAudit.Post("TV_AUDITLOG", -1, -1);
        }

        #endregion

        #region Additional Analysts

        private void BindAssignmentAnalysts()
        {
            string sql = string.Format(@"SELECT E.ANALYST, A.NAME FROM TV_EXAMANAL E
INNER JOIN TV_ANALYST A ON E.ANALYST = A.ANALYST
WHERE EXAM_KEY = {0}
ORDER BY ORDER_RES", PLCSession.PLCGlobalAssignmentKey);
            PLCQuery query = new PLCQuery(sql);
            query.Open();
            lstAnalysts.DataTextField = "NAME";
            lstAnalysts.DataValueField = "ANALYST";
            lstAnalysts.DataSource = query.PLCDataTable;
            lstAnalysts.DataBind();
        }

        private void BindSectionAnalysts(string section, string filterName)
        {
            string sql = string.Format(@"SELECT A.NAME AS ""Name"", S.ANALYST AS ""Code"", 
CASE WHEN REVIEW_REPORTS = 'T' THEN 'Yes' ELSE 'No' END AS ""Review Reports"", 
CASE WHEN WRITE_REPORTS = 'T' THEN 'Yes' ELSE 'No' END AS ""Write Reports"", 
CASE WHEN APPROVE_REPORTS = 'T' THEN 'Yes' ELSE 'No' END AS ""Approve Reports""
FROM TV_ANALSECT S INNER JOIN TV_ANALYST A ON S.ANALYST = A.ANALYST AND A.ACCOUNT_DISABLED <> 'T'
WHERE S.SECTION = ? AND WRITE_REPORTS = 'T' {0}
ORDER BY A.NAME", (string.IsNullOrEmpty(filterName) ? "" : "AND LOWER(A.NAME) LIKE LOWER(?)"));
            PLCQuery query = new PLCQuery(sql);
            query.AddSQLParameter("SECTION", section);
            if (!string.IsNullOrEmpty(filterName))
                query.AddSQLParameter("NAME", "%" + filterName + "%");
            query.Open();
            grdSA.DataSource = query.PLCDataTable;
            grdSA.DataBind();
            grdSA.CellPadding = grdSA.CellSpacing = 2;
            grdSA.Width = Unit.Percentage(100);
            grdSA.SelectedIndex = -1;
        }

        #endregion

        #region Big View Items

        private void ProcessBigViewItems()
        {
            /*
                        List<string> PREV_SELECTED_ITEM = new List<string>();
                        foreach (DataRow item in PLCSession.PLCGeneralDataSet.Tables["ASSIGNMENTITEMS"].Rows)
                        {
                            if (item["SELECTED"].ToString() == "T")
                            {
                                PREV_SELECTED_ITEM.Add(item["ECN"].ToString());
                            }
                        }

                        string ECN_REMOVE_FROM_ASSIGNMENT = string.Empty;
                        foreach (GridViewRow Item in gvBigViewItems.Rows)
                        {
                            if (Item.RowType == DataControlRowType.DataRow)
                            {
                                if (((CheckBox)Item.Cells[0].FindControl("cbSELECT")).Checked)
                                {
                                    //if (((HiddenField)Item.Cells[0].FindControl("EXAM_KEY")).Value.ToString() == PLCSession.PLCGlobalAssignmentKey)
                                    if (gvBigViewItems.DataKeys[Item.RowIndex].Values["EXAM_KEY"].ToString() == PLCSession.PLCGlobalAssignmentKey)
                                    {
                                        UpdateBigViewItem(gvBigViewItems.DataKeys[Item.RowIndex].Value.ToString(),
                                                ((TextBox)Item.Cells[5].FindControl("ITEMDESC")).Text,
                                                ((TextBox)Item.Cells[7].FindControl("QUANTITY")).Text,
                                                ((DropDownList)Item.Cells[10].FindControl("ddItemSourceField")).SelectedValue
                                                );
                                    }
                                    else
                                    {
                                        InsertBigViewItem(gvBigViewItems.DataKeys[Item.RowIndex].Value.ToString(),
                                            ((TextBox)Item.Cells[5].FindControl("ITEMDESC")).Text,
                                            ((TextBox)Item.Cells[7].FindControl("QUANTITY")).Text,
                                           ((DropDownList)Item.Cells[10].FindControl("ddItemSourceField")).SelectedValue
                                        );
                                    }
                                }
                                else
                                {
                                    if (PREV_SELECTED_ITEM.Contains(gvBigViewItems.DataKeys[Item.RowIndex].Value.ToString()))
                                    {
                                        if (ECN_REMOVE_FROM_ASSIGNMENT.Length > 0)
                                            ECN_REMOVE_FROM_ASSIGNMENT += ",";
                                        ECN_REMOVE_FROM_ASSIGNMENT += gvBigViewItems.DataKeys[Item.RowIndex].Value.ToString();
                                    }
                                }
                            }
                        }
                        //     
                        if (ECN_REMOVE_FROM_ASSIGNMENT.Length > 0)
                            RemoveBigViewItemAssignment(ECN_REMOVE_FROM_ASSIGNMENT);
                        //
                        SetControlEdit(false);
            */
        }

        private void RemoveBigViewItemAssignment(string ECN_COLLECTION)
        {
            string[] itemECNs = ECN_COLLECTION.Split(',');

            foreach (string ecn in itemECNs)
            {
                DeleteItemAssignmentRecords(PLCSession.PLCGlobalAssignmentKey, ecn, 0);
            }
        }

        private void UpdateBigViewItem(string ECN, string ITEM_DESC, string QUANTITY, string ITM_SRC)
        {

            PLCQuery qryITASSIGN = new PLCQuery("SELECT * FROM TV_ITASSIGN WHERE EXAM_KEY = " + PLCSession.PLCGlobalAssignmentKey + " AND EVIDENCE_CONTROL_NUMBER = " + ECN);
            qryITASSIGN.Open();
            if (!qryITASSIGN.IsEmpty())
            {
                qryITASSIGN.Edit();
                qryITASSIGN.SetFieldValue("EXAM_KEY", PLCSession.PLCGlobalAssignmentKey);
                qryITASSIGN.SetFieldValue("ITA_ITEM_DESCRIPTION", ITEM_DESC);
                qryITASSIGN.SetFieldValue("REPORT_DESCRIPTION", ITEM_DESC);
                qryITASSIGN.SetFieldValue("ITA_QUANTITY", QUANTITY);
                if (ITM_SRC != "")
                {
                    qryITASSIGN.SetFieldValue("ITEM_SOURCE", ITM_SRC);
                }
                qryITASSIGN.Post("TV_ITASSIGN", 29, 1);
            }
        }

        private void InsertBigViewItem(string ECN, string ITEM_DESC, string QUANTITY, string ITM_SRC)
        {
            PLCQuery qryITASSIGN = new PLCQuery("SELECT * FROM TV_ITASSIGN WHERE 0=1");
            qryITASSIGN.Open();
            qryITASSIGN.Append();

            qryITASSIGN.SetFieldValue("EXAM_KEY", PLCSession.PLCGlobalAssignmentKey);
            qryITASSIGN.SetFieldValue("EVIDENCE_CONTROL_NUMBER", ECN);
            qryITASSIGN.SetFieldValue("ITA_QUANTITY", QUANTITY);
            qryITASSIGN.SetFieldValue("ITA_ITEM_DESCRIPTION", ITEM_DESC);
            qryITASSIGN.SetFieldValue("REPORT_DESCRIPTION", ITEM_DESC);
            if (ITM_SRC != "")
            {
                qryITASSIGN.SetFieldValue("ITEM_SOURCE", ITM_SRC);
            }
            else
            {
                string defaultItemSource = PLCDBGlobal.instance.GetDefaultAssignmentItemSource(Convert.ToInt32(ECN));
                if (!string.IsNullOrEmpty(defaultItemSource))
                    qryITASSIGN.SetFieldValue("ITEM_SOURCE", defaultItemSource);
            }
            qryITASSIGN.Post("TV_ITASSIGN", 29, 2);
        }

        private string GetReportNumber()
        {
            PLCQuery qryLABEXAM = new PLCQuery("SELECT REPORT_NUMBER FROM TV_LABEXAM WHERE EXAM_KEY = " + PLCSession.PLCGlobalAssignmentKey);
            qryLABEXAM.Open();
            if (!qryLABEXAM.IsEmpty())
            {
                if (!String.IsNullOrEmpty(qryLABEXAM.PLCDataTable.Rows[0]["REPORT_NUMBER"].ToString()))
                    return qryLABEXAM.PLCDataTable.Rows[0]["REPORT_NUMBER"].ToString();
            }
            //
            return string.Empty;
        }

        private void SetControlEdit(bool IsEditMode)
        {
            BigViewEdit = IsEditMode;
            //            bnBigViewEdit.Enabled = !IsEditMode;
            //            bnBigViewOK.Enabled = IsEditMode;
            //            bnBigViewCancel.Text = IsEditMode ? "Cancel" : "Close";
        }

        private void SaveLinkedCases()
        {
            /*
                        //Get all Evidence Control Numbers not in current Exam/Case
                        List<string> listECN = new List<string>();
                        string currentCaseKey = PLCSession.PLCGlobalCaseKey;
                        string currentExamKey = PLCSession.PLCGlobalAssignmentKey;

                        string sql =
                            "SELECT ITA.EVIDENCE_CONTROL_NUMBER FROM TV_ITASSIGN ITA WHERE EXAM_KEY = " + currentExamKey +
                            " AND EVIDENCE_CONTROL_NUMBER NOT IN(SELECT EVIDENCE_CONTROL_NUMBER FROM TV_LABITEM" +
                            " WHERE CASE_KEY = " + currentCaseKey + ")";
                        PLCQuery qryECN = new PLCQuery(PLCSession.FormatSpecialFunctions(sql));
                        qryECN.Open();

                        foreach (DataRow row in qryECN.PLCDataTable.Rows)
                        {
                            string ecn = row["EVIDENCE_CONTROL_NUMBER"].ToString().Trim();
                            if (!listECN.Contains(ecn) && (ecn != string.Empty))
                            {
                                listECN.Add(ecn);
                            }
                        }

                        //Saving
                        foreach (RepeaterItem item in repBigViewLink.Items)
                        {
                            if (item.FindControl("grdBigViewLink") != null)
                            {
                                short linkedItemCount = 0;
                                GridView grid = (GridView)item.FindControl("grdBigViewLink");
                                foreach (GridViewRow row in grid.Rows)
                                {
                                    if (((CheckBox)row.FindControl("cbSELECT")).Checked)
                                    {
                                        string ecn = ((HiddenField)row.FindControl("hdnECN")).Value.Trim();
                                        string labItemNumber = ((TextBox)row.FindControl("txtLAB_ITEM_NUMBER")).Text.Trim();
                                        string itemSource = ((DropDownList)row.FindControl("ddItemSourceField")).SelectedValue;

                                        ((HiddenField)row.FindControl("hdnITEM_SOURCE")).Value = itemSource;

                                        string qry = "SELECT * FROM TV_ITASSIGN WHERE EXAM_KEY = " + currentExamKey +
                                            " AND EVIDENCE_CONTROL_NUMBER = " + ecn;

                                        PLCQuery qrySave = new PLCQuery(qry);
                                        qrySave.Open();

                                        //Record does no exist
                                        if (qrySave.IsEmpty())
                                        {
                                            qrySave.Append();
                                            qrySave.SetFieldValue("EXAM_KEY", currentExamKey);
                                            qrySave.SetFieldValue("EVIDENCE_CONTROL_NUMBER", ecn);
                                            qrySave.SetFieldValue("ITEM_NUMBER", labItemNumber);
                                            qrySave.SetFieldValue("ITEM_SOURCE", itemSource);
                                            qrySave.Post("TV_ITASSIGN", 29, 2);
                                        }
                                        //Edit record
                                        else
                                        {
                                            qrySave.Edit();
                                            qrySave.SetFieldValue("EXAM_KEY", currentExamKey);
                                            qrySave.SetFieldValue("EVIDENCE_CONTROL_NUMBER", ecn);
                                            qrySave.SetFieldValue("ITEM_NUMBER", labItemNumber);
                                            qrySave.SetFieldValue("ITEM_SOURCE", itemSource);
                                            qrySave.Post("TV_ITASSIGN", 29, 1);

                                            if (listECN.Contains(ecn))
                                                listECN.Remove(ecn);
                                        }
                                        linkedItemCount++;
                                    }
                                }

                                if (linkedItemCount == 0)
                                    item.Visible = false;
                            }
                        }

                        //Delete unchecked records
                        string ECNs = string.Empty;
                        foreach (string ecn in listECN)
                        {
                            ECNs += ecn + ",";
                        }
                        ECNs = ECNs.TrimStart(',').TrimEnd(',');
                        if (ECNs.Length > 0)
                        {
                            PLCQuery qryToDelete =
                                new PLCQuery("SELECT * FROM TV_ITASSIGN WHERE EXAM_KEY = " + currentExamKey +
                                             " AND EVIDENCE_CONTROL_NUMBER IN (" + ECNs + ")");
                            qryToDelete.Open();
                            if (qryToDelete.HasData())
                            {
                                string recordDesc = "";
                                while (!qryToDelete.EOF())
                                {
                                    string ecn = qryToDelete.FieldByName("EVIDENCE_CONTROL_NUMBER");
                                    recordDesc +=
                                        "EXAM_KEY: [" + qryToDelete.FieldByName("EXAM_KEY") + "],\r\n" +
                                        "EVIDENCE_CONTROL_NUMBER: [" + qryToDelete.FieldByName("EVIDENCE_CONTROL_NUMBER") + "],\r\n" +
                                        "ITEM_NUMBER: [" + qryToDelete.FieldByName("ITEM_NUMBER") + "],\r\n" +
                                        "ITA_ITEM_TYPE: [" + qryToDelete.FieldByName("ITA_ITEM_TYPE") + "],\r\n" +
                                        "ITA_PACKAGING_CODE: [" + qryToDelete.FieldByName("ITA_PACKAGING_CODE") + "],\r\n" +
                                        "ITA_QUANTITY: [" + qryToDelete.FieldByName("ITA_QUANTITY") + "],\r\n" +
                                        "ITA_ITEM_DESCRIPTION: [" + qryToDelete.FieldByName("ITA_ITEM_DESCRIPTION") + "],\r\n" +
                                        "ITEM_SOURCE: [" + qryToDelete.FieldByName("ITEM_SOURCE") + "],\r\n" +
                                        "REPORT_DESCRIPTION: [" + qryToDelete.FieldByName("REPORT_DESCRIPTION") + "],\r\n" +
                                        "SECTION_ITEM_NUMBER: [" + qryToDelete.FieldByName("SECTION_ITEM_NUMBER") + "],\r\n" +
                                        "SUB_ASSIGNMENT: [" + qryToDelete.FieldByName("SUB_ASSIGNMENT") + "],\r\n" +
                                        "DELIVERY_STATUS: [" + qryToDelete.FieldByName("DELIVERY_STATUS") + "]";

                                    PLCQuery qryDelete = new PLCQuery("DELETE TV_ITASSIGN WHERE EXAM_KEY = " + currentExamKey +
                                                                      " AND EVIDENCE_CONTROL_NUMBER = " + ecn);
                                    qryDelete.ExecSQL();
                                    PLCDBGlobal.instance.WriteAuditLog("27", "1", "0", "DELETED - " + recordDesc);

                                    qryToDelete.Next();
                                }
                            }
                        }
            */
        }

        private void SetBigViewButtonColor()
        {
            if (GridView1.Rows.Count <= 0)
                return;

            string currentExamKey = PLCSession.PLCGlobalAssignmentKey;
            string currentCaseKey = PLCSession.PLCGlobalCaseKey;

            string sql =
                "SELECT EVIDENCE_CONTROL_NUMBER FROM TV_ITASSIGN WHERE EXAM_KEY = " + currentExamKey +
                " AND EVIDENCE_CONTROL_NUMBER NOT IN(SELECT EVIDENCE_CONTROL_NUMBER FROM TV_LABITEM " +
                "WHERE CASE_KEY = " + currentCaseKey + ")";

            PLCQuery qryCheckLinks = new PLCQuery(PLCSession.FormatSpecialFunctions(sql));
            qryCheckLinks.Open();
            if (qryCheckLinks.IsEmpty())
            {
                bnBigView.ForeColor = Color.Black;
            }
            else
            {
                bnBigView.ForeColor = Color.Red;
            }
        }

        #endregion


        #region Service Request
        private void CreateServiceRequest()
        {
            string srmasterkey = PLCSession.GetNextSequence("SRMASTER_SEQ").ToString();
            PLCQuery qrySRMaster = new PLCQuery("SELECT * FROM TV_SRMASTER WHERE 1=0");
            qrySRMaster.Open();
            qrySRMaster.Append();
            qrySRMaster.SetFieldValue("SR_MASTER_KEY", srmasterkey);
            qrySRMaster.SetFieldValue("CASE_KEY", PLCSession.PLCGlobalCaseKey);
            qrySRMaster.SetFieldValue("STATUS", "3");
            qrySRMaster.SetFieldValue("REQUESTED_BY", PLCSession.PLCGlobalAnalyst);
            qrySRMaster.SetFieldValue("STATUS_BY", PLCSession.PLCGlobalAnalyst);
            qrySRMaster.SetFieldValue("REQUESTED_DATE", DateTime.Now.ToString(PLCSession.GetDateFormat()));
            qrySRMaster.SetFieldValue("STATUS_DATE", DateTime.Now.ToString(PLCSession.GetDateFormat()));
            qrySRMaster.Post("TV_SRMASTER", 7000, 50);

            foreach (GridViewRow row in gItems.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    string ecn = ((HiddenField)row.FindControl("hdnItemNameKey")).Value;
                    string itemnumber = ((TextBox)row.FindControl("ITEMNUM")).Text;

                    if (((CheckBox)row.FindControl("cbSELECT")).Checked)
                    {
                        string detailKey = PLCSession.GetNextSequence("SRDETAIL_SEQ").ToString();
                        PLCQuery qrySRDetail = new PLCQuery();
                        qrySRDetail.SQL = "SELECT * FROM TV_SRDETAIL WHERE 0 = 1";
                        qrySRDetail.Open();
                        qrySRDetail.Append();
                        qrySRDetail.SetFieldValue("SR_DETAIL_KEY", detailKey);
                        qrySRDetail.SetFieldValue("SR_MASTER_KEY", srmasterkey);
                        qrySRDetail.SetFieldValue("EVIDENCE_CONTROL_NUMBER", ecn);
                        qrySRDetail.SetFieldValue("SECTION", PLCDBPanel1.getpanelfield("SECTION"));
                        qrySRDetail.SetFieldValue("EXAM_KEY", PLCSession.PLCGlobalAssignmentKey);
                        qrySRDetail.SetFieldValue("STATUS_CODE", "3");
                        qrySRDetail.SetFieldValue("STATUS_DATE", DateTime.Now.ToString(PLCSession.GetDateFormat()));
                        qrySRDetail.SetFieldValue("STATUS_BY", PLCSession.PLCGlobalAnalyst);
                        qrySRDetail.Post("TV_SRDETAIL", 7000, 53);

                        int historyKey = PLCSession.GetNextSequence("SRHIST_SEQ");

                        PLCQuery qrySRHistory = new PLCQuery();
                        qrySRHistory.SQL = "SELECT * FROM TV_SRHIST WHERE 0 = 1";
                        qrySRHistory.Open();
                        qrySRHistory.Append();
                        qrySRHistory.SetFieldValue("SR_HISTORY_KEY", historyKey);
                        qrySRHistory.SetFieldValue("SR_MASTER_KEY", srmasterkey);
                        qrySRHistory.SetFieldValue("SR_DETAIL_KEY", detailKey);
                        qrySRHistory.SetFieldValue("EXAM_KEY", PLCSession.PLCGlobalAssignmentKey);
                        qrySRHistory.SetFieldValue("STATUS_DATE", DateTime.Now.ToString(PLCSession.GetDateFormat()));
                        qrySRHistory.SetFieldValue("STATUS_CODE", "3");
                        qrySRHistory.SetFieldValue("STATUS_BY", PLCSession.PLCGlobalAnalyst);
                        qrySRHistory.Post("TV_SRHIST", 7000, 62);
                    }
                        
                }
            }
        }


        //private void TestRept()
        //{
        //    String templatepath = Server.MapPath("~/templates/fx.docx");
        //    String inipath = Server.MapPath("~/templates/fx.ini");
        //    String reportpath = Server.MapPath("~/templates/report.docx");
        //    TemplateIni ini = new TemplateIni(inipath, PLCSession.PLCGlobalAnalyst);          
        //    ini.setContextCaseKey(PLCSession.PLCGlobalCaseKey);
        //    ini.setContextExamKey(PLCSession.PLCGlobalAssignmentKey);
        //    ini.generateDocument(templatepath, reportpath, true);
        //}

        private void SetServiceRequestButtonColor(bool checkRpt, bool checkData)
        {
            if (checkRpt)
            {
                // Check if RPT exists.
                string rptPath = PLCSession.FindCrystalReport("srequest.rpt");

                if (!string.IsNullOrEmpty(rptPath))
                    bnSRPrint.Style.Add("color", "Red");
                else
                    bnSRPrint.Style.Add("color", "Black");
            }
            else if (checkData)
            {
                // Check if there is data to be printed.
                string currentExamKey = PLCSession.PLCGlobalAssignmentKey;

                if (!string.IsNullOrEmpty(currentExamKey))
                {
                    PLCQuery qrySRDetail = new PLCQuery();
                    qrySRDetail.SQL = "SELECT COUNT(SR_MASTER_KEY) COUNT FROM TV_SRDETAIL SR WHERE SR.EXAM_KEY = ?";
                    qrySRDetail.AddSQLParameter("EXAM_KEY", currentExamKey);
                    qrySRDetail.Open();

                    if (qrySRDetail.iFieldByName("COUNT") > 0)
                        bnSRPrint.Style.Add("color", "Red");
                    else
                        bnSRPrint.Style.Add("color", "Black");
                }
            }
        }

        #endregion

        /// <summary>
        /// Retrieves the Adams Web Url
        /// </summary>
        /// <returns>Returns the Adams Web URL</returns>
        private string GenerateAdamsWebURL()
        {
            string strURL = string.Empty;

            // construct the basic url call with default values for now
            strURL = PLCHelperFunctions.GenerateAdamsWebURL(false);

            hdnForayURL.Value = strURL;

            return strURL;
        }

        /// <summary>
        /// Retrieve the folder path from the client pc where foray exports the assets
        /// </summary>
        /// <returns>Returns the folder path</returns>
        private string GetForayExportAssetPath()
        {
            string forayExportPath = string.Empty;

            // for now we will set a default parent folder
            forayExportPath = GetForayStoragePathFromDownloadFolder().Replace("\\", "\\\\");

            // next we attach the token id used 
            forayExportPath = string.Format("{0}\\\\{1}", forayExportPath, PLCHelperFunctions.GenerateForayToken(false));

            return forayExportPath;
        }

        /// <summary>
        /// Retrieve the downloads folder from the client pc
        /// </summary>
        /// <returns>Returns the downloads folder</returns>
        private string GetForayStoragePathFromDownloadFolder()
        {
            string strForayPath = string.Empty;

            // retrieve the downloads folder
            strForayPath = PLCHelperFunctions.KnownFolders.GetPath(PLCHelperFunctions.KnownFolder.Downloads);

            return strForayPath;
        }

        /// <summary>
        /// Sets the foray ocx parameters into a hidden field
        /// </summary>
        /// <returns>Returns true if no errors are found</returns>
        private bool SetForayOcxParametersToHiddenField()
        {
            string ocxParameterString = string.Empty;

            //IVFIXUP - Using CLient Side URL
            // set the parameter string
            ocxParameterString = string.Format("{{\"CASEKEY\":\"{0}\",\"EXAMKEY\":\"{1}\",\"PATH\":\"{2}\",\"IMAGEVAULTSERVICEURL\":\"{3}\"}}",
                                    PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalAssignmentKey, GetForayExportAssetPath(),
                                    PLCSession.GetLabCtrlFlag("USES_IMAGE_VAULT_SERVICE").Equals("T") ? (new PLCHelperFunctions()).GetCLientSideImageVaultService() : string.Empty);

            hdnForayExportParameters.Value = ocxParameterString;

            return true;
        }

        private void SavePreviewOnAccept(out string reviewType)
        {
            PLCApproveReportPreview approvePreview = (PLCApproveReportPreview)((UserControl)this.Page.Master.FindControl("UC_CustomerTitle_Master2")).FindControl("ReportPreview");
            approvePreview.ExamKey = PLCSession.PLCGlobalAssignmentKey;
            reviewType = approvePreview.ReviewType;
            approvePreview.SaveReportToPDFData();
        }

        private void ShowMessage(string message)
        {
            ScriptManager.RegisterStartupScript(this, this.GetType(), "_alert", string.Format("Alert('{0}');", message), true);
        }

        /// <summary>
        /// Indicates if the analyst can edit report for others.
        /// </summary>
        /// <param name="assigmentSection">Assignment section to validate with the analyst</param>
        private bool CanAnalystEditReportForOthers(string assigmentSection)
        {
            return PLCSession.GetUserAnalSect(assigmentSection, "EDIT_REPORTS_FOR_OTHERS").ToString().ToUpper() == "T";
        }

        /// <summary>
        /// Tells if the current analyst can use the notes feature.
        /// </summary>
        private bool CanUseNotesFeature()
        {
            bool canUseNotesFeature = true;
            bool analystHasAccessToAssignment = false;
            bool notesEnabled = false;

            string currentAnalyst = PLCSession.PLCGlobalAnalyst;
            string examKey = PLCSession.PLCGlobalAssignmentKey;
            string assignmentSection = PLCDBPanel1.getpanelfield("SECTION");

            // check if the current analyst has access to the assignment.
            analystHasAccessToAssignment = IsAnalystAssignedToAssignment(currentAnalyst, examKey) || CanAnalystEditReportForOthers(assignmentSection) || IsAdditionalAnalystForAssignment(currentAnalyst, examKey);

            // validate if the notes feature is enabled on the current section.
            notesEnabled = PLCSession.CheckSectionFlag(assignmentSection, "USES_NOTES");

            // check if the current analyst  can use the feature.
            canUseNotesFeature = notesEnabled && analystHasAccessToAssignment;

            return canUseNotesFeature;
        }

        /// <summary>
        /// Indicates if the analyst is considered an additional analyst for the assignment.
        /// </summary>
        /// <param name="analyst">Analyst to verify</param>
        /// <param name="examKey">Assignment to check</param>
        private bool IsAdditionalAnalystForAssignment(string analyst, string examKey)
        {
            return PLCDBGlobal.instance.IsAdditionalAnalyst(examKey, analyst);
        }

        /// <summary>
        /// Tells if the analyst is assigned to the assignment.
        /// </summary>
        /// <param name="analyst">Analyst to verify</param>
        /// <param name="examKey">Assignment to check</param>
        private bool IsAnalystAssignedToAssignment(string analyst, string examKey)
        {
            bool isAnalystAssignedToAssignment = false;
            PLCQuery queryObject = null;
            string queryString = string.Empty;

            // set the query
            queryString = "SELECT ANALYST_ASSIGNED FROM TV_LABASSIGN WHERE EXAM_KEY = ? AND ANALYST_ASSIGNED = ?";
            queryObject = new PLCQuery(queryString);
            queryObject.AddParameter("EXAM_KEY", examKey);
            queryObject.AddParameter("ANALYST_ASSIGNED", analyst);
            queryObject.Open();
            if (queryObject.HasData())
                isAnalystAssignedToAssignment = true;

            return isAnalystAssignedToAssignment;
        }

        private string RedirectToMatrixDefaultTab(string matrixDefaultTab, bool showMatrixItemsTab, bool showTaskListPage, bool showRevisionsTab, bool hideMatrixReportsTab, bool hideMatrixGeneralWorksheets)
        {
            string url = null;
            switch (matrixDefaultTab)
            {
                case "TASK":
                        if (showTaskListPage)
                        {
                            SetNotesSequence();
                            url = "~/AssignmentTaskList.aspx";
                        }
                        break;
                case "ITEM":
                        if (showMatrixItemsTab && PLCSession.PLCGlobalMatrixEditable != "F")
                        {
                            SetNotesSequence();
                            url = PLCSession.GetLabCtrl("USES_MATRIX_ITEM_PREVIEW") == "T" ? "~/AssignmentItemM.aspx" : "~/AssignmentItem.aspx";
                        }
                        break;
                case "WORKSHEET":
                        if (!hideMatrixGeneralWorksheets)
                        {
                            SetNotesSequence();
                            url = "~/AssignmentWorksheet.aspx";
                        }
                        break;
                case "REVISION":
                        if (showRevisionsTab)
                        {
                            SetNotesSequence();
                            url = "~/AssignmentReport_Revisions.aspx";
                        }
                        break;
                case "REPORT":
                        if (!hideMatrixReportsTab)
                        {
                            SetNotesSequence();
                            url = "~/AssignmentReport.aspx";
                        }
                        break;

                default:
                    url = null;
                    break;
            }

            return url;
        }

        private void SetLabCaseHeader()
        {
            MasterPage mp = (MasterPage)Master;
            mp.SetLabCaseHeader();
        }

        private void SetOnTopOfModal(string selector)
        {
            string script = string.Format("setOnTopOfModal('{0}')", selector);

            ScriptManager.RegisterStartupScript(Page,
                Page.GetType(),
                "_setOnTopOfModal",
                script,
                true);
        }

        private void ServerAlert(string title, string message)
        {
            string script = "Alert('" + message.Replace("'", "\\'") + "', {"
                + "title: '" + title.Replace("'", "\\'") + "'"
                + "})";

            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "_serverAlert", script, true);
        }

        private void EnablePaperClipModeless(bool enable)
        {
            MasterPage master = (MasterPage)this.Master;
            if (PLCSession.CheckUserOption("IVVIEWE") && PLCSession.GetLabCtrlFlag("USES_IMAGE_VAULT_WINDOW").Equals("T"))
            {
                master.EnablePaperClipModeless(enable);
            }
        }

        #endregion

        #region Assignment History

        protected void bnHistory_Click(object sender, EventArgs e)
        {
            if (GridView1.Rows.Count == 0)
                return;

            MultiView1.ActiveViewIndex = 7;
            string examKey = PLCSession.PLCGlobalAssignmentKey;
            BindAssignmentHistory(examKey);
        }
        /// <summary>
        /// Initialize Assignment History grid
        /// </summary>
        /// <param name="examKey"></param>
        private void BindAssignmentHistory(string examKey)
        {
            string assignmentHistorySQL = @"SELECT R.CASE_KEY, R.EXAM_KEY, R.STATUS, R.STATUS_DATE, E.DESCRIPTION AS SECTION , R.PRIORITY, SUBSTR(A.NAME, 1, 20) ""ANALYST_ASSIGNED""
                FROM TV_REPTSTAT R
                LEFT OUTER JOIN TV_ANALYST A ON R.ANALYST = A.ANALYST
                LEFT OUTER JOIN TV_EXAMCODE E ON E.EXAM_CODE = R.SECTION
                ORDER BY R.REPT_STAT_KEY";
            dbgAssignmentHistory.PLCSQLString = PLCSession.FormatSpecialFunctions(assignmentHistorySQL);
            dbgAssignmentHistory.PLCSQLString_AdditionalCriteria = "WHERE R.EXAM_KEY = " + examKey;
            dbgAssignmentHistory.PageIndex = 0;
            dbgAssignmentHistory.ClearSortExpression();
            dbgAssignmentHistory.InitializePLCDBGrid();

        }

        /// <summary>
        /// Add REPTSTAT assignment record
        /// </summary>
        private void AddAssignReptStatRecord()
        {
            string caseKey = PLCSession.PLCGlobalCaseKey;
            string examKey = PLCSession.PLCGlobalAssignmentKey;
            string status = PLCDBPanel1.getpanelfield("STATUS");
            string section = PLCDBPanel1.getpanelfield("SECTION");
            string analyst = PLCDBPanel1.getpanelfield("ANALYST_ASSIGNED");
            string statusBy = PLCDBGlobal.instance.GetLabAssignField(examKey, "ASSIGNED_BY");
            string priority = PLCDBPanel1.getpanelfield("PRIORITY");

            if (analyst != PLCDBPanel1.GetOriginalValue("ANALYST_ASSIGNED") ||
                section != PLCDBPanel1.GetOriginalValue("SECTION") ||
                status != PLCDBPanel1.GetOriginalValue("STATUS") ||
                priority != PLCDBPanel1.GetOriginalValue("PRIORITY"))
            {
                PLCDBGlobal.instance.AddAssignReptStatRecord(caseKey, examKey, status, section, analyst, statusBy, priority);
            }
        }

        #endregion
    }
}


