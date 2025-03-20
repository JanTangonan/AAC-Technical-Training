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
        string key = "ans_";

        #region Events
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                RestoreSearchCriteria(dbpAnalystSearch);
                RestoreSearchCriteria(dbpCaseSearch);
            }       
        }

        protected void bpAnalystSearch_PLCButtonClick(object sender, PLCCONTROLS.PLCButtonClickEventArgs e)
        {
            if (e.button_name == "SEARCH" && e.button_action == "AFTER")
            {
                SaveSearchCriteria(dbpAnalystSearch);
            }

            if (e.button_name == "CLEAR" && e.button_action == "AFTER")
            {
                ClearSearchCriteria(dbpAnalystSearch);
            }
        }

        protected void bpCaseSearch_PLCButtonClick(object sender, PLCButtonClickEventArgs e)
        {
            if (e.button_name == "SEARCH" && e.button_action == "AFTER")
            {
                SaveSearchCriteria(dbpCaseSearch);
            }

            if (e.button_name == "CLEAR" && e.button_action == "AFTER")
            {
                ClearSearchCriteria(dbpCaseSearch);
            }
        }

        #endregion

        #region Methods

        private void SaveSearchCriteria(DBPanel panel)
        {
            string panelKey = key + panel.ID;

            var searchCriteria = HttpContext.Current.Session["SavedSearchCriteria"]
                as Dictionary<string, Dictionary<string, Tuple<string, string, string, string>>>
                ?? new Dictionary<string, Dictionary<string, Tuple<string, string, string, string>>>();

            searchCriteria[panelKey] = panel.GetPanelFieldValues();
            HttpContext.Current.Session["SavedSearchCriteria"] = searchCriteria;
        }

        private void ClearSearchCriteria(DBPanel panel)
        {
            string panelKey = key + panel.ID;

            var searchCriteria = HttpContext.Current.Session["SavedSearchCriteria"]
                as Dictionary<string, Dictionary<string, Tuple<string, string, string, string>>>
                ?? new Dictionary<string, Dictionary<string, Tuple<string, string, string, string>>>();

            if (searchCriteria.ContainsKey(panelKey))
            {
                searchCriteria.Remove(panelKey);
                HttpContext.Current.Session["SavedSearchCriteria"] = searchCriteria;
            }
        }

        /// <summary>
        /// Restores search criteria to the dbpanel
        /// </summary>
        /// <param name="thePanel"></param>
        private void RestoreSearchCriteria(DBPanel thePanel)
        {
            var savedCriteria = Session["SavedSearchCriteria"] as Dictionary<string, Dictionary<string, Tuple<string, string, string, string>>>;

            if (savedCriteria != null)
            {
                var cleanedCriteria = new Dictionary<string, Dictionary<string, Tuple<string, string, string, string>>>();

                foreach (var item in savedCriteria)
                {
                    string originalKey = item.Key.Replace(key, ""); 
                    cleanedCriteria[originalKey] = item.Value;
                }

                thePanel.SetPanelFieldValues(cleanedCriteria);
            }

            thePanel.SetPanelFieldValues(savedCriteria);
        }

        #endregion
    }
}