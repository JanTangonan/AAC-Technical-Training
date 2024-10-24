<%@ Page Language="C#" MasterPageFile="~/CASEFILE.master" AutoEventWireup="True" CodeBehind="TAB6Assignments.aspx.cs" Inherits="BEASTiLIMS.TAB6Assignments" Title="Assignment Information" %>

<%@ Register Assembly="PLCCONTROLS" Namespace="PLCCONTROLS" TagPrefix="PLC" %>
<%@ Register Src="~/PLCWebCommon/CodeHead.ascx" TagName="CodeHead" TagPrefix="uc1" %>
<%@ Register Src="~/PLCWebCommon/PLCPageHead.ascx" TagName="PLCPageHead" TagPrefix="uc2" %>
<%@ Register Src="~/PLCWebCommon/PLCWebControl.ascx" TagName="PLCWebControl" TagPrefix="uc1" %>
<%@ Register Src="~/PLCWebCommon/PLCReviewPlan.ascx" TagName="ReviewPlan" TagPrefix="uc3" %>
<%@ Register Src="~/PLCWebCommon/PLCVerifySignature.ascx" TagName="VerifySignature" TagPrefix="uc3" %>
<%@ Register Src="~/PLCWebCommon/PLCAssignRoute.ascx" TagName="AssignRoute" TagPrefix="uc3" %>
<%@ Register Src="~/PLCWebCommon/PLCItemTask.ascx" TagName="PLCItemTask" TagPrefix="uc3" %>
<%@ Register Src="~/PLCWebCommon/PLCDialog.ascx" TagName="Dialog" TagPrefix="PLC" %>
<%@ Register Src="~/PLCWebCommon/PLCAssignWorklist.ascx" TagName="AssignWorklist" TagPrefix="PLC" %>
<%@ Register Src="~/PLCWebCommon/AutoNotesPacket.ascx" TagName="AutoNotesPacket" TagPrefix="PLC" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link type="text/css" rel="stylesheet" href="BEASTiLIMS.css" />
    <style type="text/css">
        .style1
        {
            width: 100%;
        }

        .tdAssignTask
        {
            overflow: visible !important;
        }
        
        /*#qmsdocspopup {width:550px;}*/
        #qmsdocspopup fieldset {margin:1em 1em;}
        /*#qmsdocbtns {text-align:center; padding:2em 0;}*/
        #qmsinfoheader {position:relative;}
        #qmsinfoheader {height:2em; font-weight:bold; font-size:larger;}
        #qmslblsection {position:absolute; left:1em;}
        #qmslblasof {position:absolute; right:4em;}

        /* datagrid styles */
        #bigviewTable .datagrid {
            margin: 0 0.5em 2em 0.5em;
        }
        
        #bigviewHeading .datagrid {
            margin: 0 0.5em 0 0.5em;
            border-bottom: 0;
        }
        
        #bigviewTable .datagrid th.linkcase-heading {
            text-align: left; width: auto; padding-left: 1em;
        }
        
        .datagrid {
            border: 1px solid #111111;
            table-layout: fixed;
        }

        .datagrid thead tr {
            /*background-color: lightgray;*/
            background-color: #5d7b9d;
            color: #FFFFFF;
            /*border: 1px solid #111111;*/
        }
        
        .datagrid tr {
            background-color: White;
        }

        .datagrid tr.odd {
            /*background-color: #EEEEEE;*/
            background-color: #f7f6f3;
        }

        .datagrid tfoot {
            display: none;
        }

        .datagrid {
            border-collapse: collapse ;
        }

        .datagrid th {
            vertical-align: middle;
            font-weight: bold;
            font-size: smaller;
            text-align: center;
         }

        .datagrid td {
            border-left:solid 1px #dddddd;
            border-right:solid 1px #dddddd;
            text-align: left;
            vertical-align: middle ;
            padding-left: 0.5em;
            -moz-box-sizing: border-box;
            box-sizing: border-box;
            overflow:hidden;
        }

        .datagrid .sel {width: 2em; padding: 0; text-align: center; border-left: 0px;}
        .datagrid .itemnum {width: 4em;}
        .datagrid .trackingnum {width: 10em;}
        .datagrid .thirdpartybarcode {width: 6em;}
        .datagrid .deptitemnum {width: 6em;}
        .datagrid .packtype {width: 8em;}
        .datagrid .itemdesc {width: 20em; padding:0;}
        .datagrid .itemdesc input {width: 98%; margin: 0; color: gray;}
        .datagrid .itemtype {width: 12em;}
        .datagrid .qty {width: 4em;}
        .datagrid .vouchernum {width: 6em;}
        .datagrid .servicerequest {width: 10em;}
        .datagrid .itemsource {width: 10em; padding: 0;}
        .datagrid td.itemsource select {width: 100%; height: 100%; margin: 0; text-align: center; vertical-align: middle;}
        .datagrid .delivery {width: 5em; padding: 0; border-right: 0px;}
        .datagrid td.delivery select {width: 100%; height: 100%; margin: 0; text-align: center; vertical-align: middle;}        
        .datagrid .ecn {display: none;}
        .datagrid .sectionnumber {width: 5em;}
        .datagrid .task {width: 4em; padding: 0;}
        .datagrid td.task input {width: 100%; margin: 0;}
        
        .datagrid td.task input {
            border: 1px solid #777;
            background-color: buttonface;
            padding-bottom: 2px;
            padding-top: 3px;
        }
        
        .datagrid .examresult { width: 10em; }
        .datagrid td.examresult 
        {
            text-overflow: ellipsis;
            white-space: nowrap;
            display: block;
            cursor: pointer;
            user-select: none;
            -webkit-user-select: none;
            -ms-user-select: none;
        }

        /* End datagrid styles */        

        .linkedcaseinfo .label {display: block; width: 15em; float: left;}
        .linkedcaseinfo .value {display: block; width: 20em; float: left;}
        .linkedcaseinfo div {overflow: auto; width: 100%;}

        #bigviewTable {
            overflow: auto; 
            height: 500px;
            border-bottom: 1px solid gray;
            border-top: 1px solid gray;
            margin: 0;
            padding: 0;
        }

        #bigviewBody {
            margin-bottom: 1.5em;
            /*Width: 80em; */
        }
        
        #bigviewbuttonpanel {
            margin: 1em 0;
            overflow: auto;
        }

        #bigviewbuttonpanel .buttonpanelsection {
            padding: 0 2em;
            float: left;
        }
        
        #bigviewbuttonpanel .actionbuttonset {
            margin: 0 0 0 4em;
        }
        
        #bigviewbuttonpanel .actionbuttonset input {
            width: 7em;
            padding: 0 0.5em;
        }

        #bigviewbuttonpanel .buttonpanelsection span {
            padding: 0 1em 0 0;
        }
        
        #globalitemsource-container {
            display: inline;
        }
        
        #globalitemsource-container select {
            width: 11em; text-align: center;
        }

        #searchLinkedCase {
            padding: 2em;
            margin: 0;
        }

        .linkcasefloatrow .searchinput .label {
            width: 8em;
            display: block;
        }

        .searchinput .actionbuttonset {
            margin: 10px 0;
            padding: 0;
        }
        
        .searchinput .actionbuttonset input {
            width: 7em;
            padding: 0 0.5em;
        }

        .linkcasefloatrow {
            overflow: auto;
            width: 70em;
        }

        .linkcasefloatrow .searchinput, .linkcasefloatrow .linkcaseinfo {
            float: left;
        }

        .linkcasefloatrow .searchinput {
            width: 20em;
        }

        .linkcasefloatrow .linkcaseinfo {
            width: 35em;
        }

        #linkcaseinfo div {
            width: 30em;
            overflow: auto;
        }

        #linkcaseinfo .label {
            display: block;
            float: left;
            width: 10em;
            margin: 0;
            padding: 0;
        }

        #linkcaseinfo .value {
            display: block;
            float: left;
            width: 20em;
            margin: 0;
            padding: 0;
        }

        #searchLinkCaseButtonPanel {
            margin: 1.5em 2em;
            padding: 0;
            text-align: right;
        }
        
        #searchLinkCaseButtonPanel input {
            width: 7em;
            padding: 0 0.5em;
        }

        #linkcaseitems {
            overflow: auto;
            height: 20em;
            border: 1px dotted #dddddd;
        }

        #linkcaseitems .datagrid .itemdesc {width: 100%; padding: 0 0 0 0.2em;}
        #linkcaseitems .datagrid {width: 100%;}

        /* Yes/No popup dialog styles */
        body div.yesnopopup {
            width: auto;
            font-size: 1em;
            padding: 1em 2em;
        }
        
        body div.yesnopopup p {
            padding: 0;
            margin: 0;
            text-align: left;
        }
        
        body div.yesnopopup .promptline {
            /*width: 30em;*/
            margin: 0 0 0.8em;
            padding: 0;
        }
        
        body div.yesnopopup ul {
            width: 300px;
            margin: 0;
            padding: 0.8em 0;
        }
        
        body div.yesnopopup li {
            margin: 0 0 0.8em 1.5em;
        }
        
        .localFoldersStyle{
            padding: 1px;
            border-top: solid 1px white;
            border-left: solid 1px white;
            border-right: solid 1px white;
            background: url(Images/Folder_o.gif) no-repeat;
            padding-left: 15px;
        }
        .localFilesStyle{
            padding: 1px;
            border-top: solid 1px white;
            border-left: solid 1px white;
            border-right: solid 1px white;
            background: url(Images/file.gif) no-repeat;
            padding-left: 15px;
        }
        .highlighSelect
        {
            background-color:#3399FF;
            font-weight:bold;
            color:White;
        }

        /* tooltip via title attr */
        .info-tooltip {
            display: inline;
            position: relative;
        }

        .info-tooltip.show:after {
            background: #111;
            background: rgba(0,0,0,.8);
            border-radius: .5em;
            bottom: 1.35em;
            color: #fff;
            content: attr(tooltip);
            display: block;
            left: -6em;
            padding: .3em 1em;
            position: absolute;
            text-shadow: 0 1px 0 #000;
            white-space: nowrap;
            z-index: 98;
        }

        .info-tooltip.show:before {
            border: solid;
            border-color: #111 transparent;
            border-color: rgba(0,0,0,.8) transparent;
            border-width: .4em .4em 0 .4em;
            bottom: 1em;
            content: "";
            display: block;
            left: 2em;
            position: absolute;
            z-index: 99;
        }
        /* end tooltip */
                

        /* task close/cancel on sign */

        #open-task-msg-container
        {
            text-align: center;
        }

        .task-table
        {
            display: block;
            max-height:150px;
            overflow-y: auto; 
        }
        
        .task-table th, .task-table td
        {
            padding: 2px 11px 0 11px;
        }
 
        .task-table tr:nth-child(even) td
        {
            background-color: rgba(207, 224, 252, 0.2);
        }

        /* end task close/cancel on sign */


        .blt 
        {
            margin-left: 15px;
        }
        
        .route-field-required { color: Red; }

        .item-list
        {
            margin: 10px 0 0 0;
            height: 95px;
            overflow-y: auto;
        }

        .item-list ul
        {
            padding-left: 15px;
            margin: 0px;
        }

        .modalBackground.mpeBackgroundElem {
            width: 100vw !important;
            height: 100vh !important;
            position: fixed !important;
        }

        /* #region Routing */
        .dbgrid-routing-history.dbgridblk {
            float: none;
            width: 100%;
            position: relative;
        }
        /* #endregion Routing */
    </style>

    <script type="text/javascript" src="PLCWebCommon/JavaScripts/mustache.js"></script>
    <script type="text/javascript" src="PLCWebCommon/JavaScripts/json2.js"></script>
    <script type="text/javascript" src="JavaScripts/BigViewItems.js?rev=<%= PLCSession.CurrentRev %>"></script>

    <script type="text/javascript">    
        function OnOKAssignmentChangedError() {
            // Reload the page to refresh the assignments grid.
            location.reload(true);
        }

        function checkHeaderNames(cbx, id) {
            var itemkeys = $("[id*='pnlItems']").siblings("[id*='hfItemKeys']").val().split("|");
            var itemvalues = $("[id*='pnlItems']").siblings("[id*='hfItemValues']").val().split("|");
            var namekeys = $("[id*='pnlItems']").siblings("[id*='hfNameKeys']").val().split(",");
            var namevalues = $("[id*='pnlItems']").siblings("[id*='hfNameValues']").val().split(",");

            $("#" + cbx.id).closest("table").find("input[type=checkbox][id*='" + id.replace("_All", "") + "']:enabled:checked").each(function () {
                var ecn = $(this).closest("td").children("[id*='hdnItemNameKey']").val();
                var itemindex = $.inArray(ecn, itemkeys);
                if (itemindex >= 0) {
                    var names = itemvalues[itemindex].split(",");
                    for (var i = 0; i < names.length; i++) {
                        var nameindex = $.inArray(names[i], namekeys);
                        if (nameindex >= 0) {
                            $("#" + namevalues[nameindex]).prop('checked', true);
                            saveCheckedDetail($("#" + namevalues[nameindex]).get(0), "cbSelect");
                        }
                    }
                }
            });
        }

        function checkDetailNames(cbx) {
            if (cbx.checked) {
                var ecn = $("#" + cbx.id).closest("td").children("[id*='hdnItemNameKey']").val();
                var itemkeys = $("[id*='pnlItems']").siblings("[id*='hfItemKeys']").val().split("|");
                var itemvalues = $("[id*='pnlItems']").siblings("[id*='hfItemValues']").val().split("|");
                var namekeys = $("[id*='pnlItems']").siblings("[id*='hfNameKeys']").val().split(",");
                var namevalues = $("[id*='pnlItems']").siblings("[id*='hfNameValues']").val().split(",");

                var itemindex = $.inArray(ecn, itemkeys);
                if (itemindex >= 0) {
                    var names = itemvalues[itemindex].split(",");
                    for (var i = 0; i < names.length; i++) {
                        var nameindex = $.inArray(names[i], namekeys);
                        if (nameindex >= 0) {
                            $("#" + namevalues[nameindex]).prop('checked', true);
                            saveCheckedDetail($("#" + namevalues[nameindex]).get(0), "cbSelect");
                        }
                    }
                }
            }
        }

        function selectTab(idx) {
            $("[id*='tbItems']").attr("class", idx == 0 ? "active" : "");
            $("[id*='tbNames']").attr("class", idx == 1 ? "active" : "");
            $("[id*='divTabItems']").css("display", idx == 0 ? "" : "none");
            $("[id*='divTabNames']").css("display", idx == 1 ? "" : "none");
            return false;
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cp" runat="server" >
    <asp:Panel ID="pnlBigView" runat="server" CssClass="modalPopup pnlBigView" Height="" style="display: none; overflow-x: auto;">
        <asp:Panel ID="pnlBigViewCaption" runat="server" CssClass="caption" Width="100%" style="width: 100%;">Assignment Items</asp:Panel>
        <div id="bigviewBody">
            <div id="bigviewHeading">
            </div>
            <div id="bigviewTable">
            </div>
            <div id="bigviewbuttonpanel">
                <div class="buttonpanelsection">
                    <span>Item Source</span>
                    <div id="globalitemsource-container">
                        <select></select>
                    </div>
                </div>
                <div class="buttonpanelsection actionbuttonset">
                    <input id="btnShowSearchLinkCase" type="button" value="Link" onclick="BigViewShowSearchLinkCaseClick();" class="default" />
                    <PLC:PLCButton ID="btnPullList" runat="server" Text="Print" PromptCode="TAB6Assignments.btnPullList" Width="90px" OnClick="bnPullList_Click" Enabled="false" />
                    <PLC:PLCButton ID="btnBigViewResults" runat="server" Text="Load Results" PromptCode="TAB6Assignments.btnBigViewResults" Width="100px" OnClientClick="BigViewLoadResults(); return false;" />
                    <input id="btnBigViewEdit" type="button" value="Edit" onclick="BigViewEditClick();" class="default" />
                    <input id="btnBigViewSave" type="button" value="Save" onclick="BigViewSaveClick();" class="default" />
                    <input id="btnBigViewCancel" type="button" value="Close" onclick="BigViewCancelClick();" class="default" />
                </div>
            </div>
        </div>

        <asp:Button ID="btnMpeBigView" runat="server" style="display: none;" />
        <ajaxToolkit:ModalPopupExtender ID="mpeBigView" BehaviorID="mpeBigView" runat="server" BackgroundCssClass="modalBackground" 
            PopupControlID="pnlBigView" PopupDragHandleControlID="pnlBigViewCaption" TargetControlID="btnMpeBigView" 
            OkControlID="btnMpeBigView">
        </ajaxToolkit:ModalPopupExtender>
    </asp:Panel>
    
    <asp:Button ID="bnCommentDummy" runat="server" style="display: none;" />
    <ajaxToolkit:ModalPopupExtender ID="mpeComment" BehaviorID="mpeComment" runat="server" BackgroundCssClass="modalBackground" 
        PopupControlID="pnlComment" PopupDragHandleControlID="pnlCommentCaption" TargetControlID="bnCommentDummy" >
    </ajaxToolkit:ModalPopupExtender>
    <asp:Panel ID="pnlComment" runat="server" CssClass="modalPopup" Width="350px" style="display: none;">
        <asp:Panel ID="pnlCommentCaption" runat="server" CssClass="caption" Width="100%" >Case Update Reason</asp:Panel>
        <div style="width:100%; margin-top:10px;" >
            <span style="width:20%; vertical-align:top; text-align:center; float:left;" ><img src="<%= ResolveUrl("~/Images/msginfo.jpg") %>" alt="" /></span>
            <span style="width:75%;" >
                <label style="font-family:Arial; font-size:small; width:100%; " >Please enter the reason for your changes</label>
                <textarea id="txtCommentReason" rows="8" cols="29" style="margin-top:10px;" ></textarea>
            </span>
        </div>
        <div style="width:100%; height:35px; margin-top:10px; text-align:center;" >
            <button id="bnCommentSave" type="button" style="width:70px; height:25px;" >Save</button>
            <button id="bnCommentCancel" type="button" onclick="$find('mpeComment').hide();" style="width:70px; height:25px; margin-left:3px;" >Cancel</button>
        </div> 
    </asp:Panel>

    <uc2:PLCPageHead ID="phTab6Assignments" runat="server" include="utility,codemultipick,jquery-ui" />
    <uc1:PLCWebControl ID="WebOCX" runat="server" />
    <asp:PlaceHolder ID="phPLCWebOCX" runat="server"></asp:PlaceHolder>
    
    <PLC:PLCSessionVars ID="PLCSessionVars1" runat="server" />
    <asp:PlaceHolder ID="plhMessageBox" runat="server"></asp:PlaceHolder>
    <asp:PlaceHolder ID="plhMsgBoxComments" runat="server"></asp:PlaceHolder>
    <PLC:Dialog ID="dlgMessage" runat="server" Width="300px" OnConfirmClick="dlgMessage_ConfirmClick" OnCancelClick="dlgMessage_CancelClick" />
    <PLC:Dialog ID="dlgMessageResetAssign" runat="server" OnCancelClick="dlgMessageResetAssign_Click" />
    <PLC:Dialog ID="dlgAssignmentMessage" runat="server" OnConfirmClick="dlgAssignmentMessage_ConfirmClick" />
    <PLC:PLCDataSource ID="qryLabExamGrid" runat="server">
    </PLC:PLCDataSource>
    <PLC:Dialog ID="dlgAlertMessage" runat="server" />
    <div class="dbbox">
        <div class="dbgridblk">
            <PLC:PLCDBGrid ID="GridView1" runat="server" DataKeyNames="EXAM_KEY,SEQUENCE,SECTION" OnSelectedIndexChanged="GridView1_SelectedIndexChanged"
                AllowSorting="True" AllowPaging="false" OnRowDataBound="GridView1_RowDataBound" OnSorted="GridView1_Sorted" PLCGridWidth="100%" Height="200">
            </PLC:PLCDBGrid>
        </div>
        <div class="dbgridbtnblk">        
            <asp:Button ID="bnNotes" runat="server" Text="Notes" Width="100%" OnClick="bnNotes_Click" />
            <PLC:PLCButton ID="bnReports" runat="server" Text="Report" PromptCode="TAB6Assignments.bnReports" Width="100%" OnClick="bnReports_Click" />
            <PLC:PLCButton ID="bnVerifyReports" runat="server" Text="Verification" PromptCode="TAB6Assignments.bnVerifyReports" Width="100%" OnClick="bnVerifyReports_Click" Visible="false" />
            <PLC:PLCButton ID="bnRemoveBlind" runat="server" Text="Remove Blind" PromptCode="TAB6Assignments.bnRemoveBlind" Width="100%" OnClick="bnRemoveBlind_Click" Visible="false" />
            <PLC:PLCButton ID="bnSign" runat="server" Text="Sign" PromptCode="TAB6Assignments.bnSign" Width="100%" OnClick="bnSign_Click" />
            <PLC:PLCButton ID="bnReview" runat="server" Text="Review" PromptCode="TAB6Assignments.bnReview" Width="100%" OnClick="bnReview_Click" />
            <PLC:PLCButton ID="bnCodisReview" runat="server" Text="Codis Review" PromptCode="TAB6Assignments.bnCodisReview" Width="100%" OnClick="bnCodisReview_Click" Visible="False"/>
            <PLC:PLCButton ID="bnApprove" runat="server" Text="Approve" PromptCode="TAB6Assignments.bnApprove" Width="100%" OnClick="bnApprove_Click" />
            <PLC:PLCButton ID="bnAdmClose" runat="server" Text="Admin Close" PromptCode="TAB6Assignments.bnAdmClose" Width="100%" OnClick="bnAdmClose_Click" />
            <PLC:PLCButton ID="bnRollback" runat="server" Text="Rollback" PromptCode="TAB6Assignments.bnRollback" Width="100%" OnClick="bnRollback_Click" />
            <PLC:PLCButton ID="bnUnlock" runat="server" Text="Unlock" PromptCode="TAB6Assignments.bnUnlock" Width="100%" OnClick="bnUnlock_Click" Visible="False" />
            <PLC:PLCButton ID="bnSRPrint" runat="server" Text="Service Request" PromptCode="TAB6Assignments.bnSRPrint" Width="100%" OnClick="bnSRPrint_Click" OnLoad="bnSRPrint_Load"/>
            <PLC:PLCButton ID="bnSOP" runat="server" Text="SOP" PromptCode="TAB6Assignments.bnSOP" Width="100%" OnClick="bnSOP_Click" />
            <PLC:PLCButton ID="bnQMS" runat="server" Text="QMS" PromptCode="TAB6Assignments.bnQMS" Width="100%" OnClick="bnQMS_Click" />
            <PLC:PLCButton ID="bnReviewPlan" runat="server" Text="Review Plan" PromptCode="TAB6Assignments.bnReviewPlan" Width="100%" OnClick="bnReviewPlan_Click" />
            <PLC:PLCButton ID="btnDNAWorksheets" runat="server" Text="DNA Worksheets" PromptCode="TAB6Assignments.btnDNAWorksheets" OnClientClick="GoToDNAWorksheet(); return false;" />
            <a href="DNACaseWorksheets.aspx" id="lnkDna" style="display: none;"></a>
        </div>
        <div class="clear"></div>
    </div>
    <div class="dbbox">
        <div class="dbpaneltabs">
            <asp:Panel ID="pnlLinkButtons" runat="server">
                <PLC:PLCLinkButton ID="bnDetails" runat="server" OnClick="bnDetails_Click" Text="Details" PromptCode="TAB6Assignments.bnDetails" class="dbpaneltablink" />
                <PLC:PLCLinkButton ID="bnItems" runat="server" OnClick="bnItems_Click" Text="Items" PromptCode="TAB6Assignments.bnItems" Visible="false" class="dbpaneltablink" />
                <PLC:PLCLinkButton ID="bnTasks" runat="server" OnClick="bnTasks_Click" Enabled="true" Text="Tasks" PromptCode="TAB6Assignments.bnTasks" class="dbpaneltablink" />
                <PLC:PLCLinkButton ID="bnRouting" runat="server" OnClick="bnRouting_Click" Text="Routing" PromptCode="TAB6Assignments.bnRouting" class="dbpaneltablink" />
                <PLC:PLCLinkButton ID="bnData" runat="server" OnClick="bnData_Click" Text="Data" PromptCode="TAB6Assignments.bnData" class="dbpaneltablink" />
                <PLC:PLCLinkButton ID="bnDNADates" runat="server" OnClick="bnDNADates_Click" Text="DNA Dates" PromptCode="TAB6Assignments.bnDNADates" Visible="false" class="dbpaneltablink" />
                <PLC:PLCLinkButton ID="bnAnalysts" runat="server" OnClick="bnAnalysts_Click" Text="Additional Analyst(s)" PromptCode="TAB6Assignments.bnAnalysts" Visible="false" class="dbpaneltablink" />
                <PLC:PLCLinkButton ID="bnHistory" runat="server" OnClick="bnHistory_Click" Text="History" PromptCode="TAB6Assignments.bnHistory" Visible="false" class="dbpaneltablink" />
            </asp:Panel>
        </div>
        <asp:MultiView ID="MultiView1" runat="server" OnActiveViewChanged="MultiView1_ActiveViewChanged">
            <asp:View ID="View1" runat="server">
                <div class="dbpanelblk flexboxscroll">
                    <div id="dvPLCLockStatus" runat="server" visible="false" >
                        <asp:Label ID="lPLCLockStatus" runat="server" ForeColor="Red"></asp:Label>
                    </div>
                    <table class="style1">
                        <tr>
                            <td valign="top">
                                <PLC:DBPanel ID="PLCDBPanel1" runat="server" onplcdbpanelbuttonclick="PLCDBPanel1_PLCDBPanelButtonClick"
                                    OnPLCDBPanelSetDefaultRecord="PLCDBPanel1_SetDefaultRecord"
                                    OnPLCDBPanelGetNewRecord="PLCDBPanel1_PLCDBPanelGetNewRecord" OnPLCDBPanelCodeHeadChanged="PLCDBPanel1_PLCDBPanelCodeHeadChanged"
                                    OnPreRender="PLCDBPanel1_PreRender"
                                    PLCDataTable="TV_LABEXAM" PLCPanelName="ASSIGNMENTSTAB" PLCShowAddButton="True"
                                    PLCShowDeleteButton="True" PLCShowEditButtons="True" PLCWhereClause="Where 0 = 1"
                                    PLCAuditCode="17" PLCAuditSubCode="1" PLCDeleteAuditCode="18" PLCDeleteAuditSubCode="1" PLCAttachPopupTo="body" >
                                </PLC:DBPanel>
                            </td>
                            <td valign="top">
                                <div style="width: 400px;">
                                    <ul class="tabs">
                                        <li id="tbItems" runat="server" class="active" >
                                            <PLC:PLCLinkButton ID="btnItems" runat="server" Text="Items" PromptCode="TAB6Assignments.btnItems" OnClientClick="return selectTab(0);" /></li>
                                        <li id="tbNames" runat="server">
                                            <PLC:PLCLinkButton ID="btnNames" runat="server" Text="Names" PromptCode="TAB6Assignments.btnNames" OnClientClick="return selectTab(1);" /></li> 
                                    </ul>
                                    <div class="tab_container">
                                        <div id="divTabs" runat="server" >
                                            <div ID="divTabItems" runat="server" >
                                                <asp:HiddenField ID="hdnIsROnlyAccess" runat="server" Value="" />
                                                <asp:HiddenField ID="hdnItemScrollPos" runat="server" Value="" />
                                                <asp:HiddenField ID="hfItemKeys" runat="server" Value="" />
                                                <asp:HiddenField ID="hfItemValues" runat="server" Value="" />
                                                <asp:HiddenField ID="hfNameKeys" runat="server" Value="" />
                                                <asp:HiddenField ID="hfNameValues" runat="server" Value="" />

                                                <asp:Panel ID="pnlItems" runat="server" ScrollBars="Both" Width="" Height="150px" style="margin: 5px;">
                                                    <asp:GridView ID="gItems" runat="server" AllowSorting="false" AutoGenerateColumns="false" DataKeyNames="ECN" OnRowCreated="gItems_RowCreated"
                                                        ForeColor="#333333" GridLines="Vertical" CellPadding="4" HeaderStyle-HorizontalAlign="Left" HeaderStyle-VerticalAlign="Middle" Width="100%">
                                                        <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                                                        <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
                                                        <Columns>
                                                            <asp:TemplateField HeaderText="Select">
                                                                <HeaderTemplate>
                                                                    <asp:CheckBox ID="cbSELECT_All" runat="server" Width="40" onclick="selectHeader(this, 'cbSELECT_All'); checkHeaderNames(this, 'cbSELECT_All');" ></asp:CheckBox></HeaderTemplate>
                                                                <ItemTemplate>
                                                                    <asp:HiddenField ID="hdnItemNameKey" runat="server" Value='<%# Eval("ECN").ToString() %>' />
                                                                    <asp:CheckBox ID="cbSELECT" runat="server" Width="40" onclick="selectDetail(this, 'cbSELECT'); checkDetailNames(this);"
                                                                        Checked='<%# (DataBinder.Eval(Container, "DataItem.SELECTED") == "T" ? true:false) %>'></asp:CheckBox></ItemTemplate>
                                                            </asp:TemplateField>
                                                            <asp:TemplateField HeaderText="Item #">
                                                                <ItemTemplate>
                                                                    <asp:TextBox ID="ITEMNUM" runat="server" Text='<%# DataBinder.Eval(Container, "DataItem.ITEMNUM") %>'
                                                                        Width="40" ReadOnly="True"> </asp:TextBox></ItemTemplate>
                                                            </asp:TemplateField>
                                                            <asp:TemplateField HeaderText="Property Receipt" Visible="false">
                                                                <ItemTemplate>
                                                                    <asp:TextBox ID="TRACKINGNUM" runat="server" Text='<%# DataBinder.Eval(Container, "DataItem.TRACKINGNUM") %>'
                                                                        Width="100" ReadOnly="True"> </asp:TextBox></ItemTemplate>
                                                            </asp:TemplateField>
                                                            <asp:TemplateField HeaderText="Dept Item #" Visible="false">
                                                                <ItemTemplate>
                                                                    <asp:TextBox ID="DEPTITEMNUM" runat="server" Text='<%# DataBinder.Eval(Container, "DataItem.DEPTITEMNUM") %>'
                                                                        Width="100" ReadOnly="True"> </asp:TextBox></ItemTemplate>
                                                            </asp:TemplateField>
                                                            <asp:TemplateField HeaderText="Pack Type" Visible="false">
                                                                <ItemTemplate>
                                                                    <asp:TextBox ID="PACKTYPE" runat="server" Text='<%# DataBinder.Eval(Container, "DataItem.PACKTYPE") %>'
                                                                        Width="100" ReadOnly="True"> </asp:TextBox></ItemTemplate>
                                                            </asp:TemplateField>
                                                            <asp:TemplateField HeaderText="Item Type">
                                                                <ItemTemplate>
                                                                    <asp:TextBox ID="ITEMTYPE" runat="server" Text='<%# DataBinder.Eval(Container, "DataItem.ITEMTYPE") %>'
                                                                        Width="100" ReadOnly="True"> </asp:TextBox></ItemTemplate>
                                                            </asp:TemplateField>
                                                            <asp:TemplateField HeaderText="Description / Location">
                                                                <ItemTemplate>
                                                                    <asp:TextBox ID="ITEMDESC" runat="server" Text='<%# DataBinder.Eval(Container, "DataItem.ITEMDESC") %>'
                                                                        Width="150" TextMode="SingleLine" ReadOnly="true"> </asp:TextBox></ItemTemplate>
                                                            </asp:TemplateField>
                                                            <asp:TemplateField HeaderText="Quantity">
                                                                <ItemTemplate>
                                                                    <asp:TextBox ID="QUANTITY" runat="server" Text='<%# DataBinder.Eval(Container, "DataItem.QUANTITY") %>'
                                                                        Width="50" ReadOnly="true"> </asp:TextBox></ItemTemplate>
                                                            </asp:TemplateField>                                                        
                                                            <asp:TemplateField HeaderText="Voucher#">
                                                                <ItemTemplate>
                                                                    <asp:TextBox ID="VOUCHERNUM" runat="server" Text='<%# DataBinder.Eval(Container, "DataItem.VOUCHERNUM") %>'
                                                                        Width="60" ReadOnly="True"> </asp:TextBox></ItemTemplate>
                                                            </asp:TemplateField>
                                                            <asp:TemplateField HeaderText="Service Request">
                                                                <ItemTemplate>
                                                                    <asp:TextBox ID="SERVICEREQUEST" runat="server" Text='<%# DataBinder.Eval(Container, "DataItem.SERVICEREQUEST") %>'
                                                                        Width="150" Style="text-transform: uppercase;" ReadOnly="True"> </asp:TextBox>
                                                                </ItemTemplate>
                                                            </asp:TemplateField>
                                                            <asp:TemplateField HeaderText="Task">
                                                                <ItemTemplate>
                                                                    <!-- <PLC:PLCButton ID="ADDTASK" runat="server" Text="Task" PromptCode="TAB6Assignments.ADDTASK" OnClientClick='<%# "uc_LoadItemTasks(" + Eval("ECN").ToString() + ", " + Eval("EXAM_KEY").ToString() + "); return false;" %>'
                                                                        Visible='<%# Eval("SELECTED").ToString() == "T" ? true : false %>' /> -->
                                                                </ItemTemplate>
                                                            </asp:TemplateField>
                                                        </Columns>
                                                        <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
                                                        <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
                                                        <HeaderStyle BackColor="#5D7B9D" ForeColor="White" Font-Size="11px" />
                                                        <EditRowStyle BackColor="#999999" />
                                                        <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
                                                    </asp:GridView>                            
                                                </asp:Panel>
                                                <PLC:PLCButton ID="bnBigView" runat="server" Text="Big View" PromptCode="TAB6Assignments.bnBigView"
                                                    OnClientClick="BigViewClientClick(); return false;// bnBigView_WaitBeforePostback(this);" 
                                                    Width="100px" style="margin-top: 2px;" />
                                                <PLC:PLCButton ID="btnWorklist" runat="server" Text="Worklist" PromptCode="TAB6Assignments.btnWorklist" OnClick="btnWorklist_Click" />
                                            </div>
                                            <div id="divTabNames" runat="server" style="display:none;" >
                                                <PLC:PLCDBGrid ID="gvNameItem" runat="server" DataKeyNames="NAME_KEY" PLCGridName="ASSIGNMENT_NAME_LIST" Width="390px" PLCGridWidth="390px" Height="150px"
                                                    FirstColumnCheckbox="true" CancelPostbackOnClick="true" OnRowDataBound="gvNameItem_RowDataBound" OnSorted="gvNameItem_Sorted" />
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </td>
                        </tr>
                    </table>
                </div>
                <div class="dbbtnpanel">
                    <PLC:PLCButtonPanel ID="PLCButtonPanel1" runat="server" PLCDisplayBottomBorder="False"
                        PLCDisplayTopBorder="False" PLCShowAddButton="True" PLCShowDeleteButton="True" PLCCustomButtons="Search Results...,Manage,Split,Revisions,Rev Note,RecordUnlock"
                        PLCShowEditButtons="True" PLCTargetControlID="PLCDBPanel1" OnPLCButtonClick="PLCButtonPanel1_PLCButtonClick" PLCCustomButtonFixedWidth="60"
                        PLCTargetDBGridID="GridView1">
                    </PLC:PLCButtonPanel>
                </div>
            </asp:View>
            <asp:View ID="View2" runat="server">
                <div class="dbpanelblk">
                    <PLC:PLCDBCheckList ID="PLCDBCheckList1" runat="server" Width="100%">
                        &#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;</PLC:PLCDBCheckList>
                    </div>
                    <div class="dbbtnpanel">
                    <PLC:PLCButtonPanel ID="PLCButtonPanel2" runat="server" PLCDisplayBottomBorder="False"
                        PLCDisplayTopBorder="False" PLCShowEditButtons="True" PLCTargetControlID="PLCDBCheckList1"
                        Width="100%" OnPLCButtonClick="PLCButtonPanel2_PLCButtonClick">
                        &#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;</PLC:PLCButtonPanel>
                </div>
            </asp:View>
            <asp:View ID="View3" runat="server">
                <div class="dbpanelblk">
                    <PLC:PLCDBGrid ID="gTasks" runat="server" DataKeyNames="TASK_ID" Width="100%" AllowPaging="True" AllowSorting="True">
                    </PLC:PLCDBGrid>
                </div>
            </asp:View>
            <asp:View ID="View4" runat="server">
                <div class="dbpanelblk" style="height: 350px;">
                    <asp:Panel ID="panelRoute" runat="server">
                        <asp:HiddenField ID="hdnROUTETO" runat="server" />
                        <asp:HiddenField ID="hdnROUTEBY" runat="server" />
                        <table>
                            <tr>
                                <td>
                                    Route To:
                                </td>
                                <td>
                                    <asp:Label ID="lblRouteTo" runat="server" Text="" Width="200px"></asp:Label>
                                </td>
                                <td>
                                    Route Code:
                                </td>
                                <td>
                                    <asp:Label ID="lblRouteCode" runat="server" Text="" Width="200px"></asp:Label>
                                </td>
                                <td>
                                    <PLC:PLCButton ID="btnRoute" runat="server" OnClick="btnRoute_Click" Text="Route" PromptCode="TAB6Assignments.btnRoute" Width="100%" />
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    Route By:
                                </td>
                                <td>
                                    <asp:Label ID="lblRouteBy" runat="server" Text=""></asp:Label>
                                </td>
                                <td>
                                    Route Date:
                                </td>
                                <td>
                                    <asp:Label ID="lblRouteDate" runat="server" Text=""></asp:Label>
                                </td>
                                <td>
                                    <PLC:PLCButton ID="btnRoutingReply" runat="server" Text="Reply" PromptCode="TAB6Assignments.btnRoutingReply" OnClick="btnRoutingReply_Click" Width="100%" />
                                </td>
                            </tr>
                            <tr>
                                <td valign="top">
                                    Message:
                                </td>
                                <td colspan="3">
                                    <asp:TextBox ID="tbxRoutingMessage" ReadOnly="true" TextMode="MultiLine" runat="server" Width="400px" Height="100px"></asp:TextBox>
                                </td>
                                <td valign="top" style="width: 130px">
                                    <PLC:PLCButton ID="btnRoutingCancel" runat="server" Text="Cancel" PromptCode="TAB6Assignments.btnRoutingCancel" OnClientClick="return confirm('Clear this Routing?');" OnClick="btnRoutingCancel_Click" Width="100%" /><br />
                                    <PLC:PLCButton ID="btnRoutingHistory" runat="server" Text="History" PromptCode="TAB6Assignments.btnRoutingHistory" Width="100%" OnClick="btnRoutingHistory_Click" /><br />
                                    <PLC:PLCButton ID="bnAssignRoute" runat="server" Text="Assign and Route" PromptCode="TAB6Assignments.bnAssignRoute" Visible="false" Width="100%" OnClick="bnAssignRoute_Click" />
                                </td>
                            </tr>
                        </table>
                    </asp:Panel>
                    <div class="dbgrid-routing-history dbgridblk">
                        <asp:UpdatePanel runat="server">
                            <ContentTemplate>
                                <PLC:PLCDBGrid ID="dbgRoutingHistory" runat="server"
                                    PLCGridName="ROUTE_HISTORY" PLCGridWidth="100%" Width="100%"
                                    Height="150" CancelPostbackOnClick="true"
                                    AllowSorting="true" AllowPaging="true" PageSize="5"
                                    EmptyDataText="No routing history."
                                    OnRowCreated="dbgRoutingHistory_RowCreated">
                                </PLC:PLCDBGrid>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </div>
                <asp:Button runat="server" ID="btnRoutingOK" buttonAction="submitButton" OnClick="btnRouteOK_Click"
                    Style="display: none" />
                <asp:Panel ID="pnlRouting" CssClass="modalPopup" runat="server" Style="display: none">
                    <asp:Panel ID="Panel1" runat="server">
                        <asp:Panel runat="Server" ID="panelDragHandle" Width="400px">
                            <table>
                                <tr>
                                    <td colspan="2" align="center">
                                        <asp:Panel ID="Panel2" runat="server" Height="20px" CssClass="caption">
                                            Route This Assignment</asp:Panel>
                                    </td>
                                </tr>
                                <tr>
                                    <td align="left" width="100px">
                                        Route To:
                                        <span class="route-field-required">*</span>
                                    </td>
                                    <td align="left" width="250px">
                                        <asp:PlaceHolder ID="phUserID" runat="server"></asp:PlaceHolder>
                                    </td>
                                </tr>
                                <tr>
                                    <td align="left" width="100px">
                                        Route Code:
                                        <span class="route-field-required">*</span>
                                    </td>
                                    <td align="left" width="250px">
                                        <asp:PlaceHolder ID="phRoutingCodes" runat="server"></asp:PlaceHolder>
                                    </td>
                                </tr>
                                <tr>
                                    <td align="left" valign="top" width="100px">
                                        Message:
                                    </td>
                                    <td align="left" width="300px">
                                        <asp:TextBox ID="tbxRouteMessage" runat="server" TextMode="MultiLine" Width="300px"
                                            Height="100px"></asp:TextBox>
                                    </td>
                                </tr>
                                <tr style="display:none;">
                                    <td>
                                    </td>
                                    <td align="left">
                                        <PLC:PLCCheckBox ID="chkbxSendEmail" runat="server" Enabled="false" Text="Send Email" PromptCode="TAB6Assignments.chkbxSendEmail" />
                                    </td>
                                </tr>
                            </table>
                            <asp:Label ID="lblRouteRequired" ForeColor="Red" runat="server" Visible="false">
                                Please fill in required fields<br />
                            </asp:Label>
                            <br />
                            <div id="dvBTN" runat="server" align="center">
                                <PLC:PLCButton ID="btnRouteOK" runat="server" Text="OK" PromptCode="TAB6Assignments.btnRouteOK" Width="100px" />
                                <PLC:PLCButton ID="btnRouteCancel" runat="server" Text="Cancel" PromptCode="TAB6Assignments.btnRouteCancel" Width="100px" />
                                <br />
                            </div>
                            <br />
                        </asp:Panel>
                    </asp:Panel>
                </asp:Panel>
                <ajaxToolkit:ModalPopupExtender ID="mpeRouting" runat="server" TargetControlID="btnRouteOK"
                    PopupControlID="pnlRouting" OkControlID="btnRouteOK" OnOkScript="onbtnRouteOK()"
                    CancelControlID="btnRouteCancel" BackgroundCssClass="modalBackground mpeBackgroundElem"
                    RepositionMode="RepositionOnWindowResize" Y="300">
                </ajaxToolkit:ModalPopupExtender>
            </asp:View>
            <asp:View ID="View5" runat="server">
                <div class="dbpanelblk">
                <div style="float: left;">
                    <asp:ListBox ID="lbRawData" runat="server" Width="700px" Rows="6" OnSelectedIndexChanged="lbRawData_SelectedIndexChanged" AutoPostBack="true">
                    </asp:ListBox>
                </div>
                <div style="float: right; width: 100px; text-align: right; padding: 0 10px;">
                    <PLC:PLCButton ID="bnDataAdd" runat="server" Text="Add" PromptCode="TAB6Assignments.bnDataAdd" Width="80px" OnClick="bnDataAdd_Click"/><br />
                    <PLC:PLCButton ID="bnDataRemove" runat="server" Text="Remove" PromptCode="TAB6Assignments.bnDataRemove" Width="80px" OnClick="bnDataRemove_Click"/><br />
                    <PLC:PLCButton ID="bnDataEdit" runat="server" Text="Edit" PromptCode="TAB6Assignments.bnDataEdit" Width="80px" OnClick="bnDataEdit_Click"/><br />
                    <%--for Open Button, original function is OnClientClick="clickAttachViewerLink();"--%>
                    <PLC:PLCButton ID="bnDataOpen" runat="server" Text="Open" PromptCode="TAB6Assignments.bnDataOpen" Width="80px" OnClick="btnOpenFileLink_Click"/><br />
                </div>
                </div>
            </asp:View>
            <asp:View ID="View6" runat="server">
                <div class="dbpanelblk">
                    <PLC:DBPanel ID="dbpDNADates" runat="server" PLCPanelName="DNADATES" PLCDataTable="TV_LABEXAM" PLCWhereClause="Where 0 = 1" Width="400px" PLCAllowBlankRecordSave="true"
                    PLCAuditCode="17" PLCAuditSubCode="1" PLCDeleteAuditCode="18" PLCDeleteAuditSubCode="1">
                    </PLC:DBPanel>
                </div>
                <div class="dbbtnpanel">
                    <PLC:PLCButtonPanel ID="bpDNADates" runat="server" PLCDisplayTopBorder="False" PLCDisplayBottomBorder="False" 
                        PLCShowEditButtons="True" PLCTargetControlID="dbpDNADates" OnPLCButtonClick="bpDNADates_PLCButtonClick"></PLC:PLCButtonPanel>
                </div>
            </asp:View>
            <asp:View ID="View7" runat="server">
                <div class="dbpanelblk">
                <div style="float: left; padding-right: 10px;">
                    <PLC:PLCButton ID="btnAddAnalyst" runat="server" Text="Add Analyst..." PromptCode="TAB6Assignments.btnAddAnalyst" Width="120" OnClick="btnAddAnalyst_Click" /><br />
                    <PLC:PLCButton ID="btnRemoveAnalyst" runat="server" Text="Remove Analyst" PromptCode="TAB6Assignments.btnRemoveAnalyst" Width="120" OnClick="btnRemoveAnalyst_Click" /><br />
                </div>
                <div style="padding-left: 10px;">
                    <asp:ListBox ID="lstAnalysts" runat="server" Width="300px" Rows="6"></asp:ListBox>
                </div>
                </div>
            </asp:View>
            <asp:View ID="View8" runat="server">
                <div class="dbpanelblk">
                    <div class="dbgrid-assignment-history dbgridblk">
                        <asp:UpdatePanel runat="server">
                            <ContentTemplate>
                                <PLC:PLCDBGrid ID="dbgAssignmentHistory" runat="server"
                                    PLCGridName="ASSIGNMENT_HISTORY" PLCGridWidth="100%" Width="100%"
                                    Height="150" CancelPostbackOnClick="true"
                                    AllowSorting="true" AllowPaging="true" PageSize="5"
                                    EmptyDataText="No assignment history.">
                                </PLC:PLCDBGrid>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </div>
            </asp:View>
        </asp:MultiView>
    </div>

    <div id="dialog-assignsearchresults" style="padding: 10px; display:none;">
        <div id="dialog-assignsearchresults-content" style="margin-top: 10px;">
            <asp:UpdatePanel ID="upAssignSearch" runat="server">
                <ContentTemplate>
                    <PLC:Dialog ID="dlgMessageSearchResults" runat="server" />
                    <PLC:PLCDBGrid ID="gvAssignSearch" runat="server" PLCGridName="ASSIGNSEARCH" AllowSorting="True" AllowPaging="True"
                        DataKeyNames="CASE_KEY,EXAM_KEY" OnSelectedIndexChanged="gvAssignSearch_SelectedIndexChanged" PLCGridWidth="100%" Height="380"
                        OnRowCreated="gvAssignSearch_RowCreated" OnRowCommand="gvAssignSearch_RowCommand"
                        EmptyDataText="Go to Assignment Search and select criteria then click 'Search' button." RecordsPerPage="15">
                    </PLC:PLCDBGrid>
                </ContentTemplate>
            </asp:UpdatePanel>               
        </div>
    </div>

    <asp:Button ID="btnDataSaveOK" runat="server" Style="display: none;"  buttonAction="submitButton" OnCommand="bnDataSave_Click"/>
    <ajaxToolkit:ModalPopupExtender ID="mpeDataGetFile" runat="server" BackgroundCssClass="modalBackground"
        PopupControlID="pnlDataGetFile" PopupDragHandleControlID="pnlDataGetFileCaption"
        TargetControlID="btnDataSave" DropShadow="false" OkControlID="btnDataSave" CancelControlID="btnDataCancel"
        Drag="true" OnOkScript="onbtnDataSaveOK()">
    </ajaxToolkit:ModalPopupExtender>
    <asp:Panel ID="pnlDataGetFile" runat="server" Style="display: none;">
        <asp:Panel ID="pnlDataGetFilePopup" runat="server" CssClass="modalPopup" Width="400" DefaultButton="btnDataSave">
            <asp:Panel ID="pnlDataGetFileCaption" runat="server" Height="20px" CssClass="caption">
                <PLC:PLCLabel ID="lblDataGetFileTitle" runat="server" Text="Select a File or Folder to link" PromptCode="TAB6Assignments.lblDataGetFileTitle" />
            </asp:Panel>
            <div style="padding: 5px 0 5px 25px">
                <PLC:PLCLabel ID="lblDataGetFilePath" runat="server" Text="Path: " PromptCode="TAB6Assignments.lblDataGetFilePath" AssociatedControlID="txtDataGetFileDesc" Width="80px" style="display:inline-block;" />
                <asp:FileUpload runat="server" id="fupDataGetFilePath" Width="250px" /> <br />
                <PLC:PLCLabel ID="lblDataGetFileDesc" runat="server" Text="Description: " PromptCode="TAB6Assignments.lblDataGetFileDesc" AssociatedControlID="txtDataGetFileDesc" Width="80px" style="display:inline-block;" />
                <asp:TextBox runat="server" ID="txtDataGetFileDesc" Width="250px" />
            </div>
            <div align="center" style="padding: 10px;">
                <PLC:PLCButton ID="btnDataSave" runat="server" Text="Add Link" PromptCode="TAB6Assignments.btnDataSave" Width="100px" />
                <PLC:PLCButton ID="btnDataCancel" runat="server" Text="Cancel" PromptCode="TAB6Assignments.btnDataCancel" Width="100px" />
            </div>
        </asp:Panel>
    </asp:Panel>

    <asp:Button ID="btnAdminClose" runat="server" Style="display: none;" />
    <ajaxToolkit:ModalPopupExtender ID="mpeAdminClose" runat="server" BackgroundCssClass="modalBackground"
        PopupControlID="pnlAdminClose" PopupDragHandleControlID="pnlAdminCloseCaption"
        TargetControlID="btnAdminClose" DropShadow="false" OkControlID="btnOK" CancelControlID="btnNoAdminClose"
        Drag="true" RepositionMode="RepositionOnWindowResize" >
    </ajaxToolkit:ModalPopupExtender>
    <asp:Panel ID="pnlAdminClose" runat="server" CssClass="modalPopup" Style="display: none;" Width="400">
        <asp:Panel ID="pnlAdminCloseCaption" runat="server" Height="20px" CssClass="caption">
            Admin Close Confirmation</asp:Panel>
        <asp:HiddenField ID="hdnExamKey" runat="server" />
        <asp:HiddenField ID="hdnSection" runat="server" />
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
                <table id="tblDetails" cellpadding="0" cellspacing="0">
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
                    <asp:TextBox ID="txtComments" runat="server" TextMode="MultiLine" Rows="4" Width="340"></asp:TextBox>
                </div>
                <div>
                    <asp:Label ID="lblError" runat="server" Text="" Visible="false" ForeColor="Red"></asp:Label>
                </div>
            </asp:PlaceHolder>
        </div>
        <div align="center" style="padding: 10px;">
            <PLC:PLCButton ID="btnYesAdminClose" runat="server" Text="Yes" PromptCode="TAB6Assignments.btnYesAdminClose" Width="80px" OnClick="btnYesAdminClose_Click" ValidationGroup="AdminClose" />
            <PLC:PLCButton ID="btnNoAdminClose" runat="server" Text="No" PromptCode="TAB6Assignments.btnNoAdminClose" Width="80px" OnClientClick="LockUnlockRecord('U');" />
        </div>
        <asp:Button ID="btnOK" runat="server" Style="display: none;" />
    </asp:Panel>

    <asp:Button ID="btnSA" runat="server" Style="display: none;" />
    <ajaxToolkit:ModalPopupExtender ID="mpeSA" runat="server" BackgroundCssClass="modalBackground"
        PopupControlID="pnlSA" TargetControlID="btnSA" BehaviorID="mpeSA" DropShadow="false" Drag="true">
    </ajaxToolkit:ModalPopupExtender>
    <asp:Panel ID="pnlSA" runat="server" CssClass="modalPopup" Style="display: none;" Width="740px" HorizontalAlign="Center">
        <div id="sacaption" class="caption" style="height: 20px; width: 100%;">Select Analyst</div>
            <asp:UpdatePanel ID="upAnalystList" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <div id="sadiv" style="margin: 10px; width: 95%; height: 400px; text-align: left; overflow: auto;">
                        <div style="display: block; margin: 3px; width: 640px;">
                            <div>
                                <span style="float: left; padding: 5 3 0 0;">Section:</span>
                                <PLC:FlexBox ID="fbSection" runat="server" TableName="TV_EXAMCODE" Width="220" Style="float: left;" 
                                    AutoPostBack="true" OnValueChanged="fbSection_ValueChanged"></PLC:FlexBox>
                                <span style="float: right;">Find Name:
                                    <asp:TextBox ID="txtFindAnalyst" runat="server" OnTextChanged="txtFindAnalyst_TextChanged" AutoPostBack="true"></asp:TextBox>
                                    <PLC:PLCButton ID="btnFind" runat="server" Text="Find" PromptCode="TAB6Assignments.btnFind" />
                                 </span>
                            </div>
                            <div id="sagrdanalysts" style="padding: 5 0; clear: both;">
                                <PLC:PLCGridView ID="grdSA" runat="server" AutoGenerateColumns="false" EmptyDataText="No analysts found!">
                                    <Columns>
                                        <asp:BoundField DataField="Name" HeaderText="Name" ItemStyle-Width="150" />
                                        <asp:BoundField DataField="Code" HeaderText="Code" ItemStyle-Width="100" />
                                        <asp:BoundField DataField="Review Reports" HeaderText="Review Reports" ItemStyle-Width="120" />
                                        <asp:BoundField DataField="Write Reports" HeaderText="Write Reports" ItemStyle-Width="120" />
                                        <asp:BoundField DataField="Approve Reports" HeaderText="Approve Reports" ItemStyle-Width="120" />
                                    </Columns>
                                </PLC:PLCGridView>
                            </div>
                        </div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
            <div align="center" style="padding: 15px;">
                <PLC:PLCButton ID="btnOKSA" runat="server" Text="Submit" PromptCode="TAB6Assignments.btnOKSA" OnClick="btnOKSA_Click" />
                <PLC:PLCButton ID="btnCancelSA" runat="server" Text="Cancel" PromptCode="TAB6Assignments.btnCancelSA" OnClick="btnCancelSA_OkClick" />
            </div>
    </asp:Panel>

    <PLC:MessageBox ID="mbEAnalysts" runat="server" PanelCSSClass="modalPopup" CaptionCSSClass="caption" PanelBackgroundCSSClass="modalBackground" 
        MessageType="Error" />
    <PLC:MessageBox ID="mbCAnalysts" runat="server" PanelCSSClass="modalPopup" CaptionCSSClass="caption" PanelBackgroundCSSClass="modalBackground" 
        MessageType="Confirmation" OnOkClick="mbCAnalysts_OkClick" />
    <PLC:MessageBox ID="mbNoAnalyst" runat="server" PanelCSSClass="modalPopup" CaptionCSSClass="caption" PanelBackgroundCSSClass="modalBackground" 
        MessageType="Error" Caption="Add Analyst" Message="Please assign the primary analyst first." />
    <PLC:MessageBox ID="mbNotAllowed" runat="server" PanelCSSClass="modalPopup" CaptionCSSClass="caption" PanelBackgroundCSSClass="modalBackground" 
        MessageType="Error" Caption="Add Analyst" Message="You are not allowed to change this assignment." />
    <PLC:MessageBox ID="mbConfirmCloseTasksOnSign" runat="server" PanelCSSClass="modalPopup" CaptionCSSClass="caption" PanelBackgroundCSSClass="modalBackground" 
        MessageType="Confirmation" OnOkClick="mbConfirmCloseTasksOnSign_OkClick" Width="620"/>


    <asp:HiddenField ID="hdnTaskListFocused" runat="server" />
    <asp:HiddenField ID="hdnAutoSyncAnalyst" runat="server" />
    <asp:HiddenField ID="hdnReportSigned" runat="server" Value="" />
    <asp:HiddenField ID="hdnReportReconciled" runat="server" Value="" />
    <asp:HiddenField ID="hdnAssignKey" runat="server" />
    <asp:Button ID="btnLoadAssignTaskGrid" runat="server" OnClick="btnLoadAssignTaskGrid_Click" style="display:none" />
    <asp:Button ID="btnSaveAssignTask" runat="server" Style="display: none;" OnClick="btnSaveAssignTask_Click" />
    <asp:Button ID="btnSkipAssignTask" runat="server" Style="display: none;" OnClick="btnSkipAssignTask_Click" />
    <ajaxToolkit:ModalPopupExtender ID="mpeAssignTask" runat="server" BackgroundCssClass="modalBackground"
        PopupControlID="pnlAssignTask" PopupDragHandleControlID="pnlCaptionAssignTask" Drag="true"
        OnOkScript="onSaveAssignTask()" TargetControlID="btnAssignTask" DropShadow="false" OkControlID="btnAssignTask" />
    <asp:Panel ID="pnlAssignTask" runat="server" Style="display: none;">
        <asp:Panel ID="pnlPopupAssignTask" runat="server" CssClass="modalPopup" DefaultButton="btnAssignTask">
            <asp:Panel ID="pnlCaptionAssignTask" runat="server" Height="20px" CssClass="caption">
                Assign Tasks
            </asp:Panel>
            <div style="padding: 5px; width: 100%;">
                <asp:UpdatePanel ID="upAssignTask" runat="server">
                    <ContentTemplate>
                        <div id="divAssignTask" style="height: 500px; overflow: auto;">
                            <asp:GridView ID="gvAssignTask" runat="server" AutoGenerateColumns="false"
                                OnRowCreated="gvAssignTask_RowCreated" Width="780" HeaderStyle-HorizontalAlign="Center"
                                OnDataBound="gvAssignTask_DataBound">
                                <Columns>
                                    <asp:TemplateField>
                                        <ItemTemplate>
                                            <asp:HiddenField ID="hdnAssignTaskCaseKey" runat="server" Value='<%# Eval("CASE_KEY") %>' />
                                            <asp:HiddenField ID="hdnAssignTaskECN" runat="server" Value='<%# Eval("ECN") %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField HeaderText="Item Number" DataField="ITEM_NUMBER" />
                                    <asp:TemplateField>
                                        <ItemTemplate>
                                            <asp:Label ID="lblAssignTaskItemType" runat="server" Text='<%# Eval("ITEM_TYPE_DESC") %>' />
                                            <asp:HiddenField ID="hdnAssignTaskItemType" runat="server" Value='<%# Eval("ITEM_TYPE") %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Description">
                                        <ItemTemplate>
                                            <asp:TextBox ID="txtAssignTaskItemDesc" runat="server"></asp:TextBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Tasks (F7)" ItemStyle-CssClass="tdAssignTask">
                                        <ItemTemplate>
                                            <asp:PlaceHolder ID="phTaskType" runat="server"></asp:PlaceHolder>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Priority" ItemStyle-CssClass="tdAssignTask">
                                        <ItemTemplate>
                                            <PLC:FlexBox ID="fbAssignTaskPriority" runat="server" TableName="TV_PRIORITY" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
            <div class="paneWrapper">
                <div class="leftPane" style="width:260px;">
                    <PLC:PLCCheckBox ID="chkCreateWorkList" runat="server" Text="Create Work List" PromptCode="TAB6Assignments.chkCreateWorkList" Enabled="false" />
                </div>
                <div class="rightPane" style="width:260px; text-align: center;">
				    <PLC:PLCButton ID="btnAssignTask" runat="server" Text="Save Tasks" PromptCode="TAB6Assignments.btnAssignTask" Width="100px" />
                    <PLC:PLCButton ID="btnCancelSaveAssignTask" runat="server" Text="Cancel" PromptCode="TAB6Assignments.btnCancelSaveAssignTask" Width="100px" OnClick="btnCancelSaveAssignTask_Click"/>
                </div>
                <div class="rightPane" style="width:260px; text-align:right;">
                    <%-- Just comment this out, we might need it later.
                    <asp:Button ID="btnSkipSaveAssignTask" runat="server" Text="Skip Adding Task" Width="120px" />
                    --%>
                    <%-- This section was used to simulate barcode usage inthe absence of a scanning device.
                    <input type="button" id="btn1" value="Barcode Save" style="width:100px;" onclick="focusBarcode('C1IIST');" ></input>
                    <input type="button" id="btn2" value="Barcode Clear" style="width:100px;" onclick="focusBarcode('C1IICT');" ></input>
                    <input type="button" id="btn3" value="Assign 21" style="width:100px;" onclick="focusBarcode('C1TT21');" ></input>
                    <input type="button" id="btn4" value="Assign 22" style="width:100px;" onclick="focusBarcode('C1TT22');" ></input>
                    <input type="button" id="btn5" value="Assign Dummy" style="width:100px;" onclick="focusBarcode('C1TTDUMMY');" ></input>
                    --%>
                </div>
                <div class="clear"></div>
            </div>
        </asp:Panel>
    </asp:Panel>

    <PLC:MessageBox ID="mbSigInvalidate" runat="server" MessageType="Confirmation" PanelCSSClass="modalPopup" CaptionCSSClass="caption" PanelBackgroundCSSClass="modalBackground" 
        Caption="Confirm" Message="This Report has already been signed. If you make changes to this Assignment, the Signature will be invalidated. Continue?" OnCancelClick="mbSigInvalidate_CancelClick" />
    <PLC:MessageBox ID="mbARCheck" runat="server" MessageType="Confirmation" PanelCSSClass="modalPopup" Width="500"
        CaptionCSSClass="caption" PanelBackgroundCSSClass="modalBackground" Caption="Confirm" OnOkClick="mbARCheck_OkClick" />
    <PLC:MessageBox ID="mbNotesPacketRequired" runat="server" MessageType="Confirmation" PanelCSSClass="modalPopup"
        CaptionCSSClass="caption" PanelBackgroundCSSClass="modalBackground" Caption="Confirm" Message="Do you want to generate Notes before Signing?"
        OnCancelClick="mbNotesPacketRequired_CancelClick" OnOkClick="mbNotesPacketRequired_OKClick" />
    <PLC:MessageBox ID="mbCancelTasks" runat="server" MessageType="Confirmation" PanelCSSClass="modalPopup" Width="500" CaptionCSSClass="caption" 
        PanelBackgroundCSSClass="modalBackground" Caption="Confirm" OnOkClick="mbCancelTasks_OkClick" OnCancelClick="mbCancelTasks_CancelClick" />
    <PLC:MessageBox ID="mbGenericConfirm" runat="server" MessageType="Confirmation" PanelCSSClass="modalPopup" Width="600" BehaviorID="mbGenericConfirm"
        CaptionCSSClass="caption" PanelBackgroundCSSClass="modalBackground" Caption="Confirm" OnOkClick="mbGenericConfirm_OkClick" />

    <a id="lAttachViewer" href="AttachViewer.aspx" target="_blank"></a>    
    <div>    
        <asp:HiddenField ID="EXAMKEYASSN" runat="server"/>
        <asp:HiddenField ID="AESINFOASSN" runat="server"/>
        <asp:HiddenField ID="GLOBALANALYSTASSN" runat="server"/>
        <asp:HiddenField ID="SOURCEASSN" runat="server"/>
        <asp:HiddenField ID="CINFOASSN" runat="server"/>
        <asp:HiddenField ID="SERVICEURLASSN" runat="server" />
        <asp:HiddenField ID="PREVIOUSPAGEASSN" runat="server" />
        <asp:HiddenField ID="RETURNPAGEASSN" runat="server" />
        <asp:HiddenField ID="DOCUMENTKEYASSN" runat="server" />
        <asp:HiddenField ID="VERSIONKEYASSN" runat="server" />
        <asp:HiddenField ID="DATABASEASSN" runat="server" />
        <div class="style1">                
        </div>
    </div>
    
    <asp:Button ID="btnConfirmUpdate" runat="server" Text="Button" style="display: none;" OnClick="btnConfirmUpdate_Click" />
    <asp:Button ID="btnConfirmDelete" runat="server" Text="Button" style="display: none;" OnClick="btnConfirmUpdate_Click" />
    <asp:Button ID="btnConfirmUnlock" runat="server" Text="Button" style="display: none;" OnClick="btnConfirmUnlock_Click" />
    <asp:TextBox ID="txtConfirmUpdate" runat="server" TextMode="MultiLine" style="display: none;"></asp:TextBox>
    <asp:TextBox ID="txtConfirmUnlock" runat="server" TextMode="MultiLine" style="display: none;"></asp:TextBox>
    <asp:HiddenField ID="hdnConfirmUpdate" runat="server" />
    <input type="hidden" id="hdnForayURL" runat="server" />
    <input type="hidden" id="hdnForayExportParameters" runat="server" />

    <!-- QMS Docs modal popup -->
    <div id="dialog-qmsdocs" style="padding: 10px; display:none;">
        <div id="dialog-qmsdocs-content">
            <asp:UpdatePanel ID="upQMSDocs" runat="server">
                <ContentTemplate>
                    <div id="qmsdocspopup">
                        <div id="qmsinfoheader">
                            <label id="qmslblsection">Section <asp:Label ID="lblQMSSection" runat="server" style="color:Blue; font-weight:normal;"></asp:Label></label>
                            <label id="qmslblasof">As Of <asp:Label ID="lblQMSAsOf" runat="server" style="color:Blue; font-weight:normal;"></asp:Label></label>
                        </div>
                        <fieldset>
                            <legend></legend>
                            <PLC:PLCDBGrid ID="dbgQMSExamDocs" runat="server" DataKeyNames="VERSION_KEY,DOCUMENT_KEY" 
	                            AllowSorting="true" AllowPaging="false" Height="200" PLCGridWidth="100%" EmptyDataText="No documents"
                                OnSelectedIndexChanged="dbgQMSExamDocs_SelectedIndexChanged" OnSorting="dbgQMSExamDocs_Sorting">
                            </PLC:PLCDBGrid>
                        </fieldset>
                        <fieldset id="qmsrptdocssection" runat="server">
                            <legend>Documents at time of approval</legend>
                            <PLC:PLCDBGrid ID="dbgQMSRptDocs" runat="server" DataKeyNames="VERSION_KEY,DOCUMENT_KEY"
	                            AllowSorting="true" AllowPaging="false" Height="200" PLCGridWidth="100%" EmptyDataText="No documents"
                                OnSelectedIndexChanged="dbgQMSRptDocs_SelectedIndexChanged" OnSorting="dbgQMSRptDocs_Sorting">
                            </PLC:PLCDBGrid>
                        </fieldset>
                        <div id="qmsdocbtns">
	                        <PLC:PLCButton ID="btnQMSDocOpen" runat="server" Text="Open Document" PromptCode="TAB6Assignments.btnQMSDocOpen" Width="150px" OnClick="btnQMSDocOpen_Click" style="display: none;" />
                        </div>
                    </div>
                    <asp:HiddenField ID="hdnSelectedQMSGrid" runat="server" Value="" />
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>
    <div id="dialog-openForay" style="padding: 10px; display: none;"></div>

    <asp:Button ID="btnRegenReasonDummy" runat="server" Text="Button" style="display: none;" OnClick="btnRegenReasonDummy_Click" UseSubmitBehavior="false" CausesValidation="false" />
    <asp:HiddenField ID="hdnRegenReason" runat="server" Value="" />
    <div id="dialog-regenReason" style="padding: 10px; display: none;">
        <div id="dialog-regenReason-content">
            <p style="font-size: 14px; margin: 0px 0px 6px 0px;">Why are you regenerating this document?</p>
            <PLC:FlexBox ID="fbRegenReason" runat="server" TableName="TV_PROCREGN" OnValueChangedScript="updateRegenReasonMemo" />
            <asp:TextBox ID="txtRegenReason" runat="server" TextMode="MultiLine" Rows="7" style="width: 100%; margin-top: 10px;" />
        </div>
    </div>

    <asp:Button ID="btnAuditReasonDummy" runat="server" Text="Button" style="display: none;" OnClick="btnAuditReasonDummy_Click" UseSubmitBehavior="false" CausesValidation="false" />
    <asp:HiddenField ID="hdnAuditReason" runat="server" Value="" />
    <div id="dialog-auditReason" style="padding: 10px; display: none;">
        <div id="dialog-auditReason-content">
            <p style="font-size: 14px; margin: 0px 0px 6px 0px;">Please enter the reason for your changes</p>
            <asp:TextBox ID="txtAuditReason" runat="server" TextMode="MultiLine" Rows="5" style="width: 100%; margin-top: 10px;" />
        </div>
    </div>

    <PLC:AssignWorklist ID="assignWorklist" runat="server"></PLC:AssignWorklist>

    <asp:HiddenField ID="hdnQCFlag" runat="server" Value="" />
    <asp:HiddenField ID="hdnLastRowSel" runat="server" Value="" />
    <asp:HiddenField ID="hdnCurrentExamKey" runat="server" Value="" />
    <asp:HiddenField ID="hdnCurrentCaseKey" runat="server" Value="" />
    <asp:HiddenField ID="hdnCurrentSection" runat="server" Value="" />
    <asp:HiddenField ID="hdnCurrentAnalyst" runat="server" Value="" />
    <asp:HiddenField ID="hdnPullListEnabledState" runat="server" Value="" />
    <asp:HiddenField ID="hdnIsAssignmentInBatch" runat="server" Value="" />
    <asp:HiddenField ID="hdnGroupCode" runat="server" Value="" />
    <asp:HiddenField ID="hdnUOTaskAssign" runat="server" Value="" />

    <div id="divLoading" style="display:none">
        <div style="position:absolute; left: 45%; z-index: 100001; top: 45%; width:100px; text-align: center;">
            <img src="<%= ResolveUrl("~/Images/ajax-loader.gif") %>" width="50" alt="" style="margin: 0px auto;" /><br />
            <PLC:PLCLabel ID="lblProgress" runat="server" Text="Loading..." PromptCode="TAB6Assignments.lblProgress" CssClass="loading"></PLC:PLCLabel>
        </div>
    </div>

    <ajaxToolkit:ModalPopupExtender ID="mpeBrowseFile" runat="server" BackgroundCssClass="modalBackground"
        PopupControlID="pnlBrowseDirectory" TargetControlID="Button1" 
        DropShadow="false" Drag="true">
    </ajaxToolkit:ModalPopupExtender>
    <asp:Panel ID="pnlBrowseDirectory" runat="server" DefaultButton="btnDataSaveOK" CssClass="modalPopup" Width="" Height="" Style="display: none;">
        <div id="directoryPath" style="margin: 20px;">Path: </div>
        <div id="loadDrive" style="margin: 20px; overflow: auto; height: 200px; border-color:Green;">
        No Path Available
        </div>
        <div style="margin: 20px">
            <PLC:PLCLabel ID="Label2" runat="server" Text="Path:  " PromptCode="TAB6Assignments.Label2" Width="100"></PLC:PLCLabel>
            <asp:TextBox ID="txtLinkPath" runat="server" Width="280"></asp:TextBox>
            <br />
            <PLC:PLCLabel ID="Label1" runat="server" Text="Description:  " PromptCode="TAB6Assignments.Label1" Width="100"></PLC:PLCLabel>
            <asp:TextBox ID="txtLinkDescription" runat="server" Width="280"></asp:TextBox>
        </div>
        <div style="margin: 20px;">
            <center>
                <asp:HiddenField ID="hdnPath" Value="0" runat="server" />
                <asp:Button ID="Button1" runat="server" Text="Button" style="display:none;"/>
                <PLC:PLCButton ID="btnSaveLinkFile" runat="server" Text="Select File or Path" PromptCode="TAB6Assignments.btnSaveLinkFile" Width="130" OnClientClick="onbtnDataSaveOK()" />
                <PLC:PLCButton ID="btnCancelSaveLink" runat="server" Text="Cancel" PromptCode="TAB6Assignments.btnCancelSaveLink" Width="100" OnClick="btnCancelSaveLink_Click" />
            </center>
        </div>
    </asp:Panel>
    <div class="hide">
        <asp:Button ID="btnCancelSignOCX" runat="server" OnClick="btnCancelSignOCX_Click"/>
    </div>
    <uc3:ReviewPlan ID="ReviewPlan1" runat="server" OnReviewPlanSave_Click="ReviewPlanSave_Click" />
    <uc3:VerifySignature ID="VerifySignature1" runat="server" OnSignatureOK_Click="btnOKSign_Click" />
    <uc3:AssignRoute ID="AssignRoute1" runat="server" OnAssignRouteSave_Click="AssignRouteSave_Click"
        OnClientShowing="assignRouteOpen()"/>
    <uc3:PLCItemTask ID="ItemTask1" runat="server" />
    <asp:Button ID="btnDNAProcessApprove" Text="" runat="server" OnClick="btnDNAProcessApprove_Click" style="display:none;" />
    <PLC:AutoNotesPacket ID="anpAssignment" runat="server" OnContinue="anpAssignment_Continue" OnRequeue="anpAssignment_Requeue" />
    <asp:HiddenField ID="hdnNPIntervals" runat="server" Value="" />

    <div id="mconfirm" title="" style="padding: 20px;">
        <div id="mconfirm-content" style="overflow-x: auto;"></div>
    </div>

    <script type="text/javascript">
        function saveFocusedTaskList(tasklist, hdntask) {
            document.getElementById(hdntask).value = tasklist;
        }

        function onSkipAssignTask() {
            document.getElementById("<%= btnSkipAssignTask.ClientID %>").click();
        }
        
        function onSaveAssignTask() {
            document.getElementById("<%= btnSaveAssignTask.ClientID %>").click();
        }
        
        function onbtnRouteOK() {
            document.getElementById("<%= btnRoutingOK.ClientID %>").click();
        }

        function onbtnDataSaveOK() {
            document.getElementById("<%= btnDataSaveOK.ClientID %>").click();
        }

        function clickAttachViewerLink() {
            $('#lAttachViewer').attr('href', 'AttachViewer.aspx?seq=' + $('#<%= lbRawData.ClientID %> :selected').val().split('|')[0]);
            document.getElementById("lAttachViewer").click();
        }
        
        function onbnBigViewEdit()
        {
        }
        
        function OnBigViewCancelEdit()
        {
        }
        
        function WaitBeforePostback(btn) {
            if (typeof("Page_ClientValidate") == "function" && Page_ClientValidate() == false)
                return false;
            
            btn.disabled = true;
            btn.value = "Wait...";
        }
    
        function SetGridHandlers() {
            // Only process row click event once to prevent multiple row highlights being set before gridview postback.
            var _rowwasclicked = false;

            // Bind onclick handlers to each assignment datagrid row to set row highlight immediately on clicking and erase previous row highlight.
            // This is set to provide immediate response to the user when a row is clicked.
            $("table[id$='GridView1'] tr").click(function() {
                if (!_rowwasclicked) {
                    var lastrownumtxt = $("input[id$='hdnLastRowSel']").val();
                    if (lastrownumtxt != "") {
                        var lastrownum = parseInt(lastrownumtxt, 10) + 1;
                        var $prevtr = $("table[id$='GridView1'] tr:eq(" + lastrownum + ")");
                        $prevtr.css("color", "#333333").css("background-color", "#F7F6F3");
                    }

                    $(this).css("color", "#FFFFFF").css("background-color", "#6E6E6E");

                    _rowwasclicked = true;
                }
            });
        }

        function Init() {
            // Keep session state alive indefinitely to give the user enough time to edit the document outside the browser.
            // KeepSessionAlive();

            BindBigViewControls();
            window.onunload = OnPageUnload;
        }

        $(document).keydown(function (e) {
            // Shortcut 'T' for 'Assign Tasks' popup
            if (e.which == 84 && !IsModalPopupShown()) {
                // if focus is on any input fields, do not display popup
                if ($(document.activeElement).length > 0) {
                    var $activeField = $(document.activeElement);
                    var invalidFields = "input,textarea,select";
                    // skip some fields
                    if (invalidFields.indexOf($activeField[0].nodeName.toLowerCase()) > -1)
                        return;
                }

                // if page is on browse mode, display popup
                var $grd = $('[id$="GridView1"]');
                if ($grd.length > 0) {
                    if (!$grd[0].getAttribute('disabled') && _dbPanelStatus == 'BROWSE') {
                        $('#<%= btnLoadAssignTaskGrid.ClientID %>').click();
                    } 
                }
            }
        });

        function contentPageLoad() {
            setGenericConfirmHeight();
            setBigViewFlag();
            dbpanelAttachPopupToParent("div[id$='pnlAdminClose']", "table#tblDetails");

            /* Need to set also the supplement popup if page is empty mode */
            if (typeof supplementemptymode != 'undefined') {
                var $grd = $('[id$="GridView1"]');
                supplementemptymode = $grd.length == 0 ? "T" : "";
            }

            var $txtRegenReason = $("[id$='txtRegenReason']");
            $txtRegenReason.val($("[id$='hdnRegenReason']").val());

            $("#mconfirm").dialog({
                autoOpen: false,
                modal: true,
                resizable: false,
                draggable: false,
                buttons: {
                    OK: function () {
                        $(this).dialog("close");
                    }
                }
            });
        }

        function setGenericConfirmHeight() {
            // regenerate reason popup default height is "auto", if more than 700px show scrollbar
            var modal = $find("mbGenericConfirm");
            if (modal) {
                modal.add_shown(function () {
                    var $lbl = $("span[id$='mbGenericConfirmml']");
                    $lbl.css('overflow-y', 'auto');
                    if ($lbl.height() < 700) {
                        $lbl.css('height', 'auto');
                    } else {
                        $lbl.css('height', 700);
                    }
                });
            }
        }

        var isBigViewShown = false;
        function setBigViewFlag() {
            var modal = $find("mpeBigView");
            if (modal) {
                modal.add_shown(function () {
                    isBigViewShown = true;
                });

                modal.add_hidden(function () {
                    isBigViewShown = false;
                });
            }
        }

        function GetDriveList() {
            var fso, obj, n, e, item, arr = [];
            try {
                fso = new ActiveXObject("Scripting.FileSystemObject");
            }
            catch (er) {
                alert("Could not load Drives. The ActiveX control could not be run");
                return false;
            }

            e = new Enumerator(fso.Drives);
            for (; !e.atEnd(); e.moveNext()) {
                item = e.item();
                obj = { letter: "", description: "" };

                if (item.IsReady) {
                    obj.letter = item.DriveLetter;
                    if (item.Drivetype == 3) obj.description = item.ShareName;
                    else obj.description = item.VolumeName;
                    arr[arr.length] = obj;
                }
            }
            return (arr);
        }

        function LoadDrive() {
            var drive = GetDriveList(), list = "";
            document.getElementById('<%=hdnPath.ClientID%>').value = "0";
            for (var i = 0; i < drive.length; i++) {
                list += "<div class='localFoldersStyle' onselectstart= 'return false' ondblclick=\"LoadList('" + drive[i].letter + ':\\\\\')" onclick=\"selectFile(this);' + "GetSelectedPath('" + drive[i].letter + ":\')\">" + drive[i].letter + ':\\ - ' + drive[i].description + '</div>';
            }
            document.getElementById("loadDrive").innerHTML = list;
            document.getElementById("directoryPath").innerHTML = 'Path: <a href="" onclick="LoadDrive(); selectFile(this);return false" title="My Computer">My Computer</a>\\';
        }

        function LoadList(fld) {
            var subfolders = GetSubfolders(fld), list = "", path = "", paths = fld.split("\\");
            var divPath = document.getElementById("directoryPath");
            var divPage = document.getElementById("loadDrive");

            document.getElementById('<%=btnSaveLinkFile.ClientID%>').disabled = false;
            document.getElementById('<%=hdnPath.ClientID%>').value = "0";

            for (var i = 0; i < paths.length - 1; i++) {
                if (i == paths.length - 2) {
                    path += paths[i] + ' \\';
                } else {
                    path += "<a href=\"\" onclick=\"LoadList('";
                    for (var j = 0; j < i + 1; j++) {
                        path += paths[j] + "\\\\";
                    }
                    path += '\');return false">' + paths[i] + '</a> \\ ';
                }
            }
            divPath.innerHTML = 'Path: <a href="" onclick="LoadDrive();return false">My Computer</a> \\ ' + path;

            for (var j = 0; j < subfolders.length; j++) {
                var path = (fld + subfolders[j]).replace(/\\/g, "\\\\") + "\\\\";
                list += "<div class='localFoldersStyle' onselectstart= 'return false' ondblclick=\"LoadList('" + path.replace("'", "\\\'") + "')\" onclick=\"GetSelectedPath('" + path.replace("'", "\\'") + "'); selectFile(this)\">" + subfolders[j] + "</div>";
            }

            var files = GetFiles(fld);
            for (var k = 0; k < files.length; k++) {
                list += "<div class='localFilesStyle' onselectstart= 'return false' onclick=\"GetSelectedPath('" + (fld.replace("'", "*") + files[k].replace("'", "*")).replace(/\\/g, "\\\\") + '\'); selectFile(this)">' + files[k] + "</div>";
            }

            divPage.innerHTML = list;

            if (subfolders.length == 0 && files.length == 0) {
                document.getElementById('<%=btnSaveLinkFile.ClientID%>').disabled = true;
            }
        }

        function GetSubfolders(fld) {
            var e, arr = [];
            var fso = new ActiveXObject("Scripting.FileSystemObject");
            var f = fso.GetFolder(fld.toString());
            var e = new Enumerator(f.SubFolders);
            for (; !e.atEnd(); e.moveNext()) {
                arr[arr.length] = e.item().Name;
            }
            return (arr);
        }

        function GetFiles(fld) {
            var e, arr = [];
            var fso = new ActiveXObject("Scripting.FileSystemObject");
            var f = fso.GetFolder(fld.toString());
            var e = new Enumerator(f.Files);
            for (; !e.atEnd(); e.moveNext()) {
                arr[arr.length] = e.item().Name;
            }
            return (arr);
        }

        function GetSelectedPath(fld) {
            document.getElementById('<%=hdnPath.ClientID%>').value = fld.replace("*", "'");
            document.getElementById('<%=txtLinkPath.ClientID%>').value = document.getElementById('<%=hdnPath.ClientID%>').value;
        }

        function openFile(fld) {
            try {
                var openFile = new ActiveXObject("WScript.Shell");
            }
            catch (err) {
                alert("Could not open link. The ActiveX control could not be run");
                return false;
            }
            try {
                openFile.run("\"file:" + fld + "\"");
            }
            catch (er) {
                alert("The link is not accessible");
            }
        }

        function selectFile(div) {
            $(".localFilesStyle").removeClass("highlighSelect");
            $(".localFoldersStyle").removeClass("highlighSelect");
            $(div).addClass("highlighSelect");
        }

        $(function () {
            // fix for BigView PopUp and LinkCase PopUp not showing when clicked after first page load
            //var grid = $("table [id$='GridView1']")[0];
            //if (grid && grid.rows && grid.rows.length > 0) grid.rows[$("input[id$='hdnLastRowSel']").val()].click();
        });

        // override PLCCaseSearch SelectCase() to make it process client-side
        function CustomSelectCase(casekey) {
            var params = { "casekey": casekey };
            $.ajax({
                type: "POST",
                url: "PLCWebCommon/PLCWebMethods.asmx/RetrieveBigViewLinkCaseInfo",
                data: params,
                success: function (retdata) {
                    CloseDialogCaseSearch();
                    var addlLinkedCase = { "casekey": retdata.casekey, "departmentcasenumber": retdata.departmentcasenumber, "labcase": retdata.labcase, "offense": retdata.offense };
                    BigViewClientClick(addlLinkedCase);
                },
                error: function () {
                    alert("Error searching for case record.");
                },
                dataType: "json"
            });
        }

        // override PLCCaseSearch Close to make it process client-side
        function CustomCloseCaseSearch() {
            SetToBigViewBrowseMode();
        }

        function SaveItemTask_OnSuccess(data) {
            if (data.result == "true") {
                $ucItemTask.dialog('close');
                if (data.message != "") ITShowAlert(data.message);
                if (isBigViewShown)
                    BigViewClientClick();
            }
        }


        function OpenBatch(worklistID) {
            CallPLCWebMethod(
                'PLC_WebOCX',
                'BATCHBYWORKLISTID',
                { 'worklistid': worklistID },
                true,
                function () {
                    window.location = 'TAB6Assignments.aspx';
                });
        }


        function OpenDialogAssignSearchResults() {
            $("#dialog-assignsearchresults").dialog({
                autoOpen: true,
                modal: true,
                resizable: false,
                draggable: false,
                closeOnEscape: false,
                dialogClass: "no-close",
                title: 'Search Results',
                width: 890,
                height: 550,
                buttons: {
                    Close: function () {
                        $(this).dialog("close");
                        $(this).dialog('destroy').remove()
                    }
                },
                open: function () {
                    $(this).parent().appendTo("form");
                }
            });
        }

        function OpenDialogQMSDocuments() {
            /// <summary>Initializes and shows the QMS popup</summary>

            $("[id='dialog-qmsdocs']:eq(1)").dialog("destroy").remove();
            $("#dialog-qmsdocs").dialog({
                autoOpen: true,
                modal: true,
                resizable: false,
                draggable: true,
                closeOnEscape: false,
                dialogClass: "no-close",
                title: 'Controlled Documents for Section',
                minWidth: 600,
                buttons: {
                    "Open Document": function () {
                        $('[id$="btnQMSDocOpen"]').click();
                    },
                    "Close": function () {
                        $(this).dialog("close");
                    }
                },
                open: function () {
                    $(this).parent().appendTo("form");

                    if (typeof PLCDBGrid_SeparateHeadingRowToFixedPos == "function") {
                        PLCDBGrid_SeparateHeadingRowToFixedPos('<%= dbgQMSExamDocs.ClientID %>_div');
                        PLCDBGrid_SeparateHeadingRowToFixedPos('<%= dbgQMSRptDocs.ClientID %>_div');
                    }
                }
            });
        }

        function OpenForayLink(examKey){
            if ($('[id$="hdnForayURL"]').val() != "") {
                $("[id='dialog-openForay']:eq(1)").dialog("destroy").remove();
                $("#dialog-openForay").html('<h3 style="padding-top: 10px; color: #003366;">Approve Complete. Foray application exporting all assets.</h3>').dialog({
                    autoOpen: true,
                    modal: true,
                    resizable: false,
                    draggable: true,
                    closeOnEscape: false,
                    noClose: true,
                    title: 'Foray Application',
                    minWidth: 600,
                    buttons: {
                        "Reload": function () {
                            TransferForayAssetsToImageVault();
                        }
                    }
                });

                // open a new tab with the foray url
                window.open($('[id$="hdnForayURL"]').val(), '_blank');
            }
            else 
            {
                strMessage = "The Foray Application is not properly configured.";
                
                $("[id='dialog-openForay']:eq(1)").dialog("destroy").remove();
                $("#dialog-openForay").html('<h3 style="padding-top: 10px; color: #003366;">'+ strMessage +' Please contact LIMS Administrator.</h3>').dialog({
                    autoOpen: true,
                    modal: true,
                    resizable: false,
                    draggable: true,
                    closeOnEscape: false,
                    noClose: true,
                    title: 'Foray Application',
                    minWidth: 600,
                    buttons: {
                        "Close": function () {
                            $(this).dialog("close");
                        }
                    }
                });
            }
        }

        // transfer the assets into the image vault
        function TransferForayAssetsToImageVault() {

            StartModalProgressDialog("Transferring images...");
            
            // call the ocx method
            CallPLCWebMethod("PLC_WebOCX", "SCANTOLABREPT", JSON.parse($('[id$="hdnForayExportParameters"]').val()), true, 
                function () { CheckBrowserWindowID(function () { EnablePage(); ClearKeepSessionAlive(); location.reload(); }); }, 
                function () { ShowAlert("Alert", "An OCX error has occured!"); });
            
        }

        function SetBtnQMSDocOpenState(enable) {
            /// <summary>Sets the QMS Open Document button state to Enable or Disable</summary>
            /// <param name="enable" type="Boolean">Set to 'true' if Open Document button should be enabled</param>

            if (enable)
                $(".ui-dialog-buttonpane button:contains('Open Document')").button("enable");
            else
                $(".ui-dialog-buttonpane button:contains('Open Document')").button("disable");
        }

        // add cancel click event in PLCMsgComment
        function MsgComment_Cancel(postbackID) {
            if (postbackID.indexOf("btnConfirmDelete") > -1) // for delete click, skip if edit
                LockUnlockRecord("U");
        }

        // add cancel click event in PLCVerifySignature
        function CloseVerifySignature() {
            LockUnlockRecord("U");
        }

        function WaitForNotesPacketCompletion(signID, examKey) {
            setTimeout(function () {
                var interval = setInterval(function () {
                    $.ajax({
                        type: "POST",
                        url: GetWebRootUrl() + "PLCWebCommon/PLCWebMethods.asmx/CheckProduceNotesPacket",
                        data: { k: examKey },
                        dataType: "json",
                        success: function (response) {
                            if (response.produceNP == "C") {
                                $("#" + signID).val("NP Ready");
                                clearInterval(interval);
                            }
                            else if (response.produceNP == "E") {
                                $("#" + signID).val("NP ERROR");
                                clearInterval(interval);
                            }
                            else if (response.produceNP == "D") {
                                window.location = 'TAB6Assignments.aspx';
                            }
                        },
                        error: function (response) {
                            clearInterval(interval);
                        }
                    });
                }, 5000);

                var $hdnNPIntervals = $("[id$='hdnNPIntervals']");
                var npIntervals = $hdnNPIntervals.val();
                $hdnNPIntervals.val(npIntervals + (npIntervals.length>0 ? ",":"") + interval);
            }, 10);
            
        }

        function ClearNPIntervals() {
            var $hdnNPIntervals = $("[id$='hdnNPIntervals']");
            var npIntervals = $hdnNPIntervals.val().split(',');

            for (var index = 0; index < npIntervals.length; npIntervals++)
                clearInterval(npIntervals[index]);

            $hdnNPIntervals.val('');
        }

        function openRegenReasonPopup() {
            $("[id='dialog-regenReason']:eq(1)").dialog("destroy").remove();
            $("#dialog-regenReason").dialog({
                autoOpen: true,
                modal: true,
                resizable: false,
                draggable: true,
                closeOnEscape: false,
                noClose: true,
                title: 'Confirmation',
                minWidth: 500,
                buttons: {
                    "OK": function () {
                        $("[id$='hdnRegenReason']").val($("[id$='txtRegenReason']").val());
                        $("[id$='btnRegenReasonDummy']").click();
                    },
                    "Cancel": function () {
                        $(this).dialog("close");
                        __doPostBack();
                    }
                },
                open: function () {
                    $(this).parent().find('.ui-dialog-titlebar-close').hide();
                    $("[id$='txtRegenReason']").val("");
                }
            });
        }

        function openAuditReasonPopup() {
            $("[id='dialog-auditReason']:eq(1)").dialog("destroy").remove();
            $("#dialog-auditReason").dialog({
                autoOpen: true,
                modal: true,
                resizable: false,
                draggable: true,
                closeOnEscape: false,
                noClose: true,
                title: 'Confirmation',
                minWidth: 400,
                buttons: {
                    "OK": function () {
                        if ($("[id$='txtAuditReason']").val().trim() != '') {
                            $("[id$='hdnAuditReason']").val($("[id$='txtAuditReason']").val());
                            $("[id$='btnAuditReasonDummy']").click();
                            $(this).dialog("close");
                        }
                        else {
                            Alert('Please enter a reason to proceed.');
                        }
                    },
                    "Cancel": function () {
                        $(this).dialog("close");
                        __doPostBack();
                    }
                },
                open: function () {
                    $(this).parent().find('.ui-dialog-titlebar-close').hide();
                    $("[id$='txtAuditReason']").val("");
                }
            });
        }

        function updateRegenReasonMemo() {
            var $fbRegenReason = $("[id$='fbRegenReason']").find("input:visible");
            var $txtRegenReason = $("[id$='txtRegenReason']");
           
            var selectedReason = $fbRegenReason.val();
            if ($fbRegenReason.length > 0 && selectedReason.length > 0) {
                var reasonMemoVal = $txtRegenReason.val();
                if (reasonMemoVal.length > 0) {
                    reasonMemoVal += "\n"
                }
                reasonMemoVal += selectedReason;
                $txtRegenReason.val(reasonMemoVal);
            }
        }

        function GoToDNAWorksheet() {
            document.getElementById("lnkDna").click();
        }

        function assignRouteOpen() {
            setOnTopOfModal(".dbgrid-routing-history");
        }

        function setOnTopOfModal(selector) {
            setTimeout(function () {
                var zIndex = $(".modalPopup:visible,.ui-dialog:visible").css("zIndex") || 101;
                $(selector).css("zIndex", zIndex);
            }, 100);
        }

        function OnPageUnload() {
            if (_winAttach && !_winPopup.closed) {
                OCXObj = null;
                _winAttach.close();
            }
        }

        var _winAttach;
        function OpenImageVaultWindow() {
            if (_winAttach && !_winAttach.closed) {
                reinitTaskAttachment();
            }

            OCXObj = "1";
            _winAttach = window.open('ImageVault.aspx', 'Image Vault', 'width=860, height=500, scrollbars=1, resizable=1', false);
            window.onfocus = function () {
                var params = {
                    ASSIGN_KEY: $("[id*=hdnAssignKey]").val()
                };

                InitAttachments(params);
            };
            _winAttach.focus();
            return false;
        }

        function InitAttachments(params) {
            $.ajax({
                type: "POST",
                url: "PLCWebCommon/PLCWebMethods.asmx/InitAssignAttachsVariables",
                data: params,
                success: function (e) {
                    if (e == "T")
                        $("[id*=ImageButton1]").attr("src", "Images/paperclip_withdoc.gif");
                    else
                        $("[id*=ImageButton1]").attr("src", "Images/paperclip.gif");
                },
                error: function (e) {
                    if (e.responseText == "T")
                        $("[id*=ImageButton1]").attr("src", "Images/paperclip_withdoc.gif");
                    else
                        $("[id*=ImageButton1]").attr("src", "Images/paperclip.gif");
                },
                dataType: "json"
            });
        }
    </script>

    <script id="tpl-bigviewresults" type="text/html">
        <table class="datagrid">
            <colgroup>
            {{#columnkeys}}
                <col class="{{.}}">
            {{/columnkeys}}
            </colgroup>
            <thead>
                <tr>
                {{#header}}
                    <th>{{.}}</th>
                {{/header}}
                </tr>
            </thead>
            <tbody>
            {{#rows}}
                <tr>
                {{#.}}
                    <td>{{.}}</td>
                {{/.}}
                </tr>
            {{/rows}}
            </tbody>
            <tfoot>
                {{#settings}}
                <tr class="settings">
                    <td class='key casekey'>{{casekey}}</td>
                    <td class='key examkey'>{{examkey}}</td>
                    <td class='key section'>{{section}}</td>
                </tr>
                {{/settings}}
            </tfoot>
        </table>
    </script>

    <script id="tpl-searchlinkcaseinfo" type="text/html">
        {{#caseinfo}}
        <div>
            <span class="label">Case Type:</span>
            <span class="value">{{casetype}}</span>
        </div>
        <div>
            <span class="label">Offense Type:</span>
            <span class="value">{{offensedesc}}</span>
        </div>
        <div>
            <span class="label">Offense Date:</span>
            <span class="value">{{offensedate}}</span>
        </div>
        <div>
            <span class="label">Department Name:</span>
            <span class="value">{{deptname}}</span>
        </div>
        <div>
            <span class="label">Officer:</span>
            <span class="value">{{caseofficer}}</span>
        </div>
        {{/caseinfo}}
    </script>

    <script id="tpl-searchlinkcaseitems" type="text/html">
        <table class="datagrid">
            <colgroup>
            {{#columnkeys}}
                <col class="{{.}}">
            {{/columnkeys}}
            </colgroup>
            <thead>
                <tr>
                {{#header}}
                    <th>{{.}}</th>
                {{/header}}
                </tr>
            </thead>
            <tbody>
            {{#rows}}
                <tr>
                {{#.}}
                    <td>{{.}}</td>
                {{/.}}
                </tr>
            {{/rows}}
            </tbody>
        </table>
    </script>

    <script type="text/javascript" src="Javascripts/MatrixControlScripts.js?rev=<%= PLCSession.CurrentRev %>"></script>
</asp:Content>
