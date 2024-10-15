using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PLCCONTROLS;
using PLCGlobals;
using PLCWebCommon;
using System.IO;
using System.Collections.Specialized;

namespace BEASTiLIMS
{
    public partial class AuditLog : PageBase
    {
        PLCSessionVars PLCSessionVars1 = new PLCSessionVars();
        PLCMsgBox messageBox = new PLCMsgBox();
        PLCCommon DateValidator = new PLCCommon();
        
        private string SearchFilter
        {
            get
            {
                return Convert.ToString(ViewState["Filter"]);
            }
            set
            {
                ViewState["Filter"] = value;
            }
        }

        public bool IsReferenceCleared
        {
            get
            {
                if (Session["IsReferenceCleared"] == null)
                {
                    Session["IsReferenceCleared"] = false;
                    return false;
                }
                else
                {
                    return (bool)Session["IsReferenceCleared"];
                }
            }
            set
            {
                Session["IsReferenceCleared"] = value;
            }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            ScriptManager.GetCurrent(this.Page).Services.Add(new ServiceReference("~/PLCWebCommon/ScriptServiceWebMethods.asmx"));

            BindMessageBox();

            if (!IsPostBack)
            {
                grdLogs.CellPadding = 2;
                grdLogs.RowStyle.BackColor = grdLogs.AlternatingRowStyle.BackColor = ColorTranslator.FromHtml("#F7F6F3");
                grdLogs.RowStyle.ForeColor = grdLogs.AlternatingRowStyle.ForeColor = ColorTranslator.FromHtml("#333333");

                // Do an empty postback that doesn't do anything.
                // Needed as a workaround for the issue where the second date textbox does not have the mask 
                // on initial page load.
                ScriptManager.RegisterStartupScript(this, this.GetType(), "dopostback", "doPostback();", true);

                Menu1.Items[0].Selected = true;

                hdnRowID.Value = "";
                IsReferenceCleared = false;
            }
        }

        private void BindMessageBox()
        {
            //initialize message box
            messageBox = (PLCMsgBox)LoadControl("~/PLCWebCommon/PLCMsgBox.ascx");
            phMessageBox.Controls.Add(messageBox);
        }

