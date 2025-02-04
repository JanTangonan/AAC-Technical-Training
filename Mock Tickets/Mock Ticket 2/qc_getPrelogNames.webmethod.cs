public void GetPrelogNames()
        {
            HttpContext.Current.Response.ClearHeaders();
            HttpContext.Current.Response.ClearContent();
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            HttpContext.Current.Response.ContentType = "application/json";
            HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;

            string departmentCode = HttpContext.Current.Request["departmentCode"];
            string departmentCaseNumber = HttpContext.Current.Request["departmentCaseNumber"];
            string submissionNumber = HttpContext.Current.Request["submissionNumber"];
            string prelogSequence = HttpContext.Current.Request["prelogSequence"];
            bool isPrelogLIMSSearch = (HttpContext.Current.Request["isPrelogLIMSSearch"] != null ? (HttpContext.Current.Request["isPrelogLIMSSearch"].ToString() == "T" ? true : false) : false);
            string LIMSLabCaseNumber = (HttpContext.Current.Request["LIMSLabCaseNumber"] != null ? HttpContext.Current.Request["LIMSLabCaseNumber"].ToString() : "");
            string searchKit = HttpContext.Current.Request["kitsearch"];

            if (string.IsNullOrEmpty(departmentCode) && string.IsNullOrEmpty(departmentCaseNumber) && string.IsNullOrEmpty(submissionNumber) && !string.IsNullOrEmpty(prelogSequence))
                GetPrelogSubmissionKeys(prelogSequence, out departmentCaseNumber, out departmentCode, out submissionNumber);

            List<Dictionary<string, object>> dictValues = new List<Dictionary<string, object>>();

            string tablePrefix = "TV_";
            string sql = "SELECT NAME_SEQUENCE, NAME_NUMBER, NAME_TYPE_CODE, LAST_NAME, FIRST_NAME, MIDDLE_NAME, SEX_CODE, RACE_CODE, " + 
                        " DATE_OF_BIRTH, NAME_KEY, DEPT_NAME_KEY, FBI_NUMBER, PID_NUMBER, STATE_ID, SUFFIX, DCI_NUMBER, COMMENTS, SUBJECT_CHARGED, " +
                        " TRIAL_DATE, GRAND_JURY_DATE, DOC_NUMBER, STATUS, ALIVE, BLEEDING, TRANSFUSED  FROM " + tablePrefix + @"SUBMNAME 
                WHERE DEPARTMENT_CODE = '" + departmentCode + @"' 
                AND DEPARTMENT_CASE_NUMBER = '" + departmentCaseNumber + @"' 
                AND SUBMISSION_NUMBER = '" + submissionNumber + @"' 
                AND (NAME_KEY IS NULL OR NAME_KEY = '')";

            PLCQuery query = new PLCQuery(sql);          
            query.Open();

            while (!query.EOF())
            {
                string nameNumber = query.FieldByName("NAME_NUMBER");
                string dob = PLCDBGlobal.instance.GetPaddedDateStr(query.FieldByName("DATE_OF_BIRTH"));
                string firstName = query.FieldByName("FIRST_NAME");
                string lastName = query.FieldByName("LAST_NAME");
                string trialDate = PLCDBGlobal.instance.GetPaddedDateStr(query.FieldByName("TRIAL_DATE"));
                string grandJuryDate = PLCDBGlobal.instance.GetPaddedDateStr(query.FieldByName("GRAND_JURY_DATE"));
                if ((isPrelogLIMSSearch && PrelogNameMatchesInLIMS(LIMSLabCaseNumber, query.FieldByName("NAME_TYPE_CODE"), lastName, firstName, query.FieldByName("SEX_CODE"), dob)) || 
                    (searchKit == "T" && NameExistsInLIMS(departmentCaseNumber, departmentCode, firstName, lastName)))
                    
                {
                    query.Next();
                    continue;
                }

                string stateID = GetStateID(departmentCode, departmentCaseNumber, submissionNumber, nameNumber, query.FieldByName("NAME_KEY"));
                if (string.IsNullOrEmpty(stateID))
                    stateID = query.FieldByName("STATE_ID");

                Dictionary<string, object> dictResult = new Dictionary<string, object>();
                dictResult.Add("NAMEKEY", "");
                dictResult.Add("DEPTNAMEKEY", query.FieldByName("DEPT_NAME_KEY"));
                dictResult.Add("ARRESTNUMBER", query.FieldByName("DOC_NUMBER"));
                dictResult.Add("NAMETYPE", query.FieldByName("NAME_TYPE_CODE"));
                dictResult.Add("LAST", query.FieldByName("LAST_NAME"));
                dictResult.Add("FIRST", query.FieldByName("FIRST_NAME"));
                dictResult.Add("MIDDLE", query.FieldByName("MIDDLE_NAME"));
                dictResult.Add("SEX", query.FieldByName("SEX_CODE"));
                dictResult.Add("RACE", query.FieldByName("RACE_CODE"));
                dictResult.Add("DOB", dob);
                dictResult.Add("AGE", "");
                dictResult.Add("ARRESTDATE", "");
                dictResult.Add("STATEID", query.FieldByName("STATE_ID"));
                dictResult.Add("NAMESEQ", query.FieldByName("NAME_SEQUENCE"));
                dictResult.Add("NAMEID", "");
                dictResult.Add("HOMEPHONE", "");
                dictResult.Add("CELLPHONE", "");
                dictResult.Add("PIDNUMBER", query.FieldByName("PID_NUMBER"));
                dictResult.Add("FBINUMBER", query.FieldByName("FBI_NUMBER"));
                dictResult.Add("SUFFIX", query.FieldByName("SUFFIX"));               
                dictResult.Add("DCINUMBER", query.FieldByName("DCI_NUMBER"));
                dictResult.Add("COMMENTS", query.FieldByName("COMMENTS"));
                dictResult.Add("SUBJECT_CHARGED", query.FieldByName("SUBJECT_CHARGED"));
                dictResult.Add("TRIAL_DATE", trialDate);
                dictResult.Add("GRAND_JURY_DATE", grandJuryDate);
                dictResult.Add("STATUS", query.FieldByName("STATUS"));
                dictResult.Add("ALIVE", query.FieldByName("ALIVE"));
                dictResult.Add("BLEEDING", query.FieldByName("BLEEDING"));
                dictResult.Add("TRANSFUSED", query.FieldByName("TRANSFUSED"));
                dictValues.Add(dictResult);
                query.Next();
            }

            HttpContext.Current.Response.Write(JSONStrFromStruct(dictValues.ToArray()));

            HttpContext.Current.Response.Write("");
            HttpContext.Current.Response.Flush();
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }