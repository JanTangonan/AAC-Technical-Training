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
            if (!IsPostBack)
            {
                RestoreSearchCriteria(dbpAnalystSearch);
                RestoreSearchCriteria(dbpAdressSearch);
            }       
        }

        protected void bpAnalystSearch_PLCButtonClick(object sender, PLCCONTROLS.PLCButtonClickEventArgs e)
        {

        }

        protected void bpAdressSearch_PLCButtonClick(object sender, PLCButtonClickEventArgs e)
        {
            
        }
        /// <summary>
        /// Restores search criteria to the dbpanel
        /// </summary>
        /// <param name="thePanel"></param>
        private void RestoreSearchCriteria(DBPanel thePanel)
        {
            var searchCriteria = Session["SavedSearchCriteria"] as Dictionary<string, Dictionary<string, Tuple<string, string, string, string>>>;

            if (searchCriteria != null && searchCriteria.ContainsKey(thePanel.ID))
            {
                Dictionary<string, Tuple<string, string, string, string>> fieldData = searchCriteria[thePanel.ID];

                if (fieldData.Count > 0)
                {
                    foreach (var field in fieldData)
                    {
                        string tableName = field.Value.Item1;
                        string fieldName = field.Key;
                        string prompt = field.Value.Item3;
                        string fieldValue = field.Value.Item4;

                        thePanel.SetPanelFieldValue(tableName, fieldName, prompt, fieldValue);
                    }
                }
            }
        }
    }
}