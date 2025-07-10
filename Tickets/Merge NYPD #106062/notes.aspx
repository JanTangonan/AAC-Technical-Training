<%@ Master Language="C#" AutoEventWireup="true" CodeBehind="CHEMINVHOME.master.cs" Inherits="ChemInv.CHEMINVHOME"%>

<%@ Register Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" Namespace="System.Web.UI" TagPrefix="asp" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<%@ Register Assembly="PLCCONTROLS" Namespace="PLCCONTROLS" TagPrefix="PLC" %>
<%@ Register Src="~/PLCWebCommon/PLCWebControl.ascx" TagName="PLCWebControl" TagPrefix="uc1" %>
<%@ Register Src="~/PLCWebCommon/CodeHead.ascx" TagName="CodeHead" TagPrefix="uc1" %>
<%@ Register Src="~/PLCWebCommon/UC_PLCCopyRights.ascx" TagName="UC_PLCCopyRights" TagPrefix="uc2" %>
<%@ Register Src="~/PLCWebCommon/UC_CustomerTitle.ascx" TagName="UC_CustomerTitle" TagPrefix="uc3" %>
<%@ Register Src="PLCChemInvSearch.ascx" TagName="ItemSearch" TagPrefix="uc4" %>
<%@ Register Src="~/PLCWebCommon/PLCPageHead.ascx" TagName="PLCPageHead" TagPrefix="uc5" %>
<%@ Register Src="~/PLCWebCommon/UC_SessionTimer.ascx" TagName="UC_SessionTimer" TagPrefix="uc6" %>

