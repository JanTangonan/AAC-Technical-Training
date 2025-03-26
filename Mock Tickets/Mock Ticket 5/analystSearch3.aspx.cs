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
        //unique key for the page - ans = analyst search
        private string pageKey => this.GetType().FullName + "_";
        private string saveSearchCriteria => this.GetType().FullName + "_SavedSearchCriteria";


        #region Events
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                RestoreSearchCriteria(dbpAnalystSearch);
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

        }

        #endregion

        #region Methods

        /// <summary>
        /// save search criteria to the session variable
        /// </summary>
        /// <param name="panel"></param>
        private void SaveSearchCriteria(DBPanel panel)
        {
            string panelKey = pageKey + panel.ID;

            var searchCriteria = HttpContext.Current.Session[saveSearchCriteria]
                as Dictionary<string, Dictionary<string, Tuple<string, string, string, string>>>
                ?? new Dictionary<string, Dictionary<string, Tuple<string, string, string, string>>>();

            searchCriteria[panelKey] = panel.GetPanelFieldValues();
            HttpContext.Current.Session[saveSearchCriteria] = searchCriteria;
        }


        /// <summary>
        /// clear search criteria from the session variable based on panelkey
        /// </summary>
        /// <param name="panel"></param>
        private void ClearSearchCriteria(DBPanel panel)
        {
            string panelKey = pageKey + panel.ID;

            var searchCriteria = HttpContext.Current.Session[saveSearchCriteria]
                as Dictionary<string, Dictionary<string, Tuple<string, string, string, string>>>
                ?? new Dictionary<string, Dictionary<string, Tuple<string, string, string, string>>>();

            if (searchCriteria.ContainsKey(panelKey))
            {
                searchCriteria.Remove(panelKey);
                HttpContext.Current.Session[saveSearchCriteria] = searchCriteria;
            }
        }

        /// <summary>
        /// Restores search criteria to the dbpanel
        /// </summary>
        /// <param name="thePanel"></param>
        private void RestoreSearchCriteria(DBPanel panel)
        {
            var savedCriteria = Session[saveSearchCriteria] as Dictionary<string, Dictionary<string, Tuple<string, string, string, string>>>;

            if (savedCriteria != null)
            {
                string panelKey = pageKey + panel.ID;

                if (savedCriteria.ContainsKey(panelKey))
                {
                    var panelFieldData = savedCriteria[panelKey];

                    panel.SetPanelFieldValues(panelFieldData);
                }
            }
        }

        #endregion
    }
}