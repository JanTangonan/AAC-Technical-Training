public bool ValidateSubmit(QCEditState editstate, Dictionary<string, object> dictCaseSubmissionItem, object[] names, object[] items, object[] existingitems, object[] existingnames, object[] containers, object[] references, object[] distributions, List<NameRec> NameList, List<ItemRec> ItemList, List<ReferenceRec> ReferenceList, List<ContainerRec> ContainerList, List<DistributionRec> DistributionList, List<string> offenseList, string prelogSequence, object[] chainofcustody, List<COCRec> COCRecList, List<string> ChildItemList)
        {
            if (!ValidateDBPanel(dictCaseSubmissionItem, editstate))
            {
                // Error packet was already set in ValidateDBPanel() call.
                PLCSession.WriteDebug("ValidateDBPanel: false");
                return false;
            }

            //validate names
            if (!ValidateNames(names, NameList, editstate, prelogSequence))
            {
                PLCSession.WriteDebug("ValidateNames: false");
                return false;
            }

            //validate items
            string seviceTypeCode = GetDBPanelValue(dictCaseSubmissionItem, "TV_LABSUB.LAB_SERVICE_TYPE");
            string departmentCode = GetDBPanelValue(dictCaseSubmissionItem, "TV_LABSUB.DEPARTMENT_CODE");
            bool isBiologyRequired = IsBiologyRequired(GetDBPanelValue(dictCaseSubmissionItem, "TV_LABSUB.DEPARTMENT_CASE_NUMBER"), GetDBPanelValue(dictCaseSubmissionItem, "TV_LABSUB.OFFENSE_CODE"), 
                GetDBPanelValue(dictCaseSubmissionItem, "TV_LABSUB.OFFENSE_CODE_2"), GetDBPanelValue(dictCaseSubmissionItem, "TV_LABSUB.OFFENSE_CODE_3"), offenseList);

            if (!ValidateItems(items, ItemList, editstate, seviceTypeCode, departmentCode, isBiologyRequired))
            {
                PLCSession.WriteDebug("ValidateItems: false");
                return false;
            }

            //validate references
            if (!ValidateReferences(references, ReferenceList, editstate))
            {
                PLCSession.WriteDebug("ValidateRefernces: false");
                return false;
            }

            //validate existing items for additional submission 
            if (editstate != QCEditState.New)
            {
                if (!ValidateExistingItems(existingitems, ItemList, seviceTypeCode, ChildItemList))
                {
                    PLCSession.WriteDebug("ValidateExistingItems: false");
                    return false;
                }
            }

            //validate existing names for additional submission 
            if (editstate != QCEditState.New && (PLCSession.GetLabCtrl("HIDE_QC_EXISTING_NAMES") != "T"))
            {
                if (!ValidateExistingNames(existingnames, NameList))
                {
                    return false;
                }
            }

            // Make sure containers edits are valid.
            if ((editstate == QCEditState.BulkIntake) && !IsValidContainersGrid(containers))            
                 return false;
            //else if ((editstate == QCEditState.New || editstate == QCEditState.Edit) && !AreNewContainersValid(containers, ContainerList, editstate)
            //    && (PLCSession.GetLabCtrl("USES_CONTAINER_SPLITTER") == "T"))
            //    return false;

            //validate distributions
            if (PLCSession.GetLabCtrl("USES_DISTRIBUTION").Trim().ToUpper() == "T" && editstate != QCEditState.BulkIntake)
            {
                if (!ValidateDistributions(distributions, DistributionList, editstate))
                {
                    return false;
                }
            }

            //validate chain of custody tab
            if (PLCSession.GetLabCtrlFlag("QC_USES_CHAIN_OF_CUSTODY") == "T" && editstate != QCEditState.BulkIntake)
            {
                if (!ValidateCOC(chainofcustody, COCRecList, editstate))
                {
                    return false;
                }
            }

            PLCSession.WriteDebug("CREATE - Check for errors ok.", true);
            return true;
        }