private void ExecuteButtonPanelEvent(string buttonName)
{
    Panel wrapPanel = null;

    string dupOrigChemControlNo = "";

    //This will look for the wrap panel
    foreach (Control ctrl in cp.Controls)
    {
        if (ctrl is Panel && ctrl.Controls.Count > 0)
        {
            wrapPanel = (Panel)ctrl;

            // Get set of all dbpanels.
            List<PLCDBPanel> dbpanels = new List<PLCDBPanel>();
            foreach (Control PanelCtrl in wrapPanel.Controls)
            {
                if (PanelCtrl is Panel && PanelCtrl.FindControl("dbp" + PanelCtrl.ID) != null && PanelCtrl.ID != "COMPINFO")
                {
                    PLCDBPanel DBPanel = (PLCDBPanel)PanelCtrl.FindControl("dbp" + PanelCtrl.ID);

                    if (DBPanel.ID != "UNKNOWN" && DBPanel.Visible)
                        dbpanels.Add(DBPanel);
                }
            }

            // Go through each editable dbpanel.
            for (int i = 0; i < dbpanels.Count; i++)
            {
                PLCDBPanel DBPanel = dbpanels[i];

                Barcode GetBarcode = new Barcode();
                int ChemControlNumber = (gvMainAsset.Rows.Count > 0 ? Convert.ToInt32(gvMainAsset.SelectedDataKey.Value) : 0);

                switch (buttonName)
                {
                    case "ADD":
                        hdFields.Value = string.Empty;
                        this.hdDupOrigChemControlNo.Value = string.Empty;

                        if (hdCCN.Value == string.Empty)
                        {
                            DBPanel.DoAdd();
                            if (IsCurrentTabPanelType("COMPONENT"))
                            {
                                PLCSessionVars1.SetChemInvProperty<string>("SUB_CHEM_CTRL_NUM", "0");

                                ((ImageButton)cp.FindControl("imgCompLookup")).Enabled = true;
                                ((PLCDBPanel)cp.FindControl("dbpCOMPINFO")).EmptyMode();
                                TextBox tbBarcode = (TextBox)cp.FindControl("tbBarcode");
                                tbBarcode.Style["background-color"] = "white";
                                tbBarcode.Text = "";
                                tbBarcode.ReadOnly = false;

                                PLCDBGrid dbgComponents = (PLCDBGrid)PLCGlobals.PLCCommon.FindControlWithin("dbgComponents", wrapPanel);
                                dbgComponents.Enabled = false;
                                dbgComponents.SelectedIndex = -1;
                            }
                            else if (menu_main.SelectedItem.Text.Trim().ToUpper() == "SERVICE HISTORY")
                            {
                                //ChemControlNumber = Convert.ToInt32(lbxAssetName.SelectedValue);
                                hdStatusKey.Value = Convert.ToString(PLCSessionVars1.GetChemInvProperty<int>("STATUS_KEY", null).ToString());
                                PLCSessionVars1.SetChemInvProperty<int>("STATUS_KEY", null);

                                PLCDBGrid dbgServHistory = (PLCDBGrid)PLCGlobals.PLCCommon.FindControlWithin("dbgServHistory", cp);
                                dbgServHistory.Enabled = false;
                                dbgServHistory.SelectedIndex = -1;
                            }
                            else if (menu_main.SelectedItem.Text.Trim().ToUpper() == "HISTORY OF CUSTODY")
                            {
                                //ChemControlNumber = Convert.ToInt32(lbxAssetName.SelectedValue);
                                hdStatusKey.Value = Convert.ToString(PLCSessionVars1.GetChemInvProperty<int>("STATUS_KEY", null).ToString());
                                PLCSessionVars1.SetChemInvProperty<int>("STATUS_KEY", null);

                                PLCDBGrid dbgHistCustody = FindHistCustodyDBGrid();
                                dbgHistCustody.Enabled = false;
                                dbgHistCustody.SelectedIndex = -1;
                            }
                            else if (IsCurrentTabPanelType("ISSUES"))
                            {
                                hdStatusKey.Value = Convert.ToString(PLCSessionVars1.GetChemInvProperty<int>("STATUS_KEY", null).ToString());
                                PLCSessionVars1.SetChemInvProperty<int>("STATUS_KEY", null);

                                PLCDBGrid dbgDispense = FindDispenseDBGrid(wrapPanel);
                                dbgDispense.Enabled = false;
                                dbgDispense.SelectedIndex = -1;
                            }
                            else if (IsCurrentTabPanelType("RECEIPTS"))
                            {
                                hdStatusKey.Value = Convert.ToString(PLCSessionVars1.GetChemInvProperty<int>("STATUS_KEY", null).ToString());
                                PLCSessionVars1.SetChemInvProperty<int>("STATUS_KEY", null);

                                PLCDBGrid dbgReceipt = (PLCDBGrid)PLCGlobals.PLCCommon.FindControlWithin("dbgReceipts", cp);
                                dbgReceipt.Enabled = false;
                                dbgReceipt.SelectedIndex = -1;
                            }
                            else
                            {
                                ChemControlNumber =
                                    (PLCSessionVars1.GetChemInvProperty<int>(ChemInvConstants.CHEM_CONTROL_NUM,
                                                                                0) == 0)
                                        ? PLCSessionVars1.GetNextSequence("CHEMINV_SEQ")
                                        : PLCSessionVars1.GetChemInvProperty<int>(
                                                ChemInvConstants.CHEM_CONTROL_NUM, 0);
                                //
                                if (!menu_main.Items[0].Enabled)
                                    menu_main.Items[0].Enabled = true;
                                menu_main.Items[0].Selected = true;
                            }

                            // for TV_CHEMINV.VERIFICATION_REFERENCE
                            string assetClass = PLCSessionVars1.GetChemInvProperty<string>(ChemInvConstants.ASSET_CLASS, "");
                            if (assetClass == ChemInvConstants.ASSET_DRUG)
                            {
                                var selectedTab = menu_main.SelectedItem;
                                if (selectedTab != null)
                                    if (menu_main.Items.IndexOf(selectedTab) == 0)
                                        DBPanel.setpanelfield("VERIFICATION_REFERENCE", GetDefaultVerifRef(chDeptCode.GetValue()));
                            }

                            if (gvMainAsset.Rows.Count > 0)
                                PLCSessionVars1.SetChemInvProperty<int>(ChemInvConstants.CHEM_CONTROL_NUM, ChemControlNumber);
                            else
                                Session["AddAndListEmpty"] = "T";

                            DBPanel.setpanelfield("BARCODE", CreateBarcode("J", GetBarcode.IntToBase64(ChemControlNumber), 5));
                            hdCCN.Value = ChemControlNumber.ToString();

                            string assetType = DBPanel.getpanelfield("ASSET_TYPE");
                            if (!string.IsNullOrEmpty(assetType) && string.IsNullOrEmpty(DBPanel.getpanelfield("LOT_NUMBER")))
                            {
                                string lotNumberFormat = GetDefaultLotNumber(assetType, false);
                                DBPanel.setpanelfield("LOT_NUMBER", lotNumberFormat);
                            }
                        }
                        else
                        {
                            DBPanel.PLCWhereClause = " WHERE CHEM_CONTROL_NUMBER = " + hdCCN.Value;
                            DBPanel.DoLoadRecord();
                            DBPanel.SetEditMode();
                            DBPanel.SetPanelDefaultValuesOnBlankFields();
                        }
                        DBPanel.Visible = true;
                        hdMode.Value = modeADD;
                        break;

                    case "EDIT":
                        hdFields.Value = string.Empty;
                        this.hdDupOrigChemControlNo.Value = string.Empty;

                        DBPanel.SetEditMode();

                        if (menu_main.SelectedItem.Text.Trim().ToUpper() == "HISTORY OF CUSTODY" && hdSaveSuccess.Value != "false")
                        {
                            //ChemControlNumber = Convert.ToInt32(lbxAssetName.SelectedValue);
                            PLCDBGrid dbgHistCustody = FindHistCustodyDBGrid();
                            hdStatusKey.Value = (dbgHistCustody.SelectedDataKey != null) ? dbgHistCustody.SelectedDataKey.Value.ToString() : "0";
                            dbgHistCustody.Enabled = false;
                        }
                        else if (IsCurrentTabPanelType("COMPONENT") && hdSaveSuccess.Value != "false")
                        {
                            HiddenField hdnControlNo = (HiddenField)cp.FindControl("hdnControlNo");
                            if (!DBPanel.IsNewRecord) PLCSessionVars1.SetChemInvProperty<string>("SUB_CHEM_CTRL_NUM", hdnControlNo.Value);

                            ((ImageButton)cp.FindControl("imgCompLookup")).Enabled = true;
                            TextBox tbBarcode = (TextBox)cp.FindControl("tbBarcode");
                            tbBarcode.Style["background-color"] = "white";

                            PLCDBGrid dbgComponents = (PLCDBGrid)PLCGlobals.PLCCommon.FindControlWithin("dbgComponents", cp);
                            dbgComponents.Enabled = false;

                            PLCDBPanel dbpCOMPONENTS = (PLCDBPanel)cp.FindControl("dbpCOMPONENT");
                            dbpCOMPONENTS.SetMyFieldModeEnableDisable("AMOUNT", PLCSession.CheckUserOption("COMPEDQTY") || dbpCOMPONENTS.IsNewRecord);
                        }
                        else if (menu_main.SelectedItem.Text.Trim().ToUpper() == "SERVICE HISTORY" && hdSaveSuccess.Value != "false")
                        {
                            //ChemControlNumber = Convert.ToInt32(lbxAssetName.SelectedValue);
                            PLCDBGrid dbgServHistory = (PLCDBGrid)PLCGlobals.PLCCommon.FindControlWithin("dbgServHistory", cp);
                            hdStatusKey.Value = (dbgServHistory.SelectedDataKey != null) ? dbgServHistory.SelectedDataKey.Value.ToString() : "0";
                            dbgServHistory.Enabled = false;
                        }
                        else if (IsCurrentTabPanelType("ISSUES"))
                        {
                            PLCDBGrid dbgDispense = FindDispenseDBGrid();
                            hdStatusKey.Value = (dbgDispense.SelectedDataKey != null) ? dbgDispense.SelectedDataKey.Values["CHEM_USE_KEY"].ToString() : "0";
                            dbgDispense.Enabled = false;
                        }
                        else if (IsCurrentTabPanelType("RECEIPTS"))
                        {
                            PLCDBGrid dbgReceipt = (PLCDBGrid)PLCGlobals.PLCCommon.FindControlWithin("dbgReceipts", cp);
                            hdStatusKey.Value = (dbgReceipt.SelectedDataKey != null) ? dbgReceipt.SelectedDataKey.Values["RECEIPT_KEY"].ToString() : "0";
                            dbgReceipt.Enabled = false;
                        }

                        hdMode.Value = modeEDIT;
                        break;

                    case "SAVE":
                        if (hdSaveSuccess.Value == "false")
                        {
                            DBPanel.CanSave();
                            break;
                        }

                        if (DBPanel.CanSave())
                        {



                            string whereClause = string.Empty;
                            string chemcontrolnumber = (hdCCN.Value == string.Empty) ?
                                gvMainAsset.SelectedDataKey.Value.ToString() : hdCCN.Value.ToString();
                            whereClause = "WHERE CHEM_CONTROL_NUMBER = " + chemcontrolnumber;

                            PLCDBGrid dbgrid = null;
                            string dataKeyValue = hdStatusKey.Value;
                            if (menu_main.SelectedItem.Text.Trim().ToUpper() == "HISTORY OF CUSTODY")
                            {
                                string custody = DBPanel.getpanelfield("STATUS_CODE");
                                if (hdMode.Value == modeADD)
                                {
                                    PLCQuery qryChecking = new PLCQuery("SELECT * FROM TV_CHEMINV WHERE CHEM_CONTROL_NUMBER = " + chemcontrolnumber + " AND CURRENT_STATUS = '" + custody + "' AND CURRENT_LOCATION = '" + DBPanel.getpanelfield("LOCKER") + "'");
                                    qryChecking.OpenReadOnly();
                                    if (qryChecking.HasData())
                                    {
                                        hdSaveSuccess.Value = "false";
                                        messageBox.ShowMsg("Chemical Inventory", "Item already in custody.", 0);
                                        return;
                                    }
                                }

                                if (PLCDBGlobal.instance.GetCustodyDisposedCodes().Split(',').Contains(custody))
                                {
                                    hdSaveSuccess.Value = "false";
                                    messageBox.ShowMsg("Chemical Inventory", "Transfer to a disposed location is not allowed on this tab.", 0);
                                    return;
                                }

                                whereClause += " AND STATUS_KEY = " + hdStatusKey.Value;

                                PLCDBGrid dbgHistCustody = FindHistCustodyDBGrid();
                                dbgHistCustody.Enabled = true;
                            }
                            else if (IsCurrentTabPanelType("COMPONENT"))
                            {
                                // Allow edit of preparation notes in components tab
                                if (DBPanel.ID == "dbpPREPARATIONNOTES")
                                {
                                    DBPanel.PLCWhereClause = whereClause;
                                    DBPanel.DoSave();
                                    DBPanel.SetBrowseMode();
                                    continue; // Early exit on save to prevent execution of below codes
                                }

                                //validate component
                                HiddenField hdnControlNo = (HiddenField)cp.FindControl("hdnControlNo");
                                if (hdnControlNo.Value == string.Empty || hdnControlNo.Value == "0")
                                {
                                    hdSaveSuccess.Value = "false";
                                    messageBox.ShowMsg("Chemical Inventory", "Please Enter a valid Chemical.", 0);
                                    return;
                                }

                                PLCQuery qryChecking = new PLCQuery();
                                qryChecking.SQL = "SELECT CHEM_CONTROL_NUMBER, EXPIRATION_DATE, CURRENT_STATUS FROM TV_CHEMINV WHERE CHEM_CONTROL_NUMBER = " + hdnControlNo.Value;
                                qryChecking.Open();
                                if (qryChecking.IsEmpty())
                                {
                                    hdSaveSuccess.Value = "false";
                                    messageBox.ShowMsg("Chemical Inventory", "Component not found in Master Database.", 0);
                                    return;
                                }
                                else
                                {
                                    qryChecking.First();
                                    string custodyDisposedCodes = PLCDBGlobal.instance.GetCustodyDisposedCodes();
                                    if (custodyDisposedCodes == string.Empty)
                                    {
                                        //messageBox.ShowMsg("Chemical Inventory", "Please setup the Dispose Code.", 0);
                                    }
                                    else
                                    {
                                        string[] disposedCodes = custodyDisposedCodes.ToString().Split(',');
                                        if (disposedCodes.Contains(qryChecking.FieldByName("CURRENT_STATUS").Trim()))
                                        {
                                            hdSaveSuccess.Value = "false";
                                            messageBox.ShowMsg("Chemical Inventory", "The item you scanned is disposed. Check the Chain of Custody.", 0);
                                            return;
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(qryChecking.FieldByName("EXPIRATION_DATE")) && Convert.ToDateTime(qryChecking.FieldByName("EXPIRATION_DATE")) <= DateTime.Now)
                                    {
                                        hdSaveSuccess.Value = "false";
                                        messageBox.ShowMsg("Chemical Inventory", "The item you scanned is expired.", 0);
                                        return;
                                    }
                                }

                                PLCDBGrid dbgComponents = (PLCDBGrid)PLCGlobals.PLCCommon.FindControlWithin("dbgComponents", cp);
                                foreach (GridViewRow row in dbgComponents.Rows)
                                {
                                    if (row != dbgComponents.SelectedRow)
                                    {
                                        if (dbgComponents.DataKeys[row.RowIndex].Values["SUB_CHEM_CONTROL_NUMBER"].ToString() == hdnControlNo.Value)
                                        {
                                            hdSaveSuccess.Value = "false";
                                            messageBox.ShowMsg("Chemical Inventory", "Component exists for this reagent.", 0);
                                            return;
                                        }
                                    }
                                }

                                if (hdnControlNo.Value == chemcontrolnumber)
                                {
                                    hdSaveSuccess.Value = "false";
                                    messageBox.ShowMsg("Chemical Inventory", "The currently selected record cannot be added as a component.", 0);
                                    return;
                                }
                                dbgComponents.Enabled = true;

                                //dbgrid = dbgComponents;
                                //dataKeyValue = hdnControlNo.Value;

                                // call the save hook if present
                                if (this.PLCButtonPanelSaveHookHandler != null)
                                    this.PLCButtonPanelSaveHookHandler(DBPanel, new EventArgs());

                                //save
                                whereClause += " AND SUB_CHEM_CONTROL_NUMBER = " + PLCSessionVars1.GetChemInvProperty<string>("SUB_CHEM_CTRL_NUM", "0");
                                ((ImageButton)cp.FindControl("imgCompLookup")).Enabled = false;
                                ((TextBox)cp.FindControl("tbBarcode")).Style["background-color"] = "lightgrey";
                            }
                            else if (menu_main.SelectedItem.Text.Trim().ToUpper() == "SERVICE HISTORY")
                            {
                                whereClause += " AND STATUS_KEY = " + hdStatusKey.Value;
                            }
                            else if (IsCurrentTabPanelType("ISSUES"))
                            {
                                if (!_skipIssuesValidation)
                                {
                                    string receivingAnalyst = DBPanel.getpanelfield("RECEIVING_ANALYST");
                                    string custodyCode = DBPanel.getpanelfield("ISSUED_TO_CUSTODY_CODE");
                                    string locationCode = DBPanel.getpanelfield("ISSUED_TO_LOCATION");

                                    // Ticket #68566	TG647	LAM Surplus Supply
                                    // If CUSTODY_CODE flag USES_SUPPLY_SURPLUS_LOCATIONS = 'T' then skip default behavior

                                    if (string.IsNullOrEmpty(custodyCode) || string.IsNullOrEmpty(locationCode))
                                    {
                                        hdSaveSuccess.Value = "false";
                                        messageBox.ShowMsg("Chemical Inventory", "Custody and Location is required.", 0);
                                        return;
                                    }

                                    PLCQuery qryUsesSupplySurplusLocs = new PLCQuery();
                                    qryUsesSupplySurplusLocs.SQL = String.Format(
                                        @"select USES_SUPPLY_SURPLUS_LOCATIONS from TV_CUSTCODE where CUSTODY_TYPE = '{0}'",
                                        custodyCode
                                    );

                                    qryUsesSupplySurplusLocs.Open();
                                    if (qryUsesSupplySurplusLocs.FieldByName("USES_SUPPLY_SURPLUS_LOCATIONS") == "T")
                                    {
                                        // do nothing (and not default behavior)
                                    }
                                    else
                                    if (!string.IsNullOrEmpty(custodyCode) && !string.IsNullOrEmpty(receivingAnalyst))
                                    {
                                        string analystCustodies = PLCSession.GetLabCtrl("ANALYST_CUSTODY_CODES");
                                        if (!string.IsNullOrEmpty(analystCustodies))
                                        {
                                            string[] custodies = analystCustodies.ToUpper().Replace(" ", "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                            string currentAssetClass = PLCSessionVars1.GetChemInvProperty<string>(ChemInvConstants.ASSET_CLASS, "");
                                            if (custodies.Contains(custodyCode) && !HasChemInvAccess(receivingAnalyst, currentAssetClass))
                                            {
                                                //$$ Disable analyst access transfer check in NYPD for now.
                                                //                                                        hdSaveSuccess.Value = "false";
                                                //                                                        messageBox.ShowMsg("Chemical Inventory", "The receiving analyst does not have authority to access this item type.", 0);
                                                //                                                        return;
                                            }
                                        }
                                    }

                                    if (PLCSession.GetChemInvCtrl("HIDE_ISSUE_RECEIVING_ANALYST") != "T")
                                    {
                                        // Added SUPPLY_LOCATION check for OSCO 
                                        // for when consumables are issued to supply locations from surplus locations
                                        // 2017.10.17 LME

                                        PLCQuery qryLocation = new PLCQuery(string.Format(@"
                                                    SELECT * FROM TV_CUSTLOC WHERE CUSTODY_CODE = '{0}' AND LOCATION = '{1}' 
                                                     AND (ISSUED_LOCATION = 'T' OR SUPPLY_LOCATION = 'T')
                                                    ", custodyCode, locationCode));


                                        qryLocation.Open();
                                        if (qryLocation.IsEmpty())
                                        {
                                            hdSaveSuccess.Value = "false";
                                            messageBox.ShowMsg("Chemical Inventory", "Issued to location must be a valid location that is set to make issues to.", 0);
                                            return;
                                        }
                                    }

                                    if (PLCSession.GetChemInvCtrl("USES_BEGIN_AND_END_WEIGHT") != "T")
                                    {
                                        string quantity = DBPanel.getpanelfield("AMOUNT_USED");
                                        double dblQuantity = 0;
                                        if (!Double.TryParse(quantity, out dblQuantity) || dblQuantity <= 0)
                                        {
                                            hdSaveSuccess.Value = "false";
                                            messageBox.ShowMsg("Chemical Inventory", "Quantity must be a number greater than zero.", 0);
                                            return;
                                        }
                                        else
                                        {
                                            PLCQuery qryChemInv = new PLCQuery(string.Format(@"SELECT QUANTITY, U.DESCRIPTION AS UNIT FROM TV_CHEMINV I
LEFT OUTER JOIN TV_CHEMUNIT U ON I.QUANTITY_UNITS = U.CODE
WHERE CHEM_CONTROL_NUMBER = {0}", chemcontrolnumber));
                                            qryChemInv.Open();
                                            PLCDBPanel dbpIssues = (PLCDBPanel)cp.FindControl("dbpISSUES");
                                            double dblAvailable = string.IsNullOrEmpty(qryChemInv.FieldByName("QUANTITY")) ? 0 : Convert.ToDouble(qryChemInv.FieldByName("QUANTITY"));
                                            if (dblQuantity > (dblAvailable + (string.IsNullOrEmpty(dbpIssues.GetOriginalValue("AMOUNT_USED")) ? 0 : Convert.ToDouble(dbpIssues.GetOriginalValue("AMOUNT_USED")))))
                                            {
                                                string unit = qryChemInv.FieldByName("UNIT");
                                                unit = string.IsNullOrEmpty(unit) ? "" : " (" + unit + ")";
                                                string message = string.Format("You are attempting to dispense {0} {1}, and there is/are {2} available.<br /><br />Are you sure you want to proceed?",
                                                    dblQuantity, unit, dblAvailable);
                                                mbConfirmation.Message = message;
                                                mbConfirmation.Show();
                                                hdSaveSuccess.Value = "false";
                                                return;
                                            }
                                        }
                                    }
                                }

                                whereClause += " AND CHEM_USE_KEY = " + hdStatusKey.Value;

                                PLCDBGrid dbgDispense = FindDispenseDBGrid();
                                dbgDispense.Enabled = true;
                                dbgrid = dbgDispense;
                            }
                            else if (IsCurrentTabPanelType("RECEIPTS"))
                            {
                                string quantity = DBPanel.getpanelfield("AMOUNT_RECEIVED");
                                double dblQuantity = 0;

                                // Ticket #68566	TG647	LAM Surplus Supply 
                                // OCSO: if panel contains both SUPPLY_QUANTITY and SURPLUS_QUANTITY
                                // don't enforce AMOUNT_RECEIVED
                                // Check downstream for the QUANTITY updating and override if necessary



                                // check if Supply/Surplus qty is used
                                Boolean usesSupplySurplusQty = false;
                                List<string> panelFieldNames = DBPanel.GetFieldNames();
                                if (panelFieldNames.Contains("SUPPLY_QUANTITY") && panelFieldNames.Contains("SURPLUS_QUANTITY"))
                                {
                                    usesSupplySurplusQty = true;
                                }

                                if (usesSupplySurplusQty)
                                {


                                    string strSupplyQty = DBPanel.getpanelfield("SUPPLY_QUANTITY");
                                    double dblSupplyQty;
                                    string strSurplusQty = DBPanel.getpanelfield("SURPLUS_QUANTITY");
                                    double dblSurplusQty;

                                    if (
                                        (!Double.TryParse(strSupplyQty, out dblSupplyQty) || dblSupplyQty <= 0)
                                     && (!Double.TryParse(strSurplusQty, out dblSurplusQty) || dblSurplusQty <= 0)
                                        )
                                    {
                                        hdSaveSuccess.Value = "false";
                                        messageBox.ShowMsg("Chemical Inventory", "Supply Quantity or Surplus Quantity must be a number greater than zero.", 0);
                                        return;
                                    }
                                }
                                else
                                {
                                    if (!Double.TryParse(quantity, out dblQuantity) || dblQuantity <= 0)
                                    {
                                        hdSaveSuccess.Value = "false";
                                        messageBox.ShowMsg("Chemical Inventory", "Amount received must be a number greater than zero.", 0);
                                        return;
                                    }
                                }


                                string custodyCode = DBPanel.getpanelfield("RECEIVED_FROM_CUSTODY_CODE");
                                string locationCode = DBPanel.getpanelfield("RECEIVED_FROM_LOCATION");
                                PLCQuery qryLocation = new PLCQuery(string.Format("SELECT * FROM TV_CUSTLOC WHERE CUSTODY_CODE = '{0}' AND LOCATION = '{1}' AND RECEIVED_LOCATION = 'T'", custodyCode, locationCode));
                                qryLocation.Open();
                                if (qryLocation.IsEmpty())
                                {
                                    hdSaveSuccess.Value = "false";
                                    messageBox.ShowMsg("Chemical Inventory", "Received from location must be a valid location that is set to receive from.", 0);
                                    return;
                                }

                                whereClause += " AND RECEIPT_KEY = " + hdStatusKey.Value;

                                PLCDBGrid dbgReceipt = (PLCDBGrid)PLCGlobals.PLCCommon.FindControlWithin("dbgReceipts", cp);
                                dbgReceipt.Enabled = true;
                                dbgrid = dbgReceipt;
                            }

                            List<string> dbpanelOrigFieldNames = DBPanel.GetFieldNames();


                            // $$ If some values were changed in the chFXCode_ValueChanged() right before saving, 
                            // we need to restore to the original values to prevent data loss.
                            // chFXCode_ValueChanged() shouldn't be called at all when SAVE button is clicked, 
                            // so this is a hack to work around the issue.
                            if (this.dictFXCODEOrig.Count > 0)
                                RestoreFXCodeOrigValues(DBPanel);

                            DBPanel.PLCWhereClause = whereClause;
                            DBPanel.DoSave();

                            // do not reset original values if the page is in these tabs since they use original values
                            if (!IsCurrentTabPanelType("ISSUES") && !IsCurrentTabPanelType("RECEIPTS"))
                                DBPanel.ResetOriginalValues();

                            // Ticket #68566	TG647	LAM Surplus Supply
                            // Right after DBPanel.DoSave(), the Quantity, Supply and Surplus values are correct
                            // TV_CHEMINV.QUANTITY is being updated later
                            // override this behavior if TV_CUSTCODE.USES_SUPPLY_SURPLUS_LOCATIONS = 'T'

                            // Ticket#39028 - If this is a Dup record, save all fields in the dup field list.
                            // This is to make sure that all fields are duped even though they aren't part of the dbpanel. 
                            string dupFields = this.hdFields.Value.Trim();
                            string origChemControlNo = this.hdDupOrigChemControlNo.Value.Trim();
                            if (dupFields != "" && origChemControlNo != "" && origChemControlNo != "0")
                            {
                                // Ticket#105575 - LAM- duping Kit issue
                                // remove the BOX_BOTTLE_X_OF_Y field from being duped if this function is being
                                // called from the Save button (SaveKit_Click) of Kit Components popup Inventory
                                if (GetDuplicateTimes() <= 0)
                                {
                                    dupFields = dupFields.Replace("BOX_BOTTLE_X_OF_Y", "");
                                }

                                CopyDupFields(origChemControlNo, chemcontrolnumber, dupFields, dbpanelOrigFieldNames);
                            }

                            if (IsCurrentTabPanelType("SERVICEHISTORY"))
                            {
                                UpdateInststatNextServiceDate(DBPanel, whereClause);
                                dbgrid = (PLCDBGrid)PLCGlobals.PLCCommon.FindControlWithin("dbgServHistory", cp);
                            }
                            else if (IsCurrentTabPanelType("COMPONENT") && cp.FindControl("dbpCOMPONENT") != null && hdMode.Value == modeADD)
                            {
                                CreateComponentDispenseRecord();
                            }

                            // Do not update assets grid until all dbpanels are saved
                            //LoadAssetName(SearchQuery.Trim());

                            if (RefreshGrid != null)
                            {
                                RefreshGrid(this, EventArgs.Empty);
                                if (dbgrid != null && !string.IsNullOrEmpty(dataKeyValue))
                                {
                                    dbgrid.SelectRowByDataKey(dataKeyValue);
                                }
                            }

                            DBPanel.SetBrowseMode();

                            //CopyAssetTypeInfo(PLCSessionVars1.GetChemInvProperty<int>("CHEM_CONTROL_NUMBER_DUPLICATE_TIMES", 0));

                            // Dup needs to be done after the last dbpanel because each dbpanel has a different view
                            // on the table. So multiple dbpanels save different columns on the same record.
                            if (i == dbpanels.Count - 1)
                                this.dupChemControlNos = ProcessDuplicateTimes(DBPanel, origChemControlNo);

                            HideDupeTimesCaption();
                        }
                        else
                        {
                            //Problem in saving. Set flag to false.
                            hdSaveSuccess.Value = "false";

                            // Remember previous state to be restored after edit click.
                            // This is to avoid state errors when required fields is prompted.
                            string currentMode = this.hdMode.Value;
                            string currentHDFields = this.hdFields.Value;
                            string backupDupOrigChemControlNo = this.hdDupOrigChemControlNo.Value;
                            string backupDupMsgRowStyle = this.dupmsgRow.Attributes["style"];
                            string backupDupTimes = this.spanDupTimes.InnerText;
                            string backupDupTextNum = this.txtPopupDuplicateTimes.Text;

                            if ((IsCurrentTabPanelType("ISSUES") || IsCurrentTabPanelType("RECEIPTS")) && RefreshGrid != null)
                            {
                                RefreshGrid(this, EventArgs.Empty);
                            }

                            PLCButtonPanel1.ClickEditButton();

                            // Restore the previous state.
                            this.hdMode.Value = currentMode;
                            this.hdFields.Value = currentHDFields;
                            this.hdDupOrigChemControlNo.Value = backupDupOrigChemControlNo;
                            this.dupmsgRow.Attributes["style"] = backupDupMsgRowStyle;
                            this.spanDupTimes.InnerText = backupDupTimes;
                            this.txtPopupDuplicateTimes.Text = backupDupTextNum;
                        }

                        break;

                    case "CANCEL":
                        hdFields.Value = string.Empty;
                        this.hdDupOrigChemControlNo.Value = string.Empty;

                        DBPanel.SetBrowseMode();
                        if (hdCCN.Value != string.Empty)
                        {
                            PLCQuery qryDelete = new PLCQuery();
                            if (menu_main.SelectedItem.Text.Trim().ToUpper() == "HISTORY OF CUSTODY")
                            {
                                qryDelete.SQL = "DELETE FROM TV_CHEMSTAT WHERE STATUS_KEY = " + hdStatusKey.Value.ToString();
                            }
                            else if (IsCurrentTabPanelType("COMPONENT"))
                            {
                                qryDelete.SQL = "DELETE FROM TV_CHEMREAGENT WHERE CHEM_CONTROL_NUMBER = " + gvMainAsset.SelectedDataKey.Value.ToString() +
                                                " AND SUB_CHEM_CONTROL_NUMBER = " + PLCSessionVars1.GetChemInvProperty<string>("SUB_CHEM_CTRL_NUM", "0");
                            }
                            else if (menu_main.SelectedItem.Text.Trim().ToUpper() == "SERVICE HISTORY")
                            {
                                qryDelete.SQL = "DELETE FROM TV_INSTSTAT WHERE STATUS_KEY = " + hdStatusKey.Value.ToString();
                            }
                            else if (IsCurrentTabPanelType("ISSUES"))
                            {
                                qryDelete.SQL = "DELETE FROM TV_CHEMUSE WHERE CHEM_USE_KEY = " + hdStatusKey.Value.ToString();
                            }
                            else if (IsCurrentTabPanelType("RECEIPTS"))
                            {
                                qryDelete.SQL = "DELETE FROM TV_CHEMRCPT WHERE RECEIPT_KEY = " + hdStatusKey.Value.ToString();
                            }

                            else
                            {
                                qryDelete.SQL = "DELETE FROM TV_CHEMINV WHERE CHEM_CONTROL_NUMBER = " + hdCCN.Value.ToString();
                            }
                            qryDelete.ExecSQL(PLCSessionVars1.GetConnectionString());
                        }

                        if (IsCurrentTabPanelType("COMPONENT"))
                        {
                            ((ImageButton)cp.FindControl("imgCompLookup")).Enabled = false;
                            PLCDBPanel dbpCOMPINFO = (PLCDBPanel)cp.FindControl("dbpCOMPINFO");
                            dbpCOMPINFO.PLCWhereClause = "WHERE 0 = 1";
                            dbpCOMPINFO.EmptyMode();
                            TextBox tbBarcode = (TextBox)cp.FindControl("tbBarcode");
                            tbBarcode.Style["background-color"] = "lightgrey";
                            tbBarcode.Text = "";
                        }

                        hdCCN.Value = string.Empty;
                        hdStatusKey.Value = string.Empty;
                        hdMode.Value = string.Empty;

                        if (RefreshGrid != null)
                        {
                            RefreshGrid(this, EventArgs.Empty);
                        }

                        break;

                    case "DELETE":
                        hdFields.Value = string.Empty;
                        this.hdDupOrigChemControlNo.Value = string.Empty;

                        // Do not do anything for dbpPREPARATIONNOTES in Components Tab
                        if (IsCurrentTabPanelType("COMPONENT") && DBPanel.ID == "dbpPREPARATIONNOTES")
                            continue;

                        DBPanel.DoDelete();
                        DBPanel.SetBrowseMode();

                        LoadMenuItems(rbtnAssetClass.SelectedValue);

                        if (Session["CHEM_CONTROL_NUM_RECALC"] != null)
                        {
                            RecalculateStock(Session["CHEM_CONTROL_NUM_RECALC"].ToString());
                            Session.Remove("CHEM_CONTROL_NUM_RECALC");
                        }

                        if (menu_main.SelectedItem.Text.Trim().ToUpper() == "HISTORY OF CUSTODY")
                        {
                            UpdateCustodyInfo();
                        }
                        else if (IsCurrentTabPanelType("ISSUES") && cp.FindControl("dbpISSUES") != null)
                        {
                            PLCDBPanel dbpIssues = (PLCDBPanel)cp.FindControl("dbpISSUES");
                            UpdateChemInvCurrentWeightAndExhaustDate(false, dbpIssues, PLCSessionVars1.GetChemInvProperty<int>(ChemInvConstants.CHEM_CONTROL_NUM, 0));
                        }

                        if (RefreshGrid != null)
                        {
                            RefreshGrid(this, EventArgs.Empty);
                        }

                        // Need to re-set the button state after Delete.
                        SetButtonPanel();

                        break;

                    case "Dupe":
                        if (dupOrigChemControlNo == "")
                        {
                            this.hdDupOrigChemControlNo.Value = PLCSessionVars1.GetChemInvProperty<int>(ChemInvConstants.CHEM_CONTROL_NUM, 0).ToString();
                            dupOrigChemControlNo = this.hdDupOrigChemControlNo.Value;
                        }

                        //Dupe works like Add and Edit at the same time. Add a new item copying values for the specified fields.
                        if (hdCCN.Value == string.Empty)
                        {

                            DBPanel.DoAdd();

                            ChemControlNumber =
                                (PLCSessionVars1.GetChemInvProperty<int>(ChemInvConstants.CHEM_CONTROL_NUM,
                                                                            0) == 0)
                                    ? PLCSessionVars1.GetNextSequence("CHEMINV_SEQ")
                                    : PLCSessionVars1.GetChemInvProperty<int>(
                                            ChemInvConstants.CHEM_CONTROL_NUM, 0);

                            PLCSessionVars1.SetChemInvProperty<int>(ChemInvConstants.CHEM_CONTROL_NUM, Convert.ToInt32(ChemControlNumber));

                            DBPanel.setpanelfield("BARCODE", CreateBarcode("J", GetBarcode.IntToBase64(ChemControlNumber), 5));
                            hdCCN.Value = ChemControlNumber.ToString();

                            PLCButtonPanel1.SetEditMode();
                        }
                        else
                        {
                            DBPanel.PLCWhereClause = " WHERE CHEM_CONTROL_NUMBER = " + hdCCN.Value;
                            DBPanel.DoLoadRecord();
                            DBPanel.SetEditMode();
                        }

                        PLCQuery qryDupe = new PLCQuery();
                        if (hdFields.Value.Trim() != string.Empty)
                        {
                            qryDupe.SQL = "SELECT " + hdFields.Value.ToUpper() +
                                            " FROM TV_CHEMINV WHERE CHEM_CONTROL_NUMBER = " + gvMainAsset.SelectedDataKey.Value.ToString();
                            qryDupe.Open();
                            if (!qryDupe.IsEmpty())
                            {
                                DataRow duperow = qryDupe.PLCDataTable.Rows[0];
                                foreach (string fieldname in hdFields.Value.Replace(" ", String.Empty).Split(','))
                                {
                                    if (duperow[fieldname] != null && duperow[fieldname] != DBNull.Value)
                                    {
                                        // Swallow the exception so that other dbpanel fields can proceed to be set.
                                        try
                                        {
                                            if (duperow[fieldname] is double && DBPanel.GetFieldMask(fieldname).Contains("."))
                                            {
                                                // Need to type cast as double so that 1.0 gets represented as "1.0"
                                                double n = (double)duperow[fieldname];
                                                DBPanel.setpanelfield(fieldname, n.ToString("F"));
                                            }
                                            else
                                            {
                                                DBPanel.setpanelfield(fieldname, duperow[fieldname].ToString());
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            e.ToString();
                                        }
                                    }
                                }
                            }
                        }

                        DBPanel.Visible = true;
                        hdMode.Value = modeADD;
                        break;

                    default:
                        break;
                }
            }
        }
    }
    if (buttonName == "DELETE") Session.Remove("CHEM_CONTROL_NUM_RECALC");
    else if (buttonName == "ADD" || buttonName == "Dupe" || (hdSaveSuccess.Value == "false" && buttonName == "SAVE"))
    {
        ScriptManager.RegisterStartupScript(Page, Page.GetType(), "_beforeunload", "bindBeforeUnload();", true);
    }
}