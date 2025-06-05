private void CreateDictionary()
		{
			dSearchParameters = new Dictionary<string, string>();
            PLCQuery plcParams = CacheHelper.OpenCachedSql("SELECT TABLE_NAME, FIELD_NAME, PROMPT, EDIT_MASK FROM TV_DBPANEL WHERE PANEL_NAME = '" + PANELNAME + "' AND ((REMOVE_FROM_SCREEN <> 'T') OR (REMOVE_FROM_SCREEN IS NULL)) ORDER BY SEQUENCE ASC");
			if (plcParams.HasData())
			{
				string tableName = string.Empty;
				string fieldName = string.Empty;
				string prompt = string.Empty;
				string searchValue = string.Empty;
                bool usesNotFilter = false;
                string editMask = string.Empty;
                string fieldCodeDescription = string.Empty;
                bool usesReturnToSearch = PLCSession.GetLabCtrlFlag("USES_RETURN_TO_SEARCH").Equals("T");

				DataTable parameters = plcParams.PLCDataTable;
				foreach (DataRow parameter in parameters.Rows)
				{
					tableName = parameter["TABLE_NAME"].ToString().Trim();
					fieldName = parameter["FIELD_NAME"].ToString().Trim();
					prompt = parameter["PROMPT"].ToString().Trim();
                    editMask = parameter["EDIT_MASK"].ToString().Trim();

					searchValue = dbpAssignSearch.GetFieldValue(tableName, fieldName, prompt);
					dSearchParameters.Add(prompt.Replace(" ", string.Empty), searchValue);

                    usesNotFilter = dbpAssignSearch.GetNotFilterValue(fieldName);
                    dSearchParameters.Add(prompt.Replace(" ", string.Empty) + "_notFilter", usesNotFilter ? "T" : "F");

                    if (usesReturnToSearch)
                    {
                        if (editMask.Equals("MULTIPICK") || editMask.Equals("MULTIPICK_SEARCH"))
                        {
                            string descriptionKey = prompt.Replace(" ", string.Empty) + "_DESC";
                            fieldCodeDescription = dbpAssignSearch.GetFieldDesc(tableName, fieldName);

                            if (!string.IsNullOrEmpty(fieldCodeDescription))
                                dSearchParameters.Add(descriptionKey, fieldCodeDescription);
                        }
                    }
                }

                string selectedQMSIndicatorIndex = ddlQmsIndicator.SelectedIndex.ToString();
                dSearchParameters.Add("QMS_INDICATOR", selectedQMSIndicatorIndex);
			}
			Session["AssignSearchParameters"] = dSearchParameters;
		}