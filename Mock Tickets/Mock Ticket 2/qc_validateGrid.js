// Collection of required fields        
var nameReqFields = ["NAME_TYPE_CODE", "LAST_NAME"];

function ValidateGrid($targetTable, rowPrefix, fieldSuffix) {
    var isValid = true;
    var $rows = $targetTable.find("[id^=" + rowPrefix + "]");

    for (var i = 0; i < $rows.length; i++) {
        var fieldSelector = "__" + fieldSuffix + i;
        var $rowFields = $($rows[i]).find("[id$=" + fieldSelector + "]");
        var rowID = $($rows[i]).attr("id");
        if (fieldSuffix == "ENEF")
            var $chkbox = $($rows[i]).find("[id^=CHKBOX_" + fieldSuffix + i + "]");

        if ($rowFields.length > 0) {
            //var limit = limit = $rowFields.length - 4; // horizontal limit for items and names
            var limit = $rowFields.length;

            if (name.HasData) {
                var entry = {};

                for (var j = 0; j < limit; j++) {
                    var $field = $($rowFields[j]);

                    if ($field.length > 0) {
                        var fieldID = $field.attr("id");
                        var fieldName = "";
                        var fieldVal = "";

                        if (fieldID.indexOf("fbLookup") > -1) {
                            fieldName = fieldID.replace("_fbLookup" + fieldSelector, "");
                            fieldVal = $field.getCode();
                        }
                        else {
                            fieldName = fieldID.replace(fieldSelector, "");
                            fieldVal = $field.val();
                        }

                        var required = IsFieldRequired(fieldName, fieldSuffix);

                        if (fieldSuffix == "ENEF") {
                            if ($chkbox.length > 0 && $chkbox.is(":checked")) {
                                if (required && fieldVal.trim() == "") {
                                    isValid = false;
                                    break;
                                }
                            }
                        }
                        else {
                            if (required && fieldVal.trim() == "") {
                                isValid = false;
                                entry = {};
                                break;
                            }
                        }

                    }
                }
            }
        }

        if (!isValid)
            break;
    }

    return isValid;
}

//checks for grid fields marked 'required'
function IsFieldRequired(fieldName, fieldSuffix) {
    var isRequired = false;
    var reqFields = null;

    if (fieldSuffix == "NEF" || fieldSuffix == "ENEF")
        reqFields = nameReqFields;
    else if (fieldSuffix == "IEF")
        reqFields = itemReqFields;
    else if (fieldSuffix == "DEF")
        reqFields = distributionReqFields;

    for (var i = 0; i < reqFields.length; i++) {
        if (fieldName == reqFields[i]) {
            isRequired = true;
            break;
        }
    }

    return isRequired;
}

function RowContainsData(rowID, fieldSelector, limit) {
    var isRowPopulated = false;

    for (var i = 0; i <= limit; i++) {
        var $row = $("#" + rowID).find(fieldSelector).eq(i);
        var value = "";

        if ($row.attr('type') == "hidden" || $row.closest("td").css("display") == "none")
            continue; 

        if ($row.length > 0) {
            var fieldID = $row.attr("id");
            if (fieldID.indexOf("DELETE_BUTTON") < 0) {
                if (fieldID.indexOf("_fbLookup") > 0)
                    value = $row.getCode();
                else
                    value = $row.val();

                if (value != "") {
                    //check if quantity field is populated but all other preceding fields are empty.
                    //and value equals default item quantity value
                    //if condition is true, ignore the quantity value and do not consider it as populated
                    if (fieldID.toUpperCase().indexOf("QUANTITY") >= 0
                        && !isRowPopulated
                        && value == GetDeptCtrl("DEFAULT_ITEM_QUANTITY")) {
                        isRowPopulated = false;
                    }
                    else
                        isRowPopulated = true;
                }
            }
        }
    }

    return isRowPopulated;
}

function RemoveArrayElement(arr, item) {
    for (var i = arr.length; i >= 0; i--) {
        if (arr[i] == item) {
            arr.splice(i, 1);
            break;
        }
    }
}