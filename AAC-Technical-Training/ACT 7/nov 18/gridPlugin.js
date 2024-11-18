(function ($) {
    $.fn.gridPanel = function (options) {
        var settings = $.extend(
            {
                columns: [],
                data: [],
                hiddenFields: [], // Hidden fields to store as row data attributes
                rowClick: null
            },
            options
        );

        return this.each(function () {
            var $table = $("<table>").addClass("grid-table");
            var $thead = $("<thead>");
            var $tbody = $("<tbody>");

            // Create the table header
            var $headerRow = $("<tr>");
            settings.columns.forEach(function (col) {
                $headerRow.append($("<th>").text(col));
            });
            $thead.append($headerRow);

            // Create the table rows
            settings.data.forEach(function (item) {
                var $row = $("<tr>");

                // Add visible columns
                settings.columns.forEach(function (col) {
                    $row.append($("<td>").text(item[col]));
                });

                // Add hidden fields as data attributes
                settings.hiddenFields.forEach(function (field) {
                    $row.attr(`data-${field.toLowerCase()}`, item[field]);
                });

                // Attach row click event
                if (settings.rowClick) {
                    $row.on("click", function () {
                        var rowData = {};
                        settings.columns.forEach(function (col) {
                            rowData[col] = item[col];
                        });
                        settings.hiddenFields.forEach(function (field) {
                            rowData[field] = item[field];
                        });
                        settings.rowClick(rowData);
                    });
                }

                $tbody.append($row);
            });

            $table.append($thead).append($tbody);
            $(this).empty().append($table);
        });
    };
})(jQuery);