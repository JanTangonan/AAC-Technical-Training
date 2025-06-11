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

private void PopulateGrid(string condition)
{
    string sql = string.Empty;

    if (hdnFilterCaseEvents.Value == "T")
        condition += " AND A.TYPE_RES <> 'CE' ";

    if (VSIsScheduleDBGridExists)
    {
        GridView1.PLCSQLString_AdditionalCriteria = PLCSessionVars1.FormatSpecialFunctions("A.CASE_KEY = " + PLCSessionVars1.PLCGlobalCaseKey + condition);
    }
    else
    {
        if (PLCSession.PLCDatabaseServer == "MSSQL")
        {
            sql = "Select A.SCHEDULE_KEY, A.CASE_KEY, A.DATE_RES AS \"Date\", FORMATTIME(A.TIME) AS \"Time\", " +
            "B.DESCRIPTION AS \"Schedule Type\", C.NAME AS \"Officer Name\", " +
             @" CASE WHEN A.EVIDENCE_CONTROL_NUMBER IS NULL THEN
                          CASE
                            WHEN EXISTS (
                                SELECT *
                                FROM TV_SCHDETL S
                                LEFT OUTER JOIN TV_SCHDETL SD ON SD.SCHEDULE_KEY = S.SCHEDULE_KEY
                                WHERE S.SCHEDULE_KEY = A.SCHEDULE_KEY
                            ) THEN
                              COALESCE(
                                (STUFF((SELECT ', ' + LI2.LAB_ITEM_NUMBER
                                 FROM TV_SCHEDULE S
                                 LEFT OUTER JOIN TV_SCHDETL SD ON SD.SCHEDULE_KEY = S.SCHEDULE_KEY
                                 LEFT OUTER JOIN TV_LABITEM LI2 ON LI2.EVIDENCE_CONTROL_NUMBER = SD.EVIDENCE_CONTROL_NUMBER
                                 WHERE S.SCHEDULE_KEY = A.SCHEDULE_KEY
                                 FOR XML PATH ('')), 1, 1, '')),
                                'Deleted')
                          ELSE 'Not Item Related'
                          END
                        ELSE COALESCE(I.LAB_ITEM_NUMBER, 'Deleted')
                     END AS ""Item #"" " +
            "FROM TV_SCHEDULE A LEFT OUTER JOIN TV_SCHTYPE B ON B.TYPE_RES = A.TYPE_RES " +
            "LEFT OUTER JOIN TV_ANALYST C ON C.ANALYST = A.ANALYST " +
            "LEFT OUTER JOIN TV_LABITEM I ON I.EVIDENCE_CONTROL_NUMBER = A.EVIDENCE_CONTROL_NUMBER " +
            "WHERE A.CASE_KEY = " + PLCSessionVars1.PLCGlobalCaseKey + condition + " " +
            "ORDER BY A.DATE_RES, \"Time\", A.SCHEDULE_KEY";
        }
        else
        {
            sql = @"Select A.SCHEDULE_KEY, A.CASE_KEY, A.DATE_RES AS ""Date"", TO_CHAR(CAST(A.TIME AS TIMESTAMP(7)), 'HH:MI:SS') AS ""Time"",
                    B.DESCRIPTION AS ""Schedule Type"", C.NAME AS ""Officer Name"",
                        CASE WHEN A.EVIDENCE_CONTROL_NUMBER IS NULL THEN
                        CASE
                          WHEN EXISTS (
                                SELECT *
                                FROM TV_SCHDETL S
                                LEFT OUTER JOIN TV_SCHDETL SD ON SD.SCHEDULE_KEY = S.SCHEDULE_KEY
                                WHERE S.SCHEDULE_KEY = A.SCHEDULE_KEY
                            ) THEN
                            COALESCE(
                                (SELECT LISTAGG(LI2.LAB_ITEM_NUMBER, ', ') WITHIN GROUP (ORDER BY LI2.LAB_ITEM_NUMBER)
                                FROM TV_SCHEDULE S
                                LEFT OUTER JOIN TV_SCHDETL SD ON SD.SCHEDULE_KEY = S.SCHEDULE_KEY
                                LEFT OUTER JOIN TV_LABITEM LI2 ON LI2.EVIDENCE_CONTROL_NUMBER = SD.EVIDENCE_CONTROL_NUMBER
                                WHERE S.SCHEDULE_KEY = A.SCHEDULE_KEY),
                               'Deleted')
                           ELSE 'Not Item Related'
                           END
                         ELSE COALESCE(I.LAB_ITEM_NUMBER, 'Deleted')
                    END AS ""Item #""
                    FROM TV_SCHEDULE A LEFT OUTER JOIN TV_SCHTYPE B ON B.TYPE_RES = A.TYPE_RES
                    LEFT OUTER JOIN TV_ANALYST C ON C.ANALYST = A.ANALYST
                    LEFT OUTER JOIN TV_LABITEM I ON I.EVIDENCE_CONTROL_NUMBER = A.EVIDENCE_CONTROL_NUMBER
                    WHERE A.CASE_KEY = " + PLCSessionVars1.PLCGlobalCaseKey + condition + " " +
            "ORDER BY A.DATE_RES, A.TIME, A.SCHEDULE_KEY";
        }

        GridView1.PLCSQLString = PLCSessionVars1.FormatSpecialFunctions(sql);
    }

    GridView1.InitializePLCDBGrid();

    // empty 
    if (GridView1.Rows.Count == 0)
    {
        PLCDBPanel1.EmptyMode();
        PLCButtonPanel1.SetEmptyMode();
        PLCButtonPanel1.SetCustomButtonEnabled("Back to Case", true);
        PLCButtonPanel1.SetCustomButtonEnabled("Back to Checklist", true);
        SetAttachmentsClip("CASE", "0");
    }
}


protected void bnCaseFind_Click(object sender, EventArgs e)
{
    ResetGrid(GridViewCase);
    if (GetByCaseSQLSelectCommand())
    {
        if (GridViewCase.Rows.Count > 0)
        {
            bnPrint.Enabled = true;
            bnMap.Enabled = true;
            int caseCount = ((DataView)GridViewCase.DataSource).Count;
            if (caseCount == 1)
                lblCaseCount.Text = string.Format(PLCSession.GetSysPrompt("CaseSearch.lblCaseCount.SINGULAR", "{0} record found."), caseCount);
            else
                lblCaseCount.Text = string.Format(PLCSession.GetSysPrompt("CaseSearch.lblCaseCount.PLURAL", "{0} records found."), caseCount);
            GridViewCase.SelectedIndex = -1;
        }
        else
        {
            bnPrint.Enabled = false;
            bnMap.Enabled = false;
            lblCaseCount.Text = "";
            GridViewCase.Width = Unit.Pixel(700);
        }
        SaveSearchParameters();
    }
    else
    {
        this.mbox.ShowMsg("Alert", "Please specify at least one search parameter.", 0);
        this.mbox.FocusOk();
    }
}

private void ResetGrid(PLCDBGrid grid)
{
    grid.PageIndex = 0;
    grid.ClearSortExpression();
}