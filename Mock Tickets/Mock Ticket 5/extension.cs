using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.UI.WebControls;
using PLCCONTROLS;

namespace PLCGlobals
{
	public static class PLCExtensions
    {
        /// <summary>
        /// PLCDBPanel extension for generating sql for multi-table panel
        /// </summary>
        /// <param name="dbp"></param>
        /// <returns>SQL Dictionary. Key is the table name and the value contains the sql for that table</returns>
        /// <example>var queries = PLCDBPanel.GenerateSQL()</example>
        public static Dictionary<string, string[]> GenerateSQL(this PLCDBPanel dbp)
        {
            var cacehKey = "PLCDBPanelExtension_SQL_" + dbp.PLCPanelName;
            var queries = new Dictionary<string, string[]>();
            if (CacheHelper.IsInCache(cacehKey))
            {
                queries = (Dictionary<string, string[]>)CacheHelper.GetItem(cacehKey);
                return queries;
            }

            var fields = dbp.GetPanelFieldNames();
            foreach (var fieldName in fields)
            {
                var pr = dbp.GetPanelRecByFieldName(fieldName);

                if (!queries.ContainsKey(pr.tblname))
                    queries.Add(pr.tblname, new string[2]);

                var tempstr = queries[pr.tblname][0];
                if (!string.IsNullOrEmpty(tempstr))
                    tempstr += ", ";

                if (pr.fldtype == "T")
                {
                    if (PLCSession.PLCDatabaseServer == "MSSQL")
                    {
                        if (pr.editmask.ToUpper() == "HH:MM:SS")
                            tempstr += "convert(char(8), " + fieldName + ", 108) " + fieldName;
                        else
                            tempstr += "convert(char(5), " + fieldName + ", 108) " + fieldName;
                    }
                    else
                    {
                        if (pr.editmask.ToUpper() == "HH:MM:SS")
                            tempstr += "to_char(" + fieldName + ",'HH24:MI:SS') " + fieldName;
                        else
                            tempstr += "to_char(" + fieldName + ",'HH24:MI') " + fieldName;
                    }
                }
                else if (pr.fldtype == "DT") //datetime
                {
                    if (PLCSession.PLCDatabaseServer == "MSSQL") //mssql
                    {
                        if (pr.editmask.ToUpper().Contains(":SS"))
                            tempstr += "CONVERT(VARCHAR(10), " + fieldName + ", 101) + ' ' + CONVERT(VARCHAR(8), " + fieldName + ", 114) AS " + fieldName; //with seconds
                        else
                            tempstr += "CONVERT(VARCHAR(10), " + fieldName + ", 101) + ' ' + CONVERT(VARCHAR(5), " + fieldName + ", 114) AS " + fieldName; //without seconds
                    }
                    else //oracle
                    {
                        if (pr.editmask.ToUpper().Contains(":SS"))
                            tempstr += "to_char(" + fieldName + ", 'MM/DD/YYYY HH24:MI:SS') AS " + fieldName; //with seconds
                        else
                            tempstr += "to_char(" + fieldName + ", 'MM/DD/YYYY HH24:MI') AS " + fieldName; //without seconds
                    }
                }
                else
                    tempstr += fieldName;

                queries[pr.tblname][0] = tempstr;

                tempstr = queries[pr.tblname][1];
                if (!string.IsNullOrEmpty(tempstr))
                    tempstr += ", ";
                queries[pr.tblname][1] = tempstr + fieldName;
            }

            CacheHelper.AddItem(cacehKey, queries);
            return queries;
        }
        
        /// <summary>
        /// PLCDBPanel extension for loading multi-table record
        /// </summary>
        /// <param name="dbp"></param>
        /// <example>PLCDBPanel.LoadRecord()</example>
        public static void LoadRecord(this PLCDBPanel dbp)
        {
            dbp.ClearFields();

            var queries = dbp.GenerateSQL();
            foreach (var query in queries)
            {
                var qry = new PLCQuery("SELECT " + query.Value[0] + " FROM " + query.Key + dbp.PLCWhereClause);
                qry.OpenReadOnly();
                if (qry.HasData())
                {
                    for (int i = 0; i < qry.FieldCount(); i++)
                    {
                        var pr = dbp.GetPanelRecByFieldName(qry.FieldNames(i + 1));
                        if (!pr.HandleIfNumberMask(qry))
                            dbp.setpanelfield(qry.FieldNames(i + 1), qry.FieldByIndex(i));
                    }
                }
            }

            dbp.LoadCodeDescriptions();
            dbp.ResetOriginalValues();
        }
    }
}