<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Exercise2.aspx.cs" Inherits="AAC_Technical_Training.Exercise2" %>

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
            <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" CssClass="GridView1" BorderStyle="None" BorderWidth="0px" BorderColor="Transparent"
                OnSelectedIndexChanged="GridView1_SelectedIndexChanged" AutoPostBack="True" >
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
                    <asp:DropDownList ID="ddlDepertmentCaseNumber" runat="server" Enabled="false" CssClass="ddlButtonGroup"></asp:DropDownList>
                </div>

                <!-- Department -->
                <div class="form-group">
                    <label>Department</label>
                    <asp:TextBox ID="txtDepertment" runat="server" Enabled="false"></asp:TextBox> 
                </div>

                <!-- Charge -->
                <div class="form-group">
                    <label>Charge</label>
                    <asp:DropDownList ID="ddlCharge" runat="server" Enabled="false" CssClass="ddlButtonGroup"></asp:DropDownList>
                </div>

                <!-- Lab Case # -->
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
                    <asp:Button ID="ButtonEdit" runat="server" Text="Edit" UseSubmitBehavior="false" />
                    <asp:Button ID="ButtonSave" runat="server" Text="Save" UseSubmitBehavior="false" Enabled="false" />
                    <asp:Button ID="ButtonCancel" runat="server" Text="Cancel" UseSubmitBehavior="false" Enabled="false" />
                </div>
            </fieldset>
        </div>
    </form>
</body>
</html>
