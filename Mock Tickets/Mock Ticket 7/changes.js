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

function OpenTransferSummaryDialog(items) { 
    console.log(items);
    const ul = document.getElementById("ulTransferSummary");
    ul.innerHTML = "";

    items.forEach(item => {
        // Extract LAB_ITEM_NUMBER from Description using regex
        const match = item.Description.match(/Item#:\s*(\d+)/);
        const labItemNumber = match ? match[1] : "N/A";

        const li = document.createElement("li");
        li.textContent = `${labItemNumber} ${item.CustodyDesc}`;
        ul.appendChild(li);
    });

    console.log("Opening Dialog");
    var position = { my: "center", at: "center", of: window };
    $('#mdialog-transfersummary').dialog('option', 'position', position);
    $('#mdialog-transfersummary').dialog("open");
}

function initTransferSummaryDialog() {
    console.log("initialize dialog");
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
                document.getElementById('<%= btnDialogConfirmed.ClientID %>').click();
            },
            Cancel: function () {
                $(this).dialog("close");
            }
        }
    });
}

$(function () {
    // Run this once when the page is ready
    console.log("initialize dialog");
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
                document.getElementById('<%= btnDialogConfirmed.ClientID %>').click();
            },
            Cancel: function () {
                $(this).dialog("close");
            }
        }
    });
});


this._popupManager.displayCustomConfirmationMessage(
    `The following items are missing:<br/><br/> ${itemDetailLabelList}<br/><br/><br/>Do you want to proceed with the transfer?`, () => { 
        if (this._checkTransferFields()) this._checkSignature(); this._transferButton.disabled = false; 
    }, () => { this._transferButton.disabled = false; });