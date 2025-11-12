using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml.Linq;
using PLCCONTROLS;
using PLCWebCommon;
using PLCGlobals;
using System.IO;
using AjaxControlToolkit;
using System.Text.RegularExpressions;

namespace BEASTiLIMS
{
    public partial class ConfigTAB6GroupReports : PLCGlobals.PageBase
    {
        #region Properties and Declaration

        private static List<string> notnullColumn
        {
            get;
            set;
        }
        private static bool editMode
        {
            get;
            set;
        }
        private static string originalValue
        {
            get;
            set;
        }
        private static string custody
        {
            get;
            set;
        }
        private string ThisGroup = "";

        PLCMsgBox mbox = new PLCMsgBox();
        PLCMsgComment mInput = new PLCMsgComment();
        CodeHead chCustody = new CodeHead();

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            ThisGroup = PLCSessionVars1.GetDefault("EDIT-CUSTGRP");

            Control CtrlMsgBox = LoadControl("~/PLCWebCommon/PLCMsgBox.ascx");
            mbox = (PLCMsgBox)CtrlMsgBox;
            phMsgBox.Controls.Add(mbox);

            Control CtrlmInput = LoadControl("~/PLCWebCommon/PLCMsgComment.ascx");
            mInput = (PLCMsgComment)CtrlmInput;
            phMsgBoxComments.Controls.Add(mInput);

            Page.Form.Attributes.Add("enctype", "multipart/form-data");

            ConfigMasterPage myPage = (ConfigMasterPage)Master;
            myPage.RegisterPostbackTrigger(btnUploadReport);

            if (!Page.IsPostBack)
            {
                custody = "";

                GridView1.PLCSQLString = "SELECT CUST_REPORT_KEY, CUST_REPORT_ORDER \"ORDER\", REPORT_DESCRIPTION DESCRIPTION, REPORT_FORMAT RPT_FILE FROM TV_CUSTREPORTS";
                GridView1.PLCSQLString_AdditionalCriteria = "WHERE CUST_REPORT_GROUP = '" + ThisGroup + "' order by CUST_REPORT_ORDER";
                GridView1.InitializePLCDBGrid();

                if (GridView1.Rows.Count > 0)
                {
                    if (Request.QueryString["d"] != null)
                        GridView1.SelectRowByDataKey(Request.QueryString["d"].ToString());

                    if (GridView1.SelectedIndex < 0)
                        GridView1.SelectedIndex = 0;

                    PLCButtonPanel1.SetBrowseMode();
                    PLCButtonPanel1.SetCustomButtonMode("", true);

                    GrabGridRecord(GridView1.SelectedDataKey[0].ToString());
                }
                else
                {
                    PLCDBPanel1.EmptyMode();
                    PLCButtonPanel1.SetEmptyMode();
                    PLCButtonPanel1.SetCustomButtonMode("Back to Groups", true);
                }
            }
            else
                GridView1.SetPLCGridProperties();

            if (GridView1.SelectedIndex < 0 && GridView1.Rows.Count > 0)
            {
                GridView1.SelectedIndex = 0;

                PLCButtonPanel1.SetBrowseMode();
                PLCButtonPanel1.SetCustomButtonMode("", true);

                GrabGridRecord(GridView1.SelectedDataKey[0].ToString());
            }

            Menu mymenu = (Menu)Master.FindControl("menu_main");
            mymenu.Items[5].Selected = true;
        }

        protected void MsgCommentPostBackButton_Click(object sender, EventArgs e)
        {

        }

        protected void PLCDBPanel1_SetDefaultRecord(object sender, PLCDBPanelSetDefaultRecordEventArgs e)
        {
            string defaultCustomGroup = PLCSession.GetDefault("EDIT-CUSTGRP");
            e.SetNewRecordValues("CUST_REPORT_GROUP", defaultCustomGroup);
            ViewState["CUST_REPORT_KEY"] = "0";
        }

        protected void PLCDBPanel1_PLCDBPanelGetNewRecord(object sender, PLCDBPanelGetNewRecordEventArgs e)
        {
            string custReportKey = PLCSession.GetNextSequence("CUSTREPORT_SEQ").ToString();
            e.SetNewRecordValues("CUST_REPORT_KEY", custReportKey);
            e.newWhereClause = " where CUST_REPORT_KEY = '" + custReportKey + "'";
            ViewState["CUST_REPORT_KEY"] = custReportKey;
            PLCSession.SetConfigFileSourceKeys(custReportKey, "");
        }

        protected void PLCButtonPanel1_PLCButtonClick(object sender, PLCCONTROLS.PLCButtonClickEventArgs e)
        {
            if ((e.button_name == "ADD") & (e.button_action == "AFTER"))
            {
                GridView1.SetControlMode(false);
                EnableDownloadButton(false);
            }

            if ((e.button_name == "EDIT") & (e.button_action == "AFTER"))
            {
                GridView1.SetControlMode(false);
                EnableDownloadButton(false);
            }

            if ((e.button_name == "SAVE") & (e.button_action == "AFTER"))
            {
                string custReportKey = "";
                if (e.row_added)
                    custReportKey = ViewState["CUST_REPORT_KEY"].ToString();
                else
                    custReportKey = GridView1.SelectedDataKey.Value.ToString();

                Response.Redirect("~/ConfigTab6GroupReports.aspx?d=" + custReportKey);
            }

            if ((e.button_name == "CANCEL") & (e.button_action == "AFTER"))
            {
                GridView1.InitializePLCDBGrid();
                if (GridView1.Rows.Count > 0)
                {
                    GridView1.SelectedIndex = 0;
                    GrabGridRecord(GridView1.SelectedDataKey[0].ToString());
                }
                else
                {
                    PLCDBPanel1.EmptyMode();
                    PLCButtonPanel1.SetEmptyMode();
                    PLCButtonPanel1.SetCustomButtonMode("Back to Groups", true);
                }

                GridView1.SetControlMode(true);
                PLCDBPanel1.SetBrowseMode();
                editMode = false;

                ShouldEnableDownload(GridView1.SelectedDataKey.Value.ToString());
            }

            if ((e.button_name == "DELETE") & (e.button_action == "AFTER"))
            {
                GridView1.InitializePLCDBGrid();
                if (GridView1.Rows.Count > 0)
                {
                    GridView1.SelectedIndex = 0;
                    GrabGridRecord(GridView1.SelectedDataKey[0].ToString());
                }
                else
                {
                    PLCDBPanel1.EmptyMode();
                    PLCButtonPanel1.SetEmptyMode();
                    PLCButtonPanel1.SetCustomButtonMode("Back to Groups", true);
                }
            }

            if (e.button_action == "CUSTOM")
            {
                string btnName = e.button_name;
                if (btnName == "Upload Report") 
                {
                    //Response.Redirect("~/ConfigTab6ReportUpload.aspx?d=" + GridView1.SelectedDataKey.Value.ToString());
                    InitUploadReportUI();
                }
                else if (btnName == "Run Report")
                {
                    // run the report
                    string custReportKey = GridView1.SelectedDataKey.Value.ToString();
                    RunReport(custReportKey);
                    mvReports.ActiveViewIndex = 1;
                    TriggerMasterUpdatePanel();
                }
                else if (btnName == "Download Report")
                {
                    string custReportKey = GridView1.SelectedDataKey.Value.ToString();
                    DownloadReport(custReportKey);
                }
                else if (btnName == "Back to Groups")
                {
                    Response.Redirect("~/ConfigTab6ReportGroups.aspx");
                }
            }
        }

