(function ($) {
    /// GRID PLUGIN ///
    $.fn.DbGrid = function (options) {
        // Default settings
        const defaults = {
            columns: [],
            read: null,
            tableId: null,
            onRendered: null,
            dataKey: null
        };

        var $grid = $(this);
        var settings = $.extend({}, defaults, $grid.data(), options);

        console.log("GRID OPTIONS: ",options.read);
        console.log("GRID DATA ATTRIBUTE: ", $grid.data());
        console.log("GRID DEFAULTS: ",defaults);
        console.log("GRID SETTINGS: ",settings);

        function init() {
            /// <summary>Initialize the grid</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
            if ($grid.data("read") && (!options || !options.read)) {
                const readUrl = $grid.data("read");
                console.log(readUrl);
                settings.read = function (onSuccess, onError) {
                    $.ajax({
                        type: "POST",
                        url: readUrl,
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        success: function (response) {
                            const data = JSON.parse(response.d);
                            onSuccess(data);
                        },
                        error: function (xhr, status, error) {
                            onError(error);
                        },
                    });
                };
            }

            if (!settings.tableId && $grid.attr('id')) {
                settings.tableId = $grid.attr('id'); // Capture the table's id
            }
            console.log(settings.tableId);

            if (typeof settings.read === "function") {
                console.log("Fetching data using the read function...");
                fetchData();
            } else {
                console.error("settings.read is not defined as a function. Initialization aborted.");
            }

            renderGrid($grid);

            // Fetch and bind data
            if (settings.read) {
                fetchData();
            }
        }

        function renderGrid($table) {
            /// <summary>Render the grid table</summary>
            /// <param></param>
            /// <return type = "void">Void.</return>
            const uniqueClass = `table-${settings.tableId}`;
            $table.addClass(uniqueClass);

            // Inject CSS dynamically
            const style = `.${uniqueClass} {border-collapse: collapse; font-size: 12px; margin: 20px 0; text-align: left}`;
            $("head").append(`<style>${style}</style>`);

            var thead = $('<thead><tr></tr></thead>');
            settings.columns.forEach(col => {
                thead.find("tr").append($(`<th>${col.title}</th>`));
            });
            $table.append(thead);
            $table.append("<tbody></tbody>"); // Add empty tbody
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
            console.log($table);
            console.log($tbody);

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

            $table.off("click", "tbody tr").on("click", "tbody tr", function () {
                var dataKey = settings.dataKey;
                var record = $(this).data('record');

                console.log(record[dataKey]);
                $(this).addClass("selected").siblings().removeClass("selected");
                // Trigger a custom event with the row data
                $table.trigger("rowSelected", record[dataKey]);
                //$table.trigger("rowSelected", [$(this).data('record')]);
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
            console.log($(`#${settings.tableId}`));
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