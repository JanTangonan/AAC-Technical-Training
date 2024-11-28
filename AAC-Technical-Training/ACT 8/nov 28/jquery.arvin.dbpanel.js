(function ($) {
	$.fn.dbPanel = function (options) {
		// Default settings
		const settings = $.extend({
		    controls: [],
		    dataSourceUrl: null,
		    read: null, 
		    update: null  
		}, options);

		var $container = $(this);
		this.caseKey = null;

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

		this.getData = function () {
			/// <summary>Fetches data from the server based on case_key and populates the panel.</summary>
		    /// <param></param>
			/// <return type="void">Void.</return>
		    if (typeof settings.read === "function") {
		        console.log(this.caseKey);
		        settings.read(this.caseKey, loadData, handleError);
		    }
		};

		function loadData(data) {
		    /// <summary>Load data into control panel using selected record data</summary>
		    /// <param></param>
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

		this.saveData = function () {
		    /// <summary>Save control panel updated data by posting it to the web method</summary>
		    /// <param></param>
		    /// <return type = "void">Void.</return>
		    if (typeof settings.update === "function") {
		        var formData = {
		            fieldDataList: this.getControlData()
		        };

		        console.log(formData);
		        settings.update(formData, handleSuccess, handleError);
		    } else {
		        console.error("Update function is not defined.");
		    }
		};

		this.getControlData = function () {
			/// <summary>Returns updated control panel input data</summary>
			/// <param></param>
			/// <return type="Array">Array of FieldData objects</return>
			const controlData = [];
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
