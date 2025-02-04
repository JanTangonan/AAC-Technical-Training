protected bool ValidateNames(object[] names, List<NameRec> NameList, QCEditState editstate, string prelogSequence)
        {
            DateTime defaultDate = Convert.ToDateTime("01-01-1900");
            string duplicateNameList = "";

            int GridID = 0;
            foreach (object name in names)
            {
                Dictionary<string, object> dictName = (Dictionary<string, object>)name;
               
                // {"NAMEKEY":"", "DEPTNAMEKEY":"" "ARRESTNUMBER":"", "NAMETYPE":"", "LAST":"", "FIRST":"", "MIDDLE":"", "SEX":"", "RACE":"", "DOB":"", "AGE":"", "ARRESTDATE":"", "STATEID":""}
                int NameKey = Convert.ToInt32((dictName["NAMEKEY"].ToString() == "") ? "0" : dictName["NAMEKEY"].ToString());
                int DeptNameKey = Convert.ToInt32((dictName["DEPTNAMEKEY"].ToString() == "") ? "0" : dictName["DEPTNAMEKEY"].ToString());
                string ArrestNumber = dictName["ARRESTNUMBER"].ToString().Trim();
                string NameType = dictName["NAMETYPE"].ToString().Trim().ToUpper();
                string LastName = dictName["LAST"].ToString().Trim();
                string FirstName = dictName["FIRST"].ToString().Trim();
                string MiddleName = dictName["MIDDLE"].ToString().Trim();
                string Sex = dictName["SEX"].ToString().Trim().ToUpper();
                string Race = dictName["RACE"].ToString().Trim().ToUpper();
                string sDOB = dictName["DOB"].ToString().Trim();
                string STATE_ID = dictName["STATEID"].ToString().Trim();
                string sArrestDate = dictName["ARRESTDATE"].ToString().Trim();
                string sAge = dictName["AGE"].ToString().Trim();
                string Juvenile = dictName["JUVENILE"].ToString().Trim();
                string ReferenceType = dictName["REFERENCE_TYPE"].ToString().Trim();
                string ReferenceText = dictName["REFERENCE_TEXT"].ToString().Trim();
                string OldRefType = dictName["OLD_REFERENCETYPE"].ToString().Trim();
                string OldRefText = dictName["OLD_REFERENCETEXT"].ToString().Trim();
                string OldNameType = dictName["OLD_NAMETYPE"].ToString().Trim();
                string OldLast = dictName["OLD_LAST"].ToString().Trim();
                string OldFirst = dictName["OLD_FIRST"].ToString().Trim();
                string OldMiddle = dictName["OLD_MIDDLE"].ToString().Trim();
                string OldSex = dictName["OLD_SEX"].ToString().Trim();
                string OldRace = dictName["OLD_RACE"].ToString().Trim();
                string NameID = string.Empty;
                string HomePhone = string.Empty;
                string CellPhone = string.Empty;
                string WorkPhone = string.Empty;
                string BusinessName = string.Empty;
                string Address = string.Empty;
                string PIDNumber = string.Empty;
                string FBINumber = string.Empty;
                string VersadexNameKey = string.Empty;
                string Suffix = "";
                string DCINumber = "";
                string comments = dictName["COMMENTS"].ToString().Trim();
                string subjectCharged = dictName["SUBJECT_CHARGED"].ToString().Trim();
                string trialDateText = dictName["TRIAL_DATE"].ToString().Trim();
                string grandJuryDateText = dictName["GRAND_JURY_DATE"].ToString().Trim();
                string status = dictName["STATUS"].ToString().Trim();
                string Alive = "";
                string Bleeding = "";
                string Transfused = "";

                if (dictName.ContainsKey("ADDRESS"))
                    Address = dictName["ADDRESS"].ToString().Trim();           
                if (dictName.ContainsKey("NAMEID"))
                    NameID = dictName["NAMEID"].ToString().Trim();
                if (dictName.ContainsKey("HOMEPHONE"))
                    HomePhone = dictName["HOMEPHONE"].ToString().Trim();
                if (dictName.ContainsKey("CELLPHONE"))
                    CellPhone = dictName["CELLPHONE"].ToString().Trim();
                if(dictName.ContainsKey("WORKPHONE"))
                    WorkPhone = dictName["WORKPHONE"].ToString().Trim();
                if (dictName.ContainsKey("BUSINESSNAME"))
                    BusinessName = dictName["BUSINESSNAME"].ToString().Trim();
                if(dictName.ContainsKey("PIDNUMBER"))
                    PIDNumber = dictName["PIDNUMBER"].ToString().Trim();
                if (dictName.ContainsKey("FBINUMBER"))
                    FBINumber = dictName["FBINUMBER"].ToString().Trim();
                if (dictName.ContainsKey("VERSADEXNAMEKEY"))
                    VersadexNameKey = dictName["VERSADEXNAMEKEY"].ToString().Trim();
                if (dictName.ContainsKey("SUFFIX"))
                    Suffix = dictName["SUFFIX"].ToString().Trim();
                if (dictName.ContainsKey("DCINUMBER"))
                    DCINumber = dictName["DCINUMBER"].ToString().Trim();

                if(dictName.ContainsKey("ALIVE"))
                    Alive = dictName["ALIVE"].ToString().Trim();
                if (dictName.ContainsKey("BLEEDING"))
                    Bleeding = dictName["BLEEDING"].ToString().Trim();
                if (dictName.ContainsKey("TRANSFUSED"))
                    Transfused = dictName["TRANSFUSED"].ToString().Trim();

                string nameRowContent = ArrestNumber + FirstName + MiddleName + Sex + Race + sDOB + STATE_ID + sArrestDate + sAge + Juvenile + ReferenceType + ReferenceText + NameID + HomePhone + CellPhone + WorkPhone + BusinessName + Suffix + FBINumber + DCINumber + comments + subjectCharged + trialDateText + grandJuryDateText + status + Alive + Bleeding + Transfused;

                string dateformat = PLCSession.GetDateFormat().ToUpper();
                if (string.IsNullOrEmpty(dateformat)) dateformat = "MM/DD/YYYY";

                bool duplicateNameExists = false;

                DateTime DOB = defaultDate;
                if (sDOB != "")
                {
                    try
                    {
                        if (dateformat.StartsWith("DD"))
                            DOB = DateTime.Parse(sDOB, CultureInfo.CreateSpecificCulture(PLCSession.GetCultureName()), DateTimeStyles.None);
                        else
                            DOB = DateTime.Parse(sDOB);
                    }
                    catch
                    {
                        MsgError("Invalid Date", "Date of birth is invalid", 0);
                        return false;
                    }
                }
               
                DateTime ArrestDate = defaultDate;
                if (sArrestDate != "")
                {
                    try
                    {
                        //ArrestDate = Convert.ToDateTime(sArrestDate);
                        if (dateformat.StartsWith("DD"))
                            ArrestDate = DateTime.Parse(sArrestDate, CultureInfo.CreateSpecificCulture("en-GB"));
                        else
                            ArrestDate = DateTime.Parse(sArrestDate);
                    }
                    catch
                    {
                        MsgError("Invalid Date", "Arrest date is invalid", 0);
                        return false;
                    }
                }

                DateTime? trialDate = null;
                if (trialDateText != "")
                    trialDate = PLCSession.ConvertToDateTime(trialDateText);

                DateTime? grandDuryDate = null;
                if (grandJuryDateText != "")
                    grandDuryDate = PLCSession.ConvertToDateTime(grandJuryDateText);

                int age = 0;
                if (sAge != "")
                    age = Convert.ToInt32(sAge);

                if (NameType.Trim() != "" || LastName.Trim() != "")
                {
                    if ((NameType.Trim() == "") || (!PLCSession.CodeValid("NAMETYPE", NameType)))
                    {
                        MsgError("Invalid Name Type Code", "Please select or enter a valid Name Type code", 0);
                        return false;
                    }

                    if (LastName.Trim() == "")
                    {
                        //MultiView1.ActiveViewIndex = 0;
                        MsgError("Invalid Value", "Last Name can't be blank", 0);
                        return false;
                    }

                    if (FirstName.Trim() == "")
                    {
                        if (PLCSession.GetLabCtrl("QC_FIRST_NAME_REQUIRED") == "T")
                        {
                            MsgError("Invalid Value", "First Name is required", 0);
                            return false;
                        }
                    }

                    if (Sex.Trim() == "")
                    {
                        if (PLCSession.GetLabCtrl("QC_SEX_TYPE_REQUIRED") == "T")
                        {
                            MsgError("Invalid Value", "Sex Type is required", 0);
                            return false;
                        }
                    }

                    if (sDOB.Trim() == "")
                    {
                        if (PLCSession.GetLabCtrl("QC_DATE_OF_BIRTH_REQUIRED") == "T")
                        {
                            MsgError("Invalid Value", "Date of birth is required", 0);
                            return false;
                        }
                    }


                    if (!PLCSession.CodeValid("SEXCODE", Sex))
                    {
                        //MultiView1.ActiveViewIndex = 0;
                        MsgError("Invalid Sex Code", "Please select or enter a valid gender code", 0);
                        return false;
                    }

                    if (!PLCSession.CodeValid("RACECODE", Race))
                    {
                        //MultiView1.ActiveViewIndex = 0;
                        MsgError("Invalid Race Code", "Please select or enter a valid race code", 0);
                        return false;
                    }


                    if (PLCSession.GetLabCtrl("USES_QC_NAME_REFERENCE") == "T")
                    {
                        if (!string.IsNullOrEmpty(ReferenceType) && string.IsNullOrEmpty(ReferenceText))
                        {
                            MsgError("Reference", "Please enter Reference.", 0);
                            return false;
                        }
                        else if (string.IsNullOrEmpty(ReferenceType) && !string.IsNullOrEmpty(ReferenceText))
                        {
                            MsgError("Reference", "Please select Reference Type.", 0);
                            return false;
                        }
                        else if ((editstate == QCEditState.New)
                            || (editstate == QCEditState.Edit && ReferenceHasChanges(OldRefType, OldRefText, ReferenceType, ReferenceText)))
                        {
                            if (CheckDuplicateNameReference(NameKey.ToString(), ReferenceType, ReferenceText))
                            {
                                MsgError("Reference", "Reference Type and Reference already exists for " + FirstName + " " + LastName + ".", 0);
                                return false;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(PLCSession.PLCGlobalCaseKey) && !string.IsNullOrEmpty(prelogSequence))
                    {
                        if (LastName != "" && FirstName != "")
                        {
                            PLCQuery qryLabName = new PLCQuery();

                            qryLabName.SQL = "SELECT N.NAME_KEY , N.FIRST_NAME, N.LAST_NAME, C.DEPARTMENT_CASE_NUMBER FROM TV_LABNAME N JOIN TV_LABCASE C ON C.CASE_KEY = N.CASE_KEY " +
                                            "WHERE N.CASE_KEY = ? AND LOWER(FIRST_NAME) = ? AND LOWER(LAST_NAME) = ?";
                            qryLabName.AddSQLParameter("CASE_KEY", PLCSession.PLCGlobalCaseKey);
                            qryLabName.AddSQLParameter("FIRST_NAME", FirstName.ToLower());
                            qryLabName.AddSQLParameter("LAST_NAME", LastName.ToLower());
                            qryLabName.Open();
                            if (!qryLabName.IsEmpty())
                            {
                                this.nameKeyList += qryLabName.FieldByName("NAME_KEY") + ",";
                                duplicateNameExists = true;
                            }
                        }
                    }

                    //validation will be applicable for LIMS and Prelog submissions
                    if (editstate == QCEditState.Edit)
                    {
                        if (!string.IsNullOrEmpty(LastName) && !string.IsNullOrEmpty(FirstName))
                        {
                            if (NameAlreadyExistsInsideCase(PLCSession.PLCGlobalCaseKey, FirstName, LastName))
                            {
                                duplicateNameList += "<br/>" + FirstName + " " + LastName;
                            }
                        }

                    }


                    if (!duplicateNameExists)
                    {
                        NameRec MyName = new NameRec(GridID, ArrestNumber, NameType, LastName, FirstName, MiddleName, Sex, Race, DOB, age, NameKey, ArrestDate, STATE_ID, Juvenile, ReferenceType, ReferenceText, OldRefType, OldRefText, OldNameType, OldLast, OldFirst, OldMiddle, OldSex, OldRace, Address, NameID, HomePhone, CellPhone, WorkPhone, BusinessName, PIDNumber, FBINumber, VersadexNameKey, Suffix, DCINumber, comments, subjectCharged, trialDate, grandDuryDate, status, Alive, Bleeding, Transfused);

                        MyName.DeptNameKey = DeptNameKey;
                        NameList.Add(MyName);
                    }
                }
                else if (NameType.Trim() == "" && LastName.Trim() == "")
                {
                    if(!nameRowContent.Equals("") && !nameRowContent.Equals(null))
                    {
                        MsgError("Invalid Values", "Name Type code and Last Name can't be blank", 0);
                        return false;
                    }
                }

                GridID++;
            }

            // If no names entered for BulkIntake case, add a default name.
            if ((editstate == QCEditState.BulkIntake) && (NameList.Count == 0))
            {
                string defaultNameType = "D";
                string defaultLastName = "Investigation";
                if (PLCSession.CodeValid("NAMETYPE", defaultNameType))  // Make sure that the default name type exists in NameType table.
                {

                    NameRec defaultNameRec = new NameRec(0, "", defaultNameType, defaultLastName, "", "", "", "", defaultDate, 0, 0, defaultDate, "", "", "", "", "", "", "","","","","","", "", "", "", "","","");

                    NameList.Add(defaultNameRec);
                }
            }

            if (!string.IsNullOrEmpty(duplicateNameList))
            {
                MsgError("Names", "The following name(s) already exist. Please select the name(s) in Existing Names grid <br/>" + duplicateNameList, 1);
                return false;
            }

            return true;
        }