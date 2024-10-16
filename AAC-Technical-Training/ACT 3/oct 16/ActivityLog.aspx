<%@ Page Title="Activity Log" Language="C#" MasterPageFile="~/LIMSHOME.master" AutoEventWireup="true"
    CodeBehind="ActivityLog.aspx.cs" Inherits="BEASTiLIMS.ActivityLog" %>

<%@ Register Assembly="DayPilot" Namespace="DayPilot.Web.Ui" TagPrefix="DayPilot" %>
<%@ Register Assembly="DayPilot.MonthPicker" Namespace="DayPilot.Web.UI" TagPrefix="DayPilot" %>
<%@ Register Assembly="PLCCONTROLS" Namespace="PLCCONTROLS" TagPrefix="PLC" %>
<%@ Register Src="~/PLCWebCommon/PLCPageHead.ascx" TagName="PLCPageHead" TagPrefix="ph" %>
<%@ Register Src="~/PLCWebCommon/PLCCaseSearch.ascx" TagName="CaseSearch" TagPrefix="uc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        /* Month silver theme */
        .month_silver_header
        {
            background-image: url('/Images/month_silver/top20.gif');
            background-repeat: repeat-x;
            background-color: #CFCFCF;
        }
        .month_silver_event
        {
            background-image: url('/Images/month_silver/event20.gif');
            background-repeat: repeat-x;
            background-color: #CFCFCF;
            width: 100%;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cp" runat="server">
    <ph:PLCPageHead runat="server" ID="pagehead1" include="utility" />

    <script type="text/javascript" language="javascript">
        function toggleView(viewId) {
            $('.tabHeader').each(function () { $(this).removeClass('active'); });

            if (withSubDetails)
                $("#<%= PLCButtonPanel1.ClientID %>").hide();

            if (viewId == 0) {
                // Tab
                $('#tbSummary').addClass('active');

                // View
                $('#detailsTab').hide();
                $('#summaryTab').show();
                $("input[id$='hdnActCode']").val(0);

                // Button panel
                $("#<%= PLCButtonPanel1.ClientID %>").show();
            } else if (viewId === 1) {
                // Tab
                $('#tbSummary').removeClass('active');

                // View
                $('#summaryTab').hide();
                $('#detailsTab').show();
            } else {
                $('.tabContent').each(function () { $(this).hide(); });
                $('#tbSummary').removeClass('active');
                $("#" + viewId).addClass("active");
                
                $('#summaryTab').hide();
                $('#detailsTab').show();
            }
        }

        function GetCode(code) {
            $("input[id$='hdnActCode']").val(code);
        }

        var withSubDetails = false;

        function ShowDetailsTabType(withDetailsTable) {
            withSubDetails = withDetailsTable;
            if (withDetailsTable) {
                $('#withoutDetailsTable').hide();
                $('#withDetailsTable').show();
            }
            else {
                $('#withDetailsTable').hide();
                $('#withoutDetailsTable').show();
                $("#<%= PLCButtonPanel1.ClientID %>").show();
            }
        }

        function EnableHeaders(enable) {
            if (enable) {
                $('.tabHeader').each(function () { $(this).removeAttr('disabled'); });
                $('#tbSummary').removeAttr("disabled");
            }
            else {
                $('.tabHeader').each(function () { $(this).attr('disabled', 'disabled'); });
                $('#tbSummary').attr("disabled", "disabled");
            }
        }

        $(function () {
            var prm = Sys.WebForms.PageRequestManager.getInstance();

            if (prm != null) {
                prm.add_pageLoaded(toggle);
                prm.add_pageLoaded(MaintainView);
            }

            function toggle() {
                toggleView($("input[id$='hdnActCode']").val());
            }
            function MaintainView() {
                ShowDetailsTabType(withSubDetails);
            }
        });
    </script>

    <PLC:PLCSessionVars ID="PLCSessionVars1" runat="server" />
    <asp:PlaceHolder ID="phMsgBox" runat="server"></asp:PlaceHolder>
    <div class="sectionheader">
        Activity Log</div>
    <div style="min-height: 30px; width: 100%;">
        <asp:Menu ID="Menu1" runat="server" Orientation="Horizontal" SkinID="TopMenu" OnMenuItemClick="Menu1_MenuItemClick">
            <Items>
                <asp:MenuItem Text="Activity Log Details" Value="0"></asp:MenuItem>
                <%--<asp:MenuItem Text="Activity Log Summary" Value="1"></asp:MenuItem>--%>
            </Items>
        </asp:Menu>
        <div style="border: 1px solid e8e9e4; margin: 0; padding: 0;" />
    </div>
    <asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
        <asp:View ID="View1" runat="server">
            <table cellpadding="0" cellspacing="0" class="activityLogWrapper">
                <tr>
                    <td>
                        <asp:Panel ID="pnlSearch" runat="server">
                            <table>
<%--                                <tr>
                                    <td align="right">
                                        Date &nbsp;
                                    </td>
                                    <td align="left">
                                        <asp:TextBox ID="txtActivityLogFrom" runat="server" OnTextChanged="txtActivityLog_TextChanged"
                                            Width="100px" /> onchange="javascript:__doPostBack('','');"
                                        <asp:ImageButton ID="btnCalActivityLogFrom" Style="margin-left: 1px;" runat="server"
                                            ImageUrl="~/Images/calendar.png" />
                                        <ajaxToolkit:CalendarExtender ID="calActivityLogFrom" FirstDayOfWeek="Sunday" PopupButtonID="btnCalActivityLogFrom"
                                            TargetControlID="txtActivityLogFrom" runat="server" OnClientShowing="SetDefaultDate" />
                                        <ajaxToolkit:MaskedEditExtender ID="meeActivityLogFrom" runat="server" Mask="99/99/9999"
                                            MaskType="Date" TargetControlID="txtActivityLogFrom" CultureAMPMPlaceholder=""
                                            CultureCurrencySymbolPlaceholder="" CultureDateFormat="" CultureDatePlaceholder=""
                                            CultureDecimalPlaceholder="" CultureThousandsPlaceholder="" CultureTimePlaceholder="" />
                                    </td>
                                    <td align="right">
                                        To &nbsp;
                                    </td>
                                    <td align="left">
                                        <asp:TextBox ID="txtActivityLogTo" runat="server" OnTextChanged="txtActivityLog_TextChanged"
                                            Width="100px" /> onchange="javascript:__doPostBack('','');" 
                                        <asp:ImageButton ID="btnCalActivityLogTo" Style="margin-left: 1px;" runat="server"
                                            ImageUrl="~/Images/calendar.png" />
                                        <ajaxToolkit:CalendarExtender ID="calActivityLogTo" FirstDayOfWeek="Sunday" PopupButtonID="btnCalActivityLogTo"
                                            TargetControlID="txtActivityLogTo" runat="server" OnClientShowing="SetDefaultDate" />
                                        <ajaxToolkit:MaskedEditExtender ID="meeActivityLogTo" runat="server" Mask="99/99/9999"
                                            MaskType="Date" TargetControlID="txtActivityLogTo" CultureAMPMPlaceholder=""
                                            CultureCurrencySymbolPlaceholder="" CultureDateFormat="" CultureDatePlaceholder=""
                                            CultureDecimalPlaceholder="" CultureThousandsPlaceholder="" CultureTimePlaceholder="" />
                                    </td>
                                    <td align="right">
                                        Analyst &nbsp;
                                    </td>
                                    <td align="left">
                                        <asp:PlaceHolder ID="phAnalyst" runat="server"></asp:PlaceHolder>
                                    </td>
                                    <td align="right">
                                    <input style="padding: 0 3px;" type="submit" title="Filter Activities" value="Filter" />
                                    <asp:Button ID="btnFilter" runat="server" Text="Filter" OnClick="btnFilter_Click" />
                                    </td>
                                </tr>--%>
                                <tr>
                                    <td>
                                        <PLC:PLCDBPanel ID="dbSearchActLog" PLCWhereClause="WHERE 1=0" PLCPanelName="ACTIVITYLOG_SEARCH" runat="server" IsSearchPanel="true">
                                        </PLC:PLCDBPanel>
                                    </td>
                                    <td>
                                        <asp:Button ID="btnFilter" runat="server" Text="Filter" OnClick="btnFilter_Click" />
                                    </td>
                                </tr>
                            </table>    
                        </asp:Panel>
                    </td>
                </tr>
                <tr>
                    <td>
                        <hr />
                    </td>
                </tr>
                <tr>
                    <td valign="top">
                        <PLC:PLCDBGrid ID="GridView1" Width="800px" runat="server" Height="170px" AllowPaging="True"
                            AllowSorting="True" PageSize="5" DataKeyNames="ACTIVITY_KEY,ACTIVITY_CODE" OnSelectedIndexChanged="GridView1_SelectedIndexChanged"
                            OnRowDataBound="GridView1_RowDataBound" OnSorted="GridView1_SelectedIndexChanged"
                            OnPageIndexChanged="GridView1_PageIndexChanged">
                        </PLC:PLCDBGrid>
                    </td>
                </tr>
                <tr>
                    <td>
                        <hr />
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:PlaceHolder ID="plhAttachment" runat="server" Visible="false">
                            <table class="sitecolor casetitle" width="100%" cellpadding="2" cellspacing="2" style="height: 22px;
                                margin: 2px 0px;">
                                <tr>
                                    <td width="17">
                                        <asp:HyperLink ID="lnkAttach" runat="server" NavigateUrl="~/Attachments.aspx" Text="Attachments"
                                            ImageUrl="~/Images/paperclip.gif" Width="17"></asp:HyperLink>
                                    </td>
                                    <td align="left">
                                    <asp:UpdatePanel ID="upLabCaseNumbers" runat="server">
                                        <ContentTemplate>
                                            <asp:Label ID="lblActivity" runat="server" Text=""></asp:Label>
                                            &nbsp&nbsp&nbsp&nbsp;
                                            <asp:Label ID="lblLinkedCases" runat="server" Text=""></asp:Label>
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                    </td>
                                </tr>
                            </table>
                        </asp:PlaceHolder>
                    </td>
                </tr>
                <asp:MultiView ID="mvDetails" runat="server" ActiveViewIndex="0">
                    <asp:View ID="vwMain" runat="server">
                        <tr>
                            <td colspan="6">
                                <ul class="tabs">
                                    <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                                        <ContentTemplate>
                                            <li id="tbSummary" class="active"><a href="#" onclick="toggleView(0);">Daily Activity</a></li>
                                            <%--<li id="tbDetails" runat="server"><a href="#" id="hypDetails" runat="server" onclick="toggleView(1);">
                                            </a></li>--%>
                                            <asp:Repeater ID="reptDetailsHeader" runat="server">
                                                <ItemTemplate>
                                                    <li id='<%# Eval("CODE") %>' class="tabHeader"><asp:LinkButton ID="lnkDetailsTab" runat="server" OnClientClick='GetCode($(this).parent().attr("id"));' OnClick="lnkDetailsTab_Click"><%# Eval("DESCRIPTION") %></asp:LinkButton></li>
                                                </ItemTemplate>
                                            </asp:Repeater>
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                </ul>
                                <asp:HiddenField ID="hdnActCode" runat="server" />
                                <div class="activityLogDBPanel tab_container">
                                    <div id="summaryTab">
                                        <PLC:DBPanel ID="PLCDBPanel1" runat="server" PLCPanelName="ACTIVITYLOG" OnPLCDBPanelGetNewRecord="PLCButton1_PLCDBPanelGetNewRecord"
                                            PLCDisableCaching="true" PLCWhereClause="WHERE 0=1" PLCDataTable="TV_ACTIVITY" OnPLCDBPanelCodeHeadChanged="PLCDBPanel1_PLCDBPanelCodeHeadChanged"
                                            PLCAttachPopupTo="body" PLCAuditCode="98" PLCAuditSubCode="5" PLCDeleteAuditCode="98" PLCDeleteAuditSubCode="1">
                                        </PLC:DBPanel>
                                    </div>

                                    <div id="detailsTab" style="display: none;">
                                        <div id="withoutDetailsTable" class="dbpActivityLog" style="display: none;">
                                            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                                                <ContentTemplate>
                                                    <PLC:DBPanel ID="dbpDetails" runat="server" PLCDisableCaching="true" PLCWhereClause="0=1"
                                                        OnPLCDBPanelGetNewRecord="dbpDetails_PLCDBPanelGetNewRecord" CreateControlsOnExplicitCall="true"
                                                        OnPLCDBPanelAddCustomFields="dbpDetails_PLCDBPanelAddCustomFields" PLCAllowBlankRecordSave="true" PLCAttachPopupTo="body"
                                                        PLCAuditCode="98" PLCAuditSubCode="5" PLCDeleteAuditCode="98" PLCDeleteAuditSubCode="1">
                                                    </PLC:DBPanel>
                                                </ContentTemplate>
                                            </asp:UpdatePanel>
                                        </div>
                                        <div id="withDetailsTable" style="display: none; width:800px;">
                                            <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                                                <ContentTemplate>
                                                    <PLC:PLCDBGrid ID="grdSubDetails" runat="server" AllowPaging="true" AllowSorting="true" PageSize="5"
                                                        EmptyDataText="No records found." DataKeyNames="DETAILS_KEY" OnSelectedIndexChanged="grdSubDetails_SelectedIndexChanged"
                                                        OnSorted="grdSubDetails_Sorted" OnRowDataBound="grdSubDetails_RowDataBound"></PLC:PLCDBGrid>
                                                    <PLC:DBPanel ID="dbpSubDetails" runat="server" PLCWhereClause="0=1" CreateControlsOnExplicitCall="true"
                                                        PLCDisableCaching="true" OnPLCDBPanelGetNewRecord="dbpSubDetails_PLCDBPanelGetNewRecord"
                                                        OnPLCDBPanelAddCustomFields="grdSubDetails_PLCDBPanelAddCustomFields" PLCAttachPopupTo="body" 
                                                        PLCAuditCode="98" PLCAuditSubCode="5" PLCDeleteAuditCode="98" PLCDeleteAuditSubCode="1"></PLC:DBPanel>
                                                </ContentTemplate>
                                            </asp:UpdatePanel>
                                            <br />
                                                <PLC:PLCButtonPanel ID="bplSubDetails" runat="server" PLCTargetControlID="dbpSubDetails"
                                                        OnPLCButtonClick="bplSubDetails_PLCButtonClick" PLCShowAddButton="true" PLCShowEditButtons="true"
                                                        PLCShowDeleteButton="true"></PLC:PLCButtonPanel>
                                            <br />
                                        </div>
                                </div>
                                <PLC:PLCButtonPanel ID="PLCButtonPanel1" runat="server" PLCTargetControlID="PLCDBPanel1"
                                    OnPLCButtonClick="PLCButtonPanel1_PLCButtonClick" PLCShowAddButton="true" PLCShowEditButtons="true"
                                    PLCShowDeleteButton="true" PLCCustomButtons="Approve,Reject,Link Cases,Print,Monitor,Back To Case" PLCDisplayBottomBorder="true"
                                    PLCDisplayTopBorder="true">
                                </PLC:PLCButtonPanel>
                            </td>
                        </tr>
                        <asp:Button ID="btnDetails" runat="server" Text="Details" OnClick="btnDetails_Click" style="display:none" />
                        <asp:Button ID="DummyButtonPwd" runat="server" style="display:none;" />
                    </asp:View>
                    <asp:View ID="vwCourtMonitor" runat="server">
                        <tr>
                            <td colspan="6">
                                <div style="margin: 10px;">
                                    <span style="font-weight: bold;"><%= GridView1.SelectedDataKey["ACTIVITY_CODE"].ToString() %> Monitoring by Supervisor</span>
                                    <br />
                                    <br />
                                    <asp:PlaceHolder ID="phActivityMonitor" runat="server" />
                                    <br />
                                    <asp:HiddenField ID="hdnActKey" runat="server" />
                                    <asp:Button ID="btnCMEdit" runat="server" Text="Edit" Width="100" OnClick="btnCMEdit_Click" />
                                    <asp:Button ID="btnCMSave" runat="server" Text="Save" Width="100" Enabled="false"
                                        OnClick="btnCMSave_Click" />
                                    <asp:Button ID="btnCMCancel" runat="server" Text="Cancel" Width="100" Enabled="false"
                                        OnClick="btnCMCancel_Click" />
                                    <asp:Button ID="btnCMPrint" runat="server" Text="Print Form" Width="100" OnClick="btnCMPrint_Click" />
                                    <asp:Button ID="btnCMClose" runat="server" Text="Close" Width="100" OnClick="btnCMClose_Click" />
                                </div>
                            </td>
                        </tr>
                    </asp:View>
                </asp:MultiView>
            </table>
        </asp:View>
        <asp:View ID="View2" runat="server">
            <DayPilot:MonthPicker ID="dpmPicker" runat="server" />
            <asp:Button ID="btnChangeMonth" runat="server" Text="Go to Month" OnClick="btnChangeMonth_Click" />
            <DayPilot:DayPilotMonth ID="dpmActivity" runat="server" DataEndField="end" DataStartField="start"
                DataTextField="name" DataValueField="id" DataTagFields="name, id" EventStartTime="false"
                EventEndTime="false" EventCorners="Rounded" CssClassPrefix="month_silver_" EventClickHandling="PostBack"
                OnEventClick="dpmActivity_EventClick" Width="100%" />
        </asp:View>
    </asp:MultiView>

    <div id="dialog-linkcases" style="padding: 10px; display: none;">
        <div id="dialog-linkcases-content">
            <asp:UpdatePanel ID="UpdatePanel4" runat="server">
                <ContentTemplate>
                    <div style="margin: 10px;">
                        <div style="min-height: 180px;">
                            <PLC:PLCDBGrid ID="grdLinkCases" runat="server" PLCGridName="ACTIVITYLOG_LINKCASES" DataKeyNames="ACTIVITY_KEY,CASE_KEY" AllowPaging="true" AllowSorting="true" PageSize="5"
                                EmptyDataText="No records found." OnSelectedIndexChanged="grdLinkCases_SelectedIndexChanged" Width="100%" OnSorted="grdLinkCases_Sorted" OnPageIndexChanged="grdLinkCases_PageIndexChanged">
                            </PLC:PLCDBGrid>
                        </div>
                        <PLC:DBPanel ID="dbpLinkCases" runat="server" PLCWhereClause="WHERE 0=1" PLCPanelName="ACTIVITYLOG_LINKCASES" 
                            PLCDataTable="TV_LABCASE" PLCAllowBlankRecordSave="false"
                            PLCAuditCode="98" PLCAuditSubCode="7" PLCDeleteAuditCode="98" PLCDeleteAuditSubCode="8">
                        </PLC:DBPanel>
                        <br />
                        <asp:Label ID="lblLinkCaseMsg" runat="server" ForeColor="Red" Visible="false" Text="Message"></asp:Label>
                        <div style="padding: 10px 0px 10px 0;">
                            <asp:Button ID="btnLinkCaseAdd" runat="server" Text="Add" Width="100px" OnClick="btnLinkCaseAdd_Click" />
                            <asp:Button ID="btnLinkCaseSave" runat="server" Text="Save" Width="100px" OnClick="btnLinkCaseSave_Click" />
                            <asp:Button ID="btnLinkCaseCancel" runat="server" Text="Cancel" Width="100px" OnClick="btnLinkCaseCancel_Click" />
                            <asp:Button ID="btnLinkCaseDelete" runat="server" Text="Delete" Width="100px" OnClick="btnLinkCaseDelete_Click" />
                            <asp:Button ID="btnCaseSearch" runat="server" Text="Case Search" Width="" OnClientClick="ShowSearchLinkCasePopup(); return false;" />
                        </div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>
    <asp:UpdatePanel ID="UpdatePanel5" runat="server">
        <ContentTemplate>
            <PLC:MessageBox ID="msgPromptLinkCase" runat="server" PanelCSSClass="modalPopup" CaptionCSSClass="caption"
            PanelBackgroundCSSClass="modalBackground" OnOkClick="msgPromptLinkCase_OkClick"/>
            <PLC:MessageBox ID="msgDeleteLinkCase" runat="server" PanelCSSClass="modalPopup" CaptionCSSClass="caption" MessageType="Confirmation" Caption="Delete Case Link"
            Message="Are you sure you want to delete this record?" PanelBackgroundCSSClass="modalBackground" OnOkClick="DeleteLinkCase_YesClick" OnCancelClick="msgPromptLinkCase_OkClick" />
        </ContentTemplate>
    </asp:UpdatePanel>
    <uc1:CaseSearch ID="CaseSearch" runat="server" OnSelectedCaseKeyChanged="CaseSearch_SelectedCaseKeyChanged"/>

    <script type="text/javascript">
        function ShowSearchLinkCasePopup() {
            OpenDialogCaseSearch();
        }

        function CustomCloseCaseSearch() {
        }

        function ShowLinkCaseDialog() {
            $("[id='dialog-linkcases']:eq(1)").dialog("destroy").remove();
            $("#dialog-linkcases").dialog({
                dialogClass: "no-close",
                autoOpen: true,
                modal: true,
                resizable: false,
                draggable: true,
                closeOnEscape: false,
                title: 'Link Cases',
                width: 750,
                buttons: {
                    Close: function () {
                        $(this).dialog("close");
                        // show daily activity tab
                        toggleView('0');

                        var $bnCaseSearchClose = $('[id$="bnCaseSearchClose"]');
                        if ($bnCaseSearchClose != null) {
                            var $loading = $('[id$="up1"]');
                            $loading.show();
                            $bnCaseSearchClose.click();
                        }
                    }
                },
                open: function () {
                    $(this).parent().appendTo("form");
                }
            });
        }
    </script>
</asp:Content>