        protected void grdLogs_RowCreated(object sender, GridViewRowEventArgs e)
        {
            bool usesDBGrid = UsesAuditlogDBGrid();

            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                string logStamp =  (usesDBGrid ? grdLogs.DataKeys[e.Row.RowIndex].Values["LOG_STAMP"].ToString() : "");

                grdLogs.Columns[4].HeaderText = PLCSessionVars1.GetLabCtrl("DEPT_CASE_TEXT");

                e.Row.Attributes["onmouseover"] = "this.style.cursor='hand'; this.style.backgroundColor='#E2DED6';";
                e.Row.Attributes["onmouseout"] = "this.style.cursor='default'; if (curRow != GetSelectedGridRow('" + grdLogs.ClientID + "', " + (e.Row.RowIndex + 1).ToString() + ")) this.style.backgroundColor='#FFFFFF';";
                e.Row.Attributes.Add("onclick", "ClickGridRow('" + grdLogs.ClientID + "', '" + (e.Row.RowIndex + 1).ToString() + "', '" + logStamp + "')");
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            this.dbpAuditLog.ClearFields();

            grdLogs.EmptyDataText = "";
            if (UsesAuditlogDBGrid())
                grdLogs.PLCSQLString_AdditionalCriteria = "0 = 1";
            else
                grdLogs.PLCSQLString = "SELECT * FROM TV_AUDITLOG WHERE 0 = 1";

            grdLogs.ClearSortExpression();
            grdLogs.InitializePLCDBGrid();
            plhDetails.Visible = false;
            lblURN.Text = "";
            btnPrint.Visible = false;
            btnItem.Enabled = false;
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            string strAdditionalInformation = string.Empty;

            CaseSearch1.ClearAll();

            string startDate = this.dbpAuditLog.getpanelfield("DATEFROM").Trim().Replace("'", "''");
            string endDate = this.dbpAuditLog.getpanelfield("DATETO").Trim().Replace("'", "''");
            string analystID = this.dbpAuditLog.getpanelfield("ANALYST").Trim().Replace("'", "''");
            string aCode = this.dbpAuditLog.getpanelfield("CODE").Trim().Replace("'", "''");
            string labcaseNum = this.dbpAuditLog.getpanelfield("LABCASENUM").Trim().Replace("'", "''");
            string deptCode = this.dbpAuditLog.getpanelfield("DEPARTMENT_CODE").Trim().Replace("'", "''");
            string departmentCaseNumber = this.dbpAuditLog.getpanelfield("DEPARTMENT_CASE_NUMBER").Trim().Replace("'", "''");
            bool isDeletedCasesOnly = (this.dbpAuditLog.getpanelfield("CBDELETECASEONLY") == "T");

            string codeSubCode = dbpAuditLog.GetPanelRecByFieldName("CODE_SUBCODE") != null ? dbpAuditLog.getpanelfield("CODE_SUBCODE") : "";
            string detailMessage = dbpAuditLog.GetPanelRecByFieldName("DETAIL_MESSAGE") != null ? dbpAuditLog.getpanelfield("DETAIL_MESSAGE").Trim() : "";

            strAdditionalInformation = this.dbpAuditLog.getpanelfield("ADDITIONAL_INFORMATION").Trim().Replace("'", "''");

            // check date format
            if (startDate.Length > 0)
            {
                if (!DateValidator.IsDateFormatValid(startDate))
                {
                    //messageBox.ShowMsg("Date From", "<div align='center'>Date Format should be<br /><br />MM/DD/YYYY</div>", 0);
                    messageBox.ShowMsg("Date From", "<div align='center'>Date Format should be<br /><br />" + PLCSession.GetDateFormat().ToUpper() + "</div>", 0);
                    return;
                }
            }
               
            if (endDate.Length > 0)
            {
                if (!DateValidator.IsDateFormatValid(endDate))
                {
                    //messageBox.ShowMsg("Date To", "<div align='center'>Date Format should be<br /><br />MM/DD/YYYY</div>", 0);
                    messageBox.ShowMsg("Date To", "<div align='center'>Date Format should be<br /><br />" + PLCSession.GetDateFormat().ToUpper() + "</div>", 0);
                    return;
                }
                else
                { 
                    //check if end date is older than start date
                    if (startDate.Length > 0 && endDate.Length > 0)
                    {

                        DateTime dtStart = Convert.ToDateTime(PLCSession.DateStringToMDY(startDate));
                        DateTime dtEnd = Convert.ToDateTime(PLCSession.DateStringToMDY(endDate));
                        if (dtStart > dtEnd)
                            messageBox.ShowMsg("Date Range","The 'Date From' cannot be older than 'Date To'.", 1);
                    }
                }
            }
                       
            //check if at least one criteria entered
            if (aCode == "" && startDate == "" && endDate == "" && analystID == "" && string.IsNullOrEmpty(labcaseNum) && !isDeletedCasesOnly &&
                string.IsNullOrEmpty(deptCode) && string.IsNullOrEmpty(departmentCaseNumber) && string.IsNullOrEmpty(codeSubCode) && string.IsNullOrEmpty(detailMessage) && string.IsNullOrEmpty(strAdditionalInformation))
            {
                ClearForm();
                messageBox.ShowMsg("Audit Log", "Please enter at least one search criteria.", 1);         
            }
            else
            {
                string crystalreportFilter = String.Empty;
                string[] strDT;
                //
                string filter = "";
                if (startDate != "")
                {
                    if (dbpAuditLog.GetPanelRecByFieldName("DATEFROM").editmask.ToUpper() == "DD/MM/YYYY") startDate = PLCSession.DateStringToMDY(startDate);

                    filter += " AND L.TIME_STAMP >= CONVERTTODATE('" + startDate + "')";
                    //
                    strDT = startDate.Split('/');
                    crystalreportFilter = "AND {TV_AUDITLOG.TIME_STAMP} >= DateTime(" + strDT[2] + "," + strDT[0] + "," + strDT[1] + ")";
                }
                if (endDate != "")
                {
                    if (dbpAuditLog.GetPanelRecByFieldName("DATETO").editmask.ToUpper() == "DD/MM/YYYY") endDate = PLCSession.DateStringToMDY(endDate);

                    filter += " AND L.TIME_STAMP <= CONVERTTODATE('" + endDate + "', true)";
                    //
                    strDT = endDate.Split('/');

                    // Ex. {FieldName} <= DateTime(2010,1,31,23,59,59)
                    // The 23,59,59 in the ending date range represents the end of day time 23:59:59 needed to make the ending date range work.
                    crystalreportFilter += "AND {TV_AUDITLOG.TIME_STAMP} <= DateTime(" + strDT[2] + "," + strDT[0] + "," + strDT[1] + ",23,59,59" + ")";
                }
                if (aCode != "")
                {
                    String[] aParts = aCode.Split('|');

                    filter += " AND L.CODE = " + aParts[0] + " and L.SUB_CODE = " + aParts[1];
                    //
                    crystalreportFilter += " AND {TV_AUDITLOG.CODE} = '" + aParts[0] + "AND {TV_AUDITLOG.SUB_CODE} = '" + aParts[1];
                }
                if (analystID != "")
                {
                    filter += " AND L.USER_ID = '" + analystID + "'";
                    //
                    crystalreportFilter += " AND {TV_AUDITLOG.USER_ID} = '" + analystID + "'";
                }

                if (isDeletedCasesOnly)
                {
                    filter += " AND T.CODE = 28 AND T.SUB_CODE = 1";
                    crystalreportFilter += " AND {TV_AUDITLOG.CODE} = 28 AND {TV_AUDITLOG.SUB_CODE} = 1";
                }

                if (!string.IsNullOrEmpty(codeSubCode))
                {
                    string code = codeSubCode.Split('-')[0];
                    string subcode = codeSubCode.Split('-')[1];
                    filter += " AND T.CODE = " + code + " AND T.SUB_CODE = " + subcode;
                    crystalreportFilter += " AND {TV_AUDITLOG.CODE} = " + code + " AND {TV_AUDITLOG.SUB_CODE} = " + subcode;
                }

                if (!string.IsNullOrEmpty(detailMessage))
                {
                    filter += " AND T.DETAIL_MESSAGE LIKE '%" + detailMessage + "%'";
                    crystalreportFilter += " AND {TV_AUDITLOG.DETAIL_MESSAGE} LIKE '%" + detailMessage + "%'";
                }

                if (!string.IsNullOrEmpty(strAdditionalInformation))
                {
                    filter = string.Format("{0} AND L.ADDITIONAL_INFORMATION LIKE '%{1}%'", filter, strAdditionalInformation);
                    crystalreportFilter = string.Format("{0} AND {{TV_AUDITLOG.ADDITIONAL_INFORMATION}} LIKE '%{1}%'", crystalreportFilter, strAdditionalInformation);
                }

                CheckLabCaseFilter(labcaseNum, deptCode, departmentCaseNumber, ref filter, ref crystalreportFilter);//NYPD 20101105
               
                ViewState["AUDITLOG_CrystalReportFilter"] = crystalreportFilter.Remove(0, 4);
                
                filter = filter.Remove(0, 4);
                SearchFilter = filter;

                ViewState.Add("AdvanceButtonClick", "F");
                
                GetLogs();
            }
        }

