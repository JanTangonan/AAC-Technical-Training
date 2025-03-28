protected void PLCButtonPanel1_PLCButtonClick(object sender, PLCCONTROLS.PLCButtonClickEventArgs e)
{
    switch (e.button_name.ToLower())
    {
        //case "dbpanel":
        //    Response.Redirect("~/ConfigDBPanelActive.aspx");
        //    break;
        case "pw settings":
            Response.Redirect("~/PWDconfig.aspx");
            break;
        case "sr questions":
            Response.Redirect("SRQuestion.ASPX");
            break;
        case "documents":
            BindDocumentGrid();
            mpeDoc.Show();
            return;
        case "sr route":
            Response.Redirect("~/ConfigSRRoute.aspx");
            break;
        case "edit next case":
            Response.Redirect("~/ConfigNextCase.aspx");
            break;
        case "instrument config":
            Response.Redirect("~/InstrumentConfig.aspx");
            break;
        case "confirmatory task config":
            Response.Redirect("~/ConfirmatoryTaskConfig.aspx");
            break;
        case "address book defaults":
            ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "_showAddBookPopup" + DateTime.Now.ToString(), "showAddressBook();", true);
            LabCode = GridView1.SelectedDataKey.Value.ToString();
            mpLabCode.SetText(PLCSession.PLCGlobalLabCode);
            BindAddressBook();
            break;
        case "sak delete":
            ShowSAKDelete();
            return;
        case "sak documents":
            InitSAKControls();
            ShowSAKDocumentsPopup();
            return;
        case "dashboard panels":
            ShowDashboardPanels();
            return;
    }

    if (e.button_name == "Reference")
    {
        PLCSessionVars1.PLCGloablCaseRefKey = "NONE";
        Response.Redirect("~/CaseRef.aspx");
    }

    if ((e.button_name == "ADD") & (e.button_action == "AFTER"))
    {
        GridView1.SetControlMode(false);
        PLCDBPanel1.FocusField("CUSTODY_CODE");
        btnBCSettings.Enabled = false;
    }

    if ((e.button_name == "EDIT") & (e.button_action == "AFTER"))
    {
        GridView1.SetControlMode(false);
        btnBCSettings.Enabled = false;
    }

    if ((e.button_name == "SAVE") & (e.button_action == "AFTER"))
    {
        GridView1.SetControlMode(true);
        btnBCSettings.Enabled = true;
    }

    if ((e.button_name == "CANCEL") & (e.button_action == "AFTER"))
    {
        GridView1.SetControlMode(true);
        btnBCSettings.Enabled = true;
    }

    GridView1.InitializePLCDBGrid();
}

private void ShowDashboardPanels()
{
    var defaultDBPanelsSort = PLCDBGlobal.instance.GetDefaultDashboardPanelSorting();
    string globalDBSort = PLCSession.GetGlobalIni("DASHBOARD_PANELS_SORT");
    Dictionary<string, string> dictPanelsSort = new Dictionary<string, string>();
    if (!string.IsNullOrEmpty(globalDBSort))
    {
        string[] panels = globalDBSort.Trim().Split(',').ToArray();
        foreach (string pnl in panels)
        {
            string panel = pnl.Trim();
            if (defaultDBPanelsSort.ContainsKey(panel) && !dictPanelsSort.ContainsKey(panel))
                dictPanelsSort.Add(panel, defaultDBPanelsSort[panel].ToString());
        }

        foreach (KeyValuePair<string, string> panel in defaultDBPanelsSort)
        {
            if (!dictPanelsSort.ContainsKey(panel.Key))
                dictPanelsSort.Add(panel.Key, panel.Value);
        }
    }
    else
        dictPanelsSort = defaultDBPanelsSort;

    var helper = new PLCHelperFunctions();
    string script = "showSortDashBoardPanels("
        + helper.JSONStrFromStruct(dictPanelsSort) + ");";
    ScriptManager.RegisterStartupScript(this,
        this.GetType(),
        "_showSortDashBoardPanels",
        script,
        true);
}