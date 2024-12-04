(function ($) {
    $.fn.DbGrid = function (options) {
        // Default settings
        const defaults = {
            columns: [],
            read: null,
            tableId: null,
            onRendered: null,
            DataKey: null
        };

        const $container = $(this);

        // Extract data-attributes and merge settings
        const dataOptions = extractDataAttributes($container);
        const settings = $.extend({}, defaults, dataOptions, options);

        function init() {
            if (!settings.read) {
                console.error("Read method is not defined.");
                return;
            }

            renderGrid();

            if (typeof settings.read === "string") {
                // If `read` is a URL, create a function dynamically
                settings.read = function (onSuccess, onError) {
                    $.ajax({
                        type: "POST",
                        url: settings.read,
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        success: function (response) {
                            const data = JSON.parse(response.d);
                            onSuccess(data);
                        },
                        error: function (xhr, status, error) {
                            console.error("Error fetching data for grid:", error);
                            onError(error);
                        }
                    });
                };
            }

            fetchData();
        }

        function renderGrid() {
            const tableId = settings.tableId || $container.attr("id");
            const table = $(`<table id="${tableId}" class="db-grid"></table>`);
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

        function extractDataAttributes($el) {
            const attributes = {};
            $.each($el[0].attributes, (index, attr) => {
                if (attr.name.startsWith('data-')) {
                    const key = attr.name.slice(5).replace(/-./g, match => match.charAt(1).toUpperCase()); // Convert to camelCase
                    attributes[key] = attr.value;
                }
            });
            return attributes;
        }

        init();
        return this;
    };
})(jQuery);
