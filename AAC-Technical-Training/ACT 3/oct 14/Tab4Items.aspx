<%@ Page Language="C#" MasterPageFile="~/CASEFILE.master" AutoEventWireup="True" CodeBehind="TAB4Items.aspx.cs" Inherits="BEASTiLIMS.TAB4Items" Title="Item Information" %>

<%@ Register Assembly="PLCCONTROLS" Namespace="PLCCONTROLS" TagPrefix="PLC" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<%@ Register Src="UC_Attributes.ascx" TagName="UC_Attributes" TagPrefix="uc1" %>
<%@ Register Src="PLCCurrency.ascx" TagName="Currency" TagPrefix="uc1" %>
<%@ Register Src="PLCItemSearch.ascx" TagName="ItemSearch" TagPrefix="uc1" %>
<%@ Register Src="~/PLCWebCommon/PLCWebControl.ascx" TagName="PLCWebControl" TagPrefix="uc1" %>
<%@ Register Src="~/PLCWebCommon/PLCPageHead.ascx" TagName="PLCPageHead" TagPrefix="uc1" %>
<%@ Register Src="~/PLCWebCommon/PLCItemTask.ascx" TagName="PLCItemTask" TagPrefix="uc1" %>
<%@ Register Src="~/PLCWebCommon/PLCKitTask.ascx" TagName="PLCKitTask" TagPrefix="uc1" %>
<%@ Register Src="~/PLCWebCommon/PLCDialog.ascx" TagName="Dialog" TagPrefix="PLC" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .cryoboxicon {float: right;}
        .cryoboxicon img {border: none;}
        .workproducticon {float: right;}
        .workproducticon img {border: none;}
        .item-sidetext {float: left; margin-bottom:-10px; width: 100px!important;}
        
        .fixedgridheader {
            position: relative;
            z-index: 10;
        }
        
        .modalPopup {
            z-index: 1001 !important;
        }
        .modalBackground, .msgboxmodalBackground
        {
            z-index: 1000 !important;
        }
        .dispo-review-items-title {
            margin: 5px 0 0;
            padding: 10px;
            font-size: 1.2em;
            background-color: #cfdbec;
        }
        .dispo-review-items {
            width: 100%;
            padding: 5px;
        }
        .dispo-review-items th {
            text-align: left;
        }
        .dispo-review-items-summary-overdue,
        .dispo-review-items-dnaclearance {
            color: #ff0000 !important;
        }
        .dispo-review-comments {
            margin: 5px 0;
            padding: 5px;
            width: 100%;
        }
        .dispo-review-comments-header {
        }
        .dispo-review-comments-checkbox, .dispo-review-comments-title {
            padding: 5px;
        }
        .dispo-review-comments-title {
            font-weight: bold;
            float: left;
        }
        .dispo-review-comments-checkbox {
            float: right;
        }
        .dispo-review-comment-title {
            display: inline-block;
            padding: 5px;
        }
        .dispo-review-comment-body {
            width: 100%;
            margin: 2px 5px 5px 5px;
        }
        .dispo-review-dna-clearance {
            overflow-y: auto;
            width: 100%;
            height: 350px;
        }
        .cleardiv {
            clear: both;
        }

        .custodycontainer .statuscaption {
            margin: 0 0.3em 0 0;
        }
        
        .custodycontainer span {
            margin: 0 1em 0 0;
        }
        
        .bulkdeleteitem
        {
            max-height:300px;
            overflow-y: auto;
            display:block;    
        }
        
        .tab_container {
            height: 250px;
            overflow-y: auto;
        }
    </style>

    <script type="text/javascript" language="javascript">
        function onModalPopupOK() {
            document.getElementById("<%= btnKit.ClientID %>").click();
        }
        function onModalPopupCancel() {
            document.getElementById("<%= btnKitC.ClientID %>").click();
        }
        function onOKDeleteBulk() {
            document.getElementById("<%= btnDeleteBulk.ClientID %>").click();
        }
        function onCancelDeleteBulk() {
            document.getElementById("<%= btnCancelDeleteBulk.ClientID %>").click();
        }
        function chkBoxListToggle(cbList, checkItems) {
            $("#" + cbList.id + " :checkbox").attr('checked', checkItems);
        }

        function SelectAllContainers(chk) {
            $('#<%= gvMultiPrintLabel.ClientID %>').find("input:checkbox").each(function() {
                if (this != chk) {
                    // Skip checkbox if printing for specific item barcode is disabled (for read only access)
                    if (!$(this).attr('disabled'))
                        this.checked = chk.checked;
                }
                
                if ('<%# PLCSessionVars1.CheckUserOption("WEBINQ") || PLCSessionVars1.CheckUserOption("RONLYITTAB") %>' == 'True') // Disable Print button for read only access
                    $('#<%= btnPrintMultiLabel.ClientID %>').attr('disabled', !chk.checked);
                else if ('<%# PLCSessionVars1.CheckUserOption("ITEMLBLOFF") %>' == 'False') // Disable Print button if user has no access
                    $('#<%= btnPrintMultiLabel.ClientID %>').attr('disabled', !chk.checked);
            });            
        }

        function SelectOneContainer(chk) {
            var AllItemsSelected = true;
            var AtLeastOneSelected = false;
            if (chk.checked) {
                $('#<%= gvMultiPrintLabel.ClientID %>').find("input:checkbox").each(function() {
                    if (this.id.indexOf("chkAll") == -1) {
                        if (!this.checked && !$(this).attr('disabled')) {
                            AllItemsSelected = false;
                            return false;
                        }
                    }
                });

                AtLeastOneSelected = true;
            }
            else {

                AllItemsSelected = false;

                $('#<%= gvMultiPrintLabel.ClientID %>').find("input:checkbox").each(function() {
                    if (this.id.indexOf("chkAll") == -1) {
                        if (this.checked) {
                            AtLeastOneSelected = true;
                            return false;
                        }
                    }
                });
            }

            $('#<%= gvMultiPrintLabel.ClientID %>').find("input:checkbox").each(function() {
                if (this.id.indexOf("chkAll") > -1) {
                    this.checked = AllItemsSelected;
                    return false;
                }
            });

            if ('<%# PLCSessionVars1.CheckUserOption("WEBINQ") || PLCSessionVars1.CheckUserOption("RONLYITTAB") %>' == 'True') // Disable Print button for read only access
                $('#<%= btnPrintMultiLabel.ClientID %>').attr('disabled', !AtLeastOneSelected);
            else if ('<%# PLCSessionVars1.CheckUserOption("ITEMLBLOFF") %>' == 'False') // Disable Print button if user has no access
                $('#<%= btnPrintMultiLabel.ClientID %>').attr('disabled', !AtLeastOneSelected);
        }

        function SelectAllDelItem(chk) {
            $('#<%= gvBulkDelItems.ClientID %>').find("input:checkbox").each(function () {
                if (this != chk) {
                    this.checked = chk.checked;
                }

                if (chk.checked)
                    $(".ui-dialog-buttonpane button:contains('Delete')").button("enable");
                else
                    $(".ui-dialog-buttonpane button:contains('Delete')").button("disable");
            });
        }

        function SelectOneDelItem(chk) {
            var AllItemsSelected = true;
            var AtLeastOneSelected = false;

            if (chk.checked) {
                $('#<%= gvBulkDelItems.ClientID %>').find("input:checkbox").each(function () {
                    if (this.id.indexOf("cbSelect_All") == -1) {
                        if (!this.checked) {
                            AllItemsSelected = false;
                            return false;
                        }
                    }
                });

                AtLeastOneSelected = true;
            }
            else {

                AllItemsSelected = false;

                $('#<%= gvBulkDelItems.ClientID %>').find("input:checkbox").each(function () {
                    if (this.id.indexOf("cbSelect_All") == -1) {
                        if (this.checked) {
                            AtLeastOneSelected = true;
                            return false;
                        }
                    }
                });
            }

            $('#<%= gvBulkDelItems.ClientID %>').find("input:checkbox").each(function () {
                if (this.id.indexOf("cbSelect_All") > -1) {
                    this.checked = AllItemsSelected;
                    return false;
                }
            });

            if (AtLeastOneSelected)
                $(".ui-dialog-buttonpane button:contains('Delete')").button("enable");
            else
                $(".ui-dialog-buttonpane button:contains('Delete')").button("disable");
        }

        function onOKPrintMultiLabelBC() {
            var btn = document.getElementById("<%= btnPrintMultiLabel.ClientID %>");
            btn.disabled = true;
            btn.value = "Wait...";

            document.getElementById("<%= btnPrintMultiLabelBC.ClientID %>").click();
        }

        function onCancelbtnPrintMultiLabelBC() {
            var btn = document.getElementById("<%= btnPrintMultiLabelClose.ClientID %>");
            btn.disabled = true;
            btn.value = "Wait...";

            document.getElementById("<%= btnCancelPrintMultiLabelBC.ClientID %>").click();
        }

        function toggleView(viewId) {
            switch(viewId)
            {
                case 0: //currency
                    if ($('#<%= bnCurrency.ClientID %>').is(":disabled") === false) {
                        $('#<%= bnAttribute.ClientID %>').removeClass('active');
                        
                        $('#<%= tbNames.ClientID %>').removeClass('active');
                        $('#<%= tbAttribute.ClientID %>').removeClass('active');
                        $('#<%= tbCurrency.ClientID %>').addClass('active');

                        $('#<%= vwAttribute.ClientID %>').hide();
                        $('#<%= vwNames.ClientID %>').hide();

                        $('#<%= vwCurrency.ClientID %>').show();
                    }
                break;
                case 1: //attributes
                    if ($('#<%= bnAttribute.ClientID %>').is(":disabled") === false) {
                        $('#<%= bnCurrency.ClientID %>').removeClass('active');
                        
                        $('#<%= tbNames.ClientID %>').removeClass('active');
                        $('#<%= tbCurrency.ClientID %>').removeClass('active');
                        $('#<%= tbAttribute.ClientID %>').addClass('active');

                        $('#<%= vwCurrency.ClientID %>').hide();
                        $('#<%= vwNames.ClientID %>').hide();

                        $('#<%= vwAttribute.ClientID %>').show();
                    }
                break;
                case 2: //names
                    if ($('#<%= bnNames.ClientID %>').is(":disabled") === false) {
                        $('#<%= tbAttribute.ClientID %>').removeClass('active');
                        $('#<%= tbCurrency.ClientID %>').removeClass('active');
                    
                        $('#<%= tbNames.ClientID %>').addClass('active');
                        
                        $('#<%= vwAttribute.ClientID %>').hide();
                        $('#<%= vwCurrency.ClientID %>').hide();
                    
                        $('#<%= vwNames.ClientID %>').show();
                    }
                break;
                
            }
        }

        function updateItemsGridIcons(sequence) {
            var casekey = $("[id$='hdnCaseKey']").val();
            var params = "";

            if (sequence == 0) {
                if ($("[id$='hdnUsesCryobox']").val() == "T") {
                    params = { "casekey": casekey, "worksheetcode": "RETVOL" };
                    $.ajax({
                        type: "POST",
                        url: "PLCWebCommon/PLCWebMethods.asmx/RetrieveECNsWithWorksheets",
                        data: params,
                        success: function (retdata) {
                            $(".cryoboxicon").remove();

                            var ecns = retdata.ecns;
                            for (var i = 0; i < ecns.length; i++) {
                                addCryoboxIconToGridRow(ecns[i]);
                            }
                            updateItemsGridIcons(1);
                        },
                        error: function (xhr, errortxt, e) {
                            ShowError("Error retrieving Cryobox information.", xhr, errortxt, e);
                        },
                        dataType: "json"
                    });
                }
                else
                    updateItemsGridIcons(1);
            }

            if (sequence == 1) {
                if ($("[id$='hdnUsesProduct']").val() == "T") {
                    params = { "casekey": casekey };
                    $.ajax({
                        type: "POST",
                        url: "PLCWebCommon/PLCWebMethods.asmx/RetrieveECNsWithWorkProducts",
                        data: params,
                        success: function (retdata) {
                            $(".workproducticon").remove();

                            var ecns = retdata.ecns;
                            for (var i = 0; i < ecns.length; i++) {
                                addWorkProductIconToGridRow(ecns[i]);
                            }
                        },
                        error: function (xhr, errortxt, e) {
                            ShowError("Error retrieving Work Product information.", xhr, errortxt, e);
                        },
                        dataType: "json"
                    });
                }
            }
        }

        function addCryoboxIconToGridRow(ecn) {
            if (ecn == null || ecn == "")
                return;

            // Add Cryobox icon to the grid row that contains the ecn.
            var $tr = $(".dgcell-ECN[value='" + ecn + "']").parent();
            if ($tr.length > 0) {
                var $first_td = $tr.children().eq(2);
                $first_td.append("<a href='#' class='cryoboxicon'><img src='Images/Tube.png' height='15' width='15' ></a>");
                $first_td.children().eq(0).addClass("item-sidetext");

                $first_td.find(".cryoboxicon").click(function() {
                    $("[id$='hdnRVWorksheetsECN']").val(ecn);
                    $("[id$='bnWorksheets']").click();
                    return false;
                });
            }
        }

        function addWorkProductIconToGridRow(ecn) {
            if (ecn == null || ecn == "")
                return;

            // Add icon to the grid row that contains the ecn.
            var $tr = $(".dgcell-ECN[value='" + ecn + "']").parent();
            if ($tr.length > 0) {
                var $first_td = $tr.children().eq(2);
                $first_td.append("<a href='#' class='workproducticon'><img src='Images/dna-tube.png' height='15' width='15' ></a>");
                $first_td.children().eq(0).addClass("item-sidetext");

                $first_td.find(".workproducticon").click(function () {
                    $("[id$='hdnRVWorksheetsECN']").val(ecn);
                    $("[id$='bnWorkProduct']").click();
                    return false;
                });
            }
        }
        
        function showDNAForm() {
            var myWindow = window.open("pdffiles\\DNA Clearance Form.pdf","DNAClearanceForm","titlebar=yes,toolbar=yes,location=no,resizable");
            myWindow.document.title = "DNA Clearance Form";
        }

        function BDialogOpen(open) {
            $("[id='idialog-bulkdelitems']:eq(1)").dialog("destroy").remove();
            $("#idialog-bulkdelitems").dialog({
                autoOpen: false,
                modal: true,
                resizable: false,
                draggable: false,
                title: 'Delete Item(s)',
                width: 500,
                buttons: {
                    Delete: function () {
                        $(this).dialog("close");
                        document.getElementById("<%= btnBulkDelete.ClientID %>").click();
                    },
                    Cancel: function () {
                        $(this).dialog("close");
                        document.getElementById("<%= btnCloseDeleteBulk.ClientID %>").click();
                    }
                },
                open: function () {
                    $(this).parent().appendTo("form");

                    var AtLeastOneSelected = false;
                    $('#<%= gvBulkDelItems.ClientID %>').find("input:checkbox").each(function () {
                        if (this.id.indexOf("chkAll") == -1) {
                            if (this.checked) {
                                AtLeastOneSelected = true;
                                return false;
                            }
                        }
                    });

                    if (AtLeastOneSelected)
                        $(".ui-dialog-buttonpane button:contains('Delete')").button("enable");
                    else
                        $(".ui-dialog-buttonpane button:contains('Delete')").button("disable");
                }
            });

            if (open)
                $("#idialog-bulkdelitems").dialog("open");
            else
                $("#idialog-bulkdelitems").dialog("close");
        }

        function SetBulkDeleteKeys() {
            AddWindowLoadEvent(function () {
                var canBulkDelete = $("[id$='hdnBulkDelete']").val();
                $(document).keydown(function (e) {
                    if (e.shiftKey && e.keyCode == 46 && canBulkDelete == "T") {
                        document.getElementById("<%= btnShowBulkPopup.ClientID %>").click();
                    }
                });
            });
        }

        function ShowDialogMsg(message) {
            $("#idialog-message").html(message);
            $("#idialog-alert").dialog({
                modal: true,
                resizable: false,
                draggable: false,
                closeOnEscape: false,
                dialogClass: "no-close",
                title: 'Item records about to be deleted',
                width: 400,
                height: 'auto',
                buttons: {
                    OK: function () {
                        $(this).dialog("close");
                        onOKDeleteBulk();
                    },
                    Cancel: function () {
                        $(this).dialog("close");
                        onCancelDeleteBulk();
                    }
                },
                open: function () {
                    $(this).parent().appendTo("form");
                },
                close: function () {
                    $(this).dialog("destroy");
                }

            });
            $("#idialog-alert").dialog("open");
        }
        
        var _divScrollPos = 0;
        function contentPageLoad() {
            // Disable Bulk Delete shortcut for read only access
            if ('<%# PLCSessionVars1.CheckUserOption("WEBINQ") || PLCSessionVars1.CheckUserOption("RONLYITTAB") %>' != 'True')
                SetBulkDeleteKeys();
            var $obj1 = $('#divStatChange');
            $obj1.scrollTop(_divScrollPos);
            $obj1.scroll(function () {
                _divScrollPos = $obj1.scrollTop();
                $obj1.find('.fixedgridheader').css('top', _divScrollPos);
            });

            // Bulk Delete Items DBGrid 
            var $grd_parent = $('#<%= gvBulkDelItems.ClientID %>');
            $grd_parent.find("input[type=checkbox][id$='cbSelect']").each(function () {
                var $cbxDel = $(this);
                $cbxDel.closest('div.clipcell').css('width', '25px').css('margin', '0px');
                $cbxDel.closest('td').css('width', '25px');
                $cbxDel.attr('onclick', null)
                .unbind('click')
                .click(function () {
                    SelectOneDelItem(this);
                });
            });

            var $cbx_alldel = $('#<%= gvBulkDelItems.ClientID %>_header').find("input[type=checkbox][id$='cbSelect_All']");
            $cbx_alldel.closest('th').css('width', '25px');
            $cbx_alldel.attr('onclick', null)
                .unbind('click')
                .click(function () {
                    SelectAllDelItem(this);
                });

            /* Need to set also the supplement popup if page is empty mode */
            if (typeof supplementemptymode != 'undefined') {
                var $grd = $('[id$="GridView1"]');
                if ($grd.length == 0)
                    supplementemptymode = "T";
                else if ($('[id$="lPLCLockStatus"]').length > 0)
                    supplementemptymode = "T";
                else
                    supplementemptymode = "";
            }
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cp" runat="server">    
    <asp:RadioButtonList ID="rblItemType" runat="server" RepeatDirection="Horizontal" OnSelectedIndexChanged="rblItemType_SelectedIndexChanged" 
        AutoPostBack="True" Width="426px" Visible="false">
        <asp:ListItem Value="0">Items</asp:ListItem>
        <asp:ListItem Value="1">File Items</asp:ListItem>
        <asp:ListItem Selected="True" Value="2">All Items</asp:ListItem>
    </asp:RadioButtonList>
    <PLC:PLCDataSource ID="dsItemGrid" runat="server">
    </PLC:PLCDataSource>
    <asp:UpdatePanel runat="server">
        <ContentTemplate>
            <PLC:Dialog ID="dlgMessage" runat="server" />
        </ContentTemplate>
    </asp:UpdatePanel>
    <PLC:PLCSessionVars ID="PLCSessionVars1" runat="server" />
    <asp:PlaceHolder ID="phMsgBox" runat="server"></asp:PlaceHolder>
    <asp:PlaceHolder ID="phMsgBoxComments" runat="server"></asp:PlaceHolder>
    <asp:HiddenField ID="hdnDBPanelFields" runat="server" Value=""/>

    <uc1:PLCWebControl ID="WebOCX" runat="server" />

    <div class="dbbox">
        <div class="dbgridblk">
            <PLC:PLCDBGrid ID="GridView1" runat="server" DataKeyNames="EVIDENCE_CONTROL_NUMBER"
                OnRowCreated="GridView1_RowCreated" OnSelectedIndexChanged="GridView1_SelectedIndexChanged" OnRowDataBound="GridView1_RowDataBound"
                AllowPaging="False" AllowSorting="True" OnRowCommand="GridView1_RowCommand" OnSorted="GridView1_Sorted" PLCGridWidth="100%" Height="200" >
            </PLC:PLCDBGrid>
        </div>
        <div class="dbgridbtnblk">        
            <asp:Button ID="bnDupe" runat="server" Text="Dupe" style="" Width="100%" OnClick="bnDupe_Click" />
            <asp:Button ID="bnSample" runat="server" Text="Sample" Width="100%" OnClick="bnSample_Click" />
            <asp:Button ID="bnKit" runat="server" Text="Kit" Width="100%" OnClick="bnKit_Click" />
            <asp:Button ID="bnContainer" runat="server" Text="Container" Width="100%" OnClick="bnContainer_Click" />
            <asp:Button ID="bnItemList" runat="server" Text="Item List" Width="100%" OnClick="bnItemList_Click" />
            <asp:Button ID="btnSupp49" runat="server" Text="Evidence Page" Width="100%" Visible="false" OnClick="btnSupp49_Click"/> 
            <asp:Button ID="bnTransfer" runat="server" Text="Transfer" Width="100%" OnClick="bnTransfer_Click" />
            <asp:Button ID="Delivery" runat="server" Text="Delivery" Width="100%" OnClick="bnDelivery_Click" Visible="false" />
            <asp:Button ID="btnMultiLabel" runat="server" Text="Label" CommandArgument="PrintMultiLabel" Width="100%" OnClick="bnLabel_Click" Visible="false"/>
            <asp:Button ID="btnStatusChange" runat="server" Text="Status Change" Width="100%" OnClick="btnStatusChange_Click" />
            <asp:Button ID="btnTrackRFID" runat="server" Text="Track RFID" Width="100%" OnClick="btnTrackRFID_Click" />
        </div>
        <div class="dbbtnpanel custodycontainer" style="clear: both;">
            <asp:Button ID="bnItemFind" runat="server" Text="Find" Width="60px" OnClick="bnItemFind_Click" />
            <asp:Label runat="server" ID="lblCustodyLocationCaption" CssClass="statuscaption"></asp:Label>
            <asp:Label ID="lCurrCustodyLocation" runat="server" ForeColor="Blue" />
            <asp:Label runat="server" ID="lblTrackingNumberCaption" CssClass="statuscaption"></asp:Label>
            <asp:Label runat="server" ID="lblTrackingNumber" ForeColor="Blue"></asp:Label>
        </div>
    </div>
    <div class="itemstab">
        <div class="dbbox">
            <div class="dbpanelblk flexboxscroll">
                <table width="100%">
                    <tr id="trPLCLockStatus" runat="server" visible="false" >
                        <td>
                            <asp:Label ID="lPLCLockStatus" runat="server" ForeColor="Red"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td valign="top">
                            <div style="padding-right: 5px;">
                                <div>
                                    <PLC:DBPanel ID="PLCDBPanel1" runat="server" HorizontalAlign="Left" onplcdbpanelbuttonclick="PLCDBPanel1_PLCDBPanelButtonClick"
                                        OnPLCDBPanelSetDefaultRecord="PLCDBPanel1_SetDefaultRecord"
                                        OnPLCDBPanelGetNewRecord="PLCDBPanel1_PLCDBPanelGetNewRecord" PLCDataTable="TV_LABITEM"
                                        PLCPanelName="ITEMSTAB" PLCShowAddButton="True" PLCShowDeleteButton="True" PLCShowEditButtons="True"
                                        PLCWhereClause="Where 0 = 1" OnPLCDBPanelValidate="PLCDBPanel1_PLCDBPanelValidate"
                                        OnPLCDBPanelCodeHeadChanged="PLCDBPanel1_PLCDBPanelCodeHeadChanged" OnPLCDBPanelTextChanged="PLCDBPanel1_TextChanged" PLCDisableDefaultValue="True"
                                        PLCAuditCode="7" PLCAuditSubCode="1" PLCDeleteAuditCode="11" PLCDeleteAuditSubCode="1" PLCAttachPopupTo="body" >
                                    </PLC:DBPanel>
                                </div>
                            </div>
                        </td>
                        <td valign="top">
                            <div style="min-width: 400px;">
                                <ul class="tabs">                                 
                                    <li id="tbCurrency" runat="server">
                                        <a href="javascript:toggleView(0);" id="bnCurrency" runat="server">Currency</a>
                                    </li>
                                     <li id="tbAttribute" runat="server" class="active">
                                        <a href="javascript:toggleView(1);" id="bnAttribute" style="" runat="server">Attribute</a>
                                    </li>
                                    <li id="tbNames" runat="server">
                                        <a href="javascript:toggleView(2);" id="bnNames" runat="server">Names</a>
                                    </li>
                                </ul>
                                <div class="tab_container flexboxscroll">
                                    <div>
                                        <div id="vwAttribute" runat="server" style="display: none;">
                                            <uc1:UC_Attributes ID="ItemAttribute" runat="server" OnExhibitNumberChanged="ItemAttribute_ExhibitNumberChanged" />
                                        </div>
                                        <div id="vwCurrency" runat="server" style="display: none;">
                                            <uc1:Currency ID="Currency1" runat="server" OnValidated="Currency1_Validated" />
                                        </div>
                                        <div id="vwNames" runat="server" style="display: none;">
                                            <table>
                                                <tr>
                                                    <td>
                                                        <asp:GridView ID="gvNameItem" runat="server" OnRowCreated="gvNameItem_RowCreated"
                                                            OnRowDataBound="gvNameItem_RowDataBound" EmptyDataText="No names found in this case."
                                                            EmptyDataRowStyle-ForeColor="Red" >
                                                        </asp:GridView>
                                                    </td>
                                                </tr>
                                            </table>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </td>
                    </tr>
                </table>
            </div>
            <div class="dbbtnpanel">
                <div style="float: left;">
                    <PLC:PLCButtonPanel ID="PLCButtonPanel1" runat="server" PLCShowAddButton="True" PLCShowDeleteButton="True"
                        PLCShowEditButtons="True" PLCTargetControlID="PLCDBPanel1" OnPLCButtonClick="PLCButtonPanel1_PLCButtonClick"
                        PLCCustomButtons="RecordUnlock,Bulk Delete" PLCTargetDBGridID="GridView1">
                    </PLC:PLCButtonPanel>
                    <div style="padding: 3px 0 0; float: left;">
                        <asp:Button ID="bnLabel" runat="server" Text="Label" Width="90px" OnClick="bnLabel_Click" />
                        <asp:Button ID="bnNoLabel" runat="server" Text="No Label" Width="90px" OnClick="bnNoLabel_Click" />
                    </div>
                </div>
            </div>
        </div>
    </div>

    <uc1:ItemSearch ID="ItemSearch1" runat="server" />
    <asp:TextBox ID="txtItemFindECN" runat="server" Style="display: none;"></asp:TextBox>
    <asp:Button ID="bnItemFindPostback" runat="server" OnClick="bnItemFindPostback_Click" Style="display: none;" />
    <asp:Button ID="MsgCommentPostBackButton" runat="server" Text="Button" style="display: none;" OnClick="MsgCommentPostBackButton_Click" />
    <asp:TextBox ID="UserComments" runat="server" TextMode="MultiLine" style="display: none;"></asp:TextBox>
    <asp:Button ID="btnKit" runat="server" Style="display: none;" OnClick="btnKitSave_Click" />
    <asp:Button ID="btnKitC" runat="server" Style="display: none;" OnClick="btnKitCancel_Click" />

    <asp:Panel ID="pnlKit" runat="server" CssClass="modalPopup" Style="display: none;" Width="500px">
        <div class="header caption">
            Add Items From Kit
        </div>
        <div class="content">
            <asp:UpdatePanel ID="upKit" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:CheckBoxList ID="chkListKit" runat="server" Width="100%" RepeatLayout="Flow" RepeatDirection="Vertical" />
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <div class="footer" align="center" >
            <!-- <input type="button" id="btnKitSelectAll" value="Select All" style="width: 90px;" onclick="chkBoxListToggle(<%= chkListKit.ClientID %>, true)" />
            <input type="button" id="btnKitSelectNone" value="Select None" style="width: 90px;" onclick="chkBoxListToggle(<%= chkListKit.ClientID %>, false)" /> -->
            <asp:CheckBox ID="chkKitPrintLabels" runat="server" Text="Print Label" />
            <span style="width: 25px;">&nbsp;</span>
            <asp:Button ID="btnKitSave" runat="server" Text="OK" Width="80px" />
            <asp:Button ID="btnKitCancel" runat="server" Text="Cancel" Width="80px" />
        </div>
        <ajaxToolkit:ModalPopupExtender ID="mpeKit" runat="server" BackgroundCssClass="modalBackground"
            DropShadow="false" PopupControlID="pnlKit" PopupDragHandleControlID="pnlHeadKit"
            TargetControlID="btnKitSave" Drag="true" OkControlID="btnKitSave" CancelControlID="btnKitCancel"
            OnOkScript="onModalPopupOK()" OnCancelScript = "onModalPopupCancel()">
        </ajaxToolkit:ModalPopupExtender>
    </asp:Panel>

    <PLC:MessageBox ID="mbConfirm" runat="server" MessageType="Confirmation" PanelCSSClass="modalPopup"
        CaptionCSSClass="caption" PanelBackgroundCSSClass="modalBackground" Caption="Confirm"
        Message="Changing the item type will delete saved information. Do you want to proceed?"
        OnOkClick="mbConfirm_OkClick" OnCancelClick="mbConfirm_CancelClick" />

    <PLC:MessageBox ID="mbConfirmDeleteAttr" runat="server" MessageType="Confirmation" PanelCSSClass="modalPopup"
        CaptionCSSClass="caption" PanelBackgroundCSSClass="modalBackground" Caption="Confirm"
        Message="Delete other exhibits?"
        OnOkClick="mbConfirmDeleteAttr_OkClick" OnCancelClick="mbConfirmDeleteAttr_CancelClick" />
            
    <asp:Button ID="btnHiddenDispo" runat="server" Text="HiddenDispo" style="display: none;" OnClick="doStatusChangeOK" />

    <PLC:MessageBox ID="mbSampleContainer" runat="server" MessageType="Confirmation" PanelCSSClass="modalPopup"
        CaptionCSSClass="caption" PanelBackgroundCSSClass="modalBackground" Caption="Confirm"
        Message="Do you want to separate this Sampled Item from it's Parent and place it in a new Container?"
        OnOkClick="mbSampleContainerConfirm_OkClick" OnCancelClick="mbSampleContainerConfirm_CancelClick" />
    
    <PLC:MessageBox ID="mbClear" runat="server" MessageType="Confirmation" PanelCSSClass="modalPopup"
        CaptionCSSClass="caption" PanelBackgroundCSSClass="modalBackground" Caption="Confirm"
        OnOkClick="mbClearConfirm_OkClick" OnCancelClick="mbClearConfirm_CancelClick" />

    <PLC:MessageBox ID="mbDeleteItem" runat="server" MessageType="Confirmation" PanelCSSClass="modalPopup"
        CaptionCSSClass="caption" PanelBackgroundCSSClass="modalBackground" Caption="Item Information is about to be deleted"
        OnOkClick="mbDeleteItemConfirm_OkClick" OnCancelClick="mbDeleteItemConfirm_CancelClick" 
        Width="500"/>
    
    <asp:Button runat="server" ID="btnShowPopupAgain" style="display:none"/>                   
    <asp:Button runat="server" ID="btnCreateSampleContainer" OnClick="btnCreateSampleContainer_Click" style="display:none"/>                   
    <asp:Panel ID="pnlCreateContainer" CssClass="modalPopup" runat="server" style="display:none">
        <asp:Panel ID="Panel5" runat="server">
            <asp:Panel runat="Server" ID="panel6">                                
                <table width="400px">
                    <tr>
                        <td colspan="2" align="center">
                            <asp:Panel ID="Panel7" runat="server" Height="20px" CssClass="caption">
                            Create Sample's New Container</asp:Panel>
                        </td>
                    </tr>                        
                    <tr>  
                        <td colspan="2" align="left">                                
                            <asp:Label ID="Label2" runat="server" Text="Enter Container Description"></asp:Label>
                            <br />
                            <asp:TextBox ID="tbxCreateContainerDescription" runat="server" Width="400px"></asp:TextBox>                                
                            <asp:Label ID="lblNewContainerValidation" runat="server" Text="" ForeColor="Red" Visible="false"></asp:Label>
                            <asp:PlaceHolder ID="phContainerSource" runat="server" Visible ="false">
                                <br />
                                <asp:Label ID="lblCreateSource" runat="server" Text="Source"></asp:Label>
                                <br />
                                <asp:TextBox ID="tbxCreateSource" runat="server" Width="400px"></asp:TextBox>
                                <br />
                            </asp:PlaceHolder>                                
                        </td>                    
                    </tr>
                    <tr>
                        <td style="width: 120px">
                            <asp:Label ID="lblPackagedIn" runat="server" Text="Packaged In"></asp:Label>
                        </td>
                        <td>
                            <PLC:FlexBox ID="fbPackType" runat="server" TableName="TV_PACKTYPE" Width="300" />
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <hr />
                        </td>
                    </tr>                   
                    <tr>
                        <td align="left">
                            <asp:CheckBox ID="chkPrinLabel" runat="server" Checked="false" Text="Print Label" TextAlign="Right"/>
                        </td>
                        <td align="right">                                 
                            <asp:Button ID="btnOKCreateContainer" runat="server" Text="OK" Width="100px"  /> 
                            <asp:Button ID="btnCancelCreateContainer" runat="server" Text="Cancel"  Width="100px" /> 
                        </td>
                    </tr>                                                                               
                </table>                                
                <br /> 
            </asp:Panel>
        </asp:Panel>                                 
    </asp:Panel>
    <ajaxToolkit:ModalPopupExtender ID="mpeCreateContainer" runat="server" 
        TargetControlID="btnShowPopupAgain"                        
        PopupControlID="pnlCreateContainer"           
        OkControlID="btnOKCreateContainer"
        CancelControlID="btnCancelCreateContainer"        
        BackgroundCssClass="modalBackground"></ajaxToolkit:ModalPopupExtender>

    <asp:Button ID="btnConfirmUpdate" runat="server" Text="Button" style="display: none;" OnClick="btnConfirmUpdate_Click" />
    <asp:Button ID="btnConfirmDelete" runat="server" Text="Button" style="display: none;" OnClick="btnConfirmDelete_Click" />
    <asp:Button ID="btnCustodyChangeYes" runat="server" OnClick="btnCustodyChangeYes_Click" style="display: none;" />
    <asp:TextBox ID="txtConfirmUpdate" runat="server" TextMode="MultiLine" style="display: none;"></asp:TextBox>
    <asp:HiddenField ID="hdnConfirmUpdate" runat="server" />

    <asp:Panel ID="pnlMultiPrint" runat="server" CssClass="modalPopup" style="display:none">
        <asp:Panel ID="pnlMultiPrintMain" runat="server" Width="400px">
            <asp:Panel ID="pnlMultiPrintCaption" runat="server" Height="20px" CssClass="caption">Print Label(s)</asp:Panel>
            <asp:Panel ID="pnlMultiLabelScroll" runat="server" ScrollBars="Vertical">
                <asp:GridView ID="gvMultiPrintLabel" runat="server" AutoGenerateColumns="false" DataKeyNames="ECN, LABELFORMAT, BARCODE" Width="100%" OnRowDataBound="gvMultiPrintLabel_RowDataBound" OnDataBinding="gvMultiPrintLabel_DataBinding">
                <Columns>
                    <asp:TemplateField>
                        <HeaderTemplate>
                            <asp:CheckBox ID="chkAll" runat="server" onclick="SelectAllContainers(this)"/>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <asp:CheckBox ID="chkItem" runat="server" onclick="SelectOneContainer(this)"/>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="ITEMNUM" HeaderText="Item #" />
                    <asp:BoundField DataField="CSITEMNUM" HeaderText="CS Item #" />
                    <asp:BoundField DataField="ITEMDESC" HeaderText="Description" ItemStyle-Width="200px"/>
                </Columns>
                </asp:GridView>
            </asp:Panel>
            <br />
            <asp:Label ID="lblNoticeMultiPrint"  runat="server" ForeColor="#FF3300" Text=""/>
            <table width="100%">
                <tr>                    
                    <td style="width:50%; padding-left:5px;">
                        <asp:DropDownList ID="ddlLabelFormat" runat="server" Width="180px" DataValueField="REPORT_NAME" DataTextField="REPORT_DESCRIPTION" />
                    </td>
                    <td style="text-align:right; padding-right:5px;">
                        <asp:Button ID="btnPrintMultiLabel" runat="server" Enabled="false" Text="Print" Width="90px"/>
                        <asp:Button ID="btnPrintMultiLabelClose" runat="server" Text="Close" Width="90px"/>
                    </td>
                </tr>
            </table>
            <br />            
        </asp:Panel>
    </asp:Panel>
    <asp:Button ID="btnPrintMultiLabelBC" runat="server" style="display:none" OnClick="btnPrintMultiLabel_Click" />
    <asp:Button ID="btnCancelPrintMultiLabelBC" runat="server" style="display:none" OnClick="btnPrintMultiLabelClose_Click" />
    <ajaxToolkit:ModalPopupExtender ID="mpeMultiLabelPrint" runat="server" BackgroundCssClass="modalBackground"
        PopupControlID="pnlMultiPrint" PopupDragHandleControlID="pnlMultiPrintCaption"
        TargetControlID="btnPrintMultiLabel" OnOkScript="onOKPrintMultiLabelBC();" OnCancelScript="onCancelbtnPrintMultiLabelBC();"
        OkControlID="btnPrintMultiLabel" CancelControlID="btnPrintMultiLabelClose">
    </ajaxToolkit:ModalPopupExtender>

    <asp:Panel ID="pnl49Supp" runat="server" CssClass="modalPopup" style="display:none">
        <asp:Panel ID="pnl49SuppMain" runat="server" Width="600px">
            <asp:Panel ID="pnl49SuppCaption" runat="server" Height="20px" CssClass="caption">Evidence Page</asp:Panel>
            <div class="dbbox" style="height:270px;">
                <div class="dbgridblk" style="width:575px; height:250px; overflow: scroll">
                    <div id="npdbg_49Supp"></div>
                </div>
            </div>  
            <asp:Panel ID="pnldbp49Supp" runat="server" Width="575px" style="padding: 10px">
                <PLC:PLCDBPanel ID="dbp49Supp" runat="server" PLCPanelName="EVIDENCE_PAGE"></PLC:PLCDBPanel><br />
                <asp:Button ID="btn49SuppSearch" runat="server" Enabled="true" Text="Search" Width="120" OnClientClick="SearchSupp49(); return false;" />
                <asp:Button ID="btn49SuppClearSearch" runat="server" Enabled="true" Text="Clear" Width="120" OnClientClick="ClearSupp49(); return false;" />
            </asp:Panel>
            <table align="center">
                <tr>
                    <td>
                        <asp:DropDownList ID="ddlEvidenceReport" runat="server" Width="180px" DataValueField="REPORT_NAME" DataTextField="REPORT_DESCRIPTION" onchange="ddlEvidenceReport_Change();" />
                    </td>
                    <td>
                        <asp:Button ID="btnPrintEvidenceRpt" runat="server" disabled="true" Text="Print Report" Width="120" OnClientClick="PrintSupp49Report(); return false;" />
                        <asp:Button ID="btn49SuppClose" runat="server" Text="Cancel" Width="120" OnClientClick="HideSupp49();"/>
                    </td>
                </tr>
            </table>
            <br />
        </asp:Panel>
    </asp:Panel>
    <ajaxToolkit:ModalPopupExtender ID="mpe49Supp" runat="server" BackgroundCssClass="modalBackground" PopupControlID="pnl49Supp" 
         PopupDragHandleControlID="pnl49SuppCaption" TargetControlID="btn49SuppDummy" CancelControlID="btn49SuppClose" BehaviorID="bhv49Supp" >
    </ajaxToolkit:ModalPopupExtender>
    <asp:Button ID="btnTriggerPrintEvidenceRpt" runat="server" OnClick="btnPrintEvidenceRpt_Click" style="display: none;"/>
    <asp:Button ID="btn49SuppDummy" runat="server" style="display: none;" />
    <asp:HiddenField ID="hdnECNList" runat="server" Value="" />
    <asp:HiddenField ID="hdnEvidenceReport" runat="server" Value="" />
    
    <asp:Panel ID="pnlRVWorksheets" CssClass="modalPopup" runat="server" style="display:none; overflow: auto;" Width="800px" >
        <asp:Panel ID="pnlRVWorksheetsMain" runat="server" Height="100%" >
            <asp:Panel ID="pnlRVWorksheetsCaption" runat="server" Height="20px" CssClass="caption"><asp:Literal ID="litRVWorksheetsCaption" runat="server"></asp:Literal></asp:Panel>
            <PLC:PLCDBGrid ID="dbgRVWorksheets" runat="server" AllowPaging="False" Height="300px" PLCGridWidth="100%" EmptyDataText="No Worksheets." >
            </PLC:PLCDBGrid>
            <div style="width: 100%; text-align: center;">
                <asp:Button ID="btnRVWorksheetsClose" runat="server" Text="Close" Width="150" OnClientClick="var mpe = $find('mpeRVWorksheets'); if (mpe) {mpe.hide(); return true;}" />
            </div>
        </asp:Panel>
    </asp:Panel>
    <ajaxToolkit:ModalPopupExtender ID="mpeRVWorksheets" runat="server" BackgroundCssClass="modalBackground"
         PopupControlID="pnlRVWorksheets" PopupDragHandleControlID="pnlRVWorksheetsCaption"
         TargetControlID="btnRVWorksheetsDummy" CancelControlID="btnRVWorksheetsClose">
    </ajaxToolkit:ModalPopupExtender>
    <asp:Button ID="btnRVWorksheetsDummy"  runat="server" Style="display: none;" />

    <asp:HiddenField ID="hdnCaseKey" runat="server" Value="" />
    <asp:HiddenField ID="hdnUsesCryobox" runat="server" Value="" />
    <asp:HiddenField ID="hdnUsesProduct" runat="server" Value="" />
    <asp:HiddenField ID="hdnRVWorksheetsECN" runat="server" Value="" />
    <asp:Button ID="bnWorksheets" runat="server" Text="Worksheets" Width="100%" OnClick="bnWorksheets_Click" style="display: none;" />

    <asp:Button ID="bnWPDummy" runat="server" Style='display: none;' />
    <asp:Button ID="bnWorkProduct" runat="server" Text="Work Product" OnClick="bnWorkProduct_Click" style="display: none;" />
    <ajaxToolkit:ModalPopupExtender ID="mpeWorkProduct" runat="server" BackgroundCssClass="modalBackground" PopupControlID="pnlWorkProduct" 
         PopupDragHandleControlID="pnlWPCaption" TargetControlID="bnWPDummy" CancelControlID="bnWPClose">
    </ajaxToolkit:ModalPopupExtender>
    <asp:Panel ID="pnlWorkProduct" runat="server" CssClass="modalPopup" style="display:none" Width="800px" Height="350px">
        <asp:Panel ID="pnlWPMain" runat="server" Height="100%">
            <asp:Panel ID="pnlWPCaption" runat="server" Height="20px" CssClass="caption" style="margin-bottom:0px;">Work Product</asp:Panel>
            <div class="dbbox" style="margin-top:0px;">
                <div class="dbgridblk" style="width:775px; float:inherit">
                    <PLC:PLCDBGrid ID="dbgWorkProduct" runat="server" AllowPaging="false" AllowSorting="false" CancelPostbackOnClick="true" 
                        PLCGridWidth="98%" Height="250px" AutoGenerateColumns="true" >
                    </PLC:PLCDBGrid>
                </div>
            </div>
            <table align="right">
                <tr>
                    <td style="padding-right:5px;">
                        <asp:Button ID="bnWPClose" runat="server" Text="Close" Width="100" />
                    </td>
                </tr>
            </table>
        </asp:Panel>
    </asp:Panel>

    <asp:Button ID="bnStatusChange" runat="server" Style='display: none;' />
    <div id="pnlStatusChangeOverlay" class="overlay">
        <asp:Panel ID="pnlStatusChange" CssClass="modalPopup" runat="server" Style="width:1000px;">
            <asp:Panel runat="Server" ID="pnlStatusDragHandle" DefaultButton="btnStatusSave" Width="100%" >
                <asp:Panel ID="Panel1" runat="server" Height="20px" CssClass="caption" Width="100%">Status Change</asp:Panel>
                <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                    <ContentTemplate>
                        <div class="dbbox" style="margin:0px; padding-bottom:5px;width:100%; ">
                            <div id="divStatChange" style="margin-bottom:5px; background-color:White; height:200px; overflow-y:auto;" >
                                <asp:GridView ID="gvStatusChange" runat="server" DataKeyNames="EVIDENCE_CONTROL_NUMBER,INITIAL_REVIEWER_STATUS" AllowPaging="False" AutoGenerateColumns="False" 
                                    Width="100%" OnRowDataBound="gvStatusChange_RowDataBound" OnRowCommand="gvStatusChange_RowCommand" OnDataBinding="gvStatusChange_DataBinding" EmptyDataText="No records found." 
                                    HeaderStyle-CssClass="fixedgridheader" >
                                    <Columns>
                                        <asp:TemplateField>
                                            <HeaderTemplate>
                                                <asp:CheckBox ID="chkSelect_All" runat="server"  onclick="selectHeader(this,'chkSelect_All');" />
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <asp:CheckBox ID="chkSelect" runat="server" onclick="selectDetail(this,'chkSelect');" />
                                                <asp:HiddenField ID="hfProcess" runat="server" Value='<%# Eval("PROCESS") %>' />
                                                <asp:HiddenField ID="hfReviewStatusID" runat="server" Value='<%# Eval("REVIEW_STATUS") %>' />
                                                <asp:HiddenField ID="hfSeizedForBiology" runat="server" Value='<%# Eval("SEIZED_FOR_BIOLOGY") %>' />
                                                <asp:HiddenField ID="hfFinalReviewBy" runat="server" Value='<%# Eval("FINAL_REVIEW_BY") %>' />
                                                <asp:HiddenField ID="hfItemType" runat="server" Value='<%# Eval("ITEM_TYPE") %>' />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:BoundField DataField="LAB_ITEM_NUMBER" HeaderText="Item #" />
                                        <asp:BoundField DataField="DEPARTMENT_ITEM_NUMBER" HeaderText="CSI #" />
                                        <asp:TemplateField HeaderText="Item Type" >
                                            <ItemTemplate>
                                                <asp:LinkButton ID="lnkSelect" runat="server" Text='<%# Eval("ITEM_TYPE_DESC") %>' CommandArgument='<%# Eval("EVIDENCE_CONTROL_NUMBER") %>' 
                                                    CommandName="Select" Font-Underline="false" />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:BoundField DataField="CURRENT_STATUS" HeaderText="Current Status" />
                                        <asp:BoundField DataField="INITIAL_STATUS" HeaderText="Pending Status" />
                                        <asp:BoundField DataField="FOR_REVIEW_BY" HeaderText="For Review By" />
                                        <asp:TemplateField HeaderText="DNA<br/>Clearance<br/>Form" ItemStyle-HorizontalAlign="Center" >
                                            <ItemTemplate>
                                                <asp:CheckBox ID="chkClearance" runat="server" Checked='<%# Convert.ToBoolean(Eval("DNA_CLEARANCE_ACKNOWLEDGED")) %>' />
                                            </ItemTemplate>
                                        </asp:TemplateField> 
                                    </Columns>
                                </asp:GridView>
                            </div>
                            <asp:Label ID="lblNoticeStatusChange" runat="server" ForeColor="#FF3300" Text=""/>
                            <asp:LinkButton ID="lnkDNAForm" runat="server" Text="* View DNA Clearance Form"  onClientClick="showDNAForm()"  Visible="false" />
                        </div>
                        <div class="dbbox" style="margin:10px 0px 5px 0px; padding-bottom:5px;width:100%;">
                            <div style="background-color:White;" >
                                <table id="dvReview1" runat="server" width="100%">
                                    <tr>
                                        <td style="width:30%; padding-left:5px;" ><span style="text-align:right;" class="prompt">Change Status To: </span></td>
                                        <td style="width:70%;" ><PLC:FlexBox ID="fbStatus" runat="server" TableName="TV_PROCESS" CodeCondition="PROCESS_TYPE NOT IN ('DISP','EXPART')" AttachPopupTo=".dbbox" /></td>
                                    </tr>
                                    <tr>
                                        <td style="padding-left:5px;" ><span ID="spanSecondRev" runat="server" style="text-align:right;" class='prompt required'><em><%=getpromptrequired()%></em>Second Reviewer: </span></td>
                                        <td><PLC:FlexBox ID="fbSecondReviewer" runat="server" TableName="TV_ANALYST" CodeCondition="(ACCOUNT_DISABLED != 'T' OR ACCOUNT_DISABLED IS NULL)" AttachPopupTo=".dbbox" /></td>
                                    </tr>
                                    <tr>
                                        <td style="padding-left:5px;" ><span style="text-align:right;" class="prompt required"><em>*</em>Reviewer Comments: </span></td>
                                        <td><asp:TextBox id="tbComment1" runat="server" TextMode="MultiLine" Rows="3" Width="300" ></asp:TextBox></td>
                                    </tr>                              
                                </table>
                            </div>
                            <div style="padding-top:8px; width:100%;">
                                <table width="100%">
                                    <tr>
                                        <td align="left">
                                            <asp:Button ID="btnStatusSave" runat="server" Text="Save Request" Width="140px" OnClick="btnStatusSave_Click" />
                                            <asp:Button ID="btnCancelStatusChange" runat="server" Text="Cancel Request" Width="140px" OnClick="btnCancelStatusChange_Click" />
                                        </td>
                                        <td align="right">
                                            <asp:Button ID="btnStatusCancel" runat="server" Text="Close" Width="100px" OnClick="btnStatusClose_Click" />
                                        </td>
                                     </tr>
                                 </table>
                            </div>
                        </div>
                    </ContentTemplate>
                    <Triggers>
                        <asp:PostBackTrigger ControlID="btnStatusCancel" />
                    </Triggers>
                </asp:UpdatePanel>
            </asp:Panel>
        </asp:Panel>
    </div>
    <div id="divDNAForm" title="DNA Acknowledgement Form" style="background-color:white; display:none;">
        <asp:Label ID="lblDNAAcknowledgement" CssClass="dispo-review-dna-clearance" runat="server"></asp:Label>
    </div>

    <asp:UpdatePanel ID="UpdatePanel2" runat="server">
        <ContentTemplate>
            <asp:PlaceHolder ID="phMsgBoxPopup" runat="server"></asp:PlaceHolder>        
        </ContentTemplate>    
    </asp:UpdatePanel>

    <asp:HiddenField ClientIDMode="Static" ID="ShowStatusChangePopUp" runat="server" />

    <uc1:PLCItemTask ID="ItemTask1" runat="server" />
    <uc1:PLCKitTask ID="KitTask1" runat="server" OnKitTaskSave_Click="OnKitTaskSave_Click" />

    <div id="idialog-bulkdelitems" title="" style="padding: 10px; display: none;">
        <div id="idialog-bulkdelitems-content">
            <asp:UpdatePanel runat="server">
                <ContentTemplate>
                    <PLC:PLCDBGrid ID="gvBulkDelItems" runat="server" AllowSorting="True" AllowPaging="False" PLCGridWidth="100%" Height="300px" 
                            DataKeyNames="ECN" EmptyDataText="No items for this case." PLCGridName="BULK_DELETE_ITEMS" CancelPostbackOnClick="true"
                        FirstColumnCheckbox="true" OnDataBinding="gvBulkDelItems_DataBinding" OnRowDataBound="gvBulkDelItems_RowDataBound">
                    </PLC:PLCDBGrid>
                    <br />
                    <asp:Label ID="lblNoticeBulkDelete" runat="server" ForeColor="#FF3300" Text=""/>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>

    <div id="idialog-alert" style="display: none;">
        <span id="idialog-message" class="bulkdeleteitem"></span>
    </div>

    <div class="hide">
        <div class="signature-capture">
            <asp:UpdatePanel runat="server">
                <ContentTemplate>
                    <PLC:DBPanel ID="dbpEvidReportSignature" runat="server" PLCDataTable="TV_EVIDREPORT" PLCPanelName="EVIDRPT_SIGNATURE"
                        PLCWhereClause="Where 0 = 1" PLCAuditCode="3000" PLCAuditSubCode="18" PLCDisableDefaultValue="false"
                        PLCAttachPopupTo=".popup-ffb" OnPLCDBPanelGetNewRecord="dbpEvidReportSignature_PLCDBPanelGetNewRecord"></PLC:DBPanel>
                    <PLC:DBPanel ID="dbpEvidReportSignature2" runat="server" PLCDataTable="TV_EVIDREPORT" CreateControlsOnExplicitCall="true"
                        PLCWhereClause="Where 0 = 1" PLCAuditCode="3000" PLCAuditSubCode="18" PLCAttachPopupTo=".popup-ffb"></PLC:DBPanel>
                    <asp:Button ID="btnSaveEvidReportSignature" runat="server" OnClick="btnSaveEvidReportSignature_Click" style="display: none;" />
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>

    <asp:HiddenField ID="hdnBulkDelete" runat="server" Value="" />
    <asp:Button runat="server" ID="btnBulkDelete" OnClick="btnBulkDelete_Click" style="display:none"/>
    <asp:Button runat="server" ID="btnDeleteBulk" OnClick="btnDeleteBulk_Click" style="display:none"/>
    <asp:Button runat="server" ID="btnCancelDeleteBulk" OnClick="btnCancelDeleteBulk_Click" style="display:none"/>
    <asp:Button runat="server" ID="btnCloseDeleteBulk" OnClick="btnCloseDeleteBulk_Click" style="display:none"/>
    <asp:Button runat="server" ID="btnShowBulkPopup" OnClick="btnShowBulkPopup_Click" style="display:none"/>

    <script type='text/javascript'>
        eval($("input[id*='hdnDBPanelFields']").val());

        $(function () {
            var tabbable = {
                editContainer: "div.itemstab .dbbox"
            };
            document.addEventListener('keydown', function (e) {
                var isEditMode = _dbPanelStatus === "EDIT",
                    $editContainer = $(tabbable.editContainer);

                if (isEditMode && e.keyCode === 9) {
                    var isForward = !e.shiftKey;
                    var edgeElement = $editContainer.find(":input:enabled:visible:" + (isForward ? "last" : "first"))[0];
                    var nextElement = $editContainer.find(":input:enabled:visible:" + (isForward ? "first" : "last"))[0];

                    if (edgeElement.id === document.activeElement.id) {
                        e.preventDefault();
                        nextElement.focus();
                    }
                }
            }, true);
            document.addEventListener('focus', function (e) {
                var activeEl = document.activeElement,
                    focusNext = null;

                var tabContainer = document.body;

                if (activeEl != tabContainer && !tabContainer.contains(activeEl)) {
                    focusNext = document;
                } else if (activeEl.href && activeEl.href.endsWith("SkipLink")) {
                    focusNext = $(activeEl).next().find(":tabbable:visible:first");
                }

                if (focusNext) {
                    e.preventDefault();
                    setTimeout(function () { focusNext.focus(); });
                }
            }, true);
        });

        $(function () {
            ToggleStatusChangePopUp();
            var marginTop = 0;

            var prm = Sys.WebForms.PageRequestManager.getInstance();
            if (prm != null) {
                prm.add_pageLoaded(ToggleStatusChangePopUp);
                prm.add_endRequest(function () { dbpanelAttachPopupToParent("div.dbbox", "div[id$='vwAttribute']"); });
            }

            function ToggleStatusChangePopUp() {
                if ($("#ShowStatusChangePopUp").val() == "SHOW") {
                    marginTop = ($(window).height() / 2) - 250;
                    $("#<%= pnlStatusChange.ClientID %>").css('margin-top', marginTop);
                    $("#<%= pnlStatusChange.ClientID %>").css('margin-left', ($(window).width() / 2) - 500);
                    $('#pnlStatusChangeOverlay').css("width", $(window).width());
                    $('#pnlStatusChangeOverlay').css("height", $(window).height());
                    $('#pnlStatusChangeOverlay').css("visibility", "visible");
                } else {
                    $('#pnlStatusChangeOverlay').css("visibility", "hidden");
                }
            }
        });

        function ddlEvidenceReport_Change() {
            $("[id*=hdnEvidenceReport]").val($("[id*=ddlEvidenceReport]").val());
        }

        function ShowSupp49() {
            ddlEvidenceReport_Change();
            $find("bhv49Supp").show();
        }

        function HideSupp49() {
            $find("bhv49Supp").hide();
            clearHTMLDBGridCheckbox();
        }

        function Supp49Grid(additionalCriteria) {
            var params = {
                "gridName": "EVIDENCE_PAGE",
                "additionalCriteria": IsDefined(additionalCriteria) ? additionalCriteria : "",
                "disableOnClick": "F",
                "firstColumnCheckbox": "T"
            };

            $.ajax({
                url: "PLCWebCommon/PLCWebMethods.asmx/GetHTMLDBGrid",
                success: RenderGrid,
                error: GridRenderError,
                dataType: "html",
                data: params,
                type: "POST"
            });

            function RenderGrid(e) {
                $("#npdbg_49Supp").html(e);
                var $btnPrint = $("input[id*=btnPrintEvidenceRpt]");
                if (e != "") {
                    $("[id*=cb_r]").click(function () {
                        onHTMLDBGridRowCheckboxClick(this, "EVIDENCE_PAGE"); 
                    });
                }  else {
                    $btnPrint.attr("disabled", true);
                }
            }

            function GridRenderError(e) {
                alert("Error. " + e);
            }
        }

        function PrintSupp49Report() {
            var $gridCheckboxes = $("[id*=cb_r]:checked");
            var ecnList = "";

            for (var c = 0; c < $gridCheckboxes.length; c++) {
                var rowID = $($gridCheckboxes[c]).attr("id").split("_")[1];
                var datakeys = eval('(' + $("#" + rowID).attr("datakeys") + ')');
                ecnList += GetDataKey(datakeys, "EVIDENCE_CONTROL_NUMBER") + ",";
            }

            $("[id*=hdnECNList]").val(ecnList.substring(0, ecnList.length - 1));
            $("[id*=btnTriggerPrintEvidenceRpt]").click();

            function GetDataKey(datakeys, key) {
                return datakeys[key];
            }
        }

        function SearchSupp49() {
            var $dbp49SuppFields = _plcdbpanellist["dbp49Supp"].fields;
            var criteria = "";

            for (var key in $dbp49SuppFields) {
                var value = GetDBPanelField("dbp49Supp", key);
                if (value != "")
                    criteria += key.split('.')[1] + " = '" + value + "' AND ";
            }

            Supp49Grid("WHERE " + criteria.substring(0, criteria.length - 4));
            $("[id*=btnPrintEvidenceRpt]").attr("disabled", true);
        }

        function ClearSupp49() {
            ClearPLCDBPanelFields("dbp49Supp");
            Supp49Grid();
            $("[id*=btnPrintEvidenceRpt]").attr("disabled", true);
        }

        function onHTMLDBGridClick(row) {
            // nil ;
        }

        function onHTMLDBGridColumnCheckboxClick(id) {
            var $chkSelectAll = $("#" + id);
            var $gridCheckboxes = $("[id*=cb_r]:not(:disabled)");
            var hasReport = $("[id*=ddlEvidenceReport] option").length > 0;

            if ($gridCheckboxes.length > 0 && hasReport) {
                if ($chkSelectAll.is(":checked")) {
                    $gridCheckboxes.attr("checked", "true");
                    $("[id*=btnPrintEvidenceRpt]").removeAttr("disabled");
                } else {
                    $gridCheckboxes.removeAttr("checked");
                    $("[id*=btnPrintEvidenceRpt]").attr("disabled", true);
                }
            }
        }

        function onHTMLDBGridRowCheckboxClick(e, gridName) {
            var hasReport = $("[id*=ddlEvidenceReport] option").length > 0;

            if (hasReport) {
                if ($(e).is(":checked"))
                    $("[id*=btnPrintEvidenceRpt]").removeAttr("disabled");
                else {
                    if ($("[id*=cb_r]").not("#" + $(e).attr("id")).is(":checked"))
                        $("[id*=btnPrintEvidenceRpt]").removeAttr("disabled");
                    else
                        $("[id*=btnPrintEvidenceRpt]").attr("disabled", "true");
                }
            }

            var $chkBxes = $("input[id*=cb_r]:not(:disabled)");
            var $chkAll = $("input[id=cb_" + gridName);

            var numRows = $chkBxes.length;
            var numChecked = 0;

            $chkBxes.each(function (index) {
                if (this.checked) 
                    numChecked++;
            });

            if (numChecked == numRows)
                $chkAll.attr("checked", true);
           else 
                $chkAll.attr("checked", false);
        }

        function clearHTMLDBGridCheckbox() {
            var $gridCheckboxes = $("[id*=cb_r]:not(:disabled)");
            $gridCheckboxes.removeAttr("checked");
        }

        function ClearPLCDBPanelFields(panelname) {
            var dbpanelFields = GetDBPanelFields(panelname);
            for (var fieldname in dbpanelFields) {
                SetDBPanelField(panelname, fieldname, "");
            }
        }

        function CloseSignatureCapture() {
            var $parent = $(".signature-capture").dialog("close");
        }

        function ShowSignatureCapture() {
            var $parent = $(".signature-capture").parent();
            $(".signature-capture").dialog({
                title: "Signature Required",
                resizable: false,
                draggable: true,
                closeOnEscape: false,
                width: 700,
                modal: true,
                autoOpen: true,
                close: function () {
                    // Append to original parent to avoid duplicate ids
                    $(this).parent().appendTo($parent);
                },
                open: function (event, ui) {
                    $(".ui-dialog-titlebar-close", $(this).parent()).hide();
                    $(".ui-widget-overlay").css("position", "fixed");

                    // Fix position of flexbox dropdown
                    $(".popup-ffb").css("position", "absolute").css("top", "0px").css("left", "0px");

                    $(this).parent().appendTo("form");
                },
                buttons: {
                    Save: function () {
                        //$(this).dialog("close");
                        $("[id$=btnSaveEvidReportSignature]").click();
                    },
                    Cancel: function () { $(this).dialog("close"); }
                },
                dragStart: function () {
                    $(".popup-ffb .ffb:visible").hide();
                }
            }).keypress(function (e) {
                if (e.keyCode == '13') { }
            });
        }
    </script>
</asp:Content>
