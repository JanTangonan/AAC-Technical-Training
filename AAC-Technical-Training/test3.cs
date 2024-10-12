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
using System.Data.OleDb;
using System.Data.OracleClient;
using System.Globalization;

namespace BEASTiLIMS
{
    public partial class TAB4Items : PageBase
    {
        #region Properties and Declarations

        [Serializable]
        public class TRelation
        {
            public string Code;
            public string Description;
        }

        public class StatusConfirmRec
        {
            public String fromStatus;
            public String itemNumbers;
            public String msgFormat;

            // this will format a message in singluar or plral form depending on the number of items in the list
            //  singular    | plural          single | plural
            // [This item is|These items are][an item|items]in a list.[Item|Items]:<ITEMS>
            public String getConfirmationMessage()
            {
                try
                {
                    String[] itemlist = itemNumbers.Trim().Split(',');
                    String tempMsg = msgFormat;
                    int pipeIDX = tempMsg.IndexOf("|");
                    int openIDX = 0;
                    int closeIDX = 0;
                    while (pipeIDX >= 0)
                    {
                        openIDX = tempMsg.IndexOf("[");
                        closeIDX = tempMsg.IndexOf("]");
                        if ((openIDX >= 0) && (pipeIDX > openIDX) && (closeIDX > pipeIDX))
                        {
                            if (itemlist.Count() == 1)
                            {
                                tempMsg = tempMsg.Remove(pipeIDX, closeIDX - pipeIDX + 1);
                                tempMsg = tempMsg.Remove(openIDX, 1);
                            }
                            else
                            {
                                tempMsg = tempMsg.Remove(closeIDX, 1);
                                tempMsg = tempMsg.Remove(openIDX, pipeIDX - openIDX + 1);

                            }

                            pipeIDX = tempMsg.IndexOf("|");
                        }
                        else pipeIDX = -1;


                    }

                    tempMsg = tempMsg.Replace("<ITEMS>", itemNumbers);
                    tempMsg = tempMsg.Replace("<items>", itemNumbers);

                    return tempMsg;
                }
                catch
                {
                    return msgFormat + ":" + itemNumbers;
                }

            }
        }

        PLCMsgBox mbox = new PLCMsgBox();
        PLCMsgBox mbox2 = new PLCMsgBox();
        PLCMsgBox mbox3 = new PLCMsgBox();
        PLCMsgComment mInput = new PLCMsgComment();
        PLCDBGlobal dbgbl = new PLCDBGlobal();
        PLCCommon PLCCommonClass = new PLCCommon();
        PLCHelperFunctions plcHelp = new PLCHelperFunctions();
        PLCCommon_PLCCopyRights UC_Status;

        public const int VIEW_CURRENCY = 0;
        public const int VIEW_ATTRIBUTE = 1;
        public const int VIEW_NAMES = 2;

        public const int NAMEITEM_GRID_COL_CHECK = 0;
        public const int NAMEITEM_GRID_COL_NAMEKEY = 1;
        public const int NAMEITEM_GRID_COL_DESC = 2;
        public const int NAMEITEM_GRID_COL_COUNT = 3;
        public const int NAMEITEM_GRID_COL_RELCODE = 4;
        public const int NAMEITEM_GRID_COL_DDL = 5;

        public const int RBL_ITEMTYPE_ITEM = 0;
        public const int RBL_ITEMTYPE_FILE = 1;

        public bool d_Attribute { get; set; }
        public bool d_Currency { get; set; }

        List<string> DELITEMECNS
        {
            get
            {
                return PLCSession.GetProperty<List<string>>("DELITEMECNS", new List<string>());
            }
            set
            {
                PLCSession.SetProperty<List<string>>("DELITEMECNS", value);
            }
        }

        List<string> AttachmentEntryIDs
        {
            get
            {
                if (ViewState["ItemAttachEntryIDs"] == null)
                {
                    ViewState["ItemAttachEntryIDs"] = new List<string>();
                    return (List<string>)ViewState["ItemAttachEntryIDs"];
                }
                else
                {
                    return (List<string>)ViewState["ItemAttachEntryIDs"];
                }
            }
            set
            {
                ViewState["ItemAttachEntryIDs"] = value;
            }
        }

        List<string> sItemECNs = new List<string>();
        List<string> selectedItemSCList = null;
        List<StatusConfirmRec> confirmList = null;

        public Dictionary<string, List<string>> ExamAttachments
        {
            get
            {
                if (ViewState["AttachExamLogKey"] == null)
                {
                    ViewState["AttachExamLogKey"] = new Dictionary<string, List<string>>();
                    return (Dictionary<string, List<string>>)ViewState["AttachExamLogKey"];
                }
                else
                {
                    return (Dictionary<string, List<string>>)ViewState["AttachExamLogKey"];
                }
            }
            set
            {
                ViewState["AttachExamLogKey"] = value;
            }
        }

        /// <summary>
        /// Batch Sequence generated on save of signatures
        /// </summary>
        int EvidenceReportBatchSequence
        {
            get
            {
                if (ViewState["EVIDREPORT_BATCHSEQ"] == null)
                    return 0;
                return Convert.ToInt32(ViewState["EVIDREPORT_BATCHSEQ"]);
            }
            set
            {
                ViewState["EVIDREPORT_BATCHSEQ"] = value;
            }
        }

        string ReportFormatID
        {
            get
            {
                return Convert.ToString(ViewState["REPORTFORMAT_ID"]);
            }
            set
            {
                ViewState["REPORTFORMAT_ID"] = value;
            }
        }

        string ReportFormatNumber
        {
            get
            {
                return Convert.ToString(ViewState["REPORTFORMAT_NUMBER"]);
            }
            set
            {
                ViewState["REPORTFORMAT_NUMBER"] = value;
            }
        }

        Label ErrorMessage;
        string CurrentECN = "";
        string ParentECN = "";

        private string secondrevchar = "";

        private Boolean CheckPrintSampleContainerLabel
        {
            get
            {
                return chkPrinLabel.Checked;
            }
            set
            {
                chkPrinLabel.Checked = value;
            }
        }

        private Boolean IsServiceRequestsEnabled
        {
            get
            {
                object obj = ViewState["UsesServiceRequest"];
                if (obj == null)
                {
                    ViewState["UsesServiceRequest"] = PLCDBGlobal.instance.UserHasServiceRequestAccess();
                }

                return Convert.ToBoolean(ViewState["UsesServiceRequest"]);
            }
        }

        private bool UserCanRoute
        {
            get
            {
                if (ViewState["ucr"] == null)
                    ViewState["ucr"] = PLCDBGlobal.instance.UserCanRoute(PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalAnalyst, PLCSession.PLCGlobalAnalystDepartmentCode);
                return Convert.ToBoolean(ViewState["ucr"]);
            }
        }

        private bool CanUseTransferButton
        {
            get
            {
                // must have 'Handle Evidence' and 'Can Use Transfer Button'
                return (PLCSession.CheckUserOption("HANDLE") && PLCSession.CheckUserOption("MANUALCUST"));
            }
        }

        private bool GetLabCtrl_USES_WS_BARCODE_PRINTING
        {
            get
            {
                return PLCSession.GetLabCtrl("USES_WS_BARCODE_PRINTING") == "T";
            }
        }

        private string RecordsLocked
        {
            get
            {
                if (ViewState["RecordsLocked"] != null)
                    return (string)ViewState["RecordsLocked"];
                return string.Empty;
            }
            set
            {
                ViewState["RecordsLocked"] = value;
            }
        }

        private string RecordsLockedInfo
        {
            get
            {
                if (ViewState["RecordsLockedInfo"] != null)
                    return (string)ViewState["RecordsLockedInfo"];
                return string.Empty;
            }
            set
            {
                ViewState["RecordsLockedInfo"] = value;
            }
        }

        private bool ItemTypeChangeConfirmed
        {
            get
            {
                if (ViewState["itcc"] == null)
                    return false;
                return Convert.ToBoolean(ViewState["itcc"]);
            }
            set
            {
                ViewState["itcc"] = value;
            }
        }

        private string FloorCustody
        {
            get
            {
                if (ViewState["FloorCustody"] == null)
                    return string.Empty;

                return (string)ViewState["FloorCustody"];
            }
            set
            {
                ViewState["FloorCustody"] = value;
            }
        }

        private string FloorLocation
        {
            get
            {
                if (ViewState["FloorLocation"] == null)
                    return string.Empty;

                return (string)ViewState["FloorLocation"];
            }
            set
            {
                ViewState["FloorLocation"] = value;
            }
        }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            this.hdnCaseKey.Value = PLCSession.PLCGlobalCaseKey;
            this.hdnBulkDelete.Value = PLCSession.CheckUserOption("BULKDELETE") ? "T" : "F";

            ScriptManager.RegisterStartupScript(this, this.GetType(), "updateGridIcons", "updateItemsGridIcons(0);", true);
            ScriptManager.RegisterStartupScript(this, this.GetType(), "S6" + DateTime.Now.Millisecond, "Supp49Grid();", true);

            Control CtrlMsgBox = LoadControl("~/PLCWebCommon/PLCMsgBox.ascx");
            mbox = (PLCMsgBox)CtrlMsgBox;
            phMsgBox.Controls.Add(mbox);
            mbox.OnOkScript = "";

            Control CtrlMsgBox2 = LoadControl("~/PLCWebCommon/PLCMsgBox.ascx");
            mbox2 = (PLCMsgBox)CtrlMsgBox2;
            phMsgBoxPopup.Controls.Add(mbox2);
            mbox2.OnOkScript = "";

            Control CtrlMsgBox3 = LoadControl("~/PLCWebCommon/PLCMsgBox.ascx");
            mbox3 = (PLCMsgBox)CtrlMsgBox3;
            phMsgBoxPopup.Controls.Add(mbox3);

            Control CtrlmInput = LoadControl("~/PLCWebCommon/PLCMsgComment.ascx");
            mInput = (PLCMsgComment)CtrlmInput;
            phMsgBoxComments.Controls.Add(mInput);

            if (PLCSession.PLCGlobalCaseKey == "")
            {
                Response.Redirect("~/DashBoard.aspx");
                return;
            }
            PLCCommonClass.SetSelectedMenuItem(MainMenuTab.ItemsTab, (Menu)Master.FindControl("menu_main"));

            // Set page title.
            ((MasterPage)Master).SetCaseTitle(""); //AACI 07/27/2010

            UC_Status = (PLCCommon_PLCCopyRights)Master.FindControl("UC_PLCCopyRights_Master2");
            ErrorMessage = (Label)UC_Status.FindControl("lbError");

            ShowSeizedForBiology();

            ViewState["MandatoryFieldMissing"] = false;

            PopulateEvidenceReport();
            hdnDBPanelFields.Value = dbp49Supp.GetAddToListScript();

            if (!IsPostBack)
            {
                PLCDBGlobal.instance.RemoveRecordLocks("TV_LABITEM", PLCSession.PLCGlobalCaseKey, null, null, PLCSession.PLCGlobalAnalyst);
                PLCSession.AddToRecentCases(PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalAnalyst);

                hdnUsesCryobox.Value = PLCSession.GetLabCtrl("USES_ITEMS_CRYOBOX");
                hdnUsesProduct.Value = PLCSession.GetLabCtrl("USES_WORK_PRODUCT");

                ViewState["AttributesChanges"] = null;
                ViewState["CurrencyChanges"] = null;
                ViewState["ItemNamesChanges"] = null;
                ViewState["NewRecordECN"] = null;
                PLCSession.SetDefault("LABSTAT_COMMENTS", null);
                bnTransfer.Visible = CanUseTransferButton;
                btnSupp49.Visible = ((PLCSession.GetLabCtrl("USES_EVID_REPORT") == "T") && !PLCSession.CheckUserOption("HIDEITEMEV"));
                bnSample.Visible = (!PLCSession.CheckUserOption("HIDESAMPLE"));
                bnKit.Visible = (!PLCSession.CheckUserOption("HIDEKIT"));
                bnContainer.Visible = (PLCSession.CheckUserOption("HANDLE")) && (!PLCSession.CheckUserOption("HIDECCONT"));
                btnTrackRFID.Visible = PLCDBGlobal.instance.GetBeastINIValue(PLCSession.ClientID, "HAS_RFID_SCANNER").ToUpper() == "T";

                // set floor custody and floor location properties
                SetFloorCustodyLocation();

                //get case_manager and case_analyst
                string case_manager = string.Empty;
                string case_analyst = string.Empty;
                PLCQuery qryInvOfficer = new PLCQuery("SELECT CASE_MANAGER, CASE_ANALYST FROM TV_LABCASE WHERE CASE_KEY = " + PLCSession.PLCGlobalCaseKey);
                qryInvOfficer.Open();
                if (!qryInvOfficer.IsEmpty())
                {
                    case_manager = qryInvOfficer.FieldByName("CASE_MANAGER");
                    case_analyst = qryInvOfficer.FieldByName("CASE_ANALYST");
                }

                if ((PLCSession.CheckUserOption("ITEMSTATCH")) || ((PLCSession.GetLabCtrl("IO_CHANGE_ITEM_STATUS") == "T") && ((case_manager == PLCSession.PLCGlobalAnalyst) || (case_analyst == PLCSession.PLCGlobalAnalyst))))
                {
                    btnStatusChange.Visible = true;
                }
                else
                {
                    btnStatusChange.Visible = false;
                }

                CheckSubmExtraInfoHighlight();

                tbCurrency.Visible = (PLCSession.GetLabCtrl("USES_CURRENCY_ENTRY") == "T");

                GridView1.PLCGridName = "CASE_ITEMS";
                GridView1.PLCErrorMessageLabel = ErrorMessage;
                if (PLCSession.GetLabCtrl("SHOW_FILE_ITEM") != "T")
                {
                    GridView1.PLCSQLString_AdditionalCriteria = " AND LAB_ITEM_NUMBER != '0'";
                }

                if (!string.IsNullOrEmpty(PLCSession.GetLabCtrl("EVIDENCE_PAGE_BUTTON_TEXT").Trim()))
                    btnSupp49.Text = PLCSession.GetLabCtrl("EVIDENCE_PAGE_BUTTON_TEXT");

                if (PLCSession.GetLabCtrl("ROUTE_ITEMS_TEXT") != "")
                    bnItemList.Text = PLCSession.GetLabCtrl("ROUTE_ITEMS_TEXT");

                bnItemList.Visible = (PLCSession.GetLabCtrl("USES_WEB_ITEM_LIST") == "T") 
                    &&  ((PLCSession.CheckUserOption("MAKEDREQ") || PLCHelperFunctions.CaseHasUserAsOfficer(PLCSession.PLCGlobalCaseKey)));

                //Use GetSortValue only if the database used is MSSQL and if Container Description is numeric
                if (PLCSession.PLCDatabaseServer == "MSSQL" && PLCSession.GetLabCtrl("SUGGEST_NEXT_CONTAINER") == "N")
                    GridView1.AddAdditionalSqlColumn("dbo.GetSortValue(CO.CONTAINER_DESCRIPTION, 1) AS CONT_DESC_SORT");
                else
                    GridView1.AddAdditionalSqlColumn("CO.CONTAINER_DESCRIPTION AS CONT_DESC_SORT");

                GridView1.InitializePLCDBGrid();

                // empty tab
                if (GridView1.Rows.Count == 0)
                {
                    PLCDBPanel1.EmptyMode();
                    PLCButtonPanel1.SetEmptyMode();
                    SetAttachmentsButton("CASE");
                    EnableButtonControls(false, false);
                    RecreateBlankAttributes(true);
                    LoadNames();
                    PLCButtonPanel1.SetCustomButtonEnabled("Bulk Delete", false);
                    PLCButtonPanel1.DisableLock();
                }

                // RecordUnlock
                if (!PLCSession.CheckUserOption("DELLOCKS"))
                    PLCButtonPanel1.SetCustomButtonVisible("RecordUnlock", false);

                //BulkDelete Button (Shift + Delete)
                if (hdnBulkDelete.Value != "T")
                    PLCButtonPanel1.SetCustomButtonVisible("Bulk Delete", false);

                if (PLCSession.CheckUserOption("RESDEPTANL"))
                {
                    var codeCondition = "DEPARTMENT_CODE = '" + PLCSession.PLCGlobalAnalystDepartmentCode + "'";
                    PLCDBPanel1.SetPanelCodeCondition("COLLECTED_BY", codeCondition);
                    PLCDBPanel1.SetPanelCodeCondition("BOOKED_BY", codeCondition);
                }

                UnLockKit();

                string ecnkey = PLCSession.PLCGlobalECN;
                if ((ecnkey != null) && (ecnkey != "") && (ecnkey != "0") && (GridView1.Rows.Count > 0))
                {
                    for (int intPage = 0; intPage < GridView1.PageCount; intPage++)
                    {
                        GridView1.PageIndex = intPage;
                        GridView1.DataBind();
                        for (int i = 0; i < GridView1.DataKeys.Count; i++)
                        {
                            if (Convert.ToString(GridView1.DataKeys[i].Value) == ecnkey)
                            {
                                // If it is a match set the variables and exit
                                GridView1.SelectedIndex = i;
                                GridView1.PageIndex = intPage;
                                GridView1.SetClientSideScrollToSelectedRow();

                                UpdateCustodyStatusDisplay(ecnkey);

                                GrabGridRecord();
                                ShowDefaultTab();

                                CheckPrintLabel();
                                CheckUserOptionReadOnlyAccess();
                                PDCheckItemReceivedByLab();
                                return;
                            }
                        }
                    }
                }
                else
                {
                    ClearCustodyStatusDisplay();
                }

                LoadBulkDeletePopup();
            }
            else
                GridView1.SetPLCGridProperties();

            if (GridView1.Rows.Count > 0)
            {
                if (GridView1.SelectedIndex < 0)
                {
                    GridView1.SelectedIndex = 0;
                    PLCSession.PLCGlobalECN = GridView1.SelectedDataKey.Value.ToString();
                    UpdateCustodyStatusDisplay(PLCSession.PLCGlobalECN);
                    GrabGridRecord();

                    if (!IsPostBack)
                    {
                        ShowDefaultTab();
                    }
                }
                else
                {
                    SetItemDescriptionRequired();
                    SetThirdPartyBarcodeVisibility();
                }
            }
            else
            {
                // No items.
                // But we still need to set the state of fields when Add Item is clicked, 
                //   to detect this, check if the dbpanel is in edit mode.
                if (!PLCDBPanel1.IsBrowseMode)
                {
                    SetItemDescriptionRequired();
                    SetThirdPartyBarcodeVisibility();
                }
            }

            if (!PLCDBGlobal.instance.DBPanelExists("EVIDENCE_PAGE"))
                pnldbp49Supp.Visible = false;

            GridView1.AllowPaging = false;

            CheckPrintLabel();
            CheckUserOptionReadOnlyAccess();
            lblPackagedIn.Text = PLCSession.ContainerPackagedInText();

            PDCheckItemReceivedByLab();

            if (!string.IsNullOrEmpty(ReportFormatID))
            {
                InitEvidReportCustomDBPanel();
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            string script = "";

            if (vwAttribute.Style["display"].ToUpper() != "NONE")
            {
                script = @"$(function() {
        var tabindex = " + PLCDBPanel1.HighestTabIndex + @";
        $(""ul.tabs :tabbable,:input[id*='" + ItemAttribute.ClientID + @"']"").each(function() {
            if (this.type != ""hidden"") {
                var $input = $(this);
                $input.attr(""tabIndex"", tabindex);
                tabindex++;
            }
        });
        $(""input[id*='" + PLCButtonPanel1.ClientID + @"']"").each(function() {
            if (this.type != ""hidden"") {
                var $input = $(this);
                $input.attr(""tabIndex"", tabindex);
                tabindex++;
            }
        });
    });";
            }

