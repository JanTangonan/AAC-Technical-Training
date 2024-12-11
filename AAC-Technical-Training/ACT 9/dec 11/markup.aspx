<%@ Page Title="" Language="C#" MasterPageFile="~/LIMSHOME.master" AutoEventWireup="true" CodeBehind="Exercise_Arvin_Plugin_data_bs.aspx.cs" Inherits="BEASTiLIMS.Exercise_Arvin_Plugin_data_bs" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta charset="utf-8" />
    <title>Recent Cases</title>
    <link rel="stylesheet" type="text/css" href="Exercise_Arvin_Plugin_BS.css" />
    <link rel="stylesheet" href="https://code.jquery.com/ui/1.14.0/themes/base/jquery-ui.css">
    <link rel="stylesheet" href="https://code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css">
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css">
    <script src="https://maxcdn.bootstrapcdn.com/bootstrap/4.5.2/js/bootstrap.min.js"></script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cp" runat="server">
    <h1>Recent Cases</h1>
    <%--<div class="dbgCasesTable"></div>--%>

    <table id="tblGrid" class="dynamicGrid"
        data-read="./Exercise_Arvin_Plugin2WS.asmx/GetTopCases">
    </table>    

    <!--Field set cosists of inputs and buttons-->
    <fieldset>
        <legend>Case Report</legend>
        <div id="divPanel" data-read="panelRead" data-target-grid="tblGrid" ></div>

        <div class="form-buttons">  
            <button id="btnEdit" type="button" class="btn btn-primary">Edit</button>
            <button id="btnSave" type="button" class="btn btn-success" disabled>Save</button>
            <button id="btnCancel" type="button" class="btn btn-secondary" disabled>Cancel</button>
        </div>
    </fieldset>

    <!--jquery dependencies-->
    <script src="https://code.jquery.com/jquery-3.2.1.min.js"></script>
    <script src="https://code.jquery.com/ui/1.14.0/jquery-ui.js"></script>
    <script src="exercise_arvin_plugin_data_bs.js"></script>
    <script src="jquery.dbgrid.arvin.data.js"></script>
    <script src="jquery.dbpanel.arvin.data.js"></script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="foot" runat="server">
</asp:Content>
