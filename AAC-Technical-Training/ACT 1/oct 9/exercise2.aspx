<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Exercise2.aspx.cs" Inherits="AAC_Technical_Training.Exercise2 "
    EnableEventValidation ="false"%>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="Exercise2.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h1>Recent Cases</h1>
            <!-- Recent Cases Table View using Grid View -->
            <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" CssClass="grid" DataKeyNames="CASE_KEY"
                BorderStyle="None" BorderWidth="0px" BorderColor="Transparent" SelectedRowStyle-BackColor="#7FFFD4"
                OnRowCommand="GridView1_RowCommand" AutoPostBack="true" OnRowDataBound="GridView1_RowDataBound">
                <Columns>
                    <asp:BoundField DataField="DEPARTMENT_CASE_NUMBER" HeaderText="Department Case #" />
                    <asp:BoundField DataField="DEPARTMENT_NAME" HeaderText="Department" />
                    <asp:BoundField DataField="CHARGE" HeaderText="Charge" />
                    <asp:BoundField DataField="LAB_CASE" HeaderText="Lab Case #" />
                    <asp:BoundField DataField="OFFENSE_DATE" HeaderText="Incident Report Date" DataFormatString="{0:yyyy-MM-dd}" />
                </Columns>
            </asp:GridView>
        </div>

        <div>
            <fieldset>
                <legend>Case Report</legend>
                <!-- Department Case # -->
                <div class="form-group">
                    <label>Department Case #</label>
                    <asp:TextBox ID="txtDepartmentCaseNumber" runat="server" Enabled="false"></asp:TextBox>
                    <asp:Label ID="lblDepartmentCaseNumberMessage" runat="server" ForeColor="Red" />
                </div>

                <!-- Department -->
                <div class="form-group">
                    <label>Department</label>
                    <asp:DropDownList ID="ddlDepartment" runat="server" CssClass="ddlButtonGroup" Enabled="false">
                    </asp:DropDownList>
                </div>

                <!-- Charge -->
                <div class="form-group">
                    <label>Charge</label>
                    <asp:DropDownList ID="ddlCharge" runat="server" CssClass="ddlButtonGroup" Enabled="false">
                    </asp:DropDownList>
                </div>

                <!-- Lab Case # -->
                <div class="form-group">
                    <label>Lab Case #</label>
                    <asp:TextBox ID="txtLabCaseNumber" runat="server" Enabled="false"></asp:TextBox> 
                    <asp:Label ID="lblLabCaseNumberMessage" runat="server" ForeColor="Red" />
                    
                </div>

                <!-- Report Incident Date -->
                <div class="form-group">
                    <label>Report Incident Date</label>
                    <asp:TextBox ID="txtReportIncidentDate" runat="server" Enabled="false"></asp:TextBox> 
                    <asp:Label ID="lblReportIncidentDateMessage" runat="server" ForeColor="Red" />
                </div>

                <!-- Button group -->
                <div class="button-group">
                    <asp:Button ID="btnEdit" runat="server" Text="Edit" UseSubmitBehavior="false" OnClick="BtnEdit_Click"/>
                    <asp:Button ID="btnSave" runat="server" Text="Save" Enabled="false" UseSubmitBehavior="false" Onclick="BtnSave_Click"/>
                    <asp:Button ID="btnCancel" runat="server" Text="Cancel" Enabled="false" UseSubmitBehavior="false" Onclick="BtnCancel_Click"/>
                </div>
            </fieldset>
        </div>
    </form>
</body>
</html>
