<asp:Button ID="btnSelectAnalysis" runat="server" Text="Add Analysis" OnClientClick="return false;"
    style="display: none;" />
<input type="button" class="default" value="Add Analysis" onclick="OpenAddAnalysis();"
    style='display: <%= (IsReadOnly) ? "none" : "inline" %>;' />
<asp:HiddenField ID="hdnPrintInfo" runat="server" />
<asp:Button ID="btnSortAnalysis" runat="server" Text="Sort Analysis" OnClientClick="SortAnalysisPanels(false); return false;" Visible="false" />
<asp:Button ID="btnPullLIst" runat="server" Text="Print" Width="120px" OnClientClick="SendToPrint(); return false;" />
<a href="MatrixReport.aspx" id="lnkMatrixReport" style="display: none;"></a>

--
<ajaxToolkit:ModalPopupExtender ID="mpeAnalysisPanelSort" runat="server" BehaviorID="bhvAPS" 
        BackgroundCssClass="modalBackground" PopupControlID="pnlAnalysisPanelSort" 
        OkControlID="btnDummyAPS" CancelControlID="btnCancelSort" TargetControlID="btnDummyAPS" >
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


<div class="sortanalysis-panels">
    <div class="sortanalysis-panels-item">
        <div>
            <b>Sort Analysis Panels</b>
            <ul id="sortanalysisPanelList" class="sortanalysis-panels-item"></ul>
        </div>
    </div>
    Drag & drop to sort the Analysis panels.
</div>