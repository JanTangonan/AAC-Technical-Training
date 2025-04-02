function SaveAnalysisPanelSortOrder(verification) {
    var $sortDialog = $(".sortanalysis-panels");

    if ($bhvAPS) {
        var itemsCount = $("#pnlAnalysisSort option").length;

        $('[id*=btnSaveSort]').attr("disabled", "disabled");
        $('[id*=btnSaveSort]').val("Saving...")

        if (itemsCount > 0) {
            var sortCollection = {};
            sortCollection["verification"] = verification;
            $("#pnlAnalysisSort option").each(function (i) {
                sortCollection[i] = $("#pnlAnalysisSort option").eq(i).val();
            });

            BEASTiLIMS.MatrixControl.WSMatrix.SaveSortAnalysisPanel(sortCollection, SaveSort_OK, SaveSort_Error);
        }
    }

    function SaveSort_OK(e) {
        if (e == "OK") {
            $find("bhvAPS").hide();
            $('[id*=btnSaveSort]').removeAttr("disabled");
            $('[id*=btnSaveSort]').val("Save");
            location.reload();
        }
        else
            SaveSort_Error(e);
    }

    function SaveSort_Error(e) {
        ShowAlert("Alert", "There was an error during the analysis sorting process.");
        $find("bhvAPS").hide();
        $('[id*=btnSaveSort]').removeAttr("disabled");
        $('[id*=btnSaveSort]').val("Save");
    }
}