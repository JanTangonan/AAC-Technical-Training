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

        [WebMethod]
        public string GetTopCases()
        {
            string connectionString = GetConnectionString();
            string query = @" SELECT TOP 10 C.CASE_KEY, C.DEPARTMENT_CASE_NUMBER, 
                            D.DEPARTMENT_NAME, O.OFFENSE_DESCRIPTION AS CHARGE, C.LAB_CASE, 
                            C.OFFENSE_DATE 
                            FROM TV_LABCASE C 
                            INNER JOIN TV_DEPTNAME D ON C.DEPARTMENT_CODE = D.DEPARTMENT_CODE 
                            INNER JOIN TV_OFFENSE O ON C.OFFENSE_CODE = O.OFFENSE_CODE 
                            ORDER BY C.OFFENSE_DATE DESC";

            List<CaseData> caseList = new List<CaseData>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    caseList.Add(new CaseData
                    {
                        CASE_KEY = reader["CASE_KEY"].ToString(),
                        DEPARTMENT_CASE_NUMBER = reader["DEPARTMENT_CASE_NUMBER"].ToString(),
                        DEPARTMENT_NAME = reader["DEPARTMENT_NAME"].ToString(),
                        CHARGE = reader["CHARGE"].ToString(),
                        LAB_CASE = reader["LAB_CASE"].ToString(),
                        OFFENSE_DATE = Convert.ToDateTime(reader["OFFENSE_DATE"]).ToString("yyyy-MM-dd")
                    });
                }
            }

            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(caseList);
        }
    }
}
