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
            $('#DbGrid').on('click', 'tr', function () {
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
            data: []
        }, options);

        const $container = $(this);
        let selectedRecord = null;

        // Initialize the panel
        function init() {
            renderGrid();
            SelectRow();
        }

        // Render the panel
        function renderPanel() {
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
            $('#dbgrid').on('click', 'tr', function () {
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

        // Function to get the currently selected record
        this.getSelectedRecord = function () {
            return selectedRecord;
        };

        // Initialize the plugin
        init();

        return this;
    };

})(jQuery);


// Initialize Plugins
$(document).ready(function () {
    // Grid Panel Initialization
    $.ajax({
        type: "POST",
        url: "Exercise_Arvinn_AjaxWS.asmx/GetTopCases", // Replace with your actual WebForm URL
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            var gridData = JSON.parse(response.d);
            var columns = Object.keys(gridData[0]);

            $(".dbgCasesTable").gridView({
                //columns: ["Case Key", "Department Code", "Offense Code", "Department Case #", "Department", "Charge", "Lab Case #", "Incedent Report Date"],
                //columns: columns,
                data: gridData

                //rowClick: function (rowData) {
                //    $("#txtName").val(rowData.Name);
                //    $("#txtAge").val(rowData.Age);
                //    $("#txtDepartment").val(rowData.Department);
                //}
            });
        },
        error: function (xhr, status, error) {
            console.error("Error fetching data: ", error);
        }
    });

    // Control Panel Initialization
    $(".my-controls").controlPanel({
        controls: [
            { label: "Name", type: "text", id: "txtName" },
            { label: "Age", type: "number", id: "txtAge" },
            { label: "Department", type: "text", id: "txtDepartment" }
        ]
    });

    // Button Panel Initialization
    $(".my-buttons").buttonPanel({
        buttons: [
            {
                label: "Save",
                action: function () {
                    const name = $("#txtName").val();
                    const age = $("#txtAge").val();
                    const department = $("#txtDepartment").val();
                    alert(`Saved: ${name}, ${age}, ${department}`);
                }
            },
            {
                label: "Cancel",
                action: function () {
                    $("#txtName").val("");
                    $("#txtAge").val("");
                    $("#txtDepartment").val("");
                }
            }
        ]
    });
});