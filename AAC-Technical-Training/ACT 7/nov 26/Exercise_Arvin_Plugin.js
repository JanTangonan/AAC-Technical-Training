$(document).ready(function () {
    var isFirstLoad = true;
    var currentCaseKey = null; 
    
    /// Grid Panel Initialization
    var grid = $(".dbgCasesTable").DbGrid({
        columns: [
            { name: "DEPARTMENT_CASE_NUMBER", title: "Department Case #" },
            { name: "DEPARTMENT_NAME", title: "Department" },
            { name: "CHARGE", title: "Charge" },
            { name: "LAB_CASE", title: "Lab Case #" },
            { name: "OFFENSE_DATE", title: "Incident Report Date" }
        ],
        dataSourceUrl: 'Exercise_Arvinn_AjaxWS.asmx/GetTopCases',
        tableId: 'dbGridTable',
        onRendered: function () {
            /// <summary>After multiple updates, re-select the previously selected row using CASE_KEY</summary>
            /// <param>response </param>
            /// <return type = "void">Void.</return>
            if (currentCaseKey) {
                var $rows = $(".dbgCasesTable tbody tr");
                $rows.each(function () {
                    var record = $(this).data("record");
                    if (record.CASE_KEY === currentCaseKey) {
                        $(this).addClass("selected").trigger("click");
                        return false;
                    }
                });
            }
        }
    });

    /// Control Panel Initialization
    var controlPanel = $('.dbControlPanel').DbPanel({
        controls: [
            { type: 'hidden', id: 'hndCaseKey', dataField: 'CASE_KEY' },
            { type: 'text', label: 'Department Case Number', id: 'txtDepartmentCaseNumber', dataField: 'DEPARTMENT_CASE_NUMBER' },
            {
                type: 'dropdown',
                label: 'Department',
                id: 'ddlDepartment',
                dataField: 'DEPARTMENT_CODE',
                dataSourceUrl: 'Exercise_Arvinn_AjaxWS.asmx/GetDepartments',
                valueField: 'DEPARTMENT_CODE',
                textField: 'DEPARTMENT_NAME'
            },
            {
                type: 'dropdown',
                label: 'Charge',
                id: 'ddlCharge',
                dataField: 'OFFENSE_CODE',
                dataSourceUrl: 'Exercise_Arvinn_AjaxWS.asmx/GetCharges',
                valueField: 'OFFENSE_CODE',
                textField: 'OFFENSE_DESCRIPTION'
            },
            { type: 'text', label: 'Lab Case #', id: 'txtLabCaseNumber', dataField: 'LAB_CASE' },
            { type: 'text', label: 'Incident Report Date', id: 'txtIncidentReportDate', dataField: 'OFFENSE_DATE' }
        ],
        dataSourceUrl: 'Exercise_Arvinn_AjaxWS.asmx/GetSelectedRowData',
        onRendered: function () {
            /// <summary>Handle first row selection after control panel rendering</summary>
            /// <param>response </param>
            /// <return type = "void">Void.</return>
            if (isFirstLoad) {
                /// During the first load, select the first row
                var $firstRow = grid.getFirstRow();
                setTimeout(() => {
                    if ($firstRow.length > 0) {
                        $firstRow.addClass("selected");
                        currentCaseKey = $firstRow.data("record").CASE_KEY;
                        controlPanel.getData(currentCaseKey);
                    }
                }, 500);
                isFirstLoad = false;
            } 
        }
    });

    controlPanel.browseMode();

    /// Row Click event setup
    $(".dbgCasesTable").on("click", "tr", function () {
        $(this).addClass("selected").siblings().removeClass("selected");
        var record = $(this).data('record');
        if (record) {
            currentCaseKey = record.CASE_KEY;
            controlPanel.getData(currentCaseKey);
        }
    });

    /// Datepicker Widget setup
    $('#txtIncidentReportDate').datepicker({
        maxDate: 0
    });

    $('#txtIncidentReportDate').on('keydown paste', function (e) {
        e.preventDefault();
    });

    /// Button panel setup
    $('#btnEdit').click(function () {
        edit();
    });

    $('#btnSave').click(function () {
        saveUpdate();
    });

    $('#btnCancel').click(function () {
        cancelUpdate();
    })

    function edit() {
        /// <summary>Enables input elements to edit table row data.</summary>
        /// <param>None.</param>
        /// <return type = "void">Void.</return>
        if (currentCaseKey) {
            controlPanel.editMode();
            grid.browseMode();
            disableButtons(false);
        }
    }

    function saveUpdate() {
        /// <summary>Saves update from input elements to DB.</summary>
        /// <param>None.</param>
        /// <return type = "void">Void.</return>    
        controlPanel.saveData();
        controlPanel.browseMode();
        grid.reloadData();
        grid.editMode();
        disableButtons(true);
    }

    function cancelUpdate() {
        /// <summary>Cancels update then restores initial value.</summary>
        /// <param>None.</param>
        /// <return type = "void">Void.</return>
        restoreInitialValue();
        controlPanel.browseMode();
        grid.editMode();
        disableButtons(true);
    }

    function disableButtons(disabled) {
        /// <summary>Disables input and button elements.</summary>
        /// <param name = "disabled">Boolean parameter.</param>
        /// <return type = "void">Void</return>
        $('#btnSave').prop('disabled', disabled);
        $('#btnCancel').prop('disabled', disabled);
        $('#btnEdit').prop('disabled', !disabled);
    }

    function restoreInitialValue() {
        /// <summary>Restores row value to the controls.</summary>
        /// <param>None.</param>
        /// <return type = "void">Void.</return>
        controlPanel.getData(currentCaseKey);
    }
});