function btnNewSubmission_Click() {
    setDBPanelFieldFocus("PLCDBPanel1", "TV_LABCASE.DEPARTMENT_CODE");
    $("input#bnRedirect").css("display", "none");    

    $("[id*=hdnCheckedFields]").val("");

    if (refIDs != "") {         
        if (!isPrelog() && (!IsETRMSPrelog) && !isPrelogLIMSSearch()) {
            // the third chunk should contain the LAB_CASE for LIMS cases
          
            StartLoadCaseDataFromLabCaseNumber(refIDs.split(",")[3], function () { setDefaultPriority(); });           
            hideQCSearchPopUp();
        }
        else if (isPrelogLIMSSearch()) {
       
            if (GetLabCtrl("PRELOG_MULTIPLE_LIMS_CASES") == "T") {

                var casekeyList = $("[id*=hdnEItemCaseKey]").val();
                var limsCaseKey = refIDs.split(",")[2].toString();
                var containsKey = true;
                if (casekeyList && casekeyList.length) {
                    var list = casekeyList.split(",");
                    containsKey = ($.inArray(limsCaseKey, list) !== -1);
                }

                if (!containsKey) {                   
                    Alert("Selected / Scanned prelog case contains an existing item that does not exist in this case. ");
                    return;
                }

            }
            
            jqConfirm("This will import the names and items of the prelog case to the selected case. Are you sure you want to continue?",
                function okFn() {
                    

                    if (prelogLIMSRefIDs["changedprelogvalues"]) {
                        var changedFields = prelogLIMSRefIDs["changedprelogvalues"];
                        var message = "The following Case information has changed in this submission. Would you like to accept the changes? ";

                        $("#divConfirmContent").html(message + changedFields);
                        $('#divConfirmChanged').dialog({
                            autoOpen: true,
                            modal: true,
                            resizable: false,
                            closeOnEscape: false,
                            dialogClass: "no-close",
                            title: "Prelog Import",
                            width: 550,
                            buttons: {
                                "Ok": function () {
                                    var checkedFields = getConfirmedFieldsToLoadValue();
                                    GetPrelogLIMSTran(refIDs, prelogLIMSRefIDs, checkedFields);     
                                },
                                "Cancel": function () {
                                    GetPrelogLIMSTran(refIDs, prelogLIMSRefIDs, "");
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
                        GetPrelogLIMSTran(refIDs, prelogLIMSRefIDs);                      
                }, function cancelFn() {
                    return false;
                }, "#divPrelogLimsConfirm");
            
        }
        else {

            if (IsLabCtrlSet("USES_REDIRECT_ITEMS")) {
                $("input#bnRedirect").css("display", "inline");
            }
           

            var fnData = function () {
                var params = {
                    "departmentCode": refIDs.split(",")[0],
                    "departmentCaseNumber": refIDs.split(",")[1],
                    "submissionNumber": refIDs.split(",")[2],
                    "prelogSequence": refIDs.split(",")[3]
                };

                GetPrelogTran(params);

                $("input[id$='btnSaveDraft']").css('display', 'none');
            }

            fnData();
        }

        setPictureMask(false, "PLCDBPanel1", refIDs.split(",")[0]);

        if (ETIsEnabled)		   
            UnloadEvidenceTrackerPopup();		               
        else if (RMSIsEnabled)		                
            UnloadRMSPopup(true);      
    }
    else if (isPrelogLIMSSearch() && prelogLIMSRefIDs != "" && $(".btnNewSubmission").val() == "Open Prelog") {
        if (prelogLIMSRefIDs["caseInLIMS"] == "T")
            SetPrelogCaseData(prelogLIMSRefIDs["labCase"]);

        setPictureMaskFromData(prelogLIMSRefIDs, "PLCDBPanel1");
        GetPrelogTran(prelogLIMSRefIDs);
    } 
  
    if (GetClientField(_miscClientIDs.chkhighvolume) == "true" || GetClientField(_miscClientIDs.chkhighvolume)) {
        $("input[id$='hdnHighVolumeSubmissionFlag']").val('T');
    }

    // signifies that it is a new submission during draft saving
    $('#hdnIsNewCaseDraft').val('F');
    
    return false;
}