if ((e.button_name == "SAVE") & (e.button_action == "BEFORE"))
{
    if (CheckHasHoldInRestriction())
    {
        e.button_canceled = true;
        messageBox.ShowMsg("Hold In Restriction", "Cannot transfer item, ''Hold-In Department'' set to ''" + hdnDepartmentName.Value + "''.", 0);
        return;
    }

    if (!CheckTransferAuthorization())
    {
        e.button_canceled = true;
        hdnButtonStatus.Value = "TRANSFER_AUTHORIZATION";
        return;
    }

    if ((hdnButtonStatus.Value != "FINISHED") && (IsTransferPasswordRequired(dbpTransfer.CustodyCode, dbpTransfer.LocationCode)))
    {
        // force non-save until password is entered
        e.button_canceled = true;
        hdnButtonStatus.Value = "SAVE";
        string TransferLocation = dbpTransfer.LocationCode;

        AddPasswordPanel(TransferLocation);

        return;
    }

    string reasonText = "";
    bool requireReasonForChange = PLCSession.GetLabCtrlFlag("REQUIRE_REASON_FOR_CHANGE").Equals("T");
    bool logEditsToNarative = PLCSession.GetLabCtrlFlag("LOG_EDITS_TO_NARRATIVE").Equals("T");
    if ((logEditsToNarative || requireReasonForChange) && !dbpTransfer.IsNewRecord)
    {
        if (hdnConfirmUpdate.Value.Trim().Length > 0)
        {
            if (logEditsToNarative)
                SaveConfirmUpdate(PrepareCaseNarrative());

            reasonText = hdnConfirmUpdate.Value;
            hdnConfirmUpdate.Value = "";
        }
        else if (dbpTransfer.HasChanges(PLCSession.GetLabCtrl("LOG_BLANK_TO_DATA_CHANGES") == "T"))
        {
            if (requireReasonForChange)
            {
                mInput.ShowMsg("Case update reason", "Please enter the reason for your changes", 0, txtConfirmUpdate.ClientID, btnConfirmUpdate.ClientID, "Save", "Cancel");
                e.button_canceled = true;
                return;
            }
            else if (logEditsToNarative)
            {
                SaveConfirmUpdate(PrepareCaseNarrative());
            }
        }
    }

    if (dbpTransfer.HasChanges(PLCSession.GetLabCtrl("LOG_BLANK_TO_DATA_CHANGES") == "T"))
        dbpTransfer.ReasonChangeKey = ItemTransfer.GenerateReasonChange("CUSTODY TAB SAVE", reasonText);
}