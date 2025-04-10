// Initialize Transfer Summary Dialog
$(function () {
    $("#mdialog-transfersummary").dialog({
        autoOpen: false,
        modal: true,
        resizable: false,
        draggable: false,
        title: 'Transfer Summary',
        width: 450,
        height: 500,
        buttons: {
            OK: function () {
                $(this).dialog("close");
                TransferItems();
            },
            Cancel: function () {
                EnableButtons();
                $(this).dialog("close");
            }
        }
    });
});

// Open Transfer Summary Dialog
function OpenTransferSummaryDialog() {
    var itemsData  = $.parseJSON(GetServerSideControlValue("hdnItemsJSON")).ITEMS;
    console.log(itemsData );
    var selectedSummary = [];

    $("#ulTransferSummary").empty();

    $(".rowItem td:first-child input[type='checkbox']").each(function (index) {
        if ($(this).is(":checked")) {
            var itemData = itemsData[index];
            if (itemData) {
                var line = itemData.LAB_ITEM_NUMBER + " " + itemData.CUSTODY_DESC;
                console.log(line);
                selectedSummary.push("<li>" + line + "</li>");
            }
            console.log(selectedSummary);
        }
    });

    $("#ulTransferSummary").html(selectedSummary.join(""));

    var position = { my: "center", at: "center", of: window };
    $('#mdialog-transfersummary').dialog('option', 'position', position);
    $('#mdialog-transfersummary').dialog("open");
}


$("#mdialog-transfersummary").dialog({
    autoOpen: false,
    modal: true,
    resizable: false,
    draggable: false,
    title: 'Transfer Summary',
    width: 450,
    height: 500,
    buttons: {
        OK: function () {
            $(this).dialog("close");
            TransferItems();
        },
        Cancel: function () {
            EnableButtons();
            $(this).dialog("close");
        }
    }
});