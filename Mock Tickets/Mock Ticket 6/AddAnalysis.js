//#region AssignmentReport.aspx
$(function () {
    $("#mdialog-addanalysis").dialog({
        autoOpen: false,
        modal: true,
        resizable: false,
        draggable: false,
        title: 'Add Analysis Type',
        width: 450,
        //height: 500,
        buttons: {
            OK: function () {
                $(this).dialog("close");
                // AddAnalysisType(<%=IsReadOnly.ToString().ToLower()%>, false);
            },
            Cancel: function () {
                $(this).dialog("close");
            }
        }
    });
    $("#mdialog-annfontsize").dialog({
        autoOpen: false,
        modal: true,
        resizable: false,
        draggable: false,
        title: 'Change Annotation Font Size',
        width: 400,
        height: 'auto',
        buttons: {
            OK: function () {
                ChangeAnnotationFontSize();
            },
            Cancel: function () {
                $(this).dialog("close");
            }
        }
    });
    $("#mdialog-verify").dialog({
        autoOpen: false,
        modal: true,
        resizable: false,
        draggable: false,
        title: 'Confirm',
        width: 450,
        height: 'auto',
        buttons: {
            Confirm: function () {
                CheckPassword();
            },
            Cancel: function () {
                $(this).dialog("close");
            }
        }
    });
    $("#mdialog-matrixsample").dialog({
        autoOpen: false,
        modal: true,
        resizable: false,
        draggable: true
    });
    $("#mdialog-kititems").dialog({
        autoOpen: false,
        modal: true,
        resizable: false,
        draggable: false
    });
    $("#mdialog-addnames").dialog({
        autoOpen: false,
        modal: true,
        resizable: false,
        draggable: false,
        title: 'Add Names',
        width: 950,
        //height: 500,
        buttons: {
            Save: function () {
                $(this).dialog("close");
                SaveNames();
            },
            Cancel: function () {
                $(this).dialog("close");
            }
        }
    });
    $("#mdialog-testtypes").dialog({
        autoOpen: false,
        modal: true,
        resizable: false,
        draggable: false,
        title: 'Test Types',
        width: 450,
        height: 350,
        buttons: {
            Save: function () {
                AddEditDelTestTypeVal("");
            },
            Delete: function () {
                AddEditDelTestTypeVal("DELETE");
            },
            Cancel: function () {
                $(this).dialog("close");
            }
        }
    });
    $("#mdialog-selectitems").dialog({
        autoOpen: false,
        modal: true,
        resizable: false,
        draggable: false,
        title: 'Select Items/Entries',
        dialogClass: 'dialog-scroll',
        width: 500,
        buttons: {
            Overwrite: function () {
                OverwriteItemEntries($(this));
            },
            Append: function () {
                AppendItemEntries($(this));
            },
            'Cut and Paste': function () {
                CutAndPasteItemEntries($(this));
            },
            Cancel: function () {
                $(this).dialog("close");
                $("#divDupeItems tr").find(':checkbox').prop('checked', false);
                $(".copyentries.activeCE").removeClass("activeCE");
            }
        }
    });
    $("#mdialog-editbatchvalidate").dialog({
        autoOpen: false,
        modal: true,
        resizable: false,
        draggable: false,
        title: 'Confirm',
        width: 300,
        height: 'auto',
        buttons: {
            Confirm: function () {
                EditBatchValidateUser();
            },
            Cancel: function () {
                $(this).dialog("close");
            }
        }
    });
    $("#dialog-form").dialog({
        autoOpen: false,
        modal: true,
        resizable: false,
        draggable: false
    });
    $("#mdialog").dialog({
        autoOpen: false,
        modal: true,
        resizable: false,
        draggable: false,
        buttons: {
            OK: function () {
                $(this).dialog("close");
            }
        }
    });
    $("#malert").dialog({
        autoOpen: false,
        modal: true,
        resizable: false,
        draggable: false,
        title: 'Alert',
        width: 300,
        height: 'auto',
        buttons: {
            OK: function () {
                $(this).dialog("close");
            }
        }
    });
    $("#mconfirm").dialog({
        autoOpen: false,
        modal: true,
        resizable: false,
        draggable: false,
        buttons: {
            OK: function () {
                $(this).dialog("close");
            }
        }
    });
    $("#mdialog-rptattachments").dialog({
        autoOpen: false,
        modal: true,
        resizable: false,
        draggable: false,
        title: $("[id$='hdnLabCase']").val(),
        width: 600,
        height: 500,
        buttons: {
            'Expand All': function () {
                treeViewExpandAll("<%=tvAttachments.ClientID%>");
            },
            'Collapse All': function () {
                treeViewCollapseAll("<%=tvAttachments.ClientID%>");
            },
            Save: function () {
                if($("[id*=tvAttachments] input[type=checkbox]").length <= 1)
                    return;
                
                $("[id*=tvAttachments] a").attr("disabled", true);
                $("[id*=tvAttachments] img").attr("disabled", true);
                $("#mdialog-rptattachments").attr("disabled", true);
                $("#mdialog-rptattachments").parent().find(".ui-button").attr("disabled", true);
                $(this).closest(".ui-dialog").find(".ui-dialog-buttonset").attr("disabled", "disabled");
                LinkAttachmentsToReport();
            },
            Close: function () {
                $(this).dialog("close");
            }
        },
        open: function () {
            $("[id*=tvAttachments] a").attr("disabled", false)
            $("[id*=tvAttachments] img").attr("disabled", false);
            $("#mdialog-rptattachments").attr("disabled", false);
            $("#mdialog-rptattachments").parent().find(".ui-button").attr("disabled", false);

            // Fix for Sys is undefined when opening the dialog(IE9)
            $(this).parent().appendTo("form");
            $overlay = $(".ui-widget-overlay");
            $(".ui-dialog-titlebar-close", $(this).parent()).hide(); // Remove close icon
            $("[id$='hdnAttachPopupShown']").val("T");
        },
        close: function () {
            $("[id$='hdnAttachPopupShown']").val("");
        }
    });
    $("#mdialog-spparam").dialog({
        autoOpen: false,
        modal: true,
        resizable: false,
        draggable: false,
        width: 400,
        height: 200
    });
});
//#endregion

//#region MatrixControlScript.js
/* Sort Analysis Panels */
function SortAnalysisPanels(verification) {
    var fnPost = function () {
        var params = { verification: verification };
        BEASTiLIMS.MatrixControl.WSMatrix.RenderSortAnalysisTypes(params, GetPanels_OK, GetPanels_Error);
    };

    SaveMatrixWithDelay(false, fnPost);
}

function OpenAddAnalysis() {
    SaveMatrixWithDelay(false);
    var position = { my: "center", at: "center", of: window };
    $('#mdialog-addanalysis').dialog('option', 'position', position);
    $('#mdialog-addanalysis').dialog("open");
    $("input[id*='fbSection_input']").change();

    if ($('#satc .atcgridrow').hasClass("atcselectedgridrow"))
        $('#satc .atcgridrow').removeClass("atcselectedgridrow").addClass("atcgridrow");
    $("#satc input[id*='chkAnalysis']:checked").prop("checked", false);
}

//#endregion