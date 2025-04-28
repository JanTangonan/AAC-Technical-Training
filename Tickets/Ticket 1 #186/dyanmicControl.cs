//Create Dynamic controls
protected void dynactrl_modal()
{
    if ((_PLCConnectionString == "") || (_PLCConnectionString == "Provider=;Data Source=;User ID=;Password=;"))
        return;

    Dictionary<object, string> FlexBoxControlsWithParent = new Dictionary<object, string>();
    Dictionary<string, string> MandatoryBases = new Dictionary<string, string>();
    Dictionary<string, string> AddFieldBases = new Dictionary<string, string>();
    Dictionary<string, string> EditFieldBases = new Dictionary<string, string>();
    Dictionary<object, string> SignatureControlsWithName = new Dictionary<object, string>();

    AjaxControlToolkit.MaskedEditExtender mymaskedit;

    TableRow tr = null;
    TableCell tc = null;

    TextBox tb = null;
    DropDownList ddl = null;


    Literal promptlbl = null;

    CheckBox cb = null;

    int ctrlnum = -1;
    int nUPCount = 5;
    LastTabIndex = 0;

    PLCSessionVars sv = new PLCSessionVars();

    // inject javascript block
    //            SetCodeHeadJavaScript();

    Table tbl = new Table();
    tbl.BorderWidth = 0;
    tbl.BorderStyle = BorderStyle.None;
    //    tbl.Font = this.Font;

    PanelRec myprec = null;
    string theEditMask = "";
    bool isDateTime = false;
    string theCodeTable = "";
    int theFldLen = 0;
    int theDisplayWidth = 0;
    string theTblName = "";
    string thePrompt = "";
    string theFldName = "";
    string theRequired = "";
    int theMemoLines = 0;
    string theCodeCondition = "";
    string theSupplementLink = "";
    string theInitialValue = "";
    string theCustom1 = "";
    string theCustom2 = "";
    string AttributeType = "";
    string sBumpNextFieldUp = "";
    string maxDescriptionItems = "";
    bool isBumped = false;
    bool isPacked = false;
    string listOptions = "";
    string defaultValue = "";
    string sPostBack = "";
    bool allowEdit = EnableClientBehavior;
    bool allowEditDBPanel = false; //If the field EDIT_ACCESS_CODES is blank then allow edit. If there is an AUTHCODE entry in it, then the user must have that AUTHCODE assigned to them in order to allow then to edit. 
    bool allowAdd = false;
    string checkboxLabel = "";
    bool hideDescription = false;
    bool hidePrompt = false;
    bool hideField = false; // AAC (09/13/2010) - Rename to hideField as removeFromScreen serves a different purpose
    bool filterByLabCode = false;
    string masterFieldName = string.Empty;
    bool noFutureDates = false;
    bool noPastDates = false;
    bool usesLikeSearch = false;

    string customreportsType;

    string duplicateValues;

    bool uniqueConstraint = false;
    string mandatoryOptionalLabCodes = string.Empty;
    bool barcodeScan = false;

    string separatorText = string.Empty;
    bool hasSeparator = false;

    string mandatoryBase = string.Empty;
    string addBase = string.Empty;
    string editBase = string.Empty;

    string requiredMessage = string.Empty;

    string autoFillTargets = string.Empty;
    string autoFillQuery = string.Empty;

    string displayFormula = string.Empty;

    bool validateOnSave = false;

    string timeDateField = string.Empty;

    bool allowToggleActiveOnly = false;

    bool usesAutoWildCardSearch = false;

    bool usesNotFilter = false;
    int notFilterCount = 0;
    bool usesNullBlankOption = false;

    bool usesProsecutor = false;
    string memoCounter = "";

    if (PLCPanelName == "")
        PLCPanelName = "UNKNOWN";

    PLCQuery qryDBPANEL;
    string sqlDBPANEL = "select TABLE_NAME, PROMPT, FIELD_NAME, EDIT_MASK, LENGTH, CODE_TABLE, MANDATORY, MEMO_FIELD_LINES, " +
        "CODE_CONDITION, BUMP_NEXT_FIELD_UP, NO_PROMPT, OPTIONS, DEFAULT_VALUE, POSTBACK, EDIT_ACCESS_CODES, ADD_ACCESS_CODES, CODE_TABLE_USES_KEY, " +
        "FIELD_DISPLAY_WIDTH, CODE_DESC_FORMAT, CODE_DESC_SEPARATOR, HIDE_DESCRIPTION, REMOVE_FROM_SCREEN, FILTER_BY_LAB_CODE, " +
        "MASTER_FIELD_NAME, SEQUENCE, TAB_ORDER, NO_FUTURE_DATES, NO_PAST_DATES, CUSTOM_REPORTS, FORCE_MASK, SUPPLEMENT_LINK, " +
        "DUPLICATE_VALUES, UNIQUE_CONSTRAINT, MANDATORY_OPTIONAL_LAB_CODES, RETAIN_FOCUS_ON_BARCODE_SCAN, SEPARATOR_TEXT, HAS_SEPARATOR, " +
        "MANDATORY_BASE, LIKE_SEARCH, REQUIRED_FIELD_MESSAGE, AUTOFILL_TARGET, AUTOFILL_QUERY, DISPLAY_FORMULA, VALIDATE_ON_SAVE, " +
        "PROMPT_CODE, TIME_DATE_FIELD, ALLOW_TOGGLE_ACTIVE_ONLY, ADD_BASE, EDIT_BASE, AUTO_WILDCARD_SEARCH, NOT_FILTER, USES_NULL_BLANK_OPTION, ADD_EDIT_ACCESS_PROSECUTOR, MEMO_CHAR_COUNT_COLOR, MAX_DESCRIPTION_ITEMS  from TV_DBPANEL where PANEL_NAME = '" +
        PLCPanelName + "'" + (PLCDbPanelQuery != "" ? PLCDbPanelQuery : "") + " AND ((REMOVE_FROM_SCREEN <> 'T') OR (REMOVE_FROM_SCREEN Is NULL)) Order By SEQUENCE";
    if (!this.useCaching)
    {
        // Not cached, initialize DBPanel field list.
        qryDBPANEL = new PLCQuery();
        qryDBPANEL.SQL = sqlDBPANEL;
        qryDBPANEL.Open();
    }
    else
    {
        // Return cached DBPanel field list.
        qryDBPANEL = CacheHelper.OpenCachedSql(sqlDBPANEL, PLCSession.GetMetadataCacheDuration());
    }

    // PrimaryKey and ROWID constraint needs to be removed before PLCDataTable.Copy() or a constraint exception will occur.
    qryDBPANEL.PLCDataTable.PrimaryKey = null;

    //remove all other constraints
    try
    {
        qryDBPANEL.PLCDataTable.Constraints.Clear();
    }
    catch (Exception ex1)
    {
        PLCSession.WriteDebug("Unable to clear constraints:" + ex1.Message);
    }


    if (qryDBPANEL.PLCDataTable.Columns.IndexOf("ROWID") >= 0)
        qryDBPANEL.PLCDataTable.Columns.Remove("ROWID");

    DataTable fieldlst = qryDBPANEL.PLCDataTable.Copy();
    maxDescriptionItems = "3";



    foreach (DataColumn dc in fieldlst.Columns)
    {

        if (dc.ReadOnly)
        {
            PLCSession.WriteDebug(dc.ColumnName + " is readonly -> attempting to reset");
            try
            {
                dc.ReadOnly = false;
            }
            catch (Exception ex2)
            {
                PLCSession.WriteDebug("***********Unable to reset readonly property to false:" + ex2.Message);
            }
        }
    }

    int colSpan = GetColSpanForPackedField(fieldlst);

    DataColumn DC;
    if (!fieldlst.Columns.Contains("INITIAL_VALUE"))
    {
        DC = new DataColumn();
        DC.DataType = System.Type.GetType("System.String");
        DC.ColumnName = "INITIAL_VALUE";
        fieldlst.Columns.Add(DC);
    }

    if (!fieldlst.Columns.Contains("CUSTOM_1"))
    {
        DC = new DataColumn();
        DC.DataType = System.Type.GetType("System.String");
        DC.ColumnName = "CUSTOM_1";
        fieldlst.Columns.Add(DC);
    }

    if (!fieldlst.Columns.Contains("CUSTOM_2"))
    {
        DC = new DataColumn();
        DC.DataType = System.Type.GetType("System.String");
        DC.ColumnName = "CUSTOM_2";
        fieldlst.Columns.Add(DC);
    }

    if (!fieldlst.Columns.Contains("ATTRIBUTE_TYPE"))
    {
        DC = new DataColumn();
        DC.DataType = System.Type.GetType("System.String");
        DC.ColumnName = "ATTRIBUTE_TYPE";
        fieldlst.Columns.Add(DC);
    }

    // Set the list of tab index numbers per dbpanel field.
    // Use TAB_ORDER column to get tab index. If TAB_ORDER is not specified, default to SEQUENCE column.
    List<short> rowSequenceNumbers = new List<short>();
    foreach (DataRow dr in fieldlst.Rows)
    {
        short tabIndex;
        if (dr["TAB_ORDER"].ToString() != "")
            tabIndex = Convert.ToInt16(dr["TAB_ORDER"]);
        else
            tabIndex = Convert.ToInt16(dr["SEQUENCE"]);

        rowSequenceNumbers.Add(tabIndex);

        // HighestTabIndex is used to maintain the last control in the tab order 
        // so that other objects can add newer controls right after the dbpanel controls.
        if (tabIndex > this.HighestTabIndex)
            this.HighestTabIndex = tabIndex;

        if (dr["NOT_FILTER"].ToString().ToUpper() == "T")
            notFilterCount++;
    }

    // Remove non-nullable SEQUENCE column as it causes problems in CustomFields delegates that add dbpanel fields on the fly.
    if (fieldlst.Columns.Contains("SEQUENCE"))
        fieldlst.Columns.Remove("SEQUENCE");

    PLCDBPanelAddCustomFieldsEventArgs e = new PLCDBPanelAddCustomFieldsEventArgs(fieldlst);
    onplcdbpaneladdcustomfields(e);

    // PLCDBPanel is initialized.

    SetDBPanelProperties();

    int currentRow = 0;
    short tabCount = -1;
    LastTabIndex = -1;
    foreach (DataRow dr in fieldlst.Rows)
    {
        isBumped = (sBumpNextFieldUp == "T");
        isPacked = (sBumpNextFieldUp == "P");


        ctrlnum = ctrlnum + 1;

        // Set tab index for the dbpanel control.
        if (currentRow < rowSequenceNumbers.Count)
            LastTabIndex = rowSequenceNumbers[currentRow];
        else
            LastTabIndex++;

        tabCount++;
        currentRow++;





        theDisplayWidth = MakeInt(dr["FIELD_DISPLAY_WIDTH"].ToString());
        theTblName = dr["TABLE_NAME"].ToString();
        theFldName = dr["FIELD_NAME"].ToString();

        maxDescriptionItems = dr["MAX_DESCRIPTION_ITEMS"].ToString();
        if (String.IsNullOrWhiteSpace(maxDescriptionItems) || PLCSession.SafeInt(maxDescriptionItems) < 1) maxDescriptionItems = "3";
        PLCSession.WriteDebug("DBPANEL maxDescriptionItems:" + maxDescriptionItems);


        thePrompt = PLCSession.GetSysPrompt(dr["PROMPT_CODE"].ToString(), dr["PROMPT"].ToString());
        theSupplementLink = dr["SUPPLEMENT_LINK"].ToString();

        addBase = dr["ADD_BASE"].ToString();
        editBase = dr["EDIT_BASE"].ToString();

        checkboxLabel = thePrompt;
        theEditMask = dr["EDIT_MASK"].ToString();
        isDateTime = theEditMask.StartsWith("DATETIME");
        if (isDateTime)
        {
            theEditMask = theEditMask == "DATETIMEDMY"
                ? "dd/mm/yyyy"
                : "mm/dd/yyyy";
        }

        theFldLen = MakeInt(dr["LENGTH"].ToString());

        bool isConfigAutoCorrectEnabled = PLCSession.GetWebConfiguration("DBPANEL_CONFIG_AUTOCORRECT").ToUpper().Equals("T");
        if (isConfigAutoCorrectEnabled
            && IsDataPanel()
            && !string.IsNullOrWhiteSpace(theTblName))
        {
            DataColumn dc;
            if (!IsConfigValidLength(theFldLen, theTblName, theFldName, out dc))
            {
                theFldLen = dc.MaxLength;
            }
        }

        // If picture mask goes beyond the field length, truncate it to match the max field length.
        if ((theFldLen > 0) && !this.reservedEditMasks.Contains(theEditMask))   // Don't truncate for reserved edit masks such as 'CB', 'COMBOLIST', etc.
            theEditMask = TruncatePictureMaskToMax(theEditMask, theFldLen);

        if (theFldLen < 1)
            theFldLen = 1;

        if (string.IsNullOrEmpty(theEditMask) && dr["FORCE_MASK"].Equals("T"))
            for (int maskCount = 0; maskCount < theFldLen; maskCount++)
                theEditMask += "X";

        theCodeTable = GetCodeTableViewName(dr["CODE_TABLE"].ToString());
        theRequired = dr["MANDATORY"].ToString();
        if (theRequired == "")
            theRequired = "F";

        // Override the required / not required state if it was set previously 
        //   via AddRequiredFieldOverride().
        theRequired = OverrideRequiredFieldIfSpecified(theFldName, theRequired);


        /* Override TV_DBPANEL.MANDATORY_BASE if MANDATORY is T or M*/
        mandatoryBase = dr["MANDATORY_BASE"].ToString();
        if (mandatoryBase == "T")
            theRequired = "T";
        else if (!string.IsNullOrEmpty(mandatoryBase))
        {
            if (theRequired == "F")
                MandatoryBases.Add(theFldName, mandatoryBase);
            else //this is for the scenario where MANDATORY is M or T and MANDATORY_BASE is not empty. MANDATORY_BASE will always be overridden by MANDATORY
                AddMandatoryChildFieldOverride(theFldName);
        }

        // ADD_BASE
        if (!string.IsNullOrEmpty(addBase))
            AddFieldBases.Add(theFldName, addBase);

        // EDIT_BASE
        if (!string.IsNullOrEmpty(editBase))
            EditFieldBases.Add(theFldName, editBase);

        /* override required field and set it to 'F' if logged on user's lab code 
          is defined in TV_DBPANEL.MANDATORY_OPTIONAL_LAB_CODES */
        mandatoryOptionalLabCodes = dr["MANDATORY_OPTIONAL_LAB_CODES"].ToString().Replace(" ", "").ToUpper();
        OverrideRequiredFieldBasedOnLabCodes(mandatoryOptionalLabCodes, ref theRequired);


        PromptType promptType;
        if (theRequired == "T")
            promptType = PromptType.Required;
        else if (theRequired == "M")
            promptType = PromptType.Mandatory;
        else
            promptType = PromptType.Standard;

        theMemoLines = MakeInt(dr["MEMO_FIELD_LINES"].ToString());


        if (theCodeTable == "TV_ANALYST" && !IsSearchPanel)
            theCodeCondition = "(ACCOUNT_DISABLED <> 'T' OR ACCOUNT_DISABLED IS NULL)" +
                (dr["CODE_CONDITION"].ToString() != "" ? " AND " + dr["CODE_CONDITION"].ToString() : "");

        else
            theCodeCondition = dr["CODE_CONDITION"].ToString();

        filterByLabCode = dr["FILTER_BY_LAB_CODE"].ToString() == "T";


        string addChar = (PLCSession.PLCDatabaseServer == "MSSQL" ? "+" : "||");

        if (filterByLabCode)
        {
            string labCodeCondition = "',' " + addChar + " LAB_CODE_FILTER " + addChar + "',' LIKE '%," +
                PLCSession.PLCGlobalLabCode + ",%'";
            if (theCodeTable != string.Empty)
                labCodeCondition += " OR (SELECT COUNT(LAB_CODE_FILTER) FROM " + theCodeTable +
                                    " WHERE ',' " + addChar + " LAB_CODE_FILTER " + addChar + "',' LIKE '%," +
                                    PLCSession.PLCGlobalLabCode + ",%') = 0";
            theCodeCondition = (string.IsNullOrEmpty(theCodeCondition)) ? "(" + labCodeCondition + ")"
                : theCodeCondition + " AND (" + labCodeCondition + ")";
        }

        masterFieldName = dr["MASTER_FIELD_NAME"].ToString();

        if (masterFieldName != string.Empty)
        {
            string masterCodeCondition = "  ( (MASTER_FIELD_FILTER IS NULL)  OR  ( ',' " + addChar + " MASTER_FIELD_FILTER " + addChar + " ',' LIKE '%,[" + masterFieldName + "],%') ) ";
            theCodeCondition = (string.IsNullOrEmpty(theCodeCondition)) ? "(" + masterCodeCondition + ")"
                : theCodeCondition + " AND (" + masterCodeCondition + ")";
        }

        theInitialValue = dr["INITIAL_VALUE"].ToString();
        theCustom1 = dr["CUSTOM_1"].ToString();
        theCustom2 = dr["CUSTOM_2"].ToString();
        AttributeType = dr["ATTRIBUTE_TYPE"].ToString();
        sBumpNextFieldUp = dr["BUMP_NEXT_FIELD_UP"].ToString();
        listOptions = dr["OPTIONS"].ToString();
        defaultValue = dr["DEFAULT_VALUE"].ToString();
        sPostBack = dr["POSTBACK"].ToString();
        allowEditDBPanel = HasEditAccess(dr["EDIT_ACCESS_CODES"].ToString(), sv);
        allowAdd = HasEditAccess(dr["ADD_ACCESS_CODES"].ToString(), sv);
        string codeDescFormat = dr["CODE_DESC_FORMAT"].ToString().Trim(); //flexbox text format
        string codeDescSeparator = dr["CODE_DESC_SEPARATOR"].ToString().Trim(); //flexbox separator for code and description

        hideDescription = (dr["HIDE_DESCRIPTION"].ToString() == "T");
        hidePrompt = (dr["NO_PROMPT"].ToString() == "T");
        hideField = (dr["REMOVE_FROM_SCREEN"].ToString() == "H");
        noFutureDates = dr["NO_FUTURE_DATES"].ToString() == "T";
        noPastDates = dr["NO_PAST_DATES"].ToString() == "T";

        customreportsType = dr["CUSTOM_REPORTS"].ToString();

        duplicateValues = dr["DUPLICATE_VALUES"].ToString();

        uniqueConstraint = dr["UNIQUE_CONSTRAINT"].ToString() == "T";
        barcodeScan = dr["RETAIN_FOCUS_ON_BARCODE_SCAN"].ToString().ToUpper() == "T";

        separatorText = dr["SEPARATOR_TEXT"].ToString();
        hasSeparator = dr["HAS_SEPARATOR"].ToString() == "T";
        usesLikeSearch = dr["LIKE_SEARCH"].ToString() == "T";
        usesAutoWildCardSearch = dr["AUTO_WILDCARD_SEARCH"].ToString().ToUpper().Equals("T");

        requiredMessage = dr["REQUIRED_FIELD_MESSAGE"].ToString();

        autoFillTargets = dr["AUTOFILL_TARGET"].ToString();
        autoFillQuery = dr["AUTOFILL_QUERY"].ToString();

        displayFormula = IsValidConfigSelectSQL(dr["DISPLAY_FORMULA"].ToString()) ? dr["DISPLAY_FORMULA"].ToString() : "";

        validateOnSave = dr["VALIDATE_ON_SAVE"].ToString().ToUpper() == "T";

        timeDateField = dr["TIME_DATE_FIELD"].ToString().ToUpper();

        allowToggleActiveOnly = dr["ALLOW_TOGGLE_ACTIVE_ONLY"].ToString().ToUpper() == "T";

        usesNotFilter = dr["NOT_FILTER"].ToString().ToUpper() == "T";
        usesNullBlankOption = dr["USES_NULL_BLANK_OPTION"].ToString().ToUpper() == "T";

        usesProsecutor = dr["ADD_EDIT_ACCESS_PROSECUTOR"].ToString().ToUpper() == "T";
        memoCounter = dr["MEMO_CHAR_COUNT_COLOR"].ToString().Trim();

        if (hideField) sBumpNextFieldUp = "";

        if (!EnableClientBehavior)
            allowEdit = allowEditDBPanel;

        //add/edit access codes will only be referenced if TV_DEPTNAME.PROSECUTOR is T for the selected department
        //supported in configwebuser page on select of dbpanel value
        if (usesProsecutor)
        {
            allowAdd = allowEdit = allowEditDBPanel = true;
        }

        myprec = new PanelRec(theTblName, thePrompt, theFldName, theEditMask, theCodeTable, theFldLen, theRequired, theMemoLines, theCodeCondition, sBumpNextFieldUp, defaultValue, sPostBack, allowEditDBPanel, theDisplayWidth, hideField, customreportsType, theSupplementLink, uniqueConstraint, mandatoryOptionalLabCodes, mandatoryBase, usesLikeSearch, requiredMessage, autoFillTargets, autoFillQuery, displayFormula, validateOnSave, timeDateField, allowAdd, addBase, editBase, usesAutoWildCardSearch);
        myprec.duplicateValues = duplicateValues;
        panelrecs.Add(myprec);

        myprec.StatusMsg = new Label();
        myprec.StatusMsg.ForeColor = Color.Blue;
        myprec.StatusMsg.Font.Italic = true;
        myprec.StatusMsg.Font.Size = 10;
        myprec.StatusMsg.Visible = false;


        myprec.ErrMsg = new Label();
        myprec.ErrMsg.ForeColor = Color.Red;
        myprec.ErrMsg.Visible = false;


        Panel wrapper = new Panel();
        if (sBumpNextFieldUp == "P")
        {
            wrapper.Style.Add("float", "left");
            wrapper.Style.Add("padding-right", "5px");
        }

        if (!string.IsNullOrEmpty(autoFillTargets) && !string.IsNullOrEmpty(autoFillQuery))
        {
            wrapper.Attributes["AUTOFILLTARGETS"] = autoFillTargets.Replace("%", "");

            var matches = Regex.Matches(autoFillQuery, "(?:%)(.+?)(?:%)");
            var autoFillParams = string.Join(",", matches.Cast<Match>().Select(m => m.Result("$1")));
            wrapper.Attributes["AUTOFILLPARAM"] = autoFillParams;

            wrapper.Attributes["AUTOFILLTRIGGER"] = this.ID + "|" + theTblName + "." + theFldName;
        }

        int packedWidth = (sBumpNextFieldUp == "P" || isPacked) ? (theDisplayWidth > 0 ? theDisplayWidth : (theFldLen > 1 ? theFldLen : (theEditMask.Length > 0 ? theEditMask.Length : 10)) * 10) + (myprec.isDateMask(theEditMask) ? 20 : 0) : 0;

        if ((isBumped || isPacked) && !hideField)
        {
            if (isBumped)
            {
                TableCell cell = new TableCell();
                string style = hidePrompt ? "display:none;" : "";
                promptlbl = CreatePromptMarkup(thePrompt, style, "", promptType, theEditMask);
                promptlbl.ID = this.ID + "_ltrl" + theFldName + ctrlnum.ToString();

                if (theEditMask.Equals("CB") || theEditMask.Equals("CB-R"))
                {
                    promptlbl.Text = "";
                    promptlbl.ID = "";
                }

                cell.Controls.Add(promptlbl);
                tr.Cells.Add(cell);
                tc = new TableCell();
            }

            if (isPacked)
            {
                tc.Attributes.Add("colspan", colSpan.ToString());
                string style = hidePrompt ? "display:none;" : "display:inline-block; vertical-align:" + (theCodeTable == "" ? "5%" : "50%") + ";";
                promptlbl = CreatePromptMarkup(thePrompt, style, "packedLabel", promptType, theEditMask);
                promptlbl.ID = this.ID + "_ltrl" + theFldName + ctrlnum.ToString();

                if (theEditMask.Equals("CB") || theEditMask.Equals("CB-R"))
                {
                    promptlbl.Text = "";
                    promptlbl.ID = "";
                }

                tc.Width = (int)tc.Width.Value + packedWidth + (thePrompt.Length * 8) + 30; // HTML5 fixed for pack

                wrapper.Controls.Add(promptlbl);
            }
        }
        else
        {
            // Create the table row
            tr = new TableRow();
            // Create the cell
            tc = new TableCell();
            //tc.Width = Unit.Point(300);
            tc.BorderWidth = 0;
            tc.BorderStyle = BorderStyle.None;
            //  tc.BackColor = Color.Silver;
            if (sBumpNextFieldUp == "P" && !string.IsNullOrEmpty(theCodeTable))
                tc.VerticalAlign = VerticalAlign.Top;

            string style = hidePrompt ? "display:none;" : "";

            if (theEditMask == "CB")
            {
                promptlbl = CreatePromptMarkup(checkboxLabel, "display:inline-block;width:100%;", "", promptType, "CheckboxPrompt");
            }
            else
            {
                promptlbl = CreatePromptMarkup(thePrompt, style, "", promptType, theEditMask);
            }

            promptlbl.ID = this.ID + "_ltrl" + theFldName + ctrlnum.ToString();

            if (theEditMask.Equals("CB-R"))
            {
                promptlbl.Text = "";
                promptlbl.ID = "";
            }

            tc.Controls.Add(promptlbl);
            tr.Cells.Add(tc);
            tc = new TableCell();
            if (sBumpNextFieldUp == "P") tc.Width = (int)tc.Width.Value + packedWidth; // HTML5 fixed for pack
                                                                                       //tc.BorderWidth = 0;
                                                                                       //tc.BorderStyle = BorderStyle.None;
                                                                                       //    tc.BackColor = Color.Silver;
        }

        // if all fields doesn't have Not Filter on, do not add the cell between prompt and field (where checkbox is added)
        if (notFilterCount > 0)
        {
            TableCell notFilterCell = new TableCell();

            if (usesNotFilter)
            {
                CheckBox chkNotFilter = new CheckBox();
                chkNotFilter.ID = GenerateControlID("chkNot", ref ctrlnum);
                chkNotFilter.ToolTip = "NOT Filter";
                chkNotFilter.TabIndex = LastTabIndex;

                if (isPacked)
                {
                    // if packed, just add the checkbox to the wrapper instead of cell
                    wrapper.Controls.Add(chkNotFilter);
                    tc.Width = (int)tc.Width.Value + 13;
                }
                else
                    notFilterCell.Controls.Add(chkNotFilter);

                myprec.chkNotFilter = chkNotFilter;
            }
            if (!isPacked)
                tr.Cells.Add(notFilterCell);
        }

        if (myprec.isDateMask(theEditMask))
        {
            tb = new TextBox();
            tb.TabIndex = LastTabIndex;
            if (!allowEdit)
            {
                tb.ReadOnly = true;
                tb.BackColor = Color.LightGray;
            }

            if (theDisplayWidth > 0)
            {
                tb.Width = theDisplayWidth;
            }
            else
            {
                tb.Width = Unit.Pixel(110);
            }
            //tb.CssClass = PLCDBPanelCSS;
            tb.ID = "DBP_DATE_" + ID + ctrlnum.ToString();
            myprec.tb = tb;
            tb.Text = theInitialValue;

            tb.Attributes.Clear();
            if (!IsSearchPanel)
                tb.Attributes.Add("onkeydown", "onSingleLineTextBoxKeyDown(event);");
            tb.Attributes.Add("INITVAL", theInitialValue);
            tb.Attributes.Add("CUSTOM1", theCustom1);
            tb.Attributes.Add("CUSTOM2", theCustom2);

            var flexWrapper = new Panel();
            flexWrapper.Attributes.Add("style", "display: -ms-inline-flexbox;display: inline-flex;");

            flexWrapper.Controls.Add(tb);

            AjaxControlToolkit.MaskedEditExtender maskedit = new AjaxControlToolkit.MaskedEditExtender();
            //maskedit.ClearMaskOnLostFocus = true;

            //Set date format as MonthDayYear as default;
            AjaxControlToolkit.MaskedEditUserDateFormat theDateFormat = AjaxControlToolkit.MaskedEditUserDateFormat.MonthDayYear;

            if (IsDMYFormat(theEditMask))
                theDateFormat = AjaxControlToolkit.MaskedEditUserDateFormat.DayMonthYear;

            maskedit.ID = GenerateControlID("maskedit_", ref ctrlnum); //"maskedit_" + ctrlnum.ToString();
            maskedit.Century = 2000;
            //maskedit.UserDateFormat = AjaxControlToolkit.MaskedEditUserDateFormat.MonthDayYear;
            maskedit.UserDateFormat = theDateFormat;
            maskedit.MaskType = myprec.isDateTimeMask(theEditMask) ? MaskedEditType.DateTime : MaskedEditType.Date;
            maskedit.Mask = myprec.isDateTimeMask(theEditMask)
                ? theEditMask.ToUpper().Contains(":SS")
                    ? "99/99/9999 99:99:99"
                    : "99/99/9999 99:99"
                : "99/99/9999";
            maskedit.TargetControlID = tb.ID;
            //             
            ImageButton imgCalendar = new ImageButton();
            imgCalendar.ID = GenerateControlID("imgCalendar_", ref ctrlnum);
            imgCalendar.ImageUrl = "~/Images/calendar.png";
            LastTabIndex++;
            tabCount++;
            imgCalendar.TabIndex = LastTabIndex;
            imgCalendar.Visible = allowEdit;
            //
            myprec.btn = imgCalendar;
            CalendarExtender calendar = new CalendarExtender();
            calendar.FirstDayOfWeek = FirstDayOfWeek.Sunday;
            if (IsDMYFormat(theEditMask))
            {
                calendar.Format = myprec.isDateTimeMask(theEditMask)
                    ? theEditMask.ToUpper().Contains(":SS")
                        ? "dd/MM/yyyy HH:mm:ss "
                        : "dd/MM/yyyy HH:mm "
                    : "dd/MM/yyyy";
                maskedit.CultureName = "en-GB";
            }
            else //Set MM/dd/yyyy as default
            {
                calendar.Format = myprec.isDateTimeMask(theEditMask)
                    ? theEditMask.ToUpper().Contains(":SS")
                        ? "MM/dd/yyyy HH:mm:ss "
                        : "MM/dd/yyyy HH:mm "
                    : "MM/dd/yyyy";
                maskedit.CultureName = "en-US";
            }
            calendar.TargetControlID = tb.ID;
            calendar.PopupButtonID = imgCalendar.ID;
            calendar.Enabled = allowEdit;

            if (noFutureDates)
            {
                if (IsDMYFormat(theEditMask))
                    maskedit.MaxDate = DateTime.Now.ToString("dd/MM/yyyy");
                else
                    maskedit.MaxDate = DateTime.Now.ToShortDateString();

                calendar.MaxDate = DateTime.Now.ToShortDateString();

                myprec.NoFutureDates = true;
            }

            if (noPastDates)
            {
                if (IsDMYFormat(theEditMask))
                    maskedit.MinDate = DateTime.Now.ToString("dd/MM/yyyy");
                else
                    maskedit.MinDate = DateTime.Now.ToShortDateString();

                calendar.MinDate = DateTime.Now.ToShortDateString();

                myprec.NoPastDates = true;
            }

            calendar.OnClientShowing = "CalendarOnClientShowing";

            imgCalendar.Height = Unit.Pixel(16);
            flexWrapper.Controls.Add(imgCalendar);

            if (isDateTime)
            {
                var time = CreateTimeTextBox(flexWrapper, ctrlnum);
                time.TabIndex = LastTabIndex;
                myprec.time = time;
            }

            wrapper.Controls.Add(flexWrapper);
            wrapper.Controls.Add(maskedit);
            wrapper.Controls.Add(calendar);
            wrapper.Controls.Add(myprec.ErrMsg);
            wrapper.Controls.Add(myprec.StatusMsg);

            tc.Controls.Add(wrapper);
        }

        else
        {
            if (theCodeTable == "")
            {
                if (theEditMask == "COMBOLIST")
                {
                    int optionlen = 1;

                    if (listOptions != "")
                    {
                        if (listOptions.Contains("{") && listOptions.Contains("}"))
                        {
                            //Options has select statement that will be used as flexbox datasource 
                            string query = listOptions.Substring(listOptions.IndexOf("{") + 1, (listOptions.IndexOf("}")) - (listOptions.IndexOf("{") + 1));
                            query = PLCSession.ProcessSQLMacro(query);

                            FlexBox flexBox = new FlexBox();
                            flexBox.ID = GenerateControlID("FlexBox", ref ctrlnum);
                            flexBox.Width = 200;
                            flexBox.ReadOnly = !allowEdit;
                            flexBox.TabIndex = LastTabIndex;
                            flexBox.DescriptionFormatCode = codeDescFormat;
                            flexBox.DescriptionSeparator = codeDescSeparator;
                            flexBox.ComboBox = true;
                            flexBox.MaxLength = theFldLen;
                            flexBox.ShowActiveOnly = !IsSearchPanel;
                            flexBox.UsesNullOrBlank = usesNullBlankOption;

                            PLCQuery qryOption = new PLCQuery(query);
                            qryOption.Open();
                            if (!qryOption.IsEmpty())
                            {

                                flexBox.DataSource = qryOption.PLCDataTable.AsEnumerable().Select(a => new
                                {
                                    DESCRIPTION = a["DESCRIPTION"].ToString(),
                                    CODE = a["CODE"].ToString()
                                });
                                flexBox.DataTextField = "DESCRIPTION";
                                flexBox.DataValueField = "CODE";
                                flexBox.DataBind();
                            }
                            else
                            {
                                ListItemCollection emptyCollection = new ListItemCollection();
                                emptyCollection.Add(new ListItem(string.Empty, string.Empty));
                                flexBox.DataSource = emptyCollection;
                                flexBox.DataBind();
                            }

                            myprec.chFlexBox = flexBox;
                            myprec.chFlexBox.Attributes.Clear();
                            myprec.chFlexBox.Attributes.Add("INITVAL", theInitialValue);
                            myprec.chFlexBox.Attributes.Add("CUSTOM1", theCustom1);
                            myprec.chFlexBox.Attributes.Add("CUSTOM2", theCustom2);

                            if (defaultValue != string.Empty)
                            {
                                myprec.chFlexBox.Attributes.Add("DEFAULT_VALUE", defaultValue.Trim());
                            }

                            if (sPostBack == "T")
                                flexBox.AutoPostBack = true;

                            flexBox.ValueChanged += new ValueChangedEventHandler(flexBox_ValueChanged);
                            if (isPacked)
                                flexBox.Style.Add("display", "inline");

                            wrapper.Controls.Add(flexBox);
                            wrapper.Controls.Add(myprec.ErrMsg);
                            wrapper.Controls.Add(myprec.StatusMsg);

                            tc.Controls.Add(wrapper);

                        }
                        else
                        {
                            ddl = new DropDownList();
                            ddl.ID = GenerateControlID("DDL", ref ctrlnum);
                            ddl.Items.Add(new ListItem("", ""));

                            if (usesNullBlankOption)
                                AddNullOrBlankOptions(ddl.Items);

                            ddl.TabIndex = LastTabIndex;
                            ddl.Enabled = allowEdit;

                            ddl.Attributes.Clear();
                            ddl.Attributes.Add("INITVAL", theInitialValue);
                            ddl.Attributes.Add("CUSTOM1", theCustom1);
                            ddl.Attributes.Add("CUSTOM2", theCustom2);
                            ddl.Attributes.Add("DEFAULT_VALUE", defaultValue.Trim());
                            ddl.Attributes.Add("FIELDNAME", theFldName);

                            string[] options = listOptions.Split(',');
                            foreach (string item in options)
                            {
                                string code = string.Empty;
                                string description = string.Empty;
                                //format: (<code1>)<description1>,(<code2>)<description2>,..(<codeN>)<descriptionN>
                                if (item.TrimStart().StartsWith("(") && item.Contains(")"))
                                {
                                    code = item.Substring(item.IndexOf("(") + 1, (item.IndexOf(")")) - (item.IndexOf("(") + 1));
                                    description = item.Substring(item.IndexOf(")") + 1);
                                }
                                else
                                {
                                    code = item.Trim();
                                    description = item.Trim();
                                }
                                ddl.Items.Add(new ListItem(description, code));

                                if (description.Length > optionlen)
                                    optionlen = description.Length;
                            }

                            if (theDisplayWidth == 0)
                            {
                                theDisplayWidth = (optionlen * 10) + 20;
                            }

                            if (sPostBack == "T")
                            {
                                ddl.AutoPostBack = true;
                                ddl.TextChanged += new EventHandler(ddl_TextChanged);
                            }

                            ddl.Width = Unit.Pixel(theDisplayWidth);
                            myprec.ddl = ddl;

                            wrapper.Controls.Add(ddl);
                            wrapper.Controls.Add(myprec.ErrMsg);
                            wrapper.Controls.Add(myprec.StatusMsg);

                            tc.Controls.Add(wrapper);
                        }
                    }
                }
                else if (theEditMask == "HR")
                {


                    Literal thisLiteral = new Literal();
                    thisLiteral.Text = "<hr>";
                    wrapper.Controls.Add(thisLiteral);
                    tc.Controls.Add(wrapper);

                }
                else if (theEditMask == "FREEFORMLIST")
                {
                    // Field type freeform flexbox with CSV-based option
                    PLCSession.WriteDebug("FREEFORMLIST start", true);

                    if (!string.IsNullOrEmpty(listOptions))
                    {
                        ListItemCollection licOptions = new ListItemCollection();
                        List<string> options = new List<string>();
                        string code = string.Empty;
                        string description = string.Empty;
                        int optionLen = 1;

                        PLCSession.WriteDebug("parse flexbox list options", true);
                        options = listOptions
                            .Split(',')
                            .Select(opt => opt.Trim())
                            .ToList();

                        // create option list
                        foreach (string option in options)
                        {
                            if (!string.IsNullOrEmpty(option))
                            {
                                if (option.StartsWith("(") && option.Contains(")"))
                                {
                                    // format: (<code1>)<description1>,(<code2>)<description2>,..(<codeN>)<descriptionN>
                                    code = option.Substring(option.IndexOf("(") + 1, option.IndexOf(")") - (option.IndexOf("(") + 1)).Trim();
                                    description = option.Substring(option.IndexOf(")") + 1).Trim();
                                }
                                else
                                {
                                    code = option;
                                    description = option;
                                }

                                // create item list
                                licOptions.Add(new ListItem(description, code));

                                if (description.Length > optionLen)
                                {
                                    optionLen = description.Length;
                                }
                            }
                        }

                        if (theDisplayWidth == 0)
                        {
                            theDisplayWidth = (optionLen * 10) + 20;
                        }

                        // create flexbox
                        PLCSession.WriteDebug("create flexbox", true);
                        FlexBox fbFreeFormList = new FlexBox();
                        fbFreeFormList.ID = GenerateControlID("FlexBox", ref ctrlnum);
                        fbFreeFormList.Width = theDisplayWidth;
                        fbFreeFormList.ReadOnly = !allowEdit;
                        fbFreeFormList.TabIndex = LastTabIndex;
                        fbFreeFormList.ComboBox = true;
                        fbFreeFormList.MaxLength = theFldLen;
                        fbFreeFormList.DataSource = licOptions;
                        fbFreeFormList.DataBind();
                        fbFreeFormList.ValueChanged += new ValueChangedEventHandler(flexBox_ValueChanged);
                        fbFreeFormList.AttachPopupTo = PLCAttachPopupTo;

                        if (sPostBack == "T")
                        {
                            fbFreeFormList.AutoPostBack = true;
                        }

                        // add to panel rec
                        myprec.chFlexBox = fbFreeFormList;
                        myprec.chFlexBox.Attributes.Clear();
                        myprec.chFlexBox.Attributes.Add("INITVAL", theInitialValue);
                        myprec.chFlexBox.Attributes.Add("CUSTOM1", theCustom1);
                        myprec.chFlexBox.Attributes.Add("CUSTOM2", theCustom2);
                        myprec.chFlexBox.Attributes.Add("DEFAULT_VALUE", defaultValue.Trim());

                        // add control to wrapper
                        PLCSession.WriteDebug("add flexbox to wrapper", true);

                        // add to container
                        if (isPacked)
                        {
                            wrapper.Style.Add("display", "flex");
                            //wrapper.Style.Add("align-items", "center");
                            wrapper.Style.Add("flex-wrap", "wrap");
                        }

                        wrapper.Controls.Add(fbFreeFormList);
                        wrapper.Controls.Add(myprec.ErrMsg);
                        wrapper.Controls.Add(myprec.StatusMsg);

                        PLCSession.WriteDebug("add wrapper to container", true);
                        tc.Controls.Add(wrapper);
                    }

                    PLCSession.WriteDebug("FREEFORMLIST end", true);
                }
                else if (theEditMask == "RADIOLIST")
                {
                    var rbl = new RadioButtonList();
                    rbl.TabIndex = LastTabIndex;
                    rbl.Enabled = allowEdit;

                    rbl.Attributes.Clear();
                    rbl.Attributes.Add("INITVAL", theInitialValue);
                    rbl.Attributes.Add("CUSTOM1", theCustom1);
                    rbl.Attributes.Add("CUSTOM2", theCustom2);
                    rbl.Attributes.Add("FLDNAME", theTblName + "." + theFldName);

                    int optionlen = 1;
                    if (listOptions != "")
                    {
                        string[] options = listOptions.Split(',');
                        foreach (string item in options)
                        {
                            string code = string.Empty;
                            string description = string.Empty;
                            //format: (<code1>)<description1>,(<code2>)<description2>,..(<codeN>)<descriptionN>
                            if (item.Contains("(") && item.Contains(")"))
                            {
                                code = item.Substring(item.IndexOf("(") + 1, (item.IndexOf(")")) - (item.IndexOf("(") + 1));
                                description = item.Substring(item.IndexOf(")") + 1);
                            }
                            else
                            {
                                code = item;
                                description = item;
                            }
                            ListItem li = new ListItem(description.Trim(), code.Trim());
                            rbl.Items.Add(li);

                            if (description.Length > optionlen)
                                optionlen = description.Length;
                        }
                    }

                    //if (theDisplayWidth == 0)
                    //{
                    //    theDisplayWidth = (optionlen * 10) + 20;
                    //}

                    //rbl.Width = Unit.Pixel(theDisplayWidth);
                    rbl.Width = Unit.Percentage(100);
                    myprec.rbl = rbl;

                    wrapper.Controls.Add(rbl);
                    wrapper.Controls.Add(myprec.ErrMsg);
                    wrapper.Controls.Add(myprec.StatusMsg);

                    tc.Controls.Add(wrapper);
                }
                else if (theEditMask == "CB" || theEditMask == "CBDISPLAY" || theEditMask == "CB-R")
                {
                    cb = new CheckBox();
                    cb.TabIndex = LastTabIndex;
                    cb.Enabled = allowEdit;
                    cb.ID = "" + ID + ctrlnum.ToString();
                    cb.Attributes.Add("LINKEDFIELD", theFldName);
                    cb.Attributes.Add("LINKEDLABEL", thePrompt);
                    cb.Attributes.Add("EDITMASK", theEditMask);
                    //*AAC 07072010 - set checkbox postback only if POSTBACK field = 'T'
                    if (sPostBack == "T")
                    {
                        cb.AutoPostBack = true;
                        cb.CheckedChanged += new EventHandler(cb_CheckedChanged);
                    }

                    myprec.cb = cb;

                    wrapper.Controls.Add(cb);

                    if (isBumped || isPacked || theEditMask.Equals("CB-R"))
                    {
                        promptlbl = CreatePromptMarkup(checkboxLabel, "display:inline-block;", "", promptType, "CheckboxPrompt");
                        promptlbl.ID = this.ID + "_ltrl" + theFldName + ctrlnum.ToString();
                        wrapper.Controls.Add(promptlbl);
                        wrapper.Attributes.Add("style", "display: inline-flex;display: -ms-inline-flexbox;");
                    }

                    tc.Controls.Add(wrapper);
                }
                else if (theEditMask == "MULTIPICK" || theEditMask == "MULTIENUM")
                {
                    if (listOptions != "" || theEditMask == "MULTIENUM")
                    {
                        CodeMultiPick multiLookup;
                        if (theEditMask == "MULTIENUM")
                            multiLookup = new CodeMultiPick(ID + ctrlnum.ToString(), true);
                        else
                        {
                            bool listOptionsProcessed = false;
                            if (listOptions.Contains("{") && listOptions.Contains("}"))
                            {
                                listOptions = GetMultiPickSelections(listOptions);
                                listOptionsProcessed = true;
                            }

                            multiLookup = new CodeMultiPick(ID + ctrlnum.ToString(), listOptions);
                            multiLookup.DoNotUpperCaseCode = listOptionsProcessed ? "T" : "";
                        }

                        if (sPostBack == "T")
                        {
                            multiLookup.PostBack = true;
                            multiLookup.CodeMultiPickValueChanged += new EventHandler(multiPick_ValueChanged);
                        }

                        multiLookup.PopupX = "0";
                        multiLookup.PopupY = "0";

                        multiLookup.HeaderPrompt = thePrompt.Trim();
                        multiLookup.SetTextBoxTabindex(LastTabIndex);
                        LastTabIndex++;
                        tabCount++;
                        multiLookup.Enabled = allowEdit;
                        multiLookup.UsesNullOrBlank = usesNullBlankOption;

                        myprec.chMultiLookup = multiLookup;
                        myprec.chMultiLookup.Attributes.Clear();
                        myprec.chMultiLookup.Attributes.Add("INITVAL", theInitialValue);
                        myprec.chMultiLookup.Attributes.Add("CUSTOM1", theCustom1);
                        myprec.chMultiLookup.Attributes.Add("CUSTOM2", theCustom2);

                        wrapper.Controls.Add(multiLookup);
                        wrapper.Controls.Add(myprec.ErrMsg);
                        wrapper.Controls.Add(myprec.StatusMsg);
                        tc.Controls.Add(wrapper);
                    }
                }
                else if (theEditMask == "MULTIPICK_SEARCH")
                {
                    //do nothing
                }
                else if (theEditMask == "MULTIPICK-AC")
                {
                    if (listOptions != "" || theEditMask == "MULTIENUM")
                    {
                        CodeMultiPickAC multiLookup;
                        if (theEditMask == "MULTIENUM")
                        {
                            multiLookup = new CodeMultiPickAC(ID + ctrlnum.ToString(), true);

                        }
                        else
                        {
                            bool listOptionsProcessed = false;
                            if (listOptions.Contains("{") && listOptions.Contains("}"))
                            {
                                listOptions = GetMultiPickSelections(listOptions);
                                listOptionsProcessed = true;
                            }

                            multiLookup = new CodeMultiPickAC(ID + ctrlnum.ToString(), listOptions);
                            multiLookup.DoNotUpperCaseCode = listOptionsProcessed ? "T" : "";

                        }

                        if (sPostBack == "T")
                        {
                            multiLookup.PostBack = true;
                            multiLookup.CodeMultiPickValueChanged += new EventHandler(multiPick_ValueChanged);
                        }

                        multiLookup.PopupX = "0";
                        multiLookup.PopupY = "0";

                        multiLookup.HeaderPrompt = thePrompt.Trim();
                        multiLookup.SetTextBoxTabindex(LastTabIndex);
                        LastTabIndex++;
                        tabCount++;
                        multiLookup.Enabled = allowEdit;
                        multiLookup.UsesNullOrBlank = usesNullBlankOption;

                        myprec.chMultipickAc = multiLookup;
                        myprec.chMultipickAc.Attributes.Clear();
                        myprec.chMultipickAc.Attributes.Add("INITVAL", theInitialValue);
                        myprec.chMultipickAc.Attributes.Add("CUSTOM1", theCustom1);
                        myprec.chMultipickAc.Attributes.Add("CUSTOM2", theCustom2);

                        wrapper.Controls.Add(multiLookup);
                        wrapper.Controls.Add(myprec.ErrMsg);
                        wrapper.Controls.Add(myprec.StatusMsg);
                        tc.Controls.Add(wrapper);
                    }
                }
                else if (theEditMask == "SIGNATURE")
                {
                    var sig = new Signature();
                    sig.ID = GenerateControlID("Signature", ref ctrlnum);
                    sig.Width = theDisplayWidth > 0 ? theDisplayWidth : 200;
                    sig.TabIndex = LastTabIndex;
                    sig.Enabled = allowEdit;

                    if (!string.IsNullOrEmpty(theCodeCondition))
                    {
                        if (theCodeCondition.StartsWith("{") && theCodeCondition.EndsWith("}"))
                        {
                            SignatureControlsWithName.Add(sig, theCodeCondition);
                        }
                    }

                    //if (sPostBack == "T")
                    //{
                    //}

                    myprec.sig = sig;

                    wrapper.Controls.Add(sig);
                    wrapper.Controls.Add(myprec.ErrMsg);
                    wrapper.Controls.Add(myprec.StatusMsg);

                    tc.Controls.Add(wrapper);
                }
                else
                {
                    tb = new TextBox();
                    tb.Attributes.Clear();
                    tb.TabIndex = LastTabIndex;
                    if (!allowEdit)
                    {
                        //tb.ReadOnly = true; // if readonly is set to true, the textbox changes on the client side are ignored during postback
                        tb.Attributes["readonly"] = "readonly";
                        tb.BackColor = Color.LightGray;
                    }

                    tb.Text = theInitialValue;
                    tb.Attributes.Add("INITVAL", theInitialValue);
                    tb.Attributes.Add("CUSTOM1", theCustom1);
                    tb.Attributes.Add("CUSTOM2", theCustom2);

                    if (barcodeScan)
                        tb.Attributes.Add("BarcodeScanHere", "true");

                    if (theFldLen <= 60)
                        tb.Columns = (int)theFldLen;
                    else
                        tb.Columns = 60;

                    tb.MaxLength = (int)theFldLen;

                    tb.ID = "DBP_" + ID + ctrlnum.ToString();
                    myprec.tb = tb;

                    if (myprec.memolines > 0)
                    {
                        myprec.tb.TextMode = TextBoxMode.MultiLine;
                        myprec.tb.Rows = (int)myprec.memolines;
                        myprec.tb.Attributes.Add("onblur", "limitTextBoxLengthOnBlur(event, this, " + theFldLen + ");");
                        myprec.tb.Attributes.Add("onkeydown", "return limitTextBoxLength(event, this, " + theFldLen + ");");
                        myprec.tb.Attributes.Add("ondblclick", "return expandTextBox(event, this);");
                        myprec.tb.Style.Add("white-space", "pre-wrap");

                        if (!string.IsNullOrEmpty(memoCounter) && IsValidColor(memoCounter))
                        {
                            myprec.tb.Attributes.Add("onkeyup", "return setTextBoxLength(event, this, " + theFldLen + ");");
                            myprec.tb.Attributes.Add("onkeypress", "return setTextBoxLength(event, this, " + theFldLen + ");");
                            myprec.tb.Attributes.Add("onchange", "return setTextBoxLength(event, this, " + theFldLen + ");");

                            if (!string.IsNullOrEmpty(myprec.editmask))
                                myprec.tb.Attributes.Add("onfocus", "return setTextBoxLength(event, this, " + theFldLen + ");");

                            myprec.tb.Attributes.Add("MEMOCOUNTER", memoCounter + "|" + theFldLen);

                        }

                    }
                    else
                    {
                        // Client-side callback to prevent Enter key from being processed in single-line textboxes.
                        if (!IsSearchPanel)
                            myprec.tb.Attributes.Add("onkeydown", "onSingleLineTextBoxKeyDown(event);");
                    }

                    if (theDisplayWidth > 0)
                        tb.Width = theDisplayWidth;

                    mymaskedit = null;

                    theEditMask = theEditMask.ToUpper() == "EMAILADDR" || theEditMask.ToUpper() == "WEBLINK" ? "" : theEditMask;

                    if (!myprec.isDateMask(theEditMask) && !string.IsNullOrEmpty(displayFormula))
                    {
                        myprec.AllowEdit = false;
                        myprec.AllowAdd = false;
                        tb.Attributes["readonly"] = "readonly";
                        tb.BackColor = Color.LightGray;
                        tb.Attributes.Add("DISPLAYFORMULAFIELD", this.ID + "|" + theTblName + "." + theFldName);
                        tb.Attributes.Add("FORMULAFIELDVALUE", "");
                    }

                    if (theEditMask != "")
                    {
                        mymaskedit = new AjaxControlToolkit.MaskedEditExtender();

                        mymaskedit.BehaviorID = mymaskedit.ID = "maskedit_" + ID + ctrlnum.ToString();
                        mymaskedit.MaskType = AjaxControlToolkit.MaskedEditType.None;

                        if (theEditMask.ToUpper() == "HH:MM")
                        {
                            mymaskedit.MaskType = MaskedEditType.Time;
                            mymaskedit.Mask = "99:99";
                        }
                        else if (theEditMask.ToUpper() == "HH:MM:SS")
                        {
                            mymaskedit.MaskType = MaskedEditType.Time;
                            mymaskedit.Mask = "99:99:99";
                        }
                        else if (theEditMask.ToUpper() == "BC") //barcode field *09102009*
                        {
                            mymaskedit.Enabled = false;
                            tb.Attributes.Add("onkeydown", "var evt = (evt) ? evt : ((event) ? event : null); var node = (evt.target) ? evt.target : ((evt.srcElement) ? evt.srcElement : null); if ((evt.keyCode == 13) && (node.type=='text')) { if (txt != '') { this.blur(); this.focus(); return false;} } else { txt = this.value; }");
                            tb.Attributes.Add("onblur", "txt = ''");
                        }
                        else if (theEditMask.Contains("9") && //check if numeric with decimal or comma separator
                            (theEditMask.Contains(".") || theEditMask.Contains(",")))
                        {

                            if (theEditMask.StartsWith("-"))
                            {
                                theEditMask = theEditMask.Substring(1);
                                mymaskedit.Mask = theEditMask;
                                mymaskedit.MaskType = MaskedEditType.Number;
                                mymaskedit.AcceptNegative = MaskedEditShowSymbol.Left;
                            }
                            else
                            {
                                mymaskedit.Mask = theEditMask;
                                mymaskedit.MaskType = MaskedEditType.Number;
                            }

                            mymaskedit.InputDirection = MaskedEditInputDirection.RightToLeft;

                        }
                        else
                        {
                            mymaskedit.Mask = theEditMask;   // disabled for now maskedit does not support dbpanel edit mask
                            mymaskedit.ClearMaskOnLostFocus = true;
                        }

                        if (theEditMask.ToUpper().StartsWith("HH:MM"))
                        {
                            myprec.NoFutureDates = noFutureDates;
                            myprec.NoPastDates = noPastDates;
                        }

                        myprec.mee = mymaskedit;
                        mymaskedit.TargetControlID = tb.ID;
                    }

                    if (mymaskedit != null)
                    {
                        wrapper.Controls.Add(mymaskedit);
                    }

                    wrapper.Controls.Add(tb);
                    wrapper.Controls.Add(myprec.ErrMsg);
                    wrapper.Controls.Add(myprec.StatusMsg);

                    tc.Controls.Add(wrapper);
                }
            }
            else if (theCodeTable != "" && (theEditMask == "MULTIPICK_SEARCH" || theEditMask == "MULTIPICK" || AttributeType == "M"))
            {
                // Multilookup controls.
                CodeMultiPick multiLookup;
                if (AttributeType == "M")
                {
                    multiLookup = new CodeMultiPick(ID + ctrlnum.ToString(), theCodeTable, "VALUE", "DESCRIPTION", theCodeCondition, "", true);
                    multiLookup.MaxDescriptionItems = maxDescriptionItems;
                }
                else
                {
                    if (theCodeCondition != "")
                    {
                        multiLookup = new CodeMultiPick(ID + ctrlnum.ToString(), theCodeTable, "", "", this.FormatCodeCondition(theCodeCondition), "", true);
                        multiLookup.MaxDescriptionItems = maxDescriptionItems;
                    }
                    else
                    {
                        multiLookup = new CodeMultiPick(ID + ctrlnum.ToString(), theCodeTable, true);
                        multiLookup.MaxDescriptionItems = maxDescriptionItems;
                    }

                    multiLookup.IsKeepInvalidCodes = !IsSearchPanel;

                    if (sPostBack == "T")
                    {
                        multiLookup.PostBack = true;
                        multiLookup.CodeMultiPickValueChanged += new EventHandler(multiPick_ValueChanged);
                    }
                }

                multiLookup.UsesSearchBar = (theEditMask == "MULTIPICK_SEARCH");
                multiLookup.PopupX = "0";
                multiLookup.PopupY = "0";

                multiLookup.HeaderPrompt = thePrompt.Trim();
                multiLookup.SetTextBoxTabindex(LastTabIndex);
                LastTabIndex++;
                tabCount++;
                multiLookup.Enabled = allowEdit;
                multiLookup.UsesNullOrBlank = usesNullBlankOption;

                multiLookup.MaxDescriptionItems = maxDescriptionItems;


                myprec.chMultiLookup = multiLookup;
                myprec.chMultiLookup.Attributes.Clear();
                myprec.chMultiLookup.Attributes.Add("INITVAL", theInitialValue);
                myprec.chMultiLookup.Attributes.Add("CUSTOM1", theCustom1);
                myprec.chMultiLookup.Attributes.Add("CUSTOM2", theCustom2);

                wrapper.Controls.Add(multiLookup);
                wrapper.Controls.Add(myprec.ErrMsg);
                wrapper.Controls.Add(myprec.StatusMsg);

                PLCSession.WriteDebug("wrapper added:", true);
                tc.Controls.Add(wrapper);
            }
            else if (theCodeTable != "" && (theEditMask == "MULTIPICK-AC"))
            {
                // Multilookup controls.
                CodeMultiPickAC multiLookup;
                if (AttributeType == "M")
                    multiLookup = new CodeMultiPickAC(ID + ctrlnum.ToString(), theCodeTable, "VALUE", "DESCRIPTION", theCodeCondition, "", true);
                else
                {
                    if (theCodeCondition != "")
                    {
                        multiLookup = new CodeMultiPickAC(ID + ctrlnum.ToString(), theCodeTable, "", "", this.FormatCodeCondition(theCodeCondition), "", true);
                    }
                    else
                    {
                        multiLookup = new CodeMultiPickAC(ID + ctrlnum.ToString(), theCodeTable, true);
                    }

                    if (sPostBack == "T")
                    {
                        multiLookup.PostBack = true;
                        multiLookup.CodeMultiPickValueChanged += new EventHandler(multiPick_ValueChanged);
                    }
                }

                multiLookup.UsesSearchBar = (theEditMask == "MULTIPICK_SEARCH");
                multiLookup.PopupX = "0";
                multiLookup.PopupY = "0";

                multiLookup.HeaderPrompt = thePrompt.Trim();
                multiLookup.SetTextBoxTabindex(LastTabIndex);
                LastTabIndex++;
                tabCount++;
                multiLookup.Enabled = allowEdit;
                multiLookup.UsesNullOrBlank = usesNullBlankOption;

                myprec.chMultipickAc = multiLookup;
                myprec.chMultipickAc.Attributes.Clear();
                myprec.chMultipickAc.Attributes.Add("INITVAL", theInitialValue);
                myprec.chMultipickAc.Attributes.Add("CUSTOM1", theCustom1);
                myprec.chMultipickAc.Attributes.Add("CUSTOM2", theCustom2);

                wrapper.Controls.Add(multiLookup);
                wrapper.Controls.Add(myprec.ErrMsg);
                wrapper.Controls.Add(myprec.StatusMsg);

                PLCSession.WriteDebug("wrapper added:", true);
                tc.Controls.Add(wrapper);
            }
            else if (theCodeTable != "" && theEditMask == "IMAGEMULTIPICK")
            {
                ImageMultiPick imgMultipick = new ImageMultiPick(ID + ctrlnum.ToString(), theCodeTable, "", "", this.FormatCodeCondition(theCodeCondition), "");
                imgMultipick.HeaderPrompt = thePrompt.Trim();
                imgMultipick.CustomTabIndex = LastTabIndex;
                LastTabIndex++;
                tabCount++;
                imgMultipick.TextWidth = theDisplayWidth;
                imgMultipick.Draggable = true;

                if (sPostBack == "T")
                {
                    imgMultipick.PostBack = true;
                    imgMultipick.ImageMultiPickValueChanged += new EventHandler(imgMultiPick_ValueChanged);
                }

                myprec.chImgMultiLookup = imgMultipick;
                myprec.chImgMultiLookup.Attributes.Clear();

                wrapper.Controls.Add(imgMultipick);
                wrapper.Controls.Add(myprec.ErrMsg);
                wrapper.Controls.Add(myprec.StatusMsg);

                PLCSession.WriteDebug("wrapper added:", true);
                tc.Controls.Add(wrapper);
            }
            else
            {
                // CodeHead type controls.

                PLCSession.WriteDebug("flex 1", true);

                myprec.attribtype = AttributeType;

                nUPCount++;

                int codeHeadWidth = 200; //default

                CodeHeadAttributes attributes = CodeHeadConfigurations(theCodeTable);

                PLCSession.WriteDebug("flex 2", true);

                codeHeadWidth = attributes.Width;
                string sortOrder = attributes.SortOrder;

                //for flexbox only; if DBPANEL DescFormat and DescSeparator are empty, use CODEHEAD DescFormat and DescSeparator.
                if (codeDescFormat == "")
                    codeDescFormat = attributes.DescriptionFormat;
                if (codeDescSeparator == "")
                    codeDescSeparator = attributes.DescriptionSeparator;

                if (theDisplayWidth > 0)
                    codeHeadWidth = theDisplayWidth;

                PLCSession.WriteDebug("flex 3, displaywidth:" + codeHeadWidth.ToString(), true);


                switch (attributes.CodeHeadType)
                {
                    case ControlType.ComboBox:
                        PLCSession.WriteDebug("case ControlType.ComboBox:", true);
                        TADropDown comboBox = new TADropDown();
                        comboBox.ID = GenerateControlID("ComboBox", ref ctrlnum);
                        comboBox.Font.Size = 10;
                        comboBox.Width = codeHeadWidth;
                        comboBox.TabIndex = LastTabIndex;
                        comboBox.SelectedValue = theInitialValue;
                        comboBox.Enabled = allowEdit;

                        SqlDataSource chComboDataSource = new SqlDataSource();
                        chComboDataSource.ID = GenerateControlID("SqlDataSource", ref nUPCount);
                        chComboDataSource.ConnectionString = _PLCConnectionString;
                        chComboDataSource.Init += new EventHandler(SqlDataSource1_Init);
                        // enable caching on datasource
                        chComboDataSource.EnableCaching = true;
                        chComboDataSource.CacheExpirationPolicy = DataSourceCacheExpiry.Absolute;
                        chComboDataSource.CacheDuration = 600;

                        myprec.chSqlDataSource = chComboDataSource;
                        myprec._codetable = theCodeTable;
                        myprec.chComboBox = comboBox;
                        myprec.chComboBox.Attributes.Clear();
                        myprec.chComboBox.Attributes.Add("INITVAL", theInitialValue);
                        myprec.chComboBox.Attributes.Add("CUSTOM1", theCustom1);
                        myprec.chComboBox.Attributes.Add("CUSTOM2", theCustom2);
                        myprec.chSortOrder = sortOrder;

                        comboBox.Style.Add("display", "block");
                        comboBox.DataSourceID = chComboDataSource.ID;

                        if (sPostBack == "T")
                        {
                            comboBox.AutoPostBack = true;
                            comboBox.Attributes.Add("LINKEDCODETABLE", theCodeTable);
                            comboBox.Attributes.Add("LINKEDFIELDNAME", theFldName);
                            comboBox.SelectedIndexChanged += new EventHandler(comboBox_SelectedIndexChanged);
                        }

                        wrapper.Controls.Add(chComboDataSource);
                        wrapper.Controls.Add(comboBox);
                        wrapper.Controls.Add(myprec.ErrMsg);
                        wrapper.Controls.Add(myprec.StatusMsg);

                        PLCSession.WriteDebug("wrapper added:", true);
                        tc.Controls.Add(wrapper);
                        break;

                    case ControlType.FlexBox:
                        PLCSession.WriteDebug("case ControlType.FlexBox:", true);
                        FlexBox flexBox = new FlexBox();
                        flexBox.ID = GenerateControlID("FlexBox", ref ctrlnum);
                        flexBox.TableName = theCodeTable;
                        flexBox.Width = codeHeadWidth;
                        flexBox.ReadOnly = !allowEdit;
                        flexBox.TabIndex = LastTabIndex;
                        flexBox.DescriptionFormatCode = codeDescFormat;
                        flexBox.DescriptionSeparator = codeDescSeparator;
                        flexBox.ShowActiveOnly = !IsSearchPanel;
                        flexBox.EnableActiveOnlyToggle = allowToggleActiveOnly;
                        flexBox.AttachPopupTo = PLCAttachPopupTo;
                        flexBox.UsesNullOrBlank = usesNullBlankOption;

                        if (AttributeType == "F")
                        {
                            flexBox.ComboBox = true;
                            flexBox.MaxLength = theFldLen;
                        }

                        if (!string.IsNullOrEmpty(theCodeCondition))
                        {
                            //check if it has a parent flexbox
                            if (theCodeCondition.Contains("{") && theCodeCondition.Contains("}"))
                                FlexBoxControlsWithParent.Add(flexBox, theCodeCondition);
                            else if (theCodeCondition.Contains("[") && theCodeCondition.Contains("]"))
                                FlexBoxControlsWithParent.Add(flexBox, theCodeCondition);
                            else
                                flexBox.CodeCondition = theCodeCondition;
                        }

                        myprec._codetable = theCodeTable;
                        myprec.chFlexBox = flexBox;
                        myprec.chFlexBox.Attributes.Clear();
                        myprec.chFlexBox.Attributes.Add("INITVAL", theInitialValue);
                        myprec.chFlexBox.Attributes.Add("CUSTOM1", theCustom1);
                        myprec.chFlexBox.Attributes.Add("CUSTOM2", theCustom2);

                        if (defaultValue != string.Empty)
                        {
                            myprec.chFlexBox.Attributes.Add("DEFAULT_VALUE", defaultValue.Trim());
                        }

                        myprec.chSortOrder = sortOrder;

                        if (sPostBack == "T")
                            flexBox.AutoPostBack = true;

                        flexBox.Attributes.Add("LINKEDCODETABLE", theCodeTable);
                        flexBox.Attributes.Add("LINKEDFIELDNAME", theFldName);
                        flexBox.ValueChanged += new ValueChangedEventHandler(flexBox_ValueChanged);
                        if (isPacked)
                            flexBox.Style.Add("display", "inline-block");

                        if (!String.IsNullOrWhiteSpace(myprec.supplementLink))
                        {
                            String suppTableName = "";
                            String suppTableDesc = "";
                            string suppPrimaryKey = "";
                            string suppFilterCondition = "";
                            string suppSearchPanelName = "";
                            string suppFilterValue = "";
                            try
                            {
                                suppTableName = myprec.supplementLink.Split('|')[0];
                            }
                            catch
                            {
                                suppTableName = "";
                            }


                            try
                            {
                                suppTableDesc = myprec.supplementLink.Split('|')[1];
                            }
                            catch
                            {
                                suppTableDesc = "";
                            }

                            try
                            {
                                suppPrimaryKey = myprec.supplementLink.Split('|')[2];
                            }
                            catch
                            {
                                suppPrimaryKey = "";
                            }

                            try
                            {
                                suppFilterCondition = myprec.supplementLink.Split('|')[3];
                            }
                            catch
                            {
                                suppFilterCondition = "";
                            }

                            try
                            {
                                suppSearchPanelName = myprec.supplementLink.Split('|')[4];
                            }
                            catch
                            {
                                suppSearchPanelName = "";
                            }

                            string suppFilterField = "";
                            string codeCondition = "";
                            string encSuppFilterCondition = AESEncryption.EncryptCodeCondition(suppFilterCondition);

                            if ((!String.IsNullOrWhiteSpace(suppTableName)) && (!String.IsNullOrWhiteSpace(suppTableDesc)))
                            {
                                if (!string.IsNullOrEmpty(suppFilterCondition))
                                {
                                    //support for flexboxes without parent to use DEPARTMENT_CODE filter for Add Officer pop up
                                    if (suppFilterCondition.IndexOf("<&>") >= 0)
                                    {
                                        ProcessMacro(suppFilterCondition, out codeCondition, out suppFilterValue);
                                        int start = suppFilterCondition.IndexOf("<&>") + 3;
                                        int end = suppFilterCondition.IndexOf("</&>");
                                        suppFilterField = suppFilterCondition.Substring(start, end - start);
                                        string filterField = (suppFilterField.Contains("TV_") && suppFilterField.Contains(".")) ? suppFilterField : myprec.tblname + "." + suppFilterField;

                                        flexBox.Attributes.Add("FILTERFIELD", this.ID + "|" + filterField + "|" + encSuppFilterCondition);
                                        flexBox.DisableCaching = true;

                                        flexBox.CodeCondition = codeCondition;
                                    }
                                    else
                                    {
                                        int start = suppFilterCondition.IndexOf('{') + 1;
                                        int end = suppFilterCondition.IndexOf('}');
                                        suppFilterField = suppFilterCondition.Substring(start, end - start);
                                        string filterField = (suppFilterField.Contains("TV_") && suppFilterField.Contains(".")) ? suppFilterField : myprec.tblname + "." + suppFilterField;

                                        flexBox.Attributes.Add("FILTERFIELD", filterField + "|" + encSuppFilterCondition);
                                        flexBox.DisableCaching = true;
                                        FlexBoxControlsWithParent.Add(flexBox, suppFilterCondition);


                                    }


                                }

                                wrapper.Style.Add("white-space", "nowrap");

                                LinkButton lbtn = new LinkButton();

                                //lbtn.Style.Add("display", "inline");
                                lbtn.Text = suppTableDesc;
                                lbtn.Style.Add("white-space", "nowrap");

                                LastTabIndex++;
                                tabCount++;
                                lbtn.TabIndex = LastTabIndex;

                                String pStr = "'" + suppTableName + "','" + lbtn.Text + "','" + this.ID + "','" + myprec.tblname + "','" + myprec.fldname + "','" + suppPrimaryKey + "','" + suppFilterField + "','" + encSuppFilterCondition.Replace("'", "\\'") + "','" + suppFilterValue + "','" + suppSearchPanelName + "'";
                                lbtn.OnClientClick = "return showSupplementDialog(" + pStr + ");";

                                //flexBox.Controls.Add(lbtn);

                                Table t = new Table();
                                t.Style.Add("border-width", "0");
                                t.Style.Add("border-style", "none");
                                t.Style.Add("border-collapse", "collapse");
                                t.Style.Add("margin", "0");
                                t.Style.Add("padding", "0");
                                TableRow r = new TableRow();
                                t.Rows.Add(r);
                                TableCell c = new TableCell();
                                r.Cells.Add(c);
                                c.Style.Add("border-width", "0");
                                c.Style.Add("border-style", "none");
                                c.Style.Add("border-collapse", "collapse");
                                c.Style.Add("margin", "0");
                                c.Style.Add("padding", "0");
                                c.Controls.Add(flexBox);


                                c = new TableCell();
                                c.Wrap = false;
                                c.Style.Add("border-width", "0");
                                c.Style.Add("border-style", "none");
                                c.Style.Add("border-collapse", "collapse");
                                c.Style.Add("margin", "0");
                                c.Style.Add("padding", "0");
                                r.Cells.Add(c);
                                c.Controls.Add(lbtn);

                                wrapper.Controls.Add(t);


                            }
                        }
                        else
                            wrapper.Controls.Add(flexBox);


                        wrapper.Controls.Add(myprec.ErrMsg);
                        wrapper.Controls.Add(myprec.StatusMsg);

                        if (!isPacked && !isBumped)
                            tc.VerticalAlign = VerticalAlign.Top;

                        PLCSession.WriteDebug("wrapper added:", true);
                        tc.Controls.Add(wrapper);
                        break;

                    case ControlType.CodeHeadDialog:
                        PLCSession.WriteDebug("case ControlType.CodeHeadDialog:", true);
                        CodeHead chDlg = new CodeHead();

                        chDlg.ID = GenerateControlID("ch", ref ctrlnum);
                        chDlg.TableName = theCodeTable;
                        chDlg.PopupPanel.Width = Unit.Pixel(550);

                        chDlg.AdditionalFields = "DEPARTMENT_CODE";
                        chDlg.PopupCaption = "Case Officer";
                        chDlg.PopupCaptionCSSClass = "caption";
                        chDlg.PopupCSSClass = "modalPopup";
                        chDlg.ReadOnly = !allowEdit;
                        chDlg.TabIndex = LastTabIndex;

                        myprec.codeHeadDlg = chDlg;
                        wrapper.Controls.Add(chDlg);
                        wrapper.Controls.Add(myprec.ErrMsg);
                        wrapper.Controls.Add(myprec.StatusMsg);

                        PLCSession.WriteDebug("wrapper added:", true);
                        tc.Controls.Add(wrapper);
                        break;

                    case ControlType.Popup:
                        PLCSession.WriteDebug("case ControlType.Popup:", true);
                        myprec.chSortOrder = sortOrder;

                        // parent panel of all controls
                        UpdatePanel chParentUpdatePanel = new UpdatePanel();
                        //chParentUpdatePanel.ID = "ParentUpdatePanel" + nUPCount.ToString();
                        chParentUpdatePanel.ID = GenerateControlID("ParentUpdatePanel", ref nUPCount);
                        chParentUpdatePanel.UpdateMode = UpdatePanelUpdateMode.Always;

                        Panel chPanel = new Panel();
                        chPanel.ID = GenerateControlID("Panel", ref nUPCount);
                        chPanel.CssClass = "modalPopup";
                        chPanel.Style.Value = "display: none;"; //background-color: #f5f5f5; border-width: 3px; border-style: solid; border-color: #294B29;";
                        chPanel.Width = 350;

                        chPanel.BackColor = Color.WhiteSmoke;

                        Panel chCaptionPanel = new Panel();
                        chCaptionPanel.ID = GenerateControlID("CaptionPanel", ref nUPCount);
                        chCaptionPanel.CssClass = "caption";

                        LiteralControl lcText = new LiteralControl();
                        lcText.Text = thePrompt.ToUpper() + " CODES ";
                        chCaptionPanel.Controls.Add(new LiteralControl("<h4 style=\"text-align:center;margin-top:0px;\"><font color=\"#FFFFFF\">"));
                        chCaptionPanel.Controls.Add(lcText);
                        chCaptionPanel.Controls.Add(new LiteralControl("</font></h4>"));

                        UpdatePanel chUpdatePanel = new UpdatePanel();
                        chUpdatePanel.ID = GenerateControlID("UpdatePanel", ref nUPCount);
                        chUpdatePanel.UpdateMode = UpdatePanelUpdateMode.Conditional;

                        Panel pnlSearch = new Panel();

                        TextBox txtSearch = new TextBox();
                        txtSearch.ID = GenerateControlID("TxtSearch", ref nUPCount);
                        txtSearch.AutoPostBack = false;
                        pnlSearch.Controls.Add(txtSearch);
                        myprec.chtxtSearch = txtSearch;

                        Button btnSearch = new Button();
                        btnSearch.ID = GenerateControlID("btnSearch", ref nUPCount);
                        btnSearch.Text = "Search";
                        btnSearch.Click += new EventHandler(BtnSearch_Click);
                        pnlSearch.Controls.Add(btnSearch);
                        myprec.chbtnSearch = btnSearch;

                        ImageButton btnRefresh = new ImageButton();
                        btnRefresh.ID = GenerateControlID("btnRefresh", ref nUPCount);
                        btnRefresh.Height = 22;
                        btnRefresh.Width = 22;
                        btnRefresh.Click += new ImageClickEventHandler(btnRefresh_Click);
                        btnRefresh.ImageUrl = "~/Images/refresh.bmp";
                        btnRefresh.ToolTip = "Refresh Codes";
                        btnRefresh.Enabled = true;
                        pnlSearch.Controls.Add(btnRefresh);
                        myprec.chbtnRefresh = btnRefresh;

                        pnlSearch.DefaultButton = btnSearch.UniqueID;

                        SqlDataSource DataSourceCH = new SqlDataSource();
                        DataSourceCH.ID = GenerateControlID("SqlDataSource", ref nUPCount);
                        DataSourceCH.ConnectionString = _PLCConnectionString;
                        DataSourceCH.Init += new EventHandler(SqlDataSource1_Init);
                        // enable caching on datasource
                        if (!PLCDisableCaching)
                        {
                            DataSourceCH.EnableCaching = true;
                            DataSourceCH.CacheExpirationPolicy = DataSourceCacheExpiry.Absolute;
                            DataSourceCH.CacheDuration = 600;
                        }
                        else
                            DataSourceCH.EnableCaching = false;

                        myprec.chSqlDataSource = DataSourceCH;

                        Panel pnlGrid = new Panel();

                        GridView GridViewCH = new GridView();
                        GridViewCH.ID = GenerateControlID("GridView", ref nUPCount);
                        GridViewCH.DataSourceID = DataSourceCH.ID.ToString();
                        GridViewCH.AllowPaging = true;
                        //GridViewCH.RowCreated += new GridViewRowEventHandler(GridView_RowCreated);
                        GridViewCH.RowDataBound += new GridViewRowEventHandler(GridView_RowDataBound);
                        GridViewCH.PageIndexChanging += new GridViewPageEventHandler(GridView_PageIndexChanging);
                        GridViewCH.Sorting += new GridViewSortEventHandler(GridView_Sorting);
                        GridViewCH.GridLines = GridLines.Horizontal;
                        GridViewCH.Width = Unit.Percentage(100);
                        GridViewCH.AllowSorting = true;
                        GridViewCH.SkinID = "LookupGrid";

                        pnlGrid.Controls.Add(GridViewCH);
                        myprec.chPnlGrid = pnlGrid;
                        myprec.chGridView = GridViewCH;

                        CheckBox chkActive = new CheckBox();
                        chkActive.ID = GenerateControlID("chkActive", ref nUPCount);
                        chkActive.Text = "Active Only";
                        chkActive.AutoPostBack = true;
                        chkActive.CheckedChanged += new EventHandler(chkActive_CheckedChanged);
                        myprec.chActive = chkActive;

                        chUpdatePanel.ContentTemplateContainer.Controls.Add(pnlSearch);
                        chUpdatePanel.ContentTemplateContainer.Controls.Add(pnlGrid);
                        chUpdatePanel.ContentTemplateContainer.Controls.Add(chkActive);
                        chUpdatePanel.ContentTemplateContainer.Controls.Add(DataSourceCH);

                        chPanel.Controls.Add(chCaptionPanel);
                        chPanel.Controls.Add(chUpdatePanel);

                        Button OkButton = new Button();
                        OkButton.ID = GenerateControlID("OkButton", ref nUPCount);
                        OkButton.Text = "OK";
                        OkButton.Width = 80;
                        myprec.chOkButton = OkButton;

                        Button CancelButton = new Button();
                        CancelButton.ID = GenerateControlID("CancelButton", ref nUPCount);
                        CancelButton.Text = "Cancel";
                        CancelButton.Width = 80;
                        myprec.chCancelButton = CancelButton;

                        //// triggers when tabbing from txtLookup
                        //Button TabPostBackButton = new Button();
                        ////TabPostBackButton.ID = "PostBackButton" + nUPCount.ToString();
                        //TabPostBackButton.ID = GenerateControlID("PostBackButton", nUPCount);
                        ////*AAC 061009
                        ////TabPostBackButton.CssClass = PLCDBPanelCSS;
                        //TabPostBackButton.Text = "Cancel";
                        //TabPostBackButton.Width = 1;
                        //TabPostBackButton.Height = 1;
                        //TabPostBackButton.Click += new EventHandler(TabPostBackButton_Click);
                        //myprec.chTabPostBackButton = TabPostBackButton;

                        chPanel.Controls.Add(new LiteralControl("<h3 style=\"text-align:center\">"));
                        chPanel.Controls.Add(OkButton);
                        chPanel.Controls.Add(CancelButton);
                        //chPanel.Controls.Add(TabPostBackButton);
                        chPanel.Controls.Add(new LiteralControl("</h3>"));

                        // end of popupmodal

                        UpdateProgress updprog = new UpdateProgress();
                        //updprog.ID = "myupdateprogress" + nUPCount.ToString();
                        updprog.ID = GenerateControlID("myupdateprogress", ref nUPCount);
                        updprog.ProgressTemplate = new PLCProgressTemplate(updprog);
                        updprog.AssociatedUpdatePanelID = chParentUpdatePanel.ID;
                        myprec.chupdateprog = updprog;

                        TextBox txtLookup = new TextBox();
                        txtLookup.TabIndex = LastTabIndex;
                        txtLookup.Enabled = allowEdit;
                        //*AAC 061109
                        //txtLookup.CssClass = PLCDBPanelCSS;
                        //txtLookup.Font.Name = "Times New Roman";
                        //txtLookup.Font.Size = 9;

                        //txtLookup.ID = "TxtLookup" + nUPCount.ToString();
                        txtLookup.ID = GenerateControlID("txtLookup", ref nUPCount);
                        txtLookup.Width = 158;


                        if (theDisplayWidth > 0)
                            txtLookup.Width = theDisplayWidth;

                        txtLookup.AutoPostBack = true;
                        txtLookup.TextChanged += new EventHandler(txtLookup_TextChanged);

                        myprec.chtb = txtLookup;
                        myprec.chtb.Text = theInitialValue;
                        myprec.chtb.Attributes.Clear();
                        myprec.chtb.Attributes.Add("INITVAL", theInitialValue);
                        myprec.chtb.Attributes.Add("CUSTOM1", theCustom1);
                        myprec.chtb.Attributes.Add("CUSTOM2", theCustom2);
                        myprec.chtb.Attributes.Add("LINKEDCODETABLE", theCodeTable);
                        myprec.chtb.Attributes.Add("LINKEDFIELDNAME", theFldName);

                        HiddenField hdnPostBack = new HiddenField();
                        hdnPostBack.ID = GenerateControlID("hdnPostBack", ref nUPCount);
                        myprec.chHdnPostBack = hdnPostBack;

                        if (sPostBack == "T")
                        {
                            hdnPostBack.Value = "1";
                        }

                        ////This code is for adding postback functionality in a code head (you have to set DBPANEL.POSTBACK to 'T')
                        //if (sPostBack == "T")
                        //{
                        //    Button SelectPostBackButton = new Button();
                        //    SelectPostBackButton.ID = GenerateControlID("SelectPostBackButton", ref nUPCount);
                        //    SelectPostBackButton.Attributes.Add("LINKEDCODETABLE", theCodeTable);
                        //    SelectPostBackButton.Text = "Cancel";
                        //    SelectPostBackButton.Width = 1;
                        //    SelectPostBackButton.Height = 1;
                        //    SelectPostBackButton.Click += new EventHandler(SelectPostBackButton_Click);
                        //    myprec.chSelectPostBackButton = SelectPostBackButton;
                        //    tc.Controls.Add(SelectPostBackButton);
                        //}

                        mymaskedit = null;
                        if (theEditMask.Trim().Length > 0)
                        {
                            mymaskedit = new AjaxControlToolkit.MaskedEditExtender();
                            mymaskedit.ClearMaskOnLostFocus = true;
                            //mymaskedit.ID = "chMaskEdit_" + nUPCount.ToString();
                            mymaskedit.ID = GenerateControlID("chMaskEdit_", ref nUPCount);

                            if ((theEditMask.StartsWith("-")) && (theEditMask.Contains("9")))
                            {
                                theEditMask = theEditMask.Substring(1);
                                mymaskedit.MaskType = AjaxControlToolkit.MaskedEditType.Number;
                                mymaskedit.AcceptNegative = MaskedEditShowSymbol.Left;
                                mymaskedit.Mask = theEditMask;
                            }
                            else
                            {
                                mymaskedit.MaskType = AjaxControlToolkit.MaskedEditType.None;
                                mymaskedit.Mask = theEditMask;
                            }

                            mymaskedit.TargetControlID = txtLookup.ID;

                        }

                        ImageButton imgLookup = new ImageButton();
                        //imgLookup.ID = "ImgLookup" + imgLookup.ClientID.ToString();
                        imgLookup.ID = GenerateControlID("ImgLookup", ref nUPCount);
                        imgLookup.Height = 16;
                        imgLookup.Width = 20;
                        imgLookup.Click += new ImageClickEventHandler(ImgLookup_Click);
                        imgLookup.ImageUrl = "~/Images/Question.PNG";
                        LastTabIndex++;
                        tabCount++;
                        imgLookup.TabIndex = LastTabIndex;
                        imgLookup.Enabled = allowEdit;
                        //                               
                        myprec.chib = imgLookup;

                        Label lblLookup = new Label();
                        //if ((theCodeTable == "ANALYST") && (sBumpNextFieldUp == "T"))
                        //{
                        //    //lblLookup.ID = "LblLookupDummy" + nUPCount.ToString();
                        //    lblLookup.ID = GenerateControlID("LblLookupDummy", ref nUPCount);
                        //    lblLookup.Text = "";
                        //    //*AAC 061009
                        //    //lblLookup.Style.Value = "font-family: Arial; font-size:smaller; color: blue";
                        //    //lblLookup.Height = 1;
                        //    //lblLookup.Width = 1;
                        //    //lblLookup.CssClass = PLCDBPanelCSS;
                        //    lblLookup.Visible = true;
                        //    txtLookup.Width = 100;
                        //    myprec.chlabel = lblLookup;

                        //    tbn = new TextBox();

                        //    tbn.Text = theInitialValue;
                        //    //tbn.ID = "LblLookup" + tbn.ClientID.ToString();
                        //    tbn.ID = GenerateControlID("LblLookup", ref nUPCount);
                        //    myprec.DescCtrl = tbn;
                        //}
                        //else
                        //{
                        //lblLookup.ID = "LblLookup" + nUPCount.ToString();
                        lblLookup.ID = GenerateControlID("LblLookup", ref nUPCount);
                        lblLookup.Text = "";
                        if (hideDescription)
                        {
                            lblLookup.Style.Value = "display: none;";
                        }
                        else
                        {
                            lblLookup.Style.Value = "padding-left:4px;";
                        }
                        //*AAC 061009
                        //lblLookup.Style.Value = "font-family: Arial; font-size:smaller; color: blue";
                        //lblLookup.CssClass = PLCDBPanelCSS;
                        myprec.chlabel = lblLookup;
                        //}

                        Button DummyButton = new Button();
                        //DummyButton.ID = "DummyButton" + nUPCount.ToString();
                        DummyButton.ID = GenerateControlID("DummyButton", ref nUPCount);
                        DummyButton.Style.Value = "display: none;visible:false;";

                        AjaxControlToolkit.ModalPopupExtender mpeCodeHead = new AjaxControlToolkit.ModalPopupExtender();
                        //mpeCodeHead.ID = "mpeCodeHead" + nUPCount.ToString();
                        mpeCodeHead.ID = GenerateControlID("mpeCodeHead", ref nUPCount);

                        //mpeCodeHead.TargetControlID = "DummyButton" + nUPCount.ToString();
                        mpeCodeHead.TargetControlID = DummyButton.ID.ToString();
                        mpeCodeHead.PopupControlID = chPanel.ID.ToString();
                        //mpeCodeHead.BackgroundCssClass = "modalBackground";


                        mpeCodeHead.OkControlID = OkButton.ID.ToString();
                        mpeCodeHead.CancelControlID = CancelButton.ID.ToString();
                        mpeCodeHead.OnOkScript = "onOk()";
                        mpeCodeHead.PopupDragHandleControlID = chCaptionPanel.ID.ToString();

                        chPanel.Controls.Add(mpeCodeHead);

                        myprec.chmpe = mpeCodeHead;
                        myprec._codetable = theCodeTable;

                        chParentUpdatePanel.ContentTemplateContainer.Controls.Add(updprog);
                        chParentUpdatePanel.ContentTemplateContainer.Controls.Add(txtLookup);
                        chParentUpdatePanel.ContentTemplateContainer.Controls.Add(hdnPostBack);
                        if (mymaskedit != null)
                            chParentUpdatePanel.ContentTemplateContainer.Controls.Add(mymaskedit);
                        chParentUpdatePanel.ContentTemplateContainer.Controls.Add(imgLookup);
                        chParentUpdatePanel.ContentTemplateContainer.Controls.Add(lblLookup);
                        //if ((theCodeTable == "ANALYST") && (sBumpNextFieldUp == "T") && (tbn != null))
                        //    chParentUpdatePanel.ContentTemplateContainer.Controls.Add(tbn);

                        // Put lookup modal popup in a separate update panel.
                        UpdatePanel chLookupUpdatePanel = new UpdatePanel();
                        chLookupUpdatePanel.ID = GenerateControlID("LookupUpdatePanel", ref nUPCount);
                        chLookupUpdatePanel.UpdateMode = UpdatePanelUpdateMode.Conditional;
                        chLookupUpdatePanel.ContentTemplateContainer.Controls.Add(DummyButton);
                        chLookupUpdatePanel.ContentTemplateContainer.Controls.Add(chPanel);
                        myprec.lookupUpdatePanel = chLookupUpdatePanel;

                        wrapper.Controls.Add(chLookupUpdatePanel);
                        wrapper.Controls.Add(chParentUpdatePanel);
                        wrapper.Controls.Add(myprec.ErrMsg);
                        wrapper.Controls.Add(myprec.StatusMsg);

                        PLCSession.WriteDebug("wrapper added:", true);
                        tc.Controls.Add(wrapper);

                        //*******END CODEHEAD
                        break;

                    default:
                        PLCSession.WriteDebug("default switch, !!!!!!!!!!!!!!!!!:" + attributes.CodeHeadType.ToString(), true);
                        break;
                        //else
                        //    tc.Controls.Add(myprec.ErrMsg);
                }
            }
        }

        if (tb != null && theCodeTable == "")
        {
            if (sPostBack == "T")
            {
                //postback when textbox value is changed
                tb.Attributes.Add("FIELDNAME", theFldName);
                tb.AutoPostBack = true;
                tb.TextChanged += new EventHandler(tb_TextChanged);
            }
        }

        myprec.original.ID = this.ID + "_hdn" + theFldName + ctrlnum + "_original";
        tc.Controls.Add(myprec.original);
        myprec.originalDesc.ID = this.ID + "_hdn" + theFldName + ctrlnum + "_originalDesc";
        tc.Controls.Add(myprec.originalDesc);

        tr.Cells.Add(tc);

        if (hideField)
        {
            tr.Style.Value = "visibility: hidden; display: none;";
        }

        if (!string.IsNullOrEmpty(separatorText))
        {
            TableRow trSeparator = new TableRow();
            trSeparator.ID = this.ID + "_tr" + theFldName + ctrlnum + "_separatortext";
            trSeparator.CssClass = "dbpanel-separator";
            trSeparator.Cells.Add(new TableCell() { Text = separatorText, ColumnSpan = 10 });
            tbl.Rows.Add(trSeparator);
        }

        tr.ID = this.ID + "_tr" + theFldName + ctrlnum.ToString();
        tbl.Rows.Add(tr);
        if (tabCount > this.HighestTabIndex)
            this.HighestTabIndex = tabCount;

        if (hasSeparator)
        {
            TableRow trSeparator = new TableRow();
            trSeparator.ID = this.ID + "_tr" + theFldName + ctrlnum + "_separator";
            trSeparator.Controls.Add(new TableCell() { Text = "<hr/>", ColumnSpan = 10 });
            tbl.Rows.Add(trSeparator);
        }
    }

    foreach (KeyValuePair<object, string> fb in FlexBoxControlsWithParent)
    {
        string codeCondition = fb.Value;
        if (codeCondition.Contains("{") && codeCondition.Contains("}"))
        {
            int startIndex = codeCondition.IndexOf("{") + 1;
            int endIndex = codeCondition.IndexOf("}");
            string fieldName = codeCondition.Substring(startIndex, endIndex - startIndex);

            FlexBox parentFlexBox = GetFlexBoxControl(fieldName);

            FlexBox flexBox = (FlexBox)fb.Key;

            if (parentFlexBox != null)
                flexBox.ParentFlexBox = parentFlexBox;
            else
                flexBox.ParentControl = GetControl(fieldName);

            flexBox.CodeCondition = codeCondition;

            AssignFlexBoxParentControls(flexBox);
        }
        else
        {
            int startIndex = codeCondition.IndexOf("[") + 1;
            int endIndex = codeCondition.IndexOf("]");
            string fieldName = codeCondition.Substring(startIndex, endIndex - startIndex);

            FlexBox parentFlexBox = GetFlexBoxControl(fieldName);

            FlexBox flexBox = (FlexBox)fb.Key;
            flexBox.ParentFlexBox = parentFlexBox;
            flexBox.CodeCondition = codeCondition;
        }
    }

    foreach (var sig in SignatureControlsWithName)
    {
        string nameField = sig.Value;
        if (nameField.StartsWith("{") && nameField.EndsWith("}"))
        {
            var signature = (Signature)sig.Key;
            var parentPR = GetPanelRecByFieldName(nameField.Trim(new char[] { '{', '}' }));
            signature.TargetControl = (parentPR.tb != null ? (Control)parentPR.tb : parentPR.chFlexBox);
            signature.NameRequired = parentPR.required == "T";
        }
    }

    // done with buttonpanel
    //thereader.Close();
    //thereader.Dispose();

    Literal myLiteral = null;

    myLiteral = new Literal();
    myLiteral.Text = "<P> </P>";
    this.Controls.Add(myLiteral);

    bool isConfigCheckEnabled = PLCSession.GetWebConfiguration("DBPANEL_CONFIG_CHECK").ToUpper().Equals("T");
    if (isConfigCheckEnabled
        && IsDataPanel())
    {
        string errorMessage;
        if (!IsConfigValid(out errorMessage))
        {
            myLiteral.Text = "<div style='color:red;'>" + errorMessage + "</div>";
        }
    }

    if (PLCDisplayTopBorder)
    {

        myLiteral = new Literal();
        myLiteral.Text = "<hr>";
        this.Controls.Add(myLiteral);

    }

    //*AAC 0601509
    //tbl.CssClass = PLCDBPanelCSS;

    this.Controls.Add(tbl);

    if (PLCDisplayBottomBorder)
    {
        myLiteral = new Literal();
        myLiteral.Text = "<hr>";
        this.Controls.Add(myLiteral);
    }

    foreach (PanelRec pr in panelrecs)
    {
        if (!string.IsNullOrEmpty(pr.duplicateValues))
        {
            if (pr.chFlexBox != null)
                pr.chFlexBox.Attributes.Add("DUPLICATEVALUES", "fb|" + pr.duplicateValues);
        }
    }

    List<string> baseFieldControlsToValidate = new List<string>();

    foreach (KeyValuePair<string, string> mb in MandatoryBases)
    {
        ParseMandatoryBaseValues(mb.Key, mb.Value, ref baseFieldControlsToValidate);
    }

    BindOnChangeEventToControls(baseFieldControlsToValidate);

    // ADD_BASE
    List<string> addFieldControlsToValidate = new List<string>();
    foreach (KeyValuePair<string, string> mb in AddFieldBases)
    {
        PanelRec rec = this.GetPanelRecByFieldName(mb.Key);
        if (rec != null && rec.AllowAdd)
        {
            ParseMandatoryBaseValues(mb.Key, mb.Value, ref addFieldControlsToValidate);
        }
    }
    if (addFieldControlsToValidate.Count > 0)
        BindOnChangeEventToAddBases("ADD", addFieldControlsToValidate);

    // EDIT_BASE
    List<string> editFieldControlsToValidate = new List<string>();
    foreach (KeyValuePair<string, string> mb in EditFieldBases)
    {
        PanelRec rec = this.GetPanelRecByFieldName(mb.Key);
        if (rec != null && rec.AllowEdit)
        {
            ParseMandatoryBaseValues(mb.Key, mb.Value, ref editFieldControlsToValidate);
        }
    }

    if (editFieldControlsToValidate.Count > 0)
        BindOnChangeEventToAddBases("EDIT", editFieldControlsToValidate);
}