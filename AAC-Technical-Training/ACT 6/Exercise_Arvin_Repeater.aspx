<%@ Page Title="" Language="C#" MasterPageFile="~/LIMSHOME.master" AutoEventWireup="true" CodeBehind="Exercise_Arvin_Repeater.aspx.cs" Inherits="BEASTiLIMS.Exercise_Arvin_Repeater" %>
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
                
                <asp:Repeater ID="repLabCaseControl" runat="server">
                    <ItemTemplate>
                        <div class="item-container" style="display: flex; flex-direction: column; gap: 10px;">
                            <!-- Department Case Number -->
                            <label for="txtDepartmentCaseNumber">Department Case Number:</label>
                            <asp:TextBox ID="txtDepartmentCaseNumber" runat="server" Text='<%# Eval("DepartmentCaseNumber") %>'>
                            </asp:TextBox>

                            <!-- Department DropDownList -->
                            <label for="ddlDepartment">Department:</label>
                            <asp:DropDownList ID="ddlDepartment" runat="server" DataTextField="DEPARTMENT_NAME"
                                DataValueField="DEPARTMENT_CODE"></asp:DropDownList>

                            <!-- Charge DropDownList -->
                            <label for="ddlCharge">Charge:</label>
                            <asp:DropDownList ID="ddlCharge" runat="server" DataTextField="OFFENSE_DESCRIPTION"
                                DataValueField="OFFENSE_CODE"></asp:DropDownList>

                            <!-- Lab Case Number -->
                            <label for="txtLabCaseNumber">Lab Case Number:</label>
                            <asp:TextBox ID="txtLabCaseNumber" runat="server" Text='<%# Eval("LabCaseNumber") %>'></asp:TextBox>

                            <!-- Incident Report Date -->
                            <label for="txtReportIncidentDate">Report Incident Date:</label>
                            <asp:TextBox ID="txtReportIncidentDate" runat="server"
                                Text='<%# Eval("ReportIncidentDate", "{0:yyyy-MM-dd}") %>'></asp:TextBox>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>

                <PLC:PLCButtonPanel ID="bpLabCase" runat="server" OnPLCButtonClick="bpLabCase_PLCButtonClick"
                    PLCShowEditButtons="True" PLCTargetControlID="dbpLabCase" >
                </PLC:PLCButtonPanel>
            </ContentTemplate>
        </asp:UpdatePanel>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="foot" runat="server">
</asp:Content>
