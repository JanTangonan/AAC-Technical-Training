<div id="dialog-regenReason" style="padding: 10px; display: none;">
    <div id="dialog-regenReason-content">
        <p style="font-size: 14px; margin: 0px 0px 6px 0px;">Why are you regenerating this document?</p>
        <PLC:FlexBox ID="fbRegenReason" runat="server" TableName="TV_PROCREGN"
            OnValueChangedScript="updateRegenReasonMemo" />
        <asp:TextBox ID="txtRegenReason" runat="server" TextMode="MultiLine" Rows="7"
            style="width: 100%; margin-top: 10px;" />
    </div>
</div>

function updateRegenReasonMemo() {
    var $fbRegenReason = $("[id$='fbRegenReason']").find("input:visible");
    var $txtRegenReason = $("[id$='txtRegenReason']");
   
    var selectedReason = $fbRegenReason.val();
    if ($fbRegenReason.length > 0 && selectedReason.length > 0) {
        var reasonMemoVal = $txtRegenReason.val();
        if (reasonMemoVal.length > 0) {
            reasonMemoVal += "\n"
        }
        reasonMemoVal += selectedReason;
        $txtRegenReason.val(reasonMemoVal);
    }
}

function openRegenReasonPopup() {
    $("[id='dialog-regenReason']:eq(1)").dialog("destroy").remove();
    $("#dialog-regenReason").dialog({
        autoOpen: true,
        modal: true,
        resizable: false,
        draggable: true,
        closeOnEscape: false,
        noClose: true,
        title: 'Confirmation',
        minWidth: 500,
        buttons: {
            "OK": function () {
                $("[id$='hdnRegenReason']").val($("[id$='txtRegenReason']").val());
                $("[id$='btnRegenReasonDummy']").click();
            },
            "Cancel": function () {
                $(this).dialog("close");
                __doPostBack();
            }
        },
        open: function () {
            $(this).parent().find('.ui-dialog-titlebar-close').hide();
            $("[id$='txtRegenReason']").val("");
        }
    });
}

<asp:Button ID="btnRegenReasonDummy" runat="server" Text="Button" style="display: none;" 
    OnClick="btnRegenReasonDummy_Click" UseSubmitBehavior="false" 
    CausesValidation="false" />

// protected void btnRegenReasonDummy_Click(object sender, EventArgs e)
// {
//             string reasonForRegen = hdnRegenReason.Value.ToString();
//     reasonForRegen = reasonForRegen.Replace("\n", "\\n").Replace("\\", "\\\\").Replace("'", "\\'");

//     if ((string)Session["SignOCXcalled"] == "standby")
//     Session["SignOCXcalled"] = "T";

//             string returnPage = "Dashboard.aspx";
//     if (Request.UrlReferrer != null)
//         returnPage = Request.UrlReferrer.AbsolutePath.Substring(Request.UrlReferrer.AbsolutePath.LastIndexOf("/") + 1);

//     WebOCX.OnSuccessScript = "CheckBrowserWindowID(setTimeout(function () { ShowLoading();  window.location = '" + returnPage + "'; }));";
//     WebOCX.CreateNotesPacket(PLCSession.PLCGlobalAssignmentKey, reasonForRegen);
// }



//
function updateRegenReasonMemo() {
    var $fbRegenReason = $("[id$='fbRegenReason']").find("input:visible");
    var $txtRegenReason = $("[id$='txtRegenReason']");
   
    var selectedReason = $fbRegenReason.val();
    console.log(selectedReason);
    if ($fbRegenReason.length > 0 && selectedReason.length > 0) {
        var reasonMemoVal = $txtRegenReason.val();
        var lowerMemo = reasonMemoVal.toLowerCase();
        var lowerSelected = selectedReason.toLowerCase();

        if (!lowerMemo.includes(lowerSelected)) {
            if (reasonMemoVal.length > 0) {
                reasonMemoVal += "\n";
            }
            reasonMemoVal += selectedReason;
            console.log(reasonMemoVal);
            $txtRegenReason.val(reasonMemoVal);
        }
    }
}