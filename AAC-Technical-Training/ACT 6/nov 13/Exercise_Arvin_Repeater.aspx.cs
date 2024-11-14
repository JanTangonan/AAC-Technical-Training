using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
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
                
                bpLabCase.SetBrowseMode();
                BindRepeater();
            }
        }

        public class CaseField
        {
            public string FieldName { get; set; }
            public string DisplayName { get; set; }
            public string FieldValue { get; set; }
            public string ControlType { get; set; } // e.g., "TextBox" or "DropDownList"
            public string CodeTable { get; set; } // Name of the code table if it's a DropDownList
            public List<ListItem> DropDownOptions { get; set; } // Only used if ControlType is DropDownList
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
                var placeholder = (PlaceHolder)e.Item.FindControl("plcControls");

                DataTable configTable = GetControls();

                foreach (DataRow row in configTable.Rows)
                {
                    string fieldName = row["FIELD_NAME"].ToString();
                    string prompt = row["PROMPT"].ToString();
                    bool isMandatory = row["MANDATORY"].ToString() == "T";
                    int displayWidth = Convert.IsDBNull(row["FIELD_DISPLAY_WIDTH"]) ? 0 : Convert.ToInt32(row["FIELD_DISPLAY_WIDTH"]);

                    HtmlGenericControl formRow = new HtmlGenericControl("div");
                    formRow.Attributes["class"] = "form-row";

                    // Add a Label for the prompt
                    Label lblPrompt = new Label { Text = prompt };
                    lblPrompt.CssClass = "form-label";

                    formRow.Controls.Add(lblPrompt);

                    if (isMandatory)
                    {
                        Literal asterisk = new Literal();
                        asterisk.Text = "<span class='required-marker'>*</span>";
                        formRow.Controls.Add(asterisk);
                    }

                    // Determine the control type based on CODE_TABLE
                    if (row["CODE_TABLE"] != DBNull.Value)
                    {
                        FlexBox fb = new FlexBox { ID = "fb" + fieldName };
                        fb.TableName = row["CODE_TABLE"].ToString();
                        fb.Width = displayWidth;
                        fb.CssClass = "form-control";

                        formRow.Controls.Add(fb);
                    }
                    else
                    {
                        TextBox txt = new TextBox { ID = "txt" + fieldName, Width = Unit.Pixel(displayWidth) };
                        txt.Width = 325;
                        txt.CssClass = "form-control";

                        formRow.Controls.Add(txt);
                    }

                    
                    placeholder.Controls.Add(formRow);
                }

                // Bind data to generated controls
                DataRow dataRow = GetSelectedRowData();
                foreach (DataRow row in configTable.Rows)
                {
                    string fieldName = row["FIELD_NAME"].ToString();
                    var control = placeholder.FindControl("txt" + fieldName) ?? placeholder.FindControl("fb" + fieldName);

                    if (control is TextBox txt)
                    {
                        if (fieldName == "OFFENSE_DATE")
                        {
                            txt.CssClass += " datepicker";
                        }
                        txt.Text = dataRow[fieldName].ToString();
                        txt.Enabled = IsEditMode;
                    }
                    else if (control is FlexBox fb)
                    {
                        fb.SelectedValue = dataRow[fieldName].ToString();
                        fb.Enabled = IsEditMode;
                    }
                }
            }
        }

        #region METHODS
        private DataTable GetControls()
        {
            string qry = @"SELECT TABLE_NAME, FIELD_NAME, PROMPT, CODE_TABLE,
                           FIELD_DISPLAY_WIDTH, MANDATORY, MEMO_FIELD_LINES
                           FROM TV_DBPANEL
                           WHERE PANEL_NAME = 'CASETAB' AND REMOVE_FROM_SCREEN = 'F'
                           ORDER BY SEQUENCE";
            PLCQuery qryControls = new PLCQuery(PLCSession.FormatSpecialFunctions(qry));
            qryControls.Open();

            return qryControls.PLCDataTable;
        }

        private DataRow GetSelectedRowData()
        {
            DataTable selectedData = new DataTable();
            string qry = @"SELECT DEPARTMENT_CODE, DEPARTMENT_CASE_NUMBER, DEPARTMENT_CODE, OFFENSE_CODE, LAB_CASE, OFFENSE_DATE,
                           INVESTIGATING_AGENCY, CASE_MANAGER, CASE_ANALYST, OFFENSE_CATEGORY, CASE_STATUS
                           FROM TV_LABCASE WHERE CASE_KEY = " + dbgLabCase.SelectedDataKey.Value.ToString();
            PLCQuery qrySelectedRow = new PLCQuery(PLCSession.FormatSpecialFunctions(qry));
            qrySelectedRow.Open();

            selectedData = qrySelectedRow.PLCDataTable;
            return selectedData.Rows.Count > 0 ? selectedData.Rows[0] : null;
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
                //dlgMessage.ShowAlert("MESSAGE", message);
            }
            else
            {
                return;
            }
        }
        #endregion
    }
}