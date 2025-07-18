private int GetAgeByOffenseDate(string dateOfBirth)
{
    int result = 0;
    if (!string.IsNullOrEmpty(dateOfBirth))
    {
        PLCQuery qryCase = new PLCQuery(string.Format("SELECT OFFENSE_DATE FROM TV_LABCASE WHERE CASE_KEY = {0}", PLCSession.PLCGlobalCaseKey));
        if (qryCase.Open() && qryCase.HasData() && !string.IsNullOrEmpty(qryCase.FieldByName("OFFENSE_DATE")))
        {
            DateTime birthDate = DateTime.MinValue;
            if (DateTime.TryParse(dateOfBirth, out birthDate))
            {
                DateTime offenseDate = DateTime.Parse(qryCase.FieldByName("OFFENSE_DATE"));

                result = offenseDate.Year - birthDate.Year;

                if (offenseDate.Month < birthDate.Month || (offenseDate.Month == birthDate.Month && offenseDate.Day < birthDate.Day))
                    result--;

                if (result > 0 && birthDate > offenseDate.AddYears(-result))
                    result--;
            }
        }
    }
s
    return result;
}