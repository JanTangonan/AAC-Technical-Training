(function ($) {
	/// GRID PLUGIN ///
	$.fn.DbGrid = function (options) {
		// Default settings
		const settings = $.extend({
		    columns: [],
		    read: null,
		    tableId: null,
		    onRendered: null,
		    dataKey: null
		}, options);

		var $container = $(this);

		function init() {
			/// <summary>Initialize the grid</summary>
			/// <param></param>
			/// <return type = "void">Void.</return>
			renderGrid();

			if (settings.read) {
			    fetchData();
			}
		}

		function renderGrid() {
			/// <summary>Render the grid table</summary>
			/// <param></param>
			/// <return type = "void">Void.</return>
		    var table = $(`<table id="${settings.tableId}" class="db-grid"></table>`);

		    var thead = $('<thead><tr></tr></thead>');
		    settings.columns.forEach(col => {
		        var th = $(`<th>${col.title}</th>`);
		        thead.find('tr').append(th);
		    });
		    table.append(thead);

		    var tbody = $('<tbody></tbody>');
		    table.append(tbody);

		    $container.empty().append(table);
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
		    var $tbody = $container.find(`#${settings.tableId} tbody`);
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