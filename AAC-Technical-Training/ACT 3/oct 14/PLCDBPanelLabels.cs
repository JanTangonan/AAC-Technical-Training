using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using AjaxControlToolkit;
using System.Drawing;

namespace PLCCONTROLS
{
    public class PLCDBPanelLabels : Panel
    {
        #region Properties

        public string Title
        {
            get
            {
                return this.GroupingText;
            }
            set
            {
                this.GroupingText = value;
            }
        }

        public string TableName
        {
            get
            {
                return Convert.ToString(ViewState["DBPL_TableName"]);
            }
            set
            {
                ViewState["DBPL_TableName"] = value;
            }
        }

        public string PanelName
        {
            get
            {
                return Convert.ToString(ViewState["DBPL_PanelName"]);
            }
            set
            {
                ViewState["DBPL_PanelName"] = value;
            }
        }

        public string WhereClause
        {
            get
            {
                return Convert.ToString(ViewState["DBPL_WhereClause"]);
            }
            set
            {
                ViewState["DBPL_WhereClause"] = value;
            }
        }

        public override Unit Height
        {
            get
            {
                return base.Height;
            }
            set
            {
                base.Height = value;
                this.Visible = value.Value > 0;
            }
        }

        #endregion

        private Unit OHeight {
            get
            {
                if (ViewState["DBPL_OHeight"] != null)
                    return (Unit)ViewState["DBPL_OHeight"];
                return Unit.Empty;
            }
            set
            {
                ViewState["DBPL_OHeight"] = value;
            }
        }

        #region Events

