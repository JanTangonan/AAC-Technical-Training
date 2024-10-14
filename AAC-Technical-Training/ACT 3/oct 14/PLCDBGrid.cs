#region Assembly references

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Drawing;
using System.Drawing.Design;
using System.ComponentModel;
using System.Resources;
using System.Reflection;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Design;
using System.Text.RegularExpressions;

#endregion using

namespace PLCCONTROLS
{
    public class PLCDBGrid : GridView, IPostBackDataHandler
    {
        public class PLCDBClass
        {
            public const string PLC_ConnectionString = "";     // Provider and Connection Sring (Provider=msdaora;Data Source=PLCORA1;User Id=FDLE;Password=FDLE)
            public const string PLC_Provider = "";             // Provider only (Provider=msdaora)
            public const string PLC_ConnectionStringOnly = ""; // Connection String only (Data Source=PLCORA1;User Id=FDLE;Password=FDLE)
        }

        public Dictionary<string, string> userparams;

        //*AAC 061109
        [Bindable(true)]
        [Browsable(true)]
        [Category("PLC Properties"), Description("CSS Format")]
        [DefaultValue("")]
        [Localizable(true)]
        public static string PLCDBGridCSS { get; set; }

        public enum PLCConnections
        {
            ADO, OLEDB
        }

        public enum PLCFieldType
        {
            BOUND,
            CHECKBOX,
            BUTTON,
            DATE,
            DATEDMY,
            HYPERLINK,
            TIME24,
            TIMEAMPM,
            DATETIME24
        }

        public PLCDBGrid()
        {
            SetPLCGridProperties();
            userparams = new Dictionary<string, string>();
            this.PageIndexChanging += PLCDBGrid_PageIndexChanging;
            this.Sorting += PLCDBGrid_Sorting;
            this.RowCreated += PLCDBGrid_RowCreated;
            this.RowDataBound += PLCDBGrid_RowDataBound;
            this.SelectedIndexChanged += PLCDBGrid_SelectedIndexChanged;
        }

        private bool usesDBGridDL()
        {

            String udl = PLCUSESDL;
            if (udl == "T") return true;
            if (udl == "F") return false;
            if (String.IsNullOrWhiteSpace(PLCGridName))
            {
                PLCUSESDL = "F";
                return false;
            }

            //PLCQuery qry = new PLCQuery("Select * from TV_DBGRIDDL Where GRID_NAME = '" + PLCGridName + "' And HIDE_FIELD <> 'T'");
            //            qry.Open();

            PLCQuery qry = CacheHelper.OpenCachedSqlReadOnly("Select * from TV_DBGRIDDL Where GRID_NAME = '" + PLCGridName + "' And HIDE_FIELD <> 'T'");
            //          qry.Open();


            if (qry.HasData())
            {
                PLCUSESDL = "T";
                return true;
            }
            else
            {
                PLCUSESDL = "F";
                return false;
            }
            // return !qry.IsEmpty();
        }

