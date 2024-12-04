(function ($) {
    // Plugin defaults
    $.fn.DbGrid.defaults = {
        columns: [],
        read: null,
        tableId: null,
        onRendered: null,
        DataKey: null
    };

    $.fn.DbGrid = function (options) {
        const $container = $(this);

        // Extract data-attributes from the container
        const dataOptions = extractDataAttributes($container);

        // Merge options: JSON -> data-attributes -> defaults
        const settings = $.extend({}, $.fn.DbGrid.defaults, dataOptions, options);

        function init() {
            renderGrid();

            if (settings.read) {
                fetchData();
            }
        }

        function renderGrid() {
            const table = $(`<table id="${settings.tableId}" class="db-grid"></table>`);
            const thead = $('<thead><tr></tr></thead>');

            settings.columns.forEach(col => {
                const th = $(`<th>${col.title}</th>`);
                thead.find('tr').append(th);
            });

            table.append(thead);
            table.append('<tbody></tbody>');
            $container.empty().append(table);
        }

        function fetchData() {
            settings.read(
                function (data) {
                    bindTableData(data);

                    if (typeof settings.onRendered === "function") {
                        settings.onRendered();
                    }
                },
                function (error) {
                    console.error("Error fetching data for grid:", error);
                }
            );
        }

        function bindTableData(data) {
            const $tbody = $container.find('tbody');
            $tbody.empty();

            data.forEach(item => {
                const row = $('<tr></tr>').data('record', item);

                settings.columns.forEach(col => {
                    const cellValue = item[col.name] || '';
                    row.append(`<td>${cellValue}</td>`);
                });

                $tbody.append(row);
            });
        }

        this.reloadData = function () {
            fetchData();
        };

        this.getFirstRow = function () {
            return $container.find('tbody tr:first');
        };

        this.getSelectedRowKey = function ($row) {
            return $row.data('record')?.[settings.DataKey];
        };

        this.editMode = function () {
            $container.removeClass('disabled');
        };

        this.browseMode = function () {
            $container.addClass('disabled');
        };

        // Extract data-attributes
        function extractDataAttributes($el) {
            const attributes = {};
            $.each($el[0].attributes, (index, attr) => {
                if (attr.name.startsWith('data-')) {
                    const key = attr.name.slice(5).replace(/-./g, match => match.charAt(1).toUpperCase()); // Convert to camelCase
                    attributes[key] = parseAttributeValue(attr.value);
                }
            });
            return attributes;
        }

        // Parse data-attribute values (e.g., JSON strings, numbers)
        function parseAttributeValue(value) {
            try {
                return JSON.parse(value); // Parse JSON
            } catch (e) {
                return value; // Return as string if not JSON
            }
        }

        init();

        return this;
    };
})(jQuery);
