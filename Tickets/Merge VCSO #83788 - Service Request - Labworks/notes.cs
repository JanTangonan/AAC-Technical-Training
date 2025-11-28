/// <summary>
        /// Displays the appropriate details panel
        /// </summary>
        /// <returns>Returns true if no errors are found</returns>
        private bool ShowDetails()
        {
            string panelToDisplay = string.Empty;

            // hide all the details panel
            HideDetailsPanel();
            litDetails.Text = string.Empty;

            // retrieve the panel to display
            panelToDisplay = GetPanelToDisplay();

            // check if the panel is available
            if (IsDBPanelAvailable(panelToDisplay))
            {
                // display appropriate details according to the current view
                switch (CurrentView)
                {
                    case LabWorkView.ServiceRequest:
                        if (trvSR.SelectedNode.Depth == 0) // srmaster
                            DisplaySRMasterDetailPanel(trvSR.SelectedNode.Value);
                        else if (trvSR.SelectedNode.Depth == 1) //srdetail-assignment
                            DisplayAssignmentDetailsPanel(trvSR.SelectedNode.Value.Substring(trvSR.SelectedNode.Value.IndexOf("_") + 1));
                        break;
                    case LabWorkView.Section:
                        if (trvSR.SelectedNode.Depth == 1) //assignment
                            DisplayAssignmentDetailsPanel(trvSR.SelectedNode.Value);
                        break;
                    case LabWorkView.Assignment:
                        if (trvSR.SelectedNode.Depth == 0) //assignment
                            DisplayAssignmentDetailsPanel(trvSR.SelectedNode.Value);
                        else if (trvSR.SelectedNode.Depth == 2) //related assignment
                            DisplayAssignmentDetailsPanel(trvSR.SelectedNode.Value);
                        break;
                }
            }
            else
            {
                ShowDetailsText();
            }

            return true;
        }

/// <summary>
        /// Displays the SR Master Detail panel and set the print button properties for it
        /// </summary>
        /// <param name="srMasterKey">The srmaster key whose details will be displayed</param>
        /// <returns>Returns true if no errors are found</returns>
        private bool DisplaySRMasterDetailPanel(string srMasterKey)
        {
            // set the print button configuration
            SetPrintButtonDefaultConfiguration();

            // load panel record
            dbpSRMasterDetails.PLCWhereClause = string.Format(" WHERE SR_MASTER_KEY = {0}", srMasterKey);
            dbpSRMasterDetails.DoLoadRecord();
            dbpSRMasterDetails.SetBrowseMode();

            if (srMasterKey != string.Empty)
            {
                // set the print button configurations
                btnPrint.Enabled = true;
                btnPrint.CommandArgument = srMasterKey;
            }

            // display the panel 
            divSRMasterDetails.Visible = true;

            return true;
        }

public void EmptyMode()
{
    foreach (PanelRec pr in panelrecs)
    {
        if (pr.btn != null)
            pr.btn.Enabled = false;

        if (pr.tb != null)
        {
            pr.tb.BackColor = Color.LightGray;
            pr.tb.Attributes["readonly"] = "readonly";
            pr.tb.Text = "";

            if (pr.time != null)
            {
                SetTimeTextBoxMode(pr, true);
                pr.time.Text = "";
            }
        }
        else if (pr.ddl != null)
        {
            if (pr.ddl.Items.FindByValue("") != null)
                pr.ddl.SelectedValue = "";
            SetDropDownListMode(pr.ddl, true);
        }
        else if (pr.rbl != null)
        {
            if (pr.rbl.Items.FindByValue("") != null)
                pr.rbl.SelectedValue = "";
            SetRadioButtonListMode(pr.rbl, true);
            //pr.rbl.SelectedIndex = -1;
        }
        else if (pr.cb != null)
        {
            pr.cb.Checked = false;
            pr.cb.Enabled = false;
        }
        else if (pr.chMultiLookup != null)
        {
            TextBox tb = pr.chMultiLookup.GetTextBox();
            tb.BackColor = Color.LightGray;
            tb.ReadOnly = true;

            pr.chMultiLookup.SetText("");
            pr.chMultiLookup.Enable(false);
        }
        else if (pr.chMultipickAc != null)
        {
            TextBox tb = pr.chMultipickAc.GetTextBox();
            tb.BackColor = Color.LightGray;
            tb.ReadOnly = true;

            pr.chMultipickAc.SetText("");
            pr.chMultipickAc.Enable(false);
        }
        else if (pr.chImgMultiLookup != null)
        {
            TextBox tb = pr.chImgMultiLookup.GetTextBox();
            tb.BackColor = Color.LightGray;
            tb.ReadOnly = true;

            pr.chImgMultiLookup.SetText("");
            pr.chImgMultiLookup.Enable(false);
        }
        else if (pr._codetable != "")
        {
            if (pr.chtb != null)
            {
                CodeHeadEnableControls(pr, false);
                pr.chtb.Text = "";
                pr.chlabel.Text = "";
            }
            else if (pr.chComboBox != null)
            {
                SetDropDownListMode(pr.chComboBox, true);
                if (pr.chComboBox.Items.FindByValue("") != null)
                    pr.chComboBox.SelectedValue = "";
            }
            else if (pr.chFlexBox != null)
            {
                pr.chFlexBox.ReadOnly = true;
                pr.chFlexBox.SelectedValue = "";
            }
        }
    }

    SetDBPanelChildRequired();
}