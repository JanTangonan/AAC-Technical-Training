using System;
using System.Collections;
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
using PLCGlobals;
using PLCWebCommon;
using System.Text;
using System.Globalization;
using System.Data.OleDb;
using System.Data.OracleClient;
using System.Collections.Specialized;
using PLCGlobals.MEIMS;
using System.Drawing;
using PLCGlobals.CCAP;

namespace BEASTiLIMS
{
    public partial class TAB1Case : PageBase
    {
        public class FieldRec
        {
            public string TableName;
            public string FieldName;
            public string FieldValue;
            public string PictureMask;

            public FieldRec(string TableName, string FieldName, string FieldValue, string PictureMask)
            {
                this.TableName = TableName;
                this.FieldName = FieldName;
                this.FieldValue = FieldValue;
                this.PictureMask = PictureMask;
            }
        }

        enum DialogKeys
        {
            None,
            CCAPForceUpdate,
        }

        private string _OffenseCodeCondition
        {
            get
            {
                if (ViewState["occ"] == null)
                    ViewState["occ"] = string.Empty;
                return ViewState["occ"].ToString();
            }
            set
            {
                ViewState["occ"] = value;
            }
        }

        private bool NotifyRetentionMessage
        {

            get
            {
                if (ViewState["NotifyRetentionMessage"] == null)
                {
                    ViewState["NotifyRetentionMessage"] = true;
                    return true;
                }
                else
                {
                    return (bool)ViewState["NotifyRetentionMessage"];
                }
            }
            set
            {
                ViewState["NotifyRetentionMessage"] = value;
            }
        }

        private enum ExpungeType
        {
            Expunge,
            AdminRemoval
        }

        private List<FieldRec> SubmissionFieldList;

        PLCCommon PLCCommonClass = new PLCCommon();
        PLCDBGlobal PLCDBGlobalClass = new PLCDBGlobal();

        PLCMsgBox mbox = new PLCMsgBox();
        PLCMsgComment mInput = new PLCMsgComment();

        private Boolean ReferenceExists { get; set; }

        private String OriginalInvestigatingAGency = "";

        bool referenceUpdate = false;

        //Page is double submitting, using viewstate wasnt working so changing to session
        private String OriginalDepartmentCode
        {
            get
            {
                if (Session["ORIGINALDEPTCODE"] == null)
                {
                    Session["ORIGINALDEPTCODE"] = PLCDBPanel1.GetOriginalValue("DEPARTMENT_CODE");
                }
                return (String)Session["ORIGINALDEPTCODE"];
            }


            set
            {
                Session["ORIGINALDEPTCODE"] = value;
            }

        }

        public List<string> AttachmentEntryIDs
        {
            get
            {
                if (ViewState["CaseAttachEntryIDs"] == null)
                {
                    ViewState["CaseAttachEntryIDs"] = new List<string>();
                    return (List<string>)ViewState["CaseAttachEntryIDs"];
                }
                else
                {
                    return (List<string>)ViewState["CaseAttachEntryIDs"];
                }
            }
            set
            {
                ViewState["CaseAttachEntryIDs"] = value;
            }
        }

        //Page is double submitting, using viewstate wasnt working so changing to session
        private String OriginalRefType
        {
            get
            {
                if (Session["ORIGINALREFTYPE"] == null)
                {
                    Session["ORIGINALREFTYPE"] = PLCDBPanel1.GetOriginalValue("CASE_REFERENCE_TYPE");
                }
                return (String)Session["ORIGINALREFTYPE"];
            }


            set
            {
                Session["ORIGINALREFTYPE"] = value;
            }

        }




        private void EnableButtonControls(bool Enable, bool UnLock)
        {
            bnCaseJacket.Enabled = Enable;
            bnCaseLabel.Enabled = Enable;
            bnDispo.Enabled = Enable;
            bnSchedule.Enabled = Enable;
            bnReference.Enabled = Enable;
            bnSupplements.Enabled = Enable;
            bnCaseReports.Enabled = Enable;
            //bnUnlock.Enabled = UnLock;
            bnTeam.Enabled = (Enable ? PLCDBGlobalClass.isCaseLock(PLCSessionVars1.PLCGlobalCaseKey) : Enable); // Fix improper enabling/disabling of Team button
            bnCaseLock.Enabled = Enable;
            //btnChangeIOR.Enabled = Enable ? CanChangeIOR : false;
            //btnChangeURN.Enabled = Enable ? CanChangeURN : false;
            //btnRouteItems.Enabled = Enable ? CanRouteItem : false;
            //
            btnRelatedCase.Enabled = Enable;
            btnForms.Enabled = Enable;
            btnNICS.Enabled = Enable;
            bnDiscoveryPacket.Enabled = Enable;
            btnActivity.Enabled = Enable;
            btnJIMSReport.Enabled = Enable;
            chkCaseLocked.Enabled = !Enable;
            btnFOIA.Enabled = Enable;
            btnDistribution.Enabled = Enable;
            btnLaboffense.Enabled = Enable;
            btnCodisHits.Enabled = Enable;
            btnAddressLabel.Enabled = Enable;
            btnChangeCase.Enabled = Enable;
            btnDatabankError.Enabled = Enable;
            btnSeizureTrack.Enabled = Enable;
            btnCaseEmail.Enabled = Enable;
        }

        protected void bnTeam_Load(object sender, EventArgs e)
        {

            if (!PLCDBPanel1.IsBrowseMode) return;
            if (PLCDBPanel1.IsSavingMode) return;
            if (IsPostBack) return;

            bnTeam.Visible = (PLCSessionVars1.GetLabCtrl("USES_INVESTIGATOR_LOCKED_CASES") == "T") && (PLCDBPanel1.getpanelfield("CASE_MANAGER") == PLCSessionVars1.PLCGlobalAnalyst || PLCDBPanel1.getpanelfield("CASE_ANALYST") == PLCSessionVars1.PLCGlobalAnalyst || PLCSessionVars1.CheckUserOption("SUPERLOCK"));
            if (bnTeam.Visible)
            {
                // set color of Team button                
                PLCQuery qryCaseTeam = new PLCQuery();
                qryCaseTeam.SQL = "SELECT * from TV_CASEACCESS WHERE CASE_KEY = " + PLCSessionVars1.PLCGlobalCaseKey;
                qryCaseTeam.Open();
                if (!qryCaseTeam.IsEmpty())
                    bnTeam.Style.Add("color", "Red");
                else
                    bnTeam.Style.Add("color", "Black");
            }
        }

        protected void btnSeizureTrack_Load(object sender, EventArgs e)
        {

            if (!PLCDBPanel1.IsBrowseMode) return;
            if (PLCDBPanel1.IsSavingMode) return;
            if (IsPostBack) return;

            
            if (btnSeizureTrack.Visible)
            {
                // set color of Team button                
                PLCQuery qrySeizure = new PLCQuery();
                qrySeizure.SQL = "SELECT * from TV_SEIZURE WHERE Case_Key = " + PLCSession.PLCGlobalCaseKey;
                qrySeizure.Open();
                if (!qrySeizure.IsEmpty())
                    btnSeizureTrack.Style.Add("color", "Red");
                else
                    btnSeizureTrack.Style.Add("color", "Black");
            }
        }

        protected void bnCaseLock_Load(object sender, EventArgs e)
        {
            if (!PLCDBPanel1.IsBrowseMode) return;
            if (PLCDBPanel1.IsSavingMode) return;
            if (IsPostBack) return;

            if (PLCCommonClass.IsReadOnlyAccess("WEBINQ,RONLYCATAB"))
                bnCaseLock.Visible = false;
            else
                bnCaseLock.Visible = (PLCSessionVars1.GetLabCtrl("USES_INVESTIGATOR_LOCKED_CASES") == "T") && (PLCDBPanel1.getpanelfield("CASE_MANAGER") == PLCSessionVars1.PLCGlobalAnalyst || PLCDBPanel1.getpanelfield("CASE_ANALYST") == PLCSessionVars1.PLCGlobalAnalyst || PLCSessionVars1.CheckUserOption("SUPERLOCK"));
        }

        protected void bnReference_Load(object sender, EventArgs e)
        {
            lblCaseReference.Text = HttpUtility.HtmlEncode(GetCaseReferenceStr(PLCSessionVars1.PLCGlobalCaseKey));
            string caseReferenceLabelColor = PLCSession.GetLabCtrlFlag("CASE_REFERENCE_LABEL_COLOR");
            if (PLCSession.GetLabCtrlFlag("SHOW_CASE_REFERENCE_LABEL") == "T")
            {
                lblCaseReference.Visible = true;
                lblCaseReference.ForeColor = Color.FromName(caseReferenceLabelColor);
            }
            else
            {
                lblCaseReference.Visible = false;
            }

            if (!PLCDBPanel1.IsBrowseMode) return;
            if (PLCDBPanel1.IsSavingMode) return;
            if (IsPostBack) return;

            if (ReferenceExists)
            {
                bnReference.Style.Add("color", "Red");
            }
            else
            {
                bnReference.Style.Add("color", "Black");
            }
        }

        protected void btnForms_Load(object sender, EventArgs e)
        {

            if (!PLCDBPanel1.IsBrowseMode) return;
            if (PLCDBPanel1.IsSavingMode) return;
            if (IsPostBack) return;

            PLCQuery qryWorkSheet = new PLCQuery();
            qryWorkSheet.SQL = "SELECT WORK_SHEET_ID FROM TV_WRKSHEET WHERE CASE_KEY = " + PLCSession.PLCGlobalCaseKey + " AND SOURCE = 'CASEFORMS'";
            qryWorkSheet.Open();
            if (qryWorkSheet.HasData())
            {
                btnForms.Style.Add("color", "Red");
            }
            else
            {
                btnForms.Style.Add("color", "Black");
            }
        }

        protected void btnError_Load(object sender, EventArgs e)
        {
            if (!PLCDBPanel1.IsBrowseMode) return;
            if (PLCDBPanel1.IsSavingMode) return;
            if (IsPostBack) return;

            if (!btnDatabankError.Visible)
                return;

            PLCQuery qryDBSError = new PLCQuery();
            qryDBSError.SQL = "SELECT * FROM TV_DBSERROR WHERE CASE_KEY = " + PLCSession.PLCGlobalCaseKey;
            qryDBSError.Open();

            btnDatabankError.Style.Add("color", qryDBSError.HasData() ? "Red" : "Black");
        }


        private Boolean allNumeric(String checkStr) 
        {

            foreach (Char ch in checkStr.ToCharArray() )
            {
                if (!"0123456789".Contains(ch)) return false;
            }

            return true;
        }

        protected void Page_Init()
        {
            //fix/work-around for error 
            //"Two components with the same id 'someId' can’t be added to the application"
            ScriptManager scriptManager = (ScriptManager)this.Master.FindControl("ScriptManager1");
            if (scriptManager != null)
            {
                scriptManager.ScriptMode = ScriptMode.Release;
                scriptManager.AsyncPostBackTimeout = 600;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Control CtrlMsgBox = LoadControl("~/PLCWebCommon/PLCMsgBox.ascx");
            mbox = (PLCMsgBox)CtrlMsgBox;
            phMsgBox.Controls.Add(mbox);

            Control CtrlmInput = LoadControl("~/PLCWebCommon/PLCMsgComment.ascx");
            mInput = (PLCMsgComment)CtrlmInput;
            phMsgBoxComments.Controls.Add(mInput);

            if (PLCSession.PLCGlobalCaseKey == "")
            {
                Response.Redirect("~/DashBoard.aspx");
                return;
            }

            if (!PLCSession.CheckUserOption("DELCASE"))
                PLCButtonPanel1.HideDeleteButton();

            if (PLCSessionVars1.GetLabCtrl("USES_DISCOVERY_PACKET").Trim() == "T" && !PLCSessionVars1.CheckUserOption("HIDECADS"))
            {
                bnDiscoveryPacket.Visible = true;
            }

            if (PLCSession.GetLabCtrl("USES_DISTRIBUTION").ToUpper().Trim() == "T")
            {
                btnDistribution.Visible = true;
            }



            if (PLCSession.GetLabCtrl("EDIT_DISTRIBUTION").ToUpper().Trim() == "T")
            {
                btnDistribution.Visible = true;
            }



            if (PLCSession.GetLabCtrl("USES_LAB_OFFENSE").ToUpper().Trim() == "T")
            {
                btnLaboffense.Visible = true;
            }

            SetSeizurePermission();

            string changeDeptCaseText = PLCSession.GetLocalizedTextData("CaseTab_ChangeCase");
            btnChangeCase.Text = string.IsNullOrWhiteSpace(changeDeptCaseText) ? "Change Department/Case Number" : changeDeptCaseText;
            btnChangeCase.Visible = PLCSession.CheckUserOption("DEPTCASECODECHANGE");

            PLCCommonClass.SetSelectedMenuItem(MainMenuTab.CaseTab, (Menu)Master.FindControl("menu_main"));

            // Set page title.
            string caseTitleCacheKey = "CASETITLE@@@" + PLCSessionVars1.PLCGlobalCaseKey;
            PLCSession.PLCGlobalLabCase = PLCDBGlobal.instance.GetLabCaseField(PLCSession.SafeInt(PLCSessionVars1.PLCGlobalCaseKey), "LAB_CASE");
            CacheHelper.DeleteItem(caseTitleCacheKey);
            ((MasterPage)Master).SetCaseTitle("");//AACI 07/27/2010
            ((MasterPage)Master).SetLabCaseHeader();

            if (!IsPostBack)
            {
                PLCDBGlobal.instance.RemoveRecordLocks("TV_LABCASE", PLCSession.PLCGlobalCaseKey, null, null, PLCSession.PLCGlobalAnalyst);

                if (PLCDBGlobalClass.IsCaseInDataBankMode(PLCSessionVars1.PLCGlobalCaseKey))
                {
                    Response.Redirect("~/Tab1CaseCODNA.aspx");
                    return;
                }
                else if (MEIMSHelper.IsMECase(PLCSession.SafeInt(PLCSession.PLCGlobalCaseKey)))
                {
                    Response.Redirect("~/Tab1CaseMEIMS.aspx", true);
                }
                else if(PLCDBGlobalClass.IsLabInHitMode(PLCSession.SafeInt(PLCSession.PLCGlobalCaseKey)))
                {
                    Response.Redirect("~/HitTab.aspx", true);
                }

                Session["SuppMainDataKey"] = null;
                PLCSession.SetDefault("OFFENSE_VIEWNAME", "");
                PLCSession.SetDefault("OFFENSE_VIEWCOND", "");

                GetLabCaseInformation();

                //caselock
                chkCaseLocked.Visible = PLCSessionVars1.CheckUserOption("CASELOCK");
                bool bSuperLocked = false;
                IsCaseLocked = bnTeam.Enabled = PLCDBGlobalClass.isCaseLock(PLCSessionVars1.PLCGlobalCaseKey, ref bSuperLocked);
                if (IsCaseLocked)
                {
                    lCaseLockStatus.Visible = true;
                    if (bSuperLocked)
                        lCaseLockStatus.Text = PLCSession.GetSysPrompt("TAB1Case.lCaseLockStatus.SUPER", "Case is superlocked.");
                    else
                        lCaseLockStatus.Text = PLCSession.GetSysPrompt("TAB1Case.lCaseLockStatus.LOCKED", "Case is locked.");
                }
                else
                    lCaseLockStatus.Visible = false;

                if ((PLCSessionVars1.GetLabCtrl("USES_STORY").Trim() == "T") && (PLCSessionVars1.GetLabCtrl("STORY_TEXT").Trim() != string.Empty))
                {
                    bnSchedule.Text = PLCSession.GetSysPrompt("LABCTRL.STORY_TEXT", PLCSessionVars1.GetLabCtrl("STORY_TEXT").Trim());
                }
                else
                    bnSchedule.Text = PLCSession.GetSysPrompt("TAB1Case.bnSchedule", "Schedule");
                PLCDBPanel1.PLCWhereClause = "where case_key = " + PLCSessionVars1.PLCGlobalCaseKey;
                PLCButtonPanel1.SetBrowseMode();
                PLCDBPanel1.DoLoadRecord();

                PLCDBPanelLabels1.Height = Unit.Parse(PLCSession.GetLabCtrl("CASE_STATUS_PANEL_HEIGHT"));
                PLCDBPanelLabels1.WhereClause = "WHERE CASE_KEY = " + PLCSession.PLCGlobalCaseKey;

                if (PLCDBPanel1.HasPanelRec("TV_LABCASE", "OFFENSE_CODE"))
                {
                    PLCDBPanel.PanelRec prOffense = PLCDBPanel1.GetPanelRecByFieldName("OFFENSE_CODE");
                    _OffenseCodeCondition = prOffense.codecondition;
                }
                UpdateOffenseCategoryOptions();
                UpdateOffenseOptions();

                PLCSession.AddToRecentCases(PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalAnalyst);
                if (PLCSession.PLCNewCaseKey != "")
                {
                    PLCButtonPanel1.ClickEditButton();
                }
                else if (!PLCCommonClass.IsReadOnlyAccess("WEBINQ,RONLYCATAB"))
                {
                    string LockedInfo = "";
                    bool isLocked = PLCDBGlobal.instance.IsRecordLocked("TV_LABCASE", PLCSession.PLCGlobalCaseKey, "-1", "-1", out LockedInfo);
                    PLCButtonPanel1.SetButtonsForLock(isLocked);

                    lPLCLockStatus.Text = LockedInfo;
                    dvPLCLockStatus.Visible = isLocked;
                }

                // Case Label 
                bnCaseLabel.Visible = !PLCSessionVars1.CheckUserOption("HIDECASLBL");

                // Case Jacket
                bnCaseJacket.Visible = !PLCSession.CheckUserOption("RESCASJACK");

                btnRelatedCase.Visible = (PLCSessionVars1.GetLabCtrl("USES_RELATED_CASES") == "T" && !PLCSessionVars1.CheckUserOption("HIDECARC"));

                btnJIMSReport.Visible = PLCSessionVars1.GetLabCtrl("USES_JIMS_INTERFACE") == "T";

                btnFOIA.Visible = PLCSession.CheckUserOption("SHOWFOIA");

                // Case WorkSheet
                btnForms.Visible = (PLCSessionVars1.GetLabCtrl("USES_FORMS") == "T" && !PLCSessionVars1.CheckUserOption("HIDECAFR"));

                // Schedule HIDE_SCHEDULE
                bnSchedule.Visible = (PLCSessionVars1.GetLabCtrl("HIDE_SCHEDULE") != "T" && !PLCSessionVars1.CheckUserOption("HIDECACC"));
                bnSchedule.Style["color"] = IsCaseHasCorrespondence(PLCSessionVars1.PLCGlobalCaseKey) ? "Red" : "Black";

                bnCaseReports.Visible = (PLCSessionVars1.GetLabCtrl("USES_CASE_REPORTS") == "T" && !PLCSessionVars1.CheckUserOption("HIDECACR"));

                // Use Supplements
                bnSupplements.Visible = !PLCSessionVars1.CheckUserOption("HIDESUPP");

                // Reference
                bnReference.Visible = !PLCSession.CheckUserOption("HIDECAREF");

                // Seizure Tracking
                btnSeizureTrack.Visible = PLCSession.GetLabCtrlFlag("USES_SEIZURE_TRACKING").Equals("T");
                if (btnSeizureTrack.Visible)
                    btnSeizureTrack.Visible = !PLCSession.CheckUserOption("HIDESEIZURE");

                // RecordUnlock
                if (!PLCSessionVars1.CheckUserOption("DELLOCKS"))
                    PLCButtonPanel1.SetCustomButtonVisible("RecordUnlock", false);

                //Set Distribution button text color
                btnDistribution.Style["color"] = HasDistribution(PLCSessionVars1.PLCGlobalCaseKey) ? "Red" : "Black";

                //Set discovery packet button text color
                bnDiscoveryPacket.Style["color"] = HasDiscoveryRequests(PLCSessionVars1.PLCGlobalCaseKey) ? "Red" : "Black";

                btnActivity.Visible = PLCSession.GetLabCtrlFlag("ACTIVITY_LINK_IN_CASE") == "L";
                if (PLCSession.GetLabCtrlFlag("ACTIVITY_LINK_IN_CASE") == "T" || 
                    PLCSession.GetLabCtrlFlag("ACTIVITY_LINK_IN_CASE") == "L")
                {
                    PLCQuery queryActivity = new PLCQuery("SELECT * FROM TV_ACTCASES WHERE CASE_KEY = " + PLCSession.PLCGlobalCaseKey);
                    queryActivity.Open();
                    btnActivity.Visible = PLCSession.GetLabCtrlFlag("ACTIVITY_LINK_IN_CASE") == "L" || queryActivity.HasData();
                    btnActivity.Style["color"] = queryActivity.HasData() ? "Red" : "Black";
                    btnActivity.Enabled = CheckUserActLogAccess();
                }

                if (PLCSession.GetLabCtrl("VIEW_CODIS_HITS_BTN").ToUpper().Trim() == "T")
                {
                    btnCodisHits.Visible = true;
                }

                if (PLCSession.GetLabCtrlFlag("SHOW_CASE_ADDRESS_BUTTON") == "T")
                {
                    btnAddressLabel.Visible = true;
                }

                if (PLCSession.GetLabCtrlFlag("SHOW_DATABANK_ERROR_BUTTON").Equals("T"))
                {
                    btnDatabankError.Visible = true;
                }


                btnCaseEmail.Visible = PLCSession.GetLabCtrlFlag("USES_CASE_EMAIL") == "T";

                if (btnCaseEmail.Text.Trim() == string.Empty)
                    btnCaseEmail.Text = "Case Info";

                btnCaseEmail.Style["color"] = PLCDBGlobal.instance.HasCaseEmail(PLCSession.PLCGlobalCaseKey) ? "Red" : "Black";

                btnCCAP.Visible = PLCSession.GetLabCtrlFlag("USES_CCAP").Equals("T");

                if (Session["ShowSupplementsGrid"] != null && Convert.ToBoolean(Session["ShowSupplementsGrid"]))
                {
                    Session["ShowSupplementsGrid"] = null;
                    BindSupplementsGrid();
                    mpeSupplements.Show();
                }

                ShowCaseTypeLabel();

                //NICS permissions
                btnNICS.Visible = PLCSession.GetLabCtrl("MIDEO_INTERFACE") == "T" && PLCSession.CheckUserOption("NICSIMG");

                SetExpungeControls();

                if (Request.QueryString["c"] == PLCSession.PLCGlobalDepartmentCaseNumber)
                {
                    NotifyRetentionMessage = false;
                    PLCButtonPanel1.ClickEditButton();
                }

                SetReadOnlyAccess();

                ResetChangeLabCaseDialog();
            }

            btnReUpload.Enabled = (PLCSession.GetLabCtrlFlag("USES_IMAGE_VAULT_SERVICE").Equals("T") && HasPendingQCImages());

            CheckOffenseLookupSettings();
            ToggleResultsButton();
            SetPanelScript();

            ScriptManager.RegisterStartupScript(this, this.GetType(), "_addtoList", dbpChangeLabCase.GetAddToListScript(), true);
        }

        protected void Page_LoadComplete(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                SetAttachmentsButton("CASE");

                if (Request.QueryString["c"] == PLCSession.PLCGlobalDepartmentCaseNumber)
                {
                    PLCDBPanel1.SetPanelDefaultValuesOnBlankFields();
                }
            }
        }

        private void CheckOffenseLookupSettings()
        {
            string OffenseViewTable = PLCSession.GetDefault("OFFENSE_VIEWNAME");
            string OffenseViewCond = PLCSession.GetDefault("OFFENSE_VIEWCOND");

            if (!string.IsNullOrEmpty(OffenseViewTable))
            {
                PLCDBPanel1.SetPanelCodeTable("OFFENSE_CODE", OffenseViewTable);
                PLCDBPanel1.SetPanelCodeCondition("OFFENSE_CODE", OffenseViewCond);
                return;
            }

            string statCode = "";
            string urn = PLCDBPanel1.getpanelfield("DEPARTMENT_CASE_NUMBER");
            string deptCode = PLCDBPanel1.getpanelfield("DEPARTMENT_CODE");
            if (urn.Length == 18 ) statCode = urn.Substring(15, 3);
            if  ( !string.IsNullOrEmpty(statCode) && (statCode != "999") && PLCDBGlobalClass.IsSheriffDepartment(deptCode) && PLCDBGlobalClass.looksLikeURN(urn))
            {
                PLCSession.SetDefault("OFFENSE_VIEWNAME","CV_OFFENSE_STAT");
                PLCSession.SetDefault("OFFENSE_VIEWCOND", (string.IsNullOrEmpty(_OffenseCodeCondition) ? "" : _OffenseCodeCondition + " AND ") + string.Format("STAT_CODE = '{0}'", statCode));
            }
            else
            {
                PLCSession.SetDefault("OFFENSE_VIEWNAME", "TV_OFFENSE");
                PLCSession.SetDefault("OFFENSE_VIEWCOND", _OffenseCodeCondition);
            }

            PLCDBPanel1.SetPanelCodeTable("OFFENSE_CODE", PLCSession.GetDefault("OFFENSE_VIEWNAME"));
            PLCDBPanel1.SetPanelCodeCondition("OFFENSE_CODE", PLCSession.GetDefault("OFFENSE_VIEWCOND"));
        }



        private void ShowCaseTypeLabel()
        {
            //case type label

            if (PLCSession.GetLabCtrl("USES_CASE_TYPE_CHARGE_GROUPS") == "T")
            {
                string caseType = PLCDBGlobal.instance.GetCaseType(PLCSession.PLCGlobalCaseKey);
                lblCaseType.Text = "<br />" + caseType;
                lblCaseType.Visible = true;
            }
            else
                lblCaseType.Visible = false;
        }

        private void EnableDisableFields()
        {
            PLCDBPanel1.SetMyFieldMode("DEPARTMENT_CODE", (!CanChangeURN || IsLinkedToPrelog()));
            PLCDBPanel1.SetMyFieldMode("DEPARTMENT_CASE_NUMBER", !CanChangeURN || IsLinkedToPrelog());

            if (PLCSessionVars1.GetLabCtrl("TRACK_IOR_CHANGE") == "T")
            {
                if (PLCDBPanel1.getpanelfield("CASE_MANAGER").Length > 0)
                {
                    PLCDBPanel1.SetMyFieldMode("CASE_MANAGER", !CanChangeIOR);
                }

                if (PLCDBPanel1.getpanelfield("CASE_ANALYST").Length > 0)
                {
                    PLCDBPanel1.SetMyFieldMode("CASE_ANALYST", !CanChangeIOR);
                }
            }
        }

        private string _freeFormDeptPicMask() {
            return PLCSessionVars1.GetLabCtrl("FREEFORM_CASE_MASK");
        }

        private void SetFreeFormDeptPicMask()
        {
            PLCDBPanel1.SetMaskEditExtenderValue("DEPARTMENT_CASE_NUMBER", _freeFormDeptPicMask());

        }


        private Boolean Validate_LASD_CaseNumber_Format()
        {

            string deptCode = PLCDBPanel1.getpanelfield("DEPARTMENT_CODE");
            string theMask = _freeFormDeptPicMask();
            string theCaseNum = PLCDBPanel1.getpanelfield("DEPARTMENT_CASE_NUMBER").Trim();
            string refType = PLCDBPanel1.getpanelfield("CASE_REFERENCE_TYPE");

            if ((refType != Constants.REFERENCE_TYPE_MASTERCASE) && (refType != Constants.REFERENCE_TYPE_URN) && (refType != Constants.REFERENCE_TYPE_DEPTCASENUMBER))
            {
                // its a citation or something to book...
                return true;
            }

            if (PLCSessionVars1.GetLabCtrl("USES_DATE_RANGE_PICTURE_MASK") == "T")
            {
                theMask = PLCDBGlobalClass.GetProcPictureMask(deptCode, false);
            }
            else if (PLCSession.GetLabCtrl("USE_DEPTNAME_PICTURE_MASK") == "T")
            {
                theMask = PLCDBGlobal.instance.GetDeptPictureMask(deptCode);
            }

            if ( (theMask == _freeFormDeptPicMask()) || (string.IsNullOrEmpty(theMask)) ) return true;

            theMask = theMask.Trim();

            if (theMask.Length != theCaseNum.Length)
            {

                mbox.ShowMsg("Invalid Case Number Format","The length of the case number is incorrect", 1);
                return false;

            }

            return true;


        }


        private void setAllowEdit(String tbl, String fld, Boolean ae)
        {
            PLCDBPanel.PanelRec pr = null;
            pr = PLCDBPanel1.GetPanelRecByFieldName(fld);
            if (pr != null) pr.AllowEdit = ae;

        }

