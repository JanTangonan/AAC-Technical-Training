function ValidateNamesGrid() {
    var isValid = true;
    var $table = $("#tblNames"); // Target the Names Grid table
    var $rows = $table.find("tr:gt(0)"); // Skip header row

    $rows.each(function () {
        var $row = $(this);
        var hasData = false;
        var rowIsValid = true; // Track validation for this row

        console.log("Checking row...");

        // Check if the row has any input in any field (not just required fields)
        $row.find("input, select, textarea").each(function () {
            if ($(this).val().trim() !== "") {
                hasData = true;
            }
        });

        // If row has input, check if all required fields are filled
        if (hasData) {
            for (var i = 0; i < nameReqFields.length; i++) {
                var fieldSelector = "[id^='NAME_" + nameReqFields[i] + "']";
                var $field = $row.find(fieldSelector);

                if ($field.length > 0) {
                    var fieldVal = $field.val().trim();

                    // If any required field is empty, mark row invalid
                    if (fieldVal === "") {
                        rowIsValid = false;
                        console.log("Missing required field: " + fieldSelector);
                        //$field.addClass("alert-required"); // Optional: Highlight empty fields
                    }
                }
            }
        }

        // If the row has data but is invalid, fail validation
        if (hasData && !rowIsValid) {
            isValid = false;
            console.log("Row failed validation!");
            return false; // Stop further checking
        }

        console.log("Row validation status: " + rowIsValid);
    });

    console.log("Final validation status: " + isValid);
    return isValid;
}