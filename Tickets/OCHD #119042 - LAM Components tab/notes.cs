if ((pr.tb.Text.Trim() == "") && (pr.required == "T"))
                    {
                        var test = (pr.prompt.Trim() == "Amount");
                        var test2 = !(PLCSession.CheckUserOption("COMPEDQTY"));
                        if ((pr.prompt.Trim() == "Amount") && !(PLCSession.CheckUserOption("COMPEDQTY")))
                        {
                            continue;
                        }
                        else
                        {
                            pr.ErrMsg.Text = pr.GetRequiredMessage();
                            pr.ErrMsg.Visible = true;
                            RecordOK = false;
                        }
                    }