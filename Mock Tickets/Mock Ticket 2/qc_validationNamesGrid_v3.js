function ValidateNamesGrid() {
    var isValid = true;
    var missingField = ""; // Variable to store the name of the missing field
    var $table = $("#tblNames");  
    var $rows = $table.find("tr:gt(0)");  
    var rownum = 1;

    $rows.each(function () {
        var $row = $(this);
        var hasData = false;
        var rowIsValid = true; 

        console.log("Checking row " + rownum);

        // Check if row has any input in any field
        $row.find("input, select, textarea").each(function () {
            if ($(this).attr("type") === "checkbox") {
                console.log("Checkbox detected - Name: " + this.name + ", Checked: " + $(this).prop("checked"));

                // Only consider checkboxes with checked state
                if ($(this).prop("checked")) {
                    hasData = true;
                }
            } else {
                console.log(this.name + ": " + $(this).val());

                if ($(this).val().trim() !== "") {
                    hasData = true;
                }
            }
        });

        // Check if all required fields are filled
        if (hasData) {
            for (var i = 0; i < nameReqFields.length; i++) {
                var fieldSelector = "[id^='NAME_" + nameReqFields[i] + "']";
                var $field = $row.find(fieldSelector);
                console.log("FIELD: " + $field);

                if ($field.length > 0) {
                    console.log("Field Value: " + $field.val());
                    var fieldVal = $field.val().trim();

                    // If any required field is empty, mark row invalid and store the missing field
                    if (fieldVal === "") {
                        rowIsValid = false;
                        missingField = nameReqFields[i]; // Store the missing field name
                        console.log("Missing required field: " + fieldSelector);
                        return false; // Exit the loop early
                    }
                }
            }
        }

        // If the row has data but is invalid, fail validation
        if (hasData && !rowIsValid) {
            isValid = false;
            console.log("Row failed validation");
            return false; // Exit the row loop early
        }

        rownum++;
        console.log("Row validation status: " + rowIsValid);
    });

    console.log("Final validation status: " + isValid);
    return { isValid: isValid, missingField: missingField }; // Return both validation status and missing field
}