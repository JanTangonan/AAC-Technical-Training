function GetPrelogNames(params) {
    _namesInitializtionDone = false;
    $.ajax({
        type: "POST",
        url: "PLCWebCommon/PLCWebMethods.asmx/GetPrelogNames",
        data: params,
        success: function (data) {
            LoadNamesGrid(data);
            setPageTabIndexes();
        },
        error: function () {
            alert("failed");
        },
        dataType: "json"
    });

}