//-------------------------- PRELOG NAMES AND ITEMS GRID ---------------------------//
    
// grid creator
function CreateGrid() { 
    CreateNamesGrid(true, 20, false, "NameEntryList");
    CreateItemsGrid(true, 20, false, "ItemEntryList");
    CreateExistingItemsGrid(true, 20, false, "SubmittedItemEntryList");
    CreateExistingNamesGrid(true, 20, false, "ExistingNamesEntryList");
    CreateDistributionGrid(true, 20, false, "DistributionEntryList");
    BindScrollOnGrid();
}

// the 2 functions below are triggered when 'More Fields' link is clicked 
function AddMoreFieldsNameEntryList() {
    CreateNamesGrid(false, 10, false, "NameEntryList");
    return false;
}

function AddMoreFieldsItemEntryList() {
    CreateItemsGrid(false, 10, false, "ItemEntryList");
    return false;

}

// creates the Names Grid by script
function CreateNamesGrid(onLoad, genLimit, clearGrid, parentTableID) {
    var namesTable = $("#" + parentTableID);
    var reqField = "<strong style='color: red; font-size: 11pt;'>*</strong>";
    var currRowCount = 0;

    RemoveArrayElement(nameReqFields, "STATE_ID");
    RemoveArrayElement(nameReqFields, "FBI_NUMBER");
    RemoveArrayElement(nameReqFields, "DCI_NUMBER");

    if (GetDeptCtrl("USES_STATE_ID") === "R" && $.inArray("STATE_ID", nameReqFields) < 0)
        nameReqFields.push("STATE_ID");
    if (GetDeptCtrl("USES_FBI_NUMBER") === "R" && $.inArray("FBI_NUMBER", nameReqFields) < 0)
        nameReqFields.push("FBI_NUMBER");
    if (GetDeptCtrl("USES_DCI_NUMBER") === "R" && $.inArray("DCI_NUMBER", nameReqFields) < 0)
        nameReqFields.push("DCI_NUMBER");

    if (onLoad) {
        namesTable.html("");
        var trHeader = $("<tr/>", { align: "left" });
        trHeader.addClass("prelogGridHeader").attr("valign", "middle").attr("align", "left");

        var thDeleteRow = $("<th/>", { scope: "col" }).html("&nbsp;&nbsp;"); // delete column
        var thNameType = $("<th/>", { scope: "col" }).html(GetCasePrelogSysPrompt("CasePrelog.thNameType", "Name Type " )+ reqField);
        var thLastName = $("<th/>", { scope: "col" }).html(GetCasePrelogSysPrompt("CasePrelog.thLastName", "Last Name ") + reqField);
        var thFirstName = $("<th/>", { scope: "col" }).html(GetCasePrelogSysPrompt("CasePrelog.thFirstName", "First Name"));
        var thMiddleName = $("<th/>", { scope: "col" }).html(GetCasePrelogSysPrompt("CasePrelog.thMiddleName", "Middle Name"));
        var thSuffix = $("<th/>", { scope: "col" }).html(GetCasePrelogSysPrompt("CasePrelog.thSuffix", "Suffix"));
        var thDateOfBirth = $("<th/>", { scope: "col" }).html(GetCasePrelogSysPrompt("CasePrelog.thDateOfBirth", "Date of Birth"));
        var thSexCode = $("<th/>", { scope: "col" }).html(GetCasePrelogSysPrompt("CasePrelog.thSexCode", "Sex"));
        var thRaceCode = $("<th/>", { scope: "col" }).html(GetCasePrelogSysPrompt("CasePrelog.thRaceCode", "Race"));
        var thAlive = $("<th/>", { scope: "col" }).html(GetCasePrelogSysPrompt("CasePrelog.thAlive", "Alive?"));
        var thBleeding = $("<th/>", { scope: "col" }).html(GetCasePrelogSysPrompt("CasePrelog.thBleeding", "Bleeding?"));
        var thTransfused = $("<th/>", { scope: "col" }).html(GetCasePrelogSysPrompt("CasePrelog.thTransfused", "Transfused?"));
        var thDeptCode = $("<th/>", { scope: "col" }).html("Dept Code");
        var thDeptCaseNum = $("<th/>", { scope: "col" }).html("Dept Case #");
        var thSubNum = $("<th/>", { scope: "col" }).html("Submission #");
        var thNameNum = $("<th/>", { scope: "col" }).html("Name #");
        var thNameSeq = $("<th/>", { scope: "col" }).html("Name Sequence");
        var thStateID = $("<th/>", { scope: "col" }).html("DL # " + (GetDeptCtrl("USES_STATE_ID") === "R" ? reqField : ""));
        var thFBINumber = $("<th/>", { scope: "col" }).html("FBI # " + (GetDeptCtrl("USES_FBI_NUMBER") === "R" ? reqField : ""));
        var thDCINumber = $("<th/>", { scope: "col" }).html("DCI # " + (GetDeptCtrl("USES_DCI_NUMBER") === "R" ? reqField : ""));
        var thNameFields = $("<th/>", { scope: "col" }).html("&nbsp;&nbsp;&nbsp;&nbsp;");
        var thComments = $("<th/>", { scope: "col" }).html("Comments");
        var thSubjectCharged = $("<th/>", { scope: "col" }).html("Charged");
        var thTrialDate = $("<th/>", { scope: "col" }).html("Trial Date");
        var thGrandJuryDate = $("<th/>", { scope: "col" }).html("Grand Jury Date");
        var thArrestNumber = $("<th/>", { scope: "col" }).html("Arrest #");
        var thStatus = $("<th/>", { scope: "col" }).html("Status");

        trHeader.append(thDeleteRow);
        trHeader.append(thNameType);
        trHeader.append(thLastName);
        trHeader.append(thFirstName);
        trHeader.append(thMiddleName);
        trHeader.append(thSuffix);
        trHeader.append(thDateOfBirth);
        trHeader.append(thSexCode);
        trHeader.append(thRaceCode);
        trHeader.append(thSubNum);
        trHeader.append(thNameNum);
        trHeader.append(thDeptCode);
        trHeader.append(thDeptCaseNum);
        trHeader.append(thNameSeq);
        trHeader.append(thStateID);
        trHeader.append(thFBINumber);
        trHeader.append(thDCINumber);
        trHeader.append(thComments);
        trHeader.append(thSubjectCharged);
        trHeader.append(thTrialDate);
        trHeader.append(thGrandJuryDate);
        trHeader.append(thArrestNumber);
        trHeader.append(thStatus);
        trHeader.append(thNameFields);
        trHeader.append(thAlive);
        trHeader.append(thBleeding);
        trHeader.append(thTransfused);
        namesTable.append(trHeader);

        HideColumn(thDeptCode);
        HideColumn(thDeptCaseNum);
        HideColumn(thSubNum);
        HideColumn(thNameNum);
        HideColumn(thNameSeq);

        if (GetDeptCtrl("USES_UNKNOWN_JUVENILE") !== "T" && GetDeptCtrl("USES_NAME_VERIFICATION") !== "T")
            HideColumn(thNameFields);


        if (GetDeptCtrl("HIDE_RACE") == "T")
            HideColumn(thRaceCode);

        if (GetDeptCtrl("USES_NAME_SUFFIX") !== "T")
            HideColumn(thSuffix);


        if (GetDeptCtrl("USES_DCI_NUMBER") !== "T" && GetDeptCtrl("USES_DCI_NUMBER") !== "R")
            HideColumn(thDCINumber);

        if (GetDeptCtrl("USES_FBI_NUMBER") !== "T" && GetDeptCtrl("USES_FBI_NUMBER") !== "R")
            HideColumn(thFBINumber);

        if (GetDeptCtrl("USES_STATE_ID") !== "T" && GetDeptCtrl("USES_STATE_ID") !== "R")
            HideColumn(thStateID);

        if (GetDeptCtrl("USES_COMMENTS") !== "T")
            HideColumn(thComments);
        if (GetDeptCtrl("USES_SUBJECT_CHARGED") !== "T")
            HideColumn(thSubjectCharged);
        if (GetDeptCtrl("USES_TRIAL_DATE") !== "T")
            HideColumn(thTrialDate);
        if (GetDeptCtrl("USES_GRAND_JURY_DATE") !== "T")
            HideColumn(thGrandJuryDate);
        if (GetDeptCtrl("USES_DOC_NUMBER") !== "T")
            HideColumn(thArrestNumber);
        if (GetDeptCtrl("USES_NAME_STATUS") !== "T")
            HideColumn(thStatus);

        if (GetDeptCtrl("USES_MEDSTATUS_OPTIONS") !== "T") {
            HideColumn(thAlive);
            HideColumn(thBleeding);
            HideColumn(thTransfused);
        }

        nameRowCtr = 0;
    }

    while (currRowCount < genLimit) {

        AddNameRow(namesTable, true);
        nameRowCtr++;
        currRowCount++;
    }
}

