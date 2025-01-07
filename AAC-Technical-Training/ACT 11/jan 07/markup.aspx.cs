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
            GridViewRow row = dbgLabCase.SelectedRow;

            if (row.RowType == DataControlRowType.DataRow)
            {
                BindItemsGrid();
            }
        }

        protected void dbgLabItem_SelectedIndexChanged(object sender, EventArgs e)
        {

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
            dbgLabItem.PLCSQLString = @"SELECT LAB_CASE_SUBMISSION, LAB_ITEM_NUMBER, ITEM_SORT, 
                                        ITEM_DESCRIPTION, ITEM_TYPE, PACKAGING_CODE 
                                        FROM TV_LABCASE C 
                                        INNER JOIN TV_LABITEM AS I ON C.CASE_KEY = I.CASE_KEY
                                        INNER JOIN TV_DEPTNAME AS D ON C.DEPARTMENT_CODE = D.DEPARTMENT_CODE
                                        INNER JOIN TV_OFFENSE AS O ON C.OFFENSE_CODE = O.OFFENSE_CODE
                                        WHERE C.CASE_KEY = " + dbgLabCase.SelectedDataKey.Value.ToString();;

            dbgLabItem.InitializePLCDBGrid();
        }

        private void PrintCaseDetails()
        {
            PLCSession.PLCCrystalReportName = PLCSession.FindCrystalReport("WICK_CASE.rpt");
            PLCSession.PLCCrystalReportTitle = "Container Contents";
            PLCSession.PLCCrystalSelectionFormula = "{TV_LABCASE.CASE_KEY} = " + dbgLabCase.SelectedDataKey.Value.ToString();
            PLCSession.PrintCRWReport(true);
        }

        private void PrintItemDetails()
        {
            PLCSession.PLCCrystalReportName = PLCSession.FindCrystalReport("WICK_ITEM.rpt");
            PLCSession.PLCCrystalReportTitle = "Container Contents";
            PLCSession.PLCCrystalSelectionFormula = "{TV_LABITEM.CASE_KEY} = " + dbgLabItem.SelectedDataKey.Value.ToString();
            PLCSession.PrintCRWReport(true);
        }

        
    }
}