using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PLCCONTROLS;

namespace AAC_Technical_Training
{
    public partial class Exercise3 : System.Web.UI.Page
    {
        ///// Queries to be used for grid view and Dropdown lists
        private string queryGridView1 = @"SELECT TOP 10 C.CASE_KEY, C.DEPARTMENT_CASE_NUMBER, D.DEPARTMENT_NAME, 
                            O.OFFENSE_DESCRIPTION AS CHARGE, LAB_CASE, OFFENSE_DATE 
                            FROM TV_LABCASE C 
                            INNER JOIN TV_DEPTNAME D ON C.DEPARTMENT_CODE = D.DEPARTMENT_CODE 
                            INNER JOIN TV_OFFENSE O ON C.OFFENSE_CODE = O.OFFENSE_CODE 
                            ORDER BY CASE_DATE DESC";
        //private string queryDdlDept = "SELECT DEPARTMENT_NAME, DEPARTMENT_CODE FROM TV_DEPTNAME";
        //private string queryDdlOffense = "SELECT OFFENSE_DESCRIPTION, OFFENSE_CODE FROM TV_OFFENSE";

        ///// Field Names from tables TV_DEPTNAME and TV_OFFENSE
        //private string departmentDataTextField = "DEPARTMENT_NAME";
        //private string departmentDataValueField = "DEPARTMENT_CODE";
        //private string chargeDataTextField = "OFFENSE_DESCRIPTION";
        //private string chargeDataValueField = "OFFENSE_CODE";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindGrid();
            }

