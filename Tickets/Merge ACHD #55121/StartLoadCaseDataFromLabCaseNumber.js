function StartLoadCaseDataFromLabCaseNumber(labcasenumber, fnPostCB) {
    if (labcasenumber == "")
        return;

    StartProgressDialog("Searching...");

    UpdateCaseSubmissionText();
    var jsonCaseSubmission = $("input[id$='hdnCaseSubmissionJSON']").val();

    var params = {
        "labcasenumber":labcasenumber,
        "jsoncasesubmission":jsonCaseSubmission,
        "editstate":GetEditState()
    };

    function LoadSuccess(retdata) {
        ResetSessionTimer();

        if (retdata.result == "ok") {
            // Case found.
            ClearAll();
            SetEditState(retdata.editstate);
            ToggleGridVisibility();
            SetSubmissionTitle(retdata.casetitle);
            SetCaseKey(retdata.casekey);
            SetLastContainer(retdata.lastcontainer);
            disableFlexboxShowActiveOnly([
                { source: "PLCDBPanel1", elements: getDBPanelFlexbox("PLCDBPanel1", retdata.casesubmission, "DEPTNAME") },
                { source: "DISTRIBUTIONS", elements: getDistributionsFlexbox(retdata.distributions) }
            ]);
            getLastVisitText(retdata.casekey);

            //lims > new sub
            if (!isPrelog() && (!IsETRMSPrelog) && !isPrelogLIMSSearch()) {
                if (IsLabCtrlSet("QC_OFFENSE_ADDRESS_BOOK"))
                    updateOffenseAddressIM();
            }

            LoadPLCDBPanel("PLCDBPanel1", retdata.casesubmission);
            LoadNamesGrid(retdata.names);
            LoadItemsGrid(retdata.items);

            if (GetLabCtrl("USES_QC_LAB_REF") == "T") {
                if (retdata.references) {
                    RecreateReferencesGrid(retdata.references.length);
                    LoadReferencesGrid(retdata.references);
                }
            }

            CreateExistingItemsGrid(retdata.existingitems, true);
            CreateExistingNamesGrid(retdata.existingnames);
            LoadDistributionsGrid(retdata.distributions);
            
            RepositionQCGridHeaders();
            $("#txtStmtOfFacts").val(retdata.stmtOfFacts);
            $("#txtFileComments").val(retdata.fileComments);

            // Always reset to default labcode and submission type.
            ResetDefaultLabCode();
            ResetDefaultSubmissionType();

            TriggerMandatoryBaseEvents();

            CloseProgressDialog();

            SetQCByUserPrefs(true);

            loadJSONDBPanelValues("PLCDBPanel1", { clear: true });

            bindChangeEventToQCOverWriteFields(retdata.casesubmission);

            if (GetLabCtrl("QC_PREVIEW_LAB_CASE_NUMBER") == "T")
                $("[id$='pnlLabCaseNum']").css("display", "none");
            //ShowPopup("Case loaded.");
        }
        else if (retdata.result == "none") {
            ShowPopup("No case found.");

            CloseProgressDialog();
        }

        if (IsDefined(fnPostCB))
            fnPostCB();
    }
    
    function LoadError(xhr, errortxt, e) {
        CloseProgressDialog();
        ShowError("Case Load Error", xhr, errortxt, e);
    }

    $.ajax({type:"POST", url:"PLCWebCommon/PLCWebMethods.asmx/LoadCaseDataFromLabCaseNumber", data:params, success:LoadSuccess, error:LoadError, dataType:"json"});
}