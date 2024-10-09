<asp:GridView ID="GridView1" runat="server" AutoPostBack="True" 
    OnSelectedIndexChanged="GridView1_SelectedIndexChanged"
    OnRowDataBound="GridView1_RowDataBound"
    CssClass="gridview-table">
    <Columns>
        <asp:BoundField DataField="DepartmentCaseNumber" HeaderText="Dept Case #" />
        <asp:BoundField DataField="LabCaseNumber" HeaderText="Lab Case #" />
        <asp:BoundField DataField="ReportIncidentDate" HeaderText="Incident Date" />
        <asp:BoundField DataField="Department" HeaderText="Department" />
        <asp:BoundField DataField="Charge" HeaderText="Charge" />
        <asp:TemplateField>
            <ItemTemplate>
                <!-- Edit Button -->
                <asp:Button ID="btnEdit" runat="server" Text="Edit"
                    OnClientClick="DisableOtherRows(this); return false;" 
                    OnClick="btnEdit_Click" />
                
                <!-- Save Button -->
                <asp:Button ID="btnSave" runat="server" Text="Save"
                    OnClientClick="EnableAllRows(); return false;"
                    OnClick="btnSave_Click" />
                
                <!-- Cancel Button -->
                <asp:Button ID="btnCancel" runat="server" Text="Cancel"
                    OnClientClick="EnableAllRows(); return false;"
                    OnClick="btnCancel_Click" />
            </ItemTemplate>
        </asp:TemplateField>
    </Columns>
</asp:GridView>

<script type="text/javascript">
    function DisableOtherRows(editButton) {
        // Get the parent row of the clicked Edit button
        var selectedRow = editButton.closest('tr');
        // Get all rows in the GridView
        var allRows = document.querySelectorAll('#<%= GridView1.ClientID %> tr');

        // Loop through all rows and disable those that are not selected
        allRows.forEach(function (row) {
            if (row !== selectedRow) {
                row.classList.add('disabled-row');
                row.style.pointerEvents = "none";  // Disable mouse events
                row.style.opacity = "0.5";  // Visually indicate disabled state
            }
        });
    }

    function EnableAllRows() {
        // Get all rows in the GridView
        var allRows = document.querySelectorAll('#<%= GridView1.ClientID %> tr');

        // Loop through all rows and re-enable them
        allRows.forEach(function (row) {
            row.classList.remove('disabled-row');
            row.style.pointerEvents = "";  // Re-enable mouse events
            row.style.opacity = "1";  // Restore original appearance
        });
    }
</script>

<style>
    .disabled-row {
        pointer-events: none;
        opacity: 0.5;
    }
</style>
