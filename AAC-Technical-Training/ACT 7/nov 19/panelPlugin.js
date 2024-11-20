(function ($) {
    $.fn.controlPanel = function (options) {
        // Default settings
        const settings = $.extend({
            controls: [] // Array of control configurations
        }, options);

        const $container = $(this);
        let formData = {};

        // Initialize the control panel
        function init() {
            renderControls();
        }

        // Function to render controls based on the configuration
        function renderControls() {
            settings.controls.forEach(control => {
                const $formGroup = $('<div class="form-group"></div>');
                const $label = $(`<label>${control.label}</label>`);
                let $input;

                if (control.type === 'text') {
                    $input = $(`<input type="text" id="${control.field}" class="control-input">`);
                } else if (control.type === 'date') {
                    $input = $(`<input type="date" id="${control.field}" class="control-input">`);
                }

                $formGroup.append($label).append($input);
                $container.append($formGroup);
            });
        }

        // Function to load data into the controls
        this.loadData = function (record) {
            if (record) {
                settings.controls.forEach(control => {
                    const value = record[control.field];
                    $(`#${control.field}`).val(value);
                });
            }
        };

        // Function to get data from the controls
        this.getData = function () {
            formData = {};
            settings.controls.forEach(control => {
                formData[control.field] = $(`#${control.field}`).val();
            });
            return formData;
        };

        // Initialize the plugin
        init();

        return this;
    };
})(jQuery);

$(document).ready(function () {
    const departments = [
        { code: "HR", name: "Human Resources", date: "2023-12-01" },
        { code: "FIN", name: "Finance", date: "2024-01-15" },
        { code: "IT", name: "Information Technology", date: "2024-02-20" }
    ];

    // Initialize the grid view plugin
    $('#departmentGrid').gridView({
        data: departments
    });

    // Initialize the control panel plugin
    const controlPanel = $('#controlPanel').controlPanel({
        controls: [
            { type: 'text', label: 'Department Name', field: 'name' },
            { type: 'date', label: 'Start Date', field: 'date' }
        ]
    });

    // Load data from the grid view to the control panel on row selection
    $('#departmentGrid').on('click', 'tr', function () {
        const selectedRecord = $('#departmentGrid').gridView().getSelectedRecord();
        controlPanel.loadData(selectedRecord);
    });

    // Example of getting data from the control panel
    $('<button>Get Control Panel Data</button>').appendTo('body').on('click', function () {
        const formData = controlPanel.getData();
        alert(`Form Data:\n${JSON.stringify(formData)}`);
    });
});