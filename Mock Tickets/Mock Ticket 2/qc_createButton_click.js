function Create_Click(btn) {
    var $btn = $(btn);

    if (!!$btn.data("qcCreateClicked"))
        return;

    $btn.data("qcCreateClicked", true);

    if ($("#expandModal:visible").length) {
        $("#expandModal a.ui-dialog-titlebar-close").click();
    }

//    if (GetLabCtrl("USES_CONTAINER_SPLITTER") == "T") {
//        if (!GetContainerItems())
//            return false;
    //    }

    if (!ValidateHighVolumeFieldRequirement()) {
        $btn.data("qcCreateClicked", false);
        return;
    }

    var fnDoSubmitQC = function () { ClearCaseSubmissionFieldsRequiredStatus(); disableDepartmentCaseField(); DoSubmitQC(btn); };
    
    // Validate case existence.
    if (GetEditState() == "New")
        CaseExistenceValidation_PreSubmit(fnDoSubmitQC);
    else {

        if (CanValidateSubmittingAndInvestigatingAgency())
            ValidateSubmittingAndInvestigatingAgency(fnDoSubmitQC);
        else
            ValidateCaseSubmissionFields(fnDoSubmitQC);
    }

    $btn.data("qcCreateClicked", false);
}