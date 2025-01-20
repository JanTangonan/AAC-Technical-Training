using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Configuration;
using System.Web.Script.Serialization;
using PLCGlobals;
using PLCCONTROLS;
using System.Globalization;
using System.Data.OleDb;
using System.Data.OracleClient;

namespace BEASTiLIMS
{
    /// <summary>
    /// Summary description for Exercise_Arvinn_AjaxWS
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class Exercise_Arvinn_AjaxWS : System.Web.Services.WebService
    {
        #region Models
        public class CaseData
        {
            public string CASE_KEY { get; set; }
            public string DEPARTMENT_CODE { get; set; }
            public string OFFENSE_CODE { get; set; }
            public string DEPARTMENT_CASE_NUMBER { get; set; }
            public string DEPARTMENT_NAME { get; set; }
            public string CHARGE { get; set; }
            public string LAB_CASE { get; set; }
            public string OFFENSE_DATE { get; set; }
        }

        public class SelectedRowData
        {
            public string CASE_KEY { get; set; }
            public string DEPARTMENT_CASE_NUMBER { get; set; }
            public string DEPARTMENT_CODE { get; set; }
            public string OFFENSE_CODE { get; set; }
            public string LAB_CASE { get; set; }
            public string OFFENSE_DATE { get; set; }
        }

        public class Department
        {
            public string DEPARTMENT_CODE { get; set; }
            public string DEPARTMENT_NAME { get; set; }
        }

        public class Charge
        {
            public string OFFENSE_CODE { get; set; }
            public string OFFENSE_DESCRIPTION { get; set; }
        }

        public class FieldData
        {
            public string FieldName { get; set; }
            public string Value { get; set; }
        }

        #endregion

        #region Endpoints
        /// <summary>
        /// Gets top 10 Cases based on recent OFFENSE_DATE
        /// </summary>
        /// <returns>Serialized Case List - list of case data</returns>
        [WebMethod(EnableSession = true)]
        public string GetTopCases()
        {
            PLCQuery qry = new PLCQuery();
            qry.SQL = @"SELECT TOP 10 C.CASE_KEY, C.DEPARTMENT_CASE_NUMBER, 
                        D.DEPARTMENT_NAME, D.DEPARTMENT_CODE, 
                        O.OFFENSE_DESCRIPTION AS CHARGE, O.OFFENSE_CODE,
                        C.LAB_CASE, C.OFFENSE_DATE
                        FROM TV_LABCASE C 
                        INNER JOIN TV_DEPTNAME D ON C.DEPARTMENT_CODE = D.DEPARTMENT_CODE 
                        INNER JOIN TV_OFFENSE O ON C.OFFENSE_CODE = O.OFFENSE_CODE 
                        ORDER BY C.OFFENSE_DATE DESC";

            List<CaseData> caseList = new List<CaseData>();

            qry.Open();
            if (qry.HasData())
            {
                while (!qry.EOF())
                {
                    caseList.Add(new CaseData
                    {
                        CASE_KEY = qry.FieldByName("CASE_KEY"),
                        DEPARTMENT_CODE = qry.FieldByName("DEPARTMENT_CODE"),
                        OFFENSE_CODE = qry.FieldByName("OFFENSE_CODE"),
                        DEPARTMENT_CASE_NUMBER = qry.FieldByName("DEPARTMENT_CASE_NUMBER"),
                        DEPARTMENT_NAME = qry.FieldByName("DEPARTMENT_NAME"),
                        CHARGE = qry.FieldByName("CHARGE"),
                        LAB_CASE = qry.FieldByName("LAB_CASE"),
                        OFFENSE_DATE = qry.FieldByName("OFFENSE_DATE")
                    });
                    qry.Next();
                }
            }

            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(caseList);
        }

        /// <summary>
        /// Gets top 10 Cases based on recent OFFENSE_DATE
        /// </summary>
        /// <returns>Serialized Case List - list of case data</returns>
        [WebMethod(EnableSession = true)]
        public string GetSelectedRowData(string CASE_KEY)
        {
            PLCQuery qry = new PLCQuery();
            qry.SQL = @"SELECT CASE_KEY,
                           DEPARTMENT_CASE_NUMBER, 
                           DEPARTMENT_CODE, 
                           OFFENSE_CODE, 
                           LAB_CASE, 
                           OFFENSE_DATE
                           FROM TV_LABCASE WHERE CASE_KEY = " + CASE_KEY;
        
            SelectedRowData selectedRowData = null;

            qry.Open();
            if (qry.HasData())
            {
                selectedRowData = new SelectedRowData
                {
                    CASE_KEY = qry.FieldByName("CASE_KEY"),
                    DEPARTMENT_CASE_NUMBER = qry.FieldByName("DEPARTMENT_CASE_NUMBER"),
                    DEPARTMENT_CODE = qry.FieldByName("DEPARTMENT_CODE"),
                    OFFENSE_CODE = qry.FieldByName("OFFENSE_CODE"),
                    LAB_CASE = qry.FieldByName("LAB_CASE"),
                    OFFENSE_DATE = qry.FieldByName("OFFENSE_DATE")
                };
            }

            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(selectedRowData);

        }

        /// <summary>
        /// Gets Department details - DEPARTMENT_CODE and DEPARTMENT_NAME
        /// </summary>
        /// <returns>Serialized departments - list of department details</returns>
        [WebMethod(EnableSession = true)]
        public string GetDepartments()
        {
            PLCQuery qry = new PLCQuery();
            qry.SQL = "SELECT DEPARTMENT_CODE, DEPARTMENT_NAME FROM TV_DEPTNAME";
            List<Department> departments = new List<Department>();
            qry.Open();
            if (qry.HasData())
            {
                while (!qry.EOF())
                {
                    departments.Add(new Department
                    {
                        DEPARTMENT_CODE = qry.FieldByName("DEPARTMENT_CODE"),
                        DEPARTMENT_NAME = qry.FieldByName("DEPARTMENT_NAME")
                    });
                    qry.Next();
                }
            }

            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(departments);
        }

        /// <summary>
        /// Gets Charge details - OFFENSE_CODE and OFFENSE_DESCRIPTION
        /// </summary>
        /// <returns>Serialized charges - list of charge details</returns>
        [WebMethod(EnableSession = true)]
        public string GetCharges()
        {
            PLCQuery qry = new PLCQuery();
            qry.SQL = "SELECT OFFENSE_CODE, OFFENSE_DESCRIPTION FROM TV_OFFENSE";
            List<Charge> charges = new List<Charge>();
            qry.Open();
            if (qry.HasData())
            {
                while (!qry.EOF())
                {
                    charges.Add(new Charge
                    {
                        OFFENSE_CODE = qry.FieldByName("OFFENSE_CODE"),
                        OFFENSE_DESCRIPTION = qry.FieldByName("OFFENSE_DESCRIPTION")
                    });
                    qry.Next();
                }
            }

            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(charges);
        }

        /// <summary>
        /// Saves input data from client-side to db
        /// </summary>
        /// <param name="fieldDataList"></param>
        /// <returns>result string</returns>
        [WebMethod(EnableSession = true)]
        public string SaveData(List<FieldData> fieldDataList)
        {
            try
            {
                string caseKey = fieldDataList.FirstOrDefault(field => field.FieldName == "CASE_KEY")?.Value;

                if (string.IsNullOrEmpty(caseKey))
                {
                    return $"CASE_KEY is missing or invalid";
                }

                PLCQuery qry = new PLCQuery();
                qry.SQL = "SELECT * FROM TV_LABCASE WHERE CASE_KEY = " + caseKey;
                qry.Open();
                if (qry.HasData())
                {
                    qry.Edit();
                    foreach (var fieldData in fieldDataList)
                    {
                        qry.SetFieldValue(fieldData.FieldName, fieldData.Value);
                    }
                    qry.Post("TV_LABCASE");
                }                

                return $"Save sucessful";
            }
            catch (Exception ex)
            {
                return $"Error saving data: {ex.StackTrace}";
            }
        }

        /// <summary>
        /// Calls ReverserCsvOrder stored proc then reverses the order of csvInput
        /// </summary>
        /// <param name="csvInput"></param>
        /// <returns>csvOutput</returns>
        [WebMethod(EnableSession = true)]
        public string GetReversedCsv(string csvInput)
        {
            string csvOutput = string.Empty;
            if (PLCSession.PLCDatabaseServer == "MSSQL")
            {
                PLCQuery qry = new PLCQuery();
                qry.AddProcedureParameter("CsvInput", csvInput, 8000, OleDbType.VarChar, ParameterDirection.Input);
                qry.AddProcedureParameter("CsvOutput", DBNull.Value, 8000, OleDbType.VarChar, ParameterDirection.Output);
                Dictionary<string, object> result = qry.ExecuteProcedure("ReverseCsvOrder");

                csvOutput = Convert.ToString(result["CsvOutput"]);
            }
            else
            {
                PLCQuery qry = new PLCQuery();
                List<OracleParameter> parameters = new List<OracleParameter>();
                parameters.Add(new OracleParameter("CsvInput", csvInput));
                parameters.Add(new OracleParameter("CsvOutput", DBNull.Value));

                Dictionary<string, object> result = qry.ExecuteOracleProcedure("ReverseCsvOrder", parameters);

                csvOutput = Convert.ToString(result["CsvOutput"]);
            }
            
            return csvOutput;
        }

        #endregion
    }
}