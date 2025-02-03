#region Names
            PLCSession.WriteDebug("CREATE - Start Add Names", true);
            for (int i = 0; i < NameList.Count(); i++)
            {
                NameRec MyName = (NameRec)NameList[i];

                int nameAuditCode = -1;
                int nameAuditSubCode = -1;
                int newseq = 0;

                PLCQuery qryLabNameAppend = new PLCQuery();

                if (MyName.NameKey == 0) //create new case name
                {
                    //get next name key
                    newseq = PLCSession.GetNextSequence("LABNAME_SEQ");
                    MyName.NameKey = newseq;

                    this.nameKeyList += newseq + ",";

                    //get next casename number
                    nameNumber = GetNextNameNumber(PLCSession.PLCGlobalCaseKey, nameNumber);

                    nameAuditCode = 6;
                    nameAuditSubCode = 10;

                    qryLabNameAppend.SQL = "Select * FROM TV_LABNAME where 0 = 1";
                    qryLabNameAppend.Open();
                    qryLabNameAppend.Append();
                    qryLabNameAppend.SetFieldValue("CASE_KEY", PLCSession.PLCGlobalCaseKey);
                    qryLabNameAppend.SetFieldValue("NAME_KEY", newseq);
                    qryLabNameAppend.SetFieldValue("NUMBER_RES", nameNumber);
                    qryLabNameAppend.SetFieldValue("ENTRY_ANALYST", PLCSession.PLCGlobalAnalyst);
                    qryLabNameAppend.SetFieldValue("ENTRY_TIME_STAMP", DateTime.Now);
                }
                else //edit existing case name
                {
                    nameAuditCode = 6;
                    nameAuditSubCode = 1;

                    this.nameKeyList += MyName.NameKey + ",";

                    qryLabNameAppend.SQL = "Select * FROM TV_LABNAME where NAME_KEY = " + MyName.NameKey;
                    qryLabNameAppend.Open();
                    qryLabNameAppend.Edit();
                }

                qryLabNameAppend.SetFieldValue("DOC_NUMBER", MyName.ArrestNumber);
                qryLabNameAppend.SetFieldValue("LAST_NAME", MyName.LastName);
                qryLabNameAppend.SetFieldValue("LAST_NAME_SEARCH", MyName.LastName.ToUpper());
                qryLabNameAppend.SetFieldValue("FIRST_NAME", MyName.FirstName);
                qryLabNameAppend.SetFieldValue("FIRST_NAME_SEARCH", MyName.FirstName.ToUpper());
                qryLabNameAppend.SetFieldValue("NAME_TYPE", MyName.NameType);
                qryLabNameAppend.SetFieldValue("MIDDLE_NAME", MyName.MiddleName);
                qryLabNameAppend.SetFieldValue("MIDDLE_NAME_SEARCH", MyName.MiddleName.ToUpper());
                qryLabNameAppend.SetFieldValue("SEX", MyName.Sex);
                qryLabNameAppend.SetFieldValue("RACE", MyName.Race);
                if (MyName.DOB.Year > 1900)
                    qryLabNameAppend.SetFieldValue("DATE_OF_BIRTH", MyName.DOB);
                if (MyName.Age > 0)
                    qryLabNameAppend.SetFieldValue("AGE", MyName.Age);
                if (MyName.ArrestDate.Year > 1900)
                {
                    qryLabNameAppend.SetFieldValue("ARREST_DATE", MyName.ArrestDate);
                    if (MyName.ArrestDate < dtEarliestArrestDate)
                    {
                        dtEarliestArrestDate = MyName.ArrestDate;
                    }
                }

                if(PLCSession.GetLabCtrlFlag("QC_STATE_ID_LABEL") != "")
                    qryLabNameAppend.SetFieldValue("STATE_ID", MyName.STATE_ID);

                qryLabNameAppend.SetFieldValue("JUVENILE", MyName.Juvenile);

                if (caseIsFromCrimePad)
                {                  
                    string prelogNameNumber = GetNameNumber(MyName.OldNameType, MyName.OldLast, MyName.OldFirst, MyName.OldMiddle, MyName.OldSex, MyName.OldRace, prelogSequence);
                    CollectCrimePadNameInformation(ref qryLabNameAppend, sDepartmentCaseNumber, sDeptCode, prelogSequence, prelogNameNumber);
                }

                if (MyName.DeptNameKey > 0)
                    qryLabNameAppend.SetFieldValue("DEPT_NAME_KEY", MyName.DeptNameKey);

                qryLabNameAppend.SetFieldValue("NAME_ID", MyName.NameID);
                qryLabNameAppend.SetFieldValue("HOME_PHONE", MyName.HomePhone);
                qryLabNameAppend.SetFieldValue("CELL_PHONE", MyName.CellPhone);

                // save itemloc reference
                if (PLCSession.GetLabCtrl("QC_NAMES_ADDRESS_BOOK").ToUpper().Equals("T"))
                {
                    if (editstate == QCEditState.New)
                    {
                        if (dictItemLocKeys.Keys.Contains(MyName.Address))
                            qryLabNameAppend.SetFieldValue("NAME_ADDRESS_KEY", dictItemLocKeys[MyName.Address]);
                    }
                    else if (editstate == QCEditState.Edit)
                    {
                        qryLabNameAppend.SetFieldValue("NAME_ADDRESS_KEY", MyName.Address);
                    }
                }

                qryLabNameAppend.SetFieldValue("WORK_PHONE", MyName.WorkPhone);
                qryLabNameAppend.SetFieldValue("BUSINESS_NAME", MyName.BusinessName);
                qryLabNameAppend.SetFieldValue("PID_NUMBER", MyName.PIDNumber);
                qryLabNameAppend.SetFieldValue("VERSADEX_NAME_KEY", MyName.VersadexNameKey);

                if(PLCSession.GetLabCtrlFlag("USES_NAME_SUFFIX") == "T")
                    qryLabNameAppend.SetFieldValue("SUFFIX", MyName.Suffix);

                if (PLCSession.GetLabCtrlFlag("USES_FBI_NUMBER") == "T")
                    qryLabNameAppend.SetFieldValue("FBI_NUMBER", MyName.FBINumber);

                if (PLCSession.GetLabCtrlFlag("USES_DCI_NUMBER") == "T")
                    qryLabNameAppend.SetFieldValue("DCI_NUMBER", MyName.DCINumber);

                if(PLCSession.GetLabCtrlFlag("USES_QC_COMMENTS").Equals("T"))
                    qryLabNameAppend.SetFieldValue("COMMENTS", MyName.Comments);

                if (PLCSession.GetLabCtrlFlag("USES_QC_STATUS").Equals("T"))
                    qryLabNameAppend.SetFieldValue("STATUS", MyName.Status);

                if (PLCSession.GetLabCtrlFlag("USES_QC_SUBJECT_CHARGED").Equals("T"))
                {
                    qryLabNameAppend.SetFieldValue("SUBJECT_CHARGED", string.IsNullOrEmpty(MyName.SubjectCharged)
                        ? "F"
                        : MyName.SubjectCharged);
                }

                if (PLCSession.GetLabCtrlFlag("USES_QC_TRIAL_DATE").Equals("T"))
                    qryLabNameAppend.SetFieldValue("TRIAL_DATE", MyName.TrialDate);

                if (PLCSession.GetLabCtrlFlag("USES_QC_GRAND_JURY_DATE").Equals("T"))
                    qryLabNameAppend.SetFieldValue("GRAND_JURY_DATE", MyName.GrandJuryDate);

                if (PLCSession.GetLabCtrlFlag("USES_MEDSTATUS_OPTIONS") == "T")
                {
                    qryLabNameAppend.SetFieldValue("ALIVE", MyName.Alive);
                    qryLabNameAppend.SetFieldValue("BLEEDING", MyName.Bleeding);
                    qryLabNameAppend.SetFieldValue("TRANSFUSED", MyName.Transfused);
                }

                //save case name record
                qryLabNameAppend.Post("TV_LABNAME", nameAuditCode, nameAuditSubCode);

                //Update prelog TV_SUBMNAME.NAME_KEY
                if (nameAuditSubCode == 10 && !string.IsNullOrEmpty(prelogSequence))
                {
                    string prelogNameNumber = GetNameNumber(MyName.OldNameType, MyName.OldLast, MyName.OldFirst, MyName.OldMiddle, MyName.OldSex, MyName.OldRace, prelogSequence);

                    if (!string.IsNullOrEmpty(prelogNameNumber))
                    {
                        UpdatePrelogNameData(prelogDeptCode, sDepartmentCaseNumber, prelogNameNumber, MyName.NameKey, prelogSequence);
                      
                        //This will create SUBMRESUBNAME record once the prelog case has been successfully created
                        if (dictRedirectNameDetails.Count > 0)
                        {                          
                            string departmentCaseNumber = GetDeptCaseUsingSequence(prelogSequence); //this is to get the correct form of the department case number
                            UpdateRedirectPrelogRelatedEntries(departmentCaseNumber, prelogNameNumber, MyName, dictRedirectNameDetails);

                        }
                    }

                }
                else
                {
                    if (dictPrelogLIMSData.Count > 0 && dictPrelogLIMSData["isPrelogLIMSSearch"].ToString() == "T")
                    {
                        string prelogNameNumber = GetNameNumber(MyName.OldNameType, MyName.OldLast, MyName.OldFirst, MyName.OldMiddle, MyName.OldSex, MyName.OldRace, dictPrelogLIMSData["prelogSequence"].ToString());
                        UpdatePrelogNameData(dictPrelogLIMSData["departmentCode"].ToString(), dictPrelogLIMSData["departmentCaseNumber"].ToString(), prelogNameNumber, MyName.NameKey, dictPrelogLIMSData["prelogSequence"].ToString());
                    }
                }

                // Save case name reference
                if ((!string.IsNullOrEmpty(MyName.ReferenceType) && !string.IsNullOrEmpty(MyName.ReferenceText))
                    && PLCSession.GetLabCtrl("USES_QC_NAME_REFERENCE") == "T")
                {
                    SaveNameRef(editstate, MyName.NameKey.ToString(), MyName.OldRefType, MyName.OldRefText, MyName.ReferenceType, MyName.ReferenceText);
                }
               
                // SUBNLINK record
                PLCQuery qryNameSubLink = new PLCQuery("SELECT * FROM TV_SUBNLINK WHERE NAME_KEY = " + MyName.NameKey + " AND SUBMISSION_KEY = " + PLCSession.PLCGlobalSubmissionKey);
                qryNameSubLink.Open();
                if (qryNameSubLink.IsEmpty())
                {
                    qryNameSubLink.Append();
                    qryNameSubLink.SetFieldValue("NAME_KEY", MyName.NameKey);
                }
                else
                {
                    qryNameSubLink.Edit();
                }

                qryNameSubLink.SetFieldValue("RESUBMISSION", (nameAuditSubCode == 1 ? "T" : null));
                qryNameSubLink.SetFieldValue("SUBMISSION_KEY", PLCSession.PLCGlobalSubmissionKey);
                qryNameSubLink.Post("TV_SUBNLINK");
            }


            if (dictPrelogLIMSData.Count > 0 && dictPrelogLIMSData["isPrelogLIMSSearch"].ToString() == "T")
            {
                UpdatePrelogLIMSNameData(dictPrelogLIMSData["departmentCode"].ToString(), dictPrelogLIMSData["departmentCaseNumber"].ToString(), dictPrelogLIMSData["submissionNumber"].ToString(), dictPrelogLIMSData["prelogSequence"].ToString(), PLCSession.PLCGlobalCaseKey);
            }

            PLCSession.WriteDebug("CREATE - End Add Names", true);
            #endregion Names