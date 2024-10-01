$(document).ready(() => {
    // Highlight row on click
    $('.cases-table tr').click(function () { 
        $('.cases-table tr').removeClass('table-row-color');
        $(this).addClass('table-row-color');
    });

    
});