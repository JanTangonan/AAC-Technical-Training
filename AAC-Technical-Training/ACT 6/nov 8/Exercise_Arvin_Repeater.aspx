<%@ Page Title="" Language="C#" MasterPageFile="~/LIMSHOME.master" AutoEventWireup="true" CodeBehind="Exercise_Arvin_Repeater.aspx.cs" Inherits="BEASTiLIMS.Exercise_Arvin_Repeater" %>
<%@ Register Assembly="PLCCONTROLS" Namespace="PLCCONTROLS" TagPrefix="PLC" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .item-container {
            display: flex;
            flex-direction: column;
            padding: 10px;
            border: 1px solid #ccc;
            border-radius: 5px;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cp" runat="server">
    <asp:UpdatePanel ID="UpdatePanel" runat="server">
            <ContentTemplate>
                <PLC:PLCDBGrid ID="dbgLabCase" runat="server" AutoGenerateColumns="false" CssClass="grid" DataKeyNames="CASE_KEY" AutoPostBack="true"
                    BorderStyle="None" BorderWidth="0px" BorderColor="Transparent" SelectedRowStyle-BackColor="#7FFFD4"
                    OnSelectedIndexChanged="dbgLabCase_SelectedIndexChanged" PLCGridName="TEST_NICE_NOICE" PLCGridWidth="100%">
                </PLC:PLCDBGrid>
                
                <asp:Repeater ID="repLabCaseControl" runat="server" OnItemDataBound="repLabCaseControl_ItemDataBound">
                    <ItemTemplate>
                        <div class="item-container" style="display: flex; flex-direction: column;">
                            <!-- Department Case Number -->
                            <label for="txtDepartmentCaseNumber">Department Case Number:</label>
                            <asp:TextBox ID="txtDepartmentCaseNumber" runat="server" Text='<%# Eval("DEPARTMENT_CASE_NUMBER") %>'></asp:TextBox>

                            <!-- Department DropDownList -->
                            <label for="ddlDepartment">Department:</label>
                            <asp:DropDownList ID="ddlDepartment" runat="server" DataTextField="DEPARTMENT_NAME" DataValueField="DEPARTMENT_CODE"></asp:DropDownList>

                            <!-- Charge DropDownList -->
                            <label for="ddlCharge">Charge:</label>
                            <asp:DropDownList ID="ddlCharge" runat="server" DataTextField="OFFENSE_DESCRIPTION" DataValueField="OFFENSE_CODE"></asp:DropDownList>

                            <!-- Investigation Unit -->
                            <label for="txtInvestigatingUnit">Investigation Unit:</label>
                            <asp:TextBox ID="txtInvestigatingUnit" runat="server" Text='<%# Eval("INVESTIGATING_AGENCY") %>'></asp:TextBox>

                            <!-- Investigator -->
                            <label for="txtCaseManager">Investigator:</label>
                            <asp:TextBox ID="txtCaseManager" runat="server" Text='<%# Eval("CASE_MANAGER") %>'></asp:TextBox>

                            <!-- Alternative Investigator -->
                            <label for="txtCaseAnalyst">Alternative Investigator:</label>
                            <asp:TextBox ID="txtCaseAnalyst" runat="server" Text='<%# Eval("CASE_ANALYST") %>'></asp:TextBox>

                            <!-- Category -->
                            <label for="txtOffenseCategory">Category:</label>
                            <asp:TextBox ID="txtOffenseCategory" runat="server" Text='<%# Eval("OFFENSE_CATEGORY") %>'></asp:TextBox>

                            <!-- Lab Case Number -->
                            <label for="txtLabCaseNumber">Lab Case Number:</label>
                            <asp:TextBox ID="txtLabCaseNumber" runat="server" Text='<%# Eval("LAB_CASE") %>'></asp:TextBox>

                            <!-- Incident Report Date -->
                            <label for="txtReportIncidentDate">Report Incident Date:</label>
                            <asp:TextBox ID="txtReportIncidentDate" runat="server" Text='<%# Eval("OFFENSE_DATE") %>'></asp:TextBox>

                            <!-- Case Status -->
                            <label for="txtCaseStatus">Case Status:</label>
                            <asp:TextBox ID="txtCaseStatus" runat="server" Text='<%# Eval("CASE_STATUS") %>'></asp:TextBox>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>

                <PLC:PLCButtonPanel ID="bpLabCase" runat="server" OnPLCButtonClick="bpLabCase_PLCButtonClick"
                    PLCShowEditButtons="True" PLCTargetControlID="dbpLabCase" >
                </PLC:PLCButtonPanel>
            </ContentTemplate>
        </asp:UpdatePanel>

    <link rel="stylesheet" href="https://code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css">
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="https://code.jquery.com/ui/1.12.1/jquery-ui.min.js"></script>
    <script type="text/javascript">
    $(document).ready(function () {
        // Initialize the datepicker on elements with the 'datepicker' class
        $(document).on("focus", ".datepicker", function () {
            $(this).datepicker({
                dateFormat: 'yy-mm-dd',
                maxDate: new Date()
            });
        });
    });
</script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="foot" runat="server">
</asp:Content>
