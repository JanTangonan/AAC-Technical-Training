<asp:Repeater ID="myRepeater" runat="server">
    <ItemTemplate>
        <div style="margin-bottom: 15px;">
            <asp:Label ID="lblPrompt" runat="server"></asp:Label>
            
            <%-- Conditionally render TextBox or DropDownList --%>
            <asp:PlaceHolder ID="phTextBox" runat="server" Visible='<%# Eval("ControlType").ToString() == "TextBox" %>'>
                <asp:TextBox ID="txtInput" runat="server"></asp:TextBox>
            </asp:PlaceHolder>

            <asp:PlaceHolder ID="phDropDownList" runat="server" Visible='<%# Eval("ControlType").ToString() == "DropDownList" %>'>
                <asp:DropDownList ID="ddlInput" runat="server"></asp:DropDownList>
            </asp:PlaceHolder>

            <%-- Label for mandatory fields --%>
            <asp:Label ID="lblMandatory" runat="server" Text=""></asp:Label>
        </div>
    </ItemTemplate>
</asp:Repeater>