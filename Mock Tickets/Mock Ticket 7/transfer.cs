public int TransferItem(string sECN, string sCustodyOf, string sLocation, string department, string userFieldName, string user, string sTransferDate, string sTransferTime,
            string sComments, string sTrackingNumber, int batchSeq, bool UpdateSubItemBarcode, string ItemType, string ContainerKey, string sCustLocProcess,
            string authorizedBy, string overrideComments, string itemParentECN, string itemBarcode, string weight, string weightUnits)
        {
            string sSequence = PLCSessionVars1.GetNextSequence("LABSTAT_SEQ").ToString();
            DateTime dtCustodyDate = AddDateAndTime(sTransferDate, sTransferTime);

            //Current Department should always be filled in.
            if (string.IsNullOrEmpty(department))
            {
                PLCQuery qryDept = new PLCQuery(string.Format("SELECT DEPARTMENT_CODE FROM TV_CUSTCODE WHERE CUSTODY_TYPE = '{0}'", sCustodyOf));
                qryDept.Open();
                if (qryDept.HasData()) department = qryDept.FieldByName("DEPARTMENT_CODE");
            }


            // if TV_LABCTRL3.USES_MULTI_POINT_TRANSFER is set to 'T' and if current custody is not in Analyst Custody Codes (Physical location) AND 
            // current custody is not equal to global analyst custody OR current location is not equal to global analyst location AND
            // new custody is not equal to global analyst custody OR new location is not equal to global analyst location
            // will add custody record for the global analyst/entry analyst
            bool UsesMultiPointTransfer = PLCSession.GetLabCtrl("USES_MULTI_POINT_TRANSFER").ToUpper() == "T";
            string AnalystCustodyCodes = PLCSession.GetLabCtrl("ANALYST_CUSTODY_CODES").ToUpper();
            if (UsesMultiPointTransfer)// if set to 'T' and if current custody is not in Analyst Custody Codes (Physical location) will add custody record for the global analyst/entry analyst
            {
                PLCQuery qryCurrentCustody = new PLCQuery("SELECT CUSTODY_OF, LOCATION FROM TV_LABITEM WHERE EVIDENCE_CONTROL_NUMBER = '" + sECN + "'");
                qryCurrentCustody.OpenReadOnly();
                string currentCustody = qryCurrentCustody.FieldByName("CUSTODY_OF");
                string currentLocation = qryCurrentCustody.FieldByName("LOCATION");
                string globalAnalystCustody = PLCSessionVars1.PLCGlobalDefaultAnalystCustodyOf;
                string globalAnalystLocation = PLCSessionVars1.PLCGlobalAnalyst;
                if (!CustodyMemberOfAnalystCustodyCodes(currentCustody.ToUpper().Trim()) && (currentCustody != globalAnalystCustody || currentLocation != globalAnalystLocation) && (sCustodyOf != globalAnalystCustody || sLocation != globalAnalystLocation))
                {
                    //Adding a new entry in TV_LABSTAT
                    PLCQuery qryEntryAnalystLabStat = new PLCQuery();
                    qryEntryAnalystLabStat.SQL = "Select * FROM TV_LABSTAT where 0 = 1";
                    qryEntryAnalystLabStat.Open();
                    qryEntryAnalystLabStat.Append();
                    qryEntryAnalystLabStat.SetFieldValue("CASE_KEY", PLCSessionVars1.PLCGlobalCaseKey);
                    qryEntryAnalystLabStat.SetFieldValue("EVIDENCE_CONTROL_NUMBER", sECN);
                    qryEntryAnalystLabStat.SetFieldValue("STATUS_KEY", sSequence);
                    qryEntryAnalystLabStat.SetFieldValue("STATUS_DATE", PLCSession.ConvertToDateTime(sTransferDate));
                    qryEntryAnalystLabStat.SetFieldValue("STATUS_TIME", dtCustodyDate);
                    qryEntryAnalystLabStat.SetFieldValue("STATUS_CODE", globalAnalystCustody);
                    qryEntryAnalystLabStat.SetFieldValue("LOCKER", globalAnalystLocation);
                    qryEntryAnalystLabStat.SetFieldValue("COMMENTS", sComments);
                    qryEntryAnalystLabStat.SetFieldValue("TRACKING_NUMBER", sTrackingNumber);
                    qryEntryAnalystLabStat.SetFieldValue("ENTRY_TIME", System.DateTime.Now);
                    qryEntryAnalystLabStat.SetFieldValue("ENTERED_BY", globalAnalystLocation);
                    qryEntryAnalystLabStat.SetFieldValue("ENTRY_ANALYST", globalAnalystLocation);
                    qryEntryAnalystLabStat.SetFieldValue("ENTRY_TIME_STAMP", System.DateTime.Now);
                    qryEntryAnalystLabStat.SetFieldValue("SOURCE", "M");
                    qryEntryAnalystLabStat.SetFieldValue("BATCH_SEQUENCE", batchSeq);

                    if (!string.IsNullOrEmpty(weight))
                    {
                        try
                        {
                            double dWeight = PLCSession.GetWeight(weight, ref weightUnits);
                            qryEntryAnalystLabStat.SetFieldValue("WEIGHT", dWeight);
                        }
                        catch
                        {
                        }
                        qryEntryAnalystLabStat.SetFieldValue("PACKAGE_WEIGHT", weight);
                    }

                    if (!string.IsNullOrEmpty(weightUnits))
                        qryEntryAnalystLabStat.SetFieldValue("WEIGHT_UNITS", weightUnits);

                    if (!UpdateSubItemBarcode && !string.IsNullOrWhiteSpace(itemParentECN) && string.IsNullOrWhiteSpace(itemBarcode))
                        qryEntryAnalystLabStat.SetFieldValue("PARENT_ECN", itemParentECN);

                    if (!string.IsNullOrEmpty(department))
                        qryEntryAnalystLabStat.SetFieldValue("DEPARTMENT_CODE", department);
                    if (!string.IsNullOrEmpty(user))
                        qryEntryAnalystLabStat.SetFieldValue(userFieldName, user);

                    if (!string.IsNullOrEmpty(authorizedBy))
                        qryEntryAnalystLabStat.SetFieldValue("AUTHORIZED_BY", authorizedBy);

                    if (!string.IsNullOrEmpty(overrideComments))
                    {
                        qryEntryAnalystLabStat.SetFieldValue("OVERRIDE_BY", PLCSession.PLCGlobalAnalyst);
                        qryEntryAnalystLabStat.SetFieldValue("OVERRIDE_COMMENTS", overrideComments);
                    }

                    if (ContainerKey.Length > 0)
                    {
                        qryEntryAnalystLabStat.SetFieldValue("CONTAINER_KEY", ContainerKey);
                    }

                    qryEntryAnalystLabStat.Post("TV_LABSTAT", 13, 10);
                    sSequence = PLCSessionVars1.GetNextSequence("LABSTAT_SEQ").ToString();
                }
            }

            //Adding a new entry in TV_LABSTAT
            PLCQuery qryParentLabStatAppend = new PLCQuery();
            qryParentLabStatAppend.SQL = "Select * FROM TV_LABSTAT where 0 = 1";
            qryParentLabStatAppend.Open();
            qryParentLabStatAppend.Append();
            qryParentLabStatAppend.SetFieldValue("CASE_KEY", PLCSessionVars1.PLCGlobalCaseKey);
            qryParentLabStatAppend.SetFieldValue("EVIDENCE_CONTROL_NUMBER", sECN);
            qryParentLabStatAppend.SetFieldValue("STATUS_KEY", sSequence);
            qryParentLabStatAppend.SetFieldValue("STATUS_DATE", PLCSession.ConvertToDateTime(sTransferDate));
            qryParentLabStatAppend.SetFieldValue("STATUS_TIME", dtCustodyDate);
            qryParentLabStatAppend.SetFieldValue("STATUS_CODE", sCustodyOf);
            qryParentLabStatAppend.SetFieldValue("LOCKER", sLocation);
            qryParentLabStatAppend.SetFieldValue("COMMENTS", sComments);
            qryParentLabStatAppend.SetFieldValue("TRACKING_NUMBER", sTrackingNumber);
            qryParentLabStatAppend.SetFieldValue("ENTRY_TIME", System.DateTime.Now);
            qryParentLabStatAppend.SetFieldValue("ENTERED_BY", PLCSessionVars1.PLCGlobalAnalyst);
            qryParentLabStatAppend.SetFieldValue("ENTRY_ANALYST", PLCSessionVars1.PLCGlobalAnalyst);
            qryParentLabStatAppend.SetFieldValue("ENTRY_TIME_STAMP", System.DateTime.Now);
            qryParentLabStatAppend.SetFieldValue("SOURCE", "M");
            qryParentLabStatAppend.SetFieldValue("BATCH_SEQUENCE", batchSeq);

            if (!string.IsNullOrEmpty(weight))
            {
                try
                {
                    double dWeight = PLCSession.GetWeight(weight, ref weightUnits);
                    qryParentLabStatAppend.SetFieldValue("WEIGHT", dWeight);
                }
                catch
                {
                }
                qryParentLabStatAppend.SetFieldValue("PACKAGE_WEIGHT", weight);
            }

            if (!string.IsNullOrEmpty(weightUnits))
                qryParentLabStatAppend.SetFieldValue("WEIGHT_UNITS", weightUnits);

            if (!UpdateSubItemBarcode && !string.IsNullOrWhiteSpace(itemParentECN) && string.IsNullOrWhiteSpace(itemBarcode))
                qryParentLabStatAppend.SetFieldValue("PARENT_ECN", itemParentECN);

            if (!string.IsNullOrEmpty(department))
                qryParentLabStatAppend.SetFieldValue("DEPARTMENT_CODE", department);

            if (!string.IsNullOrEmpty(user))
                qryParentLabStatAppend.SetFieldValue(userFieldName, user);

            if (!string.IsNullOrEmpty(authorizedBy))
                qryParentLabStatAppend.SetFieldValue("AUTHORIZED_BY", authorizedBy);

            if (!string.IsNullOrEmpty(overrideComments))
            {
                qryParentLabStatAppend.SetFieldValue("OVERRIDE_BY", PLCSession.PLCGlobalAnalyst);
                qryParentLabStatAppend.SetFieldValue("OVERRIDE_COMMENTS", overrideComments);
            }

            if (ContainerKey.Length > 0)
            {
                qryParentLabStatAppend.SetFieldValue("CONTAINER_KEY", ContainerKey);
            }

            qryParentLabStatAppend.Post("TV_LABSTAT", 13, 10);

            PLCQuery qryLatestLabStat = new PLCQuery();
            qryLatestLabStat.SQL = "SELECT L.STATUS_CODE, L.LOCKER, L.STATUS_TIME, L.STATUS_DATE FROM TV_LABSTAT L WHERE L.EVIDENCE_CONTROL_NUMBER=" + sECN +
                        " ORDER BY L.ENTRY_TIME DESC, L.STATUS_DATE DESC,L.STATUS_TIME DESC, L.STATUS_KEY DESC";
            qryLatestLabStat.Open();
            if (qryLatestLabStat.HasData())
            {
                //Updating an entry in TV_LABITEM     
                PLCQuery qryParentLabItemEdit = new PLCQuery();
                qryParentLabItemEdit.SQL = "SELECT * FROM TV_LABITEM WHERE EVIDENCE_CONTROL_NUMBER = " + sECN;
                qryParentLabItemEdit.Open();
                if (!qryParentLabItemEdit.IsEmpty())
                {
                    qryParentLabItemEdit.Edit();
                    qryParentLabItemEdit.SetFieldValue("CUSTODY_OF", qryLatestLabStat.FieldByName("STATUS_CODE"));
                    qryParentLabItemEdit.SetFieldValue("LOCATION", qryLatestLabStat.FieldByName("LOCKER"));
                    qryParentLabItemEdit.SetFieldValue("CUSTODY_DATE", qryLatestLabStat.FieldByName("STATUS_DATE"));
                    if (sCustLocProcess != "")
                    {
                        string prevStatus = qryParentLabItemEdit.FieldByName("PROCESS");
                        string newStatus = "";
                        if (sCustLocProcess.ToUpper() == "*NULL")
                            qryParentLabItemEdit.SetFieldValue("PROCESS", "");
                        else
                        {
                            qryParentLabItemEdit.SetFieldValue("PROCESS", sCustLocProcess);
                            newStatus = sCustLocProcess;
                        }
                        //Recording of the status change of items
                        AddRevReqHistory(sECN, prevStatus, newStatus, "Manual Transfer", PLCSessionVars1.PLCGlobalAnalyst, Convert.ToInt32(sSequence));
                    }

                    //
                    if (UpdateSubItemBarcode)
                        qryParentLabItemEdit.SetFieldValue("BARCODE", sECN);
                    //
                    qryParentLabItemEdit.Post("TV_LABITEM", 7, 1);
                    
                    PLCDBGlobal.instance.UpdateDeliveryRequestStatus(batchSeq.ToString());

                    var itemUpdated = new PLCCONTROLS.Objects.Core.Item(sECN);
                    itemUpdated.LoadItemList();
                    itemUpdated.UpdateItemList();

                    //Ticket#42220 - Update RMS Interface for custody record change.
                    bool locationIsMarkedRMS = (PLCDBGlobal.instance.GetCustodyLocationDetails(sCustodyOf, sLocation, "RETURN_TO_RMS") == "T");
                    bool caseIsFromRMS = (PLCDBGlobal.instance.GetTableDetailsByCustomKey("TV_LABCASE", "CASE_FROM_RMS", "CASE_KEY", PLCSession.PLCGlobalCaseKey) == "T");

                    if (locationIsMarkedRMS && caseIsFromRMS)
                        PLCDBGlobal.instance.UpdateRMSEvidence(PLCSession.PLCGlobalDepartmentCaseNumber, qryParentLabItemEdit.FieldByName("THIRD_PARTY_BARCODE"), "RMS", sLocation);

                    if (PLCSession.GetLabCtrl("USE_CASE_ACCESS") == "T")
                        this.SetCaseAccessCode(PLCSession.PLCGlobalCaseKey, sCustodyOf, sLocation);

                    if (PLCSession.PLCDatabaseServer == "MSSQL")
                    {
                        //Update delivery_status in TV_LABASSIGN of all open assignments where this item is assigned 
                        UpdateAssignmentDeliveryStatus(sECN, sCustodyOf, sLocation);
                    }

                    // Update assignments with custody change.
                    this.SyncAssignmentsWithItemCustody(Convert.ToInt32(sECN));
                }
            }

            UpdateLabItemCurrentDepartment(sECN);

            if (ContainerKey.Length > 0)
            {
                ManualTransfer_UpdateContainerCustody(ContainerKey, sCustodyOf, sLocation, sComments, authorizedBy, weight, weightUnits);
            }

            return Convert.ToInt32(sSequence);
        }