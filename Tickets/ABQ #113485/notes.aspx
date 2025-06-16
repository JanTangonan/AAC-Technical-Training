<asp:View ID="View1" runat="server">
    <asp:Panel ID="Panel1" runat="server" Height="" Width="100%" HorizontalAlign="Left" style="margin-top:16px;">
        <PLC:PLCDBPanel ID="dbpAssignSearch" runat="server" PLCPanelName="ASSIGNSEARCH"
            OnPLCDBPanelCodeHeadChanged="dbpAssignSearch_PLCDBPanelCodeHeadChanged"
            OnPLCDBPanelAddCustomFields="dbpAssignSearch_PLCDBPanelAddCustomFields"
            OnPreRender="dbpAssignSearch_PreRender" IsSearchPanel="true" PLCAttachPopupTo="body"
            DefaultButton="bnRequestFind" />
        <br />
        <asp:Button ID="bnRequestFind" runat="server" Text="Search" OnClick="bnRequestFind_Click" Width="140px" />
        <asp:Button ID="bnClear" runat="server" Text="Clear" OnClick="bnClear_Click" Width="140px" />
        <asp:CheckBox ID="chkDRF" runat="server" Text="Default Report Format" />
    </asp:Panel>
</asp:View>
<asp:View ID="View2" runat="server">
    <asp:UpdatePanel ID="UpdatePanel1" runat="server" style="margin-top: 16px;">
        <ContentTemplate>
            <asp:TextBox ID="txtCriteria" Height="50" Width="100%" Wrap="true" TextMode="MultiLine" Text=""
                ReadOnly="true" runat="server"></asp:TextBox>
            <PLC:PLCDBGrid ID="dbgAssignSearch" runat="server" AllowSorting="true" AllowPaging="true"
                PLCGridWidth="100%" FirstColumnCheckbox="true" HorizontalAlign="Left"
                DataKeyNames="CASE_KEY,EXAM_KEY,SECTION,BATCH_ASSIGN_KEY"
                EmptyDataText="Select criteria in the Search tab." PLCGridName="ASSIGNSEARCH"
                OnRowDataBound="dbgAssignSearch_RowDataBound" OnRowCreated="dbgAssignSearch_RowCreated"
                OnRowCommand="dbgAssignSearch_RowCommand" OnSelectedIndexChanged="dbgAssignSearch_SelectedIndexChanged"
                UseDataCaching="true">
            </PLC:PLCDBGrid>
            <hr />
            <PLC:PLCDBPanel ID="dbpAssignSearchInfo" runat="server" PLCPanelName="ASSIGNSEARCHINFO"
                PLCDataTable="TV_LABEXAM" PLCWhereClause="Where 0 = 1" IsSearchPanel="true" />
            <PLC:MessageBox ID="msgPrompt" runat="server" PanelCSSClass="modalPopup" CaptionCSSClass="caption"
                PanelBackgroundCSSClass="modalBackground" MessageType="Information" />
        </ContentTemplate>