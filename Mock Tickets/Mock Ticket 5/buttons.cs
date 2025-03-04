if (PLCShowAddButton)
{

    //add
    mybutton = new Button();
    mybutton.Width = Unit.Point(btnwidth);
    mybutton.ID = this.ID + "_DBP_ADDBUTTON";
    mybutton.Text = PLCSession.GetSysPrompt("ButtonPanel.Add", "Add");
    mybutton.Click += addbutton_Click;
    tabIndex++;
    mybutton.TabIndex = tabIndex;
    tc.Controls.Add(mybutton);
    //hrwidth = 65;
}

if (PLCShowEditButtons)
{

    //Edit

    mybutton = new Button();
    mybutton.AccessKey = "E";
    mybutton.Width = Unit.Point(btnwidth);
    mybutton.ID = this.ID + "_DBP_EDITBUTTON";
    mybutton.Text = PLCSession.GetSysPrompt("ButtonPanel.Edit", "Edit");
    mybutton.Click += editbutton_Click;
    tabIndex++;
    mybutton.TabIndex = tabIndex;
    tc.Controls.Add(mybutton);


    //Save

    mybutton = new Button();
    mybutton.Width = Unit.Point(btnwidth);
    mybutton.ID = this.ID + "_DBP_SAVEBUTTON";
    mybutton.AccessKey = "S";
    mybutton.Text = PLCSession.GetSysPrompt("ButtonPanel.Save", "Save");
    mybutton.Click += savebutton_Click;
    tabIndex++;
    mybutton.TabIndex = tabIndex;
    mybutton.OnClientClick = "this.disabled = true; this.value = 'Wait...';";
    mybutton.UseSubmitBehavior = false;
    tc.Controls.Add(mybutton);
    if (PLCSetDefaultButton)
        SetPanelDefaultButton(mybutton);

    // Hidden session autosave button.
    if (PLCSession.GetWebConfiguration("DBPANEL_ISAUTOSAVE") == "T")
    {
        Button hdnbtnAutoSave = new Button();
        hdnbtnAutoSave.ID = this.ID + "_DBP_SESSIONAUTOSAVEBUTTON";
        hdnbtnAutoSave.Text = "AutoSave";
        hdnbtnAutoSave.Click += sessionautosavebutton_Click;
        hdnbtnAutoSave.UseSubmitBehavior = false;
        hdnbtnAutoSave.Style.Add("display", "none");
        tc.Controls.Add(hdnbtnAutoSave);
    }

    //Cancel

    mybutton = new Button();
    mybutton.Width = Unit.Point(btnwidth);
    mybutton.ID = this.ID + "_DBP_CANCELBUTTON";
    mybutton.Text = PLCSession.GetSysPrompt("ButtonPanel.Cancel", "Cancel");
    mybutton.AccessKey = "C";
    mybutton.Click += cancelbutton_Click;
    tabIndex++;
    mybutton.TabIndex = tabIndex;

    //Ticket#37756 - Cancel button in PLCButtonPanel shouldn't act like a default submit button.
    mybutton.UseSubmitBehavior = false;

    tc.Controls.Add(mybutton);

    //hrwidth += 195;
}


//Delete

if (PLCShowDeleteButton)
{

    mybutton = new Button();
    mybutton.Width = Unit.Point(btnwidth);
    mybutton.ID = this.ID + "_DBP_DELETEBUTTON";
    mybutton.Text = PLCSession.GetSysPrompt("ButtonPanel.Delete", "Delete");
    mybutton.Click += deletebutton_Click;
    mybutton.OnClientClick = "this.disabled = true; this.value = 'Wait...';";
    mybutton.UseSubmitBehavior = false;
    tabIndex++;
    mybutton.TabIndex = tabIndex;
    tc.Controls.Add(mybutton);


    PLCDBPanel Panel = (PLCDBPanel)this.Parent.FindControl(PLCTargetControlID);
    if (Panel != null)
        if (Panel.PLCDataTable != "TV_LABITEM")
        {
            // Add the "Are you sure?" extender
            ConfirmButtonExtender cbe = new ConfirmButtonExtender();
            cbe.TargetControlID = mybutton.ID;
            cbe.ConfirmText = PLCDeleteMessage;
            tc.Controls.Add(cbe);
        }


    mybutton.UseSubmitBehavior = false;

    //hrwidth += 65;
}

//hrwidth += 130;
//*AAC - 06/02/2010 - set a fixed width to custom buttons. If text is too long, ScaleToFit() will adjust the width.
if (PLCCustomButtons != "")
{
    Literal mySpacer = new Literal();
    mySpacer.Text = "&nbsp;&nbsp;&nbsp;&nbsp;";
    tc.Controls.Add(mySpacer);

    char[] DelimChars = { ',' };
    string[] btnnames = PLCCustomButtons.Split(DelimChars);

    foreach (string s in btnnames)
    {
        mybutton = new Button();
        //mybutton.Width = Unit.Point(btnwidth);
        mybutton.ID = this.ID + "_DBP_Custom_" + s;
        mybutton.Text = PLCSession.GetSysPrompt("ButtonPanel." + s.Replace(" ", "_"), s);
        mybutton.CommandName = s;
        mybutton.Width = Unit.Point(PLCCustomButtonFixedWidth);
        //                    ScaleToFit(mybutton, s, new Font(mybutton.Font.Name, 10));
        ScaleToFit(mybutton, s, new Font(mybutton.Font.Name, 14));
        mybutton.Click += custombutton_Click;
        tabIndex++;
        mybutton.TabIndex = tabIndex;
        tc.Controls.Add(mybutton);
        //hrwidth += Convert.ToInt32(mybutton.Width.Value);
    }
}