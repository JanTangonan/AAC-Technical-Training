DateTime dateOfBirth = DateTime.MinValue;
                        if (DateTime.TryParse(PLCDBPanel1.getpanelfield("DATE_OF_BIRTH"), out dateOfBirth))
                        {
                            DateTime offenseDate = DateTime.Parse(qryCase.FieldByName("OFFENSE_DATE"));
                            int nameAge = computeAge(offenseDate, dateOfBirth);
                            int oldestJuvenile = PLCSession.GetLabCtrl("OLDEST_JUVENILE") == "" || PLCSession.GetLabCtrl("OLDEST_JUVENILE") == "0" ? 18 : Convert.ToInt32(PLCSession.GetLabCtrl("OLDEST_JUVENILE"));
                            PLCDBPanel1.setpanelfield("JUVENILE", nameAge <= oldestJuvenile ? "T" : "F");
                        }
                        else
                            PLCDBPanel1.setpanelfield("JUVENILE", "F");


protected void PLCDBPanel1_TextChanged(object sender, PLCDBPanelTextChangedEventArgs e)
{
    switch (e.FieldName)
    {
        case "DATE_OF_BIRTH":
            if (PLCDBPanel1.HasPanelRec("TV_LABNAME", "AGE") || PLCDBPanel1.HasPanelRec("TV_LABNAME", "JUVENILE"))
            {
                string dateOfBirth = PLCDBPanel1.getpanelfield("DATE_OF_BIRTH").Replace("_", "");
                if (string.IsNullOrWhiteSpace(dateOfBirth.Replace("/", "")))
                    dateOfBirth = "";

                int computedAge = GetAgeByOffenseDate(dateOfBirth);

                if (PLCDBPanel1.HasPanelRec("TV_LABNAME", "AGE"))
                {
                    if (computedAge > 0)
                        PLCDBPanel1.setpanelfield("AGE", computedAge.ToString());
                    else
                        PLCDBPanel1.setpanelfield("AGE", null);
                }

                if (PLCDBPanel1.HasPanelRec("TV_LABNAME", "JUVENILE"))
                {
                    if (computedAge > 0)
                        PLCDBPanel1.setpanelfield("JUVENILE", computedAge <= GetOldestJuvenileAge() ? "T" : "F");
                    else
                    {
                        if (!PLCSession.GetLabCtrlFlag("ALLOW_NO_DOB_JUVENILE").Equals("T"))
                            PLCDBPanel1.setpanelfield("JUVENILE", "F");
                    }
                }
            }
            break;
        default:
            break;
    }
}

protected void PLCDBPanel1_TextChanged(object sender, PLCDBPanelTextChangedEventArgs e)
{
    if (e.FieldName == "DATE_OF_BIRTH" && PLCSession.GetLabCtrl("USES_QC_NAME_AGE") == "T")
    {
        PLCDBPanel1.setpanelfield("JUVENILE", "F");
        if (!string.IsNullOrEmpty(PLCDBPanel1.getpanelfield("DATE_OF_BIRTH")))
        {
            PLCQuery qryCase = new PLCQuery("SELECT OFFENSE_DATE FROM TV_LABCASE WHERE CASE_KEY = " + PLCSession.PLCGlobalCaseKey);
            if (qryCase.Open() && qryCase.HasData() && !string.IsNullOrEmpty(qryCase.FieldByName("OFFENSE_DATE")))
            {
                DateTime dateOfBirth = DateTime.MinValue;
                if (DateTime.TryParse(PLCDBPanel1.getpanelfield("DATE_OF_BIRTH"), out dateOfBirth))
                {
                    DateTime offenseDate = DateTime.Parse(qryCase.FieldByName("OFFENSE_DATE"));
                    int nameAge = computeAge(offenseDate, dateOfBirth);
                    int oldestJuvenile = PLCSession.GetLabCtrl("OLDEST_JUVENILE") == "" || PLCSession.GetLabCtrl("OLDEST_JUVENILE") == "0" ? 18 : Convert.ToInt32(PLCSession.GetLabCtrl("OLDEST_JUVENILE"));
                    PLCDBPanel1.setpanelfield("JUVENILE", nameAge <= oldestJuvenile ? "T" : "F");
                }
                else
                    PLCDBPanel1.setpanelfield("JUVENILE", "F");
            }
        }
    }
}

protected void PLCDBPanel1_TextChanged(object sender, PLCDBPanelTextChangedEventArgs e)
{
    if (e.FieldName != "DATE_OF_BIRTH") return;

    string dobRaw = PLCDBPanel1.getpanelfield("DATE_OF_BIRTH");
    string dateOfBirth = dobRaw?.Replace("_", "").Trim();
    bool hasDOB = !string.IsNullOrWhiteSpace(dateOfBirth?.Replace("/", ""));

    // USES_QC_NAME_AGE logic
    if (PLCSession.GetLabCtrl("USES_QC_NAME_AGE") == "T")
    {
        PLCDBPanel1.setpanelfield("JUVENILE", "F");

        if (hasDOB)
        {
            PLCQuery qryCase = new PLCQuery("SELECT OFFENSE_DATE FROM TV_LABCASE WHERE CASE_KEY = " + PLCSession.PLCGlobalCaseKey);
            if (qryCase.Open() && qryCase.HasData() && !string.IsNullOrEmpty(qryCase.FieldByName("OFFENSE_DATE")))
            {
                if (DateTime.TryParse(dobRaw, out DateTime dateOfBirthParsed))
                {
                    DateTime offenseDate = DateTime.Parse(qryCase.FieldByName("OFFENSE_DATE"));
                    int nameAge = computeAge(offenseDate, dateOfBirthParsed);
                    int oldestJuvenile = string.IsNullOrEmpty(PLCSession.GetLabCtrl("OLDEST_JUVENILE")) || PLCSession.GetLabCtrl("OLDEST_JUVENILE") == "0"
                        ? 18
                        : Convert.ToInt32(PLCSession.GetLabCtrl("OLDEST_JUVENILE"));

                    PLCDBPanel1.setpanelfield("JUVENILE", nameAge <= oldestJuvenile ? "T" : "F");
                }
                else
                {
                    PLCDBPanel1.setpanelfield("JUVENILE", "F");
                }
            }
        }
    }

    // AGE and JUVENILE fallback logic if fields exist
    if (PLCDBPanel1.HasPanelRec("TV_LABNAME", "AGE") || PLCDBPanel1.HasPanelRec("TV_LABNAME", "JUVENILE"))
    {
        int computedAge = GetAgeByOffenseDate(dateOfBirth);

        if (PLCDBPanel1.HasPanelRec("TV_LABNAME", "AGE"))
        {
            PLCDBPanel1.setpanelfield("AGE", computedAge > 0 ? computedAge.ToString() : null);
        }

        if (PLCDBPanel1.HasPanelRec("TV_LABNAME", "JUVENILE"))
        {
            if (computedAge > 0)
            {
                PLCDBPanel1.setpanelfield("JUVENILE", computedAge <= GetOldestJuvenileAge() ? "T" : "F");
            }
            else if (PLCSession.GetLabCtrlFlag("ALLOW_NO_DOB_JUVENILE") != "T")
            {
                PLCDBPanel1.setpanelfield("JUVENILE", "F");
            }
        }
    }
}