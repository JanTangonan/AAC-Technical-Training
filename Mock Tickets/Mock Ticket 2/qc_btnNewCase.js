function btnNewCase_Click() {  
    if (GetLabCtrl("QC_PREVIEW_LAB_CASE_NUMBER") == "T")
        $("[id$='pnlLabCaseNum']").css("display", "none");

    if (!isPrelog() && !isPrelogLIMSSearch()) {

        $("input#bnRedirect").css("display", "none");

        var dictName = { "NAMEKEY": "", "DEPTNAMEKEY": "", "ARRESTNUMBER": "", "NAMETYPE": "", "LAST": "", "FIRST": "", "MIDDLE": "", "SEX": "", "RACE": "", "DOB": "", "AGE": "", "ARRESTDATE": "", "STATEID": "" };
        dictName.LAST = $(".txtLastName").val();
        dictName.FIRST = $(".txtFirstName").val();
        var name = new Array(1);
        name[0] = dictName;
        LoadNamesGrid(name, false);

        //why is the Names Tab reloaded on click of New Case??? thus, we have to reattach the popup controls
        dbpanelAttachPopupToParent("div[id$='divNames']", "#tblNames");

        var $deptCode = GetDBPanelFieldReferenceName("PLCDBPanel1", "DEPARTMENT_CODE");
        var $deptCaseNumber = GetDBPanelFieldReferenceName("PLCDBPanel1", "DEPARTMENT_CASE_NUMBER");
        var $labCode = GetDBPanelFieldReferenceName("PLCDBPanel1", "LAB_CODE");
        var $caseDeptCode = GetDBPanelFieldReferenceName("PLCDBPanel1", "CASE_DEPARTMENT_CODE");
      
        var deptCode = $("[id$='fbDepartmentCode']").getCode();
        var deptCaseNumber = $("[id$='txtDepartmentCaseNumber']").val();
        var labCase = "";
        var labCaseLabel = GetLabCtrl("LAB_CASE_TEXT") == "" ? "Lab Case Number" : GetLabCtrl("LAB_CASE_TEXT");
        var defaultLabCode = $("input[id$=hdnDefaultLabCode]").val();
        var CASESEARCHPOPUPMode = $("[id*=searchBox2]").css("display") == "block";

        if (CASESEARCHPOPUPMode) {
            var deptCode_customPanel = GetDBPanelFieldReferenceName("dbpCaseSearch", "DEPARTMENT_CODE");
            var deptCaseNumber_customPanel = GetDBPanelFieldReferenceName("dbpCaseSearch", "DEPARTMENT_CASE_NUMBER");
            var labCase_customPanel = GetDBPanelFieldReferenceName("dbpCaseSearch", "LAB_CASE");

            labCase = (labCase_customPanel != null ? GetDBPanelField("dbpCaseSearch", labCase_customPanel) : "");
            if (deptCode_customPanel != null && deptCaseNumber_customPanel != null) {
                deptCode = GetDBPanelField("dbpCaseSearch", deptCode_customPanel);
                deptCaseNumber = GetDBPanelField("dbpCaseSearch", deptCaseNumber_customPanel);
            }

            if (IsLabCtrlSet("USES_WILD_CARD_SEARCH")) {
                if (labCase.indexOf("*") > -1 || labCase.indexOf("?") > -1) labCase = "";
                if (deptCode.indexOf("*") > -1 || deptCode.indexOf("?") > -1) deptCode = "";
                if (deptCaseNumber.indexOf("*") > -1 || deptCaseNumber.indexOf("?") > -1) deptCaseNumber = "";
            }
        }

        if ($("#chkHistCase").is(":checked") && labCase == "") {
            ShowPopup("You need the " + labCaseLabel + " for historical cases.");
            return;
        }

        if ((deptCaseNumber != "" || labCase != "") && GetLabCtrl("PRELOG_MULTIPLE_LIMS_CASES") != "T") {
            if (!CASESEARCHPOPUPMode && GetLabCtrl("USES_QC_DEPT_CASE_SEARCH") != "T") {
                labCase = deptCaseNumber;
                deptCaseNumber = "";
            }
        
            var params = { departmentCaseNumber: deptCaseNumber, departmentCode: deptCode, labCase: labCase };
            $.ajax({
                type: "POST",
                url: "PLCWebCommon/PLCWebMethods.asmx/CheckCaseExistence",
                data: params,
                success: function (data) {
                    if (data == "T")
                        ShowPopup("Case already exists.");
                    else if (data == "X")
                        ShowPopup(labCaseLabel + " already exists.");
                    else
                        InitNewCaseQC();
                },
                error: function () {
                    ShowPopup("Case checking failed.");
                },
                dataType: "text"
            });
           
        }
        else
            InitNewCaseQC();

        // signifies that it is a new case during draft saving
        $('#hdnIsNewCaseDraft').val('T');

        function InitNewCaseQC() {
            //ClearAll();

            if ($("[id*=searchBox2]").css("display") == "block") {
                var $customPanelFields = _plcdbpanellist["dbpCaseSearch"].fields;
                if ($customPanelFields != null) {
                    for (var key in $customPanelFields) {
                        if (key in _plcdbpanellist["PLCDBPanel1"].fields) {
                            var val = GetDBPanelField("dbpCaseSearch", key);
                            if (val && (!IsLabCtrlSet("USES_WILD_CARD_SEARCH") || (val.indexOf("*") == -1 && val.indexOf("?") == -1)))
                                SetDBPanelField("PLCDBPanel1", key, val);
                        }
                    }
                }
            }

            if (deptCaseNumber) SetDBPanelField("PLCDBPanel1", $deptCaseNumber, deptCaseNumber);

            if (deptCode) {             
                   SetDBPanelField("PLCDBPanel1", $deptCode, deptCode);               
                if ($caseDeptCode != null)
                    SetDBPanelField("PLCDBPanel1", $caseDeptCode, deptCode);

                if (CASESEARCHPOPUPMode)
                    setPictureMask(false, "PLCDBPanel1", deptCode);
            }

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

            if ($("input[id$='hdnHighVolumeSubmissionFlag']").val() == 'T')                
                ResetDefaultSubmissionType();
           
            if ($("#chkHistCase").is(":checked")) {
                var labcaseyear = $("#txtQC_LabCaseYear").val();

                if (labcaseyear != "") {
                    $("[id*=hdnPreCaseLabCaseYear]").val(labcaseyear);
                    hideQCSearchPopUp();
                }
                else
                    ShowPopup("You need to enter a lab case year when creating historical cases.");
            }
            else
                hideQCSearchPopUp();

            TriggerMandatoryBaseEvents();
            ToggleGridVisibility();

            SetQCByUserPrefs(true);

            loadJSONDBPanelValues("PLCDBPanel1", { clear: true });


            if (GetLabCtrl("QC_PREVIEW_LAB_CASE_NUMBER") == "T") {
                $("[id$='pnlLabCaseNum']").css("display", "block");
                GetNextLabCaseNumber();
            }
        }
    }
    else if (isPrelogLIMSSearch() && prelogLIMSRefIDs != "") {
        //if (prelogLIMSRefIDs["caseInLIMS"] == "T")
        //  SetPrelogCaseData(prelogLIMSRefIDs["labCase"]);

        if (GetLabCtrl("PRELOG_MULTIPLE_LIMS_CASES") == "T") {
            if ($("[id*=hdnEItemCaseKey]").val() != "") {

                Alert("Selected / Scanned prelog case contains existing items. Creating a new case is restricted");
                return;
            }
        }

        setPictureMaskFromData(prelogLIMSRefIDs, "PLCDBPanel1");
        GetPrelogTran(prelogLIMSRefIDs, true);
        $("[id*=hdnFromKitSearch]").val("");
    }
    

    //if ($("#chkHighVolume").is(":checked")) {
    if (GetClientField(_miscClientIDs.chkhighvolume) == "true" || GetClientField(_miscClientIDs.chkhighvolume)) {
        $("input[id$='hdnHighVolumeSubmissionFlag']").val('T');
    }

    setDefaultPriority();

  //overwrite current value with default value for CURRENTDATE and CURRENTTIME
    setCurrentDateAndTimeDefaultValue("PLCDBPanel1");

    //callAutoFillEvent("PLCDBPanel1", true);
    setTimeout(function () {
        callAutoFillEvent("PLCDBPanel1", true);
    }, 1000);

}