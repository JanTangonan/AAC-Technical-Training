<ajaxToolkit:ModalPopupExtender ID="mpeAnalysisPanelSort" runat="server" BehaviorID="bhvAPS"
    BackgroundCssClass="modalBackground" PopupControlID="pnlAnalysisPanelSort" OkControlID="btnDummyAPS"
    CancelControlID="btnCancelSort" TargetControlID="btnDummyAPS">
</ajaxToolkit:ModalPopupExtender>
<asp:Panel ID="pnlAnalysisPanelSort" runat="server" CssClass="modalPopup" Width="400px">
    <asp:Panel ID="pnlAPSCaption" runat="server" CssClass="caption">Sort Analysis Panels</asp:Panel>
    <div style="padding: 10px">
        <div id="pnlAPS" style="border: 1px solid gray;"></div>
        <br />
        <center>
            <asp:Button ID="btnSaveSort" runat="server" Text="Save" OnClientClick="SaveAnalysisPanelSortOrder(false); return false;" />
            <asp:Button ID="btnCancelSort" runat="server" Text="Cancel" />
            <asp:Button ID="btnDummyAPS" runat="server" style="visibility: hidden; display: none;" />
        </center>
    </div>
</asp:Panel>