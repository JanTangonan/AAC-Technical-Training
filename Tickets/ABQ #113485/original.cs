protected void btnAllAnalyst_Click(object sender, EventArgs e)
{
    fbAnalyst.SetValue(string.Empty);
    fbType.SetValue(string.Empty);
    GridView1.SelectedIndex = -1;
    PopulateGrid(string.Empty);
    DisplaySelectedSchedule();

    ToggleSupplementBtnUserOptions();
}

protected void btnCurrentAnalyst_Click(object sender, EventArgs e)
{
    fbAnalyst.SetValue(PLCSessionVars1.PLCGlobalAnalyst);
    fbType.SetValue(string.Empty);
    Flexbox_ValueChanged(null, null);
}

protected void Flexbox_ValueChanged(object sender, EventArgs e)
{
    string condition = string.Empty;
    if (fbAnalyst.GetValue().Trim() != string.Empty)
    {
        condition += " AND A.ANALYST = '" + fbAnalyst.GetValue().Trim() + "'";
    }

    if (fbType.GetValue().Trim() != string.Empty)
    {
        condition += " AND A.TYPE_RES = '" + fbType.GetValue().Trim() + "'";
    }
    GridView1.SelectedIndex = -1;
    PopulateGrid(condition);
    DisplaySelectedSchedule();

    ToggleSupplementBtnUserOptions();
}