(function ($) {
    //GRID PLUGIN
    $.fn.DbGrid = function (options) {
        // Default settings
        const settings = $.extend({
            data: []
        }, options);

        const $container = $(this);
        let selectedRecord = null;

        // Initialize the grid
        function init() {
            renderGrid();
            SelectRow();
        }

        // Render the grid table
        function renderGrid() {
            const table = $('<table id="gridView"></table>');
            const thead = `<thead>
                            <tr>
                                <th>Department Case #</th>
                                <th>Department Name</th>
                                <th>Charge</th>
                                <th>Lab Case #</th>
                                <th>Incedent Report Date</th>
                            </tr>
                          </thead>`;
            const tbody = $('<tbody></tbody>');

            settings.data.forEach(item => {
                const row = `<tr data-case-key="${item.CASE_KEY}" data-department-case-number="${item.DEPARTMENT_CASE_NUMBER}" data-department-code="${item.DEPARTMENT_CODE}" 
                             data-offense-code="${item.OFFENSE_CODE}" data-lab-case="${item.LAB_CASE}" data-offense-date="${item.OFFENSE_DATE}">
                                <td>${item.DEPARTMENT_CASE_NUMBER}</td>
                                <td>${item.DEPARTMENT_NAME}</td>
                                <td>${item.CHARGE}</td>
                                <td>${item.LAB_CASE}</td>
                                <td>${item.OFFENSE_DATE}</td>
                             </tr>`;
                tbody.append(row);
            });

            table.append(thead).append(tbody);
            $container.empty().append(table);
        }

        // Bind events for row selection and interaction
        function SelectRow() {
            $('#gridView').on('click', 'tr', function () {
                $(this).addClass('selected').siblings().removeClass('selected');

                // Get selected row data
                selectedRecord = {
                    CASE_KEY: $(this).data('caseKey'),
                    DEPARTMENT_CASE_NUMBER: $(this).data('departmentCaseNumber'),
                    DEPARTMENT_CODE: $(this).data('departmentCode'),
                    OFFENSE_CODE: $(this).data('offenseCode'),
                    LAB_CASE: $(this).data('labCase'),
                    OFFENSE_DATE: $(this).data('offenseDate')
                };

                console.log(selectedRecord);
            });
        }

        // Public method to enable edit mode
        this.editMode = function () {
            $('#gridView').removeClass('disabled'); // Enable interactions
            console.log("Grid is now in edit mode.");
        };

        // Public method to enable browse mode
        this.browseMode = function () {
            $('#gridView').addClass('disabled'); // Disable interactions
            console.log("Grid is now in browse mode.");
        };

        // Function to get the currently selected record
        this.getSelectedRecord = function () {
            return selectedRecord;
        };

        // Initialize the plugin
        init();

        return this;
    };

    //PANEL PLUGIN
    $.fn.dbPanel = function (options) {
        // Default settings
        const settings = $.extend({
            controls: []
        }, options);

        const $container = $(this);

        // Initialize the panel
        function init() {
            renderControls();
        }

        // Function to render controls based on the configuration
        function renderControls() {
            settings.controls.forEach(control => {
                const $formGroup = $('<div class="form-group"></div>');
                const $label = $(`<label>${control.label}</label>`);
                let $input;

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

        //Function to fetch data from WebMethod and bind to the dropdown
        function fetchDropdownData(url, $dropdown, valueField, textField) {
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

        // Function to bind data to a dropdown
        function bindDropdown($dropdown, options) {
            $dropdown.empty(); // Clear existing options
            options.forEach(option => {
                $dropdown.append(`<option value="${option.value}">${option.text}</option>`);
            });
        }

        // Function to toggle control enable/disable
        function toggleControls(enabled) {
            settings.controls.forEach(control => {
                const $control = $(`#${control.id}`);
                if (enabled) {
                    $control.prop('disabled', false);
                } else {
                    $control.prop('disabled', true);
                }
            });
        }

        // Enable edit mode
        this.editMode = function () {
            toggleControls(true);
            console.log("Control panel is now in edit mode.");
        };

        // Enable browse mode
        this.browseMode = function () {
            toggleControls(false);
            console.log("Control panel is now in browse mode.");
        };

        this.loadData = function (record) {
            if (record) {
                settings.controls.forEach(control => {
                    const fieldId = `#${control.id}`;
                    const value = record[control.dataField];
                    console.log(`Populating field ${fieldId} with value: ${value}`);


                    if (control.type === 'text' || control.type === 'hidden') {
                        $(fieldId).val(value);
                    } else if (control.type === 'dropdown') {
                        $(fieldId).val(value).change(); // Trigger change to update dropdowns if needed
                    }
                });
            }
        };

        this.getData = function () {
            formData = {};
            settings.controls.forEach(control => {
                formData[control.id] = $(`#${control.id}`).val();
            });
            return formData;
            console.log(formData);
        };

        // Initialize the plugin
        init();

        return this;
    };

})(jQuery);


$(document).ready(function () {
    // Grid Panel Initialization

    var gridData = null;
    
    $('#txtIncidentReportDate').datepicker({
        maxDate: 0
    });

    $('#btnEdit').click(function () {
        edit();
    });

    $('#btnSave').click(function () {
        saveUpdate();
    });

    $('#btnCancel').click(function () {
        cancelUpdate();
    })

    function handleResponse(response) {
        // Parse the response and assign it to the global variable
        gridData = JSON.parse(response.d);

        console.log(gridData); 
        initializeGrid();
    }

    function initializeGrid() {
        // Assign the grid to the global variable
        grid = $(".dbgCasesTable").DbGrid({
            data: gridData // Use the globalData here
        });

        console.log("Grid initialized:", grid);
    }

    function bindCasesTable() {
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
    
    // Control Panel Initialization
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

    bindCasesTable();
    controlPanel.browseMode();

    $(".dbgCasesTable").on('click', 'tr', function () {
        var selectedRecord = grid.getSelectedRecord();
        console.log(selectedRecord);
        if (selectedRecord) {
            controlPanel.loadData(selectedRecord);
        }
    });

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

        console.log(updatedData);

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
        /// <summary>Restores initial row value.</summary>
        /// <param>None.</param>
        /// <return type = "void">Void.</return>
        var selectedRecord = grid.getSelectedRecord();
        console.log(selectedRecord);
        if (selectedRecord) {
            controlPanel.loadData(selectedRecord);
        }
    }
});