$(document).ready(function () {
    // Target both number range fields inside the repeater
    $(document).on('blur', '[id$="txtNumberStart"], [id$="txtNumberEnd"]', function () {
        console.log("Blured!");
        var $txt = $(this);
        var val = $txt.val().trim();
        if (val === "") return;

        var extenderId = $txt.attr('id').replace('txtNumber', 'meeNumber');
        console.log("extenderId: " + extenderId);
        var $extender = $('[id$="' + extenderId + '"]');
        console.log("$extender: " + $extender);

        var mask = $extender.attr('mask') || "";
        console.log("mask: " + mask);
        var isDecimal = mask.includes('.');
        console.log("isDecimal: " + isDecimal);

        if (isDecimal) {
            var num = parseFloat(val);
            if (!isNaN(num)) {
                // Determine decimal count based on mask, e.g. "9.99" → 2 decimals
                var decimals = mask.split('.')[1]?.length || 2;
                console.log("decimals: " + decimals);
                $txt.val(num.toFixed(decimals));
            }
        }
    });
});


// version 2
$(document).ready(function () {
    $(document).on('blur', '[id$="txtNumberStart"], [id$="txtNumberEnd"]', function () {
        console.log("Blured!");
        var $txt = $(this);
        var val = $txt.val().trim();
        if (val === "") return;

        var mask = String($txt.data('mask') || "");
        console.log("mask: " + mask);

        var isDecimal = mask.includes('.');
        console.log("isDecimal: " + isDecimal);

        if (isDecimal) {
            var num = parseFloat(val);
            if (!isNaN(num)) {
                var decimals = mask.split('.')[1]?.length || 2;
                console.log("decimals: " + decimals);
                setTimeout(function () {
                    console.log(num.toFixed(decimals));
                    $txt.val(num.toFixed(decimals));
                }, 10);
            }
        }
    });
});

// version 3
$(document).ready(function () {
    $(document).on('blur', '[id$="txtNumberStart"], [id$="txtNumberEnd"]', function () {
        console.log("Blured!");
        var $txt = $(this);
        var val = ($txt.val() || "").trim();
        if (val === "") return;

        var mask = String($txt.data('mask') || "");
        console.log("mask:", mask);

        var isDecimal = (mask.indexOf('.') !== -1);
        if (!isDecimal) return;

        var num = parseFloat(val);
        if (isNaN(num)) return;

        var decimals = (mask.split('.')[1] && mask.split('.')[1].length) ? mask.split('.')[1].length : 2;

        // 🧩 Prevent reformatting if value already matches decimal precision
        var parts = val.split('.');
        if (parts.length === 2 && parts[1].length === decimals) {
            console.log("Already formatted:", val);
            return;
        }

        setTimeout(function () {
            var formatted = num.toFixed(decimals);
            console.log("Formatted:", formatted);
            $txt.val(formatted);
            $txt.trigger('input').trigger('change');
        }, 10);
    });
});
