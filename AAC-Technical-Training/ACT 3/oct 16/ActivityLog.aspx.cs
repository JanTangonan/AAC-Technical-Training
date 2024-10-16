using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PLCCONTROLS;
using PLCGlobals;
using PLCWebCommon;
using System.Globalization;
using System.Drawing;
using DayPilot.Web.Ui.Events;

namespace BEASTiLIMS
{
    public partial class ActivityLog : PageBase
    {

        #region Declarations & Properties

        private const string QRY_ACTIVITY_DETAIL = "SELECT {0} FROM {1} T WHERE T.ACTIVITY_KEY = {2}";
        private const string QRY_CHECK_DBPANEL_EXISTS = "SELECT FIELD_NAME, EDIT_MASK FROM TV_DBPANEL P WHERE P.PANEL_NAME = 'ACT_{0}'";
        string ACTIVITYLOG_GRID = "ACTIVITYLOG";

        CodeHead chAnalyst;
        PLCMsgBox mbox;

        private bool AddNewActivityMode
        {
            get
            {
                if (ViewState["ISADDREC"] == null)
                    ViewState["ISADDREC"] = false;
                return (bool)ViewState["ISADDREC"];
            }
            set
            {
                ViewState["ISADDREC"] = value;
            }
        }

        private bool EditActivityMode
        {
            get
            {
                if (ViewState["ISEDITREC"] == null)
                    ViewState["ISEDITREC"] = false;
                return (bool)ViewState["ISEDITREC"];
            }
            set
            {
                ViewState["ISEDITREC"] = value;
            }
        }

        private bool AfterPLCButtonClick
        {
            get
            {
                if (ViewState["APBC"] == null)
                    ViewState["APBC"] = false;
                return (bool)ViewState["APBC"];
            }
            set
            {
                ViewState["APBC"] = value;
            }
        }

        private class ActivityCodes
        {
            public string CODE { get; set; }
            public string DESCRIPTION
            {
                get
                {
                    return PLCSession.GetCodeDesc("ACTCODE", this.CODE);
                }
            }
            public string PANELNAME
            {
                get
                {
                    return "ACT_" + this.CODE;
                }
            }
        }

        private Dictionary<string, Dictionary<string, object>> Activities
        {
            get
            {
                if (ViewState["ACTRECORDS"] == null)
                    ViewState["ACTRECORDS"] = new Dictionary<string, Dictionary<string, object>>();
                return (Dictionary<string, Dictionary<string, object>>)ViewState["ACTRECORDS"];
            }
            set
            {
                ViewState["ACTRECORDS"] = value;
            }
        }

        private string ActivityKey
        {
            get
            {
                if (Session["ACTIVITY_KEY"] == null)
                    return "";
                else
                    return Session["ACTIVITY_KEY"].ToString();
            }
            set { Session["ACTIVITY_KEY"] = value; }
        }

        private bool FromCaseInfo
        {
            get
            {
                if (ViewState["FROM_CASE_INFO"] == null)
                    return false;
                return (bool)ViewState["FROM_CASE_INFO"];
            }
            set
            {
                ViewState["FROM_CASE_INFO"] = value;
            }
        }

        private string DetailsKey
        {
            get
            {
                if (ViewState["DETAILS_KEY"] == null)
                    return "";
                else
                    return ViewState["DETAILS_KEY"].ToString();
            }
            set { ViewState["DETAILS_KEY"] = value; }
        }

        private Dictionary<string, DataTable> DetailsTableObject
        {
            get
            {
                if (ViewState["DTO"] == null)
                    ViewState["DTO"] = new Dictionary<string, DataTable>();
                return (Dictionary<string, DataTable>)ViewState["DTO"];
            }
            set { ViewState["DTO"] = value; }
        }

        private string SelectedActivityCodes
        {
            get
            {
                if (ViewState["SAC"] == null)
                    return "";
                else
                    return ViewState["SAC"].ToString();
            }
            set { ViewState["SAC"] = value; }
        }

        private string SelectedActivityKey
        {
            get
            {
                if (ViewState["SAK"] == null)
                    return "";
                else
                    return ViewState["SAK"].ToString();
            }
            set { ViewState["SAK"] = value; }
        }

        private bool CaseSearchScreen
        {
            get
            {
                if (ViewState["CSS"] == null)
                    return false;
                else
                    return (bool)ViewState["CSS"];
            }
            set { ViewState["CSS"] = value; }
        }

        private string AIActivityKey
        {
            get { return PLCSession.GetDefault("ACTLOG_AI_KEY"); }
            set { PLCSession.SetDefault("ACTLOG_AI_KEY", value); }
        }

        private bool ActivityLogGridExists
        {
            get
            {
                if (ViewState["ACTLOG_GRID_EXISTS"] == null)
                {
                    ViewState["ACTLOG_GRID_EXISTS"] = IsActivityLogGridExists();
                }

                return (bool)ViewState["ACTLOG_GRID_EXISTS"];
            }
            set
            {
                ViewState["ACTLOG_GRID_EXISTS"] = value;
            }
        }

        #endregion

        #region Events

        protected void Page_Init()
        {
            if (!IsPostBack)
            {
                if (Request.UrlReferrer != null && !ValidateFromCaseThenAttach())
                {
                    FromCaseInfo = Request.UrlReferrer.ToString().ToLower().Contains("tab1case.aspx");
                    PLCSession.SetDefault("ACTLOG_FROM_CASE", FromCaseInfo ? "T" : "F");
                }

                PLCButtonPanel1.SetCustomButtonVisible("Back To Case", FromCaseInfo);
                if (FromCaseInfo) PLCButtonPanel1.DisableButton("ADD");
            }

            LoadCtrls();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (PLCSession.GetLabCtrl("USES_ACTIVITY_LOG_CALENDAR") == "T")
                {
                    Menu1.Items.Add(new MenuItem("Activity Log Summary", "1"));
                }

                // Need to handle if page is from case tab > activity log > attachments then back to activity log
                if (Request.UrlReferrer != null && (Request.UrlReferrer.AbsolutePath.ToLower().Contains("tab1case.aspx") || 
                    (Request.UrlReferrer.AbsolutePath.ToLower().Contains("attachments.aspx") && !string.IsNullOrEmpty(PLCSession.GetDefault("UOREADONLY")))))
                    PLCSession.SetDefault("UOREADONLY", "RONLYCATAB");
                else
                    PLCSession.SetDefault("UOREADONLY", "");


                if (Request.UrlReferrer != null &&
                    ((Request.UrlReferrer.AbsolutePath.ToLower().Contains("actionitems.aspx") ||
                      Request.UrlReferrer.AbsolutePath.ToLower().Contains("attachments.aspx")) &&
                     (Session["AI_KEY"] != null || !string.IsNullOrEmpty(AIActivityKey))))
                {
                    if (Session["AI_KEY"] != null)
                    {
                        AIActivityKey = Session["AI_KEY"].ToString();
                        Session["AI_KEY"] = null;
                    }

                    PLCButtonPanel1.SetCustomButtonVisible("Back To Case", true);
                    PLCButtonPanel1.GetCustomButton("Back To Case").Width = Unit.Empty;
                    PLCButtonPanel1.SetCustomButtonText("Back To Case", "Back to Action Items");
                    pnlSearch.Visible = false;
                }
                else
                {
                    AIActivityKey = null;
                }

                //Calendar1.SelectedDate = DateTime.Now;
                //Calendar1.VisibleDate = DateTime.Now;
                //txtActivityLogFrom.Text = Calendar1.TodaysDate.ToString("MM/dd/yyyy");
                LoadActivityGrid();
                Session["DetailsDBPanelFieldsList"] = null;
                PLCSession.PLCGlobalAttachmentSource = "ACTIVITY";
                Menu1.Items[0].Selected = true;
            }
            else
            {
                GridView1.SetPLCGridProperties();

                MaintainCurrentTab();
                MaintainLinkCasePopup();
                RenderDTControls(hdnActCode.Value);
            }

            if ((GridView1.Rows.Count == 0 || GridView1.SelectedIndex < 0) && !AddNewActivityMode)
            {
                HideDetailsTabs();
            }

            if ((GridView1.SelectedDataKey != null) && (GridView1.SelectedDataKey["ACTIVITY_CODE"] != null))
                Session["ActivityMonitor_ACTIVITY_CODE"] = GridView1.SelectedDataKey["ACTIVITY_CODE"];
            else
                Session["ActivityMonitor_ACTIVITY_CODE"] = null;

            LoadActivityMonitorForm();
        }

        private void ReDrawDetailsPanel()
        {
            try
            {
                dbpDetails.ReDrawControls();
            }
            catch
            {
            }
        }

        private void RefreshDetailsOnEdit()
        {
            SetDetailsPanel();
            dbpDetails.DoEdit();
        }

        protected void chAnalyst_ValueChanged(object sender, EventArgs e)
        {
            // search for analyst in grid
            string sAnalyst = chAnalyst.GetValue();

            // remove mask underscore first
            while ((sAnalyst.Length > 0) && (sAnalyst[sAnalyst.Length - 1] == '_'))
                sAnalyst = sAnalyst.Remove(sAnalyst.Length - 1, 1);

            chAnalyst.SetPostbackRefreshLabel();
            LoadActivityGrid();
        }

        protected void txtActivityLog_TextChanged(object sender, EventArgs e)
        {
            //if (txtActivityLogFrom.Text.Length > 0)
            //{
            //    if (IsEnteredDateValid())
            //    {                    
            //        Calendar1.SelectedDate = DateTime.Parse(txtActivityLogFrom.Text);
            //        Calendar1.VisibleDate = DateTime.Parse(txtActivityLogFrom.Text); 
            //    }
            //}

            //LoadActivityGrid();
        }