        protected void btnFindCase_Click(object sender, EventArgs e)
        {
            ClearForm();
            CaseSearch1.ClearAll();
            CaseSearch1.Show();
            ViewState.Add("AdvanceButtonClick", "T");
            IsReferenceCleared = false;
        }

		protected void CaseSearch1_SelectedCaseKeyChanged(object sender, EventArgs e)
        {
            ClearForm();
            if (!IsReferenceCleared)
            {
                ViewState["AUDITLOG_CrystalReportFilter"] = "{TV_LABCASE.CASE_KEY} = " + CaseSearch1.SelectedCaseKey;
                GetLogs(true);
                CaseSearch1.ClearAll();

                if (CaseSearch1.SelectedCaseKey == "")
                    IsReferenceCleared = true;
            }    


            
        }

        private void GetLogs(bool fromCaseSearch = false)
        {
            btnPrint.Visible = false;
            btnItem.Enabled = false;

            string sql = "";
			if (CaseSearch1.SelectedCaseKey != "" && fromCaseSearch)
            {
                sql = @"
SELECT L.LOG_STAMP, A.NAME AS USERNAME, FORMATDATETIME(L.TIME_STAMP) AS TIME_STAMP, L.PROGRAM, 
CASE WHEN (L.DEPARTMENT_CASE_NUMBER IS NOT NULL OR L.DEPARTMENT_CASE_NUMBER <> '') THEN L.DEPARTMENT_CASE_NUMBER ELSE C.DEPARTMENT_CASE_NUMBER END AS DEPARTMENT_CASE_NUMBER, 
T.MESSAGE, L.OS_COMPUTER_NAME, L.OS_USER_NAME AS OS_USER_NAME, L.EVIDENCE_CONTROL_NUMBER, 
  D.DEPARTMENT_NAME, L.BUILD_NUMBER,  L.CODE, L.SUB_CODE
FROM TV_AUDITLOG L
LEFT OUTER JOIN TV_ANALYST A ON L.USER_ID = A.ANALYST
LEFT OUTER JOIN TV_LABCASE C ON L.CASE_KEY = C.CASE_KEY
LEFT OUTER JOIN TV_AUDITTXT T ON L.CODE = T.CODE AND L.SUB_CODE = T.SUB_CODE
LEFT OUTER JOIN TV_DEPTNAME D ON C.DEPARTMENT_CODE = D.DEPARTMENT_CODE
WHERE L.CASE_KEY = " + CaseSearch1.SelectedCaseKey + @" ORDER BY L.TIME_STAMP DESC"; 
            }
            else if (SearchFilter != "")
            {
                sql = @"
SELECT L.LOG_STAMP, A.NAME AS USERNAME, FORMATDATETIME(L.TIME_STAMP) AS TIME_STAMP, L.PROGRAM, 
CASE WHEN (L.DEPARTMENT_CASE_NUMBER IS NOT NULL OR L.DEPARTMENT_CASE_NUMBER <> '') THEN L.DEPARTMENT_CASE_NUMBER ELSE C.DEPARTMENT_CASE_NUMBER END AS DEPARTMENT_CASE_NUMBER,  
T.MESSAGE, L.OS_COMPUTER_NAME, L.OS_USER_NAME AS OS_USER_NAME, L.EVIDENCE_CONTROL_NUMBER, 
  D.DEPARTMENT_NAME, L.BUILD_NUMBER, L.CODE, L.SUB_CODE
FROM TV_AUDITLOG L
LEFT OUTER JOIN TV_ANALYST A ON L.USER_ID = A.ANALYST
LEFT OUTER JOIN TV_LABCASE C ON L.CASE_KEY = C.CASE_KEY
LEFT OUTER JOIN TV_AUDITTXT T ON L.CODE = T.CODE AND L.SUB_CODE = T.SUB_CODE
LEFT OUTER JOIN TV_DEPTNAME D ON C.DEPARTMENT_CODE = D.DEPARTMENT_CODE
WHERE " + SearchFilter + @" ORDER BY L.TIME_STAMP DESC";
            }

            if (PLCSession.GetLabCtrl("USES_AUDITLOG_SCROLLBAR") == "T")
            {
                grdLogs.AllowPaging = false;
                pnlGridLogs.Height = Unit.Pixel(250);
                pnlGridLogs.ScrollBars = ScrollBars.Vertical;
            }

            grdLogs.EmptyDataText = "No logs found.";

            string filter = ((CaseSearch1.SelectedCaseKey != "" && fromCaseSearch) ? " L.CASE_KEY = " + CaseSearch1.SelectedCaseKey : SearchFilter);

            if (UsesAuditlogDBGrid() && !string.IsNullOrEmpty(filter)) //this is if the page uses dbgrid
            {
                if (!CheckDBGridOrderBy())
                {
                    string strOrderClause = " ORDER BY L.TIME_STAMP DESC";
                    grdLogs.PLCSQLString_AdditionalCriteria = PLCSessionVars1.FormatSpecialFunctions(filter + strOrderClause);
                }
                else
                {
                    grdLogs.PLCSQLString_AdditionalCriteria = PLCSessionVars1.FormatSpecialFunctions(filter);
                }
            }

            grdLogs.PLCSQLString = PLCSessionVars1.FormatSpecialFunctions(sql);
            grdLogs.InitializePLCDBGrid();
            
            plhDetails.Visible = grdLogs.Rows.Count > 0;

            string urn = "";
            if (grdLogs.Rows.Count > 0)
            {
                grdLogs.SelectedIndex = 0;

                string logStamp = UsesAuditlogDBGrid() ? grdLogs.DataKeys[0].Values["LOG_STAMP"].ToString() : "";
              
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "clickrow", "setTimeout(function() { ClickGridRow('" + grdLogs.ClientID + "', '1', '" + logStamp + "'); }, 50);", true);
				if (CaseSearch1.SelectedCaseKey != "")
                    urn = Convert.ToString(grdLogs.DataKeys[0].Values["DEPARTMENT_CASE_NUMBER"]);

                if ((string)ViewState["AdvanceButtonClick"] != "T")
                {
                    btnItem.Enabled = false;

                    string sCurrECN =
                        Convert.ToString(grdLogs.DataKeys[0].Values["EVIDENCE_CONTROL_NUMBER"]);

                    btnItem.Enabled = IsItemReportVisible(sCurrECN);
                }

                btnPrint.Visible = true;
            }
			else if (CaseSearch1.SelectedCaseKey != "")
            {
                PLCQuery qryCase = new PLCQuery();
				qryCase.SQL = "SELECT DEPARTMENT_CASE_NUMBER FROM TV_LABCASE WHERE CASE_KEY = " + CaseSearch1.SelectedCaseKey;
                qryCase.Open();
                if (!qryCase.IsEmpty())
                {
                    urn = qryCase.FieldByName("DEPARTMENT_CASE_NUMBER");
                }
            }

