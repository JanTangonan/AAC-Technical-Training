function CreateNamesGrid(names, setClear) {
    var reqField = "<strong style='color: red; font-size: 11pt;'>*</strong>";
    var $divNames = $("<div/>", { id: "divnames" });
    $divNames.addClass("qcGrid").addClass("flexboxscroll").css("display", "block");

    RemoveArrayElement(nameReqFields, "COMMENTS");    //Mock Ticket #30001 – New column in Names grid
    if (GetDeptCtrl("USES_COMMENTS") === "R" && $.inArray("COMMENTS", nameReqFields) < 0)
        nameReqFields.push("COMMENTS");
  
    var $tbl = $("<table/>", {id:"tblNames"});
    var $trHead = $("<tr/>", {align:"left"});
    $trHead.addClass("qcGridHeader").attr("valign","middle").attr("align","left");
    var $thArrestNumber     = $("<th/>", { scope: "col" }).html(GetQCSysPrompt("QuickCreate_Advanced.thArrestNumber", "Arrest Number", true));
    var $thType             = $("<th/>", { scope: "col" }).html(GetQCSysPrompt("QuickCreate_Advanced.thType", "Type"));
    var $thLast             = $("<th/>", { scope: "col" }).html(GetQCSysPrompt("QuickCreate_Advanced.thLast", LastNameHeader));
    var $thFirst            = $("<th/>", { scope: "col" }).html(GetQCSysPrompt("QuickCreate_Advanced.thFirst", "First"));
    var $thMiddle           = $("<th/>", { scope: "col" }).html(GetQCSysPrompt("QuickCreate_Advanced.thMiddle", "Middle"));
    var $thSuffix           = $("<th/>", { scope: "col" }).html(GetQCSysPrompt("QuickCreate_Advanced.thSuffix", "Suffix"));
    var $thSex              = $("<th/>", { scope: "col" }).html(GetQCSysPrompt("QuickCreate_Advanced.thSex", "Sex"));
    var $thRace             = $("<th/>", { scope: "col" }).html(GetQCSysPrompt("QuickCreate_Advanced.thRace", "Race"));
    var $thDOB              = $("<th/>", { scope: "col" }).html(GetQCSysPrompt("QuickCreate_Advanced.thDOB", "Date of Birth"));
    var $thAge              = $("<th/>", { scope: "col" }).html(GetQCSysPrompt("QuickCreate_Advanced.thAge", "Age"));
    var $thArrestDate       = $("<th/>", { scope: "col" }).html(GetQCSysPrompt("QuickCreate_Advanced.thArrestDate", "Arrest Date"));
    var $thJuvenile         = $("<th/>", { scope: "col" }).html(GetQCSysPrompt("QuickCreate_Advanced.thJuvenile", "Juvenile"));
    var $thStateID          = $("<th/>", { scope: "col" }).html(GetQCSysPrompt("QuickCreate_Advanced.thStateID", GetLabCtrl("QC_STATE_ID_LABEL")));
    var referenceText       = (GetLabCtrl("NAME_REF_TEXT") == "" ? "Reference Type and Reference" : GetLabCtrl("NAME_REF_TEXT"));
    var $thReference        = $("<th/>", { scope: "col" }).html(GetQCSysPrompt("QuickCreate_Advanced.thReference", referenceText));
    var $thOldNameType      = $("<th/>", { scope: "col" }).html("Old Name Type");
    var $thOldLName         = $("<th/>", { scope: "col" }).html("Old Last Name");
    var $thOldFName         = $("<th/>", { scope: "col" }).html("Old First Name");
    var $thOldMName         = $("<th/>", { scope: "col" }).html("Old Middle Name");
    var $thOldSex           = $("<th/>", { scope: "col" }).html("Old Sex");
    var $thOldRace          = $("<th/>", { scope: "col" }).html("Old Race");
    var $thAddressBook      = $("<th/>", { scope: "col" }).html(GetQCSysPrompt("QuickCreate_Advanced.thAddressBook", "Address"));

    var NameIDText          = (GetLabCtrl("QC_NAME_ID_TEXT") == "" ? "Name ID" : GetLabCtrl("QC_NAME_ID_TEXT"));
    var $thNameID           = $("<th/>", { scope: "col" }).html(GetQCSysPrompt("QuickCreate_Advanced.thNameID", NameIDText));
    var $thHomePhone        = $("<th/>", { scope: "col" }).html(GetQCSysPrompt("QuickCreate_Advanced.thHomePhone", "Home Phone"));
    var $thCellPhone        = $("<th/>", { scope: "col" }).html(GetQCSysPrompt("QuickCreate_Advanced.thCellPhone", "Cell Phone"));

    //hidden fields
    var $thPIDNumber = $("<th/>", { scope: "col" }).html(GetQCSysPrompt("QuickCreate_Advanced.thPIDNumber","PID Number"));
    var $thFBINumber = $("<th/>", { scope: "col" }).html(GetQCSysPrompt("QuickCreate_Advanced.thFBINumber","FBI #"));
    var $thDCINumber = $("<th/>", { scope: "col" }).html(GetQCSysPrompt("QuickCreate_Advanced.thDCINumber", "DCI #"));

    //Mock Ticket #30001 – New column in Names grid
    var $thComments = $("<th/>", { scope: "col" }).html(GetQCSysPrompt("QuickCreate_Advanced.thComments", "Comments" + (GetLabCtrl("USES_QC_COMMENTS") === "R" ? reqField : "")));

    var $thSubjectCharged = $("<th/>", { scope: "col" }).html(GetQCSysPrompt("QuickCreate_Advanced.thSubjectCharged", "Charged"));
    var $thTrialDate = $("<th/>", { scope: "col" }).html(GetQCSysPrompt("QuickCreate_Advanced.thTrialDate", "Trial Date"));
    var $thGrandJuryDate = $("<th/>", { scope: "col" }).html(GetQCSysPrompt("QuickCreate_Advanced.thGrandJuryDate", "Grand Jury Date"));
    var $thStatus = $("<th/>", { scope: "col" }).html(GetQCSysPrompt("QuickCreate_Advanced.thStatus", "Status"));
    var $thAlive = $("<th/>", { scope: "col" }).html(GetQCSysPrompt("QuickCreate_Advanced.thAlive", "Alive?"));
    var $thBleeding = $("<th/>", { scope: "col" }).html(GetQCSysPrompt("QuickCreate_Advanced.thBleeding", "Bleeding?"));
    var $thTransfused = $("<th/>", { scope: "col" }).html(GetQCSysPrompt("QuickCreate_Advanced.thTransfused", "Transfused?"));

    $trHead.append($thType);
    $trHead.append($thLast);
    $trHead.append($thFirst);
    $trHead.append($thMiddle);
    $trHead.append($thSuffix);
    $trHead.append($thSex);
    $trHead.append($thRace);
    $trHead.append($thDOB);
    $trHead.append($thAge);
    $trHead.append($thArrestDate);
    $trHead.append($thJuvenile);
    $trHead.append($thArrestNumber);
    $trHead.append($thStateID);
    $trHead.append($thReference);
    $trHead.append($thOldNameType);
    $trHead.append($thOldLName);
    $trHead.append($thOldFName);
    $trHead.append($thOldMName);
    $trHead.append($thOldSex);
    $trHead.append($thOldRace);
    $trHead.append($thAddressBook);
    $trHead.append($thNameID);
    $trHead.append($thHomePhone);
    $trHead.append($thCellPhone);
    $trHead.append($thPIDNumber);
    $trHead.append($thFBINumber);
    $trHead.append($thDCINumber);

    //Mock Ticket #30001 – New column in Names grid
    $trHead.append($thComments);

    $trHead.append($thSubjectCharged);
    $trHead.append($thTrialDate);
    $trHead.append($thGrandJuryDate);
    $trHead.append($thStatus);
    $trHead.append($thAlive);
    $trHead.append($thBleeding);
    $trHead.append($thTransfused);

    $tbl.append($trHead);

    $divNames.append($tbl);
    $divNames.scroll(function () { HandleFlexboxScroll(); });
    $("div[id$='divNames']").empty2();
    $("div[id$='divNames']").append($divNames);

    $thType.html("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + $thType.html());
    if (IsLabCtrlSet("HIDE_RACE_IN_QC"))
        HideColumn($thRace);
    if (!IsLabCtrlSet("USES_QC_NAME_AGE"))
        HideColumn($thAge);
    if (!IsLabCtrlSet("USE_ARREST_DATE"))
        HideColumn($thArrestDate);
    if (!IsLabCtrlSet("QC_SHOW_JUVENILE"))
        HideColumn($thJuvenile);
    if (GetLabCtrl("QC_STATE_ID_LABEL") == "")
        HideColumn($thStateID);
    if (GetLabCtrl("USES_QC_NAME_REFERENCE") != "T")
        HideColumn($thReference);
    if (GetLabCtrl("QC_NAMES_ADDRESS_BOOK") != "T")
        HideColumn($thAddressBook);
    if (!IsLabCtrlSet("QC_NAME_ID"))
        HideColumn($thNameID);
    if (!IsLabCtrlSet("QC_HOME_PHONE"))
        HideColumn($thHomePhone);
    if (!IsLabCtrlSet("QC_CELL_PHONE"))
        HideColumn($thCellPhone);
    if (!IsLabCtrlSet("USES_NAME_SUFFIX"))
        HideColumn($thSuffix);
    if (!IsLabCtrlSet("USES_FBI_NUMBER"))
        HideColumn($thFBINumber);

    if (!IsLabCtrlSet("USES_MEDSTATUS_OPTIONS")) {
        HideColumn($thAlive);
        HideColumn($thBleeding);
        HideColumn($thTransfused);
    }

    if (!IsLabCtrlSet("USES_DCI_NUMBER"))
        HideColumn($thDCINumber);

    //Mock Ticket #30001 – New column in Names grid
    if (GetLabCtrl("USES_QC_COMMENTS") !== "T" && GetLabCtrl("USES_QC_COMMENTS") !== "R")
        HideColumn($thComments);
    //if (!IsUserOptionSet("QCNAMECOM"))
    //    HideColumn($thComments);

    if (!IsLabCtrlSet("USES_QC_STATUS"))
        HideColumn($thStatus);
    if (!IsLabCtrlSet("USES_QC_SUBJECT_CHARGED"))
        HideColumn($thSubjectCharged);
    if (!IsLabCtrlSet("USES_QC_TRIAL_DATE"))
        HideColumn($thTrialDate);
    if (!IsLabCtrlSet("USES_QC_GRAND_JURY_DATE"))
        HideColumn($thGrandJuryDate);
    if (!IsLabCtrlSet("USES_QC_ARREST_NUMBER"))
        HideColumn($thArrestNumber);

    HideColumn($thOldNameType);
    HideColumn($thOldLName);
    HideColumn($thOldFName);
    HideColumn($thOldSex);
    HideColumn($thOldMName);
    HideColumn($thOldRace);
    HideColumn($thPIDNumber);
 

    _lastNameNum = 0;
    for (var i=0; i < names.length; i++) {
        var row = names[i];
        AddNameRow(row, $tbl, setClear);
    }

    //$$ Uncomment the line below to enable dynamic adding of names.
    AddNameRowLink();
}

function AddNameRowLink() {
    var $tbl = $("#tblNames");
    AddTableRowLink($tbl, 10, "names_addrowslink", "javascript:AddNewNameRows()", "Add more names");
}

function RemoveNameRowLink() {
    $("#names_addrowslink").remove();
}

function AddTableRowLink($tbl, numcols, rowlinkID, hreflink, hreftext) {
    var $tr = $("<tr/>", {id:rowlinkID});
    var $td = $("<td/>", {colspan:numcols.toString()});
    var $href = $("<a/>", {href:hreflink});
    $href.append(hreftext);

    $td.append($href);
    $tr.append($td);
    $tbl.append($tr);
}

function AddNewNameRows() {
    // Delete the current add rows link.
    RemoveNameRowLink();

    var $tbl = $("#tblNames");
    AddNameRows($tbl, 5);
    AddNameRowLink();

    dbpanelAttachPopupToParent("div[id$='divNames']", "#tblNames");

    setPageTabIndexes();
    var $deleteBtns = $("#qcDivs > div:visible").find("[id^='NAME_DELETEBUTTON']");
    $deleteBtns[$deleteBtns.length - 5].focus();
}

function AddNameRows($tbl, n) {
    for (var i=0; i<n; i++)
        AddNameRow(null, $tbl, true);
}

function AddNameRow(row, $tbl, setClear) {
    var $tr = $("<tr/>");

    // Create controls for this name row.

    // <td>..</td> elements
    var $tdArrestNumber = $("<td/>");
    var $tdNameType     = $("<td/>");
    var $tdLast         = $("<td/>");
    var $tdFirst        = $("<td/>");
    var $tdMiddle       = $("<td/>");
    var $tdSex          = $("<td/>");
    var $tdRace         = $("<td/>");
    var $tdDOB          = $("<td/>");
    var $tdAge          = $("<td/>");
    var $tdArrestDate   = $("<td/>");
    var $tdJuvenile     = $("<td/>");
    var $tdStateID      = $("<td/>");
    var $tdReference    = $("<td/>");
    var $tdOldReference = $("<td/>").css("display", "none");
    var $tdoldnametype  = $("<td/>");
    var $tdoldlname     = $("<td/>");
    var $tdoldfname     = $("<td/>");
    var $tdoldmname     = $("<td/>");
    var $tdoldsex       = $("<td/>");
    var $tdoldrace      = $("<td/>");
    var $tdAddress      = $("<td/>");
    var $tdNameID       = $("<td/>");
    var $tdHomePhone    = $("<td/>");
    var $tdCellPhone    = $("<td/>");
    var $tdFBINumber    = $("<td/>");
    var $tdPIDNumber    = $("<td/>");
    var $tdSuffix       = $("<td/>");
    var $tdAlive        = $("<td/>");
    var $tdBleeding     = $("<td/>");
    var $tdTransfused   = $("<td/>");
    var $tdDCINumber = $("<td/>");

    //Mock Ticket #30001 – New column in Names grid
    var $tdComments = $("<td/>");

    var $tdSubjectCharged = $("<td/>");
    var $tdTrialDate    = $("<td/>");
    var $tdGrandJuryDate = $("<td/>");
    var $tdStatus = $("<td/>");

    // <input>..</input> elements
    _lastNameNum++;
    var istr = _lastNameNum.toString();

    var $inputNameKey       = $("<input/>", { id: "NAME_NAMEKEY" + istr, type: "hidden" });
    var $inputDeptNameKey   = $("<input/>", { id: "NAME_DEPTNAMEKEY" + istr, type: "hidden" });
    var $stateID            = $("<input/>", {id:"NAME_STATEID"+istr, type:"text"}).css("width", "85px");
    var $inputArrestNumber  = $("<input/>", {id:"NAME_ARRESTNUMBER"+istr, type:"text"}).css("width","100px");
    var $fbdivNameType      = $("<div/>", {id:"NAME_NAMETYPE_fbLookup"+istr});
    var $inputLast          = $("<input/>", {id:"NAME_LAST" + istr, type:"text"}).css("width","175px");
    var $inputFirst         = $("<input/>", {id:"NAME_FIRST" + istr, type:"text"}).css("width","175px");
    var $inputMiddle        = $("<input/>", { id: "NAME_MIDDLE" + istr, type: "text" }).css("width", "80px");
    var $inputSuffix        = $("<input/>", { id: "NAME_SUFFIX" + istr, type: "text" }).css("width", "80px");
    var $fbdivSex           = $("<div/>", {id:"NAME_SEX_fbLookup"+istr});
    var $fbdivRace          = $("<div/>", { id: "NAME_RACE_fbLookup" + istr });
    var $inputDOB           = $("<input/>", {id:"NAME_DOB" + istr, type:"text"}).css("width","85px");
    var $inputAge           = $("<input/>", {id:"NAME_AGE" + istr, type:"text"}).css("width","35px");
    var $inputArrestDate    = $("<input/>", { id: "NAME_ARRESTDATE" + istr, type: "text"}).css("width", "85px");
    var $inputJuvenile      = $("<input/>", {id: "NAME_JUVENILE_cb" + istr, type:"checkbox"}).css("width", "50px");
    var $fbdivRefType       = $("<div/>", { id: "NAME_REFERENCETYPE_fbLookup" + istr }).css("float","left");
    var $inputRefText       = $("<input/>", { id: "NAME_REFERENCETEXT" + istr, type: "text" }).css("width", "95px");
    var $oldRefType         = $("<input/>", { id: "NAME_OLDREFTYPE" + istr, type: "hidden" });
    var $oldRefText         = $("<input/>", { id: "NAME_OLDREFTEXT" + istr, type: "hidden" });
    var $divReference       = $("<div/>").css("width", "185px");
    var $oldnametype        = $("<input/>", { id: "NAME_OLDNAMETYPE" + istr, type: "hidden" });
    var $oldlname           = $("<input/>", { id: "NAME_OLDLNAME" + istr, type: "hidden" });
    var $oldfname           = $("<input/>", { id: "NAME_OLDFNAME" + istr, type: "hidden" });
    var $oldmname           = $("<input/>", { id: "NAME_OLDMNAME" + istr, type: "hidden" });
    var $oldsex             = $("<input/>", { id: "NAME_OLDSEX" + istr, type: "hidden" });
    var $oldrace            = $("<input/>", { id: "NAME_OLDRACE" + istr, type: "hidden" });
    var $divAddress         = $("<div/>");
    var $hdnAddress         = $("<input/>", { id: "NAME_ADDRESS" + istr, type: "hidden" });
    var $inpuAddressImg     = $("<input/>", { id: "NAME_ADDRESSimage" + istr, type: "image", src: "Images/address-no-record.png" });
    $inpuAddressImg.css("width", "20px").css("height", "20px").click(function () { return showSupplementPopup("NAME_ADDRESS" + istr, "NAME_ADDRESSimage" + istr); });
    var $inputNameID        = $("<input/>", { id: "NAME_NAMEID" + istr, type: "text" }).css("width", "85px");
    var $inputHomePhone     = $("<input/>", { id: "NAME_HOMEPHONE" + istr, type: "text" }).css("width", "85px");
    var $inputCellPhone     = $("<input/>", { id: "NAME_CELLPHONE" + istr, type: "text" }).css("width", "85px");
    var $inputPIDNumber     = $("<input/>", { id: "NAME_PIDNUMBER" + istr, type: "text" }).css("width", "85px");
    var $inputFBINumber     = $("<input/>", { id: "NAME_FBINUMBER" + istr, type: "text" }).css("width", "85px");
    var $inputDeleteNameRow = $("<button/>", { id: "NAME_DELETEBUTTON" + istr }).css({ "width": "20px", "height": "18px", "font-size": "10px", "text-align": "center", "padding": "1px" });   
    var $inputDCINumber = $("<input/>", { id: "NAME_DCINUMBER" + istr, type: "text" }).css("width", "85px");

    //Mock Ticket #30001 – New column in Names grid
    var $inputComments      = $("<input/>", { id: "NAME_COMMENTS" + istr, type: "text", readonly: "readonly" }).css("width", "200px").addClass("readonly");;
    //var $txtComments        = $("<textarea/>", { id: "$COMMENTS" + istr }).css("display", "none");
    //var $btnEditComments = $("<input/>", { title: "Edit", id: "NAME_COMMENT_BTN" + istr, type: "image", src: "Images/edit-text.png", style: "width:22px;" });

    var $inputSubjectCharged = $("<input/>", { id: "NAME_SUBJECT_CHARGED_cb" + istr, type: "checkbox" }).css("width", "50px");
    var $inputTrialDate = $("<input/>", { id: "NAME_TRIAL_DATE" + istr, type: "text" }).css("width", "85px");
    var $inputGrandJuryDate = $("<input/>", { id: "NAME_GRAND_JURY_DATE" + istr, type: "text" }).css("width", "85px");
    var $fbdivStatus        = $("<div/>", { id: "NAME_STATUS_fbLookup" + istr });
    var $fbdivAlive         = $("<div/>", { id: "NAME_ALIVE_fbLookup" + istr });
    var $fbdivBleeding      = $("<div/>", { id: "NAME_BLEEDING_fbLookup" + istr });
    var $fbdivTransfused    = $("<div/>", { id: "NAME_TRANSFUSED_fbLookup" + istr });

    /* bind auto scroll events */
    if (GetLabCtrl("USES_QC_AUTO_SCROLL") == "T") {
        $stateID.bind("click", function () { ScrollToCenter($("#divnames"), this); });
        $inputArrestNumber.bind("click", function () { ScrollToCenter($("#divnames"), this); });
        $fbdivNameType.bind("click", function () { ScrollToCenter($("#divnames"), this); });
        $inputLast.bind("click", function () { ScrollToCenter($("#divnames"), this); });
        $inputFirst.bind("click", function () { ScrollToCenter($("#divnames"), this); });
        $inputMiddle.bind("click", function () { ScrollToCenter($("#divnames"), this); });
        $fbdivSex.bind("click", function () { ScrollToCenter($("#divnames"), this); });
        $fbdivRace.bind("click", function () { ScrollToCenter($("#divnames"), this); });
        $inputDOB.bind("click", function () { ScrollToCenter($("#divnames"), this); });
        $inputAge.bind("click", function () { ScrollToCenter($("#divnames"), this); });
        $inputArrestDate.bind("click", function () { ScrollToCenter($("#divnames"), this); });
        $inputJuvenile.bind("click", function () { ScrollToCenter($("#divnames"), this); });
        $fbdivRefType.bind("click", function () { ScrollToCenter($("#divnames"), this); });
        $inputRefText.bind("click", function () { ScrollToCenter($("#divnames"), this); });
        $inputNameID.bind("click", function () { ScrollToCenter($("#divnames"), this); });
        $inputHomePhone.bind("click", function () { ScrollToCenter($("#divnames"), this); });
        $inputCellPhone.bind("click", function () { ScrollToCenter($("#divnames"), this); });
        $fbdivAlive.bind("click", function () { ScrollToCenter($("#divnames"), this); });
        $fbdivBleeding.bind("click", function () { ScrollToCenter($("#divnames"), this); });
        $fbdivTransfused.bind("click", function () { ScrollToCenter($("#divnames"), this); });
    }

    // <td><input .. /></td>
    $tdNameType.append($inputNameKey);
    $tdNameType.append($inputDeptNameKey);
    $tdArrestNumber.append($inputArrestNumber);
    $tdNameType.append($fbdivNameType);
    $tdLast.append(FormatInputMask($inputLast, true));
    $tdFirst.append(FormatInputMask($inputFirst, true));
    $tdMiddle.append(FormatInputMask($inputMiddle, true));
    $tdSuffix.append($inputSuffix);
    $tdSex.append($fbdivSex);
    $tdRace.append($fbdivRace);
    $tdDOB.append($inputDOB);
    $tdAge.append($inputAge);
    $tdArrestDate.append($inputArrestDate);
    $tdArrestDate.append($stateID);
    $tdJuvenile.append($inputJuvenile);
    $tdStateID.append($stateID);
    $divReference.append($fbdivRefType).append($inputRefText).appendTo($tdReference);
    $tdOldReference.append($oldRefType).append($oldRefText);
    $tdoldnametype.append($oldnametype);
    $tdoldlname.append($oldlname);
    $tdoldfname.append($oldfname);
    $tdoldmname.append($oldmname);
    $tdoldsex.append($oldsex);
    $tdoldrace.append($oldrace);
    $divAddress.append($hdnAddress);
    $divAddress.append($inpuAddressImg);
    $tdAddress.append($divAddress.css("text-align", "center"));
    $tdNameID.append($inputNameID);
    $tdHomePhone.append($inputHomePhone);
    $tdCellPhone.append($inputCellPhone);
    $tdPIDNumber.append($inputPIDNumber);
    $tdFBINumber.append($inputFBINumber);
    $tdDCINumber.append($inputDCINumber);

    //Mock Ticket #30001 – New column in Names grid
    $tdComments.append($inputComments);
    //$tdComments.append($txtComments);
    //$tdComments.append($btnEditComments);

    $tdSubjectCharged.append($inputSubjectCharged);
    $tdTrialDate.append($inputTrialDate);
    $tdGrandJuryDate.append($inputGrandJuryDate);
    $tdStatus.append($fbdivStatus);
    $tdAlive.append($fbdivAlive);
    $tdBleeding.append($fbdivBleeding);
    $tdTransfused.append($fbdivTransfused);

    $inputArrestDate.mask("99/99/99?99");
    $inputDeleteNameRow.text("X");
    $inputDeleteNameRow.bind("click", fnDeleteNameRow); 

    if (GetLabCtrl("QC_NAME_ID_MASK") != "")
        $inputNameID.mask(GetLabCtrl("QC_NAME_ID_MASK"));
    if (GetLabCtrl("QC_HOME_PHONE_MASK") != "")
        $inputHomePhone.mask(GetLabCtrl("QC_HOME_PHONE_MASK"));
    if (GetLabCtrl("QC_CELL_PHONE_MASK") != "")
        $inputCellPhone.mask(GetLabCtrl("QC_CELL_PHONE_MASK"));

    // Initialize flexboxes.
    $fbdivNameType.flexbox("PLCWebCommon/Results.aspx?t=TV_NAMETYPE&f=", { method: 'POST', initialValue: '', initialText: '', width: 100, descFormat: '', descSeparator: '', usesArrowNavigation: true, attachPopupTo: "body" });
    $fbdivSex.flexbox("PLCWebCommon/Results.aspx?t=TV_SEXCODE&f=", { method: 'POST', initialValue: '', initialText: '', width: 100, descFormat: '', descSeparator: '', usesArrowNavigation: true, attachPopupTo: "body" });
    $fbdivRace.flexbox("PLCWebCommon/Results.aspx?t=TV_RACECODE&f=", { method: 'POST', initialValue: '', initialText: '', width: 125, descFormat: '', descSeparator: '', usesArrowNavigation: true, attachPopupTo: "body" });
    $fbdivRefType.flexbox("PLCWebCommon/Results.aspx?t=TV_REFTYPE&f=", { method: 'POST', initialValue: '', initialText: '', width: 85, descFormat: '', descSeparator: '', usesArrowNavigation: true, attachPopupTo: "body", valueChanged: function (obj) { checkNameRefTypeRequired(obj) } });
    $fbdivStatus.flexbox("PLCWebCommon/Results.aspx?t=TV_NAMESTAT&f=", { method: 'POST', initialValue: '', initialText: '', width: 85, descFormat: '', descSeparator: '', usesArrowNavigation: true, attachPopupTo: "body", valueChanged: function (obj) { checkQCAssignmentPriority(obj) } });
    $fbdivAlive.flexbox("PLCWebCommon/Results.aspx?t=TV_MEDSTATUSOPTIONS&f=", { method: 'POST', initialValue: '', initialText: '', width: 125, descFormat: '', descSeparator: '', usesArrowNavigation: true, attachPopupTo: "body" });
    $fbdivBleeding.flexbox("PLCWebCommon/Results.aspx?t=TV_MEDSTATUSOPTIONS&f=", { method: 'POST', initialValue: '', initialText: '', width: 125, descFormat: '', descSeparator: '', usesArrowNavigation: true, attachPopupTo: "body" });
    $fbdivTransfused.flexbox("PLCWebCommon/Results.aspx?t=TV_MEDSTATUSOPTIONS&f=", { method: 'POST', initialValue: '', initialText: '', width: 125, descFormat: '', descSeparator: '', usesArrowNavigation: true, attachPopupTo: "body" });

    BindDateValidation($inputArrestDate);

    // Auto-fill in age when date of birth is entered.
    BindSetAgeJuvenile($inputDOB, $inputAge, $inputJuvenile);

    BindNamesCleared($fbdivNameType, $inputLast, istr - 1);

    
    BindFieldMask($inputNameID,GetLabCtrl("QC_NAME_ID_MASK"), "QC_NAME_ID");
    BindFieldMask($inputHomePhone,GetLabCtrl("QC_HOME_PHONE_MASK"),"QC_HOME_PHONE");
    BindFieldMask($inputCellPhone,GetLabCtrl("QC_CELL_PHONE_MASK"), "QC_CELL_PHONE");

    // Arrow key navigation
    BindNavigateToCell($fbdivNameType.find("input[type='text']"), true);
    BindNavigateToCell($fbdivSex.find("input[type='text']"), true);
    BindNavigateToCell($fbdivRace.find("input[type='text']"), true);
    BindNavigateToCell($fbdivRefType.find("input[type='text']"), true);
    BindNavigateToCell($inputArrestNumber);
    BindNavigateToCell($inputLast);
    BindNavigateToCell($inputFirst);
    BindNavigateToCell($inputMiddle);
    BindNavigateToCell($inputDOB);
    BindNavigateToCell($inputAge);
    BindNavigateToCell($inputArrestDate);
    BindNavigateToCell($inputJuvenile);
    BindNavigateToCell($stateID);
    BindNavigateToCell($inputRefText);
    BindNavigateToCell($inputNameID);
    BindNavigateToCell($inputHomePhone);
    BindNavigateToCell($inputCellPhone);
    BindNavigateToCell($inputPIDNumber);
    BindNavigateToCell($inputFBINumber);
    
    // <tr><td></td>...</tr>
    $tr.append($tdNameType);
    $tr.append($tdLast);
    $tr.append($tdFirst);
    $tr.append($tdMiddle);
    $tr.append($tdSuffix);
    $tr.append($tdSex);
    $tr.append($tdRace);
    $tr.append($tdDOB);
    $tr.append($tdAge);
    $tr.append($tdArrestDate);
    $tr.append($tdJuvenile);
    $tr.append($tdArrestNumber);
    $tr.append($tdStateID);
    $tr.append($tdReference);
    $tr.append($tdOldReference);
    $tr.append($tdoldnametype);
    $tr.append($tdoldlname);
    $tr.append($tdoldfname);
    $tr.append($tdoldmname);
    $tr.append($tdoldsex);
    $tr.append($tdoldrace);
    $tr.append($tdAddress);
    $tr.append($tdNameID);
    $tr.append($tdHomePhone);
    $tr.append($tdCellPhone);
    $tr.append($tdPIDNumber);
    $tr.append($tdFBINumber);
    $tr.append($tdDCINumber);

    //Mock Ticket #30001 – New column in Names grid
    $tr.append($tdComments);

    $tr.append($tdSubjectCharged);
    $tr.append($tdTrialDate);
    $tr.append($tdGrandJuryDate);
    $tr.append($tdStatus);
    $tr.append($tdAlive);
    $tr.append($tdBleeding);
    $tr.append($tdTransfused);

    // <table><tr></tr>...</table>
    $tbl.append($tr);

    // Hide input fields.
    if (!IsLabCtrlSet("USES_QC_ARREST_NUMBER")) {
        HideColumn($tdArrestNumber);
    }
    if (IsLabCtrlSet("HIDE_RACE_IN_QC"))
        HideColumn($tdRace);
    if (!IsLabCtrlSet("USES_QC_NAME_AGE"))
        HideColumn($tdAge);
    if (!IsLabCtrlSet("USE_ARREST_DATE"))
        HideColumn($tdArrestDate);
    if (!IsLabCtrlSet("QC_SHOW_JUVENILE"))
        HideColumn($tdJuvenile);
    if (GetLabCtrl("QC_STATE_ID_LABEL") == "")
        HideColumn($tdStateID);
    if (GetLabCtrl("USES_QC_NAME_REFERENCE") != "T")
        HideColumn($tdReference);
    if (GetLabCtrl("QC_NAMES_ADDRESS_BOOK") != "T")
        HideColumn($tdAddress);
    if (!IsLabCtrlSet("QC_NAME_ID"))
        HideColumn($tdNameID);
    if (!IsLabCtrlSet("QC_HOME_PHONE"))
        HideColumn($tdHomePhone);
    if (!IsLabCtrlSet("QC_CELL_PHONE"))
        HideColumn($tdCellPhone);
    if (!IsLabCtrlSet("USES_NAME_SUFFIX"))
        HideColumn($tdSuffix);
    if (!IsLabCtrlSet("USES_MEDSTATUS_OPTIONS")) {
        HideColumn($tdAlive);
        HideColumn($tdBleeding);
        HideColumn($tdTransfused);
    }

    if (!IsLabCtrlSet("USES_FBI_NUMBER"))
        HideColumn($tdFBINumber);
    if (!IsLabCtrlSet("USES_DCI_NUMBER"))
        HideColumn($tdDCINumber);

    //Mock Ticket #30001 – New column in Names grid
    if (GetLabCtrl("USES_QC_COMMENTS") !== "T" && GetLabCtrl("USES_QC_COMMENTS") !== "R")
        HideColumn($tdComments);
    //if (!IsUserOptionSet("QCNAMECOM"))
    //    HideColumn($tdComments);

    if (!IsLabCtrlSet("USES_QC_STATUS"))
        HideColumn($tdStatus);
    if (!IsLabCtrlSet("USES_QC_SUBJECT_CHARGED"))
        HideColumn($tdSubjectCharged);
    if (!IsLabCtrlSet("USES_QC_TRIAL_DATE"))
        HideColumn($tdTrialDate);
    if (!IsLabCtrlSet("USES_QC_GRAND_JURY_DATE"))
        HideColumn($tdGrandJuryDate);

    $tdNameType.css("display", "inline-flex");
    $tdNameType.prepend("&nbsp;");
    $tdNameType.prepend($inputDeleteNameRow);

    appendDOBDatePicker($inputDOB, "maskNameDOB" + istr);
    appendDateControl($inputTrialDate, "maskNameTrialDate" + istr);
    appendDateControl($inputGrandJuryDate, "maskNameGrandJuryDate" + istr);

    //Mock Ticket #30001 – New column in Names grid
    //initExpandTextBox($inputComments, $txtComments, $btnEditComments, "Comment");
    $inputRefText.bind("change", checkNameRefTextRequired);

    HideColumn($tdoldnametype);
    HideColumn($tdoldlname);
    HideColumn($tdoldfname);
    HideColumn($tdoldmname);
    HideColumn($tdoldsex);
    HideColumn($tdoldrace);
    HideColumn($tdPIDNumber);

    // Set values.
    if (setClear) {
        SetClientField($inputNameKey.attr("id"), "");
        SetClientField($inputDeptNameKey.attr("id"), "");
        SetClientField($stateID.attr("id"), "");
        SetClientField($inputArrestNumber.attr("id"), "");
        SetClientField($fbdivNameType.attr("id"), "");
        SetClientField($inputLast.attr("id"), "");
        SetClientField($inputFirst.attr("id"), "");
        SetClientField($inputMiddle.attr("id"), "");
        SetClientField($fbdivSex.attr("id"), "");
        SetClientField($fbdivRace.attr("id"), "");
        SetClientField($inputDOB.attr("id"), "");
        SetClientField($inputAge.attr("id"), "");
        SetClientField($inputArrestDate.attr("id"), "");
        SetClientField($inputJuvenile.attr("id"), "");
        SetClientField($fbdivRefType.attr("id"), "");
        SetClientField($inputRefText.attr("id"), "");
        SetClientField($oldRefType.attr("id"), "");
        SetClientField($oldRefText.attr("id"), "");
        SetClientField($oldnametype.attr("id"), "");
        SetClientField($oldlname.attr("id"), "");
        SetClientField($oldfname.attr("id"), "");
        SetClientField($oldmname.attr("id"), "");
        SetClientField($oldsex.attr("id"), "");
        SetClientField($oldrace.attr("id"), "");
        SetClientField($inputNameID.attr("id"), "");
        SetClientField($inputHomePhone.attr("id"), "");
        SetClientField($inputCellPhone.attr("id"), "");
        SetClientField($inputPIDNumber.attr("id"), "");
        SetClientField($inputFBINumber.attr("id"), "");
        SetClientField($inputSuffix.attr("id"), "");      
        SetClientField($inputDCINumber.attr("id"), "");
        SetClientField($fbdivAlive.attr("id"), "");
        SetClientField($fbdivBleeding.attr("id"), "");
        SetClientField($fbdivTransfused.attr("id"), "");
    }
    else {
        SetClientField($inputNameKey.attr("id"), row.NAMEKEY);
        SetClientField($inputDeptNameKey.attr("id"), row.DEPTNAMEKEY);
        SetClientField($stateID.attr("id"), row.STATEID);
        SetClientField($inputArrestNumber.attr("id"), row.ARRESTNUMBER);
        SetClientField($fbdivNameType.attr("id"), row.NAMETYPE);
        SetClientField($inputLast.attr("id"), row.LAST);
        SetClientField($inputFirst.attr("id"), row.FIRST);
        SetClientField($inputMiddle.attr("id"), row.MIDDLE);
        SetClientField($fbdivSex.attr("id"), row.SEX);
        SetClientField($fbdivRace.attr("id"), row.RACE);
        SetClientField($inputDOB.attr("id"), isValidDateText(row.DOB) ? row.DOB : "");
        SetClientField($inputAge.attr("id"), row.AGE);
        SetClientField($inputArrestDate.attr("id"), isValidDateText(row.ARRESTDATE) ? row.ARRESTDATE : "");
        SetClientField($inputJuvenile.attr("id"), row.JUVENILE);
        SetClientField($fbdivRefType.attr("id"), row.REFERENCE_TYPE);
        SetClientField($inputRefText.attr("id"), row.REFERENCE_TEXT);
        SetClientField($oldRefType.attr("id"), row.REFERENCE_TYPE);
        SetClientField($oldRefText.attr("id"), row.REFERENCE_TEXT);
        SetClientField($oldnametype.attr("id"), row.NAMETYPE);
        SetClientField($oldlname.attr("id"), row.LAST);
        SetClientField($oldfname.attr("id"), row.FIRST);
        SetClientField($oldmname.attr("id"), row.MIDDLE);
        SetClientField($oldsex.attr("id"), row.SEX);
        SetClientField($oldrace.attr("id"), row.RACE);
        SetClientField($inputNameID.attr("id"), row.NAMEID);
        SetClientField($inputHomePhone.attr("id"), row.HOMEPHONE);
        SetClientField($inputCellPhone.attr("id"), row.CELLPHONE);
        SetClientField($inputPIDNumber.attr("id"), row.PIDNUMBER);
        SetClientField($inputFBINumber.attr("id"), row.FBINUMBER);
        SetClientField($inputSuffix.attr("id"), row.SUFFIX);
        SetClientField($inputDCINumber.attr("id"), row.DCINUMBER);
        SetClientField($fbdivAlive.attr("id"), row.ALIVE);
        SetClientField($fbdivBleeding.attr("id"), row.BLEEDING);
        SetClientField($fbdivTransfused.attr("id"), row.TRANSFUSED);
    }
}