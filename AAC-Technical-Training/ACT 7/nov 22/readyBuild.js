(function ($) {
    /// GRID PLUGIN ///
    $.fn.DbGrid = function (options) {
        // Default settings
        const settings = $.extend({
            data: [],
            columns: [],
            onRendered: null 
        }, options);

        const $container = $(this);
        let selectedRecord = null;

        function init() {
            /// <summary>Initialize the grid</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
            renderGrid();
            selectRow();

            /// execute onRendered function lastly
            if (typeof settings.onRendered === "function") {
                settings.onRendered();
            }
        }

        function renderGrid() {
            /// <summary>Render the grid table</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
            const table = $('<table id="gridView" class="db-grid"></table>');

            const thead = $('<thead><tr></tr></thead>');
            settings.columns.forEach(col => {
                const th = $(`<th>${col.title}</th>`);
                thead.find('tr').append(th);
            });
            table.append(thead);

            const tbody = $('<tbody></tbody>');
            settings.data.forEach(item => {
                const row = $('<tr></tr>');
                row.data('record', item);

                settings.columns.forEach(col => {
                    const cellValue = item[col.name] || '';
                    row.append(`<td>${cellValue}</td>`);
                });

                tbody.append(row);
            });

            table.append(tbody);
            $container.empty().append(table);
        }

        function selectRow() {
            /// <summary>Bind events for row selection and interaction</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
            $('#gridView').on('click', 'tr', function () {
                $(this).addClass('selected').siblings().removeClass('selected');
                selectedRecord = $(this).data('record');
            });
        }

        this.editMode = function () {
            /// <summary>Enable edit mode - enables grid table and enables interactions</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
            $('#gridView').removeClass('disabled');  
        };

        this.browseMode = function () {
            /// <summary>Enable browse mode - disables grid table and disables interactions</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
            $('#gridView').addClass('disabled');  
        };

        this.getSelectedRecord = function () {
            /// <summary>Returns currently selected record</summary>
            /// <param>selectedRecord</param>
            /// <return type = "void">Void.</return>
            return selectedRecord;
        };

        /// Initialize the plugin
        init();

        return this;
    };

    /// PANEL PLUGIN ///
    $.fn.dbPanel = function (options) {
        // Default settings
        const settings = $.extend({
            controls: []
        }, options);

        const $container = $(this);

        function init() {
            /// <summary>Initialize the panel</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
            renderControls();
        }

        function renderControls() {
            /// <summary>Renders controls based on the configuration</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
            settings.controls.forEach(control => {
                const $formGroup = $('<div class="control-row"></div>');
                let $label;
                let $input;

                if (control.type !== 'hidden') {
                    $label = $(`<label class="control-label">${control.label}</label>`);
                }

                // Create input elements based on control type
                if (control.type === 'text') {
                    $input = $(`<input type="text" id="${control.id}" class="control-input">`);
                } else if (control.type === 'dropdown') {
                    $input = $(`<select id="${control.id}" class="control-input"></select>`);
                    if (control.dataSourceUrl) {
                        fetchDropdownData(control.dataSourceUrl, $input, control.valueField, control.textField);
                    }
                } else if (control.type === 'hidden')
                {
                    $input = $(`<input type="hidden" id="${control.id}" class="control-input">`);
                }

                $formGroup.append($label).append($input);
                $container.append($formGroup);
            });
        }

        function fetchDropdownData(url, $dropdown, valueField, textField) {
            /// <summary>Fetch data from WebMethod and bind to the dropdown</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
            $.ajax({
                type: "POST",
                url: url,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (response) {
                    var data = JSON.parse(response.d);
                    const options = data.map(item => ({
                        value: item[valueField],
                        text: item[textField]
                    }));
                    bindDropdown($dropdown, options);
                },
                error: function (xhr, status, error) {
                    console.error("Error fetching dropdown data:", error);
                }
            });
        }

        function bindDropdown($dropdown, options) {
            /// <summary>Binds data to a dropdown</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
            $dropdown.empty(); // Clear existing options
            options.forEach(option => {
                $dropdown.append(`<option value="${option.value}">${option.text}</option>`);
            });
        }

        function toggleControls(enabled) {
            /// <summary>Toggles control enable/disable</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
            settings.controls.forEach(control => {
                const $control = $(`#${control.id}`);
                if (enabled) {
                    $control.prop('disabled', false);
                } else {
                    $control.prop('disabled', true);
                }
            });
        }

        this.editMode = function () {
            /// <summary>Enable edit mode</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
            toggleControls(true);
        };

        this.browseMode = function () {
            /// <summary>Enable browse mode</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
            toggleControls(false);
        };

        this.loadData = function (record) {
            /// <summary>Load data into control panel using record data</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
            if (record) {
                settings.controls.forEach(control => {
                    const fieldId = `#${control.id}`;
                    const value = record[control.dataField];

                    if (control.type === 'text' || control.type === 'hidden') {
                        $(fieldId).val(value);
                    } else if (control.type === 'dropdown') {
                        $(fieldId).val(value).change();
                    }
                });
            }
        };

        this.getData = function () {
            /// <summary>Returns updated control panel input data</summary>
            /// <param>formData </param>
            /// <return type = "void">Void.</return>
            formData = {};
            settings.controls.forEach(control => {
                formData[control.id] = $(`#${control.id}`).val();
            });
            return formData;
        };

        /// Initialize the plugin
        init();

        return this;
    };

})(jQuery);


