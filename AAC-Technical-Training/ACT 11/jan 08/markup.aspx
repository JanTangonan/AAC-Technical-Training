<%@ Page Title="" Language="C#" MasterPageFile="~/LIMSHOME.master" AutoEventWireup="true" CodeBehind="Exercise_Arvin_Reports.aspx.cs" Inherits="BEASTiLIMS.Exercise_Arvin_Reports" %>
<%@ Register Assembly="PLCCONTROLS" Namespace="PLCCONTROLS" TagPrefix="PLC" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cp" runat="server">
    <asp:UpdatePanel ID="UpdatePanel" runat="server">
        <ContentTemplate>
            <PLC:PLCDBGrid ID="dbgLabCase" runat="server" AutoGenerateColumns="false" CssClass="grid" DataKeyNames="CASE_KEY" AutoPostBack="true"
                BorderStyle="None" BorderWidth="0px" BorderColor="Transparent" SelectedRowStyle-BackColor="#7FFFD4"
                OnSelectedIndexChanged="dbgLabCase_SelectedIndexChanged" PLCGridName="TEST_NICE_NOICE" PLCGridWidth="100%">
            </PLC:PLCDBGrid>

            <PLC:PLCDBGrid ID="dbgLabItem" runat="server" AutoGenerateColumns="false" CssClass="grid" DataKeyNames="EVIDENCE_CONTROL_NUMBER" AutoPostBack="true"
                BorderStyle="None" BorderWidth="0px" BorderColor="Transparent" SelectedRowStyle-BackColor="#7FFFD4"
                OnSelectedIndexChanged="dbgLabItem_SelectedIndexChanged" PLCGridWidth="100%">
            </PLC:PLCDBGrid>

            <asp:Button  ID="btnPrintCaseDetails" runat="server" Text="Print Case Details" OnClick="btnPrintCaseDetails_Click" Width="130px"/>
            <asp:Button  ID="btnPrintItemDetails" runat="server" Text="Print Item Details" OnClick="btnPrintItemDetails_Click" Width="130px"/>
                
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="foot" runat="server">
</asp:Content>
