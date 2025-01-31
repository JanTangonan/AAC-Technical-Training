function SetPrelogCaseData(labcasenumber, fnPostImport) { // presets QC form page for prelog case import (new case/submission)
    if (labcasenumber == "")
        return;

    StartProgressDialog("Searching...");

    UpdateCaseSubmissionText();
    var jsonCaseSubmission = $("input[id$='hdnCaseSubmissionJSON']").val();

    var params = {
        "labcasenumber": labcasenumber,
        "jsoncasesubmission": jsonCaseSubmission,
        "editstate": GetEditState()
    };

    function PrelogLoadSuccess(retdata) {
        ResetSessionTimer();

        if (retdata.result == "ok") {
            ClearAll();
            SetEditState(retdata.editstate);
            ToggleGridVisibility();
            SetSubmissionTitle(retdata.casetitle);
            SetCaseKey(retdata.casekey);
            SetLastContainer(retdata.lastcontainer);

            CreateExistingItemsGrid(retdata.existingitems, true);
            CreateExistingNamesGrid(retdata.existingnames);
            ResetDefaultLabCode();
            ResetDefaultSubmissionType();

            if (IsDefined(fnPostImport))
                fnPostImport();

            CloseProgressDialog();

        }
        else if (retdata.result == "none") {
            ShowPopup("No prelog case found.");
            CloseProgressDialog();
        }
    }

    function PrelogLoadError(xhr, errortxt, e) {
        CloseProgressDialog();
        ShowError("Case Load Error", xhr, errortxt, e);
    }

    $.ajax({ type: "POST", url: "PLCWebCommon/PLCWebMethods.asmx/LoadCaseDataFromLabCaseNumber", data: params, success: PrelogLoadSuccess, error: PrelogLoadError, dataType: "json" });
}