            //PLCButtonPanel1.DisableButton("save");
            //PLCButtonPanel1.DisableButton("cancel");
        }

        protected void grdCasesTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulatePanelFields();
        }

        protected void grdCasesTable_RowDataBound(object sender, EventArgs e)
        {

        }

        protected void grdCasesTable_RowCommand(object sender, EventArgs e)
        {

        }

        protected void PLCDBPanel1_PLCDBPanelButtonClick(object sender, PLCButtonClickEventArgs e)
        {
            if (e.button_name == "EDIT")
            {

            }

            if (e.button_name == "SAVE")
            {
                if (e.button_action == "BEFORE")
                {
                    grdCasesTable.InitializePLCDBGrid();
                }

                if (e.button_action == "AFTER")
                    {
                    grdCasesTable.InitializePLCDBGrid();
                }
            }

            if (e.button_name == "CANCEL")
            {

            }
        }

        private void PopulatePanelFields()
        {
            PLCDBPanel1.PLCWhereClause = " WHERE CASE_KEY = " + grdCasesTable.SelectedDataKey.Value.ToString();
            PLCDBPanel1.DoLoadRecord();
        }

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


        protected void PLCDBPanel1_PLCDBPanelSetDefaultRecord(object sender, PLCDBPanelSetDefaultRecordEventArgs e)
        {
            //e.SetNewRecordValues("DEPARTMENT_CASE_NUMBER", string.Empty);
            //e.SetNewRecordValues("DEPARTMENT_NAME", string.Empty);
            //e.SetNewRecordValues("OFFENSE_DESCRIPTION", string.Empty);
            //e.SetNewRecordValues("LAB_CASE", string.Empty);
            //e.SetNewRecordValues("OFFENSE_DATE", string.Empty);
        }

        //protected void grdCasesTable_RowDataBound(object sender, GridViewRowEventArgs e)
        //{
        //    if (e.Row.RowType == DataControlRowType.DataRow)
        //    {
        //        if (ViewState["IsEditing"] == null || !(bool)ViewState["IsEditing"])
        //        {
        //            e.Row.Attributes["onclick"] = Page.ClientScript.GetPostBackClientHyperlink(grdCasesTable, "Select$" + e.Row.RowIndex);
        //            e.Row.Attributes["style"] = "cursor:pointer";
        //        }
        //        else
        //        {
        //            //disables non-selected rows
        //            if (grdCasesTable.SelectedIndex != e.Row.RowIndex)
        //            {
        //                e.Row.CssClass = "disabled";
        //            }
        //            else
        //            {
        //                e.Row.CssClass = "";
        //            }
        //        }
        //    }
        //}

        //protected void BindGrid()
        //{
        //    Bind(GetConnectionString(), queryGridView1, grdCasesTable);
        //}

        //protected void grdCasesTable_RowDataBound(object sender, GridViewRowEventArgs e)
        //{
        //    if (e.Row.RowType == DataControlRowType.DataRow)
        //    {
        //        if (ViewState["IsEditing"] == null || !(bool)ViewState["IsEditing"])
        //        {
        //            e.Row.Attributes["onclick"] = Page.ClientScript.GetPostBackClientHyperlink(grdCasesTable, "Select$" + e.Row.RowIndex);
        //            e.Row.Attributes["style"] = "cursor:pointer";
        //        }
        //        else
        //        {
        //            //disables non-selected rows
        //            if (grdCasesTable.SelectedIndex != e.Row.RowIndex)
        //            {
        //                e.Row.CssClass = "disabled";
        //            }
        //            else
        //            {
        //                e.Row.CssClass = "";
        //            }
        //        }
        //    }
        //}

        //protected void grdCasesTable_RowCommand(object sender, GridViewCommandEventArgs e)
        //{
        //    //Binds drop down lists
        //    Bind(GetConnectionString(), queryDdlDept, ddlDepartment, departmentDataTextField, departmentDataValueField);
        //    Bind(GetConnectionString(), queryDdlOffense, ddlCharge, chargeDataTextField, chargeDataValueField);

        //    if (e.CommandName == "Select")
        //    {
        //        //Display grid view row data into controls
        //        int rowIndex = Convert.ToInt32(e.CommandArgument);
        //        GridToControls(rowIndex);

        //        BindGrid();
        //    }
        //}

        //protected void btnEdit_Click(object sender, EventArgs e)
        //{
        //    //Check if there are any selected element in the grid view
        //    if (grdCasesTable.SelectedIndex >= 0)
        //    {
        //        //Set IsEditing to true to remove onclick function that triggers postbacks 
        //        ViewState["IsEditing"] = true;
        //        EnableControls(true);
        //        BindGrid();
        //        ClearLabelMessage();
        //    }
        //}

        //protected void btnSave_Click(object sender, EventArgs e)
        //{
        //    //Set IsEditing to false to enable onclick function that triggers postbacks 
        //    ViewState["IsEditing"] = false;

        //    // Retrieve values from textboxes, dropdown lists, and DataKeyNames
        //    string newDepartmentCaseNumber = txtDepartmentCaseNumber.Text;
        //    string newDepartmentCode = ddlDepartment.SelectedValue;
        //    string newOffenseCode = ddlCharge.SelectedValue;
        //    string newLabCaseNumber = txtLabCaseNumber.Text;
        //    string newReportIncidentDate = txtReportIncidentDate.Text.Trim();
        //    string caseKey = grdCasesTable.SelectedDataKey.Value.ToString();

        //    string dateFormat = "yyyy-MM-dd";
        //    bool isValidDate = DateTime.TryParseExact(newReportIncidentDate, dateFormat,
        //                System.Globalization.CultureInfo.InvariantCulture,
        //                System.Globalization.DateTimeStyles.None, out DateTime parsedReportIncidentDate);

        //    //txtReportIncidentDate validation
        //    if (!isValidDate || string.IsNullOrWhiteSpace(newReportIncidentDate))
        //    {
        //        lblReportIncidentDateMessage.Text = "Please enter the date in the format yyyy-MM-dd.";
        //        return;
        //    }
        //    // Update TV_LABCASE
        //    using (SqlConnection connection = new SqlConnection(GetConnectionString()))
        //    {
        //        connection.Open();

        //        string updateLabcaseQuery = @"
        //            UPDATE TV_LABCASE
        //            SET 
        //                DEPARTMENT_CASE_NUMBER = @DepartmentCaseNumber,
        //                DEPARTMENT_CODE = @DepartmentCode,
        //                OFFENSE_CODE = @OffenseCode,
        //                LAB_CASE = @LabCase,
        //                OFFENSE_DATE = @OffenseDate
        //                WHERE CASE_KEY = @CaseKey";

        //        try
        //        {
        //            using (SqlCommand command = new SqlCommand(updateLabcaseQuery, connection))
        //            {
        //                command.Parameters.AddWithValue("@DepartmentCaseNumber", newDepartmentCaseNumber);
        //                command.Parameters.AddWithValue("@DepartmentCode", newDepartmentCode);
        //                command.Parameters.AddWithValue("@OffenseCode", newOffenseCode);
        //                command.Parameters.AddWithValue("@LabCase", newLabCaseNumber);
        //                command.Parameters.AddWithValue("@OffenseDate", parsedReportIncidentDate);
        //                command.Parameters.AddWithValue("@CaseKey", caseKey); //

        //                command.ExecuteNonQuery();
        //                lblLabCaseNumberMessage.Text = " ";
        //            }
        //        }
        //        catch (SqlException ex)
        //        {
        //            if (ex.Number == 2627)
        //            {
        //                lblLabCaseNumberMessage.Text = "Duplicate value entry. Please input different Lab Case";
        //                return;
        //            }
        //        }

        //        connection.Close();
        //    }

        //    lblReportIncidentDateMessage.Text = " ";

        //    //Rebind Grid View
        //    Bind(GetConnectionString(), queryGridView1, grdCasesTable);
        //    EnableControls(false);
        //}

        //protected void btnCancel_Click(object sender, EventArgs e)
        //{
        //    //Set IsEditing to false to enable onclick function that triggers postbacks 
        //    ViewState["IsEditing"] = false;

        //    //Display grid view table data to controls
        //    int rowIndex = grdCasesTable.SelectedIndex;
        //    GridToControls(rowIndex);

        //    //Rebind grid view
        //    Bind(GetConnectionString(), queryGridView1, grdCasesTable);

        //    EnableControls(false);
        //    ClearLabelMessage();
        //}

        /// <summary>
        /// Data binder for grid and dropdownlist data from db
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

        ///// <summary>
        ///// Diplays grid view row data into controls e.g textboxes and dropdown lists
        ///// </summary>
        ///// <param name="rowIndex"></param>
        //private void GridToControls(int rowIndex)
        //{
        //    GridViewRow row = grdCasesTable.Rows[rowIndex];

        //    string selectedDepartmentText = row.Cells[1].Text;
        //    string selectedChargeText = row.Cells[2].Text;

        //    txtDepartmentCaseNumber.Text = row.Cells[0].Text;
        //    txtLabCaseNumber.Text = row.Cells[3].Text;
        //    txtReportIncidentDate.Text = row.Cells[4].Text;

        //    DdlSelector(ddlCharge, selectedChargeText);
        //    DdlSelector(ddlDepartment, selectedDepartmentText);
        //}

        ///// <summary>
        ///// Selects dropdown text value based on selected text value from the grid
        ///// </summary>
        ///// <param name="ddlControl"></param>
        ///// <param name="textToSelect"></param>
        //private void DdlSelector(DropDownList ddlControl, string textToSelect)
        //{
        //    ListItem selectedItem = ddlControl.Items.FindByText(textToSelect);
        //    if (selectedItem != null)
        //    {
        //        ddlControl.ClearSelection();
        //        selectedItem.Selected = true;
        //    }
        //}

        ///// <summary>
        ///// Enables controls e.g textboxes, dropdown lists, and buttons
        ///// </summary>
        ///// <param name="enable"></param>
        //private void EnableControls(bool enable)
        //{
        //    btnSave.Enabled = enable;
        //    btnCancel.Enabled = enable;

        //    txtDepartmentCaseNumber.Enabled = enable;
        //    txtLabCaseNumber.Enabled = enable;
        //    txtReportIncidentDate.Enabled = enable;

        //    ddlCharge.Enabled = enable;
        //    ddlDepartment.Enabled = enable;

        //    btnEdit.Enabled = !enable;
        //}
        ///// <summary>
        ///// Clears label messages for txtLabCaseNumber and txtReportIncidentDate
        ///// </summary>
        //private void ClearLabelMessage()
        //{
        //    lblLabCaseNumberMessage.Text = " ";
        //    lblReportIncidentDateMessage.Text = " ";
        //}
    }
}