            if (script != "")
                ScriptManager.RegisterStartupScript(this, this.GetType(), "tabindex", script, true);
        }

        protected void rblItemType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rblItemType.SelectedIndex == RBL_ITEMTYPE_ITEM)
            {
                //GridView1.PLCSQLString = "Select EVIDENCE_CONTROL_NUMBER,CASE_KEY,LAB_CASE_SUBMISSION,ITEM_TYPE,PACKAGING_CODE,ITEM_DESCRIPTION,LAB_ITEM_NUMBER,DEPARTMENT_ITEM_NUMBER,CUSTODY_OF,LOCATION,PROCESS from TV_LABITEM Where CASE_KEY = " + PLCSession.PLCGlobalCaseKey + " AND ITEM_TYPE != 'FILE' Order by ITEM_SORT";
                if (PLCSession.GetLabCtrl("SHOW_FILE_ITEM") != "T")
                {
                    GridView1.PLCSQLString_AdditionalCriteria = " AND I.LAB_ITEM_NUMBER != '0' And I.ITEM_TYPE != 'FILE' ";
                }
                else
                {
                    GridView1.PLCSQLString_AdditionalCriteria = " AND I.ITEM_TYPE != 'FILE' ";
                }

                GridView1.InitializePLCDBGrid();
            }
            else if (rblItemType.SelectedIndex == RBL_ITEMTYPE_FILE)
            {
                //GridView1.PLCSQLString = "Select EVIDENCE_CONTROL_NUMBER,CASE_KEY,LAB_CASE_SUBMISSION,ITEM_TYPE,PACKAGING_CODE,ITEM_DESCRIPTION,LAB_ITEM_NUMBER,DEPARTMENT_ITEM_NUMBER,CUSTODY_OF,LOCATION,PROCESS from TV_LABITEM Where CASE_KEY = " + PLCSession.PLCGlobalCaseKey + " AND ITEM_TYPE = 'FILE' Order by ITEM_SORT";
                if (PLCSession.GetLabCtrl("SHOW_FILE_ITEM") != "T")
                {
                    GridView1.PLCSQLString_AdditionalCriteria = " AND I.LAB_ITEM_NUMBER != '0' And I.ITEM_TYPE = 'FILE' ";
                }
                else
                {
                    GridView1.PLCSQLString_AdditionalCriteria = " AND I.ITEM_TYPE = 'FILE' ";
                }

                GridView1.InitializePLCDBGrid();
            }
            else
            {
                if (PLCSession.GetLabCtrl("SHOW_FILE_ITEM") != "T")
                {
                    GridView1.PLCSQLString_AdditionalCriteria = " AND I.LAB_ITEM_NUMBER != '0'";
                }
                else
                {
                    GridView1.PLCSQLString_AdditionalCriteria = " AND (I.ITEM_TYPE = 'FILE' OR I.ITEM_TYPE != 'FILE') ";
                }

                GridView1.InitializePLCDBGrid();
            }
            GridView1.SetPLCGridProperties();
            if (GridView1.Rows.Count == 0)
            {
                PLCDBPanel1.EmptyMode();
                PLCButtonPanel1.SetEmptyMode();
                EnableButtonControls(false, false);
            }
            else
            {
                GridView1.SelectedIndex = 0;

                PLCSession.PLCGlobalECN = GridView1.SelectedDataKey.Value.ToString();
                UpdateCustodyStatusDisplay(PLCSession.PLCGlobalECN);

                GrabGridRecord();
            }
        }

        protected void Currency1_Validated(object sender, EventArgs e)
        {
            ClickSavePLCButton();
        }

        protected void bnCurrency_Click(object sender, EventArgs e)
        {
            if (GridView1.Rows.Count == 0)
                return;

            SelectTab(VIEW_CURRENCY);

            Currency1.LoadData();
        }

        protected void bnNames_Click(object sender, EventArgs e)
        {
            if (GridView1.Rows.Count == 0)
                return;

            SelectTab(VIEW_NAMES);
        }

        protected void bnAttribute_Click(object sender, EventArgs e)
        {
            if (GridView1.Rows.Count == 0)
                return;

            SelectTab(VIEW_ATTRIBUTE);
        }

        protected void bnDupe_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(PLCSession.PLCGlobalECN))
            {
                mbox.ShowMsg("Item", "Please select an Item.", 1);
                return;
            }

            if (IsSelectedRecordsLocked(null))
            {
                dlgMessage.ShowAlert("Record Lock", "Item locked by another user for editing.<br/>" + RecordsLockedInfo);
                return;
            }

            ViewState["DUPE_ITEM"] = true;

            CurrentECN = PLCSession.PLCGlobalECN;
            PLCButtonPanel1.ClickAddButton();
            CurrentECN = "";
        }

        protected void bnSample_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(PLCSession.PLCGlobalECN))
            {
                mbox.ShowMsg("Sample Item", "Please select an Item.", 1);
                return;
            }

            if (IsSelectedRecordsLocked(null))
            {
                dlgMessage.ShowAlert("Record Lock", "Item locked by another user for editing.<br/>" + RecordsLockedInfo);
                return;
            }

            if (PLCDBGlobal.instance.CustodyPreventAccess(Convert.ToInt32(PLCSession.PLCGlobalECN), false))
            {
                bool canOverrideCustody = false;
                string targetButton = string.Empty;
                string script = string.Empty;

                canOverrideCustody = PLCSession.CheckUserOption("CUSTOVER");

                if (canOverrideCustody)
                {
                    PLCSession.SetDefault("LABSTAT_COMMENTS", null);

                    targetButton = btnCustodyChangeYes.ClientID;
                    script =
                        "ShowCustodyChange(function() {" +
                            "var btn = document.getElementById('" + targetButton + "');" +
                            "if (btn != null) " +
                                "btn.click();" +
                        "});";

                    ScriptManager.RegisterStartupScript(Page, Page.GetType(), "CustodyChange_" + Guid.NewGuid(), script, true);
                }
                else
                    dlgMessage.ShowAlert("Sample Item", "Parent Item # " + PLCDBPanel1.getpanelfield("LAB_ITEM_NUMBER") + " is not in your Custody.");

                return;
            }

            ViewState["SAMPLE_ITEM_NEW_CONTAINER"] = true;

            ParentECN = PLCSession.PLCGlobalECN;
            PLCButtonPanel1.ClickAddButton();
            ParentECN = "";
        }

        protected void bnKit_Click(object sender, EventArgs e)
        {
            if (IsSelectedRecordsLocked(null))
            {
                dlgMessage.ShowAlert("Record Lock", "Item locked by another user for editing.<br/>" + RecordsLockedInfo);
                return;
            }

            if (PLCSession.GetLabCtrl("SAMPLES_REQUIRE_CUSTODY") == "T" && !PLCDBGlobal.instance.IsItemInAnalystCustody(PLCSession.PLCGlobalECN))
            {
                dlgMessage.ShowAlert("Kit Item", "Parent Item # " + PLCDBPanel1.getpanelfield("LAB_ITEM_NUMBER") + " is not in your Custody.");
                return;
            }

            PLCQuery qryKit = new PLCQuery(string.Format("SELECT * FROM TV_KITITEMS WHERE KIT_CODE = '{0}'", PLCDBPanel1.getpanelfield("ITEM_TYPE")));
            if (qryKit.Open() && !qryKit.IsEmpty())
            {
                PLCDBGlobal.instance.LockUnlockRecord("TV_LABITEM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, "-1", true);

                if (PLCSession.CheckUserOption("TASKITEM"))
                {
                    KitTask1.EvidenceControlNumber = Convert.ToInt32(PLCSession.PLCGlobalECN);
                    KitTask1.Show();
                }
                else
                {
                    ParentECN = "";
                    chkListKit.Items.Clear();
                    chkKitPrintLabels.Enabled = PLCSession.CheckUserOption("PRINTBAR");

                    PLCQuery qryKitItems = new PLCQuery(string.Format("SELECT K.ITEM_TYPE, K.KIT_SEQUENCE, K.ITEM_DESCRIPTION, IT.DESCRIPTION " +
                        "FROM TV_KITITEMS K INNER JOIN TV_LABITEM L ON L.ITEM_TYPE = K.KIT_CODE INNER JOIN TV_ITEMTYPE IT ON IT.ITEM_TYPE = K.ITEM_TYPE " +
                        "WHERE L.CASE_KEY = {0} AND EVIDENCE_CONTROL_NUMBER = {1} ORDER BY K.KIT_CODE, K.KIT_SEQUENCE", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN));
                    if (qryKitItems.Open() && !qryKitItems.IsEmpty())
                    {
                        ListItem item;
                        while (!qryKitItems.EOF())
                        {
                            item = new ListItem(qryKitItems.FieldByName("KIT_SEQUENCE") + ". " + qryKitItems.FieldByName("DESCRIPTION") +
                                (qryKitItems.FieldByName("ITEM_DESCRIPTION").Length > 0 ? " : " + qryKitItems.FieldByName("ITEM_DESCRIPTION") : ""),
                                qryKitItems.FieldByName("ITEM_TYPE"));

                            chkListKit.Items.Add(item);
                            qryKitItems.Next();
                        }
                        mpeKit.Show();
                    }
                }
            }
            else
                mbox.ShowMsg("Kit Item", "No Kits have been defined for this Item Type.", 0);
        }

        protected void btnKitCancel_Click(object sender, EventArgs e)
        {
            PLCDBGlobal.instance.RemoveRecordLocks("TV_LABITEM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, "-1", PLCSession.PLCGlobalAnalyst);
        }

        protected void btnKitSave_Click(object sender, EventArgs e)
        {
            PLCDBGlobal.instance.RemoveRecordLocks("TV_LABITEM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, "-1", PLCSession.PLCGlobalAnalyst);

            StringBuilder sbEcnList = new StringBuilder();
            StringBuilder sbEcnListForWS = new StringBuilder();

            ParentECN = PLCSession.PLCGlobalECN;
            string batchKey = PLCSession.GetNextSequence("BATCH_SEQ").ToString();

            Dictionary<string, string> dictItemBCDefaultSettings = new Dictionary<string, string>();

            var isCaseFromCPD = PLCDBGlobal.instance.GetTableDetailsByCustomKey("TV_LABCASE", "CASE_FROM_CPD", "CASE_KEY", PLCSession.PLCGlobalCaseKey) == "T";

            PLCQuery qryParentItem = new PLCQuery();
            qryParentItem.SQL = "SELECT * FROM TV_LABITEM WHERE EVIDENCE_CONTROL_NUMBER = " + ParentECN;
            if (qryParentItem.Open() && !qryParentItem.IsEmpty())
            {
                foreach (ListItem item in chkListKit.Items)
                {
                    if (item.Selected)
                    {
                        if (!string.IsNullOrEmpty(PLCSession.PLCGlobalCaseKey))
                        {
                            string nextItemNumber = PLCDBGlobal.instance.GetSampleItemNumber(qryParentItem.FieldByName("CASE_KEY"), 
                                qryParentItem.FieldByName("LAB_ITEM_NUMBER"), qryParentItem.FieldByName("LAB_CASE_SUBMISSION"));

                            if (string.IsNullOrEmpty(nextItemNumber))
                            {
                                dlgMessage.ShowAlert("Kit Item", "There is an error in generating the Lab Item Number. Please contact the LIMS Administrator.");
                                break;
                            }

                            string newECN = Convert.ToString(PLCSession.GetNextSequence("LABITEM_SEQ"));

                            PLCQuery qryAddKitItem = new PLCQuery("SELECT * FROM TV_LABITEM WHERE 0 = 1");
                            qryAddKitItem.Open();
                            qryAddKitItem.Append();

                            PLCQuery qryKitItem = new PLCQuery(string.Format("SELECT K.* FROM TV_KITITEMS K JOIN TV_LABITEM L ON L.ITEM_TYPE = K.KIT_CODE " +
                                "WHERE L.CASE_KEY = {0} AND L.EVIDENCE_CONTROL_NUMBER = {1} AND K.ITEM_TYPE = '{2}'", PLCSession.PLCGlobalCaseKey, ParentECN, item.Value));

                            if (qryKitItem.Open() && !qryKitItem.IsEmpty())
                            {
                                qryAddKitItem.SetFieldValue("QUANTITY", qryKitItem.FieldByName("QUANTITY"));
                                qryAddKitItem.SetFieldValue("PACKAGING_CODE", qryKitItem.FieldByName("PACKAGING_CODE"));
                                qryAddKitItem.SetFieldValue("ITEM_TYPE", qryKitItem.FieldByName("ITEM_TYPE"));
                                qryAddKitItem.SetFieldValue("ITEM_DESCRIPTION", qryKitItem.FieldByName("ITEM_DESCRIPTION"));
                                qryAddKitItem.SetFieldValue("INNER_PACK", qryKitItem.FieldByName("INNER_PACK"));
                            }

                            qryAddKitItem.SetFieldValue("EVIDENCE_CONTROL_NUMBER", newECN);
                            qryAddKitItem.SetFieldValue("CASE_KEY", qryParentItem.FieldByName("CASE_KEY"));
                            qryAddKitItem.SetFieldValue("LAB_ITEM_NUMBER", nextItemNumber);
                            qryAddKitItem.SetFieldValue("ITEM_CATEGORY", qryParentItem.FieldByName("ITEM_CATEGORY"));
                            qryAddKitItem.SetFieldValue("CUSTODY_OF", qryParentItem.FieldByName("CUSTODY_OF"));
                            qryAddKitItem.SetFieldValue("LOCATION", qryParentItem.FieldByName("LOCATION"));
                            qryAddKitItem.SetFieldValue("ITEM_SORT", PLCCommon.instance.GetItemSort(nextItemNumber));
                            
                            if ((chkKitPrintLabels.Checked) && (PLCSession.GetLabCtrl("USES_SUBITEM_TRANSFER") == "T"))
                                qryAddKitItem.SetFieldValue("BARCODE", newECN); // this should be include logic to generate barcode
                            qryAddKitItem.SetFieldValue("LAB_CASE_SUBMISSION", qryParentItem.FieldByName("LAB_CASE_SUBMISSION"));
                            qryAddKitItem.SetFieldValue("PARENT_ECN", ParentECN);

                            if (qryParentItem.FieldByName("PROCESS") == "HOLD")
                            {
                                qryAddKitItem.SetFieldValue("PROCESS_REVIEW_DATE", qryParentItem.FieldByName("PROCESS_REVIEW_DATE"));
                                qryAddKitItem.SetFieldValue("PROCESS_DATE", qryParentItem.FieldByName("PROCESS_DATE"));
                                qryAddKitItem.SetFieldValue("PROCESS", qryParentItem.FieldByName("PROCESS"));
                            }

                            if (!isCaseFromCPD)
                            {
                                qryAddKitItem.SetFieldValue("DEPARTMENT_ITEM_NUMBER", qryParentItem.FieldByName("DEPARTMENT_ITEM_NUMBER"));
                                if (PLCSession.GetLabCtrl("DUPE_DEPT_ITEM_NUM_ON_SAMPLE") == "T")
                                {
                                    string nextDeptItem = qryParentItem.FieldByName("DEPARTMENT_ITEM_NUMBER");
                                    if (!String.IsNullOrEmpty(nextDeptItem))
                                        nextDeptItem = PLCDBGlobal.instance.GetSampleDeptItemNumber(qryParentItem.FieldByName("CASE_KEY"), nextDeptItem, qryParentItem.FieldByName("LAB_CASE_SUBMISSION"));

                                    qryAddKitItem.SetFieldValue("DEPARTMENT_ITEM_NUMBER", nextDeptItem);
                                    qryAddKitItem.SetFieldValue("DEPT_ITEM_SORT", PLCCommon.instance.GetItemSort(nextDeptItem));
                                }
                            }

                            if (PLCSession.GetLabCtrlFlag("KIT_NO_DEPT_ITEM_NUMBER").Equals("T"))
                                qryAddKitItem.SetFieldValue("DEPARTMENT_ITEM_NUMBER", string.Empty);

                            dictItemBCDefaultSettings = PLCSession.GetItemBCDefaultSettings("KIT");
                            foreach (KeyValuePair<String, String> pair in dictItemBCDefaultSettings)
                            {
                                if (pair.Value == "PARENT")
                                    qryAddKitItem.SetFieldValue(pair.Key, qryParentItem.FieldByName(pair.Key));
                                else if (pair.Value == "ANALYST")
                                {
                                    if (pair.Key == "COLLECTED_BY" || pair.Key == "BOOKED_BY")
                                        qryAddKitItem.SetFieldValue(pair.Key, PLCSession.PLCGlobalAnalyst);
                                }
                                else if (pair.Value == "CURRENT")
                                {
                                    if (pair.Key == "DATE_COLLECTED" || pair.Key == "BOOKING_DATE")
                                        qryAddKitItem.SetFieldValue(pair.Key, DateTime.Now.ToString("MM/dd/yyyy"));
                                    else if (pair.Key == "TIME_COLLECTED" || pair.Key == "BOOKING_TIME")
                                        qryAddKitItem.SetFieldValue(pair.Key, DateTime.Now.ToString("HH:mm"));

                                }
                                else//NONE or NULL
                                    qryAddKitItem.SetFieldValue(pair.Key, string.Empty);
                            }

                            if (PLCSession.GetLabCtrl("DUPE_COLLECTED_BY_AND_DATE") == "T")
                            {
                                qryAddKitItem.SetFieldValue("RECOVERY_LOCATION", qryParentItem.FieldByName("RECOVERY_LOCATION"));
                                qryAddKitItem.SetFieldValue("RECOVERY_ADDRESS_KEY", qryParentItem.FieldByName("RECOVERY_ADDRESS_KEY"));
                                qryAddKitItem.SetFieldValue("SEIZED_FOR_BIOLOGY", qryParentItem.FieldByName("SEIZED_FOR_BIOLOGY"));
                            }

                            if (PLCSession.GetLabCtrl("INITIAL_PROCESS_CODE") != "")
                            {
                                qryAddKitItem.SetFieldValue("PROCESS", PLCSession.GetLabCtrl("INITIAL_PROCESS_CODE"));
                                qryAddKitItem.SetFieldValue("PROCESS_DATE", DateTime.Today);
                            }

                            if (PLCSession.GetLabCtrl("RMS_EXTERNAL_IS_VOUCHER") == "T")
                            {
                                qryAddKitItem.SetFieldValue("RMS_EXTERNAL", qryParentItem.FieldByName("RMS_EXTERNAL"));
                            }

                            // For ISP CPD Interface
                            qryAddKitItem.SetFieldValue("ETRACK_INVENTORY_ID", qryParentItem.FieldByName("ETRACK_INVENTORY_ID"));

                            qryAddKitItem.Post("TV_LABITEM", 7, 10);
                            sbEcnList.Append(newECN + ",");
                            sbEcnListForWS.Append("IT:" + newECN + ",");

                            // Set default Process based on Item Type
                            PLCDBGlobal.instance.Set_Default_Process(newECN, "");

                            PLCSession.WriteDebug("Kit Items, ITEM_STAT_DEFAULT=" + PLCSession.GetLabCtrl("ITEM_STAT_DEFAULT"));
                            if (PLCSession.GetLabCtrl("ITEM_STAT_DEFAULT") == "T")
                            {
                                PLCHelperFunctions plcHelp = new PLCHelperFunctions();
                                plcHelp.SetItemDefaultStatus(qryParentItem.FieldByName("CASE_KEY"), newECN, true);
                                PLCDBGlobal.instance.UpdateNextReviewDate(newECN);
                            }

                            //Add record of Kit Item to TV_REVREQHIST
                            if (!string.IsNullOrWhiteSpace(newECN))
                            {
                                PLCQuery qryKitItemProcess = new PLCQuery("SELECT * FROM TV_LABITEM WHERE EVIDENCE_CONTROL_NUMBER = " + newECN);
                                qryKitItemProcess.Open();
                                if (!qryKitItemProcess.IsEmpty())
                                    PLCDBGlobal.instance.AddRevReqHistory(newECN.ToString(), "", qryKitItemProcess.FieldByName("PROCESS"), "KitItem - TAB4Items", PLCSession.PLCGlobalAnalyst, 0);
                            }

                            PLCSession.AddDefaultCustody(PLCSession.PLCGlobalCaseKey, newECN, batchKey, qryParentItem.FieldByName("CONTAINER_KEY"), "KIT");
                            PLCDBGlobal.instance.AppendSubLinkItem(newECN, qryParentItem.FieldByName("LAB_CASE_SUBMISSION"), PLCDBPanel1.getpanelfield("ITEM_TYPE"), false);

                            if (PLCSession.GetLabCtrl("DEFAULT_ASSIGNMENTS") == "T")
                                PLCDBGlobal.instance.AddItemDefaultAssignment(newECN);
                        }
                    }
                }

                if (chkKitPrintLabels.Checked && sbEcnList.Length > 0)
                {
                    if (PLCSession.CheckUserOption("PRINTBAR"))
                    {
                        string ecnList = sbEcnList.ToString().Remove(sbEcnList.Length - 1, 1);
                        string ecnListForWS = sbEcnListForWS.ToString().Remove(sbEcnListForWS.Length - 1, 1);

                        if ((PLCSession.GetLabCtrl("USES_WS_BARCODE_PRINTING") == "T") ||
                            (PLCSession.GetLabCtrl("USES_WS_BARCODE_PRINTING") == "W"))
                        {
                            PLCSession.WriteDebug("Using Web Service to print Item BC List", true);
                            PLCSession.PLCCrystalReportName = string.Empty;
                            PLCSession.SetDefault("BARCODE_KEYLIST", ecnListForWS);
                            Response.Redirect("~/PrintBC_List.aspx");
                        }

                        PLCSession.SetDefault("BARCODE_KEYLIST", ecnListForWS);

                        PLCSession.WriteDebug("Using Crystal Reports print Item BC", true);
                        PLCSession.PLCCrystalReportName = "BCLIST";
                        PLCSession.PLCCrystalReportTitle = "ITEM Barcode Label";
                        PLCSession.PrintCRWReport(true);
                    }
                    else
                    {
                        mbox.ShowMsg("Print Label", "You are not authorized to print barcode labels.", 1);
                    }
                }

                ParentECN = "";
                GridView1.InitializePLCDBGrid();
            }
        }

        protected void OnKitTaskSave_Click(object sender, EventArgs e)
        {
            PLCDBGlobal.instance.RemoveRecordLocks("TV_LABITEM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, "-1", PLCSession.PLCGlobalAnalyst);
            GridView1.InitializePLCDBGrid();
        }

        protected void Requests_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(PLCSession.PLCGlobalECN))
            {
                mbox.ShowMsg("Item", "Please select an Item.", 1);
                return;
            }
            Response.Redirect("ServiceRequest.aspx");
        }

        protected void bnLabel_Click(object sender, EventArgs e)
        {
            if (PLCSession.GetLabCtrl("USES_MULTI_LABEL_PRINT") == "T")
            {
                CheckMultiLabelPrint();
                return;
            }

            if (string.IsNullOrEmpty(PLCSession.PLCGlobalECN))
            {
                mbox.ShowMsg("Item", "Please select an Item.", 1);
                return;
            }

            if (!PLCCommonClass.IsReadOnlyAccess("WEBINQ,RONLYITTAB") && IsSelectedRecordsLocked(null))
            {
                if (ViewState["itemhasbarcode"] == null || !(bool)ViewState["itemhasbarcode"])
                {
                    dlgMessage.ShowAlert("Record Lock", "Item locked by another user for editing.<br/>" + RecordsLockedInfo);
                    return;
                }
            }

            if (PLCSession.CheckUserOption("PRINTBAR"))
            {
                bool hasResponse = false;
                string redirectString = "";
                PLCDBGlobal.instance.PrintLASDLabel(PLCSession.PLCGlobalECN, out hasResponse, out redirectString);
                if (hasResponse)
                    Response.Redirect(redirectString);
                else
                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "windowfocus", "_winPopup.name = 'barcodePopup';", true);

                //Refresh the Clear/NoLabel buttons...
                GrabGridRecord();
            }
            else mbox.ShowMsg("Print Label", "You are not authorized to print barcode labels.", 1);
        }

        protected void bnNoLabel_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(PLCSession.PLCGlobalECN))
            {
                mbox.ShowMsg("Item", "Please select an Item.", 1);
                return;
            }

            if (IsSelectedRecordsLocked(null))
            {
                dlgMessage.ShowAlert("Record Lock", "Item locked by another user for editing.<br/>" + RecordsLockedInfo);
                return;
            }

            if (PLCSession.CheckUserOption("HANDLE"))
            {
                ClearBarcode(PLCSession.PLCGlobalECN);
                bnNoLabel.Enabled = false;
                bnNoLabel.Text = "No Label";

            }
            else
            {
                mbox.ShowMsg("Custody", "You do not have access rights to Handle Evidence", 0);
            }
        }

        protected void bnContainer_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(PLCSession.PLCGlobalECN))
            {
                mbox.ShowMsg("Item", "Please select an Item.", 1);
                return;
            }

            PLCSession.PLCGlobalScannedContainerKey = String.Empty;
            Response.Redirect("Container_Maintainer.aspx");
        }

        protected void bnItemList_Click(object sender, EventArgs e)
        {
            PLCSession.SetProperty<string>("PreviousPage", Page.Request.Url.ToString());
            Response.Redirect("~/RouteItems.aspx");
        }

        protected void bnTransfer_Click(object sender, EventArgs e)
        {
            if (PLCSession.CheckUserOption("HANDLE"))
            {
                string TransferPage = PLCSession.GetWebConfiguration("TRANSFER_PAGE");
                if (!string.IsNullOrEmpty(TransferPage))
                {
                    PLCSession.SetDefault("BC_TRANSFER_CASE_KEY", PLCSession.PLCGlobalCaseKey);
                    Response.Redirect(TransferPage);
                }
                else
                    //Response.Redirect("Transfer.ASPX");
                    Response.Redirect("Transferjs.aspx");
            }
            else
                mbox.ShowMsg("Custody", "You do not have access rights to Handle Evidence", 0);
        }

        protected void bnDelivery_Click(object sender, EventArgs e)
        {
            Response.Redirect("ItemDelivery.ASPX");
        }

        protected void bnItemFind_Click(object sender, EventArgs e)
        {
            ItemSearch1.Show(txtItemFindECN.ClientID, bnItemFindPostback.ClientID);
        }

        protected void bnItemFindPostback_Click(object sender, EventArgs e)
        {
            LoadItem(txtItemFindECN.Text);
        }

        protected void btnCreateSampleContainer_Click(object sender, EventArgs e)
        {
            if (chkPrinLabel.Checked)
                CheckPrintSampleContainerLabel = true;
            else
                CheckPrintSampleContainerLabel = false;

            if (tbxCreateContainerDescription.Text == "")
            {
                lblNewContainerValidation.Visible = true;
                mpeCreateContainer.Show();
                return;
            }

            PLCQuery qryContainer = new PLCQuery();
            qryContainer.SQL = "SELECT * FROM TV_CONTAINER WHERE CASE_KEY = " + PLCSession.PLCGlobalCaseKey +
                " AND UPPER(CONTAINER_DESCRIPTION) = '" + tbxCreateContainerDescription.Text.Trim().ToUpper() + "'";
            qryContainer.Open();
            if (!qryContainer.IsEmpty())
            {
                lblNewContainerValidation.Visible = true;
                mbox.OnOkScript = "javascript:document.getElementById('" + btnShowPopupAgain.ClientID + "').click();";
                mbox.ShowMsg("Container Description", "Container Description '" + Server.HtmlEncode(tbxCreateContainerDescription.Text) + "' already exist.", 0);
                return;
            }

            CreateNewContainer();

            if (CheckPrintSampleContainerLabel)
                PrintNewContainerLabel();
        }

        protected void btnPrintMultiLabel_Click(object sender, EventArgs e)
        {
            PrintMultiLabel();
        }

        protected void btnPrintMultiLabelClose_Click(object sender, EventArgs e)
        {
            GrabGridRecord();
        }

        protected void bnWorksheets_Click(object sender, EventArgs e)
        {
            string clickedECN = this.hdnRVWorksheetsECN.Value;
            this.hdnRVWorksheetsECN.Value = "";
            if (clickedECN == "")
                return;

            PLCQuery qryItem = new PLCQuery("SELECT LAB_ITEM_NUMBER, ITEM_DESCRIPTION FROM TV_LABITEM WHERE EVIDENCE_CONTROL_NUMBER = ?");
            qryItem.AddParameter("EVIDENCE_CONTROL_NUMBER", clickedECN);
            qryItem.Open();
            if (qryItem.HasData())
            {
                this.litRVWorksheetsCaption.Text = String.Format("Worksheets for {0} - {1}",
                    qryItem.FieldByName("LAB_ITEM_NUMBER").Trim(), qryItem.FieldByName("ITEM_DESCRIPTION").Trim());
            }
            else
            {
                this.litRVWorksheetsCaption.Text = "Worksheets";
            }

            string csvRVWorksheetIDs = GetCsvRVWorksheetIDs(clickedECN);

            this.dbgRVWorksheets.PLCGridName = "RV_WORKSHEETS";
            this.dbgRVWorksheets.PLCSQLString_AdditionalCriteria = String.Format(" AND DE.WORKSHEET_ID IN ({0})", csvRVWorksheetIDs);
            this.dbgRVWorksheets.InitializePLCDBGrid();

            this.mpeRVWorksheets.Show();
        }

        protected void btnStatusChange_Click(object sender, EventArgs e)
        {
            lblDNAAcknowledgement.Text = PLCSession.GetLabCtrl("DNA_ACKNOWLEDGEMENT_FORM");
            InitializeStatusChangeGrid();
            InitializeStatusChangePanel();
            ShowStatusChangePopUp.Value = "SHOW";
            SetReadOnlyAccess();
        }

        protected void btnTrackRFID_Click(object sender, EventArgs e)
        {
            ProcessRFIDTracker();
        }

        protected void doStatusChangeOK(object sender, EventArgs e)
        {
            if (IsStatusChangeValid())
            {
                if (SaveStatusChange())
                {
                    InitializeStatusChangeGrid();
                    InitializeStatusChangePanel();
                }
            }
        }

        protected void btnStatusSave_Click(object sender, EventArgs e)
        {
            if (!UnConfirmedItemFound())
                doStatusChangeOK(null, null);
        }

        protected void btnStatusClose_Click(object sender, EventArgs e)
        {
            ShowStatusChangePopUp.Value = "";
            GridView1.InitializePLCDBGrid();

            // need to handle when selected item in GridView1 was deleted on status change popup close
            if (GridView1.SelectedDataKey == null && GridView1.Rows.Count > 0)
                GridView1.SelectedIndex = 0;

            // need to refresh record lock feature
            GrabGridRecord();
        }

        protected void btnCancelStatusChange_Click(object sender, EventArgs e)
        {
            if (!IsCancelStatusChangeValid()) return;

            if (CancelStatusChange())
            {
                InitializeStatusChangeGrid();
                InitializeStatusChangePanel();
            }
        }

        protected void btnShowBulkPopup_Click(object sender, EventArgs e)
        {
            if (GridView1.Rows.Count > 0)
            {
                LoadBulkDeletePopup();
                ScriptManager.RegisterStartupScript(btnShowBulkPopup, btnShowBulkPopup.GetType(), "Popup", "BDialogOpen(true);", true);
            }
        }

        protected void btnBulkDelete_Click(object sender, EventArgs e)
        {
            BulkDeleteItems();
        }

        protected void btnDeleteBulk_Click(object sender, EventArgs e)
        {
            DeleteBulk();
        }

        protected void btnCloseDeleteBulk_Click(object sender, EventArgs e)
        {
            GrabGridRecord();
        }

        protected void btnCancelDeleteBulk_Click(object sender, EventArgs e)
        {
            GrabGridRecord();
            BulkDeleteLockRecords();
        }

        protected void bnWorkProduct_Click(object sender, EventArgs e)
        {
            if (hdnRVWorksheetsECN.Value != "")
            {
                dbgWorkProduct.PLCSQLString = string.Format("SELECT \"Lab Case\", \"Lab Item Number\", \"Item Description\", \"Custody Of - Location\" " +
                    "FROM CV_WORKPRODUCT WHERE EVIDENCE_CONTROL_NUMBER = {0}", hdnRVWorksheetsECN.Value);
                dbgWorkProduct.InitializePLCDBGrid();
                mpeWorkProduct.Show();
            }
        }

        protected void btnPrintEvidenceRpt_Click(object sender, EventArgs e)
        {
            string ecnListStr = hdnECNList.Value.Trim();
            // early return when no ecn list
            if (string.IsNullOrEmpty(ecnListStr))
                return;

            string reportName = hdnEvidenceReport.Value;
            bool usesTempList = false;
            string selectionFormula = "";
            bool requireSignature = false;
            string reportDescription = "";

            var qry = new PLCQuery(string.Format("SELECT USES_TEMPLIST, REQUIRE_SIGNATURE, REPORT_DESCRIPTION FROM TV_RPTFORMT WHERE REPORT_ID = 'EPR' AND REPORT_NAME='{0}'", reportName));
            if (qry.Open() && qry.HasData())
            {
                usesTempList = qry.FieldByName("USES_TEMPLIST") == "T";
                requireSignature = qry.FieldByName("REQUIRE_SIGNATURE") == "T";
                reportDescription = qry.FieldByName("REPORT_DESCRIPTION");
            }

            int batchKey = 0;
            if (usesTempList)
            {
                if (requireSignature)
                {
                    batchKey = EvidenceReportBatchSequence;
                    if (batchKey == 0)
                    {
                        ShowSignatureCapture();
                        return;
                    }
                    EvidenceReportBatchSequence = 0;
                }
                else
                {
                    batchKey = PLCSession.GetNextSequence("BATCH_SEQ");
                }

                int displayOrder = 0;
                string[] ecnList = ecnListStr.Split(',');

                foreach (string ecn in ecnList)
                {
                    displayOrder++;
                    var qryItemList = new PLCQuery("SELECT * FROM TV_TEMPLIST WHERE 0 = 1");
                    qryItemList.Open();
                    qryItemList.Append();
                    qryItemList.SetFieldValue("BATCH_SEQUENCE", batchKey);
                    qryItemList.SetFieldValue("SEQUENCE", displayOrder);
                    qryItemList.SetFieldValue("EVIDENCE_CONTROL_NUMBER", ecn);
                    qryItemList.SetFieldValue("ITEM_LINKED", "F");
                    qryItemList.Post("TV_TEMPLIST", 3000, 18);

                    if (requireSignature)
                    {
                        AddEvidenceReportItem(batchKey, ecn);
                    }
                }

                selectionFormula = "{TV_TEMPLIST.BATCH_SEQUENCE} = " + batchKey;
            }
            else
            {
                selectionFormula = "{TV_LABITEM.EVIDENCE_CONTROL_NUMBER} IN [" + ecnListStr + "]";
            }

            PLCSession.PLCCrystalReportName = PLCSession.FindCrystalReport(reportName + ".RPT");
            PLCSession.PLCCrystalSelectionFormula = selectionFormula;
            PLCSession.PLCCrystalReportTitle = ddlEvidenceReport.SelectedValue == "Evidence Page" ? "Receipt # " + PLCSession.PLCGlobalSubmissionNumber : ddlEvidenceReport.SelectedItem.Text;
            PLCSession.PrintCRWReport(false);

            if (usesTempList && requireSignature)
            {
                int printlogKey = SaveEvidenceReportAsCaseAttachment(reportDescription);
                LinkPrintLogToEvidenceReport(printlogKey, batchKey);
            }
        }

        protected void btnSupp49_Click(object sender, EventArgs e)
        {
            hdnEvidenceReport.Value = ddlEvidenceReport.SelectedValue;
            dbp49Supp.ClearFields();
            mpe49Supp.Show();
        }

        protected void dbpEvidReportSignature_PLCDBPanelGetNewRecord(object sender, PLCDBPanelGetNewRecordEventArgs e)
        {
            int batchSeq = EvidenceReportBatchSequence = PLCSession.GetNextSequence("BATCH_SEQ");
            e.NewRecordValues.Add("CASE_KEY", PLCSession.PLCGlobalCaseKey);
            e.NewRecordValues.Add("BATCH_SEQUENCE", batchSeq);
            e.NewRecordValues.Add("REPORT_DATE", DateTime.Now);
            e.NewRecordValues.Add("REPORT_BY", PLCSession.PLCGlobalAnalyst);
            e.NewRecordValues.Add("REPORT_ID", ReportFormatID);
            e.NewRecordValues.Add("REPORT_FORMAT_NUMBER", ReportFormatNumber);

            e.newWhereClause = " WHERE BATCH_SEQUENCE = " + batchSeq;
        }

        protected void btnSaveEvidReportSignature_Click(object sender, EventArgs e)
        {
            if (dbpEvidReportSignature.CanSave() && dbpEvidReportSignature2.CanSave())
            {
                dbpEvidReportSignature.DoSave();
                dbpEvidReportSignature2.PLCWhereClause = "WHERE BATCH_SEQUENCE = " + EvidenceReportBatchSequence;
                dbpEvidReportSignature2.DoSave();
                btnPrintEvidenceRpt_Click(sender, e);
                CloseSignatureCapture();
            }
        }

        protected void btnCancelEvidReportSignature_Click(object sender, EventArgs e)
        {
            EvidenceReportBatchSequence = 0;
        }

        protected void mbClearConfirm_OkClick(object sender, EventArgs e)
        {
            ClearBarcodeContainer();
        }

        protected void mbClearConfirm_CancelClick(object sender, EventArgs e)
        {
            GrabGridRecord(); // leave clear button enabled
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

        protected void MsgCommentPostBackButton_Click(object sender, EventArgs e)
        {
            if (UserComments.Text != "")
            {
                PLCDBGlobal.instance.LockUnlockRecord("TV_LABITEM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, "-1", false);
                SetControlsForRecordsLocked(false, 0);
            }
        }

        protected void mbConfirm_OkClick(object sender, EventArgs e)
        {
            //set flag to true to delete previously saved attributes on savebutton click
            ItemTypeChangeConfirmed = true;

            //regardless of changed itemtype (currency to non-currency, non-currency to currency, attribute to a different attribute),
            //currency tab should be just "Currency" (without the $ value)
            SetCurrencyTabText("");

            //show new attributes based on new itemtype
            RecreateBlankAttributes(true);
        }

        protected void mbConfirm_CancelClick(object sender, EventArgs e)
        {
            //attributes panel is unchanged
            //revert back itemtype
            PLCDBPanel1.setpanelfield("ITEM_TYPE", PLCDBPanel1.GetOriginalValue("ITEM_TYPE"));
        }

        protected void mbConfirmDeleteAttr_OkClick(object sender, EventArgs e)
        {
            ItemAttribute.UpdateMaxExhibitNumber(PLCSession.SafeInt(PLCDBPanel1.getpanelfield("QUANTITY"), 1));
            ItemAttribute.LoadAttributeViaExhibitNumber(ItemAttribute.ExhibitNumber > ItemAttribute.MaxExhibitNumber ? 1 : ItemAttribute.ExhibitNumber);
            ShowAttributesTab();
        }

        protected void mbConfirmDeleteAttr_CancelClick(object sender, EventArgs e)
        {
            PLCDBPanel1.setpanelfield("QUANTITY", ItemAttribute.MaxExhibitNumber.ToString());
            ItemAttribute.LoadAttributeViaExhibitNumber(ItemAttribute.ExhibitNumber);
            ShowAttributesTab();
        }

        protected void mbSampleContainerConfirm_OkClick(object sender, EventArgs e)
        {
            tbxCreateContainerDescription.Text = "";
            tbxCreateSource.Text = "";
            tbxCreateContainerDescription.Focus();
            mpeCreateContainer.OnOkScript = "javascript:document.getElementById('" + btnCreateSampleContainer.ClientID + "').click();";
            CheckPrintSampleContainerLabel = true;
            lblNewContainerValidation.Visible = false;
            mpeCreateContainer.Show();
        }

        protected void mbSampleContainerConfirm_CancelClick(object sender, EventArgs e)
        {
            // do nothing
        }

        protected void btnConfirmDelete_Click(object sender, EventArgs e)
        {
            if (txtConfirmUpdate.Text.Trim().Length > 0)
            {
                hdnConfirmUpdate.Value = txtConfirmUpdate.Text;

                if (ViewState["IsBulkDelete"] != null && Convert.ToBoolean(ViewState["IsBulkDelete"]))
                    DeleteBulk();
                else
                    mbDeleteItemConfirm_OkClick(null, null);
            }
        }

        protected void btnCustodyChangeYes_Click(object sender, EventArgs e)
        {
            ViewState["SAMPLE_ITEM_NEW_CONTAINER"] = true;
            ParentECN = PLCSession.PLCGlobalECN;
            PLCButtonPanel1.ClickAddButton();
            ParentECN = "";
        }

        protected void mbDeleteItemConfirm_OkClick(object sender, EventArgs e)
        {
            string deleteReason = string.Empty;

            PLCQuery qryApprovedAssignment = new PLCQuery();
            qryApprovedAssignment.SQL = "SELECT * FROM TV_LABREPT A INNER JOIN TV_ITASSIGN B ON A.EXAM_KEY = B.EXAM_KEY WHERE A.COMPLETED = 'T' AND B.EVIDENCE_CONTROL_NUMBER = " + PLCSession.PLCGlobalECN;
            qryApprovedAssignment.Open();

            if (qryApprovedAssignment.IsEmpty())
            {
                PLCQuery qryReceivedSR = new PLCQuery(@"SELECT * FROM TV_SRMASTER M INNER JOIN TV_SRDETAIL D ON D.SR_MASTER_KEY = M.SR_MASTER_KEY
                    WHERE (M.LAB_CASE IS NOT NULL OR M.LAB_CASE != '') AND D.EVIDENCE_CONTROL_NUMBER = " + PLCSession.PLCGlobalECN);
                qryReceivedSR.Open();
                if (qryReceivedSR.HasData())
                {
                    PLCDBGlobal.instance.RemoveRecordLocks("TV_LABITEM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, "-1", PLCSession.PLCGlobalAnalyst);
                    mbox.ShowMsg("Delete Item Error", "The selected item is part of a submission received by the lab which cannot be deleted.", 1);
                    return;
                }

                if (PLCSession.GetLabCtrl("LOG_EDITS_TO_NARRATIVE") == "T")
                {
                    if (hdnConfirmUpdate.Value.Trim().Length > 0)
                    {
                        StringBuilder auditText = new StringBuilder();
                        auditText.AppendLine("ITEM DELETED");
                        auditText.AppendLine(string.Format("ITEM #: {0} - ECN: {1}", PLCDBPanel1.getpanelfield("LAB_ITEM_NUMBER"), PLCSession.PLCGlobalECN));
                        SaveConfirmUpdate(auditText.ToString());
                        deleteReason = hdnConfirmUpdate.Value;
                        hdnConfirmUpdate.Value = "";
                    }
                    else
                    {
                        PLCDBGlobal.instance.RemoveRecordLocks("TV_LABITEM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, "-1", PLCSession.PLCGlobalAnalyst);
                        mInput.ShowMsg("Case update reason", "Please enter the reason for your changes", 0, txtConfirmUpdate.ClientID, btnConfirmDelete.ClientID, "Save", "Cancel");
                        return;
                    }
                }

                GetAttachEntryIDs(new List<string>() { PLCSession.PLCGlobalECN });

                if (PLCSession.PLCDatabaseServer == "MSSQL")
                {
                    PLCQuery qryDeleteCase = new PLCQuery();
                    qryDeleteCase.AddProcedureParameter("caseKey", Convert.ToInt32(PLCSession.PLCGlobalCaseKey), 10, OleDbType.Integer, ParameterDirection.Input);
                    qryDeleteCase.AddProcedureParameter("deleteReason", deleteReason, deleteReason.Length, OleDbType.VarChar, ParameterDirection.Input);
                    qryDeleteCase.AddProcedureParameter("userId", PLCSession.PLCGlobalAnalyst, 15, OleDbType.VarChar, ParameterDirection.Input);
                    qryDeleteCase.AddProcedureParameter("program", "iLIMS" + PLCSession.PLCBEASTiLIMSVersion, 8, OleDbType.VarChar, ParameterDirection.Input);
                    qryDeleteCase.AddProcedureParameter("OSComputerName", PLCSession.GetOSComputerName(), 50, OleDbType.VarChar, ParameterDirection.Input);
                    qryDeleteCase.AddProcedureParameter("OSUserName", PLCSession.GetOSUserName(), 50, OleDbType.VarChar, ParameterDirection.Input);
                    qryDeleteCase.AddProcedureParameter("OSAddress", PLCSession.GetOSAddress(), 50, OleDbType.VarChar, ParameterDirection.Input);
                    qryDeleteCase.AddProcedureParameter("BuildNumber", PLCSession.PLCBEASTiLIMSVersion, 100, OleDbType.VarChar, ParameterDirection.Input);
                    qryDeleteCase.AddProcedureParameter("labCode", PLCSession.PLCGlobalLabCode, 3, OleDbType.VarChar, ParameterDirection.Input);
                    qryDeleteCase.AddProcedureParameter("ecn", Convert.ToInt32(PLCSession.PLCGlobalECN), 10, OleDbType.Integer, ParameterDirection.Input);
                    qryDeleteCase.ExecuteProcedure("DELETE_LABITEM");
                }
                else
                {
                    PLCQuery qryDeleteCase = new PLCQuery();
                    List<OracleParameter> parameters = new List<OracleParameter>();
                    parameters.Add(new OracleParameter("caseKey", Convert.ToInt32(PLCSession.PLCGlobalCaseKey)));
                    parameters.Add(new OracleParameter("deleteReason", deleteReason));
                    parameters.Add(new OracleParameter("userId", PLCSession.PLCGlobalAnalyst));
                    parameters.Add(new OracleParameter("program", ("iLIMS" + PLCSession.PLCBEASTiLIMSVersion).Substring(0, 8)));
                    parameters.Add(new OracleParameter("OSComputerName", PLCSession.GetOSComputerName()));
                    parameters.Add(new OracleParameter("OSUserName", PLCSession.GetOSUserName()));
                    parameters.Add(new OracleParameter("OSAddress", PLCSession.GetOSAddress()));
                    parameters.Add(new OracleParameter("BuildNumber", PLCSession.PLCBEASTiLIMSVersion));
                    parameters.Add(new OracleParameter("labCode", PLCSession.PLCGlobalLabCode));
                    parameters.Add(new OracleParameter("ecn", PLCSession.PLCGlobalECN));
                    qryDeleteCase.ExecuteOracleProcedure("DELETE_LABITEM", parameters);
                }

            }
            else
            {
                mbox.ShowMsg("Delete Item Error", "The selected item is part of an approved assignment which cannot be deleted.", 1);
            }

            PLCCONTROLS.PLCButtonClickEventArgs btnClick = new PLCCONTROLS.PLCButtonClickEventArgs("DELETE", "AFTER", false);
            PLCButtonPanel1_PLCButtonClick(PLCButtonPanel1, btnClick);
        }

        protected void mbDeleteItemConfirm_CancelClick(object sender, EventArgs e)
        {
            PLCDBGlobal.instance.RemoveRecordLocks("TV_LABITEM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, "-1", PLCSession.PLCGlobalAnalyst);
        }

        protected void GridView1_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                GetSubmExtraInfoHighlight(e);
            }
        }

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                foreach (TableCell cell in e.Row.Cells)
                    if (cell.Controls.Count > 0 && cell.Controls[0].GetType().ToString() == "System.Web.UI.WebControls.DataControlLinkButton" && ((LinkButton)cell.Controls[0]).Text == "Task")
                        if (!PLCSession.CheckUserOption("TASKITEM"))
                            cell.Width = 0;
            }

            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                foreach (TableCell cell in e.Row.Cells)
                    if (cell.Controls.Count > 1 && cell.Controls[1].GetType().ToString() == "System.Web.UI.WebControls.DataControlLinkButton" && ((LinkButton)cell.Controls[1]).Text == "Add Task")
                        if (PLCSession.CheckUserOption("TASKITEM"))
                            ((LinkButton)cell.Controls[1]).CommandArgument = e.Row.DataItemIndex.ToString();
                        else
                            cell.Width = 0;
            }
        }

        protected void GridView1_Sorted(object sender, EventArgs e)
        {
            if (GridView1.SelectedIndex != 0)
            {
                GridView1.SelectedIndex = 0;
                GridView1.SetClientSideScrollToSelectedRow();
            }

            PLCSession.PLCGlobalECN = GridView1.SelectedDataKey.Value.ToString();
            UpdateCustodyStatusDisplay(PLCSession.PLCGlobalECN);
            GrabGridRecord();
            ShowDefaultTab();

            UnLockKit();
            CheckUserOptionReadOnlyAccess();

            PDCheckItemReceivedByLab();
        }

        protected void GridView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            PLCSession.PLCGlobalECN = GridView1.SelectedDataKey.Value.ToString();
            UpdateCustodyStatusDisplay(PLCSession.PLCGlobalECN);
            GrabGridRecord();
            ShowDefaultTab();

            UnLockKit();
            CheckUserOptionReadOnlyAccess();

            PDCheckItemReceivedByLab();
        }

        protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ADDTASK")
            {
                int index = Convert.ToInt32(e.CommandArgument);
                ScriptManager.RegisterStartupScript(this, GetType(), "ucItemTask", "uc_LoadItemTasks(" + GridView1.DataKeys[index].Value.ToString() + ");", true);
            }
        }

        protected void gvNameItem_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                TableHeaderCell tcCheckCell = new TableHeaderCell();
                CheckBox chkCheckBox = new CheckBox();
                chkCheckBox.ID = "chkNameItem_All";
                chkCheckBox.Attributes.Add("onclick", "selectHeader(this,'" + chkCheckBox.ID + "');");
                tcCheckCell.Controls.Add(chkCheckBox);
                e.Row.Cells.AddAt(NAMEITEM_GRID_COL_CHECK, tcCheckCell);

                TableHeaderCell tcDDL = new TableHeaderCell();
                Label lbl = new Label();
                lbl.Font.Bold = true;
                lbl.Text = "Relation";
                tcDDL.Controls.Add(lbl);
                tcDDL.Attributes.Add("style", "vertical-align:middle;");
                e.Row.Cells.AddAt(NAMEITEM_GRID_COL_DDL, tcDDL);

                GetRelationItems();

                // hide ecn, count, relation code
                e.Row.Cells[NAMEITEM_GRID_COL_NAMEKEY].Visible = false;
                e.Row.Cells[NAMEITEM_GRID_COL_COUNT].Visible = false;
                e.Row.Cells[NAMEITEM_GRID_COL_RELCODE].Visible = false;

                e.Row.Cells[NAMEITEM_GRID_COL_DESC].Width = 350;
                e.Row.Cells[NAMEITEM_GRID_COL_DESC].Attributes.Add("style", "vertical-align:middle;");
            }
            else if (e.Row.RowType == DataControlRowType.DataRow)
            {
                ArrayList ArrayITNAMERL = (ArrayList)ViewState["ITEM_NAME_RELATION"];

                // checkbox
                TableCell tcCheckCell = new TableCell();
                CheckBox chkCheckBox = new CheckBox();
                chkCheckBox.ID = "chkNameItem";
                chkCheckBox.Attributes.Add("onclick", (gvNameItem.AllowPaging ? "" : "selectDetail(this,'" + chkCheckBox.ID + "');") + "window.event.cancelBubble = true;");
                tcCheckCell.Controls.Add(chkCheckBox);
                e.Row.Cells.AddAt(NAMEITEM_GRID_COL_CHECK, tcCheckCell);

                // dropdownlist
                TableCell tcDDL = new TableCell();
                DropDownList ddlRelation = new DropDownList();
                ddlRelation.ID = "ddlRelation" + e.Row.RowIndex;
                for (int i = 0; i < ArrayITNAMERL.Count; i++)
                {
                    ddlRelation.Items.Add(((TRelation)ArrayITNAMERL[i]).Description);
                }
                ddlRelation.SelectedIndex = -1;
                tcDDL.Controls.Add(ddlRelation);
                e.Row.Cells.Add(tcDDL);

                // hide ecn, count, relation code
                e.Row.Cells[NAMEITEM_GRID_COL_NAMEKEY].Visible = false;
                e.Row.Cells[NAMEITEM_GRID_COL_COUNT].Visible = false;
                e.Row.Cells[NAMEITEM_GRID_COL_RELCODE].Visible = false;
                e.Row.Cells[NAMEITEM_GRID_COL_DESC].Width = 350;
            }
        }

        protected void gvNameItem_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (Convert.ToInt32(e.Row.Cells[NAMEITEM_GRID_COL_COUNT].Text) > 0)
                {
                    CheckBox chk = (CheckBox)e.Row.FindControl("chkNameItem");
                    if (chk != null)
                        chk.Checked = true;
                }

                // select relation ddl
                DropDownList ddl = (DropDownList)e.Row.FindControl("ddlRelation" + e.Row.RowIndex);
                string sCode = e.Row.Cells[NAMEITEM_GRID_COL_RELCODE].Text;
                ddl.SelectedIndex = GetRelationIndex(sCode);
            }
        }

        protected void gvMultiPrintLabel_DataBinding(object sender, EventArgs e)
        {
            if (!PLCCommonClass.IsReadOnlyAccess("WEBINQ,RONLYITTAB"))
            {
                UpdateRecordsLocked(1);
                // override lblNoticeMultiPrint.Text
                lblNoticeMultiPrint.Text = string.Empty;
            }
        }

        protected void gvMultiPrintLabel_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                CheckBox cbSelect = (CheckBox)e.Row.FindControl("chkItem");
                CheckBox cbSelect_All = (CheckBox)e.Row.FindControl("chkAll");
                string ecn = gvMultiPrintLabel.DataKeys[e.Row.RowIndex].Values["ECN"].ToString();

                if (PLCCommonClass.IsReadOnlyAccess("WEBINQ,RONLYITTAB"))
                {
                    if (string.IsNullOrEmpty(gvMultiPrintLabel.DataKeys[e.Row.RowIndex]["BARCODE"].ToString()))
                        cbSelect.Enabled = false;
                }
                else if (RecordsLocked.Contains(ecn))
                {
                    if (string.IsNullOrEmpty(gvMultiPrintLabel.DataKeys[e.Row.RowIndex]["BARCODE"].ToString()))
                    {
                        cbSelect.Enabled = false;
                        // display notice if user option WEBINQ is off and records without barcodes are locked
                        lblNoticeMultiPrint.Text = "Disabled item(s) are locked by another user for editing";
                    }
                }
            }
        }

        protected void gvStatusChange_DataBinding(object sender, EventArgs e)
        {
            if (!PLCCommonClass.IsReadOnlyAccess("WEBINQ,RONLYITTAB"))
                UpdateRecordsLocked(1);
        }

        protected void gvStatusChange_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                String ecn = gvStatusChange.DataKeys[e.Row.RowIndex][0].ToString();
                String process = ((HiddenField)e.Row.FindControl("hfProcess")).Value;
                String reviewStatus = ((HiddenField)e.Row.FindControl("hfReviewStatusID")).Value;
                String seizedForBiology = ((HiddenField)e.Row.FindControl("hfSeizedForBiology")).Value;
                String finalReviewBy = ((HiddenField)e.Row.FindControl("hfFinalReviewBy")).Value;

                LinkButton lnkSelect = (LinkButton)e.Row.FindControl("lnkSelect");
                CheckBox chkSelect = (CheckBox)e.Row.FindControl("chkSelect");
                CheckBox chkSelect_All = (CheckBox)gvStatusChange.HeaderRow.FindControl("chkSelect_All");
                CheckBox chkClearance = (CheckBox)e.Row.FindControl("chkClearance");

                chkClearance.Enabled = seizedForBiology == "Y";

                lnkSelect.ForeColor = seizedForBiology == "Y" ? System.Drawing.Color.Red : System.Drawing.Color.Black;
                e.Row.ForeColor = seizedForBiology == "Y" ? System.Drawing.Color.Red : System.Drawing.Color.Black;

                if (RecordsLocked.Contains(ecn))
                {
                    chkSelect.Enabled = false;
                    chkSelect_All.Enabled = false;
                }
                else
                    chkSelect.Enabled = true;
            }
        }

        protected void gvStatusChange_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Select")
                InitializeStatusChangePanel(e.CommandArgument.ToString());
        }

        protected void gvBulkDelItems_DataBinding(object sender, EventArgs e)
        {
            UpdateRecordsLocked(1);
        }

        protected void gvBulkDelItems_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                CheckBox cbSelect_All = (CheckBox)gvBulkDelItems.HeaderRow.FindControl("cbSelect_All");
                CheckBox cbSelect = (CheckBox)e.Row.FindControl("cbSelect");
                string ecn = gvBulkDelItems.DataKeys[e.Row.RowIndex].Values["ECN"].ToString();

                if (RecordsLocked.Contains(ecn))
                {
                    cbSelect.Enabled = false;
                    cbSelect_All.Enabled = false;
                }
            }
        }

        protected void dbgWorkProduct_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header || e.Row.RowType == DataControlRowType.DataRow)
            {
                e.Row.Cells[1].Visible = false;
            }
        }

        protected void PLCDBPanel1_SetDefaultRecord(object sender, PLCDBPanelSetDefaultRecordEventArgs e)
        {
            string submissionNumber = PLCSession.PLCGlobalSubmissionNumber = "";
            string submissionURN = "";

            //Dictionary for DUPED item
            Dictionary<string, string> DictItemDefaultValues = new Dictionary<string, string>();

            //Dictionary for Sampled item (Booked and collected settings)
            Dictionary<string, string> dictItemBCDefaultSettings = new Dictionary<string, string>();

            PLCQuery qrySubmission = new PLCQuery("SELECT SUBMISSION_NUMBER, DEPARTMENT_CASE_NUMBER FROM TV_LABSUB WHERE CASE_KEY = " + PLCSession.PLCGlobalCaseKey + " ORDER BY SUBMISSION_NUMBER DESC");
            qrySubmission.Open();
            if (!qrySubmission.IsEmpty())
            {
                submissionNumber = PLCSession.PLCGlobalSubmissionNumber = qrySubmission.FieldByName("SUBMISSION_NUMBER");
                submissionURN = qrySubmission.FieldByName("DEPARTMENT_CASE_NUMBER");
            }
            else
            {
                return;
            }

            string itemNumber = PLCDBGlobal.instance.GetNextItemNumber();

            e.SetNewRecordValues("CASE_KEY", PLCSession.PLCGlobalCaseKey);
            e.SetNewRecordValues("LAB_ITEM_NUMBER", itemNumber);
            e.SetNewRecordValues("ITEM_SORT", PLCCommon.instance.GetItemSort(itemNumber));
            e.SetNewRecordValues("LAB_CASE_SUBMISSION", submissionNumber);
            e.SetNewRecordValues("QUANTITY", 1);
            ItemAttribute.InitializeMultiEntries(1);
            e.SetNewRecordValues("ENTRY_ANALYST", PLCSession.PLCGlobalAnalyst);
            e.SetNewRecordValues("ENTRY_TIME_STAMP", System.DateTime.Now);

            if (CurrentECN != "") //Dupe Item
            {
                DictItemDefaultValues = PLCDBGlobal.instance.GetItemDupeDefaultValues(CurrentECN, PLCDBPanel1.GetPanelRecByFieldName("ITEM_TYPE").codecondition);
                if (DictItemDefaultValues.Count > 0)
                {
                    foreach (KeyValuePair<string, string> item in DictItemDefaultValues)
                    {
                        e.SetNewRecordValues(item.Key, item.Value);
                    }
                }
            }

            if (ParentECN != "") //Sample Item
            {
                PLCQuery qryParentItem = new PLCQuery();
                qryParentItem.SQL = "Select * from TV_LABITEM WHERE EVIDENCE_CONTROL_NUMBER = " + ParentECN;
                qryParentItem.Open();
                if (!qryParentItem.EOF())
                {
                    string newpackcode = qryParentItem.FieldByName("PACKAGING_CODE");
                    string newinpack = qryParentItem.FieldByName("INNER_PACK");
                    string newqty = qryParentItem.FieldByName("QUANTITY");
                    string newlabcasesub = qryParentItem.FieldByName("LAB_CASE_SUBMISSION");
                    string newitemtype = qryParentItem.FieldByName("ITEM_TYPE");
                    string newitemdesc = qryParentItem.FieldByName("ITEM_DESCRIPTION");
                    string newcontkey = qryParentItem.FieldByName("CONTAINER_KEY");
                    string newcasekey = qryParentItem.FieldByName("CASE_KEY");
                    string newparentcustody = qryParentItem.FieldByName("CUSTODY_OF");
                    string newparentloc = qryParentItem.FieldByName("LOCATION");
                    string parentitemnumber = qryParentItem.FieldByName("LAB_ITEM_NUMBER");
                    string newitemcategory = qryParentItem.FieldByName("ITEM_CATEGORY");
                    string newcollectedby = qryParentItem.FieldByName("COLLECTED_BY");
                    string newdatecollected = qryParentItem.FieldByName("DATE_COLLECTED");
                    string newtimecollected = qryParentItem.FieldByName("TIME_COLLECTED");
                    string newbookedby = qryParentItem.FieldByName("BOOKED_BY");
                    string newbookingdate = qryParentItem.FieldByName("BOOKING_DATE");
                    string newbookingtime = qryParentItem.FieldByName("BOOKING_TIME");
                    string newrecoverloc = qryParentItem.FieldByName("RECOVERY_LOCATION");
                    string newrecoverlockey = qryParentItem.FieldByName("RECOVERY_ADDRESS_KEY");
                    string newProcessReviewDate = qryParentItem.FieldByName("PROCESS_REVIEW_DATE");
                    string newProcessDate = qryParentItem.FieldByName("PROCESS_DATE");
                    string newProcess = qryParentItem.FieldByName("PROCESS");
                    string newSeizedDNA = qryParentItem.FieldByName("SEIZED_FOR_BIOLOGY");
                    string newInventoryID = qryParentItem.FieldByName("ETRACK_INVENTORY_ID");

                    itemNumber = PLCDBGlobal.instance.GetSampleItemNumber(qryParentItem.FieldByName("CASE_KEY"), qryParentItem.FieldByName("LAB_ITEM_NUMBER"), newlabcasesub);

                    //check if item type is valid
                    if (!PLCDBGlobal.instance.IsItemTypeValidByCodeCondition(newitemtype, newitemcategory, PLCDBPanel1.GetPanelRecByFieldName("ITEM_TYPE").codecondition))
                        newitemtype = "";

                    e.SetNewRecordValues("LAB_ITEM_NUMBER", itemNumber);

                    e.SetNewRecordValues("QUANTITY", (PLCDBGlobal.instance.ThisFieldIsSampled("QUANTITY")) ? newqty : "1");
                    e.SetNewRecordValues("PACKAGING_CODE", (PLCDBGlobal.instance.ThisFieldIsSampled("PACKAGING_CODE")) ? newpackcode : "");
                    e.SetNewRecordValues("ITEM_TYPE", (PLCDBGlobal.instance.ThisFieldIsSampled("ITEM_TYPE")) ? newitemtype : "");
                    e.SetNewRecordValues("ITEM_DESCRIPTION", (PLCDBGlobal.instance.ThisFieldIsSampled("ITEM_DESCRIPTION")) ? newitemdesc : "");
                    e.SetNewRecordValues("ITEM_CATEGORY", (PLCDBGlobal.instance.ThisFieldIsSampled("ITEM_CATEGORY")) ? newitemcategory : "");
                    e.SetNewRecordValues("LAB_CASE_SUBMISSION", (PLCDBGlobal.instance.ThisFieldIsSampled("LAB_CASE_SUBMISSION")) ? newlabcasesub : submissionNumber);
                    e.SetNewRecordValues("INNER_PACK", (PLCDBGlobal.instance.ThisFieldIsSampled("INNER_PACK")) ? newinpack : "");
                    e.SetNewRecordValues("PARENT_ECN", ParentECN);
                    // For ISP CPD Interface
                    e.SetNewRecordValues("ETRACK_INVENTORY_ID", newInventoryID);                    

                    if (newProcess == "HOLD")
                    {
                        if (PLCDBGlobal.instance.ThisFieldIsSampled("PROCESS_REVIEW_DATE"))
                            e.SetNewRecordValues("PROCESS_REVIEW_DATE", newProcessReviewDate);

                        if (PLCDBGlobal.instance.ThisFieldIsSampled("PROCESS_DATE"))
                            e.SetNewRecordValues("PROCESS_DATE", newProcessDate);

                        if (PLCDBGlobal.instance.ThisFieldIsSampled("PROCESS"))
                            e.SetNewRecordValues("PROCESS", newProcess);
                    }
                    else
                    {
                        e.SetNewRecordValues("PROCESS_REVIEW_DATE", "");
                        e.SetNewRecordValues("PROCESS_DATE", "");
                        e.SetNewRecordValues("PROCESS", "");
                    }

                    var isCaseFromCPD = PLCDBGlobal.instance.GetTableDetailsByCustomKey("TV_LABCASE", "CASE_FROM_CPD", "CASE_KEY", PLCSession.PLCGlobalCaseKey) == "T";

                    if (isCaseFromCPD)
                        e.SetNewRecordValues("DEPARTMENT_ITEM_NUMBER", "");
                    else if (PLCSession.GetLabCtrl("DUPE_DEPT_ITEM_NUM_ON_SAMPLE") == "T" && !String.IsNullOrEmpty(qryParentItem.FieldByName("DEPARTMENT_ITEM_NUMBER")))
                    {
                        string nextDeptItem = PLCDBGlobal.instance.GetSampleDeptItemNumber(qryParentItem.FieldByName("CASE_KEY"), qryParentItem.FieldByName("DEPARTMENT_ITEM_NUMBER"), newlabcasesub);
                        e.SetNewRecordValues("DEPARTMENT_ITEM_NUMBER", nextDeptItem);
                    }
                    else if (PLCSession.GetLabCtrl("DUPE_DEPARTMENT_ITEM") == "T" || (PLCDBGlobal.instance.ThisFieldIsSampled("DEPARTMENT_ITEM_NUMBER")))
                        e.SetNewRecordValues("DEPARTMENT_ITEM_NUMBER", qryParentItem.FieldByName("DEPARTMENT_ITEM_NUMBER"));
                    else
                        e.SetNewRecordValues("DEPARTMENT_ITEM_NUMBER", "");

                    dictItemBCDefaultSettings = PLCSession.GetItemBCDefaultSettings("SAMPLE");
                    foreach (KeyValuePair<String, String> pair in dictItemBCDefaultSettings)
                    {
                        if (pair.Value == "PARENT")
                            e.SetNewRecordValues(pair.Key, qryParentItem.FieldByName(pair.Key));
                        else if (pair.Value == "ANALYST")
                        {
                            if (pair.Key == "COLLECTED_BY" || pair.Key == "BOOKED_BY")
                                e.SetNewRecordValues(pair.Key, PLCSession.PLCGlobalAnalyst);
                        }
                        else if (pair.Value == "CURRENT")
                        {
                            if (pair.Key == "DATE_COLLECTED" || pair.Key == "BOOKING_DATE")
                                e.SetNewRecordValues(pair.Key, DateTime.Now.ToString("MM/dd/yyyy"));
                            else if (pair.Key == "TIME_COLLECTED" || pair.Key == "BOOKING_TIME")
                                e.SetNewRecordValues(pair.Key, DateTime.Now.ToString("HH:mm"));

                        }
                        else
                        {
                            if (pair.Key == "COLLECTED_BY" && PLCDBGlobal.instance.ThisFieldIsSampled("COLLECTED_BY"))
                                e.SetNewRecordValues(pair.Key, newcollectedby);
                            else if (pair.Key == "DATE_COLLECTED" && PLCDBGlobal.instance.ThisFieldIsSampled("DATE_COLLECTED"))
                                e.SetNewRecordValues(pair.Key, newdatecollected);
                            else if (pair.Key == "TIME_COLLECTED" && PLCDBGlobal.instance.ThisFieldIsSampled("TIME_COLLECTED"))
                                e.SetNewRecordValues(pair.Key, newtimecollected);
                            else if (pair.Key == "BOOKED_BY" && PLCDBGlobal.instance.ThisFieldIsSampled("BOOKED_BY"))
                                e.SetNewRecordValues(pair.Key, newbookedby);
                            else if (pair.Key == "BOOKING_DATE" && PLCDBGlobal.instance.ThisFieldIsSampled("BOOKING_DATE"))
                                e.SetNewRecordValues(pair.Key, newbookingdate);
                            else if (pair.Key == "BOOKING_TIME" && PLCDBGlobal.instance.ThisFieldIsSampled("BOOKING_TIME"))
                                e.SetNewRecordValues(pair.Key, newbookingtime);
                            else
                                e.SetNewRecordValues(pair.Key, string.Empty);
                        }
                    }

                    if (PLCSession.GetLabCtrl("DUPE_COLLECTED_BY_AND_DATE") == "T")
                    {
                        if (PLCDBGlobal.instance.ThisFieldIsSampled("RECOVERY_LOCATION"))
                            e.SetNewRecordValues("RECOVERY_LOCATION", newrecoverloc);

                        if (PLCDBGlobal.instance.ThisFieldIsSampled("RECOVERY_ADDRESS_KEY"))
                            e.SetNewRecordValues("RECOVERY_ADDRESS_KEY", newrecoverlockey);

                        if (PLCDBGlobal.instance.ThisFieldIsSampled("SEIZED_FOR_BIOLOGY"))
                            e.SetNewRecordValues("SEIZED_FOR_BIOLOGY", newSeizedDNA); 
                    }

                    if (PLCSession.GetLabCtrl("INITIAL_PROCESS_CODE") != "")
                        e.SetNewRecordValues("PROCESS", PLCSession.GetLabCtrl("INITIAL_PROCESS_CODE"));

                    if ((PLCSession.GetLabCtrl("RMS_EXTERNAL_IS_VOUCHER") == "T") || (PLCDBGlobal.instance.ThisFieldIsSampled("RMS_EXTERNAL")))
                        e.SetNewRecordValues("RMS_EXTERNAL", qryParentItem.FieldByName("RMS_EXTERNAL"));

                    if (isCaseFromCPD)
                        e.SetNewRecordValues("ETRACK_INVENTORY_ID", qryParentItem.FieldByName("ETRACK_INVENTORY_ID"));
                }
            }

            if (string.IsNullOrEmpty(CurrentECN)
                && string.IsNullOrEmpty(ParentECN)) // Add Item
            {
                if ((!string.IsNullOrEmpty(FloorCustody) 
                    && !string.IsNullOrEmpty(FloorLocation)))
                {
                    e.SetNewRecordValues("CUSTODY_OF", FloorCustody);
                    e.SetNewRecordValues("LOCATION", FloorLocation);
                }
            }

            e.SetNewRecordValues("CUSTODY_BY", PLCSession.PLCGlobalAnalyst);
            e.SetNewRecordValues("CUSTODY_DATE", System.DateTime.Now);

            ViewState["ParentECN"] = PLCSession.PLCGlobalECN;
            ViewState["NewRecordECN"] = PLCSession.PLCGlobalECN = "0";
        }

        protected void PLCDBPanel1_PLCDBPanelGetNewRecord(object sender, PLCDBPanelGetNewRecordEventArgs e)
        {
            string ecn = Convert.ToString(PLCSession.GetNextSequence("LABITEM_SEQ"));
            e.SetNewRecordValues("EVIDENCE_CONTROL_NUMBER", ecn);
            e.newWhereClause = " WHERE EVIDENCE_CONTROL_NUMBER = " + ecn;

            ViewState["NewRecordECN"] = PLCSession.PLCGlobalECN = ecn;
        }

        protected void PLCDBPanel1_PLCDBPanelValidate(object sender, PLCDBPanelValidateEventArgs e)
        {
            if (e.fldName == "LAB_CASE_SUBMISSION")
            {
                if (!validSubmission(e.fldValue))
                {
                    e.isValid = false;
                    e.errMsg = "Invalid Submission Number";
                }
            }

            if (!AllTabsValid() && !(bool)ViewState["MandatoryFieldMissing"]) // AACI(05/04/2011): Moved Details Tab validation to Main Tab validation
            {
                e.isValid = false;
                ViewState["MandatoryFieldMissing"] = true;
            }
        }

        protected void PLCDBPanel1_PLCDBPanelCodeHeadChanged(object sender, PLCDBPanelCodeHeadChangedEventArgs e)
        {
            if ((e.CodeTable == "CV_ITEMCAT" || e.CodeTable == "TV_ITEMCAT") || (e.CodeTable == "CV_PACKTYPE" || e.CodeTable == "TV_PACKTYPE"))
            {
                SetItemTypeFilter();

                if (PLCDBPanel1.GetFlexBoxControl("ITEM_TYPE").TableName == "CV_ITEMTYPEREF")
                {
                    PLCDBPanel1.setpanelfield("ITEM_TYPE", "");

                    if (!ItemAttribute.IsBrowseMode)
                        DoUpdateItemAttributes();
                }

                // Set focus to the control that initiated the postback. If we don't do this, the focus reverts back to the first control after auto-postback.
                string nextFieldClientID = e.CodeTable.IndexOf("ITEMCAT", 2) > 0 ? PLCDBPanel1.GetNextFieldClientID("ITEM_CATEGORY") : PLCDBPanel1.GetNextFieldClientID("PACKAGING_CODE");
                if (nextFieldClientID != "" && (PLCDBPanel1.IsPostbackPanelField("ITEM_CATEGORY") || PLCDBPanel1.IsPostbackPanelField("PACKAGING_CODE")))
                {
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "nextfieldfocus",
                        String.Format("SetFocusWithDelay('{0}', 1000)", nextFieldClientID), true);
                }
            }
            else if (e.CodeTable == "CV_ITEMTYPE" || e.CodeTable == "TV_ITEMTYPE" || e.CodeTable == "CV_ITEMTYPEREF")
            {
                if (!ItemAttribute.IsBrowseMode)
                {
                    bool hasConfirmation = DoUpdateItemAttributes();

                    // Set focus to the control that initiated the postback. If we don't do this, the focus reverts back to the first control after auto-postback.
                    string nextFieldClientID = PLCDBPanel1.GetNextFieldClientID("ITEM_TYPE");
                    if (nextFieldClientID != "" && PLCDBPanel1.IsPostbackPanelField("ITEM_TYPE") && !hasConfirmation)
                    {
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "nextfieldfocus",
                            String.Format("SetFocusWithDelay('{0}', 1000)", nextFieldClientID), true);
                    }
                }
            }
            SetItemDescriptionRequired();
            SetThirdPartyBarcodeVisibility();
        }

        protected void PLCDBPanel1_TextChanged(object sender, PLCDBPanelTextChangedEventArgs e)
        {
            if (e.FieldName == "QUANTITY")
            {
                if(!PLCSession.GetLabCtrl("USES_ITEM_ATTRIBUTE_ENTRIES").Equals("T"))
                    return;
                
                int quantity = PLCSession.SafeInt(PLCDBPanel1.getpanelfield("QUANTITY"), 1);
                if (quantity > 0 && quantity < ItemAttribute.MaxExhibitNumber)
                {
                    mbConfirmDeleteAttr.Message = "Delete Exhibits above " + quantity + "?";
                    mbConfirmDeleteAttr.Show();
                }
                else
                {
                    ItemAttribute.UpdateMaxExhibitNumber(PLCSession.SafeInt(PLCDBPanel1.getpanelfield("QUANTITY"), 1));
                    //SelectTab(VIEW_ATTRIBUTE);
                }
            }
        }

        protected void PLCButtonPanel1_PLCButtonClick(object sender, PLCCONTROLS.PLCButtonClickEventArgs e)
        {
            #region ValidateItems

            if ((e.button_name == "SAVE") && (e.button_action == "BEFORE"))
            {
                string CurrentItemNumber = PLCDBPanel1.getpanelfield("LAB_ITEM_NUMBER");

                // validate if item still exists if record unlock user option is on. other issues regarding editing deleted items are not yet handled.
                if (PLCSession.CheckUserOption("DELLOCKS") && !e.row_added)
                {
                    PLCQuery qryItemExists = new PLCQuery();
                    qryItemExists.SQL = string.Format("SELECT EVIDENCE_CONTROL_NUMBER FROM TV_LABITEM WHERE CASE_KEY = {0} AND EVIDENCE_CONTROL_NUMBER = {1}", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN);
                    if (qryItemExists.Open() && qryItemExists.IsEmpty())
                    {
                        e.button_canceled = true;
                        PLCDBGlobal.instance.RemoveRecordLocks("TV_LABITEM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, "-1", PLCSession.PLCGlobalAnalyst);
                        dlgMessage.ShowAlert("Item Record", "Item #" + CurrentItemNumber + " does not exists.");
                        GridView1.InitializePLCDBGrid();

                        if (GridView1.Rows.Count > 0)
                        {
                            GridView1.SelectedIndex = 0;
                            PLCSession.PLCGlobalECN = GridView1.SelectedDataKey.Value.ToString();
                            UpdateCustodyStatusDisplay(PLCSession.PLCGlobalECN);
                            GrabGridRecord();
                        }
                        else
                        {
                            PLCSession.PLCGlobalECN = "";
                            PLCDBPanel1.EmptyMode();
                            PLCButtonPanel1.SetEmptyMode();
                            SetAttachmentsButton("CASE");
                            EnableButtonControls(false, false);
                            ClearCustodyStatusDisplay();
                        }

                        return;
                    }
                }

                //validate labitemnumber                
                if (!ValidLabItemNumber(CurrentItemNumber))
                {
                    e.button_canceled = true;
                    return;
                }

                if (LabItemNumberExists(CurrentItemNumber, e.row_added))
                {
                    bool blnItemNumberSet = false;

                    if (e.row_added)
                    {
                        blnItemNumberSet = GetItemNumber();
                        if (blnItemNumberSet)
                            mbox.ShowMsg("Item Number", "The Item #" + CurrentItemNumber + " already exists. The next item number will be assigned.", 1);
                        else
                        {
                            mbox.ShowMsg("Item Number", "The Item #" + CurrentItemNumber + " already exists.", 1);
                            e.button_canceled = true;
                            return;
                        }

                    }
                    else
                    {
                        mbox.ShowMsg("Item Number", "The Item #" + CurrentItemNumber + " already exists.", 1);
                        e.button_canceled = true;
                        return;
                    }
                }

                if (PLCSession.GetLabCtrl("DUPE_DEPT_ITEM_NUM_ON_SAMPLE") == "T" && !String.IsNullOrEmpty(PLCDBPanel1.getpanelfield("DEPARTMENT_ITEM_NUMBER")) && DeptItemNumberExists(PLCDBPanel1.getpanelfield("DEPARTMENT_ITEM_NUMBER"), e.row_added))
                {
                    mbox.ShowMsg("Department Item Number", "The Department Item #" + PLCDBPanel1.getpanelfield("DEPARTMENT_ITEM_NUMBER") + " already exists.", 1);
                    e.button_canceled = true;
                    return;
                }

                //validate thirdpartybarcode
                if (!IsThirdPartyBarcodeValid())
                {
                    e.button_canceled = true;
                    return;
                }

                // see if names with selected relation are checked
                string nameList = NameRelationCheck();
                if (!string.IsNullOrEmpty(nameList))
                {
                    mbox.ShowMsg("Items Names Link", "The following Names need to be checked: <br>" + nameList, 0);
                    e.button_canceled = true;
                    return;
                }

                // NYPD: copy voucher from submission if blank on new item
                if (PLCSession.GetLabCtrl("RMS_EXTERNAL_IS_VOUCHER") == "T")
                {
                    // validate that voucher does not exist yet
                    if ((!String.IsNullOrEmpty(PLCDBPanel1.getpanelfield("RMS_EXTERNAL").ToString())) && (dbgbl.VoucherExistsInAnotherCase(PLCDBPanel1.getpanelfield("RMS_EXTERNAL").ToString())))
                    {
                        mbox.ShowMsg("Voucher Number", "Voucher already exists in a different case.", 1);
                        e.button_canceled = true;
                        return;
                    }

                    if ((e.row_added) && (String.IsNullOrEmpty(PLCDBPanel1.getpanelfield("RMS_EXTERNAL").ToString())))
                    {
                        PLCDBPanel1.setpanelfield("RMS_EXTERNAL", GetSubmissionCommandVoucher(PLCSession.PLCGlobalCaseKey, PLCDBPanel1.getpanelfield("LAB_CASE_SUBMISSION")));
                    }
                }

                if (ViewState["MandatoryFieldMissing"] != null ? Convert.ToBoolean(ViewState["MandatoryFieldMissing"]) : false)
                {
                    e.button_canceled = true;
                    ViewState["MandatoryFieldMissing"] = false;
                    PLCButtonPanel1.SetSaveError("    Unable to save this information. Look for errors in red.");
                    return;
                }

                if (!Currency1.IsVerified())
                {
                    e.button_canceled = true;
                    return;
                }

                if (ItemTypeChangeConfirmed)
                {
                    //delete existing currency records
                    Currency1.DeleteData(PLCSession.PLCGlobalECN);

                    if (!PLCSession.GetLabCtrl("USES_ITEM_ATTRIBUTE_ENTRIES").Equals("T"))
                        ItemAttribute.DeleteCurrentAttributes();

                    ItemTypeChangeConfirmed = false;
                }

                // Call RMS Evidence Custody update for items created in a case from RMS.
                bool LIMSInRMSMode = !string.IsNullOrEmpty(PLCSession.GetWebConfiguration("RMS_URL"));
                bool barcodeExists = ItemHasThirdPartyBarcode(PLCSession.PLCGlobalECN);
                bool caseIsFromRMS = (PLCDBGlobal.instance.GetTableDetailsByCustomKey("TV_LABCASE", "CASE_FROM_RMS", "CASE_KEY", PLCSession.PLCGlobalCaseKey) == "T");
                string thirdPartyBarcode = PLCDBPanel1.getpanelfield("THIRD_PARTY_BARCODE");

                if (!barcodeExists && caseIsFromRMS && !string.IsNullOrEmpty(thirdPartyBarcode) && LIMSInRMSMode)
                {
                    List<ItemRec> ItemList = new List<ItemRec>();
                    ItemList.Add(new ItemRec(0, "", "", "", "", "", null, 0, "", "", Convert.ToInt32(PLCSession.PLCGlobalECN), thirdPartyBarcode, "", "", "", "", "", ""));
                    PLCDBGlobal.instance.UpdateRMSEvidenceCustodyOut(PLCSession.PLCGlobalDepartmentCaseNumber, ItemList);
                }
            }

            if (e.button_name == "EDIT" && e.button_action == "BEFORE")
            {
                if (IsSelectedRecordsLocked(null))
                {
                    dlgMessage.ShowAlert("Record Lock", "Item locked by another user for editing.<br/>" + RecordsLockedInfo);
                    e.button_canceled = true;
                    return;
                }
                else
                    PLCDBGlobal.instance.LockUnlockRecord("TV_LABITEM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, "-1", true);
            }

            if (e.button_name == "DELETE" && e.button_action == "BEFORE")
            {
                if (IsSelectedRecordsLocked(null))
                {
                    dlgMessage.ShowAlert("Record Lock", "Item locked by another user for editing.<br/>" + RecordsLockedInfo);
                    e.button_canceled = true;
                    return;
                }
                else
                    PLCDBGlobal.instance.LockUnlockRecord("TV_LABITEM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, "-1", true);

                if (GetAffectedItemRecords(e))
                    return;

                DeleteItemNameLink(GridView1.SelectedDataKey.Value.ToString());
            }


            #endregion ValidateItems

            #region After Action

            if ((e.button_name == "ADD") & (e.button_action == "AFTER"))
            {
                // check whether we will be using the dbpanel default values on sample
                if (((ViewState["SAMPLE_ITEM_NEW_CONTAINER"] != null) || (Convert.ToBoolean(ViewState["SAMPLE_ITEM_NEW_CONTAINER"]))) && PLCSession.GetLabCtrl("USES_DBPANEL_DEFAULT_ON_SAMPLE") == "T")
                {
                    PLCDBPanel1.SetPanelDefaultValues();
                }


                if ( (PLCSession.GetLabCtrl("SUGGEST_ITEM_NUMBERS") == "T") && (string.IsNullOrEmpty(PLCDBPanel1.getpanelfield("LAB_ITEM_NUMBER"))))
                {
                    string msgTitle = "New Item";
                    if (ViewState["DUPE_ITEM"] != null && Convert.ToBoolean(ViewState["DUPE_ITEM"]))
                        msgTitle = "Dupe Item";
                    else if (ViewState["SAMPLE_ITEM_NEW_CONTAINER"] != null && Convert.ToBoolean(ViewState["SAMPLE_ITEM_NEW_CONTAINER"]))
                        msgTitle = "Sample Item";

                    ViewState["DUPE_ITEM"] = null;
                    ViewState["SAMPLE_ITEM_NEW_CONTAINER"] = null;
                    ViewState["NewRecordECN"] = null;

                    dlgMessage.ShowAlert(msgTitle, "There is an error in generating the Lab Item Number. Please contact the LIMS Administrator.");
                    e.button_canceled = true;
                    SetPageBrowseMode(true);
                    GrabGridRecord();
                }
                else if (string.IsNullOrEmpty(PLCSession.PLCGlobalSubmissionNumber))
                {
                    dlgMessage.ShowAlert("New Item", "Item needs submission.");
                    e.button_canceled = true;
                    SetPageBrowseMode(true);
                    GrabGridRecord();
                }
                else
                {
                    if (!string.IsNullOrEmpty(CurrentECN) || !string.IsNullOrEmpty(ParentECN))
                    {
                        RecreateBlankAttributes(true);
                    }
                    else
                    {
                        // to force inequality (before and after) of item type field
                        PLCDBPanel1.GetFlexBoxControl("ITEM_TYPE").ClearBeforePostbackValue();
                    }
                    SetPageBrowseMode(false);
                    SetCurrencyTabText("");
                    SetItemDescriptionRequired();
                }

                ShowDefaultTab();

                // set DBPanel defaults except for Sample and Kit
                if ((ViewState["SAMPLE_ITEM_NEW_CONTAINER"] == null) || (!Convert.ToBoolean(ViewState["SAMPLE_ITEM_NEW_CONTAINER"])))
                {
                    // if Dup flag on, set DBPanel defaults if not Dupe button clicked
                    if (PLCSession.GetLabCtrl("DUPE_COLLECTED_BY_AND_DATE") == "T")
                    {
                        if ((ViewState["DUPE_ITEM"] == null) || (!Convert.ToBoolean(ViewState["DUPE_ITEM"])))
                            PLCDBPanel1.SetPanelDefaultValues();
                    }
                    else
                    {
                        if ((ViewState["DUPE_ITEM"] == null) || (!Convert.ToBoolean(ViewState["DUPE_ITEM"])))
                            PLCDBPanel1.SetPanelDefaultValues();
                    }

                    if (PLCSession.GetLabCtrl("ANALYST_COLLECTED_BY_AND_DATE") == "T" && PLCSession.GetLabCtrl("DUPE_COLLECTED_BY_AND_DATE") != "T")
                    {
                        if ((ViewState["DUPE_ITEM"] != null) && (Convert.ToBoolean(ViewState["DUPE_ITEM"])))
                        {
                            PLCDBPanel1.setpanelfield("COLLECTED_BY", PLCSession.PLCGlobalAnalyst);
                            PLCDBPanel1.setpanelfield("DATE_COLLECTED", DateTime.Now.ToString("MM/dd/yyyy"));
                            PLCDBPanel1.setpanelfield("TIME_COLLECTED", DateTime.Now.ToString("HH:mm"));
                        }
                    }
                }
            }

            if ((e.button_name == "EDIT") & (e.button_action == "AFTER"))
            {
                SetPageBrowseMode(false);
                //Ticket # 22449
                ViewState["ITASSIGN_ITEM_DESCRIPTION"] = PLCDBPanel1.getpanelfield("ITEM_DESCRIPTION").Trim();
                ViewState["NewRecordECN"] = null;
            }

            #endregion After Action

            #region Delete Items
            if ((e.button_name == "DELETE") & (e.button_action == "AFTER"))
            {
                DeleteHangingItemRecords();
                DeleteAttachmentsOnServer();

                GridView1.InitializePLCDBGrid();

                if (GridView1.Rows.Count > 0)
                {
                    GridView1.SelectedIndex = 0;
                    GridView1.SetClientSideScrollToSelectedRow();

                    PLCSession.PLCGlobalECN = GridView1.SelectedDataKey.Value.ToString();
                    UpdateCustodyStatusDisplay(PLCSession.PLCGlobalECN);

                    GrabGridRecord();
                }
                else
                {
                    PLCSession.PLCGlobalECN = "";
                    PLCDBPanel1.EmptyMode();
                    PLCButtonPanel1.SetEmptyMode();
                    ItemAttribute.InitializeMultiEntries(1);
                    SetAttachmentsButton("CASE");
                    EnableButtonControls(false, false);
                    ClearCustodyStatusDisplay();
                }
            }
            #endregion Delete Items

            #region Save Items
            if (e.button_name == "SAVE")
            {
                if (e.button_action == "BEFORE")
                {
                    string itmCat = PLCDBPanel1.getpanelfield("ITEM_CATEGORY");

                    if (IsCategoryNameLinkRequired(itmCat) && !ItemHasNamesChecked())
                    {
                        string categoryPrompt = GetNameLinkPrompt(itmCat);
                        string msgPrompt = !string.IsNullOrEmpty(categoryPrompt) ? categoryPrompt : "Please select a Name for this " + PLCSession.GetCodeDesc("TV_ITEMCAT", itmCat) + " category.";

                        mbox.ShowMsg("Name Link", msgPrompt, 0);
                        e.button_canceled = true;
                        return;
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
                            mInput.ShowMsg("Case update reason", "Please enter the reason for your changes", 0, txtConfirmUpdate.ClientID, btnConfirmUpdate.ClientID, "Save", "Cancel");
                            e.button_canceled = true;
                            return;
                        }
                    }

                    if (hasChanges)
                    {
                        dbgbl.MarkAssignmentsForRegeneration(PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, "LABITEM", auditText);
                    }

                    if (PLCDBPanel1.HasChanges() || ItemNameHasChanges() || (!bnAttribute.Disabled && ItemAttribute.HasChanges()) || (!bnCurrency.Disabled && Currency1.HasChanges()))
                        PLCDBPanel1.ReasonChangeKey = dbgbl.GenerateReasonChange("ITEMS TAB SAVE", reasonText);
                }

                if (e.button_action == "AFTER")
                {

                    NotifyRetentionReviewChanged();

                    SaveTabs();

                    if (!e.row_added)
                        PLCDBGlobal.instance.RemoveRecordLocks("TV_LABITEM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, "-1", PLCSession.PLCGlobalAnalyst);

                    AddItemSortToLabItem(PLCSession.PLCGlobalECN, PLCDBPanel1.getpanelfield("LAB_ITEM_NUMBER"), PLCDBPanel1.getpanelfield("DEPARTMENT_ITEM_NUMBER"));
                    // Ticket # 22449 - update ITASSIGN on Item Edit
                    if (!e.row_added)
                        UpdateITAssignDescription(PLCSession.PLCGlobalECN, (string)ViewState["ITASSIGN_ITEM_DESCRIPTION"], PLCDBPanel1.getpanelfield("ITEM_DESCRIPTION"));

                    PLCSession.CheckNarcoSubmission(PLCSession.PLCGlobalECN);

                    if ((!e.row_added) && (!string.IsNullOrEmpty(PLCDBPanel1.getpanelfield("LAB_CASE_SUBMISSION"))) && (PLCDBPanel1.getpanelfield("LAB_CASE_SUBMISSION") != PLCDBPanel1.GetOriginalValue("LAB_CASE_SUBMISSION")))
                    {
                        //update submission-item link
                        DeleteSubLinkItem(PLCSession.PLCGlobalECN);
                        PLCDBGlobal.instance.AppendSubLinkItem(PLCSession.PLCGlobalECN, PLCDBPanel1.getpanelfield("LAB_CASE_SUBMISSION"), PLCDBPanel1.getpanelfield("ITEM_TYPE"), false);
                    }

                    UpdateBiologicalCourierDestination();

                    if (e.row_added)
                    {
                        //add default custody
                        if (ViewState["DUPE_ITEM"] != null && Convert.ToBoolean(ViewState["DUPE_ITEM"]))
                        {
                            //check if item has a parent item
                            //if not, check SUBMISSION_CUSTODY_TYPE flag and create two custody records linked to the submission
                            bool analystCustodyAlreadyExists = false;
                            PLCQuery qryParent = new PLCQuery("SELECT PARENT_ECN FROM TV_LABITEM WHERE EVIDENCE_CONTROL_NUMBER = " + PLCSession.PLCGlobalECN);
                            qryParent.Open();
                            if (qryParent.HasData() && string.IsNullOrEmpty(qryParent.FieldByName("PARENT_ECN")))
                                analystCustodyAlreadyExists = CreateCustodyRecordsForSubmissionItem();

                            //for duplicate item
                            PLCSession.AddDefaultCustody(PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, null, null, "DUPE", analystCustodyAlreadyExists);
                            ViewState["DUPE_ITEM"] = null;
                        }
                        else if (ViewState["SAMPLE_ITEM_NEW_CONTAINER"] != null && Convert.ToBoolean(ViewState["SAMPLE_ITEM_NEW_CONTAINER"]))
                        {
                            //for sample item
                            PLCSession.AddDefaultCustody(PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, null, null, "SAMPLE");

                            //Ticket#42220 - Update RMS Interface for item sampling.
                            string ecn = PLCSession.PLCGlobalECN;
                            string departmentCaseNumber = PLCDBGlobal.instance.GetTableDetailsByCustomKey("TV_LABCASE", "DEPARTMENT_CASE_NUMBER", "CASE_KEY", PLCSession.PLCGlobalCaseKey);
                            string thirdPartyBarcode = PLCDBGlobal.instance.GetTableDetailsByCustomKey("TV_LABITEM", "THIRD_PARTY_BARCODE", "EVIDENCE_CONTROL_NUMBER", ecn);

                            PLCDBGlobal.instance.UpdateRMSDerivativeEvidence(PLCSession.PLCGlobalCaseKey, thirdPartyBarcode, PLCDBPanel1.getpanelfield("LAB_ITEM_NUMBER"), ecn);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(FloorCustody)
                                && !string.IsNullOrEmpty(FloorLocation))
                            {
                                PLCSession.AddCustody(PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, FloorCustody, FloorLocation);
                            }
                            else
                            {
                                //check SUBMISSION_CUSTODY_TYPE flag and create two custody records linked to the submission
                                bool analystCustodyAlreadyExists = CreateCustodyRecordsForSubmissionItem();

                                //for new item
                                PLCSession.AddDefaultCustody(PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, analystCustodyAlreadyExists);
                            }
                        }

                        PLCDBGlobal.instance.Set_Default_Process(PLCSession.PLCGlobalECN, "");
                        Create_Default_Service_Request(PLCSession.PLCGlobalECN);

                        //save new sublink info
                        if (!string.IsNullOrEmpty(PLCDBPanel1.getpanelfield("LAB_CASE_SUBMISSION")))
                            PLCDBGlobal.instance.AppendSubLinkItem(PLCSession.PLCGlobalECN, PLCDBPanel1.getpanelfield("LAB_CASE_SUBMISSION"), PLCDBPanel1.getpanelfield("ITEM_TYPE"), false);
                        if (PLCSession.GetLabCtrl("AUTO_PRINT_ITEM_LABELS") == "T")
                        {
                            bool hasResponse;
                            string redirectString;
                            PLCDBGlobal.instance.PrintLASDLabel(PLCSession.PLCGlobalECN, out hasResponse, out redirectString);
                            if (hasResponse)
                                Response.Redirect(redirectString);
                        }

                        if (PLCSession.GetLabCtrl("DEFAULT_ASSIGNMENTS") == "T")
                            PLCDBGlobal.instance.AddItemDefaultAssignment(PLCSession.PLCGlobalECN, true);

                        if (ViewState["SAMPLE_ITEM_NEW_CONTAINER"] != null)
                        {
                            ViewState["SAMPLE_ITEM_PROCESS"] = ViewState["SAMPLE_ITEM_NEW_CONTAINER"];
                            if (Convert.ToBoolean(ViewState["SAMPLE_ITEM_NEW_CONTAINER"]))
                            {
                                if (PLCSession.GetLabCtrl("USES_SAMPLE_CONTAINER") == "T")
                                {
                                    mbSampleContainer.Show();
                                }
                                ViewState["SAMPLE_ITEM_NEW_CONTAINER"] = null;
                            }
                        }

                        (new PLCDBGlobal()).UpdateItemWeightBasedOnAttribute(PLCSession.PLCGlobalECN);
                    }

                    if (PLCSession.GetLabCtrl("RMS_EXTERNAL_IS_VOUCHER") == "T")
                    {
                        // rms_external does not exist in labsub, create new labsub record with command_voucher = rms_external
                        if (!string.IsNullOrEmpty(PLCDBPanel1.getpanelfield("RMS_EXTERNAL").ToString().Trim()))
                        {
                            string SubKey = "";
                            string SubNum = "";
                            if (!SubmissionCommandVoucherExists(PLCDBPanel1.getpanelfield("RMS_EXTERNAL").ToString(), ref SubKey, ref SubNum))
                                AddNewLabSubCommandVoucher(PLCDBPanel1.getpanelfield("RMS_EXTERNAL").ToString(), PLCSession.PLCGlobalECN);
                            else
                                // update submission number just in case user changes voucher# belonging to a different submission 
                                UpdateItemSubmissionNumber(PLCSession.PLCGlobalECN, SubKey, SubNum);
                        }
                    }

                    //refresh grid
                    GridView1.InitializePLCDBGrid();
                    GridView1.SelectRowByDataKey(PLCSession.PLCGlobalECN);

                    //show custody location
                    UpdateCustodyStatusDisplay(PLCSession.PLCGlobalECN);

                    PLCDBPanel1.ReasonChangeKey = 0;
                    ItemAttribute.IsItemTypeChanged = false;
                    GrabGridRecord();
                    ViewState["NewRecordECN"] = null;


                    //display aftermessage here

                    PostSavingMessage();

                    }
            }

            #endregion Save Items

            if ((e.button_name == "CANCEL") & (e.button_action == "AFTER"))
            {
                SetItemTypeFilter(true);

                ViewState["SAMPLE_ITEM_NEW_CONTAINER"] = null;      // Reset Sample add state.
                ViewState["DUPE_ITEM"] = null;      // Reset Dupe add state.
                ViewState["NewRecordECN"] = null;
                PLCSession.SetDefault("LABSTAT_COMMENTS", null);

                if (!e.row_added)
                    PLCDBGlobal.instance.RemoveRecordLocks("TV_LABITEM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, "-1", PLCSession.PLCGlobalAnalyst);

                GridView1.InitializePLCDBGrid();
                GridView1.SelectRowByDataKey(PLCSession.PLCGlobalECN);
                GrabGridRecord();

                LoadNames();
                LoadCurrencyData();

                RecreateBlankAttributes(false);
                ItemTypeChangeConfirmed = false;

                ViewState["AttributesChanges"] = null;
                ViewState["CurrencyChanges"] = null;
                ViewState["ItemNamesChanges"] = null;
            }

            if (e.button_name == "RecordUnlock")
            {
                if (string.IsNullOrEmpty(PLCSession.PLCGlobalECN))
                {
                    mbox.ShowMsg("Item", "Please select an Item.", 1);
                    return;
                }

                mInput.ShowMsg("Unlock record", "Please enter why you have to unlock the record ?", 0, UserComments.ClientID, MsgCommentPostBackButton.ClientID);
                return;
            }

            if (e.button_name == "SAVE" && e.button_action == "AFTER")
            {
                SetItemTypeFilter(true);
                PLCSession.WriteDebug("Save After, ITEM_STAT_DEFAULT=" + PLCSession.GetLabCtrl("ITEM_STAT_DEFAULT"));

                if (PLCSession.GetLabCtrl("ITEM_STAT_DEFAULT") == "T")
                {
                    PLCSession.WriteDebug("ITEM_STAT_DEFAULT 1");
                    PLCHelperFunctions plcHelp = new PLCHelperFunctions();
                    plcHelp.SetItemDefaultStatus(PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, true);
                    PLCSession.WriteDebug("ITEM_STAT_DEFAULT 2");
                    PLCDBGlobal.instance.UpdateNextReviewDate(PLCSession.PLCGlobalECN);
                    PLCSession.WriteDebug("ITEM_STAT_DEFAULT 3");

                    //refresh grid and dbpanel
                    GridView1.InitializePLCDBGrid();
                    GridView1.SelectRowByDataKey(PLCSession.PLCGlobalECN);
                    GrabGridRecord();
                }

                //Add record of Sampled Item to TV_REVREQHIST
                if ((ViewState["SAMPLE_ITEM_PROCESS"] != null) || (Convert.ToBoolean(ViewState["SAMPLE_ITEM_PROCESS"])))
                {
                    if (!string.IsNullOrWhiteSpace(PLCSession.PLCGlobalECN))
                    {
                        PLCQuery qrySampleItem = new PLCQuery("SELECT * FROM TV_LABITEM WHERE EVIDENCE_CONTROL_NUMBER = " + PLCSession.PLCGlobalECN);
                        qrySampleItem.Open();
                        if (!qrySampleItem.IsEmpty())
                            PLCDBGlobal.instance.AddRevReqHistory(PLCSession.PLCGlobalECN, "", qrySampleItem.FieldByName("PROCESS"), "SampleItem - TAB4Items", PLCSession.PLCGlobalAnalyst, 0);
                    }
                }
            }

            if ((e.button_name == "ADD" || e.button_name == "EDIT") && e.button_action == "AFTER")
            {
                SetItemTypeFilter();

                if (PLCSession.CheckUserOption("ITEMSTAT"))
                {
                    PLCDBPanel1.EnablePanelField("PROCESS", false);
                    //PLCDBPanel1.SetMyFieldMode("PROCESS", true);
                }
            }

            if (e.button_name == "Bulk Delete")
            {
                if (GridView1.Rows.Count > 0)
                {
                    LoadBulkDeletePopup();
                    ScriptManager.RegisterStartupScript(btnShowBulkPopup, btnShowBulkPopup.GetType(), "Popup", "BDialogOpen(true);", true);
                }
            }
        }



        protected void NotifyRetentionReviewChanged()
            {

            Boolean pleaseNotify = false;
            String ChangedFields = "";
            String rtCaseFields = PLCSession.GetLabCtrl("RETENTION_TRACKING_ITEM_FIELDS");
            String rtMessage = PLCSession.GetLabCtrl("RETENTION_TRACKING_ITEM_MESSAGE");
            if (String.IsNullOrWhiteSpace(rtCaseFields)) return;
            if (String.IsNullOrWhiteSpace(rtMessage)) return;

            String[] fList = rtCaseFields.Split(',');

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

            rtMessage = rtMessage.Replace("{FIELDS}", ChangedFields);
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

        protected void PostSavingMessage()
            {
            if (ViewState["PostSaveMessage"] != null)
                {
                string message = ViewState["PostSaveMessage"].ToString();
                mbox.ShowMsg("Case Info", message, 0);
                ViewState["PostSaveMessage"] = null;
                }
            }

        protected void ItemAttribute_ExhibitNumberChanged(object sender, EventArgs e)
        {
            ShowAttributesTab();
        }

        #endregion

        #region Methods

        public string getpromptrequired()
        {
            return secondrevchar;
        }

        private void EnableButtonControls(bool Enable, bool UnLock)
        {
            if (PLCSession.GetLabCtrl("PROTECT_SUBMISSION_NUMBER") == "T")
                PLCDBPanel1.SetMyFieldMode("LAB_CASE_SUBMISSION", true);

            if (!Enable)
            {
                bnLabel.Enabled = false;
                bnNoLabel.Enabled = false;
            }
            else
            {
                if (PLCSession.GetLabCtrl("USES_WS_BARCODE_PRINTING") == "T")
                    bnLabel.Enabled = Enable;
                else
                    bnLabel.Enabled = PLCSession.GetLabCtrl("USES_MULTI_LABEL_PRINT") != "T";
            }

            MasterPage mp = (MasterPage)this.Master;
            mp.EnablePaperClip(!(GridView1.Rows.Count == 0));
            if (GridView1.Rows.Count <= 0) mp.SetPaperClipImageManually(false);

            bnSample.Enabled = Enable;
            bnDupe.Enabled = Enable;
            bnContainer.Enabled = Enable;
            bnKit.Enabled = Enable;
            bnItemList.Enabled = Enable;
            bnTransfer.Enabled = (Enable && CanUseTransferButton);
            bnItemFind.Enabled = Enable;
            btnSupp49.Enabled = Enable;
            btnStatusChange.Enabled = Enable;
            btnTrackRFID.Enabled = Enable;

            EnableMultiLabelPrint(Enable);
        }

        private void UnLockKit()
        {
            string LockedInfo = "";
            PLCDBGlobal.instance.IsRecordLocked("TV_KITITEMS", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, "-1", out LockedInfo);
        }

        private void CheckUserOptionReadOnlyAccess()
        {
            if (PLCSession.CheckUserOption("ITEMLBLOFF"))
            {
                bnLabel.Enabled = false;
                bnNoLabel.Enabled = false;
                btnPrintMultiLabel.Enabled = false;
            }

            SetReadOnlyAccess();

            if (!PLCSession.CheckUserOption("DELITEM"))
                PLCButtonPanel1.DisableButton("DELETE");

            // Ticket#38428 - User restriction on conditional item edit
            if ((PLCSession.CheckUserOption("EDITBOOKBY")) && (PLCDBPanel1.getpanelfield("BOOKED_BY") != PLCSession.PLCGlobalAnalyst))
                PLCButtonPanel1.DisableButton("EDIT");
        }

        private string GetItemCustodyLocation(string ecnkey)
        {
            PLCQuery query = new PLCQuery();
            query.SQL = "select C.DESCRIPTION as CustDesc, L.DESCRIPTION as LocDesc " +
                             "from TV_LABITEM I " +
                             "left outer join TV_CUSTCODE C ON C.CUSTODY_TYPE = I.CUSTODY_OF " +
                             "left outer join " + PLCDBGlobal.instance.GetTableNameView("TV_CUSTLOC") + " L ON I.CUSTODY_OF = L.custody_code AND I.LOCATION = L.location " +
                             "where EVIDENCE_CONTROL_NUMBER = " + Convert.ToInt32(ecnkey);
            query.Open();

            if (!query.EOF())
                return query.FieldByName("CustDesc") + "-" + query.FieldByName("LocDesc");
            else
                return "";
        }

        private void GrabGridRecord()
        {
            bnNoLabel.Visible = false;
            if (GridView1.Rows.Count > 0)
            {
                PLCSession.PLCGlobalECN = GridView1.SelectedDataKey.Value.ToString();
                PLCDBPanel1.PLCWhereClause = " WHERE EVIDENCE_CONTROL_NUMBER = " + PLCSession.PLCGlobalECN;

                ViewState["itemhasbarcode"] = false;
                PLCQuery qryTemp = new PLCQuery("SELECT PROCESS, BARCODE, PARENT_ECN, CUSTODY_OF, LOCATION FROM TV_LABITEM WHERE EVIDENCE_CONTROL_NUMBER = " + PLCSession.PLCGlobalECN);
                if (qryTemp.Open() && qryTemp.HasData())
                {
                    if (PLCSession.GetLabCtrl("USES_SUBITEM_TRANSFER") == "T" && PLCSession.CheckUserOption("HANDLE") && PLCSession.CheckUserOption("PRINTBAR"))
                    {
                        ViewState["itemhasbarcode"] = !string.IsNullOrEmpty(qryTemp.FieldByName("BARCODE"));
                        if (!String.IsNullOrEmpty(qryTemp.FieldByName("PARENT_ECN")) && qryTemp.FieldByName("PARENT_ECN") != "0")
                        {
                            // Ticket#24498 - Get parent ECN custody/location; compare with subitem's custody/location
                            string sParentCustOf = string.Empty;
                            string sParentLocation = string.Empty;

                            PLCQuery qryParentECN = new PLCQuery("SELECT CUSTODY_OF, LOCATION FROM TV_LABITEM WHERE EVIDENCE_CONTROL_NUMBER = " + qryTemp.FieldByName("PARENT_ECN"));
                            if (qryParentECN.Open() && qryParentECN.HasData())
                            {
                                sParentCustOf = qryParentECN.FieldByName("CUSTODY_OF");
                                sParentLocation = qryParentECN.FieldByName("LOCATION");
                            }

                            if (qryTemp.FieldByName("CUSTODY_OF") == sParentCustOf && qryTemp.FieldByName("LOCATION") == sParentLocation)
                            {
                                bnNoLabel.Visible = true;
                                if (!String.IsNullOrEmpty(qryTemp.FieldByName("BARCODE")))
                                {
                                    bnNoLabel.Enabled = true;
                                    bnNoLabel.Text = "Clear";
                                }
                                else
                                {
                                    bnNoLabel.Enabled = false;
                                    bnNoLabel.Text = "No Label";
                                }
                            }
                        }
                    }
                }
            }

            PLCDBPanel1.DoLoadRecord();
            SetAttachmentsButton("ITEM");
            SetPageBrowseMode(true);
            SetCurrencyTabText(PLCSession.PLCGlobalECN);
            if (!PLCCommonClass.IsReadOnlyAccess("WEBINQ,RONLYITTAB"))
                IsSelectedRecordsLocked(null);

            MasterPage mp = (MasterPage)this.Master;
            mp.SetFilterDescription(PLCDBPanel1.getpanelfield("LAB_ITEM_NUMBER"));
            ItemAttribute.CurrentItemType = PLCDBPanel1.getpanelfield("ITEM_TYPE");
            ItemAttribute.InitializeMultiEntries(PLCSession.SafeInt(PLCDBPanel1.getpanelfield("QUANTITY"), 1));

            SetItemDescriptionRequired();
            SetThirdPartyBarcodeVisibility();

            RefreshMultiview();
        }

        private void SetCurrencyTabText(string ecn)
        {
            if (!string.IsNullOrEmpty(ecn) && ecn != "0")
            {
                string sCurrency = CheckCurrency(ecn);
                if (sCurrency != "")
                {
                    bnCurrency.InnerText = "Currency ($" + sCurrency + ")";
                    bnCurrency.Style.Add("font-weight", "bold");
                    return;
                }
            }

            bnCurrency.InnerText = "Currency ";
            bnCurrency.Style.Add("font-weight", "normal");
        }

        private void PDCheckItemReceivedByLab()
        {
            // LIMS is used by PD. Restrict editing if submission is received by the lab
            if (!string.IsNullOrEmpty(PLCSession.PLCGlobalECN))
            {
                PLCQuery qrySRPDF = new PLCQuery("SELECT * FROM TV_SRPDF417 P " +
                    "LEFT JOIN TV_SRMASTER S ON S.SR_MASTER_KEY = P.SR_MASTER_KEY " +
                    "LEFT JOIN TV_SUBLINK L ON L.SUBMISSION_KEY = P.SUBMISSION_KEY " +
                    "WHERE (S.LAB_ID IS NOT NULL OR S.LAB_ID != '') AND EVIDENCE_CONTROL_NUMBER = " + PLCSession.PLCGlobalECN);
                qrySRPDF.OpenReadOnly();
                if (qrySRPDF.HasData())
                {
                    PLCButtonPanel1.DisableEditDeleteButton();
                }
            }
        }

        /// <summary>
        /// Selects the default tab between Attributes and Currency
        /// </summary>
        private void ShowDefaultTab()
        {
            bnAttribute.Disabled = false;

            if (AllowCurrencyEntry(PLCDBPanel1.getpanelfield("ITEM_TYPE")))
            {
                SelectTab(VIEW_CURRENCY); //show currency tab              
                bnCurrency.Disabled = false;
            }
            else
            {
                SelectTab(VIEW_ATTRIBUTE); //show attributes tab
                bnCurrency.Disabled = true;
            }
        }

        private bool IsItemTypeValidByCodeCondition(string itemType, string itemCategory)
        {
            string itemTypeFilter = PLCDBPanel1.GetPanelRecByFieldName("ITEM_TYPE").codecondition;
            string sql = PLCSession.FormatSpecialFunctions(PLCSession.GenerateCodeHeadSQL("TV_ITEMTYPE", itemType, itemTypeFilter, "", "", itemCategory, true));
            PLCQuery qryCodeHead = new PLCQuery(sql);
            qryCodeHead.Open();
            return !qryCodeHead.IsEmpty();
        }

        private void DeleteHangingItemRecords()
        {
            try
            {
                // currency table
                PLCQuery qryDel = new PLCQuery("Delete from TV_CURRENCY where EVIDENCE_CONTROL_NUMBER = " + PLCSession.PLCGlobalECN);
                qryDel.ExecSQL();

                // labstat table
                qryDel = new PLCQuery("DELETE from TV_LABSTAT where EVIDENCE_CONTROL_NUMBER = " + PLCSession.PLCGlobalECN);
                qryDel.ExecSQL();

                // sublink
                qryDel = new PLCQuery("DELETE from TV_SUBLINK where EVIDENCE_CONTROL_NUMBER = " + PLCSession.PLCGlobalECN);
                qryDel.ExecSQL();

                ItemAttribute.DeleteCurrentAttributes();

                // review request
                qryDel = new PLCQuery(string.Format("DELETE FROM TV_REVREQUEST WHERE EVIDENCE_CONTROL_NUMBER = {0}", PLCSession.PLCGlobalECN));
                qryDel.ExecSQL();

                // tasks
                qryDel = new PLCQuery(string.Format("DELETE FROM TV_TASKLIST WHERE EVIDENCE_CONTROL_NUMBER = {0}", PLCSession.PLCGlobalECN));
                qryDel.ExecSQL();

                // plclock
                PLCDBGlobal.instance.RemoveRecordLocks("TV_LABITEM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, "-1", PLCSession.PLCGlobalAnalyst);
            }
            catch
            {
            }
        }

        private List<string> GetSubItems(string sItemEcn)
        {
            //Get descendants of the current sub item
            PLCQuery qrySubItems = new PLCQuery();
            qrySubItems.SQL = @"SELECT EVIDENCE_CONTROL_NUMBER FROM TV_LABITEM WHERE PARENT_ECN = " + sItemEcn + " AND BARCODE IS NULL";
            if (qrySubItems.Open() && qrySubItems.HasData())
                while (!qrySubItems.EOF())
                {
                    sItemECNs.Add(qrySubItems.FieldByName("EVIDENCE_CONTROL_NUMBER"));
                    GetSubItems(qrySubItems.FieldByName("EVIDENCE_CONTROL_NUMBER"));
                    qrySubItems.Next();
                }

            return sItemECNs;
        }

        private void ClearBarcode(string ECN)
        {
            int parentContainer;
            int itemContainer;
            string confirmMessage = "";

            List<string> subItemECNs = GetSubItems(ECN); // list that will hold all descendants of the current subitem

            ViewState.Add("SUBITEM_ECNS", subItemECNs.ToArray()); // convert list to array then save it's values to a viewstate

            PLCQuery qryLabItem = new PLCQuery();
            qryLabItem.SQL = @"SELECT P.CONTAINER_KEY AS PARENT_CONTAINER , I.CONTAINER_KEY AS ITEM_CONTAINER FROM TV_LABITEM I 
INNER JOIN TV_LABITEM P ON I.PARENT_ECN = P.EVIDENCE_CONTROL_NUMBER WHERE I.EVIDENCE_CONTROL_NUMBER = " + ECN;
            qryLabItem.Open();
            if (!qryLabItem.IsEmpty())
            {
                parentContainer = string.IsNullOrWhiteSpace(qryLabItem.FieldByName("PARENT_CONTAINER"))
                    ? 0
                    : Convert.ToInt32(qryLabItem.FieldByName("PARENT_CONTAINER"));
                itemContainer = string.IsNullOrWhiteSpace(qryLabItem.FieldByName("ITEM_CONTAINER"))
                    ? 0
                    : Convert.ToInt32(qryLabItem.FieldByName("ITEM_CONTAINER"));

                if (parentContainer == itemContainer)
                {
                    //parent and item not in container but separated / parent and item are inside same container but separated - Status : 4
                    ViewState["ItemKey"] = ECN;
                    ViewState["ClearStatus"] = "4";
                    ClearBarcodeContainer();
                }
                else
                {
                    if (parentContainer > 0 && itemContainer == 0)
                    {
                        //parent in container, sub item not in container - Status : 1
                        confirmMessage = "Item will be placed in the same container as its parent. Continue?";
                        ViewState["ClearStatus"] = "1";
                    }
                    else if (parentContainer == 0 && itemContainer > 0)
                    {
                        //parent not in container, sub item in container - Status : 2
                        confirmMessage = "Item will be removed from its container. Continue?";
                        ViewState["ClearStatus"] = "2";
                    }
                    else
                    {
                        //parent and sub item in different containers - Status : 3
                        confirmMessage = "Item will be placed in the same container as its parent. Continue?";
                        ViewState["ClearStatus"] = "3";
                    }

                    ViewState["ParentContKey"] = parentContainer;
                    ViewState["ItemKey"] = ECN;
                    mbClear.CancelButtonText = "No";
                    mbClear.OKButtonText = "Yes";
                    mbClear.Message = confirmMessage;
                    mbClear.Show();
                }
            }
        }

        private void ClearBarcodeContainer()
        {
            PLCQuery qryLabItem = new PLCQuery();
            qryLabItem.SQL = @"SELECT BARCODE, CONTAINER_KEY FROM TV_LABITEM WHERE EVIDENCE_CONTROL_NUMBER = " + ViewState["ItemKey"];
            qryLabItem.Open();
            if (!qryLabItem.IsEmpty())
            {
                if (ViewState["ClearStatus"].ToString() == "1") //link sub item to parent and put it inside parent item's container
                {
                    qryLabItem.Edit();
                    qryLabItem.SetFieldValue("CONTAINER_KEY", ViewState["ParentContKey"]);
                    qryLabItem.SetFieldValue("BARCODE", "");

                    UpdateSubItems("1");
                }
                else if (ViewState["ClearStatus"].ToString() == "2") //link sub item to parent and remove it from it's container
                {
                    qryLabItem.Edit();
                    qryLabItem.SetFieldValue("CONTAINER_KEY", "");
                    qryLabItem.SetFieldValue("BARCODE", "");

                    UpdateSubItems("2");
                }
                else if (ViewState["ClearStatus"].ToString() == "3") //link sub item to parent then change it's container to parent item's container
                {
                    qryLabItem.Edit();
                    qryLabItem.SetFieldValue("CONTAINER_KEY", ViewState["ParentContKey"]);
                    qryLabItem.SetFieldValue("BARCODE", "");

                    UpdateSubItems("1");
                }
                else if (ViewState["ClearStatus"].ToString() == "4") //link sub item to parent 
                {
                    qryLabItem.Edit();
                    qryLabItem.SetFieldValue("BARCODE", "");
                }
                qryLabItem.Post("TV_LABITEM", 7, 1);

                GridView1.InitializePLCDBGrid();
            }
        }

        private void UpdateSubItems(string containerStatus)
        {
            string[] sItemsArray = (string[])ViewState["SUBITEM_ECNS"];
            List<string> subItemECNs = new List<string>(sItemsArray);

            // update container of the descendants of the current selected subitem cleared
            if (subItemECNs.Count > 0)
            {
                foreach (string key in subItemECNs)
                {
                    PLCQuery qrySItems = new PLCQuery();
                    qrySItems.SQL = @"SELECT BARCODE, CONTAINER_KEY FROM TV_LABITEM WHERE EVIDENCE_CONTROL_NUMBER = " + key;
                    qrySItems.Open();
                    if (!qrySItems.IsEmpty())
                    {
                        if (containerStatus == "1")// place inside container / change container
                        {
                            qrySItems.Edit();
                            qrySItems.SetFieldValue("CONTAINER_KEY", ViewState["ParentContKey"]);
                        }
                        else
                        {
                            qrySItems.Edit();
                            qrySItems.SetFieldValue("CONTAINER_KEY", "");
                        }
                        qrySItems.Post("TV_LABITEM", 7, 1);
                    }
                }
            }
        }

        private bool ValidLabItemNumber(string LabItemNum)
        {
            if (LabItemNum == "")
            {
                mbox.ShowMsg("Validation", "Item number can not be blank", 1);
                return false;
            }

            return true;
        }

        private void ClickSavePLCButton()
        {
            PLCButtonPanel1.ClickSaveButton();
        }

        private bool IsThirdPartyBarcodeValid()
        {
            return true;
        }

        private bool IsDeptUsesThirdPartyBarcode()
        {
            string submissionNumber = PLCDBPanel1.getpanelfield("LAB_CASE_SUBMISSION");
            if (!string.IsNullOrEmpty(submissionNumber))
            {
                PLCQuery qryDeptName = new PLCQuery();
                qryDeptName.SQL = "SELECT * FROM TV_LABSUB S LEFT JOIN TV_DEPTNAME D ON D.DEPARTMENT_CODE = S.DEPARTMENT_CODE WHERE D.THIRD_PARTY_BARCODE = 'F' " +
                    "AND S.CASE_KEY = " + PLCSession.PLCGlobalCaseKey + " AND S.SUBMISSION_NUMBER = " + submissionNumber;
                qryDeptName.Open();
                return qryDeptName.IsEmpty();
            }

            return false;
        }

        private bool IsThirdPartyBarcodeValid_QQQ()
        {
            if (PLCSession.GetLabCtrl("USES_THIRD_PARTY_BARCODE") == "T")
            {
                string sItemType = PLCDBPanel1.getpanelfield("ITEM_TYPE");
                string sThirdPartyBarcode = PLCDBPanel1.getpanelfield("THIRD_PARTY_BARCODE");

                if (sItemType != "")
                {
                    PLCQuery qryItemType = new PLCQuery();
                    qryItemType.SQL = "SELECT THIRD_PARTY_BARCODE FROM TV_ITEMTYPE WHERE ITEM_TYPE = '" + sItemType + "'";
                    qryItemType.Open();
                    if (!qryItemType.IsEmpty())
                    {
                        if (qryItemType.FieldByName("THIRD_PARTY_BARCODE") == "T")
                        {
                            if (sThirdPartyBarcode == "")
                            {
                                if (PLCDBPanel1.GetControlClientID("THIRD_PARTY_BARCODE") != null)
                                    mbox.OnOkScript = "document.getElementById('" + PLCDBPanel1.GetControlClientID("THIRD_PARTY_BARCODE") + "').focus();";
                                mbox.ShowMsg("Validation",
                                             "Third Party Barcode is required for the selected item type.", 1);
                                return false;
                            }
                        }
                    }
                }

                if (sThirdPartyBarcode != "")
                {
                    PLCQuery qryLabItem = new PLCQuery();
                    qryLabItem.SQL =
                        "SELECT I.LAB_ITEM_NUMBER, C.DEPARTMENT_CASE_NUMBER FROM TV_LABITEM I LEFT OUTER JOIN TV_LABCASE C ON I.CASE_KEY = C.CASE_KEY WHERE THIRD_PARTY_BARCODE = '" +
                        sThirdPartyBarcode + "' AND NOT EVIDENCE_CONTROL_NUMBER = " + PLCSession.PLCGlobalECN;
                    qryLabItem.Open();
                    if (!qryLabItem.IsEmpty())
                    {
                        string sError = "This barcode is already linked to Item {" +
                                        qryLabItem.FieldByName("LAB_ITEM_NUMBER") + "} on Case {" +
                                        qryLabItem.FieldByName("DEPARTMENT_CASE_NUMBER") + "}.";
                        mbox.ShowMsg("Validation", sError, 1);
                        return false;
                    }
                }
            }

            return true;
        }

        private void SetThirdPartyBarcodeVisibility()
        {
            if (PLCSession.GetLabCtrl("USES_THIRD_PARTY_BARCODE") == "T")
            {
                string sItemType = PLCDBPanel1.getpanelfield("ITEM_TYPE");
                bool display = false;
                if (sItemType != string.Empty)
                {
                    PLCQuery qryItemType = new PLCQuery();
                    qryItemType.SQL = "SELECT THIRD_PARTY_BARCODE FROM TV_ITEMTYPE WHERE ITEM_TYPE = '" + sItemType + "'";
                    qryItemType.Open();
                    if (!qryItemType.IsEmpty())
                    {
                       display = IsDeptUsesThirdPartyBarcode() && qryItemType.FieldByName("THIRD_PARTY_BARCODE") == "T";
                       var thirdPartybarcode = PLCDBPanel1.GetPanelRecByFieldName("THIRD_PARTY_BARCODE");
                       if (display == true && thirdPartybarcode != null)
                            thirdPartybarcode.tb.Attributes.Add("BarcodeScanHere", "");
                    }
                }

                PLCDBPanel1.HidePanelRecRow("THIRD_PARTY_BARCODE", display);
            }
        }

        private void SetItemDescriptionRequired()
        {
            string sItemType = PLCDBPanel1.getpanelfield("ITEM_TYPE");
            string itemNameLinkRequired = "";
            if (sItemType != string.Empty)
            {
                PLCQuery qryItemType = new PLCQuery();
                qryItemType.SQL = "SELECT ITEM_DESC_REQUIRED, PRELOG_NAMITM_LINK FROM TV_ITEMTYPE WHERE ITEM_TYPE = '" + sItemType + "'";
                qryItemType.Open();
                if (!qryItemType.IsEmpty())
                {
                    if (qryItemType.FieldByName("ITEM_DESC_REQUIRED") == "T")
                    {
                        PLCDBPanel1.SetFieldAsRequired("ITEM_DESCRIPTION", true);
                    }
                    else
                    {
                        PLCDBPanel1.SetFieldAsRequired("ITEM_DESCRIPTION", false);
                    }

                    itemNameLinkRequired = qryItemType.FieldByName("PRELOG_NAMITM_LINK");
                }
            }
            else
            {
                PLCDBPanel1.SetFieldAsRequired("ITEM_DESCRIPTION", false);
            }

            tbNames.Attributes.Add("required", itemNameLinkRequired);
            bnNames.Style.Add("color", "");
        }

        /// <summary>
        /// Check if SEIZED_FOR_BIOLOGY is a required field
        /// </summary>
        private void ShowSeizedForBiology()
        {
            bool showField;
            if (PLCDBPanel1.HasPanelRec("TV_LABITEM", "SEIZED_FOR_BIOLOGY"))
            {
                if (PLCSession.GetLabCtrl("USES_LAB_OFFENSE") == "T")
                {
                    showField = HasFelonyLabOffense();

                    PLCDBPanel1.HidePanelRecRow("SEIZED_FOR_BIOLOGY", showField);
                    PLCDBPanel1.SetFieldAsRequired("SEIZED_FOR_BIOLOGY", showField);
                }
                else if (PLCSession.GetLabCtrl("USES_BIO_IDENTITY_PURPOSES") == "T")
                {
                    showField = HasFelonyCaseOffense();

                    PLCDBPanel1.HidePanelRecRow("SEIZED_FOR_BIOLOGY", showField);
                    PLCDBPanel1.SetFieldAsRequired("SEIZED_FOR_BIOLOGY", showField);
                }
            }
        }

        /// <summary>
        /// Check if case has a felony record in Lab Offense
        /// </summary>
        /// <returns>True if case has felony in Lab Offense</returns>
        private bool HasFelonyLabOffense()
        {
            bool hasFelony = false;

            PLCQuery qryOffense = new PLCQuery("SELECT LO.OFFENSE_CODE FROM TV_LABOFFENSE LO INNER JOIN TV_OFFENSE O ON LO.OFFENSE_CODE = O.OFFENSE_CODE WHERE LO.CASE_KEY = ? AND O.LEVEL_RES = 'F'");
            qryOffense.AddSQLParameter("CASE_KEY", PLCSession.PLCGlobalCaseKey);
            if (qryOffense.Open() && qryOffense.HasData())
                hasFelony = true;

            return hasFelony;
        }

        /// <summary>
        /// Check if case has a felony in any of its offense codes
        /// </summary>
        /// <returns>True if case has felony offense</returns>
        private bool HasFelonyCaseOffense()
        {
            bool hasFelony = false;

            PLCQuery qryCase = new PLCQuery("SELECT C.OFFENSE_CODE, C.OFFENSE_CODE_2, C.OFFENSE_CODE_3 FROM TV_LABCASE C WHERE C.CASE_KEY = " + PLCSession.PLCGlobalCaseKey);
            if (qryCase.Open() && qryCase.HasData())
            {
                List<string> lstOffenseCodes = new List<string>();

                if (!string.IsNullOrEmpty(qryCase.FieldByName("OFFENSE_CODE")))
                    lstOffenseCodes.Add(string.Format("'{0}'", qryCase.FieldByName("OFFENSE_CODE")));

                if (!string.IsNullOrEmpty(qryCase.FieldByName("OFFENSE_CODE_2")))
                    lstOffenseCodes.Add(string.Format("'{0}'", qryCase.FieldByName("OFFENSE_CODE_2")));

                if (!string.IsNullOrEmpty(qryCase.FieldByName("OFFENSE_CODE_3")))
                    lstOffenseCodes.Add(string.Format("'{0}'", qryCase.FieldByName("OFFENSE_CODE_3")));

                if (lstOffenseCodes.Count > 0)
                {
                    // CMPD: Felony is specified in LABCASE.OFFENSE_CATEGORY
                    if (PLCSession.GetLabCtrl("WEB_USES_ADVANCED_QC") == "C")
                        qryCase.SQL = "SELECT OFFENSE_CATEGORY FROM TV_LABCASE WHERE CASE_KEY = " + PLCSession.PLCGlobalCaseKey + " AND OFFENSE_CATEGORY = 'F'";
                    else
                        qryCase.SQL = "SELECT OFFENSE_CODE FROM TV_OFFENSE WHERE OFFENSE_CODE IN (" + string.Join(",", lstOffenseCodes) + ") AND LEVEL_RES = 'F'";

                    if (qryCase.Open() && qryCase.HasData())
                        hasFelony = true;
                }
            }

            return hasFelony;
        }

        private bool AllTabsValid()
        {
            var allValid = true;
            if (!bnAttribute.Disabled && !ItemAttribute.CanSave())
            {
                allValid = false;
            }

            if (!bnCurrency.Disabled && !Currency1.IsValidAmount())
            {
                allValid = false;
            }

            if (!bnNames.Disabled && tbNames.Attributes["required"] == "T")
            {
                if (gvNameItem.GetSelectedCheckboxes("chkNameItem").Count == 0)
                {
                    bnNames.Style.Add("color", "red");
                    allValid = false;
                }
                else
                {
                    bnNames.Style.Add("color", "");
                }
            }

            if (tbNames.Attributes["class"] == "active")
            {

            }
            return allValid;
        }

        private bool CreateCustodyRecordsForSubmissionItem()
        {
            bool analystCustodyAlreadyExists = false;
            if (!string.IsNullOrWhiteSpace(PLCSession.GetLabCtrl("SUBMISSION_CUSTODY_TYPE")) && !string.IsNullOrWhiteSpace(PLCDBPanel1.getpanelfield("LAB_CASE_SUBMISSION")))
            {
                PLCQuery qrySubmission = new PLCQuery(string.Format("SELECT SUBMISSION_KEY, SUBMISSION_TYPE, TRACKING_NUMBER, COMMENTS, RECEIVED_BY FROM TV_LABSUB WHERE CASE_KEY = {0} AND SUBMISSION_NUMBER = {1}",
                    PLCSession.PLCGlobalCaseKey, PLCDBPanel1.getpanelfield("LAB_CASE_SUBMISSION")));
                qrySubmission.Open();
                if (!qrySubmission.IsEmpty())
                {
                    //save custody records linked to the submission
                    PLCDBGlobal.instance.CreateCustodyRecordsForSubmissionItem(PLCSession.PLCGlobalCaseKey, qrySubmission.FieldByName("SUBMISSION_KEY"),
                        qrySubmission.FieldByName("SUBMISSION_TYPE"), qrySubmission.FieldByName("TRACKING_NUMBER"), qrySubmission.FieldByName("COMMENTS"),
                        PLCSession.PLCGlobalECN, PLCDBPanel1.getpanelfield("ITEM_TYPE"), null, null, null, null, false, true, qrySubmission.FieldByName("RECEIVED_BY"));
                    analystCustodyAlreadyExists = true;
                }
            }

            return analystCustodyAlreadyExists;
        }

        private bool GetItemNumber()
        {
            bool blnItemNumberUpdated = false;
            string itemNumber = "";

            if ((ViewState["SAMPLE_ITEM_NEW_CONTAINER"] != null) && (Convert.ToBoolean(ViewState["SAMPLE_ITEM_NEW_CONTAINER"]))) //Sample Item
            {
                PLCQuery qryParentItem = new PLCQuery();
                qryParentItem.SQL = "Select * from TV_LABITEM WHERE EVIDENCE_CONTROL_NUMBER = " + ViewState["ParentECN"];
                qryParentItem.Open();
                if (!qryParentItem.EOF())
                {
                    string newlabcasesub = qryParentItem.FieldByName("LAB_CASE_SUBMISSION");
                    itemNumber = PLCDBGlobal.instance.GetSampleItemNumber(qryParentItem.FieldByName("CASE_KEY"), qryParentItem.FieldByName("LAB_ITEM_NUMBER"), newlabcasesub);
                    blnItemNumberUpdated = true;
                }
            }
            else if (ViewState["DUPE_ITEM"] != null && Convert.ToBoolean(ViewState["DUPE_ITEM"]))//Dupe Item
            {
                PLCQuery qryItem = new PLCQuery();
                qryItem.SQL = "Select * from TV_LABITEM WHERE EVIDENCE_CONTROL_NUMBER = " + ViewState["ParentECN"];
                qryItem.Open();
                if (!qryItem.EOF())
                {
                    itemNumber = PLCDBGlobal.instance.GetSampleItemNumber(qryItem.FieldByName("CASE_KEY"), qryItem.FieldByName("LAB_ITEM_NUMBER"), qryItem.FieldByName("LAB_CASE_SUBMISSION"), true);
                    blnItemNumberUpdated = true;
                }
            }
            else
            {
                itemNumber = PLCDBGlobal.instance.GetNextItemNumber();
                if (itemNumber != string.Empty)
                    blnItemNumberUpdated = true;
            }

            if(blnItemNumberUpdated)
                PLCDBPanel1.setpanelfield("LAB_ITEM_NUMBER", itemNumber);

            return blnItemNumberUpdated;
        }

        private void GetDepartmentItemNumber()
        {
            if (ViewState["SAMPLE_ITEM_NEW_CONTAINER"] != null && Convert.ToBoolean(ViewState["SAMPLE_ITEM_NEW_CONTAINER"])) //Sample Item
            {
                PLCQuery qryParentItem = new PLCQuery("SELECT * FROM TV_LABITEM WHERE EVIDENCE_CONTROL_NUMBER = " + ViewState["ParentECN"]);
                if (qryParentItem.Open() && qryParentItem.HasData())
                {
                    string itemNumber = PLCDBGlobal.instance.GetSampleDeptItemNumber(qryParentItem.FieldByName("CASE_KEY"), qryParentItem.FieldByName("DEPARTMENT_ITEM_NUMBER"), qryParentItem.FieldByName("LAB_CASE_SUBMISSION"));
                    PLCDBPanel1.setpanelfield("DEPARTMENT_ITEM_NUMBER", itemNumber);
                }
            }
        }

        private void SetBarcodeIfDiffLocation(string ecnKey, string currentUser)
        {
            PLCQuery qryGetItemInfo = new PLCQuery(@"SELECT P.CUSTODY_OF AS PARENT_CUSTODY_OF, P.LOCATION AS PARENT_LOCATION 
FROM TV_LABITEM I 
INNER JOIN TV_LABITEM P ON I.PARENT_ECN = P.EVIDENCE_CONTROL_NUMBER 
WHERE I.EVIDENCE_CONTROL_NUMBER = " + ecnKey);

            qryGetItemInfo.Open();
            if (!qryGetItemInfo.IsEmpty())
            {
                string parentCustodyOf = qryGetItemInfo.FieldByName("PARENT_CUSTODY_OF");
                string parentLocation = qryGetItemInfo.FieldByName("PARENT_LOCATION");
                if ((parentCustodyOf != PLCSession.PLCGlobalDefaultAnalystCustodyOf) || (parentLocation != currentUser))
                {
                    PLCQuery qryUpdateItemBarcode = new PLCQuery("SELECT BARCODE FROM TV_LABITEM WHERE EVIDENCE_CONTROL_NUMBER = " + ecnKey);
                    qryUpdateItemBarcode.Open();
                    if (!qryUpdateItemBarcode.IsEmpty())
                    {
                        qryUpdateItemBarcode.Edit();
                        qryUpdateItemBarcode.SetFieldValue("BARCODE", ecnKey);
                        qryUpdateItemBarcode.Post("TV_LABITEM", 7, 1);
                    }
                }
            }
        }

        private DateTime GetNextReviewDate(DateTime bookDate)
        {
            DateTime reviewDate = bookDate.AddMonths(10);
            PLCQuery qryReviewDate = new PLCQuery(string.Format(
@"SELECT
    CASE
        WHEN IT.NARCO_ITEM = 'T'
            THEN 'T'
        ELSE ''
    END AS IS_NARCO_ITEM,
    CASE
        WHEN D.SPECIAL_VICTIM_UNIT = 'T'
            THEN 'T'
        ELSE '' 
    END AS IS_SVU,
    CASE
        WHEN D.HOMICIDE_UNIT = 'T'
            THEN 'T'
        ELSE ''
    END AS IS_HOMICIDE
FROM
    TV_LABCASE C LEFT OUTER JOIN
    TV_DEPTNAME D ON C.DEPARTMENT_CODE = D.DEPARTMENT_CODE INNER JOIN
    TV_LABITEM I ON C.CASE_KEY = I.CASE_KEY INNER JOIN
    TV_ITEMTYPE IT ON I.ITEM_TYPE = IT.ITEM_TYPE
WHERE
    I.EVIDENCE_CONTROL_NUMBER = {0}", PLCSession.PLCGlobalECN));

            if (qryReviewDate.Open() && qryReviewDate.HasData())
            {
                if (qryReviewDate.FieldByName("IS_NARCO_ITEM") == "T")
                {
                    reviewDate = bookDate.AddMonths(6);
                }
                else if (qryReviewDate.FieldByName("IS_SVU") == "T" || qryReviewDate.FieldByName("IS_HOMICIDE") == "T")
                {
                    reviewDate = bookDate.AddMonths(28);
                }
            }
            return reviewDate;
        }

        private bool CheckForChanges()
        {
            bool changesInTabs = false;

            // Check Main DBPanel
            changesInTabs = PLCDBPanel1.HasChanges(false);

            // Check Attributes Tab Changes
            List<string> attributesChanges;
            ViewState["AttributesChanges"] = null;
            if (!bnAttribute.Disabled && ItemAttribute.GetChanges(out attributesChanges))
            {
                ViewState["AttributesChanges"] = attributesChanges;
                changesInTabs = true;
            }

            // Check Currency Tab Changes
            List<string> currencyChanges;
            ViewState["CurrencyChanges"] = null;
            if (!bnCurrency.Disabled && Currency1.GetChanges(out currencyChanges))
            {
                ViewState["CurrencyChanges"] = currencyChanges;
                changesInTabs = true;
            }

            // Check Item Names Tab Changes
            List<string> itemNamesChanges;
            ViewState["ItemNamesChanges"] = null;
            if (CheckItemNameChanges(out itemNamesChanges))
            {
                ViewState["ItemNamesChanges"] = itemNamesChanges;
                changesInTabs = true;
            }

            return changesInTabs;
        }

        private bool CheckItemNameChanges(out List<string> itemNameChanges)
        {
            List<string> listOfChanges = new List<string>();
            Dictionary<string, string> nameList = new Dictionary<string, string>();
            Dictionary<string, string> origList = new Dictionary<string, string>();

            PLCQuery qryItemNames = new PLCQuery(GetItemNamesSQL());

            // COUNT, NAME_KEY, RELATION, DESCRIPTION
            if (qryItemNames.Open() && qryItemNames.HasData())
            {
                while (!qryItemNames.EOF())
                {
                    if (Convert.ToInt32(qryItemNames.FieldByName("COUNT")) == 1)
                    {
                        origList.Add(qryItemNames.FieldByName("NAME_KEY"), qryItemNames.FieldByName("RELATION"));
                    }
                    nameList.Add(qryItemNames.FieldByName("NAME_KEY"), qryItemNames.FieldByName("DESCRIPTION"));
                    qryItemNames.Next();
                }
            }

            foreach (GridViewRow row in gvNameItem.Rows)
            {
                CheckBox cbItemName = (CheckBox)row.FindControl("chkNameItem");
                DropDownList ddlItemName = (DropDownList)row.FindControl("ddlRelation" + row.RowIndex);

                string key = row.Cells[NAMEITEM_GRID_COL_NAMEKEY].Text.Trim();
                string name;
                string relCode = GetRelationCode(ddlItemName.Text);

                if (cbItemName != null && cbItemName.Checked)
                {
                    if (origList.ContainsKey(key)) // May or may not be a relation update
                    {
                        if (origList[key].Trim().Length > 0 && origList[key].Trim() != relCode.Trim())
                        {
                            listOfChanges.Add("NAME LINK CHANGED");
                            listOfChanges.Add("NAME: " + (nameList.TryGetValue(key, out name) ? name : "") + " FROM: " +
                                (string.IsNullOrEmpty(origList[key]) ? "-" : GetRelationDesc(origList[key])) + " TO: " + ddlItemName.Text);
                        }
                    }
                }
                else
                {
                    if (origList.ContainsKey(key)) // Delete Item Name Record
                    {
                        listOfChanges.Add("REMOVED NAME LINK");
                        listOfChanges.Add("NAME: " + (nameList.TryGetValue(key, out name) ? name : "") + " RELATION: " + (string.IsNullOrEmpty(origList[key]) ? "-" : GetRelationDesc(origList[key])));
                    }
                }
            }

            itemNameChanges = listOfChanges;

            return listOfChanges.Count > 0;
        }

        private bool ItemNameHasChanges()
        {
            Dictionary<string, string> origList = new Dictionary<string, string>();
            PLCQuery qryItemNames = new PLCQuery(GetItemNamesSQL());

            // COUNT, NAME_KEY, RELATION, DESCRIPTION
            if (qryItemNames.Open() && qryItemNames.HasData())
            {
                while (!qryItemNames.EOF())
                {
                    if (Convert.ToInt32(qryItemNames.FieldByName("COUNT")) == 1)
                        origList.Add(qryItemNames.FieldByName("NAME_KEY"), qryItemNames.FieldByName("RELATION"));
                    qryItemNames.Next();
                }
            }

            foreach (GridViewRow row in gvNameItem.Rows)
            {
                CheckBox cbItemName = (CheckBox)row.FindControl("chkNameItem");
                DropDownList ddlItemName = (DropDownList)row.FindControl("ddlRelation" + row.RowIndex);
                string key = row.Cells[NAMEITEM_GRID_COL_NAMEKEY].Text.Trim();
                string relCode = GetRelationCode(ddlItemName.Text);

                if (cbItemName != null && cbItemName.Checked)
                {
                    if (origList.ContainsKey(key))
                    {
                        if (origList[key].Trim() != relCode.Trim())
                            return true;
                    }
                    else
                        return true;
                }
                else
                {
                    if (origList.ContainsKey(key)) // Delete Item Name Record
                        return true;
                }
            }
            return false;
        }

        private string PrepareCaseNarrative()
        {
            StringBuilder auditText = new StringBuilder();

            auditText.AppendLine(string.Format("ITEM #: {0} - ECN: {1}",
                PLCDBPanel1.getpanelfield("LAB_ITEM_NUMBER"), PLCSession.PLCGlobalECN));

            // Main Panel Changes
            foreach (string fieldName in PLCDBPanel1.GetFieldNames())
            {
                string fieldType = PLCDBPanel1.GetPanelRecByFieldName(fieldName).fldtype;
                string originalValue = PLCDBPanel1.GetOriginalValue(fieldName);
                string newValue = PLCDBPanel1.getpanelfield(fieldName);
                string editmask = PLCDBPanel1.GetPanelRecByFieldName(fieldName).editmask;

                if (fieldType == "D")
                {
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
                    auditText.AppendLine(PLCDBPanel1.GetPanelRecByFieldName(fieldName).prompt + " (" + fieldName + ") CHANGED");
                    if (fieldType == "D")
                        auditText.AppendLine("FROM: " + (originalValue == "" ? "-" : originalValue) +
                        " TO: " + (newValue == "" ? "-" : newValue));
                    else
                        auditText.AppendLine("FROM: " + (originalValue == "" ? "-" : PLCDBPanel1.GetOriginalDesc(fieldName)) +
                        " TO: " + (newValue == "" ? "-" : PLCDBPanel1.GetFieldDesc(fieldName)));
                }
            }

            // Attributes Changes
            if (ViewState["AttributesChanges"] != null && ((List<string>)ViewState["AttributesChanges"]).Count > 0)
            {
                auditText.AppendLine("ITEM ATTRIBUTE CHANGES");

                foreach (string attribChange in (List<string>)ViewState["AttributesChanges"])
                {
                    auditText.AppendLine(attribChange);
                }

                ViewState["AttributesChanges"] = null;
            }

            // Currency Changes
            if (ViewState["CurrencyChanges"] != null && ((List<string>)ViewState["CurrencyChanges"]).Count > 0)
            {
                auditText.AppendLine("CURRENCY CHANGES");

                foreach (string currChange in (List<string>)ViewState["CurrencyChanges"])
                {
                    auditText.AppendLine(currChange);
                }

                ViewState["CurrencyChanges"] = null;
            }

            // Item Names Changes
            if (ViewState["ItemNamesChanges"] != null && ((List<string>)ViewState["ItemNamesChanges"]).Count > 0)
            {
                auditText.AppendLine("NAME LINK CHANGES");

                foreach (string itemNamesChange in (List<string>)ViewState["ItemNamesChanges"])
                {
                    auditText.AppendLine(itemNamesChange);
                }

                ViewState["ItemNamesChanges"] = null;
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
        /// <summary>
        /// Checks if Item Type selected is a currency item type
        /// </summary>
        /// <param name="itemType">Selected item type from the DBPANEL</param>
        /// <returns>True or False</returns>
        private bool AllowCurrencyEntry(string itemType)
        {
            //show currency tab by default
            if (!string.IsNullOrEmpty(itemType) && PLCSession.GetLabCtrl("USES_CURRENCY_ENTRY") == "T")
            {
                PLCQuery qryItemType = new PLCQuery();
                qryItemType.SQL = "Select ALLOW_CURRENCY_ENTRY from TV_ITEMTYPE Where ITEM_TYPE = '" + itemType + "'";
                qryItemType.Open();
                if (!qryItemType.EOF())
                    return qryItemType.FieldByName("ALLOW_CURRENCY_ENTRY") == "T";
            }

            return false;
        }

        /// <summary>
        /// Save values from Currency, Attributes and Names tabs
        /// </summary>
        private void SaveTabs()
        {
            if (AllowCurrencyEntry(PLCDBPanel1.getpanelfield("ITEM_TYPE")))
            {
                Currency1.SaveData(PLCDBPanel1.ReasonChangeKey);
            }

            ItemAttribute.SavePanel(PLCDBPanel1.ReasonChangeKey);
            SaveItemNames(PLCDBPanel1.ReasonChangeKey);
        }

        private void UpdateBiologicalCourierDestination()
        {
            //if BIOLOGICAL_COURIER_DEST code exists for the lab
            if (!string.IsNullOrEmpty(PLCSession.GetLabCtrl("BIOLOGICAL_COURIER_DEST")))
            {
                //if SEIZED_FOR_BIOLOGY field has been changed
                if (PLCDBPanel1.getpanelfield("SEIZED_FOR_BIOLOGY") != PLCDBPanel1.GetOriginalValue("SEIZED_FOR_BIOLOGY"))
                {
                    //if SEIZED_FOR_BIOLOGY is YES
                    if (PLCDBPanel1.getpanelfield("SEIZED_FOR_BIOLOGY") == "Y")
                    {
                        UpdateDeliverInfo(PLCSession.GetLabCtrl("BIOLOGICAL_COURIER_DEST"), PLCSession.PLCGlobalAnalyst, DateTime.Now.ToShortDateString());
                    }
                    //if SEIZED_FOR_BIOLOGY is changed from YES to NO
                    else if (PLCDBPanel1.getpanelfield("SEIZED_FOR_BIOLOGY") == "N" && PLCDBPanel1.GetOriginalValue("SEIZED_FOR_BIOLOGY") == "Y")
                    {
                        PLCQuery qryDelivery = new PLCQuery();
                        qryDelivery.SQL = "SELECT DELIVERY_REQUESTED_BY, DELIVERY_REQUEST_DATE, DELIVER_TO_DEPARTMENT, (SELECT COUNT(EVIDENCE_CONTROL_NUMBER) FROM TV_SRDETAIL WHERE EVIDENCE_CONTROL_NUMBER = " + PLCSession.PLCGlobalECN + ") AS SERVICE_REQUEST_COUNT FROM TV_LABITEM WHERE EVIDENCE_CONTROL_NUMBER = " + PLCSession.PLCGlobalECN;
                        qryDelivery.Open();
                        if (!qryDelivery.IsEmpty())
                        {
                            if (qryDelivery.FieldByName("DELIVERY_REQUESTED_BY") == PLCSession.PLCGlobalAnalyst //if the logged on user is the same user as "Delivery Requested By"
                                && qryDelivery.FieldByName("DELIVER_TO_DEPARTMENT") == PLCSession.GetLabCtrl("BIOLOGICAL_COURIER_DEST") //and it is the same date as "Delivery Request Date" 
                                && Convert.ToDateTime(qryDelivery.FieldByName("DELIVERY_REQUEST_DATE")).ToShortDateString() == DateTime.Now.ToShortDateString() //and if "Deliver To Department" is the same as "Biological Courier Dest" 
                                && qryDelivery.FieldByName("SERVICE_REQUEST_COUNT") == "0") //and if there are no service requests for the item
                            {
                                qryDelivery.Edit();
                                qryDelivery.SetFieldValue("DELIVERY_REQUESTED_BY", null);
                                qryDelivery.SetFieldValue("DELIVERY_REQUEST_DATE", null);
                                qryDelivery.SetFieldValue("DELIVER_TO_DEPARTMENT", null);
                                qryDelivery.Post("TV_LABITEM", 7000, 12);
                            }
                        }
                    }
                }
            }
        }

        private void UpdateITAssignDescription(string ECN, string oldDescription, string newDescription) // Ticket # 22449
        {
            if (newDescription.ToLower() != oldDescription.ToLower())
            {
                //For all the open assignments (Status assigned to Analyst – 1) that are linked to this item
                //and TV_ITASSIGN.ITA_ITEM_DESCRIPTION is blank
                //or TV_ITASSIGN.ITA_ITEM_DESCRIPTION matches with the old description
                //then update TV_ITASSING.ITA_ITEM_DESCRIPTION with the new description

                PLCQuery qryITAssign = new PLCQuery(string.Format(@"SELECT IA.EXAM_KEY, ITA_ITEM_DESCRIPTION FROM TV_ITASSIGN IA
INNER JOIN TV_LABASSIGN A ON IA.EXAM_KEY = A.EXAM_KEY
WHERE STATUS = 1 AND EVIDENCE_CONTROL_NUMBER = {0}", ECN));
                qryITAssign.Open();
                while (!qryITAssign.EOF())
                {
                    if (string.IsNullOrEmpty(qryITAssign.FieldByName("ITA_ITEM_DESCRIPTION")) || qryITAssign.FieldByName("ITA_ITEM_DESCRIPTION").Trim() == oldDescription.Trim())
                    {
                        PLCQuery qryReportDesc = new PLCQuery(string.Format("SELECT ITA_ITEM_DESCRIPTION FROM TV_ITASSIGN WHERE EVIDENCE_CONTROL_NUMBER = {0} AND EXAM_KEY = {1}",
                            ECN, qryITAssign.FieldByName("EXAM_KEY")));
                        qryReportDesc.Open();
                        if (qryReportDesc.HasData())
                        {
                            qryReportDesc.Edit();
                            qryReportDesc.SetFieldValue("ITA_ITEM_DESCRIPTION", newDescription);
                            qryReportDesc.Post("TV_ITASSIGN", 7, 1);
                        }
                    }

                    qryITAssign.Next();
                }
            }
        }

        private void Create_Default_Service_Request(string ecn)
        {
            string TheDeptCode = "";
            string TheItemType = "";
            string TheTaskType = "";
            string TheLabCode = "";
            string DefaultPriority = "";

            DefaultPriority = PLCSession.GetLabCtrl("DEFAULT_PRIORITY");

            //Position the labitem table
            PLCQuery qryLabItem = new PLCQuery();
            qryLabItem.SQL = "SELECT * from TV_LABITEM Where EVIDENCE_CONTROL_NUMBER = " + ecn;
            qryLabItem.Open();

            TheItemType = qryLabItem.FieldByName("ITEM_TYPE");

            //Position the labcase table
            PLCQuery qryCase = new PLCQuery();
            qryCase.SQL = "Select DEPARTMENT_CODE from TV_LABCASE where CASE_KEY = " + qryLabItem.FieldByName("CASE_KEY");
            qryCase.Open();

            //Position the labsubtable
            PLCQuery qryLabSub = new PLCQuery();
            qryLabSub.SQL = "SELECT DEPARTMENT_CODE from TV_LABSUB where CASE_KEY = " + qryLabItem.FieldByName("CASE_KEY") + " AND SUBMISSION_NUMBER = " + qryLabItem.FieldByName("LAB_CASE_SUBMISSION");
            qryLabSub.Open();

            TheDeptCode = qryCase.FieldByName("DEPARTMENT_CODE");
            if (qryLabSub.FieldByName("DEPARTMENT_CODE") != "") TheDeptCode = qryLabSub.FieldByName("DEPARTMENT_CODE");


            //Go to the lookup table to find the default service request (task Type) for the ITEM
            PLCQuery qryDSQ = new PLCQuery();
            qryDSQ.SQL = "SELECT * FROM TV_DSERVREQ where DEPARTMENT_CODE = '" + TheDeptCode + "' AND ITEM_TYPE = '" + TheItemType + "'";
            qryDSQ.Open();
            if (qryDSQ.IsEmpty()) return;

            TheTaskType = qryDSQ.FieldByName("SERVICE_REQUEST");
            TheLabCode = qryDSQ.FieldByName("SERVICE_LAB");

            if (TheTaskType == "")
            {
                PLCSession.ClearError();
                PLCSession.PLCErrorURL = "TAB4ITEMS.ASPX";
                PLCSession.PLCErrorProc = "CREATE_DEFAULT_SERVICE_REQUEST";
                PLCSession.PLCErrorSQL = qryDSQ.SQL;
                PLCSession.PLCErrorMessage = "Task Type is Undefined IN DSERVREQ";
                PLCSession.SaveError();
                Response.Redirect("TAB4ITEMS.ASPX");
                return;
            }

            if (TheLabCode == "")
            {
                PLCSession.ClearError();
                PLCSession.PLCErrorURL = "TAB4ITEMS.ASPX";
                PLCSession.PLCErrorProc = "CREATE_DEFAULT_SERVICE_REQUEST";
                PLCSession.PLCErrorSQL = qryDSQ.SQL;
                PLCSession.PLCErrorMessage = "LAB_CODE is Undefined IN DSERVREQ";
                PLCSession.SaveError();
                Response.Redirect("TAB4ITEMS.ASPX");
                return;
            }

            if (TheLabCode == "") return;

            PLCQuery qryTaskType = new PLCQuery();
            qryTaskType.SQL = "SELECT * FROM TV_TASKTYPE where TASK_TYPE = '" + TheTaskType + "'";
            qryTaskType.Open();
            if (qryTaskType.IsEmpty())
            {
                PLCSession.ClearError();
                PLCSession.PLCErrorURL = "TAB4ITEMS.ASPX";
                PLCSession.PLCErrorProc = "CREATE_DEFAULT_SERVICE_REQUEST";
                PLCSession.PLCErrorSQL = qryTaskType.SQL;
                PLCSession.PLCErrorMessage = "Task Type not found in TV_TASKTYPE.  You must specify a valid TASK_TYPE IN DSERVREQ";
                PLCSession.SaveError();
                Response.Redirect("TAB4ITEMS.ASPX");
                return;
            }
        }

        /// <summary>
        /// Load Currency, Attributes and Names tab values
        /// </summary>
        private void RefreshMultiview()
        {
            ItemAttribute.CreateControls();
            LoadCurrencyData();
            if (!ItemAttribute.IsItemTypeChanged)
                LoadNames();

            //if in add mode or in edit mode but no saved attributes, show new attributes based on itemtype
            ItemAttribute.CurrentItemType = PLCDBPanel1.getpanelfield("ITEM_TYPE");
            ItemAttribute.CreateControls();
            bnAttribute.Disabled = false;

            if (AllowCurrencyEntry(PLCDBPanel1.getpanelfield("ITEM_TYPE")))
            {
                //show currency tab               
                bnCurrency.Disabled = false;
            }
            else
            {               
                bnCurrency.Disabled = true;
            }
        }

        private string GetItemNamesSQL()
        {
            string sqlstr = "SELECT NAME_KEY, '(' || TV_LABNAME.NAME_TYPE || ') ' || TV_LABNAME.LAST_NAME  || ', ' || NVL(TV_LABNAME.FIRST_NAME, '') Description,";

            if (ViewState["NewRecordECN"] != null)
            {
                sqlstr += "(SELECT COUNT(*) FROM TV_ITEMNAME WHERE EVIDENCE_CONTROL_NUMBER = " +
                    ViewState["NewRecordECN"].ToString() + " AND NAME_KEY = TV_LABNAME.NAME_KEY) Count, " +
                    "(SELECT RELATION_CODE FROM TV_ITEMNAME WHERE EVIDENCE_CONTROL_NUMBER = " +
                    ViewState["NewRecordECN"].ToString() + " AND NAME_KEY = TV_LABNAME.NAME_KEY) Relation ";
            }
            else if (GridView1.SelectedDataKey != null)
            {
                sqlstr += "(SELECT count(*) FROM TV_ITEMNAME WHERE EVIDENCE_CONTROL_NUMBER = " +
                    GridView1.SelectedDataKey.Value.ToString() + " AND NAME_KEY = TV_LABNAME.NAME_KEY) Count, " +
                    "(SELECT RELATION_CODE FROM TV_ITEMNAME WHERE EVIDENCE_CONTROL_NUMBER = " +
                    GridView1.SelectedDataKey.Value.ToString() + " AND NAME_KEY = TV_LABNAME.NAME_KEY) Relation ";
            }
            else
            {
                sqlstr += "0 AS Count, ";
                sqlstr += "'' AS Relation ";
            }

            sqlstr += "FROM TV_LABNAME WHERE TV_LABNAME.CASE_KEY = " + PLCSession.PLCGlobalCaseKey;

            return PLCSession.FormatSpecialFunctions(sqlstr);
        }

        private void LoadNames()
        {
            PLCQuery qryItemNamesRel = new PLCQuery(GetItemNamesSQL());

            if (qryItemNamesRel.OpenReadOnly() && qryItemNamesRel.HasData())
            {
                gvNameItem.DataSource = qryItemNamesRel.PLCDataTable;
                gvNameItem.DataBind();
            }
        }

        private void LoadCurrencyData()
        {
            if (!String.IsNullOrEmpty(PLCSession.PLCGlobalECN)) // when nothing is selected in gridview, PLCGlobalECN is null.
            {
                Currency1.LoadData();
            }
        }

        private void LoadItemTypeAttr()
        {
            string sItemType = "";
            PLCQuery qryLabItemItemType = new PLCQuery();
            qryLabItemItemType.SQL = "select ITEM_TYPE from TV_LABITEM Where EVIDENCE_CONTROL_NUMBER = " + PLCSession.PLCGlobalECN;
            if (qryLabItemItemType.Open())
                sItemType = qryLabItemItemType.FieldByName("ITEM_TYPE");

            if (sItemType != "")
            {
                string sAllowCurrencyEntry = "";
                PLCQuery qryItemType = new PLCQuery();
                qryItemType.SQL = "Select ALLOW_CURRENCY_ENTRY from TV_ITEMTYPE Where ITEM_TYPE = '" + sItemType + "'";
                qryItemType.Open();
                if (!qryItemType.IsEmpty())
                    sAllowCurrencyEntry = qryItemType.FieldByName("ALLOW_CURRENCY_ENTRY");

                if (sAllowCurrencyEntry == "T")
                {
                    LoadCurrencyData();
                }
            }
        }

        private void AppendSubLinkItem(string ECN, string submissionNumber, bool resubmission)
        {
            PLCQuery qryLabSub = new PLCQuery();
            qryLabSub.SQL = "Select SUBMISSION_KEY from TV_LABSUB where CASE_KEY = " + PLCSession.PLCGlobalCaseKey + " and " +
                            "SUBMISSION_NUMBER = " + submissionNumber;
            if (qryLabSub.Open() && (!qryLabSub.EOF()))
            {
                string subkey = qryLabSub.FieldByName("SUBMISSION_KEY");

                PLCQuery qrySubLink = new PLCQuery();
                qrySubLink.SQL = "Select * from TV_SUBLINK where 1 = 0";
                qrySubLink.Open();
                qrySubLink.Append();
                qrySubLink.SetFieldValue("SUBMISSION_KEY", subkey);
                qrySubLink.SetFieldValue("EVIDENCE_CONTROL_NUMBER", ECN);
                qrySubLink.SetFieldValue("RESUBMISSION", resubmission ? "T" : "F");
                qrySubLink.Post("TV_SUBLINK", 7000, 17);

                //update submission as Narcotics Review Pending if the item type added requires a Narcotics Review
                if (PLCSession.IsNarcoticsReviewRequired(PLCDBPanel1.getpanelfield("ITEM_TYPE")))
                    UpdateSubmissionAsPending(subkey);
            }
        }

        private void DeleteSubLinkItem(string ecn)
        {
            PLCQuery qrySubItem = new PLCQuery("DELETE FROM TV_SUBLINK WHERE EVIDENCE_CONTROL_NUMBER = " + ecn + " AND (RESUBMISSION = 'F' OR RESUBMISSION IS NULL)");
            qrySubItem.ExecSQL();
        }

        private void UpdateSubmissionAsPending(string submissionKey)
        {
            PLCQuery qrySubmission = new PLCQuery();
            qrySubmission.SQL = "SELECT NARCOTICS_REVIEW FROM TV_LABSUB WHERE SUBMISSION_KEY = " + submissionKey;
            qrySubmission.Open();
            if (!qrySubmission.IsEmpty())
            {
                qrySubmission.Edit();
                qrySubmission.SetFieldValue("NARCOTICS_REVIEW", "P");
                qrySubmission.Post("TV_LABSUB", 8, 1);
            }
        }

        private void AddItemSortToLabItem(string ecn, string labItemNum, string deptItemNum)
        {
            PLCQuery qryLabItem = new PLCQuery();
            qryLabItem.SQL = String.Format("UPDATE TV_LABITEM SET ITEM_SORT = '{0}' WHERE EVIDENCE_CONTROL_NUMBER = {1}", PLCCommon.instance.GetItemSort(labItemNum), ecn);
            qryLabItem.ExecSQL("");

            if (PLCSession.GetLabCtrl("DUPE_DEPT_ITEM_NUM_ON_SAMPLE") == "T")
            {
                qryLabItem.SQL = String.Format("UPDATE TV_LABITEM SET DEPT_ITEM_SORT = '{0}' WHERE EVIDENCE_CONTROL_NUMBER = {1}", PLCCommon.instance.GetItemSort(deptItemNum), ecn);
                qryLabItem.ExecSQL("");
            }
        }

        private string CheckCurrency(string ECN)
        {
            PLCQuery qryCurrency = new PLCQuery();
            qryCurrency.SQL = "select * from TV_CURRENCY Where EVIDENCE_CONTROL_NUMBER = " + ECN;
            qryCurrency.Open();
            if (!qryCurrency.IsEmpty())
            {
                return Math.Round(Convert.ToDecimal("0.00") + Convert.ToDecimal(string.IsNullOrEmpty(qryCurrency.FieldByName("CURRENT_VALUE")) ? "0.00" : qryCurrency.FieldByName("CURRENT_VALUE")), 2, MidpointRounding.AwayFromZero).ToString();
            }
            else
                return "";
        }

        private void GetRelationItems()
        {
            ArrayList ArrayITNAMERL = new ArrayList();
            TRelation recInit = new TRelation();
            recInit.Code = "NONE";
            recInit.Description = "(None)";
            ArrayITNAMERL.Add(recInit);

            PLCQuery qryItNameRl = CacheHelper.OpenCachedSqlReadOnly("Select * from TV_ITNAMERL");
            while (!qryItNameRl.EOF())
            {
                TRelation recITNAMERL = new TRelation();
                recITNAMERL.Code = qryItNameRl.FieldByName("CODE");
                recITNAMERL.Description = qryItNameRl.FieldByName("DESCRIPTION");
                ArrayITNAMERL.Add(recITNAMERL);
                qryItNameRl.Next();
            }

            ViewState.Add("ITEM_NAME_RELATION", ArrayITNAMERL);
        }

        private string GetRelationCode(string sDesc)
        {
            ArrayList ArrayITNAMERL = (ArrayList)ViewState["ITEM_NAME_RELATION"];
            for (int i = 0; i < ArrayITNAMERL.Count; i++)
            {
                if (sDesc == ((TRelation)ArrayITNAMERL[i]).Description)
                {
                    return ((TRelation)ArrayITNAMERL[i]).Code;
                }
            }
            return "";
        }

        private string GetRelationDesc(string sCode)
        {
            ArrayList ArrayITNAMERL = (ArrayList)ViewState["ITEM_NAME_RELATION"];
            for (int i = 0; i < ArrayITNAMERL.Count; i++)
                if (sCode == ((TRelation)ArrayITNAMERL[i]).Code)
                    return ((TRelation)ArrayITNAMERL[i]).Description;
            return "";
        }

        private int GetRelationIndex(string sCode)
        {
            ArrayList ArrayITNAMERL = (ArrayList)ViewState["ITEM_NAME_RELATION"];
            for (int i = 0; i < ArrayITNAMERL.Count; i++)
            {
                if (sCode == ((TRelation)ArrayITNAMERL[i]).Code)
                {
                    return i;
                }
            }
            return -1;
        }

        private void SaveItemNames(int changeKey)
        {
            // first delete records
            PLCQuery qryItnamerl = new PLCQuery();
            qryItnamerl.SQL = "Delete from TV_ITEMNAME WHERE EVIDENCE_CONTROL_NUMBER = " + PLCSession.PLCGlobalECN;
            qryItnamerl.ExecSQL(PLCSession.GetConnectionString());

            foreach (GridViewRow gr in gvNameItem.Rows)
            {
                CheckBox chk = (CheckBox)gr.FindControl("chkNameItem");
                if ((chk != null) && (chk.Checked))
                {
                    qryItnamerl.SQL = "Select * from TV_ITEMNAME WHERE 1 = 0";
                    qryItnamerl.Open();
                    qryItnamerl.Append();
                    int nextseq = PLCSession.GetNextSequence("ITEMNAME_SEQ");
                    qryItnamerl.SetFieldValue("LINK_KEY", nextseq);
                    qryItnamerl.SetFieldValue("NAME_KEY", gr.Cells[NAMEITEM_GRID_COL_NAMEKEY].Text);
                    qryItnamerl.SetFieldValue("EVIDENCE_CONTROL_NUMBER", PLCSession.PLCGlobalECN);
                    DropDownList ddl = (DropDownList)gr.FindControl("ddlRelation" + gr.RowIndex);
                    if ((ddl != null) && (ddl.SelectedIndex > 0))
                    {
                        qryItnamerl.SetFieldValue("RELATION_CODE", GetRelationCode(ddl.Text));
                    }
                    qryItnamerl.Post("TV_ITEMNAME", 6, 10, changeKey);
                }
            }

            NameBrowseMode(true);
        }

        private void NameBrowseMode(bool isBrowseMode)
        {
            gvNameItem.Enabled = !isBrowseMode && gvNameItem.Rows.Count > 0;
        }

        private Boolean validSubmission(string subnum)
        {
            if (string.IsNullOrWhiteSpace(subnum))
                return false;

            PLCQuery qry = new PLCQuery();
            qry.SQL = "SELECT COUNT(*) THECOUNT FROM TV_LABSUB where CASE_KEY = :CASEKEY and SUBMISSION_NUMBER = :SUBNUM";
            qry.SetParam("CASEKEY", PLCSession.PLCGlobalCaseKey);
            qry.SetParam("SUBNUM", subnum);
            qry.Open();
            if (!qry.IsEmpty())
            {
                if (qry.iFieldByName("THECOUNT") > 0)
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }

        private string GetDeptItemNumPattern(string TheCaseKey, string SubmNum, string S)
        {
            string HStr;
            string SampleStr;
            PLCDBGlobal itemNumFunctions = new PLCDBGlobal();

            PLCQuery qryItemRunnerC = new PLCQuery();
            qryItemRunnerC.SQL = "SELECT * FROM TV_LABITEM WHERE CASE_KEY = " + TheCaseKey + " ORDER BY CASE_KEY, ITEM_SORT";
            qryItemRunnerC.Open();
            HStr = "";
            while (!qryItemRunnerC.EOF())
            {
                if (S != "")
                {
                    string lin = qryItemRunnerC.FieldByName("DEPARTMENT_ITEM_NUMBER");
                    try
                    {
                        if ((S.Length <= lin.Length) && (S == lin.Substring(0, S.Length)))
                        {
                            SampleStr = itemNumFunctions.GetSampleStr(S, qryItemRunnerC.FieldByName("DEPARTMENT_ITEM_NUMBER"));
                            try
                            {
                                if (Convert.ToInt32(SampleStr) > Convert.ToInt32(HStr))
                                    HStr = SampleStr;
                            }
                            catch
                            {
                                HStr = SampleStr;
                            }
                        }
                    }
                    catch
                    { }

                }
                qryItemRunnerC.Next();
            }

            if (HStr == "")
                HStr = "0";

            return S + HStr;

        }

        private string GetSampleDepartmentItemNumber(string ThisCaseKey, string ThisItemNumber, string ThisItemSort, string SubmNumber)
        {
            string newItm = "";
            PLCCommon common = new PLCCommon();

            newItm = common.IncStr(GetDeptItemNumPattern(ThisCaseKey, SubmNumber, ThisItemNumber + PLCSession.GetLabCtrl("SAMPLE_CHAR")), 3, 0);
            return newItm;
        }

        private bool IsItemProcessHOLD()
        {
            if (!string.IsNullOrEmpty(PLCSession.PLCGlobalECN) && PLCSession.PLCGlobalECN != "0")
            {
                PLCQuery qryItemProcess = new PLCQuery();
                qryItemProcess.SQL = "SELECT PROCESS FROM TV_LABITEM WHERE EVIDENCE_CONTROL_NUMBER=" + PLCSession.PLCGlobalECN;
                qryItemProcess.Open();
                if (!qryItemProcess.IsEmpty())
                {
                    if (qryItemProcess.FieldByName("PROCESS").IndexOf("HOLD") > -1)
                        return true;
                }
            }

            return false;
        }

        private void UpdateDeliverInfo(string departmentCode, string requestedBy, string requestDate)
        {
            PLCQuery qryUpdateDeliverInfo = new PLCQuery();
            qryUpdateDeliverInfo.SQL = "SELECT DELIVER_TO_DEPARTMENT,DELIVERY_REQUESTED_BY,DELIVERY_REQUEST_DATE FROM TV_LABITEM WHERE EVIDENCE_CONTROL_NUMBER=" + PLCSession.PLCGlobalECN;
            qryUpdateDeliverInfo.Open();
            if (!qryUpdateDeliverInfo.IsEmpty())
            {
                qryUpdateDeliverInfo.Edit();
                qryUpdateDeliverInfo.SetFieldValue("DELIVER_TO_DEPARTMENT", departmentCode);
                qryUpdateDeliverInfo.SetFieldValue("DELIVERY_REQUESTED_BY", requestedBy);
                qryUpdateDeliverInfo.SetFieldValue("DELIVERY_REQUEST_DATE", requestDate);
                qryUpdateDeliverInfo.Post("TV_LABITEM", 7000, 12);
            }
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

        private void GetSubmExtraInfoHighlight(GridViewRowEventArgs Item)
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
                            if (item == GridView1.DataKeys[Item.Row.RowIndex].Value.ToString())
                            {
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
                                ViewState["SUBM_EXTRA_INFO_TABLE"] = ECN;
                            }
                        }
                    }
                }
            }
        }

        private string GetSubmissionCommandVoucher(string sCaseKey, string sSubNum)
        {
            PLCQuery qryLabSub = new PLCQuery();
            qryLabSub.SQL = "SELECT COMMAND_VOUCHER FROM TV_LABSUB WHERE CASE_KEY = " + sCaseKey + " AND SUBMISSION_NUMBER = " + sSubNum;
            qryLabSub.Open();
            if (!qryLabSub.IsEmpty())
                return qryLabSub.FieldByName("COMMAND_VOUCHER");
            else
                return "";
        }

        private void SetPageBrowseMode(bool isBrowseMode)
        {
            lPLCLockStatus.Text = "";
            trPLCLockStatus.Visible = false;

            //grid
            GridView1.SetControlMode(isBrowseMode);

            if (isBrowseMode)
            {
                //panel
                PLCDBPanel1.SetBrowseMode();
                //panelbuttons
                PLCButtonPanel1.SetBrowseMode();
                //currency tab
                Currency1.SetBrowseMode();
                //route item tab

                //gridbuttons
                if (GridView1.Rows.Count > 0)
                {
                    EnableButtonControls(true, false);
                }
                else
                {
                    PLCSession.PLCGlobalECN = "";
                    PLCDBPanel1.EmptyMode();
                    PLCButtonPanel1.SetEmptyMode();
                    EnableButtonControls(false, false);
                }
            }
            else
            {
                //panel
                PLCDBPanel1.SetEditMode();
                //panelbuttons
                PLCButtonPanel1.SetEditMode();

                //currency tab
                Currency1.SetEditMode();

                //gridbuttons
                EnableButtonControls(false, false);

                // If Item Num is never editable, make it readonly.
                if (PLCSession.CheckUserOption("NOEDITNUM"))
                    PLCDBPanel1.SetMyFieldMode("LAB_ITEM_NUMBER", true);
            }

            //attribute tab
            ItemAttribute.IsBrowseMode = isBrowseMode;

            //names tab
            NameBrowseMode(isBrowseMode);

            //tabs
            //bnAttribute.Enabled = bnCurrency.Enabled = bnNames.Enabled = isBrowseMode;

            //
            CheckUserOptionReadOnlyAccess();
        }

        /// <summary>
        /// Called on change of Item Type
        /// This displays a confirmation pop up if there are saved item attribute and currency values
        /// </summary>
        /// <returns>returns true if a confirm message will be shown</returns>
        private bool DoUpdateItemAttributes()
        {
            bool itemTypeChanged = PLCDBPanel1.getpanelfield("ITEM_TYPE") != PLCDBPanel1.GetOriginalValue("ITEM_TYPE")
                        ? true
                        : PLCDBPanel1.IsNewRecord
                            ? PLCDBPanel1.GetOriginalValue("ITEM_TYPE") == "" //true when item type is blanked out while adding a record
                            : false;

            if (!itemTypeChanged) itemTypeChanged = (PLCSession.PLCGlobalECN == "0" || PLCSession.PLCGlobalECN == "") && PLCDBPanel1.IsNewRecord;

            //if itemtype changed
            //  if in add mode
            //      no prompt, recreate attributes, no saved attributes to delete on savebutton click
            //  if in edit mode and no saved attributes
            //      no prompt, recreate attributes, no saved attributes to delete on savebutton click
            //  if in edit mode but with saved attributes
            //      prompt confirmation, delete? yes or no
            //          if yes
            //              recreate attributes
            //              delete saved attributes on savebutton click
            //          if no
            //              revert back to original itemtype
            //if itemtype is reverted back to original by the user
            //  no prompt, recreate attributes with saved ones
            //  do not delete saved attributes on savebutton click

            if (itemTypeChanged)
            {
                if (!PLCDBPanel1.IsNewRecord)
                {
                    if ((ItemAttribute.HasSavedAttributes && !PLCSession.GetLabCtrl("USES_ITEM_ATTRIBUTE_ENTRIES").Equals("T")) && (AllowCurrencyEntry(PLCDBPanel1.GetOriginalValue("ITEM_TYPE")) && !AllowCurrencyEntry(PLCDBPanel1.getpanelfield("ITEM_TYPE"))))
                    {
                        //if in edit mode and item has saved attributes, prompt user that these attributes will be deleted
                        mbConfirm.Message = "Changing the item type will delete saved attribute and currency information. Do you want to proceed?";
                        mbConfirm.Show();
                        mbConfirm.FocusOK();
                        return true;
                    }
                    else if (ItemAttribute.HasSavedAttributes && !PLCSession.GetLabCtrl("USES_ITEM_ATTRIBUTE_ENTRIES").Equals("T"))
                    {
                        //if in edit mode and item has saved attributes, prompt user that these attributes will be deleted
                        mbConfirm.Message = "Changing the item type will delete saved attribute information. Do you want to proceed?";
                        mbConfirm.Show();
                        mbConfirm.FocusOK();
                        return true;
                    }
                    else if (AllowCurrencyEntry(PLCDBPanel1.GetOriginalValue("ITEM_TYPE")) && !AllowCurrencyEntry(PLCDBPanel1.getpanelfield("ITEM_TYPE"))) //from currency to non-currency itemtype
                    {
                        mbConfirm.Message = "Changing the item type will delete saved currency information. Do you want to proceed?";
                        mbConfirm.Show();
                        mbConfirm.FocusOK();
                        return true;
                    }
                }
            }
            else if (ItemTypeChangeConfirmed)
            {
                //itemtype is changed
                //it has saved attributes, so prompt is displayed
                //confirm itemtype change (answer YES so ItemTypeChangeConfirmed is true)
                //but itemtype is reverted back to original
                //set ItemTypeChangeConfirmed as false, so attributes are not deleted on savebutton click

                ItemTypeChangeConfirmed = false; //reset to default value
            }

            //show new attributes based on new itemtype
            RecreateBlankAttributes(itemTypeChanged);

            return false;
        }

        /// <summary>
        /// Recreates blank item attribute DBPANEL fields
        /// </summary>
        /// <param name="itemTypeChanged">Boolean value</param>
        private void RecreateBlankAttributes(bool itemTypeChanged)
        {
            ItemAttribute.IsItemTypeChanged = itemTypeChanged;
            ItemAttribute.InitializeMultiEntries(PLCSession.SafeInt(PLCDBPanel1.getpanelfield("QUANTITY"), 1));

            //if in add mode or in edit mode but no saved attributes, show new attributes based on itemtype
            ItemAttribute.CurrentItemType = PLCDBPanel1.getpanelfield("ITEM_TYPE");
            ItemAttribute.CreateControls();
            bnAttribute.Disabled = false;

            if (AllowCurrencyEntry(PLCDBPanel1.getpanelfield("ITEM_TYPE")))
            {
                //show currency tab
                SelectTab(VIEW_CURRENCY);               
                bnCurrency.Disabled = false;
            }
            else
            {
                SelectTab(VIEW_ATTRIBUTE);
                bnCurrency.Disabled = true;
            }

            ((UpdatePanel)this.Page.Master.FindControl("UpdatePanel1")).Update();
        }

        private void SelectTab(int tabView)
        {
            RefreshMultiview();

            tbAttribute.Attributes["class"] = tbCurrency.Attributes["class"] = tbNames.Attributes["class"] = "";
            vwAttribute.Style["display"] = vwCurrency.Style["display"] = vwNames.Style["display"] = "none";

            switch (tabView)
            {
                case VIEW_ATTRIBUTE:
                    tbAttribute.Attributes["class"] = "active";
                    vwAttribute.Style["display"] = "block";
                    break;
                case VIEW_CURRENCY:
                    tbCurrency.Attributes["class"] = "active";
                    vwCurrency.Style["display"] = "block";
                    break;
                case VIEW_NAMES:
                    tbNames.Attributes["class"] = "active";
                    vwNames.Style["display"] = "block";
                    break;

            }
        }

        private bool LabItemNumberExists(string ItemNum, bool rowAdded)
        {
            PLCQuery qryLabItem;
            if (rowAdded)
            {
                qryLabItem = new PLCQuery("SELECT * FROM TV_LABITEM WHERE CASE_KEY = " + PLCSession.PLCGlobalCaseKey + " AND LAB_ITEM_NUMBER = '" + ItemNum + "'");
            }
            else
            {
                qryLabItem = new PLCQuery("SELECT * FROM TV_LABITEM WHERE CASE_KEY = " + PLCSession.PLCGlobalCaseKey + " AND LAB_ITEM_NUMBER = '" + ItemNum + "' " +
                                                   "AND EVIDENCE_CONTROL_NUMBER <> " + PLCSession.PLCGlobalECN);
            }
            qryLabItem.Open();
            if (!qryLabItem.IsEmpty())
                return true;
            else
                return false;
        }

        private bool DeptItemNumberExists(string itemNum, bool rowAdded)
        {
            PLCQuery qryLabItem;
            if (rowAdded)
                qryLabItem = new PLCQuery("SELECT * FROM TV_LABITEM WHERE CASE_KEY = " + PLCSession.PLCGlobalCaseKey + " AND DEPARTMENT_ITEM_NUMBER = '" + itemNum + "'");
            else
                qryLabItem = new PLCQuery("SELECT * FROM TV_LABITEM WHERE CASE_KEY = " + PLCSession.PLCGlobalCaseKey + " AND DEPARTMENT_ITEM_NUMBER = '" + itemNum + "' AND EVIDENCE_CONTROL_NUMBER <> " + PLCSession.PLCGlobalECN);
            qryLabItem.Open();
            if (!qryLabItem.IsEmpty())
                return true;
            else
                return false;
        }

        private bool SubmissionCommandVoucherExists(string sVoucher, ref string sSubKey, ref string sLabSubNum)
        {
            PLCQuery qryLabSub = new PLCQuery("SELECT * FROM TV_LABSUB WHERE COMMAND_VOUCHER = '" + sVoucher + "'");
            qryLabSub.Open();
            if (!qryLabSub.IsEmpty())
            {
                sSubKey = qryLabSub.FieldByName("SUBMISSION_KEY");
                sLabSubNum = qryLabSub.FieldByName("SUBMISSION_NUMBER");
                return true;
            }
            else
                return false;
        }

        private void AddNewLabSubCommandVoucher(string sVoucher, string sECN)
        {
            string newsub = "";
            string strparentecn = "";
            int intparentecn = 0;
            int newseq = PLCSession.GetNextSequence("LABSUB_SEQ");

            PLCQuery qryNextSubNumber = new PLCQuery();
            qryNextSubNumber.SQL = "select MAX([SUBMISSION_NUMBER]) + 1 NEXTSUB from TV_LABSUB where CASE_KEY = " + PLCSession.PLCGlobalCaseKey;
            qryNextSubNumber.Open();
            if (qryNextSubNumber.Open())
            {
                newsub = qryNextSubNumber.FieldByName("NEXTSUB");
                if (newsub == "")
                    newsub = "1";
            }

            PLCQuery qryLabSubAppend = new PLCQuery("SELECT * FROM TV_LABSUB WHERE 0 = 1");
            if (qryLabSubAppend.Open())
            {
                qryLabSubAppend.Append();
                qryLabSubAppend.SetFieldValue("CASE_KEY", PLCSession.PLCGlobalCaseKey);
                qryLabSubAppend.SetFieldValue("SUBMISSION_KEY", newseq);
                qryLabSubAppend.SetFieldValue("SUBMISSION_NUMBER", newsub);
                qryLabSubAppend.SetFieldValue("RECEIVED_DATE", System.DateTime.Today);
                qryLabSubAppend.SetFieldValue("RECEIVED_TIME", System.DateTime.Now);
                qryLabSubAppend.SetFieldValue("RECEIVED_BY", PLCSession.PLCGlobalAnalyst);

                qryLabSubAppend.SetFieldValue("INVOICE_OFFICER", PLCSession.PLCGlobalAnalyst);
                qryLabSubAppend.SetFieldValue("INVOICE_OFFICER_KEY", dbgbl.GetDEPTPERSKey(PLCSession.PLCGlobalAnalyst));
                qryLabSubAppend.SetFieldValue("SUBMITTED_BY", PLCSession.PLCGlobalAnalyst);
                qryLabSubAppend.SetFieldValue("SUBMITTED_BY_KEY", dbgbl.GetDEPTPERSKey(PLCSession.PLCGlobalAnalyst));

                qryLabSubAppend.SetFieldValue("COMMAND_VOUCHER", sVoucher);
                qryLabSubAppend.SetFieldValue("COMMAND_NUMBER", "208");
                qryLabSubAppend.SetFieldValue("SUBMISSION_TYPE", "HD");
                qryLabSubAppend.SetFieldValue("VOUCHER_DATE", System.DateTime.Today);

                PLCQuery qryLabCase = new PLCQuery();
                qryLabCase.SQL = "Select * FROM TV_LABCASE where CASE_KEY = " + PLCSession.PLCGlobalCaseKey;
                qryLabCase.Open();
                if (!qryLabCase.IsEmpty())
                {
                    qryLabSubAppend.SetFieldValue("DEPARTMENT_CODE", qryLabCase.FieldByName("DEPARTMENT_CODE"));
                    qryLabSubAppend.SetFieldValue("DEPARTMENT_CASE_NUMBER", qryLabCase.FieldByName("DEPARTMENT_CASE_NUMBER"));
                }

                // update labitem submission number to newly created submission
                PLCQuery qryLabItemUpdate = new PLCQuery("SELECT * FROM TV_LABITEM WHERE EVIDENCE_CONTROL_NUMBER = " + sECN);
                qryLabItemUpdate.Open();
                if (!qryLabItemUpdate.IsEmpty())
                {
                    // get parent ecn
                    strparentecn = qryLabItemUpdate.FieldByName("PARENT_ECN");

                    qryLabItemUpdate.Edit();
                    qryLabItemUpdate.SetFieldValue("LAB_CASE_SUBMISSION", newsub);
                    qryLabItemUpdate.Post("TV_LABITEM", 7, 1);
                }

                try
                {
                    intparentecn = Convert.ToInt32(strparentecn);
                }
                catch
                {
                    intparentecn = -1;
                }
                // if parent ecn exists, copy parent ecn's submission fields to new submission
                string subkeyparent = "";
                PLCQuery qrySubLinkParent = new PLCQuery("SELECT * FROM TV_SUBLINK WHERE EVIDENCE_CONTROL_NUMBER = " + intparentecn.ToString());
                qrySubLinkParent.Open();
                if (!qrySubLinkParent.IsEmpty())
                {
                    subkeyparent = qrySubLinkParent.FieldByName("SUBMISSION_KEY");
                    PLCQuery qryLabSubParent = new PLCQuery("SELECT * FROM TV_LABSUB WHERE SUBMISSION_KEY = " + subkeyparent);
                    qryLabSubParent.Open();
                    if (!qryLabSubParent.IsEmpty())
                    {
                        qryLabSubAppend.SetFieldValue("ARREST_OFFICER", qryLabSubParent.FieldByName("ARREST_OFFICER"));
                        qryLabSubAppend.SetFieldValue("ARREST_OFFICER_KEY", qryLabSubParent.FieldByName("ARREST_OFFICER_KEY"));
                        qryLabSubAppend.SetFieldValue("CASE_OFFICER", qryLabSubParent.FieldByName("CASE_OFFICER"));
                        qryLabSubAppend.SetFieldValue("CASE_OFFICER_KEY", qryLabSubParent.FieldByName("CASE_OFFICER_KEY"));
                        qryLabSubAppend.SetFieldValue("OFFENSE_CODE", qryLabSubParent.FieldByName("OFFENSE_CODE"));
                        qryLabSubAppend.SetFieldValue("OFFENSE_DATE", qryLabSubParent.FieldByName("OFFENSE_DATE"));
                        qryLabSubAppend.SetFieldValue("CASE_DEPARTMENT_CODE", qryLabSubParent.FieldByName("CASE_DEPARTMENT_CODE"));
                    }
                }

                qryLabSubAppend.Post("TV_LABSUB", 8, 10);

                // add to sublink
                PLCQuery qrySubLink = new PLCQuery("SELECT * FROM TV_SUBLINK WHERE EVIDENCE_CONTROL_NUMBER = " + sECN);
                qrySubLink.Open();
                if (qrySubLink.IsEmpty())
                {
                    qrySubLink.Append();
                    qrySubLink.SetFieldValue("SUBMISSION_KEY", newseq);
                    qrySubLink.SetFieldValue("EVIDENCE_CONTROL_NUMBER", sECN);
                    qrySubLink.SetFieldValue("RESUBMISSION", "F");
                    qrySubLink.Post("TV_SUBLINK", 7000, 17);
                }
                else
                {
                    qrySubLink.Edit();
                    qrySubLink.SetFieldValue("SUBMISSION_KEY", newseq);
                    qrySubLink.Post("TV_SUBLINK", 7000, 17);
                }

            }
        }

        private void UpdateItemSubmissionNumber(string ECN, string SubKey, string SubNum)
        {
            PLCQuery qryLabItem = new PLCQuery("SELECT * FROM TV_LABITEM WHERE EVIDENCE_CONTROL_NUMBER = " + ECN);
            qryLabItem.Open();
            if (!qryLabItem.EOF())
            {
                if (qryLabItem.FieldByName("LAB_CASE_SUBMISSION") != SubNum)
                {
                    qryLabItem.Edit();
                    qryLabItem.SetFieldValue("LAB_CASE_SUBMISSION", SubNum);
                    qryLabItem.Post("TV_LABITEM", 7, 1);

                    // update sublink too            
                    PLCQuery qrySubLink = new PLCQuery("SELECT * FROM TV_SUBLINK WHERE EVIDENCE_CONTROL_NUMBER = " + ECN);
                    qrySubLink.Open();
                    if (qrySubLink.IsEmpty())
                    {
                        qrySubLink.Append();
                        qrySubLink.SetFieldValue("SUBMISSION_KEY", SubKey);
                        qrySubLink.SetFieldValue("EVIDENCE_CONTROL_NUMBER", ECN);
                        qrySubLink.SetFieldValue("RESUBMISSION", "F");
                        qrySubLink.Post("TV_SUBLINK", 7000, 17);
                    }
                    else
                    {
                        qrySubLink.Edit();
                        qrySubLink.SetFieldValue("SUBMISSION_KEY", SubKey);
                        qrySubLink.Post("TV_SUBLINK", 7000, 17);
                    }
                }
            }

        }

        private string NameRelationCheck()
        {
            string namesWithErrors = string.Empty;
            foreach (GridViewRow row in gvNameItem.Rows)
            {
                CheckBox cbItemName = (CheckBox)row.FindControl("chkNameItem");
                DropDownList ddlItemName = (DropDownList)row.FindControl("ddlRelation" + row.RowIndex);
                string relCode = GetRelationCode(ddlItemName.Text);

                if (!cbItemName.Checked && !relCode.Equals("NONE"))
                {
                    namesWithErrors += (!namesWithErrors.Equals("") ? ", <br>" : "") + row.Cells[NAMEITEM_GRID_COL_DESC].Text.Trim();
                }
            }

            return namesWithErrors;
        }

        private void CreateNewContainer()
        {
            int NewContKey = PLCSession.GetNextSequence("CONTAINER_SEQ");
            PLCSession.PLCGlobalScannedContainerKey = NewContKey.ToString();

            PLCQuery qryContainer = new PLCQuery();
            qryContainer.SQL = "SELECT * FROM TV_CONTAINER WHERE 0 = 1";
            qryContainer.Open();
            qryContainer.Append();
            qryContainer.SetFieldValue("CONTAINER_KEY", NewContKey);
            qryContainer.SetFieldValue("CASE_KEY", PLCSession.PLCGlobalCaseKey);
            qryContainer.SetFieldValue("CONTAINER_DESCRIPTION", tbxCreateContainerDescription.Text.ToUpper());
            qryContainer.SetFieldValue("CUSTODY_OF", PLCSession.PLCGlobalDefaultAnalystCustodyOf);
            qryContainer.SetFieldValue("LOCATION", PLCSession.PLCGlobalAnalyst);
            qryContainer.SetFieldValue("LAB_CODE", PLCSession.PLCGlobalLabCode);
            //
            qryContainer.SetFieldValue("PACKAGING_CODE", fbPackType.GetValue());

            if (PLCSession.GetLabCtrl("USES_CONTAINER_SOURCE") == "T")
                qryContainer.SetFieldValue("SOURCE", tbxCreateSource.Text.Trim());
            //
            qryContainer.Post("TV_CONTAINER", 99, 3);
            //
            PLCSession.PLCGlobalBatchKey = PLCSession.GetNextSequence("BATCH_SEQ").ToString();
            DateTime thisdate = DateTime.Now;
            DateTime thistime = DateTime.Now.ToLocalTime();

            SetLabItemContainer(NewContKey.ToString(), thisdate, thistime);

            AddContainerCustody(thisdate, thistime);
        }

        private void SetLabItemContainer(string ContainerKey, DateTime thisdate, DateTime thistime)
        {
            PLCQuery qryLABITEM = new PLCQuery("SELECT * FROM TV_LABITEM WHERE EVIDENCE_CONTROL_NUMBER=" + PLCSession.PLCGlobalECN);
            qryLABITEM.Open();
            if (!qryLABITEM.IsEmpty())
            {
                qryLABITEM.Edit();
                qryLABITEM.SetFieldValue("CONTAINER_KEY", ContainerKey);
                qryLABITEM.SetFieldValue("BARCODE", PLCSession.PLCGlobalECN);
                qryLABITEM.Post("TV_LABITEM", 7, 1);
            }

            AddItemInContainerCustody(thisdate, thistime);
        }

        private void AddItemInContainerCustody(DateTime thisdate, DateTime thistime)
        {
            string StatusKey = PLCSession.GetNextSequence("LABSTAT_SEQ").ToString();
            PLCQuery qryLabStat = new PLCQuery("SELECT * FROM TV_LABSTAT where (0 = 1)");
            qryLabStat.Open();
            qryLabStat.Append();
            qryLabStat.SetFieldValue("STATUS_KEY", StatusKey);
            qryLabStat.SetFieldValue("CONTAINER_KEY", PLCSession.PLCGlobalScannedContainerKey);
            qryLabStat.SetFieldValue("EVIDENCE_CONTROL_NUMBER", PLCSession.PLCGlobalECN);
            qryLabStat.SetFieldValue("CASE_KEY", PLCSession.PLCGlobalCaseKey);
            qryLabStat.SetFieldValue("BATCH_SEQUENCE", PLCSession.PLCGlobalBatchKey);
            qryLabStat.SetFieldValue("STATUS_CODE", PLCSession.PLCGlobalDefaultAnalystCustodyOf);
            qryLabStat.SetFieldValue("LOCKER", PLCSession.PLCGlobalAnalyst);
            qryLabStat.SetFieldValue("STATUS_DATE", thisdate);
            qryLabStat.SetFieldValue("STATUS_TIME", thistime);
            qryLabStat.SetFieldValue("SOURCE", "M");
            qryLabStat.SetFieldValue("ENTRY_ANALYST", PLCSession.PLCGlobalAnalyst);
            qryLabStat.SetFieldValue("ENTERED_BY", PLCSession.PLCGlobalAnalyst);
            qryLabStat.SetFieldValue("ENTRY_TIME", System.DateTime.Now);
            qryLabStat.SetFieldValue("ENTRY_TIME_STAMP", System.DateTime.Now);
            //
            qryLabStat.Post("TV_LABSTAT", 13, 10);
        }

        private void AddContainerCustody(DateTime thisdate, DateTime thistime)
        {
            string StatusKey = PLCSession.GetNextSequence("LABSTAT_SEQ").ToString();
            PLCQuery qryLabStat = new PLCQuery("SELECT * FROM TV_LABSTAT where (0 = 1)");
            qryLabStat.Open();
            qryLabStat.Append();
            qryLabStat.SetFieldValue("STATUS_KEY", StatusKey);
            qryLabStat.SetFieldValue("CONTAINER_KEY", PLCSession.PLCGlobalScannedContainerKey);
            qryLabStat.SetFieldValue("CASE_KEY", PLCSession.PLCGlobalCaseKey);
            qryLabStat.SetFieldValue("BATCH_SEQUENCE", PLCSession.PLCGlobalBatchKey);
            qryLabStat.SetFieldValue("STATUS_CODE", PLCSession.PLCGlobalDefaultAnalystCustodyOf);
            qryLabStat.SetFieldValue("LOCKER", PLCSession.PLCGlobalAnalyst);
            qryLabStat.SetFieldValue("STATUS_DATE", thisdate);
            qryLabStat.SetFieldValue("STATUS_TIME", thistime);
            qryLabStat.SetFieldValue("SOURCE", "M");
            qryLabStat.SetFieldValue("ENTRY_ANALYST", PLCSession.PLCGlobalAnalyst);
            qryLabStat.SetFieldValue("ENTERED_BY", PLCSession.PLCGlobalAnalyst);
            qryLabStat.SetFieldValue("ENTRY_TIME", System.DateTime.Now);
            qryLabStat.SetFieldValue("ENTRY_TIME_STAMP", System.DateTime.Now);
            //
            qryLabStat.Post("TV_LABSTAT", 13, 10);
        }

        private void PrintNewContainerLabel()
        {
            if (GetLabCtrl_USES_WS_BARCODE_PRINTING)
            {
                PLCSession.PLCSelectedBulkContainerKey = PLCSession.PLCGlobalScannedContainerKey;
                PLCSession.WriteDebug("Using Web Service to print Container BC", true);
                Response.Redirect("~/PrintBC_Container.aspx");
            }

            PLCSession.PLCCrystalReportName = "BCLIST";
            PLCSession.SetDefault("BARCODE_KEYLIST", "CO:" + PLCSession.PLCGlobalScannedContainerKey);

            PLCSession.PLCCrystalReportTitle = "Container Barcode Label";
            PLCSession.PrintCRWReport(true);
        }

        private bool GetAffectedItemRecords(PLCCONTROLS.PLCButtonClickEventArgs e)
        {
            PLCQuery qryItemRecords = new PLCQuery();
            Dictionary<string, string> AffectedItemRecords = qryItemRecords.GetAffectedItemRecords();
            if (AffectedItemRecords.Count > 0)
            {
                StringBuilder MSG = new StringBuilder("Item # " + PLCDBPanel1.getpanelfield("LAB_ITEM_NUMBER") + " for case " + PLCSession.PLCGlobalLabCase + " is about to be deleted.\r\nPlease confirm this is what you want to do.\r\n");
                MSG.AppendLine("<br/><br/>");
                MSG.AppendLine("1 Item record will be deleted<br/>");
                MSG.AppendLine(AffectedItemRecords["TV_LABSTAT"] + " custody record(s) will be deleted<br/>");
                MSG.AppendLine(AffectedItemRecords["TV_ITASSIGN"] + " assignment record(s) will be deleted<br/>");
                MSG.AppendLine(AffectedItemRecords["TV_SRDETAIL"] + " pending service request(s) will be deleted");

                //need to handle removing of record lock on delete cancel 
                mbDeleteItem.CancelButtonText = "No";
                mbDeleteItem.OKButtonText = "Yes";
                mbDeleteItem.Message = MSG.ToString();
                mbDeleteItem.Show();

                e.button_canceled = true;
                return true;
            }

            return false;
        }

        private void CheckPrintLabel()
        {
            if ((PLCSession.CheckUserOption("PRINTBAR")) && (PLCSession.CheckUserOption("HANDLE")))
            {
                if (PLCSession.GetLabCtrl("USES_MULTI_LABEL_PRINT") == "T")
                {
                    bnLabel.Visible = false;
                    btnMultiLabel.Visible = true;
                }
                else
                {
                    bnLabel.Visible = true;
                    btnMultiLabel.Visible = false;
                }
            }
            else
            {
                bnLabel.Visible = false;
                bnNoLabel.Visible = false;
                btnMultiLabel.Visible = false;
            }
        }

        private void EnableMultiLabelPrint(bool Enable)
        {
            if (PLCSession.GetLabCtrl("USES_MULTI_LABEL_PRINT") != "T")
                Enable = false;
            btnMultiLabel.Enabled = Enable;
        }

        private void PrintMultiLabel()
        {
            StringBuilder ItemECN = new StringBuilder();
            Dictionary<string, string> dictLblFormat = new Dictionary<string, string>();
            List<string> selectedItemsWithoutBarcode = new List<string>();
            CheckBox cbx;
            foreach (GridViewRow item in gvMultiPrintLabel.Rows)
            {
                cbx = (CheckBox)item.FindControl("chkItem");
                if (cbx.Checked)
                {
                    if (ItemECN.Length > 0)
                        ItemECN.Append(",");

                    ItemECN.Append("IT:" + gvMultiPrintLabel.DataKeys[item.RowIndex].Value.ToString());
                    int i = int.Parse(gvMultiPrintLabel.DataKeys[item.RowIndex].Value.ToString());
                    Barcode bc = new Barcode();
                    PLCSession.WriteDebug("BASE64:" + bc.IntToBase64(i), true);

                    string lblFormat = !string.IsNullOrEmpty(gvMultiPrintLabel.DataKeys[item.RowIndex].Values[1].ToString()) ?
                        gvMultiPrintLabel.DataKeys[item.RowIndex].Values[1].ToString() : "BCLIST";
                    string itemSelected = "IT:" + gvMultiPrintLabel.DataKeys[item.RowIndex].Value.ToString();

                    if (dictLblFormat.ContainsKey(lblFormat))
                    {
                        dictLblFormat[lblFormat] += "," + itemSelected;
                    }
                    else
                        dictLblFormat.Add(lblFormat, itemSelected);

                    if (string.IsNullOrEmpty(gvMultiPrintLabel.DataKeys[item.RowIndex]["BARCODE"].ToString()))
                        selectedItemsWithoutBarcode.Add(gvMultiPrintLabel.DataKeys[item.RowIndex].Value.ToString());
                }
            }

            if (!PLCCommonClass.IsReadOnlyAccess("WEBINQ,RONLYITTAB") && IsSelectedRecordsLocked(selectedItemsWithoutBarcode))
            {
                mpeMultiLabelPrint.Show();
                string selectedItems = ItemECN.ToString().Replace("IT:", "");
                string itemKey = string.Empty;
                string barcode = string.Empty;
                // update checkbox state of locked items and other items
                foreach (GridViewRow row in gvMultiPrintLabel.Rows)
                {
                    cbx = (CheckBox)row.FindControl("chkItem");
                    itemKey = gvMultiPrintLabel.DataKeys[row.RowIndex].Values["ECN"].ToString();
                    barcode = gvMultiPrintLabel.DataKeys[row.RowIndex].Values["BARCODE"].ToString();

                    if (RecordsLocked.Contains(itemKey) && string.IsNullOrEmpty(barcode))
                    {
                        cbx.Checked = false;
                        cbx.Enabled = false;
                    }
                    else
                    {
                        if (selectedItems.Contains(itemKey))
                        {
                            cbx.Checked = true;
                            ScriptManager.RegisterClientScriptBlock(cbx, cbx.GetType(), cbx.ClientID, "SelectOneContainer($('#" + cbx.ClientID + "'));", true);
                        }

                        cbx.Enabled = true;
                    }
                }

                dlgMessage.ShowAlert("Record Lock", RecordsLockedInfo);
                return;
            }

            if (PLCSession.GetLabCtrl("USES_SUBITEM_TRANSFER") == "T")
            {
                PLCQuery qryItemECN = new PLCQuery();
                qryItemECN.SQL = "Update TV_LABITEM SET BARCODE = EVIDENCE_CONTROL_NUMBER WHERE (PARENT_ECN > 0) and (EVIDENCE_CONTROL_NUMBER IN (" + ItemECN.ToString().Replace("IT:", "") + "))";
                qryItemECN.ExecSQL();
                //Refresh the Clear/NoLabel buttons...
                GrabGridRecord();
            }

            if (ddlLabelFormat.SelectedIndex > -1)
            {
                PLCSession.SetDefault("BARCODE_KEYLIST", ItemECN.ToString());
                PLCSession.PLCCrystalReportName = ddlLabelFormat.SelectedValue;
            }
            else
            {
                PLCSession.SetDefault("BARCODE_KEYLIST", ItemECN.ToString());
                PLCSession.SetProperty<Dictionary<string, string>>("BARCODE_KEYLIST", dictLblFormat);
                PLCSession.PLCCrystalReportName = "BCLIST";
            }

            // If USES_WS_BARCODE_PRINTING is set, use the OCX to print barcode list. Else, use crystal report.
            if (PLCSession.GetLabCtrl("USES_WS_BARCODE_PRINTING") == "T")
            {
                ItemECN.Remove(0, ItemECN.Length);
                Response.Redirect("~/PrintBC_List.aspx");
            }
            else
            {
                PLCSession.PrintCRWReport();
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "windowfocus", "_winPopup.name = 'barcodePopup';", true);
            }
        }

        private string GetCsvRVWorksheetIDs(string ecn)
        {
            PLCQuery qry = new PLCQuery(
@"SELECT DISTINCT DD.WORKSHEET_ID 
FROM TV_DNAWDATA DD LEFT OUTER JOIN TV_DNAEXAMW DE ON DD.WORKSHEET_ID = DE.WORKSHEET_ID 
WHERE DE.WORKSHEET_CODE = 'RETVOL' 
AND DD.EVIDENCE_CONTROL_NUMBER = ?");
            qry.AddParameter("EVIDENCE_CONTROL_NUMBER", ecn);
            qry.Open();

            List<string> worksheetIDs = new List<string>();
            while (!qry.EOF())
            {
                worksheetIDs.Add(String.Format("'{0}'", qry.FieldByName("WORKSHEET_ID")));
                qry.Next();
            }

            return String.Join(",", worksheetIDs.ToArray());
        }

        private void InitializeStatusChangeGrid()
        {
            gvStatusChange.SelectedRowStyle.ForeColor = gvStatusChange.RowStyle.ForeColor;
            gvStatusChange.SelectedRowStyle.BackColor = gvStatusChange.RowStyle.BackColor;
            gvStatusChange.SelectedRowStyle.Font.Bold = false;

            PLCQuery qryItems = new PLCQuery(String.Format(@"SELECT
				LI.EVIDENCE_CONTROL_NUMBER,LI.ITEM_TYPE, LI.LAB_ITEM_NUMBER, LI.DEPARTMENT_ITEM_NUMBER, IT.DESCRIPTION AS ITEM_TYPE_DESC, AN.NAME as FOR_REVIEW_BY,
				LI.PROCESS, PC.DESCRIPTION AS CURRENT_STATUS, RR.REVIEW_STATUS, 
				RR.INITIAL_REVIEW_BY, PI.DESCRIPTION AS INITIAL_STATUS,RR.INITIAL_REVIEWER_STATUS,
				RR.FINAL_REVIEW_BY, CASE WHEN RR.FINAL_REVIEWER_STATUS <> 'Agree' AND RR.FINAL_REVIEWER_STATUS <> 'Disagree' THEN '' ELSE RR.FINAL_REVIEWER_STATUS END AS FINAL_REVIEWER_STATUS,
				CASE WHEN RR.DNA_CLEARANCE_ACKNOWLEDGED = 'T' THEN 1 ELSE 0 END AS DNA_CLEARANCE_ACKNOWLEDGED, LI.SEIZED_FOR_BIOLOGY
				FROM TV_LABITEM LI 
				INNER JOIN TV_LABCASE LC ON LI.CASE_KEY = LC.CASE_KEY 
				INNER JOIN TV_ITEMTYPE IT ON LI.ITEM_TYPE = IT.ITEM_TYPE
				LEFT OUTER JOIN TV_REVREQUEST RR ON LI.EVIDENCE_CONTROL_NUMBER = RR.EVIDENCE_CONTROL_NUMBER
				LEFT OUTER JOIN TV_PROCESS PC ON LI.PROCESS = PC.PROCESS_TYPE
				LEFT OUTER JOIN TV_PROCESS PI ON RR.INITIAL_REVIEWER_STATUS = PI.PROCESS_TYPE
                LEFT OUTER JOIN TV_ANALYST AN ON AN.ANALYST = RR.FINAL_REVIEW_BY 
				WHERE LI.CASE_KEY = {0}
				ORDER BY LI.ITEM_SORT", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalAnalyst));
            qryItems.Open();

            gvStatusChange.DataSource = qryItems.PLCDataTable;
            gvStatusChange.DataBind();

            bool isInitial = false;
            bool isSeized = false;
            foreach (GridViewRow row in gvStatusChange.Rows)
            {
                String reviewStatus = ((HiddenField)row.FindControl("hfReviewStatusID")).Value;
                String finalReviewBy = ((HiddenField)row.FindControl("hfFinalReviewBy")).Value;
                String seizedForBiology = ((HiddenField)row.FindControl("hfSeizedForBiology")).Value;

                if (reviewStatus == "2" && finalReviewBy == PLCSession.PLCGlobalAnalyst)
                {
                    //					isFinal = true;
                }
                else
                    isInitial = true;

                if (seizedForBiology == "Y")
                    isSeized = true;
            }
            fbStatus.Enabled = isInitial;
            fbSecondReviewer.Enabled = isInitial;
            tbComment1.Enabled = isInitial;
            lnkDNAForm.Visible = isSeized;
        }

        private void InitializeStatusChangePanel(String ecn = "")
        {
            fbStatus.ClearValues();
            fbSecondReviewer.ClearValues();
            tbComment1.Text = "";

            if (ecn.Length > 0)
            {
                PLCQuery qryItems = new PLCQuery(String.Format(@"SELECT RR.EVIDENCE_CONTROL_NUMBER, 
				RR.INITIAL_REVIEW_BY, RR.INITIAL_REVIEWER_STATUS, RR.INITIAL_REVIEWER_COMMENT, 
				RR.FINAL_REVIEW_BY, RR.FINAL_REVIEWER_STATUS, RR.FINAL_REVIEWER_COMMENT
				FROM TV_REVREQUEST RR WHERE RR.EVIDENCE_CONTROL_NUMBER = {0}", ecn));

                if (qryItems.Open() && qryItems.HasData())
                {
                    fbStatus.SelectedValue = qryItems.FieldByName("INITIAL_REVIEWER_STATUS");
                    fbSecondReviewer.SelectedValue = qryItems.FieldByName("FINAL_REVIEW_BY");
                    tbComment1.Text = qryItems.FieldByName("INITIAL_REVIEWER_COMMENT");
                }
            }
        }

        private bool SaveStatusChange()
        {
            string strRevRequestBatchKey = string.Empty;
            DateTime RightNow = DateTime.Now;

            // generate the request batch key
            strRevRequestBatchKey = PLCSession.GetNextSequence("REVREQUEST_BATCH_SEQ").ToString();

            foreach (GridViewRow row in gvStatusChange.Rows)
            {
                CheckBox cbx = (CheckBox)row.FindControl("chkSelect");
                if (cbx.Checked)
                {
                    String ecn = gvStatusChange.DataKeys[row.RowIndex][0].ToString();
                    String reviewStatus = ((HiddenField)row.FindControl("hfReviewStatusID")).Value;
                    CheckBox cbClearance = (CheckBox)row.FindControl("chkClearance");

                    //LabItem for CaseCorrespondence
                    String prevStatus = "";
                    PLCQuery qryLabItem = new PLCQuery("SELECT * FROM TV_LABITEM WHERE EVIDENCE_CONTROL_NUMBER = " + ecn);
                    if (qryLabItem.Open() && qryLabItem.HasData())
                        prevStatus = qryLabItem.FieldByName("PROCESS");
                    String pendingStatus = fbStatus.SelectedValue.Trim() == "" ? prevStatus : fbStatus.SelectedValue;


                    String OverrideReviewStatus = "";
                    String sqlStr = String.Format("SELECT * FROM TV_PROCESS WHERE PROCESS_TYPE = '{0}'",pendingStatus);
                    PLCQuery qryProcess = new PLCQuery(sqlStr);
                    qryProcess.Open();
                    if (qryProcess.HasData() && qryProcess.FieldExist("OVERRIDE_REVIEW_STATUS"))
                        {
                        OverrideReviewStatus = qryProcess.FieldByName("OVERRIDE_REVIEW_STATUS");
                        }


                    //Cancel the old request if it exists.
                    PLCDBGlobal.instance.CreateRevReqHistory(ecn, true);

                    PLCQuery qryRevRequest = new PLCQuery("SELECT * FROM TV_REVREQUEST WHERE EVIDENCE_CONTROL_NUMBER = " + ecn);
                    qryRevRequest.Open();
                    qryRevRequest.Append();
                    qryRevRequest.SetFieldValue("EVIDENCE_CONTROL_NUMBER", ecn);
                    qryRevRequest.SetFieldValue("INITIAL_REVIEW_DATE", RightNow);
                    qryRevRequest.SetFieldValue("INITIAL_REVIEW_REQUESTOR", PLCSession.PLCGlobalAnalyst);
                    qryRevRequest.SetFieldValue("INITIAL_REVIEW_BY", PLCSession.PLCGlobalAnalyst);
                    qryRevRequest.SetFieldValue("INITIAL_REVIEWER_STATUS", pendingStatus);
                    qryRevRequest.SetFieldValue("INITIAL_REVIEWER_COMMENT", tbComment1.Text);
                    qryRevRequest.SetFieldValue("DNA_CLEARANCE_ACKNOWLEDGED", cbClearance.Checked ? "T" : "F");

                    qryRevRequest.SetFieldValue("FINAL_REVIEW_DATE", null);
                    qryRevRequest.SetFieldValue("FINAL_REVIEWER_STATUS", null);
                    qryRevRequest.SetFieldValue("FINAL_REVIEWER_COMMENT", null);

                    // save the request batch key
                    qryRevRequest.SetFieldValue("REVREQUEST_BATCH_KEY", strRevRequestBatchKey);

                    bool isSecondReviewRequired = pendingStatus != prevStatus && plcHelp.IsSecondReviewNeeded(ecn, pendingStatus);
                    if (isSecondReviewRequired)
                    {
                        qryRevRequest.SetFieldValue("PENDING_REVIEW_DATE", RightNow);
                        qryRevRequest.SetFieldValue("FINAL_REVIEW_BY", fbSecondReviewer.SelectedValue);

                        if (String.IsNullOrWhiteSpace(OverrideReviewStatus))
                            OverrideReviewStatus = "2";

                        qryRevRequest.SetFieldValue("REVIEW_STATUS", OverrideReviewStatus);
                    }
                    else
                    {
                        if (String.IsNullOrWhiteSpace(OverrideReviewStatus))
                            OverrideReviewStatus = "3";

                        qryRevRequest.SetFieldValue("FINAL_REVIEW_BY", PLCSession.PLCGlobalAnalyst);
                        qryRevRequest.SetFieldValue("REVIEW_STATUS", OverrideReviewStatus);
                        qryRevRequest.SetFieldValue("REVIEW_COMPLETE_DATE", DateTime.Now);
                        qryRevRequest.SetFieldValue("FINAL_REVIEWER_COMMENT", tbComment1.Text);
                        qryRevRequest.SetFieldValue("FINAL_REVIEW_DATE", RightNow);
                    }

                    qryRevRequest.Post("TV_REVREQUEST", 7000, 34);

                    if (!isSecondReviewRequired)
                    {
                        SetItemProcess(ecn, pendingStatus);
                        //move the REVREQUEST row to a history table
                        PLCDBGlobal.instance.CreateRevReqHistory(ecn, false);
                    }

                    StringBuilder sbComments = new StringBuilder();
                    sbComments.AppendLine(isSecondReviewRequired ? "INITIAL RETENTION REVIEW" : "FINAL RETENTION REVIEW");
                    sbComments.AppendLine("AUDIT LOG:");
                    sbComments.AppendLine("ITEM #: " + qryLabItem.FieldByName("LAB_ITEM_NUMBER"));
                    sbComments.AppendLine("DATE: " + DateTime.Now);
                    sbComments.AppendLine("REVIEW STATUS: " + GetStatusDesc(isSecondReviewRequired ? "2" : "3"));
                    sbComments.AppendLine((isSecondReviewRequired ? "CURR STATUS: " : "PREV STATUS: ") + GetProcessDesc(prevStatus));
                    sbComments.AppendLine((isSecondReviewRequired ? "PENDING STATUS: " : "CURR STATUS: ") + fbStatus.SelectedText);

                    if (isSecondReviewRequired)
                    {
                        sbComments.AppendLine("FINAL REVIEWER ASSIGNED: " + fbSecondReviewer.SelectedText);
                        sbComments.AppendLine("DNA CLEARANCE ACKNOWLEDGED: " + cbClearance.Checked.ToString());
                    }

                    sbComments.AppendLine("REVIEWER: " + PLCSession.PLCGlobalAnalystName);
                    sbComments.AppendLine("COMMENTS: " + tbComment1.Text);
                    AddScheduleDREV(ecn, sbComments.ToString());
                }
            }
            return true;
        }

        private bool CancelStatusChange()
        {
            foreach (GridViewRow row in gvStatusChange.Rows)
            {
                CheckBox cbx = (CheckBox)row.FindControl("chkSelect");
                if (cbx.Checked)
                {
                    String ecn = gvStatusChange.DataKeys[row.RowIndex][0].ToString();
                    PLCDBGlobal.instance.CreateRevReqHistory(ecn, true);
                }
            }

            return true;
        }

        private bool IsCancelStatusChangeValid()
        {
            Boolean selected = false;
            selectedItemSCList = new List<string>();
            foreach (GridViewRow row in gvStatusChange.Rows)
                if (((CheckBox)row.FindControl("chkSelect")).Checked)
                {
                    selected = true;
                    selectedItemSCList.Add(gvStatusChange.DataKeys[row.RowIndex].Values["EVIDENCE_CONTROL_NUMBER"].ToString());
                }

            if (IsSelectedRecordsLocked(selectedItemSCList))
            {
                SelectItemsCb(gvStatusChange, "EVIDENCE_CONTROL_NUMBER", "chkSelect_All", "chkSelect", false);
                dlgMessage.ShowAlert("Record Lock", RecordsLockedInfo);
                return false;
            }
            else if (!string.IsNullOrEmpty(RecordsLocked))
                SelectItemsCb(gvStatusChange, "EVIDENCE_CONTROL_NUMBER", "chkSelect_All", "chkSelect", false);
            else
                ResetItemsCbState(gvStatusChange, "chkSelect_All", "chkSelect");

            if (!selected)
            {
                mbox2.ShowMsg("Cancel Status Change Request", "Please select an item.", 1);
                return false;
            }

            return true;
        }

        private Boolean statusChangeRequiresConfirmation(String currentProcess, String pendingStatus)
        {
            pendingStatus = pendingStatus.Trim().ToUpper();
            currentProcess = currentProcess.Trim().ToUpper();

            PLCQuery qryCheckStatus = new PLCQuery(String.Format("SELECT * FROM TV_PROCESS where PROCESS_TYPE = '{0}'", currentProcess));
            qryCheckStatus.Open();

            if (!qryCheckStatus.HasData()) return false;

            if (String.IsNullOrEmpty(qryCheckStatus.FieldByName("CONFIRM_CHANGE_TO"))) return false;

            String[] confirmcodes = qryCheckStatus.FieldByName("CONFIRM_CHANGE_TO").ToUpper().Split(',');

            foreach (String s in confirmcodes)
            {
                String localS = s.Trim();
                if ((s == pendingStatus) || (s == "*")) return true;
            }

            return false;
        }

        private void AddItemToConfirmationList(String itemnumber, String currentProcess)
        {
            if (confirmList == null) confirmList = new List<StatusConfirmRec>();

            foreach (StatusConfirmRec scr in confirmList)
            {
                if (scr.fromStatus.Equals(currentProcess, StringComparison.OrdinalIgnoreCase))
                {
                    scr.itemNumbers += ", " + itemnumber;
                    return;
                }
            }

            PLCQuery qryCheckStatus = new PLCQuery(String.Format("SELECT * FROM TV_PROCESS where PROCESS_TYPE = '{0}'", currentProcess));
            qryCheckStatus.Open();

            StatusConfirmRec scRec = new StatusConfirmRec();
            scRec.fromStatus = currentProcess;
            scRec.itemNumbers = itemnumber;
            scRec.msgFormat = qryCheckStatus.FieldByName("CONFIRMATION_MESSAGE");
            confirmList.Add(scRec);
        }

        private Boolean UnConfirmedItemFound()
        {
            string pendingStatus = fbStatus.SelectedValue;
            if (String.IsNullOrEmpty(tbComment1.Text)) return false;
            if (String.IsNullOrEmpty(pendingStatus)) return false;

            foreach (GridViewRow row in gvStatusChange.Rows)
            {
                if ((row.RowType == DataControlRowType.DataRow) && (((CheckBox)row.FindControl("chkSelect")).Checked))
                {
                    String currentProcess = ((HiddenField)row.FindControl("hfProcess")).Value;
                    string itemNumber = row.Cells[1].Text;

                    if (statusChangeRequiresConfirmation(currentProcess, pendingStatus))
                    {
                        AddItemToConfirmationList(itemNumber, currentProcess);
                    }
                }
            }

            if (confirmList == null) return false;

            if (confirmList.Count > 0)
            {
                mbox3.OnOkScript = "javascript:document.getElementById('" + btnHiddenDispo.ClientID + "').click();";

                String fullMessage = "";

                foreach (StatusConfirmRec scr in confirmList)
                {
                    if (!String.IsNullOrEmpty(fullMessage))
                        fullMessage += "<P></P>";
                    fullMessage += scr.getConfirmationMessage();
                }

                fullMessage += "<P></P>Do you want to proceed with the status change?";

                confirmList = null;

                mbox3.ShowMsg("Status Change Confirmation", fullMessage, 3);
                return true;

            }
            else
            {
                return false;
            }
        }

        private bool IsStatusChangeValid()
        {
            bool selected = false;
            selectedItemSCList = new List<string>();
            foreach (GridViewRow row in gvStatusChange.Rows)
            {
                if (((CheckBox)row.FindControl("chkSelect")).Checked)
                {
                    selected = true;
                    selectedItemSCList.Add(gvStatusChange.DataKeys[row.RowIndex].Values["EVIDENCE_CONTROL_NUMBER"].ToString());
                }
            }

            if (IsSelectedRecordsLocked(selectedItemSCList))
            {
                SelectItemsCb(gvStatusChange, "EVIDENCE_CONTROL_NUMBER", "chkSelect_All", "chkSelect", false);
                dlgMessage.ShowAlert("Record Lock", RecordsLockedInfo);
                return false;
            }
            else if (!string.IsNullOrEmpty(RecordsLocked))
                SelectItemsCb(gvStatusChange, "EVIDENCE_CONTROL_NUMBER", "chkSelect_All", "chkSelect", false);
            else
                ResetItemsCbState(gvStatusChange, "chkSelect_All", "chkSelect");

            if (!selected)
            {
                mbox2.ShowMsg("Validation", "Please select an item.", 1);
                return false;
            }

            string pendingStatus = fbStatus.SelectedValue;
            string secondReviewer = fbSecondReviewer.SelectedValue;

            if ((PLCSession.PLCGlobalAnalyst == secondReviewer))
            {
                mbox2.ShowMsg("Validation", "Select a different user to review your status change request.", 1);
                return false;
            }

            if (!string.IsNullOrEmpty(secondReviewer) && !PLCSession.CheckUserOption(secondReviewer, "RETENTION"))
            {
                mbox2.ShowMsg("Validation", "Selected reviewer is not authorized to review this status change request.", 1);
                return false;
            }

            if (tbComment1.Text.Trim() == "")
            {
                mbox2.ShowMsg("Validation", "Reviewer comments are required.", 1);
                return false;
            }

            foreach (GridViewRow row in gvStatusChange.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow && ((CheckBox)row.FindControl("chkSelect")).Checked)
                {
                    String ecn = gvStatusChange.DataKeys[row.RowIndex][0].ToString();
                    String process = ((HiddenField)row.FindControl("hfProcess")).Value;
                    String reviewStatus = ((HiddenField)row.FindControl("hfReviewStatusID")).Value;
                    String thisItemType = ((HiddenField)row.FindControl("hfItemType")).Value;
                    String seizedForBiology = ((HiddenField)row.FindControl("hfSeizedForBiology")).Value;
                    CheckBox cbClearance = (CheckBox)row.FindControl("chkClearance");

                    string itemnumber = row.Cells[1].Text;

                    if (pendingStatus == process)
                    {
                        mbox2.ShowMsg("Validation", "Item " + itemnumber + " already has a status of " + fbStatus.SelectedText, 1);
                        return false;
                    }

                    String errInfo = "";
                    if (!plcHelp.IsProcessChangeValid(pendingStatus, process, ecn, out errInfo))
                    {
                        string errstr = string.Format("{0} is not a valid status for {1} items", PLCSession.GetCodeDesc("PROCESS", pendingStatus), PLCSession.GetCodeDesc("ITEMTYPE", thisItemType));
                        if (!String.IsNullOrEmpty(errInfo)) errstr += errInfo;
                        errstr += ".";
                        mbox2.ShowMsg("Validation", errstr, 1);
                        return false;
                    }

                    if (seizedForBiology == "Y" && !cbClearance.Checked)
                    {
                        mbox2.ShowMsg("Validation", "DNA Clearance Form is required for item #" + itemnumber, 1);
                        return false;
                    }

                    if (plcHelp.IsSecondReviewNeeded(ecn, pendingStatus) && secondReviewer.Trim() == "")
                    {
                        secondrevchar = "*";
                        mbox2.ShowMsg("Validation", "Second Reviewer is required.", 1);
                        return false;
                    }
                }
            }

            return true;
        }

        private void AddScheduleDREV(string ecn, string scheduleComments)
        {
            int scheduleKey = PLCSession.GetNextSequence("LABSUPP_SEQ");

            PLCQuery qrySchedule = new PLCQuery();
            qrySchedule.SQL = "SELECT SCHEDULE_KEY, CASE_KEY, EVIDENCE_CONTROL_NUMBER, DATE_RES, TIME, TYPE_RES, COMMENTS, ANALYST FROM TV_SCHEDULE WHERE 0 = 1";
            qrySchedule.Open();
            qrySchedule.Append();
            qrySchedule.SetFieldValue("SCHEDULE_KEY", scheduleKey);
            qrySchedule.SetFieldValue("CASE_KEY", PLCSession.PLCGlobalCaseKey);
            qrySchedule.SetFieldValue("EVIDENCE_CONTROL_NUMBER", ecn);
            qrySchedule.SetFieldValue("DATE_RES", DateTime.Now);
            qrySchedule.SetFieldValue("TIME", DateTime.Now);
            qrySchedule.SetFieldValue("TYPE_RES", "DREV");
            qrySchedule.SetFieldValue("COMMENTS", scheduleComments);
            qrySchedule.SetFieldValue("ANALYST", PLCSession.PLCGlobalAnalyst);
            qrySchedule.Post("TV_SCHEDULE", 41, 3);
        }

        private void SetItemProcess(string ecn, string status)
        {
            PLCQuery qryLabItem = new PLCQuery(String.Format("SELECT * FROM TV_LABITEM WHERE EVIDENCE_CONTROL_NUMBER = {0}", ecn));
            if (qryLabItem.Open() && qryLabItem.HasData())
            {
                qryLabItem.Edit();
                qryLabItem.SetFieldValue("PROCESS", status);
                qryLabItem.SetFieldValue("PROCESS_DATE", DateTime.Today);
                qryLabItem.Post("TV_LABITEM", 7, 1);
            }

            try
                {
            PLCDBGlobal.instance.UpdateNextReviewDate(ecn);
                }
            catch (Exception e)
                {

                PLCSession.WriteDebug("Exception updating Next Review Date:" + e.Message + Environment.NewLine + e.StackTrace);
                }
            }

        private string GetStatusDesc(string status)
        {
            PLCQuery qryRevStat = new PLCQuery("SELECT DESCRIPTION FROM TV_REVSTAT WHERE STATUS_CODE = '" + status + "'");
            if (qryRevStat.Open() && qryRevStat.HasData())
                return qryRevStat.FieldByName("DESCRIPTION");
            return null;
        }

        private string GetProcessDesc(string process)
        {
            PLCQuery qryProcess = new PLCQuery("SELECT DESCRIPTION FROM TV_PROCESS WHERE PROCESS_TYPE = '" + process + "'");
            if (qryProcess.Open() && qryProcess.HasData())
                return qryProcess.FieldByName("DESCRIPTION");
            return null;
        }

        private void UpdateCustodyStatusDisplay(string ecn)
        {
            if (String.IsNullOrEmpty(ecn))
            {
                ClearCustodyStatusDisplay();
                return;
            }

            // Set custody location info.
            this.lblCustodyLocationCaption.Text = "Custody Location: ";
            this.lCurrCustodyLocation.Text = GetItemCustodyLocation(ecn);

            // Set tracking number info.
            this.lblTrackingNumberCaption.Text = "";
            this.lblTrackingNumber.Text = "";

            if (PLCSession.GetLabCtrl("SHOW_TRACKING_NUM_IN_ITEMS") == "T")
            {
                string custodyCode;
                string locationCode;
                string trackingNumber;

                PLCDBGlobal.instance.GetLatestCustodyInfo(ecn, out custodyCode, out locationCode, out trackingNumber);
                if (trackingNumber != "")
                {
                    this.lblTrackingNumber.Text = trackingNumber;

                    // Default tracking number caption.
                    this.lblTrackingNumberCaption.Text = "Tracking Number: ";

                    // Set override tracking number caption if specified.
                    if ((custodyCode != "") && (locationCode != ""))
                    {
                        PLCQuery qryCustLoc = new PLCQuery("SELECT TRACKING_NUMBER_TITLE FROM TV_CUSTLOC WHERE CUSTODY_CODE = ? AND LOCATION = ?");
                        qryCustLoc.AddParameter("CUSTODY_CODE", custodyCode);
                        qryCustLoc.AddParameter("LOCATION", locationCode);
                        qryCustLoc.Open();
                        if (qryCustLoc.HasData() && (qryCustLoc.FieldByName("TRACKING_NUMBER_TITLE").Trim() != ""))
                            this.lblTrackingNumberCaption.Text = qryCustLoc.FieldByName("TRACKING_NUMBER_TITLE") + ": ";
                    }
                }
            }
        }

        private void ClearCustodyStatusDisplay()
        {
            this.lblCustodyLocationCaption.Text = "";
            this.lCurrCustodyLocation.Text = "";
            this.lblTrackingNumberCaption.Text = "";
            this.lblTrackingNumber.Text = "";
        }

        public void CheckMultiLabelPrint()
        {
            string FileItemCriteria = string.Empty;
            if (PLCSession.GetLabCtrl("SHOW_FILE_ITEM") != "T")
                FileItemCriteria = " AND I.LAB_ITEM_NUMBER != '0'";

            PLCQuery qryLABITEM = new PLCQuery(PLCSession.FormatSpecialFunctions("SELECT " +
                "EVIDENCE_CONTROL_NUMBER AS ECN,LAB_ITEM_NUMBER AS ITEMNUM,DEPARTMENT_ITEM_NUMBER AS CSITEMNUM," +
                "T.DESCRIPTION + ' ' + COALESCE(I.ITEM_DESCRIPTION, '') AS ITEMDESC, T.LABEL_FORMAT AS LABELFORMAT," +
                "I.BARCODE " +
                "FROM TV_LABITEM I " +
                "LEFT OUTER JOIN TV_ITEMTYPE T ON I.ITEM_TYPE = T.ITEM_TYPE " +
                "WHERE CASE_KEY = " + PLCSession.PLCGlobalCaseKey + FileItemCriteria + " ORDER BY I.ITEM_SORT"));
            if (qryLABITEM.Open() && qryLABITEM.HasData())
            {
                if (qryLABITEM.PLCDataTable.Rows.Count > 10)
                    pnlMultiLabelScroll.Height = Unit.Pixel(300);

                gvMultiPrintLabel.DataSource = qryLABITEM.PLCDataTable;
                gvMultiPrintLabel.DataBind();

                if (PLCCommonClass.IsReadOnlyAccess("WEBINQ,RONLYITTAB") || !string.IsNullOrEmpty(RecordsLocked))
                {
                    bool itemsbarcode = qryLABITEM.PLCDataTable.AsEnumerable().Count(a => !string.IsNullOrEmpty(a["BARCODE"].ToString())) > 0;
                    ((CheckBox)gvMultiPrintLabel.HeaderRow.FindControl("chkAll")).Enabled = itemsbarcode;
                }
            }

            PLCQuery qryLabelFormat = new PLCQuery(string.Format("SELECT REPORT_NAME, REPORT_DESCRIPTION FROM TV_LABELFMT WHERE SOURCE = 'ITEM' AND LAB_CODE = '{0}' ORDER BY SEQUENCE", PLCSession.PLCGlobalLabCode));
            qryLabelFormat.Open();
            ddlLabelFormat.DataSource = qryLabelFormat.PLCDataTable;
            ddlLabelFormat.DataBind();
            if (ddlLabelFormat.Items.Count > 0)
                ddlLabelFormat.SelectedIndex = 0;
            mpeMultiLabelPrint.Show();
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
            else if (source == "ITEM")
            {
                PLCSession.PLCGlobalAttachmentSource = source;
                PLCSession.PLCGlobalAttachmentSourceDesc = "Item Number: " + PLCDBPanel1.getpanelfield("LAB_ITEM_NUMBER") + ", Item Type: " + PLCDBPanel1.getpanelfield("ITEM_TYPE");
                master.ApplyImageAttachedClip("LABITEM", PLCSession.PLCGlobalECN);
            }
        }

        private bool ItemHasThirdPartyBarcode(string ecn)
        {
            if (ecn == "0" || string.IsNullOrEmpty(ecn))
                return false;
            else
            {
                PLCQuery qryItem = new PLCQuery();
                qryItem.SQL = "SELECT THIRD_PARTY_BARCODE FROM TV_LABITEM WHERE EVIDENCE_CONTROL_NUMBER = " + ecn;
                qryItem.Open();

                if (qryItem.HasData())
                    return !string.IsNullOrEmpty(qryItem.FieldByName("THIRD_PARTY_BARCODE"));
                else
                    return false;
            }
        }

        private void DeleteItemNameLink(string ecn)
        {
            try
            {
                PLCQuery qryItemName = new PLCQuery();
                qryItemName.SQL = "DELETE FROM TV_ITEMNAME WHERE EVIDENCE_CONTROL_NUMBER = " + ecn;
                qryItemName.ExecSQL();
            }
            catch (Exception e)
            {
                PLCSession.WriteDebug("Item " + ecn + " ITEMNAME record link delete failed. " + e.Message, true);
            }
        }

        public void LoadBulkDeletePopup()
        {
            if (PLCSession.GetLabCtrl("SHOW_FILE_ITEM") != "T")
            {
                gvBulkDelItems.PLCSQLString_AdditionalCriteria = " AND LAB_ITEM_NUMBER != '0'";
            }

            gvBulkDelItems.InitializePLCDBGrid();
            gvBulkDelItems.ClearSortExpression();
        }

        private void BulkDeleteItems()
        {
            List<string> lstItemECNs = new List<string>();
            var selectedKeys = gvBulkDelItems.GetSelectedCheckboxes();
            if (selectedKeys.Count > 0)
            {
                foreach (int index in selectedKeys)
                {
                    lstItemECNs.Add(gvBulkDelItems.DataKeys[index].Value.ToString());
                }
            }
            else
            {
                return;
            }
            DELITEMECNS = lstItemECNs;
            GetBulkItemsRec(lstItemECNs);
        }

        private void GetBulkItemsRec(List<string> itemECNs)
        {
            Dictionary<string, Dictionary<string, string>> BulkItemsRecLabStat = GetBulkItemsRecords(itemECNs, "TV_LABSTAT");
            Dictionary<string, Dictionary<string, string>> BulkItemsRecITAssign = GetBulkItemsRecords(itemECNs, "TV_ITASSIGN");

            if (IsSelectedRecordsLocked(itemECNs))
            {
                SelectItemsCb(gvBulkDelItems, "ECN", "cbSelect_All", "cbSelect", true);
                ScriptManager.RegisterStartupScript(btnShowBulkPopup, btnShowBulkPopup.GetType(), "Popup", "BDialogOpen(true);", true);
                dlgMessage.ShowAlert("Record Lock", RecordsLockedInfo);
            }
            else if (BulkItemsRecLabStat.Count > 0 || BulkItemsRecITAssign.Count > 0)
            {
                List<string> itemsRecLabstat = new List<string>();
                List<string> itemsRecITAssign = new List<string>();
                foreach (string itm in itemECNs)
                {
                    // Add items for record lock
                    PLCDBGlobal.instance.LockUnlockRecord("TV_LABITEM", PLCSession.PLCGlobalCaseKey, itm, "-1", true);

                    if (BulkItemsRecLabStat.ContainsKey(itm))
                    {
                        Dictionary<string, string> itmVal = BulkItemsRecLabStat[itm];
                        itemsRecLabstat.Add("* Item #: " + itmVal["LAB_ITEM_NUMBER"] + " has " + itmVal["TOTAL"] + " custody record(s)");
                    }

                    if (BulkItemsRecITAssign.ContainsKey(itm))
                    {
                        Dictionary<string, string> itmVal2 = BulkItemsRecITAssign[itm];
                        itemsRecITAssign.Add("* Item #: " + itmVal2["LAB_ITEM_NUMBER"] + " has " + itmVal2["TOTAL"] + " assignment record(s)");
                    }
                }

                StringBuilder MSG = new StringBuilder("The following item(s) for case " + PLCSession.PLCGlobalLabCase + " is/are about to be deleted. Please confirm this is what you want to do.<br/>");
                MSG.AppendLine("<br/><br/>");

                if (itemsRecLabstat.Count > 0)
                    MSG.AppendLine("Custody record(s) to be deleted<br/><br/>" + string.Join("<br/>", itemsRecLabstat) + "<br/><br/>");
                if (itemsRecITAssign.Count > 0)
                    MSG.AppendLine("Assignment record(s) to be deleted<br/><br/>" + string.Join("<br/>", itemsRecITAssign));

                ScriptManager.RegisterStartupScript(btnDeleteBulk, btnDeleteBulk.GetType(), "Popup", "ShowDialogMsg(\"" + MSG.ToString().Replace(Environment.NewLine, " ") + "\")", true);
            }
            else
                DeleteBulk();
        }

        private void DeleteBulk()
        {
            List<string> lstItemECNs = DELITEMECNS;
            string deleteReason = string.Empty;

            PLCQuery qryApprovedAssignment = new PLCQuery();
            qryApprovedAssignment.SQL = "SELECT * FROM TV_LABREPT A INNER JOIN TV_ITASSIGN B ON A.EXAM_KEY = B.EXAM_KEY WHERE A.COMPLETED = 'T' AND B.EVIDENCE_CONTROL_NUMBER IN ('" + string.Join("','", lstItemECNs) + "')";
            qryApprovedAssignment.Open();

            if (qryApprovedAssignment.IsEmpty())
            {
                if (PLCSession.GetLabCtrl("LOG_EDITS_TO_NARRATIVE") == "T")
                {
                    ViewState["IsBulkDelete"] = true;
                    if (hdnConfirmUpdate.Value.Trim().Length > 0)
                    {
                        StringBuilder auditText = new StringBuilder();
                        auditText.AppendLine("ITEM DELETED");
                        auditText.AppendLine(string.Format("ITEM #: {0} - ECN: {1}", PLCDBPanel1.getpanelfield("LAB_ITEM_NUMBER"), PLCSession.PLCGlobalECN));
                        SaveConfirmUpdate(auditText.ToString());
                        deleteReason = hdnConfirmUpdate.Value;
                        hdnConfirmUpdate.Value = "";
                    }
                    else
                    {
                        BulkDeleteLockRecords();
                        mInput.ShowMsg("Case update reason", "Please enter the reason for your changes", 0, txtConfirmUpdate.ClientID, btnConfirmDelete.ClientID, "Save", "Cancel");
                        return;
                    }
                }

                GetAttachEntryIDs(lstItemECNs);

                if (PLCSession.PLCDatabaseServer == "MSSQL")
                {
                    PLCQuery qryDeleteCase = new PLCQuery();
                    qryDeleteCase.AddProcedureParameter("caseKey", Convert.ToInt32(PLCSession.PLCGlobalCaseKey), 10, OleDbType.Integer, ParameterDirection.Input);
                    qryDeleteCase.AddProcedureParameter("deleteReason", deleteReason, deleteReason.Length, OleDbType.VarChar, ParameterDirection.Input);
                    qryDeleteCase.AddProcedureParameter("userId", PLCSession.PLCGlobalAnalyst, 15, OleDbType.VarChar, ParameterDirection.Input);
                    qryDeleteCase.AddProcedureParameter("program", "iLIMS" + PLCSession.PLCBEASTiLIMSVersion, 8, OleDbType.VarChar, ParameterDirection.Input);
                    qryDeleteCase.AddProcedureParameter("OSComputerName", PLCSession.GetOSComputerName(), 50, OleDbType.VarChar, ParameterDirection.Input);
                    qryDeleteCase.AddProcedureParameter("OSUserName", PLCSession.GetOSUserName(), 50, OleDbType.VarChar, ParameterDirection.Input);
                    qryDeleteCase.AddProcedureParameter("OSAddress", PLCSession.GetOSAddress(), 50, OleDbType.VarChar, ParameterDirection.Input);
                    qryDeleteCase.AddProcedureParameter("BuildNumber", PLCSession.PLCBEASTiLIMSVersion, 100, OleDbType.VarChar, ParameterDirection.Input);
                    qryDeleteCase.AddProcedureParameter("labCode", PLCSession.PLCGlobalLabCode, 3, OleDbType.VarChar, ParameterDirection.Input);
                    qryDeleteCase.AddProcedureParameter("ecnList", string.Join(",", lstItemECNs), 4000, OleDbType.VarChar, ParameterDirection.Input);
                    qryDeleteCase.ExecuteProcedure("DELETE_BULK_ITEMS");
                }
                else
                {
                    PLCQuery qryDeleteCase = new PLCQuery();
                    List<OracleParameter> parameters = new List<OracleParameter>();
                    parameters.Add(new OracleParameter("caseKey", Convert.ToInt32(PLCSession.PLCGlobalCaseKey)));
                    parameters.Add(new OracleParameter("deleteReason", deleteReason));
                    parameters.Add(new OracleParameter("userId", PLCSession.PLCGlobalAnalyst));
                    parameters.Add(new OracleParameter("program", ("iLIMS" + PLCSession.PLCBEASTiLIMSVersion).Substring(0, 8)));
                    parameters.Add(new OracleParameter("OSComputerName", PLCSession.GetOSComputerName()));
                    parameters.Add(new OracleParameter("OSUserName", PLCSession.GetOSUserName()));
                    parameters.Add(new OracleParameter("OSAddress", PLCSession.GetOSAddress()));
                    parameters.Add(new OracleParameter("BuildNumber", PLCSession.PLCBEASTiLIMSVersion));
                    parameters.Add(new OracleParameter("labCode", PLCSession.PLCGlobalLabCode));
                    parameters.Add(new OracleParameter("ecnList", string.Join(",", lstItemECNs)));
                    qryDeleteCase.ExecuteOracleProcedure("DELETE_BULK_ITEMS", parameters);
                }

                BulkDeleteHangingItemRecords(lstItemECNs);
                DeleteAttachmentsOnServer();
                GridView1.InitializePLCDBGrid();

                if (GridView1.Rows.Count > 0)
                {
                    GridView1.SelectedIndex = 0;

                    PLCSession.PLCGlobalECN = GridView1.SelectedDataKey.Value.ToString();
                    UpdateCustodyStatusDisplay(PLCSession.PLCGlobalECN);

                    GrabGridRecord();

                    LoadBulkDeletePopup();
                }
                else
                {
                    PLCSession.PLCGlobalECN = "";
                    PLCDBPanel1.EmptyMode();
                    PLCButtonPanel1.SetEmptyMode();
                    SetAttachmentsButton("CASE");
                    EnableButtonControls(false, false);
                    ClearCustodyStatusDisplay();
                    ItemAttribute.InitializeMultiEntries(1);
                }
            }
            else
            {
                BulkDeleteLockRecords();
                mbox.ShowMsg("Bulk Delete Items Error", "There is/are selected item(s) that are part of approved assignments which cannot be deleted.", 1);
            }

            ViewState["IsBulkDelete"] = false;
        }

        private Dictionary<string, Dictionary<string, string>> GetBulkItemsRecords(List<string> itemECNs, string table)
        {
            Dictionary<string, Dictionary<string, string>> LstBulkItemsRecords = new Dictionary<string, Dictionary<string, string>>();
            Dictionary<string, string> BulkItemsRecords;

            PLCQuery qrySQL = new PLCQuery("SELECT I.EVIDENCE_CONTROL_NUMBER, I.LAB_ITEM_NUMBER, COUNT(T.EVIDENCE_CONTROL_NUMBER) AS TOTAL FROM " + table + " T LEFT OUTER JOIN " +
"TV_LABITEM I ON T.EVIDENCE_CONTROL_NUMBER = I.EVIDENCE_CONTROL_NUMBER WHERE T.EVIDENCE_CONTROL_NUMBER IN ('" + string.Join("','", itemECNs) + "') GROUP BY I.EVIDENCE_CONTROL_NUMBER, I.LAB_ITEM_NUMBER");
            qrySQL.Open();

            if (!qrySQL.IsEmpty())
            {
                while (!qrySQL.EOF())
                {
                    BulkItemsRecords = new Dictionary<string, string>();
                    BulkItemsRecords.Add("TOTAL", qrySQL.FieldByName("TOTAL"));
                    BulkItemsRecords.Add("LAB_ITEM_NUMBER", qrySQL.FieldByName("LAB_ITEM_NUMBER"));
                    LstBulkItemsRecords.Add(qrySQL.FieldByName("EVIDENCE_CONTROL_NUMBER"), BulkItemsRecords);
                    qrySQL.Next();
                }
            }

            return LstBulkItemsRecords;
        }

        private void BulkDeleteHangingItemRecords(List<string> itemECNs)
        {
            try
            {
                // currency table
                PLCQuery qryDel = new PLCQuery("Delete from TV_CURRENCY where EVIDENCE_CONTROL_NUMBER IN ('" + string.Join("','", itemECNs) + "')");
                qryDel.ExecSQL();

                // labstat table
                qryDel = new PLCQuery("DELETE from TV_LABSTAT where EVIDENCE_CONTROL_NUMBER IN ('" + string.Join("','", itemECNs) + "')");
                qryDel.ExecSQL();

                // sublink
                qryDel = new PLCQuery("DELETE from TV_SUBLINK where EVIDENCE_CONTROL_NUMBER IN ('" + string.Join("','", itemECNs) + "')");
                qryDel.ExecSQL();

                ItemAttribute.DeleteCurrentAttributes();

                // review request
                qryDel = new PLCQuery("DELETE FROM TV_REVREQUEST WHERE EVIDENCE_CONTROL_NUMBER IN ('" + string.Join("','", itemECNs) + "')");
                qryDel.ExecSQL();

                // tasks
                qryDel = new PLCQuery("DELETE FROM TV_TASKLIST WHERE EVIDENCE_CONTROL_NUMBER IN ('" + string.Join("','", itemECNs) + "')");
                qryDel.ExecSQL();

                // plclock
                BulkDeleteLockRecords();
            }
            catch
            {
            }
        }

        private void BulkDeleteLockRecords()
        {
            PLCQuery qryDel = new PLCQuery(string.Format("DELETE FROM TV_PLCLOCK WHERE TABLE_NAME = '{0}' AND TABLE_KEY1 = '{1}' AND TABLE_KEY2 IN ('{2}') AND TABLE_KEY3 = '{3}' AND LOCKED_BY = '{4}'", "TV_LABITEM", PLCSession.PLCGlobalCaseKey, string.Join("','", DELITEMECNS), "-1", PLCSession.PLCGlobalAnalyst));
            qryDel.ExecSQL();
        }

        private void SetItemTypeFilter(bool doRemoveCustomFilter = false)
        {
            FlexBox fbItemType = PLCDBPanel1.GetFlexBoxControl("ITEM_TYPE");
            string fieldCodeCondition = PLCDBPanel1.GetFieldCodeCondition("ITEM_TYPE");
            string itemCategory = PLCDBPanel1.getpanelfield("ITEM_CATEGORY");
            string itemPackaging = PLCDBPanel1.getpanelfield("PACKAGING_CODE");
            string additionalCodeCondition = string.Empty;

            if (PLCSession.GetLabCtrl("USES_ITEM_TYPE_FILTERING") == "T" && !doRemoveCustomFilter)
            {
                fbItemType.TableName = "CV_ITEMTYPEREF";
                fbItemType.DescriptionFormatCode = "D";

                additionalCodeCondition += string.Format("AND (ITEM_CATEGORY = '{0}' OR ITEM_CATEGORY = '*') AND (PACKAGING_CODE = '{1}' OR PACKAGING_CODE = '*')", itemCategory, itemPackaging);

                if (string.IsNullOrEmpty(fieldCodeCondition))
                    additionalCodeCondition = additionalCodeCondition.Substring(4, additionalCodeCondition.Length - 4);
            }
            else
            {
                fbItemType.TableName = "TV_ITEMTYPE";
            }

            fbItemType.CodeCondition = fieldCodeCondition + additionalCodeCondition;
        }

        private bool IsCategoryNameLinkRequired(string itemCategory)
        {
            bool isRequired = false;
            PLCQuery qryCategory = new PLCQuery();
            qryCategory.SQL = "SELECT * FROM TV_ITEMCAT WHERE CAT_CODE = '" + itemCategory + "'";
            qryCategory.Open();
            if (!qryCategory.IsEmpty())
            {
                if (qryCategory.FieldByName("MANDATORY_NAME_LINK") == "T")
                    isRequired = true;
            }

            return isRequired;
        }

        private bool ItemHasNamesChecked()
        {
            foreach (GridViewRow row in gvNameItem.Rows)
            {
                CheckBox cbItemName = (CheckBox)row.FindControl("chkNameItem");
                if (cbItemName != null && cbItemName.Checked)
                    return true;
            }

            return false;
        }

        private string GetNameLinkPrompt(string itemCategory)
        {
            string prompt = string.Empty;

            PLCQuery qryItemCat = new PLCQuery();
            qryItemCat.SQL = "SELECT * FROM TV_ITEMCAT WHERE CAT_CODE = '" + itemCategory + "'";
            qryItemCat.Open();
            if (!qryItemCat.IsEmpty())
            {
                prompt = qryItemCat.FieldByName("NAME_LINK_PROMPT");
            }

            return prompt;
        }

        private void SetReadOnlyAccess()
        {
            // Disable controls in the page for read only access
            if (PLCCommonClass.IsReadOnlyAccess("WEBINQ,RONLYITTAB"))
            {
                if (ShowStatusChangePopUp.Value == "SHOW")
                {
                    gvStatusChange.Enabled = false;
                    fbStatus.Enabled = false;
                    fbSecondReviewer.Enabled = false;
                    tbComment1.Enabled = false;
                    btnStatusSave.Enabled = false;
                    btnCancelStatusChange.Enabled = false;
                }
                else
                {
                    // Disable plcbuttonpanel and other plcbuttonpanel custom button for read only access
                    PLCCommonClass.SetReadOnlyAccess(PLCButtonPanel1, "RecordUnlock,Bulk Delete");
                    bnLabel.Enabled = ViewState["itemhasbarcode"] == null ? false : (bool)ViewState["itemhasbarcode"];
                    bnNoLabel.Enabled = false;

                    bnDupe.Visible = false;
                    bnSample.Visible = false;
                    bnKit.Visible = false;
                    bnTransfer.Visible = false;
                }
            }
        }

        private void GetAttachEntryIDs(List<string> ecns)
        {
            if (!PLCSession.GetLabCtrl("USES_IMAGE_VAULT_SERVICE").Equals("T"))
                return;

            AttachmentEntryIDs.Clear();
            ExamAttachments.Clear();

            PLCQuery qryImages = new PLCQuery();
            string dataType = string.Empty;

            foreach (string ecn in ecns)
            {
                qryImages.SQL = @"SELECT I.ENTRY_ID, P.DATA_TYPE, I.FORMAT
FROM TV_IMAGES I 
LEFT OUTER JOIN TV_PRINTLOG P ON P.IMAGE_ID = I.IMAGE_ID 
WHERE P.FILE_SOURCE = 'LABITEM' AND P.FILE_SOURCE_KEY1 = " + ecn +
@" UNION
SELECT I.ENTRY_ID, E.DATA_TYPE, I.FORMAT
FROM TV_IMAGES I 
LEFT OUTER JOIN TV_EXAMIMAG E ON E.IMAGES_TABLE_ID = I.IMAGE_ID
WHERE E.EVIDENCE_CONTROL_NUMBER = " + ecn +
@" UNION 
SELECT I.ENTRY_ID, E.DATA_TYPE, I.FORMAT
FROM TV_IMAGES I 
INNER JOIN TV_EXAMIMAG E ON E.ANNOTATED_IMAGES_TABLE_ID = I.IMAGE_ID
WHERE E.EVIDENCE_CONTROL_NUMBER = " + ecn;
                qryImages.Open();

                while (!qryImages.EOF())
                {
                    dataType = !string.IsNullOrEmpty(qryImages.FieldByName("DATA_TYPE")) ? qryImages.FieldByName("DATA_TYPE") : qryImages.FieldByName("FORMAT");
                    AttachmentEntryIDs.Add(qryImages.FieldByName("ENTRY_ID") + "." + dataType);
                    qryImages.Next();
                }
            }

            PLCQuery qryITAssign = new PLCQuery();
            qryITAssign.SQL = "SELECT EXAM_KEY FROM TV_ITASSIGN WHERE EVIDENCE_CONTROL_NUMBER IN (" + string.Join(",", ecns) + ")";
            qryITAssign.Open();

            if (!qryITAssign.IsEmpty())
            {
                if (!ExamAttachments.ContainsKey(qryITAssign.FieldByName("EXAM_KEY")))
                    ExamAttachments.Add(qryITAssign.FieldByName("EXAM_KEY"), new List<string>());
            }

            if (ExamAttachments.Count <= 0)
                return;

            PLCQuery qryExamImages = new PLCQuery();
            qryExamImages.SQL = "SELECT EXAM_KEY, DATA_TYPE, FORMAT, ENTRY_ID FROM UV_MATRIXATTACHMENTS WHERE EXAM_KEY IN (" + string.Join(",", ExamAttachments.Keys) + ")";
            qryExamImages.Open();

            if (qryExamImages.IsEmpty())
                return;

            string format = string.Empty;
            while (!qryExamImages.EOF())
            {
                format = !string.IsNullOrEmpty(qryExamImages.FieldByName("DATA_TYPE")) ? qryExamImages.FieldByName("DATA_TYPE") : qryExamImages.FieldByName("FORMAT");
                ExamAttachments[qryExamImages.FieldByName("EXAM_KEY")].Add(qryExamImages.FieldByName("ENTRY_ID") + "." + format);

                qryExamImages.Next();
            }
        }

        private void AttachmentsFromLabAssign()
        {
            if (ExamAttachments.Count() <= 0)
                return;

            PLCQuery qryLabAssign = new PLCQuery();
            qryLabAssign.SQL = "SELECT * FROM TV_LABASSIGN WHERE EXAM_KEY IN (" + string.Join(",", ExamAttachments.Keys) + ")";
            qryLabAssign.Open();

            if (!qryLabAssign.IsEmpty())
            {
                while (!qryLabAssign.EOF())
                {
                    if (ExamAttachments.ContainsKey(qryLabAssign.FieldByName("EXAM_KEY")))
                        ExamAttachments.Remove(qryLabAssign.FieldByName("EXAM_KEY"));

                    qryLabAssign.Next();
                }
            }

            foreach (string examKey in ExamAttachments.Keys)
            {
                foreach (string entry in ExamAttachments[examKey])
                    AttachmentEntryIDs.Add(entry);
            }
        }

        private void DeleteAttachmentsOnServer()
        {
            if (!PLCSession.GetLabCtrl("USES_IMAGE_VAULT_SERVICE").Equals("T"))
                return;

            AttachmentsFromLabAssign();

            if (AttachmentEntryIDs.Count <= 0)
                return;

            PLCHelperFunctions PLCHelper = new PLCHelperFunctions();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "_deleteItemAttachments" + DateTime.Now.ToString(), "DeleteAttachmentsOnServer('" + PLCHelper.GetDeleteIVFromServerURL("") + "', ['" + string.Join("','", AttachmentEntryIDs) + "']);", true);

            AttachmentEntryIDs.Clear();
            ExamAttachments.Clear();
        }

        private void UpdateRecordsLocked(int mode, string ecn = "")
        {
            RecordsLocked = string.Empty;
            RecordsLockedInfo = string.Empty;

            switch (mode)
            {
                case 0: // get single item by ecn
                    string lockedInfo;
                    bool isItemLock = PLCDBGlobal.instance.IsRecordLocked("TV_LABITEM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, "-1", out lockedInfo);

                    if (isItemLock)
                    {
                        RecordsLocked = PLCSession.PLCGlobalECN;
                        RecordsLockedInfo = lockedInfo; // get record lock info via single ECN
                    }

                    SetControlsForRecordsLocked(isItemLock, 0);
                    break;
                case 1: // get multiple items by case key
                    List<string> itemsLocked = PLCDBGlobal.instance.GetRecordsLocked("TV_LABITEM", PLCSession.PLCGlobalCaseKey, "", "");
                    bool isItemsLock = itemsLocked.Count > 0;

                    if (isItemsLock)
                    {
                        foreach (string item in itemsLocked)
                            RecordsLocked += item.Split(',')[1] + ",";

                        RecordsLocked = RecordsLocked.TrimEnd(',');
                        RecordsLockedInfo = PLCDBGlobal.instance.GetRecordsLockedInfo("TV_LABITEM", PLCSession.PLCGlobalCaseKey, ecn, "-1"); // get record lock info via multiple ECNs if set
                    }

                    SetControlsForRecordsLocked(isItemsLock, 1);
                    break;
            }
        }

        private bool IsSelectedRecordsLocked(List<string> items)
        {
            bool isSelectedItemsLocked = false;

            if (items == null) // single item
            {
                UpdateRecordsLocked(0);

                if (!string.IsNullOrEmpty(RecordsLocked) && RecordsLocked.Contains(PLCSession.PLCGlobalECN))
                    isSelectedItemsLocked = true;
            }
            else // multiple items
            {
                UpdateRecordsLocked(1, string.Join<string>(",", items));

                if (!string.IsNullOrEmpty(RecordsLocked) && items != null)
                {
                    foreach (string itemKey in items)
                    {
                        if (RecordsLocked.Contains(itemKey))
                        {
                            isSelectedItemsLocked = true;
                            break;
                        }
                    }
                }
            }

            return isSelectedItemsLocked;
        }

        private void SelectItemsCb(GridView grd, string dataKeyName, string cbSelect_All, string cbSelect, bool doSaveCbState)
        {
            if (grd.Rows.Count > 0)
            {
                string newSelectedValue = string.Empty;

                foreach (GridViewRow row in grd.Rows)
                {
                    CheckBox cb = (CheckBox)row.FindControl(cbSelect);
                    string itemKey = grd.DataKeys[row.RowIndex].Values[dataKeyName].ToString();
                    // if current item is locked for editing
                    if (RecordsLocked.Contains(itemKey))
                    {
                        // uncheck item
                        cb.Checked = false;
                        // disable item
                        cb.Enabled = false;
                        // set state value to "0" (unchecked)
                        newSelectedValue += "0";
                    }
                    else
                    {
                        // set state value to its original state
                        newSelectedValue += (cb.Checked ? "1" : "0");
                        cb.Enabled = true;
                    }
                }

                CheckBox cbAll = (CheckBox)grd.HeaderRow.FindControl(cbSelect_All);
                cbAll.Enabled = false;
                cbAll.Checked = false;
                // save new checkbox selected values state
                if (doSaveCbState)
                    ((HiddenField)grd.HeaderRow.FindControl("cbSelect_state")).Value = newSelectedValue;
            }
        }

        private void ResetItemsCbState(GridView grd, string cbSelect_All, string cbSelect)
        {
            if (grd.Rows.Count > 0)
            {
                foreach (GridViewRow row in grd.Rows)
                    ((CheckBox)row.FindControl(cbSelect)).Enabled = true;

                ((CheckBox)grd.HeaderRow.FindControl(cbSelect_All)).Enabled = true;
            }
        }

        private void SetControlsForRecordsLocked(bool isLock, int mode)
        {
            switch (mode)
            {
                case 0:
                    if (isLock)
                    {
                        // notice in dbpanel
                        lPLCLockStatus.Text = RecordsLockedInfo;
                        trPLCLockStatus.Visible = true;
                        PLCButtonPanel1.SetButtonsForLock(true);

                        bnNoLabel.Enabled = false;

                        if (!PLCSession.CheckUserOption("ITEMLBLOFF"))
                            bnLabel.Enabled = ViewState["itemhasbarcode"] == null ? false : (bool)ViewState["itemhasbarcode"];
                    }
                    else
                    {
                        lPLCLockStatus.Text = string.Empty;
                        trPLCLockStatus.Visible = false;
                        SetPageBrowseMode(true);
                        PLCButtonPanel1.DisableLock();
                        if (!PLCSession.CheckUserOption("ITEMLBLOFF"))
                            bnNoLabel.Enabled = bnNoLabel.Text == "Clear";
                    }

                    break;
                case 1:
                    if (isLock)
                        lblNoticeMultiPrint.Text = lblNoticeStatusChange.Text = lblNoticeBulkDelete.Text = "Disabled item(s) are locked by another user for editing";
                    else
                        lblNoticeMultiPrint.Text = lblNoticeStatusChange.Text = lblNoticeBulkDelete.Text = string.Empty;

                    break;
            }
        }

        private void PopulateEvidenceReport()
        {
            Boolean showATFForm = PLCSession.CheckUserOption("ATFFORM");
            Boolean showInvHistory = PLCSession.CheckUserOption("INVCREATE") || PLCSession.CheckUserOption("INVPRINT") || PLCSession.CheckUserOption("INVFINAL") ||
                PLCSession.CheckUserOption("INVCLOSE") || PLCSession.CheckUserOption("INVDELETE") || PLCSession.CheckUserOption("INVSCAN");

            PLCQuery qryItems = new PLCQuery("SELECT REPORT_NAME, REPORT_DESCRIPTION FROM TV_RPTFORMT WHERE REPORT_ID = 'EPR'" +
                (showATFForm ? "" : " AND REPORT_NAME <> 'ATFFORM'") + (showInvHistory ? "" : " AND REPORT_NAME <> 'inventory_history'"));
            qryItems.Open();

            ddlEvidenceReport.DataSource = qryItems.PLCDataTable;
            ddlEvidenceReport.DataBind();
        }

        /// <summary>
        /// Close the signature capture popup
        /// </summary>
        private void CloseSignatureCapture()
        {
            // Clear RPTFRMT data
            ReportFormatID = "";
            ReportFormatNumber = "";

            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "_sigcap", "CloseSignatureCapture();", true);
        }

        /// <summary>
        /// Shows and initialize signature capture popup
        /// </summary>
        private void ShowSignatureCapture()
        {
            // Get RPTFORMT data
            string reportID, reportFormatNumber;
            GetReportFormatInfo(hdnEvidenceReport.Value, out reportID, out reportFormatNumber);
            ReportFormatID = reportID;
            ReportFormatNumber = reportFormatNumber;

            // Initialize dbpanels for signature
            InitEvidReportCustomDBPanel();
            dbpEvidReportSignature.DoAdd();
            dbpEvidReportSignature2.SetPanelDefaultValues();
            dbpEvidReportSignature2.DoEdit();

            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "_sigcap", "ShowSignatureCapture();", true);
        }

        private void InitEvidReportCustomDBPanel()
        {
            dbpEvidReportSignature2.PLCPanelName = "EVIDRPT_SIGNATURE_" + ReportFormatID + "_" + ReportFormatNumber;
            dbpEvidReportSignature2.CreateControls();
        }

        /// <summary>
        /// Adds item to the current batch of evidence items for evidence report
        /// </summary>
        /// <param name="batchKey"></param>
        /// <param name="ecn"></param>
        private void AddEvidenceReportItem(int batchKey, string ecn)
        {
            var qryEvidItems = new PLCQuery("SELECT * FROM TV_EVIDITEMS WHERE 0 = 1");
            qryEvidItems.Open();
            qryEvidItems.Append();
            qryEvidItems.SetFieldValue("BATCH_SEQUENCE", batchKey);
            qryEvidItems.SetFieldValue("EVIDENCE_CONTROL_NUMBER", ecn);
            qryEvidItems.Post("TV_EVIDITEMS", 3000, 18);
        }

        /// <summary>
        /// Save the current report for printing as a PDF attachment of the current case
        /// </summary>
        private int SaveEvidenceReportAsCaseAttachment(string description = "")
        {
            // Save report to a pdf file
            var pdfFilename = PLCDBGlobal.instance.SaveCurrentReportAsPDF("EVIDREPORT-" + Guid.NewGuid().ToString() + ".pdf");

            // Save pdf file to IMAGES table
            int imageKey = PLCSession.GetNextSequence("IMAGES_SEQ");
            if (!PLCCommon.instance.SaveRawDataToDatabase(imageKey, null, "Evidence Page", PLCSession.PLCGlobalAnalyst, pdfFilename, "application/pdf", false, true))
                throw new Exception("There is a problem in saving the file to database.");

            // Create thumbnail preview
            PLCCommon.instance.CreatePdfThumbnailPreview(imageKey, pdfFilename);

            // Delete the temporary pdf file
            System.IO.File.Delete(pdfFilename);

            // Log image to PRINTLOG as a case attachment
            var caseKey = PLCSession.PLCGlobalCaseKey;
            var fileSource = "LABCASE";
            int printlogKey = PLCSession.GetNextSequence("PRINTLOG_SEQ");
            PLCQuery qryPrintLog = new PLCQuery("SELECT * FROM TV_PRINTLOG WHERE 0 = 1");
            qryPrintLog.Open();
            qryPrintLog.Append();
            qryPrintLog.AddParameter("PRINTLOG_KEY", printlogKey);
            qryPrintLog.AddParameter("IMAGE_ID", imageKey);
            qryPrintLog.AddParameter("CASE_KEY", caseKey);
            qryPrintLog.AddParameter("DOCUMENT_TYPE", "ATTACH");
            qryPrintLog.AddParameter("DATA_FILE_NAME", pdfFilename);
            qryPrintLog.AddParameter("DATA_TYPE", "PDF");
            qryPrintLog.AddParameter("DATE_PRINTED", DateTime.Now);
            qryPrintLog.AddParameter("PRINTED_BY", PLCSession.PLCGlobalAnalyst);
            qryPrintLog.AddParameter("FILE_SOURCE", fileSource);
            qryPrintLog.AddParameter("FILE_SOURCE_KEY1", caseKey);
            qryPrintLog.AddParameter("DOCUMENT_DESCRIPTION", string.IsNullOrEmpty(description) ? PLCSession.PLCCrystalReportTitle : description);

            PLCQuery qryDO = new PLCQuery("SELECT MAX(DISPLAY_ORDER) + 1 NEXT_NUM FROM TV_PRINTLOG WHERE CASE_KEY = ? AND FILE_SOURCE = ? AND FILE_SOURCE_KEY1 = ?");
            qryDO.AddSQLParameter("CASE_KEY", caseKey);
            qryDO.AddSQLParameter("FILE_SOURCE", fileSource);
            qryDO.AddSQLParameter("FILE_SOURCE_KEY1", caseKey);
            qryDO.OpenReadOnly();
            int nextDisplayOrder = PLCSession.SafeInt(qryDO.FieldByName("NEXT_NUM"));
            qryPrintLog.AddParameter("DISPLAY_ORDER", nextDisplayOrder);

            qryPrintLog.Save("TV_PRINTLOG", 31, 3);

            return printlogKey;
        }

        /// <summary>
        /// <para>Link the attached pdf report to the evidence report</para>
        /// See <see cref="SaveEvidenceReportAsCaseAttachment"/>
        /// </summary>
        /// <param name="printlogKey"></param>
        /// <param name="batchKey"></param>
        private void LinkPrintLogToEvidenceReport(int printlogKey, int batchKey)
        {
            PLCQuery qry = new PLCQuery("SELECT * FROM TV_EVIDREPORT WHERE BATCH_SEQUENCE = " + batchKey);
            qry.Open();
            if (qry.HasData())
            {
                qry.Edit();
                qry.SetFieldValue("PRINTLOG_KEY", printlogKey);
                qry.Post("TV_EVIDREPORT", 3000, 18);
            }
        }

        /// <summary>
        /// Gets the report id and format number of a report
        /// </summary>
        /// <param name="reportName"></param>
        /// <param name="reportID"></param>
        /// <param name="reportFormatNumber"></param>
        private void GetReportFormatInfo(string reportName, out string reportID, out string reportFormatNumber)
        {
            reportID = "";
            reportFormatNumber = "";

            PLCQuery qry = new PLCQuery("SELECT REPORT_ID, REPORT_FORMAT_NUMBER FROM TV_RPTFORMT WHERE REPORT_NAME = ?");
            qry.AddSQLParameter("REPORT_NAME", reportName);
            qry.OpenReadOnly();
            if (qry.HasData())
            {
                reportID = qry.FieldByName("REPORT_ID");
                reportFormatNumber = qry.FieldByName("REPORT_FORMAT_NUMBER");
            }
        }

        /// <summary>
        /// toggle to Attributes tab via client side
        /// </summary>
        private void ShowAttributesTab()
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "_showAttributesTab" + DateTime.Now.ToString(), "toggleView(1);", true);
        }

        /// <summary>
        /// Retrieves the RFID tag data hex of a specific item
        /// </summary>
        /// <param name="pstrEvidenceControlNumber">The item key</param>
        /// <returns>Returns the tag data hex</returns>
        private string GetRFIDTagHex(string pstrEvidenceControlNumber)
        {
            PLCQuery objQuery = null;
            string strQuery = string.Empty;
            string strRFIDTagHex = string.Empty;

            // set the query
            strQuery = "SELECT TAG_DATA_HEX FROM RFTAG WHERE EVIDENCE_CONTROL_NUMBER = ?";
            objQuery = new PLCQuery(strQuery);
            objQuery.AddParameter("EVIDENCE_CONTROL_NUMBER", pstrEvidenceControlNumber);

            // do query
            objQuery.Open();
            if (objQuery.HasData())
            {
                strRFIDTagHex = objQuery.FieldByName("TAG_DATA_HEX");
            }

            return strRFIDTagHex;
        }

        /// <summary>
        /// Retrieves data required when using the RFID Tracker
        /// </summary>
        /// <param name="pstrEvidenceControlNumber">The item key</param>
        /// <param name="pstrCaseNumber">Out string of the Case number</param>
        /// <param name="pstrItemNumber">Out string of the Item number</param>
        /// <returns>Returns true if no errors are found</returns>
        private bool GetRFIDTrackerRequirements(string pstrEvidenceControlNumber, out string pstrCaseNumber, out string pstrItemNumber)
        {
            PLCQuery objQuery = null;
            string strQuery = string.Empty;

            // set default
            pstrCaseNumber = string.Empty;
            pstrItemNumber = string.Empty;

            // set the query
            strQuery = "SELECT I.LAB_ITEM_NUMBER, C.LAB_CASE FROM TV_LABITEM I LEFT JOIN TV_LABCASE C ON I.CASE_KEY = C.CASE_KEY WHERE I.EVIDENCE_CONTROL_NUMBER = ?";
            objQuery = new PLCQuery(strQuery);
            objQuery.AddParameter("EVIDENCE_CONTROL_NUMBER", pstrEvidenceControlNumber);

            // do query
            objQuery.Open();
            if (objQuery.HasData())
            {
                pstrCaseNumber = objQuery.FieldByName("LAB_CASE");
                pstrItemNumber = objQuery.FieldByName("LAB_ITEM_NUMBER");
            }

            return true;
        }

        /// <summary>
        /// Process the RFID Tracker and its requirements
        /// </summary>
        /// <returns>Returns true if no errors are found</returns>
        private bool ProcessRFIDTracker()
        {
            string strCaseNumber = string.Empty;
            string strEvidenceControlNumber = string.Empty;
            string strItemNumber = string.Empty;
            string strRFIDTag = string.Empty;

            strEvidenceControlNumber = PLCSession.PLCGlobalECN;
            
            // retrieve the rfid tag
            strRFIDTag = GetRFIDTagHex(strEvidenceControlNumber);

            // retrieve other required values
            GetRFIDTrackerRequirements(strEvidenceControlNumber, out strCaseNumber, out strItemNumber);

            if (strEvidenceControlNumber == string.Empty || strCaseNumber == string.Empty || strItemNumber == string.Empty || strRFIDTag == string.Empty)
            {
                dlgMessage.ShowAlert("Track RFID", "No Record found.");
                return false;
            }
            else
            {
                WebOCX.OnSuccessScript = "CheckBrowserWindowID();";
                WebOCX.TrackRFID(strEvidenceControlNumber, strCaseNumber, strItemNumber, strRFIDTag);
            }

            return true;
        }

        /// <summary>
        /// Sets the values of Floor Custody and Floor Location properties
        /// based on DEPTNAME's or LABCTRL's FLOOR_CUSTODY_OF and FLOOR_LOCATION fields
        /// </summary>
        private void SetFloorCustodyLocation()
        {
            string floorCust = string.Empty;
            string floorLoc = string.Empty;

            PLCDBGlobal.instance.GetFloorCustodyLocation(out floorCust, out floorLoc);

            FloorCustody = floorCust;
            FloorLocation = floorLoc;
        }

        #endregion
    }
}