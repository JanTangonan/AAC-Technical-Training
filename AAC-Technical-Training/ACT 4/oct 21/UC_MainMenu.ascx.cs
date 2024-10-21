using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using PLCCONTROLS;
using PLCGlobals;
using System.Collections.Specialized;

namespace BEASTiLIMS
{
    public partial class UC_MainMenu : System.Web.UI.UserControl
    {
        private bool _showLastState;
        public bool ShowLastState
        {
            get
            {
                return _showLastState;
            }
            set
            {
                _showLastState = true;
            }
        }

        //     enum tExpandOnLoad { Yes, No, UserPref };

        private tExpandOnLoad _expandOnLoad;
        public tExpandOnLoad ExpandOnLoad
        {
            get
            {
                return _expandOnLoad;
            }
            set
            {
                _expandOnLoad = value;
            }
        }

        // Cache dependency key is unique to follow the VaryByCustom params declared in the ascx file.
        private string GetControlCacheKey()
        {
            return String.Format("ascx-UC_MainMenu-{0}-{1}-{2}",
                PLCSession.PLCGlobalAnalyst, PLCSession.PLCGlobalLabCode, PLCSession.PLCDBName);
        }

        private void SetCacheDependency()
        {
            string controlCacheKey = GetControlCacheKey();

            HttpRuntime.Cache.Insert(controlCacheKey, DateTime.Now);

            string[] dependencyKeys = new string[1];
            dependencyKeys[0] = controlCacheKey;

            BasePartialCachingControl pcc = Parent as BasePartialCachingControl;
            pcc.Dependency = new System.Web.Caching.CacheDependency(null, dependencyKeys);
        }

        private string _newCaseText;
        public string NewCaseText
        {
            get { return _newCaseText; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                SetMenuItems();

                /*
                                // Always expand menu in dashboard page.
                                if (HttpContext.Current.Request.Url.AbsolutePath.ToLower().EndsWith("/dashboard.aspx"))
                                    this.ExpandOnLoad = tExpandOnLoad.Yes;

                                if (ExpandOnLoad == tExpandOnLoad.UserPref)
                                {
                                    string myDefault = PLCSession.GetUserPref("MENUDEFAULT");
                                    if (myDefault == "OPEN")
                                        ToggleMenu(tExpandOnLoad.Yes);
                                    else if (myDefault == "CLOSED")
                                        ToggleMenu(tExpandOnLoad.No);
                                    else if (myDefault == "LEAVE")
                                        ToggleMenu((PLCSession.GetDefault("MAINMENU") == "OPEN") ? tExpandOnLoad.Yes : tExpandOnLoad.No);
                                }
                                else
                                {
                                    ToggleMenu(ExpandOnLoad);
                                }
                */
                //                SetCacheDependency();
            }

            //on postback, control is not initialized
            ScriptManager.RegisterStartupScript(this, this.GetType(), "UCMAINMENU_INIT", "init();", true);
        }

        protected void SideMenu_MenuItemClick(object sender, EventArgs e)
        {
            if (SideMenu.SelectedValue == "PrelogBatchIntake")
            {
                Session["ScannedPrelogCases"] = null;
                Session["ManifestID"] = string.Empty;
                Session["CPDBarcodes"] = null;
                Session["ACKSavedCases"] = null;
                Session["PrelogBatchSequence"] = string.Empty;
                PLCSession.WriteDebug("@PBIntake from menu");
                Response.Redirect("~/PrelogBatchIntake.aspx");
            }

            if  (SideMenu.SelectedValue == "QueueSamples")
            {
                Session["IsQueueSamples"] = "T";
                Response.Redirect("~/BCTaskAssignment.aspx");
            }

            if (SideMenu.SelectedValue == "InstUpload")
            {
                String appName = "Instupload.application";
                int salt = new Random().Next(1000);
                String connString = PLCSession.PLCGlobalAnalyst + "|" + PLCSession.PLCDBName + "|" + PLCSession.GetDefault("ENCINFO") + "|" + salt.ToString();
                connString = AESEncryption.Encrypt(connString);

                string script = "window.open('" + PLCSession.GetApplicationURL(Request) + "/app_bin/InstUpload/InstUpload.application?v=" + connString + "','_blank','status = no, height = 500; width = 500; resizeable = 0');";
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "CLICKONCE-" + appName, script, true);
            }

            if (SideMenu.SelectedValue == "Logout")
            {
                string suppKey = "";

                if (Session["SupplementKeys"] != null)
                    suppKey = Session["SupplementKeys"].ToString();

                PLCDBGlobal.instance.ClearBeforeLogout(suppKey);

                NameValueCollection appSettings = ConfigurationManager.AppSettings;
                String[] vals = appSettings.GetValues("SIGNOFFURL");
                String ts = "";
                if (vals != null)
                {
                    if (vals.Length > 0) ts = vals[0];
                }

                if (String.IsNullOrWhiteSpace(ts))
                    ts = "~/login.aspx";

                PLCDBGlobal.instance.WriteAuditLog("2000", "3", "0", "USER: " + PLCSession.PLCGlobalAnalyst + " logged out of LIMS.");

                Session.Abandon();
                Response.Redirect(ts);
            }


            if (SideMenu.SelectedValue.ToUpper() == "DISCOVERY")
            {
                Session["FROMCASEINFOTAB"] = string.Empty;
                Response.Redirect("~/DiscoveryPacket.aspx");
            }

            if (SideMenu.SelectedValue == "DeptConfig")
            {
                if (!PLCSession.CheckUserOption("DEPTCFGUSERS") && !PLCSession.CheckUserOption("DEPTCFGLOCATIONS"))
                {
                    ServerAlert("Department configuration tabs are not enabled for this security group <b>(" + PLCSession.PLCGlobalAnalystGroup + ").</b>");
                    SideMenu.SelectedItem.Selected = false;
                }
                else
                    Response.Redirect("~/DeptConfigUsers.aspx");
            }


