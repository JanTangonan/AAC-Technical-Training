public class ButtonPanel : Panel
{
    // Property to store table aliases, set from analystsearch.aspx
    public Dictionary<string, OrderedDictionary> TableAliases { get; set; } = new Dictionary<string, OrderedDictionary>();
}


protected void searchbutton_Click(object sender, EventArgs e)
{
    PLCDBPanel ThePanel = this.Parent.FindControl(PLCTargetControlID) as PLCDBPanel;
    if (ThePanel == null) return;

    PLCDBGrid TheGrid = this.Parent.FindControl(PLCTargetDBGridID) as PLCDBGrid;
    if (TheGrid == null) return;

    List<string> fieldNames = ThePanel.GetFieldNames();
    Dictionary<string, Tuple<string, string, string>> fieldData = new Dictionary<string, Tuple<string, string, string>>();

    foreach (string fieldName in fieldNames)
    {
        string fieldValue = ThePanel.GetMyFieldValue(fieldName);
        string fieldTableName = ThePanel.GetFieldTableName(fieldName);
        string fieldPrompt = ThePanel.GetFieldPrompt(fieldName);

        if (!string.IsNullOrEmpty(fieldValue))
        {
            fieldData[fieldName] = Tuple.Create(fieldTableName, fieldPrompt, fieldValue);
        }
    }

    List<string> whereConditions = new List<string>();

    foreach (var field in fieldData)
    {
        string fieldValue = field.Value.Item3;
        string fieldTableName = field.Value.Item1;

        // Use stored TableAliases property instead of calling helper
        string alias = TableAliases.ContainsKey(fieldTableName) && TableAliases[fieldTableName].Count > 0
            ? TableAliases[fieldTableName][0].ToString()
            : fieldTableName;

        whereConditions.Add($"{alias}.{field.Key} = '{fieldValue}'");
    }

    string whereClause = whereConditions.Count > 0 ? "WHERE " + string.Join(" AND ", whereConditions) : "";

    TheGrid.Visible = true;
    TheGrid.PLCSQLString_AdditionalCriteria = whereClause;
    TheGrid.InitializePLCDBGrid();
}

//
fieldData[fieldName] = Tuple.Create(tableName, fieldName, prompt, mask, codeTable, searchValue, usesLikeSearch);

//
public string WhereClause { get; set; } = string.Empty;

protected void searchbutton_Click(object sender, EventArgs e)
{
    PLCDBPanel ThePanel;
    PLCDBGrid TheGrid;

    bool hasInput = false;

    if (PLCTargetControlID != "")
    {
        if (this.Parent.FindControl(PLCTargetControlID) != null && this.Parent.FindControl(PLCTargetControlID) is PLCDBPanel)
        {
            ThePanel = (PLCDBPanel)this.Parent.FindControl(PLCTargetControlID);
            if (ThePanel != null)
            {
                TheGrid = this.Parent.FindControl(PLCTargetDBGridID) as PLCDBGrid;
                if (TheGrid == null) return;

                Dictionary<string, Dictionary<string, Tuple<string, string, string, string, string, string, bool>>> searchCriteria =
                    HttpContext.Current.Session["SavedSearchCriteria"] as Dictionary<string, Dictionary<string, Tuple<string, string, string, string, string, string, bool>>>
                    ?? new Dictionary<string, Dictionary<string, Tuple<string, string, string, string, string, string, bool>>>();

                // NEW: fieldData now expects 8 values to match your new tuple structure
                Dictionary<string, Tuple<string, string, string, string, string, string, bool>> fieldData =
                    new Dictionary<string, Tuple<string, string, string, string, string, string, bool>>();

                string panelKey = ThePanel.ID;
                var fields = ThePanel.GetPanelFieldRecs();
                List<string> fieldNames = ThePanel.GetFieldNames();

                foreach (var panelRec in fields)
                {
                    string tableName = panelRec.tblname;
                    string fieldName = panelRec.fldname;
                    string prompt = panelRec.prompt;
                    string mask = panelRec.editmask.ToUpper();
                    string codeTable = panelRec.codetable;
                    bool usesLikeSearch = panelRec.usesLikeSearch;

                    string searchValue = ThePanel.GetFieldValue(tableName, fieldName, prompt).Trim();

                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        fieldData[fieldName] = Tuple.Create(tableName, fieldName, prompt, mask, codeTable, searchValue, usesLikeSearch);
                        hasInput = true;
                    }
                }

                if (!hasInput)
                {
                    ShowAlert(Page);
                    return;
                }

                searchCriteria[panelKey] = fieldData;

                // Store search criteria into session variable
                HttpContext.Current.Session["SavedSearchCriteria"] = searchCriteria;

                string whereClauseCondition = WhereClause;

                TheGrid.Visible = true;
                TheGrid.PLCSQLString_AdditionalCriteria = whereClauseCondition;
                TheGrid.InitializePLCDBGrid();
            }
        }
    }
}

//


