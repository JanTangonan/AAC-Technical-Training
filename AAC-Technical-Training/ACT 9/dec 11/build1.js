(function ($) {
    /// CONTROL PANEL PLUGIN ///
    $.fn.dbPanel = function (options) {
        /// Default settings
        var defaults = {
            tableName: null,
            controls: [],
            dataSourceUrl: null,
            read: null,
            update: null,
            targetGrid: null
        };

        var $controlPanel = $(this);

        /// options will be the first priority followed by data-attributes then default values
        var settings = $.extend({}, defaults, $controlPanel.data(), options);

        this.dataKey = null;

        function init() {
            var readMethod = $controlPanel.data("read");

            // Check if valid function in the global scope
            if (readMethod && typeof window[readMethod] === "function") {
                settings.read = window[readMethod];
            }

            renderControls();
            bindGridEvents();
        }

        function renderControls() {
            /// <summary>Renders controls based on the configuration</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
            settings.controls.forEach(control => {
                var $formGroup = $("<div>", { class: "form-group row mb-2" });
                let $label, $input;

                if (control.type !== 'hidden') {
                    $label = $(`<label class="col-sm-4 col-form-label">${control.label}</label>`);
                }

                var $inputWrapper = $("<div>", { class: "col-sm-8" });

                // Create input elements based on control type
                if (control.type === 'text') {
                    $input = $(`<input type="text" id="${control.id}" class="form-control form-control-sm">`);
                } else if (control.type === 'dropdown') {
                    $input = $(`<select id="${control.id}" class="form-control form-control-sm"></select>`);
                    if (control.dataSourceUrl) {
                        fetchDropdownData(control.dataSourceUrl, $input, control.valueField, control.textField);
                    }
                } else if (control.type === 'hidden') {
                    $input = $(`<input type="hidden" id="${control.id}">`);
                }

                if (control.type !== 'hidden') {
                    $formGroup.append($label);
                }

                $inputWrapper.append($input);
                $formGroup.append($inputWrapper);
                $controlPanel.append($formGroup);
            });
        }

        function bindGridEvents() {
            /// Bind the `rowSelected` event from the grid to the control panel
            /// <param>data</param>
            /// <return type = "void">Void.</return>
            if (settings.targetGrid) {
                $(`#${settings.targetGrid}`).on("rowSelected", function (e, rowData) {
                    loadDataFromGrid(rowData);
                });
            }
        }

        function loadDataFromGrid(dataKey) {
            /// <summary>Loads data into the panel by fetching it using the unique dataKey</summary>
            /// <param>dataKey</param>
            /// <return type = "void">Void.</return>
            this.dataKey = dataKey;

            if (typeof settings.read === "function") {
                var requestData = {
                    KEY: this.dataKey,
                    tableName: settings.tableName,
                    fields: settings.controls.map(control => control.dataField)
                };
                settings.read(requestData, loadData, handleError); // Fetch the data
            } else {
                console.error("The panelRead function (settings.read) is not defined.");
            }
        }

        function loadData(data) {
            /// <summary>Load data into control panel using selected record data</summary>
            /// <param>data</param>
            /// <return type = "void">Void.</return>
            if (data) {
                settings.controls.forEach(control => {
                    var fieldId = `#${control.id}`;
                    var value = data[control.dataField];

                    if (control.type === 'text' || control.type === 'hidden') {
                        $(fieldId).val(value);
                    } else if (control.type === 'dropdown') {
                        $(fieldId).val(value).change();
                    }
                });
            }
        };

        function fetchDropdownData(url, $dropdown, valueField, textField) {
            /// <summary>Fetch data from WebMethod and bind to the dropdown</summary>
            /// <param>url, dropdown element, value field, text field</param>
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

                },
                error: function (xhr, status, error) {
                    console.error("Error fetching dropdown data:", error);
                }
            });
        }

        function bindDropdown($dropdown, options) {
            /// <summary>Binds data to a dropdown</summary>
            /// <param>dropdown element, options</param>
            /// <return type = "void">Void.</return>
            $dropdown.empty(); // Clear existing options
            options.forEach(option => {
                $dropdown.append(`<option value="${option.value}">${option.text}</option>`);
            });
        }

        function toggleControls(enabled) {
            /// <summary>Toggles control enable/disable</summary>
            /// <param>enabled</param>
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

        this.getData = function () {
            /// <summary>Fetches data from the server based on case_key and loads/populates the panel.</summary>
            /// <param></param>
            /// <return type="void">Void.</return>
            if (typeof settings.read === "function") {
                var requestData = {
                    KEY: this.dataKey,
                    tableName: settings.tableName,
                    fields: settings.controls.map(control => control.dataField)
                };

                settings.read(requestData, loadData, handleError);
            }
        };

        this.saveData = function () {
            /// <summary>Save control panel updated data by posting it to the web method</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
            if (typeof settings.update === "function") {
                var formData = {
                    fieldDataList: this.getControlData()
                };

                settings.update(formData, handleSuccess, handleError);
            } else {
                console.error("Update function is not defined.");
            }
        };

        this.getControlData = function () {
            /// <summary>Returns updated control panel input data</summary>
            /// <param></param>
            /// <return type="Array">Array of FieldData objects</return>
            var controlData = [];
            settings.controls.forEach(control => {
                controlData.push({
                    FieldName: control.dataField,
                    Value: $(`#${control.id}`).val()
                });
            });
            return controlData;
        };

        function handleSuccess() {
            alert("Data saved successfully.");
        }

        function handleError(error) {
            console.error("Error:", error);
            alert("An error occurred. Please try again.");
        }

        /// Initialize the plugin
        init();

        return this;
    };

})(jQuery);
