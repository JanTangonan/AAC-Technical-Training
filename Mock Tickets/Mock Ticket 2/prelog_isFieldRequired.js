var namesGridIsValid = ValidateGrid($("#NameEntryList"), "TR_NEF", "NEF");

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

            if (RowContainsData(rowID, "[id$=" + fieldSelector + "]", limit)) {
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

                        if (fieldName == "ITEM_NUMBER" && fieldSuffix == "IEF")
                            ItemNumberQueue.push(fieldVal);

                        if (fieldSuffix == "NEF" && fieldID.indexOf("NAME_FIELDS_") >= 0) {
                            var data = $field.data("fieldvalues") || [];
                            if (data) {
                                entry["JUVENILE"] = data.juvenile;
                                entry["NAME_VERIFIED"] = data.nameverified;
                            }
                            else {
                                entry["JUVENILE"] = "";
                                entry["NAME_VERIFIED"] = "";
                            }
                        }
                        else if (fieldSuffix == "NEF" && fieldName == "SUBJECT_CHARGED") {
                            var $subjectChargedCB = $($rows[i]).find("[id^=SUBJECT_CHARGED_]");
                            $subjectChargedCB.is(":checked") ? entry["SUBJECT_CHARGED"] = "T" : entry["SUBJECT_CHARGED"] = "F";
                        }
                        else
                            entry[fieldName] = fieldVal;
                    }
                }

                if (fieldSuffix == "NEF") {
                    entry["RESUBMISSION"] = "";
                    CheckNamesRowIfSuspectVictimRequired(entry);
                    namesDict.push(JSON.stringify(entry));
                }
                else if (fieldSuffix == "IEF") {
                    entry["RESUBMISSION"] = "";
                    itemsDict.push(JSON.stringify(entry));
                }
                else if (fieldSuffix == "DEF") {
                    entry["RESUBMISSION"] = "";
                    distributionDict.push(JSON.stringify(entry));
                }
            }
        }

        if (!isValid)
            break;
    }

    return isValid;
}//checks for grid fields marked 'required'

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