        protected void GridView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["ActivityMonitor_ACTIVITY_CODE"] = GridView1.SelectedDataKey["ACTIVITY_CODE"];
            hdnActCode.Value = "0";
            GrabGridRecord();
        }

        protected void GridView1_PageIndexChanged(object sender, EventArgs e)
        {
            GridView1.SelectedIndex = 0;
            GrabGridRecord();
            ShowDailyActivityTab();
        }

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (!ActivityLogGridExists)
            {
                GridView1.Columns[GridView1.Columns.Count - 1].Visible = false;
                GridView1.Columns[GridView1.Columns.Count - 2].Visible = false;
            }
        }

        protected void dbpDetails_PLCDBPanelAddCustomFields(object sender, PLCDBPanelAddCustomFieldsEventArgs e)
        {
            if (!UsesDBPanel())
            {
                if ((GridView1.Rows.Count > 0 && GridView1.SelectedDataKey["ACTIVITY_CODE"] != null) || (AddNewActivityMode))
                {
                    //string tableName = "TV_ACT_" + (AddNewActivityMode ? dbpDetails.PLCDataTable.Replace("TV_ACT_", "") : GridView1.SelectedDataKey["ACTIVITY_CODE"].ToString()).Trim();
                    string tableName = dbpDetails.PLCDataTable;

                    if (tableName.Substring(tableName.Length - 1).Equals("_"))
                        return;

                    if (!CheckIfTableExist(tableName))
                        return;

                    PLCQuery qryDetailRecords = new PLCQuery(string.Format(QRY_ACTIVITY_DETAIL, "*", tableName,
                        AddNewActivityMode ? "0" : (GridView1.SelectedDataKey["ACTIVITY_KEY"] != null ? GridView1.SelectedDataKey["ACTIVITY_KEY"].ToString() : "0")));

                    if (qryDetailRecords.Open())
                    {
                        CreateCustomFields(tableName, qryDetailRecords, e);
                    }
                }
            }
        }

        protected void PLCButtonPanel1_PLCButtonClick(object sender, PLCCONTROLS.PLCButtonClickEventArgs e)
        {
            string activityCode = PLCDBPanel1.getpanelfield("ACTIVITY_CODE");

            if ((e.button_name == "ADD") && (e.button_action == "AFTER"))
            {
                PLCDBPanel1.setpanelfield("ANALYST", PLCSessionVars1.PLCGlobalAnalyst);
                PLCDBPanel1.setpanelfield("ACTIVITY_DATE", DateTime.Now.ToString("MM/dd/yyyy"));
                //PLCDBPanel1.setpanelfield("ACTIVITY_DATE", PLCDBPanel1.ConvertToFieldShortDateFormat("ACTIVITY_DATE", DateTime.Now));
                ClearLabels();
                SetLockMode(false);
                AddNewActivityMode = true;
                Activities.Clear();
                ShowDailyActivityTab();
                LoadMultipickOrigValues(false);
                SetNoAutoSortSelectedCodes(false);
                UserRestrictionOnEdit();
            }

            if ((e.button_name == "EDIT") && (e.button_action == "BEFORE"))
            {
                SetLockMode(false);
                lnkAttach.Enabled = false;
                EditActivityMode = true;
                Activities.Clear();
            }

            if ((e.button_name == "EDIT") && (e.button_action == "AFTER"))
            {
                ModifyActivities(PLCDBPanel1.getpanelfield("ACTIVITY_CODE"), "EDIT");
                dbpDetails.DoLoadRecord();

                if(!string.IsNullOrEmpty(hdnActCode.Value) && !hdnActCode.Value.Equals("0"))
                    AccessDetailsPanel(hdnActCode.Value, "EDIT");

                LoadMultipickOrigValues(true);
                UserRestrictionOnEdit();
            }

            if (e.button_name == "SAVE")
            {
                if (e.button_action == "BEFORE")
                {
                    if (Activities.Count <= 0)
                        ResetDetailsPanel();
                    
                    string actCode = string.Empty;
                    bool isValidForSaving = true;
                    SaveActivityToDictionary();

                    // check for required fields that are empty
                    foreach (string tableName in Activities.Keys)
                    {
                        actCode = tableName.Replace("TV_ACT_", "");
                        isValidForSaving = AccessDetailsPanel(actCode, "VALIDATE");
                        if (!isValidForSaving)
                        {
                            e.button_canceled = true;
                            hdnActCode.Value = actCode;
                            break;
                        }
                    }

                    if (!isValidForSaving)
                    {
                        AccessDetailsPanel(hdnActCode.Value, "EDIT");
                        dbpDetails.CanSave();
                        DisplayDetailsTab(false);
                    }
                }
                else if (e.button_action == "AFTER")
                {
                    SaveActivityToDictionary();
                    foreach (string tableName in Activities.Keys)
                    {
                        AccessDetailsPanel(tableName.Replace("TV_ACT_", ""), "SAVE");
                    }

                    // save added activity codes with details tables in the database
                    if (DetailsTableObject.Count() > 0)
                    {
                        SaveAllDTRecords();
                        SelectedActivityKey = "";
                    }
                    // clear data table that stores records with details table
                    DetailsTableObject.Clear();
                    ResetDTControls();

                    // Set the newly added data as the PLCGlobalAttachmentSourceKey base on PLCDBPanel where clause
                    // that is being set in PLCDBPanel GetNewRecord event. LoadActivityGrid will then select this key on the grid
                    PLCSession.PLCGlobalAttachmentSourceKey = PLCDBPanel1.PLCWhereClause.Replace("WHERE ACTIVITY_KEY =", "").Trim();

                    SetLockMode(true);
                    AddNewActivityMode = false;
                    EditActivityMode = false;
                    AfterPLCButtonClick = true;
                    lnkAttach.Enabled = true;
                    HideDetailsTabIfNoRecord();
                    Activities.Clear();
                    dbSearchActLog.ClearFields();
                    dbSearchActLog.setpanelfield("ANALYST",PLCSession.PLCGlobalAnalyst);
                    LoadActivityGrid();
                    SetNoAutoSortSelectedCodes(true);

                    if (!string.IsNullOrEmpty(hdnActCode.Value) && !hdnActCode.Value.Equals("0"))
                        AccessDetailsPanel(hdnActCode.Value, "LOAD");
                }
            }

            if ((e.button_name == "CANCEL") && (e.button_action == "AFTER"))
            {
                if (AddNewActivityMode)
                {
                    foreach (string tableName in Activities.Keys)
                    {
                        AccessDetailsPanel(tableName.Replace("TV_ACT_", ""), "DELETE");
                    }
                    Activities.Clear();
                    hdnActCode.Value = "0";
                }
                ResetDTControls();
                LoadActivityGrid();

                if (!string.IsNullOrEmpty(hdnActCode.Value) && !hdnActCode.Value.Equals("0"))
                    AccessDetailsPanel(hdnActCode.Value, "LOAD");

                string[] activityCodes = PLCDBPanel1.getpanelfield("ACTIVITY_CODE").Split(',');
                if (!activityCode.Contains(hdnActCode.Value))
                    hdnActCode.Value = "0";
                
                AddNewActivityMode = false;
                EditActivityMode = false;
                AfterPLCButtonClick = true;
                SetLockMode(true);
                lnkAttach.Enabled = true;
                // clear data table that stores records with details table
                DetailsTableObject.Clear();
                SetNoAutoSortSelectedCodes(true);
            }

            if ((e.button_name == "DELETE") && (e.button_action == "AFTER"))
            {
                foreach (string tableName in Activities.Keys)
                {
                    AccessDetailsPanel(tableName.Replace("TV_ACT_", ""), "DELETE");
                }

                //delete records with details table
                DeleteAllDTRecords(SelectedActivityCodes, SelectedActivityKey);

                // delete records in TV_ACTCASES that are linked in the deleted activity
                DeleteLinkedCaseRecords(SelectedActivityKey, "");

                GridView1.SelectedIndex = -1;

                LoadActivityGrid();
                SetLockMode(true);
                HideDetailsTabIfNoRecord();
                Activities.Clear();
                hdnActCode.Value = "0";
                ShowDailyActivityTab();
            }

            if (e.button_name == "Print")
            {
                PrintActivity();
                ReloadCurrentTab(hdnActCode.Value);
            }

            if (e.button_name.ToUpper() == "MONITOR")
            {
                ShowActivityMonitorForm();
            }

            if (e.button_name == "Back To Case")
            {
                Response.Redirect("~/Tab1Case.aspx");
            }

            if (e.button_name.Equals("Link Cases"))
            {
                LoadLinkedCases(SelectedActivityKey);
                LinkCaseEditMode(false);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "Link Case Popup", "ShowLinkCaseDialog();", true);
                ReloadCurrentTab(hdnActCode.Value);
            }

            if (e.button_name == "Approve" || e.button_name == "Reject")
            {
                UpdateQAIssue(e.button_name == "Approve" ? "Approved" : "Rejected");

                if (!string.IsNullOrEmpty(hdnActCode.Value) && !hdnActCode.Value.Equals("0"))
                    AccessDetailsPanel(hdnActCode.Value, "LOAD");
            }

            if (e.button_name == "Back to Action Items")
            {
                Response.Redirect("~/ActionItems.aspx");
            }
        }

        protected void PLCButton1_PLCDBPanelGetNewRecord(object sender, PLCDBPanelGetNewRecordEventArgs e)
        {
            int NewKey = PLCSessionVars1.GetNextSequence("ACTIVITY_SEQ");
            e.newWhereClause = "WHERE ACTIVITY_KEY = " + NewKey;
            e.NewRecordValues.Add("ACTIVITY_KEY", NewKey);
            SelectedActivityKey = NewKey.ToString();
        }

        protected void dbpDetails_PLCDBPanelGetNewRecord(object sender, PLCDBPanelGetNewRecordEventArgs e)
        {
            string activityKey = "";

            if(AddNewActivityMode)
                activityKey = PLCDBPanel1.PLCWhereClause.Substring(PLCDBPanel1.PLCWhereClause.IndexOf("=") + 1).Trim();
            else
                activityKey = GridView1.SelectedDataKey["ACTIVITY_KEY"].ToString();

            e.newWhereClause = " WHERE ACTIVITY_KEY = " + activityKey;
            e.NewRecordValues.Add("ACTIVITY_KEY", activityKey);
        }

        protected void btnCMEdit_Click(object sender, EventArgs e)
        {
            EnableActivityMonitorForEditing(true);
        }

        private void SaveActivityMonitor(string activityKey, string activityCode, string monitorKey, string answer)
        {
            int auditCode = -1;
            int auditSubCode = -1;
            PLCQuery qryCM = new PLCQuery("SELECT * FROM TV_ACT_" + activityCode + "_MONITOR WHERE ACTIVITY_KEY = " + activityKey + " AND SEQUENCE = " + monitorKey);
            qryCM.Open();
            if (qryCM.IsEmpty())
            {
                auditCode = 98;
                auditSubCode = 3;
                qryCM.Append();
                qryCM.SetFieldValue("ACTIVITY_KEY", activityKey);
            }
            else
            {
                auditCode = 98;
                auditSubCode = 5;
                qryCM.Edit();
            }

            qryCM.SetFieldValue("ACTIVITY_CODE", activityCode);
            qryCM.SetFieldValue("SEQUENCE", monitorKey);
            qryCM.SetFieldValue("ANSWER", answer);
            qryCM.Post("TV_ACT_" + activityCode + "_MONITOR", auditCode, auditSubCode);
        }

        protected void btnCMSave_Click(object sender, EventArgs e)
        {
            string activityKey = hdnActKey.Value;
            string activityCode = ReturnCourtString(GridView1.SelectedDataKey["ACTIVITY_CODE"].ToString());

            for (int i = 1; i < phActivityMonitor.Controls[0].Controls.Count; i = i + 2)
            {
                Control control = phActivityMonitor.Controls[0].Controls[i].Controls[1].Controls[0];
                switch (control.GetType().Name)
                {
                    case "TextBox":
                        SaveActivityMonitor(activityKey, activityCode, ((TextBox)control).Attributes["MONITOR_KEY"], ((TextBox)control).Text);
                        break;
                    case "FlexBox":
                        SaveActivityMonitor(activityKey, activityCode, ((FlexBox)control).Attributes["MONITOR_KEY"], ((FlexBox)control).SelectedText);
                        break;
                    case "RadioButtonList":
                        SaveActivityMonitor(activityKey, activityCode, ((RadioButtonList)control).Attributes["MONITOR_KEY"], ((RadioButtonList)control).SelectedValue);
                        break;
                }
            }

            EnableActivityMonitorForEditing(false);
        }

        protected void btnCMCancel_Click(object sender, EventArgs e)
        {
            PopulateActivityMonitorForm();
            EnableActivityMonitorForEditing(false);
        }

        protected void btnCMPrint_Click(object sender, EventArgs e)
        {
            PrintActivityMonitoring(GridView1.SelectedDataKey["ACTIVITY_CODE"].ToString());
        }

        protected void btnCMClose_Click(object sender, EventArgs e)
        {
            mvDetails.ActiveViewIndex = 0;
            ReloadCurrentTab(hdnActCode.Value);
        }

        protected void Menu1_MenuItemClick(object sender, MenuEventArgs e)
        {
            int selectedMenuIndex;
            int.TryParse(e.Item.Value, out selectedMenuIndex);

            Menu1.Items[selectedMenuIndex].Selected = true;

            switch (selectedMenuIndex)
            {
                case 1: // Summary
                    MultiView1.ActiveViewIndex = 1;
                    LoadCalendar();
                    break;
                default:
                    MultiView1.ActiveViewIndex = selectedMenuIndex;
                    ReloadCurrentTab(hdnActCode.Value);
                    break;
            }
        }

        protected void btnChangeMonth_Click(object sender, EventArgs e)
        {
            dpmActivity.StartDate = dpmPicker.StartDate;
            LoadCalendar();
        }

        protected void dpmActivity_EventClick(object sender, DayPilot.Web.Ui.Events.EventClickEventArgs e)
        {
            Dictionary<string, string> kvpList = new Dictionary<string, string>();
            kvpList.Add("ACTIVITY_KEY", e.Value);
            MultiView1.SetActiveView(View1);
            Menu1.Items[0].Selected = true;
            LoadActivityGrid();
            GridView1.SelectRowByDataKeyValues(kvpList);
            GrabGridRecord();
            ShowDailyActivityTab();
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            LoadActivityGrid();
            ShowDailyActivityTab();
        }

        protected void btnDetails_Click(object sender, EventArgs e)
        {
            // for partial postback trigger in codehead change of dbpDetails panel
            // do nothing
        }

        protected void PLCDBPanel1_PLCDBPanelCodeHeadChanged(object sender, PLCDBPanelCodeHeadChangedEventArgs e)
        {
            if (e.FieldName.Equals("ACTIVITY_CODE"))
            {
                string[] activityCodes = PLCDBPanel1.getpanelfield("ACTIVITY_CODE").Replace(" ", "").Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);

                if(AddNewActivityMode)
                    PLCDBPanel1.HideErrorMessage();
                CreateDetailsTabHeaders(PLCDBPanel1.getpanelfield("ACTIVITY_CODE"));    //create header for selected activity codes
                SaveActivityToDictionary();
                DeletePrevActCodes(activityCodes);  // delete the previously selected activity codes that are not included in the newly selected activities
                ModifyActivities(PLCDBPanel1.getpanelfield("ACTIVITY_CODE"), AddNewActivityMode ? "ADD":"EDIT");
            }
        }

        protected void lnkDetailsTab_Click(object sender, EventArgs e)
        {
            Session["DetailsDBPanelFieldsList"] = null;
            bool hasDetailsTable = HasDetailsTable(hdnActCode.Value);

            if (hasDetailsTable)
            {
                SetDetailsTableProperties(hdnActCode.Value);
                doPartialPostbackForRendering();
            }
            else
            {
                SaveActivityToDictionary();
                AccessDetailsPanel(hdnActCode.Value, (AddNewActivityMode || EditActivityMode) ? "EDIT" : "LOAD");
                ResetDTControls();
            }

            ToggleView(hdnActCode.Value);
            DisplayDetailsTab(hasDetailsTable);
        }

        /* Start of SubDetails Properties */

        protected void bplSubDetails_PLCButtonClick(object sender, PLCButtonClickEventArgs e)
        {
            // ADD
            if (e.button_name == "ADD")
            {
                if (e.button_action == "BEFORE")
                {
                    SetDTColumns(hdnActCode.Value, dbpSubDetails);
                }
                else if (e.button_action == "AFTER")
                {
                    SetDTControlsMode(false, (AddNewActivityMode || EditActivityMode));
                }
            }

            // EDIT
            else if (e.button_name == "EDIT")
            {
                if (e.button_action == "BEFORE")
                {
                    
                }
                else if (e.button_action == "AFTER")
                {
                    SetDTControlsMode(false, (AddNewActivityMode || EditActivityMode));
                }
            }

            // SAVE
            else if (e.button_name == "SAVE")
            {
                if (e.button_action == "BEFORE")
                {
                    // during add mode of the main button panel, records saved in details table are temporarily saved in data table.
                    if (AddNewActivityMode)
                    {
                        FetchDTFieldValues(hdnActCode.Value, dbpSubDetails, e.row_added);
                        e.button_canceled = true;
                        bplSubDetails.ClickCancelButton();
                    }
                }
                else if (e.button_action == "AFTER")
                {
                    // Records are saved in database during browse mode and edit mode of the main button panel
                    // if add mode is enabled in main button panel, records saved in Details table are temporarily saved in a data table. The user has a chance to cancel the adding the activity
                    if (e.row_added)
                        SaveToMasterTable(hdnActCode.Value, GridView1.SelectedDataKey["ACTIVITY_KEY"].ToString(), DetailsKey);

                    SetDetailsTableProperties(hdnActCode.Value);
                    grdSubDetails.SelectRowByDataKey(DetailsKey);
                    SetDTControlsMode(true, (AddNewActivityMode || EditActivityMode));
                }
            }

            // DELETE
            else if(e.button_name == "DELETE")
            {
                if (e.button_action == "BEFORE")
                {
                    if (AddNewActivityMode)
                    {
                        DeleteDTTempRecord(hdnActCode.Value, DetailsKey, dbpSubDetails);
                        e.button_canceled = true;
                        bplSubDetails.ClickCancelButton();
                    }
                }
                else if (e.button_action == "AFTER")
                {
                    DeleteRecordFromMT("TV_ACT_" + hdnActCode.Value, "DETAILS_KEY", DetailsKey);
                    SetDetailsTableProperties(hdnActCode.Value);
                    SetDTControlsMode(true, (AddNewActivityMode || EditActivityMode));
                }
            }

            // CANCEL
            else if (e.button_name == "CANCEL")
            {
                if (e.button_action == "BEFORE")
                {
                }
                else if (e.button_action == "AFTER")
                {
                    SetDetailsTableProperties(hdnActCode.Value);
                    SetDTControlsMode(true, (AddNewActivityMode || EditActivityMode));
                }
            }
        }

        protected void dbpSubDetails_PLCDBPanelGetNewRecord(object sender, PLCDBPanelGetNewRecordEventArgs e)
        {
            string detailsKey = DetailsKey = PLCSession.GetNextSequence("ACTIVITY_DET_SEQ").ToString();

            e.newWhereClause = "WHERE DETAILS_KEY = " + detailsKey;
            e.NewRecordValues.Add("DETAILS_KEY", detailsKey);
        }

        protected void grdSubDetails_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (AddNewActivityMode)
            {
                DisplayTempDTRecords(hdnActCode.Value);
                GrabDTTempRecord(hdnActCode.Value, dbpSubDetails, grdSubDetails.SelectedDataKey["DETAILS_KEY"].ToString());
            }
            else
                GrabDTRecord();
        }

        protected void grdSubDetails_Sorted(object sender, EventArgs e)
        {
            if (AddNewActivityMode)
                DisplayTempDTRecords(hdnActCode.Value);
            else
            {
                grdSubDetails.SelectedIndex = 0;
                GrabDTRecord();
            }
        }

        protected void grdSubDetails_PLCDBPanelAddCustomFields(object sender, PLCDBPanelAddCustomFieldsEventArgs e)
        {

            if (string.IsNullOrEmpty(dbpSubDetails.PLCPanelName) || IsDBPanelExists(dbpSubDetails.PLCPanelName)
                || dbpSubDetails.PLCPanelName.Equals("UNKNOWN"))
                return;

            string tableName = "TV_" + dbpSubDetails.PLCPanelName;
            string whereClause = "0=1";

            if (grdSubDetails.Rows.Count > 0 && grdSubDetails.SelectedIndex >= 0)
                whereClause = "DETAILS_KEY = " + grdSubDetails.SelectedDataKey["DETAILS_KEY"].ToString();

            PLCQuery qry = new PLCQuery();
            qry.SQL = "SELECT * FROM " + tableName + " WHERE " + whereClause;

            if (qry.Open())
            {
                CreateCustomFields(tableName, qry, e);
            }
        }

        protected void grdSubDetails_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (grdSubDetails.Columns.Count <= 0)
                return;
            int i = 0;
            foreach (DataControlField columnName in grdSubDetails.Columns)
            {
                if (columnName.ToString().Equals("DETAILS_KEY"))
                    break;
                else
                    i++;
            }

            if(i < grdSubDetails.Columns.Count)
                grdSubDetails.Columns[i].Visible = false;
        }
        /* End of SubDetails Properties */

        #region Link Case Properties
        /* Start of Link Case Properties */

        protected void btnLinkCaseClose_Click(object sender, EventArgs e)
        {
            //grdLinkCases.SetPLCGridProperties();
            //dbpLinkCases.EmptyMode();
            //ShowDailyActivityTab();
            //doPartialPostbackForRendering();
        }

        protected void btnLinkCaseAdd_Click(object sender, EventArgs e)
        {
            dbpLinkCases.DoAdd();
            LinkCaseEditMode(true);
        }

        protected void btnLinkCaseSave_Click(object sender, EventArgs e)
        {
            PLCQuery qryCheckLabCase = new PLCQuery();
            string caseKey = string.Empty;
            string whereClause = GetDBPanelWhereClause(dbpLinkCases);

            // 
            bool isCaseValidToLink = ValidateCaseToLink(SelectedActivityKey, false, out caseKey);

            //
            if (isCaseValidToLink)
            {
                if (caseKey == PLCSession.PLCGlobalCaseKey && PLCSession.GetDefault("ACTLOG_FROM_CASE") == "T")
                    PLCButtonPanel1.SetCustomButtonVisible("Back To Case", true);

                SaveLinkedCase(SelectedActivityKey, caseKey);
                LoadLinkedCases(SelectedActivityKey);
                LinkCaseEditMode(false);
                SelectLinkCaseRecord(SelectedActivityKey, caseKey);
                GrabLinkedCaseRecord(SelectedActivityKey, caseKey);
                lblLinkedCases.Text = DisplayLinkedCases(SelectedActivityKey);
            }
        }

        protected void btnLinkCaseCancel_Click(object sender, EventArgs e)
        {
            LoadLinkedCases(SelectedActivityKey);
            LinkCaseEditMode(false);
        }

        protected void btnLinkCaseDelete_Click(object sender, EventArgs e)
        {
            msgDeleteLinkCase.Show();
        }

        protected void msgPromptLinkCase_OkClick(object sender, EventArgs e)
        {
        }

        protected void DeleteLinkCase_YesClick(object sender, EventArgs e)
        {
            if (grdLinkCases.SelectedDataKey["CASE_KEY"].ToString() == PLCSession.PLCGlobalCaseKey && PLCSession.GetDefault("ACTLOG_FROM_CASE") == "T")
                PLCButtonPanel1.SetCustomButtonVisible("Back To Case", false);

            DeleteLinkedCaseRecords(grdLinkCases.SelectedDataKey["ACTIVITY_KEY"].ToString(), grdLinkCases.SelectedDataKey["CASE_KEY"].ToString());
            lblLinkedCases.Text = DisplayLinkedCases(SelectedActivityKey);
            msgPromptLinkCase.ShowMessage("Case Link", "Case has been unlinked and removed.", MessageBoxType.Information);
            LoadLinkedCases(SelectedActivityKey);
        }

        protected void CaseSearch_SelectedCaseKeyChanged(object sender, EventArgs e)
        {
            ScriptManager.RegisterStartupScript(this, this.GetType(), "Link Case Popup", "ShowLinkCaseDialog();", true);

            string caseKey = CaseSearch.SelectedCaseKey;
            bool isCaseValid = ValidateCaseToLink(SelectedActivityKey, true, out caseKey);

            if (isCaseValid)
            {
                SaveLinkedCase(SelectedActivityKey, caseKey);
                LoadLinkedCases(SelectedActivityKey);
                SelectLinkCaseRecord(SelectedActivityKey, caseKey);
                GrabLinkedCaseRecord(SelectedActivityKey, caseKey);
                lblLinkedCases.Text = DisplayLinkedCases(SelectedActivityKey);
                LinkCaseEditMode(false);

                msgPromptLinkCase.ShowMessage("Link Case", "Case successfully linked.", MessageBoxType.Information);
            }

            CaseSearch.ClearAll();
        }

        protected void grdLinkCases_SelectedIndexChanged(object sender, EventArgs e)
        {
            GrabLinkedCaseRecord(grdLinkCases.SelectedDataKey["ACTIVITY_KEY"].ToString(), grdLinkCases.SelectedDataKey["CASE_KEY"].ToString());
        }

        protected void grdLinkCases_Sorted(object sender, EventArgs e)
        {
            grdLinkCases.SelectedIndex = 0;
            GrabLinkedCaseRecord(grdLinkCases.SelectedDataKey["ACTIVITY_KEY"].ToString(), grdLinkCases.SelectedDataKey["CASE_KEY"].ToString());
        }

        protected void grdLinkCases_PageIndexChanged(object sender, EventArgs e)
        {
            grdLinkCases.SelectedIndex = 0;
            GrabLinkedCaseRecord(grdLinkCases.SelectedDataKey["ACTIVITY_KEY"].ToString(), grdLinkCases.SelectedDataKey["CASE_KEY"].ToString());
        }

        /* End of Link Case Properties */
        #endregion

        #endregion

        #region Methods

        private void doPartialPostbackForRendering()
        {
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "detailsCodeHead" + DateTime.Now, "document.getElementById('" + btnDetails.ClientID + "').click();", true);
        }

        private void ToggleView(string tab)
        {
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "detailsTab" + DateTime.Now, "toggleView('" + tab + "')", true);
        }

        private void LoadCtrls()
        {
            Control CtrlMsgBox = LoadControl("~/PLCWebCommon/PLCMsgBox.ascx");
            mbox = (PLCMsgBox)CtrlMsgBox;
            phMsgBox.Controls.Add(mbox);
            SetNoAutoSortSelectedCodes(true);

            if (ActivityKey == "" && !FromCaseInfo && string.IsNullOrEmpty(dbSearchActLog.getpanelfield("ANALYST")))
                dbSearchActLog.setpanelfield("ANALYST", PLCSessionVars1.PLCGlobalAnalyst);

            CheckUserRestriction();

            //
            ((Button)PLCButtonPanel1.FindControl("PLCButtonPanel1_DBP_Custom_Print")).Width = Unit.Pixel(90);
            ((Button)PLCButtonPanel1.FindControl("PLCButtonPanel1_DBP_Custom_Monitor")).Width = Unit.Pixel(90);

            PLCButtonPanel1.SetCustomButtonVisible("Monitor", false);

            //LoadActicityMonitorForm();
        }

        private void LoadActivityGrid()
        {
            GridView1.SetControlMode(true);

            if (ActivityLogGridExists)
            {
                LoadActivityViaDBGRID();
            }
            else
            {
                GridView1.PLCSQLString = GetActivitiesQuery();
                GridView1.InitializePLCDBGrid();
            }
            //
            if (GridView1.Rows.Count > 0)
            {
                if (ActivityKey != "")
                {
                    GridView1.SelectRowByDataKey(ActivityKey);
                    ActivityKey = null;
                }
                else if (PLCSession.PLCGlobalAttachmentSource == "ACTIVITY" && !string.IsNullOrEmpty(PLCSession.PLCGlobalAttachmentSourceKey))
                {
                    GridView1.SelectRowByDataKey(PLCSession.PLCGlobalAttachmentSourceKey);
                    //string activityKey = PLCSession.PLCGlobalAttachmentSourceKey;

                    //for (int page = 0; page < GridView1.PageCount; page++)
                    //{
                    //    GridView1.PageIndex = page;
                    //    GridView1.DataBind();

                    //    for (int i = 0; i < GridView1.DataKeys.Count; i++)
                    //    {
                    //        if (Convert.ToString(GridView1.DataKeys[i].Value) == activityKey)
                    //        {
                    //            GridView1.SelectedIndex = i;
                    //            //GridView1.PageIndex = page;

                    //            GrabGridRecord();

                    //            return;
                    //        }
                    //    }
                    //}
                }

                // fix to issue... when the previous search record is greater than the cuerrent search
                // and the selected index is greater the the search results count, an error will occur
                if (GridView1.SelectedIndex > (GridView1.Rows.Count - 1))
                {
                    SelectFirstRecord();
                }

                if ((GridView1.Rows.Count == 1 && GridView1.PageCount == 1) || GridView1.SelectedIndex == -1 ||
                    (GridView1.SelectedDataKey["ACTIVITY_KEY"] != null && PLCSession.PLCGlobalAttachmentSourceKey != GridView1.SelectedDataKey["ACTIVITY_KEY"].ToString()))
                {
                    SelectFirstRecord();
                }

                GrabGridRecord();
                Session["ActivityMonitor_ACTIVITY_CODE"] = GridView1.SelectedDataKey["ACTIVITY_CODE"];
            }
            else
            {
                PLCDBPanel1.EmptyMode();
                PLCButtonPanel1.SetEmptyMode();
                mvDetails.ActiveViewIndex = 0;
                plhAttachment.Controls.Clear();
                HideDetailsTabs();
                reptDetailsHeader.DataSource = null;
                reptDetailsHeader.DataBind();
                reptDetailsHeader.Controls.Clear();

                // Disable plcbuttonpanel and other plcbuttonpanel custom button for read only access
                if (PLCCommon.instance.IsReadOnlyAccess("WEBINQ," + PLCSession.GetDefault("UOREADONLY")))
                    PLCCommon.instance.SetReadOnlyAccess(PLCButtonPanel1);
            }
        }

        private void LoadCalendar()
        {
            DataRow dr;
            DataTable dtActivities = new DataTable();
            PLCQuery qryActivities = new PLCQuery(GetActivitiesQuery());

            dtActivities.Columns.Add("start", typeof(DateTime));
            dtActivities.Columns.Add("end", typeof(DateTime));
            dtActivities.Columns.Add("name", typeof(string));
            dtActivities.Columns.Add("id", typeof(string));
            dtActivities.Columns.Add("column", typeof(string));
            dtActivities.Columns.Add("allday", typeof(bool));
            dtActivities.Columns.Add("color", typeof(string));

            if (qryActivities.Open() && qryActivities.HasData())
            {
                while (!qryActivities.EOF())
                {
                    if (!string.IsNullOrEmpty(qryActivities.FieldByName("Activity Date")))
                    {
                        dr = dtActivities.NewRow();
                        dr["id"] = qryActivities.FieldByName("ACTIVITY_KEY");
                        dr["start"] = Convert.ToDateTime(qryActivities.FieldByName("Activity Date"));
                        dr["end"] = Convert.ToDateTime(qryActivities.FieldByName("Activity Date")).AddHours(23);
                        dr["name"] = qryActivities.FieldByName("ACTIVITY_CODE") + Environment.NewLine + qryActivities.FieldByName("Description");
                        //dr["column"] = "D";
                        dr["allday"] = true;
                        dtActivities.Rows.Add(dr);
                    }

                    qryActivities.Next();
                }
            }

            dpmActivity.DataSource = dtActivities;
            dpmActivity.DataBind();
        }

        private void GrabGridRecord()
        {
            mvDetails.ActiveViewIndex = 0;

            if (GridView1.Rows.Count > 0)
            {
                PLCDBPanel1.PLCWhereClause = "WHERE ACTIVITY_KEY = " + GridView1.SelectedDataKey["ACTIVITY_KEY"].ToString();
            }

            //SetDetailsPanel();

            PLCDBPanel1.DoLoadRecord();

            PLCDBPanel1.SetBrowseMode();

            PLCButtonPanel1.SetBrowseMode();

            SelectedActivityCodes = PLCDBPanel1.getpanelfield("ACTIVITY_CODE");
            SelectedActivityKey = GridView1.SelectedDataKey["ACTIVITY_KEY"].ToString();

            CreateDetailsTabHeaders(PLCDBPanel1.getpanelfield("ACTIVITY_CODE"));
            ModifyActivities(PLCDBPanel1.getpanelfield("ACTIVITY_CODE"), "LOAD");

            plhAttachment.Visible = GridView1.Rows.Count > 0;
            if (GridView1.Rows.Count > 0)
            {
                string activityKey = GridView1.SelectedDataKey["ACTIVITY_KEY"].ToString();
                bool hasAttachments = HasAttachments(activityKey);
                string activityLabel = (PLCDBPanel1.getpanelfield("ACTIVITY_CODE").IndexOf(',')) > -1 ? "ACTIVITIES: " : "ACTIVITY: ";
                string strAnalystName = string.Empty;
                string strLinkedCases = string.Empty;

                lnkAttach.ImageUrl = hasAttachments ? "~/Images/paperclip_withdoc.gif" : "~/Images/paperclip.gif";
                PLCSession.PLCGlobalAttachmentSourceKey = activityKey;

                strAnalystName = PLCSession.GetCodeDesc("ANALYST", PLCDBPanel1.getpanelfield("ANALYST"));
                strLinkedCases = DisplayLinkedCases(GridView1.SelectedDataKey["ACTIVITY_KEY"].ToString());

                PLCSession.PLCGlobalAttachmentSourceDesc = lblActivity.Text = GenerateActivityHeaderLabel(PLCDBPanel1.getpanelfield("ACTIVITY_CODE"), activityLabel, strAnalystName, PLCDBPanel1.ConvertToFieldShortDateFormat("ACTIVITY_DATE", PLCDBPanel1.getpanelfield("ACTIVITY_DATE")));

                if (strLinkedCases.Trim() != string.Empty)
                    lblLinkedCases.Text = string.Format("Linked Case(s): {0}", strLinkedCases);
                else
                    lblLinkedCases.Text = "";

            }
            else
            {
                plhAttachment.Controls.Clear();
            }

            ToggleQAIssueButtons();

            // Disable plcbuttonpanel and other plcbuttonpanel custom button for read only access
            if (PLCCommon.instance.IsReadOnlyAccess("WEBINQ," + PLCSession.GetDefault("UOREADONLY")))
                PLCCommon.instance.SetReadOnlyAccess(PLCButtonPanel1);
        }

        private bool HasAttachments(string activityKey)
        {
            if (!string.IsNullOrEmpty(activityKey))
            {
                PLCQuery qryAttachments = new PLCQuery("SELECT PRINTLOG_KEY FROM TV_PRINTLOG WHERE FILE_SOURCE = 'ACTIVITY' AND FILE_SOURCE_KEY1 = " + activityKey);
                qryAttachments.Open();
                if (!qryAttachments.IsEmpty())
                    return true;
            }

            return false;
        }

        private void SetDetailsPanel()
        {

            try
            {
                dbpDetails.Dispose();
                string activityCode = GridView1.SelectedDataKey["ACTIVITY_CODE"].ToString();
                if (GridView1.Rows.Count == 0 || string.IsNullOrEmpty(GridView1.SelectedDataKey["ACTIVITY_CODE"].ToString()))
                {
                    HideDetailsTabs();
                }
                else
                {
                    if (Session["ACTIVITY_CODE"] == null || activityCode != Session["ACTIVITY_CODE"].ToString())
                    {
                        Session["DetailsDBPanelFieldsList"] = null;
                        Session["ACTIVITY_CODE"] = activityCode;
                    }
                    dbpDetails.PLCPanelName = "ACT_" + activityCode;
                    dbpDetails.PLCDataTable = "TV_ACT_" + activityCode;
                    dbpDetails.PLCWhereClause = "WHERE ACTIVITY_KEY = " + GridView1.SelectedDataKey["ACTIVITY_KEY"].ToString();
                    dbpDetails.CreateControls();
                    dbpDetails.ClearFields();
                    dbpDetails.DoLoadRecord();
                }
                ShowTabByActivityCode(activityCode, false);
            }
            catch
            {
            }
        }

        private void CreateDetailsPanelOnAddMode()
        {
            if(!AddNewActivityMode)
                return;

            string activityCode = PLCDBPanel1.getpanelfield("ACTIVITY_CODE");

            if (activityCode.Equals(""))
            {
                HideDetailsTabs();
                return;
            }

            if (Session["ACTIVITY_CODE"] == null || activityCode != Session["ACTIVITY_CODE"].ToString())
            {
                Session["DetailsDBPanelFieldsList"] = null;
                Session["ACTIVITY_CODE"] = activityCode;
            }

            // delete the previous activity code selected when the user decides to change it to another one.
            if (!dbpDetails.PLCDataTable.Equals(""))
            {
                PLCQuery qryDeletePrevAct = new PLCQuery();
                qryDeletePrevAct.SQL = "SELECT * FROM " + dbpDetails.PLCDataTable + " " + PLCDBPanel1.PLCWhereClause;
                qryDeletePrevAct.Open();

                if(!qryDeletePrevAct.IsEmpty())
                {
                    dbpDetails.PLCWhereClause = PLCDBPanel1.PLCWhereClause;
                    if(dbpDetails.CanDelete())
                        dbpDetails.DoDelete();
                }
            }

            dbpDetails.PLCPanelName = "ACT_" + activityCode.Trim();
            dbpDetails.PLCDataTable = "TV_ACT_" + activityCode.Trim();
            dbpDetails.CreateControls();

            if (dbpDetails.CanAdd())
            {
                dbpDetails.DoAdd();
                dbpDetails.DoSave();
                dbpDetails.DoLoadRecord();
                dbpDetails.DoEdit();
            }

            ShowTabByActivityCode(activityCode, true);
        }

        private void SetLockMode(bool Lock)
        {
            btnFilter.Enabled = Lock;
            GridView1.SetControlMode(Lock);
            //dbSearchActLog.Enabled = Lock;
            Menu1.Enabled = Lock;

            if (Lock)
            {
                dbSearchActLog.SetEditMode();
            }
            else
            {
                dbSearchActLog.SetBrowseMode();
            }

            CheckUserRestriction();
        }

        private void PrintActivity()
        {
            if (GridView1.Rows.Count == 0)
            {
                mbox.ShowMsg("Print Activity", "You don''t have any Activity to print.", 1);
                return;
            }

            string reportName = "ACTIVITY.rpt";
            string activityCode = GridView1.SelectedDataKey["ACTIVITY_CODE"].ToString();

            if (!activityCode.Contains(","))
            {
                bool hasActivityCodeRpt = !string.IsNullOrEmpty(PLCSession.FindCrystalReport("ACT_" + activityCode + ".rpt"));
                if (hasActivityCodeRpt)
                    reportName = "ACT_" + activityCode + ".rpt";
            }

            string reportPath = PLCSession.FindCrystalReport(reportName);
            if (string.IsNullOrEmpty(reportPath))
            {
                mbox.ShowMsg("Print Activity", "Report not found.", 1);
                return;
            }

            //                
            PLCSessionVars1.PLCCrystalReportName = reportPath;
            PLCSessionVars1.PLCCrystalSelectionFormula = "{TV_ACTIVITY.ACTIVITY_KEY} = " + GridView1.SelectedDataKey["ACTIVITY_KEY"].ToString();
            PLCSessionVars1.PLCCrystalReportTitle = "Activity";
            PLCSessionVars1.PrintCRWReport(false);
        }

        private void HideDetailsTabs()
        {
            //tbDetails.Attributes["style"] = "display: none;";
            reptDetailsHeader.DataSource = null;
            reptDetailsHeader.DataBind();
            reptDetailsHeader.Controls.Clear();
        }

        private void ShowTabByActivityCode(string activityCode, bool isAddMode)
        {
            //tbDetails.Attributes["style"] = "";
            //hypDetails.InnerText = PLCSession.GetCodeDesc("ACTCODE", activityCode);

            if(!AddNewActivityMode)
                PLCButtonPanel1.SetCustomButtonVisible("Monitor", ShowActivityMonitorButton(activityCode, GridView1.SelectedDataKey["ACTIVITY_KEY"].ToString()));
        }

        private bool ShowActivityMonitorButton(string activityCodes, string activityKey)
        {
            string[] codes = activityCodes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            bool show = false;

            for (int i = 0; i < codes.Length; i++)
            {
                show = HasActivityMonitor(codes[i]);
                //by default, there are no restrictions to the MONITOR screen
                //but if RESTRICT_COURT_MONITORING is true, check user permissions
                if (codes[i].ToUpper() == "COURT" && PLCSession.GetLabCtrl("RESTRICT_COURT_MONITORING") == "T")
                {
                    show = PLCSession.CheckUserOption("MCRTSUP"); //show button if user has SUPERVISOR access

                    if (!show) //if not, check if user is the court examiner
                    {
                        PLCQuery qryCM = new PLCQuery();
                        string sql = string.Format(@"SELECT DT.ANALYST FROM TV_ACT_COURT_DET DT 
LEFT OUTER JOIN TV_ACT_COURT MT ON DT.DETAILS_KEY = MT.DETAILS_KEY 
WHERE MT.ACTIVITY_KEY  = {0}", activityKey);

                        qryCM.SQL = HasDetailsTable("COURT") ? sql : "SELECT ANALYST FROM TV_ACT_COURT WHERE ACTIVITY_KEY = " + activityKey;
                        qryCM.Open();
                        if (!qryCM.IsEmpty()) //show button if user is the court examiner
                            show = PLCSession.PLCGlobalAnalyst == qryCM.FieldByName("ANALYST");
                    }
                    else
                        return show;
                }
            }

            return show;
        }

        private bool HasActivityMonitor(string activityCode)
        {
            PLCQuery query = new PLCQuery();
            query.SQL = "SELECT * FROM TV_ACTIVITY_MONITOR WHERE ACTIVITY_CODE = '" + activityCode + "'";
            query.Open();
            if (query.HasData())
            {
                if (PLCSession.PLCDatabaseServer == "MSSQL")
                        query.SQL = "SELECT COUNT(*) FROM sysobjects WHERE sysobjects.name = 'TV_ACT_" + activityCode + "_MONITOR'";
                else
                    query.SQL = "SELECT COUNT(*) FROM all_objects WHERE object_type IN ('TABLE','VIEW') AND object_name = 'TV_ACT_" + activityCode + "_MONITOR'";
                query.Open();
                query.First();
                return (int.Parse(query.FieldByIndex(0)) > 0);
            }
            return false;
        }

        private bool HasDetailsData(string activityCode)
        {
            PLCQuery qryDetails = new PLCQuery("SELECT * FROM TV_ACT_" + activityCode +
                        " WHERE ACTIVITY_KEY = " + GridView1.SelectedDataKey["ACTIVITY_KEY"].ToString());

            return qryDetails.Open() && qryDetails.HasData();
        }

        private bool UsesDBPanel()
        {
            bool usesDBPanel = false;

            if (Session["DetailsDBPanelFieldsList"] != null)
            {
                usesDBPanel = true;
            }
            else
            {
                if (GridView1.SelectedDataKey != null || AddNewActivityMode)
                {
                    //string activityCode = AddNewActivityMode ? dbpDetails.PLCDataTable.Replace("TV_ACT_", "") : GridView1.SelectedDataKey["ACTIVITY_CODE"].ToString();
                    string activityCode = dbpDetails.PLCDataTable.Replace("TV_ACT_", "");

                    if (activityCode.Trim().Equals(""))
                        return usesDBPanel; ;
                    
                    PLCQuery qryCheckDBPanel = new PLCQuery(string.Format(QRY_CHECK_DBPANEL_EXISTS, activityCode.Trim()));

                    if (qryCheckDBPanel.Open() && qryCheckDBPanel.HasData())
                    {
                        List<string> rows = new List<string>();

                        foreach (DataRow row in qryCheckDBPanel.PLCDataTable.Rows)
                        {
                            if (row["EDIT_MASK"].ToString().ToUpper() == "MM/DD/YYYY" ||
                                row["EDIT_MASK"].ToString().ToUpper() == "DD/MM/YYYY")
                            {
                                if (PLCSession.PLCDatabaseServer == "MSSQL")
                                {
                                    rows.Add("CONVERT(varchar(10), " + row["FIELD_NAME"].ToString() + ", 101) AS " + row["FIELD_NAME"].ToString());
                                }
                                else
                                {
                                    rows.Add("to_char(" + row["FIELD_NAME"].ToString() + ", 'MM/DD/YYYY') AS " + row["FIELD_NAME"].ToString());
                                }
                            }
                            else
                            {
                                rows.Add(row["FIELD_NAME"].ToString());
                            }
                        }

                        if (!rows.Contains("ACTIVITY_KEY"))
                        {
                            rows.Insert(0, "ACTIVITY_KEY");
                        }

                        Session["DetailsDBPanelFieldsList"] = string.Join(",", (rows.Select(x => x).ToArray()));

                        usesDBPanel = true;
                    }
                }
            }
            return usesDBPanel;
        }

        private void LoadActivityMonitorForm()
        {
            if (Page.IsPostBack && Session["ActivityMonitor_ACTIVITY_CODE"] != null)
            {
                string activityCode = ReturnCourtString(Session["ActivityMonitor_ACTIVITY_CODE"].ToString());
                PLCQuery query = new PLCQuery();
                query.SQL = "SELECT * FROM TV_ACTIVITY_MONITOR WHERE ACTIVITY_CODE = '" + activityCode + "' ORDER BY SEQUENCE";
                query.OpenReadOnly();

                Table activityMonitor = new Table();
                activityMonitor.Width = Unit.Percentage(95);
                activityMonitor.CellPadding = 2;
                activityMonitor.CellSpacing = 2;
                activityMonitor.HorizontalAlign = HorizontalAlign.Center;
                int questionCtr = 0;
                while (!query.EOF())
                {
                    TableRow trQuestion = new TableRow();

                    TableCell tdQuestionNo = new TableCell();
                    tdQuestionNo.Width = Unit.Percentage(3);
                    tdQuestionNo.HorizontalAlign = HorizontalAlign.Right;
                    tdQuestionNo.VerticalAlign = VerticalAlign.Top;
                    tdQuestionNo.Text = ++questionCtr + ".";
                    trQuestion.Controls.Add(tdQuestionNo);

                    TableCell tdQuestion = new TableCell();
                    tdQuestion.Width = Unit.Percentage(97);
                    tdQuestion.VerticalAlign = VerticalAlign.Top;
                    tdQuestion.Text = query.FieldByName("QUESTION");
                    trQuestion.Controls.Add(tdQuestion);

                    TableRow trAnswer = new TableRow();
                    trAnswer.Controls.Add(new TableCell());

                    TableCell tdAnswerControl = new TableCell();
                    switch (query.FieldByName("QUESTION_TYPE"))
                    {
                        case "T":
                            TextBox tControl = new TextBox();
                            tControl.TextMode = TextBoxMode.MultiLine;
                            tControl.Rows = 3;
                            tControl.Width = Unit.Percentage(90);
                            tControl.Attributes.Add("MONITOR_KEY", query.FieldByName("SEQUENCE"));
                            tdAnswerControl.Controls.Add(tControl);
                            break;
                        case "P":
                            FlexBox pControl = new FlexBox();
                            List<string> pList = new List<string>();
                            string[] pValue = query.FieldByName("LIST_VALUE").Split(',');
                            pList.AddRange(pValue);
                            pControl.DataSource = pList;
                            pControl.DataBind();
                            pControl.Attributes.Add("MONITOR_KEY", query.FieldByName("SEQUENCE"));
                            tdAnswerControl.Controls.Add(pControl);
                            break;
                        case "R":
                            RadioButtonList rControl = new RadioButtonList();
                            string[] rList = query.FieldByName("LIST_VALUE").Split(',');
                            foreach (string val in rList)
                                rControl.Items.Add(val);
                            rControl.Attributes.Add("MONITOR_KEY", query.FieldByName("SEQUENCE"));
                            tdAnswerControl.Controls.Add(rControl);
                            break;
                    }
                    trAnswer.Controls.Add(tdAnswerControl);

                    activityMonitor.Controls.Add(trQuestion);
                    activityMonitor.Controls.Add(trAnswer);

                    query.Next();
                }
                phActivityMonitor.Controls.Add(activityMonitor);
            }
            else
            {
                // Load a temporary control
                Table tempTable = new Table();

                TableRow trTempRow = new TableRow();
                trTempRow.Controls.Add(new TableCell());

                TableCell trTempCell = new TableCell();
                    
                TextBox tempTBControl = new TextBox();
                trTempCell.Controls.Add(tempTBControl);

                trTempRow.Controls.Add(trTempCell);

                tempTable.Controls.Add(trTempRow);

                phActivityMonitor.Controls.Add(tempTable);
            }
        }

        private void ShowActivityMonitorForm()
        {
            mvDetails.ActiveViewIndex = 1;
            EnableActivityMonitorForEditing(false);
            PopulateActivityMonitorForm();

            if (PLCSession.GetLabCtrl("RESTRICT_COURT_MONITORING") == "T")
                btnCMEdit.Enabled = PLCSession.CheckUserOption("MCRTSUP"); //only allow user with SUPERVISOR access to edit the record

            // Disable controls in the page for read only access
            if (PLCCommon.instance.IsReadOnlyAccess("WEBINQ," + PLCSession.GetDefault("UOREADONLY")))
                btnCMEdit.Enabled = false;
        }

        private void PopulateActivityMonitorForm()
        {
            ClearActivityMonitor();

            string activityKey = hdnActKey.Value = GridView1.SelectedDataKey["ACTIVITY_KEY"].ToString();
            string activityCode = ReturnCourtString(GridView1.SelectedDataKey["ACTIVITY_CODE"].ToString());

            PLCQuery query = new PLCQuery();
            query.SQL = "SELECT * FROM TV_ACT_" + activityCode + "_MONITOR WHERE ACTIVITY_KEY = " + activityKey;
            query.OpenReadOnly();

            while (!query.EOF())
            {
                bool found = false;
                for (int i = 1; i < phActivityMonitor.Controls[0].Controls.Count; i = i + 2)
                {
                    Control control = phActivityMonitor.Controls[0].Controls[i].Controls[1].Controls[0];
                    switch (control.GetType().Name)
                    {
                        case "TextBox":
                            if (((TextBox)control).Attributes["MONITOR_KEY"] == query.FieldByName("SEQUENCE"))
                                ((TextBox)control).Text = query.FieldByName("ANSWER");
                            break;
                        case "FlexBox":
                            if (((FlexBox)control).Attributes["MONITOR_KEY"] == query.FieldByName("SEQUENCE"))
                                ((FlexBox)control).SelectedText = query.FieldByName("ANSWER");
                            break;
                        case "RadioButtonList":
                            if (((RadioButtonList)control).Attributes["MONITOR_KEY"] == query.FieldByName("SEQUENCE"))
                            {
                                string answer = query.FieldByName("ANSWER");
                                if (((RadioButtonList)control).Items.FindByText(answer) != null)
                                    ((RadioButtonList)control).Items.Cast<ListItem>().FirstOrDefault(a => a.Text == answer).Selected = true;
                            }
                            break;
                    }
                    if (found) break;
                }
                query.Next();
            }
        }

        private void EnableActivityMonitorForEditing(bool enable)
        {
            for (int i = 1; i < phActivityMonitor.Controls[0].Controls.Count; i = i + 2)
            {
                Control control = phActivityMonitor.Controls[0].Controls[i].Controls[1].Controls[0];
                switch (control.GetType().Name)
                {
                    case "TextBox":
                        ((TextBox)control).ReadOnly = !enable;
                        ((TextBox)control).BackColor = enable ? Color.White : Color.LightGray;
                        break;
                    case "FlexBox":
                        ((FlexBox)control).Enabled = enable;
                        break;
                    case "RadioButtonList":
                        ((RadioButtonList)control).Enabled = enable;
                        break;
                }
            }
            btnCMEdit.Enabled = btnCMPrint.Enabled = btnCMClose.Enabled = lnkAttach.Enabled = !enable;
            btnCMSave.Enabled = btnCMCancel.Enabled = enable;
            GridView1.SetControlMode(!enable);
            pnlSearch.Enabled = !enable;
        }

        private void ClearActivityMonitor()
        {
            for (int i = 1; i < phActivityMonitor.Controls[0].Controls.Count; i = i + 2)
            {
                Control control = phActivityMonitor.Controls[0].Controls[i].Controls[1].Controls[0];
                switch (control.GetType().Name)
                {
                    case "TextBox":
                        ((TextBox)control).Text = "";
                        break;
                    case "FlexBox":
                        ((FlexBox)control).SelectedValue = ((FlexBox)control).SelectedText = "";
                        break;
                    case "RadioButtonList":
                        ((RadioButtonList)control).SelectedIndex = -1;
                        break;
                }
            }
        }

        private string GetReportPath(string reportfilename)
        {
            return PLCSession.FindCrystalReport(reportfilename);
        }

        private void PrintActivityMonitoring(string activityCode)
        {
            activityCode = ReturnCourtString(activityCode);       
            string ReportPath = GetReportPath(activityCode.ToUpper() + "Monitoring");
            // If specialized report doesn't exist, use generic CHKLIST report instead.
            if (string.IsNullOrEmpty(ReportPath))
            {
                mbox.ShowMsg("Activity " + new CultureInfo("en-US", false).TextInfo.ToTitleCase(activityCode) + " Monitoring", "Report CourtMonitoring not found.", 1);
                return;
            }

            PLCSessionVars1.PLCCrystalReportName = ReportPath;
            PLCSessionVars1.PLCCrystalSelectionFormula = "{TV_ACT_" + activityCode.ToUpper() + "_MONITOR.ACTIVITY_KEY} = " + GridView1.SelectedDataKey.Values["ACTIVITY_KEY"].ToString();
            PLCSessionVars1.PLCCrystalReportTitle = "Activity " + new CultureInfo("en-US", false).TextInfo.ToTitleCase(activityCode) + " Monitoring";

            // Log the report filename and crystal report criteria.
            PLCSessionVars1.WriteDebug(String.Format("Report File: {0}, Selection Formula: {1}", PLCSessionVars1.PLCCrystalReportName, PLCSessionVars1.PLCCrystalSelectionFormula.Trim()), true);

            PLCSessionVars1.PrintCRWReport(true);
        }

        private string GetActivitiesQuery()
        {
            string Criteria = ActivityWhereClause(true);

            string SQL = string.Empty;

            if (PLCSessionVars1.PLCDatabaseServer == "MSSQL")
                SQL = "SELECT ANALYST AS \"Analyst\",ACTIVITY_CODE AS \"Activity Code\",DESCRIPTION AS \"Description\",CONVERT(CHAR,ACTIVITY_DATE,101) AS \"Activity Date\", ACTIVITY_KEY, ACTIVITY_CODE FROM TV_ACTIVITY"
                        + Criteria + " ORDER BY ACTIVITY_DATE DESC";
            else
                SQL = PLCSessionVars1.FormatSpecialFunctions("SELECT ANALYST AS \"Analyst\",ACTIVITY_CODE AS \"Activity Code\",DESCRIPTION AS \"Description\",FORMATDATE(ACTIVITY_DATE) AS \"Activity Date\", ACTIVITY_KEY, ACTIVITY_CODE FROM TV_ACTIVITY"
                        + Criteria + " ORDER BY ACTIVITY_DATE DESC");

            return SQL;
        }

        private void HideDetailsTabIfNoRecord()
        {
            if (GridView1.Rows.Count == 0 || GridView1.SelectedIndex < 0)
            {
                HideDetailsTabs();
                dbpDetails.EmptyMode();
                dbpDetails.PLCWhereClause = "";
            }
        }

        private void SelectFirstRecord()
        {
            GridView1.PageIndex = 0;
            GridView1.SelectedIndex = 0;
            GridView1.DataBind();
        }

        private bool CheckIfTableExist(string tableName)
        {
            try
            {
                if (tableName.Equals(""))
                    return true;
                PLCQuery qryCheckTable = new PLCQuery("SELECT COUNT(ACTIVITY_KEY) FROM " + tableName);
                return qryCheckTable.ExecSQL(string.Empty, true);
            }
            catch(Exception e)
            {
                return false;
            }
        }

        private void ClearLabels()
        {
            HideDetailsTabs();
            Activities.Clear();
            lnkAttach.Enabled = false;
            lnkAttach.ImageUrl = "~/Images/paperclip.gif";
            lblActivity.Text = string.Empty;
            lblLinkedCases.Text = string.Empty;
        }

        private void CreateDetailsTabHeaders(string activityCodes)
        {
            string[] actCodes = activityCodes.Split(',');
            List<ActivityCodes> repeaterData = new List<ActivityCodes>();
            string activityCode = string.Empty;

            // gather all the codes and save to the repeater control
            for (int i = 0; i < actCodes.Count(); i++)
            {
                activityCode = actCodes[i].ToString().Trim();
                if (CheckIfTableExist("TV_ACT_" + activityCode))
                    repeaterData.Add(new ActivityCodes { CODE = activityCode });
            }

            // bind data to the details panel header
            reptDetailsHeader.DataSource = repeaterData;
            reptDetailsHeader.DataBind();
        }

        // access chosen activity codes
        private void ModifyActivities(string activityCodes, string mode)
        {
            string[] actCodes = activityCodes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < actCodes.Length; i++)
            {
                AccessDetailsPanel(actCodes[i], mode);
            }
            ShowTabByActivityCode(activityCodes, false);
        }

        // save the currently loaded activity code to viewstate
        private void SaveActivityToDictionary()
        {
            string dbpActCode = dbpDetails.PLCDataTable;
            
            Activities.Remove(dbpActCode);

            Dictionary<string, object> activity = new Dictionary<string, object>();
            List<string> fields = dbpDetails.GetFieldNames();

            for (int i = 0; i < fields.Count; i++)
            {
                string mask = dbpDetails.GetFieldMask(fields[i]);
                if(mask.Equals("COMBOLIST"))
                    activity.Add(fields[i], dbpDetails.GetPanelDDL(fields[i]).SelectedValue);
                else
                    activity.Add(fields[i], dbpDetails.getpanelfield(fields[i]));
            }

            Activities.Add(dbpActCode, activity);
        }

        private bool AccessDetailsPanel(string activityCode, string mode)
        {
            if (string.IsNullOrEmpty(activityCode)
                || HasDetailsTable(activityCode))
                return true;

            if (!CheckIfTableExist("TV_ACT_" + activityCode))
                return true;
            
            Session["DetailsDBPanelFieldsList"] = null;
            
            dbpDetails.Dispose();
            dbpDetails.PLCPanelName = "ACT_" + activityCode.Trim();
            dbpDetails.PLCDataTable = "TV_ACT_" + activityCode.Trim();
            dbpDetails.ReDrawControls();
            dbpDetails.PLCWhereClause = PLCDBPanel1.PLCWhereClause;
            dbpDetails.DoLoadRecord();
            if(AddNewActivityMode || EditActivityMode)
                doPartialPostbackForRendering();

            if (mode.Equals("ADD"))
            {
                if (dbpDetails.CanAdd())
                {
                    LoadDetailsPanel(dbpDetails.PLCDataTable);
                    SaveActivityToDictionary();
                    dbpDetails.DoAdd();
                }
            }
            else if (mode.Equals("LOAD"))
            {
                SaveActivityToDictionary();
                dbpDetails.SetBrowseMode();
            }
            else if (mode.Equals("EDIT"))
            {
                LoadDetailsPanel(dbpDetails.PLCDataTable);
                SaveActivityToDictionary();
                if (dbpDetails.CanEdit())
                {
                    dbpDetails.DoEdit();
                }
            }
            else if (mode.Equals("SAVE"))
            {
                if (AddNewActivityMode
                    || (EditActivityMode && !IsRecordExisting(dbpDetails.PLCDataTable, dbpDetails.PLCWhereClause)))
                    dbpDetails.DoAdd();

                LoadDetailsPanel(dbpDetails.PLCDataTable);
                if (dbpDetails.CanSave())
                {
                    dbpDetails.DoSave();
                }
            }
            else if (mode.Equals("DELETE"))
            {
                if (dbpDetails.CanDelete())
                    dbpDetails.DoDelete();
            }
            else if (mode.Equals("VALIDATE"))
            {
                LoadDetailsPanel(dbpDetails.PLCDataTable);
                dbpDetails.DoEdit();
                if (!dbpDetails.CanSave())
                    return false;
            }
            return true;
        }

        private void LoadDetailsPanel(string tableName)
        {
            if (Activities.ContainsKey(tableName))
            {
                Dictionary<string, object> activity = Activities[tableName];
                foreach (string fieldname in activity.Keys)
                {
                    dbpDetails.setpanelfield(fieldname, activity[fieldname].ToString());
                }
            }
        }

        private void DeletePrevActCodes(string[] activityCodes)
        {
            List<string> removedActivities = new List<string>();

            // removing records in activity code with no details table
            removedActivities = (from table in Activities.Keys
                                     where !activityCodes.Contains(table.Replace("TV_ACT_", ""))
                                     select table).ToList();

            foreach (string activityCode in removedActivities)
            {
                Activities.Remove(activityCode);
            }

            // removing records in activity code with details table
            removedActivities = (from table in DetailsTableObject.Keys
                                 where !activityCodes.Contains(table.Replace("TV_ACT_", ""))
                                 select table).ToList();

            foreach (string activityCode in removedActivities)
            {
                DetailsTableObject.Remove(activityCode);
            }

        }

        private void ShowDailyActivityTab()
        {
            hdnActCode.Value = "0";
            ToggleView(hdnActCode.Value);
        }

        private void MaintainCurrentTab()
        {          
            if (EditActivityMode || AddNewActivityMode || AfterPLCButtonClick)
            {
                ReDrawDetailsPanel();
                LoadDetailsPanel("TV_ACT_" + hdnActCode.Value);
                if (!string.IsNullOrEmpty(hdnActCode.Value) && Activities.Keys.Contains("TV_ACT_" + hdnActCode.Value))
                    ToggleView(hdnActCode.Value);
                AfterPLCButtonClick = false;
            }
        }

        private void ReloadCurrentTab(string activityCode)
        {
            if (!string.IsNullOrEmpty(activityCode) && !activityCode.Equals("0"))
                AccessDetailsPanel(hdnActCode.Value, "LOAD");
        }

        private string GetCaseKey(string labCase)
        {
            if (string.IsNullOrEmpty(labCase))
                return string.Empty;
            PLCQuery qryLabCase = new PLCQuery();
            qryLabCase.SQL = string.Format("SELECT CASE_KEY FROM TV_LABCASE WHERE LAB_CASE = '{0}'", labCase);
            qryLabCase.Open();

            if(!qryLabCase.IsEmpty())
                return qryLabCase.FieldByName("CASE_KEY");

            return string.Empty;
        }

        private string ReturnCourtString(string activityCode)
        {
            string[] codes = activityCode.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (codes.Contains("COURT"))
                return "COURT";
            return activityCode;
        }

        private bool IsRecordExisting(string tableName, string whereClause)
        {
            PLCQuery qryCheckRecord = new PLCQuery();
            qryCheckRecord.SQL = string.Format("SELECT * FROM {0} {1}", tableName, whereClause);
            qryCheckRecord.Open();

            return qryCheckRecord.HasData();
        }

        private string LoadMultipickOrigValues(bool load)
        {
            string mpOriginalValues = "";
            if (PLCDBPanel1.GetFieldMask("ACTIVITY_CODE").Equals("MULTIPICK") || PLCDBPanel1.GetFieldMask("ACTIVITY_CODE").Equals("MULTIPICK_SEARCH"))
            {
                CodeMultiPick cmActivityCode = PLCDBPanel1.GetPanelRecByFieldName("ACTIVITY_CODE").chMultiLookup;
                cmActivityCode.OriginalValues = mpOriginalValues = load ? cmActivityCode.GetSelectedSectionsCsv() : "";
            }
            return mpOriginalValues;
        }

        private void SetNoAutoSortSelectedCodes(bool noAuto)
        {
            if (PLCDBPanel1.GetFieldMask("ACTIVITY_CODE").Equals("MULTIPICK") || PLCDBPanel1.GetFieldMask("ACTIVITY_CODE").Equals("MULTIPICK_SEARCH"))
            {
                CodeMultiPick cmActivityCode = PLCDBPanel1.GetPanelRecByFieldName("ACTIVITY_CODE").chMultiLookup;
                cmActivityCode.NoAutoSortSelectedCodes = noAuto;
            }
        }

        private void ResetDetailsPanel()
        {
            dbpDetails.PLCPanelName = "";
            dbpDetails.PLCDataTable = "";
            ReDrawDetailsPanel();
            dbpDetails.EmptyMode();
        }

        private string GetOriginalActivityCodes(string whereClause)
        {
            PLCQuery qryActivityCodes = new PLCQuery();
            qryActivityCodes.SQL = string.Format("SELECT ACTIVITY_CODE FROM TV_ACTIVITY {0}", whereClause);
            qryActivityCodes.Open();

            return (!qryActivityCodes.IsEmpty() ? qryActivityCodes.FieldByName("ACTIVITY_CODE") : "");
        }

        private void CreateCustomFields(string tableName, PLCQuery qry, PLCDBPanelAddCustomFieldsEventArgs e)
        {
            foreach (DataColumn dc in qry.PLCDataTable.Columns)
            {
                switch (dc.ColumnName)
                {
                    case "ROWID":
                    case "ACTIVITY_KEY":
                    case "DETAILS_KEY":
                        break;
                    default:
                        DataRow dr;
                        dr = e.customfields.Rows.Add();

                        string editMask = "";
                        int maxLength = dc.MaxLength == -1 ? 10 : dc.MaxLength;

                        // Arbitrary value to preempt maxlength issue
                        if (maxLength > 2000)
                        {
                            maxLength = 2000;
                        }

                        if (dc.DataType == typeof(DateTime))
                        {
                            //editMask = "99/99/9999";
                            editMask = PLCSessionVars1.GetDateFormat();
                        }
                        else
                        {
                            if (maxLength > 50)
                            {
                                dr["MEMO_FIELD_LINES"] = "2";
                            }
                            else
                            {
                                dr["MEMO_FIELD_LINES"] = "0";
                                editMask = new string('X', maxLength);
                            }
                        }

                        dr["TABLE_NAME"] = tableName;
                        dr["FIELD_NAME"] = dc.ColumnName;
                        dr["PROMPT"] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dc.ColumnName.Replace('_', ' ').ToLower());
                        dr["EDIT_MASK"] = editMask;
                        dr["LENGTH"] = maxLength;
                        dr["MANDATORY"] = "F";
                        if (qry.HasData())
                        {
                            dr["INITIAL_VALUE"] = qry.FieldByName(dc.ColumnName);
                        }
                        break;
                }
            }
        }

        private bool IsDBPanelExists(string panelName)
        {
            PLCQuery qry = new PLCQuery(String.Format("SELECT * FROM TV_DBPANEL WHERE PANEL_NAME = '{0}'", panelName));
            return qry.Open() && qry.HasData();
        }

        private bool ValidateFromCaseThenAttach()
        {
            string actLogFromCase = PLCSession.GetDefault("ACTLOG_FROM_CASE");
            bool fromAttach = Request.UrlReferrer.ToString().ToLower().Contains("attachments.aspx");

            FromCaseInfo = actLogFromCase.Equals("T") && fromAttach;

            if (!fromAttach) PLCSession.SetDefault("ACTLOG_FROM_CASE", null);

            return actLogFromCase.Equals("T") && fromAttach;
        }

        #region Details Table Methods
        /*Start of Details Table Methods*/

        public bool HasDetailsTable(string activityCode)
        {
            PLCQuery qryActivityDetails = new PLCQuery();
            qryActivityDetails.SQL = "SELECT DETAILS_TABLE FROM TV_ACTCODE WHERE ACTIVITY_CODE = '" + activityCode + "'";
            qryActivityDetails.Open();

            return (qryActivityDetails.HasData() && qryActivityDetails.FieldByName("DETAILS_TABLE").Equals("T"));
        }

        public void DisplayDetailsTab(bool withDetailsTable)
        {
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "detailsType" + DateTime.Now, "ShowDetailsTabType(" + withDetailsTable.ToString().ToLower() + ");", true);
        }

        private void InitDTColumns(string activityCode)
        {
            //check if DBGRIDHD exists
            if (IsDBGridHDExists(activityCode))
                grdSubDetails.PLCGridName = activityCode;
            else
            {
                List<string> fieldNames = dbpSubDetails.GetPanelFieldNames().ToList<string>();
                grdSubDetails.PLCSQLString = "SELECT MT.DETAILS_KEY, " + string.Join(",", fieldNames) + " FROM TV_" + activityCode;
                grdSubDetails.PLCGridName = "";
            }

            grdSubDetails.PLCSQLString_AdditionalFrom = string.Format("LEFT OUTER JOIN TV_{0} MT ON TV_{1}.DETAILS_KEY = MT.DETAILS_KEY", activityCode.Replace("_DET", ""), activityCode);
            grdSubDetails.PLCSQLString_AdditionalCriteria = "0=1";            
            grdSubDetails.InitializePLCDBGrid();
        }

        private void FilterDTRecords(string activityCode, string activityKey)
        {
            if (!IsDBGridHDExists(activityCode))
            {
                List<string> fieldNames = dbpSubDetails.GetPanelFieldNames().ToList<string>();
                grdSubDetails.PLCSQLString = "SELECT MT.DETAILS_KEY, " + string.Join(",", fieldNames) + " FROM TV_" + activityCode;
            }
            grdSubDetails.PLCSQLString_AdditionalCriteria = string.Format("MT.ACTIVITY_KEY = " + activityKey);
            grdSubDetails.InitializePLCDBGrid();
        }

        private void SetDetailsTableProperties(string activityCode)
        {
            string actCodeName = string.IsNullOrEmpty(activityCode) ? "" : string.Format("ACT_{0}_DET", activityCode);
            bool temporaryRecords = false;

            // create dbpanel
            dbpSubDetails.PLCPanelName = actCodeName;
            dbpSubDetails.PLCDataTable = "TV_" + actCodeName;
            dbpSubDetails.CreateControls();

            InitDTColumns(actCodeName);

            // create gridview
            if (!AddNewActivityMode)
            {
                FilterDTRecords(actCodeName, GridView1.SelectedDataKey["ACTIVITY_KEY"].ToString());
            }
            else if (AddNewActivityMode && DetailsTableObject.Count() > 0)
            {               
                DisplayTempDTRecords(hdnActCode.Value);
                temporaryRecords = true;
            }

            // 
            grdSubDetails.AllowSorting = !temporaryRecords;

            int rowCount = grdSubDetails.Rows.Count;
            int selectedIndex = grdSubDetails.SelectedIndex;

            if (rowCount > 0)
            {
                if (selectedIndex < 0 || selectedIndex >= rowCount)
                    grdSubDetails.SelectedIndex = 0;

                if (!string.IsNullOrEmpty(DetailsKey))
                    grdSubDetails.SelectRowByDataKey(DetailsKey);

                dbpSubDetails.SetBrowseMode();
                bplSubDetails.SetBrowseMode();

                if(!temporaryRecords)
                    GrabDTRecord();
                else
                    GrabDTTempRecord(hdnActCode.Value, dbpSubDetails, grdSubDetails.SelectedDataKey["DETAILS_KEY"].ToString());
            }
            else
            {
                grdSubDetails.EmptyDataText = "No records found.";
                dbpSubDetails.EmptyMode();
                bplSubDetails.SetEmptyMode();
            }
        }

        private void GrabDTRecord()
        {
            string detailsKey = DetailsKey = grdSubDetails.SelectedDataKey["DETAILS_KEY"].ToString();
            if (grdSubDetails.Rows.Count <= 0 && grdSubDetails.SelectedIndex < 0)
                return;
            dbpSubDetails.PLCWhereClause = "WHERE DETAILS_KEY = " + (string.IsNullOrEmpty(detailsKey) ? "0": detailsKey);
            dbpSubDetails.DoLoadRecord();
        }

        private void SaveToMasterTable(string activityCode, string activityKey, string detailsKey)
        {
            if (string.IsNullOrEmpty(activityCode))
                return;

            string tableName = "TV_ACT_" + activityCode;

            PLCQuery qrySaveRecord = new PLCQuery();
            qrySaveRecord.SQL = string.Format("SELECT * FROM {0} WHERE ACTIVITY_KEY = {1} AND DETAILS_KEY = {2}", tableName, activityKey, detailsKey);
            qrySaveRecord.Open();

            if (qrySaveRecord.IsEmpty())
            {
                qrySaveRecord.Append();
                qrySaveRecord.SetFieldValue("ACTIVITY_KEY", activityKey);
                qrySaveRecord.SetFieldValue("DETAILS_KEY", detailsKey);
                qrySaveRecord.Post(tableName, 98, 3);
            }
        }

        private void SetDTColumns(string activityCode, DBPanel panel)
        {
            if (!DetailsTableObject.ContainsKey(activityCode))
                DetailsTableObject.Add(activityCode, new DataTable());
            else
                return;
            
            List<string> fieldNames = panel.GetPanelFieldNames().ToList<string>();

            // declare data column and data row
            DataColumn column;

            //create column for primary key
            DetailsTableObject[activityCode].Columns.Add("DETAILS_KEY");

            // add columns
            foreach (string field in fieldNames)
            {
                column = new DataColumn();
                column.ColumnName = field;
                if(!DetailsTableObject[activityCode].Columns.Contains(field))
                    DetailsTableObject[activityCode].Columns.Add(column);
            }
        }

        private void FetchDTFieldValues(string activityCode, DBPanel panel, bool newRecord)
        {
            if (!DetailsTableObject.ContainsKey(activityCode))
                return;

            List<string> fieldNames = panel.GetPanelFieldNames().ToList<string>();

            DataRow row = (newRecord) ? DetailsTableObject[activityCode].NewRow()
                : (from result in DetailsTableObject[activityCode].AsEnumerable()
                   where result.Field<string>("DETAILS_KEY").Equals(grdSubDetails.SelectedDataKey["DETAILS_KEY"].ToString())
                   select result).Cast<DataRow>().FirstOrDefault();

            int tempdetailsKey = 0;

            // get and assign a temporary detail key
            if (DetailsTableObject[activityCode].Rows.Count > 0)
            {
                DataRow lastRow = DetailsTableObject[activityCode].Rows[DetailsTableObject[activityCode].Rows.Count - 1];
                tempdetailsKey = Convert.ToInt32(lastRow["DETAILS_KEY"]) + 1;
            }

            if(newRecord)
                row["DETAILS_KEY"] = DetailsKey = tempdetailsKey.ToString();

            foreach (string field in fieldNames)
            {
                row[field] = panel.getpanelfield(field);
            }

            if(newRecord)
                DetailsTableObject[activityCode].Rows.Add(row);
        }

        private void GrabDTTempRecord(string activityCode, DBPanel panel, string detailsKey)
        {
            if (!DetailsTableObject.ContainsKey(activityCode))
                return;

            DataRow row = (from result in DetailsTableObject[activityCode].AsEnumerable()
                          where result.Field<string>("DETAILS_KEY").Equals(detailsKey)
                          select result).Cast<DataRow>().FirstOrDefault();

            List<string> fieldNames = panel.GetFieldNames().ToList<string>();
            foreach (string field in fieldNames)
            {
                panel.setpanelfield(field, row[field].ToString());
            }
            panel.SetBrowseMode();
            grdSubDetails.SelectRowByDataKey(detailsKey);
        }

        private void DisplayTempDTRecords(string activityCode)
        {
            if (!DetailsTableObject.ContainsKey(activityCode))
                return;

            DataView view = new DataView(DetailsTableObject[activityCode]);
            grdSubDetails.DataSource = view;
            grdSubDetails.DataBind();
        }

        private void SaveAllDTRecords()
        {
            foreach (string activityCode in DetailsTableObject.Keys)
            {
                SaveDTRecord(activityCode, DetailsTableObject[activityCode]);
            }
        }

        private void SaveDTRecord(string activityCode, DataTable recordsTable)
        {
            List<string> columns = (from DataColumn dc in recordsTable.Columns
                                        select dc.ColumnName).ToList<string>();

            foreach (DataRow dr in recordsTable.Rows)
            {

                PLCQuery qrySave = new PLCQuery();
                string detailsKey = PLCSession.GetNextSequence("ACTIVITY_DET_SEQ").ToString();

                qrySave.SQL = "SELECT * FROM TV_ACT_" + activityCode + "_DET WHERE DETAILS_KEY = " + detailsKey;
                qrySave.Open();

                if (qrySave.IsEmpty())
                {
                    qrySave.Append();
                    qrySave.AddParameter("DETAILS_KEY", detailsKey);
                }
                else
                {
                    qrySave.Edit();
                }

                foreach (string column in columns)
                {
                    if(!column.Equals("DETAILS_KEY") && !string.IsNullOrEmpty(dr[column].ToString()))
                        qrySave.AddParameter(column, dr[column]);
                }

                qrySave.Save("TV_ACT_" + activityCode + "_DET", 98, 3);

                // save the record to the master table
                SaveToMasterTable(activityCode, SelectedActivityKey, detailsKey);
            }
        }

        private void DeleteAllDTRecords(string selectedActCodes, string activityKey)
        {
            string[] activityCodes = selectedActCodes.Replace(" ", "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string activityCode in activityCodes)
            {
                if(HasDetailsTable(activityCode))
                {
                    DeleteDTRecords(activityCode, activityKey);
                }
            }
        }

        private void DeleteDTRecords(string activityCode, string activityKey)
        {
            //delete records in details table
            PLCQuery qryDTRecords = new PLCQuery();
            qryDTRecords.SQL = string.Format("SELECT DETAILS_KEY FROM TV_ACT_{0} WHERE ACTIVITY_KEY = {1}", activityCode, activityKey);
            if (qryDTRecords.Open() && qryDTRecords.HasData())
            {
                while (!qryDTRecords.EOF())
                {
                    PLCQuery qryDeleteDT = new PLCQuery();
                    qryDeleteDT.Delete("TV_ACT_" + activityCode + "_DET", "WHERE DETAILS_KEY = " + qryDTRecords.FieldByName("DETAILS_KEY"), 98, 1);
                    qryDTRecords.Next();
                }
            }

            //delete records in master table using activity key
            DeleteRecordFromMT("TV_ACT_" + activityCode, "ACTIVITY_KEY", activityKey);
        }

        private void DeleteRecordFromMT(string tableName, string fieldName, string key)
        {
            //delete record in Master table 
            PLCQuery qryDeleteMT = new PLCQuery();
            qryDeleteMT.Delete(tableName, string.Format("WHERE {0} = {1}", fieldName, key ), 98, 1);
        }

        private bool IsDBGridHDExists(string activityCode)
        {
            //check if DBGRIDHD exists
            PLCQuery qryCheckGridHD = new PLCQuery();
            qryCheckGridHD.SQL = "SELECT * FROM TV_DBGRIDHD WHERE GRID_NAME = '" + activityCode + "'";
            qryCheckGridHD.Open();

            return (qryCheckGridHD.HasData() && qryCheckGridHD.HasData());
        }

        private void DeleteDTTempRecord(string activityCode, string detailsKey, DBPanel panel)
        {
            if (!DetailsTableObject.ContainsKey(activityCode))
                return;

            DataRow row = (from result in DetailsTableObject[activityCode].AsEnumerable()
                           where result.Field<string>("DETAILS_KEY").Equals(detailsKey)
                           select result).Cast<DataRow>().FirstOrDefault();
            row.Delete();
        }

        private void SetDTControlsMode(bool enable, bool isMainInEdit)
        {
            SetLockMode(enable && !isMainInEdit);
            grdSubDetails.SetControlMode(enable);
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "headers" + DateTime.Now, "EnableHeaders(" + enable.ToString().ToLower() + ");", true);
            foreach (RepeaterItem item in reptDetailsHeader.Items)
            {
                ((LinkButton)item.FindControl("lnkDetailsTab")).Enabled = enable;
            }
        }

        private void RenderDTControls(string activityCode)
        {
            bool hasDetailsTable = HasDetailsTable(activityCode);

            if (hasDetailsTable)
            {
                dbpSubDetails.ReDrawControls();
                DisplayDetailsTab(hasDetailsTable);
            }
        }

        private void ResetDTControls()
        {
            grdSubDetails.SetPLCGridProperties();
            dbpSubDetails.PLCPanelName = "";
            dbpSubDetails.CreateControls();
            dbpSubDetails.EmptyMode();
        }

        /* End of Details Table Methods */
        #endregion

        #region Link Case Methods

        private void LoadLinkedCases(string activityKey)
        {
            PLCHelperFunctions HelperFunctions = new PLCHelperFunctions();
            Dictionary<string, OrderedDictionary> tables = HelperFunctions.GetTablesAndAlias(grdLinkCases.PLCGridName);

            grdLinkCases.PLCSQLString_AdditionalCriteria = string.Format("{0}.ACTIVITY_KEY = {1}", tables["TV_ACTCASES"][0], activityKey);
            grdLinkCases.InitializePLCDBGrid();

            if (grdLinkCases.Rows.Count > 0)
            {
                grdLinkCases.PageIndex = 0;
                grdLinkCases.SelectedIndex = 0;
                GrabLinkedCaseRecord(grdLinkCases.SelectedDataKey["ACTIVITY_KEY"].ToString(), grdLinkCases.SelectedDataKey["CASE_KEY"].ToString());
            }
            else
            {
                dbpLinkCases.EmptyMode();
                btnLinkCaseDelete.Enabled = false;
            }
        }

        private void GrabLinkedCaseRecord(string activityKey, string casekey)
        {
            dbpLinkCases.PLCWhereClause = "WHERE CASE_KEY = " + casekey;
            dbpLinkCases.DoLoadRecord();
        }

        private void LinkCaseEditMode(bool editMode)
        {
            dbpLinkCases.HideErrorMessage();
            grdLinkCases.Enabled = !editMode;
            btnLinkCaseAdd.Enabled = btnLinkCaseDelete.Enabled = btnCaseSearch.Enabled = !editMode;
            btnLinkCaseSave.Enabled = btnLinkCaseCancel.Enabled = editMode;

            if (editMode)
            {
                dbpLinkCases.EditMode();
            }
            else
            {
                dbpLinkCases.SetBrowseMode();
                btnLinkCaseDelete.Enabled = (grdLinkCases.Rows.Count > 0);
            }

            // Disable controls in the page for read only access
            if (PLCCommon.instance.IsReadOnlyAccess("WEBINQ," + PLCSession.GetDefault("UOREADONLY")))
            {
                btnLinkCaseAdd.Enabled = false;
                btnLinkCaseDelete.Enabled = false;
                btnCaseSearch.Enabled = false;
            }
        }

        private string GetDBPanelWhereClause(DBPanel panel)
        {
            List<string> fields = dbpLinkCases.GetFieldNames().ToList<string>();
            Dictionary<string, string> table = new Dictionary<string, string>();
            string whereClause = string.Empty;
            table.Add(panel.PLCDataTable, panel.PLCDataTable);

            foreach (string fieldname in fields)
            {
                string fieldValue = panel.getpanelfield(fieldname);
                if (!string.IsNullOrEmpty(fieldValue))
                {
                    whereClause += PLCGlobals.PLCCommon.instance.GetWhereClauseByType(panel.PLCDataTable, fieldname, panel.GetFieldPrompt(fieldname),
                        panel.GetFieldMask(fieldname), panel.GetFieldCodeTable(fieldname), panel.getpanelfield(fieldname), table, false);
                }
            }

            if (whereClause.Trim().ToUpper().StartsWith("AND "))
                whereClause = whereClause.Substring(4);

            whereClause = whereClause.Replace(panel.PLCDataTable + ".", "");

            return whereClause;
        }

        private bool ValidateCaseToLink(string activityKey, bool fromCaseSearchScreen, out string caseKey)
        {
            caseKey = fromCaseSearchScreen ? CaseSearch.SelectedCaseKey : string.Empty;
            string whereClause = fromCaseSearchScreen ? "CASE_KEY = " + caseKey : GetDBPanelWhereClause(dbpLinkCases);

            // check if the user has search values
            if (string.IsNullOrEmpty(whereClause))
            {
                msgPromptLinkCase.ShowMessage("Link Case", "Please specify at least one parameter.", MessageBoxType.Information);
                return false;
            }

            // search for the case in TV_LABCASE accoridng to the values entered by the user
            PLCQuery qry = new PLCQuery("SELECT * FROM TV_LABCASE WHERE " + whereClause);
            qry.Open();

            if (qry.IsEmpty())
            {
                msgPromptLinkCase.ShowMessage("Link Case", "Please enter an existing case.", MessageBoxType.Information);
                return false;
            }
            else if(string.IsNullOrEmpty(caseKey))
            {
                caseKey = qry.FieldByName("CASE_KEY");
            }

            // check if the case is already linked in the activity (TV_ACTCASES)
            qry.SQL = string.Format("SELECT * FROM TV_ACTCASES WHERE ACTIVITY_KEY = {0} AND CASE_KEY = {1}", activityKey, caseKey);

            if (qry.Open() && qry.HasData())
            {
                msgPromptLinkCase.ShowMessage("Link Case", "Case is already linked in this activity.", MessageBoxType.Information);
                return false;
            }

            return true;
        }

        private void SaveLinkedCase(string activityKey, string caseKey)
        {
            // Save record
            PLCQuery qrySave = new PLCQuery("SELECT * FROM TV_ACTCASES WHERE 0=1");
            qrySave.Open();
            qrySave.Append();
            qrySave.SetFieldValue("ACTIVITY_KEY", SelectedActivityKey);
            qrySave.SetFieldValue("CASE_KEY", caseKey);
            qrySave.Post("TV_ACTCASES", 98, 7);
        }

        private void MaintainLinkCasePopup()
        {
            //if (CaseSearchScreen)
            //{
            //}

            //CaseSearchScreen = false;
        }

        private void SelectLinkCaseRecord(string activityKey, string caseKey)
        {
            Dictionary<string, string> keyValues = new Dictionary<string, string>();
            keyValues.Add("ACTIVITY_KEY", activityKey);
            keyValues.Add("CASE_KEY", caseKey);

            grdLinkCases.SelectRowByDataKeyValues<string>(keyValues);
        }

        private void DeleteLinkedCaseRecords(string activityKey, string caseKey)
        {
            PLCQuery qryDelete = new PLCQuery();
            string whereClause = "WHERE ACTIVITY_KEY = " + activityKey + (string.IsNullOrEmpty(caseKey) ? "" : " AND CASE_KEY = " + caseKey);

            qryDelete.Delete("TV_ACTCASES", whereClause, 98, 8);
        }

        private string DisplayLinkedCases(string activityKey)
        {
            PLCQuery qryLabCaseNumbers = new PLCQuery();
            List<string> labCaseNumbers = new List<string>();
            qryLabCaseNumbers.SQL = @"SELECT LC.LAB_CASE
                                      FROM TV_LABCASE LC
                                      LEFT OUTER JOIN TV_ACTCASES AC ON AC.CASE_KEY = LC.CASE_KEY
                                      WHERE AC.ACTIVITY_KEY = " + activityKey;
            qryLabCaseNumbers.Open();

            // Set button text color
            Button btnLinkedCases = PLCButtonPanel1.GetCustomButton("Link Cases");
            if (btnLinkedCases != null)
                btnLinkedCases.Style.Add("color", qryLabCaseNumbers.IsEmpty() ? "Black" : "Red");

            //
            if (!qryLabCaseNumbers.IsEmpty())
            {
                while (!qryLabCaseNumbers.EOF())
                {
                    labCaseNumbers.Add(qryLabCaseNumbers.FieldByName("LAB_CASE"));
                    qryLabCaseNumbers.Next();
                }
            }

            return string.Join(", ", labCaseNumbers);
        }

        #endregion

        private void UpdateQAIssue(string status)
        {
            string qaiAnalyst = "";
            PLCQuery qry = new PLCQuery("SELECT * FROM TV_ACT_QAISSUE WHERE ACTIVITY_KEY = " + SelectedActivityKey);
            qry.Open();
            if (qry.HasData())
            {
                qaiAnalyst = qry.FieldByName("ANALYST");

                qry.Edit();
                qry.SetFieldValue("STATUS", status);
                qry.SetFieldValue("STATUS_BY", PLCSession.PLCGlobalAnalyst);
                qry.SetFieldValue("STATUS_DATE", DateTime.Now);
                qry.Post("TV_ACT_QAISSUE", 98, 5);
            }

            string report = "QA Issue Requested by " + qaiAnalyst;
            if (status == "Approved")
            {
                qry = new PLCQuery("SELECT * FROM TV_QMSPREPT WHERE 0=1");
                qry.Open();
                qry.Append();
                qry.SetFieldValue("PROGRAM_REPORT_KEY", PLCSession.GetNextSequence("QMSPREPT_SEQ"));
                qry.SetFieldValue("ACTIVITY_KEY", SelectedActivityKey);
                qry.SetFieldValue("ASSIGNED_PROGRAM_KEY", 0);
                qry.SetFieldValue("REPORT_NUMBER", 1);
                qry.SetFieldValue("REPORT_TYPE", "QAI");
                qry.SetFieldValue("START_DATE", DateTime.Now);
                qry.SetFieldValue("DESCRIPTION", "QA Issue");
                qry.SetFieldValue("REPORT_TITLE", report);
                qry.Post("TV_QMSPREPT", 98, 5);
            }

            ToggleQAIssueButtons();

            ServerAlert(report + " has been " + status.ToLower() + ".");
        }

        private void ToggleQAIssueButtons()
        {
            bool hasQAI = SelectedActivityCodes.Replace(" ", "").Split(',').Contains("QAISSUE") && AIActivityKey != "";
            PLCButtonPanel1.SetCustomButtonVisible("Approve", hasQAI);
            PLCButtonPanel1.SetCustomButtonVisible("Reject", hasQAI);

            if (hasQAI)
            {
                PLCQuery qry = new PLCQuery("SELECT STATUS FROM TV_ACT_QAISSUE WHERE ACTIVITY_KEY = " + SelectedActivityKey);
                qry.Open();
                if (qry.HasData())
                {
                    bool hasStatus = !string.IsNullOrEmpty(qry.FieldByName("STATUS").Trim());
                    PLCButtonPanel1.SetCustomButtonMode("Approve", !hasStatus);
                    PLCButtonPanel1.SetCustomButtonMode("Reject", !hasStatus);
                }

                PLCButtonPanel1.DisableButton("ADD");
                PLCButtonPanel1.DisableButton("DELETE");
            }
        }

        private void ServerAlert(string message)
        {
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "SERVER_ALERT", "setTimeout(function () { Alert('" + message + "'); });", true);
        }

        public bool HasAnalystField()
        {
            return dbSearchActLog.GetPanelRecByFieldName("ANALYST") != null;
        }

        private void CheckUserRestriction()
        {
            if (PLCSession.CheckUserOption("ACTLOGUSER"))
            {
                if (dbSearchActLog.GetPanelFieldNames().Contains("ANALYST"))
                {
                    dbSearchActLog.setpanelfield("ANALYST", PLCSession.PLCGlobalAnalyst);
                    dbSearchActLog.EnablePanelField("ANALYST", false);
                }
            }
        }

        private void UserRestrictionOnEdit()
        {
            if (PLCSession.CheckUserOption("ACTLOGUSER"))
            {
                if (PLCDBPanel1.GetPanelFieldNames().Contains("ANALYST"))
                {
                    PLCDBPanel1.setpanelfield("ANALYST", PLCSession.PLCGlobalAnalyst);
                    PLCDBPanel1.EnablePanelField("ANALYST", false);
                }
            }
        }

        /// <summary>
        /// Check DBGRID ACTIVITYLOG existence
        /// </summary>
        /// <returns></returns>
        private bool IsActivityLogGridExists()
        {
            PLCQuery qryDBGrid = new PLCQuery();
            qryDBGrid.SQL = "SELECT GRID_NAME FROM TV_DBGRIDHD WHERE GRID_NAME = ?";
            qryDBGrid.AddSQLParameter("GRID_NAME", ACTIVITYLOG_GRID);
            qryDBGrid.Open();

            return qryDBGrid.HasData();
        }

        /// <summary>
        /// Load activity log records using DBGRID configuration
        /// </summary>
        private void LoadActivityViaDBGRID()
        {
            PLCHelperFunctions HelperFunctions = new PLCHelperFunctions();

            GridView1.PLCGridName = ACTIVITYLOG_GRID;
            GridView1.PLCSQLString_AdditionalCriteria = ActivityWhereClause();
            GridView1.InitializePLCDBGrid();

        }

        /// <summary>
        /// Generate where clause based on search fields
        /// </summary>
        /// <returns></returns>
        private string ActivityWhereClause(bool noAlias=false)
        {
            PLCHelperFunctions HelperFunctions = new PLCHelperFunctions();
            Dictionary<string, OrderedDictionary> tables = ActivityLogGridExists ? HelperFunctions.GetTablesAndAlias(ACTIVITYLOG_GRID) : new Dictionary<string, OrderedDictionary>();
            if(noAlias)
                tables = new Dictionary<string, OrderedDictionary>();

            DateTime outDateFrom, outDateTo;
            string dateFrom = dbSearchActLog.getpanelfield("DATEFROM");
            string dateFromMask = dbSearchActLog.GetPanelRecByFieldName("DATEFROM").editmask;
            string dateTo = dbSearchActLog.getpanelfield("DATETO");
            string dateToMask = dbSearchActLog.GetPanelRecByFieldName("DATETO").editmask;
            string Criteria = " WHERE ";

            bool dateFromValid = DateTime.TryParse(dateFrom,
                ((dateFromMask.ToUpper().StartsWith("DD")) ? CultureInfo.CreateSpecificCulture("en-GB") : CultureInfo.CreateSpecificCulture("en-US")),
                DateTimeStyles.None, out outDateFrom);

            bool dateToValid = DateTime.TryParse(dateTo,
                ((dateToMask.ToUpper().StartsWith("DD")) ? CultureInfo.CreateSpecificCulture("en-GB") : CultureInfo.CreateSpecificCulture("en-US")),
                DateTimeStyles.None, out outDateTo);

            string actDateString = (tables.ContainsKey("TV_ACTIVITY") ? tables["TV_ACTIVITY"][0] + "." : "") + "ACTIVITY_DATE";

            if (PLCSessionVars1.PLCDatabaseServer == "MSSQL")
            {
                Criteria += (string.IsNullOrEmpty(dateFrom) || !dateFromValid ? "" : " " + actDateString + " >= CONVERT(DATETIME, '" + outDateFrom.ToString(dateFromMask.Replace("D", "d").Replace("m", "M").Replace("Y", "y")) + ((dateFromMask.ToUpper().StartsWith("DD")) ? "', 103)" : "', 101)"))
                    + (string.IsNullOrEmpty(dateTo) || !dateToValid ? "" : (!dateFromValid ? "" : " AND") + " " + actDateString + " < CONVERT(DATETIME, '" + outDateTo.AddDays(1).ToString(dateToMask.Replace("D", "d").Replace("m", "M").Replace("Y", "y")) + ((dateToMask.ToUpper().StartsWith("DD")) ? "', 103)" : "', 101)"));
            }
            else
            {
                Criteria += (string.IsNullOrEmpty(dateFrom) || !dateFromValid ? "" : " " + actDateString + " >= CONVERTTODATE('" + outDateFrom.ToString("MM/dd/yyyy") + "')")
                    + (string.IsNullOrEmpty(dateTo) || !dateToValid ? "" : (!dateFromValid ? "" : " AND") + " " + actDateString + " < CONVERTTODATE('" + outDateTo.AddDays(1).ToString("MM/dd/yyyy") + "', 101)");
            }

            if (!dateFromValid)
                dbSearchActLog.setpanelfield("DATEFROM", "");

            if (!dateToValid)
                dbSearchActLog.setpanelfield("DATETO", "");

            if (Criteria.IndexOf("ACTIVITY_DATE") == -1)
                Criteria = string.Empty;

            // get all the field names of the activity log search dbpanel
            List<string> actLogSearchFields = dbSearchActLog.GetFieldNames();
            string activityCodeClause = PLCSession.FormatSpecialFunctions("(ACTIVITY_CODE = '{0}' OR (',' + ACTIVITY_CODE + ',') LIKE '%,{0},%')");
            if (tables.ContainsKey("TV_ACTIVITY"))
                activityCodeClause = activityCodeClause.Replace("ACTIVITY_CODE", tables["TV_ACTIVITY"][0] + "." + "ACTIVITY_CODE");

            // form the where clause 
            foreach (string fieldName in actLogSearchFields)
            {
                string fieldValue = dbSearchActLog.getpanelfield(fieldName);
                string fldName = (tables.ContainsKey("TV_ACTIVITY") ? tables["TV_ACTIVITY"][0] + "." : "") + fieldName;

                if (!fieldName.Equals("DATEFROM") && !fieldName.Equals("DATETO"))
                {
                    Criteria += string.IsNullOrEmpty(fieldValue) ? ""
                        : (string.IsNullOrEmpty(Criteria) ? " WHERE " : " AND ") + (fieldName.Equals("ACTIVITY_CODE") ? string.Format(activityCodeClause, fieldValue) : fldName + " = '" + fieldValue + "'");
                }
            }

            if (ActivityKey != "")
            {
                Criteria += (string.IsNullOrEmpty(Criteria) ? " WHERE " : " AND ") + "ACTIVITY_KEY = " + ActivityKey;
            }

            if (FromCaseInfo)
                Criteria += (string.IsNullOrEmpty(Criteria) ? " WHERE " : " AND ") + "ACTIVITY_KEY IN (SELECT ACTIVITY_KEY FROM TV_ACTCASES WHERE CASE_KEY = " + PLCSession.PLCGlobalCaseKey + ")";

            return PLCSession.FormatSpecialFunctions(Criteria);
        }


        /// <summary>
        /// Generate the activity header label
        /// </summary>
        /// <param name="pstrActivityCode">The activity code selected</param>
        /// <param name="pstrActivityLabel">The hardcoded activity label</param>
        /// <param name="pstrAnalyst">The analyst name of the activity</param>
        /// <param name="pstrActivityDate">The activity date</param>
        /// <returns>Returns the activity header label</returns>
        private string GenerateActivityHeaderLabel(string pstrActivityCode, string pstrActivityLabel, string pstrAnalyst, string pstrActivityDate)
        {
            string strHeaderLabel = string.Empty;

            if (pstrActivityCode == string.Empty)
            {
                strHeaderLabel = "ACTIVITY";
            }
            else
            {
                strHeaderLabel = string.Format("{0}{1}", pstrActivityLabel, pstrActivityCode);

                if (pstrAnalyst != string.Empty)
                {
                    strHeaderLabel = string.Format("{0} By: {1}", strHeaderLabel, pstrAnalyst);
                }

                strHeaderLabel = string.Format("{0} On: {1}", strHeaderLabel, pstrActivityDate);
            }

            return strHeaderLabel;
        }

        #endregion
    }
}
