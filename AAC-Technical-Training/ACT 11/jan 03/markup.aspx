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

                <fieldset>
                    <legend>Case Report</legend>
                    <PLC:DBPanel ID="dbpLabCase" PLCPanelName="CASETAB" runat="server">
                    </PLC:DBPanel>
                </fieldset>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="foot" runat="server">
</asp:Content>
