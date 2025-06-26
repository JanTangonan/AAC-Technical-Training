function showExpungePopup(title) {
    $("[id='div-expunge']:eq(1)").dialog("destroy").remove();
    $("#div-expunge").dialog({
        title: title,
        resizable: false,
        autoOpen: true,
        modal: true,
        draggable: true,
        closeOnEscape: false,
        width: 500,
        height: 400,
        open: function (event, ui) {
            $(this).parent().appendTo("form");
            $(".ui-dialog-titlebar-close", ui.dialog | ui).hide();
        },
        close: function () {
            $(this).dialog("close");
        }
    });
    return false;
}

