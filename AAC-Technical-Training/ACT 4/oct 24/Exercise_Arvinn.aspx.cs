using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BEASTiLIMS
{
    public partial class Exercise_Arvinn : PLCGlobals.PageBase
    {
        #region EVENTS
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindGrid();
                bpLabCase.DisableAllButtons();
                EnablePnlCasesTab(false);
                
            }
        }

        protected void dbgLabCase_SelectedIndexChanged(object sender, EventArgs e)
        {
            GridViewRow row = dbgLabCase.SelectedRow;

            if (row.RowType == DataControlRowType.DataRow)
            {
                PopulatePanelFields();
                bpLabCase.EnableButton("Edit");
            }
        }

        protected void bpLabCase_PLCButtonClick(object sender, PLCCONTROLS.PLCButtonClickEventArgs e)
        {
            if (e.button_name == "EDIT" && e.button_action == "AFTER")
            {
                dbgLabCase.SetControlMode(false);
                EnablePnlCasesTab(true);
            }

            if (e.button_name == "SAVE" && e.button_action == "AFTER")
            {
                dbgLabCase.InitializePLCDBGrid();
                EnablePnlCasesTab(false);
                PopulatePanelFields();
            }

            if (e.button_name == "CANCEL" && e.button_action == "AFTER")
            {
                dbgLabCase.SetControlMode(true);
                EnablePnlCasesTab(false);
                PopulatePanelFields();
            }
        }
        #endregion

        #region METHODS

        /// <summary>
        /// Enables Cases Tab DBPanel
        /// </summary>
        /// <param name="enabled"></param>
        private void EnablePnlCasesTab(bool enabled)
        {
            if (enabled)
            {
                dbpLabCase.EditMode();
                dbpLabCase.Enabled = enabled;
            }
            else
            {
                dbpLabCase.EmptyMode();
                dbpLabCase.Enabled = enabled;
            }
        }

        /// <summary>
        /// Populate panel fields
        /// </summary>
        private void PopulatePanelFields()
        {
            dbpLabCase.PLCWhereClause = " WHERE CASE_KEY = " + dbgLabCase.SelectedDataKey.Value.ToString();
            dbpLabCase.PLCDataTable = "TV_LABCASE";
            dbpLabCase.DoLoadRecord();
        }

        /// <summary>
        /// Binds PLCDBGrid from db
        /// </summary>
        private void BindGrid()
        {
            dbgLabCase.PLCSQLString = @"SELECT TOP 10 C.CASE_KEY, C.DEPARTMENT_CASE_NUMBER, 
                                           D.DEPARTMENT_NAME, O.OFFENSE_DESCRIPTION AS CHARGE, LAB_CASE, 
                                           OFFENSE_DATE 
                                           FROM TV_LABCASE C 
                                           INNER JOIN TV_DEPTNAME D ON C.DEPARTMENT_CODE = D.DEPARTMENT_CODE 
                                           INNER JOIN TV_OFFENSE O ON C.OFFENSE_CODE = O.OFFENSE_CODE 
                                           ORDER BY CASE_DATE DESC";

            dbgLabCase.InitializePLCDBGrid();
        }
        #endregion
    }
}