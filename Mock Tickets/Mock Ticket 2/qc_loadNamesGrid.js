function LoadNamesGrid(names, setClear) {
    var defaultGridSize = parseInt(names.length);

    if (defaultGridSize > 20) {
        _emptyNames = new Array(defaultGridSize + 10);
        for (var i = 0; i < 20; i++) {
            var dictNameRow = { "NAMEKEY": "", "DEPTNAMEKEY": "", "ARRESTNUMBER": "", "NAMETYPE": "", "LAST": "", "FIRST": "", "MIDDLE": "", "SEX": "", "RACE": "", "DOB": "", "AGE": "", "ARRESTDATE": "", "STATEID": "", "JUVENILE": "", "REFERENCE_TYPE": "", "REFERENCE_TEXT": "", "NAMEID": "", "HOMEPHONE": "", "CELLPHONE": "", "PIDNUMBER": "", "FBINUMBER": "", "ALIVE": "", "BLEEDING": "", "TRANSFUSED": "" };
            _emptyNames[i] = dictNameRow;
        }
    }
    var defaultNameRefType = GetLabCtrl("QC_NAME_REF_TYPE");
    CreateNamesGrid(_emptyNames, true);

    var dateformat = GetLabCtrl("SHORT_DATE_FORMAT");

    for (var i=0; i < names.length; i++) {
        var row = names[i];

        var istr = (i+1).toString();
        var $inputNameKey       = $("input#NAME_NAMEKEY" + istr);
        var $inputDeptNameKey   = $("input#NAME_DEPTNAMEKEY" + istr);
        var $stateID            = $("input#NAME_STATEID"+istr);
        var $inputArrestNumber  = $("input#NAME_ARRESTNUMBER"+istr);
        var $fbdivNameType      = $("div#NAME_NAMETYPE_fbLookup"+istr);
        var $inputLast          = $("input#NAME_LAST"+istr);
        var $inputFirst         = $("input#NAME_FIRST"+istr);
        var $inputMiddle        = $("input#NAME_MIDDLE"+istr);
        var $fbdivSex           = $("div#NAME_SEX_fbLookup"+istr);
        var $fbdivRace          = $("div#NAME_RACE_fbLookup"+istr);
        var $inputDOB           = $("input#NAME_DOB"+istr);
        var $inputAge           = $("input#NAME_AGE"+istr);
        var $inputArrestDate    = $("input#NAME_ARRESTDATE" + istr);
        var $inputJuvenile      = $("input#NAME_JUVENILE_cb" + istr);
        var $fbdivRefType       = $("div#NAME_REFERENCETYPE_fbLookup" + istr);
        var $inputRefText       = $("input#NAME_REFERENCETEXT" + istr);
        var $oldRefType         = $("input#NAME_OLDREFTYPE" + istr);
        var $oldRefText         = $("input#NAME_OLDREFTEXT" + istr);
        var offensedatetxt      = GetDBPanelField("PLCDBPanel1", "TV_LABCASE.OFFENSE_DATE");
        var $oldNameType        = $("input#NAME_OLDNAMETYPE" + istr);
        var $oldLName           = $("input#NAME_OLDLNAME" + istr);
        var $oldFName           = $("input#NAME_OLDFNAME" + istr);
        var $oldMName           = $("input#NAME_OLDMNAME" + istr);
        var $oldSex             = $("input#NAME_OLDSEX" + istr);
        var $oldRace            = $("input#NAME_OLDRACE" + istr);
        var $inputNameID        = $("input#NAME_NAMEID" + istr);
        var $inputHomePhone     = $("input#NAME_HOMEPHONE" + istr);
        var $inputCellPhone     = $("input#NAME_CELLPHONE" + istr);
        var $inputPIDnumber     = $("input#NAME_PIDNUMBER" + istr);
        var $inputFBINumber     = $("input#NAME_FBINUMBER" + istr);
        var $inputSuffix = $("input#NAME_SUFFIX" + istr);
        var $fbdivAlive = $("div#NAME_ALIVE_fbLookup" + istr);
        var $fbdivBleeding = $("div#NAME_BLEEDING_fbLookup" + istr);
        var $fbdivTransfused = $("div#NAME_TRANSFUSED_fbLookup" + istr);
        var $inputDCINumber = $("input#NAME_DCINUMBER" + istr);

        //Mock Ticket #30001 – New column in Names grid
        var $inputComments = $("input#NAME_COMMENTS" + istr);

        var $inputSubjectCharged= $("input#NAME_SUBJECT_CHARGED_cb" + istr);
        var $inputTrialDate     = $("input#NAME_TRIAL_DATE" + istr);
        var $inputGrandJuryDate = $("input#NAME_GRAND_JURY_DATE" + istr);
        var $fbdivStatus        = $("div#NAME_STATUS_fbLookup" + istr)
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
            SetClientField($inputJuvenile.attr("id"), false);
            SetClientField($fbdivRefType.attr("id"), "");
            SetClientField($inputRefText.attr("id"), "");
            SetClientField($oldRefType.attr("id"), "");
            SetClientField($oldRefText.attr("id"), "");
            SetClientField($oldNameType.attr("id"), "");
            SetClientField($oldLName.attr("id"), "");
            SetClientField($oldFName.attr("id"), "");
            SetClientField($oldMName.attr("id"), "");
            SetClientField($oldSex.attr("id"), "");
            SetClientField($oldRace.attr("id"), "");
            SetClientField($inputNameID.attr("id"), "");
            SetClientField($inputHomePhone.attr("id"), "");
            SetClientField($inputCellPhone.attr("id"), "");
            SetClientField($inputPIDnumber.attr("id"), "");
            SetClientField($inputFBINumber.attr("id"), "");
            SetClientField($inputSuffix.attr("id"), "");
            SetClientField($inputDCINumber.attr("id"), "");

            //Mock Ticket #30001 – New column in Names grid
            SetClientField($inputComments.attr("id"), "");

            SetClientField($inputSubjectCharged.attr("id"), "");
            SetClientField($inputTrialDate.attr("id"), "");
            SetClientField($inputGrandJuryDate.attr("id"), "");
            SetClientField($fbdivStatus.attr("id"), "");
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
            SetClientField($oldNameType.attr("id"), row.NAMETYPE);
            SetClientField($oldLName.attr("id"), row.LAST);
            SetClientField($oldFName.attr("id"), row.FIRST);
            SetClientField($oldMName.attr("id"), row.MIDDLE);
            SetClientField($oldSex.attr("id"), row.SEX);
            SetClientField($oldRace.attr("id"), row.RACE);
            SetClientField($inputNameID.attr("id"), row.NAMEID);
            SetClientField($inputHomePhone.attr("id"), row.HOMEPHONE);
            SetClientField($inputCellPhone.attr("id"), row.CELLPHONE);

            if (dateformat.toUpperCase() == "DD/MM/YYYY" && row.DOB != "") {
                SetClientField($inputDOB.attr("id"), isValidDateText(DMYtoMDY(row.DOB)) ? row.DOB : "");
            }
            else {
                SetClientField($inputDOB.attr("id"), isValidDateText(row.DOB) ? row.DOB : "");
            }
            SetClientField($inputJuvenile.attr("id"), isJuvenile(row.JUVENILE));
            SetClientField($fbdivRefType.attr("id"), IsDefined(row.REFERENCE_TYPE)
                ? row.REFERENCE_TYPE  
                : (row.NAMETYPE != "" && row.LAST != "")  ? defaultNameRefType : "");
            SetClientField($inputRefText.attr("id"), row.REFERENCE_TEXT);
            SetClientField($oldRefType.attr("id"), row.REFERENCE_TYPE);
            SetClientField($oldRefText.attr("id"), row.REFERENCE_TEXT);
            SetClientField($inputPIDnumber.attr("id"), row.PIDNUMBER);
            SetClientField($inputFBINumber.attr("id"), row.FBINUMBER);
            SetClientField($fbdivAlive.attr("id"), row.ALIVE);
            SetClientField($fbdivBleeding.attr("id"), row.BLEEDING);
            SetClientField($fbdivTransfused.attr("id"), row.TRANSFUSED);

            if (offensedatetxt == "")
                offensedatetxt = GetDBPanelField("PLCDBPanel1", "TV_LABSUB.OFFENSE_DATE");

            SetClientField($inputAge.attr("id"), ($.trim(row.DOB) != "" && $.trim(offensedatetxt) != "" &&  IsLabCtrlSet("USES_QC_NAME_AGE")) ?
                function () {
                    var age = getAge(row.DOB, offensedatetxt);

                    if (dateformat.toUpperCase() == "DD/MM/YYYY") {
                        age = getAge(DMYtoMDY(row.DOB), DMYtoMDY(offensedatetxt));
                        
                    }

                    if ((age > 0) && (age <= getOldestJuvenile()))  $inputJuvenile.prop("checked", true);

                    if (age > 0)
                        return age.toString();
                    else
                        return row.AGE;
                } : row.AGE);



            if (dateformat.toUpperCase() == "DD/MM/YYYY" && row.ARRESTDATE != "") {
                SetClientField($inputArrestDate.attr("id"), isValidDateText(DMYtoMDY(row.ARRESTDATE)) ? row.ARRESTDATE : "");
            }
            else {
                SetClientField($inputArrestDate.attr("id"), isValidDateText(row.ARRESTDATE) ? row.ARRESTDATE : "");
            }

            SetClientField($inputSuffix.attr("id"), row.SUFFIX);

            SetClientField($inputDCINumber.attr("id"), row.DCINUMBER);

            //Mock Ticket #30001 – New column in Names grid
            SetClientField($inputComments.attr("id"), row.COMMENTS);

            SetClientField($inputSubjectCharged.attr("id"), row.SUBJECT_CHARGED == "T" ? "true" : "false");
            if (row.TRIAL_DATE != "")
                SetClientField($inputTrialDate.attr("id"), isValidDateText(row.TRIAL_DATE) ? row.TRIAL_DATE : "");
            if (row.GRAND_JURY_DATE != "")
                SetClientField($inputGrandJuryDate.attr("id"), isValidDateText(row.GRAND_JURY_DATE) ? row.GRAND_JURY_DATE : "");
            SetClientField($fbdivStatus.attr("id"), row.STATUS);

            if (row.NAMESEQ) {
                $("input[id^='ITEM_NAMESimage'].has-linked").each(function () {
                    var names = $(this).data("names");
                    if (names && names.length) {
                        names.forEach(function (name) {
                            if (name.nameSeq == row.NAMESEQ)
                                name.index = i;
                        });
                        $(this).data("names", names);
                    }
                });
            }

            if (IsLabCtrlSet("USES_QC_NAME_REFERENCE") && $fbdivRefType.getCode() != "")
                checkNameRefTypeRequired($fbdivRefType);
        }
    }

    //_lastNameNum = names.length-1;
    _namesInitializtionDone = true;
}

var _namesInitializtionDone = false;
function BindNamesCleared($fbdivNameType, $inputLast, index) {
    var nameClearedHandler = function () {
        if (!$fbdivNameType.getCode() && !$inputLast.val().trim() && _namesInitializtionDone)
            RemoveItemNameLink({ index: index });
    };
    $inputLast.change(nameClearedHandler);
    $fbdivNameType.data("instanceopt").valueChanged = nameClearedHandler;
}