        protected void GridView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            GrabGridRecord(GridView1.SelectedDataKey[0].ToString());
        }

        protected void GridView1_PageIndexChanged(object sender, EventArgs e)
        {
            GridView1.SelectedIndex = 0;
            GrabGridRecord(GridView1.SelectedDataKey[0].ToString());
        }

        protected void GridView1_Sorted(object sender, EventArgs e)
        {
            GridView1.SelectedIndex = 0;
            GrabGridRecord(GridView1.SelectedDataKey[0].ToString());
        }

        protected void btnUploadReport_Click(object sender, EventArgs e)
        {
            if (fuUploadReport.HasFile)
            {
                string custRptKey = GridView1.SelectedDataKey.Value.ToString();
                bool uploadOk = SaveReportFileToDB(fuUploadReport, custRptKey);

                string message = "Upload success!";
                string type = "info";

                if (!uploadOk)
                {
                    message = "Something went wrong while uploading your file...";
                    type = "error";
                }
                else
                {
                    EnableDownloadButton(true);
                    GridView1.InitializePLCDBGrid();
                }

                ShowAlert(message, "", type);
            }
        }


        protected void btnViewReport_Click(object sender, EventArgs e)
        {
            string custReportKey = GridView1.SelectedDataKey.Value.ToString();
            var record = GetCustReportRecord(custReportKey);
            string reportFile = record["REPORT_FORMAT"];
            ViewReportUsingAdhocIni(custReportKey, reportFile);

            // RebindParameters for CodeMultipick
            RebindParameters();
        }
        
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            mvReports.ActiveViewIndex = 0;
        }

        #endregion

        #region Methods

        private void GrabGridRecord(string rkey)
        {
            PLCDBPanel1.PLCWhereClause = " where CUST_REPORT_KEY = " + rkey;
            PLCDBPanel1.DoLoadRecord();

            PLCSession.SetConfigFileSourceKeys(rkey, "");

            TriggerAfterClickEvents();

            //ShouldEnableDownload(rkey);
        }

        /// <summary>
        /// show log history dialog, don't use single quotes
        /// </summary>
        private void ShowUploadDialog(string dialogTitle)
        {
            string script = string.Format("$(function(){{__showUploadDialog('{0}');}});", dialogTitle);
            ScriptManager.RegisterStartupScript(
                this,
                this.GetType(),
                "____SHOW_UPLOAD_REPORT_DIALOG",
                script,
                true
            );
        }

        /// <summary>
        /// Show alert dialog
        /// don't use single quotes
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="type">type of alert (info, error)</param>
        private void ShowAlert(string message, string title, string type = "info")
        {
            string script = string.Format("$(function(){{__showAlert('{0}', '{1}', '{2}');}});", message, title, type);
            ScriptManager.RegisterStartupScript(
                this,
                this.GetType(),
                "____SHOW_ALERT_DIALOG",
                script,
                true
            );
        }

        /// <summary>
        /// Initialize upload report dialog
        /// </summary>
        private void InitUploadReportUI()
        {
            string custReportKey = GridView1.SelectedDataKey.Value.ToString();
            bool reportInDb = PLCDBGlobal.instance.IsCustReportFileInDatabase(custReportKey);
            
            // init dialog title
            string dialogTitle = "Upload Report File to Database";
            if (reportInDb)
                dialogTitle = "Update Report File";

            ShowUploadDialog(dialogTitle);

            TriggerMasterUpdatePanel();
        }

        /// <summary>
        /// Helper method to Update or Save a 
        /// report file
        /// </summary>
        /// <param name="fuControl"></param>
        /// <param name="custReportKey"></param>
        private bool SaveReportFileToDB(FileUpload fuControl, string custReportKey)
        {
            string fileName = Guid.NewGuid().ToString() + ".rpt";
            string filePath = PLCPath.Temp + fileName;

            fuControl.SaveAs(filePath);

            string checksum = PLCHelperFunctions.GetFileChecksum(filePath);

            byte[] blob;
            int fileSize = PLCGlobals.PLCCommon.instance.ConvertFileToBlob(filePath, out blob, true);

            var qry = new PLCQuery();
            string whereClause = string.Format("WHERE CUST_REPORT_KEY={0}", custReportKey);

            // get cust report record before deleting
            Dictionary<string, string> custReportRecord = GetCustReportRecord(custReportKey);


            // temporary delete the record if it exists
            qry.Delete("TV_CUSTREPORTS", whereClause);

            // re-insert record deleted record
            qry.SQL = "SELECT * FROM TV_CUSTREPORTS " + whereClause;
            qry.Open();
            qry.Append();

            var skippedFields = new List<string> { 
                "REPORT_DATA", 
                "REPORT_HASH", 
                "UPLOADED_BY", 
                "DATE_UPLOADED" 
            };

            foreach (var fieldName in custReportRecord.Keys)
            {
                if (skippedFields.Any(field => fieldName.Contains(field))) continue;
                qry.AddParameter(fieldName, custReportRecord[fieldName]);
            }

            qry.AddParameter("REPORT_DATA", blob);
            qry.AddParameter("REPORT_HASH", checksum);
            qry.AddParameter("UPLOADED_BY", PLCSession.PLCGlobalAnalyst);
            qry.AddParameter("DATE_UPLOADED", DateTime.Now);

            bool uploadOk = qry.Save("TV_CUSTREPORTS");

            if (uploadOk)
            {
                string msg = string.Format("\n\nCustom report file for report key '{0}' uploaded to database by analyst '{1}'\n\n", custReportKey, PLCSession.PLCGlobalAnalyst);
                PLCSession.WriteDebug(msg);
            }

            return uploadOk;
        }

        /// <summary>
        /// Get a record from TV_CUSTREPORTS using cust report key
        /// </summary>
        /// <param name="custReportKey"></param>
        /// <returns></returns>
        private Dictionary<string, string> GetCustReportRecord(string custReportKey)
        {
            var record = new Dictionary<string, string>();

            var colList = GetTableColumnList("TV_CUSTREPORTS");

            var qry = new PLCQuery("SELECT * FROM TV_CUSTREPORTS WHERE CUST_REPORT_KEY=?");
            qry.AddSQLParameter("@CUST_REPORT_KEY", custReportKey);
            qry.Open();

            if (qry.HasData())
            {
                foreach (string col in colList)
                {
                    record.Add(col, qry.FieldByName(col));
                }
            }

            return record;
        }

        /// <summary>
        /// Get list of column names from a table
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private List<string> GetTableColumnList(string tableName)
        {
            var colList = new List<string>();
            string sql = "";

            if (PLCSession.PLCDatabaseServer == "MSSQL")
            {
                sql = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME=?";
            }
            else
            {
                sql = "SELECT COLUMN_NAME FROM USER_TAB_COLS WHERE TABLE_NAME=?";
            }

            var qr = new PLCQuery(sql);
            qr.AddSQLParameter("@TABLE_NAME", tableName);

            qr.Open();
            while (!qr.EOF())
            {
                string col = qr.FieldByName("COLUMN_NAME");
                colList.Add(col);
                qr.Next();
            }

            return colList;
        }

        /// <summary>
        /// Trigger update of master update panel (mode is conditional)
        /// </summary>
        private void TriggerMasterUpdatePanel()
        {
            UpdatePanel up = (UpdatePanel)this.Master.FindControl("UpdatePanel1");
            if (up != null)
            {
                up.Update();
            }
        }

        #region Run Reports Methods

        private List<CustomReports.Parameter> ReportParameters
        {
            get
            {
                object obj = ViewState["ReportParameters"];
                return (obj == null) ? new List<CustomReports.Parameter>() : ViewState["ReportParameters"] as List<CustomReports.Parameter>;
            }
            set
            {
                ViewState["ReportParameters"] = value;
            }
        }

        private void RunReport(string custReportKey)
        {
            //contains the dictionary of macros
            //key: macro
            //value: corresponding value for macro
            Dictionary<string, string> macroDict = new Dictionary<string, string>
            {
                {"%DEPTNAME%", PLCSession.PLCGlobalAnalystDepartmentCode},
                {"%USER%", PLCSession.PLCGlobalAnalyst},
                {"%LABCODE%", PLCSession.PLCGlobalLabCode}
            };
            //get parameters of report from adhoc.ini
            var parameters = new List<PLCWebCommon.CustomReports.Parameter>();

            var custReportRecord = GetCustReportRecord(custReportKey);
            string reptDesc = custReportRecord["REPORT_DESCRIPTION"];
            lblReport.Text = reptDesc;

            IniReader iniReader = new IniReader(CreateAdhocIni(custReportKey));
            iniReader.Section = reptDesc;
            foreach (string key in iniReader.GetKeyNames())
            {
                string[] attributes = iniReader.ReadString(key).Split('|');
                
                var p = new PLCWebCommon.CustomReports.Parameter();
                p.Name = key;
                p.ParameterText = key;

                var type = "";
                var mask = "";
                if (attributes.Length > 0)
                {
                    type = attributes[0].Trim();
                    if (type.Contains(":"))
                    {
                        mask = type.Split(':')[1];
                        type = type.Split(':')[0];
                    }
                }

                p.Type = type;
                p.Mask = mask;

                if ((p.Type == "IFDEF") || (p.Type == "IFNDEF"))
                {
                    p.ControlField = (attributes.Length > 1) ? attributes[1].Trim() : "";
                    p.Field = (attributes.Length > 2) ? attributes[2].Trim() : "";
                }
                else
                    p.Field = (attributes.Length > 1) ? attributes[1].Trim() : "";

                p.LocaField = "";
                p.Lookup = "";

                if (p.Type == "CUSTODY")
                    p.LocaField = (attributes.Length > 2) ? attributes[2].Trim() : "";
                else
                    p.Lookup = (attributes.Length > 2) ? attributes[2].Trim() : "";

                p.Required = (attributes.Length > 3) ? attributes[3].Trim() : "F";
                if ((p.Required == "T") || (p.Required == "TF")) p.Name += "<span style='color:red;'>*</span>";
                if (p.Required == "TT") p.Name += "<span style='color:red;'>**</span>";
                if (p.Required == "FT") p.Name += "<span style='color:green;'>*</span>";
                p.CodeCondition = (attributes.Length > 4) ? attributes[4].Trim() : "(1 = 1)";

                p.CustLocCodeCondition = (attributes.Length > 5) ? attributes[5].Trim() : "CUSTODY_CODE = ''";

                p.Value = "";
                p.LocaValue = "";

                //not sure why this is here, change to ALWAYS2 so  can be used if needed                    
                if (p.Type == "ALWAYS2" && p.Field.Contains("CUSTODY_OF")) //Parameters with ALWAYS type for CUSTUDY_OF field
                {
                    string field = p.Field;
                    p.Type = "CODE_ALWAYS";
                    p.Field = field.Substring(0, field.IndexOf("=") + 1).Trim();
                    p.Value = field.Substring(field.IndexOf("=") + 1).Trim().TrimStart('\'').TrimEnd('\'');
                    p.Lookup = "TV_CUSTCODE";
                }
                else if (p.Type == "CODE" || p.Type == "CODE_READONLY")
                {
                    //CODE          : enabled flexbox
                    //CODE_READONLY : disabled flexbox
                    string field = p.Field;

                    //proceed only when field is in this form: {SOME_TABLE}=SOME_VALUE
                    int indexOfEqual = field.IndexOf("=");
                    if (indexOfEqual > -1)
                    {
                        p.Field = field.Substring(0, indexOfEqual + 1).Trim();
                        string value = field.Substring(indexOfEqual + 1).Trim()
                                            .TrimStart('\'').TrimEnd('\'');

                        //see if the value is a macro
                        if (macroDict.ContainsKey(value))
                        {
                            //get the corresponding value for the macro
                            p.Value = macroDict[value];
                        }
                    }
                }

                parameters.Add(p);
            }

            ReportParameters = parameters;

            InitParamWithParent(ReportParameters);

            repParameters.DataSource = parameters;
            repParameters.DataBind();

            pnlParameters.Visible = parameters.Count > 0;
        }

        protected string GetDate(string dates, int index)
        {
            string[] dateRange = dates.Split(' ');
            return (dateRange.Length > index) ? dateRange[index] : "";
        }

        protected string GetCultureName()
        {
            if (PLCSession.GetDateFormat().ToUpper().StartsWith("DD/"))
                return "en-GB";
            else
                return "en-US";
        }

        protected MaskedEditUserDateFormat GetUserDateFormat()
        {
            if (PLCSession.GetDateFormat().ToUpper().StartsWith("DD/"))
                return AjaxControlToolkit.MaskedEditUserDateFormat.DayMonthYear;
            else
                return AjaxControlToolkit.MaskedEditUserDateFormat.MonthDayYear;
        }

        protected string GetCalendarFormat()
        {
            if (PLCSession.GetDateFormat().ToUpper().StartsWith("DD/"))
                return "dd/MM/yyyy";
            else
                return "MM/dd/yyyy";
        }

        protected void flxCustCode_ValueChanged(object sender, EventArgs e)
        {
            // RebindParameters for CodeMultipick
            RebindParameters(true);

            FlexBox flxCust = (FlexBox)sender;
            PlaceHolder ph = (PlaceHolder)flxCust.Parent;
            FlexBox flxLoc = (FlexBox)ph.FindControl("flxCustCodeLocation");
            flxLoc.CodeCondition = "CUSTODY_CODE = '" + flxCust.SelectedValue + "'";
            flxLoc.SetValue("");
        }

        protected void chLookup_ValueChanged(object sender, EventArgs e)
        {
            var chLookup = (FlexBox)sender;
            string parameterText = GetControlParameterText(chLookup);
            var param = ReportParameters.Where(p => p.ParameterText == parameterText).First();

            // check if parent param
            if (param.AutoPostBack)
            {
                RebindParameters(clearChildOfParent: parameterText);
            }
        }

        protected void txtValue_TextChanged(object sender, EventArgs e)
        {
            var txtValue = (TextBox)sender;
            string parameterText = GetControlParameterText(txtValue);
            var param = ReportParameters.Where(p => p.ParameterText == parameterText).First();

            // check if parent param
            if (param.AutoPostBack)
            {
                RebindParameters(clearChildOfParent: parameterText);
            }
        }

        private string CreateAdhocIni(string custReportKey)
        {
            try
            {
                string adhocini = "";
                string desc = "";
                var qry = new PLCQuery("SELECT SELECTION_CRITERIA, REPORT_DESCRIPTION FROM TV_CUSTREPORTS WHERE CUST_REPORT_KEY=?");
                qry.AddSQLParameter("@CUST_REPORT_KEY", custReportKey);
                qry.Open();
                if (qry.HasData())
                {
                    adhocini = qry.FieldByName("SELECTION_CRITERIA");
                    desc = "[" + qry.FieldByName("REPORT_DESCRIPTION") + "]";
                }

                if (!string.IsNullOrEmpty(adhocini.Trim()))
                {
                    var adhocPath = Path.GetTempPath() + Guid.NewGuid().ToString() + ".ini";
                    var adhocConfig = new System.IO.StreamWriter(adhocPath);
                    adhocConfig.WriteLine(desc);
                    adhocConfig.Write(adhocini);
                    adhocConfig.Close();
                    return adhocPath;
                }
            }
            catch (Exception ex) { }
            return Server.MapPath("~/Reports/Custom/adhoc.ini");
        }

        private void ViewReportUsingAdhocIni(string custReportKey, string reportFile)
        {
            string selectionFormula = "";
            string description = "";
            string thisRepeatField = "";
            string formulastring = "";
            int repeatcount = 0;
            int thisrepeat = 0;

            RollUpRepeatingValues();

            try
            {
                //generate selection formula
                foreach (RepeaterItem item in repParameters.Items)
                {
                    string fieldType = ((HiddenField)item.FindControl("hdnType")).Value;
                    string field = ((HiddenField)item.FindControl("hdnField")).Value.Trim();
                    string controlfield = ((HiddenField)item.FindControl("hdnControlField")).Value.Trim();
                    string locafield = ((HiddenField)item.FindControl("hdnLocaField")).Value.Trim();
                    string prompt = ((HiddenField)item.FindControl("hdnParameterText")).Value;

                    string value = "";
                    string valueDesc = "";
                    string thiscondition = "";

                    switch (fieldType)
                    {
                        case "TEXT": //format: [field] = 'value'
                            value = ((TextBox)item.FindControl("txtValue")).Text;

                            if ((field.StartsWith("@")) && (!string.IsNullOrEmpty(value)))
                            {
                                field = field.Substring(1);
                                formulastring += field + "='" + value + "';";
                                break;
                            }

                            if (!HasOperator(field))
                                field = field + "=";

                            if (value != "")
                            {
                                if (!string.IsNullOrEmpty(selectionFormula)) selectionFormula += " and ";
                                selectionFormula += field + " '" + value + "' ";
                                if (!string.IsNullOrEmpty(description)) description += "<br />";
                                description += "- " + prompt + GetOperatorDesc(field) + value + " ";
                            }
                            break;

                        case "CUSTODY": //format: [field] = 'value'
                            value = ((FlexBox)item.FindControl("flxCustCode")).SelectedValue;
                            valueDesc = ((FlexBox)item.FindControl("flxCustCode")).SelectedText;
                            string value2 = ((FlexBox)item.FindControl("flxCustCodeLocation")).SelectedValue;
                            string value2Desc = ((FlexBox)item.FindControl("flxCustCodeLocation")).SelectedText;

                            if ((field.StartsWith("@")) && (!string.IsNullOrEmpty(value)))
                            {
                                field = field.Substring(1);
                                formulastring += field + "='" + value + "';";
                                formulastring += field + "_DESC" + "='" + valueDesc + "';";
                                value = ((FlexBox)item.FindControl("flxCustCodeLocation")).SelectedValue;
                                if ((locafield.StartsWith("@")) && (!string.IsNullOrEmpty(value2)))
                                {
                                    locafield = locafield.Substring(1);
                                    formulastring += locafield + "='" + value2 + "';";
                                    formulastring += locafield + "_DESC" + "='" + value2Desc + "';";
                                }
                                break;
                            }

                            if (value != "")
                            {
                                repeatcount = GetRepeatCount(field);

                                if (!HasOperator(field))
                                    field = field + "=";

                                if (!HasOperator(locafield))
                                    locafield = locafield + "=";

                                string custDesc = "";
                                if (!string.IsNullOrEmpty(value2))
                                    custDesc = PLCSession.GetCustodyDesc(value, value2);
                                else
                                    custDesc = PLCSession.GetCodeDesc("TV_CUSTCODE", value);

                                thiscondition = "(" + field + "'" + value + "'";

                                if (!string.IsNullOrEmpty(value2)) thiscondition += " and " + locafield + "'" + value2 + "'";
                                thiscondition += ")";

                                if ((thisRepeatField != field) && (repeatcount > 1))
                                {
                                    thisrepeat = 0;
                                    thisRepeatField = field;
                                }

                                if (repeatcount > 1)
                                {
                                    //begin repeat
                                    thisrepeat++;

                                    if (thisrepeat == 1)
                                    {
                                        if (!string.IsNullOrEmpty(selectionFormula)) selectionFormula += " and ";
                                        selectionFormula += "(" + thiscondition;
                                        if (!string.IsNullOrEmpty(description)) description += "<br />";
                                        description += "- " + prompt + GetOperatorDesc(field) + "'" + custDesc + "'";
                                    }
                                    else if (thisrepeat < repeatcount)
                                    {
                                        selectionFormula += " or " + thiscondition;
                                        if (!string.IsNullOrEmpty(description)) description += "<br />";
                                        description += "     or,  " + prompt + GetOperatorDesc(field) + "'" + custDesc + "'";
                                    }
                                    else if (thisrepeat == repeatcount)
                                    {
                                        selectionFormula += " or " + thiscondition + ")";
                                        if (!string.IsNullOrEmpty(description)) description += "<br />";
                                        description += "     or,  " + prompt + GetOperatorDesc(field) + "'" + custDesc + "'";
                                    }

                                    //end repeat
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(selectionFormula)) selectionFormula += " and ";
                                    selectionFormula += field + "'" + value + "'";
                                    if (!string.IsNullOrEmpty(description)) description += "<br />";
                                    description += "- " + prompt + GetOperatorDesc(field) + custDesc;

                                    if (!String.IsNullOrWhiteSpace(value2))
                                    {
                                        if (!string.IsNullOrEmpty(selectionFormula)) selectionFormula += " and ";
                                        selectionFormula += locafield + "'" + value2 + "'";
                                    }
                                }
                            }
                            break;

                        case "LIST": //format: [field] = 'value'
                            value = ((TextBox)item.FindControl("txtValue")).Text;
                            if (!HasOperator(field))
                                field = field + " IN";
                            if (value != "")
                            {
                                if (!string.IsNullOrEmpty(selectionFormula)) selectionFormula += " and ";
                                selectionFormula += field + " " + MakeList(value);
                                if (!string.IsNullOrEmpty(description)) description += "<br />";
                                description += "- " + prompt + GetOperatorDesc(field) + "'" + value + "'";
                            }
                            break;

                        case "NUMBER": //format: [field] = 'value'
                            value = ((TextBox)item.FindControl("txtValue")).Text;

                            if ((field.StartsWith("@")) && (!string.IsNullOrEmpty(value)))
                            {
                                field = field.Substring(1);
                                formulastring += field + "=" + value + ";";
                                break;
                            }

                            if (!HasOperator(field))
                                field = field + "=";
                            if (value != "")
                            {
                                if (!string.IsNullOrEmpty(selectionFormula)) selectionFormula += " and ";
                                selectionFormula += field + " " + value + " ";
                                if (!string.IsNullOrEmpty(description)) description += "<br />";
                                description += "- " + prompt + GetOperatorDesc(field) + value + " ";
                            }
                            break;

                        case "NUMBERRANGE": //format: [fieldstart] >= 'value' and [fieldend] <= 'value'
                            string fldStart = field;
                            string fldEnd = field;
                            string numStart = ((TextBox)item.FindControl("txtNumberStart")).Text;
                            string numEnd = ((TextBox)item.FindControl("txtNumberEnd")).Text;

                            if (field.StartsWith("@") && field.Contains(","))
                            {
                                if (!string.IsNullOrEmpty(numStart) || !string.IsNullOrEmpty(numEnd))
                                {
                                    string[] formulaFields = field.Split(',');
                                    fldStart = formulaFields[0].Substring(1);
                                    fldEnd = formulaFields[1].Substring(1);

                                    if (!string.IsNullOrEmpty(numStart))
                                        formulastring += fldStart + "=" + numStart + ";";

                                    if (!string.IsNullOrEmpty(numEnd))
                                        formulastring += fldEnd + "=" + numEnd + ";";
                                }

                                break;
                            }

                            if (numStart != "")
                            {
                                if (!string.IsNullOrEmpty(selectionFormula)) selectionFormula += " and ";
                                selectionFormula += fldStart + " >= " + numStart;
                                if (!string.IsNullOrEmpty(description)) description += "<br />";
                                description += "- " + prompt + " is from " + numStart;
                            }

                            if (numEnd != "")
                            {
                                if (!string.IsNullOrEmpty(selectionFormula)) selectionFormula += " and ";
                                selectionFormula += fldEnd + " <= " + numEnd;

                                if (numStart != "")
                                    description += " to " + numEnd;
                                else
                                {
                                    if (!string.IsNullOrEmpty(description)) description += "<br />";
                                    description += "- " + prompt + " is until " + numEnd;
                                }
                            }
                            break;

                        case "CODE": //format: [field] = 'value'
                        case "CODE_ALWAYS":
                        case "CODE_READONLY":
                            value = ((FlexBox)item.FindControl("chLookup")).GetValue();
                            string codeDescription = ((FlexBox)item.FindControl("chLookup")).SelectedText;

                            if ((field.StartsWith("@")) && (!string.IsNullOrEmpty(value)))
                            {
                                field = field.Substring(1);
                                formulastring += field + "='" + value + "';";
                                formulastring += field + "_DESC" + "='" + codeDescription + "';";
                                break;
                            }

                            if (!HasOperator(field))
                                field = field + "=";
                            if (value != "")
                            {
                                thiscondition = field + "'" + value + "'";
                                repeatcount = GetRepeatCount(field);

                                if ((thisRepeatField != field) && (repeatcount > 1))
                                {
                                    thisrepeat = 0;
                                    thisRepeatField = field;
                                }

                                if (repeatcount > 1)
                                {
                                    thisrepeat++;

                                    if (thisrepeat == 1)
                                    {
                                        if (!string.IsNullOrEmpty(selectionFormula)) selectionFormula += " and ";
                                        selectionFormula += "(" + thiscondition;
                                        if (!string.IsNullOrEmpty(description)) description += "<br />";
                                        description += "- " + prompt + GetOperatorDesc(field) + "'" + codeDescription + "'";
                                    }
                                    else if (thisrepeat < repeatcount)
                                    {
                                        selectionFormula += " or " + thiscondition;
                                        if (!string.IsNullOrEmpty(description)) description += "<br />";
                                        description += "     or,  " + prompt + GetOperatorDesc(field) + "'" + codeDescription + "'";
                                    }
                                    else if (thisrepeat == repeatcount)
                                    {
                                        selectionFormula += " or " + thiscondition + ")";
                                        if (!string.IsNullOrEmpty(description)) description += "<br />";
                                        description += "     or,  " + prompt + GetOperatorDesc(field) + "'" + codeDescription + "'";
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(selectionFormula)) selectionFormula += " and ";
                                    selectionFormula += thiscondition;
                                    if (!string.IsNullOrEmpty(description)) description += "<br />";
                                    description += "- " + prompt + " is '" + codeDescription + "'";
                                }
                            }
                            break;

                        case "DATE": //format: [date] = Date(y,m,d)
                            if (field.EndsWith("="))
                                field = field.TrimEnd(new char[] { '=' });
                            if (((TextBox)item.FindControl("txtValue")).Text != "")
                            {
                                DateTime dateEntry = PLCSession.ConvertToDateTime(((TextBox)item.FindControl("txtValue")).Text).Date;
                                value = "Date(" + dateEntry.Year + "," + dateEntry.Month + "," + dateEntry.Day + ")";

                                if (field.StartsWith("@"))
                                {
                                    field = field.Substring(1);
                                    formulastring += field + "=" + value + ";";
                                    break;
                                }

                                if (!string.IsNullOrEmpty(selectionFormula)) selectionFormula += " and ";
                                selectionFormula += field + " = " + value;
                                if (!string.IsNullOrEmpty(description)) description += "<br />";
                                description += "- " + prompt + " is on " + dateEntry.ToShortDateString();
                            }
                            break;

                        case "DATERANGE": //format: [datestart] >= Date(y,m,d) and [dateend] <= Date(y,m,d)
                            if (field.EndsWith("="))
                                field = field.TrimEnd(new char[] { '=' });
                            string fld1 = field;
                            string fld2 = field;

                            string dateStart = ((TextBox)item.FindControl("txtDateStart")).Text;
                            string dateEnd = ((TextBox)item.FindControl("txtDateEnd")).Text;

                            if (field.StartsWith("@"))
                            {
                                if ((!string.IsNullOrEmpty(dateStart)) || (!string.IsNullOrEmpty(dateEnd)))
                                {
                                    string[] formulaFields = field.Split(',');
                                    fld1 = formulaFields[0].Substring(1);
                                    fld2 = formulaFields[1].Substring(1);

                                    if (!string.IsNullOrEmpty(dateStart))
                                    {
                                        try
                                        {
                                            DateTime dateEntry = PLCSession.ConvertToDateTime(dateStart).Date;
                                            formulastring += fld1 + "=" + "Date(" + dateEntry.Year + "," + dateEntry.Month + "," + dateEntry.Day + ");";
                                        }
                                        catch
                                        {

                                        }
                                    }

                                    if (!string.IsNullOrEmpty(dateEnd))
                                    {
                                        try
                                        {
                                            DateTime dateEntry = PLCSession.ConvertToDateTime(dateEnd).Date;
                                            formulastring += fld2 + "=" + "Date(" + dateEntry.Year + "," + dateEntry.Month + "," + dateEntry.Day + ");";
                                        }
                                        catch
                                        {
                                        }
                                    }
                                }

                                break;
                            }

                            if (dateStart != "")
                            {
                                DateTime dateEntry = PLCSession.ConvertToDateTime(dateStart).Date;
                                if (!string.IsNullOrEmpty(selectionFormula)) selectionFormula += " and ";
                                selectionFormula += fld1 + " >= Date(" + dateEntry.Year + "," + dateEntry.Month + "," + dateEntry.Day + ")";
                                if (!string.IsNullOrEmpty(description)) description += "<br />";
                                description += "- " + prompt + " is from " + dateEntry.ToShortDateString();
                            }

                            if (dateEnd != "")
                            {
                                DateTime dateEntry = PLCSession.ConvertToDateTime(dateEnd).Date;
                                if (!string.IsNullOrEmpty(selectionFormula)) selectionFormula += " and ";
                                selectionFormula += fld2 + " <= Date(" + dateEntry.Year + "," + dateEntry.Month + "," + dateEntry.Day + ")";
                                if (dateStart != "")
                                    description += " to " + dateEntry.ToShortDateString();
                                else
                                {
                                    if (!string.IsNullOrEmpty(description)) description += "<br />";
                                    description += "- " + prompt + " is until " + dateEntry.ToShortDateString();
                                }
                            }
                            break;

                        case "ALWAYS": //format: [field] = [value]
                            if (!string.IsNullOrEmpty(selectionFormula)) selectionFormula += " and ";
                            selectionFormula += field;
                            break;

                        case "IFDEF": //format: [field] = [value]
                            if (controlfield.StartsWith("@"))
                            {
                                if (HasValue(controlfield))
                                {
                                    if (!string.IsNullOrEmpty(selectionFormula)) selectionFormula += " and ";
                                    selectionFormula += field;
                                }
                            }
                            break;

                        case "IFNDEF": //format: [field] = [value]
                            if (controlfield.StartsWith("@"))
                            {
                                if (!HasValue(controlfield))
                                {
                                    if (!string.IsNullOrEmpty(selectionFormula)) selectionFormula += " and ";
                                    selectionFormula += field;
                                }
                            }
                            break;

                        case "MULTIPICK": //format: [field] IN [csv]
                            var cmpMultiLookup = ((CodeMultiPick)item.FindControl("cmpMultiLookup"));
                            value = cmpMultiLookup.GetText();
                            if (field.EndsWith("="))
                                field = field.TrimEnd(new char[] { '=' });
                            if (!HasOperator(field))
                                field = field + " IN";
                            if (value != "")
                            {
                                if (!string.IsNullOrEmpty(selectionFormula)) selectionFormula += " and ";
                                selectionFormula += "(" + field + " "
                                    + string.Join(" OR " + field + " ", PLCHelperFunctions.MakeListsForSqlIN(value))
                                    + ")";
                                if (!string.IsNullOrEmpty(description)) description += "<br />";
                                description += "- " + prompt + GetOperatorDesc(field) + "'" + PLCHelperFunctions.CreateListDisplayText(value) + "'";
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error encountered: " + ex.Message;
                return;
            }

            PLCSession.WriteDebug("Custom Report: " + lblReport.Text + "; FileName: " + reportFile + "; Parameters: " + selectionFormula, true);
            PLCSession.PLCCrystalReportName = "~/reports/custom/" + reportFile;
            PLCSession.PLCCrystalReportTitle = lblReport.Text;
            PLCSession.PLCCrystalReportComments = (string.IsNullOrEmpty(description) ? "" : description);
            PLCSession.PLCCrystalSelectionFormula = selectionFormula;

            formulastring += "GLOBALANALYST=\"" + PLCSession.PLCGlobalAnalyst + "\";";

            PLCSession.PLCCrystalReportFormulaList = formulastring;
            PLCSession.PLCCrystalCustomReportKey = custReportKey;

            //if (format != "")
            //    Response.Redirect("CustomRptExport.aspx?FMT=" + format);
            //else
                PLCSession.PrintCRWReport(false);
        }

        private Boolean HasOperator(string field)
        {
            field = field.ToUpper().Trim();

            if (field.EndsWith("="))
            {
                return true;
            }

            if (field.EndsWith("<>"))
            {
                return true;
            }

            if (field.EndsWith(">"))
            {
                return true;
            }

            if (field.EndsWith("<"))
            {
                return true;
            }

            if (field.EndsWith(" NOT IN"))
            {
                return true;
            }

            if (field.EndsWith(" IN"))
            {
                return true;
            }

            if (field.EndsWith("LIKE"))
            {
                return true;
            }

            return false;
        }

        private string GetOperatorDesc(string field)
        {
            field = field.ToUpper().Trim();

            if (field.EndsWith("="))
            {
                return " is equal to ";
            }

            if (field.EndsWith("<>"))
            {
                return " is not equal to ";
            }

            if (field.EndsWith(">"))
            {
                return " is greater than ";
            }

            if (field.EndsWith("<"))
            {
                return " is less than ";
            }

            if (field.EndsWith("LIKE"))
            {
                return " is like ";
            }

            if (field.EndsWith(" IN"))
            {
                return " is one of ";
            }

            if (field.EndsWith(" NOT IN"))
            {
                return " is not one of ";
            }

            return " is equal to";
        }
        
        private Boolean HasValue(string cfield)
        {
            string value = "";

            foreach (RepeaterItem item in repParameters.Items)
            {
                string fieldType = ((HiddenField)item.FindControl("hdnType")).Value;
                string field = ((HiddenField)item.FindControl("hdnField")).Value.Trim();

                if (field == cfield)
                {
                    switch (fieldType)
                    {
                        case "TEXT": //format: [field] = 'value'
                        case "NUMBER":
                        case "LIST":
                            value = ((TextBox)item.FindControl("txtValue")).Text;
                            break;
                        case "NUMBERRANGE":
                            value = ((TextBox)item.FindControl("txtNumberStart")).Text;
                            break;
                        case "CUSTODY": //format: [field] = 'value'
                            value = ((FlexBox)item.FindControl("flxCustCode")).SelectedValue;
                            break;
                        case "CODE": //format: [field] = 'value'
                        case "CODE_ALWAYS":
                        case "CODE_READONLY":
                            value = ((FlexBox)item.FindControl("chLookup")).GetValue();
                            break;
                        case "DATERANGE":
                            value = ((TextBox)item.FindControl("txtDateStart")).Text;
                            break;
                        case "DATE":
                            value = ((TextBox)item.FindControl("txtValue")).Text;
                            if (value == null) value = "";
                            if (value.Length < 8) value = "";
                            break;
                        case "MULTIPICK":
                            value = ((CodeMultiPick)item.FindControl("cmpMultiLookup")).GetText();
                            break;
                    }
                }

                if (value == null) value = "";
                value = value.Trim();
            }

            return (value != "");
        }

        private int GetRepeatCount(string condfield)
        {
            int condCount = 0;

            foreach (RepeaterItem item in repParameters.Items)
            {
                string field = ((HiddenField)item.FindControl("hdnField")).Value;
                string fieldType = ((HiddenField)item.FindControl("hdnType")).Value;

                if (fieldType == "CODE" || fieldType == "CODE_READONLY")
                {
                    string value = ((FlexBox)item.FindControl("chLookup")).GetValue();
                    if ((field == condfield) && (!string.IsNullOrEmpty(value))) condCount++;
                }

                if (fieldType == "CUSTODY")
                {
                    string value = ((FlexBox)item.FindControl("flxCustCode")).GetValue();
                    if ((field == condfield) && (!string.IsNullOrEmpty(value))) condCount++;
                }
            }

            return condCount;
        }

        private string MakeList(string value)
        {
            string[] items = value.Split(',');
            value = "";
            foreach (string item in items)
            {
                if (!string.IsNullOrEmpty(value)) value += ",";
                value += "'" + item + "'";
            }

            return "[" + value + "]";
        }

        private void RollUpRepeatingValues()
        {
            foreach (RepeaterItem item in repParameters.Items)
            {
                string fieldType = ((HiddenField)item.FindControl("hdnType")).Value;
                string field = ((HiddenField)item.FindControl("hdnField")).Value.Trim();
                string controlfield = ((HiddenField)item.FindControl("hdnControlField")).Value.Trim();
                string locafield = ((HiddenField)item.FindControl("hdnLocaField")).Value.Trim();
                string prompt = ((HiddenField)item.FindControl("hdnParameterText")).Value;

                int repeatcount = GetRollupCount(field);
                Boolean switched = false;
                if (repeatcount > 0)
                {
                    do
                    {
                        switched = false;

                        for (int r = 1; r < repeatcount; r++)
                        {
                            RepeaterItem itemA = GetRepeaterItemByIndex(field, r);
                            RepeaterItem itemB = GetRepeaterItemByIndex(field, r + 1);

                            String valA = GetValue(itemA);
                            String valB = GetValue(itemB);

                            if ((IsEmptyVal(valA)) && (!IsEmptyVal(valB)))
                            {
                                switched = true;
                                SetValue(itemA, valB);
                                SetValue(itemB, "");
                            }
                        }

                    } while (switched);
                }
            }
        }

        private Boolean IsEmptyVal(String v)
        {
            v = v.Trim();
            return ((String.IsNullOrWhiteSpace(v)) || (v == "~"));
        }

        private int GetRollupCount(string condfield)
        {
            int condCount = 0;

            condfield = Regex.Replace(condfield, @"[\d-]", string.Empty);

            foreach (RepeaterItem item in repParameters.Items)
            {
                string field = ((HiddenField)item.FindControl("hdnField")).Value;
                field = Regex.Replace(field, @"[\d-]", string.Empty);
                string fieldType = ((HiddenField)item.FindControl("hdnType")).Value;

                if (fieldType == "CODE" || fieldType == "CODE_READONLY")
                {
                    string value = ((FlexBox)item.FindControl("chLookup")).GetValue();
                    if (field == condfield) condCount++;
                }

                if (fieldType == "CUSTODY")
                {

                    if (field == condfield) condCount++;
                }
            }

            return condCount;

        }

        private RepeaterItem GetRepeaterItemByIndex(String condfield, int itemIdx)
        {
            int condCount = 0;

            condfield = Regex.Replace(condfield, @"[\d-]", string.Empty);

            foreach (RepeaterItem item in repParameters.Items)
            {
                string field = ((HiddenField)item.FindControl("hdnField")).Value;
                field = Regex.Replace(field, @"[\d-]", string.Empty);
                string fieldType = ((HiddenField)item.FindControl("hdnType")).Value;

                if (fieldType == "CODE" || fieldType == "CODE_READONLY")
                {
                    if (field == condfield) condCount++;
                }

                if (fieldType == "CUSTODY")
                {
                    if (field == condfield) condCount++;
                }

                if (condCount == itemIdx) return item;
            }

            return (RepeaterItem)null;
        }

        private String GetValue(RepeaterItem item)
        {
            string value = "";

            if (item == null) return "";

            string fieldType = ((HiddenField)item.FindControl("hdnType")).Value;
            string field = ((HiddenField)item.FindControl("hdnField")).Value.Trim();

            switch (fieldType)
            {
                case "TEXT": //format: [field] = 'value'
                case "NUMBER":
                case "LIST":
                    value = ((TextBox)item.FindControl("txtValue")).Text;
                    break;
                case "NUMBERRANGE":
                    value = ((TextBox)item.FindControl("txtNumberStart")).Text;
                    break;
                case "CUSTODY": //format: [field] = 'value'
                    String value1 = ((FlexBox)item.FindControl("flxCustCode")).SelectedValue;
                    String value2 = ((FlexBox)item.FindControl("flxCustCodeLocation")).SelectedValue;
                    value = value1 + "~" + value2;
                    break;
                case "CODE": //format: [field] = 'value'
                case "CODE_ALWAYS":
                case "CODE_READONLY":
                    value = ((FlexBox)item.FindControl("chLookup")).GetValue();
                    break;
                case "DATERANGE":
                    value = ((TextBox)item.FindControl("txtDateStart")).Text;
                    break;
                case "DATE":
                    value = ((TextBox)item.FindControl("txtValue")).Text;
                    if (value == null) value = "";
                    if (value.Length < 8) value = "";
                    break;
                case "MULTIPICK":
                    value = ((CodeMultiPick)item.FindControl("cmpMultiLookup")).GetText();
                    break;
            }

            if (value == null) value = "";
            value = value.Trim();

            return value;
        }

        private void SetValue(RepeaterItem item, String val)
        {
            string fieldType = ((HiddenField)item.FindControl("hdnType")).Value;
            string field = ((HiddenField)item.FindControl("hdnField")).Value.Trim();

            switch (fieldType)
            {
                case "TEXT": //format: [field] = 'value'
                case "NUMBER":
                case "LIST":
                    ((TextBox)item.FindControl("txtValue")).Text = val;
                    break;
                case "NUMBERRANGE":
                    ((TextBox)item.FindControl("txtNumberStart")).Text = val;
                    break;
                case "CUSTODY": //format: [field] = 'value'
                    string v1 = "";
                    string v2 = "";
                    if ((val.Contains("~") && (val != "~")))
                    {
                        v1 = val.Split('~')[0];
                        v2 = val.Split('~')[1];
                    }

                    ((FlexBox)item.FindControl("flxCustCode")).ClearValues();
                    ((FlexBox)item.FindControl("flxCustCodeLocation")).ClearValues();
                    ((FlexBox)item.FindControl("flxCustCode")).SelectedValue = v1;

                    ((FlexBox)item.FindControl("flxCustCodeLocation")).CodeCondition = "CUSTODY_CODE = '" + v1 + "'";
                    ((FlexBox)item.FindControl("flxCustCodeLocation")).SelectedValue = v2;
                    break;
                case "CODE": //format: [field] = 'value'
                case "CODE_ALWAYS":
                case "CODE_READONLY":
                    ((FlexBox)item.FindControl("chLookup")).SelectedValue = val;
                    break;
                case "DATERANGE":
                    ((TextBox)item.FindControl("txtDateStart")).Text = val;
                    break;
                case "DATE":
                    ((TextBox)item.FindControl("txtValue")).Text = val;
                    break;
                case "MULTIPICK":
                    ((CodeMultiPick)item.FindControl("cmpMultiLookup")).SetText(val);
                    break;
            }
        }

        #region Parent Parameter
        private string ApplyParentFilter(string codeCondition, string parent, string value)
        {
            // apply parent value to the code condition
            // parent value is not enclosed in single quotes
            string cc = codeCondition;
            cc = cc.Replace("{" + parent + "}", value);
            return cc;
        }

        private List<CustomReports.Parameter> ProcessParamWithParent(List<CustomReports.Parameter> parameters, string clearChildOfParent)
        {
            var paramWithParent = parameters.Where(p => p.OrigCodeCondition != null);

            if (paramWithParent.Any())
            {
                foreach (var param in paramWithParent)
                {
                    string codeCondition = param.OrigCodeCondition;

                    do
                    {
                        // parse parent name
                        int startIndex = codeCondition.IndexOf("{") + 1;
                        int endIndex = codeCondition.IndexOf("}");
                        string parent = codeCondition.Substring(startIndex, endIndex - startIndex);

                        // get parent param
                        var parentParam = parameters.First(p => p.ParameterText.ToUpper() == parent.ToUpper());
                        if (parentParam == null)
                        {
                            continue;
                        }

                        if (parentParam.ParameterText == clearChildOfParent)
                        {
                            param.Value = "";
                        }

                        codeCondition = ApplyParentFilter(codeCondition, parent, parentParam.Value);

                    } while (codeCondition.Contains("{") && codeCondition.Contains("}"));

                    param.CodeCondition = codeCondition;
                }
            }

            return parameters;
        }

        private List<CustomReports.Parameter> InitParamWithParent(List<CustomReports.Parameter> parameters)
        {
            var paramWithParent = parameters.Where(p =>
                p.CodeCondition.Contains("{") && p.CodeCondition.Contains("}"));

            if (paramWithParent.Any())
            {
                foreach (var param in paramWithParent)
                {
                    string codeCondition = param.CodeCondition;
                    param.OrigCodeCondition = codeCondition;

                    do
                    {
                        // parse parent name
                        int startIndex = codeCondition.IndexOf("{") + 1;
                        int endIndex = codeCondition.IndexOf("}");
                        string parent = codeCondition.Substring(startIndex, endIndex - startIndex);

                        // get parent param
                        var parentParam = parameters.First(p => p.ParameterText.ToUpper() == parent.ToUpper());
                        if (parentParam == null)
                        {
                            PLCSession.WriteDebug("@ParamWithParent: " + parent + " param not found.");
                            continue;
                        }

                        parentParam.AutoPostBack = true;

                        codeCondition = ApplyParentFilter(codeCondition, parent, parentParam.Value);

                    } while (codeCondition.Contains("{") && codeCondition.Contains("}"));

                    param.CodeCondition = codeCondition;
                }
            }

            return parameters;
        }
        #endregion Parent Parameter

        private string GetControlParameterText(WebControl control)
        {
            var hdnParameterText = (HiddenField)control.NamingContainer.FindControl("hdnParameterText");
            return hdnParameterText.Value;
        }

        #endregion

        /// <summary>
        /// Download custom report
        /// </summary>
        /// <param name="crk"></param>
        private void DownloadReport(string crk)
        {
            string script = string.Format("$(function(){{__dlReport({0});}});", crk);
            ScriptManager.RegisterStartupScript(
                this,
                this.GetType(),
                "____DOWNLOAD_REPORT",
                script,
                true
            ); 
        }

        private void EnableDownloadButton(bool enable)
        {
            PLCButtonPanel1.SetCustomButtonEnabled("Download Report", enable);
        }

        private void ShouldEnableDownload(string custReptKey)
        {
            bool inDb = PLCDBGlobal.instance.IsCustReportFileInDatabase(custReptKey);
            if (inDb)
                EnableDownloadButton(true);
            else
                EnableDownloadButton(false);
        }

        private void RebindParameters(bool refreshCustLoc = false, string clearChildOfParent = "")
        {
            string value = "";
            string locavalue = "";

            if (repParameters.Items.Count > 0)
            {
                List<CustomReports.Parameter> parameters = ReportParameters;

                for (int i = 0; i < repParameters.Items.Count; i++)
                {
                    RepeaterItem item = repParameters.Items[i];
                    string fieldType = ((HiddenField)item.FindControl("hdnType")).Value;
                    value = "";
                    locavalue = "";
                    switch (fieldType)
                    {
                        case "CUSTODY":
                            FlexBox fb = (FlexBox)item.FindControl("flxCustCode");
                            if (fb != null) value = fb.SelectedValue;
                            fb = (FlexBox)item.FindControl("flxCustCodeLocation");
                            if (fb != null)
                            {
                                if (refreshCustLoc)
                                    locavalue = "";
                                else
                                    locavalue = fb.SelectedValue;
                                parameters[i].CustLocCodeCondition = "CUSTODY_CODE = '" + value + "'";
                            }
                            break;
                        case "TEXT":
                            value = ((TextBox)item.FindControl("txtValue")).Text;
                            break;
                        case "LIST":
                            value = ((TextBox)item.FindControl("txtValue")).Text;
                            break;
                        case "NUMBER":
                            value = ((TextBox)item.FindControl("txtValue")).Text;
                            break;
                        case "NUMBERRANGE":
                            string numStart = ((TextBox)item.FindControl("txtNumberStart")).Text;
                            string numEnd = ((TextBox)item.FindControl("txtNumberEnd")).Text;
                            value = numStart + " " + numEnd;
                            break;
                        case "CODE":
                        case "CODEALWAYS":
                        case "CODE_READONLY":
                            FlexBox flexboxControl = (FlexBox)item.FindControl("chLookup");
                            if (flexboxControl != null) value = flexboxControl.SelectedValue;
                            break;
                        case "DATE":
                            value = ((TextBox)item.FindControl("txtValue")).Text;
                            break;
                        case "DATERANGE":
                            string dateStart = ((TextBox)item.FindControl("txtDateStart")).Text;
                            string dateEnd = ((TextBox)item.FindControl("txtDateEnd")).Text;
                            value = dateStart + " " + dateEnd;
                            break;
                        case "ALWAYS":
                        case "IFDEF":
                        case "IFNDEF":
                            value = "";
                            break;
                        case "MULTIPICK":
                            value = ((CodeMultiPick)item.FindControl("cmpMultiLookup")).GetText();
                            break;
                    }

                    parameters[i].Value = value;
                    if (locavalue == null) locavalue = "";
                    parameters[i].LocaValue = locavalue;
                }

                ReportParameters = parameters;
            }

            ProcessParamWithParent(ReportParameters, clearChildOfParent);

            repParameters.DataSource = ReportParameters;
            repParameters.DataBind();
        }

        protected void btnPageEvents_Click(object sender, EventArgs e)
        {
            if (GridView1.Rows.Count > 0 && GridView1.SelectedIndex >= 0)
            {
                ShouldEnableDownload(GridView1.SelectedDataKey[0].ToString());
            }
        }

        /// <summary>
        /// need this since PLCButton Panel is outside UpdatePanel control
        /// </summary>
        private void TriggerAfterClickEvents()
        {
            string script = string.Format("clickPageEventsButton();");
            ScriptManager.RegisterStartupScript(
                this,
                this.GetType(),
                "____TRIGGER_PAGE_EVENTS",
                script,
                true
            );
        }

        #endregion
    }
}