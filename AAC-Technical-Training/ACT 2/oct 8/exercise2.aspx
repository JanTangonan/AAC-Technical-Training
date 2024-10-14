<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Exercise2.aspx.cs" Inherits="AAC_Technical_Training.Exercise2 "
    EnableEventValidation ="false"%>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="Exercise2.css" rel="stylesheet" type="text/css" />
    <script src="Exercise2.js"></script>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h1>Recent Cases</h1>
            <!-- Recent Cases Table View using Grid View -->
            <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" CssClass="GridView1" DataKeyNames="CASE_KEY"
                BorderStyle="None" BorderWidth="0px" BorderColor="Transparent" SelectedRowStyle-BackColor="#7FFFD4"
                OnRowCommand="GridView1_RowCommand" AutoPostBack="true" OnRowDataBound="GridView1_RowDataBound">
                <Columns>
                    <asp:BoundField DataField="DEPARTMENT_CASE_NUMBER" HeaderText="Department Case #" />
                    <asp:BoundField DataField="DEPARTMENT_NAME" HeaderText="Department" />
                    <asp:BoundField DataField="CHARGE" HeaderText="Charge" />
                    <asp:BoundField DataField="LAB_CASE" HeaderText="Lab Case #" />
                    <asp:BoundField DataField="OFFENSE_DATE" HeaderText="Incident Report Date" DataFormatString="{0:MM-dd-yyyy}" />
                </Columns>
            </asp:GridView>

        </div>

        <div>
            <fieldset>
                <legend>Case Report</legend>
                <!-- Department Case # -->
                <div class="form-group">
                    <label>Department Case #</label>
                    <asp:TextBox ID="txtDepartmentCaseNumber" runat="server" Enabled="false" ></asp:TextBox>
                    
                </div>

                <!-- Department -->
                <div class="form-group">
                    <label>Department</label>
                    <%--<asp:TextBox ID="txtDepartment" runat="server" Enabled="false" HtmlEncode="false"></asp:TextBox> --%>
                    <asp:DropDownList ID="ddlDepartment" runat="server" CssClass="ddlButtonGroup" Enabled="false">
                    </asp:DropDownList>
                </div>

                <!-- Charge -->
                <div class="form-group">
                    <label>Charge</label>
                    <%--<asp:TextBox ID="txtCharge" runat="server" Enabled="false"></asp:TextBox>--%>
                    <asp:DropDownList ID="ddlCharge" runat="server" CssClass="ddlButtonGroup" Enabled="false">
                    </asp:DropDownList>
                </div>

                <!-- Lab Case # -->
                <div class="form-group">
                    <label>Lab Case #</label>
                    <asp:TextBox ID="txtLabCaseNumber" runat="server" Enabled="false"></asp:TextBox> 
                </div>

                <!-- Report Incident Date -->
                <div class="form-group">
                    <label>Report Incident Date</label>
                    <asp:TextBox ID="txtReportIncidentDate" runat="server" Enabled="false"></asp:TextBox> 
                </div>

                <!-- Button group -->
                <div class="button-group">
                    <asp:Button ID="btnEdit" runat="server" Text="Edit" UseSubmitBehavior="false" OnClick="BtnEdit_Click"/>
                    <asp:Button ID="btnSave" runat="server" Text="Save" UseSubmitBehavior="false" Enabled="false" Onclick="BtnSave_Click"/>
                    <asp:Button ID="btnCancel" runat="server" Text="Cancel" UseSubmitBehavior="false" Enabled="false" Onclick="BtnCancel_Click"/>
                </div>
            </fieldset>
        </div>
    </form>
</body>
</html>
