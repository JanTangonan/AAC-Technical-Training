$(document).ready(function () {
    /// Grid Panel Initialization

    var isFirstLoad = true;

    var grid = $("#tblGrid").DbGrid({
        dataKey: "CASE_KEY",
        columns: [
            { name: "DEPARTMENT_CASE_NUMBER", title: "Department Case #" },
            { name: "DEPARTMENT_NAME", title: "Department" },
            { name: "CHARGE", title: "Charge" },
            { name: "LAB_CASE", title: "Lab Case #" },
            { name: "OFFENSE_DATE", title: "Incident Report Date" }
        ],
        onRendered: function () {
            /// <summary>Selects first row of grid upon start of page.</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
            if (isFirstLoad) {
                // During the first load, select the first row
                setTimeout(() => {
                    var $firstRow = grid.getFirstRow();
                    if ($firstRow.length > 0) {
                        $firstRow.addClass("selected");

                        controlPanel.dataKey = grid.getSelectedRowKey($firstRow);
                        controlPanel.getData();
                    }
                }, 1000);
                isFirstLoad = false;
            }
            else if (controlPanel.dataKey) {
                // After subsequent updates, re-select the previously selected row
                var $rows = $(".dbgCasesTable tbody tr");
                $rows.each(function () {
                    var rowKey = grid.getSelectedRowKey($(this));
                    if (rowKey === controlPanel.dataKey) {
                        $(this).addClass("selected").trigger("click");
                        return false;
                    }
                });
            }
        }
    });

    /// Control Panel Initialization
    var controlPanel = $('.dbpCasesControls').dbPanel({
        tableName: 'TV_LABCASE',
        controls: [
            { type: 'hidden', id: 'hndCaseKey', dataField: 'CASE_KEY' },
            { type: 'text', label: 'Department Case Number', id: 'txtDepartmentCaseNumber', dataField: 'DEPARTMENT_CASE_NUMBER' },
            {
                type: 'dropdown',
                label: 'Department',
                id: 'ddlDepartment',
                dataField: 'DEPARTMENT_CODE',
                dataSourceUrl: 'Exercise_Arvin_Plugin2WS.asmx/GetDepartments',
                valueField: 'DEPARTMENT_CODE',
                textField: 'DEPARTMENT_NAME'
            },
            {
                type: 'dropdown',
                label: 'Charge',
                id: 'ddlCharge',
                dataField: 'OFFENSE_CODE',
                dataSourceUrl: 'Exercise_Arvin_Plugin2WS.asmx/GetCharges',
                valueField: 'OFFENSE_CODE',
                textField: 'OFFENSE_DESCRIPTION'
            },
            { type: 'text', label: 'Lab Case #', id: 'txtLabCaseNumber', dataField: 'LAB_CASE' },
            { type: 'text', label: 'Incident Report Date', id: 'txtIncidentReportDate', dataField: 'OFFENSE_DATE' }
        ],
        read: function (requestData, onSuccess, onError) {
            /// Read selected row
            $.ajax({
                type: "POST",
                url: "Exercise_Arvin_Plugin2WS.asmx/GetSelectedRowData",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(requestData),
                success: function (response) {
                    var data = JSON.parse(response.d);
                    onSuccess(data);
                },
                error: onError,
            });
        },
        update: function (data, onSuccess, onError) {
            /// Save updated control panel input data to DB
            $.ajax({
                type: "POST",
                url: "Exercise_Arvin_Plugin2WS.asmx/SaveData",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify(data),
                success: onSuccess,
                error: onError,
            });
        }
    });

    controlPanel.browseMode();

    /// Row Click event setup
    grid.on("click", "tr", function () {
        $(this).addClass("selected").siblings().removeClass("selected");
        var selectedKey = grid.getSelectedRowKey($(this));
        controlPanel.dataKey = selectedKey;
        controlPanel.getData();
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
        if (controlPanel.dataKey) {
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
        controlPanel.getData();
    }
});