(function ($) {
    $.fn.gridView = function (options) {
        // Default settings
        const settings = $.extend({
            data: [],
            textBoxSelector: '',
            dropdownSelector: ''
        }, options);

        const $container = $(this);
        let selectedRecord = null;

        // Initialize the grid
        function init() {
            renderGrid();
            populateDropdown();
            bindEvents();
        }

        // Render the grid table
        function renderGrid() {
            const table = $('<table id="gridView"></table>');
            const thead = `<thead>
                            <tr>
                                <th>Department Code</th>
                                <th>Department Name</th>
                                <th>Location</th>
                            </tr>
                          </thead>`;
            const tbody = $('<tbody></tbody>');

            settings.data.forEach(dept => {
                const row = `<tr data-code="${dept.code}" data-name="${dept.name}" data-location="${dept.location}">
                                <td>${dept.code}</td>
                                <td>${dept.name}</td>
                                <td>${dept.location}</td>
                             </tr>`;
                tbody.append(row);
            });

            table.append(thead).append(tbody);
            $container.empty().append(table);
        }

        // Populate the dropdown with department codes
        function populateDropdown() {
            const $dropdown = $(settings.dropdownSelector);
            $dropdown.empty().append('<option value="">Select a record</option>');
            settings.data.forEach(dept => {
                $dropdown.append(`<option value="${dept.code}">${dept.code}</option>`);
            });
        }

        // Bind events for row selection and interaction
        function bindEvents() {
            $('#gridView').on('click', 'tr', function () {
                // Highlight the selected row
                $(this).addClass('selected').siblings().removeClass('selected');

                // Get selected row data
                selectedRecord = {
                    code: $(this).data('code'),
                    name: $(this).data('name'),
                    location: $(this).data('location')
                };

                // Update the text box and dropdown with selected data
                updateInputs();
            });
        }

        // Update text box and dropdown with the selected record
        function updateInputs() {
            if (selectedRecord) {
                $(settings.textBoxSelector).val(selectedRecord.name);
                $(settings.dropdownSelector).val(selectedRecord.code);
            }
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
