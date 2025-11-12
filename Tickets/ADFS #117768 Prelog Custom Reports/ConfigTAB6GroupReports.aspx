<%@ Page Language="C#" MasterPageFile="~/CONFIG.master" AutoEventWireup="True" CodeBehind="ConfigTAB6GroupReports.aspx.cs" Inherits="BEASTiLIMS.ConfigTAB6GroupReports" Title="Group Reports" %>

<%@ Register Assembly="System.Web.Extensions" Namespace="System.Web.UI" TagPrefix="asp" %>
<%@ Register assembly="PLCCONTROLS" namespace="PLCCONTROLS" tagprefix="PLC" %>
<%@ Register Src="~/PLCWebCommon/PLCPageHead.ascx" TagName="PLCPageHead" TagPrefix="ph" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .ajax__calendar_container 
        {
            z-index: 1000;
        }
        /* custom color tags for adhoc.ini */
        plc-blue {color:blue;}
        plc-green {color:green;}
        plc-orange {color:orange;}
        plc-red {color:red;}
        plc-yellow {color:yellow;}
        plc-blink {animation: blinker 1s linear infinite;} 
        @keyframes blinker {
		  50% {
			opacity: 0;
		  }
		}
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cp" runat="server">
    <ph:PLCPageHead runat="server" ID="pagehead1" include="codemultipick" />
    <PLC:PLCSessionVars ID="PLCSessionVars1" runat="server" />
    <asp:PlaceHolder ID="phMsgBox" runat="server"></asp:PlaceHolder>
    <asp:PlaceHolder ID="phMsgBoxComments" runat="server"></asp:PlaceHolder>
    <asp:Button ID="MsgCommentPostBackButton" runat="server" Text="Button" OnClick="MsgCommentPostBackButton_Click" style="display:none;" />

    <asp:MultiView ID="mvReports" runat="server" ActiveViewIndex="0">
        <asp:View ID="vReports" runat="server">
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <div style="margin-top:10px;">
                        <PLC:PLCDBGrid ID="GridView1" runat="server" DataKeyNames="CUST_REPORT_KEY" 
                          onselectedindexchanged="GridView1_SelectedIndexChanged"
                          onpageindexchanged="GridView1_PageIndexChanged"  PLCGridName="CUSTREPORTS"
                          Width="100%" AllowPaging="True" AllowSorting="True" HeightOnScrollEnabled="170px"
                          OnSorted="GridView1_Sorted" EmptyDataText="No Reports Found."
                          PageSize="10">
                        </PLC:PLCDBGrid>
                    </div>
                    <hr/>
                    <PLC:DBPanel ID="PLCDBPanel1" runat="server"
                        PLCDataTable="TV_CUSTREPORTS"
                        PLCPanelName="CTCODES_TV_CUSTREPT"
                        PLCShowEditButtons="True" PLCWhereClause="Where 0 = 1"
                        OnPLCDBPanelSetDefaultRecord="PLCDBPanel1_SetDefaultRecord"
                        OnPLCDBPanelGetNewRecord="PLCDBPanel1_PLCDBPanelGetNewRecord" PLCConfigPanel="true" 
                        PLCAuditCode="26" PLCAuditSubCode="1" PLCDeleteAuditCode="20" PLCDeleteAuditSubCode="1" Width="100%">
                    </PLC:DBPanel>
                </ContentTemplate>
        </asp:UpdatePanel>
            <asp:Button ID="btnPageEvents" runat="server" OnClick="btnPageEvents_Click" Style='display: none;' />
        <PLC:PLCButtonPanel ID="PLCButtonPanel1" runat="server"
            PLCShowAddButton="True" PLCTargetControlID="PLCDBPanel1"
            Width="100%" PLCShowEditButtons="True" PLCDisplayBottomBorder="True"
            PLCShowDeleteButton = "true"
            PLCDisplayTopBorder="True"
            onplcbuttonclick="PLCButtonPanel1_PLCButtonClick" 
            PLCCustomButtons="Run Report,Upload Report,Download Report,Back to Groups" >
        </PLC:PLCButtonPanel>   
        </asp:View>
        <asp:View ID="vRunReport" runat="server">
            <div style="margin: 10px;">
                <div>
                    <b>Run Report: </b>
                    <asp:Label ID="lblReport" runat="server"></asp:Label>
                </div><br />
                <div>
                    <asp:Panel ID="pnlParameters" runat="server" GroupingText="Enter parameters for the report">
                        <div id="divRepParameters" runat="server">
                            <asp:Repeater ID="repParameters" runat="server" EnableViewState="true" >
                                <HeaderTemplate>
                                    <table style="margin: 10px;">
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <tr style="display: <%# ( Eval("Type").ToString() == "IFNDEF" || Eval("Type").ToString() == "IFDEF" ||   Eval("Type").ToString() == "ALWAYS" || Eval("Type").ToString() == "CODE_ALWAYS") ? "none" : "block" %>">
                                        <td align="right">
                                            <asp:Label ID="lblParameter" runat="server"  Visible='<%# Eval("Type").ToString() != "BLANK" %>' Text='<%# Eval("Name") %>'></asp:Label>
                                            <asp:Literal ID="literalBlank" runat="server" Text="&nbsp;" Visible='<%# Eval("Type").ToString() == "BLANK" %>' />
                                            <asp:HiddenField ID="hdnField" runat="server" Value='<%# Eval("Field") %>' />
                                            <asp:HiddenField ID="hdnControlField" runat="server" Value='<%# Eval("ControlField") %>' />
                                            <asp:HiddenField ID="hdnLocaField" runat="server" Value='<%# Eval("LocaField") %>' />
                                            <asp:HiddenField ID="hdnParameterText" runat="server" Value='<%# Eval("ParameterText") %>' />
                                        </td>
                                        <td align="left">
                                            <asp:Literal ID="literalValue" runat="server" Text="<hr />" Visible='<%# Eval("Type").ToString() == "BREAK" %>' />
                                            <asp:TextBox ID="txtValue" runat="server"
                                                Visible='<%# Eval("Type").ToString() == "LIST" || Eval("Type").ToString() == "TEXT" || Eval("Type").ToString() == "NUMBER" || Eval("Type").ToString() == "DATE" %>' 
                                                Text='<%# Eval("Value") %>'
                                                AutoPostBack='<%# Eval("AutoPostBack") %>' OnTextChanged="txtValue_TextChanged">
                                            </asp:TextBox>
                                            <asp:ImageButton ID="imgCalendar" runat="server"
                                                Style="margin-left:-3px;" ImageUrl="~/Images/calendar.png"
                                                Visible='<%# Eval("Type").ToString() == "DATE" %>' />
                                            <ajaxToolkit:CalendarExtender ID="calDate" runat="server"
                                                FirstDayOfWeek="Sunday" Format='<%# GetCalendarFormat() %>'
                                                PopupButtonID="imgCalendar" TargetControlID="txtValue"
                                                Enabled='<%# Eval("Type").ToString() == "DATE" %>' />
                                            <ajaxToolkit:MaskedEditExtender ID="meeDate" runat="server" 
                                                Mask="99/99/9999" MaskType="Date" UserDateFormat='<%# GetUserDateFormat() %>'
                                                CultureName='<%# GetCultureName() %>' TargetControlID="txtValue"
                                                Enabled='<%# Eval("Type").ToString() == "DATE" %>'>
                                            </ajaxToolkit:MaskedEditExtender>
                                            <ajaxToolkit:MaskedEditExtender ID="meeNumber" runat="server"
                                               Mask='<%# !string.IsNullOrEmpty(Eval("Mask").ToString()) ? Eval("Mask").ToString() : "9999999999" %>'
                                               AutoComplete=false MaskType="NONE"
                                               TargetControlID="txtValue" Enabled='<%# Eval("Type").ToString() == "NUMBER" || !string.IsNullOrEmpty(Eval("Mask").ToString()) %>'>
                                            </ajaxToolkit:MaskedEditExtender>
                                            <asp:HiddenField ID="hdnLookup" runat="server" Value='<%# Eval("Lookup").ToString() %>' />
                                            <asp:HiddenField ID="hdnValue" runat="server" Value='<%# Eval("Value").ToString() %>' />
                                            <asp:PlaceHolder ID="plhLookup" runat="server" Visible='<%# Eval("Type").ToString() == "CODE" || 
                                                    Eval("Type").ToString() == "CODE_ALWAYS" || Eval("Type").ToString() == "CODE_READONLY" %>' >
                                                <plc:FlexBox ID="chLookup" runat="server" Width="400px" TableName='<%# Eval("Lookup").ToString() %>' 
                                                    CodeCondition='<%# Eval("CodeCondition").ToString() %>'  SelectedValue='<%# Eval("Value").ToString() %>' 
                                                    Enabled='<%# (Eval("Type").ToString() != "CODE_READONLY") %>' ShowActiveOnly="false"
                                                    AutoPostBack='<%# Eval("AutoPostBack") %>' OnValueChanged="chLookup_ValueChanged">
                                                </plc:FlexBox>
                                            </asp:PlaceHolder>
                                            <asp:PlaceHolder ID="plhCustody" runat="server" Visible='<%# Eval("Type").ToString() == "CUSTODY" %>'>
                                                <table cellpadding="0" cellspacing="0">
                                                  <tr>
                                                      <td><plc:FlexBox ID="flxCustCode" runat="server"  AutoPostBack="true"  OnValueChanged="flxCustCode_ValueChanged" Width="275px" TableName="TV_CUSTCODE"  CodeCondition='<%# Eval("CodeCondition").ToString() %>' SelectedValue='<%# Eval("Value").ToString() %>' ></plc:FlexBox></td>
                                                      <td>Location&nbsp;</td>
                                                      <td><plc:FlexBox ID="flxCustCodeLocation" runat="server" Width="300px" TableName="CV_CUSTLOC" SelectedValue='<%# Eval("LocaValue").ToString() %>' CodeCondition='<%# Eval("CustLocCodeCondition").ToString() %>'></plc:FlexBox></td>
                                                  </tr>
                                                </table>
                                            </asp:PlaceHolder>
                                            <asp:PlaceHolder ID="plhNumberRange" runat="server" Visible='<%# Eval("Type").ToString() == "NUMBERRANGE" %>'>
                                                <table cellpadding="0" cellspacing="0">
                                                    <tr>
                                                        <td>
                                                        </td>
                                                        <td>
                                                            <asp:TextBox ID="txtNumberStart" runat="server" Text='<%# GetDate(Eval("Value").ToString(), 0) %>'></asp:TextBox>
                                                            <ajaxToolkit:MaskedEditExtender ID="meeNumberStart" runat="server"
                                                               Mask='<%# !string.IsNullOrEmpty(Eval("Mask").ToString()) ? Eval("Mask").ToString() : "9999999" %>'
                                                                AutoComplete="false" MaskType="NONE" TargetControlID="txtNumberStart">
                                                            </ajaxToolkit:MaskedEditExtender>
                                                        </td>
                                                        <td>
                                                            &nbsp;to&nbsp;
                                                        </td>
                                                        <td>
                                                            <asp:TextBox ID="txtNumberEnd" runat="server" Text='<%# GetDate(Eval("Value").ToString(), 1) %>'></asp:TextBox>
                                                            <ajaxToolkit:MaskedEditExtender ID="meeNumberEnd" runat="server"
                                                               Mask='<%# !string.IsNullOrEmpty(Eval("Mask").ToString()) ? Eval("Mask").ToString() : "9999999" %>'
                                                               AutoComplete="false" MaskType="NONE" TargetControlID="txtNumberEnd">
                                                            </ajaxToolkit:MaskedEditExtender>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </asp:PlaceHolder>
                                            <asp:PlaceHolder ID="plhDateRange" runat="server" Visible='<%# Eval("Type").ToString() == "DATERANGE" %>'>
                                                <table cellpadding="0" cellspacing="0">
                                                    <tr>
                                                        <td>
                                                        </td>
                                                        <td>
                                                            <asp:TextBox ID="txtDateStart" runat="server" Text='<%# GetDate(Eval("Value").ToString(), 0) %>'></asp:TextBox>
                                                            <asp:ImageButton ID="ibDateStart" Style="margin-left:1px;" runat="server" ImageUrl="~/Images/calendar.png" />
                                                            <ajaxToolkit:CalendarExtender ID="calDateStart" FirstDayOfWeek="Sunday" Format='<%# GetCalendarFormat() %>'
                                                                PopupButtonID="ibDateStart" TargetControlID="txtDateStart" runat="server" />
                                                            <ajaxToolkit:MaskedEditExtender ID="meeDateStart" runat="server" Mask="99/99/9999" UserDateFormat='<%# GetUserDateFormat() %>'
                                                                MaskType="Date" CultureName='<%# GetCultureName() %>' TargetControlID="txtDateStart">
                                                            </ajaxToolkit:MaskedEditExtender>
                                                        </td>
                                                        <td>
                                                            &nbsp;to&nbsp;
                                                        </td>
                                                        <td>
                                                            <asp:TextBox ID="txtDateEnd" runat="server" Text='<%# GetDate(Eval("Value").ToString(), 1) %>'></asp:TextBox>
                                                            <asp:ImageButton ID="ibDateEnd" Style="margin-left:1px;" runat="server" ImageUrl="~/Images/calendar.png" />
                                                            <ajaxToolkit:CalendarExtender ID="calDateEnd" FirstDayOfWeek="Sunday" Format='<%# GetCalendarFormat() %>'
                                                                PopupButtonID="ibDateEnd" TargetControlID="txtDateEnd" runat="server"/>
                                                            <ajaxToolkit:MaskedEditExtender ID="meeDateEnd" runat="server" Mask="99/99/9999" UserDateFormat='<%# GetUserDateFormat() %>'
                                                                MaskType="Date" CultureName='<%# GetCultureName() %>' TargetControlID="txtDateEnd">
                                                            </ajaxToolkit:MaskedEditExtender>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </asp:PlaceHolder>
                                            <asp:PlaceHolder ID="phMultiLookup" runat="server"
                                                Visible='<%# Eval("Type").ToString().StartsWith("MULTIPICK" )%>'>
                                                <plc:CodeMultiPick ID="cmpMultiLookup" runat="server" Width="400px"
                                                    UsesSearchBar="true"
                                                    UpdateOnRender="true"
                                                    HeaderPrompt='<%# Eval("Name") %>'
                                                    CodeTable='<%# Eval("Lookup").ToString() %>' 
                                                    WhereClause='<%# Eval("CodeCondition").ToString() %>'
                                                    InitText='<%# Eval("Value").ToString() %>'>
                                                    <%--Enabled='<%# (Eval("Type").ToString() != "MULTIPICK_READONLY") || (Eval("Value").ToString() == string.Empty) %>'>--%>
                                                </plc:CodeMultiPick>
                                            </asp:PlaceHolder>
                                            <asp:HiddenField ID="hdnType" runat="server" Value='<%# Eval("Type") %>' />
                                            <asp:HiddenField ID="hdnRequired" runat="server" Value='<%# Eval("Required") %>' />
                                        </td>
                                    </tr>
                                </ItemTemplate>
                                <FooterTemplate>
                                    </table>
                                </FooterTemplate>
                            </asp:Repeater>
                        </div>
                    </asp:Panel>
                </div><br />
                <div>
                    <asp:Button ID="btnViewReport" runat="server" Text="View Report" Width="120"  OnClick="btnViewReport_Click" />
                    <asp:Button ID="btnCancel" runat="server" Text="Cancel" Width="120" OnClick="btnCancel_Click" />
                </div>
                <div style="padding-top: 5px;">
                    <asp:Label ID="lblMessage" runat="server" Text="" ForeColor="Red"></asp:Label>
                </div>
            </div>
        </asp:View>
    </asp:MultiView>
    
    <div id='divUploadReport' style="display:none;">
        <asp:TextBox ID="txtFileName" runat="server" Width="500px" ReadOnly="true" BackColor="LightGray"></asp:TextBox>
        <asp:FileUpload ID="fuUploadReport" runat="server" Width="300px" onkeydown="return false;" onchange="__fuReportChange();" accept="application/x-rpt" style="display:none;" />
        <br /><hr />
        <div id="dvUploadMessage">
            <asp:Label ID="lblUploadMessage" runat="server" Text="No file selected" ForeColor="Red"/>
            <br />
        </div>
        <br />
        <asp:Button ID="btnUploadReport" runat="server" Text="Upload" OnClick="btnUploadReport_Click" />
        <asp:Button ID="btnCancelUpload" runat="server" Text="Cancel" OnClientClick="__closeUploadDialog(); return false;"/>
        <asp:Button ID="btnBrowseFile" runat="server" Text="Browse..." OnClientClick="__showBrowseDialog(); return false;" style="float:right;" />
    </div>

    <a href="DownloadReport.ashx" id="dlReport" style="display: none;">
        
    <script type="text/javascript">
        $(document).ready(function () {
            // Target both number range fields inside the repeater
            $(document).on('blur', '[id$="txtNumberStart"], [id$="txtNumberEnd"]', function () {
                console.log("Blured!");
                var $txt = $(this);
                var val = $txt.val().trim();
                if (val === "") return;

                var extenderId = $txt.attr('id').replace('txtNumber', 'meeNumber');
                console.log("extenderId: " + extenderId);
                var $extender = $('[id$="' + extenderId + '"]');
                console.log("$extender: " + $extender);

                var mask = $extender.attr('mask') || "";
                console.log("mask: " + mask);
                var isDecimal = mask.includes('.');
                console.log("isDecimal: " + isDecimal);

                if (isDecimal) {
                    var num = parseFloat(val);
                    if (!isNaN(num)) {
                        // Determine decimal count based on mask, e.g. "9.99" → 2 decimals
                        var decimals = mask.split('.')[1]?.length || 2;
                        console.log("decimals: " + decimals);
                        $txt.val(num.toFixed(decimals));
                    }
                }
            });
        });

        function __dlReport(crk) {
            var e = document.getElementById('dlReport');
            e.href = 'downloadreport.ashx?crk=' + crk;
            e.click();
        }

        function __showBrowseDialog() {
            document.getElementById('<%=fuUploadReport.ClientID%>').click();  
        }

        //enclose a div into a jquery dialog
        function __showUploadDialog(title) {
            $div = $('#divUploadReport');
            
            __enableUploadButton(false);

            var $dlg = $div.dialog({
                modal: true,
                title: title ? title : document.title,
                autoOpen: true,
                width: 'auto',
                height: 'auto',
                resizable: false,
                closeOnEscape: false,
                close: function (event, ui) {
                },
                open: function (event, ui) {
                    $(this).closest('.ui-dialog').find('.ui-dialog-titlebar-close').hide();
                }
            });

            $dlg.parent().appendTo(jQuery("form:first"));
        }

        function __closeUploadDialog() {
            $("#divUploadReport").dialog('destroy');
        }

        function __enableUploadButton(enabled) {
            document.getElementById('<%= btnUploadReport.ClientID %>').disabled = !enabled;
        }

        function __fuReportChange() {
            var $e = $("#<%= fuUploadReport.ClientID %>");
            var fileName = $e.val().replace(/.*(\/|\\)/, '');
            if (fileName) {
                if (fileName.toUpperCase().endsWith('.RPT')) {
                    $("#<%= txtFileName.ClientID %>").val($e.val());
                    __enableUploadButton(true);
                    __showUploadUserMessage();
                } else {
                    $e.val('');
                    __showUploadUserMessage('Only .rpt files are allowed');
                }
            } else {
                __enableUploadButton(false);
                __showUploadUserMessage('No file selected');
            }
        }

        // show or hide message
        function __showUploadUserMessage(msg) {
            var $e = $("#<%= lblUploadMessage.ClientID %>");
            var $c = $("#dvUploadMessage");

            msg = msg ? msg : '';
            $e.text(msg);

            if (msg) {
                $c.show();
            } else {
                $c.hide();
            }
        }

        function __showAlert(msg, title, type, okFn) {
            var m = msg ? msg : "";
            var left = {
                float: 'left',
                width: '10%',
                height: '100%'
            };

            var right = {
                float: 'left',
                width: '90%',
                'text-align': 'justify',
                height: '100%'
            };

            var icon = 'info';

            if (type === 'error') {
                icon = 'alert';
            }

            var $span = $("<span class='ui-icon ui-icon-" + icon + "' style='float:left; margin:0 7px 20px 0;'></span>");
            $span.css({
                "-ms-transform": "scale(1.2)", /* IE 9 */
                "-webkit-transform": "scale(1.2)", /* Chrome, Safari, Opera */
                "transform": "scale(1.2)"
            });

            var $icon = $("<div></div>").append($span);
            $icon.css(left);

            var $message = $("<div>" + m + "</div>");
            $message.css(right);

            $div = $("<div></div>");

            $div.append($icon).append($message);

            var t = title ? title : document.title;
            $div.dialog({
                modal: true,
                title: t,
                autoOpen: true,
                width: '350px',
                resizable: false,
                draggable: false,
                height: 'auto',
                buttons: {
                    Ok: function () {
                        if (okFn && typeof okFn === "function") {
                            okFn();
                        }
                        $(this).dialog("close");
                    }
                },
                close: function (event, ui) {
                    $(this).remove();
                },
                open: function (event, ui) {
                    $(this).closest('.ui-dialog').find('.ui-dialog-titlebar-close').hide();
                }
            });

            $div.parent().appendTo(jQuery("form:first"));
        }

        function clickPageEventsButton() {
            window.setTimeout(function () {
                $("[id$='btnPageEvents']").click();
            }, 100);
        }
    </script>
</asp:Content>