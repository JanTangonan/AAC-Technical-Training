if (
    PLCDBGlobal.instance.GetWebUserField("ALLOW_INQUIRY", PLCSession.PLCGlobalPrelogUser).ToUpper() != "T" || 
    PLCSession.GetDeptCtrlFlag("ALLOW_INQUIRY") != "T")
                RemoveMenuItem("Case Inquiry");
                
private void SetDepartmentAccess()
        {
            bool showAllDepartments = PLCDBGlobal.instance.GetWebUserField("INQUIRY_ALL_DEPTS", PLCSession.PLCGlobalPrelogUser) == "T";

            if (!showAllDepartments)
            {
                String codeCondition = "DEPARTMENT_CODE IN (" + PLCDBGlobal.instance.getUserDeptCodes() + ")";

                //string codeCondition = PLCDBGlobal.instance.AssemblePrelogDeptCodeCondition(PLCSession.PLCGlobalAnalystDepartmentCode, PLCSession.PLCGlobalPrelogUser);
                if (!string.IsNullOrEmpty(codeCondition))
                {
                    string origCodeCondition = dbpPrelogCaseSearch.GetFieldCodeCondition("DEPARTMENT_CODE");
                    if (!string.IsNullOrEmpty(origCodeCondition))
                        dbpPrelogCaseSearch.AppendPanelCodeCondition("DEPARTMENT_CODE", origCodeCondition, codeCondition);
                    else
                        dbpPrelogCaseSearch.SetPanelCodeCondition("DEPARTMENT_CODE", codeCondition);

                    dbpPrelogCaseSearch.setpanelfield("DEPARTMENT_CODE", PLCSessionVars1.PLCGlobalAnalystDepartmentCode);

                    if (!codeCondition.Contains(","))
                        dbpPrelogCaseSearch.EnablePanelField("DEPARTMENT_CODE", false);
                }
            }
        }