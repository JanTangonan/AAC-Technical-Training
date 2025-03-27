[WebMethod(EnableSession = true)]
public string RenderSortAnalysisTypes(object input)
{
    bool isVerification = Convert.ToBoolean(GetParameterValue(input, "verification"));
    string examKey = PLCSession.PLCGlobalAssignmentKey;
    string html = string.Empty;

    string verificationFilter = isVerification
        ? " AND AR.VERIFICATION_PANEL = 'T'"
        : " AND AR.VERIFICATION_PANEL <> 'T'";
    PLCQuery qryAnalysisSequences = new PLCQuery();
    qryAnalysisSequences.SQL = @"SELECT AR.ANALYSIS_TYPE_SEQUENCE, AR.ANALYSIS_TYPE_CODE, AT.DESCRIPTION, 0 AS TEMPNUM
FROM TV_ARRPTLNK AR 
INNER JOIN TV_ARTYPE AT ON AR.ANALYSIS_TYPE_CODE = AT.ANALYSIS_TYPE_CODE 
WHERE AR.EXAM_KEY = " + examKey + verificationFilter + @"
ORDER BY TEMPNUM, AR.ANALYSIS_TYPE_SEQUENCE";
    qryAnalysisSequences.Open();

    if (qryAnalysisSequences.HasData())
    {
        html = "<div style=\"overflow-y: scroll; height: 180px;\">";
        html += "<select id=\"pnlAnalysisSort\" style=\"width: 350px\" size=\"" + qryAnalysisSequences.PLCDataTable.Rows.Count + "\">";
        while (!qryAnalysisSequences.EOF())
        {
            html += "<option value=\"" + qryAnalysisSequences.FieldByName("ANALYSIS_TYPE_CODE") + "," + qryAnalysisSequences.FieldByName("ANALYSIS_TYPE_SEQUENCE") + "\">" +
                qryAnalysisSequences.FieldByName("DESCRIPTION") + "</option>";
            qryAnalysisSequences.Next();
        }
        html += "</select>";
        html += "</div>";
        html += "<center><div><input type=\"button\" id=\"btnMoveAnalysisUp\" value=\"Move Panel Up\" onclick=\"btnMoveAnalysisUp_Click()\">" +
            "<input type=\"button\" id=\"btnMoveAnalysisDown\" value=\"Move Panel Down\" onclick=\"btnMoveAnalysisDown_Click()\"></div></center>";
    }
    else
        html = "<center>No panels found.</center>";

    return html;
}