            if (SideMenu.SelectedValue == "CreateMECase")
            {
                PLCSession.PLCGlobalNameKey = "";
                PLCSession.PLCGlobalCaseKey = "";
                PLCSession.PLCGlobalSubmissionKey = "";
                PLCSession.SetDefault("IS_PARTIAL_MEIMS_CASE", "T");
                PLCSession.SetDefault("MEIMS_NAMEKEY", "");
                Response.Redirect("~/Tab1CaseMEIMS.aspx");
            }

        }

        private string GetNewCaseNavigateUrl()
        {
            if (PLCSession.GetLabCtrl("SIMPLE_QUICK_CREATE") == "T")
                return "~/QuickCreateSimple.aspx";
            if (PLCSession.GetLabCtrl("WEB_USES_ADVANCED_QC") == "T")
                return "~/QuickCreate_Advanced.aspx";
            else if (PLCSession.GetLabCtrl("WEB_USES_ADVANCED_QC") == "C")
                return "~/QuickCreateCM.aspx";
            else if (PLCSession.GetLabCtrl("WEB_USES_ADVANCED_QC") == "V")
                return "~/QuickCreateVC.aspx";
            else if (PLCSession.GetLabCtrl("WEB_USES_ADVANCED_QC") == "A")
                return "~/QuickCreate_AA.aspx";
            else if (PLCSession.GetLabCtrl("WEB_USES_ADVANCED_QC") == "X")
                return "~/QuickCreate_X.aspx";
            else if (PLCSession.GetLabCtrl("USES_SERVICE_REQUESTS") == "L")
                return "~/QuickCreate2.aspx";
            else
                return "~/QuickCreateDefault.aspx";
        }
        /*
                protected void btnMainMenu_Click(object sender, ImageClickEventArgs e)
                {
                    if (!plhMenu.Visible)
                        ToggleMenu(tExpandOnLoad.Yes);
                    else
                        ToggleMenu(tExpandOnLoad.No);

                }

                private void ToggleMenu(tExpandOnLoad expanded)
                {

                    if (expanded == tExpandOnLoad.Yes)
                    {
                        plhMenu.Visible = true;
                        btnMainMenu.ImageUrl = "~/Images/arrowbullet_left.gif";
                        PLCSession.SetDefault("MAINMENU","OPEN");
                    }

                    if (expanded == tExpandOnLoad.No)
                    {
                        plhMenu.Visible = false;
                        btnMainMenu.ImageUrl = "~/Images/arrowbullet_right.gif";
                        PLCSession.SetDefault("MAINMENU", "CLOSED");
                    }
                            
                }
        */


        private Boolean UsesChartView()
        {
            String testMe = PLCSession.GetDefault("CHARTVIEW_LIST");
            return (!String.IsNullOrWhiteSpace(testMe));
        }

