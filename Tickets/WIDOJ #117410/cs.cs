else
{
    mInput.ShowMsg(
        "Case update reason",                                           //sTitle
        PLCSession.GetSysPrompt("TAB6Assignments.UPDATE_REASON", "Please enter the reason for your changes"),                    //sMessage
        0,                                                    //nMessageType            
        txtConfirmUpdate.ClientID,                            //ControlID
        btnConfirmDelete.ClientID,                            //PostBackID
        PLCSession.GetSysPrompt("TAB6Assignments.mInput.SAVE", "Save"),         //okButtonText
        PLCSession.GetSysPrompt("TAB6Assignments.mInput.CANCEL", "Cancel"));    //cancelButtonText
    e.button_canceled = true;
    return;
}

public void ShowMsg(string sTitle, string sMessage, int nMessageType, string ControlID, string PostBackID, string okButtonText, string cancelButtonText)
{
    OkButton.Text = okButtonText;
    CancelButton.Text = cancelButtonText;
    SetCommentProperties(sTitle, sMessage, nMessageType, ControlID, PostBackID);
}

private void SetCommentProperties(string sTitle, string sMessage, int nMessageType, string ControlID, string PostBackID)
{
    txtComment.Text = "";
    LiteralControl lcText = new LiteralControl();
    lcText.Text = "&nbsp;&nbsp;" + sTitle;
    MsgCaption.Controls.Add(new LiteralControl("<font color=\"#FFFFFF\" face=\"Arial\" size=\"2\">"));
    MsgCaption.Controls.Add(lcText);
    MsgCaption.Controls.Add(new LiteralControl("</font>"));

    if (nMessageType == 0)
        imgMsg.ImageUrl = "~/Images/msginfo.jpg";
    else if (nMessageType == 1)
        imgMsg.ImageUrl = "~/Images/msgerror.jpg";

    lblMsg.Text = sMessage;
    mpeMsgBox.Show();
    OkButton.Attributes.Add("onclick", "OK_Click('" + ControlID + "', '" + PostBackID + "');");
    CancelButton.Attributes.Add("onclick", "Cancel_Click('" + ControlID + "', '" + PostBackID + "');");
    txtComment.Attributes.Add("onkeydown", "EnableDisableOKButton('" + OkButton.ClientID + "', '" + txtComment.ClientID + "');");
    txtComment.Focus();
}

function EnableDisableOKButton(okID, commentID)
    {        
        bnOkButtonID = okID;
        textboxCommentID = commentID;        
        // use timer to check comment field
        setTimeout("Timer_EnableDisableOKButton()",2000);        
    }