        protected void PLCDBGrid_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                if (FirstColumnCheckbox)
                {
                    var hfSelect = e.Row.FindControl("cbSelect_state") as HiddenField;
                    hfSelect.Value = "".PadLeft(((DataView)DataSource).Count, '0');
                }
            }
            else if (e.Row.RowType == DataControlRowType.DataRow)
            {
                double totalWidth = 0; // get the width programatically set
                // Skip over the 'Select' button in plcdbgrid, so start at index 1.
                for (int i = 1; i < e.Row.Cells.Count; i++)
                {
                    if (this.Columns.Count >= i)
                    {
                        TableCell cell = e.Row.Cells[i];

                        // check if we need to apply row highlight color
                        if (_lstRowColorConditions != null)
                        {
                            List<RowColorCondition> conditions = _lstRowColorConditions.FindAll(obj => obj.DataField.ToLower() == this.Columns[i - 1].SortExpression.ToLower());

                            foreach (RowColorCondition condition in conditions)
                            {
                                ApplyRowColor(condition, e.Row, cell.Text.ToUpper());
                            }
                        }

                        double width = this.Columns[i - 1].ItemStyle.Width.Value - 8; // deduct cellpadding value (4)
                        if (width > 0)
                        {
                            if (cell.Controls.Count == 0)
                            {
                                string wrap = this.Columns[i - 1].ItemStyle.Wrap ? "class=\"wrapcell\"" : "class=\"clipcell\" ";
                                cell.Text = string.Format("<div {0}style=\"width:{1}px;\">{2}</div>", wrap, width, cell.Text);
                            }
                        }
                        else
                        {
                            // Add a css class to all zero width cells so that they can be accessed. 
                            // The class value will be set with the FIELD_HEADER value in TV_DBGRIDDL.
                            // This is in order to allow hidden columsn to be selectable in javascript.
                            if (!String.IsNullOrEmpty(this.Columns[i - 1].HeaderText))
                            {
                                if (usesDBGridDL())
                                {
                                    cell.Attributes["class"] = "hide ";
                                    this.Columns[i - 1].HeaderStyle.CssClass = "hide";
                                }

                                cell.Attributes["class"] += "dgcell-" + this.Columns[i - 1].HeaderText.Replace(" ", "").Replace("#", "").Replace("@", "");
                                cell.Attributes["value"] = cell.Text;
                            }
                        }
                        totalWidth += cell.Width.Value;
                    }
                }

                if (FirstColumnCheckbox)
                {
                    var cbSelect = e.Row.FindControl("cbSelect") as CheckBox;
                    cbSelect.Attributes["itemindex"] = e.Row.DataItemIndex.ToString();
                }

                this.Width = totalWidth > this.Width.Value ? Unit.Pixel((int)totalWidth) : this.Width;
            }
        }

        protected override void OnInit(EventArgs e)
        {

            PLCSessionVars sv = new PLCSessionVars();
            sv.WriteDebug(this.PLCGridName + " OnInit (Enter)", true);

            base.OnInit(e);

            sv.WriteDebug(this.PLCGridName + " OnInit (1) ", true);

            if (this.Page != null)
                this.Page.RegisterRequiresPostBack(this);

            if (!usesDBGridDL() && this.Width == Unit.Empty) // HTML5 Fix for grids without DBGRIDDL configured
            {
                this.Width = PLCGridWidth != Unit.Percentage(100) ? PLCGridWidth : Unit.Pixel(700);
            }

            sv.WriteDebug(this.PLCGridName + " OnInit (exit)", true);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            string scrolldivID = "";
            if (!FirstColumnCheckbox && !AllowPaging)
            {
                ScriptManager sm = ScriptManager.GetCurrent(Page);
                scrolldivID = String.Format("{0}_div", this.ClientID);
                if (sm != null)
                {
                    ScriptManager.RegisterStartupScript(this.Page, this.GetType(), String.Format("{0}_PLCDBGridHeadingRow", this.ClientID),
                        String.Format("PLCDBGrid_SeparateHeadingRowToFixedPos('{0}');", scrolldivID), true);
                }
                else
                {
                    this.Page.ClientScript.RegisterStartupScript(this.GetType(), String.Format("{0}_PLCDBGridHeadingRow", this.ClientID),
                        String.Format("PLCDBGrid_SeparateHeadingRowToFixedPos('{0}');", scrolldivID));
                }
            }

            //if ((PLCSession.CheckUserOption("DBPEDIT") || PLCSession.GetWebUserField("SHOW_SCRIPT_CONFIGURATION") == "T") && !string.IsNullOrEmpty(this.PLCGridName))
            //{
            //    string iconProps = "";
            //    string iconOnClick = "";

            //    if (PLCSession.CheckUserOption("DBPEDIT"))
            //    {
            //        string iconEditorUrl = ResolveClientUrl("~/DBGridEditor.aspx") + "?name=" + this.PLCGridName + "&detach=true";
            //        iconOnClick = @"
            //            $('#" + this.ID + @"_editdbgridbutton').click(function() {
            //                var editorWin = window.open('" + iconEditorUrl + @"', 'DBGridEditor', 'width=860, height=500, scrollbars=1, resizable=1', false)
            //                editorWin.focus();
            //            });
            //        ";
            //        iconProps += "style: 'cursor: pointer;',";
            //        iconProps += "title: 'Edit DBGrid [" + this.PLCGridName + @"]',";
            //    }
            //    else if (PLCSession.GetWebUserField("SHOW_SCRIPT_CONFIGURATION") == "T")
            //        iconProps = "title: 'Grid Name: " + this.PLCGridName + "',";

            //    string editDBGridButton = @"
            //        if ($('#" + this.ID + @"_editdbgridbutton').length <= 0) {
            //            $('#" + (string.IsNullOrEmpty(scrolldivID) ? this.ClientID + "_div" : scrolldivID + "_header") + @"').before($('<img/>', {
            //                " + iconProps + @"
            //                id: '" + this.ID + @"_editdbgridbutton',
            //                src: '" + ResolveClientUrl("~/Images/edit-panel.png") + @"',
            //                height: '15px',
            //                width: '15px'
            //            }));" + iconOnClick + @"
            //        } 
            //    ";
            //    ScriptManager.RegisterStartupScript(this.Page, this.GetType(), this.ClientID + "_editDBGridButton", editDBGridButton, true);
            //}
        }

        //override protected void OnInit(EventArgs e)
        //{
        //    base.OnInit(e);  
        //    if (Page != null)
        //    {
        //        PLCConnectionString = Page.Session[PLCDBClass.PLC_ConnectionString].ToString();
        //        SetErrorMessage("");
        //        CreatePLCGridColumns();
        //    }
        //}

        #region Private Variables

        private string _PLCProviderName;
        private System.Web.UI.WebControls.Label _PLCErrorMessageLabel;
        private string _PLCErrorMessage;
        private PLCConnections _PLCConnectionType = PLCConnections.OLEDB;
        private bool useCaching = true;

        private List<RowColorCondition> _lstRowColorConditions;

        private List<string> additionalColumns
        {
            get
            {
                if (ViewState["AddtlColumns"] == null)
                    ViewState["AddtlColumns"] = new List<string>();

                return ViewState["AddtlColumns"] as List<string>;
            }
            set
            {
                ViewState["AddtlColumns"] = value;
            }
        }

        #endregion Private Variables

        #region Private procedures
        protected string PLCErrorMessage
        {
            get
            {
                return _PLCErrorMessage;
            }
            set
            {
                _PLCErrorMessage = value;
            }
        }

        //protected void SetErrorMessage(string ErrorMessage)
        //{
        //    PLCErrorMessage = ErrorMessage;
        //    if (ErrorMessage.IndexOf("Data not found") == 0)
        //        ErrorMessage = "";

        //    if (PLCErrorMessageLabel != null)
        //    {
        //        if (ErrorMessage.Trim() == "")
        //        {
        //            ErrorMessage = ".";
        //            PLCErrorMessageLabel.ForeColor = Color.White;
        //        }
        //        else
        //        {
        //            PLCErrorMessageLabel.ForeColor = Color.Red;
        //        }
        //        PLCErrorMessageLabel.Text = ErrorMessage;
        //    }
        //}
        #endregion Private procedures

        #region Porter Lee Properties

        #region Hidden properties
        [Category("PLC Properties"), Description("Connection type default OLEDB")]
        protected PLCConnections PLCConnectionType
        {
            get
            {
                return _PLCConnectionType;
            }
            set
            {
                _PLCConnectionType = value;
            }
        }

        [Category("PLC Properties"), Description("Provider Name - Provider=msdaora")]
        protected string PLCProviderName
        {
            get
            {
                return _PLCProviderName;
            }

            set
            {
                _PLCProviderName = value;
            }
        }

        [Category("PLC Properties"), Description("Connection String - Data Source=PLCORA1;User Id=FDLE;Password=FDLE")]
        [Bindable(true)]
        [DefaultValue("")]
        [Localizable(true)]
        protected string PLCConnectionString
        {
            get
            {
                String s = (String)ViewState["PLCConnectionString"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["PLCConnectionString"] = value;
            }
        }
        #endregion Hidden properties

        [Category("PLC Properties"), Description("Grid Name  -  DBGRID.Grid Name")]
        [Browsable(true)]
        [Bindable(true)]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGridName
        {
            get
            {
                SetPLCGridProperties();
                String s = (String)ViewState["PLCGridName"];
                return ((s == null) ? String.Empty : s);
            }
            set
            {
                SetPLCGridProperties();
                ViewState["PLCGridName"] = value;
            }
        }

        [Category("PLC Properties"), Description("Sql String  - DBGRIDHD.SQL String")]
        [Bindable(true)]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCSQLString
        {
            get
            {
                String s = (String)ViewState["PLCSQLString"];
                return ((s == null) ? String.Empty : s);
            }
            set
            {
                PLCSessionVars sv = new PLCSessionVars();
                value = sv.FixedSQLStr(value);
                ViewState["PLCSQLString"] = value;
            }
        }

        public string PLCUSESDL
        {
            get
            {
                String s = (String)ViewState["PLCUSESDL"];
                return ((s == null) ? String.Empty : s);
            }
            set
            {
                ViewState["PLCUSESDL"] = value;
            }
        }


        [Category("PLC Properties"), Description("Sql String  - DBGRIDHD.Panel Name = DBPANEL.Panel Name")]
        [Bindable(true)]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCPanelName
        {
            get
            {
                String s = (String)ViewState["PLCPanelName"];
                return ((s == null) ? String.Empty : s);
            }
            set
            {
                ViewState["PLCPanelName"] = value;
            }
        }

        [Category("PLC Properties"), Description("Default Data Key Names if the grid is not defined in the database")]
        [Bindable(true)]
        [DefaultValue("")]
        [Localizable(true)]
        [TypeConverterAttribute(typeof(StringArrayConverter))]
        public string[] PLCDefaultDataKeyNames
        {
            get
            {
                String[] s = (String[])ViewState["PLCDefaultDataKeyNames"];
                return ((s == null) ? null : s);
            }
            set
            {
                ViewState["PLCDefaultDataKeyNames"] = value;
            }
        }

        [Category("PLC Properties"), Description("Additional FROM Clause  example 1. INNER/LEFT OUTER/RIGHT OUTER JOIN TABLE_NAME [ALIAS]")]
        [Bindable(true)]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCSQLString_AdditionalFrom
        {
            get
            {
                String s = (String)ViewState["PLCSQLString_AdditionalFrom"];
                return ((s == null) ? String.Empty : s);
            }
            set
            {
                PLCSessionVars sv = new PLCSessionVars();
                value = sv.FixedSQLStr(value);
                ViewState["PLCSQLString_AdditionalFrom"] = value;
            }
        }



        [Category("PLC Properties"), Description("The Sort Expression")]
        [Bindable(true)]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCSortExpression
        {
            get
            {
                String s = (String)ViewState["PLCSortExpression"];
                return ((s == null) ? String.Empty : s);
            }
            set
            {
                ViewState["PLCSortExpression"] = value;
            }
        }

        [Category("PLC Properties"), Description("Filter String")]
        [Bindable(true)]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCFilterString
        {
            get
            {
                String s = (String)ViewState["PLCFilterString"];
                return ((s == null) ? String.Empty : s);
            }
            set
            {
                ViewState["PLCFilterString"] = value;
            }
        }

        [Category("PLC Properties"), Description("Additional Sql string  example 1. Where \"Lab Code\" = 'T' 2. And \"Lab Code\" = 'T'")]
        [Bindable(true)]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCSQLString_AdditionalCriteria
        {
            get
            {
                String s = (String)ViewState["PLCSQLString_AdditionalCriteria"];
                return ((s == null) ? String.Empty : s);
            }
            set
            {
                PLCSessionVars sv = new PLCSessionVars();
                value = sv.FixedSQLStr(value);
                ViewState["PLCSQLString_AdditionalCriteria"] = value;
            }
        }

        [Category("PLC Properties"), Description("Label to set the error message, if the component thorws exceptional errors/messages this component will display the message from the server")]
        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Content)]
        public System.Web.UI.WebControls.Label PLCErrorMessageLabel
        {
            get
            {
                return _PLCErrorMessageLabel;
            }
            set
            {
                _PLCErrorMessageLabel = value;
            }
        }


        [Category("PLC Properties"), Description("Grid Width")]
        [Bindable(true)]
        [Localizable(true)]
        public Unit PLCGridWidth
        {
            get
            {
                if (ViewState["PLCGridWidth"] == null)
                    ViewState["PLCGridWidth"] = Unit.Pixel(700); //default width if not set

                return (Unit)ViewState["PLCGridWidth"];
            }
            set
            {
                ViewState["PLCGridWidth"] = value;
            }
        }


        [Category("PLC Properties"), Description("CancelPostbackOnClick")]
        [Bindable(true)]
        [Localizable(true)]
        public bool CancelPostbackOnClick
        {
            get
            {
                if (ViewState["CancelPostbackOnClick"] == null)
                    ViewState["CancelPostbackOnClick"] = false;

                return (bool)ViewState["CancelPostbackOnClick"];
            }
            set
            {
                ViewState["CancelPostbackOnClick"] = value;
            }
        }

        [Category("PLC Properties"), Description("Height on Scroll Enabled. Set UsesScrollbar property to true to enable scroll.")]
        [Bindable(true)]
        [Localizable(true)]
        public Unit HeightOnScrollEnabled
        {
            get
            {
                if (ViewState["HeightOnScrollEnabled"] != null)
                {
                    return (Unit)ViewState["HeightOnScrollEnabled"];
                }
                else
                {
                    return Unit.Empty;
                }
            }
            set
            {
                ViewState["HeightOnScrollEnabled"] = value;
            }
        }

        [Category("PLC Properties"), Description("Automatically add a Checkbox Control as first column")]
        [DefaultValue(false)]
        public Boolean FirstColumnCheckbox
        {
            get
            {
                if (ViewState["FirstColumnCheckbox"] == null)
                    ViewState["FirstColumnCheckbox"] = false;

                return (bool)ViewState["FirstColumnCheckbox"];
            }
            set
            {
                ViewState["FirstColumnCheckbox"] = value;
            }
        }

        [Category("PLC Properties"), Description("Remember the checked state of the checkboxes added by FirstColumnCheckbox")]
        [DefaultValue(true)]
        public Boolean EnablePersistedCheckedData
        {
            get
            {
                if (ViewState["EnablePersistedCheckedData"] == null)
                    ViewState["EnablePersistedCheckedData"] = true;

                return (bool)ViewState["EnablePersistedCheckedData"];
            }
            set
            {
                ViewState["EnablePersistedCheckedData"] = value;
            }
        }

        [Category("PLC Properties"), Description("Cache the data")]
        [DefaultValue(false)]
        public Boolean UseDataCaching
        {
            get
            {
                if (ViewState["UseDataCaching"] == null)
                    ViewState["UseDataCaching"] = false;

                return (bool)ViewState["UseDataCaching"];
            }
            set
            {
                ViewState["UseDataCaching"] = value;
            }
        }

        // Set this property to set a custom connection string in the grid
        [Category("PLC Properties"), Description("Custom Connection String  - DBGRID")]
        [Browsable(true)]
        [Bindable(true)]
        [DefaultValue("")]
        [Localizable(true)]
        public string CustomConnectionString
        {
            get
            {
                String s = (String)ViewState["CustomConnectionString"];
                return ((s == null) ? String.Empty : s);
            }
            set
            {
                ViewState["CustomConnectionString"] = value;
            }
        }

        [Category("PLC Properties"), Description("Number of records per page")]
        [Bindable(true)]
        [Localizable(true)]
        public int RecordsPerPage
        {
            get
            {
                if (ViewState["RecordsPerPage"] == null)
                    return 0;

                return (int)ViewState["RecordsPerPage"];
            }
            set
            {
                ViewState["RecordsPerPage"] = value;
            }
        }

        [Category("PLC Properties"), Description("Enables the scroll feature.")]
        [Bindable(true)]
        [Localizable(true)]
        public bool UsesScrollbar
        {
            get
            {
                if (ViewState["UsesScrollbar"] == null)
                    return false;

                return (bool)ViewState["UsesScrollbar"];
            }
            set
            {
                ViewState["UsesScrollbar"] = value;
            }
        }

        /// <summary>
        /// Original value of AllowPaging property
        /// </summary>
        private bool? DefaultAllowPaging
        {
            get
            {
                return (bool?)ViewState["DefaultAllowPaging"];
            }
            set
            {
                ViewState["DefaultAllowPaging"] = value;
            }
        }

        /// <summary>
        /// Original value of Height property
        /// </summary>
        private Unit? DefaultHeight
        {
            get
            {
                return (Unit?)ViewState["DefaultHeight"];
            }
            set
            {
                ViewState["DefaultHeight"] = value;
            }
        }

        /// <summary>
        /// Use exact matching of FROM keyword in SQL_STRING using regex
        /// </summary>
        public bool UseRegexSQLParsing
        {
            get
            {
                if (ViewState["UseRegexSQLParsing"] == null)
                    return false;


                return (bool)ViewState["UseRegexSQLParsing"];
            }
            set
            {
                ViewState["UseRegexSQLParsing"] = value;
            }

        }

        #endregion Porter Lee Properties

        protected void PLCDBGrid_Sorting(object sender, GridViewSortEventArgs e)
        {
            var checkedRowDataKeys = new List<DataKey>();
            var rememberCheckbox = FirstColumnCheckbox && EnablePersistedCheckedData;
            if (rememberCheckbox)
            {
                rememberCheckbox = ((HiddenField)HeaderRow.FindControl("cbSelect_state")).Value.Length < 1001;

                if (rememberCheckbox)
                    checkedRowDataKeys = GetCheckedDataKeys();
            }

            SetSortExpression(e.SortExpression);
            PLCSortExpression = Attributes["SortExpression"];
            CreatePLCGridColumns();

            if (rememberCheckbox && checkedRowDataKeys.Count > 0)
            {
                CheckRowByDataKeys(checkedRowDataKeys);

                var rowIndexes = ((HiddenField)HeaderRow.FindControl("cbSelect_state")).Value;
                ((CheckBox)HeaderRow.FindControl("cbSelect_All")).Checked = !rowIndexes.Contains("0");
            }
        }

        protected void PLCDBGrid_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            Boolean headerChecked = false;
            String rowIndexes = "";
            if (FirstColumnCheckbox)
            {
                headerChecked = ((CheckBox)HeaderRow.FindControl("cbSelect_All")).Checked;
                rowIndexes = ((HiddenField)HeaderRow.FindControl("cbSelect_state")).Value;
            }

            this.PageIndex = e.NewPageIndex;
            CreatePLCGridColumns();

            if (FirstColumnCheckbox)
                LoadCheckedRows(headerChecked, rowIndexes);
        }

        private string SetSortExpression(string sortExpression)
        {
            string[] sortColumns = null;
            string sortAttribute = Attributes["SortExpression"];
            PLCSession.WriteDebug("@SortExpression:" + sortExpression + " : " + sortAttribute);
            if (!String.IsNullOrEmpty(sortAttribute))
            {
                sortColumns = sortAttribute.Split(",".ToCharArray());
                if (Regex.IsMatch(sortAttribute, "\\b" + Regex.Replace(sortExpression, @"[^-0-9A-Za-z _#\.]", "") + "(\\s(ASC|DESC))?\\b") || sortAttribute.Trim() == sortExpression.Trim())
                    sortAttribute = ModifySortExpression(sortColumns, sortExpression);
                else
                    sortAttribute += sortExpression + " ASC,";
            }
            else
                sortAttribute += sortExpression + " ASC,";

            Attributes["SortExpression"] = sortAttribute;
            return sortAttribute;
        }

        private string ModifySortExpression(string[] sortColumns, string sortExpression)
        {
            for (int i = 0; i < sortColumns.Length; i++)
            {
                string ascSortExpression = String.Concat(sortExpression, " ASC");
                string descSortExpression = String.Concat(sortExpression, " DESC");

                if (ascSortExpression.Equals(sortColumns[i]))
                {
                    sortColumns[i] = descSortExpression;
                }

                else if (descSortExpression.Equals(sortColumns[i]))
                {
                    Array.Clear(sortColumns, i, 1);
                }
            }
            return String.Join(",", sortColumns).Replace(",,", ",").TrimStart(",".ToCharArray());
        }

        private void ProcessAdditionalSqlScript()
        {
            PLCSessionVars sv = new PLCSessionVars();
            sv.WriteDebug("PLCSQLString_AdditionalFrom AND PLCDBGrid.processAdditionalSQLScript", true);
            if ((!String.IsNullOrEmpty(PLCSQLString)) && (!string.IsNullOrEmpty(PLCSQLString_AdditionalCriteria)) && (!PLCSQLString.Contains(PLCSQLString_AdditionalCriteria)))
            {
                string OrderString = "";
                string GroupString = string.Empty;
                string TempSqlString = PLCSQLString.ToUpper();

                int Index = TempSqlString.LastIndexOf("ORDER BY");
                int GroupIndex = TempSqlString.LastIndexOf("GROUP BY");

                if (Index >= 0)
                {
                    OrderString = PLCSQLString.Substring(Index, PLCSQLString.Length - Index);
                    PLCSQLString = PLCSQLString.Substring(0, Index - 1);
                }

                if (GroupIndex >= 0)
                {
                    GroupString = PLCSQLString.Substring(GroupIndex, PLCSQLString.Length - GroupIndex) + " ";
                    PLCSQLString = PLCSQLString.Substring(0, GroupIndex - 1);
                }

                string TempAdditionalString = PLCSQLString_AdditionalCriteria.ToUpper();
                if (TempAdditionalString.Trim().StartsWith("WHERE ")) //check if additional filter starts with WHERE
                    PLCSQLString_AdditionalCriteria = PLCSQLString_AdditionalCriteria.Substring(TempAdditionalString.IndexOf("WHERE ") + 6); //remove WHERE
                else if (TempAdditionalString.Trim().StartsWith("AND ")) //check if additional filter starts with AND
                    PLCSQLString_AdditionalCriteria = PLCSQLString_AdditionalCriteria.Substring(TempAdditionalString.IndexOf("AND ") + 4); //remove AND

                bool hasWhereClause = (TempSqlString.IndexOf("WHERE", TempSqlString.LastIndexOf("FROM")) > -1);

                if (UseRegexSQLParsing)
                {
                    int fromLastIndex = Regex.Match(TempSqlString, @"\bFROM\b", RegexOptions.RightToLeft).Index;
                    hasWhereClause = (TempSqlString.IndexOf("WHERE", fromLastIndex) > -1);
                }


                //check if SQL already contains WHERE clause
                if (hasWhereClause)
                {
                    // AAC 08/31/2010: Process Additional FROM Script
                    if (PLCSQLString_AdditionalFrom.Trim().Length > 0)
                    {
                        int fromIndex = PLCSQLString.LastIndexOf("WHERE");
                        string whereClause = PLCSQLString.Substring(fromIndex);
                        PLCSQLString = PLCSQLString.Substring(0, fromIndex - 1) + " " + PLCSQLString_AdditionalFrom + " " + whereClause;
                    }

                    PLCSQLString = PLCSQLString + " AND " + PLCSQLString_AdditionalCriteria;
                }
                else
                {
                    // AAC 08/31/2010: Process Additional FROM Script
                    if (PLCSQLString_AdditionalFrom.Trim().Length > 0)
                    {
                        PLCSQLString = PLCSQLString + " " + PLCSQLString_AdditionalFrom;
                    }

                    PLCSQLString = PLCSQLString + " WHERE " + PLCSQLString_AdditionalCriteria;
                }

                PLCSQLString += " " + GroupString + OrderString;
            }
        }

        private DataTable QueryNewDBGridHD(string gridname)
        {
            //PLCQuery qry = new PLCQuery("Select * from TV_DBGRIDHD Where GRID_NAME = '" + gridname + "'");
            //qry.Open();

            PLCQuery qry = CacheHelper.OpenCachedSqlReadOnly("Select * from TV_DBGRIDHD Where GRID_NAME = '" + gridname + "'");
            return qry.PLCDataTable;
        }

        // Get sql string associated with this gridname.
        private DataTable QueryDBGridHD(string gridname)
        {
            DataTable retResults;
            string hdPanelKey = String.Format("{0}_{1}", "dbgridhd", gridname);

            if (this.useCaching)
            {
                // If not previously cached, run the dbgridhd query.
                if (!CacheHelper.IsInCache(hdPanelKey))
                {
                    retResults = QueryNewDBGridHD(gridname);
                    CacheHelper.AddItem(hdPanelKey, retResults, PLCSession.GetMetadataCacheDuration());
                }
                else
                {
                    // Get cached result set.
                    retResults = (DataTable)CacheHelper.GetItem(hdPanelKey);
                }
            }
            else
            {
                // Caching is turned off so always requery.
                retResults = QueryNewDBGridHD(gridname);
            }

            return retResults;
        }

        public void addDataKeys()
        {

            if (String.IsNullOrWhiteSpace(PLCGridName))
            {

                return;
            }

            PLCQuery qry = CacheHelper.OpenCachedSqlReadOnly("Select * from TV_DBGRIDDL Where GRID_NAME = '" + PLCGridName + "' And HIDE_FIELD in ('T', 'K') Order by DISPLAY_ORDER");


            //PLCQuery qry = new PLCQuery("Select * from TV_DBGRIDDL Where GRID_NAME = '" + PLCGridName + "' And HIDE_FIELD in ('T', 'K') Order by DISPLAY_ORDER");
            //qry.Open();

            if (qry.IsEmpty()) return;

            List<string> savekeys = DataKeyNames.ToList();
            string thiskey = "";

            while (!qry.EOF())
            {
                thiskey = qry.FieldByName("FIELD_NAME");
                if (savekeys.IndexOf(thiskey) < 0) savekeys.Add(thiskey);
                qry.Next();
            }
            DataKeyNames = savekeys.ToArray();
            DataBind();
        }

        private DataTable QueryNewDBGridDL(string gridname)
        {

            if (String.IsNullOrWhiteSpace(gridname))
            {

                return null;
            }


            PLCQuery qry = CacheHelper.OpenCachedSqlReadOnly("Select * from TV_DBGRIDDL Where GRID_NAME = '" + PLCGridName + "' And HIDE_FIELD <> 'T' Order by DISPLAY_ORDER");

            //PLCQuery qry = new PLCQuery("Select * from TV_DBGRIDDL Where GRID_NAME = '" + PLCGridName + "' And HIDE_FIELD <> 'T' Order by DISPLAY_ORDER");
            //qry.Open();
            return qry.PLCDataTable;
        }

        // Get grid fields associated with this gridname.
        private DataTable QueryDBGridDL(string gridname)
        {

            //no need to query and cache a grid without a name.
            if (String.IsNullOrWhiteSpace(gridname))
            {
                return new DataTable("TBL");
            }

            DataTable retResults;
            string dlPanelKey = String.Format("{0}_{1}", "dbgriddl", gridname);

            if (this.useCaching)
            {
                // If not previously cached, run the dbgridhd query.
                if (!CacheHelper.IsInCache(dlPanelKey))
                {
                    retResults = QueryNewDBGridDL(gridname);
                    CacheHelper.AddItem(dlPanelKey, retResults, PLCSession.GetMetadataCacheDuration());
                }
                else
                {
                    // Get cached result set.
                    retResults = (DataTable)CacheHelper.GetItem(dlPanelKey);
                }
            }
            else
            {
                // Caching is turned off so always requery.
                retResults = QueryNewDBGridDL(gridname);
            }

            return retResults;
        }

        protected void PLCDBGrid_RowCreated(object sender, GridViewRowEventArgs e)
        {
            // Collapse the 'Select' column.
            if (this.AutoGenerateSelectButton)
            {
                e.Row.Cells[0].Style.Value = "padding: 0; width: 0px";
            }

            this.EmptyDataRowStyle.CssClass = "plcdbgridEmpty";

            if (e.Row.RowType == DataControlRowType.Header)
            {
                if (!String.IsNullOrEmpty(Attributes["SortExpression"]))
                    DisplaySortOrderImages(Attributes["SortExpression"], e.Row);
            }
            else if (e.Row.RowType == DataControlRowType.Footer)
            {
            }
            //else if (e.Row.RowType == DataControlRowType.DataRow)
            //{
            //    e.Row.Attributes["onmouseover"] = "this.style.cursor='hand'; this.style.backgroundColor = '#ACACAC';";

            //    int CurrentRow = e.Row.RowIndex % 2;
            //    if (CurrentRow == 0)
            //        e.Row.Attributes["onmouseout"] = "this.ForeColor = '#333333'; this.style.backgroundColor = '#ffffff';this.Style.ForeColor = '#333333';this.AlternatingRowStyle.BackColor = '#ffffff';this.AlternatingRowStyle.ForeColor='#284775';";
            //    else
            //        e.Row.Attributes["onmouseout"] = "this.ForeColor = '#333333'; this.style.backgroundColor = '#F7F6F3';this.Style.ForeColor = '#333333';this.AlternatingRowStyle.BackColor = '#ffffff';this.AlternatingRowStyle.ForeColor='#284775';";

            //    e.Row.Attributes["onclick"] = Page.ClientScript.GetPostBackClientHyperlink(this, "Select$" + e.Row.RowIndex);
            //}

            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Skip over the 'Select' button in plcdbgrid, so start at index 1.
                for (int i = 1; i < e.Row.Cells.Count; i++)
                {
                    if (this.Columns.Count >= i)
                    {
                        TableCell cell = e.Row.Cells[i];
                        double width = this.Columns[i - 1].ItemStyle.Width.Value - 8;
                        if (width > 0)
                        {
                            if (cell.Controls.Count > 0)
                            {
                                Literal openingDiv = new Literal();
                                openingDiv.Text = string.Format("<div class=\"clipcell\" style=\"width:{0}px;\">", width);
                                cell.Controls.AddAt(0, openingDiv);

                                Literal closingDiv = new Literal();
                                closingDiv.Text = "</div>";
                                cell.Controls.Add(closingDiv);
                            }
                        }
                    }
                }

                // Hide the 'Select' control.
                if ((e.Row.Cells[0].Controls.Count > 0) && (e.Row.Cells[0].Controls[0] is LinkButton) && (((LinkButton)e.Row.Cells[0].Controls[0]).Text == "Select"))
                    ((LinkButton)e.Row.Cells[0].Controls[0]).Attributes["style"] = "display:none;";

                // Set highlight when hovering mouse over row and add postback when row is selected. 
                e.Row.Attributes["onmouseover"] = WrapInEnabledCondition("this.style.cursor='pointer'; this.style.textDecoration='underline'; /* _bgcolor=this.style.backgroundColor; this.style.backgroundColor='C8FFED'; */");
                e.Row.Attributes["onmouseout"] = WrapInEnabledCondition("this.style.textDecoration='none'; /* this.style.backgroundColor=_bgcolor; */");

                //$$ Need a cleaner way to code this!
                //    Calling code needs to set norowclick=true when clicking on a specific control (such as checkbox) 
                //    should not activate the rowclick. See LabRptSearch.aspx code below for a sample:
                //        var norowclick = null;
                //        function SetItemNoRowClicks() {
                //          $(".gridview_norowclick").click(function() { norowclick=true; });
                //
                //   pseudocode for onclick (the row attribute 'onclick' setting below):
                //     if (enabled)
                //     {
                //          if (norowclick is undefined or null)
                //          {
                //              do rowclick
                //          }
                //      }
                //      norowclick = null;
                //
                if (!CancelPostbackOnClick)
                {
                    e.Row.Attributes["onclick"] = WrapInEnabledCondition(
                            WrapInIfCondition(
                                this.Page.ClientScript.GetPostBackEventReference(this, "Select$" + e.Row.RowIndex.ToString()),
                                "(typeof norowclick == 'undefined') || (norowclick==null)"
                            )
                        )
                        + "norowclick = null;";
                }

            }
        }

        private string WrapInIfCondition(string codeblock, string ifcondition)
        {
            return String.Format("if ({0}) {{{1}}};", ifcondition, codeblock);
        }

        // Wrap the code block in an if condition so that code block is called only when grid div is enabled.
        private string WrapInEnabledCondition(string codeblock)
        {
            // .disabled does not work in Chrome
            return String.Format("if (!document.getElementById('{0}').getAttribute('disabled')) {{{1}}};", this.ClientID, codeblock);
        }

        protected void PLCDBGrid_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetPLCGridProperties();
        }

        private void DisplaySortOrderImages(string sortExpression, GridViewRow dgItem)
        {
            string[] sortColumns = sortExpression.TrimEnd(',').Split(",".ToCharArray());
            for (int i = 0; i < dgItem.Cells.Count; i++)
            {

                if (dgItem.Cells[i].Controls.Count > 0 && dgItem.Cells[i].Controls[0] is LinkButton)
                {
                    string sortOrder;
                    int sortOrderNo;
                    string column = ((LinkButton)dgItem.Cells[i].Controls[0]).CommandArgument;

                    SearchSortExpression(sortColumns, column, out sortOrder, out sortOrderNo);
                    if (sortOrderNo > 0)
                    {
                        if (sortOrder.Equals("ASC"))
                        {
                            System.Web.UI.WebControls.Image imgUp = new System.Web.UI.WebControls.Image();
                            imgUp.ImageUrl = "~/Images/SortAsc.bmp";
                            dgItem.Cells[i].Controls.Add(imgUp);
                            Literal litOrder = new Literal();
                            litOrder.Text = "" + sortOrderNo.ToString();
                            dgItem.Cells[i].Controls.Add(litOrder);
                        }
                        else if (sortOrder.Equals("DESC"))
                        {
                            System.Web.UI.WebControls.Image imgDown = new System.Web.UI.WebControls.Image();
                            imgDown.ImageUrl = "~/Images/SortDesc.bmp";
                            dgItem.Cells[i].Controls.Add(imgDown);
                            Literal litOrder = new Literal();
                            litOrder.Text = "" + sortOrderNo.ToString();
                            dgItem.Cells[i].Controls.Add(litOrder);
                        }
                        ((LinkButton)dgItem.Cells[i].Controls[0]).CssClass = "wrapcell";
                    }
                }
            }
        }

        private void SearchSortExpression(string[] sortColumns, string sortColumn, out string sortOrder, out int sortOrderNo)
        {
            sortOrder = "";
            sortOrderNo = -1;

            for (int i = 0; i < sortColumns.Length; i++)
            {
                if (sortColumns[i].Substring(0, sortColumns[i].Length - 4).Trim() == sortColumn)
                {
                    sortOrderNo = i + 1;
                    sortOrder = sortColumns[i].Substring(sortColumn.Length).Trim();
                }
            }
        }

        public void SetPLCGridProperties()
        {
            this.CssClass = PLCDBGridCSS;
            this.AutoGenerateColumns = false;
            this.AllowSorting = true;
            this.CellPadding = 4;
            this.AutoGenerateSelectButton = true;
        }

        public BoundField AddBoundField(string DataField, string Heading, int Width, bool isDate, Dictionary<string, string> options)
        {
            BoundField NewCol = new BoundField();
            NewCol.DataField = DataField;
            if (options != null && options.ContainsKey("SORT"))
                NewCol.SortExpression = options["SORT"];
            else
                NewCol.SortExpression = DataField;
            NewCol.ReadOnly = true;
            if (Width > 0)
            {
                NewCol.ItemStyle.Width = Width;
                NewCol.HeaderStyle.Width = Width;
            }
            if (isDate)
            {
                if (PLCSession.GetDateFormat().ToUpper().StartsWith("DD"))
                    NewCol.DataFormatString = "{0:" + PLCSession.GetDateFormat() + "}";
                else
                    NewCol.DataFormatString = "{0:d}"; //shortdatestring
            }
            NewCol.HeaderText = Heading;
            NewCol.HeaderStyle.HorizontalAlign = HorizontalAlign.Left;
            NewCol.HeaderStyle.VerticalAlign = VerticalAlign.Top;
            NewCol.HeaderStyle.Wrap = false;
            NewCol.ItemStyle.Wrap = options != null && options.ContainsKey("WRAP") && options["WRAP"].ToUpper() == "T";
            if (options != null && options.ContainsKey("HTML"))
                NewCol.HtmlEncode = options["HTML"] != "T";//allow HTML 

            // generate the row color condition if any
            if (options != null && options.ContainsKey("ROWHIGHLIGHT"))
            {
                GetRowCondition(options["ROWHIGHLIGHT"], DataField, this.Columns.Count);
            }

            return NewCol;
        }

        public CheckBoxField AddCheckBoxField(string dataField, string headerText, int width, Dictionary<string, string> options)
        {
            CheckBoxField column = new CheckBoxField();
            column.DataField = dataField;
            column.SortExpression = dataField;
            column.HeaderText = headerText;
            column.HeaderStyle.HorizontalAlign = HorizontalAlign.Left;
            column.HeaderStyle.VerticalAlign = VerticalAlign.Top;
            column.HeaderStyle.Wrap = false;

            if (width > 0)
            {
                column.ItemStyle.Width = width;
                column.HeaderStyle.Width = width;
            }
            if (options.ContainsKey("TEXT"))
            {
                column.Text = options["TEXT"];
            }
            if (options.ContainsKey("DATEFORMAT"))
            {
                if (options["DATEFORMAT"] == "SHORT")
                {
                    column.DataFormatString = "{0:d}"; //shortdatestring
                }
            }
            if (options.ContainsKey("READONLY"))
            {
                column.ReadOnly = Convert.ToBoolean(options["READONLY"]);
            }

            return column;
        }

        public ButtonField AddButtonField(string dataField, string headerText, int width, Dictionary<string, string> options)
        {
            ButtonField column = new ButtonField();
            column.DataTextField = dataField;
            column.SortExpression = dataField;
            column.HeaderText = headerText;
            column.HeaderStyle.HorizontalAlign = HorizontalAlign.Left;
            column.HeaderStyle.VerticalAlign = VerticalAlign.Top;
            column.HeaderStyle.Wrap = false;

            if (width > 0)
            {
                column.ItemStyle.Width = width;
                column.HeaderStyle.Width = width;
            }
            if (options.ContainsKey("TEXT"))
            {
                column.Text = options["TEXT"];
            }
            if (options.ContainsKey("DATEFORMAT"))
            {
                if (options["DATEFORMAT"] == "SHORT")
                {
                    column.DataTextFormatString = "{0:d}"; //shortdatestring
                }
            }
            if (options.ContainsKey("BUTTONTYPE"))
            {
                column.ButtonType = (ButtonType)Enum.Parse(typeof(ButtonType), options["BUTTONTYPE"], true);
            }
            else
            {
                column.ButtonType = ButtonType.Link;
            }
            if (options.ContainsKey("COMMANDNAME"))
            {
                column.CommandName = options["COMMANDNAME"];
            }
            if (options.ContainsKey("COLOR"))
            {
                column.ItemStyle.ForeColor = Color.FromName(options["COLOR"]);
            }
            return column;
        }

        public BoundField AddDateField(string DataField, string Heading, int Width)
        {
            return AddFormattedDateTimeField(DataField, Heading, Width, "{0:MM/dd/yyyy}");
        }

        public BoundField AddDateFieldDMY(string DataField, string Heading, int Width)
        {
            return AddFormattedDateTimeField(DataField, Heading, Width, "{0:dd/MM/yyyy}");
        }

        public BoundField AddTime24Field(string DataField, string Heading, int Width)
        {
            return AddFormattedDateTimeField(DataField, Heading, Width, "{0:H:mm:ss}");
        }

        public BoundField AddTimeAMPMField(string DataField, string Heading, int Width)
        {
            return AddFormattedDateTimeField(DataField, Heading, Width, "{0:t}");
        }

        public BoundField AddDateTime24Field(string DataField, string Heading, int Width)
        {
            return AddFormattedDateTimeField(DataField, Heading, Width, "{0:MM/dd/yyyy H:mm:ss}");
        }

        private BoundField AddFormattedDateTimeField(string DataField, string Heading, int Width, string format)
        {
            BoundField NewCol = new BoundField();
            NewCol.DataField = DataField;
            NewCol.SortExpression = DataField;
            NewCol.ReadOnly = true;
            if (Width > 0)
            {
                NewCol.ItemStyle.Width = Width;
                NewCol.HeaderStyle.Width = Width;
            }

            NewCol.DataFormatString = format;
            NewCol.HeaderText = Heading;
            NewCol.HeaderStyle.HorizontalAlign = HorizontalAlign.Left;
            NewCol.HeaderStyle.VerticalAlign = VerticalAlign.Top;
            NewCol.HeaderStyle.Wrap = false;
            NewCol.HtmlEncode = false;
            return NewCol;
        }


        public void SetUserParameter(string pname, string pval)
        {
            //string paramtag = "{" + pname + "}";
            //PLCSQLString = PLCSQLString.Replace(paramtag, pval);
            if (userparams.ContainsKey(pname))
            {
                userparams[pname] = pval;
            }
            else
            {
                userparams.Add(pname, pval);
            }
        }


        private void ProcessMacro()
        {
            PLCSessionVars sv = new PLCSessionVars();
            int P;
            int P1;
            string TempString = PLCSQLString;
            string SqlString = "";
            string Macro;

            if (TempString.IndexOf("<&>") >= 0)
            {
                P = TempString.IndexOf("<&>");
                while (P >= 0)
                {
                    SqlString += TempString.Substring(0, P);
                    TempString = TempString.Remove(0, P + 3);
                    P1 = TempString.IndexOf("</&>");
                    if (P1 >= 0)
                    {
                        Macro = TempString.Substring(0, P1);
                        TempString = TempString.Remove(0, P1 + 4);

                        // If <&>LABCTRL(NNN)</&> was passed.
                        if (Macro.StartsWith("LABCTRL(") && (Macro.EndsWith(")")))
                        {
                            int iLeftParens = Macro.IndexOf('(');
                            int iRightParens = Macro.IndexOf(')');

                            if (iRightParens > iLeftParens + 1)     // Make sure there's a parameter such as LABCTRL(USES_RD). Ignore if no parameter.
                            {
                                // Given LABCTRL(USES_RD): Extract 'USES_RD' string to parm, then get labctrl setting.
                                string parm = Macro.Substring(iLeftParens + 1, iRightParens - iLeftParens - 1);
                                SqlString += sv.GetLabCtrl(parm);
                            }
                        }
                        else
                        {
                            switch (Macro)
                            {
                                case "CASE_KEY":
                                    SqlString += sv.PLCGlobalCaseKey;
                                    break;
                                case "ANALYST":
                                    SqlString += sv.PLCGlobalAnalyst;
                                    break;
                                case "USER":
                                    SqlString += sv.PLCGlobalAnalyst;
                                    break;
                                case "EVIDENCE_CONTROL_NUMBER":
                                    SqlString += sv.PLCGlobalECN;
                                    break;
                                case "EXAM_KEY":
                                    SqlString += sv.PLCGlobalAssignmentKey;
                                    break;
                                case "BATCH_NO":
                                    SqlString += sv.PLCGlobalBatchKey;
                                    break;
                                case "CHEM_CONTROL_NUM":
                                    //$$ We should be using ChemInvConstants.CHEM_CONTROL_NUM instead of "CHEM_CONTROL_NUM" 
                                    //   but we don't have access to PLCGlobals from here. We could have ChemInv use 
                                    //   a single named session variable for chem control number instead of GetChemInvProperty()
                                    //   but that would mean replacing all references to GetChemInvProperty().
                                    //   Figure this out later.
                                    SqlString += Convert.ToString(sv.GetChemInvProperty<int>("CHEM_CONTROL_NUM", 0));
                                    break;
                                case "PRELOG_DEPT_CODE":
                                    SqlString += sv.PLCGlobalPrelogDepartmentCode;
                                    break;
                                case "PRELOG_USER":
                                    SqlString += sv.PLCGlobalPrelogUser;
                                    break;
                                case "PRELOG_DEPT_CASE_NUM":
                                    SqlString += sv.PLCGlobalPrelogDepartmentCaseNumber;
                                    break;
                                case "PRELOG_SUB_NUM":
                                    SqlString += sv.PLCGlobalPrelogSubmissionNumber;
                                    break;
                                case "WEBUSER_DEPT_CODE":
                                    SqlString += sv.PLCGlobalAnalystDepartmentCode;
                                    break;
                                case "CONTAINER_DESCRIPTION":
                                    SqlString += sv.PLCCaseContainerDescription;
                                    break;
                                case "CONTAINER_SOURCE":
                                    SqlString += sv.PLCCaseContainerSource;
                                    break;
                                case "CONTAINER_COMMENTS":
                                    SqlString += sv.PLCCaseContainerComments;
                                    break;
                                case "PRELOG_CASE_KEY":
                                    SqlString += sv.PLCGlobalPrelogCaseKey;
                                    break;
                                case "LAB_CODE":
                                    SqlString += sv.PLCGlobalLabCode;
                                    break;
                                case "SR_LP_REFERENCE_TYPE":
                                    SqlString +=  string.IsNullOrEmpty(PLCSession.PLCGlobalPrelogUser) ? PLCSession.GetLabCtrl("DEFAULT_SR_LP_NAME_REF_TYPE") : PLCSession.GetDeptCtrl("DEFAULT_SR_LP_NAME_REF_TYPE");
                                    break;
                                case "ANALYST_ACCESS_CODES":
                                    SqlString += string.Join(",", PLCSession.GetGlobalAnalystAccessCodes(PLCSession.PLCGlobalAnalyst));
                                    break;
                                default:
                                    SqlString += "Macro not found " + Macro;
                                    break;
                            }
                        }
                    }
                    P = TempString.IndexOf("<&>");
                }
                SqlString += TempString;

                PLCSQLString = SqlString;

            }

            foreach (KeyValuePair<string, string> kv in userparams)
            {
                string keytag = "{" + kv.Key + "}";
                PLCSQLString = PLCSQLString.Replace(keytag, kv.Value.ToString());
            }

            SqlString = PLCSQLString;
            SqlString = SqlString.Replace("%USER%", "'" + PLCSession.PLCGlobalAnalyst + "'");
            SqlString = SqlString.Replace("%user%", "'" + PLCSession.PLCGlobalAnalyst + "'");
            //SqlString = SqlString.Replace("%SR_REQUIRES_APPROVAL_STATUS%", "'" + GetDeptNameField("SR_REQUIRES_APPROVAL_STATUS", PLCSession.PLCGlobalAnalystDepartmentCode) + "'");
            PLCSQLString = SqlString;

        }

        // Ex. AddAdditionalSqlColumn("DELIVERY_STATUS AS \"Delivery\"") to add 'DELIVER_STATUS' to the result columns.
        public void AddAdditionalSqlColumn(string columnText)
        {
            if (!this.additionalColumns.Contains(columnText))
                this.additionalColumns.Add(columnText);
        }

        public void ClearAdditionalSqlColumn()
        {
            this.additionalColumns.Clear();
        }

        // Return first occurrence of FROM found in sql statement or -1 if none.
        private int GetSqlFromOffset(string sqlstr)
        {
            string uppersql = sqlstr.ToUpper();

            Regex regex = new Regex(@"\sfrom\s", RegexOptions.IgnoreCase);
            Match match = regex.Match(uppersql);

            if (match.Success)
                return match.Index;
            else
                return -1;
        }

        // Return last occurrence of FROM found in sql statement or -1 if none.
        private int GetSqlLastFromOffset(string sqlstr)
        {
            string uppersql = sqlstr.ToUpper().Replace("\r", " ").Replace("\n", " ");

            return uppersql.LastIndexOf(" FROM ");
        }

        public void ProcessAdditionalColumns()
        {
            // Return if there are no additional columns to add.
            if (this.additionalColumns.Count == 0)
                return;

            // leftSql = sql part before FROM statement
            // rightSql = sql part starting and extending from FROM statement 
            int startOfFrom = GetSqlLastFromOffset(this.PLCSQLString);
            string leftSql = this.PLCSQLString.Substring(0, startOfFrom);
            string rightSql = this.PLCSQLString.Substring(startOfFrom);

            if (!leftSql.Contains("*"))     // Don't add additional columns if sql statement starts with SELECT *
            {
                foreach (string col in this.additionalColumns)
                    leftSql += String.Format(", {0}", col);
            }

            // Return new sql statement with additional SELECT columns.
            this.PLCSQLString = leftSql + rightSql;
        }

        public void CreatePLCGridColumnsOnly(DataTable MyDataTable)
        {
            this.Columns.Clear();
            if (MyDataTable == null) return;

            // remember AllowPaging property so it could be restored
            if (this.DefaultAllowPaging == null)
                this.DefaultAllowPaging = this.AllowPaging;

            // remember Height property so it could be restored
            if (this.DefaultHeight == null)
            {
                if (this.UsesScrollbar
                    && this.Height == Unit.Empty) // set default height
                    this.DefaultHeight = Unit.Pixel(200);
                else
                    this.DefaultHeight = this.Height;
            }

            // set paging and height
            if ((this.UsesScrollbar
                    || !this.AllowPaging)
                && MyDataTable.Rows.Count < 1001) // set grid to scrollable if configured and record count < 1001
            {
                this.AllowPaging = false;

                if (this.HeightOnScrollEnabled != Unit.Empty) // prioritize HeightOnScrollEnabled over Height
                    this.Height = this.HeightOnScrollEnabled;
                else
                    this.Height = (Unit)this.DefaultHeight;
            }
            else // restore the properties before it became scrollable
            {
                this.AllowPaging = (bool)this.DefaultAllowPaging;

                if (!this.AllowPaging
                    && this.HeightOnScrollEnabled != Unit.Empty) // prioritize HeightOnScrollEnabled over Height for grid that defaults to AllowPaging = false
                    this.Height = this.HeightOnScrollEnabled;
                else
                    this.Height = (Unit)this.DefaultHeight;
            }

            if (MaxRowCount > 0)
            {
                this.AllowPaging = MyDataTable.Rows.Count > MaxRowCount;
            }

            // set page size
            if (this.AllowPaging
                && this.RecordsPerPage > 0)
                this.PageSize = this.RecordsPerPage;

            if (FirstColumnCheckbox)
                this.Columns.Add(new PLCCheckBoxField());

            DataTable MyDataTable_SQL = QueryDBGridDL(this.PLCGridName);
            if (MyDataTable_SQL.Rows.Count > 0)
            {
                int TotalWidth = 0;
                foreach (DataRow MyRow in MyDataTable_SQL.Rows)
                {
                    DataControlField field;
                    string fieldName = null;
                    string fieldHeader = null;
                    PLCFieldType fieldType = PLCFieldType.BOUND;
                    Dictionary<string, string> fieldOptions = new Dictionary<string, string>();
                    int MyFieldWidth = 0;
                    bool isDate = false;

                    foreach (DataColumn column in MyRow.Table.Columns)
                    {
                        string MyColName = column.ColumnName;

                        if ((MyColName == "Field Name") || (MyColName == "FIELD_NAME"))
                        {
                            fieldName = MyRow[column].ToString();

                            if (fieldName.ToUpper().StartsWith("DATE_") || fieldName.ToUpper().Contains("_DATE"))
                                isDate = true;
                        }
                        else if ((MyColName == "Field Header") || (MyColName == "FIELD_HEADER"))
                            fieldHeader = string.IsNullOrEmpty(fieldHeader) ? MyRow[column].ToString() : fieldHeader;
                        else if ((MyColName == "Prompt Code") || (MyColName == "PROMPT_CODE"))
                            fieldHeader = PLCSession.GetSysPrompt(MyRow[column].ToString(), fieldHeader);
                        else if ((MyColName == "Field Width") || (MyColName == "FIELD_WIDTH"))
                        {
                            Int32.TryParse(MyRow[column].ToString(), out MyFieldWidth);
                            TotalWidth += MyFieldWidth;
                        }
                        else if ((MyColName == "Field Type") || (MyColName == "FIELD_TYPE"))
                        {
                            if (MyRow[column] != null && !string.IsNullOrEmpty(MyRow[column].ToString()))
                            {
                                fieldType = (PLCFieldType)Enum.Parse(typeof(PLCFieldType), MyRow[column].ToString(), true);
                            }
                        }
                        else if (MyColName.ToUpper() == "OPTIONS")
                        {
                            if (MyRow[column] != null && !string.IsNullOrEmpty(MyRow[column].ToString()))
                            {
                                string[] arrOptions = MyRow[column].ToString().Split(';');

                                foreach (string option in arrOptions)
                                {
                                    if (option != string.Empty)
                                    {
                                        string[] arrOption = option.ToUpper().Split(':');
                                        fieldOptions.Add(arrOption[0].Trim(), arrOption[1].Trim());
                                    }
                                }
                            }
                        }
                    }

                    switch (fieldType)
                    {
                        case PLCFieldType.CHECKBOX:
                            field = AddCheckBoxField(fieldName, fieldHeader, MyFieldWidth, fieldOptions);
                            break;
                        case PLCFieldType.BUTTON: // Three types of button, put in options dictionary under key "BUTTONTYPE"
                            field = AddButtonField(fieldName, fieldHeader, MyFieldWidth, fieldOptions);
                            break;
                        //case PLCFieldType.HYPERLINK: // For future use
                        //break;
                        case PLCFieldType.DATE:
                            field = AddDateField(fieldName, fieldHeader, MyFieldWidth);
                            break;
                        case PLCFieldType.DATEDMY:
                            field = AddDateFieldDMY(fieldName, fieldHeader, MyFieldWidth);
                            break;
                        case PLCFieldType.TIME24:
                            field = AddTime24Field(fieldName, fieldHeader, MyFieldWidth);
                            break;
                        case PLCFieldType.TIMEAMPM:
                            field = AddTimeAMPMField(fieldName, fieldHeader, MyFieldWidth);
                            break;
                        case PLCFieldType.DATETIME24:
                            field = AddDateTime24Field(fieldName, fieldHeader, MyFieldWidth);
                            break;
                        default: // either FIELD_TYPE has no value or is "BOUND"
                            field = AddBoundField(fieldName, fieldHeader, MyFieldWidth, isDate, fieldOptions);
                            break;
                    }
                    this.Columns.Add(field);
                }
                this.Width = TotalWidth;

                try
                {
                    this.DataBind();
                }
                catch (Exception e)
                {
                    PLCSessionVars sv = new PLCSessionVars();
                    sv.PLCErrorProc = "PLCDBGRID.CreatePLCGridColumnsOnly(" + this.PLCGridName + ")";
                    sv.PLCErrorSQL = e.Message + Environment.NewLine + e.StackTrace;
                    sv.SaveError();
                    sv.Redirect("~/PLCWebCommon/Error.aspx");
                }
            }
            else  // All the columns from the PLCSqlString
            {
                foreach (DataColumn column in MyDataTable.Columns)
                {
                    bool isDate = column.DataType.ToString() == "System.DateTime";
                    this.Columns.Add(AddBoundField(column.ColumnName, column.ColumnName, -1, isDate, null));
                }
                this.DataBind();
            }
        }

        public void CreatePLCGridColumns(bool refreshData = false)
        {
            if (!String.IsNullOrEmpty(PLCConnectionString))
            {
                LoadDBGridHDSettings();

                if (!String.IsNullOrEmpty(PLCSQLString))
                {
                    PLCSessionVars sv = new PLCSessionVars();
                    PLCSQLString = sv.FormatSpecialFunctions(PLCSQLString);

                    ProcessAdditionalSqlScript();
                    ProcessMacro();
                    ProcessAdditionalColumns();

                    if (PLCSession.GetDateFormat().ToUpper().StartsWith("DD")) //Checking... not all PLCDBGRID uses DBGRIDHD.
                        PLCSQLString = PLCSQLString.Replace(", 101)", ", 103)").Replace(",101", ",103");

                    PLCQuery qry = (!string.IsNullOrEmpty(CustomConnectionString) ? new PLCQuery(PLCSQLString, CustomConnectionString) : new PLCQuery(PLCSQLString));
                    DataTable dataTable;
                    if (UseDataCaching)
                    {
                        string dataCacheName = PLCGridName + "_datatable";
                        if (!refreshData && CacheHelper.IsInUserCache(dataCacheName))
                        {
                            dataTable = (DataTable)CacheHelper.GetUserItem(dataCacheName);
                        }
                        else
                        {
                            qry.OpenReadOnly();
                            dataTable = qry.PLCDataTable;
                            CacheHelper.AddUserItem(dataCacheName, dataTable);
                        }
                    }
                    else
                    {
                        qry.OpenReadOnly();
                        dataTable = qry.PLCDataTable;
                    }

                    // merge from SLED PRELOG Revision#15507
                    Boolean filterFound = false;
                    String searchText = PLCFilterString;

                    if (!String.IsNullOrWhiteSpace(searchText))
                    {
                        searchText = searchText.ToUpper().Trim();

                        List<String> visFields = new List<String>();

                        //Get a list of the visible fields
                        DataTable visDataTable = QueryDBGridDL(this.PLCGridName);
                        foreach (DataRow dlRow in visDataTable.Rows)
                        {
                            String visColName = dlRow["FIELD_NAME"].ToString();
                            Boolean isHidden = (dlRow["HIDE_FIELD"].ToString() == "T");
                            if (!isHidden)
                                visFields.Add(visColName);
                        }

                        foreach (DataRow r in qry.PLCDataTable.Rows)
                        {
                            filterFound = false;

                            foreach (DataColumn c in qry.PLCDataTable.Columns)
                            {
                                String cName = c.ColumnName;
                                if (!visFields.Contains(cName))
                                {
                                    // skip if not visible
                                    continue;

                                }

                                String txtTarget = r[cName].ToString();
                                if (!String.IsNullOrWhiteSpace(txtTarget)) txtTarget = txtTarget.ToUpper();
                                if (txtTarget.Contains(searchText))
                                    filterFound = true;
                            }

                            if ((!String.IsNullOrWhiteSpace(searchText)) && !filterFound)
                                r.Delete();
                        }
                    }
                    // end merge from SLED PRELOG

                    DataView datasourceView = new DataView(dataTable);

                    string sortExpression = Attributes["SortExpression"];
                    if (!String.IsNullOrEmpty(sortExpression))
                    {
                        sortExpression = sortExpression.TrimEnd(",".ToCharArray());
                        datasourceView.Sort = sortExpression;
                    }

                    this.DataSource = datasourceView;
                    CreatePLCGridColumnsOnly(dataTable);
                }
                else
                {
                    var error = "Missing select statement / Grid Name";
                    if (!string.IsNullOrEmpty(this.PLCGridName))
                        error = "Missing DBGrid Configuration (" + this.PLCGridName + ")";
                    throwException(error);
                }
            }
            else
            {
                throwException("Connection String is missing");
            }
        }

        public void LoadDBGridHDSettings()
        {
            if (!String.IsNullOrEmpty(PLCGridName))
            {
                DataTable MyDataTable_SQL = QueryDBGridHD(PLCGridName);
                if (MyDataTable_SQL.Rows.Count > 0)
                {
                    DataRow row = MyDataTable_SQL.Rows[0];
                    if (!String.IsNullOrEmpty(row["SQL_STRING"].ToString()))
                        PLCSQLString = row["SQL_STRING"].ToString();

                    if (MyDataTable_SQL.Columns.IndexOf("GRID_WIDTH") > -1)
                    {
                        int gridWidth = 0;
                        if (int.TryParse(row["GRID_WIDTH"].ToString(), out gridWidth) && gridWidth > 0)
                            PLCGridWidth = gridWidth;
                    }

                    if (MyDataTable_SQL.Columns.IndexOf("USES_SCROLLBAR") > -1)
                    {
                        UsesScrollbar = row["USES_SCROLLBAR"].ToString().Trim().ToUpper() == "T" || UsesScrollbar;
                    }

                    if (MyDataTable_SQL.Columns.IndexOf("GRID_HEIGHT") > -1)
                    {
                        int gridHeight = 0;
                        if (int.TryParse(row["GRID_HEIGHT"].ToString(), out gridHeight) && gridHeight > 0)
                            HeightOnScrollEnabled = gridHeight;
                    }

                    if (MyDataTable_SQL.Columns.IndexOf("RECORDS_PER_PAGE") > -1)
                    {
                        int gridPageCount = 0;

                        if (int.TryParse(row["RECORDS_PER_PAGE"].ToString(), out gridPageCount)
                            && gridPageCount > 0)
                            RecordsPerPage = gridPageCount;
                    }
                }
            }
        }

        private void throwException(string errorMessage)
        {
            PLCSessionVars PLCSessionVars1 = new PLCSessionVars();
            PLCSessionVars1.ClearError();
            PLCSessionVars1.PLCErrorURL = "*";
            PLCSessionVars1.PLCErrorProc = "PLCDBGrid.CreatePLCGridColumns";
            PLCSessionVars1.PLCErrorMessage = errorMessage;
            PLCSessionVars1.SaveError();
            PLCSessionVars1.Redirect("~/PLCWebCommon/Error.aspx");
        }

        public void InitializePLCDBGrid(bool refreshData = true)
        {
            PLCSessionVars sv = new PLCSessionVars();
            PLCConnectionString = sv.GetConnectionString();
            CreatePLCGridColumns(refreshData);

            if (!string.IsNullOrEmpty(this.PLCGridName))
                PLCSession.WriteDebug(PLCSession.PLCBEASTiLIMSVersion + " DBGridHD Name: " + this.PLCGridName, true);
        }

        public bool SelectRowByDataKeyIfExists(string datakey)
        {
            int currPageIdx = this.PageIndex;

            if (this.Rows.Count > 0)
            {
                for (int intPage = 0; intPage < this.PageCount; intPage++)
                {
                    this.PageIndex = intPage;
                    this.DataBind();
                    for (int i = 0; i < this.DataKeys.Count; i++)
                    {
                        if (Convert.ToString(this.DataKeys[i].Value) == datakey)
                        {
                            // If it is a match set the variables and exit
                            this.SelectedIndex = i;
                            this.PageIndex = intPage;
                            return true;
                        }
                    }
                }

                this.PageIndex = currPageIdx;
                this.DataBind();
            }

            return false;
        }

        public void SelectRowByDataKey(string datakey)
        {
            int currPageIdx = this.PageIndex;

            if (this.Rows.Count > 0)
            {
                for (int intPage = 0; intPage < this.PageCount; intPage++)
                {
                    this.PageIndex = intPage;
                    this.DataBind();
                    for (int i = 0; i < this.DataKeys.Count; i++)
                    {
                        if (Convert.ToString(this.DataKeys[i].Value) == datakey)
                        {
                            // If it is a match set the variables and exit
                            this.SelectedIndex = i;
                            this.PageIndex = intPage;
                            return;
                        }
                    }
                }

                this.PageIndex = currPageIdx;
                this.DataBind();
            }
        }

        public void SelectRowByDataKeyValues<T>(Dictionary<string, T> keyValueList)
        {
            if (this.Rows.Count > 0)
            {
                for (int intPage = 0; intPage < this.PageCount; intPage++)
                {
                    this.PageIndex = intPage;
                    this.DataBind();
                    for (int i = 0; i < this.DataKeys.Count; i++)
                    {
                        bool selectedItem = true;
                        foreach (KeyValuePair<string, T> kvp in keyValueList)
                        {
                            selectedItem &= kvp.Value.Equals(Convert.ChangeType(this.DataKeys[i][kvp.Key], typeof(T)));
                        }

                        if (selectedItem)
                        {
                            // If it is a match set the variables and exit
                            this.SelectedIndex = i;
                            this.PageIndex = intPage;
                            return;
                        }
                    }
                }
            }
        }

        public void CheckRowsByDataKey(string DataKeyName, string DataKeyValue, DataTable data = null)
        {
            if (this.Rows.Count > 0 && FirstColumnCheckbox)
            {
                string selectedState = ((HiddenField)HeaderRow.FindControl("cbSelect_state")).Value;

                CreatePLCGridColumns();

                DataTable tbl = null;

                if (this.DataSource != null)
                {
                    tbl = ((DataView)this.DataSource).ToTable();
                }
                else if (data != null)
                {
                    tbl = data;
                }
                else
                {
                    string sql = this.PLCSQLString +
                        (this.PLCSQLString.Contains(this.PLCSQLString_AdditionalCriteria) ? string.Empty :
                         " " + this.PLCSQLString_AdditionalCriteria) +
                        (string.IsNullOrEmpty(this.Attributes["SortExpression"]) ? string.Empty :
                            " ORDER BY " + this.Attributes["SortExpression"].TrimEnd(','));

                    PLCQuery qry = new PLCQuery(PLCSession.FormatSpecialFunctions(sql));
                    qry.Open();
                    if (qry.HasData())
                    {
                        tbl = qry.PLCDataTable;
                    }
                }

                if (tbl != null)
                {
                    StringBuilder newSelectedState = new StringBuilder(string.Empty);

                    int counter = 0;
                    int total = tbl.Rows.Count;
                    while (counter < total)
                    {
                        if (selectedState[counter] == '0')
                        {
                            if (Convert.ToString(tbl.Rows[counter][DataKeyName]) == DataKeyValue)
                            {
                                newSelectedState.Append('1');
                            }
                            else
                            {
                                newSelectedState.Append('0');
                            }
                        }
                        else
                            newSelectedState.Append('1');

                        counter++;
                    }

                    ((HiddenField)HeaderRow.FindControl("cbSelect_state")).Value = newSelectedState.ToString();
                    LoadCheckedRows(false, newSelectedState.ToString());
                }
            }
        }

        public void SetClientSideScrollToSelectedRow()
        {
            string script = String.Format("PLCDBGrid_SetClientSideScrollToSelectedRow('{0}', {1}, {2});", this.ClientID, this.Rows.Count, this.SelectedIndex);
            SetClientSideScript(script, "PLCDBGrid_SetClientSideScrollToSelectedRow");
        }

        private void SetClientSideScript(string script, string scriptIDPrefix)
        {
            ScriptManager sm = ScriptManager.GetCurrent(this.Page);
            string key = scriptIDPrefix + "_" + this.ClientID;
            if (sm != null)
                ScriptManager.RegisterStartupScript(this.Page, this.GetType(), key, script.ToString(), true);
            else
                this.Page.ClientScript.RegisterStartupScript(this.GetType(), key, script.ToString(), true);
        }

        public void SetControlMode(bool Enable)
        {
            this.Enabled = Enable;
        }

        public int ScrollPosition
        {
            get
            {
                if (ViewState["ScrollPosition"] != null)
                    return (int)ViewState["ScrollPosition"];
                else
                    return 0;
            }
            set
            {
                ViewState["ScrollPosition"] = value;
            }
        }

        /// <summary>
        /// Set the value that defines the maximum row count.
        /// If grid row count is greater than MaxRowCount, apply paging to true. Otherwise, apply false.
        /// If not set, logic will be as it is.
        /// </summary>
        [DefaultValue(0)]
        public int MaxRowCount
        {
            get
            {
                if (ViewState["MaxRowCount"] != null)
                    return (int)ViewState["MaxRowCount"];
                else
                    return 0;
            }
            set
            {
                ViewState["MaxRowCount"] = value;
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (this.Height.Value > 0)
            {
                ScriptManager sm = ScriptManager.GetCurrent(this.Page);

                //register javascript blocks
                string key = "ScrollingGridView";
                StringBuilder script = new StringBuilder();
                script.AppendLine("function saveScrollPos(whereID, whatID) {");
                script.AppendLine("  document.getElementById(whereID).value = document.getElementById(whatID).scrollTop;");
                script.AppendLine("}");
                script.AppendLine("function setScrollPos(whereID, whatID) {");
                script.AppendLine("  document.getElementById(whatID).scrollTop = (document.getElementById(whereID).value.length > 0) ? document.getElementById(whereID).value : 0;");
                script.AppendLine("}");

                if (sm != null)
                    ScriptManager.RegisterStartupScript(this.Page, this.GetType(), key, script.ToString(), true);
                else
                    this.Page.ClientScript.RegisterStartupScript(this.GetType(), key, script.ToString(), true);

                //register hidden field to save scroll position
                if (sm != null)
                    ScriptManager.RegisterHiddenField(this.Page, String.Format("{0}_scroll", this.ClientID), this.ScrollPosition.ToString());
                else
                    this.Page.ClientScript.RegisterHiddenField(String.Format("{0}_scroll", this.ClientID), this.ScrollPosition.ToString());

                //register startup javascripts
                key = String.Format("{0}_setScrollPos", this.ClientID);
                script = new StringBuilder();
                script.AppendLine("setScrollPos('" + String.Format("{0}_scroll", this.ClientID) + "','" + String.Format("{0}_div", this.ClientID) + "');");
                if (sm != null)
                    ScriptManager.RegisterStartupScript(this.Page, this.GetType(), key, script.ToString(), true);
                else
                    this.Page.ClientScript.RegisterStartupScript(this.GetType(), key, script.ToString(), true);


                string scrolldivID = String.Format("{0}_div", this.ClientID);

                if (sm != null)
                {
                    ScriptManager.RegisterStartupScript(this.Page, this.GetType(), String.Format("{0}_PLCDBGridHeadingRow", this.ClientID),
                        String.Format("PLCDBGrid_SeparateHeadingRowToFixedPos('{0}');", scrolldivID), true);
                }
                else
                {
                    this.Page.ClientScript.RegisterStartupScript(this.GetType(), String.Format("{0}_PLCDBGridHeadingRow", this.ClientID),
                        String.Format("PLCDBGrid_SeparateHeadingRowToFixedPos('{0}');", scrolldivID));
                }
            }
        }

        bool IPostBackDataHandler.LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
        {
            //get and set the new scroll position
            string postedValue = postCollection[this.ClientID + "_scroll"];
            if ((postedValue != null))
            {
                int presentValue = this.ScrollPosition;
                if (postedValue.Contains("."))
                    this.ScrollPosition = Convert.ToInt32(Convert.ToDecimal(postedValue));
                else
                    this.ScrollPosition = Convert.ToInt32(postedValue);
                return !presentValue.Equals(this.ScrollPosition);
            }

            return false;
        }

        void IPostBackDataHandler.RaisePostDataChangedEvent()
        {

        }

        protected override void Render(HtmlTextWriter output)
        {
            if (!Visible)
                return;

            string heightStyle = "";
            string onScrollScript = "";
            if (this.Height.Value > 0)
            {
                heightStyle = " height: " + Convert.ToInt32(this.Height.Value) + "px; overflow-y: auto;";
                this.Height = 0;
                onScrollScript = " onscroll=\"saveScrollPos('" + String.Format("{0}_scroll", this.ClientID) + "', '" + String.Format("{0}_div", this.ClientID) + "');\"";
            }

            // Capture the default html output of the page
            StringWriter writer = new StringWriter();
            HtmlTextWriter buffer = new HtmlTextWriter(writer);
            base.Render(buffer);

            string htmlMarkup = writer.ToString();
            htmlMarkup = htmlMarkup.Replace("<div>", "<div id='" + String.Format("{0}_div", this.ClientID) + "' class='plcdbgridScroll' style='width: " + PLCGridWidth.ToString() + ";" + heightStyle + "'" + onScrollScript + ">");

            // now write the html to the browser
            output.Write(htmlMarkup);
            return;
        }

        private void LoadCheckedRows(Boolean headerChecked, string rowIndexes)
        {
            if (Rows.Count > 0)
            {
                ((CheckBox)HeaderRow.FindControl("cbSelect_All")).Checked = headerChecked;
                ((HiddenField)HeaderRow.FindControl("cbSelect_state")).Value = rowIndexes;

                foreach (GridViewRow row in Rows)
                    ((CheckBox)row.FindControl("cbSelect")).Checked = rowIndexes[row.DataItemIndex] == '1';
            }
        }

        public List<DataKey> GetCheckedDataKeys()
        {
            List<DataKey> result = new List<DataKey>();
            if (Rows.Count > 0)
            {
                bool headerChecked = ((CheckBox)HeaderRow.FindControl("cbSelect_All")).Checked;
                string rowIndexes = ((HiddenField)HeaderRow.FindControl("cbSelect_state")).Value;

                CreatePLCGridColumns();

                OrderedDictionary dictionary;
                DataView dt = (DataView)DataSource;
                for (int i = 0; i < rowIndexes.Length; i++)
                    if (rowIndexes[i] == '1')
                    {
                        dictionary = new OrderedDictionary();
                        foreach (String keyname in DataKeyNames)
                            dictionary.Add(keyname, dt[i].Row[keyname]);
                        result.Add(new DataKey(dictionary));
                    }

                LoadCheckedRows(headerChecked, rowIndexes);
            }
            return result;
        }

        public void CheckRowByDataKeys(List<DataKey> dataKeys)
        {
            if (this.Rows.Count > 0)
            {
                var rowIndexes = "".PadLeft(((DataView)DataSource).Count, '0');

                int originalPageIndex = this.PageIndex;
                for (int intPage = 0; intPage < this.PageCount; intPage++)
                {
                    this.PageIndex = intPage;
                    this.DataBind();
                    for (int i = 0; i < this.DataKeys.Count; i++)
                    {
                        if (dataKeys.Contains(this.DataKeys[i]))
                        {
                            int index = (intPage * this.PageSize) + i;
                            rowIndexes = rowIndexes.Remove(index, 1);
                            rowIndexes = rowIndexes.Insert(index, "1");
                        }
                    }
                }

                ((HiddenField)HeaderRow.FindControl("cbSelect_state")).Value = rowIndexes;

                this.PageIndex = originalPageIndex;
                this.DataBind();

                LoadCheckedRows(false, rowIndexes);
            }
        }

        public void CheckAllRows()
        {
            if (this.Rows.Count > 0)
            {
                var rowIndexes = "".PadLeft(((DataView)DataSource).Count, '1');
                ((HiddenField)HeaderRow.FindControl("cbSelect_state")).Value = rowIndexes;
                LoadCheckedRows(true, rowIndexes);
            }
        }

        /// <summary>
        /// Remove checked state to all rows
        /// </summary>
        public void UnCheckAllRows()
        {
            if (this.Rows.Count > 0)
            {
                var rowIndexes = "".PadLeft(((DataView)DataSource).Count, '0');
                ((HiddenField)HeaderRow.FindControl("cbSelect_state")).Value = rowIndexes;
                LoadCheckedRows(false, rowIndexes);
            }
        }

        #region DBGridEditor Methods
        public void InitializePLCDBGridForEditor()
        {
            PLCSessionVars sv = new PLCSessionVars();
            PLCConnectionString = sv.GetConnectionString();
            CreatePLCGridColumns(true);
            //CreatePLCGridColumnsForEditor();
        }

        private void CreatePLCGridColumnsForEditor()
        {
            if (!String.IsNullOrEmpty(PLCConnectionString))
            {
                if (!String.IsNullOrEmpty(PLCGridName))
                {
                    DataTable MyDataTable_SQL = QueryDBGridHD(this.PLCGridName);
                    if (MyDataTable_SQL.Rows.Count > 0)
                    {
                        PLCSQLString = this.PLCSQLString;

                        int GridWidthIdx = MyDataTable_SQL.Rows[0].Table.Columns.IndexOf("GRID_WIDTH");
                        if ((GridWidthIdx >= 0) && (MyDataTable_SQL.Rows[0].ItemArray[GridWidthIdx].ToString() != ""))
                        {
                            int GridWidth = Convert.ToInt32(MyDataTable_SQL.Rows[0].ItemArray[GridWidthIdx].ToString());
                            if (GridWidth > 0)
                                PLCGridWidth = GridWidth;
                        }
                    }
                }

                if (!String.IsNullOrEmpty(PLCSQLString))
                {
                    PLCSessionVars sv = new PLCSessionVars();
                    PLCSQLString = sv.FormatSpecialFunctions(PLCSQLString);

                    ProcessAdditionalSqlScript();
                    ProcessAdditionalColumns();

                    PLCQuery qry = new PLCQuery(PLCSQLString);
                    qry.OpenReadOnly();

                    DataView datasourceView = new DataView(qry.PLCDataTable);
                    this.DataSource = datasourceView;
                    CreatePLCGridColumnsOnly(qry.PLCDataTable);
                }
                else
                {
                    var error = "Missing select statement / Grid Name";
                    if (!string.IsNullOrEmpty(this.PLCGridName))
                        error = "Missing DBGrid Configuration (" + this.PLCGridName + ")";
                    throwException(error);
                }
            }
            else
            {
                throwException("Connection String is missing");
            }
        }
        #endregion

        public void ClearSortExpression()
        {
            Attributes["SortExpression"] = null;
            PLCSortExpression = null;
        }

        public void UnRegisterPageIndexChanging()
        {
            this.PageIndexChanging -= PLCDBGrid_PageIndexChanging;
        }

        public void UnRegisterSorting()
        {
            this.Sorting -= PLCDBGrid_Sorting;
        }

        /// <summary>
        /// Gets all the data keys of the grid
        /// </summary>
        /// <returns>List of data keys</returns>
        public List<DataKey> GetAllDataKeys()
        {
            List<DataKey> dataKeys = new List<DataKey>();
            int currPageIdx = this.PageIndex;

            if (this.Rows.Count > 0)
            {
                for (int intPage = 0; intPage < this.PageCount; intPage++)
                {
                    this.PageIndex = intPage;
                    this.DataBind();
                    for (int i = 0; i < this.DataKeys.Count; i++)
                    {
                        dataKeys.Add(this.DataKeys[i]);
                        this.PageIndex = intPage;
                    }
                }

                this.PageIndex = currPageIdx;
                this.DataBind();
            }

            return dataKeys;
        }

        #region 74783 - Configurable Row Color Condition

        #region Private Methods


        /// <summary>
        /// Generate the row color condition and save it into a global variable
        /// </summary>
        /// <param name="pstrRowCondition">The row color condition (Value-Color)</param>
        /// <param name="pstrColumnField">The column to be checked to know when to change the row color</param>
        /// <param name="pintColumnIndex">The reference to the column</param>
        /// <returns>Returns true if no errors are found</returns>
        private bool GetRowCondition(string pstrRowCondition, string pstrColumnField, int pintColumnIndex)
        {
            RowColorCondition objRowCondition = null;
            string strRowColor = string.Empty;

            // retrieve everything inside a curly bracket {}
            MatchCollection conditions = Regex.Matches(pstrRowCondition, @"(?<=\{).*?(?=\})");

            foreach (Match condition in conditions)
            {
                // retrieve the condition value enclosed on double qoutes ""
                Match conditionValue = Regex.Match(condition.Value.Replace('\"', '"'), @"(?<=\"").*?(?=\"")");

                // retrieve the value after "-
                Match color = Regex.Match(condition.Value, @"(?<=""(\s+)?-).*");

                if (color.Value == string.Empty)
                    strRowColor = "Yellow";
                else
                    strRowColor = color.Value;

                // save the condition into the global variable
                if (_lstRowColorConditions == null)
                    _lstRowColorConditions = new List<RowColorCondition>();

                if (_lstRowColorConditions.Find(rowCondition => rowCondition.RowCondition == conditionValue.Value.Trim() && rowCondition.DataField.Trim() == pstrColumnField) == null)
                {
                    objRowCondition = new RowColorCondition(conditionValue.Value.Trim().ToUpper(), color.Value.Trim(), pstrColumnField, pintColumnIndex);
                    _lstRowColorConditions.Add(objRowCondition);
                }
            }

            return true;
        }

        /// <summary>
        /// Check to see if we can apply row colors
        /// </summary>
        /// <param name="pobjRowConditions">The row color conditions to meet</param>
        /// <param name="pobjGridRow">The grid row where we will apply the color</param>
        /// <param name="pstrCellValue">The current value of the cell in the grid</param>
        /// <param name="pstrColumnDataField">The column of the cell</param>
        /// <returns>Returns true if no errors are found</returns>
        private bool ApplyRowColor(RowColorCondition pobjRowConditions, GridViewRow pobjGridRow, string pstrCellValue)
        {
            // validate the condition
            if (ValidateCondition(pobjRowConditions, pstrCellValue))
            {
                pobjGridRow.BackColor = pobjRowConditions.BackColor;
            }

            return true;
        }

        /// <summary>
        /// Check to see if we can apply row colors
        /// </summary>
        /// <param name="plstRowConditions">The list of row color conditions to meet</param>
        /// <param name="pobjGridRow">The grid row where we will apply the color</param>
        /// <param name="pstrCellValue">The current value of the cell in the grid</param>
        /// <param name="pstrColumnDataField">The column of the cell</param>
        /// <returns>Returns true if no errors are found</returns>
        private bool ApplyRowColor(List<RowColorCondition> plstRowConditions, GridViewRow pobjGridRow, string pstrCellValue)
        {
            foreach (RowColorCondition objCondition in plstRowConditions)
            {
                // validate the condition
                if (ValidateCondition(objCondition, pstrCellValue))
                {
                    pobjGridRow.BackColor = objCondition.BackColor;
                }
            }

            return true;
        }

        /// <summary>
        /// Validate the condition for row color change
        /// </summary>
        /// <param name="pobjCondition">The condition object to be used to check the values</param>
        /// <param name="pstrCellValue">The current value of the cell in the grid</param>
        /// <param name="pstrColumnDataField">The column grid to be checked</param>
        /// <returns>Returns true if the condition for row color change has been met</returns>
        private bool ValidateCondition(RowColorCondition pobjCondition, string pstrCellValue)
        {
            bool blnIsValid = false;

            if (pobjCondition.UsesCodeTable)
            {
                blnIsValid = ValidateConditionByQuery(pobjCondition.CodeTable, pobjCondition.CodeField, pobjCondition.CodeDescription, pobjCondition.CodeValue, pstrCellValue.ToLower());
            }
            else
            {
                blnIsValid = pstrCellValue.ToLower() == pobjCondition.RowCondition.ToLower();
            }


            return blnIsValid;
        }

        /// <summary>
        /// Validate the condition using a query into the database
        /// </summary>
        /// <param name="pstrCodeTable">The table to search</param>
        /// <param name="pstrCodeField">The column to code used</param>
        /// <param name="pstrCodeDescription">The column of the description used</param>
        /// <param name="pstrCodeValue">The code value</param>
        /// <param name="pstrForValidation">The description value</param>
        /// <returns>Returns true if the a match is found</returns>
        private bool ValidateConditionByQuery(string pstrCodeTable, string pstrCodeField, string pstrCodeDescription, string pstrCodeValue, string pstrDescriptionValue)
        {
            bool blnHasValue = false;
            PLCQuery objQuery = null;
            string strQuery = string.Empty;

            // set the query
            strQuery = string.Format("SELECT {0} FROM {1} WHERE RTRIM(LTRIM(LOWER({0}))) = LOWER('{2}') AND RTRIM(LTRIM(LOWER({3}))) = LOWER('{4}')", pstrCodeField, pstrCodeTable, pstrCodeValue.Replace("'", "\'"), pstrCodeDescription, pstrDescriptionValue.Replace("'", @"''"));
            objQuery = new PLCQuery(strQuery);

            // do query
            objQuery.Open();
            if (objQuery.HasData())
                blnHasValue = true;

            return blnHasValue;
        }

        #endregion

        #endregion

        #region Public Methods

        /// <summary>
        /// Checks if the current grid has HD configs in the database. 
        /// </summary>
        public bool HasHDConfig()
        {
            var dataTable = QueryDBGridHD(PLCGridName);
            return dataTable != null && dataTable.Rows.Count > 0;
        }

        /// <summary>
        /// Applies sorting then binds the dataTable to PLCDBGrid
        /// </summary>
        /// <param name="e"></param>
        /// <param name="dataTable"></param>
        public void ApplySorting(GridViewSortEventArgs e, DataTable dataTable)
        {
            SetSortExpression(e.SortExpression);
            PLCSortExpression = Attributes["SortExpression"];

            DataBind(dataTable);
        }

        /// <summary>
        /// Binds the dataTable to PLCDBGrid
        /// </summary>
        /// <param name="dataTable"></param>
        public void DataBind(DataTable dataTable)
        {
            DataView datasourceView = new DataView(dataTable);

            string sortExpression = Attributes["SortExpression"];
            if (!String.IsNullOrEmpty(sortExpression))
            {
                sortExpression = sortExpression.TrimEnd(",".ToCharArray());
                datasourceView.Sort = sortExpression;
            }

            this.DataSource = datasourceView;
            this.DataBind();
        }

        #endregion

        private string GetDeptNameField(string fieldName, string departmentCode)
        {
            PLCQuery qryDeptName = new PLCQuery();
            qryDeptName.SQL = "SELECT " + fieldName + " FROM TV_DEPTNAME WHERE DEPARTMENT_CODE = '" + departmentCode + "'";
            qryDeptName.Open();

            if (qryDeptName.HasData())
                return qryDeptName.FieldByName(fieldName);
            else
                return string.Empty;
        }
    }

    //public class PLCSlidePanel : Panel
    //{

    //    public PLCSlidePanel()
    //    {
    //        this.Height = 300;

    //        pnlHeader = new Panel();
    //        pnlHeader.Height = HeaderPanel_Height;
    //        pnlHeader.BorderStyle = BorderStyle.Solid;
    //        pnlHeader.BorderWidth = 1;
    //        pnlHeader.Visible = true;
    //        pnlHeader.GroupingText = "Hello";

    //        SlideButton = new ImageButton();
    //        SlideButton.ImageUrl = ExpandImageUrl;
    //        SlideButton.Click += SlideButton_Click;
    //        SlideButton.AlternateText = "Collapse";
    //        pnlHeader.Controls.Add(SlideButton);

    //        Heading = new Label();
    //        Heading.Text = PanelHeading;
    //        pnlHeader.Controls.Add(Heading);

    //        pnlDetail = new Panel();
    //        pnlDetail.Height = Convert.ToInt16(this.Height.Value - HeaderPanel_Height);
    //        pnlDetail.BorderStyle = BorderStyle.Solid;
    //        pnlDetail.BorderWidth = 1;
    //        pnlDetail.GroupingText = "Hi";
    //        this.Controls.Add(pnlDetail);

    //        dynactrl();
    //    }

    //    private void dynactrl()
    //    {
    //    }

    //    private Panel pnlHeader;
    //    private Panel pnlDetail;
    //    private ImageButton SlideButton;
    //    private Label Heading;
    //    private int HeaderPanel_Height = 25;
    //    private string _PanelHeading = "Heading";

    //    [DefaultValue("")]
    //    [Bindable(true)]
    //    [Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
    //    [UrlProperty]
    //    [Category("PLC Properties"), Description("Image Expand Url")]
    //    public virtual string ExpandImageUrl { get; set; }

    //    [DefaultValue("")]
    //    [Bindable(true)]
    //    [Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
    //    [UrlProperty]
    //    [Category("PLC Properties"), Description("Image Collapse Url")]
    //    public virtual string CollapseImageUrl { get; set; }

    //    [Category("PLC Properties"), Description("Heading")]
    //    [Bindable(true)]
    //    [DefaultValue("")]
    //    [Localizable(true)]
    //    public string PanelHeading
    //    {
    //        get
    //        {
    //            return _PanelHeading;
    //        }
    //        set
    //        {
    //            _PanelHeading = value;
    //        }
    //    }

    //    protected void SlideButton_Click(object sender, ImageClickEventArgs e)
    //    {
    //        if (this.Height.Value > 30)
    //        {
    //            this.Height = 30;
    //            pnlDetail.Visible = false;
    //            SlideButton.ImageUrl = ExpandImageUrl;
    //            SlideButton.AlternateText = "Expand";
    //        }
    //        else
    //        {
    //            this.Height = Convert.ToInt16(pnlDetail.Height.Value + HeaderPanel_Height);
    //            pnlDetail.Visible = true;
    //            SlideButton.ImageUrl = CollapseImageUrl;
    //            SlideButton.AlternateText = "Collapse";
    //        }
    //    }


    //}
    //public string FormatSpecialFunctions(string SQLString)
    //{
    //    if (!String.IsNullOrEmpty(SQLString))
    //    {
    //        string TempSqlString = SQLString.ToUpper();
    //        string Temp1 = "";
    //        string Temp2 = "";
    //        string FieldName = "";

    //        int Index = TempSqlString.IndexOf("FORMATDATE(");

    //        if (Index >= 0)
    //        {
    //            Temp1 = SQLString.Substring(0, Index);
    //            TempSqlString = SQLString.Substring(Index + 11, SQLString.Length - (Index + 11));

    //            Index = TempSqlString.IndexOf(")");
    //            if (Index >= 0)
    //            {
    //                FieldName = TempSqlString.Substring(0, Index);
    //                Temp2 = TempSqlString.Substring(Index + 1, TempSqlString.Length - (Index + 1));

    //                PLCSessionVars sv = new PLCSessionVars();
    //                if (sv.PLCDatabaseServer == "MSSQL")
    //                    SQLString = Temp1 + " convert(char(10), " + FieldName + ", 101)" + Temp2;
    //                else
    //                    SQLString = Temp1 + " to_char(" + FieldName + ", 'mm/dd/yyyy')" + Temp2;
    //            }
    //        }

    //        TempSqlString = SQLString.ToUpper();
    //        Index = TempSqlString.IndexOf("FORMATTIME(");

    //        if (Index >= 0)
    //        {
    //            Temp1 = SQLString.Substring(0, Index);
    //            TempSqlString = SQLString.Substring(Index + 11, SQLString.Length - (Index + 11));

    //            Index = TempSqlString.IndexOf(")");
    //            if (Index >= 0)
    //            {
    //                FieldName = TempSqlString.Substring(0, Index);
    //                Temp2 = TempSqlString.Substring(Index + 1, TempSqlString.Length - (Index + 1));

    //                PLCSessionVars sv = new PLCSessionVars();
    //                if (sv.PLCDatabaseServer == "MSSQL")
    //                    SQLString = Temp1 + " convert(char(5), " + FieldName + ", 108)" + Temp2;
    //                else
    //                    SQLString = Temp1 + " to_char(" + FieldName + ", 'HH24:MI')" + Temp2;
    //            }
    //        }


    //        TempSqlString = SQLString.ToUpper();
    //        Index = TempSqlString.IndexOf("FORMATDATETIME(");

    //        if (Index >= 0)
    //        {
    //            Temp1 = SQLString.Substring(0, Index);
    //            TempSqlString = SQLString.Substring(Index + 15, SQLString.Length - (Index + 15));

    //            Index = TempSqlString.IndexOf(")");
    //            if (Index >= 0)
    //            {
    //                FieldName = TempSqlString.Substring(0, Index);
    //                Temp2 = TempSqlString.Substring(Index + 1, TempSqlString.Length - (Index + 1));

    //                PLCSessionVars sv = new PLCSessionVars();
    //                if (sv.PLCDatabaseServer == "MSSQL")
    //                    SQLString = Temp1 + " convert(char(10), " + FieldName + ", 101) convert(char(5), " + FieldName + ", 108)" + Temp2;
    //                else
    //                    SQLString = Temp1 + " to_char(" + FieldName + ", 'mm/dd/yyyy HH24:MI')" + Temp2;
    //            }
    //        }

    //        return SQLString;
    //    }
    //    else
    //        return SQLString;
    //}

    public class RowColorCondition
    {

        #region Private Members

        /// <summary>
        /// Index of the column in the grid
        /// </summary>
        private int _intColumnIndex;

        /// <summary>
        /// The description used by the code
        /// </summary>
        private string _strCodeDescription;

        /// <summary>
        /// The code field to be verified in the code table 
        /// </summary>
        private string _strCodeField;

        /// <summary>
        /// The code table to be checked if we are using the code as the condition
        /// </summary>
        private string _strCodeTable;

        /// <summary>
        /// The code value to be match to meet the condition
        /// </summary>
        private string _strCodeValue;

        /// <summary>
        /// The name of the column in the grid
        /// </summary>
        private string _strDataColumn;

        /// <summary>
        /// The row condition that was set
        /// </summary>
        private string _strRowCondition;

        /// <summary>
        /// The row color to be used when the condition is met
        /// </summary>
        private string _strRowColor;

        #endregion

        #region Public Properties

        /// <summary>
        /// The back color to be used when the condition is met
        /// </summary>
        public Color BackColor
        {
            get
            {
                if (_strRowColor.Contains("#"))
                {
                    return ColorTranslator.FromHtml(_strRowColor);
                }
                else
                {
                    return Color.FromName(_strRowColor);
                }
            }
        }

        /// <summary>
        /// The description used by the code
        /// </summary>
        public string CodeDescription
        {
            get
            {
                return _strCodeDescription;
            }
        }

        /// <summary>
        /// The code field to be verified in the code table 
        /// </summary>
        public string CodeField
        {
            get
            {
                return _strCodeField;
            }
        }

        /// <summary>
        /// The code table to be checked if we are using the code as the condition
        /// </summary>
        public string CodeTable
        {
            get
            {
                return _strCodeTable;
            }
        }

        /// <summary>
        /// The code value to be match to meet the condition
        /// </summary>
        public string CodeValue
        {
            get
            {
                return _strCodeValue;
            }
        }

        /// <summary>
        /// The column field to be verified
        /// </summary>
        public string DataField
        {
            get
            {
                return _strDataColumn;
            }
        }

        /// <summary>
        /// The column index to be verified
        /// </summary>
        public int ColumnIndex
        {
            get
            {
                return _intColumnIndex;
            }
        }

        /// <summary>
        /// Row color to be applied if the conditions are met. 
        /// </summary>
        public string RowColor
        {
            get
            {
                return _strRowColor;
            }
        }

        /// <summary>
        /// The condition to be verified to change the row color
        /// </summary>
        public string RowCondition
        {
            get
            {
                return _strRowCondition;
            }
        }

        /// <summary>
        /// Check if the condition set uses a code table
        /// </summary>
        public bool UsesCodeTable
        {
            get
            {
                return _strCodeTable != null;
            }
        }

        #endregion

        #region Static Functions

        /// <summary>
        /// Retrieve the color to change the row into if the conditions are met. 
        /// </summary>
        /// <param name="conditions">Conditions to check.</param>
        /// <param name="fieldValue">Field value to compare into.</param>
        public static string GetRowColor(List<RowColorCondition> conditions, string fieldValue)
        {
            string color = string.Empty;

            foreach(RowColorCondition condition in conditions)
            {
                if(condition.ValidateRowCondition(fieldValue))
                {
                    color = condition.RowColor;
                }
            }

            return color;
        }

        /// <summary>
        /// Retrieves the list of row color conditions based on the given rowhighlight option. 
        /// </summary>
        /// <param name="option">Rowhighlight options string</param>
        public static List<RowColorCondition> GetRowColorConditions(string option, string columnName)
        {
            List<RowColorCondition> rowColorConditions = null;
            string rowColor = string.Empty;

            // retrieve everything inside a curly bracket {}
            MatchCollection conditions = Regex.Matches(option.Trim(), @"(?<=\{).*?(?=\})");

            foreach (Match condition in conditions)
            {
                // retrieve the condition value enclosed on double qoutes ""
                Match conditionValue = Regex.Match(condition.Value.Replace('\"', '"'), @"(?<=\"").*?(?=\"")");

                // retrieve the value after "-
                Match color = Regex.Match(condition.Value, @"(?<=""(\s+)?-).*");

                if (color.Value == string.Empty)
                    rowColor = "Yellow";
                else
                    rowColor = color.Value;

                // save the condition into the global variable
                if (rowColorConditions == null)
                    rowColorConditions = new List<RowColorCondition>();

                if (rowColorConditions.Find(rowCondition => rowCondition.RowCondition == conditionValue.Value.Trim() && rowCondition.DataField.Trim() == columnName) == null)
                {
                    rowColorConditions.Add(new RowColorCondition(conditionValue.Value.Trim().ToUpper(), color.Value.Trim(), columnName, 0));
                }
            }

            return rowColorConditions;
        }

        

        #endregion

        #region Constructor

        /// <summary>
        /// Create a row color condition object and parse the condition if it is complex condition.
        /// </summary>
        /// <param name="condition">Criteria to match if it will apply the change in row color.</param>
        /// <param name="color">Color to change the row into.</param>
        public RowColorCondition(string condition, string color)
        {
            _strRowColor = color;

            _strRowCondition = condition;
            if (_strRowCondition.Contains('.') && _strRowCondition.Contains('='))
            {
                ParseCondition(_strRowCondition);
            }
        }

        public RowColorCondition(string pstrRowCondition, string pstrRowColor, string pstrDataColumn, int pintColumnIndex)
        {
            _intColumnIndex = pintColumnIndex;
            _strDataColumn = pstrDataColumn;
            _strRowColor = pstrRowColor;

            _strRowCondition = pstrRowCondition;
            if (_strRowCondition.Contains('.') && _strRowCondition.Contains('='))
            {
                ParseCondition(_strRowCondition);
            }
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Parse the condition to get the code table details into the global variables
        /// </summary>
        /// <param name="pstrCondition">The string to be parsed</param>
        /// <returns>Returns true if no errors are found</returns>
        private bool ParseCondition(string pstrCondition)
        {
            string strTempCodeTable = string.Empty;
            string strTempValue = string.Empty;

            // split by = to retrieve the code table fields and value to be matched
            strTempCodeTable = pstrCondition.Split('=')[0];
            strTempValue = pstrCondition.Split('=')[1];

            // split by . to get the code value and description
            _strCodeValue = strTempValue.Split('.')[0];

            // split by . to get the code table and column
            _strCodeTable = strTempCodeTable.Split('.')[0];
            _strCodeField = strTempCodeTable.Split('.')[1];
            _strCodeDescription = strTempCodeTable.Split('.')[2];

            return true;
        }

        /// <summary>
        /// Validate the field value if it meets the row condition.
        /// </summary>
        /// <param name="fieldValue">Value to check</param>
        private bool ValidateRowCondition(string fieldValue)
        {
            bool isValid = false;

            if (_strCodeTable != null)
            {
                isValid = ValidateRowConditionByQuery(_strCodeTable, _strCodeField, _strCodeDescription, _strCodeValue, fieldValue);
            }
            else
            {
                isValid = fieldValue.ToLower() == _strRowCondition.ToLower();
            }

            return isValid;
        }

        /// <summary>
        /// Validate the condition using a query into the database
        /// </summary>
        /// <param name="codeTable">The table to search</param>
        /// <param name="codeField">The column to code used</param>
        /// <param name="codeDescription">The column of the description used</param>
        /// <param name="codeValue">The code value</param>
        /// <param name="descriptionValue">The description value</param>
        /// <returns>Returns true if the a match is found</returns>
        private bool ValidateRowConditionByQuery(string codeTable, string codeField, string codeDescription, string codeValue, string descriptionValue)
        {
            bool hasValue = false;
            PLCQuery queryObject = null;
            string queryString = string.Empty;

            // set the query
            queryString = string.Format("SELECT {0} FROM {1} WHERE RTRIM(LTRIM(LOWER({0}))) = LOWER('{2}') AND RTRIM(LTRIM(LOWER({3}))) = LOWER('{4}')", codeField, codeTable, codeValue.Replace("'", "\'"), codeDescription, descriptionValue.Replace("'", @"''"));
            queryObject = new PLCQuery(queryString);

            // do query
            queryObject.Open();
            if (queryObject.HasData())
                hasValue = true;

            return hasValue;
        }

        #endregion
    }
}
