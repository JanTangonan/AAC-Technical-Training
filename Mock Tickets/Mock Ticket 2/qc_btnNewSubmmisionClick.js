$btnNewSubmission.click(function (event) {
    // Existing Case - submit case key (for new submission.)
    $hdnPreCaseState.val("existingcase");

    var labcasenumber = $txtCaseNumberSearch.val();
    LoadItemsCaselog(function () {
        StartLoadCaseDataFromLabCaseNumber(labcasenumber, function () { StartLoadCaseLogData(labcasenumber); });
        HidePreCaseContainer();
    });
});