        private void SetMenuItems()
        {
            MenuItem mi = null;

            MenuItem miNewCase = SideMenu.FindItem("NewCase");
            if (miNewCase != null)
            {
                bool showOnlyDefault = false;
                string newCaseMenuText;
                if (PLCSession.GetLabCtrl("NEW_CASE_MENU_TEXT").Trim().Length > 0)
                    newCaseMenuText = PLCSession.GetLabCtrl("NEW_CASE_MENU_TEXT");
                else
                    newCaseMenuText = miNewCase.Text;

                bool hideCreateCase = PLCSession.CheckUserOption("HIDECRCASE") || PLCSession.GetLabCtrlFlag("HIDE_CREATE_CASE") == "T";

                // added validation to hide the menu when web inquiry mode is on
                if (!PLCSession.CheckUserOption("ADDCASE") || PLCCommon.instance.IsWebInquiryMode())
                {
                    SideMenu.Items.Remove(miNewCase);
                }
                else if (PLCSession.GetLabCtrl("WEB_USES_ADVANCED_QC") == "T")
                {
                    miNewCase.Text = newCaseMenuText;
                    //
                    if (PLCSession.GetLabCtrl("USES_BULK_INTAKE") == "T")
                    {
                        miNewCase.Selectable = false;
                        MenuItem BulkIntake = new MenuItem();
                        BulkIntake.Text = "Bulk Intake";
                        BulkIntake.Value = "BulkIntake";
                        BulkIntake.NavigateUrl = "~/BulkIntake.aspx";
                        miNewCase.ChildItems.Add(BulkIntake);
                        //
                        if (!hideCreateCase)
                        {
                            MenuItem SingleIntake = new MenuItem();
                            SingleIntake.Text = "Single Intake";
                            SingleIntake.Value = "SingleIntake";
                            SingleIntake.NavigateUrl = GetNewCaseNavigateUrl();
                            miNewCase.ChildItems.Add(SingleIntake);
                        }

                        MenuItem BatchVoucher = new MenuItem();
                        BatchVoucher.Text = "Batch Voucher";
                        BatchVoucher.Value = "BatchVoucher";
                        BatchVoucher.NavigateUrl = "~/BatchVoucher.aspx";
                        miNewCase.ChildItems.Add(BatchVoucher);

                        if (PLCSession.CheckUserOption("QMS"))
                        {
                            MenuItem ProficiencyTest = new MenuItem();
                            ProficiencyTest.Text = "Proficiency Test";
                            ProficiencyTest.Value = "ProficiencyTest";
                            ProficiencyTest.NavigateUrl = "~/ProficiencyTest.aspx";
                            miNewCase.ChildItems.Add(ProficiencyTest);
                        }

                        if (PLCSession.CheckUserOption("PRLGBINT"))
                        {
                            MenuItem PrelogBatchIntake = new MenuItem();
                            PrelogBatchIntake.Text = "Prelog Batch Intake";
                            PrelogBatchIntake.Value = "PrelogBatchIntake";
                            miNewCase.ChildItems.Add(PrelogBatchIntake);
                        }
                    }
                    else if (PLCSession.CheckUserOption("QMS") || PLCSession.CheckUserOption("PRLGBINT") || !hideCreateCase)
                    {
                        miNewCase.Selectable = false;

                        if (!hideCreateCase)
                        {
                            MenuItem SingleIntake = new MenuItem();
                            SingleIntake.Text = "Create Case";
                            SingleIntake.Value = "CreateCase";
                            SingleIntake.NavigateUrl = GetNewCaseNavigateUrl();
                            miNewCase.ChildItems.Add(SingleIntake);
                        }

                        if (PLCSession.CheckUserOption("QMS"))
                        {
                            MenuItem ProficiencyTest = new MenuItem();
                            ProficiencyTest.Text = "Proficiency Test";
                            ProficiencyTest.Value = "ProficiencyTest";
                            ProficiencyTest.NavigateUrl = "~/ProficiencyTest.aspx";
                            miNewCase.ChildItems.Add(ProficiencyTest);
                        }

                        if (PLCSession.CheckUserOption("PRLGBINT"))
                        {
                            MenuItem PrelogBatchIntake = new MenuItem();
                            PrelogBatchIntake.Text = "Prelog Batch Intake";
                            PrelogBatchIntake.Value = "PrelogBatchIntake";
                            miNewCase.ChildItems.Add(PrelogBatchIntake);
                        }
                    }
                    else
                    {
                        miNewCase.NavigateUrl = GetNewCaseNavigateUrl();
                    }
                }
                else if (PLCSession.GetLabCtrl("DATABANK_MODE").Equals("T") 
                    && !PLCSession.GetLabCtrlFlag("USES_NEW_OFFENDER_INTAKE").Equals("T"))
                {
                    miNewCase.Text = "New Convicted Offender";
                    miNewCase.Value = "NewConvictedOffender";
                    miNewCase.NavigateUrl = "QuickCreateCODNA.aspx";
                }
                else if (PLCSession.GetLabCtrl("HIT_MODE").Equals("T")
                    || PLCSession.GetLabCtrl("HIT_MODE").Equals("C"))
                {
                    miNewCase.Text = "New Hit";
                    miNewCase.Value = "NewHit";
                    miNewCase.NavigateUrl = "HitTab.aspx";
                }
                else
                {
                    showOnlyDefault = true;
                    miNewCase.Text = newCaseMenuText;
                    miNewCase.NavigateUrl = GetNewCaseNavigateUrl();
                }

                if (PLCSession.CheckUserOption("PRELIMSCA") || PLCSession.CheckUserOption("MEDEX"))
                {
                    if (miNewCase.ChildItems.Count == 0)
                    {
                        if (showOnlyDefault)
                        {
                            if (!hideCreateCase)
                            {
                                MenuItem miDefaultNewCase = new MenuItem();
                                miDefaultNewCase.Text = "Create Case";
                                miDefaultNewCase.Value = "CreateCase";
                                miDefaultNewCase.NavigateUrl = GetNewCaseNavigateUrl();
                                miNewCase.ChildItems.Add(miDefaultNewCase);
                            }
                        }
                        else
                        {
                            MenuItem miSubMenuNewCase = new MenuItem();
                            miSubMenuNewCase.Text = miNewCase.Text;
                            miSubMenuNewCase.Value = miNewCase.Value;
                            miSubMenuNewCase.NavigateUrl = miNewCase.NavigateUrl;
                            miNewCase.ChildItems.Add(miSubMenuNewCase);
                        }

                        if (miNewCase.ChildItems.Count > 0)
                        {
                            miNewCase.Selectable = false;
                            miNewCase.Text = newCaseMenuText;
                            miNewCase.NavigateUrl = string.Empty;
                        }
                    }

                    if (PLCSession.CheckUserOption("PRELIMSCA"))
                    {
                        if (miNewCase.ChildItems.Count > 0)
                        {
                            MenuItem miPreLIMSCase = new MenuItem();
                            miPreLIMSCase.Text = "Create Pre-LIMS Case";
                            miPreLIMSCase.Value = "CreatePreLIMSCase";
                            miPreLIMSCase.NavigateUrl = "~/QuickCreatePreLIMSCase.aspx";
                            miNewCase.ChildItems.Add(miPreLIMSCase);
                        }
                        else
                        {
                            miNewCase.Text = "Create Pre-LIMS Case";
                            miNewCase.Value = "CreatePreLIMSCase";
                            miNewCase.NavigateUrl = "~/QuickCreatePreLIMSCase.aspx";
                        }
                    }

                    if (PLCSession.CheckUserOption("MEDEX") && PLCSession.GetLabCtrlFlag("USES_WEB_MEIMS").Equals("T"))
                    {
                        if (miNewCase.ChildItems.Count > 0)
                        {
                            var menuItem = new MenuItem();
                            menuItem.Text = "Create ME Case";
                            menuItem.Value = "CreateMECase";
                            miNewCase.ChildItems.Add(menuItem);
                        }
                        else
                        {
                            miNewCase.Text = "Create ME Case";
                            miNewCase.Value = "CreateMECase";
                            miNewCase.NavigateUrl = string.Empty;
                        }
                    }
                }
                else if (showOnlyDefault && hideCreateCase)
                {
                    SideMenu.Items.Remove(miNewCase);
                }

                _newCaseText = miNewCase.Text;
            }

            //AAC Ticket #357 LABCTRL4.USES_WEB_ITEM_LIST
            // added validation to hide the menu when web inquiry mode is on
            if (PLCSession.GetLabCtrl("USES_WEB_ITEM_LIST") != "T" || PLCCommon.instance.IsWebInquiryMode() || PLCSession.CheckUserOption("HIDEITEMLIST"))
            {
                mi = SideMenu.FindItem("ItemList");
                if (mi!=null) SideMenu.Items.Remove(mi);  //item list link
            }

            // added validation to hide the menu when web inquiry mode is on
            if ((PLCSession.CheckUserOption("HIDEBLKCS") && PLCSession.CheckUserOption("RESBULK")) || PLCSession.GetLabCtrl("USES_BULK_CONTAINERS") != "T" || PLCCommon.instance.IsWebInquiryMode())
            {
                mi = SideMenu.FindItem("BulkContainer");
                if (mi != null) SideMenu.Items.Remove(mi);  //item list link
            }

            // hide the Service Request menu when web inquiry mode is on
            if (PLCCommon.instance.IsWebInquiryMode())
            {
                mi = SideMenu.FindItem("ServiceRequest");
                if (mi != null) SideMenu.Items.Remove(mi); //item list link
            }

            // hide the Setup menu when web inquiry mode is on
            if (PLCCommon.instance.IsWebInquiryMode())
            {
                mi = SideMenu.FindItem("Setup");
                if (mi != null) SideMenu.Items.Remove(mi); //item list link
            }

            // hide the remote transfer menu if the flag is not set.
            if (!PLCSession.CheckUserOption("REMOTE"))
            {
                mi = SideMenu.FindItem("RemoteTransfer");
                if (mi != null) SideMenu.Items.Remove(mi);
            }

            if (PLCSession.CheckUserOption("HIDESEARCH"))
            {
                mi = SideMenu.FindItem("Search");
                if (mi != null) SideMenu.Items.Remove(mi);
            }
            else
            {
                if (PLCSession.GetLabCtrl("USES_SERVICE_REQUESTS") == "")
                {
                    try { RemoveChildItem("Search", "SRSearch"); } //service request search
                    catch { }
                }
                else
                {
                    if (!PLCDBGlobal.instance.HasAnalystSectionAccess(PLCSession.PLCGlobalAnalyst, "REVIEW_SERVICE_REQUESTS") || PLCSession.CheckUserOption("HIDESRREV"))
                    {
                        try { RemoveChildItem("ServiceRequest", "SRReview"); } //service request review
                        catch { }
                    }

                    if (PLCSession.CheckUserOption("HIDESETAB"))
                    {
                        try { RemoveChildItem("Search", "SRSearch"); } //service request search
                        catch { }
                    }
                }

                if (PLCSession.CheckUserOption("HIDECASRCH"))
                {
                    try { RemoveChildItem("Search", "CaseSearch"); }
                    catch { }
                }

                if (!PLCSession.CheckUserOption("SEEASGN"))
                {
                    try { RemoveChildItem("Search", "AssignmentSearch"); } //assignment search
                    catch { }
                }
                else if (PLCSession.CheckUserOption("HIDEASTAB"))
                {
                    try { RemoveChildItem("Search", "AssignmentSearch"); } //assignment search
                    catch { }
                }

                // added validation to hide the menu when web inquiry mode is on
                if (!PLCSession.CheckUserOption("ABATCHID") || PLCCommon.instance.IsReadOnlyAccess("WEBINQ,RONLYASTAB"))
                {
                    try { RemoveChildItem("Search", "BatchAssignment"); } //batch assign
                    catch { }
                }

                if (PLCSession.CheckUserOption("HIDESRCHLO"))
                {
                    try { RemoveChildItem("Search", "Location"); }//location search
                    catch { }
                }

                if (PLCSession.CheckUserOption("HIDESRCHCU"))
                {
                    try { RemoveChildItem("Search", "Custody"); }//custody search
                    catch { }
                }

                if (PLCSession.CheckUserOption("HIDESRCHCC"))
                {
                    try { RemoveChildItem("Search", "ContainerSearch"); }//container search
                    catch { }

                }

                if (PLCSession.GetLabCtrl("USES_ACTIVITY_LOG") != "T")
                {

                    try { RemoveChildItem("Search", "ActivitySearch"); }//ActivitySearc
                    catch { }

                    //mi = SideMenu.FindItem("ActivitySearch");
                    //if (mi != null) SideMenu.Items.Remove(mi);  // activity search
                }

                if (!PLCSession.CheckUserOption("ITEMTRACK"))
                {
                    try { RemoveChildItem("Search", "ItemTracker"); }
                    catch { }
                }

                if (!PLCSession.CheckUserOption("BREATH"))
                {
                    try { RemoveChildItem("Search", "BreathAlcohol"); }
                    catch { }
                }
            }

            if (PLCSession.GetLabCtrl("USES_SERVICE_REQUESTS") == "" || PLCSession.CheckUserOption("HIDESRREV"))
            {
                try {   RemoveChildItem("ServiceRequest", "SRReview");} //service request review
                catch {}
            }

            if (!PLCSession.CheckUserOption("AUDITLOG"))
            {
                try { RemoveChildItem("Admin", "AuditLog"); }
                catch { }
            }

            if (!PLCSession.CheckUserOption("DLLOCAL"))
            {
                try { RemoveChildItem("Setup", "DownloadLocalFiles"); }
                catch { }
            }

            if (!PLCSession.CheckUserOption("CONFIG"))
            {
                try
                {
                    RemoveChildItem("Admin", "Config");
                    RemoveChildItem("Admin", "WebLabCtrl");
                    RemoveChildItem("Admin", "DeptCtrlSetup");

                }
                catch { }
            }

            if (!PLCSession.CheckUserOption("DEPTCONFIG"))
            {
                try
                {
                    RemoveChildItem("Admin", "DeptConfig");
                }
                catch { }
            }

            if (!PLCSession.CheckUserOption("GETSIG"))
            {
                try { RemoveChildItem("Admin", "GetSig"); }
                catch { }
            }

            if (!PLCSession.CheckUserOption("SAKDELETE"))
            {
                try { RemoveChildItem("Admin", "SAK"); }
                catch { }
            }

            if(!PLCSession.CheckUserOption("PRELOGSETUP"))
            {
                try { RemoveChildItem("Admin", "PrelogSetup"); }
                catch { }
            }

            if (!PLCSession.CheckUserOption("CODNASETUP"))
            {
                try { RemoveChildItem("Admin", "CODNASetup"); }
                catch { }
            }

            if (!PLCSession.CheckUserOption("SYSFLAGS"))
            {
                try
                {
                    RemoveChildItem("Admin", "SystemFlags");
                }
                catch { }
            }

            // added validation to hide the menu when web inquiry mode is on
            if (PLCSession.GetLabCtrl("USES_WEB_INVENTORY") != "T" || PLCCommon.instance.IsWebInquiryMode())
            {
                try
                {
                    mi = SideMenu.FindItem("Inventory");
                    if (mi != null) SideMenu.Items.Remove(mi);
                }
                catch { }
            }
            else
            {
                if (!(PLCSession.CheckUserOption("INVCREATE") || PLCSession.CheckUserOption("INVPRINT") || PLCSession.CheckUserOption("INVFINAL") ||
                    PLCSession.CheckUserOption("INVCLOSE") || PLCSession.CheckUserOption("INVDELETE") || PLCSession.CheckUserOption("INVSCAN")))
                {
                    try {
                        RemoveChildItem("Inventory", "LocationInventory");
                        RemoveChildItem("Inventory", "LAMInventory");
                    }
                    catch { }
                }

                if (PLCSession.CheckUserOption("HIDELAMINV"))
                {
                    try
                    {
                        //need to remove LAM Inventory 
                        //from the Inventory menu
                        RemoveChildItem("Inventory", "LAMInventory");
                    }
                    catch
                    { }
                }

                if (PLCSession.CheckUserOption("HIDELOCINV"))
                {
                    try
                    {
                        //need to remove Location Inventory 
                        //from the Inventory menu
                        RemoveChildItem("Inventory", "LocationInventory");
                    }
                    catch { }
                }

                if (!PLCSession.CheckUserOption("CONTINV"))
                {
                    try { RemoveChildItem("Inventory", "ContainerInventory"); }
                    catch { }
                }
            }

            //if (PLCSession.GetLabCtrl("USES_SERVICE_REQUESTS") != "L")
            //{
            //    try { RemoveChildItem("ServiceRequest", "PhotoRequest"); }
            //    catch { }
            //}

            if (!PLCSession.CheckUserOption("CREATEDIGI"))
            {
                try { RemoveChildItem("ServiceRequest", "PhotoRequest"); }
                catch { }
            }

            // Show Search > Completed Lab Reports menu item if either of the following:
            //   user option REPTDEPT is set
            //   user has analsect access to at least one section
            if (PLCSession.CheckUserOption("HIDERETAB"))
            {
                try { RemoveChildItem("Search", "ReportSearch"); }
                catch { }
            }
            else if (PLCSession.CheckUserOption("COMPREPT") ||
                PLCSession.GetHasUserAnalSect("VIEW_APPROVED_REPORTS"))
            {
                // User has access, keep menu item
            }
            else
            {
                // No access to Report Search, remove menu item.
                try { RemoveChildItem("Search", "ReportSearch"); }
                catch { }
            }

            if (PLCSession.CheckUserOption("HIDEREPMN"))
            {
                mi = SideMenu.FindItem("Reports");
                if (mi != null) { SideMenu.Items.Remove(mi); }
            }
            else
            {

                if (PLCSession.CheckUserOption("HIDECUSTRE"))
                {
                    try { RemoveChildItem("Reports", "CustomReports"); }
                    catch { }
                }
                else if (!PLCDBGlobal.instance.HasReportGroup(PLCSession.PLCGlobalAnalyst))
                {
                    //remove custom reports
                    try { RemoveChildItem("Reports", "CustomReports"); }
                    catch { }
                }


                if (!UsesChartView()) RemoveChildItem("Reports", "Analytics");

                if (PLCSession.CheckUserOption("REPORTS"))
                {
                    if (PLCSession.CheckUserOption("REPTHIDEAS")) RemoveChildItem("Reports", "Assignments");
                    if (PLCSession.CheckUserOption("REPTHIDECA")) RemoveChildItem("Reports", "Cases");
                    if (PLCSession.CheckUserOption("REPTHIDESU")) RemoveChildItem("Reports", "Submission");
                    if (PLCSession.CheckUserOption("REPTHIDERE")) RemoveChildItem("Reports", "ReviewApproval");
                    if (PLCSession.CheckUserOption("REPTHIDEAC")) RemoveChildItem("Reports", "ActivityLog");
                    if (PLCSession.CheckUserOption("REPTHIDETO")) RemoveChildItem("Reports", "TurnAround");
                    if (PLCSession.CheckUserOption("REPTHIDEPE")) RemoveChildItem("Reports", "Pending");
                    if (PLCSession.CheckUserOption("REPTHIDEAR")) RemoveChildItem("Reports", "ActivityReport");
                    if (PLCSession.CheckUserOption("REPTHIDEIT")) RemoveChildItem("Reports", "ItemType");
                    if (PLCSession.CheckUserOption("REPTHIDEIM")) RemoveChildItem("Reports", "Items");
                    if (PLCSession.CheckUserOption("REPTHIDEST")) RemoveChildItem("Reports", "Statistics");
                    if (PLCSession.CheckUserOption("REPTHIDESM")) RemoveChildItem("Reports", "Summary");
                }
                else
                {
                    RemoveChildItem("Reports", "Assignments");
                    RemoveChildItem("Reports", "Cases");
                    RemoveChildItem("Reports", "Submission");
                    RemoveChildItem("Reports", "ReviewApproval");
                    RemoveChildItem("Reports", "ActivityLog");
                    RemoveChildItem("Reports", "TurnAround");
                    RemoveChildItem("Reports", "Pending");
                    RemoveChildItem("Reports", "ActivityReport");
                    RemoveChildItem("Reports", "ItemType");
                    RemoveChildItem("Reports", "Items");
                    RemoveChildItem("Reports", "Statistics");
                    RemoveChildItem("Reports", "Summary");
                }

                Boolean UsesPullList = (PLCSession.CheckUserOption("PLIST1") || PLCSession.CheckUserOption("PLIST2") || PLCSession.CheckUserOption("PLIST3") || PLCSession.CheckUserOption("PLIST4"));
                if (!UsesPullList)
                {
                    try { RemoveChildItem("Reports", "Courier"); }
                    catch { }
                }
            }

            if (PLCSession.GetLabCtrl("USES_DOCUMENT_LINKS") != "T" || PLCSession.CheckUserOption("HIDEDOC"))
            {
                mi = SideMenu.FindItem("Documents");
                if (mi != null)
                    SideMenu.Items.Remove(mi);
            }

            //DNA
            // added validation to hide the menu when web inquiry mode is on
            if (!PLCSession.CheckUserOption("DNABATCH") || PLCCommon.instance.IsWebInquiryMode())
            {
                mi = SideMenu.FindItem("DNA");
                if (mi != null)
                    SideMenu.Items.Remove(mi);
            }
            else
            {
                if (!PLCSession.CheckUserOption("DNACOURT"))
                {
                    try { RemoveChildItem("DNA", "CourtOrder"); }
                    catch { }
                }

                if (!PLCSession.CheckUserOption("CODISIMPT"))
                {
                    try { RemoveChildItem("DNA", "CODISImport"); }
                    catch { }
                }

                if (PLCSession.CheckUserOption("HIDEDNACOE"))
                {
                    try { RemoveChildItem("DNA", "CODISExport"); }
                    catch { }
                }

                if (!PLCSession.CheckUserOption("DCJSEXP"))
                {
                    try { RemoveChildItem("DNA", "DCJSExport"); }
                    catch { }
                }

                if (!PLCSession.CheckUserOption("CODISUPL"))
                {
                    try { RemoveChildItem("DNA", "CODISUpload"); }
                    catch { }
                }

                mi = GetMenuItem("DNA");
                mi.Text = PLCSession.GetLabCtrl("DNA_MENU_TEXT").Trim().Length > 0 ? PLCSession.GetLabCtrl("DNA_MENU_TEXT") : mi.Text;

                mi = GetMenuItem("DNA", "Worklist");
                mi.Text = PLCSession.GetLabCtrl("DNA_BATCH_CREATE_MENU_TEXT").Trim().Length > 0 ? PLCSession.GetLabCtrl("DNA_BATCH_CREATE_MENU_TEXT") : mi.Text;

                mi = GetMenuItem("DNA", "Batch");
                mi.Text = PLCSession.GetLabCtrl("DNA_BATCH_RESULTS_MENU_TEXT").Trim().Length > 0 ? PLCSession.GetLabCtrl("DNA_BATCH_RESULTS_MENU_TEXT") : mi.Text;
            }

            //Instrument
            // added validation to hide the menu when web inquiry mode is on
            if (!PLCSession.CheckUserOption("INSTMENU") || PLCCommon.instance.IsWebInquiryMode())
            {
                mi = SideMenu.FindItem("Instrument");
                if (mi != null)
                    SideMenu.Items.Remove(mi);
            }
            else
            {
                mi = GetMenuItem("Instrument");
                mi.Text = PLCSession.GetLabCtrl("INSTRUMENT_MENU_TEXT").Trim().Length > 0 ? PLCSession.GetLabCtrl("INSTRUMENT_MENU_TEXT") : mi.Text;

                if (PLCSession.CheckUserOption("IBATCHCRE"))
                {
                    mi = GetMenuItem("Instrument", "BatchCreate");
                    mi.Text = PLCSession.GetLabCtrl("BATCH_CREATE_MENU_TEXT").Trim().Length > 0 ? PLCSession.GetLabCtrl("BATCH_CREATE_MENU_TEXT") : mi.Text;
                }
                else
                    RemoveMenuItem("Instrument", "BatchCreate");

                if (PLCSession.CheckUserOption("IBATCHRES"))
                {
                    mi = GetMenuItem("Instrument", "BatchResults");
                    mi.Text = PLCSession.GetLabCtrl("BATCH_RESULTS_MENU_TEXT").Trim().Length > 0 ? PLCSession.GetLabCtrl("BATCH_RESULTS_MENU_TEXT") : mi.Text;
                }
                else
                    RemoveMenuItem("Instrument", "BatchResults");

                if (!PLCSession.CheckUserOption("BATSBATCH"))
                    RemoveMenuItem("Instrument", "BatsBatch");

                if (!PLCSession.CheckUserOption("SEQWIZ"))
                    RemoveMenuItem("Instrument", "Olympus");

                if (!PLCSession.CheckUserOption("INSTVIEW"))
                    RemoveMenuItem("Instrument", "InstView");

                if (!PLCSession.CheckUserOption("TOX_INST"))
                    RemoveMenuItem("Instrument", "InstrumentInterface");

                if (!PLCSession.CheckUserOption("INSTPOP"))
                    RemoveMenuItem("Instrument", "InstPop");

                if (!PLCSession.CheckUserOption("INSTUPLD"))
                    RemoveMenuItem("Instrument", "InstUpload");

                if (!PLCSession.CheckUserOption("RESULTSUP"))
                    RemoveMenuItem("Instrument", "ResultsUpload");

            }


            if (!PLCSession.CheckUserOption("CREATEPDR"))
            {
                try { RemoveChildItem("ServiceRequest", "DeputyRequest"); } //print deputy request
                catch { }
            }

            if (!PLCSession.CheckUserOption("CREATECSIR"))
            {
                try { RemoveChildItem("ServiceRequest", "CSIRequest"); } //print deputy request
                catch { }
            }

            if (!PLCSession.CheckUserOption("CRYOBOX"))
            {
                try { RemoveChildItem("DNA", "Cryobox"); }
                catch { }
            }

            if (PLCSession.CheckUserOption("HIDEDNAWL"))
            {
                try { RemoveChildItem("DNA", "Worklist"); }
                catch { }
            }

            if (PLCSession.CheckUserOption("HIDEDNABAT"))
            {
                try { RemoveChildItem("DNA", "Batch"); }
                catch { }
            }

            if (PLCSession.GetLabCtrl("USES_ACTIVITY_LOG") != "T" || PLCSession.CheckUserOption("HIDEACTLOG"))
            {
                mi = SideMenu.FindItem("ActivityLog");
                if (mi != null) SideMenu.Items.Remove(mi);  // activity log
            }

            if (!PLCSession.CheckUserOption("CHEMINV")) // chemical inventory
            {
                mi = SideMenu.FindItem("ChemInv");
                if (mi != null) SideMenu.Items.Remove(mi);
            }

            // added validation to hide the menu when web inquiry mode is on
            if (!PLCSession.CheckUserOption("DISPOS") || PLCCommon.instance.IsWebInquiryMode()) // Uses Disposition
            {
                mi = SideMenu.FindItem("Disposition");
                if (mi != null) SideMenu.Items.Remove(mi);
            }

            // added validation to hide the menu when web inquiry mode is on
            if (!PLCSession.CheckUserOption("QMS") || PLCCommon.instance.IsWebInquiryMode())
            {
                mi = SideMenu.FindItem("QMS");
                if (mi != null) SideMenu.Items.Remove(mi);
            }
            else
            {
                if (!PLCSession.CheckUserOption("QMS-DOCS"))
                {
                    try { RemoveChildItem("QMS", "DocumentControl"); } //documents
                    catch { }
                }

                if (!PLCSession.CheckUserOption("QMS-PROGS"))
                {
                    try { RemoveChildItem("QMS", "Programs"); } //programs
                    catch { }
                }

                if (!PLCSession.CheckUserOption("QMS-AUDIT"))
                {
                    try { RemoveChildItem("QMS", "Audits"); } //audits
                    catch { }
                }

                if (!PLCSession.CheckUserOption("QMS-PTEST"))
                {
                    try { RemoveChildItem("QMS", "Proficiency"); } //proficiency
                    catch { }
                }

                if (!PLCSession.CheckUserOption("QMS-QCFLAG"))
                {
                    try { RemoveChildItem("QMS", "AnalystQCFlag"); } //analyst qc flag
                    catch { }
                }

                if (!PLCSession.CheckUserOption("QMS-MONIT"))
                {
                    try { RemoveChildItem("QMS", "Testimony"); } //testimony
                    catch { }
                }

                if (!PLCSession.CheckUserOption("QMS-MONTRA"))
                {
                    try { RemoveChildItem("QMS", "Training"); } //training
                    catch { }
                }
            }

            if (PLCSession.GetLabCtrl("USES_QMS") == "T" && !PLCSession.CheckUserOption("HIDECONDOC"))
            {
                mi = SideMenu.FindItem("ControlledDocuments");

                if (mi != null)
                    mi.Text = !string.IsNullOrEmpty(PLCSession.GetLabCtrl("CONTROLLED_DOCS_BUTTON_TEXT"))
                        ? PLCSession.GetLabCtrl("CONTROLLED_DOCS_BUTTON_TEXT")
                        : mi.Text;
            }
            else
            {
                mi = SideMenu.FindItem("ControlledDocuments");

                if (mi != null)
                    SideMenu.Items.Remove(mi);
            }

            // added validation to hide the menu when web inquiry mode is on
            if (!PLCSession.CheckUserOption("BATCHRES") || PLCCommon.instance.IsReadOnlyAccess("WEBINQ,RONLYASTAB"))
            {
                mi = SideMenu.FindItem("BatchWorklist");
                if (mi != null) SideMenu.Items.Remove(mi);
            }
            else
            {
                if (PLCSession.CheckUserOption("HIDEBATWLC"))
                {
                    try { RemoveChildItem("BatchWorklist", "WorklistCreate"); }
                    catch { }
                }

                if (PLCSession.CheckUserOption("HIDEBATWLS"))
                {
                    try { RemoveChildItem("BatchWorklist", "WorklistSearch"); }
                    catch { }
                }
            }

            // only display the locker transfer button when these flags are true, else hide them
            // added validation to hide the menu when web inquiry mode is on
            if (!PLCSession.CheckUserOption("HANDLE") || !PLCSession.CheckUserOption("LOCKTRANS") || PLCCommon.instance.IsWebInquiryMode())
            {
                mi = SideMenu.FindItem("LockerTransfer");
                if (mi != null) SideMenu.Items.Remove(mi);
            }

            if (!PLCSession.CheckUserOption("ATFEXPORT"))
            {
                mi = SideMenu.FindItem("ATFExport");
                if (mi != null) SideMenu.Items.Remove(mi);
            }

            if (!PLCSession.CheckUserOption("WORKSTN"))
            {
                try { RemoveChildItem("Setup", "CLIENTSETUP"); } //Workstation Setup
                catch { }
            }

            if (PLCSession.CheckUserOption("HIDEUSERPR"))
            {
                try { RemoveChildItem("Setup", "UserPref"); } //Workstation Setup
                catch { }
            }

            if (!PLCSession.CheckUserOption("LABWORK"))
            {
                mi = SideMenu.FindItem("LABWORK");
                if (mi != null) SideMenu.Items.Remove(mi);
            }

            if (!PLCSession.CheckUserOption("PRELOG2D"))
            {
                mi = SideMenu.FindItem("EMSSCAN2D");
                if (mi != null) SideMenu.Items.Remove(mi);
            }

            if (PLCSession.GetLabCtrl("USES_PRELOG_DISCOVERY_PACKET") != "T")
            {
                mi = SideMenu.FindItem("DISCOVERY");
                if (mi != null) SideMenu.Items.Remove(mi);
            }

            if (PLCSession.GetLabCtrl("USES_ANALYST_SCHEDULER") != "T" || PLCSession.CheckUserOption("HIDESCHED"))
            {
                mi = SideMenu.FindItem("AnalystScheduler");
                if (mi != null) SideMenu.Items.Remove(mi);
            }

            if (!PLCSession.CheckUserOption("CODNA"))
            {
                mi = SideMenu.FindItem("CODNA");
                if (mi != null) SideMenu.Items.Remove(mi);
            }

            if (!PLCSession.CheckUserOption("CDDOCUP"))
            {
                mi = SideMenu.FindItem("CDDOCUP");
                if (mi != null) SideMenu.Items.Remove(mi);
            }

            if (!PLCSession.CheckUserOption("LABDOCUP"))
            {
                mi = SideMenu.FindItem("LABDOCUP");
                if (mi != null) SideMenu.Items.Remove(mi);
            }

            if (!PLCSession.GetLabCtrlFlag("USES_UPS_INTERFACE").Equals("T"))
            {
                mi = SideMenu.FindItem("UPSSHIP");
                if (mi != null) SideMenu.Items.Remove(mi);
            }
            else
            {
                if (!PLCSession.CheckUserOption("UPSWORLDSHIP"))
                {
                    mi = SideMenu.FindItem("UPSSHIP");
                    if (mi != null) mi.Enabled = false;
                }
            }

            if (!PLCSession.GetLabCtrl("SHOW_DUPLICATE_NAMES").ToUpper().Equals("T"))
            {
                mi = SideMenu.FindItem("DUPNAMES");
                if (mi != null) SideMenu.Items.Remove(mi);
            }

            if (PLCSession.GetLabCtrlFlag("USES_NEW_OFFENDER_INTAKE").Equals("T"))
            {
                mi = SideMenu.FindItem("Databank");
                if (mi != null)
                {
                    string newOffenderIntake = string.Empty;
                    if (PLCSession.GetLabCtrl("NEW_OFFENDER_INTAKE_LABEL").Trim().Length > 0)
                        newOffenderIntake = PLCSession.GetLabCtrl("NEW_OFFENDER_INTAKE_LABEL");
                    else
                        newOffenderIntake = "Databank Intake";
                    mi.Text = newOffenderIntake;
                }
            }
            else
            {
                mi = SideMenu.FindItem("Databank");
                if (mi != null) SideMenu.Items.Remove(mi);
            }

            if (!PLCSession.CheckUserOption("QUEUESAMPLES"))
            {
                mi = SideMenu.FindItem("QueueSamples");
                if (mi != null) SideMenu.Items.Remove(mi);
            }

            if(!PLCSession.GetLabCtrl("DATABANK_VERSION").Equals("3"))
            {
                mi = SideMenu.FindItem("SubmissionEnvelope");
                if (mi != null) SideMenu.Items.Remove(mi);
            }

            if (!PLCSession.CheckUserOption("NAMUS"))
            {
                mi = SideMenu.FindItem("NAMUS");
                if (mi != null) SideMenu.Items.Remove(mi);
            }

            if(!PLCSession.CheckUserOption("DCJSSUBMIT"))
            {
                mi = SideMenu.FindItem("DCJS");
                if (mi != null) SideMenu.Items.Remove(mi);
            }
        }

