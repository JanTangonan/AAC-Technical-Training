/// <summary>
/// Creates a basic notes object and retrieve its detail
/// </summary>
/// <param name="pstrSection">The section of the notes</param>
/// <param name="pstrGroupName">The groud name</param>
public CustomNotes(string pstrSection, string pstrGroupName)
{
    // set the values
    _strSection = pstrSection;
    _strGroupName = pstrGroupName;

    // retrieve the list of details based on the section and group name
    _lstNoteDetails = GetCustomNotesDetails(pstrGroupName, pstrSection);
}

/// <summary>
/// Creates a basic notes object and retrieve its detail and saved answers
/// </summary>
/// <param name="pstrSection">The section of the notes</param>
/// <param name="pstrGroupName">The groud name</param>
/// <param name="pstrCaseKey">The case key of the assignment</param>
/// <param name="pstrExamKey">The assignment key</param>
public CustomNotes(string pstrSection, string pstrGroupName, string pstrCaseKey, string pstrExamKey)
{
    // set the values
    _strSection = pstrSection;
    //_strGroupName = pstrGroupName.First().ToString().ToUpper() + pstrGroupName.Substring(1);
    _strGroupName = pstrGroupName;

    // retrieve the list of details based on the section and group name
    _lstNoteDetails = GetCustomNotesDetails(pstrGroupName, pstrSection);

    if (_lstNoteDetails != null)
    {
        // foreach note details, we will retrieve its saved values
        foreach (CustomNoteDetails objDetails in _lstNoteDetails)
        {
            objDetails.AssignSavedAnswer(pstrCaseKey, pstrExamKey);
        }
    }
}

/// <summary>
/// Creates a basic notes object and retrieve its detail and saved answers
/// </summary>
/// <param name="pstrSection">The section of the notes</param>
/// <param name="pstrGroupName">The groud name</param>
/// <param name="pstrCaseKey">The case key of the assignment</param>
/// <param name="pstrExamKey">The assignment key</param>
/// <param name="pblnIsPremade">Indicate whether we are trying to create a premade note</param>
public CustomNotes(string pstrSection, string pstrGroupName, string pstrCaseKey, string pstrExamKey, bool pblnIsPremade)
{
    // set the values
    _strSection = pstrSection;
    _strGroupName = pstrGroupName;

    // check whether we need to retrieve a premade value or not
    if (pblnIsPremade)
    {
        // retrieve the list of details based on the section and group name
        _lstNoteDetails = GetCustomNotesDetails(pstrGroupName);

        if (_lstNoteDetails != null)
        {
            // foreach note details, we will retrieve its saved values
            foreach (CustomNoteDetails objDetails in _lstNoteDetails)
            {
                objDetails.AssignSavedAnswer(pstrCaseKey, pstrExamKey, pstrGroupName);
            }
        }
    }
    else
    {
        // retrieve the list of details based on the section and group name
        _lstNoteDetails = GetCustomNotesDetails(pstrGroupName, pstrSection);

        if (_lstNoteDetails != null)
        {
            // foreach note details, we will retrieve its saved values
            foreach (CustomNoteDetails objDetails in _lstNoteDetails)
            {
                objDetails.AssignSavedAnswer(pstrCaseKey, pstrExamKey);
            }
        }
    }
}

/// <summary>
/// Retrieves the list of examination note fields of a specific section
/// </summary>
[WebMethod(EnableSession = true)]
public void GetExaminationNoteFields()
{
    HttpContext.Current.Response.ClearHeaders();
    HttpContext.Current.Response.ClearContent();
    HttpContext.Current.Response.Clear();
    HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
    HttpContext.Current.Response.ContentType = "application/json";
    HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;

    List<CustomNotes> lstCustomNotes = null;
    string strSection = string.Empty;
    string strCaseKey = string.Empty;
    string strExamKey = string.Empty;

    // retrieve the section to be used
    strSection = HttpContext.Current.Request["SECTION"].ToString();
    strCaseKey = HttpContext.Current.Request["CASE_KEY"].ToString();
    strExamKey = HttpContext.Current.Request["EXAM_KEY"].ToString();

    // retrieve the list of custom notes
    lstCustomNotes = GetCustomNotes(strSection, strCaseKey, strExamKey);

    HttpContext.Current.Response.Write(JSONStrFromStruct(lstCustomNotes));
    HttpContext.Current.Response.Flush();
    HttpContext.Current.ApplicationInstance.CompleteRequest();
}