        private void AssignDeptPicMask()
        {

            // if I am not editing then dont need to worry about the masks.
            if (PLCDBPanel1.IsBrowseMode) return;
            if (PLCDBPanel1.IsSavingMode) return;

            //if I can't edit case number, just set to readonly and get out of here...

            setAllowEdit("TV_LABCASE", "DEPARTMENT_CODE", (!IsLinkedToPrelog() && CanChangeURN));
            setAllowEdit("TV_LABCASE", "DEPARTMENT_CASE_NUMBER", (!IsLinkedToPrelog() && CanChangeURN));

            if (!CanChangeURN)
            {
                PLCDBPanel1.SetMyFieldModeEnableDisable("CASE_REFERENCE_TYPE", false);
                return;
            }

            // Load up the defaults.
            string theMask = _freeFormDeptPicMask();

            string currdeptCode = PLCDBPanel1.getpanelfield("DEPARTMENT_CODE");
            string prevdeptCode = OriginalDepartmentCode;

            Boolean currIsInside = isInsideDepartment(currdeptCode);
            Boolean prevIsInside = isInsideDepartment(prevdeptCode);

            string currRefType = PLCDBPanel1.getpanelfield("CASE_REFERENCE_TYPE");
            string prevRefType = OriginalRefType;

            string currCaseNum = PLCDBPanel1.getpanelfield("DEPARTMENT_CASE_NUMBER");
            string prevCaseNum = PLCDBPanel1.GetOriginalValue("DEPARTMENT_CASE_NUMBER");

            if (string.IsNullOrEmpty(theMask)) theMask = _freeFormDeptPicMask();

            // this is for LA
            if (PLCSession.GetLabCtrl("USES_RD") == "T")
            {

                // the department code was changed
                if (currdeptCode != prevdeptCode)
                {

                    // I am switching from inside to outside or vice versa
                    if (currIsInside != prevIsInside)
                    {

                        OriginalDepartmentCode = currdeptCode;

                        //changing from outside to inside
                        if (currIsInside)
                        {
                            PLCDBPanel1.setpanelfield("CASE_REFERENCE_TYPE", "");
                            PLCDBPanel1.setpanelfield("DEPARTMENT_CASE_NUMBER", "");
                            theMask = _freeFormDeptPicMask();
                            PLCDBPanel1.SetMaskEditExtenderValue("DEPARTMENT_CASE_NUMBER", theMask);
                            PLCDBPanel1.SetMyFieldModeEnableDisable("CASE_REFERENCE_TYPE", true);
                            return;
                        }
                        else
                        {
                            //changing from inside to outside
                            PLCDBPanel1.setpanelfield("DEPARTMENT_CASE_NUMBER", "");
                            PLCDBPanel1.setpanelfield("CASE_REFERENCE_TYPE", Constants.REFERENCE_TYPE_DEPTCASENUMBER);
                            OriginalRefType = Constants.REFERENCE_TYPE_DEPTCASENUMBER;
                            theMask = PLCDBGlobalClass.GetProcPictureMask(currdeptCode, false);
                            if (String.IsNullOrEmpty(theMask) ) theMask = _freeFormDeptPicMask();
                            PLCDBPanel1.SetMaskEditExtenderValue("DEPARTMENT_CASE_NUMBER", theMask);
                            PLCDBPanel1.SetMyFieldModeEnableDisable("CASE_REFERENCE_TYPE", false);
                            PLCDBPanel1.ResetOriginalValues();
                            return;

                        }

                    }
                    else
                    {


                        // Switching between 2 outside agencies
                        if (!currIsInside)
                        {
                            OriginalDepartmentCode = currdeptCode;
                            OriginalRefType = currRefType;
                            PLCDBPanel1.setpanelfield("DEPARTMENT_CASE_NUMBER", "");
                            PLCDBPanel1.setpanelfield("CASE_REFERENCE_TYPE", Constants.REFERENCE_TYPE_DEPTCASENUMBER);
                            PLCDBPanel1.SetMyFieldModeEnableDisable("CASE_REFERENCE_TYPE", false);
                            if (PLCSessionVars1.GetLabCtrl("USES_DATE_RANGE_PICTURE_MASK") == "T")
                                theMask = PLCDBGlobalClass.GetProcPictureMask(currdeptCode, false);
                            if (String.IsNullOrEmpty(theMask)) theMask = _freeFormDeptPicMask();
                            PLCDBPanel1.SetMaskEditExtenderValue("DEPARTMENT_CASE_NUMBER", theMask);
                            return;
                        }
                        else // switching from outside  2 inside
                        {
                            OriginalDepartmentCode = currdeptCode;
                            OriginalRefType = null;
                            PLCDBPanel1.setpanelfield("DEPARTMENT_CASE_NUMBER", "");
                            PLCDBPanel1.setpanelfield("CASE_REFERENCE_TYPE","");
                            PLCDBPanel1.SetMyFieldModeEnableDisable("CASE_REFERENCE_TYPE",true);
                            if (PLCSessionVars1.GetLabCtrl("USES_DATE_RANGE_PICTURE_MASK") == "T")
                                theMask = _freeFormDeptPicMask();
                            PLCDBPanel1.SetMaskEditExtenderValue("DEPARTMENT_CASE_NUMBER", theMask);
                            return;
                        }

                    }


                }


                // I am changing the reference type, only inside can do this.
                if (currRefType != prevRefType)
                {

                    OriginalRefType = currRefType;

                    if (currRefType == Constants.REFERENCE_TYPE_URN)
                    {

                        PLCDBPanel1.setpanelfield("DEPARTMENT_CASE_NUMBER", "");
                        PLCDBPanel1.SetMaskEditExtenderValue("DEPARTMENT_CASE_NUMBER", "999-99999-9999-999");
                        PLCDBPanel1.SetMyFieldModeEnableDisable("CASE_REFERENCE_TYPE", true);
                        return;

                    }
                    else
                    {

                        if (currIsInside)
                        {
                            PLCDBPanel1.setpanelfield("DEPARTMENT_CASE_NUMBER", "");
                            PLCDBPanel1.SetMaskEditExtenderValue("DEPARTMENT_CASE_NUMBER",_freeFormDeptPicMask());
                            PLCDBPanel1.SetMyFieldModeEnableDisable("CASE_REFERENCE_TYPE", true);
                            return;
                        }
                        else
                        {   //should never get here because outside cannot change reftype
                            PLCDBPanel1.setpanelfield("DEPARTMENT_CASE_NUMBER", "");
                            PLCDBPanel1.setpanelfield("CASE_REFERENCE_TYPE",Constants.REFERENCE_TYPE_DEPTCASENUMBER);
                            theMask = PLCDBGlobalClass.GetProcPictureMask(currdeptCode, false);
                            if (String.IsNullOrEmpty(theMask)) theMask = _freeFormDeptPicMask();
                            PLCDBPanel1.SetMaskEditExtenderValue("DEPARTMENT_CASE_NUMBER",theMask);
                            PLCDBPanel1.SetMyFieldModeEnableDisable("CASE_REFERENCE_TYPE", false);
                            return;
                        }

                    }

                    //we should never get here
                    //  return;

                }

                //Changing the case number
                if (currCaseNum != prevCaseNum)
                {
                    // I'm an inside department changing it to an URN or to a master case number
                    if (currIsInside)
                    {

                        if ((currRefType == Constants.REFERENCE_TYPE_URN) || (currRefType == Constants.REFERENCE_TYPE_MASTERCASE))
                        {
                            String tmpRefType = Constants.REFERENCE_TYPE_URN;
                            if (currCaseNum.EndsWith("-999")) tmpRefType = Constants.REFERENCE_TYPE_MASTERCASE;
                            PLCDBPanel1.setpanelfield("CASE_REFERENCE_TYPE", tmpRefType);
                            PLCDBPanel1.SetMyFieldModeEnableDisable("CASE_REFERENCE_TYPE", true);
                            PLCDBPanel1.SetMaskEditExtenderValue("DEPARTMENT_CASE_NUMBER", "999-99999-9999-999");
                            OriginalRefType = tmpRefType;
                            return;
                        }
                        else
                        {
                            //non urn, its freeform.
                            return;

                        }
                    }
                    else
                    {
                        // i do care about outside case numbers being edited.
                        PLCDBPanel1.SetMyFieldModeEnableDisable("CASE_REFERENCE_TYPE", false);
                        PLCDBPanel1.setpanelfield("CASE_REFERENCE_TYPE", Constants.REFERENCE_TYPE_DEPTCASENUMBER);
                        if (PLCSessionVars1.GetLabCtrl("USES_DATE_RANGE_PICTURE_MASK") == "T")
                            theMask = PLCDBGlobalClass.GetProcPictureMask(currdeptCode, false);
                        if (String.IsNullOrEmpty(theMask)) theMask = _freeFormDeptPicMask();
                        PLCDBPanel1.SetMaskEditExtenderValue("DEPARTMENT_CASE_NUMBER", theMask);
                        return;

                    }

                    //we should never get here...
                    //    return;

                }


                //i haven't changed anything yet, so what are the defaults for the original record

                if (currIsInside)
                {
                    if ((currRefType == Constants.REFERENCE_TYPE_URN) || (currRefType == Constants.REFERENCE_TYPE_MASTERCASE))
                    {
                        theMask = "999-99999-9999-999";
                        String tmpRefType = Constants.REFERENCE_TYPE_URN;
                        if (currCaseNum.EndsWith("-999")) tmpRefType = Constants.REFERENCE_TYPE_MASTERCASE;
                        PLCDBPanel1.setpanelfield("CASE_REFERENCE_TYPE", tmpRefType);
                        PLCDBPanel1.SetMaskEditExtenderValue("DEPARTMENT_CASE_NUMBER", theMask);
                        PLCDBPanel1.SetMyFieldModeEnableDisable("CASE_REFERENCE_TYPE",false);
                        OriginalRefType = tmpRefType;
                        return;
                    }
                    else
                    {

                        PLCDBPanel1.SetMyFieldModeEnableDisable("CASE_REFERENCE_TYPE",true);
                        theMask = _freeFormDeptPicMask();
                        PLCDBPanel1.SetMaskEditExtenderValue("DEPARTMENT_CASE_NUMBER", theMask);
                        return;
                    }

                    //we should never get here...
                    //  return;


                }
                else
                {
                    PLCDBPanel1.SetMyFieldModeEnableDisable("CASE_REFERENCE_TYPE", false);
                    PLCDBPanel1.setpanelfield("CASE_REFERENCE_TYPE", Constants.REFERENCE_TYPE_DEPTCASENUMBER);
                    if (PLCSessionVars1.GetLabCtrl("USES_DATE_RANGE_PICTURE_MASK") == "T")
                        theMask = PLCDBGlobalClass.GetProcPictureMask(currdeptCode, false);
                    if (String.IsNullOrEmpty(theMask)) theMask = _freeFormDeptPicMask();
                    PLCDBPanel1.SetMaskEditExtenderValue("DEPARTMENT_CASE_NUMBER", theMask);
                    return;

                }

                //we should never get here...
                // return;


            }

            else // non LA
            {
                if (PLCSessionVars1.GetLabCtrl("USES_DATE_RANGE_PICTURE_MASK") == "T")
                    theMask = PLCDBGlobalClass.GetProcPictureMask(currdeptCode, false);
                else if (PLCSession.GetLabCtrl("USE_DEPTNAME_PICTURE_MASK") == "T")
                    theMask = PLCDBGlobal.instance.GetDeptPictureMask(currdeptCode);

                if (string.IsNullOrEmpty(theMask)) theMask = _freeFormDeptPicMask();

                PLCDBPanel1.SetMaskEditExtenderValue("DEPARTMENT_CASE_NUMBER", theMask);
            }

        }

        protected void PLCDBPanel1_PLCDBPanelCodeHeadChanged(object sender, PLCDBPanelCodeHeadChangedEventArgs e)
        {
            //More than one field has the TV_DEPTNAME table as its code table. So we need to test the field name.
            if ((e.CodeTable.Contains("DEPTNAME")) && (e.FieldName == "DEPARTMENT_CODE"))
            {
                AssignDeptPicMask();
            }

            if (e.FieldName == "CASE_REFERENCE_TYPE")
            {
                AssignDeptPicMask();
            }

            if (e.CodeTable.Contains("OFFENSE") && (e.FieldName == "OFFENSE_CODE"))
            {
                UpdateOffenseCategoryOptions();
            }

            if (e.FieldName == "OFFENSE_CATEGORY")
            {
                UpdateOffenseOptions();
            }
        }

        protected string getdeptdesc(string dcode)
        {
            return PLCSession.GetCodeDesc("DEPTNAME", dcode);
        }
        protected Boolean thisCaseIsAnURN(string ckey)
        {
            PLCQuery qry = new PLCQuery("SELECT CASE_REFERENCE_TYPE FROM TV_LABCASE WHERE CASE_KEY = " + ckey);
            qry.Open();
            return ((qry.FieldByName("CASE_REFERENCE_TYPE") == "U") || (qry.FieldByName("CASE_REFERENCE_TYPE") == "M"));
        }


        private Boolean isInsideDepartment(String deptCode)
        {
            if (PLCSession.GetLabCtrl("USES_RD") == "T")
            {
                String qry = String.Format("SELECT LAB_CODE FROM TV_DEPTNAME where DEPARTMENT_CODE = '{0}'", deptCode);

                PLCQuery qryGetDepartment = CacheHelper.OpenCachedSqlReadOnly(qry);
                if (qryGetDepartment.HasData())
                    return (qryGetDepartment.FieldByName("LAB_CODE") != "O");

            }
            return false;
        }


        //Call this function when you are changing a case number and you want to make sure it will not conflict with another case
        protected string IsCaseNumberAlreadyInUse_LASD(string existingCaseKey, string newdept, string newnum)
        {

            Boolean isInside = false;
            Boolean isUrnFormat = false;

            // An "Inside Department" is one of the LASD Sheriff's Office Stations. The row in TV_DEPTNAME for these will not have an "O" in the LAB_CODE field.
            isInside =  isInsideDepartment(newdept);


            //An "URN" is the case number for an "Inside Department". A can number canonly be an URN for aninstde department.
            //Special Rules apply to "URNS" "URN is NOT interchangable with "Department Case Number"
            if ( isInside )
                isUrnFormat = PLCDBGlobal.instance.looksLikeURN(newnum);

            //If its not an URN we handle it like any other department_case_number
            if (!isUrnFormat)
            {
                return IsCaseNumberAlreadyInUse_General(existingCaseKey,newdept,newnum);
            }

            //Not that we know the new case number will be an URN, lets see if it conflict with another case...
            string sRetentionYr, sReportingYr, sSeq, stationID, sPatrol, sStatCode, rdCode = "";
            PLCDBGlobal.instance.ParseSheriffCaseUrn(newnum, out sRetentionYr, out sReportingYr, out sSeq, out stationID, out sPatrol, out sStatCode, out rdCode);

            string thisDeptCode = PLCDBGlobal.instance.GetRptDstrctDeptCode(rdCode);

            //The department Code pulled from the RD must match the deparment code passed in, otherwise its an error
            if (thisDeptCode != newdept)
            {
                return "The selected ORI (" + getdeptdesc(newdept) + ") does not match the department configured for this RD: " + rdCode + " (" + getdeptdesc(thisDeptCode) + ")";
            }


            if (string.IsNullOrEmpty(thisDeptCode))
            {
                return "No ORI is defined for this RD: " + rdCode;
            }

            PLCQuery qryCaseCount = new PLCQuery();
            qryCaseCount.SQL = string.Format("select COUNT(*) CNT from TV_LABCASE where (CASE_KEY <> {0}) AND (DEPARTMENT_CODE = '{1}') and (LARCIS_YEAR = '{2}') and (LARCIS_SEQUENTIAL = '{3}')", existingCaseKey, thisDeptCode, sReportingYr, sSeq);
            qryCaseCount.Open();

            int c = qryCaseCount.iFieldByName("CNT");
            if (c > 0)
            {
                return "This case number: " + newnum + ", is already in use by: " + PLCSession.GetCodeDesc("TV_DEPTNAME", thisDeptCode);
            }

            // if we get this far, we have found an acceptable case number
            return "";

        }

        protected string IsCaseNumberAlreadyInUse_General(string existingCaseKey, string newdept, string newnum)
        {

            int caseCount = 0;
            string qryStr = "";
            string errorMsg = "";
            string caseRefType = "";

            // USES_CASE_REFERENCE_TYPE is a new feature tha allow a case number to be classified     
            // We will need to consider the case_reference_type, that is a citation. jail booking number, or a regular case number
            // it is acceptpable that within the same department a jail booking number could be the same as a citation number, and these numbers would reference different cases.
            // These are defined in the CASEREF table and stored in the LABCASE.CASE_REFERENCE_TYPE column
            //Examples:
            //   "D" - Department case number
            //   "CIT" = Citation Number

            if (PLCSession.GetLabCtrl("USES_CASE_REFERENCE_TYPE") == "T")
            {
                PLCQuery qryGetCaseRefType = new PLCQuery(string.Format("select CASE_REFERENCE_TYPE FROM TV_LABCASE WHERE CASE_KEY = {0}", existingCaseKey));
                qryGetCaseRefType.Open();
                if (qryGetCaseRefType.HasData())
                    caseRefType = qryGetCaseRefType.FieldByName("CASE_REFERENCE_TYPE");

            }

            if ((PLCSession.GetLabCtrl("USES_RD") == "T") && ((caseRefType == Constants.REFERENCE_TYPE_URN) || (caseRefType == Constants.REFERENCE_TYPE_MASTERCASE)))
                caseRefType = Constants.REFERENCE_TYPE_DEPTCASENUMBER;


            if (!string.IsNullOrEmpty(caseRefType))
            {
                qryStr = string.Format("SELECT COUNT(*) CNT FROM TV_LABCASE WHERE CASE_KEY <> {0} and  CASE_REFERENCE_TYPE = '{1}' AND DEPARTMENT_CODE = '{2}' AND DEPARTMENT_CASE_NUMBER = '{3}'", existingCaseKey, caseRefType, newdept, newnum);
                errorMsg = "Case numbers may not be duplicated within the same department and reference type.";
            }
            else
            {
                qryStr = string.Format("SELECT COUNT(*) CNT FROM TV_LABCASE WHERE CASE_KEY <> {0} and  DEPARTMENT_CODE = '{1}' AND DEPARTMENT_CASE_NUMBER = '{2}'", existingCaseKey, newdept, newnum);
                errorMsg = "Case numbers may not be duplicated within the same department.";
            }

            PLCQuery qryCaseCount = new PLCQuery();
            qryCaseCount.SQL = PLCSessionVars1.FormatSpecialFunctions(qryStr);
            qryCaseCount.Open();
            caseCount = qryCaseCount.iFieldByName("CNT");
            if (caseCount > 0)
                return errorMsg;
            else
                return "";

        }

        protected string IsCaseNumberAlreadyInUse(string existingCaseKey, string newdept, string newnum)
        {

            if (PLCSessionVars1.GetLabCtrl("USES_RD") == "T") // Currently, is for LosAngeles County Only
            {
                return IsCaseNumberAlreadyInUse_LASD(existingCaseKey, newdept, newnum);
            }
            else
            {
                return IsCaseNumberAlreadyInUse_General(existingCaseKey, newdept, newnum);
            }
        }

        protected void NotifyRetentionReviewChanged()
        {

            Boolean pleaseNotify = false;
            String ChangedFields = "";
            String rtCaseFields = PLCSession.GetLabCtrl("RETENTION_TRACKING_CASE_FIELDS");
            String rtMessage = PLCSession.GetLabCtrl("RETENTION_TRACKING_CASE_MESSAGE");
            if (String.IsNullOrWhiteSpace(rtCaseFields)) return;
            if (String.IsNullOrWhiteSpace(rtMessage)) return;

            String [] fList = rtCaseFields.Split(',');

            foreach (String fld in fList)
            {

                Boolean changed = (PLCDBPanel1.getpanelfield(fld) != PLCDBPanel1.GetOriginalValue(fld));

                pleaseNotify = changed || pleaseNotify;

                if (changed)
                {

                    if (!String.IsNullOrWhiteSpace(ChangedFields)) ChangedFields += ", ";
                    ChangedFields += PLCDBPanel1.GetFieldPrompt(fld);
                }

            }

            if (!pleaseNotify) return;

            rtMessage = rtMessage.Replace("{FIELDS}",ChangedFields);
            rtMessage = rtMessage.Replace("{fields}", ChangedFields);


            if (ViewState["PostSaveMessage"] == null)
                ViewState.Add("PostSaveMessage", rtMessage);
            else
            {
                string message = ViewState["PostSaveMessage"].ToString();
                message += "<br /><br />" + rtMessage;
                ViewState["PostSaveMessage"] = message;
            }

        }


        protected void PLCButtonPanel1_PLCButtonClick(object sender, PLCCONTROLS.PLCButtonClickEventArgs e)
        {
            if (e.button_name == "EDIT")
            {
                if (e.button_action == "BEFORE")
                {
                    OriginalRefType = null;
                    OriginalDepartmentCode = null;
                    OriginalInvestigatingAGency = PLCDBPanel1.getpanelfield("INVESTIGATING_AGENCY");

                    string LockedInfo = "";
                    if (PLCDBGlobal.instance.IsRecordLocked("TV_LABCASE", PLCSession.PLCGlobalCaseKey, "-1", "-1", out LockedInfo))
                    {
                        e.button_canceled = true;
                        PLCButtonPanel1.SetButtonsForLock(true);

                        mbox.ShowMsg("Record Lock", PLCSession.GetSysPrompt("Tab1Case.RECORD_LOCK", "Case locked by another user for editing.") + "<br/>" + LockedInfo, 1);
                        lPLCLockStatus.Text = LockedInfo;
                        dvPLCLockStatus.Visible = true;
                    }
                    else
                        PLCDBGlobal.instance.LockUnlockRecord("TV_LABCASE", PLCSession.PLCGlobalCaseKey, "-1", "-1", true);
                }
                else if (e.button_action == "AFTER")
                {
                    EnableButtonControls(false, false);
                    if (PLCDBPanel1.getpanelfield("CASE_MANAGER") != "")
                        PLCDBPanel1.SetMyFieldModeEnableDisable("CASE_OFFICER", false);

                    AssignDeptPicMask();

                    EnableDisableFields();

                    // Enable lab case change button.
                    SetLabCaseChangeEnableScript(true);

                    if (!IsValidURNAndOffense() ) return;
                }
            }
            
            if ((e.button_name == "DELETE") && (e.button_action == "BEFORE"))
            {
                if (!ValidateDeleteCase())
                {
                    e.button_canceled = true;
                    return;
                }
            }

            if ((e.button_name == "SAVE") && (e.button_action == "AFTER"))
            {

               if(NotifyRetentionMessage)
                    NotifyRetentionReviewChanged();

                NotifyRetentionMessage = true;

                OriginalRefType = null;
                OriginalDepartmentCode = null;


                if (referenceUpdate)
                {

                    //saving the old citation number reference needs to happen before save....
                    PLCDBGlobalClass.CreateSystemGeneratedReferences(PLCSessionVars1.PLCGlobalCaseKey);
                    if (PLCSessionVars1.GetLabCtrl("AUTO_CROSS_REFERENCING") == "T")
                    {
                        PLCDBGlobalClass.MultiCrossReference(PLCSessionVars1.PLCGlobalCaseKey);
                    }
                }

                PLCDBPanelLabels1.Reload();
                referenceUpdate = false;
            }



            if ((e.button_name == "SAVE") && (e.button_action == "BEFORE"))
            {
                string LockedInfo = "";
                referenceUpdate = false;

                //has the department case number changed?
                string originalValue = PLCDBPanel1.GetOriginalValue("DEPARTMENT_CASE_NUMBER");
                originalValue = originalValue.ToUpper();

                string newValue = PLCDBPanel1.getpanelfield("DEPARTMENT_CASE_NUMBER");
                newValue = newValue.ToUpper();

                if (originalValue != newValue) referenceUpdate = true;

                if (newValue != PLCDBPanel1.getpanelfield("DEPARTMENT_CASE_NUMBER"))
                {
                    PLCDBPanel1.setpanelfield("DEPARTMENT_CASE_NUMBER",newValue);
                }


                if (referenceUpdate)
                {

                    string thisvalue = "";

                    if (PLCSession.GetLabCtrl("USES_RD") == "T") // this is for LA only.
                    {
                        //Here we promote the old system generated references to be visible. This is needed if we changed the department case number to become an URN.
                        PLCQuery qryScanSysGen = new PLCQuery();
                        qryScanSysGen.SQL = "select * from TV_LABREF where CASE_KEY = " + PLCSession.PLCGlobalCaseKey + " AND SYSTEM_GENERATED = 'T' AND REFERENCE_TYPE NOT IN ('M','U','D','L')";
                        qryScanSysGen.Open();
                        while (!qryScanSysGen.EOF())
                        {
                            thisvalue = qryScanSysGen.FieldByName("REFERENCE");
                            if (originalValue == thisvalue )
                            {
                                qryScanSysGen.Edit();
                                qryScanSysGen.SetFieldValue("SYSTEM_GENERATED", "F");
                                qryScanSysGen.Post("TV_LABREF", 5, 3);
                            }
                            qryScanSysGen.Next();
                        }

                    }
                }

                //has the lab case number changed
                originalValue = PLCDBPanel1.GetOriginalValue("LAB_CASE");
                newValue = PLCDBPanel1.getpanelfield("LAB_CASE");
                if (originalValue != newValue) referenceUpdate = true;

                originalValue = PLCDBPanel1.GetOriginalValue("DEPARTMENT_CODE");
                newValue = PLCDBPanel1.getpanelfield("DEPARTMENT_CODE");
                Boolean CheckNumber = (originalValue != newValue) || referenceUpdate;


                if (CheckNumber)
                {
                    //The IsCaseNumberAlreadyInUse function is meant to check for existance of a case with the "same" case number. It function is to prevent a user from changing 
                    // the case number of a case to create a "duplicate" case number.
                    String CaseInUseMessage =
                        IsCaseNumberAlreadyInUse(PLCSession.PLCGlobalCaseKey,PLCDBPanel1.getpanelfield("DEPARTMENT_CODE"),PLCDBPanel1.getpanelfield("DEPARTMENT_CASE_NUMBER"));
                    //String ValidCaseNumberMessage = CheckValidCaseNumber(PLCDBPanel1.getpanelfield("DEPARTMENT_CODE"),PLCDBPanel1.getpanelfield("DEPARTMENT_CASE_NUMBER"));
                    if (!string.IsNullOrEmpty(CaseInUseMessage))
                    {
                        e.button_canceled = true;
                        mbox.ShowMsg("Invalid Case Number", CaseInUseMessage, 1);
                        return;

                    }


                    //------------------------------------------------------------------------
                    //  This is for LASD only.
                    //  Right now this basically just checks the length of the case number.
                    //------------------------------------------------------------------------
                    if (PLCSession.GetLabCtrl("USES_RD") == "T" )
                    {
                        if (!Validate_LASD_CaseNumber_Format())
                        {
                            e.button_canceled = true;
                            return;
                        }
                    }


                }

                string URNNarrative;
                string auditText = PrepareCaseNarrative(out URNNarrative);

                if (PLCDBGlobal.instance.IsRecordLocked("TV_LABCASE", PLCSession.PLCGlobalCaseKey, "-1", "-1", out LockedInfo))
                {
                    e.button_canceled = true;
                    PLCButtonPanel1.ClickCancelButton();
                    PLCButtonPanel1.SetButtonsForLock(true);

                    mbox.ShowMsg("Record Lock", PLCSession.GetSysPrompt("Tab1Case.RECORD_LOCK", "Case locked by another user for editing.") + "<br/>" + LockedInfo, 1);
                    lPLCLockStatus.Text = LockedInfo;
                    dvPLCLockStatus.Visible = true;
                    return;
                }

                if (!IsValidURNAndOffense())
                {
                    e.button_canceled = true;
                    return;
                }

                // Check if user needs to state reason for updating the case record
                bool hasChanges = PLCDBPanel1.HasChanges(PLCSession.GetLabCtrl("LOG_BLANK_TO_DATA_CHANGES") == "T");
                string reasonText = "";

                bool requireReasonForChange = PLCSession.GetLabCtrlFlag("REQUIRE_REASON_FOR_CHANGE").Equals("T");
                bool logEditsToNarative = PLCSession.GetLabCtrlFlag("LOG_EDITS_TO_NARRATIVE").Equals("T");
                if (logEditsToNarative || requireReasonForChange)
                {
                    if (hdnConfirmUpdate.Value.Trim().Length > 0)
                    {
                        if (logEditsToNarative)
                        {
                            SaveConfirmUpdate(auditText);
                            if (URNNarrative != "")
                            {
                                SaveURNChangeNarrative(URNNarrative);
                            }
                        }
                        reasonText = hdnConfirmUpdate.Value;
                        hdnConfirmUpdate.Value = "";
                        bnSchedule.Style["color"] = IsCaseHasCorrespondence(PLCSessionVars1.PLCGlobalCaseKey) ? "Red" : "Black";
                    }
                    else if (hasChanges)
                    {
                        if (requireReasonForChange)
                        {
                            mInput.ShowMsg("Case update reason", "Please enter the reason for your changes", 0, txtConfirmUpdate.ClientID, btnConfirmUpdate.ClientID, "Save", "Cancel");
                            e.button_canceled = true;
                            return;
                        }
                        else if (logEditsToNarative)
                        {
                            SaveConfirmUpdate(auditText);
                            if (URNNarrative != "")
                            {
                                SaveURNChangeNarrative(URNNarrative);
                            }
                            bnSchedule.Style["color"] = IsCaseHasCorrespondence(PLCSessionVars1.PLCGlobalCaseKey) ? "Red" : "Black";
                        }
                    }
                }

                if (hasChanges)
                {
                    PLCDBGlobalClass.MarkAssignmentsForRegeneration(PLCSessionVars1.PLCGlobalCaseKey, "", "LABCASE", URNNarrative + auditText);
                    PLCDBPanel1.ReasonChangeKey = PLCDBGlobalClass.GenerateReasonChange("CASE TAB SAVE", reasonText);
                }
            }

            if ((e.button_name == "SAVE") && (e.button_action == "AFTER"))
            {
                try
                {
                    string caseTitleCacheKey = "CASETITLE@@@" + PLCSessionVars1.PLCGlobalCaseKey;
                    CacheHelper.DeleteItem(caseTitleCacheKey);
                    ((MasterPage)Master).SetCaseTitle("");
                }
                catch
                {
                }

                if (PLCSessionVars1.CheckUserOption("CASELOCK") && IsCaseLocked != chkCaseLocked.Checked) //if user has permission and checkbox value has been changed
                    UpdateCaseLock(chkCaseLocked.Checked, PLCDBPanel1.ReasonChangeKey);

                PLCDBGlobal.instance.LockUnlockRecord("TV_LABCASE", PLCSession.PLCGlobalCaseKey, "-1", "-1", false);

                if (PLCSessionVars1.GetLabCtrl("USES_18080_CALC") == "T")
                {
                    SyncLabAssignDueDate();
                }

                // on new case, go to submission tab add submission record
                if (PLCSessionVars1.PLCNewCaseKey != "")
                {
                    Response.Redirect("~/TAB2Submissions.aspx");
                }

                PLCDBGlobalClass.UpdateSheriffCaseFields(PLCDBPanel1.getpanelfield("DEPARTMENT_CODE"), PLCDBPanel1.getpanelfield("DEPARTMENT_CASE_NUMBER"), PLCDBPanel1.ReasonChangeKey);

                if (PLCSessionVars1.GetLabCtrl("SUB_TAB_SYNC") == "T")
                {
                    SyncLabSubOffenseInfo();
                }

                if (PLCSession.GetLabCtrlFlag("DISABLE_SUBMISSION_CASE_SYNC") != "T")
                    UpdateFirstSubmission();

                if(PLCSession.GetLabCtrlFlag("CASEINFO_CHECK_CASEDIST_RECORD") == "T")
                {
                    UpdateCasedistOfficer();
                }

                //PLCDBPanel1.SetMyFieldModeEnableDisable("CASE_OFFICER", true);
                //EnableDisableFields(true);
                PLCButtonPanel1.SetButtonsForLock(false);
                EnableButtonControls(true, false);
                ToggleResultsButton();

                //if OFFENSE_CODE has been changed, update case type label
                if (PLCDBPanel1.getpanelfield("OFFENSE_CODE") != PLCDBPanel1.GetOriginalValue("OFFENSE_CODE"))
                    ShowCaseTypeLabel();

                // Disable lab case change button.
                SetLabCaseChangeEnableScript(false);

                if (PLCSession.GetLabCtrl("USES_LAB_OFFENSE").ToUpper().Trim() == "T")
                    UpdateLabOffenseCodes();

                GetLabCaseInformation();
                PLCSession.SetDefault("IS_PARTIAL_CASE", "");

                PLCDBPanel1.ReasonChangeKey = 0;
                PLCDBPanel1.DoLoadRecord();
            }


            if ((e.button_name == "CANCEL") && (e.button_action == "BEFORE"))
            {
                bool isPartialCase = PLCSession.GetDefault("IS_PARTIAL_CASE") == "T";

                if (isPartialCase)
                {
                    string message = "Case information is incomplete. Clicking OK will delete the current case. Continue?";
                    dlgConfirm.DialogKey = "Cancel_case";
                    dlgConfirm.ShowConfirm("Cancel Case", message);
                    e.button_canceled = true;
                    return;
                }

                PLCSession.SetDefault("OFFENSE_VIEWNAME", "");
                PLCSession.SetDefault("OFFENSE_VIEWCOND", "");
            }

            if ((e.button_name == "CANCEL") && (e.button_action == "AFTER"))
            {

                OriginalRefType = null;
                OriginalDepartmentCode = null;

                if (PLCSessionVars1.CheckUserOption("CASELOCK"))
                    chkCaseLocked.Checked = IsCaseLocked;

                PLCDBGlobal.instance.LockUnlockRecord("TV_LABCASE", PLCSession.PLCGlobalCaseKey, "-1", "-1", PLCSession.PLCGlobalAnalyst, PLCSession.GetOSUserName(), PLCSession.GetOSComputerName(), false, PLCSession.CheckUserOption("DELLOCKS"));

                Response.Redirect("Tab1Case.aspx");

            }
            if ((e.button_name == "RecordUnlock"))
            {
                mInput.ShowMsg("Unlock record", "Please enter why you have to unlock the record ?", 0, UserComments.ClientID, MsgCommentPostBackButton.ClientID);
                ToggleResultsButton();
                return;
            }

            if (e.button_name == "Search Results...")
            {
                // Check the session data for search grid name and search criteria
                ConfigureResultsGrid();

                // Open Search Results Popup
                ScriptManager.RegisterStartupScript(PLCButtonPanel1, PLCButtonPanel1.GetType(), "OpenDialogCaseSearchResults", "OpenDialogCaseSearchResults();", true);
            }

            if (e.button_name == "SAVE")
            {
                CheckUsesBioIdentityPurposes(e.button_action);
                CheckForDefaultProcess(e.button_action);
                if (e.button_action == "AFTER")
                {



                    if (PLCDBPanel1.getpanelfield("OFFENSE_CODE") != PLCDBPanel1.GetOriginalValue("OFFENSE_CODE"))
                        ShowCaseTypeLabel();


                    String thisInvAgency = PLCDBPanel1.getpanelfield("INVESTIGATING_AGENCY");
                    if (thisInvAgency != OriginalInvestigatingAGency)
                    {
                        PLCDBGlobal.instance.updateReviewDates(PLCSessionVars1.PLCGlobalCaseKey);
                        OriginalInvestigatingAGency = PLCDBPanel1.getpanelfield("INVESTIGATING_AGENCY");
                    }

                    PostSavingMessage();

                }
            }
        }



