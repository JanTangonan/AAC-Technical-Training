﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Configuration;
using System.Web.Script.Serialization;
using PLCGlobals;
using PLCCONTROLS;
using System.Globalization;

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
                    //OFFENSE_DATE = Convert.ToDateTime(qry.FieldByName("OFFENSE_DATE")).ToString("yyyy-MM-dd HH:mm:ss.fff")
                    OFFENSE_DATE = qry.FieldByName("OFFENSE_DATE")
                });
                qry.Next();
            }

            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(caseList);
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
            while (!qry.EOF())
            {
                departments.Add(new Department
                {
                    DEPARTMENT_CODE = qry.FieldByName("DEPARTMENT_CODE"),
                    DEPARTMENT_NAME = qry.FieldByName("DEPARTMENT_NAME")
                });
                qry.Next();
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
            while (!qry.EOF())
            {
                charges.Add(new Charge
                {
                    OFFENSE_CODE = qry.FieldByName("OFFENSE_CODE"),
                    OFFENSE_DESCRIPTION = qry.FieldByName("OFFENSE_DESCRIPTION")
                });
                qry.Next();
            }

            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(charges);
        }

        /// <summary>
        /// Saves input data from client-side to db
        /// </summary>
        /// <param name="CASE_KEY"></param>
        /// <param name="OFFENSE_CODE"></param>
        /// <param name="DEPARTMENT_CASE_NUMBER"></param>
        /// <param name="DEPARTMENT_CODE"></param>
        /// <param name="LAB_CASE"></param>
        /// <param name="OFFENSE_DATE"></param>
        /// <returns>result string</returns>
        [WebMethod(EnableSession = true)]
        public string SaveData(string CASE_KEY, string OFFENSE_CODE, string DEPARTMENT_CASE_NUMBER, string DEPARTMENT_CODE, string LAB_CASE, string OFFENSE_DATE)
        {
            //DateTime PARSED_OFFENSE_DATE = DateTime.ParseExact(OFFENSE_DATE, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            try
            {
                PLCQuery qry = new PLCQuery();
                qry.SQL = "SELECT * FROM TV_LABCASE WHERE CASE_KEY = " + CASE_KEY;
                qry.Open();
                if (qry.HasData())
                {
                    qry.Edit();
                    qry.SetFieldValue("DEPARTMENT_CASE_NUMBER", DEPARTMENT_CASE_NUMBER);
                    qry.SetFieldValue("DEPARTMENT_CODE", DEPARTMENT_CODE);
                    qry.SetFieldValue("OFFENSE_CODE", OFFENSE_CODE);
                    qry.SetFieldValue("LAB_CASE", LAB_CASE);
                    //qry.SetFieldValue("OFFENSE_DATE", PARSED_OFFENSE_DATE);
                    qry.SetFieldValue("OFFENSE_DATE", OFFENSE_DATE);

                    qry.Post("TV_LABCASE");
                }                

                return $"Save sucessful";
            }
            catch (Exception ex)
            {
                // Log the exception and return an error message
                return $"Error saving data: {ex.StackTrace}";
            }
        }
        #endregion
    }
}
