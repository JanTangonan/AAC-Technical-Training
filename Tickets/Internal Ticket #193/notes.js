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


$(document).on("keydown", function (e) {
    if (e.key === "Enter") {
        // If expunge popup is visible, override default Enter behavior
        if ($("#expungeDialog").is(":visible")) {
            e.preventDefault(); // prevent any global default
            $("#btnOKExpunge:visible, #btnOKAdminRemoval:visible").click();
        }
    }
});



if ($btnExpunge.length) {
    __doPostBack('<%= btnOKExpunge.UniqueID %>', '');
} else if ($btnAdmin.length) {
    __doPostBack('<%= btnOKAdminRemoval.UniqueID %>', '');
}