        private void RemoveChildItem(string parentValue, string childValue)
        {
            MenuItem parentItem = SideMenu.FindItem(parentValue);
            MenuItem childItem = null;

            if (parentItem.ChildItems.Count > 0)
            {
                foreach (MenuItem child in parentItem.ChildItems)
                {
                    if (child.Value == childValue)
                    {
                        childItem = child;
                        break;
                    }
                }
            }

            parentItem.ChildItems.Remove(childItem);

            if (parentItem.ChildItems.Count == 0)
                SideMenu.Items.Remove(parentItem);
        }

        private void HighlightMenuItem(string itemval)
        {
            Menu sidemenu = (Menu) PLCCommon.FindControlWithin("SideMenu", this);
            MenuItem item = sidemenu.FindItem(itemval);
            if (item != null)
            {
                // Save/Restore selectable setting to allow menu items such as Search to be highlighted.
                bool isselectable = item.Selectable;
                item.Selectable = true;

                // Selecting item will set the 'sidemenuselected' css class.
                item.Selected = true;

                item.Selectable = isselectable;
            }
        }

        private MenuItem GetMenuItem(string key)
        {
            return SideMenu.FindItem(key);
        }

        private MenuItem GetMenuItem(string parentkey, string key)
        {
            MenuItem parentItem = GetMenuItem(parentkey);
            MenuItem childItem = null;

            if (parentItem != null)
                foreach (MenuItem child in parentItem.ChildItems)
                    if (child.Value == key)
                    {
                        childItem = child;
                        break;
                    }
            return childItem;
        }

