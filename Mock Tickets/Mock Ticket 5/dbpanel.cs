/// <summary>
/// Returns Panel field values
/// </summary>
/// <returns></returns>
public Dictionary<string, Tuple<string, string, string, string>> GetPanelFieldValues()
{
    Dictionary<string, Tuple<string, string, string, string>> fieldData = new Dictionary<string, Tuple<string, string, string, string>>();
    bool hasInput = false;

    var fields = this.GetPanelFieldRecs();

    foreach (var panelRec in fields)
    {
        string tableName = panelRec.tblname;
        string fieldName = panelRec.fldname;
        string prompt = panelRec.prompt;

        string fieldValue = this.GetFieldValue(tableName, fieldName, prompt).Trim();

        if (!string.IsNullOrEmpty(fieldValue))
        {
            fieldData[fieldName] = Tuple.Create(tableName, fieldName, prompt, fieldValue);
            hasInput = true;
        }
    }
    return hasInput ? fieldData : null;
}

/// <summary>
/// Sets panel field values 
/// </summary>
/// <param name="searchCriteria"></param>
public void SetPanelFieldValues(Dictionary<string, Dictionary<string, Tuple<string, string, string, string>>> fieldValues)
{
    if (fieldValues != null && fieldValues.ContainsKey(this.ID))
    {
        Dictionary<string, Tuple<string, string, string, string>> fieldData = fieldValues[this.ID];

        if (fieldData.Count > 0)
        {
            foreach (var field in fieldData)
            {
                string tableName = field.Value.Item1;
                string fieldName = field.Key;
                string prompt = field.Value.Item3;
                string fieldValue = field.Value.Item4;

                this.SetPanelFieldValue(tableName, fieldName, prompt, fieldValue);
            }
        }
    }
}

/// <summary>
/// Restores search criteria to the dbpanel
/// </summary>
/// <param name="thePanel"></param>
private void RestoreSearchCriteria(DBPanel panel)
{
    var savedCriteria = Session[saveSearchCriteria] as Dictionary<string, Dictionary<string, Tuple<string, string, string, string>>>;

    if (savedCriteria != null)
    {
        var cleanedCriteria = new Dictionary<string, Dictionary<string, Tuple<string, string, string, string>>>();

        foreach (var item in savedCriteria)
        {
            string originalKey = item.Key.Replace(pageKey, "");
            cleanedCriteria[originalKey] = item.Value;
        }

        panel.SetPanelFieldValues(cleanedCriteria);
    }
}