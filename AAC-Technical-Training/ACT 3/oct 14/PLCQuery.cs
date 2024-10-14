using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Data;
using System.Text.RegularExpressions;
using System.Data.OleDb;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Configuration;

namespace PLCCONTROLS
{  
    [ToolboxData("<{0}:PLCQuery runat=server></{0}:PLCQuery>")]

    public class PLCLock
    {
        private OleDbConnection dbConn;
        private OleDbTransaction dbTrans;
        
        public PLCLock() : base()
        {
          dbConn = null;
          dbTrans = null;
        }
        
        public OleDbConnection getconnection()
        {
            if (dbConn == null) 
            {
                PLCSessionVars sv = new PLCSessionVars();
                string connStr = sv.GetConnectionString();
                dbConn = new OleDbConnection(connStr);
                dbConn.Open();
                dbTrans = dbConn.BeginTransaction();
            }

            return dbConn;
        }
        
        public void releaseconnection()
        {
            if (dbConn != null) 
            {
                dbTrans.Commit();
                dbConn.Close();
                dbTrans.Dispose();
                dbConn.Dispose();
                dbTrans = null;
                dbConn = null;
            }            
        }
        
        public Boolean locktable(string tblname)
        {
            try
            {
                string LockSQL = "LOCK TABLE " + tblname + " IN EXCLUSIVE MODE NOWAIT";
                OleDbConnection myConn = getconnection();
                OleDbCommand cmd = new OleDbCommand(LockSQL, myConn);
                cmd.Transaction = dbTrans;
                cmd.ExecuteNonQuery();
                PLCSession.WriteDebug("Table Locked:" + tblname, true);
                return true;
            }
            catch (Exception e)
            {
                PLCSession.WriteDebug("Lock Failed:" + tblname + e.Message, true);
                return false;
            }
        }
        
        public Boolean releasetable()
        {
            try
            {
                releaseconnection();
                PLCSession.WriteDebug("Lock released", true);
            }
            catch (Exception e)
            {
                PLCSession.WriteDebug("Lock release failed:" + e.Message, true);
            }
            return true;
        }

        public Boolean LocktableMsOrcl(string tblname)
        {
            try
            {
                if (PLCSession.PLCDatabaseServer == "ORACLE")
                {
                    string LockSQL = "LOCK TABLE PLCLOCK IN EXCLUSIVE MODE NOWAIT";
                    OleDbConnection myConn = getconnection();
                    OleDbCommand cmd = new OleDbCommand(LockSQL, myConn);
                    cmd.Transaction = dbTrans;
                    cmd.ExecuteNonQuery();
                    PLCSession.WriteDebug("Table Locked:" + tblname, true);
                    return true;
                }
                else
                {
                    return AcquireLockMssql(tblname);
                }
            }
            catch (Exception e)
            {
                PLCSession.WriteDebug("Lock Failed:" + tblname + e.Message, true);
                return false;
            }
        }

        public Boolean ReleaseTableMsOrcl(string tblName = "")
        {
            try
            {
                if (PLCSession.PLCDatabaseServer == "ORACLE")
                    releaseconnection();
                else
                    ReleaseLockMssql(tblName);

                PLCSession.WriteDebug("Lock released", true);
            }
            catch (Exception e)
            {
                PLCSession.WriteDebug("Lock release failed:" + e.Message, true);
            }
            return true;
        }

