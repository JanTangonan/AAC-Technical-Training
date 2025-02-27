//V1
// Extend CreateNoteControls to support new tab logic
function CreateNoteControls(pobjDetails, pstrGroupRes) {
    var strControls = "";

    if (pobjDetails != null) {
        for (var i = 0; i < pobjDetails.length; i++) {
            strControls += "<div style='margin-bottom: 6px'>";
            strControls += CreateNoteControl(pobjDetails[i], pstrGroupRes);
            strControls += "</div>";
        }
    }
    return strControls;
}

// Modify CreateNoteControl to support reference-based dynamic updates
function CreateNoteControl(pobjDetail, pstrGroupRes) {
    var strControl = "";
    var strControlId = CreateControlId(pobjDetail.StatCode.replace(/'/g, '&#39;'));
    
    if (pobjDetail.ReferenceControl) {
        strControl += `<input type='text' id='${strControlId}' value='${pobjDetail.LoadedAnswer || ''}' disabled>`;
        strControl += `<script>
            var refControls = '${pobjDetail.ReferenceControl}'.split(',');
            refControls.forEach(ref => {
                $('#'+ref).on('input', function() {
                    var first = $('#ans1st').val() || '';
                    var second = $('#ans2nd').val() || '';
                    $('#${strControlId}').val(first + second + first);
                });
            });
        </script>`;
    } else {
        strControl += CreateTextbox(strControlId, pobjDetail.Description, pobjDetail.LoadedAnswer, pobjDetail.IsRequired);
    }
    return strControl;
}

//V2
// Extend CreateNoteControls to support new tab logic
function CreateNoteControls(pobjDetails) {
    var strControls = "";

    if (pobjDetails != null) {
        for (var i = 0; i < pobjDetails.length; i++) {
            strControls += "<div style='margin-bottom: 6px'>";
            strControls += CreateNoteControl(pobjDetails[i]);
            strControls += "</div>";
        }
    }
    return strControls;
}

// Modify CreateNoteControl to support reference-based dynamic updates
function CreateNoteControl(pobjDetail) {
    var strControl = "";
    var strControlId = CreateControlId(pobjDetail.StatCode.replace(/'/g, '&#39;'));
    var rfcControls = pobjDetail.ReferenceControl ? pobjDetail.ReferenceControl.split(',') : [];
    
    strControl += CreateTextbox(strControlId, pobjDetail.Description, pobjDetail.LoadedAnswer, pobjDetail.IsRequired, rfcControls);
    return strControl;
}

// Modify CreateTextbox to apply reference control logic
function CreateTextbox(pstrId, pstrDescription, pstrAnswer, pblnRequired, referenceControl) {
    var strTextbox = CreateLabel(pstrId, pstrDescription, pblnRequired);

    strTextbox += `<input type='text' id='${pstrId}'`;
    if (referenceControl.length > 0) strTextbox += " disabled";
    strTextbox += ` value='${pstrAnswer || ''}'>`;

    if (referenceControl.length > 0) {
        strTextbox += `<script>
            var refs = ${JSON.stringify(referenceControl)};
            refs.forEach(ref => {
                $('#' + ref).on('input', function() {
                    var combinedValue = refs.map(id => $('#' + id).val() || '').join('');
                    $('#${pstrId}').val(combinedValue);
                });
            });
        </script>`;
    }
    return strTextbox;
}

//V3
// Function to create a textbox control
function CreateTextbox(pstrId, pstrDescription, pstrAnswer, pblnRequired, statDefaultValue, statPictureMask, referenceControl, enableControl) {
    var strTextbox = CreateLabel(pstrId, pstrDescription, pblnRequired);

    // Convert referenceControl to an array if it's not null
    var refControls = referenceControl ? referenceControl.split(',').map(s => s.trim()) : [];

    strTextbox += `<input type='text' id='${pstrId}'`;
    
    // If the control should be disabled, apply the disabled attribute
    if (enableControl === "F" || refControls.length > 0) {
        strTextbox += " disabled";
    }

    // Set required field class
    if (pblnRequired) {
        strTextbox += " class='required'";
    }

    // Set barcode scanning attribute
    strTextbox += " barcodescanhere='' onkeyup='DenyEnterKey();'";

    // Apply loaded answer or default value
    var isSetToUpperCase = statPictureMask && statPictureMask.indexOf("X") > -1;
    if (pstrAnswer) {
        if (isSetToUpperCase) pstrAnswer = pstrAnswer.toUpperCase();
        strTextbox += ` value='${pstrAnswer}'`;
    } else if (statDefaultValue) {
        if (isSetToUpperCase) statDefaultValue = statDefaultValue.toUpperCase();
        strTextbox += ` value='${statDefaultValue}'`;
    }
    
    // Apply stat mask if available
    if (statPictureMask) {
        strTextbox += ` data-stat-mask='${statPictureMask}'`;
    }
    
    strTextbox += "> <br/>";

    // Append script separately to avoid HTML issues
    if (refControls.length > 0) {
        setTimeout(() => {
            refControls.forEach(ref => {
                $('#' + ref).on('input', function() {
                    var combinedValue = refControls.map(id => $('#' + id).val() || '').join('');
                    $('#' + pstrId).val(combinedValue);
                });
            });
        }, 100);
    }
    
    return strTextbox;
}

//V4
// creates a textbox control
function CreateTextbox(pstrId, pstrDescription, pstrAnswer, pblnRequired, statDefaultValue, statPictureMask, referenceControl, enableControl) {
    var strTextbox = CreateLabel(pstrId, pstrDescription, pblnRequired);

    var refControls = referenceControl ? referenceControl.split(',').map(s => 'ans' + s.trim()) : [];
    var isReadonly = enableControl === "F";
    var readonlyClass = isReadonly ? "readonly-textbox" : "";

    strTextbox += `<input type='text' id='${pstrId}' class='${readonlyClass}'`;

    if (isReadonly) {
        strTextbox += " readonly"; // Use readonly instead of disabled to allow JS updates
    }

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

    console.log("REFERENCE CONTROLS: " + referenceControl);
    console.log("REFERENCE CONTROLS: " + refControls);

    if (statPictureMask) {
        strTextbox += " data-stat-mask='" + statPictureMask + "'";
    }

    strTextbox += "> <br/>";

    // Append script separately to avoid HTML issues
    if (refControls.length > 0) {
        setTimeout(() => {
            refControls.forEach(ref => {
                $('#' + ref).on('input', function() {
                    var combinedValue = refControls.map(id => $('#' + id).val() || '').join('');
                    $('#' + pstrId).val(combinedValue);
                });
            });
        }, 100);
    }

    console.log(strTextbox);
    return strTextbox;
}


//
if (refControls.length > 0) {
    let refSelector = refControls.map(id => `#${id}`).join(',');

    setTimeout(() => {
        $(document).on('input', refSelector, function () {
            let combinedValue = refControls.map(id => $('#' + id).val() || '').join('');
            console.log(`Updating ${pstrId} with combined value:`, combinedValue);
            $('#' + pstrId).val(combinedValue).trigger('change').trigger('input');;
        });
    }, 100);
}

//
if (refControls.length > 0) {
    setTimeout(() => {
        $(document).on('input', refControls.map(id => `#${id}`).join(','), function() {
            var combinedValue = refControls.map(id => $('#' + id).val() || '').join('');
            console.log(`Updating ${pstrId} with value:`, combinedValue);
            $('#' + pstrId).val(combinedValue).trigger('change').trigger('input');
        });
    }, 100);
}
