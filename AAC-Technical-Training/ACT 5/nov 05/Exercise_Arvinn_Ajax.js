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
});