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
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Web.UI.Design;
using System.Web.UI.Design.WebControls;
using System.Web.Script.Serialization;
using AjaxControlToolkit;
using System.Net;
using System.Text.RegularExpressions;



#endregion using

namespace PLCCONTROLS
{
    public enum PromptType
    {
        Standard,
        Required,
        Mandatory
    };

    [Designer(typeof(PLCDBPanelDesigner)), ParseChildren(false)]
    [ToolboxData("<{0}:PLCDBPanel runat=\"server\"> </{0}:PLCDBPanel>")]
    public class PLCDBPanel : PLCControlPanel
    {
        public PLCDBPanel()
            : base()
        {
            SetDBPanelProperties();
            PLCSessionVars sv = new PLCSessionVars();
            _PLCConnectionString = sv.GetConnectionString();
        }

        //Declare The Events (See PLC_ControlEvents for definitions)
        public event PLCDBPanelAddCustomFieldsEventHandler PLCDBPanelAddCustomFields;
        public event PLCDBPanelValidateEventHandler PLCDBPanelValidate;
        public event PLCDBPanelGetNewRecordEventHandler PLCDBPanelGetNewRecord;
        public event PLCDBPanelCheckBoxChangedEventHandler PLCDBPanelCheckBoxChanged;
        public event PLCDBPanelCodeHeadChangedEventHandler PLCDBPanelCodeHeadChanged;
        public event PLCDBPanelTextChangedEventHandler PLCDBPanelTextChanged;

        //*AAC 061109
        [Bindable(true)]
        [Browsable(true)]
        [Category("PLC Properties"), Description("CSS Format")]
        [DefaultValue("")]
        [Localizable(true)]
        public static string PLCDBPanelCSS
        {
            get;
            set;
        }

        private string _PLCConnectionString;
        protected List<PanelRec> panelrecs;
        public Int16 HighestTabIndex = 0;
        private Int16 LastTabIndex;
        public bool useCaching = true;
		public int ReasonChangeKey = 0;

        private bool isUsesRequiredFieldsIndicator = PLCSession.GetLabCtrl("USES_DBPANEL_VALIDATOR") == "T" ? true : false;

        // Place reserved edit mask values here.
        private string[] reservedEditMasks = new string[] { "CB", "CB-R", "CHECKBOX", "COMBO", "COMBOBOX", "COMBOLIST", "MULTIPICK", "MULTIPICK_SEARCH", "RADIOLIST", "FREEFORMLIST" };

        //-------------------------------------------------------------------

        protected virtual void onplcdbpanelgetnewrecord(PLCDBPanelGetNewRecordEventArgs e)
        {
            if (PLCDBPanelGetNewRecord != null)
                PLCDBPanelGetNewRecord(this, e);
        }

        //-------------------------------------------------------------------

        protected virtual void onplcdbpanelvalidate(PLCDBPanelValidateEventArgs e)
        {
            if (PLCDBPanelValidate != null)
                PLCDBPanelValidate(this, e);
        }

        //-------------------------------------------------------------------

        protected virtual void onplcdbpaneladdcustomfields(PLCDBPanelAddCustomFieldsEventArgs e)
        {         
            if (PLCDBPanelAddCustomFields != null)
            {
                try
                {
                    PLCDBPanelAddCustomFields(this, e);
                }
                catch (Exception ex)
                {
                    PLCSession.WriteDebug("");
                    PLCSession.WriteDebug("---------- DBPANEL EXCEPTION ------------------");
                    PLCSession.WriteDebug("PANEL_NAME:" + this.PLCPanelName + "SQL: " + this.PLCDbPanelQuery);
                    PLCSession.WriteDebug("Exception in PLCDBPanelAddCustomFields:" + ex.Message + Environment.NewLine + ex.StackTrace);
                    PLCSession.WriteDebug("-----------------------------------------------");
                    PLCSession.WriteDebug("");
                    throw ex;
                }
            }
        }

        //-------------------------------------------------------------------
        protected virtual void OnPLCDBPanelCheckBoxChanged(PLCDBPanelCheckBoxChangedEventArgs e)
        {
            if (PLCDBPanelCheckBoxChanged != null)
                PLCDBPanelCheckBoxChanged(this, e);
        }

        protected virtual void OnPLCDBPanelCodeHeadChanged(PLCDBPanelCodeHeadChangedEventArgs e)
        {
            if (PLCDBPanelCodeHeadChanged != null)
                PLCDBPanelCodeHeadChanged(this, e);
        }

        protected virtual void OnPLCDBPanelTextChanged(PLCDBPanelTextChangedEventArgs e)
        {
            if (PLCDBPanelTextChanged != null)
                PLCDBPanelTextChanged(this, e);
        }

        //For PLCDBPanels that are created dynamically after a postback event (i.e. ButtonClick, SelectedIndexChanged), 
        //  the CreateControls method must be called after the postback event (i.e. in PageLoad, PagePreRender, PageRaisePostBackEvent),
        //  and the CreateControlsOnExplicitCall property must be set to true so that dynactrl_modal() is not called in this control's Init event.
        private bool _createControlsOnExplicitCall;
        public bool CreateControlsOnExplicitCall
        {
            get
            {
                return _createControlsOnExplicitCall;
            }
            set
            {
                _createControlsOnExplicitCall = value;
            }
        }

        //This is to bypass allowedit setting based on TV_DBPANEL.EDIT_ACCESS_CODES configuration on create of controls
        private bool _enableClientBehavior;
        public bool EnableClientBehavior
        {
            get
            {
                return _enableClientBehavior;
            }
            set
            {
                _enableClientBehavior = value;
            }
        }

        public void CreateControls()
        {
            this.Controls.Clear();
            this.panelrecs.Clear();

            dynactrl_modal();
        }

        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (this.Site != null)
            {
                if (this.Site.DesignMode)
                    return;
            }

            if (!CreateControlsOnExplicitCall)
                dynactrl_modal();

            if (!Page.IsPostBack && !IsBrowseMode && IsNewRecord)
            {
                PLCWhereClause = HttpContext.Current.Session[this.UniqueID + "_PLCWhereClause"].ToString();
                DoCancel();
            }
        }

        //-------------------------------------------------------------------
        [Bindable(true)]
        [Browsable(true)]
        [Category("PLC Properties"), Description("Set additional condition to filter controls in TV_DBPANEL, <space><AND><space><New Query>")]
        [DefaultValue(false)]
        [Localizable(true)]
        public virtual String PLCDbPanelQuery
        {
            get;
            set;
        }

        //-------------------------------------------------------------------

        [Bindable(true)]
        [Browsable(true)]
        [Category("PLC Properties"), Description("Set this property to True to display a <HR> element below the button panel")]
        [DefaultValue(false)]
        [Localizable(true)]
        public virtual Boolean PLCDisplayBottomBorder
        {
            get
            {
                if (ViewState["PLCDisplayBottomBorder"] == null)
                    return false;
                else
                {
                    Boolean b = (Boolean)ViewState["PLCDisplayBottomBorder"];
                    return b;
                }

            }

            set
            {
                ViewState["PLCDisplayBottomBorder"] = value;
            }
        }

        //-------------------------------------------------------------------

        [Bindable(true)]
        [Browsable(true)]
        [Category("PLC Properties"), Description("Code used for audit log")]
        [DefaultValue(false)]
        [Localizable(true)]
        public virtual int PLCAuditCode
        {
            get
            {
                if (ViewState["PLCAuditCode"] == null)
                    return 0;
                else
                {
                    int b = (int)ViewState["PLCAuditCode"];
                    return b;
                }

            }

            set
            {
                ViewState["PLCAuditCode"] = value;
            }
        }

        //-------------------------------------------------------------------

        [Bindable(true)]
        [Browsable(true)]
        [Category("PLC Properties"), Description("Sub-Code used for audit log")]
        [DefaultValue(true)]
        [Localizable(true)]
        public virtual int PLCAuditSubCode
        {
            get
            {
                if (ViewState["PLCAuditSubCode"] == null)
                    return 1;
                else
                {
                    int b = (int)ViewState["PLCAuditSubCode"];
                    return b;
                }

            }

            set
            {
                ViewState["PLCAuditSubCode"] = value;
            }
        }


        //-------------------------------------------------------------------

        [Bindable(true)]
        [Browsable(true)]
        [Category("PLC Properties"), Description("Delete code used for audit log")]
        [DefaultValue(false)]
        [Localizable(true)]
        public virtual int PLCDeleteAuditCode
        {
            get
            {
                if (ViewState["PLCDeleteAuditCode"] == null)
                    return 0;
                else
                {
                    int b = (int)ViewState["PLCDeleteAuditCode"];
                    return b;
                }

            }

            set
            {
                ViewState["PLCDeleteAuditCode"] = value;
            }
        }

        //-------------------------------------------------------------------

        [Bindable(true)]
        [Browsable(true)]
        [Category("PLC Properties"), Description("Delete sub-code used for audit log")]
        [DefaultValue(true)]
        [Localizable(true)]
        public virtual int PLCDeleteAuditSubCode
        {
            get
            {
                if (ViewState["PLCDeleteAuditSubCode"] == null)
                    return 2;
                else
                {
                    int b = (int)ViewState["PLCDeleteAuditSubCode"];
                    return b;
                }

            }

            set
            {
                ViewState["PLCDeleteAuditSubCode"] = value;
            }
        }

        //-------------------------------------------------------------------

        [Bindable(true)]
        [Browsable(true)]
        [Category("PLC Properties"), Description("Set this property to True to display a <HR> element on top of the button panel")]
        [DefaultValue(false)]
        [Localizable(true)]
        public virtual Boolean PLCDisplayTopBorder
        {
            get
            {
                if (ViewState["PLCDisplayTopBorder"] == null)
                    return false;
                else
                {
                    Boolean b = (Boolean)ViewState["PLCDisplayTopBorder"];
                    return b;
                }

            }

            set
            {
                ViewState["PLCDisplayTopBorder"] = value;
            }
        }

        //-------------------------------------------------------------------

        [Bindable(true)]
        [Browsable(true)]
        [Category("PLC Properties"), Description("This is the Where part of the SQL statement")]
        [DefaultValue("")]
        [Localizable(true)]
        public virtual string PLCWhereClause
        {
            get
            {
                String s = (String)ViewState["PLCWhereClause"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {

                string s = value;

                s = s.Replace("[", "\"");
                s = s.Replace("]", "\"");

                ViewState["PLCWhereClause"] = s;
            }
        }

        //-------------------------------------------------------------------

        [Category("PLC Properties"), Description("This is the name of the TABLE or VIEW which will be selected from")]
        [Bindable(true)]
        [Browsable(true)]
        [DefaultValue("")]
        [Localizable(true)]
        public virtual string PLCDataTable
        {
            get
            {
                String s = (String)ViewState["PLCDataTable"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["PLCDataTable"] = value;
            }
        }

        //-------------------------------------------------------------------

        [Bindable(true)]
        [Browsable(true)]
        [Category("PLC Properties"), Description("This is the name of the TABLE or VIEW which will be selected from")]
        [DefaultValue("")]
        [Localizable(true)]
        public virtual string PLCPanelName
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

        [Bindable(true)]
        [Browsable(true)]
        [Category("PLC Properties"), Description("Stores the cache state on/off")]
        [DefaultValue(false)]
        [Localizable(true)]
        public virtual Boolean PLCDisableCaching
        {
            get
            {
                if (ViewState["PLCDisableCaching"] == null)
                    return false;
                else
                {
                    Boolean s = (Boolean)ViewState["PLCDisableCaching"];
                    return (s);
                }
            }
            set
            {
                ViewState["PLCDisableCaching"] = value;
            }
        }

        //-------------------------------------------------------------------

        [Bindable(true)]
        [Browsable(true)]
        [Category("PLC Properties"), Description("Allow blank record to be saved in PLCDBPanel")]
        [DefaultValue(false)]
        [Localizable(true)]
        public virtual Boolean PLCAllowBlankRecordSave
        {
            get
            {
                if (ViewState["PLCAllowBlankRecordSave"] == null)
                    return false;
                else
                {
                    Boolean b = (Boolean)ViewState["PLCAllowBlankRecordSave"];
                    return b;
                }

            }

            set
            {
                ViewState["PLCAllowBlankRecordSave"] = value;
            }
        }

        [Bindable(true)]
        [Browsable(true)]
        [Category("PLC Properties"), Description("Disable DBPanel Default Value")]
        [DefaultValue(false)]
        [Localizable(true)]
        public virtual Boolean PLCDisableDefaultValue
        {
            get
            {
                if (ViewState["PLCDisableDefaultValue"] == null)
                    return false;
                else
                {
                    Boolean s = (Boolean)ViewState["PLCDisableDefaultValue"];
                    return (s);
                }
            }
            set
            {
                ViewState["PLCDisableDefaultValue"] = value;
            }
        }

        //-------------------------------------------------------------------

        public virtual bool IsBrowseMode
        {
            get
            {
                if (HttpContext.Current.Session[this.UniqueID + "_IsBrowseMode"] == null)
                {
                    HttpContext.Current.Session[this.UniqueID + "_IsBrowseMode"] = false;
                }

                return (bool)HttpContext.Current.Session[this.UniqueID + "_IsBrowseMode"];
            }
            set
            {
                HttpContext.Current.Session[this.UniqueID + "_IsBrowseMode"] = value;
            }
        }

        public bool IsSavingMode
        {
            get
            {
                if (HttpContext.Current.Session[this.UniqueID + "_IsSavingMode"] == null)
                {
                    HttpContext.Current.Session[this.UniqueID + "_IsSavingMode"] = false;
                }

                return (bool)HttpContext.Current.Session[this.UniqueID + "_IsSavingMode"];
            }
            set
            {
                HttpContext.Current.Session[this.UniqueID + "_IsSavingMode"] = value;
            }
        }

        public bool IsSearchPanel
        {
            get
            {
                if (ViewState["DBPSearchPanel"] == null)
                    ViewState["DBPSearchPanel"] = false;

                return Convert.ToBoolean(ViewState["DBPSearchPanel"]);
            }
            set
            {
                ViewState["DBPSearchPanel"] = value;
            }
        }

        /// <summary>In some cases, we want the flexbox popups to attach to another div container
        /// <example>this, body, .dbbox, #divTabs</example>
        /// </summary>
        public string PLCAttachPopupTo
        {
            get
            {
                String s = (String)ViewState["PLCAttachPopupTo"];
                return ((s == null) ? String.Empty : s);
            }
            set
            {
                ViewState["PLCAttachPopupTo"] = value;
            }
        }

        //-------------------------------------------------------------------

        public bool PLCConfigPanel
        {
            get
            {
                if (ViewState["PLCConfigPanel"] == null)
                    ViewState["PLCConfigPanel"] = false;

                return Convert.ToBoolean(ViewState["PLCConfigPanel"]);
            }
            set
            {
                ViewState["PLCConfigPanel"] = value;
            }
        }

        //-------------------------------------------------------------------

        protected void SetDBPanelProperties()
        {
            //  this.BackColor = Color.Blue;
            panelrecs = new List<PanelRec>();
        }

        //-------------------------------------------------------------------

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "tempvar", "var txt = '';", true);

            if (!Page.IsPostBack)
            {
                if (this.Site != null)
                {
                    if (this.Site.DesignMode)
                        return;
                }

                LoadRecord();

                PLCSession.WriteDebug(PLCSession.PLCBEASTiLIMSVersion + " DBPanel Name: " + this.PLCPanelName, true);
            }

//$$            if (!IsBrowseMode)
//                FocusFirstField();

            if (!IsBrowseMode && !Page.IsPostBack)
                FocusFirstField();

            SetDBPanelChildRequired();
            RefreshMultipickControls();

            String scripName = this.ID + "_plcdbpanellist";
            ScriptManager.RegisterStartupScript(this, this.GetType(), scripName, GetAddToListScript(), true);

            // put the code with labctrl and user option after this checking
            if ((_PLCConnectionString == "") || (_PLCConnectionString == "Provider=;Data Source=;User ID=;Password=;"))
                return;
            
            //if ((PLCSession.CheckUserOption("DBPEDIT") || PLCSession.GetWebUserField("SHOW_SCRIPT_CONFIGURATION") == "T") && !string.IsNullOrEmpty(this.PLCPanelName))
            //{
            //    string iconProps = "";
            //    string iconOnClick = "";

            //    if (PLCSession.CheckUserOption("DBPEDIT"))
            //    {
            //        string iconEditorUrl = ResolveClientUrl("~/DBPanelEditor.aspx") + "?name=" + this.PLCPanelName + "&detach=true";
            //        iconOnClick = @"
            //            $('#" + this.ID + @"_editdbpanelbutton').click(function() {
            //                var editorWin = window.open('" + iconEditorUrl + @"', 'DBPanelEditor', 'width=860, height=500, scrollbars=1, resizable=1', false)
            //                editorWin.focus();
            //            });
            //        ";
            //        iconProps += "style: 'cursor: pointer;',";
            //        iconProps += "title: 'Edit DBPanel [" + this.PLCPanelName + @"]',";
            //    }
            //    else if (PLCSession.GetWebUserField("SHOW_SCRIPT_CONFIGURATION") == "T")
            //        iconProps = "title: 'Panel Name: " + this.PLCPanelName + "',";

            //    string editDBPanelButton = @"
            //        if ($('#" + this.ID + @"_editdbpanelbutton').length <= 0) {
            //            $('#" + this.ClientID + @"').prepend($('<img/>', {
            //                " + iconProps + @"
            //                id: '" + this.ID + @"_editdbpanelbutton',
            //                src: '" + ResolveClientUrl("~/Images/edit-panel.png") + @"',
            //                height: '15px',
            //                width: '15px'
            //            }));" + iconOnClick + @"
            //        } 
            //    ";
            //    ScriptManager.RegisterStartupScript(this.Page, this.GetType(), this.ClientID + "_editDBPanelButton", editDBPanelButton, true);
            //}
        }


        //-------------------------------------------------------------------

        public void EditMode()
        {
            IsBrowseMode = false;

            foreach (PanelRec pr in panelrecs)
            {
                bool allowEnable = pr.AllowEdit;
                if (IsNewRecord)
                    allowEnable = pr.AllowAdd;

                if (pr.btn != null)
                    pr.btn.Enabled = true;


                if (pr.tb != null && allowEnable)
                {
                    pr.tb.BackColor = Color.White;
                    pr.tb.Attributes.Remove("readonly");

                    //show calendar button
                    if (pr.isDateMask(pr.editmask) && allowEnable)
                       ShowCalendarButton(pr, true);

                    if (pr.time != null)
                        SetTimeTextBoxMode(pr, false);
                }
                else if (pr.ddl != null && allowEnable)
                {
                    pr.ddl.Enabled = true;
                    SetDropDownListMode(pr.ddl, false);
                }
                else if (pr.rbl != null)
                {
                    SetRadioButtonListMode(pr.rbl, false);
                }
                else if (pr.cb != null && allowEnable)
                {
                    pr.cb.Enabled = true;
                }
                else if (pr.chMultiLookup != null && allowEnable)
                {
                    TextBox tb = pr.chMultiLookup.GetTextBox();
                    tb.BackColor = Color.White;
                    tb.ReadOnly = false;

                    pr.chMultiLookup.Enable(true);
                }
                else if (pr.chMultipickAc != null && allowEnable)
                {
                    TextBox tb = pr.chMultipickAc.GetTextBox();
                    tb.BackColor = Color.White;
                    tb.ReadOnly = false;

                    pr.chMultipickAc.Enable(true);
                }
                else if (pr._codetable != "")
                {
                    if (pr.chtb != null)
                    {
                        //string table_code : 
                        CodeHeadEnableControls(pr, true);

                        //string sCustomPostBackJS = string.Empty;

                        //if (pr.chSelectPostBackButton != null)
                        //{
                        //    sCustomPostBackJS = "if ((event.which == 9) || (event.keyCode == 9)) { var pb = document.getElementById('" + pr.chSelectPostBackButton.ClientID + "'); " + " if (pb != null) pb.click(); }";
                        //}

                        pr.chib.Attributes.Add("onclick", "cancelID = '" + pr.chCancelButton.ClientID + "'; " +
                                                          "bnSearchID = '" + pr.chbtnSearch.ClientID + "'; " +
                                                          "fnSetFocus('" + pr.chtxtSearch.ClientID + "');");
                        pr.chtb.Attributes.Add("onkeydown", "cancelID = '" + pr.chCancelButton.ClientID + "'; " +
                                                            "bnSearchID = '" + pr.chbtnSearch.ClientID + "'; " +
                                               "if(event.which || event.keyCode){if ((event.which == 118)" +
                                               " || (event.keyCode == 118)) {document.getElementById('" + pr.chib.ClientID + "').click();} " +
                                                //"if ((event.which == 9) || (event.keyCode == 9)) { document.getElementById('" +
                                                //pr.chTabPostBackButton.ClientID + "').click(); }" + sCustomPostBackJS + "
                                                "}");
                        // set focus on search textbox after every search button click
                        pr.chbtnSearch.Attributes.Add("onclick", "fnSetFocus('" + pr.chtxtSearch.ClientID + "');");
                        pr.chbtnRefresh.Attributes.Add("onclick", "fnSetFocus('" + pr.chtxtSearch.ClientID + "');");

                    }
                    else if (pr.chComboBox != null)
                    {
                        SetDropDownListMode(pr.chComboBox, false);
                    }
                    else if (pr.chFlexBox != null && allowEnable)
                    {
                        pr.chFlexBox.ReadOnly = false;
                    }
                }
            }

            SetDisplayFormulaFieldValue();

        }

        //-------------------------------------------------------------------

        public void SetMyFieldMode(string fldname, bool ReadOnly)
        {
            foreach (PanelRec pr in panelrecs)
            {
                if (pr.fldname.CompareTo(fldname) == 0)
                {
                    try
                    {
                        if (pr.tb != null)
                        {
                            pr.tb.ReadOnly = ReadOnly;

                            if (ReadOnly)
                            {
                                pr.tb.BackColor = Color.LightGray;
                            }
                            else
                                pr.tb.BackColor = Color.White;

                            // this is the calendar image button
                            if (pr.btn != null)
                                pr.btn.Enabled = !ReadOnly;

                            if (pr.time != null)
                                SetTimeTextBoxMode(pr, ReadOnly);

                            return;
                        }
                        else if (pr.ddl != null)
                        {
                            SetDropDownListMode(pr.ddl, ReadOnly);
                        }
                        else if (pr.rbl != null)
                        {
                            SetRadioButtonListMode(pr.rbl, ReadOnly);
                        }
                        else if (pr.chFlexBox != null)
                        {
							pr.chFlexBox.ReadOnly = ReadOnly;
						}
                    }
                    finally
                    {
                    }
                }
            }
        }


        //-------------------------------------------------------------------

        public void SetMyFieldModeEnableDisable(string fldname, bool Enable)
        {
            foreach (PanelRec pr in panelrecs)
            {
                if (pr.fldname.CompareTo(fldname) == 0)
                {
                    try
                    {
                        if (pr.tb != null)
                        {
                            pr.tb.Enabled = Enable;

                            if (pr.time != null)
                                SetTimeTextBoxMode(pr, !Enable);
                        }
                        else if (pr.ddl != null)
                        {
                            pr.ddl.Enabled = Enable;
                        }
                        else if (pr.rbl != null)
                        {
                            pr.rbl.Enabled = Enable;
                        }
                        else if (pr.chtb != null)
                        {
                            pr.chtb.Enabled = Enable;
                            pr.chib.Enabled = Enable;
                        }
                        else if (pr.chComboBox != null)
                        {
                            pr.chComboBox.Enabled = Enable;
                        }
                        else if (pr.chFlexBox != null)
                        {
                            pr.chFlexBox.Enabled = Enable;
                        }

                        return;

                    }
                    finally
                    {
                    }
                }
            }
        }


        //-------------------------------------------------------------------

        public string GetMyFieldValue(string fldname)
        {
            foreach (PanelRec pr in panelrecs)
            {
                if (pr.fldname.CompareTo(fldname) == 0)
                {
                    try
                    {
                        if (pr.tb != null)
                        {
                            return pr.tb.Text + GetTimeValueIfExist(pr);
                        }
                        else if (pr.ddl != null)
                        {
                            return pr.ddl.SelectedValue;
                        }
                        else if (pr.rbl != null)
                        {
                            return pr.rbl.SelectedValue;
                        }
                        else if (pr.chtb != null)
                        {
                            return pr.chtb.Text;
                        }
                        else if (pr.chComboBox != null)
                        {
                            return pr.chComboBox.SelectedValue;
                        }
                        else if (pr.chFlexBox != null)
                        {
                            return pr.chFlexBox.SelectedValue;
                        }
                    }
                    finally
                    {
                    }
                }
            }
            return "";
        }
        //

        private void FocusControlJS(Control control)
        {
            string clientID = control.ClientID;
            ScriptManager.RegisterStartupScript(control, control.GetType(), clientID + "_focusControlJS", "setTimeout(function() { var focusControl = document.getElementById('" + clientID + "'); focusControl && focusControl.focus(); }, 250);", true);
        }

        public void FocusFirstField()
        {
            foreach (PanelRec pr in panelrecs)
            {
                bool allowEnable = pr.AllowEdit;
                if (IsNewRecord)
                    allowEnable = pr.AllowAdd;

                if (!pr.hideField && allowEnable)
                {
                    if (pr.tb != null)
                    {
                        if (!pr.tb.ReadOnly && pr.tb.Enabled)
                        {
                            FocusControlJS(pr.tb);
                            //pr.tb.Focus();
                            break;
                        }
                    }
                    else if (pr.ddl != null)
                    {
                        if (pr.ddl.Enabled)
                        {
                            pr.ddl.Focus();
                            break;
                        }
                    }
	                else if (pr.rbl != null)
	                {
	                    if (pr.rbl.Enabled)
	                    {
	                        pr.rbl.Focus();
	                        break;
	                    }
	                }
                    else if (pr.cb != null)
                    {
                        if (pr.cb.Enabled)
                        {
                            pr.cb.Focus();
                            break;
                        }
                    }
                    else if (pr.chtb != null)
                    {
                        if (pr.chib.Enabled && !pr.chtb.ReadOnly && pr.chtb.Enabled)
                        {
                            pr.chtb.Focus();
                            break;
                        }
                    }
                    else if (pr.chComboBox != null)
                    {
                        if (pr.chComboBox.Enabled)
                        {
                            pr.chComboBox.Focus();
                            break;
                        }
                    }
                    else if (pr.chFlexBox != null)
                    {
                        if (pr.chFlexBox.Enabled && !pr.chFlexBox.ReadOnly)
                        {
                            pr.chFlexBox.Focus();
                            break;
                        }
                    }
                    else if (pr.chMultiLookup != null)
                    {
                        if (pr.chMultiLookup.IsEnabled())
                        {
                            pr.chMultiLookup.Focus();
                            break;
                        }
                    }
                }
            }
        }

        //-------------------------------------------------------------------

        public void FocusField(string fldname)
        {
            foreach (PanelRec pr in panelrecs)
            {
                if (pr.fldname.CompareTo(fldname) == 0)
                {
                    try
                    {
                        if (pr.tb != null)
                        {
                            pr.tb.Focus();
                            return;
                        }
                        else if (pr.ddl != null)
                        {
                            pr.ddl.Focus();
                            return;
                        }
                        else if (pr.rbl != null)
                        {
                            pr.rbl.Focus();
                            return;
                        }
                        else if (pr.cb != null)
                        {
                            pr.cb.Focus();
                            break;
                        }
                        else if (pr.chtb != null)
                        {
                            pr.chtb.Focus();
                            return;
                        }
                        else if (pr.chComboBox != null)
                        {
                            pr.chComboBox.Focus();
                            return;
                        }
                        else if (pr.chFlexBox != null)
                        {
                            pr.chFlexBox.Focus();
                            return;
                        }
                    }
                    finally
                    {
                    }
                }
            }
        }

        public bool CanFocusField(string fieldName)
        {
            bool canFocusField = false;
            foreach (PanelRec pr in panelrecs)
            {
                if (pr.fldname.CompareTo(fieldName) == 0)
                { 
                    bool allowEnable = pr.AllowEdit;
                    if (IsNewRecord)
                        allowEnable = pr.AllowAdd;

                    if (!pr.hideField && allowEnable)
                    {
                        if (pr.tb != null)
                        {
                            canFocusField = !pr.tb.ReadOnly && pr.tb.Enabled;
                            break;
                        }
                        else if (pr.ddl != null)
                        {
                            canFocusField = pr.ddl.Enabled;
                            break;
                        }
                        else if (pr.rbl != null)
                        {
                            canFocusField = pr.rbl.Enabled;
                            break;
                        }
                        else if (pr.cb != null)
                        {
                            canFocusField = pr.cb.Enabled;
                            break;
                        }
                        else if (pr.chtb != null)
                        {
                            canFocusField = pr.chib.Enabled && !pr.chtb.ReadOnly && pr.chtb.Enabled;
                            break;
                        }
                        else if (pr.chComboBox != null)
                        {
                            canFocusField = pr.chComboBox.Enabled;
                            break;
                        }
                        else if (pr.chFlexBox != null)
                        {
                            canFocusField = pr.chFlexBox.Enabled && !pr.chFlexBox.ReadOnly;
                            break;
                        }
                        else if (pr.chMultiLookup != null)
                        {
                            canFocusField = pr.chMultiLookup.IsEnabled();
                            break;
                        }
                    }
                }
            }
            return canFocusField;
        }

        // Return the client ID of the field after fldname.
        // Used in maintaining tab order position after PLCDBPanel autopostback.
        public string GetNextFieldClientID(string fldname)
        {
            PanelRec pr = null;
            for (int i = 0; i < panelrecs.Count; i++)
            {
                // Get current panel rec.
                if (panelrecs[i].fldname.CompareTo(fldname) == 0)
                {
                    // Get panel rec following it.
                    if (i < panelrecs.Count - 1)
                        pr = panelrecs[i + 1];

                    break;
                }
            }

            // Set focus.
            if (pr != null)
            {
                Control foundControl = null;
                if (pr.tb != null)
                {
                    foundControl = pr.tb;
                }
                else if (pr.ddl != null)
                {
                    foundControl = pr.ddl;
                }
                else if (pr.rbl != null)
                {
                    foundControl = pr.rbl;
                }
                else if (pr.chtb != null)
                {
                    foundControl = pr.chtb;
                }
                else if (pr.chComboBox != null)
                {
                    foundControl = pr.chComboBox;
                }
                else if (pr.chFlexBox != null)
                {
                    foundControl = pr.chFlexBox;
                    return foundControl.ClientID + "_input";
                }

                if (foundControl != null)
                    return foundControl.ClientID;
            }

            return "";
        }

        //-------------------------------------------------------------------

        public virtual void DoLoadRecord()
        {
            LoadRecord();

            BrowseMode();
        }

        //-------------------------------------------------------------------

        public string getpanelfieldAttribute(string fldname)
        {
            foreach (PanelRec pr in panelrecs)
            {
                if (pr.fldname == fldname)
                {
                    if (pr.chtb != null)
                        return pr.chtb.Text;
                    else if (pr.ddl != null)
                        return pr.ddl.SelectedValue;
                    else if (pr.rbl != null)
                        return pr.rbl.SelectedValue;
                    else if (pr.tb != null)
                        return pr.tb.Text + GetTimeValueIfExist(pr);
                    else if (pr.chComboBox != null)
                        return pr.chComboBox.SelectedValue;
                    else if (pr.chFlexBox != null)
                        return pr.chFlexBox.SelectedValue;
                }

            }

            return "";
        }


        //-------------------------------------------------------------------


        /// <summary>
        /// Get value from a DBPanel field using field name
        /// </summary>
        /// <param name="fldname"></param>
        /// <returns></returns>
        public string getpanelfield(string fldname)
        {
            foreach (PanelRec pr in panelrecs)
            {
                if (pr.fldname.ToUpper() == fldname.ToUpper())
                {
                    if (pr.chtb != null)
                        return StripMask(pr.chtb.Text);
                    if (pr.tb != null)
                        return StripMask(pr.tb.Text + GetTimeValueIfExist(pr));
                    if (pr.ddl != null)
                        return StripMask(pr.ddl.SelectedValue);
                    if (pr.rbl != null)
                        return StripMask(pr.rbl.SelectedValue);
                    if (pr.chComboBox != null)
                        return StripMask(pr.chComboBox.SelectedValue);
                    if (pr.chFlexBox != null)
                        return StripMask(pr.chFlexBox.SelectedValue);
                    if (pr.chMultiLookup != null)
                        return pr.chMultiLookup.GetText();
                    if (pr.cb != null)
                        return pr.cb.Checked ? "T" : "F";
                    if (pr.chMultipickAc != null)
                        return pr.chMultipickAc.GetSelectedCodes();
                }
            }
            return "";
        }

        /// <summary>
        /// Get value from a DBPanel field using table name and field name
        /// </summary>
        /// <param name="tblname"></param>
        /// <param name="fldname"></param>
        /// <returns></returns>
        public string getpanelfield(string tblname, string fldname)
        {
            foreach (PanelRec pr in panelrecs)
            {
                if ((pr.tblname.ToUpper() == tblname.ToUpper()) && (pr.fldname.ToUpper() == fldname.ToUpper()))
                {
                    if (pr.chtb != null)
                        return StripMask(pr.chtb.Text);
                    if (pr.tb != null)
                        return StripMask(pr.tb.Text + GetTimeValueIfExist(pr));
                    if (pr.ddl != null)
                        return StripMask(pr.ddl.SelectedValue);
                    if (pr.rbl != null)
                        return StripMask(pr.rbl.SelectedValue);
                    if (pr.chComboBox != null)
                        return StripMask(pr.chComboBox.SelectedValue);
                    if (pr.chFlexBox != null)
                        return StripMask(pr.chFlexBox.SelectedValue);
                    if (pr.chMultiLookup != null)
                        return pr.chMultiLookup.GetText();
                    if (pr.chMultipickAc != null)
                        return pr.chMultipickAc.GetSelectedCodes();
                }
            }
            return "";
        }


        /// <summary>
        /// Get value from a DBPanel field using table name and field name and prompt/label
        /// Useful when you have 2 DBPanel field with same field name
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="fieldName"></param>
        /// <param name="prompt"></param>
        /// <returns></returns>
        public string getpanelfield(string tableName, string fieldName, string prompt)
        {
            foreach (PanelRec pr in panelrecs)
            {
                if ((pr.tblname.ToUpper() == tableName.ToUpper()) && 
                    (pr.fldname.ToUpper() == fieldName.ToUpper() &&
                    pr.prompt.ToUpper().Trim() == prompt.ToUpper().Trim()))
                {
                    if (pr.chtb != null)
                        return StripMask(pr.chtb.Text);
                    if (pr.tb != null)
                        return StripMask(pr.tb.Text + GetTimeValueIfExist(pr));
                    if (pr.ddl != null)
                        return StripMask(pr.ddl.SelectedValue);
                    if (pr.rbl != null)
                        return StripMask(pr.rbl.SelectedValue);
                    if (pr.chComboBox != null)
                        return StripMask(pr.chComboBox.SelectedValue);
                    if (pr.chFlexBox != null)
                        return StripMask(pr.chFlexBox.SelectedValue);
                    if (pr.chMultiLookup != null)
                        return pr.chMultiLookup.GetText();
                }
            }
            return "";
        }

        public bool IsPostbackPanelField(string fieldname)
        {
            bool retIsPostback = false;

            foreach (PanelRec rec in this.panelrecs)
            {
                if (rec.fldname.ToUpper() == fieldname.ToUpper())
                {
                    retIsPostback = (rec.PostBack == "T") ? true : false;
                    break;
                }
            }

            return retIsPostback;
        }

        public void SetPanelFieldError(string fieldname, string errormsg)
        {
            foreach (PanelRec rec in this.panelrecs)
            {
                if (rec.fldname.ToUpper() == fieldname.ToUpper())
                {
                    rec.ErrMsg.Text = errormsg;
                    rec.ErrMsg.Visible = true;
                    rec.ErrMsg.ForeColor = Color.Red;
                }
            }
        }


        public void SetPanelFieldStatusMessage(string fieldname, string statusMsg)
        {
            foreach (PanelRec rec in this.panelrecs)
            {
                if (rec.fldname.ToUpper() == fieldname.ToUpper())
                {
                    
                    if (string.IsNullOrEmpty(statusMsg))
                    {
                      rec.StatusMsg.Text = "";
                      rec.StatusMsg.Visible = false;
                    }
                    else
                    {
                      rec.StatusMsg.Text = statusMsg;
                      rec.StatusMsg.Visible = true;
                    }
                }
            }
        }

        public FlexBox GetFlexBoxControl(string fieldName)
        {

            String tblName = "";
            //added to support two fields with the same name from differet tables in the dbpanel.
            if (fieldName.Contains("."))
            {
                String[] parts = fieldName.Split('.');
                if (parts.Count() == 2)
                {
                    tblName = parts[0];
                    fieldName = parts[1];
                }

            }

            foreach (PanelRec pr in panelrecs)
            {

                if (pr.fldname.ToUpper() == fieldName.ToUpper() && ((String.IsNullOrWhiteSpace(tblName)) || (tblName == pr.tblname)))
                {
                    if (pr.chFlexBox != null)
                        return pr.chFlexBox;
                }
            }

            return null;
        }

        /// <summary>
        /// Get control based on field name
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public Control GetControl(string fieldName)
        {

            String tblName = "";
            //added to support two fields with the same name from differet tables in the dbpanel.
            if (fieldName.Contains("."))
            {
                String[] parts = fieldName.Split('.');
                if (parts.Count() == 2)
                {
                    tblName = parts[0];
                    fieldName = parts[1];
                }

            }

            foreach (PanelRec pr in panelrecs)
            {

                if (pr.fldname.ToUpper() == fieldName.ToUpper() && ((String.IsNullOrWhiteSpace(tblName)) || (tblName == pr.tblname)))
                {
                    if (pr.chFlexBox != null)
                        return pr.chFlexBox;
                    else if (pr.tb != null)
                        return pr.tb;
                }
            }

            return null;
        }
        

        public DropDownList GetDropDownListControl(string fieldName)
        {
            foreach (PanelRec pr in panelrecs)
            {
                if (pr.fldname.ToUpper() == fieldName.ToUpper())
                {
                    if (pr.ddl != null)
                        return pr.ddl;
                }
            }

            return null;
        }

        public RadioButtonList GetRadioButtonListControl(string fieldName)
        {
            foreach (PanelRec pr in panelrecs)
            {
                if (pr.fldname.ToUpper() == fieldName.ToUpper())
                {
                    if (pr.rbl != null)
                        return pr.rbl;
                }
            }

            return null;
        }

        public void setpanelfield(string fldname, string tempval)
        {
            // setpanelfield by default considers UsesKey flag.
            setpanelfield(fldname, tempval, true);
        }

        // Set isConsiderUsesKey to false to always use code and not consider the UsesKey setting.
        public void setpanelfield(string fldname, string tempval, bool isConsiderUsesKey)
        {
            foreach (PanelRec pr in panelrecs)
            {
                if (pr.fldname.ToUpper() == fldname.ToUpper())
                {
                    if (pr.chtb != null)
                        CodeHeadSetValue(pr, tempval);

                    if (pr.tb != null)
                    {
                        if (pr.time != null)
                        {
                            var dateTime = ConvertToDateTimeFromDB(tempval);
                            pr.time.Text = dateTime.ToString("HH:mm");
                        }

                        if ((pr.fldtype == "T") && (!string.IsNullOrEmpty(tempval)))
                        {
                            if (pr.editmask.ToUpper() == "HH:MM:SS")
                                tempval = Convert.ToDateTime(tempval).ToString("HH:mm:ss");
                            else
                                tempval = Convert.ToDateTime(tempval).ToString("HH:mm");
                        }
                        else if ((pr.fldtype == "D") && (!string.IsNullOrEmpty(tempval)))
                        {
                            tempval = ConvertToDateTimeFromDB(tempval).ToString(pr.editmask.Replace("D", "d").Replace("m", "M").Replace("Y", "y"));
                        }

                        pr.tb.Text = tempval;
                    }

                    if (pr.ddl != null)
                    {
                        if (pr.ddl.Items.FindByValue(tempval) != null)
                            pr.ddl.SelectedValue = tempval;
                    }

                    if (pr.rbl != null)
                    {
                        if (pr.rbl.Items.FindByValue(tempval) != null)
                            pr.rbl.SelectedValue = tempval;
                    }

                    if (pr.chComboBox != null)
                    {
                        if (pr.chComboBox.Items.FindByValue(tempval) != null)
                            pr.chComboBox.SelectedValue = tempval;
                    }

                    if (pr.chFlexBox != null)
                    {
                        pr.chFlexBox.SelectedValue = tempval;

                        if (pr.editmask == "FREEFORMLIST")
                        {
                            pr.chFlexBox.SelectedText = tempval;
                        }
                    }

                    if (pr.cb != null)
                        pr.cb.Checked = tempval == "T";

                    if (pr.chMultiLookup != null)
                        pr.chMultiLookup.SetText(tempval);

                    if (pr.chMultipickAc != null)
                        pr.chMultipickAc.SetText(tempval);

                    return;
                }
            }
        }

        // For controls with the same fieldname (Date From, Date To, Date Start, Date End,...)
        public void setpanelfield(string fldname, string prompt, string tempval)
        {
            bool isConsiderUsesKey = true;

            foreach (PanelRec pr in panelrecs)
            {
                if (pr.fldname.ToUpper() == fldname.ToUpper() && pr.prompt.ToUpper() == prompt.ToUpper())
                {
                    if (pr.chtb != null)
                        CodeHeadSetValue(pr, tempval);

                    if (pr.tb != null)
                    {
                        if (pr.time != null)
                        {
                            var dateTime = ConvertToDateTimeFromDB(tempval);
                            pr.time.Text = dateTime.ToString("HH:mm");
                        }

                        if (pr.fldtype == "T")
                        {
                            if (pr.editmask.ToUpper() == "HH:MM:SS")
                                tempval = Convert.ToDateTime(tempval).ToString("HH:mm:ss");
                            else
                                tempval = Convert.ToDateTime(tempval).ToString("HH:mm");
                        }

                        pr.tb.Text = tempval;
                    }

                    if (pr.ddl != null)
                    {
                        if (pr.ddl.Items.FindByValue(tempval) != null)
                            pr.ddl.SelectedValue = tempval;
                    }

                    if (pr.rbl != null)
                    {
                        if (pr.rbl.Items.FindByValue(tempval) != null)
                            pr.rbl.SelectedValue = tempval;
                    }

                    if (pr.chComboBox != null)
                    {
                        if (pr.chComboBox.Items.FindByValue(tempval) != null)
                            pr.chComboBox.SelectedValue = tempval;
                    }

                    if (pr.chFlexBox != null)
                    {
                        pr.chFlexBox.SelectedValue = tempval;

                        if (pr.editmask == "FREEFORMLIST")
                        {
                            pr.chFlexBox.SelectedText = tempval;
                        }
                    }

                    if (pr.chMultiLookup != null)
                    {
                        pr.chMultiLookup.SetText(tempval);
                    }

                    if (pr.chMultipickAc != null)
                    {
                        pr.chMultipickAc.SetText(tempval);
                    }

                    return;
                }
            }

        }

        public void SetPanelCodeCondition(string fieldName, string codeCondition)
        {
            PanelRec pr = panelrecs.FirstOrDefault(a => a.fldname.ToUpper() == fieldName.ToUpper());
            if (pr != null)
            {
                pr.codecondition = codeCondition;
                if (pr.chFlexBox != null)
                    pr.chFlexBox.CodeCondition = codeCondition;
                if (pr.chMultiLookup != null)
                    //WhereClause is the Multipick public var for CODE_CONDITION
                    pr.chMultiLookup.WhereClause = codeCondition;
            }
        }


        public void SetPanelCodeTable(string fieldName, string codeTable)
        {
            PanelRec pr = panelrecs.FirstOrDefault(a => a.fldname.ToUpper() == fieldName.ToUpper());
            if (pr != null)
            {
                pr.codetable = codeTable;
                if (pr.chFlexBox != null)
                    pr.chFlexBox.TableName = codeTable;
            }
        }
        //-------------------------------------------------------------------

        public DropDownList GetPanelDDL(string fldname)
        {
            foreach (PanelRec pr in panelrecs)
            {
                if (pr.fldname.ToUpper() == fldname.ToUpper())
                {
                    if (pr.ddl != null)
                        return pr.ddl;

                    break;
                }
            }

            return null;
        }

        public void DoSaveRecord(PLCQuery qry)
        {
            SaveRecord(qry);
            LoadCodeDescriptions();
            BrowseMode();
        }

        //-------------------------------------------------------------------

        private string generateupdatesql()
        {
            string updatestr = "";

            foreach (PanelRec pr in panelrecs)
            {
                if ((pr.tb != null))
                {
                    if (updatestr != "")
                        updatestr += ", ";

                    if (pr.fldtype == "D")
                    {

                        if (cleanit(pr.tb.Text) == "")
                            updatestr += "\"" + pr.fldname + "\" = NULL";
                        else
                            updatestr += "\"" + pr.fldname + "\" = to_date('" + cleanit(pr.tb.Text) + "','mm/dd/yyyy')";
                    }
                    else if (pr.fldtype == "T")
                    {

                        if (cleanit(pr.tb.Text) == "")
                            updatestr += "\"" + pr.fldname + "\" = NULL";
                        else
                            updatestr += "\"" + pr.fldname + "\" = to_date('" + "19000101" + cleanit(pr.tb.Text) + "','YYYYMMDD:HH24:MI')";
                    }
                    else
                    {
                        updatestr += "\"" + pr.fldname + "\" = '" + cleanit(pr.tb.Text) + "'";
                    }
                }
                else if (pr.chMultiLookup != null)
                {
                    updatestr += "\"" + pr.fldname + "\" = '" + cleanit(pr.chMultiLookup.GetText()) + "'";
                }

                if ((pr._codetable != "") && (pr._codetable != null))
                {
                    if (updatestr != "")
                        updatestr += ", ";
                    if (pr.chtb != null)
                        updatestr += "\"" + pr.fldname + "\" = '" + pr.chtb.Text + "'";
                    else if (pr.chComboBox != null)
                        updatestr += "\"" + pr.fldname + "\" = '" + pr.chComboBox.SelectedValue + "'";
                    else if (pr.chFlexBox != null)
                        updatestr += "\"" + pr.fldname + "\" = '" + pr.chFlexBox.SelectedValue + "'";
                }
            }

            return "update " + PLCDataTable + " set " + updatestr + " " + PLCWhereClause;
        }


        //-------------------------------------------------------------------



        protected virtual void SaveRecord(PLCQuery qry)
        {
            PLCQuery qryUpdate;

            if (qry == null)
            {
                qryUpdate = new PLCQuery();
                qryUpdate.SQL = "SELECT * FROM " + PLCDataTable + " " + PLCWhereClause;
                qryUpdate.Open();
                if (qryUpdate.HasData())
                {

                    qryUpdate.Edit();
                }
            }
            else
                qryUpdate = qry;

            if (PLCSession.PLCDatabaseServer == "ORACLE")
                qryUpdate.UseQuotedFieldNames = true;

            string fldName;

            foreach (PanelRec pr in panelrecs)
            {
                fldName = pr.fldname.Replace("\"", "");

                if (pr.tb != null && !string.IsNullOrEmpty(pr.tb.Text) && pr.isDateMask(pr.editmask))
                {
                    qryUpdate.SetFieldValue(pr.fldname, ConvertToDateTime(pr.tb.Text + GetTimeValueIfExist(pr), pr.editmask));
                }
                else if (pr.tb != null)
                {
                    if (string.IsNullOrEmpty(pr.DisplayFormula) && pr.tb.Attributes["DISPLAYFORMULAFIELD"] == null)
                    {
                        qryUpdate.SetFieldValue(fldName, pr.tb.Text);
                    }
                }
                if (pr.ddl != null)
                {
                    qryUpdate.SetFieldValue(fldName, pr.ddl.SelectedValue);
                }
                if (pr.rbl != null)
                {
                    qryUpdate.SetFieldValue(fldName, pr.rbl.SelectedValue);
                }
                if (pr.chtb != null)
                {
                    qryUpdate.SetFieldValue(fldName, pr.chtb.Text);
                }
                if (pr.chComboBox != null)
                {
                    qryUpdate.SetFieldValue(fldName, pr.chComboBox.SelectedValue);
                }
                if (pr.chFlexBox != null)
                {
                    qryUpdate.SetFieldValue(fldName, pr.chFlexBox.SelectedValue);
                }

                if (pr.editmask == "CB" || pr.editmask == "CB-R")
                {
                    if (pr.cb != null)
                    {
                        if (pr.cb.Checked)
                            qryUpdate.SetFieldValue(fldName, "T");
                        else
                            qryUpdate.SetFieldValue(fldName, "F");
                    }
                }

                if (pr.chMultiLookup != null)
                {
                    qryUpdate.SetFieldValue(fldName, pr.chMultiLookup.GetSelectedSectionsCsv());
                }

                if (pr.sig != null)
                {
                    qryUpdate.SetFieldValue(fldName, pr.sig.Key);
            }
            }
			qryUpdate.Post(PLCDataTable, PLCAuditCode, PLCAuditSubCode, ReasonChangeKey, PLCConfigPanel);
        }

        //$$ Need to match PLCControlPanel.SetBrowseMode() signature and use override instead?
        public new void SetBrowseMode()
        {
            BrowseMode();
        }

        //$$ Need to match PLCControlPanel.SetEditMode() signature and use override instead?
        public new void SetEditMode()
        {
            EditMode();
            SetRestrictionsBasedOnAddEditBase(IsNewRecord ? "ADD" : "EDIT");
        }

        public bool IsRecordUpToDate(out List<string> changes)
        {
            changes = new List<string>();

            const string CACHED_DATA_CHECK = "DBPANEL_CACHED_DATA_CHECK";
            var appSettings = ConfigurationManager.AppSettings;
            if (!appSettings.AllKeys.Contains(CACHED_DATA_CHECK)
                || !appSettings[CACHED_DATA_CHECK].ToUpper().Equals("T"))
            {
                return true;
            }

            if (IsNewRecord)
                return true;

            string sql = generatesql();
            if (string.IsNullOrEmpty(sql))
                return true;

            var qry = new PLCQuery(sql);
            qry.Open();
            if (qry.IsEmpty())
                return true;

            string fieldName;
            bool isUpToDate = true;
            foreach (PanelRec pr in panelrecs)
            {
                fieldName = pr.fldname.Replace("\"", "");
                string current = qry.FieldByName(fieldName);
                string original = pr.original.Value;
                string originalMasked = GetOriginalValueMasked(fieldName);
                string newValue = getpanelfield(fieldName);

                bool isChanged = original != current;

                DateTime? dateCurrent = null;
                DateTime? dateOriginal = null;
                if (pr.tb != null && pr.isDateMask(pr.editmask))
                {
                    string format = IsDMYFormat(pr.editmask)
                        ? "d/M/yyyy HH:mm:ss"
                        : "M/d/yyyy HH:mm:ss";

                    if (!string.IsNullOrEmpty(current))
                    {
                        dateCurrent = ConvertToDateTimeFromDB(current);
                        current = dateCurrent.Value.ToString(format);
                    }

                    if (!string.IsNullOrEmpty(original))
                    {
                        dateOriginal = ConvertToDateTime(original, pr.editmask);
                        original = dateOriginal.Value.ToString(format);
                    }
                }
                else if (pr.tb != null && pr.isNumberMask(pr.editmask))
                {
                    if (!string.IsNullOrEmpty(current))
                    {
                        double numValue;
                        if (double.TryParse(current, out numValue))
                            current = numValue.ToString();
                    }

                    if (!string.IsNullOrEmpty(original))
                    {
                        double numValue;
                        if (double.TryParse(original, out numValue))
                            original = numValue.ToString();
                    }
                }

                if (dateCurrent != null && dateOriginal != null)
                {
                    isChanged = dateOriginal != dateCurrent;
                }
                else
                    isChanged = original != current;

                if (isChanged || originalMasked != newValue)
                {
                    string change = "\r\n" + PLCDataTable + "." + fieldName
                        + "\r\n  Old Value => " + original
                        + (isChanged ? "\r\n  Modified Value in DB => " + current : "")
                        + "\r\n  New Value => " + newValue;
                    changes.Add(change);

                    if (isChanged)
                        isUpToDate = false;
                }
            }

            return isUpToDate;
        }

        override public Boolean CanSave()
        {
            //*AAC* 060509 Check if at least one field has a value.
            bool hasEntries = false;
          
            if (panelrecs.Count < 1)
                return true;

            List<string> changes;
            if (!string.IsNullOrEmpty(PLCDataTable) && !IsNewRecord
                && !IsRecordUpToDate(out changes))
            {
                string dataChanges = string.Join("\r\n", changes);
                string auditLog = "The following changes were aborted because "
                    + "the DBPanel data changed:" + dataChanges;
                PLCSession.WriteAuditLog("0", "0", "1", auditLog);

                string message = "<div style='max-height:400px;'>"
                    + "Another user just saved changes to this data. "
                    + "Please refresh and try again." + dataChanges.Replace("\r\n", "<br/>")
                    + "</div>";
                ShowAlert(message);

                return false;
            }

            DateTime TestDate;
            Boolean RecordOK = true;
            PLCDBPanelValidateEventArgs TheArgs = new PLCDBPanelValidateEventArgs(false, "", "", false, "", "", "", "");

            string qryUniqueConstraint = "";
            string errUniqueConstraint = "";

            foreach (PanelRec pr in panelrecs)
            {            
                pr.ErrMsg.Text = "";
                pr.ErrMsg.Visible = false;
                 
                if (pr.chtb != null)
                    pr.chtb.Text = StripMask(pr.chtb.Text.Trim());
                if (pr.tb != null)
                {
                    //StripMask currently only removes underscores at the end of the string, which doesnt work on Date
                    if (pr.fldtype == "D" && pr.tb.Text.Trim() == "__/__/____")
                        pr.tb.Text = "";
                    else
                        pr.tb.Text = StripMask(pr.tb.Text.Trim());
                }

                if (pr.chtb != null)
                {
                    //Here is the user validation event for the codefield
                    try
                    {
                        //Check the user Validation routine....
                        TheArgs.handled = false;
                        TheArgs.fldName = pr.fldname;
                        TheArgs.fldValue = pr.chtb.Text;
                        TheArgs.errMsg = "";
                        TheArgs.isValid = true;
                        TheArgs.initVal = "";
                        TheArgs.custom1 = "";
                        TheArgs.custom2 = "";


                        if (pr.chtb.Attributes["INITVAL"] != null)
                            TheArgs.initVal = (string)pr.chtb.Attributes["INITVAL"];

                        if (pr.chtb.Attributes["CUSTOM1"] != null)
                            TheArgs.custom1 = (string)pr.chtb.Attributes["CUSTOM1"];

                        if (pr.chtb.Attributes["CUSTOM2"] != null)
                            TheArgs.custom2 = (string)pr.chtb.Attributes["CUSTOM2"];

                        onplcdbpanelvalidate(TheArgs);


                        if ((TheArgs.errMsg != "") && (TheArgs.isValid == false))
                        {
                            pr.ErrMsg.Text = TheArgs.errMsg;
                            pr.ErrMsg.Visible = true;
                            RecordOK = false;
                        }

                        if (TheArgs.handled)
                            continue;

                    }
                    catch (Exception e)
                    {
                        pr.ErrMsg.Text = "Exception in " + pr.prompt + " validation:" + e.Message;
                        pr.ErrMsg.Visible = true;
                        RecordOK = false;
                    };


                    // End of user validation event for the codefield


                    if ((pr.chtb.Text.Trim() == "") && (pr.required == "T"))
                    {
                        pr.ErrMsg.Text = pr.GetRequiredMessage();
                        pr.ErrMsg.Visible = true;
                        RecordOK = false;
                    }

                    if ((pr.chtb.Text != "") && (pr.codetable != ""))
                    {
                        if ((pr.attribtype == "") && (!CodeValid(pr.codetable, pr.chtb.Text, pr.codecondition)))
                        {                                                 
                            //pr.codelbl.Text = "";
                            pr.ErrMsg.Text = "  [" + HttpUtility.HtmlEncode(pr.chtb.Text) + "] is not a valid code for " + pr.prompt;
                            //pr.codelbl.Text = "";
                            pr.ErrMsg.Visible = true;
                            RecordOK = false;                                                 
                        }
                        else if (pr.attribtype == "P" && !IsValidTableValue(pr.codetable, "VALUE", pr.chtb.Text, pr.codecondition))
                        {
                            pr.ErrMsg.Text = "  [" + HttpUtility.HtmlEncode(pr.chtb.Text) + "] is not a valid value for " + pr.prompt;
                            pr.ErrMsg.Visible = true;
                            RecordOK = false;
                        }
                    }


                    if (pr.attribtype == "P")
                    {
                        if ((pr.chtb.Text.Trim() == "") && (pr.required == "T"))
                        {
                            pr.ErrMsg.Text = pr.GetRequiredMessage();
                            pr.ErrMsg.Visible = true;
                            RecordOK = false;
                        }

                        if (!ValidAttribute(pr.fldname, pr.chtb.Text))
                        {
                            pr.ErrMsg.Text = "  [" + HttpUtility.HtmlEncode(pr.chtb.Text) + "] is not a valid code for " + pr.prompt;
                            pr.ErrMsg.Visible = true;
                            RecordOK = false;
                        }
                    }                    
                    else if (pr.attribtype == "M")
                    {
                        if ((pr.chtb.Text.Trim() == "") && (pr.required == "T"))
                        {
                            pr.ErrMsg.Text = pr.GetRequiredMessage();
                            pr.ErrMsg.Visible = true;
                            RecordOK = false;
                        }
                        string TempValue = pr.chtb.Text;
                        string FieldValue = "";
                        int P = TempValue.IndexOf(',');
                        while (P >= 0)
                        {
                            FieldValue = TempValue.Substring(0, P);
                            TempValue = TempValue.Remove(0, P + 1);
                            P = TempValue.IndexOf(',');

                            if (!ValidAttribute(pr.fldname, FieldValue))
                            {
                                pr.ErrMsg.Text = "  [" + HttpUtility.HtmlEncode(FieldValue) + "] is not a valid code for " + pr.prompt;
                                pr.ErrMsg.Visible = true;
                                RecordOK = false;
                            }
                        }
                        FieldValue = TempValue;
                        if (!ValidAttribute(pr.fldname, FieldValue))
                        {
                            pr.ErrMsg.Text = "  [" + HttpUtility.HtmlEncode(FieldValue) + "] is not a valid code for " + pr.prompt;
                            pr.ErrMsg.Visible = true;
                            RecordOK = false;
                        }

                    }
                }

                if (pr.chComboBox != null)
                {
                    //Here is the user validation event for the codefield
                    try
                    {
                        //Check the user Validation routine....
                        TheArgs.handled = false;
                        TheArgs.fldName = pr.fldname;
                        TheArgs.fldValue = pr.chComboBox.SelectedValue;
                        TheArgs.errMsg = "";
                        TheArgs.isValid = true;
                        TheArgs.initVal = "";
                        TheArgs.custom1 = "";
                        TheArgs.custom2 = "";


                        if (pr.chComboBox.Attributes["INITVAL"] != null)
                            TheArgs.initVal = (string)pr.chComboBox.Attributes["INITVAL"];

                        if (pr.chComboBox.Attributes["CUSTOM1"] != null)
                            TheArgs.custom1 = (string)pr.chComboBox.Attributes["CUSTOM1"];

                        if (pr.chComboBox.Attributes["CUSTOM2"] != null)
                            TheArgs.custom2 = (string)pr.chComboBox.Attributes["CUSTOM2"];

                        onplcdbpanelvalidate(TheArgs);


                        if ((TheArgs.errMsg != "") && (TheArgs.isValid == false))
                        {
                            pr.ErrMsg.Text = TheArgs.errMsg;
                            pr.ErrMsg.Visible = true;
                            RecordOK = false;
                        }

                        if (TheArgs.handled)
                            continue;

                    }
                    catch (Exception e)
                    {
                        pr.ErrMsg.Text = "Exception in " + pr.prompt + " validation:" + e.Message;
                        pr.ErrMsg.Visible = true;
                        RecordOK = false;
                    };


                    // End of user validation event for the codefield


                    if ((pr.chComboBox.SelectedValue.Trim() == "") && (pr.required == "T"))
                    {
                        pr.ErrMsg.Text = pr.GetRequiredMessage();
                        pr.ErrMsg.Visible = true;
                        RecordOK = false;
                    }

                    if ((pr.chComboBox.SelectedValue.Trim() != "") && (pr.codetable != ""))
                    {
                        if ((pr.attribtype == "") && (!CodeValid(pr.codetable, pr.chComboBox.SelectedValue, pr.codecondition)))
                        {
                            if (pr.chComboBox.SelectedValue != GetOriginalValue(pr.fldname))
                            {                               
                                //pr.codelbl.Text = "";
                                pr.ErrMsg.Text = "  [" + pr.chComboBox.SelectedValue + "] is not a valid code for " + pr.prompt;
                                //pr.codelbl.Text = "";
                                pr.ErrMsg.Visible = true;
                                RecordOK = false;                              
                            }
                        }
                        else if (pr.attribtype == "P" && !IsValidTableValue(pr.codetable, "VALUE", pr.chComboBox.SelectedValue, pr.codecondition))
                        {
                            pr.ErrMsg.Text = "  [" + pr.chComboBox.SelectedValue + "] is not a valid value for " + pr.prompt;
                            pr.ErrMsg.Visible = true;
                            RecordOK = false;
                        }
                    }
             
                    if (pr.attribtype == "P")
                    {
                        if ((pr.chComboBox.SelectedValue.Trim() == "") && (pr.required == "T"))
                        {
                            pr.ErrMsg.Text = pr.GetRequiredMessage();
                            pr.ErrMsg.Visible = true;
                            RecordOK = false;
                        }

                        if (!ValidAttribute(pr.fldname, pr.chComboBox.SelectedValue))
                        {
                            pr.ErrMsg.Text = "  [" + pr.chComboBox.SelectedValue + "] is not a valid code for " + pr.prompt;
                            pr.ErrMsg.Visible = true;
                            RecordOK = false;
                        }
                    }
                    else if (pr.attribtype == "M")
                    {
                        if ((pr.chComboBox.SelectedValue.Trim() == "") && (pr.required == "T"))
                        {
                            pr.ErrMsg.Text = pr.GetRequiredMessage();
                            pr.ErrMsg.Visible = true;
                            RecordOK = false;
                        }
                        string TempValue = pr.chComboBox.SelectedValue;
                        string FieldValue = "";
                        int P = TempValue.IndexOf(',');
                        while (P >= 0)
                        {
                            FieldValue = TempValue.Substring(0, P);
                            TempValue = TempValue.Remove(0, P + 1);
                            P = TempValue.IndexOf(',');

                            if (!ValidAttribute(pr.fldname, FieldValue))
                            {
                                pr.ErrMsg.Text = "  [" + FieldValue + "] is not a valid code for " + pr.prompt;
                                pr.ErrMsg.Visible = true;
                                RecordOK = false;
                            }
                        }
                        FieldValue = TempValue;
                        if (!ValidAttribute(pr.fldname, FieldValue))
                        {
                            pr.ErrMsg.Text = "  [" + FieldValue + "] is not a valid code for " + pr.prompt;
                            pr.ErrMsg.Visible = true;
                            RecordOK = false;
                        }

                    }

                }

                if (pr.chFlexBox != null)
                {
                    bool validateOnSave = (pr.validateOnSave && pr.chFlexBox.ParentFlexBox != null);
                    //Here is the user validation event for the codefield
                    try
                    {
                        //Check the user Validation routine....
                        TheArgs.handled = false;
                        TheArgs.fldName = pr.fldname;
                        TheArgs.fldValue = (pr.chFlexBox.ComboBox) ? pr.chFlexBox.SelectedText : pr.chFlexBox.SelectedValue;
                        TheArgs.errMsg = "";
                        TheArgs.isValid = true;
                        TheArgs.initVal = "";
                        TheArgs.custom1 = "";
                        TheArgs.custom2 = "";


                        if (pr.chFlexBox.Attributes["INITVAL"] != null)
                            TheArgs.initVal = (string)pr.chFlexBox.Attributes["INITVAL"];

                        if (pr.chFlexBox.Attributes["CUSTOM1"] != null)
                            TheArgs.custom1 = (string)pr.chFlexBox.Attributes["CUSTOM1"];

                        if (pr.chFlexBox.Attributes["CUSTOM2"] != null)
                            TheArgs.custom2 = (string)pr.chFlexBox.Attributes["CUSTOM2"];

                        onplcdbpanelvalidate(TheArgs);


                        if ((TheArgs.errMsg != "") && (TheArgs.isValid == false))
                        {
                            pr.ErrMsg.Text = TheArgs.errMsg;
                            pr.ErrMsg.Visible = true;
                            RecordOK = false;
                        }

                        if (TheArgs.handled)
                            continue;

                    }
                    catch (Exception e)
                    {
                        pr.ErrMsg.Text = "Exception in " + pr.prompt + " validation:" + e.Message;
                        pr.ErrMsg.Visible = true;
                        RecordOK = false;
                    };


                    // End of user validation event for the codefield

                    if ((pr.chFlexBox.SelectedValue.Trim() != "") && (pr.codetable != ""))
                    {

                        try
                        {
                            string codeCondition =  validateOnSave ? (!string.IsNullOrEmpty(pr.supplementLink.Trim()) ? pr.chFlexBox.CodeCondition : pr.codecondition) : pr.codecondition;

                            if ((pr.attribtype == "") && (!CodeValid(pr.codetable, pr.chFlexBox.SelectedValue, codeCondition)))
                            {
                                if (validateOnSave)
                                {
                                    pr.chFlexBox.SelectedValue = "";

                                }
                                else
                                {
                                    // Do not invalidate disabled analyst if selected analyst is not modified
                                    if (pr.codetable != "TV_ANALYST" || pr.chFlexBox.SelectedValue != GetOriginalValue(pr.fldname))
                                    {
                                        string code = pr.chFlexBox.SelectedValue;
                                        // check if code contains a comma, if it does then validate the code again using the whole value
                                        if (!code.Contains(",") || !IsCodeValid(pr.codetable, code, pr.codecondition))
                                        {
                                            pr.ErrMsg.Text = "  [" + pr.chFlexBox.SelectedText + "] is not a valid value for " + pr.prompt;
                                            pr.ErrMsg.Visible = true;
                                            RecordOK = false;
                                        }
                                    }
                                }
                            }
                            else if (pr.attribtype == "P" && !IsValidTableValue(pr.codetable, "VALUE", pr.chFlexBox.SelectedValue, pr.codecondition))
                            {
                                pr.ErrMsg.Text = "  [" + pr.chFlexBox.SelectedText + "] is not a valid value for " + pr.prompt;
                                pr.ErrMsg.Visible = true;
                                RecordOK = false;
                            }
                        }
                        catch (Exception e)
                        {
                            pr.ErrMsg.Text = "Exception in " + pr.prompt + " validation:" + e.Message;
                            pr.ErrMsg.Visible = true;
                            RecordOK = false;
                        };
                    }


                    if ((pr.chFlexBox.SelectedValue.Trim() == "") && (pr.required == "T"))
                    {
                        pr.ErrMsg.Text = pr.GetRequiredMessage();
                        pr.ErrMsg.Visible = true;
                        RecordOK = false;
                    }

                    if (pr.attribtype == "P")
                    {
                        if ((pr.chFlexBox.SelectedValue.Trim() == "") && (pr.required == "T"))
                        {
                            pr.ErrMsg.Text = pr.GetRequiredMessage();
                            pr.ErrMsg.Visible = true;
                            RecordOK = false;
                        }

                        if (!ValidAttribute(pr.fldname, pr.chFlexBox.SelectedValue))
                        {
                            pr.ErrMsg.Text = "  [" + pr.chFlexBox.SelectedText + "] is not a valid value for " + pr.prompt;
                            pr.ErrMsg.Visible = true;
                            RecordOK = false;
                        }
                    }
                    else if (pr.attribtype == "M")
                    {
                        if ((pr.chFlexBox.SelectedValue.Trim() == "") && (pr.required == "T"))
                        {
                            pr.ErrMsg.Text = pr.GetRequiredMessage();
                            pr.ErrMsg.Visible = true;
                            RecordOK = false;
                        }
                        string TempValue = pr.chFlexBox.SelectedValue;
                        string FieldValue = "";
                        int P = TempValue.IndexOf(',');
                        while (P >= 0)
                        {
                            FieldValue = TempValue.Substring(0, P);
                            TempValue = TempValue.Remove(0, P + 1);
                            P = TempValue.IndexOf(',');

                            if (!ValidAttribute(pr.fldname, FieldValue))
                            {
                                pr.ErrMsg.Text = "  [" + FieldValue + "] is not a valid code for " + pr.prompt;
                                pr.ErrMsg.Visible = true;
                                RecordOK = false;
                            }
                        }
                        FieldValue = TempValue;
                        if (!ValidAttribute(pr.fldname, FieldValue))
                        {
                            pr.ErrMsg.Text = "  [" + FieldValue + "] is not a valid code for " + pr.prompt;
                            pr.ErrMsg.Visible = true;
                            RecordOK = false;
                        }

                    }

                }


                if (pr.tb != null)
                {
                    try
                    {
                        //Check the user Validation routine....
                        TheArgs.handled = false;
                        TheArgs.fldName = pr.fldname;
                        TheArgs.fldValue = pr.tb.Text + GetTimeValueIfExist(pr);
                        TheArgs.errMsg = "";
                        TheArgs.isValid = true;
                        TheArgs.initVal = "";
                        TheArgs.custom1 = "";
                        TheArgs.custom2 = "";

                        if (pr.tb.Attributes["INITVAL"] != null)
                            TheArgs.initVal = (string)pr.tb.Attributes["INITVAL"];

                        if (pr.tb.Attributes["CUSTOM1"] != null)
                            TheArgs.custom1 = (string)pr.tb.Attributes["CUSTOM1"];

                        if (pr.tb.Attributes["CUSTOM2"] != null)
                            TheArgs.custom2 = (string)pr.tb.Attributes["CUSTOM2"];

                        onplcdbpanelvalidate(TheArgs);

                        if (pr.PostBack == "T")
                        {
                            PLCDBPanelTextChangedEventArgs textChangedArgs = new PLCDBPanelTextChangedEventArgs(pr.fldname, pr.tb.Text + GetTimeValueIfExist(pr));
                            OnPLCDBPanelTextChanged(textChangedArgs);
                            TheArgs.errMsg = textChangedArgs.ErrorMessage;
                            TheArgs.isValid = textChangedArgs.IsValid;
                        }

                        if ((TheArgs.errMsg != "") && (TheArgs.isValid == false))
                        {
                            pr.ErrMsg.Text = TheArgs.errMsg;
                            pr.ErrMsg.Visible = true;
                            RecordOK = false;
                        }

                        if (TheArgs.handled)
                            continue;

                    }
                    catch
                    {
                    };



                    //Validate a required field
                    if ((pr.tb.Text.Trim() == "") && (pr.required == "T"))
                    {
                        pr.ErrMsg.Text = pr.GetRequiredMessage();
                        pr.ErrMsg.Visible = true;
                        RecordOK = false;
                    }



                    DateTime? dateTime = null;

                    if ((pr.fldtype == "T") && (pr.tb.Text != ""))
                    {


                        try
                        {
                            //TestDate = DateTime.Parse(pr.tb.Text);
                            TestDate = ConvertToDateTime(pr.tb.Text, pr.editmask);

                            string timeDateValue = string.Empty;
                            string timeDateField = pr.TimeDateField;
                            if (timeDateField.StartsWith("{") && timeDateField.EndsWith("}"))
                            {
                                timeDateField = timeDateField.Trim(new char[] { '{', '}', ' ' });
                                if (!string.IsNullOrEmpty(timeDateField))
                                {
                                    timeDateValue = getpanelfield(timeDateField) + " ";
                                }
                            }

                            if (!string.IsNullOrEmpty(timeDateValue.Trim()))
                            {
                                string timeDateMask = GetFieldMask(timeDateField);
                                dateTime = ConvertToDateTime(timeDateValue + pr.tb.Text, timeDateMask);
                            }
                        }
                        catch
                        {

                            pr.ErrMsg.Text = "  " + pr.prompt + " does not contain a valid time.";
                            pr.ErrMsg.Visible = true;
                            RecordOK = false;
                        }

                    }


                    if ((pr.fldtype == "D") && (pr.tb.Text != ""))
                    {

                        try
                        {
                            //TestDate = DateTime.Parse(pr.tb.Text);
                            TestDate = ConvertToDateTime(pr.tb.Text + GetTimeValueIfExist(pr), pr.editmask);

                            if (pr.time != null)
                                dateTime = TestDate;
                        }
                        catch
                        {

                            pr.ErrMsg.Text = "  " + pr.prompt + " does not contain a valid date.";
                            pr.ErrMsg.Visible = true;
                            RecordOK = false;
                        }

                    }

                    if ((pr.fldtype == "DT") && (pr.tb.Text != ""))
                    {
                        try
                        {
                            //TestDate = DateTime.Parse(pr.tb.Text);
                            TestDate = ConvertToDateTime(pr.tb.Text, pr.editmask);

                            dateTime = TestDate;
                        }
                        catch
                        {
                            pr.ErrMsg.Text = "  " + pr.prompt + " does not contain a valid date/time.";
                            pr.ErrMsg.Visible = true;
                            RecordOK = false;
                        }
                    }

                    if ((pr.fldtype == "DY") && (pr.tb.Text != ""))
                    {
                        int year = 0;
                        Int32.TryParse(pr.tb.Text, out year);
                        if (year < 1900 || year > 3000)
                        {
                            pr.ErrMsg.Text = "  " + pr.prompt + " does not contain a valid year.";
                            pr.ErrMsg.Visible = true;
                            RecordOK = false;
                        }
                    }


                    //Ticket#74727 - DBPANEL Email Validation
                    if (pr.fldtype == "EA" && pr.tb.Text.Trim() != "")
                    {
                        var rawEmails = pr.tb.Text
                            .Split(';'); // split emails
                        var trimmedEmails = rawEmails
                            .Select(email => email.Trim())
                            .Where(email => !string.IsNullOrEmpty(email)); // trim, and remove blank parts

                        if (rawEmails.Count() != trimmedEmails.Count()) // check if input has blank parts
                        {
                            pr.ErrMsg.Text = "  " + pr.prompt + " is invalid";
                            pr.ErrMsg.Visible = true;
                            RecordOK = false;
                        }
                        else
                        {
                            var hasInvalidEmail = trimmedEmails
                                .Any(email => !IsValidEmail(email)); // check if has atleast 1 invalid email

                            if (hasInvalidEmail)
                            {
                                pr.ErrMsg.Text = "  " + pr.prompt + " is invalid";
                                pr.ErrMsg.Visible = true;
                                RecordOK = false;
                            }
                            else
                            {
                                pr.tb.Text = string.Join("; ", trimmedEmails); // save trimmed emails
                            }
                        }
                    }


                    if (dateTime != null)
                    {
                        int secondsBase = 0;

                        if (pr.NoFutureDates)
                        {
                            bool isFuture = DateTime.Now.Subtract(dateTime.Value).TotalSeconds < secondsBase;
                            if (isFuture)
                            {
                                pr.ErrMsg.Text = "  " + pr.prompt + " contains a future date/time.";
                                pr.ErrMsg.Visible = true;
                                RecordOK = false;
                            }
                        }

                        if (pr.NoPastDates)
                        {
                            if (!pr.editmask.ToUpper().Contains(":SS"))
                                secondsBase = -59;

                            bool isPast = dateTime.Value.Subtract(DateTime.Now).TotalSeconds < secondsBase;
                            if (isPast)
                            {
                                pr.ErrMsg.Text = "  " + pr.prompt + " contains a past date/time.";
                                pr.ErrMsg.Visible = true;
                                RecordOK = false;
                            }
                        }
                    }
                }

                if (pr.ddl != null)
                {
                    try
                    {
                        //Check the user Validation routine....
                        TheArgs.handled = false;
                        TheArgs.fldName = pr.fldname;
                        TheArgs.fldValue = pr.ddl.SelectedValue;
                        TheArgs.errMsg = "";
                        TheArgs.isValid = true;
                        TheArgs.initVal = "";
                        TheArgs.custom1 = "";
                        TheArgs.custom2 = "";

                        if (pr.ddl.Attributes["INITVAL"] != null)
                            TheArgs.initVal = (string)pr.ddl.Attributes["INITVAL"];

                        if (pr.ddl.Attributes["CUSTOM1"] != null)
                            TheArgs.custom1 = (string)pr.ddl.Attributes["CUSTOM1"];

                        if (pr.ddl.Attributes["CUSTOM2"] != null)
                            TheArgs.custom2 = (string)pr.ddl.Attributes["CUSTOM2"];

                        onplcdbpanelvalidate(TheArgs);


                        if ((TheArgs.errMsg != "") && (TheArgs.isValid == false))
                        {
                            pr.ErrMsg.Text = TheArgs.errMsg;
                            pr.ErrMsg.Visible = true;
                            RecordOK = false;
                        }

                        if (TheArgs.handled)
                            continue;

                    }
                    catch
                    {
                    };



                    //Validate a required field
                    if ((pr.ddl.SelectedValue.Trim() == "") && (pr.required == "T"))
                    {
                        pr.ErrMsg.Text = pr.GetRequiredMessage();
                        pr.ErrMsg.Visible = true;
                        RecordOK = false;
                    }



                    if ((pr.fldtype == "T") && (pr.ddl.SelectedValue != ""))
                    {


                        try
                        {
                            TestDate = DateTime.Parse(pr.ddl.SelectedValue);
                        }
                        catch
                        {

                            pr.ErrMsg.Text = "  " + pr.prompt + " does not contain a valid time.";
                            pr.ErrMsg.Visible = true;
                            RecordOK = false;
                        }

                    }


                    if ((pr.fldtype == "D") && (pr.ddl.SelectedValue != ""))
                    {

                        try
                        {
                            TestDate = DateTime.Parse(pr.ddl.SelectedValue);
                        }
                        catch
                        {

                            pr.ErrMsg.Text = "  " + pr.prompt + " does not contain a valid date.";
                            pr.ErrMsg.Visible = true;
                            RecordOK = false;
                        }

                    }



                }

                if (pr.rbl != null)
                {
                    try
                    {
                        //Check the user Validation routine....
                        TheArgs.handled = false;
                        TheArgs.fldName = pr.fldname;
                        TheArgs.fldValue = pr.rbl.SelectedValue;
                        TheArgs.errMsg = "";
                        TheArgs.isValid = true;
                        TheArgs.initVal = "";
                        TheArgs.custom1 = "";
                        TheArgs.custom2 = "";

                        if (pr.rbl.Attributes["INITVAL"] != null)
                            TheArgs.initVal = (string)pr.rbl.Attributes["INITVAL"];

                        if (pr.rbl.Attributes["CUSTOM1"] != null)
                            TheArgs.custom1 = (string)pr.rbl.Attributes["CUSTOM1"];

                        if (pr.rbl.Attributes["CUSTOM2"] != null)
                            TheArgs.custom2 = (string)pr.rbl.Attributes["CUSTOM2"];

                        onplcdbpanelvalidate(TheArgs);


                        if ((TheArgs.errMsg != "") && (TheArgs.isValid == false))
                        {
                            pr.ErrMsg.Text = "Error: " + TheArgs.errMsg;
                            pr.ErrMsg.Visible = true;
                            RecordOK = false;
                        }

                        if (TheArgs.handled)
                            continue;

                    }
                    catch
                    {
                    };

                    //Validate a required field
                    if ((pr.rbl.SelectedValue.Trim() == "") && (pr.required == "T"))
                    {
                        pr.ErrMsg.Text = "  " + pr.prompt + " is required";
                        pr.ErrMsg.Visible = true;
                        RecordOK = false;
                    }
                }

                //08152011 - Added Validation for code multipick
                if (pr.chMultiLookup != null)
                {
                    try
                    {
                        //Check the user Validation routine....
                        TheArgs.handled = false;
                        TheArgs.fldName = pr.fldname;
                        TheArgs.fldValue = pr.chMultiLookup.GetText();
                        TheArgs.errMsg = "";
                        TheArgs.isValid = true;
                        TheArgs.initVal = "";
                        TheArgs.custom1 = "";
                        TheArgs.custom2 = "";

                        if (pr.chMultiLookup.Attributes["INITVAL"] != null)
                            TheArgs.initVal = (string)pr.chMultiLookup.Attributes["INITVAL"];

                        if (pr.chMultiLookup.Attributes["CUSTOM1"] != null)
                            TheArgs.custom1 = (string)pr.chMultiLookup.Attributes["CUSTOM1"];

                        if (pr.chMultiLookup.Attributes["CUSTOM2"] != null)
                            TheArgs.custom2 = (string)pr.chMultiLookup.Attributes["CUSTOM2"];

                        onplcdbpanelvalidate(TheArgs);

                        if ((TheArgs.errMsg != "") && (TheArgs.isValid == false))
                        {
                            pr.ErrMsg.Text = TheArgs.errMsg;
                            pr.ErrMsg.Visible = true;
                            RecordOK = false;
                        }

                        if (TheArgs.handled)
                            continue;

                    }
                    catch (Exception e)
                    {
                        pr.ErrMsg.Text = "Exception in " + pr.prompt + " validation:" + e.Message;
                        pr.ErrMsg.Visible = true;
                        RecordOK = false;
                    };

                    string[] multipickValues = pr.chMultiLookup.GetText().Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    if (pr.codetable != "")
                    {
                        PLCQuery qrySchema = new PLCQuery();
                        qrySchema.SQL = "SELECT * FROM " + pr.codetable + " WHERE 1=0";
                        qrySchema.Open();
                        string codeTableField = string.Empty;

                        if (qrySchema.PLCDataTable.Columns.Count > 0)
                        {
                            codeTableField = qrySchema.PLCDataTable.Columns[0].ColumnName;
                            if (codeTableField == "ROWID")
                                codeTableField = qrySchema.PLCDataTable.Columns[1].ColumnName;
                        }

                        foreach (string multipickValue in multipickValues)
                        {
                            string value = multipickValue.Trim();

                            if (pr.codetable.ToUpper().Contains("ATTRPICK"))
                            {
                                if (value != "" && !ValidAttribute(pr.fldname, value))
                                {
                                    pr.ErrMsg.Text = "  [" + value + "] is not a valid code for " + pr.prompt;
                                    pr.ErrMsg.Visible = true;
                                    RecordOK = false;
                                }
                            }
                            else
                            {
                                if (value != "" && !CheckMultipickChoiceValidity(pr.codetable, codeTableField, value))
                                {
                                    pr.ErrMsg.Text = "  [" + value + "] is not a valid code for " + pr.prompt;
                                    pr.ErrMsg.Visible = true;
                                    RecordOK = false;
                                }
                            }
                        }
                    }

                    //Validate a required field
                    if ((multipickValues.Length == 0) && (pr.required == "T"))
                    {
                        pr.ErrMsg.Text = pr.GetRequiredMessage();
                        pr.ErrMsg.Visible = true;
                        RecordOK = false;
                    }
                }

                if (pr.sig != null)
                {
                    try
                    {
                        //Check the user Validation routine....
                        TheArgs.handled = false;
                        TheArgs.fldName = pr.fldname;
                        TheArgs.fldValue = pr.sig.Key.ToString();
                        TheArgs.errMsg = "";
                        TheArgs.isValid = true;
                        TheArgs.initVal = "";
                        TheArgs.custom1 = "";
                        TheArgs.custom2 = "";

                        if (pr.sig.Attributes["INITVAL"] != null)
                            TheArgs.initVal = (string)pr.sig.Attributes["INITVAL"];

                        if (pr.sig.Attributes["CUSTOM1"] != null)
                            TheArgs.custom1 = (string)pr.sig.Attributes["CUSTOM1"];

                        if (pr.sig.Attributes["CUSTOM2"] != null)
                            TheArgs.custom2 = (string)pr.sig.Attributes["CUSTOM2"];

                        onplcdbpanelvalidate(TheArgs);

                        if ((TheArgs.errMsg != "") && (TheArgs.isValid == false))
                        {
                            pr.ErrMsg.Text = TheArgs.errMsg;
                            pr.ErrMsg.Visible = true;
                            RecordOK = false;
                        }

                        if (TheArgs.handled)
                            continue;
                    }
                    catch (Exception e)
                    {
                        pr.ErrMsg.Text = "Exception in " + pr.prompt + " validation:" + e.Message;
                        pr.ErrMsg.Visible = true;
                        RecordOK = false;
                    };

                    //Validate a required field
                    if ((pr.sig.IsEmpty()) && (pr.required == "T"))
                    {
                        pr.ErrMsg.Text = "  " + pr.prompt + " is required";
                        pr.ErrMsg.Visible = true;
                        RecordOK = false;
                    }
                }

                if (pr.uniqueConstraint)
                {
                    qryUniqueConstraint += (qryUniqueConstraint != "" ? " AND " : "SELECT * FROM " + PLCDataTable + " WHERE " ) + pr.fldname + " = '" + TheArgs.fldValue + "'";
                    errUniqueConstraint += (errUniqueConstraint != "" ? ", " : "<br />A record already exists for ") + pr.prompt;
                }

                //*AAC* 060509 Check if at least one field has a value.
                if (TheArgs.fldValue.Length > 0)
                    hasEntries = true;
            }

            //*AAC* 060509 Check if at least one field has a value.
            if ((!hasEntries) && (!PLCAllowBlankRecordSave))
            {
                int lastIndex = panelrecs.Count - 1;
                panelrecs[lastIndex].ErrMsg.Text += "<br />A blank record cannot be saved.";
                panelrecs[lastIndex].ErrMsg.Visible = true;
                RecordOK = false;
            }

            if (IsNewRecord && RecordOK && !string.IsNullOrEmpty(qryUniqueConstraint))
            {
                PLCQuery query = new PLCQuery(qryUniqueConstraint);
                query.Open();
                if (query.HasData())
                {
                    int lastIndex = panelrecs.Count - 1;
                    panelrecs[lastIndex].ErrMsg.Text = errUniqueConstraint;
                    panelrecs[lastIndex].ErrMsg.Visible = true;
                    RecordOK = false;
                }
            }

            return RecordOK;
        }

        public bool CanSearch()
        {
            this.PLCAllowBlankRecordSave = true;

            var codeConditions = new Dictionary<PanelRec, string>();

            // format code condition to handle empty parent
            foreach (PanelRec pr in panelrecs)
            {
                string codeCondition = pr.codecondition;
                if (!string.IsNullOrEmpty(codeCondition))
                {
                    pr.codecondition = "(" + FormatCodeCondition(codeCondition, processEmptyParent: true) + ")";
                    codeConditions.Add(pr, codeCondition);
                }
            }

            bool canSearch = this.CanSave();
            
            // restore original code conditions
            foreach (PanelRec pr in panelrecs)
            {
                if (codeConditions.ContainsKey(pr))
                {
                    PLCSession.WriteDebug("@CanSearch: " + pr.codecondition);
                    pr.codecondition = codeConditions[pr];
                    PLCSession.WriteDebug("@CanSearch: " + pr.codecondition);
                }
            }

            return canSearch;
        }

        protected bool CheckMultipickChoiceValidity(string CodeTable, string fldname, string flddata)
        {
            PLCQuery qryMultiPick = new PLCQuery();
            qryMultiPick.SQL = "SELECT  * FROM " + CodeTable + " WHERE " + fldname + " = ?";
            qryMultiPick.AddSQLParameter(fldname, flddata);
            qryMultiPick.Open();
            if (qryMultiPick.HasData())
                return true;
            else
                return false;
            
        }

        public void HideErrorMessage()
        {
            foreach (PanelRec pr in panelrecs)
            {
                pr.ErrMsg.Text = "";
                pr.ErrMsg.Visible = false;
            }
        }

        // Enable or disable field control.
        public void EnablePanelField(string fldname, bool bEnable)
        {
            bool bIsReadOnly = !bEnable;
            Color backColor = bEnable ? Color.White : Color.LightGray;

            foreach (PanelRec pr in panelrecs)
            {
                if (pr.fldname.ToUpper() == fldname.ToUpper())
                {
                    if (pr.btn != null)
                        pr.btn.Enabled = bEnable;

                    if (pr.tb != null)
                    {
                        pr.tb.BackColor = backColor;
                        pr.tb.ReadOnly = bIsReadOnly;

                        //hide calendar button
                        if (pr.isDateMask(pr.editmask))
                            ShowCalendarButton(pr, bEnable);

                        if (pr.time != null)
                            SetTimeTextBoxMode(pr, bIsReadOnly);
                    }
                    else if (pr.cb != null)
                    {
                        pr.cb.Enabled = bEnable;
                    }
                    else if (pr.ddl != null)
                    {
                        SetDropDownListMode(pr.ddl, bIsReadOnly);
                    }
                    else if (pr.rbl != null)
                    {
                        SetRadioButtonListMode(pr.rbl, bIsReadOnly);
                    }
                    else if (pr.chMultiLookup != null)
                    {
                        TextBox tb = pr.chMultiLookup.GetTextBox();
                        tb.BackColor = backColor;
                        tb.ReadOnly = bIsReadOnly;

                        pr.chMultiLookup.Enable(bEnable);
                    }
                    else if (pr._codetable != "")
                    {
                        if (pr.chtb != null)
                        {
                            CodeHeadEnableControls(pr, bEnable);
                            if (pr.DescCtrl == null)
                                CodeHeadSetLabel(pr, GetCodeDesc(pr._codetable, pr.CodeTablePrimaryKeyField, pr.CodeTablePrimaryKeyData, pr._desc, pr.codecondition));
                            else
                            {
                                if (pr.chtb.Text != "")
                                    pr.DescCtrl.ReadOnly = bIsReadOnly;
                                else
                                    pr.DescCtrl.ReadOnly = bEnable;
                            }
                        }
                        else if (pr.chComboBox != null)
                        {
                            SetDropDownListMode(pr.chComboBox, bIsReadOnly);
                        }
                        else if (pr.chFlexBox != null)
                        {
                            pr.chFlexBox.ReadOnly = bIsReadOnly;
                        }
                    }

                    break;
                }
            }
        }

        /// <summary>
        /// Version 2 of EnablePanelField that works with
        /// the new implementation of readonly
        /// </summary>
        /// <param name="fldname"></param>
        /// <param name="bEnable"></param>
        public void EnablePanelFieldByAttrib(string fldname, bool bEnable)
        {
            bool bIsReadOnly = !bEnable;
            Color backColor = bEnable ? Color.White : Color.LightGray;

            foreach (PanelRec pr in panelrecs)
            {
                if (pr.fldname.ToUpper() == fldname.ToUpper())
                {
                    if (pr.btn != null)
                        pr.btn.Enabled = bEnable;

                    if (pr.tb != null)
                    {
                        pr.tb.BackColor = backColor;

                        if (bIsReadOnly)
                            pr.tb.Attributes["readonly"] = "readonly";
                        else
                            pr.tb.Attributes.Remove("readonly");

                        //hide calendar button
                        if (pr.isDateMask(pr.editmask))
                            ShowCalendarButton(pr, bEnable);

                        if (pr.time != null)
                            SetTimeTextBoxMode(pr, bIsReadOnly);
                    }
                    else if (pr.cb != null)
                    {
                        pr.cb.Enabled = bEnable;
                    }
                    else if (pr.ddl != null)
                    {
                        SetDropDownListMode(pr.ddl, bIsReadOnly);
                    }
                    else if (pr.rbl != null)
                    {
                        SetRadioButtonListMode(pr.rbl, bIsReadOnly);
                    }
                    else if (pr.chMultiLookup != null)
                    {
                        TextBox tb = pr.chMultiLookup.GetTextBox();
                        tb.BackColor = backColor;
                        tb.ReadOnly = bIsReadOnly;

                        pr.chMultiLookup.Enable(bEnable);
                    }
                    else if (pr._codetable != "")
                    {
                        if (pr.chtb != null)
                        {
                            CodeHeadEnableControls(pr, bEnable);
                            if (pr.DescCtrl == null)
                                CodeHeadSetLabel(pr, GetCodeDesc(pr._codetable, pr.CodeTablePrimaryKeyField, pr.CodeTablePrimaryKeyData, pr._desc, pr.codecondition));
                            else
                            {
                                if (pr.chtb.Text != "")
                                    pr.DescCtrl.ReadOnly = bIsReadOnly;
                                else
                                    pr.DescCtrl.ReadOnly = bEnable;
                            }
                        }
                        else if (pr.chComboBox != null)
                        {
                            SetDropDownListMode(pr.chComboBox, bIsReadOnly);
                        }
                        else if (pr.chFlexBox != null)
                        {
                            pr.chFlexBox.ReadOnly = bIsReadOnly;
                        }
                    }

                    break;
                }
            }
        }


        protected void StartSavingMode()
        {
            IsSavingMode = true;
        }



        protected void StopSavingMode()
        {
            IsSavingMode = false;
        }
        


        protected void BrowseMode()
        {
            IsBrowseMode = true;

            foreach (PanelRec pr in panelrecs)
            {


                if (pr.btn != null)
                    pr.btn.Enabled = false;

                if (pr.tb != null)
                {
                    pr.tb.BackColor = Color.LightGray;
                    pr.tb.Attributes["readonly"] = "readonly";

                    //hide calendar button
                    if (pr.isDateMask(pr.editmask))
                        ShowCalendarButton(pr, false);

                    if (pr.time != null)
                        SetTimeTextBoxMode(pr, true);
                }
                else if (pr.cb != null)
                {
                    pr.cb.Enabled = false;
                }
                else if (pr.ddl != null)
                {
                    pr.ddl.Enabled = false;
                    SetDropDownListMode(pr.ddl, true);
                }
                else if (pr.rbl != null)
                {
                    SetRadioButtonListMode(pr.rbl, true);
                }
                else if (pr.chMultiLookup != null)
                {
                    TextBox tb = pr.chMultiLookup.GetTextBox();
                    tb.BackColor = Color.LightGray;
                    tb.ReadOnly = true;

                    pr.chMultiLookup.Enable(false);
                }
                else if (pr.chMultipickAc != null)
                {
                    TextBox tb = pr.chMultipickAc.GetTextBox();
                    tb.BackColor = Color.LightGray;
                    tb.ReadOnly = true;

                    pr.chMultipickAc.Enable(false);
                }
                else if (pr._codetable != "")
                {
                    if (pr.chtb != null)
                    {
                        CodeHeadEnableControls(pr, false);
                        if (pr.DescCtrl == null)
                            CodeHeadSetLabel(pr, GetCodeDesc(pr._codetable, pr.CodeTablePrimaryKeyField, pr.CodeTablePrimaryKeyData, pr._desc, pr.codecondition));
                        else
                        {
                            if (pr.chtb.Text != "")
                                pr.DescCtrl.ReadOnly = true;
                            else
                                pr.DescCtrl.ReadOnly = false;
                        }
                    }
                    else if (pr.chComboBox != null)
                    {
                        SetDropDownListMode(pr.chComboBox, true);
                    }
                    else if (pr.chFlexBox != null)
                    {
                        pr.chFlexBox.ReadOnly = true;
                    }
                }
            }

        }

        private static void ShowCalendarButton(PanelRec pr, bool isVisible)
        {
            if (pr.tb.Parent is TableCell)
            {
                foreach (Control childControl in pr.tb.Parent.Controls)
                {
                    if (childControl is ImageButton)
                    {
                        ((ImageButton)childControl).Visible = isVisible;
                    }
                    if (childControl is CalendarExtender)
                    {
                        ((CalendarExtender)childControl).Enabled = isVisible;
                    }
                }
            }
        }

        private static void SetDropDownListMode(DropDownList ddl, bool isReadOnly)
        {
            ddl.BackColor = isReadOnly ? Color.LightGray : Color.White;
            foreach (ListItem item in ddl.Items)
            {
                item.Enabled = true;
                if (!item.Selected)
                    item.Enabled = !isReadOnly;
                else
                {

                }
            }
        }

        private static void SetRadioButtonListMode(RadioButtonList rbl, bool isReadOnly)
        {
            foreach (ListItem item in rbl.Items)
            {
                item.Enabled = true;
                if (!item.Selected)
                    item.Enabled = !isReadOnly;
            }
        }

        public void ClearErrors()
        {
            foreach (PanelRec pr in panelrecs)
            {
                pr.ErrMsg.Text = "";
                pr.ErrMsg.Visible = false;
            }
        }

        public void ClearFields()
        {
            foreach (PanelRec pr in panelrecs)
            {
                pr.ErrMsg.Text = "";
                pr.ErrMsg.Visible = false;

                if (pr.tb != null)
                {
                    pr.tb.Text = "";

                    if (pr.time != null)
                        pr.time.Text = "";
                }
                else if (pr.ddl != null)
                {
                    if (pr.ddl.Items.FindByValue("") != null)
                        pr.ddl.SelectedValue = "";
                    else
                        pr.ddl.SelectedIndex = -1;
                }
                else if (pr.rbl != null)
                {
                    if (pr.rbl.Items.FindByValue("") != null)
                        pr.rbl.SelectedValue = "";
                    else
                        pr.rbl.SelectedIndex = -1;
                }
                else if (pr.cb != null)
                {
                    pr.cb.Checked = false;
                }
                else if (pr.chMultiLookup != null)
                {
                    pr.chMultiLookup.Clear();
                }
                else if (pr.chMultipickAc != null)
                {
                    pr.chMultipickAc.Clear();
                }
                else if (pr.sig != null)
                {
                    pr.sig.Clear();
                }
                else if (pr._codetable != "")
                {
                    if (pr.chtb != null)
                    {
                        pr.chtb.Text = "";
                        pr.chlabel.Text = "";
                    }
                    else if (pr.chComboBox != null)
                    {
                        pr.chComboBox.Items.Clear();
                    }
                    else if (pr.chFlexBox != null)
                    {
                        pr.chFlexBox.SelectedValue = "";
                    }
                }
            }
        }

        public void EmptyMode()
        {
            foreach (PanelRec pr in panelrecs)
            {
                if (pr.btn != null)
                    pr.btn.Enabled = false;

                if (pr.tb != null)
                {
                    pr.tb.BackColor = Color.LightGray;
                    pr.tb.Attributes["readonly"] = "readonly";
                    pr.tb.Text = "";

                    if (pr.time != null)
                    {
                        SetTimeTextBoxMode(pr, true);
                        pr.time.Text = "";
                    }
                }
                else if (pr.ddl != null)
                {
                    if (pr.ddl.Items.FindByValue("") != null)
                        pr.ddl.SelectedValue = "";
                    SetDropDownListMode(pr.ddl, true);
                }
                else if (pr.rbl != null)
                {
                    if (pr.rbl.Items.FindByValue("") != null)
                        pr.rbl.SelectedValue = "";
                    SetRadioButtonListMode(pr.rbl, true);
                    //pr.rbl.SelectedIndex = -1;
                }
                else if (pr.cb != null)
                {
                    pr.cb.Checked = false;
                    pr.cb.Enabled = false;
                }
                else if (pr.chMultiLookup != null)
                {
                    TextBox tb = pr.chMultiLookup.GetTextBox();
                    tb.BackColor = Color.LightGray;
                    tb.ReadOnly = true;

                    pr.chMultiLookup.SetText("");
                    pr.chMultiLookup.Enable(false);
                }
                else if (pr.chMultipickAc != null)
                {
                    TextBox tb = pr.chMultipickAc.GetTextBox();
                    tb.BackColor = Color.LightGray;
                    tb.ReadOnly = true;

                    pr.chMultipickAc.SetText("");
                    pr.chMultipickAc.Enable(false);
                }
                else if (pr._codetable != "")
                {
                    if (pr.chtb != null)
                    {
                        CodeHeadEnableControls(pr, false);
                        pr.chtb.Text = "";
                        pr.chlabel.Text = "";
                    }
                    else if (pr.chComboBox != null)
                    {
                        SetDropDownListMode(pr.chComboBox, true);
                        if (pr.chComboBox.Items.FindByValue("") != null)
                            pr.chComboBox.SelectedValue = "";
                    }
                    else if (pr.chFlexBox != null)
                    {
                        pr.chFlexBox.ReadOnly = true;
                        pr.chFlexBox.SelectedValue = "";
                    }
                }
            }

            SetDBPanelChildRequired();
        }

        protected void DoDeleteRecord()
        {

            //string sqlstr = "DELETE FROM " + PLCDataTable + " " + PLCWhereClause;

            //PLCQuery qryDelete = new PLCQuery();
            //qryDelete.SQL = sqlstr;
            //qryDelete.ExecSQL("");
            PLCQuery qryDelete = new PLCQuery();
            qryDelete.Delete(PLCDataTable, PLCWhereClause, PLCDeleteAuditCode, PLCDeleteAuditSubCode, PLCConfigPanel);
        }

        public virtual Boolean IsNewRecord
        {
            get
            {
                if (HttpContext.Current.Session[this.UniqueID + "_IsNewRecord"] == null)
                {
                    HttpContext.Current.Session[this.UniqueID + "_IsNewRecord"] = false;
                }

                return (bool)HttpContext.Current.Session[this.UniqueID + "_IsNewRecord"];
            }
            set
            {
                HttpContext.Current.Session[this.UniqueID + "_IsNewRecord"] = value;
                HttpContext.Current.Session[this.UniqueID + "_PLCWhereClause"] = PLCWhereClause;
            }
        }

        protected void SetRecordAddMode()
        {
            //ViewState["ADDMODE"] = PLCWhereClause;
            IsNewRecord = true;
        }

        protected void ClearRecordAddMode()
        {
            //ViewState["ADDMODE"] = "";
            IsNewRecord = false;
        }

        protected void BlankRecord()
        {
            foreach (PanelRec pr in panelrecs)
            {

                pr.ErrMsg.Text = "";

                if (pr.tb != null)
                {
                    pr.tb.Text = "";

                    if (pr.time != null)
                        pr.time.Text = "";
                }

                if (pr.ddl != null)
                {
                    if (pr.ddl.Items.FindByValue("") != null)
                        pr.ddl.SelectedValue = "";
                }

                if (pr.rbl != null)
                {
                    pr.rbl.SelectedIndex = -1;
                }

                if (pr.codelbl != null)
                {
                    pr.codelbl.Text = "";
                }

            }
        }


        // If string is null, return empty.
        private string UnNull(string str)
        {
            if (String.IsNullOrEmpty(str))
                return "";
            else
                return str;
        }

        protected string GetCodeDesc(string tblname, string codeField, string codedata, string descField, string codecond)
        {
            string sqlstr = "";

            try
            {

                //string codeField = "";
                //string descField = "";
                //string keyField = "";
                //string codeDesc = "";
                string result = "";

                PLCQuery qryCodeTable = new PLCQuery();

                if (codedata == "")
                    return "";
                if (codedata == "\0")
                    return "";

                string myCode = "";
                string[] flditems = codedata.Split(',');
                foreach (string acode in flditems)
                {
                    myCode = acode.Trim();
                    sqlstr = "select  * from " + tblname + " Where  " + codeField + " = ?";
                    string fmtCodeCond = FormatCodeCondition(codecond);
                    if ((codecond != null) && (codecond != "") && (fmtCodeCond != ""))
                        sqlstr += " AND " + fmtCodeCond;
                    qryCodeTable.SQL = sqlstr;
                    // prevent excption from parameter already existing since we are looping
                    qryCodeTable.ClearParameters();
                    qryCodeTable.AddSQLParameter(codeField, myCode);
                    qryCodeTable.Open();
                    if (qryCodeTable.HasData())
                    {
                        if (result == "")
                            result = qryCodeTable.FieldByName(descField);
                        else
                            result += ", " + qryCodeTable.FieldByName(descField);
                    }
                }
                return result;
            }

            catch (Exception ex)
            {
                PLCSessionVars PLCSessionVars1 = new PLCSessionVars();
                PLCSessionVars1.ClearError();
                PLCSessionVars1.PLCErrorURL = "*";
                PLCSessionVars1.PLCErrorProc = "PLCDBPanel." + PLCPanelName;
                PLCSessionVars1.PLCErrorSQL = sqlstr;
                PLCSessionVars1.PLCErrorMessage = "Exception: " + ex.Message.Trim() + String.Format("in GetCodeDesc({0}, {1}, {2}, {3}, {4});", UnNull(tblname), UnNull(codeField), UnNull(codedata), UnNull(descField), UnNull(codecond));
                PLCSessionVars1.SaveError();
                PLCSessionVars1.Redirect("~/PLCWebCommon/Error.aspx");
                return "";
            }
        }

        public string generatesql()
        {
            PLCSessionVars sv = new PLCSessionVars();

            string tempstr = "";

            if (PLCDataTable == "")
                return "";

            foreach (PanelRec pr in panelrecs)
            {
                string fieldName = pr.fldname;

                if (tempstr != "")
                    tempstr += ", ";

                if (((pr.tblname == "LABCASE") || (pr.tblname == "TV_LABCASE")) && ((fieldName == "Reference") || (fieldName.ToUpper() == "REFERENCE")))
                {
                    tempstr += "'*REFERENCE'";
                }
                else
                {

                    if (pr.fldtype == "T")
                    {
                        if (sv.PLCDatabaseServer == "MSSQL")
                        {
                            if (pr.editmask.ToUpper() == "HH:MM:SS")
                                tempstr += "convert(char(8), " + fieldName + ", 108) " + fieldName;
                            else
                                tempstr += "convert(char(5), " + fieldName + ", 108) " + fieldName;
                        }
                        else
                        {
                            if (pr.editmask.ToUpper() == "HH:MM:SS")
                                tempstr += "to_char(" + fieldName + ",'HH24:MI:SS') " + fieldName;
                            else
                                tempstr += "to_char(" + fieldName + ",'HH24:MI') " + fieldName;
                        }
                    }
                    else if (pr.fldtype == "DT") //datetime
                    {
                        if (sv.PLCDatabaseServer == "MSSQL") //mssql
                        {
                            if (pr.editmask.ToUpper().Contains(":SS"))
                                tempstr += "CONVERT(VARCHAR(10), " + fieldName + ", 101) + ' ' + CONVERT(VARCHAR(8), " + fieldName + ", 114) AS " + fieldName; //with seconds
                                //tempstr += string.Format("CONVERT(VARCHAR(10), {0}, {1}) + ' ' + CONVERT(VARCHAR(8), {0}, 114) AS {0}", fieldName, IsDMYFormat(pr.editmask) ? "103" : "101"); //with seconds
                            else
                                tempstr += "CONVERT(VARCHAR(10), " + fieldName + ", 101) + ' ' + CONVERT(VARCHAR(5), " + fieldName + ", 114) AS " + fieldName; //without seconds
                                //tempstr += string.Format("CONVERT(VARCHAR(10), {0}, {1}) + ' ' + CONVERT(VARCHAR(5), {0}, 114) AS {0}", fieldName, IsDMYFormat(pr.editmask) ? "103" : "101"); //without seconds
                        }
                        else //oracle
                        {
                            if (pr.editmask.ToUpper().Contains(":SS"))
                                tempstr += "to_char(" + fieldName + ", 'MM/DD/YYYY HH24:MI:SS') AS " + fieldName; //with seconds
                                //tempstr += string.Format("to_char({0}, '{1} HH24:MI:SS') AS {0}", fieldName, IsDMYFormat(pr.editmask) ? "DD/MM/YYYY" : "MM/DD/YYYY"); //with seconds
                            else
                                tempstr += "to_char(" + fieldName + ", 'MM/DD/YYYY HH24:MI') AS " + fieldName; //without seconds
                                //tempstr += string.Format("to_char({0}, '{1} HH24:MI') AS {0}", fieldName, IsDMYFormat(pr.editmask) ? "DD/MM/YYYY" : "MM/DD/YYYY"); //without seconds
                        }
                    }
                    else
                        tempstr += fieldName;
                }
            }

            if (tempstr == "")
                tempstr = " * ";
            tempstr = "select " + tempstr + " from " + PLCDataTable + " " + PLCWhereClause;

            return tempstr;

        }

        protected Boolean ValidAttribute(string AttrCode, string flddata)
        {

            if (flddata == "")
                return true;
            if (flddata == "\0")
                return true;

            PLCQuery qryAttrPick = new PLCQuery();
            qryAttrPick.SQL = "select  * from TV_ATTRPICK " + " Where  ATTRIBUTE = '" + AttrCode + "' and VALUE = '" + flddata + "'";
            qryAttrPick.Open();
            if (qryAttrPick.HasData())
                return true;
            else
                return false;

        }

        private bool IsValidTableValue(string tableName, string column, string value, string filter)
        {
            if (string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(column) || string.IsNullOrEmpty(value))
                return false;

            PLCQuery qryValue = new PLCQuery();
            qryValue.SQL = "SELECT * FROM " + tableName + " WHERE UPPER(" + column + ") = '" + value.ToUpper() + "'" + (string.IsNullOrEmpty(filter) ? "" : " AND " + filter);
            qryValue.Open();
            if (!qryValue.IsEmpty())
                return true;

            return false;
        }

        protected Boolean CodeValid(string tblname, string flddata, string codeCondition)
        {
            string codeField = "";

            if (flddata == "")
                return true;
            if (flddata == "\0")
                return true;

            try
            {

                //PLCQuery qryCodeTable = new PLCQuery();
                //qryCodeTable.SQL = "select  * from " + tblname + " Where  0 = 1";
                //qryCodeTable.Open();

                PLCQuery qryCodeFields = CacheHelper.OpenCachedSqlFieldNames("SELECT * FROM " + tblname + " WHERE 0 = 1");
                codeField = qryCodeFields.FieldNames(1);


                PLCQuery qryCodeTable = new PLCQuery();
                string sqlstr = "";
                string myCode = "";
                string[] flditems = flddata.Split(',');
                foreach (string acode in flditems)
                {
                    myCode = acode.Trim();
                    sqlstr = "select  * from " + tblname + " Where  \"" + codeField + "\" = ?" + (string.IsNullOrEmpty(codeCondition) ? "" : " AND " + FormatCodeCondition(codeCondition));
                    qryCodeTable.SQL = sqlstr;
                    // prevent excption from parameter already existing since we are looping
                    qryCodeTable.ClearParameters();
                    qryCodeTable.AddSQLParameter(codeField, myCode);
                    qryCodeTable.OpenReadOnly();
                    if (qryCodeTable.IsEmpty())
                    {

                        return false;

                    }
                }

                return true;
            }

            catch
            {
                return true;
            }


        }

        /// <summary>
        /// Validates a single code value
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="code"></param>
        /// <param name="codeCondition"></param>
        /// <returns></returns>
        protected bool IsCodeValid(string tableName, string code, string codeCondition)
        {
            string codeField = "";

            if (code == "")
                return true;
            if (code == "\0")
                return true;

            try
            {
                PLCQuery qryCodeFields = CacheHelper.OpenCachedSqlFieldNames("SELECT * FROM " + tableName + " WHERE 0 = 1");
                codeField = qryCodeFields.FieldNames(1);

                PLCQuery qryCodeTable = new PLCQuery();
                string sql = "select  * from " + tableName + " Where  \"" + codeField + "\" = ?" + (string.IsNullOrEmpty(codeCondition) ? "" : " AND " + FormatCodeCondition(codeCondition));
                qryCodeTable.SQL = sql;
                qryCodeTable.AddSQLParameter(codeField, code);
                qryCodeTable.OpenReadOnly();
                return qryCodeTable.HasData();
            }
            catch
            {
                return true;
            }
        }

        public void LoadCodeDescriptions()
        {
            foreach (PanelRec pr in panelrecs)
            {


                //   if (pr.codehead != null)
                //       pr.codehead.SetLabel(pr.codehead.GetCodeDesc(pr.codetable, pr.codehead.GetValue()));

                if (pr.codelbl != null)
                {
                    if (pr.DescCtrl == null)
                    {
                        pr.codelbl.ReadOnly = false;
                        pr.codelbl.Text = GetCodeDesc(pr.codetable, pr.CodeTablePrimaryKeyField, pr.CodeTablePrimaryKeyData, pr._desc, pr.codecondition);
                        pr.codelbl.ReadOnly = true;
                    }
                }
            }
        }

        public string GetOriginalValue(string fieldName)
        {
            string originalValue = "";

            foreach (PanelRec pr in panelrecs)
            {
                if (pr.fldname.ToUpper() == fieldName.ToUpper())
                    return pr.original.Value;
            }

            return originalValue;
        }

		public string GetOriginalDesc(string fieldName)
		{
			foreach (PanelRec pr in panelrecs)
				if (pr.fldname.ToUpper() == fieldName.ToUpper())
					return pr.originalDesc.Value;
			return null;
		}

        public string GetControlClientID(string fieldName)
        {
            string clientID = null;

            foreach (PanelRec pr in panelrecs)
            {
                if (pr.fldname.ToUpper() == fieldName.ToUpper())
                {
                    if (pr.tb != null)
                        return pr.tb.ClientID;
                    else if (pr.ddl != null)
                        return pr.ddl.ClientID;
                    else if (pr.rbl != null)
                        return pr.rbl.ClientID;
                    else if (pr.chtb != null)
                        return pr.chtb.ClientID;
                    else if (pr.chComboBox != null)
                        return pr.chComboBox.ClientID;
                    else if (pr.chFlexBox != null)
                        return pr.chFlexBox.ClientID;
                }
            }

            return clientID;
        }

        public void LoadRecordForSpecialPanel(Dictionary<string, string> dicTemp)
        {
            if (dicTemp == null)
            {
                return;
            }

            BlankRecord();

            //PLCQuery qryLoad = new PLCQuery();
            //qryLoad.SQL = sqlstr;
            //qryLoad.Open();
            //if (qryLoad.IsEmpty()) return;

            foreach (PanelRec pr in panelrecs)
            {
                pr.original.Value = dicTemp[pr.fldname];

                if (!string.IsNullOrEmpty(pr.defaultValue) && pr.original.Value.Equals(pr.defaultValue))
                    pr.original.Value = string.Empty;

                if (pr.tb != null)
                {
                    if (!(HandleIfNumberMask(pr, dicTemp[pr.fldname])))
                        pr.tb.Text = dicTemp[pr.fldname];
					pr.originalDesc.Value = pr.tb.Text;
                }

                if (pr.ddl != null)
                {
                    if (IsNewRecord)
                    {
						if (pr.ddl.Items.FindByValue(pr.defaultValue) != null)
						{
							pr.ddl.SelectedValue = pr.defaultValue;
							pr.originalDesc.Value = pr.ddl.SelectedItem.Text;
						}
                    }
                    else
                    {
						if (pr.ddl.Items.FindByValue(dicTemp[pr.fldname]) != null)
						{
							pr.ddl.SelectedValue = dicTemp[pr.fldname];
							pr.originalDesc.Value = pr.ddl.SelectedItem.Text;
						}
					}
                }

                if (pr.rbl != null)
                {
                    string value = dicTemp[pr.fldname];
                    if (pr.rbl.Items.FindByValue(value) != null)
                    {
                        if (pr.rbl.SelectedIndex > -1)
                        {
                            pr.rbl.SelectedValue = value;
                            pr.originalDesc.Value = pr.rbl.SelectedItem.Text;
                        }
                    }
                }
                //if (pr.codelbl != null)
                //pr.codelbl.CssClass = "'<%# css_plcdbpanel %>'";

                if (pr.chtb != null)
                {
                    string fieldName = pr.fldname;

                    CodeHeadSetValue(pr, dicTemp[pr.fldname]);
					pr.originalDesc.Value = pr.chlabel.Text != "" ? pr.chlabel.Text : dicTemp[pr.fldname];
                }
                else if (pr.chComboBox != null)
                {
                    CodeHeadSetValue(pr, dicTemp[pr.fldname]);
					pr.originalDesc.Value = pr.chComboBox.SelectedItem.Text != "" ? pr.chComboBox.SelectedItem.Text : dicTemp[pr.fldname];
                }
                else if (pr.chFlexBox != null)
                {
                    CodeHeadSetValue(pr, dicTemp[pr.fldname]);
					pr.originalDesc.Value = pr.chFlexBox.SelectedText != "" ? pr.chFlexBox.SelectedText : pr.chFlexBox.GetDescription(dicTemp[pr.fldname]);				
				}
                else if (pr.chMultiLookup != null)
                {
                    pr.chMultiLookup.SetText(dicTemp[pr.fldname]);
					string desc = string.Join(",", pr.chMultiLookup.GetSelectionDescription().ToArray());
					pr.originalDesc.Value = desc.Trim() != "" ? desc : pr.chMultiLookup.GetText();
                }

                if (pr.editmask == "CB" || pr.editmask == "CB-R")
                {
                    if (pr.cb != null)
                    {
                        if (dicTemp[pr.fldname] == "T")
                            pr.cb.Checked = true;
                        else
                            pr.cb.Checked = false;
						pr.originalDesc.Value = pr.cb.Checked ? "True" : "False";
                    }
                }
            }

            LoadCodeDescriptions();
        }

        override public void ResetOriginalValues()
        {
            foreach (PanelRec pr in panelrecs)
            {
                if (pr.tb != null && pr.isDateMask(pr.editmask) && !string.IsNullOrEmpty(pr.tb.Text))
                {
                    var time = GetTimeValueIfExist(pr);

                    if (IsDMYFormat(pr.editmask))
                    {
                        if (pr.editmask.ToUpper().Contains(":SS") || pr.editmask.ToUpper().Contains("HH:MM"))
                            pr.original.Value = ConvertToDateTime(pr.tb.Text, pr.editmask).ToString("dd/MM/yyyy HH:mm:ss");
                        else
                            pr.original.Value = ConvertToDateTime(pr.tb.Text, pr.editmask).ToString("dd/MM/yyyy") + time;

                        pr.tb.Text = pr.original.Value;
                    }
                    else
                        pr.original.Value = pr.tb.Text + time;

                    pr.originalDesc.Value = pr.original.Value;
                }
                else if (pr.tb != null)
                {
                    pr.original.Value = pr.tb.Text;
                    pr.originalDesc.Value = pr.tb.Text;
				}
				else if (pr.ddl != null)
				{
					pr.original.Value = pr.ddl.SelectedValue;
					pr.originalDesc.Value = pr.ddl.SelectedItem.Text;
				}
                else if (pr.rbl != null)
                {
                    if (pr.rbl.SelectedIndex > -1)
                    {
                        pr.original.Value = pr.rbl.SelectedValue;
                        pr.originalDesc.Value = pr.rbl.SelectedItem.Text;
                    }
                }
				else if (pr.chtb != null)
				{
					pr.original.Value = pr.chtb.Text;
					pr.originalDesc.Value = pr.chlabel.Text != "" ? pr.chlabel.Text : pr.chtb.Text;
				}
				else if (pr.chComboBox != null)
				{
					pr.original.Value = pr.chComboBox.SelectedValue;
					pr.originalDesc.Value = pr.chComboBox.SelectedItem.Text != "" ? pr.chComboBox.SelectedItem.Text : pr.chComboBox.SelectedValue;
				}
				else if (pr.chFlexBox != null)
				{
					pr.original.Value = pr.chFlexBox.SelectedValue;
					pr.originalDesc.Value = pr.chFlexBox.SelectedText != "" ? pr.chFlexBox.SelectedText : pr.chFlexBox.GetDescription();
				}
				else if (pr.chMultiLookup != null)
				{
					pr.original.Value = pr.chMultiLookup.GetText();
					string desc = string.Join(",", pr.chMultiLookup.GetSelectionDescription().ToArray());
					pr.originalDesc.Value = desc.Trim() != "" ? desc : pr.chMultiLookup.GetText();
				}
				else if (pr.cb != null)
				{
					pr.original.Value = pr.cb.Checked ? "T" : "F";
					pr.originalDesc.Value = pr.cb.Checked ? "True" : "False";
				}
            }

            SetDBPanelChildRequired();
            SetDisplayFormulaFieldValue();
        }

        protected void LoadRecord()
        {
            // This procedure will get the data form the table

            if (_PLCConnectionString == "")
                return;
            if (this.PLCPanelName == "")
                return;

            string sqlstr = generatesql();

            if ((sqlstr == "") || (sqlstr == null))
            {
                return;
            }

            BlankRecord();

            PLCQuery qryLoad = new PLCQuery();
            qryLoad.SQL = sqlstr;
            qryLoad.Open();
            if (qryLoad.IsEmpty())
                return;

            string fldName;

            foreach (PanelRec pr in panelrecs)
            {
                fldName = pr.fldname.Replace("\"", "");

                pr.original.Value = qryLoad.FieldByName(pr.fldname);

                if (pr.tb != null && !string.IsNullOrEmpty(qryLoad.FieldByName(fldName)) && pr.isDateMask(pr.editmask))
                {
                    var time = string.Empty;
                    var date = ConvertToDateTimeFromDB(qryLoad.FieldByName(fldName));
                    if (pr.time != null)
                    {
                        time = date.ToString("HH:mm");
                        pr.time.Text = time;
                        time = " " + time;
                    }

                    if (IsDMYFormat(pr.editmask))
                    {
                        if (pr.editmask.ToUpper().Contains(":SS") || pr.editmask.ToUpper().Contains("HH:MM"))
                        {
                            pr.original.Value = ConvertToDateTimeFromDB(qryLoad.FieldByName(fldName)).ToString("dd/MM/yyyy HH:mm:ss");
                            pr.tb.Text = pr.original.Value;
                        }
                        else
                        {
                            pr.original.Value = ConvertToDateTimeFromDB(qryLoad.FieldByName(fldName)).ToString("dd/MM/yyyy");
                            pr.tb.Text = pr.original.Value;

                            // if (pr.time != null) // always include time just like when format is MDY
                            pr.original.Value += " " + date.ToString("HH:mm:ss");
                        }
                    }
                    else if (pr.time != null)
                    {
                        pr.tb.Text = date.ToString("MM/dd/yyyy");
                    }
                    else
                    {
                        // DATETIME(MSSQL) and DATE(ORCL) returns the values as M/d/yyyy HH:mm:ss tt
                        // But in DATE(MSSQL) it is returned as yyyy-MM-dd since it is not identified as date
                        pr.tb.Text = date.ToString("M/d/yyyy HH:mm:ss tt");
                    }
                    pr.originalDesc.Value = pr.tb.Text + time;
                } 
                else if (pr.tb != null)
                {
                    if (!string.IsNullOrEmpty(pr.DisplayFormula) && pr.tb.Attributes["FORMULAFIELDVALUE"] != null)
                    {
                            pr.tb.Attributes["FORMULAFIELDVALUE"] = qryLoad.FieldByName(fldName);
                    }
                    else
                    {
                        if (!(HandleIfNumberMask(pr, qryLoad)))
                            pr.tb.Text = qryLoad.FieldByName(fldName);

                        pr.originalDesc.Value = pr.tb.Text;
                    }
                }

                if (pr.ddl != null)
                {
                    if (IsNewRecord)
                    {
						if (pr.ddl.Items.FindByValue(pr.defaultValue) != null)
						{
							pr.ddl.SelectedValue = pr.defaultValue;
							pr.originalDesc.Value = pr.ddl.SelectedItem.Text;
						}
                    }
                    else
                    {
						if (pr.ddl.Items.FindByValue(qryLoad.FieldByName(fldName)) != null)
						{
							pr.ddl.SelectedValue = qryLoad.FieldByName(fldName);
							pr.originalDesc.Value = pr.ddl.SelectedItem.Text;
						}
                    }
                }

                if (pr.rbl != null)
                {
                    string value = qryLoad.FieldByName(fldName);
                    if (pr.rbl.Items.FindByValue(value) != null)
                    {
                        pr.rbl.SelectedValue = value;
                        pr.originalDesc.Value = pr.rbl.SelectedItem.Text;
                    }
                }
                //if (pr.codelbl != null)
                //pr.codelbl.CssClass = "'<%# css_plcdbpanel %>'";

                if (pr.chtb != null)
                {
                    string fieldName = fldName;

                    CodeHeadSetValue(pr, qryLoad.FieldByName(fieldName));
					pr.originalDesc.Value = pr.chlabel.Text != "" ? pr.chlabel.Text : qryLoad.FieldByName(fieldName);
                }
                else if (pr.chComboBox != null)
                {
                    CodeHeadSetValue(pr, qryLoad.FieldByName(fldName));
					pr.originalDesc.Value = pr.chComboBox.SelectedItem.Text != "" ? pr.chComboBox.SelectedItem.Text : qryLoad.FieldByName(fldName);
                }
                else if (pr.chFlexBox != null)
                {
                    CodeHeadSetValue(pr, qryLoad.FieldByName(fldName));
					pr.originalDesc.Value = pr.chFlexBox.SelectedText != "" ? pr.chFlexBox.SelectedText : pr.chFlexBox.GetDescription(qryLoad.FieldByName(fldName));
                }
                else if (pr.chMultiLookup != null)
                {
                    pr.chMultiLookup.SetText(qryLoad.FieldByName(fldName));
					string desc = string.Join(",", pr.chMultiLookup.GetSelectionDescription().ToArray());
					pr.originalDesc.Value = desc.Trim() != "" ? desc : pr.chMultiLookup.GetText();
                }
                else if (pr.chMultipickAc != null)
                {
                    pr.chMultipickAc.SetText(qryLoad.FieldByName(fldName));
                    string desc = string.Join(",", pr.chMultipickAc.GetSelectionDescription().ToArray());
                    pr.originalDesc.Value = desc.Trim() != "" ? desc : pr.chMultipickAc.GetSelectedCodes();
                }

                if (pr.editmask == "CB" || pr.editmask == "CB-R")
                {
                    if (pr.cb != null)
                    {
                        if (qryLoad.FieldByName(fldName) == "T")
                            pr.cb.Checked = true;
                        else
                            pr.cb.Checked = false;
						pr.originalDesc.Value = pr.cb.Checked ? "True" : "False";
                    }
                }
            }

            LoadCodeDescriptions();
            SetDBPanelChildRequired();
            SetDisplayFormulaFieldValue();
        }

        //PanelRec user for iterating through controls
        public class PanelRec
        {
            public string tblname;
            public string prompt;
            public string fldname;
            public string defaultValue;
            public string editmask;
            public string codetable;
            public string fldtype;
            public decimal fldlen;
            public decimal fldWidth;
            public bool hideField;
            public decimal memolines;
            public string codecondition;
            public string required;
            public string supplementLink;
            public DropDownList ddl;
            public RadioButtonList rbl;
            public TextBox tb;
            public HiddenField original;
			public HiddenField originalDesc;
			public ImageButton btn;
            public Literal lit;
            public TextBox codelbl;
            public Label ErrMsg;
            public Label StatusMsg;
            public CalendarExtender calendar;
            public TextBox time;

            public CodeHead codeHeadDlg;

            public CodeMultiPick chMultiLookup;
            public CodeMultiPickAC chMultipickAc;
            public TextBox chtb;
            public ImageButton chib;
            public Label chlabel;
            public UpdateProgress chupdateprog;
            public TextBox chtxtSearch;
            public Button chbtnSearch;
            public ImageButton chbtnRefresh;
            public CheckBox chActive;
            public AjaxControlToolkit.ModalPopupExtender chmpe;
            public SqlDataSource chSqlDataSource;
            public Panel chPnlGrid;
            public GridView chGridView;
            public Button chOkButton;
            public Button chCancelButton;
            //public Button chTabPostBackButton;
            public string _codetable;
            public string _code;
            public string _desc;
            public string _codedesc;
            public string attribtype;
            public string BumpNextFieldUp;
            public TextBox DescCtrl;
            public CheckBox cb;
            public MaskedEditExtender mee;
            //public Button chSelectPostBackButton;
            public HiddenField chHdnPostBack;
            public string PostBack;
            public bool AllowEdit;
            public bool AllowAdd;
            public TADropDown chComboBox;
            public FlexBox chFlexBox;
            public string chSortOrder;
            public UpdatePanel lookupUpdatePanel = null;
            //public HiddenField hdnKey;
            public Signature sig;

            public string customreportsType;

            public string duplicateValues;
            public bool uniqueConstraint = false;

            public string mandatoryOptionalLabCodes;

            public string mandatoryBase;
            public string addBase;
            public string editBase;
            public bool usesLikeSearch;
            public bool usesAutoWildCardSearch;

            public string RequiredMessage;

            public string AutoFillTargets;
            public string AutoFillQuery;

            public string DisplayFormula;

            public bool validateOnSave;

            internal bool NoFutureDates;
            internal bool NoPastDates;
            public string TimeDateField;

            public CheckBox chkNotFilter;
            public bool usesNullBlankOpt;

            public PanelRec(string tblname, string prompt, string fldname, string editmask, string codetable, decimal fldlen, string required, decimal memolines, string codecondition, string sBumpNextFieldUp, string defaultValue, string sPostBack, bool allowEdit, int fldWidth, bool hideField, string customreportsType, string supplementLink, bool uniqueConstraint, string mandatoryOptionalLabCodes, string mandatoryBase, bool usesLikeSearch, string requiredMessage, string autoFillTargets, string autoFillQuery, string displayFormula, bool validateOnSave, string timeDateField, bool allowAdd/*, string addBase, string editBase, bool usesAutoWildCardSearch*/)
            {
                this.ddl = null;
                this.rbl = null;
                this.tb = null;
                this.time = null;
                this.original = new HiddenField();
				this.originalDesc = new HiddenField();
				this.btn = null;
                this.lit = null;
                this.codelbl = null;
                this.ErrMsg = null;
                this.DescCtrl = null;
                this.cb = null;
                this.mee = null;
                this.chComboBox = null;
                this.chFlexBox = null;
                //this.hdnKey = null;
                this.codeHeadDlg = null;
                this.sig = null;
                this.chkNotFilter = null;

                this.tblname = tblname;
                this.prompt = prompt;
                this.fldname = fldname;
                this.defaultValue = defaultValue;
                this.editmask = editmask;
                this.codetable = codetable;
                this.fldlen = fldlen;
                this.memolines = memolines;
                this.codecondition = codecondition;
                this.supplementLink = supplementLink;
                this.fldtype = "C";
                this.required = required;

                if (isDateMask(editmask))
                    this.fldtype = "D";
                if (isDateTimeMask(editmask)) 
                    this.fldtype = "DT";
                if (editmask == "99:99")
                    this.fldtype = "T";
                if (editmask == "hh:mm")
                    this.fldtype = "T";
                if (editmask == "HH:MM")
                    this.fldtype = "T";
                if (editmask == "99:99:99")
                    this.fldtype = "T";
                if (editmask == "hh:mm:ss")
                    this.fldtype = "T";
                if (editmask == "HH:MM:SS")
                    this.fldtype = "T";
                if (editmask == "EMAILADDR")
                    this.fldtype = "EA";
                this.BumpNextFieldUp = sBumpNextFieldUp;
                this.PostBack = sPostBack;
                this.AllowEdit = allowEdit;
                this.AllowAdd = allowAdd;
                this.fldWidth = fldWidth;
                this.hideField = hideField;

                this.customreportsType = customreportsType;
                this.uniqueConstraint = uniqueConstraint;

                this.mandatoryOptionalLabCodes = mandatoryOptionalLabCodes;

                this.mandatoryBase = mandatoryBase;
                this.addBase = addBase;
                this.editBase = editBase;
                this.usesLikeSearch = usesLikeSearch;
                this.usesAutoWildCardSearch = usesAutoWildCardSearch;

                this.RequiredMessage = requiredMessage.Trim();

                this.AutoFillTargets = autoFillTargets;
                this.AutoFillQuery = autoFillQuery;

                this.DisplayFormula = displayFormula;

                this.validateOnSave = validateOnSave;

                this.TimeDateField = timeDateField;
                this.usesNullBlankOpt = usesNullBlankOpt;
            }

            public string CodeTablePrimaryKeyField
            {
                get
                {
                    return this._code;
                }
            }

            public string CodeTablePrimaryKeyData
            {
                get
                {
                    return this.chtb.Text;
                }
            }

            public Boolean isDateMask(string dtemask)
            {
                Boolean hasmonth = dtemask.ToUpper().Contains("MM");
                Boolean hasday = dtemask.ToUpper().Contains("DD");
                Boolean hasyear = dtemask.ToUpper().Contains("YY");
                Boolean hasnumbers = dtemask.ToUpper().Contains("99/99/9999");
                return ((hasmonth && hasday && hasyear) || hasnumbers);
            }

            public bool isDateTimeMask(string mask)
            {
                bool isTimeMask = mask.ToUpper().Contains("HH:MM") || mask.ToUpper().Contains("99:99");
                return isTimeMask && isDateMask(mask);
            }

            public bool isNumberMask(string mask)
            {
                bool isNumberMask = Regex.IsMatch(mask, @"^\d+\.?\d*$");
                return isNumberMask;
            }

            // Return client-side ID of panelrec control.
            public string GetClientControlID()
            {
                string clientID = "";

                if (this.tb != null)
                    clientID = this.tb.ClientID;
                else if (this.ddl != null)
                    clientID = this.ddl.ClientID;
                else if (this.rbl != null)
                    clientID = this.rbl.ClientID;
                else if (this.cb != null)
                    clientID = this.cb.ClientID;
                else if (this.chMultiLookup != null)
                    clientID = this.chMultiLookup.ClientID;
                else if (this._codetable != "")
                {
                    if (this.chtb != null)
                        clientID = this.chtb.ClientID;
                    else if (this.chComboBox != null)
                        clientID = this.chComboBox.ClientID;
                    else if (this.chFlexBox != null)
                        clientID = this.chFlexBox.ClientID;
                    else if (this.codeHeadDlg != null)
                        clientID = this.codeHeadDlg.CodeTextBoxClientID;
                }

                return clientID;
            }

            public string GetRequiredMessage()
            {
                return string.IsNullOrEmpty(this.RequiredMessage) ?
                    "  " + this.prompt + " is required" :
                    this.RequiredMessage;
            }
        }

        public List<string> GetFieldNames()
        {
            List<string> fieldNames = new List<string>();

            foreach (PanelRec pr in panelrecs)
            {
                fieldNames.Add(pr.fldname);
            }

            return fieldNames;
        }

        public string GetFieldDesc(string fieldName)
        {
            foreach (PanelRec pr in panelrecs)
            {
                if (pr.fldname.ToUpper() == fieldName.ToUpper())
                {
					if (pr.tb != null)
						return pr.tb.Text + GetTimeValueIfExist(pr);
					if (pr.ddl != null)
						return pr.ddl.SelectedItem.Text;
                    if (pr.rbl != null)
                    {
                        if(pr.rbl.SelectedIndex > -1)
                            return pr.rbl.SelectedItem.Text;
                    }
					if (pr.chtb != null)
						return pr.chlabel.Text != "" ? pr.chlabel.Text : pr.chtb.Text;
					if (pr.chComboBox != null)
						return pr.chComboBox.SelectedItem.Text != "" ? pr.chComboBox.SelectedItem.Text : pr.chComboBox.SelectedValue;
                    if (pr.chFlexBox != null)
						return pr.chFlexBox.SelectedText != "" ? pr.chFlexBox.SelectedText : pr.chFlexBox.SelectedValue;
                    if (pr.chMultiLookup != null)
                        return string.Join(",", pr.chMultiLookup.GetSelectionDescription().ToArray());
						//return pr.chMultiLookup.GetText();
					if (pr.codeHeadDlg != null)
                        return pr.codeHeadDlg.Description;
					if (pr.cb != null)
						return pr.cb.Checked ? "True" : "False";
                }
            }
            return "";
        }

        public string GetFieldDesc(string tablename, string fieldName)
        {
            foreach (PanelRec pr in panelrecs)
            {
                if ((pr.tblname.ToUpper() == tablename.ToUpper()) && (pr.fldname.ToUpper() == fieldName.ToUpper()))
                {
					if (pr.tb != null)
                        return pr.tb.Text + GetTimeValueIfExist(pr);
					if (pr.ddl != null)
						return pr.ddl.SelectedItem.Text;
                    if (pr.rbl != null)
                    {
                        if(pr.rbl.SelectedIndex > -1)
                            return pr.rbl.SelectedItem.Text;
                    }
					if (pr.chtb != null)
						return pr.chlabel.Text != "" ? pr.chlabel.Text : pr.chtb.Text;
					if (pr.chComboBox != null)
						return pr.chComboBox.SelectedItem.Text != "" ? pr.chComboBox.SelectedItem.Text : pr.chComboBox.SelectedValue;
					if (pr.chFlexBox != null)
						return pr.chFlexBox.SelectedText != "" ? pr.chFlexBox.SelectedText : pr.chFlexBox.SelectedValue;
					if (pr.chMultiLookup != null)
						return string.Join(",", pr.chMultiLookup.GetSelectionDescription().ToArray());
						//return pr.chMultiLookup.GetText();
					if (pr.codeHeadDlg != null)
						return pr.codeHeadDlg.Description;
					if (pr.cb != null)
						return pr.cb.Checked ? "True" : "False";
                }
            }
            return "";
        }

        /// <summary>
        /// Get Panel Rec inside the PanelRec list by using Field Name
        /// </summary>
        /// <param name="fieldName">Value pertains to TV_DBPANEL FIELD_NAME</param>
        /// <returns></returns>
        public PanelRec GetPanelRecByFieldName(string fieldName)
        {
            PanelRec pr =
                panelrecs.Find(
                    delegate(PanelRec pRec)
                    {
                        return pRec.fldname.ToUpper() == fieldName.ToUpper();
                    }
                );

            if (pr != null)
            {
                return pr;
            }
            else
            {
                return null;
            }
        }



        public PanelRec GetPanelRecByFieldName(string fieldName, int startIndex)
        {
            PanelRec pr = null;

            int panelRecIndex =
                panelrecs.FindIndex(startIndex, 1,
                    delegate (PanelRec pRec)
                    {
                        return pRec.fldname.ToUpper() == fieldName.ToUpper();
                    }
                );

            if (panelRecIndex > -1)
            {
                pr = panelrecs[panelRecIndex];
            }

            if (pr != null)
            {
                return pr;
            }
            else
            {
                return null;
            }
        }




        public string GetFieldTableName(string fieldName)
        {
            return GetPanelRecByFieldName(fieldName).tblname;
        }

        public string GetFieldPrompt(string fieldName)
        {
            return GetPanelRecByFieldName(fieldName).prompt;
        }


        public string GetFieldPrompt(string fieldName, int startIndex)
        {
            
            return GetPanelRecByFieldName(fieldName, startIndex).prompt;
        }


        public string GetFieldMask(string fieldName)
        {
            if (GetPanelRecByFieldName(fieldName).mee != null)
                return GetPanelRecByFieldName(fieldName).mee.Mask;

            return GetPanelRecByFieldName(fieldName).editmask;
        }

        public string GetFieldCodeTable(string fieldName)
        {
            return GetPanelRecByFieldName(fieldName).codetable;
        }

        public string GetFieldCodeCondition(string fieldName)
        {
            return GetPanelRecByFieldName(fieldName).codecondition;
        }


        public bool UseLikeSearch(string fieldName)
            {
            return GetPanelRecByFieldName(fieldName).usesLikeSearch;
            }



        private string StripMask(string s)
        {
            int idx = 0;

            if (!String.IsNullOrEmpty(s))
            {
                idx = s.Length - 1;

                while ((idx >= 0) && (s.Substring(idx, 1) == "_"))
                {
                    s = s.Substring(0, idx);
                    idx = s.Length - 1;
                }
            }

            return s;

        }

        private string cleanit(string s)
        {

            if (s == "")
                return s;

            string holdstr = "<--!.#.@.$.#.%.->";
            string singlequote = "'";
            string escapedsinglequote = "''";
            s = s.Replace(singlequote, holdstr);
            s = s.Replace(holdstr, escapedsinglequote);

            s = StripMask(s);

            return s;
        }

        // DoAdd
        override public Boolean DoAdd()
        {
            base.DoAdd();

            PLCDBPanelGetNewRecordEventArgs TheArg = new PLCDBPanelGetNewRecordEventArgs("");

            try
            {
                onplcdbpanelgetnewrecord(TheArg);
            }
            catch (Exception ex)
            {

                PLCSessionVars PLCSessionVars1 = new PLCSessionVars();
                PLCSessionVars1.ClearError();
                PLCSessionVars1.PLCErrorURL = "*";
                PLCSessionVars1.PLCErrorProc = "PLCDBPANEL.DOADD(" + PLCPanelName + ")";
                PLCSessionVars1.PLCErrorSQL = PLCWhereClause;
                PLCSessionVars1.PLCErrorMessage = "Exception: " + ex.Message;
                PLCSessionVars1.SaveError();
                PLCSessionVars1.Redirect("~/PLCWebCommon/Error.aspx");

                return false;
            }

            if (TheArg.newWhereClause == "")
            {
                return false;
            }

            PLCWhereClause = TheArg.newWhereClause;
            SetRecordAddMode();
            LoadRecord();
            EditMode();

            if (!PLCDisableDefaultValue)
                SetPanelDefaultValues();

            SetRestrictionsBasedOnAddEditBase("ADD");

            return true;

        }

        // DoEdit
        override public Boolean DoEdit()
        {
            base.DoEdit();
            EditMode();
            SetRestrictionsBasedOnAddEditBase("EDIT");
            return true;
        }

        override public Boolean DoSave()
        {
            base.DoSave();

            DoSaveRecord(null);

            if (IsNewRecord)
                ClearRecordAddMode();

            return true;

        }


        // DoSave
        override public Boolean DoSave(PLCQuery qry)
        {

            StartSavingMode();

            try 
            {
                base.DoSave();

                DoSaveRecord(qry);

                if (IsNewRecord)
                    ClearRecordAddMode();

                StopSavingMode();

            }
            catch 
            {
                StopSavingMode();
            }

            return true;

        }

        // DoCancel
        override public Boolean DoCancel()
        {
            base.DoCancel();

            if (IsNewRecord)
            {
                //ClearRecordAddMode();
                DoDeleteRecord();
                BlankRecord();
                ClearRecordAddMode();
            }
            else
            {
                LoadRecord();
                BrowseMode();
            }

            return true;
        }

        // DoDelete
        override public Boolean DoDelete()
        {
            base.DoDelete();

            DoDeleteRecord();

            return true;
        }

        private int MakeInt(string s)
        {
            try
            {
                if (s == null)
                    return 0;
                if (s == "")
                    return 0;
                return Convert.ToInt32(s);
            }
            catch
            {
                return 0;
            }
        }

        public void ReDrawControls()
        {
            this.Controls.Clear();

            dynactrl_modal();
        }

        private bool HasEditAccess(string editAccessCode, PLCSessionVars session)
        {
            if (editAccessCode.Trim() == "")
                return true;


            List<string> analystAccess =  PLCSession.GetGlobalAnalystAccessCodes(PLCSession.PLCGlobalAnalyst);

            if (analystAccess.Contains(editAccessCode) ) return true;

            if (session.CheckUserOption(editAccessCode))
                return true;

            return false;
        }

        enum ControlType
        {
            Popup = 1,
            ComboBox = 2,
            FlexBox = 3,
            CodeHeadDialog = 4
        }

        [Serializable]
        private class CodeHeadAttributes
        {
            public ControlType CodeHeadType
            {
                get;
                set;
            }
            public int Width
            {
                get;
                set;
            }
            public string SortOrder
            {
                get;
                set;
            }
            public string DescriptionFormat
            {
                get;
                set;
            }
            public string DescriptionSeparator
            {
                get;
                set;
            }
        }

        private CodeHeadAttributes CodeHeadConfigurations(string tableName)
        {

            PLCSession.WriteDebug("CodeHeadConfigurations in, tablename:" + tableName, true);

            if (ViewState["CH_" + tableName] == null)
            {
                CodeHeadAttributes attributes = new CodeHeadAttributes();

                //default is popup type
                ControlType codeHeadType = ControlType.Popup;

                if (!string.IsNullOrEmpty(tableName))
                {
                    string codeHeadTableName = tableName;

                    if ((tableName.Substring(0, 3) == "TV_") || (tableName.Substring(0, 3) == "CV_"))
                        codeHeadTableName = tableName.Substring(3);

                    PLCSession.WriteDebug("CodeHeadConfigurations, codeheadtablename:" + codeHeadTableName, true);

                    PLCQuery qryCodeHead = CacheHelper.OpenCachedSql("SELECT COMBO_BOX, SORT_ORDER, CODE_DESC_FORMAT, CODE_DESC_SEPARATOR FROM TV_CODEHEAD WHERE TABLE_NAME = '" + codeHeadTableName + "'", PLCSession.GetMetadataCacheDuration());
                    if (qryCodeHead.IsEmpty())
                    {
                        attributes.SortOrder = "D";
                        attributes.CodeHeadType = ControlType.FlexBox;
                        PLCSession.WriteDebug(codeHeadTableName + " not found in codehead. Using Defaults", true);
                    }
                    else
                    {
                        string comboBoxFieldValue = qryCodeHead.FieldByName("COMBO_BOX").ToUpper();
                    
                        if (comboBoxFieldValue.Length > 1 && comboBoxFieldValue.Contains(":"))
                        {
                            attributes.Width = Convert.ToInt32(comboBoxFieldValue.Substring(comboBoxFieldValue.IndexOf(":") + 1));
                            comboBoxFieldValue = comboBoxFieldValue.Substring(0, 1);
                    
                        }

                        attributes.SortOrder = qryCodeHead.FieldByName("SORT_ORDER");
                    

                        //description format and separator only applies to flexbox
                        attributes.DescriptionFormat = qryCodeHead.FieldByName("CODE_DESC_FORMAT").ToUpper();
                        attributes.DescriptionSeparator = qryCodeHead.FieldByName("CODE_DESC_SEPARATOR");

                        if (comboBoxFieldValue == "T")
                        {
                            codeHeadType = ControlType.ComboBox;
                    
                            
                        }
                        else if (comboBoxFieldValue == "D")
                        {
                            codeHeadType = ControlType.CodeHeadDialog;
                    
                        }
                        else if (comboBoxFieldValue == "X")
                        {
                            codeHeadType = ControlType.FlexBox;
                    
                        }
                        else
                        {
                            codeHeadType = ControlType.Popup;
                    
                        }

                        attributes.CodeHeadType = codeHeadType;
                    }
                }

                ViewState["CH_" + tableName] = attributes;
            }

            return (CodeHeadAttributes)ViewState["CH_" + tableName];
        }

        // Return view equivalent of table. Ex. Given 'ANALYST', returns 'TV_ANALYST'. Given 'TV_ANALYST', return 'TV_ANALYST' (unchanged as it's already a view name.)
        private string GetCodeTableViewName(string theCodeTable)
        {
            if (String.IsNullOrEmpty(theCodeTable))
                return "";

            string viewName;

            if ((theCodeTable.IndexOf("_") < 0) && (theCodeTable.Substring(0, 3).ToUpper() != "TV_"))
                viewName = "TV_" + theCodeTable;
            else
                viewName = theCodeTable;

            return viewName;
        }

        //$$ This was copied from PLCGlobal.cs. Ideally, this should be accessed as a common function, but PLCControls can't access PLCGlobals
        //     as PLCControls is lower in the dependence hierarchy.
        // Truncate picture mask to fit max length. Ex. If pic mask is "99999" and maxlen is 2, return "99".
        private string TruncatePictureMaskToMax(string mask, int maxlen)
        {
            if (mask.Length > maxlen)              // mask is greater than max field length, so truncate mask.
                return mask.Substring(0, maxlen);
            else
                return mask;
        }

        private string GetPromptCssClass(PromptType promptType)
        {
            string cssClass = "";

            switch (promptType)
            {
                case PromptType.Required:
                    cssClass = "required";
                    break;
                case PromptType.Mandatory:
                    cssClass = "mandatory";
                    break;
                default:
                    cssClass = "default";
                    break;
            }

            return cssClass;
        }

        private string GetRequiredFieldsMarkup(PromptType promptType, string editMask)
        {
            if (this.isUsesRequiredFieldsIndicator && ((promptType == PromptType.Required) || (promptType == PromptType.Mandatory)))
            {
                if (editMask == "CheckboxPrompt")
                    return String.Format("<em class='{0}'>*</em>", "cb" + promptType.ToString().ToLower());
                else
                    return "<em>*</em>";
            }
            else
            {
                return "<em>*</em>";
            }
        }

        private Literal CreatePromptMarkup(string promptTxt, string cssStyle, string cssClass, PromptType promptType, string editMask)
        {
            // If edit control is a checkbox, don't show the prompt as the prompt will be displayed to the right of the checkbox.
            // removed editMask == "CB" from the condition to fix the issue with hidden prompt when calling SetFieldAsRequired in requiring DBPanel fields
            if (editMask == "CBDISPLAY")
                cssStyle = "display:none";

            Literal lit = new Literal();

            if (promptTxt.Length > 50 || promptTxt.ToLower().Contains("<br />") || promptTxt.ToLower().Contains("<br/>"))
                lit.Text = String.Format("<table width='100%'><tr><td>{3}</td><td><span style='{0}; float: right;' class='{1} prompt {2}'>{4}</span></td></tr></table>", cssStyle, cssClass, GetPromptCssClass(promptType), promptTxt, GetRequiredFieldsMarkup(promptType, editMask));
            else
                lit.Text = String.Format("<span style='{0}' class='{1} prompt {2}'>{4}{3}</span>", cssStyle, cssClass, GetPromptCssClass(promptType), promptTxt, GetRequiredFieldsMarkup(promptType, editMask));

            return lit;
        }

  
        //Create Dynamic controls
        protected void dynactrl_modal()
        {
            if ((_PLCConnectionString == "") || (_PLCConnectionString == "Provider=;Data Source=;User ID=;Password=;"))
                return;

            Dictionary<object, string> FlexBoxControlsWithParent = new Dictionary<object, string>();
            Dictionary<string, string> MandatoryBases = new Dictionary<string, string>();
            Dictionary<string, string> AddFieldBases = new Dictionary<string, string>();
            Dictionary<string, string> EditFieldBases = new Dictionary<string, string>();
            Dictionary<object, string> SignatureControlsWithName = new Dictionary<object, string>();

            AjaxControlToolkit.MaskedEditExtender mymaskedit;

            TableRow tr = null;
            TableCell tc = null;

            TextBox tb = null;
            DropDownList ddl = null;


            Literal promptlbl = null;

            CheckBox cb = null;

            int ctrlnum = -1;
            int nUPCount = 5;
            LastTabIndex = 0;

            PLCSessionVars sv = new PLCSessionVars();

            // inject javascript block
            //            SetCodeHeadJavaScript();

            Table tbl = new Table();
            tbl.BorderWidth = 0;
            tbl.BorderStyle = BorderStyle.None;
            //    tbl.Font = this.Font;

            PanelRec myprec = null;
            string theEditMask = "";
            bool isDateTime = false;
            string theCodeTable = "";
            int theFldLen = 0;
            int theDisplayWidth = 0;
            string theTblName = "";
            string thePrompt = "";
            string theFldName = "";
            string theRequired = "";
            int theMemoLines = 0;
            string theCodeCondition = "";
            string theSupplementLink = "";
            string theInitialValue = "";
            string theCustom1 = "";
            string theCustom2 = "";
            string AttributeType = "";
            string sBumpNextFieldUp = "";
            bool isBumped = false;
            bool isPacked = false;
            string listOptions = "";
            string defaultValue = "";
            string sPostBack = "";
            bool allowEdit =  EnableClientBehavior;
            bool allowEditDBPanel = false; //If the field EDIT_ACCESS_CODES is blank then allow edit. If there is an AUTHCODE entry in it, then the user must have that AUTHCODE assigned to them in order to allow then to edit. 
            bool allowAdd = false;
            string checkboxLabel = "";
            bool hideDescription = false;
            bool hidePrompt = false;
            bool hideField = false; // AAC (09/13/2010) - Rename to hideField as removeFromScreen serves a different purpose
            bool filterByLabCode = false;
            string masterFieldName = string.Empty;
			bool noFutureDates = false;
            bool noPastDates = false;
            bool usesLikeSearch = false;

            string customreportsType;

            string duplicateValues;

            bool uniqueConstraint = false;
            string mandatoryOptionalLabCodes = string.Empty;
            bool barcodeScan = false;

            string separatorText = string.Empty;
            bool hasSeparator = false;

            string mandatoryBase = string.Empty;
            string addBase = string.Empty;
            string editBase = string.Empty;

            string requiredMessage = string.Empty;

            string autoFillTargets = string.Empty;
            string autoFillQuery = string.Empty;

            string displayFormula = string.Empty;

            bool validateOnSave = false;

            string timeDateField = string.Empty;

            bool allowToggleActiveOnly = false;

            bool usesAutoWildCardSearch = false;

            bool usesNotFilter = false;
            int notFilterCount = 0;
            bool usesNullBlankOption = false;

            bool usesProsecutor = false;

            if (PLCPanelName == "")
                PLCPanelName = "UNKNOWN";

            PLCQuery qryDBPANEL;
            string sqlDBPANEL = "select TABLE_NAME, PROMPT, FIELD_NAME, EDIT_MASK, LENGTH, CODE_TABLE, MANDATORY, MEMO_FIELD_LINES, " +
                "CODE_CONDITION, BUMP_NEXT_FIELD_UP, NO_PROMPT, OPTIONS, DEFAULT_VALUE, POSTBACK, EDIT_ACCESS_CODES, CODE_TABLE_USES_KEY, " +
                "FIELD_DISPLAY_WIDTH, CODE_DESC_FORMAT, CODE_DESC_SEPARATOR, HIDE_DESCRIPTION, REMOVE_FROM_SCREEN, FILTER_BY_LAB_CODE, " +
                "MASTER_FIELD_NAME, SEQUENCE, TAB_ORDER, NO_FUTURE_DATES, NO_PAST_DATES, CUSTOM_REPORTS, FORCE_MASK, SUPPLEMENT_LINK, " +
                "DUPLICATE_VALUES, UNIQUE_CONSTRAINT, MANDATORY_OPTIONAL_LAB_CODES, RETAIN_FOCUS_ON_BARCODE_SCAN, SEPARATOR_TEXT, HAS_SEPARATOR, " +
                "MANDATORY_BASE, LIKE_SEARCH, REQUIRED_FIELD_MESSAGE, AUTOFILL_TARGET, AUTOFILL_QUERY, DISPLAY_FORMULA, VALIDATE_ON_SAVE, " +
                "PROMPT_CODE, TIME_DATE_FIELD from TV_DBPANEL where PANEL_NAME = '" +
                PLCPanelName + "'" + (PLCDbPanelQuery != "" ? PLCDbPanelQuery : "") + " AND ((REMOVE_FROM_SCREEN <> 'T') OR (REMOVE_FROM_SCREEN Is NULL)) Order By SEQUENCE";
            if (!this.useCaching)
            {
                // Not cached, initialize DBPanel field list.
                qryDBPANEL = new PLCQuery();
                qryDBPANEL.SQL = sqlDBPANEL;
                qryDBPANEL.Open();
            }
            else
            {
                // Return cached DBPanel field list.
                qryDBPANEL = CacheHelper.OpenCachedSql(sqlDBPANEL, PLCSession.GetMetadataCacheDuration());
            }

            // PrimaryKey and ROWID constraint needs to be removed before PLCDataTable.Copy() or a constraint exception will occur.
            qryDBPANEL.PLCDataTable.PrimaryKey = null;

            //remove all other constraints
            try
            {
                qryDBPANEL.PLCDataTable.Constraints.Clear();
            }
            catch (Exception ex1)
            {
                PLCSession.WriteDebug("Unable to clear constraints:" + ex1.Message);
            }


            if (qryDBPANEL.PLCDataTable.Columns.IndexOf("ROWID") >= 0)
                qryDBPANEL.PLCDataTable.Columns.Remove("ROWID");

            DataTable fieldlst = qryDBPANEL.PLCDataTable.Copy();

            foreach (DataColumn dc in fieldlst.Columns)
            {

                if (dc.ReadOnly)
                {
                    PLCSession.WriteDebug(dc.ColumnName + " is readonly -> attempting to reset");
                    try
                    {
                        dc.ReadOnly = false;
                    }
                    catch (Exception ex2)
                    {
                        PLCSession.WriteDebug("***********Unable to reset readonly property to false:" + ex2.Message);
                    }
                }
            }

            int colSpan = GetColSpanForPackedField(fieldlst);

            DataColumn DC;
            if (!fieldlst.Columns.Contains("INITIAL_VALUE"))
            {
                DC = new DataColumn();
                DC.DataType = System.Type.GetType("System.String");
                DC.ColumnName = "INITIAL_VALUE";
                fieldlst.Columns.Add(DC);
            }

            if (!fieldlst.Columns.Contains("CUSTOM_1"))
            {
                DC = new DataColumn();
                DC.DataType = System.Type.GetType("System.String");
                DC.ColumnName = "CUSTOM_1";
                fieldlst.Columns.Add(DC);
            }

            if (!fieldlst.Columns.Contains("CUSTOM_2"))
            {
                DC = new DataColumn();
                DC.DataType = System.Type.GetType("System.String");
                DC.ColumnName = "CUSTOM_2";
                fieldlst.Columns.Add(DC);
            }

            if (!fieldlst.Columns.Contains("ATTRIBUTE_TYPE"))
            {
                DC = new DataColumn();
                DC.DataType = System.Type.GetType("System.String");
                DC.ColumnName = "ATTRIBUTE_TYPE";
                fieldlst.Columns.Add(DC);
            }

            // Set the list of tab index numbers per dbpanel field.
            // Use TAB_ORDER column to get tab index. If TAB_ORDER is not specified, default to SEQUENCE column.
            List<short> rowSequenceNumbers = new List<short>();
            foreach (DataRow dr in fieldlst.Rows)
            {
                short tabIndex;
                if (dr["TAB_ORDER"].ToString() != "")
                    tabIndex = Convert.ToInt16(dr["TAB_ORDER"]);
                else
                    tabIndex = Convert.ToInt16(dr["SEQUENCE"]);

                rowSequenceNumbers.Add(tabIndex);

                // HighestTabIndex is used to maintain the last control in the tab order 
                // so that other objects can add newer controls right after the dbpanel controls.
                if (tabIndex > this.HighestTabIndex)
                    this.HighestTabIndex = tabIndex;
                
                //if (dr["NOT_FILTER"].ToString().ToUpper() == "T")
                    notFilterCount++;
            }

            // Remove non-nullable SEQUENCE column as it causes problems in CustomFields delegates that add dbpanel fields on the fly.
            if (fieldlst.Columns.Contains("SEQUENCE"))
                fieldlst.Columns.Remove("SEQUENCE");

            PLCDBPanelAddCustomFieldsEventArgs e = new PLCDBPanelAddCustomFieldsEventArgs(fieldlst);
            onplcdbpaneladdcustomfields(e);

            // PLCDBPanel is initialized.

            SetDBPanelProperties();

            int currentRow = 0;
            short tabCount = -1;
            LastTabIndex = -1;
            foreach (DataRow dr in fieldlst.Rows)
            {
                isBumped = (sBumpNextFieldUp == "T");
                isPacked = (sBumpNextFieldUp == "P");


                ctrlnum = ctrlnum + 1;
                
                // Set tab index for the dbpanel control.
                if (currentRow < rowSequenceNumbers.Count)
                    LastTabIndex = rowSequenceNumbers[currentRow];
                else
                    LastTabIndex++;

                tabCount++;
                currentRow++;

                theDisplayWidth = MakeInt(dr["FIELD_DISPLAY_WIDTH"].ToString());
                theTblName = dr["TABLE_NAME"].ToString();
                theFldName = dr["FIELD_NAME"].ToString();

                thePrompt = PLCSession.GetSysPrompt(dr["PROMPT_CODE"].ToString(), dr["PROMPT"].ToString());
                theSupplementLink = dr["SUPPLEMENT_LINK"].ToString();

                //addBase = dr["ADD_BASE"].ToString();
                //editBase = dr["EDIT_BASE"].ToString();

                checkboxLabel = thePrompt;
                theEditMask = dr["EDIT_MASK"].ToString();
                isDateTime = theEditMask.StartsWith("DATETIME");
                if (isDateTime)
                {
                    theEditMask = theEditMask == "DATETIMEDMY"
                        ? "dd/mm/yyyy"
                        : "mm/dd/yyyy";
                }

                theFldLen = MakeInt(dr["LENGTH"].ToString());

                // If picture mask goes beyond the field length, truncate it to match the max field length.
                if ((theFldLen > 0) && !this.reservedEditMasks.Contains(theEditMask))   // Don't truncate for reserved edit masks such as 'CB', 'COMBOLIST', etc.
                    theEditMask = TruncatePictureMaskToMax(theEditMask, theFldLen);

                if (theFldLen < 1)
                    theFldLen = 1;

                if (string.IsNullOrEmpty(theEditMask) && dr["FORCE_MASK"].Equals("T"))
                    for (int maskCount = 0; maskCount < theFldLen; maskCount++)
                        theEditMask += "X";

                theCodeTable = GetCodeTableViewName(dr["CODE_TABLE"].ToString());
                theRequired = dr["MANDATORY"].ToString();
                if (theRequired == "")
                    theRequired = "F";

                // Override the required / not required state if it was set previously 
                //   via AddRequiredFieldOverride().
                theRequired = OverrideRequiredFieldIfSpecified(theFldName, theRequired);


                /* Override TV_DBPANEL.MANDATORY_BASE if MANDATORY is T or M*/
                mandatoryBase = dr["MANDATORY_BASE"].ToString();
                if (mandatoryBase == "T")
                    theRequired = "T";
                else if (!string.IsNullOrEmpty(mandatoryBase))
                {
                    if (theRequired == "F")
                        MandatoryBases.Add(theFldName, mandatoryBase);
                    else //this is for the scenario where MANDATORY is M or T and MANDATORY_BASE is not empty. MANDATORY_BASE will always be overridden by MANDATORY
                        AddMandatoryChildFieldOverride(theFldName);
                }

                // ADD_BASE
                //if (!string.IsNullOrEmpty(addBase))
                //    AddFieldBases.Add(theFldName, addBase);

                // EDIT_BASE
                //if (!string.IsNullOrEmpty(editBase))
                //    EditFieldBases.Add(theFldName, editBase);

                /* override required field and set it to 'F' if logged on user's lab code 
                  is defined in TV_DBPANEL.MANDATORY_OPTIONAL_LAB_CODES */
                mandatoryOptionalLabCodes = dr["MANDATORY_OPTIONAL_LAB_CODES"].ToString().Replace(" ", "").ToUpper();
                OverrideRequiredFieldBasedOnLabCodes(mandatoryOptionalLabCodes, ref theRequired);


                PromptType promptType;
                if (theRequired == "T")
                    promptType = PromptType.Required;
                else if (theRequired == "M")
                    promptType = PromptType.Mandatory;
                else
                    promptType = PromptType.Standard;

                theMemoLines = MakeInt(dr["MEMO_FIELD_LINES"].ToString());


                if (theCodeTable == "TV_ANALYST" && !IsSearchPanel)
                    theCodeCondition = "(ACCOUNT_DISABLED <> 'T' OR ACCOUNT_DISABLED IS NULL)" +
                        (dr["CODE_CONDITION"].ToString() != "" ? " AND " + dr["CODE_CONDITION"].ToString() : "");
            
                else
                    theCodeCondition = dr["CODE_CONDITION"].ToString();

                filterByLabCode = dr["FILTER_BY_LAB_CODE"].ToString() == "T";


                string addChar = (PLCSession.PLCDatabaseServer == "MSSQL" ? "+" : "||");

                if (filterByLabCode)
                {
                    string labCodeCondition = "',' " + addChar + " LAB_CODE_FILTER " + addChar + "',' LIKE '%," + 
                        PLCSession.PLCGlobalLabCode + ",%'";
                    if (theCodeTable != string.Empty)
                        labCodeCondition += " OR (SELECT COUNT(LAB_CODE_FILTER) FROM " + theCodeTable +
                                            " WHERE ',' " + addChar + " LAB_CODE_FILTER " + addChar + "',' LIKE '%," + 
                                            PLCSession.PLCGlobalLabCode + ",%') = 0";
                    theCodeCondition = (string.IsNullOrEmpty(theCodeCondition)) ? "(" + labCodeCondition + ")"
                        : theCodeCondition + " AND (" + labCodeCondition + ")";
                }

                masterFieldName = dr["MASTER_FIELD_NAME"].ToString();

                if (masterFieldName != string.Empty)
                {
                    string masterCodeCondition = "  ( (MASTER_FIELD_FILTER IS NULL)  OR  ( ',' " + addChar + " MASTER_FIELD_FILTER " + addChar + " ',' LIKE '%,[" + masterFieldName + "],%') ) ";
                    theCodeCondition = (string.IsNullOrEmpty(theCodeCondition)) ? "(" + masterCodeCondition + ")"
                        : theCodeCondition + " AND (" + masterCodeCondition + ")";
                }

                theInitialValue = dr["INITIAL_VALUE"].ToString();
                theCustom1 = dr["CUSTOM_1"].ToString();
                theCustom2 = dr["CUSTOM_2"].ToString();
                AttributeType = dr["ATTRIBUTE_TYPE"].ToString();
                sBumpNextFieldUp = dr["BUMP_NEXT_FIELD_UP"].ToString();
                listOptions = dr["OPTIONS"].ToString();
                defaultValue = dr["DEFAULT_VALUE"].ToString();
                sPostBack = dr["POSTBACK"].ToString();
                allowEditDBPanel = HasEditAccess(dr["EDIT_ACCESS_CODES"].ToString(), sv);              
                //allowAdd = HasEditAccess(dr["ADD_ACCESS_CODES"].ToString(), sv);
                string codeDescFormat = dr["CODE_DESC_FORMAT"].ToString().Trim(); //flexbox text format
                string codeDescSeparator = dr["CODE_DESC_SEPARATOR"].ToString().Trim(); //flexbox separator for code and description

                hideDescription = (dr["HIDE_DESCRIPTION"].ToString() == "T");
                hidePrompt = (dr["NO_PROMPT"].ToString() == "T");
                hideField = (dr["REMOVE_FROM_SCREEN"].ToString() == "H");
				noFutureDates = dr["NO_FUTURE_DATES"].ToString() == "T";
                noPastDates = dr["NO_PAST_DATES"].ToString() == "T";

                customreportsType = dr["CUSTOM_REPORTS"].ToString();

                duplicateValues = dr["DUPLICATE_VALUES"].ToString();

                uniqueConstraint = dr["UNIQUE_CONSTRAINT"].ToString() == "T";
                barcodeScan = dr["RETAIN_FOCUS_ON_BARCODE_SCAN"].ToString() == "T";

                separatorText = dr["SEPARATOR_TEXT"].ToString();
                hasSeparator = dr["HAS_SEPARATOR"].ToString() == "T";
                usesLikeSearch = dr["LIKE_SEARCH"].ToString() == "T";
                //usesAutoWildCardSearch = dr["AUTO_WILDCARD_SEARCH"].ToString().ToUpper().Equals("T");

                requiredMessage = dr["REQUIRED_FIELD_MESSAGE"].ToString();

                autoFillTargets = dr["AUTOFILL_TARGET"].ToString();
                autoFillQuery = dr["AUTOFILL_QUERY"].ToString();

                displayFormula = IsValidConfigSelectSQL(dr["DISPLAY_FORMULA"].ToString()) ? dr["DISPLAY_FORMULA"].ToString() : "";

                validateOnSave = dr["VALIDATE_ON_SAVE"].ToString().ToUpper() == "T";

                timeDateField = dr["TIME_DATE_FIELD"].ToString().ToUpper();

                //allowToggleActiveOnly = dr["ALLOW_TOGGLE_ACTIVE_ONLY"].ToString().ToUpper() == "T";

                //usesNotFilter = dr["NOT_FILTER"].ToString().ToUpper() == "T";
                //usesNullBlankOption = dr["USES_NULL_BLANK_OPTION"].ToString().ToUpper() == "T";

                //usesProsecutor = dr["ADD_EDIT_ACCESS_PROSECUTOR"].ToString().ToUpper() == "T";

                if (hideField) sBumpNextFieldUp = "";

                if (!EnableClientBehavior)
                    allowEdit = allowEditDBPanel;

                //add/edit access codes will only be referenced if TV_DEPTNAME.PROSECUTOR is T for the selected department
                //supported in configwebuser page on select of dbpanel value
                if (usesProsecutor)
                {
                    allowAdd = allowEdit = allowEditDBPanel=  true; 
                }

                myprec = new PanelRec(theTblName, thePrompt, theFldName, theEditMask, theCodeTable, theFldLen, theRequired, theMemoLines, theCodeCondition, sBumpNextFieldUp, defaultValue, sPostBack, allowEditDBPanel, theDisplayWidth, hideField, customreportsType, theSupplementLink, uniqueConstraint, mandatoryOptionalLabCodes, mandatoryBase, usesLikeSearch, requiredMessage, autoFillTargets, autoFillQuery, displayFormula, validateOnSave, timeDateField, allowAdd/*, addBase, editBase, usesAutoWildCardSearch*/);
                myprec.duplicateValues = duplicateValues;
                panelrecs.Add(myprec);

                myprec.StatusMsg = new Label();
                myprec.StatusMsg.ForeColor = Color.Blue;
                myprec.StatusMsg.Font.Italic = true;
                myprec.StatusMsg.Font.Size = 10;
                myprec.StatusMsg.Visible = false;


                myprec.ErrMsg = new Label();
                myprec.ErrMsg.ForeColor = Color.Red;
                myprec.ErrMsg.Visible = false;


                Panel wrapper = new Panel();
                if (sBumpNextFieldUp == "P")
                {
                    wrapper.Style.Add("float", "left");
                    wrapper.Style.Add("padding-right", "5px");
                }

                if (!string.IsNullOrEmpty(autoFillTargets) && !string.IsNullOrEmpty(autoFillQuery))
                {
                    wrapper.Attributes["AUTOFILLTARGETS"] = autoFillTargets.Replace("%", "");

                    var matches = Regex.Matches(autoFillQuery, "(?:%)(.+?)(?:%)");
                    var autoFillParams = string.Join(",", matches.Cast<Match>().Select(m => m.Result("$1")));
                    wrapper.Attributes["AUTOFILLPARAM"] = autoFillParams;

                    wrapper.Attributes["AUTOFILLTRIGGER"] = this.ID + "|" + theTblName + "." + theFldName;
                }

                int packedWidth = (sBumpNextFieldUp == "P" || isPacked) ? (theDisplayWidth > 0 ? theDisplayWidth : (theFldLen > 1 ? theFldLen : (theEditMask.Length > 0 ? theEditMask.Length : 10)) * 10) + (myprec.isDateMask(theEditMask) ? 20 : 0) : 0;

                if ((isBumped || isPacked) && !hideField)
                {
                    if (isBumped)
                    {
                        TableCell cell = new TableCell();
                        string style = hidePrompt ? "display:none;" : "";
                        promptlbl = CreatePromptMarkup(thePrompt, style, "", promptType, theEditMask);
                        promptlbl.ID = this.ID + "_ltrl" + theFldName + ctrlnum.ToString();

                        if (theEditMask.Equals("CB") || theEditMask.Equals("CB-R"))
                        {
                            promptlbl.Text = "";
                            promptlbl.ID = "";
                        }

                        cell.Controls.Add(promptlbl);
                        tr.Cells.Add(cell);
                        tc = new TableCell();
                    }

                    if (isPacked)
                    {
                        tc.Attributes.Add("colspan", colSpan.ToString());
                        string style = hidePrompt ? "display:none;" : "display:inline-block; vertical-align:" + (theCodeTable == "" ? "5%" : "50%")  + ";";
                        promptlbl = CreatePromptMarkup(thePrompt, style, "packedLabel", promptType, theEditMask);
                        promptlbl.ID = this.ID + "_ltrl" + theFldName + ctrlnum.ToString();

                        if (theEditMask.Equals("CB") || theEditMask.Equals("CB-R"))
                        {
                            promptlbl.Text = "";
                            promptlbl.ID = "";
                        }

                        tc.Width = (int)tc.Width.Value + packedWidth + (thePrompt.Length * 8) + 30; // HTML5 fixed for pack

                        wrapper.Controls.Add(promptlbl);
                    }
                }
                else
                {
                    // Create the table row
                    tr = new TableRow();
                    // Create the cell
                    tc = new TableCell();
                    //tc.Width = Unit.Point(300);
                    tc.BorderWidth = 0;
                    tc.BorderStyle = BorderStyle.None;
                    //  tc.BackColor = Color.Silver;
                    if (sBumpNextFieldUp == "P" && !string.IsNullOrEmpty(theCodeTable))
                        tc.VerticalAlign = VerticalAlign.Top;

                        string style = hidePrompt ? "display:none;" : "";

                    if (theEditMask == "CB")
                    {
                        promptlbl = CreatePromptMarkup(checkboxLabel, "display:inline-block;width:100%;", "", promptType, "CheckboxPrompt");
                    }
                    else
                    {
                        promptlbl = CreatePromptMarkup(thePrompt, style, "", promptType, theEditMask);
                    }

                    promptlbl.ID = this.ID + "_ltrl" + theFldName + ctrlnum.ToString();

                    if (theEditMask.Equals("CB-R"))
                    {
                        promptlbl.Text = "";
                        promptlbl.ID = "";
                    }

                    tc.Controls.Add(promptlbl);
                    tr.Cells.Add(tc);
                    tc = new TableCell();
                    if (sBumpNextFieldUp == "P") tc.Width = (int)tc.Width.Value + packedWidth; // HTML5 fixed for pack
                    //tc.BorderWidth = 0;
                    //tc.BorderStyle = BorderStyle.None;
                    //    tc.BackColor = Color.Silver;
                }

                // if all fields doesn't have Not Filter on, do not add the cell between prompt and field (where checkbox is added)
                if (notFilterCount > 0)
                {
                    TableCell notFilterCell = new TableCell();
                    
                    if (usesNotFilter)
                    {
                        CheckBox chkNotFilter = new CheckBox();
                        chkNotFilter.ID = GenerateControlID("chkNot", ref ctrlnum);
                        chkNotFilter.ToolTip = "NOT Filter";
                        chkNotFilter.TabIndex = LastTabIndex;

                        if (isPacked)
                        {
                            // if packed, just add the checkbox to the wrapper instead of cell
                            wrapper.Controls.Add(chkNotFilter);
                            tc.Width = (int)tc.Width.Value + 13;
                        }
                        else
                            notFilterCell.Controls.Add(chkNotFilter);

                        myprec.chkNotFilter = chkNotFilter;
                    }
                    if (!isPacked)
                        tr.Cells.Add(notFilterCell);
                }

                if (myprec.isDateMask(theEditMask))
                {
                    tb = new TextBox();
                    tb.TabIndex = LastTabIndex;
                    if (!allowEdit)
                    {
                        tb.ReadOnly = true;
                        tb.BackColor = Color.LightGray;
                    }

                    if (theDisplayWidth > 0)
                    {
                        tb.Width = theDisplayWidth;
                    }
                    else
                    {
                        tb.Width = Unit.Pixel(110);
                    }
                    //tb.CssClass = PLCDBPanelCSS;
                    tb.ID = "DBP_DATE_" + ID + ctrlnum.ToString();
                    myprec.tb = tb;
                    tb.Text = theInitialValue;

                    tb.Attributes.Clear();
                    if (!IsSearchPanel)
                        tb.Attributes.Add("onkeydown", "onSingleLineTextBoxKeyDown(event);");
					tb.Attributes.Add("INITVAL", theInitialValue);
                    tb.Attributes.Add("CUSTOM1", theCustom1);
                    tb.Attributes.Add("CUSTOM2", theCustom2);

                    var flexWrapper = new Panel();
                    flexWrapper.Attributes.Add("style", "display: -ms-inline-flexbox;display: inline-flex;");

                    flexWrapper.Controls.Add(tb);

                    AjaxControlToolkit.MaskedEditExtender maskedit = new AjaxControlToolkit.MaskedEditExtender();
                    //maskedit.ClearMaskOnLostFocus = true;

                    //Set date format as MonthDayYear as default;
                    AjaxControlToolkit.MaskedEditUserDateFormat theDateFormat = AjaxControlToolkit.MaskedEditUserDateFormat.MonthDayYear;

                    if (IsDMYFormat(theEditMask))
                        theDateFormat = AjaxControlToolkit.MaskedEditUserDateFormat.DayMonthYear;

                    maskedit.ID = GenerateControlID("maskedit_", ref ctrlnum); //"maskedit_" + ctrlnum.ToString();
                    maskedit.Century = 2000;
                    //maskedit.UserDateFormat = AjaxControlToolkit.MaskedEditUserDateFormat.MonthDayYear;
                    maskedit.UserDateFormat = theDateFormat;
                    maskedit.MaskType = myprec.isDateTimeMask(theEditMask) ? MaskedEditType.DateTime : MaskedEditType.Date;
                    maskedit.Mask = myprec.isDateTimeMask(theEditMask)
                        ? theEditMask.ToUpper().Contains(":SS")
                            ? "99/99/9999 99:99:99"
                            : "99/99/9999 99:99"
                        : "99/99/9999";
                    maskedit.TargetControlID = tb.ID;
                    //             
                    ImageButton imgCalendar = new ImageButton();
                    imgCalendar.ID = GenerateControlID("imgCalendar_", ref ctrlnum);
                    imgCalendar.ImageUrl = "~/images/calendar.png";
                    LastTabIndex++;
                    tabCount++;
                    imgCalendar.TabIndex = LastTabIndex;
                    imgCalendar.Visible = allowEdit;
                    //
                    myprec.btn = imgCalendar;
                    CalendarExtender calendar = new CalendarExtender();
                    calendar.FirstDayOfWeek = FirstDayOfWeek.Sunday;
                    if (IsDMYFormat(theEditMask))
                    {
                        calendar.Format = myprec.isDateTimeMask(theEditMask)
                            ? theEditMask.ToUpper().Contains(":SS")
                                ? "dd/MM/yyyy HH:mm:ss "
                                : "dd/MM/yyyy HH:mm "
                            : "dd/MM/yyyy";
                        maskedit.CultureName = "en-GB";
                    }
                    else //Set MM/dd/yyyy as default
                    {
                        calendar.Format = myprec.isDateTimeMask(theEditMask)
                            ? theEditMask.ToUpper().Contains(":SS")
                                ? "MM/dd/yyyy HH:mm:ss "
                                : "MM/dd/yyyy HH:mm "
                            : "MM/dd/yyyy";
                        maskedit.CultureName = "en-US";
                    }
                    calendar.TargetControlID = tb.ID;
                    calendar.PopupButtonID = imgCalendar.ID;
                    calendar.Enabled = allowEdit;
                   
					if (noFutureDates)
					{
                        if (IsDMYFormat(theEditMask))
                            maskedit.MaxDate = DateTime.Now.ToString("dd/MM/yyyy");
                        else
                            maskedit.MaxDate = DateTime.Now.ToShortDateString();
                        
                        calendar.MaxDate = DateTime.Now.ToShortDateString();

                        myprec.NoFutureDates = true;
					}

                    if (noPastDates)
                    {
                        if (IsDMYFormat(theEditMask))
                            maskedit.MinDate = DateTime.Now.ToString("dd/MM/yyyy");
                        else
                            maskedit.MinDate = DateTime.Now.ToShortDateString();

                        calendar.MinDate = DateTime.Now.ToShortDateString();

                        myprec.NoPastDates = true;
                    }

                    calendar.OnClientShowing = "CalendarOnClientShowing";

                    imgCalendar.Height = Unit.Pixel(16);
                    flexWrapper.Controls.Add(imgCalendar);

                    if (isDateTime)
                    {
                        var time = CreateTimeTextBox(flexWrapper, ctrlnum);
                        time.TabIndex = LastTabIndex;
                        myprec.time = time;
                    }

                    wrapper.Controls.Add(flexWrapper);
                    wrapper.Controls.Add(maskedit);
                    wrapper.Controls.Add(calendar);
                    wrapper.Controls.Add(myprec.ErrMsg);
                    wrapper.Controls.Add(myprec.StatusMsg);

                    tc.Controls.Add(wrapper);
                }

                else
                {
                    if (theCodeTable == "")
                    {
                        if (theEditMask == "COMBOLIST")
                        {
                            int optionlen = 1;

                            if (listOptions != "")
                            {
                                if (listOptions.Contains("{") && listOptions.Contains("}"))
                                {
                                    //Options has select statement that will be used as flexbox datasource 
                                    string query = listOptions.Substring(listOptions.IndexOf("{") + 1, (listOptions.IndexOf("}")) - (listOptions.IndexOf("{") + 1));
                                    query = PLCSession.ProcessSQLMacro(query);

                                    FlexBox flexBox = new FlexBox();
                                    flexBox.ID = GenerateControlID("FlexBox", ref ctrlnum);
                                    flexBox.Width = 200;
                                    flexBox.ReadOnly = !allowEdit;
                                    flexBox.TabIndex = LastTabIndex;
                                    flexBox.DescriptionFormatCode = codeDescFormat;
                                    flexBox.DescriptionSeparator = codeDescSeparator;
                                    flexBox.ComboBox = true;
                                    flexBox.MaxLength = theFldLen;
                                    flexBox.ShowActiveOnly = !IsSearchPanel;
                                    flexBox.UsesNullOrBlank = usesNullBlankOption;
    
                                    PLCQuery qryOption = new PLCQuery(query);
                                    qryOption.Open();
                                    if (!qryOption.IsEmpty())
                                    {

                                        flexBox.DataSource = qryOption.PLCDataTable.AsEnumerable().Select(a => new
                                        {
                                            DESCRIPTION = a["DESCRIPTION"].ToString(),
                                            CODE = a["CODE"].ToString()
                                        });
                                        flexBox.DataTextField = "DESCRIPTION";
                                        flexBox.DataValueField = "CODE";
                                        flexBox.DataBind();
                                    }
                                    else 
                                    { 
                                        ListItemCollection emptyCollection = new ListItemCollection();
                                        emptyCollection.Add(new ListItem(string.Empty, string.Empty));
                                        flexBox.DataSource = emptyCollection;
                                        flexBox.DataBind();
                                    }

                                    myprec.chFlexBox = flexBox;
                                    myprec.chFlexBox.Attributes.Clear();
                                    myprec.chFlexBox.Attributes.Add("INITVAL", theInitialValue);
                                    myprec.chFlexBox.Attributes.Add("CUSTOM1", theCustom1);
                                    myprec.chFlexBox.Attributes.Add("CUSTOM2", theCustom2);

                                    if (defaultValue != string.Empty)
                                    {
                                        myprec.chFlexBox.Attributes.Add("DEFAULT_VALUE", defaultValue.Trim());
                                    }

                                    if (sPostBack == "T")
                                        flexBox.AutoPostBack = true;

                                    flexBox.ValueChanged += new ValueChangedEventHandler(flexBox_ValueChanged);
                                    if (isPacked)
                                        flexBox.Style.Add("display", "inline");

                                    wrapper.Controls.Add(flexBox);
                                    wrapper.Controls.Add(myprec.ErrMsg);
                                    wrapper.Controls.Add(myprec.StatusMsg);

                                    tc.Controls.Add(wrapper);

                                }
                                else
                                {
                                    ddl = new DropDownList();
                                    ddl.ID = GenerateControlID("DDL", ref ctrlnum);
                                    ddl.Items.Add(new ListItem("", ""));

                                    if (usesNullBlankOption)
                                        AddNullOrBlankOptions(ddl.Items);

                                    ddl.TabIndex = LastTabIndex;
                                    ddl.Enabled = allowEdit;

                                    ddl.Attributes.Clear();
                                    ddl.Attributes.Add("INITVAL", theInitialValue);
                                    ddl.Attributes.Add("CUSTOM1", theCustom1);
                                    ddl.Attributes.Add("CUSTOM2", theCustom2);
                                    ddl.Attributes.Add("DEFAULT_VALUE", defaultValue.Trim());
                                    ddl.Attributes.Add("FIELDNAME", theFldName);

                                    string[] options = listOptions.Split(',');
                                    foreach (string item in options)
                                    {
                                        string code = string.Empty;
                                        string description = string.Empty;
                                        //format: (<code1>)<description1>,(<code2>)<description2>,..(<codeN>)<descriptionN>
                                        if (item.TrimStart().StartsWith("(") && item.Contains(")"))
                                        {
                                            code = item.Substring(item.IndexOf("(") + 1, (item.IndexOf(")")) - (item.IndexOf("(") + 1));
                                            description = item.Substring(item.IndexOf(")") + 1);
                                        }
                                        else
                                        {
                                            code = item.Trim();
                                            description = item.Trim();
                                        }
                                        ddl.Items.Add(new ListItem(description, code));

                                        if (description.Length > optionlen)
                                            optionlen = description.Length;
                                    }

                                    if (theDisplayWidth == 0)
                                    {
                                        theDisplayWidth = (optionlen * 10) + 20;
                                    }

                                    if (sPostBack == "T")
                                    {
                                        ddl.AutoPostBack = true;
                                        ddl.TextChanged += new EventHandler(ddl_TextChanged);
                                    }  

                                    ddl.Width = Unit.Pixel(theDisplayWidth);
                                    myprec.ddl = ddl;

                                    wrapper.Controls.Add(ddl);
                                    wrapper.Controls.Add(myprec.ErrMsg);
                                    wrapper.Controls.Add(myprec.StatusMsg);

                                    tc.Controls.Add(wrapper);
                                }
							}
                        }
                        else if (theEditMask == "FREEFORMLIST")
                        {
                            // Field type freeform flexbox with CSV-based option
                            PLCSession.WriteDebug("FREEFORMLIST start", true);

                            if (!string.IsNullOrEmpty(listOptions))
                            {
                                ListItemCollection licOptions = new ListItemCollection();
                                List<string> options = new List<string>();
                                string code = string.Empty;
                                string description = string.Empty;
                                int optionLen = 1;

                                PLCSession.WriteDebug("parse flexbox list options", true);
                                options = listOptions
                                    .Split(',')
                                    .Select(opt => opt.Trim())
                                    .ToList();

                                // create option list
                                foreach (string option in options)
                                {
                                    if (!string.IsNullOrEmpty(option))
                                    {
                                        if (option.StartsWith("(") && option.Contains(")"))
                                        {
                                            // format: (<code1>)<description1>,(<code2>)<description2>,..(<codeN>)<descriptionN>
                                            code = option.Substring(option.IndexOf("(") + 1, option.IndexOf(")") - (option.IndexOf("(") + 1)).Trim();
                                            description = option.Substring(option.IndexOf(")") + 1).Trim();
                                        }
                                        else
                                        {
                                            code = option;
                                            description = option;
                                        }

                                        // create item list
                                        licOptions.Add(new ListItem(description, code));

                                        if (description.Length > optionLen)
                                        {
                                            optionLen = description.Length;
                                        }
                                    }
                                }

                                if (theDisplayWidth == 0)
                                {
                                    theDisplayWidth = (optionLen * 10) + 20;
                                }

                                // create flexbox
                                PLCSession.WriteDebug("create flexbox", true);
                                FlexBox fbFreeFormList = new FlexBox();
                                fbFreeFormList.ID = GenerateControlID("FlexBox", ref ctrlnum);
                                fbFreeFormList.Width = theDisplayWidth;
                                fbFreeFormList.ReadOnly = !allowEdit;
                                fbFreeFormList.TabIndex = LastTabIndex;
                                fbFreeFormList.ComboBox = true;
                                fbFreeFormList.MaxLength = theFldLen;
                                fbFreeFormList.DataSource = licOptions;
                                fbFreeFormList.DataBind();
                                fbFreeFormList.ValueChanged += new ValueChangedEventHandler(flexBox_ValueChanged);
                                fbFreeFormList.AttachPopupTo = PLCAttachPopupTo;

                                if (sPostBack == "T")
                                {
                                    fbFreeFormList.AutoPostBack = true;
                                }

                                // add to panel rec
                                myprec.chFlexBox = fbFreeFormList;
                                myprec.chFlexBox.Attributes.Clear();
                                myprec.chFlexBox.Attributes.Add("INITVAL", theInitialValue);
                                myprec.chFlexBox.Attributes.Add("CUSTOM1", theCustom1);
                                myprec.chFlexBox.Attributes.Add("CUSTOM2", theCustom2);
                                myprec.chFlexBox.Attributes.Add("DEFAULT_VALUE", defaultValue.Trim());

                                // add control to wrapper
                                PLCSession.WriteDebug("add flexbox to wrapper", true);

                                // add to container
                                if (isPacked)
                                {
                                    wrapper.Style.Add("display", "flex");
                                    //wrapper.Style.Add("align-items", "center");
                                    wrapper.Style.Add("flex-wrap", "wrap");
                                }

                                wrapper.Controls.Add(fbFreeFormList);
                                wrapper.Controls.Add(myprec.ErrMsg);
                                wrapper.Controls.Add(myprec.StatusMsg);

                                PLCSession.WriteDebug("add wrapper to container", true);
                                tc.Controls.Add(wrapper);
                            }

                            PLCSession.WriteDebug("FREEFORMLIST end", true);
                        }
                        else if (theEditMask == "RADIOLIST")
                        {
                            var rbl = new RadioButtonList();
                            rbl.TabIndex = LastTabIndex;
                            rbl.Enabled = allowEdit;
                            
                            rbl.Attributes.Clear();
                            rbl.Attributes.Add("INITVAL", theInitialValue);
                            rbl.Attributes.Add("CUSTOM1", theCustom1);
                            rbl.Attributes.Add("CUSTOM2", theCustom2);
                            rbl.Attributes.Add("FLDNAME", theTblName + "." + theFldName);

                            int optionlen = 1;
                            if (listOptions != "")
                            {
                                string[] options = listOptions.Split(',');
                                foreach (string item in options)
                                {
                                    string code = string.Empty;
                                    string description = string.Empty;
                                    //format: (<code1>)<description1>,(<code2>)<description2>,..(<codeN>)<descriptionN>
                                    if (item.Contains("(") && item.Contains(")"))
                                    {
                                        code = item.Substring(item.IndexOf("(") + 1, (item.IndexOf(")")) - (item.IndexOf("(") + 1));
                                        description = item.Substring(item.IndexOf(")") + 1);
                                    }
                                    else
                                    {
                                        code = item;
                                        description = item;
                                    }
                                    ListItem li = new ListItem(description.Trim(), code.Trim());
                                    rbl.Items.Add(li);

                                    if (description.Length > optionlen)
                                        optionlen = description.Length;
                                }
                            }

                            //if (theDisplayWidth == 0)
                            //{
                            //    theDisplayWidth = (optionlen * 10) + 20;
                            //}

                            //rbl.Width = Unit.Pixel(theDisplayWidth);
                            rbl.Width = Unit.Percentage(100);
                            myprec.rbl= rbl;

                            wrapper.Controls.Add(rbl);
                            wrapper.Controls.Add(myprec.ErrMsg);
                            wrapper.Controls.Add(myprec.StatusMsg);

                            tc.Controls.Add(wrapper);
                        }
                        else if (theEditMask == "CB" || theEditMask == "CBDISPLAY" || theEditMask == "CB-R")
                        {
                            cb = new CheckBox();
                            cb.TabIndex = LastTabIndex;
                            cb.Enabled = allowEdit;
                            cb.ID = "" + ID + ctrlnum.ToString();
                            cb.Attributes.Add("LINKEDFIELD", theFldName);
                            cb.Attributes.Add("LINKEDLABEL", thePrompt);
                            cb.Attributes.Add("EDITMASK", theEditMask);
                            //*AAC 07072010 - set checkbox postback only if POSTBACK field = 'T'
                            if (sPostBack == "T")
                            {
                                cb.AutoPostBack = true;
                                cb.CheckedChanged += new EventHandler(cb_CheckedChanged);
                            }

                            myprec.cb = cb;

                            wrapper.Controls.Add(cb);

                            if (isBumped || isPacked || theEditMask.Equals("CB-R"))
                            {
                                promptlbl = CreatePromptMarkup(checkboxLabel, "display:inline-block;", "", promptType, "CheckboxPrompt");
                                promptlbl.ID = this.ID + "_ltrl" + theFldName + ctrlnum.ToString();
                                wrapper.Controls.Add(promptlbl);
                                wrapper.Attributes.Add("style", "display: inline-flex;display: -ms-inline-flexbox;");
                            }

                            tc.Controls.Add(wrapper);
                        }
                        else if (theEditMask == "MULTIPICK" || theEditMask == "MULTIENUM")
                        {
                            if (listOptions != "" || theEditMask == "MULTIENUM")
                            {
                                CodeMultiPick multiLookup;
                                if (theEditMask == "MULTIENUM")
                                    multiLookup = new CodeMultiPick(ID + ctrlnum.ToString(), true);
                                else
                                {
                                    bool listOptionsProcessed = false;
                                    if (listOptions.Contains("{") && listOptions.Contains("}"))
                                    {
                                        listOptions = GetMultiPickSelections(listOptions);
                                        listOptionsProcessed = true;
                                    }

                                    multiLookup = new CodeMultiPick(ID + ctrlnum.ToString(), listOptions);
                                    multiLookup.DoNotUpperCaseCode = listOptionsProcessed ? "T" : "";
                                }

                                if (sPostBack == "T")
                                {
                                    multiLookup.PostBack = true;
                                    multiLookup.CodeMultiPickValueChanged += new EventHandler(multiPick_ValueChanged);
                                }

                                multiLookup.PopupX = "0";
                                multiLookup.PopupY = "0";

                                multiLookup.HeaderPrompt = thePrompt.Trim();
                                multiLookup.SetTextBoxTabindex(LastTabIndex);
                                LastTabIndex++;
                                tabCount++;
                                multiLookup.Enabled = allowEdit;
                                multiLookup.UsesNullOrBlank = usesNullBlankOption;

                                myprec.chMultiLookup = multiLookup;
                                myprec.chMultiLookup.Attributes.Clear();
                                myprec.chMultiLookup.Attributes.Add("INITVAL", theInitialValue);
                                myprec.chMultiLookup.Attributes.Add("CUSTOM1", theCustom1);
                                myprec.chMultiLookup.Attributes.Add("CUSTOM2", theCustom2);
                                
                                wrapper.Controls.Add(multiLookup);
                                wrapper.Controls.Add(myprec.ErrMsg);
                                wrapper.Controls.Add(myprec.StatusMsg);
                                tc.Controls.Add(wrapper);
                            }
                        }
                        else if (theEditMask == "MULTIPICK_SEARCH")
                        {
                            //do nothing
                        }
                        else if (theEditMask == "MULTIPICK-AC")
                        {
                            if (listOptions != "" || theEditMask == "MULTIENUM")
                            {
                               CodeMultiPickAC multiLookup;
                                if (theEditMask == "MULTIENUM")
                                    multiLookup = new CodeMultiPickAC(ID + ctrlnum.ToString(), true);
                                else
                                {
                                    bool listOptionsProcessed = false;
                                    if (listOptions.Contains("{") && listOptions.Contains("}"))
                                    {
                                        listOptions = GetMultiPickSelections(listOptions);
                                        listOptionsProcessed = true;
                                    }

                                    multiLookup = new CodeMultiPickAC(ID + ctrlnum.ToString(), listOptions);
                                    multiLookup.DoNotUpperCaseCode = listOptionsProcessed ? "T" : "";
                                }

                                if (sPostBack == "T")
                                {
                                    multiLookup.PostBack = true;
                                    multiLookup.CodeMultiPickValueChanged += new EventHandler(multiPick_ValueChanged);
                                }

                                multiLookup.PopupX = "0";
                                multiLookup.PopupY = "0";

                                multiLookup.HeaderPrompt = thePrompt.Trim();
                                multiLookup.SetTextBoxTabindex(LastTabIndex);
                                LastTabIndex++;
                                tabCount++;
                                multiLookup.Enabled = allowEdit;
                                multiLookup.UsesNullOrBlank = usesNullBlankOption;

                                myprec.chMultipickAc = multiLookup;
                                myprec.chMultipickAc.Attributes.Clear();
                                myprec.chMultipickAc.Attributes.Add("INITVAL", theInitialValue);
                                myprec.chMultipickAc.Attributes.Add("CUSTOM1", theCustom1);
                                myprec.chMultipickAc.Attributes.Add("CUSTOM2", theCustom2);

                                wrapper.Controls.Add(multiLookup);
                                wrapper.Controls.Add(myprec.ErrMsg);
                                wrapper.Controls.Add(myprec.StatusMsg);
                                tc.Controls.Add(wrapper);
                            }
                        }
                        else if (theEditMask == "SIGNATURE")
                        {
                            var sig = new Signature();
                            sig.ID = GenerateControlID("Signature", ref ctrlnum);
                            sig.Width = theDisplayWidth > 0 ? theDisplayWidth : 200;
                            sig.TabIndex = LastTabIndex;
                            sig.Enabled = allowEdit;

                            if (!string.IsNullOrEmpty(theCodeCondition))
                            {
                                if (theCodeCondition.StartsWith("{") && theCodeCondition.EndsWith("}"))
                                {
                                    SignatureControlsWithName.Add(sig, theCodeCondition);
                                }
                            }

                            //if (sPostBack == "T")
                            //{
                            //}

                            myprec.sig = sig;

                            wrapper.Controls.Add(sig);
                            wrapper.Controls.Add(myprec.ErrMsg);
                            wrapper.Controls.Add(myprec.StatusMsg);

                            tc.Controls.Add(wrapper);
                        }
                        else
                        {
                            tb = new TextBox();
                            tb.Attributes.Clear();
                            tb.TabIndex = LastTabIndex;
                            if (!allowEdit)
                            {
                                //tb.ReadOnly = true; // if readonly is set to true, the textbox changes on the client side are ignored during postback
                                tb.Attributes["readonly"] = "readonly";
                                tb.BackColor = Color.LightGray;
                            }

                            tb.Text = theInitialValue;
                            tb.Attributes.Add("INITVAL", theInitialValue);
                            tb.Attributes.Add("CUSTOM1", theCustom1);
                            tb.Attributes.Add("CUSTOM2", theCustom2);

                            if (barcodeScan)
                                tb.Attributes.Add("BarcodeScanHere", "true");

                            if (theFldLen <= 60)
                                tb.Columns = (int)theFldLen;
                            else
                                tb.Columns = 60;

                            tb.MaxLength = (int)theFldLen;

                            tb.ID = "DBP_" + ID + ctrlnum.ToString();
                            myprec.tb = tb;

                            if (myprec.memolines > 0)
                            {
                                myprec.tb.TextMode = TextBoxMode.MultiLine;
                                myprec.tb.Rows = (int)myprec.memolines;
                                myprec.tb.Attributes.Add("onblur", "limitTextBoxLengthOnBlur(event, this, " + theFldLen + ");");
                                myprec.tb.Attributes.Add("onkeydown", "return limitTextBoxLength(event, this, " + theFldLen + ");");
                                myprec.tb.Attributes.Add("ondblclick", "return expandTextBox(event, this);");
                                myprec.tb.Style.Add("white-space", "pre-wrap");
                            }
                            else
                            {
                                // Client-side callback to prevent Enter key from being processed in single-line textboxes.
                                if (!IsSearchPanel)
                                  myprec.tb.Attributes.Add("onkeydown", "onSingleLineTextBoxKeyDown(event);");
                            }

                            if (theDisplayWidth > 0)
                                tb.Width = theDisplayWidth;

                            mymaskedit = null;

                            theEditMask = theEditMask.ToUpper() == "EMAILADDR" ? "" : theEditMask;
              
                            if(!myprec.isDateMask(theEditMask) && !string.IsNullOrEmpty(displayFormula))
                            {
                                 myprec.AllowEdit = false;
                                 myprec.AllowAdd = false;
                                 tb.Attributes["readonly"] = "readonly";
                                 tb.BackColor = Color.LightGray;
                                 tb.Attributes.Add("DISPLAYFORMULAFIELD", this.ID + "|" + theTblName + "." + theFldName);
                                 tb.Attributes.Add("FORMULAFIELDVALUE", "");
                            }

                            if (theEditMask != "")
                            {
                                mymaskedit = new AjaxControlToolkit.MaskedEditExtender();
                                mymaskedit.BehaviorID = mymaskedit.ID = "maskedit_" + ID + ctrlnum.ToString();
                                mymaskedit.MaskType = AjaxControlToolkit.MaskedEditType.None;

                                if (theEditMask.ToUpper() == "HH:MM")
                                {
                                    mymaskedit.MaskType = MaskedEditType.Time;
                                    mymaskedit.Mask = "99:99";
                                }
                                else if (theEditMask.ToUpper() == "HH:MM:SS")
                                {
                                    mymaskedit.MaskType = MaskedEditType.Time;
                                    mymaskedit.Mask = "99:99:99";
                                }
                                else if (theEditMask.ToUpper() == "BC") //barcode field *09102009*
                                {
                                    mymaskedit.Enabled = false;
                                    tb.Attributes.Add("onkeydown", "var evt = (evt) ? evt : ((event) ? event : null); var node = (evt.target) ? evt.target : ((evt.srcElement) ? evt.srcElement : null); if ((evt.keyCode == 13) && (node.type=='text')) { if (txt != '') { this.blur(); this.focus(); return false;} } else { txt = this.value; }");
                                    tb.Attributes.Add("onblur", "txt = ''");
                                }
                                else if (theEditMask.Contains("9") && //check if numeric with decimal or comma separator
                                    (theEditMask.Contains(".") || theEditMask.Contains(",")))
                                {
                                    mymaskedit.Mask = theEditMask;
                                    mymaskedit.MaskType = MaskedEditType.Number;
                                    mymaskedit.InputDirection = MaskedEditInputDirection.RightToLeft;
                                }
                                else
                                {
                                    mymaskedit.Mask = theEditMask;   // disabled for now maskedit does not support dbpanel edit mask
                                    mymaskedit.ClearMaskOnLostFocus = true;
                                }

                                if (theEditMask.ToUpper().StartsWith("HH:MM"))
                                {
                                    myprec.NoFutureDates = noFutureDates;
                                    myprec.NoPastDates = noPastDates;
                                }

                                myprec.mee = mymaskedit;
                                mymaskedit.TargetControlID = tb.ID;
                            }

                            if (mymaskedit != null)
                            {
                                wrapper.Controls.Add(mymaskedit);
                            }

                            wrapper.Controls.Add(tb);
                            wrapper.Controls.Add(myprec.ErrMsg);
                            wrapper.Controls.Add(myprec.StatusMsg);

                            tc.Controls.Add(wrapper);
                        }
                    }
                    else if (theCodeTable != "" && (theEditMask == "MULTIPICK_SEARCH" || theEditMask == "MULTIPICK" || AttributeType == "M"))
                    {
                        // Multilookup controls.
                        CodeMultiPick multiLookup;
                        if (AttributeType == "M")
                            multiLookup = new CodeMultiPick(ID + ctrlnum.ToString(), theCodeTable, "VALUE", "DESCRIPTION", theCodeCondition, "", true);
                        else
                        {
                            if (theCodeCondition != "")
                            {
                                multiLookup = new CodeMultiPick(ID + ctrlnum.ToString(), theCodeTable, "", "", this.FormatCodeCondition(theCodeCondition), "", true);
                            }
                            else
                            {
                                multiLookup = new CodeMultiPick(ID + ctrlnum.ToString(), theCodeTable, true);
                            }

                            if (sPostBack == "T")
                            {
                                multiLookup.PostBack = true;
                                multiLookup.CodeMultiPickValueChanged += new EventHandler(multiPick_ValueChanged);
                            }
                        }

                        multiLookup.UsesSearchBar = (theEditMask == "MULTIPICK_SEARCH");
                        multiLookup.PopupX = "0";
                        multiLookup.PopupY = "0";
                        
                        multiLookup.HeaderPrompt = thePrompt.Trim();
                        multiLookup.SetTextBoxTabindex(LastTabIndex);
                        LastTabIndex++;
                        tabCount++;
                        multiLookup.Enabled = allowEdit;
                        multiLookup.UsesNullOrBlank = usesNullBlankOption;

                        myprec.chMultiLookup = multiLookup;
                        myprec.chMultiLookup.Attributes.Clear();
                        myprec.chMultiLookup.Attributes.Add("INITVAL", theInitialValue);
                        myprec.chMultiLookup.Attributes.Add("CUSTOM1", theCustom1);
                        myprec.chMultiLookup.Attributes.Add("CUSTOM2", theCustom2);

                        wrapper.Controls.Add(multiLookup);
                        wrapper.Controls.Add(myprec.ErrMsg);
                        wrapper.Controls.Add(myprec.StatusMsg);

                        PLCSession.WriteDebug("wrapper added:", true);
                        tc.Controls.Add(wrapper);
                    }
                    else if (theCodeTable != "" && (theEditMask == "MULTIPICK-AC"))
                    {
                        // Multilookup controls.
                        CodeMultiPickAC multiLookup;
                        if (AttributeType == "M")
                            multiLookup = new CodeMultiPickAC(ID + ctrlnum.ToString(), theCodeTable, "VALUE", "DESCRIPTION", theCodeCondition, "", true);
                        else
                        {
                            if (theCodeCondition != "")
                            {
                                multiLookup = new CodeMultiPickAC(ID + ctrlnum.ToString(), theCodeTable, "", "", this.FormatCodeCondition(theCodeCondition), "", true);
                            }
                            else
                            {
                                multiLookup = new CodeMultiPickAC(ID + ctrlnum.ToString(), theCodeTable, true);
                            }

                            if (sPostBack == "T")
                            {
                                multiLookup.PostBack = true;
                                multiLookup.CodeMultiPickValueChanged += new EventHandler(multiPick_ValueChanged);
                            }
                        }

                        multiLookup.UsesSearchBar = (theEditMask == "MULTIPICK_SEARCH");
                        multiLookup.PopupX = "0";
                        multiLookup.PopupY = "0";

                        multiLookup.HeaderPrompt = thePrompt.Trim();
                        multiLookup.SetTextBoxTabindex(LastTabIndex);
                        LastTabIndex++;
                        tabCount++;
                        multiLookup.Enabled = allowEdit;
                        multiLookup.UsesNullOrBlank = usesNullBlankOption;

                        myprec.chMultipickAc = multiLookup;
                        myprec.chMultipickAc.Attributes.Clear();
                        myprec.chMultipickAc.Attributes.Add("INITVAL", theInitialValue);
                        myprec.chMultipickAc.Attributes.Add("CUSTOM1", theCustom1);
                        myprec.chMultipickAc.Attributes.Add("CUSTOM2", theCustom2);

                        wrapper.Controls.Add(multiLookup);
                        wrapper.Controls.Add(myprec.ErrMsg);
                        wrapper.Controls.Add(myprec.StatusMsg);

                        PLCSession.WriteDebug("wrapper added:", true);
                        tc.Controls.Add(wrapper);
                    }
                    else
                    {
                        // CodeHead type controls.

                        PLCSession.WriteDebug("flex 1",true);

                        myprec.attribtype = AttributeType;

                        nUPCount++;

                        int codeHeadWidth = 200; //default

                        CodeHeadAttributes attributes = CodeHeadConfigurations(theCodeTable);

                        PLCSession.WriteDebug("flex 2", true);

                        codeHeadWidth = attributes.Width;
                        string sortOrder = attributes.SortOrder;

                        //for flexbox only; if DBPANEL DescFormat and DescSeparator are empty, use CODEHEAD DescFormat and DescSeparator.
                        if (codeDescFormat == "")
                            codeDescFormat = attributes.DescriptionFormat;
                        if (codeDescSeparator == "")
                            codeDescSeparator = attributes.DescriptionSeparator;

                        if (theDisplayWidth > 0)
                            codeHeadWidth = theDisplayWidth;

                        PLCSession.WriteDebug("flex 3, displaywidth:" + codeHeadWidth.ToString(), true);


                        switch (attributes.CodeHeadType)
                        {
                            case ControlType.ComboBox:
                                PLCSession.WriteDebug("case ControlType.ComboBox:", true);    
                                TADropDown comboBox = new TADropDown();
                                comboBox.ID = GenerateControlID("ComboBox", ref ctrlnum);
                                comboBox.Font.Size = 10;
                                comboBox.Width = codeHeadWidth;
                                comboBox.TabIndex = LastTabIndex;
                                comboBox.SelectedValue = theInitialValue;
                                comboBox.Enabled = allowEdit;
                                
                                SqlDataSource chComboDataSource = new SqlDataSource();
                                chComboDataSource.ID = GenerateControlID("SqlDataSource", ref nUPCount);
                                chComboDataSource.ConnectionString = _PLCConnectionString;
                                chComboDataSource.Init += new EventHandler(SqlDataSource1_Init);
                                // enable caching on datasource
                                chComboDataSource.EnableCaching = true;
                                chComboDataSource.CacheExpirationPolicy = DataSourceCacheExpiry.Absolute;
                                chComboDataSource.CacheDuration = 600;

                                myprec.chSqlDataSource = chComboDataSource;
                                myprec._codetable = theCodeTable;
                                myprec.chComboBox = comboBox;
                                myprec.chComboBox.Attributes.Clear();
                                myprec.chComboBox.Attributes.Add("INITVAL", theInitialValue);
                                myprec.chComboBox.Attributes.Add("CUSTOM1", theCustom1);
                                myprec.chComboBox.Attributes.Add("CUSTOM2", theCustom2);
                                myprec.chSortOrder = sortOrder;

                                comboBox.Style.Add("display", "block");
                                comboBox.DataSourceID = chComboDataSource.ID;

                                if (sPostBack == "T")
                                {
                                    comboBox.AutoPostBack = true;
                                    comboBox.Attributes.Add("LINKEDCODETABLE", theCodeTable);
                                    comboBox.Attributes.Add("LINKEDFIELDNAME", theFldName);
                                    comboBox.SelectedIndexChanged += new EventHandler(comboBox_SelectedIndexChanged);
                                }

                                wrapper.Controls.Add(chComboDataSource);
                                wrapper.Controls.Add(comboBox);
                                wrapper.Controls.Add(myprec.ErrMsg);
                                wrapper.Controls.Add(myprec.StatusMsg);

                                PLCSession.WriteDebug("wrapper added:", true);
                                tc.Controls.Add(wrapper);
                                break;

                            case ControlType.FlexBox:
                                PLCSession.WriteDebug("case ControlType.FlexBox:", true);    
                                FlexBox flexBox = new FlexBox();
                                flexBox.ID = GenerateControlID("FlexBox", ref ctrlnum);
                                flexBox.TableName = theCodeTable;
                                flexBox.Width = codeHeadWidth;
                                flexBox.ReadOnly = !allowEdit;
                                flexBox.TabIndex = LastTabIndex;
                                flexBox.DescriptionFormatCode = codeDescFormat;
                                flexBox.DescriptionSeparator = codeDescSeparator;
                                flexBox.ShowActiveOnly = !IsSearchPanel;
                                flexBox.EnableActiveOnlyToggle = allowToggleActiveOnly;
                                flexBox.AttachPopupTo = PLCAttachPopupTo;
                                flexBox.UsesNullOrBlank = usesNullBlankOption;

                                if (AttributeType == "F")
                                {
                                    flexBox.ComboBox = true;
                                    flexBox.MaxLength = theFldLen;
                                }

                                if (!string.IsNullOrEmpty(theCodeCondition))
                                {
                                    //check if it has a parent flexbox
                                    if (theCodeCondition.Contains("{") && theCodeCondition.Contains("}"))
                                        FlexBoxControlsWithParent.Add(flexBox, theCodeCondition);
                                    else if (theCodeCondition.Contains("[") && theCodeCondition.Contains("]"))
                                        FlexBoxControlsWithParent.Add(flexBox, theCodeCondition);
                                    else
                                        flexBox.CodeCondition = theCodeCondition;
                                }

                                myprec._codetable = theCodeTable;
                                myprec.chFlexBox = flexBox;
                                myprec.chFlexBox.Attributes.Clear();
                                myprec.chFlexBox.Attributes.Add("INITVAL", theInitialValue);
                                myprec.chFlexBox.Attributes.Add("CUSTOM1", theCustom1);
                                myprec.chFlexBox.Attributes.Add("CUSTOM2", theCustom2);

                                if (defaultValue != string.Empty)
                                {
                                    myprec.chFlexBox.Attributes.Add("DEFAULT_VALUE", defaultValue.Trim());
                                }

                                myprec.chSortOrder = sortOrder;

                                if (sPostBack == "T")
                                    flexBox.AutoPostBack = true;

                                flexBox.Attributes.Add("LINKEDCODETABLE", theCodeTable);
                                flexBox.Attributes.Add("LINKEDFIELDNAME", theFldName);
                                flexBox.ValueChanged += new ValueChangedEventHandler(flexBox_ValueChanged);
								if (isPacked)
									flexBox.Style.Add("display", "inline-block");
                                                       
                                if (!String.IsNullOrWhiteSpace(myprec.supplementLink))
                                {
                                    String suppTableName = "";
                                    String suppTableDesc = "";
                                    string suppPrimaryKey = "";
                                    string suppFilterCondition = "";
                                    string suppSearchPanelName = "";
                                    string suppFilterValue = "";
                                    try
                                    {
                                        suppTableName = myprec.supplementLink.Split('|')[0];
                                    }
                                    catch
                                    {
                                        suppTableName = "";
                                    }


                                    try
                                    {
                                        suppTableDesc = myprec.supplementLink.Split('|')[1];
                                    }
                                    catch
                                    {
                                        suppTableDesc = "";
                                    }

                                    try
                                    {
                                        suppPrimaryKey = myprec.supplementLink.Split('|')[2];
                                    }
                                    catch
                                    {
                                        suppPrimaryKey = "";
                                    }

                                    try
                                    {
                                        suppFilterCondition = myprec.supplementLink.Split('|')[3];
                                    }
                                    catch
                                    {
                                        suppFilterCondition = "";
                                    }

                                    try
                                    {
                                        suppSearchPanelName = myprec.supplementLink.Split('|')[4];
                                    }
                                    catch
                                    {
                                        suppSearchPanelName = "";
                                    }

                                    string suppFilterField = "";
                                    string codeCondition = "";
                                    
                                    if ((!String.IsNullOrWhiteSpace(suppTableName)) && (!String.IsNullOrWhiteSpace(suppTableDesc) ))
                                    {
                                        if (!string.IsNullOrEmpty(suppFilterCondition))
                                        {
                                            //support for flexboxes without parent to use DEPARTMENT_CODE filter for Add Officer pop up
                                            if (suppFilterCondition.IndexOf("<&>") >= 0) 
                                            {
                                                ProcessMacro(suppFilterCondition, out codeCondition, out suppFilterValue);
                                                int start = suppFilterCondition.IndexOf("<&>") + 3;
                                                int end = suppFilterCondition.IndexOf("</&>");
                                                suppFilterField = suppFilterCondition.Substring(start, end - start);
                                                string filterField = (suppFilterField.Contains("TV_") && suppFilterField.Contains(".")) ? suppFilterField : myprec.tblname + "." + suppFilterField;
                                                                    
                                                flexBox.Attributes.Add("FILTERFIELD", this.ID + "|" + filterField + "|" + suppFilterCondition);
                                                flexBox.DisableCaching = true;

                                               flexBox.CodeCondition = codeCondition;
                                            }
                                            else
                                            {
                                                int start = suppFilterCondition.IndexOf('{') + 1;
                                                int end = suppFilterCondition.IndexOf('}');
                                                suppFilterField = suppFilterCondition.Substring(start, end - start);
                                                string filterField = (suppFilterField.Contains("TV_") && suppFilterField.Contains(".")) ? suppFilterField : myprec.tblname + "." + suppFilterField;

                                                flexBox.Attributes.Add("FILTERFIELD", filterField + "|" + suppFilterCondition);
                                                flexBox.DisableCaching = true;
                                                FlexBoxControlsWithParent.Add(flexBox, suppFilterCondition);                                             

                                              
                                            }


                                        }

                                     wrapper.Style.Add("white-space", "nowrap");

                                    LinkButton lbtn = new LinkButton();
                                    
                                    //lbtn.Style.Add("display", "inline");
                                    lbtn.Text = suppTableDesc;
                                    lbtn.Style.Add("white-space", "nowrap");

                                    LastTabIndex++;
                                    tabCount++;
                                    lbtn.TabIndex = LastTabIndex;

                                    String pStr = "'" + suppTableName + "','" + lbtn.Text + "','" + this.ID + "','" + myprec.tblname + "','" + myprec.fldname + "','" + suppPrimaryKey + "','" + suppFilterField + "','" + suppFilterCondition.Replace("'", "\\'") + "','" + suppFilterValue + "','" + suppSearchPanelName + "'";
                                    lbtn.OnClientClick = "return showSupplementDialog(" + pStr + ");";

                                    //flexBox.Controls.Add(lbtn);

                                    Table t = new Table();
                                    t.Style.Add("border-width", "0");
                                    t.Style.Add("border-style", "none");
                                    t.Style.Add("border-collapse", "collapse");
                                    t.Style.Add("margin", "0");
                                    t.Style.Add("padding", "0");
                                    TableRow r = new TableRow();
                                    t.Rows.Add(r);
                                    TableCell c = new TableCell();
                                    r.Cells.Add(c);
                                    c.Style.Add("border-width", "0");
                                    c.Style.Add("border-style", "none");
                                    c.Style.Add("border-collapse", "collapse");
                                    c.Style.Add("margin", "0");
                                    c.Style.Add("padding", "0");
                                    c.Controls.Add(flexBox);

                                    
                                    c = new TableCell();
                                    c.Wrap = false;
                                    c.Style.Add("border-width", "0");
                                    c.Style.Add("border-style", "none");
                                    c.Style.Add("border-collapse", "collapse");
                                    c.Style.Add("margin", "0");
                                    c.Style.Add("padding", "0");
                                    r.Cells.Add(c);
                                    c.Controls.Add(lbtn);
                                    
                                    wrapper.Controls.Add(t);
                                    
                          
                                    }
                                }
                                else
                                    wrapper.Controls.Add(flexBox);
                                                           

                                wrapper.Controls.Add(myprec.ErrMsg);
                                wrapper.Controls.Add(myprec.StatusMsg);

                                if (!isPacked && !isBumped)
                                    tc.VerticalAlign = VerticalAlign.Top;

                                PLCSession.WriteDebug("wrapper added:", true);
                                tc.Controls.Add(wrapper);
                                break;

                            case ControlType.CodeHeadDialog:
                                PLCSession.WriteDebug("case ControlType.CodeHeadDialog:", true);
                                CodeHead chDlg = new CodeHead();

                                chDlg.ID = GenerateControlID("ch", ref ctrlnum);
                                chDlg.TableName = theCodeTable;
                                chDlg.PopupPanel.Width = Unit.Pixel(550);

                                chDlg.AdditionalFields = "DEPARTMENT_CODE";
                                chDlg.PopupCaption = "Case Officer";
                                chDlg.PopupCaptionCSSClass = "caption";
                                chDlg.PopupCSSClass = "modalPopup";
                                chDlg.ReadOnly = !allowEdit;
                                chDlg.TabIndex = LastTabIndex;

                                myprec.codeHeadDlg = chDlg;
                                wrapper.Controls.Add(chDlg);
                                wrapper.Controls.Add(myprec.ErrMsg);
                                wrapper.Controls.Add(myprec.StatusMsg);

                                PLCSession.WriteDebug("wrapper added:", true);
                                tc.Controls.Add(wrapper);
                                break;

                            case ControlType.Popup:
                                    PLCSession.WriteDebug("case ControlType.Popup:", true);
                                    myprec.chSortOrder = sortOrder;

                                    // parent panel of all controls
                                    UpdatePanel chParentUpdatePanel = new UpdatePanel();
                                    //chParentUpdatePanel.ID = "ParentUpdatePanel" + nUPCount.ToString();
                                    chParentUpdatePanel.ID = GenerateControlID("ParentUpdatePanel", ref nUPCount);
                                    chParentUpdatePanel.UpdateMode = UpdatePanelUpdateMode.Always;

                                    Panel chPanel = new Panel();
                                    chPanel.ID = GenerateControlID("Panel", ref nUPCount);
                                    chPanel.CssClass = "modalPopup";
                                    chPanel.Style.Value = "display: none;"; //background-color: #f5f5f5; border-width: 3px; border-style: solid; border-color: #294B29;";
                                    chPanel.Width = 350;

                                    chPanel.BackColor = Color.WhiteSmoke;

                                    Panel chCaptionPanel = new Panel();
                                    chCaptionPanel.ID = GenerateControlID("CaptionPanel", ref nUPCount);
                                    chCaptionPanel.CssClass = "caption";

                                    LiteralControl lcText = new LiteralControl();
                                    lcText.Text = thePrompt.ToUpper() + " CODES ";
                                    chCaptionPanel.Controls.Add(new LiteralControl("<h4 style=\"text-align:center;margin-top:0px;\"><font color=\"#FFFFFF\">"));
                                    chCaptionPanel.Controls.Add(lcText);
                                    chCaptionPanel.Controls.Add(new LiteralControl("</font></h4>"));

                                    UpdatePanel chUpdatePanel = new UpdatePanel();
                                    chUpdatePanel.ID = GenerateControlID("UpdatePanel", ref nUPCount);
                                    chUpdatePanel.UpdateMode = UpdatePanelUpdateMode.Conditional;

                                    Panel pnlSearch = new Panel();

                                    TextBox txtSearch = new TextBox();
                                    txtSearch.ID = GenerateControlID("TxtSearch", ref nUPCount);
                                    txtSearch.AutoPostBack = false;
                                    pnlSearch.Controls.Add(txtSearch);
                                    myprec.chtxtSearch = txtSearch;

                                    Button btnSearch = new Button();
                                    btnSearch.ID = GenerateControlID("btnSearch", ref nUPCount);
                                    btnSearch.Text = "Search";
                                    btnSearch.Click += new EventHandler(BtnSearch_Click);
                                    pnlSearch.Controls.Add(btnSearch);
                                    myprec.chbtnSearch = btnSearch;

                                    ImageButton btnRefresh = new ImageButton();
                                    btnRefresh.ID = GenerateControlID("btnRefresh", ref nUPCount);
                                    btnRefresh.Height = 22;
                                    btnRefresh.Width = 22;
                                    btnRefresh.Click += new ImageClickEventHandler(btnRefresh_Click);
                                    btnRefresh.ImageUrl = "~/images/refresh.bmp";
                                    btnRefresh.ToolTip = "Refresh Codes";
                                    btnRefresh.Enabled = true;
                                    pnlSearch.Controls.Add(btnRefresh);
                                    myprec.chbtnRefresh = btnRefresh;

                                    pnlSearch.DefaultButton = btnSearch.UniqueID;

                                    SqlDataSource DataSourceCH = new SqlDataSource();
                                    DataSourceCH.ID = GenerateControlID("SqlDataSource", ref nUPCount);
                                    DataSourceCH.ConnectionString = _PLCConnectionString;
                                    DataSourceCH.Init += new EventHandler(SqlDataSource1_Init);
                                    // enable caching on datasource
                                    if (!PLCDisableCaching)
                                    {
                                        DataSourceCH.EnableCaching = true;
                                        DataSourceCH.CacheExpirationPolicy = DataSourceCacheExpiry.Absolute;
                                        DataSourceCH.CacheDuration = 600;
                                    }
                                    else
                                        DataSourceCH.EnableCaching = false;

                                    myprec.chSqlDataSource = DataSourceCH;

                                    Panel pnlGrid = new Panel();

                                    GridView GridViewCH = new GridView();
                                    GridViewCH.ID = GenerateControlID("GridView", ref nUPCount);
                                    GridViewCH.DataSourceID = DataSourceCH.ID.ToString();
                                    GridViewCH.AllowPaging = true;
                                    //GridViewCH.RowCreated += new GridViewRowEventHandler(GridView_RowCreated);
                                    GridViewCH.RowDataBound += new GridViewRowEventHandler(GridView_RowDataBound);
                                    GridViewCH.PageIndexChanging += new GridViewPageEventHandler(GridView_PageIndexChanging);
                                    GridViewCH.Sorting += new GridViewSortEventHandler(GridView_Sorting);
                                    GridViewCH.GridLines = GridLines.Horizontal;
                                    GridViewCH.Width = Unit.Percentage(100);
                                    GridViewCH.AllowSorting = true;
                                    GridViewCH.SkinID = "LookupGrid";

                                    pnlGrid.Controls.Add(GridViewCH);
                                    myprec.chPnlGrid = pnlGrid;
                                    myprec.chGridView = GridViewCH;

                                    CheckBox chkActive = new CheckBox();
                                    chkActive.ID = GenerateControlID("chkActive", ref nUPCount);
                                    chkActive.Text = "Active Only";
                                    chkActive.AutoPostBack = true;
                                    chkActive.CheckedChanged += new EventHandler(chkActive_CheckedChanged);
                                    myprec.chActive = chkActive;

                                    chUpdatePanel.ContentTemplateContainer.Controls.Add(pnlSearch);
                                    chUpdatePanel.ContentTemplateContainer.Controls.Add(pnlGrid);
                                    chUpdatePanel.ContentTemplateContainer.Controls.Add(chkActive);
                                    chUpdatePanel.ContentTemplateContainer.Controls.Add(DataSourceCH);

                                    chPanel.Controls.Add(chCaptionPanel);
                                    chPanel.Controls.Add(chUpdatePanel);

                                    Button OkButton = new Button();
                                    OkButton.ID = GenerateControlID("OkButton", ref nUPCount);
                                    OkButton.Text = "OK";
                                    OkButton.Width = 80;
                                    myprec.chOkButton = OkButton;

                                    Button CancelButton = new Button();
                                    CancelButton.ID = GenerateControlID("CancelButton", ref nUPCount);
                                    CancelButton.Text = "Cancel";
                                    CancelButton.Width = 80;
                                    myprec.chCancelButton = CancelButton;

                                    //// triggers when tabbing from txtLookup
                                    //Button TabPostBackButton = new Button();
                                    ////TabPostBackButton.ID = "PostBackButton" + nUPCount.ToString();
                                    //TabPostBackButton.ID = GenerateControlID("PostBackButton", nUPCount);
                                    ////*AAC 061009
                                    ////TabPostBackButton.CssClass = PLCDBPanelCSS;
                                    //TabPostBackButton.Text = "Cancel";
                                    //TabPostBackButton.Width = 1;
                                    //TabPostBackButton.Height = 1;
                                    //TabPostBackButton.Click += new EventHandler(TabPostBackButton_Click);
                                    //myprec.chTabPostBackButton = TabPostBackButton;

                                    chPanel.Controls.Add(new LiteralControl("<h3 style=\"text-align:center\">"));
                                    chPanel.Controls.Add(OkButton);
                                    chPanel.Controls.Add(CancelButton);
                                    //chPanel.Controls.Add(TabPostBackButton);
                                    chPanel.Controls.Add(new LiteralControl("</h3>"));

                                    // end of popupmodal

                                    UpdateProgress updprog = new UpdateProgress();
                                    //updprog.ID = "myupdateprogress" + nUPCount.ToString();
                                    updprog.ID = GenerateControlID("myupdateprogress", ref nUPCount);
                                    updprog.ProgressTemplate = new PLCProgressTemplate(updprog);
                                    updprog.AssociatedUpdatePanelID = chParentUpdatePanel.ID;
                                    myprec.chupdateprog = updprog;

                                    TextBox txtLookup = new TextBox();
                                    txtLookup.TabIndex = LastTabIndex;
                                    txtLookup.Enabled = allowEdit;
                                    //*AAC 061109
                                    //txtLookup.CssClass = PLCDBPanelCSS;
                                    //txtLookup.Font.Name = "Times New Roman";
                                    //txtLookup.Font.Size = 9;

                                    //txtLookup.ID = "TxtLookup" + nUPCount.ToString();
                                    txtLookup.ID = GenerateControlID("txtLookup", ref nUPCount);
                                    txtLookup.Width = 158;


                                    if (theDisplayWidth > 0)
                                        txtLookup.Width = theDisplayWidth;

                                    txtLookup.AutoPostBack = true;
                                    txtLookup.TextChanged += new EventHandler(txtLookup_TextChanged);

                                    myprec.chtb = txtLookup;
                                    myprec.chtb.Text = theInitialValue;
                                    myprec.chtb.Attributes.Clear();
                                    myprec.chtb.Attributes.Add("INITVAL", theInitialValue);
                                    myprec.chtb.Attributes.Add("CUSTOM1", theCustom1);
                                    myprec.chtb.Attributes.Add("CUSTOM2", theCustom2);
                                    myprec.chtb.Attributes.Add("LINKEDCODETABLE", theCodeTable);
                                    myprec.chtb.Attributes.Add("LINKEDFIELDNAME", theFldName);

                                    HiddenField hdnPostBack = new HiddenField();
                                    hdnPostBack.ID = GenerateControlID("hdnPostBack", ref nUPCount);
                                    myprec.chHdnPostBack = hdnPostBack;

                                    if (sPostBack == "T")
                                    {
                                        hdnPostBack.Value = "1";
                                    }

                                    ////This code is for adding postback functionality in a code head (you have to set DBPANEL.POSTBACK to 'T')
                                    //if (sPostBack == "T")
                                    //{
                                    //    Button SelectPostBackButton = new Button();
                                    //    SelectPostBackButton.ID = GenerateControlID("SelectPostBackButton", ref nUPCount);
                                    //    SelectPostBackButton.Attributes.Add("LINKEDCODETABLE", theCodeTable);
                                    //    SelectPostBackButton.Text = "Cancel";
                                    //    SelectPostBackButton.Width = 1;
                                    //    SelectPostBackButton.Height = 1;
                                    //    SelectPostBackButton.Click += new EventHandler(SelectPostBackButton_Click);
                                    //    myprec.chSelectPostBackButton = SelectPostBackButton;
                                    //    tc.Controls.Add(SelectPostBackButton);
                                    //}

                                    mymaskedit = null;
                                    if (theEditMask.Trim().Length > 0)
                                    {
                                        mymaskedit = new AjaxControlToolkit.MaskedEditExtender();
                                        mymaskedit.ClearMaskOnLostFocus = true;
                                        //mymaskedit.ID = "chMaskEdit_" + nUPCount.ToString();
                                        mymaskedit.ID = GenerateControlID("chMaskEdit_", ref nUPCount);
                                        mymaskedit.MaskType = AjaxControlToolkit.MaskedEditType.None;
                                        mymaskedit.Mask = theEditMask;
                                        mymaskedit.TargetControlID = txtLookup.ID;
                                    }

                                    ImageButton imgLookup = new ImageButton();
                                    //imgLookup.ID = "ImgLookup" + imgLookup.ClientID.ToString();
                                    imgLookup.ID = GenerateControlID("ImgLookup", ref nUPCount);
                                    imgLookup.Height = 16;
                                    imgLookup.Width = 20;
                                    imgLookup.Click += new ImageClickEventHandler(ImgLookup_Click);
                                    imgLookup.ImageUrl = "~/Images/Question.PNG";
                                    LastTabIndex++;
                                    tabCount++;
                                    imgLookup.TabIndex = LastTabIndex;
                                    imgLookup.Enabled = allowEdit;
                                    //                               
                                    myprec.chib = imgLookup;

                                    Label lblLookup = new Label();
                                    //if ((theCodeTable == "ANALYST") && (sBumpNextFieldUp == "T"))
                                    //{
                                    //    //lblLookup.ID = "LblLookupDummy" + nUPCount.ToString();
                                    //    lblLookup.ID = GenerateControlID("LblLookupDummy", ref nUPCount);
                                    //    lblLookup.Text = "";
                                    //    //*AAC 061009
                                    //    //lblLookup.Style.Value = "font-family: Arial; font-size:smaller; color: blue";
                                    //    //lblLookup.Height = 1;
                                    //    //lblLookup.Width = 1;
                                    //    //lblLookup.CssClass = PLCDBPanelCSS;
                                    //    lblLookup.Visible = true;
                                    //    txtLookup.Width = 100;
                                    //    myprec.chlabel = lblLookup;

                                    //    tbn = new TextBox();

                                    //    tbn.Text = theInitialValue;
                                    //    //tbn.ID = "LblLookup" + tbn.ClientID.ToString();
                                    //    tbn.ID = GenerateControlID("LblLookup", ref nUPCount);
                                    //    myprec.DescCtrl = tbn;
                                    //}
                                    //else
                                    //{
                                    //lblLookup.ID = "LblLookup" + nUPCount.ToString();
                                    lblLookup.ID = GenerateControlID("LblLookup", ref nUPCount);
                                    lblLookup.Text = "";
                                    if (hideDescription)
                                    {
                                        lblLookup.Style.Value = "display: none;";
                                    }
                                    else
                                    {
                                        lblLookup.Style.Value = "padding-left:4px;";
                                    }
                                    //*AAC 061009
                                    //lblLookup.Style.Value = "font-family: Arial; font-size:smaller; color: blue";
                                    //lblLookup.CssClass = PLCDBPanelCSS;
                                    myprec.chlabel = lblLookup;
                                    //}

                                    Button DummyButton = new Button();
                                    //DummyButton.ID = "DummyButton" + nUPCount.ToString();
                                    DummyButton.ID = GenerateControlID("DummyButton", ref nUPCount);
                                    DummyButton.Style.Value = "display: none;visible:false;";

                                    AjaxControlToolkit.ModalPopupExtender mpeCodeHead = new AjaxControlToolkit.ModalPopupExtender();
                                    //mpeCodeHead.ID = "mpeCodeHead" + nUPCount.ToString();
                                    mpeCodeHead.ID = GenerateControlID("mpeCodeHead", ref nUPCount);

                                    //mpeCodeHead.TargetControlID = "DummyButton" + nUPCount.ToString();
                                    mpeCodeHead.TargetControlID = DummyButton.ID.ToString();
                                    mpeCodeHead.PopupControlID = chPanel.ID.ToString();
                                    //mpeCodeHead.BackgroundCssClass = "modalBackground";


                                    mpeCodeHead.OkControlID = OkButton.ID.ToString();
                                    mpeCodeHead.CancelControlID = CancelButton.ID.ToString();
                                    mpeCodeHead.OnOkScript = "onOk()";
                                    mpeCodeHead.PopupDragHandleControlID = chCaptionPanel.ID.ToString();

                                    chPanel.Controls.Add(mpeCodeHead);

                                    myprec.chmpe = mpeCodeHead;
                                    myprec._codetable = theCodeTable;

                                    chParentUpdatePanel.ContentTemplateContainer.Controls.Add(updprog);
                                    chParentUpdatePanel.ContentTemplateContainer.Controls.Add(txtLookup);
                                    chParentUpdatePanel.ContentTemplateContainer.Controls.Add(hdnPostBack);
                                    if (mymaskedit != null)
                                        chParentUpdatePanel.ContentTemplateContainer.Controls.Add(mymaskedit);
                                    chParentUpdatePanel.ContentTemplateContainer.Controls.Add(imgLookup);
                                    chParentUpdatePanel.ContentTemplateContainer.Controls.Add(lblLookup);
                                    //if ((theCodeTable == "ANALYST") && (sBumpNextFieldUp == "T") && (tbn != null))
                                    //    chParentUpdatePanel.ContentTemplateContainer.Controls.Add(tbn);

                                    // Put lookup modal popup in a separate update panel.
                                    UpdatePanel chLookupUpdatePanel = new UpdatePanel();
                                    chLookupUpdatePanel.ID = GenerateControlID("LookupUpdatePanel", ref nUPCount);
                                    chLookupUpdatePanel.UpdateMode = UpdatePanelUpdateMode.Conditional;
                                    chLookupUpdatePanel.ContentTemplateContainer.Controls.Add(DummyButton);
                                    chLookupUpdatePanel.ContentTemplateContainer.Controls.Add(chPanel);
                                    myprec.lookupUpdatePanel = chLookupUpdatePanel;

                                    wrapper.Controls.Add(chLookupUpdatePanel);
                                    wrapper.Controls.Add(chParentUpdatePanel);
                                    wrapper.Controls.Add(myprec.ErrMsg);
                                    wrapper.Controls.Add(myprec.StatusMsg);

                                    PLCSession.WriteDebug("wrapper added:", true);
                                    tc.Controls.Add(wrapper);

                                    //*******END CODEHEAD
                                break;

                            default:
                                PLCSession.WriteDebug("default switch, !!!!!!!!!!!!!!!!!:"  + attributes.CodeHeadType.ToString() , true);
                                break;
                            //else
                            //    tc.Controls.Add(myprec.ErrMsg);
                        }
                    }
                }

                if (tb != null && theCodeTable == "")
                {
                    if (sPostBack == "T")
                    {
                        //postback when textbox value is changed
                        tb.Attributes.Add("FIELDNAME", theFldName);
                        tb.AutoPostBack = true;
                        tb.TextChanged += new EventHandler(tb_TextChanged);
                    }
                }

                myprec.original.ID = this.ID + "_hdn" + theFldName + ctrlnum + "_original";
                tc.Controls.Add(myprec.original);
                myprec.originalDesc.ID = this.ID + "_hdn" + theFldName + ctrlnum + "_originalDesc";
                tc.Controls.Add(myprec.originalDesc);

                tr.Cells.Add(tc);

                if (hideField)
                {
                    tr.Style.Value = "visibility: hidden; display: none;";
                }

                if (!string.IsNullOrEmpty(separatorText))
                {
                    TableRow trSeparator = new TableRow();
                    trSeparator.ID = this.ID + "_tr" + theFldName + ctrlnum + "_separatortext";
                    trSeparator.CssClass = "dbpanel-separator";
                    trSeparator.Cells.Add(new TableCell() { Text = separatorText, ColumnSpan = 10 });
                    tbl.Rows.Add(trSeparator);
                }

                tr.ID = this.ID + "_tr" + theFldName + ctrlnum.ToString();
                tbl.Rows.Add(tr);
                if (tabCount > this.HighestTabIndex)
                    this.HighestTabIndex = tabCount;

                if (hasSeparator)
                {
                    TableRow trSeparator = new TableRow();
                    trSeparator.ID = this.ID + "_tr" + theFldName + ctrlnum + "_separator";
                    trSeparator.Controls.Add(new TableCell() { Text = "<hr/>", ColumnSpan = 10 });
                    tbl.Rows.Add(trSeparator);
                }
            }

            foreach (KeyValuePair<object, string> fb in FlexBoxControlsWithParent)
            {
                string codeCondition = fb.Value;
                if (codeCondition.Contains("{") && codeCondition.Contains("}"))
                {
                    int startIndex = codeCondition.IndexOf("{") + 1;
                    int endIndex = codeCondition.IndexOf("}");
                    string fieldName = codeCondition.Substring(startIndex, endIndex - startIndex);

                    FlexBox parentFlexBox = GetFlexBoxControl(fieldName);

                    FlexBox flexBox = (FlexBox)fb.Key;

                    if (parentFlexBox != null)
                        flexBox.ParentFlexBox = parentFlexBox;
                    else
                        flexBox.ParentControl = GetControl(fieldName);

                    flexBox.CodeCondition = codeCondition;

                    AssignFlexBoxParentControls(flexBox);
                }
                else
                {
                    int startIndex = codeCondition.IndexOf("[") + 1;
                    int endIndex = codeCondition.IndexOf("]");
                    string fieldName = codeCondition.Substring(startIndex, endIndex - startIndex);

                    FlexBox parentFlexBox = GetFlexBoxControl(fieldName);

                    FlexBox flexBox = (FlexBox)fb.Key;
                    flexBox.ParentFlexBox = parentFlexBox;
                    flexBox.CodeCondition = codeCondition;
                }
            }

            foreach (var sig in SignatureControlsWithName)
            {
                string nameField = sig.Value;
                if (nameField.StartsWith("{") && nameField.EndsWith("}"))
                {
                    var signature = (Signature)sig.Key;
                    var parentPR = GetPanelRecByFieldName(nameField.Trim(new char[] { '{', '}' }));
                    signature.TargetControl = (parentPR.tb != null ? (Control)parentPR.tb : parentPR.chFlexBox);
                    signature.NameRequired = parentPR.required == "T";
                }
            }

            // done with buttonpanel
            //thereader.Close();
            //thereader.Dispose();

            Literal myLiteral = null;

            myLiteral = new Literal();
            myLiteral.Text = "<P> </P>";
            this.Controls.Add(myLiteral);

            if (PLCDisplayTopBorder)
            {

                myLiteral = new Literal();
                myLiteral.Text = "<hr>";
                this.Controls.Add(myLiteral);

            }

            //*AAC 0601509
            //tbl.CssClass = PLCDBPanelCSS;

            this.Controls.Add(tbl);

            if (PLCDisplayBottomBorder)
            {
                myLiteral = new Literal();
                myLiteral.Text = "<hr>";
                this.Controls.Add(myLiteral);
            }

            foreach (PanelRec pr in panelrecs)
            {
                if (!string.IsNullOrEmpty(pr.duplicateValues))
                {
                    if (pr.chFlexBox != null)
                        pr.chFlexBox.Attributes.Add("DUPLICATEVALUES", "fb|" + pr.duplicateValues);
                }
            }

            List<string> baseFieldControlsToValidate = new List<string>();

            foreach (KeyValuePair<string, string> mb in MandatoryBases)
            {
                ParseMandatoryBaseValues(mb.Key, mb.Value, ref baseFieldControlsToValidate);
            }

            BindOnChangeEventToControls(baseFieldControlsToValidate);

            // ADD_BASE
            List<string> addFieldControlsToValidate = new List<string>();
            foreach (KeyValuePair<string, string> mb in AddFieldBases)
            {
                PanelRec rec = this.GetPanelRecByFieldName(mb.Key);
                if (rec != null && rec.AllowAdd)
                {
                    ParseMandatoryBaseValues(mb.Key, mb.Value, ref addFieldControlsToValidate);
                }
            }
            if (addFieldControlsToValidate.Count > 0)
                BindOnChangeEventToAddBases("ADD", addFieldControlsToValidate);

            // EDIT_BASE
            List<string> editFieldControlsToValidate = new List<string>();
            foreach (KeyValuePair<string, string> mb in EditFieldBases)
            {
                PanelRec rec = this.GetPanelRecByFieldName(mb.Key);
                if (rec != null && rec.AllowEdit)
                {
                    ParseMandatoryBaseValues(mb.Key, mb.Value, ref editFieldControlsToValidate);
                }
            }

            if (editFieldControlsToValidate.Count > 0)
                BindOnChangeEventToAddBases("EDIT", editFieldControlsToValidate);
        }

        /*private string GetDuplicateClientIDs(string duplicateValues)
        {
            string ids = "";
            foreach (PanelRec pr in panelrecs)
            {
                if (pr.duplicateValues == duplicateValues)
                {
                    if (pr.tb != null)
                        ids += pr.tb.ClientID + ",";
                    else if (pr.ddl != null)
                        ids += pr.ddl.ClientID + ",";
                    else if (pr.chtb != null)
                        ids += pr.chtb.ClientID + ",";
                    else if (pr.chComboBox != null)
                        ids += pr.chComboBox.ClientID + ",";
                    else if (pr.chFlexBox != null)
                        ids += pr.chFlexBox.ClientID + ",";
                    else if (pr.deptPersCH != null)
                        ids += pr.deptPersCH.ClientID + ",";
                }
            }
            return ids.Remove(ids.Length - 1);
        }*/

        void lbtn_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private int GetColSpanForPackedField(DataTable fieldlst)
        {
            int colSpan = 0;
            int consecutiveHits = 0;

            foreach (DataRow dr in fieldlst.Rows)
            {
                if (dr["BUMP_NEXT_FIELD_UP"].ToString() == "P")
                {
                    consecutiveHits++;
                }
                else
                {
                    if (colSpan < consecutiveHits)
                    {
                        colSpan = consecutiveHits;
                    }
                    consecutiveHits = 0;
                }
            }

            return (colSpan * 2) + 1;
        }

        void flexBox_ValueChanged(object sender, EventArgs e)
        {
            FlexBox flexBox = (FlexBox)sender;
            string linkedCodeTable = flexBox.Attributes["LINKEDCODETABLE"];
            string linkedFieldName = flexBox.Attributes["LINKEDFIELDNAME"];

            PLCDBPanelCodeHeadChangedEventArgs codeHeadEventArgs = new PLCDBPanelCodeHeadChangedEventArgs(linkedCodeTable,linkedFieldName);
            OnPLCDBPanelCodeHeadChanged(codeHeadEventArgs);
        }

        void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            TADropDown comboBox = (TADropDown)sender;
            string linkedCodeTable = comboBox.Attributes["LINKEDCODETABLE"];
            string linkedFieldName = comboBox.Attributes["LINKEDFIELDNAME"];

            PLCDBPanelCodeHeadChangedEventArgs codeHeadEventArgs = new PLCDBPanelCodeHeadChangedEventArgs(linkedCodeTable,linkedFieldName);
            OnPLCDBPanelCodeHeadChanged(codeHeadEventArgs);
        }

        void multiPick_ValueChanged(object sender, EventArgs e)
        {
            CodeMultiPick multipick = (CodeMultiPick)sender;
            string linkedCodeTable = multipick.CodeTable;
            string linkedFieldName = multipick.CodeCol;
            

            PLCDBPanelCodeHeadChangedEventArgs codeHeadEventArgs = new PLCDBPanelCodeHeadChangedEventArgs(linkedCodeTable, linkedFieldName);
            OnPLCDBPanelCodeHeadChanged(codeHeadEventArgs);
        }

        /* --- Ticket#40993 Multipick Support for DBPanel --- */
        private void RefreshMultipickControls()
        {
            foreach (PanelRec pr in panelrecs)
            {
                if (pr.chMultiLookup != null)
                {
                    this.SetPanelCodeCondition(pr.fldname, this.FormatCodeCondition(pr.codecondition));
                }
            }
        }

        //private void CodeHeadGetKey(PanelRec pr, string code)
        //{
        //    try
        //    {
        //        PLCQuery qryCodeTable = new PLCQuery();
        //        qryCodeTable.SQL = "SELECT " + pr.CodeTablePrimaryKeyField + ", " + pr._desc + " FROM " + pr.codetable + " WHERE " + pr._code + " = '" + code + "'";
        //        qryCodeTable.Open();
        //        string keyValue = "";
        //        string description = "";
        //        if (qryCodeTable.PLCDataTable.Rows.Count > 1)
        //        {
        //            var latest = (from a in qryCodeTable.PLCDataTable.AsEnumerable()
        //                          orderby a.Field<int>(pr.CodeTablePrimaryKeyField) descending
        //                          select a).FirstOrDefault();

        //            keyValue = latest.Field<int>(pr.CodeTablePrimaryKeyField).ToString();
        //            description = latest.Field<string>(pr._desc);
        //        }
        //        else
        //        {
        //            keyValue = qryCodeTable.FieldByName(pr.CodeTablePrimaryKeyField);
        //            description = qryCodeTable.FieldByName(pr._desc);
        //        }
        //else
        //{ 
        //    keyValue = "-1";
        //    description = "";
        //}

        //        //pr.hdnKey.Value = keyValue;
        //        pr.chlabel.Text = description;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        private int CurrentPanelIndex = -1;

        void txtLookup_TextChanged(object sender, EventArgs e)
        {
            TextBox txt = (TextBox)sender;
            string linkedCodeTable = txt.Attributes["LINKEDCODETABLE"];
            string linkedFieldName = txt.Attributes["LINKEDFIELDNAME"];

            PLCDBPanelCodeHeadChangedEventArgs codeHeadEventArgs = new PLCDBPanelCodeHeadChangedEventArgs(linkedCodeTable,linkedFieldName);
            OnPLCDBPanelCodeHeadChanged(codeHeadEventArgs);

            int i = 0;
            foreach (PanelRec pr in panelrecs)
            {
                //if ((pr.chSelectPostBackButton != null) && (pr.chSelectPostBackButton.ID == button.ID))
                //{
                // Update lookup code description when lookup edit box text is changed and set focus to lookup button.
                if ((pr.chtb != null) && (pr.chtb.ID == txt.ID))
                {
                    CodeHeadSetLabel(pr, GetCodeDesc(pr._codetable, pr.CodeTablePrimaryKeyField, pr.chtb.Text, pr._desc, pr.codecondition));
                    CurrentPanelIndex = i;
                    break;
                }

                i++;
            }


            //$$ No need to refresh the main update panel as updates can be done in the custom event handlers.
            //   See QuickCreate_Advanced.PLCDBPanel1_PLCDBPanelTextChanged() for a sample.
            //            if (PLCDBPanelCodeHeadChanged != null)
            //            {
            //                if (this.Page.Master.FindControl("UpdatePanel1") != null)
            //                {
            //                    ((UpdatePanel)this.Page.Master.FindControl("UpdatePanel1")).Update();
            //                }
            //            }
        }

        public void RedrawCodeHeadLabels()
        {
            foreach (PanelRec pr in this.panelrecs)
            {
                if ((pr.chtb != null) && (pr._codetable != null))
                    CodeHeadSetLabel(pr, GetCodeDesc(pr._codetable, pr.CodeTablePrimaryKeyField, pr.CodeTablePrimaryKeyData, pr._desc, pr.codecondition));
            }
        }

        /// <summary>
        /// Generate Control ID
        /// </summary>
        /// <param name="controlName">Control Name</param>
        /// <param name="controlIndex">Index</param>
        /// <returns>Control ID</returns>
        private string GenerateControlID(string controlName, ref int controlIndex)
        {
            string controlID = controlName + controlIndex.ToString();
            while (this.FindControl(controlID) != null)
            {
                controlIndex++;
                controlID = controlName + controlIndex.ToString();
            }

            return controlID;
        }

        protected void BtnSearch_Click(Object sender, EventArgs e)
        {
            Button btnSearch = (Button)sender;
            foreach (PanelRec pr in panelrecs)
            {
                if ((pr.chbtnSearch != null) && (pr.chbtnSearch.ID == btnSearch.ID))
                {
                    string searchText = pr.chtxtSearch.Text;

                    if (searchText.Contains('\''))
                        searchText = searchText.Replace("'", "''");

                    if (pr.chActive.Checked)
                        pr.chSqlDataSource.SelectCommand = "SELECT " + pr._codedesc + " FROM " + pr._codetable + " " +
                                                    "WHERE (upper(" + pr._code + ") LIKE upper('%" + searchText + "%') OR " +
                                                    "upper(" + pr._desc + ") LIKE upper('%" + searchText + "%')) AND (ACTIVE != 'F' OR ACTIVE is null)";
                    else
                        pr.chSqlDataSource.SelectCommand = "SELECT " + pr._codedesc + " FROM " + pr._codetable + " " +
                                                    "WHERE (upper(" + pr._code + ") LIKE upper('%" + searchText + "%') OR " +
                                                    "upper(" + pr._desc + ") LIKE upper('%" + searchText + "%'))";

                    if (pr.codecondition != "")
                    {
                        string CodeConditionStr = FormatCodeCondition(pr.codecondition);
                        if (CodeConditionStr != "")
                        {
                            pr.chSqlDataSource.SelectCommand += " AND " + CodeConditionStr;
                        }
                    }

                    pr.chSqlDataSource.SelectCommand += SortClause(pr.chSortOrder, pr._code, pr._desc, pr._codetable);

                    pr.chSqlDataSource.DataBind();

                    pr.chGridView.DataBind(); //bind gridview to get total pagecount
                    pr.chGridView.AllowPaging = (pr.chGridView.PageCount > 4 || pr.chGridView.Rows.Count > 40); //if more than 40 records, allow paging
                    pr.chPnlGrid.CssClass = ((pr.chGridView.PageCount > 1 && pr.chGridView.PageCount <= 4) || (pr.chGridView.Rows.Count > 10 && pr.chGridView.Rows.Count <= 40)) ? "lookupscroll" : ""; //show scrollbar for 11-40 records
                }
            }

        }

        private string SortClause(string sortOrder, string codeField, string descField, string tableName)
        {
            string sortClause = "";

            switch (sortOrder)
            {
                case "C":
                    sortClause = " ORDER BY " + codeField;
                    break;
                case "D":
                    sortClause = " ORDER BY " + descField;
                    break;
                case "U":
                    PLCQuery qryUserField = new PLCQuery();
                    qryUserField.SQL = "SELECT * FROM " + tableName + " WHERE 1 = 0";
                    qryUserField.Open();
                    if (qryUserField.FieldExist("USER_RES"))
                        sortClause = " ORDER BY USER_RES";
                    else
                        sortClause = " ORDER BY " + codeField;
                    break;
                default:
                    sortClause = " ORDER BY " + codeField;
                    break;
            }

            return sortClause;
        }

        protected void ImgLookup_Click(Object sender, ImageClickEventArgs e)
        {

            ImageButton imgbtn = (ImageButton)sender;
            foreach (PanelRec pr in panelrecs)
            {
                if ((pr.chib != null) && (pr.chib.ID == imgbtn.ID))
                {
                    // fix label postback reset issue
                    string sCode = pr.CodeTablePrimaryKeyData;
                    // remove mask underscore first
                    while ((sCode.Length > 0) && (sCode[sCode.Length - 1] == '_'))
                        sCode = sCode.Remove(sCode.Length - 1, 1);
                    string sDesc = GetCodeDesc(pr.codetable, pr.CodeTablePrimaryKeyField, sCode, pr._desc, pr.codecondition);
                    if (pr.DescCtrl == null)
                        pr.chlabel.Text = sDesc;
                    else
                    {
                        pr.chlabel.Text = "";
                        pr.DescCtrl.Text = sDesc;
                        if (sCode != "")
                            pr.DescCtrl.ReadOnly = true;
                        else
                            pr.DescCtrl.ReadOnly = false;
                    }

                    pr.chtxtSearch.Text = "";
                    pr.chActive.Enabled = ActiveFieldExists(pr._codetable);
                    pr.chActive.Checked = pr.chActive.Enabled;

                    pr.chmpe.Show();
                    RefreshGrid(pr);

                    // Refresh lookup update panel so that binding gets displayed.
                    if (pr.lookupUpdatePanel != null)
                        pr.lookupUpdatePanel.Update();
                }
            }
        }

        //protected void TabPostBackButton_Click(Object sender, EventArgs e)
        //{
        //    Button btnTabPost = (Button)sender;
        //    foreach (PanelRec pr in panelrecs)
        //    {
        //        if ((pr.chTabPostBackButton != null) && (pr.chTabPostBackButton.ID == btnTabPost.ID))
        //        {
        //            string sCode = pr.chtb.Text;
        //            // remove mask underscore first
        //            while ((sCode.Length > 0) && (sCode[sCode.Length - 1] == '_'))
        //                sCode = sCode.Remove(sCode.Length - 1, 1);
        //            string sDesc = GetCodeDesc(pr.codetable, sCode, pr.codecondition);
        //            pr.chtb.Text = sCode;
        //            if (pr.DescCtrl == null)
        //                pr.chlabel.Text = sDesc;
        //            else
        //            {
        //                pr.chlabel.Text = "";
        //                pr.DescCtrl.Text = sDesc;
        //                if (sCode != "")
        //                    pr.DescCtrl.Enabled = false;
        //                else
        //                    pr.DescCtrl.Enabled = true;
        //            }
        //            pr.chib.Focus();
        //        }
        //    }
        //}

        protected void GridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            GridView grd = (GridView)sender;
            string DescCtrlClientID = "";
            foreach (PanelRec pr in panelrecs)
            {
                if ((pr.chGridView != null) && (pr.chGridView.ID == grd.ID))
                {
                    if (e.Row.RowType == DataControlRowType.DataRow)
                    {

                        e.Row.Attributes["onmouseover"] = "this.style.cursor='hand';";
                        if (pr.attribtype == "M")
                        {
                            e.Row.Attributes.Add("onclick", "onGridViewMultiRowSelected('" + pr.chGridView.ClientID + "', '" +
                                              pr.chtb.ClientID + "', '" + pr.chlabel.ClientID + "', '" +
                                              pr.chCancelButton.ClientID + "', '" + Convert.ToString(e.Row.RowIndex + 1) + "', '" + pr.attribtype + "')");
                            //e.Row.Attributes.Add("ondblclick", "onGridViewMultiRowDoubleClicked('" + pr.chGridView.ClientID + "', '" +
                            //                  pr.chtb.ClientID + "', '" + pr.chlabel.ClientID + "', '" +
                            //                  pr.chCancelButton.ClientID + "', '" + pr.chOkButton.ClientID + "', '" + Convert.ToString(e.Row.RowIndex + 1) + ", '" + pr.attribtype + "')");
                        }
                        else
                        {
                            DescCtrlClientID = "";

                            string hdnPostBackId = string.Empty;

                            //if (pr.chSelectPostBackButton != null)
                            //{
                            //    sPostBackButtonId = pr.chSelectPostBackButton.ClientID;
                            //}
                            if (pr.chHdnPostBack != null)
                            {
                                hdnPostBackId = pr.chHdnPostBack.ClientID;
                            }

                            string codeKey = "null, null";

                            if (pr.DescCtrl != null)
                                DescCtrlClientID = pr.DescCtrl.ClientID;
                            e.Row.Attributes.Add("onclick", "onGridViewRowSelected('" + pr.chGridView.ClientID + "', '" +
                                              pr.chtb.ClientID + "', '" + pr.chlabel.ClientID + "', '" +
                                              pr.chCancelButton.ClientID + "', '" + Convert.ToString(e.Row.RowIndex + 1) + "', '" + pr.attribtype + "', '" + DescCtrlClientID + "', '" + hdnPostBackId + "'," + codeKey + ")");
                            e.Row.Attributes.Add("ondblclick", "onGridViewRowDoubleClicked('" + pr.chGridView.ClientID + "', '" +
                                              pr.chtb.ClientID + "', '" + pr.chlabel.ClientID + "', '" +
                                              pr.chCancelButton.ClientID + "', '" + pr.chOkButton.ClientID + "', '" + Convert.ToString(e.Row.RowIndex + 1) + "', '" + DescCtrlClientID + "', '" + hdnPostBackId + "'," + codeKey + ")");
                        }
                    }
                }
            }

        }

        protected void GridView_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridView grd = (GridView)sender;

            foreach (PanelRec pr in panelrecs)
            {
                if ((pr.chGridView != null) && (pr.chGridView.ID == grd.ID))
                {
                    RefreshGrid(pr);
                }
            }
        }

        protected void GridView_Sorting(object sender, GridViewSortEventArgs e)
        {
            string strSortDir = null;
            switch (e.SortDirection)
            {
                case SortDirection.Ascending:
                    strSortDir = "ASC";
                    break;

                case SortDirection.Descending:
                    strSortDir = "DESC";
                    break;
            }

            GridView grd = (GridView)sender;
            string CodeConditionStr = "";

            foreach (PanelRec pr in panelrecs)
            {
                if ((pr.chGridView != null) && (pr.chGridView.ID == grd.ID))
                {
                    if (pr.chtxtSearch.Text != "")
                    {

                        if (pr.codecondition != "")
                        {
                            CodeConditionStr = FormatCodeCondition(pr.codecondition);
                            if (CodeConditionStr != "")
                            {
                                CodeConditionStr = " AND " + CodeConditionStr;
                            }
                        }

                        if (pr.chActive.Checked)
                            pr.chSqlDataSource.SelectCommand = "SELECT " + pr._codedesc + " FROM " + pr._codetable + " " +
                                                        "WHERE (upper(" + pr._code + ") LIKE upper('%" + pr.chtxtSearch.Text + "%') OR " +
                                                        "upper(" + pr._desc + ") LIKE upper('%" + pr.chtxtSearch.Text + "%')) AND (ACTIVE != 'F' OR ACTIVE is null) " + CodeConditionStr + " ORDER BY \"" + e.SortExpression + "\" " + strSortDir;
                        else
                            pr.chSqlDataSource.SelectCommand = "SELECT " + pr._codedesc + " FROM " + pr._codetable + " " +
                                                        "WHERE (upper(" + pr._code + ") LIKE upper('%" + pr.chtxtSearch.Text + "%') OR " +
                                                        "upper(" + pr._desc + ") LIKE upper('%" + pr.chtxtSearch.Text + "%')) " + CodeConditionStr + " ORDER BY \"" + e.SortExpression + "\" " + strSortDir;
                    }
                    else
                    {
                        if (pr.chActive.Checked)
                        {
                            if (pr.codecondition != "")
                            {
                                CodeConditionStr = FormatCodeCondition(pr.codecondition);
                                if (CodeConditionStr != "")
                                {
                                    CodeConditionStr = " AND " + CodeConditionStr;
                                }
                            }

                            pr.chSqlDataSource.SelectCommand = "SELECT " + pr._codedesc + " FROM " + pr._codetable + " WHERE (ACTIVE != 'F' OR ACTIVE is null) " + CodeConditionStr + " ORDER BY \"" + e.SortExpression + "\" " + strSortDir;
                        }
                        else
                        {
                            if (pr.codecondition != "")
                            {
                                CodeConditionStr = FormatCodeCondition(pr.codecondition);
                                if (CodeConditionStr != "")
                                {
                                    CodeConditionStr = " WHERE " + CodeConditionStr;
                                }
                            }

                            pr.chSqlDataSource.SelectCommand = "SELECT " + pr._codedesc + " FROM " + pr._codetable + CodeConditionStr + " ORDER BY \"" + e.SortExpression + "\" " + strSortDir;
                        }
                    }
                }
            }
        }

        protected void chkActive_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = (CheckBox)sender;

            foreach (PanelRec pr in panelrecs)
            {
                if ((pr.chActive != null) && (pr.chActive.ID == chk.ID))
                {
                    RefreshGrid(pr);
                }
            }
        }

        protected void SqlDataSource1_Init(object sender, EventArgs e)
        {
            SqlDataSource sqlDS = (SqlDataSource)sender;

            foreach (PanelRec pr in panelrecs)
            {
                if ((pr.chSqlDataSource != null) && (pr.chSqlDataSource.ID == sqlDS.ID))
                {
                    string t = pr.chSqlDataSource.ConnectionString;
                    pr.chSqlDataSource.ProviderName = "System.Data.OleDb";

                    GetCodeDescColumns(ref pr._codetable, ref pr._code, ref pr._desc, ref pr._codedesc);

                    if (pr.chComboBox != null)
                    {
                        PopulateComboBox(pr);
                    }
                }
            }
        }

        public void GetCodeTableData(ref DataTable tbl, string connectionString, string tableName, string code, string description, string filter, string sortOrder)
        {
            string sqlStatement = "select " + code + ", " + description + " from " + tableName;

            if (tableName == "TV_ANALYST" && string.IsNullOrEmpty(filter) && !IsSearchPanel)
            {
                filter = "(ACCOUNT_DISABLED <> 'T' OR ACCOUNT_DISABLED IS NULL)";
            }

            bool activeFieldExists = ActiveFieldExists(tableName);
            if (activeFieldExists)
                sqlStatement += " WHERE (ACTIVE <> 'F' OR ACTIVE IS NULL)";

            string CodeConditionStr = FormatCodeCondition(filter);
            if (CodeConditionStr != "")
            {
                sqlStatement += (activeFieldExists ? " AND " : " WHERE ") + CodeConditionStr;
            }

            switch (sortOrder)
            {
                case "C":
                    sqlStatement += " ORDER BY " + code;
                    break;
                case "D":
                    sqlStatement += " ORDER BY " + description;
                    break;
                case "U":
                    PLCQuery qryUserField = new PLCQuery();
                    qryUserField.SQL = "SELECT * FROM " + tableName + " WHERE 1 = 0";
                    qryUserField.Open();
                    if (qryUserField.FieldExist("USER_RES"))
                        sqlStatement += " ORDER BY USER_RES";
                    else
                        sqlStatement += " ORDER BY " + code;
                    break;
                default:
                    sqlStatement += " ORDER BY " + code;
                    break;
            }

            PLCQuery qry = new PLCQuery(sqlStatement);
            qry.OpenReadOnly();

            tbl = qry.PLCDataTable;
        }

        private void PopulateComboBox(PanelRec pr)
        {
            DataTable dtRecords = null;
            GetCodeTableData(ref dtRecords, pr.chSqlDataSource.ConnectionString, pr._codetable, pr._code, pr._desc, pr.codecondition, pr.chSortOrder);
            
            pr.chComboBox.Items.Clear();
            pr.chComboBox.Items.Insert(0, new ListItem("", ""));

            if (pr.usesNullBlankOpt)
                AddNullOrBlankOptions(pr.chComboBox.Items);

            if (dtRecords != null)
            {

                foreach (DataRow row in dtRecords.Rows)
                {
                    ListItem item = new ListItem();
                    item.Text = row.Field<string>(pr._desc);
                    item.Value = row.Field<string>(pr._code);
                    item.Attributes.Add("title", item.Text);
                    pr.chComboBox.Items.Add(item);
                }
            }
        }

        protected static void GetCodeDescColumns(ref string tablename, ref string _code, ref string _desc, ref string _codedesc)
        {
            if ((tablename.Substring(0, 3) != "TV_") && (tablename.Substring(0, 3) != "CV_"))
                tablename = "TV_" + tablename;

            if (tablename == "TV_ANALYST")
            {
                _code = "ANALYST";
                _desc = "NAME";
            }
            else if (tablename == "TV_OFFENCAT")
            {
                _code = "CODE";
                _desc = "DESCRIPTION";
            }
            else if (tablename == "TV_CASETYPE")
            {
                _code = "CASE_TYPE";
                _desc = "DESCRIPTION";
            }
            else if (tablename == "TV_DEPTNAME")
            {
                _code = "DEPARTMENT_CODE";
                _desc = "DEPARTMENT_NAME";
            }
            else if (tablename == "TV_OFFENSE")
            {
                _code = "OFFENSE_CODE";
                _desc = "OFFENSE_DESCRIPTION";
            }
            else if (tablename == "TV_SUBTYPE")
            {
                _code = "TYPE_RES";
                _desc = "DESCRIPTION";
            }
            else if (tablename == "CV_ITEMCAT")
            {
                _code = "CAT_CODE";
                _desc = "CAT_CODE_DESCRIPTION";
            }
            else if (tablename == "CV_PACKTYPE")
            {
                _code = "PACKAGING_CODE";
                _desc = "DESCRIPTION";
            }
            else if (tablename == "CV_ITEMTYPE")
            {
                _code = "ITEM_TYPE";
                _desc = "DESCRIPTION";
            }
            else if (tablename == "TV_CUSTCODE")
            {
                _code = "CUSTODY_TYPE";
                _desc = "DESCRIPTION";
            }
            else if (tablename == "CV_CUSTLOC" || tablename == "TV_CUSTLOC")
            {
                _code = "LOCATION";
                _desc = "DESCRIPTION";
            }
            else if (tablename == "TV_LABCTRL")
            {
                _code = "LAB_CODE";
                _desc = "LAB_NAME";
            }
            else if (tablename == "TV_EXAMCODE")
            {
                _code = "EXAM_CODE";
                _desc = "DESCRIPTION";
            }
            else if (tablename == "TV_PRIORITY")
            {
                _code = "PRIORITY";
                _desc = "DESCRIPTION";
            }
            else if (tablename == "TV_EXAMSTAT")
            {
                _code = "EXAM_STATUS";
                _desc = "DESCRIPTION";
            }
            else if (tablename == "TV_RACECODE")
            {
                _code = "RACE";
                _desc = "DESCRIPTION";
            }
            else if (tablename == "TV_SEXCODE")
            {
                _code = "SEX";
                _desc = "DESCRIPTION";
            }
            else if (tablename == "TV_NAMETYPE")
            {
                _code = "NAME_TYPE";
                _desc = "DESCRIPTION";
            }
            else if (tablename == "TV_DEPTPERS")
            {
                _code = "ID";
                _desc = "NAME";
            }
            else if (tablename == "TV_STATES")
            {
                _code = "STATE_CODE";
                _desc = "DESCRIPTION";                
            }
            else
            {
                //PLCQuery qryDescCols = new PLCQuery();
                //qryDescCols.SQL = "SELECT * FROM " + tablename + " where 0 = 1";
                //qryDescCols.Open();


                PLCQuery qryDescCols = CacheHelper.OpenCachedSqlFieldNames("SELECT * FROM " + tablename + " WHERE 0 = 1");

                _code = qryDescCols.FieldNames(1);
                _desc = qryDescCols.FieldNames(2);
            }

            _codedesc = _code + ", " + _desc;

        }

        protected void cb_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            string linkedFieldName = checkBox.Attributes["LINKEDFIELD"];
            string linkedLabel = checkBox.Attributes["LINKEDLABEL"];
            string editmask = checkBox.Attributes["EDITMASK"];

            PLCDBPanelCheckBoxChangedEventArgs checkBoxEventArgs = new PLCDBPanelCheckBoxChangedEventArgs(linkedLabel, linkedFieldName, editmask, checkBox);
            OnPLCDBPanelCheckBoxChanged(checkBoxEventArgs);
        }

        
        public Boolean needsMask(String aMask)
        {
            String tempStr = aMask;
            while (tempStr.StartsWith("XX")) tempStr = tempStr.Replace("XX", "X");
            while (tempStr.StartsWith("!!")) tempStr = tempStr.Replace("!!", "!");
            if (tempStr == "!") return false;
            if (tempStr == "X") return false;
            return true;
        }

        public void SetMaskEditExtenderValue(string sFieldName, string sMaskValue)
        {
            foreach (PanelRec pr in panelrecs)
            {
                if (pr.fldname.ToUpper().CompareTo(sFieldName.ToUpper()) == 0)
                {
                    try
                    {
                        if (pr.mee != null)
                        {
                            pr.mee.Mask = pr.editmask = sMaskValue;
                            return;
                        }
                        else
                        {

                            if ((sFieldName.Equals("DEPARTMENT_CASE_NUMBER")) && ( needsMask(sMaskValue) ) ) 
                            {

                                MaskedEditExtender mee = new MaskedEditExtender();
                                mee.ID = pr.tb.ID + "_mee";
                                mee.TargetControlID = pr.tb.ID;
                                mee.MaskType = AjaxControlToolkit.MaskedEditType.None;
                                mee.Mask = sMaskValue;
                                mee.ClearMaskOnLostFocus = true;
                                pr.editmask = sMaskValue;
                                pr.mee = mee;
                                pr.mee.Enabled = true;
                                pr.tb.Parent.Controls.Add(mee);
                           

                            }

                        }
                    }
                    finally
                    {
                    }
                }
            }
        }

        public void SetCodeHeadValues(string sDestCodeTable, string sValue, bool bIsEnabled)
        {
            foreach (PanelRec prDest in panelrecs)
            {
                if (prDest.codetable.ToUpper().CompareTo(sDestCodeTable.ToUpper()) == 0)
                {
                    try
                    {
                        if (prDest.chtb != null)
                        {
                            prDest.chtb.Enabled = bIsEnabled;
                            prDest.chib.Enabled = bIsEnabled;
                            CodeHeadSetValue(prDest, sValue);

                            return;
                        }
                    }
                    finally
                    {
                    }
                }
            }
        }

        public string GetCodeHeadValue(string sCodeTable)
        {
            string sValue = string.Empty;

            string sSourceValue = string.Empty;

            foreach (PanelRec prSource in panelrecs)
            {
                if (prSource.codetable.ToUpper().CompareTo(sCodeTable.ToUpper()) == 0)
                {
                    try
                    {
                        if (prSource.chtb != null)
                        {
                            sValue = (prSource.chtb.Text).Replace('_', ' ').Trim();
                            return sValue;
                        }
                    }
                    finally
                    {
                    }
                }
            }

            return sValue;
        }

        public void SetLabelFieldModeEnableDisable(string sLabel, bool isEnabled)
        {
            foreach (PanelRec pr in panelrecs)
            {
                if (pr.prompt.CompareTo(sLabel) == 0)
                {
                    try
                    {
                        if (pr.tb != null)
                        {
                            pr.tb.Enabled = isEnabled;

                            if (pr.time != null)
                                SetTimeTextBoxMode(pr, !isEnabled);
                        }
                        else if (pr.ddl != null)
                        {
                            pr.ddl.Enabled = isEnabled;
                        }
                        else if (pr.rbl != null)
                        {
                            pr.rbl.Enabled = isEnabled;
                        }
                        else if (pr.chtb != null)
                        {
                            pr.chtb.Enabled = isEnabled;
                            pr.chib.Enabled = isEnabled;
                        }
                        else if (pr.chComboBox != null)
                        {
                            pr.chComboBox.Enabled = isEnabled;
                        }
                        else if (pr.chFlexBox != null)
                        {
                            pr.chFlexBox.Enabled = isEnabled;
                        }
                        else if (pr.cb != null)
                        {
                            pr.cb.Enabled = isEnabled;
                        }
                        else if (pr.chMultiLookup != null)
                        {
                            pr.chMultiLookup.Enable(isEnabled);
                        }

                        return;

                    }
                    finally
                    {
                    }
                }
            }
        }

        public void SetCheckboxEnableDisable(string sFieldName, string sCBType, bool isEnabled)
        {
            foreach (PanelRec pr in panelrecs)
            {
                try
                {
                    if (pr.cb != null)
                    {
                        if (pr.fldname.CompareTo(sFieldName) == 0 && pr.editmask.CompareTo(sCBType) == 0)
                        {
                            pr.cb.Enabled = isEnabled;
                        }
                    }
                }
                finally
                {
                }
            }
        }

        public void SetCheckboxValue(string sFieldName, string sCBType, bool isChecked)
        {
            foreach (PanelRec pr in panelrecs)
            {
                try
                {
                    if (pr.cb != null)
                    {
                        if (pr.fldname.CompareTo(sFieldName) == 0 && pr.editmask.CompareTo(sCBType) == 0)
                        {
                            pr.cb.Checked = isChecked;
                        }
                    }
                }
                finally
                {
                }
            }
        }

        //protected string FormatCodeCondition(string condstr)
        //{
        //    string sField = "";
        //    int nPos1;
        //    int nPos2;

        //    nPos1 = condstr.IndexOf("'");
        //    if (nPos1 > 0)  // Constant Value
        //    {
        //      return condstr;
        //    }
        //    else
        //    {
        //        nPos1 = condstr.IndexOf("{");
        //        if (nPos1 > 0)
        //        {
        //            nPos2 = condstr.IndexOf("}");
        //            if (nPos2 > 0)
        //            {
        //                sField = condstr.Substring(nPos1 + 1, nPos2 - nPos1 - 1);
        //            }
        //            else
        //                return "";
        //        }
        //        else
        //            return "";

        //        if (sField != "")
        //        {
        //            string sVal = getpanelfield(sField);
        //            condstr = condstr.Replace("{" + sField + "}", "'" + sVal + "'");
        //            return condstr;
        //        }
        //        else
        //            return "";

        //    }
        //}

        private string FormatCodeCondition(string codeCondition, bool processEmptyParent = false)
        {
            codeCondition = PLCSession.ReplaceSpecialKeysInCodeCondition(codeCondition);

            if (codeCondition.Contains("{") && codeCondition.Contains("}"))
            {
                do
                {
                    int start = codeCondition.IndexOf("{") + 1;
                    int end = codeCondition.IndexOf("}", start);
                    string parentField = codeCondition.Substring(start, end - start);
                    string parentValue = this.getpanelfield(parentField);
                    if (parentField.Contains(".") && string.IsNullOrEmpty(parentValue))
                    {
                        var field = parentField.Split('.');
                        parentValue = this.getpanelfield(field[0], field[1]);
                    }

                    if (parentValue != "" || processEmptyParent)
                        codeCondition = codeCondition.Replace("{" + parentField + "}", "'" + parentValue + "'");
                    else
                        return "";
                } while (codeCondition.Contains("{") && codeCondition.Contains("}"));

                return codeCondition;
            }
            //*AAC 11232010
            else if (codeCondition.Contains("[") && codeCondition.Contains("]"))
            {
                int start = codeCondition.IndexOf("[") + 1;
                int end = codeCondition.IndexOf("]", start);
                string parentField = codeCondition.Substring(start, end - start);
                string parentValue = getpanelfield(parentField);
                return codeCondition.Replace("[" + parentField + "]", parentValue);
            }
            else
                return codeCondition;
        }

        protected void RefreshGrid(PanelRec pr)
        {
            // simulate delay
            //System.Threading.Thread.Sleep(5000);

            if (!PLCDisableCaching)
            {
                pr.chSqlDataSource.EnableCaching = true;
                pr.chSqlDataSource.CacheExpirationPolicy = DataSourceCacheExpiry.Absolute;
                pr.chSqlDataSource.CacheDuration = 600;   // 10 mins
            }
            else
                pr.chSqlDataSource.EnableCaching = false;


            try
            {
                if (pr.chtxtSearch.Text != "")
                {
                    if (pr.chActive.Checked)
                        pr.chSqlDataSource.SelectCommand = "SELECT " + pr._codedesc + " FROM " + pr._codetable + " " +
                            "WHERE (upper(" + pr._code + ") LIKE upper('%" + pr.chtxtSearch.Text + "%') OR " +
                            "upper(" + pr._desc + ") LIKE upper('%" + pr.chtxtSearch.Text + "%')) AND (ACTIVE != 'F' OR ACTIVE is null)";
                    else
                        pr.chSqlDataSource.SelectCommand = "SELECT " + pr._codedesc + " FROM " + pr._codetable + " " +
                            "WHERE (upper(" + pr._code + ") LIKE upper('%" + pr.chtxtSearch.Text + "%') OR " +
                            "upper(" + pr._desc + ") LIKE upper('%" + pr.chtxtSearch.Text + "%'))";
                    if (pr.codecondition != "")
                    {
                        string CodeConditionStr = FormatCodeCondition(pr.codecondition);
                        if (CodeConditionStr != "")
                        {
                            pr.chSqlDataSource.SelectCommand += " AND " + CodeConditionStr;
                        }
                    }

                }
                else
                {
                    if (pr.chActive.Checked)
                    {
                        pr.chSqlDataSource.SelectCommand = "SELECT " + pr._codedesc + " FROM " + pr._codetable + " WHERE (ACTIVE != 'F' OR ACTIVE is null)";

                        if (pr.codecondition != "")
                        {
                            string CodeConditionStr = FormatCodeCondition(pr.codecondition);
                            if (CodeConditionStr != "")
                            {
                                pr.chSqlDataSource.SelectCommand += " AND " + CodeConditionStr;
                            }
                        }
                    }
                    else
                    {
                        pr.chSqlDataSource.SelectCommand = "SELECT " + pr._codedesc + " FROM " + pr._codetable;

                        if (pr.codecondition != "")
                        {
                            string CodeConditionStr = FormatCodeCondition(pr.codecondition);
                            if (CodeConditionStr != "")
                            {
                                pr.chSqlDataSource.SelectCommand += " WHERE " + CodeConditionStr;
                            }
                        }
                    }
                }

                pr.chSqlDataSource.SelectCommand += SortClause(pr.chSortOrder, pr._code, pr._desc, pr._codetable);
                pr.chSqlDataSource.DataBind();
            }
            catch
            {
            }

            pr.chGridView.DataBind(); //bind gridview to get total pagecount
            pr.chGridView.AllowPaging = (pr.chGridView.PageCount > 4 || pr.chGridView.Rows.Count > 40); //if more than 40 records, allow paging
            pr.chPnlGrid.CssClass = ((pr.chGridView.PageCount > 1 && pr.chGridView.PageCount <= 4) || (pr.chGridView.Rows.Count > 10 && pr.chGridView.Rows.Count <= 40)) ? "lookupscroll" : ""; //show scrollbar for 11-40 records
        }

        protected override void OnPreRender(EventArgs e)
        {
            //add this style for the calendar extender popup
            //solves overlapping-calendar-on-flexbox bug
            if (this.FindControl("calendarstyle") == null)
            {
                System.Web.UI.HtmlControls.HtmlGenericControl style = new HtmlGenericControl("style");
                style.ID = "calendarstyle";
                style.Attributes.Add("type", "text/css");
                style.InnerText = ".ajax__calendar_container { z-index : 1000 ; }";
                this.Controls.Add(style);
            }

            base.OnPreRender(e);

            SetFocusOnNextField();
        }

        private void SetFocusOnNextField()
        {
            if (CurrentPanelIndex > -1)
            {
                for (int i = CurrentPanelIndex + 1; i < panelrecs.Count; i++)
                {
                    PanelRec pr = panelrecs[i];
                    if (pr.tb != null && pr.tb.Enabled)
                    {
                        pr.tb.Focus();
                        break;
                    }
                    else if (pr.ddl != null && pr.ddl.Enabled)
                    {
                        pr.ddl.Focus();
                        break;
                    }
                    else if (pr.rbl != null && pr.rbl.Enabled)
                    {
                        pr.rbl.Focus();
                        break;
                    }
                    else if (pr.chtb != null && pr.chtb.Enabled)
                    {
                        pr.chtb.Focus();
                        break;
                    }
                    else if (pr.chComboBox != null && pr.chComboBox.Enabled)
                    {
                        pr.chComboBox.Focus();
                        break;
                    }
                    else if (pr.chFlexBox != null && pr.chFlexBox.Enabled)
                    {
                        pr.chFlexBox.Focus();
                        break;
                    }
                }
            }
        }

        /*
                private void SetCodeHeadJavaScript()
                {
                    const string script =
                    "<script type=\"text/javascript\">" +
                    "var styleToSelect;" +
                    "var gridViewCtl;" +
                    "var curSelRow = null;" +
                    "var SelectedDesc='';" +
                    "var SelectedCode='';" +
                    "var textboxID;" +
                    "var labelID;" +
                    "var cancelID;" +
                    "var textboxFocusID;" +
                    "var AttributeType;" +
                    "var textboxDescID;" +
                    "var bnSearchID;" +
                    "var hdnFldID;" +
                    "var copyID;" +

                    "function OnKeyPress()" +
                    "{" +
                        "if(event.keyCode == 27)" +
                        "{" +
                            "var cancelbtn = document.getElementById(cancelID);" +
                            "if (cancelbtn != null)" +
                                "cancelbtn.click();" +
                        "}" +
                        "if (event.keyCode == 13) {" +
                            "try {" +
                                "if (document.getElementById(bnSearchID) != null) " +
                                    "document.getElementById(bnSearchID).click(); return false;" +
                            " } " +
                            "catch(err) { }" +
                        "}" +
                     "}" +

                    "function fnSetFocus(txtClientId)" +
                    "{" +
                        "textboxFocusID=txtClientId;" +
                        "setTimeout('fnFocus()',2000);" +
                    "}" +

                    "function fnFocus()" +
                    "{" +
                        "var txt = document.getElementById(textboxFocusID);" +
                        "if (txt != null)" +
                        "{" +
                            "try" +
                            "{" +
                                "txt.focus();" +
                            "}" +
                            "catch(err)" +
                            "{" +
                            "}" +
                        "}" +
                    "}" +

                    //"function pageLoad(sender, args)" +
                    //"{" +
                    //    "$addHandler(document, \"keydown\", OnKeyPress);" +
                    //"}" +

                    "function onOk()" +
                    "{" +                                
                        " if (AttributeType == '6')" +
                        "{" +
                            "document.getElementById(textboxID).value = SelectedDesc;" +
                            "document.getElementById(labelID).innerHTML = '';" +
                        "}" +
                        "else" +
                        "{" +
                            "if (SelectedCode != null  && textboxID != null) {" +
                                "document.getElementById(textboxID).value = SelectedCode;" +
                                "if (copyID != null && document.getElementById(copyID) != null)" +
                                    "document.getElementById(copyID).value = SelectedCode;" +
                                "if (hdnFldID != null && document.getElementById(hdnFldID) != null && document.getElementById(hdnFldID).value == '1')" +
                                    "document.getElementById(textboxID).onchange();" +
                            "}" +
                            "if (SelectedDesc != null && labelID != null)" +
                                "document.getElementById(labelID).innerHTML = SelectedDesc;" +
                            "if ((textboxDescID != '') && (document.getElementById(textboxDescID) != null))" + 
                            "{" +                        
                                "document.getElementById(textboxDescID).value = SelectedDesc;" +
                                "document.getElementById(labelID).innerHTML = '';" +
                                "if (SelectedCode != '' && SelectedCode != null)" +
                                    "document.getElementById(textboxDescID).disabled = true;" +                        
                             "}" +
                        "}" +
                        "SelectedCode = null;" +
                        "SelectedDesc = null;" +
                    "}" +

                    "function getSelectedRow(grID, rowIdx)" +
                    "{" +
                        "gridViewCtl = document.getElementById(grID);" +
                        "if (null != gridViewCtl)" +
                            "{" +
                                "return gridViewCtl.rows[rowIdx];" +
                            "}" +
                        "return null;" +
                    "}" +

                    "function AddToListCode(item)" +
                    "{" +
                        "if ((SelectedCode == '') || (SelectedCode == null))" +
                        "{" +
                            "SelectedCode = item;" +
                        "}" +
                        "else" +
                        "{" +
                            "SelectedCode += ',' + item;" +
                        "}" +
                    "}" +

                    "function AddToListDesc(item)" +
                    "{" +
                        "if ((SelectedDesc == '') || (SelectedDesc == null))" +
                        "{" +
                            "SelectedDesc = item;" +
                        "}" +
                        "else" +
                        "{" +
                            "SelectedDesc += ',' + item;" +
                        "}" +
                    "}" +

                    "function RemoveFromListCode(item)" +
                    "{" +
                        "var myStr = ',' + SelectedCode + ',';" +
                        "myStr = myStr.split(',' + item + ',').join(',');" +
                        "myStr = myStr == ',' ? '' : myStr.substring(1, myStr.length-1);" +
                        "SelectedCode = myStr;" +
                    "}" +

                    "function RemoveFromListDesc(item)" +
                    "{" +
                        "var myStr = ',' + SelectedDesc + ',';" +
                        "myStr = myStr.split(',' + item + ',').join(',');" +
                        "myStr = myStr == ',' ? '' : myStr.substring(1, myStr.length-1);" +
                        "SelectedDesc = myStr;" +
                    "}" +


                    "function onGridViewMultiRowSelected(grID, txtID, lblID, cnlID, rowIdx, attrtype)" +
                    "{" +
                    "var selRow = getSelectedRow(grID, rowIdx);" +
                    "textboxID = txtID;" +
                    "labelID = lblID;" +
                    "cancelID   = cnlID;" +
                    "AttributeType = attrtype;" +
                    "if (selRow.style.color == 'white')" +
                        "{" +
                            "selRow.style.backgroundColor = '#f5f5f5';" +
                            "selRow.style.color = 'black';" +
                            "RemoveFromListCode(gridViewCtl.rows[rowIdx].cells[0].innerHTML);" +
                            "RemoveFromListDesc(gridViewCtl.rows[rowIdx].cells[1].innerHTML);" +
                        "}" +
                    "else" +
                        "{" +
                            "selRow.style.backgroundColor = '#294B29';" +
                            "selRow.style.color = 'white';" +
                            "AddToListCode(gridViewCtl.rows[rowIdx].cells[0].innerHTML);" +
                            "AddToListDesc(gridViewCtl.rows[rowIdx].cells[1].innerHTML);" +
                        "}" +
                    "}" +

                    "function onGridViewRowSelected(grID, txtID, lblID, cnlID, rowIdx, attrtype, txtDescID, hdnID)" +
                    "{" +            
                    "var selRow = getSelectedRow(grID, rowIdx);" +
                    "textboxID = txtID;" +
                    "labelID = lblID;" +
                    "cancelID   = cnlID;" +
                    "AttributeType = attrtype;" + 
                    "textboxDescID = txtDescID;" +
                    "hdnFldID = hdnID;" +
                    "if (curSelRow != null)" +
                        "{" +
                            "curSelRow.style.backgroundColor = '#f5f5f5';" +
                            "curSelRow.style.color = 'black';" +
                        "}" +
                    "if (null != selRow)" +
                        "{" +
                            "curSelRow = selRow;" +
                            "curSelRow.style.backgroundColor = '#294B29';" +
                            "curSelRow.style.color = 'white';" +
                            "SelectedCode = gridViewCtl.rows[rowIdx].cells[0].innerHTML;" +
                            "SelectedDesc = gridViewCtl.rows[rowIdx].cells[1].innerHTML;" +
                        "}" +
                    "}" +

                    "function onGridViewRowDoubleClicked(grID, txtID, lblID, cnlID, okID, rowIdx, txtDescID, hdnID)" +
                    "{" +
                    "var selRow = getSelectedRow(grID, rowIdx);" +
                    "textboxID = txtID;" +
                    "labelID = lblID;" +
                    "cancelID   = cnlID;" +
                    "textboxDescID = txtDescID;" +
                    "hdnFldID = hdnID;" +
                        
                    "if (curSelRow != null)" +
                        "{" +
                            "curSelRow.style.backgroundColor = '#f5f5f5';" +
                            "curSelRow.style.color = 'black';" +
                        "}" +
                    "if (null != selRow)" +
                        "{" +
                            "curSelRow = selRow;" +
                            "curSelRow.style.backgroundColor = '#294B29';" +
                            "curSelRow.style.color = 'white';" +
                            "SelectedCode = gridViewCtl.rows[rowIdx].cells[0].innerHTML;" +
                            "SelectedDesc = gridViewCtl.rows[rowIdx].cells[1].innerHTML;" +
                            "document.getElementById(okID).click();" +
                        "}" +
                    "}" +

                    "function setCurrentDate(sender, args)" +
                    "{" +	 
                        "if(sender._textbox.get_element().value == '')" +
                        "{" +
                            "var todayDate = new Date();" +
                            "sender._selectedDate = todayDate;" +
                        "}" + 
                    "}" +

                    "</script>";

                    Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "load", System.Text.RegularExpressions.Regex.Unescape(script));

                }
        */
        public void CodeHeadEnableControls(PanelRec rec, bool bEnabled)
        {
            bool allowEnable = rec.AllowEdit;
            if (IsNewRecord)
                allowEnable = rec.AllowAdd;

            if (allowEnable)
                rec.chib.Enabled = bEnabled;

            if (bEnabled == false)
            {
                rec.chtb.BackColor = Color.LightGray;
                rec.chtb.ReadOnly = true;
                rec.chlabel.ForeColor = System.Drawing.ColorTranslator.FromHtml("GrayText");
            }
            else
            {
                rec.chtb.BackColor = Color.White;
                rec.chtb.ReadOnly = false;
                rec.chlabel.ForeColor = System.Drawing.ColorTranslator.FromHtml("Black");
            }
        }

        public void CodeHeadSetLabel(PanelRec rec, string val)
        {
            rec.chlabel.Text = val;
        }

        public void CodeHeadSetValue(PanelRec rec, string val)
        {
            if (rec.chtb != null)
            {
                rec.chtb.Text = val;
                string desc = GetCodeDesc(rec._codetable, rec.CodeTablePrimaryKeyField, val, rec._desc, rec.codecondition);
                if (rec.DescCtrl == null)
                    CodeHeadSetLabel(rec, desc);
            }
            else if (rec.chComboBox != null)
            {
                if (rec.chComboBox.Items.FindByValue(val) != null)
                    rec.chComboBox.SelectedValue = val;
                else
                {
                    PLCQuery qryItem = new PLCQuery();
                    qryItem.SQL = "SELECT " + rec._desc + " FROM " + rec._codetable + " WHERE " + rec._code + " = '" + val + "'";
                    qryItem.Open();
                    if (!qryItem.IsEmpty())
                    {
                        ListItem selectedItem = new ListItem();
                        selectedItem.Text = qryItem.FieldByName(rec._desc) + " (" + val + ")";
                        selectedItem.Value = val;
                        rec.chComboBox.Items.Add(selectedItem);
                        rec.chComboBox.SelectedValue = val;
                    }
                }
            }
            else if (rec.chFlexBox != null)
            {
                rec.chFlexBox.SelectedValue = val;
            }
        }

        protected bool ActiveFieldExists(string tblname)
        {
            PLCQuery qry = new PLCQuery("select * from " + tblname + " Where  0 = 1");
            qry.Open();
            return qry.FieldExist("ACTIVE");
        }

        public class PLCProgressTemplate : System.Web.UI.UserControl, ITemplate
        {
            public PLCProgressTemplate(Control control)
            {

            }
            public void InstantiateIn(Control control)
            {
                System.Web.UI.WebControls.Image img;
                try
                {
                    img = new System.Web.UI.WebControls.Image();
                    img.ImageUrl = "~/Images/indicator.gif";
                    control.Controls.Add(img);
                    // Add text
                    LiteralControl lcText = new LiteralControl();
                    lcText.Text = "&nbsp;Retrieving Codes...";
                    control.Controls.Add(lcText);
                }
                catch
                {
                }
            }

        }

        protected void ddl_TextChanged(object sender, EventArgs e)
        {
            DropDownList dropDownList = (DropDownList)sender;
            string fieldName = dropDownList.Attributes["FIELDNAME"];

            //validate field on postback
            PLCDBPanelTextChangedEventArgs textChangedArgs = new PLCDBPanelTextChangedEventArgs(fieldName, dropDownList.SelectedValue);
            OnPLCDBPanelTextChanged(textChangedArgs);
        }

        protected void tb_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string fieldName = textBox.Attributes["FIELDNAME"];

            //validate field on postback
            PLCDBPanelTextChangedEventArgs textChangedArgs = new PLCDBPanelTextChangedEventArgs(fieldName, textBox.Text);
            OnPLCDBPanelTextChanged(textChangedArgs);

            foreach (PanelRec pr in panelrecs)
            {
                if ((pr.tb != null) && (pr.tb.ID == textBox.ID))
                {
                    //show error message if field value is not valid
                    pr.ErrMsg.Text = textChangedArgs.ErrorMessage;
                    pr.ErrMsg.Visible = !textChangedArgs.IsValid;
                }
            }
        }

        //protected void SelectPostBackButton_Click(Object sender, EventArgs e)
        //{
        //    Button button = (Button)sender;
        //    string linkedCodeTable = button.Attributes["LINKEDCODETABLE"];

        //    PLCDBPanelCodeHeadChangedEventArgs codeHeadEventArgs = new PLCDBPanelCodeHeadChangedEventArgs(linkedCodeTable);
        //    OnPLCDBPanelCodeHeadChanged(codeHeadEventArgs);

        //    foreach (PanelRec pr in panelrecs)
        //    {
        //        if ((pr.chSelectPostBackButton != null) && (pr.chSelectPostBackButton.ID == button.ID))
        //        {
        //            if (pr.chtb != null)
        //            {
        //                CodeHeadSetLabel(pr, GetCodeDesc(pr._codetable, pr.chtb.Text, pr.codecondition));
        //                pr.chib.Focus();
        //            }
        //        }
        //    }

        //}

        protected void btnRefresh_Click(Object sender, EventArgs e)
        {
            ImageButton imgbtn = (ImageButton)sender;
            foreach (PanelRec pr in panelrecs)
            {
                if ((pr.chbtnRefresh != null) && (pr.chbtnRefresh.ID == imgbtn.ID))
                {
                    PLCDisableCaching = true;
                    RefreshGrid(pr);
                }
            }

        }


        //*AAC* 05282009 In MSSQL, replace the TYPE with TYPE_VALUE.
        //*AAC* 06022009 No need for this function. TYPE fieldname is now TYPE_RES for both Oracle and MSSQL.
        //private string CheckTableField(string fldname)
        //{
        //    PLCSessionVars checkDBase = new PLCSessionVars();

        //    switch (fldname.ToUpper())
        //    {
        //        case "TYPE":
        //            fldname = (checkDBase.PLCDatabaseServer == "ORACLE" ? fldname : "TYPE_VALUE");
        //            break;

        //    }

        //    return fldname;
        //}

        //end dynamic controls


        //END Pasted in for  Modal

        #region DeptPers
        private DeptPersCodeHead CreateCodeHeadWithKey(int ctrlnum, string theCodeTable, bool allowEdit, bool postBack)
        {
            DeptPersCodeHead ch = new DeptPersCodeHead();
            ch.ID = GenerateControlID("ch", ref ctrlnum);
            ch.TableName = theCodeTable;
            ch.UsesKeyOtherThanCode = true;
            ch.PopupCaption = "CODES";
            ch.PopupCaptionCSSClass = "caption";
            ch.PopupCSSClass = "modalPopup";
            ch.ReadOnly = !allowEdit;
            ch.TabIndex = LastTabIndex;

            if (postBack)
                ch.ValueChanged += new CodeHeadValueChangedEventHandler(ch_ValueChanged);

            return ch;
        }


        void ch_ValueChanged(object sender, CodeHeadValueChangedEventArgs e)
        {
            DeptPersCodeHead codeHead = (DeptPersCodeHead)sender;
            string tableName = codeHead.TableName;
            

            PLCDBPanelCodeHeadChangedEventArgs codeHeadEventArgs = new PLCDBPanelCodeHeadChangedEventArgs(tableName,"");
            OnPLCDBPanelCodeHeadChanged(codeHeadEventArgs);
        }
        #endregion

        #region DateTime
        private TextBox CreateTimeTextBox(WebControl parent, int controlNumber)
        {
            var txtTime = new TextBox();
            txtTime.ID = "DBP_DATE_TIME_" + ID + controlNumber;
            txtTime.Width = Unit.Pixel(50);
            txtTime.Attributes.Add("onblur", "checkTimeFieldValue(event);");
            parent.Controls.Add(txtTime);

            var pnlHidden = new Panel();
            pnlHidden.Style.Add("display", "none");

            var maskEdit = new MaskedEditExtender();
            maskEdit.ID = "maskedit_time_" + ID + controlNumber;
            maskEdit.MaskType = MaskedEditType.Time;
            maskEdit.Mask = "99:99";
            maskEdit.UserTimeFormat = MaskedEditUserTimeFormat.TwentyFourHour;
            maskEdit.TargetControlID = txtTime.ID;

            pnlHidden.Controls.Add(maskEdit);
            parent.Controls.Add(pnlHidden);

            parent.Style.Add("border", "1px solid #c0c0c0");
            parent.Style.Add("align-items", "center");

            return txtTime;
        }

        private void SetTimeTextBoxMode(PanelRec pr, bool readOnly)
        {
            if (readOnly)
            {
                pr.time.BackColor = Color.LightGray;
                pr.time.Attributes["readonly"] = "readonly";
            }
            else
            {
                pr.time.BackColor = Color.White;
                pr.time.Attributes.Remove("readonly");
            }
        }

        protected string GetTimeValueIfExist(PanelRec pr)
        {
            return pr.time != null ? (" " + pr.time.Text).TrimEnd() : "";
        }
        #endregion DateTime

        // Copy dbpanel entries to a savedform row with the corresponding dbpanel id.
        public SavedFormRow ExportToSavedFormRow()
        {
            SavedFormRow savedFormRow = new SavedFormRow(this.ID);

            foreach (PanelRec rec in this.panelrecs)
            {
                SavedFormEntry entry = new SavedFormEntry(rec.fldname, HttpUtility.HtmlEncode(getpanelfield(rec.fldname)));
                savedFormRow.AddEntry(entry);
            }

            return savedFormRow;
        }

        public void ImportFromSavedFormRow(SavedFormRow row)
        {
            foreach (SavedFormEntry entry in row.entries)
            {
                setpanelfield(entry.id, HttpUtility.HtmlDecode(entry.value), false);
            }
        }

        //*AAC 07072010
        public string GetFieldValue(string tableName, string fieldName, string prompt)
        {
            string fieldValue = string.Empty;

            foreach (PanelRec pr in this.panelrecs)
            {
                if (pr.tblname.Trim() == tableName.Trim() && pr.fldname.Trim() == fieldName.Trim() && pr.prompt.Trim() == prompt.Trim())
                {
                    if (pr.tb != null)
                    {
                        fieldValue = StripMask(pr.tb.Text + GetTimeValueIfExist(pr));

                        if (pr.isDateMask(pr.editmask))
                        {
                            if (!IsDateValid(pr, fieldValue))
                                fieldValue = string.Empty;
                        }
                    }
                    else if (pr.ddl != null)
                    {
                        fieldValue = pr.ddl.SelectedValue.ToString();
                    }
                    else if (pr.rbl != null)
                    {
                        fieldValue = pr.rbl.SelectedValue.ToString();
                    }
                    else if (pr.cb != null)
                    {
                        if (pr.cb.Checked)
                        {
                            fieldValue = "T";
                        }
                        else
                        {
                            fieldValue = "F";
                        }
                    }
                    else if (pr.chMultiLookup != null)
                    {
                        fieldValue = pr.chMultiLookup.GetText();
                    }
                    else if (pr.chMultipickAc != null)
                    {
                        fieldValue = pr.chMultipickAc.GetSelectedCodes();
                    }
                    else if (pr._codetable != "")
                    {
                        if (pr.chtb != null)
                        {
                            fieldValue = pr.chtb.Text;
                        }
                        else if (pr.chComboBox != null)
                        {
                            fieldValue = pr.chComboBox.SelectedValue.ToString();
                        }
                        else if (pr.chFlexBox != null)
                        {
                            fieldValue = pr.chFlexBox.SelectedValue.ToString();
                        }
                    }
                    break;
                }
            }

            return fieldValue;
        }

        //*AAC 11252010 - This should only be use for a row with only one PANELRec
        public void HidePanelRecRow(string fieldName, bool show)
        {
            string rowName = GetRowName(fieldName.Trim());
            if (rowName != string.Empty)
            {
                TableRow tr = (TableRow)this.FindControl(rowName);
                tr.Visible = show;
            }
        }

        protected string GetRowName(string fieldName)
        {
            string panelControlID = string.Empty;
            string rowName = string.Empty;
            bool ctrlFound = false;

            foreach (PanelRec pr in this.panelrecs)
            {
                if (pr.fldname.Trim() == fieldName.Trim())
                {
                    ctrlFound = true;
                    break;
                }
            }

            if (ctrlFound)
            {
                int ctrlnum = 0;

                rowName = this.ID + "_tr" + fieldName.Trim() + ctrlnum.ToString();

                while (ctrlnum <= this.panelrecs.Count)
                {
                    if (this.FindControl(rowName) != null)
                    {
                        return rowName;
                    }
                    ctrlnum++;
                    rowName = this.ID + "_tr" + fieldName.Trim() + ctrlnum.ToString();
                }

                rowName = string.Empty;
            }

            return rowName;
        }

        private void AddRequiredFieldOverride(string fieldName, bool isRequired)
        {
            if (ViewState["RequiredFieldOverride"] == null)
            {
                ViewState["RequiredFieldOverride"] = new Dictionary<string, bool>();
            }

            Dictionary<string, bool> requiredFields = (Dictionary<string, bool>)ViewState["RequiredFieldOverride"];
            if (requiredFields.ContainsKey(fieldName))
                requiredFields[fieldName] = isRequired;
            else
                requiredFields.Add(fieldName, isRequired);
        }

        private string OverrideRequiredFieldIfSpecified(string fieldName, string defaultRequired)
        {
            if (ViewState["RequiredFieldOverride"] == null)
                return defaultRequired;

            Dictionary<string, bool> requiredFields = (Dictionary<string, bool>)ViewState["RequiredFieldOverride"];
            if (requiredFields.ContainsKey(fieldName))
                return requiredFields[fieldName] ? "T" : "F";
            else
                return defaultRequired;
        }
        
        //*AAC 11302010 - Only works for non hidden fields/non hidden prompt/field on a separate row. Please adjust code if needed.
        public void SetFieldAsRequired(string fieldName, bool isRequired, bool addToOverrideList = true)
        {
            foreach (PanelRec pr in this.panelrecs)
            {
                if (pr.fldname.Trim() == fieldName.Trim())
                {
                    // if manually set to required, override it and set to false if user's lab code is in mandatory optional lab codes
                    if (isRequired && !string.IsNullOrEmpty(pr.mandatoryOptionalLabCodes))
                        isRequired = !pr.mandatoryOptionalLabCodes.Split(',').ToList().Contains(PLCSession.PLCGlobalLabCode);
                    
                    string ltrlName = GetPromptID(fieldName.Trim());
                    if (ltrlName != string.Empty)
                    {
                        Literal ltrl = (Literal)FindControl(ltrlName);
                        string strClass = string.Empty;
                        string strStyle = string.Empty;

                        if (ltrl.Text.Contains("packedLabel"))
                        {
                            strStyle = pr.hideField ? "display:none;" : "display:inline-block; vertical-align:" + (pr.codetable == "" ? "5%" : "50%") + ";";
                            strClass = "packedLabel";
                        }

                        if (isRequired)
                            ltrl.Text = CreatePromptMarkup(pr.prompt, strStyle, strClass, PromptType.Required, pr.editmask).Text;
                        else
                            ltrl.Text = CreatePromptMarkup(pr.prompt, strStyle, strClass, PromptType.Standard, pr.editmask).Text;
                    }
                    pr.required = (isRequired) ? "T" : "F";

                    if(addToOverrideList)
                        AddRequiredFieldOverride(fieldName, isRequired);

                    break;
                }
            }
        }

        public void SetFieldAsRequiredV2(string fieldName, bool isRequired, bool addToOverrideList = true)
        {
            foreach (PanelRec pr in this.panelrecs)
            {
                if (pr.fldname.Trim() == fieldName.Trim())
                {
                    // if manually set to required, override it and set to false if user's lab code is in mandatory optional lab codes
                    if (isRequired && !string.IsNullOrEmpty(pr.mandatoryOptionalLabCodes))
                        isRequired = !pr.mandatoryOptionalLabCodes.Split(',').ToList().Contains(PLCSession.PLCGlobalLabCode);

                    string ltrlName = SearchAndGetPromptID(fieldName.Trim());
                    if (ltrlName != string.Empty)
                    {
                        Literal ltrl = (Literal)FindControl(ltrlName);
                        string strClass = string.Empty;
                        string strStyle = string.Empty;

                        if (ltrl.Text.Contains("packedLabel"))
                        {
                            strStyle = pr.hideField ? "display:none;" : "display:inline-block; vertical-align:" + (pr.codetable == "" ? "5%" : "50%") + ";";
                            strClass = "packedLabel";
                        }

                        if (isRequired)
                            ltrl.Text = CreatePromptMarkup(pr.prompt, strStyle, strClass, PromptType.Required, pr.editmask).Text;
                        else
                            ltrl.Text = CreatePromptMarkup(pr.prompt, strStyle, strClass, PromptType.Standard, pr.editmask).Text;
                    }
                    pr.required = (isRequired) ? "T" : "F";

                    if (addToOverrideList)
                        AddRequiredFieldOverride(fieldName, isRequired);

                    break;
                }
            }
        }

        // Override the prompt text with your own prompt.
        public void SetFieldPrompt(string fieldName, string prompt)
        {
            foreach (PanelRec pr in this.panelrecs)
            {
                if (pr.fldname.Trim() == fieldName.Trim())
                {
                    string ltrlName = GetPromptID(fieldName.Trim());
                    if (ltrlName != string.Empty)
                    {
                        Literal ltrl = (Literal) FindControl(ltrlName);

                        PromptType promptType = pr.required == "T" ? PromptType.Required : PromptType.Standard;
                        ltrl.Text = CreatePromptMarkup(prompt, "", "", promptType, pr.editmask).Text;
                        break;
                    }
                }
            }
        }
                
        // Restore the prompt text and required field indicator with the default values from the DBPanel definition.
        public void RestoreFieldPrompt(string fieldName)
        {
            foreach (PanelRec pr in this.panelrecs)
            {
                if (pr.fldname.Trim() == fieldName.Trim())
                {
                    string ltrlName = GetPromptID(fieldName.Trim());
                    if (ltrlName != string.Empty)
                    {
                        Literal ltrl = (Literal)FindControl(ltrlName);

                        PromptType promptType = pr.required == "T" ? PromptType.Required : PromptType.Standard;
                        ltrl.Text = CreatePromptMarkup(pr.prompt, "", "", promptType, pr.editmask).Text;
                    }
                }
            }
        }
        
        protected string GetPromptID(string fieldName)
        {
            string panelControlID = string.Empty;
            string ltrlName = string.Empty;
            bool ctrlFound = false;

            foreach (PanelRec pr in this.panelrecs)
            {
                if (pr.fldname.Trim() == fieldName.Trim())
                {
                    ctrlFound = true;
                    break;
                }
            }

            if (ctrlFound)
            {
                int ctrlnum = 0;

                ltrlName = this.ID + "_ltrl" + fieldName.Trim() + ctrlnum.ToString();

                while (ctrlnum <= this.panelrecs.Count) // test only
                {
                    if (this.FindControl(ltrlName) != null)
                    {
                        return ltrlName;
                    }
                    ctrlnum++;
                    ltrlName = this.ID + "_ltrl" + fieldName.Trim() + ctrlnum.ToString();
                }

                ltrlName = string.Empty;
            }

            return ltrlName;
        }

        protected string SearchAndGetPromptID(string fieldName)
        {
            string panelControlID = string.Empty;
            string ltrlName = string.Empty;
            bool ctrlFound = false;

            foreach (PanelRec pr in this.panelrecs)
            {
                if (pr.fldname.Trim() == fieldName.Trim())
                {
                    ctrlFound = true;
                    break;
                }
            }

            if (ctrlFound)
            {
                ltrlName = this.ID + "_ltrl" + fieldName.Trim();
                return FindControlRecursive(this, ltrlName);
            }

            return ltrlName;
        }

        private string FindControlRecursive(Control rootControl, string controlID)
        {
            if (!string.IsNullOrEmpty(rootControl.ID))
            {
                if (rootControl.ID.StartsWith(controlID))
                    return rootControl.ID;
            }

            foreach (Control controlToSearch in rootControl.Controls)
            {
                string controlToReturn = FindControlRecursive(controlToSearch, controlID);
                if (!string.IsNullOrEmpty(controlToReturn))
                    return controlToReturn;
            }
            return string.Empty;
        }

        // Return PLCDBPanel clientIDs as a javascript structure.
        public string GetClientIDsJSON()
        {
            // Output:
            // {
            //     "PLCDataTable": "TV_LABCASE",
            //     "PLCWhereClause": "",
            //     "fields": {
            //         "LABCASE.LABCODE": {"clientid":"clientid1", "prompt":"Lab Code", "required":"T", "codetable":"", "codecondition":"", "fldtype":"D", "codeuseskey":"true", "keyclientid":""},
            //         "LABSUB.INVOICE_OFFICER": {"clientid":"clientid2", "prompt":"Vouchering Officer", "required":"F", "codetable":"", "codecondition":"", "fldtype":"D", "codeuseskey":"false", "keyclientid":""}
            //     }
            // }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendFormat("\t\"{0}\": \"{1}\",\r\n", "PLCDataTable", this.PLCDataTable);
            sb.AppendFormat("\t\"{0}\": \"{1}\",\r\n", "PLCWhereClause", this.PLCWhereClause);
            sb.AppendFormat("\t\"{0}\": \"{1}\",\r\n", "PLCPanelName", this.PLCPanelName);
            sb.AppendLine("\t\"fields\": {");

            for (int i = 0; i < this.panelrecs.Count; i++)
            {
                PanelRec rec = this.panelrecs[i];

                string tblname = rec.tblname.ToUpper();
                if (!tblname.StartsWith("TV_") && !tblname.StartsWith("CV_") && !tblname.StartsWith("UV_") && !tblname.StartsWith("EV_"))
                    tblname = "TV_" + tblname;

                string fldnameExtension = "";
                if (rec.fldname.ToUpper().Contains("DATE"))
                {
                    if (rec.prompt.ToUpper().Contains("FROM"))
                        fldnameExtension = "(FROM)";
                    else if (rec.prompt.ToUpper().Contains("TO"))
                        fldnameExtension = "(TO)";
                }
                
                // Serialize the originalvalue string to properly format and escape it to json string
                string originalvalue = rec.original.Value;
                JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                originalvalue = jsonSerializer.Serialize(originalvalue);
                originalvalue = originalvalue.Substring(1, originalvalue.Length - 2);


                string timeClientID = rec.time != null
                    ? rec.time.ClientID
                    : string.Empty;

                string validateCondition = rec.validateOnSave? (!string.IsNullOrEmpty(rec.supplementLink.Trim()) ? rec.chFlexBox.CodeCondition : rec.codecondition) : rec.codecondition;


                // "LABCASE.LABCODE": {"clientid":"clientid1", "prompt":"Lab Code", "required":"T", "codetable":"", "codecondition":"", "fldtype":"D", "codeuseskey":"false", "keyclientid":"", "defaultvalue": ""},
                sb.AppendFormat("\t\t\"{0}.{1}\": {{\"clientid\":\"{2}\", \"prompt\":\"{3}\", \"required\":\"{4}\", \"codetable\":\"{5}\", \"codecondition\":\"{6}\", \"fldtype\":\"{7}\", \"defaultvalue\":\"{8}\", \"editmask\":\"{9}\", \"mandatorybase\":\"{10}\", \"hidefield\":\"{11}\", \"originalvalue\":\"{12}\", \"timeClientID\":\"{13}\", \"validateonsave\":\"{14}\", \"validatecondition\":\"{15}\", \"timedatefield\":\"{16}\", \"nofuturedates\":\"{17}\", \"nopastdates\":\"{18}\", \"allowedit\":\"{19}\",\"allowadd\":\"{20}\",\"likesearch\":\"{21}\", \"defvalueref\":\"{22}\"}}",
                    tblname, rec.fldname.ToUpper() + fldnameExtension, rec.GetClientControlID(), rec.prompt.Replace("\"", "\\\"").Replace("\n", "\\n"), rec.required, rec.codetable, rec.codecondition, rec.fldtype, GetDefaultValue(rec), rec.editmask, rec.mandatoryBase, (rec.hideField ? "T" : "F"), originalvalue, timeClientID, rec.validateOnSave, validateCondition, rec.TimeDateField, (rec.NoFutureDates ? "T" : "F"), (rec.NoPastDates ? "T" : "F"), (rec.AllowEdit ? "T" : "F"), (rec.AllowAdd ? "T" : "F"), (rec.usesLikeSearch ? "T" : "F"), rec.defaultValue);
                // Don't output comma on the last field
                if (i < this.panelrecs.Count - 1)
                    sb.AppendLine(",");
                else
                    sb.AppendLine();
            }

            sb.AppendLine("\t}");
            sb.AppendLine("}");

            return sb.ToString();
        }

        public string GetAddToListScript()
        {
            // Output:
            // _plcdbpanellist["plcdbpanelname"] = {...};
            return String.Format("_plcdbpanellist[\"{0}\"] = {1};", this.ID, GetClientIDsJSON());
        }

        public string SearchFilterSQL(Dictionary<string, string> specialConditions = null)
        {
            // IMPORTANT NOTES:
            // 1. This function is for Search DBPanels to generate the SQL filter (WHERE clause).
            // 2. If this DBPanel control is used to display the search results in a DBGrid control:
            //      a. TV_DBPANEL table and fields must be present in the corresponding TV_DBGRIDHD.SQL_STRING.
            //      b. In the TV_DBGRIDHD.SQL_STRING, make use of table names as identifiers instead of aliases. 
            //          For example: ... INNER JOIN TV_LABITEM ON TV_LABCASE.CASE_KEY = TV_LABITEM.CASE_KEY (instead of ... INNER JOIN TV_LABITEM I ON C.CASE_KEY = I.CASE_KEY)
            // 3. Pass a SpecialConditions dictionary parameter, if a FIELDNAME-FIELDVALUE pair filter does not follow the typical formats below.
            //      For example, this is a special condition -> TV_LABITEM.LAB_ITEM_NUMBER IN ('1','2','3') OR TV_ANALYST.NAME LIKE '%MI%'
            // 4. Only non-empty values are added in the SQL filter. If a field with an empty value needs to be part of the SQL filter, add it to the SpecialConditions dictionary parameter.

            List<string> whereConditions = new List<string>();

            foreach (PanelRec pr in panelrecs)
            {
                string tableName = pr.tblname;
                string fieldName = pr.fldname;
                string prompt = pr.prompt;
                string editMask = pr.editmask.ToUpper();
                string fieldValue = GetFieldValue(tableName, fieldName, prompt).Replace("'", "''");
                bool usesLikeSearch = pr.usesLikeSearch;

                if (PLCSession.PLCDatabaseServer == "ORACLE")
                    fieldValue = fieldValue.Replace("+", "' + chr(43) + '");

                if (specialConditions != null && specialConditions.ContainsKey(tableName + "." + fieldName))
                {
                    whereConditions.Add(specialConditions[tableName + "." + fieldName]);
                }
                else if (!string.IsNullOrEmpty(fieldValue))
                {
                    if (editMask == "DD/MM/YYYY")
                        fieldValue = PLCSession.DateStringToMDY(fieldValue);

                    if (pr.isDateMask(pr.editmask) && prompt.ToUpper().EndsWith(" FROM"))
                        whereConditions.Add(string.Format("{0}.{1} >= CONVERTTODATE('{2}')", tableName, fieldName, fieldValue));
                    else if (pr.isDateMask(pr.editmask) && prompt.ToUpper().EndsWith(" TO"))
                        whereConditions.Add(string.Format("{0}.{1} <= CONVERTTODATE('{2}')", tableName, fieldName, fieldValue));
                    else
                    {
                        if (usesLikeSearch)
                            whereConditions.Add(string.Format("{0}.{1} LIKE '%{2}%'", tableName, fieldName, fieldValue));
                        else 
                            whereConditions.Add(string.Format("{0}.{1} = '{2}'", tableName, fieldName, fieldValue));
                    }
                }
            }

            return PLCSession.FormatSpecialFunctions(string.Join(" AND ", whereConditions.Where(a => a.Trim() != "").ToArray()));
        }

        public string SearchFilterCrystalReport(Dictionary<string, string> specialConditions)
        {
            // This function is for Search DBPanels to generate the filter (where clause) of Crystal Reports.
            // See SearchFilterSQL function for IMPORTANT NOTES.

            List<string> whereConditions = new List<string>();

            foreach (PanelRec pr in panelrecs)
            {
                string tableName = pr.tblname;
                string fieldName = pr.fldname;
                string prompt = pr.prompt;
                string editMask = pr.editmask.ToUpper();
                string fieldValue = GetFieldValue(tableName, fieldName, prompt);

                if (specialConditions.ContainsKey(tableName + "." + fieldName))
                {
                    whereConditions.Add(specialConditions[tableName + "." + fieldName]);
                }
                else if (!string.IsNullOrEmpty(fieldValue))
                {
                    if (pr.isDateMask(pr.editmask) && prompt.ToUpper().EndsWith(" FROM"))
                        whereConditions.Add("{" + tableName + "." + fieldName + "} >= DATE(" + PLCSession.ConvertToDateTime(fieldValue).Year + "," + PLCSession.ConvertToDateTime(fieldValue).Month + "," + PLCSession.ConvertToDateTime(fieldValue).Day + ")");
                    else if (pr.isDateMask(pr.editmask) && prompt.ToUpper().EndsWith(" TO"))
                        whereConditions.Add("{" + tableName + "." + fieldName + "} <= DATE(" + PLCSession.ConvertToDateTime(fieldValue).Year + "," + PLCSession.ConvertToDateTime(fieldValue).Month + "," + PLCSession.ConvertToDateTime(fieldValue).Day + ")");
                    else
                        whereConditions.Add("{" + tableName + "." + fieldName + "} = '" + fieldValue + "'");
                }
            }

            return PLCSession.FormatSpecialFunctions(string.Join(" AND ", whereConditions.ToArray()));
        }

        public bool HasPanelRec(string tableName, string fieldName)
        {
            return (panelrecs.FirstOrDefault(a => a.tblname.ToUpper() == tableName.ToUpper() && a.fldname.ToUpper() == fieldName.ToUpper()) != null);
        }

        public bool HasPanelRec(string tableName, string fieldName, string prompt)
        {
            return (panelrecs.FirstOrDefault(a => a.tblname.ToUpper() == tableName.ToUpper() && a.fldname.ToUpper() == fieldName.ToUpper() && a.prompt.ToUpper() == prompt.ToUpper()) != null);
        }

        public int NumVisiblePanelRecs()
        {
            int numPanelRecs = 0;
            foreach (PanelRec pr in this.panelrecs)
            {
                if (!pr.hideField)
                    numPanelRecs++;
            }

            return numPanelRecs;
        }

        public void SetPanelDefaultValues()
        {
            foreach (PanelRec pr in panelrecs)
            {

                if (pr.defaultValue.ToUpper() == "GLOBALANALYST")
                {
                    if (pr.tb != null)
                        pr.tb.Text = PLCSession.PLCGlobalAnalyst;
                    else if (pr.chtb != null)
                        pr.chtb.Text = PLCSession.PLCGlobalAnalyst;
                    else if (pr.chFlexBox != null)
                        pr.chFlexBox.SetValue(PLCSession.PLCGlobalAnalyst);
                }
                else if (pr.defaultValue.ToUpper() == "CURRENTYEAR2")
                    {
                    String cy2 = DateTime.Today.Year.ToString();
                    cy2 = cy2.Substring(2);                   
                    if (pr.tb != null)
                        pr.tb.Text = cy2;
                    else if (pr.chtb != null)
                        pr.chtb.Text = cy2;
                    else if (pr.chFlexBox != null)
                        pr.chFlexBox.SetValue(cy2);
                    }

                else if (pr.defaultValue.ToUpper() == "CURRENTYEAR")
                    {
                    String cy = DateTime.Today.Year.ToString();
                    if (pr.tb != null)
                        pr.tb.Text = cy;
                    else if (pr.chtb != null)
                        pr.chtb.Text = cy;
                    else if (pr.chFlexBox != null)
                        pr.chFlexBox.SetValue(cy);
                    }

                else if (pr.defaultValue.ToUpper() == "GLOBALLABCODE")
                {
                    if (pr.tb != null)
                        pr.tb.Text = PLCSession.PLCGlobalLabCode;
                    else if (pr.chtb != null)
                        pr.chtb.Text = PLCSession.PLCGlobalLabCode;
                    else if (pr.chFlexBox != null)
                        pr.chFlexBox.SetValue(PLCSession.PLCGlobalLabCode);

                    pr.defaultValue = PLCSession.PLCGlobalLabCode;
                }
                else if (pr.defaultValue.ToUpper() == "CURRENTDATE")
                {
                    if (pr.tb != null)
                    {
                        if (!String.IsNullOrEmpty(pr.editmask))
                            pr.tb.Text = System.DateTime.Now.ToString(pr.editmask.Replace("m", "M").Replace("D", "d").Replace("Y", "y"));
                        else
                            pr.tb.Text = System.DateTime.Now.ToShortDateString();
                    }
                }
                else if (pr.defaultValue.ToUpper() == "CURRENTTIME")
                {
                    if (pr.tb != null)
                    {
                        if (!String.IsNullOrEmpty(pr.editmask))
                            pr.tb.Text = System.DateTime.Now.ToString(pr.editmask.Replace("h","H"));
                        else
                            pr.tb.Text = System.DateTime.Now.ToShortTimeString();
                    }
                }
                else if (pr.defaultValue.ToUpper() == "CURRENTDATETIME")
                {
                    if (pr.tb != null)
                    {
                        if (!String.IsNullOrEmpty(pr.editmask))
                            pr.tb.Text = System.DateTime.Now.ToString(pr.editmask.Split(' ')[0].Replace("m", "M").Replace("D", "d").Replace("Y", "y") + " " + pr.editmask.Split(' ')[1].Replace("h", "H"));
                        else
                            pr.tb.Text = System.DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
                    }
                }
                else if (pr.defaultValue.ToUpper() == "GLOBALCODNAUSER")
                {
                    if (pr.tb != null)
                        pr.tb.Text = PLCSession.PLCGlobalPrelogUser;
                    else if (pr.chtb != null)
                        pr.chtb.Text = PLCSession.PLCGlobalPrelogUser;
                    else if (pr.chFlexBox != null)
                        pr.chFlexBox.SetValue(PLCSession.PLCGlobalPrelogUser);

                    pr.defaultValue = PLCSession.PLCGlobalPrelogUser;
                }
                else if (pr.defaultValue.Trim().ToUpper().StartsWith(":SELECT")) // defaultValue is select query
                {
                    setpanelfield(pr.fldname, pr.prompt, GetDefaultValue(pr));
                }
                else if (pr.defaultValue.ToUpper() == "GLOBALDEPTCODE")
                {
                    if (pr.tb != null)
                        pr.tb.Text = PLCSession.PLCGlobalAnalystDepartmentCode;
                    else if (pr.chtb != null)
                        pr.chtb.Text = PLCSession.PLCGlobalAnalystDepartmentCode;
                    else if (pr.chFlexBox != null)
                        pr.chFlexBox.SetValue(PLCSession.PLCGlobalAnalystDepartmentCode);

                    pr.defaultValue = PLCSession.PLCGlobalAnalystDepartmentCode;
                }
                else if (pr.defaultValue.ToUpper() == "PRELOGDEPTLABCODE")
                {
                    string deptLabCode = GetDeptLabCode(PLCSession.PLCGlobalPrelogDepartmentCode);

                    if (pr.tb != null)
                        pr.tb.Text = deptLabCode;
                    else if (pr.chtb != null)
                        pr.chtb.Text = deptLabCode;
                    else if (pr.chFlexBox != null)
                        pr.chFlexBox.SetValue(deptLabCode);

                    pr.defaultValue = deptLabCode;
                }
                else if (!string.IsNullOrEmpty(pr.defaultValue))
                {
                    setpanelfield(pr.fldname, pr.defaultValue);
                }
            }            
        }

        public void SetPanelDefaultValuesOnBlankFields()
        {
            foreach (PanelRec pr in panelrecs)
            {
                if (pr.cb != null && !string.IsNullOrEmpty(pr.defaultValue))
                {
                    if (!pr.cb.Checked)
                        pr.cb.Checked = pr.defaultValue.Equals("T");
                    continue;
                }
                
                if (!string.IsNullOrEmpty(getpanelfield(pr.fldname)))
                    continue;
                
                if (pr.defaultValue.ToUpper() == "GLOBALANALYST")
                {
                    if (pr.tb != null)
                        pr.tb.Text = PLCSession.PLCGlobalAnalyst;
                    else if (pr.chtb != null)
                        pr.chtb.Text = PLCSession.PLCGlobalAnalyst;
                    else if (pr.chFlexBox != null)
                        pr.chFlexBox.SetValue(PLCSession.PLCGlobalAnalyst);
                }
                else if (pr.defaultValue.ToUpper() == "CURRENTDATE")
                {
                    if (pr.tb != null)
                    {
                        if (!String.IsNullOrEmpty(pr.editmask))
                            pr.tb.Text = System.DateTime.Now.ToString(pr.editmask.Replace("m", "M").Replace("D", "d").Replace("Y", "y"));
                        else
                            pr.tb.Text = System.DateTime.Now.ToShortDateString();
                    }
                }
                else if (pr.defaultValue.ToUpper() == "CURRENTTIME")
                {
                    if (pr.tb != null)
                    {
                        if (!String.IsNullOrEmpty(pr.editmask))
                            pr.tb.Text = System.DateTime.Now.ToString(pr.editmask.Replace("h", "H"));
                        else
                            pr.tb.Text = System.DateTime.Now.ToShortTimeString();
                    }
                }
                else if (pr.defaultValue.ToUpper() == "GLOBALCODNAUSER")
                {
                    if (pr.tb != null)
                        pr.tb.Text = PLCSession.PLCGlobalPrelogUser;
                    else if (pr.chtb != null)
                        pr.chtb.Text = PLCSession.PLCGlobalPrelogUser;
                    else if (pr.chFlexBox != null)
                        pr.chFlexBox.SetValue(PLCSession.PLCGlobalPrelogUser);

                    pr.defaultValue = PLCSession.PLCGlobalPrelogUser;
                }
                else if (pr.defaultValue.ToUpper() == "GLOBALDEPTCODE")
                {
                    if (pr.tb != null)
                        pr.tb.Text = PLCSession.PLCGlobalAnalystDepartmentCode;
                    else if (pr.chtb != null)
                        pr.chtb.Text = PLCSession.PLCGlobalAnalystDepartmentCode;
                    else if (pr.chFlexBox != null)
                        pr.chFlexBox.SetValue(PLCSession.PLCGlobalAnalystDepartmentCode);

                    pr.defaultValue = PLCSession.PLCGlobalAnalystDepartmentCode;
                }
                else if (pr.defaultValue.ToUpper() == "PRELOGDEPTLABCODE")
                {   
                    string deptLabCode = GetDeptLabCode(PLCSession.PLCGlobalPrelogDepartmentCode);

                    if (pr.tb != null)
                        pr.tb.Text = deptLabCode;
                    else if (pr.chtb != null)
                        pr.chtb.Text = deptLabCode;
                    else if (pr.chFlexBox != null)
                        pr.chFlexBox.SetValue(deptLabCode);

                    pr.defaultValue = deptLabCode;
                }
                else if (pr.defaultValue.Trim().ToUpper().StartsWith(":SELECT")) // defaultValue is select query
                {
                    setpanelfield(pr.fldname, pr.prompt, GetDefaultValue(pr));
                }
                else if (!string.IsNullOrEmpty(pr.defaultValue))
                {
                    setpanelfield(pr.fldname, pr.defaultValue);
                }
            }
        }

        private string GetDefaultValue(PanelRec pr)
        {
            string value = "";
            if (!pr.defaultValue.Trim().ToUpper().StartsWith(":SELECT"))
            {

                if (pr.defaultValue.ToUpper() == "GLOBALANALYST")
                    value = PLCSession.PLCGlobalAnalyst;
                else if (pr.defaultValue.ToUpper() == "CURRENTYEAR2")
                {
                    String cy2 = DateTime.Today.Year.ToString();
                    cy2 = cy2.Substring(2);
                    value = cy2;
                }
                else if (pr.defaultValue.ToUpper() == "CURRENTYEAR")
                {
                    String cy = DateTime.Today.Year.ToString();
                    value = cy;
                }

                else if (pr.defaultValue.ToUpper() == "GLOBALLABCODE")
                {
                    value = PLCSession.PLCGlobalLabCode;
                }
                else if (pr.defaultValue.ToUpper() == "CURRENTDATE")
                {
                    if (pr.tb != null)
                    {
                        if (!String.IsNullOrEmpty(pr.editmask))
                            value = System.DateTime.Now.ToString(pr.editmask.Replace("m", "M").Replace("D", "d").Replace("Y", "y"));
                        else
                            value = System.DateTime.Now.ToShortDateString();
                    }
                }
                else if (pr.defaultValue.ToUpper() == "CURRENTTIME")
                {
                    if (pr.tb != null)
                    {
                        if (!String.IsNullOrEmpty(pr.editmask))
                            value = System.DateTime.Now.ToString(pr.editmask.Replace("h", "H"));
                        else
                            value = System.DateTime.Now.ToShortTimeString();
                    }
                }
                else if (pr.defaultValue.ToUpper() == "CURRENTDATETIME")
                {
                    if (pr.tb != null)
                    {
                        if (!String.IsNullOrEmpty(pr.editmask))
                            value = System.DateTime.Now.ToString(pr.editmask.Split(' ')[0].Replace("m", "M").Replace("D", "d").Replace("Y", "y") + " " + pr.editmask.Split(' ')[1].Replace("h", "H"));
                        else
                            value = System.DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
                    }
                }
                else if (pr.defaultValue.ToUpper() == "GLOBALCODNAUSER")
                {
                    value = PLCSession.PLCGlobalPrelogUser;

                }
                else if (pr.defaultValue.ToUpper() == "GLOBALDEPTCODE")
                {
                    value = PLCSession.PLCGlobalAnalystDepartmentCode;
                }
                else if (pr.defaultValue.ToUpper() == "PRELOGDEPTLABCODE")
                {
                    string deptLabCode = GetDeptLabCode(PLCSession.PLCGlobalPrelogDepartmentCode);

                    value = deptLabCode;
                }
                else
                    value = pr.defaultValue;
              
            }
            else
            {
                PLCQuery qry = new PLCQuery(PLCSession.ProcessSQLMacro(pr.defaultValue.Trim().Substring(1)));
                qry.Open();
                if (qry.HasData())
                {
                    if (pr.editmask != "MULTIPICK" && pr.editmask != "MULTIPICK_SEARCH")
                    {
                        value = qry.FieldByIndex(0);
                    }
                    else
                    {
                        var values = new List<string>();
                        while (!qry.EOF())
                        {
                            values.Add(qry.FieldByIndex(0));
                            qry.Next();
                        }
                        if (values.Count > 0)
                            value = string.Join(",", values);
                    }
                }               
            }
            return value;
        }

		public bool HasChanges(bool validateEmpty = true)
		{
            return HasChanges(validateEmpty, new string[] { });
		}

        public bool HasChanges(string[] fieldsToCheck)
        {
            foreach (string fieldName in GetFieldNames())
            {
                if (fieldsToCheck.Contains(fieldName))
                {
                    string originalValue;
                    string newValue;
                    GetPanelFieldBeforeAndAfterValues(fieldName, out originalValue, out newValue);

                    if (originalValue.Trim() != newValue.Trim())
                        return true;
                }
            }

            return false;
        }

        public bool HasChanges(bool validateEmpty, params string[] excludeField)
        {
            foreach (string fieldName in GetFieldNames())
            {
                string originalValue;
                string newValue;
                GetPanelFieldBeforeAndAfterValues(fieldName, out originalValue, out newValue);

                if ((validateEmpty || excludeField.Contains(fieldName) || !string.IsNullOrEmpty(originalValue)) &&
                    originalValue.Trim() != newValue.Trim())
                    return true;
            }
            return false;
        }

        private void GetPanelFieldBeforeAndAfterValues(string fieldName, out string originalValue, out string newValue)
        {
            PanelRec panel = GetPanelRecByFieldName(fieldName);
            string fieldType = panel.fldtype;
            string editmask = panel.editmask;
            originalValue = GetOriginalValue(fieldName);
            newValue = getpanelfield(fieldName);

            if (fieldType == "D")
            {
                if (!string.IsNullOrEmpty(originalValue))
                {
                    if (IsDMYFormat(editmask))
                        originalValue = DateTime.Parse(originalValue, CultureInfo.CreateSpecificCulture("en-GB")).ToString(CultureInfo.CreateSpecificCulture("en-GB"));
                    else
                        originalValue = DateTime.Parse(originalValue).ToShortDateString();
                }
                if (!string.IsNullOrEmpty(newValue))
                {
                    if (IsDMYFormat(editmask))
                        newValue = DateTime.Parse(newValue, CultureInfo.CreateSpecificCulture("en-GB")).ToString(CultureInfo.CreateSpecificCulture("en-GB"));
                    else
                        newValue = DateTime.Parse(newValue).ToShortDateString();
                }
            }
            else if (fieldType == "T")
            {
                if (!string.IsNullOrEmpty(originalValue))
                    originalValue = DateTime.Parse(originalValue).ToShortTimeString();
                if (!string.IsNullOrEmpty(newValue))
                    newValue = DateTime.Parse(newValue).ToShortTimeString();
            }
        }

        public string[] GetPanelFieldNames()
        {
            List<string> fieldNames = new List<string>();

            foreach (PanelRec pr in panelrecs)
            {
                fieldNames.Add(pr.fldname);
            }

            return fieldNames.ToArray();
        }

        /// <summary>
        /// Gets the all the fields as object
        /// </summary>
        /// <returns>List of panel rec object</returns>
        public List<PanelRec> GetPanelFieldRecs()
        {
            return panelrecs;
        }

        /// <summary>
        /// Modify the value of the field if the mask is a number(ex. "9999", "999.999").
        /// </summary>
        /// <param name="pr"></param>
        /// <param name="qryLoad"></param>
        /// <returns>boolean - true if handled otherwise false</returns>
        private bool HandleIfNumberMask(PanelRec pr, PLCQuery qryLoad)
        {
            PLCSessionVars sv = new PLCSessionVars();
            if (pr.editmask.Length < 1)
            {
                return false;
            }
            
            string[] mask = pr.editmask.Split('.');

            if (mask.Length > 2) return false;

            string wholeNumberMask = mask[0];
            string decimalNumberMask = mask.Length > 1 ? mask[1] : "";

            foreach (char maskChar in wholeNumberMask)
            {
                if (maskChar != '9') return false;
            }

            foreach (char maskChar in decimalNumberMask)
            {
                if (maskChar != '9') return false;
            }

            string[] value = qryLoad.FieldByName(pr.fldname).ToString().Split('.');
            string wholeNumber = value[0];
            string decimalNumber = "";
            if (decimalNumberMask.Length > 0)
            {
                if (value.Length > 1)
                    decimalNumber = value[1];

                for (int i = decimalNumber.Length; i < decimalNumberMask.Length; i++)
                    decimalNumber += "0";
            }

            pr.tb.Text = wholeNumber;

            if (decimalNumber != "")
                pr.tb.Text += "." + decimalNumber;

            return true;
        }

        /// <summary>
        /// Modify the value of the field if the mask is a number(ex. "9999", "999.999").
        /// </summary>
        /// <param name="pr"></param>
        /// <param name="loadValue"></param>
        /// <returns>boolean - true if handled otherwise false</returns>
        private bool HandleIfNumberMask(PanelRec pr, string loadValue)
        {
            PLCSessionVars sv = new PLCSessionVars();
            if (pr.editmask.Length < 1)
            {
                return false;
            }

            string[] mask = pr.editmask.Split('.');

            if (mask.Length > 2) return false;

            string wholeNumberMask = mask[0];
            string decimalNumberMask = mask.Length > 1 ? mask[1] : "";

            foreach (char maskChar in wholeNumberMask)
            {
                if (maskChar != '9') return false;
            }

            foreach (char maskChar in decimalNumberMask)
            {
                if (maskChar != '9') return false;
            }

            string[] value = loadValue.Split('.');
            string wholeNumber = value[0];
            string decimalNumber = "";
            if (decimalNumberMask.Length > 0)
            {
                if (value.Length > 1)
                    decimalNumber = value[1];

                for (int i = decimalNumber.Length; i < decimalNumberMask.Length; i++)
                    decimalNumber += "0";
            }

            pr.tb.Text = wholeNumber;

            if (decimalNumber != "")
                pr.tb.Text += "." + decimalNumber;

            return true;
        }

        protected bool IsDMYFormat(string dateMask)
        {
            return (dateMask.ToUpper().StartsWith("DD/"));
        }

        protected DateTime ConvertToDateTime(string dateTimeText, string mask)
        {
            DateTime myDateTime;
            if (IsDMYFormat(mask))
            {
                bool isDate = DateTime.TryParse(dateTimeText, CultureInfo.CreateSpecificCulture("en-GB"), DateTimeStyles.None, out myDateTime);
                if (isDate)
                    return myDateTime;
            }

            return Convert.ToDateTime(dateTimeText, CultureInfo.CreateSpecificCulture("en-US"));
        }

        protected DateTime ConvertToDateTimeFromDB(string dateTimeText)
        {
            DateTime myDateTime;
            string culture = CultureInfo.CurrentCulture.Name;
            if (string.IsNullOrEmpty(culture)) culture = "en-US";

            //bool isDate = DateTime.TryParse(dateTimeText, CultureInfo.CreateSpecificCulture("en-US"), DateTimeStyles.None, out myDateTime);
            bool isDate = DateTime.TryParse(dateTimeText, CultureInfo.CreateSpecificCulture(culture), DateTimeStyles.None, out myDateTime);
            if (isDate)
                return myDateTime;
            else
                return DateTime.Parse(dateTimeText, CultureInfo.CreateSpecificCulture("en-GB"));
        }

        public string ConvertToFieldShortDateFormat(string fieldname, DateTime datetime)
        {
            PanelRec rec = this.GetPanelRecByFieldName(fieldname);
            string editmask = string.Empty;
            if (rec != null)
                editmask = rec.editmask;
            else
                editmask = PLCSession.GetDateFormat();

            if (IsDMYFormat(editmask))
                return datetime.ToString("dd/MM/yyyy");
            else
                return datetime.ToString("MM/dd/yyyy");
        }

        public string ConvertToFieldShortDateFormat(string fieldname, string datetext)
        {
            PanelRec rec = this.GetPanelRecByFieldName(fieldname);
            string editmask = string.Empty;
            if (rec != null)
                editmask = rec.editmask;
            else
                editmask = PLCSession.GetDateFormat();

            DateTime datetime;
            bool isDate = DateTime.TryParse(datetext, out datetime);
            if (isDate)
            {
                if (IsDMYFormat(editmask))
                    return datetime.ToString("dd/MM/yyyy");
                else
                    return datetime.ToString("MM/dd/yyyy");
            }
            else
                return string.Empty;
        }

        public string GetDateFieldMDYFormat(string fieldname)
        {
            PanelRec rec = this.GetPanelRecByFieldName(fieldname);
            string editmask = string.Empty;
            if (rec != null)
                editmask = rec.editmask;
            else
                return string.Empty;

            string datetext = rec.tb.Text;

            CultureInfo culture;
            if (editmask.ToUpper().StartsWith("DD"))
                culture = CultureInfo.CreateSpecificCulture("en-GB");
            else
                culture = CultureInfo.CreateSpecificCulture("en-US");
            DateTime datetime;
            bool isDate = DateTime.TryParse(datetext, culture, DateTimeStyles.None, out datetime);
            if (isDate)
            {
                return datetime.ToString("MM/dd/yyyy");
            }
            else
                return string.Empty;
        }

        private void OverrideRequiredFieldBasedOnLabCodes(string labCodes, ref string theRequired)
        {
            List<string> liLabCodes = labCodes.Split(',').ToList();
            theRequired = liLabCodes.Contains(PLCSession.PLCGlobalLabCode) ? "F" : theRequired;
        }

        public void ProcessMacro(string filterCondition, out string codeCondition, out string filterValue)
        {
            int P;
            int P1;
            string TempString = filterCondition;
            string Macro;
            codeCondition = "";
            filterValue = "";

            P = TempString.IndexOf("<&>");
            if (P >= 0)
            {
               codeCondition += TempString.Substring(0, P);
               TempString = TempString.Remove(0, P + 3);
                P1 = TempString.IndexOf("</&>");
                if (P1 >= 0)
                {
                    Macro = TempString.Substring(0, P1);
                    TempString = TempString.Remove(0, P1 + 4);

                    if (Macro == "DEPARTMENT_CODE")
                    {
                        filterValue = GetCaseDepartmentCode();
                        codeCondition += "'" + filterValue + "'";
                    }
                    
                }
            }

        }

        private string GetCaseDepartmentCode()
        {
            string departmentCode = string.Empty;

            if (PLCSession.PLCGlobalCaseKey != "")
            {
                PLCQuery qryLabCase = new PLCQuery();
                qryLabCase.SQL = string.Format("SELECT * FROM TV_LABCASE WHERE CASE_KEY = '{0}'", PLCSession.PLCGlobalCaseKey);
                qryLabCase.Open();
                if (!qryLabCase.IsEmpty())
                    departmentCode = qryLabCase.FieldByName("DEPARTMENT_CODE");

            }
            else if (PLCSession.PLCGlobalPrelogDepartmentCode != "")
                departmentCode = PLCSession.PLCGlobalPrelogDepartmentCode;

            return departmentCode;
        }

        /// <summary>
        /// Gets the original value then apply its edit mask
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public string GetOriginalValueMasked(string fieldName)
        {
            string originalValue = "";

            foreach (PanelRec pr in panelrecs)
            {
                if (pr.fldname.ToUpper() == fieldName.ToUpper())
                {
                    if (pr.tb != null && pr.isDateMask(pr.editmask) && !string.IsNullOrEmpty(pr.original.Value))
                    {
                        if (IsDMYFormat(pr.editmask))
                        {
                            if (pr.editmask.ToUpper().Contains(":SS") || pr.editmask.ToUpper().Contains("HH:MM"))
                                return ConvertToDateTime(pr.original.Value, pr.editmask).ToString("dd/MM/yyyy HH:mm:ss");
                            else if (pr.time != null)
                                return ConvertToDateTime(pr.original.Value, pr.editmask).ToString("dd/MM/yyyy HH:mm");
                            else
                                return ConvertToDateTime(pr.original.Value, pr.editmask).ToString("dd/MM/yyyy");
                        }
                        else
                        {
                            if (pr.editmask.ToUpper().Contains(":SS") || pr.editmask.ToUpper().Contains("HH:MM"))
                                return ConvertToDateTime(pr.original.Value, pr.editmask).ToString("MM/dd/yyyy HH:mm:ss");
                            else if (pr.time != null)
                                return ConvertToDateTime(pr.original.Value, pr.editmask).ToString("MM/dd/yyyy HH:mm");
                            else
                                return ConvertToDateTime(pr.original.Value, pr.editmask).ToString("MM/dd/yyyy");
                        }
                    }
                    else
                        return pr.original.Value;
                }
            }

            return originalValue;
        }

        /// <summary>
        /// Set the Value of the Panel Field
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public void SetPanelFieldValue(string tableName, string fieldName, string prompt, string fieldValue, bool isConsiderUsesKey = false)
        {
            foreach (PanelRec pr in panelrecs)
            {
                if (pr.tblname.Trim().ToUpper() == tableName.Trim().ToUpper()
                    && pr.fldname.Trim().ToUpper() == fieldName.Trim().ToUpper()
                    && pr.prompt.Trim().ToUpper() == prompt.Trim().ToUpper())
                {
                    if (pr.chtb != null)
                        CodeHeadSetValue(pr, fieldValue);

                    if (pr.tb != null)
                    {
                        if (pr.time != null)
                        {
                            var dateTime = ConvertToDateTimeFromDB(fieldValue);
                            pr.time.Text = dateTime.ToString("HH:mm");
                        }

                        if ((pr.fldtype == "T") && (!string.IsNullOrEmpty(fieldValue)))
                        {
                            if (pr.editmask.ToUpper() == "HH:MM:SS")
                                fieldValue = Convert.ToDateTime(fieldValue).ToString("HH:mm:ss");
                            else
                                fieldValue = Convert.ToDateTime(fieldValue).ToString("HH:mm");
                        }

                        pr.tb.Text = fieldValue;
                    }

                    if (pr.ddl != null)
                    {
                        if (pr.ddl.Items.FindByValue(fieldValue) != null)
                            pr.ddl.SelectedValue = fieldValue;
                    }

                    if (pr.rbl != null)
                    {
                        if (pr.rbl.Items.FindByValue(fieldValue) != null)
                            pr.rbl.SelectedValue = fieldValue;
                    }

                    if (pr.chComboBox != null)
                    {
                        if (pr.chComboBox.Items.FindByValue(fieldValue) != null)
                            pr.chComboBox.SelectedValue = fieldValue;
                    }

                    if (pr.chFlexBox != null)
                    {
                        pr.chFlexBox.SelectedValue = fieldValue;
                    }


                    if (pr.cb != null)
                        pr.cb.Checked = fieldValue == "T";

                    return;
                }
            }
        }



        #region MandatoryBase

        private void ParseMandatoryBaseValues(string reqFieldName, string sMandatoryBases, ref List<string> BaseFieldControlsToValidate)
        {
            string panelName = "";
            string fieldName = "";
            string manBase = sMandatoryBases;

            if (manBase == "T")
            {
                //do nothing
            }
            else //macro mandatory base
            {
                string expectedValue = "";

                if (manBase.StartsWith("%") && manBase.IndexOf('%', 1) > 0)
                {
                    string[] info = manBase.Split('=');
                    if (info.Length > 1)
                        expectedValue = info[1].Trim();

                    info = info[0].Substring(1, info[0].IndexOf('%', 1) - 1).Split('.');
                    if (info.Length == 1)
                        fieldName = info[0];
                    else
                    {
                        panelName = info[0];
                        fieldName = info[1];
                    }
                }

                string baseField = panelName + "~" + fieldName + "~" + expectedValue + "~" + reqFieldName;
                if (!BaseFieldControlsToValidate.Contains(baseField))
                    BaseFieldControlsToValidate.Add(baseField);
            }
        }

        protected void ParentTextbox_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string parentTbValue = textBox.Text;

            var attributes = textBox.Attributes["validated-controls"].Split('|');

            foreach (string attr in attributes)
            {
                var valInfo = attr.Split(',');
                string expectedVal = valInfo[0];

                List<string> liExpectedValues = new List<string>();
                if (!string.IsNullOrEmpty(expectedVal))
                    liExpectedValues = expectedVal.Split('~').ToList();

                valInfo = valInfo[1].Split('.');
                string panelName = valInfo[0];
                string reqFieldName = valInfo[1];

                PLCDBPanel dbpanel = this;
                if (!string.IsNullOrEmpty(panelName))
                {
                    foreach (PLCDBPanel control in GetControlList<PLCDBPanel>(Page.Controls))
                        if (((PLCDBPanel)control).PLCPanelName == panelName)
                        {
                            dbpanel = (PLCDBPanel)control;
                            break;
                        }
                }

                bool isRequired = !string.IsNullOrEmpty(parentTbValue) && (string.IsNullOrEmpty(expectedVal) || liExpectedValues.Contains(parentTbValue.ToUpper().Trim()));
                dbpanel.SetFieldAsRequiredV2(reqFieldName, isRequired, false);
            }
        }

        protected void ParentFlexBox_ValueChanged(object sender, EventArgs e)
        {
            FlexBox flexBox = (FlexBox)sender;
            string parentFBValue = flexBox.GetValue();
            var attributes = flexBox.Attributes["validated-controls"].Split('|');

            foreach (string attr in attributes)
            {
                var valInfo = attr.Split(',');
                string expectedVal = valInfo[0];

                List<string> liExpectedValues = new List<string>();
                if (!string.IsNullOrEmpty(expectedVal))
                    liExpectedValues = expectedVal.Split('~').ToList();

                valInfo = valInfo[1].Split('.');
                string panelName = valInfo[0];
                string reqFieldName = valInfo[1];

                PLCDBPanel dbpanel = this;
                if (!string.IsNullOrEmpty(panelName))
                {
                    foreach (PLCDBPanel control in GetControlList<PLCDBPanel>(Page.Controls))
                        if (((PLCDBPanel)control).PLCPanelName == panelName)
                        {
                            dbpanel = (PLCDBPanel)control;
                            break;
                        }
                }

                bool isRequired = !string.IsNullOrEmpty(parentFBValue) && (string.IsNullOrEmpty(expectedVal) || liExpectedValues.Contains(parentFBValue.ToUpper().Trim()));
                dbpanel.SetFieldAsRequiredV2(reqFieldName, isRequired, false);
            }
        }

        private void SetDBPanelChildRequired()
        {
            Dictionary<string, string> MandatoryBases = new Dictionary<string, string>();
            foreach (PanelRec pr in this.panelrecs)
            {
                if (!string.IsNullOrEmpty(pr.mandatoryBase) && pr.mandatoryBase != "T")
                {
                    if(!FieldNameInList(pr.fldname))
                        MandatoryBases.Add(pr.fldname, pr.mandatoryBase);
                }
            }

            foreach (KeyValuePair<string, string> mb in MandatoryBases)
            {
                string panelName = "";
                string fieldName = "";
                string reqFieldName = mb.Key;
                string manBase = mb.Value;
                List<string> liExpectedValues = new List<string>();

                if (manBase == "T")
                {
                    //do nothing
                }
                else //macro mandatory base
                {
                    string expectedValue = "";
                    string baseFieldValue = "";

                    if (manBase.StartsWith("%") && manBase.IndexOf('%', 1) > 0)
                    {
                        string[] info = manBase.Split('=');
                        if (info.Length > 1)
                        {
                            expectedValue = info[1].Trim();
                            if (!string.IsNullOrEmpty(expectedValue))
                                liExpectedValues = expectedValue.Split(',').ToList();
                        }

                        info = info[0].Substring(1, info[0].IndexOf('%', 1) - 1).Split('.');
                        if (info.Length == 1)
                            fieldName = info[0];
                        else
                        {
                            panelName = info[0];
                            fieldName = info[1];
                        }
                    }

                    PLCDBPanel dbpanel = this;
                    if (!string.IsNullOrEmpty(panelName))
                    {
                        foreach (PLCDBPanel control in GetControlList<PLCDBPanel>(Page.Controls))
                            if (((PLCDBPanel)control).PLCPanelName == panelName)
                            {
                                dbpanel = (PLCDBPanel)control;
                                break;
                            }
                    }

                    baseFieldValue = dbpanel.getpanelfield(fieldName);
                    bool isRequired = !string.IsNullOrEmpty(baseFieldValue) && (string.IsNullOrEmpty(expectedValue) || liExpectedValues.Contains(baseFieldValue.ToUpper().Trim()));
                    SetFieldAsRequired(reqFieldName, isRequired, false);
                }
            }
        }

        private void BindOnChangeEventToControls(List<string> baseFieldControlsToValidate)
        {
            List<string> Validators = new List<string>();
            foreach (string controls in baseFieldControlsToValidate)
            {
                var controlInfo = controls.Split('~');
                string panelName = controlInfo[0];
                string baseFieldName = controlInfo[1];
                string baseFieldExpectedValue = controlInfo[2].Replace(",", "~").Replace(" ", "").ToUpper();
                string controlToValidate = controlInfo[3];

                PLCDBPanel dbpanel = this;
                if (!string.IsNullOrEmpty(panelName))
                {
                    foreach (PLCDBPanel control in GetControlList<PLCDBPanel>(Page.Controls))
                        if (((PLCDBPanel)control).PLCPanelName == panelName)
                        {
                            dbpanel = (PLCDBPanel)control;
                            break;
                        }
                }

                foreach (PanelRec pr in dbpanel.panelrecs)
                {
                    if (pr.fldname.Trim() == baseFieldName.Trim())
                    {
                        if (pr.chFlexBox != null)
                        {
                            FlexBox parentFlexBox = pr.chFlexBox;
                            parentFlexBox.AutoPostBack = true;

                            string validationInfo = baseFieldExpectedValue + "," + this.PLCPanelName + "." + controlToValidate;

                            if (parentFlexBox.Attributes["validated-controls"] != null)
                                parentFlexBox.Attributes["validated-controls"] = parentFlexBox.Attributes["validated-controls"].ToString() + "|" + validationInfo;
                            else
                                parentFlexBox.Attributes.Add("validated-controls", validationInfo);

                            if (!Validators.Contains(baseFieldName))
                                parentFlexBox.ValueChanged += new ValueChangedEventHandler(ParentFlexBox_ValueChanged);
                        }
                        else if (pr.tb != null)
                        {
                            TextBox parentTextbox = pr.tb;
                            parentTextbox.AutoPostBack = true;

                            string validationInfo = baseFieldExpectedValue + "," + this.PLCPanelName + "." + controlToValidate;

                            if (parentTextbox.Attributes["validated-controls"] != null)
                                parentTextbox.Attributes["validated-controls"] = parentTextbox.Attributes["validated-controls"].ToString() + "|" + validationInfo;
                            else
                                parentTextbox.Attributes.Add("validated-controls", validationInfo);

                            if (!Validators.Contains(baseFieldName))
                                parentTextbox.TextChanged += new EventHandler(ParentTextbox_TextChanged);
                        }

                        if (!Validators.Contains(baseFieldName))
                            Validators.Add(baseFieldName);
                    }
                }
            }
        }

        private void AddMandatoryChildFieldOverride(string fldName)
        {
            if (ViewState["MandatoryChildFieldOverride"] == null)
            {
                ViewState["MandatoryChildFieldOverride"] = new List<string>();
            }

            List<string> lstChildOverride = (List<string>)ViewState["MandatoryChildFieldOverride"];
            if (!lstChildOverride.Contains(fldName))
                lstChildOverride.Add(fldName);
        }

        private bool FieldNameInList(string fieldName)
        {
            if (ViewState["MandatoryChildFieldOverride"] == null)
                return false;

            List<string> lstChildOverride = (List<string>)ViewState["MandatoryChildFieldOverride"];
            return lstChildOverride.Contains(fieldName); 
        }

        private IEnumerable<T> GetControlList<T>(ControlCollection controlCollection) where T : Control
        {
            foreach (Control control in controlCollection)
            {
                if (control is T)
                    yield return (T)control;

                if (control.HasControls())
                    foreach (T childControl in GetControlList<T>(control.Controls))
                        yield return childControl;
            }
        }

        #endregion

        #region Add Base

        private void BindOnChangeEventToAddBases(string baseMode, List<string> baseFieldControlsToValidate)
        {
            string attributName = (baseMode.Equals("ADD") ? "add" : "edit") + "-base-controls";
            List<string> Validators = new List<string>();
            foreach (string controls in baseFieldControlsToValidate)
            {
                var controlInfo = controls.Split('~');
                string panelName = controlInfo[0];
                string baseFieldName = controlInfo[1];
                string baseFieldExpectedValue = controlInfo[2].Replace(",", "~").Replace(" ", "").ToUpper();
                string controlToValidate = controlInfo[3];

                PLCDBPanel dbpanel = this;
                if (!string.IsNullOrEmpty(panelName))
                {
                    foreach (PLCDBPanel control in GetControlList<PLCDBPanel>(Page.Controls))
                        if (((PLCDBPanel)control).PLCPanelName == panelName)
                        {
                            dbpanel = (PLCDBPanel)control;
                            break;
                        }
                }

                foreach (PanelRec pr in dbpanel.panelrecs)
                {
                    if (pr.fldname.Trim() == baseFieldName.Trim())
                    {
                        if (pr.chFlexBox != null)
                        {
                            FlexBox parentFlexBox = pr.chFlexBox;
                            parentFlexBox.AutoPostBack = true;

                            string validationInfo = baseFieldExpectedValue + "," + this.PLCPanelName + "." + controlToValidate;

                            if (parentFlexBox.Attributes[attributName] != null)
                                parentFlexBox.Attributes[attributName] = parentFlexBox.Attributes[attributName].ToString() + "|" + validationInfo;
                            else
                                parentFlexBox.Attributes.Add(attributName, validationInfo);

                            if (!Validators.Contains(baseFieldName))
                            {
                                parentFlexBox.ValueChanged -= new ValueChangedEventHandler(ParentAddEditFlexBox_ValueChanged);
                                parentFlexBox.ValueChanged += new ValueChangedEventHandler(ParentAddEditFlexBox_ValueChanged);
                            }
                        }
                        else if (pr.tb != null)
                        {
                            TextBox parentTextbox = pr.tb;
                            parentTextbox.AutoPostBack = true;

                            string validationInfo = baseFieldExpectedValue + "," + this.PLCPanelName + "." + controlToValidate;

                            if (parentTextbox.Attributes[attributName] != null)
                                parentTextbox.Attributes[attributName] = parentTextbox.Attributes[attributName].ToString() + "|" + validationInfo;
                            else
                                parentTextbox.Attributes.Add(attributName, validationInfo);

                            if (!Validators.Contains(baseFieldName))
                            {
                                parentTextbox.TextChanged -= new EventHandler(ParentAddEditBaseTextbox_TextChanged);
                                parentTextbox.TextChanged += new EventHandler(ParentAddEditBaseTextbox_TextChanged);
                            }
                        }

                        if (!Validators.Contains(baseFieldName))
                            Validators.Add(baseFieldName);
                    }
                }
            }
        }

        protected void ParentAddEditBaseTextbox_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string parentTbValue = textBox.Text;

            var attributes = textBox.Attributes["add-base-controls"].Split('|');
            if(!IsNewRecord)
                attributes = textBox.Attributes["edit-base-controls"].Split('|');

            foreach (string attr in attributes)
            {
                var valInfo = attr.Split(',');
                string expectedVal = valInfo[0];

                List<string> liExpectedValues = new List<string>();
                if (!string.IsNullOrEmpty(expectedVal))
                    liExpectedValues = expectedVal.Split('~').ToList();

                valInfo = valInfo[1].Split('.');
                string panelName = valInfo[0];
                string enableFieldName = valInfo[1];

                PLCDBPanel dbpanel = this;
                if (!string.IsNullOrEmpty(panelName))
                {
                    foreach (PLCDBPanel control in GetControlList<PLCDBPanel>(Page.Controls))
                        if (((PLCDBPanel)control).PLCPanelName == panelName)
                        {
                            dbpanel = (PLCDBPanel)control;
                            break;
                        }
                }

                bool isRestricted = false;
                PanelRec panelRec = dbpanel.GetPanelRecByFieldName(enableFieldName);
                if (panelRec != null)
                {
                    isRestricted = IsNewRecord ? !panelRec.AllowAdd : !panelRec.AllowEdit;
                }

                if (isRestricted)
                    continue;

                bool isEnabled = !string.IsNullOrEmpty(parentTbValue) && (string.IsNullOrEmpty(expectedVal) || liExpectedValues.Contains(parentTbValue.ToUpper().Trim()));
                if (!isEnabled)
                    dbpanel.setpanelfield(enableFieldName, string.Empty);
                dbpanel.EnablePanelField(enableFieldName, isEnabled);
            }
        }

        protected void ParentAddEditFlexBox_ValueChanged(object sender, EventArgs e)
        {
            FlexBox flexBox = (FlexBox)sender;
            string parentFBValue = flexBox.GetValue();

            var attributes = new string[0];
            if (flexBox.Attributes["add-base-controls"] != null)
                attributes = flexBox.Attributes["add-base-controls"].Split('|');

            if (!IsNewRecord)
            {
                attributes = new string[0];
                if (flexBox.Attributes["edit-base-controls"] != null)
                    attributes = flexBox.Attributes["edit-base-controls"].Split('|');
            }

            if (attributes.Length <= 0)
                return;

            foreach (string attr in attributes)
            {
                var valInfo = attr.Split(',');
                string expectedVal = valInfo[0];

                List<string> liExpectedValues = new List<string>();
                if (!string.IsNullOrEmpty(expectedVal))
                    liExpectedValues = expectedVal.Split('~').ToList();

                valInfo = valInfo[1].Split('.');
                string panelName = valInfo[0];
                string enableFieldName = valInfo[1];

                PLCDBPanel dbpanel = this;
                if (!string.IsNullOrEmpty(panelName))
                {
                    foreach (PLCDBPanel control in GetControlList<PLCDBPanel>(Page.Controls))
                        if (((PLCDBPanel)control).PLCPanelName == panelName)
                        {
                            dbpanel = (PLCDBPanel)control;
                            break;
                        }
                }

                bool isRestricted = false;
                PanelRec panelRec = dbpanel.GetPanelRecByFieldName(enableFieldName);
                if (panelRec != null)
                {
                    isRestricted = IsNewRecord ? !panelRec.AllowAdd : !panelRec.AllowEdit;
                }

                if (isRestricted)
                    continue;

                bool isEnabled = !string.IsNullOrEmpty(parentFBValue) && (string.IsNullOrEmpty(expectedVal) || liExpectedValues.Contains(parentFBValue.ToUpper().Trim()));
                if (!isEnabled)
                    dbpanel.setpanelfield(enableFieldName, string.Empty);
                dbpanel.EnablePanelField(enableFieldName, isEnabled);
            }
        }

        public void SetRestrictionsBasedOnAddEditBase(string baseMode)
        {
            string baseName = baseMode.Equals("ADD") ? "add-base-controls" : "edit-base-controls";
            foreach (PanelRec pr in this.panelrecs)
            {
                var attributes = new string[0];
                string parentValue = this.getpanelfield(pr.fldname);

                if (pr.chFlexBox != null)
                {
                    if (pr.chFlexBox.Attributes[baseName] != null)
                        attributes = pr.chFlexBox.Attributes[baseName].Split('|');
                }
                else if (pr.tb != null)
                {
                    if (pr.tb.Attributes[baseName] != null)
                        attributes = pr.tb.Attributes[baseName].Split('|');
                }

                if(attributes.Length <= 0)
                    continue;

                foreach (string attr in attributes)
                {
                    var valInfo = attr.Split(',');
                    string expectedVal = valInfo[0];

                    List<string> liExpectedValues = new List<string>();
                    if (!string.IsNullOrEmpty(expectedVal))
                        liExpectedValues = expectedVal.Split('~').ToList();

                    valInfo = valInfo[1].Split('.');
                    string panelName = valInfo[0];
                    string reqFieldName = valInfo[1];

                    PLCDBPanel dbpanel = this;
                    if (!string.IsNullOrEmpty(panelName))
                    {
                        foreach (PLCDBPanel control in GetControlList<PLCDBPanel>(Page.Controls))
                            if (((PLCDBPanel)control).PLCPanelName == panelName)
                            {
                                dbpanel = (PLCDBPanel)control;
                                break;
                            }
                    }

                    bool isRestricted = false;
                    PanelRec childRec = dbpanel.GetPanelRecByFieldName(reqFieldName);
                    if (childRec != null)
                    {
                        isRestricted = IsNewRecord ? !childRec.AllowAdd : !childRec.AllowEdit;
                    }

                    if (isRestricted)
                        continue;

                    bool isEnabled = !string.IsNullOrEmpty(parentValue) && (string.IsNullOrEmpty(expectedVal) || liExpectedValues.Contains(parentValue.ToUpper().Trim()));
                    dbpanel.EnablePanelField(reqFieldName, isEnabled);
                }              
            }
        }

        #endregion Add Base

        #region Validate Email Address

        /// <summary>
        /// This function checks if the string is a valid email address
        /// </summary>
        /// <param name="strIn"></param>
        /// <returns></returns>
        public static bool IsValidEmail(string strIn)
        {
            bool invalid = false;
            // Use IdnMapping class to convert Unicode domain names.
            try
            {
                var match = Regex.Match(strIn, @"(@)(.+)$");
                strIn = Regex.Replace(strIn, @"(@)(.+)$", DomainMapper(match, invalid), RegexOptions.None);
               
            }
            catch
            {
                return false;
            }
            
            if (invalid)
                return false;



            // Return true if strIn is in valid email format.
            try
            {
                return Regex.IsMatch(strIn,
                      @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                      RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        protected static string DomainMapper(Match match, bool invalid)
        {
            // IdnMapping class with default property values.
            IdnMapping idn = new IdnMapping();

            string domainName = match.Groups[2].Value;
            try
            {
                domainName = idn.GetAscii(domainName);
            }
            catch (ArgumentException)
            {
                invalid = true;
            }

            return match.Groups[1].Value + domainName;
        }

        #endregion


        #region Display Formula

        /// <summary>
        /// This will check if the DISPLAY_FORMULA field is valid before actually assigning its value in the PanelRec class
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static bool IsValidConfigSelectSQL(string sql)
        {
            sql = sql.Trim().ToUpper();
            return sql.StartsWith("SELECT") && !sql.Contains(";");
        }

        /// <summary>
        /// This is the function beibng used by PLCDBGrid in replacing macro values
        /// </summary>
        /// <param name="sql"></param>
        private void ProcessSQLMacro(ref string sql)
        {
            PLCSessionVars sv = new PLCSessionVars();
            int P;
            int P1;
            string TempString = sql;
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
                                default:
                                    SqlString += "Macro not found " + Macro;
                                    break;
                            }
                        }
                    }
                    P = TempString.IndexOf("<&>");
                }
                SqlString += TempString;

                sql = SqlString;

            }

            SqlString = sql;
            SqlString = SqlString.Replace("%USER%", "'" + PLCSession.PLCGlobalAnalyst + "'");
            SqlString = SqlString.Replace("%user%", "'" + PLCSession.PLCGlobalAnalyst + "'");
            sql = SqlString;

        }

        /// <summary>
        /// This works like the processmacro function but using {} instead of <&></&>
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="panelName"></param>
        /// <param name="currFieldName"></param>
        /// <param name="dbpanel"></param>
        private void ProcessFormulaField(ref string sql, string panelName, string currFieldName, PLCDBPanel dbpanel, string currFieldValue)
        {
            try
            {
                int startIndex;
                int endIndex;
                string tempString = sql;
                string htmlVal = "";
                string macro;

                if (tempString.IndexOf("{") >= 0)
                {
                    startIndex = tempString.IndexOf("{");
                    while (startIndex >= 0)
                    {
                        htmlVal += tempString.Substring(0, startIndex);
                        tempString = tempString.Remove(0, startIndex + 1);
                        endIndex = tempString.IndexOf("}");
                        if (endIndex >= 0)
                        {
                            macro = tempString.Substring(0, endIndex);
                            tempString = tempString.Remove(0, endIndex + 1);

                            if (!string.IsNullOrEmpty(macro))
                            {
                                string value = "";
                                if (macro.Contains("#VALUE"))
                                {
                                    if (dbpanel.IsNewRecord)
                                        value = string.IsNullOrEmpty(dbpanel.getpanelfield(currFieldName)) ? dbpanel.getpanelfield(currFieldName) : currFieldValue;
                                    else
                                        value = string.IsNullOrEmpty(currFieldValue) ? dbpanel.getpanelfield(currFieldName) : currFieldValue;

                                }
                                else if (macro.Contains("GLOBALANALYST"))
                                    value = PLCSession.PLCGlobalAnalyst;
                                else if (macro.Contains("GLOBALLABCODE"))
                                    value = PLCSession.PLCGlobalLabCode;
                                else
                                    value = GetFieldNameValue(macro, dbpanel);

                                htmlVal += value;
                            }
                        }

                        startIndex = tempString.IndexOf("{");
                    }

                    htmlVal += tempString;
                    sql = htmlVal;
                }

            }
            catch (Exception e)
            {
                sql = "";
            }

        }

        /// <summary>
        /// This function gets the value of the field inside the {} macro
        /// </summary>
        /// <param name="field"></param>
        /// <param name="dbpanel"></param>
        /// <returns></returns>
        private string GetFieldNameValue(string field, PLCDBPanel dbpanel)
        {
            string fieldValue = dbpanel.getpanelfield(field);
            if (field.Contains("TV_") && field.Contains("."))
            {
                string table = "";
                String[] parts = field.Split('.');
                if (parts.Count() == 2)
                {
                    table = parts[0];
                    field = parts[1];
                }

                fieldValue = dbpanel.getpanelfield(table, field);
            }

            return fieldValue;
        }
    

        /// <summary>
        /// This function will set the value of the DBPANEL field based on TV_DBPANEL.DISPLAY_FORMULA result provided that the query is valid
        /// </summary>
        public void SetDisplayFormulaFieldValue()
        {
            
            string panelName = this.PLCPanelName;
            PLCDBPanel dbpanel = this;
            if (!string.IsNullOrEmpty(panelName))
            {
                foreach (PLCDBPanel control in GetControlList<PLCDBPanel>(Page.Controls))
                    if (((PLCDBPanel)control).PLCPanelName == panelName)
                    {
                        dbpanel = (PLCDBPanel)control;
                        break;
                    }
            }


            if (dbpanel.IsNewRecord) return;

            foreach (PanelRec pr in this.panelrecs)
            {
                if (pr.tb != null && !string.IsNullOrEmpty(pr.tb.Attributes["DISPLAYFORMULAFIELD"]))
                {
                    string currValue = pr.tb.Attributes["FORMULAFIELDVALUE"].ToString();
                    string formula = pr.DisplayFormula;
                    ProcessSQLMacro(ref formula); //for DBGRID macros
                    ProcessFormulaField(ref formula, panelName, pr.fldname, dbpanel, currValue); //for DBPANEL field macros

                    if (!string.IsNullOrWhiteSpace(formula))
                    {
                        try
                        {
                            PLCQuery qryFormula = new PLCQuery();
                            qryFormula.SQL = PLCSession.FormatSpecialFunctions(formula);
                            qryFormula.Open();
                            if (!qryFormula.IsEmpty())
                            {
                                string formulaValue = qryFormula.FieldByIndex(0);
                                if (!string.IsNullOrWhiteSpace(formulaValue))
                                    pr.tb.Text = formulaValue;

                            }
                            else
                                pr.tb.Text = "";
                            
                        }
                        catch (Exception e)
                        {
                            //do nothing
                        }

                    }
                }


            }
        }

        #endregion

        /// <summary>
        /// Get the PanelRec that matches the field name and partially matches the prompt (TO, FROM, START, END)
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="prompt"></param>
        /// <returns></returns>
        public PanelRec GetDateRangePanelRec(string fieldName, string prompt)
        {
            List<PanelRec> fields = GetPanelFieldRecs().FindAll(pr => pr.fldname == fieldName && pr.prompt.ToUpper().Contains(prompt));
            if (fields.Count() > 0)
            {
                return fields.First();
            }
            return null;
        }

        /// <summary>
        /// Get the value of panel field that matches the field name and partially matches the prompt (TO, FROM, START, END)
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="prompt"></param>
        /// <returns></returns>
        public string GetDateRangeValue(string fieldName, string prompt)
        {
            PanelRec field = GetDateRangePanelRec(fieldName, prompt);
            if (field != null)
                return GetFieldValue(field.tblname, field.fldname, field.prompt);
            return "";
        }


        private string GetDeptLabCode(string departmentCode)
        {
            string deptLabCode = "";

            PLCQuery qryDeptname = new PLCQuery();
            qryDeptname.SQL = "SELECT LAB_CODE FROM TV_DEPTNAME WHERE DEPARTMENT_CODE = ?";
            qryDeptname.AddSQLParameter("DEPARTMENT_CODE", departmentCode);
            qryDeptname.Open();
            if (!qryDeptname.IsEmpty())
                deptLabCode = qryDeptname.FieldByName("LAB_CODE");

            return deptLabCode;
        }


        protected void ShowAlert(string message)
        {
            string script = "setTimeout(function () { "
                + "Alert('" + message.Replace("'", "\\'") + "'); "
                + "});";
            ScriptManager.RegisterStartupScript(this, this.GetType(), this.ID + "ShowAlert", script, true);
        }

        private bool IsDateValid(PanelRec pr, string currentValue)
        {
            string value = string.Empty;
            DateTime? dateCurrent = null;
            if (pr.tb != null && pr.isDateMask(pr.editmask))
            {
                if (!string.IsNullOrEmpty(currentValue))
                {
                    try
                    {
                        dateCurrent = ConvertToDateTime(currentValue, pr.editmask);
                        value = currentValue;
                    }
                    catch
                    {
                        value = string.Empty;
                    }
                }
            }

            return !string.IsNullOrEmpty(value);
        }

        private string GetMultiPickSelections(string listOptions)
        {
            if (!listOptions.Contains("{") || !listOptions.Contains("}"))
                return listOptions;

            string query = listOptions.Substring(listOptions.IndexOf("{") + 1, (listOptions.IndexOf("}")) - (listOptions.IndexOf("{") + 1));
            query = PLCSession.ProcessSQLMacro(query);

            PLCQuery qryRecords = new PLCQuery(query);
            qryRecords.Open();

            if (qryRecords.IsEmpty())
                return string.Empty;

            List<string> selections = new List<string>();
            string fieldName = qryRecords.FieldNames(1);

            while (!qryRecords.EOF())
            {
                selections.Add(qryRecords.FieldByName(fieldName));
                qryRecords.Next();
            }

            return string.Join(",", selections);
        }

        private void AssignFlexBoxParentControls(FlexBox flexBox)
        {
            int startIndex;
            int endIndex;
            string fieldName;
            Control parentControl;
            var parentControls = new Dictionary<string, Control>();
            string codeCondition = flexBox.CodeCondition;

            // get flexbox parent controls
            string cc = PLCSession.ReplaceSpecialKeysInCodeCondition(codeCondition);
            while (cc.Contains("{") && cc.Contains("}"))
            {
                // get field name
                startIndex = cc.IndexOf("{") + 1;
                endIndex = cc.IndexOf("}");
                fieldName = cc.Substring(startIndex, endIndex - startIndex);

                // find control if it exist then add as parent
                parentControl = GetControl(fieldName);
                if (parentControl != null)
                {
                    parentControls.Add(fieldName, parentControl);
                }

                // remove processed field
                cc = cc.Replace("{" + fieldName + "}", string.Empty);
            }

            // assign if multiple parents
            if (parentControls.Count > 1)
                flexBox.ParentControls = parentControls;
        }

        #region Public Methods

        /// <summary>
        /// Add additional code condition to a field
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="codeCondition"></param>
        public void AddFieldCodeCondition(string fieldName, string codeCondition)
        {
            PanelRec pr = panelrecs.FirstOrDefault(a => a.fldname.ToUpper() == fieldName.ToUpper());
            if (pr != null && !string.IsNullOrEmpty(codeCondition))
            {
                string condition = pr.codecondition;
                if (pr.chFlexBox != null)
                {
                    if (!string.IsNullOrEmpty(condition))
                        condition += " AND ";
                    condition += codeCondition;
                    pr.chFlexBox.CodeCondition = condition;
                }
                else if (pr.chMultiLookup != null)
                {
                    pr.chMultiLookup.AdditionalWhereClause(codeCondition);
                    condition = pr.chMultiLookup.WhereClause;
                }
                pr.codecondition = condition;
            }
        }

        /// <summary>
        /// Indicate if the panel name is configured in the database.
        /// </summary>
        public bool HasPanelConfig()
        {
            string queryString = "SELECT PANEL_NAME FROM TV_DBPANEL WHERE PANEL_NAME = ?";
            var queryObject = new PLCQuery(queryString);
            queryObject.AddParameter("PANEL_NAME", PLCPanelName);
            queryObject.Open();
            return queryObject.HasData();
        }

        #endregion


        #region NOT Filter Checkbox

        /// <summary>
        /// Get the Not Filter checkbox
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public CheckBox GetNotFilterCheckbox(string fieldName, string prompt = "")
        {
            foreach (PanelRec pr in this.panelrecs)
            {
                if (pr.fldname.Trim() == fieldName.Trim() && (string.IsNullOrEmpty(prompt) || pr.prompt.Trim() == prompt.Trim()))
                    return pr.chkNotFilter;
            }
            return null;
        }

        /// <summary>
        /// Get the value of Not Filter checkbox
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="prompt"></param>
        /// <returns></returns>
        public bool GetNotFilterValue(string fieldName, string prompt = "")
        {
            CheckBox chkNotFilter = GetNotFilterCheckbox(fieldName);
            if (chkNotFilter != null)
                return chkNotFilter.Checked;
            return false;
        }

        /// <summary>
        /// Set the value of Not Filter checkbox
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="isChecked"></param>
        /// <param name="prompt"></param>
        public void SetNotFilterValue(string fieldName, bool isChecked, string prompt = "")
        {
            CheckBox chkNotFilter = GetNotFilterCheckbox(fieldName, prompt);
            if (chkNotFilter != null)
                chkNotFilter.Checked = isChecked;
        }

        /// <summary>
        /// Set the value of Not Filter checkbox for all DBPanel fields
        /// </summary>
        /// <param name="isChecked"></param>
        public void SetAllNotFilterValue(bool isChecked)
        {
            foreach (PanelRec pr in this.panelrecs)
            {
                if (pr.chkNotFilter != null)
                    pr.chkNotFilter.Checked = isChecked;
            }
        }

        #endregion

        #region Null or Blank Options

        /// <summary>
        /// Add the Null, Blank and Null or Blank options, pass the control.Items property
        /// </summary>
        /// <param name="items"></param>
        protected void AddNullOrBlankOptions(ListItemCollection items)
        {
            items.Add(new ListItem("Null", "_NULL"));
            items.Add(new ListItem("Blank", "_BLANK"));
            items.Add(new ListItem("Null or Blank", "_NOB"));
        }

        #endregion
    }
}

// Example ("Provider=msdaora;Data Source=LABSYS;User Id=LABSYS;Password=labsys"))
// PLCConnectionString  Data Source=PLCORA1;User Id=FDLE;Password=FDLE
// PLCProviderName = Provider=msdaora

