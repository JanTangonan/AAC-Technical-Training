<%@ Page Title="" Language="C#" MasterPageFile="~/LIMSHOME.master" AutoEventWireup="true" CodeBehind="Exercise_Arvinn.aspx.cs" Inherits="BEASTiLIMS.Exercise_Arvinn" %>
<%@ Register Assembly="PLCCONTROLS" Namespace="PLCCONTROLS" TagPrefix="PLC" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .disabled {
            pointer-events: none;
            opacity: 0.5;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cp" runat="server">
    <asp:UpdatePanel ID="UpdatePanel" runat="server">
            <ContentTemplate>
                <PLC:PLCDBGrid ID="dbgLabCase" runat="server" AutoGenerateColumns="false" CssClass="grid" DataKeyNames="CASE_KEY" AutoPostBack="true"
                    BorderStyle="None" BorderWidth="0px" BorderColor="Transparent" SelectedRowStyle-BackColor="#7FFFD4" Width="800"
                    OnSelectedIndexChanged="dbgLabCase_SelectedIndexChanged">
                </PLC:PLCDBGrid>

                <fieldset>
                    <legend>Case Report</legend>
                    <PLC:DBPanel ID="dbpLabCase" PLCPanelName="CASETAB" runat="server">
                    </PLC:DBPanel>
                </fieldset>

                <PLC:PLCButtonPanel ID="bpLabCase" runat="server" OnPLCButtonClick="bpLabCase_PLCButtonClick"
                    PLCShowEditButtons="True" PLCTargetControlID="dbpLabCase" >
                </PLC:PLCButtonPanel>
            </ContentTemplate>
        </asp:UpdatePanel>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="foot" runat="server">
</asp:Content>
