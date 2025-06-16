private void InitializeStatusChangeGrid()
{
    gvStatusChange.SelectedRowStyle.ForeColor = gvStatusChange.RowStyle.ForeColor;
    gvStatusChange.SelectedRowStyle.BackColor = gvStatusChange.RowStyle.BackColor;
    gvStatusChange.SelectedRowStyle.Font.Bold = false;

    PLCQuery qryItems = new PLCQuery(String.Format(@"SELECT
				LI.EVIDENCE_CONTROL_NUMBER,LI.ITEM_TYPE, LI.LAB_ITEM_NUMBER, LI.DEPARTMENT_ITEM_NUMBER, IT.DESCRIPTION AS ITEM_TYPE_DESC, AN.NAME as FOR_REVIEW_BY,
				LI.PROCESS, PC.DESCRIPTION AS CURRENT_STATUS, RR.REVIEW_STATUS, 
				RR.INITIAL_REVIEW_BY, PI.DESCRIPTION AS INITIAL_STATUS,RR.INITIAL_REVIEWER_STATUS,
				RR.FINAL_REVIEW_BY, CASE WHEN RR.FINAL_REVIEWER_STATUS <> 'Agree' AND RR.FINAL_REVIEWER_STATUS <> 'Disagree' THEN '' ELSE RR.FINAL_REVIEWER_STATUS END AS FINAL_REVIEWER_STATUS,
				CASE WHEN RR.DNA_CLEARANCE_ACKNOWLEDGED = 'T' THEN 1 ELSE 0 END AS DNA_CLEARANCE_ACKNOWLEDGED, LI.SEIZED_FOR_BIOLOGY
				FROM TV_LABITEM LI 
				INNER JOIN TV_LABCASE LC ON LI.CASE_KEY = LC.CASE_KEY 
				INNER JOIN TV_ITEMTYPE IT ON LI.ITEM_TYPE = IT.ITEM_TYPE
				LEFT OUTER JOIN TV_REVREQUEST RR ON LI.EVIDENCE_CONTROL_NUMBER = RR.EVIDENCE_CONTROL_NUMBER
				LEFT OUTER JOIN TV_PROCESS PC ON LI.PROCESS = PC.PROCESS_TYPE
				LEFT OUTER JOIN TV_PROCESS PI ON RR.INITIAL_REVIEWER_STATUS = PI.PROCESS_TYPE
                LEFT OUTER JOIN TV_ANALYST AN ON AN.ANALYST = RR.FINAL_REVIEW_BY 
				WHERE LI.CASE_KEY = {0}
				ORDER BY LI.ITEM_SORT", PLCSession.PLCGlobalCaseKey, PLCSession.PLCGlobalAnalyst));
    qryItems.Open();

    gvStatusChange.DataSource = qryItems.PLCDataTable;
    gvStatusChange.DataBind();

    bool isInitial = false;
    bool isSeized = false;
    foreach (GridViewRow row in gvStatusChange.Rows)
    {
        String reviewStatus = ((HiddenField)row.FindControl("hfReviewStatusID")).Value;
        String finalReviewBy = ((HiddenField)row.FindControl("hfFinalReviewBy")).Value;
        String seizedForBiology = ((HiddenField)row.FindControl("hfSeizedForBiology")).Value;

        if (reviewStatus == "2" && finalReviewBy == PLCSession.PLCGlobalAnalyst)
        {
            //					isFinal = true;
        }
        else
            isInitial = true;

        if (seizedForBiology == "Y")
            isSeized = true;
    }
    fbStatus.Enabled = isInitial;
    fbSecondReviewer.Enabled = isInitial;
    tbComment1.Enabled = isInitial;
    lnkDNAForm.Visible = isSeized;
}

//
protected void bnRequestFind_Click(object sender, EventArgs e)
{
    Menu1_MenuItemClick(Menu1, new MenuEventArgs(Menu1.Items[RESULTS_INDEX]));
}

protected void Menu1_MenuItemClick(object sender, MenuEventArgs e)
{
    int selectedMenuIndex;
    int.TryParse(e.Item.Value, out selectedMenuIndex);
    Menu1.Items[selectedMenuIndex].Selected = true;
    MultiView1.ActiveViewIndex = selectedMenuIndex;

    if (selectedMenuIndex == RESULTS_INDEX)
    {
        if (!RefreshGrid())
        {
            MultiView1.ActiveViewIndex = 0;
            Menu1.Items[0].Selected = true;
            this.mbox.ShowMsg("Alert", "Please specify at least one search parameter.", 0);
        }
    }
}

private bool RefreshGrid()
{
    dbgAssignSearch.EmptyDataText = "No Assignments found.";
    bnAddToBatch.Visible = Session["BATCH_ASSIGN_KEY"] != null;
    dbpAssignSearchInfo.EmptyMode();

    if (BindAssignmentGrid())
    {
        if (dbgAssignSearch.Rows.Count > 0)
        {
            bnCreateBatch.Enabled = !PLCSession.CheckUserOption("DSBLASSG"); //true && !DSBLASSG = !DSLASSG
            bnAddToBatch.Enabled = !PLCSession.CheckUserOption("DSBLASSG"); //true && !DSBLASSG = !DSLASSG
            bnAssign.Enabled = true;
            bnAssignRoute.Enabled = true;
            bnPrint.Enabled = true;
            bnSRPrint0.Enabled = true;
            lAssignmentCount.Text = ((DataView)dbgAssignSearch.DataSource).Count + " assignments";

            SetReadOnlyAccess();
        }
        else
        {
            bnCreateBatch.Enabled = false;  //false && !DSBLASSG = false
            bnAddToBatch.Enabled = false; //false && !DSBLASSG = false
            bnAssign.Enabled = false;
            bnAssignRoute.Enabled = false;
            bnPrint.Enabled = false;
            bnSRPrint0.Enabled = false;
            lAssignmentCount.Text = string.Empty;
        }

        CreateDictionary();
        return true;
    }
    return false;
}

private bool BindAssignmentGrid()
{
    dbgAssignSearch.PLCSQLString_AdditionalFrom = string.Empty;
    dbgAssignSearch.PLCSQLString_AdditionalCriteria = string.Empty;

    Dictionary<string, PanelRecItem> panelValues = GetPanelValues();
    StringBuilder sqlWhere = new StringBuilder("WHERE 1 = 1");
    StringBuilder sReportCondition = new StringBuilder("1 = 1");

    string sCriteria = string.Empty;
    List<string> lCriteria = new List<string>();
    Dictionary<string, OrderedDictionary> tables = GetTablesAndAlias(false);

    foreach (KeyValuePair<string, PanelRecItem> kvp in panelValues)
    {
        string key = kvp.Key;
        PanelRecItem pr = kvp.Value;

        if (pr.fieldName == "SELECTION_GROUP") continue;

        if (pr.fieldName == "DRUG_TYPE" && !string.IsNullOrEmpty(pr.value))
        {
            dbgAssignSearch.PLCSQLString_AdditionalFrom = "INNER JOIN " +
                "TV_ITASSIGN IA2 ON IA2.EXAM_KEY = E.EXAM_KEY INNER JOIN " +
                "TV_ATTRCODE AC2 on AC2.EVIDENCE_CONTROL_NUMBER = IA2.EVIDENCE_CONTROL_NUMBER ";
        }

        if (PLCSession.GetLabCtrl("USES_CASE_TYPE_CHARGE_GROUPS") == "T" && pr.fieldName == "CASE_TYPE" && !string.IsNullOrEmpty(pr.value))
        {
            dbgAssignSearch.PLCSQLString_AdditionalFrom = "INNER JOIN " +
                "TV_OFFENSE O ON C.OFFENSE_CODE = O.OFFENSE_CODE ";
        }

        if (pr.fieldType != PanelRecFieldType.DATE && !string.IsNullOrEmpty(pr.value.Trim()))
        {
            if (pr.fieldType == PanelRecFieldType.MULTIPICK || pr.fieldType == PanelRecFieldType.MULTIPICK_SEARCH)
            {
                if (pr.tableName == "TV_TASKLIST")
                {
                    if (!dbgAssignSearch.PLCSQLString_AdditionalFrom.Contains("INNER JOIN TV_TASKLIST TL "))
                        dbgAssignSearch.PLCSQLString_AdditionalFrom += "INNER JOIN TV_TASKLIST TL ON TL.EXAM_KEY = E.EXAM_KEY ";

                    StringBuilder taskTypeList = new StringBuilder();
                    StringBuilder taskTypeDesc = new StringBuilder();

                    List<string> selectedSections = pr.value.Split(',').Select(x => x.Trim()).ToList();
                    List<string> selectionDescription = pr.desc.Split(',').Select(x => x.Trim()).ToList();

                    int ctr = 1;

                    foreach (string taskType in selectedSections)
                    {
                        taskTypeList.AppendFormat("'{0}'{1}", taskType, (selectedSections.Count > ctr ? ", " : string.Empty));
                        ctr++;
                    }

                    ctr = 1;

                    sqlWhere.AppendFormat(" AND TL.{1} IN ({0})", taskTypeList.ToString(), pr.fieldName);

                    sReportCondition.AppendFormat(" AND {{TV_TASKLIST.{1}}} IN [{0}]", taskTypeList.ToString(), pr.fieldName);

                    foreach (string desc in selectionDescription)
                    {
                        taskTypeDesc.AppendFormat("{0}{1}", desc, (selectionDescription.Count > ctr ? " OR " : string.Empty));
                        ctr++;
                    }

                    string friendlyFieldName;
                    if (pr.fieldName == "TASK_TYPE")
                        friendlyFieldName = "Task Type";
                    else
                        friendlyFieldName = pr.prompt;

                    lCriteria.Add(String.Format("{0} IN {1}", friendlyFieldName, taskTypeDesc.ToString()));
                }
                else
                //if (pr.tableName = "TV_LABEXAM" && pr.fieldName == "SECTION")
                {
                    StringBuilder fieldList = new StringBuilder();
                    StringBuilder fieldDesc = new StringBuilder();

                    List<string> selectedSections = pr.value.Split(',').Select(x => x.Trim()).ToList();
                    List<string> selectionDescription = pr.desc.Split(',').Select(x => x.Trim()).ToList();

                    int ctr = 1;

                    foreach (string fieldType in selectedSections)
                    {
                        fieldList.AppendFormat("'{0}'{1}", fieldType, (selectedSections.Count > ctr ? ", " : string.Empty));
                        ctr++;
                    }

                    ctr = 1;

                    sqlWhere.AppendFormat(" AND " + tables[pr.tableName][0] + "." + pr.fieldName + " IN ({0})", fieldList.ToString());

                    sReportCondition.AppendFormat(" AND {{" + pr.tableName + "." + pr.fieldName + "}} IN [{0}]", fieldList.ToString());

                    foreach (string desc in selectionDescription)
                    {
                        fieldDesc.AppendFormat("{0}{1}", desc, (selectionDescription.Count > ctr ? " OR " : string.Empty));
                        ctr++;
                    }

                    lCriteria.Add(pr.prompt + " IN " + fieldDesc.ToString());
                }
            }
            else
            {
                if (pr.fieldName == "DRUG_TYPE")
                {
                    /* Ticket#40993 - The 'desc' value is the code needed as parameter for this sql string, 
                     * TV_ATTRCODES only contains CODE.
                     */
                    sqlWhere.Append(" AND (AC.ATTRIBUTE = 'DRG') AND (AC.VALUE = '" + pr.desc + "') ");

                }
                else if (PLCSession.GetLabCtrl("USES_CASE_TYPE_CHARGE_GROUPS") == "T" && pr.fieldName == "CASE_TYPE")
                {
                    sqlWhere.Append(" AND C.CASE_TYPE = '" + pr.value + "' ");
                }
                else
                {
                    sqlWhere.AppendFormat(" AND {0}.{1} = '{2}' ", tables[pr.tableName][0], pr.fieldName, pr.value);
                }
                lCriteria.Add(pr.prompt + " = " + pr.value);
                sReportCondition.AppendFormat(" AND {0}.{1} = '{2}'", "{" + pr.tableName, pr.fieldName + "}", pr.value);
            }
        }

    }

    Dictionary<string, string> dateRangeContainer = new Dictionary<string, string>();

    string dateCondition = " AND FORMATDATE({0}.{1}) {2} '{3}' ";
    string dateConditionFrom = " AND {0}.{1} {2} CONVERTTODATE('{3}') ";
    string dateConditionTo = " AND {0}.{1} {2} CONVERTTODATE('{3}',true) ";

    var dateFields = from item in panelValues
                     where item.Value.fieldType == PanelRecFieldType.DATE
                     orderby item.Value.tableName, item.Key
                     group item.Value by item.Value.tableName;

    foreach (IGrouping<string, PanelRecItem> dateField in dateFields)
    {
        string rawDateName = string.Empty;
        string tableName = dateField.Key;
        string editmask = string.Empty;

        foreach (PanelRecItem pr in dateField)
        {
            editmask = dbpAssignSearch.GetPanelRecByFieldName(pr.fieldName).editmask;

            if (!string.IsNullOrEmpty(pr.value))
            {
                string[] datePrompt = pr.prompt.Split(' ');
                if (datePrompt[datePrompt.Length - 1].ToUpper() == "FROM")
                {
                    if (editmask.ToUpper().StartsWith("DD"))
                        sqlWhere.AppendFormat(dateConditionFrom, tables[tableName][0], pr.fieldName, ">=", PLCSession.DateStringToMDY(pr.value));
                    else
                        sqlWhere.AppendFormat(dateConditionFrom, tables[tableName][0], pr.fieldName, ">=", pr.value);
                    dateRangeContainer.Add(pr.fieldName + "_START", pr.value);
                    rawDateName = pr.fieldName;
                }
                else if (datePrompt[datePrompt.Length - 1].ToUpper() == "TO")
                {
                    if (editmask.ToUpper().StartsWith("DD"))
                        sqlWhere.AppendFormat(dateConditionTo, tables[pr.tableName][0], pr.fieldName, "<=", PLCSession.DateStringToMDY(pr.value));
                    else
                        sqlWhere.AppendFormat(dateConditionTo, tables[pr.tableName][0], pr.fieldName, "<=", pr.value);
                    dateRangeContainer.Add(pr.fieldName + "_END", pr.value);
                    rawDateName = pr.fieldName;
                }
                else
                {
                    sqlWhere.AppendFormat(dateCondition, tables[pr.tableName][0], pr.fieldName, "=", pr.value);

                    DateTime dtEqual = Convert.ToDateTime(pr.value);
                    sReportCondition.AppendFormat(" AND ({{{0}}} >= DateTime({1},{2},{3},0,0,0))", pr.tableName + "." + pr.fieldName,
                        dtEqual.Year, dtEqual.Month, dtEqual.Day);
                }
                lCriteria.Add(pr.prompt + " = " + pr.value);
            }
        }


        if (!string.IsNullOrEmpty(rawDateName) && (dateRangeContainer.ContainsKey(rawDateName + "_START") || dateRangeContainer.ContainsKey(rawDateName + "_END")))
        {
            sReportCondition.Append(" AND " + PLCCommonClass.GetDateRangeReportCriteria(dateRangeContainer.ContainsKey(rawDateName + "_START") ?
                dateRangeContainer[rawDateName + "_START"] : string.Empty, dateRangeContainer.ContainsKey(rawDateName + "_END") ?
                dateRangeContainer[rawDateName + "_END"] : string.Empty, tableName + "." + rawDateName));
        }

    }

    // Don't allow an empty search.
    if (PLCCommon.instance.IsNoCriteriaWhereClause(sqlWhere.ToString()))
        return false;

    // Log search criteria.
    PLCSession.ForceWriteDebug("AssignSearch: " + sqlWhere.ToString(), true);

    if (chkDRF.Checked)
    {
        sqlWhere.Append(" AND (E.REPORT_FORMAT IS NULL OR E.REPORT_FORMAT = '') ");
    }

    dbgAssignSearch.PLCSQLString_AdditionalCriteria = PLCSession.FormatSpecialFunctions(sqlWhere.ToString());

    lCriteria.Sort();
    foreach (string item in lCriteria)
    {
        if (sCriteria.Length > 0)
            sCriteria += ", ";
        sCriteria += item;
    }

    txtCriteria.Text = sCriteria;
    ViewState["ASSIGNSEARCH_SQLWHERE"] = sReportCondition;

    // Set search parameters to session to be retrieved elsewhere as necessary
    Session["RedirectFromAssignSearch"] = true;
    PLCSession.PLCAssignSearchSelect = dbgAssignSearch.PLCSQLString;
    PLCSession.PLCAssignSearchFrom = dbgAssignSearch.PLCSQLString_AdditionalFrom;
    PLCSession.PLCAssignSearchWhere = dbgAssignSearch.PLCSQLString_AdditionalCriteria;

    dbgAssignSearch.PLCSQLString = PLCSession.FormatSpecialFunctions(dbgAssignSearch.PLCSQLString);
    dbgAssignSearch.InitializePLCDBGrid();
    return true;
}