<%@ Page Language="C#" MasterPageFile="~/CASEFILE.master" AutoEventWireup="True"
    CodeBehind="Sample.aspx.cs" Inherits="BEASTiLIMS.Sample" Title="Sample" %>

<%@ Register Assembly="PLCCONTROLS" Namespace="PLCCONTROLS" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cp" runat="server">
<!-- Sample Template Page with DBGrid, DBPanel, and ButtonPanel. -->
<style type="text/css">
    /* Add page styles here. Or preferably, the styles should be in App_Themes folder or BEASTiLIMS.css */
</style>

    <p>Sample Page with DBGrid, DBPanel, ButtonPanel</p>

    <div class="dbbox">
        <div class="dbgridblk">
            <!-- Put the grid name defined in the TV_DBGRIDHD and TV_DBGRIDDL tables in the 'PLCGridName=' attribute. -->
            <cc1:PLCDBGrid ID="gvNames" runat="server" DataKeyNames="NAME_KEY" PLCGridName='CASE_NAMES' 
                OnSelectedIndexChanged="gvNames_SelectedIndexChanged" AllowPaging="True" AllowSorting="True" PLCGridWidth="100%" Height="200">                    
            </cc1:PLCDBGrid>
        </div>
        <div class="dbgridbtnblk">
            <asp:Button ID="bnAction1" runat="server" Text="Action1" Width="100%" OnClick="bnAction1_Click" />
            <asp:Button ID="bnAction2" runat="server" Text="Action2" Width="100%" OnClick="bnAction2_Click" />
        </div>
    </div>

    <div class="dbbox">
        <div class="dbpanelblk">
            <!-- Put the panel name defined in the TV_DBPANEL table in the 'PLCPanelName=' attribute. -->
            <cc1:DBPanel ID="dbpNames" runat="server" PLCPanelName="NAMESTAB" PLCDataTableName="TV_LABNAME"
                PLCWhereClause="Where 0 = 1" 
                PLCShowAddButton="True" PLCShowDeleteButton="True" PLCShowEditButtons="True"
                PLCAuditCode="6" OnPLCDBPanelGetNewRecord="dbpNames_PLCDBPanelGetNewRecord" OnPLCDBPanelSetDefaultRecord="dbpNames_SetDefaultRecord"
                Width="100%" PLCDataTable="TV_LABNAME" PLCDisplayBottomBorder="False" PLCDisplayTopBorder="False">
            </cc1:DBPanel>
        </div>
        <div class="dbbtnpanel">
            <cc1:PLCButtonPanel ID="PLCButtonPanel1" runat="server" PLCDisplayBottomBorder="False"
                PLCDisplayTopBorder="False" PLCShowAddButton="True" PLCShowDeleteButton="True"
                PLCShowEditButtons="True" PLCTargetControlID="PLCDBPanel1" Width="100%" OnPLCButtonClick="PLCButtonPanel1_PLCButtonClick">
            </cc1:PLCButtonPanel>
        </div>
    </div>

    <script type="text/javascript">
        $(document).ready(function() {
            Init();
        });

        function Init() {
            // Set client-side event handlers, etc.
        }
    </script>
</asp:Content>
