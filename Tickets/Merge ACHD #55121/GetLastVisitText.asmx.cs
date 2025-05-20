[WebMethod(EnableSession = true)]
public void GetLastVisitText()
{
    HttpContext.Current.Response.ClearHeaders();
    HttpContext.Current.Response.ClearContent();
    HttpContext.Current.Response.Clear();
    HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
    HttpContext.Current.Response.ContentType = "application/json";
    HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;

    string casekey = HttpContext.Current.Request["casekey"];

    Dictionary < string, object > dictLastVisitText = new Dictionary < string, object > ();
            PLCQuery qry = new PLCQuery();
    qry.SQL = "SELECT * FROM UV_QCSUBHEADING WHERE CASE_KEY = ?";
    qry.AddParameter("CASE_KEY", casekey);
    if (qry.Open() && qry.HasData()) {
        dictLastVisitText["subheading"] = Convert.ToDateTime((qry.FieldByName("SUBHEADING"))).ToShortDateString();
    }
    HttpContext.Current.Response.Write(JSONStrFromStruct(dictLastVisitText));

    HttpContext.Current.Response.Write("");
    HttpContext.Current.Response.Flush();
    HttpContext.Current.ApplicationInstance.CompleteRequest();
}

//original
[WebMethod(EnableSession = true)]
        public void GetLastVisitText()
        {
            HttpContext.Current.Response.ClearHeaders();
            HttpContext.Current.Response.ClearContent();
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            HttpContext.Current.Response.ContentType = "application/json";
            HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;

            object casekey = (object) JSONStructFromStr(HttpContext.Current.Request["casekey"]);

            Dictionary<string, object> dictLastVisitText = new Dictionary<string, object>();
            PLCQuery qry = new PLCQuery();
            qry.SQL = string.Format(@"Select * from UV_QCSUBHEADING where CASE_KEY = {0}", casekey);
            if (qry.Open() && qry.HasData())
            {
                dictLastVisitText["subheading"] = Convert.ToDateTime((qry.FieldByName("SUBHEADING"))).ToShortDateString();
            }
            HttpContext.Current.Response.Write(JSONStrFromStruct(dictLastVisitText));

            HttpContext.Current.Response.Write("");
            HttpContext.Current.Response.Flush();
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }


DateTime tempDate = new DateTime();
if (DateTime.TryParse(dateOfBirth, out tempDate))
    dateOfBirth = tempDate.ToString(PLCSession.GetDateFormat());


string dobStr = qryLiveScan.FieldByName("DOB");
    if (!string.IsNullOrEmpty(dobStr))
    {
        DateTime dtDOB = DateTime.ParseExact(dobStr, "yyyy-MM-dd", null);
        dictParams.Add("TV_LABNAME.DATE_OF_BIRTH", dtDOB.ToString(PLCSession.GetDateFormat()));
    }


try
            {
                PLCQuery qry = new PLCQuery();
                qry.SQL = "SELECT * FROM UV_QCSUBHEADING WHERE CASE_KEY = ?";
                qry.AddSQLParameter("CASE_KEY", caseKey);
                if (qry.Open() && qry.HasData())
                {
                    dictLastVisitText["subheading"] = Convert.ToDateTime((qry.FieldByName("SUBHEADING"))).ToString(PLCSession.GetDateFormat());
                }
            }