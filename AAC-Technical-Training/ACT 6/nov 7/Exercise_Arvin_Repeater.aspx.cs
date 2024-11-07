using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PLCCONTROLS;
using PLCGlobals;

namespace BEASTiLIMS
{
    public partial class Exercise_Arvin_Repeater : PLCGlobals.PageBase
    {
        //private string selectedCaseKey = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindGrid();
                dbgLabCase.SelectedIndex = 0;
                bpLabCase.SetBrowseMode();
            }
        }

        private bool IsEditMode
        {
            get { return (bool)(ViewState["IsEditMode"] ?? false); }
            set { ViewState["IsEditMode"] = value; }
        }

        protected void bpLabCase_PLCButtonClick(object sender, PLCCONTROLS.PLCButtonClickEventArgs e)
        {
            if (e.button_name == "EDIT" && e.button_action == "AFTER")
            {
                dbgLabCase.SetControlMode(false);
                IsEditMode = true;
                BindRepeater();
            }

            if (e.button_name == "SAVE" && e.button_action == "AFTER")
            {
                dbgLabCase.InitializePLCDBGrid();
                dbgLabCase.SetControlMode(true);
                IsEditMode = false;
                BindRepeater();

            }

            if (e.button_name == "CANCEL" && e.button_action == "AFTER")
            {
                dbgLabCase.SetControlMode(true);
                IsEditMode = false;
                BindRepeater();
            }
        }

        protected void dbgLabCase_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindRepeater();
        }

        protected void repLabCaseControl_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                TextBox txtDepartmentCaseNumber = (TextBox)e.Item.FindControl("txtDepartmentCaseNumber");
                TextBox txtLabCaseNumber = (TextBox)e.Item.FindControl("txtLabCaseNumber");
                TextBox txtReportIncidentDate = (TextBox)e.Item.FindControl("txtReportIncidentDate");

                DropDownList ddlDepartment = (DropDownList)e.Item.FindControl("ddlDepartment");
                ddlDepartment.DataSource = GetDepartmentDataTable(); // Populate from method or data source
                ddlDepartment.DataTextField = "DEPARTMENT_NAME";
                ddlDepartment.DataValueField = "DEPARTMENT_CODE";
                ddlDepartment.DataBind();

                DropDownList ddlCharge = (DropDownList)e.Item.FindControl("ddlCharge");
                ddlCharge.DataSource = GetChargesDataTable(); // Populate from method or data source
                ddlCharge.DataTextField = "OFFENSE_DESCRIPTION";
                ddlCharge.DataValueField = "OFFENSE_CODE";
                ddlCharge.DataBind();

                ddlDepartment.SelectedValue = DataBinder.Eval(e.Item.DataItem, "DEPARTMENT_CODE").ToString();
                ddlCharge.SelectedValue = DataBinder.Eval(e.Item.DataItem, "OFFENSE_CODE").ToString();

                txtDepartmentCaseNumber.Enabled = IsEditMode;
                ddlDepartment.Enabled = IsEditMode;
                ddlCharge.Enabled = IsEditMode;
                txtLabCaseNumber.Enabled = IsEditMode;
                txtReportIncidentDate.Enabled = IsEditMode;
            }
        }


        #region METHODS
        private DataTable GetDepartmentDataTable()
        {
            string qry = "SELECT DEPARTMENT_CODE, DEPARTMENT_NAME FROM TV_DEPTNAME";
            PLCQuery qryDdlDepartment = new PLCQuery(PLCSession.FormatSpecialFunctions(qry));
            qryDdlDepartment.Open();

            return qryDdlDepartment.PLCDataTable;
        }

        private DataTable GetChargesDataTable()
        {
            string qry = "SELECT OFFENSE_CODE, OFFENSE_DESCRIPTION FROM TV_OFFENSE";
            PLCQuery qryDdlDepartment = new PLCQuery(PLCSession.FormatSpecialFunctions(qry));
            qryDdlDepartment.Open();

            return qryDdlDepartment.PLCDataTable;
        }

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

            //dbgLabCase.SelectedIndex = 0;
        }

        private void BindRepeater()
        {
            PLCQuery qry = new PLCQuery();
            qry.SQL = "SELECT DEPARTMENT_CASE_NUMBER, DEPARTMENT_CODE, OFFENSE_CODE, LAB_CASE, OFFENSE_DATE  FROM TV_LABCASE WHERE CASE_KEY = " + dbgLabCase.SelectedDataKey.Value.ToString();

            qry.Open();
            if (!qry.IsEmpty())
            {
                repLabCaseControl.DataSource = qry.PLCDataTable;
                repLabCaseControl.DataBind();
            }
        }

        private string SaveData(string CASE_KEY, string DEPARTMENT_CASE_NUMBER, string DEPARTMENT_CODE, string OFFENSE_CODE, string LAB_CASE , string OFFENSE_DATE)
        {
            try
            {
                PLCQuery qry = new PLCQuery();
                qry.SQL = "SELECT * FROM TV_LABCASE WHERE CASE_KEY = " + CASE_KEY;
                qry.Open();
                if (qry.HasData())
                {
                    qry.Edit();
                    qry.SetFieldValue("DEPARTMENT_CASE_NUMBER", DEPARTMENT_CASE_NUMBER);
                    qry.SetFieldValue("DEPARTMENT_CODE", DEPARTMENT_CODE);
                    qry.SetFieldValue("OFFENSE_CODE", OFFENSE_CODE);
                    qry.SetFieldValue("LAB_CASE", LAB_CASE);
                    qry.SetFieldValue("OFFENSE_DATE", OFFENSE_DATE);

                    qry.Post("TV_LABCASE");
                }

                return $"Save sucessful";
            }
            catch (Exception ex)
            {
                // Log the exception and return an error message
                return $"Error saving data: {ex.StackTrace}";
            }
        }
        #endregion
    }
}