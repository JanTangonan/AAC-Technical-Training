<%@ Page Title="" Language="C#" MasterPageFile="~/LIMSHOME.master" AutoEventWireup="true" CodeBehind="Exercise_Arvin_SP.aspx.cs" Inherits="BEASTiLIMS.Exercise_Arvin_SP" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta charset="utf-8" />
    <title>Recent Cases</title>
    <link rel="stylesheet" type="text/css" href="Exercise_Arvinn_Ajax.css" />
    <link rel="stylesheet" href="https://code.jquery.com/ui/1.14.0/themes/base/jquery-ui.css">
    <link rel="stylesheet" href="https://code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cp" runat="server">
    <%--<h1>Recent Cases</h1>
    <table class="cases-table">
        <thead>
            <tr id="table-row-header">
                <th>Department Case #</th>
                <th>Department</th>
                <th>Charge</th>
                <th>Lab Case #</th>
                <th>Incedent Report Date</th>
            </tr>
        </thead>
        <tbody class="cases-table-body">
        </tbody>
    </table>

    <!--Field set cosists of inputs and buttons-->
    <fieldset>
        <table class="form-table">
            <tr>
                <td><label for="txtDepartmentCaseNumber">Department Case #</label></td>
                <td><input type="text" id="txtDepartmentCaseNumber" name="txtDepartmentCaseNumber" disabled /></td>
            </tr>
            <tr>
                <td><label for="ddlDepartment">Department</label></td>
                <td>
                    <select id="ddlDepartment" name="ddlDepartment" disabled>
                        <option value=""></option>
                    </select>
                </td>
            </tr>
            <tr>
                <td><label for="ddlCharge">Charge</label></td>
                <td>
                    <select id="ddlCharge" name="ddlCharge" disabled>
                        <option value=""></option>
                    </select>
                </td>
            </tr>
            <tr>
                <td><label for="txtLabCaseNumber">Lab Case #</label></td>
                <td><input type="text" id="txtLabCaseNumber" name="txtLabCaseNumber" disabled /></td>
            </tr>
            <tr>
                <td><label for="txtIncidentReportDate">Incedent Report Date</label></td>
                <td><input type="text" id="txtIncidentReportDate" name="txtIncidentReportDate" disabled /></td>
            </tr>
        </table>
        <br />

        <div class="form-buttons">
            <button id="btnEdit" type="button">Edit</button>
            <button id="btnSave" type="button" disabled>Save</button>
            <button id="btnCancel" type="button" disabled>Cancel</button>
        </div>
    </fieldset>--%>

    <div>
        <!-- Input textbox for the number of rows -->
        <label for="txtNumber">Enter Number of Rows:</label>
        <input type="number" id="txtNumber" min="1" />
        <button id="btnGenerateTable" type="button">Generate Table</button>
    </div>

    <!-- Table container -->
    <div>
        <table id="dynamicTable" border="1">
            <thead>
                <tr>
                    <th>First Name</th>
                    <th>Middle Name</th>
                    <th>Last Name</th>
                    <th>Gender</th>
                    <th>Date of Birth (DOB)</th>
                    <th>Age</th>
                </tr>
            </thead>
            <tbody>
                <!-- Rows will be dynamically added here -->
            </tbody>
        </table>
    </div>

    <div id="buttonContainer"></div>
    <%--<div>
        <label for="txtCsv">Enter CSV:</label>
        <input type="text" id="txtCsv" placeholder="Enter comma-separated values" />
        <button id="btnReverseOrder" type="button">Reverse Order</button>
    </div>

    <table id="tblResults" border="1">
        <thead>
            <tr>
                <th>Reversed Records</th>
            </tr>
        </thead>
        <tbody>
        </tbody>
    </table>--%>

    <!--jquery dependencies-->
    <script>

    </script>
    <script src="https://code.jquery.com/jquery-3.2.1.min.js"></script>
    <script src="https://code.jquery.com/ui/1.14.0/jquery-ui.js"></script>
    <script src="Exercise_Arvinn_Ajax.js"></script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="foot" runat="server">
</asp:Content>
