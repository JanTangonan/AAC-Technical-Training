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
        public class DynamicField
        {
            public string TableName { get; set; }
            public string FieldName { get; set; }
            public string Prompt { get; set; }
            public string CodeTable { get; set; }
            public int FieldDisplayWidth { get; set; }
            public bool Mandatory { get; set; }
            public int MemoFieldLines { get; set; }
            public string ControlType { get; set; }  // "TextBox" or "DropDownList"
            public List<string> DropDownOptions { get; set; } // Options for dropdown, if applicable
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Example of fetching data from the database (adjust the connection string and query as necessary)
                string query = @"SELECT TABLE_NAME, FIELD_NAME, PROMPT, CODE_TABLE,
                                        FIELD_DISPLAY_WIDTH, MANDATORY, MEMO_FIELD_LINES
                                FROM TV_DBPANEL
                                WHERE PANEL_NAME = 'CASETAB' AND REMOVE_FROM_SCREEN = 'F'
                                ORDER BY SEQUENCE";

                // Get the data from the database (implement your data access here)
                var fields = GetFieldsFromDatabase(query);

                // Bind the data to the Repeater
                myRepeater.DataSource = fields;
                myRepeater.DataBind();
            }
        }

        private List<DynamicField> GetFieldsFromDatabase(string query)
        {
            List<DynamicField> fields = new List<DynamicField>();

            // Implement database access (use ADO.NET, Entity Framework, or Dapper)
            // For now, here's an example with mock data
            fields.Add(new DynamicField
            {
                FieldName = "Name",
                Prompt = "Full Name",
                ControlType = "TextBox",
                Mandatory = true,
                FieldDisplayWidth = 30
            });
            fields.Add(new DynamicField
            {
                FieldName = "Gender",
                Prompt = "Gender",
                ControlType = "DropDownList",
                CodeTable = "GENDER_CODES", // Assuming the drop-down comes from a code table
                Mandatory = true
            });

            return fields;
        }

        protected void myRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var item = (DynamicField)e.Item.DataItem;
                
                var lblPrompt = (Label)e.Item.FindControl("lblPrompt");
                lblPrompt.Text = item.Prompt;

                if (item.ControlType == "TextBox")
                {
                    var txtBox = (TextBox)e.Item.FindControl("txtInput");
                    txtBox.Text = item.FieldName;  // You can set initial value here if necessary
                }
                else if (item.ControlType == "DropDownList")
                {
                    var ddl = (DropDownList)e.Item.FindControl("ddlInput");
                    
                    // Here you should query the options based on CodeTable (e.g., "GENDER_CODES")
                    List<ListItem> options = GetDropDownOptions(item.CodeTable); 

                    ddl.DataSource = options;
                    ddl.DataTextField = "Text";
                    ddl.DataValueField = "Value";
                    ddl.DataBind();

                    // Set selected value if applicable
                    // ddl.SelectedValue = item.Value;
                }

                // Handle mandatory field (red asterisk)
                var lblMandatory = (Label)e.Item.FindControl("lblMandatory");
                if (item.Mandatory)
                {
                    lblMandatory.Text = "*";
                    lblMandatory.ForeColor = System.Drawing.Color.Red;
                }
            }
        }

        private List<ListItem> GetDropDownOptions(string codeTable)
        {
            var options = new List<ListItem>();

            if (codeTable == "GENDER_CODES")
            {
                options.Add(new ListItem("Male", "M"));
                options.Add(new ListItem("Female", "F"));
            }

            return options;
        }
        #region METHODS
        
        #endregion
    }
}