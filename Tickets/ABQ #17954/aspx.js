function onSaveCompClientClick(btn) {
    // run your existing validation
    var isValid = isKitComponentsGridValid();

    if (isValid) {
        // ✅ Only disable when valid
        btn.disabled = true;
        btn.value = 'Saving...';
        return true; // proceed with postback
    } else {
        // ❌ Keep it clickable
        btn.disabled = false;
        return false; // cancel postback
    }
}

function isKitComponentsGridValid() {
    var gridIsValid = true;
    var dialogTitle = 'Kit Components Inventory';
    var gridKitId = "<%= gvComponentKit.ClientID %>";
    var kitGridJson = getKitComponentGridData(gridKitId);
    var expirationDateRequired = getExpirationDatesRequired(kitGridJson);

    var validationList = {
        'expirationDateRequired': expirationDateRequired
    };

    if (expirationDateRequired.length > 0) {
        gridIsValid = false;
    }

    // not valid? show a message
    if (!gridIsValid) {
        message = '<div>Expiration date is required for the following kit item/s: <br/>' +
            expirationDateRequired.join(', ') + '</div>';
        showDialog(dialogTitle, message);
    }

    return gridIsValid;
}