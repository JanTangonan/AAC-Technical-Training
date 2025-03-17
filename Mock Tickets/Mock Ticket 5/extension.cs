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
        #region PLCDBPanel
        /// <summary>
        /// PLCDBPanel extension for getting where clause string 
        /// </summary>
        /// <param name="dbp"></param>
        /// <param name="tables"></param>
        /// <returns></returns>
        public static string GetWhereClause(this PLCDBPanel dbp, Dictionary<string, OrderedDictionary> tablesAndAliases = null)
        {
            PLCCommon helper = new PLCCommon();
            string whereClause = string.Empty;

            var fields = dbp.GetPanelFieldRecs();

            foreach (var panelRec in fields)
            {
                string tableName = panelRec.tblname;
                string fieldName = panelRec.fldname;
                string prompt = panelRec.prompt;
                string mask = panelRec.editmask.ToUpper();
                string codeTable = panelRec.codetable;
                bool usesLikeSearch = panelRec.usesLikeSearch;

                string fieldValue = dbp.GetFieldValue(tableName, fieldName, prompt).Trim();

                if (!string.IsNullOrEmpty(fieldValue))
                {
                    whereClause += helper.GetWhereClauseByType(tableName, fieldName, prompt, mask, codeTable, fieldValue, tablesAndAliases, usesLikeSearch);
                }
            }

            return whereClause;
        }

        #endregion PLCDBPanel

        #region PLCDBGrid

        /// <summary>
        /// PLCDBGrid extension for getting tables and aliases dictionary
        /// </summary>
        /// <param name="dbg"></param>
        /// <returns></returns>
        public static Dictionary<string, OrderedDictionary> GetTablesAndAlias(PLCDBGrid dbg)
        {
            PLCHelperFunctions helper = new PLCHelperFunctions();
            Dictionary<string, OrderedDictionary> tablesAndAliases = new Dictionary<string, OrderedDictionary>();
            tablesAndAliases = helper.GetTablesAndAlias(dbg.PLCGridName);

            return tablesAndAliases;
        }
        #endregion PLCDBGrid

    }
}