<!docTyPe hTml>
<html>
<head runat="server">
<meta http-equiv="Page-Enter" content="blendTrans(Duration=0)" />
<title>Chemical Inventory</title>
<uc5:PLCPageHead runat="server" ID="pagehead1" include="codehead,plcdbpanel,maskedinput,jquery,jquery-ui,utility,codemultipick" />
<link rel="stylesheet" type="text/css" href="<%# ResolveUrl("~/ChemInv/ChemInv.css") %>" />
<link rel="shortcut icon" href="~/images/favicon.ico" />
<asp:ContentPlaceHolder ID="head" runat="server">
</asp:ContentPlaceHolder>

    <style type="text/css">
        .hide {
            display: none;
        }

        .cheminv-header {
            font-size: 20px; font-weight: bold; display: block; text-align: center; margin-bottom: 10px;
        }
        .chemprintbtnpanel {padding: 0; margin: 0.5em 0 0;}
        .chemprintbtnpanel input {margin: 0; padding-left: 0.3em; padding-right: 0.3em;}
        
        /* number-spinner */
        .number-spinner 
        {
            display: inline;
        }
        
        .number-spinner button, 
        .number-spinner input
        {
            height: 20px;
            display: inline-block;
            vertical-align: middle;
        }
        
        .number-spinner input
        {
            margin: 0px;
            width: 5em;
            text-align: center;
            border: 1px solid lightgray;
        }
        
        .number-spinner button.increment,
        .number-spinner button.decrement
        {
            width: 1.7em;
            padding: 0px;
            margin: 0px;
            text-align: center;
            border-radius: 50%;
            border: 1px solid lightgray;
            cursor: pointer;
        }
        
        .number-spinner button.increment:hover,
        .number-spinner button.decrement:hover
        {
            box-shadow: 0 2px 4px 0 rgba(0, 0, 0, 0.2), 0 3px 10px 0 rgba(0, 0, 0, 0.19);
        }
        /* end number-spinner */

        .modalPopup2 {
            z-index: 10003 !important;
        }
        .modalBackground2 {
            z-index: 10002 !important;
        }
        .timeoutpopup {
            z-index: 10005 !important;
        }
        .ui-widget-overlay {
            z-index: 10004 !important;
        }
        .multi-print-label {
            font-size: 14px;
        }
        .multi-print-label input::-webkit-outer-spin-button,
        .multi-print-label input::-webkit-inner-spin-button {
            -webkit-appearance: none;
            margin: 0;
        }
    </style>

    <script type="text/javascript" language="javascript">
        function Init() {
            var $txtDup = $("[id$='txtPopupDuplicateTimes']").addClass("noXButton");
            $txtDup.mask("9?9999");
            // Override fix for header expanding
            setTimeout(function () { $("[id$='_div_header']").css("height", "auto") });
        }

        function AssignDate(condition, dateFrom, dateTo) {
            var returnFrom, returnTo;
            var date = new Date();
            var thisMonth = date.getMonth() + 1;
            if (thisMonth.toString().length == 1) {
                thisMonth = "0" + thisMonth;
            }
            var thisYear = date.getFullYear();
            var thisDay = date.getDate();
            if (thisDay.toString().length == 1) {
                thisDay = "0" + thisDay;
            }

            switch (condition) {
                case "thisMonth": // this month
                    var thisMonthMax = GetDaysInMonth(thisMonth, thisYear);

                    returnFrom = thisMonth + "/01/" + thisYear;
                    returnTo = thisMonth + "/" + thisMonthMax + "/" + thisYear;
                    break;

                case "lastMonth": // last month
                    var lastMonth = thisMonth - 1;
                    if (lastMonth == 0) {
                        lastMonth = 12;
                        thisYear = thisYear - 1;
                    }
                    if (lastMonth.toString().length == 1) {
                        lastMonth = "0" + lastMonth;
                    }
                    
                    var lastMonthMax = GetDaysInMonth(lastMonth, thisYear);

                    returnFrom = lastMonth + "/01/" + thisYear;
                    returnTo = lastMonth + "/" + lastMonthMax + "/" + thisYear;
                    break;

                case "thisYear": // this year
                    returnFrom = "01/01/" + thisYear;
                    returnTo = "12/31/" + thisYear;
                    break;

                case "lastYear": // last year
                    var lastYear = thisYear - 1;

                    returnFrom = "01/01/" + lastYear;
                    returnTo = "12/31/" + lastYear;
                    break;
                case "today": //today
                    var dateToday = thisMonth + "/" + thisDay + "/" + thisYear;

                    returnFrom = dateToday;
                    returnTo = dateToday;
                    break;
                case "yesterday": //yesterday
                    var lastDay = thisDay - 1;
                    if (lastDay == 0) {
                        thisMonth = thisMonth - 1;
                        if (thisMonth == 0) {
                            thisMonth = 12;
                            thisYear = thisYear - 1;
                        }
                        if (thisMonth.toString().length == 1) {
                            thisMonth = "0" + thisMonth;
                        }
                        lastDay = GetDaysInMonth(lastMonth, thisYear);
                    }
                    if (lastDay.toString().length == 1) {
                        lastDay = "0" + lastDay;
                    }

                    returnFrom = thisMonth + "/" + lastDay + "/" + thisYear;
                    returnTo = thisMonth + "/" + lastDay + "/" + thisYear;                    
                    break;
            }

            //alert(returnFrom + " " + returnTo);

            document.getElementById(dateFrom).value = returnFrom;
            document.getElementById(dateTo).value = returnTo;
            
        }

        function GetDaysInMonth(month, year) {
            var dd = new Date(year, month, 0);
            return dd.getDate();
        }

        function ToggleAsset(obj) {
            if ($(obj).attr('src').indexOf('collapse') > -1) {
                $(obj).parent().parent().find('#divAssetRecs').hide();
                $(obj).attr('src', $(obj).attr('src').replace('collapse', 'expand'));
            }
            else {
                $(obj).parent().parent().find('#divAssetRecs').show();
                $(obj).attr('src', $(obj).attr('src').replace('expand', 'collapse'));
            }
        }

        function ToggleWebLink(toggleValue, panelID) {
            eval($("input[id$='DBPanelFieldsDictionary']").val());
            var WebLinkTextBox = GetDBPanelElement(panelID || "dbpMSDS", "TV_CHEMINV.MSDS_LINK");
            if (panelID == "dbpSupplement" && !WebLinkTextBox.length)
                WebLinkTextBox = $("[id*='trMSDS_LINK'] input[type='text']");
            if (WebLinkTextBox.length > 0) {
                var WebLinkTextBoxObj = WebLinkTextBox.get(0);
                var WebLinkTextBoxParent = WebLinkTextBoxObj.parentNode;

                if ($("#lnkWebLink").length > 0) {
                    $("#lnkWebLink").remove();
                }

                var control = document.createElement("a");
                control.setAttribute("id", "lnkWebLink");
                control.setAttribute("href", "javascript:void(0);");
                control.innerHTML = "Go To Web Link";
                control.setAttribute("display", "block");
                WebLinkTextBoxParent.appendChild(control);

                if (WebLinkTextBoxObj.value == "" || WebLinkTextBoxObj.value == null) {
                    toggleValue = false;
                }

                $("#lnkWebLink").click(function () {
                    openWebLink(WebLinkTextBoxObj.value);
                });
            }

            if (toggleValue == false) {
                $("#lnkWebLink").css("visibility", "hidden");
            } else {
                $("#lnkWebLink").css("visibility", "visible");
            }
        }

        function openWebLink(URLString) {
            if (!(/^([a-z][a-z\d]*:)?\/\//i).test(URLString)) {
                URLString = (/^\\\\/.test(URLString) ? "file:/" : "") + "//" + URLString;
            }

            // For Chrome and Firefox
            if (noOCX() && (/^(file:\/\/)/i).test(URLString))
                return Alert("This link can only be opened using Internet Explorer. Please contact the LIMS Administrator.");

            window.open(URLString, "_blank");
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server">
    </asp:ScriptManager>
    <div id="content">
   <table align="center" style="width: 1200px;">
   <tr>
   <td>
    <div class="pagechemInv">
                    <asp:UpdatePanel ID="upBarcode" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>                    
                        <uc3:UC_CustomerTitle ID="UC_CustomerTitle_Master2" runat="server" />                                        
                    </ContentTemplate>
                    </asp:UpdatePanel>
                    
                    <div class = "secondcontainer">
                        <table style="width: 100%;">
                            <tr>
                                <td style="width:604px; text-align:left;">
                                    <asp:Label ID="lblName" runat="server" Text="Asset Name" CssClass="hide" ></asp:Label><br />
                                    <asp:Label ID="lblHeader" runat="server" Text="" CssClass="cheminv-header" ></asp:Label>
                                    <asp:Label ID="lblCustodyOfLocation" runat="server" Text="Custody Of: "></asp:Label>
                                </td>
                                <td align="center" valign="middle">
                                    <asp:Label ID="DisposedAsset" runat="server" Text="DISPOSED ASSET" Font-Names="Arial" Font-Bold="true" ForeColor="Red" Font-Size="X-Large" Visible="false"></asp:Label>
                                    <asp:Label ID="lblExpiredAsset" runat="server" Text="EXPIRED ASSET" Font-Names="Arial" Font-Bold="true" ForeColor="Red" Font-Size="X-Large" Visible="false"></asp:Label>
                                    <asp:Label ID="lblSPHeaderText" runat="server" Text="" Font-Names="Arial" Font-Bold="true" ForeColor="Red" Font-Size="X-Large" Visible="false"></asp:Label>
                                </td>
                                <td style="width:220px; text-align:right;">
                                   <div>
                                        <asp:LinkButton ID="lbtnDash" runat="server" Text="Back to Dashboard"  OnClick="lbtnDash_Click"  />
                                        <asp:LinkButton ID="lbtnLogout" runat="server" Text="Logout" OnClick="lbtnLogout_Click" />
                                   </div>
                                 </td>
                            </tr>
                        </table>
                        <PLC:PLCSessionVars ID="PLCSessionVars1" runat="server"  />
                    </div>
                    
                    <!-- Div for Left Panel and Tab  -->
                    <div>
                        <table>
                            <tr>
                                <td style="width:256px; " valign = "top" class = "leftmenuContent">
                               <asp:UpdatePanel ID="UpdatePanel3" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>    
                        <div class="chemInvLeftMenu" style="height: 100%;">
                        <asp:Panel ID="pnlCodeHeads" runat="server" >
                            <table  >
                            <tr>
                            <td>
                                <asp:Label ID="lblDeptCode" runat="server" Text="Lab " ></asp:Label>
                                <uc1:CodeHead ID="chDeptCode" runat="server" TableName="TV_LABCTRL" ControlWidth="260" ></uc1:CodeHead>
                             </td>
                           </tr>
                           <tr>
                           <td>
                                <asp:Label ID="lblSection" runat="server" Text="Section " ></asp:Label>
                                <uc1:CodeHead ID="chSection" runat="server" TableName="TV_EXAMCODE" ControlWidth="260" ></uc1:CodeHead>
                            </td>
                            </tr>    
                                </table>
                        </asp:Panel>
                        
                        <asp:Panel ID="pnlControls" runat="server"    >
                            <asp:Panel ID="pnlWorkWith" runat="server" GroupingText=" Work With ">
                                <asp:RadioButtonList ID="rbtnAssetClass" AutoPostBack="true" OnSelectedIndexChanged="rbtnAssetClass_SelectedIndexChanged" runat="server">                           
                                </asp:RadioButtonList>
                            </asp:Panel>
                            <br />                    
                            <asp:Button ID="btnSearch" Width="72" runat="server" Text="Search..." CssClass="chemInvBtnLeft" OnClick="btnSearch_Click" />
                            <asp:Button ID="btnViewAll" Width="72" runat="server" Text="View All" CssClass="chemInvBtnLeft" OnClick="btnViewAll_Click" />
                            <br />
                            <asp:CheckBox ID="chkIncludeDisposedAssets" runat="server" Text="Include Disposed Assets" AutoPostBack="true" OnCheckedChanged="chkIncludeDisposedAssets_CheckedChanged" />
                            <br />
                            <asp:CheckBox ID="chkbxExcludeKitContent" runat="server" Text="Exclude Kit Content" AutoPostBack="true" OnCheckedChanged="chkbxExcludeKitContent_CheckedChanged" />
                            <div>
                                <fieldset style="margin: 0px; padding: 0px">
                                    <legend>Filter Assets</legend>
                                    <table>
                                        <tr><td>
                                            <asp:Label ID="lblAssetType" runat="server"></asp:Label>
                                        </td></tr>
                                        <tr><td>
                                            <PLC:FlexBox ID="fbAssetType" runat="server" AutoPostBack="true" TableName="TV_ASSETTYP" OnValueChanged="fbFilters_ValueChanged" Width="250"></PLC:FlexBox>
                                        </td></tr>
                                        <tr><td>
                                            <asp:Label ID="lblDistinctAsset" runat="server"></asp:Label>
                                        </td></tr>
                                        <tr><td>
                                            <PLC:FlexBox ID="fbDistinctAsset" runat="server" AutoPostBack="true" TableName="CV_DISTINCTASSETS" OnValueChanged="fbFilters_ValueChanged" ComboBox="true" Width="250"></PLC:FlexBox>
                                        </td></tr>
                                    </table>
                                </fieldset>
                            </div>
                            <div style="overflow: auto;">
                                <PLC:PLCDBGrid ID="gvMainAsset" runat="server" DataKeyNames="CHEM_CONTROL_NUMBER" OnSelectedIndexChanged="gvMainAsset_SelectedIndexChanged" EmptyDataText="No records found."
                                    AllowSorting="true" Height="300px" PLCGridName="CHEMINV_ASSETS" PLCGridWidth="100%" OnSorted="gvMainAsset_Sorted"
                                    AllowPaging='<%# MainAssetGridAllowPaging %>' PageSize='<%# PLCSessionVars1.GetChemInvProperty<int>("ASSET_PAGE_SIZE", 1) %>'
                                    OnPageIndexChanging="gvMainAsset_PageIndexChanging" PageIndex='<%# MainAssetGridPageIndex %>'>
                                </PLC:PLCDBGrid>
                            </div>
                            <asp:Panel ID="pnlDistinctAssets" runat="server" Visible="false" style="margin: 10px 0px; height: 300px; overflow: auto;">
                                <asp:GridView ID="grdDistinctAssets" runat="server" DataKeyNames="ASSET_TYPE,ASSET_NAME" OnSelectedIndexChanged="grdDistinctAssets_SelectedIndexChanged" EmptyDataText="No records found."
                                    AllowSorting="false" AllowPaging="false" AutoGenerateColumns="false" Width="100%">
                                    <Columns>
                                        <asp:CommandField ButtonType="Image" SelectImageUrl="~/Images/toggle_expand.png" ShowSelectButton="true" SelectText="Expand" ItemStyle-VerticalAlign="Top" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="32" />
                                        <asp:TemplateField HeaderText="Assets">
                                            <ItemTemplate>
                                                <div style="padding: 8px 0px;"><%# Eval("ASSET_TYPE") %> - <%# Eval("ASSET_NAME") %> (<%# Eval("ROW_COUNT") %>)</div>
                                                <asp:PlaceHolder ID="plhAssetRecs" runat="server" Visible="false">
                                                    <div id="divAssetRecs" style="padding: 0 0 5 0;">
                                                    <asp:GridView ID="grdAssetRecs" runat="server" Width="100%" DataKeyNames="CHEM_CONTROL_NUMBER" DataSource='<%# AssetRecords(Convert.ToString(Eval("ASSET_TYPE")), Convert.ToString(Eval("ASSET_NAME"))) %>' 
                                                        OnRowCommand="grdAssetRecs_RowCommand"  OnPreRender="grdAssetRecs_PreRender" EmptyDataText="No records found." AllowSorting="false" AutoGenerateColumns="false"
                                                        AllowPaging='<%# PLCSessionVars1.GetChemInvProperty<bool>("USE_ASSET_PAGING", false) %>' PageSize='<%# PLCSessionVars1.GetChemInvProperty<int>("ASSET_PAGE_SIZE", 1) %>'
                                                        OnPageIndexChanging="grdAssetRecs_PageIndexChanging" PageIndex='<%# PLCSessionVars1.GetChemInvProperty<int>("ASSET_PAGE_INDEX", 0) %>'>
                                                        <Columns>
                                                            <asp:ButtonField ButtonType="Link" HeaderText="Barcode" DataTextField="BARCODE" ItemStyle-Width="50" />
                                                            <asp:ButtonField ButtonType="Link" HeaderText="Lot Number" DataTextField="LOT_NUMBER" ItemStyle-Width="100" />
                                                        </Columns>
                                                    </asp:GridView>
                                                    </div>
                                                </asp:PlaceHolder>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                            </asp:Panel>
                            <div class="chemprintbtnpanel">
                                <asp:Button ID="btnPrintList" runat="server" Text="Print List" OnClick="btnPrintList_Click" />
                                <asp:Button ID="btnPrintListX" runat="server" Text="Print List XLS" OnClick="btnPrintListX_Click" />
                                <asp:Button ID="btnPrintListXD" runat="server" Text="Export XLS Data" OnClick="btnPrintListXD_Click" />
                                <asp:Button ID="btnPrintDetails" runat="server" Text="Print Details" OnClick="btnPrintDetails_Click" />
                                <asp:Button ID="btnPrintReceipt" runat="server" Text="Print Receipt" OnClick="btnPrintReceipt_Click" Visible="false" />
                            </div>
                        </asp:Panel>
                        <uc4:ItemSearch ID="ChemInvSearch" runat="server" />
                        <asp:PlaceHolder ID="plhMessageBox" runat="server"></asp:PlaceHolder>
                        </div>
                        </ContentTemplate>
                        <Triggers>
                            <asp:PostBackTrigger ControlID="chkIncludeDisposedAssets" />
                        </Triggers>
                    </asp:UpdatePanel>
                                </td>
                                <td style="width:908px;" valign = "top">
                                   <div>
                                      <asp:Menu ID="menu_main" CssClass="menubar" runat="server" SkinID="TopMenu" OnMenuItemClick="menu_main_MenuItemClick"
                            EnableTheming="True"  ></asp:Menu>
                                   </div>
                                   <div>
                                    <asp:UpdateProgress ID="up1" runat="server">
                            <ProgressTemplate>
                                <div style="position: absolute; margin: 0px; top: 0px; left: 0px; filter: alpha(opacity=60);
                                    -moz-opacity: .60; background-color: #EEE; z-index: 1000000; width: 100%; height: 100%;">
                                </div>
                                <div style="position: absolute; left: 45%; z-index: 1000001; top: 45%; width: 100px;
                                    text-align: center;">
                                    <img src="<%= ResolveUrl("~/Images/ajax-loader.gif") %>" width="50" style="margin: 0px auto;"
                                        alt="" /><br />
                                    <asp:Label ID="lblProgress" runat="server" Text="Loading" CssClass="loading"></asp:Label>
                                </div>
                            </ProgressTemplate>
                        </asp:UpdateProgress>
                        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <asp:ContentPlaceHolder ID="cp" runat="server"></asp:ContentPlaceHolder>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                                   </div>
                                   
                                </td>
                            </tr>
                        </table>
                    </div>
                    
                    
                    <div>
                        <table>
                            <tr>
                                 <td>
                                    <div style="position:fixed; border:1px solid #000; background-color:#cfccbf; bottom: 0px; right: 0px; width: 99.9%;">
                                    <div style="position: relative; display: table; margin: 0px auto;">
                                     <PLC:PLCButtonPanel ID="PLCButtonPanel1" runat="server" PLCShowEditButtons="true" PLCShowAddButton="true"
                            PLCShowDeleteButton="true" OnPLCButtonClick="PLCButtonPanel_PLCButtonClick" 
                            
                              PLCCustomButtons="Images,New Container,Transfer,Print Label,Setup,Reports,Dupe,Instrument Path,UOM" 
                              PLCCustomButtonFixedWidth="65" />
                                    </div></div>
                            </td>
                            <td>
                                 <asp:Button ID="btnHistReportReceipt" runat="server" Text="Reprint Receipt" Visible="false" OnClick="btnHistReportReceipt_Onclick" />
                                 <asp:Button ID="btnIPrintReceipt" runat="server" Text="Print Receipt" Visible="false" OnClick="btnIPrintReceipt_Onclick" />
                                 <asp:Button ID="btnRPrintReceipt" runat="server" Text="Print Receipt" Visible="false" OnClick="btnRPrintReceipt_Onclick" />
                            </td>
                            </tr>
                            <tr id="dupmsgRow" runat="server" style="display: none;">
                                <td><span style="background-color: yellow;">Duplicate <span id="spanDupTimes" runat="server"></span> times.</span></td>
                            </tr>
                           
                        </table>
                      
                       
                              
                               <!-- hidden fields-->
                               
                    <asp:HiddenField ID="hdFields" runat="server" />
                    <asp:HiddenField ID="hdCCN" runat="server" />
                    <asp:HiddenField ID="hdStatusKey" runat="server" />
                    <asp:HiddenField ID="hdSaveSuccess" runat="server" />
                    <asp:HiddenField ID="hdMode" runat="server" />
                    <asp:HiddenField ID="hdDupOrigChemControlNo" runat="server" />
                    
                    </div>
                    <div>
                        <uc2:UC_PLCCopyRights ID="UC_PLCCopyRights_Master2" runat="server" />
                    </div>
     </div>
     </td>
     </tr>
    </table>
       
           
        
   
    
    <asp:Button ID="btnSigPadProcess" runat="server" style="display:none;" OnClick="btnSigPadProcess_Click" />
    <asp:PlaceHolder ID="phSigPad" runat="server" ></asp:PlaceHolder>
    
    
    <asp:Button ID="btnSearchPostback" runat="server" OnClick="btnSearchPostback_Click" Style="display:none;" />
    
    <asp:Button ID="DummyButton" runat="server" Style="display: none;" />
    <cc1:modalpopupextender ID="mpeSetup" runat="server" 
        BackgroundCssClass="modalBackground" PopupControlID="pnlSetup" 
        PopupDragHandleControlID="pnlSetupCaption" TargetControlID="DummyButton" 
        DropShadow="false" CancelControlID="btnCloseSetup"
        Drag="true" OnOkScript="SetupEdit()">
    </cc1:modalpopupextender>
    <asp:Panel ID="pnlSetup" runat="server" Style="display:none;">
        <asp:Panel ID="pnlSetupPopup" runat="server" CssClass="modalPopup" DefaultButton="btnSetupEdit">
            <asp:Panel ID="pnlSetupCaption" runat="server" Height="20px" CssClass="caption" Width="600px">
                <asp:Label ID="lblSetupTitle" runat="server" Text="Laboratory Asset Setup" />
            </asp:Panel>
            <asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <center>
                    <asp:Panel ID="pnlSetupAssets" runat="server" Width="570px" BorderColor="#cfccbf" BorderStyle="Solid"
                        BorderWidth="2px">
                        <div style="margin: 5px;">
                            <PLC:PLCDBGrid ID="gvAssets" runat="server" AllowSorting="True"
                                DataKeyNames="TABLE_NAME" Width="540px" Height="300px" OnSelectedIndexChanged="gvAssets_SelectedIndexChanged"
                                OnRowDataBound="gvAssets_RowDataBound" PLCGridWidth="100%" RowStyle-HorizontalAlign="Left">
                            </PLC:PLCDBGrid>
                        </div>
                    </asp:Panel>
                    </center>
                    <br />
                    <div align="center">                      
                        <asp:Button ID="btnSetupEdit" runat="server" Text="Edit" Width="65" OnClick="btnSetupEdit_Click" Visible="false" />
                        <asp:Button ID="btnSetupRecalculate" runat="server" Text="Recalc. Stock" OnClick="btnSetupRecalculate_Click" />
                        <asp:Button ID="btnSetupCustomize" runat="server" Text="Customize Tabs" Enabled="false" Visible="false" />
                        <asp:Button ID="btnSetupTransferCols" runat="server" Text="Transfer Columns" OnClick="btnSetupTransferCols_Click" />
                        <asp:Button ID="btnSetupLocations" runat="server" Text="Locations" OnClick="btnSetupLocations_Click" />
                        <asp:Button ID="btnChemCtrlFlags" runat="server" Text="Chem Ctrl Flags" OnClick="btnChemCtrlFlags_Click" />
                        <asp:Button ID="btnSetupClose" runat="server" Text="Close" Width="65" OnClick="btnSetupClose_Click" />
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
            <br />
            <asp:Button ID="btnCloseSetup" runat="server" Style="display:none;" />
        </asp:Panel>
    </asp:Panel>
    
    <asp:Button ID="btnTransferCols" runat="server" Style="display: none;" />
    <cc1:ModalPopupExtender ID="mpeTransferCols" runat="server" BackgroundCssClass="modalBackground modalBackground2"
        PopupControlID="pnlTransferCols"
        PopupDragHandleControlID="pnlTransferColsCaption" TargetControlID="btnTransferCols"
        DropShadow="false" Drag="true" CancelControlID="btnCancelTransferCols">
    </cc1:ModalPopupExtender>
    <asp:Panel ID="pnlTransferCols" runat="server" CssClass="modalPopup modalPopup2" style="display:none;" Width="600px">
        <asp:Panel ID="pnlTransferColsCaption" runat="server" Height="20px" CssClass="caption">
            <asp:Label ID="Label4" runat="server" Text="Customize Transfer Pages Columns" />
        </asp:Panel>
        <asp:UpdatePanel ID="upTransferCols" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div style="display: flex; padding: 12px;" tabindex="-1">
                    <div style="width: 43%;">
                        <div style="padding: 5px 0px 5px 0px;">
                            <b>Available</b>
                        </div>
                        <asp:ListBox ID="lstAvailableColumns" DataTextField="DESCRIPTION" DataValueField="COLUMNID" runat="server"
                            SelectionMode="Multiple" Width="100%" Rows="15" TabIndex="1"></asp:ListBox>
                        Press CTRL key to multi-select.
                        <br />
                         <asp:Label ID="lblTransferColMsg" runat="server" ForeColor="Red"></asp:Label>
                    </div>
                    <div style="width: 7%; display: flex; flex-direction: column; justify-content: center; align-items: center;">
                        <asp:ImageButton ID="btnMoveRight" runat="server" AlternateText="Add to SR List" ImageUrl="~/Images/arrow_right.JPG"
                            OnClick="btnMoveRight_Click" border="0" width="26" onmouseover="this.style.cursor = 'pointer';" TabIndex="2" />
                        <asp:ImageButton ID="btnMoveLeft" runat="server" AlternateText="Remove from SR List" ImageUrl="~/Images/arrow_left.JPG"
                            OnClick="btnMoveLeft_Click" border="0" width="26" onmouseover="this.style.cursor = 'pointer';" TabIndex="3" />
                    </div>
                    <div style="width: 43%;">
                        <div style="padding: 5px 0px 5px 0px;">
                            <b>Selected</b>
                        </div>
                        <div>
                            <asp:ListBox ID="lstSelectedColumns" DataTextField="DESCRIPTION" DataValueField="COLUMNID" runat="server"
                                Width="100%" Rows="15" TabIndex="4"></asp:ListBox>                         
                        </div>
                    </div>
                    <div style="width: 7%; display: flex; flex-direction: column; justify-content: center; align-items: center;">
                        <asp:ImageButton ID="btnMoveUp" runat="server" AlternateText="Move Up" ImageUrl="~/Images/arrow_up.JPG"
                            OnClick="btnMoveUp_Click" border="0" width="26" onmouseover="this.style.cursor = 'pointer';" TabIndex="5" />
                        <asp:ImageButton ID="btnItemMoveDown" runat="server" AlternateText="Move Down" ImageUrl="~/Images/arrow_down.JPG"
                            OnClick="btnItemMoveDown_Click" border="0" width="26" onmouseover="this.style.cursor = 'pointer';" TabIndex="6" />
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
        <div align="center" style="padding: 10px;">
            <asp:Button ID="btnOkTransferCols" runat="server" Text="OK" style="width:80px;" OnClick="btnOkTransferCols_Click" />
            <asp:Button ID="btnCancelTransferCols" runat="server" Text="Cancel" style="width:80px;" />
        </div>
    </asp:Panel>

    <asp:Button ID="btnNewContainer" runat="server" Style="display: none;" />
    <cc1:modalpopupextender ID="mpeNewContainer" runat="server" BackgroundCssClass="modalBackground"
        PopupControlID="pnlNewContainer" 
        PopupDragHandleControlID="pnlNewContainerCaption" TargetControlID="btnNewContainer" 
        DropShadow="false" Drag="true" OnOkScript="SetupEdit()" 
        CancelControlID="btnContainerCancel" >
    </cc1:modalpopupextender>
    <asp:Panel ID="pnlNewContainer" runat="server" Style="display:none;">
        <asp:Panel ID="pnlNewContainerPopup" runat="server" CssClass="modalPopup" Width="520px" DefaultButton="btnNewContainerOK">
            <asp:Panel ID="pnlNewContainerCaption" runat="server" Height="20px" CssClass="caption">
                <asp:Label ID="lblNewContainerCaption" runat="server" Text="Create Bulk Container" />
            </asp:Panel>
            <asp:UpdatePanel ID="upNewContainer" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                <center>
                    <asp:Panel ID="pnlNCCreate" runat="server" Width="500px" BorderColor="#cfccbf" BorderStyle="Solid" BorderWidth="2px">
                        <div style="margin: 5px;">
                            <table style="width:100%">
                                <tr>
                                    <td style="text-align:right;width:100px;">
                                        Container Type
                                    </td>
                                    <td style="text-align:left" colspan="2">
                                        <uc1:CodeHead ID="chContainerType" runat="server" TableName="TV_CONTCODE" ControlWidth="150" PopupX="200" PopupY="75"></uc1:CodeHead>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="text-align:right">
                                        Location
                                    </td>
                                    <td  style="text-align:left;width:150px;">
                                        <uc1:CodeHead ID="chCustody" runat="server" TableName="TV_CUSTCODE" ControlWidth="150" PopupX="200" PopupY="75"></uc1:CodeHead>
                                    </td>
                                    <td>
                                        <uc1:CodeHead ID="chLocation" runat="server" TableName="TV_CUSTLOC" PopupX="200" PopupY="75" CodeCondition="CUSTODY_CODE={CUSTODY_CODE}"></uc1:CodeHead>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="text-align:right"></td>
                                    <td colspan="2">
                                        <asp:TextBox ID="txtNCComments" Width="200" runat="server" visible="false" />
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </asp:Panel>
                </center>
                <asp:Label ID="lblNewContainerError" runat="server" ForeColor="Red" />
                <hr />
                <div style="margin: 5px; width:100%; text-align:left;">                      
                    <asp:Button ID="btnNewContainerOK" runat="server" Text="OK" OnClick="btnNewContanerOK_Click" Width="100" />
                    <asp:Button ID="btnNewContainerCancel" runat="server" Text="Cancel" Width="100" OnClick="btnNewContainerCancel_Click" />
                </div>
                </ContentTemplate>
            </asp:UpdatePanel>
            <asp:Button ID="btnContainerCancel" runat="server" style="display:none;" />
            <br />
        </asp:Panel>
    </asp:Panel>
    
    <asp:Button ID="DummyReceipt" runat="server" Style="display: none;"     />
    <cc1:modalpopupextender ID="mpePrintReceipt" runat="server" BackgroundCssClass="modalBackground"
        PopupControlID="pnlPrintReceipt" 
        PopupDragHandleControlID="pnlPrintReceiptCaption" TargetControlID="DummyReceipt" 
        DropShadow="false" Drag="true" OnOkScript="SetupEdit()" 
        CancelControlID="btnPrintReceiptClose" >
    </cc1:modalpopupextender>
    <asp:Panel ID="pnlPrintReceipt" runat="server" Style="display:none;" >
        <asp:Panel ID="pnlPrintReceiptPopup" runat="server" CssClass="modalPopup" Width="670px" DefaultButton="btnPrintReceiptOK">
            <asp:Panel ID="pnlPrintReceiptCaption" runat="server" Height="20px" CssClass="caption" Width="670px">
                <asp:Label ID="Label1" runat="server" Text="Print Firearms Receipt" />
            </asp:Panel>
            
            <asp:UpdatePanel ID="upPrintReceipt" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <center>
                    <asp:Panel ID="pnlPrintFAReceipt" runat="server" Width="650px" BorderColor="#cfccbf" BorderStyle="Solid" BorderWidth="2px">
                        <table style="width:100%; height:100%; text-align:left;">
                            <tr>
                                <td style="width:55%">
                                    <table style="width:100%;text-align:left;">
                                        <tr>
                                            <td>
                                                Received From
                                            </td>
                                            <td>
                                                <asp:TextBox ID="txtPRReceivedFrom" runat="server" Width="200" />
                                                <%--<ajaxtoolkit:MaskedEditExtender ID="meePRReceivedFrom" runat="server" TargetControlID="txtPRReceivedFrom"
                                                     Mask="XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX"></ajaxtoolkit:MaskedEditExtender>--%>
                                                <asp:ImageButton ID="ibtnPRReceivedFrom" runat="server" ImageUrl="~/Images/question.png" OnClick="ibtnPRReceivedFrom_Click" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td style="text-align:right"></td>
                                            <td>
                                                <asp:TextBox ID="TextBox3" Width="200" runat="server" visible="false" ></asp:TextBox>
                                            </td>
                                        </tr>
                                    </table>
                                    <div style="height:120px;">
                                        <asp:HiddenField ID="hdnPRReceivedFrom" runat="server" />
                                    </div>   
                                    <div style="text-align:left;">
                                        <asp:CheckBox ID="cboxPRGetSignature" runat="server" Text="Get Signature" TextAlign="Right" />                                                
                                    </div>
                                </td>
                                <td style="width:45%">
                                    <asp:Panel ID="pnlPRReceivedDate" runat="server" GroupingText="Receive Date" >
                                        <table style="width:100%; text-align:center;">
                                            <tr>
                                                <td style="width:50%">Start Date</td>
                                                <td style="width:50%">
                                                    <asp:TextBox ID="txtPRStartDate" runat="server" Width="100" MaxLength="10"  />
                                                    <cc1:MaskedEditExtender ID="meePRStartDate" runat="server" TargetControlID="txtPRStartDate"
                                                        Mask="99/99/9999"></cc1:MaskedEditExtender>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>End Date</td>
                                                <td>
                                                    <asp:TextBox ID="txtPREndDate" runat="server" Width="100" MaxLength="10" />
                                                    <cc1:MaskedEditExtender ID="meePREndDate" runat="server" TargetControlID="txtPREndDate"
                                                        Mask="99/99/9999"></cc1:MaskedEditExtender>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td>
                                                    <asp:Panel ID="pnlPRCurrent" runat="server" GroupingText="Current" >
                                                        <%--<asp:Button ID="btnPRToday" runat="server" Text="Today" Width="80" />
                                                        <asp:Button ID="btnPRThisMonth" runat="server" Text="Month" Width="80" />
                                                        <asp:Button ID="btnPRThisYear" runat="server" Text="Year" Width="80" />--%>
                                                        <input type="button" id="btnPRToday" runat="server" value="Today" style="width:80px;" />
                                                        <input type="button" id="btnPRThisMonth" runat="server" value="Month" style="width:80px;" />
                                                        <input type="button" id="btnPRThisYear" runat="server" value="Year" style="width:80px;" /></asp:Panel>
                                                </td>
                                                <td>
                                                    <asp:Panel ID="pnlPRLast" runat="server" GroupingText="Last" >
                                                        <%--<asp:Button ID="btnPRYesterday" runat="server" Text="Yesterday" Width="80" />
                                                        <asp:Button ID="btnPRLastMonth" runat="server" Text="Month" Width="80" />
                                                        <asp:Button ID="btnPRLastYear" runat="server" Text="Year" Width="80" />--%>
                                                        <input type="button" id="btnPRYesterday" runat="server" value="Yesterday" style="width:80px;" />
                                                        <input type="button" id="btnPRLastMonth" runat="server" value="Month" style="width:80px;" />
                                                        <input type="button" id="btnPRLastYear" runat="server" value="Year" style="width:80px;" /></asp:Panel>
                                                </td>
                                            </tr>
                                        </table>
                                    </asp:Panel>
                                </td>
                            </tr>
                        </table>
                    </asp:Panel>
                    <asp:Panel ID="pnlFADept" runat="server" Width="650px" BorderColor="#cfccbf" BorderStyle="Solid" BorderWidth="2px" Visible="false">
                        <div style="margin: 5px;">
                            <table style="width:100%">
                                <tr>
                                    <td colspan="2">
                                        <PLC:PLCDBGrid ID="gvFADeptCode" runat="server" AllowSorting="True" Height="250px" Width="700px" EmptyDataText="No Records Found"
                                            OnSelectedIndexChanged="gvFADeptCode_SelectedIndexChanged" PLCGridWidth="630px" RowStyle-HorizontalAlign="Left">
                                        </PLC:PLCDBGrid>
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </asp:Panel>
                </center>
                <asp:Label ID="lblPrintReceiptError" runat="server" ForeColor="Red" />
                <hr />
                <table style="width:100%; padding:5px;">
                    <tr>
                        <td style="text-align:left;">
                            <asp:Panel ID="pnlFADeptSearch" runat="server" Visible="false" DefaultButton="btnFADeptSearch" >
                            Code Starts With
                            <asp:TextBox ID="txtFADeptSearch" runat="server" ></asp:TextBox>
                            <asp:Button ID="btnFADeptSearch" runat="server" Text="Search" OnClick="btnFADeptSearch_Click" />
                            </asp:Panel>
                        </td>
                        <td>
                            <div style="margin-right: 5px; width:100%; text-align:right;">                      
                                <asp:Button ID="btnPrintReceiptOK" runat="server" Text="OK" OnClick="btnPrintReceiptOK_Click" Width="100" />
                                <asp:Button ID="btnPrintReceiptCancel" runat="server" Text="Cancel" Width="100" OnClick="btnPrintReceiptCancel_Click" />
                            </div>                        
                        </td>
                    </tr>
                </table>
                </ContentTemplate>
            </asp:UpdatePanel>
            <asp:Button ID="btnPrintReceiptClose" runat="server" style="display:none;" />
            <br />
        </asp:Panel>
    </asp:Panel>
    
    <asp:Button ID="btnDeleteReason" runat="server" Style="display: none;" />
    <cc1:modalpopupextender ID="mpeDeleteReason" runat="server" 
        BackgroundCssClass="modalBackground" PopupControlID="pnlDeleteReason" 
        PopupDragHandleControlID="pnlDeleteCaption" 
        TargetControlID="btnDeleteReason" DropShadow="false" CancelControlID="btnDeleteClose"
        Drag="true" OnOkScript="SetupEdit()">
    </cc1:modalpopupextender>
    <asp:Panel ID="pnlDeleteReason" runat="server" Style="display:none;">
        <asp:Panel ID="pnlDeleteReasonPopup" runat="server" CssClass="modalPopup" Width="400px" DefaultButton="btnDeleteOk">
            <asp:Panel ID="pnlDeleteCaption" runat="server" Height="20px" CssClass="caption">
                <asp:Label ID="Label2" runat="server" Text="Delete" />
            </asp:Panel>
            <asp:UpdatePanel ID="UpdatePanel4" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                <table>
                    <tr>
                        <td style="width:5%">
                        &nbsp;
                        </td>
                        <td style="width:90%">
                            <div align="left">
                                Please enter the reason for deleting
                                <asp:TextBox ID="txtDeleteReason" runat="server" TextMode="MultiLine" Height="60px" Width="360px" />
                                <asp:Label ID="lblDeleteReason" runat="server" ForeColor="Red" />
                            </div>            
                            <hr />
                            <div align="right">                      
                                <asp:Button ID="btnDeleteOk" runat="server" Text="OK" Width="65" OnClick="btnDeleteOk_Click" />
                                <asp:Button ID="btnDeleteCancel" runat="server" Text="Cancel" Width="65" OnClick="btnDeleteCancel_Click" />
                            </div>                        
                        </td>
                        <td style="width:5%">
                        &nbsp;
                        </td>
                    </tr>
                </table>
                </ContentTemplate>
            </asp:UpdatePanel>
            <asp:Button ID="btnDeleteClose" runat="server" style="display:none;" />
        </asp:Panel>
    </asp:Panel>

    <asp:Button ID="SaveKit" runat="server" style="display:none" />
    <cc1:ModalPopupExtender ID="mpeChemInvKit" runat="server"
     BackgroundCssClass="modalBackground"
     PopupControlID="pnlMainChemInvKit"
     PopupDragHandleControlID="pnlChemInvKit"
     Drag="true"
     TargetControlID="SaveKit">
    </cc1:ModalPopupExtender>
    <asp:Panel ID="pnlMainChemInvKit" runat="server" CssClass="modalPopup" style="display:none; max-width:900px">
        <asp:Panel ID="pnlChemInvKit" runat="server" CssClass="caption">
            Kit Components Inventory
        </asp:Panel>
        <table>
            <tr>
                <td colspan="2">
                    <asp:Panel ID="pnlComponentKit" runat="server" ScrollBars="Auto" style="max-width:880px" Height="500px">
                        <asp:GridView ID="gvComponentKit" runat="server" AutoGenerateColumns="false">
                            <Columns>
                                <asp:BoundField DataField="CODE" HeaderText="Code" />
                                <asp:BoundField DataField="DESCRIPTION" HeaderText="Description" />
                                <asp:TemplateField HeaderText="Lot Number">
                                    <ItemTemplate>
                                        <asp:TextBox ID="tbxLotNumber" runat="server" Text='<%# Eval("LOT_NUMBER") %>' 
                                            Width="80px" MaxLength="25">
                                        </asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Expiration Date">
                                    <ItemTemplate>
                                        <asp:TextBox ID="tbxExpirationDate" runat="server" Width="100px" Text='<%# Eval("EXPIRATION_DATE") %>'></asp:TextBox>
                                        <cc1:MaskedEditExtender ID="MaskedEditExtender1" runat="server" TargetControlID="tbxExpirationDate" 
                                            Mask="99/99/9999" MaskType="Date" UserDateFormat="MonthDayYear" InputDirection="LeftToRight">
                                        </cc1:MaskedEditExtender>
                                        <asp:HiddenField ID="hdnExpDateRequired" runat="server" Value='<%# Eval("EXPIRATION_DATE_REQUIRED") %>' />
                                        <asp:HiddenField ID="hdnComments" runat="server" Value='<%# Eval("COMMENTS") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="QC Date">
                                    <ItemTemplate>
                                        <asp:TextBox ID="txtQCDate" runat="server" Width="100px" Text='<%# Eval("QC_DATE") %>'></asp:TextBox>
                                        <cc1:MaskedEditExtender ID="meeQCDate" runat="server" TargetControlID="txtQCDate" 
                                            Mask="99/99/9999" MaskType="Date" UserDateFormat="MonthDayYear" InputDirection="LeftToRight">
                                        </cc1:MaskedEditExtender>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Quantity">
                                    <ItemTemplate>
                                        <asp:TextBox ID="tbxQuantity" runat="server" Text='<%# Eval("DEFAULT_QUANTITY") %>' Width="80px"></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Storage Conditions">
                                    <ItemTemplate>
                                        <asp:Label ID="lblStorageConditionDescription" runat="server" Text='<%# Eval("STORAGE_CONDITIONS_DESCRIPTION") %>'></asp:Label>
                                        <asp:HiddenField ID="hdnStorageCondition" runat="server" Value='<%# Eval("STORAGE_CONDITIONS") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Units">
                                    <ItemTemplate>
                                        <asp:Label ID="lblUnitDescription" runat="server" Text='<%# Eval("UNIT_DESCRIPTION") %>'></asp:Label>
                                        <asp:HiddenField ID="hdnUnits" runat="server" Value='<%# Eval("UNITS") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Vendor Name">
                                    <ItemTemplate>
                                        <asp:Label ID="lblVendorName" runat="server" Text='<%# Eval("VENDOR_NAME_DESCRIPTION") %>'></asp:Label>
                                        <asp:HiddenField ID="hdnVendorName" runat="server" Value='<%# Eval("VENDOR_NAME") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </asp:Panel>
                    <br />
                </td>
            </tr>
            <tr>
                <td align="left">
                    <asp:CheckBox ID="chkPrintLabels" runat="server" Text="Print Labels"/>
                </td>
                <td align="right">
                    <%--<asp:Button ID="btnSaveKit" runat="server" Text="Save" Width="80px" Enabled="false" 
                        OnClick="SaveKit_Click" />--%>
                    <asp:Button ID="btnSaveComp" runat="server" Text="Save" Width="80px" 
                        OnClick="SaveKit_Click" OnClientClick="return isKitComponentsGridValid();" />
                    <asp:Button ID="btnCancelKit" runat="server" Text="Cancel" Width="80px" />
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <br />
                </td>
            </tr>
        </table>
    </asp:Panel>
    <asp:Button ID="btnCancelKitConfirm" runat="server" OnClick="ConfirmRemoveKit" style="display:none;" />
    <asp:Button ID="btnCancelCreateKit" runat="server" OnClick="btnCancelCreateKit_Click" style="display:none;"/>

    <PLC:MessageBox ID="mbDeleteCustody" runat="server" MessageType="Confirmation" PanelCSSClass="modalPopup"
        Width="400" CaptionCSSClass="caption" PanelBackgroundCSSClass="modalBackground" Message="Are you sure you want to delete this custody?"
        Caption="Confirm" OnOkClick="DeleteCustody_Click"/>
    <PLC:MessageBox ID="mbConfirmation" runat="server" MessageType="Confirmation" PanelCSSClass="modalPopup"
        Width="500" CaptionCSSClass="caption" PanelBackgroundCSSClass="modalBackground" Caption="Confirm" />
    <cc1:ModalPopupExtender ID="mpeChemCtrlAccess" 
        runat="server" TargetControlID="ChemCtrlAccess" PopupControlID="pnlChemCtrlAccess"                        
        DropShadow="false"  BackgroundCssClass="modalBackground" 
        PopupDragHandleControlID="pnlLogin" Drag="true">
    </cc1:ModalPopupExtender>
    <asp:Button ID="ChemCtrlAccess" runat="server" style="display: none" />
    <asp:Panel ID="pnlChemCtrlAccess" runat="server" DefaultButton="btnOkChemCtrlAccess" CssClass="modalPopup" Style="display: none;" Width="300px">                  
        <asp:Panel ID="pnlLogin" runat="server" CssClass="caption">Web Chem Ctrl Access</asp:Panel>					
           &nbsp;&nbsp;&nbsp;<asp:Label ID="lblPassword" runat="server" AssociatedControlID="txtPassword" Text="Enter Password: "></asp:Label>           
            <asp:TextBox ID="txtPassword" runat="server" TextMode="Password"></asp:TextBox><br />
            <div align="center">
                <asp:Label ID="lblMsg" runat="server" Text="Invalid password. Please try again." ForeColor="Red" Visible="false"></asp:Label><br />
                <asp:Button ID="btnOkChemCtrlAccess" runat="server" Text="OK" OnClick="btnChemCtrlCheck_Click" Width="80px"/>
                <asp:Button ID="btnCancelChemCtrlAccess" runat="server" Text="Cancel" OnClick="btnCancelChemCtrlCheck_Click" Width="80px"/><br /><br />
            </div>
        </asp:Panel>

    <cc1:ModalPopupExtender ID="mpePopupDuplicateTimes" 
        runat="server" TargetControlID="btnShowPopupDuplicateTimes" PopupControlID="pnlPopupDuplicateTimes"
        DropShadow="false"  BackgroundCssClass="modalBackground" 
        PopupDragHandleControlID="pnlPopupDuplicateTimesCaption" Drag="true">
    </cc1:ModalPopupExtender>
    <asp:Button ID="btnShowPopupDuplicateTimes" runat="server" style="display: none" />
    <asp:Panel ID="pnlPopupDuplicateTimes" runat="server" DefaultButton="btnPopupDuplicateTimesOK" CssClass="modalPopup" Style="display: none;" Width="300px">                  
        <asp:Panel ID="pnlPopupDuplicateTimesCaption" runat="server" CssClass="caption">Duplicate Number of Times</asp:Panel>					
        <div style="padding: 1em 2em;">
            <asp:Label ID="Label3" runat="server" AssociatedControlID="txtPopupDuplicateTimes" Text="Enter Number of Times to Duplicate: "></asp:Label>           
            <asp:TextBox ID="txtPopupDuplicateTimes" runat="server"></asp:TextBox><br />
        </div>
        <div align="center">
            <asp:Button ID="btnPopupDuplicateTimesOK" runat="server" Text="OK" OnClick="btnPopupDuplicateTimesOK_Click" Width="80px"/>
            <asp:Button ID="btnPopupDuplicateTimesCancel" runat="server" Text="Cancel" OnClick="btnPopupDuplicateTimesCancel_Click" Width="80px"/><br /><br />
        </div>
    </asp:Panel>

    <%-- LAM New Asset Popup --%>
        <div id="new-asset-popup" style="display:none;">
            <asp:UpdatePanel ID="UpdatePanel5" runat="server">
                <ContentTemplate>
                    # of asset record: 
                    <div id="number-spinner" class="number-spinner">
                        <button class="decrement" type="button">-</button>
                        <asp:TextBox ID="txtNumNewAsset" runat="server" Text="1">
                        </asp:TextBox>
                        <button class="increment" type="button">+</button>
                    </div>

                    <div>
                        <PLC:DBPanel ID="dbpNewAsset" runat="server" CreateControlsOnExplicitCall="true" 
                            PLCAllowBlankRecordSave="true" PLCAuditCode="3000" PLCAuditSubCode="22" 
                            PLCDeleteAuditCode="3000" PLCDeleteAuditSubCode="3" PLCAttachPopupTo=".popup-ffb"
                            OnPLCDBPanelSetDefaultRecord="dbpNewAsset_SetDefaultRecord" 
                            OnPLCDBPanelGetNewRecord="dbpNewAsset_GetNewRecord">
                        </PLC:DBPanel>
                        <br />
                    </div>

                    <asp:HiddenField ID="hdnNewAssetCCN" runat="server" Value="" />
                    <asp:HiddenField ID="hdnNewAssetBarcode" runat="server" Value="" />
                    <asp:HiddenField ID="hdnNewAssetDefaultAssetClass" runat="server" Value="" />
                    <asp:HiddenField ID="hdnNewAssetDefaultAssignedSection" runat="server" Value="" />
                    <asp:HiddenField ID="hdnNewAssetDefaultCurrentStatus" runat="server" Value="" />
                    <asp:HiddenField ID="hdnNewAssetDefaultCurrentLocation" runat="server" Value="" />
                    <asp:HiddenField ID="hdnNewAssetDefaultLabCode" runat="server" Value="" />
                </ContentTemplate>
            </asp:UpdatePanel>

            <div style="display:none;">
                <div id="new-asset-printbc" style="display: inline;">
                    Print Barcodes: 
                    <asp:CheckBox ID="cbNewAssetPrintBarcodes" runat="server" Checked="true" />
                </div>

                <div id='new-asset-error-message' style='display:inline; margin-left: 5em;'>
                    <asp:Label ID="lblNewAssetError" runat="server" Text="Unable to save this information. Look for errors in red" ForeColor="Red" Visible="false">
                    </asp:Label>
                </div>
            </div>

            <div style="display:none;">
                <%-- we need a full postback on save so this is outside the dbpanel --%>
                <asp:Button ID="btnSaveNewAsset" runat="server" OnClick="btnSaveNewAsset_Click" />
                <%-- for printing bc when we can't do postback --%>
                <asp:HiddenField ID="hdnNewAssetPrintBCCCN" runat="server" Value="" />
                <asp:Button ID="btnPrintNewAssetBC" runat="server" OnClick="btnPrintNewAssetBC_Click" />
            </div>
        </div>
    
    <%-- End LAM New Asset Popup --%>

    <div class="hide">
        <div class="uom-config">
            <asp:UpdatePanel runat="server">
                <ContentTemplate>
                    <div style="height: 290px;">
                        <PLC:PLCDBGrid ID="dbgUOMConfig" runat="server" AllowSorting="True" AllowPaging="True" PageSize="10"
                            PLCGridName="CHEMINV_UOMCONFIG" DataKeyNames="DATA_KEY,CHEM_CONTROL_NUMBER" PLCGridWidth="660px" Height="270px"
                            EmptyDataText="No records found." OnSelectedIndexChanged="dbgUOMConfig_SelectedIndexChanged"
                            OnSorted="dbgUOMConfig_Sorted" OnPageIndexChanged="dbgUOMConfig_PageIndexChanged">
                        </PLC:PLCDBGrid>
                    </div>
                    <PLC:DBPanel ID="dbpUOMConfig" runat="server" PLCDataTable="TV_UOMCONFIG" PLCPanelName="CHEMINV_UOMCONFIG" Height="200px"
                        PLCWhereClause="Where 0 = 1" PLCAuditCode="0" PLCAuditSubCode="0" PLCDeleteAuditCode="0" PLCDeleteAuditSubCode="0"
                        PLCDisableDefaultValue="false" PLCAttachPopupTo=".popup-ffb" OnPLCDBPanelGetNewRecord="dbpUOMConfig_PLCDBPanelGetNewRecord">
                    </PLC:DBPanel>
                    <PLC:PLCButtonPanel ID="bpUOMConfig" runat="server" PLCTargetControlID="dbpUOMConfig" Width="100%"
                        PLCShowAddButton="True" PLCShowEditButtons="True" PLCShowDeleteButton="true" PLCCustomButtons="Close"
                        OnPLCButtonClick="bpUOMConfig_PLCButtonClick">
                    </PLC:PLCButtonPanel>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>

    <asp:Button ID="btnMultiPrintLabels" runat="server" OnClick="btnMultiPrintLabels_Click" />
    <div class="hide">
        <div class="multi-print-label">
            Label Format:
            <asp:DropDownList ID="ddlMultiPrintFormats" runat="server" Width="250px" /><br /><br />
            
            Number of Labels: 
            <div class="number-spinner">
                <button class="decrement" type="button">-</button>
                <asp:TextBox ID="txtMultiPrintCount" runat="server" Text="1" TextMode="Number" />
                <button class="increment" type="button">+</button>
            </div>
        </div>
    </div>

    <asp:HiddenField ID="grdAssetRecsSelectedIndex" runat="server" Value="0" />
    <asp:HiddenField ID="hdnIncorrectCalcConfirmed" runat="server" Value="F" />
    <script type="text/javascript">
        $(document).ready(function () {
            SetBrowserWindowID();
        });

        $(function () {
            SetF7HotKey();

            var el;
            if (document.getElementById("divAssetRecs") != null)
                el = document.getElementById("divAssetRecs").children[0].children[0];
            else
                el = $("[id$='gvMainAsset']")[0];
            scrollGridToIndex(el, $("[id$='grdAssetRecsSelectedIndex']").val());

            $(".plcdbpanel table tr td div span>div[id*='multipick']").each(function () {
                $(this).closest('td').prev().addClass('plcdbpanel-multipick-prompt');
            });

            var prm = Sys.WebForms.PageRequestManager.getInstance();
            prm.add_pageLoaded(pageLoaded);
        });

        function scrollGridToIndex(grid, index) {
            if (grid && index < grid.rows.length) {
                var row = grid.rows[index];
                if ($(row).offset().top > $(window).height()) {
                    row.scrollIntoView();
                    if (document.activeElement && document.activeElement.scrollIntoView) document.activeElement.scrollIntoView();
                    document.body.scrollLeft = 0;
                }
            }
        }

        function scrollGridToIndexById(grid, index) {
            $grid = document.getElementById(grid);
            //if (index < $grid.rows.length) $grid.rows[index].scrollIntoView();
            scrollGridToIndex($grif, index);
        }

        // Disables F5(refresh) on page to prevent user from resubmitting
        $(document).keydown(function (event) {
            if (window.event && window.event.keyCode == 116) {
                disableUnloadCheck();
                window.event.keyCode = 0;
                window.location.href = location.href;
                return false;
            }
        });
        
        // Do not submit when pressing enter key on flexboxes
        $(document).keypress(function(event) {
            if (event.keyCode === 13 && document.activeElement != null && document.activeElement.id != null && document.activeElement.className.indexOf('ffb-input') > -1) {
                return false;
            }
        });

        function pageLoaded() {
            // fix for PLCDBGrid header
            $(".plcdbgridScroll[id$='_header']").css("height", "auto");

            // fix for gvMainAssets header width when no DBGRIDDL defined
            $("th.AppendDiv").each(function (i, e) {
                $(e).html("<div style='width:" + $(e).css("width") + "'>" + $(e).text() + "</div>");
            });

            // fix for number-spinner not working after full/partial postback
            initNumberSpinner("<%= txtNumNewAsset.ClientID %>");

            // fix for ajax calendar not showing in correct position
            $("#<%= dbpNewAsset.ClientID %>").find('input[name*="DBP_DATE"]').parent().css({ 'position': 'relative' });

            setNewAssetTabIndexes();

            $("#<%= btnOkTransferCols.ClientID  %>").attr("tabindex", "7");
            $("#<%= btnCancelTransferCols.ClientID  %>").attr("tabindex", "8");
            $("#<%= btnCancelTransferCols.ClientID  %>").on("keydown", function (e) {
                if (e.keyCode == 9 && !e.shiftKey) {
                    $("#<%= lstAvailableColumns.ClientID  %>").focus();
                    return false;
                }
            });
            $("#<%= lstAvailableColumns.ClientID %>").on("keydown", function (e) {
                if (e.keyCode == 9 && e.shiftKey) {
                    $("#<%= btnCancelTransferCols.ClientID  %>").focus();
                    return false;
                }
            });

	    setTimeout(function () {
		    var $deleteCustodyOk = $("[id$='<%= mbDeleteCustody.ClientID %>ob']");
            var $deleteCustodyCancel = $("[id$='<%= mbDeleteCustody.ClientID %>cb']");

                $deleteCustodyOk.click(function () {
                    // workaround to disable the buttons since 
                    // attr("disabled") also prevents the postback
                    $deleteCustodyOk.css({
                        "pointer-events": "none",
                        "opacity": "0.6"
                    });
                    $deleteCustodyCancel.css({
                        "pointer-events": "none",
                        "opacity": "0.6"
                    });
                });
            }, 200);
        }

        var isDoUnload = true;
        function disableUnloadCheck() {
            isDoUnload = false;
        }

        function bindBeforeUnload(supplement) {
            var cleanDBPanel = function () {
                var doUnload = isDoUnload;
                isDoUnload = true;

                var prm = Sys.WebForms.PageRequestManager.getInstance();
                if (doUnload && !prm._processingRequest) {
                    if (isChrome()) {
                        $.ajax({
                            type: "POST",
                            url: GetWebRootUrl() + "PLCWebCommon/PLCWebMethods.asmx/CleanDBPanel",
                            data: { "supplement": !!supplement },
                            success: function () { },
                            error: function () { },
                            dataType: "text"
                        });
                    } else {
                        return "Please cancel the current action before leaving.";
                    }
                }
            };
            window.onbeforeunload = cleanDBPanel;

            var $saveButton = $("[id$='PLCButtonPanel1_DBP_SAVEBUTTON']")[0];
            var saveClickEvent = $saveButton.onclick;
            $saveButton.onclick = function (event) { disableUnloadCheck(); saveClickEvent(event); };
            var $cancelButton = $("[id$='PLCButtonPanel1_DBP_CANCELBUTTON']")[0];
            var cancelClickEvent = $cancelButton.onclick;
            $cancelButton.onclick = function (event) { disableUnloadCheck(); cancelClickEvent(event); };
        }

        //show a jquery dialog message
        function showDialog(title, message) {
            var $div = $("<div></div>");
            var $message = $("<div>" + message + "</div>");

            $div.append($message);

            var dlg = $div.dialog({
                modal: true,
                title: title ? title : document.title,
                autoOpen: true,
                width: 'auto',
                height: 'auto',
                zIndex: 10003,
                closeOnEscape: false,
                resizable: false,
                buttons: {
                    Ok: function () {
                        $(this).dialog("close");
                    }
                },
                close: function (event, ui) {
                    $(this).remove();
                },
                open: function (event, ui) {
                    $(".ui-dialog-titlebar-close", ui.dialog | ui).hide();
                }
            });
//            dlg.parent().appendTo(jQuery("form:first"));
        }

        function showCancelKitConfirm() {
            showConfirm(
                "Cancelling will not create kit components. Are you sure you want to proceed?",
                "Kit Components Inventory",
                function () {
                    var btn = $("#<%= btnCancelKitConfirm.ClientID%>");
                    setTimeout(function () {
                        btn.click();
                    }, 0);
                }
            );
        }

        function __cancelCreateKit() {
            var $btn = $("#<%= btnCancelCreateKit.ClientID %>");
            setTimeout(function () {
                $btn.click();
            }, 0);
        }

        //msg: displayed in the dialog body
        //title: dialog title
        //okFn: callback function when ok is clicked
        //cancelFn: callback function when cancel is clicked
        function showConfirm(msg, title, okFn, cancelFn) {
            var m = msg ? msg : "Are you sure you want to continue?";
            var left = {
                float: 'left',
                width: '10%',
                height: '100%'
            };

            var right = {
                float: 'left',
                width: '90%',
                'text-align': 'justify',
                height: '100%'
            };

            var $icon = $("<div><span class='ui-icon ui-icon-info' style='float:left; margin:0 7px 20px 0;'></span></div>");
            $icon.css(left);

            var $message = $("<div>" + m + "</div>");
            $message.css(right);

            $div = $("<div></div>");

            $div.append($icon).append($message);

            var t = title ? title : document.title;
            $div.dialog({
                modal: true,
                title: t,
                autoOpen: true,
                width: '350px',
                resizable: false,
                height: 'auto',
                zIndex: 10003,
                buttons: {
                    Yes: function () {
                        if (okFn && typeof okFn === "function") {
                            okFn();
                        }
                        $(this).dialog("close");
                    },
                    No: function () {
                        if (cancelFn && typeof cancelFn === "function") {
                            cancelFn();
                        }
                        $(this).dialog("close");
                    }
                },
                close: function (event, ui) {
                    $(this).remove();
                },
                open: function (event, ui) {
                    $(".ui-dialog-titlebar-close", ui.dialog | ui).hide();
                }
            });
        }

        // get kit content grid data in json array format
        // modify this function as needed
        function getKitComponentGridData(gridClientId) {
            var columnIndices = {
                0: 'code',
                1: 'description',
                2: 'lotNumber',
                3: 'expirationDate',
                4: 'quantity',
                5: 'units'
            };

            var getTdInnerHtml = function (td) {
                return td.innerHTML
            }
            var getTdInputValue = function (td) {
                var input = $(td).find('input')
                if (input.length > 0) {
                    return input[0].value;
                }
            }

            var getExpirationDateRequired = function (td) {
                var input = $(td).find('input[id$=hdnExpDateRequired]');
                if (input.length > 0) {
                    return input[0].value === 'T';
                }
            }
            var columnCallbacks = {
                code: getTdInnerHtml,
                description: getTdInnerHtml,
                lotNumber: getTdInputValue,
                expirationDate: getTdInputValue,
                quantity: getTdInputValue,
                units: getTdInputValue
            };

            var rowVals = [];

            $("#" + gridClientId)
            .find("tr")
            .filter(function (idx, tr) {
                // exclude header row
                return idx !== 0;
            })
            .each(function (idx, tr) {
                var columnVals = {}
                $(tr).find('td').each(function (idx, td) {
                    var column = columnIndices[idx];
                    var val = columnCallbacks[column](td);
                    columnVals[column] = val

                    if (column === 'expirationDate') {
                        columnVals['expirationDateRequired'] = getExpirationDateRequired(td);
                    }
                });
                rowVals.push(columnVals);
            });

            return rowVals
        }

        function getExpirationDatesRequired(kitCompArray) {
            var codes = kitCompArray.filter(function (kit) {
                return kit.expirationDateRequired && kit.expirationDate === ''
            })
            .map(function (kit) {
                return kit.code
            });

            // return only unique codes
            unique = codes.filter(function (item, i, ar) {
                return ar.indexOf(item) === i;
            });
            return unique;
        }

        function isKitComponentsGridValid() {
            var gridIsValid = true;
            var dialogTitle = 'Kit Components Inventory';
            var gridKitId = "<%= gvComponentKit.ClientID %>";
            var kitGridJson = getKitComponentGridData(gridKitId);
            var expirationDateRequired = getExpirationDatesRequired(kitGridJson);

            var validationList = {
                'expirationDateRequired': expirationDateRequired
            };

            if (expirationDateRequired.length > 0) {
                gridIsValid = false;
            }

            // not valid? show a message
            if (!gridIsValid) {
                message = '<div>Expiration date is required for the following kit item/s: <br/>' +
                    expirationDateRequired.join(', ') + '</div>';
                showDialog(dialogTitle, message);
            }

            return gridIsValid;
        }

        function showNewAssetPopup(title) {
            var $assetPopup = $("#new-asset-popup");
            var $parent = $assetPopup.parent();
            var height = 600;

            $assetPopup.dialog({
                title: "New " + (title || "Asset"),
                resizable: false,
                draggable: true,
                closeOnEscape: false,
                width: 1000,
                height: height,
                modal: true,
                autoOpen: false,
                position: { my: "center", at: "center" },
                close: function (event, ui) {
                    // Append to original parent to avoid duplicate ids
                    $(this).parent().appendTo($parent);
                },
                open: function (event, ui) {
                    $(".ui-dialog-titlebar-close", $(this).parent()).hide();
                    $(".ui-widget-overlay").css("position", "fixed");

                    // Fix position of flexbox dropdown
                    // Delay execution to make sure popup-ffb already exist before adjustment
                    setTimeout(function () {
                        $(".popup-ffb").css("position", "absolute").css("top", "0px").css("left", "0px");
                    });

                    $(this).parent().appendTo("form");

                    initNumberSpinner("<%= txtNumNewAsset.ClientID %>");

                    $(this).parent().find('.ui-dialog-buttonpane').prepend($('#new-asset-error-message'));
                    $(this).parent().find('.ui-dialog-buttonpane').prepend($('#new-asset-printbc'));

                    setNewAssetTabIndexes();
                },
                buttons: {
                    Save: function () {
                        setTimeout(function () {
                            $("#<%= btnSaveNewAsset.ClientID %>").click();
                        }, 0);
                    },
                    Cancel: function () {
                        $(this).dialog("close");
                    }
                },
                dragStart: function (e, ui) {
                    hidePopups();

                    $(this).parent().height(height);
                },
                dragStop: function (e, ui) {
                    $(this).parent().height(height);
                }
            }).keypress(function (e) {
                //if (e.keyCode == '13') {
                //    alert('Did you just pressed enter?');
                //}
            }).scroll(function (e) {
                hidePopups();
            });

            setTimeout(function () {
                $assetPopup.dialog("open");
            });

            $("#new-asset-popup").scroll(function (e) {
                hidePopups();
            });

            function hidePopups() {
                // hide flexbox list items on scroll
                var $popupFFB = $(".popup-ffb > .ffb:visible");
                if ($popupFFB.length)
                    $popupFFB.hide();
            }
        }

        // reinitialize after every partial/full postback
        function initNumberSpinner(id, opts) {
            var maxVal = 100;
            var minVal = 1
            var incStep = 1;
            var decStep = 1;

            var $container = $(".number-spinner");
            if (!$container.length || $container.data('initialized')) return;

            // flag to indicate that events have already been initialized
            $container.data('initialized', true);

            var $input = $("#" + id);
            $input.bind("keyup", function (event) {
                var target = event.target;
                var value = target.value.replace(/[^\d]/, '');
                var num = parseInt(value, 10);
                num = isNaN(num) || num === 0 ? 1 : num;

                if (num > maxVal)
                    num = maxVal;

                target.value = num;
            });

            var $btnDec = $(".number-spinner .decrement");
            $btnDec.bind('click', function () {
                var val = parseInt($input.val(), 10);

                if (val <= minVal) return;

                $input.val(val - decStep);
            });

            var $btnInc = $(".number-spinner .increment");
            $btnInc.bind('click', function () {
                var val = parseInt($input.val(), 10);

                if (val >= maxVal) return;

                $input.val(val + incStep);
            });
        }

        function __printNewAssetBC() {
            setTimeout(function () {
                $("#<%= btnPrintNewAssetBC.ClientID %>").click();
            }, 0);
        }

        function closeUOMConfig() {
            var $parent = $(".uom-config").dialog("close");
        }

        function showUOMConfig(title) {
            var $parent = $(".uom-config").parent();
            var height = 600;
            $(".uom-config").dialog({
                title: title || "UOM Info",
                resizable: false,
                draggable: true,
                closeOnEscape: false,
                width: 700,
                height: height,
                modal: true,
                autoOpen: true,
                close: function () {
                    // Append to original parent to avoid duplicate ids
                    $(this).parent().appendTo($parent);
                },
                open: function (event, ui) {
                    $(".ui-dialog-titlebar-close", $(this).parent()).hide();
                    $(".ui-widget-overlay").css("position", "fixed");

                    // Fix position of flexbox dropdown
                    $(".popup-ffb").css("position", "absolute").css("top", "0px").css("left", "0px");

                    $(this).parent().appendTo("form");
                },
                buttons: {
                    // Cancel: function () { $(this).dialog("close"); }
                },
                dragStart: function () {
                    $(".popup-ffb .ffb:visible").hide();
                    $(this).parent().height(height);
                },
                dragStop: function (e, ui) {
                    $(this).parent().height(height);
                }
            }).keypress(function (e) {
                if (e.keyCode == '13') { }
            });
        }

        function setNewAssetTabIndexes() {
            var lastTabIndex = 0;
            $('.pagechemInv [tabindex]').attr('tabindex', function (i, value) {
                lastTabIndex = Math.max(lastTabIndex, value);
            });

            // number spinner
            $(".number-spinner .decrement").attr("tabindex", ++lastTabIndex);
            $("#<%= txtNumNewAsset.ClientID %>").attr("tabindex", ++lastTabIndex);
            $(".number-spinner .increment").attr("tabindex", ++lastTabIndex);

            lastTabIndex++;

            // dbpanel fields
            var dbpLastTabIndex = 0;
            $("#<%= dbpNewAsset.ClientID %> :input:visible").each(function () {
                var currTabIndex = $(this).attr("tabindex");
                if (currTabIndex != undefined)
                    $(this).attr("tabindex", parseInt(currTabIndex) + lastTabIndex);
                else
                    $(this).attr("tabindex", ++lastTabIndex);
                
                dbpLastTabIndex = Math.max($(this).attr("tabindex"), dbpLastTabIndex);
            });
            lastTabIndex = dbpLastTabIndex;

            // print bc
            $("#<%= cbNewAssetPrintBarcodes.ClientID %>").attr("tabindex", ++lastTabIndex);

            // popup buttons
            $("#new-asset-popup").parent().find(".ui-dialog-buttonset button").each(function () {
                $(this).attr("tabindex", ++lastTabIndex);
            });
        }

        function showMultiPrintLabel() {
            var $parent = $(".multi-print-label").parent();
            var height = 200;
            $(".multi-print-label").dialog({
                title: "Multi Print Label",
                resizable: false,
                draggable: true,
                closeOnEscape: false,
                width: 400,
                height: height,
                modal: true,
                autoOpen: true,
                close: function () {
                    // Append to original parent to avoid duplicate ids
                    $(this).parent().appendTo($parent);
                },
                open: function (event, ui) {
                    $(".ui-dialog-titlebar-close", $(this).parent()).hide();
                    $(".ui-widget-overlay").css("position", "fixed");

                    // Fix position of flexbox dropdown
                    $(".popup-ffb").css("position", "absolute").css("top", "0px").css("left", "0px");

                    initNumberSpinner("<%= txtMultiPrintCount.ClientID %>");

                    $(this).parent().appendTo("form");
                },
                buttons: {
                    OK: function () {
                        $("[id$='btnMultiPrintLabels']").click();
                    },
                    Cancel: function () { $(this).dialog("close"); }
                },
                dragStart: function () {
                    $(".popup-ffb .ffb:visible").hide();
                    $(this).parent().height(height);
                },
                dragStop: function (e, ui) {
                    $(this).parent().height(height);
                }
            }).keypress(function (e) {
                if (e.keyCode == '13') { }
            });
        }

        function confirmIssuedIncorrectCalc(message) {
            confirmYesNo(message, function () {
                $("[id$='hdnIncorrectCalcConfirmed']").val("T");
                $("[id$='PLCButtonPanel1_DBP_SAVEBUTTON']").click();
            });
            setTimeout(function () {
                $(".ui-dialog .ui-dialog-titlebar-close").hide();
                $(".ui-dialog .ui-dialog-buttonset button").first().focus();
            }, 300);
        }

	    function bindDeleteCustodyHandlerWhenReady() {
		    console.log("bindDeleteCustodyHandlerWhenReady");
    		var $deleteCustodyOk = $("[id$='<%= mbDeleteCustody.ClientID %>ob']");
    		var $deleteCustodyCancel = $("[id$='<%= mbDeleteCustody.ClientID %>cb']");

            console.log($deleteCustodyOk.length);
            console.log($deleteCustodyCancel.length.length);

    		if ($deleteCustodyOk.length === 0 || $deleteCustodyCancel.length === 0) {
        		// Button not yet rendered, try again in 200ms
        		setTimeout(bindDeleteCustodyHandlerWhenReady, 200);
        		return;
    		}

    		$deleteCustodyOk.on("click", function () {
        		$deleteCustodyOk.css({
            		"pointer-events": "none",
            		"opacity": "0.6"
        		});
        		$deleteCustodyCancel.css({
            		"pointer-events": "none",
            		"opacity": "0.6"
        		});
    		});

    		console.log("Delete custody buttons are now bound.");
	}
    </script>
    <uc6:UC_SessionTimer ID="timerTimeout" runat="server" />
    </div>
    </form>

    <%--<div class="popup-ffb"></div>--%>

    <script src="../PLCWebCommon/JavaScripts/ClearedEvent.js?rev=<%= PLCSession.CurrentRev %>" type="text/JavaScript" language="JavaScript"></script>
</body>
</html>


<PLC:MessageBox ID="mbDeleteCustody" runat="server" MessageType="Confirmation" PanelCSSClass="modalPopup"
        Width="400" CaptionCSSClass="caption" PanelBackgroundCSSClass="modalBackground" Message="Are you sure you want to delete this custody?"
        Caption="Confirm" OnOkClick="DeleteCustody_Click"/>