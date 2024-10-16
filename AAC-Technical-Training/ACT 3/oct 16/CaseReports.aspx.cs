using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PLCCONTROLS;
using PLCGlobals;
using PLCWebCommon;

namespace BEASTiLIMS
{
    public partial class CaseReports : PageBase
    {

        #region Properties and Declarations

        PLCSessionVars plcSV = new PLCSessionVars();
        PLCMsgBox messageBox = new PLCMsgBox();    

        [Serializable]
        public class Parameter
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public string Field { get; set; }
            public string Lookup { get; set; }
            public string Value { get; set; }
        }

        [Serializable]
        public class CustomReport
        {
            public string REPORTNAME { get; set; }
            public string REPORTFILE { get; set; }
        }

        private List<Parameter> ReportParameters
        {
            get
            {
                object obj = ViewState["ReportParameters"];
                return (obj == null) ? new List<Parameter>() : ViewState["ReportParameters"] as List<Parameter>;
            }
            set
            {
                ViewState["ReportParameters"] = value;
            }
        }

        private enum colGrid
        {
            ReportName = 1
        }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (String.IsNullOrEmpty(plcSV.PLCGlobalCaseKey))
                    Response.Redirect("TAB1Case.aspx");

                //highlight assignments tab
                Menu mymenu = (Menu) Master.FindControl("menu_main");
                mymenu.Items[0].Selected = true;

                // Set page title.
                ((MasterPage) Master).SetCaseTitle("Report for");

