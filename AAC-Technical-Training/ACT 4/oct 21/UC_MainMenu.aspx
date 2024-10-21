<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UC_MainMenu.ascx.cs" Inherits="BEASTiLIMS.UC_MainMenu" %>
<%@ Register Assembly="PLCCONTROLS" Namespace="PLCCONTROLS" TagPrefix="cc1" %>
<%@ Register Src="~/PLCWebCommon/PLCPageHead.ascx" TagName="PLCPageHead" TagPrefix="uc5" %>

<cc1:PLCSessionVars ID="PLCSessionVars1" runat="server" />
<uc5:PLCPageHead runat="server" ID="pagehead1" include="utility" />
<table align="left" class="sitecolor sidemenubar" cellpadding="0" cellspacing="0">
    <tr>
        <td valign="top">
            <div style="float: left; width: 26px; padding: 7px 5px 3px 5px;" class="sidemenuheader">
                <input class="UCMainMenuCollapseButton" style="border-width: 0px;" src="Images/arrowbullet_left.gif" type="image" />
            </div>
            <div class="UCMainMenuItems">
                <div style="padding: 5px 5px 5px 5px;" class="sidemenuheader">
                    Menu
                </div>
                <cc1:PLCMenu ID="SideMenu" runat="server" Orientation="Vertical" SkinID="SideMenu" Width="100%" OnMenuItemClick="SideMenu_MenuItemClick">
                    <Items>
                        <asp:MenuItem Text="$Menu_LIMS_Dashboard" Value="Dashboard" NavigateUrl="~/Dashboard.aspx"></asp:MenuItem>
                        <asp:MenuItem Text="$Menu_LIMS_NewCase" Value="NewCase" NavigateUrl=""></asp:MenuItem>
                        <asp:MenuItem Text="$Menu_LIMS_DataBank" Value="Databank" NavigateUrl="" Selectable="false">
                            <asp:MenuItem Text="$Menu_LIMS_DataBank_Receive" Value="DatabankReceive" NavigateUrl="~/DatabankReceiveKit.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_DataBank_Wrangling" Value="DatabankWrangling" NavigateUrl="~/DatabankWranglingKit.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_DataBank_Entry" Value="DatabankEntry" NavigateUrl="~/DatabankEntry.aspx"></asp:MenuItem>
                        </asp:MenuItem>
                        <asp:MenuItem Text="$Menu_LIMS_Search" Value="Search" Selectable="false">
                            <asp:MenuItem Text="$Menu_LIMS_Search_Case" Value="CaseSearch" NavigateUrl="~/CaseSearch.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Search_Assign" Value="AssignmentSearch" NavigateUrl="~/AssignSearch.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Search_ServiceRequest" Value="SRSearch" NavigateUrl="~/ServiceSearch.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Search_AuditLoc" Value="Location" NavigateUrl="~/AuditLoc.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Search_Custody" NavigateUrl="~/CustodyInquiry.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Search_Report" Value="ReportSearch" NavigateUrl="~/LabRptSearch.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Search_Container" Value="ContainerSearch" NavigateUrl="~/ContainerSearch.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Search_BatchAssign" Value="BatchAssignment" NavigateUrl="~/BatchAssign.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Search_Activity" Value="ActivitySearch" NavigateUrl="~/ActivitySearch.aspx"></asp:MenuItem>
							<asp:MenuItem Text="$Menu_LIMS_Search_ItemTracker" Value="ItemTracker" NavigateUrl="~/ItemTracker.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Search_BreathAlcohol" Value="BreathAlcohol" NavigateUrl="~/BreathAlcohol.aspx"></asp:MenuItem>
                        </asp:MenuItem>
                        <asp:MenuItem Text="$Menu_LIMS_BulkContainer" Value="BulkContainer" NavigateUrl="~/BulkContainer.aspx"></asp:MenuItem>
                        <asp:MenuItem Text="$Menu_LIMS_Disposition" Value="Disposition" Selectable="false">
                            <asp:MenuItem Text="$Menu_LIMS_Disposition_DispoBatch" NavigateUrl="~/DispoBatch.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Disposition_DispoApproval" NavigateUrl="~/DispoApproval.aspx"></asp:MenuItem>
                        </asp:MenuItem>
                        <asp:MenuItem Text="$Menu_LIMS_ServiceRequest" Value="ServiceRequest" Selectable="false">
                            <asp:MenuItem Text="$Menu_LIMS_ServiceRequest_PhotoRequest" Value="PhotoRequest" NavigateUrl="~/PhotoRequest.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Request_SRReview" Value="SRReview" NavigateUrl="~/ServiceRequestReview.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_ServiceRequest_DeputyRequest" Value="DeputyRequest" NavigateUrl="~/DeputyRequest.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_ServiceRequest_CSIRequest" Value="CSIRequest" NavigateUrl="~/CSIRequest.aspx"></asp:MenuItem>
                        </asp:MenuItem>
                        <asp:MenuItem Text="$Menu_LIMS_Inventory" Value="Inventory" Selectable="false" >
                            <asp:MenuItem Text="$Menu_LIMS_Inventory_Container" Value="ContainerInventory" NavigateUrl="~/ContainerInventory.aspx" ></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Inventory_Location" Value="LocationInventory" NavigateUrl="~/InventoryMulti.aspx" ></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Inventory_LAM" Value="LAMInventory" NavigateUrl="~/InventoryLAM.aspx" ></asp:MenuItem>
                        </asp:MenuItem>
                        <asp:MenuItem Text="$Menu_LIMS_ChemInventory" Value="ChemInv" NavigateUrl="~/ChemInv/ChemInv.aspx"></asp:MenuItem>
                        <asp:MenuItem Text="$Menu_LIMS_Admin" Value="Admin" Selectable="false">
                            <asp:MenuItem Text="$Menu_LIMS_Admin_AuditLog" Value="AuditLog" NavigateUrl="~/AuditLog.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Admin_Config" Value="Config" NavigateUrl="~/ConfigTAB1Users.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Admin_DeptConfig" Value="DeptConfig"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Admin_GetSig" Value="GetSig" NavigateUrl="~/GetSig.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Admin_WebLabCtrl" Value="WebLabCtrl" NavigateUrl="~/WebLabCtrl.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Admin_DeptCtrlSetup" Value="DeptCtrlSetup" NavigateUrl="~/DeptCtrl.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Admin_SystemFlags" Value="SystemFlags" NavigateUrl="~/SystemFlags.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Admin_SAK" Value="SAK" NavigateUrl="~/SAK.aspx"></asp:MenuItem>                           
                            <asp:MenuItem Text="$Menu_LIMS_Admin_PrelogSetup" Value="PrelogSetup" Selectable="false" >
                                <asp:MenuItem Text="$Menu_LIMS_Admin_PrelogSetup_PrelogUser" Value="PrelogUser" NavigateUrl="~/ConfigWebUser.aspx"></asp:MenuItem>
                                <asp:MenuItem Text="$Menu_LIMS_Admin_PrelogSetup_PrelogDocument" Value="PrelogDocument" NavigateUrl="~/ConfigPrelogDocs.aspx"></asp:MenuItem>
                                <asp:MenuItem Text="$Menu_LIMS_Admin_PrelogSetup_PrelogGlobalNotice" Value="PrelogGlobalNotice" NavigateUrl="~/ConfigPrelogDeptNotice.aspx"></asp:MenuItem>                              
                            </asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Admin_CODNASetup" Value="CODNASetup" Selectable="false" >
                                <asp:MenuItem Text="$Menu_LIMS_Admin_CODNASetup_CODNAUser" Value="CODNAUser" NavigateUrl="~/ConfigCODNAUser.aspx"></asp:MenuItem>
                                <asp:MenuItem Text="$Menu_LIMS_Admin_CODNASetup_CODNADocument" Value="CODNADocument" NavigateUrl="~/ConfigCODNADocs.aspx"></asp:MenuItem>                              
                            </asp:MenuItem>
                        </asp:MenuItem>
                        <asp:MenuItem Text="$Menu_LIMS_Reports" Value="Reports" Selectable="false">                            
                            <asp:MenuItem Text="$Menu_LIMS_Reports_Analytics" Value="Analytics" NavigateUrl="~/ChartView.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Reports_ActivityLog" Value="ActivityLog" NavigateUrl="~/ManagementReports/StockReports.aspx?reportstart=4"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Reports_ActivityReport" Value="ActivityReport" NavigateUrl="~/ManagementReports/StockReports.aspx?reportstart=7"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Reports_Assign" Value="Assignments" NavigateUrl="~/ManagementReports/StockReports.aspx?reportstart=0"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Reports_Case" Value="Cases" NavigateUrl="~/ManagementReports/StockReports.aspx?reportstart=1"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Reports_CustomReports" Value="CustomReports" NavigateUrl="~/PLCWebCommon/CustomReports.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Reports_ItemType" Value="ItemType" NavigateUrl="~/ManagementReports/StockReports.aspx?reportstart=8"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Reports_Items" Value="Items" NavigateUrl="~/ManagementReports/StockReports.aspx?reportstart=9"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Reports_Pending" Value="Pending" NavigateUrl="~/ManagementReports/StockReports.aspx?reportstart=6"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Reports_Courier" Value="Courier" NavigateUrl="~/PullList.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Reports_ReviewApproval" Value="ReviewApproval" NavigateUrl="~/ManagementReports/StockReports.aspx?reportstart=3"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Reports_Statistics" Value="Statistics" NavigateUrl="~/ManagementReports/StockReports.aspx?reportstart=10"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Reports_Submission" Value="Submission" NavigateUrl="~/ManagementReports/StockReports.aspx?reportstart=2"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Reports_Summary" Value="Summary" NavigateUrl="~/ManagementReports/StockReports.aspx?reportstart=11"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Reports_TurnAround" Value="TurnAround" NavigateUrl="~/ManagementReports/StockReports.aspx?reportstart=5"></asp:MenuItem>
                        </asp:MenuItem>
                        <asp:MenuItem Text="$Menu_LIMS_ItemList" Value="ItemList" NavigateUrl="~/ListMulti.aspx"></asp:MenuItem>
                        <asp:MenuItem Text="$Menu_LIMS_RemoteTransfer" Value="RemoteTransfer" NavigateUrl="~/RemoteTransfer.aspx"></asp:MenuItem>
                        <asp:MenuItem Text="$Menu_LIMS_ActivityLog" Value="ActivityLog" NavigateUrl="~/ActivityLog.aspx"></asp:MenuItem>
                        <asp:MenuItem Text="$Menu_LIMS_Documents" Value="Documents" NavigateUrl="~/Documents.aspx"></asp:MenuItem>
                        <asp:MenuItem Text="$Menu_LIMS_Setup" Value="Setup" Selectable="false" >
                            <asp:MenuItem Text="$Menu_LIMS_Setup_UserPref" Value="UserPref" NavigateUrl="~/UserPreferences.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Setup_Client" Value="CLIENTSETUP" NavigateUrl="~/ClientSetup.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Setup_UserInfo" Value="UserInfo" NavigateUrl="~/UserInfo.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Setup_LocalFiles" Value="DownloadLocalFiles" NavigateUrl="~/DownloadLocalFiles.aspx"></asp:MenuItem>
                        </asp:MenuItem>
                        <asp:MenuItem Text="$Menu_LIMS_DNA" Value="DNA" Selectable="false" >
                            <asp:MenuItem Text="$Menu_LIMS_DNA_Worklist" Value="Worklist" NavigateUrl="~/DNAWorklist.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_DNA_Batch" Value="Batch" NavigateUrl="~/DNABatch.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_DNA_Cryobox" Value="Cryobox" NavigateUrl="~/DNACryoBox.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_DNA_CODISExport" Value="CODISExport" NavigateUrl="~/CODISExport.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_DNA_CODISImport" Value="CODISImport" NavigateUrl="~/CODISImport.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="Court Order" Value="CourtOrder" NavigateUrl="~/CODNAPrelog/CODNAPrelogCase.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_DNA_DCJS" Value="DCJSExport" NavigateUrl="javascript: CallPLCWebMethod('PLC_WebOCX', 'DCJSEXP', '', true, CheckBrowserWindowID)"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_DNA_CODISUpload" Value="CODISUpload" NavigateUrl="javascript: CallPLCWebMethod('PLC_WebOCX', 'CODISUPLOAD', '', true, CheckBrowserWindowID)"></asp:MenuItem>
                        </asp:MenuItem>
                        <asp:MenuItem Text="$Menu_LIMS_Instrument" Value="Instrument" Selectable="false">
                            <asp:MenuItem Text="$Menu_LIMS_Instrument_BatchCreate" Value="BatchCreate" NavigateUrl="~/InstBatchCreate.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Instrument_BatchResults" Value="BatchResults" NavigateUrl="~/InstBatchResults.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Instrument_BatsBatch" Value="BatsBatch" NavigateUrl="~/BatsBatch.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Instrument_Olympus" Value="Olympus" NavigateUrl="~/olympus.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Instrument_InstView" Value="InstView" NavigateUrl="~/InstmntInstView.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Instrument_InstrumentInterface" Value="InstrumentInterface" NavigateUrl="~/InstrumentInterface.aspx"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Instrument_InstPop" Value="InstPop" NavigateUrl="javascript: CallPLCWebMethod('PLC_WebOCX', 'INSTPOP', '', true, CheckBrowserWindowID)"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Instrument_Upload" Value="InstUpload" NavigateUrl=""></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_Instrument_ResultsUpload" Value="ResultsUpload" NavigateUrl="~/ResultsUpload.aspx"></asp:MenuItem>
                        </asp:MenuItem>
                        <asp:MenuItem Text="$Menu_LIMS_QMS" Value="QMS"  Selectable="false">
                            <asp:MenuItem Text="$Menu_LIMS_QMS_DocumentControl" Value="DocumentControl" NavigateUrl="~/QMS.aspx?v=1"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_QMS_Programs" Value="Programs" NavigateUrl="~/QMS.aspx?v=2"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_QMS_Audits" Value="Audits" NavigateUrl="~/QMS.aspx?v=3"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_QMS_Proficiency" Value="Proficiency" NavigateUrl="~/QMS.aspx?v=4"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_QMS_AnalystQCFlag" Value="AnalystQCFlag" NavigateUrl="~/QMS.aspx?v=5"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_QMS_Testimony" Value="Testimony" NavigateUrl="~/QMS.aspx?v=6"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_QMS_Training" Value="Training" NavigateUrl="~/QMS.aspx?v=7"></asp:MenuItem>
                        </asp:MenuItem>
                        <asp:MenuItem Text="$Menu_LIMS_ControlledDocuments" Value="ControlledDocuments" NavigateUrl="~/ControlledDocuments.aspx"></asp:MenuItem>
                        <asp:MenuItem Text="$Menu_LIMS_LockerTransfer" Value="LockerTransfer" NavigateUrl="~/LockerTransfer/Lockers.aspx"></asp:MenuItem>
                        <asp:MenuItem Text="$Menu_LIMS_Worklist" Value="BatchWorklist" Selectable="false">
                            <asp:MenuItem Text="$Menu_LIMS_WorklistCreate" Value="WorklistCreate" NavigateUrl="~/BatchWorklist.aspx?m=c"></asp:MenuItem>
                            <asp:MenuItem Text="$Menu_LIMS_WorklistSearch" Value="WorklistSearch" NavigateUrl="~/BatchWorklist.aspx"></asp:MenuItem>
                        </asp:MenuItem>
                        <asp:MenuItem Text="ATF Export" Value="ATFExport" NavigateUrl="~/ATFExport.aspx"></asp:MenuItem>
                        <asp:MenuItem Text="Lab Workflow" Value="LABWORK" NavigateUrl="~/LabWorkflow.aspx"></asp:MenuItem>
                        <asp:MenuItem Text="Scan 2D" Value="EMSSCAN2D" NavigateUrl="javascript: CallPLCWebMethod('PLC_WebOCX', 'EMSSCAN2D', '', true, CheckBrowserWindowID)"></asp:MenuItem>
                        <asp:MenuItem Text="$Menu_LIMS_Discovery" Value="DISCOVERY"></asp:MenuItem>
                        <asp:MenuItem Text="$Menu_LIMS_AnalystScheduler" Value="AnalystScheduler" NavigateUrl="~/AnalystScheduler.aspx"></asp:MenuItem>
                        <asp:MenuItem Text="$Menu_LIMS_CODNA" Value="CODNA" NavigateUrl="~/CODNA.aspx"></asp:MenuItem>
                        <asp:MenuItem Text="$Menu_LIMS_DocumentUploader" Value="CDDOCUP" NavigateUrl="javascript: CallPLCWebMethod('PLC_WebOCX', 'CDDOCUP', '', true, CheckBrowserWindowID)"></asp:MenuItem>
                        <asp:MenuItem Text="$Menu_LIMS_LabDocUploader" Value="LABDOCUP" NavigateUrl="javascript: CallPLCWebMethod('PLC_WebOCX', 'LABDOCUPLOADER', '', true, CheckBrowserWindowID)"></asp:MenuItem>
                        <asp:MenuItem Text="$Menu_LIMS_UPS" Value="UPSSHIP" NavigateUrl="~/UPSShip.aspx"></asp:MenuItem>
                        <asp:MenuItem Text="$Menu_LIMS_DuplicateNames" Value="DUPNAMES" NavigateUrl="~/DuplicateNames.aspx"></asp:MenuItem>
                        <asp:MenuItem Text="$Menu_LIMS_QueueSamples" Value="QueueSamples" NavigateUrl=""></asp:MenuItem>
                        <asp:MenuItem Text="$Menu_LIMS_SubEnvelope" Value="SubmissionEnvelope" NavigateUrl="SubmissionEnvelope.aspx"></asp:MenuItem>
                        <asp:MenuItem Text="$Menu_LIMS_NAMUS" Value="NAMUS" NavigateUrl="~/NamUsBatchIntake.aspx"></asp:MenuItem>
                        <asp:MenuItem Text="$Menu_LIMS_DCJS" Value="DCJS" NavigateUrl="~/DCJSSubmission.aspx"></asp:MenuItem>
                        <asp:MenuItem Text="" Value="Exercise Arvin" NavigateUrl="~/Exercise_Arvinn.aspx"></asp:MenuItem>
                        <asp:MenuItem Text="$Global_Logout" Value="Logout"></asp:MenuItem>
                    </Items>
                </cc1:PLCMenu>
            </div>
        </td>                        
    </tr>                        
