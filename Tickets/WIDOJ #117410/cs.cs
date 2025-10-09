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
    setTimeout("Timer_EnableDisableOKButton()", 2000);
}



protected void btnConfirmUpdate_Click(object sender, EventArgs e)
{
    // save text in txtConfirmUpdate
    if (txtConfirmUpdate.Text.Trim().Length > 0)
    {
        hdnConfirmUpdate.Value = txtConfirmUpdate.Text;
        if (((Button)sender).ID == "btnConfirmDelete")
            PLCButtonPanel1.ClickDeleteButton();
        else
            PLCButtonPanel1.ClickSaveButton();
    }
    else
    {
        PLCButtonPanel1.ClickCancelButton();
    }
}


protected Boolean DoButtonEvent(string btnname, string btnaction)
{
    if (PLCButtonClick == null) return true;

    Boolean recordadded = false;
    if (ViewState["AddMode"] == null)
        recordadded = false;
    else
        recordadded = ((string)ViewState["AddMode"] == "T");

    PLCButtonClickEventArgs e = new PLCButtonClickEventArgs(btnname, btnaction, false, recordadded);

    try { PLCButtonClick(this, e); }
    catch (Exception ex)
    {
        const string DEVDEBUG = "DEVDEBUG";
        var appSettings = System.Configuration.ConfigurationManager.AppSettings;
        if (appSettings.AllKeys.Contains(DEVDEBUG))
        {
            string errorLog = string.Format("ButtonPanel: {0} - {1}\nError: {2}\n{3}",
                btnname,
                btnaction,
                ex.Message,
                ex.StackTrace);
            PLCSession.ForceWriteDebug(errorLog, true);
            this.SetSaveError("An error has occurred, please check debug log");
        }

        return false;
    }
    if (btnaction.ToUpper() == "AFTER")
    {
        String eventName = "DBButtonPanelButtonClick";
        String js = "_doDBPanelButtonEvent('" + btnname + "');";
        ScriptManager.RegisterStartupScript(Parent.Page, typeof(Page), eventName, js, true);
    }

    return (!e.button_canceled);
}