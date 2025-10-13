<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="PLCMsgComment.ascx.cs" Inherits="PLCWebCommon.PLCMsgComment" %>
<%@ Register Assembly="PLCCONTROLS" Namespace="PLCCONTROLS" TagPrefix="cc1" %>
<%@ Register assembly="AjaxControlToolkit" namespace="AjaxControlToolkit" tagprefix="cc1" %>
 

<script type="text/javascript">    
    var textboxFocusID;
    var bnPostBackID;
    var bnOkButtonID;
    var textboxCommentID;
        
    function fnSetFocus(txtClientId)
    {        
    	textboxFocusID=txtClientId;
    	setTimeout("fnFocus()",1000);        
    }

    function fnFocus()
    {        
        var txt = document.getElementById(textboxFocusID);                
        if (txt != null)
        {
            try
            {
                txt.focus();
                txt.select();
            }
            catch(err)
            {
            }
        }
    }
    
    function Timer_EnableDisableOKButton()
    {
        var str = document.getElementById(textboxCommentID).value;
        str = str.replace(/^\s+|\s+$/g, '');
        if (str == '')
        {
            document.getElementById(bnOkButtonID).disabled = true;
        }
        else
        {
            document.getElementById(bnOkButtonID).disabled = false;
        }      
    }
    
    function EnableDisableOKButton(okID, commentID)
    {        
        bnOkButtonID = okID;
        textboxCommentID = commentID;        
        // use timer to check comment field
        setTimeout("Timer_EnableDisableOKButton()",2000);        
    }
    
    function DoPostBack()
    {
        var bn = document.getElementById(bnPostBackID);
        console.log(bnPostBackID);
        if (bn != null)
        {           
            console.log("buton clicked");
            bn.click();
        }        
    }    
    
    function OK_Click(ctrlID, postbackID) 
    {           
        try
        {        
            if (ctrlID != "")
            {  
                // check comment is not empty on OK
                if (document.getElementById("<%=txtComment.ClientID %>").value != '') 
                {
                    document.getElementById(ctrlID).value = document.getElementById("<%=txtComment.ClientID %>").value;                          
                    if (postbackID != "")
                    {   
                        bnPostBackID = postbackID;                 
                        setTimeout("DoPostBack()", 1000);        
                    }
                }                
            }
        }
        catch(err)
        {            
        }
        
    }           
    
    function Cancel_Click(ctrlID, postbackID) 
    {   
        try
        {        
            if (ctrlID != "")
            {   
                document.getElementById(ctrlID).value = '';

                if (typeof MsgComment_Cancel == "function")
                    MsgComment_Cancel(postbackID);
            }
        }
        catch(err)
        {            
        }
        
    }           

    
    </script>

<asp:Button ID="DummyButton" runat="server" style='display: none;visible:false;' />    
    <cc1:ModalPopupExtender ID="mpeMsgBox" 
            runat="server" TargetControlID="DummyButton" PopupControlID="ParentPanel"                        
            DropShadow="false" BackgroundCssClass="modalBackground"
            PopupDragHandleControlID="MsgCaption" Drag="true">
    </cc1:ModalPopupExtender>         
    <asp:Panel ID="ParentPanel" runat="server" CssClass="modalPopup" Style="display: none" Width="350px">                  
        <asp:Panel ID="MsgCaption" runat="server" Height="20px" CssClass="caption"></asp:Panel>					
            <asp:UpdatePanel ID="UpdatePanel3" runat="server" UpdateMode="Conditional">                
                <ContentTemplate>             
                    <table>                                            
                        <tr>
                            <td style="width:20%; ">
                                <asp:Image ID="imgMsg" runat="server" /> 
                            </td>
                            <td style="width:75%">
                                <asp:Label ID="lblMsg" runat="server" Text="Message Here" Font-Names="Arial" Font-Size="Small"></asp:Label>
                            </td>
                            <td style="width:5%">
                            </td>
                        </tr>
                        <tr>
                            <td style="width:20%; ">                                
                            </td>
                            <td style="width:75%">
                                <asp:TextBox ID="txtComment" runat="server" Width="250px" Height="70px" TextMode="MultiLine"></asp:TextBox>
                            </td>
                            <td style="width:5%">
                            </td>
                        </tr>
                    </table>                    
               </ContentTemplate>                
               <Triggers>
                <asp:AsyncPostBackTrigger ControlID="OkButton" EventName="Click" />                
               </Triggers>
            </asp:UpdatePanel>
            <br />
            <div align="center">
                <asp:Button ID="OkButton" runat="server" Width="70px" Height="25px" Font-Size="Smaller" Font-Bold="false" Enabled="false"/>
                <asp:Button ID="CancelButton" runat="server" Width="70px" Height="25px" Font-Size="Smaller" Font-Bold="false"/>
                <br /><br />                                
            </div>            
        </asp:Panel>
        