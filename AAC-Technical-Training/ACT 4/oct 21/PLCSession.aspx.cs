using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Security.Cryptography;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.Security.Principal;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Xml;
using PLCCONTROLS.Objects.Components;
using PLCCONTROLS.Objects.Transfer.Components;
using PLCCONTROLS.Objects.Transfer.Components.AndroidTransfer;

namespace PLCCONTROLS
{
    //For Main Menu
    [Serializable]
    public enum tExpandOnLoad
    {
        UserPref,
        Yes,
        No
    };

    [Serializable]
    public enum CaseSearchType { None, ByCase, ByName, ByReference, ByAttribute, ByTestType, CODISSearch, CustomSearch, ByRecentCases, ByMyAssignments, MEIMSSearch, HitSearch };

    [Serializable]
    public class ContainerHeader
    {
        public bool CaseContainer = false;
        public int CaseContainerCaseKey = 0;
        public bool CanMaintainThisContainer = true;
        public bool ContainerCustodyOverride = false;
        public string Custody = "";
        public string Location = "";
        public string ThisGroup = "GROUP1";
    }

    [Serializable]
    public class PLCCrystalInputParams
    {
        public string PLCCrystalReportName;
        public string PLCCrystalSelectionFormula;
        public string PLCCrystalReportTitle;
        public string PLCCrystalReportFormulaList;
        public string PLCCrystalReportComments;

        public PLCCrystalInputParams(string reportName, string selectionFormula, string reportTitle, string formulaList, string reportcomments)
        {
            this.PLCCrystalReportName = reportName;
            this.PLCCrystalSelectionFormula = selectionFormula;
            this.PLCCrystalReportTitle = reportTitle;
            this.PLCCrystalReportFormulaList = formulaList;
            this.PLCCrystalReportComments = reportcomments;
        }
    }

    [Serializable]
    public class PLCSession
    {
        private static PLCSessionVars instance = null;

        private static PLCSessionVars GetInstance()
        {
            // Create PLCSessionVars instance if it hasn't been instantiated yet.
            if (PLCSession.instance == null)
                PLCSession.instance = new PLCSessionVars();

            return PLCSession.instance;
        }

        public PLCSession()
        {
            throw new ApplicationException("No need to instantiate PLCSessionVarsIntance. Just call its static methods directly.");
        }

        //--- Properties ---
        public static string CurrentRev
        {
            get { return PLCSessionVars.CurrentRev; }
        }

        public static string ClientID
        {
            get { return GetInstance().ClientID; }
        }

        public static string PLCGlobalUserHostAddress
        {
            get { return GetInstance().PLCGlobalUserHostAddress; }
            set { GetInstance().PLCGlobalUserHostAddress = value; }
        }

        public static HttpApplicationState TheApplication
        {
            get { return GetInstance().TheApplication(); }
        }

        public static Boolean PLCQueryLog
        {
            get { return GetInstance().PLCQueryLog; }
            set { GetInstance().PLCQueryLog = value; }
        }

        public static OleDbConnection SessionConnection
        {
            get { return GetInstance().SessionConnection; }
            set { GetInstance().SessionConnection = value; }
        }

        public static string ConnectionString
        {
            get { return GetInstance().ConnectionString; }
            set { GetInstance().ConnectionString = value; }
        }

        public static string PLCDBProvider
        {
            get { return GetInstance().PLCDBProvider; }
            set { GetInstance().PLCDBProvider = value; }
        }

        public static string PLCBEASTiLIMSVersion
        {
            get { return GetInstance().PLCBEASTiLIMSVersion; }
            set { GetInstance().PLCBEASTiLIMSVersion = value; }
        }

        public static string PLCCrystalReportName
        {
            get { return GetInstance().PLCCrystalReportName; }
            set { GetInstance().PLCCrystalReportName = value; }
        }

        public static string PLCCrystalReportComments
        {
            get { return GetInstance().PLCCrystalReportComments; }
            set { GetInstance().PLCCrystalReportComments = value; }
        }

        public static Boolean PLCCrystalReportAutoPrint
        {
            get { return GetInstance().PLCCrystalReportAutoPrint; }
            set { GetInstance().PLCCrystalReportAutoPrint = value; }
        }

        public static string PLCCrystalReportLabelPrinter
        {
            get { return GetInstance().PLCCrystalReportLabelPrinter; }
            set { GetInstance().PLCCrystalReportLabelPrinter = value; }
        }

        public static string PLCCrystalReportFormulaList
        {
            get { return GetInstance().PLCCrystalReportFormulaList; }
            set { GetInstance().PLCCrystalReportFormulaList = value; }
        }

        public static string PLCCrystalCustomReportKey
        {
            get { return GetInstance().PLCCrystalCustomReportKey; }
            set { GetInstance().PLCCrystalCustomReportKey = value; }
        }

        public static string PLCErrorMessage
        {
            get { return GetInstance().PLCErrorMessage; }
            set { GetInstance().PLCErrorMessage = value; }
        }

        public static string PLCErrorSQL
        {
            get { return GetInstance().PLCErrorSQL; }
            set { GetInstance().PLCErrorSQL = value; }
        }

        public static string PLCErrorURL
        {
            get { return GetInstance().PLCErrorURL; }
            set { GetInstance().PLCErrorURL = value; }
        }

        public static string PLCErrorProc
        {
            get { return GetInstance().PLCErrorProc; }
            set { GetInstance().PLCErrorProc = value; }
        }

        public static string PLCCrystalReportTitle
        {
            get { return GetInstance().PLCCrystalReportTitle; }
            set { GetInstance().PLCCrystalReportTitle = value; }
        }

        public static string PLCCrystalSelectionFormula
        {
            get { return GetInstance().PLCCrystalSelectionFormula; }
            set { GetInstance().PLCCrystalSelectionFormula = value; }
        }

        public static string PLCDBDataSource
        {
            get { return GetInstance().PLCDBDataSource; }
            set { GetInstance().PLCDBDataSource = value; }
        }

        public static string PLCDBUserID
        {
            get { return GetInstance().PLCDBUserID; }
            set { GetInstance().PLCDBUserID = value; }
        }

        public static string PLCDBDatabase
        {
            get { return GetInstance().PLCDBDatabase; }
            set { GetInstance().PLCDBDatabase = value; }
        }

        public static string PLCTransferLoc
        {
            get { return GetInstance().PLCTransferLoc; }
            set { GetInstance().PLCTransferLoc = value; }
        }

        public static string PLCTransferECN
        {
            get { return GetInstance().PLCTransferECN; }
            set { GetInstance().PLCTransferECN = value; }
        }

        public static string PLCTransferItems
        {
            get { return GetInstance().PLCTransferItems; }
            set { GetInstance().PLCTransferItems = value; }
        }

        public static string PLCDBPW
        {
            get { return GetInstance().PLCDBPW; }
            set { GetInstance().PLCDBPW = value; }
        }

        public static string PLCDBName
        {
            get { return GetInstance().PLCDBName; }
            set { GetInstance().PLCDBName = value; }
        }

        public static string PLCDomainName
        {
            get { return GetInstance().PLCDomainName; }
            set { GetInstance().PLCDomainName = value; }
        }

        public static string PLCLoginMode
        {
            get { return GetInstance().PLCLoginMode; }
            set { GetInstance().PLCLoginMode = value; }
        }

        #region Prelog Global Session Variables

        public static string PLCGlobalPrelogUser
        {
            get { return GetInstance().PLCGlobalPrelogUser; }
            set { GetInstance().PLCGlobalPrelogUser = value; }
        }

        public static string PLCGlobalPrelogDepartmentCode
        {
            get { return GetInstance().PLCGlobalPrelogDepartmentCode; }
            set { GetInstance().PLCGlobalPrelogDepartmentCode = value; }
        }

        public static string PLCGlobalPrelogDepartmentCaseNumber
        {
            get { return GetInstance().PLCGlobalPrelogDepartmentCaseNumber; }
            set { GetInstance().PLCGlobalPrelogDepartmentCaseNumber = value; }
        }

        public static string PLCGlobalPrelogCaseKey
        {
            get { return GetInstance().PLCGlobalPrelogCaseKey; }
            set { GetInstance().PLCGlobalPrelogCaseKey = value; }
        }

        public static string PLCGlobalPrelogSubmissionNumber
        {
            get { return GetInstance().PLCGlobalPrelogSubmissionNumber; }
            set { GetInstance().PLCGlobalPrelogSubmissionNumber = value; }
        }

        public static string PLCGlobalPrelogMasterKey
        {
            get { return GetInstance().PLCGlobalPrelogMasterKey; }
            set { GetInstance().PLCGlobalPrelogMasterKey = value; }
        }

        public static string PLCPrelogCaseBarcode
        {
            get { return GetInstance().PLCPrelogCaseBarcode; }
            set { GetInstance().PLCPrelogCaseBarcode = value; }
        }

        public static string PLCProficiencyTestKey
        {
            get { return GetInstance().PLCProficiencyTestKey; }
            set { GetInstance().PLCProficiencyTestKey = value; }
        }

        #endregion

        public static string PLCDomainPrefix
        {
            get { return GetInstance().PLCDomainPrefix; }
            set { GetInstance().PLCDomainPrefix = value; }
        }

        public static string PLCDomainUserName
        {
            get { return GetInstance().PLCDomainUserName; }
            set { GetInstance().PLCDomainUserName = value; }
        }

        public static string PLCGlobalAnalyst
        {
            get { return GetInstance().PLCGlobalAnalyst; }
            set { GetInstance().PLCGlobalAnalyst = value; }
        }

        public static string PLCGlobalAnalystPassword
        {
            get { return GetInstance().PLCGlobalAnalystPassword; }
            set { GetInstance().PLCGlobalAnalystPassword = value; }
        }

        public static string PLCGlobalAnalystGroup
        {
            get { return GetInstance().PLCGlobalAnalystGroup; }
            set { GetInstance().PLCGlobalAnalystGroup = value; }
        }

        public static string PLCGlobalDefaultAnalystCustodyOf
        {
            get { return GetInstance().PLCGlobalDefaultAnalystCustodyOf; }
            set { GetInstance().PLCGlobalDefaultAnalystCustodyOf = value; }
        }

        public static string PLCGlobalAnalystName
        {
            get { return GetInstance().PLCGlobalAnalystName; }
            set { GetInstance().PLCGlobalAnalystName = value; }
        }

        public static string PLCGlobalAnalystDepartmentCode
        {
            get { return GetInstance().PLCGlobalAnalystDepartmentCode; }
            set { GetInstance().PLCGlobalAnalystDepartmentCode = value; }
        }

        public static Boolean PLCGlobalProficiencyCaseNoItems
        {
            get { return GetInstance().PLCGlobalProficiencyCaseNoItems; }
            set { GetInstance().PLCGlobalProficiencyCaseNoItems = value; }
        }

        public static string PLCQCDepartmentCode
        {
            get { return GetInstance().PLCQCDepartmentCode; }
            set { GetInstance().PLCQCDepartmentCode = value; }
        }

        public static string PLCGlobalAnalystDepartmentName
        {
            get { return GetInstance().PLCGlobalAnalystDepartmentName; }
            set { GetInstance().PLCGlobalAnalystDepartmentName = value; }
        }

        public static string PLCGlobalLabCode
        {
            get { return GetInstance().PLCGlobalLabCode; }
            set { GetInstance().PLCGlobalLabCode = value; }
        }

        public static string PLCGlobalLabName
        {
            get { return GetInstance().PLCGlobalLabName; }
            set { GetInstance().PLCGlobalLabName = value; }
        }

        public static string PLCGlobalURN
        {
            get { return GetInstance().PLCGlobalURN; }
            set { GetInstance().PLCGlobalURN = value; }
        }

        public static string PLCGlobalSelectedGrid
        {
            get { return GetInstance().PLCGlobalSelectedGrid; }
            set { GetInstance().PLCGlobalSelectedGrid = value; }
        }

        public static string PLCGlobalSelectedGridIdx
        {
            get { return GetInstance().PLCGlobalSelectedGridIdx; }
            set { GetInstance().PLCGlobalSelectedGridIdx = value; }
        }

        public static string PLCGlobalCaseKey
        {
            get { return GetInstance().PLCGlobalCaseKey; }
            set { GetInstance().PLCGlobalCaseKey = value; }
        }

        public static string PLCWebOCXSource
        {
            get { return GetInstance().PLCWebOCXSource; }
            set { GetInstance().PLCWebOCXSource = value; }
        }

        public static string PLCWebOCXWorksheetSource
        {
            get { return GetInstance().PLCWebOCXWorksheetSource; }
            set { GetInstance().PLCWebOCXWorksheetSource = value; }
        }

        public static string PLCSelectedTemplateKey
        {
            get { return GetInstance().PLCSelectedTemplateKey; }
            set { GetInstance().PLCSelectedTemplateKey = value; }
        }

        public static string PLCSelectedWorksheetKey
        {
            get { return GetInstance().PLCSelectedWorksheetKey; }
            set { GetInstance().PLCSelectedWorksheetKey = value; }
        }

        public static TextBox MyTextBox
        {
            get { return GetInstance().MyTextBox; }
            set { GetInstance().MyTextBox = value; }
        }

        public static string PLCGlobalScannedContainerKey
        {
            get { return GetInstance().PLCGlobalScannedContainerKey; }
            set { GetInstance().PLCGlobalScannedContainerKey = value; }
        }

        public static string PLCGlobalSubmissionKey
        {
            get { return GetInstance().PLCGlobalSubmissionKey; }
            set { GetInstance().PLCGlobalSubmissionKey = value; }
        }

        public static string PLCGlobalECN
        {
            get { return GetInstance().PLCGlobalECN; }
            set { GetInstance().PLCGlobalECN = value; }
        }

        public static string PLCGlobalTaskID
        {
            get { return GetInstance().PLCGlobalTaskID; }
            set { GetInstance().PLCGlobalTaskID = value; }
        }

        public static string PLCSelectedBulkContainerKey
        {
            get { return GetInstance().PLCSelectedBulkContainerKey; }
            set { GetInstance().PLCSelectedBulkContainerKey = value; }
        }

        public static TransferContainerBox CurrentTransferContainerBox
        {
            get { return GetInstance().CurrentTransferContainerBox; }
            set { GetInstance().CurrentTransferContainerBox = value; }
        }

        public static string PLCGlobalNameKey
        {
            get { return GetInstance().PLCGlobalNameKey; }
            set { GetInstance().PLCGlobalNameKey = value; }
        }

        public static string PLCGlobalAnalystSearchKey
        {
            get { return GetInstance().PLCGlobalAnalystSearchKey; }
            set { GetInstance().PLCGlobalAnalystSearchKey = value; }
        }

        public static string PLCDBGridEditorSQLString
        {
            get { return GetInstance().PLCDBGridEditorSQLString; }
            set { GetInstance().PLCDBGridEditorSQLString = value; }
        }

        public static string PLCGlobalNameNumber
        {
            get { return GetInstance().PLCGlobalNameNumber; }
            set { GetInstance().PLCGlobalNameNumber = value; }
        }

        public static string PLCGlobalStatusKey
        {
            get { return GetInstance().PLCGlobalStatusKey; }
            set { GetInstance().PLCGlobalStatusKey = value; }
        }

        public static string PLCGlobalBatchKey
        {
            get { return GetInstance().PLCGlobalBatchKey; }
            set { GetInstance().PLCGlobalBatchKey = value; }
        }

        public static string PLCGlobalAttachmentSource
        {
            get { return GetInstance().PLCGlobalAttachmentSource; }
            set { GetInstance().PLCGlobalAttachmentSource = value; }
        }

        public static string PLCGlobalAttachmentSourceDesc
        {
            get { return GetInstance().PLCGlobalAttachmentSourceDesc; }
            set { GetInstance().PLCGlobalAttachmentSourceDesc = value; }
        }

        public static string PLCGlobalAssignmentKey
        {
            get { return GetInstance().PLCGlobalAssignmentKey; }
            set { GetInstance().PLCGlobalAssignmentKey = value; }
        }

        public static string PLCGlobalAttachmentKey
        {
            get { return GetInstance().PLCGlobalAttachmentKey; }
            set { GetInstance().PLCGlobalAttachmentKey = value; }
        }

        public static string PLCGlobalAssignmentBatchKey
        {
            get { return GetInstance().PLCGlobalAssignmentBatchKey; }
            set { GetInstance().PLCGlobalAssignmentBatchKey = value; }
        }

        public static string PLCGlobalLabCase
        {
            get { return GetInstance().PLCGlobalLabCase; }
            set { GetInstance().PLCGlobalLabCase = value; }
        }

        public static string PLCGlobalDepartmentCaseNumber
        {
            get { return GetInstance().PLCGlobalDepartmentCaseNumber; }
            set { GetInstance().PLCGlobalDepartmentCaseNumber = value; }
        }

        public static string PLCGlobalKitCustody
        {
            get
            {
                return GetInstance().PLCGlobalKitCustody;
            }
        }

        public static string PLCAdditionalSubmissionKey
        {
            get { return GetInstance().PLCAdditionalSubmissionKey; }
            set { GetInstance().PLCAdditionalSubmissionKey = value; }
        }

        public static string PLCNewCaseKey
        {
            get { return GetInstance().PLCNewCaseKey; }
            set { GetInstance().PLCNewCaseKey = value; }
        }

        public static string PLCNewLastName
        {
            get { return GetInstance().PLCNewLastName; }
            set { GetInstance().PLCNewLastName = value; }
        }

        public static string PLCNewFirstName
        {
            get { return GetInstance().PLCNewFirstName; }
            set { GetInstance().PLCNewFirstName = value; }
        }

        public static string PLCGlobalSubmissionNumber
        {
            get { return GetInstance().PLCGlobalSubmissionNumber; }
            set { GetInstance().PLCGlobalSubmissionNumber = value; }
        }

        public static string PLCGlobalCourierStat
        {
            get { return GetInstance().PLCGlobalCourierStat; }
            set { GetInstance().PLCGlobalCourierStat = value; }
        }

        public static string PLCGlobalCourierSQL
        {
            get { return GetInstance().PLCGlobalCourierSQL; }
            set { GetInstance().PLCGlobalCourierSQL = value; }
        }

        public static string PLCNewItem
        {
            get { return GetInstance().PLCNewItem; }
            set { GetInstance().PLCNewItem = value; }
        }

        public static DataSet PLCDataSet
        {
            get { return GetInstance().PLCDataSet; }
            set { GetInstance().PLCDataSet = value; }
        }

        public static DataSet PLCQCDataSet
        {
            get { return GetInstance().PLCQCDataSet; }
            set { GetInstance().PLCQCDataSet = value; }
        }

        public static DataSet PLCGeneralDataSet
        {
            get { return GetInstance().PLCGeneralDataSet; }
            set { GetInstance().PLCGeneralDataSet = value; }
        }

        public static DataSet PLCDataSet2
        {
            get { return GetInstance().PLCDataSet2; }
            set { GetInstance().PLCDataSet2 = value; }
        }

        public static DataSet PLCDBGridDataSet
        {
            get { return GetInstance().PLCDBGridDataSet; }
            set { GetInstance().PLCDBGridDataSet = value; }
        }

        public static string PLCGloablCaseRefKey
        {
            get { return GetInstance().PLCGloablCaseRefKey; }
            set { GetInstance().PLCGloablCaseRefKey = value; }
        }

        public static string PLCGlobalScheduleKey
        {
            get { return GetInstance().PLCGlobalScheduleKey; }
            set { GetInstance().PLCGlobalScheduleKey = value; }
        }

        public static string TempString
        {
            get { return GetInstance().TempString; }
            set { GetInstance().TempString = value; }
        }

        public static AndroidTransferConnector AndroidTransferConnector
        {
            get { return GetInstance().CurrentAndroidTransferConnector; }
            set { GetInstance().CurrentAndroidTransferConnector = value; }
        }


        public static string WhatIsNext
        {
            get { return GetInstance().WhatIsNext; }
            set { GetInstance().WhatIsNext = value; }
        }

        public static string TransferCustody
        {
            get { return GetInstance().TransferCustody; }
            set { GetInstance().TransferCustody = value; }
        }

        public static string TransferLocation
        {
            get { return GetInstance().TransferLocation; }
            set { GetInstance().TransferLocation = value; }
        }

        public static string ReportStart
        {
            get { return GetInstance().ReportStart; }
            set { GetInstance().ReportStart = value; }
        }

        public static bool IsNYPrintDeputyRequest
        {
            get { return GetInstance().IsNYPrintDeputyRequest; }
            set { GetInstance().IsNYPrintDeputyRequest = value; }
        }

        public static string PLCRPTFilePath
        {
            get { return GetInstance().PLCRPTFilePath; }
            set { GetInstance().PLCRPTFilePath = value; }
        }

        public static string PLCLBLFilePath
        {
            get { return GetInstance().PLCLBLFilePath; }
            set { GetInstance().PLCLBLFilePath = value; }
        }

        public static ContainerHeader PLCContHeaderInfo
        {
            get { return GetInstance().PLCContHeaderInfo; }
            set { GetInstance().PLCContHeaderInfo = value; }
        }

        public static string PLCDatabaseServer
        {
            get { return GetInstance().PLCDatabaseServer; }
            set { GetInstance().PLCDatabaseServer = value; }
        }

        public static string PLCServerIP
        {
            get { return GetInstance().PLCServerIP; }
            set { GetInstance().PLCServerIP = value; }
        }

        public static string PLCActiveApp
        {
            get { return GetInstance().PLCActiveApp; }
            set { GetInstance().PLCActiveApp = value; }
        }

        public static string PLCDupedRecoveryLocationInCase
        {
            get { return GetInstance().PLCDupedRecoveryLocationInCase; }
            set { GetInstance().PLCDupedRecoveryLocationInCase = value; }
        }

        public static string PLCDupedRecoveryAddressKeyInCase
        {
            get { return GetInstance().PLCDupedRecoveryAddressKeyInCase; }
            set { GetInstance().PLCDupedRecoveryAddressKeyInCase = value; }
        }

        public static string PLCDupedCollectedBy
        {
            get { return GetInstance().PLCDupedCollectedBy; }
            set { GetInstance().PLCDupedCollectedBy = value; }
        }

        public static string PLCDupedCollectedDate
        {
            get { return GetInstance().PLCDupedCollectedDate; }
            set { GetInstance().PLCDupedCollectedDate = value; }
        }

        public static string PLCDupedTimeCollected
        {
            get { return GetInstance().PLCDupedTimeCollected; }
            set { GetInstance().PLCDupedTimeCollected = value; }
        }

        public static string PLCGlobalSignaturePad
        {
            get { return GetInstance().PLCGlobalSignaturePad; }
            set { GetInstance().PLCGlobalSignaturePad = value; }
        }

        public static string PLCGlobalSignaturePadInstalled
        {
            get { return GetInstance().PLCGlobalSignaturePadInstalled; }
            set { GetInstance().PLCGlobalSignaturePadInstalled = value; }
        }

        public static string PLCGlobalSRMasterKey
        {
            get { return GetInstance().PLCGlobalSRMasterKey; }
            set { GetInstance().PLCGlobalSRMasterKey = value; }
        }

        public static string PLCGlobalAttachmentSourceKey
        {
            get { return GetInstance().PLCGlobalAttachmentSourceKey; }
            set { GetInstance().PLCGlobalAttachmentSourceKey = value; }
        }

        public static CaseSearchType PLCCaseSearchType
        {
            get { return GetInstance().PLCCaseSearchType; }
            set { GetInstance().PLCCaseSearchType = value; }
        }

        public static string PLCCaseSearchSelect
        {
            get { return GetInstance().PLCCaseSearchSelect; }
            set { GetInstance().PLCCaseSearchSelect = value; }
        }

        public static string PLCCaseSearchFrom
        {
            get { return GetInstance().PLCCaseSearchFrom; }
            set { GetInstance().PLCCaseSearchFrom = value; }
        }

        public static string PLCCaseSearchWhere
        {
            get { return GetInstance().PLCCaseSearchWhere; }
            set { GetInstance().PLCCaseSearchWhere = value; }
        }

        public static string PLCAssignSearchSelect
        {
            get { return GetInstance().PLCAssignSearchSelect; }
            set { GetInstance().PLCAssignSearchSelect = value; }
        }

        public static string PLCAssignSearchFrom
        {
            get { return GetInstance().PLCAssignSearchFrom; }
            set { GetInstance().PLCAssignSearchFrom = value; }
        }

        public static string PLCAssignSearchWhere
        {
            get { return GetInstance().PLCAssignSearchWhere; }
            set { GetInstance().PLCAssignSearchWhere = value; }
        }

        public static string PLCReportSource
        {
            get { return GetInstance().PLCReportSource; }
            set { GetInstance().PLCReportSource = value; }
        }

        public static PLCCrystalInputParams[] PLCCrystalInputs
        {
            get { return GetInstance().PLCCrystalInputs; }
            set { GetInstance().PLCCrystalInputs = value; }
        }

        public static string PLCRVWorkstationIDs
        {
            get { return GetInstance().PLCRVWorkstationIDs; }
            set { GetInstance().PLCRVWorkstationIDs = value; }
        }

        public static string PLCGlobalTableKey
        {
            get { return GetInstance().PLCGlobalTableKey; }
            set { GetInstance().PLCGlobalTableKey = value; }
        }

        public static string PLCNotesSequence
        {
            get { return GetInstance().PLCNotesSequence; }
            set { GetInstance().PLCNotesSequence = value; }
        }

        public static string PLCCrystalParameterFields
        {
            get { return GetInstance().PLCCrystalParameterFields; }
            set { GetInstance().PLCCrystalParameterFields = value; }
        }

        public static string PLCBatchTaskID
        {
            get { return GetInstance().PLCBatchTaskID; }
            set { GetInstance().PLCBatchTaskID = value; }
        }

        public static string PLCGlobalMatrixEditable
        {
            get { return GetInstance().PLCGlobalMatrixEditable; }
            set { GetInstance().PLCGlobalMatrixEditable = value; }
        }

        public static string PLCCODNAPrelogSequence
        {
            get { return GetInstance().PLCCODNAPrelogSequence; }
            set { GetInstance().PLCCODNAPrelogSequence = value; }
        }

        public static string PLCGlobalUsesDBTextResources
        {
            get { return GetInstance().PLCGlobalUsesDBTextResources; }
            set { GetInstance().PLCGlobalUsesDBTextResources = value; }
        }

        public static string PLCGlobalPrelogUserIsAdmin
        {
            get { return GetInstance().PLCGlobalPrelogUserIsAdmin; }
            set { GetInstance().PLCGlobalPrelogUserIsAdmin = value; }
        }

        public static string PLCGlobalConfigSourceKey1
        {
            get { return GetInstance().PLCGlobalConfigSourceKey1; }
            set { GetInstance().PLCGlobalConfigSourceKey1 = value; }
        }

        public static string PLCGlobalConfigSourceKey2
        {
            get { return GetInstance().PLCGlobalConfigSourceKey2; }
            set { GetInstance().PLCGlobalConfigSourceKey2 = value; }
        }

        public static void SetConfigFileSourceKeys(string fileSourceKey1, string fileSourceKey2)
        {
            GetInstance().SetConfigFileSourceKeys(fileSourceKey1, fileSourceKey2);
        }

        public static string FixedSQLStr(string SQLStr)
        {
            return GetInstance().FixedSQLStr(SQLStr);
        }

        public static OleDbConnection GetDBConn(String cstr)
        {
            return GetInstance().GetDBConn(cstr);
        }

        public static OleDbConnection GetDBConn()
        {
            return GetInstance().GetDBConn();
        }

        public static int GetKeyValueFromBarCode(string barcode)
        {
            return GetInstance().GetKeyValueFromBarCode(barcode);
        }

        public static Boolean AutoApproveServiceRequest(string scode)
        {
            return GetInstance().AutoApproveServiceRequest(scode);
        }

        public static void PrintCRWReport()
        {
            GetInstance().PrintCRWReport();
        }

        public static void PrintCRWReport(Boolean IsCaseMaster)
        {
            GetInstance().PrintCRWReport(IsCaseMaster);
        }

        public static void PrintCRWReport(int crystalInputIndex)
        {
            GetInstance().PrintCRWReport(crystalInputIndex);
        }

        public static void PrintCRWReportFromPopup()
        {
            GetInstance().PrintCRWReportFromPopup();
        }

        public static void PrintPDFData(int dataKey)
        {
            GetInstance().PrintPDFData(dataKey);
        }

        public static void LoadUserPrefs()
        {
            GetInstance().LoadUserPrefs();
        }

        public static string GetUserPref(string prefCode)
        {
            return GetInstance().GetUserPref(prefCode);
        }

        public static void SetUserPref(string prefCode, string prefValue, bool bypassPageCheck = false)
        {
            GetInstance().SetUserPref(prefCode, prefValue, bypassPageCheck);
        }

        public static void UpdateUserPrefSession(string prefCode, string prefValue)
        {
            GetInstance().UpdateUserPrefSession(prefCode, prefValue);
        }

        public static void ClearTransferInfo()
        {
            GetInstance().ClearTransferInfo();
        }

        public static bool SectionRequiresAnalyst(string ExamCode)
        {
            return GetInstance().SectionRequiresAnalyst(ExamCode);
        }

        public static string GetSectionFlag(string examCode, string flagName)
        {
            return GetInstance().GetSectionFlag(examCode, flagName);
        }

        public static bool CheckSectionFlag(string examCode, string flagName)
        {
            return GetInstance().CheckSectionFlag(examCode, flagName);
        }

        public static bool UpdateGroupMembership(string UserID)
        {
            return GetInstance().UpdateGroupMembership(UserID);
        }

        public static bool ValidatePassword(string UserID, string UserPassword, out string ErrMsg, bool allowPin = false)
        {
            return GetInstance().ValidatePassword(UserID, UserPassword, out ErrMsg, allowPin);
        }

        public static string GetSSOUserName(string analyst)
        {
            return GetInstance().GetSSOUserName(analyst);
        }

        public static string GetSSOAnalyst(string userName)
        {
            return GetInstance().GetSSOAnalyst(userName);
        }

        public static string GetCodeDesc(string codetype, string codevalue)
        {
            return GetInstance().GetCodeDesc(codetype, codevalue);
        }

        public static string GetCustodyDesc(string custcode, string custloc)
        {
            return GetInstance().GetCustodyDesc(custcode, custloc);
        }

        public static bool CodeValid(string codetype, string codevalue)
        {
            return GetInstance().CodeValid(codetype, codevalue);
        }

        public static void SetUserMessage(string s, Boolean error)
        {
            GetInstance().SetUserMessage(s, error);
        }

        public static Boolean SetupGridView(GridView gv)
        {
            return GetInstance().SetupGridView(gv);
        }

        public static void AddToRecentCases(string CaseKey, string Analyst)
        {
            GetInstance().AddToRecentCases(CaseKey, Analyst);
        }

        public static void AddToRecentCODNASamples(string Sequence, string UserID)
        {
            GetInstance().AddToRecentCODNASamples(Sequence, UserID);
        }

        public static void AddToRecentPrelogCases(string departmentCaseNumber, string departmentCode, string webUser)
        {
            GetInstance().AddToPrelogRecentCases(departmentCaseNumber, departmentCode, webUser);
        }

        public static string GetTruncatedCaseNumber(string Picture, string CaseNumber)
        {
            return GetInstance().GetTruncatedCaseNumber(Picture, CaseNumber);
        }

        public static bool CheckUserOption(string Option)
        {
            return GetInstance().CheckUserOption(Option);
        }

        public static bool CheckPanelUserOption(string panelName, string panelNameField)
        {
            return GetInstance().CheckPanelUserOption(panelName, panelNameField);
        }

        public static Boolean isInvalidTableName(String tn)
        {
            return GetInstance().isInvalidTableName(tn);
        }

        public static List<String> sqlInjWords()
        {
            return GetInstance().sqlInjWords();
        }

        public static List<String> sqlInjFilters()
        {
            return GetInstance().sqlInjFilters();
        }

        public static Boolean SQLInjectionSuspected(String sqlStr)
        {
            return GetInstance().SQLInjectionSuspected(sqlStr);
        }

        public static void sqlInjAddValidHash(String hashStr)
        {

            GetInstance().sqlInjAddValidHash(hashStr);
        }

        public static Boolean sqlInjIsValidHash(String hashStr)
        {

            return GetInstance().sqlInjIsValidHash(hashStr);

        }

        public static bool CheckUserOption(string analyst, string option)
        {
            return GetInstance().CheckUserOption(analyst, option);
        }

        public static object GetUserAnalSect(string section, string setting)
        {
            return GetInstance().GetUserAnalSect(section, setting);
        }

        /// <summary>
        /// Get Uppercased AnalSect Flag value
        /// </summary>
        /// <param name="section"></param>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static string GetAnalSectFlag(string section, string setting)
        {
            return GetInstance().GetAnalSectFlag(section, setting);
        }

        /// <summary>
        /// Check if AnalSect flag is T
        /// </summary>
        /// <param name="section"></param>
        /// <param name="setting"></param>
        /// <returns>true if flag value is T</returns>
        public static bool CheckAnalSectFlag(string section, string setting)
        {
            return GetInstance().CheckAnalSectFlag(section, setting);
        }

        public static string GetUserAnalSect(string analyst, string section, string setting)
        {
            return GetInstance().GetUserAnalSect(analyst, section, setting);
        }

        public static bool GetHasUserAnalSect(string setting)
        {
            return GetInstance().GetHasUserAnalSect(setting);
        }

        public static void ClearError()
        {
            GetInstance().ClearError();
        }

        public static void Redirect(string URL)
        {
            GetInstance().Redirect(URL);
        }

        public static string GetPLCErrorText(Boolean UseHTML)
        {
            return GetInstance().GetPLCErrorText(UseHTML);
        }

        public static void SaveErrorToDB()
        {
            GetInstance().SaveErrorToDB();
        }

        public static void SaveError()
        {
            GetInstance().SaveError();
        }

        public static void SetLabCaseVars(string TheCaseKey)
        {
            GetInstance().SetLabCaseVars(TheCaseKey);
        }

        public static void SetCaseVariables(string caseKey)
        {
            GetInstance().SetCaseVariables(caseKey);
        }

        public static void SetCaseVariables(string caseKey, string urn, string labCase)
        {
            GetInstance().SetCaseVariables(caseKey, urn, labCase);
        }

        public static void ClearLabCaseVars()
        {
            GetInstance().ClearLabCaseVars();
        }

        public static void ClearSessionVars(HttpServerUtility server)
        {
            GetInstance().ClearSessionVars(server);
        }

        public static string GetCaseTitle()
        {
            return GetInstance().GetCaseTitle();
        }

        public static string GetTableOwnerforCrystal(string tblname)
        {
            return GetInstance().GetTableOwnerforCrystal(tblname);
        }

        public static string GetConnectionString()
        {
            return GetInstance().GetConnectionString();
        }

        public static string GetCINFOConnectionString(string CINFO, out string dbServer)
        {
            return GetInstance().GetCINFOConnectionString(CINFO, out dbServer);
        }

        public static string GetConnectionString(string dbnameOverride, string datasourceOverride)
        {
            return GetInstance().GetConnectionString(dbnameOverride, datasourceOverride);
        }

        public static string GetOracleDataReaderConnectionString()
        {
            return GetInstance().GetOracleDataReaderConnectionString();
        }

        public static string GetDefault(string settingname)
        {
            return GetInstance().GetDefault(settingname);
        }

        public static string GetDefaultAsIs(string settingname)
        {
            return GetInstance().GetDefaultAsIs(settingname);
        }

        public static void SetDefault(string settingname, string value)
        {
            GetInstance().SetDefault(settingname, value);
        }

        public static void SetDefaultAsIs(string settingname, string value)
        {
            GetInstance().SetDefaultAsIs(settingname, value);
        }

        public static void SetLabCtrl(string sField, string sValue)
        {
            GetInstance().SetLabCtrl(sField, sValue);
        }

        public static string GetChemInvCtrl(string ChemCtrlName)
        {
            return GetInstance().GetChemInvCtrl(ChemCtrlName);
        }

        public static void WriteTrace(string Msg)
        {
            GetInstance().WriteTrace(Msg);
        }

        public static void WriteDebug(string Msg, bool TimeStamp=true)
        {
            GetInstance().WriteDebug(Msg, TimeStamp);
        }

        public static void ForceWriteDebug(string Msg, bool TimeStamp)
        {
            GetInstance().ForceWriteDebug(Msg, TimeStamp);
        }

        public static void WriteNotificationLog(string msg)
        {
            GetInstance().WriteNotificationLog(msg);
        }

        public static bool HaveMandatoryAttributesForItemType(string ecn)
        {
            return GetInstance().HaveMandatoryAttributesForItemType(ecn);
        }

        public static int SafeInt(string s)
        {
            return GetInstance().SafeInt(s);
        }

        public static int SafeInt(string s, int defaultVal)
        {
            return GetInstance().SafeInt(s, defaultVal);
        }

        public static bool SafeDate(string d, out DateTime dt)
        {
            return GetInstance().SafeDate(d, out dt);
        }

        public static double GetWeight(string value, ref string unit)
        {
            return GetInstance().GetWeight(value, ref unit);
        }

        public static bool TryGetWeight(string value, out double weight, ref string unit)
        {
            return GetInstance().TryGetWeight(value, out weight, ref unit);
        }

        public static string GetOSUserName()
        {
            return GetInstance().GetOSUserName();
        }

        public static string GetOSComputerName()
        {
            return GetInstance().GetOSComputerName();
        }

        public static string GetOSAddress()
        {
            return GetInstance().GetOSAddress();
        }

        public static string OSUserName
        {
            get { return GetInstance().OSUserName; }
            set { GetInstance().OSUserName = value; }
        }

        public static string OSComputerName
        {
            get { return GetInstance().OSComputerName; }
            set { GetInstance().OSComputerName = value; }
        }

        public static void FocusBarCodeField()
        {
            GetInstance().FocusBarCodeField();
        }

        public static void RemoveFocusBarCodeField()
        {
            GetInstance().RemoveFocusBarCodeField();
        }

        public static string FormatSpecialFunctions(string SQLString)
        {
            return GetInstance().FormatSpecialFunctions(SQLString);
        }

        /// <summary>
        /// Format SQL functions that uses multi-cross connection.
        /// </summary>
        /// <param name="dabaseServer"></param>
        /// <param name="SQLString"></param>
        /// <returns></returns>
        public static string FormatSpecialSQLFunctions(string dabaseServer, string SQLString)
        {
            return GetInstance().FormatSpecialSQLFunctions(dabaseServer, SQLString);
        }
        public static string GetBase64(int i)
        {
            return GetInstance().GetBase64(i);
        }

        public static int GetNextSequence(string seqname)
        {
            return GetInstance().GetNextSequence(seqname);
        }
        public static int GetNextResultNum(Int64 TaskID)
        {
            return GetInstance().GetNextResultNum(TaskID);
        }

        public static bool IsNarcoticsReviewRequired(string itemType)
        {
            return GetInstance().IsNarcoticsReviewRequired(itemType);
        }

        public static string CustodyDepartmentCode(string custody)
        {
            return GetInstance().CustodyDepartmentCode(custody);
        }

        public static void AddDefaultCustody(string caseKey, string ecn, bool analystCustodyAlreadyExists = false, bool updateContainerCustody = false)
        {
            GetInstance().AddDefaultCustody(caseKey, ecn, analystCustodyAlreadyExists, updateContainerCustody);
        }

        public static void AddDefaultCustody(string caseKey, string ecn, string batchKey, string containerKey, string weight, string weightUnit, bool analystCustodyAlreadyExists = false, bool updateContainerCustody = false, DateTime? custodyDate = null, DateTime? custodyTime = null)
        {
            GetInstance().AddDefaultCustody(caseKey, ecn, batchKey, containerKey, weight, weightUnit, analystCustodyAlreadyExists, updateContainerCustody, custodyDate, custodyTime);
        }

        public static void AddDefaultCustody(string caseKey, string ecn, string batchKey, string containerKey, string source, bool analystCustodyAlreadyExists = false, bool updateContainerCustody = false, DateTime? custodyDate = null, DateTime? custodyTime = null)
        {
            GetInstance().AddDefaultCustody(caseKey, ecn, batchKey, containerKey, source, null, null, analystCustodyAlreadyExists, updateContainerCustody, custodyDate, custodyTime);
        }

        public static void AddDefaultCustody(string caseKey, string ecn, string batchKey, string containerKey, int submissionNumber, string itemType, string weight, string weightUnit, bool analystCustodyAlreadyExists = false, bool updateContainerCustody = false, DateTime? custodyDate = null, DateTime? custodyTime = null)
        {
            GetInstance().AddDefaultCustody(caseKey, ecn, batchKey, containerKey, submissionNumber, itemType, weight, weightUnit, analystCustodyAlreadyExists, updateContainerCustody, custodyDate, custodyTime);
        }

        public static void UpdateItemCustody(string ecn, string custodyCode, string custodyLocation)
        {
            GetInstance().UpdateItemCustody(ecn, custodyCode, custodyLocation);
        }

        public static void UpdateItemCustody(string ecn, string custodyCode, string custodyLocation, DateTime? custodyDate, DateTime? custodyTime)
        {
            GetInstance().UpdateItemCustody(ecn, custodyCode, custodyLocation, custodyDate, custodyTime);
        }

        public static void UpdateContainerCustody(string containerKey, string custodyCode, string custodyLocation)
        {
            GetInstance().UpdateContainerCustody(containerKey, custodyCode, custodyLocation);
        }

        public static void UpdateItemCustody(string ecn, string custodyCode, string custodyLocation, string itemType)
        {
            GetInstance().UpdateItemCustody(ecn, custodyCode, custodyLocation, itemType);
        }

        public static void UpdateItemCustody(string ecn, string custodyCode, string custodyLocation, string itemType, DateTime? custodyDate, DateTime? custodyTime)
        {
            GetInstance().UpdateItemCustody(ecn, custodyCode, custodyLocation, itemType, custodyDate, custodyTime);
        }

        public static void AddCustody(string caseKey, string ecn, string custodyCode, string custodyLocation)
        {
            GetInstance().AddCustody(caseKey, ecn, custodyCode, custodyLocation);
        }

        public static void AddCustody(string caseKey, string ecn, string custodyCode, string custodyLocation, string batchKey, string containerKey, DateTime? custodyDate, DateTime? custodyTime, string parentECN, string weight, string weightUnit)
        {
            GetInstance().AddCustody(caseKey, ecn, custodyCode, custodyLocation, batchKey, containerKey, custodyDate, custodyTime, null, null, null, parentECN, weight, weightUnit);
        }

        public static void AddCustody(string caseKey, string ecn, string custodyCode, string custodyLocation, string batchKey, string containerKey,
            string submissionKey = null, string trackingNumber = null, string comments = null, string parentECN = null, string weight = null, string weightUnit = null)
        {
            GetInstance().AddCustody(caseKey, ecn, custodyCode, custodyLocation, batchKey, containerKey, null, null, submissionKey, trackingNumber, comments, parentECN, weight, weightUnit);
        }

        public static void AddCustody(string caseKey, string ecn, string custodyCode, string custodyLocation, string batchKey, string containerKey, string submissionKey, string trackingNumber, string comments, string parentECN, string weight, string weightUnit, bool floorCustody, bool itemsTab)
        {
            GetInstance().AddCustody(caseKey, ecn, custodyCode, custodyLocation, batchKey, containerKey, submissionKey, trackingNumber, comments, parentECN, weight, weightUnit, floorCustody, itemsTab);
        }

        public static void AddCustody(string caseKey, string ecn, string custodyCode, string custodyLocation, string batchKey, string containerKey, DateTime? custodyDate, DateTime? custodyTime, string submissionKey, string trackingNumber, string comments, string parentECN, string weight, string weightUnit, bool floorCustody, bool itemsTab)
        {
            GetInstance().AddCustody(caseKey, ecn, custodyCode, custodyLocation, batchKey, containerKey, custodyDate, custodyTime, submissionKey, trackingNumber, comments, parentECN, weight, weightUnit, floorCustody, itemsTab);
        }

        public static Boolean SetupConnectionFromCINFO(string CINFO)
        {
            return GetInstance().SetupConnectionFromCINFO(CINFO);
        }

        public static Boolean CanDeleteAssignment(string ExamKey)
        {
            return GetInstance().CanDeleteAssignment(ExamKey);
        }

        public static void GetQualifiedItems(string ckey, string batchKey)
        {
            GetInstance().GetQualifiedItems(ckey, batchKey);
        }

        public static void CheckNarcoSubmission(string ecn)
        {
            GetInstance().CheckNarcoSubmission(ecn);
        }

        public static string GetAppStartPage()
        {
            return GetInstance().GetAppStartPage();
        }

        public static string GetCustomLabCtrl(string labCtrlKey, string labCode)
        {
            return GetInstance().GetCustomLabCtrl(labCtrlKey, labCode);
        }

        public static string GetLabCtrl(string labctrlkey)
        {
            return GetInstance().GetLabCtrl(labctrlkey);
        }

        public static string GetLabCtrlXmlNode(string labctrlkey, string xmlNodeName)
        {
            return GetInstance().GetLabCtrlXmlNode(labctrlkey, xmlNodeName);
        }

        public static string GetDeptCtrlXmlNode(string deptCtrlKey, string xmlNodeName, string defaultValue = "")
        {
            return GetInstance().GetDeptCtrlXmlNode(deptCtrlKey, xmlNodeName, defaultValue);
        }

        public static string GetSAKCtrlXmlNode(string id, string sakctrlkey, string xmlNodeName)
        {
            return GetInstance().GetSAKCtrlXmlNode(id, sakctrlkey, xmlNodeName);
        }

        public static void ClearLabCtrlXmlNode()
        {
            GetInstance().ClearLabCtrlXmlNode();
        }

        public static string GetLabCtrlFlag(string labctrlkey)
        {
            return GetInstance().GetLabCtrlFlag(labctrlkey);
        }

        public static int GetLabCtrlNum(string labctrlkey)
        {
            return GetInstance().GetLabCtrlNum(labctrlkey);
        }

        public static string GetCaseLabCtrl(string labctrlkey)
        {
            return GetInstance().GetCaseLabCtrl(labctrlkey);
        }

        public static string GetLabCtrlTableOfKey(string labctrlkey)
        {
            return GetInstance().GetLabCtrlTableOfKey(labctrlkey);
        }

        public static string GetDeptCtrl(string deptctrlKey)
        {
            return GetInstance().GetDeptCtrl(deptctrlKey);
        }

        public static string GetDeptCtrlFlag(string deptctrlKey)
        {
            return GetInstance().GetDeptCtrlFlag(deptctrlKey);
        }

        public static string GetCODCtrl(string codCtrlKey)
        {
            return GetInstance().GetCODCtrl(codCtrlKey);
        }

        public static string GetCODCtrlFlag(string codCtrlKey)
        {
            return GetInstance().GetCODCtrlFlag(codCtrlKey);
        }

        public static void ClearUPSessionTraceAppCache()
        {
            GetInstance().ClearUPSessionTraceAppCache();
        }

        public static string GetUPSessionTracAppCache(bool refreshValue=false)
        {
            return GetInstance().GetUPSessionTracAppCache(refreshValue);
        }

        public static string GetWebConfiguration(string code)
        {
            return GetInstance().GetWebConfiguration(code);
        }

        public static void SetWebConfiguration(string code, string value)
        {
            GetInstance().SetWebConfiguration(code, value);
        }

        public static string GetStoredProcDecryptValue(string codeValue)
        {
            return GetInstance().GetStoredProcDecryptValue(codeValue);
        }

        public static string GetStoredProcEncrypt(string codeValue)
        {
            return GetInstance().GetStoredProcEncrypt(codeValue);
        }

        public static string GetDNAConfig(string flag)
        {
            return GetInstance().GetDNAConfig(flag);
        }

        public static string GetCache(string key, string sqlIfCacheNotFound)
        {
            return GetInstance().GetCache(key, sqlIfCacheNotFound);
        }

        public static void GetCodeHeadFieldNames(string tableName, out string codeField, out string descriptionField, out string activeField)
        {
            GetInstance().GetCodeHeadFieldNames(tableName, out codeField, out descriptionField, out activeField);
        }

        public static string GenerateCodeHeadSQL(string tableName, string selectedValue, string filter, string descFormat, string descSeparator, string parentFlexBoxValue, bool showActiveOnly, Dictionary<string, object> parentControlValues = null)
        {
            return GetInstance().GenerateCodeHeadSQL(tableName, selectedValue, filter, descFormat, descSeparator, parentFlexBoxValue, showActiveOnly, parentControlValues);
        }

        public static string GenerateCodeHeadSQL(string tableName, string selectedValue, string filter, string descFormat, string descSeparator, string parentFlexBoxValue, bool showActiveOnly, string sortOrder, Dictionary<string, object> parentControlValues = null)
        {
            return GetInstance().GenerateCodeHeadSQL(tableName, selectedValue, filter, descFormat, descSeparator, parentFlexBoxValue, showActiveOnly, sortOrder, parentControlValues);
        }

        public static string ReplaceSpecialKeysInCodeCondition(string codeCondition)
        {
            return GetInstance().ReplaceSpecialKeysInCodeCondition(codeCondition);
        }

        public static void ReInitAvailableLabCtrlKeys()
        {
            GetInstance().ReInitAvailableLabCtrlKeys();
        }

        public static void ClearLabCtrlSessionVars()
        {
            GetInstance().ClearLabCtrlSessionVars();
        }

        public static void ClearDeptCtrlSessionVars()
        {
            GetInstance().ClearDeptCtrlSessionVars();
        }

        public static TimeSpan GetMetadataCacheDuration()
        {
            return GetInstance().GetMetadataCacheDuration();
        }

        public static T GetProperty<T>(string Key, object Value)
        {
            return GetInstance().GetProperty<T>(Key, Value);
        }

        public static void SetProperty<T>(string Key, object Value)
        {
            GetInstance().SetProperty<T>(Key, Value);
        }

        public static void WriteAuditLog(string caseKey, string evidenceControlNumber, string logCode, string logSubcode, string errorCode, string userId, string programName, string logInfo, int changeKey)
        {
            GetInstance().WriteAuditLog(caseKey, evidenceControlNumber, null, logCode, logSubcode, errorCode, userId, programName, logInfo, changeKey);
        }

        public static void WriteAuditLog(string caseKey, string evidenceControlNumber, string logCode, string logSubcode, string errorCode, string userId, string programName, string logInfo)
        {
            GetInstance().WriteAuditLog(caseKey, evidenceControlNumber, null, logCode, logSubcode, errorCode, userId, programName, logInfo, 0);
        }

        public static void WriteAuditLog(string caseKey, string evidenceControlNumber, string examKey, string logCode, string logSubcode, string errorCode, string userId, string programName, string logInfo, int changeKey)
        {
            GetInstance().WriteAuditLog(caseKey, evidenceControlNumber, examKey, logCode, logSubcode, errorCode, userId, programName, logInfo, changeKey);
        }

        public static void WriteAuditLog(string caseKey, string evidenceControlNumber, string examKey, string logCode, string logSubcode, string errorCode, string userId, string programName, string logInfo)
        {
            GetInstance().WriteAuditLog(caseKey, evidenceControlNumber, examKey, logCode, logSubcode, errorCode, userId, programName, logInfo, 0);
        }

        public static void WriteAuditLog(string logCode, string logSubcode, string errorCode, string logInfo, int changeKey)
        {
            GetInstance().WriteAuditLog(logCode, logSubcode, errorCode, logInfo, changeKey);
        }

        public static void WriteAuditLog(string logCode, string logSubcode, string errorCode, string logInfo)
        {
            GetInstance().WriteAuditLog(logCode, logSubcode, errorCode, logInfo, 0);
        }

        public static void WriteAuditCon(string tableName, string logCode, string logSubcode, string errorCode, string logInfo, int changeKey)
        {
            GetInstance().WriteAuditCon(tableName, logCode, logSubcode, errorCode, logInfo, changeKey);
        }

        public static void WriteAuditLam(string chemControlNumber, string tableName, string logCode, string logSubcode, string errorCode, string logInfo, int changeKey)
        {
            GetInstance().WriteAuditLam(chemControlNumber, tableName, logCode, logSubcode, errorCode, logInfo, changeKey);
        }

        public static void WriteAuditCon(string tableName, string fileSourceKey1, string fileSourceKey2, string logCode, string logSubcode, string logInfo, int changeKey)
        {
            GetInstance().WriteAuditCon(tableName, fileSourceKey1, fileSourceKey2, logCode, logSubcode, logInfo);
        }

        public static void ClearAllCache()
        {
            GetInstance().ClearAllCache();
        }

        public static bool UserCanAccessPage(HttpRequest req)
        {
            return GetInstance().UserCanAccessPage(req);
        }

        public static bool UserCanAccessPage(string pageFilename)
        {
            return GetInstance().UserCanAccessPage(pageFilename);
        }

        public static bool CanAccessPageThroughAddressBar(HttpRequest req)
        {
            return GetInstance().CanAccessPageThroughAddressBar(req);
        }

        public static bool CanAccessPageThroughAddressBar(string pageFilename)
        {
            return GetInstance().CanAccessPageThroughAddressBar(pageFilename);
        }

        public static string EncryptConfig(string value, string code, int id)
        {
            return GetInstance().EncryptConfig(value, code, id);
        }

        public static string EncryptConfig(string value, string code, string id)
        {
            return GetInstance().EncryptConfig(value, code, id);
        }

        public static string DecryptConfig(string value, string code, int id)
        {
            return GetInstance().DecryptConfig(value, code, id);
        }

        public static string DecryptConfig(string value, string code, string id)
        {
            return GetInstance().DecryptConfig(value, code, id);
        }

        /// <summary>
        /// Reset/Restore the value of Session Timeout
        /// </summary>
        /// <param name="timeout">Optional. Less than or equal to 0 means restore original Session Timeout and is the default value.</param>
        public static void ResetSessionTimeout(int timeout = 0)
        {
            GetInstance().ResetSessionTimeout(timeout);
        }

        public static string ProcessSQLMacro(string sql)
        {
            return GetInstance().ProcessSQLMacro(sql);
        }

        public static string GetDateFormat()
        {
            return GetInstance().GetDateFormat();
        }

        public static string GetDeptCtrlDateFormat()
        {
            return GetInstance().GetDeptCtrlDateFormat();
        }

        public static string GetLongDateFormat()
        {
            return GetInstance().GetLongDateFormat();
        }

        public static AjaxControlToolkit.MaskedEditUserDateFormat GetUserDateFormat()
        {
            return GetInstance().GetUserDateFormat();
        }

        public static string GetDateMask()
        {
            return GetInstance().GetDateMask();
        }

        public static String GetCultureName()
        {
            return GetInstance().GetCultureName();
        }


        public static String GetPrelogCultureName()
        {
            return GetInstance().GetPrelogCultureName();
        }
        public static DateTime ConvertToDateTime(string dateTimeText)
        {
            return GetInstance().ConvertToDateTime(dateTimeText);
        }

        public static string DateStringToMDY(string dateText)
        {
            return GetInstance().DateStringToMDY(dateText);
        }

        public static string DateStringToDMY(string dateText)
        {
            return GetInstance().DateStringToDMY(dateText);
        }

        public static string ContainerPackagedInText()
        {
            return GetInstance().ContainerPackagedInText();
        }

        public static string GetDNACtrl(string columnName, string section)
        {
            return GetInstance().GetDNACtrl(columnName, section);
        }

        public static string GetSectionItemSettings(string columnName, string section)
        {
            return GetInstance().GetSectionItemSettings(columnName, section);
        }

        public static void FormatDatePLCGridViewBoundFields(PLCGridView grid)
        {
            GetInstance().FormatDatePLCGridViewBoundFields(grid);
        }

        public static void FormatDateGridViewBoundFields(GridView grid)
        {
            GetInstance().FormatDateGridViewBoundFields(grid);
        }

        public static Dictionary<String, String> GetItemBCDefaultSettings(string source = "")
        {
            return GetInstance().GetItemBCDefaultSettings(source);
        }

        public static List<string> GetGlobalAnalystAccessCodes(String theAnalyst)
        {
            return GetInstance().GetGlobalAnalystAccessCodes(theAnalyst);
        }

        public static bool assignmentUsesOpenXML()
        {
            return GetInstance().assignmentUsesOpenXML();
        }

        public static bool resetTemplateOnRegen()
        {
            return GetInstance().resetTemplateOnRegen();
        }

        public static string GetGlobalIni(string flag)
        {
            return GetInstance().GetGlobalIni(flag);
        }

        public static void WriteIVServiceLog(string time = "", string valid = "", string details = "")
        {
            GetInstance().WriteIVServiceLog(time, valid, details);
        }

        public static string GetJWT()
        {
            return GetInstance().GetJWT();
        }

        public static string FindCrystalReport(string RPTNAME, bool IsChemInv = false, bool checkReportService = true)
        {
            return GetInstance().FindCrystalReport(RPTNAME, IsChemInv, checkReportService);
        }

        public static string FindCustomCrystalReport(string RPTNAME, bool checkReportService = true)
        {
            return GetInstance().FindCustomCrystalReport(RPTNAME, checkReportService);
        }

        public static bool PLCPrelogInquiryMode
        {
            get { return GetInstance().PLCPrelogInquiryMode; }
            set { GetInstance().PLCPrelogInquiryMode = value; }
        }

        public static bool PLCCodnaInquiryMode
        {
            get { return GetInstance().PLCCodnaInquiryMode; }
            set { GetInstance().PLCCodnaInquiryMode = value; }
        }

        /// <summary>
        /// Computes the hash of a user password
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="UserPassword"></param>
        /// <returns></returns>
        public static string GetPasswordHash(string UserID, string UserPassword)
        {
            return PLCSessionVars.GetPasswordHash(UserID, UserPassword);
        }

        /// <summary>
        /// Returns the HashAlgorithm used for password. Configurable via PASSWORDENCRYPT appSetting in web.config 
        /// </summary>
        /// <returns></returns>
        public static HashAlgorithm GetPasswordHashAlgorithm()
        {
            return PLCSessionVars.GetPasswordHashAlgorithm();
        }

        /// <summary>
        /// Update password hash in the db to the new algo if the credentials match
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="hash"></param>
        /// <param name="userID"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string UpdatePasswordHash(string tableName, string hash, string userID, string password)
        {
            return GetInstance().UpdatePasswordHash(tableName, hash, userID, password);
        }

        public static void RenewSessionID()
        {
            GetInstance().RenewSessionID();
        }

        public static string GetLocalizedTextData(string resourceKey)
        {
            return GetInstance().GetLocalizedTextData(resourceKey);
        }

        public static bool CheckCaseTitleFuncExist()
        {
            return GetInstance().CheckCaseTitleFuncExist();
        }

        public static string GetCaseTitleFromFunc(string tabName)
        {
            return GetInstance().GetCaseTitleFromFunc(tabName);
        }

        public static string GetSysPrompt(string code, string value = "", string language = "")
        {
            return GetInstance().GetSysPrompt(code, value, language);
        }

        public static string PrelogUserIsAdmin()
        {
            return GetInstance().PrelogUserIsAdmin();
        }

        public static string GetWebUserField(string fieldName)
        {
            return GetInstance().GetWebUserField(fieldName);
        }

        /// <summary>
        /// Get the application base URL. Set PROXY_SERVER_NAME in appSettings when using reverse proxy.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetApplicationURL(HttpRequest request)
        {
            return GetInstance().GetApplicationURL(request);
        }

        /// <summary>
        /// Get ocx module used by the function
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        public static string GetOCXModule(string functionName)
        {
            return GetInstance().GetOCXModule(functionName);
        }

        /// <summary>
        /// Get RDB config
        /// </summary>
        /// <param name="functionName"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public static string GetRDBConfig(string functionName, string flag)
        {
            return GetInstance().GetRDBConfig(functionName, flag);
        }

        /// <summary>
        /// Check if RDB config is set to T
        /// </summary>
        /// <param name="functionName"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public static bool CheckRDBConfig(string functionName, string flag)
        {
            return GetInstance().CheckRDBConfig(functionName, flag);
        }

        /// <summary>
        /// Get the application mode of the ocx module call.
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns>"DIRECT" or "REMOTE"</returns>
        public static string GetOCXApplicationMode(string functionName)
        {
            return GetInstance().GetOCXApplicationMode(functionName);
        }

        public static int GetCodnaNextSampleID(string category, int year)
        {
            return GetInstance().GetCodnaNextSampleID(category, year);
        }

        /// <summary>
        /// Get item type flag value
        /// </summary>
        /// <param name="taskType"></param>
        /// <param name="flagName"></param>
        /// <returns></returns>
        public static string GetItemTypeFlag(string itemType, string flagName)
        {
            return GetInstance().GetItemTypeFlag(itemType, flagName);
        }

        /// <summary>
        /// Check if item type flag value is T
        /// </summary>
        /// <param name="taskType"></param>
        /// <param name="flagName"></param>
        /// <returns>returns true if flag value is T</returns>
        public static bool CheckItemTypeFlag(string itemType, string flagName)
        {
            return GetInstance().CheckItemTypeFlag(itemType, flagName);
        }

        /// <summary>
        /// Get task type flag value
        /// </summary>
        /// <param name="taskType"></param>
        /// <param name="flagName"></param>
        /// <returns></returns>
        public static string GetTaskTypeFlag(string taskType, string flagName)
        {
            return GetInstance().GetTaskTypeFlag(taskType, flagName);
        }

        /// <summary>
        /// Check if task type flag value is T
        /// </summary>
        /// <param name="taskType"></param>
        /// <param name="flagName"></param>
        /// <returns>returns true if flag value is T</returns>
        public static bool CheckTaskTypeFlag(string taskType, string flagName)
        {
            return GetInstance().CheckTaskTypeFlag(taskType, flagName);
        }

        /// <summary>
        /// Checks if LABELFMT flag of a report is set to T
        /// </summary>
        /// <param name="reportName"></param>
        /// <param name="flagName"></param>
        /// <returns></returns>
        public static string GetLabelFormatFlag(string reportName, string flagname)
        {
            return GetInstance().GetLabelFormatFlag(reportName, flagname);
        }

        /// <summary>
        /// Checks if LABELFMT flag of a report is set to T
        /// </summary>
        /// <param name="reportName"></param>
        /// <param name="flagName"></param>
        /// <returns></returns>
        public static bool CheckLabelFormatFlag(string reportName, string flagname)
        {
            return GetInstance().CheckLabelFormatFlag(reportName, flagname);
        }
    }

    [Serializable]
    public class PLCSessionVars : WebControl
    {


        private void CallDefaultErrorPage()
        {

            if (System.IO.File.Exists(ThePage().MapPath("PlcWebCommon/DefaultError.aspx")))
            {
                ThePage().Response.Redirect("PlcWebCommon/DefaultError.aspx");
            }

            if (System.IO.File.Exists(ThePage().MapPath("../PlcWebCommon/DefaultError.aspx")))
            {
                ThePage().Response.Redirect("../PlcWebCommon/DefaultError.aspx");
            }

            if (System.IO.File.Exists(ThePage().MapPath("DefaultError.aspx")))
            {
                ThePage().Response.Redirect("DefaultError.aspx");
            }


        }


        // Initialize mapping between page filename and access requirements.
        // An 'access requirement' can be one or more user option or labctrl flags separated by commas.
        // Sample access requirements:
        //   'CONFIG' - user option CONFIG needed
        //   '!CONFIG' - user option CONFIG should be false
        //   'ADDCASE,labctrl.USES_BULK_INTAKE' 
        //      - user option ADDCASE and labctrl USES_BULK_INTAKE needed
        //   'ADDCASE,!labctrl.USES_BULK_INTAKE'
        //      - user option ADDCASE should be true and labctrl USES_BULK_INTAKE should be false

        private static readonly Dictionary<string, string> pageAccessMap = new Dictionary<string, string>()
        {
            {"quickcreate_advanced.aspx",   "ADDCASE, labctrl.WEB_USES_ADVANCED_QC"},
            {"quickcreate2.aspx",           "ADDCASE, !labctrl.WEB_USES_ADVANCED_QC"},
            {"bulkintake.aspx",             "ADDCASE, labctrl.USES_BULK_INTAKE, labctrl.WEB_USES_ADVANCED_QC"},
            {"batchvoucher.aspx",           "ADDCASE, labctrl.USES_BULK_INTAKE, labctrl.WEB_USES_ADVANCED_QC"},
            {"listmulti.aspx",              "labctrl.USES_WEB_ITEM_LIST"},
            {"bulkcontainer.aspx",          "labctrl.USES_BULK_CONTAINERS"},
            {"assignsearch.aspx",           "SEEASGN, !HIDEASTAB"},
            {"auditlog.aspx",               "AUDITLOG"},
            {"configtab1users.aspx",        "CONFIG"},
            {"configtab2codes.aspx",        "CONFIG"},
            {"configtab3depts.aspx",        "CONFIG"},
            {"configtab4defaults.aspx",     "CONFIG"},
            {"configtab5locations.aspx",    "CONFIG"},
            {"configtab6reportgroups.aspx", "CONFIG"},
            {"configweb.aspx",              "CONFIG"},
            {"weblabctrl.aspx",             "CONFIG"},
            {"inventorymulti.aspx",         "labctrl.USES_WEB_INVENTORY"},
            {"courier.aspx",                "labctrl.USES_COURIER_FUNCTION"},
            {"labrptsearch.aspx",           "COMPREPT, !HIDERETAB"},
            {"documents.aspx",              "labctrl.USES_DOCUMENT_LINKS"},
            {"dnaworklist.aspx",            "DNABATCH"},
            {"dnabatch.aspx",               "DNABATCH"},
            {"dnacryobox.aspx",             "DNABATCH, CRYOBOX"},
            {"codisexport.aspx",            "DNABATCH"},
            {"instbatchcreate.aspx",        "INSTMENU, IBATCHCRE"},
            {"instbatchresults.aspx",       "INSTMENU, IBATCHRES"},
            {"batsbatch.aspx",              "INSTMENU, BATSBATCH"},
            {"olympus.aspx",                "INSTMENU, SEQWIZ"},
            {"instmntinstview.aspx",        "INSTMENU, INSTVIEW"},
            {"deputyrequest.aspx",          "CREATEPDR"},
            {"csirequest.aspx",             "CREATECSIR"},
            {"dispobatch.aspx",             "DISPOS"},
            {"dispoapproval.aspx",          "DISPOS"},
            {"qms.aspx",                    "QMS"},
            {"servicesearch.aspx",          "labctrl.USES_SERVICE_REQUESTS"},
            {"servicerequestreview.aspx",   "labctrl.USES_SERVICE_REQUESTS"},
            {"tab1case.aspx",               "!HIDECATAB"},
            {"tab2submissions.aspx",        "!HIDESUTAB"},
            {"tab3names.aspx",              "!HIDENATAB"},
            {"tab4items.aspx",              "!HIDEITTAB"},
            {"tab5custody.aspx",            "!HIDECUTAB"},
            {"servicerequest.aspx",         "!HIDESETAB, labctrl.USES_SERVICE_REQUESTS"},
            {"tab6assignments.aspx",        "!HIDEASTAB, SEEASGN"},
            {"tab7reports.aspx",            "!HIDERETAB"},
            {"transfer.aspx",               "MANUALCUST"},
            {"transferjs.aspx",             "MANUALCUST"},
            {"stockreports.aspx",           "REPORTS"},
            {"sak.aspx",                    "SAKDELETE"},
            {"", ""}
        };

        // The list of all pages that you can't access by typing the url in the address bar.
        private static readonly Dictionary<string, bool> pageNoAddressBarAccess = new Dictionary<string, bool>()
        {
            {"assignmentitem.aspx", true},
            {"assignmentreport.aspx", true},
            {"assignmentreport_edit.aspx", true},
            {"assignmentreport_verify.aspx", true},
            {"assignmenttasklist.aspx", true},
            {"bccontainer.aspx", true},
            {"assignmentworksheet.aspx", true},
            {"assignmentworksheet_edit.aspx", true},
            {"assignmentworksheet_printonly.aspx", true},
            {"webscan.aspx", true},
            {"upload.aspx", true},
            {"thumbnails.aspx", true},
            {"container_maintainer.aspx", true},
            {"printbc_container.aspx", true},
            {"bctransfer.aspx", true},
            {"caseform_edit.aspx", true},
            {"plcweb.aspx", true},
            {"courier_reject.aspx", true},
            {"attachviewer.aspx", true},
            {"bctaskassignment.aspx", true}
        };

        /// <summary>
        /// LocalizedText keys default values
        /// </summary>
        public static readonly Dictionary<string, string> DefaultLocalizedTextValue = new Dictionary<string, string>()
        {
            {"AssignmentTab_BackButton", "Back to Assignments"},
            {"AssignSearch_PageTitle", "Assign Search"},
            {"Button_PrintChecklistHistory", "Checklist History"},
            {"CaseSearch_CaseTab", "By Case"},
            {"CaseSearch_HitTab", "Hit Search"},
            {"CaseSearch_MEIMSTab", "MEIMS Search"},
            {"CaseSearch_CODISTab", "CODIS Search"},
            {"CaseSearch_PageTitle", "Case Search"},
            {"CaseTab_CaseJacket", "Case Jacket"},
            {"CaseTab_CaseLabel", "Case Label"},
            {"CaseTab_CaseReports", "Case Reports"},
            {"CaseTab_ChangeCase","Change Department/Case Number"},
            {"CaseTab_DatabankError","Databank Error"},
            {"CaseTab_CaseEmail", "Case Info"},
            {"Dashboard_MyAssignment", "My Assignments"},
            {"Dashboard_RecentCases", "Recent Cases"},
            {"Global_Login_UserName", "User Name"},
            {"Global_Logout", "Logout"},
            {"Global_SR_LP_Mainnum", "CII/FBI/Main#"},
            {"ItemsTab_OpenAssign", "Open Assignments"},
            {"LabRptSearch_PageTitle", "Lab Report Search"},
            {"Menu_LIMS_ActivityLog", "Activity Log"},
            {"Menu_LIMS_Admin", "Admin"},
            {"Menu_LIMS_Admin_AuditLog", "Audit Log"},
            {"Menu_LIMS_Admin_Config", "Configuration"},
            {"Menu_LIMS_Admin_DeptCtrlSetup", "DeptCtrl Setup"},
            {"Menu_LIMS_Admin_SystemFlags", "System Flags"},
            {"Menu_LIMS_Admin_SAK", "Delete SAK"},
            {"Menu_LIMS_Admin_PrelogSetup", "Prelog Setup" },
            {"Menu_LIMS_Admin_PrelogSetup_PrelogUser", "Prelog User" },
            {"Menu_LIMS_Admin_PrelogSetup_PrelogDocument", "Prelog Documents" },
            {"Menu_LIMS_Admin_PrelogSetup_PrelogGlobalNotice", "Prelog Global Notice" },
            {"Menu_LIMS_Admin_CODNASetup", "CODNA Setup" },
            {"Menu_LIMS_Admin_CODNASetup_CODNAUser", "CODNA User" },
            {"Menu_LIMS_Admin_CODNASetup_CODNADocument", "CODNA Documents"},
            {"Menu_LIMS_Admin_GetSig", "Get Signature"},
            {"Menu_LIMS_Admin_WebLabCtrl", "LabCtrl Setup"},
            {"Menu_LIMS_AnalystScheduler", "Scheduler"},
            {"Menu_LIMS_BulkContainer", "Bulk Container"},
            {"Menu_LIMS_ChemInventory", "LAM"},
            {"Menu_LIMS_CODNA", "CODNA"},
            {"Menu_LIMS_ControlledDocuments", "Controlled Documents"},
            {"Menu_LIMS_Dashboard", "Dashboard"},
            {"Menu_LIMS_DCJS", "DCJS Submission"},
            {"Menu_LIMS_Discovery", "Discovery Packet"},
            {"Menu_LIMS_Disposition", "Disposition"},
            {"Menu_LIMS_Disposition_DispoApproval", "Approve Dispositions"},
            {"Menu_LIMS_Disposition_DispoBatch", "Dispose by Batch"},
            {"Menu_LIMS_DNA", "DNA"},
            {"Menu_LIMS_DNA_Batch", "Batch"},
            {"Menu_LIMS_DNA_CODISExport", "CODIS Export"},
            {"Menu_LIMS_DNA_CODISImport", "CODIS Import"},
            {"Menu_LIMS_DNA_CODISUpload", "CODIS Upload"},
            {"Menu_LIMS_DNA_Cryobox", "Cryobox"},
            {"Menu_LIMS_DNA_DCJS", "DCJS Export"},
            {"Menu_LIMS_DNA_Worklist", "Worklist"},
            {"Menu_LIMS_Documents", "Documents"},
            {"Menu_LIMS_DocumentUploader", "Document Uploader"},
            {"Menu_LIMS_DownloadLocalFiles", "Download Local Files"},
            {"Menu_LIMS_Instrument", "Instrument"},
            {"Menu_LIMS_Instrument_BatchCreate", "Batch Create"},
            {"Menu_LIMS_Instrument_BatchResults", "Batch Results"},
            {"Menu_LIMS_Instrument_BatsBatch", "Worklist Search"},
            {"Menu_LIMS_Instrument_InstPop", "Inst Pop"},
            {"Menu_LIMS_Instrument_InstrumentInterface", "Tox Interface"},
            {"Menu_LIMS_Instrument_InstView", "Inst View"},
            {"Menu_LIMS_Instrument_Olympus", "Olympus"},
            {"Menu_LIMS_Instrument_ResultsUpload", "Results Upload"},
            {"Menu_LIMS_Instrument_Upload", "InstUpload"},
            {"Menu_LIMS_Inventory", "Inventory"},
            {"Menu_LIMS_Inventory_Container", "Container Audit"},
            {"Menu_LIMS_Inventory_LAM", "LAM Inventory"},
            {"Menu_LIMS_Inventory_Location", "Location/Container Inventory"},
            {"Menu_LIMS_ItemList", "Item List"},
            {"Menu_LIMS_LockerTransfer", "Locker Transfer"},
            {"Menu_LIMS_NAMUS", "NamUs"},
            {"Menu_LIMS_NewCase", "New Submission"},
            {"Menu_LIMS_QMS", "QMS"},
            {"Menu_LIMS_QMS_AnalystQCFlag", "Analyst QC Flag"},
            {"Menu_LIMS_QMS_Audits", "Audits"},
            {"Menu_LIMS_QMS_DocumentControl", "Document Control"},
            {"Menu_LIMS_QMS_Proficiency", "Proficiency"},
            {"Menu_LIMS_QMS_Programs", "Programs"},
            {"Menu_LIMS_QMS_Testimony", "Testimony"},
            {"Menu_LIMS_QMS_Training", "Training"},
            {"Menu_LIMS_Reports", "Reports"},
            {"Menu_LIMS_Reports_ActivityLog", "Activity Log"},
            {"Menu_LIMS_Reports_Analytics", "Analytics"},
            {"Menu_LIMS_Reports_Assign", "Assignment"},
            {"Menu_LIMS_Reports_Case", "Cases"},
            {"Menu_LIMS_Reports_Courier", "Pull List"},
            {"Menu_LIMS_Reports_CustomReports", "Custom Reports"},
            {"Menu_LIMS_Reports_Pending", "Pending"},
            {"Menu_LIMS_Reports_ReviewApproval", "Review/Approval"},
            {"Menu_LIMS_Reports_Submission", "Submission"},
            {"Menu_LIMS_Reports_TurnAround", "Turn Around"},
            {"Menu_LIMS_Request_SRReview", "Review Requests"},
            {"Menu_LIMS_Search", "Search"},
            {"Menu_LIMS_Search_Activity", "Activity Log"},
            {"Menu_LIMS_Search_Assign", "Assignment"},
            {"Menu_LIMS_Search_AuditLoc", "Audit Location"},
            {"Menu_LIMS_Search_BatchAssign", "Batch Assignment"},
            {"Menu_LIMS_Search_BreathAlcohol", "Breath Alcohol"},
            {"Menu_LIMS_Search_Case", "Case"},
            {"Menu_LIMS_Search_Container", "Case Container"},
            {"Menu_LIMS_Search_Custody", "Custody"},
            {"Menu_LIMS_Search_ItemTracker", "Item Tracker"},
            {"Menu_LIMS_Search_Report", "Completed Lab Reports"},
            {"Menu_LIMS_Search_ServiceRequest", "Service Request"},
            {"Menu_LIMS_ServiceRequest", "Service Request"},
            {"Menu_LIMS_ServiceRequest_CSIRequest", "CSI Request"},
            {"Menu_LIMS_ServiceRequest_DeputyRequest", "Print Deputy Request"},
            {"Menu_LIMS_ServiceRequest_PhotoRequest", "Digital Imaging Request"},
            {"Menu_LIMS_Setup", "Setup"},
            {"Menu_LIMS_Setup_Client", "Workstation Setup"},
            {"Menu_LIMS_Setup_LocalFiles", "Download Local Files"},
            {"Menu_LIMS_Setup_UserPref", "User Preferences"},
            {"Menu_LIMS_Setup_UserInfo", "User Info"},
            {"Menu_LIMS_SubEnvelope", "Submission Envelope" },
            {"Menu_LIMS_Worklist", "Batch Worklist"},
            {"Menu_LIMS_WorklistCreate", "Worklist Create"},
            {"Menu_LIMS_WorklistSearch", "Worklist Search"},
            {"Menu_LIMS_UPS", "UPS"},
            {"Menu_LIMS_DuplicateNames", "Duplicate Names"},
            {"Menu_LIMS_LabDocUploader", "Lab Doc Uploader"},
            {"Menu_LIMS_DataBank", "Databank Intake"},
            {"Menu_LIMS_DataBank_Receive", "Receive Kit"},
            {"Menu_LIMS_DataBank_Wrangling", "Wrangling Kit"},
            {"Menu_LIMS_DataBank_Entry", "Data Entry"},
            {"NewAssign_PageTitle", "Create a New Assignment"},
            {"PrelogLoginText", " "},
            {"QuickCreate_CaseSearch", "CASE SEARCH"},
            {"SubmissionTab_Receipt", "Receipt"},
            {"SubmissionTab_SubmissionNum", "Submission ## Items"},
            {"Tabs_Assign_GeneralWorksheets", "GENERAL WORKSHEETS"},
            {"Tabs_Assign_Item", "ITEM"},
            {"Tabs_Assign_ReportWriting", "REPORT WRITING"},
            {"Tabs_Assign_Revisions", "REVISIONS"},
            {"Tabs_Assign_Tasks", "TASKS"},
            {"Tabs_Assign_Verifications", "VERIFICATION"},
            {"Tabs_Assign_VerificationWorksheets", "VERIFICATION WORKSHEETS"},
            {"Tabs_Case_Assignments", "ASSIGNMENTS"},
            {"Tabs_Case_CaseInfo", "CASE INFO"},
            {"Tabs_Case_Custody", "CUSTODY"},
            {"Tabs_Case_Items", "ITEMS"},
            {"Tabs_Case_Names", "NAMES"},
            {"Tabs_Case_Reports", "REPORTS"},
            {"Tabs_Case_ServiceRequests", "SERVICE REQUESTS"},
            {"Tabs_Case_Submission", "SUBMISSION"},
            {"Prelog_Self_Service", "Self Service"}
        };

        public PLCSessionVars()
        {

        }

        private static PLCSessionVars _instance = null;

        private static PLCSessionVars GetInstance()
        {
            // Create PLCSessionVars instance if it hasn't been instantiated yet.
            if (PLCSessionVars._instance == null)
                PLCSessionVars._instance = new PLCSessionVars();

            return PLCSessionVars._instance;
        }

        // Use instance property to instantiate and call PLCSessionVars without a constructor. Ex. PLCSessionVars.instance.GetLabCtrl()
        public static PLCSessionVars instance
        {
            get { return GetInstance(); }
        }

        // The current revision number. This will be initialized in ClearSessionVars().
        public static string CurrentRev = "0";

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalUserHostAddress
        {
            get
            {
                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalUserHostAddress"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalUserHostAddress"];
                    return s;
                }
            }

            set
            {
                TheSession()["PLCGlobalUserHostAddress"] = value;
            }
        }

        public string FixedSQLStr(string SQLStr)
        {


            if (PLCDatabaseServer == "ORACLE")
            {
                return SQLStr;
            }

            if (GetDefault("FIXUPSQLSTR") == "F")
                return SQLStr;

            string[] TempSQL = SQLStr.Split(' ');
            string FixedSQL = "";

            for (int i = 0; i < TempSQL.Count(); i++)
            {
                if ((TempSQL[i].Trim().IndexOf("'") == 0) || (TempSQL[i].Trim().IndexOf("dbo.") == 0))
                    FixedSQL = FixedSQL + " " + TempSQL[i];
                else
                    FixedSQL = FixedSQL + " " + TempSQL[i].ToUpper();
            }

            return FixedSQL;
        }

        public OleDbConnection GetDBConn()
        {
            OleDbConnection c;
            //            OleDbConnection c = SessionConnection;
            //            if (c != null)
            //            {
            //                return c;
            //            }

            string connStr = GetConnectionString();
            c = new OleDbConnection(connStr);
            //            SessionConnection = c;
            c.Open();
            return c;

        }



        public OleDbConnection GetDBConn(string connStr)
        {
            try
            {
                OleDbConnection c = new OleDbConnection(connStr);

                c.Open();
                return c;
            }
            catch (Exception e)
            {                
                PLCSession.WriteDebug("Exception in GetDBConn:" + e.Message + Environment.NewLine + e.StackTrace);
                CallDefaultErrorPage();                
                return null;
            }
        }

        [DllImport("DOTNETUTILS.DLL", EntryPoint = "GetKeyValueFromBarCode", CallingConvention = CallingConvention.Cdecl)]
        public static extern /*unsafe*/ int _GetKeyValueFromBarCode(
            [MarshalAs(UnmanagedType.LPStr)]
            string barcode);

        public int GetKeyValueFromBarCode(string barcode)
        {
            return _GetKeyValueFromBarCode(barcode);
        }

        [Serializable]
        class tUserPref
        {
            public string PrefCode;
            public string PrefValue;
        }

        public Boolean AutoApproveServiceRequest(string scode)
        {
            PLCQuery qry = new PLCQuery();
            qry.SQL = "SELECT * FROM TV_EXAMCODE where EXAM_CODE = ':SCODE'";
            qry.SetParam("SCODE", scode);
            qry.Open();
            if (qry.IsEmpty())
                return false;

            if (qry.FieldByName("AUTO_APPROVE_SR") == "T")
                return true;

            return false;
        }

        public void PrintCRWReport()
        {
            Page p = ThePage();
            if (p == null)
                return;

            string PDFSTR = GetLabCtrl("USES_PDF_VIEWER");
            if (PDFSTR == "T") //PDF In the browser
            {
                p.Response.Redirect("~/PLCWebCommon/PDFView.aspx");
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(p, typeof(Page), "uniqueKey" + DateTime.Now,
                    "_winPopup = window.open('" + GetApplicationURL(p.Request) + "/PLCWebCommon/PDFView.aspx','_blank','titlebar=no,status=no,toolbar=no,location=no,resizable=yes');", true);
            }
        }

        public void PrintCRWReportPrelog()
        {
            Page p = ThePage();
            if (p == null)
                return;

            if (UsesPrelogPDFView() == "T")
            {
                string PDFSTR = GetLabCtrl("USES_PDF_VIEWER");
                if (PDFSTR == "T") //PDF In the browser
                {
                    p.Response.Redirect("~/LIMSPrelogV2/PDFView.aspx");
                }
                else
                {
                    ScriptManager.RegisterClientScriptBlock(p, typeof(Page), "uniqueKey" + DateTime.Now,
                        "_winPopup = window.open('" + GetApplicationURL(p.Request) + "/LIMSPrelogV2/PDFView.aspx','_blank','titlebar=no,status=no,toolbar=no,location=no,resizable=yes');", true);
                }
            }
            else
                PrintCRWReport();
        }


        private string UsesPrelogPDFView()
        {
            const string USE_PDFVIEW_PRELOG = "USE_PDFVIEW_PRELOG";
            string usePrelogPDF = string.Empty;

            var appSettings = System.Configuration.ConfigurationManager.AppSettings;
            if (appSettings.AllKeys.Contains(USE_PDFVIEW_PRELOG))
            {
                usePrelogPDF = appSettings[USE_PDFVIEW_PRELOG].Trim().ToUpper();
            }

            return usePrelogPDF;
        }


        public void PrintCRWReport(Boolean IsCaseMaster)
        {
            Page p = ThePage();
            if (p == null)
                return;

            string PDFSTR = GetLabCtrl("USES_PDF_VIEWER");
            if (PDFSTR == "T") //PDF In the browser
            {
                p.Response.Redirect("~/PLCWebCommon/PDFView.aspx");
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(p, typeof(Page), "uniqueKey" + DateTime.Now,
                    "_winPopup = window.open('" + GetApplicationURL(p.Request) + "/PLCWebCommon/PDFView.aspx','_blank','titlebar=no,status=no,toolbar=no,location=no,resizable=yes');", true);
            }
        }

        public void PrintCRWReport(int crystalInputIndex)
        {
            Page p = ThePage();
            if (p == null)
            {
                PLCSession.WriteDebug("Print Error Page is NULL");
                return;
            }

            PLCSession.WriteDebug("Opening PDFView for PLCCrystalInputs index = " + crystalInputIndex);
            ScriptManager.RegisterClientScriptBlock(p, typeof(Page), "indexedReport" + crystalInputIndex + DateTime.Now,
                "_pdfPopup" + crystalInputIndex + " = window.open('" + GetApplicationURL(p.Request) + "/PLCWebCommon/PDFView.aspx?inputsel=" + crystalInputIndex.ToString() + "','_blank','titlebar=no,status=no,toolbar=no,location=no,resizable=yes');", true);
        }

        public void PrintCRWReportFromPopup()
        {
            Page p = ThePage();
            if (p == null)
                return;

            string PDFSTR = GetLabCtrl("USES_PDF_VIEWER");
            if (PDFSTR == "T") //PDF In the browser
            {
                ScriptManager.RegisterStartupScript(p, typeof(Page), "uniqueKey" + DateTime.Now, string.Format("window.top.location = '{0}';", ResolveUrl("~/PLCWebCommon/PDFView.aspx")), true);
                //p.Response.Redirect("~/PLCWebCommon/PDFView.aspx");
            }
            else
            {
                ScriptManager.RegisterStartupScript(p, typeof(Page), "uniqueKey" + DateTime.Now, "setTimeout(function () { pdfPopup = window.open('" + GetApplicationURL(p.Request) + "/PLCWebCommon/PDFView.aspx','_blank','titlebar=no,status=no,toolbar=no,location=no,resizable=yes'); }, 200);", true);
            }
        }

        public void PrintPDFData(int dataKey)
        {
            Page p = ThePage();
            if (p == null)
                return;

            var request = p.Request;

            ScriptManager.RegisterStartupScript(p, typeof(Page), "uniqueKey" + DateTime.Now,
                "_winPopup = window.open('" + GetApplicationURL(request) + "/ShowReportPDF.aspx?datakey=" + dataKey + "','_blank','titlebar=no,status=no,toolbar=no,location=no,resizable=yes');", true);
        }

        public void LoadUserPrefs()
        {

            if (ThePage() == (Page)null)
                return;

            List<tUserPref> myPrefs = new List<tUserPref>();
            tUserPref myPref;

            PLCQuery qry = new PLCQuery();
            qry.SQL = "select * from TV_USERPREFS where USER_ID = '" + PLCGlobalAnalyst + "'";
            qry.Open();
            while (!qry.EOF())
            {
                myPref = new tUserPref();
                myPref.PrefCode = qry.FieldByName("PREFERENCE_CODE");
                myPref.PrefValue = qry.FieldByName("PREFERENCE_VALUE");
                myPrefs.Add(myPref);
                qry.Next();
            }

            TheSession()["USERPREFS"] = myPrefs;

        }

        public string GetUserPref(string prefCode)
        {
            //if (ThePage() == (Page)null) return "";
            List<tUserPref> myPrefs = (List<tUserPref>)TheSession()["USERPREFS"];

            if (myPrefs == null) return "";

            foreach (tUserPref apref in myPrefs)
            {
                if (apref.PrefCode == prefCode)
                {
                    return apref.PrefValue;
                }
            }
            return "";
        }

        public void SetUserPref(string prefCode, string prefValue, bool bypassPageCheck = false)
        {

            if (GetUserPref(prefCode) == prefValue) return;

            if (ThePage() == (Page)null && !bypassPageCheck) return;

            List<tUserPref> myPrefs = (List<tUserPref>)TheSession()["USERPREFS"];

            if (myPrefs == null) return;

            if (myPrefs.FirstOrDefault(p => p.PrefCode == prefCode) == null)
            {
                tUserPref myPref = new tUserPref();
                myPref.PrefCode = prefCode;
                myPref.PrefValue = prefValue;
                myPrefs.Add(myPref);
            }

            foreach (tUserPref apref in myPrefs)
            {
                if (apref.PrefCode == prefCode)
                {
                    apref.PrefValue = prefValue;
                }
            }


            PLCQuery qry = new PLCQuery();
            qry.SQL = "SELECT * FROM TV_USERPREFS where USER_ID = ':USERID' AND PREFERENCE_CODE = ':PREFCODE'";
            qry.SetParam("USERID", PLCGlobalAnalyst);
            qry.SetParam("PREFCODE", prefCode);
            qry.Open();
            if (!qry.IsEmpty())
            {
                qry.Edit();
                qry.SetFieldValue("PREFERENCE_VALUE", prefValue);
                qry.Post("TV_USERPREFS", 106, 2);
            }
            else
            {
                qry.Append();
                qry.SetFieldValue("USER_ID", PLCGlobalAnalyst);
                qry.SetFieldValue("PREFERENCE_CODE", prefCode);
                qry.SetFieldValue("PREFERENCE_VALUE", prefValue);
                qry.Post("TV_USERPREFS", 106, 1);
            }


        }

        public void UpdateUserPrefSession(string prefCode, string prefValue)
        {
            if (GetUserPref(prefCode) == prefValue) return;

            if (ThePage() == (Page)null) return;

            List<tUserPref> myPrefs = (List<tUserPref>)TheSession()["USERPREFS"];

            if (myPrefs == null) return;

            if (myPrefs.FirstOrDefault(p => p.PrefCode == prefCode) == null)
            {
                tUserPref myPref = new tUserPref();
                myPref.PrefCode = prefCode;
                myPref.PrefValue = prefValue;
                myPrefs.Add(myPref);
            }

            foreach (tUserPref apref in myPrefs)
            {
                if (apref.PrefCode == prefCode)
                {
                    apref.PrefValue = prefValue;
                }
            }
        }

        public void ClearTransferInfo()
        {

            PLCTransferLoc = "";
            PLCTransferItems = "";
            PLCTransferECN = "";
            PLCDataSet = null;
            PLCDataSet2 = null;
            PLCGeneralDataSet = null;
            TransferCustody = "";
            TransferLocation = "";

        }

        public bool SectionRequiresAnalyst(string ExamCode)
        {
            if (ThePage() == (Page)null)
                return false;
            string AnalystRequiredSections = (string)TheSession()["ANALYST_REQIRED_SECTIONS"];
            if (AnalystRequiredSections.IndexOf(":" + ExamCode + ":") >= 0)
                return true;
            else
                return false;
        }

        public string Signed_Status()
        {
            //if (ThePage() == (Page)null) return "";
            return (string)TheSession()["SIGNED_STATUS"];
        }

        public string Reviewed_Status()
        {
            //if (ThePage() == (Page)null) return "";
            return (string)TheSession()["REVIEWED_STATUS"];
        }

        public string Approved_Status()
        {
            //if (ThePage() == (Page)null) return "";
            return (string)TheSession()["APPROVED_STATUS"];
        }

        public string Admin_Closed_Status()
        {
            //if (ThePage() == (Page)null) return "";
            return (string)TheSession()["ADMIN_CLOSED_STATUS"];
        }

        /// <summary>
        /// Get Section Flag value.
        /// </summary>
        /// <param name="examCode">Exam Code</param>
        /// <param name="flagName">Column Name</param>
        /// <returns>Flag value</returns>
        public string GetSectionFlag(string examCode, string flagName)
        {
            PLCQuery qryExamCode = new PLCQuery("SELECT * FROM TV_EXAMCODE WHERE EXAM_CODE = ?");
            qryExamCode.AddParameter("EXAM_CODE", examCode);
            qryExamCode.Open();

            if (qryExamCode.HasData())
                if (qryExamCode.FieldExist(flagName))
                    return qryExamCode.FieldByName(flagName);

            return string.Empty;
        }

        /// <summary>
        /// Check if Section Flag value is T
        /// </summary>
        /// <param name="examCode">Exam Code</param>
        /// <param name="flagName">Column Name</param>
        /// <returns>True if Flag value is T</returns>
        public bool CheckSectionFlag(string examCode, string flagName)
        {
            string flag = GetSectionFlag(examCode, flagName);
            return flag.Trim().ToUpper() == "T";
        }

        public bool ValidatePassword(string analyst, string userPassword, out string ErrMsg, bool allowPin = false)
        {
            bool usesPinPassword = allowPin
                && GetLabCtrlFlag("USE_PIN_FOR_VERIFICATIONS").Equals("T");

            if (string.IsNullOrEmpty(userPassword))
            {
                ErrMsg = usesPinPassword
                    ? "Invalid PIN"
                    : "Invalid Password";
                return false;
            }

            if (usesPinPassword)
                return ValidatePIN(analyst, userPassword, out ErrMsg);
            if (String.IsNullOrEmpty(PLCDomainName))
                return ValidatePassword_LIMS(analyst, userPassword, out ErrMsg);
            else if (PLCSession.GetProperty<bool>("USES_ADFS_AUTH", false))
                return ValidatePassword_ADFS(analyst, userPassword, out ErrMsg);
            else
                return ValidatePassword_ActiveDirectory(analyst, userPassword, out ErrMsg);
        }

        public bool ValidatePIN(string userID, string userPIN, out string errMsg)
        {
            WriteDebug("ValidatePIN: UserID = " + userID, true);
            errMsg = string.Empty;

            string hashPIN = GetPasswordHash(userID, userPIN);
            string dbPIN = string.Empty;
            bool isValid = false;

            var qryPassword = new PLCQuery();
            qryPassword.SQL = "SELECT * FROM TV_PASSWORD WHERE PASSWORD_ID = ?";
            qryPassword.AddSQLParameter("PASSWORD_ID", userID);
            qryPassword.Open();
            if (qryPassword.IsEmpty())
            {
                errMsg = "PIN is not assigned, please contact your administrator";
            }
            else
            {
                dbPIN = qryPassword.FieldByName("PIN");

                if (string.IsNullOrEmpty(dbPIN))
                {
                    errMsg = "No PIN found. Please create a PIN.";
                }
                else if (dbPIN != hashPIN)
                {
                    errMsg = "Invalid PIN";
                }
                else
                {
                    isValid = true;
                }
            }

            WriteDebug("ValidatePIN: " + isValid, true);

            return isValid;
        }

        private Dictionary<String, String>  LoadUserGroupLinks()
        {


            WriteDebug("LoadUserGroupLinks in", true);

            //if (ADGroupLinks != null) return;

            Dictionary<String, String> ADGroupLinks = new Dictionary<string, string>();

            WriteDebug("LoadUserGroupLinks in 2", true);
            PLCQuery qrycodefields = CacheHelper.OpenCachedSqlReadOnly("SELECT * FROM TV_GROUPS WHERE AD_GROUP_LINK IS NOT NULL");
            PLCQuery getLinks = new PLCQuery("SELECT * FROM TV_GROUPS WHERE AD_GROUP_LINK IS NOT NULL");
            getLinks.Open();
            while (!getLinks.EOF())
            {
                WriteDebug("AD_GROUP_LINK ->" +  getLinks.FieldByName("AD_GROUP_LINK") + "<",true);
                WriteDebug("GROUP_CODE ->" + getLinks.FieldByName("GROUP_CODE") + "<", true);
                try
                {
                    ADGroupLinks.Add(getLinks.FieldByName("AD_GROUP_LINK").ToUpper(), getLinks.FieldByName("GROUP_CODE"));
                }
                catch
                {
                    WriteDebug("Duplicate security group mapping", true);
                }

                getLinks.Next();
            }

            WriteDebug("LoadUserGroupLinks out", true);


            return ADGroupLinks;
        }

        public bool ValidatePassword_ActiveDirectory(string analyst, string userPassword, out string ErrMsg)
        {
            ErrMsg = "";

            string osUserName = GetSSOUserName(analyst);
            if (!String.IsNullOrEmpty(osUserName))
            {
                try
                {
                    WriteDebug("ValidatePassword_ActiveDirectory, DOMAIN:" + PLCDomainName + " | ANALYST:" + analyst + " | OS_USER_NAME:" + osUserName, true);
                    using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, PLCDomainName))
                    {
                        if (pc.ValidateCredentials(osUserName, userPassword))
                        {
                            PLCSession.PLCDomainUserName = osUserName;
                            return true;
                        }
                        else
                            ErrMsg = "Invalid Domain Username/Password";
                    }
                }
                catch (Exception e)
                {
                    WriteDebug("Exception in ValidatePassword_ActiveDirectory, Message:" + e.Message, true);
                    ErrMsg = e.Message;
                }
            }
            else
                ErrMsg = "Domain User Not Found";

            return false;
        }

        public bool ValidatePassword_ActiveDirectory_Old(string UserID, string UserPassword, out string ErrMsg, out Boolean adError)
        {

            WriteDebug("ValidatePassword_ActiveDirectory in, UserID:"+ UserID + ": Domain:" + PLCDomainName,true);
            adError = false;
            ErrMsg = "";

            PLCQuery qryAnalyst = new PLCQuery("SELECT * FROM TV_ANALYST WHERE ANALYST = '" + UserID + "'");
            qryAnalyst.Open();
            string OSUserName = qryAnalyst.FieldByName("OS_USER_NAME");
            string[] osUserNames = OSUserName.Split('|');


            WriteDebug("ValidatePassword_ActiveDirectory, OSUserName:" + OSUserName + ":", true);

            try
            {

                Dictionary<String, String> glinks =  LoadUserGroupLinks();

                using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, PLCDomainName))
                {

                    WriteDebug("ValidatePassword_ActiveDirectory, Server:" + pc.ConnectedServer + ": CONTAINER:" + pc.Container + ":", true);
                    // validate the credentials

                    Boolean pwValid = false;

                    foreach (String s in osUserNames)
                    {
                        if (!pwValid)
                        {
                            pwValid = pc.ValidateCredentials(s, UserPassword);
                            if (pwValid) PLCSession.PLCDomainUserName = s;
                        }
                    }


                    if (pwValid)
                    {
                        WriteDebug("ValidatePassword_ActiveDirectory, Credentials Validated", true);
                        // Check and Syncronize the user options

                        // get the user of that domain by his username, within the context
                        UserPrincipal thePrincipal = UserPrincipal.FindByIdentity(pc, OSUserName);
                        PrincipalSearchResult<Principal> groups = thePrincipal.GetGroups();
                        Principal[] ADGroups = groups.ToArray();

                        string grouplist = "";
                        foreach (Principal gp in ADGroups)
                        {
                            WriteDebug("GroupName:" + gp.Name, true);
                            string thisgroup = "";

                            if (glinks.ContainsKey(gp.Name.ToUpper()))
                            {
                                thisgroup = "";
                                try
                                {
                                    glinks.TryGetValue(gp.Name.ToUpper(), out thisgroup);
                                    if (!String.IsNullOrWhiteSpace(thisgroup))
                                    {
                                        WriteDebug("GroupCodeFOund:" + thisgroup, true);
                                        if (!string.IsNullOrEmpty(grouplist)) grouplist += ",";
                                        grouplist += thisgroup;
                                    }

                                }
                                catch (Exception e)
                                {
                                    WriteDebug("GroupLink Exception:" + e.Message, true);
                                }


                            }
                            else
                                WriteDebug("KeyNotFOund:" + gp.Name, true);




                        }

                        if (grouplist != "")
                        {
                            WriteDebug("Updating Analyst GroupList:" + grouplist, true);
                            // SOrt the groups
                            string[] g = grouplist.Split(',');
                            List<String> gl = g.ToList();
                            gl.Sort();
                            grouplist = string.Join(",", gl.ToArray());
                            // compare and update if required
                            if (qryAnalyst.FieldByName("GROUP_CODE") != grouplist)
                            {
                                qryAnalyst.Edit();
                                qryAnalyst.SetFieldValue("GROUP_CODE", grouplist);
                                qryAnalyst.Post("TV_ANALYST", 3, 6);
                            }

                        }
                        else
                        {
                            qryAnalyst.Edit();
                            qryAnalyst.SetFieldValue("GROUP_CODE", "");
                            qryAnalyst.Post("TV_ANALYST", 3, 6);
                        }

                        // End of the check user options

                        WriteDebug("Updating Analyst Password", true);

                        PLCQuery qryPassword = new PLCQuery("SELECT * FROM TV_PASSWORD where PASSWORD_ID = '" + UserID + "'");
                        qryPassword.Open();
                        string oldPW = "";
                        string newPW = "";

                        if (qryPassword.HasData())
                        {
                            oldPW = qryPassword.FieldByName("PASSWORD");

                            newPW = GetPasswordHash(UserID, UserPassword);
                        }

                        if (newPW == oldPW) return true;

                        if (qryPassword.HasData())
                        {
                            qryPassword.Edit();
                        }
                        else
                        {
                            qryPassword.Append();
                            qryPassword.SetFieldValue("PASSWORD_ID", UserID);

                        }

                        qryPassword.SetFieldValue("PASSWORD", newPW);
                        qryPassword.SetFieldValue("LAST_CHANGE_DATE", DateTime.Now.ToString("MM/dd/yyyy"));
                        qryPassword.Post("TV_PASSWORD", 7000, 14);

                    }
                    else
                    {
                        WriteDebug("ValidatePassword_ActiveDirectory, Credentials Not Accepted", true);
                        return false;
                    }

                    WriteDebug("ValidatePassword_ActiveDirectory Out, UserID:"+ UserID + ":",true);

                    return true;


                }

            }
            catch (Exception e)
            {

                WriteDebug("Exception in ValidatePassword_ActiveDirectory, MSG:" + e.Message,true);
                adError = true;
                return false;
            }


        }

        public bool UpdateGroupMembership(string UserID)
        {
            if (String.IsNullOrWhiteSpace(PLCDomainName))
            {
                WriteDebug("Group membership not updated, domain not specified.",true);
                return false;
            }

            WriteDebug("UpdateGroupMembership in, UserID:" + UserID + ": Domain:" + PLCDomainName, true);

            PLCQuery qryAnalyst = new PLCQuery("SELECT * FROM TV_ANALYST WHERE ANALYST = '" + UserID + "'");
            qryAnalyst.Open();
            string OSUserName = qryAnalyst.FieldByName("OS_USER_NAME");
            string[] osUserNames = OSUserName.Split('|');

            WriteDebug("UpdateGroupMembership, OSUserName:" + OSUserName + ":", true);

            try
            {
                Dictionary<String, String> glinks = LoadUserGroupLinks();
                using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, PLCDomainName))
                {
                    WriteDebug("UpdateGroupMembership, Server:" + pc.ConnectedServer + ": CONTAINER:" + pc.Container + ":", true);

                    // GroupName:Domain Users
                    // 04/13/2016 21:27:02 (888)  : KeyNotFOund:Domain Users
                    // 04/13/2016 21:27:02 (888)  : GroupName:JIMS Local Machine Admins
                    // 04/13/2016 21:27:02 (888)  : KeyNotFOund:JIMS Local Machine Admins
                    // 04/13/2016 21:27:02 (888)  : GroupName:CITRIX - Limited Access
                    // 04/13/2016 21:27:02 (888)  : KeyNotFOund:CITRIX - Limited Access
                    // 04/13/2016 21:27:02 (888)  : GroupName:JIMS - LIMS Admins
                    // 04/13/2016 21:27:02 (888)  : KeyNotFOund:JIMS - LIMS Admins
                    // 04/13/2016 21:27:02 (888)  : GroupName:PL - Admin
                    // 04/13/2016 21:27:02 (888)  : KeyNotFOund:PL - Admin
                    // 04/13/2016 21:27:02 (888)  : UpdateGroupMembership Out, UserID:MIKE:  Group List:
                    // Check and Syncronize the user options

                    // get the user of that domain by his username, within the context
                    UserPrincipal thePrincipal = UserPrincipal.FindByIdentity(pc, OSUserName);
                    PrincipalSearchResult<Principal> groups = thePrincipal.GetGroups();
                    Principal[] ADGroups = groups.ToArray();

                    string grouplist = "";
                    foreach (Principal gp in ADGroups)
                    {
                        WriteDebug("GroupName:" + gp.Name, true);
                        string thisgroup = "";

                        if (glinks.ContainsKey(gp.Name.ToUpper()))
                        {
                            thisgroup = "";
                            try
                            {
                                glinks.TryGetValue(gp.Name.ToUpper(), out thisgroup);
                                if (!String.IsNullOrWhiteSpace(thisgroup))
                                {
                                    WriteDebug("GroupCodeFOund:" + thisgroup, true);
                                    if (!string.IsNullOrEmpty(grouplist)) grouplist += ",";
                                    grouplist += thisgroup;
                                }
                            }
                            catch (Exception e)
                            {
                                WriteDebug("GroupLink Exception:" + e.Message, true);
                            }
                        }
                        else
                            WriteDebug("KeyNotFOund:" + gp.Name, true);
                    }

                    if (grouplist != "")
                    {
                        WriteDebug("Updating Analyst GroupList:" + grouplist, true);

                        // SOrt the groups
                        string[] g = grouplist.Split(',');
                        List<String> gl = g.ToList();
                        gl.Sort();
                        grouplist = string.Join(",", gl.ToArray());

                        // compare and update if required
                        if (qryAnalyst.FieldByName("GROUP_CODE") != grouplist)
                        {
                            qryAnalyst.Edit();
                            qryAnalyst.SetFieldValue("GROUP_CODE", grouplist);
                            qryAnalyst.Post("TV_ANALYST", 3, 6);
                        }
                    }
                    else
                    {
                        qryAnalyst.Edit();
                        qryAnalyst.SetFieldValue("GROUP_CODE", "");
                        qryAnalyst.Post("TV_ANALYST", 3, 6);
                    }

                    WriteDebug("UpdateGroupMembership Out, UserID:" + UserID + ":  Group List:" + grouplist, true);
                    return true;
                }
            }
            catch (Exception e)
            {
                WriteDebug("Exception in ValidatePassword_ActiveDirectory, MSG:" + e.Message, true);
                return false;
            }
        }

        public bool ValidatePassword_LIMS(string UserID, string UserPassword, out string ErrMsg)
        {
            WriteDebug("ValidatePassword_LIMS in, UserID:" + UserID + ":", true);

            string hashpw = GetPasswordHash(UserID, UserPassword);
            string dbpw = "";

            PLCQuery qryPassword = new PLCQuery();
            qryPassword.SQL = "select * from TV_PASSWORD where PASSWORD_ID = ?";
            qryPassword.AddSQLParameter("PASSWORD_ID", UserID);
            qryPassword.Open();
            if (qryPassword.IsEmpty())
            {
                ErrMsg = "Password is not assigned, please contact your administrator";
                WriteDebug("ValidatePassword_LIMS Returing: FALSE", true);
                return false;
            }
            else
            {
                dbpw = qryPassword.FieldByName("PASSWORD");
                if (dbpw.Length != hashpw.Length)
                {
                    dbpw = UpdatePasswordHash("TV_PASSWORD", dbpw, UserID, UserPassword);
                }

                if (dbpw != hashpw)
                {
                    ErrMsg = "Invalid Password";
                    WriteDebug("ValidatePassword_LIMS Returing: FALSE", true);
                    return false;
                }
            }
            ErrMsg = "";

            WriteDebug("ValidatePassword_LIMS Returing: TRUE", true);
            return true;
        }

        public bool ValidatePassword_ADFS(string analyst, string userPassword, out string ErrMsg)
        {
            ErrMsg = "";

            string osUserName = GetSSOUserName(analyst);
            if (!string.IsNullOrEmpty(osUserName))
            {
                try
                {
                    WriteDebug("ValidatePassword_ADFS, DOMAIN:" + PLCDomainName + " | ANALYST:" + analyst + " | OS_USER_NAME:" + osUserName, true);
                    var request = HttpContext.Current.Request;
                    string appUrl = GetApplicationURL(request);
                    string tokenIssuer = appUrl + "/adfs/services/trust/13/usernamemixed";
                    string endpointName = PLCDomainName + ".sts";
                    var wsTrust = new Security.WSTrust(endpointName, appUrl);
                    string adfsUser = PLCDomainName + "\\" + osUserName;
                    if (wsTrust.ValidateCredentials(adfsUser, userPassword))
                    {
                        return true;
                    }

                    ErrMsg = "Invalid ADFS Username/Password";
                }
                catch (Exception e)
                {
                    WriteDebug("Exception in ValidatePassword_ADFS, Message:" + e.Message, true);
                    ErrMsg = e.Message;
                }
            }
            else
                ErrMsg = "ADFS User Not Found";

            return false;
        }

        public string GetSSOUserName(string analyst)
        {
            if (!String.IsNullOrEmpty(analyst))
            {
                PLCQuery qryAnalyst = new PLCQuery(string.Format("SELECT OS_USER_NAME FROM TV_ANALYST WHERE ANALYST = '{0}'", analyst.ToUpper()));
                if (qryAnalyst.Open() && qryAnalyst.HasData())
                    return qryAnalyst.FieldByName("OS_USER_NAME");
            }
            return "";
        }

        public string GetSSOAnalyst(string userName)
        {
            userName = RemoveDomainFromUsername(userName);
            if (!String.IsNullOrEmpty(userName))
            {
                PLCQuery qryAnalyst = new PLCQuery(string.Format("SELECT ANALYST FROM TV_ANALYST WHERE UPPER(OS_USER_NAME) = '{0}' OR UPPER(ALT_USER_NAME) = '{0}'", userName.ToUpper()));
                if (qryAnalyst.Open() && qryAnalyst.HasData())
                    return qryAnalyst.FieldByName("ANALYST");
            }
            return "";
        }

        private string RemoveDomainFromUsername(string username)
        {
            string user = username;

            try
            {
                if (username.Contains("\\"))
                {
                    var parts = username.Split('\\');
                    if (parts.Length == 2)
                        user = parts[1];
                }
                else if (username.Contains("@"))
                {
                    var parts = username.Split('@');
                    if (parts.Length == 2)
                        user = parts[0];
                }
            }
            catch (Exception ex)
            {
                WriteDebug("Error in RemoveDomainFromUsername: " + username
                    + Environment.NewLine + ex.Message, true);
            }

            return user;
        }

        public string GetCustodyDesc(string custcode, string custloc)
        {
            PLCQuery qryCodeFile = new PLCQuery();
            qryCodeFile.SQL = string.Format("select CC.DESCRIPTION D1, CL.DESCRIPTION D2 from TV_CUSTCODE CC, TV_CUSTLOC CL WHERE CC.CUSTODY_TYPE = '{0}' and CL.CUSTODY_CODE = CC.CUSTODY_TYPE and CL.LOCATION = '{1}'", custcode, custloc);
            qryCodeFile.OpenReadOnly();
            if (qryCodeFile.IsEmpty()) return "";
            return qryCodeFile.FieldByName("D1") + " / " + qryCodeFile.FieldByName("D2");
        }

        public string GetCodeDesc(string codetype, string codevalue)
        {
            string TheCodeFile = codetype;
            if ((TheCodeFile.Substring(0, 3) != "TV_") && (TheCodeFile.Substring(0, 3) != "UV_") && (TheCodeFile.Substring(0, 3) != "CV_"))
                TheCodeFile = "TV_" + TheCodeFile;

            //PLCQuery qrycodefile = new PLCQuery();
            //qrycodefile.SQL = "select * from " + TheCodeFile + " where 0 = 1";

            PLCQuery qrycodefields = CacheHelper.OpenCachedSqlFieldNames("SELECT * FROM " + TheCodeFile + " WHERE 0 = 1");

            string theFieldName = qrycodefields.FieldNames(1);
            string theDescName = qrycodefields.FieldNames(2);

            if (theFieldName == "")
                return "";

            PLCQuery qrycodefile = new PLCQuery();
            qrycodefile.SQL = "select * from " + TheCodeFile + " where " + theFieldName + " = '" + codevalue + "'";
            qrycodefile.OpenReadOnly();
            if (qrycodefile.IsEmpty())
                return "";
            return qrycodefile.FieldByName(theDescName);

        }

        public bool CodeValid(string codetype, string codevalue)
        {
            if (codevalue.Trim() == "")
                return true;
            string TheCodeFile = codetype;
            if ((TheCodeFile.Substring(0, 3) != "TV_") && (TheCodeFile.Substring(0, 3) != "CV_") && (TheCodeFile.Substring(0, 3) != "UV_"))
                TheCodeFile = "TV_" + TheCodeFile;

            //PLCQuery qrycodefile = new PLCQuery();
            //qrycodefile.SQL = "select * from " + TheCodeFile + " where 0 = 1";

            PLCQuery qrycodefields = CacheHelper.OpenCachedSqlFieldNames("SELECT * FROM " + TheCodeFile + " WHERE 0 = 1");

            string theFieldName = qrycodefields.FieldNames(1);
            string theDescName = qrycodefields.FieldNames(2);

            if (theFieldName == "")
                return false;


            PLCQuery qrycodefile = new PLCQuery();
            qrycodefile.SQL = "select * from " + TheCodeFile + " where " + theFieldName + " = '" + codevalue + "'";
            qrycodefile.OpenReadOnly();
            if (qrycodefile.IsEmpty())
                return false;
            else
                return true;
        }

        public void SetUserMessage(string s, Boolean error)
        {
            Page p = ThePage();
            if (p == null)
                return;

            Label lbStatus = (Label)p.FindControl("lbStatus");
            if (lbStatus != null)
            {
                lbStatus.Text = s;
            }

            MasterPage mp = ThePage().Master;
            UserControl uc = null;

            if (mp != null)
            {
                uc = (UserControl)mp.FindControl("UC_PLCCopyRights_Master2");
            }

            if (uc == null)
            {
                uc = (UserControl)p.FindControl("UC_PLCCopyRights_Master2");
            }

            if (uc == null)
                return;

            Label msg = (Label)uc.FindControl("lbError");
            if (msg == null)
                return;
            msg.Text = s;

            if (error)
                msg.ForeColor = Color.Red;
            else
                msg.ForeColor = Color.Blue;

        }

        public Boolean SetupGridView(GridView gv)
        {
            gv.AutoGenerateSelectButton = true;
            gv.AutoGenerateColumns = true;
            gv.AllowSorting = true;
            gv.AllowPaging = true;
            gv.ForeColor = Color.FromName("#333333");
            gv.GridLines = GridLines.None;
            gv.CellPadding = 4;
            gv.FooterStyle.BackColor = Color.FromName("#5D7B9D");
            gv.FooterStyle.Font.Bold = true;
            gv.FooterStyle.ForeColor = Color.FromName("White");
            gv.RowStyle.BackColor = Color.FromName("#F7F6F3");
            gv.RowStyle.ForeColor = Color.FromName("#333333");
            gv.PagerStyle.BackColor = Color.FromName("#5D7B9D");
            gv.PagerStyle.ForeColor = Color.FromName("White");
            gv.PagerStyle.HorizontalAlign = HorizontalAlign.Left;
            gv.SelectedRowStyle.BackColor = Color.FromName("#E2DED6");
            gv.SelectedRowStyle.Font.Bold = true;
            gv.SelectedRowStyle.ForeColor = Color.FromName("#333333");
            gv.HeaderStyle.BackColor = Color.FromName("#5D7B9D");
            gv.HeaderStyle.Font.Bold = true;
            gv.HeaderStyle.ForeColor = Color.FromName("White");
            gv.EditRowStyle.BackColor = Color.FromName("#999999");
            gv.AlternatingRowStyle.BackColor = Color.FromName("White");
            gv.AlternatingRowStyle.ForeColor = Color.FromName("#284775");
            gv.HeaderStyle.HorizontalAlign = HorizontalAlign.Left;
            gv.PagerStyle.HorizontalAlign = HorizontalAlign.Right;

            return true;

        }

        public int GetNextSequence_XXX(string seqname) //use the one in plcdbglobal.cs
        {

            PLCQuery qry = new PLCQuery();
            qry.SQL = "SELECT " + seqname + ".NEXTVAL NV FROM DUAL";
            qry.Open();
            return qry.iFieldByName("NV");

        }

        public void AddToRecentCases(string CaseKey, string Analyst)
        {
            PLCQuery qryAnalQuic = new PLCQuery();
            qryAnalQuic.SQL = "Select * from TV_ANALQUIC Where CASE_KEY = " + CaseKey + " And CASE_OFFICER = '" + Analyst + "'";
            qryAnalQuic.Open();
            if (qryAnalQuic.IsEmpty())
            {
                qryAnalQuic.Append();
                qryAnalQuic.SetFieldValue("CASE_KEY", CaseKey);
                qryAnalQuic.SetFieldValue("CASE_OFFICER", Analyst);
                qryAnalQuic.SetFieldValue("OPEN_DATE", DateTime.Now);
            }
            else
            {
                qryAnalQuic.Edit();
                qryAnalQuic.SetFieldValue("OPEN_DATE", DateTime.Now);
            }
            qryAnalQuic.Post("TV_ANALQUIC", -1, -1);
        }

        public void AddToRecentCODNASamples(string Sequence, string UserID)
        {
            try
            {
                PLCQuery qryAnalQuic = new PLCQuery();
                qryAnalQuic.SQL = "Select * from TV_COWEBQUIC Where SEQUENCE = " + Sequence + " And USER_ID = '" + UserID + "'";
                qryAnalQuic.Open();
                if (qryAnalQuic.IsEmpty())
                {
                    qryAnalQuic.Append();
                    qryAnalQuic.SetFieldValue("SEQUENCE", Sequence);
                    qryAnalQuic.SetFieldValue("USER_ID", UserID);
                    qryAnalQuic.SetFieldValue("OPEN_DATE", DateTime.Now);
                }
                else
                {
                    qryAnalQuic.Edit();
                    qryAnalQuic.SetFieldValue("OPEN_DATE", DateTime.Now);
                }
                qryAnalQuic.Post("TV_COWEBQUIC", -1, -1);
            }
            catch (Exception e)
            {
                WriteDebug("AddToRecentCODNASamples error: " + e.Message, true);
            }
        }

        public void AddToPrelogRecentCases(string departmentCaseNumber, string departmentCode, string webUser)
        {
            PLCQuery qryWebQuic = new PLCQuery();
            qryWebQuic.SQL = "SELECT * FROM TV_WEBQUIC WHERE DEPARTMENT_CASE_NUMBER = '" + departmentCaseNumber + "'  " +
                "AND DEPARTMENT_CODE = '" + departmentCode + "' " +
                "AND WEBUSER = '" + webUser + "' ";
            qryWebQuic.Open();
            if (qryWebQuic.IsEmpty())
            {
                qryWebQuic.Append();
                qryWebQuic.SetFieldValue("DEPARTMENT_CASE_NUMBER", departmentCaseNumber);
                qryWebQuic.SetFieldValue("DEPARTMENT_CODE", departmentCode);
                qryWebQuic.SetFieldValue("WEBUSER", webUser);
                qryWebQuic.SetFieldValue("OPEN_DATE", DateTime.Now);
            }
            else
            {
                qryWebQuic.Edit();
                qryWebQuic.SetFieldValue("OPEN_DATE", DateTime.Now);
            }
            qryWebQuic.Post("TV_WEBQUIC", -1, -1);
        }

        public string GetTruncatedCaseNumber(string Picture, string CaseNumber)
        {

            if (Picture.Trim() != "999-99999-9999-999")
            {
                return CaseNumber.Trim();
            }


            if (CaseNumber.Trim().IndexOf(" ") >= 0)
            {
                return "";
            }



            if (CaseNumber.Trim().Length < Picture.Trim().Length)
            {
                return "";
            }

            if (Picture.Trim() == "999-99999-9999-999")
            {
                return CaseNumber.Substring(1, 11);
            }
            else
            {
                return CaseNumber.Trim();
            }
        }

//Conditional compalition to support ImageUploader Project     
#if STAND_ALONE

        private Hashtable _sessionState = null;
        private Hashtable _application = null;
          
        protected Hashtable TheSession()
          {

          if (_sessionState == null) _sessionState = new Hashtable();

          return _sessionState;
          }



        protected Hashtable TheApplication()
            {

            if (_application == null) _application = new Hashtable();

            return _application;
            }

#else

        public HttpApplicationState TheApplication()
            {
            return Context.Application;
            }

        protected HttpSessionState TheSession()
            {
            return Context.Session;
            }

#endif


        protected Page ThePage()
        {

            try
            {
                return (Page)HttpContext.Current.Handler;
            }

            catch
            {



                return (Page)null;
            }

        }

        //returns true if the input could not be a table name
        public Boolean isInvalidTableName(String tn)
        {
            //Change to Regex ?? :)
            if (String.IsNullOrWhiteSpace(tn)) return true;
            if (tn.Contains(" ")) return true;
            if (tn.Contains(";")) return true;
            if (tn.Contains("--")) return true;
            if (tn.Contains("'")) return true;
            if (tn.Length > 32) return true;
            return false;
        }

        public void sqlInjAddValidHash(String hashStr)
        {
            if (String.IsNullOrWhiteSpace(hashStr)) return;
            String ts = (String)TheApplication()["sqj_inj_hash"];

            if ((String.IsNullOrWhiteSpace(ts)) || (!ts.Contains(hashStr))) ts += "|" + hashStr;
            TheApplication()["sqj_inj_hash"] = ts;
        }

        public Boolean sqlInjIsValidHash(String hashStr)
        {
            String ts = (String)TheApplication()["sqj_inj_hash"];
            if (String.IsNullOrWhiteSpace(ts)) return false;
            return ts.Contains(hashStr);
        }

        public List<String> sqlInjFilters()
        {

            //load a list of SQL Injecton ruler from labctrl and store them in the ApplicaionState as a TLIST
            if (TheApplication() == null) return null;

            List<String> theList = (List<String>)TheApplication()["SQL_INJ_FILTERS"];

            if (theList != null) return theList;

            theList = new List<String>();

            //filters of type R are RexEx Expressions for valid filters.  One example
            PLCQuery qry = new PLCQuery("SELECT * FROM TV_VALIDFILTERS WHERE FILTER_TYPE = 'R'");
            qry.Open();
            while (!qry.EOF())
            {

                String ts = qry.FieldByName("FILTER_TABLE") + "|" + qry.FieldByName("FILTER_VALUE");
                theList.Add(ts);
                qry.Next();
            }

            TheApplication()["SQL_INJ_FILTERS"] = theList;

            return theList;
        }

        public List<String> sqlInjWords()
        {
            //load a list of SQL Injecton ords from labctrl and store them in the ApplicaionState as a TLIST
            if (TheApplication() == null) return null;

            List<String> theList = (List<String>) TheApplication()["SQL_INJ_LIST"];

            if (theList != null) return theList;

            theList = new List<String>();

            String lcWordList = "";

            lcWordList = System.Configuration.ConfigurationManager.AppSettings.Get("SQL_INJ_WORDS");
            if (String.IsNullOrWhiteSpace(lcWordList))   lcWordList = "DELETE |DROP |CREATE |EXEC |WAITFOR |XP_|--|SET |DECLARE |";

            lcWordList = lcWordList.ToUpper().Trim();

            if (lcWordList != "*NONE")
            {

                String[] myWords = lcWordList.Split('|');
                foreach (String theWord in myWords)
                {
                    if (!String.IsNullOrWhiteSpace(theWord)) theList.Add(theWord);
                }

            }

            if (!String.IsNullOrWhiteSpace(PLCGlobalLabCode)) TheApplication()["SQL_INJ_WORDS"] = theList;

            return theList;
        }

        public Boolean SQLInjectionSuspected(String sqlStr)
        {

            if (String.IsNullOrWhiteSpace(sqlStr)) return false;

            String checkMode = (String)TheApplication()["SQL_CHECK_MODE"];
            if (checkMode != "ENABLED") return false;

            //suspect SQLInjection any of the SQL_INJ_WORDS occur in the "command" part of the SQL after the initial command word.
            //suspect SQLInjection if we detect a comment "--" within the command part.
            //we will suspect SQLInjection if we detect a semi colon within the command part (except at the very end)

            List<String> sqlWords = sqlInjWords();

            //working trimmed string
            String ts = sqlStr.Trim().ToUpper();

            String kw = "";
            while (ts.EndsWith(";")) ts = ts.Remove(ts.Length - 1, 1);
            if (String.IsNullOrWhiteSpace(ts)) return false;

            //while scanning, inparam is used to track if the current position in the scan is in a literal or the command
            Boolean inParam = false;
            int stackcount = 1;
            int idx = 0;
            string ch = "";

            //replace escaped double quote with a different literal char            
            ts = ts.Replace(@"\""", "z");
            //replace escaped single quote with a different literal char            
            //ts = ts.Replace(@"'", "z");

            //change double quotes to single because for scanning purposes, double quoted and single quoted constants are the same.
            //ts = ts.Replace(@"""", "'");

            //now scan - maybe replacing this with regexes that strip quoted idetifiers would be faster????

            for (idx = 0; idx < ts.Length; idx++)
            {
                kw = "";
                ch = ts.Substring(idx, 1);
                if (idx > 0)
                    kw = ts.Substring(idx);
                if ((!inParam) && (ch == "'")) inParam = true;
                else if ((inParam) && (ch == "'")) inParam = false;

                if ((!inParam) && (ch == ";"))
                {
                    ForceWriteDebug("SQLINJ Suspected :;: found in :" + kw, true);
                    stackcount++;
                }

                if (!inParam)
                {
                    if (sqlWords != null)
                    {
                        // process the entire word to avoid false positive on columns like NEVER_POPULATE_COMP_EXP_DATE, VICTIM_RPT_EXP_DAYS
                        var match = Regex.Match(kw, "^\\w+");
                        if (match.Success)
                            idx += match.Length - 1;

                        foreach (String siw in sqlWords)
                        {
                            if (kw.StartsWith(siw))
                            {
                                ForceWriteDebug("SQLINJ Suspected :" + siw + ": found in SQL_INJ_WORDS :" + kw, true);
                                stackcount++;
                            }
                        }
                    }

                    if (kw.StartsWith("--"))
                    {
                        ForceWriteDebug("SQLINJ Suspected :--: found in command part :" + kw, true);
                        stackcount++;
                    }

                }

            }
            return (stackcount > 1);

        }

        public bool CheckUserOption(string Option)
        {


            if (String.IsNullOrWhiteSpace(PLCSession.PLCGlobalAnalyst)) return false;

            if ((string)TheSession()["UO_" + Option] == null)
            {
                bool isOptionSet = GetUserOption(PLCSession.PLCGlobalAnalyst, PLCSession.PLCGlobalAnalystGroup, Option);
                TheSession()["UO_" + Option] = isOptionSet ? "T" : "F";

                return isOptionSet;
            }
            else
            {
                return TheSession()["UO_" + Option].ToString() == "T";
            }
        }

        public bool CheckUserOption(string analyst, string option)
        {


            if (String.IsNullOrWhiteSpace(analyst)) return false;

            PLCQuery qryGroup = new PLCQuery(string.Format("SELECT GROUP_CODE FROM TV_ANALYST WHERE ANALYST = '{0}'", analyst));
            qryGroup.Open();
            if (!qryGroup.IsEmpty())
            {
                return GetUserOption(analyst, qryGroup.FieldByName("GROUP_CODE"), option);
            }

            return false;
        }

        private bool GetUserOption(string analyst, string groupCsv, string option)
        {
            // ADMIN,ANDNA,CLER ==> '*ADMIN','*ANDNA','*CLER','MIKE'

            if (String.IsNullOrWhiteSpace(analyst)) return false;

            string groupCodeParams = "'*" + groupCsv.Replace(" ", "").Replace(",", "','*") + "','" + analyst + "'";

            PLCQuery qryOption = new PLCQuery(string.Format("SELECT * FROM TV_USEROPT WHERE USERID IN ({0}) AND OPTION_RES = '{1}'", groupCodeParams, option));
            qryOption.Open();
            return !qryOption.IsEmpty();
        }

        public bool CheckPanelUserOption(string panelName, string panelNameField)
        {
            if (string.IsNullOrWhiteSpace(panelName)) return false;

            bool hasUserOpt = false;

            // Query all the auth used by the panel
            var qry = new PLCQuery();
            qry.SQL = "SELECT AUTHORITY_CODE FROM TV_PANELAUTH WHERE " + panelNameField + " = ?";
            qry.AddSQLParameter(panelNameField, panelName);
            qry.OpenReadOnly();
            while (!qry.EOF())
            {
                // check if user option is turned on
                hasUserOpt = CheckUserOption(qry.FieldByName("AUTHORITY_CODE"));

                // stop the loop when a user option is turned on
                if (hasUserOpt)
                {
                    break;
                }

                qry.Next();
            }

            return hasUserOpt;
        }

        public static string SingleQuotes(string str)
        {
            return "'" + str + "'";
        }

        // Query and return whether analyst has analsect permission on any of the sections.
        private bool QueryHasAnalSectPermission(string analyst, string analsectColumn)
        {
            PLCQuery query = new PLCQuery();
            // Ex. select COUNT(*) from TV_ANALSECT where UPPER(ANALYST) = '12346' AND VIEW_APPROVED_REPORTS = 'T'
            query.SQL = "SELECT SECTION FROM TV_ANALSECT WHERE UPPER(ANALYST) = " + SingleQuotes(analyst) + " AND " + analsectColumn + " = 'T'";
            query.Open();

            // If one or more analsect sections is set to 'T', then analyst has access permission.
            if (!query.IsEmpty())
                return true;
            else
                return false;
        }

        // Query and return whether analyst has analsect permission on a specific section.
        private bool QueryHasAnalSectPermission(string analyst, string analsectSection, string analsectColumn)
        {
            PLCQuery query = new PLCQuery();
            // Ex. select COUNT(*) from TV_ANALSECT where UPPER(ANALYST) = '12346' AND SECTION = 'BAC' AND VIEW_APPROVED_REPORTS = 'T'
            query.SQL = "SELECT SECTION FROM TV_ANALSECT WHERE UPPER(ANALYST) = " + SingleQuotes(analyst) + " AND SECTION = " + SingleQuotes(analsectSection) + " AND " + analsectColumn + " = 'T'";
            query.Open();

            // If the section is set to 'T', then analyst has access permission.
            if (!query.IsEmpty())
                return true;
            else
                return false;
        }

        // Query and return the AnalSect setting corresponding to the user, section and column option.
        private string QueryUserAnalSect(string analyst, string section, string columnName)
        {
            PLCQuery query = new PLCQuery();
            // Ex. select VIEW_APPROVED_REPORTS from TV_ANALSECT where UPPER(ANALYST) = '12346' AND SECTION = 'BAC'
            query.SQL = "SELECT " + columnName + " FROM TV_ANALSECT WHERE UPPER(ANALYST) = " + SingleQuotes(analyst) + " AND SECTION = " + SingleQuotes(section);
            query.Open();

            if (!query.EOF())
                return query.FieldByName(columnName);
            else
                return "F";
        }

        // Get Read AnalSect key format. Ex. "TOX", "VIEW_APPROVED_REPORTS" = "AS-TOX-VIEW_APPROVED_REPORTS" key
        private string GetUserAnalSectKey(string section, string setting)
        {
            return "AS-" + section + "-" + setting;
        }

        // Return the AnalSect setting corresponding to user, section, and column setting.
        // Cache value via session variable. Session var format is: AS-[section name]-[analsect column]. 
        // Ex. "AS-TOX-VIEW_APPROVED_REPORTS"
        public object GetUserAnalSect(string section, string setting)
        {
            string analsectKey = GetUserAnalSectKey(section, setting);

            // Check if analsect setting was previously loaded in session variable.
            // If not loaded, query the setting from tv_analsect and initialize session variable for future access.
            if ((string)TheSession()[analsectKey] == null)
            {
                string analsectVal = QueryUserAnalSect(this.PLCGlobalAnalyst, section, setting);
                TheSession()[analsectKey] = analsectVal;
                return analsectVal;
            }
            else
            {
                return (string)TheSession()[analsectKey];
            }
        }

        /// <summary>
        /// Get Uppercased AnalSect Flag value
        /// </summary>
        /// <param name="section"></param>
        /// <param name="setting"></param>
        /// <returns></returns>
        public string GetAnalSectFlag(string section, string setting)
        {
            string flag = Convert.ToString(GetUserAnalSect(section, setting));
            return flag.Trim().ToUpper();
        }

        /// <summary>
        /// Check if AnalSect flag is T
        /// </summary>
        /// <param name="section"></param>
        /// <param name="setting"></param>
        /// <returns>true if flag value is T</returns>
        public bool CheckAnalSectFlag(string section, string setting)
        {
            string flag = GetAnalSectFlag(section, setting);
            return flag == "T";
        }

        // Return AnalSect setting corresponding to user, section, and analsect setting.
        // This is similar to GetUserAnalSect(section, setting) with the following exceptions:
        //   - Return value is not cached in a session variable.
        //   - You can pass any user (Get the analsect status of other analysts.)
        public string GetUserAnalSect(string analyst, string section, string setting)
        {
            return QueryUserAnalSect(analyst, section, setting);
        }

        // Get Has AnalSect key format. Ex. To check if any section has "VIEW_APPROVED_REPORTS",use "AS-*-VIEW_APPROVED_REPORTS" key
        private string GetHasUserAnalSectKey(string setting)
        {
            return "AS-*-" + setting;
        }

        // Return whether any of the AnalSect user's section's column settings is set to True. 
        // Cache value via session variable. Session var format is: AS-*-[analsect column]
        // Ex. "AS-*-VIEW_APPROVED_REPORTS"
        public bool GetHasUserAnalSect(string setting)
        {
            string hasAnalsectKey = GetHasUserAnalSectKey(setting);

            // Check if analsect setting was previously loaded in session variable.
            // If not loaded, query the setting from tv_analsect and initialize session variable for future access.
            if (TheSession()[hasAnalsectKey] == null)
            {
                bool hasAnalsect = QueryHasAnalSectPermission(this.PLCGlobalAnalyst, setting);
                TheSession()[hasAnalsectKey] = hasAnalsect;
                return hasAnalsect;
            }
            else
            {
                return (bool)TheSession()[hasAnalsectKey];
            }
        }

        public void ClearError()
        {

            PLCErrorMessage = "";
            PLCErrorSQL = "";
            PLCErrorURL = "";
            PLCErrorProc = "";

        }

        public void Redirect(string URL)
        {
            if (ThePage() == null)
                return;
            ThePage().Response.Redirect(URL);
        }

        public string GetPLCErrorText(Boolean UseHTML)
        {
            Page ThisPage = ThePage();
            string NewLine = "";
            if (UseHTML)
                NewLine = "[newline:p]";
            else
                NewLine = System.Environment.NewLine;


            string errstr = NewLine;
            errstr += "********** PLC Application Error **********" + NewLine;
            errstr += "LIMS Version : " + PLCBEASTiLIMSVersion + NewLine;
            errstr += "Error Message : " + PLCErrorMessage + NewLine;
            if (ThisPage != null)
            {
                if (ThisPage.Header != null)
                {
                    errstr += "Error Page Title : " + ThisPage.Header.Title + NewLine;
                }


                if (ThisPage.Session != null)
                {
                    errstr += "Session ID : " + ThisPage.Session.SessionID + NewLine;
                }


            }
            errstr += "Error URL : " + PLCErrorURL + NewLine;
            errstr += "Error Database: " + PLCDatabaseServer + NewLine;
            errstr += "Error Proc : " + PLCErrorProc + NewLine;
            errstr += "Error SQL : " + PLCErrorSQL + NewLine;
            errstr += "Error USER : " + PLCGlobalAnalyst + NewLine;
            //if (PLCGlobalLabCase != "")
            //  errstr += "Error LABCASE : " + PLCGlobalLabCase + NewLine;
            if (PLCGlobalDepartmentCaseNumber != "")
                errstr += "Error CASE : " + PLCGlobalDepartmentCaseNumber + NewLine;

            errstr += "*******************************************" + NewLine + NewLine;

            if (UseHTML)
                errstr = HttpUtility.HtmlEncode(errstr).Replace(NewLine, "<p/>");

            return errstr;

        }

        public void SaveErrorToDB()
        {
        }

        public void SaveError()
        {

            try
            {
                WriteDebug(GetPLCErrorText(false), true);
            }
            catch
            {
            }


            try
            {
                SaveErrorToDB();
            }

            catch
            {
            }

        }

        public void SetLabCaseVars(string TheCaseKey)
        {

            PLCQuery qry = new PLCQuery();
            qry.SQL = "SELECT * FROM TV_LABCASE where CASE_KEY = " + TheCaseKey;
            qry.Open();

            PLCGlobalAttachmentSource = "";
            PLCGlobalCaseKey = TheCaseKey;
            PLCGlobalURN = qry.FieldByName("DEPARTMENT_CASE_NUMBER");
            PLCGlobalSubmissionKey = "";
            PLCGlobalECN = "";
            PLCGlobalTaskID = "";
            PLCGlobalLabCase = qry.FieldByName("LAB_CASE");
            ;
            PLCGlobalDepartmentCaseNumber = qry.FieldByName("DEPARTMENT_CASE_NUMBER");
            ;
            PLCGlobalStatusKey = "";
            PLCGlobalBatchKey = "";
            PLCGlobalNameKey = "";
            PLCGlobalAnalystSearchKey = "";
            PLCGlobalNameNumber = "";
            PLCGlobalAssignmentKey = "";
            PLCGlobalAttachmentKey = "";
            PLCNewCaseKey = "";
            PLCNewLastName = "";
            PLCNewFirstName = "";
            PLCTransferECN = "";
            PLCAdditionalSubmissionKey = "";
            PLCGlobalSubmissionNumber = "";
            PLCSelectedBulkContainerKey = "";
            PLCCrystalReportAutoPrint = false;
            PLCCrystalReportLabelPrinter = "";
            PLCCrystalReportName = "";  //case.rpt
            PLCCrystalReportFormulaList = "";
            PLCCrystalSelectionFormula = ""; //{LABCASE.Case Key} = 1234
            PLCCrystalReportTitle = "";  //Case Jacket for F08-1234
            PLCCrystalCustomReportKey = "";
            PLCGlobalScheduleKey = "";
            WhatIsNext = "NOTHING";
            TransferLocation = "";
            TransferCustody = "";
        }

        public void SetCaseVariables(string caseKey)
        {
            PLCQuery qryCase = new PLCQuery();
            qryCase.SQL = "SELECT LAB_CASE, DEPARTMENT_CASE_NUMBER FROM TV_LABCASE WHERE CASE_KEY = " + caseKey;
            qryCase.Open();

            SetCaseVariables(caseKey, qryCase.FieldByName("DEPARTMENT_CASE_NUMBER"), qryCase.FieldByName("LAB_CASE"));
        }

        public void SetCaseVariables(string caseKey, string urn, string labCase)
        {
            PLCGlobalCaseKey = caseKey;
            PLCGlobalDepartmentCaseNumber = urn;
            PLCGlobalURN = urn;
            PLCGlobalLabCase = labCase;
            PLCGlobalECN = "";   // important to blank out and start fresh
            PLCGlobalNameKey = "";
        }

        public void ClearLabCaseVars()
        {
            PLCGlobalAttachmentSource = "";
            PLCGlobalCaseKey = "";
            PLCGlobalURN = "";
            PLCGlobalSubmissionKey = "";
            PLCGlobalECN = "";
            PLCGlobalTaskID = "";
            PLCGlobalLabCase = "";
            PLCGlobalDepartmentCaseNumber = "";
            PLCGlobalStatusKey = "";
            PLCGlobalBatchKey = "";
            PLCGlobalNameKey = "";
            PLCGlobalNameNumber = "";
            PLCGlobalAssignmentKey = "";
            PLCGlobalAttachmentKey = "";
            PLCNewCaseKey = "";
            PLCNewLastName = "";
            PLCNewFirstName = "";
            PLCTransferECN = "";
            PLCAdditionalSubmissionKey = "";
            PLCGlobalSubmissionNumber = "";
            PLCSelectedBulkContainerKey = "";
            PLCCrystalReportAutoPrint = false;
            PLCCrystalReportLabelPrinter = "";
            PLCCrystalReportName = "";  //case.rpt
            PLCCrystalSelectionFormula = ""; //{LABCASE.Case Key} = 1234
            PLCCrystalReportTitle = "";  //Case Jacket for F08-1234
            PLCCrystalCustomReportKey = "";
            PLCGlobalScheduleKey = "";
            WhatIsNext = "NOTHING";
            TransferLocation = "";
            TransferCustody = "";
        }

        // Get current revision number from the text file 'currentrev' in web root directory. If 'currentrev' doesn't exist, use current date as the rev number.
        public string GetCurrentRev(HttpServerUtility server)
        {
            string MainVersion = "1";

            // Get the .NET Framework version. Ex. 4.0.30319
            string FrameworkVersion = this.GetType().Assembly.ImageRuntimeVersion.Replace("v", "");

            string revFilename = server.MapPath("currentrev");


            //for Prelog build number support
            if (revFilename.ToUpper().Contains("LIMSPRELOG"))
                revFilename = revFilename.ToUpper().Replace("\\LIMSPRELOGV2", "").Replace("\\LIMSPRELOG", "");


            string revno;

            if (!File.Exists(revFilename))
            {
                revFilename = server.MapPath("~/currentrev");
            }

            if (File.Exists(revFilename))
            {
                StreamReader stream = new StreamReader(revFilename);
                revno = stream.ReadLine();
                stream.Close();
            }
            else
            {
                revno = DateTime.Now.ToShortDateString();
            }

            string currentRev = String.Format("{0}.{1} NET{2}", MainVersion, revno, FrameworkVersion);

            return Regex.Replace(currentRev, @"[ <>/]+", "-");
        }

        public void ClearSessionVars(HttpServerUtility server)
        {

            // 1.7 - Mike, 10/30/08, Added photo service requests to quick create
            // 1.9 Build 6 - 08/17/09 - Transfer Screen (barcode scanning fix) 
            //                        - MSSQL barcode stored procedure fix
            //                        - Quick Create user message fix

            //1.9 Build 7 - 08/19/09 - MikeE - Fixed Printing of crystal reports with Public snyononyms.
            //                         AddedControl support for ThemeableAttribute PLCWEB report editing OCX control.

            //1.9 Build 8 - 08/19/09 - MikeE - Don't call FixRoutine oracle....

            //1.9 Build 9 - 08/20/09 - MikeE - AAC Weekend Update for Lists

            //1.9 Build 11 - 09-08-2009 - MikeE - AAC Weekend Update for 090409

            //1.9 Build 12 - 09-09-2009 - MikeE - Removed HostRegistery Access
            //1.9 bUILD 12 - 09-11-2009 - Julius
            //1.9 bUILD 13 - 09-11-2009 - 12 already used... Just bumped it up.
            //1.9 bUILD 14 - 09-14-2009 - Some Changes to the inventoryMulti to make it work in oracle.
            //1.9 Build 15 - 09-16-2009 - refer to release notes 091809 (interim build)
            //1.9 Build 16 - 09-18-2009 - refer to release notes 091809
            //1.9 Build 17 - 09-18-2009 - Mike - Changes to Inventory and CodeHead.ascx - Also added new barcode printing unit. - (Not turned on yet.)
            //1.9 Build 18 - 09-18-2009 - MikeE - Printing Item Barcodes through web service on the item tab.
            //1.9 Build 19 - 10-02-2009 - refer to release notes 100209
            //1.9 Build 20 - 10-07-2009 - User Preferences
            //1.9 Build 21 - 10-09-2009 - refer to release notes 100909
            //1.9 Build 22 - 10-16-2009 - refer to release notes 101609
            //1.9 Build 23 - 10-19-2009 - Build sent to LA. Includes custom report editing.
            //1.9 Build 25  - x86  test Build for WebServer.
            //1.9 Build 26 - 10-23-2009 - refer to release notes 102309, Plus fixes to BadBarCodeScan (MikeE)
            //1.9 Build 27 - 11-06-2009 - refer to release notes 110609
            //1.9 Build 28 - 11-09-2009 - Fix for Config Report Groups...
            //1.9 Build 29 - 11-11-2009 - Fix for Printing case lbl, dbpanel comboboxes, and  CaseTitle
            //1.9 Build 30 - 11-13-2009 - refer to release notes 111309
            //1.9 Build 31 - 11-13-2009 - Combobox sort order temp fix to dbpanel, some changes to photorequest
            //1.9 Build 32 - 11-17-2009 - ServiceRequestQuery Problem SRD.SR_MASTER_KEY in sub query
            //1.9 Build 34 - 11-17-2009 - ServiceRequestQuery Problem SRD.SR_MASTER_KEY in sub query
            //1.9 Build 35 - 11-24-2009 - refer to release notes 112409
            //Mike Build 36 to LA. Changes to quickcreateadvanced to use submissionstab if uses_book_cases is set..
            //1.9 Build 37 - 12-04-2009 - refer to release notes 120409
            //1.9 Build 38 - 12-04-2009 - Added support for scanning to imagevault. (Not finished)
            //1.9 Build 39 - 12-07-2009 - Crystal Repor Export #21293
            //1.9 Build 40 - 12-07-2009 - Photo Fixes including Media Type
            //1.9 Build 41 - 12-15-2009 - refer to release notes 121509
            //1.9 Build 42 - 12-17-2009 - Fixed PhotoRequest.cs and rptviewer.cs
            //1.9 Build 43 - 12-22-2009 - refer to release notes 122209
            //1.9 Build 44 - 12-32-2009 - Additional Changes for Bulk intake
            //1.9 Build 45 - 12-32-2009 - Additional Changes for Bulk intake
            //1.9 Build 46 - 01-12-2010 - refer to release notes 011210
            //test PDFVIEW for LA rpt viewing 47
            // Build 48 - fix for case lock on dashboard
            //Build 49 Has changes to crystal report printing 
            //     USES_PDF_VIEWERFLAG F = rptviewer, T = Acrobat Viewer, P = Acrobat Viewer as a Popup
            //Build 50 - Changed Photo Request changing to digital imaging. Added Uses_RD to QuickCreate.aspx
            //1.9 Build 52 - 01-21-2010 - refer to release notes 012110
            //Build 53 - Narco ServiceRequests
            //1.9 Build 54 - 02-02-2010 - refer to release notes 020210
            //1.9 Build 60 - 02-22-2010 - refer to release notes 022210
            // Build 60 + Editable comoboxes + Fix to Adding case to recent.
            //1.9 Build 62 - 03-03-2010 - refer to release notes 030310
            //1.9 Build 63 - 03-05-2010 - refer to release notes 030510
            //1.9 Build 64 - ValidateSql() in PLCQuery
            //1.9 Build 65 - Increment build number to test build and deploy scripts.
            //1.9 Build 66 - Increment build number to test build and deploy scripts. -MikeE 3/15/2010 Beware the Idus Martiae
            //1.9 Build 67 - 03-16-2010 - refer to release notes 031610
            //1.9 Build 68 - 03-17-2010 - Change to the UserControl for EPAD. so it wont slow down./
            //1.9 Build 73 - 03-25-2010 - refer to release notes 032510
            //1.9 Build 78 - 04-07-2010 - refer to release notes 040710
            //1.9 Build 83 - 04-16-2010 - EPAD Resolution
            //1.9 Build 86 - 04-19-2010 - refer to release notes 041910
            //1.9 Build 88 - 04-28-2010 - refer to release notes 042810
            //1.9 Build 94 - 05-11-2010 - refer to release notes 051110
            //1.9 Build 98 - 05-21-2010 - refer to release notes 052110
            //1.9 Build 101 - 06-01-2010 - refer to release notes 060110
            //1.9 Build 103 - 06-10-2010 - refer to release notes 061010
            //1.9 Build 106 - 06-22-2010 - refer to release notes 062210
            //1.9 Build 112 - 07-02-2010 - refer to release notes 070210
            //1.9 Build 119 - 07-13-2010 - refer to release notes 071310
            //1.9 Build 123 - 07-22-2010 - refer to release notes 072210
            //1.9 Build 127 - 07-30-2010 - refer to release notes 073010
            //1.9 Build 132 - 08-10-2010 - refer to release notes 081010
            //1.9 Build 136 - 08-20-2010 - refer to release notes 082010
            //Build 135 - Includes new PrintList function in webservice for QuickCreate,

            TheSession().Clear();

            PLCSessionVars.CurrentRev = GetCurrentRev(server);
            PLCBEASTiLIMSVersion = "Master Build " + PLCSessionVars.CurrentRev;

            PLCTransferECN = "";
            PLCDBProvider = "";    //ORACLE.OleDB.Oracle
            PLCDBDataSource = "";  //ORACLE (in TNSNAMES.ORA)
            PLCDBName = "";        //PRODUCTION (in DATABASE.INI
            PLCDBPW = "";          //******
            PLCDBUserID = "";      //LABSYS
            PLCDBDatabase = "";

            PLCRPTFilePath = "";
            PLCLBLFilePath = "";
            PLCDatabaseServer = "ORACLE";

            PLCGlobalPrelogUser = "";
            PLCGlobalAnalyst = "";  //MIKE
            PLCDomainName = "";
            PLCDomainPrefix = "";
            PLCDomainUserName = "";
            PLCGlobalAnalystPassword = "";
            PLCGlobalAnalystName = ""; //Mike Smith
            PLCGlobalLabCode = ""; //LAB
            PLCGlobalLabName = ""; //Forensic Laboratory
            PLCQCDepartmentCode = ""; //DEPTNAME.Department Code
            PLCGlobalAnalystDepartmentCode = ""; //DEPTNAME.Department Code
            PLCGlobalAnalystDepartmentName = "";
            PLCGlobalDefaultAnalystCustodyOf = "";
            PLCGlobalAnalystGroup = "";


            //Lab Case vars
            PLCGlobalSelectedGridIdx = "-1";
            PLCGlobalSelectedGrid = "";
            PLCGlobalAttachmentSource = "";
            PLCGlobalCaseKey = "";
            PLCGlobalURN = "";
            PLCGlobalSubmissionKey = "";
            PLCGlobalECN = "";
            PLCGlobalTaskID = "";
            PLCGlobalLabCase = "";
            PLCGlobalDepartmentCaseNumber = "";
            PLCGlobalStatusKey = "";
            PLCGlobalBatchKey = "";
            PLCGlobalNameKey = "";
            PLCGlobalAnalystSearchKey = "";
            PLCGlobalNameNumber = "";
            PLCGlobalAssignmentKey = "";
            PLCGlobalAttachmentKey = "";
            PLCNewCaseKey = "";
            PLCNewLastName = "";
            PLCNewFirstName = "";
            PLCAdditionalSubmissionKey = "";
            PLCGlobalSubmissionNumber = "";
            PLCSelectedBulkContainerKey = "";
            PLCSelectedTemplateKey = "";
            PLCSelectedWorksheetKey = "";
            PLCWebOCXSource = "";

            PLCCrystalReportAutoPrint = false;
            PLCCrystalReportLabelPrinter = "";
            PLCCrystalReportName = "";  //case.rpt
            PLCCrystalSelectionFormula = ""; //{LABCASE.Case Key} = 1234
            PLCCrystalReportTitle = "";  //Case Jacket for F08-1234
            PLCCrystalCustomReportKey = "";

            InitPLCCrystalInputs();

            ClearError();
            ClearLabCaseVars();

        }

        private void InitPLCCrystalInputs()
        {
            int maxCrystalInput = PLCSession.SafeInt(System.Configuration.ConfigurationManager.AppSettings.Get("MaxPLCCrystalInput"), 5);
            maxCrystalInput = Math.Max(maxCrystalInput, 5);
            PLCCrystalInputs = new PLCCrystalInputParams[maxCrystalInput];
            for (int iSel = 0; iSel < maxCrystalInput; iSel++)
                PLCCrystalInputs[iSel] = new PLCCrystalInputParams("", "", "", "", "");
            PLCSession.WriteDebug("MaxPLCCrystalInput: " + maxCrystalInput + "; Current count: " + PLCCrystalInputs.Length);
        }

        // Return " / Name: LastName, FirstName"
        private string GetNameInTitle(PLCQuery queryNames, string namePrefix, string lastNameField, string firstNameField)
        {
            string retName = String.Empty;

            string lastName = queryNames.FieldByName(lastNameField).Trim();
            string firstName = queryNames.FieldByName(firstNameField).Trim();

            // Ex. " / Suspect: LastName, FirstName"
            if ((lastName.Length == 0) && (firstName.Length == 0))
                return retName;

            retName += String.Format(" / {0}: ", namePrefix);           // " / Suspect:"
            if (lastName.Length > 0)                                    // " / Suspect: Last Name, First Name"
            {
                retName += lastName;
                if (firstName.Length > 0)
                    retName += String.Format(", {0}", firstName);
            }
            else
            {
                retName += firstName;                                   // " / Suspect: First Name"
            }

            return retName;
        }

        // Output name with trailing or leading separator. Ex. " / Name"
        private string SeparatorName(string name, string sep)
        {
            if (name.Trim().Length > 0)        // Don't return anything if field is empty.
                return String.Format("{0}{1}", sep, name.Trim());
            else
                return String.Empty;
        }

        // Output name with trailing separator. Ex. " / Name"
        private string SeparatorName(string name)
        {
            return SeparatorName(name, " / ");
        }

        public string GetCaseTitle()
        {
            // Cache case title for 1 minute to minimize database queries when browsing around the tab pages.
            string caseTitleCacheKey = "CASETITLE@@@" + this.PLCGlobalCaseKey;
            string caseTitle = (string)CacheHelper.GetItem(caseTitleCacheKey);
            if (caseTitle == null)
            {
                caseTitle = String.Format("{2}: <span class=\"casetitlebase\">{0}</span><span class=\"casetitleaddl\">{1}</span>", GetBaseCaseTitle(), GetCaseDiscrepancyText(), (PLCSession.GetLabCtrl("SHOW_DEPT_CASE_TEXT_HEADER") == "T" ? PLCSession.GetLabCtrl("DEPT_CASE_TEXT") : ""));
                CacheHelper.AddItem(caseTitleCacheKey, caseTitle, TimeSpan.FromMinutes(1));
            }

            return caseTitle;
        }

        // Get case discrepancy text for case title.
        private string GetCaseDiscrepancyText()
        {
            string retText = "";

            if (!String.IsNullOrEmpty(PLCSession.PLCGlobalCaseKey))
            {
                PLCQuery qry = new PLCQuery(
@"select a.PROMPT_MSG as PROMPT_MSG
from tv_labitem i
inner join tv_itemattr ia on ia.ITEM_TYPE = i.ITEM_TYPE
inner join tv_attrib a on a.ATTRIBUTE = ia.ATTRIBUTE
left outer join tv_attrcode attrcode on (attrcode.EVIDENCE_CONTROL_NUMBER = i.EVIDENCE_CONTROL_NUMBER) and (attrcode.ATTRIBUTE = ia.ATTRIBUTE) 
where
((a.PROMPT_MSG is not null) or (a.PROMPT_MSG <> '')) and 
((attrcode.VALUE is not null) or (attrcode.VALUE <> '')) and 
(i.CASE_KEY = ?)"
                );

                qry.AddParameter("CASE_KEY", PLCSession.PLCGlobalCaseKey);
                qry.Open();

                if (!qry.IsEmpty())
                    retText = qry.FieldByName("PROMPT_MSG");
            }

            return retText;
        }

        // Get Case Title displayed above Case menu. Ex. "Case Info for 406-03925-0284-061 / B-2009-0113 / TheCaseName"
        private string GetBaseCaseTitle()
        {
            if (String.IsNullOrEmpty(PLCGlobalCaseKey))
                return "";

            string retTitle = String.Empty;


            String caseTitleProc = PLCSession.GetLabCtrl("CASE_TITLE_PROC");
            //caseTitleProc = "GETCASETITLE";
            if (!String.IsNullOrWhiteSpace(caseTitleProc))
            {

                try
                {
                    PLCQuery qryCaseTitle = new PLCQuery();
                    qryCaseTitle.AddProcedureParameter("@CASE_KEY", PLCGlobalCaseKey, 10, OleDbType.Integer, ParameterDirection.Input);
                    qryCaseTitle.AddProcedureParameter("@TITLE", 0, 4000, OleDbType.VarChar, ParameterDirection.Output);
                    Dictionary<string, object> spOutput = qryCaseTitle.ExecuteProcedure(caseTitleProc);
                    String thisTitle = Convert.ToString(spOutput["@TITLE"]);
                    return thisTitle;
                }
                catch (Exception ex)
                {
                    PLCSession.WriteDebug("Exception processing  case title proc:" + ex.Message);
                }


            }


            PLCQuery query = new PLCQuery();
            query.SQL = "SELECT C.*, D.DEPARTMENT_NAME FROM TV_LABCASE C LEFT OUTER JOIN TV_DEPTNAME D ON C.DEPARTMENT_CODE = D.DEPARTMENT_CODE WHERE C.CASE_KEY = :CKEY";
            query.SetParam("CKEY", PLCGlobalCaseKey);
            query.Open();
            if (query.IsEmpty())
                return retTitle;


            PLCSessionVars sv = new PLCSessionVars();
            if (sv.GetLabCtrl("SHOW_DEPARTMENT_TITLE") == "T")
            {
                // Add Dept Name
                if (GetLabCtrl("SHOW_DEPT_NAME_IN_TITLE") == "T")
                {
                    retTitle += SeparatorName(query.FieldByName("DEPARTMENT_NAME"));
                }
                return HttpUtility.HtmlEncode(query.FieldByName("DEPARTMENT_CASE_NUMBER")) + retTitle;
            }

            //$$ Similar (a) and (b) conditions below could be simplified.
            if (GetLabCtrl("SHOW_LAB_CASE_NUMBER") == "T")
            {
                // (a) With LAB_CASE
                if (GetLabCtrl("USES_PROPERTY_CONTROL") == "U")     // "URN# / Property Control Number / Lab Case"
                    retTitle = String.Format("{0}{1}{2}", query.FieldByName("DEPARTMENT_CASE_NUMBER"), SeparatorName(query.FieldByName("PROPERTY_CONTROL_NUMBER")), SeparatorName(query.FieldByName("LAB_CASE")));
                else if (GetLabCtrl("USES_PROPERTY_CONTROL") == "T")
                    retTitle = String.Format("{0}{1}", SeparatorName(query.FieldByName("PROPERTY_CONTROL_NUMBER"), "PC# "), SeparatorName(query.FieldByName("LAB_CASE"), " / Lab# "));
                else if (query.FieldByName("LAB_CASE").Trim().Length > 0)  // "Lab Case / URN #"
                    retTitle = String.Format("{0}{1}", query.FieldByName("LAB_CASE"), SeparatorName(query.FieldByName("DEPARTMENT_CASE_NUMBER")));
                else
                    retTitle = query.FieldByName("DEPARTMENT_CASE_NUMBER");
            }
            else
            {
                // (b) Without LAB_CASE
                //     This is the same as (a), but with LAB_CASE removed.
                if (GetLabCtrl("USES_PROPERTY_CONTROL") == "U")     // "URN# / Property Control Number / Lab Case"
                    retTitle = String.Format("{0}{1}", query.FieldByName("DEPARTMENT_CASE_NUMBER"), SeparatorName(query.FieldByName("PROPERTY_CONTROL_NUMBER")));
                else if (GetLabCtrl("USES_PROPERTY_CONTROL") == "T")
                    retTitle = String.Format("{0}", SeparatorName(query.FieldByName("PROPERTY_CONTROL_NUMBER"), "PC# "));
                else
                    retTitle = query.FieldByName("DEPARTMENT_CASE_NUMBER");
            }

            if (query.FieldByName("CASE_NAME").Trim().Length > 0)
            {
                retTitle += SeparatorName(query.FieldByName("CASE_NAME"));  // " / Case Name"

                // Add Dept Name
                if (GetLabCtrl("SHOW_DEPT_NAME_IN_TITLE") == "T")
                {
                    retTitle += SeparatorName(query.FieldByName("DEPARTMENT_NAME"));
                }

                return HttpUtility.HtmlEncode(retTitle);
            }

            if (GetLabCtrl("PREFER_DEPARTMENT_CASE") == "T")
                retTitle = query.FieldByName("DEPARTMENT_CASE_NUMBER");

            PLCQuery queryNames = new PLCQuery();
            queryNames.SQL = "select * from TV_LABNAME where CASE_KEY = :CKEY order by CASE_KEY, NUMBER_RES";
            queryNames.SetParam("CKEY", PLCGlobalCaseKey);
            queryNames.Open();

            string nameInTitle = String.Empty;
            while (!queryNames.EOF())
            {
                // Suspect name
                if ((nameInTitle == String.Empty) && (queryNames.FieldByName("NAME_TYPE") == "S"))
                    nameInTitle = GetNameInTitle(queryNames, "Suspect", "LAST_NAME", "FIRST_NAME");

                // Victim name
                if ((nameInTitle == String.Empty) && (queryNames.FieldByName("NAME_TYPE").Trim() == "V"))
                    nameInTitle = GetNameInTitle(queryNames, "Name", "LAST_NAME", "FIRST_NAME");

                if (nameInTitle.Length > 0)
                    break;

                queryNames.Next();
            }

            if (GetLabCtrl("DISPLAY_FIRST_NAME_IN_TITLE") == "T")
            {
                PLCQuery qryName = new PLCQuery();
                qryName.SQL = "SELECT * FROM TV_LABNAME WHERE CASE_KEY = :CKEY ORDER BY NUMBER_RES";
                qryName.SetParam("CKEY", PLCGlobalCaseKey);
                qryName.Open();

                if (!qryName.IsEmpty())
                {
                    nameInTitle = GetNameInTitle(qryName, "Name", "LAST_NAME", "FIRST_NAME");
                }

            }


            // If Locations in Title
            if (GetLabCtrl("LOCATIONS_IN_TITLE") == "T")
                nameInTitle = GetItemLocations(query.FieldByName("CASE_KEY"));
            else if (nameInTitle == String.Empty)
                // " / Location: location desc"
                nameInTitle = SeparatorName(query.FieldByName("OFFENSE_LOCATION"), " / Location: ");

            retTitle += nameInTitle;

            // Add Dept Name
            if (GetLabCtrl("SHOW_DEPT_NAME_IN_TITLE") == "T")
            {
                retTitle += SeparatorName(query.FieldByName("DEPARTMENT_NAME"));
            }

            return HttpUtility.HtmlEncode(retTitle);
        }

        // Get distinct item custodies for case title.
        private string GetItemLocations(string caseKey)
        {
            string retItemLocations = String.Empty;

            PLCQuery query = new PLCQuery();
            query.SQL = "select distinct CUSTODY_OF from tv_labitem where LAB_ITEM_NUMBER != '0' and CASE_KEY = :CKEY";
            query.SetParam("CKEY", caseKey);
            query.Open();

            while (!query.EOF())
            {
                if (retItemLocations.Length > 0)
                    retItemLocations += ",";
                retItemLocations += query.FieldByName("CUSTODY_OF");
                query.Next();
            }

            if (retItemLocations.Length > 0)
                retItemLocations = " Stored In: " + retItemLocations;

            return retItemLocations;
        }

        public string GetTableOwnerforCrystal(string tblname)
        {
            if (PLCDatabaseServer == "ORACLE")
            {
                PLCQuery qry = new PLCQuery();
                qry.SQL = "select * from ALL_SYNONONYMS where TABLE_NAME = :TBLNAME";
                qry.SetParam("TBLNAME", tblname);
                qry.Open();




            }
            else
            {
                return "";
            }

            return "";

        }

        public string GetConnectionString()
        {
            return GetConnectionString(null, null);
        }

        public string GetConnectionString(string dbnameOverride, string datasourceOverride)
        {

            String theDatabaseName = "";
            String theDataSource = "";
            String theLimsSession = GetDefault("LIMS_SESSION");

            if (String.IsNullOrEmpty(theLimsSession))
                {
                theLimsSession = "LIMS-" + Guid.NewGuid().ToString();
                SetDefault("LIMS_SESSION",theLimsSession);
                }

            

            if (!String.IsNullOrEmpty(datasourceOverride))
                theDataSource = datasourceOverride;
            else
                theDataSource = PLCDBDataSource;


            if (!String.IsNullOrEmpty(dbnameOverride))
                theDatabaseName = dbnameOverride;
            else
                theDatabaseName = PLCDBDatabase;



            if (String.IsNullOrEmpty(dbnameOverride) && String.IsNullOrEmpty(datasourceOverride) && (this.ConnectionString != null))
            {
                return this.ConnectionString;
            }
            else
            {
                string tempstr = "";

                if (PLCDatabaseServer == "MSSQL")
                {

                    if (PLCDBProvider.StartsWith("MSOLEDBSQL"))
                    {
                        //DataTypeCompatibility=80; needed to support DATETIME COLUMN TYPES
                        tempstr = "Provider={4};Server={0}; DataTypeCompatibility=80; Database={1}; User ID={2}; Password={3};";
                        tempstr = String.Format(tempstr, theDataSource, theDatabaseName, PLCDBUserID, PLCDBPW, PLCDBProvider);
                    }
                    else
                    {
                        tempstr = "Provider=" + this.PLCDBProvider + ";";
                        tempstr += "Data Source=" + theDataSource + ";";
                        tempstr += "Initial Catalog=" + theDatabaseName + ";";
                        tempstr += "User ID=" + PLCDBUserID + ";";
                        tempstr += "Password=" + PLCDBPW + ";";
                        tempstr += "Application Name=" + theLimsSession + ";";
                    }

                }
                else
                {
                    tempstr = "Provider=" + this.PLCDBProvider + ";";
                    tempstr += "Data Source=" + theDataSource + ";";
                    tempstr += "User ID=" + PLCDBUserID + ";";
                    tempstr += "Password=" + PLCDBPW + ";";
                    // how to do thisinoracle
                    //       tempstr += "Application Name=" + theLimsSession + ";";

                    //   tempstr += "Min Pool Size=20;Connection Lifetime=120;Connection Timeout=60;Incr Pool Size=10;Decr Pool Size=2;";

                    //tempstr += "PLSQLRSet=1;";
                }

                String extraParams = System.Configuration.ConfigurationManager.AppSettings.Get("EXTRA_CONNECTION_PARAMS");
                if (!String.IsNullOrWhiteSpace(extraParams))
                    tempstr += " " + extraParams;

                // Save to session and return connection string.
                if (String.IsNullOrEmpty(dbnameOverride) && String.IsNullOrEmpty(datasourceOverride))
                    this.ConnectionString = tempstr;

                return tempstr;
            }
        }

        public string GetCINFOConnectionString(string CINFO, out string dbServer)
        {
            dbServer = string.Empty;
            string dataSource = string.Empty;
            string dbUserID = string.Empty;
            string dbUserPwd = string.Empty;
            string dbName = string.Empty;
            string dbIP = string.Empty;
            string dbDomainName = string.Empty;
            string dbDomainPrefix = string.Empty;
            string dbProvider = string.Empty;

            GetConnectionFromCINFO(CINFO, out dataSource, out dbUserID, out dbUserPwd, out dbName, out dbServer, out dbIP, out dbDomainName, out dbDomainPrefix, out dbProvider);

            string tempstr = "";

            if (dbServer == "MSSQL")
            {

                if (dbProvider.StartsWith("MSOLEDBSQL"))
                {
                    //DataTypeCompatibility=80; needed to support DATETIME COLUMN TYPES
                    tempstr = "Provider={4};Server={0}; DataTypeCompatibility=80; Database={1}; User ID={2}; Password={3};";
                    tempstr = String.Format(tempstr, dataSource, dbName, dbUserID, dbUserPwd, dbProvider);
                }
                else
                {
                    tempstr = "Provider=" + dbProvider + ";";
                    tempstr += "Data Source=" + dataSource + ";";
                    tempstr += "Initial Catalog=" + dbName + ";";
                    tempstr += "User ID=" + dbUserID + ";";
                    tempstr += "Password=" + dbUserPwd + ";";
                }

            }
            else
            {
                tempstr = "Provider=" + dbProvider + ";";
                tempstr += "Data Source=" + dataSource + ";";
                tempstr += "User ID=" + dbUserID + ";";
                tempstr += "Password=" + dbUserPwd + ";";
            }

            return tempstr;
        }

        // Return connection string from session if it exists, or create a new connection string and return it.
        public string GetConnectionString_OLD(string dbnameOverride, string datasourceOverride)
        {
            if (String.IsNullOrEmpty(dbnameOverride) && String.IsNullOrEmpty(datasourceOverride) && (this.ConnectionString != null))
            {
                return this.ConnectionString;
            }
            else
            {
                string tempstr = "";

                if (PLCDatabaseServer == "MSSQL")
                {
                    tempstr = "Provider=" + this.PLCDBProvider + ";";

                    // Use datasource override if one was passed.
                    if (!String.IsNullOrEmpty(datasourceOverride))
                        tempstr += "Data Source=" + datasourceOverride + ";";
                    else
                        tempstr += "Data Source=" + PLCDBDataSource + ";";

                    // Use database override if one was passed.
                    if (!String.IsNullOrEmpty(dbnameOverride))
                        tempstr += "Initial Catalog=" + dbnameOverride + ";";
                    else
                        tempstr += "Initial Catalog=" + PLCDBDatabase + ";";

                    tempstr += "User ID=" + PLCDBUserID + ";";
                    tempstr += "Password=" + PLCDBPW + ";";
                }
                else
                {
                    tempstr = "Provider=" + this.PLCDBProvider + ";";

                    // Use datasource override if one was passed.
                    if (!String.IsNullOrEmpty(datasourceOverride))
                        tempstr += "Data Source=" + datasourceOverride + ";";
                    else
                        tempstr += "Data Source=" + PLCDBDataSource + ";";

                    tempstr += "User ID=" + PLCDBUserID + ";";
                    tempstr += "Password=" + PLCDBPW + ";";
                    //   tempstr += "Min Pool Size=20;Connection Lifetime=120;Connection Timeout=60;Incr Pool Size=10;Decr Pool Size=2;";

                    //tempstr += "PLSQLRSet=1;";
                }

                // Save to session and return connection string.
                if (String.IsNullOrEmpty(dbnameOverride) && String.IsNullOrEmpty(datasourceOverride))
                    this.ConnectionString = tempstr;

                return tempstr;
            }
        }

        // OracleClient.OracleConnection and OracleClient.OracleDataReader requires a connection string that doesn't contain the 'Provider=' section.
        public string GetOracleDataReaderConnectionString()
        {
            string tempstr = "";
            tempstr += "Data Source=" + PLCDBDataSource + ";";
            tempstr += "User ID=" + PLCDBUserID + ";";
            tempstr += "Password=" + PLCDBPW + ";";
            return tempstr;
        }

        public string GetDataSourceConnectionString_XXX() //Not used use the getconnection string
        {
            string tempstr = "";
            tempstr += "Data Source=" + PLCDBDataSource + ";";
            tempstr += "User ID=" + PLCDBUserID + ";";
            tempstr += "Password=" + PLCDBPW + ";";
            return tempstr;
        }

        public string GetDefault(string settingname)
        {
            //if (ThePage() == (Page)null) return "";
            string token = "APPDEFAULT_" + settingname;
            token = token.Trim();
            token = token.ToUpper();
            token = token.Replace(" ", "");
            if (TheSession()[token] == null)
                return "";
            else
            {
                string s = (string)TheSession()[token];
                return s;
            }
        }

        public string GetDefaultAsIs(string settingname)
        {
            //if (ThePage() == (Page)null) return "";
            string token = "APPDEFAULT_" + settingname;
            token = token.Trim();
            token = token.Replace(" ", "");
            if (TheSession()[token] == null)
                return "";
            else
            {
                string s = (string)TheSession()[token];
                return s;
            }
        }

        public void SetDefault(string settingname, string value)
        {
            //if (ThePage() == (Page)null)
            //    return;
            string token = "APPDEFAULT_" + settingname;
            token = token.Trim();
            token = token.ToUpper();
            token = token.Replace(" ", "");
            TheSession()[token] = value;
        }

        public void SetDefaultAsIs(string settingname, string value)
        {
            if (ThePage() == (Page)null)
                return;
            string token = "APPDEFAULT_" + settingname;
            token = token.Trim();
            token = token.Replace(" ", "");
            TheSession()[token] = value;
        }

        // DB Connection string stored in session.
        public string ConnectionString
        {
            get
            {
                if (TheSession()["ConnectionString"] != null)
                    return (string)TheSession()["ConnectionString"];
                else
                    return null;
            }

            set
            {
                TheSession()["ConnectionString"] = value;
            }
        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public AndroidTransferConnector CurrentAndroidTransferConnector
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["CurrentAndroidTransferConnector"] == null)
                    return null;
                else
                {
                    AndroidTransferConnector s = (AndroidTransferConnector)TheSession()["CurrentAndroidTransferConnector"];
                    return s;
                }

            }

            set
            {
                TheSession()["CurrentAndroidTransferConnector"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue(false)]
        [Localizable(true)]
        public Boolean PLCQueryLog
        {
            get
            {
                if (ThePage() == (Page)null)
                    return false;

                if (TheSession()["PLCQueryLog"] == null)
                    return false;
                else
                {
                    Boolean b = (Boolean)TheSession()["PLCQueryLog"];
                    return b;
                }
            }
            set
            {
                TheSession()["PLCQueryLog"] = value;
            }
        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue(null)]
        [Localizable(true)]
        public OleDbConnection SessionConnection
        {
            get
            {
                if (ThePage() == (Page)null)
                    return null;

                if (TheSession()["SessionConnection"] == null)
                    return null;
                else
                {
                    OleDbConnection c = (OleDbConnection)TheSession()["SessionConnection"];
                    return c;
                }
            }
            set
            {
                TheSession()["SessionConnection"] = value;
            }
        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCDBProvider
        {
            get
            {
                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCDBProvider"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCDBProvider"];
                    return s;
                }

            }
            set
            {
                TheSession()["PLCDBProvider"] = value;
            }
        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCBEASTiLIMSVersion
        {
            get
            {
                if (TheSession()["PLCBEASTiLIMSVersion"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCBEASTiLIMSVersion"];
                    return s;
                }
            }
            set
            {
                TheSession()["PLCBEASTiLIMSVersion"] = value;
            }
        }

        [DefaultValue("")]
        [Localizable(true)]
        public string PLCCrystalReportName
        {

            get
            {
                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCCrystalReportName"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCCrystalReportName"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCCrystalReportName"] = value;
            }

        }

        [DefaultValue("")]
        [Localizable(true)]
        public string PLCCrystalReportComments
        {

            get
            {
                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCCrystalReportComments"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCCrystalReportComments"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCCrystalReportComments"] = value;
            }

        }

        [DefaultValue("")]
        [Localizable(true)]
        public string PLCCrystalReportFormulaList
        {
            get
            {
                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCCrystalReportFormulaList"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCCrystalReportFormulaList"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCCrystalReportFormulaList"] = value;
            }

        }

        [DefaultValue(false)]
        [Localizable(true)]
        public Boolean PLCCrystalReportAutoPrint
        {
            get
            {
                if (ThePage() == (Page)null)
                    return false;

                if (TheSession()["PLCCrystalReportAutoPrint"] == null)
                    return false;
                else
                {
                    Boolean b = (Boolean)TheSession()["PLCCrystalReportAutoPrint"];
                    return b;
                }

            }

            set
            {
                TheSession()["PLCCrystalReportAutoPrint"] = value;
            }

        }

        [DefaultValue("")]
        [Localizable(true)]
        public string PLCCrystalReportLabelPrinter
        {

            get
            {
                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCCrystalReportLabelPrinter"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCCrystalReportLabelPrinter"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCCrystalReportLabelPrinter"] = value;
            }

        }

        [DefaultValue("")]
        [Localizable(true)]
        public string PLCCrystalCustomReportKey
        {
            get
            {
                if (TheSession()["PLCCrystalCustomReportKey"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCCrystalCustomReportKey"];
                    return s;
                }
            }
            set
            {
                TheSession()["PLCCrystalCustomReportKey"] = value;
            }
        }

        [DefaultValue("")]
        [Localizable(true)]
        public string PLCErrorMessage
        {

            get
            {


                if (TheSession()["PLCErrorMessage"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCErrorMessage"];
                    return s;
                }

            }

            set
            {
                try
                {
                    TheSession()["PLCErrorMessage"] = value;
                }
                catch
                { }
            }

        }

        [DefaultValue("")]
        [Localizable(true)]
        public string PLCErrorSQL
        {

            get
            {
                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCErrorSQL"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCErrorSQL"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCErrorSQL"] = value;
            }

        }

        [DefaultValue("")]
        [Localizable(true)]
        public string PLCErrorURL
        {

            get
            {
                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCErrorURL"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCErrorURL"];
                    return s;
                }

            }

            set
            {
                if (value == "*")
                {

                    if (ThePage() != null)
                    {
                        try
                        {
                            value = ThePage().Request.Url.ToString();
                        }
                        catch
                        {
                        }

                    }
                }

                TheSession()["PLCErrorURL"] = value;
            }

        }

        [DefaultValue("")]
        [Localizable(true)]
        public string PLCErrorProc
        {

            get
            {
                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCErrorProc"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCErrorProc"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCErrorProc"] = value;
            }

        }

        [DefaultValue("")]
        [Localizable(true)]
        public string PLCCrystalReportTitle
        {

            get
            {
                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCCrystalReportTitle"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCCrystalReportTitle"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCCrystalReportTitle"] = value;
            }

        }

        [DefaultValue("")]
        [Localizable(true)]
        public string PLCCrystalSelectionFormula
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCCrystalSelectionFormula"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCCrystalSelectionFormula"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCCrystalSelectionFormula"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCDBDataSource
        {

            get
            {
                if (TheSession()["PLCDBDataSource"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCDBDataSource"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCDBDataSource"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCDBUserID
        {

            get
            {


                if (TheSession()["PLCDBUserID"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCDBUserID"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCDBUserID"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCDBDatabase
        {

            get
            {
                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCDBDatabase"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCDBDatabase"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCDBDatabase"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCTransferLoc
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCTransferLoc"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCTransferLoc"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCTransferLoc"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCTransferECN
        {
            get
            {
                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCTransferECN"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCTransferECN"];
                    return s;
                }
            }
            set
            {
                TheSession()["PLCTransferECN"] = value;
            }
        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCTransferItems
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCTransferItems"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCTransferItems"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCtransferItems"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCDBPW
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCDBPW"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCDBPW"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCDBPW"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCDBName
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession() == null)
                    return "";

                if (TheSession()["PLCDBName"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCDBName"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCDBName"] = value;

                // Add database name to list of available database names.
                AddToAvailableDBNames(value);
            }

        }

        private void AddToAvailableDBNames(string dbname)
        {
            // If application session var doesn't exist, create it.
            if (TheApplication()["PLCDBServerNameList"] == null)
                TheApplication()["PLCDBServerNameList"] = new Dictionary<string, string>();

            // Add dbserver to available db server list.
            Dictionary<string, string> dictDBServers = ((Dictionary<string, string>)TheApplication()["PLCDBServerNameList"]);
            if (!dictDBServers.ContainsKey(dbname))
                dictDBServers.Add(dbname, "");
        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalAnalyst
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalAnalyst"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalAnalyst"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalAnalyst"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue(false)]
        [Localizable(true)]
        public Boolean PLCGlobalProficiencyCaseNoItems
        {
            get
            {
                //if (ThePage() == (Page)null)
                //return false;

                if (TheSession()["PLCGlobalProficiencyCaseNoItems"] == null)
                    return false;
                else
                {
                    Boolean b = (Boolean)TheSession()["PLCGlobalProficiencyCaseNoItems"];
                    return b;
                }
            }
            set
            {
                TheSession()["PLCGlobalProficiencyCaseNoItems"] = value;
            }
        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalReviewType
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalReviewType"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalReviewType"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalReviewType"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCDomainName
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCDomainName"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCDomainName"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCDomainName"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCDomainPrefix
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCDomainPrefix"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCDomainPrefix"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCDomainPrefix"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCDomainUserName
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCDomainUserName"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCDomainUserName"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCDomainUserName"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCLoginMode
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCLoginMode"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCLoginMode"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCLoginMode"] = value;
            }

        }

#region Prelog Global Variables

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalPrelogUser
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalPrelogUser"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalPrelogUser"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalPrelogUser"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalPrelogCaseKey
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalPrelogCaseKey"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalPrelogCaseKey"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalPrelogCaseKey"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalPrelogDepartmentCaseNumber
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalPrelogDepartmentCaseNumber"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalPrelogDepartmentCaseNumber"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalPrelogDepartmentCaseNumber"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalPrelogDepartmentCode
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalPrelogDepartmentCode"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalPrelogDepartmentCode"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalPrelogDepartmentCode"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalPrelogSubmissionNumber
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalPrelogSubmissionNumber"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalPrelogSubmissionNumber"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalPrelogSubmissionNumber"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalPrelogMasterKey
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalPrelogMasterKey"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalPrelogMasterKey"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalPrelogMasterKey"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalPrelogUserIsAdmin
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalPrelogUserIsAdmin"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalPrelogUserIsAdmin"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalPrelogUserIsAdmin"] = value;
            }

        }

#endregion

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCPrelogCaseBarcode
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCPrelogCaseBarcode"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCPrelogCaseBarcode"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCPrelogCaseBarcode"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCProficiencyTestKey
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCProficiencyTestKey"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCProficiencyTestKey"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCProficiencyTestKey"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalAnalystPassword
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalAnalystPassword"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalAnalystPassword"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalAnalystPassword"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalAnalystGroup
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalAnalystGroup"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalAnalystGroup"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalAnalystGroup"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalDefaultAnalystCustodyOf
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalDefaultAnalystCustodyOf"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalDefaultAnalystCustodyOf"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalDefaultAnalystCustodyOf"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalAnalystName
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalAnalystName"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalAnalystName"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalAnalystName"] = value;
            }

        }

        [Category("PLC Properties"), Description("DEPTNAME.Department Code")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalAnalystDepartmentCode
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalAnalystDepartmentCode"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalAnalystDepartmentCode"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalAnalystDepartmentCode"] = value;
            }

        }

        [Category("PLC Properties"), Description("DEPTNAME.Department Code")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCQCDepartmentCode
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCQCDepartmentCode"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCQCDepartmentCode"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCQCDepartmentCode"] = value;
            }

        }

        [Category("PLC Properties"), Description("DEPTNAME.Department Name")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalAnalystDepartmentName
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalAnalystDepartmentName"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalAnalystDepartmentName"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalAnalystDepartmentName"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalLabCode
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalLabCode"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalLabCode"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalLabCode"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalLabName
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalLabName"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalLabName"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalLabName"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalURN
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalURN"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalURN"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalURN"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalSelectedGrid
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalSelectedGrid"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalSelectedGrid"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalSelectedGrid"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalSelectedGridIdx
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalSelectedGridIdx"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalSelectedGridIdx"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalSelectedGridIdx"] = value;
            }
        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalCaseKey
        {
            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalCaseKey"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalCaseKey"];
                    return s;
                }

            }
            set
            {
                TheSession()["PLCGlobalCaseKey"] = value;
            }
        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCWebOCXSource
        {
            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCWebOCXSource"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCWebOCXSource"];
                    return s;
                }

            }
            set
            {
                TheSession()["PLCWebOCXSource"] = value;
            }
        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCWebOCXWorksheetSource
        {
            get
            {
                if (TheSession()["PLCWebOCXWorksheetSource"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCWebOCXWorksheetSource"];
                    return s;
                }
            }
            set
            {
                TheSession()["PLCWebOCXWorksheetSource"] = value;
            }
        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCSelectedTemplateKey
        {
            get
            {
                if (TheSession()["PLCSelectedTemplateKey"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCSelectedTemplateKey"];
                    return s;
                }
            }
            set
            {
                TheSession()["PLCSelectedTemplateKey"] = value;
            }
        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCSelectedWorksheetKey
        {
            get
            {
                if (TheSession()["PLCSelectedWorksheetKey"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCSelectedWorksheetKey"];
                    return s;
                }
            }
            set
            {
                TheSession()["PLCSelectedWorksheetKey"] = value;
            }
        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue(null)]
        [Localizable(true)]
        public TextBox MyTextBox
        {
            get
            {
                if (ThePage() == (Page)null)
                    return null;

                if (TheSession()["MyTextBox"] == null)
                    return null;
                else
                {

                    return (TextBox)TheSession()["MyTextBox"];
                }

            }

            set
            {
                TheSession()["MyTextBox"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalScannedContainerKey
        {
            get
            {
                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalScannedContainerKey"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalScannedContainerKey"];
                    return s;
                }
            }

            set
            {
                TheSession()["PLCGlobalScannedContainerKey"] = value;
            }
        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalSubmissionKey
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalSubmissionKey"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalSubmissionKey"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalSubmissionKey"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalECN
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalECN"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalECN"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalECN"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalTaskID
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalTaskID"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalTaskID"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalTaskID"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public TransferContainerBox CurrentTransferContainerBox
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["CurrentTransferContainerBox"] == null)
                    return null;
                else
                {
                    TransferContainerBox s = (TransferContainerBox)TheSession()["CurrentTransferContainerBox"];
                    return s;
                }

            }

            set
            {
                TheSession()["CurrentTransferContainerBox"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCSelectedBulkContainerKey
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCSelectedBulkContainerKey"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCSelectedBulkContainerKey"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCSelectedBulkContainerKey"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalNameKey
        {
            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalNameKey"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalNameKey"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalNameKey"] = value;
            }
        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalAnalystSearchKey
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalAnalystSearchKey"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalAnalystSearchKey"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalAnalystSearchKey"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCDBGridEditorSQLString
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCDBGridEditorSQLString"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCDBGridEditorSQLString"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCDBGridEditorSQLString"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalNameNumber
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalNameNumber"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalNameNumber"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalNameNumber"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalStatusKey
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalStatusKey"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalStatusKey"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalStatusKey"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalBatchKey
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalBatchKey"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalBatchKey"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalBatchKey"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalAttachmentSource
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalAttachmentSource"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalAttachmentSource"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalAttachmentSource"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalAttachmentSourceDesc
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalAttachmentSourceDesc"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalAttachmentSourceDesc"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalAttachmentSourceDesc"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalAssignmentKey
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalAssignmentKey"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalAssignmentKey"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalAssignmentKey"] = value;
            }

        }

        public string PLCGlobalAssignmentBatchKey
        {
            get
            {
                if (TheSession()["PLCGlobalAssignmentBatchKey"] == null)
                    return "";
                else
                    return (string)TheSession()["PLCGlobalAssignmentBatchKey"];
            }
            set
            {
                TheSession()["PLCGlobalAssignmentBatchKey"] = value;
            }
        }

        public string PLCGlobalAttachmentKey
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalAttachmentKey"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalAttachmentKey"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalAttachmentKey"] = value;
            }

        }

        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalLabCase
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalLabCase"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalLabCase"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalLabCase"] = value;
            }

        }

        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalDepartmentCaseNumber
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalDepartmentCaseNumber"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalDepartmentCaseNumber"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalDepartmentCaseNumber"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [Localizable(true)]
        public string PLCGlobalKitCustody
        {
            get
            {
                if (TheSession()["PLCGlobalKitCustody"] == null)
                {
                    TheSession()["PLCGlobalKitCustody"] = GetGlobalINIValue("KIT_CUSTODY");
                }
                return (string)TheSession()["PLCGlobalKitCustody"];
            }
        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCAdditionalSubmissionKey
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCAdditionalSubmissionKey"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCAdditionalSubmissionKey"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCAdditionalSubmissionKey"] = value;
            }

        }

        public string PLCNewCaseKey
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCNewCaseKey"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCNewCaseKey"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCNewCaseKey"] = value;
            }

        }

        public string PLCNewLastName
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCNewLastName"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCNewLastName"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCNewLastName"] = value;
            }

        }

        public string PLCNewFirstName
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCNewFirstName"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCNewFirstName"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCNewFirstName"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalSubmissionNumber
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalSubmissionNumber"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalSubmissionNumber"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalSubmissionNumber"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalConfigSourceKey1
        {
            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalConfigSourceKey1"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalConfigSourceKey1"];
                    return s;
                }

            }
            set
            {
                TheSession()["PLCGlobalConfigSourceKey1"] = value;
            }
        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalConfigSourceKey2
        {
            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalConfigSourceKey2"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalConfigSourceKey2"];
                    return s;
                }

            }
            set
            {
                TheSession()["PLCGlobalConfigSourceKey2"] = value;
            }
        }

        public CaseSearchType PLCCaseSearchType
        {
            get
            {
                if (TheSession()["PLCCaseSearchType"] == null)
                    return CaseSearchType.None;
                else
                    return (CaseSearchType)TheSession()["PLCCaseSearchType"];
            }
            set
            {
                TheSession()["PLCCaseSearchType"] = value;
            }
        }

        public string PLCCaseSearchSelect
        {
            get
            {
                if (TheSession()["PLCCaseSearchSelect"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCCaseSearchSelect"];
                    return s;
                }
            }
            set
            {
                TheSession()["PLCCaseSearchSelect"] = value;
            }
        }

        public string PLCCaseSearchFrom
        {
            get
            {
                if (TheSession()["PLCCaseSearchFrom"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCCaseSearchFrom"];
                    return s;
                }
            }
            set
            {
                TheSession()["PLCCaseSearchFrom"] = value;
            }
        }

        public string PLCCaseSearchWhere
        {
            get
            {
                if (TheSession()["PLCCaseSearchWhere"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCCaseSearchWhere"];
                    return s;
                }
            }
            set
            {
                TheSession()["PLCCaseSearchWhere"] = value;
            }
        }

        public string PLCAssignSearchSelect
        {
            get
            {
                if (TheSession()["PLCAssignSearchSelect"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCAssignSearchSelect"];
                    return s;
                }
            }
            set
            {
                TheSession()["PLCAssignSearchSelect"] = value;
            }
        }

        public string PLCAssignSearchFrom
        {
            get
            {
                if (TheSession()["PLCAssignSearchFrom"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCAssignSearchFrom"];
                    return s;
                }
            }
            set
            {
                TheSession()["PLCAssignSearchFrom"] = value;
            }
        }

        public string PLCAssignSearchWhere
        {
            get
            {
                if (TheSession()["PLCAssignSearchWhere"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCAssignSearchWhere"];
                    return s;
                }
            }
            set
            {
                TheSession()["PLCAssignSearchWhere"] = value;
            }
        }

        public string PLCReportSource
        {
            get
            {
                if (TheSession()["PLCReportSource"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCReportSource"];
                    return s;
                }
            }
            set
            {
                TheSession()["PLCReportSource"] = value;
            }
        }

        public PLCCrystalInputParams[] PLCCrystalInputs
        {
            get { return (PLCCrystalInputParams[])TheSession()["PLCCrystalInputs"]; }
            set { TheSession()["PLCCrystalInputs"] = value; }
        }

        public string PLCRVWorkstationIDs
        {
            get
            {
                if (TheSession()["PLCRVWorkstationIDs"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCRVWorkstationIDs"];
                    return s;
                }
            }

            set
            {
                TheSession()["PLCRVWorkstationIDs"] = value;
            }
        }

        public string PLCGlobalTableKey
        {
            get
            {
                if (TheSession()["PLCGlobalTableKey"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalTableKey"];
                    return s;
                }
            }

            set
            {
                TheSession()["PLCGlobalTableKey"] = value;
            }
        }

        public string PLCNotesSequence
        {
            get
            {
                if (TheSession()["PLCNotesSequence"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCNotesSequence"];
                    return s;
                }
            }
            set
            {
                TheSession()["PLCNotesSequence"] = value;
            }
        }

        public string PLCCrystalParameterFields
        {
            get
            {
                if (TheSession()["PLCCrystalParameterFields"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCCrystalParameterFields"];
                    return s;
                }
            }
            set
            {
                TheSession()["PLCCrystalParameterFields"] = value;
            }
        }

        public string PLCBatchTaskID

        {
            get
            {
                if (TheSession()["PLCBatchTaskID"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCBatchTaskID"];
                    return s;
                }
            }
            set
            {
                TheSession()["PLCBatchTaskID"] = value;
            }
        }

        public string PLCCODNAPrelogSequence
        {
            get
            {
                if (TheSession()["PLCCODNAPrelogSequence"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCCODNAPrelogSequence"];
                    return s;
                }
            }

            set
            {
                TheSession()["PLCCODNAPrelogSequence"] = value;
            }
        }

        public string PLCGlobalUsesDBTextResources
        {
            get
            {
                if (TheSession()["PLCGlobalUsesDBTextResources"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalUsesDBTextResources"];
                    return s;
                }
            }

            set
            {
                TheSession()["PLCGlobalUsesDBTextResources"] = value;
            }
        }

        // No longer in use as labctrl session vars are lazy loaded.
        public string GetLabCtrl_old(string LabCtrlView)
        {
            if ((string)TheSession()["LC_" + LabCtrlView] == null)
                return "";
            else
                return (string)TheSession()["LC_" + LabCtrlView];
        }

        public void SetLabCtrl(string sField, string sValue)
        {
            string sViewName = "LC_" + (sField.ToUpper()).Replace(" ", "_");
            TheSession()[sViewName] = sValue;
        }

        public void SetConfigFileSourceKeys(string fileSourceKey1, string fileSourceKey2)
        {
            PLCGlobalConfigSourceKey1 = fileSourceKey1;
            PLCGlobalConfigSourceKey2 = fileSourceKey2;
        }

#region ChemInv

        //modified to enforce lazy loading
        public string GetChemInvCtrl_old(string ChemCtrlName)
        {
            if (TheSession()["CHEMCTRL"] != null)
            {
                Dictionary<string, string> ChemCtrl = (Dictionary<string, string>)TheSession()["CHEMCTRL"];
                if (ChemCtrl.ContainsKey(ChemCtrlName))
                    return ChemCtrl[ChemCtrlName];
            }
            //
            return "";
        }

        public T GetChemInvProperty<T>(string key, object value)
        {
            Dictionary<string, object> chemInvProperty = GuaranteeKeyExists<T>(key);
            object testObject = chemInvProperty[key];

            // check whether testObject is null and set default value
            if (testObject == null)
            {
                testObject = value;
                chemInvProperty[key] = value;
            }

            return (T)testObject;
        }

        public void SetChemInvProperty<T>(string key, object value)
        {
            Dictionary<string, object> chemInvProperty = GuaranteeKeyExists<T>(key);
            chemInvProperty[key] = value;
        }

        private Dictionary<string, object> GuaranteeKeyExists<T>(string key)
        {
            Dictionary<string, object> chemInvProperty = new Dictionary<string, object>();
            if (TheSession()["CHEMPROP"] == null)
            {
                TheSession()["CHEMPROP"] = chemInvProperty;
            }
            else
            {
                chemInvProperty = (Dictionary<string, object>)TheSession()["CHEMPROP"];
            }

            if (!chemInvProperty.ContainsKey(key))
            {
                chemInvProperty.Add(key, default(T));
            }

            return chemInvProperty;
        }

        private string GetChemInvCtrlSessionKey(string ChemCtrlName)
        {
            return "CI_" + ChemCtrlName;
        }

        public string GetChemInvCtrl(string ChemCtrlName)
        {
            string retVal;

            // Initialize valid chemctrl keys and chemctrl defaults application vars if not yet initialized.
            InitAvailableChemCtrlKeys();

            string cheminvctrlSessionKey = GetChemInvCtrlSessionKey(ChemCtrlName);
            if (TheSession()[cheminvctrlSessionKey] != null)
            {
                retVal = (string)TheSession()[cheminvctrlSessionKey];
            }
            else
            {
                retVal = GetChemCtrlDb(ChemCtrlName);
            }

            TheSession()[cheminvctrlSessionKey] = retVal;

            return retVal;
        }

        private string GetChemCtrlDb(string chemctrlkey)
        {
            if (IsValidChemCtrlKey(chemctrlkey))
            {
                PLCQuery qry = QueryChemCtrlValue(chemctrlkey);
                return qry.FieldByName(chemctrlkey);
            }
            else
            {
                // labctrlkey does not belong to any of the tv_labctrl fields.
                return "";
            }
        }

        private bool IsValidChemCtrlKey(string chemctrlkey)
        {
            if (GetDictChemCtrlKeys().ContainsKey(chemctrlkey))
                return true;
            else
                return false;
        }

        private PLCQuery QueryChemCtrlValue(string chemctrlkey)
        {
            string sql = @"SELECT {0} FROM TV_CHEMCTRL WHERE LAB_CODE = '{1}'";

            PLCQuery qry = new PLCQuery();
            qry.SQL = String.Format(sql, chemctrlkey, this.PLCGlobalLabCode);
            qry.Open();
            return qry;
        }

        private void InitAvailableChemCtrlKeys()
        {
            if (GetDictChemCtrlKeys() == null)
            {
                Dictionary<string, string> dictChemCtrlKeys = new Dictionary<string, string>();

                // Add all columns from all labctrl tables to valid labctrl keys.
                string chemctrltable = "TV_CHEMCTRL";

                PLCQuery qry = new PLCQuery();
                qry.SQL = String.Format("SELECT * FROM {0} WHERE 0=1", chemctrltable);
                qry.Open();

                foreach (DataColumn col in qry.PLCDataTable.Columns)
                {
                    if (!dictChemCtrlKeys.ContainsKey(col.ColumnName))
                        dictChemCtrlKeys.Add(col.ColumnName, "");
                }

                TheApplication()[GetValidChemCtrlKeysKey()] = dictChemCtrlKeys;
            }
        }

        Dictionary<string, string> GetDictChemCtrlKeys()
        {
            return (Dictionary<string, string>)TheApplication()[GetValidChemCtrlKeysKey()];
        }

        private string GetValidChemCtrlKeysKey()
        {
            return "dictChemCtrlKeys" + this.PLCDBName;
        }

#endregion // ChemInv

        public void WriteTrace(string msg)
        {
            string currentTime = DateTime.Now.ToString("HH:mm:ss (fff)");
            System.Diagnostics.Trace.WriteLine(currentTime + " : " + msg);
        }

        public void ForceWriteDebug(string Msg, bool TimeStamp)
        {
            WriteDebug(Msg, TimeStamp, true);
        }

        public void WriteDebug(string Msg, bool TimeStamp)
        {
            WriteDebug(Msg, TimeStamp, false);
        }

        private void WriteDebug(string Msg, bool TimeStamp, bool bAlwaysLog)
        {
            try
            {
                bool isPrelogMode = !string.IsNullOrEmpty(PLCGlobalPrelogUser) && PLCGlobalAnalyst == "";
                string prelogDebugName = "Prelog-";
                if (isPrelogMode) prelogDebugName = GetPrelogDebugName();

                if ((GetUserPref("SESSIONTRACE") == "ENABLED") || (PLCGlobalAnalyst == "") || bAlwaysLog)
                {
                    string debugLogMode = System.Configuration.ConfigurationManager.AppSettings.Get("DEBUG_LOG_MODE"); ;
                    string debugLogSuffix = "";
                    if (debugLogMode == "C")
                    {
                        if (!string.IsNullOrEmpty(ClientID))
                            debugLogSuffix = "-" + ClientID;
                    }
                    else if (debugLogMode == "S")
                        debugLogSuffix = "-" + HttpContext.Current.Session.SessionID;
                    
                    string FileName = PLCPath.LabSave + "webdebug-" + (isPrelogMode ? prelogDebugName : "") + DateTime.Today.Date.Month + "-" + DateTime.Today.Date.Day + "-" + DateTime.Today.Date.Year + "(" + (isPrelogMode ? PLCGlobalPrelogUser : PLCGlobalAnalyst) + debugLogSuffix + ").TXT";
                    TextWriter tw = new StreamWriter(FileName, true);
                    if (TimeStamp)
                        tw.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss (fff) ") + " : " + Msg);
                    //tw.WriteLine(DateTime.Now.ToString(GetDateFormat() + " HH:mm:ss (fff) ") + " : " + Msg);
                    else
                        tw.WriteLine(Msg);
                    tw.Close();

                    //  HttpContext.Current.Trace.Write(Msg);

                }
            }
            catch (Exception E)
            {
                PLCErrorMessage = E.Message + "  " + Msg;
            }
        }

        private string GetPrelogDebugName()
        {
            string name = "Prelog-";
            try
            {
                if (Context.Request.UrlReferrer.ToString().ToUpper().Contains("CODNAPRELOG/"))
                    name = "CODNAPrelog-";
            }
            catch
            {
            }
            return name;
        }

        private bool IsCodnaMode()
        {
            try
            {
                return (Context.Request.UrlReferrer.ToString().ToUpper().Contains("CODNAPRELOG/"));
            }
            catch
            {
            }
            return false;
        }

        /// <summary>
        /// Checks if CODNAPrelog project is being accessed
        /// </summary>
        /// <returns></returns>
        private bool IsCODNAPrelog()
        {
            try
            {
                return (Context.Request.UrlReferrer.ToString().ToUpper().Contains("CODNAPRELOG/"));
            }
            catch
            {
            }
            return false;
        }

        public bool IsChemInvMode()
        {
            try
            {
                return (Context.Request.UrlReferrer.ToString().ToUpper().Contains("CHEMINV/"));
            }
            catch
            {
            }
            return false;
        }

        public bool IsChemInvMaintenance()
        {
            try
            {
                return (Context.Request.UrlReferrer.ToString().ToUpper().Contains("CHEMINV/CHEMINVTABLEMAINTENANCE"));
            }
            catch
            {
            }
            return false;
        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalCourierStat
        {
            get
            {
                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalCourierStat"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalCourierStat"];
                    return s;
                }
            }

            set
            {
                TheSession()["PLCGlobalCourierStat"] = value;
            }
        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalCourierSQL
        {
            get
            {
                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalCourierSQL"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalCourierSQL"];
                    return s;
                }
            }

            set
            {
                TheSession()["PLCGlobalCourierSQL"] = value;
            }
        }

        public bool HaveMandatoryAttributesForItemType(string ecn)
        {
            string ItemType = "";
            PLCQuery qryLabItem = new PLCQuery();
            qryLabItem.SQL = "SELECT * FROM TV_LABITEM where evidence_control_number = " + ecn;
            qryLabItem.Open();
            if (qryLabItem.IsEmpty())
                return false;
            ItemType = qryLabItem.FieldByName("item_type");
            if (ItemType == "")
                return false;

            PLCQuery qyItemAttrib = new PLCQuery();
            qyItemAttrib.SQL = "select count(*) as attribcount from tv_itemattr where item_type = '" + ItemType + "' and required_res = 'T'";
            qyItemAttrib.Open();
            if (qyItemAttrib.iFieldByName("attribcount") > 0)
                return true;
            else
                return false;
        }

#region String Parsing
        public int SafeInt(string s)
        {

            try
            {
                return Convert.ToInt32(s);
            }
            catch
            {
                return 0;
            }

        }

        public int SafeInt(string s, int defaultVal)
        {

            try
            {
                return Convert.ToInt32(s);
            }
            catch
            {
                return defaultVal;
            }

        }

        public bool SafeDate(string d, out DateTime dt)
        {
            var culture = (GetDateFormat().ToUpper().StartsWith("DD"))
                ? CultureInfo.CreateSpecificCulture("en-GB")
                : CultureInfo.CreateSpecificCulture("en-US");

            return DateTime.TryParse(d, culture, DateTimeStyles.None, out dt);
        }

        /// <summary>
        /// Remove spaces and commas before converting to double
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private double ParseDouble(string value)
        {
            return Convert.ToDouble(value.Replace(" ", "").Replace(",", ""));
        }

        /// <summary>
        /// <para>Get the weight in the string and also the unit if it is empty.</para>
        /// <para>
        /// Commas and spaces between numbers are ignored when getting the number.
        /// E.g. "1,1,1,1", "1 000,0.100,100 000"
        /// </para>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public double GetWeight(string value, ref string unit)
        {
            string weight = Regex.Match(value.Trim(), @"^[+-]?\s*\d[\d,\s]*(\.[\d,\s]+)?").Value;
            if (string.IsNullOrEmpty(unit))
                unit = value.Trim().Substring(weight.Length).Trim();
            return ParseDouble(weight.Trim('+').Trim('-'));
        }

        /// <summary>
        /// Get the weight in the string and also the unit if it is empty
        /// </summary>
        /// <param name="value"></param>
        /// <param name="weight"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public bool TryGetWeight(string value, out double weight, ref string unit)
        {
            try
            {
                weight = GetWeight(value, ref unit);
            }
            catch (Exception ex)
            {
                weight = 0;
                return false;
            }

            return true;
        }
#endregion String Parsing

        public string PLCNewItem
        {
            get
            {
                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCNewItem"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCNewItem"];
                    return s;
                }
            }
            set
            {
                TheSession()["PLCNewItem"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public DataSet PLCDataSet
        {
            get
            {
                if (ThePage() == (Page)null)
                    return null;

                if (TheSession()["PLCDataSet"] == null)
                    return null;
                else
                {
                    DataSet s = (DataSet)TheSession()["PLCDataSet"];
                    return s;
                }
            }
            set
            {
                TheSession()["PLCDataSet"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public DataSet PLCQCDataSet
        {
            get
            {
                if (ThePage() == (Page)null)
                    return null;

                if (TheSession()["PLCQCDataSet"] == null)
                    return null;
                else
                {
                    DataSet s = (DataSet)TheSession()["PLCQCDataSet"];
                    return s;
                }
            }
            set
            {
                TheSession()["PLCQCDataSet"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public DataSet PLCGeneralDataSet
        {
            get
            {
                if (ThePage() == (Page)null)
                    return null;

                if (TheSession()["PLCGeneralDataSet"] == null)
                    return null;
                else
                {
                    DataSet s = (DataSet)TheSession()["PLCGeneralDataSet"];
                    return s;
                }
            }
            set
            {
                TheSession()["PLCGeneralDataSet"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public DataSet PLCDataSet2
        {
            get
            {
                if (ThePage() == (Page)null)
                    return null;

                if (TheSession()["PLCDataSet2"] == null)
                    return null;
                else
                {
                    DataSet s = (DataSet)TheSession()["PLCDataSet2"];
                    return s;
                }
            }
            set
            {
                TheSession()["PLCDataSet2"] = value;
            }
        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public DataSet PLCDBGridDataSet
        {
            get
            {
                if (ThePage() == (Page)null)
                    return null;

                if (TheSession()["PLCDBGridDataSet"] == null)
                    return null;
                else
                {
                    DataSet s = (DataSet)TheSession()["PLCDBGridDataSet"];
                    return s;
                }
            }
            set
            {
                TheSession()["PLCDBGridDataSet"] = value;
            }
        }

        public string PLCGloablCaseRefKey
        {
            get
            {
                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGloablCaseRefKey"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGloablCaseRefKey"];
                    return s;
                }
            }
            set
            {
                TheSession()["PLCGloablCaseRefKey"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalScheduleKey
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalScheduleKey"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalScheduleKey"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalScheduleKey"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string TempString
        {
            get
            {
                //if (ThePage() == (Page)null) return "";

                if (TheSession()["TempString"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["TempString"];
                    return s;
                }
            }

            set
            {
                TheSession()["TempString"] = value;
            }
        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string WhatIsNext
        {
            get
            {
                //if (ThePage() == (Page)null) return "";

                if (TheSession()["WhatIsNext"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["WhatIsNext"];
                    return s;
                }
            }

            set
            {
                TheSession()["WhatIsNext"] = value;
            }
        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string TransferCustody
        {
            get
            {
                //if (ThePage() == (Page)null) return "";

                if (TheSession()["TransferCustody"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["TransferCustody"];
                    return s;
                }
            }

            set
            {
                TheSession()["TransferCustody"] = value;
            }
        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string TransferLocation
        {
            get
            {
                //if (ThePage() == (Page)null) return "";

                if (TheSession()["TransferLocation"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["TransferLocation"];
                    return s;
                }
            }

            set
            {
                TheSession()["TransferLocation"] = value;
            }
        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string ReportStart
        {
            get
            {
                //if (ThePage() == (Page)null) return "";

                if (TheSession()["ReportStart"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["ReportStart"];
                    return s;
                }
            }

            set
            {
                TheSession()["ReportStart"] = value;
            }
        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public bool IsNYPrintDeputyRequest
        {
            get
            {
                if (TheSession()["IsNYPrintDeputyRequest"] == null)
                {
                    return false;
                }
                else
                {
                    bool b = (bool)TheSession()["IsNYPrintDeputyRequest"];
                    return b;
                }
            }

            set
            {
                TheSession()["IsNYPrintDeputyRequest"] = value;
            }
        }

        public string GetOSUserName()
        {
            //if (ThePage() == (Page)null) return "";
            try
            {
                String UserName = System.Web.HttpContext.Current.User.Identity.Name.ToString();
                if (String.IsNullOrWhiteSpace(UserName))
                    UserName = ThePage().Request.ServerVariables["REMOTE_USER"];
                return UserName;
            }
            catch
            {
                return "";
            }
        }

        public string GetOSComputerName()
        {
            //if (ThePage() == (Page)null) return "";
            try
            {
                //return System.Net.Dns.GetHostEntry(GetOSAddress()).HostName;
                return ThePage().Request.ServerVariables["REMOTE_HOST"];
            }
            catch
            {
                return "";
            }
        }

        public string GetOSAddress()
        {
            //if (ThePage() == (Page)null) return "";
            try
            {
                //return HttpContext.Current.Request.UserHostAddress;
                return ThePage().Request.ServerVariables["REMOTE_ADDR"];
            }
            catch
            {
                return "";
            }
        }

        public string OSUserName
        {
            get
            {
                //if (ThePage() == (Page)null) return "";

                if (TheSession()["OSUserName"] == null)
                    return GetOSUserName();
                else
                {
                    string s = (string)TheSession()["OSUserName"];
                    return s;
                }
            }

            set
            {
                TheSession()["OSUserName"] = value;
            }
        }

        public string OSComputerName
        {
            get
            {
                //if (ThePage() == (Page)null) return "";

                if (TheSession()["OSComputerName"] == null)
                    return GetOSComputerName();
                else
                {
                    string s = (string)TheSession()["OSComputerName"];
                    return s;
                }
            }

            set
            {
                TheSession()["OSComputerName"] = value;
            }
        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCRPTFilePath
        {
            get
            {
                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCRPTFilePath"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCRPTFilePath"];
                    return s;
                }
            }

            set
            {
                TheSession()["PLCRPTFilePath"] = value;
            }
        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCLBLFilePath
        {
            get
            {
                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCLBLFilePath"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCLBLFilePath"];
                    return s;
                }
            }

            set
            {
                TheSession()["PLCLBLFilePath"] = value;
            }
        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public ContainerHeader PLCContHeaderInfo
        {
            get
            {
                if (ThePage() == (Page)null)
                    return null;

                if (TheSession()["PLCContHeaderInfo"] == null)
                    return null;
                else
                {
                    ContainerHeader s = (ContainerHeader)TheSession()["PLCContHeaderInfo"];
                    return s;
                }
            }

            set
            {
                TheSession()["PLCContHeaderInfo"] = value;
            }
        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCDatabaseServer
        {
            get
            {
                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCDatabaseServer"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCDatabaseServer"];
                    return s;
                }
            }

            set
            {
                TheSession()["PLCDatabaseServer"] = value;
            }
        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCServerIP
        {
            get
            {
                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCServerIP"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCServerIP"];
                    return s;
                }
            }

            set
            {
                TheSession()["PLCServerIP"] = value;
            }
        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCActiveApp
        {
            get
            {
                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCActiveApp"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCActiveApp"];
                    return s;
                }
            }

            set
            {
                TheSession()["PLCActiveApp"] = value;
            }
        }

        [Category("PLC Properties"), Description("Remembers the last saved LabItem Recovery Location inside Case")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCDupedRecoveryLocationInCase
        {
            get
            {

                if (TheSession()["PLCDupedRecoveryLocationInCase"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCDupedRecoveryLocationInCase"];
                    return s;
                }
            }

            set
            {
                TheSession()["PLCDupedRecoveryLocationInCase"] = value;
            }
        }

        [Category("PLC Properties"), Description("Remembers the last saved LabItem Recovery Address Key inside Case")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCDupedRecoveryAddressKeyInCase
        {
            get
            {

                if (TheSession()["PLCDupedRecoveryAddressKeyInCase"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCDupedRecoveryAddressKeyInCase"];
                    return s;
                }
            }

            set
            {
                TheSession()["PLCDupedRecoveryAddressKeyInCase"] = value;
            }
        }

        [Category("PLC Properties"), Description("Remembers the last saved Collected By")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCDupedCollectedBy
        {
            get
            {

                if (TheSession()["PLCDupedCollectedBy"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCDupedCollectedBy"];
                    return s;
                }
            }

            set
            {
                TheSession()["PLCDupedCollectedBy"] = value;
            }
        }

        [Category("PLC Properties"), Description("Remembers the last saved Collected Date")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCDupedCollectedDate
        {
            get
            {

                if (TheSession()["PLCDupedCollectedDate"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCDupedCollectedDate"];
                    return s;
                }
            }

            set
            {
                TheSession()["PLCDupedCollectedDate"] = value;
            }
        }

        [Category("PLC Properties"), Description("Remembers the last saved Time Collected")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCDupedTimeCollected
        {
            get
            {

                if (TheSession()["PLCDupedTimeCollected"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCDupedTimeCollected"];
                    return s;
                }
            }

            set
            {
                TheSession()["PLCDupedTimeCollected"] = value;
            }
        }

        public void FocusBarCodeField()
        {

            if (ThePage() == (Page)null)
                return;
            TextBox tb = (TextBox)ThePage().FindControl("ctl00$UC_CustomerTitle_Master2$txtBARCODE");
            //TextBox tb = (TextBox) ThePage().FindControl("ctl00_UC_CustomerTitle_Master2_txtBARCODE");
            if (tb != null)
                tb.Focus();

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalSignaturePad
        {
            get
            {
                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCGlobalSignaturePad"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalSignaturePad"];
                    return s;
                }
            }

            set
            {
                TheSession()["PLCGlobalSignaturePad"] = value;
            }
        }

        public string PLCGlobalSignaturePadInstalled
        {
            get
            {
                if (TheSession()["PLCGlobalSignaturePadInstalled"] == null)
                    return "";
                else
                    return (string)TheSession()["PLCGlobalSignaturePadInstalled"];
            }
            set
            {
                TheSession()["PLCGlobalSignaturePadInstalled"] = value;
            }
        }

        [Category("PLC Properties"), Description("SRMaster")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalSRMasterKey
        {
            get
            {
                return (TheSession()["SRMaster"] == null) ? "" : (string)TheSession()["SRMaster"];
            }
            set
            {
                TheSession()["SRMaster"] = value;
            }
        }

        public string PLCGlobalAttachmentSourceKey
        {
            get
            {
                return (TheSession()["FileSourceKey"] == null) ? "" : (string)TheSession()["FileSourceKey"];
            }
            set
            {
                TheSession()["FileSourceKey"] = value;
            }
        }

        //$$ Use override instead?
        public new string ClientID
        {
            get
            {
                return this.GetDefault("CLIENTID");
            }
        }

        [DefaultValue("")]
        [Localizable(true)]
        public string PLCGlobalMatrixEditable
        {

            get
            {

                if (TheSession()["PLCGlobalMatrixEditable"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCGlobalMatrixEditable"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCGlobalMatrixEditable"] = value;
            }

        }

#region PLCCaseContainerSearch
        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCCaseContainerDescription
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCCaseContainerDescription"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCCaseContainerDescription"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCCaseContainerDescription"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCCaseContainerSource
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCCaseContainerSource"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCCaseContainerSource"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCCaseContainerSource"] = value;
            }

        }

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue("")]
        [Localizable(true)]
        public string PLCCaseContainerComments
        {

            get
            {

                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCCaseContainerComments"] == null)
                    return "";
                else
                {
                    string s = (string)TheSession()["PLCCaseContainerComments"];
                    return s;
                }

            }

            set
            {
                TheSession()["PLCCaseContainerComments"] = value;
            }

        }
#endregion

        public void RemoveFocusBarCodeField()
        {
            if (ThePage() == (Page)null)
                return;
            TextBox tb = (TextBox)ThePage().FindControl("ctl00$UC_CustomerTitle_Master2$txtBARCODE");
            if (tb != null)
                tb.TabIndex = -1;
        }

        // Given an argument list (Ex. parm1, parm2, parm3), return an array containing the arguments passed: [parm1, parm2, parm3].
        private List<string> GetParams(string arglist)
        {
            string[] argitems = arglist.Split(' ', ',');

            List<string> retargs = new List<string>
            {
            };

            // Go through the split args list and skip over the empty args that result from the ',' and ' ' delimiters appearing consecutively.
            // Ex. when "parm1, parm2, parm3" is split, the resulting contents are: ["parm1", "", "parm2", "", "parm3"]
            foreach (string argitem in argitems)
            {
                if (argitem.Trim().Length > 0)
                    retargs.Add(argitem);
            }

            return retargs;
        }

        public string FormatSpecialFunctions(string SQLString)
        {
            if (!String.IsNullOrEmpty(SQLString))
            {
                //*AAC* 05272009 Added code to check if MSSQL, then replace '||' to '+' (for concatenating strings).
                //*AAC* 07142009 Replace NVL with ISNULL for MSSQL
                if (PLCDatabaseServer == "MSSQL")
                {
                    SQLString = SQLString.Replace("||", "+");
                    SQLString = SQLString.Replace("NVL(", " ISNULL(");
                    SQLString = SQLString.Replace("SUBSTR(", "SUBSTRING(");
                    SQLString = SQLString.Replace("LENGTH(", "LEN(");
                    SQLString = SQLString.Replace("SYSDATE", "GETDATE[]");
                }
                else
                {
                    //regretfully, we still have some queries that use the Oracle syntax...
                    string ojhack = "..!OUTER$JOIN!..";
                    SQLString = SQLString.Replace("(+)", ojhack);
                    SQLString = SQLString.Replace("+", "||");
                    SQLString = SQLString.Replace(ojhack, "(+)");
                }


                string TempSqlString = SQLString.ToUpper();
                string Temp1 = "";
                string Temp2 = "";
                string FieldName = "";
                string FunctionName = string.Empty;

                //*AAC* 05272009 Updated by Kristine. Added code to replace CONVERTTODATE with corresponding Oracle/MSSQL date function.
                int Index = TempSqlString.IndexOf("CONVERTTODATE(");

                // Format: CONVERTTODATE('<date>', [lastflag])
                // Where <date> is the text representation of the date
                //       [lastflag] is an optional parameter set to 'true' when this date is to be used as the end date in a comparison sequence
                //          as when determining whether a date falls within the range of a start or end date.
                // Ex. if (parmDate >= CONVERTTODATE('1/1/2010') && parmDate <= CONVERTTODATE('1/1/2010', true).
                //      CONVERTTODATE('1/1/2010', true) gets converted to the datetime representation '1/1/2010 23:59:59'

                while (Index >= 0)
                {
                    Temp1 = SQLString.Substring(0, Index);
                    TempSqlString = SQLString.Substring(Index + 14, SQLString.Length - (Index + 14));

                    Index = TempSqlString.IndexOf(")");
                    if (Index >= 0)
                    {
                        List<string> args = GetParams(TempSqlString.Substring(0, Index));
                        FieldName = args[0];
                        string argflag = null;
                        if (args.Count > 1)
                            argflag = args[1];

                        //                        FieldName = TempSqlString.Substring(0, Index);
                        Temp2 = TempSqlString.Substring(Index + 1, TempSqlString.Length - (Index + 1));

                        if (PLCDatabaseServer == "MSSQL")
                        {
                            if (argflag == "true")
                                SQLString = Temp1 + String.Format("'{0} 23:59:59'", FieldName.Replace("'", "")) + Temp2;
                            else
                                SQLString = Temp1 + FieldName + Temp2;
                        }
                        else
                        {
                            if (argflag == "true")
                                SQLString = Temp1 + String.Format("to_date('{0} 23:59:59', 'mm/dd/yyyy hh24:mi:ss')", FieldName.Replace("'", "")) + Temp2;
                            else
                                SQLString = Temp1 + " to_date(" + FieldName + ", 'mm/dd/yyyy')" + Temp2;
                        }
                    }

                    TempSqlString = SQLString;
                    Index = TempSqlString.IndexOf("CONVERTTODATE(");
                }

                TempSqlString = SQLString.ToUpper();
                Index = TempSqlString.IndexOf("FORMATDATE(");

                while (Index >= 0)
                {
                    Temp1 = SQLString.Substring(0, Index);
                    TempSqlString = SQLString.Substring(Index + 11, SQLString.Length - (Index + 11));

                    Index = TempSqlString.IndexOf(")");
                    if (Index >= 0)
                    {
                        FieldName = TempSqlString.Substring(0, Index);
                        Temp2 = TempSqlString.Substring(Index + 1, TempSqlString.Length - (Index + 1));

                        if (PLCDatabaseServer == "MSSQL")
                        {
                            if (GetDateFormat().ToUpper().StartsWith("DD"))
                                SQLString = Temp1 + " convert(char(10), " + FieldName + ", 103)" + Temp2;
                            else
                                SQLString = Temp1 + " convert(char(10), " + FieldName + ", 101)" + Temp2;
                        }
                        else
                            SQLString = Temp1 + " to_char(" + FieldName + ", 'mm/dd/yyyy')" + Temp2;
                    }

                    TempSqlString = SQLString;
                    Index = TempSqlString.IndexOf("FORMATDATE(");
                }

                TempSqlString = SQLString.ToUpper();
                Index = TempSqlString.IndexOf("FORMATTIME(");

                while (Index >= 0)
                {
                    Temp1 = SQLString.Substring(0, Index);
                    TempSqlString = SQLString.Substring(Index + 11, SQLString.Length - (Index + 11));

                    Index = TempSqlString.IndexOf(")");
                    if (Index >= 0)
                    {
                        FieldName = TempSqlString.Substring(0, Index);
                        Temp2 = TempSqlString.Substring(Index + 1, TempSqlString.Length - (Index + 1));

                        if (PLCDatabaseServer == "MSSQL")
                            SQLString = Temp1 + " convert(char(8), " + FieldName + ", 108)" + Temp2;
                        else
                            SQLString = Temp1 + " to_char(" + FieldName + ", 'HH24:MI:SS')" + Temp2;
                    }

                    TempSqlString = SQLString;
                    Index = TempSqlString.IndexOf("FORMATTIME(");
                }


                TempSqlString = SQLString.ToUpper();
                Index = TempSqlString.IndexOf("FORMATDATETIME(");

                while (Index >= 0)
                {
                    Temp1 = SQLString.Substring(0, Index);
                    TempSqlString = SQLString.Substring(Index + 15, SQLString.Length - (Index + 15));

                    Index = TempSqlString.IndexOf(")");
                    if (Index >= 0)
                    {
                        FieldName = TempSqlString.Substring(0, Index);
                        Temp2 = TempSqlString.Substring(Index + 1, TempSqlString.Length - (Index + 1));

                        //*AAC* 05272009 Updated by Kristine. Fixed bug in concatenating date and time.
                        //old code
                        //SQLString = Temp1 + " convert(char(10), " + FieldName + ", 101) convert(char(5), " + FieldName + ", 108)" + Temp2;

                        if (PLCDatabaseServer == "MSSQL")
                        {
                            if (GetDateFormat().ToUpper().StartsWith("DD"))
                                SQLString = Temp1 + " convert(char(10), " + FieldName + ", 103) + ' ' + convert(char(8), " + FieldName + ", 108)" + Temp2;
                            else
                                SQLString = Temp1 + " convert(char(10), " + FieldName + ", 101) + ' ' + convert(char(8), " + FieldName + ", 108)" + Temp2;
                        }
                        else
                            SQLString = Temp1 + " to_char(" + FieldName + ", 'mm/dd/yyyy HH24:MI:SS')" + Temp2;
                    }

                    TempSqlString = SQLString;
                    Index = TempSqlString.IndexOf("FORMATDATETIME(");
                }

                // convert int field to char when concatenating a string with the int type field.
                if (SQLString.IndexOf("INTTOCHAR(") > -1)
                {
                    TempSqlString = SQLString.ToUpper();
                    Index = TempSqlString.IndexOf("INTTOCHAR(");

                    while (Index > -1)
                    {
                        Temp1 = SQLString.Substring(0, Index);
                        TempSqlString = SQLString.Substring(Index + 10, SQLString.Length - (Index + 10));

                        Index = TempSqlString.IndexOf(")");
                        if (Index > -1)
                        {
                            FieldName = TempSqlString.Substring(0, Index);
                            Temp2 = TempSqlString.Substring(Index + 1, TempSqlString.Length - (Index + 1));

                            if (PLCDatabaseServer == "MSSQL")
                                SQLString = Temp1 + " convert(char, " + FieldName + ") " + Temp2;
                            else
                                SQLString = Temp1 + " to_char(" + FieldName + ") " + Temp2;
                        }

                        TempSqlString = SQLString;
                        Index = TempSqlString.IndexOf("INTTOCHAR(");
                    }
                }

                SQLString = SQLString.Replace("GETDATE[]", "GETDATE()");

                FunctionName = "CONVERTTODATETYPE(";
                if (SQLString.IndexOf(FunctionName) > -1)
                {
                    TempSqlString = SQLString.ToUpper();
                    Index = TempSqlString.IndexOf(FunctionName);

                    while (Index >= 0)
                    {
                        Temp1 = SQLString.Substring(0, Index);
                        TempSqlString = SQLString.Substring(Index + FunctionName.Length, SQLString.Length - (Index + FunctionName.Length));

                        Index = TempSqlString.IndexOf(")");
                        if (Index >= 0)
                        {
                            FieldName = TempSqlString.Substring(0, Index);
                            Temp2 = TempSqlString.Substring(Index + 1, TempSqlString.Length - (Index + 1));

                            if (PLCDatabaseServer == "MSSQL")
                            {
                                if (GetDateFormat().ToUpper().StartsWith("DD"))
                                    SQLString = Temp1 + " convert(date, " + FieldName + ", 103)" + Temp2;
                                else
                                    SQLString = Temp1 + " convert(date, " + FieldName + ", 101)" + Temp2;
                            }
                            else
                                SQLString = Temp1 + " to_date(" + FieldName + ", '" + GetDateFormat() + "')" + Temp2;
                        }

                        TempSqlString = SQLString;
                        Index = TempSqlString.IndexOf(FunctionName);
                    }
                }

                return SQLString;
            }
            else
                return SQLString;
        }

        public string FormatSpecialSQLFunctions(string dabaseServer, string sqlString)
        {
            if (!String.IsNullOrEmpty(sqlString))
            {
                if (dabaseServer == "MSSQL")
                {
                    sqlString = sqlString.Replace("||", "+");
                    sqlString = sqlString.Replace("NVL(", " ISNULL(");
                    sqlString = sqlString.Replace("SUBSTR(", "SUBSTRING(");
                    sqlString = sqlString.Replace("LENGTH(", "LEN(");
                    sqlString = sqlString.Replace("SYSDATE", "GETDATE[]");
                }
                else
                {
                    //regretfully, we still have some queries that use the Oracle syntax...
                    string ojhack = "..!OUTER$JOIN!..";
                    sqlString = sqlString.Replace("(+)", ojhack);
                    sqlString = sqlString.Replace("+", "||");
                    sqlString = sqlString.Replace(ojhack, "(+)");
                }


                string TempSqlString = sqlString.ToUpper();
                string Temp1 = "";
                string Temp2 = "";
                string FieldName = "";
                string FunctionName = string.Empty;

                int Index = TempSqlString.IndexOf("CONVERTTODATE(");

                while (Index >= 0)
                {
                    Temp1 = sqlString.Substring(0, Index);
                    TempSqlString = sqlString.Substring(Index + 14, sqlString.Length - (Index + 14));

                    Index = TempSqlString.IndexOf(")");
                    if (Index >= 0)
                    {
                        List<string> args = GetParams(TempSqlString.Substring(0, Index));
                        FieldName = args[0];
                        string argflag = null;
                        if (args.Count > 1)
                            argflag = args[1];

                        Temp2 = TempSqlString.Substring(Index + 1, TempSqlString.Length - (Index + 1));

                        if (dabaseServer == "MSSQL")
                        {
                            if (argflag == "true")
                                sqlString = Temp1 + String.Format("'{0} 23:59:59'", FieldName.Replace("'", "")) + Temp2;
                            else
                                sqlString = Temp1 + FieldName + Temp2;
                        }
                        else
                        {
                            if (argflag == "true")
                                sqlString = Temp1 + String.Format("to_date('{0} 23:59:59', 'mm/dd/yyyy hh24:mi:ss')", FieldName.Replace("'", "")) + Temp2;
                            else
                                sqlString = Temp1 + " to_date(" + FieldName + ", 'mm/dd/yyyy')" + Temp2;
                        }
                    }

                    TempSqlString = sqlString;
                    Index = TempSqlString.IndexOf("CONVERTTODATE(");
                }

                TempSqlString = sqlString.ToUpper();
                Index = TempSqlString.IndexOf("CONVERTTODATE24(");

                while (Index >= 0)
                {
                    Temp1 = sqlString.Substring(0, Index);
                    TempSqlString = sqlString.Substring(Index + 16, sqlString.Length - (Index + 16));

                    Index = TempSqlString.IndexOf(")");
                    if (Index >= 0)
                    {
                        List<string> args = GetParams(TempSqlString.Substring(0, Index));
                        FieldName = args[0];
                        string fieldValue = null;
                        if (args.Count > 1)
                            fieldValue = args[1];

                        Temp2 = TempSqlString.Substring(Index + 1, TempSqlString.Length - (Index + 1));

                        if (dabaseServer == "ORACLE")
                        {
                            string rangeValue = string.Format("({0} >= TO_DATE('{1} 00:00:00', 'MM/dd/yyyy HH24:MI:SS') AND {0} <= TO_DATE('{1} 23:59:59', 'MM/dd/yyyy HH24:MI:SS'))", FieldName, fieldValue);
                            sqlString = Temp1 + rangeValue + Temp2;
                        }
                        else
                        {
                            string rangeValue = string.Format("({0} >= CONVERT(DATETIME, '{1} 00:00:00', 101) AND {0} <= CONVERT(DATETIME, '{1} 23:59:59', 101))", FieldName, fieldValue);
                            sqlString = Temp1 + rangeValue + Temp2;
                        }
                    }

                    TempSqlString = sqlString;
                    Index = TempSqlString.IndexOf("CONVERTTODATE24(");
                }

                TempSqlString = sqlString.ToUpper();
                Index = TempSqlString.IndexOf("FORMATDATE(");

                while (Index >= 0)
                {
                    Temp1 = sqlString.Substring(0, Index);
                    TempSqlString = sqlString.Substring(Index + 11, sqlString.Length - (Index + 11));

                    Index = TempSqlString.IndexOf(")");
                    if (Index >= 0)
                    {
                        FieldName = TempSqlString.Substring(0, Index);
                        Temp2 = TempSqlString.Substring(Index + 1, TempSqlString.Length - (Index + 1));

                        if (dabaseServer == "MSSQL")
                        {
                            if (PLCSession.GetDateFormat().ToUpper().StartsWith("DD"))
                                sqlString = Temp1 + " convert(char(10), " + FieldName + ", 103)" + Temp2;
                            else
                                sqlString = Temp1 + " convert(char(10), " + FieldName + ", 101)" + Temp2;
                        }
                        else
                            sqlString = Temp1 + " to_char(" + FieldName + ", 'mm/dd/yyyy')" + Temp2;
                    }

                    TempSqlString = sqlString;
                    Index = TempSqlString.IndexOf("FORMATDATE(");
                }

                TempSqlString = sqlString.ToUpper();
                Index = TempSqlString.IndexOf("FORMATTIME(");

                while (Index >= 0)
                {
                    Temp1 = sqlString.Substring(0, Index);
                    TempSqlString = sqlString.Substring(Index + 11, sqlString.Length - (Index + 11));

                    Index = TempSqlString.IndexOf(")");
                    if (Index >= 0)
                    {
                        FieldName = TempSqlString.Substring(0, Index);
                        Temp2 = TempSqlString.Substring(Index + 1, TempSqlString.Length - (Index + 1));

                        if (dabaseServer == "MSSQL")
                            sqlString = Temp1 + " convert(char(8), " + FieldName + ", 108)" + Temp2;
                        else
                            sqlString = Temp1 + " to_char(" + FieldName + ", 'HH24:MI:SS')" + Temp2;
                    }

                    TempSqlString = sqlString;
                    Index = TempSqlString.IndexOf("FORMATTIME(");
                }


                TempSqlString = sqlString.ToUpper();
                Index = TempSqlString.IndexOf("FORMATDATETIME(");

                while (Index >= 0)
                {
                    Temp1 = sqlString.Substring(0, Index);
                    TempSqlString = sqlString.Substring(Index + 15, sqlString.Length - (Index + 15));

                    Index = TempSqlString.IndexOf(")");
                    if (Index >= 0)
                    {
                        FieldName = TempSqlString.Substring(0, Index);
                        Temp2 = TempSqlString.Substring(Index + 1, TempSqlString.Length - (Index + 1));

                        //*AAC* 05272009 Updated by Kristine. Fixed bug in concatenating date and time.
                        //old code
                        //SQLString = Temp1 + " convert(char(10), " + FieldName + ", 101) convert(char(5), " + FieldName + ", 108)" + Temp2;

                        if (dabaseServer == "MSSQL")
                        {
                            if (PLCSession.GetDateFormat().ToUpper().StartsWith("DD"))
                                sqlString = Temp1 + " convert(char(10), " + FieldName + ", 103) + ' ' + convert(char(8), " + FieldName + ", 108)" + Temp2;
                            else
                                sqlString = Temp1 + " convert(char(10), " + FieldName + ", 101) + ' ' + convert(char(8), " + FieldName + ", 108)" + Temp2;
                        }
                        else
                            sqlString = Temp1 + " to_char(" + FieldName + ", 'mm/dd/yyyy HH24:MI:SS')" + Temp2;
                    }

                    TempSqlString = sqlString;
                    Index = TempSqlString.IndexOf("FORMATDATETIME(");
                }

                // convert int field to char when concatenating a string with the int type field.
                if (sqlString.IndexOf("INTTOCHAR(") > -1)
                {
                    TempSqlString = sqlString.ToUpper();
                    Index = TempSqlString.IndexOf("INTTOCHAR(");

                    while (Index > -1)
                    {
                        Temp1 = sqlString.Substring(0, Index);
                        TempSqlString = sqlString.Substring(Index + 10, sqlString.Length - (Index + 10));

                        Index = TempSqlString.IndexOf(")");
                        if (Index > -1)
                        {
                            FieldName = TempSqlString.Substring(0, Index);
                            Temp2 = TempSqlString.Substring(Index + 1, TempSqlString.Length - (Index + 1));

                            if (dabaseServer == "MSSQL")
                                sqlString = Temp1 + " convert(char, " + FieldName + ") " + Temp2;
                            else
                                sqlString = Temp1 + " to_char(" + FieldName + ") " + Temp2;
                        }

                        TempSqlString = sqlString;
                        Index = TempSqlString.IndexOf("INTTOCHAR(");
                    }
                }

                sqlString = sqlString.Replace("GETDATE[]", "GETDATE()");

                FunctionName = "CONVERTTODATETYPE(";
                if (sqlString.IndexOf(FunctionName) > -1)
                {
                    TempSqlString = sqlString.ToUpper();
                    Index = TempSqlString.IndexOf(FunctionName);

                    while (Index >= 0)
                    {
                        Temp1 = sqlString.Substring(0, Index);
                        TempSqlString = sqlString.Substring(Index + FunctionName.Length, sqlString.Length - (Index + FunctionName.Length));

                        Index = TempSqlString.IndexOf(")");
                        if (Index >= 0)
                        {
                            FieldName = TempSqlString.Substring(0, Index);
                            Temp2 = TempSqlString.Substring(Index + 1, TempSqlString.Length - (Index + 1));

                            if (dabaseServer == "MSSQL")
                            {
                                if (PLCSession.GetDateFormat().ToUpper().StartsWith("DD"))
                                    sqlString = Temp1 + " convert(date, " + FieldName + ", 103)" + Temp2;
                                else
                                    sqlString = Temp1 + " convert(date, " + FieldName + ", 101)" + Temp2;
                            }
                            else
                                sqlString = Temp1 + " to_date(" + FieldName + ", '" + PLCSession.GetDateFormat() + "')" + Temp2;
                        }

                        TempSqlString = sqlString;
                        Index = TempSqlString.IndexOf(FunctionName);
                    }
                }

                return sqlString;
            }
            else
                return sqlString;
        }

        public string GetBase64(int i)
        {
            return "";
        }

        public int GetNextResultNum(Int64 TaskID)
        {
            int nextNum = -1;
            PLCQuery qryResultNum = new PLCQuery();
            qryResultNum.SQL = "select coalesce(max(RESULT_NUMBER),0) + 1 NEXT_RESULT_NUMBER from TV_RESULTS where TASK_ID = ?";
            qryResultNum.AddParameter("TASK_ID", TaskID);

            qryResultNum.Open();
            if (qryResultNum.HasData())
            {
                nextNum = qryResultNum.iFieldByName("NEXT_RESULT_NUMBER");
            }

            return nextNum;
        }

        public int GetNextSequence(string seqname)
        {
            int newseq = 0;


            if (PLCDatabaseServer == "MSSQL")
            {
                string connStr = GetConnectionString();
                using (OleDbConnection dbconn = new OleDbConnection(connStr))
                {
                    try
                    {
                        dbconn.Open();
                    }
                    catch
                    {
                        // throwexception("Open", "Cannot Open Database Connection", e.Message);                        
                        return -1;
                    }


                    OleDbDataAdapter da = new OleDbDataAdapter("NEXTVAL", dbconn);
                    //OleDbDataAdapter da = new OleDbDataAdapter("GET_AUDITLOG_SEQ", dbconn);
                    da.SelectCommand.CommandType = CommandType.StoredProcedure;
                    da.SelectCommand.Parameters.AddWithValue("@SEQUENCE_NAME", seqname);
                    da.SelectCommand.Parameters.Add("@results", OleDbType.Integer);
                    da.SelectCommand.Parameters["@results"].Direction = ParameterDirection.Output;
                    da.SelectCommand.Parameters["@results"].Value = OleDbType.UnsignedInt;

                    DataSet DS = new DataSet();
                    da.Fill(DS);

                    try
                    {
                        newseq = Convert.ToInt32(da.SelectCommand.Parameters[1].Value.ToString());
                    }
                    catch
                    {
                        newseq = -1;
                    }
                }
            }
            else
            {
                PLCQuery qrySeq = new PLCQuery();
                qrySeq.SQL = "select " + seqname + ".NEXTVAL NEXTSEQ from DUAL";
                qrySeq.Open();
                if (qrySeq.HasData())
                {
                    newseq = SafeInt(qrySeq.FieldByName("NEXTSEQ"));
                }
            }
            return newseq;
        }

        public int GetNextSequenceProperty(string seqname, string tblName)
        {
            string sprocName = "";

            sprocName = "GET_" + seqname + "_SEQ";

            int newseq = 0;


            if (PLCDatabaseServer == "MSSQL")
            {
                string connStr = GetConnectionString();
                using (OleDbConnection dbconn = new OleDbConnection(connStr))
                {
                    try
                    {
                        dbconn.Open();
                    }
                    catch
                    {
                        // throwexception("Open", "Cannot Open Database Connection", e.Message);                        
                        return -1;
                    }


                    OleDbDataAdapter da = new OleDbDataAdapter(sprocName, dbconn);
                    da.SelectCommand.CommandType = CommandType.StoredProcedure;
                    da.SelectCommand.Parameters.Add("@results", OleDbType.Integer);
                    da.SelectCommand.Parameters["@results"].Direction = ParameterDirection.Output;
                    da.SelectCommand.Parameters["@results"].Value = OleDbType.UnsignedInt;

                    DataSet DS = new DataSet();
                    da.Fill(DS);

                    try
                    {
                        newseq = Convert.ToInt32(da.SelectCommand.Parameters[0].Value.ToString());
                    }
                    catch
                    {
                        newseq = -1;
                    }
                }
            }
            else
            {
                PLCQuery qrySeq = new PLCQuery();
                qrySeq.SQL = "select " + seqname + ".NEXTVAL NEXTSEQ from DUAL";
                qrySeq.Open();
                if (qrySeq.HasData())
                {
                    newseq = SafeInt(qrySeq.FieldByName("NEXTSEQ"));
                }
            }
            return newseq;
        }

        public bool IsNarcoticsReviewRequired(string itemType)
        {
            if (GetLabCtrl("NARCOTICS_REVIEW_SECTION") == "")
                return false;

            PLCQuery qryItemType = new PLCQuery();
            qryItemType.SQL = "SELECT NARCOTICS_REVIEW_REQUIRED FROM TV_ITEMTYPE WHERE ITEM_TYPE = '" + itemType + "'";
            qryItemType.Open();
            if (qryItemType.IsEmpty())
                return false;
            else
                return (qryItemType.FieldByName("NARCOTICS_REVIEW_REQUIRED") == "T");
        }

        public string CustodyDepartmentCode(string custody)
        {
            string departmentCode = "";

            PLCQuery qryCustody = new PLCQuery();
            qryCustody.SQL = "SELECT DEPARTMENT_CODE FROM TV_CUSTCODE WHERE CUSTODY_TYPE = '" + custody + "'";
            qryCustody.Open();
            if (!qryCustody.IsEmpty())
                departmentCode = qryCustody.FieldByName("DEPARTMENT_CODE");

            return departmentCode;
        }

        public void AddDefaultCustody(string caseKey, string ecn, bool analystCustodyAlreadyExists = false, bool updateContainerCustody = false)
        {
            AddDefaultCustody(caseKey, ecn, null, null, null, null, analystCustodyAlreadyExists, updateContainerCustody);
        }

        public void AddDefaultCustody(string caseKey, string ecn, string batchKey, string containerKey, string weight, string weightUnit, bool analystCustodyAlreadyExists = false, bool updateContainerCustody = false, DateTime? custodyDate = null, DateTime? custodyTime = null)
        {
            AddDefaultCustody(caseKey, ecn, null, containerKey, "ADD", weight, weightUnit, analystCustodyAlreadyExists, updateContainerCustody, custodyDate, custodyTime);
        }

        public void AddDefaultCustody(string caseKey, string ecn, string batchKey, string containerKey, string source, string weight, string weightUnit, bool analystCustodyAlreadyExists = false, bool updateContainerCustody = false, DateTime? custodyDate = null, DateTime? custodyTime = null)
        {
            int submissionNumber = 0;
            string itemType = "";
            int parentEcn = 0;
            string parentCustody = "";
            string parentLocation = "";

            PLCQuery qryItem = new PLCQuery(@"SELECT I.LAB_CASE_SUBMISSION, I.ITEM_TYPE, I.PARENT_ECN, P.CUSTODY_OF, P.LOCATION FROM TV_LABITEM I
LEFT OUTER JOIN TV_LABITEM P ON I.PARENT_ECN = P.EVIDENCE_CONTROL_NUMBER
WHERE I.EVIDENCE_CONTROL_NUMBER = " + ecn);
            qryItem.Open();

            if (!qryItem.IsEmpty())
            {
                submissionNumber = string.IsNullOrEmpty(qryItem.FieldByName("LAB_CASE_SUBMISSION")) ? 0 : Convert.ToInt32(qryItem.FieldByName("LAB_CASE_SUBMISSION"));
                itemType = qryItem.FieldByName("ITEM_TYPE");
                parentEcn = string.IsNullOrEmpty(qryItem.FieldByName("PARENT_ECN")) ? 0 : Convert.ToInt32(qryItem.FieldByName("PARENT_ECN"));
                parentCustody = qryItem.FieldByName("CUSTODY_OF");
                parentLocation = qryItem.FieldByName("LOCATION");
            }

            AddDefaultCustody(caseKey, ecn, batchKey, containerKey, source, submissionNumber, itemType, parentEcn, parentCustody, parentLocation, weight, weightUnit, analystCustodyAlreadyExists, updateContainerCustody, custodyDate, custodyTime);
        }

        public void AddDefaultCustody(string caseKey, string ecn, string batchKey, string containerKey, string source, int submissionNumber, string itemType, int parentEcn,
            string parentCustody, string parentLocation, string weight, string weightUnit, bool analystCustodyAlreadyExists, bool updateContainerCustody,
            DateTime? initialDate = null, DateTime? initialTime = null)
        {
            //get default custody settings from TV_GLOBALINI

            string custodyType = "";
            string custodyTimeType = "";
            string itemContainerKey;
            source = source.ToUpper();

            if (source == "DUPE")
            {
                string dupeCustody = GetGlobalINIValue("DUPE_CUSTODY");
                string dupeCustodyTime = GetGlobalINIValue("DUPE_CUSTODY_TIME");
                custodyType = string.IsNullOrEmpty(dupeCustody) ? "DEFAULT" : dupeCustody;
                custodyTimeType = string.IsNullOrEmpty(dupeCustodyTime) ? "CURRENT" : dupeCustodyTime;
            }
            else if (source == "SAMPLE")
            {
                string sampleCustody = GetGlobalINIValue("SAMPLE_CUSTODY");
                string sampleCustodyTime = GetGlobalINIValue("SAMPLE_CUSTODY_TIME");
                custodyType = string.IsNullOrEmpty(sampleCustody) ? "ANALYST" : sampleCustody;
                custodyTimeType = string.IsNullOrEmpty(sampleCustodyTime) ? "CURRENT" : sampleCustodyTime;
            }
            else if (source == "KIT")
            {
                string kitCustody = GetGlobalINIValue("KIT_CUSTODY");
                string kitCustodyTime = GetGlobalINIValue("KIT_CUSTODY_TIME");
                custodyType = string.IsNullOrEmpty(kitCustody) ? "PARENT" : kitCustody;
                custodyTimeType = string.IsNullOrEmpty(kitCustodyTime) ? "SUBMISSION" : kitCustodyTime;
            }
            else if (source == "RETAIN")
            {
                string retainCustody = GetGlobalINIValue("RETAIN_CUSTODY");
                string retainCustodyTime = GetGlobalINIValue("RETAIN_CUSTODY_TIME");
                custodyType = string.IsNullOrEmpty(retainCustody) ? "DEFAULT" : retainCustody;
                custodyTimeType = string.IsNullOrEmpty(retainCustodyTime) ? "SUBMISSION" : retainCustodyTime;
            }
            else //default: ADD
            {
                string addCustody = GetGlobalINIValue("ADD_CUSTODY");
                string addCustodyTime = GetGlobalINIValue("ADD_CUSTODY_TIME");
                custodyType = string.IsNullOrEmpty(addCustody) ? "DEFAULT" : addCustody;
                custodyTimeType = string.IsNullOrEmpty(addCustodyTime) ? "SUBMISSION" : addCustodyTime;
            }

            string custody = "";
            string location = "";

            if (custodyType == "NONE")
            {
                custody = "";
                location = "";
            }
            else if (custodyType == "ANALYST")
            {
                if (analystCustodyAlreadyExists)
                    return;

                custody = PLCSession.PLCGlobalDefaultAnalystCustodyOf;
                location = PLCSession.PLCGlobalAnalyst;
            }
            else if (custodyType == "PARENT")
            {
                custody = parentCustody;
                location = parentLocation;
            }
            else if (custodyType == "DEFAULT")
            {
                PLCQuery qryItemType =
                    new PLCQuery(
                        string.Format(
                            "SELECT DEFAULT_CUSTODY, DEFAULT_LOCATION FROM TV_ITEMTYPE WHERE ITEM_TYPE = '{0}'",
                            itemType));
                qryItemType.Open();
                if (!qryItemType.IsEmpty())
                {
                    custody = qryItemType.FieldByName("DEFAULT_CUSTODY");
                    location = qryItemType.FieldByName("DEFAULT_LOCATION");
                }

                if (custody == "" || location == "")
                {
                    custody = PLCSession.GetLabCtrl("DEFAULT_ITEM_STATUS");
                    location = PLCSession.GetLabCtrl("DEFAULT_ITEM_LOCKER");
                }
            }
            else if (custodyType == "SUBMISSION")
            {
                PLCQuery qrySubmissionAnalyst =
                    new PLCQuery(
                        string.Format(
                            @"SELECT A.ANALYST, A.CUSTODY_OF FROM TV_LABSUB S
INNER JOIN TV_ANALYST A ON S.RECEIVED_BY = A.ANALYST
WHERE CASE_KEY = {0} AND SUBMISSION_NUMBER = {1}",
                            caseKey, submissionNumber));
                qrySubmissionAnalyst.Open();
                if (!qrySubmissionAnalyst.IsEmpty())
                {
                    custody = qrySubmissionAnalyst.FieldByName("CUSTODY_OF");
                    location = qrySubmissionAnalyst.FieldByName("ANALYST");
                }
            }

            if (custodyTimeType == "SUBMISSION")
            {
                PLCQuery qrySubmission =
                    new PLCQuery(
                        string.Format("SELECT * FROM TV_LABSUB WHERE CASE_KEY = {0} AND SUBMISSION_NUMBER = {1}",
                                      caseKey, submissionNumber));
                qrySubmission.Open();
                if (!qrySubmission.IsEmpty())
                {
                    string labsubReceivedDate = qrySubmission.FieldByName("RECEIVED_DATE").Trim();
                    if (labsubReceivedDate != "" && !string.IsNullOrEmpty(labsubReceivedDate))
                        initialDate = Convert.ToDateTime(qrySubmission.FieldByName("RECEIVED_DATE"));

                    string labsubReceivedTime = qrySubmission.FieldByName("RECEIVED_TIME").Trim();
                    if (labsubReceivedTime != "" && !string.IsNullOrEmpty(labsubReceivedTime))
                        initialTime = Convert.ToDateTime(qrySubmission.FieldByName("RECEIVED_TIME"));
                }
                else
                {
                    initialDate = initialDate ?? DateTime.Today;
                    initialTime = initialTime ?? DateTime.Now;
                }
            }

            if (parentEcn > 0 && PLCSession.GetLabCtrl("SUB_ITEM_DEFAULT_CUSTODY") != "" &&
                PLCSession.GetLabCtrl("SUB_ITEM_DEFAULT_LOCATION") != "")
            {
                custody = PLCSession.GetLabCtrl("SUB_ITEM_DEFAULT_CUSTODY");
                location = PLCSession.GetLabCtrl("SUB_ITEM_DEFAULT_LOCATION");
            }

            if (custody == "" || location == "") //if no settings, use current ANALYST as custody location
            {
                if (analystCustodyAlreadyExists)
                    return;

                custody = PLCSession.PLCGlobalDefaultAnalystCustodyOf;
                location = PLCSession.PLCGlobalAnalyst;
            }

            if ((source.Equals("KIT") || source == "SAMPLE") && custodyTimeType.Trim().ToUpper().Equals("CURRENT"))
            {
                initialDate = initialDate ?? DateTime.Today;
                initialTime = initialTime ?? DateTime.Now;
            }

            // Update item custody.
            UpdateItemCustody(ecn, custody, location, initialDate, initialTime);

            if (updateContainerCustody && !string.IsNullOrEmpty(containerKey))
                UpdateContainerCustody(containerKey, custody, location);


            if (source == "KIT" || source == "SAMPLE")
            {
                //Update item container key and barcode
                itemContainerKey = UpdateBarcodeContainer(parentCustody, parentLocation, ecn);
                AddCustody(caseKey, ecn, custody, location, batchKey, itemContainerKey, initialDate, initialTime, parentEcn.ToString(), weight, weightUnit);
            }
            else
            {
                AddCustody(caseKey, ecn, custody, location, batchKey, containerKey, initialDate, initialTime, null, weight, weightUnit);
            }
        }

        /// <summary>
        /// Use this function when using PLCQuery Transaction
        /// </summary>
        /// <param name="caseKey"></param>
        /// <param name="ecn"></param>
        /// <param name="batchKey"></param>
        /// <param name="containerKey"></param>
        /// <param name="source"></param>
        /// <param name="submissionNumber"></param>
        /// <param name="itemType"></param>
        /// <param name="initialDate">ignore</param>
        /// <param name="initialTime">ignore</param>
        /// <param name="parentEcn"></param>
        /// <param name="parentCustody"></param>
        /// <param name="parentLocation"></param>
        public void AddDefaultCustody(string caseKey, string ecn, string batchKey, string containerKey, string source, int submissionNumber, string itemType, DateTime? initialDate, DateTime? initialTime, int parentEcn,
            string parentCustody, string parentLocation, string weight, string weightUnit, bool analystCustodyAlreadyExists, bool updateContainerCustody)
        {
            //get default custody settings from TV_GLOBALINI

            string custodyType = "";
            string custodyTimeType = "";
            string itemContainerKey;
            source = source.ToUpper();

            if (source == "DUPE")
            {
                string dupeCustody = GetGlobalINIValue("DUPE_CUSTODY");
                string dupeCustodyTime = GetGlobalINIValue("DUPE_CUSTODY_TIME");
                custodyType = string.IsNullOrEmpty(dupeCustody) ? "DEFAULT" : dupeCustody;
                custodyTimeType = string.IsNullOrEmpty(dupeCustodyTime) ? "CURRENT" : dupeCustodyTime;
            }
            else if (source == "SAMPLE")
            {
                string sampleCustody = GetGlobalINIValue("SAMPLE_CUSTODY");
                string sampleCustodyTime = GetGlobalINIValue("SAMPLE_CUSTODY_TIME");
                custodyType = string.IsNullOrEmpty(sampleCustody) ? "ANALYST" : sampleCustody;
                custodyTimeType = string.IsNullOrEmpty(sampleCustodyTime) ? "CURRENT" : sampleCustodyTime;
            }
            else if (source == "KIT")
            {
                string kitCustody = GetGlobalINIValue("KIT_CUSTODY");
                string kitCustodyTime = GetGlobalINIValue("KIT_CUSTODY_TIME");
                custodyType = string.IsNullOrEmpty(kitCustody) ? "PARENT" : kitCustody;
                custodyTimeType = string.IsNullOrEmpty(kitCustodyTime) ? "SUBMISSION" : kitCustodyTime;
            }
            else if (source == "RETAIN")
            {
                string retainCustody = GetGlobalINIValue("RETAIN_CUSTODY");
                string retainCustodyTime = GetGlobalINIValue("RETAIN_CUSTODY_TIME");
                custodyType = string.IsNullOrEmpty(retainCustody) ? "DEFAULT" : retainCustody;
                custodyTimeType = string.IsNullOrEmpty(retainCustodyTime) ? "SUBMISSION" : retainCustodyTime;
            }
            else //default: ADD
            {
                string addCustody = GetGlobalINIValue("ADD_CUSTODY");
                string addCustodyTime = GetGlobalINIValue("ADD_CUSTODY_TIME");
                custodyType = string.IsNullOrEmpty(addCustody) ? "DEFAULT" : addCustody;
                custodyTimeType = string.IsNullOrEmpty(addCustodyTime) ? "SUBMISSION" : addCustodyTime;
            }

            string custody = "";
            string location = "";

            if (custodyType == "NONE")
            {
                custody = "";
                location = "";
            }
            else if (custodyType == "ANALYST")
            {
                if (analystCustodyAlreadyExists)
                    return;

                custody = PLCSession.PLCGlobalDefaultAnalystCustodyOf;
                location = PLCSession.PLCGlobalAnalyst;
            }
            else if (custodyType == "PARENT")
            {
                custody = parentCustody;
                location = parentLocation;
            }
            else if (custodyType == "DEFAULT")
            {
                PLCQuery qryItemType =
                    new PLCQuery(
                        string.Format(
                            "SELECT DEFAULT_CUSTODY, DEFAULT_LOCATION FROM TV_ITEMTYPE WHERE ITEM_TYPE = '{0}'",
                            itemType));
                qryItemType.Open();
                if (!qryItemType.IsEmpty())
                {
                    custody = qryItemType.FieldByName("DEFAULT_CUSTODY");
                    location = qryItemType.FieldByName("DEFAULT_LOCATION");
                }

                if (custody == "" || location == "")
                {
                    custody = PLCSession.GetLabCtrl("DEFAULT_ITEM_STATUS");
                    location = PLCSession.GetLabCtrl("DEFAULT_ITEM_LOCKER");
                }
            }
            else if (custodyType == "SUBMISSION")
            {
                PLCQuery qrySubmissionAnalyst =
                    new PLCQuery(
                        string.Format(
                            @"SELECT A.ANALYST, A.CUSTODY_OF FROM TV_LABSUB S
INNER JOIN TV_ANALYST A ON S.RECEIVED_BY = A.ANALYST
WHERE CASE_KEY = {0} AND SUBMISSION_NUMBER = {1}",
                            caseKey, submissionNumber));
                qrySubmissionAnalyst.Open();
                if (!qrySubmissionAnalyst.IsEmpty())
                {
                    custody = qrySubmissionAnalyst.FieldByName("CUSTODY_OF");
                    location = qrySubmissionAnalyst.FieldByName("ANALYST");
                }
            }

            if (custodyTimeType == "SUBMISSION")
            {
                PLCQuery qrySubmission =
                    new PLCQuery(
                        string.Format("SELECT * FROM TV_LABSUB WHERE CASE_KEY = {0} AND SUBMISSION_NUMBER = {1}",
                                      caseKey, submissionNumber));
                qrySubmission.Open();
                if (!qrySubmission.IsEmpty())
                {
                    string labsubReceivedDate = qrySubmission.FieldByName("RECEIVED_DATE").Trim();
                    if (labsubReceivedDate != "" && !string.IsNullOrEmpty(labsubReceivedDate))
                        initialDate = Convert.ToDateTime(qrySubmission.FieldByName("RECEIVED_DATE"));

                    string labsubReceivedTime = qrySubmission.FieldByName("RECEIVED_TIME").Trim();
                    if (labsubReceivedTime != "" && !string.IsNullOrEmpty(labsubReceivedTime))
                        initialTime = Convert.ToDateTime(qrySubmission.FieldByName("RECEIVED_TIME"));
                }
                else
                {
                    initialDate = initialDate == null ?  DateTime.Today : initialDate;
                    initialTime = initialTime == null ? DateTime.Now : initialTime;
                }
            }

            if (parentEcn > 0 && PLCSession.GetLabCtrl("SUB_ITEM_DEFAULT_CUSTODY") != "" &&
                PLCSession.GetLabCtrl("SUB_ITEM_DEFAULT_LOCATION") != "")
            {
                custody = PLCSession.GetLabCtrl("SUB_ITEM_DEFAULT_CUSTODY");
                location = PLCSession.GetLabCtrl("SUB_ITEM_DEFAULT_LOCATION");
            }

            if (custody == "" || location == "") //if no settings, use current ANALYST as custody location
            {
                if (analystCustodyAlreadyExists)
                    return;

                custody = PLCSession.PLCGlobalDefaultAnalystCustodyOf;
                location = PLCSession.PLCGlobalAnalyst;
            }

            if ((source.Equals("KIT") || source == "SAMPLE") && custodyTimeType.Trim().ToUpper().Equals("CURRENT"))
            {
                initialDate = initialDate == null ? DateTime.Today : initialDate;
                initialTime = initialTime == null ? DateTime.Now : initialTime;
            }

            // Update item custody.
            UpdateItemCustody(ecn, custody, location, itemType, initialDate, initialTime);

            if (updateContainerCustody && !string.IsNullOrEmpty(containerKey))
                UpdateContainerCustody(containerKey, custody, location);


            if (source == "KIT" || source == "SAMPLE")
            {
                //Update item container key and barcode
                itemContainerKey = UpdateBarcodeContainer(parentCustody, parentLocation, ecn);
                AddCustody(caseKey, ecn, custody, location, batchKey, itemContainerKey, initialDate, initialTime, parentEcn.ToString(), weight, weightUnit);
            }
            else
            {
                AddCustody(caseKey, ecn, custody, location, batchKey, containerKey, initialDate, initialTime, null, weight, weightUnit);
            }
        }

        public void AddDefaultCustody(string caseKey, string ecn, string batchKey, string containerKey, int submissionNumber, string itemType, string weight, string weightUnit, bool analystCustodyAlreadyExists, bool updateContainerCustody, DateTime? custodyDate = null, DateTime? custodyTime = null)
        {
            int parentEcn = 0;
            string parentCustody = "";
            string parentLocation = "";

            PLCQuery qryItem = new PLCQuery(@"SELECT I.PARENT_ECN, P.CUSTODY_OF, P.LOCATION FROM TV_LABITEM I
LEFT OUTER JOIN TV_LABITEM P ON I.PARENT_ECN = P.EVIDENCE_CONTROL_NUMBER
WHERE I.EVIDENCE_CONTROL_NUMBER = " + ecn);
            qryItem.Open();

            if (!qryItem.IsEmpty())
            {
                parentEcn = string.IsNullOrEmpty(qryItem.FieldByName("PARENT_ECN")) ? 0 : Convert.ToInt32(qryItem.FieldByName("PARENT_ECN"));
                parentCustody = qryItem.FieldByName("CUSTODY_OF");
                parentLocation = qryItem.FieldByName("LOCATION");
            }

            AddDefaultCustody(caseKey, ecn, batchKey, containerKey, "ADD", submissionNumber, itemType, custodyDate, custodyTime, parentEcn, parentCustody, parentLocation, weight, weightUnit, analystCustodyAlreadyExists, updateContainerCustody);
        }

        public string UpdateBarcodeContainer(string parentCustody, string parentLocation,string ecn)
        {
            string parentContainerKey = null;
            string barcode = null;

            PLCQuery qryGetItemInfo = new PLCQuery("SELECT I.CUSTODY_OF, I.LOCATION, P.CONTAINER_KEY AS PARENT_CONTAINER " +
                "FROM TV_LABITEM I " +
                "INNER JOIN TV_LABITEM P ON I.PARENT_ECN = P.EVIDENCE_CONTROL_NUMBER " +
                "WHERE I.EVIDENCE_CONTROL_NUMBER = " + ecn);

            qryGetItemInfo.Open();
            if (!qryGetItemInfo.IsEmpty())
            {
                if (!string.IsNullOrWhiteSpace(qryGetItemInfo.FieldByName("PARENT_CONTAINER")) && qryGetItemInfo.FieldByName("PARENT_CONTAINER") != "0")
                    parentContainerKey = qryGetItemInfo.FieldByName("PARENT_CONTAINER");
                else
                    parentContainerKey = null;

                PLCQuery qryUpdateItemBarcode = new PLCQuery("SELECT BARCODE, CONTAINER_KEY FROM TV_LABITEM WHERE EVIDENCE_CONTROL_NUMBER = " + ecn);
                qryUpdateItemBarcode.Open();
                if (!qryUpdateItemBarcode.IsEmpty())
                {
                    if (parentCustody == qryGetItemInfo.FieldByName("CUSTODY_OF") && parentLocation == qryGetItemInfo.FieldByName("LOCATION"))
                    {
                        qryUpdateItemBarcode.Edit();
                        qryUpdateItemBarcode.SetFieldValue("CONTAINER_KEY", parentContainerKey);
                        qryUpdateItemBarcode.Post("TV_LABITEM", 7, 1);
                    }
                    else
                    {
                        qryUpdateItemBarcode.Edit();
                        qryUpdateItemBarcode.SetFieldValue("BARCODE", ecn);
                        qryUpdateItemBarcode.Post("TV_LABITEM", 7, 1);
                        parentContainerKey = null;
                        barcode = ecn;
                    }
                }
            }

            return parentContainerKey + "-" + barcode;
        }

        public void UpdateContainerCustody(string ContainerKey, string CustodyCode, string CustodyLocation)
        {
            PLCQuery qryContainer = new PLCQuery();
            qryContainer.SQL = "SELECT * FROM TV_CONTAINER WHERE CONTAINER_KEY = " + ContainerKey;
            qryContainer.Open();
            if (!qryContainer.IsEmpty())
            {
                qryContainer.Edit();
                qryContainer.SetFieldValue("CUSTODY_OF", CustodyCode);
                qryContainer.SetFieldValue("LOCATION", CustodyLocation);
                qryContainer.Post("TV_CONTAINER", -1, -1);
            }
        }

        // Update labitem custody.
        public void UpdateItemCustody(string ecn, string custodyCode, string custodyLocation)
        {
            UpdateItemCustody(ecn, custodyCode, custodyLocation, null, null);
        }

        public void UpdateItemCustody(string ecn, string custodyCode, string custodyLocation, DateTime? custodyDate, DateTime? custodyTime)
        {
            PLCQuery qryLabItem = new PLCQuery();
            qryLabItem.SQL = "SELECT * FROM TV_LABITEM where EVIDENCE_CONTROL_NUMBER = " + ecn;
            qryLabItem.Open();

            if (!qryLabItem.IsEmpty())
            {
                qryLabItem.Edit();
                qryLabItem.SetFieldValue("CUSTODY_OF", custodyCode);
                qryLabItem.SetFieldValue("LOCATION", custodyLocation);
                qryLabItem.SetFieldValue("CUSTODY_BY", PLCSession.PLCGlobalAnalyst);

                if (custodyDate != null && custodyTime != null)
                {
                    qryLabItem.SetFieldValue("CUSTODY_DATE", custodyDate.Value.Add(custodyTime.Value.TimeOfDay));
                }

                if (IsNarcoticsReviewRequired(qryLabItem.FieldByName("ITEM_TYPE")))
                {
                    qryLabItem.SetFieldValue("CURRENT_DEPARTMENT", CustodyDepartmentCode(custodyCode));
                    qryLabItem.SetFieldValue("IN_DEPARTMENT_DATE", DateTime.Now);
                }

                qryLabItem.Post("TV_LABITEM", 7, 1);
            }
        }

        /// <summary>
        /// Use this function when using PLCQuery Transaction
        /// </summary>
        /// <param name="ecn"></param>
        /// <param name="custodyCode"></param>
        /// <param name="custodyLocation"></param>
        /// <param name="itemType"></param>
        /// <param name="custodyDate"></param>
        /// <param name="custodyTime"></param>
        public void UpdateItemCustody(string ecn, string custodyCode, string custodyLocation, string itemType, DateTime? custodyDate, DateTime? custodyTime)
        {
            PLCQuery qryLabItem = new PLCQuery();

            if (PLCSession.GetWebConfiguration("DBTRANS") == "T")
            {
                try
                {
                    //this is for new items and DBTRANS flag is on
                    bool hasCustodyDate = (custodyDate != null && custodyTime != null);
                    string inDepartmentDate = DateTime.Now.ToString("d-MMM-yyyy HH:mm:ss");

                    DateTime dtCustody = new DateTime();
                    string custodyDateValue = "";
                    if (hasCustodyDate)
                    {
                        try
                        {
                            dtCustody = Convert.ToDateTime(custodyDate.Value.Add(custodyTime.Value.TimeOfDay));
                            custodyDateValue = dtCustody.ToString("d-MMM-yyyy HH:mm:ss");
                        }
                        catch
                        {
                            custodyDateValue = "";
                        }

                    }

                    if (PLCSession.PLCDatabaseServer == "MSSQL")
                    {
                        //assign the value as is
                        inDepartmentDate = "'" + inDepartmentDate + "'";
                        custodyDateValue = "'" + custodyDateValue + "'";
                    }
                    else
                    {
                        inDepartmentDate = "to_date('" + inDepartmentDate + "','DD-MON-YYYY HH24:MI:SS')";
                        custodyDateValue = "to_date('" + custodyDateValue + "','DD-MON-YYYY HH24:MI:SS')";

                    }

                    string set = "SET";
                    if (!string.IsNullOrEmpty(custodyCode))
                    {
                        set += " CUSTODY_OF = '" + custodyCode + "'";
                    }

                    if (!string.IsNullOrEmpty(custodyLocation))
                    {
                        if (set != "SET") set += ",";
                        set += " LOCATION = '" + custodyLocation + "'";

                    }

                    if (!string.IsNullOrEmpty(custodyDateValue) && hasCustodyDate)
                    {
                        if (set != "SET") set += ",";
                        set += " CUSTODY_DATE = " + custodyDateValue;

                    }

                    if (IsNarcoticsReviewRequired(itemType))
                    {
                        if (set != "SET") set += ",";
                        set += " IN_DEPARTMENT_DATE = " + inDepartmentDate;

                        string custodyDepartment = CustodyDepartmentCode(custodyCode);

                        if (!string.IsNullOrEmpty(custodyDepartment))
                        {
                            if (set != "SET") set += ",";
                            set += " CURRENT_DEPARTMENT = '" + custodyDepartment + "'";

                        }
                    }

                    qryLabItem.SQL = PLCSession.FormatSpecialFunctions("UPDATE TV_LABITEM " + set + " WHERE EVIDENCE_CONTROL_NUMBER = " + ecn);
                    qryLabItem.ExecSQL(PLCSession.GetConnectionString());
                }
                catch
                {

                }

            }
            else
            {
                qryLabItem = new PLCQuery();
                qryLabItem.SQL = "SELECT * FROM TV_LABITEM where EVIDENCE_CONTROL_NUMBER = " + ecn;
                qryLabItem.Open();
                if (!qryLabItem.IsEmpty())
                {
                    //this is for existing items
                    qryLabItem.Edit();
                    qryLabItem.SetFieldValue("CUSTODY_OF", custodyCode);
                    qryLabItem.SetFieldValue("LOCATION", custodyLocation);
                    qryLabItem.SetFieldValue("CUSTODY_BY", PLCSession.PLCGlobalAnalyst);

                    if (custodyDate != null && custodyTime != null)
                    {
                        qryLabItem.SetFieldValue("CUSTODY_DATE", custodyDate.Value.Add(custodyTime.Value.TimeOfDay));
                    }

                    if (IsNarcoticsReviewRequired(itemType))
                    {
                        qryLabItem.SetFieldValue("CURRENT_DEPARTMENT", CustodyDepartmentCode(custodyCode));
                        qryLabItem.SetFieldValue("IN_DEPARTMENT_DATE", DateTime.Now);
                    }

                    qryLabItem.Post("TV_LABITEM", 7, 1);

                }

            }



        }

        public void UpdateItemCustody(string ecn, string custodyCode, string custodyLocation, string itemType)
        {
            UpdateItemCustody(ecn, custodyCode, custodyLocation, itemType, null, null);
        }

        // Add new Custody record.
        public void AddCustody(string caseKey, string ecn, string custodyCode, string custodyLocation)
        {
            AddCustody(caseKey, ecn, custodyCode, custodyLocation, null, null);
        }

        public void AddCustody(string caseKey, string ecn, string custodyCode, string custodyLocation, string batchKey, string containerKey)
        {
            AddCustody(caseKey, ecn, custodyCode, custodyLocation, batchKey, containerKey, null, null, null);
        }

        public void AddCustody(string caseKey, string ecn, string custodyCode, string custodyLocation, string batchKey, string containerKey, DateTime? custodyDate, DateTime? custodyTime, string parentECN, string weight, string weightUnit)
        {
            AddCustody(caseKey, ecn, custodyCode, custodyLocation, batchKey, containerKey, custodyDate, custodyTime, null, null, null, parentECN, weight, weightUnit);
        }

        public void AddCustody(string caseKey, string ecn, string custodyCode, string custodyLocation, string batchKey, string containerKey, string submissionKey, string trackingNumber, string comments, string parentECN,
            string weight, string weightUnit, bool floorCustody, bool itemsTab)
        {
            AddCustody(caseKey, ecn, custodyCode, custodyLocation, batchKey, containerKey, null, null, submissionKey, trackingNumber, comments, parentECN, weight, weightUnit, floorCustody, itemsTab);
        }

        public void AddCustody(string caseKey, string ecn, string custodyCode, string custodyLocation, string batchKey, string containerBarcodeKey,
            DateTime? custodyDate, DateTime? custodyTime, string submissionKey = null, string trackingNumber = null, string comments = null, string parentECN = null, string weight = null, string weightUnit = null,
            bool floorCustody = false, bool itemsTab = false)
        {
            string batchKeyToSet;
            string contKey = string.Empty;
            string barcode = string.Empty;
            string receivedBy = string.Empty;
            string defaultComments = string.Empty;
            string newseq = GetNextSequence("LABSTAT_SEQ").ToString();
            bool clearLabstatComments = true;

            if (batchKey == null)
                batchKeyToSet = GetNextSequence("BATCH_SEQ").ToString();
            else
                batchKeyToSet = batchKey;

            if (!string.IsNullOrWhiteSpace(containerBarcodeKey) && containerBarcodeKey.Contains("-"))
            {
                string[] parts = containerBarcodeKey.Split('-');
                contKey = parts[0].ToString();
                barcode = parts[1].ToString();
            }

            //get only TV_LABSUB.RECEIVED_DATE and TV_LABSUB.RECEIVED_TIME if items are added from QC
            //if items are added/sampled/kit/duped from ITEMSTAB then current date/time will be applied to TV_LABSTAT.STATUS_DATE and TV_LABSTAT.STATUS_TIME 
            if (submissionKey != null && itemsTab == false)
            {
                //get received_date and received_time from TV_LABSUB
                PLCQuery qrySubDateTime = new PLCQuery("SELECT RECEIVED_BY, RECEIVED_DATE, RECEIVED_TIME FROM TV_LABSUB WHERE SUBMISSION_KEY = " + submissionKey);
                qrySubDateTime.Open();
                if (!qrySubDateTime.IsEmpty())
                {
                    string receivedDate = qrySubDateTime.FieldByName("RECEIVED_DATE").ToString();
                    string receivedTime = qrySubDateTime.FieldByName("RECEIVED_TIME").ToString();

                    if(!string.IsNullOrEmpty(receivedDate))
                        custodyDate = Convert.ToDateTime(receivedDate);

                    if(!string.IsNullOrEmpty(receivedTime))
                        custodyTime = Convert.ToDateTime(receivedTime);

                    if (CheckUserOption("QCRECBY"))
                        receivedBy = qrySubDateTime.FieldByName("RECEIVED_BY").ToString();
                }
            }

            // get comments based on custody location change
            defaultComments = PLCSession.GetDefault("LABSTAT_COMMENTS");
            clearLabstatComments = PLCSession.GetProperty<bool>("CLEAR_LABSTAT_COMMENTS", true);
            if (!string.IsNullOrEmpty(defaultComments))
            {

                if (string.IsNullOrEmpty(comments))
                    comments = defaultComments;

                if (clearLabstatComments)
                {
                    // clear default
                    PLCSession.SetDefault("LABSTAT_COMMENTS", null);
                }
            }

            PLCQuery qryLabStatAppend = new PLCQuery();
            qryLabStatAppend.SQL = "Select * FROM TV_LABSTAT where 0 = 1";
            if (qryLabStatAppend.Open())
            {
                qryLabStatAppend.Append();
                qryLabStatAppend.SetFieldValue("CASE_KEY", caseKey);
                qryLabStatAppend.SetFieldValue("EVIDENCE_CONTROL_NUMBER", ecn);
                qryLabStatAppend.SetFieldValue("STATUS_KEY", newseq);
                qryLabStatAppend.SetFieldValue("STATUS_CODE", custodyCode);
                qryLabStatAppend.SetFieldValue("LOCKER", custodyLocation);
                qryLabStatAppend.SetFieldValue("STATUS_DATE", custodyDate == null ? System.DateTime.Today : custodyDate);
                qryLabStatAppend.SetFieldValue("STATUS_TIME", custodyTime == null ? System.DateTime.Now : custodyTime);
                qryLabStatAppend.SetFieldValue("ENTERED_BY", PLCGlobalAnalyst);
                qryLabStatAppend.SetFieldValue("ENTRY_TIME", System.DateTime.Now);
                qryLabStatAppend.SetFieldValue("ENTRY_ANALYST", string.IsNullOrEmpty(receivedBy) ? PLCGlobalAnalyst : receivedBy);
                qryLabStatAppend.SetFieldValue("ENTRY_TIME_STAMP", System.DateTime.Now);
                qryLabStatAppend.SetFieldValue("BATCH_SEQUENCE", batchKeyToSet);
                qryLabStatAppend.SetFieldValue("OS_USER_NAME", GetOSUserName());
                qryLabStatAppend.SetFieldValue("OS_COMPUTER_NAME", GetOSComputerName());
                qryLabStatAppend.SetFieldValue("SOURCE", "A");

                if (!string.IsNullOrWhiteSpace(contKey))
                    qryLabStatAppend.SetFieldValue("CONTAINER_KEY", contKey);

                if (string.IsNullOrWhiteSpace(barcode) && !string.IsNullOrWhiteSpace(parentECN))
                    qryLabStatAppend.SetFieldValue("PARENT_ECN", parentECN);

                if (submissionKey != null && floorCustody == false)//populate TV_LABSTAT.SUBMISION_KEY only if submissionKey is being passed and if FLOOR_CUSTODY_OF and FLOOR_LOCATION is empty
                    qryLabStatAppend.SetFieldValue("SUBMISSION_KEY", submissionKey);

                if (trackingNumber != null)
                    qryLabStatAppend.SetFieldValue("TRACKING_NUMBER", trackingNumber);

                if (comments != null)
                    qryLabStatAppend.SetFieldValue("COMMENTS", comments);

                if (!string.IsNullOrEmpty(weight))
                {
                    try
                    {
                        double dWeight = PLCSession.GetWeight(weight, ref weightUnit);
                        qryLabStatAppend.SetFieldValue("WEIGHT", dWeight);
                    }
                    catch
                    {
                    }
                    qryLabStatAppend.SetFieldValue("PACKAGE_WEIGHT", weight);
                }

                if (!string.IsNullOrEmpty(weightUnit))
                    qryLabStatAppend.SetFieldValue("WEIGHT_UNITS", weightUnit);

                qryLabStatAppend.Post("TV_LABSTAT", 13, 10);
            }
        }

        public Boolean SetupConnectionFromCINFO(string CINFO)
        {

            try
            {
                WriteDebug("SetupConnectionFromCINFO In:" + CINFO + ":", true);

                ModifyRegistry myRegistry = new ModifyRegistry();

                WriteDebug("SetupConnectionFromCINFO 1:" + CINFO + ":", true);

                myRegistry.ShowError = true;
                WriteDebug("SetupConnectionFromCINFO 2:", true);
                myRegistry.BaseRegistryKey = Microsoft.Win32.Registry.LocalMachine;

                WriteDebug("SetupConnectionFromCINFO 3:", true);
                myRegistry.SubKey = @"SOFTWARE\CRIMEFIGHTERBEAST\ILIMS\DATABASES";

                WriteDebug("SetupConnectionFromCINFO 4:", true);
                string EncryptedStr = myRegistry.Read(CINFO);

                SetDefault("ENCINFO", EncryptedStr);

                string AESUniStr = "";
                try
                {
                    AESUniStr = myRegistry.Read("_" + CINFO);
                }
                catch
                {
                    AESUniStr = "";
                }


                if (EncryptedStr == null) //AAC - Please do not remove this block of Code - Murugan
                {
                    myRegistry.BaseRegistryKey = Microsoft.Win32.Registry.CurrentUser;
                    myRegistry.SubKey = @"SOFTWARE\CRIMEFIGHTERBEAST\ILIMS\DATABASES";

                    EncryptedStr = myRegistry.Read(CINFO);
                    AESUniStr = myRegistry.Read("_" + CINFO);
                }

                WriteDebug("SetupConnectionFromCINFO 5:", true);
                string DecryptStr = AESEncryption.Decrypt(EncryptedStr);

                WriteDebug("SetupConnectionFromCINFO 6:", true);
                string[] items = DecryptStr.Split(',');

                WriteDebug("SetupConnectionFromCINFO 7:", true);

                //We need to support commas in the service name for sql alias.
                //so they are replaced with semicolons when saved and switched back to commas when loaded.

                String ts = items[0];   //Service Name
                ts = ts.Replace(";", ",");
                PLCDBDataSource = ts;   //Service Name

                WriteDebug("SetupConnectionFromCINFO 8:", true);
                PLCDBUserID = items[1];       // Database user

                WriteDebug("SetupConnectionFromCINFO 9:", true);
                PLCDBDatabase = items[2];     // Database Name (MSSql)

                WriteDebug("SetupConnectionFromCINFO 10:", true);
                PLCDBPW = items[3];           // Database Password

                WriteDebug("SetupConnectionFromCINFO 11:", true);
                PLCDatabaseServer = items[4]; // Provider (Oracle/MsSql)


                if (items.Count() > 5)
                    PLCServerIP = items[5];   // Server IP (Host:Port:SID / Host,SID)
                else
                    PLCServerIP = "";


                if (items.Count() > 6)
                    PLCDomainName = items[6];
                else
                    PLCDomainName = "";

                if (items.Count() > 7)
                    PLCDomainPrefix = items[7];
                else
                    PLCDomainPrefix = "";

                if (items.Count() > 8)
                    SetDefault("JWTSECRET", items[8]);
                else
                    SetDefault("JWTSECRET", "");


                if (PLCDatabaseServer == "ORACLE")
                    PLCDBProvider = "OraOleDB.Oracle";
                else if (PLCDatabaseServer == "MSSQL")
                    PLCDBProvider = "SQLOLEDB";
                else
                {
                    PLCDBProvider = PLCDatabaseServer;
                    PLCDatabaseServer = "MSSQL";
                }


                PLCDBName = CINFO;

                try
                {
                    SetDefault("AESINFO", AESUniStr);
                }
                catch
                {
                }


                WriteDebug("----------------------", true);
                WriteDebug("Server Name:" + PLCDBDataSource, true);
                WriteDebug("UserID:" + PLCDBUserID, true);
                WriteDebug("DatabaseName:" + PLCDBDatabase, true);
                WriteDebug("Password:" + "???? :) ????", true);
                WriteDebug("PLCDBProvider:" + PLCDBProvider, true);
                WriteDebug("DatabaseServer:" + PLCDatabaseServer, true);
                WriteDebug("----------------------", true);



            }
            catch (Exception e)
            {
                WriteDebug("Exception SetupConnectionFromCINFO:" + e.Message, true);
                return false;
            }

            return true;


        }

        public void GetConnectionFromCINFO(string CINFO, out string dataSource, out string dbUserID, out string dbUserPwd, out string dbName, out string dbServer, 
            out string dbIP, out string dbDomainName, out string dbDomainPrefix, out string dbProvider)
        {

            dataSource = dbUserID = dbUserPwd = dbName = dbServer = dbIP = dbDomainName = dbDomainPrefix = dbProvider = string.Empty;

            try
            {
                WriteDebug("GetConnectionFromCINFO In:" + CINFO + ":", true);

                ModifyRegistry myRegistry = new ModifyRegistry();

                WriteDebug("GetConnectionFromCINFO 1:" + CINFO + ":", true);

                myRegistry.ShowError = true;
                WriteDebug("GetConnectionFromCINFO 2:", true);
                myRegistry.BaseRegistryKey = Microsoft.Win32.Registry.LocalMachine;

                WriteDebug("GetConnectionFromCINFO 3:", true);
                myRegistry.SubKey = @"SOFTWARE\CRIMEFIGHTERBEAST\ILIMS\DATABASES";

                WriteDebug("GetConnectionFromCINFO 4:", true);
                string EncryptedStr = myRegistry.Read(CINFO);


                if (EncryptedStr == null) //AAC - Please do not remove this block of Code - Murugan
                {
                    myRegistry.BaseRegistryKey = Microsoft.Win32.Registry.CurrentUser;
                    myRegistry.SubKey = @"SOFTWARE\CRIMEFIGHTERBEAST\ILIMS\DATABASES";

                    EncryptedStr = myRegistry.Read(CINFO);
                }

                WriteDebug("GetConnectionFromCINFO 5:", true);
                string DecryptStr = AESEncryption.Decrypt(EncryptedStr);

                WriteDebug("GetConnectionFromCINFO 6:", true);
                string[] items = DecryptStr.Split(',');

                WriteDebug("GetConnectionFromCINFO 7:", true);

                //We need to support commas in the service name for sql alias.
                //so they are replaced with semicolons when saved and switched back to commas when loaded.

                String ts = items[0];   //Service Name
                ts = ts.Replace(";", ",");
                dataSource = ts;   //Service Name

                WriteDebug("GetConnectionFromCINFO 8:", true);
                dbUserID = items[1];       // Database user

                WriteDebug("GetConnectionFromCINFO 9:", true);
                dbName = items[2];     // Database Name (MSSql)

                WriteDebug("GetConnectionFromCINFO 10:", true);
                dbUserPwd = items[3];           // Database Password

                WriteDebug("GetConnectionFromCINFO 11:", true);
                dbServer = items[4]; // Provider (Oracle/MsSql)


                if (items.Count() > 5)
                    dbIP = items[5];   // Server IP (Host:Port:SID / Host,SID)
                else
                    dbIP = "";


                if (items.Count() > 6)
                    dbDomainName = items[6];
                else
                    dbDomainName = "";

                if (items.Count() > 7)
                    dbDomainPrefix = items[7];
                else
                    dbDomainPrefix = "";


                if (dbServer == "ORACLE")
                    dbProvider = "OraOleDB.Oracle";
                else if (dbServer == "MSSQL")
                    dbProvider = "SQLOLEDB";
                else
                {
                    dbProvider = PLCDatabaseServer;
                    dbServer = "MSSQL";
                }


                WriteDebug("----------------------", true);
                WriteDebug("Custom CINFO:" + CINFO, true);
                WriteDebug("Custom Server Name:" + dataSource, true);
                WriteDebug("Custom UserID:" + dbUserID, true);
                WriteDebug("Custom DatabaseName:" + dbName, true);
                WriteDebug("Custom Password:" + "???? :) ????", true);
                WriteDebug("Custom PLCDBProvider:" + dbProvider, true);
                WriteDebug("Custom DatabaseServer:" + dbServer, true);
                WriteDebug("----------------------", true);



            }
            catch (Exception e)
            {
                WriteDebug("Exception GetConnectionFromCINFO:" + e.Message, true);
                return;
            }

            return;


        }

        public Boolean CanDeleteAssignment(string ExamKey)
        {

            PLCQuery qry = new PLCQuery();
            qry.SQL = "SELECT SECTION, ANALYST_ASSIGNED FROM TV_LABASSIGN where EXAM_KEY = " + ExamKey;
            qry.Open();

            if (qry.IsEmpty())
                return false;

            // Can I delete my own assignment...
            Boolean RestrictDeletingOwnAssignment = CheckUserOption("RESOASGN");

            //In general can I delete assignments...
            Boolean CanDeleteAssignments = CheckUserOption("DELASSGN");

            string TheAnalyst = qry.FieldByName("ANALYST_ASSIGNED");

            if ((PLCGlobalAnalyst == TheAnalyst) && (RestrictDeletingOwnAssignment))
            {
                return false;
            }

            if (PLCGlobalAnalyst == TheAnalyst)
            {
                return true;
            }

            return CanDeleteAssignments;

        }

        //---------------------------



        //Used by LASD/NYPD NARCO Setup
        private Boolean NeedsReport(string ckey, string ecn)
        {

            string tempSQL = "Select count(*) CNT from TV_LABASSIGN, TV_ITASSIGN ";
            tempSQL += " where TV_LABASSIGN.CASE_KEY = " + ckey;
            tempSQL += " and TV_ITASSIGN.EXAM_KEY = TV_LABASSIGN.EXAM_KEY";
            if (GetLabCtrl("USES_SERVICE_REQUESTS") == "N")
            {
                tempSQL += " and TV_LABASSIGN.STATUS <> 'I' ";
            }

            tempSQL += " and TV_ITASSIGN.EVIDENCE_CONTROL_NUMBER = " + ecn;

            PLCQuery qryCountReports = new PLCQuery();
            qryCountReports.SQL = tempSQL;
            qryCountReports.Open();

            int thecount = qryCountReports.iFieldByName("CNT");

            if (thecount == 0)
                return true;

            return false;
        }

        //Used by LASD NARCO Setup
        private Boolean IsJuvenileItem(string ecn)
        {
            string tempSQL = "Select count(*) CNT from TV_ITEMNAME, TV_LABNAME ";
            tempSQL += " where TV_ITEMNAME.EVIDENCE_CONTROL_NUMBER = " + ecn;
            tempSQL += " and TV_LABNAME.NAME_KEY = TV_ITEMNAME.NAME_KEY";
            tempSQL += " and (TV_LABNAME.NAME_TYPE = 'SJ' or TV_LABNAME.NAME_TYPE = 'SJ/V')";

            PLCQuery qryIsJuvenile = new PLCQuery();
            qryIsJuvenile.SQL = tempSQL;
            qryIsJuvenile.Open();

            int thecount = qryIsJuvenile.iFieldByName("CNT");

            if (thecount > 0)
                return true;

            return false;
        }

        public void GetQualifiedItems(string ckey, string batchKey)
        {

            PLCQuery qryDelete = new PLCQuery();
            qryDelete.SQL = "DELETE FROM TV_ECNLIST where BATCH_NO = " + batchKey + " and CASE_KEY = " + ckey;
            qryDelete.ExecSQL("");


            string tempSQL = "SELECT * FROM TV_LABITEM, TV_ITEMTYPE where CASE_KEY = " + ckey;
            tempSQL += " and (TV_LABITEM.ITEM_TYPE = TV_ITEMTYPE.ITEM_TYPE) and ((TV_ITEMTYPE.NARCO_ITEM = 'T') or (TV_ITEMTYPE.NARCO_ITEM = 'J'))";

            PLCQuery qryECNList = new PLCQuery();
            qryECNList.SQL = "SELECT * FROM TV_ECNLIST where 0 = 1";
            qryECNList.Open();

            PLCQuery qryLabItem = new PLCQuery();
            qryLabItem.SQL = tempSQL;
            qryLabItem.Open();
            while (!qryLabItem.EOF())
            {

                string myEcn = qryLabItem.FieldByName("EVIDENCE_CONTROL_NUMBER");

                if (qryLabItem.FieldByName("NARCO_ITEM") == "T")
                {

                    if (NeedsReport(ckey, myEcn))
                    {
                        qryECNList.Open();
                        qryECNList.Append();
                        qryECNList.SetFieldValue("BATCH_NO", batchKey);
                        qryECNList.SetFieldValue("EVIDENCE_CONTROL_NUMBER", myEcn);
                        qryECNList.SetFieldValue("CASE_KEY", ckey);
                        qryECNList.Post("TV_ECNLIST", -1, -1);
                    }

                }

                if (qryLabItem.FieldByName("NARCO_ITEM") == "J")
                {
                    if ((NeedsReport(ckey, myEcn) && IsJuvenileItem(myEcn)))
                    {

                        qryECNList.Open();
                        qryECNList.Append();
                        qryECNList.SetFieldValue("BATCH_NO", batchKey);
                        qryECNList.SetFieldValue("EVIDENCE_CONTROL_NUMBER", myEcn);
                        qryECNList.SetFieldValue("CASE_KEY", ckey);
                        qryECNList.Post("TV_ECNLIST", -1, -1);

                    }

                }

                qryLabItem.Next();
            }


        }

        public void CheckNarcoSubmission(string ecn)
        {


            PLCQuery qryUpdate = new PLCQuery();
            qryUpdate.SQL = "SELECT * FROM TV_LABSUB where SUBMISSION_KEY = :SKEY";

            string ckey = "";

            string tempSQL = "SELECT * FROM TV_LABSUB, TV_LABITEM where EVIDENCE_CONTROL_NUMBER = " + ecn;
            tempSQL += " and (TV_LABSUB.CASE_KEY = TV_LABITEM.CASE_KEY) and (TV_LABSUB.SUBMISSION_NUMBER = TV_LABITEM.LAB_CASE_SUBMISSION)";



            PLCQuery qryItemType = new PLCQuery();


            PLCQuery qryLabItem = new PLCQuery();
            qryLabItem.SQL = tempSQL;
            qryLabItem.Open();
            while (!qryLabItem.EOF())
            {
                ckey = qryLabItem.FieldByName("CASE_KEY");

                string myEcn = qryLabItem.FieldByName("EVIDENCE_CONTROL_NUMBER");

                qryItemType.SQL = "SELECT NARCO_ITEM FROM TV_ITEMTYPE WHERE ITEM_TYPE = '" + qryLabItem.FieldByName("ITEM_TYPE") + "'";
                qryItemType.Open();

                if (qryItemType.FieldByName("NARCO_ITEM") == "T")
                {
                    if (NeedsReport(ckey, myEcn))
                    {
                        qryUpdate.SetParam("SKEY", qryLabItem.FieldByName("SUBMISSION_KEY"));
                        qryUpdate.Open();
                        qryUpdate.Edit();
                        qryUpdate.SetFieldValue("NARCOTICS_REVIEW", "P");
                        qryUpdate.Post("TV_LABSUB", 8, 1);
                        return;
                    }

                }

                if (qryItemType.FieldByName("NARCO_ITEM") == "J")
                {
                    if ((NeedsReport(ckey, myEcn) && IsJuvenileItem(myEcn)))
                    {

                        qryUpdate.SetParam("SKEY", qryLabItem.FieldByName("SUBMISSION_KEY"));
                        qryUpdate.Open();
                        qryUpdate.Edit();
                        qryUpdate.SetFieldValue("NARCOTICS_REVIEW", "P");
                        qryUpdate.Post("TV_LABSUB", 8, 1);
                        return;
                    }

                }


                qryLabItem.Next();
            }



        }

        // Return start page for the current app.
        public string GetAppStartPage()
        {
            string startpage;

            if (this.PLCActiveApp == "ManagementReports")
                startpage = "~/PLCWebCommon/CustomReports.aspx";
            else if (this.PLCActiveApp == "ChemInv")
                startpage = "~/ChemInv/ChemInv.aspx";
            else if (this.PLCActiveApp == "LockerTransfer")
                startpage = "~/LockerTransfer/LockerInit.aspx";
            else
                startpage = "~/Dashboard.aspx";

            return startpage;
        }

        private string GetLabCtrlSessionKey(string labctrlkey, string labcode, string dbname)
        {
            // Ex. "LC_ORACLE1_B_USES_THIRD_PARTY_BARCODE" where dbname = 'ORACLE1', labcode = 'B' and labctrlkey = 'USES_THIRD_PARTY_BARCODE'.
            return String.Format("LC_{0}_{1}_{2}", dbname, labcode, labctrlkey);
        }

        public string GetCustomLabCtrl(string labCtrlKey, string labCode)
        {
            return GetLabCtrl(labCtrlKey, labCode)
                .Trim()
                .ToUpper();
        }

        public string GetCaseLabCtrl(string labctrlkey)
        {
            PLCQuery qry = CacheHelper.OpenCachedSqlReadOnly("SELECT LAB_CODE FROM TV_LABCASE WHERE CASE_KEY = " + SafeInt(PLCGlobalCaseKey));
            if (qry.HasData())
            {
                string labCode = qry.FieldByName("LAB_CODE");
                if (!string.IsNullOrEmpty(labCode))
                {
                    return GetLabCtrl(labctrlkey, labCode);
                }
            }
            return "";
        }

        public string GetLabCtrl(string labctrlkey)
        {
            return GetLabCtrl(labctrlkey, this.PLCGlobalLabCode);
        }

        public string GetLabCtrlFlag(string labctrlkey)
        {
            string flag = GetLabCtrl(labctrlkey, this.PLCGlobalLabCode);
            return flag.Trim().ToUpper();
        }

        public int GetLabCtrlNum(string labctrlkey)
        {
            int labvalue = 0;
            int.TryParse(GetLabCtrl(labctrlkey, this.PLCGlobalLabCode), out labvalue);

            return labvalue;
        }

        private string GetLabCtrl(string labctrlkey, string labcode)
        {
            string retVal;

            if (String.IsNullOrWhiteSpace(labcode)) return "";

            // Initialize valid labctrl keys and labctrl defaults application vars if not yet initialized.
            InitAvailableLabCtrlKeys();
            InitLabCtrlDefaults();

            string labctrlSessionKey = GetLabCtrlSessionKey(labctrlkey, labcode, this.PLCDBName);
            if (TheApplication()[labctrlSessionKey] != null)
            {
                // Return labctrl value from application session.
                retVal = (string)TheApplication()[labctrlSessionKey];
            }
            else
            {
                // Read labctrl value from database.
                retVal = GetLabCtrlDb(labctrlkey, labcode);

                // If empty labctrl value, check if there's a default and return that instead.
                if (String.IsNullOrWhiteSpace(retVal))
                {
                    if (GetDictLabCtrlDefaults().ContainsKey(labctrlkey))
                        retVal = GetDictLabCtrlDefaults()[labctrlkey];
                }

                // Save to application session.
                TheApplication()[labctrlSessionKey] = retVal;
            }

            return retVal;
        }

        // Clear labctrl application session values.
        public void ClearLabCtrlSessionVars()
        {
            // labctrlkeys = list of labctrl keys.
            PLCSession.WriteDebug("1. ClearLabCtrlSessionVars start labctrlkeys.");
            string[] labctrlkeys = new string[GetDictLabCtrlKeys().Keys.Count];

            PLCSession.WriteDebug("2. ClearLabCtrlSessionVars start GetDictLabCtrlKeys.");
            GetDictLabCtrlKeys().Keys.CopyTo(labctrlkeys, 0);

            // Get list of existing labcodes.
            PLCSession.WriteDebug("3. ClearLabCtrlSessionVars start query tv_labctrl.");
            PLCQuery qry = new PLCQuery("select LAB_CODE from tv_labctrl");
            qry.Open();
            PLCSession.WriteDebug("4. ClearLabCtrlSessionVars start query tv_labctrl while.");
            while (!qry.EOF())
            {
                // Check for Application Session containing dbserver + labcode + labctrl key. Ex. "LC_B_USES_THIRD_PARTY_BARCODE" returned by GetLabCtrlSessionKey().
                string labcode = qry.FieldByName("LAB_CODE");
                PLCSession.WriteDebug("5. ClearLabCtrlSessionVars LAB_CODE: " + labcode);

                PLCSession.WriteDebug("6.1. ClearLabCtrlSessionVars start get dictDBServers");
                Dictionary<string, string> dictDBServers = ((Dictionary<string, string>)TheApplication()["PLCDBServerNameList"]);
                PLCSession.WriteDebug("6.2. ClearLabCtrlSessionVars end get dictDBServers");
                foreach (string dbserver in dictDBServers.Keys)
                {
                    PLCSession.WriteDebug("7.1. ClearLabCtrlSessionVars start labCode: " + labcode + " dbserver: " + dbserver);
                    foreach (string labctrlkey in labctrlkeys)
                    {
                        // Delete Application Session variable if present.
                        string labctrlSessionKey = GetLabCtrlSessionKey(labctrlkey, labcode, dbserver);
                        if (TheApplication()[labctrlSessionKey] != null)
                            TheApplication().Remove(labctrlSessionKey);
                    }
                    PLCSession.WriteDebug("7.2 ClearLabCtrlSessionVars end labCode: " + labcode + " dbserver: " + dbserver);
                }
                qry.Next();
            }
            PLCSession.WriteDebug("8. ClearLabCtrlSessionVars end function ClearLabCtrlSessionVars.");
        }

        private string GetLabCtrlDb(string labctrlkey, string labcode)
        {
            string retVal = "";
            string labctrlTable = GetLabCtrlTableOfKey(labctrlkey);
            if (labctrlTable != null)
            {
                PLCQuery qry = QueryLabCtrlValue(labctrlkey, labcode, labctrlTable);
                if ( qry.HasData() && (qry.FieldExist(labctrlkey)) )
                    retVal = qry.FieldByName(labctrlkey);
                else
                    retVal = "";
            }
            else
            {
                // labctrlkey does not belong to any of the tv_labctrl fields.
                retVal = "";
            }

            if (String.IsNullOrEmpty(retVal))
            {
                try
                {
                    retVal = System.Configuration.ConfigurationManager.AppSettings.Get("LABCTRL_" + labctrlkey);
                    if (String.IsNullOrWhiteSpace(retVal)) retVal = "";
                }
                catch (Exception e)
                {
                    PLCSession.WriteDebug("Exception in GetLabCtrlDb(" + labctrlkey + "):" + e.Message);
                    retVal = "";
                }
            }

            return retVal;
        }

        // Return query that returns labctrl value.
        private PLCQuery QueryLabCtrlValue(string labctrlkey, string labcode, string labctrlTable)
        {
            //see if this helps performance       
            String sql = "select * from " + labctrlTable + " where LAB_CODE = '" + labcode + "'";
            PLCQuery qry = CacheHelper.OpenCachedSql(sql);
            return qry;

            /*
                PLCQuery qry = new PLCQuery();
                qry.SQL = String.Format("select {0} from {1} where LAB_CODE = ?", labctrlkey, labctrlTable);
                qry.AddParameter("LAB_CODE", labcode);
                qry.OpenReadOnly();
                return qry;
            */

        }

        public void ClearUPSessionTraceAppCache()
        {
            List<string> upKeys = new List<string>();

            if (TheApplication() != null)
            {
                foreach (string key in TheApplication().AllKeys)
                {
                    if (key.ToUpper().Trim().StartsWith("UP_SESSIONTRACE_"))
                        upKeys.Add(key);
                }
            }

            foreach (string key in upKeys)
                TheApplication().Remove(key);
        }

        public string GetUPSessionTracAppCache(bool refreshValue=false)
        {
            if (string.IsNullOrEmpty(PLCSession.PLCGlobalAnalyst))
                return string.Empty;

            string userprefApplicationKey = GetUserPrefsApplicationKey();
            string sessiontraceValue = string.Empty;

            try
            {
                if (TheApplication()[userprefApplicationKey] != null && !refreshValue)
                {
                    sessiontraceValue = (string)TheApplication()[userprefApplicationKey];
                }
                else
                {
                    TheApplication()[userprefApplicationKey] = sessiontraceValue = GetUserPrefSessionTraceDB();
                }
            }
            catch (Exception ex)
            {
                return string.Empty;
            }

            return sessiontraceValue;
        }

        private string GetUserPrefSessionTraceDB()
        {
            //return "DISABLED";

            string value = "";
            PLCQuery qryUserPrefs = new PLCQuery();
            qryUserPrefs.SQL = "SELECT PREFERENCE_VALUE FROM TV_USERPREFS WHERE PREFERENCE_CODE = ? AND USER_ID = ?";
            qryUserPrefs.AddSQLParameter("PREFERENCE_CODE", "SESSIONTRACE");
            qryUserPrefs.AddSQLParameter("USER_ID", PLCGlobalAnalyst);
            qryUserPrefs.OpenReadOnly();

            if (!qryUserPrefs.IsEmpty())
                value = qryUserPrefs.FieldByName("PREFERENCE_VALUE");

            return value;
        }

        private string GetUserPrefsApplicationKey()
        {
            return String.Format("UP_SESSIONTRACE_{0}_{1}", PLCDBName, PLCGlobalAnalyst);
        }

        // Return corresponding labctrltable that the labctrl key belongs to.
        // Ex. GetLabCtrlTableOfKey("USES_THIRD_PARTY_BARCODE") returns "TV_LABCTRL3" as TV_LABCTRL3.USES_THIRD_PARTY_BARCODE is the valid table/key.
        public string GetLabCtrlTableOfKey(string labctrlkey)
        {
            if (GetDictLabCtrlKeys().ContainsKey(labctrlkey))
                return "TV_LABCTRL" + GetDictLabCtrlKeys()[labctrlkey];
            else
                return null;
        }

        private bool IsValidLabCtrlKey(string labctrlkey)
        {
            if (GetDictLabCtrlKeys().ContainsKey(labctrlkey))
                return true;
            else
                return false;
        }

        // Initialize dictionary of valid labctrl keys.
        // Note: Each database name has its separate valid lab keys application state dictionary because different labctrl columns are present in each database.
        private string GetValidLabCtrlKeysKey()
        {
            return "dictLabCtrlKeys" + this.PLCDBName;
        }

        Dictionary<string, string> GetDictLabCtrlKeys()
        {
            return (Dictionary<string, string>)TheApplication()[GetValidLabCtrlKeysKey()];
        }

#region DeptCtrl

        private string GetDeptCtrlSessionKey(string deptctrlkey, string departmentcode, string dbname)
        {
            // Ex. "LC_ORACLE1_B_USES_THIRD_PARTY_BARCODE" where dbname = 'ORACLE1', labcode = 'B' and labctrlkey = 'USES_THIRD_PARTY_BARCODE'.
            return String.Format("LC_{0}_{1}_{2}", dbname, departmentcode, deptctrlkey);
        }

        public string GetDeptCtrl(string deptCtrlKey)
        {
            string departmentCode = this.PLCGlobalAnalystDepartmentCode;
            PLCQuery qryDeptCtrl = new PLCQuery();
            qryDeptCtrl.SQL = "SELECT * FROM TV_DEPTCTRL WHERE DEPARTMENT_CODE = '" + departmentCode + "'";
            qryDeptCtrl.Open();

            if (qryDeptCtrl.HasData() && qryDeptCtrl.FieldExist(deptCtrlKey) )
                return qryDeptCtrl.FieldByName(deptCtrlKey);
            else
                return String.Empty;
        }

        public string GetDeptCtrlFlag(string deptCtrlKey)
        {
            string flag = GetDeptCtrl(deptCtrlKey);
            return flag.Trim().ToUpper();
        }

        public string GetCODCtrl(string codCtrlKey)
        {
            string labCode = this.PLCGlobalLabCode;
            PLCQuery qryCODCtrl = new PLCQuery();

            try
            {
                qryCODCtrl = CacheHelper.OpenCachedSqlReadOnly("SELECT " + codCtrlKey + " FROM TV_CODCTRL WHERE LAB_CODE = '" + labCode + "'");
                if (qryCODCtrl.HasData())
                    return qryCODCtrl.FieldByName(codCtrlKey);
                else
                    return String.Empty;
            }
            catch (Exception e)
            {
                PLCSession.WriteDebug("Error in GetCODCtrl. Message: " + e.Message);
                return string.Empty;
            }
        }

        public string GetCODCtrlFlag(string codCtrlKey)
        {
            string flag = GetCODCtrl(codCtrlKey);
            return flag.Trim().ToUpper();
        }

        // Clear labctrl application session values.
        public void ClearDeptCtrlSessionVars()
        {
            string[] deptctrlkeys = null;
            GetDeptCtrlKeys(out deptctrlkeys);

            PLCQuery qry = new PLCQuery("SELECT DEPARTMENT_CODE FROM TV_DEPTCTRL");
            qry.Open();
            while (!qry.EOF())
            {
                string departmentCode = qry.FieldByName("DEPARTMENT_CODE");
                Dictionary<string, string> dictDBServers = ((Dictionary<string, string>)TheApplication()["PLCDBServerNameList"]);

                if (dictDBServers != null)
                {
                    foreach (string dbserver in dictDBServers.Keys)
                    {
                        foreach (string deptctrlkey in deptctrlkeys)
                        {
                            string deptctrlSessionKey = GetDeptCtrlSessionKey(deptctrlkey, departmentCode, dbserver);
                            if (TheApplication()[deptctrlSessionKey] != null)
                                TheApplication().Remove(deptctrlSessionKey);
                        }
                    }
                }

                qry.Next();
            }
        }

        private void GetDeptCtrlKeys(out string[] deptCtrlList)
        {
            PLCQuery qryDeptCtrlKeys = new PLCQuery();
            List<string> deptCtrlKeys = new List<string>();

            if (PLCSession.PLCDatabaseServer == "ORACLE")
                qryDeptCtrlKeys.SQL = "SELECT COLUMN_NAME FROM USER_TAB_COLUMNS WHERE TABLE_NAME = 'TV_DEPTCTRL'";
            else
                qryDeptCtrlKeys.SQL = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TV_DEPTCTRL'";

            qryDeptCtrlKeys.Open();
            while (!qryDeptCtrlKeys.EOF())
            {
                deptCtrlKeys.Add(qryDeptCtrlKeys.FieldByName("COLUMN_NAME"));
                qryDeptCtrlKeys.Next();
            }

            deptCtrlList = deptCtrlKeys.ToArray();
        }

#endregion

#region Lab Control XML

        public string GetLabCtrlXmlNode(string labctrlkey, string xmlNodeName)
        {
            string xmlNodeValue = string.Empty;
            Dictionary<string, string> dictLabCtrlXml = new Dictionary<string, string>();
            string labCode = this.PLCGlobalLabCode;
            string labCtrlXmlSessionKey = GetLabCtrlCtrlXmlSessionKey(labCode, labctrlkey, this.PLCDBName);

            if (TheApplication()[labCtrlXmlSessionKey] != null)
            {
                dictLabCtrlXml = (Dictionary<string, string>)TheApplication()[labCtrlXmlSessionKey];
                if (dictLabCtrlXml.ContainsKey(xmlNodeName))
                    xmlNodeValue = dictLabCtrlXml[xmlNodeName];
            }
            else
            {
                xmlNodeValue = GetLabCtrlXmlDb(labCode, labctrlkey, xmlNodeName, ref dictLabCtrlXml);
                TheApplication()[labCtrlXmlSessionKey] = dictLabCtrlXml;
            }

            return xmlNodeValue;
        }

        private string GetLabCtrlXmlDb(string labCode, string labctrlkey, string xmlNodeName, ref Dictionary<string, string> dictLabCtrlXml)
        {
            string labctrlTable = GetLabCtrlTableOfKey(labctrlkey);
            string sql = string.Format("SELECT {0} FROM {2}  WHERE LAB_CODE = '{3}'", labctrlkey, xmlNodeName, labctrlTable, labCode);
            string val = string.Empty;

            PLCQuery qryLabCtrl = new PLCQuery();
            qryLabCtrl.SQL = sql;
            qryLabCtrl.Open();

            if (qryLabCtrl.IsEmpty())
                dictLabCtrlXml = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(qryLabCtrl.FieldByName(labctrlkey).Trim()))
                dictLabCtrlXml = new Dictionary<string, string>();

            try
            {
                XmlDocument header = new XmlDocument();
                string xmlString = qryLabCtrl.FieldByName(labctrlkey).Trim();

                header.XmlResolver = null; // Disable DTD
                xmlString = xmlString.Replace("json:Array=\"true\"", "");
                header.LoadXml(xmlString);

                XmlNode xmlNode = header.DocumentElement;
                foreach (XmlNode node in xmlNode.ChildNodes)
                {
                    string nodeName = node.Name;
                    string nodeValue = node.InnerText;
                    string encryption = GetNodeAttribute(node, "encryption");

                    if (encryption.Equals("V2") && !string.IsNullOrEmpty(nodeValue))
                        nodeValue = TryDecrypt(labctrlkey, nodeName, nodeValue);
                    if (encryption.Equals("SP") && !string.IsNullOrEmpty(nodeValue))
                        nodeValue = GetStoredProcDecryptValue(nodeValue);

                    if (!dictLabCtrlXml.ContainsKey(nodeName))
                        dictLabCtrlXml.Add(nodeName, nodeValue);
                }

                if (dictLabCtrlXml.ContainsKey(xmlNodeName))
                    val = dictLabCtrlXml[xmlNodeName];
            }
            catch (Exception ex)
            {
                PLCSession.WriteDebug("GetLabCtrlXmlDb error getting " + labctrlkey + ". Message: " + ex.Message, true);
            }

            return val;
        }

        private string GetLabCtrlCtrlXmlSessionKey(string labCode, string labctrlkey, string dbname)
        {
            return String.Format("LCXML_{0}_{1}_{2}", dbname, labCode, labctrlkey);
        }

        private string GetNodeAttribute(XmlNode node, string attribute)
        {
            try
            {
                return node.Attributes[attribute].Value;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string TryDecrypt(string fieldName, string nodeName, string val)
        {
            try
            {
                return PLCSession.DecryptConfig(val, fieldName, nodeName);
            }
            catch
            {
                return val;
            }
        }

        public string GetSAKCtrlXmlNode(string id, string sakCtrlKey, string xmlNodeName)
        {
            string xmlNodeValue = string.Empty;
            Dictionary<string, string> dictSakCtrlXml = new Dictionary<string, string>();
            string sakCtrlXmlSessionKey = GetSAKCtrlCtrlXmlSessionKey(id, sakCtrlKey, this.PLCDBName);

            if (TheApplication()[sakCtrlXmlSessionKey] != null)
            {
                dictSakCtrlXml = (Dictionary<string, string>)TheApplication()[sakCtrlXmlSessionKey];
                if (dictSakCtrlXml.ContainsKey(xmlNodeName))
                    xmlNodeValue = dictSakCtrlXml[xmlNodeName];
            }
            else
            {
                xmlNodeValue = GetSakCtrlXmlDb(id, sakCtrlKey, xmlNodeName, ref dictSakCtrlXml);
                TheApplication()[sakCtrlXmlSessionKey] = dictSakCtrlXml;
            }

            return xmlNodeValue;
        }

        private string GetSAKCtrlCtrlXmlSessionKey(string id, string sakctrlkey, string dbname)
        {
            return String.Format("SAKXML_{0}_{1}_{2}", id, dbname, sakctrlkey);
        }

        private string GetSakCtrlXmlDb(string id, string sakctrlkey, string xmlNodeName, ref Dictionary<string, string> dictSakCtrlXml)
        {
            string sql = string.Format("SELECT {0} FROM TV_SAKCTRL WHERE ID = '{1}'", sakctrlkey, id);
            string val = string.Empty;

            PLCQuery qrySakCtrl = new PLCQuery();
            qrySakCtrl.SQL = sql;
            qrySakCtrl.Open();

            if (qrySakCtrl.IsEmpty())
                dictSakCtrlXml = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(qrySakCtrl.FieldByName(sakctrlkey).Trim()))
                dictSakCtrlXml = new Dictionary<string, string>();

            try
            {
                XmlDocument header = new XmlDocument();
                string xmlString = qrySakCtrl.FieldByName(sakctrlkey).Trim();

                header.XmlResolver = null; // Disable DTD
                xmlString = xmlString.Replace("json:Array=\"true\"", "");
                header.LoadXml(xmlString);

                XmlNode xmlNode = header.DocumentElement;
                foreach (XmlNode node in xmlNode.ChildNodes)
                {
                    string nodeName = node.Name;
                    string nodeValue = node.InnerText;
                    string encryption = GetNodeAttribute(node, "encryption");

                    if (encryption.Equals("V2") && !string.IsNullOrEmpty(nodeValue))
                        nodeValue = TryDecrypt(sakctrlkey, nodeName, nodeValue);

                    if (!dictSakCtrlXml.ContainsKey(nodeName))
                        dictSakCtrlXml.Add(nodeName, nodeValue);
                }

                if (dictSakCtrlXml.ContainsKey(xmlNodeName))
                    val = dictSakCtrlXml[xmlNodeName];
            }
            catch (Exception ex)
            {
                PLCSession.WriteDebug("GetSakCtrlXmlDb error getting " + sakctrlkey + ". Message: " + ex.Message, true);
            }

            return val;
        }

        public void ClearLabCtrlXmlNode()
        {
            List<string> upKeys = new List<string>();

            if (TheApplication() != null)
            {
                foreach (string key in TheApplication().AllKeys)
                {
                    if (key.ToUpper().Trim().StartsWith("LCXML_" + this.PLCDBName + "_" + this.PLCGlobalLabCode))
                        upKeys.Add(key);
                }
            }

            foreach (string key in upKeys)
                TheApplication().Remove(key);
        }

        #endregion

        #region Dept Control XML
        public string GetDeptCtrlXmlNode(string deptCtrlkey, string xmlNodeName, string defaultValue = "")
        {
            string xmlNodeValue = string.Empty;
            Dictionary<string, string> dictDeptCtrlXml = new Dictionary<string, string>();
            string deptCode = this.PLCGlobalAnalystDepartmentCode;
            string deptCtrlXmlSessionKey = GetDeptCtrlXmlSessionKey(deptCode, deptCtrlkey, this.PLCDBName);

            if (TheApplication()[deptCtrlXmlSessionKey] != null)
            {
                dictDeptCtrlXml = (Dictionary<string, string>)TheApplication()[deptCtrlXmlSessionKey];
                if (dictDeptCtrlXml.ContainsKey(xmlNodeName))
                    xmlNodeValue = dictDeptCtrlXml[xmlNodeName];
            }
            else
            {
                xmlNodeValue = GetDeptCtrlXmlDb(deptCode, deptCtrlkey, xmlNodeName, ref dictDeptCtrlXml);
                TheApplication()[deptCtrlXmlSessionKey] = dictDeptCtrlXml;
            }

            if (string.IsNullOrEmpty(xmlNodeValue))
            {
                xmlNodeValue = defaultValue;
            }

            return xmlNodeValue;
        }

        private string GetDeptCtrlXmlDb(string deptCode, string deptCtrlKey, string xmlNodeName, ref Dictionary<string, string> dictDeptCtrlXml)
        {
            string sql = string.Format("SELECT {0} FROM TV_DEPTCTRL WHERE DEPARTMENT_CODE = '{1}'", deptCtrlKey, deptCode);
            string val = string.Empty;

            PLCQuery qryDeptCtrl = new PLCQuery();
            qryDeptCtrl.SQL = sql;
            qryDeptCtrl.Open();

            if (qryDeptCtrl.IsEmpty())
                dictDeptCtrlXml = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(qryDeptCtrl.FieldByName(deptCtrlKey).Trim()))
                dictDeptCtrlXml = new Dictionary<string, string>();

            try
            {
                XmlDocument header = new XmlDocument();
                string xmlString = qryDeptCtrl.FieldByName(deptCtrlKey).Trim();

                header.XmlResolver = null; // Disable DTD
                xmlString = xmlString.Replace("json:Array=\"true\"", "");
                header.LoadXml(xmlString);

                XmlNode xmlNode = header.DocumentElement;
                foreach (XmlNode node in xmlNode.ChildNodes)
                {
                    string nodeName = node.Name;
                    string nodeValue = node.InnerText;
                    string encryption = GetNodeAttribute(node, "encryption");

                    if (encryption.Equals("V2") && !string.IsNullOrEmpty(nodeValue))
                        nodeValue = TryDecrypt(deptCtrlKey, nodeName, nodeValue);
                    if (encryption.Equals("SP") && !string.IsNullOrEmpty(nodeValue))
                        nodeValue = GetStoredProcDecryptValue(nodeValue);

                    if (!dictDeptCtrlXml.ContainsKey(nodeName))
                        dictDeptCtrlXml.Add(nodeName, nodeValue);
                }

                if (dictDeptCtrlXml.ContainsKey(xmlNodeName))
                    val = dictDeptCtrlXml[xmlNodeName];
            }
            catch (Exception ex)
            {
                PLCSession.WriteDebug("GetDeptCtrlXmlDb error getting " + deptCtrlKey + ". Message: " + ex.Message, true);
            }

            return val;
        }

        private string GetDeptCtrlXmlSessionKey(string deptCode, string deptCtrlKey, string dbName)
        {
            return String.Format("DCXML_{0}_{1}_{2}", dbName, deptCode, deptCtrlKey);
        }
#endregion

        //*AAC 09242010 - Spell Check Dictionary
        public object WordDictionary
        {
            get { return TheApplication()["SpellCheckDictionary"]; }
            set { TheApplication()["SpellCheckDictionary"] = value; }
        }


        private void InitAvailableLabCtrlKeys()
        {
            InitAvailableLabCtrlKeys(false);
        }

        // Reinitialize list of available labctrl keys that can be requested.
        public void ReInitAvailableLabCtrlKeys()
        {
            InitAvailableLabCtrlKeys(true);
        }

        // Initialize the list of labctrlkeys that can be requested. These are the existing columns in each of the tv_labctrl tablese.
        // Set bForceInit to true to always initialize.
        // The returned table is a dictionary keyed by the labctrl flag with a value of the corresponding labctrl table number.
        private void InitAvailableLabCtrlKeys(bool bForceInit)
        {
            if (bForceInit || GetDictLabCtrlKeys() == null)
            {
                Dictionary<string, string> dictLabCtrlKeys = new Dictionary<string, string>();

                // Add all columns from all labctrl tables to valid labctrl keys.
                foreach (string labctrltable in new string[] { "tv_labctrl", "tv_labctrl2", "tv_labctrl3", "tv_labctrl4", "tv_labctrl5" })
                {
                    // Store labctrl table by their number. Ex. tv_labctrl2 will be "2", tv_labctrl3 will be "3", tv_labctrl will be "". 
                    string labctrltableNumber = labctrltable.Replace("tv_labctrl", "");

                    PLCQuery qry = new PLCQuery();
                    qry.SQL = String.Format("select * from {0} where 0=1", labctrltable);
                    qry.Open();


                    foreach (DataColumn col in qry.PLCDataTable.Columns)
                    {
                        // Ex. for TV_LABCTRL3.USES_THIRD_PARTY_BARCODE, dictionary item will be dictLabCtrlKeys["USES_THIRD_PARTY_BARCODE"] = "3" ('3' refers to tv_labctrl3 table)
                        if (!dictLabCtrlKeys.ContainsKey(col.ColumnName))
                            dictLabCtrlKeys.Add(col.ColumnName, labctrltableNumber);
                    }
                }

                TheApplication()[GetValidLabCtrlKeysKey()] = dictLabCtrlKeys;
            }
        }

        // Initialize dictionary of labctrl defaults.
        Dictionary<string, string> GetDictLabCtrlDefaults()
        {
            return (Dictionary<string, string>)TheApplication()["dictLabCtrlDefaults"];
        }

        private void InitLabCtrlDefaults()
        {
            if (GetDictLabCtrlDefaults() == null)
            {
                Dictionary<string, string> dictLabCtrlDefaults = new Dictionary<string, string>();

                // Put default settings here. Defaults migrated from PLCLabCtrl.cs LoadLabCtrl().
                dictLabCtrlDefaults.Add("CASE_OFFICER_TEXT", "Case Officer");
                dictLabCtrlDefaults.Add("SUBMITTED_BY_TEXT", "Submitted By");
                dictLabCtrlDefaults.Add("DEPARTMENT_TEXT", "Department");
                dictLabCtrlDefaults.Add("SECTION_TEXT", "Section");
                dictLabCtrlDefaults.Add("NAME_STATUS_TEXT", "Status");
                dictLabCtrlDefaults.Add("OLDEST_JUVENILE", "18");
                dictLabCtrlDefaults.Add("JURISDICTION_TEXT", "County");
                dictLabCtrlDefaults.Add("REFERENCE_TEXT", "Reference");
                dictLabCtrlDefaults.Add("CASE_TAB_TEXT", "Case Info");
                dictLabCtrlDefaults.Add("OFFENSE_LOCATION_TEXT", "Location");
                dictLabCtrlDefaults.Add("DEPT_CASE_TEXT", "Department Case");
                dictLabCtrlDefaults.Add("TRACKING_NUMBER_TEXT", "Tracking Number");
                dictLabCtrlDefaults.Add("SUBMISSION_TYPE_TEXT", "Submission Type");
                dictLabCtrlDefaults.Add("NAME_REF_TEXT", "Name ID");
                dictLabCtrlDefaults.Add("OFFENSE_DATE_TEXT", "Offense Date");
                dictLabCtrlDefaults.Add("OFFENSE_CODE_TEXT", "Offense Type");
                dictLabCtrlDefaults.Add("NOTES_BUTTON_TEXT", "Notes");
                dictLabCtrlDefaults.Add("REPORT_BUTTON_TEXT", "Report");
                dictLabCtrlDefaults.Add("REVIEW_BUTTON_TEXT", "Review");
                dictLabCtrlDefaults.Add("APPROVE_BUTTON_TEXT", "Approve");
                dictLabCtrlDefaults.Add("ADMIN_BUTTON_TEXT", "Admin");
                dictLabCtrlDefaults.Add("LAB_CASE_TEXT", "Lab Case #");
                dictLabCtrlDefaults.Add("RECEIVE_DISK_TEXT", "Receive Disk");
                dictLabCtrlDefaults.Add("PROCESS_FIELD_TEXT", "Process");
                dictLabCtrlDefaults.Add("DRAFT_DATE_TEXT", "Draft Date");
                dictLabCtrlDefaults.Add("DRAFT_PRINTED_STATUS", "2");
                dictLabCtrlDefaults.Add("OPEN_SUPPLEMENT_TEXT", "Edit Supplement");
                dictLabCtrlDefaults.Add("QC_ORIGINAL_RECIPIENT", "T");
                dictLabCtrlDefaults.Add("SIGN_BUTTON_TEXT", "Sign");
                dictLabCtrlDefaults.Add("COMBINED_REPORT_TEXT", "Combined Report");
                dictLabCtrlDefaults.Add("NEED_TO_MAIL_TEXT", "Need to Mail");
                dictLabCtrlDefaults.Add("TURN_AROUND_CALC", "N");
                dictLabCtrlDefaults.Add("TURN_AROUND_END_DATE", "C");
                dictLabCtrlDefaults.Add("INVENTORY_MODE", "S");
                dictLabCtrlDefaults.Add("ALPHA_PAD_CHAR", "{");
                dictLabCtrlDefaults.Add("NUMERIC_PAD_CHAR", "#");
                dictLabCtrlDefaults.Add("BIRTHPLACE_TEXT", "Birthplace");
                dictLabCtrlDefaults.Add("KIT_CHAR", ".");
                dictLabCtrlDefaults.Add("INITIAL_TASK_STATUS", "O");
                dictLabCtrlDefaults.Add("EMAIL_FROM_NAME", "LIMS_SYSTEM");
                dictLabCtrlDefaults.Add("EMAIL_FROM_ADDRESS", "LIMS_SYSTEM");
                dictLabCtrlDefaults.Add("SAMPLE_CHAR", ".");
                dictLabCtrlDefaults.Add("ADDRESS_TEXT", "Address");
                dictLabCtrlDefaults.Add("STORY_TEXT", "Story");
                dictLabCtrlDefaults.Add("CASE_MANAGER_TEXT", "Case Manager");
                dictLabCtrlDefaults.Add("CASE_ANALYST_TEXT", "Case Analyst");
                dictLabCtrlDefaults.Add("CASE_ANALYST_SECTION", "EXAM");
                dictLabCtrlDefaults.Add("CASE_MANAGER_SECTION", "MNGR");
                dictLabCtrlDefaults.Add("SUBMISSION_TAB_TEXT", "Submissions");
                dictLabCtrlDefaults.Add("DEFAULT_DIST_RECIPIENT", "T");
                dictLabCtrlDefaults.Add("SUBM_EXTRA_INFO_CAPTION", "Crime Scene");
                dictLabCtrlDefaults.Add("DISCOVERY_DATE_TEXT", "Discovery Date");
                dictLabCtrlDefaults.Add("PANEL_TASK_STATUS", "P");
                dictLabCtrlDefaults.Add("OFFENSE_CODE_2_TEXT", "Offense Type 2");
                dictLabCtrlDefaults.Add("OFFENSE_CODE_3_TEXT", "Offense Type 3");
                dictLabCtrlDefaults.Add("FREEFORM_CASE_MASK", "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");

                TheApplication()["dictLabCtrlDefaults"] = dictLabCtrlDefaults;
            }
        }

        private string GetGlobalINIValue(string fieldName)
        {
            string globalINIValue = "";
            PLCQuery qryGlobalINI = new PLCQuery("SELECT " + fieldName + " FROM TV_GLOBALINI");

            if (qryGlobalINI.Open() && qryGlobalINI.HasData())
            {
                globalINIValue = qryGlobalINI.FieldByName(fieldName);
            }

            return globalINIValue;
        }

#region WebConfig

        public string GetWebConfiguration(string code)
        {
            //If item is in cache, return cached item.
            //If not, query its value from the database and add item to cache.
            //If code is not found in the database, return null.
            if (!CacheHelper.IsInCache(code))
            {
                //Check for override in web.config app setting
                String overrideCode = "";
                String overrideVal = "";
                try {
                    overrideCode = "OVERRIDE_" + code;
                    overrideVal = System.Configuration.ConfigurationManager.AppSettings.Get(overrideCode);
                    if (!String.IsNullOrWhiteSpace(overrideVal))
                    {
                        PLCSession.WriteDebug("GetWebConfiguration(" + code + ") overridden to:" + overrideVal);
                        if (overrideVal == "*NULL") overrideVal = "";
                        CacheHelper.AddItem(code, overrideVal);
                        return overrideVal;
                    }
                }
                catch (Exception e)
                {
                    PLCSession.WriteDebug("Exception in GetWebConfiguration(" + code + "):" + e.Message);
                }



                PLCQuery qryWebConfig = new PLCQuery();
                qryWebConfig.SQL = "SELECT ID, VALUE_RES, ENCRYPTED, STORED_PROC_ENCRYPT FROM TV_WEBCONFIG WHERE CODE = '" + code + "'";
                qryWebConfig.OpenReadOnly();

                if (!qryWebConfig.IsEmpty())
                {
                    string val = qryWebConfig.FieldByName("VALUE_RES");
                    if (qryWebConfig.FieldByName("ENCRYPTED") == "T")
                        val = SymmetricalDecrypt(val, qryWebConfig.FieldByName("ID"), code);
                    if (qryWebConfig.FieldByName("STORED_PROC_ENCRYPT") == "T")
                        val = GetStoredProcDecryptValue(val);

                    CacheHelper.AddItem(code, val);
                }
                else
                    CacheHelper.AddItem(code, "");
            }

            return Convert.ToString(CacheHelper.GetItem(code));
        }

        public void SetWebConfiguration(string code, string value)
        {
            //If item is cached, update its value.
            //If not, there's no need to cache the item at this point.
            //It will be cached whenever it's needed (when GetWebConfiguration is called).
            if (CacheHelper.IsInCache(code))
                CacheHelper.AddItem(code, value);
        }

#endregion

#region DNAConfig

        public string GetDNAConfig(string flag)
        {
            string flagName = flag.ToUpper();
            string cacheKey = "DNACONFIG" + flagName;

            if (!CacheHelper.IsInCache(cacheKey))
            {
                var qry = new PLCQuery();
                qry.SQL = "SELECT VALUE FROM TV_DNACONFIG WHERE UPPER(FLAG_NAME) = ?";
                qry.AddSQLParameter("FLAG_NAME", flagName);
                qry.OpenReadOnly();

                CacheHelper.AddItem(cacheKey, qry.HasData()
                    ? qry.FieldByName("VALUE")
                    : string.Empty);
            }

            return Convert.ToString(CacheHelper.GetItem(cacheKey));
        }

#endregion DNAConfig

        public string GetCache(string key, string sqlIfCacheNotFound)
        {
            if (!CacheHelper.IsInCache(key))
            {
                if (string.IsNullOrEmpty(sqlIfCacheNotFound))
                    return null;

                PLCQuery qry = new PLCQuery(sqlIfCacheNotFound);
                qry.Open();
                if (!qry.IsEmpty())
                    CacheHelper.AddItem(key, qry.PLCDataTable.Rows[0][0]);
            }

            return Convert.ToString(CacheHelper.GetItem(key));
        }

        public void GetCodeHeadFieldNames(string tableName, out string codeField, out string descriptionField, out string activeField)
        {
            codeField = "CODE";
            descriptionField = "DESCRIPTION";
            activeField = "ACTIVE";

            try
            {
                if ((tableName.Substring(0, 3) != "TV_") && (tableName.Substring(0, 3) != "CV_") && (tableName.Substring(0, 3) != "UV_"))
                    tableName = "TV_" + tableName;

                if (tableName == "TV_ANALYST")
                {
                    codeField = "ANALYST";
                    descriptionField = "NAME";
                    activeField = "ACTIVE";
                }
                else if (tableName == "TV_OFFENCAT")
                {
                    codeField = "CODE";
                    descriptionField = "DESCRIPTION";
                    activeField = "ACTIVE";
                }
                else if (tableName == "TV_CASETYPE")
                {
                    codeField = "CASE_TYPE";
                    descriptionField = "DESCRIPTION";
                    activeField = "ACTIVE";
                }
                else if (tableName == "TV_DEPTNAME")
                {
                    codeField = "DEPARTMENT_CODE";
                    descriptionField = "DEPARTMENT_NAME";
                    activeField = "ACTIVE";
                }
                else if (tableName == "TV_OFFENSE")
                {
                    codeField = "OFFENSE_CODE";
                    descriptionField = "OFFENSE_DESCRIPTION";
                    activeField = "ACTIVE";
                }
                else if (tableName == "TV_SUBTYPE")
                {
                    codeField = "TYPE_RES";
                    descriptionField = "DESCRIPTION";
                    activeField = "ACTIVE";
                }
                else if (tableName == "CV_ITEMCAT")
                {
                    codeField = "CAT_CODE";
                    descriptionField = "CAT_CODE_DESCRIPTION";
                    activeField = "ACTIVE";
                }
                else if (tableName == "CV_PACKTYPE")
                {
                    codeField = "PACKAGING_CODE";
                    descriptionField = "DESCRIPTION";
                    activeField = "ACTIVE";
                }
                else if (tableName == "CV_ITEMTYPE")
                {
                    codeField = "ITEM_TYPE";
                    descriptionField = "DESCRIPTION";
                    activeField = "ACTIVE";
                }
                else if (tableName == "TV_CUSTCODE")
                {
                    codeField = "CUSTODY_TYPE";
                    descriptionField = "DESCRIPTION";
                    activeField = "ACTIVE";
                }
                else if (tableName == "CV_CUSTLOC" || tableName == "TV_CUSTLOC")
                {
                    codeField = "LOCATION";
                    descriptionField = "DESCRIPTION";
                    activeField = "ACTIVE";
                }
                else if (tableName == "TV_LABCTRL")
                {
                    codeField = "LAB_CODE";
                    descriptionField = "LAB_NAME";
                    activeField = "ACTIVE";
                }
                else if (tableName == "TV_EXAMCODE")
                {
                    codeField = "EXAM_CODE";
                    descriptionField = "DESCRIPTION";
                    activeField = "ACTIVE";
                }
                else if (tableName == "TV_PRIORITY")
                {
                    codeField = "PRIORITY";
                    descriptionField = "DESCRIPTION";
                    activeField = "ACTIVE";
                }
                else if (tableName == "TV_EXAMSTAT")
                {
                    codeField = "EXAM_STATUS";
                    descriptionField = "DESCRIPTION";
                    activeField = "ACTIVE";
                }
                else if (tableName == "TV_RACECODE")
                {
                    codeField = "RACE";
                    descriptionField = "DESCRIPTION";
                    activeField = "ACTIVE";
                }
                else if (tableName == "TV_SEXCODE")
                {
                    codeField = "SEX";
                    descriptionField = "DESCRIPTION";
                    activeField = "ACTIVE";
                }
                else if (tableName == "TV_NAMETYPE")
                {
                    codeField = "NAME_TYPE";
                    descriptionField = "DESCRIPTION";
                    activeField = "ACTIVE";
                }
                else if (tableName == "TV_DEPTPERS")
                {
                    codeField = "DEPTPERS_KEY";
                    descriptionField = "NAME";
                    activeField = "ACTIVE";
                }
                else if (tableName == "CV_DEPTPERS")
                {
                    codeField = "DEPTPERS_KEY";
                    descriptionField = "DESCRIPTION";
                    activeField = "ACTIVE";
                }
                else if (tableName == "TV_CONTAINER")
                {
                    codeField = "CONTAINER_KEY";
                    descriptionField = "CONTAINER_DESCRIPTION";
                    activeField = "";
                }
                else if (tableName == "TV_STATES")
                {
                    codeField = "STATE_CODE";
                    descriptionField = "DESCRIPTION";
                    activeField = "ACTIVE";
                }
                else if (tableName == "TV_COWEBUSE")
                {
                    codeField = "USER_ID";
                    descriptionField = "NAME";
                    activeField = "ACTIVE";
                }
                else if (tableName == "TV_LABCASE")
                {
                    codeField = "CASE_KEY";
                    descriptionField = "LAB_CASE";
                    activeField = "";
                }
                else if (tableName == "TV_LABITEM")
                {
                    codeField = "EVIDENCE_CONTROL_NUMBER";
                    descriptionField = "LAB_ITEM_NUMBER";
                    activeField = "";
                }
                else if (tableName == "TV_LABOFFENSE")
                {
                    codeField = "SUPPKEY";
                    descriptionField = "OFFENSE_DESCRIPTION";
                    activeField = "";
                }
                else if (tableName == "CV_LABOFFENSE")
                {
                    codeField = "SUPPKEY";
                    descriptionField = "DESCRIPTION";
                    activeField = "";
                }
                else
                {
                    PLCQuery qryLookup = CacheHelper.OpenCachedSqlFieldNames("SELECT * FROM " + tableName + " WHERE 0 = 1");
                    codeField = qryLookup.PLCDataTable.Columns[0].ColumnName;
                    descriptionField = qryLookup.PLCDataTable.Columns[1].ColumnName;
                    activeField = qryLookup.PLCDataTable.Columns.Contains("ACTIVE") ? "ACTIVE" : "";
                }
            }
            catch (Exception e)
            {
                WriteDebug("Exception in GetCodeHeadFieldNames: " + e.Message, true);
            }
        }

        // Return the cache duration timespan for 'metadata' tables such as TV_DBPANEL, TV_CODEHEAD, DBGRID, etc.
        // This is the cache duration for how long entries in metadata stay valid without needing to be refetched.
        public TimeSpan GetMetadataCacheDuration()
        {
            double cacheMins;
            if (!Double.TryParse(GetWebConfiguration("DBCACHE_EXP"), out cacheMins))
                cacheMins = 0;      // Not a valid cache expiration so default to 0 mins.

            return TimeSpan.FromMinutes(cacheMins);
        }

#region Report Service

        /// <summary>
        /// Check if RPTSVCURL is configured
        /// </summary>
        /// <param name="rptServiceUrl"></param>
        /// <returns></returns>
        private bool HasReportServiceUrl(out string rptServiceUrl)
        {
            rptServiceUrl = "";

            try
            {
                rptServiceUrl = System.Configuration.ConfigurationManager.AppSettings["RPTSVCURL"];
                if (string.IsNullOrWhiteSpace(rptServiceUrl))
                    rptServiceUrl = GetWebConfiguration("RPTSVCURL");

                WriteDebug("RPTSVCURL:" + rptServiceUrl, true);

            }
            catch (Exception ex)
            {
                WriteDebug("Exception in GetWebConfiguration:" + ex.Message, true);
                rptServiceUrl = "";
            }

            rptServiceUrl = rptServiceUrl.Trim();

            bool usesReportService = rptServiceUrl != "*LOCAL" && !string.IsNullOrEmpty(rptServiceUrl);

            if (usesReportService && !rptServiceUrl.EndsWith("/"))
                rptServiceUrl += "/";

            return usesReportService;
        }

        /// <summary>
        /// Send http post request to the report service to check if the crystal report exist
        /// </summary>
        /// <param name="url">Report Service URL</param>
        /// <param name="data"></param>
        /// <param name="responseData"></param>
        /// <param name="withCredentials"></param>
        /// <returns></returns>
        private bool RSFindCrystalReport(string url, Dictionary<string, string> data, out string responseData, bool withCredentials = false)
        {
            string methodName = "RSFindCrystalReport";
            try
            {
                var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url + "/" + methodName);

                if (withCredentials)
                    request.UseDefaultCredentials = withCredentials;

                request.ContentType = "application/x-www-form-urlencoded";
                request.Accept = "text/xml";
                request.Method = "POST";

                string body = string.Empty;
                foreach (var param in data)
                    body += param.Key + "=" + param.Value + "&";
                body.TrimEnd('&');

                using (var requestStream = new StreamWriter(request.GetRequestStream()))
                {
                    requestStream.Write(body);
                    requestStream.Flush();
                    requestStream.Close();
                }

                using (var response = (System.Net.HttpWebResponse)request.GetResponse())
                {
                    string status = response.StatusDescription;

                    using (var responseStream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(responseStream);
                        string result = reader.ReadToEnd();
                        reader.Close();

                        var xmlResponse = new XmlDocument();
                        xmlResponse.LoadXml(result);
                        responseData = xmlResponse.LastChild.InnerText;

                        PLCSession.WriteDebug(methodName + " - Response: " + result, true);

                        responseStream.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                responseData = "Error sending web request. " + ex.Message;
                PLCSession.WriteDebug(methodName + " - Error: " + responseData, true);

                return false;
            }

            return true;

        }

        /// <summary>
        /// Get the report path from the report service 
        /// </summary>
        /// <param name="reportName"></param>
        /// <param name="reportPath"></param>
        /// <param name="isCustom"></param>
        /// <param name="isChemInv"></param>
        /// <returns></returns>
        private bool GetReportPathFromService(string reportName, out string reportPath, bool isCustom = false, bool isChemInv = false)
        {
            reportPath = string.Empty;

            string rptServiceUrl;
            if (HasReportServiceUrl(out rptServiceUrl))
            {
                string webmethodsUrl = rptServiceUrl + "PLCWebCommon/PLCWebMethods.asmx";
                var data = new Dictionary<string, string>()
                {
                    { "cInfo", PLCDBName },
                    { "reportName", reportName },
                    { "isCustom", isCustom.ToString().ToLower() },
                    { "isChemInv", isChemInv.ToString().ToLower() }
                };

                if (!RSFindCrystalReport(webmethodsUrl, data, out reportPath, !string.IsNullOrEmpty(PLCDomainName)))
                    reportPath = string.Empty;

                return true;
            }

            return false;
        }

#endregion Report Service

        public string FindCustomCrystalReport(string RPTNAME, bool checkReportService = true)
        {
            if (checkReportService)
            {
                string reportPath;
                if (GetReportPathFromService(RPTNAME, out reportPath, isCustom: true))
                    return reportPath;
            }

            string DatabaseServer = PLCDatabaseServer;
            string rptfile = System.AppDomain.CurrentDomain.BaseDirectory + "reports\\custom\\" + RPTNAME;


            WriteDebug("FindCrystalReport (3) Searching for: " + rptfile, true);

            if (File.Exists(rptfile))
            {
                WriteDebug("FindCrystalReport (3) FOUND: " + rptfile, true);
                return rptfile;
            }

            rptfile = System.AppDomain.CurrentDomain.BaseDirectory + "reports\\custom\\" + RPTNAME + ".RPT";
            if (File.Exists(rptfile))
            {
                WriteDebug("FindCrystalReport (4) FOUND: " + rptfile, true);
                return rptfile;
            }

            rptfile = System.AppDomain.CurrentDomain.BaseDirectory + "reports\\custom\\" + RPTNAME + ".PLC";
            if (File.Exists(rptfile))
            {
                WriteDebug("FindCrystalReport (3) FOUND: " + rptfile, true);
                return rptfile;
            }

            return "";

        }

        public string FindCrystalReport(string RPTNAME, bool IsChemInv = false, bool checkReportService = true)
        {
            if (checkReportService)
            {
                string reportPath;
                if (GetReportPathFromService(RPTNAME, out reportPath, isChemInv: IsChemInv))
                    return reportPath;
            }

            var reportFileLocator = new ReportFileLocator();
            reportFileLocator.IsChemInvPage = IsChemInv;
            reportFileLocator.FindReportFile(RPTNAME);

            if (reportFileLocator.IsCurrentReportFound)
                return reportFileLocator.CurrentValidFilePath;

            return string.Empty;
        }

        /// <summary>
        /// Checks if report exists in Custom Path using extensions .rpt and .plc
        /// else, checks if report exists in DBServer Path using extensions .rpt and .plc
        /// </summary>
        /// <remarks>
        /// Added additional validation since ocx pages send only reportname
        /// </remarks>
        /// <example>
        /// CheckCustomReport("~//Reports/MSSQL/report.rpt");
        /// CheckCustomReport("~//ChemInv/Reports/MSSQL/report.rpt");
        /// CheckCustomReport("report");
        /// </example>
        /// <param name="reportVirtualPath">Report Relative Path</param>
        /// <returns>Report Physical Path</returns>
        public string CheckCustomReport(string reportVirtualPath)
        {
            string[] defaultPath = reportVirtualPath.Replace("~\\", System.AppDomain.CurrentDomain.BaseDirectory)
               .Replace("~/", System.AppDomain.CurrentDomain.BaseDirectory)
               .Split(new String[] { "/", "\\" }, StringSplitOptions.RemoveEmptyEntries);
            defaultPath[defaultPath.Length - 1] = Path.GetFileNameWithoutExtension(defaultPath[defaultPath.Length - 1]);

            if (defaultPath.Length == 1)
            {
                string rptpath = FindCustomCrystalReport(defaultPath[0]);
                if (rptpath == "")
                    rptpath = FindCrystalReport(defaultPath[0]);
                return rptpath;
            }

            //find in Custom folder
            defaultPath[defaultPath.Length - 2] = "Custom";
            string reportPath = String.Join("\\", defaultPath);
            if (File.Exists(reportPath + ".rpt"))
            {
                return reportPath + ".rpt";
            }
            else if (File.Exists(reportPath + ".plc"))
            {
                return reportPath + ".plc";
            }

            //find in default folder
            defaultPath[defaultPath.Length - 2] = PLCDatabaseServer;
            reportPath = String.Join("\\", defaultPath);
            if (File.Exists(reportPath + ".rpt"))
            {
                return reportPath + ".rpt";
            }
            else if (File.Exists(reportPath + ".plc"))
            {
                return reportPath + ".plc";
            }

            return "";
        }

        public T GetProperty<T>(string key, object value)
        {
            Dictionary<string, object> PropertyCollection = GetPropertyCollection<T>(key);
            object testObject = PropertyCollection[key];

            // check whether testObject is null and set default value
            if (testObject == null)
            {
                testObject = value;
                PropertyCollection[key] = value;
            }

            return (T)testObject;
        }

        public void SetProperty<T>(string key, object value)
        {
            Dictionary<string, object> PropertyCollection = GetPropertyCollection<T>(key);
            PropertyCollection[key] = value;
        }

        private Dictionary<string, object> GetPropertyCollection<T>(string key)
        {
            Dictionary<string, object> PropertyCollection = new Dictionary<string, object>();
            if (TheSession()["BEASTPROP"] == null)
            {
                TheSession()["BEASTPROP"] = PropertyCollection;
            }
            else
            {
                PropertyCollection = (Dictionary<string, object>)TheSession()["BEASTPROP"];
            }

            if (!PropertyCollection.ContainsKey(key))
            {
                PropertyCollection.Add(key, default(T));
            }

            return PropertyCollection;
        }

        public void WriteAuditLog(string caseKey, string evidenceControlNumber, string examKey, string logCode, string logSubcode, string errorCode, string userId, string programName, string logInfo, int changeKey)
        {

            PLCSession.WriteDebug("auditlog-internal 1", true);

            int tryInt;
            PLCQuery qryLog = new PLCQuery();

            bool isPrelog = !string.IsNullOrEmpty(PLCGlobalPrelogUser);

            //$$ Use direct sql insert to add a new auditlog record to optimize a new audit log record write.
            /*
                        qryLog.SQL = "SELECT * FROM TV_AUDITLOG WHERE 0 = 1";
                        qryLog.Open();
                        qryLog.Append();
                        qryLog.AddParameter("LOG_STAMP", PLCSession.GetNextSequence("AUDITLOG_SEQ"));
                        qryLog.AddParameter("TIME_STAMP", System.DateTime.Now);
                        qryLog.AddParameter("USER_ID", userId);
                        qryLog.AddParameter("PROGRAM", programName.Length < 9 ? programName : programName.Substring(0, 8));
                        qryLog.AddParameter("CODE", logCode);
                        qryLog.AddParameter("SUB_CODE", logSubcode);
                        qryLog.AddParameter("ERROR_LEVEL", errorCode);
                        qryLog.AddParameter("OS_USER_NAME", PLCSession.GetOSUserName());
                        qryLog.AddParameter("OS_COMPUTER_NAME", PLCSession.GetOSComputerName());
                        qryLog.AddParameter("OS_ADDRESS", PLCSession.GetOSAddress());
                        if (!string.IsNullOrEmpty(caseKey))
                            qryLog.AddParameter("CASE_KEY", caseKey);
                        if (!string.IsNullOrEmpty(evidenceControlNumber))
                            qryLog.AddParameter("EVIDENCE_CONTROL_NUMBER", evidenceControlNumber);
                        qryLog.AddParameter("ADDITIONAL_INFORMATION", logInfo);
                        qryLog.Save("TV_AUDITLOG", -1, -1);
            */

            PLCSession.WriteDebug("auditlog-internal 2", true);

            if (isPrelog)
            {
                caseKey = string.IsNullOrEmpty(caseKey) ? PLCGlobalPrelogCaseKey : caseKey;
                bool isCodna = IsCodnaMode();

                qryLog.SQL = String.Format("INSERT INTO TV_AUDITWEB " +
                   "(LOG_STAMP, TIME_STAMP, USER_ID, PROGRAM, CODE, SUB_CODE, ERROR_LEVEL, OS_USER_NAME, OS_COMPUTER_NAME, " +
                   "OS_ADDRESS, BUILD_NUMBER, ADDITIONAL_INFORMATION{0}{1}{2}{3}{4}{5}) " +
                   "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?{6}{7}{8}{9}{10}{11})",
                   !string.IsNullOrEmpty(caseKey) && Int32.TryParse(caseKey, out tryInt) ? ", CASE_KEY" : "",
                   !string.IsNullOrEmpty(evidenceControlNumber) ? ", EVIDENCE_CONTROL_NUMBER" : "",
                   !string.IsNullOrEmpty(PLCSession.PLCGlobalPrelogDepartmentCode) ? ", DEPARTMENT_CODE" : "",
                   !string.IsNullOrEmpty(PLCSession.PLCGlobalPrelogDepartmentCaseNumber) ? ", DEPARTMENT_CASE_NUMBER" : "",
                   !string.IsNullOrEmpty(PLCSession.PLCGlobalPrelogSubmissionNumber) ? ", SUBMISSION_NUMBER" : "",
                   (!string.IsNullOrEmpty(PLCSession.PLCCODNAPrelogSequence) && isCodna) ? ", PRELOG_SEQUENCE" : "",
                   !string.IsNullOrEmpty(caseKey) && Int32.TryParse(caseKey, out tryInt) ? ", ?" : "",
                   !string.IsNullOrEmpty(evidenceControlNumber) ? ", ?" : "",
                   !string.IsNullOrEmpty(PLCSession.PLCGlobalPrelogDepartmentCode) ? ", ?" : "",
                   !string.IsNullOrEmpty(PLCSession.PLCGlobalPrelogDepartmentCaseNumber) ? ", ?" : "",
                   !string.IsNullOrEmpty(PLCSession.PLCGlobalPrelogSubmissionNumber) ? ", ?" : "",
                   (!string.IsNullOrEmpty(PLCSession.PLCCODNAPrelogSequence) && isCodna) ? ", ?" : "");

                PLCSession.WriteDebug("auditlog-internal 3", true);

                qryLog.AddSQLParameter("@log_stamp", PLCSession.GetNextSequence("AUDITWEB_SEQ"));
                qryLog.AddSQLParameter("@time_stamp", System.DateTime.Now);
                qryLog.AddSQLParameter("@user_id", PLCGlobalPrelogUser);
                qryLog.AddSQLParameter("@program", IsCODNAPrelog() ? "WEBCODNA" : "WEBPRELO");
                qryLog.AddSQLParameter("@code", logCode);
                qryLog.AddSQLParameter("@sub_code", logSubcode);
                qryLog.AddSQLParameter("@error_level", errorCode);
                qryLog.AddSQLParameter("@os_user_name", PLCSession.GetOSUserName());
                qryLog.AddSQLParameter("@os_computer_name", PLCSession.GetOSComputerName());
                qryLog.AddSQLParameter("@os_address", PLCSession.GetOSAddress());
                qryLog.AddSQLParameter("@build_number", PLCSession.PLCBEASTiLIMSVersion);
                qryLog.AddSQLParameter("@additional_information", logInfo + "\r\n" + PrelogUserName(PLCGlobalPrelogUser));

                if (!string.IsNullOrEmpty(caseKey) && Int32.TryParse(caseKey, out tryInt))
                    qryLog.AddSQLParameter("@case_key", caseKey);
                if (!string.IsNullOrEmpty(evidenceControlNumber))
                    qryLog.AddSQLParameter("@evidence_control_number", evidenceControlNumber);
                if (!string.IsNullOrEmpty(PLCSession.PLCGlobalPrelogDepartmentCode))
                    qryLog.AddSQLParameter("@department_code", PLCSession.PLCGlobalPrelogDepartmentCode);
                if (!string.IsNullOrEmpty(PLCSession.PLCGlobalPrelogDepartmentCaseNumber))
                    qryLog.AddSQLParameter("@department_case_number", PLCSession.PLCGlobalPrelogDepartmentCaseNumber);
                if (!string.IsNullOrEmpty(PLCSession.PLCGlobalPrelogSubmissionNumber))
                    qryLog.AddSQLParameter("@submission_number", PLCSession.PLCGlobalPrelogSubmissionNumber);
                if (!string.IsNullOrEmpty(PLCSession.PLCCODNAPrelogSequence) && isCodna)
                    qryLog.AddSQLParameter("@prelog_sequence", PLCSession.PLCCODNAPrelogSequence);

                qryLog.WriteToAuditLog = false;

                PLCSession.WriteDebug("auditlog-internal 4", true);

                qryLog.ExecInsertSQL("TV_AUDITWEB");

                PLCSession.WriteDebug("auditlog-internal 5", true);

            }
            else
            {

                qryLog.SQL = String.Format("INSERT INTO TV_AUDITLOG " +
                    "(LOG_STAMP, TIME_STAMP, USER_ID, PROGRAM, CODE, SUB_CODE, ERROR_LEVEL, OS_USER_NAME, OS_COMPUTER_NAME, " +
                    "OS_ADDRESS, ADDITIONAL_INFORMATION, BUILD_NUMBER{0}{1}{2}{3}{4}{5}) " +
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?{6}{7}{8}{9}{10}{11})",
                    !string.IsNullOrEmpty(caseKey) && Int32.TryParse(caseKey, out tryInt) ? ", CASE_KEY" : "",
                    !string.IsNullOrEmpty(evidenceControlNumber) ? ", EVIDENCE_CONTROL_NUMBER" : "",
                    !string.IsNullOrEmpty(examKey) ? ", EXAM_KEY" : "",
                    changeKey > 0 ? ", REASON_CHANGE_KEY" : "",
                    !string.IsNullOrEmpty(PLCSession.PLCGlobalLabCase) ? ", LAB_CASE" : "",
                    !string.IsNullOrEmpty(PLCSession.PLCGlobalDepartmentCaseNumber) ? ", DEPARTMENT_CASE_NUMBER" : "",
                    !string.IsNullOrEmpty(caseKey) && Int32.TryParse(caseKey, out tryInt) ? ", ?" : "",
                    !string.IsNullOrEmpty(evidenceControlNumber) ? ", ?" : "",
                    !string.IsNullOrEmpty(examKey) ? ", ?" : "",
                    changeKey > 0 ? ", ?" : "",
                    !string.IsNullOrEmpty(PLCSession.PLCGlobalLabCase) ? ", ?" : "",
                    !string.IsNullOrEmpty(PLCSession.PLCGlobalDepartmentCaseNumber) ? ", ?" : "");

                PLCSession.WriteDebug("auditlog-internal 3", true);

                qryLog.AddSQLParameter("@log_stamp", PLCSession.GetNextSequence("AUDITLOG_SEQ"));
                qryLog.AddSQLParameter("@time_stamp", System.DateTime.Now);
                qryLog.AddSQLParameter("@user_id", userId);
                qryLog.AddSQLParameter("@program", programName.Length < 9 ? programName : programName.Substring(0, 8));
                qryLog.AddSQLParameter("@code", logCode);
                qryLog.AddSQLParameter("@sub_code", logSubcode);
                qryLog.AddSQLParameter("@error_level", errorCode);
                qryLog.AddSQLParameter("@os_user_name", PLCSession.GetOSUserName());
                qryLog.AddSQLParameter("@os_computer_name", PLCSession.GetOSComputerName());
                qryLog.AddSQLParameter("@os_address", PLCSession.GetOSAddress());
                qryLog.AddSQLParameter("@additional_information", logInfo + "\r\n" + PLCGlobalAnalystName);
                qryLog.AddSQLParameter("@build_number", PLCSession.PLCBEASTiLIMSVersion);

                if (!string.IsNullOrEmpty(caseKey) && Int32.TryParse(caseKey, out tryInt))
                    qryLog.AddSQLParameter("@case_key", caseKey);
                if (!string.IsNullOrEmpty(evidenceControlNumber))
                    qryLog.AddSQLParameter("@evidence_control_number", evidenceControlNumber);
                if (!string.IsNullOrEmpty(examKey))
                    qryLog.AddSQLParameter("@exam_key", examKey);
                if (changeKey > 0)
                    qryLog.AddSQLParameter("@reason_change_key", changeKey);
                if (!string.IsNullOrEmpty(PLCSession.PLCGlobalLabCase))
                    qryLog.AddSQLParameter("@lab_case", PLCSession.PLCGlobalLabCase);
                if (!string.IsNullOrEmpty(PLCSession.PLCGlobalDepartmentCaseNumber))
                    qryLog.AddSQLParameter("@department_case_number", PLCSession.PLCGlobalDepartmentCaseNumber);

                qryLog.WriteToAuditLog = false;

                PLCSession.WriteDebug("auditlog-internal 4", true);

                qryLog.ExecInsertSQL("TV_AUDITLOG");

                PLCSession.WriteDebug("auditlog-internal 5", true);
            }

        }

        public void WriteAuditLog(string logCode, string logSubcode, string errorCode, string logInfo, int changeKey)
        {
            WriteAuditLog(PLCGlobalCaseKey, PLCGlobalECN, PLCGlobalAssignmentKey, logCode, logSubcode, errorCode, PLCGlobalAnalyst, "iLIMS" + PLCBEASTiLIMSVersion, logInfo, changeKey);
        }

        public void WriteAuditCon(string tableName, string fileSourceKey1, string fileSourceKey2, string logCode, string logSubcode, string errorCode, string userId, string programName, string logInfo, int changeKey)
        {
            PLCQuery qryLog = new PLCQuery();
            qryLog.SQL = string.Format("INSERT INTO TV_AUDITCON " +
                "(LOG_STAMP, TIME_STAMP, ENTRY_ANALYST, TABLE_NAME, FILE_SOURCE_KEY1, FILE_SOURCE_KEY2, CODE, SUB_CODE, OS_COMPUTER_NAME, OS_USER_NAME, OS_ADDRESS, ADDITIONAL_INFORMATION) " +
                "VALUES(?,?,?,?,?,?,?,?,?,?,?,?)");

            qryLog.AddSQLParameter("@log_stamp", PLCSession.GetNextSequence("AUDITCON_SEQ"));
            qryLog.AddSQLParameter("@time_stamp", System.DateTime.Now);
            qryLog.AddSQLParameter("@entry_analyst", PLCGlobalAnalyst);
            qryLog.AddSQLParameter("@table_name", tableName);
            qryLog.AddSQLParameter("@file_source_key1", fileSourceKey1);
            qryLog.AddSQLParameter("@file_source_key2", fileSourceKey2);
            qryLog.AddSQLParameter("@code", logCode);
            qryLog.AddSQLParameter("@sub_code", logSubcode);
            qryLog.AddSQLParameter("@os_user_name", PLCSession.GetOSUserName());
            qryLog.AddSQLParameter("@os_computer_name", PLCSession.GetOSComputerName());
            qryLog.AddSQLParameter("@os_address", PLCSession.GetOSAddress());
            qryLog.AddSQLParameter("@additional_information", logInfo + "\r\n" + PLCGlobalAnalystName);
            qryLog.ExecInsertSQL("TV_AUDITCON");
        }

        public void WriteAuditLam(string chemControlNumber, string tableName, string logCode, string logSubcode, string errorCode, string userId, string programName, string logInfo, int changeKey)
        {
            PLCSessionVars sv = new PLCSessionVars();
            if (string.IsNullOrEmpty(chemControlNumber))
                chemControlNumber = Convert.ToString(sv.GetChemInvProperty<int>("CHEM_CONTROL_NUM", 0));

            if (sv.IsChemInvMaintenance())
                chemControlNumber = "0";

            PLCQuery qryLog = new PLCQuery();
            qryLog.SQL = string.Format("INSERT INTO TV_CHEMAUDT " +
                "(CHEM_AUDIT_KEY, ENTRY_ANALYST, ENTRY_TIME_STAMP, TABLE_NAME, CHEM_CONTROL_NUMBER, CODE, SUB_CODE, OS_COMPUTER_NAME, OS_USER_NAME, OS_ADDRESS, ADDITIONAL_INFORMATION, PROGRAM) " +
                "VALUES(?,?,?,?,?,?,?,?,?,?,?,?)");

            qryLog.AddSQLParameter("@chem_audit_key", PLCSession.GetNextSequence("CHEMAUDT_SEQ"));
            qryLog.AddSQLParameter("@entry_analyst", PLCGlobalAnalyst);
            qryLog.AddSQLParameter("@entry_time_stamp", System.DateTime.Now);
            qryLog.AddSQLParameter("@table_name", tableName);
            qryLog.AddSQLParameter("@chem_control_number", chemControlNumber);
            qryLog.AddSQLParameter("@code", logCode);
            qryLog.AddSQLParameter("@sub_code", logSubcode);
            qryLog.AddSQLParameter("@os_user_name", PLCSession.GetOSUserName());
            qryLog.AddSQLParameter("@os_computer_name", PLCSession.GetOSComputerName());
            qryLog.AddSQLParameter("@os_address", PLCSession.GetOSAddress());
            qryLog.AddSQLParameter("@additional_information", logInfo + "\r\n" + PLCGlobalAnalystName);
            qryLog.AddSQLParameter("@program", programName);
            qryLog.ExecInsertSQL("TV_CHEMAUDT");
        }

        public void WriteAuditCon(string tableName, string logCode, string logSubcode, string errorCode, string logInfo, int changeKey)
        {
            WriteAuditCon(tableName, PLCGlobalConfigSourceKey1, PLCGlobalConfigSourceKey2, logCode, logSubcode, errorCode, PLCGlobalAnalyst, "iLIMS" + PLCBEASTiLIMSVersion, logInfo, changeKey);
        }

        public void WriteAuditLam(string chemControlNumber, string tableName, string logCode, string logSubcode, string errorCode, string logInfo, int changeKey)
        {
            WriteAuditLam(chemControlNumber, tableName, logCode, logSubcode, errorCode, PLCGlobalAnalyst, "CHEMINV", logInfo, changeKey);
        }

        public void WriteAuditCon(string tableName, string fileSourceKey1, string fileSourceKey2, string logCode, string logSubcode, string logInfo)
        {
            WriteAuditCon(tableName, fileSourceKey1, fileSourceKey2, logCode, logSubcode, "-1", PLCGlobalAnalyst, "iLIMS" + PLCBEASTiLIMSVersion, logInfo, 0);
        }

        public void ClearAllCache()
        {
            // Clear .NET cache.
            PLCSession.WriteDebug("1. ClearAllCache: Clear .NET cache.");
            CacheHelper.ClearAllCache();

            // Reinitialize list of available labctrl keys (for when a new labctrl column is added to any of the labctrl tables.)
            PLCSession.WriteDebug("2. ClearAllCache: ReInitAvailableLabCtrlKeys");
            PLCSession.ReInitAvailableLabCtrlKeys();

            PLCSession.WriteDebug("3 ClearAllCache start check PLCDBServerNameList");
            Dictionary<string, string> dictDBServers = ((Dictionary<string, string>)TheApplication()["PLCDBServerNameList"]);
            PLCSession.WriteDebug("4. ClearAllCache end check PLCDBServerNameList");
            if (dictDBServers == null)
            {
                PLCSession.WriteDebug("4.1. ClearAllCache dictDBServers is null. Start calling AddToAvailableDBNames.");
                AddToAvailableDBNames(PLCSession.PLCDBName);
                PLCSession.WriteDebug("4.2. ClearAllCache End calling AddToAvailableDBNames.");
            }

            // Clear labctrl application session vars.
            PLCSession.WriteDebug("5. ClearAllCache: ClearLabCtrlSessionVars");
            PLCSession.ClearLabCtrlSessionVars();

            // Clear deptctrl application session vars.
            PLCSession.WriteDebug("6. ClearAllCache: ClearDeptCtrlSessionVars");
            PLCSession.ClearDeptCtrlSessionVars();

            // Clear Session trace
            PLCSession.WriteDebug("7. ClearAllCache: ClearUPSessionTraceAppCache");
            PLCSession.ClearUPSessionTraceAppCache();

            PLCSession.WriteDebug("8. ClearAllCache: ClearLabCtrlXmlNode");
            PLCSession.ClearLabCtrlXmlNode();
        }

        public string GenerateCodeHeadSQL(string tableName, string selectedValue, string filter, string descFormat, string descSeparator, string parentFlexBoxValue, bool showActiveOnly, Dictionary<string, object> parentControlValues = null)
        {
            return GenerateCodeHeadSQL(tableName, selectedValue, filter, descFormat, descSeparator, parentFlexBoxValue, showActiveOnly, "", parentControlValues);
        }

        public string GenerateCodeHeadSQL(string tableName, string selectedValue, string filter, string descFormat, string descSeparator, string parentFlexBoxValue, bool showActiveOnly, string sortOrder, Dictionary<string, object> parentControlValues = null)
        {
            string sql;
            string codeField;
            string descriptionField;
            string activeField;

            if (filter.StartsWith("plc-x-"))
            {
                filter = filter.Substring(6);
                filter = AESEncryption.Decrypt(filter);
            }


            if (!String.IsNullOrWhiteSpace(parentFlexBoxValue)) parentFlexBoxValue = parentFlexBoxValue.Replace("'", "''");

            bool hasPCV = parentControlValues != null
                && parentControlValues.Count > 0;
            if (hasPCV)
            {
                foreach (var pc in parentControlValues.ToList())
                {
                    parentControlValues[pc.Key] = pc.Value.ToString().Replace("'", "''");
                }
            }

            GetCodeHeadFieldNames(tableName, out codeField, out descriptionField, out activeField);

            string sqlActiveField = activeField;
            if (string.IsNullOrEmpty(sqlActiveField))
                sqlActiveField = "''";

            if (sqlActiveField == "''")
            {
                tableName = "(SELECT T.*, '' AS ACTIVE FROM " + tableName + " T) " + tableName;
                sqlActiveField = "ACTIVE";
            }

            if (descFormat == "C")
                sql = "SELECT " + codeField + " AS CODE, RTRIM(LTRIM(" + codeField + ")) AS DESCRIPTION, " + sqlActiveField + " AS ACTIVE FROM " + tableName;
            else if (descFormat == "B")
                sql = PLCSession.FormatSpecialFunctions("SELECT " + codeField + " AS CODE, RTRIM(LTRIM(" + codeField + ")) || ' " + descSeparator +
                        " ' || RTRIM(LTRIM(" + descriptionField + ")) AS DESCRIPTION, " + sqlActiveField + " AS ACTIVE FROM " + tableName);
            else if (descFormat == "D")
                sql = "SELECT DISTINCT " + codeField + " AS CODE, RTRIM(LTRIM(" + descriptionField + ")) AS DESCRIPTION, " + sqlActiveField + " AS ACTIVE FROM " + tableName;
            else
                sql = "SELECT " + codeField + " AS CODE, RTRIM(LTRIM(" + descriptionField + ")) AS DESCRIPTION, " + sqlActiveField + " AS ACTIVE FROM " + tableName;

            if (!string.IsNullOrEmpty(filter))
            {
                filter = filter.Replace("\"", "'");

                filter = ReplaceSpecialKeysInCodeCondition(filter);

                //LIMITATION: one parent flexbox only
                if (filter.Contains("{") && filter.Contains("}"))
                {
                    string parentValue = parentFlexBoxValue;

                    while ((filter.Contains("{") && filter.Contains("}")))
                    {
                        int startIndex = filter.IndexOf("{");
                        int endIndex = filter.IndexOf("}");

                        if (hasPCV) // if multiple parents
                        {
                            string pc = filter.Substring(startIndex + 1, endIndex - startIndex - 1);
                            parentValue = parentControlValues.ContainsKey(pc)
                                ? parentControlValues[pc].ToString()
                                : parentFlexBoxValue;
                        }

                        filter = filter.Substring(0, startIndex) + "'" + parentValue + "'" +
                                 filter.Substring(endIndex + 1);
                    }

                    if (hasPCV) WriteDebug("@pcv-filter: " + filter, true);
                }
                //*AAC 11232010 - similar to filter with "{ }" but without the addition of single quotes. See LIMITATION.
                else if (filter.Contains("[") && filter.Contains("]"))
                {
                    int startIndex = filter.IndexOf("[");
                    int endIndex = filter.IndexOf("]");

                    filter = filter.Substring(0, startIndex) + parentFlexBoxValue + filter.Substring(endIndex + 1);
                }
            }

            var additionalFilters = new List<string>();

            if (filter != string.Empty)
                additionalFilters.Add(filter);

            if (showActiveOnly && (tableName == "TV_ANALYST" || tableName == "CV_ANALYST"))
                additionalFilters.Add("(ACCOUNT_DISABLED <> 'T' OR ACCOUNT_DISABLED IS NULL)");
            else if (tableName == "TV_CONTAINER")
                additionalFilters.Add("(DELETED <> 'T' OR DELETED IS NULL)");

            if (!string.IsNullOrEmpty(selectedValue))
            {
                // This value won't go through ValidateSQL so we will escape single quotes here
                selectedValue = selectedValue.Replace("'", "''");
                if (PLCDatabaseServer == "ORACLE") selectedValue = selectedValue.Replace("+", "' + chr(43) + '");
                    additionalFilters.Add(string.Format("{0} IN ('{1}')", codeField, selectedValue));
            }

            if (showActiveOnly && !string.IsNullOrEmpty(activeField))
                additionalFilters.Add(string.Format("(ACTIVE <> 'F' OR ACTIVE IS NULL)"));

            additionalFilters = additionalFilters.Select(s => string.Format("({0})", s)).ToList();

            if (additionalFilters.Count > 0)
                sql += " WHERE " + string.Join(" AND ", additionalFilters);

            if (sortOrder != "")
            {
                if (sortOrder == "U" && USER_RESExists(tableName))
                    sql += " ORDER BY USER_RES";
                else
                {
                    if (PLCSession.PLCDatabaseServer == "ORACLE")
                        sql += " ORDER BY UPPER(DESCRIPTION)";
                    else
                        sql += " ORDER BY DESCRIPTION";
                }
            }
            else if (tableName.Substring(0, 3) != "CV_")
            {
                if (PLCSession.PLCDatabaseServer == "ORACLE")
                    sql += " ORDER BY UPPER(DESCRIPTION)";
                else
                    sql += " ORDER BY DESCRIPTION";
            }

            return sql;
        }

        private bool USER_RESExists(string tableName)
        {
            int count = 0;
            PLCQuery qryCheckUserRes = new PLCQuery();
            if (PLCSession.PLCDatabaseServer == "ORACLE")
                qryCheckUserRes.SQL = "SELECT COUNT(COLUMN_NAME) AS CTR FROM USER_TAB_COLUMNS WHERE TABLE_NAME = '" + tableName + "' AND COLUMN_NAME = 'USER_RES'";
            else
                qryCheckUserRes.SQL = "SELECT COUNT(COLUMN_NAME) AS CTR FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tableName + "' AND COLUMN_NAME = 'USER_RES'";

            qryCheckUserRes.Open();
            while (!qryCheckUserRes.EOF())
            {
                count = Convert.ToInt32(qryCheckUserRes.FieldByName("CTR"));
                break;
            }

            if (count > 0)
                return true;
            else
                return false;
        }

        public string ReplaceSpecialKeysInCodeCondition(string codeCondition)
        {
            //Special codes for commonly used session keys
            //This is used in DBPANEL codeconditions when required parent flexbox does not exist, but its session variable is available (eg. exam key).

            //We replace the following in the filter string with its corresponding session value:
            // {CASE_KEY} => PLCSession.PLCGlobalCaseKey
            // {EXAM_KEY} => PLCSession.PLCGlobalAssignmentKey
            // {ECN} => PLCSession.PLCGlobalECN
            // {NAME_KEY} => PLCSession.PLCGlobalNameKey
            // {STATUS_KEY} => PLCSession.PLCGlobalStatusKey
            // {GLOBAL_ANALYST} => PLCSession.PLCGlobalAnalyst

            codeCondition = codeCondition.Replace("{CASE_KEY}", PLCSession.PLCGlobalCaseKey).Replace("{case_key}", PLCSession.PLCGlobalCaseKey);
            codeCondition = codeCondition.Replace("{EXAM_KEY}", PLCSession.PLCGlobalAssignmentKey).Replace("{exam_key}", PLCSession.PLCGlobalAssignmentKey);
            codeCondition = codeCondition.Replace("{ECN}", PLCSession.PLCGlobalECN).Replace("{ecn}", PLCSession.PLCGlobalECN);
            codeCondition = codeCondition.Replace("{NAME_KEY}", PLCSession.PLCGlobalNameKey).Replace("{name_key}", PLCSession.PLCGlobalNameKey);
            codeCondition = codeCondition.Replace("{STATUS_KEY}", PLCSession.PLCGlobalStatusKey).Replace("{status_key}", PLCSession.PLCGlobalStatusKey);
            codeCondition = codeCondition.Replace("{GLOBAL_ANALYST}", PLCSession.PLCGlobalAnalyst).Replace("{global_analyst}", PLCSession.PLCGlobalAnalyst);
            codeCondition = codeCondition.Replace("{PRELOGDEPTCODE}", PLCSession.PLCGlobalPrelogDepartmentCode).Replace("{prelogdeptcode}", PLCSession.PLCGlobalPrelogDepartmentCode);
            codeCondition = codeCondition.Replace("{PRELOGDEPTCASENUMBER}", PLCSession.PLCGlobalPrelogDepartmentCaseNumber).Replace("{prelogdeptcasenumber}", PLCSession.PLCGlobalPrelogDepartmentCaseNumber);
            codeCondition = codeCondition.Replace("{GLOBAL_LAB_CODE}", PLCSession.PLCGlobalLabCode).Replace("{global_lab_code}", PLCSession.PLCGlobalLabCode);
            codeCondition = codeCondition.Replace("{LAB_CODE}", PLCSession.PLCGlobalLabCode).Replace("{lab_code}", PLCSession.PLCGlobalLabCode);

            return codeCondition;
        }

        // Return whether the user has access to the currently loaded page.
        public bool UserCanAccessPage(HttpRequest req)
        {
            if (req.Url.Segments.Length == 0)
                return true;

            string pageFilename = req.Url.Segments[req.Url.Segments.Length - 1];
            return UserCanAccessPage(pageFilename);
        }

        // Return whether the user has access to the page filename.
        public bool UserCanAccessPage(string pageFilename)
        {
            // Remove any leading forward slashes and use lowercase.
            // "/Dashboard.aspx" becomes "dashboard.aspx"
            pageFilename = pageFilename.Replace("/", "").ToLower();

            // If page has no access requirement, then user always has access.
            if (!PLCSessionVars.pageAccessMap.ContainsKey(pageFilename))
                return true;

            // Get access requirement.
            string accessRequirement = PLCSessionVars.pageAccessMap[pageFilename.ToLower()].Trim();
            if (accessRequirement == "")
                return true;

            // Check each requirement one by one that user has access to it.
            string[] reqs = accessRequirement.Split(',');
            foreach (string req in reqs)
            {
                if (HasAccessToRequirementItem(req.Trim()))
                    return true;
            }

            return false;
        }

        // Return whether user has access to the specified requirement item.
        // The requirement item can be either a user option or labctrl.
        // Ex. HasAccessToRequirementItem("CONFIG") checks whether user opt CONFIG is set.
        //     HasAccessToRequirementItem("labctrl.USES_BULK_INTAKE") checks whether 
        //        labctrl USES_BULK_INTAKE is set.
        //     HasAccessToRequirementItem("!CONFIG")  not (!) operator can also be used
        //        to check whether the user option or labctrl is false.
        private bool HasAccessToRequirementItem(string req)
        {
            if (req == "")
                return true;

            // Check whether (!) NOT operator is specified.
            bool applyNotOperator = false;
            if (req.Substring(0, 1) == "!")
            {
                applyNotOperator = true;

                // Remove the '!' part from the flag.
                req = req.Substring(1);
            }

            bool retHasAccess;

            if (req.StartsWith("labctrl."))
            {
                // Test labctrl flag.
                string labctrlflag = req.Split('.')[1];
                if (IsLabCtrlSet(labctrlflag))
                    retHasAccess = true;
                else
                    retHasAccess = false;
            }
            else
            {
                // Test user option.
                retHasAccess = PLCSession.CheckUserOption(req);
            }

            return ModifyWithNotOperator(retHasAccess, applyNotOperator);
        }

        // Return the result modified with an optional not operator.
        private bool ModifyWithNotOperator(bool baseResult, bool applyNotOperator)
        {
            if (applyNotOperator)
                return !baseResult;
            else
                return baseResult;
        }

        private bool IsLabCtrlSet(string labctrlflag)
        {
            string labctrlVal = PLCSession.GetLabCtrl(labctrlflag);

            if (labctrlVal == "T")
            {
                return true;
            }
            else if ((labctrlVal == "F") || (labctrlVal == ""))
            {
                return false;
            }
            else if ((labctrlflag == "USES_SERVICE_REQUESTS") && (PLCSession.GetLabCtrl(labctrlflag) != ""))
            {
                // Handle special case where "L" or "T" is used in labctrl USES_SERVICE_REQUESTS 
                // to indicate that it's set.
                // Current code checks if labctrl USES_SERVICE_REQUESTS is empty to signify that it's not set
                // so we'll do the same type of check here and return true if USES_SERVICE_REQUESTS is set to anything
                // other than an empty string.
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CanAccessPageThroughAddressBar(HttpRequest req)
        {
            if (req.Url.Segments.Length == 0)
                return true;

            string pageFilename = req.Url.Segments[req.Url.Segments.Length - 1];
            return CanAccessPageThroughAddressBar(pageFilename);
        }

        // Return whether the page can be accessed through address bar.
        public bool CanAccessPageThroughAddressBar(string pageFilename)
        {
            // Remove any leading forward slashes and use lowercase.
            // "/Dashboard.aspx" becomes "dashboard.aspx"
            pageFilename = pageFilename.Replace("/", "").ToLower();

            // Check if page is restricted from being accessed through address bar.
            if (PLCSessionVars.pageNoAddressBarAccess.ContainsKey(pageFilename))
                return true;
            else
                return false;
        }

        public string GetStoredProcDecryptValue(string codeValue)
        {
            if (string.IsNullOrEmpty(codeValue))
                return codeValue;

            string passPhrase = "LIM$Encryption101!";
            string result = codeValue;

            try
            {
                PLCQuery qry = new PLCQuery();
                if (PLCSession.PLCDatabaseServer == "ORACLE")
                {
                    qry.AddProcedureParameter("p_codeValue", codeValue, 4000, OleDbType.VarChar, ParameterDirection.Input);
                    qry.AddProcedureParameter("p_passPhrase", passPhrase, 100, OleDbType.VarChar, ParameterDirection.Input);
                    qry.AddProcedureParameter("o_value", "", 300, OleDbType.VarChar, ParameterDirection.Output);

                    Dictionary<string, object> output = qry.ExecuteProcedure("SP_LIMS_DECRYPT");
                    result = Convert.ToString(output["o_value"]);
                }
                else
                {
                    qry.AddProcedureParameter("@p_codeValue", codeValue, 4000, OleDbType.VarChar, ParameterDirection.Input);
                    qry.AddProcedureParameter("@p_passPhrase", passPhrase, 100, OleDbType.VarChar, ParameterDirection.Input);
                    qry.AddProcedureParameter("@o_value", "", 300, OleDbType.VarChar, ParameterDirection.Output);

                    Dictionary<string, object> output = qry.ExecuteProcedure("SP_LIMS_DECRYPT");
                    result = Convert.ToString(output["@o_value"]);
                }
            }
            catch
            {
                result = codeValue;
            }

            return result;
        }

        public string GetStoredProcEncrypt(string codeValue)
        {
            if (string.IsNullOrEmpty(codeValue))
                return codeValue;

            string passPhrase = "LIM$Encryption101!";
            string result = codeValue;

            try
            {
                PLCQuery qry = new PLCQuery();
                if (PLCSession.PLCDatabaseServer == "ORACLE")
                {
                    qry.AddProcedureParameter("p_codeValue", codeValue, 300, OleDbType.VarChar, ParameterDirection.Input);
                    qry.AddProcedureParameter("p_passPhrase", passPhrase, 100, OleDbType.VarChar, ParameterDirection.Input);
                    qry.AddProcedureParameter("o_value", "", 4000, OleDbType.VarChar, ParameterDirection.Output);

                    Dictionary<string, object> output = qry.ExecuteProcedure("SP_LIMS_ENCRYPT");
                    result = Convert.ToString(output["o_value"]);
                }
                else
                {
                    qry.AddProcedureParameter("@p_codeValue", codeValue, 300, OleDbType.VarChar, ParameterDirection.Input);
                    qry.AddProcedureParameter("@p_passPhrase", passPhrase, 100, OleDbType.VarChar, ParameterDirection.Input);
                    qry.AddProcedureParameter("@o_value", "", 4000, OleDbType.VarChar, ParameterDirection.Output);

                    Dictionary<string, object> output = qry.ExecuteProcedure("SP_LIMS_ENCRYPT");
                    result = Convert.ToString(output["@o_value"]);
                }
            }
            catch
            {
                result = codeValue;
            }

            return result;
        }

#region Symmetrical Cyprtography

        private int _iterations = 2;
        private int _keySize = 256;
        private string _hash = "SHA1";

        private string SymmetricalEncrypt(string value, string password, string random)
        {
            random = random.PadRight(16, '0').Substring(0, 16);

            byte[] vectorBytes = ASCIIEncoding.ASCII.GetBytes(random);
            byte[] saltBytes = ASCIIEncoding.ASCII.GetBytes(random);

            byte[] valueBytes = UTF8Encoding.UTF8.GetBytes(value);

            byte[] encrypted;

            using (AesManaged cipher = new AesManaged())
            {
                PasswordDeriveBytes _passwordBytes = new PasswordDeriveBytes(password, saltBytes, _hash, _iterations);
                byte[] keyBytes = _passwordBytes.GetBytes(_keySize / 8);

                cipher.Mode = CipherMode.CBC;

                try
                {
                    using (ICryptoTransform encryptor = cipher.CreateEncryptor(keyBytes, vectorBytes))
                    {
                        using (MemoryStream to = new MemoryStream())
                        {
                            using (CryptoStream writer = new CryptoStream(to, encryptor, CryptoStreamMode.Write))
                            {
                                writer.Write(valueBytes, 0, valueBytes.Length);
                                writer.FlushFinalBlock();
                                encrypted = to.ToArray();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    WriteDebug(ex.Message, true);
                    return String.Empty;
                }
                cipher.Clear();
            }

            return Convert.ToBase64String(encrypted);
        }

        private string SymmetricalDecrypt(string value, string password, string random)
        {
            random = random.PadRight(16, '0').Substring(0, 16);

            byte[] vectorBytes = ASCIIEncoding.ASCII.GetBytes(random);
            byte[] saltBytes = ASCIIEncoding.ASCII.GetBytes(random);

            byte[] valueBytes = Convert.FromBase64String(value);

            byte[] decrypted;
            int decryptedByteCount = 0;

            using (AesManaged cipher = new AesManaged())
            {
                PasswordDeriveBytes _passwordBytes = new PasswordDeriveBytes(password, saltBytes, _hash, _iterations);
                byte[] keyBytes = _passwordBytes.GetBytes(_keySize / 8);

                cipher.Mode = CipherMode.CBC;

                try
                {
                    using (ICryptoTransform decryptor = cipher.CreateDecryptor(keyBytes, vectorBytes))
                    {
                        using (MemoryStream from = new MemoryStream(valueBytes))
                        {
                            using (CryptoStream reader = new CryptoStream(from, decryptor, CryptoStreamMode.Read))
                            {
                                decrypted = new byte[valueBytes.Length];
                                decryptedByteCount = reader.Read(decrypted, 0, decrypted.Length);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    WriteDebug(ex.Message, true);
                    return String.Empty;
                }

                cipher.Clear();
            }

            return Encoding.UTF8.GetString(decrypted, 0, decryptedByteCount);
        }

        public string EncryptConfig(string value, string code, int id)
        {
            return SymmetricalEncrypt(value, Convert.ToString(id), code);
        }

        public string EncryptConfig(string value, string code, string id)
        {
            return SymmetricalEncrypt(value, id, code);
        }

        public string DecryptConfig(string value, string code, int id)
        {
            return SymmetricalDecrypt(value, Convert.ToString(id), code);
        }

        public string DecryptConfig(string value, string code, string id)
        {
            return SymmetricalDecrypt(value, id, code);
        }

#endregion

        /// <summary>
        /// Reset/Restore the value of Session Timeout
        /// </summary>
        /// <param name="timeout">Optional. Less than or equal to 0 means restore original Session Timeout and is the default value.</param>
        public void ResetSessionTimeout(int timeout = 0)
        {
            if (timeout > 0 && TheSession()["PLCGlobalSessionTimeout"] == null)
            {
                TheSession()["PLCGlobalSessionTimeout"] = TheSession().Timeout;
                TheSession().Timeout = timeout;
            }
            else if (TheSession()["PLCGlobalSessionTimeout"] != null)
            {
                TheSession().Timeout = (int)TheSession()["PLCGlobalSessionTimeout"];
                TheSession()["PLCGlobalSessionTimeout"] = null;
            }
        }

        public string ProcessSQLMacro(string sql)
        {
            PLCSessionVars sv = new PLCSessionVars();
            int startIndex;
            int endIndex;
            string tempString = sql;
            string sqlString = "";
            string macro;

            if (tempString.IndexOf("<&>") >= 0)
            {
                startIndex = tempString.IndexOf("<&>");
                while (startIndex >= 0)
                {
                    sqlString += tempString.Substring(0, startIndex);
                    tempString = tempString.Remove(0, startIndex + 3);
                    endIndex = tempString.IndexOf("</&>");
                    if (endIndex >= 0)
                    {
                        macro = tempString.Substring(0, endIndex);
                        tempString = tempString.Remove(0, endIndex + 4);

                        //If <&>LABCTRL(NNN)</&> was passed.
                        if (macro.StartsWith("LABCTRL(") && (macro.EndsWith(")")))
                        {
                            int leftIndex = macro.IndexOf('(');
                            int rightIndex = macro.IndexOf(')');

                            if (rightIndex > leftIndex + 1) //Make sure there's a parameter such as LABCTRL(USES_RD). Ignore if no parameter.
                            {
                                //Given LABCTRL(USES_RD): Extract 'USES_RD' string to flag, then get labctrl setting.
                                string flag = macro.Substring(leftIndex + 1, rightIndex - leftIndex - 1);
                                sqlString += sv.GetLabCtrl(flag);
                            }
                        }
                        else
                        {
                            switch (macro)
                            {
                                case "CASE_KEY":
                                case "CASEKEY":
                                    sqlString += sv.PLCGlobalCaseKey;
                                    break;
                                case "ANALYST":
                                    sqlString += sv.PLCGlobalAnalyst;
                                    break;
                                case "USER":
                                    sqlString += sv.PLCGlobalAnalyst;
                                    break;
                                case "EVIDENCE_CONTROL_NUMBER":
                                case "EVIDENCECONTROLNUMBER":
                                    sqlString += sv.PLCGlobalECN;
                                    break;
                                case "ECN":
                                    sqlString += sv.PLCGlobalECN;
                                    break;
                                case "EXAM_KEY":
                                case "EXAMKEY":
                                    sqlString += sv.PLCGlobalAssignmentKey;
                                    break;
                                case "BATCH_NO":
                                case "BATCHNO":
                                    sqlString += sv.PLCGlobalBatchKey;
                                    break;
                                case "CHEM_CONTROL_NUM":
                                    //$$ We should be using ChemInvConstants.CHEM_CONTROL_NUM instead of "CHEM_CONTROL_NUM" 
                                    //   but we don't have access to PLCGlobals from here. We could have ChemInv use 
                                    //   a single named session variable for chem control number instead of GetChemInvProperty()
                                    //   but that would mean replacing all references to GetChemInvProperty().
                                    //   Figure this out later.
                                    sqlString += Convert.ToString(sv.GetChemInvProperty<int>("CHEM_CONTROL_NUM", 0));
                                    break;
                                case "PRELOG_DEPT_CODE":
                                    sqlString += sv.PLCGlobalPrelogDepartmentCode;
                                    break;
                                case "PRELOG_DEPT_CASE_NUM":
                                    sqlString += sv.PLCGlobalPrelogDepartmentCaseNumber;
                                    break;
                                case "PRELOG_SUB_NUM":
                                    sqlString += sv.PLCGlobalPrelogSubmissionNumber;
                                    break;
                                case "WEBUSER_DEPT_CODE":
                                    sqlString += sv.PLCGlobalAnalystDepartmentCode;
                                    break;
                                case "CONTAINER_DESCRIPTION":
                                    sqlString += sv.PLCCaseContainerDescription;
                                    break;
                                case "CONTAINER_SOURCE":
                                    sqlString += sv.PLCCaseContainerSource;
                                    break;
                                case "CONTAINER_COMMENTS":
                                    sqlString += sv.PLCCaseContainerComments;
                                    break;
                                default:
                                    sqlString += "Macro not found " + macro;
                                    break;
                            }
                        }
                    }

                    startIndex = tempString.IndexOf("<&>");
                }

                sqlString += tempString;
                sql = sqlString;
            }

            sqlString = sql;
            sqlString = sqlString.Replace("%USER%", "'" + PLCSession.PLCGlobalAnalyst + "'");
            sqlString = sqlString.Replace("%user%", "'" + PLCSession.PLCGlobalAnalyst + "'");
            sql = sqlString;

            return sql;
        }

        public string GetDateFormat()
        {
            string dateFormat = GetLabCtrl("SHORT_DATE_FORMAT").ToLower().Replace("m", "M");

            if (string.IsNullOrEmpty(dateFormat))
                dateFormat = "MM/dd/yyyy";

            return dateFormat;
        }

        public string GetDeptCtrlDateFormat()
        {
            string dateFormat = GetDeptCtrl("SHORT_DATE_FORMAT").ToLower().Replace("m", "M");

            if (string.IsNullOrEmpty(dateFormat))
                dateFormat = "MM/dd/yyyy";

            return dateFormat;
        }

        public string GetLongDateFormat()
        {
            string dateFormat = GetLabCtrl("LONG_DATE_FORMAT");

            if (string.IsNullOrEmpty(dateFormat))
                dateFormat = "MM/dd/yyyy HH:mm:ss";

            return dateFormat;
        }

        public AjaxControlToolkit.MaskedEditUserDateFormat GetUserDateFormat()
        {
            AjaxControlToolkit.MaskedEditUserDateFormat userDateFormat = AjaxControlToolkit.MaskedEditUserDateFormat.MonthDayYear;
            if (GetDateFormat().ToUpper().StartsWith("DD"))
                userDateFormat = AjaxControlToolkit.MaskedEditUserDateFormat.DayMonthYear;

            return userDateFormat;
        }

        public string GetDateMask()
        {
            Regex pattern = new Regex(@"\w", RegexOptions.IgnoreCase);

            return pattern.Replace(GetDateFormat(), "9");
        }

        public string GetCultureName()
        {
            if (GetDateFormat().ToUpper().StartsWith("DD"))
                return "en-GB";
            else
                return "en-US";
        }

        public string GetPrelogCultureName()
        {
            if (GetDeptCtrlDateFormat().ToUpper().StartsWith("DD"))
                return "en-GB";
            else
                return "en-US";
        }

        public DateTime ConvertToDateTime(string dateTimeText)
        {
            return (GetDateFormat().ToUpper().StartsWith("DD"))
                ? Convert.ToDateTime(dateTimeText, CultureInfo.CreateSpecificCulture("en-GB"))
                : Convert.ToDateTime(dateTimeText, CultureInfo.CreateSpecificCulture("en-US"));
        }

        public string DateStringToMDY(string date)
        {
            if (string.IsNullOrEmpty(date))
                return string.Empty;
            else
            {
                if (GetDateFormat().ToUpper().StartsWith("DD"))
                {
                    return DateTime.Parse(date, CultureInfo.CreateSpecificCulture("en-GB")).ToString("MM/dd/yyyy");
                }
                else
                {
                    return date;
                }
            }
        }

        public string DateStringToDMY(string date)
        {
            if (string.IsNullOrEmpty(date))
                return string.Empty;
            else
            {
                return DateTime.Parse(date, CultureInfo.CreateSpecificCulture("en-US")).ToString("dd/MM/yyyy");
            }
        }

        public string ContainerPackagedInText()
        {
            string packagingText = GetLabCtrl("CONTAINER_PACKAGING_TEXT").Trim();
            packagingText = (string.IsNullOrEmpty(packagingText) ? "Packaged In" : packagingText);
            return packagingText;
        }

        public string GetDNACtrl(string columnName, string section)
        {
            string value = string.Empty;

            try
            {
                if (!string.IsNullOrEmpty(columnName))
                {
                    PLCQuery qryDNACtrl = new PLCQuery();
                    qryDNACtrl.SQL = "SELECT " + columnName + " FROM TV_DNACTRL WHERE LAB_CODE = '" + PLCSession.PLCGlobalLabCode + "' AND SECTION = '" + section + "'";
                    qryDNACtrl.Open();

                    if (qryDNACtrl.HasData())
                        return qryDNACtrl.FieldByName(columnName).ToString();
                }
            }
            catch (Exception e) {}

            return value;
        }

        public string GetSectionItemSettings(string columnName, string section)
        {
            string value = string.Empty;

            try
            {
                if (!string.IsNullOrEmpty(columnName))
                {
                    PLCQuery qrySectItem = new PLCQuery();
                    qrySectItem.SQL = "SELECT * FROM TV_SECTITEM WHERE LAB_CODE = '" + PLCSession.PLCGlobalLabCode + "' AND SECTION = '" + section + "' AND CATEGORY_RES = '" + DateTime.Now.Year.ToString() + "'";
                    qrySectItem.Open();

                    if (!qrySectItem.HasData())
                    {
                        qrySectItem.Append();
                        qrySectItem.SetFieldValue("LAB_CODE", PLCSession.PLCGlobalLabCode);
                        qrySectItem.SetFieldValue("SECTION", section);
                        qrySectItem.SetFieldValue("CATEGORY_RES", DateTime.Now.Year.ToString());
                        qrySectItem.SetFieldValue("PREFIX", "YYYY"); //default format
                        qrySectItem.SetFieldValue("PAD_LENGTH", 5);
                        qrySectItem.SetFieldValue("NEXT_NUMBER", 1);
                        qrySectItem.SetFieldValue("FORMAT", "YY99999TT");
                        qrySectItem.Post("TV_SECTITEM", 700, 1);
                    }

                    value = GetSectionItemFlag(columnName, qrySectItem);
                }
            }
            catch (Exception e) {}

            return value;
        }

        private string GetSectionItemFlag(string columnName, PLCQuery qry)
        {
            qry.Open();
            string resultValue = qry.FieldByName(columnName).ToString();

            if (columnName == "NEXT_NUMBER")
            {
                qry.Edit();
                qry.SetFieldValue(columnName, Convert.ToInt32(resultValue) + 1);
                qry.Post("TV_SECTITEM", 700, 2);
            }

            return resultValue;
        }

        public void FormatDatePLCGridViewBoundFields(PLCGridView grid)
        {
            if (grid != null)
            {
                foreach (object columnObj in grid.Columns)
                {
                    if (columnObj.GetType().Equals(typeof(BoundField)))
                    {
                        BoundField column = (BoundField)columnObj;
                        if (column.DataField.ToUpper().StartsWith("DATE_") || column.DataField.ToUpper().Contains("_DATE"))
                            column.DataFormatString = GetDateFormatString();
                    }
                }
            }
        }

        public void FormatDateGridViewBoundFields(GridView grid)
        {
            if (grid != null)
            {
                foreach (object columnObj in grid.Columns)
                {
                    if (columnObj.GetType().Equals(typeof(BoundField)))
                    {
                        BoundField column = (BoundField)columnObj;
                        if (column.DataField.ToUpper().StartsWith("DATE_") || column.DataField.ToUpper().Contains("_DATE"))
                            column.DataFormatString = GetDateFormatString();
                    }
                }
            }
        }

        private string GetDateFormatString()
        {
            if (this.GetDateFormat().ToUpper().StartsWith("DD"))
                return "{0:" + this.GetDateFormat() + "}";
            else
                return "{0:d}";
        }

#region GlobalApplicationProperty
        public static void SetGlobalApplicationProperty<T>(string key, T value)
        {
            lock (locker)
            {
                if (GlobalApplicationProperty.ContainsKey(key))
                    GlobalApplicationProperty[key] = value;
                else
                    GlobalApplicationProperty.Add(key, value);
            }
        }

        public static T GetGlobalApplicationProperty<T>(string key)
        {
            lock (locker)
            {
                if (GlobalApplicationProperty.ContainsKey(key))
                    return (T)GlobalApplicationProperty[key];
                else
                    return default(T);
            }
        }

        static Dictionary<string, object> GlobalApplicationProperty = new Dictionary<string, object>();
        static object locker = new object();
#endregion

        public Dictionary<String, String> GetItemBCDefaultSettings(string source = "")
        {
            Dictionary<String, String> dictDefaultSettings = new Dictionary<String, String>();

            //COLLECTED SETTINGS
            string defaultCollectedBy = string.Empty;
            string defaultCollectedDate = string.Empty;
            string defaultCollectedTime = string.Empty;

            //BOOKED SETTINGS
            string defaultBookedBy = string.Empty;
            string defaultBookingDate = string.Empty;
            string defaultBookingTime = string.Empty;

            if (source == "DUPE")
            {
                defaultCollectedBy = GetGlobalINIValue("DUPE_COLLECTED_BY");
                defaultCollectedDate = GetGlobalINIValue("DUPE_COLLECTED_DATE");
                defaultCollectedTime = GetGlobalINIValue("DUPE_COLLECTED_TIME");

                defaultBookedBy = GetGlobalINIValue("DUPE_BOOKED_BY");
                defaultBookingDate = GetGlobalINIValue("DUPE_BOOKING_DATE");
                defaultBookingTime = GetGlobalINIValue("DUPE_BOOKING_TIME");
            }
            else if (source == "SAMPLE")
            {
                //COLLECTED SETTINGS
                defaultCollectedBy = GetGlobalINIValue("SAMPLE_COLLECTED_BY");
                defaultCollectedDate = GetGlobalINIValue("SAMPLE_COLLECTED_DATE");
                defaultCollectedTime = GetGlobalINIValue("SAMPLE_COLLECTED_TIME");

                //BOOKED SETTINGS
                defaultBookedBy = GetGlobalINIValue("SAMPLE_BOOKED_BY");
                defaultBookingDate = GetGlobalINIValue("SAMPLE_BOOKING_DATE");
                defaultBookingTime = GetGlobalINIValue("SAMPLE_BOOKING_TIME");
            }
            else if (source == "KIT")
            {
                //COLLECTED SETTINGS
                defaultCollectedBy = GetGlobalINIValue("KIT_COLLECTED_BY");
                defaultCollectedDate = GetGlobalINIValue("KIT_COLLECTED_DATE");
                defaultCollectedTime = GetGlobalINIValue("KIT_COLLECTED_TIME");

                //BOOKED SETTINGS
                defaultBookedBy = GetGlobalINIValue("KIT_BOOKED_BY");
                defaultBookingDate = GetGlobalINIValue("KIT_BOOKING_DATE");
                defaultBookingTime = GetGlobalINIValue("KIT_BOOKING_TIME");
            }

            dictDefaultSettings.Add("COLLECTED_BY", defaultCollectedBy);
            dictDefaultSettings.Add("DATE_COLLECTED", defaultCollectedDate);
            dictDefaultSettings.Add("TIME_COLLECTED", defaultCollectedTime);
            dictDefaultSettings.Add("BOOKED_BY", defaultBookedBy);
            dictDefaultSettings.Add("BOOKING_DATE", defaultBookingDate);
            dictDefaultSettings.Add("BOOKING_TIME", defaultBookingTime);

            return dictDefaultSettings;
        }

        public string FormattingTags()
        {
            return GetGlobalINIValue("FORMATTING_TAGS");
        }

        public List<string> GetGlobalAnalystAccessCodes(String theAnalyst)
        {
            String analystAccessCodes = "";
            String analystGroupCodes = "";

            char[] separators = { ',', ' ' };
            string ret = PLCSession.GetDefault("ANALYST_ACCESS_CODES_" + theAnalyst);
            if (ret == "*NONE") return "".Split(',').ToList<String>();
            if (!String.IsNullOrWhiteSpace(ret)) return ret.Split(separators, StringSplitOptions.RemoveEmptyEntries).ToList<String>();

            //nothing is cached for this analyst so lets go get it.
            PLCQuery qryAnalystAccess = new PLCQuery(string.Format("SELECT ACCESS_CODE, GROUP_CODE FROM TV_ANALYST WHERE ANALYST = '{0}'", theAnalyst));
            qryAnalystAccess.Open();
            if (!qryAnalystAccess.IsEmpty())
            {
                analystAccessCodes = qryAnalystAccess.FieldByName("ACCESS_CODE");
                analystGroupCodes = qryAnalystAccess.FieldByName("GROUP_CODE");
            }

            if (!String.IsNullOrWhiteSpace(analystGroupCodes))
            {
                String inStr = "";
                List<String> gcodes = analystGroupCodes.Split(separators, StringSplitOptions.RemoveEmptyEntries).ToList<String>();
                foreach (String s in gcodes)
                {
                    if (!String.IsNullOrWhiteSpace(inStr)) inStr += ",";
                    inStr += "'" + s + "'";
                }

                if (!String.IsNullOrWhiteSpace(inStr))
                {
                    PLCQuery qryGroupAccess = new PLCQuery("SELECT ACCESS_CODES FROM TV_GROUPS WHERE GROUP_CODE IN (" + inStr + ") and ACCESS_CODES IS NOT NULL");
                    qryGroupAccess.Open();
                    while (!qryGroupAccess.EOF())
                    {
                        if (!String.IsNullOrWhiteSpace(analystAccessCodes)) analystAccessCodes += ",";
                        analystAccessCodes += qryGroupAccess.FieldByName("ACCESS_CODES");
                        qryGroupAccess.Next();
                    }
                }
            }

            if (String.IsNullOrWhiteSpace(analystAccessCodes))
            {
                //save the cached valus and return empty list
                PLCSession.SetDefault("ANALYST_ACCESS_CODES_" + theAnalyst, "*NONE");
                return "".Split(',').ToList<String>();
            }
            else
            {
                // save the cached value and return the codes in a list
                PLCSession.SetDefault("ANALYST_ACCESS_CODES_" + theAnalyst, analystAccessCodes);
                return analystAccessCodes.Split(separators, StringSplitOptions.RemoveEmptyEntries).ToList<String>();
            }
        }

        public string GetCODNAUserField(string userID, string fieldName)
        {
            var qry = new PLCQuery();
            qry.SQL = "SELECT " + fieldName + " FROM TV_COWEBUSE WHERE USER_ID = ?";
            qry.AddSQLParameter("USER_ID", userID);
            qry.Open();

            if (qry.HasData() && qry.FieldExist(fieldName))
                return qry.FieldByName(fieldName);
            else
                return string.Empty;
        }

        public string PrelogUserName(string userID)
        {
            if (IsCODNAPrelog())
                return GetCODNAUserField(PLCGlobalPrelogUser, "NAME");

            PLCQuery qryWebUserName = new PLCQuery();
            qryWebUserName.SQL = string.Format("SELECT USER_NAME FROM TV_WEBUSER WHERE USER_ID = '{0}'", userID);
            qryWebUserName.Open();
            if (!qryWebUserName.IsEmpty())
                return qryWebUserName.FieldByName("USER_NAME");
            else
                return string.Empty;

        }

        public bool assignmentUsesOpenXML()
        {
            const string strFlagName = "USES_OPEN_XML";
            bool sectionUsesOpenXML = false;

            try
            {

                PLCQuery qryExamCode = new PLCQuery();
                qryExamCode.SQL = String.Format(
                    @"select ec.{0} 
                    from TV_LABASSIGN la 
                    join TV_EXAMCODE ec on ec.EXAM_CODE = la.SECTION
                    where la.EXAM_KEY = {1}",
                    strFlagName,
                    PLCSession.PLCGlobalAssignmentKey
                );

                qryExamCode.Open();
                sectionUsesOpenXML = qryExamCode.FieldByName(strFlagName) == "T" ? true : false;
            }
            catch (Exception e)
            {
                PLCSession.WriteDebug("Could not retrieve Open XML settings for this section, will proceed to use default (client)." + System.Environment.NewLine + e.Message, true);
            }

            return sectionUsesOpenXML;
        }

        public bool resetTemplateOnRegen()
        {
            const string strFlagName = "RESET_TEMPLATE_ON_REGEN";
            bool resetTemplate = false;

            try
            {
                PLCQuery qryExamCode = new PLCQuery();
                qryExamCode.SQL = String.Format(
                    @"select ec.{0} 
                    from TV_LABASSIGN la 
                    join TV_EXAMCODE ec on ec.EXAM_CODE = la.SECTION
                    where la.EXAM_KEY = {1}",
                    strFlagName,
                    PLCSession.PLCGlobalAssignmentKey
                );

                qryExamCode.Open();
                resetTemplate = qryExamCode.FieldByName(strFlagName) == "T" ? true : false;
            }
            catch (Exception e)
            {
                PLCSession.WriteDebug("Could not retrieve RESET_TEMPLATE_ON_REGEN value. Defaulting to False." + System.Environment.NewLine + e.Message, true);
            }

            return resetTemplate;
        }

        public string GetGlobalIni(string flag)
        {
            return GetGlobalINIValue(flag);
        }

        public void WriteIVServiceLog(string time = "", string valid = "", string details = "")
        {
            details = "JWT: " + PLCSession.GetDefault("JWT") + System.Environment.NewLine + details;

            PLCQuery qry = new PLCQuery("SELECT * FROM TV_IMAGEVLTSVCLOG WHERE 0 = 1");
            qry.Open();
            qry.Append();
            qry.SetFieldValue("LOG_KEY", PLCSession.GetNextSequence("IMAGEVLTSVCLOG_SEQ"));
            qry.SetFieldValue("USER_ID", PLCSession.PLCGlobalAnalyst);
            qry.SetFieldValue("REQUEST_DATE", Convert.ToDateTime(time));
            qry.SetFieldValue("TOKEN_VALID", valid);
            qry.SetFieldValue("DETAILS", details);
            qry.SetFieldValue("PROGRAM", "iLIMS" + PLCSession.PLCBEASTiLIMSVersion.Substring(0, 5));
            qry.Post("TV_IMAGEVLTSVCLOG");
        }

        public string GetJWT()
        {
            var expiration = PLCSession.GetProperty<DateTime>("JWT_EXP", DateTime.UtcNow);
            if (expiration != DateTime.MinValue && expiration.AddMinutes(-3) > DateTime.UtcNow)
                return PLCSession.GetDefault("JWT");

            int jwtExpiry = PLCSession.SafeInt(GetGlobalINIValue("IMAGEVLT_JWT_EXPIRY"), 20);
            if (jwtExpiry < 1)
                jwtExpiry = 20;

            var header = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"));

            var iss = Context.Request.ServerVariables["LOCAL_ADDR"];
            var aud = AESEncryption.GetMD5Hash(PLCSession.PLCDBDatabase);
            var jti = PLCSession.GetDefault("JTI");
            var iat = DateTime.UtcNow;
            var exp = iat.AddMinutes(jwtExpiry);
            var format = "yyyy-MM-dd@HH:mm:ss";
            var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(
                "{\"iss\":\"" + iss + "\",\"aud\":\"" + aud + "\",\"jti\":\"" + jti + "\",\"iat\":\"" + iat.ToString(format) + "\",\"exp\":\"" + exp.ToString(format) + "\"}"
            ));

            var key = Encoding.UTF8.GetBytes(PLCSession.GetDefault("JWTSECRET"));
            var data = Encoding.UTF8.GetBytes(header + "." + payload);
            var hmac = System.Security.Cryptography.HMAC.Create("HMACSHA256");
            hmac.Key = key;
            var signature = Convert.ToBase64String(hmac.ComputeHash(data));

            var token = header + "." + payload + "." + signature;

            PLCSession.SetDefault("JWT", token);
            PLCSession.SetProperty<DateTime>("JWT_EXP", exp);

            return token;
        }

#region Password Hash

        /// <summary>
        /// Computes the hash of a user password
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="UserPassword"></param>
        /// <param name="hashName"></param>
        /// <returns></returns>
        internal static string GetPasswordHash(string UserID, string UserPassword, string hashName = null)
        {
            byte[] data = Encoding.Default.GetBytes(UserID.ToUpper() + UserPassword);
            HashAlgorithm hashAlg = string.IsNullOrEmpty(hashName) ?
                GetPasswordHashAlgorithm() : HashAlgorithm.Create(hashName);

            byte[] hashBytes = hashAlg.ComputeHash(data);
            string hashpw = BitConverter.ToString(hashBytes);
            hashpw = hashpw.Replace("-", "");
            return hashpw;
        }

        /// <summary>
        /// Returns the HashAlgorithm used for password. Configurable via PASSWORDENCRYPT appSetting in web.config 
        /// </summary>
        /// <returns></returns>
        internal static HashAlgorithm GetPasswordHashAlgorithm()
        {
            HashAlgorithm hashAlg = null;
            string hashName = System.Configuration.ConfigurationManager.AppSettings.Get("PASSWORDENCRYPT");
            if (!string.IsNullOrEmpty(hashName))
                hashAlg = HashAlgorithm.Create("System.Security.Cryptography." + hashName);
            if (hashAlg == null)
                hashAlg = new SHA1Cng();
            return hashAlg;
        }

        /// <summary>
        /// Get FIPS compliant hash algo based on the length of a generated hash
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        internal static string GetFIPSHashNameBasedOnLength(string hash)
        {
            string hashName = "System.Security.Cryptography.";
            if (hash.Length == 64)
            {
                hashName += "SHA256Cng";
            }
            else if (hash.Length == 96)
            {
                hashName += "SHA384Cng";
            }
            else if (hash.Length == 128)
            {
                hashName += "SHA512Cng";
            }
            else
            {
                hashName += "SHA1Cng";
            }
            return hashName;
        }

        /// <summary>
        /// Update password hash in the db to the new algo if the credentials match
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="hash"></param>
        /// <param name="userID"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        internal string UpdatePasswordHash(string tableName, string hash, string userID, string password)
        {
            string hashName = GetFIPSHashNameBasedOnLength(hash);
            // Get hash using the previous algo to verify password entered
            string hashPassword = GetPasswordHash(userID, password, hashName);
            if (hashPassword == hash)
            {
                // Update password hash in the database using the current algo
                string idField = tableName.Equals("TV_PASSWORD") ? "PASSWORD_ID" : "USER_ID";
                var qry = new PLCQuery("SELECT * FROM " + tableName + " WHERE " + idField + " = ?");
                qry.AddSQLParameter(idField, userID);
                qry.Open();
                if (qry.HasData())
                {
                    hashPassword = GetPasswordHash(userID, password);

                    var qryPassword = new PLCQuery("SELECT * FROM " + tableName + " WHERE " + idField + " = '" + userID + "'");
                    qryPassword.Open();
                    qryPassword.Edit();
                    qryPassword.SetFieldValue("PASSWORD", hashPassword);
                    qryPassword.Post(tableName, 1001, 0);

                    return hashPassword;
                }
            }
            return string.Empty;
        }

#endregion Password Hash

#region Prelog Inquiry

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue(false)]
        [Localizable(true)]
        public bool PLCPrelogInquiryMode
        {
            get
            {
                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCPrelogInquiryMode"] == null)
                    return false;
                else
                {
                    bool s = (bool)TheSession()["PLCPrelogInquiryMode"];
                    return s;
                }
            }

            set
            {
                TheSession()["PLCPrelogInquiryMode"] = value;
            }
        }

        #endregion

        [Category("PLC Properties"), Description("SessionVariableStorageClass")]
        [DefaultValue(false)]
        [Localizable(true)]
        public bool PLCCodnaInquiryMode
        {
            get
            {
                //if (ThePage() == (Page)null) return "";

                if (TheSession()["PLCCodnaInquiryMode"] == null)
                    return false;
                else
                {
                    bool s = (bool)TheSession()["PLCCodnaInquiryMode"];
                    return s;
                }
            }

            set
            {
                TheSession()["PLCCodnaInquiryMode"] = value;
            }
        }

        public void WriteNotificationLog(string msg)
        {
            try
            {
                bool isPrelogMode = !string.IsNullOrEmpty(PLCGlobalPrelogUser) && PLCGlobalAnalyst == "";
                string prelogDebugName = isPrelogMode ? GetPrelogDebugName() : "Prelog-";

                if (GetUserPref("SESSIONTRACE") == "ENABLED" || PLCGlobalAnalyst == "")
                {
                    string fileName = PLCPath.Notifications + "webdebug-" + (isPrelogMode ? prelogDebugName : "") +
                        DateTime.Today.Date.Month + "-" + DateTime.Today.Date.Day + "-" + DateTime.Today.Date.Year +
                        "(" + (isPrelogMode ? PLCGlobalPrelogUser : PLCGlobalAnalyst) + ").TXT";

                    TextWriter tw = new StreamWriter(fileName, true);
                    tw.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss (fff) ") + " : " + msg);
                    tw.Close();
                }
            }
            catch (Exception E)
            {
                PLCErrorMessage = E.Message + "  " + msg;
            }
        }

        /// <summary>
        /// On successful login, assign a new Session ID to the authenticated user.
        /// </summary>
        public void RenewSessionID()
        {
            // Initialise variables for regenerating the session id
            HttpContext context = HttpContext.Current;
            SessionIDManager manager = new SessionIDManager();
            string oldID = manager.GetSessionID(context);
            string newID = manager.CreateSessionID(context);
            bool isAdd = false, isRedir = false;

            // Save a new session ID
            manager.SaveSessionID(context, newID, out isRedir, out isAdd);

            // Get the fields using the below and create variables for storage
            HttpApplication appInstance = HttpContext.Current.ApplicationInstance;
            HttpModuleCollection modules = appInstance.Modules;
            SessionStateModule ssm = (SessionStateModule)modules.Get("Session");
            FieldInfo[] fields = ssm.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            SessionStateStoreProviderBase store = null;
            FieldInfo rqIDField = null, rqLockIDField = null, rqStateNotFoundField = null;
            SessionStateStoreData rqItem = null;

            // Assign to each variable the appropriate field values
            foreach (FieldInfo field in fields)
            {
                if (field.Name.Equals("_store")) store = (SessionStateStoreProviderBase)field.GetValue(ssm);
                if (field.Name.Equals("_rqId")) rqIDField = field;
                if (field.Name.Equals("_rqLockId")) rqLockIDField = field;
                if (field.Name.Equals("_rqSessionStateNotFound")) rqStateNotFoundField = field;
                if (field.Name.Equals("_rqItem")) rqItem = (SessionStateStoreData)field.GetValue(ssm);
            }

            // Remove the previous session value
            object lockID = rqLockIDField.GetValue(ssm);
            if ((lockID != null) && (oldID != null))
                store.RemoveItem(context, oldID, lockID, rqItem);

            rqStateNotFoundField.SetValue(ssm, true);
            rqIDField.SetValue(ssm, newID);
        }

        /// <summary>
        /// This will get the string value on LocalizedText via key.
        /// It should be on try catch to prevent issues when file is not present.
        /// </summary>
        /// <param name="resourceKey"></param>
        /// <returns></returns>
        public string GetLocalizedTextData(string resourceKey)
        {
            try
            {
                string resourceValue = string.Empty;

                resourceValue = GetSysPrompt(resourceKey);

                if (string.IsNullOrEmpty(resourceValue))
                    // text from database
                    resourceValue = GetLocalizedTextDataFromDB(resourceKey);

                // text from customized resource file
                if (string.IsNullOrEmpty(resourceValue))
                {
                    try
                    {
                        resourceValue = HttpContext.GetGlobalResourceObject("LocalizedText", resourceKey, CultureInfo.CurrentCulture).ToString();
                    }
                    catch { }
                }

                // text from default text values
                if (string.IsNullOrEmpty(resourceValue))
                    resourceValue = DefaultLocalizedTextValue[resourceKey];

                return resourceValue;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Get the value of LocalizedText from TV_TEXTRESOURCES via key
        /// </summary>
        /// <param name="resourceKey"></param>
        /// <returns></returns>
        public string GetLocalizedTextDataFromDB(string resourceKey)
        {
            // check for database connection before trying to fetch text resource
            if ((this.ConnectionString == "") || (this.ConnectionString == "Provider=;Data Source=;User ID=;Password=;"))
                return string.Empty;

            if (PLCSession.PLCDBName != "")
            {
                if (PLCSession.PLCGlobalUsesDBTextResources == "")
                {
                    try
                    {
                        PLCQuery qryTextResourcesRows = new PLCQuery();
                        qryTextResourcesRows.SQL = "SELECT COUNT(*) AS TEXT_RESOURCE_COUNT FROM TV_TEXTRESOURCES";
                        qryTextResourcesRows.Open();
                        if (qryTextResourcesRows.HasData())
                        {
                            int rowCount = Convert.ToInt32(qryTextResourcesRows.FieldByName("TEXT_RESOURCE_COUNT"));
                            if (rowCount > 0)
                                PLCSession.PLCGlobalUsesDBTextResources = "T";
                            else
                                PLCSession.PLCGlobalUsesDBTextResources = "F";
                        }
                    }
                    catch {
                        PLCSession.PLCGlobalUsesDBTextResources = "F";
                    }
                }

                if (PLCSession.PLCGlobalUsesDBTextResources == "T")
                {
                    string sessionKey = "TR_" + resourceKey;
                    if (!CacheHelper.IsInCache(sessionKey))
                    {
                        PLCQuery qryGetTextData = new PLCQuery();
                        qryGetTextData.SQL = "SELECT * FROM TV_TEXTRESOURCES WHERE RESOURCE_ID = ?";
                        qryGetTextData.AddParameter("RESOURCE_ID", resourceKey);
                        qryGetTextData.Open();
                        if (qryGetTextData.HasData())
                            CacheHelper.AddItem(sessionKey, qryGetTextData.FieldByName("TEXT_DATA"));
                        else
                            CacheHelper.AddItem(sessionKey, "");
                    }

                    return CacheHelper.GetItem(sessionKey).ToString();
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Checks if CASEHEADERTEXT function exists
        /// </summary>
        /// <returns></returns>
        public bool CheckCaseTitleFuncExist()
        {
            PLCQuery qryHasTitleFunc = new PLCQuery();
            if (PLCSession.PLCDatabaseServer == "MSSQL")
                qryHasTitleFunc.SQL = "SELECT name FROM sys.objects WHERE name = 'CASEHEADERTEXT' AND type IN ('FN', 'IF', 'TF', 'FS', 'FT')";
            else
                qryHasTitleFunc.SQL = "SELECT OBJECT_NAME FROM USER_OBJECTS WHERE OBJECT_TYPE = 'FUNCTION' AND OBJECT_NAME = 'CASEHEADERTEXT'";
            qryHasTitleFunc.Open();

            return qryHasTitleFunc.HasData();
        }

        /// <summary>
        /// Get the case title from CASEHEADERTEXT function
        /// </summary>
        /// <returns></returns>
        public string GetCaseTitleFromFunc(string tabName)
        {
            PLCQuery qryCaseTitleFunc = new PLCQuery();
            if (PLCSession.PLCDatabaseServer == "MSSQL")
                qryCaseTitleFunc.SQL = "SELECT dbo.CASEHEADERTEXT('" + tabName + "', " + PLCSession.PLCGlobalCaseKey + ") AS HEADER_TEXT";
            else
                qryCaseTitleFunc.SQL = "SELECT CASEHEADERTEXT('" + tabName + "', " + PLCSession.PLCGlobalCaseKey + ") AS HEADER_TEXT FROM DUAL";

            qryCaseTitleFunc.Open();
            if (qryCaseTitleFunc.HasData())
                return HttpUtility.HtmlEncode(qryCaseTitleFunc.FieldByName("HEADER_TEXT"));

            return "";
        }

#region System Language
        public string GetSysPrompt(string code, string value = "", string language = "")
        {
            string prompt = string.Empty;
            string lang = string.IsNullOrEmpty(language)
                ? GetSystemLanguage()
                : language;

            if (string.IsNullOrEmpty(lang)
                || string.IsNullOrEmpty(code)
                || string.IsNullOrEmpty(PLCDBName))
                return value;

            string cacheKey = CacheHelper.GetCacheKey("SYS_" + lang + "_" + code);
            if (CacheHelper.IsInCache(cacheKey))
            {
                prompt = CacheHelper.GetItem(cacheKey).ToString();
            }
            else
            {
                prompt = GetSysPromptFromDB(code, lang);
                CacheHelper.AddItem(cacheKey, prompt);
            }

            return string.IsNullOrEmpty(prompt) ? value : prompt;
        }

        private string GetSysPromptFromDB(string code, string language)
        {
            var qry = new PLCQuery();
            qry.SQL = "SELECT PROMPT_TEXT FROM TV_SYSPROMPT "
                + "WHERE PROMPT_CODE = ? AND LANGUAGE = ?";
            qry.AddSQLParameter("PROMPT_CODE", code);
            qry.AddSQLParameter("LANGUAGE", language);
            qry.OpenReadOnly();
            if (qry.HasData())
            {
                return qry.FieldByName("PROMPT_TEXT");
            }

            return string.Empty;
        }

        public string GetSystemLanguage()
        {
            string setting = !string.IsNullOrEmpty(PLCGlobalPrelogUser) ? GetWebUserField("SYSTEM_LANGUAGE") : PLCSession.GetUserPref("SYS_LANGUAGE");
            if (!string.IsNullOrEmpty(setting))
                return setting;

            const string SYS_LANG = "SYS_LANG";

            var appSettings = System.Configuration.ConfigurationManager.AppSettings;
            if (appSettings.AllKeys.Contains(SYS_LANG))
            {
                setting = appSettings[SYS_LANG].Trim();
            }

            return setting;
        }
#endregion System Language

        /// <summary>
        /// Return T is the prelog user is an administrator
        /// </summary>
        public string PrelogUserIsAdmin()
        {
            if (PLCSession.PLCGlobalPrelogUserIsAdmin == "")
            {
                PLCQuery qryUserIsAdmin = new PLCQuery();
                string userID = PLCSession.PLCGlobalPrelogUser;
                string query = "SELECT ADMINISTRATOR FROM {1} WHERE USER_ID = '{0}'";
                if (this.ThePage().Request.Url.ToString().ToUpper().Contains("CODNAPRELOG"))
                    qryUserIsAdmin.SQL = string.Format(query, userID, "TV_COWEBUSE");
                else
                    qryUserIsAdmin.SQL = string.Format(query, userID, "TV_WEBUSER");

                qryUserIsAdmin.Open();

                if (qryUserIsAdmin.HasData())
                    PLCSession.PLCGlobalPrelogUserIsAdmin = qryUserIsAdmin.FieldByName("ADMINISTRATOR").ToUpper();
                else
                    PLCSession.PLCGlobalPrelogUserIsAdmin = "F";
            }
            return PLCSession.PLCGlobalPrelogUserIsAdmin;
        }

        /// <summary>
        /// get the value of a flag of a webuser
        /// </summary>
        public string GetWebUserField(string fieldName)
        {
            string userID = PLCSession.PLCGlobalPrelogUser;
            string table = "TV_WEBUSER";
            if (this.ThePage() != null && this.ThePage().Request.Url.ToString().ToUpper().Contains("CODNAPRELOG"))
                table = "TV_COWEBUSE";

            string flagKey = string.Format("WebF_{0}", fieldName);

            if(!CacheHelper.IsInUserCache(flagKey))
            {
                PLCQuery qryUser = new PLCQuery();
                qryUser.SQL = string.Format("SELECT {0} FROM {1} WHERE USER_ID = '{2}'", fieldName, table, userID);
                qryUser.Open();

                if (qryUser.HasData())
                    CacheHelper.AddUserItem(flagKey, qryUser.FieldByName(fieldName).ToUpper());
                else
                    CacheHelper.AddUserItem(flagKey, "");
            }
            return CacheHelper.GetUserItem(flagKey).ToString();
        }

        public int GetCodnaNextSampleID(string category, int year)
        {
            int nextSampleNum = 0;
            string sNextCaseNum = "";

            PLCQuery qryNextCase = new PLCQuery();
            qryNextCase.SQL = "SELECT * from TV_PREPRNLB Where CATEGORY_RES  = '" + category + "' and YEAR = " + year.ToString();
            qryNextCase.ExcludeFromTransaction = true;
            qryNextCase.Open();
            if (qryNextCase.IsEmpty())
            {
                nextSampleNum = 1;
                qryNextCase.Append();
                qryNextCase.SetFieldValue("CATEGORY_RES", category);
                qryNextCase.SetFieldValue("YEAR", year);
                qryNextCase.SetFieldValue("NEXT_BARCODE", 2);
                qryNextCase.Post("TV_PREPRNLB", 9710, 1);
                return nextSampleNum;
            }
            else
            {
                sNextCaseNum = qryNextCase.FieldByName("NEXT_BARCODE");
                if (sNextCaseNum == "") sNextCaseNum = "-1";
                nextSampleNum = PLCSession.SafeInt(sNextCaseNum, -1);
                if (nextSampleNum < 0)
                {
                }
                qryNextCase.Edit();
                qryNextCase.SetFieldValue("NEXT_BARCODE", nextSampleNum + 1);
                qryNextCase.Post("TV_PREPRNLB", 9710, 2);
                return nextSampleNum;
            }
        }

#region AppURL
        /// <summary>
        /// Get the application base URL. Set PROXY_SERVER_NAME in appSettings when using reverse proxy.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public string GetApplicationURL(HttpRequest request)
        {
            string authority = GetProxyServer();

            if (string.IsNullOrEmpty(authority))
                authority = request.Url.Authority;

            string url = request.Url.Scheme + "://" + authority + request.ApplicationPath.TrimEnd('/');

            PLCSession.WriteDebug("@AppURL: " + url);

            return url;
        }

        /// <summary>
        /// Get the proxy server configured in web.config > appSettings
        /// </summary>
        /// <returns></returns>
        public string GetProxyServer()
        {
            const string PROXY_SERVER_NAME = "PROXY_SERVER_NAME";
            string proxyServer = string.Empty;

            var appSettings = System.Configuration.ConfigurationManager.AppSettings;
            if (appSettings.AllKeys.Contains(PROXY_SERVER_NAME))
            {
                proxyServer = appSettings[PROXY_SERVER_NAME].Trim();
            }

            return proxyServer;
        }
#endregion AppURL

#region OCX
        /// <summary>
        /// Get ocx module used by the function
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        public string GetOCXModule(string functionName)
        {
            string module = string.Empty;

            string cacheKey = CacheHelper.GetCacheKey("OCX_MODULE_" + functionName);
            if (CacheHelper.IsInCache(cacheKey))
            {
                module = CacheHelper.GetItem(cacheKey).ToString();
            }
            else
            {
                module = GetOCXModuleCalls(functionName);
                CacheHelper.AddItem(cacheKey, module);
            }

            return module;
        }

        /// <summary>
        /// Get ocx module used by the function from DB
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        private string GetOCXModuleCalls(string functionName)
        {
            string module = string.Empty;

            var qry = new PLCQuery();
            qry.SQL = "SELECT MODULE FROM TV_OCXMODULECALLS "
                + "WHERE FUNCTION_NAME = ?";
            qry.AddSQLParameter("FUNCTION_NAME", functionName);
            qry.OpenReadOnly();

            if (qry.HasData())
            {
                module = qry.FieldByName("MODULE");
            }

            return module;
        }

        /// <summary>
        /// Get RDB config
        /// </summary>
        /// <param name="functionName"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public string GetRDBConfig(string functionName, string flag)
        {
            string config = string.Empty;
            string module = GetOCXModule(functionName);
            string clientId = ClientID;

            if (!string.IsNullOrEmpty(module)
                && !string.IsNullOrEmpty(clientId))
            {
                var qry = new PLCQuery();
                qry.SQL = "SELECT * FROM TV_RDBCONFIG "
                    + "WHERE MODULE = ? AND CLIENT_ID = ?";
                qry.AddSQLParameter("MODULE", module);
                qry.AddSQLParameter("CLIENT_ID", clientId);
                qry.OpenReadOnly();

                if (qry.HasData() && qry.FieldExist(flag))
                {
                    config = qry.FieldByName(flag);
                }
            }

            return config;
        }

        /// <summary>
        /// Check if RDB config is set to T
        /// </summary>
        /// <param name="functionName"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public bool CheckRDBConfig(string functionName, string flag)
        {
            string config = GetRDBConfig(functionName, flag);
            return config.Trim().ToUpper() == "T";
        }

        /// <summary>
        /// Get the application mode of the ocx module call.
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns>"DIRECT" or "REMOTE"</returns>
        public string GetOCXApplicationMode(string functionName)
        {
            string appMode = "DIRECT";
            bool hasRDBConfig = HasRDBConfig();
            if (PLCSession.GetLabCtrlFlag("USES_RDB") == "T"
                && (!hasRDBConfig
                || PLCSession.CheckRDBConfig(functionName, "USES_RDB")))
            {
                appMode = "REMOTE";
            }

            return appMode;
        }

        private bool HasRDBConfig()
        {
            string clientId = ClientID;

            if (!string.IsNullOrEmpty(clientId))
            {
                var qry = new PLCQuery();
                qry.SQL = "SELECT * FROM TV_RDBCONFIG WHERE CLIENT_ID = ?";
                qry.AddSQLParameter("CLIENT_ID", clientId);
                qry.OpenReadOnly();

                return qry.HasData();
            }

            return false;
        }
#endregion OCX

        #region ItemType
        /// <summary>
        /// Get item type flag value
        /// </summary>
        /// <param name="itemType"></param>
        /// <param name="flagName"></param>
        /// <returns></returns>
        public string GetItemTypeFlag(string itemType, string flagName)
        {
            var qry = new PLCQuery("SELECT * FROM TV_ITEMTYPE WHERE ITEM_TYPE = ?");
            qry.AddParameter("ITEM_TYPE", itemType);
            qry.OpenReadOnly();

            return qry.HasData() && qry.FieldExist(flagName)
                ? qry.FieldByName(flagName)
                : string.Empty;
        }

        /// <summary>
        /// Check if item type flag value is T
        /// </summary>
        /// <param name="itemType"></param>
        /// <param name="flagName"></param>
        /// <returns>returns true if flag value is T</returns>
        public bool CheckItemTypeFlag(string itemType, string flagName)
        {
            string flag = GetItemTypeFlag(itemType, flagName);
            return flag.Trim().ToUpper() == "T";
        }
        #endregion ItemType

        #region TaskType
        /// <summary>
        /// Get task type flag value
        /// </summary>
        /// <param name="taskType"></param>
        /// <param name="flagName"></param>
        /// <returns></returns>
        public string GetTaskTypeFlag(string taskType, string flagName)
        {
            var qry = new PLCQuery("SELECT * FROM TV_TASKTYPE WHERE TASK_TYPE = ?");
            qry.AddParameter("TASK_TYPE", taskType);
            qry.OpenReadOnly();

            return qry.HasData() && qry.FieldExist(flagName)
                ? qry.FieldByName(flagName)
                : string.Empty;
        }

        /// <summary>
        /// Check if task type flag value is T
        /// </summary>
        /// <param name="taskType"></param>
        /// <param name="flagName"></param>
        /// <returns>returns true if flag value is T</returns>
        public bool CheckTaskTypeFlag(string taskType, string flagName)
        {
            string flag = GetTaskTypeFlag(taskType, flagName);
            return flag.Trim().ToUpper() == "T";
        }
        #endregion TaskType

        #region Report Settings

        #region Label Format
        /// <summary>
        /// Get LABELFMT flag for a report
        /// </summary>
        /// <param name="reportName"></param>
        /// <param name="flagName"></param>
        /// <returns></returns>
        public string GetLabelFormatFlag(string reportName, string flagName)
        {
            string cacheKey = "LABELFMT_" + flagName + "_" + reportName;

            if (!CacheHelper.IsInCache(cacheKey))
            {
                var qry = new PLCQuery();
                qry.SQL = "SELECT * FROM TV_LABELFMT WHERE REPORT_NAME = ?";
                qry.AddSQLParameter("REPORT_NAME", reportName);
                qry.OpenReadOnly();

                string flagValue = string.Empty;
                if (qry.HasData() && qry.FieldExist(flagName))
                    flagValue = qry.FieldByName(flagName);

                CacheHelper.AddItem(cacheKey, flagValue);
            }

            return Convert.ToString(CacheHelper.GetItem(cacheKey));
        }

        /// <summary>
        /// Checks if LABELFMT flag of a report is set to T
        /// </summary>
        /// <param name="reportName"></param>
        /// <param name="flagName"></param>
        /// <returns></returns>
        public bool CheckLabelFormatFlag(string reportName, string flagName)
        {
            string flag = GetLabelFormatFlag(reportName, flagName);
            return flag.Trim().ToUpper() == "T";
        }
        #endregion Label Format

        #endregion Report Settings
    }
}