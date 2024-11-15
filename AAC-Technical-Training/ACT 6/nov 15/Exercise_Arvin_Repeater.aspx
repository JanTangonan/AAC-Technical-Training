<%@ Page Title="" Language="C#" MasterPageFile="~/LIMSHOME.master" AutoEventWireup="true" CodeBehind="Exercise_Arvin_Repeater.aspx.cs" Inherits="BEASTiLIMS.Exercise_Arvin_Repeater" %>
<%@ Register Assembly="PLCCONTROLS" Namespace="PLCCONTROLS" TagPrefix="PLC" %>
<%@ Register Src="~/PLCWebCommon/PLCDialog.ascx" TagName="Dialog" TagPrefix="PLC1" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .form-row {
            display: flex;
            align-items: center;
            margin-bottom: 3px; /* Space between each row */
        }

        .form-label {
            width: 20%; /* Adjust this based on your form width */
            text-align: left;
        }

        .form-control {
            flex: 1; /* Allow the control to take remaining space */
            max-width: 70%; /* Prevents the control from stretching too wide */
        }
        
        .required-marker {
            color: red;
            margin-left: -10px; /* Space between the asterisk and control */
            margin-right: 5px;
        }

        .error-message {
            color: red;
            font-size: 0.9em;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cp" runat="server">
    <asp:UpdatePanel ID="UpdatePanel" runat="server">
            <ContentTemplate>
                <PLC1:Dialog ID="dlgMessage" runat="server"/>
                <PLC:PLCDBGrid ID="dbgLabCase" runat="server" AutoGenerateColumns="false" CssClass="grid" DataKeyNames="CASE_KEY" AutoPostBack="true"
                    BorderStyle="None" BorderWidth="0px" BorderColor="Transparent" SelectedRowStyle-BackColor="#7FFFD4"
                    OnSelectedIndexChanged="dbgLabCase_SelectedIndexChanged" PLCGridName="TEST_NICE_NOICE" PLCGridWidth="100%">
                </PLC:PLCDBGrid>
                
                <asp:Repeater ID="repLabCaseControl" runat="server">
                    <ItemTemplate>
                            <div class="item-container">
                                <div class="form-row">
                                    <asp:HiddenField ID="hfFieldName" runat="server" Value='<%# Eval("FIELD_NAME") %>' />
                                    <asp:HiddenField ID="hfMandatory" runat="server" Value='<%# Eval("MANDATORY") %>' />

                                    <label ID="lblInput" class="form-label"> <%# Eval("PROMPT") %>:</label>
                                    <asp:Label ID="lblMandatory" runat="server" Text="*" CssClass="required-marker" Visible='<%# Eval("MANDATORY") != null && Eval("MANDATORY").ToString() == "T" %>'></asp:Label>
                                    <asp:TextBox ID="txtInput" runat="server" Visible='<%# Eval("CODE_TABLE") == DBNull.Value %>' Width="350" 
                                        CssClass='<%# (Eval("FIELD_NAME").ToString() == "OFFENSE_DATE" ? "form-control datepicker" : "form-control") %>'></asp:TextBox>
                                    <PLC:FlexBox ID="flexBoxInput" runat="server" CssClass="form-control" Visible='<%# Eval("CODE_TABLE") != DBNull.Value %>' TableName='<%# Eval("CODE_TABLE") %>' Width="600"></PLC:FlexBox>
                                </div>
                            </div>
                    </ItemTemplate>
                </asp:Repeater>

                <PLC:PLCButtonPanel ID="bpLabCase" runat="server" OnPLCButtonClick="bpLabCase_PLCButtonClick"
                    PLCShowEditButtons="True">
                </PLC:PLCButtonPanel>
            </ContentTemplate>
    </asp:UpdatePanel>

    <script type="text/javascript">
        $(document).ready(function () {
            // Initialize the datepicker on elements with the 'datepicker' class
            $(document).on("focus", ".datepicker", function () {
                $(this).on('keydown paste', function(e) {
                    e.preventDefault();
                });


                $(this).datepicker({
                    dateFormat: 'yy-mm-dd',
                    maxDate: new Date()
                });
            });
        });
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="foot" runat="server">
</asp:Content>
