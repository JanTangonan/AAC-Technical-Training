public void LoadCaseDataFromLabCaseNumber()
        {
            HttpContext.Current.Response.ClearHeaders();
            HttpContext.Current.Response.ClearContent();
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            HttpContext.Current.Response.ContentType = "application/json";
            HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;

            string labcasenumber = HttpContext.Current.Request["labcasenumber"];
            string jsonCaseSubmission = HttpContext.Current.Request["jsoncasesubmission"];

            Dictionary<string, object> dictLoadResult = new Dictionary<string, object>();

            QCAdvancedUtil qcutil = new QCAdvancedUtil();
            if (qcutil.LoadCaseInfoFromLabCaseNumber(labcasenumber, jsonCaseSubmission))
            {
                CaseDataPacket casedata = qcutil.GetCaseDataPacket();
                dictLoadResult["result"] = "ok";
                dictLoadResult["editstate"] = "Edit";
                dictLoadResult["casekey"] = casedata.casekey;
                dictLoadResult["casesubmission"] = casedata.casesubmission;
                dictLoadResult["names"] = casedata.names;
                dictLoadResult["items"] = casedata.items;
                dictLoadResult["existingitems"] = casedata.existingitems;
                dictLoadResult["existingnames"] = casedata.existingnames;
                dictLoadResult["casetitle"] = casedata.titleText;
                dictLoadResult["lastcontainer"] = casedata.lastContainer;
                dictLoadResult["stmtOfFacts"] = casedata.stmtOfFacts;
                dictLoadResult["fileComments"] = casedata.fileComments;
                dictLoadResult["distributions"] = casedata.distributions;
                dictLoadResult["references"] = casedata.references;
            }
            else
            {
                // No case data loaded.
                dictLoadResult["result"] = "none";
            }

            HttpContext.Current.Response.Write(JSONStrFromStruct(dictLoadResult));

            HttpContext.Current.Response.Write("");
            HttpContext.Current.Response.Flush();
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }