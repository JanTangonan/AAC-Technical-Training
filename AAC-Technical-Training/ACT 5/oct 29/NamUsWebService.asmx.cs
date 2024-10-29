using PLCCONTROLS;
using PLCGlobals;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace BEASTiLIMS.WebServices
{
    /// <summary>
    /// Summary description for NamUsWebService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class NamUsWebService : System.Web.Services.WebService
    {
        #region Models
        public class DBPanelData {
            public IDictionary<string, string> Values { get; set; }
            public IDictionary<string, DBPanelField> Fields { get; set; }
        }

        public class DBPanelField
        {
            public string Prompt { get; set; }
            public string EditMask { get; set; }
            public string CodeTable { get; set; }
            public char LikeSearch { get; set; }

        }

        public class RFUParameter
        {
            public RFUFile[] Files { get; set; }
        }

        public class RFUFile
        {
            public string ContentType { get; set; }
            public string FileName { get; set; }
            public string FileSize { get; set; }
            public string Guid { get; set; }

            internal string GetFilePath()
            {
                return Path.GetTempPath() + Guid;
            }
        }

        public class NamUsBatch
        {
            public IDictionary<string, string> RTI { get; set; }
            public IList<IDictionary<string, object>> Data { get; set; }
            public RFUFile File { get; set; }
        }

        #region Case
        public class Submission
        {
            public int SubmissionKey { get; set; }
            public int SubmissionNumber { get; set; }
        }

        public class Item
        {
            public int ECN { get; set; }
            public string LabItemNumber { get; set; }
            public string ItemType { get; set; }
            public string ItemDescription { get; set; }
            internal bool IsNew { get; set; }
        }

        public class Assignment
        {
            public int ExamKey { get; set; }
            public string Section { get; set; }
            public int ECN { get; set; }
            internal int TaskNumber { get; set; }
            public string ReportFormat { get; internal set; }
        }

        public class WorkList
        {
            public int WorkListId { get; set; }
            public int Sequence { get; set; }
        }
        #endregion Case
        #endregion Models

        #region Declarations
        static PLCHelperFunctions helper = new PLCHelperFunctions();
        #endregion Declarations

        #region EndPoints
        [WebMethod(EnableSession = true)]
        public string SearchNamUsBatch(DBPanelData data)
        {
            var values = data.Values;
            var fields = data.Fields;
            var tables = GetTablesAndAlias("NAMUS_BATCH",
                new Dictionary<string, OrderedDictionary>()
                {
                    { "TV_NAMUSBATCHDATA", new OrderedDictionary { { String.Empty, "NBD" } } }
                });
            string whereClause = string.Empty;

            foreach (var value in values)
            {
                string fieldValue = value.Value.Trim();
                string tableFieldName = value.Key;
                PLCSession.WriteDebug("@SearchNamUsBatch: " + tableFieldName + "=" + fieldValue);

                string tableName;
                string fieldName;
                if (string.IsNullOrEmpty(fieldValue)
                    || !ParseTableFieldName(tableFieldName, out tableName, out fieldName))
                {
                    continue;
                }
                PLCSession.WriteDebug("@SearchNamUsBatch: " + tableName + "." + fieldName);

                // Get search options
                var field = fields[tableFieldName];
                string editMask = field.EditMask.Trim().ToUpper();
                string codeTable = field.CodeTable;
                string prompt = field.Prompt;
                bool useLikeSearch = field.LikeSearch == 'T';
                bool useSoundexSearch = false;

                // Handle DMY format
                if (editMask == "DD/MM/YYYY")
                {
                    fieldValue = PLCSession.DateStringToMDY(fieldValue);
                }

                // Get where clause
                whereClause += PLCCommon.instance.GetWhereClauseByType(
                    tableName: tableName,
                    fieldName: fieldName,
                    prompt: prompt,
                    mask: editMask,
                    codeTable: codeTable,
                    searchValue: fieldValue,
                    tables: tables,
                    likeSearch: useLikeSearch,
                    soundexSearch: useSoundexSearch);
            }

            if (!string.IsNullOrEmpty(whereClause))
                whereClause = PLCSession.FormatSpecialFunctions(whereClause.Substring(5));

            PLCSession.WriteDebug("@SearchNamUsBatch: " + whereClause);
            return whereClause;
        }

        [WebMethod(EnableSession = true)]
        public string ImportNamUsBatchFile(RFUParameter parameter)
        {
            var result = new Dictionary<string, object>();
            if (parameter.Files.Length > 0)
            {
                var file = parameter.Files[0];
                var filePath = file.GetFilePath();
                if (File.Exists(filePath))
                {
                    string connectionString = "Provider=Microsoft.ACE.OLEDB.12.0"
                        + ";Data Source=" + filePath
                        + ";Extended Properties=\"Excel 8.0;HDR=YES;\"";
                    string commandText = "";

                    DataTable dataTable = new DataTable();

                    using (var connection = new OleDbConnection(connectionString))
                    {
                        OleDbDataReader dataReader = null;

                        try
                        {
                            connection.Open();

                            if (commandText == "")
                            {
                                string sheetName = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null).Rows[0]["TABLE_NAME"].ToString();
                                commandText = "SELECT * FROM [" + sheetName + "]";
                            }

                            OleDbCommand command = new OleDbCommand(commandText, connection);
                            dataReader = command.ExecuteReader(CommandBehavior.CloseConnection);
                            dataTable.Load(dataReader);
                        }
                        catch (Exception ex)
                        {
                            PLCSession.WriteDebug("@ImportNamUsBatchFile: " + file.FileName
                                + "\n" + ex.Message
                                + "\n" + ex.StackTrace);
                            throw ex;
                        }
                        finally
                        {
                            if (dataReader != null)
                                dataReader.Close();
                        }
                    }

                    if (dataTable.Rows.Count > 0)
                    {
                        // "." gets replaced with "#" in the DataTable
                        var mapping = new Dictionary<string, string>() {
                            { "RTI Submission # (MN to Add)",   "TRACKING_NUMBER" }, // LABSUB
                            { "Bode No#",                       "DEPARTMENT_ITEM_NUMBER" }, // LABITEM
                            { "NamUs No#",                      "REFERENCE" }, // LABREF
                            { "Case Number",                    "DEPARTMENT_CASE_NUMBER" }, // LABCASE
                            { "Case Type",                      "CASE_TYPE|TV_CASETYPE" }, // LABCASE
                            { "Test Type",                      "TASK_TYPE|TV_TASKTYPE" }, // TASKLIST
                            { "Evidence Type",                  "ITEM_TYPE|TV_ITEMTYPE" }, // LABITEM
                            { "Family Reference Type",          "RELATION_CODE|TV_ITNAMERL" }, // ITEMNAME
                            { "Description",                    "ITEM_DESCRIPTION" }, // LABITEM
                            { "Note",                           "ITEM_TEXT" } // LABITEM
                        };

                        var data = new List<Dictionary<string, object>>();
                        var columns = dataTable.Columns;
                        var rows = dataTable.Rows;
                        string trackingNumber = string.Empty;
                        for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
                        {
                            var row = rows[rowIndex];
                            // do not include rows without NamUs No.
                            if (string.IsNullOrEmpty(row["NamUs No#"].ToString().Trim()))
                                continue;

                            var dataRow = new Dictionary<string, object>();
                            dataRow.Add("INDEX", rowIndex);
                            foreach (var map in mapping)
                            {
                                string columnName = map.Key;
                                if (columns.Contains(columnName))
                                {
                                    string field = map.Value;
                                    string value = row[columnName].ToString().Trim();

                                    if (!field.Contains("|"))
                                    {
                                        dataRow.Add(field, value);
                                    }
                                    else
                                    {
                                        string[] fieldTable = field.Split('|');
                                        string codeField = fieldTable[0];
                                        string descField = codeField + "_DESC";

                                        bool isActive;
                                        string code = GetCodeFromDescription(fieldTable[1], value, out isActive);
                                        dataRow.Add(codeField, isActive ? code : string.Empty);
                                        dataRow.Add(descField, value);
                                        if (!string.IsNullOrEmpty(value))
                                        {
                                            if (string.IsNullOrEmpty(code))
                                                dataRow.Add(codeField + ".NOTFOUND", "T");
                                            if (!isActive)
                                                dataRow.Add(codeField + ".INACTIVE", "T");
                                        }
                                    }
                                }
                            }

                            // names
                            var names = new List<Dictionary<string, string>>();
                            names.Add(new Dictionary<string, string>() {
                                { "NAME_TYPE", "M" },
                                { "NAME_TYPE_DESC", PLCSession.GetCodeDesc("TV_NAMETYPE", "M") },
                                { "FIRST_NAME", row["Missing Person First Name"].ToString().Trim() },
                                { "MIDDLE_NAME", row["Missing Person Middle Name"].ToString().Trim() },
                                { "LAST_NAME", row["Missing Person Last Name"].ToString().Trim() }
                            });
                            names.Add(new Dictionary<string, string>() {
                                { "NAME_TYPE", "R" },
                                { "NAME_TYPE_DESC", PLCSession.GetCodeDesc("TV_NAMETYPE", "R") },
                                { "FIRST_NAME", row["Description - FRS First Name"].ToString().Trim() },
                                { "MIDDLE_NAME", row["Description - FRS Middle Name"].ToString().Trim() },
                                { "LAST_NAME", row["Description - FRS Last Name"].ToString().Trim() }
                            });
                            dataRow.Add("names", names);

                            // ori
                            string ori = row["ORI No#"].ToString();
                            string deptName = row["Agency"].ToString();
                            string mailingAddress = row["Address"]
                                + "\n" + row["City"]
                                + "\n" + row["State"]
                                + "\n" + row["Zip Code"];
                            var departments = GetOriDepartments(ori);
                            string deptCode = departments.Count == 1
                                ? departments[0]["DEPARTMENT_CODE"]
                                : "";
                            var department = new Dictionary<string, object>()
                            {
                                { "ORI", ori },
                                { "DEPARTMENT_NAME", deptName },
                                { "MAILING_ADDRESS", mailingAddress },
                                { "DEPARTMENT_CODE", deptCode },
                                { "departments", departments }
                            };
                            dataRow.Add("department", department);

                            // contacts
                            var contactNames = row["Contact Name "].ToString().Split(';');
                            var contactEmails = row["Contact Email"].ToString().Split(';');
                            var contactPhones = row["Contact Phone Number"].ToString().Split(';');
                            var contacts = new List<Dictionary<string, string>>();
                            for (int i = 0; i < contactNames.Length; i++)
                            {
                                var contact = new Dictionary<string, string>();
                                contact.Add("NAME", contactNames[i]);
                                contact.Add("EMAIL_ADDRESS", contactEmails.Length > i ? contactEmails[i] : "");
                                contact.Add("PHONE_NUMBER", contactPhones.Length > i ? contactPhones[i] : "");

                                contacts.Add(contact);
                            }
                            dataRow.Add("contacts", contacts);

                            data.Add(dataRow);
                        }

                        // make this configurable (XML flag?)
                        string labCode = "S";
                        string submissionType = "ES"; // Electronic Submission
                        string jurisdiction = "OUT"; // Out of State
                        string section = "NU";
                        string analyst = "DBODE";
                        var rti = new Dictionary<string, string>();
                        rti.Add("LAB_CODE", labCode);
                        rti.Add("SUBMISSION_TYPE", submissionType);
                        rti.Add("JURISDICTION_CODE", jurisdiction);
                        rti.Add("SECTION", section);
                        rti.Add("ANALYST", analyst);
                        rti.Add("TRACKING_NUMBER", data[0]["TRACKING_NUMBER"].ToString());

                        result.Add("rti", rti);
                        result.Add("data", data);
                        result.Add("file", file);
                    }
                }
            }
            return helper.JSONStrFromStruct(result);
        }

        [WebMethod(EnableSession = true)]
        public string SubmitNamUsBatch(NamUsBatch parameter)
        {
            var result = new Dictionary<string, object>();

            // Save file to NAMUSBATCHDATA
            var file = parameter.File;
            string filePath = file.GetFilePath();
            string namUsBatchName = Path.GetFileNameWithoutExtension(file.FileName);
            byte[] namUsBatchData = File.ReadAllBytes(filePath);
            int batchKey = SaveNamUsBatchData(namUsBatchName, namUsBatchData);

            // Save imported data
            var data = parameter.Data;
            var caseSubmissions = new Dictionary<int, Submission>();
            var caseAssignments = new Dictionary<int, List<Assignment>>();
            var taskTypeWorkLists = new Dictionary<string, WorkList>();
            DateTime dateReceived = DateTime.Today;
            DateTime timeReceived = DateTime.Now;
            string receivedBy = PLCSession.PLCGlobalAnalyst;

            // rti
            var rti = parameter.RTI;
            string labCode = rti["LAB_CODE"];
            string submissionType = rti["SUBMISSION_TYPE"]; // Electronic Submission
            string jurisdiction = rti["JURISDICTION_CODE"]; // Out of State
            string section = rti["SECTION"];
            string bodeAnalyst = rti["ANALYST"];
            string trackingNumber = rti["TRACKING_NUMBER"];

            // Add batch key to rti, might come from client-side in the future if signature is added
            rti.Add("BATCH_SEQUENCE", batchKey.ToString());

            #region Create RTI Case
            PLCSession.WriteDebug("@SubmitNamUsBatch: Creating RTI Case - Begin");
            PLCSession.ClearLabCaseVars();

            // labcase
            string rtiDeptCode = rti["DEPARTMENT_CODE"];
            int rtiCaseKey = EnsureLabCase(rtiDeptCode, namUsBatchName, "", labCode, jurisdiction);
            PLCSession.PLCGlobalCaseKey = rtiCaseKey.ToString();

            // labsub
            var submission = CreateLabSub(rtiCaseKey, rti, rtiDeptCode, dateReceived, timeReceived);
            int rtiSubmissionKey = submission.SubmissionKey;
            int rtiSubmissionNumber = submission.SubmissionNumber;
            PLCSession.PLCGlobalSubmissionKey = rtiSubmissionKey.ToString();
            PLCSession.PLCGlobalSubmissionNumber = rtiSubmissionNumber.ToString();

            // labassign
            int rtiExamKey = CreateLabAssign(rtiCaseKey, section, labCode);
            PLCSession.PLCGlobalAssignmentKey = rtiExamKey.ToString();
            PLCSession.WriteDebug("@SubmitNamUsBatch: Creating RTI Case - End");
            #endregion Create RTI Case

            foreach (var row in data)
            {
                PLCSession.WriteDebug("@SubmitNamUsBatch: Creating Case Data - Begin");
                PLCSession.ClearLabCaseVars();

                string caseNumber = row["DEPARTMENT_CASE_NUMBER"].ToString().ToUpper();
                string caseType = row["CASE_TYPE"].ToString();
                string bodeNumber = row["DEPARTMENT_ITEM_NUMBER"].ToString();

                #region Department
                // deptname
                var department = (IDictionary<string, object>)row["department"];
                string departmentCode = department["DEPARTMENT_CODE"].ToString();
                if (string.IsNullOrEmpty(departmentCode))
                    departmentCode = EnsureDepartment(department);

                // deptpers
                var contacts = (object[])row["contacts"];
                int? caseOfficerKey = null;
                foreach (var contact in contacts)
                {
                    var contactDict = (IDictionary<string, object>)contact;
                    int deptPersKey = EnsureDeptPers(departmentCode, contactDict);
                    contactDict.Add("DEPTPERS_KEY", deptPersKey);

                    if (!caseOfficerKey.HasValue)
                        caseOfficerKey = deptPersKey;
                }
                #endregion Department

                #region Case
                // labcase
                int caseKey = EnsureLabCase(departmentCode, caseNumber, caseType, labCode, jurisdiction, caseOfficerKey);
                PLCSession.PLCGlobalCaseKey = caseKey.ToString();
                PLCSession.PLCGlobalDepartmentCaseNumber = caseNumber;

                // casedist
                foreach (var contact in contacts)
                {
                    EnsureCaseDist(caseKey, departmentCode, (IDictionary<string, object>)contact);
                }
                #endregion Case

                #region Submission
                // labsub
                bool isNewSubmission = true;
                if (caseSubmissions.ContainsKey(caseKey))
                {
                    submission = caseSubmissions[caseKey];
                    isNewSubmission = false;
                }
                else
                {
                    submission = CreateLabSub(caseKey, rti, departmentCode, dateReceived, timeReceived);
                    caseSubmissions.Add(caseKey, submission);
                }
                int submissionKey = submission.SubmissionKey;
                int submissionNumber = submission.SubmissionNumber;
                PLCSession.PLCGlobalSubmissionKey = submissionKey.ToString();
                PLCSession.PLCGlobalSubmissionNumber = submissionNumber.ToString();
                #endregion Submission

                #region References
                // labref
                string namUsNumber = row["REFERENCE"].ToString();
                EnsureLabRef(caseKey, submissionKey, "NAMUS", namUsNumber);
                string bodeCaseNumber = bodeNumber.Substring(0, 12);
                EnsureLabRef(caseKey, submissionKey, "BODE", bodeCaseNumber);
                #endregion References

                #region Names
                // labname
                var names = (object[])row["names"];
                foreach (var name in names)
                {
                    var nameDict = (IDictionary<string, object>)name;
                    int nameKey = EnsureLabName(caseKey, nameDict);
                    nameDict.Add("NAME_KEY", nameKey);
                }
                #endregion Names

                #region Item
                // Add the new submission's default items
                if (isNewSubmission
                    && PLCSession.GetLabCtrl("DEFAULT_SUBMISSION_ITEMS") != "")
                {
                    PLCDBGlobal.instance.AddSubmissionDefaultItem(submissionNumber.ToString(), addNamesToAssignment: true);
                }

                // labitem
                string departmentItemNumber = bodeNumber.Substring(13);
                var item = EnsureLabItem(caseKey, submissionNumber, departmentItemNumber, row, dateReceived, timeReceived);
                int ecn = item.ECN;
                string itemType = item.ItemType;
                string labItemNumber = item.LabItemNumber;
                string itemDescription = item.ItemDescription;
                bool isNewItem = item.IsNew;
                PLCSession.PLCGlobalECN = ecn.ToString();

                // labstat
                AddItemCustody(caseKey, ecn, itemType, isNewItem, submissionKey, submissionType, submissionNumber, trackingNumber, receivedBy);

                // sublink
                string resub = isNewItem ? "F" : "T";
                PLCDBGlobal.instance.CreateSubmissionLink(ecn, submissionKey, resub, clearItemContainer: false);

                // Update the resubmitted item's submission number
                if (!isNewItem)
                {
                    PLCDBGlobal.instance.UpdateItemLabCaseSubmission(ecn.ToString(), submissionNumber.ToString());
                }

                // itemname
                string relationCode = row["RELATION_CODE"].ToString();
                foreach (var name in names)
                {
                    var nameDict = (IDictionary<string, object>)name;
                    if (nameDict["NAME_TYPE"].ToString() == "R")
                    {
                        int nameKey = (int)nameDict["NAME_KEY"];
                        EnsureItemNameLink(ecn, nameKey, relationCode);
                    }
                }
                #endregion Item

                #region Assignment
                // Initialize case assignments
                if (!caseAssignments.ContainsKey(caseKey))
                {
                    caseAssignments.Add(caseKey, new List<Assignment>());
                }

                // Add the new item's default assignments
                if (isNewItem
                    && PLCSession.GetLabCtrlFlag("DEFAULT_ASSIGNMENTS") == "T")
                {
                    PLCDBGlobal.instance.AddItemDefaultAssignment(ecn.ToString(), addNamesToAssignment: true);
                }

                // labassign
                int examKey;
                int taskNumber = 0;
                string taskType = row["TASK_TYPE"].ToString();
                string itemQuantity = PLCSession.GetSectionFlag(section, "DEFAULT_ITEM_QUANTITY");
                var assignments = caseAssignments[caseKey];
                bool isSeparateAssignment = PLCSession.CheckItemTypeFlag(itemType, "SEPARATE_ASSIGNMENTS");
                int assignmentECN = isSeparateAssignment ? ecn : -1;
                var assignment = assignments
                    .Where(a => a.Section == section && a.ECN == assignmentECN)
                    .FirstOrDefault();
                if (assignment != null)
                {
                    examKey = assignment.ExamKey;
                    taskNumber = assignment.TaskNumber;
                }
                else
                {
                    examKey = CreateLabAssign(caseKey, section, labCode, taskType);

                    assignment = new Assignment()
                    {
                        ExamKey = examKey,
                        Section = section,
                        ECN = assignmentECN,
                        TaskNumber = taskNumber
                    };
                    assignments.Add(assignment);

                    PLCDBGlobal.instance.UpdateCaseStatus();

                    //if (PLCSession.GetLabCtrlFlag("USES_NY_SECTION_CASE_FILE") == "T")
                    //{
                    //    int newecn = AddFileItem(caseKey, submissionNumber, "0" + section, sVoucherNo);
                    //}

                    PLCDBGlobal.instance.GenerateSectionFileItem(examKey.ToString());
                }
                PLCSession.PLCGlobalAssignmentKey = examKey.ToString();

                // reptname
                AddAssignmentNames(caseKey, examKey, ecn);

                // itassign
                if (PLCDBGlobal.instance.IsItemNotInAssignment(examKey, ecn))
                {
                    PLCDBGlobal.instance.AddItemToAssignment(new ItemAssignment
                    {
                        ExamKey = examKey,
                        ECN = ecn,
                        ItemNumber = labItemNumber,
                        ItemType = itemType,
                        ItemQuantity = itemQuantity,
                        ItemDescription = itemDescription
                    });
                }

                // tasklist
                taskNumber++;
                string taskTypeDescription = PLCSession.GetCodeDesc("TASKTYPE", taskType);
                int taskId = CreateTaskList(caseKey, examKey, ecn, taskNumber, taskType, taskTypeDescription, section, itemType, bodeAnalyst);

                taskNumber = CreateAdditionalTaskList(caseKey, examKey, ecn, taskNumber, taskType, taskTypeDescription, section, itemType);
                assignment.TaskNumber = taskNumber;

                // Set assignment report format
                string reportFormat = assignment.ReportFormat;
                if (string.IsNullOrEmpty(reportFormat))
                {
                    reportFormat = PLCSession.GetTaskTypeFlag(taskType, "DEFAULT_REPORT_FORMAT");
                    if (!string.IsNullOrEmpty(reportFormat))
                    {
                        PLCDBGlobal.instance.SetAssignmentReportFormat(examKey);
                        assignment.ReportFormat = reportFormat;
                    }
                }
                #endregion Assignment

                #region WorkList
                // worklist
                WorkList workList;
                int workListId;
                int workListSequence = 0;
                if (taskTypeWorkLists.ContainsKey(taskType))
                {
                    workList = taskTypeWorkLists[taskType];
                    workListId = workList.WorkListId;
                    workListSequence = workList.Sequence;
                }
                else
                {
                    workListId = CreateWorkList(taskType);

                    workList = new WorkList()
                    {
                        WorkListId = workListId,
                        Sequence = workListSequence
                    };
                    taskTypeWorkLists.Add(taskType, workList);
                }

                // worktask
                workListSequence++;
                AddWorkListTask(workListId, taskId, workListSequence);
                workList.Sequence = workListSequence;
                #endregion WorkList

                #region RTI Case/Item Link
                PLCSession.ClearLabCaseVars();
                PLCSession.PLCGlobalCaseKey = rtiCaseKey.ToString();

                // labref
                string referenceType = Constants.REFERENCE_TYPE_DEPTCASENUMBER;
                EnsureLabRef(rtiCaseKey, rtiSubmissionKey, referenceType, caseNumber, referencedCaseKey: caseKey);

                // itassign
                if (PLCDBGlobal.instance.IsItemNotInAssignment(rtiExamKey, ecn))
                {
                    PLCDBGlobal.instance.AddItemToAssignment(new ItemAssignment
                    {
                        ExamKey = rtiExamKey,
                        ECN = ecn,
                        ItemNumber = labItemNumber,
                        ItemType = itemType,
                        ItemQuantity = itemQuantity,
                        ItemDescription = itemDescription
                    });
                }
                #endregion RTI Case/Item Link
                PLCSession.WriteDebug("@SubmitNamUsBatch: Creating Case Data - End");
            }

            File.Delete(filePath);

            result.Add("success", true);
            result.Add("namUsBatchName", namUsBatchName);

            return helper.JSONStrFromStruct(result);
        }

        [WebMethod(EnableSession = true)]
        public string PrintNamUsBatch(int namUsBatchKey)
        {
            var result = new Dictionary<string, object>();

            PLCSession.PLCCrystalReportName = "NAMUS.rpt";
            PLCSession.PLCCrystalSelectionFormula = "{TV_NAMUSBATCHDATA.NAMUS_BATCH_KEY} = " + namUsBatchKey;
            PLCSession.PLCCrystalReportTitle = "NamUs Batch #" + namUsBatchKey;

            result.Add("success", true);

            return helper.JSONStrFromStruct(result);
        }

        [WebMethod(EnableSession = true)]
        public void OpenWorkList(int workListId)
        {
            PLCSession.PLCBatchTaskID = workListId.ToString();
        }

        [WebMethod(EnableSession = true)]
        public string CompleteNamUsBatch(int namUsBatchKey)
        {
            var result = new Dictionary<string, object>();

            var qry = new PLCQuery();
            qry.SQL = "SELECT NAMUS_BATCH_KEY, STATUS "
                + "FROM TV_NAMUSBATCHDATA WHERE NAMUS_BATCH_KEY = " + namUsBatchKey;
            qry.Open();
            if (qry.HasData())
            {
                qry.Edit();
                qry.AddParameter("STATUS", "C"); // Completed
                qry.Save("TV_NAMUSBATCHDATA");
            }

            result.Add("success", true);

            return helper.JSONStrFromStruct(result);
        }
        #endregion EndPoints

        #region Methods

        private static Dictionary<string, OrderedDictionary> GetTablesAndAlias(string gridName, Dictionary<string, OrderedDictionary> defaultTA)
        {
            var tables = new Dictionary<string, OrderedDictionary>();

            var helper = new PLCHelperFunctions();
            tables = helper.GetTablesAndAlias(gridName);

            if (tables.Count == 0)
                tables = defaultTA;

            return tables;
        }

        private static bool ParseTableFieldName(string tableFieldName, out string tableName, out string fieldName)
        {
            tableName = string.Empty;
            fieldName = string.Empty;

            string[] tableField = tableFieldName.Split('.');
            if (tableField.Length == 2)
            {
                tableName = tableField[0];
                fieldName = tableField[1].Replace("(FROM)", "").Replace("(TO)", "");
            }

            return !string.IsNullOrEmpty(tableName)
                && !string.IsNullOrEmpty(fieldName);
        }

        private static string GetCodeFromDescription(string tableName, string description, out bool isActive)
        {
            PLCSession.WriteDebug("@GetCodeFromDescription: " + tableName + "=" + description);
            var qry = CacheHelper.OpenCachedSqlFieldNames("SELECT * FROM " + tableName + " WHERE 0 = 1");
            string codeField = qry.FieldNames(1);
            string descField = qry.FieldNames(2);
            bool hasActiveField = qry.FieldExist("ACTIVE");

            string code = "";
            isActive = true;

            string cacheKey = tableName + "." + descField + "=" + description + ">" + codeField;
            if (CacheHelper.IsInCache(cacheKey))
            {
                string field = CacheHelper.GetItem(cacheKey).ToString();
                code = field.Substring(0, field.Length - 1);
                isActive = field.Substring(field.Length - 1) == "T";
            }
            else
            {
                string fields = codeField
                    + (hasActiveField ? ", ACTIVE" : "");

                qry = new PLCQuery();
                qry.SQL = "SELECT " + fields + " FROM " + tableName + " WHERE UPPER(" + descField + ") = ?";
                qry.AddSQLParameter(descField, description.Trim().ToUpper());
                qry.OpenReadOnly();
                if (qry.HasData())
                {
                    code = qry.FieldByName(codeField);
                    if (hasActiveField)
                    {
                        isActive = !qry.FieldByName("ACTIVE").ToUpper().Equals("F");
                    }
                }
                CacheHelper.AddItem(cacheKey, code + (isActive ? "T" : "F"));
            }

            PLCSession.WriteDebug("@GetCodeFromDescription: " + code + "=" + isActive);
            return code;
        }

        private IList<IDictionary<string, string>> GetOriDepartments(string ori)
        {
            PLCSession.WriteDebug("@GetOriDepartments: " + ori);
            var depts = new List<Dictionary<string, string>>();

            var qry = new PLCQuery();
            qry.SQL = "SELECT DEPARTMENT_CODE, DEPARTMENT_NAME, MAILING_ADDRESS "
                + "FROM TV_DEPTNAME WHERE ORI = ? AND (ACTIVE = 'T' OR ACTIVE IS NULL)";
            qry.AddSQLParameter("ORI", ori);
            qry.OpenReadOnly();
            while (!qry.EOF())
            {
                var dept = new Dictionary<string, string>();
                for (int i = 1; i <= qry.FieldCount(); i++)
                {
                    string fieldName = qry.FieldNames(i);
                    dept.Add(fieldName, qry.FieldByName(fieldName));
                }
                depts.Add(dept);

                qry.Next();
            }

            PLCSession.WriteDebug("@GetOriDepartments: " + depts.Count);
            return depts.ToArray();
        }

        private static int SaveNamUsBatchData(string namUsBatchName, byte[] data)
        {
            PLCSession.WriteDebug("@SaveNamUsBatchData: " + namUsBatchName);
            int namUsBatchKey = PLCSession.GetNextSequence("BATCH_SEQ");

            var qry = new PLCQuery();
            qry.SQL = "SELECT * FROM TV_NAMUSBATCHDATA WHERE 0 = 1";
            qry.Open();
            qry.Append();
            qry.AddParameter("NAMUS_BATCH_KEY", namUsBatchKey);
            qry.AddParameter("NAMUS_BATCH_NAME", namUsBatchName);
            qry.AddParameter("DATA", data);
            qry.AddParameter("IMPORT_DATE", DateTime.Now);
            qry.AddParameter("IMPORT_BY", PLCSession.PLCGlobalAnalyst);
            qry.AddParameter("STATUS", "I"); // Imported status
            qry.AddParameter("ENTRY_TIME_STAMP", DateTime.Now);
            qry.Save("TV_NAMUSBATCHDATA");

            PLCSession.WriteDebug("@SaveNamUsBatchData: " + namUsBatchKey);
            return namUsBatchKey;
        }

        private static string EnsureDepartment(IDictionary<string, object> department)
        {
            string departmentCode = "";

            string ori = department["ORI"].ToString();

            PLCSession.WriteDebug("@EnsureDepartment: " + ori);
            var qry = new PLCQuery();
            qry.SQL = "SELECT DEPARTMENT_CODE FROM TV_DEPTNAME WHERE ORI = ?";
            qry.AddSQLParameter("ORI", ori);
            qry.OpenReadOnly();
            if (qry.HasData())
            {
                departmentCode = qry.FieldByName("DEPARTMENT_CODE");
            }
            else
            {
                departmentCode = GetNextSysDeptCode();
                department["DEPARTMENT_CODE"] = departmentCode;

                PLCSession.WriteDebug("@EnsureDepartment: New " + ori);
                qry.ClearParameters();
                qry.SQL = "SELECT * FROM TV_DEPTNAME WHERE 0 = 1";
                qry.Open();
                qry.Append();
                qry.AddParameter("ACTIVE", "T");
                foreach (var field in department)
                {
                    string fieldName = field.Key;
                    if (qry.FieldExist(fieldName))
                    {
                        qry.AddParameter(fieldName, field.Value);
                    }
                }
                qry.Save("TV_DEPTNAME", 1001, 1);
            }

            PLCSession.WriteDebug("@EnsureDepartment: " + departmentCode);
            return departmentCode;
        }

        private static string GetNextSysDeptCode()
        {
            var qry = new PLCQuery();
            qry.SQL = "SELECT MAX(DEPARTMENT_CODE) AS DEPARTMENT_CODE FROM TV_DEPTNAME WHERE DEPARTMENT_CODE LIKE 'SYS______'";
            qry.OpenReadOnly();
            string departmentCode = qry.FieldByName("DEPARTMENT_CODE");
            if (string.IsNullOrEmpty(departmentCode))
                departmentCode = "SYS000001";
            else
            {
                int deptCount = PLCSession.SafeInt(departmentCode.Substring(3)) + 1;
                departmentCode = "SYS" + deptCount.ToString().PadLeft(6, '0');
            }
            return departmentCode;
        }

        private static int EnsureDeptPers(string departmentCode, IDictionary<string, object> officer)
        {
            int deptPersKey;

            string name = officer["NAME"].ToString();

            PLCSession.WriteDebug("@EnsureDeptPers: " + departmentCode + ";" + name);
            var qry = new PLCQuery();
            qry.SQL = "SELECT DEPTPERS_KEY FROM TV_DEPTPERS WHERE UPPER(NAME) = ? AND DEPARTMENT_CODE = ?";
            qry.AddSQLParameter("NAME", name.ToUpper());
            qry.AddSQLParameter("DEPARTMENT_CODE", departmentCode);
            qry.OpenReadOnly();
            if (qry.HasData())
            {
                deptPersKey = qry.iFieldByName("DEPTPERS_KEY");
            }
            else
            {
                deptPersKey = PLCSession.GetNextSequence("DEPTPERS_SEQ");

                PLCSession.WriteDebug("@EnsureDeptPers: New " + departmentCode + ";" + name);
                qry.ClearParameters();
                qry.SQL = "SELECT * FROM TV_DEPTPERS WHERE 0 = 1";
                qry.Open();
                qry.Append();
                qry.AddParameter("DEPTPERS_KEY", deptPersKey);
                qry.AddParameter("DEPARTMENT_CODE", departmentCode);
                qry.AddParameter("ACTIVE", "T");
                foreach (var field in officer)
                {
                    string fieldName = field.Key;
                    if (qry.FieldExist(fieldName))
                    {
                        qry.AddParameter(fieldName, field.Value);
                    }
                }
                qry.Save("TV_DEPTPERS", 7000, 1);
            }

            PLCSession.WriteDebug("@EnsureDeptPers: " + deptPersKey);
            return deptPersKey;
        }

        private int EnsureLabCase(string departmentCode, string caseNumber, string caseType, string labCode, string jurisdiction, int? caseOfficerKey = null)
        {
            PLCSession.WriteDebug("@EnsureLabCase: " + departmentCode + ";" + caseNumber);
            int caseKey;
            if (!IsLabCaseExists(departmentCode, caseNumber, out caseKey))
            {
                caseKey = PLCSession.GetNextSequence("LABCASE_SEQ");
                int labCaseYear = DateTime.Now.Year;
                int labCaseNumber = PLCCommon.instance.GetNextLabCaseNumberForNewCase(labCode, labCaseYear);
                string labCase = PLCCommon.instance.FormatCaseNumber(labCode, labCaseYear, labCaseNumber);
                string accessCode = GetCaseAccessCode();

                PLCSession.WriteDebug("@EnsureLabCase: " + labCase);
                PLCQuery qryLabCase = new PLCQuery();
                qryLabCase.SQL = "SELECT * FROM TV_LABCASE WHERE 0 = 1";
                qryLabCase.Open();
                qryLabCase.Append();
                qryLabCase.AddParameter("CASE_KEY", caseKey);
                qryLabCase.AddParameter("LAB_CODE", labCode);
                qryLabCase.AddParameter("LAB_CASE_YEAR", labCaseYear);
                qryLabCase.AddParameter("LAB_CASE_NUMBER", labCaseNumber);
                qryLabCase.AddParameter("LAB_CASE", labCase);
                qryLabCase.AddParameter("DEPARTMENT_CASE_NUMBER", caseNumber);
                qryLabCase.AddParameter("DEPARTMENT_CODE", departmentCode);
                qryLabCase.AddParameter("INVESTIGATING_AGENCY", departmentCode);
                qryLabCase.AddParameter("JURISDICTION_CODE", jurisdiction);
                qryLabCase.AddParameter("CASE_TYPE", caseType);
                qryLabCase.AddParameter("CASE_OFFICER_KEY", caseOfficerKey);
                qryLabCase.AddParameter("CASE_DATE", DateTime.Today);
                qryLabCase.AddParameter("CASE_STATUS", "O");
                qryLabCase.AddParameter("STATUS_DATE", DateTime.Today);
                qryLabCase.AddParameter("ACCESS_RES", accessCode);
                qryLabCase.AddParameter("ENTRY_ANALYST", PLCSession.PLCGlobalAnalyst);
                qryLabCase.AddParameter("ENTRY_TIME_STAMP", DateTime.Now);
                qryLabCase.Save("TV_LABCASE", 5, 6);
            }

            PLCSession.WriteDebug("@EnsureLabCase: " + caseKey);
            return caseKey;
        }

        private static bool IsLabCaseExists(string departmentCode, string departmentCaseNumber, out int caseKey)
        {
            PLCQuery qryLabCase = new PLCQuery();
            qryLabCase.SQL = "SELECT CASE_KEY FROM TV_LABCASE WHERE DEPARTMENT_CODE = ? AND DEPARTMENT_CASE_NUMBER = ?";
            qryLabCase.AddSQLParameter("DEPARTMENT_CODE", departmentCode);
            qryLabCase.AddSQLParameter("DEPARTMENT_CASE_NUMBER", departmentCaseNumber);
            qryLabCase.Open();

            bool hasData = qryLabCase.HasData();
            caseKey = hasData
                ? qryLabCase.iFieldByName("CASE_KEY")
                : 0;

            return hasData;
        }

        private static string GetCaseAccessCode()
        {
            string accessCode = null;

            if (PLCSession.GetLabCtrlFlag("USE_CASE_ACCESS") == "T")
            {
                accessCode = PLCDBGlobal.instance.GetLabCaseAccessValue();
            }

            return !string.IsNullOrEmpty(accessCode)
                ? accessCode
                : null;
        }

        private static Submission CreateLabSub(int caseKey, IDictionary<string, string> submission, string caseDepartmentCode, DateTime dateReceived, DateTime timeReceived)
        {
            PLCSession.WriteDebug("@CreateLabSub: " + caseKey + ";" + caseDepartmentCode);
            int submissionKey = PLCSession.GetNextSequence("LABSUB_SEQ");
            int submissionNumber = PLCDBGlobal.instance.GetNextSubmissionNumber(caseKey);
            string analyst = PLCSession.PLCGlobalAnalyst;

            PLCQuery qryLabSub = new PLCQuery();
            qryLabSub.SQL = "SELECT * FROM TV_LABSUB WHERE 0 = 1";
            qryLabSub.Open();
            qryLabSub.Append();
            qryLabSub.AddParameter("CASE_KEY", caseKey);
            qryLabSub.AddParameter("SUBMISSION_KEY", submissionKey);
            qryLabSub.AddParameter("SUBMISSION_NUMBER", submissionNumber);
            qryLabSub.AddParameter("CASE_DEPARTMENT_CODE", caseDepartmentCode);
            qryLabSub.AddParameter("RECEIVED_BY", analyst);
            qryLabSub.AddParameter("RECEIVED_DATE", dateReceived);
            qryLabSub.AddParameter("RECEIVED_TIME", timeReceived);
            qryLabSub.AddParameter("ENTRY_ANALYST", analyst);
            qryLabSub.AddParameter("ENTRY_TIME_STAMP", DateTime.Now);
            foreach (var field in submission)
            {
                string fieldName = field.Key;
                if (qryLabSub.FieldExist(fieldName))
                {
                    qryLabSub.AddParameter(fieldName, field.Value);
                }
            }
            qryLabSub.Save("TV_LABSUB", 8, 10);

            PLCSession.WriteDebug("@CreateLabSub: " + submissionNumber + "=" + submissionKey);
            return new Submission()
            {
                SubmissionKey = submissionKey,
                SubmissionNumber = submissionNumber
            };
        }

        private static int EnsureCaseDist(int caseKey, string departmentCode, IDictionary<string, object> officer)
        {
            int distKey;

            string deptPersKey = officer["DEPTPERS_KEY"].ToString();

            PLCSession.WriteDebug("@EnsureCaseDist: " + departmentCode + ";" + deptPersKey);
            var qry = new PLCQuery();
            qry.SQL = "SELECT DIST_KEY FROM TV_CASEDIST "
                + "WHERE DEPTPERS_KEY = ? AND DEPARTMENT_CODE = ? AND CASE_KEY = ?";
            qry.AddSQLParameter("DEPTPERS_KEY", deptPersKey);
            qry.AddSQLParameter("DEPARTMENT_CODE", departmentCode);
            qry.AddSQLParameter("CASE_KEY", caseKey);
            qry.OpenReadOnly();
            if (qry.HasData())
            {
                distKey = qry.iFieldByName("DIST_KEY");
            }
            else
            {
                distKey = PLCSession.GetNextSequence("CASEDIST_SEQ");

                PLCSession.WriteDebug("@EnsureCaseDist: New " + departmentCode + ";" + deptPersKey);
                qry.ClearParameters();
                qry.SQL = "SELECT * FROM TV_CASEDIST WHERE 0 = 1";
                qry.Open();
                qry.Append();
                qry.AddParameter("DIST_KEY", distKey);
                qry.AddParameter("CASE_KEY", caseKey);
                qry.AddParameter("DEPARTMENT_CODE", departmentCode);
                qry.AddParameter("ATTENTION", officer["NAME"]);
                foreach (var field in officer)
                {
                    string fieldName = field.Key;
                    if (qry.FieldExist(fieldName))
                    {
                        qry.AddParameter(fieldName, field.Value);
                    }
                }
                qry.Save("TV_CASEDIST", 7000, 1);
            }

            PLCSession.WriteDebug("@EnsureCaseDist: " + distKey);
            return distKey;
        }

        private static int EnsureLabRef(int caseKey, int submissionKey, string referenceType, string reference, int? referencedCaseKey = null)
        {
            PLCSession.WriteDebug("@EnsureLabRef: " + referenceType + "=" + reference);
            int referenceKey;

            var qry = new PLCQuery();
            qry.SQL = "SELECT REFERENCE_KEY FROM TV_LABREF "
                + "WHERE CASE_KEY = ? AND REFERENCE_TYPE = ? AND REFERENCE = ?";
            qry.AddSQLParameter("CASE_KEY", caseKey);
            qry.AddSQLParameter("REFERENCE_TYPE", referenceType);
            qry.AddSQLParameter("REFERENCE", reference);
            qry.OpenReadOnly();
            if (qry.HasData())
            {
                referenceKey = qry.iFieldByName("REFERENCE_KEY");
            }
            else
            {
                referenceKey = PLCSession.GetNextSequence("LABREF_SEQ");

                PLCSession.WriteDebug("@EnsureLabRef: New " + referenceType + "=" + reference);
                qry.ClearParameters();
                qry.SQL = "SELECT * FROM TV_LABREF WHERE 0 = 1";
                qry.Open();
                qry.Append();
                qry.AddParameter("REFERENCE_KEY", referenceKey);
                qry.AddParameter("CASE_KEY", caseKey);
                qry.AddParameter("SUBMISSION_KEY", submissionKey);
                qry.AddParameter("REFERENCE_TYPE", referenceType);
                qry.AddParameter("REFERENCE", reference);
                qry.AddParameter("REFERENCED_CASE_KEY", referencedCaseKey);
                qry.AddParameter("ENTRY_ANALYST", PLCSession.PLCGlobalAnalyst);
                qry.AddParameter("ENTRY_TIME_STAMP", DateTime.Now);
                qry.AddParameter("SYSTEM_GENERATED", "T");
                qry.Save("TV_LABREF", 5, 3);
            }

            PLCSession.WriteDebug("@EnsureLabRef: " + referenceKey);
            return referenceKey;
        }

        private static int EnsureLabName(int caseKey,IDictionary<string, object> name)
        {
            int nameKey;

            string firstName = name["FIRST_NAME"].ToString();
            string middleName = name["MIDDLE_NAME"].ToString();
            string lastName = name["LAST_NAME"].ToString();
            string nameType = name["NAME_TYPE"].ToString();

            PLCSession.WriteDebug("@EnsureLabName: " + nameType + "=" + firstName + ";" + middleName + ";" + lastName);
            var qry = new PLCQuery();
            qry.SQL = "SELECT NAME_KEY FROM TV_LABNAME "
                + "WHERE FIRST_NAME_SEARCH = ? AND MIDDLE_NAME_SEARCH = ? AND LAST_NAME_SEARCH = ? "
                + "AND NAME_TYPE = ? AND CASE_KEY = ?";
            qry.AddSQLParameter("FIRST_NAME_SEARCH", firstName.ToUpper());
            qry.AddSQLParameter("MIDDLE_NAME_SEARCH", middleName.ToUpper());
            qry.AddSQLParameter("LAST_NAME_SEARCH", lastName.ToUpper());
            qry.AddSQLParameter("NAME_TYPE", nameType);
            qry.AddSQLParameter("CASE_KEY", caseKey);
            qry.OpenReadOnly();
            if (qry.HasData())
            {
                nameKey = qry.iFieldByName("NAME_KEY");
            }
            else
            {
                nameKey = PLCSession.GetNextSequence("LABNAME_SEQ");
                int nameNumber = GetNextNameNumber(caseKey);

                PLCSession.WriteDebug("@EnsureLabName: New " + nameNumber);
                qry.ClearParameters();
                qry.SQL = "SELECT * FROM TV_LABNAME WHERE 0 = 1";
                qry.Open();
                qry.Append();
                qry.AddParameter("CASE_KEY", caseKey);
                qry.AddParameter("NAME_KEY", nameKey);
                qry.AddParameter("NUMBER_RES", nameNumber);
                qry.AddParameter("ENTRY_ANALYST", PLCSession.PLCGlobalAnalyst);
                qry.AddParameter("ENTRY_TIME_STAMP", DateTime.Now);
                foreach (var field in name)
                {
                    string fieldName = field.Key;
                    if (qry.FieldExist(fieldName))
                    {
                        string fieldValue = field.Value.ToString();
                        qry.AddParameter(fieldName, fieldValue);

                        if (fieldName == "FIRST_NAME"
                            || fieldName == "MIDDLE_NAME"
                            || fieldName == "LAST_NAME")
                        {
                            qry.AddParameter(fieldName + "_SEARCH", fieldValue.ToUpper());
                        }
                    }
                }
                qry.Save("TV_LABNAME", 6, 10);
            }

            PLCSession.WriteDebug("@EnsureLabName: " + nameKey);
            return nameKey;
        }

        private static int GetNextNameNumber(int caseKey)
        {
            var qry = new PLCQuery();
            qry.SQL = "SELECT MAX(NUMBER_RES) + 1 NEXTNUMBER FROM TV_LABNAME WHERE CASE_KEY = " + caseKey;
            qry.OpenReadOnly();
            return qry.FieldByName("NEXTNUMBER") != ""
                ? qry.iFieldByName("NEXTNUMBER")
                : 1;
        }

        private static Item EnsureLabItem(int caseKey, int submissionNumber, string departmentItemNumber, IDictionary<string, object> data, DateTime dateReceived, DateTime timeReceived)
        {
            PLCSession.WriteDebug("@EnsureLabItem: " + departmentItemNumber);
            int ecn;
            string labItemNumber;
            string itemType = data["ITEM_TYPE"].ToString();
            string itemDescription = data["ITEM_DESCRIPTION"].ToString();
            bool isNew = true;

            var qry = new PLCQuery();
            qry.SQL = "SELECT EVIDENCE_CONTROL_NUMBER, LAB_ITEM_NUMBER, ITEM_TYPE, ITEM_DESCRIPTION "
                + "FROM TV_LABITEM "
                + "WHERE DEPARTMENT_ITEM_NUMBER = ? AND CASE_KEY = ?";
            qry.AddSQLParameter("DEPARTMENT_ITEM_NUMBER", departmentItemNumber);
            qry.AddSQLParameter("CASE_KEY", caseKey);
            qry.OpenReadOnly();
            if (qry.HasData())
            {
                ecn = qry.iFieldByName("EVIDENCE_CONTROL_NUMBER");
                labItemNumber = qry.FieldByName("LAB_ITEM_NUMBER");
                itemType = qry.FieldByName("ITEM_TYPE");
                itemDescription = qry.FieldByName("ITEM_DESCRIPTION");
                isNew = false;
            }
            else
            {
                ecn = PLCSession.GetNextSequence("LABITEM_SEQ");
                labItemNumber = PLCDBGlobal.instance.GetNextItemNumber();
                string itemSort = PLCCommon.instance.GetItemSort(labItemNumber);
                string deptItemSort = PLCSession.GetLabCtrlFlag("DUPE_DEPT_ITEM_NUM_ON_SAMPLE") == "T"
                    ? PLCCommon.instance.GetItemSort(departmentItemNumber)
                    : null;
                string analyst = PLCSession.PLCGlobalAnalyst;

                string[] itemFields = { "ITEM_TYPE", "ITEM_DESCRIPTION", "ITEM_TEXT" };

                PLCSession.WriteDebug("@EnsureLabItem: " + labItemNumber);
                qry.ClearParameters();
                qry.SQL = "SELECT * FROM TV_LABITEM WHERE 0 = 1";
                qry.Open();
                qry.Append();
                qry.AddParameter("CASE_KEY", caseKey);
                qry.AddParameter("EVIDENCE_CONTROL_NUMBER", ecn);
                qry.AddParameter("LAB_CASE_SUBMISSION", submissionNumber);
                qry.AddParameter("LAB_ITEM_NUMBER", labItemNumber);
                qry.AddParameter("ITEM_SORT", itemSort);
                qry.AddParameter("DEPARTMENT_ITEM_NUMBER", departmentItemNumber);
                qry.AddParameter("DEPT_ITEM_SORT", deptItemSort);
                qry.AddParameter("BOOKED_BY", analyst);
                qry.AddParameter("BOOKING_DATE", dateReceived);
                qry.AddParameter("BOOKING_TIME", timeReceived);
                // CUSTODY_OF and LOCATION is set on add of custody
                qry.AddParameter("CUSTODY_DATE", dateReceived.Add(timeReceived.TimeOfDay));
                qry.AddParameter("ENTRY_ANALYST", analyst);
                qry.AddParameter("ENTRY_TIME_STAMP", DateTime.Now);
                foreach (var fieldName in itemFields)
                {
                    if (qry.FieldExist(fieldName) && data.ContainsKey(fieldName))
                    {
                        qry.AddParameter(fieldName, data[fieldName].ToString());
                    }
                }
                qry.Save("TV_LABITEM", 7, 10);
            }

            PLCSession.WriteDebug("@EnsureLabItem: " + ecn);
            return new Item()
            {
                ECN = ecn,
                LabItemNumber = labItemNumber,
                ItemType = itemType,
                ItemDescription = itemDescription,
                IsNew = isNew
            };
        }

        private static void AddItemCustody(int caseKey, int ecn, string itemType, bool isNewItem, int submissionKey, string submissionType, int submissionNumber, string trackingNumber, string receivedBy)
        {
            PLCSession.WriteDebug("@AddItemCustody: " + ecn + ";isResub=" + !isNewItem);
            bool isAnalystCustodyAlreadyExists = false;
            if (!string.IsNullOrWhiteSpace(PLCSession.GetLabCtrl("SUBMISSION_CUSTODY_TYPE")))
            {
                //save custody records linked to the submission
                PLCDBGlobal.instance.CreateCustodyRecordsForSubmissionItem(
                    caseKey: caseKey.ToString(),
                    submissionKey: submissionKey.ToString(),
                    submissionType: submissionType,
                    submissionTrackingNumber: trackingNumber,
                    submissionComments: string.Empty,
                    ecn: ecn.ToString(),
                    itemType: itemType,
                    containerKey: string.Empty,
                    labSubReceivedBy: receivedBy);

                isAnalystCustodyAlreadyExists = true;
            }
            PLCSession.WriteDebug("@AddItemCustody: isAnalystCustodyAlreadyExists=" + isAnalystCustodyAlreadyExists);

            string floorCustody = string.Empty;
            string floorLocation = string.Empty;
            if (GetFloorCustodyLocation(out floorCustody, out floorLocation))
            {
                PLCSession.WriteDebug("@AddItemCustody: Floor Custody=" + floorCustody + ";Location=" + floorLocation);
                PLCSession.UpdateItemCustody(ecn.ToString(), floorCustody, floorLocation);

                // Add new custody record for this labitem.
                PLCSession.AddCustody(
                    caseKey: caseKey.ToString(),
                    ecn: ecn.ToString(),
                    custodyCode: floorCustody,
                    custodyLocation: floorLocation,
                    batchKey: string.Empty,
                    containerKey: string.Empty,
                    submissionKey: submissionKey.ToString(),
                    trackingNumber: trackingNumber,
                    comments: string.Empty,
                    parentECN: string.Empty,
                    weight: string.Empty,
                    weightUnit: string.Empty,
                    floorCustody: true,
                    itemsTab: false);
            }
            else
            {
                PLCSession.WriteDebug("@AddItemCustody: Default custody");
                //save default custody for this item
                PLCSession.AddDefaultCustody(
                    caseKey: caseKey.ToString(),
                    ecn: ecn.ToString(),
                    batchKey: string.Empty,
                    containerKey: string.Empty,
                    submissionNumber: submissionNumber,
                    itemType: itemType,
                    weight: string.Empty,
                    weightUnit: string.Empty,
                    analystCustodyAlreadyExists: isAnalystCustodyAlreadyExists,
                    updateContainerCustody: !isNewItem);
            }

            if (isNewItem)
            {
                // Add intake custody for this item.                    
                //AddIntakeCustody(caseKey.ToString(), ecn.ToString(), null, contKey, defaultIntakeLocation, weight, weightUnit);
            }
            PLCSession.WriteDebug("@AddItemCustody: Done");
        }

        private static bool GetFloorCustodyLocation(out string floorCustody, out string floorLocation)
        {
            PLCDBGlobal.instance.GetFloorCustodyLocation(out floorCustody, out floorLocation);

            return !string.IsNullOrEmpty(floorCustody)
                && !string.IsNullOrEmpty(floorLocation);
        }

        private static int EnsureItemNameLink(int ecn, int nameKey, string relationCode)
        {
            PLCSession.WriteDebug("@EnsureItemNameLink: " + ecn + ";" + nameKey + ";" + relationCode);
            int linkKey;

            var qry = new PLCQuery();
            qry.SQL = "SELECT LINK_KEY FROM TV_ITEMNAME WHERE EVIDENCE_CONTROL_NUMBER = ? AND NAME_KEY = ?";
            qry.AddSQLParameter("EVIDENCE_CONTROL_NUMBER", ecn);
            qry.AddSQLParameter("NAME_KEY", nameKey);
            qry.OpenReadOnly();
            if (qry.HasData())
            {
                linkKey = qry.iFieldByName("LINK_KEY");
            }
            else
            {
                linkKey = PLCSession.GetNextSequence("ITEMNAME_SEQ");

                PLCSession.WriteDebug("@EnsureItemNameLink: New " + ecn + ";" + nameKey + ";" + relationCode);
                qry.ClearParameters();
                qry = new PLCQuery();
                qry.SQL = "SELECT * FROM TV_ITEMNAME WHERE 0 = 1";
                qry.Open();
                qry.Append();
                qry.AddParameter("LINK_KEY", linkKey);
                qry.AddParameter("EVIDENCE_CONTROL_NUMBER", ecn);
                qry.AddParameter("NAME_KEY", nameKey);
                qry.AddParameter("RELATION_CODE", relationCode);
                qry.Save("TV_ITEMNAME", 7, 10);
            }

            PLCSession.WriteDebug("@EnsureItemNameLink: " + linkKey);
            return linkKey;
        }

        private static int CreateLabAssign(int caseKey, string section, string labCode, string taskType = "")
        {
            PLCSession.WriteDebug("@CreateLabAssign: " + caseKey + ";" + section + ";" + labCode);
            int examKey = PLCSession.GetNextSequence("LABEXAM_SEQ");
            int sequence = PLCDBGlobal.instance.GetNextAssignmentSequence(caseKey.ToString());
            string priority = PLCCommon.instance.LabAssignPriority();
            string sectionNumber = PLCSession.GetLabCtrlFlag("AUTO_GEN_SECTION_NUM") == "S"
                ? (new PLCHelperFunctions()).GetNextSectionNumber(section, caseKey.ToString())
                : null;
            string analystAssigned = GetAssignmentDefaultAnalyst(section, taskType);
            bool hasAnalystAssigned = !string.IsNullOrEmpty(analystAssigned);
            DateTime? analystDate = hasAnalystAssigned ? DateTime.Today : (DateTime?)null;
            string status = hasAnalystAssigned ? PLCDBGlobal.instance.GetExamStatusCode("L","1") : PLCDBGlobal.instance.GetExamStatusCode("S", "0");
            DateTime? submissionDate = PLCSession.GetLabCtrlFlag("SET_LABASSIGN_SUBMISSION_DATE") == "T"
                ? DateTime.Today
                : (DateTime?)null;
            string analyst = PLCSession.PLCGlobalAnalyst;

            var qry = new PLCQuery("SELECT * FROM TV_LABASSIGN WHERE 0 = 1");
            qry.Open();
            qry.Append();
            qry.AddParameter("CASE_KEY", caseKey);
            qry.AddParameter("EXAM_KEY", examKey);
            qry.AddParameter("SEQUENCE", sequence);
            qry.AddParameter("SECTION", section);
            qry.AddParameter("SECTION_NUMBER", sectionNumber);
            qry.AddParameter("RECEIVED_IN_SECTION", DateTime.Today);
            qry.AddParameter("ANALYST_ASSIGNED", analystAssigned);
            qry.AddParameter("ANALYST_DATE", analystDate);
            qry.AddParameter("ASSIGNED_BY", analyst);
            qry.AddParameter("DATE_ASSIGNED", DateTime.Today);
            qry.AddParameter("STATUS", status);
            qry.AddParameter("LAB_CODE", labCode);
            qry.AddParameter("PRIORITY", priority);
            qry.AddParameter("SUBMISSION_DATE", submissionDate);
            qry.AddParameter("REPORT_NUMBER", 0);
            qry.AddParameter("APPROVED", "N");
            qry.AddParameter("COMPLETED", "F");
            qry.AddParameter("ENTRY_ANALYST", analyst);
            qry.AddParameter("ENTRY_TIME_STAMP", DateTime.Now);
            qry.Save("TV_LABASSIGN", 17, 10);

            PLCSession.WriteDebug("@CreateLabAssign: " + examKey);
            return examKey;
        }

        private static string GetAssignmentDefaultAnalyst(string section, string taskType)
        {
            PLCSession.WriteDebug("@GetAssignmentDefaultAnalyst: " + section + ";" + taskType);
            string analystAssigned = null;

            // teammbr.anal
            if (!string.IsNullOrEmpty(taskType))
            {
                analystAssigned = PLCHelperFunctions.GetAnalystAssignedByTaskType(taskType);
            }

            // labassign.analyst_assigned
            if (string.IsNullOrEmpty(analystAssigned))
            {
                string autoAssign = PLCSession.GetSectionFlag(section, "AUTO_ASSIGN_ADDTNL_SUBS").Trim().ToUpper();
                if (autoAssign == "T")
                    analystAssigned = PLCDBGlobal.instance.GetDefaultAnalyst(section);
                else if (autoAssign == "S")
                    analystAssigned = PLCDBGlobal.instance.FirstReportAnalyst(section, PLCSession.PLCGlobalCaseKey);
            }

            PLCSession.WriteDebug("@GetAssignmentDefaultAnalyst: " + analystAssigned);
            return !string.IsNullOrEmpty(analystAssigned)
                ? analystAssigned
                : null;
        }

        private static void AddAssignmentNames(int caseKey, int examKey, int ecn)
        {
            PLCSession.WriteDebug("@AddAssignmentNames: " + examKey + ";" + ecn);
            if (PLCSession.GetLabCtrlFlag("CHECK_ASSIGNMENT_NAME_ITEM") == "T")
            {
                PLCDBGlobal.instance.AddNamesToAssignment(examKey, ecn);
            }
            else
            {
                PLCQuery qryNames = new PLCQuery();
                qryNames.SQL = "SELECT NAME_KEY FROM TV_LABNAME "
                    + "WHERE CASE_KEY = ? "
                    + "AND NAME_KEY NOT IN (SELECT NAME_KEY FROM TV_REPTNAME WHERE EXAM_KEY = ?)";
                qryNames.AddSQLParameter("CASE_KEY", caseKey);
                qryNames.AddSQLParameter("EXAM_KEY", examKey);
                qryNames.Open();
                while (!qryNames.EOF())
                {
                    string nameKey = qryNames.FieldByName("NAME_KEY");

                    PLCQuery qryNameAssignment = new PLCQuery("SELECT * FROM TV_REPTNAME WHERE 0 = 1");
                    qryNameAssignment.Open();
                    qryNameAssignment.Append();
                    qryNameAssignment.SetFieldValue("EXAM_KEY", examKey);
                    qryNameAssignment.SetFieldValue("NAME_KEY", nameKey);
                    qryNameAssignment.Post("TV_REPTNAME", 37, 2);

                    qryNames.Next();
                }
            }
            PLCSession.WriteDebug("@AddAssignmentNames: Done");
        }

        private static int CreateTaskList(int caseKey, int examKey, int ecn, int taskNumber, string taskType, string description, string section, string itemType, string analystAssigned)
        {
            PLCSession.WriteDebug("@CreateTaskList: " + examKey + ";" + ecn + ";" + taskNumber + ";" + taskType);
            int taskId = PLCSession.GetNextSequence("TASKLIST_SEQ");
            string taskStatus = !string.IsNullOrEmpty(analystAssigned) ? "A" : "O";
            string analyst = PLCSession.PLCGlobalAnalyst;

            PLCQuery qry = new PLCQuery();
            qry.SQL = "SELECT * FROM TV_TASKLIST WHERE 0 = 1";
            qry.Open();
            qry.Append();
            qry.AddParameter("CASE_KEY", caseKey);
            qry.AddParameter("TASK_ID", taskId);
            qry.AddParameter("TASK_NUM", taskNumber);
            qry.AddParameter("TASK_TYPE", taskType);
            qry.AddParameter("DESCRIPTION", description);
            qry.AddParameter("ANALYST", analystAssigned);
            qry.AddParameter("DATE_ASSIGNED", DateTime.Now);
            qry.AddParameter("STATUS", taskStatus);
            qry.AddParameter("EXAM_KEY", examKey);
            qry.AddParameter("SECTION", section);
            qry.AddParameter("EVIDENCE_CONTROL_NUMBER", ecn);
            qry.AddParameter("ITEM_TYPE", itemType);
            qry.AddParameter("REQUESTED_BY", analyst);
            qry.AddParameter("REQUESTED_DATE", DateTime.Now);
            qry.AddParameter("ENTRY_ANALYST", analyst);
            qry.Save("TV_TASKLIST", 52, 3);

            PLCSession.WriteDebug("@CreateTaskList: " + taskNumber + "=" + taskId);
            return taskId;
        }

        private static int CreateAdditionalTaskList(int caseKey, int examKey, int ecn, int currentTaskNumber, string taskType, string description, string section, string itemType)
        {
            PLCSession.WriteDebug("@CreateAdditionalTaskList: " + examKey + ";" + ecn + ";" + currentTaskNumber + ";" + taskType);
            string taskStatus = "O";
            string taskPriority = string.Empty;

            if (PLCSession.GetLabCtrlFlag("USES_AUTO_TASKS") == "T")
            {
                currentTaskNumber = PLCDBGlobal.instance.AutoCreateItemTask(examKey, ecn, currentTaskNumber, taskStatus);
            }
            else
            {
                currentTaskNumber = PLCDBGlobal.instance.CreateAdditionalTask(
                    taskType,
                    itemType,
                    description,
                    taskStatus,
                    section,
                    ecn,
                    examKey,
                    currentTaskNumber,
                    caseKey.ToString(),
                    itemSource: string.Empty);
            }

            if (PLCSession.CheckTaskTypeFlag(taskType, "PANEL_TASK"))
            {
                string labCode = PLCSession.PLCGlobalLabCode;
                currentTaskNumber = AddPanelTaskList(
                    caseKey,
                    examKey,
                    ecn,
                    currentTaskNumber,
                    taskType,
                    itemType,
                    labCode,
                    taskStatus,
                    taskPriority: string.Empty,
                    itemSource: string.Empty);
            }

            PLCSession.WriteDebug("@CreateAdditionalTaskList: " + currentTaskNumber);
            return currentTaskNumber;
        }

        private static int AddPanelTaskList(int caseKey, int examKey, int ecn, int currentTaskNumber, string taskType, string itemType, string labCode, string taskStatus, string taskPriority, string itemSource)
        {
            PLCSession.WriteDebug("@AddPanelTaskList: " + examKey + ";" + ecn + ";" + currentTaskNumber + ";" + taskType);
            PLCQuery qry = new PLCQuery();
            qry.SQL = "SELECT P.TASK_TYPE, T.SECTION "
                + "FROM TV_PANLTASK P "
                + "INNER JOIN TV_TASKTYPE T ON P.TASK_TYPE = T.TASK_TYPE "
                + "WHERE PANEL_TYPE = ? "
                + "AND (ITEM_TYPE = ? OR ITEM_TYPE = '*') "
                + "AND (LAB_CODE = ? OR LAB_CODE = '*') "
                + "ORDER BY ORDER_RES ASC";
            qry.AddSQLParameter("PANEL_TYPE", taskType);
            qry.AddSQLParameter("ITEM_TYPE", itemType);
            qry.AddSQLParameter("LAB_CODE", labCode);
            qry.OpenReadOnly();
            if (qry.HasData())
            {
                while (!qry.EOF())
                {
                    currentTaskNumber++;
                    string panelTaskType = qry.FieldByName("TASK_TYPE");
                    string panelTaskTypeDescription = PLCSession.GetCodeDesc("TASKTYPE", panelTaskType);
                    string panelTaskSection = qry.FieldByName("SECTION");

                    PLCDBGlobal.instance.CreateTask(
                        panelTaskType,
                        itemType,
                        panelTaskTypeDescription,
                        taskStatus,
                        panelTaskSection,
                        ecn,
                        currentTaskNumber,
                        taskPriority,
                        examKey,
                        caseKey.ToString(),
                        itemSource);

                    qry.Next();
                }
            }

            PLCSession.WriteDebug("@AddPanelTaskList: " + currentTaskNumber);
            return currentTaskNumber;
        }

        private static int CreateWorkList(string taskType)
        {
            PLCSession.WriteDebug("@CreateWorkList: " + taskType);
            int workListId = PLCSession.GetNextSequence("WORKLIST_SEQ");
            string importMethod;
            string analysisMethod;
            string templateCode = GetWorkListMethods(taskType, out importMethod, out analysisMethod);

            var qryWorklist = new PLCQuery();
            qryWorklist.SQL = "SELECT * FROM TV_WORKLIST WHERE 0 = 1";
            qryWorklist.Open();
            qryWorklist.Append();
            qryWorklist.AddParameter("WORKLIST_ID", workListId);
            qryWorklist.AddParameter("ANALYST", PLCSession.PLCGlobalAnalyst);
            qryWorklist.AddParameter("TASK_TYPE", taskType);
            qryWorklist.AddParameter("WORKLIST_DATE", DateTime.Today);
            qryWorklist.AddParameter("TEMPLATE_CODE", templateCode);
            qryWorklist.AddParameter("IMPORT_METHOD", importMethod);
            qryWorklist.AddParameter("ANALYSIS_METHOD", analysisMethod);
            qryWorklist.Save("TV_WORKLIST", 32, 2);

            PLCSession.WriteDebug("@CreateWorkList: " + workListId);
            return workListId;
        }

        private static string GetWorkListMethods(string taskType, out string importMethod, out string analysisMethod)
        {
            string templateCode = null;

            PLCDBGlobal.instance.GetWorklistMethods(taskType, out templateCode, out importMethod, out analysisMethod);

            if (string.IsNullOrEmpty(templateCode))
            {
                templateCode = null;
                importMethod = null;
                analysisMethod = null;
            }

            return templateCode;
        }

        private static void AddWorkListTask(int workListId, int taskId, int sequence)
        {
            PLCSession.WriteDebug("@AddWorkListTask: " + workListId + ";" + taskId);
            PLCQuery qry = new PLCQuery();
            qry.SQL = "SELECT * FROM TV_WORKTASK WHERE 0 = 1";
            qry.Open();
            qry.Append();
            qry.AddParameter("WORKLIST_ID", workListId);
            qry.AddParameter("SEQUENCE", sequence);
            qry.AddParameter("TASK_ID", taskId);
            qry.Save("TV_WORKTASK", 52, 4);
            PLCSession.WriteDebug("@AddWorkListTask: " + sequence);
        }
        #endregion Methods
    }
}
