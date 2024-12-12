<%@ Page Title="" Language="C#" MasterPageFile="~/LIMSHOME.master" AutoEventWireup="true" CodeBehind="Exercise_Arvin_Plugin_data_bs.aspx.cs" Inherits="BEASTiLIMS.Exercise_Arvin_Plugin_data_bs" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta charset="utf-8" />
    <title>Recent Cases</title>
    <link rel="stylesheet" type="text/css" href="Exercise_Arvin_Plugin_BS.css" />
    <link rel="stylesheet" href="https://code.jquery.com/ui/1.14.0/themes/base/jquery-ui.css">
    <link rel="stylesheet" href="https://code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css">
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css">

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cp" runat="server">
    <h1>Recent Cases</h1>
    <%--<div class="dbgCasesTable"></div>--%>

    <table id="tblGrid" class="dynamicGrid"
        data-data-key="CASE_KEY"
        data-read="./Exercise_Arvin_Plugin2WS.asmx/GetTopCases"
        data-columns='[
            { "name": "DEPARTMENT_CASE_NUMBER", "title": "Department Case #" },
            { "name": "DEPARTMENT_NAME", "title": "Department" },
            { "name": "CHARGE", "title": "Charge" },
            { "name": "LAB_CASE", "title": "Lab Case #" },
            { "name": "OFFENSE_DATE", "title": "Incident Report Date" }
        ]'>
    </table>    

    <!--Field set cosists of inputs and buttons-->
    <fieldset>
        <legend>Case Report</legend>
        <div id="divPanel" 
            data-table-name="TV_LABCASE"
            data-read="panelRead" 
            data-update="panelUpdate" 
            data-target-grid="tblGrid"
            data-controls='[
                 { "type": "hidden", "id": "hndCaseKey", "dataField": "CASE_KEY" },
                 { "type": "text", "label": "Department Case Number", "id": "txtDepartmentCaseNumber", "dataField": "DEPARTMENT_CASE_NUMBER" },
                 {
                     "type": "dropdown",
                     "label": "Department",
                     "id": "ddlDepartment",
                     "dataField": "DEPARTMENT_CODE",
                     "dataSourceUrl": "Exercise_Arvin_Plugin_dataWS.asmx/GetDepartments",
                     "valueField": "DEPARTMENT_CODE",
                     "textField": "DEPARTMENT_NAME"
                 },
                 {
                     "type": "dropdown",
                     "label": "Charge",
                     "id": "ddlCharge",
                     "dataField": "OFFENSE_CODE",
                     "dataSourceUrl": "Exercise_Arvin_Plugin_dataWS.asmx/GetCharges",
                     "valueField": "OFFENSE_CODE",
                     "textField": "OFFENSE_DESCRIPTION"
                 },
                 { "type": "text", "label": "Lab Case #", "id": "txtLabCaseNumber", "dataField": "LAB_CASE" },
                 { "type": "text", "label": "Incident Report Date", "id": "txtIncidentReportDate", "dataField": "OFFENSE_DATE" }
             ]'>
        </div>

        <div class="form-buttons">  
            <button id="btnEdit" type="button" class="btn btn-primary">Edit</button>
            <button id="btnSave" type="button" class="btn btn-success" disabled>Save</button>
            <button id="btnCancel" type="button" class="btn btn-secondary" disabled>Cancel</button>
        </div>
    </fieldset>

    <!--jquery dependencies-->
    <script src="https://code.jquery.com/jquery-3.2.1.min.js"></script>
    <script src="https://code.jquery.com/ui/1.14.0/jquery-ui.js"></script>
    <script src="https://maxcdn.bootstrapcdn.com/bootstrap/4.5.2/js/bootstrap.min.js"></script>
    <script src="exercise_arvin_plugin_data_bs.js"></script>
    <script src="jquery.dbgrid.arvin.data.js"></script>
    <script src="jquery.dbpanel.arvin.data.js"></script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="foot" runat="server">
</asp:Content>