        private void UpdateFirstSubmission()
        {
            PLCQuery qryLabSub = new PLCQuery();
            qryLabSub.SQL = "Select * FROM TV_LABSUB where CASE_KEY = " + PLCSessionVars1.PLCGlobalCaseKey + " and SUBMISSION_NUMBER = '1'";
            qryLabSub.Open();
            if (!qryLabSub.IsEmpty())
            {
                qryLabSub.Edit();

                for (int i = 0; i < SubmissionFieldList.Count; i++)
                {
                    FieldRec MyField = (FieldRec)SubmissionFieldList[i];
                    if (qryLabSub.FieldExist(MyField.FieldName.ToUpper()))
                    {

                        //update TV_LABSUB.CASE_DEPARTMENT_CODE with TV_LABCASE.DEPARTMENT_CODE Value
                        if (PLCSession.GetLabCtrlFlag("CASE_USES_SUBMISSION_CASE_DEPT") == "T"
                            && MyField.FieldName.ToUpper() == "DEPARTMENT_CODE")
                        {
                            qryLabSub.SetFieldValue("CASE_DEPARTMENT_CODE", MyField.FieldValue.Trim());
                        }
                        else
                            qryLabSub.SetFieldValue(MyField.FieldName.ToUpper(), MyField.FieldValue.Trim());                      
                    }
                }
                qryLabSub.Post("TV_LABSUB", 8, 10);
            }

            SubmissionFieldList.Clear();

        }

        private string PrepareCaseNarrative(out String URNNarrative)
        {
            StringBuilder auditText = new StringBuilder();
            URNNarrative = String.Empty;

            foreach (string fieldName in PLCDBPanel1.GetFieldNames())
            {
                string fieldType = PLCDBPanel1.GetPanelRecByFieldName(fieldName).fldtype;
                string originalValue = PLCDBPanel1.GetOriginalValue(fieldName);
                string newValue = PLCDBPanel1.getpanelfield(fieldName);
                string editmask = PLCDBPanel1.GetPanelRecByFieldName(fieldName).editmask;

                if (fieldType == "D")
                {
                    //if (!string.IsNullOrEmpty(originalValue))
                    //    originalValue = DateTime.Parse(originalValue).ToShortDateString();
                    //if (!string.IsNullOrEmpty(newValue))
                    //    newValue = DateTime.Parse(newValue).ToShortDateString();

                    if (!string.IsNullOrEmpty(originalValue))
                    {
                        if (editmask.ToUpper().StartsWith("DD"))
                            originalValue = DateTime.Parse(originalValue, CultureInfo.CreateSpecificCulture("en-GB")).ToString(CultureInfo.CreateSpecificCulture("en-GB"));
                        else
                            originalValue = DateTime.Parse(originalValue).ToShortDateString();
                    }
                    if (!string.IsNullOrEmpty(newValue))
                    {
                        if (editmask.ToUpper().StartsWith("DD"))
                            newValue = DateTime.Parse(newValue, CultureInfo.CreateSpecificCulture("en-GB")).ToString(CultureInfo.CreateSpecificCulture("en-GB"));
                        else
                            newValue = DateTime.Parse(newValue).ToShortDateString();
                    }
                }
                else if (fieldType == "T")
                {
                    if (!string.IsNullOrEmpty(originalValue))
                        originalValue = DateTime.Parse(originalValue).ToShortTimeString();
                    if (!string.IsNullOrEmpty(newValue))
                        newValue = DateTime.Parse(newValue).ToShortTimeString();
                }

                if ((!string.IsNullOrEmpty(originalValue) && originalValue != newValue) || (PLCSession.GetLabCtrl("LOG_BLANK_TO_DATA_CHANGES") == "T" && originalValue != newValue))
                {
                    if (PLCSessionVars1.GetLabCtrl("TRACK_URN_CHANGE") == "T" && fieldName == "DEPARTMENT_CASE_NUMBER")
                    {
                        StringBuilder URNText = new StringBuilder();
                        URNText.AppendLine(PLCDBPanel1.GetPanelRecByFieldName(fieldName).prompt + " CHANGED");
                        URNText.AppendLine("FROM: " + (originalValue == "" ? "-" : PLCDBPanel1.GetOriginalDesc(fieldName)));
                        URNText.AppendLine("TO: " + (newValue == "" ? "-" : PLCDBPanel1.GetFieldDesc(fieldName)));
                        URNNarrative = URNText.ToString();
                    }
                    else
                    {
                        auditText.AppendLine(PLCDBPanel1.GetPanelRecByFieldName(fieldName).prompt + " (" + fieldName + ") CHANGED");
                        if (fieldType == "D")
                            auditText.AppendLine("FROM: " + (originalValue == "" ? "-" : originalValue) +
                            " TO: " + (newValue == "" ? "-" : newValue));
                        else
                        {
                            auditText.AppendLine("FROM: " + (originalValue == "" ? "-" : PLCDBPanel1.GetOriginalDesc(fieldName)));
                            auditText.AppendLine("TO: " + (newValue == "" ? "-" : PLCDBPanel1.GetFieldDesc(fieldName)));
                        }
                    }
                }
            }
            return auditText.ToString();
        }

        private void SaveURNChangeNarrative(string URNNarrative)
        {
            string type = PLCSessionVars1.GetLabCtrl("URN_CHANGE_CODE");
            StringBuilder scheduleComments = new StringBuilder(URNNarrative);
            scheduleComments.AppendLine("REASON FOR CHANGE");
            scheduleComments.AppendLine(hdnConfirmUpdate.Value == "" ? "-" : hdnConfirmUpdate.Value);

            AddSchedule(type, scheduleComments.ToString());
            PLCDBGlobalClass.UpdateSheriffCaseFields(PLCDBPanel1.getpanelfield("DEPARTMENT_CODE"), PLCDBPanel1.getpanelfield("DEPARTMENT_CASE_NUMBER"));

            PLCSessionVars1.PLCGlobalDepartmentCaseNumber = PLCDBPanel1.getpanelfield("DEPARTMENT_CASE_NUMBER");

            if (thisCaseIsAnURN(PLCSessionVars1.PLCGlobalCaseKey))
                mbox.ShowMsg("ORI and/or URN Change", "Please remember to change CHARGE #1 if there was a STAT code change. Please remember to reprint any forms or labels as necessary.", 0);
        }

        //*AAC 12022010
        protected void CheckUsesBioIdentityPurposes(string action)
        {
            if (PLCSessionVars1.GetLabCtrl("USES_BIO_IDENTITY_PURPOSES") == "T")
            {
                PLCHelperFunctions plcHelp = new PLCHelperFunctions();
                if (action == "BEFORE")
                {
                    if (plcHelp.CheckOffenseLevel("F", PLCSessionVars1.PLCGlobalCaseKey))
                    {
                        ViewState.Add("BioFelonyBeforeSaving", "T");
                    }
                }
                else if (action == "AFTER")
                {
                    bool beforeSaving = (ViewState["BioFelonyBeforeSaving"] != null &&
                                         ViewState["BioFelonyBeforeSaving"].ToString().Trim() == "T");
                    bool afterSaving = plcHelp.CheckOffenseLevel("F", PLCSessionVars1.PLCGlobalCaseKey);

                    ViewState["BioFelonyBeforeSaving"] = null;

                    if (!beforeSaving && afterSaving)
                    {

                        if (ViewState["PostSaveMessage"] == null)
                        {
                            ViewState.Add("PostSaveMessage", "At least one of the case charges is now a felony. Please make sure the <B>\"Seized for DNA identity?\"</B> and <B>\"Status\" fields for each item are</B> updated as necessary. ");
                        }
                        else
                        {
                            string message = ViewState["PostSaveMessage"].ToString();
                            message += "<br /><br />" +
                                       "At least one of the case charges is now a felony. Please make sure the <B>\"Seized for DNA identity?\"</B> and <B>\"Status\" fields for each item are</B> updated as necessary. ";
                            ViewState["PostSaveMessage"] = message;
                        }
                    }
                }
            }
        }

        protected void CheckForDefaultProcess(string action)
        {
            if (PLCSessionVars1.GetLabCtrl("USES_BIO_IDENTITY_PURPOSES") == "T")
            {
                PLCHelperFunctions plcHelp = new PLCHelperFunctions();
                if (action == "BEFORE")
                {
                    if (plcHelp.CheckOffenseLevel("F", PLCSessionVars1.PLCGlobalCaseKey))
                    {
                        ViewState.Add("StatFelonyBeforeSaving", "T");
                    }
                }
                else if (action == "AFTER")
                {
                    bool beforeSaving = (ViewState["StatFelonyBeforeSaving"] != null &&
                                         ViewState["StatFelonyBeforeSaving"].ToString().Trim() == "T");
                    bool afterSaving = plcHelp.CheckOffenseLevel("F", PLCSessionVars1.PLCGlobalCaseKey);

                    ViewState["StatFelonyBeforeSaving"] = null;

                    if (!beforeSaving && afterSaving)
                    {
                        // plcHelp.SetItemDefaultStatus(PLCSessionVars1.PLCGlobalCaseKey, true);

                        if (ViewState["PostSaveMessage"] == null)
                        {
                            ViewState.Add("PostSaveMessage", "This case now has a felony charge. The Biological Properties field for each item will need to be updated on the ITEMS tab.");
                        }
                        else
                        { //Prioritize this message since both address to the Biological Properties update. Else, adjust;
                            //string message = ViewState["PostSaveMessage"].ToString();
                            //message += "<br /><br />" +

                            //Check if Digital media item exists
                            PLCQuery sqlBio = new PLCQuery();
                            sqlBio.SQL = "SELECT * FROM TV_LABITEM WHERE ITEM_CATEGORY = 'D' AND CASE_KEY = " + PLCSessionVars1.PLCGlobalCaseKey;
                            sqlBio.Open();
                            if (sqlBio.HasData())
                            {
                                string message = "This case now has a felony charge. The Biological Properties field for each item will need to be updated on the ITEMS tab.";
                                ViewState["PostSaveMessage"] = message;
                            }
                        }
                    }
                    else if(beforeSaving && !afterSaving)
                    {
                        // plcHelp.SetItemDefaultStatus(PLCSessionVars1.PLCGlobalCaseKey, false);
                    }
                }
            }
        }

        protected void PostSavingMessage()
        {
            if (ViewState["PostSaveMessage"] != null)
            {
                string message = ViewState["PostSaveMessage"].ToString();

                if (Request.QueryString["c"] == PLCSession.PLCGlobalDepartmentCaseNumber)
                    mbReload.ShowMessage("Case Info", message, MessageBoxType.Information);
                else
                    mbox.ShowMsg("Case Info", message, 0);

                ViewState["PostSaveMessage"] = null;
            }
            else
            {
                if (Request.QueryString["c"] == PLCSession.PLCGlobalDepartmentCaseNumber)
                    Response.Redirect("Tab1Case.aspx");
            }
        }

        protected void PrintCaseLabel()
        {
            string caseLabCode = PLCDBGlobal.instance.GetLabCaseField(PLCSession.PLCGlobalCaseKey, "LAB_CODE");
            string reportName = PLCDBGlobal.instance.GetLabelReportName(caseLabCode, "CASE", defaultReport: "BCLIST");

            if (PLCSessionVars1.GetLabCtrl("USES_WS_BARCODE_PRINTING") == "T"
                || PLCSession.CheckLabelFormatFlag(reportName, "CALL_URI"))
            {
                PLCSessionVars1.WriteDebug("Using Web Service to print Item BC", true);
                Response.Redirect("~/PrintBC_Case.aspx");
            }


            if (PLCSessionVars1.GetLabCtrl("USES_WS_BARCODE_PRINTING") == "W")
            {
                PLCSessionVars1.WriteDebug("Using Toolkit to print Item BC", true);
                Response.Redirect("~/PrintCaseWBL.aspx");
            }

            PLCSession.SetDefault("BARCODE_KEYLIST", "CA:" + PLCSession.PLCGlobalCaseKey);

            PLCSessionVars1.PLCCrystalReportName = reportName;
            //PLCSessionVars1.PLCCrystalSelectionFormula = "{TV_LABCASE.CASE_KEY} = " + PLCSessionVars1.PLCGlobalCaseKey;
            PLCSessionVars1.PLCCrystalReportTitle = "Case Barcode Label";
            PLCSessionVars1.PrintCRWReport();
        }

        protected void bnDiscoveryPacket_Click(object sender, EventArgs e)
        {
            if (PLCSession.GetLabCtrlFlag("USES_ENTERPRISE_DISCOVERY") == "T")
            {
                Session["FROMCASEINFOTAB"] = "T";
                Response.Redirect("~/DiscoveryPacket.aspx");
            }
            else
            {

                string returnPage = "Dashboard.aspx";
                if (Request.UrlReferrer != null)
                    returnPage = Request.UrlReferrer.AbsolutePath.Substring(Request.UrlReferrer.AbsolutePath.LastIndexOf("/") + 1);

                WebOCX.OnSuccessScript = "CheckBrowserWindowID(function () { window.location = '" + returnPage + "'; });";
                WebOCX.CreateDiscoveryPacket(PLCSession.PLCGlobalCaseKey);
            }
        }

        protected void btnCaseEmail_Click(object sender, EventArgs e)
        {
            WebOCX.OnSuccessScript = "CheckBrowserWindowID();";
            WebOCX.CallCaseMail(PLCSession.PLCGlobalCaseKey);
        }

        protected void btnError_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/DataBankError.aspx");
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            PLCDBPanel1.FocusField("CASE_OFFICER");
        }

        protected void Button5_Click(object sender, EventArgs e)
        {

        }

        protected void MsgCommentPostBackButton_Click(object sender, EventArgs e)
        {
            if (UserComments.Text != "")
            {
                PLCDBGlobal.instance.LockUnlockRecord("TV_LABCASE", PLCSession.PLCGlobalCaseKey, "-1", "-1", false);
                lPLCLockStatus.Text = "";
                dvPLCLockStatus.Visible = false;
                PLCButtonPanel1.SetButtonsForLock(false);
            }
        }

        protected void btnConfirmUpdate_Click(object sender, EventArgs e)
        {
            // save text in txtConfirmUpdate
            if (txtConfirmUpdate.Text.Trim().Length > 0)
            {
                hdnConfirmUpdate.Value = txtConfirmUpdate.Text;
                PLCButtonPanel1.ClickSaveButton();
            }
            else
            {
                PLCButtonPanel1.ClickCancelButton();
            }
        }

        protected void Button5_Click1(object sender, EventArgs e)
        {
            string Message = PLCSessionVars1.GetNextSequence("LABCASE_SEQ").ToString();
            mbox.ShowMsg("Test", Message, 0);
        }

        protected void bnCaseJacket_Click(object sender, EventArgs e)
        {
            string THEPATH = PLCSession.FindCrystalReport("case.rpt");
            PLCSessionVars1.PLCCrystalReportName = THEPATH;

            PLCSessionVars1.PLCCrystalSelectionFormula = "{TV_LABCASE.CASE_KEY} = " + PLCSessionVars1.PLCGlobalCaseKey;
            PLCSessionVars1.PLCCrystalReportTitle = "Case Jacket for " + PLCSessionVars1.PLCGlobalDepartmentCaseNumber;
            PLCSessionVars1.PrintCRWReport();
        }

        // Save update confirmation comment and the auditlog as Case narrative record
        private void SaveConfirmUpdate(string auditLogText)
        {
            if (auditLogText.Trim() == "")
                return;
            StringBuilder caseUpdateComments = new StringBuilder();
            caseUpdateComments.AppendLine("CASE UPDATED");
            caseUpdateComments.AppendLine("AUDIT LOG:");
            caseUpdateComments.Append(auditLogText);
            if (!string.IsNullOrEmpty(hdnConfirmUpdate.Value))
                caseUpdateComments.Append("REASON FOR CHANGE: " + hdnConfirmUpdate.Value);
            AddSchedule("CE", caseUpdateComments.ToString()); // Used CE (Case event) for saving case updates
        }

        protected void bnCaseLabel_Click(object sender, EventArgs e)
        {

            if (PLCSessionVars1.CheckUserOption("PRINTBAR"))
                PrintCaseLabel();
            else
                mbox.ShowMsg("Print Label", "You are not authorized to print barcode labels.", 1);

        }

        protected void bnDispo_Load(object sender, EventArgs e)
        {
            if (!PLCDBPanel1.IsBrowseMode) return;
            if (PLCDBPanel1.IsSavingMode) return;
            if (IsPostBack) return;

            if (PLCSession.CheckUserOption("RETREVIEW"))
            {
                PLCQuery qryRetentionReview = new PLCQuery(string.Format(
@"SELECT
    COUNT(DISTINCT C.CASE_KEY) AS CASE_COUNT
FROM 
    TV_LABITEM I 
    INNER JOIN TV_REVREQUEST R ON R.EVIDENCE_CONTROL_NUMBER = I.EVIDENCE_CONTROL_NUMBER
    INNER JOIN TV_LABCASE C ON C.CASE_KEY = I.CASE_KEY
WHERE
  C.CASE_KEY = {1} 
  AND R.REVIEW_STATUS in ('1','2') 
  AND ( ( C.CASE_MANAGER = '{0}' OR C.CASE_ANALYST = '{0}' or R.FINAL_REVIEW_BY = '{0}' )
        OR (SELECT COUNT(*) FROM TV_DEPTUSER DU LEFT JOIN TV_DEPTROLE DR ON DR.ROLE_RES = DU.ROLE_RES WHERE DU.ANALYST = '{0}' AND DR.CAN_DO_RETREVIEW = 'T') > 0 )", PLCSession.PLCGlobalAnalyst, PLCSession.PLCGlobalCaseKey));

                if (qryRetentionReview.Open() && qryRetentionReview.HasData())
                {
                    int retentionReviewCount;
                    if (int.TryParse(qryRetentionReview.FieldByName("CASE_COUNT"), out retentionReviewCount) && retentionReviewCount > 0)
                    {
                        bnDispo.Style.Add("color", "Red");
                    }
                    else
                    {
                        bnDispo.Style.Add("color", "Black");
                    }
                }
                else
                {
                    bnDispo.Style.Add("color", "Black");
                }
            }
            else
            {
                bnDispo.Visible = false;
            }
        }

        protected void bnDispo_Click(object sender, EventArgs e)
        {
            // Response.Redirect("~/DispoItems.aspx");
            Response.Redirect("~/RetentionReview.aspx");
        }

        protected void bnSchedule_Click(object sender, EventArgs e)
        {
            if (PLCSessionVars1.GetLabCtrl("USES_STORY") == "T")
            {
                Response.Redirect("~/Story.aspx");
            }
            else
            {
                Response.Redirect("~/Schedule.aspx");
            }

        }

        protected void bnReference_Click(object sender, EventArgs e)
        {
            PLCSessionVars1.PLCGloablCaseRefKey = "NONE";
            Response.Redirect("~/CaseRef.aspx");
        }

        // Ticket 22463
        protected void btnRelatedCases_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/RelatedCase.aspx");
        }

        //protected void bnUnlock_Click(object sender, EventArgs e)
        //{
        //    mInput.ShowMsg("Unlock record", "Please enter why you have to unlock the record ?", 0, UserComments.ClientID, MsgCommentPostBackButton.ClientID);
        //    return;
        //}

        protected void bnTeam_Click(object sender, EventArgs e)
        {
            Response.Redirect("CaseTeam.aspx");
        }

        // *AAC 08/20/20010 - Ticket 23741 
        protected void bnCaseReports_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/CaseReports.aspx");
        }



        private bool CanRouteItem
        {
            get
            {
                object obj = ViewState["CanRouteItem"];
                if (obj == null)
                {
                    PLCDBGlobal DBGlobal = new PLCDBGlobal();
                    ViewState["CanRouteItem"] = DBGlobal.UserCanRoute(PLCSessionVars1.PLCGlobalCaseKey, PLCSessionVars1.PLCGlobalAnalyst, PLCSessionVars1.PLCGlobalAnalystDepartmentCode);
                }

                return Convert.ToBoolean(ViewState["CanRouteItem"]);
            }
        }

        #region IOR/URN Tracking
        private bool CanChangeURN
        {
            get
            {
                return PLCSessionVars1.CheckUserOption("URNCHANGE") || PLCSessionVars1.CheckUserOption("DEPTCASE");
            }
        }

        private bool CanChangeIOR
        {
            get
            {
                return PLCSessionVars1.CheckUserOption("IORCHANGE");
            }
        }

        //protected void btnChangeIOR_Load(object sender, EventArgs e)
        //{
        //    if (!IsPostBack)
        //    {
        //        InitIORDialog();
        //    }
        //}

        //private void InitIORDialog()
        //{
        //    btnChangeIOR.Visible = (PLCSessionVars1.GetLabCtrl("TRACK_IOR_CHANGE") == "T");
        //    btnChangeIOR.Enabled = CanChangeIOR;

        //    if (PLCSessionVars1.GetLabCtrl("TRACK_IOR_CHANGE") == "T")
        //    {
        //        chCaseManager.SetValue(PLCDBPanel1.getpanelfield("CASE_MANAGER"));
        //        chCaseAnalyst.SetValue(PLCDBPanel1.getpanelfield("CASE_ANALYST"));
        //    }

        //    changeIORErrorSpan.InnerHtml = "";
        //    changeIORErrorSpan.Visible = false;
        //}

        //protected void btnChangeURN_Load(object sender, EventArgs e)
        //{
        //    if (!IsPostBack)
        //    {
        //        btnChangeURN.Visible = (PLCSessionVars1.GetLabCtrl("TRACK_URN_CHANGE") == "T");
        //        btnChangeURN.Enabled = CanChangeURN;

        //        if (PLCSessionVars1.GetLabCtrl("TRACK_URN_CHANGE") == "T")
        //        {
        //            txtURN.Text = PLCDBPanel1.getpanelfield("DEPARTMENT_CASE_NUMBER");
        //            SetURNMask();
        //        }
        //    }
        //}

        //        protected void btnOKChangeIOR_Click(object sender, EventArgs e)
        //        {
        //            //check if Track IOR Change is allowed
        //            if (PLCSessionVars1.GetLabCtrl("TRACK_IOR_CHANGE") == "T" && CanChangeIOR)
        //            {
        //                string newIOR = chCaseManager.GetValue();
        //                string newAlternateIOR = chCaseAnalyst.GetValue();
        //                string comments = txtCommentsIOR.Text;

        //                if (PLCDBPanel1.getpanelfield("CASE_MANAGER") != newIOR || PLCDBPanel1.getpanelfield("CASE_ANALYST") != newAlternateIOR)
        //                {
        //                    changeIORErrorSpan.InnerHtml = "";
        //                    changeIORErrorSpan.Visible = false;

        //                    //check if codes are valid
        //                    if (!IsIORValid(newIOR))
        //                    {
        //                        changeIORErrorSpan.InnerHtml = "The Investigator code you entered is not valid.";
        //                        changeIORErrorSpan.Visible = true;
        //                        mpeChangeIOR.Show();
        //                        return;
        //                    }

        //                    if (!IsAlternateIORValid(newAlternateIOR))
        //                    {
        //                        changeIORErrorSpan.InnerHtml = "The Alternate Inv code you entered is not valid.";
        //                        changeIORErrorSpan.Visible = true;
        //                        mpeChangeIOR.Show();
        //                        return;
        //                    }

        //                    if (comments.Trim() == "")
        //                    {
        //                        changeIORErrorSpan.InnerHtml = "Please enter a reason for this change.";
        //                        changeIORErrorSpan.Visible = true;
        //                        mpeChangeIOR.Show();
        //                        return;
        //                    }



