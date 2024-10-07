<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="WebApplication1.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <!-- GridView to display department names -->
            <asp:GridView ID="gvDepartments" runat="server" AutoGenerateColumns="False"
                OnSelectedIndexChanged="gvDepartments_SelectedIndexChanged">
                <Columns>
                    <asp:BoundField DataField="DepartmentID" HeaderText="Department ID" />
                    <asp:BoundField DataField="DepartmentName" HeaderText="Department Name" />
                    <asp:CommandField ShowSelectButton="True" />
                </Columns>
            </asp:GridView>

            <!-- Dropdown List for Department Names -->
            <asp:DropDownList ID="ddlDepartments" runat="server"></asp:DropDownList>
        </div>
    </form>
</body>
</html>