                LoadReportList();
            }

            if (grdReports.Rows.Count > 0 && grdReports.SelectedIndex >= 0)
            {
                pnlReportParams.Visible = true;
                LoadReportParameters();
                RebindParameters();    
            }
            else
            {
                pnlReportParams.Visible = false;
            }
            
            
            messageBox = (PLCMsgBox)LoadControl("~/PLCWebCommon/PLCMsgBox.ascx");
            plhMessageBox.Controls.Add(messageBox);
        }

        protected void btnXLSExport_Click(object sender, EventArgs e)
        {
            ViewReport("XLS");
        }

        protected void btnPDFExport_Click(object sender, EventArgs e)
        {
            ViewReport("PDF");
        }

        protected void btnViewReport_Click(object sender, EventArgs e)
        {
            ViewReport("");
        }

        protected void btnShowFormula_Click(object sender, EventArgs e)
        {
            if (grdReports.SelectedIndex >= 0)
            {
                string paramValues = string.Empty;
                string selectFormula = string.Empty;

                GenerateSelectionFormula(out selectFormula, out paramValues);

                messageBox.ShowMsg("Case Report", paramValues + "<br /><br />" + selectFormula, 1);
            }
            else
            {
                messageBox.ShowMsg("Case Report", "Please Select a Report", 1);
            }
        }

        protected void repParameters_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                string lookupTable = ((HiddenField)e.Item.FindControl("hdnLookup")).Value;
                string lookupValue = ((HiddenField)e.Item.FindControl("hdnValue")).Value;

                PlaceHolder plhLookup = (PlaceHolder)e.Item.FindControl("plhLookup");
                if (plhLookup.Visible)
                {
                    CodeHead chLookup = (CodeHead)LoadControl("~/PLCWebCommon/CodeHead.ascx");
                    chLookup.ID = "chLookup";
                    chLookup.TableName = lookupTable;
                    if (lookupValue != "")
                        chLookup.SetValue(lookupValue);

                    //if codehead is LOCATION, link this to codehead CUSTODY
                    if (lookupTable == "CUSTLOC" || lookupTable == "CV_CUSTLOC" || lookupTable == "TV_CUSTLOC")
                    {
                        if (ReportParameters.FirstOrDefault(p => (p.Type == "CODE" && (p.Lookup == "TV_CUSTCODE" || p.Lookup == "CUSTCODE"))
                            || (p.Type == "CODE_ALWAYS" && p.Field.Contains("CUSTODY_OF"))) != null)
                        {
                            chLookup.CodeCondition = "custody_code = {Status Code}";
                            CodeHead chCustody = null;
                            FindCodeHeadControl(ref chCustody, repParameters, new string[] { "TV_CUSTCODE", "CUSTCODE" });
                            chLookup.AddCodeControl(ref chCustody);
                        }
                    }

                    plhLookup.Controls.Add(chLookup);
                }
            }
        }

        protected void grdReports_IndexChanged(object sender, EventArgs e)
        {
            LoadReportParameters();
            RebindParameters();
        }

        #endregion

        #region Methods

        protected void LoadReportList()
        {
            List<CustomReport> customReports = new List<CustomReport>();

            CustomReport customReport = new CustomReport();
            IniReader iniReader = new IniReader(Server.MapPath("~/Reports/Custom/adhoc.ini"));

            iniReader.Section = "CASEREPORTS";
            foreach (string reportName in iniReader.GetKeyNames())
            {
                string reportFile = iniReader.ReadString(reportName);
                customReport = new CustomReport();
                customReport.REPORTNAME = reportName;
                customReport.REPORTFILE = reportFile;

                customReports.Add(customReport);
            }

            grdReports.DataSource = customReports;
            grdReports.DataBind();

            pnlReportButtons.Enabled = (grdReports.Rows.Count) > 0 ? true : false;
            grdReports.SelectedIndex = (grdReports.Rows.Count) > 0 ? 0 : -1;
        }

        protected void LoadReportParameters()
        {
            hdnReportFile.Value = grdReports.SelectedDataKey.Value.ToString();
            ReportParameters = GenerateReportParameters();

            repParameters.DataSource = ReportParameters;
            repParameters.DataBind();

            pnlParameters.Visible = ReportParameters.Count > 0;

            //if (ReportParameters.Where(p => p.Type != "ALWAYS" && p.Type != "CODE_ALWAYS").Count() == 0)
            //{
            //    ViewReport("");
            //}
        }

        protected List<Parameter> GenerateReportParameters()
        {
            string reportName = grdReports.SelectedRow.Cells[colGrid.ReportName.GetHashCode()].Text;
            lblReport.Text = reportName;

            //get parameters of report from adhoc.ini
            List<Parameter> parameters = new List<Parameter>();

            IniReader iniReader = new IniReader(Server.MapPath("~/Reports/Custom/adhoc.ini"));
            iniReader.Section = reportName;
            foreach (string key in iniReader.GetKeyNames())
            {
                string[] attributes = iniReader.ReadString(key).Split('|');
                Parameter p = new Parameter();
                p.Name = key;
                p.Type = (attributes.Length > 0) ? attributes[0].Trim() : "";
                p.Field = (attributes.Length > 1) ? attributes[1].Trim() : "";
                p.Lookup = (attributes.Length > 2) ? attributes[2].Trim() : "";
                p.Value = "";

                
                if (p.Type == "ALWAYS2" && p.Field.Contains("CUSTODY_OF"))
                {
                    string field = p.Field;
                    p.Type = "CODE_ALWAYS";
                    p.Field = field.Substring(0, field.IndexOf("=") + 1).Trim();
                    p.Value = field.Substring(field.IndexOf("=") + 1).Trim().TrimStart('\'').TrimEnd('\'');
                    p.Lookup = "TV_CUSTCODE";
                }

                parameters.Add(p);
            }

            return parameters;
        }

        private void RebindParameters()
        {
            if (repParameters.Items.Count > 0)
            {
                List<Parameter> parameters = ReportParameters;

                for (int i = 0; i < repParameters.Items.Count; i++)
                {
                    RepeaterItem item = repParameters.Items[i];
                    string fieldType = ((HiddenField)item.FindControl("hdnType")).Value;
                    string value = "";
                    switch (fieldType)
                    {
                        case "TEXT":
                            value = ((TextBox)item.FindControl("txtValue")).Text;
                            break;
                        case "CODE":
                        case "CODEALWAYS":
                            break;
                        case "DATE":
                            value = ((TextBox)item.FindControl("txtValue")).Text;
                            break;
                        case "DATERANGE":
                            string dateStart = ((TextBox)item.FindControl("txtDateStart")).Text;
                            string dateEnd = ((TextBox)item.FindControl("txtDateEnd")).Text;
                            value = dateStart + " " + dateEnd;
                            break;
                        case "ALWAYS":
                            value = "";
                            break;
                    }

                    parameters[i].Value = value;
                }

                ReportParameters = parameters;
            }

            repParameters.DataSource = ReportParameters;
            repParameters.DataBind();
        }

        protected string GetDate(string dates, int index)
        {
            string[] dateRange = dates.Split(' ');
            return (dateRange.Length > index) ? dateRange[index] : "";
        }

        protected void FindCodeHeadControl(ref CodeHead chCtrl, Control parent, string[] tableNames)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl.Controls.Count > 0)
                {
                    FindCodeHeadControl(ref chCtrl, ctrl, tableNames);
                }

                if (ctrl is CodeHead && tableNames.Count(c => c == ((CodeHead)ctrl).TableName) > 0)
                {
                    chCtrl = (CodeHead)ctrl;
                    break;
                }
            }
        }

        private void GenerateSelectionFormula(out string extraWhereClause, out string rptDescription)
        {
            string selectionFormula = string.Empty;
            string description = string.Empty;

            try
            {
                //generate selection formula
                foreach (RepeaterItem item in repParameters.Items)
                {
                    string fieldType = ((HiddenField)item.FindControl("hdnType")).Value;
                    string field = ((HiddenField)item.FindControl("hdnField")).Value;
                    string prompt = ((Label)item.FindControl("lblParameter")).Text;
                    string lookup = ((HiddenField)item.FindControl("hdnLookup")).Value;
                    string value;

                    switch (fieldType)
                    {
                        case "TEXT": //format: [field] = 'value'
                            value = ((TextBox)item.FindControl("txtValue")).Text;
                            if (!field.EndsWith("="))
                                field = field + "=";
                            if (value != "")
                            {
                                selectionFormula += " and " + field + "'" + value + "'";
                                description += "<br />- " + prompt + " is '" + value + "'";
                            }
                            break;
                        case "CODE": //format: [field] = 'value'
                        case "CODE_ALWAYS":
                            value = ((CodeHead)item.FindControl("chLookup")).GetValue();
                            string codeDescription = ((CodeHead)item.FindControl("chLookup")).GetDescription();
                            if (!field.EndsWith("="))
                                field = field + "=";
                            if (value != "")
                            {
                                selectionFormula += " and " + field + "'" + value + "'";
                                description += "<br />- " + prompt + " is '" + codeDescription + "'";
                            }
                            break;
                        case "DATE": //format: [date] = Date(y,m,d)
                            if (field.EndsWith("="))
                                field = field.TrimEnd(new char[] { '=' });
                            if (((TextBox)item.FindControl("txtValue")).Text != "")
                            {
                                DateTime dateEntry = Convert.ToDateTime(((TextBox)item.FindControl("txtValue")).Text);
                                selectionFormula += " and " + field + " = Date(" + dateEntry.Year + "," + dateEntry.Month + "," + dateEntry.Day + ")";
                                description += "<br />- " + prompt + " is on " + dateEntry.ToShortDateString();
                            }
                            break;
                        case "DATERANGE": //format: [datestart] >= Date(y,m,d) and [dateend] <= Date(y,m,d)
                            if (field.EndsWith("="))
                                field = field.TrimEnd(new char[] { '=' });
                            string field1 = field;
                            string field2 = field;
                            if (field.Contains("@"))
                            {
                                string[] formulaFields = field.Split(',');
                                field1 = formulaFields[0].Replace("{", "").Replace("}", "");
                                field2 = formulaFields[1].Replace("{", "").Replace("}", "");
                            }

                            string dateStart = ((TextBox)item.FindControl("txtDateStart")).Text;
                            string dateEnd = ((TextBox)item.FindControl("txtDateEnd")).Text;

                            if (dateStart != "")
                            {
                                DateTime dateEntry = Convert.ToDateTime(dateStart);
                                selectionFormula += " and " + field1 + " >= Date(" + dateEntry.Year + "," + dateEntry.Month + "," + dateEntry.Day + ")";
                                description += "<br />- " + prompt + " is from " + dateEntry.ToShortDateString();
                            }

                            if (dateEnd != "")
                            {
                                DateTime dateEntry = Convert.ToDateTime(dateEnd);
                                selectionFormula += " and " + field2 + " <= Date(" + dateEntry.Year + "," + dateEntry.Month + "," + dateEntry.Day + ")";
                                if (dateStart != "")
                                    description += " to " + dateEntry.ToShortDateString();
                                else
                                    description += "<br />- " + prompt + " is until " + dateEntry.ToShortDateString();
                            }

                            break;
                        case "ALWAYS": //format: [field] = [value]
                            if (!field.Contains("@"))
                                selectionFormula += " and " + field;
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(plcSV.PLCGlobalCaseKey))
                {
                    selectionFormula += " and {TV_LABCASE.CASE_KEY} = " + plcSV.PLCGlobalCaseKey;
                    description += "<br />- Case Key is " + plcSV.PLCGlobalCaseKey;
                }

                if (!string.IsNullOrEmpty(plcSV.PLCGlobalECN))
                {
                    selectionFormula += " and {TV_LABITEM.EVIDENCE_CONTROL_NUMBER} = " + plcSV.PLCGlobalECN;
                    description += "<br />- Evidence Control Number is " + plcSV.PLCGlobalECN;
                }

                selectionFormula = (selectionFormula == "") ? "" : selectionFormula.Remove(0, 5);
            }
            catch (Exception ex)
            {
                messageBox.ShowMsg("Case Report", "Error encountered: " + ex.Message, 1);
                extraWhereClause = string.Empty;
                rptDescription = string.Empty;
                return;
            }

            extraWhereClause = selectionFormula;
            rptDescription = description;
        }

        private void ViewReport(string fmt)
        {
            string reportFile = hdnReportFile.Value;
            string selectionFormula = string.Empty;
            string description = string.Empty;

            GenerateSelectionFormula(out selectionFormula, out description);

            plcSV.WriteDebug("Custom Report: " + lblReport.Text + "; FileName: " + reportFile + "; Parameters: " + selectionFormula, true);

            plcSV.PLCCrystalReportName = MapPath("~\\Reports\\" + PLCCommon.instance.CrystalReportSubFolder() + "\\" + reportFile);
            plcSV.PLCCrystalReportTitle = lblReport.Text + (string.IsNullOrEmpty(description) ? "" : description);
            plcSV.PLCCrystalSelectionFormula = selectionFormula;

            //ScriptManager.RegisterClientScriptBlock(this, typeof(Page), "uniqueKey" + DateTime.Now, "window.open('.aspx','_blank');", true);

            string tempstr = "";

            if (fmt != "")
            {
                //tempstr = "~/PLCWebCommon/CustomRptViewer.aspx?FMT=" + fmt;
                tempstr = "~/PLCWebCommon/CustomRptExport.aspx?FMT=" + fmt;
                Response.Redirect(tempstr);
            }
            else
            {
                //Response.Redirect("~/PLCWebCommon/CustomRptViewer.aspx");
                plcSV.PrintCRWReport(true);
            }
        }

        #endregion
    }
}
