var PATH_SubmitPrelog = "../PLCWebCommon/PLCWebMethods.asmx/SubmitPrelog";

function SubmitPrelog() {
    var opcode = "SAVE_PRELOG";

    if (GetDeptCtrl("NO_UNDERSCORE_IN_CASE_NUMBER") == "T" && GetDBPanelField(DBPanelName, "TV_SUBMTRAN.DEPARTMENT_CASE_NUMBER").includes("_")) {
        PrelogNotifier("Underscore is not allowed in Prelog Department Case Number.", false);
        return;
    }

    if (sessionPrelog_ProcessFlag == 2)
        opcode = "UPDATE_PRELOG";

    var dataToSend = {
        "opcode": opcode,
        "header": JSON.stringify(GetFullHeaderInfo()),
        "names": "{\"NEF\": [" + namesDict.toString() + "] }",
        "items": "{\"IEF\": [" + itemsDict.toString() + "] }", 
        "distribution": "{\"DEF\": [" + distributionDict.toString() + "] }",
        "namesforremoval": "{\"NEF\": [" + namesDelDict.toString() + "] }",
        "itemsforremoval": "{\"IEF\": [" + itemsDelDict.toString() + "] }",
        "submissionattachments": "{\"GUID\": \"" + $("#hdnCaseItemAttachGuid").val().replace(/\"/gi, "") + "\",\"M_ATTACH\": \"" + $("#hdnCaseAttachMarked").val() + "\"}"
    };

    isRemotePrelogMode = false;
    caseStatus = opcode;
    itemsDelDict = [];
    ChangeButtonStatus("[id*=btnSubmit]", "Transmitting...", true);
    ajaxObj(PATH_SubmitPrelog, SubmitPrelog_OnSuccess, SubmitPrelog_onError, "html", dataToSend);
}

function ajaxObj(targetURL, execFunctionOnSuccess, execFunctionOnError, typeOfResponse, dataForSending) {
    $.ajax({
        url: targetURL,
        success: execFunctionOnSuccess,
        error: execFunctionOnError,
        dataType: typeOfResponse,
        data: dataForSending,
        type: "POST"
    });
}

function SubmitPrelog_OnSuccess(e) {
    if (e.indexOf("success") > -1) {
        ChangeButtonStatus("[id*=btnSubmit]", "Redirecting...", true);
        PrelogNotifier(e, true);
        hasChanges = false;
        RedirectServiceRequests();
    }
    else if (e.indexOf("Error") > -1) {
        PrelogNotifier("A server-side error occurred.<br />" + e + "<br />----<br />", false);
        ToggleButtonsPostSubmit();
    }
    else {
        PrelogNotifier(e, false);
        ToggleButtonsPostSubmit();
    }
}

function ChangeButtonStatus(buttonElement, buttonText, isDisabled) {
    if (buttonText != null)
        $(buttonElement).val(buttonText);
    if (isDisabled)
        SetAttribute($(buttonElement), "disabled", "disabled", null, true);
    else
        SetAttribute($(buttonElement), "disabled", null, null, false);
}

/* a rudimentary notifier: uses an element ID'd as 'notification' */
function PrelogNotifier(e, type) {
    if (e == "[object]")
        e = "<strong>Unable to perform transaction. A server-side error has occurred.</strong>";
    if (type == false)
        SetAttribute($("#notification"), null, null, { "color": "red" }, true);
    else
        SetAttribute($("#notification"), null, null, { "color": "#0B610B" }, true);

    $("#notification").html(e);
    $("#notification").fadeIn(300);
    $("#notification").delay(4000).fadeOut(300);
}

function RedirectServiceRequests() {
    var deptCaseNum = GetDBPanelField(DBPanelName, "TV_SUBMTRAN.DEPARTMENT_CASE_NUMBER");
    var deptCode = GetDBPanelField(DBPanelName, "TV_SUBMTRAN.DEPARTMENT_CODE");
    var usesSR = GetDeptCtrl("USES_SERVICE_REQUESTS");
    var usesOptionalSR = GetDeptCtrl("ALLOW_OPTIONAL_SR");
    var hasItems =  PrelogHasItems();

    if (caseStatus == "SAVE_PRELOG") {
        if ($('#grdPRELOG_SUBMISSIONS tr').length > 0) {
            var newRowID = "r" + ($('#grdPRELOG_SUBMISSIONS tr').length - 1);
            $("#hdnRowID").val(newRowID);
        }
    }

    if (usesOptionalSR == "T" && hasItems)
        ConfirmRedirect();
    else
        DecideRedirect(false);

    function DecideRedirect(notify) {
        if (hasItems) {
            if ((usesSR == "T" || usesSR == "W" || usesSR == "S"))
                RedirectToServiceRequests();
            else {
                if (notify)
                    PrelogNotifier("Cannot redirect to SR. Please enable Uses Service Requests.", false);

                LoadCaseWithNoRedirect();
            }
        } else {
            LoadCaseWithNoRedirect();
         
        }
    }

    function LoadCaseWithNoRedirect() {
      window.location = "Tab4PrelogItems.aspx";
    }

    function ConfirmRedirect() {
        $("<div>Do you want to add Service Requests?</div>").appendTo("body").dialog({
            draggable: true, resizable: false, modal: true,
            title: "Service Requests", zIndex: 99990,
            buttons: {
                Yes: function () {
                    DecideRedirect(true);
                    $(this).dialog("close");
                },
                No: function () {
                    LoadCaseWithNoRedirect();
                    $(this).dialog("close");
                }
            },
            close: function () {
                $(this).remove();
            },
            open: function (event, ui) {
                $(".ui-dialog-titlebar-close", ui.dialog | ui).css("display", "none");
                $(":button:contains('Yes')").focus();
            }
        });
    }
}