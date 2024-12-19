class PLCQuery
{
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
        OleDbConnection dbConnection = GetDBConnection(connectionString);
        bool isDBTransaction = IsDBTransaction();
        try
        {
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
                if (!isDBTransaction)
                    dbConnection.Close();
            }
        }
        finally
        {
            if (!isDBTransaction)
                dbConnection.Dispose();
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
}



