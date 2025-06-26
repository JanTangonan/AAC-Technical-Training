<%@ Page Language="C#" MasterPageFile="~/CASEFILE.master" AutoEventWireup="True" CodeBehind="TAB1Case.aspx.cs" Inherits="BEASTiLIMS.TAB1Case" Title="Case Information" %>

<%@ Register Assembly="PLCCONTROLS" Namespace="PLCCONTROLS" TagPrefix="cc1" %>
<%@ Register Src="~/PLCWebCommon/CodeHead.ascx" TagName="CodeHead" TagPrefix="uc1" %>
<%@ Register Src="~/PLCWebCommon/PLCWebControl.ascx" TagName="PLCWebControl" TagPrefix="uc1" %>
<%@ Register Src="~/PLCWebCommon/PLCDialog.ascx" TagName="Dialog" TagPrefix="PLC" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<%@ Register Src="~/PLCWebCommon/PLCPageHead.ascx" TagName="PageHead" TagPrefix="PLC" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <PLC:PageHead runat="server" include="plccontrols" />
    <script type="text/javascript">
        function OnChangeLabCase(evt) {
            var mpe;
            if (evt.shiftKey) {
                mpe = $find("mpeAutoChangeLabCase");
                if(mpe)
                    mpe.show();
            }
            else
                showChangeLabCasePopup();
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


                $("[id*=labcasenolabel]").text(labcasetxtbox.value);
                $(".labcasetxt").text(GetDBPanelPrompt("PLCDBPanel1", "TV_LABCASE.LAB_CASE"));
            }
        }
    
        function EnableLabCaseNumberChangeButton(bEnable) {
            var btnChange = document.getElementById("btnChange");
            if (btnChange) {
                if (bEnable)
                    btnChange.removeAttribute("disabled");
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
                dataType: "json"
            });
        }

        function LoadReport_Success(response) {
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
        .labcasejdlgcontrols {margin-bottom:1em;}

        .change-panel{
            display:inline-flex;
            margin-top: 5px;
            margin-left:5px;

        }

        .change-hide{
            display:none;

        }

        .change-buttons{
            margin-top:25px;
            margin-bottom:10px;         
            text-align:center;
            
        }


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

    <div id="mdialog-change-labcase" class="change-hide">
         <div id="mdialog-change-labcase-content"> 
             <asp:UpdatePanel runat="server" id="UpdatePanel4" UpdateMode="Conditional" ChildrenAsTriggers="true">           
                <ContentTemplate>
                    <div class="labcasedlgcaption">
		                Changing <span runat="server" id="lblChangeLabCase" class="labcasetxt"></span> for <span runat="server" id="labcasenolabel"></span>
                    </div>
                    <fieldset id="labcaseinputdetailed" class="labcasejdlgcontrols">
                        <cc1:DBPanel ID="dbpChangeLabCase" runat="server" PLCPanelName="CHANGE_LABCASE" PLCAllowBlankRecordSave="true" PLCAttachPopupTo=".popup-ffb" Width="100%">
                        </cc1:DBPanel>                
                    </fieldset>

                    <div class="labcasedlgbuttons">
		                <cc1:PLCButton ID="btnChangeLabCaseOK" runat="server" Text="OK" PromptCode="TAB1Case.btnChangeLabCaseOK" OnClick="btnChangeLabCaseOK_Click" Width="5em" />
		                <cc1:PLCButton ID="btnChangeLabCaseCancel" runat="server" Text="Cancel" PromptCode="TAB1Case.btnChangeLabCaseCancel" onclick="btnCloseDummy_Click" Width="5em" />
                    </div>                  
                </ContentTemplate>
             </asp:UpdatePanel>
             <asp:Button runat="server" ID="btnDummyLabCase" style="display:none;" OnClick="btnDummyLabCase_Click" />
             <asp:HiddenField runat="server" ID="hdnCaseOffenseDate" />
             <asp:HiddenField runat="server" ID="hdnCaseLabCode" />
            
         </div>     
    </div>

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
		        <cc1:PLCButton ID="btnAutoChangeLabCaseOK" runat="server" Text="OK" PromptCode="TAB1Case.btnAutoChangeLabCaseOK" OnClick="btnAutoChangeLabCaseOK_Click" Width="5em" />
		        <cc1:PLCButton ID="btnAutoChangeLabCaseCancel" runat="server" Text="Cancel" PromptCode="TAB1Case.btnAutoChangeLabCaseCancel" OnClick="btnAutoChangeLabCaseCancel_Click" Width="5em" />
            </div>
        </div>
    </asp:Panel>

    <div class="dbbox">
        <div class="dbpanelblk withbtnblk" style="height: auto;">
            <table class="style1" cellpadding="0">
                <tr>
                    <td>
                        <cc1:PLCLabel ID="lCaseLockStatus" runat="server" Text="Case is locked." PromptCode="TAB1Case.lCaseLockStatus.LOCKED" ForeColor="Red" Visible="false"></cc1:PLCLabel>
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
                <cc1:PLCLabel ID="lblCaseReference" runat="server" Text="Case Reference#" PromptCode="TAB1Case.lblCaseReference" ForeColor="Black" 
                    Visible="false" CssClass="word-break-all"></cc1:PLCLabel>
                <asp:Label ID="lblCaseType" runat="server" Text="" ForeColor="Black" Visible="false"></asp:Label>
            </div>
            <div style="margin: 4px;">
                <cc1:PLCCheckBox ID="chkCaseLocked" runat="server" Text="Case is locked" PromptCode="TAB1Case.chkCaseLocked" Font-Bold="true" Enabled="false" />
            </div>
            <cc1:PLCDBPanelLabels ID="PLCDBPanelLabels1" runat="server" TableName="UV_CASESTATUS" PanelName="CASESTATUS" WhereClause="Where 0 = 1" Title="Case Status"></cc1:PLCDBPanelLabels>
        </div>
        <div class="dbpanelbtnblk">
            <div>
                <cc1:PLCButton ID="bnSupplements" runat="server" Text="Supplements" PromptCode="TAB1Case.bnSupplements" Width="100%" OnClick="bnSupplements_Click" OnLoad="bnSupplements_Load" />
                <cc1:PLCButton ID="bnCaseJacket" runat="server" Text="$CaseTab_CaseJacket" Width="100%" OnClick="bnCaseJacket_Click" />
                <cc1:PLCButton ID="bnCaseLabel" runat="server" Text="$CaseTab_CaseLabel" Width="100%" OnClick="bnCaseLabel_Click" />
                <cc1:PLCButton ID="bnReference" runat="server" Text="Reference" PromptCode="TAB1Case.bnReference" Width="100%" OnClick="bnReference_Click" OnLoad="bnReference_Load" />
                <cc1:PLCButton ID="btnRelatedCase" runat="server" Text="Related Cases" PromptCode="TAB1Case.btnRelatedCase" Width="100%" OnClick="btnRelatedCases_Click" />
                <cc1:PLCButton ID="btnForms" runat="server" Text="Forms" PromptCode="TAB1Case.btnForms" Width="100%" OnClick="btnForms_Click" OnLoad="btnForms_Load" />
                <cc1:PLCButton ID="bnSchedule" runat="server" Text="Schedule" PromptCode="TAB1Case.bnSchedule" Width="100%" OnClick="bnSchedule_Click" />
                <cc1:PLCButton ID="bnDispo" runat="server" Text="Retention Review" PromptCode="TAB1Case.bnDispo" Width="100%" OnClick="bnDispo_Click" OnLoad="bnDispo_Load" />
                <cc1:PLCButton ID="bnTeam" runat="server" Text="Team" PromptCode="TAB1Case.bnTeam" Width="100%" OnClick="bnTeam_Click" OnLoad="bnTeam_Load" />
                <cc1:PLCButton ID="bnCaseLock" runat="server" Text="Lock Case" PromptCode="TAB1Case.bnCaseLock.LOCK" Width="100%" OnClick="bnCaseLock_Click" OnLoad="bnCaseLock_Load" />
                <cc1:PLCButton ID="bnCaseReports" runat="server" Text="$CaseTab_CaseReports" Width="100%" OnClick="bnCaseReports_Click" />
                <cc1:PLCButton ID="bnDiscoveryPacket" runat="server" Text="Discovery Packet" PromptCode="TAB1Case.bnDiscoveryPacket" OnClick="bnDiscoveryPacket_Click" Width="100%" Visible="false" />
                <cc1:PLCButton ID="btnNICS" runat="server" Text="NICS" PromptCode="TAB1Case.btnNICS" Width="100%" OnClick="btnNICS_Click" />
                <cc1:PLCButton ID="btnActivity" runat="server" Text="Activity" PromptCode="TAB1Case.btnActivity" Width="100%" OnClick="btnActivity_Click" Visible="false" />
                <cc1:PLCButton ID="btnJIMSReport" runat="server" Text="JIMS" PromptCode="TAB1Case.btnJIMSReport" OnClientClick="btnJIMSReport_Click(); return false;" Width="100%"  Visible="false" />
                <cc1:PLCButton ID="btnFOIA" runat="server" Text="FOIA" PromptCode="TAB1Case.btnJIMSReport" Width="100%" OnClick="btnFOIA_Click" Visible="false"/>
                <cc1:PLCButton ID="btnDistribution" runat="server" Text="Distribution" PromptCode="TAB1Case.btnDistribution" OnClick="btnDistribution_Click" Width="100%"  Visible="false" />
                <cc1:PLCButton ID="btnLaboffense" runat="server" Text="Case Offense" PromptCode="TAB1Case.btnDistribution" OnClick="btnLaboffense_Click" Width="100%" Visible="false" />
                <cc1:PLCButton ID="btnCodisHits" runat="server" Text="Codis Hits" PromptCode="TAB1Case.btnCodisHits" OnClick="btnCodisHits_Click" Width="100%" Visible="false" />
                <cc1:PLCButton ID="btnAddressLabel" runat="server" Text="Address Label" PromptCode="TAB1Case.btnAddressLabel" OnClick="btnAddressLabel_Click" Width="100%" Visible="false"/>
                <cc1:PLCButton ID="btnSeizureTrack" runat="server" Text="Seizure Tracking" PromptCode="TAB1Case.btnSeizureTrack" OnClick="btnSeizureTrack_Click" Width="100%" OnLoad="btnSeizureTrack_Load" Visible="false"/>
                <cc1:PLCButton ID="btnChangeCase" runat="server" Text="$CaseTab_ChangeCase" OnClick="btnChangeCase_Click" Width="100%" style="white-space:normal;"></cc1:PLCButton>           
                <cc1:PLCButton ID="btnDatabankError" runat="server" Text="Databank Error" PromptCode="TAB1Case.btnDatabankError" OnClick="btnError_Click" OnLoad="btnError_Load" Width="100%" style="white-space:normal;" Visible="false" />
                <cc1:PLCButton ID="btnCaseEmail" runat="server" Text="$CaseTab_CaseEmail" Width="100%" style="white-space:normal;" OnClick="btnCaseEmail_Click" Visible="false"/>
                <cc1:PLCButton ID="btnReUpload" runat="server" Text="Re-upload Pending QC Images" PromptCode="TAB1Case.btnReUpload" OnClick="btnReUpload_Click" Width="100%" style="white-space:normal;" />
                <cc1:PLCButton ID="btnCCAP" runat="server" Text="CCAP" PromptCode="TAB1Case.btnCCAP" OnClick="btnCCAP_Click" Width="100%" style="white-space:normal;"></cc1:PLCButton>
                <cc1:PLCButton ID="btnExpunge" runat="server" Text="Expunge" PromptCode="TAB1Case.btnExpunge" OnClick="btnExpunge_Click" Width="100%" style="white-space:normal;" />
                <cc1:PLCButton ID="btnAdminRemoval" runat="server" Text="Admin Removal" PromptCode="TAB1Case.btnAdminRemoval" OnClick="btnExpunge_Click" Width="100%" style="white-space:normal;" />
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
                        <cc1:PLCButton ID="btnOpenCodisHitPopup" runat="server" Text="Open" PromptCode="TAB1Case.btnOpenCodisHitPopup" Visible="true" Enabled="false" style="position:absolute; float:left; margin:5px;" OnClientClick="OpenCodisHitDocument(); return false;" />
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>
     

    <div id="mdialog-change-case-reason" class="change-hide">
        <div id="mdialog-reason-content">
            <asp:UpdatePanel runat="server" id="upChangeCase" UpdateMode="Conditional" ChildrenAsTriggers="true">           
            <ContentTemplate>  
                  <asp:Label runat="server" Text="Please enter reason for change"></asp:Label>
                  <hr />
                  <asp:TextBox runat="server" TextMode="MultiLine" Rows="5" Width="400px" ID="txtChangeCaseReason" onKeyUp="enableDisableOkButton(this);"></asp:TextBox>                  
                  <div class="change-buttons">
                     <cc1:PLCButton runat="server" ID="btnReasonOK" Text="OK" PromptCode="TAB1Case.btnReasonOK" OnClick="btnReasonOK_Click" Enabled="false" />
                     <cc1:PLCButton runat="server" ID="btnReasonCancel" Text="Cancel" PromptCode="TAB1Case.btnReasonCancel" OnClientClick="closeDialog('mdialog-change-case-reason');"/>
                  </div>
            </ContentTemplate>
            </asp:UpdatePanel>
        </div>

    </div>
    

    <div id="mdialog-change-case" class="change-hide">
         <div id="mdialog-change-case-content"> 
            <asp:UpdatePanel runat="server" id="upChangeCasePopup" UpdateMode="Conditional" ChildrenAsTriggers="true">           
                <ContentTemplate>  
                    <div class="change-panel">
                        <asp:Label runat="server" Text="Department Code" Width="220px"></asp:Label>
                        <cc1:FlexBox runat="server" TableName="TV_DEPTNAME" ID="fbDepartmentCode" OnValueChanged="fbDepartmentCode_Changed" Width="250px" AutoPostBack="true" AttachPopupTo="body"></cc1:FlexBox>
                    </div>
                   <div class="change-panel">              
                        <asp:Label runat="server" Text="Department Case Number" Width="220px"></asp:Label>
                        <asp:Textbox runat="server" ID="txtChangeDepartment" Width="370px"
                            onkeydown="onSingleLineTextBoxKeyDown(event);">
                        </asp:Textbox>
                        <ajaxToolkit:MaskedEditExtender ID="meChangeDepartment"  runat="server" ClearMaskOnLostFocus="true"
                             MaskType="None" Mask="!!!!!!!!!!!!!!!!!!!!" TargetControlID="txtChangeDepartment"></ajaxToolkit:MaskedEditExtender>
             
                    </div>
                    <div class="change-buttons">
                        <cc1:PLCButton runat="server" ID="btnUpdateDeptInfo" Text="OK" PromptCode="TAB1Case.btnUpdateDeptInfo" OnClick="btnUpdateDeptInfo_Click"  />
                        <cc1:PLCButton runat="server" ID="btnUpdateCancel" Text="Cancel" PromptCode="TAB1Case.btnUpdateCancel" OnClientClick="closeDialog('mdialog-change-case');" />
                    </div>

                    <PLC:Dialog ID="dlgMessage" runat="server" />
                </ContentTemplate>
          </asp:UpdatePanel>
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
                DataKeyNames="SUPPLEMENT_TABLE,SUPPLEMENT_DESCRIPTION" Height="300px"
                AutoGenerateSelectButton="true" AutoGenerateColumns="false" EmptyDataText="No existing supplement tables found."
                OnSelectedIndexChanged="gvSupplements_SelectedIndexChanged" AllowSorting="True">
                <Columns>
                    <asp:BoundField DataField="SUPPLEMENT_DESCRIPTION" HeaderText="Supplement Description" ItemStyle-Width="250px" />
                    <asp:BoundField DataField="SUPP_CNT" HeaderText="Number of Entries" ItemStyle-Width="170px" />
                </Columns>
            </cc1:PLCGridView>
        </div>
        <div align="center" style="padding: 10px;">
            <cc1:PLCButton ID="btnSuppPrint" runat="server" Text="Print" PromptCode="TAB1Case.btnSuppPrint" Visible="false" />
            <cc1:PLCButton ID="btnSuppOpen" runat="server" Text="Edit Supplement" PromptCode="TAB1Case.btnSuppOpen" OnClick="btnSuppOpen_Click" />
            <cc1:PLCButton ID="btnSuppClose" runat="server" Text="Close" />
            <asp:Button ID="btnDummySuppOK" runat="server" Style="display: none;" />
        </div>
    </asp:Panel>

    <div id="dialog-seizuretrack" title="" style="padding: 10px; width: 720px; height: 600px; display: none;">
            <asp:UpdatePanel ID="upSeizureTrack" runat="server">
                <ContentTemplate>
                    <div class="seizure-grids" style="width: 100%;">
                        <div style="float: left; width: 40%;" id="div-seizure-records" class="seizure-grids">
                            <cc1:PLCDBGrid ID="dbgSeizureTrack" runat="server" AllowSorting="True" AllowPaging="false"
                                PLCGridName="SEIZURE_TRACKING" DataKeyNames="SEIZURE_NBR"
                                PLCGridWidth="100%" EmptyDataText="No records found." HeightOnScrollEnabled="180"
                                OnSelectedIndexChanged="dbgSeizureTrack_SelectedIndexChanged"
                                OnSorted="dbgSeizureTrack_Sorted" OnPageIndexChanged="dbgSeizureTrack_PageIndexChanged">
                            </cc1:PLCDBGrid>
                        </div>
                        <div style="float: right; width: 60%; padding-left: 10PX;" class="seizure-grids">
                            <cc1:PLCDBGrid ID="dbgSeizureItems" runat="server" AllowSorting="false" AllowPaging="false"
                                PLCGridName="SEIZURE_ITEMS" FirstColumnCheckbox="true" CancelPostbackOnClick="true" HeightOnScrollEnabled="180"
                                PLCGridWidth="100%" EmptyDataText="No records found." DataKeyNames="EVIDENCE_CONTROL_NUMBER"
                                OnRowDataBound="dbgSeizureItems_RowDataBound" OnRowCreated="dbgSeizureItems_RowCreated">
                            </cc1:PLCDBGrid>
                        </div>
                    </div>
                    <br />
                    <div class="seizure-panel" style="overflow-y:auto;">
                        <div class="sezure-dbpanel" style="overflow-y:auto;">
                            <cc1:DBPanel ID="dbpSeizureTrack" runat="server" PLCDataTable="TV_SEIZURE" PLCPanelName="SEIZURE_TRACKING"
                                PLCShowEditButtons="True" PLCWhereClause="Where 0 = 1" Width="100%" PLCAttachPopupTo=".popup-ffb"
                                OnPLCDBPanelTextChanged="dbpSeizureTrack_TextChanged">
                            </cc1:DBPanel>
                        </div>
                        <br />
                        <cc1:PLCButtonPanel ID="bpSeizureTrack" runat="server" PLCShowAddButton="true" PLCCustomButtons="Close"
                            PLCTargetControlID="dbpSeizureTrack" Width="100%" PLCShowEditButtons="True" PLCShowDeleteButton="true" PLCDisplayBottomBorder="False"
                            PLCDisplayTopBorder="true" OnPLCButtonClick="bpSeizureTrack_PLCButtonClick" >
                        </cc1:PLCButtonPanel>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
    </div>

    <div class="hide">
        <div class="ccap-search">
            <asp:UpdatePanel runat="server">
                <ContentTemplate>
                    <cc1:PLCDBGrid ID="dbgCCAP" runat="server" PLCGridName="CCAPDATA_SEARCH"
                        DataKeyNames="COUNTY,CCAP_NUMBER" Width="100%" PLCGridWidth="100%" Height="300px"
                        AllowSorting="true" AllowPaging="true" PageSize="8"
                        EmptyDataText="Click 'Search CCAP' to find cases.">
                    </cc1:PLCDBGrid>
                    <div class="hide">
                        <cc1:PLCButton ID="btnSearchCCAP" runat="server" Text="Search CCAP" PromptCode="Tab1Case.btnSearchCCAP" OnClick="btnSearchCCAP_Click" />
                        <cc1:PLCButton ID="btnLinkCCAP" runat="server" Text="Link" PromptCode="Tab1Case.btnLinkCCAP" OnClick="btnLinkCCAP_Click" />
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>

    <PLC:Dialog ID="dlgConfirm" runat="server"  OnConfirmClick="dlgConfirm_Click" OnCancelClick="dlgConfirmCancel_Click" />

    <div ID="div-expunge" Width="500px" Style="display: none;">
        <asp:Panel ID="pnlPopupExpunge" runat="server" Width="415">
            <div style="margin: 5px; width: 100%; float: right;">
                <div class="div-Expunge-Main" style="margin-left: 10px;">
                    <asp:Panel ID="pnlExpungePassword" runat="server">
                        <div class="form-group">
                            <cc1:PLCLabel ID="lblUser" runat="server" Text="Supervisor ID" PromptCode="TAB1CaseCODNA.lblUser" CssClass="label" Width="120px"></cc1:PLCLabel>
                            <asp:TextBox ID="txtUserID" runat="server" Style="text-transform: uppercase;"></asp:TextBox>
                        </div>
                        <div class="form-group">
                            <cc1:PLCLabel ID="lblPassword" runat="server" Text="Password" PromptCode="TAB1CaseCODNA.lblPassword" CssClass="label" Width="120px"></cc1:PLCLabel>
                            <asp:TextBox ID="txtPassword" TextMode="Password" runat="server"></asp:TextBox>
                        </div>
                        <asp:Label ID="lblExpungWarning" runat="server" Text="" ForeColor="Red" Width="350px"
                            Visible="false"></asp:Label>
                        <hr />
                    </asp:Panel>
                    <asp:Label ID="lblExpungeInfo" runat="server" Text="" ForeColor="Red" Font-Bold="true" Width="350px"></asp:Label>
                    <hr />
                    <div class="div-Expunge-reason">
                        <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                            <ContentTemplate>
                                <cc1:FlexBox ID="fbExpungeCode" runat="server" TableName="TV_DBEXPRSN" AutoPostBack="true"
                                    OnValueChanged="fbExpungeCode_SelectedIndexChanged">
                                </cc1:FlexBox>
                                <br />
                                <br />
                                <asp:TextBox ID="txtExpungeComments" runat="server" TextMode="MultiLine" Width="400px"
                                    Rows="5"></asp:TextBox>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </div>
            </div>
            <div align="center" style="padding: 10px;">
                <cc1:PLCButton ID="btnOKExpunge" runat="server" Text="Ok" PromptCode="Tab1Case.btnOKExpunge" Width="100px" OnClick="btnOkExpunge_Click" />
                <cc1:PLCButton ID="btnOKAdminRemoval" runat="server" Text="Ok" PromptCode="Tab1Case.btnOKAdminRemoval" Width="100px" OnClick="btnOkAdminRemoval_Click" />
                <cc1:PLCButton ID="btnCancelExpunge" runat="server" Text="Close" PromptCode="Tab1Case.btnCancelExpunge" Width="100px" />
            </div>
        </asp:Panel>
    </div>

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
    <cc1:MessageBox ID="mbReload" runat="server" PanelCSSClass="modalPopup" CaptionCSSClass="caption" OnOkClick="mbReload_OkClick" />
    <PLC:Dialog ID="dlgMsg" runat="server" OnConfirmClick="dlgMsg_ConfirmClick" />

    <script type="text/javascript">
        var seizGridHeight = .35;
        var seizPanelHeight = .5;
        $(function () {
            var prm = Sys.WebForms.PageRequestManager.getInstance();
            if (prm != null) {
                prm.add_pageLoaded(setSeizureGridsSize);
                prm.add_beginRequest(BeginRequestHandler);
                prm.add_endRequest(EndRequestHandler);
            }

            function BeginRequestHandler(sender, args) {
                xPos = $("#div-seizure-records").scrollLeft();
                yPos = $("#div-seizure-records").scrollTop();
            }
            function EndRequestHandler(sender, args) {
                $("#div-seizure-records").scrollLeft(xPos);
                $("#div-seizure-records").scrollTop(yPos);
            }
        });

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

        function restoreGridScrollById(id) {
            setTimeout(function () {
                setScrollPos(id + "_scroll", id + "_div");
            });
        }

        function resetGridScrollById(id) {
            setTimeout(function () {
                saveScrollPos(id + "_scroll", id + "_div");
            });
        }

        function ShowSeizureTrackPopUp() {
            $("[id='dialog-seizuretrack']:eq(1)").dialog("destroy").remove();
            var $parent = $("#dialog-seizuretrack").parent();
            $("#dialog-seizuretrack").dialog({
                title: "Seizure Tracking",
                resizable: false,
                modal: true,
                draggable: false,
                closeOnEscape: false,
                width: 720,
                height: 600,
                buttons: {
                    
                },
                open: function (event, ui) {
                    var $img = '<div style="padding-left:95%;">' +
                        '<img src = "Images/icon-fullscreen-black.png" style = "height: 25px; width: 25px;" id = "imgSeizureMaximize" onclick = "ResizeSeizurePopup(true);" >' +
                        '<img src = "Images/icon-exit-fullscreen-black.png" style = "height: 25px; width: 25px; display: none;" id = "imgSeizureReg" onclick = "ResizeSeizurePopup(false);" >' +
                    '</div > ';
                    $(this).parent().appendTo("form");
                    $(".ui-dialog-titlebar-close", ui.dialog | ui).hide();
                    $(".ui-dialog-titlebar").append($img)
                    setSeizureGridsSize();

                    setTimeout(function () {
                        $(".popup-ffb").css("position", "absolute").css("top", "0px").css("left", "0px");
                    }, 100);
                },
                close: function () {
                    $(this).parent().appendTo($parent);
                },
                dragStart: function () {
                    $(".popup-ffb .ffb:visible").hide();
                }
            });
            return false;
        }

        function CloseSeizureTrackPopup(withRecord) {
            $("[id*=btnSeizureTrack]").css("color", (withRecord == 'T' ? "red" : ""));
            $('#dialog-seizuretrack').dialog("close");
        }

        function ResizeSeizurePopup(maximize) {
            var popUpHeight = 600;
            var popupWidth = 720;

            if (maximize) {
                popUpHeight = window.innerHeight || Math.max(document.documentElement.clientHeight, document.body.clientHeight);
                popupWidth = window.innerWidth || Math.max(document.documentElement.clientWidth, document.body.clientWidth);
                popupWidth = popupWidth - 20;

                seizGridHeight = .48;
                seizPanelHeight = .40;
            }
            else {
                seizGridHeight = .35;
                seizPanelHeight = .5;
            }

            $("#imgSeizureMaximize").css("display", (maximize ? "none" : ""));
            $("#imgSeizureReg").css("display", (maximize ? "" : "none"));

            $("#dialog-seizuretrack").dialog("option", "height", popUpHeight);
            $("#dialog-seizuretrack").dialog("option", "width", popupWidth);
            $("#dialog-seizuretrack").dialog('widget').position({
               my: "center",
               at: "center",
               of: window
            });

            setSeizureGridsSize();
        }

        function setSeizureGridsSize() {
            $(".seizure-grids").height($("#dialog-seizuretrack").height() * seizGridHeight);
            $(".sezure-dbpanel").height($("#dialog-seizuretrack").height() * seizPanelHeight);

            $('#' + '<%= dbgSeizureItems.ClientID + "_div" %>').css("height", (($("#dialog-seizuretrack").height() * seizGridHeight) - 20) + "px");
            $('#' + '<%= dbgSeizureTrack.ClientID + "_div" %>').css("height", (($("#dialog-seizuretrack").height() * seizGridHeight) - 20) + "px");
            $('#' + '<%= dbgSeizureItems.ClientID + "_header" %>').find("a").attr("href", "#");
        }


        function enableDisableOkButton(obj) {

            if (typeof obj === "object") {

                if (obj.value.trim() === "") {
                    $("[id*=btnReasonOK]").attr("disabled", "disabled");
                }
                else {
                    $("[id*=btnReasonOK]").removeAttr("disabled");
                }

            }


        }

        function closeDialog(dialog) {
            $("#" + dialog).dialog("close");

        }

       
         function showChangeCaseReasonPopup() {

            $("[id='mdialog-change-case-reason']:eq(1)").dialog("destroy").remove();
            $("#mdialog-change-case-reason").dialog({
                closeOnEscape: false,
                autoOpen: true,
                modal: true,
                resizable: false,
                draggable: false,
                title: 'Change Department/Case Number',
                width: 450,
                height: 280,
                open: function () {
                    $(this).parent().appendTo("form");                 
                    $(".ui-dialog-titlebar-close", $(this).parent()).hide(); // Remove close icon  

                    setTimeout(function () {                        
                        setFocusFirstActiveElement($("#mdialog-change-case-reason"));                      
                    }, 200);
                    
                },
                close: function () {
                    $("[id*=txtChangeCaseReason]").val("");                                   
                }
             });         
        }


        function showChangeCasePopup() {
            $("#mdialog-change-case").dialog({
                closeOnEscape: false,
                autoOpen: true,
                modal: true,
                resizable: false,
                draggable: false,
                title: 'Change Department/Case Number',
                width: 640,
                height: 280,
                open: function () {
                    $(this).parent().appendTo("form");                 
                    $(".ui-dialog-titlebar-close", $(this).parent()).hide(); // Remove close icon  
                    closeDialog("mdialog-change-case-reason"); 

                    $("[id$='txtChangeDepartment']").val("");
                    $("[id$='fbDepartmentCode']").setCode("");

                    positionFlexboxOptions($(this));

                    setTimeout(function () {                        
                            setFocusFirstActiveElement($("#mdialog-change-case"));
                    }, 200);
                },
                close: function () {
                     $(this).dialog("destroy").remove();                                                  
                }
            });
        }

        function setFocusFirstActiveElement($dialog) {
            /// <summary>Sets the focus on the first active element.</summary>

            var $element = $dialog
                .find('input:not("[type=hidden]"),textarea')
                .not(':disabled,[readonly],.readonly,:hidden');

            if ($element)
                $element.first().focus();
        }

        function positionFlexboxOptions($divID) {
            setTimeout(function () {
                $("[id*=fbDepartmentCode]").css("z-index", $divID.closest('.ui-dialog').css("z-index") + 1);
            }, 500);
        }


        function showChangeLabCasePopup() {            
            $("#mdialog-change-labcase").dialog({
                closeOnEscape: false,
                autoOpen: true,
                modal: true,
                resizable: false,
                draggable: false,
                width: 400,
                open: function () {
                    $(this).parent().appendTo("form");                 
                    $(".ui-dialog-titlebar-close", $(this).parent()).hide(); // Remove close icon  

                    // workaround for flexbox issue gets stuck behind popup if there are multiple jquery dialog with flexbox
                    $(".popup-ffb").attr("style", "left: 0px; top: 0px; position: absolute; z-index: 110000 !important;").appendTo(window.top.document.body);

                    var $labcasetxtbox = GetDBPanelElement("PLCDBPanel1", "TV_LABCASE.LAB_CASE");
                    if ($labcasetxtbox.length > 0) {
                        var labcasetxtbox = $labcasetxtbox.get(0);
                        $("[id*=labcasenolabel]").text(labcasetxtbox.value);
                    }

                    setTimeout(function () {
                        checkLabCtrlAsync("NO_LABCASE_ON_CASE_CREATE", function (isNoLabCase) {
                            if (isNoLabCase)
                                setLabCasePopupValues();
                        });
                    }, 100);

                    setTimeout(function () {                        
                            setFocusFirstActiveElement($("#mdialog-change-labcase"));
                    }, 200);
                },
                close: function () {

                    $(this).dialog("close");                                                 
                }
            });
        }

        
        function closeLabCaseDialog(dialog) {
            EnableLabCaseNumberChangeButton(true);
            $("#" + dialog).dialog("destroy");             
        }

        function setMonthYearValueBasedOnOffense() {
            var offenseDateValue = GetDBPanelField("PLCDBPanel1", "TV_LABCASE.OFFENSE_DATE");
            var offense = $("[id*=hdnCaseOffenseDate]").val();

            if (offenseDateValue)
                offense = offenseDateValue;

            if (offense) {
                var parts = offense.split("/");
                var month;
                var year;
                var mask = GetDBPanelMask("PLCDBPanel1", "TV_LABCASE.OFFENSE_DATE");
                if (mask.toLowerCase() == "mm/dd/yyyy") {
                    month = parseInt(parts[0], 10);
                    year = parseInt(parts[2], 10);
                }
                else if (mask.toLowerCase() == "dd/mm/yyyy") {
                    month = parseInt(parts[1], 10);
                    year = parseInt(parts[2], 10);
                }

                var panelName = "dbpChangeLabCase";
                if (month) {
                    var fieldName = "TV_LABCASE.LAB_CASE_MONTH";
                    var id = GetDBPanelClientID(panelName, fieldName);
                    if (id) {
                        SetAttribute($("#" + id), "disabled", "disabled", { "background-color": "#C0C0C0" }, true);
                        SetDBPanelField(panelName, fieldName, month);
                    }
                }

                if (year) {
                    var fieldName = "TV_LABCASE.LAB_CASE_YEAR";
                    var id = GetDBPanelClientID(panelName, fieldName);
                    if (id) {
                        SetAttribute($("#" + id), "disabled", "disabled", { "background-color": "#C0C0C0" }, true);
                        SetDBPanelField(panelName, fieldName, year);
                    }
                }
            }
        }

        function setCaseLabCodeValue() {
            var panelLabCodeValue = GetDBPanelField("PLCDBPanel1", "TV_LABCASE.LAB_CODE");
            var labCode = $("[id*=hdnCaseLabCode]").val();
            var panelName = "dbpChangeLabCase";
            if (panelLabCodeValue)
                labCode = panelLabCodeValue;

            if (labCode) {
                  SetDBPanelField(panelName, "TV_LABCASE.LAB_CODE", labCode);
            }


        }

        function setLabCasePopupValues()
        {
            setMonthYearValueBasedOnOffense();
            setCaseLabCodeValue();
        }

        //#region CCAP
        var dlgCCAPSearch;

        function closeCCAPSearch() {
            dlgCCAPSearch.close();
        }

        function showCCAPSearch() {
            var div = document.querySelector(".ccap-search");
            dlgCCAPSearch = new Dialog(div, {
                ui: {
                    title: "Search CCAP",
                    buttons: {
                        "Search CCAP": function () {
                            $("[id$='btnSearchCCAP']").click();
                        },
                        "Link": function () {
                            $("[id$='btnLinkCCAP']").click();
                        },
                        "Close": function () {
                            dlgCCAPSearch.close();
                        }
                    }
                },
                spacer: "Link",
                updatePanel: true
            });
            dlgCCAPSearch.openCenter();
        }
        //#endregion CCAP

        
        function SetAttribute($element, attributeKey, attributeVal, cssConfig, isSetMode) {
            if ($element != null) {
                // set/remove HTML attr
                if (attributeKey != null) {
                    if (isSetMode)
                        $element.attr(attributeKey, attributeVal);
                    else
                        $element.removeAttr(attributeKey);
                }

                // set CSS attr
                if (cssConfig != null)
                    $element.css(cssConfig);
            }
        }

        function showExpungePopup(title) {
            $("[id='div-expunge']:eq(1)").dialog("destroy").remove();
            $("#div-expunge").dialog({
                title: title,
                resizable: false,
                autoOpen: true,
                modal: true,
                draggable: true,
                closeOnEscape: false,
                width: 500,
                height: 400,
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

    </script>

    <asp:HiddenField ID="hdnDBPanelClientIDsScript" runat="server" />
</asp:Content>
