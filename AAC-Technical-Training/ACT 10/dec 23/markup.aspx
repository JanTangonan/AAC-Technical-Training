<%@ Page Title="" Language="C#" MasterPageFile="~/LIMSHOME.master" AutoEventWireup="true" CodeBehind="Exercise_Arvin_SP.aspx.cs" Inherits="BEASTiLIMS.Exercise_Arvin_SP" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta charset="utf-8" />
    <title>Recent Cases</title>
    <link rel="stylesheet" type="text/css" href="Exercise_Arvinn_Ajax.css" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet">
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js"></script>
    <link rel="stylesheet" href="https://code.jquery.com/ui/1.14.0/themes/base/jquery-ui.css">
    <link rel="stylesheet" href="https://code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cp" runat="server">
    <div>
        <!-- Input textbox for the number of rows -->
        <label for="txtNumber">Enter Number of Rows:</label>
        <input type="number" id="txtNumber" min="1" />
        <button id="btnGenerateTable" type="button">Generate Table</button>
    </div>

    <!-- Table container -->
    <div>
        <table id="dynamicTable" class="table table-bordered table-custom">
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
    <div id="reversedTableContainer"></div>

    <script src="https://code.jquery.com/jquery-3.2.1.min.js"></script>
    <script src="https://code.jquery.com/ui/1.14.0/jquery-ui.js"></script>
    <script src="Exercise_Arvinn_Ajax.js"></script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="foot" runat="server">
</asp:Content>
