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
        /// Query declarations for grid view and Dropdown lists
        private string queryGridView1 = @"SELECT TOP 10 C.CASE_KEY, C.DEPARTMENT_CASE_NUMBER, D.DEPARTMENT_NAME, 
                            O.OFFENSE_DESCRIPTION AS CHARGE, LAB_CASE, OFFENSE_DATE 
                            FROM TV_LABCASE C 
                            INNER JOIN TV_DEPTNAME D ON C.DEPARTMENT_CODE = D.DEPARTMENT_CODE 
                            INNER JOIN TV_OFFENSE O ON C.OFFENSE_CODE = O.OFFENSE_CODE 
                            ORDER BY CASE_DATE DESC";

        private string queryDdlDept = "SELECT DEPARTMENT_NAME, DEPARTMENT_CODE FROM TV_DEPTNAME";
        private string queryDdlOffense = "SELECT OFFENSE_DESCRIPTION, OFFENSE_CODE FROM TV_OFFENSE";

        /// Field Names from table
        private string departmentDataTextField = "DEPARTMENT_NAME";
        private string departmentDataValueField = "DEPARTMENT_CODE";
        private string chargeDataTextField = "OFFENSE_DESCRIPTION";
        private string chargeDataValueField = "OFFENSE_CODE";

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
            Bind(connectionString, queryGridView1, GridView1);
        }

        /// <summary>
        /// Data binder for grid and drop down list data from db
        /// </summary>
        /// <param name="ConnectionString"></param>
        /// <param name="query"></param>
        /// <param name="control"></param>
        /// <param name="textField"></param>
        /// <param name="valueField"></param>
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
        /// Grid view row data bound function, enables row click if IsEditing = false, disables row clicking if IsEditing = true
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (ViewState["IsEditing"] == null || !(bool)ViewState["IsEditing"])
                {
                    e.Row.Attributes["onclick"] = Page.ClientScript.GetPostBackClientHyperlink(GridView1, "Select$" + e.Row.RowIndex);
                    e.Row.Attributes["style"] = "cursor:pointer"; 
                }
                else
                {
                    //disables non-selected rows
                    if (GridView1.SelectedIndex != e.Row.RowIndex)
                    {
                        e.Row.CssClass = "disabled"; 
                    }
                    else
                    {
                        e.Row.CssClass = ""; 
                    }
                }
            }
        }

        /// <summary>
        /// Grid view row command
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string connectionString = GetConnectionString();

            //Binds drop down lists
            Bind(connectionString, queryDdlDept, ddlDepartment, departmentDataTextField, departmentDataValueField);
            Bind(connectionString, queryDdlOffense, ddlCharge, chargeDataTextField, chargeDataValueField);

            if (e.CommandName == "Select")
            {
                //Display grid view row data into controls
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                GridToControls(rowIndex);

                BindGrid();
            }
        }

        /// <summary>
        /// Edit button click functionality 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void BtnEdit_Click(object sender, EventArgs e)
        {
            //Check if there are any selected element in the grid view
            if (GridView1.SelectedIndex >= 0)
            {
                //Set IsEditing to true to remove onclick function that triggers postbacks 
                ViewState["IsEditing"] = true;
                EnableControls(true, true, true, true, true, true, true, false);
                BindGrid();
            }
        }

        /// <summary>
        /// Save button click functionality
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void BtnSave_Click(object sender, EventArgs e)
        {
            //Set IsEditing to false to enable onclick function that triggers postbacks 
            ViewState["IsEditing"] = false;
            string connectionString = GetConnectionString();
            // Retrieve values from textboxes, dropdown lists, and DataKeyNames
            string newDepartmentCaseNumber = txtDepartmentCaseNumber.Text;
            string newDepartmentCode = ddlDepartment.SelectedValue;
            string newOffenseCode = ddlCharge.SelectedValue;
            string newLabCaseNumber = txtLabCaseNumber.Text;
            string newReportIncidentDate = txtReportIncidentDate.Text.Trim();
            string caseKey = GridView1.SelectedDataKey.Value.ToString();

            //txtReportIncidentDate textbox validation
            string dateFormat = "yyyy-MM-dd";
            bool isValidDate = DateTime.TryParseExact(newReportIncidentDate, dateFormat,
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out DateTime parsedReportIncidentDate);

            if (isValidDate && string.IsNullOrEmpty(newLabCaseNumber) && string.IsNullOrEmpty(newDepartmentCaseNumber))
            {
                lblReportIncidentDateMessage.Text = "";
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
                        command.Parameters.AddWithValue("@OffenseDate", parsedReportIncidentDate);
                        command.Parameters.AddWithValue("@CaseKey", caseKey); //

                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            else
            {
                lblReportIncidentDateMessage.Text = "Please enter the date in the format yyyy-MM-dd.";
                lblLabCaseNumberMessage.Text = "Input is required.";
                lblDepartmentCaseNumberMessage.Text = "Input is required.";
            }
            //Rebind Grid View
            Bind(connectionString, queryGridView1, GridView1);
            EnableControls(false, false, false, false, false, false, false, true);
        }

        /// <summary>
        /// Cancel button click functionality
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void BtnCancel_Click(object sender, EventArgs e)
        {
            //Set IsEditing to false to enable onclick function that triggers postbacks 
            ViewState["IsEditing"] = false;
            string connectionString = GetConnectionString();

            //Display grid view table data to controls
            int rowIndex = GridView1.SelectedIndex;
            GridToControls(rowIndex);

            //Rebind grid view and dropdown list
            Bind(connectionString, queryGridView1, GridView1);
            Bind(connectionString, queryDdlDept, ddlDepartment, departmentDataTextField, departmentDataValueField);
            Bind(connectionString, queryDdlOffense, ddlCharge, chargeDataTextField, chargeDataValueField);

            EnableControls(false, false, false, false, false, false, false, true);
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

        /// <summary>
        /// Diplays grid view row data into controls e.g textboxes and dropdown lists
        /// </summary>
        /// <param name="rowIndex"></param>
        private void GridToControls(int rowIndex)
        {
            GridViewRow row = GridView1.Rows[rowIndex];

            string selectedDepartmentText = row.Cells[1].Text;
            string selectedChargeText = row.Cells[2].Text;

            txtDepartmentCaseNumber.Text = row.Cells[0].Text;
            txtLabCaseNumber.Text = row.Cells[3].Text;
            txtReportIncidentDate.Text = row.Cells[4].Text;

            DdlSelector(ddlCharge, selectedChargeText);
            DdlSelector(ddlDepartment, selectedDepartmentText);
        }

        /// <summary>
        /// Selects dropdown text value based on selected text value from the grid
        /// </summary>
        /// <param name="ddlControl"></param>
        /// <param name="textToSelect"></param>
        private void DdlSelector(DropDownList ddlControl, string textToSelect)
        {
            ListItem selectedItem = ddlControl.Items.FindByText(textToSelect);
            if (selectedItem != null)
            {
                ddlControl.ClearSelection();
                selectedItem.Selected = true;
            }
        }

        /// <summary>
        /// Enables controls e.g textboxes, dropdown lists, and buttons
        /// </summary>
        /// <param name="boolBtnSave"></param>
        /// <param name="boolBtnCancel"></param>
        /// <param name="boolTxtDepartmentCaseNumber"></param>
        /// <param name="boolTxtLabCaseNumber"></param>
        /// <param name="boolTxtReportIncedentDate"></param>
        /// <param name="boolDdlCharge"></param>
        /// <param name="boolDdlDepartment"></param>
        /// <param name="boolBtnEdit"></param>
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
    }
}