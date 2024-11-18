<%@ Page Title="" Language="C#" MasterPageFile="~/LIMSHOME.master" AutoEventWireup="true" CodeBehind="Exercise_Arvin_Plugin.aspx.cs" Inherits="BEASTiLIMS.Exercise_Arvin_Plugin" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta charset="utf-8" />
    <title>Recent Cases</title>
    <link rel="stylesheet" type="text/css" href="Exercise_Arvin_Plugin.css" />
    <link rel="stylesheet" href="https://code.jquery.com/ui/1.14.0/themes/base/jquery-ui.css">
    <link rel="stylesheet" href="https://code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cp" runat="server">
    <h1>Recent Cases</h1>
    <div class="dbgCasesTable"></div>

    <!--Field set cosists of inputs and buttons-->
    <fieldset>
        <div class="my-controls"></div>
        <div class="my-buttons"></div>
    </fieldset>

    <!--jquery dependencies-->
    <script src="https://code.jquery.com/jquery-3.2.1.min.js"></script>
    <script src="https://code.jquery.com/ui/1.14.0/jquery-ui.js"></script>
    <script src="Exercise_Arvin_Plugin.js"></script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="foot" runat="server">
</asp:Content>