        //                    UpdateIOR(newIOR, newAlternateIOR);

        //                    string type = PLCSessionVars1.GetLabCtrl("IOR_CHANGE_CODE");
        //                    string scheduleComments = "";

        //                    if (PLCDBPanel1.getpanelfield("CASE_MANAGER") != newIOR)
        //                        scheduleComments += @"IOR CHANGED
        //From: " + ((PLCDBPanel1.getpanelfield("CASE_MANAGER") == "") ? "-" : chCaseManager.GetCodeDesc("TV_ANALYST", PLCDBPanel1.getpanelfield("CASE_MANAGER"), "") + " (" + PLCDBPanel1.getpanelfield("CASE_MANAGER") + ")") + @"
        //To: " + ((chCaseManager.GetValue() == "") ? "-" : chCaseManager.GetCodeDesc("TV_ANALYST", chCaseManager.GetValue(), "") + " (" + chCaseManager.GetValue() + ")") + @"
        //
        //";
        //                    if (PLCDBPanel1.getpanelfield("CASE_ANALYST") != newAlternateIOR)
        //                        scheduleComments += @"Alternate IOR CHANGED
        //From: " + ((PLCDBPanel1.getpanelfield("CASE_ANALYST") == "") ? "-" : chCaseAnalyst.GetCodeDesc("TV_ANALYST", PLCDBPanel1.getpanelfield("CASE_ANALYST"), "") + " (" + PLCDBPanel1.getpanelfield("CASE_ANALYST") + ")") + @"
        //To: " + ((chCaseAnalyst.GetValue() == "") ? "-" : chCaseAnalyst.GetCodeDesc("TV_ANALYST", chCaseAnalyst.GetValue(), "") + " (" + chCaseAnalyst.GetValue() + ")") + @"
        //
        //";

        //                    scheduleComments += @"REASON FOR CHANGE
        //" + (comments == "" ? "-" : comments);

        //                    AddSchedule(type, scheduleComments);

        //                    Response.Redirect("TAB1Case.aspx");
        //                }
        //            }
        //        }

        //protected void btnCancelChangeIOR_Click(object sender, EventArgs e)
        //{
        //    InitIORDialog();
        //}

        private bool IsIORValid(string newIOR)
        {
            if (newIOR.Trim().Length > 0)
            {
                PLCQuery qryValidIOR = new PLCQuery();
                qryValidIOR.SQL = "SELECT ANALYST FROM TV_ANALYST WHERE ANALYST = '" + newIOR + "'";
                qryValidIOR.Open();
                return !qryValidIOR.IsEmpty();
            }
            else
            {
                // Blank IOR is valid.
                return true;
            }
        }

        private bool IsAlternateIORValid(string newAlternateIOR)
        {
            if (newAlternateIOR.Trim().Length > 0)
            {
                PLCQuery qryValidAIOR = new PLCQuery();
                qryValidAIOR.SQL = "SELECT ANALYST FROM TV_ANALYST WHERE ANALYST = '" + newAlternateIOR + "'";
                qryValidAIOR.Open();
                return !qryValidAIOR.IsEmpty();
            }
            else
            {
                // Blank Alt IOR is valid.
                return true;
            }
        }

        //        protected void btnOKChangeURN_Click(object sender, EventArgs e)
        //        {
        //            //check if Track URN Change is allowed
        //            if (PLCSessionVars1.GetLabCtrl("TRACK_URN_CHANGE") == "T" && CanChangeURN)
        //            {
        //                string newURN = txtURN.Text;
        //                string comments = txtCommentsURN.Text;

        //                if (PLCDBPanel1.getpanelfield("DEPARTMENT_CASE_NUMBER") != newURN)
        //                {
        //                    UpdateURN(newURN);

        //                    string type = PLCSessionVars1.GetLabCtrl("URN_CHANGE_CODE");
        //                    string scheduleComments = @"URN CHANGED
        //From: " + PLCDBPanel1.getpanelfield("DEPARTMENT_CASE_NUMBER") + @"
        //To: " + txtURN.Text + @"
        //
        //REASON FOR CHANGE
        //" + (comments == "" ? "-" : comments);

        //                    AddSchedule(type, scheduleComments);
        //                    PLCDBGlobalClass.UpdateSheriffCaseFields(PLCDBPanel1.getpanelfield("DEPARTMENT_CODE"), newURN);

        //                    PLCSessionVars1.PLCGlobalDepartmentCaseNumber = newURN;
        //                    Response.Redirect("TAB1Case.aspx");
        //                }
        //            }
        //        }

        private void UpdateIOR(string newIOR, string newAlternateIOR)
        {
            PLCQuery qryUpdateCase = new PLCQuery();
            qryUpdateCase.SQL = "SELECT CASE_MANAGER, CASE_ANALYST FROM TV_LABCASE WHERE CASE_KEY = " + PLCSessionVars1.PLCGlobalCaseKey;
            qryUpdateCase.Open();
            if (!qryUpdateCase.IsEmpty())
            {
                qryUpdateCase.Edit();
                qryUpdateCase.SetFieldValue("CASE_MANAGER", newIOR);
                qryUpdateCase.SetFieldValue("CASE_ANALYST", newAlternateIOR);
                qryUpdateCase.Post("TV_LABCASE", 7000, 9);
            }
        }

        private void UpdateURN(string newURN)
        {
            PLCQuery qryUpdateCase = new PLCQuery();
            qryUpdateCase.SQL = "SELECT DEPARTMENT_CASE_NUMBER FROM TV_LABCASE WHERE CASE_KEY = " + PLCSessionVars1.PLCGlobalCaseKey;
            qryUpdateCase.Open();
            qryUpdateCase.Edit();
            qryUpdateCase.SetFieldValue("DEPARTMENT_CASE_NUMBER", newURN);
            qryUpdateCase.Post("TV_LABCASE", 7000, 10);
        }

        private void AddSchedule(string type, string scheduleComments)
        {
            int scheduleKey = PLCSessionVars1.GetNextSequence("LABSUPP_SEQ");

            PLCQuery qrySchedule = new PLCQuery();
            qrySchedule.SQL = "SELECT SCHEDULE_KEY, CASE_KEY, DATE_RES, TIME, TYPE_RES, COMMENTS, ANALYST FROM TV_SCHEDULE WHERE 0 = 1";
            qrySchedule.Open();
            qrySchedule.Append();
            qrySchedule.SetFieldValue("SCHEDULE_KEY", scheduleKey);
            qrySchedule.SetFieldValue("CASE_KEY", PLCSessionVars1.PLCGlobalCaseKey);
            qrySchedule.SetFieldValue("DATE_RES", DateTime.Now);
            qrySchedule.SetFieldValue("TIME", DateTime.Now);
            qrySchedule.SetFieldValue("TYPE_RES", type);
            qrySchedule.SetFieldValue("COMMENTS", scheduleComments);
            qrySchedule.SetFieldValue("ANALYST", PLCSessionVars1.PLCGlobalAnalyst);
            qrySchedule.Post("TV_SCHEDULE", 41, 3);
        }

        //private void SetURNMask()
        //{
        //    string departmentCode = PLCDBPanel1.getpanelfield("DEPARTMENT_CODE");

        //    if (!string.IsNullOrEmpty(departmentCode))
        //    {
        //        string urnMask = URNMask(departmentCode);
        //        if (urnMask != "")
        //        {
        //            meeURN.Mask = urnMask;
        //            meeURN.Enabled = true;
        //        }
        //    }
        //}

        private string URNMask(string departmentCode)
        {
            string result = "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!"; //default

            PLCQuery qryMask = new PLCQuery();
            qryMask.SQL = "SELECT PICTURE_MASK FROM TV_DEPTNAME WHERE DEPARTMENT_CODE = '" + departmentCode + "'";
            qryMask.Open();
            if (!qryMask.IsEmpty())
                result = qryMask.FieldByName("PICTURE_MASK");

            return result;
        }
        #endregion

        private void SyncLabSubOffenseInfo()
        {
            PLCQuery qryLabSub = new PLCQuery();
            qryLabSub.SQL = "Select * FROM TV_LABSUB where CASE_KEY = " + PLCSessionVars1.PLCGlobalCaseKey;
            qryLabSub.Open();
            while (!qryLabSub.EOF())
            {
                qryLabSub.Edit();
                qryLabSub.SetFieldValue("OFFENSE_CODE", PLCDBPanel1.getpanelfield("OFFENSE_CODE"));
                qryLabSub.SetFieldValue("OFFENSE_CODE_2", PLCDBPanel1.getpanelfield("OFFENSE_CODE_2"));
                qryLabSub.SetFieldValue("OFFENSE_CODE_3", PLCDBPanel1.getpanelfield("OFFENSE_CODE_3"));
                qryLabSub.SetFieldValue("OFFENSE_DATE", PLCDBPanel1.getpanelfield("OFFENSE_DATE"));
                qryLabSub.Post("TV_LABSUB", 8, 1);
                qryLabSub.Next();
            }
        }

        private void SyncLabAssignDueDate()
        {
            string offenseCategories = PLCDBPanel1.getpanelfield("OFFENSE_CATEGORY") + "," + PLCDBPanel1.getpanelfield("OFFENSE_CATEGORY_2") + "," + PLCDBPanel1.getpanelfield("OFFENSE_CATEGORY_3");
            offenseCategories = offenseCategories.Trim(new char[',']);

            PLCQuery qryLabAssign = new PLCQuery();
            qryLabAssign.SQL = "SELECT * FROM TV_LABEXAM where CASE_KEY = " + PLCSessionVars1.PLCGlobalCaseKey;
            if (qryLabAssign.Open() && qryLabAssign.HasData())
            {
                while (!qryLabAssign.EOF())
                {
                    DateTime dtNewDueDate;
                    string examKey = qryLabAssign.FieldByName("EXAM_KEY").ToString();

                    if (string.IsNullOrEmpty(qryLabAssign.FieldByName("DUE_DATE")))
                    {
                        dtNewDueDate = Convert.ToDateTime("01-01-1900");
                    }
                    else
                    {
                        dtNewDueDate = Convert.ToDateTime(qryLabAssign.FieldByName("DUE_DATE"));
                    }

                    if (PLCDBGlobalClass.GetUpdatedDueDate(offenseCategories, ref dtNewDueDate))
                    {
                        qryLabAssign.Edit();
                        qryLabAssign.SetFieldValue("DUE_DATE", dtNewDueDate);
                        qryLabAssign.Post("TV_LABEXAM", 17, 1);

                        string logInfo = "DUE_DATE: [" + dtNewDueDate + "]";
                        PLCSession.WriteAuditLog(PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, examKey, "17", "1", "0", PLCSession.PLCGlobalAnalyst, ("iLIMS" + PLCSession.PLCBEASTiLIMSVersion).Substring(0, 8), logInfo, 0);

                    }

                    qryLabAssign.Next();
                }
            }
        }

        private string GetCaseReferenceStr(string CaseKey)
        {
            string str = "";
            List<string> referenceCaseKeys = new List<string>();

            PLCQuery qryLabRef = new PLCQuery();
            qryLabRef.SQL = "Select CASE_KEY, REFERENCE, REFERENCE_TYPE, REFERENCED_CASE_KEY from TV_LABREF where CASE_KEY = " + CaseKey + " And ((SYSTEM_GENERATED IS NULL) OR (SYSTEM_GENERATED <> 'T')) ORDER BY ENTRY_TIME_STAMP";
            qryLabRef.Open();

            if (!qryLabRef.IsEmpty())
                ReferenceExists = true;

            string caseReferences = GetCaseReferenceSP();
            if (!string.IsNullOrEmpty(caseReferences))
                return caseReferences;

            while (!qryLabRef.EOF())
            {
                if (qryLabRef.FieldByName("REFERENCE").Trim() != "")
                {
                    string referenceCaseKey = qryLabRef.FieldByName("REFERENCED_CASE_KEY");

                    if (PLCSession.GetLabCtrlFlag("PREFER_DEPARTMENT_CASE") == "T")
                    {
                        if (!referenceCaseKeys.Contains(referenceCaseKey))
                        {
                            string deptCaseNumber = PLCDBGlobal.instance.GetLabCaseField(referenceCaseKey, "DEPARTMENT_CASE_NUMBER");

                            if (!string.IsNullOrEmpty(deptCaseNumber))
                            {
                                if (str == "")
                                    str = "D: " + deptCaseNumber;
                                else
                                    str += ", " + "D: " + deptCaseNumber;

                            }

                            referenceCaseKeys.Add(referenceCaseKey);
                        }
                    }
                    else
                    {
                        if (str == "")
                        {
                            if (qryLabRef.FieldByName("REFERENCE_TYPE").Trim() != "")
                                str = qryLabRef.FieldByName("REFERENCE_TYPE") + ": " + qryLabRef.FieldByName("REFERENCE");
                            else
                                str = qryLabRef.FieldByName("REFERENCE");
                        }
                        else
                        {
                            if (qryLabRef.FieldByName("REFERENCE_TYPE").Trim() != "")
                                str += ", " + qryLabRef.FieldByName("REFERENCE_TYPE") + ": " + qryLabRef.FieldByName("REFERENCE");
                            else
                                str = ", " + qryLabRef.FieldByName("REFERENCE");
                        }
                    }

                }
                qryLabRef.Next();
            }
            return str;
        }

        private string GetCaseReferenceSP()
        {
            string spName = PLCSession.GetLabCtrl("SP_CASE_REFERENCE_LABEL");

            if (string.IsNullOrEmpty(spName))
                return string.Empty;

            if (PLCDBGlobal.instance.IsStoredProcedureExist(spName))
            {
                PLCQuery qrySP = new PLCQuery();
                qrySP.AddProcedureParameter("P_CASEKEY", PLCSession.PLCGlobalCaseKey, 10, OleDbType.Integer, ParameterDirection.Input);
                qrySP.AddProcedureParameter("P_MESSAGE", string.Empty, 1000, OleDbType.VarChar, ParameterDirection.Output);

                try
                {
                    Dictionary<string, object> results = qrySP.ExecuteProcedure(spName);
                    return results["P_MESSAGE"].ToString();
                }
                catch (Exception ex)
                {
                    PLCSession.WriteDebug("GetCaseReferenceSP Exception error: " + ex.Message);
                }
            }

            return string.Empty;
        }

        protected void btnForms_Click(object sender, EventArgs e)
        {
            Response.Redirect("CaseForm.aspx");
        }

        #region CaseLock

        private bool IsCaseLocked
        {
            get
            {
                if (ViewState["lc"] == null)
                    return false;
                return Convert.ToBoolean(ViewState["lc"]);
            }
            set
            {
                ViewState["lc"] = value;
                chkCaseLocked.Checked = value;
                bnCaseLock.Text = value ? PLCSession.GetSysPrompt("TAB1Case.bnCaseLock.UNLOCK", "Unlock Case") : PLCSession.GetSysPrompt("TAB1Case.bnCaseLock.LOCK", "Lock Case");
            }
        }

        protected void bnCaseLock_Click(object sender, EventArgs e)
        {
            UpdateCaseLock(!IsCaseLocked); //toggle (lock if previously unlocked, and vice versa)

            bnTeam.Enabled = IsCaseLocked;
            lCaseLockStatus.Visible = IsCaseLocked;
        }

        //protected void btnRouteItems_Click(object sender, EventArgs e)
        //{

        //}

        private void UpdateCaseLock(bool lockCase, int changeKey = 0)
        {
            IsCaseLocked = lockCase;

            PLCQuery qryCaseLock = new PLCQuery();
            qryCaseLock.SQL = "SELECT CASE_LOCKED FROM TV_LABCASE WHERE CASE_KEY = " + PLCSessionVars1.PLCGlobalCaseKey;
            qryCaseLock.Open();
            qryCaseLock.Edit();
            qryCaseLock.SetFieldValue("CASE_LOCKED", IsCaseLocked ? "T" : "F");
            qryCaseLock.Post("TV_LABCASE", 7000, IsCaseLocked ? 5 : 6, changeKey);
        }

        #endregion

        #region Supplements
        private const string QUERY_BASIC_SUPPLEMENT = "SELECT L.SUPPLEMENT_TABLE {0} FROM TV_LABSUPP L " +
            "WHERE ((L.NAME_SUPPLEMENT <> 'T') OR (L.NAME_SUPPLEMENT IS NULL)) {1} ORDER BY L.SUPPLEMENT_DESCRIPTION";
        private const string CASE_DYNAMIC_SUPPLEMENT = "WHEN L.SUPPLEMENT_TABLE = '{0}' THEN (SELECT COUNT(*) FROM {1} T WHERE T.CASE_KEY = {2}) ";
        private const string WHERE_CONVERSION_ONLY = "AND CONVERSION_ONLY = 'T'";
        private const string WHERE_REGULAR_SUPPLEMENT = "AND CONVERSION_ONLY IS NULL OR CONVERSION_ONLY <> 'T'";

        protected void gvSupplements_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["SuppMainDataKey"] = gvSupplements.SelectedDataKey["SUPPLEMENT_TABLE"].ToString() + "|" + gvSupplements.SelectedDataKey["SUPPLEMENT_DESCRIPTION"].ToString();
            mpeSupplements.Show();

