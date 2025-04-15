protected void bnSave_Click(object sender, EventArgs e)
        {
            //check required fields
            string errorMessage = "";

            if (GridView1.Rows.Count == 0)
            {
                ShowMessage("Info", "Please scan items or containers.", MessageBoxType.Information, string.Empty);
                return;
            }

            if (!dbpTransfer.CanSave())
                return;

            errorMessage = CheckRequiredWeightAndUnits();
            if (!string.IsNullOrEmpty(errorMessage))
            {
                ShowMessage("Info", "<div style='overflow:auto; max-height: 170px;'>Please fill in the following required fields. <br/><br/>" + errorMessage + "</div>", MessageBoxType.Information, string.Empty);
                return;
            }

            foreach (GridViewRow row in GridView1.Rows)
            {
                //check if refusal reason is required and filled-out
                if (IsRefusalRightEnabled && ((CodeMultiPick)row.FindControl("mpRefuseReason")).GetText().Trim() == "")
                {
                    mbox.ShowMsg("Transfer Custody", "Refusal reason for each selected item/container is mandatory.", 0);
                    if (mbox.FindControl("OkButton") != null)
                    {
                        System.Web.UI.ScriptManager.RegisterStartupScript(Page, Page.GetType(), "SetPopupFocus",
                            "setTimeout(function() { $('#" + ((Button)mbox.FindControl("OkButton")).ClientID + "').focus(); }, 700);", true);
                    }
                    return;
                }
            }

            // Re-validation on save
            if (!RemoveFromContainerConfirmed && PLCSession.PLCDataSet2.Tables["ItemContainerToRemove"] != null)
            {
                PLCQuery qryDesc;
                List<string> lstContainerInBulk = new List<string>();
                List<string> lstItemInContainer = new List<string>();

                DataTable dtRemove = PLCSession.PLCDataSet2.Tables["ItemContainerToRemove"];
                foreach (DataRow row in dtRemove.Rows)
                {
                    string key = row["ECN"].ToString();
                    bool isItem = Convert.ToBoolean(row["IsItemType"].ToString());

                    if (isItem)
                    {
                        qryDesc = new PLCQuery("SELECT I.LAB_ITEM_NUMBER, L.DEPARTMENT_CASE_NUMBER, T.DESCRIPTION AS TYPEDESC FROM TV_LABITEM I " +
                            "LEFT OUTER JOIN TV_LABCASE L ON I.CASE_KEY = L.CASE_KEY " +
                            "LEFT OUTER JOIN TV_ITEMTYPE T ON I.ITEM_TYPE = T.ITEM_TYPE " +
                            "WHERE I.EVIDENCE_CONTROL_NUMBER = ?");
                        qryDesc.AddSQLParameter("EVIDENCE_CONTROL_NUMBER", key);
                        qryDesc.Open();

                        if (PLCSession.GetLabCtrlFlag("NO_PROMPT_ITEM_FROM_CONTAINER") != "T")
                            lstItemInContainer.Add(qryDesc.FieldByName("DEPARTMENT_CASE_NUMBER") + ", Item# " + qryDesc.FieldByName("LAB_ITEM_NUMBER") + " - " + qryDesc.FieldByName("TYPEDESC"));
                    }
                    else
                    {
                        qryDesc = new PLCQuery("SELECT C.CONTAINER_DESCRIPTION, L.DEPARTMENT_CASE_NUMBER, P.DESCRIPTION AS TYPEDESC FROM TV_CONTAINER C " +
                            "LEFT OUTER JOIN TV_LABCASE L ON C.CASE_KEY = L.CASE_KEY " +
                            "LEFT OUTER JOIN TV_PACKTYPE P ON C.PACKAGING_CODE = P.PACKAGING_CODE " +
                            "WHERE C.CONTAINER_KEY = ?");
                        qryDesc.AddSQLParameter("CONTAINER_KEY", key);
                        qryDesc.Open();

                        lstContainerInBulk.Add(qryDesc.FieldByName("DEPARTMENT_CASE_NUMBER") + ", Container: " +
                            (string.IsNullOrEmpty(qryDesc.FieldByName("CONTAINER_DESCRIPTION")) ? "" : qryDesc.FieldByName("CONTAINER_DESCRIPTION") + " - ") + qryDesc.FieldByName("TYPEDESC"));
                    }
                }

                if (lstContainerInBulk.Count > 0 || lstItemInContainer.Count > 0)
                {
                    string message = string.Empty;
                    if (lstContainerInBulk.Count > 0)
                        message += "This container(s) is in a bulk. Are you sure you want to remove from the bulk container?<br/>" + string.Join("<br/>", lstContainerInBulk);

                    if (lstItemInContainer.Count > 0)
                        message += (string.IsNullOrEmpty(message) ? "" : "<br/><br/>") + "This item(s) is in a container. Are you sure you want to remove from the container?<br/>" + string.Join("<br/>", lstItemInContainer);

                    EnableTxtBarcode(false);
                    dlgMessage.ShowConfirm("Confirm", message);
                    return;
                }
            }

            Custody custody = null;

            try
            {
                custody = new Custody(PLCSession.TransferCustody);
            }
            catch
            {
            }

            if (!OpenAssignmentConfirmed && custody != null && custody.DisplayOpenAssignMessage && HasOpenAssignments())
            {
                ShowMessage("Confirm", "This transfer contains item(s) with open assignment(s). Would you like to continue?", MessageBoxType.Confirmation, "OPEN_ASSIGNMENT");
                return;
            }

            if (PLCSessionVars1.GetLabCtrl("ALL_ITEM_SUBMISSION_CHECK") == "T")
            {
                DataSet DS = PLCSessionVars1.PLCDataSet;
                DataTable DT = DS.Tables[0];

                string sItemECNs = string.Empty;

                foreach (DataRow dr in DT.Rows)
                {
                    if (sItemECNs.Length > 0)
                    {
                        sItemECNs += ",";
                    }

                    sItemECNs += dr["KEY"].ToString();
                }

                string sSubItemSummary = PLCDBGlobalClass.GetSubmissionItemSummary(sItemECNs, PLCSessionVars1.PLCGlobalCaseKey, PLCSessionVars1.GetLabCtrl("SHOW_LAB_CASE_NUMBER"));

                if (!string.IsNullOrEmpty(sSubItemSummary) && PLCCommonClass.IsCustodySubmissionCheck(dbpTransfer.CustodyCode, PLCSessionVars1.GetLabCtrl("ALL_ITEM_SUBM_CUSTODY")))
                //if (!string.IsNullOrEmpty(sSubItemSummary))
                {
                    SubBatch.Show(sSubItemSummary, false, btnProcBCTransfer.ClientID);
                }
                else
                {
                    ProcessBCTransfer();
                }
            }
            else
            {
                ProcessBCTransfer();
            }
        }

