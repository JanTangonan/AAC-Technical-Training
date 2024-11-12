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
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindGrid();
                
                BindRepeater();
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
                Save();
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
                //TextBox txtInvestigatingUnit = (TextBox)e.Item.FindControl("txtInvestigatingUnit");
                //TextBox txtCaseManager = (TextBox)e.Item.FindControl("txtCaseManager");
                //TextBox txtCaseAnalyst = (TextBox)e.Item.FindControl("txtCaseAnalyst");
                //TextBox txtOffenseCategory = (TextBox)e.Item.FindControl("txtOffenseCategory");

                TextBox txtDepartmentCaseNumber = (TextBox)e.Item.FindControl("txtDepartmentCaseNumber");
                TextBox txtLabCaseNumber = (TextBox)e.Item.FindControl("txtLabCaseNumber");
                TextBox txtCaseStatus = (TextBox)e.Item.FindControl("txtCaseStatus");
                TextBox txtReportIncidentDate = (TextBox)e.Item.FindControl("txtReportIncidentDate");
                if (txtReportIncidentDate != null)
                {
                    txtReportIncidentDate.CssClass += " datepicker";
                    System.Diagnostics.Debug.WriteLine("Datepicker class added to textbox.");
                }

                DropDownList ddlDepartment = (DropDownList)e.Item.FindControl("ddlDepartment");
                ddlDepartment.DataSource = GetDepartmentDataTable(); // Populate from method or data source
                ddlDepartment.DataTextField = "DEPARTMENT_NAME";
                ddlDepartment.DataValueField = "DEPARTMENT_CODE";
                ddlDepartment.DataBind();

                DropDownList ddlInvestigatingUnit = (DropDownList)e.Item.FindControl("ddlInvestigatingUnit");
                ddlInvestigatingUnit.DataSource = GetDepartmentDataTable(); // Populate from method or data source
                ddlInvestigatingUnit.DataTextField = "DEPARTMENT_NAME";
                ddlInvestigatingUnit.DataValueField = "DEPARTMENT_CODE";
                ddlInvestigatingUnit.DataBind();

                DropDownList ddlCharge = (DropDownList)e.Item.FindControl("ddlCharge");
                ddlCharge.DataSource = GetChargesDataTable(); // Populate from method or data source
                ddlCharge.DataTextField = "OFFENSE_DESCRIPTION";
                ddlCharge.DataValueField = "OFFENSE_CODE";
                ddlCharge.DataBind();

                DropDownList ddlCaseManager = (DropDownList)e.Item.FindControl("ddlCaseManager");
                ddlCaseManager.DataSource = GetCaseManagerDataTable();
                ddlCaseManager.DataTextField = "MANAGER_NAME";
                ddlCaseManager.DataValueField = "MANAGER_ID";
                ddlCaseManager.DataBind();

                DropDownList ddlCaseAnalyst = (DropDownList)e.Item.FindControl("ddlCaseAnalyst");
                ddlCaseAnalyst.DataSource = GetCaseAnalystDataTable();
                ddlCaseAnalyst.DataTextField = "ANALYST_NAME";
                ddlCaseAnalyst.DataValueField = "ANALYST_ID";
                ddlCaseAnalyst.DataBind();

                DropDownList ddlOffenseCategory = (DropDownList)e.Item.FindControl("ddlOffenseCategory");
                ddlOffenseCategory.DataSource = GetCategoryDataTable();
                ddlOffenseCategory.DataTextField = "DESCRIPTION";
                ddlOffenseCategory.DataValueField = "CODE";
                ddlOffenseCategory.DataBind();


                ddlDepartment.SelectedValue = DataBinder.Eval(e.Item.DataItem, "DEPARTMENT_CODE").ToString();
                ddlCharge.SelectedValue = DataBinder.Eval(e.Item.DataItem, "OFFENSE_CODE").ToString();
                ddlInvestigatingUnit.SelectedValue = DataBinder.Eval(e.Item.DataItem, "INVESTIGATING_AGENCY").ToString();
                ddlCaseManager.SelectedValue = DataBinder.Eval(e.Item.DataItem, "CASE_MANAGER").ToString();
                ddlCaseAnalyst.SelectedValue = DataBinder.Eval(e.Item.DataItem, "CASE_ANALYST").ToString();
                ddlOffenseCategory.SelectedValue = DataBinder.Eval(e.Item.DataItem, "OFFENSE_CATEGORY").ToString();

                txtDepartmentCaseNumber.Enabled = IsEditMode;
                //txtInvestigatingUnit.Enabled = IsEditMode;
                //txtCaseManager.Enabled = IsEditMode;
                //txtCaseAnalyst.Enabled = IsEditMode;
                //txtOffenseCategory.Enabled = IsEditMode;
                txtCaseStatus.Enabled = IsEditMode;
                txtLabCaseNumber.Enabled = IsEditMode;
                txtReportIncidentDate.Enabled = IsEditMode;

                ddlDepartment.Enabled = IsEditMode;
                ddlCharge.Enabled = IsEditMode;
                ddlInvestigatingUnit.Enabled = IsEditMode;
                ddlCaseManager.Enabled = IsEditMode;
                ddlCaseAnalyst.Enabled = IsEditMode;
                ddlOffenseCategory.Enabled = IsEditMode;
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

        private DataTable GetCaseManagerDataTable()
        {
            string qry = "SELECT ANALYST AS MANAGER_ID, NAME AS MANAGER_NAME FROM TV_ANALYST";
            PLCQuery qryDdlCaseManager = new PLCQuery(PLCSession.FormatSpecialFunctions(qry));
            qryDdlCaseManager.Open();

            return qryDdlCaseManager.PLCDataTable;
        }

        private DataTable GetCaseAnalystDataTable()
        {
            string qry = "SELECT ANALYST AS ANALYST_ID, NAME AS ANALYST_NAME FROM TV_ANALYST";
            PLCQuery qryDdlCaseAnalyst = new PLCQuery(PLCSession.FormatSpecialFunctions(qry));
            qryDdlCaseAnalyst.Open();

            return qryDdlCaseAnalyst.PLCDataTable;
        }

        private DataTable GetCategoryDataTable()
        {
            string qry = "SELECT CODE, DESCRIPTION FROM TV_OFFENCAT";
            PLCQuery qryCategory = new PLCQuery(PLCSession.FormatSpecialFunctions(qry));
            qryCategory.Open();

            return qryCategory.PLCDataTable;
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
            dbgLabCase.SelectedIndex = 0;
        }

        private void BindRepeater()
        {
            PLCQuery qry = new PLCQuery();
            qry.SQL = @"SELECT DEPARTMENT_CODE, DEPARTMENT_CASE_NUMBER, DEPARTMENT_CODE, OFFENSE_CODE, LAB_CASE, OFFENSE_DATE,
                        INVESTIGATING_AGENCY, CASE_MANAGER, CASE_ANALYST, OFFENSE_CATEGORY, CASE_STATUS
                        FROM TV_LABCASE WHERE CASE_KEY = " + dbgLabCase.SelectedDataKey.Value.ToString();

            qry.Open();
            if (!qry.IsEmpty())
            {
                repLabCaseControl.DataSource = qry.PLCDataTable;
                repLabCaseControl.DataBind();
            }
        }

        private void Save()
        {
            foreach (RepeaterItem item in repLabCaseControl.Items)
            {
                TextBox txtDepartmentCaseNumber = (TextBox)item.FindControl("txtDepartmentCaseNumber");
                DropDownList ddlDepartment = (DropDownList)item.FindControl("ddlDepartment");
                DropDownList ddlCharge = (DropDownList)item.FindControl("ddlCharge");

                DropDownList ddlInvestigatingUnit = (DropDownList)item.FindControl("ddlInvestigatingUnit");
                DropDownList ddlCaseManager = (DropDownList)item.FindControl("ddlCaseManager");
                DropDownList ddlCaseAnalyst = (DropDownList)item.FindControl("ddlCaseAnalyst");
                DropDownList ddlOffenseCategory = (DropDownList)item.FindControl("ddlOffenseCategory");

                //TextBox txtInvestigatingUnit = (TextBox)item.FindControl("txtInvestigatingUnit");
                //TextBox txtCaseManager = (TextBox)item.FindControl("txtCaseManager");
                //TextBox txtCaseAnalyst = (TextBox)item.FindControl("txtCaseAnalyst");
                //TextBox txtOffenseCategory = (TextBox)item.FindControl("txtOffenseCategory");

                TextBox txtCaseStatus = (TextBox)item.FindControl("txtCaseStatus");
                TextBox txtLabCaseNumber = (TextBox)item.FindControl("txtLabCaseNumber");
                TextBox txtReportIncidentDate = (TextBox)item.FindControl("txtReportIncidentDate");

                string departmentCaseNumber = txtDepartmentCaseNumber.Text;
                string departmentcode = ddlDepartment.SelectedValue;
                string offenseCode = ddlCharge.SelectedValue;

                string investigatingUnit = ddlInvestigatingUnit.SelectedValue;
                string caseManager = ddlCaseManager.SelectedValue;
                string caseAnalyst = ddlCaseAnalyst.SelectedValue;
                string offenseCategory = ddlOffenseCategory.SelectedValue;

                //string investigatingUnit = txtInvestigatingUnit.Text;
                //string caseManager = txtCaseManager.Text;
                //string caseAnalyst = txtCaseAnalyst.Text;
                //string offenseCategory = txtOffenseCategory.Text;
                string caseStatus = txtCaseStatus.Text;
                string labCaseNumber = txtLabCaseNumber.Text;
                string reportIncidentDate = txtReportIncidentDate.Text;

                ValidateField(departmentCaseNumber);
                ValidateField(investigatingUnit);
                ValidateField(offenseCategory);
                SaveData(departmentCaseNumber, departmentcode, offenseCode, investigatingUnit, caseManager, caseAnalyst, offenseCategory, caseStatus, labCaseNumber, reportIncidentDate);
            }
        }

        private string SaveData(string DEPARTMENT_CASE_NUMBER, string DEPARTMENT_CODE, string OFFENSE_CODE, string INVESTIGATING_AGENCY, string CASE_MANAGER, 
            string CASE_ANALYST, string OFFENSE_CATEGORY, string CASE_STATUS, string LAB_CASE , string OFFENSE_DATE)
        {
            try
            {
                PLCQuery qry = new PLCQuery();
                qry.SQL = "SELECT * FROM TV_LABCASE WHERE CASE_KEY = " + dbgLabCase.SelectedDataKey.Value.ToString();
                qry.Open();
                if (qry.HasData())
                {
                    qry.Edit();
                    qry.SetFieldValue("DEPARTMENT_CASE_NUMBER", DEPARTMENT_CASE_NUMBER);
                    qry.SetFieldValue("DEPARTMENT_CODE", DEPARTMENT_CODE);
                    qry.SetFieldValue("OFFENSE_CODE", OFFENSE_CODE);

                    qry.SetFieldValue("INVESTIGATING_AGENCY", INVESTIGATING_AGENCY);
                    qry.SetFieldValue("CASE_MANAGER", CASE_MANAGER);
                    qry.SetFieldValue("CASE_ANALYST", CASE_ANALYST);
                    qry.SetFieldValue("OFFENSE_CATEGORY", OFFENSE_CATEGORY);
                    qry.SetFieldValue("CASE_STATUS", CASE_STATUS);

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

        private void ValidateField(string field)
        {
            if(string.IsNullOrWhiteSpace(field))
            {
                string message = "Input required on marked controls."; 
                dlgMessage.ShowAlert("MESSAGE", message);
            }
            else
            {
                return;
            }
        }
        #endregion
    }
}