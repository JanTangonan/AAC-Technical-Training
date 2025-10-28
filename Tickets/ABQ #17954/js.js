function getKitComponentGridData(gridClientId) {
    var columnIndices = {
        0: 'code',
        1: 'description',
        2: 'lotNumber',
        3: 'expirationDate',
        4: 'qcDate',
        5: 'quantity',
        6: 'units'
    };

    var getTdInnerHtml = function (td) {
        return td.innerHTML
    }
    var getTdInputValue = function (td) {
        var input = $(td).find('input')
        if (input.length > 0) {
            return input[0].value;
        }
    }

    var getExpirationDateRequired = function (td) {
        var input = $(td).find('input[id$=hdnExpDateRequired]');
        if (input.length > 0) {
            return input[0].value === 'T';
        }
    }
    var columnCallbacks = {
        code: getTdInnerHtml,
        description: getTdInnerHtml,
        lotNumber: getTdInputValue,
        expirationDate: getTdInputValue,
        qcDate: getTdInputValue,
        quantity: getTdInputValue,
        units: getTdInputValue
    };

    var rowVals = [];

    $("#" + gridClientId)
        .find("tr")
        .filter(function (idx, tr) {
            // exclude header row
            return idx !== 0;
        })
        .each(function (idx, tr) {
            var columnVals = {}
            console.log('Row ' + idx + ' has ' + $(tr).find('td').length + ' cells');
            $(tr).find('td').each(function (idx, td) {
                console.log(idx, $(td).html());
                var column = columnIndices[idx];
                var val = columnCallbacks[column](td);
                columnVals[column] = val

                if (column === 'expirationDate') {
                    columnVals['expirationDateRequired'] = getExpirationDateRequired(td);
                }
            });
            rowVals.push(columnVals);
        });

    return rowVals
}

mpeChemInvKit.Show();
ScriptManager.RegisterStartupScript(
    this,
    GetType(),
    "EnableSaveButton",
    "$('#" + btnSaveComp.ClientID + "').removeClass('button-disabled');",
    true
);

Sys.Application.add_load(function () {
    var panel = document.getElementById('<%= pnlMainChemInvKit.ClientID %>');
    var btnSave = document.getElementById('<%= btnSaveComp.ClientID %>');

    var observer = new MutationObserver(function (mutations) {
        if ($(panel).is(":visible")) {
            btnSave.classList.remove("button-disabled");
            btnSave.disabled = false;
        }
    });

    observer.observe(panel, { attributes: true, attributeFilter: ['style'] });
});