private void ProcessBCTransfer(ConfirmationLevel confirmationLevel = ConfirmationLevel.None)
        {
            //
            DataSet DS = PLCSessionVars1.PLCDataSet;
            DataTable DT = DS.Tables[0];
            if (DT.Rows.Count == 0)
                return;
            UpdateUserInput(DT);

            DataSet DSP = PLCSessionVars1.PLCDataSet2;
            DataTable DTP = DSP.Tables["PASSWORD"];



            //check passwords
            string errorMessage;
            bool isTransferValidated = tppBCTransfer.IsTransferAuthenticated(out errorMessage);
            if (!isTransferValidated)
            {
                mbox.ShowMsg("Transfer Custody", errorMessage, 1);
                return;
            }

            if (confirmationLevel < ConfirmationLevel.EvidenceRetained)
            {
                string message;
                if (HasPromptOnReturnCustody(out message))
                {
                    EnableTxtBarcode(false);
                    dlgMessage.DialogKey = ConfirmationLevel.EvidenceRetained.ToString();
                    dlgMessage.ShowConfirm("Confirm", message.Replace("'", "\\'"));
                    return;
                }
            }

            //bnSave.Enabled = false;
            //bnCancel.Enabled = false;

            // *AAC* Modified get sequence
            //PLCQuery qryNextKey = new PLCQuery();
            //qryNextKey.SQL = "SELECT BATCH_SEQ.NEXTVAL NV FROM DUAL";
            //qryNextKey.Open();
            //string batchseq = qryNextKey.FieldByName("NV");
            string batchseq = PLCSessionVars1.GetNextSequence("BATCH_SEQ").ToString();

            PLCSessionVars1.PLCGlobalBatchKey = batchseq;
            PLCSessionVars1.SetProperty<string>("BC_TRANSFER_BATCH_SEQ", batchseq);

            if (cbGetSig.Checked)
            {
                if (cbPrintReceipt.Checked)
                    PLCSessionVars1.WhatIsNext = "PRINTTRANSFER";
                else
                    PLCSessionVars1.WhatIsNext = "SIGCOMPLETE";

                string SignaturePadType = PLCSessionVars1.PLCGlobalSignaturePad;

                if (String.IsNullOrEmpty(SignaturePadType))
                {
                    mpeNoSigPadSetup.Show();
                }
                else
                {
                    PLCSession.SetCaseVariables("", "", "");
                    if (SignaturePadType == "TOPAZ")
                    {
                        SignaturePad.Show(btnSaveWithSigPad.ClientID);
                    }
                    else if (SignaturePadType == "EPAD")
                    {
                        uctGetSig_Epad.Show(btnSaveWithSigPad.ClientID);
                    }
                }

            }
            else
            {
                if (PLCSessionVars1.TransferCustody != "" && PLCSessionVars1.TransferLocation != "")
                    TransferItems(batchseq, cbPrintReceipt.Checked);
                else
                {
                    ShowMessage("Scanned Custody and Location", "Please scan a location.", MessageBoxType.Error, string.Empty);
                }
            }
        }

