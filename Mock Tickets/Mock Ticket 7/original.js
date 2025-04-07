function GetItemUserInputs() {
    var arr = [];

    $(".rowItem td:first-child input[type='checkbox']").each(function (index, value) {
        if ($(this).is(":checked")) {
            //ischecked = true;
            var row = $(this).closest('tr');
            var ecn = $(row).find("td:eq(" + INDEX_OpenAssignments + ") input[type='hidden']").first().val();
            var weight = "";
            var weightunit = "";
            var codes = "";
            var other = "";

            if (GetServerSideControlValue("hdnHideWeightCol") != "T") {
                weight = $(row).find("td:eq(" + INDEX_Weight + ") input[type='text']").first().val();
            }

            if (GetServerSideControlValue("hdnUsesWeightUnit") == "T") {
                weightunit = $(row).find("td:eq(" + INDEX_WeightUnits + ") div[id^='fbUnits_']").first().getCode();
            }

            //REFUSAL RIGHT
            var rr = $(row).find("td:eq(" + INDEX_RefusalReason + ")");
            if ($(rr).html() != null) {
                codes = $(rr).find("input[type='hidden']").first().val();
                other = $(rr).find("input[type='hidden']").last().val().replace(/"/g, '\\"');
            }

            arr.push('{"EVIDENCE_CONTROL_NUMBER":"' + ecn +
                '","WEIGHT":"' + weight +
                '","WEIGHT_UNITS":"' + weightunit +
                '","REASON_CODES":"' + codes +
                '","OTHER_REASON":"' + other + '"}');
        }
    });

    return arr.join(",");
}