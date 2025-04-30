//TAB4Items.aspx
function expandTextBox(e, textbox, defaultTitle, zindex, container) {
    // Expands textbox
    if ($(textbox).attr('id') == 'expandMemo')
        return;
    var $panelBlk = $(textbox).closest('.dbpanelblkv2');
    var $parentBlk = $panelBlk.length > 0 ? $panelBlk : $(textbox).closest('table').closest('div');
    var $parentDialog = $parentBlk.closest(".ui-dialog-content");
    var zIndex = zindex ? zindex : 10;
    if ($parentDialog.length && $parentDialog.dialog("option", "modal"))
        zIndex = $parentDialog.closest(".ui-dialog").css("zIndex");
    var $expandModal = $('<div class="ui-dialog ui-widget ui-widget-content ui-corner-all" id="expandModal"></div>').css('width', $parentBlk[0].offsetWidth).css('height', (container ? $("[id*=" + container + "]").height() : ($parentBlk[0].offsetHeight < 200 ? '200px' : $parentBlk[0].offsetHeight))).css('z-index', zIndex || 10)
            .css("top", "auto").css("left", "auto");
    var $expandTitle = $('<div class="ui-dialog-titlebar ui-widget-header ui-corner-all ui-helper-clearfix"><span class="ui-dialog-title" id="expandTitle">&nbsp;</span><a role="button" class="ui-dialog-titlebar-close ui-corner-all" href="#"><span class="ui-icon ui-icon-minusthick" style="cursor: hand;">close</span></a></div>');
    $expandTitle.find('a').click(function () {
        var memoVal = $('#expandModal').find("#expandMemo").text();
        $('#expandModal').remove();
        $(textbox).text(memoVal);
        $(textbox).removeAttr('disabled');
        $(textbox).change();
        $(textbox).focus();

    });
    var $label = $(textbox).closest('td').prev().find('.prompt').text().replace('*', '');
    $expandTitle.find('#expandTitle').text($label || defaultTitle);
    $expandModal.append($expandTitle);
    $expandModal.insertBefore($parentBlk);
    var $memo = $(textbox).clone().attr('id', 'expandMemo').attr('name', 'expandMemo').css('width', '100%').css('height', ($expandModal.height() - $expandTitle.height() - 12).toString() + 'px');
    $memo.bind("keyup input change", function () {
        $(textbox).val($(this).val());
    });
    $memo.focus(function () {
        $(textbox).attr('disabled', 'disabled');
    });
    $memo.blur(function () {
        $(textbox).removeAttr('disabled');

         if ($memo.attr("memocounter")) {
                let expandMemoCounter = $(this).parent().find('.dbpanel-memo-length').html();
                $(textbox).parent().find('.dbpanel-memo-length').html(expandMemoCounter);

                //to fix the issue with expanded memo counter with mask
                $(textbox).attr("memo-blur-counter", expandMemoCounter);
        }
    });
    $memo.val($(textbox).val());
    if (!!$(textbox).attr('readonly'))
        $memo.attr('readonly', 'readonly');
    $expandModal.append($memo);
    // disable to prevent validation on blur when the focus is switced to expanded modal
    $(textbox).attr('disabled', 'disabled');
    $memo.focus();

    if ($memo.attr("memocounter")) {    // MEMOCOUNTER

        let memocounter = $memo.attr("memocounter").split("|");
        let bgcolor = memocounter[0];
        let maxlength = memocounter[1];

        appendMemoCounter($memo, bgcolor, maxlength);

    }
}

//PLCDBPanel.js
function appendMemoCounter($memofield, bgcolor, maxlength) {
    let memoWidth = $memofield.css("width");
    let length = $memofield.val().length;

    if ($memofield.hasClass("MaskedEditFocus"))
        length = $memofield.val().replace(/(^_+|_+$)/g, "").length; //remove leading and trailing underscore

    let spanhmtl = length + "/" + maxlength;
    let span = $('<span></span>').addClass('dbpanel-memo-length').css("background-color", bgcolor).html(spanhmtl);  
    let divMemoCounter = $('<div></div>').addClass("dbpanel-memo-flex").css("width", memoWidth).append(span);

    if ($memofield.parent() && !$memofield.parent().find(".dbpanel-memo-flex").length) {
        $memofield.after(divMemoCounter);
        $memofield.css("margin-bottom", "-10px");
        $memofield.css("width", memoWidth);
    }
}