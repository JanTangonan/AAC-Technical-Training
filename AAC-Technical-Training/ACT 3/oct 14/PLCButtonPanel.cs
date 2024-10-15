using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Web.UI.Design;
using System.Web.UI.Design.WebControls;
using AjaxControlToolkit;

namespace PLCCONTROLS
{

    [Designer(typeof(PLCButtonPanelDesigner)), ParseChildren(false)]
    [ToolboxData("<{0}:PLCButtonPanel runat=server></{0}:PLCButtonPanel>")]
    public class PLCButtonPanel : Panel
    {
        public event PLCButtonClickEventHandler PLCButtonClick;

        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (this.Site != null)
            {
                if (this.Site.DesignMode) return;
            }

            dynactrl();
        }
        
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!string.IsNullOrEmpty(PLCTargetControlID))
            {
                // Set button panel for readonly access
                // when target dbpanel have no fields
                var targetControl = this.Parent.FindControl(PLCTargetControlID);
                if (targetControl != null
                    && targetControl is PLCDBPanel)
                {
                    var dbPanel = (PLCDBPanel)targetControl;
                    if (dbPanel.PLCPanelName != "UNKNOWN"
                        && dbPanel.NumVisiblePanelRecs() < 1)
                    {
                        const string CONFIGCHECK = "BUTTONPANELCONFIGCHECK";
                        var appSettings = System.Configuration.ConfigurationManager.AppSettings;
                        if (!appSettings.AllKeys.Contains(CONFIGCHECK)
                            || !appSettings[CONFIGCHECK].Trim().ToUpper().Equals("OFF"))
                        {
                            DisableButtonsForReadOnlyAccess();
                        }
                        PLCSession.WriteDebug("ButtonPanel: No Visible DBPanel fields: " + dbPanel.PLCPanelName);
                    }
                }
            }
        }


        [Bindable(true)]
        [Category("PLC Properties"), Description("Set this to true to show the edit/save/cancel buttons")]
        [DefaultValue(false)]
        [Localizable(true)]
        public Boolean PLCShowEditButtons
        {
            get
            {
                if (ViewState["PLCShowEditButtons"] == null)
                    return false;
                else
                {
                    Boolean b = (Boolean)ViewState["PLCShowEditButtons"];
                    return b;
                }

            }

            set
            {
                ViewState["PLCShowEditButtons"] = value;
            }
        }

        [Bindable(true)]
        [Category("PLC Properties"), Description("Set this to true to show the Add button")]
        [DefaultValue(false)]
        [Localizable(true)]
        public Boolean PLCShowAddButton
        {
            get
            {
                if (ViewState["PLCShowAddButton"] == null)
                    return false;
                else
                {
                    Boolean b = (Boolean)ViewState["PLCShowAddButton"];
                    return b;
                }

            }

            set
            {
                ViewState["PLCShowAddButton"] = value;
            }
        }

        [Bindable(true)]
        [Category("PLC Properties"), Description("Set this property to True to display a <HR> element below the button panel")]
        [DefaultValue(false)]
        [Localizable(true)]
        public Boolean PLCDisplayBottomBorder
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

        [Bindable(true)]
        [Category("PLC Properties"), Description("Set this property to True to display a <HR> element on top of the button panel")]
        [DefaultValue(false)]
        [Localizable(true)]
        public Boolean PLCDisplayTopBorder
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

        [Bindable(true)]
        [Category("PLC Properties"), Description("use this property to link the button panel to a DBPanel or other PLCData Control")]
        [DefaultValue(false)]
        [Localizable(true)]
        public string PLCTargetControlID
        {
            get
            {
                if (ViewState["PLCTargetControlID"] == null)
                    return "";
                else
                {
                    string s = (string)ViewState["PLCTargetControlID"];
                    return s;
                }

            }

            set
            {
                ViewState["PLCTargetControlID"] = value;
            }
        }

        public string PLCTargetDBGridID = "";

        //---------------------------------------------------------------------------------------------------

        [Bindable(true)]
        [Category("PLC Properties"), Description("Set this to true to show the Delete button")]
        [DefaultValue(false)]
        [Localizable(true)]
        public Boolean PLCShowDeleteButton
        {
            get
            {
                if (ViewState["PLCShowDeleteButton"] == null)
                    return false;
                else
                {
                    Boolean b = (Boolean)ViewState["PLCShowDeleteButton"];
                    return b;
                }

            }

            set
            {
                ViewState["PLCShowDeleteButton"] = value;
            }
        }

        
        [Bindable(true)]
        [Category("PLC Properties"), Description("This is a comma seperated list of button names")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCCustomButtons
        {
            get
            {
                String s = (String)ViewState["PLCCustomButtons"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["PLCCustomButtons"] = value;
            }
        }

        //*AAC 06/02/2010
        [Bindable(true)]
        [Category("PLC Properties"), Description("This is to set a fixed width for custom buttons. Will automatically adjust if the text is too long for the assigned width.")]
        [DefaultValue(0)]
        [Localizable(true)]
        public int PLCCustomButtonFixedWidth
        {
            get
            {
                if (ViewState["PLCCustomButtonFixedWidth"] == null)
                    return 0;
                else
                {
                    int size = (int) ViewState["PLCCustomButtonFixedWidth"];
                    return size;
                }
            }

            set
            {
                ViewState["PLCCustomButtonFixedWidth"] = value;
            }
        }

		[Category("PLC Properties"), Description("Set this to false to prevent setting the Default Button of TargetControl to 'Save' button.")]
		[DefaultValue(true)]
		public Boolean PLCSetDefaultButton
		{
			get
			{
				if (ViewState["PLCSetDefaultButton"] == null)
					return true;
				else
				{
					Boolean b = (Boolean)ViewState["PLCSetDefaultButton"];
					return b;
				}
			}
			set
			{
				ViewState["PLCSetDefaultButton"] = value;
			}
		}

        [Category("PLC Properties"), Description("Delete message")]
        [DefaultValue("Delete, Are you sure?")]
        public string PLCDeleteMessage
        {
            get
            {
                String s = (String)ViewState["PLCDeleteMessage"];
                return ((s == null) ? "Delete, Are you sure?" : s);
            }

            set
            {
                ViewState["PLCDeleteMessage"] = value;
            }
        }

        private void dynactrl()
        {
            int btnwidth = 65;

            Table tbl = new Table();
            TableRow tr = new TableRow();
            TableCell tc = new TableCell();
            Button mybutton = null;
            Literal myLiteral;

            Int16 tabIndex = 1000;

            if (!string.IsNullOrEmpty(PLCTargetControlID) && this.Parent.FindControl(PLCTargetControlID) != null && this.Parent.FindControl(PLCTargetControlID) is PLCDBPanel)
            {
                tabIndex = ((PLCDBPanel)this.Parent.FindControl(PLCTargetControlID)).HighestTabIndex;

                if (tabIndex <= 0)
                    tabIndex = 1000;
            }
            
            if (PLCShowAddButton)
            {

                //add
                mybutton = new Button();
                mybutton.Width = Unit.Point(btnwidth);
                mybutton.ID = this.ID + "_DBP_ADDBUTTON";
                mybutton.Text = PLCSession.GetSysPrompt("ButtonPanel.Add", "Add");
                mybutton.Click += addbutton_Click;
                tabIndex++;
                mybutton.TabIndex = tabIndex;
                tc.Controls.Add(mybutton);
                //hrwidth = 65;
            }

            if (PLCShowEditButtons)
            {

                //Edit

                mybutton = new Button();
                mybutton.AccessKey = "E";
                mybutton.Width = Unit.Point(btnwidth);
                mybutton.ID = this.ID + "_DBP_EDITBUTTON";
                mybutton.Text = PLCSession.GetSysPrompt("ButtonPanel.Edit", "Edit");
                mybutton.Click += editbutton_Click;
                tabIndex++;
                mybutton.TabIndex = tabIndex;
                tc.Controls.Add(mybutton);


                //Save
                mybutton = new Button();
                mybutton.Width = Unit.Point(btnwidth);
                mybutton.ID = this.ID + "_DBP_SAVEBUTTON";
                mybutton.AccessKey = "S";
                mybutton.Text = PLCSession.GetSysPrompt("ButtonPanel.Save", "Save");
                mybutton.Click += savebutton_Click;
                tabIndex++;
                mybutton.TabIndex = tabIndex;
                mybutton.OnClientClick = "this.disabled = true; this.value = 'Wait...';";
                mybutton.UseSubmitBehavior = false;
                tc.Controls.Add(mybutton);
				if (PLCSetDefaultButton)
					SetPanelDefaultButton(mybutton);
                
                // Hidden session autosave button.
                if (PLCSession.GetWebConfiguration("DBPANEL_ISAUTOSAVE") == "T")
                {
                    Button hdnbtnAutoSave = new Button();
                    hdnbtnAutoSave.ID = this.ID + "_DBP_SESSIONAUTOSAVEBUTTON";
                    hdnbtnAutoSave.Text = "AutoSave";
                    hdnbtnAutoSave.Click += sessionautosavebutton_Click;
                    hdnbtnAutoSave.UseSubmitBehavior = false;
                    hdnbtnAutoSave.Style.Add("display", "none");
                    tc.Controls.Add(hdnbtnAutoSave);
                }

                //Cancel

                mybutton = new Button();
                mybutton.Width = Unit.Point(btnwidth);
                mybutton.ID = this.ID + "_DBP_CANCELBUTTON";
                mybutton.Text = PLCSession.GetSysPrompt("ButtonPanel.Cancel", "Cancel");
                mybutton.AccessKey = "C";
                mybutton.Click += cancelbutton_Click;
                tabIndex++;
                mybutton.TabIndex = tabIndex;

                //Ticket#37756 - Cancel button in PLCButtonPanel shouldn't act like a default submit button.
                mybutton.UseSubmitBehavior = false;

                tc.Controls.Add(mybutton);

                //hrwidth += 195;
            }


            //Delete

            if (PLCShowDeleteButton)
            {

                mybutton = new Button();
                mybutton.Width = Unit.Point(btnwidth);
                mybutton.ID = this.ID + "_DBP_DELETEBUTTON";
                mybutton.Text = PLCSession.GetSysPrompt("ButtonPanel.Delete", "Delete");
                mybutton.Click += deletebutton_Click;
                mybutton.OnClientClick = "this.disabled = true; this.value = 'Wait...';";
                mybutton.UseSubmitBehavior = false;
                tabIndex++;
                mybutton.TabIndex = tabIndex;
                tc.Controls.Add(mybutton);

               
                    PLCDBPanel Panel = (PLCDBPanel)this.Parent.FindControl(PLCTargetControlID);
                    if (Panel != null)
                        if (Panel.PLCDataTable != "TV_LABITEM")
                        {
                            // Add the "Are you sure?" extender
                            ConfirmButtonExtender cbe = new ConfirmButtonExtender();
                            cbe.TargetControlID = mybutton.ID;
                            cbe.ConfirmText = PLCDeleteMessage;
                            tc.Controls.Add(cbe);
                        }
                

                mybutton.UseSubmitBehavior = false;

                //hrwidth += 65;
            }

            //hrwidth += 130;
            //*AAC - 06/02/2010 - set a fixed width to custom buttons. If text is too long, ScaleToFit() will adjust the width.
            if (PLCCustomButtons != "")
            {
                Literal mySpacer = new Literal();
                mySpacer.Text = "&nbsp;&nbsp;&nbsp;&nbsp;";
                tc.Controls.Add(mySpacer);
                
                char[] DelimChars = { ',' };
                string[] btnnames = PLCCustomButtons.Split(DelimChars);

                foreach (string s in btnnames)
                {
                    mybutton = new Button();
                    //mybutton.Width = Unit.Point(btnwidth);
                    mybutton.ID = this.ID + "_DBP_Custom_" + s;
                    mybutton.Text = PLCSession.GetSysPrompt("ButtonPanel." + s.Replace(" ", "_"), s);
                    mybutton.CommandName = s;
                    mybutton.Width = Unit.Point(PLCCustomButtonFixedWidth);
//                    ScaleToFit(mybutton, s, new Font(mybutton.Font.Name, 10));
                    ScaleToFit(mybutton, s, new Font(mybutton.Font.Name, 14));
                    mybutton.Click += custombutton_Click;
                    tabIndex++;
                    mybutton.TabIndex = tabIndex;
                    tc.Controls.Add(mybutton);
                    //hrwidth += Convert.ToInt32(mybutton.Width.Value);
                }
            }

            tr.Cells.Add(tc);

            //if (hrwidth < 700)
            //    hrwidth = 700;

            //Empty Label For Error Message.....
            Label lblError = new Label();
            lblError.ID = this.ID + "_DBP_ERROR";
            lblError.Text = "";
            lblError.ForeColor = Color.Red;
            tc.Controls.Add(lblError);

            tbl.Rows.Add(tr);


            //myLiteral = new Literal();
            //myLiteral.Text = "<div class='plcdbbuttonpanelScroll'>";
            //this.Controls.Add(myLiteral);


            if (PLCDisplayTopBorder)
            {

                //  myLiteral = new Literal();
                //  myLiteral.Text = "<p /> ";
                //  this.Controls.Add(myLiteral);
                // align='left' width='" + Unit.Point(hrwidth) + "'>
                myLiteral = new Literal();
                myLiteral.Text = "<hr>";
                //tc.Controls.Add(myLiteral);
                //tr.Controls.Add(tc);
                //tbl.Controls.Add(tr);
                this.Controls.Add(myLiteral);
            }

            this.Controls.Add(tbl);



            if (PLCDisplayBottomBorder)
            {
                myLiteral = new Literal();
                myLiteral.Text = "<hr>";
                this.Controls.Add(myLiteral);
            }



        }

        private void SetPanelDefaultButton(Button mybutton)
        {
            if (!string.IsNullOrEmpty(PLCTargetControlID))
            {
                if (this.Parent.FindControl(PLCTargetControlID) != null)
                {
                    PLCControlPanel ThePanel = (PLCControlPanel)this.Parent.FindControl(PLCTargetControlID);
                    if (ThePanel is PLCDBPanel)
                    {
                        ThePanel.DefaultButton = mybutton.UniqueID;
                    }
                }
            }
        }

        //*AAC 061509
        private void ScaleToFit(Button btn, string s, Font font)
        {
            Bitmap bitmap = new Bitmap(300, 50);
            Graphics graphics = Graphics.FromImage(bitmap);
            SizeF textSize = new SizeF();
            textSize = graphics.MeasureString(s, font);
            if (btn.Width.Value <= Convert.ToInt32(textSize.Width))
               // btn.Width = new Unit(textSize.Width + 2);
            btn.Width = new Unit(textSize.Width + 8);
        }


        protected void addbutton_Click(object sender, EventArgs e)
        {
            PLCControlPanel ThePanel = null;

            if (!DoButtonEvent("ADD", "BEFORE")) return;

            ViewState["AddMode"] = "T";
            
            if (PLCTargetControlID != "")
            {
                ThePanel = (PLCControlPanel)this.Parent.FindControl(PLCTargetControlID);
                if (ThePanel != null)
                {
                    if (!ThePanel.CanAdd() ) return;
                    if (!ThePanel.DoAdd() ) return;
                }
            }

            InternalEditMode();

            DoButtonEvent("ADD", "AFTER");

            if (PLCTargetControlID != "")
            {
                if (this.Parent.FindControl(PLCTargetControlID) != null && this.Parent.FindControl(PLCTargetControlID) is PLCDBPanel)
                {
                    ThePanel = (PLCDBPanel)this.Parent.FindControl(PLCTargetControlID);
                    if (ThePanel != null)
                    {
                        ((PLCDBPanel)ThePanel).FocusFirstField();
                    }
                }
            }

            if (ThePanel != null)
                ThePanel.ResetOriginalValues();
        }


        protected void editbutton_Click(object sender, EventArgs e)
        {


            if (!DoButtonEvent("EDIT", "BEFORE")) return;

            ViewState["AddMode"] = "F";

            if (PLCTargetControlID != "")
            {
                PLCControlPanel ThePanel = (PLCControlPanel)this.Parent.FindControl(PLCTargetControlID);
                if (ThePanel != null)
                {                  
                    if ( !ThePanel.CanEdit()  ) return;
                    if ( !ThePanel.DoEdit() ) return;
                }
            }

            InternalEditMode();

            DoButtonEvent("EDIT", "AFTER");

            if (PLCTargetControlID != "")
            {
                if (this.Parent.FindControl(PLCTargetControlID) != null && this.Parent.FindControl(PLCTargetControlID) is PLCDBPanel)
                {
                    PLCDBPanel ThePanel = (PLCDBPanel)this.Parent.FindControl(PLCTargetControlID);
                    if (ThePanel != null)
                    {
                        ThePanel.FocusFirstField();
                    }
                }
            }
        }

        public void SetSaveError(string msg)
        {
           Label lblError = (Label)this.FindControl(this.ID + "_DBP_ERROR");
           if (lblError == null) return;
           lblError.Text = msg;
        }


        protected void savebutton_Click(object sender, EventArgs e)
        {
            DoSave();
        }

        protected void sessionautosavebutton_Click(object sender, EventArgs e)
        {
            DoSave();
            this.Page.Session.Abandon();
            ScriptManager.RegisterStartupScript(Parent.Page, typeof(Page), "endsession", "if (IsDefined('ShowSessionTimeoutAlert')) ShowSessionTimeoutAlert();", true);
        }

        private void DoSave()
        {
            PLCControlPanel ThePanel = null;

            if (PLCTargetControlID != "")
            {
                ThePanel = (PLCControlPanel)this.Parent.FindControl(PLCTargetControlID);
                if (ThePanel != null)
                {
                    if (!ThePanel.CanSave())
                    {
                        SetSaveError("    Unable to save this information. Look for errors in red.");
                        return;
                    }
                    SetSaveError("");
                    
                    if (!DoButtonEvent("SAVE", "BEFORE")) return; // AACI(05/04/2011): Moved SAVE BEFORE event after validating the panel.
                    if (!ThePanel.DoSave()) return;
                }
            }

            InternalBrowseMode();

            DoButtonEvent("SAVE", "AFTER");

            if (ThePanel != null)
                ThePanel.ResetOriginalValues();

            // If target dbgrid control is set in add mode, scroll to the selected row so that the 
            // newly added record will be visible.
            if ((this.PLCTargetDBGridID != "") && ((string) ViewState["AddMode"] == "T"))
            {
            //    string script = String.Format("ScrollDbGridToBottom('{0}', 1000);", PLCTargetDBGridID);
            //    ScriptManager.RegisterStartupScript(Parent.Page, typeof(Page), "scrollbottom" + PLCTargetDBGridID, script, true);
                ((PLCDBGrid)this.Parent.FindControl(PLCTargetDBGridID)).SetClientSideScrollToSelectedRow();
            }

            ViewState["AddMode"] = "F";
        }

        protected void cancelbutton_Click(object sender, EventArgs e)
        {

            if (!DoButtonEvent("CANCEL", "BEFORE")) return;

            if (PLCTargetControlID != "")
            {
                PLCControlPanel ThePanel = (PLCControlPanel)this.Parent.FindControl(PLCTargetControlID);
                if (ThePanel != null)
                {
                    if (!ThePanel.CanCancel()) return;
                    if (!ThePanel.DoCancel()) return;
                }
            }

            SetSaveError("");

            InternalBrowseMode();

            DoButtonEvent("CANCEL", "AFTER");

          ViewState["AddMode"] = "F";

        }


        protected void deletebutton_Click(object sender, EventArgs e)
        {

            if (!DoButtonEvent("DELETE", "BEFORE"))  return;

            ViewState["AddMode"] = "F";

            if (PLCTargetControlID != "")
            {
                PLCControlPanel ThePanel = (PLCControlPanel)this.Parent.FindControl(PLCTargetControlID);
                if (ThePanel != null)
                {
                    if (!ThePanel.CanDelete()) return;
                    if (!ThePanel.DoDelete()) return;
                }
            }
                        
            DoButtonEvent("DELETE", "AFTER");

        }

        public void ClickEditButton()
        {
            Button mybutton = null;

            mybutton = (Button)this.FindControl(this.ID + "_DBP_EDITBUTTON");
            if (mybutton != null)
                editbutton_Click(mybutton, EventArgs.Empty);
        }

        public void ClickAddButton()
        {
            Button mybutton = null;

            mybutton = (Button)this.FindControl(this.ID + "_DBP_ADDBUTTON");
            if (mybutton != null)
                addbutton_Click(mybutton, EventArgs.Empty);
        }

        public void ClickSaveButton()
        {
            Button mybutton = null;

            mybutton = (Button)this.FindControl(this.ID + "_DBP_SAVEBUTTON");
            if (mybutton != null)
                savebutton_Click(mybutton, EventArgs.Empty);
        }

        public void ClickCancelButton()
        {
            Button mybutton = null;

            mybutton = (Button)this.FindControl(this.ID + "_DBP_CANCELBUTTON");
            if (mybutton != null)
                cancelbutton_Click(mybutton, EventArgs.Empty);
        }

        public void ClickDeleteButton()
        {
            Button mybutton = null;

            mybutton = (Button)this.FindControl(this.ID + "_DBP_DELETEBUTTON");
            if (mybutton != null)
                deletebutton_Click(mybutton, EventArgs.Empty);
        }

        public void HideDeleteButton()
        {
            Button deleteButton = (Button)this.FindControl(this.ID + "_DBP_DELETEBUTTON");
            if (deleteButton != null)
                deleteButton.Visible = false;
        }

        public void HideCancelButton()
        {
            Button cancelButton = (Button)this.FindControl(this.ID + "_DBP_CANCELBUTTON");
            if (cancelButton != null)
                cancelButton.Visible = false;
        }

        public void HideEditButton()
        {
            Button editButton = (Button)this.FindControl(this.ID + "_DBP_EDITBUTTON");
            if (editButton != null)
                editButton.Visible = false;
        }

        public void HideAddButton()
        {
            Button addButton = (Button)this.FindControl(this.ID + "_DBP_ADDBUTTON");
            if (addButton != null)
                addButton.Visible = false;
        }

        public void ShowEditButton()
        {
            Button editButton = (Button)this.FindControl(this.ID + "_DBP_EDITBUTTON");
            if (editButton != null)
                editButton.Visible = true;
        }

        public void ShowAddButton()
        {
            Button addButton = (Button)this.FindControl(this.ID + "_DBP_ADDBUTTON");
            if (addButton != null)
                addButton.Visible = true;
        }

        public void ShowCancelButton()
        {
            Button cancelButton = (Button)this.FindControl(this.ID + "_DBP_CANCELBUTTON");
            if (cancelButton != null)
                cancelButton.Visible = true;
        }
        
        public void SetEditMode()
        {
            InternalEditMode();
        }


        private void InternalEditMode()
        {

                    
            Button mybutton = null;

            mybutton = (Button)this.FindControl(this.ID + "_DBP_ADDBUTTON");
            if (mybutton != null) mybutton.Enabled = false;

            mybutton = (Button)this.FindControl(this.ID + "_DBP_EDITBUTTON");
            if (mybutton != null) mybutton.Enabled = false;

            mybutton = (Button)this.FindControl(this.ID + "_DBP_DELETEBUTTON");
            if (mybutton != null) mybutton.Enabled = false;

            mybutton = (Button)this.FindControl(this.ID + "_DBP_SAVEBUTTON");
            if (mybutton != null) mybutton.Enabled = true;

            mybutton = (Button)this.FindControl(this.ID + "_DBP_SESSIONAUTOSAVEBUTTON");
            if (mybutton != null) mybutton.Enabled = true;

            mybutton = (Button)this.FindControl(this.ID + "_DBP_CANCELBUTTON");
            if (mybutton != null) mybutton.Enabled = true;

            SetCustomButtonStatus(false);

            SetMainTabEditMode(true);
        }


        public void SetBrowseMode()
        {
            InternalBrowseMode();
        }


        private void InternalBrowseMode()
        {
         
                 
               Button mybutton = null;

               try
               {
                   mybutton = (Button)this.FindControl(this.ID + "_DBP_ADDBUTTON");
               }
               catch
               {
                   mybutton = null;
               }
            
                if (mybutton != null) mybutton.Enabled = true;

                try
                {
                    mybutton = (Button)this.FindControl(this.ID + "_DBP_EDITBUTTON");
                }
                catch
                {
                    mybutton = null;
                }
            
                if (mybutton != null) mybutton.Enabled = true;

                try
                {
                    mybutton = (Button)this.FindControl(this.ID + "_DBP_DELETEBUTTON");
                }
                catch
                {
                    mybutton = null;
                }

                if (mybutton != null) mybutton.Enabled = true;

                try
                {
                    mybutton = (Button)this.FindControl(this.ID + "_DBP_SAVEBUTTON");
                }
                catch
                {
                    mybutton = null;
                }

                if (mybutton != null) mybutton.Enabled = false;


                try
                {
                    mybutton = (Button)this.FindControl(this.ID + "_DBP_SESSIONAUTOSAVEBUTTON");
                }
                catch
                {
                    mybutton = null;
                }

                if (mybutton != null) mybutton.Enabled = false;

                try
                {

                    mybutton = (Button)this.FindControl(this.ID + "_DBP_CANCELBUTTON");
                }
                catch
                {
                    mybutton = null;
                }

                if (mybutton != null) mybutton.Enabled = false;

                SetCustomButtonStatus(true);

                SetMainTabEditMode(false);
        }

        //
        public void SetEmptyMode()
        {
            InternalEmptyMode();
        }


        private void InternalEmptyMode()
        {
            Button mybutton = null;

            mybutton = (Button)this.FindControl(this.ID + "_DBP_ADDBUTTON");
            if (mybutton != null) mybutton.Enabled = true;

            mybutton = (Button)this.FindControl(this.ID + "_DBP_EDITBUTTON");
            if (mybutton != null) mybutton.Enabled = false;

            mybutton = (Button)this.FindControl(this.ID + "_DBP_DELETEBUTTON");
            if (mybutton != null) mybutton.Enabled = false;

            mybutton = (Button)this.FindControl(this.ID + "_DBP_SAVEBUTTON");
            if (mybutton != null) mybutton.Enabled = false;

            mybutton = (Button)this.FindControl(this.ID + "_DBP_SESSIONAUTOSAVEBUTTON");
            if (mybutton != null) mybutton.Enabled = false;

            mybutton = (Button)this.FindControl(this.ID + "_DBP_CANCELBUTTON");
            if (mybutton != null) mybutton.Enabled = false;

            SetCustomButtonStatus(false);

            SetMainTabEditMode(false);
        }

        public void SetButtonsForLock(bool Lock)
        {
            Button mybutton = null;

            mybutton = (Button)this.FindControl(this.ID + "_DBP_EDITBUTTON");
            if (mybutton != null) mybutton.Enabled = !Lock;

            mybutton = (Button)this.FindControl(this.ID + "_DBP_DELETEBUTTON");
            if (mybutton != null) mybutton.Enabled = !Lock;

            mybutton = (Button)this.FindControl(this.ID + "_DBP_Custom_" + "RecordUnlock");
            if (mybutton != null) mybutton.Enabled = Lock;
        }

        public void DisableEditDeleteButton()
        {
            Button mybutton = null;
            mybutton = (Button)this.FindControl(this.ID + "_DBP_EDITBUTTON");
            if (mybutton != null) mybutton.Enabled = false;

            mybutton = (Button)this.FindControl(this.ID + "_DBP_DELETEBUTTON");
            if (mybutton != null) mybutton.Enabled = false;
        }

        public void DisableButton(string ButtonName)
        {
            Button mybutton = null;
            mybutton = (Button)this.FindControl(this.ID + "_DBP_" + ButtonName + "BUTTON");
            if (mybutton != null)
                mybutton.Enabled = false;
            else
            {
                mybutton = (Button)this.FindControl(this.ID + "_DBP_Custom_" + ButtonName);
                if (mybutton != null) mybutton.Enabled = false;                
            }            
        }

        public void EnableButton(string ButtonName)
        {
            Button mybutton = null;
            mybutton = (Button)this.FindControl(this.ID + "_DBP_" + ButtonName + "BUTTON");
            if (mybutton != null)
                mybutton.Enabled = true;
            else
            {
                mybutton = (Button)this.FindControl(this.ID + "_DBP_Custom_" + ButtonName);
                if (mybutton != null) mybutton.Enabled = true;
            }
        }
        
        /// <summary>
        /// Currently disables ADD, EDIT and DELETE buttons.
        /// </summary>
        public void DisableButtonsForReadOnlyAccess()
        {
            List<string> MainButtons = new List<string>();
            MainButtons.Add("_DBP_ADDBUTTON");
            MainButtons.Add("_DBP_EDITBUTTON");
            MainButtons.Add("_DBP_DELETEBUTTON"); 

            Button btn;
            foreach (String RequiredButton in MainButtons)
            {
                btn = (Button)this.FindControl(this.ID + RequiredButton);
                if (btn != null) btn.Enabled = false;
            }            
        }

        public void DisableLock()
        {
            Button mybutton = null;
            mybutton = (Button)this.FindControl(this.ID + "_DBP_Custom_" + "RecordUnlock");
            if (mybutton != null) mybutton.Enabled = false;

            
        }
        public void enableLock()
        {
            Button mybutton = null;
            mybutton = (Button)this.FindControl(this.ID + "_DBP_Custom_" + "RecordUnlock");
            if (mybutton != null) mybutton.Enabled = true;


        }

        public void SetCustomButtonMode(string ButtonName, bool Enable)
        {
            Button mybutton = null;
            mybutton = (Button)this.FindControl(this.ID + "_DBP_Custom_" + ButtonName);
            if (mybutton != null) mybutton.Enabled = Enable;
        }

        public void SetCustomButtonVisible(string ButtonName, bool Visible)
        {
            Button mybutton = null;
            mybutton = (Button)this.FindControl(this.ID + "_DBP_Custom_" + ButtonName);
            if (mybutton != null) mybutton.Visible = Visible;
        }
        public void SetCustomButtonEnabled(string ButtonName, bool enabled)
        {
            Button mybutton = null;
            mybutton = (Button)this.FindControl(this.ID + "_DBP_Custom_" + ButtonName);
            if (mybutton != null) mybutton.Enabled = enabled;
        }

        public void SetCustomButtonCaption(string ButtonName, string ButtonCaption)
        {
            Button mybutton = null;
            mybutton = (Button)this.FindControl(this.ID + "_DBP_Custom_" + ButtonName);
            if (mybutton != null)
            {
                mybutton.ID = this.ID + "_DBP_Custom_" + ButtonCaption;
                mybutton.Text = ButtonCaption;
            }
        }

        public void SetCustomButtonText(string ButtonName, string ButtonText)
        {
            Button mybutton = null;
            mybutton = (Button)this.FindControl(this.ID + "_DBP_Custom_" + ButtonName);
            if (mybutton != null)
            {
                mybutton.Text = ButtonText;
            }
        }

        /// <summary>
        /// Set the custom button's color
        /// </summary>
        /// <param name="pstrButtonName">The custom button name</param>
        /// <param name="pobjColor">The color to change into</param>
        public void SetCustomButtonColor(string pstrButtonName, Color pobjColor)
        {
            Button mybutton = null;
            mybutton = (Button)this.FindControl(this.ID + "_DBP_Custom_" + pstrButtonName);
            if (mybutton != null)
            {
                mybutton.ForeColor = pobjColor;
            }
        }
        
        public void DisableAllButtons()
        {
            Button mybutton = null;

            mybutton = (Button)this.FindControl(this.ID + "_DBP_ADDBUTTON");
            if (mybutton != null) mybutton.Enabled = false;

            mybutton = (Button)this.FindControl(this.ID + "_DBP_EDITBUTTON");
            if (mybutton != null) mybutton.Enabled = false;

            mybutton = (Button)this.FindControl(this.ID + "_DBP_DELETEBUTTON");
            if (mybutton != null) mybutton.Enabled = false;

            mybutton = (Button)this.FindControl(this.ID + "_DBP_SAVEBUTTON");
            if (mybutton != null) mybutton.Enabled = false;

            mybutton = (Button)this.FindControl(this.ID + "_DBP_SESSIONAUTOSAVEBUTTON");
            if (mybutton != null) mybutton.Enabled = false;

            mybutton = (Button)this.FindControl(this.ID + "_DBP_CANCELBUTTON");
            if (mybutton != null) mybutton.Enabled = false;

            SetCustomButtonStatus(false);

        }


    
      protected void custombutton_Click(object sender, EventArgs e)
        {

          Button mybutton = (Button)sender;
          string btntext = mybutton.CommandName;
         
          DoButtonEvent(btntext,"CUSTOM");

        }


     public void SetCustomButtonStatus(Boolean disable)
        {

            if (PLCCustomButtons != "")
            {

                char[] DelimChars = { ',' };
                string[] btnnames = PLCCustomButtons.Split(DelimChars);
                string btnID = "";
                Button mybutton = null;

                foreach (string s in btnnames)
                {

                    btnID = this.ID + "_DBP_Custom_" + s;
                    mybutton = (Button)this.FindControl(btnID);
                    if (mybutton != null)
                    {
                        mybutton.Enabled = disable;
                    }

                }

            }
        }



        protected Boolean DoButtonEvent(string btnname, string btnaction)
        {
            if (PLCButtonClick == null) return true;

            Boolean recordadded = false;
            if (ViewState["AddMode"] == null)
                recordadded = false;
            else
                recordadded = ((string)ViewState["AddMode"] == "T");

            PLCButtonClickEventArgs e = new PLCButtonClickEventArgs(btnname, btnaction, false, recordadded);

            try { PLCButtonClick(this, e); }
            catch (Exception ex)
            {
                const string DEVDEBUG = "DEVDEBUG";
                var appSettings = System.Configuration.ConfigurationManager.AppSettings;
                if (appSettings.AllKeys.Contains(DEVDEBUG))
                {
                    string errorLog = string.Format("ButtonPanel: {0} - {1}\nError: {2}\n{3}",
                        btnname,
                        btnaction,
                        ex.Message,
                        ex.StackTrace);
                    PLCSession.ForceWriteDebug(errorLog, true);
                    this.SetSaveError("An error has occurred, please check debug log");
                }

                return false;
            }
            if (btnaction.ToUpper() == "AFTER")
            {
                String eventName = "DBButtonPanelButtonClick";
                String js = "_doDBPanelButtonEvent('" + btnname + "');";
                ScriptManager.RegisterStartupScript(Parent.Page, typeof(Page), eventName, js, true);
            }

            return (!e.button_canceled);
        }

        protected void SetMainTabEditMode(bool EditMode)
        {        
            //Main Tab
            if (this.Page == null) return;
            if (this.Page.Master == null) return;

            Menu MasterPageMenu = (Menu)this.Page.Master.FindControl("menu_main");

            if (MasterPageMenu != null)
            {
                //*AAC 10222010
                //foreach (MenuItem item in MasterPageMenu.Items)
                //    item.Enabled = !EditMode;
                MasterPageMenu.Enabled = !EditMode;
            }

          
                //Barcode field
            try
            {
                TextBox txtBarCode = (TextBox)((UserControl)this.Page.Master.FindControl("UC_CustomerTitle_Master2")).FindControl("txtBarCode");
                if (txtBarCode != null)
                    txtBarCode.ReadOnly = EditMode;
            }
            catch { }
           
            HyperLink lnkDashboard = (HyperLink)this.Page.Master.FindControl("lnkDashboard");
            if (lnkDashboard != null)
                lnkDashboard.Enabled = !EditMode;
          
            //         
            LinkButton lnkLogout = (LinkButton)this.Page.Master.FindControl("lnkLogout");
            if (lnkLogout != null)
                lnkLogout.Enabled = !EditMode;
         

                //Case Search Popup
                ImageButton btnCaseSearch = (ImageButton)this.Page.Master.FindControl("btnCaseSearch");
                if (btnCaseSearch != null)
                    btnCaseSearch.Enabled = !EditMode;

                ////Paper Clip image button
                //ImageButton ImageButton1 = (ImageButton)this.Page.Master.FindControl("ImageButton1");
                //if (ImageButton1 != null)
                //    ImageButton1.Enabled = !EditMode;

                ////Dashboard Link button
                //LinkButton lbDashBoard = (LinkButton)((UserControl)this.Page.Master.FindControl("UC_CustomerTitle_Master2")).FindControl("lbDashBoard");
                //if (lbDashBoard != null)
                //    lbDashBoard.Enabled = !EditMode;

                ////Log Out link button
                //LinkButton lbLogOut = (LinkButton)((UserControl)this.Page.Master.FindControl("UC_CustomerTitle_Master2")).FindControl("lbLogOut");
                //if (lbLogOut != null)
                //    lbLogOut.Enabled = !EditMode;

        }

        public Button GetCustomButton(string ButtonName)
        {
            Button mybutton = null;
            mybutton = (Button)this.FindControl(this.ID + "_DBP_Custom_" + ButtonName);
            if (mybutton != null) return mybutton;

            return null;
        }
      
    }
}
