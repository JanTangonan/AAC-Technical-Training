<%@ Page Language="C#" MasterPageFile="~/LIMSHOME.master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="BEASTiLIMS.DashBoard" Title="BEASTiLIMS Home Page" %>

<%@ Register Assembly="System.Web.Extensions" Namespace="System.Web.UI" TagPrefix="asp" %>
<%@ Register Assembly="PLCCONTROLS" Namespace="PLCCONTROLS" TagPrefix="PLC" %>
<%@ Register Src="~/PLCWebCommon/CodeHead.ascx" TagName="CodeHead" TagPrefix="uc1" %>
<%@ Register Src="~/PLCWebCommon/PLCPageHead.ascx" TagName="PLCPageHead" TagPrefix="uc5" %>
<%@ Register Src="~/PLCWebCommon/PLCWebControl.ascx" TagName="PLCWebControl" TagPrefix="uc1" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="./plcwebcommon/dbgridjs/dbgrid.css">
    <uc5:PLCPageHead runat="server" ID="pagehead1" include="jquery-ui" />
    <style type="text/css">
    .IOBoard
    {
        font-size: 14px;
        font-weight: bold;
    }

    .btn-admin-close
    {
        width: 140px;
    }

    .btn-admin-close input
    {
        width: 100%;
    }

    .hidden
    {
        display: none;
    }

    .toggleWrapper {
        margin: 10px 0 !important;
    }

   .dashboard-panel {
       visibility:hidden;
    }
   </style>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cp" runat="server">
    <PLC:PLCSessionVars ID="PLCSessionVars1" runat="server" />



 



    <div style="display: none;">
        <uc1:CodeHead ID="chDummy" runat="server" TableName="TV_DEPTNAME" />
    </div>
    <asp:PlaceHolder ID="phMsgBox" runat="server"></asp:PlaceHolder>
    <div class="dashboard-panel">
    <asp:Panel ID="pnlMarquee" runat="server" Width="100%" Style="margin-bottom: 7px;" Visible="false" class="dashboard-panel-item">
        <marquee id="mqAIMarquee" runat="server" class="sectionheader" scrollamount="5" style="width:100%;">
            <asp:Label ID="lbAIMarquee" runat="server" ></asp:Label>
        </marquee>
    </asp:Panel>
    <asp:Panel ID="pnlCharts" runat="server" Width="100%" Style="margin-bottom: 7px;" class="dashboard-panel-item">
        <asp:Panel ID="pnlChartHeader" runat="server" Width="100%" CssClass="sectionheader">
            <asp:ImageButton ID="btnChart" runat="server" AlternateText="Collapse" ImageUrl="~/Images/collapse.jpg" OnClick="btnChart_Click" />&nbsp;
            <PLC:PLCLabel ID="lbChartTitle" runat="server" Text="Charts" PromptCode="Dashboard.lbChartTitle"></PLC:PLCLabel> <PLC:PLCLinkButton runat="server" ID="btnFullScreenLink" text="Full Screen" PromptCode="Dashboard.btnFullScreenLink" OnClick="btnFullScreen_Click" > </PLC:PLCLinkButton>      
        </asp:Panel>
        <asp:Panel ID="pnlChartDetail" runat="server" Width="100%" BorderColor="#cfccbf"  BorderStyle="Solid" BorderWidth="2px" style >
            <div style="margin: 1px;  position:relative;">
                <iframe  style="width:100%; height:540px; overflow: hidden;" runat="server" id="ifChart"></iframe>
                

            </div>
            <div style="width:100%; height:30px; ">
                <asp:DropDownList ID="ddlChartType" runat="server" Width="250" 
                    AutoPostBack="True" 
                    onselectedindexchanged="ddlChartType_SelectedIndexChanged" 
                    ViewStateMode="Enabled"  ></asp:DropDownList>
                <PLC:PLCButton ID="btnRefreshChart" runat="server" Text="Refresh" PromptCode="Dashboard.btnRefreshChart" Width="80" OnClick="btnRefreshChart_Click" /> 
            </div>
        </asp:Panel>
    </asp:Panel>

    <asp:Panel ID="pnlQF" runat="server" Width="100%" Style="margin-bottom: 7px;" class="dashboard-panel-item">
        <asp:Panel ID="pnlQFHeader" runat="server" Width="100%" CssClass="sectionheader">
            <asp:ImageButton ID="bnQF" runat="server" AlternateText="Collapse" ImageUrl="~/Images/collapse.jpg"
                OnClick="bnQF_Click" />&nbsp;
            <PLC:PLCLabel ID="laQF" runat="server" Text="Quick Find" PromptCode="Dashboard.laQF"></PLC:PLCLabel>
        </asp:Panel>
        <asp:Panel ID="pnlQFDetail" runat="server" CssClass="sectiondetail" DefaultButton="bnGo">
            <table style="margin: 15px;">
                
                <tr>
                    <td colspan="5">
                        <PLC:PLCDBPanel ID="dbpQuickFind" PLCWhereClause="WHERE 1=0" PLCPanelName="QUICKFIND" runat="server" IsSearchPanel="true" OnPLCDBPanelCodeHeadChanged="dbpQuickFind_PLCDBPanelCodeHeadChanged" >
                        </PLC:PLCDBPanel>
                    </td>
                </tr>
                <tr>
                    <td colspan="5">
                        <asp:Label ID="lbSearchStatus" runat="server" ForeColor="Red"></asp:Label>
                    </td>
                </tr>
                <tr class="qfbuttons">
                    <td colspan="5" style="padding-top: 10px;">
                        <PLC:PLCButton ID="Button4" runat="server" Text="Clear" PromptCode="Dashboard.Button4" Width="80" OnClick="Button4_Click" />
                        <PLC:PLCButton ID="bnGo" runat="server" OnClick="bnGo_Click" Text="Search" PromptCode="Dashboard.bnGo" Width="80" /> <%--ValidationGroup="ValidateURN"--%>
                    </td>
                </tr>
            </table>
            <table>
               <tr>
                    <td colspan="5" style="padding-left:15px; padding-bottom:20px">
                        <PLC:PLCDBGrid ID="grdSearchResults" runat="server" PLCGridName="QUICKFIND" AllowPaging="True" AllowSorting="True" 
                            DataKeyNames="CASE_KEY" Width="100%" OnSelectedIndexChanged="grdSearchResults_SelectedIndexChanged"
                            OnRowDataBound="grdSearchResults_RowDataBound" Visible="false" OnPageIndexChanging = "grdSearchResults_PageIndexChanging"
                            OnSorting="grdSearchResults_Sorting">
                        </PLC:PLCDBGrid>
                    </td>
               </tr>
            </table>
        </asp:Panel>
    </asp:Panel>
    <asp:Panel ID="pnlRC" runat="server" Width="100%" Style="margin-bottom: 7px;" class="dashboard-panel-item">
        <asp:Panel ID="pnlRCHeader" runat="server" Width="100%" CssClass="sectionheader">
            <asp:ImageButton ID="bnRC" runat="server" AlternateText="Collapse" ImageUrl="~/Images/collapse.jpg"
                OnClick="bnRC_Click" />&nbsp;
            <PLC:PLCLabel ID="laRecentCases" runat="server" Text="$Dashboard_RecentCases"></PLC:PLCLabel>
              <asp:DropDownList ID="ddlRecentCases" runat="server" Width="250" 
                    AutoPostBack="True" 
                    onselectedindexchanged="ddlRecentCases_SelectedIndexChanged" 
                    ViewStateMode="Enabled"  ></asp:DropDownList>
        </asp:Panel>
        <asp:Panel ID="pnlRCDetail" runat="server" CssClass="sectiondetail">
            <div style="margin: 5px;">
                <PLC:PLCDBGrid ID="grRecentCases" runat="server" AllowPaging="True" AllowSorting="True"
                    DataKeyNames="case_key" Width="100%" OnSelectedIndexChanged="grRecentCases_SelectedIndexChanged"
                    OnRowCreated="grRecentCases_RowCreated"
                    OnRowDataBound="grRecentCases_RowDataBound" PLCGridWidth="100%" HeightOnScrollEnabled="250px">
                </PLC:PLCDBGrid>
            </div>
        </asp:Panel>
    </asp:Panel>
    <asp:Panel ID="pnlAssignments" runat="server" Width="100%" Style="margin-bottom: 7px;"
        Visible="false" class="dashboard-panel-item">
        <asp:Panel ID="pnlAssignmentsHeader" runat="server" Width="100%" CssClass="sectionheader">
            <asp:ImageButton ID="bnAssignments" runat="server" AlternateText="Collapse" ImageUrl="~/Images/collapse.jpg"
                OnClick="bnAssignments_Click" />&nbsp;
            <PLC:PLCLabel ID="lbMyAssign" runat="server" Text="$Dashboard_MyAssignment"></PLC:PLCLabel>            
        </asp:Panel>
        <asp:Panel ID="pnlAssignmentsDetail" runat="server" CssClass="sectiondetail">
            <div style="margin: 5px;">
                <PLC:PLCDBGrid ID="grdAssignments" runat="server" AllowPaging="True" AllowSorting="True"
                    DataKeyNames="EXAM_KEY,CASE_KEY,SECTION" Width="100%" OnSelectedIndexChanged="grdAssignments_SelectedIndexChanged"
                    OnRowCreated="grdAssignments_RowCreated" OnDataBound="grdAssignments_DataBound"
                    PLCGridWidth="100%">
                </PLC:PLCDBGrid>
            </div>
            <div style="margin: 10px 5px;">
                <PLC:PLCButton ID="btnPrint" runat="server" Text="Print" PromptCode="Dashboard.btnPrint" Width="80" OnClick="btnPrint_Click" />
                <input type="button" id="btnShowPrintWorksheets" class="default hidden" value="Print Worksheets" onclick="ShowAssignmentWorksheetsPopup();" />
            </div>
        </asp:Panel>
    </asp:Panel>
    <asp:Panel ID="pnlAI" runat="server" Visible="false" Width="100%" Style="margin-bottom: 7px;" class="dashboard-panel-item">
        <asp:Panel ID="pnlAIHeader" runat="server" Width="100%" CssClass="sectionheader"
            Visible="True">
            <asp:ImageButton ID="bnAI" runat="server" AlternateText="Collapse" ImageUrl="~/Images/collapse.jpg"
                OnClick="bnAI_Click" />&nbsp;
            <PLC:PLCLabel ID="Label1" runat="server" Text="Action Items" PromptCode="Dashboard.Label1"></PLC:PLCLabel>
        </asp:Panel>
        <asp:Panel ID="pnlActionItems" runat="server" CssClass="sectiondetail">
            <div style="margin: 15px;">
                <asp:Repeater ID="rptrActionItems" runat="server">
                    <ItemTemplate>
                        <div style="margin: 6px;">
                            <asp:LinkButton ID="PROMPT" runat="server" ForeColor="#003366" OnClick="PROMPT_Click"
                                CommandName='<%# Eval("ACTION_PAGE") %>' CommandArgument='<%# Eval("ACN_ITEM_CODE") + "_" + Eval("ACN_TYPE_CODE") %>'
                                Font-Bold="true" Text='<%# Eval("PROMPT") %>' Visible='<%# SummaryCount(sender, Convert.ToString(Eval("SUMMARY_QUERY")), Convert.ToString(Eval("PROMPT")), Convert.ToString(Eval("ACN_ITEM_CODE"))) %>'></asp:LinkButton>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </asp:Panel>
    </asp:Panel>
    <asp:Panel ID="pnlPending" runat="server" Visible="false" Width="100%" Style="margin-bottom: 7px;" class="dashboard-panel-item">
        <asp:Panel ID="pnlPendingHeader" runat="server" Width="100%" CssClass="sectionheader"
            Visible="True">
            <asp:ImageButton ID="bnPending" runat="server" AlternateText="Collapse" ImageUrl="~/Images/collapse.jpg"
                OnClick="bnPending_Click" />&nbsp;
            <PLC:PLCLabel ID="Label3" runat="server" Text="Notifications" PromptCode="Dashboard.Label3"></PLC:PLCLabel></asp:Panel>
        <asp:Panel ID="pnlPendingItems" runat="server" CssClass="sectiondetail">
            <div style="margin: 15px;">
                <asp:Repeater ID="rptrPendingItems" runat="server">
                    <ItemTemplate>
                        <div style="margin: 6px;">
                            <asp:LinkButton ID="PROMPT" runat="server" ForeColor="#003366" OnClick="PROMPT_Click"
                                CommandName='<%# Eval("ACTION_PAGE") %>' CommandArgument='<%# Eval("ACN_ITEM_CODE") + "_" + Eval("ACN_TYPE_CODE") %>'
                                Font-Bold="true" Text='<%# Eval("PROMPT") %>' Visible='<%# SummaryCount(sender, Convert.ToString(Eval("SUMMARY_QUERY")), Convert.ToString(Eval("PROMPT")), Convert.ToString(Eval("ACN_ITEM_CODE"))) %>'></asp:LinkButton>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </asp:Panel>
    </asp:Panel>
    <asp:HiddenField ID="hdnIsPageLoad" runat="server" />
    <asp:HiddenField ID="hdnRefreshGrid" runat="server" />
    <asp:HiddenField ID="hdnIsDefaultScrollPosition" runat="server"/>
    <asp:HiddenField ID="hdnDoNotFocusOnQF" runat="server"/>
    <asp:Panel ID="pnlInOut" runat="server" Width="100%" Style="margin-bottom: 7px;" Visible="false" class="dashboard-panel-item">
        <asp:Panel ID="pnlInOutHeader" runat="server" Width="100%" CssClass="sectionheader">

        <table width="100%" class="IOBoard">
            <tr>
                <td style="width:45%">
                    <asp:ImageButton ID="bnInOut" runat="server" AlternateText="Collapse" ImageUrl="~/Images/collapse.jpg"
                OnClick="bnInOut_Click" />&nbsp;
                    <PLC:PLCLabel ID="Label2" runat="server" Text="IN/OUT Board" PromptCode="Dashboard.Label2"></PLC:PLCLabel>&nbsp;&nbsp;   
                    <asp:ImageButton ID="bnFilter" runat="server" AlternateText="Filter" ImageUrl="~/Images/FilterIO.png" ToolTip="Filter" Enabled="false" Height="20px" Width="20px"/> 
                    <asp:LinkButton ID="linkFilter" runat="server" OnClientClick="return ShowFilterPopUp();" Text="" Enabled="false"/>
                </td>
                <td style="width:5%">
                </td>
                <td style="align:right;width:35%">
                       <PLC:PLCLabel ID="lblLastUpdate" runat="server" Text="Last Refresh:" PromptCode="Dashboard.lblLastUpdate"></PLC:PLCLabel>
                      
                </td>
                <td style="align:right;width:15%;margin-right:5px">
                      <PLC:PLCButton runat="server" ID="btnForceRefresh" Text="Force Refresh" PromptCode="Dashboard.btnForceRefresh" OnClick="btnForceRefresh_Click"/>   
                </td>
                
            </tr>
        
        </table>
                                
        </asp:Panel>
       
        <asp:Panel ID="pnlInOutBoard" runat="server" CssClass="sectiondetail">
          <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
            <div style="margin: 5px;">
                <table>
                    <tr>
                        <td>
                                
                            <PLC:PLCDBGrid ID="dbgInOut" runat="server" AllowPaging="false" AllowSorting="True" PLCGridName="INOUTBOARD"
                            DataKeyNames="ENTRY_TYPE,ANALYST,SIGNOUT_STATUS" Width="100%"  EmptyDataText="No records found." OnRowCommand="dbgInOut_RowCommand" 
                            CancelPostbackOnClick="true" PLCGridWidth="100%"  OnDataBound="dbgInOut_DataBound" UseDataCaching="true">
                            </PLC:PLCDBGrid>
                                                                      
                        </td>
                    </tr>
                    <tr>
                        <td>
                              <PLC:PLCDBPanel ID="dbpInOut" PLCPanelName="INOUTBOARD" runat="server" OnPLCDBPanelValidate="dbpInOut_PLCDBPanelValidate">
                              </PLC:PLCDBPanel>
                              <br />
                              <PLC:PLCButton ID="btnSubmit" runat="server" Text="Submit" PromptCode="Dashboard.btnSubmit" Width="80" OnClick="btnSubmit_Click"/>
                        </td>
                    </tr>                               
                </table>
             
            </div>
            </ContentTemplate>
            </asp:UpdatePanel>         
        </asp:Panel>
    </div>

     <div id="IOAnalystContactInfo" style="display:none;">
         <asp:UpdatePanel ID="UpdatePanel2" runat="server">
            <ContentTemplate>
            <div style="display:inline-block;vertical-align:top;">
                <PLC:PLCDBPanel ID="dbpIOContactInfo" runat="server" PLCPanelName="IOANALYSTCONTACT"  PLCDataTable="TV_ANALYST" Width="500px"></PLC:PLCDBPanel>
            </div>
        </ContentTemplate>
        </asp:UpdatePanel> 
    </div>
    </asp:Panel>
      <div id="divFilter" style="display:none;">  
       <asp:UpdatePanel ID="UpdatePanel3" runat="server">
            <ContentTemplate>    
            <div style="display:inline-block;vertical-align:top;width:500px;height:350px;overflow:auto;">      
                 <table>
                    <tr>
                        <td></td>
                        <td>
                             <PLC:PLCLabel runat="server" ID="lblLabCode" Text="Filter by Lab Code" PromptCode="Dashboard.lblLabCode"></PLC:PLCLabel>
                        </td>
                        <td>
                             <PLC:FlexBox ID="fbLabCode" runat="server" TableName="TV_LABCTRL" OnValueChanged="flexbox_Changed" Width="200px" AutoPostBack="true" AttachPopupTo=".popup-ffb"></PLC:FlexBox>   
                        </td>
                    </tr>
                    <tr>
                        <td></td>
                        <td>
                              <PLC:PLCLabel runat="server"  ID="lblSection" Text="Filter by Section" PromptCode="Dashboard.lblSection"></PLC:PLCLabel>
                        </td>
                         <td>
                             <PLC:FlexBox ID="fbSection" runat="server" TableName="TV_EXAMCODE" OnValueChanged="flexbox_Changed" Width="200px" AutoPostBack="true" CodeCondition=" (ACTIVE <> 'F' OR ACTIVE IS NULL)" AttachPopupTo=".popup-ffb"></PLC:FlexBox>   
                        </td>
                    </tr> 
                    <tr>
                        <td></td>
                        <td>
                            <PLC:PLCLabel runat="server"  ID="lblAnalyst" Text="Filter by Analyst" PromptCode="Dashboard.lblAnalyst"></PLC:PLCLabel>
                            
                        </td>
                         <td>
                             <PLC:FlexBox ID="fbAnalyst" runat="server" TableName="TV_ANALYST" OnValueChanged="flexbox_Changed" Width="200px" AutoPostBack="true"  AttachPopupTo=".popup-ffb"></PLC:FlexBox>   
                        </td>
                    </tr>
                      
                    <tr>
                        <td></td>
                        <td valign="top">
                            <PLC:PLCLabel runat="server" ID="lblGroupCode" Text="Filter by Group Code" PromptCode="Dashboard.lblGroupCode"></PLC:PLCLabel>
                        </td>
                        <td valign="top">
                             <PLC:CodeMultiPick ID="mpGroupCode" runat="server" CodeTable="TV_GROUPS" CodeCol="GROUP_CODE" DescCol="GROUP_DESCRIPTION" HeaderPrompt="Select Group Code" Width="200px" 
                                PostBack="true" WhereClause=" (ACTIVE <> 'F' OR ACTIVE IS NULL)"/>
                             <asp:Button ID="btnPostBack" OnClick="btnPostBack_Click" runat="server" style="display:none;"/>
                        </td>
                    
                    </tr>         
                </table>
                 
            </div>
        </ContentTemplate>
        </asp:UpdatePanel>
     </div>


    <asp:Button ID="btnAdminClose" runat="server" Style="display: none;" />
    <ajaxToolkit:ModalPopupExtender ID="mpeAdminClose" runat="server" BackgroundCssClass="modalBackground"
        PopupControlID="pnlAdminClose" PopupDragHandleControlID="pnlAdminCloseCaption"
        TargetControlID="btnAdminClose" DropShadow="false" OkControlID="btnOK" CancelControlID="btnNoAdminClose"
        Drag="true">
    </ajaxToolkit:ModalPopupExtender>
    <asp:Panel ID="pnlAdminClose" runat="server" CssClass="modalPopup flexboxscroll" Width="" Style="display: none;">
        <asp:Panel ID="pnlAdminCloseCaption" runat="server" Height="20px" CssClass="caption">
            Admin Close Confirmation
        </asp:Panel>
        <asp:HiddenField ID="hdnExamKey" runat="server" />
        <div style="padding: 20px; width: 100%; text-align: left;">
            <div style="padding-bottom: 5px;">
                <b>Are you sure you want to close this report?</b>
            </div>
            <div>
                <asp:Label ID="lblURNText" runat="server" Text=""></asp:Label>:
                <asp:Label ID="lblURN" runat="server" Text=""></asp:Label>
            </div>
            <div>
                Sequence:
                <asp:Label ID="lblSequence" runat="server" Text=""></asp:Label>
            </div>
            <asp:PlaceHolder ID="plhCloseCode" runat="server" Visible="false">
                <hr style="margin: 10px 0px;" />
                <table cellpadding="0" cellspacing="0">
                    <tr>
                        <td>
                            Completion Code:&nbsp;
                        </td>
                        <td>
                            <asp:PlaceHolder ID="plhCompletionCode" runat="server"></asp:PlaceHolder>
                        </td>
                    </tr>
                </table>
                <div>
                    Comments:
                    <span id="spRequired" runat="server" visible="false" style="color:Red;" >*</span>
                </div>
                <div>
                    <asp:TextBox ID="txtComments" runat="server" TextMode="MultiLine" Rows="4" Width="100%"></asp:TextBox>
                </div>
                <div>
                    <asp:Label ID="lblError" runat="server" Text="" Visible="false" ForeColor="Red"></asp:Label>
                </div>
            </asp:PlaceHolder>
        </div>
        <div align="center" style="padding: 10px;">
            <PLC:PLCButton ID="btnYesAdminClose" runat="server" Text="Yes" PromptCode="Dashboard.btnYesAdminClose" Width="80px" OnClick="btnYesAdminClose_Click"
                ValidationGroup="AdminClose" />
            <PLC:PLCButton ID="btnNoAdminClose" runat="server" Text="No" PromptCode="Dashboard.btnNoAdminClose" Width="80px" />
        </div>
        <asp:Button ID="btnOK" runat="server" Style="display: none;" />
    </asp:Panel>

    <div id="dlgPrintAllContainers" style="display: none;">
        <div id="dlgPrintAllContainers-content" style="height: 400px; max-height: 400px; overflow-y: auto;">
            <asp:UpdatePanel runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                <ContentTemplate>
                    <PLC:PLCDBGrid ID="grdPrintAllContainers"
                        runat="server"
                        PLCGridName="MYASSIGNMENTS_PRINT_LABELS"
                        DataKeyNames="CONTAINER_KEY"
                        FirstColumnCheckbox="true"
                        CancelPostbackOnClick="true"
                        AllowPaging="true" 
                        AllowSorting="true"
                        EmptyDataText="There are no container labels to print."
                        Width="100%">
                    </PLC:PLCDBGrid>
                    <PLC:PLCButton ID="btnPrintContainers" runat="server" Text="Print" PromptCode="Dashboard.btnPrintContainers" OnClick="btnPrintContainers_Click" style="display: none;"/>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>

    <div id="dlgPrintAllWorksheets" style="display: none;">
        <div id="dlgPrintAllWorksheets-content" style="height: 400px; max-height: 400px; overflow-y: auto;">
            <div id="grdWorksheet"></div>
        </div>
    </div>

    <uc1:PLCWebControl ID="WebOCX" runat="server"/>
    <div id="dvActionAlert" style="display:none;"></div>

    <asp:Button ID="btnPrintTransferRpt" runat="server" OnClick="btnPrintTransferRpt_Click" Style="display: none;" />

    <script src="./PLCWebCommon/JavaScripts/SigWebTablet.js" type="text/javascript"></script>
    <script src="./plcwebcommon/javascripts/extension.js"></script>
    <script src="./plcwebcommon/javascripts/polyfill.js"></script>
    <script src="./plcwebcommon/dbgridjs/dbgrid.js"></script>
    <script src="./PLCWebCommon/Javascripts/PLCControls.js" type="text/javascript"></script>
    <script type="text/javascript" language="javascript">
        function ShowActionAlert(message, height) {
            var settings = {
                title: "Action Item Alert",
                resizable: false,
                modal: true,
                width: 450,
                buttons: [{
                    id: "btnOkDlg",
                    text: "Ok",
                    click: function () {
                        $(this).dialog("close");
                        selectQuickFindFirstField();
                    }
                }],
                open: function (event, ui) {
                    height = height ? height : "150px";
                    $(this).css('height', height).css('overflow', 'auto');

                    // center align title and hide close button
                    $(this).closest('.ui-dialog').find('.ui-dialog-title').css('float', 'none').css('font-size', '14px');
                    $(this).closest('.ui-dialog').find('.ui-dialog-titlebar').css('text-align', 'center');
                    $(this).closest('.ui-dialog').find('.ui-dialog-titlebar-close').hide();

                    // center align buttons and apply css
                    $(this).closest('.ui-dialog').find('.ui-dialog-buttonset').css('float', 'none');
                    $(this).closest('.ui-dialog').find('.ui-dialog-buttonpane').css('border', '0').css('margin', '0').css('text-align', 'center').css('background-color', '#f5f5f5');
                    $(this).closest("#btnOkDlg").focus();  
                }
            };
            $("#dvActionAlert").html("<p>" + message + "</p>");
            $("#dvActionAlert").dialog(settings);
        }

        function selectQuickFindFirstField() {
             setTimeout(function () {
                var dbpQuickFind = new DBPanel("dbpQuickFind");
                dbpQuickFind.focusFirstField();
            }, 200);
        }

        function contentPageLoad() {
            //setFilterFlexboxPosition($("#divFilter"));
            handleCheckStateForPrintContainers();
        }

        function fixMyAssignmentsGrid() {
            setTimeout(function () {
                $("[id$='grdAssignments_header']").find("th").each(function () {
                    var colClass = "dgcell-" + $(this).find("a").text().replace(" ", "").replace("#", "");
                    var colWidth = $(this).width();

                    colClass = jqEscMetaChar(colClass);

                    $("[id$='grdAssignments']").find("." + colClass).width(colWidth + "px");
                });
            });
        }
        fixMyAssignmentsGrid();
        
        $(function () {
            var prm = Sys.WebForms.PageRequestManager.getInstance();
            if (prm != null) {
                prm.add_pageLoaded(enablePanelOnEnterSubmit);
                prm.add_pageLoaded(function () {
                    setTimeout(setQuickFindButtonTabIndex, 250);
                });
                prm.add_pageLoaded(windowFocus);
                prm.add_pageLoaded(fixMyAssignmentsGrid);
            }

            function enablePanelOnEnterSubmit() {
                $("div[id$='dbpQuickFind']").find('input[type=text]').each(function (idx, el) {
                    var $tb = $(el);
                    if ($tb.attr('onkeydown')) {
                        var okd = $tb.attr('onkeydown').toString();
                        if (okd.indexOf('onSingleLineTextBoxKeyDown') > -1)
                            $tb.attr('onkeydown', okd.replace('onSingleLineTextBoxKeyDown(event);', ''));
                    }
                });
            }

            function setQuickFindButtonTabIndex() {

                var lastTabIndex = 0;
                //get the dbpanel last tab index
                $("[id*=dbpQuickFind] [tabindex]").each(function () {
                    lastTabIndex = Math.max($(this).attr("tabindex"), lastTabIndex);
                });

                var $qfButtons = $(".qfbuttons input");
                if ($qfButtons && $qfButtons.length) {
                    $qfButtons.each(function () {
                        $(this).attr("tabindex", ++lastTabIndex);

                    });
                }
            }

            function windowFocus() {
                setTimeout(function () {
                    // when multiple reports
                    var crIndex = 5;
                    while (crIndex-- >= 0) {
                        var _pdfPopup = window["_pdfPopup" + crIndex];
                        if (_pdfPopup) {
                            _pdfPopup.focus();
                            window["_pdfPopup" + crIndex] = null;
                        }
                    }

                    if (_winPopup) {
                        _winPopup.focus();
                        _winPopup = false;
                    }
                }, 150);
            }

            // Make sure dashboard is focused on QF and scroll is at top on load
            $("[id$='dbpQuickFind'] :input:visible:first").focus();
            setTimeout(function () { window.scrollTo(0, 0); });
        });

        function ClickPrintTransferRpt() {
            window.setTimeout(function () {
                document.getElementById('<%= btnPrintTransferRpt.ClientID %>').click();
            }, 150);
        }


        function ShowIOAnalystContacts() {
            $("[id='IOAnalystContactInfo']:eq(1)").dialog("destroy").remove();
            $("#IOAnalystContactInfo").dialog({
                title: "Analyst Contact Information",
                resizable: false,
                modal: true,
                width: "auto",
                buttons: {
                    Close: function () {
                        $(this).dialog("close");
                    }
                },
                open: function (event, ui) {
                    $(this).parent().appendTo("form");
                    $(".ui-dialog-titlebar-close", ui.dialog | ui).hide();
                },
                close: function () {
                    $(this).dialog("close");               
                }
            });
            return false;
        }

        function ShowFilterPopUp() {      
            $("[id='divFilter']:eq(1)").dialog("destroy").remove();
            $("#divFilter").dialog({
                title: "Filter Options",
                resizable: false,
                modal: true,
                width: "auto",
                buttons: {
                    "Clear Filter": function () {
                        $("[id*='fbSection']").setCode("");
                        $("[id*='fbLabCode']").setCode("");
                        $("[id*='fbAnalyst']").setCode("");

                        var $mpinput = $("[id$='mpGroupCode_multipick']");
                        if ($mpinput) {
                            $("input[id$='" + $mpinput.attr("id") + "_sectionlisttxt']").val("");
                        }
                        $("[id$='hdnDoNotFocusOnQF']").val("T");
                        document.getElementById('<%= btnPostBack.ClientID %>').click();
                    },
                    Close: function () {
                        $("[id$='hdnDoNotFocusOnQF']").val("T");
                        $("[id*=mpGroupCode]").find(".sectionpopup").css('display', 'none');
                        $(this).dialog("close");

                    }
                },
                open: function (event, ui) {
                    $(this).parent().appendTo("form");
                    $(".ui-dialog-titlebar-close", ui.dialog | ui).hide();

                    $(".popup-ffb").css("position", "absolute").css("top", "0px").css("left", "0px");
                    $("[id$='hdnDoNotFocusOnQF']").val("T");
                },
                close: function () {
                    $(this).dialog("close");
                },
                dragStart: function () {
                    $(".ffb:visible").hide();
                    $("[id*=mpGroupCode]").find(".sectionpopup").css('display', 'none');
                }
            });
            return false;
        }


        function setFilterFlexboxPosition($divID) {
            $("[id*='fbSection']").css("z-index", $divID.closest('.ui-dialog').css("z-index") + 1);
            $("[id*='fbLabCode']").css("z-index", $divID.closest('.ui-dialog').css("z-index") + 1);
            $("[id*='fbAnalyst']").css("z-index", $divID.closest('.ui-dialog').css("z-index") + 1);
            $("[id$='mpGroupCode_multipick_sectionpopup']").css("z-index", $divID.closest('.ui-dialog').css("z-index") + 1);
            $("[id$='hdnDoNotFocusOnQF']").val("T");
        }


        function SetLinkButtonText(isempty) {
            var obj = document.getElementById("<%= linkFilter.ClientID %>");

            if (obj) {
                if (isempty == "T") {
                    obj.innerText = "Unfiltered";
                    obj.href = '#';
                }
                else {
                    obj.innerText = "Filtered";
                }
            }
        }

        function ShowPrintAllContainersPopup() {
            /// <summary>Shows the print all containers popup.</summary>

            var dialog = document.getElementById("dlgPrintAllContainers");

            $(dialog).dialog({
                autoOpen: true,
                modal: true,
                resizable: false,
                closeOnEscape: false,
                dialogClass: "no-close",
                title: "Print All Containers",
                width: 550,
                //height: 500,
                buttons: {
                    Print: function () {
                        var btnPrintContainers = document.querySelector("[id$=btnPrintContainers]");

                        if (btnPrintContainers != null)
                            btnPrintContainers.click();
                    },
                    Close: function () {
                        $(this).dialog("close");
                    }
                },
                open: function () {
                    $(this).parent().appendTo("form"); // needed for submit type buttons

                    const btnPrint = $(dialog)
                        .siblings(".ui-dialog-buttonpane")
                        .find(".ui-dialog-buttonset button")
                        .eq(0);
                    const grdPrintAllContainers = $(dialog).find(".plcdbgridEmpty");
                    const hasRecords = grdPrintAllContainers.length == 0;

                    if (hasRecords) {
                        // Initially disable the `Print` button
                        btnPrint
                            .attr("disabled", false)
                            .removeClass("ui-state-disabled");

                        // Initially focus to `Print` button
                        btnPrint.focus();
                    }
                    else {
                        // Initially enable the `Print` button
                        btnPrint
                            .attr("disabled", true)
                            .addClass("ui-state-disabled");

                        // Initially focus to `Close` button
                        btnPrint.siblings().eq(0).focus()
                    }

                    // Add events to each checkbox
                    handleCheckStateForPrintContainers();
                },
                close: function () {
                    $(this).dialog().remove();
                }
            });
        }

        function ShowBtnShowPrintWorksheets() {
            /// <summary>Shows the print worksheets button.</summary>

            const btnShowPrintWorksheets = document.getElementById("btnShowPrintWorksheets");
            if (!btnShowPrintWorksheets)
                return;

            btnShowPrintWorksheets.classList.remove("hidden");
        }

        function ShowAssignmentWorksheetsPopup() {
            /// <summary>Shows the worksheets popup.</summary>

            let dialog = document.querySelectorAll("#dlgPrintAllWorksheets");
            let dbgrid = null;

            // Fix for partial postback causes the element to duplicate
            if (dialog.length > 1) {
                $(dialog[1]).remove();
            }

            dialog = dialog[0];

            $(dialog).dialog({
                autoOpen: true,
                modal: true,
                resizable: false,
                closeOnEscape: false,
                dialogClass: "no-close",
                title: "Print All Worksheets",
                width: 750,
                //height: 500,
                buttons: {
                    Print: function () {
                        const dataKeys = dbgrid.row.getData.call(dbgrid);

                        if (dataKeys.length > 0) {
                            const worksheets = dataKeys.map(function (data) {
                                    return "AS:" + data.primaryKey + "|" + data.selectedKeys.join(",");
                                }, []).join(";");
    
                            CallPLCWebMethod("PLC_WebOCX",
                                "PRINTWORKSHEETS",
                                {
                                    "examkey": "-1",
                                    "templatekey": worksheets,
                                    "worksheetid": "-1",
                                    "source": "CREATE-PRINT",
                                    "worksheetsource": ""
                                },
                                true);
                        }
                    },
                    Close: function () {
                        $(this).dialog("close");
                    }
                },
                open: function () {
                    const btnPrint = $(dialog)
                        .siblings(".ui-dialog-buttonpane")
                        .find(".ui-dialog-buttonset button")
                        .eq(0);

                    // Initially disable the `Print` button
                    btnPrint
                        .attr("disabled", true)
                        .addClass("ui-state-disabled");

                    // Initially focus to `Close` button
                    btnPrint.siblings().eq(0).focus();
                    
                    // Create parent grid
                    dbgrid = new DBGrid({
                        wrapper: "#grdWorksheet",
                        gridName: "MYASSIGNMENTS",
                        dataKeyNames: "EXAM_KEY,CASE_KEY,SECTION,PRIORITY",
                        hiddenFields: "0,1,2", // field index
                        additionalCriteria: "",
                        allowSorting: true,
                        allowPaging: true,
                        pageSize: 5,
                        pagerCount: 10,
                        cancelSelectOnClick: true,
                        width: 700,
                        height: 500,
                        customFields: [
                            { type: "toggle", hideColumn: true, autoOpen: true }
                        ],
                        events: {
                            toggle: function (row, contentRow, sender, event) {
                                const grid = this;

                                const td = contentRow.querySelector("td");
                                const div = td.querySelector("div")
                                    ? td.querySelector("div")
                                    : document.createElement("div");
                                let dbgridChild = div.querySelector("table");
                                
                                // Create child grid
                                if (!dbgridChild) {
                                    dbgridChild = new DBGrid({
                                        wrapper: div,
                                        gridName: "MYASSIGNMENTS_WORKSHEETS",
                                        dataKeyNames: "TEMPLATE_KEY",
                                        hiddenFields: "TEMPLATE_KEY",
                                        additionalCriteria: "SECTION = '" + row.dataset["SECTION"] + "'",
                                        cancelSelectOnClick: true,
                                        allowPaging: true,
                                        pageSize: 5,
                                        customFields: [
                                            { type: "checkbox" }
                                        ],
                                        events: {
                                            columnCreated: function (sender) {
                                                const parentGrid = dbgrid;
                                                const dataHolder = parentGrid.table.tHead.children[0];
                                                const row = sender.parent(".toggle-row");
                                                const dataFromParent = dataHolder.dataset["DATA"]
                                                    ? JSON.parse(dataHolder.dataset["DATA"])
                                                    : [];

                                                dataFromParent.forEach(function (data) {
                                                    const primaryKeyValue = data.primaryKey;
                                                    const selectedKeys = data.selectedKeys;

                                                    const rowPrimaryKey = row.dataset[parentGrid.options.primaryKeyName];

                                                    if (rowPrimaryKey === primaryKeyValue) {

                                                        const table = row.querySelector(".tableception");
                                                        const checkboxAll = table.tHead.querySelector("th input[type=checkbox]");

                                                        // Save to checkbox header
                                                        checkboxAll.dataset["SELECTED_KEYS"] = selectedKeys;
                                                    }
                                                });
                                            },
                                            checkAll: function (sender, event) {
                                                const grid = this;
                                                const parentGrid = dbgrid;
                                                const toggleRow = sender.parent("tr.toggle-row");

                                                const dataHolder = parentGrid.table.tHead.children[0];
                                                const data = dataHolder.dataset["DATA"]
                                                    ? JSON.parse(dataHolder.dataset["DATA"])
                                                    : [];
                                                const selectedKeys = grid.row.getSelectedKeys.call(grid);
                                                const primaryKeyValue = toggleRow.dataset[parentGrid.options.primaryKeyName];
                                                const rowDataFoundIndex = data.findIndex(function (rowData) {
                                                    return rowData.primaryKey === primaryKeyValue;
                                                });

                                                if (rowDataFoundIndex > -1) {
                                                    data.splice(rowDataFoundIndex, 1);
                                                }

                                                if (selectedKeys.length) {
                                                    data.push({
                                                        primaryKey: primaryKeyValue,
                                                        selectedKeys: selectedKeys
                                                    });
                                                }

                                                dataHolder.dataset["DATA"] = JSON.stringify(data);

                                                if (grid.options.allowPaging && data.length > 0) {
                                                    btnPrint.attr("disabled", false).removeClass("ui-state-disabled");
                                                }
                                                else {
                                                    btnPrint.attr("disabled", true).addClass("ui-state-disabled");
                                                }
                                            },
                                            check: function (checkboxAll, row, sender, event) {
                                                const grid = this;
                                                const parentGrid = dbgrid;
                                                const toggleRow = checkboxAll.parent("tr.toggle-row");

                                                const dataHolder = parentGrid.table.tHead.children[0];
                                                const data = dataHolder.dataset["DATA"]
                                                    ? JSON.parse(dataHolder.dataset["DATA"])
                                                    : [];
                                                const selectedKeys = grid.row.getSelectedKeys.call(grid);
                                                const primaryKeyValue = toggleRow.dataset[parentGrid.options.primaryKeyName];
                                                const rowDataFoundIndex = data.findIndex(function (rowData) {
                                                    return rowData.primaryKey === primaryKeyValue;
                                                });

                                                if (rowDataFoundIndex > -1) {
                                                    data.splice(rowDataFoundIndex, 1);
                                                }

                                                if (selectedKeys.length) {
                                                    data.push({
                                                        primaryKey: primaryKeyValue,
                                                        selectedKeys: selectedKeys
                                                    });
                                                }

                                                dataHolder.dataset["DATA"] = JSON.stringify(data);

                                                if (data.length > 0) {
                                                    btnPrint.attr("disabled", false).removeClass("ui-state-disabled");
                                                }
                                                else {
                                                    btnPrint.attr("disabled", true).addClass("ui-state-disabled");
                                                }
                                            }
                                        }
                                    });

                                    dbgridChild.table.classList.add("tableception"); // header index issue fix
                                    dbgridChild.table.width = "100%";
                                    div.appendChild(dbgridChild.table);
                                    div.classList.add("toggleWrapper");
                                    div.classList.add("dbgridjs"); // manually add this for child grids
                                    td.appendChild(div);
                                }
                            },
                        }
                    });
                }
            });
        }
        
        function handleCheckStateForPrintContainers() {
            /// <summary>Handles the `Print` button state (enable/disable) based on selected checkboxes.</summary>

            const dialog = document.getElementById("dlgPrintAllContainers");
            const dlgPrintAllContainersContent = document.getElementById("dlgPrintAllContainers-content");
            const cbSelectState = $(dlgPrintAllContainersContent).find("input[type=hidden][id$=cbSelect_state]");

            if (!cbSelectState.length) {
                return;
            }

            const hasChecked = cbSelectState.val().indexOf("1") > -1;
            const btnPrint = $(dialog)
                .siblings(".ui-dialog-buttonpane")
                .find(".ui-dialog-buttonset button")
                .eq(0);

            if (!hasChecked) {
                if (!hasChecked) {
                    btnPrint
                        .attr("disabled", true)
                        .addClass("ui-state-disabled");
                }
                else {
                    btnPrint
                        .attr("disabled", false)
                        .removeClass("ui-state-disabled");
                }
            }

            const checkboxes = $(dlgPrintAllContainersContent).find("input[type=checkbox]");

            checkboxes.each(function () {
                const fnClick = this.onclick;

                const evalPrintButton = function (prevClickEvent) {
                    const hasChecked = $(dlgPrintAllContainersContent).find("input[type=hidden][id$=cbSelect_state]").val().indexOf("1") > -1;

                    if (!hasChecked) {
                        btnPrint
                            .attr("disabled", true)
                            .addClass("ui-state-disabled");
                    }
                    else {
                        btnPrint
                            .attr("disabled", false)
                            .removeClass("ui-state-disabled");
                    }

                    if (typeof prevClickEvent === "function") {
                        prevClickEvent.call(this); // Need to rebind "this"
                    }
                }

                this.addEventListener("click", evalPrintButton.bind(this, fnClick));
            });
        }

        function sortDashboardPanels(dashboardPanelList) {
            setTimeout(function () {
                var $dashboardPanel = $("div.dashboard-panel");
                var $dashboardPanelItems = $dashboardPanel.find(".dashboard-panel-item");

                for (var item in dashboardPanelList) {
                    $dashboardPanelItems.each(function () {
                        var $pnlDiv = $(this);
                        if ($pnlDiv.attr("id").indexOf(dashboardPanelList[item]) > -1)
                            $dashboardPanel.append($pnlDiv);
                    });
                }

                showDashboardPanels();
            }, 500);
        }

        function showDashboardPanels() {
            var $dashboardPanel = $("div.dashboard-panel");
            $dashboardPanel.css("visibility", "visible");
        }

        function setFieldFocus() {
            setTimeout(function () {
                var $actionAlert = $("#dvActionAlert.ui-dialog-content");
                if ($actionAlert.length == 0) {
                    var isDefaultScrollPosition = $("[id$='hdnIsDefaultScrollPosition']").val();
                    var doNotFocusOnQF = $("[id$='hdnDoNotFocusOnQF']").val();
                    if (isDefaultScrollPosition == "T") {
                        selectQuickFindFirstField();
                        window.scrollTo(0, 0);
                    }
                    else {
                        var top = window.pageYOffset || document.documentElement.scrollTop;
                        var left = window.pageXOffset || document.documentElement.scrollLeft;
                        if (left == 0 && top == 0 && doNotFocusOnQF != "T")
                            selectQuickFindFirstField();
                        window.scrollTo(left, top);
                    }
                }
                $("[id$='hdnDoNotFocusOnQF']").val("F");

            }, 500);
        }
    </script>
</asp:Content>

