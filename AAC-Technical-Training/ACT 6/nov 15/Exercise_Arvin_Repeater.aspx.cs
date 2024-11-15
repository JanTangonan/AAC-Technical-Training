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
        #region PROPERTIES AND MODELS
        private bool IsEditMode
        {
            get { return (bool)(ViewState["IsEditMode"] ?? false); }
            set { ViewState["IsEditMode"] = value; }
        }

        public class FieldData
        {
            public string FieldName { get; set; }
            public string Value { get; set; }
        }
        #endregion

        #region EVENTS
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindGrid();
                bpLabCase.SetBrowseMode();
                BindRep();
                PopulateRepeaterControls();
                SetControlEditMode();
            }
        }
        
        protected void bpLabCase_PLCButtonClick(object sender, PLCCONTROLS.PLCButtonClickEventArgs e)
        {
            
            if (e.button_name == "EDIT" && e.button_action == "AFTER")
            {
                dbgLabCase.SetControlMode(false);
                BindRep();
                PopulateRepeaterControls();
                IsEditMode = true;
                SetControlEditMode();
            }

            if (e.button_name == "SAVE" && e.button_action == "AFTER")
            {
                ValidateAndSaveInput();
                dbgLabCase.InitializePLCDBGrid();
                dbgLabCase.SetControlMode(true);
                BindRep();
                PopulateRepeaterControls();
                IsEditMode = false;
                SetControlEditMode();
            }

            if (e.button_name == "CANCEL" && e.button_action == "AFTER")
            {
                dbgLabCase.SetControlMode(true);
                BindRep();
                PopulateRepeaterControls();
                IsEditMode = false;
                SetControlEditMode();
            }
        }

        protected void dbgLabCase_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateRepeaterControls();
        }
        #endregion

        #region METHODS
        /// <summary>
        /// Sets control edit mode
        /// </summary>
        private void SetControlEditMode()
        {
            foreach (RepeaterItem item in repLabCaseControl.Items)
            {
                // Find the controls inside each Repeater item
                TextBox txtInput = item.FindControl("txtInput") as TextBox;
                FlexBox flexBoxInput = item.FindControl("flexBoxInput") as FlexBox;

                if (txtInput != null)
                {
                    txtInput.Enabled = IsEditMode;
                }

                if (flexBoxInput != null)
                {
                    flexBoxInput.Enabled = IsEditMode;
                }
            }
        }

        /// <summary>
        /// Validate and Save input data to db
        /// </summary>
        protected void ValidateAndSaveInput()
        {
            bool isValid = true;

            foreach (RepeaterItem item in repLabCaseControl.Items)
            {
                HiddenField hfMandatory = (HiddenField)item.FindControl("hfMandatory");
                string mandatory = hfMandatory?.Value;

                TextBox txtBox = item.FindControl("txtInput") as TextBox;
                FlexBox flexBox = item.FindControl("flexBoxInput") as FlexBox;

                if (txtBox != null && txtBox.Visible && mandatory == "T")
                {
                    if (string.IsNullOrWhiteSpace(txtBox.Text))
                    {
                        isValid = false;
                    }
                }

                if (flexBox != null && flexBox.Visible && mandatory == "T")
                {
                    if (string.IsNullOrWhiteSpace(flexBox.SelectedValue))
                    {
                        isValid = false;
                    }
                }
            }

            if (!isValid)
            {
                string message = "Input required on marked controls.";
                dlgMessage.ShowAlert("MESSAGE", message);
                return;
            }
            else
            {
                List<FieldData> fieldDataList = CollectFieldData();
                SaveFieldDataToDatabase(fieldDataList);
            }
        }

        /// <summary>
        /// Populates Repeater Controls
        /// </summary>
        private void PopulateRepeaterControls()
        {
            DataRow rowData = GetSelectedRowData();

            if (rowData == null) return;
            foreach (RepeaterItem item in repLabCaseControl.Items)
            {
                HiddenField hfFieldName = (HiddenField)item.FindControl("hfFieldName");
                string fieldName = hfFieldName?.Value;

                if (string.IsNullOrEmpty(fieldName) || !rowData.Table.Columns.Contains(fieldName))
                    continue;

                TextBox txtBox = item.FindControl("txtInput") as TextBox;
                FlexBox flexBox = item.FindControl("flexBoxInput") as FlexBox;

                if (txtBox != null && txtBox.Visible)
                {
                    txtBox.Text = rowData[fieldName].ToString();
                }
                else if (flexBox != null && flexBox.Visible)
                {
                    flexBox.SelectedValue = rowData[fieldName].ToString();
                }
            }
        }

        /// <summary>
        /// Gets selected row data and returns first row
        /// </summary>
        /// <returns>Returns first row of the data table</returns>
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

        /// <summary>
        /// Binds data to DbgLabCase 
        /// </summary>
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

        /// <summary>
        /// Binds data to repLabCaseControl
        /// </summary>
        private void BindRep()
        {
            PLCQuery qry = new PLCQuery();
            qry.SQL = @"SELECT TABLE_NAME, FIELD_NAME, PROMPT, CODE_TABLE,
                       FIELD_DISPLAY_WIDTH, MANDATORY, MEMO_FIELD_LINES
                       FROM TV_DBPANEL
                       WHERE PANEL_NAME = 'CASETAB' AND REMOVE_FROM_SCREEN = 'F'
                       ORDER BY SEQUENCE";
            qry.Open();

            if (!qry.IsEmpty())
            {
                repLabCaseControl.DataSource = qry.PLCDataTable;
                repLabCaseControl.DataBind();
            }
        }

        /// <summary>
        /// Collects all data from the repeater controls
        /// </summary>
        /// <returns>collected field data list</returns>
        private List<FieldData> CollectFieldData()
        {
            var fieldDataList = new List<FieldData>();

            foreach (RepeaterItem item in repLabCaseControl.Items)
            {
                HiddenField hfFieldName = (HiddenField)item.FindControl("hfFieldName");
                string fieldName = hfFieldName?.Value;
                string value = string.Empty;

                if (string.IsNullOrEmpty(fieldName)) continue;

                TextBox txtBox = item.FindControl("txtInput") as TextBox;
                FlexBox flexBox = item.FindControl("flexBoxInput") as FlexBox;

                if (txtBox != null && txtBox.Visible)
                {
                    value = txtBox.Text;
                }
                else if (flexBox != null && flexBox.Visible)
                {
                    value = flexBox.SelectedValue;
                }

                if (!string.IsNullOrEmpty(value))
                {
                    fieldDataList.Add(new FieldData { FieldName = fieldName, Value = value });
                }
            }

            return fieldDataList;
        }

        /// <summary>
        /// Saves collected field data list into DB
        /// </summary>
        /// <param name="fieldDataList"></param>
        private void SaveFieldDataToDatabase(List<FieldData> fieldDataList)
        {
            try
            {
                PLCQuery qry = new PLCQuery();
                qry.SQL = "SELECT * FROM TV_LABCASE WHERE CASE_KEY = " + dbgLabCase.SelectedDataKey.Value.ToString();
                qry.Open();
                if (qry.HasData())
                {
                    qry.Edit();
                    foreach (var fieldData in fieldDataList)
                    {
                        qry.SetFieldValue(fieldData.FieldName, fieldData.Value);
                    }
                    qry.Post("TV_LABCASE");
                }

                Console.WriteLine("Save sucessful");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }
        #endregion
    }
}