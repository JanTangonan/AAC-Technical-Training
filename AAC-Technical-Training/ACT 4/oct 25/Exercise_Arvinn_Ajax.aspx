<%@ Page Title="" Language="C#" MasterPageFile="~/LIMSHOME.master" AutoEventWireup="true" CodeBehind="Exercise_Arvinn_Ajax.aspx.cs" Inherits="BEASTiLIMS.Exercise_Arvinn_Ajax1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta charset="utf-8" />
    <title>Recent Cases</title>
    <link rel="stylesheet" type="text/css" href="Exercise_Arvinn_Ajax.css" />
    <link rel="stylesheet" href="https://code.jquery.com/ui/1.14.0/themes/base/jquery-ui.css">
    <link rel="stylesheet" href="https://code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cp" runat="server">
    <h1>Recent Cases</h1>
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
            <tr>
                <td>400-12345-1234-123</td>
                <td>FORI-PALMDALE</td>
                <td>BURGLARY</td>
                <td>13-123456</td>
                <td>10/18/2022</td>
            </tr>
            <tr>
                <td>330-11145-1234-123</td>
                <td>TEST-PALMDALE</td>
                <td>BURGLARY</td>
                <td>13-123456</td>
                <td>05/08/2023</td>
            </tr>
            <tr>
                <td>111-11145-1114-123</td>
                <td>FORI-TESTDALE</td>
                <td>TESTTESTLARY</td>
                <td>33-343456</td>
                <td>09/28/2024</td>
            </tr>
            <tr>
                <td>555-55545-5534-523</td>
                <td>MOCK-MOCKDATA</td>
                <td>MOCKTEST</td>
                <td>33-222256</td>
                <td>12/21/2023</td>
            </tr>
        </tbody>
    </table>

    <!--Field set cosists of inputs and buttons-->
    <table>
        <tr>
            <td><label for="departmentCaseNumber">Department Case #</label></td>
            <td><input type="text" id="departmentCaseNumber" name="departmentCaseNumber" disabled /></td>
        </tr>
        <tr>
            <td><label for="departmentCase">Department Case</label></td>
            <td><input type="text" id="departmentCase" name="departmentCase" disabled /></td>
        </tr>
        <tr>
            <td><label for="charge">Charge</label></td>
            <td><input type="text" id="charge" name="charge" disabled /></td>
        </tr>
        <tr>
            <td><label for="lab-case-no">Lab Case #</label></td>
            <td><input type="text" id="labCaseNumber" name="lab-case-no" disabled /></td>
        </tr>
        <tr>
            <td><label for="incedentReportDate">Incedent Report Date</label></td>
            <td><input type="text" id="incedentReportDate" name="incedentReportDate" disabled /></td>
        </tr>
    </table>
    <br />

    <div class="form-buttons">
        <button id="btnEdit" type="button">Edit</button>
        <button id="btnSave" type="button" disabled>Save</button>
        <button id="btnCancel" type="button" disabled>Cancel</button>
    </div>

    <!--Pop up dialog upon saving-->
    <div id="dialog" title="Save Complete">
        <p>Case has been updated!</p>
    </div>

    <!--jquery dependencies-->
    <script src="https://code.jquery.com/jquery-3.2.1.min.js"></script>
    <script src="https://code.jquery.com/ui/1.14.0/jquery-ui.js"></script>
    <script src="Exercise_Arvinn_Ajax.js"></script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="foot" runat="server">
</asp:Content>
