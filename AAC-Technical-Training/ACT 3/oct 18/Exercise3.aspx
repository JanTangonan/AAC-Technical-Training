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
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <asp:UpdatePanel ID="UpdatePanel" runat="server">
            <ContentTemplate>
                <PLC:PLCDBGrid ID="dbgLabCase" runat="server" AutoGenerateColumns="false" CssClass="grid" DataKeyNames="CASE_KEY" AutoPostBack="true"
                        BorderStyle="None" BorderWidth="0px" BorderColor="Transparent" SelectedRowStyle-BackColor="#7FFFD4"
                        OnSelectedIndexChanged="dbgLabCase_SelectedIndexChanged">
                </PLC:PLCDBGrid>

                <fieldset>
                    <legend>Case Report</legend>
                    <PLC:DBPanel ID="dbpLabCase" PLCPanelName="CASETAB" runat="server" >
                    </PLC:DBPanel>
                </fieldset>

                <PLC:PLCButtonPanel ID="bpLabCase" runat="server" OnPLCButtonClick="bpLabCase_PLCButtonClick"
                    PLCShowEditButtons="True" PLCTargetControlID="dbpLabCase">
                </PLC:PLCButtonPanel>
            </ContentTemplate>
        </asp:UpdatePanel>
    </form>
</body>
</html>
