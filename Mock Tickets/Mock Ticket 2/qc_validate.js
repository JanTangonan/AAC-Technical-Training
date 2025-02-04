function ValidateCaseSubmissionFields(fnDoSubmit) {
    if (!ValidateUniqueDeptItemNumbers()) {
        ShowPopup("Duplicate " + GetDeptItemHeaderText() + " found.");
        return;
    }

    validateOnSave("PLCDBPanel1");
    ValidateDBPanel("PLCDBPanel1", fnDoSubmit);
}

function ValidateDBPanel(panelName, fnSubmit) {
    ClearPLCDBPanelRequiredStatus(panelName);
    disableDepartmentCaseField();

    var requiredFieldRecs = [];
    var mandatoryFieldRecs = [];
    var timedatefields = [];

    var dbpanelFields = GetDBPanelFields(panelName);
    for (var fieldname in dbpanelFields) {
        var fieldrec = dbpanelFields[fieldname];
        var fieldval = GetDBPanelField(panelName, fieldname).trim();
        var clientid = GetDBPanelClientID(panelName, fieldname);

        if ((fieldrec.required == "T") && (fieldval == "") && (fieldrec.hidefield != "T")) {
            var requiredfieldRec = { "clientid": clientid, "prompt": fieldrec.prompt };
            requiredFieldRecs.push(requiredfieldRec);

            SetClientHighlight(clientid, "requiredfield", "Yellow");
        }

        //collect field with TIME_DATE_FIELD value for validation
        var timeDateField = fieldrec.timedatefield;
        if (fieldval && timeDateField && timeDateField.startsWith("{") && timeDateField.endsWith("}")) {


            timeDateField = timeDateField.replace('{', '').replace('}', '');
            var timeDateFieldTableName = (timeDateField.indexOf("TV") >= 0 ? "" : getDBPanelTableName(panelName, timeDateField));
            var field = {
                fieldname: fieldname,
                datefieldname: timeDateFieldTableName + "." + timeDateField,
                nopast: fieldrec.nopastdates,
                nofuture: fieldrec.nofuturedates,
                datemask: dbpanelFields[timeDateFieldTableName + "." + timeDateField].editmask,
                currfieldmask: fieldrec.editmask,
                currfieldprompt: fieldrec.prompt

            };

            timedatefields.push(field);
        }
    }

    // Only check mandatory fields if all required fields have been entered.
    if (requiredFieldRecs.length == 0) {
        for (var fieldname in dbpanelFields) {
            var fieldrec = dbpanelFields[fieldname];
            var fieldval = GetDBPanelField(panelName, fieldname).trim();
            var clientid = GetDBPanelClientID(panelName, fieldname);

            if (fieldrec.required == "M" && (fieldval == "")) {
                var mandatoryfieldRec = { "clientid": clientid, "prompt": fieldrec.prompt };
                mandatoryFieldRecs.push(mandatoryfieldRec);

                SetClientHighlight(clientid, "mandatoryfield", "SpringGreen");
            }
        }
    }

    //        "fieldname":{"val":"%s", "required":"%s", "codetable":"%s", "codecondition":"%s", "fldtype":"%s", "codeuseskey":"%s", "keyval":"%s", "prompt":"%s"}
    //                    fieldname, EscapeJSON(fieldval), required, codetable, codecondition, fldtype, codeuseskey, EscapeJSON(keyval), EscapeJSON(prompt)));


    if (!IsFutureOrPastDateTimeField(timedatefields, panelName))
        return;

    if (requiredFieldRecs.length > 0) {
        $(document).scrollTop(0);
        ShowPopup("Fields highlighted in yellow are required.");
    }
    else if (IsUnderscoreInDeptCaseNotAllowed(panelName)) {
        $(document).scrollTop(0);
        ShowPopup("Underscore is not allowed in Department Case Number.");
    }
    else if (!IsRequiredItemAttributesEntered()) {
        ShowPopup("Please enter all required item attributes.");
    }
    else if (!IsRequiredItemNamesEntered()) {
        // Select Items grid.
        ShowTab('divItems', $("a[id$='bnItems']"));

        Alert("Please enter all required item-name link.");
    }
    else if (mandatoryFieldRecs.length > 0) {
        $(document).scrollTop(0);
        ShowYesNoPopup("Fields highlighted in green are blank. Submit anyway?", { "fnYes": fnSubmit });
    }
    else if (!isRequiredNameReferenceEntered())
    {
        ShowTab('divNames', $("a[id$='bnNames']"));
        ShowPopup("Fields highlighted in yellow are required.");
    }
    else {
        fnSubmit();
    }
}