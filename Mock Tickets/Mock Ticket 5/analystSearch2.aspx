<%@ Page Title="" Language="C#" MasterPageFile="~/LIMSHOME.master" AutoEventWireup="true" CodeBehind="AnalystSearch.aspx.cs" Inherits="BEASTiLIMS.AnalystSearch" %>
<%@ Register Assembly="PLCCONTROLS" Namespace="PLCCONTROLS" TagPrefix="PLC" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cp" runat="server">
    <p class="sectionheader">
        <PLC:PLCLabel ID="lblAnalystSearch" runat="server" Text="Analyst Search" PromptCode="AnalystSearch.lblAnalystSearch"></PLC:PLCLabel>
    </p>
    <asp:UpdatePanel ID="UpdatePanel" runat="server">
        <ContentTemplate>
            <fieldset>
                <PLC:DBPanel ID="dbpAnalystSearch" PLCPanelName="ANALSEARCH" runat="server">
                </PLC:DBPanel>
            </fieldset>

            <PLC:PLCButtonPanel ID="bpAnalystSearch" runat="server" OnPLCButtonClick="bpAnalystSearch_PLCButtonClick" PLCShowSearchButton="True" PLCShowClearButton="True"
                PLCTargetControlID="dbpAnalystSearch" PLCTargetDBGridID="dbgAnalystSearchResult">
            </PLC:PLCButtonPanel>

            <PLC:PLCDBGrid ID="dbgAnalystSearchResult" runat="server" AutoGenerateColumns="false" AutoPostBack="true"
                BorderStyle="None" BorderWidth="0px" BorderColor="Transparent" SelectedRowStyle-BackColor="#7FFFD4"
                PLCGridWidth="100%" PLCGridName="ANALSEARCH">
            </PLC:PLCDBGrid>

            <fieldset>
                <PLC:DBPanel ID="dbpCaseSearch" PLCPanelName="SEARCH_BY_CASE" runat="server">
                </PLC:DBPanel>
            </fieldset>

            <PLC:PLCButtonPanel ID="bpCaseSearch" runat="server" OnPLCButtonClick="bpCaseSearch_PLCButtonClick" PLCShowSearchButton="True" PLCShowClearButton="True"
                PLCTargetControlID="dbpCaseSearch" PLCTargetDBGridID="dbgCaseSearchResult">
            </PLC:PLCButtonPanel>

            <PLC:PLCDBGrid ID="dbgCaseSearchResult" runat="server" AutoGenerateColumns="false" AutoPostBack="true"
                BorderStyle="None" BorderWidth="0px" BorderColor="Transparent" SelectedRowStyle-BackColor="#7FFFD4"
                PLCGridWidth="100%" PLCGridName="SEARCH_BY_CASE">
            </PLC:PLCDBGrid>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="foot" runat="server">
</asp:Content>
