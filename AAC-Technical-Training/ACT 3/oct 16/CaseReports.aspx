<%@ Page Title="Case Reports" Language="C#" MasterPageFile="~/CASEFILE.master" AutoEventWireup="true" CodeBehind="CaseReports.aspx.cs" Inherits="BEASTiLIMS.CaseReports" %>
<%@ Register Assembly="PLCCONTROLS" Namespace="PLCCONTROLS" TagPrefix="PLC" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cp" runat="server">
    <asp:PlaceHolder ID="plhMessageBox" runat="server"></asp:PlaceHolder>
    <br />
    <div style="height:270px; width:100%" >
    <asp:Panel ID="pnlGrid" runat="server">
        <asp:GridView ID="grdReports" runat="server" DataKeyNames="REPORTFILE" AutoGenerateColumns="false"
            AutoGenerateSelectButton="true" Width="600px" EmptyDataText="No Custom Reports found." OnSelectedIndexChanged="grdReports_IndexChanged">
            <Columns>
                <asp:BoundField HeaderText="Report" DataField="REPORTNAME" />
            </Columns>
        </asp:GridView>
    </asp:Panel>
    <asp:Panel ID="pnlReportParams" runat="server">
        <p>
            <b>Report: </b>
            <asp:Label ID="lblReport" runat="server"></asp:Label>
            <asp:HiddenField ID="hdnReportFile" runat="server" />
        </p>
        <p>
            <asp:Panel ID="pnlParameters" runat="server" GroupingText="Enter parameters for the report">
                <asp:Repeater ID="repParameters" runat="server" OnItemDataBound="repParameters_ItemDataBound" EnableViewState="true">
                    <HeaderTemplate>
                        <table style="margin: 10px;">
                    </HeaderTemplate>
                    <ItemTemplate>
                            <tr style="display: <%# (Eval("Type").ToString() == "ALWAYS" || Eval("Type").ToString() == "CODE_ALWAYS") ? "none" : "block" %>">
                            <td align="right">
                                <asp:Label ID="lblParameter" runat="server" Text='<%# Eval("Name") %>'></asp:Label>:
                                <asp:HiddenField ID="hdnField" runat="server" Value='<%# Eval("Field") %>' />
                            </td>
                            <td align="left">
                                <asp:TextBox ID="txtValue" runat="server" Visible='<%# Eval("Type").ToString() == "TEXT" || Eval("Type").ToString() == "DATE" %>'
                                    Text='<%# Eval("Value") %>'></asp:TextBox>
                                <ajaxToolkit:MaskedEditExtender ID="meeDate" runat="server" Mask="99/99/9999" MaskType="Date"
                                    TargetControlID="txtValue" Enabled='<%# Eval("Type").ToString() == "DATE" %>'>
                                </ajaxToolkit:MaskedEditExtender>
                                <asp:HiddenField ID="hdnLookup" runat="server" Value='<%# Eval("Lookup").ToString() %>' />
                                <asp:HiddenField ID="hdnValue" runat="server" Value='<%# Eval("Value").ToString() %>' />
                                <asp:PlaceHolder ID="plhLookup" runat="server" Visible='<%# Eval("Type").ToString() == "CODE" || Eval("Type").ToString() == "CODE_ALWAYS" %>'>
                                </asp:PlaceHolder>
                                <asp:PlaceHolder ID="plhDateRange" runat="server" Visible='<%# Eval("Type").ToString() == "DATERANGE" %>'>
                                    <table cellpadding="0" cellspacing="0">
                                        <tr>
                                            <td>
                                                from&nbsp;
                                            </td>
                                            <td>
                                                <asp:TextBox ID="txtDateStart" runat="server" Text='<%# GetDate(Eval("Value").ToString(), 0) %>'></asp:TextBox>
                                                <ajaxToolkit:MaskedEditExtender ID="meeDateStart" runat="server" Mask="99/99/9999"
                                                    MaskType="Date" TargetControlID="txtDateStart">
                                                </ajaxToolkit:MaskedEditExtender>
                                            </td>
                                            <td>
                                                &nbsp;to&nbsp;
                                            </td>
                                            <td>
                                                <asp:TextBox ID="txtDateEnd" runat="server" Text='<%# GetDate(Eval("Value").ToString(), 1) %>'></asp:TextBox>
                                                <ajaxToolkit:MaskedEditExtender ID="meeDateEnd" runat="server" Mask="99/99/9999"
                                                    MaskType="Date" TargetControlID="txtDateEnd">
                                                </ajaxToolkit:MaskedEditExtender>
                                            </td>
                                        </tr>
                                    </table>
                                </asp:PlaceHolder>
                                <asp:HiddenField ID="hdnType" runat="server" Value='<%# Eval("Type") %>' />
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                        </table>
                    </FooterTemplate>
                </asp:Repeater>
            </asp:Panel>
        </p>
        <br />
    </asp:Panel>
    </div>
    <asp:Panel ID="pnlReportButtons" runat="server" >
        <hr />
        <asp:Button ID="btnViewReport" runat="server" Text="View Report" Width="100"  OnClick="btnViewReport_Click" />
        <asp:Button ID="btnXLSExport" runat="server" Text="Export to XLS" Width="100" OnClick="btnXLSExport_Click" Visible="false" />
        <asp:Button ID="btnPDFExport" runat="server" Text="Export to PDF" Width="100" OnClick="btnPDFExport_Click" Visible="false" />
        <asp:Button ID="btnShowFormula" runat="server" Text="Show Formula" Width="100" OnClick="btnShowFormula_Click" />
        <hr />
    </asp:Panel>
</asp:Content>