            lblURN.Text = (urn == "" ? "" : ("for " + (string.IsNullOrEmpty(PLCSessionVars1.GetLabCtrl("DEPT_CASE_TEXT")) ? "" : (PLCSessionVars1.GetLabCtrl("DEPT_CASE_TEXT") + " ")) + urn));
        }

        protected void btnPrint_Click(object sender, EventArgs e)
        {
            

            int index = PLCSession.SafeInt(hdnRowID.Value, -1);
            if (index != -1)
            {
                string logStamp = UsesAuditlogDBGrid() ? grdLogs.DataKeys[index].Values["LOG_STAMP"].ToString() : "";
                ClickGridRow(logStamp, index + 1, "T");
            }

            grdLogs.SelectedIndex = index;

            PLCSessionVars1.PLCCrystalReportName = PLCSession.FindCrystalReport("auditlog.rpt");
            PLCSessionVars1.PLCCrystalSelectionFormula = (String)ViewState["AUDITLOG_CrystalReportFilter"];
            PLCSessionVars1.PrintCRWReport(false);

        }

        protected void grdLogs_SelectedIndexChanged(object sender, EventArgs e)
        {
            string sCurrECN = Convert.ToString(grdLogs.DataKeys[grdLogs.SelectedIndex].Values["EVIDENCE_CONTROL_NUMBER"]);
            btnItem.Enabled = IsItemReportVisible(sCurrECN);
            
            string logStamp = UsesAuditlogDBGrid() ? grdLogs.SelectedDataKey["LOG_STAMP"].ToString() : "";
            ClickGridRow(logStamp, (grdLogs.SelectedIndex + 1));
        }

