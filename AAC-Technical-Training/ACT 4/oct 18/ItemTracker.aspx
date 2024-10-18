<%@ Page Title="" Language="C#" MasterPageFile="~/LIMSHOME.master" AutoEventWireup="true" CodeBehind="ItemTracker.aspx.cs" Inherits="BEASTiLIMS.ItemTracker" %>
<%@ Register assembly="PLCCONTROLS" namespace="PLCCONTROLS" tagprefix="PLC" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
	<style>
		.divSearchButtons{
			padding-left: 10px;
		}
		.itemTrackerLabel{
			padding: 5px 5px 5px 5px;
			font-size: larger;
		}
	</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cp" runat="server">
	<p class="sectionheader">
		<asp:Label ID="lblItemTrackSection" runat="server" Text="Item Tracker"></asp:Label>
	</p>

    <div class="divSearchDBPanel">
        <PLC:PLCDBPanel ID="dbpSearchItems" PLCWhereClause="WHERE 1=0" PLCPanelName="ITEM_TRACKER"
            runat="server" IsSearchPanel="true" PLCAttachPopupTo="body">
        </PLC:PLCDBPanel>
    </div>

    <hr />
    <div class="divSearchButtons">
        <asp:Button runat="server" ID="btnSearch" Text="Search" Width="100" OnClick="btnSearch_Click" />
        <asp:Button runat="server" ID="btnOpen" Text="Open" Width="100" OnClick="btnOpen_Click" />
        <asp:Button runat="server" ID="btnPrintList" Text="Print List" Enabled="false" Width="100" OnClick="btnPrintList_Click" />
        <asp:Button runat="server" ID="btnPrintInventory" Enabled="false" Text="Inventory History" Width="130" OnClick="btnPrintInventory_Click" />
        <asp:Button runat="server" ID="btnClear" Text="Clear" Width="100" OnClick="btnClear_Click" />
    </div>

	<hr />
    <div class="divItems">
        <div class="itemTrackerLabel">
            <asp:Label ID="lblItems" runat="server" Text="Items"></asp:Label>
        </div>
        <PLC:PLCDBGrid ID="dbgItems" runat="server" AllowSorting="True" AllowPaging="True"
            PageSize="5" PLCGridWidth="100%" DataKeyNames="EVIDENCE_CONTROL_NUMBER" EmptyDataText="Select criteria then click 'Search' button."
            PLCGridName="ITEM_TRACKER" OnSelectedIndexChanged="dbgItems_SelectedIndexChanged" OnPageIndexChanged="dbgItems_PageIndexChanged"
            OnRowCreated="dbgItems_RowCreated" OnSorted="dbgItems_Sorted">
        </PLC:PLCDBGrid>
    </div>
	
	<div class="divWorkSheets">
        <div class="itemTrackerLabel">
            <asp:Label ID="lblWorkSheets" runat="server" Text="Work Sheets"></asp:Label>
        </div>
        <PLC:PLCDBGrid ID="dbgWorkSheets" runat="server" AllowSorting="True" AllowPaging="True"
            PageSize="5" PLCGridWidth="100%" EmptyDataText="No existing work sheets." OnSorted="dbgWorkSheets_Sorted"
            PLCGridName="ITEM_TRACKER_WORKSHEETS" OnPageIndexChanged="dbgWorkSheets_PageIndexChanged" OnRowCreated="dbgWorkSheets_RowCreated">
        </PLC:PLCDBGrid>   
    </div>

	<PLC:MessageBox runat="server" ID="msgBox" PanelCSSClass="modalPopup" CaptionCSSClass="caption" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="foot" runat="server">
</asp:Content>
