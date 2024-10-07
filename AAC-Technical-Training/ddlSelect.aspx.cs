using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApplication1
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        // Mock data for departments
        private List<Department> departments = new List<Department>
    {
        new Department { DepartmentID = 1, DepartmentName = "HR" },
        new Department { DepartmentID = 2, DepartmentName = "Finance" },
        new Department { DepartmentID = 3, DepartmentName = "IT" }
    };

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Load data into GridView on first load
                LoadGridViewData();
                LoadDropdownData();
            }
        }

        // Loads mock data into GridView
        private void LoadGridViewData()
        {
            gvDepartments.DataSource = departments;
            gvDepartments.DataBind();
        }

        // Loads mock data into Dropdown List
        private void LoadDropdownData()
        {
            ddlDepartments.DataSource = departments;
            ddlDepartments.DataTextField = "DepartmentName";   // Text displayed in dropdown
            ddlDepartments.DataValueField = "DepartmentID";    // Value behind each item
            ddlDepartments.DataBind();

            // Optionally add a default item at the top
            ddlDepartments.Items.Insert(0, new ListItem("--Select Department--", "0"));
        }

        // Event handler for selecting a row in the GridView
        protected void gvDepartments_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Get the selected row
            GridViewRow selectedRow = gvDepartments.SelectedRow;
            string departmentName = selectedRow.Cells[1].Text; // Assume the second cell is DepartmentName

            // Find the corresponding department in the dropdown list and set it as selected
            ListItem selectedItem = ddlDepartments.Items.FindByText(departmentName);
            if (selectedItem != null)
            {
                ddlDepartments.ClearSelection(); // Clear any previous selection
                selectedItem.Selected = true;    // Set the found item as selected
            }
        }

        
    }

    private void ddlSelector(DropDownList ddlControl,string ddl)
    {

        ListItem selectedItem = ddlControl.Items.FindByText(ddl);
        if (selectedItem != null)
        {
            ddlControl.ClearSelection(); // Clear any previous selection
            selectedItem.Selected = true;    // Set the found item as selected
        }

    }

    // Mock class for Department
    public class Department
    {
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
    }
}