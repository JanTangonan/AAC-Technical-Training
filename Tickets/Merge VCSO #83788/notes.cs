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

        