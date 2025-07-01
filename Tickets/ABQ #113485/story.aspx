<%@ Page Language="C#" MasterPageFile="~/CASEFILE.master" AutoEventWireup="true" CodeBehind="Story.aspx.cs" Inherits="BEASTiLIMS.Story" Title="Schedule" %>

<%@ Register Assembly="PLCCONTROLS" Namespace="PLCCONTROLS" TagPrefix="PLC" %>



<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">  
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cp" runat="server">
    <PLC:PLCSessionVars ID="PLCSessionVars1" runat="server" />
    <asp:PlaceHolder ID="phMsgBox" runat="server"></asp:PlaceHolder>
    <PLC:PLCDataSource ID="dsSchedule" runat="server" />
    
    <asp:Panel ID="pnlFilters" runat="server">
        <table style="width:100%;">
            <tr>
                <td><asp:Label ID="lblAnalyst" runat="server" Text="User"/></td>
                <td style="width:50%; text-align:left">
                    <asp:HiddenField ID="hfPropAnalyst" runat="server" />
                    <PLC:FlexBox ID="fbAnalyst" runat="server" Width="200px" AutoPostBack="true" OnValueChanged="Flexbox_ValueChanged" />
                </td>
                <td style="width:50%; text-align:right">
                    <asp:Button ID="btnAllAnalyst" runat="server" Text="Show All" OnClick="btnAllAnalyst_Click" Width="120" />
                </td>
            </tr>
            <tr>
                <td><asp:Label ID="lblType" runat="server" Text="Type"/></td>
                <td style="width:50%; text-align:left">
                    <asp:HiddenField ID="hfPropType" runat="server" />
                    <PLC:FlexBox ID="fbType" runat="server" TableName="TV_SCHTYPE" Width="200px" AutoPostBack="true" OnValueChanged="Flexbox_ValueChanged" />
                </td>
                <td style="width:50%; text-align:right">
                    <asp:Button ID="btnCurrentAnalyst" runat="server" Text="Show Mine" OnClick="btnCurrentAnalyst_Click" Width="120" />
                </td>
            </tr>
        </table>
    </asp:Panel>
    <asp:Panel ID="pnlScheduleSearch" runat="server">
        <PLC:PLCDBPanel ID="dbpScheduleSearch" PLCWhereClause="WHERE 1=0" PLCPanelName="SCHEDULE_SEARCH" runat="server" IsSearchPanel="true" PLCAttachPopupTo="body" 
            DefaultButton="btnScheduleSearch">
        </PLC:PLCDBPanel>
        <br />
        <asp:Button ID="btnScheduleSearch" runat="server" Text="Search" OnClick="btnScheduleSearch_Click" />
        <asp:Button ID="btnCurrentAnalystSearch" runat="server" Text="Show Mine" OnClick="btnCurrentAnalystSearch_Click" Width="120" />
        <asp:Button ID="btnAllAnalystSearch" runat="server" Text="Show All" OnClick="btnAllAnalystSearch_Click" Width="120" />
    </asp:Panel>
    <hr />

    <PLC:PLCDBGrid ID="GridView1" runat="server" DataKeyNames="SCHEDULE_KEY" Width="100%" PLCGridName="SCHEDULE"
            AllowPaging="True" AllowSorting="True" AutoGenerateColumns="False" onrowdatabound="GridView1_RowDataBound" OnSorted="GridView1_Sorted" 
            AutoGenerateSelectButton="True" onselectedindexchanged="GridView1_SelectedIndexChanged" OnPageIndexChanged="GridView1_PageIndexChanged"
            EmptyDataText="No data found">
    </PLC:PLCDBGrid>    
  
    <PLC:DBPanel ID="PLCDBPanel1" runat="server" PLCDataTable="TV_SCHEDULE" 
        PLCPanelName="PANEL_STORY" PLCWhereClause="Where 0 = 1" 
        onplcdbpanelgetnewrecord="PLCDBPanel1_PLCDBPanelGetNewRecord" 
        PLCDisplayTopBorder="True" Width="100%"
        PLCAuditCode="41" PLCAuditSubCode="1" PLCDeleteAuditCode="41" PLCDeleteAuditSubCode="2">
    </PLC:DBPanel>        
    
    <PLC:PLCButtonPanel ID="PLCButtonPanel1" runat="server" PLCDisplayBottomBorder="True" PLCDisplayTopBorder="True" 
    PLCShowDeleteButton="True" PLCShowEditButtons="True" PLCShowAddButton="true" PLCTargetControlID="PLCDBPanel1" Width="100%" 
        PLCCustomButtons="Print,Summary,Back to Case,Back to Checklist" PLCCustomButtonFixedWidth="70" onplcbuttonclick="PLCButtonPanel1_PLCButtonClick" plc>
    </PLC:PLCButtonPanel>  
    
    <asp:HiddenField ID="hdnFilterCaseEvents" runat="server" Value="" />
</asp:Content>
