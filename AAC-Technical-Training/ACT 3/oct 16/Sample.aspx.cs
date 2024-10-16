using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PLCCONTROLS;
using PLCCONTROLS;
using AjaxControlToolkit;
using PLCGlobals;
using PLCWebCommon;

namespace BEASTiLIMS
{
    public partial class Sample : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
               gvNames.InitializePLCDBGrid();
            }
        }

        protected void gvNames_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        protected void dbpNames_SetDefaultRecord(object sender, PLCDBPanelSetDefaultRecordEventArgs e)
        {
        }

        protected void dbpNames_PLCDBPanelGetNewRecord(object sender, PLCDBPanelGetNewRecordEventArgs e)
        {
        }

        protected void PLCButtonPanel1_PLCButtonClick(object sender, PLCButtonClickEventArgs e)
        {
            if ((e.button_name == "ADD") & (e.button_action == "AFTER"))
            {
            }

            if ((e.button_name == "EDIT") & (e.button_action == "AFTER"))
            {
            }

            if ((e.button_name == "DELETE") & (e.button_action == "AFTER"))
            {
                if (this.gvNames.Rows.Count > 0)
                {
                }
                else
                {
                }

            }

            if (e.button_name == "SAVE")
            {
                if (e.button_action == "BEFORE")
                {
                }

                if (e.button_action == "AFTER")
                {
                }
            }

            if ((e.button_name == "CANCEL") & (e.button_action == "AFTER"))
            {
            }
        }

        protected void bnAction1_Click(object sender, EventArgs e)
        {
        }

        protected void bnAction2_Click(object sender, EventArgs e)
        {
        }
    }
}