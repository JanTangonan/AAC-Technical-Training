private void SaveNames(string caseData, string namesData)
        {
            
            Dictionary<string, object> caseJSON = (Dictionary<string, object>)JSONStructFromStr(caseData);
            Dictionary<string, object> namesJSON = (Dictionary<string, object>)JSONStructFromStr(namesData);

            string departmentCaseNumber = caseJSON["TV_SUBMTRAN.DEPARTMENT_CASE_NUMBER"].ToString();
            string departmentCode = caseJSON["TV_SUBMTRAN.DEPARTMENT_CODE"].ToString();
            string submissionNumber = caseJSON["TV_SUBMTRAN.SUBMISSION_NUMBER"].ToString();
            string caseKey = caseJSON["TV_SUBMTRAN.CASE_KEY"].ToString();
            int nameNumber = GetNameNumberBase(departmentCaseNumber, departmentCode, submissionNumber);
            int auditCode = 9003;
            int auditSubCode = 1; //default
            bool hasJuvenile = false;
            string dateformat = PLCSession.GetDeptCtrlDateFormat().ToUpper();
            if (string.IsNullOrEmpty(dateformat)) dateformat = "MM/DD/YYYY";

            ClearResubNameInfo(departmentCaseNumber, departmentCode, submissionNumber);

            foreach (string nameKey in namesJSON.Keys)
            {
                object[] nameJSONContents = (object[])namesJSON[nameKey];
                for (int i = 0; i < nameJSONContents.Length; i++)
                {
                    Dictionary<string, object> nameEntry = (Dictionary<string, object>)nameJSONContents[i];
                    string nametypedesc = GetTypeCodeOrDesc("TV_NAMETYPE", "NAME_TYPE", nameEntry["NAME_TYPE_CODE"].ToString(), "DESCRIPTION");
                    string sextypedesc = GetTypeCodeOrDesc("TV_SEXCODE", "SEX", nameEntry["SEX_CODE"].ToString(), "DESCRIPTION");
                    string racetypedesc = GetTypeCodeOrDesc("TV_RACECODE", "RACE", nameEntry["RACE_CODE"].ToString(), "DESCRIPTION");
                    bool hasNameKey = (!String.IsNullOrEmpty(nameEntry["NAME_KEY"].ToString()) ? true : false);
                    string nameSequence = nameEntry["NAME_SEQUENCE"].ToString();                 
                    string sDOB = nameEntry["DATE_OF_BIRTH"].ToString();
                    string trialDateText = nameEntry.ContainsKey("TRIAL_DATE") ? nameEntry["TRIAL_DATE"].ToString() : "";
                    string grandJuryDateText = nameEntry.ContainsKey("GRAND_JURY_DATE") ? nameEntry["GRAND_JURY_DATE"].ToString() : "";
                    bool hasSuffix = nameEntry.ContainsKey("SUFFIX");
                    DateTime? trialDate = null;
                    DateTime? grandJuryDate = null;

                    PLCQuery qryNames = new PLCQuery();

                    if (nametypedesc.ToLower().Contains("juvenile"))
                        hasJuvenile = true;

                    if (String.IsNullOrEmpty(nameEntry["SUBMISSION_NUMBER"].ToString()))
                    {
                        auditSubCode = 1;
                        int nameSeq = PLCSession.GetNextSequence("SUBMNAME_SEQ");

                        qryNames.SQL = "SELECT * FROM TV_SUBMNAME WHERE 0 = 1";
                        qryNames.Open();
                        qryNames.Append();
                        qryNames.SetFieldValue("DEPARTMENT_CODE", departmentCode);
                        qryNames.SetFieldValue("SUBMISSION_NUMBER", submissionNumber);
                        qryNames.SetFieldValue("DEPARTMENT_CASE_NUMBER", departmentCaseNumber);
                        qryNames.SetFieldValue("NAME_NUMBER", nameNumber);
                        qryNames.SetFieldValue("NAME_SEQUENCE", nameSeq);
                        nameNumber++;
                    }
                    else
                    {
                        auditSubCode = 2;
                        qryNames.SQL = @"SELECT * FROM TV_SUBMNAME WHERE DEPARTMENT_CODE = '" + departmentCode + @"' 
                        AND DEPARTMENT_CASE_NUMBER = '" + departmentCaseNumber + @"' 
                        AND NAME_NUMBER = '" + nameEntry["NAME_NUMBER"].ToString() + @"'
                        AND SUBMISSION_NUMBER = '" + nameEntry["SUBMISSION_NUMBER"].ToString() + "'";
                        qryNames.Open();
                        qryNames.Edit();
                    }
               


                    qryNames.SetFieldValue("NAME_TYPE_CODE", nameEntry["NAME_TYPE_CODE"].ToString());
                    qryNames.SetFieldValue("NAME_TYPE_DESCRIPTION", nametypedesc);
                    qryNames.SetFieldValue("LAST_NAME", nameEntry["LAST_NAME"].ToString().Trim());
                    qryNames.SetFieldValue("FIRST_NAME", nameEntry["FIRST_NAME"].ToString().Trim());
                    qryNames.SetFieldValue("MIDDLE_NAME", nameEntry["MIDDLE_NAME"].ToString().Trim());                
                    qryNames.SetFieldValue("SEX_CODE", nameEntry["SEX_CODE"]);
                    qryNames.SetFieldValue("SEX_DESCRIPTION", sextypedesc);
                    qryNames.SetFieldValue("RACE_CODE", nameEntry["RACE_CODE"]);
                    qryNames.SetFieldValue("RACE_DESCRIPTION", racetypedesc);

                    if (PLCSession.GetDeptCtrlFlag("USES_MEDSTATUS_OPTIONS") == "T")
                    {
                        qryNames.SetFieldValue("ALIVE", nameEntry.ContainsKey("ALIVE_CODE") ? nameEntry["ALIVE_CODE"] : "");
                        qryNames.SetFieldValue("BLEEDING", nameEntry.ContainsKey("BLEEDING_CODE") ? nameEntry["BLEEDING_CODE"] : "");
                        qryNames.SetFieldValue("TRANSFUSED", nameEntry.ContainsKey("TRANSFUSED_CODE") ? nameEntry["TRANSFUSED_CODE"] : "");
                    }

                    if (sDOB != "")
                    {
                        try
                        {
                            if (dateformat.StartsWith("DD"))
                                sDOB = DateTime.Parse(sDOB, CultureInfo.CreateSpecificCulture(PLCSession.GetPrelogCultureName()), DateTimeStyles.None).ToShortDateString();
                            else
                                sDOB = DateTime.Parse(sDOB).ToShortDateString();
                        }
                        catch(Exception e)
                        {

                        }                     
                    }

                    if (!string.IsNullOrEmpty(trialDateText))
                        trialDate = PLCSession.ConvertToDateTime(trialDateText);

                    if(!string.IsNullOrEmpty(grandJuryDateText))
                        grandJuryDate = PLCSession.ConvertToDateTime(grandJuryDateText);

                    qryNames.SetFieldValue("DATE_OF_BIRTH", sDOB);

                    if (hasNameKey)
                        qryNames.SetFieldValue("NAME_KEY", Convert.ToInt32(nameEntry["NAME_KEY"]));

                    if(hasSuffix)
                        qryNames.SetFieldValue("SUFFIX", nameEntry["SUFFIX"].ToString().Trim());

                    if(nameEntry.ContainsKey("STATE_ID"))
                        qryNames.SetFieldValue("STATE_ID", nameEntry["STATE_ID"].ToString().Trim());

                    if (nameEntry.ContainsKey("FBI_NUMBER"))
                        qryNames.SetFieldValue("FBI_NUMBER", nameEntry["FBI_NUMBER"].ToString().Trim());

                    if (nameEntry.ContainsKey("DCI_NUMBER"))
                        qryNames.SetFieldValue("DCI_NUMBER", nameEntry["DCI_NUMBER"].ToString().Trim());


                    if (nameEntry.ContainsKey("JUVENILE"))
                        qryNames.SetFieldValue("JUVENILE", nameEntry["JUVENILE"].ToString());
                    if (nameEntry.ContainsKey("NAME_VERIFIED"))
                        qryNames.SetFieldValue("NAME_VERIFIED", nameEntry["NAME_VERIFIED"].ToString());

                    if (nameEntry.ContainsKey("COMMENTS"))
                        qryNames.SetFieldValue("COMMENTS", nameEntry["COMMENTS"]);

                    if (nameEntry.ContainsKey("DOC_NUMBER"))
                        qryNames.SetFieldValue("DOC_NUMBER", nameEntry["DOC_NUMBER"]);

                    if (nameEntry.ContainsKey("SUBJECT_CHARGED"))
                        qryNames.SetFieldValue("SUBJECT_CHARGED", nameEntry["SUBJECT_CHARGED"]);

                    if (nameEntry.ContainsKey("TRIAL_DATE"))
                        qryNames.SetFieldValue("TRIAL_DATE", trialDate);

                    if (nameEntry.ContainsKey("GRAND_JURY_DATE"))
                        qryNames.SetFieldValue("GRAND_JURY_DATE", grandJuryDate);

                    if (nameEntry.ContainsKey("STATUS"))
                        qryNames.SetFieldValue("STATUS", nameEntry["STATUS"]);

                    qryNames.Post("TV_SUBMNAME", auditCode, auditSubCode);

                    if (nameEntry["RESUBMISSION"].ToString() == "T")
                        SaveResubNameInfo(departmentCaseNumber, departmentCode, submissionNumber, nameEntry["NAME_KEY"].ToString(), nameEntry["RESUBMISSION"].ToString(), true);
                }
            }

            PLCQuery qryUpdateCase = new PLCQuery();
            qryUpdateCase.SQL = "SELECT HAS_JUVENILE FROM TV_SUBMTRAN WHERE DEPARTMENT_CODE = '" + departmentCode + "' " +
                "AND DEPARTMENT_CASE_NUMBER = '" + departmentCaseNumber + "' " +
                "AND SUBMISSION_NUMBER = '" + submissionNumber + "' ";
            qryUpdateCase.Open();

            if (qryUpdateCase.HasData())
            {
                qryUpdateCase.Edit();
                qryUpdateCase.SetFieldValue("HAS_JUVENILE", (hasJuvenile ? "Y" : "N"));
                qryUpdateCase.Post("TV_SUBMTRAN", 9001, 2);
            }

           
        }