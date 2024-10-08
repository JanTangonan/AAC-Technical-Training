using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AAC_Technical_Training
{
    public partial class Exercise2 : System.Web.UI.Page
    {
        string queryDdlDept = "SELECT DEPARTMENT_NAME, DEPARTMENT_CODE FROM TV_DEPTNAME";
        string queryDdlOffense = "SELECT OFFENSE_DESCRIPTION, OFFENSE_CODE FROM TV_OFFENSE";

        string departmentDataTextField = "DEPARTMENT_NAME";
        string departmentDataValueField = "DEPARTMENT_CODE";

        string chargeDataTextField = "OFFENSE_DESCRIPTION";
        string chargeDataValueField = "OFFENSE_CODE";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindGrid();
            }
        }

        /// <summary>
        /// Binds and displays data from DB to GridView
        /// </summary>
        protected void BindGrid()
        {
            string connectionString = GetConnectionString();

            string queryGridView1 = @"SELECT TOP 10 C.CASE_KEY, C.DEPARTMENT_CASE_NUMBER, D.DEPARTMENT_NAME, 
                            O.OFFENSE_DESCRIPTION AS CHARGE, LAB_CASE, OFFENSE_DATE 
                            FROM TV_LABCASE C 
                            INNER JOIN TV_DEPTNAME D ON C.DEPARTMENT_CODE = D.DEPARTMENT_CODE 
                            INNER JOIN TV_OFFENSE O ON C.OFFENSE_CODE = O.OFFENSE_CODE 
                            ORDER BY CASE_DATE DESC";
            
            Bind(connectionString, queryGridView1, GridView1);
        }

        private void Bind(string ConnectionString, string query, Control control, string textField = "", string valueField = "")
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataAdapter da = new SqlDataAdapter(command);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (control is GridView gridView)
                {
                    gridView.DataSource = dt;
                    gridView.DataBind();
                }
                else if (control is DropDownList dropDownList)
                {
                    dropDownList.DataSource = dt;
                    dropDownList.DataTextField = textField;
                    dropDownList.DataValueField = valueField;
                    dropDownList.DataBind();
                }
            }
        }

        /// <summary>
        /// Connection string declaration.
        /// </summary>
        /// <returns type="string">Returns the Connection String</returns>
        private string GetConnectionString()
        {
            string serverName = "LOCALHOST";
            string databaseName = "VADFS";
            string userId = "vadfs";
            string password = "vadfs";

            string connectionString = $"Server={serverName};Database={databaseName};User Id={userId};Password={password};";

            return connectionString;
        }

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Add an attribute to trigger postback when row is clicked
                e.Row.Attributes["onclick"] = Page.ClientScript.GetPostBackClientHyperlink(GridView1, "Select$" + e.Row.RowIndex);
                e.Row.Attributes["style"] = "cursor:pointer"; 
            }
        }

        protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            //binds only if ddl counts are empty
            string connectionString = GetConnectionString();

            Bind(connectionString, queryDdlDept, ddlDepartment, departmentDataTextField, departmentDataValueField);
            Bind(connectionString, queryDdlOffense, ddlCharge, chargeDataTextField, chargeDataValueField);

            if (e.CommandName == "Select")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                GridViewRow row = GridView1.Rows[rowIndex];

                string selectedDepartmentText = row.Cells[1].Text;
                string selectedChargeText = row.Cells[2].Text;

                txtDepartmentCaseNumber.Text = row.Cells[0].Text;
                txtLabCaseNumber.Text = row.Cells[3].Text;
                txtReportIncidentDate.Text = row.Cells[4].Text;

                DdlSelector(ddlCharge, selectedChargeText);
                DdlSelector(ddlDepartment, selectedDepartmentText);
            }
        }

        private void DdlSelector(DropDownList ddlControl, string textToSelect)
        {
            if (ddlControl.Items.Count == 0)
            {
                return; 
            }

            // Find the item by text
            ListItem selectedItem = ddlControl.Items.FindByText(textToSelect);
            if (selectedItem != null)
            {
                ddlControl.ClearSelection();
                selectedItem.Selected = true;
            }
            else
            {
                Console.WriteLine($"No matching item found for: {textToSelect}");
            }
        }

        private void EnableControls(bool boolBtnSave, bool boolBtnCancel, bool boolTxtDepartmentCaseNumber, bool boolTxtLabCaseNumber, bool boolTxtReportIncedentDate, bool boolDdlCharge, bool boolDdlDepartment, 
            bool boolBtnEdit)
        {
            btnSave.Enabled = boolBtnSave;
            btnCancel.Enabled = boolBtnCancel;

            txtDepartmentCaseNumber.Enabled = boolTxtDepartmentCaseNumber;
            txtLabCaseNumber.Enabled = boolTxtLabCaseNumber;
            txtReportIncidentDate.Enabled = boolTxtReportIncedentDate;

            ddlCharge.Enabled = boolDdlCharge;
            ddlDepartment.Enabled = boolDdlDepartment;

            btnEdit.Enabled = boolBtnEdit;
        }

        protected void BtnEdit_Click(object sender, EventArgs e)
        {  
            if (GridView1.SelectedIndex >= 0)
            {
                EnableControls(true, true, true, true, true, true, true, false);
            }
        }

        protected void BtnSave_Click(object sender, EventArgs e)
        {
            // Retrieve values from textboxes and dropdown lists
            string newDepartmentCaseNumber = txtDepartmentCaseNumber.Text;
            string newDepartmentCode = ddlDepartment.SelectedValue;
            string newOffenseCode = ddlCharge.SelectedValue;
            string newLabCaseNumber = txtLabCaseNumber.Text;
            string newReportIncidentDate = txtReportIncidentDate.Text;

            string caseKey = GridView1.SelectedDataKey.Value.ToString();

            // Database connection string (adjust as needed)
            string connectionString = GetConnectionString();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Update TV_LABCASE
                string updateLabcaseQuery = @"
                    UPDATE TV_LABCASE
                    SET 
                        DEPARTMENT_CASE_NUMBER = @DepartmentCaseNumber,
                        DEPARTMENT_CODE = @DepartmentCode,
                        OFFENSE_CODE = @OffenseCode,
                        LAB_CASE = @LabCase,
                        OFFENSE_DATE = @OffenseDate
                    WHERE CASE_KEY = @CaseKey";

                using (SqlCommand command = new SqlCommand(updateLabcaseQuery, connection))
                {
                    command.Parameters.AddWithValue("@DepartmentCaseNumber", newDepartmentCaseNumber);
                    command.Parameters.AddWithValue("@DepartmentCode", newDepartmentCode);
                    command.Parameters.AddWithValue("@OffenseCode", newOffenseCode);
                    command.Parameters.AddWithValue("@LabCase", newLabCaseNumber);
                    command.Parameters.AddWithValue("@OffenseDate", newReportIncidentDate);
                    command.Parameters.AddWithValue("@CaseKey", caseKey); //

                    command.ExecuteNonQuery();
                }

                connection.Close();
            }

            BindGrid();
            Bind(connectionString, queryDdlDept, ddlDepartment, departmentDataTextField, departmentDataValueField);
            Bind(connectionString, queryDdlOffense, ddlCharge, chargeDataTextField, chargeDataValueField);

            EnableControls(false, false, false, false, false, false, false, true);
        }

        protected void BtnCancel_Click(object sender, EventArgs e)
        {
            string connectionString = GetConnectionString();

            int rowIndex = GridView1.SelectedIndex;
            GridViewRow row = GridView1.Rows[rowIndex];

            string selectedDepartmentText = row.Cells[1].Text;
            string selectedChargeText = row.Cells[2].Text;

            txtDepartmentCaseNumber.Text = row.Cells[0].Text;
            txtLabCaseNumber.Text = row.Cells[3].Text;
            txtReportIncidentDate.Text = row.Cells[4].Text;

            DdlSelector(ddlCharge, selectedChargeText);
            DdlSelector(ddlDepartment, selectedDepartmentText);
            
            EnableControls(false, false, false, false, false, false, false, true);
        }
    }
}