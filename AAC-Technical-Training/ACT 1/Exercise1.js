$(document).ready(() => {
    var selectedRow = null;
    var initialValues = {};

    ///Pop up dialog upon saving
    $("#dialog").dialog({
        autoOpen: false,
        modal: true,
        show: { effect: "fade", duration: 500 },
        hide: { effect: "fade", duration: 500 },
        closeOnEscape: true,
        draggable: false,
        resizable: false,
    });

    ///Date picker UI widget
    $('#incedent-report-date').datepicker();

    rowHighlight();

    ///Edit button to enable disabled inputs and buttons
    $('#btnEdit').click(function () {
        update();
    });

    //Save button to update table row data
    $('#btnSave').click(function () {
        saveUpdate();
    });

    ///Cancel button
    $('#btnCancel').click(function () {
        cancelUpdate();
    })

    function rowHighlight() {
        /// <summary>Highlight and display selected row data in input elements upon clicking.</summary>
        /// <param>None.</param>
        /// <return type = "void">Void.</return>
        $('.cases-table-body tr').click(function () {
            $('.cases-table-body tr').removeClass('table-row-color');
            $(this).addClass('table-row-color');

            selectedRow = $(this);

            var departmentCaseNumber = $(this).find('td:eq(0)').text();
            var departmentCase = $(this).find('td:eq(1)').text();
            var charge = $(this).find('td:eq(2)').text();
            var labCaseNumber = $(this).find('td:eq(3)').text();
            var incedentReportDate = $(this).find('td:eq(4)').text();

            $('#departmentCaseNumber').val(departmentCaseNumber);
            $('#departmentCase').val(departmentCase);
            $('#charge').val(charge);
            $('#labCaseNumber').val(labCaseNumber);
            $('#incedentReportDate').val(incedentReportDate);

            storeInitialValue();
        });
    }

    function update() {
        /// <summary>Enables input elements to update table row data.</summary>
        /// <param>None.</param>
        /// <return type = "void">Void.</return>
        if (selectedRow) {
            disableInputButtons(false, false, true);

            /// disable clicking on cases-table-row tr
            $('.cases-table-body tr').not(selectedRow).addClass('disabled');
        }
    }

    function saveUpdate() {
        /// <summary>Saves update from input elements.</summary>
        /// <param>None.</param>
        /// <return type = "void">Void.</return>        
        if (selectedRow) {
            var departmentCaseNumber = $('#departmentCaseNumber').val();
            var departmentCase = $('#departmentCase').val();
            var charge = $('#charge').val();
            var labCaseNumber = $('#labCaseNumber').val();
            var incedentReportDate = $('#incedentReportDate').val();

            selectedRow.find('td:eq(0)').text(departmentCaseNumber);
            selectedRow.find('td:eq(1)').text(departmentCase);
            selectedRow.find('td:eq(2)').text(charge);
            selectedRow.find('td:eq(3)').text(labCaseNumber);
            selectedRow.find('td:eq(4)').text(incedentReportDate);
        }

        storeInitialValue();
        disableInputButtons(true, true, false);
        dlgMessage();
        enableClick();
    }

    function cancelUpdate() {
        /// <summary>Cancels update then restores initial value.</summary>
        /// <param>None.</param>
        /// <return type = "void">Void.</return>
        restoreInitialValue();
        disableInputButtons(true, true, false);
        enableClick();
    }

    function dlgMessage() {
        /// <summary>Dialog auto close timer - 1 second.</summary>
        /// <param>None.</param>
        /// <return type = "void">Void.</return>
        $("#dialog").dialog("open");
        setTimeout(function () {
            $("#dialog").dialog("close");
        }, 1000);
    }


    function storeInitialValue() {
        /// <summary>Stores initial row value.</summary>
        /// <param>None.</param>
        /// <return type = "void">Void.</return>
        $('.form input').each(function () {
            var id = $(this).attr('id');
            initialValues[id] = $(this).val();
        });
    }

    function restoreInitialValue() {
        /// <summary>Restores initial row value.</summary>
        /// <param>None.</param>
        /// <return type = "void">Void.</return>
        $('.form input').each(function () {
            var id = $(this).attr('id');
            $(this).val(initialValues[id]);
        });
    }

    function disableInputButtons(boolButton, boolInput, boolEdit) {
        /// <summary>Disables input and button elements.</summary>
        /// <param names = "boolButton", "boolInput", "boolEdit" types = "bool", "bool", "bool">Boolean parameters.</param>
        /// <return type = "void">Void</return>
        $("button").prop("disabled", boolButton);
        $("input").prop("disabled", boolInput);
        $('#btnEdit').prop('disabled', boolEdit);
    }

    function enableClick() {
        /// <summary>Enable clicking on table body rows.</summary>
        /// <param>None.</param>
        /// <return type = "void">Void.</return>
        $('.cases-table-body tr').not(selectedRow).removeClass('disabled');
    }
});