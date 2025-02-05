var nameReqFields = ["NAME_TYPE_CODE", "LAST_NAME"];

function ValidateGrid(tableId) {
    var isValid = true;
    var $table = $("#" + tableId); // Get table by ID
    var $rows = $table.find("tr"); // Get all rows in the table

    $rows.each(function () {
        var $row = $(this);
        var name = nameRowContainsData($row); // Check if row contains data

        if (name.HasData) {
            nameReqFields.forEach(function (fieldName) {
                var $field = $row.find("[name='" + fieldName + "']"); // Find field by name
                if ($field.length > 0) {
                    var fieldVal = $field.val().trim();
                    if (fieldVal === "") {
                        isValid = false;
                        return false; // Exit loop if a required field is empty
                    }
                }
            });

            if (!isValid) {
                return false; // Exit row loop if validation fails
            }
        }
    });

    return isValid;
}