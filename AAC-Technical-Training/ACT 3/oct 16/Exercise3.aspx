<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Exercise3.aspx.cs" Inherits="AAC_Technical_Training.Exercise3" %>

<%@ Register Assembly="PLCCONTROLS" Namespace="PLCCONTROLS" TagPrefix="PLC" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Recent Cases</title>
    <link href="Exercise3.css" rel="stylesheet" type="text/css" />
    <link rel="stylesheet" href="https://code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css" />
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="https://code.jquery.com/ui/1.12.1/jquery-ui.min.js"></script>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" AsyncPostBackTimeout="600"></asp:ScriptManager>
        <asp:UpdatePanel ID="UpdatePanel" runat="server">
            <ContentTemplate>
                <PLC:PLCDBGrid ID="grdCasesTable" runat="server" AutoGenerateColumns="false" CssClass="grid" DataKeyNames="CASE_KEY" AutoPostBack="true"
                        BorderStyle="None" BorderWidth="0px" BorderColor="Transparent" SelectedRowStyle-BackColor="#7FFFD4"
                        OnSelectedIndexChanged="grdCasesTable_SelectedIndexChanged" OnRowDataBound="grdCasesTable_RowDataBound" 
                        OnRowCommand="grdCasesTable_RowCommand" PLCPanelName="PLCDBPanel1" >
                </PLC:PLCDBGrid>
                <div>
                    <fieldset>
                        <legend>Case Report</legend>
                        <PLC:DBPanel ID="PLCDBPanel1" PLCPanelName="CASETAB" runat="server" PLCDataTable="TV_LABCASE" >
                        </PLC:DBPanel>
                    </fieldset>
                </div>

                <div id="button-group">
                    <PLC:PLCButtonPanel ID="PLCButtonPanel1" runat="server" onplcdbpanelbuttonclick="PLCDBPanel1_PLCDBPanelButtonClick"  
                        PLCShowEditButtons="True" PLCTargetControlID="PLCDBPanel1" PLCTargetDBGridID="grdCasesTable">
                    </PLC:PLCButtonPanel>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </form>

</body>
</html>