</table>

<script type="text/javascript">
    function isMobile() {
      var ua = navigator.userAgent || navigator.vendor || window.opera;

        if  ( ua.match( /iPad/i ) || ua.match( /iPhone/i ) || ua.match( /iPod/i ) || ua.match( /android/i ) )
            return true;
        else
            return false;
    }

    function init() {
        // The lookup table that maps what menu item should be highlighted for
        // the corresponding active page.
        // ["<page name>", "menu item text"]
        //
        // Ex. ["quickcreate2", "New Case"]
        // corresponds to 'quickcreate2.aspx' and "New Case" menu item.
        // When quickcreate2.aspx page is loaded, highlight the "New Case" menu item.
        var menuselTable = [
            ["dashboard", "Dashboard"],
            ["config.*", "Admin"],
            ["deptpic", "Admin"],
            ["lasdnarcorgn_edit", "Admin"],
            ["pwdconfig", "Admin"],
            ["srquestion", "Admin"],
            ["quickcreate_advanced", '<%= NewCaseText %>'],
            ["quickcreatesimple", '<%= NewCaseText %>'],
            ["quickcreatecm", '<%= NewCaseText %>'],
            ["quickcreatevc", '<%= NewCaseText %>'],
            ["quickcreate2", '<%= NewCaseText %>'],
            ["quickcreatedefault", '<%= NewCaseText %>'],
            ["quickcreatecodna", '<%= NewCaseText %>'],
            ["quickcreateprelimscase", '<%= NewCaseText %>'],
            ["bulkintake", '<%= NewCaseText %>'],
            ["batchvoucher", '<%= NewCaseText %>'],
            ["proficiencytest", '<%= NewCaseText %>'],
            ["prelogbatchintake", '<%= NewCaseText %>'], 
            ["quickcreate_aa", '<%= NewCaseText %>'],
            ["quickcreate_x", '<%= NewCaseText %>'],
            ["casesearch", "Search"],
            ["assignsearch", "Search"],
            ["servicesearch", "Search"],
            ["auditloc", "Search"],
            ["courier", "Search"],
            ["custodyinquiry", "Search"],
            ["labrptsearch", "Search"],
            ["containersearch", "Search"],
            ["bulkcontainer", "Bulk Container"],
            ["dispobatch", "Disposition"],
            ["dispoapproval", "Disposition"],
            ["photorequest", "Service Request"],
            ["servicerequestreview", "Service Request"],
            ["inventorymulti", "Inventory"],
            ["auditlog", "Admin"],
            ["configtab1users", "Admin"],
            ["getsig", "Admin"],
            ["weblabctrl", "Admin"],
            ["sak", "Admin"],
            ["listmulti", "Item List"],
            ["activitylog", "Activity Log"],
            ["documents", "Documents"],
            ["userpreferences", "Setup"],
            ["clientsetup", "Setup"],
            ["driverdownload", "Setup"],
            ["DNAWorklist", "DNA"],
            ["DNABatch", "DNA"],
            ["CODISExport", "DNA"],
            ["CODISImport", "DNA"],
            ["instbatchcreate.aspx", "Instrument"],
            ["instbatchresults.aspx", "Instrument"],
            ["batsbatch.aspx", "Instrument"],
            ["olympus.aspx", "Instrument"],
            ["instmntinstview.aspx", "Instrument"],
            ["qms", "QMS"],
            ["controlleddocuments", "Controlled Documents"],
            ["lasdcourier", "Reports"],
            ["atfexport", "ATF Export"],
            ["analystscheduler", "Scheduler"],
            ["codna", "CODNA"],
            ["upsship", "UPS"],
            ["ManagementReports/StockReports", "Reports"],
            ["PullList", "Reports"],
            ["CustomReports", "Reports"],
            ["DuplicateNames", "Duplicate Names"],
            ["SubmissionEnvelope", "Submission Envelope"]
        ];

        // Whenever a page containing the menu is loaded, highlight the 
        // corresponding menu item for the active page.
        HighlightMenuItemCorrespondingToPage(menuselTable);

        // Check whether menu was previously collapsed.
        // However, the menu will always be visible when dashboard page is loaded.
        var isMenuPreviouslyVisible = ($.cookie("UCMAINMENU_VISIBLE") == "false") ? false : true;

        if (!isMenuPreviouslyVisible && !IsPageCurrentlyLoaded("dashboard.aspx")) {
            ToggleUCMainMenuVisibility();
        }

        // Collapse/expand the main menu when the collapse/expand button is clicked.
        $(".UCMainMenuCollapseButton").click(function() {
            ToggleUCMainMenuVisibility();

            // Remember menu state so we can restore it to this state when menu 
            // is loaded in a different page.
            UpdateUCMainMenuCookieState();
            return false;
        });

        $("table.sidemenubar").parentsUntil("tr", "td").addClass("sitecolor border");

        if (isMobile()) {
            $("[href*='InstBatchCreate.aspx']," +
              "[href*='InstBatchResults.aspx']," +
              "[href*='BatsBatch.aspx']," +
              "[href*='olympus.aspx']," +
              "[href*='QMS.aspx']," +
              "[href*='InstmntInstView.aspx']," +
              "[href*='CODNA.aspx']," +

              // DNA
              "[href*='DNAWorklist.aspx']," +
              "[href*='DNABatch.aspx']," +
              "[href*='DNACryoBox.aspx']," +

              // Setup
              "[href*='ClientSetup.aspx']").attr("disabled", true).attr("href", "javascript:void(0);");
        }

        if (hasTouchEvents()) {
            var usePointer = hasPointerEvents(); // windows 8 and 8.1 don't have touch event, touch only added in 8.1 Update 1
            $(".UCMainMenuItems > table > tbody > tr").filter(function () {
                return $(this).find("a").attr("href") == "#";
            }).each(function () {
                this.addEventListener((usePointer ? "pointerdown" : "touchstart"), function (e) {
                    if (window[$(".UCMainMenuItems > table").attr("id") + "_Data"])
                        window[$(".UCMainMenuItems > table").attr("id") + "_Data"].disappearAfter = -1;
                    Menu_HoverStatic(this);
                });
                this.addEventListener((usePointer ? "pointerup" : "touchend"), function (e) {
                    Menu_Unhover(this);
                    e.preventDefault(); // Prevent click from triggering
                });
                if (usePointer) { // hack click
                    this.addEventListener("click", function (e) {
                        e.preventDefault();
                        e.stopPropagation();
                    });
                }
            });
        }
    }
</script>
