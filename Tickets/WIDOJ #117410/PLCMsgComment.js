function DoPostBack() {
    var bn = document.getElementById(bnPostBackID);
    if (bn != null) {
        bn.click();
    }
}

function OK_Click(ctrlID, postbackID) {
    try {
        if (ctrlID != "") {
            // check comment is not empty on OK
            if (document.getElementById("<%=txtComment.ClientID %>").value != '') {
                document.getElementById(ctrlID).value = document.getElementById("<%=txtComment.ClientID %>").value;
                if (postbackID != "") {
                    bnPostBackID = postbackID;
                    setTimeout("DoPostBack()", 1000);
                }
            }
        }
    }
    catch (err) {
    }

}        