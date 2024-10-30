using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Configuration;
using System.Web.Script.Serialization;
using PLCGlobals;
using PLCCONTROLS;

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
        public class CaseData
        {
            public string CASE_KEY { get; set; }
            public string DEPARTMENT_CASE_NUMBER { get; set; }
            public string DEPARTMENT_NAME { get; set; }
            public string CHARGE { get; set; }
            public string LAB_CASE { get; set; }
            public string OFFENSE_DATE { get; set; }
        }

        private string GetConnectionString()
        {
            string serverName = "LOCALHOST";
            string databaseName = "VADFS";
            string userId = "vadfs";
            string password = "vadfs";

            string connectionString = $"Server={serverName};Database={databaseName};User Id={userId};Password={password};";

            return connectionString;
        }

        [WebMethod(EnableSession = true)]
        public string GetTopCases()
        {
            PLCQuery qry = new PLCQuery();
            qry.SQL = @"SELECT TOP 10 C.CASE_KEY, C.DEPARTMENT_CASE_NUMBER, 
                                    D.DEPARTMENT_NAME, O.OFFENSE_DESCRIPTION AS CHARGE, C.LAB_CASE, 
                                    C.OFFENSE_DATE 
                                    FROM TV_LABCASE C 
                                    INNER JOIN TV_DEPTNAME D ON C.DEPARTMENT_CODE = D.DEPARTMENT_CODE 
                                    INNER JOIN TV_OFFENSE O ON C.OFFENSE_CODE = O.OFFENSE_CODE 
                                    ORDER BY C.OFFENSE_DATE DESC";
            List<CaseData> caseList = new List<CaseData>();

            qry.Open();
            while (!qry.EOF())
            {
                caseList.Add(new CaseData
                {
                    CASE_KEY = qry.FieldByName("CASE_KEY"),
                    DEPARTMENT_CASE_NUMBER = qry.FieldByName("DEPARTMENT_CASE_NUMBER"),
                    DEPARTMENT_NAME = qry.FieldByName("DEPARTMENT_NAME"),
                    CHARGE = qry.FieldByName("CHARGE"),
                    LAB_CASE = qry.FieldByName("LAB_CASE"),
                    OFFENSE_DATE = qry.FieldByName("OFFENSE_DATE")
                });
                qry.Next();
            }

            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(caseList);
        }

        [WebMethod(EnableSession = true)]
        public string SaveData(string CASE_KEY, string CHARGE, string DEPARTMENT_CASE_NUMBER, string DEPARTMENT_NAME, string LAB_CASE, string OFFENSE_DATE)
        {

            PLCQuery qry = new PLCQuery();
            qry.SQL = @"UPDATE TV_LABCASE
                        SET 
                        DEPARTMENT_CASE_NUMBER,
                        DEPARTMENT_CODE,
                        OFFENSE_CODE,
                        LAB_CASE,
                        OFFENSE_DATE
                        WHERE CASE_KEY";
            try
            {
                qry.Open();
                while(!qry.EOF())
                {
                    qry.SetFieldValue("CASE_KEY", CASE_KEY);
                    qry.SetFieldValue("DEPARTMENT_CASE_NUMBER", DEPARTMENT_CASE_NUMBER);
                    qry.SetFieldValue("DEPARTMENT_CODE", DEPARTMENT_NAME);
                    qry.SetFieldValue("OFFENSE_CODE", CHARGE);
                    qry.SetFieldValue("LAB_CASE", LAB_CASE);
                    qry.SetFieldValue("OFFENSE_DATE", OFFENSE_DATE);

                    qry.Post("TV_LABCASE");

                    qry.Next();
                }
                return $"Save sucessful";
            }
            catch (Exception ex)
            {
                // Log the exception and return an error message
                return $"Error saving data: {ex.StackTrace}";
            }
        }
    }
}
