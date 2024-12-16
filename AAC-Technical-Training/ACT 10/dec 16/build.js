function calculateAge(dobInput) {
    var dob = new Date(dobInput.value);
    if (isNaN(dob)) return;

    var today = new Date();
    var age = today.getFullYear() - dob.getFullYear();
    var monthDiff = today.getMonth() - dob.getMonth();

    // Adjust age if the birthday hasn't occurred this year
    if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < dob.getDate())) {
        age--;
    }

    // Set the age value in the corresponding age input box
    var ageCell = dobInput.parentElement.nextElementSibling;
    ageCell.querySelector('input').value = age >= 0 ? age : '';
}

// Function to handle Submit button
function handleSubmit() {
    alert('Submit button clicked!');
    var table = document.getElementById('dynamicTable');
    var rows = table.getElementsByTagName('tr');
    var csvData = [];

    // Capture table header
    var headers = [];
    rows[0].querySelectorAll('th').forEach(header => {
        headers.push(header.innerText);
    });
    csvData.push(headers.join(',')); // Add header row to CSV data

    // Loop through table rows (skip the header row)
    for (var i = 1; i < rows.length; i++) {
        var row = rows[i];
        var cells = row.querySelectorAll('td input, td select'); // Inputs and selects
        var rowData = [];

        // Loop through cells in the row
        cells.forEach(cell => {
            rowData.push(cell.value || cell.options ?.[cell.selectedIndex] ?.value || ''); // Handle text and dropdown values
        });

        csvData.push(rowData.join(',')); // Add row data to CSV
    }

    // Convert to CSV string
    var csvContent = csvData.join('\n');

    // Download the CSV file
    var blob = new Blob([csvContent], { type: 'text/csv' });
    var url = URL.createObjectURL(blob);
    var a = document.createElement('a');
    a.href = url;
    a.download = 'table_data.csv';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);

    alert('CSV file generated and downloaded successfully!');
}

// Function to handle Cancel button
function handleCancel() {
    // Clear the table and re-enable the "Generate Table" button
    document.getElementById('dynamicTable').getElementsByTagName('tbody')[0].innerHTML = '';
    document.getElementById('buttonContainer').innerHTML = '';
    document.getElementById('btnGenerateTable').disabled = false;
}

