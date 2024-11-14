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

        private Dictionary<string, object> SelectedInputData { get; set; } = new Dictionary<string, object>();

        private void SaveSelectedInputData()
        {
            SelectedInputData.Clear();

            foreach (RepeaterItem item in repLabCaseControl.Items)
            {
                if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
                {
                    PlaceHolder placeholder = (PlaceHolder)item.FindControl("plcControls");

                    foreach (Control control in placeholder.Controls)
                    {
                        if (control is FlexBox fb)
                        {
                            string fieldName = fb.ID.Replace("fb", "");
                            SelectedInputData[fieldName] = fb.SelectedValue;
                        }
                        else if (control is TextBox txt)
                        {
                            string fieldName = txt.ID.Replace("txt", "");
                            SelectedInputData[fieldName] = txt.Text;
                        }
                    }
                }
            }
        }

        //private void GenerateDynamicControls()
        //{
        //    foreach (RepeaterItem item in repLabCaseControl.Items)
        //    {
        //        PlaceHolder placeholder = (PlaceHolder)item.FindControl("plcControls");
        //        placeholder.Controls.Clear(); // Clear existing controls to avoid duplication

        //        // Assuming you know what controls to create based on your requirements
        //        FlexBox fbDEPARTMENT_CODE = new FlexBox { ID = "fbDEPARTMENT_CODE" };
        //        TextBox txtDEPARTMENT_CASE_NUMBER = new TextBox { ID = "txtDEPARTMENT_CASE_NUMBER" };
        //        FlexBox fbINVESTIGATING_AGENCY = new FlexBox { ID = "fbINVESTIGATING_AGENCY" };
        //        FlexBox fbCASE_MANAGER = new FlexBox { ID = "fbCASE_MANAGER" };
        //        FlexBox fbCASE_ANALYST = new FlexBox { ID = "fbCASE_ANALYST" };
        //        FlexBox fbOFFENSE_CATEGORY = new FlexBox { ID = "fbOFFENSE_CATEGORY" };
        //        TextBox txtOFFENSE_DATE = new TextBox { ID = "txtOFFENSE_DATE" };
        //        TextBox txtCASE_STATUS = new TextBox { ID = "txtCASE_STATUS" };
        //        // ... add other controls as needed

        //        placeholder.Controls.Add(fbDEPARTMENT_CODE);
        //        placeholder.Controls.Add(txtDEPARTMENT_CASE_NUMBER);
        //        placeholder.Controls.Add(fbINVESTIGATING_AGENCY);
        //        placeholder.Controls.Add(fbCASE_MANAGER);
        //        placeholder.Controls.Add(fbCASE_ANALYST);
        //        placeholder.Controls.Add(fbOFFENSE_CATEGORY);
        //        placeholder.Controls.Add(txtOFFENSE_DATE);
        //        placeholder.Controls.Add(txtCASE_STATUS);
        //        // Add other controls to the placeholder as needed
        //    }
        //}

        private Dictionary<string, object> CaptureCurrentInputData()
        {
            var inputData = new Dictionary<string, object>();

            foreach (RepeaterItem item in repLabCaseControl.Items)
            {
                PlaceHolder placeholder = (PlaceHolder)item.FindControl("plcControls");

                foreach (Control control in placeholder.Controls)
                {
                    if (control is FlexBox fb)
                    {
                        inputData[fb.ID] = fb.SelectedValue;
                    }
                    else if (control is TextBox txt)
                    {
                        inputData[txt.ID] = txt.Text;
                    }
                }
            }
            return inputData;
        }

        private void ReapplyInputData(Dictionary<string, object> inputData)
        {
            foreach (RepeaterItem item in repLabCaseControl.Items)
            {
                PlaceHolder placeholder = (PlaceHolder)item.FindControl("plcControls");

                foreach (Control control in placeholder.Controls)
                {
                    if (control is FlexBox fb && inputData.ContainsKey(fb.ID))
                    {
                        fb.SelectedValue = inputData[fb.ID]?.ToString();
                    }
                    else if (control is TextBox txt && inputData.ContainsKey(txt.ID))
                    {
                        txt.Text = inputData[txt.ID]?.ToString();
                    }
                }
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
                SaveSelectedInputData();
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
                        txt.Text = DataBinder.Eval(e.Item.DataItem, fieldName).ToString();
                        txt.Enabled = IsEditMode;
                    }
                    else if (control is FlexBox fb)
                    {
                        fb.SelectedValue = DataBinder.Eval(e.Item.DataItem, fieldName).ToString();
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
            string qry = @"SELECT DEPARTMENT_CODE, 
                           DEPARTMENT_CASE_NUMBER, 
                           INVESTIGATING_AGENCY, 
                           CASE_MANAGER, 
                           CASE_ANALYST, 
                           OFFENSE_CATEGORY, 
                           OFFENSE_DATE, 
                           CASE_STATUS
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
            qry.SQL = @"SELECT DEPARTMENT_CODE, 
                        DEPARTMENT_CASE_NUMBER, 
                        INVESTIGATING_AGENCY, 
                        CASE_MANAGER, 
                        CASE_ANALYST, 
                        OFFENSE_CATEGORY, 
                        OFFENSE_DATE, 
                        CASE_STATUS
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
            BindRepeater();

            foreach (RepeaterItem item in repLabCaseControl.Items)
            {
                if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
                {
                    FlexBox fbDEPARTMENT_CODE = (FlexBox)item.FindControl("fbDEPARTMENT_CODE");
                    TextBox txtDEPARTMENT_CASE_NUMBER = (TextBox)item.FindControl("txtDEPARTMENT_CASE_NUMBER");
                    FlexBox fbINVESTIGATING_AGENCY = (FlexBox)item.FindControl("fbINVESTIGATING_AGENCY");
                    FlexBox fbCASE_MANAGER = (FlexBox)item.FindControl("fbCASE_MANAGER");
                    FlexBox fbCASE_ANALYST = (FlexBox)item.FindControl("fbCASE_ANALYST");
                    FlexBox fbOFFENSE_CATEGORY = (FlexBox)item.FindControl("fbOFFENSE_CATEGORY");
                    TextBox txtOFFENSE_DATE = (TextBox)item.FindControl("txtOFFENSE_DATE");
                    TextBox txtCASE_STATUS = (TextBox)item.FindControl("txtCASE_STATUS");

                    string DEPARTMENT_CODE = fbDEPARTMENT_CODE.SelectedValue;
                    string DEPARTMENT_CASE_NUMBER = txtDEPARTMENT_CASE_NUMBER.Text;
                    string INVESTIGATING_AGENCY = fbINVESTIGATING_AGENCY.SelectedValue;
                    string CASE_MANAGER = fbCASE_MANAGER.SelectedValue;
                    string CASE_ANALYST = fbCASE_ANALYST.SelectedValue;
                    string OFFENSE_CATEGORY = fbOFFENSE_CATEGORY.SelectedValue;
                    string OFFENSE_DATE = txtOFFENSE_DATE.Text;
                    string CASE_STATUS = txtCASE_STATUS.Text;

                    SaveData(DEPARTMENT_CODE, DEPARTMENT_CASE_NUMBER, INVESTIGATING_AGENCY, CASE_MANAGER, CASE_ANALYST,
                        OFFENSE_CATEGORY, OFFENSE_DATE, CASE_STATUS);
                }
            }
        }

        private string SaveData(string DEPARTMENT_CODE, string DEPARTMENT_CASE_NUMBER, string INVESTIGATING_AGENCY, string CASE_MANAGER, string CASE_ANALYST,
             string OFFENSE_CATEGORY, string OFFENSE_DATE, string CASE_STATUS)
        {
            try
            {
                PLCQuery qry = new PLCQuery();
                qry.SQL = "SELECT * FROM TV_LABCASE WHERE CASE_KEY = " + dbgLabCase.SelectedDataKey.Value.ToString();
                qry.Open();
                if (qry.HasData())
                {
                    qry.Edit();
                    qry.SetFieldValue("DEPARTMENT_CODE", DEPARTMENT_CODE);
                    qry.SetFieldValue("DEPARTMENT_CASE_NUMBER", DEPARTMENT_CASE_NUMBER);
                    qry.SetFieldValue("INVESTIGATING_AGENCY", INVESTIGATING_AGENCY);
                    qry.SetFieldValue("CASE_MANAGER", CASE_MANAGER);
                    qry.SetFieldValue("CASE_ANALYST", CASE_ANALYST);
                    qry.SetFieldValue("OFFENSE_CATEGORY", OFFENSE_CATEGORY);
                    qry.SetFieldValue("OFFENSE_DATE", OFFENSE_DATE);
                    qry.SetFieldValue("CASE_STATUS", CASE_STATUS);

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