// generates the rows for Names Grid 
function AddNameRow(tbl, setClear) {
    var trNameRow = $("<tr/>", { id: "TR_NEF" + nameRowCtr });
    var namePrfx = "_NEF" + nameRowCtr;

    var tdDeleteRow = $("<td/>");
    var tdNameType = $("<td/>");
    var tdLastName = $("<td/>");
    var tdFirstName = $("<td/>");
    var tdMiddleName = $("<td/>");
    var tdDateOfBirth = $("<td/>");
    var tdSexCode = $("<td/>");
    var tdRaceCode = $("<td/>");
    var tdSubNum = $("<td/>");
    var tdNameNum = $("<td/>");
    var tdDeptCode = $("<td/>");
    var tdDeptCaseNum = $("<td/>");
    var tdExists = $("<td/>");
    var tdNameKey = $("<td/>");
    var tdNameSeq = $("<td/>");
    var tdSuffix = $("<td/>");
    var tdStateID = $("<td/>");
    var tdFBINumber = $("<td/>");
    var tdDCINumber = $("<td/>");
    var tdNameFields = $("<td/>");
    var tdComments = $("<td/>");
    var tdSubjectCharged = $("<td/>");
    var tdTrialDate = $("<td/>");
    var tdGrandJuryDate = $("<td/>");
    var tdArrestNumber = $("<td/>");
    var tdStatus = $("<td/>");
    var tdAlive = $("<td/>");
    var tdBleeding = $("<td/>");
    var tdTransfused = $("<td/>");



    var btnDeleteRow = $("<button/>", { id: "DELETE_BUTTON_" + namePrfx }).css({ "width": "20px", "font-size": "10px", "text-align": "center", "padding": "1px" });
    var fbdivNameType = $("<div/>", { id: "NAME_TYPE_CODE_fbLookup_" + namePrfx }).css("width", "150px").addClass("plField");
    var inputLastName = $("<input/>", { id: "LAST_NAME_" + namePrfx, type: "text" }).css("width", "130px").addClass("plField");
    var inputFirstName = $("<input/>", { id: "FIRST_NAME_" + namePrfx, type: "text" }).css("width", "130px").addClass("plField");
    var inputMiddleName = $("<input/>", { id: "MIDDLE_NAME_" + namePrfx, type: "text" }).css("width", "88px").addClass("plField");
    var inputDateOfBirth = $("<input/>", { id: "DATE_OF_BIRTH_" + namePrfx, type: "text" }).css("width", "100px").addClass("plField");
    var fbdivSexCode = $("<div/>", { id: "SEX_CODE_fbLookup_" + namePrfx }).addClass("plField");
    var fbdivRaceCode = $("<div/>", { id: "RACE_CODE_fbLookup_" + namePrfx }).css("width", "100px").addClass("plField");
    var inputSuffix = $("<input/>", { id: "SUFFIX_" + namePrfx, type: "text" }).css("width", "80px").addClass("plField");
    var $inputStateID = $("<input/>", { id: "STATE_ID_" + namePrfx, type: "text" }).css("width", "80px").addClass("plField");
    var $inputFBINumber = $("<input/>", { id: "FBI_NUMBER_" + namePrfx, type: "text" }).css("width", "80px").addClass("plField");
    var $inputDCINumber = $("<input/>", { id: "DCI_NUMBER_" + namePrfx, type: "text" }).css("width", "80px").addClass("plField");
    var $btnNameFields = $("<button/>", { id: "NAME_FIELDS_" + namePrfx }).text("Additional Name Fields").addClass("addtlfields");
    var $inputComments = $("<input/>", { id: "COMMENTS_" + namePrfx, type: "text", spellcheck: "true" }).css("width", "200px").addClass("plField");
    var $txtComments = $("<textarea/>", { id: "$Comments" + namePrfx }).css({ "display": "none"});
    var $btnEditComments = $("<input/>", { title: "Edit", id: "NAME_COMMENT_BTN" + namePrfx, type: "image", src: "../Images/edit-text.png", style: "width:22px;" });
    var $inputSubjectCharged = $("<input/>", { id: "SUBJECT_CHARGED_" + namePrfx, type: "checkbox" }).css("width", "50px");
    var $inputTrialDate = $("<input/>", { id: "TRIAL_DATE_" + namePrfx, type: "text" }).css("width", "100px").addClass("plField");;
    var $inputGrandJuryDate = $("<input/>", { id: "GRAND_JURY_DATE_" + namePrfx, type: "text" }).css("width", "100px").addClass("plField");;
    var $inputArrestNumber = $("<input/>", { id: "DOC_NUMBER_" + namePrfx, type: "text" }).css("width", "80px").addClass("plField");
    var fbDivStatus = $("<div/>", { id: "STATUS_fbLookup_" + namePrfx }).css("width", "100px").addClass("plField");
    var fbdivAlive = $("<div/>", { id: "ALIVE_CODE_fbLookup_" + namePrfx }).css("width", "100px").addClass("plField");
    var fbdivBleeding = $("<div/>", { id: "BLEEDING_CODE_fbLookup_" + namePrfx }).css("width", "100px").addClass("plField");
    var fbdivTransfused = $("<div/>", { id: "TRANSFUSED_CODE_fbLookup_" + namePrfx }).css("width", "100px").addClass("plField");

    //hidden fields
    var inputSubNum = $("<input/>", { id: "SUBMISSION_NUMBER_" + namePrfx, type: "text" }).css("width", "175px").addClass("plField");
    var inputNameNum = $("<input/>", { id: "NAME_NUMBER_" + namePrfx, type: "text" }).css("width", "175px").addClass("plField");
    var inputDeptCode = $("<input/>", { id: "DEPARTMENT_CODE_" + namePrfx, type: "text" }).css("width", "175px").addClass("plField");
    var inputDeptCaseNum = $("<input/>", { id: "DEPARTMENT_CASE_NUMBER_" + namePrfx, type: "text" }).css("width", "175px").addClass("plField");
    var inputNameKey = $("<input/>", { id: "NAME_KEY_" + namePrfx, type: "text" }).css("width", "175px").addClass("plField");
    var inputNameSeq = $("<input/>", { id: "NAME_SEQUENCE_" + namePrfx, type: "text" }).css("width", "175px").addClass("plField");

    btnDeleteRow.text("X");
    btnDeleteRow.bind("click", fnDeleteRow);
    fbdivNameType.bind("change", fnHasChanges);
    inputLastName.bind("change", fnHasChanges);
    inputFirstName.bind("change", fnHasChanges);
    inputMiddleName.bind("change", fnHasChanges);
    inputDateOfBirth.bind("change", fnHasChanges);
    fbdivSexCode.bind("change", fnHasChanges);


    $btnNameFields.click(function () {
        showAdditionalNameFields(this);
        return false;
    });
    

    tdDeleteRow.append(btnDeleteRow);
    tdNameType.append(fbdivNameType);
    tdLastName.append(FormatInputMask(inputLastName));
    tdFirstName.append(FormatInputMask(inputFirstName));
    tdMiddleName.append(FormatInputMask(inputMiddleName));
    tdSuffix.append(inputSuffix);
    tdDateOfBirth.append(inputDateOfBirth);
    tdSexCode.append(fbdivSexCode);
    tdRaceCode.append(fbdivRaceCode);
    tdSubNum.append(inputSubNum);
    tdNameNum.append(inputNameNum);
    tdDeptCode.append(inputDeptCode);
    tdDeptCaseNum.append(inputDeptCaseNum);
    tdNameKey.append(inputNameKey);
    tdNameSeq.append(inputNameSeq);
    tdStateID.append($inputStateID);
    tdFBINumber.append($inputFBINumber);
    tdDCINumber.append($inputDCINumber);
    tdNameFields.append($btnNameFields);
    tdComments.append($txtComments);
    tdComments.append($inputComments);
    tdComments.append($btnEditComments);
    tdSubjectCharged.append($inputSubjectCharged);
    tdTrialDate.append($inputTrialDate);
    tdGrandJuryDate.append($inputGrandJuryDate);
    tdArrestNumber.append($inputArrestNumber)
    tdStatus.append(fbDivStatus);
    tdAlive.append(fbdivAlive);
    tdBleeding.append(fbdivBleeding);
    tdTransfused.append(fbdivTransfused);

    fbdivNameType.flexbox("../PLCWebCommon/Results.aspx?t=TV_NAMETYPE&f=INCLUDE_IN_PRELOG='T'", {
        method: 'POST', initialValue: '', initialText: '', width: 180, descFormat: '', descSeparator: '', usesArrowNavigation: true,
        valueChanged: function (obj) {
            var $tr = obj.parent().parent();
            var $btnNameFields = $tr.find("[id*=NAME_FIELDS_]");
            var nameType = obj.getCode();

            if ($btnNameFields && $btnNameFields.closest("td").css("display") != "none") {
                var savedData = $btnNameFields.data("fieldvalues") || []; 
                var required = (nameType != "");

                if (required) {

                    var addReqClass = true;
                    if (savedData) {
                        if (nameFieldHasValue(savedData)) {
                            setNameFieldsColorToFilled($btnNameFields);
                            addReqClass = false;
                        }                     
                    }

                    if (addReqClass) {
                        setNameFieldsColorToRequired($btnNameFields);
                    }
                }
                else {
                    if ($btnNameFields.hasClass("namefields-required"))
                        setNameFieldsColorToDefault($btnNameFields);
                }
              
            }
        },
        attachPopupTo: "body"
    });
    fbdivSexCode.flexbox("../PLCWebCommon/Results.aspx?t=TV_SEXCODE&f=(INCLUDE_IN_PRELOG <> 'F' OR INCLUDE_IN_PRELOG IS NULL)", { method: 'POST', initialValue: '', initialText: '', width: 100, descFormat: '', descSeparator: '', usesArrowNavigation: true, attachPopupTo: "body" });
    fbdivRaceCode.flexbox("../PLCWebCommon/Results.aspx?t=TV_RACECODE&f=(INCLUDE_IN_PRELOG <> 'F' OR INCLUDE_IN_PRELOG IS NULL)", { method: 'POST', initialValue: '', initialText: '', width: 200, descFormat: '', descSeparator: '', usesArrowNavigation: true, attachPopupTo: "body" });
    fbDivStatus.flexbox("../PLCWebCommon/Results.aspx?t=TV_NAMESTAT&f=(INCLUDE_IN_PRELOG <> 'F' OR INCLUDE_IN_PRELOG IS NULL)", { method: 'POST', initialValue: '', initialText: '', width: 100, descFormat: '', descSeparator: '', usesArrowNavigation: true, attachPopupTo: "body", valueChanged: function (obj) { checkPrelogPriority(obj)} });

    fbdivAlive.flexbox("../PLCWebCommon/Results.aspx?t=TV_MEDSTATUSOPTIONS&f=(INCLUDE_IN_PRELOG <> 'F' OR INCLUDE_IN_PRELOG IS NULL)", { method: 'POST', initialValue: '', initialText: '', width: 200, descFormat: '', descSeparator: '', usesArrowNavigation: true, attachPopupTo: "body" });
    fbdivBleeding.flexbox("../PLCWebCommon/Results.aspx?t=TV_MEDSTATUSOPTIONS&f=(INCLUDE_IN_PRELOG <> 'F' OR INCLUDE_IN_PRELOG IS NULL)", { method: 'POST', initialValue: '', initialText: '', width: 200, descFormat: '', descSeparator: '', usesArrowNavigation: true, attachPopupTo: "body" });
    fbdivTransfused.flexbox("../PLCWebCommon/Results.aspx?t=TV_MEDSTATUSOPTIONS&f=(INCLUDE_IN_PRELOG <> 'F' OR INCLUDE_IN_PRELOG IS NULL)", { method: 'POST', initialValue: '', initialText: '', width: 200, descFormat: '', descSeparator: '', usesArrowNavigation: true, attachPopupTo: "body" });

    trNameRow.append(tdDeleteRow);
    trNameRow.append(tdNameType);
    trNameRow.append(tdLastName);
    trNameRow.append(tdFirstName);
    trNameRow.append(tdMiddleName);
    trNameRow.append(tdSuffix);
    trNameRow.append(tdDateOfBirth);
    trNameRow.append(tdSexCode);
    trNameRow.append(tdRaceCode);
    trNameRow.append(tdSubNum);
    trNameRow.append(tdNameNum);
    trNameRow.append(tdDeptCode);
    trNameRow.append(tdDeptCaseNum);
    trNameRow.append(tdNameKey);
    trNameRow.append(tdNameSeq);
    trNameRow.append(tdStateID);
    trNameRow.append(tdFBINumber);
    trNameRow.append(tdDCINumber);
    trNameRow.append(tdComments);
    trNameRow.append(tdSubjectCharged);
    trNameRow.append(tdTrialDate);
    trNameRow.append(tdGrandJuryDate);
    trNameRow.append(tdArrestNumber);
    trNameRow.append(tdStatus);
    trNameRow.append(tdNameFields);
    trNameRow.append(tdAlive);
    trNameRow.append(tdBleeding);
    trNameRow.append(tdTransfused);

    tbl.append(trNameRow);

    HideColumn(tdDeptCode);
    HideColumn(tdDeptCaseNum);
    HideColumn(tdSubNum);
    HideColumn(tdNameNum);
    HideColumn(tdNameKey);
    HideColumn(tdNameSeq);

    if (GetDeptCtrl("HIDE_RACE") == "T")
        HideColumn(tdRaceCode);

    if (GetDeptCtrl("USES_NAME_SUFFIX") !== "T")
        HideColumn(tdSuffix);

    appendDOBDatePicker(inputDateOfBirth, "maskNameDOB" + namePrfx);
    appendDateControl($inputTrialDate, "maskNameTrialDate" + namePrfx);
    appendDateControl($inputGrandJuryDate, "maskNameGrandJuryDate" + namePrfx);
    initExpandTextBox($inputComments, $txtComments, $btnEditComments, "Comment");

    if (GetDeptCtrl("USES_DCI_NUMBER") !== "T" && GetDeptCtrl("USES_DCI_NUMBER") !== "R")
        HideColumn(tdDCINumber);

    if (GetDeptCtrl("USES_FBI_NUMBER") !== "T" && GetDeptCtrl("USES_FBI_NUMBER") !== "R")
        HideColumn(tdFBINumber);

    if (GetDeptCtrl("USES_STATE_ID") !== "T" && GetDeptCtrl("USES_STATE_ID") !== "R")
        HideColumn(tdStateID);

    if (GetDeptCtrl("USES_UNKNOWN_JUVENILE") !== "T" && GetDeptCtrl("USES_NAME_VERIFICATION") !== "T")
        HideColumn(tdNameFields);

    if (GetDeptCtrl("USES_COMMENTS") !== "T")
        HideColumn(tdComments);
    if (GetDeptCtrl("USES_SUBJECT_CHARGED") !== "T")
        HideColumn(tdSubjectCharged);
    if (GetDeptCtrl("USES_TRIAL_DATE") !== "T")
        HideColumn(tdTrialDate);
    if (GetDeptCtrl("USES_GRAND_JURY_DATE") !== "T")
        HideColumn(tdGrandJuryDate);
    if (GetDeptCtrl("USES_DOC_NUMBER") !== "T")
        HideColumn(tdArrestNumber);
    if (GetDeptCtrl("USES_NAME_STATUS") !== "T")
        HideColumn(tdStatus);

    if (GetDeptCtrl("USES_MEDSTATUS_OPTIONS") !== "T") {
        HideColumn(tdAlive);
        HideColumn(tdBleeding);
        HideColumn(tdTransfused);
    }
}

/* hides any id'D table column element */
function HideColumn($th, $td, $input) {
    if (IsDefined($th))
        SetAttribute($th, null, null, { "display": "none" }, true);
    if (IsDefined($td))
        SetAttribute($td, null, null, { "display": "none" }, true);
    if (IsDefined($input))
        SetAttribute($input, null, null, { "display": "none" }, true);
}