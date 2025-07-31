if (
    PLCDBGlobal.instance.GetWebUserField("ALLOW_INQUIRY", PLCSession.PLCGlobalPrelogUser).ToUpper() != "T" || 
    PLCSession.GetDeptCtrlFlag("ALLOW_INQUIRY") != "T")
                RemoveMenuItem("Case Inquiry");
                