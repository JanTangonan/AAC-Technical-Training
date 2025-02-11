function ValidateNamesGrid() {
    var isValid = true;
    var $table = $("#tblNames"); // Target the Names Grid table
    var $rows = $table.find("tr:gt(0)"); // Skip header row

    $rows.each(function () {
        var $row = $(this);
        var hasData = false;
        var rowIsValid = true; // Track validation for this row

        console.log("Checking row...");

        // Loop through required fields
        for (var i = 0; i < nameReqFields.length; i++) {
            var fieldSelector = "[id^='NAME_" + nameReqFields[i] + "']";
            var $field = $row.find(fieldSelector);

            console.log("Checking field: " + fieldSelector);

            if ($field.length > 0) {
                var fieldVal = $field.val().trim();

                // Check if the field has any data
                if (fieldVal !== "") {
                    hasData = true; // The row has some input
                    console.log("Row has input.");
                } 

                // If the row has data but this field is required and empty, mark invalid
                if (hasData && fieldVal === "") {
                    rowIsValid = false;
                    console.log("Missing required field: " + fieldSelector);
                    //$field.addClass("alert-required"); // Optional: Highlight empty fields
                }
            }
        }

        // If the row has data but failed validation, mark the whole form as invalid
        if (hasData && !rowIsValid) {
            isValid = false;
            return false; // Stop further checking
        }

        console.log("Row validation status: " + rowIsValid);
    });

    console.log("Final validation status: " + isValid);
    return isValid;
}