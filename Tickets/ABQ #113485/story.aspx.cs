using System;
using System.Collections;
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
using System.IO;
using PLCCONTROLS;
using System.Text;
using System.Drawing;
using PLCWebCommon;
using PLCGlobals;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace BEASTiLIMS
{
    public partial class Story : PLCGlobals.PageBase
	{
		#region Properties and Declarations
	
		PLCGlobals.PLCCommon PLCCommonClass = new PLCGlobals.PLCCommon();

        PLCMsgBox mbox = new PLCMsgBox();

        private bool VSIsScheduleDBGridExists
        {
            get
            {
                if (ViewState["STORYDBGRIDEXISTS"] == null)
                    ViewState["STORYDBGRIDEXISTS"] = IsScheduleDBGridExists();
                return (bool)ViewState["STORYDBGRIDEXISTS"];
            }
            set
            {
                ViewState["STORYDBGRIDEXISTS"] = value;
            }
        }

		#endregion

		#region Events

		protected void Page_Load(object sender, EventArgs e)
        {
            Control CtrlMsgBox = LoadControl("~/PLCWebCommon/PLCMsgBox.ascx");
            mbox = (PLCMsgBox)CtrlMsgBox;
            phMsgBox.Controls.Add(mbox);
            mbox.OnOkScript = "";

            if (PLCSession.PLCGlobalCaseKey == "")
            {
                Response.Redirect("~/DashBoard.aspx");
                return;
            }
            
            PLCCommonClass.SetSelectedMenuItem(PLCGlobals.MainMenuTab.DefaultTab, (Menu)Master.FindControl("menu_main"));

            // Set page title.
            string casetitle = (PLCSessionVars1.GetLabCtrl("STORY_TEXT").Trim() != string.Empty)? PLCSessionVars1.GetLabCtrl("STORY_TEXT") + " for" : "Schedules for";
            ((MasterPage)Master).SetCaseTitle(casetitle);

            if (!IsPostBack)
            {
                if (Session["FilterOutCaseEvents"] != null && Session["FilterOutCaseEvents"].ToString() == "T")
                {
                    Session["FilterOutCaseEvents"] = null;
                    hdnFilterCaseEvents.Value = "T";
                    PLCButtonPanel1.SetCustomButtonVisible("Back to Case", false);
                }
                else
                    PLCButtonPanel1.SetCustomButtonVisible("Back to Checklist", false);
                
                if (dbpScheduleSearch.NumVisiblePanelRecs() > 0)
                {
                    dbpScheduleSearch.SetPanelDefaultValuesOnBlankFields();
                    pnlFilters.Visible = false;

                    string[] fieldNames = dbpScheduleSearch.GetPanelFieldNames();
                    if (!fieldNames.Contains("ANALYST"))
                    {
                        btnCurrentAnalystSearch.Visible = false ;
                    }
                }
                pnlScheduleSearch.Visible = !pnlFilters.Visible;

                GetFilterProperties();
                PopulateGrid(string.Empty);

                DisplaySelectedSchedule();

                ToggleSupplementBtnUserOptions();
            }
            else
                GridView1.SetPLCGridProperties();

            SetFilterProperties();            
        }

        protected void GridView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            PLCSessionVars1.PLCGlobalScheduleKey = GridView1.SelectedDataKey.Value.ToString();
            GrabGridRecord();

            ToggleSupplementBtnUserOptions();
        }

        protected void GridView1_PageIndexChanged(object sender, EventArgs e)
        {
            GridView1.SelectedIndex = 0;
            GrabGridRecord();

            ToggleSupplementBtnUserOptions();
        }

        protected void GridView1_Sorted(object sender, EventArgs e)
        {
            GridView1.SelectedIndex = 0;
            GrabGridRecord();

            ToggleSupplementBtnUserOptions();
        }

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (VSIsScheduleDBGridExists)
                return;

            if (e.Row.RowType == DataControlRowType.DataRow ||
              e.Row.RowType == DataControlRowType.Header ||
               e.Row.RowType == DataControlRowType.Footer)
            {
                e.Row.Cells[1].Visible = false;
                e.Row.Cells[2].Visible = false;
                
            }
        }

        protected void PLCButtonPanel1_PLCButtonClick(object sender, PLCButtonClickEventArgs e)
        {
            if ((e.button_name == "ADD") & (e.button_action == "AFTER"))
            {                
                GridView1.SetControlMode(false);
				FilterEnable(false);
                DisableAttachmentsClip(true, true);
			}

            if ((e.button_name == "EDIT") & (e.button_action == "BEFORE"))
            {
                if (!PLCSession.CheckUserOption("EDITSTORY") && PLCDBPanel1.getpanelfield("ANALYST") != PLCSessionVars1.PLCGlobalAnalyst)
                {
                    mbox.ShowMsg("Edit Record", "You do not have the authority to edit this entry.", 0);
                    e.button_canceled = true;
                    return;
                }
            }

            if ((e.button_name == "EDIT") & (e.button_action == "AFTER"))
            {                
                GridView1.SetControlMode(false);
				FilterEnable(false);
                DisableAttachmentsClip(true);
			}

            if ((e.button_name == "DELETE") & (e.button_action == "BEFORE"))
            {
                if (!PLCSession.CheckUserOption("EDITSTORY") && PLCDBPanel1.getpanelfield("ANALYST") != PLCSessionVars1.PLCGlobalAnalyst)
                {
                    mbox.ShowMsg("Delete Record","You do not have the authority to delete another analyst's entry.", 0);
                    e.button_canceled = true;
                    return;
                }
            }

            if ((e.button_name == "DELETE") & (e.button_action == "AFTER"))
            {
                GridView1.InitializePLCDBGrid();
                if (GridView1.Rows.Count > 0)
                {
                    GridView1.SelectedIndex = 0;
                    GrabGridRecord();
                }
                else
                {
                    PLCDBPanel1.EmptyMode();
                    PLCButtonPanel1.SetEmptyMode();
                    PLCButtonPanel1.SetCustomButtonEnabled("Back to Case", true);
                    DisableAttachmentsClip(true, true);
                }

            }

            if ((e.button_name == "SAVE") & (e.button_action == "AFTER"))
            {
                GridView1.InitializePLCDBGrid();
                GridView1.SelectRowByDataKey(PLCSessionVars1.PLCGlobalScheduleKey);
                GridView1.SetControlMode(true);
                FilterEnable(true);
                GrabGridRecord();

                ToggleSupplementBtnUserOptions();
            }

            if ((e.button_name == "CANCEL") & (e.button_action == "AFTER"))
            {                
                GridView1.InitializePLCDBGrid();
                GridView1.SelectRowByDataKey(PLCSessionVars1.PLCGlobalScheduleKey);
                GridView1.SetControlMode(true);
                GrabGridRecord();
				FilterEnable(true);

                ToggleSupplementBtnUserOptions();
            }

            if (e.button_name == "Print")
            {
                PrintReport();
            }

            if (e.button_name == "Summary")
            {
                Response.Redirect("~/StorySum.aspx");
            }

            if (e.button_name.ToUpper() == "BACK TO CASE")
                Response.Redirect("TAB1Case.aspx");

            if (e.button_name.ToUpper() == "BACK TO CHECKLIST")
                Response.Redirect("TechReview2.aspx");
        }

        protected void PLCDBPanel1_PLCDBPanelGetNewRecord(object sender, PLCDBPanelGetNewRecordEventArgs e)
        {
            int newkey = 0;
            newkey = PLCSessionVars1.GetNextSequence("LABSUPP_SEQ");

            e.NewRecordValues.Add("SCHEDULE_KEY", newkey);
            e.NewRecordValues.Add("CASE_KEY", PLCSessionVars1.PLCGlobalCaseKey);

            e.newWhereClause = " where SCHEDULE_KEY = " + newkey;
            PLCSessionVars1.PLCGlobalScheduleKey = newkey.ToString();
        }

        protected void btnAllAnalyst_Click(object sender, EventArgs e)
        {
			fbAnalyst.SetValue(string.Empty);
			fbType.SetValue(string.Empty);
            GridView1.SelectedIndex = -1;
            PopulateGrid(string.Empty);
            DisplaySelectedSchedule();

            ToggleSupplementBtnUserOptions();
        }

        protected void btnCurrentAnalyst_Click(object sender, EventArgs e)
        {
			fbAnalyst.SetValue(PLCSessionVars1.PLCGlobalAnalyst);
			fbType.SetValue(string.Empty);
			Flexbox_ValueChanged(null, null);
        }

		protected void Flexbox_ValueChanged(object sender, EventArgs e)
		{
			string condition = string.Empty;
			if (fbAnalyst.GetValue().Trim() != string.Empty)
			{
				condition += " AND A.ANALYST = '" + fbAnalyst.GetValue().Trim() + "'";
			}

			if (fbType.GetValue().Trim() != string.Empty)
			{
				condition += " AND A.TYPE_RES = '" + fbType.GetValue().Trim() + "'";
			}
			GridView1.SelectedIndex = -1;
			PopulateGrid(condition);
			DisplaySelectedSchedule();

            ToggleSupplementBtnUserOptions();
		}

        protected void btnScheduleSearch_Click(object sender, EventArgs e)
        {
            var fields = dbpScheduleSearch.GetPanelFieldRecs();

            Dictionary<string, OrderedDictionary> tables = GetTablesAndAlias(false);

            string condition = string.Empty;

            foreach (var panelRec in fields)
            {
                string fieldValue = dbpScheduleSearch.GetFieldValue(panelRec.tblname, panelRec.fldname, panelRec.prompt);

                if (!string.IsNullOrEmpty(fieldValue))
                {
                    if (panelRec.fldname == "EVIDENCE_CONTROL_NUMBER")
                    {
                        condition += String.Format(" AND ({0}.EVIDENCE_CONTROL_NUMBER = '{2}' OR {1}.EVIDENCE_CONTROL_NUMBER = '{2}')", tables[panelRec.tblname][0], tables["TV_LABITEM"][0], fieldValue);
                    }
                    else
                    {
                        condition += PLCGlobals.PLCCommon.instance.GetWhereClauseByType(panelRec.tblname, panelRec.fldname, panelRec.prompt, panelRec.editmask.ToUpper(), panelRec.codetable, fieldValue, tables, panelRec.usesLikeSearch);
                    }
                }
            }
            GridView1.SelectedIndex = -1;
            PopulateGrid(condition);
            DisplaySelectedSchedule();

            ToggleSupplementBtnUserOptions();
        }

        protected void btnAllAnalystSearch_Click(object sender, EventArgs e)
        {
            dbpScheduleSearch.ClearFields();
            btnScheduleSearch_Click(null, null);
        }

        protected void btnCurrentAnalystSearch_Click(object sender, EventArgs e)
        {
            dbpScheduleSearch.setpanelfield("ANALYST", PLCSession.PLCGlobalAnalyst);
            btnScheduleSearch_Click(null, null);
        }

        #endregion

        #region Methods

        private void GrabGridRecord()
		{

            if (GridView1.Rows.Count > 0)
            {
                PLCSessionVars1.PLCGlobalScheduleKey = PLCSessionVars1.PLCGlobalAttachmentSourceKey = GridView1.SelectedDataKey.Value.ToString();
                PLCDBPanel1.PLCWhereClause = " where SCHEDULE_KEY = " + GridView1.SelectedDataKey.Value.ToString();
                PLCDBPanel1.DoLoadRecord();
            }
            else
            {
                PLCDBPanel1.EmptyMode();
                PLCButtonPanel1.SetEmptyMode();
                PLCButtonPanel1.SetCustomButtonEnabled("Back to Case", true);
                PLCButtonPanel1.SetCustomButtonEnabled("Back to Checklist", true);
            }

            SetAttachmentsClip(GridView1.Rows.Count > 0 ? "CASECOR" : "CASE", PLCSessionVars1.PLCGlobalScheduleKey);
		}

		private void PopulateGrid(string condition)
        {
            string sql = string.Empty;

            if (hdnFilterCaseEvents.Value == "T")
                condition += " AND A.TYPE_RES <> 'CE' ";

            if (VSIsScheduleDBGridExists)
            {
                GridView1.PLCSQLString_AdditionalCriteria = PLCSessionVars1.FormatSpecialFunctions("A.CASE_KEY = " + PLCSessionVars1.PLCGlobalCaseKey + condition);
            }
            else
            {
                if (PLCSession.PLCDatabaseServer == "MSSQL")
                {
                    sql = "Select A.SCHEDULE_KEY, A.CASE_KEY, A.DATE_RES AS \"Date\", FORMATTIME(A.TIME) AS \"Time\", " +
                    "B.DESCRIPTION AS \"Schedule Type\", C.NAME AS \"Officer Name\", " +
                     @" CASE WHEN A.EVIDENCE_CONTROL_NUMBER IS NULL THEN
                          CASE
                            WHEN EXISTS (
                                SELECT *
                                FROM TV_SCHDETL S
                                LEFT OUTER JOIN TV_SCHDETL SD ON SD.SCHEDULE_KEY = S.SCHEDULE_KEY
                                WHERE S.SCHEDULE_KEY = A.SCHEDULE_KEY
                            ) THEN
                              COALESCE(
                                (STUFF((SELECT ', ' + LI2.LAB_ITEM_NUMBER
                                 FROM TV_SCHEDULE S
                                 LEFT OUTER JOIN TV_SCHDETL SD ON SD.SCHEDULE_KEY = S.SCHEDULE_KEY
                                 LEFT OUTER JOIN TV_LABITEM LI2 ON LI2.EVIDENCE_CONTROL_NUMBER = SD.EVIDENCE_CONTROL_NUMBER
                                 WHERE S.SCHEDULE_KEY = A.SCHEDULE_KEY
                                 FOR XML PATH ('')), 1, 1, '')),
                                'Deleted')
                          ELSE 'Not Item Related'
                          END
                        ELSE COALESCE(I.LAB_ITEM_NUMBER, 'Deleted')
                     END AS ""Item #"" " +
                    "FROM TV_SCHEDULE A LEFT OUTER JOIN TV_SCHTYPE B ON B.TYPE_RES = A.TYPE_RES " +
                    "LEFT OUTER JOIN TV_ANALYST C ON C.ANALYST = A.ANALYST " +
                    "LEFT OUTER JOIN TV_LABITEM I ON I.EVIDENCE_CONTROL_NUMBER = A.EVIDENCE_CONTROL_NUMBER " +
                    "WHERE A.CASE_KEY = " + PLCSessionVars1.PLCGlobalCaseKey + condition + " " +
                    "ORDER BY A.DATE_RES, \"Time\", A.SCHEDULE_KEY";
                }
                else
                {
                    sql = @"Select A.SCHEDULE_KEY, A.CASE_KEY, A.DATE_RES AS ""Date"", TO_CHAR(CAST(A.TIME AS TIMESTAMP(7)), 'HH:MI:SS') AS ""Time"",
                    B.DESCRIPTION AS ""Schedule Type"", C.NAME AS ""Officer Name"",
                        CASE WHEN A.EVIDENCE_CONTROL_NUMBER IS NULL THEN
                        CASE
                          WHEN EXISTS (
                                SELECT *
                                FROM TV_SCHDETL S
                                LEFT OUTER JOIN TV_SCHDETL SD ON SD.SCHEDULE_KEY = S.SCHEDULE_KEY
                                WHERE S.SCHEDULE_KEY = A.SCHEDULE_KEY
                            ) THEN
                            COALESCE(
                                (SELECT LISTAGG(LI2.LAB_ITEM_NUMBER, ', ') WITHIN GROUP (ORDER BY LI2.LAB_ITEM_NUMBER)
                                FROM TV_SCHEDULE S
                                LEFT OUTER JOIN TV_SCHDETL SD ON SD.SCHEDULE_KEY = S.SCHEDULE_KEY
                                LEFT OUTER JOIN TV_LABITEM LI2 ON LI2.EVIDENCE_CONTROL_NUMBER = SD.EVIDENCE_CONTROL_NUMBER
                                WHERE S.SCHEDULE_KEY = A.SCHEDULE_KEY),
                               'Deleted')
                           ELSE 'Not Item Related'
                           END
                         ELSE COALESCE(I.LAB_ITEM_NUMBER, 'Deleted')
                    END AS ""Item #""
                    FROM TV_SCHEDULE A LEFT OUTER JOIN TV_SCHTYPE B ON B.TYPE_RES = A.TYPE_RES
                    LEFT OUTER JOIN TV_ANALYST C ON C.ANALYST = A.ANALYST
                    LEFT OUTER JOIN TV_LABITEM I ON I.EVIDENCE_CONTROL_NUMBER = A.EVIDENCE_CONTROL_NUMBER
                    WHERE A.CASE_KEY = " + PLCSessionVars1.PLCGlobalCaseKey + condition + " " +
                    "ORDER BY A.DATE_RES, A.TIME, A.SCHEDULE_KEY";
                }

                GridView1.PLCSQLString = PLCSessionVars1.FormatSpecialFunctions(sql);
            }
            
            GridView1.InitializePLCDBGrid();

            // empty 
            if (GridView1.Rows.Count == 0)
            {
                PLCDBPanel1.EmptyMode();
                PLCButtonPanel1.SetEmptyMode();
                PLCButtonPanel1.SetCustomButtonEnabled("Back to Case", true);
                PLCButtonPanel1.SetCustomButtonEnabled("Back to Checklist", true);
                SetAttachmentsClip("CASE", "0");
            }
        }

		private void DisplaySelectedSchedule()
        {
            if ((GridView1.SelectedIndex < 0) && (GridView1.Rows.Count > 0))
            {
                // point to currently selected record after going back from attachments page
                if (!IsPostBack && PLCSessionVars1.GetDefault("FROMATTACH").Equals("T") && !string.IsNullOrEmpty(PLCSessionVars1.PLCGlobalAttachmentSourceKey))
                {
                    PLCSessionVars1.SetDefault("FROMATTACH", null);
                    GridView1.SelectRowByDataKey(PLCSessionVars1.PLCGlobalAttachmentSourceKey);
                }

                if (GridView1.SelectedIndex < 0)
                    GridView1.SelectedIndex = 0;

                GridView1.DataBind();
                GrabGridRecord();
                PLCButtonPanel1.SetBrowseMode();
            }
        }

		private void FilterEnable(bool enable)
		{
			//disabling only the panel does not disable the flexboxes
            if (pnlFilters.Visible)
            {
                pnlFilters.Enabled = enable;
                fbAnalyst.Enabled = enable;
                fbType.Enabled = enable;
            }
			else
            {
                btnScheduleSearch.Enabled = enable;
                btnAllAnalystSearch.Enabled = enable;
                btnCurrentAnalystSearch.Enabled = enable;

                if (enable)
                {
                    dbpScheduleSearch.EditMode();
                }
                else
                {
                    dbpScheduleSearch.SetBrowseMode();
                }
            }
		}

		private void GetFilterProperties()
		{
			PLCQuery qryFilter = new PLCQuery("SELECT * FROM TV_DBPANEL" + 
				" WHERE TABLE_NAME='TV_SCHEDULE' AND PANEL_NAME='PANEL_STORY'" + 
				" AND FIELD_NAME IN ('ANALYST','TYPE_RES')");
			qryFilter.Open();
			while (!qryFilter.EOF())
			{
				String value = qryFilter.FieldByName("PROMPT") + ":" + qryFilter.FieldByName("CODE_TABLE") + ":" + qryFilter.FieldByName("CODE_DESC_FORMAT") + ":" + 
							qryFilter.FieldByName("CODE_DESC_SEPARATOR") + ":" + qryFilter.FieldByName("CODE_CONDITION");
				if (qryFilter.FieldByName("FIELD_NAME") == "ANALYST")
					hfPropAnalyst.Value = value;
				else
					hfPropType.Value = value;
				qryFilter.Next();
			}
		}

		private void SetFilterProperties()
		{
			String[] fbProperties = hfPropAnalyst.Value.Split(':');
			lblAnalyst.Text = fbProperties[0];
			fbAnalyst.TableName = fbProperties[1];
			fbAnalyst.DescriptionFormatCode = fbProperties[2];
			fbAnalyst.DescriptionSeparator = fbProperties[3];
			fbAnalyst.CodeCondition = "(ACCOUNT_DISABLED <> 'T' OR ACCOUNT_DISABLED IS NULL)" +
				(fbProperties[4] != "" ? " AND " + fbProperties[4] : "");

			fbProperties = hfPropType.Value.Split(':');
			lblType.Text = fbProperties[0];
			fbType.TableName = fbProperties[1];
			fbType.DescriptionFormatCode = fbProperties[2];
			fbType.DescriptionSeparator = fbProperties[3];
			fbType.CodeCondition = fbProperties[4];
		}

        private void ToggleSupplementBtnUserOptions()
        {
            if (!PLCSession.CheckUserOption("EDITSTORY"))
            {
                if (PLCSession.CheckUserOption("RESEDNAR"))
                    PLCButtonPanel1.DisableButton("EDIT");

                if (PLCSession.CheckUserOption("RESDELNAR"))
                    PLCButtonPanel1.DisableButton("DELETE");

                if (PLCSession.CheckUserOption("RECEIVEINQ"))
                    PLCButtonPanel1.DisableButton("ADD");
            }

            // Disable plcbuttonpanel and other plcbuttonpanel custom button for read only access
            if (PLCCommonClass.IsReadOnlyAccess("WEBINQ,RONLYCATAB"))
                PLCCommonClass.SetReadOnlyAccess(PLCButtonPanel1);
        }

        private void SetAttachmentsClip(string source, string scheduleKey)
        {
            MasterPage master = (MasterPage)this.Master;
            bool notifyAllTabs = PLCSessionVars1.GetLabCtrl("NOTIFY_ATTACH_FOR_TABS").Equals("T");

            PLCQuery qryLabCase = new PLCQuery("SELECT DEPARTMENT_CODE, DEPARTMENT_CASE_NUMBER FROM TV_LABCASE WHERE CASE_KEY = " + PLCSessionVars1.PLCGlobalCaseKey);
            qryLabCase.Open();

            PLCSessionVars1.PLCGlobalAttachmentSourceDesc = "Agency: " + qryLabCase.FieldByName("DEPARTMENT_CODE") + ", " + PLCSessionVars1.GetLabCtrl("DEPT_CASE_TEXT") + ":" + qryLabCase.FieldByName("DEPARTMENT_CASE_NUMBER");

            if (source == "CASE" && notifyAllTabs)
            {
                PLCSessionVars1.PLCGlobalAttachmentSource = source;
                master.ApplyImageAttachedClip("LABCASE", PLCSessionVars1.PLCGlobalCaseKey);
            }
            else if (source == "CASECOR" && PLCSessionVars1.SafeInt(scheduleKey, 0) > 0)
            {
                PLCSessionVars1.PLCGlobalAttachmentSource = source;
                master.ApplyImageAttachedClip("CASECOR", scheduleKey);
            }

            master.EnablePaperClip(GridView1.Rows.Count > 0);
            if (GridView1.Rows.Count <= 0) master.SetPaperClipImageManually(false);
        }

        private void DisableAttachmentsClip(bool disable, bool removeBlink = false)
        {
            MasterPage master = (MasterPage)this.Master;
            master.EnablePaperClip(!disable);
            if (removeBlink) master.SetPaperClipImageManually(false);
        }

        private bool IsScheduleDBGridExists()
        {
            PLCQuery qryDBGrid = new PLCQuery();
            qryDBGrid.SQL = "SELECT * FROM TV_DBGRIDHD WHERE GRID_NAME = '" + GridView1.PLCGridName + "'";
            qryDBGrid.Open();

            return qryDBGrid.HasData();
        }

        private void PrintReport()
        {
            string reportName = PLCSession.FindCrystalReport("story.rpt");
            if (!string.IsNullOrEmpty(reportName))
            {
                string rpttitle = (PLCSession.GetLabCtrl("STORY_TEXT").Trim() != string.Empty) ? PLCSessionVars1.GetLabCtrl("STORY_TEXT") + " Summary" : "Schedule Summary";
                PLCSession.PLCCrystalReportName = reportName;
                PLCSession.PLCCrystalSelectionFormula = "{TV_SCHEDULE.SCHEDULE_KEY} = " + PLCSession.PLCGlobalScheduleKey;
                PLCSession.PLCCrystalReportTitle = rpttitle;
                PLCSessionVars1.PrintCRWReport(true);
            }
            else
            {
                mbox.ShowMsg("Print", "Cannot print report. Report file STORY.rpt not found.", 0);
            }
        }

        private Dictionary<string, OrderedDictionary> GetTablesAndAlias(bool useDefault)
        {
            Dictionary<string, OrderedDictionary> tables = new Dictionary<string, OrderedDictionary>();
            if (useDefault)
            {
                tables.Add("TV_SCHEDULE", new OrderedDictionary { { String.Empty, "A" } });
                tables.Add("TV_SCHTYPE", new OrderedDictionary { { String.Empty, "B" } });
                tables.Add("TV_ANALYST", new OrderedDictionary { { String.Empty, "C" } });
                tables.Add("TV_LABITEM", new OrderedDictionary { { String.Empty, "I" } });
                tables.Add("TV_EXAMCODE", new OrderedDictionary { { String.Empty, "E" } });
            }
            else
            {
                PLCHelperFunctions SearchHelper = new PLCHelperFunctions();
                tables = SearchHelper.GetTablesAndAlias(GridView1.PLCGridName);

                if (tables.Count == 0)
                    tables = GetTablesAndAlias(true);
            }
            return tables;
        }

        #endregion

    }
}