/// <summary>
/// Retrieves the list of custom notes and its saved answers
/// </summary>
/// <param name="pstrSection">The section where we will search the groups</param>
/// <param name="pstrCaseKey">The Case Key of the Assignment</param>
/// <param name="pstrExamKey">The Exam Key of the Assignment</param>
/// <returns>A list of custom notes arranged in groups</returns>
private List<CustomNotes> GetCustomNotes(string pstrSection, string pstrCaseKey, string pstrExamKey)
{
    PLCDBGlobal plcDbGlobal = new PLCDBGlobal();
    List<CustomNotes> lstCustomNotes = null;
    List<string> lstDistinctGroups = null;
    List<string> lstIgnoreGroups = null;
    List<string> lstNoteSetup = null;

    // initialize
    lstCustomNotes = new List<CustomNotes>();

    // retrieve the list of groups to ignore
    lstIgnoreGroups = plcDbGlobal.GetIgnoreGroups();

    // retrieves the a list of distinct note groups
    lstDistinctGroups = plcDbGlobal.GetDistinctCustomGroups(lstIgnoreGroups, pstrSection);

    // retrieve the list of tabs indicated in the notes setup table
    lstNoteSetup = plcDbGlobal.GetNotesSetupRecords(pstrSection);

    // render notes specified in TV_NOTESETUP
    if (lstNoteSetup != null)
    {
        foreach (string strNotesTab in lstNoteSetup)
        {
            if (lstDistinctGroups != null)
            {
                if (lstDistinctGroups.Contains(strNotesTab, StringComparer.OrdinalIgnoreCase))
                {
                    lstCustomNotes.Add(new CustomNotes(pstrSection, strNotesTab, pstrCaseKey, pstrExamKey));
                    continue;
                }
            }

            if (strNotesTab.ToLower() == "statistics")
            {
                lstCustomNotes.Add(new CustomNotes(pstrSection, strNotesTab, pstrCaseKey, pstrExamKey));
            }
            else if (strNotesTab.ToLower() == "comments" || strNotesTab.ToLower() == "chemicals used")
            {
                lstCustomNotes.Add(new CustomNotes(pstrSection, strNotesTab, pstrCaseKey, pstrExamKey, true));
            }
        }
    }

    return lstCustomNotes;
}

private List<CustomNoteDetails> GetCustomNotesDetails(string pstrCustomGroup, string pstrSection)
{
    List<CustomNoteDetails> lstDetails = null;
    PLCQuery objQuery = null;
    string strQuery = string.Empty;

    // set the query
    strQuery = string.Format("SELECT STAT_CODE, DESCRIPTION, USER_RES, OPTIONS, CHECK_LIST, NOT_REQUIRED, DEFAULT_VALUE, PICTURE FROM TV_STATLIST WHERE SECTION = '{0}' AND LOWER(GROUP_RES) = '{1}' AND (UPPER(ACTIVE) <> 'F' OR ACTIVE IS NULL) ORDER BY USER_RES", pstrSection, pstrCustomGroup.ToLower());
    objQuery = new PLCQuery(strQuery);

    // do query
    objQuery.Open();
    if (objQuery.HasData())
    {
        // create a list of custom notes details
        lstDetails = new List<CustomNoteDetails>();

        while (!objQuery.EOF())
        {
            // add a new custom note detail to the list
            lstDetails.Add(new CustomNoteDetails(objQuery.FieldByName("STAT_CODE").ToString(),
                                                 objQuery.FieldByName("USER_RES").ToString(),
                                                 objQuery.FieldByName("DESCRIPTION").ToString(),
                                                 objQuery.FieldByName("OPTIONS").ToString(),
                                                 objQuery.FieldByName("CHECK_LIST").ToString(),
                                                 objQuery.FieldByName("NOT_REQUIRED").ToString() != "T",
                                                 "",
                                                 objQuery.FieldByName("DEFAULT_VALUE").ToString(),
                                                 objQuery.FieldByName("PICTURE").ToString()));

            objQuery.Next();
        }
    }

    return lstDetails;
}

public bool AssignSavedAnswer(string pstrCaseKey, string pstrExamKey)
{
    _strLoadedAnswer = GetSavedAnswer(pstrCaseKey, pstrExamKey, _strStatCode);

    return true;
}

private string GetSavedAnswer(string pstrCaseKey, string pstrExamKey, string pstrStatCode)
{
    PLCQuery objQuery = null;
    string strQuery = string.Empty;
    string strSavedAnswer = string.Empty;

    // set the query
    strQuery = string.Format("SELECT TEXT FROM TV_STATS WHERE CASE_KEY = {0} AND EXAM_KEY = {1} AND STAT_CODE = '{2}'", pstrCaseKey, pstrExamKey, pstrStatCode);
    objQuery = new PLCQuery(strQuery);

    // do query
    objQuery.Open();
    if (objQuery.HasData())
    {
        strSavedAnswer = objQuery.FieldByName("TEXT").ToString();
    }

    return strSavedAnswer;
}