        protected override void OnLoad(EventArgs e)
        {
            if (this.Height.Value == 0)
                return;

            if (OHeight == Unit.Empty)
                OHeight = this.Height;

            this.Height = OHeight;

            Table htmlTable = new Table();
            htmlTable.BorderWidth = 0;
            htmlTable.BorderStyle = BorderStyle.None;
            htmlTable.Style.Add("margin", "10px");

            TableRow htmlTr = null;
            TableCell htmlTd = null;
            Literal litFieldDescription = null;
            string bumpNextFieldUp = "";

            string sqlDbPanel = string.Format(
@"SELECT * FROM TV_DBPANEL WHERE PANEL_NAME = '{0}' AND (REMOVE_FROM_SCREEN <> 'T' OR REMOVE_FROM_SCREEN IS NULL) 
ORDER BY SEQUENCE",
                PanelName);
            PLCQuery qryDbPanel = CacheHelper.OpenCachedSql(sqlDbPanel, PLCSession.GetMetadataCacheDuration());

            string sqlPanelRecord = GenerateSql(qryDbPanel.PLCDataTable);
            if ((sqlPanelRecord == "") || (sqlPanelRecord == null))
                return;
            
            PLCQuery qryPanelRecord = new PLCQuery();
            qryPanelRecord.SQL = sqlPanelRecord;
            qryPanelRecord.Open();
            if (qryPanelRecord.IsEmpty())
                return;

            foreach (DataRow dr in qryDbPanel.PLCDataTable.Rows)
            {
                bool isBumped = (bumpNextFieldUp == "T");
                bool isPacked = (bumpNextFieldUp == "P");
                int colspan = 0;

                try
                {
                    if (dr.Table.Columns.Contains("COLUMN_SPAN"))
                        colspan = Convert.ToInt16(dr["COLUMN_SPAN"]);

                }
                catch
                {
                    colspan = 0;
                }


                bumpNextFieldUp = Convert.ToString(dr["BUMP_NEXT_FIELD_UP"]);
                string tableName = Convert.ToString(dr["TABLE_NAME"]);
                string fieldName = Convert.ToString(dr["FIELD_NAME"]).Replace("\"", "");
                string prompt = Convert.ToString(dr["PROMPT"]);
                string codeTable = Convert.ToString(dr["CODE_TABLE"]);
                string codeDescFormat = Convert.ToString(dr["CODE_DESC_FORMAT"]).Trim(); //flexbox text format
                string codeDescSeparator = Convert.ToString(dr["CODE_DESC_SEPARATOR"]).Trim(); //flexbox separator for code and description
                bool hidePrompt = Convert.ToString(dr["NO_PROMPT"]) == "T";
                bool hideField = Convert.ToString(dr["REMOVE_FROM_SCREEN"]) == "H";

                string fieldValue =  qryPanelRecord.FieldByName(fieldName);

                if (!string.IsNullOrEmpty(codeTable) && !string.IsNullOrEmpty(fieldValue))
                {
                    codeTable = (codeTable.StartsWith("TV") || codeTable.StartsWith("CV") || codeTable.StartsWith("UV") ) ? codeTable : ("TV_" + codeTable);
                    string codeTableSql = PLCSession.GenerateCodeHeadSQL(codeTable, fieldValue, "", codeDescFormat, codeDescSeparator, null, false);
                    PLCQuery qryCodeTable = new PLCQuery(codeTableSql);
                    qryCodeTable.Open();
                    if (qryCodeTable.HasData())
                        fieldValue = qryCodeTable.FieldByIndex(1);
                }

                Panel wrapper = new Panel();
                if (bumpNextFieldUp == "P")
                {
                    wrapper.Style.Add("float", "left");
                    wrapper.Style.Add("padding-right", "5px");
                }

                if (isBumped)
                {
                    litFieldDescription = CreateLabelMarkup(prompt, "", "", fieldValue, hidePrompt);

                    TableCell cell = new TableCell();
                    cell.Controls.Add(litFieldDescription);
                    htmlTr.Cells.Add(cell);
                    htmlTd = new TableCell();

                    if (colspan > 0)
                        htmlTd.ColumnSpan = colspan;


                }
                else if (isPacked)
                {
                    string style = "display:inline-block; vertical-align:30%;";
                    litFieldDescription = CreateLabelMarkup(prompt, style, "packedLabel", fieldValue, hidePrompt);

                    wrapper.Controls.Add(litFieldDescription);
                }
                else
                {
                    htmlTr = new TableRow();
                    htmlTd = new TableCell();
                    htmlTd.BorderWidth = 0;
                    htmlTd.BorderStyle = BorderStyle.None;
                    
                    if (colspan > 0)
                        htmlTd.ColumnSpan = colspan;

                    litFieldDescription = CreateLabelMarkup(prompt, "", "", fieldValue, hidePrompt);

                    htmlTd.Controls.Add(litFieldDescription);
                    htmlTr.Cells.Add(htmlTd);
                    htmlTd = new TableCell();
                }

                htmlTd.Controls.Add(wrapper);
                htmlTr.Cells.Add(htmlTd);

                if (hideField)
                    htmlTr.Style.Value = "visibility: hidden; display: none;";

                htmlTable.Rows.Add(htmlTr);
            }

            Panel mainWrapper = new Panel();
            mainWrapper.Height = this.Height;
            // set parent height to 100%
            this.Height = Unit.Percentage(100);
            mainWrapper.Controls.Add(htmlTable);

            this.Controls.Add(mainWrapper);
        }

        #endregion

        #region Methods

        public void Reload()
        {
            this.Controls.Clear();
            this.OnLoad(EventArgs.Empty);
        }

