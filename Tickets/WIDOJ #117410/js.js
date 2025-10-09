// add cancel click event in PLCMsgComment
function MsgComment_Cancel(postbackID) {
    if (postbackID.indexOf("btnConfirmDelete") > -1) // for delete click, skip if edit
        LockUnlockRecord("U");
}

"DBButtonPanelButtonClick", "_doDBPanelButtonEvent('DELETE');"

