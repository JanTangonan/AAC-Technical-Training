<script>
    var dialogTitle = 'Confirm';
    message = '<div>Are you sure you want to delete this?'</div>';
    showDialog(dialogTitle, message);

    function showDialog(title, message) {
        var $div = $("<div></div>");
        var $message = $("<div>" + message + "</div>");

        $div.append($message);

        var dlg = $div.dialog({
            modal: true,
            title: title ? title : document.title,
            autoOpen: true,
            width: 'auto',
            height: 'auto',
            zIndex: 10003,
            closeOnEscape: false,
            resizable: false,
            buttons: {
                Ok: function () {
                    $(this).dialog("close");
                }
            },
            close: function (event, ui) {
                $(this).remove();
            },
            open: function (event, ui) {
                $(".ui-dialog-titlebar-close", ui.dialog | ui).hide();
            }
        });
    //            dlg.parent().appendTo(jQuery("form:first"));
    }
</script>



function showDialog(title, message) {
    var $div = $("<div></div>");
    var $message = $("<div>" + message + "</div>");

    $div.append($message);

    var dlg = $div.dialog({
        modal: true,
        title: title ? title : document.title,
        autoOpen: true,
        width: 'auto',
        height: 'auto',
        zIndex: 10003,
        closeOnEscape: false,
        resizable: false,
        buttons: {
            Ok: function () {
                $(this).dialog("close");
            }
        },
        close: function (event, ui) {
            $(this).remove();
        },
        open: function (event, ui) {
            $(".ui-dialog-titlebar-close", ui.dialog | ui).hide();
        }
    });
//            dlg.parent().appendTo(jQuery("form:first"));
}