protected void TransferItems(string batchseq, bool PrintReceipt)
        {
            bool locationIsMarkedCPD = (PLCDBGlobal.instance.GetCustodyLocationDetails(PLCSessionVars1.TransferCustody, PLCSessionVars1.TransferLocation, "RETURN_TO_CPD") == "T");

            // remove item or container from container or bulk
            ProcessItemContainerToRemoveFromContainer();
            //
            // mbox.ShowMsg("Transfer", "Transfering Items please wait..!", 0);
            DataSet DS = PLCSessionVars1.PLCDataSet;
            DataTable DT = DS.Tables[0];
            if (DT.Rows.Count == 0)
                return;

            string CustodyDept = "";
            PLCQuery qryCustDesc = new PLCQuery();
            qryCustDesc.SQL = "select * from TV_CUSTCODE where CUSTODY_TYPE = '" + PLCSessionVars1.TransferCustody + "'";
            qryCustDesc.Open();
            if (!qryCustDesc.EOF())
            {
                CustodyDept = qryCustDesc.FieldByName("DEPARTMENT_CODE");
            }

            string defaultTransferTaskStatus;
            string defaultTransferTask = PLCDBGlobalClass.GetDefaultTransferTasks(PLCSession.TransferCustody, PLCSession.TransferLocation, out defaultTransferTaskStatus);
            string ProcessCode = string.Empty;
            ProcessCode = PLCDBGlobalClass.GetProcessCode(PLCSession.TransferCustody, PLCSession.TransferLocation);

            try
            {
                // start db transaction
                PLCQuery.BeginDBTransaction();

                string statuskey = "";
                PLCQuery qryNextKey = new PLCQuery();
                DateTime thisdate = Convert.ToDateTime(dbpTransfer.getpanelfield("STATUS_DATE"));
                DateTime thistime = Convert.ToDateTime(dbpTransfer.getpanelfield("STATUS_TIME"));
                bool ItemsTransfered = false;
                string Key;

                Dictionary<string, object> itemList = new Dictionary<string, object>();
                itemList.Clear();

                var assignmentGenerator = new AssignmentGenerator();

                for (int I = 0; I < DT.Rows.Count; I++)
                {
                    DataRow DR = DT.Rows[I];
                    string Type = DR["BARCODETYPE"].ToString();
                    Key = DR["KEY"].ToString();
                    string Weight = DR["WEIGHT"].ToString().Trim();
                    string WeightUnits = DR["WEIGHTUNIT"].ToString().Trim();
                    string reasonCodes = DR["REASON_CODES"].ToString().Trim();
                    string otherReason = DR["OTHER_REASON"].ToString().Trim();

                    if (Key != "")
                    {
                        // *AAC* Modified getting sequence
                        //qryNextKey.SQL = "SELECT LABSTAT_SEQ.NEXTVAL NV FROM DUAL";
                        //qryNextKey.Open();
                        //statuskey = qryNextKey.FieldByName("NV");
                        statuskey = PLCSessionVars1.GetNextSequence("LABSTAT_SEQ").ToString();

                        if (Type == "C")  // container
                        {
                            UpdateContainer(Key, statuskey, thisdate, thistime, Weight, WeightUnits, ProcessCode, reasonCodes, otherReason, defaultTransferTask, defaultTransferTaskStatus, true);
                            ItemsTransfered = true;
                        }
                        else if (Type == "L")  // list
                        {
                            UpdateList(ref itemList, Key, batchseq, thisdate, thistime, string.Empty, CustodyDept, Weight, WeightUnits, reasonCodes, otherReason, defaultTransferTask, defaultTransferTaskStatus);
                            ItemsTransfered = true;
                        }
                        else //Item
                        {
                            if (UpdateItem(ref itemList, Key, batchseq, statuskey, thisdate, thistime, string.Empty, string.Empty, CustodyDept, Weight, WeightUnits, ProcessCode, reasonCodes, otherReason, defaultTransferTask, defaultTransferTaskStatus))
                            {
                                PLCDBGlobalClass.UpdateItemWeightAndUnit(Key, Weight, WeightUnits);
                                ItemsTransfered = true;
                                var item = new PLCCONTROLS.Objects.Core.Item(Key);
                                assignmentGenerator.Items.Add(item);
                            }
                        }
                    }
                }

                var entryAnalyst = new Analyst(PLCSession.PLCGlobalAnalyst);
                assignmentGenerator.CreateBasedOnItemServiceRequest(entryAnalyst);

                //if (itemList.Count > 0 && locationIsMarkedCPD)
                //    PLCDBGlobal.instance.GenerateISPManifestFile(Convert.ToInt32(batchseq), itemList, Server);

                PLCSession.PLCGlobalECN = string.Empty;
                PLCSession.PLCGlobalCaseKey = PLCSession.GetDefault("BC_CURRENT_CASE_KEY");
                // restore case variables too for audit log
                int bcCurrentCaseKey = PLCSession.SafeInt(PLCSession.PLCGlobalCaseKey);
                if (bcCurrentCaseKey > 0)
                {
                    PLCSession.SetCaseVariables(PLCSession.PLCGlobalCaseKey);
                }

                if (ItemsTransfered)
                {
                    bool usesTransferConfirm = PLCSessionVars1.GetLabCtrl("USES_TRANSFER_CONFIRM") == "T";
                    string message;
                    string status = TransferCore.ExecuteCustLocSP(PLCSession.TransferCustody, PLCSession.TransferLocation, Convert.ToInt32(batchseq), out message);

                    // save attachments
                    PLCDBGlobalClass.SaveTransferImages(batchseq);

                    //reset password after the transfer
                    tppBCTransfer.ResetPasswordPanel();

                    //Update The status of lists to make sure
                    PLCDBGlobal.instance.UpdateDeliveryRequestStatus(batchseq);

                    // After successful item transfer, the collected subitems dropped by the user will fill in their barcode.
                    if (!string.IsNullOrEmpty(DroppedSubItems))
                    {
                        List<string> droppedSubItems = DroppedSubItems.ToString()
                            .Trim()
                            .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(a => a.Trim())
                            .ToList();

                        foreach (string ecn in droppedSubItems)
                        {
                            SetBarcodeForSubItem(ecn);
                        }

                        DroppedSubItems = string.Empty;
                    }

                    // commit db transaction
                    PLCQuery.Commit();

                    bool hasOtherReport = false;
                    bool printERReport = false;
                    if (PLCSession.GetLabCtrlFlag("PRINT_EVIDENCE_RETAINED_REPORT") == "T")
                    {
                        string custody = PLCSession.TransferCustody;
                        var returnCustodyCodes = PLCDBGlobal.instance.GetReturnCustodyCodes();
                        if (!string.IsNullOrEmpty(custody) && returnCustodyCodes.Contains(custody))
                        {
                            printERReport = PrepareEvidenceRetainedReport(DT);
                            hasOtherReport = printERReport;
                        }
                    }

                    var notInLab = PLCDBGlobal.instance.NotInLab(PLCSession.TransferCustody, PLCSession.TransferLocation);

                    String reportName = PLCDBGlobalClass.GetTransferReportName(PLCSession.TransferCustody, PLCSession.TransferLocation);
                    string reportPath = PLCSession.FindCrystalReport(reportName);
                    PLCSessionVars1.PLCCrystalReportName = string.IsNullOrEmpty(reportPath) ? "~\\Reports\\" + PLCSessionVars1.PLCDatabaseServer + "\\" + reportName : reportPath;
                    PLCSessionVars1.PLCCrystalSelectionFormula = "{TV_LABSTAT.BATCH_SEQUENCE} = " + batchseq;
                    PLCSessionVars1.PLCCrystalReportTitle = "Transfer Receipt";

                    PLCSessionVars1.ClearTransferInfo();

                    string alertMessage = string.Empty;
                    string callbackFn = string.Empty;
                    if (!string.IsNullOrEmpty(message))
                    {
                        alertMessage = message.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace(Environment.NewLine, " ").Replace("\r", "").Replace("\n", " ").Replace("\t", " ");
                        callbackFn = "function () { ShowTransferUpdatePopup(" + usesTransferConfirm.ToString().ToLower() + ", 'dashboard.aspx'); }";
                    }

                    if (printERReport)
                    {
                        bool redirect = string.IsNullOrEmpty(alertMessage)
                            && (IsIEBrowser() || !PrintReceipt);
                        PrintEvidenceRetainedReport(redirectPrint: redirect);
                    }

                    if (PrintReceipt)
                    {
                        bnSave.Enabled = false;
                        PLCSessionVars1.WhatIsNext = "~/dashboard.aspx";

                        int pdfDataKey = 0;
                        if (PLCSession.GetLabCtrlFlag("STORE_EVIDENCE_RETURN_RECEIPT").Equals("T") && notInLab && PLCDBGlobal.instance.IsCrystalReportExist(reportName))
                        {
                            try
                            {
                                // save to pdf data
                                String pdfName = PLCDBGlobal.instance.SaveReportAsPDF("TRECEIPT-" + Guid.NewGuid().ToString() + ".pdf", PLCSession.PLCCrystalReportName, PLCSession.PLCCrystalSelectionFormula,
                                    PLCSession.PLCCrystalReportFormulaList, PLCSession.PLCCrystalReportTitle);
                                pdfDataKey = PLCDBGlobal.instance.SaveFileToPDFData(batchseq, "TRECEIPT", pdfName, false);

                                if (PLCSession.GetLabCtrlFlag("STORE_CASE_SPECIFIC_RECEIPTS").Equals("T"))
                                {
                                    PLCDBGlobal.instance.CreateCaseSpecificTransferReceipts(batchseq, pdfDataKey);
                                }
                            }
                            catch { }
                        }

                        if (pdfDataKey > 0)
                        {
                            if (locationIsMarkedCPD)
                            {
                                PLCSession.PLCCrystalReportName = "BCLIST";
                                PLCSession.SetDefault("BARCODE_KEYLIST", "PDFDATAKEY:" + pdfDataKey);
                                ScriptManager.RegisterStartupScript(Page, typeof(Page), "uniqueKey" + DateTime.Now,
                                    "window.open('" + PLCSession.GetApplicationURL(Request) + "/PrintBC_List.aspx?next=close','_blank','menubar=no,titlebar=no,status=yes,toolbar=no,location=no,resizable=yes');", true);
                            }
                            else if (string.IsNullOrEmpty(alertMessage) && IsIEBrowser())
                            {
                                PLCSession.SetDefault("BCTRANS_REPORT", "T");
                                PLCSession.SetDefault("BCTRANS_REPORT_KEY", pdfDataKey.ToString());
                            }
                            else
                            {
                                PLCSession.PrintPDFData(pdfDataKey);
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(alertMessage) && IsIEBrowser())
                                PLCSession.SetDefault("BCTRANS_REPORT", "T");
                            else
                                PLCSessionVars1.PrintCRWReport(false);
                        }

                        if (string.IsNullOrEmpty(alertMessage))
                            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), "redirect", "window.location = 'dashboard.aspx';", true);
                        else
                        {
                            callbackFn = "function () { ShowTransferUpdatePopup(false, 'TAB4Items.aspx'); }";
                            ServerAlert(alertMessage, callbackFn);
                        }
                    }
                    else if (!string.IsNullOrEmpty(alertMessage))
                    {
                        ServerAlert(alertMessage, callbackFn);
                    }
                    else if (hasOtherReport)
                    {
                        // redirect using javascript so that reports are shown first
                        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), "redirect", "window.location = 'dashboard.aspx';", true);
                    }
                    else if (usesTransferConfirm)
                    {
                        upokTransfer.RedirectTo = PLCSession.GetProperty<string>("PreviousPage", string.Empty);
                        upokTransfer.Show();
                    }
                    else
                        Response.Redirect(PLCSession.GetProperty<string>("PreviousPage", string.Empty));
                }
                else
                {
                    PLCQuery.Rollback();
                }
            }
            catch (Exception e)
            {
                // rollback db transaction
                PLCQuery.Rollback();

                PLCSession.WriteDebug("@Transfer: " + e.Message + "\n" + e.StackTrace);
                dlgMessage.ShowAlert("Error", e.Message);
            }
        }