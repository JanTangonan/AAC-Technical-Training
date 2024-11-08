using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PLCCONTROLS;

namespace BEASTiLIMS
{
    public partial class Exercise_Arvin_Repeater : PLCGlobals.PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindGrid();
            }
        }

        protected void bpLabCase_PLCButtonClick(object sender, PLCCONTROLS.PLCButtonClickEventArgs e)
        {

        }

        protected void dbgLabCase_SelectedIndexChanged(object sender, EventArgs e)
        {
            GridViewRow row = dbgLabCase.SelectedRow;

            string departmentCaseNumber = row.Cells[0].Text;
            string department = row.Cells[1].Text;
            string charge = row.Cells[2].Text;
            string labCaseNumber = row.Cells[3].Text;
            DateTime reportIncidentDate = DateTime.Parse(row.Cells[4].Text);

            var selectedData = new List<dynamic>
            {
                new
                {
                    DepartmentCaseNumber = departmentCaseNumber,
                    Department = department,
                    Charge = charge,
                    LabCaseNumber = labCaseNumber,
                    ReportIncidentDate = reportIncidentDate
                }
            };

            repLabCaseControl.DataSource = selectedData;
            repLabCaseControl.DataBind();
        }

        protected void repLabCaseControl_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            DropDownList ddlDepartment = (DropDownList)e.Item.FindControl("ddlDepartment");
            ddlDepartment.DataSource = GetDepartmentDataTable(); // Populate from method or data source
            ddlDepartment.DataBind();

            DropDownList ddlCharge = (DropDownList)e.Item.FindControl("ddlCharge");
            ddlCharge.DataSource = GetChargesDataTable(); // Populate from method or data source
            ddlCharge.DataBind();

            ddlDepartment.SelectedValue = DataBinder.Eval(e.Item.DataItem, "Department").ToString();
            ddlCharge.SelectedValue = DataBinder.Eval(e.Item.DataItem, "Charge").ToString();
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
        }

        #endregion
    }
}