        private void RemoveMenuItem(string key)
        {
            MenuItem menuItem = GetMenuItem(key);
            if (menuItem != null)
                SideMenu.Items.Remove(menuItem);
        }

        private void RemoveMenuItem(string parentkey, string key)
        {
            MenuItem parentItem = GetMenuItem(parentkey);
            MenuItem childItem = GetMenuItem(parentkey, key);

            if (parentItem != null)
                if (childItem != null)
                    parentItem.ChildItems.Remove(childItem);
            if (parentItem.ChildItems.Count == 0)
                RemoveMenuItem(parentkey);
        }



        private void CODNAMenuItem(MenuItem miNewCase)
        {
            if (PLCSession.GetLabCtrl("DATABANK_MODE").Equals("T"))
            {
                MenuItem QuickCreateCODNA = new MenuItem();
                QuickCreateCODNA.Text = "New Convicted Offender";
                QuickCreateCODNA.Value = "NewConvictedOffender";
                QuickCreateCODNA.NavigateUrl = "QuickCreateCODNA.aspx";
                miNewCase.ChildItems.Add(QuickCreateCODNA);
            }
        }

        /// <summary>
        /// Gets the menu item text
        /// </summary>
        /// <param name="key">Menu item value</param>
        /// <returns>Menu item text</returns>
        public string GetMenuItemText(string key)
        {
            return GetMenuItem(key).Text;
        }

        private void ServerAlert(string message)
        {
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "SERVER_ALERT", "Alert('" + message + "');", true);
        }
    }
}