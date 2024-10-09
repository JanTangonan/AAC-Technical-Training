function disableOtherRows(gridViewId, editingRowId) {
    var gridView = document.getElementById(gridViewId);
    var rows = gridView.getElementsByTagName("tr");

    for (var i = 1; i < rows.length; i++) {  // skip the header row
        var row = rows[i];
        if (row.id !== editingRowId) {
            row.style.pointerEvents = 'none'; // Disable other rows
            row.style.opacity = '0.5';        // Optional: visually indicate it's disabled
        }
    }
}

function enableAllRows(gridViewId) {
    var gridView = document.getElementById(gridViewId);
    var rows = gridView.getElementsByTagName("tr");

    for (var i = 1; i < rows.length; i++) {
        var row = rows[i];
        row.style.pointerEvents = 'auto'; // Enable all rows again
        row.style.opacity = '1';          // Reset opacity
    }
}