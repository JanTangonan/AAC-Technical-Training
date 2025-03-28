<div class="hide">
    <div class="dashboard-panels">
        <div class="dashboard-panels-item">
            <div>
                <b>Dashboard Panels</b>
                <ul id="dashboardPanelList" class="dashboard-panels-item"></ul>
            </div>
        </div>
        Drag & drop to sort the dashboard panels.
    </div>
</div>

function showSortDashBoardPanels(dashboardPanelList) {
    /// <summary>Open the name fields popup</summary>

    initList("dashboardPanelList", dashboardPanelList);

    var liHeight;
    $("#dashboardPanelList").sortable({
        connectWith: ".dashboard-panels-item",
        start: function (e, ui) {
            ui.helper.height(liHeight);
        }
    }).disableSelection();

    var $parent = $(".dashboard-panels").parent();
    $(".dashboard-panels").dialog({
        closeOnEscape: false,
        autoOpen: true,
        modal: true,
        resizable: false,
        draggable: true,
        title: 'Sort Dashboard Panels',
        width: 700,
        appendTo: "form",
        buttons: {
            'Reset to Default': function () {
                savedDashboardPanelsSort(true);
                $(this).dialog("close");
            },
            Save: function () {
                savedDashboardPanelsSort(false);
                $(this).dialog("close");
            },
            Cancel: function () {
                $(this).dialog("close");
            }
        },
        open: function () {
            $(".ui-dialog-titlebar-close", $(this).parent()).hide(); // Remove close icon
            $('.ui-dialog-buttonpane button:contains("Save")').focus();

            liHeight = $(".dashboard-panels li").outerHeight();
        },
        close: function () {
            // Append to original parent to avoid duplicate ids
            $(this).parent().appendTo($parent);
        }
    });

    //#region list functions
    function initList(id, list) {
        /// <summary>Initialize the sortable list</summary>

        var $list = $("#" + id);
        for (var item in list) {
            var $li = $("<li/>").text(list[item]);
            $li.attr("data-field", item);
            $li.addClass("ui-state-default");
            $list.append($li);
        }
    }

    function savedDashboardPanelsSort(resetToDefault) {
        /// <summary>Save the global setting for the sequencing of dashboard panels</summary>

        var panels = $("#dashboardPanelList li").map(function () {
            return $(this).attr("data-field");
        }).get();

        var param = {
            resetToDefault : resetToDefault,
            panels: panels
        };

        $.ajax({
            type: "POST",
            url: "ConfigTAB4Defaults.aspx/SaveDashboardPanelsSort",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(param),
            success: function (data) {
                var result = data.d;
                if (result.saved) {
                    Alert("Dashboard panels display order is saved.");
                } else if (result.resetToDefault) {
                    Alert("Default setting of dashboard panels display order will be used.");
                } else {
                    Alert("Failed to save. " + result.message);
                }
            },
            error: function () {
                Alert("An error occurred while saving.");
            },
            dataType: "json"
        });
    }
    //#endregion list functions
}