$(document).ready(() => {
    var selectedRow = null;
    var selectedCaseKey = null;
    var selectedOffenseCode = null;
    var selectedDepartmentCode = null;
    var initialValues = {};

    bindCasesTable();
    fetchDepartments();
    fetchCharge();
    rowHighlight();

    /// Date picker UI widget
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

    $('#btnGenerateTable').click(function () {
        $('#btnGenerateTable').prop('disabled', true);
        generateTable();
    })

    $('#btnReverseOrder').click(function () {
        reverseOrder();
    })

    function bindCasesTable() {
        /// <summary>Binds Cases Table data from DB via web method</summary>
        /// <param>None.</param>
        /// <return type = "void">Void.</return>
        $.ajax({
            type: "POST",
            url: "Exercise_Arvinn_AjaxWS.asmx/GetTopCases",
            data: '{}',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                var data = JSON.parse(response.d);
                var tableBody = $(".cases-table-body");
                tableBody.empty(); 
                $.each(data, function (i, item) {
                    var row = "<tr data-case-key=' " + item.CASE_KEY +
                        "' data-department-code='" + item.DEPARTMENT_CODE +
                        "' data-offense-code='" + item.OFFENSE_CODE + "'>" +
                        "<td>" + item.DEPARTMENT_CASE_NUMBER + "</td>" +
                        "<td>" + item.DEPARTMENT_NAME + "</td>" +
                        "<td>" + item.CHARGE + "</td>" +
                        "<td>" + item.LAB_CASE + "</td>" +
                        "<td>" + item.OFFENSE_DATE + "</td>" +
                        "</tr>";
                    
                    tableBody.append(row);
                    highlightRow(selectedCaseKey);
                });
            },
            error: function (err) {
                console.log("Error: ", err);
            }
        });
    }

    function fetchDepartments() {
        /// <summary>Fetches List of Department objects from DB via web method</summary>
        /// <param>None.</param>
        /// <return type = "void">Void.</return>
        $.ajax({
            type: "POST",
            url: "Exercise_Arvinn_AjaxWS.asmx/GetDepartments",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                populateDropdown("ddlDepartment", JSON.parse(response.d), "DEPARTMENT_CODE", "DEPARTMENT_NAME");
            },
            error: function (error) {
                console.error("Error fetching Departments:", error);
            }
        });
    }

    function fetchCharge() {
        /// <summary>Fetches List of Charge objects from DB via web method</summary>
        /// <param>None.</param>
        /// <return type = "void">Void.</return>
        $.ajax({
            type: "POST",
            url: "Exercise_Arvinn_AjaxWS.asmx/GetCharges",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                populateDropdown("ddlCharge", JSON.parse(response.d), "OFFENSE_CODE", "OFFENSE_DESCRIPTION");
            },
            error: function (error) {
                console.error("Error fetching Charges:", error);
            }
        });
    }

    function populateDropdown(dropdownId, items, valueField, textField) {
        /// <summary>Populates dropdown list.</summary>
        /// <param names = "dropdownId", "items", "valueField", "textField">Dropdown element, items, value field, text field</param>
        /// <return type = "void">Void.</return>
        var dropdown = document.getElementById(dropdownId);
        dropdown.innerHTML = '<option value="">Select an option</option>';

        items.forEach(function (item) {
            var option = document.createElement("option");
            option.value = item[valueField];
            option.textContent = item[textField];
            dropdown.appendChild(option);
        });
    }

    function rowHighlight() {
        /// <summary>Highlight and display selected row data in input elements upon clicking.</summary>
        /// <param>None.</param>
        /// <return type = "void">Void.</return>
        $(document).on('click', '.cases-table-body tr', function () {
            $('.cases-table-body tr').removeClass('table-row-color');
            $(this).addClass('table-row-color');
        
            selectedRow = $(this);
            selectedCaseKey = $(this).data("caseKey")
            selectedOffenseCode = $(this).data("offenseCode")
            selectedDepartmentCode = $(this).data("departmentCode")

            var departmentCaseNumber = $(this).find('td:eq(0)').text();
            var labCaseNumber = $(this).find('td:eq(3)').text();
            var incidentReportDate = $(this).find('td:eq(4)').text();

            $('#txtDepartmentCaseNumber').val(departmentCaseNumber);
            $('#ddlDepartment').val(selectedDepartmentCode);
            $('#ddlCharge').val(selectedOffenseCode);
            $('#txtLabCaseNumber').val(labCaseNumber);
            $('#txtIncidentReportDate').val(incidentReportDate);

            storeInitialValue();
        });
    }

    function edit() {
        /// <summary>Enables input elements to edit table row data.</summary>
        /// <param>None.</param>
        /// <return type = "void">Void.</return>
        if (selectedRow) {
            disableInputButtons(false);

            /// disable clicking on cases-table-row tr
            $('.cases-table-body tr').not(selectedRow).addClass('disabled');
        }
    }

    function saveUpdate() {
        /// <summary>Saves update from input elements to DB.</summary>
        /// <param>None.</param>
        /// <return type = "void">Void.</return>        
        
        if (selectedRow) {
            var updatedData = {
                CASE_KEY: selectedCaseKey,
                DEPARTMENT_CASE_NUMBER: document.getElementById("txtDepartmentCaseNumber").value,
                DEPARTMENT_CODE: document.getElementById("ddlDepartment").value,
                OFFENSE_CODE: document.getElementById("ddlCharge").value,
                LAB_CASE: document.getElementById("txtLabCaseNumber").value,
                OFFENSE_DATE: document.getElementById("txtIncidentReportDate").value
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
                    console.error("Error saving data:", error);
                }
            });
        }
        storeInitialValue();
        disableInputButtons(true);
        enableClick();
    }

    function cancelUpdate() {
        /// <summary>Cancels update then restores initial value.</summary>
        /// <param>None.</param>
        /// <return type = "void">Void.</return>
        restoreInitialValue();
        disableInputButtons(true);
        enableClick();
    }

    function reverseOrder() {
        /// <summary>Cancels update then restores initial value.</summary>
        /// <param>None.</param>
        /// <return type = "void">Void.</return>
        $.ajax({
            type: "POST",
            url: "Default.aspx/GetReversedCsv", // WebMethod URL
            data: JSON.stringify({ csvInput: csvInput }), // Pass CSV input as parameter
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                // Parse and display the response
                var reversedData = response.d;

                // Clear and populate the table
                var tableBody = $('#tblResults tbody');
                tableBody.empty();

                for (var i = 0; i < reversedData.length; i++) {
                    tableBody.append('<tr><td>' + reversedData[i].CriminalRecord + '</td></tr>');
                }
            },
            error: function (error) {
                console.error("Error:", error);
            }
        });
    }

    function storeInitialValue() {
        /// <summary>Stores initial row value.</summary>
        /// <param>None.</param>
        /// <return type = "void">Void.</return>
        $('.form-table input, .form-table select').each(function () {
            var id = $(this).attr('id');
            initialValues[id] = $(this).val();
        });
    }

    function restoreInitialValue() {
        /// <summary>Restores initial row value.</summary>
        /// <param>None.</param>
        /// <return type = "void">Void.</return>
        $('.form-table input, .form-table select').each(function () {
            var id = $(this).attr('id');
            $(this).val(initialValues[id]);
        });
    }

    function disableInputButtons(disabled) {
        /// <summary>Disables input and button elements.</summary>
        /// <param name = "disabled">Boolean parameter.</param>
        /// <return type = "void">Void</return>
        $("button").prop("disabled", disabled);
        $("input").prop("disabled", disabled);
        $("select").prop("disabled", disabled);
        $('#btnEdit').prop('disabled', !disabled);
    }

    function enableClick() {
        /// <summary>Enable clicking on table body rows.</summary>
        /// <param>None.</param>
        /// <return type = "void">Void.</return>
        $('.cases-table-body tr').not(selectedRow).removeClass('disabled');
    }

    function highlightRow(caseKey) {
        /// <summary>Highlights the row based on case key, this is necessary since bindCaseTable refreshes applied Jquery elements</summary>
        /// <param>None.</param>
        /// <return type = "void">Void.</return>
        $(".cases-table-body tr").removeClass("table-row-color");
        $(".cases-table-body tr[data-case-key='" + caseKey + "']").addClass("table-row-color");
    }


    function generateTable() {
        // Get the number of rows from the input
        var numRows = parseInt(document.getElementById('txtNumber').value);

        if (isNaN(numRows) || numRows <= 0) {
            alert('Please enter a valid number greater than 0.');
            return;
        }

        // Reference the table body
        var tableBody = document.getElementById('dynamicTable').getElementsByTagName('tbody')[0];

        // Clear any existing rows
        tableBody.innerHTML = '';

        // Generate rows based on the input number
        for (var i = 0; i < numRows; i++) {
            var row = document.createElement('tr');

            // Create and append cells for each column
            row.innerHTML = `
                <td><input type="text" placeholder="First Name" /></td>
                <td><input type="text" placeholder="Middle Name" /></td>
                <td><input type="text" placeholder="Last Name" /></td>
                <td>
                    <select>
                        <option value="">Select Gender</option>
                        <option value="Male">Male</option>
                        <option value="Female">Female</option>
                    </select>
                </td>
                <td><input type="date" onchange="calculateAge(this)" /></td>
                <td><input type="number" placeholder="Age" readonly /></td>
            `;

            // Append the row to the table body
            tableBody.appendChild(row);
        }

        var buttonContainer = document.getElementById('buttonContainer');
        buttonContainer.innerHTML = `
        <button type="button" id="submitButton" onclick="handleSubmit()">Submit</button>
        <button type="button" id="cancelButton" onclick="handleCancel()">Cancel</button>
    `;
    }
});