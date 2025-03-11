using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PLCCONTROLS;
using PLCGlobals;

namespace BEASTiLIMS
{
    public partial class AnalystSearch : PLCGlobals.PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            PLCHelperFunctions SearchHelper = new PLCHelperFunctions();

            dbgAnalystSearchResult2.PLCSQLString = @"select ANALYST, NAME, TITLE, LAB_CODE, ACCESS_GROUP, CUSTODY_OF from TV_ANALYST";

            if (Session["SavedSearchCriteria"] != null)
            {
                bpAdressSearch.WhereClause = GetWhereClause(dbpAdressSearch, dbgAdressSearch);
                bpAnalystSearch.WhereClause = GetWhereClause(dbpAnalystSearch, dbgAnalystSearchResult);
                bpAnalystSearch2.WhereClause = GetWhereClause(dbpAnalystSearch2, dbgAnalystSearchResult2);
            }

            if (!IsPostBack)
            {
                RestoreSearchCriteria(dbpAnalystSearch);
                RestoreSearchCriteria(dbpAnalystSearch2);
                RestoreSearchCriteria(dbpAdressSearch);
            }       
        }

        protected void bpAnalystSearch_PLCButtonClick(object sender, PLCCONTROLS.PLCButtonClickEventArgs e)
        {
            //bpAnalystSearch.WhereClause = GetWhereClause(dbpAnalystSearch, dbgAnalystSearchResult);
        }

        protected void bpAnalystSearch2_PLCButtonClick(object sender, PLCCONTROLS.PLCButtonClickEventArgs e)
        {
            //bpAnalystSearch2.WhereClause = GetWhereClause(dbpAnalystSearch2, dbgAnalystSearchResult2);
        }

        protected void bpAdressSearch_PLCButtonClick(object sender, PLCButtonClickEventArgs e)
        {
            //bpAdressSearch.WhereClause = GetWhereClause(dbpAdressSearch, dbgAdressSearch);
        }
        /// <summary>
        /// Restores search criteria to the dbpanel
        /// </summary>
        /// <param name="thePanel"></param>
        private void RestoreSearchCriteria(DBPanel thePanel)
        {
            // Check if session exists
            var searchCriteria = Session["SavedSearchCriteria"] as Dictionary<string, Dictionary<string, Tuple<string, string, string, string, string, string, bool>>>;

            if (searchCriteria != null && searchCriteria.ContainsKey(thePanel.ID)) // Ensure data exists for this panel
            {
                Dictionary<string, Tuple<string, string, string, string, string, string, bool>> fieldData = searchCriteria[thePanel.ID];

                if (fieldData.Count > 0)
                {
                    foreach (var field in fieldData)
                    {
                        string tableName = field.Value.Item1;
                        string fieldName = field.Key;
                        string prompt = field.Value.Item3;
                        string fieldValue = field.Value.Item6;

                        // Restore field values in the panel
                        thePanel.SetPanelFieldValue(tableName, fieldName, prompt, fieldValue);
                    }
                }
            }
        }
        private string GetWhereClause(PLCDBPanel ThePanel, PLCDBGrid TheGrid)
        {
            string whereClause = string.Empty;

            Dictionary<string, OrderedDictionary> tables = new Dictionary<string, OrderedDictionary>();

            var searchCriteria = Session["SavedSearchCriteria"] as Dictionary<string, Dictionary<string, Tuple<string, string, string, string, string, string, bool>>>;

            PLCHelperFunctions SearchHelper = new PLCHelperFunctions();
            tables = SearchHelper.GetTablesAndAlias(TheGrid.PLCGridName);

            var fields = ThePanel.GetPanelFieldRecs();
            string panelKey = ThePanel.ID;

            if (searchCriteria != null && searchCriteria.ContainsKey(panelKey))
            {
                Dictionary<string, Tuple<string, string, string, string, string, string, bool>> fieldData = searchCriteria[panelKey];

                foreach (var field in fieldData)
                {
                    string tableName = field.Value.Item1;
                    string fieldName = field.Key; // This is the field name
                    string prompt = field.Value.Item3;
                    string mask = field.Value.Item4;
                    string codeTable = field.Value.Item5;
                    string searchValue = field.Value.Item6;
                    bool usesLikeSearch = field.Value.Item7;

                    // Use these values in GetWhereClauseByType
                    whereClause = PLCGlobals.PLCCommon.instance.GetWhereClauseByType(
                        tableName, fieldName, prompt, mask, codeTable, searchValue, tables, usesLikeSearch);
                }
            }
            return whereClause;
        }
    }
}