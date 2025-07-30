<asp:Panel ID="pnlConfirmLinkedItem" CssClass="modalPopup" runat="server" Style="display: none">
    <asp:Panel ID="Panel4" runat="server">
        <asp:Panel runat="Server" ID="panel5">
            <table width="500px">
                <tr>
                    <td colspan="2" align="center">
                        <asp:Panel ID="Panel6" runat="server" Height="20px" CssClass="caption">Confirm</asp:Panel>
                    </td>
                </tr>
                <tr>
                    <td align="center" width="80px">
                        <asp:Image ID="Image2" runat="server" ImageUrl="~/Images/msginfo.jpg" />
                    </td>
                    <td align="left" width="420px">
                        <asp:Label ID="lblMsg" runat="server" Text=""></asp:Label>
                        Continue? (Y/N)
                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                        <br />
                        <div id="Div1" runat="server" align="center">
                            <asp:Button ID="btnLinkedItemOk" runat="server" Text="OK" Width="100px" />
                            <asp:Button ID="btnLinkedItemCancel" runat="server" Text="Cancel" Width="100px" />
                            <br />
                        </div>
                    </td>
                </tr>
            </table>
            <br />
        </asp:Panel>
    </asp:Panel>
</asp:Panel>
<ajaxToolkit:ModalPopupExtender ID="mpeConfirmLinkedItem" runat="server" TargetControlID="btnLinkedItemOk"
    PopupControlID="pnlConfirmLinkedItem" OkControlID="btnLinkedItemOk" OnOkScript="onConfirmOK()"
    CancelControlID="btnLinkedItemCancel" OnCancelScript="onConfirmCancel()" BackgroundCssClass="modalBackground">
</ajaxToolkit:ModalPopupExtender>