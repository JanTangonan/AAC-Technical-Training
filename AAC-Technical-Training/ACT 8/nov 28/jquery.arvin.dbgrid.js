(function ($) {
	/// GRID PLUGIN ///
	$.fn.DbGrid = function (options) {
		// Default settings
		const settings = $.extend({
		    columns: [],
		    read: null,
		    tableId: null,
			onRendered: null 
		}, options);

		const $container = $(this);

		function init() {
			/// <summary>Initialize the grid</summary>
			/// <param></param>
			/// <return type = "void">Void.</return>
			renderGrid();

			if (settings.read) {
			    fetchDataUsingRead();
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

		function fetchDataUsingRead() {
		    /// <summary>Fetch data using the read function</summary>
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
		    fetchDataUsingRead();
		};

		this.getFirstRow = function () {
		    /// <summary>Gets the first row in the grid</summary>
		    const $table = $(`#${settings.tableId}`);
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