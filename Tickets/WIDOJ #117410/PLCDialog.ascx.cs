using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PLCWebCommon
{
    public partial class PLCDialog : System.Web.UI.UserControl
    {
        #region Public Properties

        public event DialogEventHandler CancelClick
        {
            add { _cancelClick += value; }
            remove { _cancelClick -= value; }
        }

        public event DialogEventHandler ConfirmClick
        {
            add { _confirmClick += value; }
            remove { _confirmClick -= value; }
        }

        public event DialogEventHandler OkClick
        {
            add { _okClick += value; }
            remove { _okClick -= value; }
        }

        public string DialogKey
        {
            get { return this._dialogKey; }
            set { this._dialogKey = value; }
        }

        public string Width
        {
            get { return this._width; }
            set { this._width = value; }
        }

        public string MaxHeight
        {
            get { return this._maxHeight; }
            set { this._maxHeight = value; }
        }

        public string IconType
        {
            get { return this._iconType; }
            set { this._iconType = value; }

        }

        #endregion

        #region Private Properties

        private DialogEventHandler _confirmClick;
        private DialogEventHandler _cancelClick;
        private DialogEventHandler _okClick;
        private string _dialogKey;
        private string _width = "300px";
        private string _maxHeight = "400px";
        private string _iconType = "info";

        #endregion

        #region Events

        protected override void OnInit(EventArgs e)
        {
            Page.RegisterRequiresControlState(this);
            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        protected override object SaveControlState()
        {
            object baseState = base.SaveControlState();
            object controlState = new object[] { baseState, this._dialogKey, this._maxHeight, this._width, this._iconType };
            return controlState;
        }

        protected override void LoadControlState(object savedState)
        {
            object[] stateLastRequest = (object[])savedState;

            object baseState = stateLastRequest[0];
            base.LoadControlState(baseState);

            this._dialogKey = (string)stateLastRequest[1];
            this._maxHeight = (string)stateLastRequest[2];
            this._width = (string)stateLastRequest[3];
            this._iconType = (string)stateLastRequest[4];
        }

        protected void btnDialogConfirm_Click(object sender, EventArgs e)
        {
            if (_confirmClick != null)
            {
                DialogEventArgs args = new DialogEventArgs();
                args.DialogKey = _dialogKey;
                _confirmClick(this, args);
            }
        }

        protected void btnDialogCancel_Click(object sender, EventArgs e)
        {
            if (_cancelClick != null)
            {
                DialogEventArgs args = new DialogEventArgs();
                args.DialogKey = _dialogKey;
                _cancelClick(this, args);
            }
        }

        protected void btnDialogOk_Click(object sender, EventArgs e)
        {
            if (_okClick != null)
            {
                DialogEventArgs args = new DialogEventArgs();
                args.DialogKey = _dialogKey;
                _okClick(this, args);
            }
        }

        #endregion

        #region Public Methods

        public void ShowAlert(string title, string message)
        {
            btnDialogOk.Attributes.Add("autopostback", _okClick != null ? "T" : "F");
            ScriptManager.RegisterStartupScript(this, GetType(), this.ClientID,
                string.Format("{0}_ShowAlert('{1}', '{2}', '{3}', '{4}');", this.ID, title, message, this._maxHeight, this._width), true);
        }

        public void ShowConfirm(string title, string message)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), this.ClientID,
                string.Format("{0}_ShowConfirm('{1}', '{2}', '{3}', '{4}');", this.ID, title, message, this._maxHeight, this._width), true);
        }

        public void ShowYesNo(string title, string message)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), this.ClientID,
                string.Format("{0}_ShowYesNo('{1}', '{2}', '{3}', '{4}');", this.ID, title, message, this._maxHeight, this._width), true);
        }

        /// <summary>
        /// Show [Y/N] prompt without close button and have postback button optional
        /// </summary>
        /// <param name="Title"></param>
        /// <param name="Message"></param>
        public void ShowYesNoOnly(string title, string message)
        {
            btnDialogConfirm.Attributes.Add("autopostback", _confirmClick != null ? "T" : "F");
            btnDialogCancel.Attributes.Add("autopostback", _cancelClick != null ? "T" : "F");
            ScriptManager.RegisterStartupScript(this, GetType(), this.ClientID,
                string.Format("{0}_ShowYesNoOnly('{1}', '{2}', '{3}', '{4}', '{5}');", this.ID, title, message, this._maxHeight, this._width, this._iconType), true);
        }

        public void ShowMsg(string title, string message, int messageType, string controlID, string postBackID, string okButtonText, string cancelButtonText)
        {
            txtDialogComment.Value = string.Empty;

            switch (messageType)
            {
                case 0: _iconType = "info"; break;
                case 1: _iconType = "error"; break;
                case 2: _iconType = "warning"; break;
                default: _iconType = "info"; break;
            }

            // Run client script — note the two new parameters: controlID, postBackID
            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                this.ClientID,
                string.Format("{0}_ShowMsg('{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}');",
                    this.ID, title, message, _maxHeight, _width, _iconType, okButtonText, cancelButtonText, controlID, postBackID),
                true
            );
        }


        #endregion

        #region Private Methods

        #endregion
    }

    public class DialogEventArgs : EventArgs
    {
        private String _dialogKey;
        public String DialogKey
        {
            get { return _dialogKey; }
            set { _dialogKey = value; }
        }
    }

    public delegate void DialogEventHandler(object sender, DialogEventArgs e);
}




