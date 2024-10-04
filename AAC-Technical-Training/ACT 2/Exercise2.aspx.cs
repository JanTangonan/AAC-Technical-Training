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
                BindGridView();
            }
        }

        protected void BindGrid()
        {
            // Create a DataTable and add some sample data
            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[5] {
                new DataColumn("DepartmentCaseNumber", typeof(string)),
                new DataColumn("Department", typeof(string)),
                new DataColumn("Charge", typeof(string)),
                new DataColumn("LabCaseNumber", typeof(string)),
                new DataColumn("IncidentReportDate", typeof(DateTime))
            });

            // Adding some sample rows
            dt.Rows.Add("DCN12345", "HR", "Charge A", "LCN5678", DateTime.Now.AddDays(-5));
            dt.Rows.Add("DCN67890", "IT", "Charge B", "LCN9101", DateTime.Now.AddDays(-3));
            dt.Rows.Add("DCN23456", "Finance", "Charge C", "LCN1121", DateTime.Now.AddDays(-7));
            dt.Rows.Add("DCN23456", "Finance", "Charge C", "LCN1121", DateTime.Now.AddDays(-7));
            dt.Rows.Add("DCN23456", "Finance", "Charge C", "LCN1121", DateTime.Now.AddDays(-7));

            // Bind the DataTable to the GridView
            GridView1.DataSource = dt;
            GridView1.DataBind();
        }

        /// <summary>
        /// Binds and displays data from DB to GridView
        /// </summary>
        protected void BindGridView()
        {
            string connectionString = GetConnectionString();
            string query = @"SELECT TOP 10 C.CASE_KEY, C.DEPARTMENT_CASE_NUMBER, D.DEPARTMENT_NAME, 
                O.OFFENSE_DESCRIPTION AS CHARGE, LAB_CASE, OFFENSE_DATE 
                FROM TV_LABCASE C 
                INNER JOIN TV_DEPTNAME D ON C.DEPARTMENT_CODE = D.DEPARTMENT_CODE 
                INNER JOIN TV_OFFENSE O ON C.OFFENSE_CODE = O.OFFENSE_CODE 
                ORDER BY CASE_DATE DESC";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataAdapter da = new SqlDataAdapter(command);
                DataTable dt = new DataTable();

                da.Fill(dt);

                GridView1.DataSource = dt;
                GridView1.DataBind();
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

        protected void GridView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Get the selected row
            GridViewRow row = GridView1.SelectedRow;

            // Populate textboxes with the selected row's data
            if (row != null)
            {
                ddlDepertmentCaseNumber.Text = row.Cells[1].Text; // Adjust the index according to your GridView structure
                txtDepertment.Text = row.Cells[2].Text;
                ddlCharge.Text = row.Cells[3].Text;
                txtLabCaseNumber.Text = row.Cells[4].Text;
                txtReportIncidentDate.Text = row.Cells[5].Text; // Make sure the index corresponds to the right column
            }
        }
    }
}