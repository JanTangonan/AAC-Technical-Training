using System;
using System.Web.UI.WebControls;
using PLCCONTROLS;

namespace AAC_Technical_Training
{
    public partial class Exercise3 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindGrid();
                bpLabCase.DisableAllButtons();
            }
        }

        protected void grdCasesTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ViewState["IsEditing"] == null || !(bool)ViewState["IsEditing"])
            {
                PopulatePanelFields();
                bpLabCase.EnableButton("Edit");
            }
            else
            {
                foreach (GridViewRow row in grdCasesTable.Rows)
                {
                    if(row.RowIndex == grdCasesTable.SelectedIndex)
                    {
                        row.CssClass = "";
                        row.Enabled = true;
                    }
                    else
                    {
                        row.CssClass = "disabled";
                        row.Enabled = false;
                    }
                }
            }
        }

        protected void bpLabCase_PLCButtonClick(object sender, PLCButtonClickEventArgs e)
        {
            if (grdCasesTable.SelectedIndex >= 0)
            {
                if (e.button_name == "EDIT" && e.button_action == "AFTER")
                {
                    ViewState["IsEditing"] = true;
                }

                //if (e.button_name == "EDIT" && e.button_action == "AFTER")
                //{
                //    grdCasesTable.CssClass = "disabled";
                //}

                if (e.button_name == "SAVE" && e.button_action == "AFTER")
                {
                    grdCasesTable.InitializePLCDBGrid();
                    PopulatePanelFields();

                    ViewState["isEditing"] = false;
                    grdCasesTable.CssClass = "";
                }

                if (e.button_name == "CANCEL" && e.button_action == "AFTER")
                {
                    ViewState["isEditing"] = false;
                    grdCasesTable.CssClass = "";
                }
            }
        }

        /// <summary>
        /// Populate panel fields
        /// </summary>
        private void PopulatePanelFields()
        {
            pnlCasesTab.PLCWhereClause = " WHERE CASE_KEY = " + grdCasesTable.SelectedDataKey.Value.ToString();
            pnlCasesTab.PLCDataTable = "TV_LABCASE";
            pnlCasesTab.DoLoadRecord();
        }

        /// <summary>
        /// Binds PLCDBGrid from db
        /// </summary>
        private void BindGrid()
        {
            grdCasesTable.PLCSQLString = @"SELECT TOP 10 C.CASE_KEY, C.DEPARTMENT_CASE_NUMBER, 
                                            D.DEPARTMENT_NAME, O.OFFENSE_DESCRIPTION AS CHARGE, LAB_CASE, 
                                            OFFENSE_DATE 
                                            FROM TV_LABCASE C 
                                            INNER JOIN TV_DEPTNAME D ON C.DEPARTMENT_CODE = D.DEPARTMENT_CODE 
                                            INNER JOIN TV_OFFENSE O ON C.OFFENSE_CODE = O.OFFENSE_CODE 
                                            ORDER BY CASE_DATE DESC";

            grdCasesTable.InitializePLCDBGrid();
        }
    }
}