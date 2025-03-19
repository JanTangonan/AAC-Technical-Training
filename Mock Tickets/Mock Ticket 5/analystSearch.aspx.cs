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

        }

        protected void bpCaseSearch_PLCButtonClick(object sender, PLCButtonClickEventArgs e)
        {

        }

        #endregion

        #region Methods
        /// <summary>
        /// Restores search criteria to the dbpanel
        /// </summary>
        /// <param name="thePanel"></param>
        private void RestoreSearchCriteria(DBPanel thePanel)
        {
            var searchCriteria = Session["SavedSearchCriteria"] as Dictionary<string, Dictionary<string, Tuple<string, string, string, string>>>;

            thePanel.SetPanelFieldValues(searchCriteria);
        }

        #endregion
    }
}