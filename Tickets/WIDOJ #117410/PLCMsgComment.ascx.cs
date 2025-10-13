using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

namespace PLCWebCommon
{
    public partial class PLCMsgComment : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
        }

        public void ShowMsg(string sTitle, string sMessage, int nMessageType, string ControlID, string PostBackID)
        {
            OkButton.Text = "OK";
            CancelButton.Text = "Cancel";
            SetCommentProperties(sTitle, sMessage, nMessageType, ControlID, PostBackID);
        }

        public void ShowMsg(string sTitle, string sMessage, int nMessageType, string ControlID, string PostBackID, string okButtonText, string cancelButtonText)
        {
            OkButton.Text = okButtonText;
            CancelButton.Text = cancelButtonText;
            SetCommentProperties(sTitle, sMessage, nMessageType, ControlID, PostBackID);
        }

        private void SetCommentProperties(string sTitle, string sMessage, int nMessageType, string ControlID, string PostBackID)
        {
            txtComment.Text = "";
            LiteralControl lcText = new LiteralControl();
            lcText.Text = "&nbsp;&nbsp;" + sTitle;
            MsgCaption.Controls.Add(new LiteralControl("<font color=\"#FFFFFF\" face=\"Arial\" size=\"2\">"));
            MsgCaption.Controls.Add(lcText);
            MsgCaption.Controls.Add(new LiteralControl("</font>"));

            if (nMessageType == 0)
                imgMsg.ImageUrl = "~/Images/msginfo.jpg";
            else if (nMessageType == 1)
                imgMsg.ImageUrl = "~/Images/msgerror.jpg";

            lblMsg.Text = sMessage;
            mpeMsgBox.Show();
            OkButton.Attributes.Add("onclick", "OK_Click('" + ControlID + "', '" + PostBackID + "');");
            CancelButton.Attributes.Add("onclick", "Cancel_Click('" + ControlID + "', '" + PostBackID + "');");
            txtComment.Attributes.Add("onkeydown", "EnableDisableOKButton('" + OkButton.ClientID + "', '" + txtComment.ClientID + "');");
            txtComment.Focus();
        }
    }
}