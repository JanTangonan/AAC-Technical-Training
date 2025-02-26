function GetCustomNotes() {
    // show a dialog box
    StartModalProgressDialog("Rendering Tabs...");

    var params = { "SECTION": $("[id$='hdnSection']").val(), "CASE_KEY": $("[id$='hdnCaseKey']").val(), "EXAM_KEY": $("[id$='hdnNotesExamKey']").val() };
    $.ajax({ type: "POST", url: '<%= ResolveUrl("~/PLCWebCommon/PLCWebMethods.asmx/GetExaminationNoteFields") %>', data: params, success: RenderNotes, error: function () { }, dataType: "json" });
}

// renders the tabs and contents for the custom notes
function RenderNotes(plstNotes) {

    if (plstNotes.length != 0) {
        // create and add the tab headers
        $('#divNotePlaceholder').append(CreateTabHeaders(plstNotes));

        // update the tab header widths
        UpdateHeaderTabWidth(plstNotes);

        // create and add the tab bodies
        $('#divNotePlaceholder').append(CreateTabContents(plstNotes));

        // apply mask to controls
        AddMask();

        // updates the length of the label based on the longest one
        UpdateContentWidth(plstNotes);

        // store the loaded notes into a hidden field
        $("[id$='hdnNotes']").val(JSON.stringify(plstNotes));

        // we need to initialize the rich text area controls (if any);
        InitializeRichTextArea();

        if ($("[id$='hdnCompleted']").val() == "T" && $("[id$='hdnEditAppr']").val() != "T")
        {
            document.getElementById('<%=btnSave.ClientID%>').disabled = true;
            document.getElementById('<%=btnPrint.ClientID%>').disabled = true;
        }
    }
    else {
        // create and add the tab headers
        $('#divNotePlaceholder').append("<h3>No notes setup for the specified section.</h3>");
        document.getElementById('<%=btnSave.ClientID%>').disabled = true;
        document.getElementById('<%=btnPrint.ClientID%>').disabled = true;
    }

    // close the dialog
    CloseModalProgressDialog();
}

// creates the tab body for each notes
function CreateTabContents(plstNotes) {
    var strBody = "<div class='tab_container' style='padding: 15px;'><div>";

    // create the contents for each notes
    for (var i = 0; i < plstNotes.length; i++) {
        if (i == 0) {
            strBody += "<div id='divContent" + i + "' class='customContents' style='margin-bottom: 15px; margin-top: 15px; min-height: 450px;'>";
        } else {
            strBody += "<div id='divContent" + i + "' class='customContents' style='display: none; margin-bottom: 15px; margin-top: 15px; min-height: 450px;'>";
        }

        strBody += CreateNoteControls(plstNotes[i].Details);

        strBody += "</div>";
    }

    strBody += "</div></div>";

    return strBody;
}

// create the controls for each note details
function CreateNoteControls(pobjDetails) {
    var strControls = "";

    if (pobjDetails != null) {
        // loop through each note detail
        for (var i = 0; i < pobjDetails.length; i++) {
            strControls += "<div style='margin-bottom: 6px'>";
            strControls += CreateNoteControl(pobjDetails[i]);
            strControls += "</div>";
        }
    }

    return strControls;
}

// creates the appropriate control based on the given details
function CreateNoteControl(pobjDetail) {
    console.log(pobjDetail);
    var strControl = "";
    var strControlId = CreateControlId((pobjDetail.StatCode).replace(/'/g, '&#39;'));

    // check what kind of control we need to create
    if (pobjDetail.IsChecklist) {

    } else if (pobjDetail.IsCombobox) {
        strControl += CreateCombobox(strControlId, pobjDetail.Description, pobjDetail.ListOptions, pobjDetail.LoadedAnswer, pobjDetail.IsRequired);
    } else if (pobjDetail.IsRichTextArea) {
        strControl += CreateRichTextarea(strControlId, pobjDetail.Description, pobjDetail.LoadedAnswer);
    } else if (pobjDetail.IsChemicalUseBC) {
        strControl += CreateChemicalBarcodeTextbox(strControlId, pobjDetail.Description);
        strControl += "<br/><br/>";
        strControl += CreateChemicalBarcodeTable(strControlId, pobjDetail.ChemicalsUsed);
    } else if (pobjDetail.IsChemicalUseTable) {
        
    } else {
        strControl += CreateTextbox(strControlId, pobjDetail.Description, pobjDetail.LoadedAnswer, pobjDetail.IsRequired, pobjDetail.StatDefaultValue, pobjDetail.StatPictureMask);
    }

    return strControl;
}

// create a control id for the controls to use
function CreateControlId(pstrId) {
    var strControlId = "ans" + pstrId;

    return strControlId;
}

// creates a textbox control
function CreateTextbox(pstrId, pstrDescription, pstrAnswer, pblnRequired, statDefaultValue, statPictureMask) {
    var strTextbox = CreateLabel(pstrId, pstrDescription, pblnRequired);

    strTextbox += "<input type='text' id='" + pstrId + "'";

    if ($("[id$='hdnCompleted']").val() == "T" && $("[id$='hdnEditAppr']").val() != "T") 
        strTextbox += " disabled ";

    if (pblnRequired)
        strTextbox += " class='required' ";
        
    strTextbox += " barcodescanhere='' onkeyup='DenyEnterKey();' ";

    var isSetToUpperCase = statPictureMask && statPictureMask.indexOf("X") > -1;
    
    // check if an answer has been saved
    if (pstrAnswer != "") {
        if (isSetToUpperCase) pstrAnswer = pstrAnswer.toUpperCase();
        strTextbox += " value='" + pstrAnswer + "'";
    } else {
        // if there is no saved answer, set default value if there is any
        console.log(statDefaultValue);
        if (statDefaultValue) {
            if (isSetToUpperCase) statDefaultValue = statDefaultValue.toUpperCase();
            strTextbox += " value='" + statDefaultValue + "'";
        }
    }

    if (statPictureMask) {
        strTextbox += " data-stat-mask='" + statPictureMask + "'";
    }

    strTextbox += "> <br/>";

    return strTextbox;
}