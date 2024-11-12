<%@ Page Title="" Language="C#" MasterPageFile="~/LIMSHOME.master" AutoEventWireup="true" CodeBehind="Exercise_Arvin_Repeater.aspx.cs" Inherits="BEASTiLIMS.Exercise_Arvin_Repeater" %>
<%@ Register Assembly="PLCCONTROLS" Namespace="PLCCONTROLS" TagPrefix="PLC" %>
<%@ Register Src="~/PLCWebCommon/PLCDialog.ascx" TagName="Dialog" TagPrefix="PLC" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .repeater-item {
            display: flex;
            align-items: center; /* Align label and textbox vertically */
            margin-bottom: 10px; /* Space between each pair */
        }

        .repeater-item label{
            padding-left: 5px;
            font-weight: bold;
            width: 150px; /* Adjust this width to align labels */
        }

        .repeater-item input, .repeater-item select {
            flex: 1; /* Allows the textbox to take remaining space */
            box-sizing: border-box;
        }

        .required-marker {
            color: red;
            font-weight: bold;
        }

        .not-required-marker {
            visibility: hidden;
        }

        .repeater-item input[required] + .required-marker {
            display: inline-block;
        }

        .repeater-item select[required] + .required-marker {
            display: inline-block; /* Show the asterisk for required fields */
        }

        .repeater-item input:not([required]) + .required-marker {
            display: none; /* Hide the asterisk for non-required fields */
        }

        .repeater-item select:not([required]) + .required-marker {
            display: none; /* Hide the asterisk for non-required fields */
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cp" runat="server">
    <asp:UpdatePanel ID="UpdatePanel" runat="server">
            <ContentTemplate>
                <PLC:Dialog ID="dlgMessage" runat="server" />
                <PLC:PLCDBGrid ID="dbgLabCase" runat="server" AutoGenerateColumns="false" CssClass="grid" DataKeyNames="CASE_KEY" AutoPostBack="true"
                    BorderStyle="None" BorderWidth="0px" BorderColor="Transparent" SelectedRowStyle-BackColor="#7FFFD4"
                    OnSelectedIndexChanged="dbgLabCase_SelectedIndexChanged" PLCGridName="TEST_NICE_NOICE" PLCGridWidth="100%">
                </PLC:PLCDBGrid>
                
                <asp:Repeater ID="repLabCaseControl" runat="server" OnItemDataBound="repLabCaseControl_ItemDataBound">
                    <ItemTemplate>

                        <PLC:FlexBox ID="flRepControlItem" runat="server">

                        </PLC:FlexBox>

                        <div class="repeater-item">
                            <!-- Department Case Number -->
                            <label for="txtDepartmentCode">Agency:</label>
                            <span class="required-marker">*</span>
                            <asp:TextBox ID="txtDepartmentCode" runat="server" Text='<%# Eval("DEPARTMENT_CODE") %>' required="true"></asp:TextBox>
                        </div>
                        <div class="repeater-item">
                            <!-- Department Case Number -->
                            <label for="txtDepartmentCaseNumber">Agency Case #:</label>
                            <span class="required-marker">*</span>
                            <asp:TextBox ID="txtDepartmentCaseNumber" runat="server" Text='<%# Eval("DEPARTMENT_CASE_NUMBER") %>' required="true"></asp:TextBox>
                        </div>

                        <div class="repeater-item">
                            <!-- Department DropDownList -->
                            <label for="ddlDepartment">Department:</label>
                            <span class="not-required-marker">*</span>
                            <asp:DropDownList ID="ddlDepartment" runat="server" DataTextField="DEPARTMENT_NAME" DataValueField="DEPARTMENT_CODE" ></asp:DropDownList>
                        </div>

                        <div class="repeater-item">
                            <!-- Charge DropDownList -->
                            <label for="ddlCharge">Charge:</label>
                            <span class="not-required-marker">*</span>
                            <asp:DropDownList ID="ddlCharge" runat="server" DataTextField="OFFENSE_DESCRIPTION" DataValueField="OFFENSE_CODE"></asp:DropDownList>
                        </div>

                        <div class="repeater-item">
                            <!-- Investigation Unit -->
                            <%--<label for="txtInvestigatingUnit">Investigation Unit:</label>--%>
                            <label for="ddlInvestigatingUnit">Investigation Unit:</label>
                            <span class="required-marker">*</span>
                            <%--<asp:TextBox ID="txtInvestigatingUnit" runat="server" Text='<%# Eval("INVESTIGATING_AGENCY") %>' required="true"></asp:TextBox>--%>
                            <asp:DropDownList ID="ddlInvestigatingUnit" runat="server" DataTextField="DEPARTMENT_NAME" DataValueField="DEPARTMENT_CODE"></asp:DropDownList>
                        </div>

                        <div class="repeater-item">
                            <!-- Investigator -->
                            <label for="ddlCaseManager">Investigator:</label>
                            <span class="not-required-marker">*</span>
                            <asp:DropDownList ID="ddlCaseManager" runat="server" DataTextField="MANAGER_NAME" DataValueField="CASE_MANAGER"></asp:DropDownList>
                        </div>

                        <div class="repeater-item">
                            <!-- Alternative Investigator -->
                            <label for="ddlCaseAnalyst">Alternative Inv:</label>
                            <span class="not-required-marker">*</span>
                            <asp:DropDownList ID="ddlCaseAnalyst" runat="server" DataTextField="ANALYST_NAME" DataValueField="CASE_ANALYST"></asp:DropDownList>
                        </div>

                        <div class="repeater-item">
                            <!-- Category -->
                            <%--<label for="txtOffenseCategory">Category:</label>--%>
                            <label for="ddlOffenseCategory">Category:</label>
                            <span class="required-marker">*</span>
                            <%--<asp:TextBox ID="txtOffenseCategory" runat="server" Text='<%# Eval("OFFENSE_CATEGORY") %>' required="true"></asp:TextBox>--%>
                            <asp:DropDownList ID="ddlOffenseCategory" runat="server" DataTextField="DESCRIPTION" DataValueField="CODE"></asp:DropDownList>
                        </div>

                        <div class="repeater-item">
                            <!-- Lab Case Number -->
                            <label for="txtLabCaseNumber">Lab Case Number:</label>
                            <span class="not-required-marker">*</span>
                            <asp:TextBox ID="txtLabCaseNumber" runat="server" Text='<%# Eval("LAB_CASE") %>'></asp:TextBox>
                        </div>

                        <div class="repeater-item">
                            <!-- Incident Report Date -->
                            <label for="txtReportIncidentDate">Report Incident Date:</label>
                            <span class="not-required-marker">*</span>
                            <asp:TextBox ID="txtReportIncidentDate" runat="server" Text='<%# Eval("OFFENSE_DATE") %>'></asp:TextBox>
                        </div>

                        <div class="repeater-item">
                            <!-- Case Status -->
                            <label for="txtCaseStatus">Case Status:</label>
                            <span class="not-required-marker">*</span>
                            <asp:TextBox ID="txtCaseStatus" runat="server" Text='<%# Eval("CASE_STATUS") %>'></asp:TextBox>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>

                <PLC:PLCButtonPanel ID="bpLabCase" runat="server" OnPLCButtonClick="bpLabCase_PLCButtonClick"
                    PLCShowEditButtons="True">
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