$(document).ready(function () {
    /// Grid Panel Initialization
    let grid = null;
    let isFirstLoad = true;
    let currentCaseKey = null; 

    function handleResponse(response) {
        /// <summary>Parse the response and assign it to the global variable.</summary>
        /// <param>response </param>
        /// <return type = "void">Void.</return>
        var gridData = JSON.parse(response.d);
        initializeGrid(gridData);
    }

    function initializeGrid(data) {
        /// <summary>Initialize the grid and uses the parsed data</summary>
        /// <param>data</param>
        /// <return type = "void">Void.</return>
        grid = $(".dbgCasesTable").DbGrid({
            data: data,
            columns: [
                { name: "DEPARTMENT_CASE_NUMBER", title: "Department Case #" },
                { name: "DEPARTMENT_NAME", title: "Department" },
                { name: "CHARGE", title: "Charge" },
                { name: "LAB_CASE", title: "Lab Case #" },
                { name: "OFFENSE_DATE", title: "Incident Report Date" }
            ],
            onRendered: function () {
                /// <summary>Handle row selection after rendering</summary>
                /// <param>response </param>
                /// <return type = "void">Void.</return>
                if (isFirstLoad) {
                    /// During the first load, select the first row
                    const $firstRow = $(".dbgCasesTable tbody tr:first");
                    if ($firstRow.length > 0) {
                        $firstRow.addClass("selected").trigger("click");
                        currentCaseKey = $firstRow.data("record").CASE_KEY;
                        controlPanel.loadData($firstRow.data("record"));
                    }
                    isFirstLoad = false; 
                } else if (currentCaseKey) {
                    /// After multiple updates, re-select the previously selected row using CASE_KEY
                    const $rows = $(".dbgCasesTable tbody tr");
                    $rows.each(function () {
                        const record = $(this).data("record");
                        if (record.CASE_KEY === currentCaseKey) {
                            $(this).addClass("selected").trigger("click");
                            return false; 
                        }
                    });
                }
            }
        });

        /// Row Click event setup
        $(".dbgCasesTable").on("click", "tr", function () {
            $(this).addClass("selected").siblings().removeClass("selected");
            const record = grid.getSelectedRecord();
            if (record) {
                controlPanel.loadData(record);
                currentCaseKey = record.CASE_KEY; 
            }
        });
    }

    function bindCasesTable() {
        /// <summary>Fetches response from web method and passes to the grid.</summary>
        /// <param></param>
        /// <return type = "void">Void.</return>
        $.ajax({
            type: "POST",
            url: "Exercise_Arvinn_AjaxWS.asmx/GetTopCases", // Replace with your actual WebForm URL
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                handleResponse(response);
            },
            error: function (error) {
                console.error("Error fetching data: ", error);
            }
        });
    }
    
    /// Control Panel Initialization
    const controlPanel = $('.dbControlPanel').dbPanel({
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
        ]
    });

    /// Datepicker Widget setup
    $('#txtIncidentReportDate').datepicker({
        maxDate: 0
    });

    $('#txtIncidentReportDate').on('keydown paste', function (e) {
        e.preventDefault();
    });

    bindCasesTable();
    controlPanel.browseMode();

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
        controlPanel.editMode();
        grid.browseMode();
        disableButtons(false);
    }

    function saveUpdate() {
        /// <summary>Saves update from input elements to DB.</summary>
        /// <param>None.</param>
        /// <return type = "void">Void.</return>        
        var controlPanelData = controlPanel.getData();
        var updatedData = {
            CASE_KEY: controlPanelData.hndCaseKey,
            DEPARTMENT_CASE_NUMBER: controlPanelData.txtDepartmentCaseNumber,
            DEPARTMENT_CODE: controlPanelData.ddlDepartment,
            OFFENSE_CODE: controlPanelData.ddlCharge,
            LAB_CASE: controlPanelData.txtLabCaseNumber,
            OFFENSE_DATE: controlPanelData.txtIncidentReportDate
        };

        $.ajax({
            type: "POST",
            url: "Exercise_Arvinn_AjaxWS.asmx/SaveData",
            data: JSON.stringify(updatedData),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                alert("Input Saved");
                bindCasesTable();
            },
            error: function (error) {
                alert("Input Not Saved: Error Detected");
                console.error("Error saving data:", error);
            }
        });
            
        controlPanel.browseMode();
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
        $("button").prop("disabled", disabled);
        $('#btnEdit').prop('disabled', !disabled);
    }

    function restoreInitialValue() {
        /// <summary>Restores row value to the controls.</summary>
        /// <param>None.</param>
        /// <return type = "void">Void.</return>
        var selectedRecord = grid.getSelectedRecord();
        if (selectedRecord) {
            controlPanel.loadData(selectedRecord);
        }
    }
});