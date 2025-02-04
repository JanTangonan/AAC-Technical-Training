function GetPrelogTran(params, isNewCaseLIMSSearch, isCrimeSceneCase) {
    params["isNewCaseLIMSSearch"] = (isNewCaseLIMSSearch ? "T" : "F");
    params["kitsearch"] = (isSearchBox("kit") || $("[id*=hdnFromKitSearch]").val() == "T") ? "T" : "F";
    $.ajax({
        type: "POST",
        async:false,
        url: "PLCWebCommon/PLCWebMethods.asmx/GetPrelogTran",
        data: params,
        success: function (data) {
            var labcasenumber = 0;
            var caseKey = 0;
            var nextSubNumber = "";
            var newLIMSSubmission = false;
            var hasSR = false;
            var hasDraftSR = false;
            var receivedByLab = false;     
            var deptName = "";
            var overWriteFields = "";
            var labcasePriority = "";


            for (var key in data) {
                if (data[key].Key == "$LIMS_CASE_STATUS" && data[key].Value == "T")
                    newLIMSSubmission = true;
                if (data[key].Key == "COMMENTS")
                    $("textarea[id$='edRemarks']").val(data[key].Value);
                if (data[key].Key == "PRELOG_SEQUENCE")
                    prelogsequence = data[key].Value;
                if (data[key].Key == "LAB_CASE_NUMBER")
                    labcasenumber = data[key].Value;
                if (data[key].Key == "NEXT_SUBMISSION_NUMBER")
                    nextSubNumber = data[key].Value;
                if (data[key].Key == "CASE_KEY")
                    caseKey = data[key].Value;
                if (data[key].Key == "LPNAMEREC")
                    $("input[id$='hdnLPNameRec']").val(data[key].Value);
                if (data[key].Key == "NAMEREFREC")
                    $("input[id$='hdnNameRefRec']").val(data[key].Value);
                if (data[key].Key == "SREXISTS")
                    hasSR = data[key].Value == "T";
                if (data[key].Key == "HASDRAFTSR")
                    hasDraftSR = data[key].Value == "T";
                if (data[key].Key == "QC_ATTACHMENT_KEYS") {
                    $("input[id$='hdnQCAttachmentKeys']").val(data[key].Value);
                    UpdateAttachButtonFontColor();
                }
                if (data[key].Key == "LASTCONTAINER")
                    SetLastContainer(data[key].Value);
                if (data[key].Key == "RECEIVED_BY_LAB")
                    receivedByLab = data[key].Value == "T";
                if (data[key].Key == "DEPT_NAME")
                    deptName = data[key].Value;

                if (data[key].Key == "QC_OVERWRITEFIELDS") {
                    overWriteFields = $.parseJSON(data[key].Value);
                }

                if (data[key].Key == "DEFAULT_PRIORITY")
                    labcasePriority = data[key].Value;

            }

            if (!hasSR) {
                ShowPopup("Prelog submission cannot be accepted. At least 1 Service Request is required.");
            }
            else if (hasDraftSR) {
                ShowPopup("Prelog submission cannot be accepted. One or more Service Requests is/are still in draft mode.");
            }
            else if ($("input[id$=hdnEnableLIMS]").val() == "T" && receivedByLab) {
                ShowPopup("This submission has already been taken in.");
            }
            else {
                ClearDBPanelFields("PLCDBPanel1");
                var $fields = _plcdbpanellist["PLCDBPanel1"].fields;
                var submittingOfficerKey = "";

                var fnPost = function () {

                    for (var key in data) {

                        if ("TV_LABCASE." + data[key].Key in $fields) {
                            var defaultvalue = $fields["TV_LABCASE." + data[key].Key].defaultvalue;

                            if (defaultvalue != "" && data[key].Value == "")
                                SetDBPanelField("PLCDBPanel1", "TV_LABCASE." + data[key].Key, defaultvalue);
                            else 
                                SetDBPanelField("PLCDBPanel1", "TV_LABCASE." + data[key].Key, data[key].Value);                          
                        }
                        if ("TV_LABSUB." + data[key].Key in $fields) {
                            if (data[key].Key != "CASE_DEPARTMENT_CODE" && data[key].Key != "DEPARTMENT_CODE") {
                                var defaultvalue = $fields["TV_LABSUB." + data[key].Key].defaultvalue;

                                if (defaultvalue != "" && data[key].Value == "")
                                    SetDBPanelField("PLCDBPanel1", "TV_LABSUB." + data[key].Key, defaultvalue);
                                else
                                    SetDBPanelField("PLCDBPanel1", "TV_LABSUB." + data[key].Key, data[key].Value);
                            }
                        }

                        if (data[key].Key == "PRIORITY" && data[key].Value != "") {
                            SetDBPanelField("PLCDBPanel1", "TV_LABASSIGN.PRIORITY", data[key].Value);
                        }

                        if (data[key].Key == "SUBMISSION_TYPE_CODE") {

                            var subType = ($("input[id$='hdnHighVolumeSubmissionFlag']").val() == 'T') ? GetDefaultSubmissionType() : data[key].Value;
                            SetDBPanelField("PLCDBPanel1", "TV_LABSUB.SUBMISSION_TYPE", subType);
                        }

                        if (data[key].Key == "SUBMISSION_NUMBER" && (params.isPDF417 || params.isCrimeSceneImport))
                            SetDBPanelField("PLCDBPanel1", "TV_LABSUB.DEPARTMENT_SUBMISSION", data[key].Value);

                        if (data[key].Key == "SUBMITTING_OFFICER_KEY") {
                            submittingOfficerKey = data[key].Value;
                        }

                        if (data[key].Key == "DEPARTMENT_CODE") {

                            var caseDeptCode = GetDBPanelClientID("PLCDBPanel1", "TV_LABSUB.CASE_DEPARTMENT_CODE");
                            if (caseDeptCode != null) {
                                SetDBPanelField("PLCDBPanel1", "TV_LABSUB.CASE_DEPARTMENT_CODE", data[key].Value);
                            }

                            // Default Submission Department to the Case Department Code...
                            var defaultSubDeptCode = GetDBPanelClientID("PLCDBPanel1", "TV_LABSUB.DEPARTMENT_CODE");
                            if (defaultSubDeptCode != null) {
                                SetDBPanelField("PLCDBPanel1", "TV_LABSUB.DEPARTMENT_CODE", data[key].Value);
                            }


                        }


                        if (data[key].Key == "CASE_OFFICER_PHONE") {
                            SetDBPanelField("PLCDBPanel1", "TV_LABCASE.CONTACT_INFO", data[key].Value);
                        }



                        if (data[key].Key == "CASE_DEPARTMENT_CODE") {

                            var submissionDeptCode = GetDBPanelClientID("PLCDBPanel1", "TV_LABSUB.DEPARTMENT_CODE");
                            //and dont blank it out
                            if((submissionDeptCode != null) && (data[key].Value != "")) {
                                SetDBPanelField("PLCDBPanel1", "TV_LABSUB.DEPARTMENT_CODE", data[key].Value);
                            }



                        }
                    }

                    if (labcasePriority != "") {
                        SetDBPanelField("PLCDBPanel1", "TV_LABASSIGN.PRIORITY", labcasePriority);                        
                    }

                    //Assign duplicate values based on TV_DBPANEL.DUPLICATE_VALUES on load
                    assignDuplicateValues();

                    if (isCrimeSceneImport || isPDF417 || (GetLabCtrl("USES_CONTAINER_SPLITTER") == "T")) {                      
                           ShowTab('divItems', $("a[id$='bnItems']"));
                    }

                    if (submittingOfficerKey != "")
                        SetDBPanelField("PLCDBPanel1", "TV_LABSUB.SUBMITTED_OFFICER_KEY", submittingOfficerKey);

                    setDefaultPriority();


                    var $labCode = GetDBPanelFieldReferenceName("PLCDBPanel1", "LAB_CODE");                 
                    var defaultLabCode = $("input[id$=hdnDefaultLabCode]").val();


                    var initDefaultLabCode = GetDBPanelField("PLCDBPanel1", $labCode);
                    if (initDefaultLabCode != null && initDefaultLabCode == "") {
                        if (defaultLabCode) SetDBPanelField("PLCDBPanel1", $labCode, defaultLabCode);
                    }

                    var $accessRes = GetDBPanelFieldReferenceName("PLCDBPanel1", "ACCESS_RES");
                    var defaultAccessRes = $("input[id$=hdnAccessRes]").val();

                    var initDefaultAccessRes = GetDBPanelField("PLCDBPanel1", $accessRes);
                    if (initDefaultAccessRes != null) {
                        if (defaultAccessRes) SetDBPanelField("PLCDBPanel1", $accessRes, defaultAccessRes);
                    }
                 
                    if (isCrimeSceneCase !== "T") {
                        GetPrelogItems(params, function () {
                            GetPrelogNames(params)
                        });

                        if (!isNewCaseLIMSSearch) {
                            GetPrelogItemsForResubmission(params);
                            GetPrelogNamesForResubmission(params);
                        }
                        GetLIMSDistribution(params);
                        SetPrintReceiptCBValue();
                        SetAttachImageCBValue();
                        hideQCSearchPopUp();
                        GetPrelogExtra(params);
                        TriggerMandatoryBaseEvents();
                        checkRequiredSigPadStatus();
                        SetQCByUserPrefs(true);

                        loadJSONDBPanelValues("PLCDBPanel1", { clear: true });
                    }

                    setPrelogDefaultLabCode();

                    if (overWriteFields)
                        unbindOverwriteFieldEvent($fields, overWriteFields);

                    //overwrite current value with default value for CURRENTDATE and CURRENTTIME
                    setCurrentDateAndTimeDefaultValue("PLCDBPanel1");

                }

                if (isNewCaseLIMSSearch)
                    newLIMSSubmission = false;

                if (newLIMSSubmission) {
                    SetEditState("Edit");
                    var additionalCaseDetails = params["departmentCaseNumber"] + " / " + deptName + " / ";
                    SetSubmissionTitle(additionalCaseDetails + labcasenumber + " Submission Number " + nextSubNumber);
                    SetCaseKey(caseKey);

                    $("[id*=hdnOverWriteLabCase]").val("");

                    var message = getDBPanelOverwritePrompt(overWriteFields, $fields);
                    if (message != "") {
                        confirm(message, function () {
                            $("[id*=hdnOverWriteLabCase]").val("T");
                            fnPost();
                        }, function () {
                            $("[id*=hdnOverWriteLabCase]").val("F");
                            fnPost();
                        });

                    }
                    else
                        fnPost();
                }
                else {
                    SetEditState("New");
                    fnPost();
                }
            }

            disableDepartmentCaseField();
        },
        error: function () {
            alert("failed");
        },
        dataType: "json"
    });
}