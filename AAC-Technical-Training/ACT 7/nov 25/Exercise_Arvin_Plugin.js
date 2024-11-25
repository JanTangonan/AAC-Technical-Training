(function ($) {
    /// GRID PLUGIN ///
    $.fn.DbGrid = function (options) {
        // Default settings
        var settings = $.extend({
            columns: [],
            dataSourceUrl: null,  
            onRendered: null 
        }, options);

        var $container = $(this);
        var selectedRecord = null;

        function init() {
            renderTable(); 
            selectRow();

            if (settings.dataSourceUrl) {
                fetchAndBindData(); 
            }
        }

        function renderTable() {
            /// <summary>Renders the grid table structure.</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
            var table = $('<table id="dbGrid" class="db-grid"></table>');

            var thead = $('<thead><tr></tr></thead>');
            settings.columns.forEach(col => {
                var th = $(`<th>${col.title}</th>`);
                thead.find('tr').append(th);
            });
            table.append(thead);

            var tbody = $('<tbody></tbody>');
            table.append(tbody);

            $container.empty().append(table);
        }

        function fetchAndBindData() {
            /// <summary>Fetch data via AJAX and bind to the table.</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
            $.ajax({
                type: "POST",
                url: settings.dataSourceUrl,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (response) {
                    var data = JSON.parse(response.d);
                    bindTableData(data);

                    settings.onRendered ?.();
                },
                error: function (xhr, status, error) {
                    console.error("Error fetching data for grid:", error);
                }
            });
        }

        function bindTableData(data) {
            /// <summary>Binds data to the table body.</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
            var $tbody = $container.find('#dbGrid tbody');
            $tbody.empty(); // Clear existing rows

            data.forEach(item => {
                var row = $('<tr></tr>');
                row.data('record', item); 

                settings.columns.forEach(col => {
                    var cellValue = item[col.name] || ''; 
                    row.append(`<td>${cellValue}</td>`);
                });

                $tbody.append(row);
            });
        }

        function selectRow() {
            /// <summary>Bind events for row selection and interaction</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
            $('#dbGrid').on('click', 'tr', function () {
                $(this).addClass('selected').siblings().removeClass('selected');
                selectedRecord = $(this).data('record');
            });
        }

        this.reloadData = function () {
            /// <summary>Rebind grid table</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
            if (settings.dataSourceUrl) {
                fetchAndBindData();
            } else {
                console.error("Data source URL is not defined.");
            }
        };

        this.editMode = function () {
            /// <summary>Enable edit mode - enables grid table and enables interactions</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
            $('#dbGrid').removeClass('disabled');  
        };

        this.browseMode = function () {
            /// <summary>Enable browse mode - disables grid table and disables interactions</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
            $('#dbGrid').addClass('disabled');  
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
        var settings = $.extend({
            controls: [],
            onRendered: null 
        }, options);

        var $container = $(this);
        var pendingDropdowns = 0;

        function init() {
            renderControls();
        }

        function renderControls() {
            /// <summary>Renders controls based on the configuration</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
            settings.controls.forEach(control => {
                var $formGroup = $('<div class="control-row"></div>');
                var $label;
                var $input;

                if (control.type !== 'hidden') {
                    $label = $(`<label class="control-label">${control.label}</label>`);
                }

                if (control.type === 'text') {
                    $input = $(`<input type="text" id="${control.id}" class="control-input">`);
                } else if (control.type === 'dropdown') {
                    $input = $(`<select id="${control.id}" class="control-input"></select>`);
                    if (control.dataSourceUrl) {
                        pendingDropdowns++; 
                        fetchDropdownData(control.dataSourceUrl, $input, control.valueField, control.textField, () => {
                            pendingDropdowns--; 
                            checkAllDropdownsPopulated();
                        });
                    }
                } else if (control.type === 'hidden')
                {
                    $input = $(`<input type="hidden" id="${control.id}" class="control-input">`);
                }

                $formGroup.append($label).append($input);
                $container.append($formGroup);
            });

            if (pendingDropdowns === 0) {
                settings.onRendered ?.();
            }
        }

        function fetchDropdownData(url, $dropdown, valueField, textField, onComplete) {
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
                    var options = data.map(item => ({
                        value: item[valueField],
                        text: item[textField]
                    }));
                    bindDropdown($dropdown, options);
                    onComplete ?.();
                },
                error: function (xhr, status, error) {
                    console.error("Error fetching dropdown data:", error);
                    onComplete ?.();
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

        function checkAllDropdownsPopulated() {
            /// <summary>Check if all dropdowns are populated and trigger onRendered</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
            if (pendingDropdowns === 0) {
                settings.onRendered ?.(); 
            }
        }

        function toggleControls(enabled) {
            /// <summary>Toggles control enable/disable</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
            settings.controls.forEach(control => {
                var $control = $(`#${control.id}`);
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
                    var fieldId = `#${control.id}`;
                    var value = record[control.dataField];

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
    var controlPanel = $('.dbControlPanel').dbPanel({
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
        onRendered: function () {
            /// <summary>Handle first row selection after control panel rendering</summary>
            /// <param>response </param>
            /// <return type = "void">Void.</return>
            if (isFirstLoad) {
                /// During the first load, select the first row
                var $firstRow = $(".dbgCasesTable tbody tr:first");
                if ($firstRow.length > 0) {
                    $firstRow.addClass("selected").trigger("click");
                    currentCaseKey = $firstRow.data("record").CASE_KEY;
                    controlPanel.loadData($firstRow.data("record"));
                    controlPanel.browseMode();
                }
                isFirstLoad = false;
            } 
        }
    });

    /// Row Click event setup
    $(".dbgCasesTable").on("click", "tr", function () {
        $(this).addClass("selected").siblings().removeClass("selected");
        var record = grid.getSelectedRecord();
        if (record) {
            controlPanel.loadData(record);
            currentCaseKey = record.CASE_KEY;
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
                grid.reloadData();
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