        /// <summary>
        /// MSSQL Set Lock based on resource name
        /// </summary>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public bool AcquireLockMssql(string resourceName)
        {
            resourceName = resourceName + "_APP";
            PLCQuery qryLock = new PLCQuery();
            qryLock.SQL = "SELECT request_session_id AS SESSION_ID " +
                          "FROM sys.dm_tran_locks " +
                          "WHERE  resource_type = 'APPLICATION' " +
                          "AND request_mode = 'X' " +
                          "AND resource_description LIKE '%" + resourceName + "%'";
            qryLock.Open();

            if (qryLock.HasData())
            {
                PLCSession.WriteDebug("Resource Locked:" + resourceName, true);
                return false;
            }

            string connectionString = PLCSession.GetConnectionString();

            dbConn = new OleDbConnection(connectionString);

            dbConn.Open();
            using (var cmd = new OleDbCommand())
            {
                cmd.Connection = dbConn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_getapplock";

                var parameter = new OleDbParameter("Resource", resourceName);
                cmd.Parameters.Add(parameter);

                parameter = new OleDbParameter("LockMode", "Exclusive");
                cmd.Parameters.Add(parameter);

                parameter = new OleDbParameter("LockOwner", "Session");
                cmd.Parameters.Add(parameter);

                cmd.CommandTimeout = 1;

                try
                {
                    cmd.ExecuteNonQuery();
                    PLCSession.WriteDebug("Resource Locked:" + resourceName, true);
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// MSSQL Release recource locking from AcquireLockMssql
        /// </summary>
        /// <param name="resourceName"></param>
        private void ReleaseLockMssql(string resourceName)
        {
            resourceName = resourceName + "_APP";
            using (var cmd = new OleDbCommand())
            {
                cmd.Connection = dbConn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_releaseapplock";

                var parameter = new OleDbParameter("Resource", resourceName);
                cmd.Parameters.Add(parameter);

                parameter = new OleDbParameter("LockOwner", "Session");
                cmd.Parameters.Add(parameter);

                try
                {
                    cmd.ExecuteNonQuery();

                    if (dbConn != null)
                    {
                        dbConn.Close();
                        dbConn.Dispose();
                        dbConn = null;
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }
    }

    public class PLCQuery
    {
        private Boolean _isOpen = false;
        string sql = null;
        DataTable plcDataTable = null;
        DataRow plcNewRow = null;
        DataRow plcEditRow = null;
        bool useQuotedFieldNames = false;
        bool editTrackLogs = false;
        bool canPost = false;
        int currentRow = 0;
        string dbnameOverride = null;
        string datasourceOverride = null;
        string connectionStringOverride = null;

        bool isEmptyFieldsCachingActive = true;
        bool writeToAuditLog = true;
        bool bypassSQLValidation = false;
        bool excludeFromTransaction = false;
        bool redirectOnError = true;

        public PLCQuery() : base()
        {
        }
        
        public PLCQuery(string sql)
        {
            this.SQL = sql;
        }

        public PLCQuery(string sql, string dbnameOverride, string datasourceOverride)
        {
            this.SQL = sql;

            if (dbnameOverride != null)
                dbnameOverride = dbnameOverride.Trim();
            if (datasourceOverride != null)
                datasourceOverride = datasourceOverride.Trim();

            this.DBNameOverride = dbnameOverride;
            this.DataSourceOverride = datasourceOverride;
        }

        public PLCQuery(string sql, string connectionStringOverride)
        {
            this.SQL = sql;

            if (connectionStringOverride != null)
                connectionStringOverride = connectionStringOverride.Trim();

            this.ConnectionStringOverride = connectionStringOverride;
        }

        // Set this property to override the database name to be used in the PLCQuery connection string.
        public string DBNameOverride
        {
            get {return this.dbnameOverride;}
            set {this.dbnameOverride = value;}
        }

        // Set this property to override the datasource name to be used in the PLCQuery connection string.
        public string DataSourceOverride
        {
            get { return this.datasourceOverride; }
            set { this.datasourceOverride = value; }
        }

        // Set this property to override the entire connection string to be used by PLCQuery.
        public string ConnectionStringOverride
        {
            get { return this.connectionStringOverride; }
            set { this.connectionStringOverride = value; }
        }

        public virtual string SQL
        {
            get 
            {
                return (this.sql == null) ? String.Empty : this.sql;
            }

            set
            {
                string s = value;
                s = s.Replace("[", "\"");
                s = s.Replace("]", "\"");
                this.sql = s;
            }
        }

        public virtual DataTable PLCDataTable
        {
            get {

                if ((this.plcDataTable == null) && _isOpen) getSchemaOnly();

                return this.plcDataTable;


            }
            set { this.plcDataTable = value; }
        }

        public virtual DataRow PLCNewRow
        {
            get { return this.plcNewRow; }
            set { this.plcNewRow = value; }
        }

        public virtual DataRow PLCEditRow
        {
            get { return this.plcEditRow; }
            set { this.plcEditRow = value; }
        }

        public virtual bool UseQuotedFieldNames
        {
            get { return this.useQuotedFieldNames; }
            set { this.useQuotedFieldNames = value; }
        }

        public virtual bool WriteToAuditLog
        {
            get { return this.writeToAuditLog; }
            set { this.writeToAuditLog = value; }
        }

        public virtual bool EditTrackLogs
        {
            get { return this.editTrackLogs; }
            set { this.editTrackLogs = value; }
        }

        public bool ExcludeFromTransaction
        {
            get { return this.excludeFromTransaction; }
            set { this.excludeFromTransaction = true; }
        }

        public bool BypassSQLValidation
        {
            get { return this.bypassSQLValidation; }
            set { this.bypassSQLValidation = true; }
        }

        public bool RedirectOnError
        {
            get { return this.redirectOnError; }
            set { this.redirectOnError = value; }
        }

        private List<OleDbParameter> _parameters = null;
        
        private string GetCurrentConnectionString()
        {
            if (!string.IsNullOrEmpty(ConnectionStringOverride))
                return ConnectionStringOverride;
            else
                return PLCSession.GetConnectionString(this.DBNameOverride, this.DataSourceOverride);
        }

        public void AddProcedureParameter(string parameterName, object parameterValue, int size, OleDbType type, ParameterDirection direction)
        {
            OleDbParameter parameter = new OleDbParameter();
            parameter.ParameterName = parameterName;
            parameter.Value = parameterValue;
            parameter.Size = size;
            parameter.OleDbType = type;
            parameter.Direction = direction;

            if (_parameters == null)
                _parameters = new List<OleDbParameter>();

            _parameters.Add(parameter);
        }

        public Dictionary<string, object> ExecuteProcedure(string procedureName)
        {
            return ExecuteProcedure_Main(procedureName, String.Empty);
        }

        public Dictionary<string, object> ExecuteProcedureCustom(string procedureName, string customConnectionString)
        {
            return ExecuteProcedure_Main(procedureName, customConnectionString);
        }

        private Dictionary<string, object> ExecuteProcedure_Main(string procedureName, string customConnectionString)
        {
            PLCSessionVars PLCSession = new PLCSessionVars();
            PLCSession.WriteDebug("Execute Procedure:" + procedureName, true);
            string connectionString = GetCurrentConnectionString();

            if (!String.IsNullOrEmpty(customConnectionString))
                connectionString = customConnectionString;

            PLCSession.WriteDebug("Execute Procedure (1)", true);
            using (OleDbConnection dbConnection = new OleDbConnection(connectionString))
            {
                dbConnection.Open();
                PLCSession.WriteDebug("Execute Procedure (2)", true);
                OleDbCommand dbCommand = GetDBCommand(procedureName, dbConnection);
                dbCommand.CommandType = CommandType.StoredProcedure;

                foreach (OleDbParameter parameter in _parameters)
                {
                    PLCSession.WriteDebug("Add Parameter" + parameter.Value.ToString(), true);
                    dbCommand.Parameters.Add(parameter);
                }

                PLCSession.WriteDebug("Execute Procedure (3)", true);
                try
                {
                    dbCommand.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    PLCSession.WriteDebug("Execute Procedure (error) :" + e.Message.ToString(), true);
                    throw e;
                }
                finally
                {
                    PLCSession.WriteDebug("Execute Procedure (4)", true);
                    dbConnection.Close();
                }
            }

            Dictionary<string, object> outParameters = (from p in _parameters
                                                        where p.Direction != ParameterDirection.Input
                                                        select p).ToDictionary(p => p.ParameterName, p => p.Value);
            
            PLCSession.WriteDebug("Execute Procedure (5)", true);

            return outParameters;
        }

        public Dictionary<string, object> ExecuteOracleProcedure(string procedureName, List<OracleParameter> parameters)
        {
            return ExecuteOracleProcedure_Main(procedureName, parameters, String.Empty);
        }

        public Dictionary<string, object> ExecuteOracleProcedureCustom(string procedureName, List<OracleParameter> parameters, string customConnectionString)
        {
            return ExecuteOracleProcedure_Main(procedureName, parameters, customConnectionString);
        }

        private Dictionary<string, object> ExecuteOracleProcedure_Main(string procedureName, List<OracleParameter> parameters, string customConnectionString)
        {
            Dictionary<string, object> outParameters;

            //$$ Disable the System.Data.OracleClient warnings until we get the Oracle dll to use sorted out.
            #pragma warning disable 618
            OracleConnection dbConn = 
                new OracleConnection(
                    string.Format("Data Source={0}; User ID={1}; Password={2};", PLCSession.PLCDBDataSource, PLCSession.PLCDBUserID, PLCSession.PLCDBPW));

            if (!String.IsNullOrEmpty(customConnectionString))
                dbConn = new OracleConnection(customConnectionString);

            using (dbConn)
            {
                dbConn.Open();

                OracleCommand dbComm = new OracleCommand(procedureName, dbConn);
                dbComm.CommandType = CommandType.StoredProcedure;

                foreach (OracleParameter param in parameters)
                {
                    dbComm.Parameters.Add(param);
                }

                try
                {
                    dbComm.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    PLCSession.WriteDebug("Execute Procedure (error) :" + e.Message.ToString(), true);
                    throw e;
                }
                finally
                {                    
                    dbConn.Close();
                }

                outParameters = (from p in parameters
                                where p.Direction != ParameterDirection.Input
                                select p).ToDictionary(p => p.ParameterName, p => p.Value);
            }
            #pragma warning restore 618

            return outParameters;
        }

        public DataTable ExecuteMSSQLProcedureQuery(string procedureName, List<SqlParameter> parameters)
        {
            return ExecuteMSSQLProcedureQuery_Main(procedureName, parameters, String.Empty);
        }

        public DataTable ExecuteMSSQLProcedureQueryCustom(string procedureName, List<SqlParameter> parameters, string customConnectionString)
        {
            return ExecuteMSSQLProcedureQuery_Main(procedureName, parameters, customConnectionString);
        }

        private DataTable ExecuteMSSQLProcedureQuery_Main(string procedureName, List<SqlParameter> parameters, string customConnectionString)
        {
            PLCSessionVars PLCSession = new PLCSessionVars();
            PLCSession.WriteDebug("Execute Procedure:" + procedureName, true);
            
            PLCSession.WriteDebug("Execute Procedure (1)", true);
            SqlConnection dbConnection = new SqlConnection(string.Format("Data Source={0}; User ID={1}; Password={2}; Initial Catalog={3};", 
                PLCSession.PLCDBDataSource, PLCSession.PLCDBUserID, PLCSession.PLCDBPW, PLCSession.PLCDBDatabase));

            if (!String.IsNullOrEmpty(customConnectionString))
                dbConnection = new SqlConnection(customConnectionString);
            
            PLCSession.WriteDebug("Execute Procedure (2)", true);
            SqlCommand dbCommand = new SqlCommand(procedureName, dbConnection);
            dbCommand.CommandType = CommandType.StoredProcedure;

            foreach (SqlParameter parameter in parameters)
            {
                PLCSession.WriteDebug("Add Parameter" + parameter.Value.ToString(), true);
                dbCommand.Parameters.Add(parameter);
            }

            DataTable data = new DataTable();

            PLCSession.WriteDebug("Execute Procedure (3)", true);
            try
            {
                dbConnection.Open();
                //dbCommand.ExecuteNonQuery();
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = dbCommand;
                adapter.Fill(data);
                dbConnection.Close();
            }
            catch (Exception e)
            {
                PLCSession.WriteDebug("Execute Procedure (error) :" + e.Message.ToString(), true);
                throw e;
            }
            finally
            {
                PLCSession.WriteDebug("Execute Procedure (4)", true);
                if (dbConnection.State == ConnectionState.Open)
                    dbConnection.Close();
            }

            PLCSession.WriteDebug("Execute Procedure (5)", true);

            return data;
        }

        public void AddParameter(string parameterName, object parameterValue)
        {
            var type = OleDbType.Empty;
            if (PLCDataTable != null) //If AddParameter() is used for Append() or Edit(), PLCDataTable should not be null (after Open() is called).
            {
                DataColumn dc = PLCDataTable.Columns[parameterName];
                if (dc == null)
                {
                    PLCSession.WriteDebug("Exception in AddParameter: QRY=" + sql,true);
                    throwexception("AddParameter", "Field [" + parameterName + "] is not found");
                }

                if (parameterValue != null && parameterValue != DBNull.Value)
                {
                    string parameterDataType = dc.DataType.ToString().ToUpper();
                    switch (parameterDataType)
                    {
                        case "SYSTEM.DATETIME":
                            parameterValue = ConvertToDBDateTime(parameterName, parameterValue);
                            type = OleDbType.Date;
                            break;
                        case "SYSTEM.INT16":
                        case "SYSTEM.INT32":
                        case "SYSTEM.DECIMAL":
                        case "SYSTEM.DOUBLE":
                            parameterValue = GetParseValue(parameterValue, parameterDataType);
                            break;
                    }
                }
            }

            if (parameterValue == null)
                parameterValue = DBNull.Value;
            
            if (!IsEmpty()) //for Edit(), to build UpdateSQL string
            {
                DataRow _dr = GetCurrentRow();
                _dr[parameterName] = parameterValue;
            }

            OleDbParameter parameter = new OleDbParameter();
            parameter.ParameterName = parameterName;
            parameter.Value = parameterValue;

            if (type != OleDbType.Empty)
                parameter.OleDbType = type;

            // Set default size to 5. FillSchema() requires the parameters to be explicitly defined and parameter size should not be zero.
            if (parameter.OleDbType == OleDbType.VarWChar && parameter.Size == 0)
                parameter.Size = 5;

            if (_parameters == null)
                _parameters = new List<OleDbParameter>();

            if (_parameters.SingleOrDefault(a => a.ParameterName == parameterName) != null)
                _parameters.SingleOrDefault(a => a.ParameterName == parameterName).Value = parameterValue;
            else
                _parameters.Add(parameter);
        }

        public bool CanPost()
        {
            // this.canPost only applies to Oracle database (as it needs ROWID). SQL Server can always post as it doesn't use ROWID.
            if ((new PLCSessionVars().PLCDatabaseServer == "ORACLE") && !this.canPost)
                return false;
            else
                return true;
        }

        public bool Save(string tableName)
        {
            return Save(tableName, -1, -1);
        }

        public bool Save(string tableName, int auditCode, int auditSubcode, bool LogToAuditCon=false)
        {
            if (!CanPost())
                return false;

            if (PLCNewRow != null)
            {
                string insertSQL = getInsertSQL(tableName);

                PLCSessionVars PLCSession = new PLCSessionVars();
                string connectionString = GetCurrentConnectionString();

                using (OleDbConnection dbConnection = new OleDbConnection(connectionString))
                {
                    dbConnection.Open();

                    OleDbCommand dbCommand = GetDBCommand();
                    dbCommand.CommandType = CommandType.Text;
                    dbCommand.Connection = dbConnection;
                    dbCommand.CommandText = "INSERT INTO " + tableName + " (" + string.Join(",", _parameters.Select(p => p.ParameterName).ToArray()) + ") " +
                        "VALUES (" + string.Join(",", Enumerable.Repeat("?", _parameters.Count).ToArray()) + ")";

                    foreach (OleDbParameter parameter in _parameters)
                    {
                        dbCommand.Parameters.Add(parameter);
                    }

                    int rowsAdded = dbCommand.ExecuteNonQuery();

                    dbConnection.Close();

                    if (auditCode >= 0)
                    {
                        LogSQLUpdate(tableName, insertSQL, auditCode, auditSubcode);
                        WriteAuditlog(tableName, auditCode, auditSubcode);

                        if (LogToAuditCon)
                            WriteAuditCon(tableName, auditCode, auditSubcode, 0);
                    }

                    PLCNewRow = (DataRow)null;

                    if (rowsAdded == 1)
                        return true;
                    else
                        return false;
                }
            }
            else if (PLCEditRow != null)
            {
                string updateSQL = getUpdateSQL(tableName);
                if (updateSQL == "")
                    return false;

                string filter = "";
                if (updateSQL.ToUpper().IndexOf(" WHERE ") > -1)
                    filter = updateSQL.Substring(updateSQL.ToUpper().IndexOf(" WHERE "));

                PLCSessionVars PLCSession = new PLCSessionVars();
                string connectionString = GetCurrentConnectionString();

                using (OleDbConnection dbConnection = new OleDbConnection(connectionString))
                {
                    dbConnection.Open();

                    OleDbCommand dbCommand = GetDBCommand();
                    dbCommand.CommandType = CommandType.Text;
                    dbCommand.Connection = dbConnection;
                    dbCommand.CommandText = "UPDATE " + tableName + " SET " + string.Join(",", _parameters.Select(p => p.ParameterName + " = ?").ToArray()) + filter;

                    foreach (OleDbParameter parameter in _parameters)
                    {
                        dbCommand.Parameters.Add(parameter);
                    }

                    int rowsUpdated = dbCommand.ExecuteNonQuery();

                    dbConnection.Close();

                    if (auditCode >= 0)
                    {
                        LogSQLUpdate(tableName, updateSQL, auditCode, auditSubcode);
                        WriteAuditlog(tableName, auditCode, auditSubcode);

                        if (LogToAuditCon)
                            WriteAuditCon(tableName, auditCode, auditSubcode, 0);
                    }

                    PLCEditRow = (DataRow)null;

                    if (rowsUpdated > 0)
                        return true;
                    else
                        return false;
                }
            }

            return false;
        }

        private void throwexception(string fromMethod, string messageStr)
        {
            throwexception(fromMethod, messageStr, "","");
        }

        private void throwexception(string fromMethod, string messageStr, string exMessage)
        {
            throwexception(fromMethod, messageStr, exMessage, "");
        }

        public void throwexception(string fromMethod, string messageStr, string exMessage, string exSQL)
        {
            PLCSessionVars PLCSessionVars1 = new PLCSessionVars();
            PLCSessionVars1.ClearError();
            PLCSessionVars1.PLCErrorURL = "*";
            PLCSessionVars1.PLCErrorProc = "PLCQuery." + fromMethod;
            PLCSessionVars1.PLCErrorSQL = this.SQL;
            PLCSessionVars1.PLCErrorMessage = messageStr + " - Exception: " + exMessage;
            PLCSessionVars1.SaveError();
            //PLCSessionVars1.Redirect("CASEFILE_ERROR.ASPX");
            // AAC: must be called from PLCWebCommon since this is now a common file
            if (redirectOnError) PLCSessionVars1.Redirect("~/PLCWebCommon/Error.aspx");

            string myError = System.Environment.NewLine + "From: PLCQUERY." + fromMethod;
            myError += System.Environment.NewLine + "AppMsg: " + messageStr;
            myError += System.Environment.NewLine + "SQL: " + this.SQL;
            if (exMessage != "")
            myError += System.Environment.NewLine + "ExMsg: " + exMessage;

            if (exSQL != "")
                myError += System.Environment.NewLine + "ERROR SQL: " + exSQL;

            if (isTransaction) Rollback();

            Exception myException = new Exception(myError);
            throw myException;
        }
        
        public Boolean Open()
        {
            ValidateSql();
            return this.Open("");
        }

        // Open() without a primary key. Use this in Oracle to omit ROWID field from datatable.
        public Boolean OpenReadOnly()
        {
            this.canPost = false;
            bool retOpen = Open("", false);
            RemovePrimaryKeys();
            return retOpen;
        }

        public void ValidateSql()
        {
            string input = this.SQL;

            if (!bypassSQLValidation)
            {
                // Add \sIN\s| between ?: and \sLIKE\s to include regex capture
                string regex = @"(?:\sLIKE\s|\sIS\s|=|>=|<=|[<>]|<>)\s*.*?(?=\(|\)|THEN|OR|AND|LEFT|RIGHT|OUTER|INNER|JOIN|GROUP|WHERE|$|\Z)";
                RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline;

                MatchCollection matches = Regex.Matches(input, regex, options);
                foreach (Match match in matches)
                {
                    string original = null;
                    string replacement = null;

                    if (match.Value.Contains("',"))
                    {
                        // TODO: Add handling for values handled by IN
                    }
                    else if (match.Value.Contains("'"))
                    {
                        int startParam = match.Value.Trim().IndexOf('\'');
                        int paramLength = match.Value.Trim().Length - (startParam + 2);

                        if (paramLength > 0)
                        {
                            original = match.Value.Trim().Substring(startParam + 1, paramLength);
                            replacement = original;
                            replacement = replacement.Replace("'", "''");

                            input = input.Replace(original, replacement);
                        }
                    }
                    else
                    {
                        // If no apostrophe, do not handle
                        continue;
                    }

                }
                this.SQL = input;
            }
        }

        public Boolean EOF()
        {


            if (!_isOpen) return true;
            DataTable myDataTable = PLCDataTable;
           
            if (myDataTable == null) return true;
            if (myDataTable.Rows.Count < 1) return true;

            int currentrow = this.currentRow;
            
            return ( currentrow  > (myDataTable.Rows.Count -1) );

        }

        public Boolean BOF()
        {
            if (!_isOpen) return true;

            DataTable myDataTable = PLCDataTable;

            if (myDataTable == null) return true;
            if (myDataTable.Rows.Count < 1) return true;

            int currentrow = this.currentRow;

            return (currentrow < 0);

        }

        public Boolean First()
        {
            if (!_isOpen) return true;
            DataTable myDataTable = PLCDataTable;

            if (myDataTable == null) return false;
            if (myDataTable.Rows.Count < 1) return false;

            this.currentRow = 0;

            return true;

        }
        
        public Boolean Last()
        {
            if (!_isOpen) return true;
            DataTable myDataTable = PLCDataTable;

            if (myDataTable == null) return false;
            if (myDataTable.Rows.Count < 1) return false;

            int currentrow = myDataTable.Rows.Count - 1 ;

            this.currentRow = currentrow;

            return true;

        }

        public Boolean IsEmpty()
        {
            if (!_isOpen) return true;
            DataTable myDataTable = PLCDataTable;
            if (myDataTable == null) return true;
            if (myDataTable.Rows.Count < 1) return true;
            return false;
        }

        public Boolean HasData()
        {
            if (!_isOpen) return false;

            if ( IsEmpty() )
                return false;
            else
                return true;
        }

        public Boolean Next()
        {
            
            if (!_isOpen) return false;

            if (this.EOF()) return false;

            int currentrow = this.currentRow;
            
            currentrow++;

            this.currentRow = currentrow;

            return true;
        }

        public Boolean Prev()
        {

            if (!_isOpen) return false;

            if (this.IsEmpty()) return false;

            if (this.BOF()) return false;

            int currentrow = this.currentRow;

            if (currentrow < 0) return false;

            currentrow--;

            this.currentRow = currentrow;

            return true;
        }

        public Boolean Refresh()
        {
            return this.Open("");
        }
        
        public Boolean Refresh(string connStr)
        {
            return this.Open(connStr);
        }

        public Boolean Open(string connStr)
        {
            return Open(connStr, true);
        }

        private string getSchemaCacheTag(string qry)
        {
            // the tag is defined by the SQL statement before the Where Clause.
            // Only simple SQL statements with no sub-selects and UNIONS qualify
            qry = qry.ToUpper();
            int count = new Regex("SELECT ").Matches(qry).Count;
            if (count > 1) return "";
            count = new Regex("WHERE ").Matches(qry).Count;
            if (count > 1) return "";
            count = new Regex("UNION ").Matches(qry).Count;
            if (count > 0) return "";
            int psn = qry.IndexOf("WHERE");
            if (psn >= 0)
                return "_SCHEMA_CACHE_" + qry.Substring(0, psn);
            else
                return "_SCHEMA_CACHE_" + qry;
         }

        // Set withKey to true (most cases) to add a primary key. This is needed when iterating and editing rows through Edit() / Post() / Next().
        public Boolean Open(string connStr, bool withKey)
        {
            _isOpen = false;

            if (PLCSession.SQLInjectionSuspected(this.sql))
            {
                PLCSession.ForceWriteDebug("Error 57 - SQL Injection Suspected",true);
                PLCSession.ForceWriteDebug(this.SQL,true);
                throw new System.ArgumentException("System Error 57", "See LOG for more info");              
              }

            this.canPost = withKey;
            this.currentRow = 0;

            DateTime starttime = DateTime.Now;
            TimeSpan elapsedtime;

            // Set to true to enable empty fields caching.
            this.isEmptyFieldsCachingActive = true;

            // Check first if this is an 'empty fields query'. Ex. SELECT * FROM tbl WHERE 0=1
            // If it is, and there's a cached empty fields resultset, restore it.
            string emptyFieldsTableName;
            bool isEmptyFieldsSql = IsEmptyFieldsSql(this.SQL, out emptyFieldsTableName);
            if (isEmptyFieldsSql)
            {
                if (RestoreEmptyFieldsResultSet(emptyFieldsTableName))
                {
                    elapsedtime = DateTime.Now.Subtract(starttime);
                    LogSQLUpdate("OPEN:", "RESTORE TIME: " + elapsedtime.TotalMilliseconds.ToString(), 0, 0);
                    _isOpen = true;
                    return true;
                }
            }

            this.PLCDataTable = null;
            DataTable _dt = new DataTable("TBL");
            _dt.Clear();

            this.SQL = PLCSession.FixedSQLStr(this.SQL);

            using (OleDbConnection dbconn = PLCSession.GetDBConn(GetCurrentConnectionString()))
            {
                elapsedtime = DateTime.Now.Subtract(starttime);
                LogSQLUpdate("OPEN:", "CONNECT TIME: " + elapsedtime.TotalMilliseconds.ToString(), 0, 0);

                OleDbDataAdapter da = new OleDbDataAdapter(this.SQL, dbconn);
                LogSQLUpdate("OPEN:", this.SQL, 0, 0);

                da.SelectCommand = GetDBCommand(this.SQL, dbconn);
                // Pass any parameters.
                if (this._parameters != null)
                {
                    da.SelectCommand = GetDBCommand(this.SQL, dbconn);
                    foreach (OleDbParameter parameter in _parameters)
                        da.SelectCommand.Parameters.Add(parameter);
                }

                if (withKey)
                    da.MissingSchemaAction = MissingSchemaAction.AddWithKey; //Live Query

                try
                {
                    da.Fill(_dt);
                }
                catch (Exception e)
                {
                    try
                    {
                        da = new OleDbDataAdapter(this.SQL, dbconn);
                        if (!redirectOnError) throw e;
                    }
                    catch (Exception f)
                    {
                        dbconn.Close();
                        throwexception("Open", "Cannot fill data table(1)", e.Message + "/" + f.Message);
                    }
                }

                elapsedtime = DateTime.Now.Subtract(starttime);
                LogSQLUpdate("OPEN:", "QUERY TIME: " + elapsedtime.TotalMilliseconds.ToString(), 0, 0);

                if (ConfigurationManager.AppSettings.AllKeys.Contains("PLCQUERY_EXEC_TIME"))
                {
                    double execTime = 0;
                    Double.TryParse(ConfigurationManager.AppSettings["PLCQUERY_EXEC_TIME"], out execTime);
                    execTime = Math.Round(execTime);

                    if (execTime > 0 && elapsedtime.TotalMilliseconds > execTime)
                    {
                        PLCSession.WriteNotificationLog(this.SQL);
                        PLCSession.WriteNotificationLog("QUERY TIME: " + elapsedtime.TotalMilliseconds.ToString());
                    }
                }

                if (_dt.Rows.Count > 0)
                {
                    this.PLCDataTable = _dt;

                    // Cache this result set if it's an empty fields query.
                    if (isEmptyFieldsSql)
                        CacheEmptyFieldsResultSet(emptyFieldsTableName);
                    _isOpen = true;
                    return true;
                }
                else
                {
                    if (!canPost)
                    {
                        this.PLCDataTable = _dt;

                        // Cache this result set if it's an empty fields query.
                        if (isEmptyFieldsSql)
                            CacheEmptyFieldsResultSet(emptyFieldsTableName);
                        _isOpen = true;
                        return true;
                    }

                    try
                    {
                        // Clear any parameters.
                        if (da.SelectCommand != null)
                            da.SelectCommand.Parameters.Clear();

                        string schematag = getSchemaCacheTag(SQL);
                        if (schematag != "")
                        {
                            if (RestoreEmptyFieldsResultSet(schematag))
                            {
                                elapsedtime = DateTime.Now.Subtract(starttime);
                                LogSQLUpdate("OPEN:", "RESTORE TIME: " + elapsedtime.TotalMilliseconds.ToString(), 0, 0);
                                return true;
                            }
                        }
                        
                        this.PLCDataTable = null;
                        _isOpen = true;
                        return true;
                    }
                    catch (Exception e)
                    {
                        throwexception("Open", "Cannot fill data table(2)", e.Message);
                    }
                }

                try
                {
                    _isOpen = true;

                    if (_dt.Columns.Count > 0)
                        return true;
                    else
                        return false;
                }
                catch
                {
                    _isOpen = false;
                    return false;
                }
            }
        }

        // Return whether a sql string corresponds to an 'empty fields query' such as 
        // SELECT * FROM tbl WHERE 0=1.
        // Possible matches are:
        //   SELECT * FROM tbl WHERE 0=1
        //   select * FROM tbl WHERE 0 = 1
        //     SELECT * FROM tbl WHERE 1=0
        //   SELECT * from tbl WHERE 1=0
        //   SELECT * FROM tbl WHERE 1= 0
        //   select * from tbl WHERE 0 =0
        //
        // If sql string is an 'empty fields query', return the table name in the output parameter.
        private bool IsEmptyFieldsSql(string sql, out string tableName)
        {
            if (!this.isEmptyFieldsCachingActive)
            {
                tableName = "";
                return false;
            }

            string pattern = @"^\s*SELECT\s+\*\s+FROM\s+(\w+)\s+WHERE\s+(1\s*=\s*0|0\s*=\s*1)\s*$";

            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            Match match = regex.Match(sql);

            // Sql string is an 'empty fields query'
            if (match.Success)
            {
                tableName = match.Groups[1].Value;
                return true;
            }
            else
            {
                // No match, some other sql query string.
                tableName = "";
                return false;
            }
        }

        private string GetEmptyFieldsCacheKey(string tableName)
        {
            return CacheHelper.GetCacheKey("__EMPTYFIELDS__" + tableName);
        }

        // Store a copy of empty fields resultset (datatable) so that it can be retrieved later.
        private void CacheEmptyFieldsResultSet(string tableName)
        {
            string key = GetEmptyFieldsCacheKey(tableName);
            CacheHelper.AddItem(key, this.PLCDataTable.Copy());
        }

        // Retrieve the cached empty fields resultset for use in the query.
        // Return whether or not the empty fields resultset was restored.
        private bool RestoreEmptyFieldsResultSet(string tableName)
        {
            if (!this.isEmptyFieldsCachingActive)
                return false;

            string key = GetEmptyFieldsCacheKey(tableName);
            
            if (CacheHelper.IsInCache(key))
            {
                DataTable cachedDataTable = (DataTable) CacheHelper.GetItem(key);
                this.PLCDataTable = cachedDataTable.Copy();
                return true;
            }
            else
            {
                return false;
            }
        }

        public Boolean getSchemaOnly()
        {
            if (!_isOpen) return false;
            if (this.plcDataTable != null) return true;

            this.canPost = true;
            this.currentRow = 0;

            DateTime starttime = DateTime.Now;

            // Set to true to enable empty fields caching.
            this.isEmptyFieldsCachingActive = true;

            // Check first if this is an 'empty fields query'. Ex. SELECT * FROM tbl WHERE 0=1
            // If it is, and there's a cached empty fields resultset, restore it.
            string emptyFieldsTableName;
            bool isEmptyFieldsSql = IsEmptyFieldsSql(this.SQL, out emptyFieldsTableName);
            if (isEmptyFieldsSql)
            {
                if (RestoreEmptyFieldsResultSet(emptyFieldsTableName))
                {
                    TimeSpan elapsedtimeCached = DateTime.Now.Subtract(starttime);
                    LogSQLUpdate("getSchemaOnly", "getSchemaOnly Restored: " + elapsedtimeCached.ToString(), 0, 0);
                    return true;
                }
            }
            
            TimeSpan tempspan;         
           
            using (OleDbConnection dbconn = PLCSession.GetDBConn(GetCurrentConnectionString()))
            {                
                OleDbDataAdapter da = new OleDbDataAdapter(this.SQL, dbconn);
                tempspan = DateTime.Now.Subtract(starttime);
                DataTable _dt = new DataTable("TBL");
                
                if (this._parameters != null)
                {
                    da.SelectCommand = GetDBCommand(this.SQL, dbconn);
                    foreach (OleDbParameter parameter in _parameters)
                        da.SelectCommand.Parameters.Add(parameter.ParameterName, parameter.OleDbType, parameter.Size);                       
                }

                da.MissingSchemaAction = MissingSchemaAction.AddWithKey; //Live Query
                da.FillSchema(_dt, SchemaType.Source);
                this.plcDataTable = _dt;
            }

            LogSQLUpdate("getSchemaOnly", "getSchemaOnly retrieved: " + tempspan.ToString(), 0, 0);
            return true;
        }

        /// <summary>
        /// Get OleDB Schema
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public DataTable GetOleDBSchema(string tableName)
        {
            DataTable dtSchema = null;
            using (OleDbConnection dbconn = PLCSession.GetDBConn(GetCurrentConnectionString()))
            {
                dtSchema = dbconn.GetOleDbSchemaTable(OleDbSchemaGuid.Columns,
                    new object[] { null, null, tableName, null });
            }

            return dtSchema;
        }

        /// <summary>
        /// Get table schema
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public DataTable GetDBSchema(string tableName)
        {
            if (PLCSession.PLCDatabaseServer == "MSSQL")
            {
                var qrySchema = new PLCQuery();
                qrySchema.SQL = "SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS "
                    + "WHERE TABLE_NAME = ? ORDER BY ORDINAL_POSITION";
                qrySchema.AddSQLParameter("TABLE_NAME", tableName);
                qrySchema.Open();
                if (qrySchema.HasData())
                {
                    return qrySchema.PLCDataTable;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the data type from schema
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        public string GetDataTypeFromSchema(string fieldName, DataTable schema)
        {
            if (schema == null)
                return string.Empty;

            var rows = schema.Select("COLUMN_NAME = '" + fieldName + "'");
            if (rows.Length == 0)
                return string.Empty;

            return rows[0]["DATA_TYPE"].ToString().ToUpper();
        }

        public Boolean ExecSQL()
        {
            return ExecSQL(GetCurrentConnectionString());
        }

        public Boolean ExecSQL(string connStr, bool disableThrowExeption = false)
        {

            if (connStr == "")
            {
                connStr = GetCurrentConnectionString();
            }

            using (OleDbConnection dbconn = new OleDbConnection(connStr))
            {

                try
                {
                    dbconn.Open();
                }
                catch (Exception e)
                {
                    if (!disableThrowExeption) throwexception("Open", "Cannot Open Database Connection", e.Message);
                    return false;
                }

                OleDbCommand dc = GetDBCommand(this.SQL, dbconn);
                try
                {
                    if (!isTransaction || this.excludeFromTransaction)
                    {
                        if (this._parameters != null)
                        {
                            foreach (OleDbParameter param in this._parameters)
                                dc.Parameters.Add(param);
                        }
                        dc.ExecuteNonQuery();
                        LogSQLUpdate("execsql", this.SQL, 0, 0);
                    }
                    else
                        CurrentTransactSQL.Add(new TransactSQL { SQL = dc.CommandText, Parameters = this._parameters });
                    
                    return true;
                }

                catch (Exception e)
                {
                    if (!disableThrowExeption) throwexception("ExecSQL", "Cannot ExecuteQuery", e.Message);
                    return false;
                }


            }

        }

        public void ClearParameters()
        {
            if (this._parameters != null)
                this._parameters.Clear();
        }

        private DataRow NewRow()
        {
            DataRow _dr = PLCDataTable.NewRow();
            _dr = PLCDataTable.NewRow();
            int idx = PLCDataTable.Rows.IndexOf(_dr);
            this.currentRow = -1;
            return _dr;
        }

        public Boolean Append()
        {
            ClearParameters();

            getSchemaOnly();

            PLCNewRow = NewRow();
            return true;

        }

        public Boolean Edit()
        {
            ClearParameters();

            DataRow _dr;
            if (!this.EOF())
            {
                int currentrow = this.currentRow;
                _dr = PLCDataTable.Rows[currentrow];
            }
            else if (isTransaction)
                _dr = NewRow();
            else
                return false;

            PLCEditRow = _dr;
            return true;
        }

        //public Boolean Append()
        //{
        //    ClearParameters();
            
        //    DataRow _dr = PLCDataTable.NewRow();
        //    _dr = PLCDataTable.NewRow();
        //    int idx = PLCDataTable.Rows.IndexOf(_dr);
        //    this.currentRow = -1;
        //    PLCNewRow = _dr;
        //    return true;

        //}

        //public Boolean Edit()
        //{
        //    ClearParameters();

        //    if (this.EOF()) return false;
        //    int currentrow = this.currentRow;
        //    DataRow _dr = PLCDataTable.Rows[currentrow];
        //    PLCEditRow = _dr;
        //    return true;
        //}

        private string inDoubleQuotes(string s)
        {

            return "\"" + s + "\"";
        }

        private string inSingleQuotes(string s)
        {

            return "'" + s + "'";
        }

        private string DateFieldFormat(string s)
        {

            return "to_date('" + s + "','DD-MON-YYYY HH24:MI:SS')";
        }

        private string getInsertSQL(string tblname)
        {

          
            string fldName = ""; 
            string fldDataCurrent = "";
            string fldDataOriginal = "";
            string fldDataProposed = "";
            string colstr = "";
            string datastr = "";

            DataRow _dr = GetCurrentRow();
            
            DateTime dt = new DateTime();
           
            foreach (DataColumn dc in PLCDataTable.Columns)
            {
                PLCSessionVars sv = new PLCSessionVars();
                fldName = dc.ColumnName;

                //For Oracle
                if (fldName == "ROWID") continue;

                try
                {
                    fldDataCurrent = _dr[fldName, DataRowVersion.Current].ToString();
                }
                catch
                {
                    fldDataCurrent = "";
                }

                try
                {
                    fldDataOriginal = _dr[fldName, DataRowVersion.Original].ToString();
                }
                catch
                {
                    fldDataOriginal = "";
                }

                try
                {
                    fldDataProposed = _dr[fldName, DataRowVersion.Proposed].ToString();
                }
                catch
                {
                    fldDataProposed = "";
                }

                if (dc.DataType.ToString() == "System.DateTime")
                {
                    try
                    {
                        dt = Convert.ToDateTime(fldDataCurrent);
                        fldDataCurrent = dt.ToString("d-MMM-yyyy HH:mm:ss");
                    }
                    catch
                    {
                        fldDataCurrent = "";
                    }



                  try
                    {
                        dt = Convert.ToDateTime(fldDataOriginal);
                        fldDataOriginal = dt.ToString("d-MMM-yyyy HH:mm:ss");
                    }
                    catch
                    {
                        fldDataOriginal = "";
                    }


       
                  try
                    {
                        dt = Convert.ToDateTime(fldDataProposed);
                        fldDataProposed = dt.ToString("d-MMM-yyyy HH:mm:ss");
                    }
                    catch
                    {
                        fldDataProposed = "";
                    }
                }
           


                if (fldDataProposed != "")
                {

                    if (colstr != "") colstr += ",";
                    if (datastr != "") datastr += ",";
                    //colstr += inDoubleQuotes(fldName);
                    //*AAC*

                    if (UseQuotedFieldNames)
                        colstr += "\"" + fldName + "\"";
                    else
                        colstr += fldName;

                    if (dc.DataType.ToString() == "System.DateTime")
                    {
                        if (sv.PLCDatabaseServer == "MSSQL")
                            datastr += inSingleQuotes(fldDataProposed);
                        else
                            datastr += DateFieldFormat(fldDataProposed);
                    }
                    else
                        if (fldDataProposed != "null")
                        {
                            datastr += inSingleQuotes(fldDataProposed);
                        }
                        else
                        {
                            datastr += fldDataProposed;
                        }
                }
            }
          
            return "INSERT INTO " + tblname + "(" + colstr + ") Values (" + datastr + ")";
        }

        // Return with single quotes escaped. Ex. "Don't Panic" becomes "Don''t Panic".
        private string EscapeSingleQuotes(string str)
        {
            return str.Replace("'", "''");
        }

        // Get field value formatted for use in a sql statement. Ex. 'abc' for string values, 123 (without quotes) for numbers.
        private string GetSqlFormattedFieldValue(object fieldval)
        {
            if (fieldval is System.String)
            {
                // Ex. 'abc'
                return String.Format("'{0}'", EscapeSingleQuotes(fieldval.ToString()));
            }
            else if (fieldval is System.DateTime)
            {
                // Ex. SQL: '3/5/2010 12:00:00 AM'
                //     Oracle: to_date(...)
                if (new PLCSessionVars().PLCDatabaseServer == "MSSQL")
                    return String.Format("'{0}'", fieldval.ToString());
                else
                    return DateFieldFormat(((DateTime) fieldval).ToString("d-MMM-yyyy HH:mm:ss"));
            }
            else
            {
                // Ex. 123 (without quotes)
                return fieldval.ToString();
            }
        }            

        private string getUpdateSQL(string tblname)
        {

            PLCSessionVars sv = new PLCSessionVars();

            string updateStr = "";
            string whereClause = "";
            string fldName = "";
            string fldDataCurrent = "";
            string fldDataOriginal = "";
            string fldDataProposed = "";

            //string modifiedstr = "FALSE";
            DataRow _dr = PLCEditRow;
            if (_dr == null ) return "";
            DateTime dt = new DateTime();

            DataTable dtSchema = GetDBSchema(tblname);

            foreach (DataColumn dc in PLCDataTable.Columns)
            {
                if (IsEmpty() && isTransaction) // Empty fldData to make sure no values are carried over for the next field
                {
                    fldDataCurrent = "";
                    fldDataOriginal = "";
                    fldDataProposed = "";
                }

                fldName = dc.ColumnName;

                //*AAC* 2009/05/28
                //fldName = CheckTableField(fldName);

                //For Oracle
                if (fldName == "ROWID") continue;

                // [BEGIN] AAC (09/13/2010): Removed try-catch clause and replaced with checking for nulls logic and
                // VersionNotFoundException errors. Same goes for Original and Current data row versions.
                if (_dr.HasVersion(DataRowVersion.Proposed) && _dr[fldName, DataRowVersion.Proposed] != null)
                {
                    fldDataProposed = _dr[fldName, DataRowVersion.Proposed].ToString();
                }

                if (_dr.HasVersion(DataRowVersion.Current) && _dr[fldName, DataRowVersion.Current] != null)
                {
                    fldDataCurrent = _dr[fldName, DataRowVersion.Current].ToString();
                }

                if (_dr.HasVersion(DataRowVersion.Original) && _dr[fldName, DataRowVersion.Original] != null)
                {
                    fldDataOriginal = _dr[fldName, DataRowVersion.Original].ToString();
                }

                if (!string.IsNullOrEmpty(fldDataProposed))
                {
                    fldDataCurrent = fldDataProposed;
                }
                // [END] AAC (09/13/2010)

                if (fldDataCurrent != fldDataOriginal)
                {
                    
                    if (dc.DataType.ToString() == "System.DateTime")
                    {
                        if (!String.IsNullOrEmpty(fldDataCurrent))
                        {
                            try
                            {
                                dt = Convert.ToDateTime(fldDataCurrent);
                                fldDataCurrent = dt.ToString("d-MMM-yyyy HH:mm:ss");
                            }
                            catch
                            {
                                fldDataCurrent = "";
                            }
                        }
                    }

                    string dataType = "";
                    if (PLCSession.PLCDatabaseServer == "MSSQL")
                    {
                        dataType = GetDataTypeFromSchema(fldName, dtSchema);
                        if (dataType == "DATE" && !string.IsNullOrEmpty(fldDataCurrent))
                        {
                            try
                            {
                                dt = Convert.ToDateTime(fldDataCurrent);
                                fldDataCurrent = dt.ToString("d-MMM-yyyy");
                            }
                            catch
                            {
                                fldDataCurrent = "";
                            }
                        }
                    }
          

                    if (updateStr != "") updateStr += ", ";


                    if (dc.DataType.ToString() == "System.DateTime" || dataType == "DATE")
                    {
                        if (sv.PLCDatabaseServer == "MSSQL")
                        {
                            //if (String.IsNullOrEmpty(fldDataCurrent))  // should blank out date
                            //    updateStr += fldName + " = null ";
                            //else if (useQuotedFieldNames)
                            //    updateStr += "\"" + fldName + "\" = " + inSingleQuotes(fldDataCurrent);
                            //else
                            //    updateStr += fldName + " = " + inSingleQuotes(fldDataCurrent);

                            string UpdateDataStr = "";
                            if (String.IsNullOrEmpty(fldDataCurrent))
                                UpdateDataStr = " null ";
                            else
                                UpdateDataStr = inSingleQuotes(fldDataCurrent);
                                                        
                            if (useQuotedFieldNames)
                                updateStr += "\"" + fldName + "\" = " + UpdateDataStr;
                            else
                                updateStr += fldName + " = " + UpdateDataStr;

                        }
                        else
                        {
                            if (useQuotedFieldNames)
                                updateStr += "\"" + fldName + "\" = " + DateFieldFormat(fldDataCurrent);
                            else
                                updateStr += fldName + " = " + DateFieldFormat(fldDataCurrent);
                        }
                    }
                    else if ((dc.DataType.ToString() == "System.Int32") || (dc.DataType.ToString() == "System.Int16") || (dc.DataType.ToString() == "System.Double") || (dc.DataType.ToString() == "System.Decimal"))
                    {
                        string UpdateDataStr = "";
                        if (String.IsNullOrEmpty(fldDataCurrent))
                            UpdateDataStr = " null ";
                        else
                            UpdateDataStr = inSingleQuotes(fldDataCurrent);

                        if (useQuotedFieldNames)
                            updateStr += "\"" + fldName + "\" = " + UpdateDataStr;
                        else
                            updateStr += fldName + " = " + UpdateDataStr;
                    }
                    else
                    {
                        if (useQuotedFieldNames)
                            updateStr += "\"" + fldName + "\" = " + inSingleQuotes(fldDataCurrent);
                        else
                            updateStr += fldName + " = " + inSingleQuotes(fldDataCurrent);
                    }
                }
            }

            //make the where clause
            
           Boolean pkFound = false;
          
            foreach (DataColumn dc in PLCDataTable.PrimaryKey)
            {
               
                
                    fldName = dc.ColumnName;
                    fldDataOriginal = _dr[fldName].ToString();
                    if (whereClause != "") whereClause += " and ";

                    whereClause += "(" + fldName + " = '" + fldDataOriginal + "')";

                pkFound = true;

                //whereClause += "(\"" + fldName + "\" = '" + fldDataOriginal + "')";
            }


            PLCSessionVars provider = new PLCSessionVars();

            if ((provider.PLCDatabaseServer == "MSSQL") || (!pkFound && isTransaction))
            {
                // keep the where clause generated from primary keys
                string pkWhereClause = whereClause;

                whereClause = "";

                int startWhereContent;
                int foundWhereIndex = this.SQL.ToLower().IndexOf("where ");
                if (foundWhereIndex >= 0)
                {
                    startWhereContent = foundWhereIndex + "where ".Length;
                    whereClause = "(" + this.SQL.Substring(startWhereContent, this.SQL.Length - startWhereContent) + ")";
                }

                // Test if the primary key where clause points to a single record
                bool usePKWhereClause = false;
                if (pkFound)
                {
                    var qryCount = new PLCQuery();
                    qryCount.SQL = "SELECT COUNT(*) AS COUNT FROM " + tblname
                        + " WHERE " + pkWhereClause;
                    qryCount.Open();
                    if (qryCount.HasData())
                        usePKWhereClause = qryCount.iFieldByName("COUNT") == 1;
                }

                if (usePKWhereClause)
                {
                    whereClause = pkWhereClause;
                }
                // If uses PLCQuery Transaction checked if Original already exist
                else if (_dr.HasVersion(DataRowVersion.Original) && _dr[fldName, DataRowVersion.Original] != null || !isTransaction)
                {
                    foreach (DataColumn dc in PLCDataTable.Columns)
                    {
                        fldName = dc.ColumnName;
                        object fldVal = _dr[fldName, DataRowVersion.Original];

                        string whereCondition = null;

                        if (Convert.IsDBNull(fldVal))
                            whereCondition = String.Format("({0} is NULL)", fldName);
                        else if (!(fldVal is System.Double) && !(fldVal is System.Decimal) && !(fldVal is System.Array))
                        {
                            // Need to use CONVERT() to convert any text fields to varchar in sql server to avoid sql error.
                            // $$ Look for a way to detect 'text' datatypes instead so that we don't need to use CONVERT().
                            if (fldVal is System.String)
                            {
                                string dataType = GetDataTypeFromSchema(fldName, dtSchema);
                                if (dataType == "DATE")
                                {
                                    whereCondition = !string.IsNullOrEmpty(fldVal.ToString())
                                        ? string.Format("({0} = CONVERT(date,'{1}'))", fldName, Convert.ToDateTime(fldVal).ToString("yyyy'-'MM'-'dd"))
                                        : string.Format("({0} is NULL)", fldName);
                                }
                                else
                                    whereCondition = String.Format("(CONVERT(varchar(MAX),{0}) = {1})", fldName, GetSqlFormattedFieldValue(fldVal));
                            }
                            else if (fldVal is DateTime)
                                whereCondition = String.Format("({0} = CONVERT(datetime,'{1}'))", fldName, ((DateTime)fldVal).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fff"));
                            else
                                whereCondition = String.Format("({0} = {1})", fldName, GetSqlFormattedFieldValue(fldVal));
                        }

                        // Add new where condition to where clause.
                        if (whereCondition != null)
                        {
                            if (whereClause != "")
                                whereClause += " and ";

                            whereClause += whereCondition;
                        }
                    }
                }
            }


            if (updateStr == "")
                return "";
            else
            {
                return "UPDATE " + tblname + " set " + updateStr + " where " + whereClause;
            }
        }

		private void WriteAuditlog(string tblname, int AuditCode, int AuditSubCode)
		{
			WriteAuditlog(tblname, AuditCode, AuditSubCode, 0);
		}

        private void WriteAuditlog(string tblname, int AuditCode, int AuditSubCode, int changeKey)
        {
            if (!this.WriteToAuditLog)
                return;

            //Dont need to resurs again and again... 
            if ((tblname.ToUpper() == "AUDITLOG") || (tblname.ToUpper() == "TV_AUDITLOG" ) 
                || (tblname.ToUpper() == "AUDITWEB") || (tblname.ToUpper() == "TV_AUDITWEB"))
                return;
            if (AuditCode == -1) return;

            String AuditInfo = "";            
            DataTable dt = PLCDataTable;
            DataRow dr = GetCurrentRow();
            
            string BeforeValue = "";
            string AfterValue = "";
            string ColName = "";

            var additionalData = new List<string>();

            foreach (DataColumn dc in dt.Columns)
            {
                ColName = dc.ColumnName;                
                try
                {
                    BeforeValue = dr[ColName, DataRowVersion.Original].ToString();
                }
                catch
                {
                    BeforeValue = "";
                }

                try
                {
                    AfterValue = dr[ColName, DataRowVersion.Current].ToString();
                }
                catch
                {
                  AfterValue = dr[ColName, DataRowVersion.Proposed].ToString();
                }

                if (BeforeValue != AfterValue)
                {
                    if (AuditInfo != "") AuditInfo += "\r\n";
                    AuditInfo += ColName + ": [" + BeforeValue + "] => [" + AfterValue + "]";
                }
                else if (additionalData.Count < 3 && !string.IsNullOrEmpty(AfterValue)
                    && (dt.PrimaryKey != null && !dt.PrimaryKey.Contains(dc)))
                {
                    additionalData.Add(ColName + ": [" + AfterValue + "]");
                }
            }

            if (dt.PrimaryKey != null && PLCSession.PLCDatabaseServer == "MSSQL")
            {
                string primayKeys = "";
                foreach (DataColumn dc in dt.PrimaryKey)
                {
                    ColName = dc.ColumnName;
                    try
                    {
                        AfterValue = dr[ColName, DataRowVersion.Current].ToString();
                    }
                    catch
                    {
                        AfterValue = dr[ColName, DataRowVersion.Proposed].ToString();
                    }

                    if (primayKeys != "") primayKeys += ",";
                    primayKeys += ColName + ": [" + AfterValue + "]";
                }
                AuditInfo += "\r\nPRIMARY_KEY: " + primayKeys;
            }

            if (additionalData.Count > 0)
                AuditInfo += "\r\nADDITIONAL_DATA:\r\n" + string.Join("\r\n", additionalData);

            AuditInfo = "TABLE_NAME: " + tblname + "\r\n" + AuditInfo;
			PLCSession.WriteAuditLog(AuditCode.ToString(), AuditSubCode.ToString(), "0", AuditInfo, changeKey);
        }

        protected void WriteAuditlog_OldStyle(string tblname, int AuditCode, int AuditSubCode, int changeKey)
        {
            if (!this.WriteToAuditLog)
                return;

            if ((tblname.ToUpper() == "AUDITLOG") || (tblname.ToUpper() == "AUDITWEB"))
                return;
            if (AuditCode == -1) return;

            String AuditInfo = "";
            PLCSessionVars sv = new PLCSessionVars();

            bool isPrelog = !string.IsNullOrEmpty(sv.PLCGlobalPrelogUser);
      
            string sequenceName = isPrelog ? "AUDITWEB_SEQ" : "AUDITLOG_SEQ";
            string nextVal = sv.GetNextSequence(sequenceName).ToString();

            if ((nextVal == "") || (nextVal == "0"))
                return;

            DataTable dt = PLCDataTable;
            DataRow dr = GetCurrentRow();

            string BeforeValue = "";
            string AfterValue = "";
            string ColName = "";

            foreach (DataColumn dc in dt.Columns)
            {
                ColName = dc.ColumnName;

                try
                {
                    BeforeValue = dr[ColName, DataRowVersion.Original].ToString();
                }
                catch
                {
                    BeforeValue = "";
                }

                try
                {
                    AfterValue = dr[ColName, DataRowVersion.Current].ToString();
                }
                catch
                {
                    AfterValue = dr[ColName, DataRowVersion.Proposed].ToString();
                }

                if (BeforeValue != AfterValue)
                {
                    if (AuditInfo != "") AuditInfo += "\r\n";
                    AuditInfo += ColName + ": [" + BeforeValue + "] => [" + AfterValue + "]";
                }
            }

            string userId = isPrelog ? sv.PLCGlobalPrelogUser : sv.PLCGlobalAnalyst;
            string analystname = isPrelog ? sv.PrelogUserName(userId) : sv.PLCGlobalAnalystName;    
            string tableName = isPrelog ? "AUDITWEB" : "AUDITLOG";

            PLCQuery qryAudit = new PLCQuery();
            qryAudit.useQuotedFieldNames = true;
            qryAudit.SQL = "SELECT * FROM " + tableName + " where 0 = 1";
            qryAudit.Open();
            qryAudit.Append();

            DateTime TheDateTime = DateTime.Now;

            qryAudit.SetFieldValue("Log Stamp", nextVal);
            qryAudit.SetFieldValue("Time Stamp", TheDateTime);
            qryAudit.SetFieldValue("User ID", userId);
            qryAudit.SetFieldValue("Program", isPrelog ? "WEBPRELO" : "WEBINTF");
            if (sv.PLCGlobalCaseKey != "")
                qryAudit.SetFieldValue("Case Key", sv.PLCGlobalCaseKey);
            if (sv.PLCGlobalECN != "")
                qryAudit.SetFieldValue("Evidence Control Number", sv.PLCGlobalECN);
            qryAudit.SetFieldValue("Code", AuditCode);
            qryAudit.SetFieldValue("Sub Code", AuditSubCode);
            qryAudit.SetFieldValue("Error Level", 0);
            qryAudit.SetFieldValue("Additional Information", AuditInfo + "\r\n" + analystname);
            qryAudit.SetFieldValue("OS Computer Name", sv.GetOSComputerName());
            qryAudit.SetFieldValue("OS User Name", sv.GetOSUserName());
            qryAudit.SetFieldValue("OS Address", sv.GetOSAddress());


            if (isPrelog)
            {
                qryAudit.SetFieldValue("Department Case Number", sv.PLCGlobalPrelogDepartmentCaseNumber);
                qryAudit.SetFieldValue("Department Code", sv.PLCGlobalPrelogDepartmentCode);
                qryAudit.SetFieldValue("Submission Number", sv.PLCGlobalPrelogSubmissionNumber);
            }
            else
            {
                if (changeKey > 0)
                    qryAudit.SetFieldValue("Reason Change Key", changeKey);
                qryAudit.SetFieldValue("Build Number", PLCSession.PLCBEASTiLIMSVersion);
            }

            qryAudit.Post(tableName, -1, -1);        
        }

        protected void WriteAuditlog_Property(string tblname, string logTable, int AuditCode, int AuditSubCode, int changeKey)
        {
            if (!this.WriteToAuditLog)
                return;
            
            //Dont need to resurs again and again... 
            if ((tblname.ToUpper() == "AUDITLOG") || (tblname.ToUpper() == "TV_AUDITLOG")
                || (tblname.ToUpper() == "AUDITWEB") || (tblname.ToUpper() == "TV_AUDITWEB"))
                return;
            if (AuditCode == -1) return;

            String AuditInfo = "";
            PLCSessionVars sv = new PLCSessionVars();

            bool isPrelog = !string.IsNullOrEmpty(sv.PLCGlobalPrelogUser);
            logTable = isPrelog ? "AUDITWEB" : logTable;
            
            string nextVal = "";
            nextVal = sv.GetNextSequenceProperty(logTable, tblname).ToString();
            if ((nextVal == "") || (nextVal == "0"))
                return;

            DataTable dt = PLCDataTable;
            DataRow dr = GetCurrentRow();

            string BeforeValue = "";
            string AfterValue = "";
            string ColName = "";

            foreach (DataColumn dc in dt.Columns)
            {
                ColName = dc.ColumnName;
                try
                {
                    BeforeValue = dr[ColName, DataRowVersion.Original].ToString();
                }
                catch
                {
                    BeforeValue = "";
                }

                try
                {
                    AfterValue = dr[ColName, DataRowVersion.Current].ToString();
                }
                catch
                {
                    AfterValue = dr[ColName, DataRowVersion.Proposed].ToString();
                }

                if (BeforeValue != AfterValue)
                {
                    if (AuditInfo != "") AuditInfo += "\r\n";
                    AuditInfo += ColName + ": [" + BeforeValue + "] => [" + AfterValue + "]";
                }
            }

            if (AuditInfo.Length > 4000)
                AuditInfo = AuditInfo.Substring(0, 4000);



            string userId = isPrelog ? sv.PLCGlobalPrelogUser : sv.PLCGlobalAnalyst;
            string analystname = isPrelog ? sv.PrelogUserName(userId) : sv.PLCGlobalAnalystName;                 
            string tableName = isPrelog ? "AUDITWEB" : "AUDITLOG";

            PLCQuery qryAudit = new PLCQuery();
            qryAudit.SQL = "SELECT * FROM " + tableName + " where 0 = 1";
            qryAudit.Open();
            qryAudit.Append();

            DateTime TheDateTime = DateTime.Now;

            qryAudit.SetFieldValue("LOG_STAMP", nextVal);
            qryAudit.SetFieldValue("TIME_STAMP", TheDateTime);
            qryAudit.SetFieldValue("USER_ID", userId);
            qryAudit.SetFieldValue("PROGRAM", isPrelog ? "WEBPRELO" : "WEB");
            if (sv.PLCGlobalCaseKey != "")
                qryAudit.SetFieldValue("CASE_KEY", sv.PLCGlobalCaseKey);
            if (sv.PLCGlobalECN != "")
                qryAudit.SetFieldValue("EVIDENCE_CONTROL_NUMBER", sv.PLCGlobalECN);
            qryAudit.SetFieldValue("CODE", AuditCode);
            qryAudit.SetFieldValue("SUB_CODE", AuditSubCode);
            qryAudit.SetFieldValue("ERROR_LEVEL", 0);
            qryAudit.SetFieldValue("ADDITIONAL_INFORMATION", AuditInfo + "\r\n" + analystname);

            if (isPrelog)
            {
                qryAudit.SetFieldValue("DEPARTMENT_CASE_NUMBER", sv.PLCGlobalPrelogDepartmentCaseNumber);
                qryAudit.SetFieldValue("DEPARTMENT_CODE", sv.PLCGlobalPrelogDepartmentCode);
                qryAudit.SetFieldValue("SUBMISSION_NUMBER", sv.PLCGlobalPrelogSubmissionNumber);
            }
            else
            {
                if (changeKey > 0)
                    qryAudit.SetFieldValue("Reason Change Key", changeKey);
                qryAudit.SetFieldValue("Build Number", PLCSession.PLCBEASTiLIMSVersion);
            }

            qryAudit.Post(tableName, -1, -1);        

        }

        private void WriteAuditCon(string tblname, int AuditCode, int AuditSubCode, int changeKey)
        {
            //Dont need to resurs again and again... 
            if ((tblname.ToUpper() == "AUDITLOG") || (tblname.ToUpper() == "TV_AUDITLOG")
                || (tblname.ToUpper() == "AUDITWEB") || (tblname.ToUpper() == "TV_AUDITWEB")
                || (tblname.ToUpper() == "AUDITCON") || (tblname.ToUpper() == "TV_AUDITCON"))
                return;
            if (AuditCode == -1) return;

            String AuditInfo = "";
            DataTable dt = PLCDataTable;
            DataRow dr = GetCurrentRow();

            string BeforeValue = "";
            string AfterValue = "";
            string ColName = "";

            foreach (DataColumn dc in dt.Columns)
            {
                ColName = dc.ColumnName;
                try
                {
                    BeforeValue = dr[ColName, DataRowVersion.Original].ToString();
                }
                catch
                {
                    BeforeValue = "";
                }

                try
                {
                    AfterValue = dr[ColName, DataRowVersion.Current].ToString();
                }
                catch
                {
                    AfterValue = dr[ColName, DataRowVersion.Proposed].ToString();
                }

                if (BeforeValue != AfterValue)
                {
                    if (AuditInfo != "") AuditInfo += "\r\n";
                    AuditInfo += ColName + ": [" + BeforeValue + "] => [" + AfterValue + "]";
                }
            }
            AuditInfo = "TABLE_NAME: " + tblname + "\r\n" + AuditInfo;
            PLCSession.WriteAuditCon(tblname, AuditCode.ToString(), AuditSubCode.ToString(), "0", AuditInfo, changeKey);
        }

        private void WriteAuditLam(string tblname, int AuditCode, int AuditSubCode, int changeKey)
        {
            //Dont need to resurs again and again... 
            if ((tblname.ToUpper() == "AUDITLOG") || (tblname.ToUpper() == "TV_AUDITLOG")
                || (tblname.ToUpper() == "AUDITWEB") || (tblname.ToUpper() == "TV_AUDITWEB")
                || (tblname.ToUpper() == "AUDITCON") || (tblname.ToUpper() == "TV_AUDITCON")
                || (tblname.ToUpper() == "CHEMAUDT") || (tblname.ToUpper() == "TV_CHEMAUDT"))
                return;
            if (AuditCode == -1) return;

            String AuditInfo = "";
            DataTable dt = PLCDataTable;
            DataRow dr = GetCurrentRow();

            string BeforeValue = "";
            string AfterValue = "";
            string ColName = "";
            string chemControlNumber = string.Empty;

            foreach (DataColumn dc in dt.Columns)
            {
                ColName = dc.ColumnName;
                try
                {
                    BeforeValue = dr[ColName, DataRowVersion.Original].ToString();
                }
                catch
                {
                    BeforeValue = "";
                }

                try
                {
                    AfterValue = dr[ColName, DataRowVersion.Current].ToString();
                }
                catch
                {
                    AfterValue = dr[ColName, DataRowVersion.Proposed].ToString();
                }

                if (BeforeValue != AfterValue)
                {
                    if (AuditInfo != "") AuditInfo += "\r\n";
                    AuditInfo += ColName + ": [" + BeforeValue + "] => [" + AfterValue + "]";
                }

                if (ColName.Equals("CHEM_CONTROL_NUMBER") && !BeforeValue.Equals(AfterValue))
                    chemControlNumber = AfterValue;
            }
            AuditInfo = "TABLE_NAME: " + tblname + "\r\n" + AuditInfo;
            PLCSession.WriteAuditLam(chemControlNumber, tblname, AuditCode.ToString(), AuditSubCode.ToString(), "0", AuditInfo, changeKey);
        }

        protected void LogSQLUpdate(string tblname, string sql, int AuditCode, int AuditSubCode)
        {
            try
            {
                PLCSessionVars sv = new PLCSessionVars();
                sv.WriteDebug(sql, true);
            }
            catch
            {
            }

        }

        public Boolean Post(string tblname)
        {
            return Post(tblname, 0, 0);
        }

		public Boolean Post(string tblname, int AuditCode, int AuditSubCode, bool LogToAuditCon=false)
		{
            return Post(tblname, AuditCode, AuditSubCode, 0, LogToAuditCon);
		}

        public Boolean Post(string tblname, int AuditCode, int AuditSubCode, int changeKey, bool LogToAuditCon=false)
        {
            if (!CanPost())
                return false;

            PLCSessionVars sv = new PLCSessionVars();
            if (PLCNewRow != null)
            {
                string insertSQL = getInsertSQL(tblname);
                string connStr = GetCurrentConnectionString();

                using (OleDbConnection dbconn = new OleDbConnection(connStr))
                {
                    try 
                    {
                        dbconn.Open();
                        OleDbCommand cmd = GetDBCommand(insertSQL, dbconn);

                        int rowsadded = 1;
                        if (!isTransaction || this.excludeFromTransaction)
                        {
                            rowsadded = cmd.ExecuteNonQuery();
                            LogSQLUpdate(tblname, insertSQL, AuditCode, AuditSubCode);
                        }
                        else
                            CurrentTransactSQL.Add(new TransactSQL { SQL = insertSQL });

                        if (sv.IsChemInvMode())
                        {
                            WriteAuditLam(tblname, AuditCode, AuditSubCode, changeKey);
                        }
                        else
                        {
                            if (useQuotedFieldNames)
                                WriteAuditlog_OldStyle(tblname, AuditCode, AuditSubCode, changeKey);
                            else
                                WriteAuditlog(tblname, AuditCode, AuditSubCode, changeKey);

                            if (LogToAuditCon)
                            {
                                WriteAuditCon(tblname, AuditCode, AuditSubCode, changeKey);
                            }
                        }

                        PLCNewRow = (DataRow)null;
                        
                        //dbconn.Close();
                        //dbconn.Dispose();

                        if (rowsadded == 1)
                            return true;
                        else
                            return false;
                    }
                    catch(Exception e)
                    {
                        throwexception("Post(insert)", "Cannot ExecuteNonQuery", e.Message, insertSQL);
                        //dbconn.Close();
                        //dbconn.Dispose();
                    }
                    
                }
            }

            if (PLCEditRow != null)
            {
                string updateSQL = getUpdateSQL(tblname);
                if (updateSQL == "") return false;

                PLCSession.WriteDebug("UpdateSQL:" + updateSQL);

                string connStr = GetCurrentConnectionString();
                using (OleDbConnection dbconn = new OleDbConnection(connStr))
                {
                    try
                    {
                        dbconn.Open();
                    }
                    catch (Exception e)
                    {
                        throwexception("Post(update)", "Cannot open connection", e.Message);
                    }
                
                    OleDbCommand cmd = null;
                    try
                    {
                        cmd = GetDBCommand(updateSQL, dbconn);
                    }
                    catch (Exception e)
                    {
                        throwexception("Post", "Cannot create command", e.Message);
                    }

                    int rowsupdated = 1;
                    try
                    {
                        if (!isTransaction || this.excludeFromTransaction)
                        {
                            rowsupdated = cmd.ExecuteNonQuery();
                            LogSQLUpdate(tblname, updateSQL, AuditCode, AuditSubCode);
                        }
                        else
                            CurrentTransactSQL.Add(new TransactSQL { SQL = updateSQL });
                    }
                    catch (Exception e)
                    {
                        throwexception("Post", "Cannot ExecuteNonQuery", e.Message, updateSQL);
                    }

                    if (sv.IsChemInvMode())
                    {
                        WriteAuditLam(tblname, AuditCode, AuditSubCode, changeKey);
                    }
                    else
                    {
                        if (useQuotedFieldNames)
                            WriteAuditlog_OldStyle(tblname, AuditCode, AuditSubCode, changeKey);
                        else if (EditTrackLogs)
                            WriteAuditlog_Property(tblname, "AUDITLOG", AuditCode, AuditSubCode, changeKey);
                        else
                            WriteAuditlog(tblname, AuditCode, AuditSubCode, changeKey);

                        if (LogToAuditCon)
                        {
                            WriteAuditCon(tblname, AuditCode, AuditSubCode, changeKey);
                        }
                    }

                    PLCEditRow = (DataRow)null;

                    //dbconn.Close();
                    //dbconn.Dispose();

                    if (rowsupdated == 1)
                        return true;
                    else
                        return false;
                    
                }
            }
            return false;
        }
                
        public int FieldCount()
        {
            DataTable dt = PLCDataTable;
            if (dt == null) return 0;
            return dt.Columns.Count;
        }

        //$$ FieldNames() parameter index starts at 1. Change to make it zero-based?
        public string FieldNames(int i)
        {
            DataTable dt = PLCDataTable;
             if (dt == null) return "";

            if ((i < 1) || (i > dt.Columns.Count )) return "";
            int colidx = i - 1;

            DataColumn dc = PLCDataTable.Columns[colidx];
            if (dc == null) return "";

            return dc.ColumnName;

        }

        public int FieldLength(int i)
        {
            DataTable dt = PLCDataTable;
            if (dt == null) return 0;

            if ((i < 1) || (i > dt.Columns.Count)) return 0;
            int colidx = i - 1;

            DataColumn dc = PLCDataTable.Columns[colidx];
            if (dc == null) return 0;

            return dc.MaxLength
                ;

        }

        private DataRow GetCurrentRow()
        {
            if (PLCNewRow != null)
            {
                return PLCNewRow;
            }
            
            if (PLCEditRow != null)
            {
                return PLCEditRow;
            }

            int currentrow = this.currentRow;
            DataRow _dr = PLCDataTable.Rows[currentrow];
            return _dr;

        }

        private string fixquotes(string s)
        {
            if (s == null)
                return s;
            string TempStr = char.ConvertFromUtf32(2) + char.ConvertFromUtf32(1);
            s = s.Replace("'",TempStr);
            s = s.Replace(TempStr,"''");
            return s;
        }

        public bool FieldExist(string fldName)
        {
            DataColumn dc = PLCDataTable.Columns[fldName];
            if (dc == null)
                return false;
            else
                return true;
        }

        public DateTime ConvertToDBDateTime(string fieldName, object value)
        {
            DateTime dateTimeValue;

            Regex rgxTime = new Regex(@"^((0?[1-9]|1[012])(:[0-5]\d){0,2}(\ [AP]M))$|^([01]\d|2[0-3])(:[0-5]\d){1,2}$", RegexOptions.IgnoreCase);
            if (rgxTime.IsMatch(Convert.ToString(value))) //if value is in TIME format, convert to default database DateTime
                dateTimeValue = ConvertToDBTimeFormat(value);
            else if (fieldName.ToUpper().EndsWith("_TIME") && PLCDataTable.Columns.Contains(fieldName.Replace("_TIME", "_DATE"))) //if db field is a TIME field, convert value to default database DateTime
                dateTimeValue = ConvertToDBTimeFormat(value);
            else
                dateTimeValue = Convert.ToDateTime(value);

            return dateTimeValue;
        }

        public DateTime ConvertToDBTimeFormat(object time)
        {
            //default database time format: 12/30/1899 hh24:min:ss

            DateTime dateValue = new DateTime(1899, 12, 30); //12/30/1899
            DateTime timeValue;

            try
            {
                timeValue = DateTime.Parse(Convert.ToString(time));
            }
            catch
            {
                timeValue = DateTime.MinValue;
            }

            return dateValue.Date + timeValue.TimeOfDay;
        }

        public Boolean SetFieldValue(string fldName, object value)
        {
            String TheDataType = "";
            int MaxLength = 0;
            DataRow _dr;

            if (value == null)
                value = DBNull.Value;

            _dr = GetCurrentRow();

            DataColumn dc = PLCDataTable.Columns[fldName];
            if (dc == null)
            {
                throwexception("SetFieldValue", "Field [" + fldName + "] is not found");
            }

            TheDataType = dc.DataType.ToString().ToUpper();
            MaxLength = dc.MaxLength;

            try
            {
                switch (TheDataType)
                {
                    case "SYSTEM.DATETIME":
                        if (String.IsNullOrEmpty(Convert.ToString(value)))                            
                            _dr[fldName] = DBNull.Value;
                        else if (value != DBNull.Value)
                            _dr[fldName] = ConvertToDBDateTime(fldName, value);
                        return true;
                    case "SYSTEM.INT16":
                    case "SYSTEM.INT32":
                    case "SYSTEM.DECIMAL":
                    case "SYSTEM.DOUBLE":
                        _dr[fldName] = GetParseValue(value, TheDataType);
                        return true;
                    default:
                        if (value != DBNull.Value) 
                            value = fixquotes(value.ToString());

                        if (value.ToString().Length > MaxLength)
                        {
                            value = value.ToString().Substring(0, MaxLength);

                            if (value.ToString().EndsWith("'"))
                                value = value.ToString().TrimEnd('\'');
                        }               
                        
                        _dr[fldName] = value;
                        return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private object GetParseValue(object value, string DataType)
        {
            switch (DataType)
            {
                case "SYSTEM.INT16":
                case "SYSTEM.INT32":
                    int NewInt;
                    if (Int32.TryParse(Convert.ToString(value), out NewInt))
                        value = NewInt;
                    else
                        value = DBNull.Value;
                    break;
                case "SYSTEM.DECIMAL":
                case "SYSTEM.DOUBLE":
                    double NewDouble;
                    if (Double.TryParse(Convert.ToString(value), out NewDouble))
                        value = NewDouble;
                    else
                        value = DBNull.Value;
                    break;
            }

            return value;
        }

        public Boolean SetFieldValue(string fldName, int value)
        {
            //*AAC* 2009/05/28
            //fldName = CheckTableField(fldName);            

            DataRow _dr = GetCurrentRow();
            
            DataColumn dc = PLCDataTable.Columns[fldName];

            if (dc == null)
            {
                string myError = "Field [" + fldName + "] is not found.";
                throwexception("SetFieldValue", myError);
                return false;
            }
            //dc.ExtendedProperties.Add("MODIFIED", "TRUE");
            //_dr[fldName] = value;
            //*AAC* 2009/05/29 TheDataType of fldName is Int16 but the value is greater than the memory allocated.
            switch (dc.DataType.ToString().ToUpper())
            {                
                case "SYSTEM.INT16":
                case "SYSTEM.INT32":
                    _dr[fldName] = GetParseValue(value, dc.DataType.ToString().ToUpper());
                    // _dr[fldName] = Convert.ToInt32(value);
                    return true;               
                default:
                    _dr[fldName] = value;
                    return true;
            }
           
        }

        public string FieldByIndex(int i)
        {
            string fieldname = this.FieldNames(i+1);    //$$ Need to add 1 because FieldNames() parameter is one-based. 

            if (!String.IsNullOrEmpty(fieldname))
                return this.FieldByName(fieldname);
            else
                return "";
        }

        public byte[] BlobFieldByName(string fldName)
        {
            DataTable _dt = PLCDataTable;

            if (_dt == null)
            {
                string myError = System.Environment.NewLine + "Query Is not open";
                throwexception("FieldByName", myError);
                return new byte[0];
            }

            if (_dt.Rows.Count < 1)
            {
                throwexception("FieldByName", "Field [" + fldName + "]", "no rows in result set");
                return new byte[0];
            }

            DataRow _dr = GetCurrentRow();

            int idx = _dt.Columns.IndexOf(fldName);
            if (idx >= 0)
            {
                try
                {
                    return (byte[])_dr[idx];
                }
                catch (Exception)
                {
                    return new byte[0];
                }

            }
            else
            {
                throwexception("FieldByName", "Field [" + fldName + "] is not found");
                return new byte[0];
            }
        }

        public object RawFieldByName(string fldName)
        {
            DataTable _dt = PLCDataTable;

            if (_dt == null)
            {
                string myError = System.Environment.NewLine + "Query Is not open";
                throwexception("RawFieldByName", myError);
                return "";
            }

            if (_dt.Rows.Count < 1)
            {
                throwexception("RawFieldByName", "Field [" + fldName + "]", "no rows in result set");
                return "";
            }

            DataRow _dr = GetCurrentRow();

            int idx = _dt.Columns.IndexOf(fldName);
            if (idx >= 0)
                return _dr[idx];
            else
            {
                throwexception("RawFieldByName", "Field [" + fldName + "] is not found");
                return null;
            }
        }

        public string FieldByName(string fldName)
        {
            DataTable _dt = PLCDataTable;

            //*AAC* 2009/05/28
            //fldName = CheckTableField(fldName);

            if (_dt == null)
            {
                string myError = System.Environment.NewLine + "Query Is not open";
                throwexception("FieldByName", myError);
                return "";
            }

            if (_dt.Rows.Count < 1)
            {
                throwexception("FieldByName", "Field [" + fldName + "]", "no rows in result set");
                return "";
            }


            String typeName = "";
            DataRow _dr = GetCurrentRow();

            int idx = _dt.Columns.IndexOf(fldName);
            if (idx >= 0)
            {
                String ts = _dr[idx].ToString();
                if (ts == "System.Byte[]")
                {
                    ts = System.Text.Encoding.UTF8.GetString((Byte[])_dr[idx]);
                }

                try // added to handle converting scientific notation
                {
                    typeName = _dr[idx].GetType().ToString().ToUpper();
                    if ((typeName == "SYSTEM.DOUBLE") || (typeName == "SYSTEM.SINGLE"))
                    {
                        Double dbl = (Double)_dr[idx];
                        ts = dbl.ToString("########0.##########");
                    }
                }
                catch (Exception e)
                {
                    PLCSession.WriteDebug("Error in FieldByName:" + fldName, true);
                }
                return ts;
            }
            else
            {
                throwexception("FieldByName", "Field [" + fldName + "] is not found");
                return "";
            }
        }

        public string oldFieldByName(string fldName)
        {
            DataTable _dt = PLCDataTable;

            //*AAC* 2009/05/28
            //fldName = CheckTableField(fldName);

            if (_dt == null)
            {
                string myError = System.Environment.NewLine + "Query Is not open";
                throwexception("FieldByName", myError);
                return "";
            }

            if (_dt.Rows.Count < 1)
            {
                throwexception("FieldByName", "Field [" + fldName + "]","no rows in result set");
                return "";
            }


            DataRow _dr = GetCurrentRow();

            int idx = _dt.Columns.IndexOf(fldName);
            if (idx >= 0)
            {
                String ts = _dr[idx].ToString();
                if (ts == "System.Byte[]")
                {
                    ts = System.Text.Encoding.UTF8.GetString((Byte[])_dr[idx]);
                }
                return ts;
            }
            else
            {
                throwexception("FieldByName", "Field [" + fldName + "] is not found");
                return "";
            }
        }

        public String fieldDataType(string fldName)
        {
            DataTable _dt = PLCDataTable;

            if (_dt == null)
            {
                string myError = System.Environment.NewLine + "Query Is not open";
                throwexception("FieldByName", myError);
                return "";
            }

            if (_dt.Rows.Count < 1)
            {
                throwexception("FieldByName", "Field [" + fldName + "]", "no rows in result set");
                return "";
            }

            int idx = _dt.Columns.IndexOf(fldName);
            return _dt.Columns[idx].DataType.ToString();
        }

        public int iFieldByName(string fldName, int iDefault)
        {

            String s = FieldByName(fldName);

            int i = 0;

            try {
                i = Convert.ToInt32(s);
                return i;
            }
            catch 
            {
                return iDefault;
            }
            
        }

        public string dFieldByName(string fldName)
        {
            String s = FieldByName(fldName);
            try
            {
                DateTime MyDate = DateTime.Parse(s);
                return MyDate.Date.ToShortDateString();
            }
            catch
            {
                return "";
            }
        }

        public byte[] bFieldByName(string fldName)
        {
            DataTable _dt = PLCDataTable;
            //*AAC* 2009/05/28
            //fldName = CheckTableField(fldName);

            if (_dt == null)
            {
                string myError = System.Environment.NewLine + "Query Is not open";
                throwexception("FieldByName", myError);
                return null;
            }

            if (_dt.Rows.Count < 1)
            {
                throwexception("FieldByName", "Field [" + fldName + "]", "no rows in result set");
                return null;
            }

            int currentrow = this.currentRow;

            int idx = _dt.Columns.IndexOf(fldName);

            if (idx >= 0)
            {
                try
                {
                    return (byte[])_dt.Rows[currentrow][idx];
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                throwexception("FieldByName", "Field [" + fldName + "] is not found");
                return null;
            }
        }

        public string tFieldByName(string fldName)
        {
            String s = FieldByName(fldName);
            try
            {
                DateTime MyDate = DateTime.Parse(s);
                return MyDate.TimeOfDay.ToString();
            }
            catch
            {
                return "";
            }
        }

        public int iFieldByName(string fldName)
        {
            return iFieldByName(fldName,0);
        }

        public void SetParam(string pName, string pValue)
        {

            string pToken = ":" + pName;
            SQL = SQL.Replace(pToken, pValue);

        }

        //*AAC* 2009/05/28 In MSSQL, replace the TYPE with TYPE_VALUE.
        //private string CheckTableField(string fldname)
        //{
        //    PLCSessionVars checkDBase = new PLCSessionVars();

        //    switch (fldname.ToUpper())
        //    {
        //        case "TYPE":
        //            fldname = (checkDBase.PLCDatabaseServer == "ORACLE" ? fldname : "TYPE_VALUE");
        //            break;

        //    }

        //    return fldname;
        //}

        // Remove ROWID column (in Oracle) and primary key constraints.
        public void RemovePrimaryKeys()
        {
            if (this.PLCDataTable != null)
            {
                this.PLCDataTable.PrimaryKey = null;
                
                int columnCount = this.PLCDataTable.Columns.Count;
                for (int i = 0; i < columnCount; i++)
                {
                    var rowID = "ROWID" + (i > 0 ? i.ToString() : "");
                    if (this.PLCDataTable.Columns.IndexOf(rowID) >= 0)
                        this.PLCDataTable.Columns.Remove(rowID);
                    else
                        break;
                }
            }
        }

        #region AUDIT LOG

        public Dictionary<string, string> GetAffectedItemRecords()
        {
            Dictionary<string, string> AffectedItemRecords = new Dictionary<string, string>();

            string SQL = "SELECT (SELECT COUNT(*) FROM TV_LABSTAT WHERE EVIDENCE_CONTROL_NUMBER=" + PLCSession.PLCGlobalECN + ") AS LABSTAT_CNT," +
                                "(SELECT COUNT(*) FROM TV_ITASSIGN WHERE EVIDENCE_CONTROL_NUMBER=" + PLCSession.PLCGlobalECN + ") AS ITASSIGN_CNT," +
                                "(SELECT COUNT(*) FROM TV_SRDETAIL WHERE EVIDENCE_CONTROL_NUMBER=" + PLCSession.PLCGlobalECN + ") AS SRDETAIL_CNT";

            if (PLCSession.PLCDatabaseServer != "MSSQL")
            {
                SQL = "SELECT (SELECT COUNT(*) AS CNT FROM TV_LABSTAT WHERE EVIDENCE_CONTROL_NUMBER=" + PLCSession.PLCGlobalECN + ") AS LABSTAT_CNT," +
                            "(SELECT COUNT(*) AS CNT FROM TV_ITASSIGN WHERE EVIDENCE_CONTROL_NUMBER=" + PLCSession.PLCGlobalECN + ") AS ITASSIGN_CNT," +
                            "(SELECT COUNT(*) AS CNT FROM TV_SRDETAIL WHERE EVIDENCE_CONTROL_NUMBER=" + PLCSession.PLCGlobalECN + ") AS SRDETAIL_CNT FROM DUAL";
            }

            try
            {

                using (OleDbConnection OleDbConn = new OleDbConnection(GetCurrentConnectionString()))
                {
                    try
                    {
                        OleDbConn.Open();
                    }
                    catch (Exception e)
                    {
                        throwexception("Open", "Cannot Open Database Connection", e.Message);
                    }

                    OleDbDataAdapter OleDbDA = new OleDbDataAdapter(SQL, OleDbConn);
                    DataTable DT = new DataTable();
                    OleDbDA.Fill(DT);

                    if (DT.Rows.Count > 0)
                    {
                        AffectedItemRecords.Add("TV_LABSTAT", DT.Rows[0]["LABSTAT_CNT"].ToString());
                        AffectedItemRecords.Add("TV_ITASSIGN", DT.Rows[0]["ITASSIGN_CNT"].ToString());
                        AffectedItemRecords.Add("TV_SRDETAIL", DT.Rows[0]["SRDETAIL_CNT"].ToString());
                    }
                }
            }
            catch (Exception e)
            {
                throwexception("Select", "Cannot Execute Select Query", e.Message, this.SQL);
            }

            return AffectedItemRecords;
        }

        public void Delete(string TableName, string WhereClause)
        {
            Delete(TableName, WhereClause, 0, 2);
        }

        public void Delete(string TableName, string WhereClause, int AuditCode, int AuditSubCode, bool LogToAuditCon=false)
        {
            PLCSessionVars sv = new PLCSessionVars();
            if (String.IsNullOrEmpty(TableName) || String.IsNullOrEmpty(WhereClause))
            {
                throwexception("Required Parameters", "Table Name and Where Clause Required", "TableName: " + TableName + ", Where Clause: " + WhereClause);
                return;
            }

            String AuditInfo = GetAuditLogInfo(TableName, WhereClause, AuditCode, AuditSubCode);

            using (OleDbConnection dbconn = new OleDbConnection(GetCurrentConnectionString()))
            {
                try
                {
                    dbconn.Open();
                }
                catch (Exception e)
                {
                    throwexception("Open", "Cannot Open Database Connection", e.Message);
                    return;
                }

                this.SQL = "DELETE FROM " + TableName + " " + WhereClause;
                OleDbCommand dc = GetDBCommand(this.SQL, dbconn);
                try
                {
                    dc.ExecuteNonQuery();

                    if (AuditInfo.Length > 0)
                    {
                        if (sv.IsChemInvMode())
                        {
                            PLCSession.WriteAuditLam("", TableName, AuditCode.ToString(), AuditSubCode.ToString(), "0", AuditInfo, 0);
                        }
                        else
                        {
                            PLCSession.WriteAuditLog(AuditCode.ToString(), AuditSubCode.ToString(), "0", AuditInfo);
                            if (LogToAuditCon)
                                PLCSession.WriteAuditCon(TableName, AuditCode.ToString(), AuditSubCode.ToString(), "0", AuditInfo, 0);
                            AuditInfo.Remove(0, AuditInfo.Length);
                        }
                    }
                }
                catch (Exception e)
                {
                    throwexception("Delete", "Cannot ExecuteQuery", e.Message, this.SQL);
                    return;
                }
            }
        }

        public Boolean Delete(string TableName, string WhereClause, int AuditCode, int AuditSubCode, string disableThrowOleDbExeption)
        {
            if (String.IsNullOrEmpty(TableName) || String.IsNullOrEmpty(WhereClause))
            {
                throwexception("Required Parameters", "Table Name and Where Clause Required", "TableName: " + TableName + ", Where Clause: " + WhereClause);
                return false;
            }

            String AuditInfo = GetAuditLogInfo(TableName, WhereClause, AuditCode, AuditSubCode);

            using (OleDbConnection dbconn = new OleDbConnection(GetCurrentConnectionString()))
            {
                try
                {
                    dbconn.Open();
                }
                catch (Exception e)
                {
                    throwexception("Open", "Cannot Open Database Connection", e.Message);
                    return false;
                }

                this.SQL = "DELETE FROM " + TableName + " " + WhereClause;
                OleDbCommand dc = GetDBCommand(this.SQL, dbconn);
                try
                {
                    dc.ExecuteNonQuery();

                    if (AuditInfo.Length > 0)
                    {
                        PLCSession.WriteAuditLog(AuditCode.ToString(), AuditSubCode.ToString(), "0", AuditInfo);
                        AuditInfo.Remove(0, AuditInfo.Length);
                    }
                }
                catch (OleDbException o)
                {
                    if (!disableThrowOleDbExeption.Equals("T")) throwexception("Delete", "Cannot ExecuteQuery", o.Message, this.SQL);
                    return false;
                }
                catch (Exception e)
                {
                    throwexception("Delete", "Cannot ExecuteQuery", e.Message, this.SQL);
                    return false;
                }
            }

            return true;
        }

        public String GetAuditLogInfo(string TableName, string WhereClause, int AuditCode, int AuditSubCode)
        {
            StringBuilder AuditInfo = new StringBuilder("***Deleted Information Follows***");
            AuditInfo.AppendLine();
            AuditInfo.AppendLine("Table Name, [" + TableName + "]");

            string SQL = "SELECT * FROM " + TableName + " " + WhereClause;
            PLCQuery qryLOGINFO = new PLCQuery(SQL);
            qryLOGINFO.Open();
            if (!qryLOGINFO.IsEmpty())
            {
                foreach (DataColumn Field in qryLOGINFO.PLCDataTable.Columns)
                    AuditInfo.AppendLine("\"" + Field.ColumnName + "\", [" + qryLOGINFO.PLCDataTable.Rows[0][Field.ColumnName].ToString() + "]");
                //
                switch (TableName)
                {
                    case "TV_LABITEM":
                        CollectLogInfo_TV_LABITEM(ref AuditInfo);
                        break;
                    default:
                        break;
                }
            }

            return AuditInfo.ToString();
        }

        private void CollectLogInfo_TV_LABITEM(ref StringBuilder AuditInfo)
        {
            List<string> ItemRelatedTableCollection = new List<string>();
            ItemRelatedTableCollection.Add("TV_LABSTAT");
            ItemRelatedTableCollection.Add("TV_ITASSIGN");
            ItemRelatedTableCollection.Add("TV_CURRENCY");
            ItemRelatedTableCollection.Add("TV_SUBLINK");

            foreach (String TableName in ItemRelatedTableCollection)
            {
                GetLogInfo(TableName, ref AuditInfo);
            }
        }

        private void GetLogInfo(string TableName, ref StringBuilder AuditInfo)
        {
            PLCQuery QRY = new PLCQuery("SELECT * FROM " + TableName + " WHERE EVIDENCE_CONTROL_NUMBER=" + PLCSession.PLCGlobalECN);
            QRY.Open();
            if (!QRY.IsEmpty())
            {
                AuditInfo.AppendLine();
                AuditInfo.AppendLine("***Deleted Information Follows***");
                AuditInfo.AppendLine("Table Name, [" + TableName + "]");

                foreach (DataRow Row in QRY.PLCDataTable.Rows)
                {
                    foreach (DataColumn FieldName in QRY.PLCDataTable.Columns)
                        AuditInfo.AppendLine("\"" + FieldName + "\", [" + Row[FieldName].ToString() + "]");
                    //
                    AuditInfo.AppendLine();
                }
            }
        }

        #endregion //AUDIT LOG

        private bool ParseInsertSql(string insertSql, out List<string> columnList, out List<string> valueList)
        {
            string pattern = @"^\s*INSERT\s+INTO\s+(\w+)\s+\((.+)\)\s+VALUES\s*\((.+)\)";

            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            Match match = regex.Match(sql);

            if (!match.Success)
            {
                columnList = new List<string>();
                valueList = new List<string>();
                return false;
            }

            string tableName = match.Groups[1].Value;
            string csvColumns = match.Groups[2].Value;
            string csvValues = match.Groups[3].Value;
            
            string[] columns = csvColumns.Split(',');
            string[] values = csvValues.Split(',');

            columnList = new List<string>();
            valueList = new List<string>();

            foreach (string column in columns)
            {
                columnList.Add(column.Trim());
            }

            foreach (string value in values)
            {
                valueList.Add(value.Trim());
            }

            return true;
        }

        private void WriteAuditLogForInsertSql(string sql, string tblname, int auditCode, int auditSubcode, int errorCode)
        {
            if (!this.writeToAuditLog)
                return;

            string tblnameUpper = tblname.ToUpper();
            if ((tblnameUpper == "AUDITLOG") || (tblnameUpper == "TV_AUDITLOG") ||
                ((tblnameUpper == "AUDITCON") || (tblnameUpper == "TV_AUDITCON")) ||
                ((tblnameUpper == "CHEMAUDT") || (tblnameUpper == "TV_CHEMAUDT")))
                return;
            
            List<string> columnNames;
            List<string> fieldValues;
            ParseInsertSql(sql, out columnNames, out fieldValues);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("TABLE_NAME: " + tblname);

            for (int i = 0; i < columnNames.Count; i++)
            {
                // Wrap the previous line.
                if (i > 0)
                    sb.AppendFormat("\r\n");

                // FIELD1: [] => [value]
                string colName = columnNames[i];
                string fieldVal = (i < fieldValues.Count) ? fieldValues[i] : "";

                sb.AppendFormat("{0}: [] => [{1}]", colName, fieldVal);
            }

            PLCSession.WriteAuditLog(auditCode.ToString(), auditSubcode.ToString(), errorCode.ToString(), sb.ToString());
        }

        public void ExecInsertSQL(string tblname)
        {
            this.ExecInsertSQL(tblname, 0, 0);
        }

        public void ExecInsertSQL(string tblname, int auditCode, int auditSubcode)
        {
            using (OleDbConnection dbconn = new OleDbConnection(GetCurrentConnectionString()))
            {
                try
                {
                    dbconn.Open();
                }
                catch (Exception e)
                {
                    throwexception("ExecInsertSQL", "Cannot open database connection", e.Message);
                    return;
                }

                OleDbCommand dc = GetDBCommand(this.SQL, dbconn);

                try
                {
                    if (!isTransaction || this.excludeFromTransaction)
                    {
                        if (this._parameters != null)
                        {
                            foreach (OleDbParameter param in this._parameters)
                                dc.Parameters.Add(param);
                        }
                        dc.ExecuteNonQuery();
                        LogSQLUpdate(tblname, this.SQL, auditCode, auditSubcode);
                    }
                    else
                        CurrentTransactSQL.Add(new TransactSQL { SQL = dc.CommandText, Parameters = this._parameters });
                }
                catch (Exception e)
                {
                    throwexception("ExecInsertSQL", "Exception occured in ExecuteNonQuery()", e.Message);
                    return;
                }
            }

            WriteAuditLogForInsertSql(this.SQL, tblname, auditCode, auditSubcode, 0);
        }

        public void AddSQLParameter(string name, object val)
        {
            if (_parameters == null)
                _parameters = new List<OleDbParameter>();

            OleDbParameter param = new OleDbParameter(name, val);

            // Set default size to 5. FillSchema() requires the parameters to be explicitly defined and parameter size should not be zero.
            if (param.OleDbType == OleDbType.VarWChar && param.Size == 0)
                param.Size = 5;

            this._parameters.Add(param);
        }

        #region Transaction
        /// <summary>
        /// 
        /// </summary>
        class TransactSQL
        {
            public string SQL { get; set; }
            public List<OleDbParameter> Parameters { get; set; }
        }

        //static bool isTransaction = false;
        static Dictionary<string, List<TransactSQL>> transactSQL = new Dictionary<string,List<TransactSQL>>();
        static IsolationLevel isolationLvl;
        int TransactionGroup = 0;

        private static System.Web.SessionState.HttpSessionState Session
        {
            get { return HttpContext.Current.Session; }
        }

        private static List<TransactSQL> CurrentTransactSQL
        {
            get { return transactSQL[Session.SessionID]; }
        }

        private static bool isTransaction
        {
            get { return Session["IsPLCQueryTransaction"] != null ? (bool)Session["IsPLCQueryTransaction"] : false; }
            set { Session["IsPLCQueryTransaction"] = value; }
        }

        public static bool IsTransaction()
        {
            return isTransaction;
        }

        /// <summary>
        /// Notify PLCQuery to use transaction.
        /// </summary>
        /// <param name="isolationLevel">The isolation level at which the transaction should run.</param>
        public static void BeginTransaction(IsolationLevel isolationLevel)
        {
            if (isTransaction) Rollback();
            isTransaction = true;
            transactSQL.Add(Session.SessionID, new List<TransactSQL>());
            isolationLvl = isolationLevel;
        }

        public static void BeginTransaction()
        {
            BeginTransaction(IsolationLevel.ReadCommitted);
        }

        /// <summary>
        /// Commits the database transaction.
        /// </summary>
        /// <returns></returns>
        /// <see cref="PLCQuery.BeginTransaction"/>
        /// <seealso cref="PLCQuery.Rollback"/>
        public static bool Commit()
        {
            if (isTransaction)
            {
                using (OleDbConnection dbconn = new OleDbConnection(PLCSession.GetConnectionString()))
                {
                    OleDbTransaction transaction = null;

                    try
                    {
                        dbconn.Open();
                        transaction = dbconn.BeginTransaction(isolationLvl);

                        OleDbCommand cmd = new OleDbCommand();
                        cmd.Connection = dbconn;
                        cmd.Transaction = transaction;

                        foreach (TransactSQL tsql in CurrentTransactSQL)
                        {
                            PLCSession.PLCErrorSQL = cmd.CommandText = tsql.SQL;
                            cmd.Parameters.Clear();
                            if (tsql.Parameters != null)
                            {
                                foreach (OleDbParameter param in tsql.Parameters)
                                    cmd.Parameters.Add(param);
                            }
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        isTransaction = false;
                        transactSQL.Remove(Session.SessionID);
                        return true;
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            transaction.Rollback();
                            Rollback();
                            PLCSession.PLCErrorMessage = e.Message;
                        }
                        catch
                        {

                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Rolls back a transaction from a pending state.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        ///     <see cref="PLCQuery.BeginTransaction"/>
        ///     <seealso cref="PLCQuery.Commit"/>
        /// </remarks>
        public static bool Rollback()
        {
            if (isTransaction)
            {
                isTransaction = false;
                transactSQL.Remove(Session.SessionID);
                return true;
            }
            return false;
        }
        #endregion Transaction

        #region Database Command
        private OleDbCommand GetDBCommand(string cmdText, OleDbConnection connection)
        {
            var command = GetDBCommand();

            command.CommandText = cmdText;
            command.Connection = connection;

            return command;
        }

        private OleDbCommand GetDBCommand()
        {
            var command = new OleDbCommand();

            command.CommandTimeout = 300; // 5 min

            return command;
        }
        #endregion Database Command

        /// <summary>
        /// Check if the query is an empty fields query. Ex. (SELECT * FROM TBL 0=1)
        /// </summary>
        /// <param name="pstrQuery">The query to be evaluated</param>
        /// <returns>Returns true if it is an empty fields query</returns>
        private bool CheckForEmptyFieldQuery(string pstrQuery)
        {
            bool blnIsEmptyFieldSql = false;
            DateTime dteStartTime = DateTime.MinValue;
            string strEmptyFieldTableName = string.Empty;

            dteStartTime = DateTime.Now;

            // if it is and there's a cached empty fields resultset, restore it.
            blnIsEmptyFieldSql = IsEmptyFieldsSql(pstrQuery, out strEmptyFieldTableName);
            if (blnIsEmptyFieldSql)
            {
                if (RestoreEmptyFieldsResultSet(strEmptyFieldTableName))
                {
                    LogSQLUpdate("OPEN:", "QUERY TIME: " + DateTime.Now.Subtract(dteStartTime).ToString(), 0, 0);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if the query vunerable for SQL Injections
        /// </summary>
        /// <param name="pstrQuery">The query to be evaluated</param>
        /// <returns>Returns false if it is not vunerable for SQL Injection, else will throw an error</returns>
        private bool CheckForSQLInjection(string pstrQuery)
        {
            bool blnSQLInjectionSuspected = false;

            blnSQLInjectionSuspected = PLCSession.SQLInjectionSuspected(pstrQuery);
            if (blnSQLInjectionSuspected)
            {
                PLCSession.ForceWriteDebug("Error 57 - SQL Injection Suspected", true);
                PLCSession.ForceWriteDebug(this.SQL, true);
                throw new System.ArgumentException("System Error 57", "See LOG for more info");
            }

            return false;
        }

        /// <summary>
        /// Initializes the connection
        /// </summary>
        /// <param name="pstrDatabaseServerType">The database server type being used</param>
        /// <param name="pblnHasKeys">Tells if the query uses keys</param>
        /// <param name="pobjDataTable">The data table to be filled</param>
        /// <param name="pblnIsEmptyFieldSQL">Tells if it is an empty fields query</param>
        /// <param name="pstrEmptyFieldTableName">The empty field table name (if it is an empty fields query)</param>
        /// <param name="pdteStartTime">The start time of the process</param>
        /// <returns>Returns true if no errors are found</returns>
        private bool InitializeConnection(string pstrDatabaseServerType, bool pblnHasKeys, DataTable pobjDataTable, bool pblnIsEmptyFieldSQL, string pstrEmptyFieldTableName, DateTime pdteStartTime)
        {
            if (pstrDatabaseServerType == "ORACLE")
            {
                InitializeOracleConnection(pblnHasKeys, pobjDataTable, pblnIsEmptyFieldSQL, pstrEmptyFieldTableName, pdteStartTime);
            }
            else if(pstrDatabaseServerType == "MSSQL")
            {
                InitializeMSSQLConnection(pblnHasKeys, pobjDataTable, pblnIsEmptyFieldSQL, pstrEmptyFieldTableName, pdteStartTime);
            }

            return true;
        }

        /// <summary>
        /// Initializes the connection for MSSQL database server type
        /// </summary>
        /// <param name="pblnHasKeys">Tells if the query uses keys</param>
        /// <param name="pobjDataTable">The data table to be filled</param>
        /// <param name="pblnIsEmptyFieldSQL">Tells if it is an empty fields query</param>
        /// <param name="pstrEmptyFieldTableName">The empty field table name (if it is an empty fields query)</param>
        /// <param name="pdteStartTime">The start time of the process</param>
        /// <returns>Returns true if no errors are found</returns>
        private bool InitializeMSSQLConnection(bool pblnHasKeys, DataTable pobjDataTable, bool pblnIsEmptyFieldSQL, string pstrEmptyFieldTableName, DateTime pdteStartTime)
        {
            DateTime dteStartTime = DateTime.Now;
            OleDbCommand objCommand = null;
            OleDbDataAdapter objDataAdapter = null;
            string strSchemaTag = string.Empty;

            using (OleDbConnection objConnection = PLCSession.GetDBConn(GetCurrentConnectionString()))
            {
                // log
                LogSQLUpdate("CONNECT:", "TIME3:" + DateTime.Now.Subtract(dteStartTime).ToString(), 0, 0);
                LogSQLUpdate("OPEN:", this.SQL, 0, 0);
                dteStartTime = DateTime.Now;

                // open data adapter
                objDataAdapter = new OleDbDataAdapter(this.SQL, objConnection);

                // log
                LogSQLUpdate("CONNECT:", "TIME4:" + DateTime.Now.Subtract(dteStartTime).ToString(), 0, 0);

                // Pass any parameters.
                if (this._parameters != null)
                {
                    // create the command object
                    objCommand = GetDBCommand(this.SQL, objConnection);
                    objDataAdapter.SelectCommand = objCommand;

                    // pass the parameters into the data adapter
                    foreach (OleDbParameter objParameters in _parameters)
                        objDataAdapter.SelectCommand.Parameters.Add(objParameters);
                }

                if (pblnHasKeys)
                    objDataAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey; //Live Query

                try
                {
                    // fill the data table
                    dteStartTime = DateTime.Now;
                    objDataAdapter.Fill(pobjDataTable);

                    // log
                    LogSQLUpdate("CONNECT:", "FILLTIME:" + DateTime.Now.Subtract(dteStartTime).ToString(), 0, 0);
                }
                catch (Exception e)
                {
                    try
                    {
                        dteStartTime = DateTime.Now;
                        objDataAdapter = new OleDbDataAdapter(this.SQL, objConnection);
                        LogSQLUpdate("CONNECT:", "ADAPTERTIME:" + DateTime.Now.Subtract(dteStartTime).ToString(), 0, 0);

                        if (!redirectOnError) throw e;
                    }

                    catch (Exception f)
                    {
                        objConnection.Close();
                        throwexception("Open", "Cannot fill data table(1)", e.Message + "/" + f.Message);
                    }
                }

                dteStartTime = DateTime.Now;
                LogSQLUpdate("CONNECT:", "TIME5:" + DateTime.Now.Subtract(dteStartTime).ToString(), 0, 0);

                if (pobjDataTable.Rows.Count > 0)
                {
                    this.PLCDataTable = pobjDataTable;

                    // Cache this result set if it's an empty fields query.
                    if (pblnIsEmptyFieldSQL)
                        CacheEmptyFieldsResultSet(pstrEmptyFieldTableName);
                    _isOpen = true;
                    return true;
                }
                else
                {
                    if (!canPost)
                    {
                        this.PLCDataTable = pobjDataTable;

                        // Cache this result set if it's an empty fields query.
                        if (pblnIsEmptyFieldSQL)
                            CacheEmptyFieldsResultSet(pstrEmptyFieldTableName);
                        _isOpen = true;
                        return true;
                    }

                    try
                    {
                        // Clear any parameters.
                        if (objDataAdapter.SelectCommand != null)
                            objDataAdapter.SelectCommand.Parameters.Clear();

                        strSchemaTag = getSchemaCacheTag(SQL);
                        if (strSchemaTag != "")
                        {
                            if (RestoreEmptyFieldsResultSet(strSchemaTag))
                            {
                                LogSQLUpdate("SchemaCache:", "RESTORE TIME: " + DateTime.Now.Subtract(pdteStartTime).ToString(), 0, 0);
                                return true;
                            }
                        }

                        this.PLCDataTable = null;
                        _isOpen = true;
                        return true;
                    }
                    catch (Exception e)
                    {
                        throwexception("Open", "Cannot fill data table(2)", e.Message);
                    }
                }

                // log
                LogSQLUpdate("OPEN:", "QUERY TIME: " + DateTime.Now.Subtract(pdteStartTime).ToString(), 0, 0);

                try
                {
                    _isOpen = true;

                    if (pobjDataTable.Columns.Count > 0)
                        return true;
                    else
                        return false;
                }
                catch
                {
                    _isOpen = false;
                    return false;
                }
            }
        }

        /// <summary>
        /// Initializes the connection for ORACLE Database Server Type
        /// </summary>
        /// <param name="pblnHasKeys">Tells if the query uses keys</param>
        /// <param name="pobjDataTable">The data table to be filled</param>
        /// <param name="pblnIsEmptyFieldSQL">Tells if it is an empty fields query</param>
        /// <param name="pstrEmptyFieldTableName">The empty field table name (if it is an empty fields query)</param>
        /// <param name="pdteStartTime">The start time of the process</param>
        /// <returns>Returns true if no errors are found</returns>
        private bool InitializeOracleConnection(bool pblnHasKeys, DataTable pobjDataTable, bool pblnIsEmptyFieldSQL, string pstrEmptyFieldTableName, DateTime pdteStartTime)
        {
            //$$ Disable the System.Data.OracleClient warnings until we get the Oracle dll to use sorted out.
            #pragma warning disable 618

            DateTime dteStartTime = DateTime.Now;
            OracleCommand objCommand = null;
            OracleDataAdapter objDataAdapter = null;
            string strSchemaTag = string.Empty;

            using (OracleConnection objConnection = new OracleConnection(PLCSession.GetOracleDataReaderConnectionString()))
            {
                // log
                LogSQLUpdate("CONNECT:", "TIME3:" + DateTime.Now.Subtract(dteStartTime).ToString(), 0, 0);
                LogSQLUpdate("OPEN:", this.SQL, 0, 0);
                dteStartTime = DateTime.Now;

                // open data adapter
                objDataAdapter = new OracleDataAdapter(this.SQL, objConnection);

                // log
                LogSQLUpdate("CONNECT:", "TIME4:" + DateTime.Now.Subtract(dteStartTime).ToString(), 0, 0);

                // Pass any parameters.
                if (this._parameters != null)
                {
                    // create the command object
                    objCommand = new OracleCommand(this.SQL, objConnection);
                    objDataAdapter.SelectCommand = objCommand;

                    // pass the parameters into the data adapter
                    foreach (OleDbParameter objParameters in _parameters)
                        objDataAdapter.SelectCommand.Parameters.Add(objParameters);
                }

                if (pblnHasKeys)
                    objDataAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey; //Live Query

                try
                {
                    // fill the data table
                    dteStartTime = DateTime.Now;
                    objDataAdapter.Fill(pobjDataTable);

                    // log
                    LogSQLUpdate("CONNECT:", "FILLTIME:" + DateTime.Now.Subtract(dteStartTime).ToString(), 0, 0);
                }
                catch (Exception e)
                {
                    try
                    {
                        dteStartTime = DateTime.Now;
                        objDataAdapter = new OracleDataAdapter(this.SQL, objConnection);
                        LogSQLUpdate("CONNECT:", "ADAPTERTIME:" + DateTime.Now.Subtract(dteStartTime).ToString(), 0, 0);

                        if (!redirectOnError) throw e;
                    }

                    catch (Exception f)
                    {
                        objConnection.Close();
                        throwexception("Open", "Cannot fill data table(1)", e.Message + "/" + f.Message);
                    }
                }

                dteStartTime = DateTime.Now;
                LogSQLUpdate("CONNECT:", "TIME5:" + DateTime.Now.Subtract(dteStartTime).ToString(), 0, 0);

                if (pobjDataTable.Rows.Count > 0)
                {
                    this.PLCDataTable = pobjDataTable;

                    // Cache this result set if it's an empty fields query.
                    if (pblnIsEmptyFieldSQL)
                        CacheEmptyFieldsResultSet(pstrEmptyFieldTableName);
                    _isOpen = true;
                    return true;
                }
                else
                {
                    if (!canPost)
                    {
                        this.PLCDataTable = pobjDataTable;

                        // Cache this result set if it's an empty fields query.
                        if (pblnIsEmptyFieldSQL)
                            CacheEmptyFieldsResultSet(pstrEmptyFieldTableName);
                        _isOpen = true;
                        return true;
                    }

                    try
                    {
                        // Clear any parameters.
                        if (objDataAdapter.SelectCommand != null)
                            objDataAdapter.SelectCommand.Parameters.Clear();

                        strSchemaTag = getSchemaCacheTag(SQL);
                        if (strSchemaTag != "")
                        {
                            if (RestoreEmptyFieldsResultSet(strSchemaTag))
                            {
                                LogSQLUpdate("SchemaCache:", "RESTORE TIME: " + DateTime.Now.Subtract(pdteStartTime).ToString(), 0, 0);
                                return true;
                            }
                        }

                        this.PLCDataTable = null;
                        _isOpen = true;
                        return true;
                    }
                    catch (Exception e)
                    {
                        throwexception("Open", "Cannot fill data table(2)", e.Message);
                    }
                }

                // log
                LogSQLUpdate("OPEN:", "QUERY TIME: " + DateTime.Now.Subtract(pdteStartTime).ToString(), 0, 0);

                try
                {
                    _isOpen = true;

                    if (pobjDataTable.Columns.Count > 0)
                        return true;
                    else
                        return false;
                }
                catch
                {
                    _isOpen = false;
                    return false;
                }
            }

            #pragma warning restore 618
        }

        /// <summary>
        /// Initializes the connection and query to the database
        /// </summary>
        /// <returns>Returns true if no errors are found</returns>
        public bool OpenConnection()
        {
            bool blnIsEmptyFieldSql = false;
            bool blnHasKeys = true;
            DataTable objDataTable = null;
            DateTime dteStartTime = DateTime.Now;
            string strEmptyFieldTableName = string.Empty;
            string strQuery = string.Empty;

            // validate and update the sql string
            ValidateSql();

            // retrieve global sql string
            strQuery = this.sql;

            // update object variables
            this.canPost = blnHasKeys;
            this.currentRow = 0;
            this.isEmptyFieldsCachingActive = true;  // set to true to enable empty fields caching

            // check for sql injection
            CheckForSQLInjection(strQuery);

            // check if this is an 'empty field query'. (SELECT * FROM TBL WHERE 0=1)
            blnIsEmptyFieldSql = CheckForEmptyFieldQuery(strQuery);
            if (blnIsEmptyFieldSql)
            {
                _isOpen = true;
                return true;
            }


            // initialize objects that will be used for the query
            dteStartTime = DateTime.Now;
            this.PLCDataTable = null;
            objDataTable = new DataTable("TBL");
            objDataTable.Clear();

            // log
            LogSQLUpdate("CONNECT:", "TIME1:" + DateTime.Now.Subtract(dteStartTime).ToString(), 0, 0);

            // fix SQL into the appropriate format
            dteStartTime = DateTime.Now;
            this.SQL = PLCSession.FixedSQLStr(this.SQL);

            // log
            LogSQLUpdate("CONNECT:", "TIME2:" + DateTime.Now.Subtract(dteStartTime).ToString(), 0, 0);

            // initialize connection
            InitializeConnection(PLCSession.PLCDatabaseServer, blnHasKeys, objDataTable, blnIsEmptyFieldSql, strEmptyFieldTableName, dteStartTime);

            return true;
        }
    }
}