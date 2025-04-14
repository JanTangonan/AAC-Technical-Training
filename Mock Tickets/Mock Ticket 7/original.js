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

function contentPageLoad() {
    gridviewFixPos();
    dbpanelAttachPopupToParent("#divpassword", "div[id$='phAuthorization']");

    var prm = Sys.WebForms.PageRequestManager.getInstance();
    if (prm != null) {
        prm.add_pageLoaded(windowFocus);
        prm.add_pageLoaded(getItemGridScrollPos);
    }

    function pageLoaded() {
        // Always attempt to re-initialize the dialog after UpdatePanel refresh
        initTransferSummaryDialog();
    }

    function windowFocus() {
        if (!_winPopup || _winPopup.closed)
            window.focus();
    }

    function getItemGridScrollPos() {
        $gridbody = $("[id$='gvItems'] > tbody");
        $pnlscroll = $("[id$='hdnPanel1ScrollTopPos']");

        $("[id$='gvItems']").addClass("fixed_headers");
        $gridbody.scroll(function () {
            OnContainerScroll(this, $pnlscroll.attr("id"));
        });

        if ($pnlscroll.val() != "")
            $gridbody.scrollTop($pnlscroll.val());
    }

    function initTransferSummaryDialog() {
        var $dialog = $("#mdialog-transfersummary");

        // Check if already initialized to avoid double-binding
        if ($dialog.hasClass("ui-dialog-content")) {
            $dialog.dialog("destroy"); // Rebuild it if it already exists (helps with UpdatePanel replacement)
        }

        $dialog.dialog({
            autoOpen: false,
            modal: true,
            resizable: false,
            draggable: false,
            title: 'Transfer Summary',
            width: 450,
            height: 500,
            buttons: {
                OK: function () {
                    $(this).dialog("close");
                    TransferItems();
                },
                Cancel: function () {
                    EnableButtons();
                    $(this).dialog("close");
                }
            }
        });
    }

    // Initialize once on first full page load
    //initTransferSummaryDialog();
    getItemGridScrollPos();
}