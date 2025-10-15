<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PLCDialog.ascx.cs" Inherits="PLCWebCommon.PLCDialog" %>

<style type="text/css">
    .ui-dialog-buttonpane {
        padding: 0px !important;
    }
        
    .ui-button-text {
        line-height: 0.8 !important;
    }
</style>

<div id="dvDialog" runat="server" style="display:none;" >
    <p id="pDialog" runat="server" style='min-height:50px; max-height:400px; overflow-y:auto;'>
        <span id="spIcon" runat="server" class='ui-icon ui-icon-info' style='float:left; margin:0 7px 20px 0;'></span>
        <span id="spMessage" runat="server" style='display:table-cell' ></span>
    </p>
</div>
<asp:Button ID="btnDialogConfirm" runat="server" OnClick="btnDialogConfirm_Click" style="display:none;" />
<asp:Button ID="btnDialogCancel" runat="server" sourceButton="" OnClick="btnDialogCancel_Click" style="display:none;" />
<asp:Button ID="btnDialogOk" runat="server" OnClick="btnDialogOk_Click" style="display:none;" />

<script type="text/javascript">
    function <%= this.ID %>_ShowAlert(title, message, maxHeight, width) {
        $("#<%= pDialog.ClientID %>").css("max-height", maxHeight);
        $("#<%= spIcon.ClientID %>").attr("class", "ui-icon ui-icon-alert");
        $("#<%= spMessage.ClientID %>").html(message);    
        $("#<%= dvDialog.ClientID %>").dialog({
            title: title,
            width: width,
            modal: true,
            resizable: false,
            draggable: false,
            closeOnEscape: false,
            buttons: {
                Ok: function () {
                    $(this).dialog("close");
                }
            },
            close: function () {
                // Add event when closing dialog via close icon, esc keyboard, ok button
                var $btnDialogOk = $("#<%= btnDialogOk.ClientID %>");
                if ($btnDialogOk.attr("autopostback") == "T")
                    $("#<%= btnDialogOk.ClientID %>").click();
            },
            open: function(event, ui){
                $(this).closest('.ui-dialog').find('.ui-dialog-titlebar-close').hide();
            }
        });
    }

    function <%= this.ID %>_ShowConfirm(title, message, maxHeight, width) {
        $("#<%= pDialog.ClientID %>").css("max-height", maxHeight);
        $("#<%= spIcon.ClientID %>").attr("class", "ui-icon ui-icon-info");
        $("#<%= spMessage.ClientID %>").html(message);    
        $("#<%= dvDialog.ClientID %>").dialog({
            title: title,
            width: width,
            modal: true,
            resizable: false,
            draggable: false,
            closeOnEscape: false,
            buttons: {
                Ok: function () {
                    $(this).dialog("close");
                    $("#<%= btnDialogConfirm.ClientID %>").click();
                },
                Cancel: function () {
                    $(this).dialog("close");
                    $("#<%= btnDialogCancel.ClientID %>").click();
                }
            },
            open: function(event, ui){
                $(this).closest('.ui-dialog').find('.ui-dialog-titlebar-close').hide();
            }
        });
    }

    function <%= this.ID %>_ShowYesNo(title, message, maxHeight, width, funcYes, funcNo) {
        $("#<%= pDialog.ClientID %>").css("max-height", maxHeight);
        $("#<%= spIcon.ClientID %>").attr("class", "ui-icon ui-icon-info");
        $("#<%= spMessage.ClientID %>").html(message);    
        $("#<%= dvDialog.ClientID %>").dialog({
            title: title,
            width: width,
            modal: true,
            resizable: false,
            draggable: false,
            closeOnEscape: false,
            buttons: {
                Yes: function () {
                    $(this).dialog("close");
                    if (funcYes) { funcYes(); }
                    $("#<%= btnDialogConfirm.ClientID %>").click();
                },
                No: function () {
                    $(this).dialog("close");
                    if (funcNo) { funcNo(); }
                    $("#<%= btnDialogCancel.ClientID %>").click();
                }
            },
            open: function(event, ui){
                $(this).closest('.ui-dialog').find('.ui-dialog-titlebar-close').hide();
            }
        });
    }

    function <%= this.ID %>_ShowYesNoOnly(title, message, maxHeight, width, iconType) {
        $("#<%= pDialog.ClientID %>").css("max-height", maxHeight);
        $("#<%= spIcon.ClientID %>").attr("class", "ui-icon ui-icon-" + iconType);
        $("#<%= spMessage.ClientID %>").html(message);    
        $("#<%= dvDialog.ClientID %>").dialog({
            title: title,
            width: width,
            modal: true,
            resizable: false,
            draggable: true,
            closeOnEscape: false,
			dialogClass: "no-close",
            buttons: {
                Yes: function () {
                    $(this).dialog("close");
                    if (typeof funcYes != "undefined") { funcYes(); }

                    if ($("#<%= btnDialogConfirm.ClientID %>").attr("autopostback") == "T")
                        $("#<%= btnDialogConfirm.ClientID %>").click();
                },
                No: function () {
                    $(this).dialog("close");
                    if (typeof funcNo != "undefined") { funcNo(); }

                    if ($("#<%= btnDialogCancel.ClientID %>").attr("autopostback") == "T")
                        $("#<%= btnDialogCancel.ClientID %>").click();
                }
            },
            open: function(event, ui){
                $(this).closest('.ui-dialog').find('.ui-dialog-titlebar-close').hide();
            }
        });
    }

    function <%= this.ID %>_ShowMsg(title, message, maxHeight, width, iconType, okText, cancelText) {
        // Icon setup
        $('#<%= spIcon.ClientID %>').removeClass().addClass('ui-icon ui-icon-' + iconType);

        // Message text
        $('#<%= spMessage.ClientID %>').html(message);

        // Show the comment box
        $('#<%= dvDialogComment.ClientID %>').show();
        $('#<%= txtDialogComment.ClientID %>').val('');

        // Build dialog box dynamically
        $('#<%= dvDialog.ClientID %>').dialog({
            title: title,
            modal: true,
            width: width,
            maxHeight: maxHeight,
            resizable: false,
            buttons: [
                {
                    text: okText,
                    click: function () {
                        // Store comment in hidden field or textbox (already runat=server)
                        __doPostBack('<%= btnDialogOk.UniqueID %>', '');
                        $(this).dialog('close');
                    }
                },
                {
                    text: cancelText,
                    click: function () {
                        __doPostBack('<%= btnDialogCancel.UniqueID %>', '');
                        $(this).dialog('close');
                    }
                }
            ]
        });

        // Optional autofocus to comment box
        $('#<%= txtDialogComment.ClientID %>').focus();
    }
</script>