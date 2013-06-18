namespace System.Workflow.Runtime.Hosting
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Data;
    using System.Data.Common;
    using System.Data.OleDb;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Transactions;
    using System.Workflow.Runtime;

    internal sealed class DbResourceAllocator
    {
        internal const string ConnectionStringToken = "ConnectionString";
        private string connString;
        private const string EnlistFalseToken = ";Enlist=false";
        private Provider localProvider;

        internal DbResourceAllocator(WorkflowRuntime runtime, NameValueCollection parameters, string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                if (parameters != null)
                {
                    foreach (string str in parameters.AllKeys)
                    {
                        if (string.Compare("ConnectionString", str, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            connectionString = parameters["ConnectionString"];
                            break;
                        }
                    }
                }
                if (string.IsNullOrEmpty(connectionString) && (runtime != null))
                {
                    NameValueConfigurationCollection commonParameters = runtime.CommonParameters;
                    if (commonParameters != null)
                    {
                        foreach (string str2 in commonParameters.AllKeys)
                        {
                            if (string.Compare("ConnectionString", str2, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                connectionString = commonParameters["ConnectionString"].Value;
                                break;
                            }
                        }
                    }
                }
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new ArgumentNullException("ConnectionString", ExecutionStringManager.MissingConnectionString);
                }
            }
            this.Init(connectionString);
        }

        internal void DetectSharedConnectionConflict(WorkflowCommitWorkBatchService transactionService)
        {
            SharedConnectionWorkflowCommitWorkBatchService service = transactionService as SharedConnectionWorkflowCommitWorkBatchService;
            if ((service != null) && (string.Compare(service.ConnectionString, this.connString, StringComparison.Ordinal) != 0))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.SharedConnectionStringSpecificationConflict, new object[] { this.connString, service.ConnectionString }));
            }
        }

        private static SharedConnectionInfo GetConnectionInfo(WorkflowCommitWorkBatchService txSvc, Transaction transaction)
        {
            SharedConnectionInfo connectionInfo = null;
            SharedConnectionWorkflowCommitWorkBatchService service = txSvc as SharedConnectionWorkflowCommitWorkBatchService;
            if (service != null)
            {
                connectionInfo = service.GetConnectionInfo(transaction);
                if (connectionInfo == null)
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InvalidTransaction, new object[0]));
                }
            }
            return connectionInfo;
        }

        internal DbConnection GetEnlistedConnection(WorkflowCommitWorkBatchService txSvc, Transaction transaction, out bool isNewConnection)
        {
            DbConnection dBConnection;
            SharedConnectionInfo connectionInfo = GetConnectionInfo(txSvc, transaction);
            if (connectionInfo != null)
            {
                dBConnection = connectionInfo.DBConnection;
                isNewConnection = false;
                return dBConnection;
            }
            dBConnection = this.OpenNewConnection();
            dBConnection.EnlistTransaction(transaction);
            isNewConnection = true;
            return dBConnection;
        }

        internal static DbTransaction GetLocalTransaction(WorkflowCommitWorkBatchService txSvc, Transaction transaction)
        {
            DbTransaction dBTransaction = null;
            SharedConnectionInfo connectionInfo = GetConnectionInfo(txSvc, transaction);
            if (connectionInfo != null)
            {
                dBTransaction = connectionInfo.DBTransaction;
            }
            return dBTransaction;
        }

        private void Init(string connectionStr)
        {
            this.SetConnectionString(connectionStr);
            try
            {
                using (this.OpenNewConnection(false))
                {
                }
            }
            catch (Exception exception)
            {
                throw new ArgumentException(ExecutionStringManager.InvalidDbConnection, "connectionString", exception);
            }
            if (this.localProvider == Provider.OleDB)
            {
                this.connString = this.connString + ";OLE DB Services=-4";
            }
        }

        internal DbCommand NewCommand()
        {
            return NewCommand(this.OpenNewConnection());
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal static DbCommand NewCommand(DbConnection dbConnection)
        {
            return NewCommand(null, dbConnection, null);
        }

        internal static DbCommand NewCommand(string commandText, DbConnection dbConnection, DbTransaction transaction)
        {
            DbCommand command = dbConnection.CreateCommand();
            command.CommandText = commandText;
            command.Transaction = transaction;
            return command;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal DbParameter NewDbParameter()
        {
            return this.NewDbParameter(null, null);
        }

        internal DbParameter NewDbParameter(string parameterName, DbType type)
        {
            if (this.localProvider == Provider.SqlClient)
            {
                if (type == DbType.Int64)
                {
                    return new SqlParameter(parameterName, SqlDbType.BigInt);
                }
                return new SqlParameter(parameterName, type);
            }
            if (type == DbType.Int64)
            {
                return new OleDbParameter(parameterName, OleDbType.BigInt);
            }
            return new OleDbParameter(parameterName, type);
        }

        internal DbParameter NewDbParameter(string parameterName, object value)
        {
            if (this.localProvider == Provider.SqlClient)
            {
                return new SqlParameter(parameterName, value);
            }
            return new OleDbParameter(parameterName, value);
        }

        internal DbParameter NewDbParameter(string parameterName, DbType type, ParameterDirection direction)
        {
            DbParameter parameter = this.NewDbParameter(parameterName, type);
            parameter.Direction = direction;
            return parameter;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal DbConnection OpenNewConnection()
        {
            return this.OpenNewConnection(true);
        }

        internal DbConnection OpenNewConnection(bool disallowEnlist)
        {
            DbConnection connection = null;
            string connString = this.connString;
            if (disallowEnlist)
            {
                connString = connString + ";Enlist=false";
            }
            if (this.localProvider == Provider.SqlClient)
            {
                connection = new SqlConnection(connString);
            }
            else
            {
                connection = new OleDbConnection(connString);
            }
            connection.Open();
            return connection;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal DbConnection OpenNewConnectionNoEnlist()
        {
            return this.OpenNewConnection(true);
        }

        private void SetConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(connectionString.Trim()))
            {
                throw new ArgumentNullException("connectionString", ExecutionStringManager.MissingConnectionString);
            }
            DbConnectionStringBuilder builder = new DbConnectionStringBuilder {
                ConnectionString = connectionString
            };
            if (builder.ContainsKey("enlist"))
            {
                throw new ArgumentException(ExecutionStringManager.InvalidEnlist);
            }
            this.connString = connectionString;
            this.localProvider = Provider.SqlClient;
        }

        internal string ConnectionString
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.connString;
            }
        }
    }
}

