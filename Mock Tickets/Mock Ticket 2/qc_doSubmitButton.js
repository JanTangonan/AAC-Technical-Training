function DoSubmitQC(btn) {
    var $btn = $(btn);

    if ($btn.is(":disabled"))
        return;

    // Disable button while submit is in progress.
    $btn.attr("disabled", true).val("Wait...");

    StartProgressDialog("Processing Case...");
    //CaptureQCHeaderValuesForRetention();

    var params;

    var fnSubmit = function (bClearBatchKey, batchKey, sig) {
        function SubmitQCSuccess(retdata) {
            ResetSessionTimer();

            // this will load the Enhanced Image Vault in a second tab which allows users to capture images right after the case is created.
            
            if (GetLabCtrl("USES_IMAGE_INTAKE") === "T") {
                window.localStorage.setItem("disable_browser_check", "T"); //prevent the SessionEnd for this function
                if (ImageVaultPopUpWindow != null)
                {
                    try {
                        ImageVaultPopUpWindow.location.reload(false);
                    }
                    catch (ex) {
                        ImageVaultPopUpWindow = window.open("ImageVault.aspx");
                    }
                    }
                else
                    ImageVaultPopUpWindow = window.open("ImageVault.aspx");
                
            }

            CloseProgressDialog();

            if (retdata["result"] == "ok") {
                // Submit was successful, so last submitted batch key is now the 'previous' one.
                _prevBatchKey = batchKey;
                prelogsequence = "";
                _lastAlphaLabItemNum = "";
                $("input[id$='hdnLPNameRec']").val("");
                $("input[id$='hdnNameRefRec']").val("");
                $("[id*=hdnPrelogBarcode]").val("");
                $("[id*=hdnPrelogRedirectInfo]").val("");
                $("[id*=hdnNamesDetails]").val("");
                $("[id*=hdnPrelogLIMSDataJSON]").val("");
                $("[id*=hdnEItemCaseKey]").val("");
                $("[id*=hdnOverWriteLabCase]").val("");
                $("[id*=hdnCheckedFields]").val("");


                setHighVolumeSettings(retdata["submissionkey"]);

                // Delete the previous bulkintake case that was loaded, if any.
                if (_bulkintakeCaseKey != "") {
                    $.ajax({ type: "POST", url: "PLCWebCommon/PLCWebMethods.asmx/DeleteBulkIntakeCase", data: { "casekey": _bulkintakeCaseKey }, success: function () { }, error: function () { }, dataType: "text" });
                }

                SetLastContainer("");
                // Clear current batch key if specified.
                if (bClearBatchKey) {
                    _currentBatchKey = -1;
                    _prevBatchKey = _currentBatchKey;
                }


                if (GetLabCtrl("USES_JIMS_INTERFACE") == "T") {

                    var params = {
                        deptCase: GetDBPanelField("PLCDBPanel1", GetDBPanelFieldReferenceName("PLCDBPanel1", "DEPARTMENT_CASE_NUMBER")),
                        deptCode: GetDBPanelField("PLCDBPanel1", GetDBPanelFieldReferenceName("PLCDBPanel1", "DEPARTMENT_CODE")),
                        labCase: retdata.labcasenumber
                    };

                    $.ajax({
                        type: "POST",
                        url: "PLCWebCommon/PLCWebMethods.asmx/GetProsNo",
                        data: params,
                        success: GetProsNo_Success,
                        error: GetProsNo_Error,
                        dataType: "json"
                    });


                    function GetProsNo_Success(e) {

                        //do nothing
                    }

                    function GetProsNo_Error(e) {
                        alert("Error: " + e);
                    }
                }

                var editstate = GetEditState();

                // If BulkIntake case loaded, remember it so that focus is set to barcode in next page load.
                if (editstate == "BulkIntake")
                    SetCookie("qcsetfocus", "barcode", 1);
                else
                    SetCookie("qcsetfocus", "", 1);

                RMSCaseCustody = "";

                var fnPostSubmit = function () {
                    // Validation and save successful.

                    if (GetLabCtrl("USES_QC_AUTOCOMPLETE") == "T") {
                        _autoComplete.save();
                    }

                    SetEditState("New");
                    ClearAll();
                    //InheritQCHeaderValuesForRetention(); 

                    // Reenable Create button.
                    $btn.attr("disabled", false).val("Create");

                    EnablePage();

                    PresetQuickCreateDBPanel();

                    var $caseProcessedMsg = $("#caseprocessedmsg");
                    var msg = "Lab Case " + (retdata.labcasenumber == "" ? "" : ": " + retdata.labcasenumber) + " was processed.";
                    $caseProcessedMsg.text(msg);
                    FlashStatus($caseProcessedMsg);


                    // If BulkIntake case submitted, set focus to barcode.
                    // If GotoWithReferrer() from above is called, then barcode focus will be set in Init().
                    if (editstate == "BulkIntake")
                        SetFocusToBarcode();

                    if (GetLabCtrl("QC_REDIRECT_TO_NAMES") == "T") {
                        DisableUnloadCheck();
                        window.location = "TAB3Names.aspx";
                    }

                    $("#chkHistCase").prop("checked", false);

                    if (IsLabCtrlSet("SD_QUICK_CREATE")) {
                        ShowPreCaseContainer();
                    } else if (!IsLabCtrlSet("USES_NY_OMNI_INTERFACE")) {
                        var ETIsEnabled = $("[id*=hdnETIsEnabled]").val() == "T";
                        var RMSIsEnabled = $("[id*=hdnRMSIsEnabled]").val() == "T";
                        if (ETIsEnabled && !RMSIsEnabled)
                            InitEvidenceTrackerPrompt();
                        else if (RMSIsEnabled && !ETIsEnabled)
                            InitRMSInterface();
                        else {
                            $("input[id$='hdnProficiencyTestKey']").val("");
                            QCSearchPopUpInit.initialized = true;
                            QCSearchPopUpInit(true);
                            showQCSearchPopUp(true);
                        }
                    }

                    try {
                        if ($("input[id$='hdnSignaturePad']").val() == "TOPAZ") {
                            document.SigPlus1.EncryptionMode = 0;
                            document.SigPlus1.SigCompressionMode = 0;
                        }
                    }
                    catch (ex) { }
                };

                var defaultPageSettings = "menubar=no,location=no,resizable=yes,scrollbars=yes,status=yes,width=250,height=250";
                var basePageSettings = "menubar=no,location=no,resizable=yes,scrollbars=yes,status=yes";

                var isWebScanActive = false;
                var popupPages = [];
                var popupPagesSettings = [];

                // Ex. nextpage = "WebScan.aspx;location=yes,menubar=yes|Page2.aspx;location=no"
                //   Pages are WebScan.aspx, Page2.aspx
                //   Page settings for WebScan.aspx are 'location=yes,menubar=yes'. Page2.aspx settings are 'location=no'.
                function ProcessNextPage() {
                    if (retdata["nextpage"].trim() != "") {
                        var nextpageParams = retdata["nextpage"].split("|");
                        for (var i = 0; i < nextpageParams.length; i++) {
                            var pageGroup = nextpageParams[i].split(";");

                            var nextpage = pageGroup[0];
                            var nextpageSettings = defaultPageSettings;
                            if (pageGroup.length > 1)
                                nextpageSettings = basePageSettings + "," + pageGroup[1];

                            if (nextpage == "printbc") {
                                _plcwebobj = document.getElementById("PLC_WebOCX");
                                if (typeof _plcwebobj.ClientId != "undefined" && _plcwebobj.ClientId(GetLabCtrl("USES_NETBIOS_MAC_ADDRESS"), GetLabCtrl("PLCWebBuildNumber")) == undefined)
                                    _plcwebobj = null;

                                if (_plcwebobj != null)
                                    PrintBarcodeList(_plcwebobj);
                            }
                            else if (nextpage == "webscan") {
                                _plcwebobj = document.getElementById("PLC_WebOCX");
                                if (typeof _plcwebobj.ClientId != "undefined" && _plcwebobj.ClientId(GetLabCtrl("USES_NETBIOS_MAC_ADDRESS"), GetLabCtrl("PLCWebBuildNumber")) == undefined)
                                    _plcwebobj = null;

                                if (_plcwebobj != null) {
                                    DisablePage();
                                    isWebScanActive = true;
                                    DoWebScan(_plcwebobj, fnPostSubmit);
                                }
                            }
                            else if (nextpage == "make2d") {
                                CallPLCWebMethod("PLC_WebOCX", "MAKE2D", { paramList: nextpageParams[i].replace("make2d;", "") }, true, CheckBrowserWindowID);
                            }
                            else if (nextpage != "") {
                                popupPages.push(nextpage);
                                popupPagesSettings.push(nextpageSettings);
                            }
                        }
                    }

                    // Only rest back to 'New Case' if web scan plcweb dialog isn't active. If webscan is active, fnPostSubmit() callback will be called when webscan is done.
                    if (!isWebScanActive)
                        fnPostSubmit();

                    for (var i = 0; i < popupPages.length; i++) {
                        var popupPage = popupPages[i];
                        var popupPageSettings = popupPagesSettings[i];

                        window.open(popupPage, "", popupPageSettings);
                    }
                } // End ProcessNextPage

                var rgxMAKE2D = /make2d;[^\|]+/i;
                if ((rgxMAKE2D).test(retdata["nextpage"])) {
                    var prmMAKE2D = retdata["nextpage"].trim().match(rgxMAKE2D)[0];
                    CallPLCWebMethod("PLC_WebOCX", "MAKE2D", { paramList: prmMAKE2D.replace("make2d;", "") }, true,
                        function () {
                            CheckBrowserWindowID(function () {
                                retdata["nextpage"] = retdata["nextpage"].replace(rgxMAKE2D, "");
                                ProcessNextPage();
                            });
                        }
                    );
                } else {
                    ProcessNextPage();
                }

                //refresh the drafts grid
                if ($("#hdnCurrentTab").val() == "lnkDrafts") {
                    QCSearchPopUpInit.refreshDrafts();
                }
            }
            else {
                // If error code 2: Case already exists, reload form as a second submission.
                if (retdata.errorcode == "2") {
                    SetEditState(retdata.editstate);
                    SetCaseKey(retdata.casekey);
                    SetLastContainer(retdata.lastcontainer);
                    LoadNamesGrid(retdata.names);
                    CreateExistingItemsGrid(retdata.existingitems);
                    CreateExistingNamesGrid(retdata.existingnames);
                    LoadDistributionsGrid(retdata.distributions);
                }

                // If validation error returned a new submission title to set, set it.
                if (retdata.submissiontitle != "")
                    SetSubmissionTitle(retdata.submissiontitle);

                // Reenable Create button.
                $btn.attr("disabled", false).val("Create");

                // Validation error.
                $(document).scrollTop(0);
                ShowPopup(retdata["msg"]);
            }

        }

        function SubmitQCError(xhr, errortxt, e) {
            CloseProgressDialog();

            $(document).scrollTop(0);
            ShowError("Save error.", xhr, errortxt, e);

            // Reenable Create button.
            $btn.attr("disabled", false).val("Create");
            $("#chkHistCase").prop("checked", false);
            RMSCaseCustody = "";
        }

        StartProgressDialog("Processing Case...");

        // Submit QC ajax call.
        params.batchseq = batchKey.toString();     // Pass current batch sequence number returned from either GetNextBatchSeq ajax call or previous high vol submitter batch.
        params.receivedBy = GetDBPanelField("plcQCRecByPanel", "TV_LABSUB.RECEIVED_BY");
        params.receivedDate = GetDBPanelField("plcQCRecByPanel", "TV_LABSUB.RECEIVED_DATE");
        params.receivedTime = GetDBPanelField("plcQCRecByPanel", "TV_LABSUB.RECEIVED_TIME");
        params.qcAttachmentKeys = $("input[id$='hdnQCAttachmentKeys']").val();
        params.xmlKey = $("input[id$='hdnXMLDraftKey']").val();
        params.receiptLabel = $("[id*=ddlLabelFormat]").val();
        params["auxdata"] = $("[id*=hdnAuxillaryDataJSON]").val(); // there may be changes to auxdata because of high volume submission
        params["preloglimsdata"] = $("[id*=hdnPrelogLIMSDataJSON]").val();
        params.skipSignatureReason = (sig && sig.skipped && sig.reason) || "";
        $.ajax({ type: "POST", url: "PLCWebCommon/PLCWebMethods.asmx/SubmitQC", data: params, success: SubmitQCSuccess, error: SubmitQCError, dataType: "json" });
    };

    var fnPreSubmit = function (batchKey, isHighVolDept, isSigRequired) {
        if (isHighVolDept) {
            $("input[id$='hdnHighVolumeSubmissionFlag']").val('T');
            $("input[id$='hdnHighVolumeBatchKey']").val(batchKey);

            //Need to call this in case there is a change with high volume submission.
            ConsolidateAuxillaryData();
        }
        else if ($("input[id$='hdnHighVolumeSubmissionFlag']").val() == 'T') {
            $("input[id$='hdnHighVolumeBatchKey']").val(batchKey);

        }

        var isHighVolume = $("input[id$='hdnHighVolumeSubmissionFlag']").val();
        var bClearBatchKey = true;
        var showSig = isSigRequired;

        var $subType = GetDBPanelElement("PLCDBPanel1", "TV_LABSUB.SUBMISSION_TYPE");

        // If BulkIntake case submit, don't clear batch key in case there were previous high volume submitter cases.
        if (GetEditState() == "BulkIntake") {
            showSig = false;
            bClearBatchKey = false;
            proceedToSubmit();
        }
        else if ((batchKey != -1) && isHighVolume == 'T') {
            // If entering more cases later, don't show signature pad.
            ShowYesNoPopup("Are there more cases for this submitter?", {
                fnYes: function () {
                    showSig = false;
                    bClearBatchKey = false;
                    doHighVolSubmission();
                },
                fnNo: function () {
                    $("input[id$='hdnHighVolumeSubmissionFlag']").val("");
                    //Need to call this in case there is a change with high volume submission.
                    ConsolidateAuxillaryData();
                    if (!showSig)
                        endHighVolSubmission();
                    proceedToSubmit();
                }
            });
        }
        //will just make a new else if case for less complicated coding...
        //else if ((GetClientField(_miscClientIDs.cbprintreceipt) == "true" || GetClientField(_miscClientIDs.cbprintreceipt)) && isHighVolume == 'T' && batchKey == -1) {
        else if (isHighVolume == 'T' && batchKey == -1) {
            ShowYesNoPopup("Are there more cases for this submitter?", {
                fnYes: function () {
                    showSig = false;
                    bClearBatchKey = false;
                    doHighVolSubmission();
                },
                fnNo: function () {
                    $("input[id$='hdnHighVolumeSubmissionFlag']").val("");
                    //Need to call this in case there is a change with high volume submission.
                    ConsolidateAuxillaryData();
                    if (!showSig)
                        endHighVolSubmission();
                    proceedToSubmit();
                }
            });
        } else {
            proceedToSubmit();
        }

        function proceedToSubmit() {
            var preSubmitCB = function () {
                if (showSig) {
                    // Show signature pad and submit case after signature is entered or cancelled.
                    ResetSignaturePad();
                    SetSigCallbacks(
                        function (sigData) {
                            sigData = sigData || {};
                            checkHighVolumeSignature(sigData, function () {
                                endHighVolSubmission();
                                fnSubmit(bClearBatchKey, batchKey, sigData);
                            });
                        },        // Sig save handler
                        function () { _currentBatchKey = _prevBatchKey; $btn.attr("disabled", false).val("Create"); } // Sig cancel handler
                    );
                    SetSigBatchKey(batchKey.toString());
                    setAuditSkipSignature(false); // we will handle the audit log so we can set plcsession variables
                    ShowSigPad();
                }
                else {
                    // No signature pad so proceed directly to submit case.
                    fnSubmit(bClearBatchKey, batchKey);
                }
            };

            var deptCaseNumFieldName = GetDBPanelFieldReferenceName("PLCDBPanel1", "DEPARTMENT_CASE_NUMBER");
            var departmentCaseNumber = GetDBPanelField("PLCDBPanel1", deptCaseNumFieldName);
            var caseKey = GetCaseKey();

            // do a presubmit to set PLCSession variables for audit logs
            $.ajax({
                type: "POST",
                url: "PLCWebCommon/PLCWebMethods.asmx/PreSubmitQC",
                data: {
                    caseKey: caseKey,
                    departmentCaseNumber: departmentCaseNumber
                },
                success: function () { preSubmitCB(); },
                error: function () { preSubmitCB(); },
                dataType: "text"
            });
        }

        var panelName = "PLCDBPanel1";
        var fieldName = "TV_LABSUB.SUBMISSION_TYPE";
        var dbpanelFields = GetDBPanelFields(panelName);
        var subTypeField = dbpanelFields[fieldName];
        function doHighVolSubmission() {
            if (subTypeField && typeof subTypeField._defaultvalue === "undefined") {
                subTypeField._defaultvalue = GetClientField(_miscClientIDs.defaultsubmissiontype);
                SetClientField(_miscClientIDs.defaultsubmissiontype, GetDBPanelField("PLCDBPanel1", "TV_LABSUB.SUBMISSION_TYPE"));
            }
            $subType.disable();
            $("[id$='btnSignHighVolumeSubmissions']").attr("no-sig", !isSigRequired ? "T" : "F");
            var highVolumeFields = GetLabCtrl("HIGH_VOLUME_SUBMITTER_FIELDS").replace(/\s/g, "").split(",");
            if (highVolumeFields.length)
                storeJSONDBPanelValues(panelName, highVolumeFields);
            proceedToSubmit();
        }

        function endHighVolSubmission() {
            if (subTypeField && typeof subTypeField._defaultvalue !== "undefined") {
                SetClientField(_miscClientIDs.defaultsubmissiontype, subTypeField._defaultvalue);
                delete subTypeField._defaultvalue;
            }
            $subType.enable();

            clearJSONDBPanelValues("PLCDBPanel1");
        }
        window.endHighVolSubmission = endHighVolSubmission;
    }


    var fnPreSubmitNameItemCheck = function () {

        $('#mdialog-nameitem-check').dialog({
            autoOpen: true,
            modal: true,
            resizable: false,
            closeOnEscape: false,
            dialogClass: "no-close",
            title: "Name Item Check",
            width: 'auto',
            buttons: {
                "Create Anyway": function () {
                    nameItemCheckCreateCase();
                },
                "Cancel": function () {
                    nameItemCheckCancel();
                }
            },
            open: function () {

                var $dialog = $(this).parent();

                $dialog.appendTo("form");
                $(".ui-button.ui-state-focus", $dialog).focus();
 
            },
            beforeClose: function () {
                $(this).dialog('destroy');
            }
        });
    };

    function nameItemCheckCreateCase() {
        $("#mdialog-nameitem-check").dialog("close");
        if (GetLabCtrl("NEXT_CASE_FROM_ANOTHER_DB") == "T" && GetEditState() == "New")
            fnPreSubmitNextCaseValidation();        
        else
            GetBatchSequence(highVolDept, sigRequired);

    }

    function nameItemCheckCancel() {
        $("#mdialog-nameitem-check").dialog("close");

        var $bnCreate = $("input#AjaxCreate");
        if ($bnCreate)
            $bnCreate.removeAttr("disabled");

    }

    function GetBatchSequence(isHighVolDept, isSigRequired) {

        if (_currentBatchKey == -1) {
            // Get next batch sequence and pass to presubmit (sigpad + submit).
            $.ajax({
                type: "POST", url: "PLCWebCommon/PLCWebMethods.asmx/GetNextBatchSeq", data: {},
                success: function (retdata) {
                    _prevBatchKey = _currentBatchKey;
                    _currentBatchKey = retdata.batchseq;
                    fnPreSubmit(_currentBatchKey, isHighVolDept, isSigRequired);
                },
                error: function () { fnPreSubmit(-1, isHighVolDept, isSigRequired); },
                dataType: "json"
            });
        }
        else {
            // If pending batch seq exists
            fnPreSubmit(_currentBatchKey, isHighVolDept, isSigRequired);
        }      
    }


    var fnPreSubmitNextCaseValidation = function () {

        var caseDepartmentCode = GetDBPanelField("PLCDBPanel1", "TV_LABSUB.CASE_DEPARTMENT_CODE");
         
        var param = { "casedept": caseDepartmentCode, "prelogseq": prelogsequence};

        $.ajax({
            type: "POST",
            async: false,
            url: "PLCWebCommon/PLCWebMethods.asmx/ValidateNextCaseInfo",
            data: param,
            success: function (data) {
                if (data == "A") { //for alert
                    ShowPopup("You do not have correct setup to pull the next case number. Please call Porter Lee Corp.");

                    var $bnCreate = $("input#AjaxCreate");
                    if ($bnCreate)
                        $bnCreate.removeAttr("disabled").val("Create");

                    return;
                }
                if (data == "C") { //for confirmation

                    $("#div-nextcase-message").html("This agency is flagged to pull a case number from an external source.<br/> Would you like the Department Case Number used as the LIMS Case #?");

                    $('#mdialog-nextcase').dialog({
                        autoOpen: true,
                        modal: true,
                        resizable: false,
                        closeOnEscape: false,
                        dialogClass: "no-close",
                        title: "Next Case",
                        width: 'auto',
                        buttons: {
                            "Yes": function () {
                                params["nextcasefromdeptcase"] = "T";

                                // close popup first
                                $(this).dialog("close");

                                GetBatchSequence(highVolDept, sigRequired);
                            },
                            "No": function () {
                                params["nextcasefromdeptcase"] = "F";

                                // close popup first
                                $(this).dialog("close");

                                GetBatchSequence(highVolDept, sigRequired);
                            }
                        },
                        open: function () {

                            var $dialog = $(this).parent();

                            $dialog.appendTo("form");
                            $(".ui-button.ui-state-focus", $dialog).focus();

                        },
                        beforeClose: function () {
                            $(this).dialog('destroy');
                        }
                    });
                }
                else
                    GetBatchSequence(highVolDept, sigRequired);
            },
            error: function () {
                ShowPopup("Error checking case department code next case");
            },
            dataType: "text"
        });
          

              
    };
        
    var fnValidate = function () {
        function ValidateQCSuccess(retdata) {
            ResetSessionTimer();

            CloseProgressDialog();

            // Validation successful.
            if (retdata["result"] == "ok") {

                if (retdata.ishighvoldept == "true")
                    highVolDept = true;
                else
                    highVolDept = false;

              
                if (retdata.issigrequired == "true")
                    sigRequired = true;
                else
                    sigRequired = false;

                params["separatechild"] = "";

                if ((retdata["hasNames"] === "false" || retdata["hasValidItems"] === "false")
                    && GetLabCtrl("USES_NAME_CHECKER") === "T") {
                    //show the popup here
                    var message = "";
                    if (retdata["hasNames"] === "false")
                        message = "No name(s) entered/selected. <br/>";
                    if (retdata["hasValidItems"] === "false")
                        message += "No item(s) entered/selected. <br/>";

                    $("#div-nameitem-message").html(message);

                    fnPreSubmitNameItemCheck();
                }
                else if (GetLabCtrl("NEXT_CASE_FROM_ANOTHER_DB") == "T" &&  GetEditState() == "New")
                {
                    fnPreSubmitNextCaseValidation();
                }
                else if (retdata.childItems && retdata.childItems != "") {

                    var message = " The following subitems will be separated from its parent: " + retdata.childItems + "<br/>"
                        + " Click Yes to continue, No to keep the child item with its parent, or Cancel to cancel transaction";

                    jqConfirmYesNoCancel(message,
                        function yesFn() {
                            params["separatechild"] = "T";
                            GetBatchSequence(highVolDept, sigRequired);
                        }, function noFn() {
                            params["separatechild"] = "";
                            GetBatchSequence(highVolDept, sigRequired);
                        }, function cancelFn() {

                            // Reenable Create button.
                            $btn.attr("disabled", false).val("Create");

                            return false;
                        }, "#divYesNoCancel");
                }
                else {
                    GetBatchSequence(highVolDept, sigRequired);              
                }
            }
            else {
                // If error code 2: Case already exists, reload form as a second submission.
                if (retdata.errorcode == "2") {
                    SetEditState(retdata.editstate);
                    SetCaseKey(retdata.casekey);
                    SetLastContainer(retdata.lastcontainer);
                    LoadNamesGrid(retdata.names);
                    CreateExistingItemsGrid(retdata.existingitems);
                    CreateExistingNamesGrid(retdata.existingnames);
                    LoadDistributionsGrid(retdata.distributions);
                }

                // If validation error returned a new submission title to set, set it.
                if (retdata.submissiontitle != "")
                    SetSubmissionTitle(retdata.submissiontitle);

                // Reenable Create button.
                $btn.attr("disabled", false).val("Create");

                // Validation error.
                $(document).scrollTop(0);
                ShowPopup(retdata["msg"]);
            }
        }

        function ValidateQCError(xhr, errortxt, e) {
            CloseProgressDialog();

            $(document).scrollTop(0);
            ShowError("Validation Error.", xhr, errortxt, e);

            // Reenable Create button.
            $btn.attr("disabled", false).val("Create");
        }

        StartProgressDialog("Validating Case...");

        // ValidateSubmitQC ajax call.
        $.ajax({ type: "POST", url: "PLCWebCommon/PLCWebMethods.asmx/ValidateSubmitQC", data: params, success: ValidateQCSuccess, error: ValidateQCError, dataType: "json" });
    };

    GetSubmitQCParamsAsync(function (paramsSet) {
        ConsolidateAuxillaryData();
        params = paramsSet;
        params["prelogseq"] = prelogsequence;
        params["auxdata"] = $("[id*=hdnAuxillaryDataJSON]").val();
        params["rmscasecustody"] = RMSCaseCustody;
        params["redirectnameslist"] = $("[id*=hdnNamesDetails]").val();
        params["overwritelabcase"] = $("[id*=hdnOverWriteLabCase]").val(); // use this on submit of QC case
        // Validate submission. If submit is valid, it will call signature pad and submit.
        fnValidate();
    });
}