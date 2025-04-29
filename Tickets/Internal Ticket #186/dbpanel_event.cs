protected void PLCButtonPanel1_PLCButtonClick(object sender, PLCCONTROLS.PLCButtonClickEventArgs e)
{
    #region ValidateItems

    if ((e.button_name == "SAVE") && (e.button_action == "BEFORE"))
    {
        string CurrentItemNumber = PLCDBPanel1.getpanelfield("LAB_ITEM_NUMBER");

        // validate if item still exists if record unlock user option is on. other issues regarding editing deleted items are not yet handled.
        if (PLCSession.CheckUserOption("DELLOCKS") && !e.row_added)
        {
            PLCQuery qryItemExists = new PLCQuery();
            qryItemExists.SQL = string.Format("SELECT EVIDENCE_CONTROL_NUMBER FROM TV_LABITEM WHERE CASE_KEY = {0} AND EVIDENCE_CONTROL_NUMBER = {1}", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN);
            if (qryItemExists.Open() && qryItemExists.IsEmpty())
            {
                e.button_canceled = true;
                PLCDBGlobal.instance.RemoveRecordLocks("TV_LABITEM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, "-1", PLCSession.PLCGlobalAnalyst);
                dlgMessage.ShowAlert("Item Record", "Item #" + CurrentItemNumber + " does not exists.");
                GridView1.InitializePLCDBGrid();

                if (GridView1.Rows.Count > 0)
                {
                    GridView1.SelectedIndex = 0;
                    PLCSession.PLCGlobalECN = GridView1.SelectedDataKey.Value.ToString();
                    UpdateCustodyStatusDisplay(PLCSession.PLCGlobalECN);
                    GrabGridRecord();
                }
                else
                {
                    PLCSession.PLCGlobalECN = "";
                    PLCDBPanel1.EmptyMode();
                    PLCButtonPanel1.SetEmptyMode();
                    SetAttachmentsButton("CASE");
                    EnableButtonControls(false, false);
                    ClearCustodyStatusDisplay();
                }

                return;
            }
        }

        //validate labitemnumber                
        if (!ValidLabItemNumber(CurrentItemNumber))
        {
            e.button_canceled = true;
            return;
        }

        if (LabItemNumberExists(CurrentItemNumber, e.row_added))
        {
            bool blnItemNumberSet = false;

            if (e.row_added)
            {
                blnItemNumberSet = GetItemNumber();
                if (blnItemNumberSet)
                    mbox.ShowMsg("Item Number", "The Item #" + CurrentItemNumber + " already exists. The next item number will be assigned.", 1);
                else
                {
                    mbox.ShowMsg("Item Number", "The Item #" + CurrentItemNumber + " already exists.", 1);
                    e.button_canceled = true;
                    return;
                }

            }
            else
            {
                mbox.ShowMsg("Item Number", "The Item #" + CurrentItemNumber + " already exists.", 1);
                e.button_canceled = true;
                return;
            }
        }

        if (PLCSession.GetLabCtrl("DUPE_DEPT_ITEM_NUM_ON_SAMPLE") == "T" && !String.IsNullOrEmpty(PLCDBPanel1.getpanelfield("DEPARTMENT_ITEM_NUMBER")) && DeptItemNumberExists(PLCDBPanel1.getpanelfield("DEPARTMENT_ITEM_NUMBER"), e.row_added))
        {
            mbox.ShowMsg("Department Item Number", "The Department Item #" + PLCDBPanel1.getpanelfield("DEPARTMENT_ITEM_NUMBER") + " already exists.", 1);
            e.button_canceled = true;
            return;
        }

        int caseKey = PLCSession.SafeInt(PLCSession.PLCGlobalCaseKey);
        var isCaseFromCPD = PLCDBGlobal.instance.GetTableDetailsByCustomKey("TV_LABCASE", "CASE_FROM_CPD", "CASE_KEY", PLCSession.PLCGlobalCaseKey) == "T";
        if (isCaseFromCPD)
        {
            string origItemNum = PLCDBPanel1.GetOriginalValue("DEPARTMENT_ITEM_NUMBER");
            string newItemNum = PLCDBPanel1.getpanelfield("DEPARTMENT_ITEM_NUMBER");
            if ((e.row_added || origItemNum != newItemNum)
                && PLCDBGlobal.instance.IsPrelogDeptItemNumberExists(caseKey, newItemNum))
            {
                string message = "The Item #" + newItemNum + " already exists in Prelog.";
                dlgMessage.ShowAlert("Department Item Number", message);
                e.button_canceled = true;
                return;
            }
        }

        //validate thirdpartybarcode
        if (!IsThirdPartyBarcodeValid())
        {
            e.button_canceled = true;
            return;
        }

        // see if names with selected relation are checked
        string nameList = NameRelationCheck();
        if (!string.IsNullOrEmpty(nameList))
        {
            mbox.ShowMsg("Items Names Link", "The following Names need to be checked: <br>" + nameList, 0);
            e.button_canceled = true;
            return;
        }

        nameList = RequireRelationCheck();
        if (!string.IsNullOrEmpty(nameList))
        {
            mbox.ShowMsg("Names Relation", "Please select a relation for all checked names: <br>" + nameList, 0);
            e.button_canceled = true;
            return;
        }

        string itemType = PLCDBPanel1.getpanelfield("ITEM_TYPE");
        var requiredFields = PLCDBGlobal.instance.GetItemTypeRequiredNameFields(itemType);
        if (requiredFields.Count > 0)
        {
            Dictionary<string, string> invalidNames;
            if (!CheckItemNamesRequiredFields(requiredFields.Keys, out invalidNames))
            {
                ShowRequiredNameFieldsForm(requiredFields, invalidNames);
                e.button_canceled = true;
                return;
            }
        }

        bool isDupeOrSample = ((ViewState["DUPE_ITEM"] != null && Convert.ToBoolean(ViewState["DUPE_ITEM"])) ||
                                 (ViewState["SAMPLE_ITEM_NEW_CONTAINER"] != null && Convert.ToBoolean(ViewState["SAMPLE_ITEM_NEW_CONTAINER"])));
        bool excludeRMS = isDupeOrSample && PLCSession.GetLabCtrlFlag("EXCLUDE_RMS_EXTERNAL") == "T";

        // NYPD: copy voucher from submission if blank on new item
        if (PLCSession.GetLabCtrl("RMS_EXTERNAL_IS_VOUCHER") == "T" && !excludeRMS)
        {
            // validate that voucher does not exist yet
            if ((!String.IsNullOrEmpty(PLCDBPanel1.getpanelfield("RMS_EXTERNAL").ToString())) && (dbgbl.VoucherExistsInAnotherCase(PLCDBPanel1.getpanelfield("RMS_EXTERNAL").ToString())))
            {
                mbox.ShowMsg("Voucher Number", "Voucher already exists in a different case.", 1);
                e.button_canceled = true;
                return;
            }

            if ((e.row_added) && (String.IsNullOrEmpty(PLCDBPanel1.getpanelfield("RMS_EXTERNAL").ToString())))
            {
                PLCDBPanel1.setpanelfield("RMS_EXTERNAL", GetSubmissionCommandVoucher(PLCSession.PLCGlobalCaseKey, PLCDBPanel1.getpanelfield("LAB_CASE_SUBMISSION")));
            }
        }

        if (bnSAK.Visible)
        {
            if (!CanSaveSAKPanel())
            {
                bnSAK.Style.Add("color", "red");
                PLCButtonPanel1.SetSaveError("    Unable to save this information. Look for errors in red.");
                e.button_canceled = true;
            }
            else
            {
                bnSAK.Style.Add("color", "");
            }
        }

        if (ViewState["MandatoryFieldMissing"] != null ? Convert.ToBoolean(ViewState["MandatoryFieldMissing"]) : false)
        {
            e.button_canceled = true;
            ViewState["MandatoryFieldMissing"] = false;
            PLCButtonPanel1.SetSaveError("    Unable to save this information. Look for errors in red.");
            return;
        }

        if (ItemAttribute.UsesItemAttributeEntries)
        {
            if (!bnAttribute.Disabled && !ItemAttribute.CanSave())
            {
                e.button_canceled = true;
                PLCButtonPanel1.SetSaveError("    Unable to save this information. Look for errors in red.");
                return;
            }
        }

        if (AllowCurrencyEntry(PLCDBPanel1.getpanelfield("ITEM_TYPE")) && !Currency1.IsVerified())
        {
            e.button_canceled = true;
            return;
        }

        if (ItemTypeChangeConfirmed)
        {
            //delete existing currency records
            Currency1.DeleteData(PLCSession.PLCGlobalECN);

            if (!PLCSession.GetLabCtrl("USES_ITEM_ATTRIBUTE_ENTRIES").Equals("T"))
                ItemAttribute.DeleteCurrentAttributes();

            ItemTypeChangeConfirmed = false;
        }

        // Call RMS Evidence Custody update for items created in a case from RMS.
        bool LIMSInRMSMode = !string.IsNullOrEmpty(PLCSession.GetWebConfiguration("RMS_URL"));
        bool barcodeExists = ItemHasThirdPartyBarcode(PLCSession.PLCGlobalECN);
        bool caseIsFromRMS = (PLCDBGlobal.instance.GetTableDetailsByCustomKey("TV_LABCASE", "CASE_FROM_RMS", "CASE_KEY", PLCSession.PLCGlobalCaseKey) == "T");
        string thirdPartyBarcode = PLCDBPanel1.getpanelfield("THIRD_PARTY_BARCODE");

        if (!barcodeExists && caseIsFromRMS && !string.IsNullOrEmpty(thirdPartyBarcode) && LIMSInRMSMode)
        {
            List<ItemRec> ItemList = new List<ItemRec>();
            ItemList.Add(new ItemRec(0, "", "", "", "", "", null, 0, "", "", Convert.ToInt32(PLCSession.PLCGlobalECN), thirdPartyBarcode, "", "", "", "", "", ""));
            PLCDBGlobal.instance.UpdateRMSEvidenceCustodyOut(PLCSession.PLCGlobalDepartmentCaseNumber, ItemList);
        }
    }

    if (e.button_name == "EDIT" && e.button_action == "BEFORE")
    {
        if (IsSelectedRecordsLocked(null))
        {
            dlgMessage.ShowAlert("Record Lock", "Item locked by another user for editing.<br/>" + RecordsLockedInfo);
            e.button_canceled = true;
            return;
        }
        else
            PLCDBGlobal.instance.LockUnlockRecord("TV_LABITEM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, "-1", true);
    }

    if (e.button_name == "DELETE" && e.button_action == "BEFORE")
    {
        if (IsSelectedRecordsLocked(null))
        {
            dlgMessage.ShowAlert("Record Lock", "Item locked by another user for editing.<br/>" + RecordsLockedInfo);
            e.button_canceled = true;
            return;
        }
        else
            PLCDBGlobal.instance.LockUnlockRecord("TV_LABITEM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, "-1", true);

        if (ItemInCompletedAssignment(PLCSessionVars1.PLCGlobalECN))
        {
            e.button_canceled = true;
            mbox.ShowMsg("Delete Item", "This Item cannot be deleted as it is linked to a completed Assignment.", 0);
            return;
        }

        if (GetAffectedItemRecords(e))
            return;

        DeleteItemNameLink(GridView1.SelectedDataKey.Value.ToString());
    }


    #endregion ValidateItems

    #region After Action

    if ((e.button_name == "ADD") & (e.button_action == "AFTER"))
    {
        // check whether we will be using the dbpanel default values on sample
        if (((ViewState["SAMPLE_ITEM_NEW_CONTAINER"] != null) || (Convert.ToBoolean(ViewState["SAMPLE_ITEM_NEW_CONTAINER"]))) && PLCSession.GetLabCtrl("USES_DBPANEL_DEFAULT_ON_SAMPLE") == "T")
        {
            PLCDBPanel1.SetPanelDefaultValues();
        }


        if ((PLCSession.GetLabCtrl("SUGGEST_ITEM_NUMBERS") == "T") && (string.IsNullOrEmpty(PLCDBPanel1.getpanelfield("LAB_ITEM_NUMBER"))))
        {
            string msgTitle = "New Item";
            if (ViewState["DUPE_ITEM"] != null && Convert.ToBoolean(ViewState["DUPE_ITEM"]))
                msgTitle = "Dupe Item";
            else if (ViewState["SAMPLE_ITEM_NEW_CONTAINER"] != null && Convert.ToBoolean(ViewState["SAMPLE_ITEM_NEW_CONTAINER"]))
                msgTitle = "Sample Item";

            ViewState["DUPE_ITEM"] = null;
            ViewState["SAMPLE_ITEM_NEW_CONTAINER"] = null;
            ViewState["NewRecordECN"] = null;

            dlgMessage.ShowAlert(msgTitle, "There is an error in generating the Lab Item Number. Please contact the LIMS Administrator.");
            e.button_canceled = true;
            SetPageBrowseMode(true);
            GrabGridRecord();
        }
        else if (string.IsNullOrEmpty(PLCSession.PLCGlobalSubmissionNumber))
        {
            dlgMessage.ShowAlert("New Item", "Item needs submission.");
            e.button_canceled = true;
            SetPageBrowseMode(true);
            GrabGridRecord();
        }
        else
        {
            if (!string.IsNullOrEmpty(CurrentECN) || !string.IsNullOrEmpty(ParentECN))
            {
                RecreateBlankAttributes(true);
            }
            else
            {
                // to force inequality (before and after) of item type field
                PLCDBPanel1.GetFlexBoxControl("ITEM_TYPE").ClearBeforePostbackValue();
            }

            if (bnSAK.Visible)
                dbpSAK.EmptyMode();

            SetPageBrowseMode(false);
            SetCurrencyTabText("");
            SetItemDescriptionRequired();
        }

        ShowDefaultTab();

        // set DBPanel defaults except for Sample and Kit
        if ((ViewState["SAMPLE_ITEM_NEW_CONTAINER"] == null) || (!Convert.ToBoolean(ViewState["SAMPLE_ITEM_NEW_CONTAINER"])))
        {
            // if Dup flag on, set DBPanel defaults if not Dupe button clicked
            if (PLCSession.GetLabCtrl("DUPE_COLLECTED_BY_AND_DATE") == "T")
            {
                if ((ViewState["DUPE_ITEM"] == null) || (!Convert.ToBoolean(ViewState["DUPE_ITEM"])))
                    PLCDBPanel1.SetPanelDefaultValues();
            }
            else
            {
                if ((ViewState["DUPE_ITEM"] == null) || (!Convert.ToBoolean(ViewState["DUPE_ITEM"])))
                    PLCDBPanel1.SetPanelDefaultValues();
            }

            if (PLCSession.GetLabCtrl("ANALYST_COLLECTED_BY_AND_DATE") == "T" && PLCSession.GetLabCtrl("DUPE_COLLECTED_BY_AND_DATE") != "T")
            {
                if ((ViewState["DUPE_ITEM"] != null) && (Convert.ToBoolean(ViewState["DUPE_ITEM"])))
                {
                    PLCDBPanel1.setpanelfield("COLLECTED_BY", PLCSession.PLCGlobalAnalyst);
                    PLCDBPanel1.setpanelfield("DATE_COLLECTED", DateTime.Now.ToString("MM/dd/yyyy"));
                    PLCDBPanel1.setpanelfield("TIME_COLLECTED", DateTime.Now.ToString("HH:mm"));
                }
            }
        }

        MultiView1.ActiveViewIndex = 0;
        foreach (string panelName in CustomTabLinks)
        {
            string tabName = panelName.Substring(14);
            PLCDBPanel dbpanel = (PLCDBPanel)MultiView1.FindControl("pnl" + tabName);
            dbpanel.IsNewRecord = true;
            dbpanel.EmptyMode();
            dbpanel.SetEditMode();
            HighLightCustomTabLink(tabName, false);
        }
    }

    if ((e.button_name == "EDIT") & (e.button_action == "AFTER"))
    {
        foreach (string panelName in CustomTabLinks)
        {
            string tabName = panelName.Substring(14);
            PLCDBPanel dbpanel = (PLCDBPanel)MultiView1.FindControl("pnl" + tabName);
            dbpanel.IsNewRecord = false;
            dbpanel.SetEditMode();
        }

        SetPageBrowseMode(false);
        //Ticket # 22449
        ViewState["ITASSIGN_ITEM_DESCRIPTION"] = PLCDBPanel1.getpanelfield("ITEM_DESCRIPTION").Trim();
        ViewState["NewRecordECN"] = null;
    }

    #endregion After Action

    #region Delete Items
    if ((e.button_name == "DELETE") & (e.button_action == "AFTER"))
    {
        DeleteHangingItemRecords();
        DeleteAttachmentsOnServer();

        GridView1.InitializePLCDBGrid();

        if (GridView1.Rows.Count > 0)
        {
            GridView1.SelectedIndex = 0;
            GridView1.SetClientSideScrollToSelectedRow();

            PLCSession.PLCGlobalECN = GridView1.SelectedDataKey.Value.ToString();
            UpdateCustodyStatusDisplay(PLCSession.PLCGlobalECN);

            GrabGridRecord();
        }
        else
        {
            PLCSession.PLCGlobalECN = "";
            PLCDBPanel1.EmptyMode();
            PLCButtonPanel1.SetEmptyMode();
            ItemAttribute.InitializeMultiEntries(1);
            SetAttachmentsButton("CASE");
            EnableButtonControls(false, false);
            ClearCustodyStatusDisplay();

            foreach (string panelName in CustomTabLinks)
            {
                string tabName = panelName.Substring(14);
                PLCDBPanel dbpanel = (PLCDBPanel)MultiView1.FindControl("pnl" + tabName);
                dbpanel.EmptyMode();
            }
        }
    }
    #endregion Delete Items

    #region Save Items
    if (e.button_name == "SAVE")
    {
        if (e.button_action == "BEFORE")
        {
            string itmCat = PLCDBPanel1.getpanelfield("ITEM_CATEGORY");
            String itmType = PLCDBPanel1.getpanelfield("ITEM_TYPE");


            foreach (string panelName in CustomTabLinks)
            {
                string tabName = panelName.Substring(14);
                PLCDBPanel dbpanel = (PLCDBPanel)MultiView1.FindControl("pnl" + tabName);
                if (!dbpanel.CanSave())
                {
                    mbox.ShowMsg("Save", "Unable to save this information. Please see " + tabName.Replace("_", " ") + " for errors in red.", 1);
                    e.button_canceled = true;
                    return;
                }
            }

            string errMsg;
            if (IsNameLinkRequired(itmCat, itmType, out errMsg) && !ItemHasNamesChecked())
            {
                string categoryPrompt = GetNameLinkPrompt(itmCat);
                string msgPrompt = !string.IsNullOrEmpty(categoryPrompt) ? categoryPrompt : "Please select a Name for this " + PLCSession.GetCodeDesc("TV_ITEMCAT", itmCat) + " category.";

                if (!string.IsNullOrEmpty(errMsg))
                {
                    msgPrompt = errMsg;
                }

                mbox.ShowMsg("Name Link", msgPrompt, 0);
                e.button_canceled = true;
                return;
            }

            if (e.row_added &&
                MEIMSHelper.IsMECase(PLCSession.SafeInt(PLCSession.PLCGlobalCaseKey)) &&
                PLCDBGlobal.instance.IsItemTypeRequireMEVerification(itmType) &&
                (MEIMSVerificationPrompt < MEIMSVerificationPrompts.ConfirmVerificationPrompt))
            {
                // ECN is not yet created
                vryMEIMSItem.CommandName = "Item";
                vryMEIMSItem.ShowVerification();
                e.button_canceled = true;
                return;
            }

            // Check if user needs to state reason for updating the case record
            bool hasChanges = CheckForChanges();
            string auditText = PrepareCaseNarrative();
            string reasonText = "";
            if (PLCSession.GetLabCtrl("LOG_EDITS_TO_NARRATIVE") == "T" && !PLCDBPanel1.IsNewRecord)
            {
                if (hdnConfirmUpdate.Value.Trim().Length > 0)
                {
                    SaveConfirmUpdate(auditText);
                    reasonText = hdnConfirmUpdate.Value;
                    hdnConfirmUpdate.Value = ""; // Clear flag to save case update changes
                }
                else if (hasChanges) // Changes in main dbpanel AND all details tabs
                {
                    mInput.ShowMsg("Case update reason", "Please enter the reason for your changes", 0, txtConfirmUpdate.ClientID, btnConfirmUpdate.ClientID, "Save", "Cancel");
                    e.button_canceled = true;
                    return;
                }
            }

            if (hasChanges)
            {
                dbgbl.MarkAssignmentsForRegeneration(PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, "LABITEM", auditText);
            }

            if (PLCDBPanel1.HasChanges() || ItemNameHasChanges() || (!bnAttribute.Disabled && ItemAttribute.HasChanges()) || (!bnCurrency.Disabled && Currency1.HasChanges()))
                PLCDBPanel1.ReasonChangeKey = dbgbl.GenerateReasonChange("ITEMS TAB SAVE", reasonText);
        }

        if (e.button_action == "AFTER")
        {

            if (!e.row_added)
                NotifyRetentionReviewChanged();

            foreach (string panelName in CustomTabLinks)
            {
                string tabName = panelName.Substring(14);
                PLCDBPanel dbpanel = (PLCDBPanel)MultiView1.FindControl("pnl" + tabName);
                dbpanel.SaveRecord();
            }

            SaveTabs();

            if (!e.row_added)
                PLCDBGlobal.instance.RemoveRecordLocks("TV_LABITEM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, "-1", PLCSession.PLCGlobalAnalyst);

            AddItemSortToLabItem(PLCSession.PLCGlobalECN, PLCDBPanel1.getpanelfield("LAB_ITEM_NUMBER"), PLCDBPanel1.getpanelfield("DEPARTMENT_ITEM_NUMBER"));
            // Ticket # 22449 - update ITASSIGN on Item Edit
            if (!e.row_added)
            {
                UpdateITAssignDescription(PLCSession.PLCGlobalECN, (string)ViewState["ITASSIGN_ITEM_DESCRIPTION"], PLCDBPanel1.getpanelfield("ITEM_DESCRIPTION"));

                int ecn = PLCSession.SafeInt(PLCSession.PLCGlobalECN);
                PLCDBGlobal.instance.SyncItemTasks(ecn);
            }

            PLCSession.CheckNarcoSubmission(PLCSession.PLCGlobalECN);

            if ((!e.row_added) && (!string.IsNullOrEmpty(PLCDBPanel1.getpanelfield("LAB_CASE_SUBMISSION"))) && (PLCDBPanel1.getpanelfield("LAB_CASE_SUBMISSION") != PLCDBPanel1.GetOriginalValue("LAB_CASE_SUBMISSION")))
            {
                //update submission-item link
                DeleteSubLinkItem(PLCSession.PLCGlobalECN);
                PLCDBGlobal.instance.AppendSubLinkItem(PLCSession.PLCGlobalECN, PLCDBPanel1.getpanelfield("LAB_CASE_SUBMISSION"), PLCDBPanel1.getpanelfield("ITEM_TYPE"), false);
            }

            UpdateBiologicalCourierDestination();

            bool isDupeOrSample = ((ViewState["DUPE_ITEM"] != null && Convert.ToBoolean(ViewState["DUPE_ITEM"])) ||
                                   (ViewState["SAMPLE_ITEM_NEW_CONTAINER"] != null && Convert.ToBoolean(ViewState["SAMPLE_ITEM_NEW_CONTAINER"])));
            bool excludeRMS = isDupeOrSample && PLCSession.GetLabCtrlFlag("EXCLUDE_RMS_EXTERNAL") == "T";

            if (PLCSession.GetLabCtrl("RMS_EXTERNAL_IS_VOUCHER") == "T" && !excludeRMS)
            {
                // rms_external does not exist in labsub, create new labsub record with command_voucher = rms_external
                if (!string.IsNullOrEmpty(PLCDBPanel1.getpanelfield("RMS_EXTERNAL").ToString().Trim()))
                {
                    string SubKey = "";
                    string SubNum = "";
                    if (!SubmissionCommandVoucherExists(PLCDBPanel1.getpanelfield("RMS_EXTERNAL").ToString(), ref SubKey, ref SubNum))
                        AddNewLabSubCommandVoucher(PLCDBPanel1.getpanelfield("RMS_EXTERNAL").ToString(), PLCSession.PLCGlobalECN);
                    else
                        // update submission number just in case user changes voucher# belonging to a different submission 
                        UpdateItemSubmissionNumber(PLCSession.PLCGlobalECN, SubKey, SubNum);
                }
            }

            string envelopeMessage = "";

            if (e.row_added)
            {
                if (MEIMSVerificationPrompt == MEIMSVerificationPrompts.ConfirmVerificationPrompt)
                {
                    int ecn = PLCSession.SafeInt(PLCSession.PLCGlobalECN);
                    vryMEIMSItem.SaveVerification(ecn);

                    MEIMSVerificationPrompt = MEIMSVerificationPrompts.None;
                }

                //add default custody
                if (ViewState["DUPE_ITEM"] != null && Convert.ToBoolean(ViewState["DUPE_ITEM"]))
                {
                    if (PLCSession.GetLabCtrlFlag("DO_NOT_CREATE_CUSTODY_FOR_ITEM") != "T")
                    {
                        //check if item has a parent item
                        //if not, check SUBMISSION_CUSTODY_TYPE flag and create two custody records linked to the submission
                        bool analystCustodyAlreadyExists = false;
                        PLCQuery qryParent = new PLCQuery("SELECT PARENT_ECN FROM TV_LABITEM WHERE EVIDENCE_CONTROL_NUMBER = " + PLCSession.PLCGlobalECN);
                        qryParent.Open();
                        if (qryParent.HasData() && string.IsNullOrEmpty(qryParent.FieldByName("PARENT_ECN")))
                            analystCustodyAlreadyExists = CreateCustodyRecordsForSubmissionItem();

                        //for duplicate item
                        PLCSession.AddDefaultCustody(PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, null, null, "DUPE", analystCustodyAlreadyExists, dupeECN: DupeECN);
                    }
                    DupeECN = null;
                    ViewState["DUPE_ITEM"] = null;
                }
                else if (ViewState["SAMPLE_ITEM_NEW_CONTAINER"] != null && Convert.ToBoolean(ViewState["SAMPLE_ITEM_NEW_CONTAINER"]))
                {
                    //for sample item
                    if (PLCSession.GetLabCtrlFlag("DO_NOT_CREATE_CUSTODY_FOR_ITEM") != "T")
                        PLCSession.AddDefaultCustody(PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, null, null, "SAMPLE");

                    //Ticket#42220 - Update RMS Interface for item sampling.
                    string ecn = PLCSession.PLCGlobalECN;
                    string departmentCaseNumber = PLCDBGlobal.instance.GetTableDetailsByCustomKey("TV_LABCASE", "DEPARTMENT_CASE_NUMBER", "CASE_KEY", PLCSession.PLCGlobalCaseKey);
                    string thirdPartyBarcode = PLCDBGlobal.instance.GetTableDetailsByCustomKey("TV_LABITEM", "THIRD_PARTY_BARCODE", "EVIDENCE_CONTROL_NUMBER", ecn);

                    PLCDBGlobal.instance.UpdateRMSDerivativeEvidence(PLCSession.PLCGlobalCaseKey, thirdPartyBarcode, PLCDBPanel1.getpanelfield("LAB_ITEM_NUMBER"), ecn);
                }
                else
                {
                    if (!string.IsNullOrEmpty(FloorCustody)
                        && !string.IsNullOrEmpty(FloorLocation))
                    {
                        PLCSession.AddCustody(PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, FloorCustody, FloorLocation);
                    }
                    else
                    {
                        if (PLCSession.GetLabCtrlFlag("DO_NOT_CREATE_CUSTODY_FOR_ITEM") != "T")
                        {
                            //check SUBMISSION_CUSTODY_TYPE flag and create two custody records linked to the submission
                            bool analystCustodyAlreadyExists = CreateCustodyRecordsForSubmissionItem();

                            //for new item
                            PLCSession.AddDefaultCustody(PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, analystCustodyAlreadyExists);
                        }
                    }
                }


                envelopeMessage = PLCDBGlobal.instance.UpdateItemRecordBasedOnContainerType(PLCDBPanel1.getpanelfield("ITEM_TYPE"), PLCSession.SafeInt(PLCSession.PLCGlobalECN), 0);

                PLCDBGlobal.instance.Set_Default_Process(PLCSession.PLCGlobalECN, "");
                Create_Default_Service_Request(PLCSession.PLCGlobalECN);

                //save new sublink info
                if (!string.IsNullOrEmpty(PLCDBPanel1.getpanelfield("LAB_CASE_SUBMISSION")))
                    PLCDBGlobal.instance.AppendSubLinkItem(PLCSession.PLCGlobalECN, PLCDBPanel1.getpanelfield("LAB_CASE_SUBMISSION"), PLCDBPanel1.getpanelfield("ITEM_TYPE"), false);
                if (PLCSession.GetLabCtrl("AUTO_PRINT_ITEM_LABELS") == "T")
                {
                    bool hasResponse;
                    string redirectString;
                    PLCDBGlobal.instance.PrintLASDLabel(PLCSession.PLCGlobalECN, out hasResponse, out redirectString);
                    if (hasResponse)
                        Response.Redirect(redirectString);
                }

                if (PLCSession.GetLabCtrl("DEFAULT_ASSIGNMENTS") == "T")
                {
                    PLCDBGlobal.instance.AddItemDefaultAssignment(PLCSession.PLCGlobalECN, true);
                    SetHeaderLabel();

                }

                if (ViewState["SAMPLE_ITEM_NEW_CONTAINER"] != null)
                {
                    ViewState["SAMPLE_ITEM_PROCESS"] = ViewState["SAMPLE_ITEM_NEW_CONTAINER"];
                    if (Convert.ToBoolean(ViewState["SAMPLE_ITEM_NEW_CONTAINER"]))
                    {
                        if (PLCSession.GetLabCtrl("USES_SAMPLE_CONTAINER") == "T")
                        {
                            mbSampleContainer.Show();
                        }
                        ViewState["SAMPLE_ITEM_NEW_CONTAINER"] = null;
                    }
                }

                (new PLCDBGlobal()).UpdateItemWeightBasedOnAttribute(PLCSession.PLCGlobalECN);
            }
            else // on update of item
            {
                string oldItemType = PLCDBPanel1.GetOriginalValue("ITEM_TYPE");
                string newItemType = PLCDBPanel1.getpanelfield("ITEM_TYPE");

                if (!oldItemType.Equals(newItemType))
                {
                    envelopeMessage = PLCDBGlobal.instance.UpdateItemRecordBasedOnContainerType(newItemType, PLCSession.SafeInt(PLCSession.PLCGlobalECN), 0, true);
                }

            }

            List<PLCDialog.Alert> alerts = new List<PLCDialog.Alert>();

            if (CanTransferItemToDestroyedLocation)
            {
                string message = string.Empty;

                message = PLCDBGlobal.instance.TransferItemToDestroyedLocation(Convert.ToInt32(PLCSession.PLCGlobalECN));

                if (!string.IsNullOrEmpty(message))
                {
                    alerts.Add(new PLCDialog.Alert()
                    {
                        Title = Page.Title,
                        Message = message
                    });
                }
            }

            if (alerts.Count > 0)
            {
                dlgMessage.ShowAlerts(alerts);
            }


            //refresh grid
            GridView1.InitializePLCDBGrid();
            GridView1.SelectRowByDataKey(PLCSession.PLCGlobalECN);

            //show custody location
            UpdateCustodyStatusDisplay(PLCSession.PLCGlobalECN);

            PLCDBPanel1.ReasonChangeKey = 0;
            ItemAttribute.IsItemTypeChanged = false;
            GrabGridRecord();
            ViewState["NewRecordECN"] = null;


            if (!string.IsNullOrWhiteSpace(envelopeMessage))
            {
                dlgMessage.ShowAlert("Item Envelope", envelopeMessage);
                return;

            }

            //display aftermessage here
            PostSavingMessage();

        }
    }

    #endregion Save Items

    if ((e.button_name == "CANCEL") & (e.button_action == "AFTER"))
    {
        string currKey = "";
        if (GridView1.Rows.Count > 0)
        {
            currKey = GridView1.SelectedDataKey.Value.ToString();
        }


        SetItemTypeFilter(true);

        ViewState["SAMPLE_ITEM_NEW_CONTAINER"] = null;      // Reset Sample add state.
        ViewState["DUPE_ITEM"] = null;      // Reset Dupe add state.
        ViewState["NewRecordECN"] = null;
        PLCSession.SetDefault("LABSTAT_COMMENTS", null);

        if (!e.row_added)
            PLCDBGlobal.instance.RemoveRecordLocks("TV_LABITEM", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, "-1", PLCSession.PLCGlobalAnalyst);

        GridView1.InitializePLCDBGrid();
        GridView1.SelectRowByDataKey(PLCSession.PLCGlobalECN);

        if (GridView1.Rows.Count > 0)
        {
            if (!string.IsNullOrEmpty(currKey))
            {
                if (!CurrentItemExists(currKey)) //the item has been deleted by another user
                {
                    string message = "The currently selected item has been deleted by another user.";
                    string script = "function () { window.location = 'Tab4Items.aspx'; }";
                    ServerAlert(message, script);
                    e.button_canceled = true;
                    return;
                }
                else
                {
                    string selectedKey = "";
                    try
                    {
                        selectedKey = GridView1.SelectedDataKey.Value.ToString();
                    }
                    catch (Exception ex)
                    {

                    }

                    if (string.IsNullOrEmpty(selectedKey)) //reselect the previously selected row if item exists and not deleted by another user
                        GridView1.SelectRowByDataKey(currKey);


                }
            }


        }

        GrabGridRecord();

        if (GridView1.Rows.Count <= 0)
        {
            foreach (string panelName in CustomTabLinks)
            {
                string tabName = panelName.Substring(14);
                PLCDBPanel dbpanel = (PLCDBPanel)MultiView1.FindControl("pnl" + tabName);
                dbpanel.IsNewRecord = false;
                dbpanel.EmptyMode();
                dbpanel.SetBrowseMode();
            }
        }

        LoadNames();
        LoadCurrencyData();

        RecreateBlankAttributes(false);
        ItemTypeChangeConfirmed = false;

        ViewState["AttributesChanges"] = null;
        ViewState["CurrencyChanges"] = null;
        ViewState["ItemNamesChanges"] = null;
    }

    if (e.button_name == "RecordUnlock")
    {
        if (string.IsNullOrEmpty(PLCSession.PLCGlobalECN))
        {
            mbox.ShowMsg("Item", "Please select an Item.", 1);
            return;
        }

        mInput.ShowMsg("Unlock record", "Please enter why you have to unlock the record ?", 0, UserComments.ClientID, MsgCommentPostBackButton.ClientID);
        return;
    }

    if (e.button_name == "SAVE" && e.button_action == "AFTER")
    {
        SetItemTypeFilter(true);
        PLCSession.WriteDebug("Save After, ITEM_STAT_DEFAULT=" + PLCSession.GetLabCtrl("ITEM_STAT_DEFAULT"));

        if (PLCSession.GetLabCtrl("ITEM_STAT_DEFAULT") == "T")
        {
            PLCSession.WriteDebug("ITEM_STAT_DEFAULT 1");
            PLCHelperFunctions plcHelp = new PLCHelperFunctions();
            plcHelp.SetItemDefaultStatus(PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalECN, true);
            PLCSession.WriteDebug("ITEM_STAT_DEFAULT 2");
            PLCDBGlobal.instance.UpdateNextReviewDate(PLCSession.PLCGlobalECN);
            PLCSession.WriteDebug("ITEM_STAT_DEFAULT 3");

            //refresh grid and dbpanel
            GridView1.InitializePLCDBGrid();
            GridView1.SelectRowByDataKey(PLCSession.PLCGlobalECN);
            GrabGridRecord();
        }

        //Add record of Sampled Item to TV_REVREQHIST
        if ((ViewState["SAMPLE_ITEM_PROCESS"] != null) || (Convert.ToBoolean(ViewState["SAMPLE_ITEM_PROCESS"])))
        {
            if (!string.IsNullOrWhiteSpace(PLCSession.PLCGlobalECN))
            {
                PLCQuery qrySampleItem = new PLCQuery("SELECT * FROM TV_LABITEM WHERE EVIDENCE_CONTROL_NUMBER = " + PLCSession.PLCGlobalECN);
                qrySampleItem.Open();
                if (!qrySampleItem.IsEmpty())
                    PLCDBGlobal.instance.AddRevReqHistory(PLCSession.PLCGlobalECN, "", qrySampleItem.FieldByName("PROCESS"), "SampleItem - TAB4Items", PLCSession.PLCGlobalAnalyst, 0);
            }
        }
    }

    if ((e.button_name == "ADD" || e.button_name == "EDIT") && e.button_action == "AFTER")
    {
        SetItemTypeFilter();

        if (PLCSession.CheckUserOption("ITEMSTAT"))
        {
            PLCDBPanel1.EnablePanelField("PROCESS", false);
            //PLCDBPanel1.SetMyFieldMode("PROCESS", true);
        }
    }

    if (e.button_name == "Bulk Delete")
    {
        if (GridView1.Rows.Count > 0)
        {
            LoadBulkDeletePopup();
            ScriptManager.RegisterStartupScript(btnShowBulkPopup, btnShowBulkPopup.GetType(), "Popup", "BDialogOpen(true);", true);
        }
    }

    if (e.button_name == "Verification")
    {
        int ecn = PLCSession.SafeInt(PLCSession.PLCGlobalECN);
        vryMEIMSItem.ShowVerification(ecn);
    }
}