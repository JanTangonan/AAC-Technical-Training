public void GetPrelogTran()
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
            string barcodeCaseKey = HttpContext.Current.Request["barcodeCaseKey"];           
            string limsCaseKey = HttpContext.Current.Request["LIMSCaseKey"];
            string prelogLIMSSearch = HttpContext.Current.Request["isPrelogLIMSSearch"];
            string crimeSceneCase = HttpContext.Current.Request["isCrimeSceneCase"];
            string qcAttachmentKeys = string.Empty;
            bool isPrelogLIMSSearch = prelogLIMSSearch == "T";
            bool isCrimeSceneCase = crimeSceneCase == "T";

            List<string> lstOffenses = new List<string>();

            if (string.IsNullOrEmpty(departmentCode) && string.IsNullOrEmpty(departmentCaseNumber) && string.IsNullOrEmpty(submissionNumber) && !string.IsNullOrEmpty(prelogSequence))
                GetPrelogSubmissionKeys(prelogSequence, out departmentCaseNumber, out departmentCode, out submissionNumber);

            PLCSession.SetProperty<bool>("IsQCPDF417", HttpContext.Current.Request["isPDF417"] == "T");

            string tablePrefix = "TV_";
            string sql = "SELECT * FROM " + tablePrefix + @"SUBMTRAN 
                WHERE DEPARTMENT_CODE = '" + departmentCode + @"' 
                AND DEPARTMENT_CASE_NUMBER = '" + departmentCaseNumber + @"' 
                AND SUBMISSION_NUMBER = '" + submissionNumber + "'";

            PLCQuery query = new PLCQuery(sql);
            query.Open();

            try
            {
                // ETRACK source is direct submnission from FDLE PoliceBEAST maybe others.
                //The case officeer is free form. This will make sure it is in DEPTPERS.
                if ((query.HasData()) && (query.FieldExist("SOURCE")) &&  (query.FieldByName("SOURCE") == "ETRACK"))
                {
                    String caseOfficerName = query.FieldByName("CASE_OFFICER");
                    String caseOfficerEmail = "";
                    //Check to make sure the case officer exists in tv_deptpers. throw away the email.
                    if (!String.IsNullOrWhiteSpace(caseOfficerName))
                        caseOfficerEmail = PLCDBGlobal.instance.FindAttentionEmailByName(departmentCode, caseOfficerName);
                }
            }
            catch (Exception e)
            {
                PLCSession.WriteDebug("Exception in GetPRelogTran():" + e.Message,true);
            }

            int srMasterKey = 0;
            int caseKey = 0;
            string caseIsInLIMS = (CaseExistsInLIMS(departmentCode, departmentCaseNumber, out caseKey) ? "T" : "F");
            string caseLabCode = GetSubmittedCaseLabCode(caseKey);
            bool isUsesPrelogSRComment = PLCSession.GetLabCtrlFlag("QC_USES_PRELOG_SR_COMMENTS").Equals("T");

            Dictionary<string, object> dictResult = new Dictionary<string, object>();
            for (int i = 0; i < query.FieldCount(); i++)
            {
                string fieldname = query.FieldNames(i + 1);
                string fieldvalue = query.FieldByIndex(i).Trim();

                if (fieldname.Contains("_DATE"))
                    fieldvalue = FormatDateString(fieldvalue); //to ensure all dates are returned in mm/dd/yyyy format

                if (fieldname.Contains("OFFENSE_CODE") && !string.IsNullOrEmpty(fieldvalue) && !lstOffenses.Contains(fieldvalue))
                    lstOffenses.Add(fieldvalue);

                if(fieldname.ToUpper().Equals("LAB_CODE") && string.IsNullOrWhiteSpace(fieldvalue) && !string.IsNullOrWhiteSpace(caseLabCode))
                {
                    fieldvalue = caseLabCode;
                }

                if (fieldname.ToUpper().Equals("COMMENTS") && isUsesPrelogSRComment)
                {
                    string comments = PLCDBGlobal.instance.GetPrelogSRFieldValue("REQUEST_COMMENTS", departmentCode, departmentCaseNumber, submissionNumber);
                    fieldvalue = comments;
                }
                dictResult.Add(fieldname, fieldvalue);
            }

      
            Dictionary<string,string> dictOverWriteFields = new Dictionary<string,string>();
            if (caseIsInLIMS == "T")
            {
                dictOverWriteFields = GetPrelogDBPanelOverWriteFields(caseKey);
            }

            if (isPrelogLIMSSearch && isCrimeSceneCase && !string.IsNullOrEmpty(limsCaseKey))
            {
                caseKey = PLCSession.SafeInt(limsCaseKey);
                caseIsInLIMS = "T";
            }

            if (!string.IsNullOrEmpty(barcodeCaseKey) && caseKey < 1)
            {
                caseIsInLIMS = "T";
                caseKey = PLCSession.SafeInt(barcodeCaseKey);
            }

            if (PLCSession.GetLabCtrl("QC_ATTACHMENTS_BUTTON") == "T")
            {
                qcAttachmentKeys = SaveSubImgToQCImg(departmentCode, departmentCaseNumber, submissionNumber);
            }

            dictResult["CASE_KEY"] = caseKey;
            dictResult.Add("SR_DETAIL_KEY", GetSRDetailKeyList(departmentCode, departmentCaseNumber, submissionNumber, out srMasterKey));
            dictResult.Add("SR_MASTER_KEY", srMasterKey);
            dictResult.Add("LAB_CASE_NUMBER", GetLabCaseNumber(caseKey.ToString()));
            dictResult.Add("DEFAULT_PRIORITY", GetLabCasePriority(caseKey.ToString()));
            dictResult.Add("$LIMS_CASE_STATUS", caseIsInLIMS);
            dictResult.Add("NEXT_SUBMISSION_NUMBER", GetNextLIMSSubmissionNumber(caseKey.ToString()));
            dictResult.Add("LPNAMEREC", JSONStrFromStruct(GetSUBMLPNAMETMPRecord(submissionNumber, departmentCaseNumber, departmentCode)));
            dictResult.Add("NAMEREFREC", JSONStrFromStruct(GetSUBMNameRefRec(submissionNumber, departmentCaseNumber, departmentCode)));
            dictResult.Add("SREXISTS", PLCSession.GetLabCtrl("PRELOG_SR_REQUIRED") == "T" ? CheckPrelogSRRequestExistence(departmentCaseNumber, departmentCode, submissionNumber) : "T");
            dictResult.Add("HASDRAFTSR", CheckForDraftPrelogSRRequests(departmentCaseNumber, departmentCode, submissionNumber));
            dictResult.Add("QC_ATTACHMENT_KEYS", qcAttachmentKeys);
            dictResult.Add("DEPT_NAME", PLCDBGlobal.instance.GetDeptNameField("DEPARTMENT_NAME", departmentCode));
            dictResult.Add("QC_OVERWRITEFIELDS", JSONStrFromStruct(dictOverWriteFields));

            QCAdvancedUtil qcUtil = new QCAdvancedUtil();
            string showBiology = "F";
            if (PLCSession.GetLabCtrl("USES_LAB_OFFENSE") == "T")
            {
                foreach (string offense in lstOffenses)
                    if (PLCDBGlobal.instance.CheckOffenseLevel(offense, "F"))
                        showBiology = "T";

                if (showBiology == "F" && caseKey > 0 && PLCDBGlobal.instance.CaseHasFelonyLabOffense(caseKey.ToString()))
                    showBiology = "T";
            }
            else if (PLCSession.GetLabCtrl("USES_BIO_IDENTITY_PURPOSES") == "T")
            {
                if (lstOffenses.Count > 0 && PLCDBGlobal.instance.CheckOffenseLevel(lstOffenses[0], "F"))
                    showBiology = "T";

                if (lstOffenses.Count > 1 && PLCDBGlobal.instance.CheckOffenseLevel(lstOffenses[1], "F"))
                    showBiology = "T";

                if (lstOffenses.Count > 2 && PLCDBGlobal.instance.CheckOffenseLevel(lstOffenses[2], "F"))
                    showBiology = "T";

                if (showBiology == "F" && caseKey > 0 && PLCDBGlobal.instance.CaseHasFelonyOffense(caseKey.ToString()))
                    showBiology = "T";
            }

            if (PLCSession.GetLabCtrlFlag("CASE_USES_SR_COURT_DATE") == "T")
            {
                string courtDate = FormatDateString(PLCDBGlobal.instance.GetPrelogSRFieldValue("COURT_DATE", departmentCode, departmentCaseNumber, submissionNumber));
                if (dictResult.ContainsKey("COURT_DATE"))
                    dictResult["COURT_DATE"] = courtDate;
                else
                    dictResult.Add("COURT_DATE", courtDate);
            }

            dictResult.Add("SHOWBIOLOGY", showBiology);      
            dictResult.Add("LASTCONTAINER", qcUtil.GetLastContainer(caseKey.ToString()));

          
            HttpContext.Current.Response.Write(JSONStrFromStruct(dictResult.ToArray()));
            HttpContext.Current.Response.Write("");
            HttpContext.Current.Response.Flush();
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }