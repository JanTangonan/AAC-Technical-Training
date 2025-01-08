using PLCCONTROLS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BEASTiLIMS
{
    public partial class Exercise_Arvin_Reports : PLCGlobals.PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                dbgLabCase.InitializePLCDBGrid();
            }
        }

        protected void dbgLabCase_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindItemsGrid();
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
            dbgLabCase.PLCSQLString = @"SELECT TOP 10 C.CASE_KEY, C.DEPARTMENT_CASE_NUMBER, 
                                        D.DEPARTMENT_NAME, O.OFFENSE_DESCRIPTION AS CHARGE, LAB_CASE, 
                                        OFFENSE_DATE 
                                        FROM TV_LABCASE C 
                                        INNER JOIN TV_DEPTNAME D ON C.DEPARTMENT_CODE = D.DEPARTMENT_CODE 
                                        INNER JOIN TV_OFFENSE O ON C.OFFENSE_CODE = O.OFFENSE_CODE 
                                        ORDER BY CASE_DATE DESC";

            dbgLabCase.InitializePLCDBGrid();
        }

        private void BindItemsGrid()
        {
            dbgLabItem.PLCSQLString = @"SELECT I.EVIDENCE_CONTROL_NUMBER, I.LAB_CASE_SUBMISSION, I.LAB_ITEM_NUMBER, I.ITEM_SORT,  
                                        I.ITEM_DESCRIPTION, I.ITEM_TYPE, I.PACKAGING_CODE 
                                        FROM TV_LABITEM AS I 
                                        INNER JOIN TV_LABCASE  AS C ON I.CASE_KEY = C.CASE_KEY
                                        INNER JOIN TV_DEPTNAME AS D ON C.DEPARTMENT_CODE = D.DEPARTMENT_CODE
                                        INNER JOIN TV_OFFENSE AS O ON C.OFFENSE_CODE = O.OFFENSE_CODE
                                        WHERE I.CASE_KEY = " + dbgLabCase.SelectedDataKey.Value.ToString();

            dbgLabItem.InitializePLCDBGrid();
        }

        private void PrintCaseDetails()
        {
            PLCSession.PLCCrystalReportName = PLCSession.FindCrystalReport("WICK_CASE.rpt");
            PLCSession.PLCCrystalSelectionFormula = "{TV_LABCASE.CASE_KEY} = " + dbgLabCase.SelectedDataKey.Value.ToString();
            PLCSession.PLCCrystalReportTitle = "Case Details";
            PLCSession.PrintCRWReport(true);
        }

        private void PrintItemDetails()
        {
            PLCSession.PLCCrystalReportName = PLCSession.FindCrystalReport("WICK_ITEM.rpt");
            PLCSession.PLCCrystalSelectionFormula = "{TV_LABITEM.CASE_KEY} = " + dbgLabItem.SelectedDataKey.Value.ToString();
            PLCSession.PLCCrystalReportTitle = "Item Details";
            PLCSession.PrintCRWReport(true);
        }

        
    }
}