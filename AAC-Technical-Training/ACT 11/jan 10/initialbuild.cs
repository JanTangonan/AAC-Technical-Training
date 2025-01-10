using PLCCONTROLS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BEASTiLIMS
{
    public partial class Exercise_Arvin_Report : PLCGlobals.PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                dbgLabCase.InitializePLCDBGrid();
                dbgLabCase.SelectedIndex = 0;
                dbgLabItem.SelectedIndex = 0;
                BindItemsGrid();

                if (dbgLabItem.Rows.Count > 0)
                {
                    btnPrintItemDetails.Enabled = true;
                }
                else
                {
                    btnPrintItemDetails.Enabled = false;
                }
            }
        }

        protected void dbgLabCase_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindItemsGrid();
            if (dbgLabItem.Rows.Count > 0)
            {
                btnPrintItemDetails.Enabled = true;
            }
            else
            {
                btnPrintItemDetails.Enabled = false;
            }
        }

        protected void dbgLabItem_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        protected void btnPrintCaseDetails_Click(object sender, EventArgs e)
        {
            PrintCaseDetails();

        }

        protected void btnPrintItemDetails_Click(object sender, EventArgs e)
        {
            PrintItemDetails();
        }

        private void BindCasesGrid()
        {
            dbgLabCase.InitializePLCDBGrid();
        }

        private void BindItemsGrid()
        {
            dbgLabItem.PLCSQLString = @"SELECT I.EVIDENCE_CONTROL_NUMBER, I.LAB_CASE_SUBMISSION, I.LAB_ITEM_NUMBER, I.ITEM_SORT,  
                                        I.ITEM_DESCRIPTION, I.ITEM_TYPE, I.PACKAGING_CODE 
                                        FROM TV_LABITEM I 
                                        INNER JOIN TV_LABCASE C ON I.CASE_KEY = C.CASE_KEY
                                        INNER JOIN TV_DEPTNAME D ON C.DEPARTMENT_CODE = D.DEPARTMENT_CODE
                                        INNER JOIN TV_OFFENSE O ON C.OFFENSE_CODE = O.OFFENSE_CODE
                                        WHERE I.CASE_KEY = " + dbgLabCase.SelectedDataKey.Value.ToString();

            dbgLabItem.InitializePLCDBGrid();
        }

        private void PrintCaseDetails()
        {
            PLCSession.PLCCrystalReportName = PLCSession.FindCrystalReport("WICK_CASE.rpt");
            PLCSession.PLCCrystalSelectionFormula = "{TV_LABITEM.CASE_KEY} = " + dbgLabCase.SelectedDataKey.Value.ToString();
            PLCSession.PLCCrystalReportTitle = "Case Details";
            PLCSession.PrintCRWReport(true);
        }

        private void PrintItemDetails()
        {
            PLCSession.PLCCrystalReportName = PLCSession.FindCrystalReport("WICK_ITEM.rpt");
            PLCSession.PLCCrystalSelectionFormula = "{TV_LABITEM.EVIDENCE_CONTROL_NUMBER} = " + dbgLabItem.SelectedDataKey.Value.ToString();
            PLCSession.PLCCrystalReportTitle = "Item Details";
            PLCSession.PrintCRWReport(true);
        }
    }
}