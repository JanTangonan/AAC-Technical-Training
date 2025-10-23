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

var btn = document.getElementById('<%= btnSaveComp.ClientID %>')
            btn.setAttribute("disabled", "disabled");


function isKitComponentsGridValid() {

    var btn = document.getElementById('<%= btnSaveComp.ClientID %>')
    btn.setAttribute("disabled", "disabled");

    var gridIsValid = true;
    var dialogTitle = 'Kit Components Inventory';
    var gridKitId = "<%= gvComponentKit.ClientID %>";
    var kitGridJson = getKitComponentGridData(gridKitId);
    var expirationDateRequired = getExpirationDatesRequired(kitGridJson);

    var validationList = {
        'expirationDateRequired': expirationDateRequired
    };

    console.log("expirationDateRequired.length: " + expirationDateRequired.length);
    if (expirationDateRequired.length > 0) {
        gridIsValid = false;
    }

    // not valid? show a message
    if (!gridIsValid) {
        message = '<div>Expiration date is required for the following kit item/s: <br/>' +
            expirationDateRequired.join(', ') + '</div>';
        showDialog(dialogTitle, message);

        btn.setAttribute("disabled", "enabled");
        return false;
    }

            <% --__doPostBack('<%= btnSaveComp.UniqueID %>', ''); --%>
            return gridIsValid;
}