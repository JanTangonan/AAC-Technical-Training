bindDeleteCustodyHandlerWhenReady()


setTimeout(function () {
    var $deleteCustodyOk = $("[id$='<%= mbDeleteCustody.ClientID %>ob']");
    var $deleteCustodyCancel = $("[id$='<%= mbDeleteCustody.ClientID %>cb']");
    $deleteCustodyOk.click(function() { 
        // workaround to disable the buttons since 
        // attr("disabled") also prevent the postback
        $deleteCustodyOk.css({ 
            "pointer-events": "none",
            "opacity": "0.6" 
        });
        $deleteCustodyCancel.css({ 
            "pointer-events": "none",
            "opacity": "0.6" 
        });
     });
}, 300);

setTimeout(function () {
    var $deleteCustodyOk = $("[id$='<%= mbDeleteCustody.ClientID %>ob']");
    var $deleteCustodyCancel = $("[id$='<%= mbDeleteCustody.ClientID %>cb']");

    console.log($deleteCustodyOk.length);
    console.log($deleteCustodyCancel.length);

    $deleteCustodyOk.on("click", function () {
    $deleteCustodyOk.css({
            "pointer-events": "none",
            "opacity": "0.6"
    });
    $deleteCustodyCancel.css({
            "pointer-events": "none",
            "opacity": "0.6"
    });
});
}, 500);


$deleteCustodyOk.on("click", function () {
    $deleteCustodyOk.css({
        "pointer-events": "none",
        "opacity": "0.6"
    });
    $deleteCustodyCancel.css({
        "pointer-events": "none",
        "opacity": "0.6"
    });
});

var $deleteCustodyOk = $("[id$='<%= mbDeleteCustody.ClientID %>ob']");
var $deleteCustodyCancel = $("[id$='<%= mbDeleteCustody.ClientID %>cb']");
$deleteCustodyOk.click(function () {
    // workaround to disable the buttons since 
    // attr("disabled") also prevent the postback
    $deleteCustodyOk.css({
        "pointer-events": "none",
        "opacity": "0.6"
    });
    $deleteCustodyCancel.css({
        "pointer-events": "none",
        "opacity": "0.6"
    });
});