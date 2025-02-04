function IsRequiredItemAttributesEntered() {
    var isAlertSet = false;
    var isItemGrid = false;
    $(".qcGrid .itemattrlookup").each(function() {
        if ($(this).hasClass("alert-required")) {
            isAlertSet = true;
            isItemGrid = $(this).attr('id').startsWith("ITEM");
            return false;
        }
    });
    //set the tab to be shown based on item alert-required location
    if (isAlertSet)
        isItemGrid ? ShowTab('divItems', $("a[id$='bnItems']")) : ShowTab('divEItems', $("a[id$='bnExistingItems']"));

    return !isAlertSet;
}

if (GetLabCtrl("QC_DISABLE_LABITEM_AUTOFILL") != "T") {
    $inputItemNum.unbind("blur").bind("blur", function () {
        var thisValue = $inputItemNum.val().replace(/_/g, '');
        if (thisValue.length <= 0) {
            ShowPopup("Please enter an Item Number.", function () {
                $inputItemNum.focus();
            });
        }
    });
}