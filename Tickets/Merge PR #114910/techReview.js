function showAdditionalRevRejectOptions() {
    var $parent = $(".dialog-additional-rev-reject").parent();
    var title = "Additional Review Reject Options";
    $(".dialog-additional-rev-reject").dialog({
        autoOpen: false,
        modal: true,
        resizable: false,
        draggable: false,
        dialogClass: "no-close",
        title: title,
        width: 350,
        height: 'auto',
        appendTo: "form",
        close: function () {
            $(".ui-dialog-titlebar-close", $(this).parent()).hide();

            $(this).parent().appendTo($parent);
        },
        open: function () {
            $("[id$='ddlRejectLevel']").val("");
        },
        buttons: {
            Ok: function () {
                var rejectOption = $("[id$='ddlRejectLevel']").val();
                if (!rejectOption) {
                    Alert("Please select reject level to reset assignment to.", { title: title });
                    return;
                }

                $("[id$='bnRejectAssign']").click();
                $(this).dialog("close");
            },
            Cancel: function () {
                $(this).dialog("close");
            }
        }
    }).dialog("open");
}

function initAutoSave() {
    $("[id$='hdnAutoSaveTime']").val($("[id$='hdnAutoSaveStart']").val());
    console.log("auto-save set to run once per " + (parseInt($("[id$='hdnAutoSaveStart']").val(), 10) / 60000) + " minute(s).");

    var intervalID = setInterval(function () {
        var currentAutoSave = parseInt($("[id$='hdnAutoSaveTime']").val(), 10);
        currentAutoSave -= 1000;

        if (currentAutoSave != 0 && currentAutoSave % 60000 == 0)
            console.log(currentAutoSave / 60000 + " minute(s) till next auto-save");
        else if (currentAutoSave == 30000)
            console.log("30 seconds till next auto-save");
        else if (currentAutoSave == 10000)
            console.log("10 seconds till next auto-save");
        else if (currentAutoSave == 3000)
            console.log("3 seconds till next auto-save");
        else if (currentAutoSave == 2000)
            console.log("2 seconds till next auto-save");
        else if (currentAutoSave == 1000)
            console.log("1 second till next auto-save");

        if (currentAutoSave <= 0) {
            console.log("running auto-save...")
            $("[id$='hdnAutoSaveTime']").val($("[id$='hdnAutoSaveStart']").val());
            $("[id$='hdnAutoSaveFocus']").val(document.activeElement.id);
            $("[id$='bnSave']").click();
        } else
            $("[id$='hdnAutoSaveTime']").val(currentAutoSave);
    }, 1000);
    
    $("[id$='hdnAutoSaveID']").val(intervalID);
}

function resetAutoSave() {
    var intervalID = $("[id$='hdnAutoSaveID']").val();
    var isSaveDisabled = $("[id$='bnSave']").prop("disabled");
    if (isSaveDisabled) {
        stopAutoSave();
    } else if (intervalID != "") {
        $("[id$='hdnAutoSaveTime']").val($("[id$='hdnAutoSaveStart']").val());
        console.log("resetting auto-save timer...");
    } else 
        initAutoSave();
}

function stopAutoSave() {
    var intervalID = $("[id$='hdnAutoSaveID']").val();
    if (intervalID != "") {
        clearInterval(intervalID);
        $("[id$='hdnAutoSaveID']").val("");
        console.log("auto-save stopped.");
    }
}

function refocusElement() {
    var focusElem = $("[id$='hdnAutoSaveFocus']").val();
    if (focusElem != "") {
        $("#" + focusElem).focus();
        $("[id$='hdnAutoSaveFocus']").val("");
    }
}