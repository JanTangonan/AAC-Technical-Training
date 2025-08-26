try
            {

                websvcurl = System.Configuration.ConfigurationManager.AppSettings.Get("RPTSVCURL");

                if (String.IsNullOrWhiteSpace(websvcurl))

                    {
                        websvcurl = PLCSession.GetWebConfiguration("RPTSVCURL");

                    if (websvcurl == "") websvcurl = ThisURL;
                    if (!websvcurl.EndsWith("/")) websvcurl += "/";
                    webmethods.Url = websvcurl + "PLCWebCommon/PLCWebMethods.asmx";

                    WriteDebug("Report Service URL:" + webmethods.Url.ToString(), true);


                    }
                else
                    websvcurl = websvcurl.ToUpper();
            }
            catch (Exception ex)
            {
                WriteDebug("Exception in PLCSession.GetWebConfiguration:" + ex.Message, true);
                websvcurl = "";
            }