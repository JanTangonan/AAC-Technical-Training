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

            if (!PLCDBPanel1.IsBrowseMode) return;
            if (PLCDBPanel1.IsSavingMode) return;
            if (IsPostBack) return;

            lblCaseReference.Text = GetCaseReferenceStr(PLCSessionVars1.PLCGlobalCaseKey);
            if (PLCSessionVars1.GetLabCtrl("SHOW_CASE_REFERENCE_LABEL") == "T")
            { 
                lblCaseReference.Visible = true;
            }
            else
            {
                lblCaseReference.Visible = false;
            }

            if (ReferenceExists)
            {
                bnReference.Style.Add("color", "Red");
            }
            else
            {
                bnReference.Style.Add("color", "Black");
            }
        }


        private Boolean allNumeric(String checkStr) 
        {

            foreach (Char ch in checkStr.ToCharArray() )
            {
                if (!"0123456789".Contains(ch)) return false;
            }

            return true;
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

            PLCCommonClass.SetSelectedMenuItem(MainMenuTab.CaseTab, (Menu)Master.FindControl("menu_main"));

            // Set page title.
            ((MasterPage)Master).SetCaseTitle(""); //AACI 07/27/2010

            if (!IsPostBack)
            {
                PLCDBGlobal.instance.RemoveRecordLocks("TV_LABCASE", PLCSession.PLCGlobalCaseKey, null, null, PLCSession.PLCGlobalAnalyst);

                if (PLCDBGlobalClass.IsCaseInDataBankMode(PLCSessionVars1.PLCGlobalCaseKey))
                {
                    Response.Redirect("~/Tab1CaseCODNA.aspx");
                    return;
                }
                
                Session["SuppMainDataKey"] = null;
                PLCSession.SetDefault("OFFENSE_VIEWNAME", "");
                PLCSession.SetDefault("OFFENSE_VIEWCOND", "");
                                
                //caselock
                chkCaseLocked.Visible = PLCSessionVars1.CheckUserOption("CASELOCK");
                bool bSuperLocked = false;
                IsCaseLocked = bnTeam.Enabled = PLCDBGlobalClass.isCaseLock(PLCSessionVars1.PLCGlobalCaseKey, ref bSuperLocked);
                if (IsCaseLocked)
                {
                    lCaseLockStatus.Visible = true;
                    if (bSuperLocked)
                        lCaseLockStatus.Text = "Case is superlocked.";
                    else
                        lCaseLockStatus.Text = "Case is locked.";                    
                }
                else
                    lCaseLockStatus.Visible = false;
              
                if ((PLCSessionVars1.GetLabCtrl("USES_STORY").Trim() == "T") && (PLCSessionVars1.GetLabCtrl("STORY_TEXT").Trim() != string.Empty))
                {
                    bnSchedule.Text = PLCSessionVars1.GetLabCtrl("STORY_TEXT").Trim();
                }
                else
                    bnSchedule.Text = "Schedule";
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

                SetAttachmentsButton("CASE");

                // Case Label 
                bnCaseLabel.Visible = !PLCSessionVars1.CheckUserOption("HIDECASLBL");

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
                // RecordUnlock
                if (!PLCSessionVars1.CheckUserOption("DELLOCKS"))
                    PLCButtonPanel1.SetCustomButtonVisible("RecordUnlock", false);

                //Set Distribution button text color
                btnDistribution.Style["color"] = HasDistribution(PLCSessionVars1.PLCGlobalCaseKey) ? "Red" : "Black";

                if (PLCSession.GetLabCtrl("ACTIVITY_LINK_IN_CASE") == "T")
                {
                    PLCQuery queryActivity = new PLCQuery("SELECT * FROM TV_ACTCASES WHERE CASE_KEY = " + PLCSession.PLCGlobalCaseKey);
                    queryActivity.Open();
                    btnActivity.Visible = queryActivity.HasData();
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

                if (Session["ShowSupplementsGrid"] != null && Convert.ToBoolean(Session["ShowSupplementsGrid"]))
                {
                    Session["ShowSupplementsGrid"] = null;
                    BindSupplementsGrid();
                    mpeSupplements.Show();
                }

                ShowCaseTypeLabel();

                //NICS permissions
                btnNICS.Visible = PLCSession.GetLabCtrl("MIDEO_INTERFACE") == "T" && PLCSession.CheckUserOption("NICSIMG");

                if (Request.QueryString["c"] == PLCSession.PLCGlobalDepartmentCaseNumber)
                    PLCButtonPanel1.ClickEditButton();

                SetReadOnlyAccess();
            }
           
            CheckOffenseLookupSettings();
            ToggleResultsButton();
            SetPanelScript();
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
            PLCDBPanel1.SetMyFieldMode("DEPARTMENT_CODE", !CanChangeURN);
			PLCDBPanel1.SetMyFieldMode("DEPARTMENT_CASE_NUMBER", !CanChangeURN);

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
            if (!CanChangeURN)
            {

                setAllowEdit("TV_LABCASE", "DEPARTMENT_CODE", false);
                setAllowEdit("TV_LABCASE", "DEPARTMENT_CASE_NUMBER", false);
                
                
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

                        mbox.ShowMsg("Record Lock", "Case locked by another user for editing.<br/>" + LockedInfo, 1);
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


                NotifyRetentionReviewChanged();

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

                    mbox.ShowMsg("Record Lock", "Case locked by another user for editing.<br/>" + LockedInfo, 1);
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
                bool hasChanges = PLCDBPanel1.HasChanges(false);
				string reasonText = "";

                if (PLCSession.GetLabCtrl("LOG_EDITS_TO_NARRATIVE") == "T")
                {
                    if (hdnConfirmUpdate.Value.Trim().Length > 0)
                    {
                        SaveConfirmUpdate(auditText);
						if (URNNarrative != "")
						{							
							SaveURNChangeNarrative(URNNarrative);
						}
						reasonText = hdnConfirmUpdate.Value;
						hdnConfirmUpdate.Value = "";
                        bnSchedule.Style["color"] = IsCaseHasCorrespondence(PLCSessionVars1.PLCGlobalCaseKey) ? "Red" : "Black";
                    }
                    else if (hasChanges)
                    {
                        mInput.ShowMsg("Case update reason", "Please enter the reason for your changes", 0, txtConfirmUpdate.ClientID, btnConfirmUpdate.ClientID, "Save", "Cancel");
                        e.button_canceled = true;
                        return;
                    }
                }

                if (hasChanges)
                {
                    PLCDBGlobalClass.MarkAssignmentsForRegeneration(PLCSessionVars1.PLCGlobalCaseKey, "", "LABCASE", URNNarrative + auditText);
                }

				if (PLCDBPanel1.HasChanges())
					PLCDBPanel1.ReasonChangeKey = PLCDBGlobalClass.GenerateReasonChange("CASE TAB SAVE", reasonText);
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



                UpdateSubmission1();

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

				PLCDBPanel1.ReasonChangeKey = 0;
                PLCDBPanel1.DoLoadRecord();
            }


            if ((e.button_name == "CANCEL") && (e.button_action == "BEFORE"))
            {
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



        private void UpdateSubmission1()
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

				if (!string.IsNullOrEmpty(originalValue) && originalValue != newValue)
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
                mbox.ShowMsg("Case Info", message, 0);
                ViewState["PostSaveMessage"] = null;
            }
        }

        protected void PrintCaseLabel()
        {
            if (PLCSessionVars1.GetLabCtrl("USES_WS_BARCODE_PRINTING") == "T")
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

            PLCSessionVars1.PLCCrystalReportName = "BCLIST";
            //PLCSessionVars1.PLCCrystalSelectionFormula = "{TV_LABCASE.CASE_KEY} = " + PLCSessionVars1.PLCGlobalCaseKey;
            PLCSessionVars1.PLCCrystalReportTitle = "Case Barcode Label";
            PLCSessionVars1.PrintCRWReport();
        }

        protected void bnDiscoveryPacket_Click(object sender, EventArgs e)
        {
            Session["FROMCASEINFOTAB"] = "T";
            Response.Redirect("~/DiscoveryPacket.aspx");
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
            string result = "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!"; //default

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
            PLCQuery qryLabRef = new PLCQuery();
            qryLabRef.SQL = "Select CASE_KEY, REFERENCE, REFERENCE_TYPE, REFERENCED_CASE_KEY from TV_LABREF where CASE_KEY = " + CaseKey + " And ((SYSTEM_GENERATED IS NULL) OR (SYSTEM_GENERATED <> 'T')) ORDER BY ENTRY_TIME_STAMP";
            qryLabRef.Open();
            
            if (!qryLabRef.IsEmpty())
                ReferenceExists = true;

            while (!qryLabRef.EOF())
            {
                if (qryLabRef.FieldByName("REFERENCE").Trim() != "")
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
                qryLabRef.Next();
            }
            return str;
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
                bnCaseLock.Text = value ? "Unlock Case" : "Lock Case";                
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
                bnSupplements.Text = PLCSessionVars1.GetLabCtrl("SUPPLEMENT_TEXT").ToString();
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
            //ScriptManager.RegisterStartupScript(this, this.GetType(), "EnableLabCaseChange", string.Format("EnableLabCaseNumberChangeButton({0});", bEnable ? "true" : "false"), true);
        }

        protected void btnChangeLabCaseOK_Click(object sender, EventArgs e)
        {
            string sLabCode = "";
            int iLabCaseYear = -1;
            int iLabCaseNumber = -1;
            string labcase = "";

            PLCQuery qryCase = new PLCQuery("select LAB_CODE, LAB_CASE_YEAR, LAB_CASE_NUMBER, LAB_CASE from TV_LABCASE where CASE_KEY=?");
            qryCase.AddParameter("CASE_KEY", PLCSession.PLCGlobalCaseKey);
            qryCase.Open();
            if (qryCase.HasData())
            {
                sLabCode = qryCase.FieldByName("LAB_CODE");
                iLabCaseYear = qryCase.iFieldByName("LAB_CASE_YEAR");
            }

            if (this.txtLabCase.Text.Trim() != "")
            {
                labcase = this.txtLabCase.Text.Trim();
                if (labcase.Length > 15)
                    labcase = labcase.Substring(0,15);
            }

            if (chLabCode.Code.Trim() != "")
            {
                sLabCode = chLabCode.Code.Trim();
            }

            if (txtLabCaseYear.Text.Trim() != "")
            {
                int newYear;
                if (Int32.TryParse(txtLabCaseYear.Text, out newYear))
                    iLabCaseYear = newYear;
                else
                {
                    this.mbox.ShowMsg("Invalid Year", "Please enter a valid 4-digit year.", 1);
                    this.mpeChangeLabCase.Show();
                    return;
                }
            }

            if (txtLabCaseNumber.Text.Trim() != "")
            {
                if (!Int32.TryParse(txtLabCaseNumber.Text, out iLabCaseNumber))
                {
                    this.mbox.ShowMsg("Invalid Lab Case Number", "Please enter a valid number for Lab Case Number.", 1);
                    this.mpeChangeLabCase.Show();
                    return;
                }
            }

            if (iLabCaseYear != -1)
            {
                if ((iLabCaseYear < 1900) || (iLabCaseYear > 3000))
                {
                    this.mbox.ShowMsg("Invalid Year", "Please enter a valid 4-digit year.", 1);
                    this.mpeChangeLabCase.Show();
                    return;
                }
            }
            
            if (iLabCaseNumber != -1)
            {
                int nextLabCaseNumber = PLCCommon.instance.GetNextLabCaseNumberNoIncrement(sLabCode, iLabCaseYear);

                // No next lab case number defined.
                if (nextLabCaseNumber < 0)
                {
                    this.mbox.ShowMsg("Invalid Lab Case Number", String.Format("The system does not have a next Case Number for the year {0}.<br><br>Call your adminstrator to set up the case number for the year {0} through the Config program.", iLabCaseYear), 1);
                    this.mpeChangeLabCase.Show();
                    return;
                }

                // Don't allow a Lab Case Number that equals or comes after the next available lab case number.
                if (iLabCaseNumber >= nextLabCaseNumber)
                {
                    this.mbox.ShowMsg("Invalid Lab Case Number", String.Format("The Lab Case Number cannot be greater than or equal to the next case number {0}.", PLCCommon.instance.FormatCaseNumber(sLabCode, iLabCaseYear, nextLabCaseNumber)), 1);
                    this.mpeChangeLabCase.Show();
                    return;
                }
            }

            // Update the lab case number fields.
            UpdateLabCaseNumberFields(PLCSession.PLCGlobalCaseKey, sLabCode, iLabCaseYear, iLabCaseNumber, labcase);

            if (labcase != "")
                PLCDBPanel1.setpanelfield("LAB_CASE", labcase);

            ResetChangeLabCaseDialog();

            SetLabCaseChangeEnableScript(true);
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

        protected void btnChangeLabCaseCancel_Click(object sender, EventArgs e)
        {
            ResetChangeLabCaseDialog();

            SetLabCaseChangeEnableScript(true);
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
                this.mpeChangeLabCase.Show();
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
            this.chLabCode.Code = "";
            this.txtLabCaseYear.Text = "";
            this.txtLabCaseNumber.Text = "";
            this.txtLabCase.Text = "";
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

                mbox.ShowMsg("Record Lock", "Case locked by another user for editing.<br/>" + LockedInfo, 1);
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
            qryImages.SQL = @"SELECT I.ENTRY_ID, P.DATA_TYPE, I.FORMAT
FROM TV_IMAGES I 
LEFT OUTER JOIN TV_PRINTLOG P ON P.IMAGE_ID = I.IMAGE_ID 
WHERE P.CASE_KEY = " + PLCSession.PLCGlobalCaseKey + 
@" UNION
SELECT I.ENTRY_ID, E.DATA_TYPE, I.FORMAT
FROM TV_IMAGES I 
LEFT OUTER JOIN TV_EXAMIMAG E ON E.IMAGES_TABLE_ID = I.IMAGE_ID
INNER JOIN TV_LABEXAM LE ON LE.EXAM_KEY = E.EXAM_KEY
WHERE LE.CASE_KEY = " + PLCSession.PLCGlobalCaseKey + 
@" UNION 
SELECT I.ENTRY_ID, E.DATA_TYPE, I.FORMAT
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


        private bool HasDistribution(string caseKey)
        {
            PLCQuery qryCaseDist = new PLCQuery();
            qryCaseDist.SQL = "SELECT * FROM TV_CASEDIST WHERE CASE_KEY = ?";
            qryCaseDist.AddSQLParameter("CASE_KEY", caseKey);
            qryCaseDist.Open();
            return qryCaseDist.HasData();
        }
    }
}
