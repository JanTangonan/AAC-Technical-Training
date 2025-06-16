<asp:Panel ID="pnlFilters" runat="server">
    <table style="width:100%;">
        <tr>
            <td>
                <asp:Label ID="lblAnalyst" runat="server" Text="User" />
            </td>
            <td style="width:50%; text-align:left">
                <asp:HiddenField ID="hfPropAnalyst" runat="server" />
                <PLC:FlexBox ID="fbAnalyst" runat="server" Width="200px" AutoPostBack="true"
                    OnValueChanged="Flexbox_ValueChanged" />
            </td>
            <td style="width:50%; text-align:right">
                <asp:Button ID="btnAllAnalyst" runat="server" Text="Show All" OnClick="btnAllAnalyst_Click"
                    Width="120" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:Label ID="lblType" runat="server" Text="Type" />
            </td>
            <td style="width:50%; text-align:left">
                <asp:HiddenField ID="hfPropType" runat="server" />
                <PLC:FlexBox ID="fbType" runat="server" TableName="TV_SCHTYPE" Width="200px" AutoPostBack="true"
                    OnValueChanged="Flexbox_ValueChanged" />
            </td>
            <td style="width:50%; text-align:right">
                <asp:Button ID="btnCurrentAnalyst" runat="server" Text="Show Mine" OnClick="btnCurrentAnalyst_Click"
                    Width="120" />
            </td>
        </tr>
    </table>
</asp:Panel>