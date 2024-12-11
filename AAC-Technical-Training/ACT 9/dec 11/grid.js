(function ($) {
    /// GRID PLUGIN ///
    $.fn.DbGrid = function (options) {
        /// Default settings
        var defaults = {
            columns: [],
            read: null,
            tableId: null,
            onRendered: null,
            dataKey: null
        };

        var $grid = $(this);

        /// options will be the first priority followed by data-attributes then default values
        var settings = $.extend({}, defaults, $grid.data(), options);

        function init() {
            if ($grid.data("read") && (!options || !options.read)) {
                /// check if read is missing from options then convert it to a function
                var readUrl = $grid.data("read");
                settings.read = function (onSuccess, onError) {
                    $.ajax({
                        type: "POST",
                        url: readUrl,
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        success: function (response) {
                            var data = JSON.parse(response.d);
                            onSuccess(data);
                        },
                        error: function (xhr, status, error) {
                            onError(error);
                        },
                    });
                };
            }

            /// Capture the table id
            if (!settings.tableId && $grid.attr('id')) {
                settings.tableId = $grid.attr('id'); 
            }

            if (typeof settings.read === "function") {
                console.log("Fetching data using the read function...");
                fetchData();
            } else {
                console.error("settings.read is not defined as a function. Initialization aborted.");
            }

            renderGrid($grid);

            if (settings.read) {
                fetchData();
            }
        }

        function renderGrid($table) {
            /// <summary>Render the grid table</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
            var uniqueClass = `table-${settings.tableId}`;
            $table.addClass(`table ${uniqueClass}`);
            var wrapper = $("<div>", { class: "table-responsive" });
            $table.wrap(wrapper);

            /// Inject styles dynamically 
            var style = `
                .${uniqueClass} {border-collapse: collapse; font-size: 12px; margin: 20px 0; text-align: left}
                .${uniqueClass} th {white-space: nowrap; vertical-align: middle;}
                .${uniqueClass} td {padding: 5px;}
            `;
            $("head").append(`<style>${style}</style>`);

            var thead = $('<thead><tr></tr></thead>');
            settings.columns.forEach(col => {
                thead.find("tr").append($(`<th class="text-center">${col.title}</th>`));
            });
            $table.append(thead);
            $table.append("<tbody></tbody>"); 
        }

        function fetchData() {
            /// <summary>Fetch data using the read function</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
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
            /// <summary>Binds data to the table body.</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
            var $table = $(`#${settings.tableId}`);
            var $tbody = $table.find("tbody");

            $tbody.empty(); // Clear existing rows

            data.forEach(item => {
                var row = $('<tr></tr>');
                row.data('record', item);

                if (settings.dataKey) {
                    row.data('key', item[settings.dataKey]);
                }

                settings.columns.forEach(col => {
                    var cellValue = item[col.name] || '';
                    row.append(`<td>${cellValue}</td>`);
                });

                $tbody.append(row);
            });

            /// Built in click event that captures passes dataKey and highlights row
            $table.off("click", "tbody tr").on("click", "tbody tr", function () {
                var dataKey = settings.dataKey;
                var record = $(this).data('record');
                $(this).addClass("selected").siblings().removeClass("selected");
                $table.trigger("rowSelected", record[dataKey]);
            });
        }

        this.reloadData = function () {
            /// <summary>Rebind grid table</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
            fetchData();
        };

        this.getSelectedRowKey = function (row) {
            /// <summary>Get the DataKey value of the selected row</summary>
            /// <param name="row" type="jQuery">The row object</param>
            /// <return type="string">Value of the DataKey field</return>
            return row.data('key'); // Fetch DataKey from the row
        };

        this.getFirstRow = function () {
            /// <summary>Gets the first row in the grid</summary>
            var $table = $(`#${settings.tableId}`);
            return $table.find("tbody tr:first"); // Returns the jQuery object of the first row
        };

        this.editMode = function () {
            /// <summary>Enable edit mode - enables grid table and enables interactions</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
            $(`#${settings.tableId}`).removeClass('disabled');
        };

        this.browseMode = function () {
            /// <summary>Enable browse mode - disables grid table and disables interactions</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
            $(`#${settings.tableId}`).addClass('disabled');
        };

        /// Initialize the plugin
        init();

        return this;
    };

})(jQuery);