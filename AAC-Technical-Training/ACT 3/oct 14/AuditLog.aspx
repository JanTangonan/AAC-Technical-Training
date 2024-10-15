<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AuditLog.aspx.cs" Inherits="BEASTiLIMS.AuditLog"
    MasterPageFile="~/LIMSHOME.master" Title="Audit Log" %>


<%@ Register Src="~/PLCWebCommon/PLCCaseSearch.ascx" TagName="CaseSearch" TagPrefix="uc1" %>
<%@ Register Src="~/PLCWebCommon/CodeHead.ascx" TagName="CodeHead" TagPrefix="uc2" %>
<%@ Register Src="~/PLCWebCommon/PLCPageHead.ascx" TagName="PLCPageHead" TagPrefix="uc3" %>
<%@ Register Assembly="PLCCONTROLS" Namespace="PLCCONTROLS" TagPrefix="PLC" %>
<%@ Register Src="~/PLCWebCommon/PLCDialog.ascx" TagName="Dialog" TagPrefix="PLCDialog" %>


<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <script language="javascript" type="text/javascript">
        var curRow = null;
        function ClickGridRow(grID, rowIdx, logStamp, hasRowID) {
            // Disable item button. It will be reenabled if the auditlog ecn is valid.
            $("[id$='btnItem']").attr("disabled", "disabled");
            $("[id$='hdnSelECN']").val("");

            if(hasRowID != "T")
                $("[id*='hdnRowID']").val(rowIdx - 1);

            var selRow = GetSelectedGridRow(grID, rowIdx);
            if (curRow != null) {
                curRow.style.backgroundColor = '#F7F6F3';
                curRow.style.color = '#333333';
                curRow.style.fontWeight = "normal";
            }
            if (selRow != null) {
                curRow = selRow;
                curRow.style.backgroundColor = '#E2DED6';
                curRow.style.color = '#333333';
                curRow.style.fontWeight = "bold";

                if(logStamp != "")
                {
                    ViewDetails(logStamp);
                }
                else
                {
                    var $logid =  $(selRow).find(".dgcell-LOG_STAMP");
                    if ($logid.length > 0) {
                        ViewDetails($logid.attr("value"));
                    }
                }              
              
                return true;
            }
            return false;
        }
        function GetSelectedGridRow(grID, rowIdx) {
            var gridView = document.getElementById(grID);
            if (gridView != null) {
                return gridView.rows[rowIdx];
            }
            return null;
        }
        function ViewDetails(logid) {
            ShowDetails("", "", "", "");
            var params = { logid: logid };
            PLCWebCommon.ScriptServiceWebMethods.ViewLogDetails(params, ViewDetailsOk, ViewDetailsFail);
        }
        function ViewDetailsOk(results) {
            ShowDetails(results.details, results.addlinfo, results.source, results.reason);

            var $btnItem = $("[id$='btnItem']");
            var ecn = results.ecn;

            // If valid ECN, enable the item button.
            if (ecn != "" && isValidAuditLogECN(ecn)) {
                $btnItem.attr("disabled", "");
                $("[id$='hdnSelECN']").val(ecn);
            }
        };
        function ViewDetailsFail(results) {
            alert('Error found in selected record.');
        };
        function ShowDetails(details, addl, source, reason) {
            $("#txtDetails").val(details);
            $("#txtAddlInfo").val(addl);
            $("#txtSource").val(source);
            $("#txtReason").val(reason);
        }

        function isValidAuditLogECN(ecn) {
            if (ecn == "")
                return false;

            var ecnInt = parseInt(ecn, 10);
            if (ecnInt == NaN)
                return false;

            return true;
        }

        // Do an empty postback that doesn't do anything.
        // Needed as a workaround for the issue where the second date textbox does not have the mask 
        // on initial page load.
        function doPostback() {
            <%= ClientScript.GetPostBackEventReference(btnDoPostback, String.Empty) %>;
        }

        function contentPageLoad() {
            $("#[id*='CaseSearch1']:text").blur(function () {
                $(this).val($.trim($(this).val()));
            });
        }
    </script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cp" runat="server">
  <uc3:PLCPageHead runat="server" ID="phPrelog" include="codemultipick,jquery-ui,utility">
    </uc3:PLCPageHead>
     <div style="min-height: 30px; width: 100%;">
        <asp:Menu ID="Menu1" runat="server" Orientation="Horizontal" SkinID="TopMenu" OnMenuItemClick="Menu1_MenuItemClick">
            <Items>
                <asp:MenuItem Text="Audit Log" Value="0"></asp:MenuItem>
                <asp:MenuItem Text="Audit Web" Value="1"></asp:MenuItem>
            </Items>
        </asp:Menu>
        <div style="border: 1px solid e8e9e4; margin: 0; padding: 0;" />
    </div>
    <asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
        <asp:View ID="View1" runat="server">
              <table cellpadding="0" cellspacing="0">
                <tr>
                   <td>
                       <asp:Label ID="lblURN" runat="server" Text=""></asp:Label>
                   </td>
                </tr>
                <tr>
                    <td></td>
                </tr>
                <tr>
                   <td>
                     <asp:Panel ID="pnlSearch" runat="server" Width="100%" GroupingText="Enter search values">
                        <PLC:PLCDBPanel ID="dbpAuditLog" PLCWhereClause="WHERE 1=0" PLCPanelName="AUDITLOG" runat="server" IsSearchPanel="true" PLCAttachPopupTo="body" >
                        </PLC:PLCDBPanel>
                        <table>
                            <tr>
                                <td colspan="2">
                                    <asp:Button ID="btnClear" runat="server" Text="Clear" Width="120" OnClick="btnClear_Click" />
                                    <asp:Button ID="btnSearch" runat="server" Text="Search" Width="120" OnClick="btnSearch_Click" />
                                    <asp:Button ID="btnAdvanced" runat="server" Text="Adv Case Search..." OnClick="btnFindCase_Click" />
                                </td>
                                <td>
                                </td>
                                <td align="right">
                                    <asp:Button ID="btnItem" runat="server" Text="Item" Width="100"  Enabled="false"
                                        OnClick="btnItem_Click" />
                                    <asp:Button ID="btnPrint" runat="server" Text="Print" Width="100" Visible="false"
                                        OnClick="btnPrint_Click" />
                                </td>
                            </tr>
                        </table>
                    </asp:Panel>
                </td>
                </tr>
                <tr>
                    <td>
                        <asp:Panel ID="pnlGridLogs" runat="server" >
                            <PLC:PLCDBGrid  ID="grdLogs" runat="server" DataKeyNames="LOG_STAMP, DEPARTMENT_CASE_NUMBER, EVIDENCE_CONTROL_NUMBER"
                                Width="100%" AllowPaging="True" AllowSorting="True"
                                OnSelectedIndexChanged="grdLogs_SelectedIndexChanged" PageSize="10" OnRowCreated="grdLogs_RowCreated"
                                OnSorted="grdLogs_Sorted" OnPageIndexChanged="grdLogs_PageIndexChanged"
                                EmptyDataText="" PLCGridName="AUDITLOG">
                            </PLC:PLCDBGrid>
                        </asp:Panel>  
                    </td>             
                </tr>
                <tr>
                    <td>
                         <asp:PlaceHolder ID="plhDetails" runat="server" Visible="false">
                            <table width="800" cellpadding="2">
                                <tr>
                                    <td width="50%">
                                        <b>Description</b>
                                        <br />
                                        <textarea id="txtDetails" readonly="readonly" rows="6" style="width: 99%;" class="default"></textarea>
                                    </td>
                                    <td width="50%">
                                        <b>Details</b>
                                        <br />
                                        <textarea id="txtAddlInfo" readonly="readonly" rows="6" style="width: 100%;" class="default"></textarea>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <b>Source Process</b>
                                        <br />
                                        <input id="txtSource" readonly="readonly" style="width: 100%;" class="default" />
                                    </td>
                                    <td>&nbsp;</td>
                                </tr>
                                <tr>
                                    <td>
                                        <b>Reason For Change</b>
                                        <br />
                                        <textarea id="txtReason" readonly="readonly" rows="6" style="width: 100%;" class="default"></textarea>
                                    </td>
                                    <td>&nbsp;</td>
                                </tr>
                            </table>
                        </asp:PlaceHolder>
                        
                    </td>
                </tr>
            </table>
        </asp:View>
        <asp:View ID="View2" runat="server">
           <table cellpadding="0" cellspacing="0">
               <tr>
                <td>         
                    <asp:Panel ID="pnlSearchWeb" runat="server" Width="100%">
                        <PLC:DBPanel ID="dbpAuditWeb" PLCWhereClause="WHERE 1=0" PLCPanelName="LIMSAUDITWEB_SEARCH" runat="server" IsSearchPanel="true" PLCAttachPopupTo="body">
                        </PLC:DBPanel>
                        <br />
                        <table>
                            <tr>
                                <td colspan="2">
                                    <asp:Button ID="btnClearAuditWeb" runat="server" Text="Clear" Width="120" OnClick="btnClearAuditWeb_Click" />
                                    <asp:Button ID="btnSearchAuditWeb" runat="server" Text="Search" Width="120" OnClick="btnSearchAuditWeb_Click" />
                                </td>
                                <td>
                                </td>
                                <td align="right">
                                    <asp:Button ID="btnPrintAuditWeb" runat="server" Text="Print" Width="100" Visible="false"
                                        OnClick="btnPrintAuditWeb_Click" />
                                </td>
                            </tr>
                        </table>
                    </asp:Panel>
                 </td>
               </tr>
               <tr>
                <td>
                    <div class="dbgridblk" style="padding: 10px; width: 100%">
                        <asp:Panel ID="pnlGridLogsWeb" runat="server" >
                            <PLC:PLCDBGrid  ID="grdAuditWebLogs" runat="server" DataKeyNames="LOG_STAMP, CASE_KEY, EVIDENCE_CONTROL_NUMBER"
                                Width="100%" AllowPaging="True" AllowSorting="True"
                                OnSelectedIndexChanged="grdAuditWebLogs_SelectedIndexChanged" OnSorted="grdAuditWebLogs_Sorted" OnPageIndexChanged="grdAuditWebLogs_PageIndexChanged" PageSize="10"
                                EmptyDataText="No logs found." PLCGridName="LIMSAUDITWEB">
                            </PLC:PLCDBGrid>
                        </asp:Panel>
                    </div>
                </td>
               </tr>
               <tr>
                <td>
                    <asp:PlaceHolder ID="plhDetailsWeb" runat="server" Visible="false">   
                        <div class="dbbox">    
                            <PLC:DBPanel ID="dbpAuditWebLogs" PLCDataTable="TV_AUDITWEB" PLCWhereClause="WHERE 1=0" PLCPanelName="LIMSAUDITWEB" runat="server" PLCAttachPopupTo="body">
                            </PLC:DBPanel>
                        </div>
                    </asp:PlaceHolder>
                </td>
               </tr>
           </table>
        </asp:View>
    </asp:MultiView>
     <asp:UpdatePanel ID="upDialog" runat="server">
        <ContentTemplate>
            <PLCDialog:Dialog ID="dlgMessage" runat="server" />
        </ContentTemplate>
    </asp:UpdatePanel>
    <asp:HiddenField ID="hdnSelECN" runat="server" Value="" />
    <asp:HiddenField ID="hdnRowID" runat="server" Value="" />
    <asp:Button ID="btnDoPostback" runat="server" OnClick="btnDoPostback_Click" Style="display: none;" /> 
    <uc1:CaseSearch ID="CaseSearch1" runat="server" OnSelectedCaseKeyChanged="CaseSearch1_SelectedCaseKeyChanged"  />
    <asp:PlaceHolder ID="phMessageBox" runat="server"></asp:PlaceHolder>
</asp:Content>
