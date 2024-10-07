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
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindGridDdl();
            }
        }

        /// <summary>
        /// Binds and displays data from DB to GridView
        /// </summary>
        protected void BindGridDdl()
        {
            string connectionString = GetConnectionString("VADFS");

            string queryGridView1 = @"SELECT TOP 10 C.CASE_KEY, C.DEPARTMENT_CASE_NUMBER, D.DEPARTMENT_NAME, 
                            O.OFFENSE_DESCRIPTION AS CHARGE, LAB_CASE, OFFENSE_DATE 
                            FROM TV_LABCASE C 
                            INNER JOIN TV_DEPTNAME D ON C.DEPARTMENT_CODE = D.DEPARTMENT_CODE 
                            INNER JOIN TV_OFFENSE O ON C.OFFENSE_CODE = O.OFFENSE_CODE 
                            ORDER BY CASE_DATE DESC";
            string queryDdlDept = "SELECT DEPARTMENT_NAME, DEPARTMENT_CODE FROM TV_DEPTNAME";
            string queryDdlOffense = "SELECT OFFENSE_DESCRIPTION, OFFENSE_CODE FROM TV_OFFENSE";

            string departmentDataTextField = "DEPARTMENT_NAME";
            string departmentDataValueField = "DEPARTMENT_CODE";
            string chargeDataTextField = "OFFENSE_DESCRIPTION";
            string chargeDataValueField = "OFFENSE_CODE";

            Bind(connectionString, queryGridView1, GridView1);
            Bind(connectionString, queryDdlDept, ddlDepartment, departmentDataTextField, departmentDataValueField);
            Bind(connectionString, queryDdlOffense, ddlCharge, chargeDataTextField, chargeDataValueField);
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

                    // Insert an empty item at index 0 with no text and no value
                    //dropDownList.Items.Insert(0, new ListItem("-", ""));

                }
            }
        }

        /// <summary>
        /// Connection string declaration.
        /// </summary>
        /// <returns type="string">Returns the Connection String</returns>
        private string GetConnectionString(string DataBaseName)
        {
            string serverName = "LOCALHOST";
            string databaseName = DataBaseName;
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
                e.Row.Attributes["style"] = "cursor:pointer"; // Change cursor to pointer on hover
            }
        }

        protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
        { 
            if (e.CommandName == "Select")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                GridViewRow row = GridView1.Rows[rowIndex];

                string DepartmentText = row.Cells[1].Text;
                string ChargeText = row.Cells[2].Text;

                txtDepartmentCaseNumber.Text = row.Cells[0].Text;
                txtDepartment.Text = row.Cells[1].Text;
                txtCharge.Text = row.Cells[2].Text;
                txtLabCaseNumber.Text = row.Cells[3].Text;
                txtReportIncidentDate.Text = row.Cells[4].Text;

                ddlSelector(ddlCharge, ChargeText);
                ddlSelector(ddlDepartment, DepartmentText);
            }
        }

        private void ddlSelector(DropDownList ddlControl, string textToSelect)
        {
            if (ddlControl.Items.Count == 0)
            {
                // Handle the situation when there are no items in the dropdown
                return; // or log an error, throw an exception, etc.
            }

            // Find the item by text
            ListItem selectedItem = ddlControl.Items.FindByText(textToSelect);
            if (selectedItem != null)
            {
                ddlControl.ClearSelection(); // Clear any previous selection
                selectedItem.Selected = true; // Set the found item as selected
            }
            else
            {
                // Optional: Handle case where no matching item was found
                // For example, log a message or set a default value
                Console.WriteLine($"No matching item found for: {textToSelect}");
            }
        }

    }
}