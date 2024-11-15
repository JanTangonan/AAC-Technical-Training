<%@ Master Language="C#" AutoEventWireup="True" Inherits="LimsHomeMaster" CodeBehind="LIMSHOME.master.cs" %>

<%@ Register Assembly="PLCCONTROLS" Namespace="PLCCONTROLS" TagPrefix="cc1" %>
<%@ Register Src="~/PLCWebCommon/UC_CustomerTitle.ascx" TagName="UC_CustomerTitle" TagPrefix="uc1" %>
<%@ Register Src="~/PLCWebCommon/UC_PLCCopyRights.ascx" TagName="UC_PLCCopyRights" TagPrefix="uc2" %>
<%@ Register Src="UC_MainMenu.ascx" TagName="UC_MainMenu" TagPrefix="uc3" %>
<%@ Register Src="~/PLCWebCommon/UC_SessionTimer.ascx" TagName="UC_SessionTimer" TagPrefix="uc4" %>
<%@ Register Src="~/PLCWebCommon/PLCPageHead.ascx" TagName="PLCPageHead" TagPrefix="uc5" %>

<!docTyPe hTml>
<html>
<head runat="server">
    <meta http-equiv="Page-Enter" content="blendTrans(Duration=0.10)" />
    <!-- <script src="<%# ResolveUrl("~/support/DockingPanel.js") %>" type="text/JavaScript" language="JavaScript"></script> -->
    <link href="<%# ResolveUrl("~/BEASTiLIMS.css?rev=" + PLCSession.CurrentRev) %>" type="text/css" rel="stylesheet" />

    <uc5:PLCPageHead runat="server" ID="pagehead1" include="codehead,plcdbpanel,jquery,utility,jquery-ui,codemultipick" />
    <title>BEAST Home Page</title>
    <link rel="shortcut icon" href="~/images/favicon.ico" />
    <asp:ContentPlaceHolder ID="head" runat="server">
    </asp:ContentPlaceHolder>
</head>

<body style="padding: 0 auto; margin: 0; overflow-y: scroll">
    <form id="form1" runat="server" style="font-size: small; vertical-align: top;">
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" AsyncPostBackTimeout="600">
    </asp:ScriptManager>

    <asp:UpdateProgress ID="up1" runat="server">
        <ProgressTemplate>
            <%--    Causes circle during crystal reports export. --%>
            <div style="position: fixed; margin: 0px; top: 0px; left: 0px; filter: alpha(opacity=60);
                -moz-opacity: .60; background-color: #EEE; z-index: 1000000; width: 100%; height: 100%; opacity: .6;">
            </div>
            <div style="position: fixed; left: 45%; z-index: 1000001; top: 45%; width: 100px; text-align: center;">
                <img src="<%= ResolveUrl("~/Images/ajax-loader.gif") %>" width="50" style="margin: 0px auto;" alt="" />
                <br />
                <asp:Label ID="lblProgress" runat="server" Text="Loading" CssClass="loading"></asp:Label>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>
    <div id="content">
    <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <cc1:PLCSessionVars ID="PLCSessionVars1" runat="server" />
            <div style="">
                <div style="min-width: 1000px;">
                    <table align="center" width="100%" cellpadding="0" cellspacing="0">
                        <tr>
                            <td>
                                <uc1:UC_CustomerTitle ID="UC_CustomerTitle_Master2" runat="server" UseCompact="false" />
                            </td>
                        </tr>
                        <tr>
                            <td align="left" style="height:1px;">
                            </td>
                        </tr>
                        <tr>
                            <td align="left" scope="col" valign="top">
                                <table style="width: 100%; height: 100%;" cellpadding="0" cellspacing="0">
                                    <tr>
                                        <td valign="top">
                                            <div id="pnlSideMenu" runat="server">
                                                <uc3:UC_MainMenu ID="UC_MainMenu" runat="server" />
                                            </div>
                                        </td>
                                        <td valign="top" width="100%">
                                            <asp:ContentPlaceHolder ID="cp" runat="server">
                                            </asp:ContentPlaceHolder>
                                        </td>
                                    </tr>
                                </table>
                                <br />
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <uc2:UC_PLCCopyRights ID="UC_PLCCopyRights_Master2" runat="server" />
                            </td>
                        </tr>
                    </table>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
    <uc4:UC_SessionTimer ID="timerTimeout" runat="server" />
    </div>
    </form>

    <asp:ContentPlaceHolder ID="foot" runat="server" />
        <script src="./PLCWebCommon/JavaScripts/Loading.js?rev=<%= PLCSession.CurrentRev %>" type="text/JavaScript" language="JavaScript"></script>
    <script src="./PLCWebCommon/JavaScripts/ClearedEvent.js?rev=<%= PLCSession.CurrentRev %>" type="text/JavaScript" language="JavaScript"></script>
    <script type="text/javascript" language="javascript">
        history.forward();
        SetF7HotKey();

        $(document).ready(function () {
            SetBrowserWindowID();
        });

        function pageLoad() {
            //Master pageLoad() code.

            //Content pageLoad code.
            if (typeof contentPageLoad == 'function')
                contentPageLoad();

            $(".plcdbpanel table tr td div span>div[id*='multipick']").each(function () {
                $(this).closest('td').prev().addClass('plcdbpanel-multipick-prompt');
            });
        }
    </script>

</body>
</html>
