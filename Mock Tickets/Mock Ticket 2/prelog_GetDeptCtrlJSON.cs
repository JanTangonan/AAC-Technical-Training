private string GetDeptCtrlJSON()
        {

            return String.Format("{{\"NAMES_PICTURE_MASK\":\"{0}\", \"DEPT_CASE_NUMBER_TEXT\": \"{1}\", \"USES_SERVICE_REQUESTS\":\"{2}\", \"ITEM_DESCRIPTION_REQUIRED\":\"{3}\", " +
                "\"HIDE_RACE\":\"{4}\", \"ALLOW_OPTIONAL_SR\":\"{5}\", \"REQUIRES_VICTIM_OR_SUSPECT\":\"{6}\", \"ITEM_NUMBER_MASK\":\"{7}\", \"SHORT_DATE_FORMAT\":\"{8}\",\"USES_IMAGE_ATTACHMENTS\":\"{9}\", " +
                "\"USES_NAME_SUFFIX\":\"{10}\", \"NO_UNDERSCORE_IN_CASE_NUMBER\": \"{11}\", \"USES_STATE_ID\": \"{12}\",\"USES_FBI_NUMBER\": \"{13}\",\"USES_DCI_NUMBER\": \"{14}\",\"USES_DISTRIBUTION\": \"{15}\", " +
                " \"USES_UNKNOWN_JUVENILE\": \"{16}\", \"UNKNOWN_JUVENILE_TEXT\": \"{17}\", \"USES_NAME_VERIFICATION\": \"{18}\", \"VERIFIED_NAME_TEXT\": \"{19}\" ,\"USES_COMMENTS\": \"{20}\",\"USES_DOC_NUMBER\": \"{21}\", " +
                "\"USES_SUBJECT_CHARGED\": \"{22}\", \"USES_TRIAL_DATE\": \"{23}\", \"USES_GRAND_JURY_DATE\": \"{24}\", \"USES_NAME_STATUS\": \"{25}\", \"USES_MEDSTATUS_OPTIONS\":\"{26}\", \"DEFAULT_ITEM_QUANTITY\" :\"{27}\", \"USES_PRELOG_QUANTITY\":\"{28}\" }}",
               PLCSessionVars1.GetDeptCtrl("NAMES_PICTURE_MASK"), PLCSessionVars1.GetDeptCtrl("DEPT_CASE_NUMBER_TEXT"), PLCSessionVars1.GetDeptCtrl("USES_SERVICE_REQUESTS"),
               PLCSessionVars1.GetDeptCtrl("ITEM_DESCRIPTION_REQUIRED"), PLCSessionVars1.GetDeptCtrl("HIDE_RACE"), PLCSessionVars1.GetDeptCtrl("ALLOW_OPTIONAL_SR"), 
               PLCSessionVars1.GetDeptCtrl("REQUIRES_VICTIM_OR_SUSPECT").Replace(" ", string.Empty), PLCSession.GetDeptCtrlFlag("ITEM_NUMBER_MASK"), PLCSession.GetDeptCtrl("SHORT_DATE_FORMAT").Trim(), 
               PLCSession.GetDeptCtrlFlag("USES_IMAGE_ATTACHMENTS"), PLCSession.GetDeptCtrlFlag("USES_NAME_SUFFIX"), PLCSession.GetDeptCtrlFlag("NO_UNDERSCORE_IN_CASE_NUMBER"), PLCSession.GetDeptCtrlFlag("USES_STATE_ID"),
               PLCSession.GetDeptCtrlFlag("USES_FBI_NUMBER"), PLCSession.GetDeptCtrlFlag("USES_DCI_NUMBER"), PLCSession.GetDeptCtrlFlag("USES_DISTRIBUTION"), PLCSession.GetDeptCtrlFlag("USES_UNKNOWN_JUVENILE"), 
               PLCSession.GetDeptCtrl("UNKNOWN_JUVENILE_TEXT"), PLCSession.GetDeptCtrlFlag("USES_NAME_VERIFICATION"), PLCSession.GetDeptCtrl("VERIFIED_NAME_TEXT"), PLCSession.GetDeptCtrlFlag("USES_COMMENTS"),
               PLCSession.GetDeptCtrlFlag("USES_DOC_NUMBER"), PLCSession.GetDeptCtrlFlag("USES_SUBJECT_CHARGED"), PLCSession.GetDeptCtrlFlag("USES_TRIAL_DATE"), PLCSession.GetDeptCtrlFlag("USES_GRAND_JURY_DATE"),
               PLCSession.GetDeptCtrlFlag("USES_NAME_STATUS"), PLCSession.GetDeptCtrlFlag("USES_MEDSTATUS_OPTIONS"), PLCSession.GetDeptCtrl("DEFAULT_ITEM_QUANTITY"), PLCSession.GetDeptCtrlFlag("USES_PRELOG_QUANTITY"));

        }