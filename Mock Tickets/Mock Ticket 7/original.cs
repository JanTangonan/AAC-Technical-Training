private void GetCaseItemsJSON()
        {
            PLCQuery qryCaseItems = new PLCQuery();

            string sFields = "SELECT I.EVIDENCE_CONTROL_NUMBER, I.ITEM_DESCRIPTION, I.QUANTITY, I.LAB_ITEM_NUMBER, I.DEPARTMENT_ITEM_NUMBER, I.CUSTODY_OF, I.LOCATION, I.BARCODE, I.PARENT_ECN, I.ITEM_SORT, E.SECTION, CC.DESCRIPTION || ' - ' || CL.DESCRIPTION AS CUSTODY_DESC, L.DESCRIPTION AS ITEMTYPE_DESC, P.DESCRIPTION AS PACKAGING_DESC, C.CONTAINER_DESCRIPTION AS CONTAINER_DESC, I.ITEM_TYPE, LA.DATE_COMPLETED, I.CONTAINER_KEY, I.PROCESS, LC.DEPARTMENT_CASE_NUMBER, LC.LAB_CASE, CL.TRANSFER_RESTRICTION_TYPE, L.WEIGHT_UNIT";

            if (PLCSession.GetLabCtrl("USES_ITEM_ROUTING") == "T")
            {
                sFields += ", I.HOLD_IN_DEPARTMENT";
            }

            //Made similar as to how the gird populates in Items tab
            string sCondition = " FROM TV_LABITEM I " +
                    "LEFT OUTER JOIN TV_LABCASE LC ON I.CASE_KEY = LC.CASE_KEY " +
                    "LEFT OUTER JOIN TV_ITASSIGN A ON I.EVIDENCE_CONTROL_NUMBER = A.EVIDENCE_CONTROL_NUMBER " +
                    "LEFT OUTER JOIN TV_LABEXAM E ON A.EXAM_KEY = E.EXAM_KEY " +
                    "LEFT OUTER JOIN TV_LABASSIGN LA ON LA.EXAM_KEY = A.EXAM_KEY " +
                    "LEFT OUTER JOIN TV_CUSTCODE CC ON I.CUSTODY_OF = CC.CUSTODY_TYPE " +
                    "LEFT OUTER JOIN TV_CUSTLOC CL ON I.CUSTODY_OF = CL.CUSTODY_CODE AND I.LOCATION = CL.LOCATION " +
                    "LEFT OUTER JOIN TV_ITEMTYPE L ON I.ITEM_TYPE = L.ITEM_TYPE " +
                    "LEFT OUTER JOIN TV_PACKTYPE P ON I.PACKAGING_CODE = P.PACKAGING_CODE " +
                    "LEFT OUTER JOIN TV_CONTAINER C ON I.CONTAINER_KEY = C.CONTAINER_KEY " +
                    "WHERE I.CASE_KEY = " + PLCSession.PLCGlobalCaseKey;

           // if (PLCSession.GetLabCtrl("USES_HIDDEN_ITEMS") == "T")
           //     {
           //     sCondition += " AND I.HIDDEN_ITEM = 'F' "; //+ "AND UPPER(I.ITEM_TYPE) <> 'FILE'";
           //     }

            if (PLCSession.GetLabCtrl("SHOW_FILE_ITEM") != "T")
            {
                sCondition += " AND I.LAB_ITEM_NUMBER <> '0' "; //+ "AND UPPER(I.ITEM_TYPE) <> 'FILE'";
            }

            if (PLCSession.CheckUserOption("LIMITITEMSINTRANSFER"))
            {
                sCondition += string.Format(" AND I.CUSTODY_OF = '{0}' AND I.LOCATION = '{1}'",
                    PLCSession.PLCGlobalDefaultAnalystCustodyOf, PLCSession.PLCGlobalAnalyst);
            }

            sCondition += " ORDER BY I.ITEM_SORT";

            qryCaseItems.SQL = PLCSession.FormatSpecialFunctions(sFields + sCondition);
            qryCaseItems.Open();

            bool usesItemRouting = (PLCSession.GetLabCtrl("USES_ITEM_ROUTING") == "T");
            StringBuilder itemsJSON = new StringBuilder(string.Empty);

            Dictionary<int, string> sectionList = new Dictionary<int, string>();
            List<int> ecnList = new List<int>();
            string section = string.Empty;

            while (!qryCaseItems.EOF())
            {
                int ecn = Convert.ToInt32(qryCaseItems.FieldByName("EVIDENCE_CONTROL_NUMBER"));

                if (string.IsNullOrEmpty(qryCaseItems.FieldByName("DATE_COMPLETED")) &&
                    !string.IsNullOrEmpty(qryCaseItems.FieldByName("SECTION")))
                {
                    if (sectionList.ContainsKey(ecn))
                    {
                        section = sectionList[ecn];
                        sectionList[ecn] = section.Contains(qryCaseItems.FieldByName("SECTION").Trim())
                            ? section
                            : section + "," + qryCaseItems.FieldByName("SECTION").Trim();
                    }
                    else
                    {
                        sectionList.Add(ecn, qryCaseItems.FieldByName("SECTION").Trim());
                    }
                }


                if (!ecnList.Contains(ecn))
                {
                    ecnList.Add(ecn);

                    itemsJSON.Append(
                        "{\"EVIDENCE_CONTROL_NUMBER\":\"" + qryCaseItems.FieldByName("EVIDENCE_CONTROL_NUMBER") +
                        "\", \"ITEM_DESCRIPTION\":\"" + EscapeJSON(Server.HtmlEncode(qryCaseItems.FieldByName("ITEM_DESCRIPTION"))) +
                        "\", \"QUANTITY\":\"" + qryCaseItems.FieldByName("QUANTITY") +
                        "\", \"LAB_ITEM_NUMBER\":\"" + Server.HtmlEncode(qryCaseItems.FieldByName("LAB_ITEM_NUMBER")) +
                        "\", \"DEPARTMENT_ITEM_NUMBER\":\"" + Server.HtmlEncode(qryCaseItems.FieldByName("DEPARTMENT_ITEM_NUMBER")) +
                        "\", \"CUSTODY_OF\":\"" + Server.HtmlEncode(qryCaseItems.FieldByName("CUSTODY_OF")) +
                        "\", \"LOCATION\":\"" + Server.HtmlEncode(qryCaseItems.FieldByName("LOCATION")) +
                        "\", \"CUSTODY_DESC\":\"" + EscapeJSON(qryCaseItems.FieldByName("CUSTODY_DESC")) +
                        "\", \"ITEMTYPE_DESC\":\"" + EscapeJSON(qryCaseItems.FieldByName("ITEMTYPE_DESC")) +
                        "\", \"PACKAGING_DESC\":\"" + EscapeJSON(qryCaseItems.FieldByName("PACKAGING_DESC")) +
                        "\", \"CONTAINER_DESC\":\"" + EscapeJSON(Server.HtmlEncode(qryCaseItems.FieldByName("CONTAINER_DESC"))) +
                        "\", \"BARCODE\":\"" + qryCaseItems.FieldByName("BARCODE") +
                        "\", \"PARENT_ECN\":\"" + qryCaseItems.FieldByName("PARENT_ECN") +
                        "\", \"ITEM_TYPE\":\"" + Server.HtmlEncode(EscapeJSON(qryCaseItems.FieldByName("ITEM_TYPE"))) +
                        "\", \"HOLD_IN_DEPARTMENT\":\"" + ((usesItemRouting) ? qryCaseItems.FieldByName("HOLD_IN_DEPARTMENT") : string.Empty) +
                        "\", \"CONTAINER_KEY\":\"" + qryCaseItems.FieldByName("CONTAINER_KEY") +
                        "\", \"RESTRICTED\":\"" + qryCaseItems.FieldByName("TRANSFER_RESTRICTION_TYPE").Trim() +
                        "\", \"DEPARTMENT_CASE_NUMBER\":\"" + EscapeJSON(Server.HtmlEncode(qryCaseItems.FieldByName("DEPARTMENT_CASE_NUMBER"))) +
                        "\", \"LAB_CASE\":\"" + Server.HtmlEncode(qryCaseItems.FieldByName("LAB_CASE")) +
                        "\", \"PROCESS\":\"" + Server.HtmlEncode(qryCaseItems.FieldByName("PROCESS")) +
                        "\", \"ITEM_SORT\":\"" + qryCaseItems.FieldByName("ITEM_SORT") +
                        "\", \"WEIGHT_UNIT\":\"" + qryCaseItems.FieldByName("WEIGHT_UNIT") + "\"}, "
                    );
                }

                qryCaseItems.Next();
            }

            if (itemsJSON.Length > 0)
            {
                hdnItemsJSON.Value = "{\"ITEMS\":[" + itemsJSON.ToString().Trim().TrimEnd(',') + "]}";

                StringBuilder ecnSect = new StringBuilder(string.Empty);
                foreach (int key in sectionList.Keys)
                {
                    ecnSect.Append("{\"EVIDENCE_CONTROL_NUMBER\":\"" + key.ToString() + "\",\"SECTIONS\":\"" + sectionList[key] + "\"},");
                }

                if (ecnSect.Length > 0)
                    hdnSectionsJSON.Value = "{\"ECN_SECTIONS\":[" + ecnSect.ToString().Trim().TrimEnd(',') + "]}";
            }
            else
            {
                hdnItemsJSON.Value = string.Empty;
                hdnSectionsJSON.Value = string.Empty;
            }

        }