        public string GenerateSql(DataTable dtDBPanel)
        {
            string sql = "";

            if (TableName == "")
                return "";

            foreach (DataRow dr in dtDBPanel.Rows)
            {
                string fieldName = Convert.ToString(dr["FIELD_NAME"]);
                string tableName = Convert.ToString(dr["TABLE_NAME"]);
                string editMask = Convert.ToString(dr["EDIT_MASK"]);
                
                if (sql != "")
                    sql += ", ";

                if (((tableName == "LABCASE") || (tableName == "TV_LABCASE")) && ((fieldName == "Reference") || (fieldName.ToUpper() == "REFERENCE")))
                {
                    sql += "'*REFERENCE'";
                }
                else
                {
                    if (editMask.StartsWith("99:99") || editMask.ToUpper().StartsWith("HH:MM")) //time
                    {
                        if (PLCSession.PLCDatabaseServer == "MSSQL")
                        {
                            if (editMask.ToUpper() == "HH:MM:SS")
                                sql += "CONVERT(CHAR(8), " + fieldName + ", 108) " + fieldName;
                            else
                                sql += "CONVERT(CHAR(5), " + fieldName + ", 108) " + fieldName;
                        }
                        else
                        {
                            if (editMask.ToUpper() == "HH:MM:SS")
                                sql += "TO_CHAR(" + fieldName + ",'HH24:MI:SS') " + fieldName;
                            else
                                sql += "TO_CHAR(" + fieldName + ",'HH24:MI') " + fieldName;
                        }
                    }
                    else if (IsDateTimeMask(editMask)) //datetime
                    {
                        if (PLCSession.PLCDatabaseServer == "MSSQL") //mssql
                        {
                            if (editMask.ToUpper().Contains(":SS"))
                                sql += "CONVERT(VARCHAR(10), " + fieldName + ", 101) + ' ' + CONVERT(VARCHAR(8), " + fieldName + ", 114) AS " + fieldName; //with seconds
                            else
                                sql += "CONVERT(VARCHAR(10), " + fieldName + ", 101) + ' ' + CONVERT(VARCHAR(5), " + fieldName + ", 114) AS " + fieldName; //without seconds
                        }
                        else //oracle
                        {
                            if (editMask.ToUpper().Contains(":SS"))
                                sql += "TO_CHAR(" + fieldName + ", 'MM/DD/YYYY HH24:MI:SS') AS " + fieldName; //with seconds
                            else
                                sql += "TO_CHAR(" + fieldName + ", 'MM/DD/YYYY HH24:MI') AS " + fieldName; //without seconds
                        }
                    }
                    else if (IsDateMask(editMask)) //date
                    {
                        if (PLCSession.PLCDatabaseServer == "MSSQL") //mssql
                        {
                            sql += "CONVERT(VARCHAR(10), " + fieldName + ", 101) AS " + fieldName;
                        }
                        else //oracle
                        {
                            sql += "TO_CHAR(" + fieldName + ", 'MM/DD/YYYY') AS " + fieldName;
                        }
                    }
                    else
                    {
                        sql += fieldName;
                    }
                }
            }

            if (sql == "")
                sql = " * ";

            sql = "SELECT " + sql + " FROM " + TableName + " " + WhereClause;

            return sql;
        }

        private Literal CreateLabelMarkup(string promptText, string cssStyle, string cssClass, string valueText, bool hidePrompt)
        {
            Literal litPrompt = new Literal();
            litPrompt.Text = String.Format("<span style='{0}' class='{1} prompt default'>{2}<span style='color: navy;'>{3}</span></span>", 
                cssStyle, 
                cssClass, 
                hidePrompt ? "" : (promptText + ":"), 
                " " + (string.IsNullOrEmpty(valueText) ? "-" : valueText));
            return litPrompt;
        }

        private bool IsDateMask(string mask)
        {
            Boolean hasmonth = mask.ToUpper().Contains("MM");
            Boolean hasday = mask.ToUpper().Contains("DD");
            Boolean hasyear = mask.ToUpper().Contains("YY");
            Boolean hasnumbers = mask.ToUpper().Contains("99/99/9999");
            return ((hasmonth && hasday && hasyear) || hasnumbers);
        }

        private bool IsDateTimeMask(string mask)
        {
            bool isTimeMask = mask.ToUpper().Contains("HH:MM") || mask.ToUpper().Contains("99:99");
            return isTimeMask && IsDateMask(mask);
        }

        #endregion
    }
}
