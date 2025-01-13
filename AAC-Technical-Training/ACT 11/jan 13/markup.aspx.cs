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
        #region EVENTS
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindCasesGrid();
                dbgLabCase.SelectedIndex = 0;

                BindItemsGrid();
                ItemDetailsGridChecker();
                dbgLabItem.SelectedIndex = 0;
            }
        }

        protected void dbgLabCase_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindItemsGrid();
            ItemDetailsGridChecker();
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

        #endregion

        #region METHODS

        /// <summary>
        /// Binds Cases Grid by initialization
        /// </summary>
        private void BindCasesGrid()
        {
            dbgLabCase.InitializePLCDBGrid();
        }

        /// <summary>
        /// Binds Items Grid based on selected Cases grid row
        /// </summary>
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

        /// <summary>
        /// Prints Cases Details based on selected Cases grid row via PDFView.aspx
        /// </summary>
        private void PrintCaseDetails()
        {
            PLCSession.PLCCrystalReportName = PLCSession.FindCrystalReport("WICK_CASE.rpt");
            PLCSession.PLCCrystalSelectionFormula = "{TV_LABITEM.CASE_KEY} = " + dbgLabCase.SelectedDataKey.Value.ToString();
            PLCSession.PLCCrystalReportTitle = "Case Details";
            PLCSession.PrintCRWReport(true);
        }

        /// <summary>
        /// Prints Items Details based on selected Items grid row via PDFView.aspx
        /// </summary>
        private void PrintItemDetails()
        {
            PLCSession.PLCCrystalReportName = PLCSession.FindCrystalReport("WICK_ITEM.rpt");
            PLCSession.PLCCrystalSelectionFormula = "{TV_LABITEM.EVIDENCE_CONTROL_NUMBER} = " + dbgLabItem.SelectedDataKey.Value.ToString();
            PLCSession.PLCCrystalReportTitle = "Item Details";
            PLCSession.PrintCRWReport(true);
        }

        /// <summary>
        /// Checks if Item Details Grid contains rows/data then enables/disables btnPrintItemDetails
        /// </summary>
        private void ItemDetailsGridChecker()
        {
            if (dbgLabItem.Rows.Count > 0)
            {
                btnPrintItemDetails.Enabled = true;
            }
            else
            {
                btnPrintItemDetails.Enabled = false;
            }
        }

        #endregion
    }
}