            ScriptManager.RegisterStartupScript(this, this.GetType(), "restoreGridScrollById", "restoreGridScrollById('" + gvSupplements.ClientID + "')", true);
        }

        protected void btnSuppOpen_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Supplements.aspx");
        }

        protected void bnSupplements_Click(object sender, EventArgs e)
        {
            Session["SuppMainDataKey"] = null;
            BindSupplementsGrid();
            mpeSupplements.Show();
        }

        protected void rbtnListSuppType_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindSupplementsGrid();
            mpeSupplements.Show();

            ScriptManager.RegisterStartupScript(this, this.GetType(), "resetGridScrollById", "resetGridScrollById('" + gvSupplements.ClientID + "')", true);
        }

        protected void bnSupplements_Load(object sender, EventArgs e)
        {

            if (!PLCDBPanel1.IsBrowseMode) return;
            if (PLCDBPanel1.IsSavingMode) return;
            if (IsPostBack) return;


            int itemCount = 0;
            PLCQuery qrySupplementCount = new PLCQuery(string.Format(QUERY_BASIC_SUPPLEMENT, GetSupplementCase(), WHERE_REGULAR_SUPPLEMENT));

            if (qrySupplementCount.Open() && !qrySupplementCount.IsEmpty())
            {
                while (!qrySupplementCount.EOF())
                {
                    itemCount += qrySupplementCount.iFieldByName("SUPP_CNT", 0);

                    qrySupplementCount.Next();
                }
            }

            if (itemCount > 0)
            {
                bnSupplements.Style.Add("color", "Red");
            }
            else
            {
                bnSupplements.Style.Add("color", "Black");
            }
            if (!String.IsNullOrEmpty(PLCSessionVars1.GetLabCtrl("SUPPLEMENT_TEXT").ToString()))
                bnSupplements.Text = PLCSession.GetSysPrompt("LABCTRL.SUPPLEMENT_TEXT", PLCSessionVars1.GetLabCtrl("SUPPLEMENT_TEXT").ToString());
        }

        private void BindSupplementsGrid()
        {
            gvSupplements.DataSource = GetSupplements(rbtnListSuppType.SelectedIndex);
            gvSupplements.DataBind();
            if (gvSupplements.Rows.Count > 0)
            {
                gvSupplements.SelectedIndex = 0;
                btnSuppOpen.Enabled = true;
                Session["SuppMainDataKey"] = gvSupplements.SelectedDataKey["SUPPLEMENT_TABLE"].ToString() + "|" + gvSupplements.SelectedDataKey["SUPPLEMENT_DESCRIPTION"].ToString();
            }
            else
            {
                btnSuppOpen.Enabled = false;
            }
        }

        private DataTable GetSupplements(int typeIndex)
        {
            DataTable dt = new DataTable();

            string caseQuery = GetSupplementCase();

            PLCQuery qrySupplement = new PLCQuery();

            switch (typeIndex)
            {
                case 0: // Regular
                    qrySupplement.SQL = string.Format(QUERY_BASIC_SUPPLEMENT, caseQuery, WHERE_REGULAR_SUPPLEMENT);
                    break;
                case 1: // Conversion Only
                    qrySupplement.SQL = string.Format(QUERY_BASIC_SUPPLEMENT, caseQuery, WHERE_CONVERSION_ONLY);
                    break;
                default:
                    qrySupplement.SQL = string.Format(QUERY_BASIC_SUPPLEMENT, caseQuery, "");
                    break;
            }

            if (qrySupplement.OpenReadOnly() && !qrySupplement.IsEmpty())
            {
                dt = qrySupplement.PLCDataTable;
            }

            return dt;
        }

        private string GetSupplementCase()
        {
            StringBuilder caseQuery = new StringBuilder();
            PLCQuery qrySuppCase = new PLCQuery(string.Format(QUERY_BASIC_SUPPLEMENT, "", ""));

            if (qrySuppCase.Open() && !qrySuppCase.IsEmpty())
            {
                caseQuery.AppendLine(", L.SUPPLEMENT_DESCRIPTION, CASE ");
                while (!qrySuppCase.EOF())
                {
                    caseQuery.AppendFormat(CASE_DYNAMIC_SUPPLEMENT, qrySuppCase.FieldByName("SUPPLEMENT_TABLE"), qrySuppCase.FieldByName("SUPPLEMENT_TABLE"), PLCSessionVars1.PLCGlobalCaseKey);
                    qrySuppCase.Next();
                }
                caseQuery.AppendLine("END AS SUPP_CNT");
            }

            return caseQuery.ToString();
        }
        #endregion

        #region Search Results
        private void ToggleResultsButton()
        {
            if (PLCSessionVars1.GetLabCtrl("REMEMBER_SEARCH_RESULTS") == "T")
            {
                if (PLCSession.PLCCaseSearchType == CaseSearchType.None)
                {
                    PLCButtonPanel1.SetCustomButtonVisible("Search Results...", false);
                }
                else
                {
                    PLCButtonPanel1.SetCustomButtonVisible("Search Results...", true);
                }
            }
            else
                PLCButtonPanel1.SetCustomButtonVisible("Search Results...", false);
        }

        private void ConfigureResultsGrid()
        {
            if (PLCSession.PLCCaseSearchType == CaseSearchType.None)
            {
                gvSearchCase.PLCGridName = "";
            }
            else
            {
                string searchCaseGridName;
                string[] searchCaseKeyNames;

                switch (PLCSession.PLCCaseSearchType)
                {
                    case CaseSearchType.ByCase:
                        searchCaseGridName = "SEARCH_BY_CASE";
                        searchCaseKeyNames = new string[] { "CASE_KEY" };
                        break;
                    case CaseSearchType.ByName:
                        searchCaseGridName = "SEARCH_BY_NAME";
                        searchCaseKeyNames = new string[] { "CASE_KEY", "NAME_KEY" };
                        break;
                    case CaseSearchType.ByReference:
                        searchCaseGridName = "SEARCH_BY_REF";
                        searchCaseKeyNames = new string[] { "CASE_KEY" };
                        break;
                    case CaseSearchType.ByAttribute:
                        searchCaseGridName = "SEARCH_BY_ATTR";
                        searchCaseKeyNames = new string[] { "CASE_KEY", "EVIDENCE_CONTROL_NUMBER" };
                        break;
                    case CaseSearchType.CODISSearch:
                        searchCaseGridName = "CODIS_LIMS_SEARCH_CASE";
                        searchCaseKeyNames = new string[] { "CASE_KEY" };
                        break;
                    case CaseSearchType.ByTestType:
                        searchCaseGridName = "SEARCH_BY_TEST_TYPE";
                        searchCaseKeyNames = new string[] { "CASE_KEY", "EXAM_KEY" };
                        break;
                    case CaseSearchType.MEIMSSearch:
                        searchCaseGridName = "MEIMS_LIMS_SEARCH_CASE";
                        searchCaseKeyNames = new string[] { "CASE_KEY" };
                        break;
                    case CaseSearchType.HitSearch:
                        searchCaseGridName = "HIT_SEARCH_TAB_CAPTION";
                        searchCaseKeyNames = new string[] { "CASE_KEY" };
                        break;
                    case CaseSearchType.CustomSearch:
                        searchCaseGridName = PLCSession.PLCGlobalLabCode + "-CUSTOMSEARCH";
                        searchCaseKeyNames = new string[] { "CASE_KEY" };
                        break;
                    default:
                        searchCaseGridName = "";
                        searchCaseKeyNames = null;
                        break;
                }

                gvSearchCase.PLCSQLString = PLCSession.PLCCaseSearchSelect;
                gvSearchCase.PLCSQLString_AdditionalFrom = PLCSession.PLCCaseSearchFrom;
                gvSearchCase.PLCSQLString_AdditionalCriteria = PLCSession.PLCCaseSearchWhere;
                gvSearchCase.PLCGridName = searchCaseGridName;
                gvSearchCase.DataKeyNames = searchCaseKeyNames;
            }

            gvSearchCase.InitializePLCDBGrid();

            if (gvSearchCase.Rows.Count == 0)
                PLCButtonPanel1.SetCustomButtonVisible("Search Results...", false);
        }

        protected void gvSearchCase_SelectedIndexChanged(object sender, EventArgs e)
        {
            string caseKey = gvSearchCase.SelectedDataKey["CASE_KEY"].ToString();

            // check if user has access to case
            string msg = string.Empty;
            if (!PLCDBGlobalClass.IsUserHasAccessToCase(caseKey, PLCSessionVars1.PLCGlobalAnalyst, out msg))
            {
                dlgMessageSearchResults.ShowAlert("Case Search", msg);
                return;
            }

            if (PLCDBGlobalClass.CheckCaseLock(caseKey, PLCSession.PLCGlobalAnalyst, PLCSession.PLCGlobalAnalystDepartmentCode))
            {
                string message = PLCDBGlobalClass.GetCaseLockMessage(caseKey);
                if (string.IsNullOrEmpty(message))
                    message = "Case is locked.";

                dlgMessageSearchResults.ShowAlert("Case Search", message);
                return;
            }
            else
            {
                if (PLCSession.PLCCaseSearchType == CaseSearchType.ByAttribute)
                {
                    LoadItem(gvSearchCase.SelectedDataKey["EVIDENCE_CONTROL_NUMBER"].ToString());
                }
                else
                {
                    LoadCase(caseKey);
                }
            }
        }

        private void LoadCase(string caseKey)
        {
            string url = string.Empty;
            PLCDBGlobal.instance.LoadCase(caseKey, ref url);
            Response.Redirect(url);
        }

        private void LoadItem(string ECN)
        {
            PLCQuery qryItem = new PLCQuery();
            qryItem.SQL = "SELECT C.DEPARTMENT_CASE_NUMBER, I.CASE_KEY, I.EVIDENCE_CONTROL_NUMBER FROM TV_LABITEM I, TV_LABCASE C " +
                             "Where I.CASE_KEY = C.CASE_KEY AND I.EVIDENCE_CONTROL_NUMBER = " + ECN;
            qryItem.Open();
            if (!qryItem.EOF())
            {
                string url = string.Empty;
                PLCDBGlobal.instance.LoadCase(qryItem.FieldByName("CASE_KEY"), ref url, MainMenuTab.ItemsTab, ECN);
                Response.Redirect(url);
            }
        }

        #endregion

        private void SetPanelScript()
        {
            // Run InitLabCaseNumberChangeButton() on page load to initialize lab case number change button.
            if (PLCSession.CheckUserOption("CHANGECASE"))
            {
                // PLCDBPanel client-side IDs dictionary.
                this.hdnDBPanelClientIDsScript.Value = this.PLCDBPanel1.GetAddToListScript();

                ScriptManager.RegisterStartupScript(this, this.GetType(), "initscript", "InitLabCaseNumberChangeButton();", true);
              
                //PLCDBPanel1.GetPanelRecByFieldName("LAB_CASE").AllowEdit = false;
                setAllowEdit("TV_LABCASE", "LAB_CASE", false);
            }
        }

        private void SetLabCaseChangeEnableScript(bool bEnable)
        {
            ScriptManager.RegisterStartupScript(this, this.GetType(), "EnableLabCaseChange", string.Format("EnableLabCaseNumberChangeButton({0});", bEnable ? "true" : "false"), true);
        }

        protected void btnChangeLabCaseOK_Click(object sender, EventArgs e)
        {
            if (!dbpChangeLabCase.CanSave())
            {
                ReassignValuesOnPostBack();
                return;
            }

            string sLabCode = "";
            int iLabCaseYear = -1;
            int iLabCaseNumber = -1;
            string labcase = "";

            string[] fieldNames = dbpChangeLabCase.GetPanelFieldNames();

            if (fieldNames.Contains("LAB_CASE"))
            {
                labcase = dbpChangeLabCase.getpanelfield("LAB_CASE");
                if (!string.IsNullOrEmpty(labcase))
                {
                    var qryLabCase = new PLCQuery("SELECT * FROM TV_LABCASE WHERE LAB_CASE = '" + labcase + "' AND CASE_KEY != " + PLCSession.PLCGlobalCaseKey);
                    qryLabCase.OpenReadOnly();
                    if (qryLabCase.HasData())
                    {
                        var labCasePrompt = dbpChangeLabCase.GetFieldPrompt("LAB_CASE");
                        ServerAlert("Invalid " + labCasePrompt, "The " + labCasePrompt + " already exist.");
                        ReassignValuesOnPostBack();
                        return;
                    }
                }
            }
           
            PLCQuery qry = new PLCQuery(String.Format("select * from TV_LABCASE where CASE_KEY = {0}", PLCSession.PLCGlobalCaseKey));
            qry.Open();
            if (!qry.IsEmpty())
            {
                sLabCode = qry.FieldByName("LAB_CODE");
                iLabCaseYear = qry.iFieldByName("LAB_CASE_YEAR");


                if (!string.IsNullOrEmpty(dbpChangeLabCase.getpanelfield("LAB_CODE")))
                    sLabCode = dbpChangeLabCase.getpanelfield("LAB_CODE");
                
                if(PLCSession.SafeInt(dbpChangeLabCase.getpanelfield("LAB_CASE_YEAR")) > 0)
                    iLabCaseYear = PLCSession.SafeInt(dbpChangeLabCase.getpanelfield("LAB_CASE_YEAR"));

                string dbpanelOffenseDate = PLCDBPanel1.getpanelfield("OFFENSE_DATE");
                string offense = string.IsNullOrEmpty(dbpanelOffenseDate)
                    ? qry.FieldByName("OFFENSE_DATE")
                    : PLCSession.ConvertToDateTime(dbpanelOffenseDate).ToString();
                if (!string.IsNullOrEmpty(offense))
                {
                    offense = Convert.ToDateTime(offense).ToString("MM/dd/yyyy");
                }

                qry.Edit();

                string SPLabcase = "";
                string labCaseInput = "";
                for (int fieldIndex = 0; fieldIndex < fieldNames.Length; fieldIndex++)
                {
                    string fieldName = fieldNames[fieldIndex].ToUpper();
                    string fieldLabel = dbpChangeLabCase.GetFieldPrompt(fieldName);
                    string tableName = dbpChangeLabCase.GetFieldTableName(fieldName);
                    string value = dbpChangeLabCase.GetFieldValue(tableName, fieldName, fieldLabel).Replace("'", "''").Trim();
                  
                    if (!string.IsNullOrEmpty(value))
                    {
                        if (PLCSession.GetLabCtrlFlag("NO_LABCASE_ON_CASE_CREATE") == "T")
                        {
                            string storedProcedure = "SP_LABCASE_VALIDATE";
                            string labCaseYear = dbpChangeLabCase.getpanelfield("LAB_CASE_YEAR");
                            string labCaseMonth = dbpChangeLabCase.getpanelfield("LAB_CASE_MONTH");
                            string labCaseNumber  = dbpChangeLabCase.getpanelfield("LAB_CASE_NUMBER");

                            string caseYear = string.IsNullOrEmpty(labCaseYear) ? qry.FieldByName("LAB_CASE_YEAR") : labCaseYear;
                            string caseMonth = string.IsNullOrEmpty(labCaseMonth) ? qry.FieldByName("LAB_CASE_MONTH") : labCaseMonth;
                            string caseNumber = string.IsNullOrEmpty(labCaseNumber) ? qry.FieldByName("LAB_CASE_NUMBER") : labCaseNumber;

                            try
                            {
                                PLCQuery qryProc = new PLCQuery();
                                qryProc.AddProcedureParameter("P_CASEKEY", PLCSession.SafeInt(PLCSessionVars1.PLCGlobalCaseKey), 10, OleDbType.Integer, ParameterDirection.Input);
                                qryProc.AddProcedureParameter("P_FIELDNAME", fieldName, 30, OleDbType.VarChar, ParameterDirection.Input);
                                qryProc.AddProcedureParameter("P_VALUE", value, 300, OleDbType.VarChar, ParameterDirection.Input);
                                qryProc.AddProcedureParameter("P_OFFENSEDATE", offense, 30, OleDbType.VarChar, ParameterDirection.Input);
                                qryProc.AddProcedureParameter("P_CASEYEAR", PLCSession.SafeInt(caseYear), 10, OleDbType.Integer, ParameterDirection.Input);
                                qryProc.AddProcedureParameter("P_CASEMONTH", PLCSession.SafeInt(caseMonth), 10, OleDbType.Integer, ParameterDirection.Input);
                                qryProc.AddProcedureParameter("P_CASENUMBER", PLCSession.SafeInt(caseNumber), 10, OleDbType.Integer, ParameterDirection.Input);
                                qryProc.AddProcedureParameter("P_CASENUMBERPANEL", PLCSession.SafeInt(labCaseNumber), 10, OleDbType.Integer, ParameterDirection.Input);                              
                                qryProc.AddProcedureParameter("P_MESSAGE", 0, 4000, OleDbType.VarChar, ParameterDirection.Output);
                                qryProc.AddProcedureParameter("P_LABCASE", 0, 30, OleDbType.VarChar, ParameterDirection.Output);
                                Dictionary<string, object> spOutput = qryProc.ExecuteProcedure(storedProcedure);

                                string spMessage = Convert.ToString(spOutput["P_MESSAGE"]);
                                SPLabcase = Convert.ToString(spOutput["P_LABCASE"]);

                                if (!string.IsNullOrEmpty(spMessage))
                                {
                                    ReassignValuesOnPostBack();
                                    ServerAlert(spMessage);
                                    return;
                                }                              
                            }
                            catch(Exception ex)
                            {
                                PLCSession.WriteDebug("@SP_LABCASE_VALIDATE " + ex.Message, true);
                            }
                        }
                        else
                        {
                            if (fieldName.Equals("LAB_CASE_YEAR"))
                            {
                                if (PLCSession.SafeInt(value) == -1)
                                    continue;

                                if ((PLCSession.SafeInt(value) < 1900) || (PLCSession.SafeInt(value) > 3000))
                                {
                                    ReassignValuesOnPostBack();
                                    ServerAlert("Please enter a valid 4-digit year.");
                                    return;
                                }
                            }

                            if (fieldName.Equals("LAB_CASE_NUMBER"))
                            {
                                if (PLCSession.SafeInt(value) == -1)
                                    continue;

                                iLabCaseNumber = PLCSession.SafeInt(value);

                                int nextLabCaseNumber = PLCCommon.instance.GetNextLabCaseNumberNoIncrement(sLabCode, iLabCaseYear);

                                // No next lab case number defined.
                                if (nextLabCaseNumber < 0)
                                {
                                    ReassignValuesOnPostBack();
                                    string message = String.Format("The system does not have a next Case Number for the year {0}.<br><br>Call your adminstrator to set up the case number for the year {0} through the Config program.", iLabCaseYear);
                                    ServerAlert(message);
                                    return;
                                }

                                // Don't allow a Lab Case Number that equals or comes after the next available lab case number.
                                if (iLabCaseNumber >= nextLabCaseNumber)
                                {
                                    ReassignValuesOnPostBack();
                                    string message =  String.Format("The Lab Case Number cannot be greater than or equal to the next case number {0}.", PLCCommon.instance.FormatCaseNumber(sLabCode, iLabCaseYear, nextLabCaseNumber));
                                    ServerAlert(message);
                                    return;
                                }
                            }

                            if (fieldName.Equals("LAB_CASE"))
                                labCaseInput = value;
                        }

                        if (fieldName.Equals("LAB_CASE_MONTH"))
                            value = value.TrimStart('0'); //trim leading zeros

                        if (qry.FieldExist(fieldName))
                        {
                            qry.SetFieldValue(fieldName, value);
                        }

                    }
                }

                //assign a value to the labcase field
                if (PLCSession.GetLabCtrlFlag("NO_LABCASE_ON_CASE_CREATE") == "T" && !string.IsNullOrEmpty(SPLabcase))
                {
                    if (qry.FieldExist("LAB_CASE"))
                        qry.SetFieldValue("LAB_CASE", SPLabcase);                 
                }

                if (PLCSession.GetLabCtrlFlag("NO_LABCASE_ON_CASE_CREATE") != "T"
                   && PLCSession.GetLabCtrlFlag("UPDATE_LABCASE_YEAR_AND_NUMBER") == "T"
                   && !string.IsNullOrEmpty(labCaseInput)
                   && labCaseInput.Contains("-"))
                {
                    int caseYear = DateTime.Today.Year; //default year
                    string[] labcasevalues = labCaseInput.Split('-');
                    int year = PLCSession.SafeInt(labcasevalues[0].ToString());
                    int casenumber = PLCSession.SafeInt(labcasevalues[1].ToString());

                    if (year > 0 && year.ToString().Length == 4)
                        caseYear = year;

                    qry.SetFieldValue("LAB_CASE_YEAR", caseYear);

                    if (casenumber > 0)
                        qry.SetFieldValue("LAB_CASE_NUMBER", casenumber);


                }

                qry.Post("TV_LABCASE", -1, -1);

            }

   
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "_", "javascript: document.getElementById('" + btnDummyLabCase.ClientID + "').click();", true);
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "_closeLabCaseDialog", "closeLabCaseDialog('mdialog-change-labcase');", true);
           
        }


        protected void btnDummyLabCase_Click(object sender, EventArgs e)
        {
            string labCase = PLCDBGlobal.instance.GetLabCaseField(PLCSession.PLCGlobalCaseKey, "LAB_CASE");
            if (!string.IsNullOrEmpty(labCase))
            {
                PLCDBPanel1.setpanelfield("LAB_CASE", labCase);
                labcasenolabel.InnerHtml = labCase;
                PLCSession.PLCGlobalLabCase = labCase;
                
            }

            string caseTitleCacheKey = "CASETITLE@@@" + PLCSessionVars1.PLCGlobalCaseKey;
            CacheHelper.DeleteItem(caseTitleCacheKey);
            ((MasterPage)Master).SetCaseTitle("");
            ((MasterPage)Master).SetLabCaseHeader();

            GetLabCaseInformation();
            ResetChangeLabCaseDialog();
            SetLabCaseChangeEnableScript(true);
        }

        protected void btnCloseDummy_Click(object sender, EventArgs e)
        {
            ResetChangeLabCaseDialog();
            SetLabCaseChangeEnableScript(true);
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "_closeLabCaseDialog", "closeLabCaseDialog('mdialog-change-labcase');", true);
        }

        private void UpdateLabCaseNumberFields(string casekey, string sLabCode, int iLabCaseYear, int iLabCaseNumber, string sLabCase)
        {
            PLCQuery qry = new PLCQuery(String.Format("select * from TV_LABCASE where CASE_KEY = {0}", casekey));
            qry.Open();
            if (!qry.IsEmpty())
            {
                qry.Edit();

                if (sLabCode != "")
                    qry.SetFieldValue("LAB_CODE", sLabCode);

                if (sLabCase != "")
                    qry.SetFieldValue("LAB_CASE", sLabCase);

                if (iLabCaseYear != -1)
                    qry.SetFieldValue("LAB_CASE_YEAR", iLabCaseYear);

                if (iLabCaseNumber != -1)
                    qry.SetFieldValue("LAB_CASE_NUMBER", iLabCaseNumber);

                qry.Post("TV_LABCASE", -1, -1);
            }
        }

        private void ReassignValuesOnPostBack()
        {
            labcasenolabel.InnerHtml = PLCDBPanel1.getpanelfield("LAB_CASE");
            lblChangeLabCase.InnerHtml = PLCDBPanel1.GetFieldPrompt("LAB_CASE");
            if (PLCSession.GetLabCtrlFlag("NO_LABCASE_ON_CASE_CREATE") == "T")
            {
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "_setLabCasePopupValues", "setLabCasePopupValues();", true);
            }
        }

        private void GetLabCaseInformation()
        {
            hdnCaseLabCode.Value = "";
            hdnCaseOffenseDate.Value = "";

            PLCQuery qryLabcase = new PLCQuery();
            qryLabcase.SQL = "SELECT LAB_CODE, OFFENSE_DATE FROM TV_LABCASE WHERE CASE_KEY = " + PLCSession.PLCGlobalCaseKey;
            qryLabcase.Open();
            if(!qryLabcase.IsEmpty())
            {
                hdnCaseLabCode.Value = qryLabcase.FieldByName("LAB_CODE");
                hdnCaseOffenseDate.Value = qryLabcase.FieldByName("OFFENSE_DATE");
            }
        }

        protected void btnAutoChangeLabCaseOK_Click(object sender, EventArgs e)
        {
            string sLabCode;
            PLCQuery qryCase = new PLCQuery("select LAB_CODE from TV_LABCASE where CASE_KEY=?");
            qryCase.AddParameter("CASE_KEY", PLCSession.PLCGlobalCaseKey);
            qryCase.Open();
            if (qryCase.HasData())
                sLabCode = qryCase.FieldByName("LAB_CODE");
            else
                sLabCode = PLCDBGlobal.instance.GetMasterLabCode();

            // Update current labcase record with master lab code, current year, next case number, and next labcase string.
            int iLabCaseYear = DateTime.Today.Year;
            int iLabCaseNumber = PLCCommon.instance.GetNextLabCaseNumberIncrement(sLabCode, iLabCaseYear);

            // No next lab case number defined.
            if (iLabCaseNumber < 0)
            {
                this.mbox.ShowMsg("Invalid Lab Case Number", String.Format("The system does not have a next Case Number for the year {0}.<br><br>Call your adminstrator to set up the case number for the year {0} through the Config program.", iLabCaseYear), 1);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "_showChangeLabCasePopup", "showChangeLabCasePopup();", true);
                return;
            }

            string sLabCase = PLCCommon.instance.FormatCaseNumber(sLabCode, iLabCaseYear, iLabCaseNumber);

            PLCDBGlobal.instance.UpdateLabCaseNumber(PLCSession.PLCGlobalCaseKey, sLabCode, iLabCaseYear, iLabCaseNumber, sLabCase);

            PLCDBPanel1.setpanelfield("LAB_CASE", sLabCase);

            SetLabCaseChangeEnableScript(true);
        }

        protected void btnAutoChangeLabCaseCancel_Click(object sender, EventArgs e)
        {
            SetLabCaseChangeEnableScript(true);
        }

        private void ResetChangeLabCaseDialog()
        {
            dbpChangeLabCase.ClearFields();
            dbpChangeLabCase.ClearErrors();

            if (PLCSession.GetLabCtrlFlag("NO_LABCASE_ON_CASE_CREATE") != "T")
            {
                dbpChangeLabCase.PLCDataTable = "TV_LABCASE";
                dbpChangeLabCase.PLCWhereClause = "WHERE CASE_KEY = " + PLCSession.PLCGlobalCaseKey;
                dbpChangeLabCase.DoLoadRecord();
                dbpChangeLabCase.DoEdit();
            }
        }

        #region NICS
        protected void btnNICS_Click(object sender, EventArgs e)
        {
            string deptCaseNumber = PLCDBPanel1.getpanelfield("DEPARTMENT_CASE_NUMBER") != ""
                ? PLCDBPanel1.getpanelfield("DEPARTMENT_CASE_NUMBER")
                : PLCSession.PLCGlobalDepartmentCaseNumber;
            deptCaseNumber = deptCaseNumber.Trim();
            string MD5Hash = AESEncryption.GetMD5HashASCIIUpper(deptCaseNumber + ":LIMS2MIDEO:" + DateTime.Now.ToString("MMddyyyy:HH"));
            string URL = PLCSession.GetWebConfiguration("NICSIMAGESURL") + "?URN=" + deptCaseNumber + "&KEY=" + MD5Hash;
            string script = "setTimeout(function(){window.open('" + URL + "', 'NICS', 'scrollbars=yes,resizable=yes,width=800,height=600,top=yes,left=0,hotkeys=no');}, 500);";


            PLCSessionVars1.WriteDebug("HASHTHIS:" + deptCaseNumber + ":LIMS2MIDEO:" + DateTime.Now.ToString("MMddyyyy:HH"), true) ;
            PLCSessionVars1.WriteDebug("HASHED:" + MD5Hash,true);
            PLCSessionVars1.WriteDebug("Script:" + script,true);

            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), "uniqueKey" + DateTime.Now, script, true);
        }
        #endregion

        private bool ValidateDeleteCase()
        {
            if (string.IsNullOrEmpty(PLCSession.PLCGlobalCaseKey))
                Response.Redirect("Dashboard.aspx");

            string LockedInfo = "";

            if (PLCSession.CheckUserOption("RECEIVEINQ") || !PLCSession.CheckUserOption("DELCASE"))
            {
                mbox.ShowMsg("Delete Case", "You are not authorized to delete this case.", 1);
                return false;
            }
            else if (PLCDBGlobal.instance.IsRecordLocked("TV_LABCASE", PLCSession.PLCGlobalCaseKey, "-1", "-1", out LockedInfo))
            {
                PLCButtonPanel1.SetButtonsForLock(true);

                mbox.ShowMsg("Record Lock", PLCSession.GetSysPrompt("Tab1Case.RECORD_LOCK", "Case locked by another user for editing.") + "<br/>" + LockedInfo, 1);
                lPLCLockStatus.Text = LockedInfo;
                dvPLCLockStatus.Visible = true;
                return false;
            }

            mInput.ShowMsg("Delete Case", "Clicking OK will delete the entire case. Please log why you are deleting this case.", 0, txtDeleteReason.ClientID, btnDeleteCase.ClientID);
            return false;
        }

        protected void btnDeleteCase_Click(object sender, EventArgs e)
        {
            bool usesImgVaultServ = PLCSession.GetLabCtrl("USES_IMAGE_VAULT_SERVICE").Equals("T");

            if (txtDeleteReason.Text.Trim() == "")
            {
                mbox.ShowMsg("Delete Case", "Please enter reason for deleting this case.", 1);
                return;
            }

            if (usesImgVaultServ)
                GetAttachmentEntryIds();

            try
            {
                if (PLCSession.PLCDatabaseServer == "MSSQL")
                {
                    PLCQuery qryDeleteCase = new PLCQuery();
                    qryDeleteCase.AddProcedureParameter("caseKey", Convert.ToInt32(PLCSession.PLCGlobalCaseKey), 10, OleDbType.Integer, ParameterDirection.Input);
                    qryDeleteCase.AddProcedureParameter("deleteReason", txtDeleteReason.Text, txtDeleteReason.Text.Length, OleDbType.VarChar, ParameterDirection.Input);
                    qryDeleteCase.AddProcedureParameter("userId", PLCSession.PLCGlobalAnalyst, 15, OleDbType.VarChar, ParameterDirection.Input);
                    qryDeleteCase.AddProcedureParameter("program", "iLIMS" + PLCSession.PLCBEASTiLIMSVersion, 8, OleDbType.VarChar, ParameterDirection.Input);
                    qryDeleteCase.AddProcedureParameter("OSComputerName", PLCSession.GetOSComputerName(), 50, OleDbType.VarChar, ParameterDirection.Input);
                    qryDeleteCase.AddProcedureParameter("OSUserName", PLCSession.GetOSUserName(), 50, OleDbType.VarChar, ParameterDirection.Input);
                    qryDeleteCase.AddProcedureParameter("OSAddress", PLCSession.GetOSAddress(), 50, OleDbType.VarChar, ParameterDirection.Input);
                    qryDeleteCase.AddProcedureParameter("BuildNumber", PLCSession.PLCBEASTiLIMSVersion, 100, OleDbType.VarChar, ParameterDirection.Input);
                    qryDeleteCase.AddProcedureParameter("labCode", PLCSession.PLCGlobalLabCode, 3, OleDbType.VarChar, ParameterDirection.Input);
                    qryDeleteCase.ExecuteProcedure("DELETE_CASE");
                }
                else
                {
                    PLCQuery qryDeleteCase = new PLCQuery();
                    List<OracleParameter> parameters = new List<OracleParameter>();
                    parameters.Add(new OracleParameter("caseKey", Convert.ToInt32(PLCSession.PLCGlobalCaseKey)));
                    parameters.Add(new OracleParameter("deleteReason", txtDeleteReason.Text));
                    parameters.Add(new OracleParameter("userId", PLCSession.PLCGlobalAnalyst));
                    parameters.Add(new OracleParameter("program", ("iLIMS" + PLCSession.PLCBEASTiLIMSVersion).Substring(0, 8)));
                    parameters.Add(new OracleParameter("OSComputerName", PLCSession.GetOSComputerName()));
                    parameters.Add(new OracleParameter("OSUserName", PLCSession.GetOSUserName()));
                    parameters.Add(new OracleParameter("OSAddress", PLCSession.GetOSAddress()));
                    parameters.Add(new OracleParameter("BuildNumber", PLCSession.PLCBEASTiLIMSVersion));
                    parameters.Add(new OracleParameter("labCode", PLCSession.PLCGlobalLabCode));
                    qryDeleteCase.ExecuteOracleProcedure("DELETE_CASE", parameters);
                }

                if (usesImgVaultServ)
                    DeleteAttachmentsOnServer();

                PLCSession.ClearLabCaseVars();
                mbDeleteOk.ShowMessage("Case", "Case has been deleted.", MessageBoxType.Information);
            }
            catch (Exception ex)
            {
                mbox.ShowMsg("Delete Case", "Case cannot be deleted. Error encountered: " + ex.Message, 1);
            }
        }

        protected void mbDeleteOk_OkClick(object sender, EventArgs e)
        {
            Response.Redirect("Dashboard.aspx");
        }

        protected void mbReload_OkClick(object sender, EventArgs e)
        {
            Response.Redirect("Tab1Case.aspx");
        }

        protected void PLCDBPanel1_TextChanged(object sender, PLCDBPanelTextChangedEventArgs e)
        {
            if (e.FieldName == "DEPARTMENT_CASE_NUMBER")
            {
                AssignDeptPicMask();
            }
        }

        protected void PLCDBPanel1_PLCDBPanelValidate(object sender, PLCDBPanelValidateEventArgs e)
        {
            //    if (e.fldName == "OFFENSE_CATEGORY" && PLCSessionVars1.GetLabCtrl("WEB_USES_ADVANCED_QC") == "C")
            //    {
            //        string offCode = PLCDBPanel1.getpanelfield("OFFENSE_CODE").Trim();
            //        string category = PLCDBPanel1.getpanelfield("OFFENSE_CATEGORY").Trim();

            //        if (!string.IsNullOrEmpty(offCode) && !string.IsNullOrEmpty(category))
            //        {
            //            PLCQuery qryCategory = new PLCQuery("SELECT LEVEL_RES FROM TV_OFFENSE WHERE OFFENSE_CODE = '" + offCode + "'");
            //            qryCategory.Open();
            //            if (qryCategory.HasData())
            //            {
            //                string levelRes = qryCategory.FieldByName("LEVEL_RES").Trim();
            //                if (!string.IsNullOrEmpty(levelRes) && levelRes != category)
            //                {
            //                    e.isValid = false;
            //                    e.errMsg = "Invalid Category for selected Charge.";
            //                }
            //            }
            //        }
            //    }




            FieldRec MyField = new FieldRec(e.custom1, e.fldName, e.fldValue, e.custom2);
            if (SubmissionFieldList == null)
                SubmissionFieldList = new List<FieldRec>();
            SubmissionFieldList.Add(MyField);


        }

        private bool IsValidURNAndOffense()
        {
            //This code is only for LA. So check USES_RD
            if ((PLCSessionVars1.GetLabCtrl("USES_RD") == "T"))
            {

                String thisRefType = PLCDBPanel1.getpanelfield("CASE_REFERENCE_TYPE");

                //CHeck the length of the DCN vs the length of the mask


                //This code only applies to inside departments...
                if (PLCDBGlobalClass.IsSheriffDepartment(PLCDBPanel1.getpanelfield("DEPARTMENT_CODE")))
                {
                    if ( (thisRefType == Constants.REFERENCE_TYPE_MASTERCASE) || (thisRefType == Constants.REFERENCE_TYPE_URN) )
                    {
                        //validate rd code
                        string rd = "";
                        if (PLCDBPanel1.getpanelfield("DEPARTMENT_CASE_NUMBER").ToString().Length == 18)
                            rd = PLCDBPanel1.getpanelfield("DEPARTMENT_CASE_NUMBER").ToString().Substring(10, 4);
                        string testdept = PLCDBGlobalClass.GetRptDstrctDeptCode(rd);
                        if (testdept != PLCDBPanel1.getpanelfield("DEPARTMENT_CODE"))
                        {
                            string headstr = PLCSessionVars1.GetLabCtrl("DEPT_CASE_TEXT");
                            mbox.ShowMsg(headstr , "Please specify a valid " + PLCSessionVars1.GetLabCtrl("DEPT_CASE_TEXT") +
                                " based on the selected Department Code.", 0);
                            return false;
                        }

                        //validate stat code and offense code
                        string statCode = PLCDBPanel1.getpanelfield("DEPARTMENT_CASE_NUMBER").Substring(15, 3);
                        if (!string.IsNullOrEmpty(statCode) && statCode != "999")
                        {
                            //filter charge flexbox based on statcode
                            PLCDBPanel1.SetPanelCodeTable("OFFENSE_CODE", "CV_OFFENSE_STAT");
                            PLCDBPanel1.SetPanelCodeCondition("OFFENSE_CODE", string.Format("STAT_CODE = '{0}'", statCode));
                            PLCSession.SetDefault("OFFENSE_VIEWNAME", "CV_OFFENSE_STAT");
                            PLCSession.SetDefault("OFFENSE_VIEWCOND", (string.IsNullOrEmpty(_OffenseCodeCondition) ? "" : _OffenseCodeCondition + " AND ") + string.Format("STAT_CODE = '{0}'", statCode));


                            string offenseCode = PLCDBPanel1.getpanelfield("OFFENSE_CODE");
                            PLCQuery qryOffense = new PLCQuery(string.Format(@"SELECT 1 AS KEY, COUNT(OFFENSE_CODE) AS CNT FROM CV_OFFENSE_STAT WHERE STAT_CODE = '{0}'
UNION SELECT 2 AS KEY, COUNT(OFFENSE_CODE) AS CNT FROM CV_OFFENSE_STAT WHERE STAT_CODE = '{0}' AND OFFENSE_CODE = '{1}'
ORDER BY KEY", statCode, offenseCode));
                            if (qryOffense.Open() && Convert.ToInt32(qryOffense.PLCDataTable.Rows[0]["CNT"]) > 0)
                            {
                                if (offenseCode != "" && Convert.ToInt32(qryOffense.PLCDataTable.Rows[1]["CNT"]) == 0)
                                {
                                    //current offense code is invalid
                                    mbox.ShowMsg(PLCSessionVars1.GetLabCtrl("DEPT_CASE_TEXT"), "Stat Code has been updated. Please enter a valid Charge #1.", 0);
                                    return false;
                                }
                            }
                            else
                            {
                                //stat code from URN is invalid
                                mbox.ShowMsg(PLCSessionVars1.GetLabCtrl("DEPT_CASE_TEXT"), string.Format("Invalid Offense Status Code ({0}) specified. Please enter a valid URN.", statCode), 0);
                                return false;
                            }
                        }
                        else
                        {
                            //show all offenses
                            PLCDBPanel1.SetPanelCodeTable("OFFENSE_CODE", "TV_OFFENSE");
                            PLCDBPanel1.SetPanelCodeCondition("OFFENSE_CODE", _OffenseCodeCondition);
                            PLCSession.SetDefault("OFFENSE_VIEWNAME", "TV_OFFENSE");
                            PLCSession.SetDefault("OFFENSE_VIEWCOND", _OffenseCodeCondition);
                        }
                    }
                }
            }

            return true;
        }

        private bool IsCaseHasCorrespondence(string CaseKey)
        {
            PLCQuery qrySchedule = new PLCQuery("SELECT * FROM TV_SCHEDULE WHERE CASE_KEY = " + CaseKey);
            if (qrySchedule.Open() && qrySchedule.HasData())
                return true;
            return false;
        }

        private void UpdateOffenseCategoryOptions()
        {
            if (PLCSession.GetLabCtrl("WEB_USES_ADVANCED_QC") == "C")
            {
                if (PLCDBPanel1.HasPanelRec("TV_LABCASE", "OFFENSE_CODE") && PLCDBPanel1.HasPanelRec("TV_LABCASE", "OFFENSE_CATEGORY"))
                {
                    string offCode = PLCDBPanel1.getpanelfield("OFFENSE_CODE").Trim();
                    string category = PLCDBPanel1.getpanelfield("OFFENSE_CATEGORY").Trim();

                    FlexBox fbCategory = PLCDBPanel1.GetFlexBoxControl("OFFENSE_CATEGORY");
                    if (fbCategory.CodeCondition.Contains("{OFFENSE_CODE}"))
                    {
                        ListItemCollection items = PLCDBGlobal.instance.GetOffenseCategories(offCode);
                        fbCategory.DataSource = items;
                        fbCategory.DataBind();

                        if (items.FindByValue(category) == null)
                        {
                            if (items.Count == 1)
                                PLCDBPanel1.setpanelfield("OFFENSE_CATEGORY", items[0].Value);
                            else
                                PLCDBPanel1.setpanelfield("OFFENSE_CATEGORY", "");
                        }
                    }
                }
            }
        }

        private void UpdateOffenseOptions()
        {
            if (PLCSession.GetLabCtrl("WEB_USES_ADVANCED_QC") == "C")
            {
                if (PLCDBPanel1.HasPanelRec("TV_LABCASE", "OFFENSE_CATEGORY") && PLCDBPanel1.HasPanelRec("TV_LABCASE", "OFFENSE_CODE"))
                {
                    string category = PLCDBPanel1.getpanelfield("OFFENSE_CATEGORY").Trim();
                    string offCode = PLCDBPanel1.getpanelfield("OFFENSE_CODE").Trim();

                    FlexBox fbOffense = PLCDBPanel1.GetFlexBoxControl("OFFENSE_CODE");
                    PLCDBPanel.PanelRec prOffense = PLCDBPanel1.GetPanelRecByFieldName("OFFENSE_CODE");

                    if (fbOffense.CodeCondition.Contains("{OFFENSE_CATEGORY}"))
                    {
                        ListItemCollection offenses = new ListItemCollection();
                        PLCQuery qryOffense = new PLCQuery(PLCSession.GenerateCodeHeadSQL(prOffense.codetable, "", prOffense.codecondition,
                            prOffense.chFlexBox.DescriptionFormatCode, prOffense.chFlexBox.DescriptionSeparator, category, prOffense.chFlexBox.ShowActiveOnly, prOffense.chSortOrder));
                        qryOffense.Open();
                        while (!qryOffense.EOF())
                        {
                            offenses.Add(new ListItem(qryOffense.FieldByName("DESCRIPTION"), qryOffense.FieldByName("CODE")));
                            qryOffense.Next();
                        }

                        fbOffense.DataSource = offenses;
                        fbOffense.DataBind();

                        if (offenses.FindByValue(offCode) == null)
                            PLCDBPanel1.setpanelfield("OFFENSE_CODE", "");
                    }
                }
            }
        }

        private void SetAttachmentsButton(string source)
        {
            MasterPage master = (MasterPage)this.Master;
            if (source == "CASE")
            {
                PLCSessionVars1.PLCGlobalAttachmentSource = source;
                PLCSessionVars1.PLCGlobalAttachmentSourceDesc = "Agency: " + PLCDBPanel1.getpanelfield("DEPARTMENT_CODE") + ", " + PLCSessionVars1.GetLabCtrl("DEPT_CASE_TEXT") + ":" + PLCDBPanel1.getpanelfield("DEPARTMENT_CASE_NUMBER");
                master.ApplyImageAttachedClip("LABCASE", PLCSessionVars1.PLCGlobalCaseKey);
            }
        }

        protected void btnActivity_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/ActivityLog.aspx");
        }

        protected void btnFOIA_Click(object sender, EventArgs e)
        {
            string THEPATH = PLCSession.FindCrystalReport("foia.rpt");
            PLCSessionVars1.PLCCrystalReportName = THEPATH;

            PLCSessionVars1.PLCCrystalSelectionFormula = "{TV_LABCASE.CASE_KEY} = " + PLCSessionVars1.PLCGlobalCaseKey;
            PLCSessionVars1.PLCCrystalReportTitle = "Freedom of Information Act for " + PLCSessionVars1.PLCGlobalDepartmentCaseNumber;
            PLCSessionVars1.PrintCRWReport();
        }

        private void SetReadOnlyAccess()
        {
            // Disable controls in the page for read only access
            if (PLCCommonClass.IsReadOnlyAccess("WEBINQ,RONLYCATAB"))
            {
                // Disable plcbuttonpanel and other plcbuttonpanel custom button for read only access
                PLCCommonClass.SetReadOnlyAccess(PLCButtonPanel1, "RecordUnlock");

                btnJIMSReport.Visible = false;
                btnDistribution.Visible = false;
                btnChangeCase.Visible = false;
            }
        }

        protected void btnDistribution_Click(object sender, EventArgs e)
        {
            Response.Redirect("CaseDist.aspx");
        }

        private void GetAttachmentEntryIds()
        {
            string dataType = string.Empty;
            PLCQuery qryImages = new PLCQuery();
            qryImages.SQL = @"SELECT I.ENTRY_ID, P.DATA_TYPE, I.FORMAT, I.IMAGE_ID
FROM TV_IMAGES I 
LEFT OUTER JOIN TV_PRINTLOG P ON P.IMAGE_ID = I.IMAGE_ID 
WHERE P.CASE_KEY = " + PLCSession.PLCGlobalCaseKey +
@" UNION
SELECT I.ENTRY_ID, E.DATA_TYPE, I.FORMAT, I.IMAGE_ID
FROM TV_IMAGES I 
LEFT OUTER JOIN TV_EXAMIMAG E ON E.IMAGES_TABLE_ID = I.IMAGE_ID
INNER JOIN TV_LABEXAM LE ON LE.EXAM_KEY = E.EXAM_KEY
WHERE LE.CASE_KEY = " + PLCSession.PLCGlobalCaseKey +
@" UNION 
SELECT I.ENTRY_ID, E.DATA_TYPE, I.FORMAT, I.IMAGE_ID
FROM TV_IMAGES I 
INNER JOIN TV_EXAMIMAG E ON E.ANNOTATED_IMAGES_TABLE_ID = I.IMAGE_ID
INNER JOIN TV_LABEXAM LE ON LE.EXAM_KEY = E.EXAM_KEY
WHERE LE.CASE_KEY = " + PLCSession.PLCGlobalCaseKey;
            qryImages.Open();

            if (qryImages.IsEmpty())
                return;

            while (!qryImages.EOF())
            {
                dataType = !string.IsNullOrEmpty(qryImages.FieldByName("DATA_TYPE")) ? qryImages.FieldByName("DATA_TYPE") : qryImages.FieldByName("FORMAT");

                if(!IsImageReferencedByOthers(qryImages.FieldByName("IMAGE_ID")))
                    AttachmentEntryIDs.Add(qryImages.FieldByName("ENTRY_ID") + "." + dataType);

                qryImages.Next();
            }
        }

        private void DeleteAttachmentsOnServer()
        {
            if (AttachmentEntryIDs.Count <= 0)
                return;

            PLCHelperFunctions PLCHelper = new PLCHelperFunctions();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "_deleteCaseAttachments" + DateTime.Now.ToString(), "DeleteAttachmentsOnServer('" + PLCHelper.GetDeleteIVFromServerURL("") + "', ['" + string.Join("','", AttachmentEntryIDs) + "']);", true);

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

        private bool CheckUserActLogAccess()
        {
            bool hasRecords = true;

            if (PLCSession.CheckUserOption("ACTLOGUSER"))
            {
                PLCQuery qryActivity = new PLCQuery();
                qryActivity.SQL = "SELECT * FROM TV_ACTIVITY WHERE ANALYST = ? AND ACTIVITY_KEY IN (SELECT ACTIVITY_KEY FROM TV_ACTCASES WHERE CASE_KEY = ?)";
                qryActivity.AddParameter("ANALYST", PLCSession.PLCGlobalAnalyst);
                qryActivity.AddParameter("CASE_KEY", PLCSession.PLCGlobalCaseKey);
                qryActivity.Open();

                hasRecords = qryActivity.HasData();
            }

            return hasRecords;
        }


        protected void btnLaboffense_Click(object sender, EventArgs e)
        {
            grdCustomCaseInfo.PLCGridName = "CUSTOMCASEINFO";
            string sql = "SELECT * FROM CV_CASEINFO WHERE CASE_KEY =  " + PLCSessionVars1.PLCGlobalCaseKey;
            grdCustomCaseInfo.PLCSQLString = PLCSessionVars1.FormatSpecialFunctions(sql);
            grdCustomCaseInfo.InitializePLCDBGrid();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowLabOffensePopUp", "ShowLabOffensePopUp();", true);
        }

        private string GetOffenseCode(string suppKey)
        {
            if (!string.IsNullOrEmpty(suppKey))
            {
                PLCQuery qryOffense = new PLCQuery();
                qryOffense.SQL = "SELECT * FROM TV_LABOFFENSE WHERE SUPPKEY = ? AND CASE_KEY = ?";
                qryOffense.AddParameter("SUPPKEY", suppKey);
                qryOffense.AddParameter("CASE_KEY", PLCSession.PLCGlobalCaseKey);
                qryOffense.Open();
                if (!qryOffense.IsEmpty())
                    return qryOffense.FieldByName("OFFENSE_CODE");
            }


            return string.Empty;

        }


        private void UpdateLabOffenseCodes()
        {
            PLCQuery qryLabCase = new PLCQuery();
            qryLabCase.SQL = "SELECT * FROM TV_LABCASE WHERE CASE_KEY = " + PLCSession.PLCGlobalCaseKey;
            qryLabCase.Open();
            if (!qryLabCase.IsEmpty())
            {
                qryLabCase.Edit();
                if(PLCDBPanel1.HasPanelRec("TV_LABCASE","OFFENSE_KEY_1"))
                    qryLabCase.SetFieldValue("OFFENSE_CODE", GetOffenseCode(PLCDBPanel1.getpanelfield("OFFENSE_KEY_1")));
                if(PLCDBPanel1.HasPanelRec("TV_LABCASE","OFFENSE_KEY_2"))
                    qryLabCase.SetFieldValue("OFFENSE_CODE_2", GetOffenseCode(PLCDBPanel1.getpanelfield("OFFENSE_KEY_2")));
                if(PLCDBPanel1.HasPanelRec("TV_LABCASE","OFFENSE_KEY_3"))
                    qryLabCase.SetFieldValue("OFFENSE_CODE_3", GetOffenseCode(PLCDBPanel1.getpanelfield("OFFENSE_KEY_3")));

                qryLabCase.Post("TV_LABCASE", 5, 1);

            }

        }

        protected void btnCodisHits_Click(object sender, EventArgs e)
        {
            grdCodisHits.PLCGridName = "CODIS_HITS";
            grdCodisHits.ClearSortExpression();
            grdCodisHitsDoc.PageIndex = 0;
            grdCodisHits.InitializePLCDBGrid();
            grdCodisHits.SelectedIndex = 0;
            LoadCodisHitsDocument();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowCodisHitsPopUp", "ShowCodisHitsPopUp();", true);
        }

        protected void grdCodisHits_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadCodisHitsDocument();
        }

        protected void grdCodisHits_Sorting(object sender, GridViewSortEventArgs e)
        {
            LoadCodisHitsDocument();
        }

        protected void grdCodisHitsDoc_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (grdCodisHitsDoc.SelectedDataKey != null)
            {
                hdnCodisHitNumber.Value = grdCodisHitsDoc.SelectedDataKey["HIT_NUMBER"].ToString();
                hdnDocSource.Value = grdCodisHitsDoc.SelectedDataKey["SOURCE"].ToString();
                btnOpenCodisHitPopup.Enabled = true;
            }
            else
                btnOpenCodisHitPopup.Enabled = false;
        }

        private void LoadCodisHitsDocument()
        {
            // make sure there is a selected codis hit
            if (grdCodisHits.SelectedDataKey != null)
            {
                string hitNumber = grdCodisHits.SelectedDataKey["HIT_NUMBER"].ToString();
                grdCodisHitsDoc.PLCGridName = "CODIS_HITS_DOCUMENT";
                string sql = @"SELECT FILE_SOURCE_KEY AS HIT_NUMBER, DATA_KEY + '.' + FORMAT AS DOCUMENT_DESCRIPTION, FORMAT,
'PDFDATA' AS SOURCE, DATA_KEY AS PKEY FROM TV_PDFDATA WHERE FILE_SOURCE_KEY = " + hitNumber + @" AND UPPER(FILE_SOURCE) = 'HITINFO'
UNION ALL
SELECT HIT_NUMBER, HIT_NUMBER + '.DOC' AS DOCUMENT_DESCRIPTION, 
'DOC' AS FORMAT, 'WORDCOD' AS SOURCE, HIT_NUMBER as PKEY
FROM TV_WORDCOD 
WHERE HIT_NUMBER = " + hitNumber + @"
UNION ALL 
SELECT FILE_SOURCE_KEY1 AS HIT_NUMBER, 
DOCUMENT_DESCRIPTION + '.' + DATA_TYPE AS DOCUMENT_DESCRIPTION, 
DATA_TYPE AS FORMAT, 'IMAGE VAULT' AS SOURCE, PRINTLOG_KEY AS PKEY
FROM TV_COPRNTLOG 
WHERE DNA_CASE_KEY = 0 
AND UPPER(FILE_SOURCE) = 'HIT INFO' 
AND FILE_SOURCE_KEY1  = " + hitNumber + @"
AND FILE_SOURCE_KEY2 = 0";
                grdCodisHitsDoc.PLCSQLString = PLCSessionVars1.FormatSpecialFunctions(sql);
                grdCodisHitsDoc.ClearSortExpression();
                grdCodisHitsDoc.PageIndex = 0;
                grdCodisHitsDoc.InitializePLCDBGrid();
                grdCodisHitsDoc.SelectedIndex = 0;

                if (grdCodisHitsDoc.Rows.Count == 0)
                {
                    btnOpenCodisHitPopup.Enabled = false;
                    hdnCodisHitNumber.Value = "";
                    hdnDocSource.Value = "";
                }
                else
                {
                    btnOpenCodisHitPopup.Enabled = true;
                    hdnCodisHitNumber.Value = grdCodisHitsDoc.SelectedDataKey["HIT_NUMBER"].ToString();
                    hdnDocSource.Value = grdCodisHitsDoc.SelectedDataKey["SOURCE"].ToString();
                }
            }
        }

        protected void btnAddressLabel_Click(object sender, EventArgs e)
        {
            string THEPATH = PLCSession.FindCrystalReport("caseaddress.rpt");
            PLCSessionVars1.PLCCrystalReportName = THEPATH;

            PLCSessionVars1.PLCCrystalSelectionFormula = "{TV_LABCASE.CASE_KEY} = " + PLCSessionVars1.PLCGlobalCaseKey;
            PLCSessionVars1.PLCCrystalReportTitle = "Address Label"; // for " + PLCSessionVars1.PLCGlobalDepartmentCaseNumber;
            PLCSessionVars1.PrintCRWReport();
        }

        protected void btnSeizureTrack_Click(object sender, EventArgs e)
        {
            InitSeizureItemsGrid();
            InitSeizureTracking();
            
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowSeizureTrackPopUp", "ShowSeizureTrackPopUp();", true);
        }

        protected void dlgMsg_ConfirmClick(object sender, DialogEventArgs e)
        {
            DialogKeys dialogKey;
            if (Enum.TryParse<DialogKeys>(e.DialogKey, out dialogKey))
            {
                switch (dialogKey)
                {
                    case DialogKeys.CCAPForceUpdate:
                        ForceSyncCCAPData();
                        break;
                }
            }
        }

        private bool HasDistribution(string caseKey)
        {
            PLCQuery qryCaseDist = new PLCQuery();
            qryCaseDist.SQL = "SELECT * FROM TV_CASEDIST WHERE CASE_KEY = ?";
            qryCaseDist.AddSQLParameter("CASE_KEY", caseKey);
            qryCaseDist.Open();
            return qryCaseDist.HasData();
        }


        private bool HasDiscoveryRequests(string caseKey)
        {
            if (!string.IsNullOrEmpty(caseKey))
            {
                PLCQuery qryDiscovery = new PLCQuery();
                qryDiscovery.SQL = "SELECT * FROM TV_DISCOVERY WHERE CASE_KEY = ?";
                qryDiscovery.AddSQLParameter("CASE_KEY", caseKey);
                qryDiscovery.Open();
                return qryDiscovery.HasData();
            }

            return false;
        }

        #region Seizure Tracking

        protected void dbgSeizureTrack_SelectedIndexChanged(object sender, EventArgs e)
        {
            GrabSeizureTracking();
        }

        protected void dbgSeizureTrack_Sorted(object sender, EventArgs e)
        {
            dbgSeizureTrack.SelectedIndex = 0;
            GrabSeizureTracking();
        }

        protected void dbgSeizureTrack_PageIndexChanged(object sender, EventArgs e)
        {
            dbgSeizureTrack.SelectedIndex = 0;
            GrabSeizureTracking();
        }

        protected void dbgSeizureItems_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            /*if (e.Row.RowType == DataControlRowType.DataRow)
            {
                CheckBox cbSelect_All = (CheckBox)dbgSeizureItems.HeaderRow.Cells[0].FindControl("cbSelect_All");
                cbSelect_All.Visible = false;
            }*/

            if(e.Row.RowType == DataControlRowType.Header)
            {
                //e.Row.Attributes["style"] = "pointer-events: none";
            }

            if (dbgSeizureItems.Rows.Count > 0 && !IsDBGridExists(dbgSeizureItems.PLCGridName))
                dbgSeizureItems.Columns[1].Visible = false;
        }

        protected void dbgSeizureItems_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                //e.Row.Attributes["style"] = "pointer-events: none";
            }
        }

        protected void bpSeizureTrack_PLCButtonClick(object sender, PLCCONTROLS.PLCButtonClickEventArgs e)
        {
            if (e.button_name.Equals("ADD"))
            {
                if (e.button_action.Equals("AFTER"))
                {
                    dbpSeizureTrack.EnablePanelFieldByAttrib("LIMIT45_DATE", false);
                    dbpSeizureTrack.EnablePanelFieldByAttrib("LIMIT90_DATE", false);

                    SetSeizureTrackControls(false);
                    UnCheckAllSeizureItems();
                    LoadAvailableSeizureItems();
                }
            }
            else if (e.button_name.Equals("EDIT"))
            {
                if(e.button_action.Equals("AFTER"))
                {
                    dbpSeizureTrack.EnablePanelFieldByAttrib("SEIZURE_NBR", false);
                    dbpSeizureTrack.EnablePanelFieldByAttrib("LIMIT45_DATE", false);
                    dbpSeizureTrack.EnablePanelFieldByAttrib("LIMIT90_DATE", false);

                    SetSeizureTrackControls(false);
                    LoadAvailableSeizureItems(dbpSeizureTrack.GetOriginalValue("SEIZURE_NBR"));
                }
            }
            else if (e.button_name.Equals("SAVE"))
            {
                if (e.button_action.Equals("BEFORE"))
                {
                    string msg = ValidateSeizureRecord(e.row_added);
                    if(!string.IsNullOrEmpty(msg))
                    {
                        bpSeizureTrack.SetSaveError(msg);
                        e.button_canceled = true;
                        return;
                    }
                    else if(!dbpSeizureTrack.CanSave())
                    {
                        e.button_canceled = true;
                        return;
                    }

                    SaveSeizurePanel();

                    dbgSeizureTrack.InitializePLCDBGrid();
                    string seizureNumber = dbpSeizureTrack.getpanelfield("SEIZURE_NBR").Trim();
                    dbgSeizureTrack.SelectRowByDataKeyIfExists(seizureNumber);
                    GrabSeizureTracking();
                    SetSeizureTrackControls(true);

                    e.button_canceled = true;
                }
            }
            if (e.button_name.Equals("CANCEL"))
            {
                if (e.button_action.Equals("AFTER"))
                {
                    ReloadSeizureRecords();
                    SetSeizureTrackControls(true);
                }
            }
            if (e.button_name.Equals("DELETE"))
            {
                if (e.button_action.Equals("AFTER"))
                {
                    DeleteSeizureRecord();
                    dbgSeizureTrack.SelectedIndex = 0;
                    ReloadSeizureRecords();
                    SetSeizureTrackControls(true);
                }
            }
            else if (e.button_name.Equals("Close"))
            {
                string withRecord = dbgSeizureTrack.Rows.Count > 0 ? "T" : "F";
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "closeSeizureTrackPopup" + DateTime.Now.ToString(), "CloseSeizureTrackPopup('" + withRecord + "');", true);
            }
        }

        protected void dbpSeizureTrack_TextChanged(object sender, PLCDBPanelTextChangedEventArgs e)
        {
            if(e.FieldName.ToUpper().Equals("LETTER_SENT_DATE"))
            {
                SetSeizureDateLimit("LETTER_SENT_DATE", "LIMIT45_DATE", 45);
            }
            else if (e.FieldName.ToUpper().Equals("CLAIM_DATE"))
            {
                SetSeizureDateLimit("CLAIM_DATE", "LIMIT90_DATE", 90);
            }
        }

        private void SetSeizureDateLimit(string baseDateField, string limitDateField, int days)
        {
            var basePanelRec = dbpSeizureTrack.GetPanelRecByFieldName(baseDateField);

            CultureInfo culture;
            if (basePanelRec.editmask.ToUpper().StartsWith("DD"))
                culture = CultureInfo.CreateSpecificCulture("en-GB");
            else
                culture = CultureInfo.CreateSpecificCulture("en-US");

            string baseDate = dbpSeizureTrack.getpanelfield(baseDateField);
            DateTime dtBaseDate = new DateTime();
            bool isValidBaseDate = DateTime.TryParse(baseDate, culture, DateTimeStyles.None, out dtBaseDate);

            if(isValidBaseDate)
            {
                DateTime dtLimitDate = dtBaseDate.AddDays(days);
                var limitDateRec = dbpSeizureTrack.GetPanelRecByFieldName(limitDateField);
                dbpSeizureTrack.setpanelfield(limitDateField, dtLimitDate.ToString(limitDateRec.editmask.Replace("m", "M").Replace("D", "d").Replace("Y", "y")));
            }
            else
            {
                dbpSeizureTrack.setpanelfield(limitDateField, "");
            }
        }

        private void InitSeizureTracking()
        {
            InitSeizureTrackGrid();

            if (dbgSeizureTrack.Rows.Count > 0)
            {
                btnSeizureTrack.Style["color"] = "Red";
                dbgSeizureTrack.SelectedIndex = 0;
                GrabSeizureTracking();
                bpSeizureTrack.SetBrowseMode();
            }
            else
            {
                btnSeizureTrack.Style["color"] = "Black";
                dbpSeizureTrack.EmptyMode();
                bpSeizureTrack.SetEmptyMode();
                bpSeizureTrack.SetCustomButtonEnabled("Close", true);
            }

            SetSeizureTrackControls(true);
        }

        private void ReloadSeizureRecords()
        {
            InitSeizureTrackGrid();

            if (dbgSeizureTrack.Rows.Count <= 0)
            {
                dbpSeizureTrack.EmptyMode();
                bpSeizureTrack.SetEmptyMode();
                bpSeizureTrack.SetCustomButtonEnabled("Close", true);
                UnCheckAllSeizureItems();
            }
            else
            {
                GrabSeizureTracking();
            }
        }

        private void GrabSeizureTracking()
        {
            string seizureNum = dbgSeizureTrack.SelectedDataKey["SEIZURE_NBR"].ToString();
            dbpSeizureTrack.PLCWhereClause = "WHERE SEIZURE_NBR = '" + seizureNum + "'";
            dbpSeizureTrack.DoLoadRecord();
            bpSeizureTrack.SetBrowseMode();

            LoadSeizureItems(seizureNum);
        }

        private void LoadSeizureItems(string seizureNumber)
        {
            PLCQuery qrySeizureNum = new PLCQuery();
            qrySeizureNum.SQL = "SELECT EVIDENCE_CONTROL_NUMBER FROM TV_SEIZURE WHERE SEIZURE_NBR = ?";
            qrySeizureNum.AddSQLParameter("SEIZURE_NBR", seizureNumber);
            qrySeizureNum.Open();

            if (qrySeizureNum.IsEmpty())
                return;

            List<string> liECN = new List<string>();
            while (!qrySeizureNum.EOF())
            {
                liECN.Add(qrySeizureNum.FieldByName("EVIDENCE_CONTROL_NUMBER"));
                qrySeizureNum.Next();
            }

            bool allRecordschecked = true;
            foreach (GridViewRow row in dbgSeizureItems.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    string ecn = dbgSeizureItems.DataKeys[row.RowIndex].Values["EVIDENCE_CONTROL_NUMBER"].ToString().Trim();
                    if (liECN.Contains(ecn))
                    {
                        ((CheckBox)row.FindControl("cbSELECT")).Enabled = false;
                        ((CheckBox)row.FindControl("cbSELECT")).Checked = true;
                    }
                    else
                    {
                        allRecordschecked = false;
                        ((CheckBox)row.FindControl("cbSELECT")).Checked = false;
                    }
                }
            }

            CheckBox cbSelect_All = (CheckBox)dbgSeizureItems.HeaderRow.Cells[0].FindControl("cbSelect_All");
            cbSelect_All.Checked = allRecordschecked;
        }

        private void LoadAvailableSeizureItems(string seizureNumber="")
        {
            PLCQuery qrySeizureNum = new PLCQuery();
            qrySeizureNum.SQL = "SELECT EVIDENCE_CONTROL_NUMBER FROM TV_SEIZURE WHERE Case_Key = ?";
            qrySeizureNum.AddSQLParameter("Case_Key", PLCSession.PLCGlobalCaseKey);

            if(!string.IsNullOrEmpty(seizureNumber))
            {
                qrySeizureNum.SQL = qrySeizureNum.SQL + " AND UPPER(Seizure_Nbr) NOT IN ('" + seizureNumber + "')";
            }

            qrySeizureNum.OpenReadOnly();
            if (qrySeizureNum.IsEmpty())
                return;

            List<string> liECN = new List<string>();
            while (!qrySeizureNum.EOF())
            {
                liECN.Add(qrySeizureNum.FieldByName("EVIDENCE_CONTROL_NUMBER"));
                qrySeizureNum.Next();
            }

            bool hasAvailable = false;
            foreach (GridViewRow row in dbgSeizureItems.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    string ecn = dbgSeizureItems.DataKeys[row.RowIndex].Values["EVIDENCE_CONTROL_NUMBER"].ToString().Trim();
                    ((CheckBox)row.FindControl("cbSELECT")).Enabled = !liECN.Contains(ecn);

                    if (!liECN.Contains(ecn))
                        hasAvailable = true;
                }
            }

            CheckBox cbSelect_All = (CheckBox)dbgSeizureItems.HeaderRow.Cells[0].FindControl("cbSelect_All");
            cbSelect_All.Enabled = hasAvailable;
        }

        public void SetSeizureTrackControls(bool browse)
        {
            dbgSeizureTrack.Enabled = browse;

            CheckBox cbSelect_All = (CheckBox)dbgSeizureItems.HeaderRow.Cells[0].FindControl("cbSelect_All");
            cbSelect_All.Enabled = !browse;

            foreach (GridViewRow row in dbgSeizureItems.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    ((CheckBox)row.FindControl("cbSELECT")).Enabled = !browse;
                }
            }
        }

        private void UnCheckAllSeizureItems()
        {
            foreach (GridViewRow row in dbgSeizureItems.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    ((CheckBox)row.FindControl("cbSELECT")).Checked = false;
                }
            }
        }

        public string ValidateSeizureRecord(bool newRecord)
        {
            string msg = string.Empty;
            string seizureNumber = dbpSeizureTrack.getpanelfield("SEIZURE_NBR");

            if (string.IsNullOrEmpty(seizureNumber))
                return "Seizure Number blank";

            if (newRecord)
            {
                PLCQuery qrySeizureNum = new PLCQuery();
                qrySeizureNum.SQL = "SELECT EVIDENCE_CONTROL_NUMBER FROM TV_SEIZURE WHERE UPPER(SEIZURE_NBR) = ?";
                qrySeizureNum.AddSQLParameter("SEIZURE_NBR", seizureNumber.Trim().ToUpper());
                qrySeizureNum.OpenReadOnly();

                if (qrySeizureNum.HasData())
                    msg = "Seizure Number already exists.";
            }

            if (GetSeizureCheckedDataKeys().Count() <= 0)
                msg = "Please select an item";

            return msg;
        }

        private void SaveSeizurePanel()
        {
            List<DataKey> dataKeyECN = GetSeizureCheckedDataKeys();
            string seizureNumber = dbpSeizureTrack.getpanelfield("SEIZURE_NBR").Trim();

            // override previous records


            List<string> liECNs = new List<string>();
            foreach (DataKey key in dataKeyECN)
            {
                liECNs.Add(key.Value.ToString());
                SaveSeizureRecord(seizureNumber, key.Value.ToString());
            }

            string deleteClause = "WHERE SEIZURE_NBR = '" + seizureNumber + "' AND CASE_KEY = " + PLCSession.PLCGlobalCaseKey + " AND EVIDENCE_CONTROL_NUMBER NOT IN (" + string.Join(",", liECNs) + ")";
            PLCQuery qryDelete = new PLCQuery();
            qryDelete.Delete("TV_SEIZURE", deleteClause, 460, 3);
        }

        private void DeleteSeizureRecord()
        {
            string seizureNumber = dbpSeizureTrack.getpanelfield("SEIZURE_NBR").Trim();

            // override previous records
            PLCQuery qryDelete = new PLCQuery();
            qryDelete.Delete("TV_SEIZURE", "WHERE SEIZURE_NBR = '" + seizureNumber + "' AND CASE_KEY = " + PLCSession.PLCGlobalCaseKey, 460, 3);
        }

        private void SaveSeizureRecord(string seizureNumber, string ecn)
        {
            int subCode = 1;
            PLCQuery qrySeizure = new PLCQuery();
            qrySeizure.SQL = "SELECT * FROM TV_SEIZURE WHERE SEIZURE_NBR = '" + seizureNumber + "' AND CASE_KEY = " + PLCSession.PLCGlobalCaseKey + " AND EVIDENCE_CONTROL_NUMBER = " + ecn;
            qrySeizure.Open();

            if (qrySeizure.IsEmpty())
            {
                qrySeizure.Append();
                qrySeizure.SetFieldValue("SEIZURE_NBR", seizureNumber);
                qrySeizure.SetFieldValue("Case_Key", PLCSession.PLCGlobalCaseKey);
                qrySeizure.SetFieldValue("EVIDENCE_CONTROL_NUMBER", ecn);
            }
            else
            {
                subCode = 2;
                qrySeizure.Edit();
            }
            

            List<string> fieldNames = dbpSeizureTrack.GetFieldNames();
            fieldNames.Remove("SEIZURE_NBR");
            fieldNames.Remove("CASE_KEY");
            fieldNames.Remove("EVIDENCE_CONTROL_NUMBER");

            foreach (string fieldName in fieldNames)
                qrySeizure.SetFieldValue(fieldName, dbpSeizureTrack.getpanelfield(fieldName));

            qrySeizure.Post("TV_SEIZURE", 460, subCode);
        }

        public List<DataKey> GetSeizureCheckedDataKeys()
        {
            List<DataKey> result = new List<DataKey>();

            OrderedDictionary dictionary;
            foreach (GridViewRow row in dbgSeizureItems.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    if (((CheckBox)row.FindControl("cbSELECT")).Checked)
                    {
                        dictionary = new OrderedDictionary();
                        foreach (String keyname in dbgSeizureItems.DataKeyNames)
                        {
                            string ecn = dbgSeizureItems.DataKeys[row.RowIndex].Values[keyname].ToString().Trim();
                            dictionary.Add(keyname, ecn);
                        }
                        result.Add(new DataKey(dictionary));
                    }
                }
            }
            return result;
        }

        private void SetSeizurePermission()
        {
            if (!PLCSession.CheckUserOption("CANADDSEIZ"))
                bpSeizureTrack.HideAddButton();
            if (!PLCSession.CheckUserOption("CANDELSEIZ"))
                bpSeizureTrack.HideDeleteButton();
            if (!PLCSession.CheckUserOption("CANEDITSEIZ"))
                bpSeizureTrack.HideEditButton();
        }

        private bool IsDBGridExists(string gridName)
        {
            PLCQuery qryDBGRIDHD = new PLCQuery();
            qryDBGRIDHD = CacheHelper.OpenCachedSqlReadOnly("SELECT * FROM TV_DBGRIDHD WHERE GRID_NAME = '" + gridName + "'");

            return qryDBGRIDHD.HasData();
        }

        private void InitSeizureItemsGrid()
        {
            if (!IsDBGridExists(dbgSeizureItems.PLCGridName))
            {
                string sql = "SELECT EVIDENCE_CONTROL_NUMBER, '\"' + B.ITEM_TYPE + '\" ' + A.LAB_ITEM_NUMBER + ' - ' + B.DESCRIPTION AS \"Item List\" " +
                            "FROM TV_LABITEM A " +
                            "LEFT OUTER JOIN TV_ITEMTYPE B ON B.ITEM_TYPE = A.ITEM_TYPE " +
                            "WHERE A.CASE_KEY = " + PLCSession.PLCGlobalCaseKey;
                dbgSeizureItems.PLCSQLString = sql;
            }
            dbgSeizureItems.InitializePLCDBGrid();
            dbgSeizureItems.UsesScrollbar = true;
        }

        private void InitSeizureTrackGrid()
        {
            if (!IsDBGridExists(dbgSeizureTrack.PLCGridName))
            {
                string sql = "SELECT DISTINCT(SEIZURE_NBR) " +
                            "FROM TV_SEIZURE " +
                            "WHERE CASE_KEY = " + PLCSession.PLCGlobalCaseKey;
                dbgSeizureTrack.PLCSQLString = sql;
            }
            dbgSeizureTrack.InitializePLCDBGrid();
            dbgSeizureTrack.UsesScrollbar = true;
        }

        #endregion

        #region Change Department Code/Case 


        protected void btnChangeCase_Click(object sender, EventArgs e)
        {
            fbDepartmentCode.Enabled = PLCSession.CheckUserOption("CHANGECASEDEPTCODEPL") || !IsLinkedToPrelog();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "_showChangeCaseReasonPopup", "showChangeCaseReasonPopup();", true);
        }

        protected void btnReasonOK_Click(object sender, EventArgs e)
        {
            PLCSession.SetDefault("REASONTEXT", "");

            if (!string.IsNullOrWhiteSpace(txtChangeCaseReason.Text))
            {
                PLCSession.SetDefault("REASONTEXT", txtChangeCaseReason.Text);            
                InitializeChangeDepartmentCodeCasePopup();
                upChangeCasePopup.Update();
            }


        }

        protected void fbDepartmentCode_Changed(object sender, EventArgs e)
        {
            string departmentCode = fbDepartmentCode.GetValue();
            SetDepartmentCaseNumberMask(departmentCode);
            ScriptManager.RegisterStartupScript(this, this.GetType(), "_positionFlexboxOptions", "positionFlexboxOptions($('#mdialog-change-case'));", true);

        }

         protected void btnUpdateDeptInfo_Click(object sender, EventArgs e)
         {
            try
            {
                ChangeDepartmentCaseInfo();
                ScriptManager.RegisterStartupScript(this, this.GetType(), "_positionFlexboxOptions", "positionFlexboxOptions($('#mdialog-change-case'));", true);
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "_closeDialog", "closeDialog('mdialog-change-case');", true);
                dlgMessage.ShowAlert("Change Department/Case Number", "Case cannot be updated. Error encountered: " + ex.Message);
                return;
            }
        }
     
        private void SetDepartmentCaseNumberMask(string departmentCode)
        {
            string pictureMask = PLCDBGlobal.instance.GetDeptPictureMask(departmentCode);
            meChangeDepartment.Mask = !string.IsNullOrEmpty(pictureMask) ? pictureMask : _freeFormDeptPicMask();
        }


        private void InitializeChangeDepartmentCodeCasePopup()
        {
            SetDepartmentCaseNumberMask(PLCDBPanel1.getpanelfield("DEPARTMENT_CODE"));     
            ScriptManager.RegisterStartupScript(this, this.GetType(), "_showChangeCasePopup", "showChangeCasePopup();", true);
              
        }

        private bool IsLinkedToPrelog()
        {
            PLCQuery qryCase = new PLCQuery();
            qryCase.SQL = "SELECT * FROM TV_SUBMTRAN WHERE CASE_KEY = ?";
            qryCase.AddSQLParameter("CASE_KEY", PLCSession.PLCGlobalCaseKey);
            qryCase.Open();
            return qryCase.HasData();
        }
     

        private void ChangeDepartmentCaseInfo()
        {
            bool isLinkedToPrelog = !fbDepartmentCode.Enabled;

            string oldDeptCode = PLCDBPanel1.getpanelfield("DEPARTMENT_CODE");
            string oldDepartmentCase = PLCDBPanel1.GetOriginalValue("DEPARTMENT_CASE_NUMBER").ToUpper();
            string newDepartmentCode = fbDepartmentCode.GetValue();
            string newDeptCase = txtChangeDepartment.Text.ToUpper().Trim('_');


            if(string.IsNullOrWhiteSpace(newDepartmentCode) && string.IsNullOrWhiteSpace(newDeptCase))
            {
                string message = "Department code/case number is required";
                if(isLinkedToPrelog)              
                    message = "Department case number is required";
                
                dlgMessage.ShowAlert("Change Department/Case Number", message);
                return;
            }
            else
            {

                string departmentCode = !string.IsNullOrWhiteSpace(newDepartmentCode) ? newDepartmentCode : oldDeptCode;
                string departmentCase = !string.IsNullOrWhiteSpace(newDeptCase) ? newDeptCase : oldDepartmentCase;

                if (!string.IsNullOrWhiteSpace(newDepartmentCode) && newDepartmentCode.Equals(oldDeptCode))
                {                                    
                    dlgMessage.ShowAlert("Change Department/Case Number", "Please select other Department Code");                                    
                    return;
                }

                if (!string.IsNullOrWhiteSpace(newDeptCase) && newDeptCase.Equals(oldDepartmentCase))
                {
                    dlgMessage.ShowAlert("Change Department/Case Number", "Please enter new Department Case Number");                  
                    return;
                }

                if (LIMSCaseAlreadyExists(departmentCase, departmentCode))
                {
                    dlgMessage.ShowAlert("Change Department/Case Number", "Case already exists in LIMS");
                    return;
                }
          

                if ((newDeptCase != oldDepartmentCase || newDepartmentCode != oldDeptCode)
                        && (!string.IsNullOrWhiteSpace(newDeptCase) || !string.IsNullOrWhiteSpace(newDepartmentCode)))
                {
                    UpdateCaseInfo(oldDepartmentCase, newDeptCase, newDepartmentCode, oldDeptCode);
                }
            }

        }


        private void UpdateCaseInfo(string oldDepartmentCase, string newDeptCase, string newDepartmentCode, string oldDeptCode)
        {         
            PLCQuery qry = new PLCQuery();
            qry.AddProcedureParameter("caseKey", PLCSession.SafeInt(PLCSession.PLCGlobalCaseKey), 10, OleDbType.Integer, ParameterDirection.Input);
            qry.AddProcedureParameter("oldDepartmentCaseNumber", oldDepartmentCase, 35, OleDbType.VarChar, ParameterDirection.Input);
            qry.AddProcedureParameter("newDepartmentCaseNumber", newDeptCase, 35, OleDbType.VarChar, ParameterDirection.Input);
            qry.AddProcedureParameter("newDepartmentCode", newDepartmentCode, 9, OleDbType.VarChar, ParameterDirection.Input);
            qry.AddProcedureParameter("departmentCode", oldDeptCode, 9, OleDbType.VarChar, ParameterDirection.Input);
            qry.AddProcedureParameter("userId", PLCSession.PLCGlobalAnalyst, 15, OleDbType.VarChar, ParameterDirection.Input);
            qry.AddProcedureParameter("programName", "iLIMS" + PLCSession.PLCBEASTiLIMSVersion, 8, OleDbType.VarChar, ParameterDirection.Input);
            qry.AddProcedureParameter("osComputerName", PLCSession.GetOSComputerName(), 50, OleDbType.VarChar, ParameterDirection.Input);
            qry.AddProcedureParameter("osUserName", PLCSession.GetOSUserName(), 50, OleDbType.VarChar, ParameterDirection.Input);
            qry.AddProcedureParameter("osAddress", PLCSession.GetOSAddress(), 50, OleDbType.VarChar, ParameterDirection.Input);
            qry.AddProcedureParameter("buildNumber", PLCSession.PLCBEASTiLIMSVersion, 100, OleDbType.VarChar, ParameterDirection.Input);
            qry.ExecuteProcedure("UPDATE_DEPT_CASENUMBER");


            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "_closeDialog", "closeDialog('mdialog-change-case');", true);

            try
            {
                string caseTitleCacheKey = "CASETITLE@@@" + PLCSessionVars1.PLCGlobalCaseKey;
                CacheHelper.DeleteItem(caseTitleCacheKey);
                ((MasterPage)Master).SetCaseTitle("");
            }
            catch
            {
            }


            PLCSession.WriteAuditLog("5", "1", "", "Reason text: " + PLCSession.GetDefault("REASONTEXT")); //save to auditlog      

            Response.Redirect("~/Tab1Case.aspx");
          
        }

        private bool LIMSCaseAlreadyExists(string departmentCaseNumber, string departmentCode)
        {
            PLCQuery qryLIMS = new PLCQuery();
            qryLIMS.SQL = "SELECT * FROM TV_LABCASE WHERE DEPARTMENT_CASE_NUMBER = ? AND DEPARTMENT_CODE = ?";
            qryLIMS.AddSQLParameter("DEPARTMENT_CASE_NUMBER", departmentCaseNumber);
            qryLIMS.AddSQLParameter("DEPARTMENT_CODE", departmentCode);
            qryLIMS.Open();
            return qryLIMS.HasData();

        }

        private void ServerAlert(string message)
        {
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "SERVER_ALERT", "setTimeout(function () { Alert('" + message + "'); });", true);
        }

        private void ServerAlert(string title, string message)
        {
            string script = "setTimeout(function () { Alert('" + message.Replace("'", "\\'") + "', {"
                + "title:'" + title + "'"
                + "}); });";
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "SERVER_ALERT", script, true);
        }


        private void UpdateCasedistOfficer()
        {
            if (!PLCDBPanel1.IsNewRecord && PLCDBPanel1.HasPanelRec("TV_LABCASE", "CASE_OFFICER_KEY"))
            {
                string caseOfficer = PLCDBPanel1.getpanelfield("CASE_OFFICER_KEY");
                string departmentCode = PLCDBPanel1.getpanelfield("DEPARTMENT_CODE");
                string oldCaseOfficer = PLCDBPanel1.GetOriginalValue("CASE_OFFICER_KEY");
                string officerName = PLCDBGlobal.instance.GetOfficerRecord(caseOfficer, "NAME");

                if (caseOfficer != oldCaseOfficer)
                {
                    string deptpersClause = string.IsNullOrEmpty(oldCaseOfficer) ? " AND (DEPTPERS_KEY = 0 OR DEPTPERS_KEY IS NULL) " : string.Format(" AND DEPTPERS_KEY='{0}'", oldCaseOfficer);
                    PLCQuery qryCasedist = new PLCQuery();
                    qryCasedist.SQL = string.Format("SELECT * FROM TV_CASEDIST WHERE CASE_KEY = '{0}' AND DEPARTMENT_CODE = '{1}' {2} ORDER BY DIST_KEY DESC", PLCSession.PLCGlobalCaseKey, departmentCode, deptpersClause);
                    qryCasedist.Open();
                    if (qryCasedist.HasData())
                    {
                        DeleteOldCasedistAttentionDuplicate(PLCSession.PLCGlobalCaseKey, departmentCode, officerName);

                        string latestOfficer = qryCasedist.FieldByName("DIST_KEY");

                        qryCasedist.Edit();
                        qryCasedist.SetFieldValue("DEPTPERS_KEY", caseOfficer);
                        qryCasedist.SetFieldValue("ATTENTION", officerName);
                        qryCasedist.SetFieldValue("ATTENTION_ID", PLCDBGlobal.instance.GetOfficerRecord(caseOfficer, "ID"));
                        qryCasedist.Post("TV_CASEDIST", 96, 2);

                        while (!qryCasedist.EOF())
                        {
                            string distKey = qryCasedist.FieldByName("DIST_KEY");

                            if (distKey != latestOfficer)
                            {
                                PLCQuery qryDelete = new PLCQuery("DELETE FROM TV_CASEDIST WHERE DIST_KEY = " + distKey);
                                qryDelete.ExecSQL(PLCSessionVars1.GetConnectionString());

                            }

                            qryCasedist.Next();

                        }
                    }
                }
            }

        }


        private void DeleteOldCasedistAttentionDuplicate(string caseKey, string departmentCode, string newOfficerName)
        {
            PLCQuery qryOfficer = new PLCQuery();
            string officerNameClause = string.IsNullOrEmpty(newOfficerName) ? " AND (ATTENTION = '' OR ATTENTION IS NULL)" : string.Format(" AND UPPER(ATTENTION) = '{0}'", newOfficerName);
            qryOfficer.SQL = string.Format("SELECT * FROM TV_CASEDIST WHERE DEPARTMENT_CODE = '{0}' AND CASE_KEY = '{1}' {2}", departmentCode, caseKey, officerNameClause);
            qryOfficer.Open();
            while(!qryOfficer.EOF())
            {
                string distKey = qryOfficer.FieldByName("DIST_KEY");
                DeleteCaseDistRecord(distKey);

                qryOfficer.Next();
            }

        }


        private void DeleteCaseDistRecord(string distKey)
        {
            PLCQuery qryDelete = new PLCQuery("DELETE FROM TV_CASEDIST WHERE DIST_KEY = " + distKey);
            qryDelete.ExecSQL();
        }

        #endregion

        #region Re-upload QC Images
        private bool HasPendingQCImages()
        {
            PLCQuery qryQCImages = new PLCQuery();
            qryQCImages.SQL = "SELECT * FROM TV_QCIMAGES WHERE CASE_KEY = ?";
            qryQCImages.AddSQLParameter("CASE_KEY", PLCSession.PLCGlobalCaseKey);
            qryQCImages.Open();

            return qryQCImages.HasData();

        }

        protected void btnReUpload_Click(object sender, EventArgs e)
        {
            bool usesImageVaultService = PLCSession.GetLabCtrlFlag("USES_IMAGE_VAULT_SERVICE").Equals("T");
            List<string> lstReUploadedFiles = new List<string>();
            List<string> lstFailedFiles = new List<string>();

            if (!usesImageVaultService) return;

            string testService = "";

            try
            {
                testService = new PLCHelperFunctions().HasWebServiceConfigured();
            }
            catch (Exception ee)
            {
                PLCSession.WriteDebug("Exception in Reupload HasWebServiceConfigured(): " + ee.Message, true);
            }

            if (!string.IsNullOrEmpty(testService))
            {
                dlgMessage.ShowAlert("Re-upload QC Images", testService);
                return;
            }

            try
            {

                string qry = "SELECT * FROM TV_QCIMAGES WHERE CASE_KEY = ?";
                PLCQuery qryQCImages = new PLCQuery(qry);
                qryQCImages.AddParameter("CASE_KEY", PLCSession.PLCGlobalCaseKey);
                qryQCImages.Open();
                while (!qryQCImages.EOF())
                {
                    string qcImagesKey = qryQCImages.FieldByName("IMAGE_KEY");

                    // entry id
                    string entryID = PLCSession.GetNextSequence("LIMSDOC_SEQ").ToString();
                    entryID = PLCSession.PLCDBUserID + (usesImageVaultService ? "" : "W") + entryID;

                    // retrieve required keys and data
                    string printLogKey = PLCSession.GetNextSequence("PRINTLOG_SEQ").ToString();
                    string imageKey = PLCSession.GetNextSequence("IMAGES_SEQ").ToString();
                    string auditLogKey = PLCSession.GetNextSequence("AUDITLOG_SEQ").ToString();
                    string caseKey = PLCSession.PLCGlobalCaseKey;
                    string dateTimeNow = DateTime.Now.ToString();
                    string analyst = PLCSession.PLCGlobalAnalyst;
                    string fileSource = string.Empty;
                    string fileSourceKey = string.Empty;


                    //assign TV_QCIMAGES values to variables
                    string contentType = qryQCImages.FieldByName("CONTENT_TYPE");
                    string dataType = qryQCImages.FieldByName("DATA_TYPE");
                    string description = qryQCImages.FieldByName("DESCRIPTION");
                    string destination = qryQCImages.FieldByName("DESTINATION");
                    string docType = qryQCImages.FieldByName("DOCUMENT_TYPE");
                    string fileName = qryQCImages.FieldByName("FILE_NAME");
                    string fleSize = qryQCImages.FieldByName("FILE_SIZE");
                    string itemNumber = qryQCImages.FieldByName("ITEM_NUMBER");
                    string path = qryQCImages.FieldByName("PATH");
                    string source = qryQCImages.FieldByName("SOURCE");
                    string attachDate = qryQCImages.FieldByName("ATTACH_DATE");
                    string attachID = qryQCImages.FieldByName("ATTACH_ID");
                    string attachName = qryQCImages.FieldByName("ATTACH_NAME");
                    string attachSource = qryQCImages.FieldByName("ATTACH_SOURCE");
                    byte[] bytData = (byte[])qryQCImages.PLCDataTable.Rows[0]["DATA"];
                    string ECN = string.Empty;

                    if (!string.IsNullOrEmpty(itemNumber))
                    {
                        ECN = GetECNBasedOnItemNumber(itemNumber, caseKey);
                    }

                    // retrieve the file source and key
                    GetFileSourceAndKey(destination, out fileSource, out fileSourceKey, ECN);
                    string printlogDisplayOrder = GetPrintLogDisplayOrder(caseKey, fileSource, fileSourceKey);
                    string auditInfo = GenerateAuditText(printLogKey, imageKey, caseKey, docType, fileName,
                                        dataType, fileSource, fileSourceKey);


                    //save to Images
                    bool isReUploaded = SaveToImages(imageKey, entryID, analyst, dateTimeNow, fleSize,
                        contentType, dataType, bytData, ECN, usesImageVaultService);

                    if (!isReUploaded)
                    {
                        PLCSession.WriteDebug("QC Images Re-upload: QC images key not uploaded: " + qcImagesKey);
                        PLCSession.WriteDebug("QC Images Re-upload: QC images key not uploaded file name: " + fileName);

                        if (!lstFailedFiles.Contains(fileName))
                            lstFailedFiles.Add(fileName);

                        qryQCImages.Next();
                        continue;
                    }

                    if (!lstReUploadedFiles.Contains(fileName))
                        lstReUploadedFiles.Add(fileName);

                    // save into printlog
                    SaveToPrintLog(printLogKey, imageKey, caseKey, string.IsNullOrEmpty(docType) ? "ATTACH" : docType,
                        description, fileName, dataType, dateTimeNow, analyst, fileSource, fileSourceKey, printlogDisplayOrder,
                        attachDate, attachID, attachName, attachSource);

                    // save into auditlog
                    SaveToAuditLog(auditLogKey, dateTimeNow, analyst, caseKey, auditInfo, ECN);

                    //delete the current QCIMAGES record
                    PLCQuery qryDelete = new PLCQuery();
                    qryDelete.Delete("TV_QCIMAGES", "WHERE IMAGE_KEY = " + qcImagesKey);

                    qryQCImages.Next();
                }


                string message = "";



                if (lstReUploadedFiles.Count > 0)
                    message = "<b>QCImages re-uploaded file(s):</b><br/> " + string.Join(", ", lstReUploadedFiles) + "<br/>";
                if (lstFailedFiles.Count > 0)
                    message += "<b>QCImages failed file(s):</b><br/> " + string.Join(", ", lstFailedFiles) + "<br/>";

                if (!string.IsNullOrEmpty(message))
                    dlgMessage.ShowAlert("Re-upload QC Images", message);

                btnReUpload.Enabled = (PLCSession.GetLabCtrlFlag("USES_IMAGE_VAULT_SERVICE").Equals("T") && HasPendingQCImages());
            }
            catch (Exception ex)
            {
                dlgMessage.ShowAlert("Re-upload QC Images", "Error on re-uploading QC Images. Please contact administrator");
                PLCSession.WriteDebug("@Case Tab Reupload: " + ex.Message + Environment.NewLine + ex.StackTrace);

            }

        }


        private string GenerateAuditText(string pstrPrintLogKey, string pstrImageKey, string pstrCaseKey, string pstrDocumentType, string pstrFilename, string pstrDataType, string pstrFileSource, string pstrFileSourceKey)
        {
            string strAuditText = string.Empty;

            // generate the audit text
            strAuditText = string.Format("PRINTLOG_KEY: {0}\r\nIMAGE_ID: {1}\r\nCASE_KEY: {2}\r\nDOCUMENT_TYPE: {3}\r\nDOCUMENT_DESCRIPTION: {4}\r\nDATA_FILE_NAME: {4}\r\nDATA_TYPE: {5}\r\n", pstrPrintLogKey, pstrImageKey, pstrCaseKey, pstrDocumentType, pstrFilename, pstrDataType);

            if (pstrFileSource != string.Empty)
            {
                strAuditText = string.Format("{0}FILE_SOURCE: {1}\r\nFILE_SOURCE_KEY1: {2}", strAuditText, pstrFileSource, pstrFileSourceKey);
            }

            return strAuditText;
        }
        private bool GetFileSourceAndKey(string pstrDestination, out string pstrFileSource, out string pstrFileSourceKey, string pstrEvidenceControlNumber)
        {
            if (pstrDestination == "CASE" || pstrDestination == "LABCASE")
            {
                pstrFileSource = "LABCASE";
                pstrFileSourceKey = PLCSession.PLCGlobalCaseKey;
            }
            else if (pstrDestination == "LABSUB")
            {
                pstrFileSource = "LABSUB";
                pstrFileSourceKey = PLCSession.PLCGlobalSubmissionKey;
            }
            else if (pstrDestination == "LABITEM")
            {
                pstrFileSource = "LABITEM";
                pstrFileSourceKey = pstrEvidenceControlNumber;
            }
            else
            {
                pstrFileSource = PLCSession.PLCGlobalAttachmentSource;
                pstrFileSourceKey = PLCSession.PLCGlobalAttachmentSourceKey;
            }

            return true;
        }

        private string GetPrintLogDisplayOrder(string pstrCaseKey, string pstrFileSource, string pstrFileSourceKey)
        {
            PLCQuery objQuery = null;
            string strDisplayOrder = string.Empty;
            string strQuery = string.Empty;

            // set the query
            strQuery = "SELECT MAX(DISPLAY_ORDER) + 1 NEXT_NUM FROM TV_PRINTLOG WHERE CASE_KEY = ? AND FILE_SOURCE = ? AND FILE_SOURCE_KEY1 = ?";
            objQuery = new PLCQuery(strQuery);
            objQuery.AddParameter("CASE_KEY", pstrCaseKey);
            objQuery.AddParameter("FILE_SOURCE", pstrFileSource);
            objQuery.AddParameter("FILE_SOURCE_KEY1", pstrFileSourceKey);

            // do query
            objQuery.Open();
            if (objQuery.HasData())
            {
                strDisplayOrder = objQuery.FieldByName("NEXT_NUM");
            }

            if (strDisplayOrder == string.Empty)
                strDisplayOrder = "1";

            return strDisplayOrder;
        }


        private string GetECNBasedOnItemNumber(string itemNumber, string caseKey)
        {
            PLCQuery qryItem = new PLCQuery();
            qryItem.SQL = "SELECT EVIDENCE_CONTROL_NUMBER FROM TV_LABITEM WHERE CASE_KEY = ? AND DEPARTMENT_ITEM_NUMBER = ?";
            qryItem.AddSQLParameter("CASE_KEY", caseKey);
            qryItem.AddSQLParameter("DEPARTMENT_ITEM_NUMBER", itemNumber);
            qryItem.Open();
            if (qryItem.HasData())
                return qryItem.FieldByName("EVIDENCE_CONTROL_NUMBER");

            return string.Empty;
        }
        private bool SaveToPrintLog(string pstrPrintlogKey, string pstrImageKey, string pstrCaseKey, string pstrDocumentType, string pstrDescription,
                                    string pstrFilename, string pstrDataType, string pstrDatePrinted, string pstrPrintedBy, string pstrFileSource,
                                    string pstrFileSourceKey, string pstrDisplayOrder, string attachDate, string attachID, string attachName, string attachSource)
        {
            PLCQuery objQuery = null;
            string strQuery = string.Empty;

            // set the query 
            strQuery = "SELECT * FROM TV_PRINTLOG WHERE 0 = 1";
            objQuery = new PLCQuery(strQuery);

            // do append
            objQuery.Open();
            objQuery.Append();
            objQuery.AddParameter("PRINTLOG_KEY", pstrPrintlogKey);
            objQuery.AddParameter("IMAGE_ID", pstrImageKey);
            objQuery.AddParameter("CASE_KEY", pstrCaseKey);
            objQuery.AddParameter("DOCUMENT_TYPE", pstrDocumentType);
            objQuery.AddParameter("DOCUMENT_DESCRIPTION", pstrDescription);
            objQuery.AddParameter("DATA_FILE_NAME", pstrFilename);
            objQuery.AddParameter("DATA_TYPE", pstrDataType);
            objQuery.AddParameter("DATE_PRINTED", pstrDatePrinted);
            objQuery.AddParameter("PRINTED_BY", pstrPrintedBy);
            objQuery.AddParameter("DISPLAY_ORDER", pstrDisplayOrder);

            if (pstrFileSource != string.Empty)
            {
                objQuery.AddParameter("FILE_SOURCE", pstrFileSource);
                objQuery.AddParameter("FILE_SOURCE_KEY1", pstrFileSourceKey);
            }


            if (attachSource == "PRELOG")
            {
                objQuery.AddParameter("ATTACH_ID", attachID);
                objQuery.AddParameter("ATTACH_NAME", attachName);
                if (!string.IsNullOrEmpty(attachDate))
                    objQuery.AddParameter("ATTACH_DATE", Convert.ToDateTime(attachDate));
                objQuery.AddParameter("ATTACH_SOURCE", attachSource);
            }

            objQuery.Save("TV_PRINTLOG");

            return true;
        }


        private bool SaveToAuditLog(string pstrAuditKey, string pstrTimeStamp, string pstrUserId,
                                    string pstrCaseKey, string pstrAdditionalInfo, string pstrEvidenceControlNumber)
        {

            string program = ("iLIMS" + PLCSession.PLCBEASTiLIMSVersion).Substring(0, 8);
            string OSComputerName = PLCSession.GetOSComputerName();
            string osUsername = PLCSession.GetOSUserName();
            string osAddress = PLCSession.GetOSAddress();


            PLCQuery objQuery = null;
            string strQuery = string.Empty;

            // set the query
            strQuery = "SELECT * FROM TV_AUDITLOG WHERE 0 = 1";
            objQuery = new PLCQuery(strQuery);

            // do append
            objQuery.Open();
            objQuery.Append();
            objQuery.SetFieldValue("LOG_STAMP", pstrAuditKey);
            objQuery.SetFieldValue("TIME_STAMP", pstrTimeStamp);
            objQuery.SetFieldValue("USER_ID", pstrUserId);
            objQuery.SetFieldValue("PROGRAM", program);
            objQuery.SetFieldValue("CASE_KEY", pstrCaseKey);
            objQuery.SetFieldValue("CODE", "31");
            objQuery.SetFieldValue("SUB_CODE", "0");
            objQuery.SetFieldValue("ERROR_LEVEL", "0");
            objQuery.SetFieldValue("ADDITIONAL_INFORMATION", pstrAdditionalInfo);
            objQuery.SetFieldValue("OS_COMPUTER_NAME", OSComputerName);
            objQuery.SetFieldValue("OS_USER_NAME", osUsername);
            objQuery.SetFieldValue("OS_ADDRESS", osAddress);
            objQuery.SetFieldValue("EVIDENCE_CONTROL_NUMBER", pstrEvidenceControlNumber);

            objQuery.Post("TV_AUDITLOG", -1, -1);

            return true;

        }

        private bool SaveToImages(string pstrImageKey, string pstrEntryId, string pstrEntryOfficer, string pstrTimeStamp,
            string pstrFileSize, string pstrContentType, string pstrFormat, byte[] pbytData,
            string pstrEvidenceControlNumber, bool usesImageVaultService)
        {
            PLCQuery objQuery = null;
            string strQuery = string.Empty;

            // set the query
            strQuery = "SELECT * FROM TV_IMAGES WHERE 0 = 1";
            objQuery = new PLCQuery(strQuery);

            // do append
            objQuery.Open();
            objQuery.Append();
            objQuery.AddParameter("IMAGE_ID", pstrImageKey);
            objQuery.AddParameter("ENTRY_ID", pstrEntryId);
            objQuery.AddParameter("ENTRY_OFFICER", pstrEntryOfficer);
            objQuery.AddParameter("ENTRY_TIME_STAMP", pstrTimeStamp);
            objQuery.AddParameter("FILE_SIZE", pstrFileSize);
            objQuery.AddParameter("CONTENT_TYPE", pstrContentType);
            objQuery.AddParameter("FORMAT", pstrFormat);
            objQuery.AddParameter("EVIDENCE_CONTROL_NUMBER", pstrEvidenceControlNumber);

            if (usesImageVaultService)
            {
                PLCHelperFunctions plcHelper = new PLCHelperFunctions();
                bool isUploaded = plcHelper.UploadAttachmentToLocalServer(pbytData, pstrEntryId + "." + pstrFormat, skipError: true);

                if (!isUploaded)
                    return false;

                string signature = plcHelper.CreateImageDigitalSignature(pstrEntryId + "." + pstrFormat);
                objQuery.AddParameter("DIGITAL_SIGNATURE", signature);
            }
            else
            {
                objQuery.AddParameter("DATA", pbytData);
            }


            objQuery.Save("TV_IMAGES");

            return true;
        }

        #endregion

        #region CCAP
        #region Events
        protected void btnCCAP_Click(object sender, EventArgs e)
        {
            if (!dbgCCAP.HasHDConfig())
            {
                ServerAlert(btnCCAP.Text, "Missing DBGrid config for [" + dbgCCAP.PLCGridName + "].");
                return;
            }

            int caseKey = PLCSession.SafeInt(PLCSession.PLCGlobalCaseKey);
            string ccapNumber;
            if (CCAPInterface.HasCCAPNumber(caseKey, out ccapNumber))
            {
                string message;
                if (CCAPInterface.SyncCCAPData(caseKey, out message))
                {
                    if (!string.IsNullOrEmpty(message))
                    {
                        message += "<br/>Do you want to update CCAP data?";
                        dlgMsg.DialogKey = DialogKeys.CCAPForceUpdate.ToString();
                        dlgMsg.ShowYesNoOnly(btnCCAP.Text + " - " + ccapNumber, message);
                    }
                    else
                    {
                        message = "CCAP Data has been updated!";
                        ServerAlert(btnCCAP.Text + " - " + ccapNumber, message);
                    }
                }
                else
                {
                    ServerAlert(btnCCAP.Text + " - " + ccapNumber, message);
                }
                return;
            }

            LoadCCAPSearch();

            // auto search if no CCAPDATA_SEARCH
            if (dbgCCAP.Rows.Count == 0)
                SearchCCAP();

            ShowCCAPSearch();
        }

        protected void btnSearchCCAP_Click(object sender, EventArgs e)
        {
            SearchCCAP();
        }

        protected void btnLinkCCAP_Click(object sender, EventArgs e)
        {
            if (dbgCCAP.Rows.Count == 0)
            {
                ServerAlert(btnCCAP.Text, "No CCAP data.");
                return;
            }

            int caseKey = PLCSession.SafeInt(PLCSession.PLCGlobalCaseKey);
            int county = PLCSession.SafeInt(dbgCCAP.SelectedDataKey["COUNTY"].ToString());
            string ccapNumber = dbgCCAP.SelectedDataKey["CCAP_NUMBER"].ToString();

            string message;
            CCAPInterface.LinkCCAPToCase(county, ccapNumber, caseKey, out message);

            CloseCCAPSearch();

            if (string.IsNullOrEmpty(message))
                message = ccapNumber + " has been linked.";
            ServerAlert(btnCCAP.Text + " - " + ccapNumber, message);
        }
        #endregion Events

        #region Methods
        private void SearchCCAP()
        {
            int caseKey = PLCSession.SafeInt(PLCSession.PLCGlobalCaseKey);
            string message;
            if (!CCAPInterface.SearchCCAPByName(caseKey, out message))
            {
                ServerAlert(btnCCAP.Text, message);
            }

            LoadCCAPSearch();
        }

        private void LoadCCAPSearch()
        {
            dbgCCAP.PLCSQLString_AdditionalCriteria = "WHERE CASE_KEY = " + PLCSession.PLCGlobalCaseKey;
            dbgCCAP.InitializePLCDBGrid();

            if (dbgCCAP.Rows.Count > 0)
            {
                dbgCCAP.PageIndex = 0;
                dbgCCAP.SelectedIndex = 0;
            }
        }

        private void CloseCCAPSearch()
        {
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "_ccapSearch", "closeCCAPSearch();", true);
        }

        private void ShowCCAPSearch()
        {
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "_ccapSearch", "showCCAPSearch();", true);
        }

        private void ForceSyncCCAPData()
        {
            int caseKey = PLCSession.SafeInt(PLCSession.PLCGlobalCaseKey);

            string message;
            if (CCAPInterface.SyncCCAPData(caseKey, out message, isForce: true))
            {
                message = "CCAP Data has been updated!";
            }

            if (!string.IsNullOrEmpty(message))
            {
                ServerAlert(btnCCAP.Text, message);
            }
        }
        #endregion Methods
        #endregion CCAP

        protected void dlgConfirm_Click(object sender, DialogEventArgs e)
        {
            switch (e.DialogKey)
            {
                case "Cancel_case":

                    try
                    {
                        if (PLCSession.PLCDatabaseServer == "MSSQL")
                        {
                            PLCQuery qryDeleteCase = new PLCQuery();
                            qryDeleteCase.AddProcedureParameter("caseKey", Convert.ToInt32(PLCSession.PLCGlobalCaseKey), 10, OleDbType.Integer, ParameterDirection.Input);
                            qryDeleteCase.AddProcedureParameter("deleteReason", txtDeleteReason.Text, txtDeleteReason.Text.Length, OleDbType.VarChar, ParameterDirection.Input);
                            qryDeleteCase.AddProcedureParameter("userId", PLCSession.PLCGlobalAnalyst, 15, OleDbType.VarChar, ParameterDirection.Input);
                            qryDeleteCase.AddProcedureParameter("program", "iLIMS" + PLCSession.PLCBEASTiLIMSVersion, 8, OleDbType.VarChar, ParameterDirection.Input);
                            qryDeleteCase.AddProcedureParameter("OSComputerName", PLCSession.GetOSComputerName(), 50, OleDbType.VarChar, ParameterDirection.Input);
                            qryDeleteCase.AddProcedureParameter("OSUserName", PLCSession.GetOSUserName(), 50, OleDbType.VarChar, ParameterDirection.Input);
                            qryDeleteCase.AddProcedureParameter("OSAddress", PLCSession.GetOSAddress(), 50, OleDbType.VarChar, ParameterDirection.Input);
                            qryDeleteCase.AddProcedureParameter("BuildNumber", PLCSession.PLCBEASTiLIMSVersion, 100, OleDbType.VarChar, ParameterDirection.Input);
                            qryDeleteCase.AddProcedureParameter("labCode", PLCSession.PLCGlobalLabCode, 3, OleDbType.VarChar, ParameterDirection.Input);
                            qryDeleteCase.ExecuteProcedure("DELETE_CASE");
                        }
                        else
                        {
                            PLCQuery qryDeleteCase = new PLCQuery();
                            List<OracleParameter> parameters = new List<OracleParameter>();
                            parameters.Add(new OracleParameter("caseKey", Convert.ToInt32(PLCSession.PLCGlobalCaseKey)));
                            parameters.Add(new OracleParameter("deleteReason", txtDeleteReason.Text));
                            parameters.Add(new OracleParameter("userId", PLCSession.PLCGlobalAnalyst));
                            parameters.Add(new OracleParameter("program", ("iLIMS" + PLCSession.PLCBEASTiLIMSVersion).Substring(0, 8)));
                            parameters.Add(new OracleParameter("OSComputerName", PLCSession.GetOSComputerName()));
                            parameters.Add(new OracleParameter("OSUserName", PLCSession.GetOSUserName()));
                            parameters.Add(new OracleParameter("OSAddress", PLCSession.GetOSAddress()));
                            parameters.Add(new OracleParameter("BuildNumber", PLCSession.PLCBEASTiLIMSVersion));
                            parameters.Add(new OracleParameter("labCode", PLCSession.PLCGlobalLabCode));
                            qryDeleteCase.ExecuteOracleProcedure("DELETE_CASE", parameters);
                        }

                    }
                    catch (Exception ex)
                    {

                    }

                    PLCSession.SetDefault("IS_PARTIAL_CASE", "");
                    PLCSession.ClearLabCaseVars();
                    Response.Redirect("~/Dashboard.aspx");
                    break;


            }
        }


        protected void dlgConfirmCancel_Click(object sender, DialogEventArgs e)
        {
            //do nothing for now
        }

        #region Expunge/ Admin Remove

        #region Events

        // this onclick event is used both by btnExpunge and btnAdminRemoval
        protected void btnExpunge_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            // if true, btnExpunge was clicked, btnAdminRemoval otherwise
            bool isExpunge = btn.ID == btnExpunge.ID;
            btnOKExpunge.Visible = isExpunge;
            btnOKAdminRemoval.Visible = !isExpunge;

            pnlExpungePassword.Visible = !PLCSession.CheckUserOption("EXPUNGE");
            lblExpungeInfo.Text = string.Format(
                PLCSession.GetSysPrompt("TAB1Case.lblExpungeInfo.CONFIRM", "Are you sure? If so please enter the reason for {0}."),
                btn.Text);

            lblExpungWarning.Visible = false;
            txtUserID.Text = "";
            txtPassword.Text = "";
            fbExpungeCode.SelectedValue = "";
            txtExpungeComments.Text = "";

            ShowExpungePopup(btn.Text);
        }

        protected void btnOkExpunge_Click(object sender, EventArgs e)
        {
            ExpungeCase(ExpungeType.Expunge);
        }

        protected void btnOkAdminRemoval_Click(object sender, EventArgs e)
        {
            ExpungeCase(ExpungeType.AdminRemoval);
        }

        protected void fbExpungeCode_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtExpungeComments.Text = (string.IsNullOrEmpty(txtExpungeComments.Text) ? "" : txtExpungeComments.Text + "\n") + fbExpungeCode.GetDescription();
        }

        #endregion

        #region Methods

        private void ExpungeCase(ExpungeType expungType)
        {
            string msg = string.Empty;
            string label = expungType.Equals(ExpungeType.Expunge)
                ? PLCSession.GetSysPrompt("TAB1Case.ExpungeType.EXPUNGE", "Expunge")
                : PLCSession.GetSysPrompt("TAB1Case.ExpungeType.ADMIN_REMOVAL", "Admin Removal");
            bool isSupHasDeleteAccess = true;

            if (pnlExpungePassword.Visible)
            {
                ValidateUserPassword(out msg);
                isSupHasDeleteAccess = PLCSession.CheckUserOption(txtUserID.Text.ToUpper(), "DELITEM");
            }

            if (pnlExpungePassword.Visible && string.IsNullOrEmpty(txtUserID.Text) && string.IsNullOrEmpty(txtPassword.Text))
            {
                lblExpungWarning.Text = PLCSession.GetSysPrompt("TAB1Case.lblExpungWarning", "User Name and Password are required.");
                lblExpungWarning.Visible = true;

                ShowExpungePopup(label);
                return;
            }
            else if (pnlExpungePassword.Visible && !PLCSession.CheckUserOption(txtUserID.Text, "EXPUNGE"))
            {
                msg = string.Format("This user has no {0} permission.", label);
                ShowExpungePopup(label);
                ServerAlert(label, msg);
                return;
            }
            else if (!PLCSession.CheckUserOption("EXPUNGE") && !isSupHasDeleteAccess)
            {
                ServerAlert("Expunge", "You have no authority to delete items. Contact your supervisor for help.");
                return;
            }
            else if (!string.IsNullOrEmpty(msg))
            {
                lblExpungWarning.Text = msg;
                lblExpungWarning.Visible = true;
                ShowExpungePopup(label);
                return;
            }
            else if (string.IsNullOrEmpty(txtExpungeComments.Text) && string.IsNullOrEmpty(fbExpungeCode.GetValue()))
            {
                lblExpungeInfo.Text = string.Format(
                    PLCSession.GetSysPrompt("TAB1Case.lblExpungeInfo.ENTER_REASON", "Please select/enter reason for {0}."),
                    label);

                ShowExpungePopup(label);
                return;
            }

            ProcessExpunge(expungType);

            if (PLCSession.GetLabCtrlFlag("ADD_SCHEDULE_ENTRY_ON_EXPUNGE").Equals("T"))
            {
                AddExpungeToSchedule(expungType, txtUserID.Text);
                if (bnSchedule.Visible)
                    bnSchedule.Style["color"] = IsCaseHasCorrespondence(PLCSession.PLCGlobalCaseKey) ? "Red" : "Black";
            }
        }

        private void ProcessExpunge(ExpungeType expungType)
        {
            string expungeName = expungType.Equals(ExpungeType.Expunge) ? "Expunged" : "Admin Removed";
            string expungeMsg = "Sample has been successfully " + (expungType.Equals(ExpungeType.Expunge) ? "expunged" : "Admin Removed") + ".";

            string resourceSuccessMsg = string.Empty;
            if (expungType.Equals(ExpungeType.Expunge))
                resourceSuccessMsg = PLCSession.GetLocalizedTextData("TAB1Case.EXPUNGED.SuccessMessage");
            else
                resourceSuccessMsg = PLCSession.GetLocalizedTextData("TAB1Case.ADMIN_REMOVED.SuccessMessage");

            if (!string.IsNullOrEmpty(resourceSuccessMsg))
                expungeMsg = resourceSuccessMsg;

            PLCQuery qry = new PLCQuery();

            // update lab name
            qry.SQL = "SELECT * FROM TV_LABNAME WHERE CASE_KEY = " + PLCSession.PLCGlobalCaseKey;
            qry.Open();

            if (qry.HasData())
            {
                qry.Edit();

                qry.SetFieldValue("LAST_NAME", expungeName);
                qry.SetFieldValue("FIRST_NAME", expungeName);
                qry.SetFieldValue("MIDDLE_NAME", expungeName);
                qry.SetFieldValue("OFFENDER_ID", "");
                qry.SetFieldValue("SUFFIX", "");
                qry.SetFieldValue("DOC_NUMBER", "");

                qry.SetFieldValue("FBI_NUMBER", "");
                qry.SetFieldValue("NAME_ID", "");
                qry.SetFieldValue("STATE_ID", "");
                qry.SetFieldValue("SEX", "");
                qry.SetFieldValue("RACE", "");
                qry.SetFieldValue("DATE_OF_BIRTH", "");
                qry.Post("TV_LABNAME", 6, 1);
            }

            // update labsub
            qry.SQL = "SELECT * FROM TV_LABSUB WHERE CASE_KEY = " + PLCSession.PLCGlobalCaseKey;
            qry.Open();

            if (qry.HasData())
            {
                qry.Edit();
                qry.SetFieldValue("COMMENTS", "");

                if (expungType == ExpungeType.Expunge)
                    qry.SetFieldValue("TRACKING_NUMBER", "");

                qry.Post("TV_LABSUB", 8, 1);
            }

            // update lab item
            qry.SQL = "SELECT * FROM TV_LABITEM WHERE CASE_KEY = " + PLCSession.PLCGlobalCaseKey;
            qry.Open();

            if (qry.HasData())
            {
                qry.Edit();
                qry.SetFieldValue("ERROR_STATUS", "");
                qry.SetFieldValue("SAMPLE_STATUS", "");
                qry.SetFieldValue("IN_CODIS", "");
                qry.SetFieldValue("CODIS_DATE", "");
                qry.SetFieldValue("CODIS_SPECIMEN_TYPE", "");
                qry.Post("TV_LABITEM", 9, 1);
            }

            // update lab case
            qry.SQL = "SELECT * FROM TV_LABCASE WHERE CASE_KEY = " + PLCSession.PLCGlobalCaseKey;
            qry.Open();

            if (qry.HasData())
            {
                qry.Edit();
                qry.SetFieldValue("CASE_TYPE", "");
                qry.SetFieldValue("EXPUNGED", "T");
                qry.Post("TV_LABCASE", 5, 1);
            }

            // close assignemnt and task lists
            string examKey = string.Empty;
            qry.SQL = "SELECT * FROM TV_LABASSIGN WHERE CASE_KEY = " + PLCSession.PLCGlobalCaseKey;
            qry.Open();
            if (qry.HasData())
            {
                while (!qry.EOF())
                {
                    examKey = qry.FieldByName("EXAM_KEY");
                    CloseAssigenmentAndTask(examKey);
                    qry.Next();
                }
            }

            // tv_dbexpng
            qry.SQL = "SELECT * FROM TV_DBEXPNG WHERE 0=1";
            qry.Open();
            qry.Append();
            qry.SetFieldValue("CASE_KEY", PLCSession.PLCGlobalCaseKey);
            qry.SetFieldValue("EXPUNGE_DATE", DateTime.Now);
            qry.SetFieldValue("EXPUNGED_BY", PLCSession.PLCGlobalAnalyst);
            qry.SetFieldValue("REASON_CODE", fbExpungeCode.GetValue());
            qry.SetFieldValue("COMMENTS", txtExpungeComments.Text);
            qry.Post("TV_DBEXPNG", -1, -1);

            // tv_printlog
            qry.SQL = "SELECT * FROM TV_PRINTLOG WHERE CASE_KEY = " + PLCSession.PLCGlobalCaseKey;
            qry.Open();

            if (qry.HasData())
            {
                // no deletion of TV_IMAGES record as there will be chance that these are linked to multiple records and tables. Ticket 105303
                /*while (!qry.EOF())
                {
                    string imageID = qry.FieldByName("IMAGE_ID");
                    if (!string.IsNullOrEmpty(imageID))
                    {
                        PLCQuery qryDeleteImages = new PLCQuery();
                        qryDeleteImages.Delete("TV_IMAGES", "WHERE IMAGE_ID = " + imageID, 310, 3);
                    }
                    qry.Next();
                }*/

                PLCQuery qryDeletePrintlog = new PLCQuery();
                qryDeletePrintlog.Delete("TV_PRINTLOG", "WHERE CASE_KEY = " + PLCSession.PLCGlobalCaseKey, 7, 1);
            }

            // TV_CONVDATA
            if (CheckIfTableExist("TV_CONVDATA"))
            {
                qry = new PLCQuery();
                qry.SQL = "SELECT * FROM TV_CONVDATA WHERE CASE_KEY = " + PLCSession.SafeInt(PLCSession.PLCGlobalCaseKey);
                qry.Open();
                while (!qry.EOF())
                {
                    string addClause = string.Empty;
                    if (qry.FieldExist("ID"))
                    {
                        if (!string.IsNullOrEmpty(qry.FieldByName("ID")))
                            addClause = " AND ID = '" + qry.FieldByName("ID") + "'";
                    }
                    PLCQuery qryConvData = new PLCQuery();
                    qryConvData.SQL = "SELECT * FROM TV_CONVDATA WHERE CASE_KEY = " + PLCSession.SafeInt(PLCSession.PLCGlobalCaseKey) + addClause;
                    qryConvData.Open();
                    if (!qryConvData.IsEmpty())
                    {
                        qryConvData.Edit();
                        qryConvData.SetFieldValue("NYSID", "");
                        qryConvData.SetFieldValue("OFFENDER_STATUS", "");
                        qryConvData.SetFieldValue("LAST_NAME", "");
                        qryConvData.SetFieldValue("FIRST_NAME", "");
                        qryConvData.SetFieldValue("DOB", "");
                        qryConvData.SetFieldValue("SEX", "");
                        qryConvData.SetFieldValue("RACE", "");
                        qryConvData.Post("TV_CONVDATA", 9075, 2);
                    }
                    qry.Next();
                }
            }

            ServerAlert(expungeName, expungeMsg);

            PLCDBPanel1.PLCWhereClause = "where case_key = " + PLCSession.PLCGlobalCaseKey;
            PLCButtonPanel1.SetBrowseMode();
            PLCDBPanel1.DoLoadRecord();

            btnExpunge.Enabled = btnAdminRemoval.Enabled = false;
        }

        private void ValidateUserPassword(out string msg)
        {
            msg = string.Empty;
            string userID = txtUserID.Text.ToUpper();
            string password = txtPassword.Text;

            if (!PLCSession.ValidatePassword(userID, password, out msg))
            {
                msg = PLCSession.GetSysPrompt("TAB1Case.ValidateUserPassword.INV_PW", "Invalid Password.");
                return;
            }

            PLCQuery qryAnalyst = new PLCQuery();
            qryAnalyst.SQL = string.Format("SELECT * FROM TV_ANALYST WHERE ANALYST = '{0}'", userID);
            qryAnalyst.Open();

            if (qryAnalyst.IsEmpty())
            {
                msg = PLCSession.GetSysPrompt("TAB1Case.ValidateUserPassword.NOT_FOUND", "User not found.");
                return;
            }

            if (qryAnalyst.FieldByName("ACCOUNT_DISABLED").Equals("T"))
            {
                msg = PLCSession.GetSysPrompt("TAB1Case.ValidateUserPassword.DISABLED", "Account is disabled.");
                return;
            }
            else if (qryAnalyst.FieldByName("ACTIVE").Equals("F"))
            {
                msg = PLCSession.GetSysPrompt("TAB1Case.ValidateUserPassword.INACTIVE", "Account is not active.");
                return;
            }
        }

        private bool CheckIfTableExist(string tableName)
        {
            try
            {
                if ((tableName.Substring(0, 3) != "TV_") && (tableName.Substring(0, 3) != "UV_") && (tableName.Substring(0, 3) != "CV_"))
                    tableName = "TV_" + tableName;
                if (tableName.Equals(""))
                    return false;

                string sql = string.Empty;
                if (PLCSession.PLCDatabaseServer.Equals("ORACLE"))
                    sql = "SELECT * FROM " + tableName + " WHERE ROWNUM <= 1";
                else
                    sql = "SELECT TOP 1 * FROM " + tableName;

                PLCQuery qryCheckTable = new PLCQuery(sql);
                return qryCheckTable.ExecSQL(string.Empty, true);
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private void CloseAssigenmentAndTask(string examKey)
        {
            PLCQuery qryLabExam = new PLCQuery();
            qryLabExam.SQL = "SELECT * FROM TV_LABASSIGN WHERE EXAM_KEY = " + examKey;
            qryLabExam.Open();

            if (qryLabExam.IsEmpty())
                return;

            if (qryLabExam.FieldByName("COMPLETED").Trim().Equals("T"))
                return;

            qryLabExam.Edit();
            qryLabExam.SetFieldValue("APPROVED", "A");
            qryLabExam.SetFieldValue("APPROVED_BY", PLCSession.PLCGlobalAnalyst);
            qryLabExam.SetFieldValue("STATUS", PLCSessionVars1.Admin_Closed_Status());
            qryLabExam.SetFieldValue("DATE_COMPLETED", DateTime.Today.ToShortDateString());
            qryLabExam.SetFieldValue("DATETIME_COMPLETED", DateTime.Now);
            qryLabExam.SetFieldValue("DISTRIBUTE", "");
            qryLabExam.SetFieldValue("DISTRIBUTION_TIME_STAMP", "");

            string sDateAssigned = qryLabExam.FieldByName("DATE_ASSIGNED");
            if (!string.IsNullOrEmpty(sDateAssigned))
                qryLabExam.SetFieldValue("TURN_AROUND", ComputeTurnAround(DateTime.Now, DateTime.Parse(sDateAssigned)));

            string comments = (string.IsNullOrEmpty(qryLabExam.FieldByName("COMMENTS")) ? "" : qryLabExam.FieldByName("COMMENTS") + "/n") + "Admin closed by DCJS Upload";
            qryLabExam.SetFieldValue("COMMENTS", comments);

            qryLabExam.SetFieldValue("COMPLETED", "T");
            qryLabExam.Post("TV_LABASSIGN", 82, 1);

            PLCSession.WriteAuditLog(PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, examKey, "82", "1", "0", PLCSession.PLCGlobalAnalyst, ("iLIMS" + PLCSession.PLCBEASTiLIMSVersion).Substring(0, 8), "CASE_KEY: " + qryLabExam.FieldByName("CASE_KEY") + "\r\n" +
                         "EXAM_KEY: " + qryLabExam.FieldByName("EXAM_KEY") + "\r\n" +
                         "SEQUENCE: " + qryLabExam.FieldByName("SEQUENCE") + "\r\n" +
                         "SECTION: " + qryLabExam.FieldByName("SECTION") + "\r\n" +
                         "ANALYST_ASSIGNED: " + qryLabExam.FieldByName("ANALYST_ASSIGNED") + "\r\n" +
                         "APPROVED: " + qryLabExam.FieldByName("APPROVED") + "\r\n" +
                         "APPROVED_BY: " + qryLabExam.FieldByName("APPROVED_BY") + "\r\n" +
                         "STATUS: " + qryLabExam.FieldByName("STATUS") + "\r\n" +
                         "DATE_COMPLETED: " + qryLabExam.FieldByName("DATE_COMPLETED") + "\r\n" +
                         "COMPLETED: " + qryLabExam.FieldByName("COMPLETED") + "\r\n" +
                         "CLOSE_CODE: " + qryLabExam.FieldByName("CLOSE_CODE") + "\r\n" +
                         "CLOSE_COMMENTS: " + qryLabExam.FieldByName("CLOSE_COMMENTS") + "\r\n", 0);

            PLCDBGlobal.instance.UpdateCaseStatus();

            //tasklist
            PLCQuery qryTaskList = new PLCQuery();
            qryTaskList.SQL = "SELECT * FROM TV_TASKLIST WHERE EXAM_KEY =" + examKey;
            qryTaskList.Open();

            if (qryTaskList.IsEmpty())
                return;

            qryTaskList.First();

            while (!qryTaskList.EOF())
            {
                qryTaskList.Edit();
                qryTaskList.SetFieldValue("STATUS", "C");
                qryTaskList.Post("TV_TASKLIST", 52, 1);
                qryTaskList.Next();
            }

        }

        private int ComputeTurnAround(DateTime dateCompleted, DateTime dateAssigned)
        {
            if (dateCompleted > dateAssigned)
                return 0;

            return (int)(dateCompleted - dateAssigned).TotalDays + 1;
        }

        private void AddExpungeToSchedule(ExpungeType expungType, string supervisor)
        {
            string process = expungType.Equals(ExpungeType.Expunge) ? "Expunge" : "Admin Removal";
            string schedType = expungType.Equals(ExpungeType.Expunge) ? "EXP" : "ADMX";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Process: " + process);
            sb.AppendLine("Supervisor ID: " + supervisor);
            sb.AppendLine("Reason for " + process + ":");
            sb.AppendLine("Code: " + fbExpungeCode.GetValue() + " (" + fbExpungeCode.SelectedText + ")");
            sb.AppendLine("Additional Information:");
            sb.AppendLine(txtExpungeComments.Text);
            AddSchedule(schedType, sb.ToString());
        }

        private void SetExpungeControls()
        {
            btnExpunge.Visible = PLCSession.GetLabCtrlFlag("USES_CASE_EXPUNGE").Equals("T");
            btnAdminRemoval.Visible = PLCSession.GetLabCtrlFlag("USES_ADMIN_REMOVAL").Equals("T");

            if (!btnExpunge.Visible && !btnAdminRemoval.Visible)
                return;

            // Expunge and Admin Removal buttons
            btnExpunge.Enabled = btnAdminRemoval.Enabled = !PLCDBGlobal.instance.IsCaseExpunged(PLCSession.SafeInt(PLCSession.PLCGlobalCaseKey));

            string expungLabel = PLCSession.GetLabCtrlFlag("CO_EXPUNGE_LABEL");
            if (!string.IsNullOrEmpty(expungLabel))
                btnExpunge.Text = expungLabel;
        }

        private void ShowExpungePopup(string title)
        {
            ScriptManager.RegisterStartupScript(this, this.GetType(), "showExpungePopup", string.Format("showExpungePopup('{0}');", title), true);
        }

        #endregion

        #endregion
    }
}
