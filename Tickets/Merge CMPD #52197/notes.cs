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