<%@ Page Language="C#" MasterPageFile="~/CASEFILE.master" AutoEventWireup="True" CodeBehind="TAB1Case.aspx.cs" Inherits="BEASTiLIMS.TAB1Case" Title="Case Information" %>

<%@ Register Assembly="PLCCONTROLS" Namespace="PLCCONTROLS" TagPrefix="cc1" %>
<%@ Register Src="~/PLCWebCommon/CodeHead.ascx" TagName="CodeHead" TagPrefix="uc1" %>
<%@ Register Src="~/PLCWebCommon/PLCWebControl.ascx" TagName="PLCWebControl" TagPrefix="uc1" %>
<%@ Register Src="~/PLCWebCommon/PLCDialog.ascx" TagName="Dialog" TagPrefix="PLC" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        function OnChangeLabCase(evt) {
            var mpe;
            if (evt.shiftKey)
                mpe = $find("mpeAutoChangeLabCase");
            else 
                mpe = $find("mpeChangeLabCase");

            if (mpe)
                mpe.show();
        }
    
        function InitLabCaseNumberChangeButton() {
            // Initialize client-side DBPanel IDs. Needed by GetDBPanelElement().
            eval($("input[id$='hdnDBPanelClientIDsScript']").val());

            // Look for dbpanel's lab case text box and add a 'Change' button beside it.
            var $labcasetxtbox = GetDBPanelElement("PLCDBPanel1", "TV_LABCASE.LAB_CASE");
            if ($labcasetxtbox.length > 0 && $("#btnChange").length == 0) {
                var labcasetxtbox = $labcasetxtbox.get(0);
                var labcaseparent = labcasetxtbox.parentNode;
            
                // Add Lab Case Change button beside the labcase panel textbox.
                var btnChange = document.createElement("input");
                btnChange.setAttribute("id", "btnChange");
                btnChange.setAttribute("type", "button");
                btnChange.setAttribute("value", "Change");
                btnChange.setAttribute("disabled", "disabled");
                labcaseparent.appendChild(btnChange);

                var $btnchange = $("#btnChange");
                $btnchange.mouseup(OnChangeLabCase);

                $("#labcasenolabel").text(labcasetxtbox.value);
            }
        }
    
        function EnableLabCaseNumberChangeButton(bEnable) {
            var btnChange = document.getElementById("btnChange");
            if (btnChange) {
                if (bEnable)
                    btnChange.setAttribute("disabled", "");
                else
                    btnChange.setAttribute("disabled", "disabled");
            }
        }

        function btnJIMSReport_Click() {
            $.ajax({
                type: "POST",
                url: "PLCWebCommon/PLCWebMethods.asmx/JIMSReport",
                data: "{}",
                success: LoadReport_Success,
                error: LoadReport_Error,
                dataType: "html"
            });
        }

        function LoadReport_Success(e) {
            var response = eval('(' + e + ')');
            if (response.url != "") {
                window.open(response.url);
            }
        }

        function LoadReport_Error(e) {
            alert("Error: " + e);
        }
    </script>
    <style type="text/css">
        .style1 {
            width: 100%;
        }
        .labcasedlg {margin:1.5em 1em 1em 1em;}
        .labcasedlgcaption {margin-bottom:1em;}
        .labcasedlgcontrols {margin-bottom:1em; margin-left:1em;}
        .labcasedlgbuttons {text-align:center;}
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cp" runat="server">
    <uc1:PLCWebControl ID="WebOCX" runat="server" />
    <div>
        <cc1:PLCSessionVars ID="PLCSessionVars1" runat="server" />
        <asp:HiddenField ID="CASEKEYDISPACKET" runat="server"/>
        <asp:HiddenField ID="AESINFODISPACKET" runat="server"/>
        <asp:HiddenField ID="GLOBALANALYSTDISPACKET" runat="server"/>
        <asp:HiddenField ID="SOURCEDISPACKET" runat="server"/>
        <asp:HiddenField ID="CINFODISPACKET" runat="server"/>
        <asp:HiddenField ID="SERVICEURLDISPACKET" runat="server" />
        <asp:HiddenField ID="PREVIOUSPAGEDISPACKET" runat="server" />
        <asp:HiddenField ID="RETURNPAGEDISPACKET" runat="server" />
    </div>

    <asp:Button ID="btndummyChangeLabCase" runat="server" Style='display: none;' />
    <ajaxToolkit:ModalPopupExtender ID="mpeChangeLabCase" BehaviorID="mpeChangeLabCase" runat="server" BackgroundCssClass="modalBackground"
        PopupControlID="pnlChangeLabCase" TargetControlID="btndummyChangeLabCase" DropShadow="false" >
    </ajaxToolkit:ModalPopupExtender>
    <asp:Panel ID="pnlChangeLabCase" runat="server" CssClass="modalPopup" Style="display:none;" Width="400px">
        <div class="labcasedlg">
            <div class="labcasedlgcaption">
		        Changing Lab Case Number for <span id="labcasenolabel"></span>
            </div>
            <fieldset id="labcaseinputdetailed" class="labcasedlgcontrols">
                <table>
                    <tr>
                        <td>Laboratory Code</td>
                        <td><cc1:CodeHead ID="chLabCode" runat="server" TableName="TV_LABCTRL" PopupX="100" PopupY="30" /></td>
                    </tr>
                    <tr>
                        <td>Lab Case Year</td>
                        <td><asp:TextBox ID="txtLabCaseYear" runat="server" Visible="true" Width="3.5em"></asp:TextBox></td>
                        <ajaxToolkit:MaskedEditExtender ID="txtLabCaseYear_MaskedEditExtender" runat="server" Mask="YYYY" MaskType="Number" TargetControlID="txtLabCaseYear"></ajaxToolkit:MaskedEditExtender>
                    </tr>
                    <tr>
                        <td>Lab Case Number</td>
                        <td><asp:TextBox ID="txtLabCaseNumber" runat="server" Visible="true" Width="10em"></asp:TextBox></td>
                    </tr>
                    <tr>
                        <td>Lab Case</td>
                        <td><asp:TextBox ID="txtLabCase" runat="server" Visible="true" Width="10em" MaxLength="15"></asp:TextBox></td>
                    </tr>
                </table>
            </fieldset>
            <div class="labcasedlgbuttons">
		        <asp:Button ID="btnChangeLabCaseOK" runat="server" Text="OK" OnClick="btnChangeLabCaseOK_Click" Width="5em" />
		        <asp:Button ID="btnChangeLabCaseCancel" runat="server" Text="Cancel" OnClick="btnChangeLabCaseCancel_Click" Width="5em" />
            </div>
        </div>
    </asp:Panel>

    <asp:Button ID="btndummyAutoChangeLabCase" runat="server" Style='display: none;' />
    <ajaxToolkit:ModalPopupExtender ID="mpeAutoChangeLabCase" BehaviorID="mpeAutoChangeLabCase" runat="server" BackgroundCssClass="modalBackground"
        PopupControlID="pnlAutoChangeLabCase" TargetControlID="btndummyAutoChangeLabCase" DropShadow="false" >
    </ajaxToolkit:ModalPopupExtender>
    <asp:Panel ID="pnlAutoChangeLabCase" runat="server" CssClass="modalPopup" Style="display:none;" Width="400px">
        <div class="labcasedlg">
            <div class="labcasedlgcaption">
		        Would you like to assign the next Lab Case Number?
            </div>
            <div class="labcasedlgbuttons">
		        <asp:Button ID="btnAutoChangeLabCaseOK" runat="server" Text="OK" OnClick="btnAutoChangeLabCaseOK_Click" Width="5em" />
		        <asp:Button ID="btnAutoChangeLabCaseCancel" runat="server" Text="Cancel" OnClick="btnAutoChangeLabCaseCancel_Click" Width="5em" />
            </div>
        </div>
    </asp:Panel>

    <div class="dbbox">
        <div class="dbpanelblk withbtnblk" style="height: auto;">
            <table class="style1" cellpadding="0">
                <tr>
                    <td>
                        <asp:Label ID="lCaseLockStatus" runat="server" Text="Case is locked." ForeColor="Red" Visible="false"></asp:Label>
                        <div id="dvPLCLockStatus" runat="server" visible="false" >
                            <asp:Label ID="lPLCLockStatus" runat="server" ForeColor="Red"></asp:Label>
                        </div>
                    </td>
                </tr>
                <tr>
                    <td valign="top">
                        <cc1:PLCDBPanel ID="PLCDBPanel1" runat="server" PLCDataTable="TV_LABCASE" PLCPanelName="CASETAB"
                            PLCShowEditButtons="True" PLCWhereClause="Where 0 = 1" onplcdbpanelbuttonclick="PLCDBPanel1_PLCDBPanelButtonClick"
                            OnPLCDBPanelCodeHeadChanged="PLCDBPanel1_PLCDBPanelCodeHeadChanged" OnPLCDBPanelTextChanged="PLCDBPanel1_TextChanged" PLCAuditCode="5" PLCDeleteAuditCode="28"
                            OnPLCDBPanelValidate="PLCDBPanel1_PLCDBPanelValidate" PLCDeleteAuditSubCode="1" Width="100%" PLCAttachPopupTo="body" >
                        </cc1:PLCDBPanel>
                    </td>
                </tr>
            </table>
            <div style="margin: 6px;">
                <asp:Label ID="lblCaseReference" runat="server" Text="Case Reference#" ForeColor="Black" Visible="false"></asp:Label>
                <asp:Label ID="lblCaseType" runat="server" Text="" ForeColor="Black" Visible="false"></asp:Label>
            </div>
            <div style="margin: 4px;">
                <asp:CheckBox ID="chkCaseLocked" runat="server" Text="Case is locked" Font-Bold="true" Enabled="false" />
            </div>
            <cc1:PLCDBPanelLabels ID="PLCDBPanelLabels1" runat="server" TableName="UV_CASESTATUS" PanelName="CASESTATUS" WhereClause="Where 0 = 1" Title="Case Status"></cc1:PLCDBPanelLabels>
        </div>
        <div class="dbpanelbtnblk">
            <div>
                <asp:Button ID="bnSupplements" runat="server" Text="Supplements" Width="100%" OnClick="bnSupplements_Click" OnLoad="bnSupplements_Load" />
                <cc1:PLCButton ID="bnCaseJacket" runat="server" Text="$CaseTab_CaseJacket" Width="100%" OnClick="bnCaseJacket_Click" />
                <cc1:PLCButton ID="bnCaseLabel" runat="server" Text="$CaseTab_CaseLabel" Width="100%" OnClick="bnCaseLabel_Click" />
                <asp:Button ID="bnReference" runat="server" Text="Reference" Width="100%" OnClick="bnReference_Click" OnLoad="bnReference_Load" />
                <asp:Button ID="btnRelatedCase" runat="server" Text="Related Cases" Width="100%" OnClick="btnRelatedCases_Click" />
                <asp:Button ID="btnForms" runat="server" Text="Forms" Width="100%" OnClick="btnForms_Click" />
                <asp:Button ID="bnSchedule" runat="server" Text="Schedule" Width="100%" OnClick="bnSchedule_Click" />
                <asp:Button ID="bnDispo" runat="server" Text="Retention Review" Width="100%" OnClick="bnDispo_Click" OnLoad="bnDispo_Load" />
                <asp:Button ID="bnTeam" runat="server" Text="Team" Width="100%" OnClick="bnTeam_Click" OnLoad="bnTeam_Load" />
                <asp:Button ID="bnCaseLock" runat="server" Text="Lock Case" Width="100%" OnClick="bnCaseLock_Click" OnLoad="bnCaseLock_Load" />
                <cc1:PLCButton ID="bnCaseReports" runat="server" Text="$CaseTab_CaseReports" Width="100%" OnClick="bnCaseReports_Click" />
                <asp:Button ID="bnDiscoveryPacket" runat="server" Text="Discovery Packet" OnClick="bnDiscoveryPacket_Click" Width="100%" Visible="false" />
                <asp:Button ID="btnNICS" runat="server" Text="NICS" Width="100%" OnClick="btnNICS_Click" />
                <asp:Button ID="btnActivity" runat="server" Text="Activity" Width="100%" OnClick="btnActivity_Click" Visible="false" />
                <asp:Button ID="btnJIMSReport" runat="server" Text="JIMS"  OnClientClick="btnJIMSReport_Click(); return false;" Width="100%"  Visible="false" />
                <asp:Button ID="btnFOIA" runat="server" Text="FOIA" Width="100%" OnClick="btnFOIA_Click" Visible="false"/>
                <asp:Button ID="btnDistribution" runat="server" Text="Distribution"  OnClick="btnDistribution_Click" Width="100%"  Visible="false" />
                <asp:Button ID="btnLaboffense" runat="server" Text="Case Offense" OnClick="btnLaboffense_Click" Width="100%" Visible="false" />
                <asp:Button ID="btnCodisHits" runat="server" Text="Codis Hits" OnClick="btnCodisHits_Click" Width="100%" Visible="false" />
                <asp:Button ID="btnAddressLabel" runat="server" Text="Address Label" OnClick="btnAddressLabel_Click" Width="100%" Visible="false"/>
            </div>
            <asp:Panel ID="Panel2" runat="server">
            </asp:Panel>
        </div>
        <div class="dbbtnpanel">
            <cc1:PLCButtonPanel ID="PLCButtonPanel1" runat="server" PLCShowAddButton="False" PLCCustomButtons="RecordUnlock,Search Results..."
                PLCTargetControlID="PLCDBPanel1" Width="100%" PLCShowEditButtons="True" PLCShowDeleteButton="true" PLCDisplayBottomBorder="False"
                PLCDisplayTopBorder="False" OnPLCButtonClick="PLCButtonPanel1_PLCButtonClick">
            </cc1:PLCButtonPanel>
            <div class="clear"></div>
        </div>
    </div>
    
    <div id="dialog-casesearchresults" style="padding: 10px; display: none;">
        <div id="dialog-casesearchresults-content" style="margin-top: 10px;">
            <asp:UpdatePanel ID="upSearchCase" runat="server">
                <ContentTemplate>
                    <PLC:Dialog ID="dlgMessageSearchResults" runat="server" />
                    <cc1:PLCDBGrid ID="gvSearchCase" runat="server" AllowSorting="True" AllowPaging="True"
                        OnSelectedIndexChanged="gvSearchCase_SelectedIndexChanged" 
                        PLCGridWidth="100%" Height="380" EmptyDataText="Go to Case Search and select criteria then click 'Find Cases' button." RecordsPerPage="15">
                    </cc1:PLCDBGrid>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>

    
    <div id="dialog-laboffense" style="display:none;">  
        <div id="dialog-laboffense-content">
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>                
                <cc1:PLCDBGrid ID="grdCustomCaseInfo" runat="server" AllowPaging="True" AllowSorting="True"
                    DataKeyNames="CASE_KEY" EmptyDataText="No results found for this case.">
                </cc1:PLCDBGrid>              
            </ContentTemplate>
            </asp:UpdatePanel>
        
        </div>
     </div>

         <div id="dialog-codishits" style="padding: 10px; display: none;">
        <div id="dialog-codishits-content">
            <div style="margin-bottom: 10px; width: 100%">
                <asp:UpdatePanel ID="upCodisHits" runat="server">
                    <ContentTemplate>
                        <cc1:PLCDBGrid ID="grdCodisHits" runat="server" AllowSorting="True" AllowPaging="True"
                            OnSelectedIndexChanged="grdCodisHits_SelectedIndexChanged" OnSorting="grdCodisHits_Sorting" DataKeyNames="HIT_NUMBER"
                            PLCGridWidth="100%" EmptyDataText="No codis hit files attached to the case.">
                        </cc1:PLCDBGrid>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
            <div style="margin-bottom: 10px; width: 100%">
                <asp:UpdatePanel ID="updCodisHitsDoc" runat="server">
                    <ContentTemplate>
                        <cc1:PLCDBGrid ID="grdCodisHitsDoc" runat="server" AllowSorting="True" AllowPaging="True" 
                        OnSelectedIndexChanged="grdCodisHitsDoc_SelectedIndexChanged" DataKeyNames="HIT_NUMBER,SOURCE"
                        PLCGridWidth="100%" EmptyDataText="No file found.">
                        </cc1:PLCDBGrid>
                        <asp:HiddenField ID="hdnCodisHitNumber" runat="server" />
                        <asp:HiddenField ID="hdnDocSource" runat="server" />
                        <asp:Button ID="btnOpenCodisHitPopup" runat="server" Text="Open" Visible="true" Enabled="false" style="position:absolute; float:left; margin:5px;" OnClientClick="OpenCodisHitDocument(); return false;" />
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>
     

    <asp:Button ID="btnDummySupp" runat="server" Style="display: none;" />
    <ajaxToolkit:ModalPopupExtender ID="mpeSupplements" runat="server" BackgroundCssClass="modalBackground"
        PopupControlID="pnlSupplements" PopupDragHandleControlID="pnlHeadSupplements"
        TargetControlID="btnDummySupp" DropShadow="false" OkControlID="btnDummySuppOK"
        CancelControlID="btnSuppClose" Drag="true">
    </ajaxToolkit:ModalPopupExtender>
    <asp:Panel ID="pnlSupplements" runat="server" CssClass="modalPopup" Style="display: none;" Width="450px">
        <asp:Panel ID="pnlHeadSupplements" runat="server" Height="20px" Width="100%" CssClass="caption">
            Case Supplements
        </asp:Panel>
        <asp:Panel ID="pnlSuppType" runat="server" GroupingText="Select Supplement Type" Style="padding: 5px;">
            <asp:RadioButtonList ID="rbtnListSuppType" runat="server" AutoPostBack="true" RepeatDirection="Horizontal"
                RepeatLayout="Flow" OnSelectedIndexChanged="rbtnListSuppType_SelectedIndexChanged" >
                <asp:ListItem Text="Regular" Selected="True" />
                <asp:ListItem Text="Conversion Only"/>
                <asp:ListItem Text="All Supplements"/>
            </asp:RadioButtonList>
        </asp:Panel>
        <div style="margin: 10px; width: 100%">
            <cc1:PLCGridView ID="gvSupplements" runat="server" Width="420px"
                DataKeyNames="SUPPLEMENT_TABLE,SUPPLEMENT_DESCRIPTION" Height="150px"
                AutoGenerateSelectButton="true" AutoGenerateColumns="false" EmptyDataText="No existing supplement tables found."
                OnSelectedIndexChanged="gvSupplements_SelectedIndexChanged" AllowSorting="True">
                <Columns>
                    <asp:BoundField DataField="SUPPLEMENT_DESCRIPTION" HeaderText="Supplement Description" ItemStyle-Width="250px" />
                    <asp:BoundField DataField="SUPP_CNT" HeaderText="Number of Entries" ItemStyle-Width="170px" />
                </Columns>
            </cc1:PLCGridView>
        </div>
        <div align="center" style="padding: 10px;">
            <asp:Button ID="btnSuppPrint" runat="server" Text="Print" Visible="false" />
            <asp:Button ID="btnSuppOpen" runat="server" Text="Edit Supplement" OnClick="btnSuppOpen_Click" />
            <asp:Button ID="btnSuppClose" runat="server" Text="Close" />
            <asp:Button ID="btnDummySuppOK" runat="server" Style="display: none;" />
        </div>
    </asp:Panel>

    <asp:PlaceHolder ID="phMsgBox" runat="server"></asp:PlaceHolder>
    <asp:PlaceHolder ID="phMsgBoxComments" runat="server"></asp:PlaceHolder>
    <asp:Button ID="MsgCommentPostBackButton" runat="server" Text="Button" style="display: none;" OnClick="MsgCommentPostBackButton_Click" />
    <asp:TextBox ID="UserComments" runat="server" TextMode="MultiLine" style="display: none;"></asp:TextBox>
    <asp:Button ID="btnConfirmUpdate" runat="server" Text="Button" style="display: none;" OnClick="btnConfirmUpdate_Click" />
    <asp:TextBox ID="txtConfirmUpdate" runat="server" TextMode="MultiLine" style="display: none;"></asp:TextBox>
    <asp:HiddenField ID="hdnConfirmUpdate" runat="server" />
    <asp:Button ID="btnDeleteCase" runat="server" Text="Button" OnClick="btnDeleteCase_Click" style="display: none;" />
    <asp:TextBox ID="txtDeleteReason" runat="server" TextMode="MultiLine" style="display: none;"></asp:TextBox>
    <cc1:MessageBox ID="mbDeleteOk" runat="server" PanelCSSClass="modalPopup" CaptionCSSClass="caption" OnOkClick="mbDeleteOk_OkClick" />

    <script type="text/javascript">
        function FullScreen(id) {
            var panel = document.getElementById(id);
            if (panel != null) {
                panel.style.width = window.screen.availWidth + "px";
                panel.style.height = (window.screen.availHeight * .50) + "px";
            }
        }
        
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        prm.add_endRequest(function () {
            $(GetDBPanelElement("PLCDBPanel1", "TV_LABCASE.DEPARTMENT_CASE_NUMBER")).unbind("cleared").bind("cleared", function () {
                window.focus();
                $(this).focus();
            });

        });

        function OpenDialogCaseSearchResults() {
            $("#dialog-casesearchresults").dialog({
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
                        $(this).dialog('destroy').remove();
                    }
                },
                open: function () {
                    $(this).parent().appendTo("form");
                }
            });
        }

        function ShowLabOffensePopUp() {
            $("[id='dialog-laboffense']:eq(1)").dialog("destroy").remove();
            $("#dialog-laboffense").dialog({
                title: "Case Offense",
                resizable: false,
                modal: true,
                draggable: true,
                closeOnEscape: false,
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

        function ShowCodisHitsPopUp() {
            $("[id='dialog-codishits']:eq(1)").dialog("destroy").remove();
            $("#dialog-codishits").dialog({
                title: "CODIS Hit Reports",
                resizable: false,
                modal: true,
                draggable: true,
                closeOnEscape: false,
                width: 750,
                height: 400,
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

        function OpenCodisHitDocument() {
            var hitNumber = $("[id*=hdnCodisHitNumber]").val();
            var docSource = $("[id*=hdnDocSource]").val();
            window.open("ShowReportCodisHits.aspx?HitNumber=" + hitNumber + "&DocumentSource=" + docSource + "", "_blank", "titlebar=no,status=no,toolbar=no,location=no,resizable=yes");
            return false;
        }
    </script>

    <asp:HiddenField ID="hdnDBPanelClientIDsScript" runat="server" />
</asp:Content>
