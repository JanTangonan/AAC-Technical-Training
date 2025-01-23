function PopulateGrid(e, parentTableID, rowPrefix, fieldSuffix) {
    var rows = $("#" + parentTableID).find("[id*=" + rowPrefix + "]");

    for (var i = 0; i < rows.length; i++) {
        var $item = e[i];
        var $allFields = $(rows[i]).find("[id*=__" + fieldSuffix + i + "]");
        var $chkbox = $(rows[i]).find("[id^=CHKBOX_" + fieldSuffix + i + "]");

        for (var fieldName in $item) {
            var $field = $(rows[i]).find("[id^=" + fieldName + "][id$=__" + fieldSuffix + i + "]");
            var value = $item[fieldName];

            if (fieldSuffix == "SEF" && fieldName == "RESUBMISSION" && value == "T" && $chkbox.length > 0)
                $chkbox.prop("checked", "true");

            if (fieldSuffix == "ENEF" && fieldName == "RESUBMISSION" && value == "T" && $chkbox.length > 0)
                $chkbox.prop("checked", "true");

            if (fieldName == "ITEM_NUMBER" && value != "") {
                var $oldItemNumField = $(rows[i]).find("[id^=OLD_ITEM_NUMBER][id$=__" + fieldSuffix + i + "]");
                $oldItemNumField.val(value);

                var $btnAttach = $(rows[i]).find("[id^=ITEM_ATCH_BTN]");
                UpdateAttachmentButton($btnAttach, "", "", value);
            }

            if (fieldName == "ITEM_TYPE_CODE" && value != "") {
                var $oldItemTypeField = $(rows[i]).find("[id^=OLD_ITEM_TYPE_CODE][id$=__" + fieldSuffix + i + "]");
                $oldItemTypeField.val(value);   
            }

            if (fieldName == "ATTRIBUTES" && value != "") {
                var $attrsave = $(rows[i]).find("[id^=ITEM_ATTR_SAVE][id$=__" + fieldSuffix + i + "]");
                $attrsave.val(value);
            }

            if (fieldName == "ITEMLINK" && value != "") {
                var $nameLink = $(rows[i]).find("[id^=ITEM_LINK_SAVE][id$=__" + fieldSuffix + i + "]");
                $nameLink.val(value);
            }

            if ($field.length > 0) {
                var fieldID = $field.attr("id");

                if (fieldID.indexOf("fbLookup") > -1)
                    $field.setCode(value);
                else
                    $field.val(value);
            }
        }
    }
}