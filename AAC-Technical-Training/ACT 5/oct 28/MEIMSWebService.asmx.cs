using PLCCONTROLS;
using PLCGlobals;
using PLCGlobals.MEIMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace BEASTiLIMS.Web_Services
{
    /// <summary>
    /// Summary description for MEIMSWebService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]
    public class MEIMSWebService : System.Web.Services.WebService
    {

        #region Endpoints
        [WebMethod(EnableSession = true)]
        public object SaveNewCaseRecord(object data, string panelName)
        {
            var meims = new MEIMS();
            var caseRecord = (Dictionary<string, object>)data;
            var tables = new Dictionary<string, Dictionary<string, string>>();
            var response = new Dictionary<string, string>();
            var keyList = new List<string>();
            int caseKey = 0;
            int subKey = 0;
            int medKey = 0;
            int ecnBody = 0;
            int nameKey = 0;

            ValidateDBPanel(panelName, caseRecord); // throws an error when not valid

            foreach (string key in caseRecord.Keys)
            {
                string tableName = key.Split('.')[0];
                string fieldName = key.Split('.')[1];

                if (!tables.Keys.Contains(tableName))
                    tables.Add(tableName, new Dictionary<string, string>());

                tables[tableName].Add(fieldName, caseRecord[key].ToString());
            }

            #region LABCASE
            caseKey = meims.SaveLabCase(tables);

            int submissionNumber;
            subKey = meims.SaveLabSub(caseKey, new Dictionary<string, string>(), out submissionNumber);

            if (PLCSession.GetLabCtrl("AUTO_CREATE_FILE_ITEM") == "T")
            {
                int ecn = meims.AddFileItem(caseKey, subKey, submissionNumber);
                keyList.Add("IT:" + ecn);
            }
            #endregion LABCASE

            #region MEDBODY
            if (tables.ContainsKey("TV_MEDBODY"))
            {
                medKey = meims.SaveMedBody(caseKey, PLCSession.PLCGlobalNameKey, tables["TV_MEDBODY"]);

                int ecn;
                var medIntakeDefaults = new Dictionary<string, string>() {
                    { "INTAKE_DATE", DateTime.Now.ToString("MM/dd/yyyy") },
                    { "INTAKE_TIME", DateTime.Now.ToString() }
                };
                int morgueNumber = meims.SaveMedIntake(caseKey, medKey, medIntakeDefaults, out ecn);

                // Save Case Status Tags
                meims.CreateCaseStatusTags(caseKey, medKey);

                if (ecn > 0)
                {
                    ecnBody = ecn;
                    keyList.Add("IT:" + ecn);
                }
            }
            #endregion MEDBODY

            #region MEDSCENE
            if (tables.ContainsKey("TV_MEDSCENE"))
            {
                meims.SaveMedScene(caseKey, medKey, PLCSession.PLCGlobalNameKey, tables["TV_MEDSCENE"]);
            }
            #endregion MEDSCENE

            #region LABNAME
            if (tables.ContainsKey("TV_LABNAME"))
            {
                nameKey = meims.SaveLabName(caseKey, medKey, tables["TV_LABNAME"]);

                // if body is added to labitem, create a link to name
                if (ecnBody > 0)
                {
                    meims.CreateItemNameLink(ecnBody, nameKey);
                }
            }
            #endregion LABNAME

            #region NAMEREF
            if (tables.ContainsKey("TV_NAMEREF") && nameKey > 0)
            {
                meims.SaveNameRef(caseKey, nameKey, tables["TV_NAMEREF"]);
            }
            #endregion NAMEREF

            PLCSession.PLCGlobalCaseKey = caseKey.ToString();
            PLCSession.SetCaseVariables(PLCSession.PLCGlobalCaseKey);
            PLCSession.PLCGlobalSubmissionKey = subKey.ToString();
            //PLCSession.PLCGlobalMedKey = medKey.ToString();
            PLCSession.PLCGlobalNameKey = nameKey.ToString();

            PLCSession.AddToRecentCases(PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalAnalyst);

            if (PLCSession.GetLabCtrl("USES_WS_BARCODE_PRINTING") == "T")
            {
                PLCSession.SetDefault("BARCODE_KEYLIST", string.Join(",", keyList));
                PLCSession.PLCCrystalReportName = MEIMSHelper.GetLabelReportName(PLCSession.PLCGlobalLabCode);
                response.Add("printLabel", "true");
            }

            response.Add("redirect", "Tab3Names.aspx");
            response.Add("success", "true");

            return response;
        }

        #region MEIMS Verification

        [WebMethod(EnableSession = true)]
        public object GetItemVerificationDetails(int ecn)
        {
            Dictionary<string, string> dictResponse = new Dictionary<string, string>();
            string result = "ok";
            string title = "MEIMS Verification";
            string message = string.Empty;

            PLCQuery qryLabItem = new PLCQuery();
            qryLabItem.SQL = "SELECT LABITEM.ME_VERIFICATION_TYPE, LABITEM.ME_VERIFICATION_ID, LABITEM.ME_VERIFICATION_SIGNATURE, ANALYST.NAME FROM TV_LABITEM LABITEM " +
                "LEFT OUTER JOIN TV_ANALYST ANALYST ON ANALYST.ANALYST = ME_VERIFICATION_ID " +
                "WHERE LABITEM.EVIDENCE_CONTROL_NUMBER = ?";

            qryLabItem.AddSQLParameter("EVIDENCE_CONTROL_NUMBER", ecn);
            qryLabItem.OpenReadOnly();

            if (qryLabItem.IsEmpty())
                return CreateErrorResponse("Item not found.");

            string verificationType = qryLabItem.FieldByName("ME_VERIFICATION_TYPE");

            if (string.IsNullOrEmpty(verificationType))
                return CreateErrorResponse("Item not yet verified");

            // Proceed with getting the details
            if (verificationType == "Transporter")
            {
                dictResponse["signatureKey"] = qryLabItem.FieldByName("ME_VERIFICATION_SIGNATURE");
            }
            else if (verificationType == "Attendant/Investigator")
            {
                dictResponse["userID"] = qryLabItem.FieldByName("ME_VERIFICATION_ID");
                dictResponse["userName"] = qryLabItem.FieldByName("NAME");
            }

            dictResponse["verificationType"] = verificationType;
            dictResponse["result"] = result;
            dictResponse["title"] = title;
            dictResponse["message"] = message;

            return dictResponse;
        }

        [WebMethod(EnableSession = true)]
        public object ValidateAnalyst(string userID, string userPassword)
        {
            Dictionary<string, string> dictResponse = new Dictionary<string, string>();
            string result = "ok";
            string message = string.Empty;

            if (!PLCSession.ValidatePassword(userID, userPassword, out message))
                result = "none";

            dictResponse["result"] = result;
            dictResponse["message"] = message;

            return dictResponse;
        }

        [WebMethod(EnableSession = true)]
        public object SaveItemVerification(Dictionary<string, object> data)
        {
            Dictionary<string, string> dictResponse = new Dictionary<string, string>();
            string message = string.Empty;
            string result = "ok";

            string items = data["ecnList"].ToString();
            string verificationType = data["verificationType"].ToString();
            string verificationData = data["verificationData"].ToString();

            List<int> ecnList = items.Split(',').Select(int.Parse).ToList();

            if (!MEIMSHelper.SaveItemVerification(ecnList, verificationType, verificationData, out message))
            {
                dictResponse["message"] = message;
                result = "none";
            }

            dictResponse["result"] = result;
            return dictResponse;
        }

        #endregion MEIMS Verification

        #endregion Endpoints

        #region Private Methods
        /// <summary>
        /// Validates DBPanel data
        /// </summary>
        /// <param name="panelName"></param>
        /// <param name="record"></param>
        /// <exception cref="Exception">invalid DBPanel data.</exception>
        private void ValidateDBPanel(string panelName, Dictionary<string, object> record)
        {
            PLCQuery qryDBPanel = new PLCQuery();
            qryDBPanel.SQL = "SELECT TABLE_NAME, FIELD_NAME, PROMPT FROM TV_DBPANEL " +
                "WHERE PANEL_NAME = ? AND MANDATORY = 'T' " +
                "AND (REMOVE_FROM_SCREEN IS NULL OR REMOVE_FROM_SCREEN != 'T') " +
                "ORDER BY SEQUENCE";
            qryDBPanel.AddSQLParameter("PANEL_NAME", panelName);
            qryDBPanel.Open();
            string emptyMandatoryFields = string.Empty;

            if (qryDBPanel.HasData())
            {
                while (!qryDBPanel.EOF())
                {
                    string field = qryDBPanel.FieldByName("TABLE_NAME").Replace("TV_", "") + "." + qryDBPanel.FieldByName("FIELD_NAME").ToUpper().Replace(" ", "_");
                    object fieldValue = string.Empty;
                    string key = "TV_" + field;

                    if (record.ContainsKey(key))
                    {
                        if (string.IsNullOrEmpty(record[key].ToString()))
                            emptyMandatoryFields += qryDBPanel.FieldByName("PROMPT") + "\n";
                    }

                    qryDBPanel.Next();
                }
            }

            if (!string.IsNullOrEmpty(emptyMandatoryFields))
            {
                string errorMessage = "The following fields are required: \n" + emptyMandatoryFields;

                Exception myException = new Exception(errorMessage);
                throw myException;
            }
        }

        private Dictionary<string, string> CreateErrorResponse(string message)
        {
            Dictionary<string, string> response = new Dictionary<string, string>();
            response["result"] = "error";
            response["message"] = message;
            return response;
        }

        #endregion Private Methods
    }
}