        protected void btnItem_Click(object sender, EventArgs e)
        {
            int index = PLCSession.SafeInt(hdnRowID.Value, -1);
            if (index != -1)
            {
                string logStamp = UsesAuditlogDBGrid() ? grdLogs.DataKeys[index].Values["LOG_STAMP"].ToString() : "";
                ClickGridRow(logStamp, index + 1, "T");
            }

            grdLogs.SelectedIndex = index;

            string ecn = this.hdnSelECN.Value.Trim();
            if (ecn == "")
                return;

            PLCSessionVars1.PLCCrystalReportName = PLCSession.FindCrystalReport("AUDITITM.rpt");
            string SelectionFormula = "{TV_AUDITLOG.EVIDENCE_CONTROL_NUMBER} = " + ecn;
            PLCSessionVars1.PLCCrystalSelectionFormula = SelectionFormula;
            PLCSessionVars1.PLCCrystalReportTitle = "Audit Log Report";
            PLCSessionVars1.PrintCRWReport(false);

        }

        protected bool IsItemReportVisible(string sECN)
        {
            bool isValid = false;

            if (!string.IsNullOrEmpty(sECN))
            {
                if (Convert.ToInt32(sECN) > 0)
                {
                    ViewState["AUDITLLOG_ECN"] = sECN;
                    isValid = true;
                }
            }

            return isValid;
        }

        #region NYPD 20101105

        private void CheckLabCaseFilter(string labcaseNum, string deptCode, string departmentCaseNumber, ref string SearchFilter, ref string ReportFilter)
        {
            if (!String.IsNullOrEmpty(labcaseNum.Trim()))
            {
                SearchFilter += " AND C.LAB_CASE ='" + labcaseNum + "'";
                ReportFilter += " AND {TV_LABCASE.LAB_CASE}='" + labcaseNum + "'";
            }

            if (!String.IsNullOrEmpty(deptCode.Trim()))
            {
                SearchFilter += " AND C.DEPARTMENT_CODE ='" + deptCode + "'";
                ReportFilter += " AND {TV_LABCASE.DEPARTMENT_CODE}='" + deptCode + "'";
            }

            if (!String.IsNullOrEmpty(departmentCaseNumber.Trim()))
            {
                SearchFilter += " AND C.DEPARTMENT_CASE_NUMBER ='" + departmentCaseNumber + "'";
                ReportFilter += " AND {TV_LABCASE.DEPARTMENT_CASE_NUMBER}='" + departmentCaseNumber + "'";
            }
        }

        #endregion //NYPD 20101105

        protected void btnDoPostback_Click(object sender, EventArgs e)
        {
        }


        protected void Menu1_MenuItemClick(object sender, MenuEventArgs e)
        {
            int selectedMenuIndex;
            int.TryParse(e.Item.Value, out selectedMenuIndex);

            Menu1.Items[selectedMenuIndex].Selected = true;

            ClearForm();
            ClearAuditWebForm();

            switch (selectedMenuIndex)
            {
                case 1: // AuditWeb
                    MultiView1.ActiveViewIndex = 1;
                    break;
                default:
                    MultiView1.ActiveViewIndex = selectedMenuIndex;
                    break;
            }
        }


        protected void grdLogs_Sorted(object sender, EventArgs e)
        {

            if (grdLogs.SelectedIndex != 0)
            {
                grdLogs.SelectedIndex = 0;
                grdLogs.SetClientSideScrollToSelectedRow();
            }

            string logStamp = UsesAuditlogDBGrid() ? grdLogs.SelectedDataKey["LOG_STAMP"].ToString() : "";
            ClickGridRow(logStamp, (grdLogs.SelectedIndex + 1));

        }


        protected void grdLogs_PageIndexChanged(object sender, EventArgs e)
        {
            if (grdLogs.SelectedIndex >= grdLogs.Rows.Count)
                grdLogs.SelectedIndex = grdLogs.Rows.Count - 1;

            grdLogs.SetClientSideScrollToSelectedRow();
            string logStamp = UsesAuditlogDBGrid() ? grdLogs.SelectedDataKey["LOG_STAMP"].ToString() : "";
            ClickGridRow(logStamp, (grdLogs.SelectedIndex + 1));
        }


        private bool UsesAuditlogDBGrid()
        {
            PLCQuery qryDBGrid = new PLCQuery();
            qryDBGrid.SQL = "SELECT * FROM TV_DBGRIDHD WHERE GRID_NAME = ?";
            qryDBGrid.AddSQLParameter("GRID_NAME", grdLogs.PLCGridName);
            qryDBGrid.Open();
            return qryDBGrid.HasData();
        }

        /// <summary>
        /// Checks if the DBGrid SQL has an order by clause
        /// </summary>
        /// <returns>Returns true if an order by clause is found</returns>
        private bool CheckDBGridOrderBy()
        {
            PLCQuery objQuery = new PLCQuery();
            objQuery.SQL = "SELECT * FROM TV_DBGRIDHD WHERE GRID_NAME = ?";
            objQuery.AddSQLParameter("GRID_NAME", grdLogs.PLCGridName);
            objQuery.Open();
            if (objQuery.HasData())
            {
                return objQuery.FieldByName("SQL_STRING").ToUpper().Contains("ORDER BY");
            }

            return false;

        }


        private void ClickGridRow(string logStamp, int index, string hasRowID = "")
        {         
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "clickrow", "ClickGridRow('" + grdLogs.ClientID + "', '" + index + "', '" + logStamp + "', '" + hasRowID + "')", true);
        }

        #region AuditWeb

        protected void grdAuditWebLogs_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulatePanelFields();
        }

        protected void grdAuditWebLogs_Sorted(object sender, EventArgs e)
        {
            if (grdAuditWebLogs.SelectedIndex != 0)
            {
                grdAuditWebLogs.SelectedIndex = 0;
                grdAuditWebLogs.SetClientSideScrollToSelectedRow();
            }

            PopulatePanelFields();
        }

        protected void grdAuditWebLogs_PageIndexChanged(object sender, EventArgs e)
        {
            if (grdAuditWebLogs.SelectedIndex >= grdAuditWebLogs.Rows.Count)
                grdAuditWebLogs.SelectedIndex = grdAuditWebLogs.Rows.Count - 1;

            grdAuditWebLogs.SetClientSideScrollToSelectedRow();
            PopulatePanelFields();
        }


        protected void btnClearAuditWeb_Click(object sender, EventArgs e)
        {
            ClearAuditWebForm();
        }

        protected void btnSearchAuditWeb_Click(object sender, EventArgs e)
        {

            grdAuditWebLogs.PageIndex = 0;
            grdAuditWebLogs.ClearSortExpression();

            string startDate = this.dbpAuditWeb.getpanelfield("DATEFROM").Trim().Replace("'", "''");
            string endDate = this.dbpAuditWeb.getpanelfield("DATETO").Trim().Replace("'", "''");

            //// check date format
            if (startDate.Length > 0)
            {
                if (!DateValidator.IsDateFormatValid(startDate))
                {
                    dlgMessage.ShowAlert("Date From", "<div align='center'>Date Format should be<br /><br />" + PLCSession.GetDateFormat().ToUpper() + "</div>");
                    return;
                }
            }

            if (endDate.Length > 0)
            {
                if (!DateValidator.IsDateFormatValid(endDate))
                {

                    dlgMessage.ShowAlert("Date To", "<div align='center'>Date Format should be<br /><br />" + PLCSession.GetDateFormat().ToUpper() + "</div>");
                    return;
                }
                else
                {
                    //check if end date is older than start date
                    if (startDate.Length > 0 && endDate.Length > 0)
                    {

                        DateTime dtStart = Convert.ToDateTime(PLCSession.DateStringToMDY(startDate));
                        DateTime dtEnd = Convert.ToDateTime(PLCSession.DateStringToMDY(endDate));
                        if (dtStart > dtEnd)
                        {
                            dlgMessage.ShowAlert("Date Range", "The \"Date From\" cannot be older than \"Date To\".");
                            return;
                        }
                    }
                }
            }


            bool dbPanelHasValues = false;
            bool hasEmptyRequiredField = false;
            string searchClause = GenerateAuditDBPanelSQL(out dbPanelHasValues, out hasEmptyRequiredField);


            if (dbPanelHasValues && !hasEmptyRequiredField)
            {
                GetAuditWebLogs(searchClause);
            }
            else
            {
                string message = hasEmptyRequiredField ? "Please specify required search parameter." : "Please enter at least one search criteria.";
                ClearAuditWebForm();
                dlgMessage.ShowAlert("Audit Log", message);
            }          

        }


        protected void btnPrintAuditWeb_Click(object sender, EventArgs e)
        {
            string reportPath = PLCSession.FindCrystalReport("AuditWeb.rpt");
            if (!string.IsNullOrEmpty(reportPath))
            {
                PLCSession.PLCCrystalReportName = reportPath;

                PLCSession.PLCCrystalSelectionFormula = (String)ViewState["AUDITWEB_CrystalReportFilter"];
                PLCSession.PLCCrystalReportTitle = "PRELOG AUDIT WEB";
                PLCSession.PrintCRWReport(true);
            }
            else
                dlgMessage.ShowAlert("Print Logs", "No report found.");

        }

        /// <summary>
        /// Clears the search screen on click of "Clear" button
        /// </summary>
        private void ClearAuditWebForm()
        {
            dbpAuditWeb.ClearFields();
            dbpAuditWebLogs.ClearFields();
            grdAuditWebLogs.EmptyDataText = "";
            grdAuditWebLogs.ClearSortExpression();
            grdAuditWebLogs.DataBind();
            plhDetailsWeb.Visible = false;
            btnPrintAuditWeb.Visible = false;
        }

        /// <summary>
        /// Gets the audit web logs and displays it in the grid
        /// </summary>
        /// <param name="searchClause"></param>
        private void GetAuditWebLogs(string searchClause)
        {
            btnPrintAuditWeb.Visible = false;

            string sql = @"
            SELECT W.LOG_STAMP, U.USER_NAME AS USERNAME, FORMATDATE(W.TIME_STAMP) AS TIME_STAMP, W.PROGRAM, 
            W.DEPARTMENT_CASE_NUMBER, T.MESSAGE, W.OS_COMPUTER_NAME, W.OS_USER_NAME AS OS_USER_NAME, W.EVIDENCE_CONTROL_NUMBER, W.CASE_KEY,
            D.DEPARTMENT_NAME, W.CODE, W.SUB_CODE, W.DEPARTMENT_CODE, W.SUBMISSION_NUMBER, W.ADDITIONAL_INFORMATION, T.DETAIL_MESSAGE, C.LAB_CASE 
            FROM TV_AUDITWEB W
            LEFT OUTER JOIN TV_WEBUSER U ON W.USER_ID = U.USER_ID
            LEFT OUTER JOIN TV_AUDITTXT T ON W.CODE = T.CODE AND W.SUB_CODE = T.SUB_CODE
            LEFT OUTER JOIN TV_LABCASE C ON W.CASE_KEY = C.CASE_KEY
            LEFT OUTER JOIN TV_DEPTNAME D ON W.DEPARTMENT_CODE = D.DEPARTMENT_CODE ORDER BY LOG_STAMP DESC";


            grdAuditWebLogs.EmptyDataText = "No logs found.";

            grdAuditWebLogs.PLCSQLString = PLCSessionVars1.FormatSpecialFunctions(sql);
            grdAuditWebLogs.PLCSQLString_AdditionalCriteria = searchClause;
            grdAuditWebLogs.InitializePLCDBGrid();

            plhDetailsWeb.Visible = grdAuditWebLogs.Rows.Count > 0;
            if (grdAuditWebLogs.Rows.Count > 0)
            {
                btnPrintAuditWeb.Visible = true;

                if (grdAuditWebLogs.SelectedIndex != 0)
                    grdAuditWebLogs.SelectedIndex = 0;

                PopulatePanelFields();
            }

        }

        /// <summary>
        /// Populates the DBPANEL fields on select of row in the grid
        /// </summary>
        private void PopulatePanelFields()
        {
            dbpAuditWebLogs.PLCWhereClause = " WHERE LOG_STAMP = " + grdAuditWebLogs.SelectedDataKey["LOG_STAMP"].ToString();
            dbpAuditWebLogs.DoLoadRecord();
        }

        /// <summary>
        /// Generates the criteria SQL string based on selected values in the search panel
        /// </summary>
        /// <param name="hasValues"></param>
        /// <returns></returns>
        private string GenerateAuditDBPanelSQL(out bool hasValues, out bool hasEmptyRequiredField)
        {
            hasValues = false;
            hasEmptyRequiredField = false;
            string[] fieldNames = dbpAuditWeb.GetPanelFieldNames();
            string whereClause = "";
            string crystalreportFilter = String.Empty;
            string[] strDT;

            PLCHelperFunctions HelperFunctions = new PLCHelperFunctions();
            Dictionary<string, OrderedDictionary> tables = HelperFunctions.GetTablesAndAlias(grdAuditWebLogs.PLCGridName);

            if (IsUsesDefault() && tables.Count == 0)
            {
                tables.Add("TV_AUDITWEB", new OrderedDictionary { { String.Empty, "W" } });
                tables.Add("TV_WEBUSER", new OrderedDictionary { { String.Empty, "U" } });
                tables.Add("TV_AUDITTXT", new OrderedDictionary { { String.Empty, "T" } });
                tables.Add("TV_LABCASE", new OrderedDictionary { { String.Empty, "C" } });
            }

            for (int fieldIndex = 0; fieldIndex < fieldNames.Length; fieldIndex++)
            {
                string fieldName = fieldNames[fieldIndex];
                string fieldLabel = dbpAuditWeb.GetFieldPrompt(fieldName);
                string tableName = dbpAuditWeb.GetFieldTableName(fieldName);
                string value = dbpAuditWeb.GetFieldValue(tableName, fieldName, fieldLabel).Replace("'", "''");
                string editmask = dbpAuditWeb.GetPanelRecByFieldName(fieldName).editmask;
                string sqlOperator = " = ";
                string dateReportFilter = "";
                bool isDate = false;

                bool isRequired = (dbpAuditWeb.GetPanelRecByFieldName(fieldName).required == "T");

                if (!string.IsNullOrWhiteSpace(value))
                {
                    string sqlValue = "'" + value.ToUpper() + "' ";
                    if (fieldName.Contains("DATE"))
                    {
                        isDate = true;
                        if (editmask.ToUpper() == "DD/MM/YYYY")
                            value = PLCSession.DateStringToMDY(value);

                        if (fieldLabel.ToLower().Contains("from") || fieldLabel.ToLower().Contains("start"))
                        {

                            sqlOperator = " >= ";
                            sqlValue = " CONVERTTODATE(" + sqlValue + ") ";
                            strDT = value.Split('/');
                            dateReportFilter = " AND {TV_AUDITWEB.TIME_STAMP} >= DateTime(" + strDT[2] + "," + strDT[0] + "," + strDT[1] + ")";

                        }
                        else if (fieldLabel.ToLower().Contains("to") || fieldLabel.ToLower().Contains("end"))
                        {
                            sqlOperator = " <= ";
                            sqlValue = " CONVERTTODATE(" + sqlValue + ", true) ";
                            strDT = value.Split('/');
                            dateReportFilter += " AND {TV_AUDITWEB.TIME_STAMP} <= DateTime(" + strDT[2] + "," + strDT[0] + "," + strDT[1] + ",23,59,59" + ")";
                        }

                        fieldName = "TIME_STAMP";

                    }

                    string alias = tableName;
                    if (tables.ContainsKey(tableName))
                        alias = tables[tableName][0].ToString();

                    string field = (isDate && PLCSession.PLCDatabaseServer == "MSSQL") ? "CONVERTTODATE(" + alias + "." + fieldName + ")" : "UPPER(" + alias + "." + fieldName + ")";

                    crystalreportFilter += string.IsNullOrEmpty(dateReportFilter) ? " AND {" + tableName + "." + fieldName + "} = " + sqlValue : dateReportFilter;
                    whereClause += string.Format(" AND {0}{1}{2}", field, sqlOperator, sqlValue);
                    hasValues = true;

                }
                else
                {
                    if (isRequired)
                    {
                        hasEmptyRequiredField = true;
                        return "";
                    }
                    
                }
            }


            ViewState["AUDITWEB_CrystalReportFilter"] = !string.IsNullOrEmpty(crystalreportFilter) ? crystalreportFilter.Remove(0, 4) : crystalreportFilter;

            if (!string.IsNullOrEmpty(whereClause))
                whereClause = whereClause.Remove(0, 4);
            whereClause = PLCSessionVars1.FormatSpecialFunctions(whereClause);

            return whereClause;
        }

        private bool IsUsesDefault()
        {
            PLCQuery qryCheck = CacheHelper.OpenCachedSqlReadOnly("SELECT HD.GRID_NAME AS \"HDGRID\", DL.GRID_NAME AS \"DLGRID\" FROM TV_DBGRIDHD HD LEFT OUTER JOIN " +
                          " TV_DBGRIDDL DL ON HD.GRID_NAME = DL.GRID_NAME WHERE HD.GRID_NAME = '" + grdAuditWebLogs.PLCGridName + "'");
            return qryCheck.IsEmpty();
        }

        #endregion



     

    }
}
