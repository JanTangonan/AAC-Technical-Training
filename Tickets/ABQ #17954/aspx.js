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

function handleSaveButtonClick(e) {
    e.preventDefault(); // stop default postback

    var btn = document.getElementById('<%= btnSaveComp.ClientID %>');
    btn.disabled = true; // disable immediately

    // run validation
    if (!isKitComponentsGridValid()) {
        btn.disabled = false; // re-enable if invalid
        return false;
    }

    // force postback manually
    __doPostBack('<%= btnSaveComp.UniqueID %>', '');
    return false; // prevent default, since we manually triggered it
}


function handleSaveButtonClick(e) {
    e.preventDefault(); // stop default postback

    var btn = document.getElementById('<%= btnSaveComp.ClientID %>');
    btn.disabled = true; // disable immediately

    // run validation
    if (!isKitComponentsGridValid()) {
        btn.disabled = false; // re-enable if invalid
        return false;
    }

    // force postback manually
    __doPostBack('<%= btnSaveComp.UniqueID %>', '');
    return false; // prevent default, since we manually triggered it
}

function tryDisableButton() {
    var button = document.getElementById('<%= btnSaveComp.ClientID %>');
    var isValid = isKitComponentsGridValid();

    alert(isValid);
    if (isValid) {
        button.disabled = true;
        button.value = 'Saving...';
    }

    return isValid;
}

function isKitComponentsGridValid() {
    alert("Validation is running 1!");

    var gridIsValid = true;
    alert("Validation is running 2!");
    var dialogTitle = 'Kit Components Inventory';
    alert("Validation is running 2.1!");
    var gridKitId = "<%= gvComponentKit.ClientID %>";
    alert("Validation is running 2.2!");
    var kitGridJson = getKitComponentGridData(gridKitId);
    alert("Validation is running 2.3!");
    var expirationDateRequired = getExpirationDatesRequired(kitGridJson);

    alert("Validation is running 3!");

    var validationList = {
        'expirationDateRequired': expirationDateRequired
    };

    alert("Validation is running 4!");

    if (expirationDateRequired.length > 0) {
        gridIsValid = false;
    }

    alert("Validation is running 5!");

    // not valid? show a message
    if (!gridIsValid) {
        message = '<div>Expiration date is required for the following kit item/s: <br/>' +
            expirationDateRequired.join(', ') + '</div>';
        showDialog(dialogTitle, message);
    }

    alert("Validation is running 6!");
    return gridIsValid;
}

///
<asp:Button ID="btnSaveComp"
    runat="server"
    Text="Save"
    Width="80px"
    OnClick="SaveKit_Click"
    OnClientClick="return disableAndPostback(this);"
    UseSubmitBehavior="false" />


function disableAndPostback(button) {
    if (isKitComponentsGridValid()) {
        // Disable the button and change text
        button.disabled = true;
        button.value = 'Saving...';

        // Manually trigger the postback
        __doPostBack('<%= btnSaveComp.UniqueID %>', '');
        return false; // Prevent the default postback
    }
    return false; // Validation failed, prevent postback
}
