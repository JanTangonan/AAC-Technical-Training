<ajaxToolkit:ModalPopupExtender ID="mpeComment" BehaviorID="mpeComment" runat="server" BackgroundCssClass="modalBackground" 
        PopupControlID="pnlComment" PopupDragHandleControlID="pnlCommentCaption" TargetControlID="bnCommentDummy" >
    </ajaxToolkit:ModalPopupExtender>
    <asp:Panel ID="pnlComment" runat="server" CssClass="modalPopup" Width="350px" style="display: none;">
        <asp:Panel ID="pnlCommentCaption" runat="server" CssClass="caption" Width="100%" >Case Update Reason</asp:Panel>
        <div style="width:100%; margin-top:10px;" >
            <span style="width:20%; vertical-align:top; text-align:center; float:left;" ><img src="<%= ResolveUrl("~/Images/msginfo.jpg") %>" alt="" /></span>
            <span style="width:75%;" >
                <label style="font-family:Arial; font-size:small; width:100%; " >Please enter the reason for your changes</label>
                <textarea id="txtCommentReason" rows="8" cols="29" style="margin-top:10px;" ></textarea>
            </span>
        </div>
        <div style="width:100%; height:35px; margin-top:10px; text-align:center;" >
            <button id="bnCommentSave" type="button" style="width:70px; height:25px;" >Save</button>
            <button id="bnCommentCancel" type="button" onclick="$find('mpeComment').hide();" style="width:70px; height:25px; margin-left:3px;" >Cancel</button>
        </div> 
    </asp:Panel>


