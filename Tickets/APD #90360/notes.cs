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