namespace System.Data.SqlClient
{
    using Microsoft.SqlServer.Server;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Data.Sql;
    using System.Data.SqlTypes;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Transactions;
    using System.Xml;

    [DefaultEvent("RecordsAffected"), Designer("Microsoft.VSDesigner.Data.VS.SqlCommandDesigner, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ToolboxItem(true)]
    public sealed class SqlCommand : DbCommand, ICloneable
    {
        private SqlConnection _activeConnection;
        private bool _batchRPCMode;
        private CachedAsyncState _cachedAsyncState;
        private _SqlMetaDataSet _cachedMetaData;
        private string _commandText;
        private int _commandTimeout;
        private System.Data.CommandType _commandType;
        private int _currentlyExecutingBatch;
        private bool _designTimeInvisible;
        private bool _dirty;
        private EXECTYPE _execType;
        private bool _hiddenPrepare;
        private bool _inPrepare;
        private SqlNotificationRequest _notification;
        private bool _notificationAutoEnlist;
        private static int _objectTypeCount;
        private SmiEventSink_DeferedProcessing _outParamEventSink;
        private List<SqlParameterCollection> _parameterCollectionList;
        private SqlParameterCollection _parameters;
        private volatile bool _pendingCancel;
        private int _prepareHandle;
        internal int _rowsAffected;
        private _SqlRPC[] _rpcArrayOf1;
        private List<_SqlRPC> _RPCList;
        private CommandEventSink _smiEventSink;
        private SmiRequestExecutor _smiRequest;
        private SmiContext _smiRequestContext;
        internal SqlDependency _sqlDep;
        private _SqlRPC[] _SqlRPCBatchArray;
        private TdsParserStateObject _stateObj;
        private SqlTransaction _transaction;
        private UpdateRowSource _updatedRowSource;
        internal static readonly string[] KatmaiProcParamsNames;
        internal readonly int ObjectID;
        internal static readonly string[] PreKatmaiProcParamsNames;

        [System.Data.ResCategory("DataCategory_StatementCompleted"), System.Data.ResDescription("DbCommand_StatementCompleted")]
        public event StatementCompletedEventHandler StatementCompleted;

        static SqlCommand()
        {
            string[] strArray2 = new string[15];
            strArray2[0] = "PARAMETER_NAME";
            strArray2[1] = "PARAMETER_TYPE";
            strArray2[2] = "DATA_TYPE";
            strArray2[4] = "CHARACTER_MAXIMUM_LENGTH";
            strArray2[5] = "NUMERIC_PRECISION";
            strArray2[6] = "NUMERIC_SCALE";
            strArray2[7] = "UDT_CATALOG";
            strArray2[8] = "UDT_SCHEMA";
            strArray2[9] = "TYPE_NAME";
            strArray2[10] = "XML_CATALOGNAME";
            strArray2[11] = "XML_SCHEMANAME";
            strArray2[12] = "XML_SCHEMACOLLECTIONNAME";
            strArray2[13] = "UDT_NAME";
            PreKatmaiProcParamsNames = strArray2;
            string[] strArray = new string[15];
            strArray[0] = "PARAMETER_NAME";
            strArray[1] = "PARAMETER_TYPE";
            strArray[3] = "MANAGED_DATA_TYPE";
            strArray[4] = "CHARACTER_MAXIMUM_LENGTH";
            strArray[5] = "NUMERIC_PRECISION";
            strArray[6] = "NUMERIC_SCALE";
            strArray[7] = "TYPE_CATALOG_NAME";
            strArray[8] = "TYPE_SCHEMA_NAME";
            strArray[9] = "TYPE_NAME";
            strArray[10] = "XML_CATALOGNAME";
            strArray[11] = "XML_SCHEMANAME";
            strArray[12] = "XML_SCHEMACOLLECTIONNAME";
            strArray[14] = "SS_DATETIME_PRECISION";
            KatmaiProcParamsNames = strArray;
        }

        public SqlCommand()
        {
            this.ObjectID = Interlocked.Increment(ref _objectTypeCount);
            this._commandTimeout = 30;
            this._updatedRowSource = UpdateRowSource.Both;
            this._prepareHandle = -1;
            this._rowsAffected = -1;
            this._notificationAutoEnlist = true;
            GC.SuppressFinalize(this);
        }

        private SqlCommand(SqlCommand from) : this()
        {
            this.CommandText = from.CommandText;
            this.CommandTimeout = from.CommandTimeout;
            this.CommandType = from.CommandType;
            this.Connection = from.Connection;
            this.DesignTimeVisible = from.DesignTimeVisible;
            this.Transaction = from.Transaction;
            this.UpdatedRowSource = from.UpdatedRowSource;
            SqlParameterCollection parameters = this.Parameters;
            foreach (object obj2 in from.Parameters)
            {
                parameters.Add((obj2 is ICloneable) ? (obj2 as ICloneable).Clone() : obj2);
            }
        }

        public SqlCommand(string cmdText) : this()
        {
            this.CommandText = cmdText;
        }

        public SqlCommand(string cmdText, SqlConnection connection) : this()
        {
            this.CommandText = cmdText;
            this.Connection = connection;
        }

        public SqlCommand(string cmdText, SqlConnection connection, SqlTransaction transaction) : this()
        {
            this.CommandText = cmdText;
            this.Connection = connection;
            this.Transaction = transaction;
        }

        internal void AddBatchCommand(string commandText, SqlParameterCollection parameters, System.Data.CommandType cmdType)
        {
            _SqlRPC rpc = new _SqlRPC();
            this.CommandText = commandText;
            this.CommandType = cmdType;
            this.GetStateObject();
            if (cmdType == System.Data.CommandType.StoredProcedure)
            {
                this.BuildRPC(false, parameters, ref rpc);
            }
            else
            {
                this.BuildExecuteSql(CommandBehavior.Default, commandText, parameters, ref rpc);
            }
            this._RPCList.Add(rpc);
            this._parameterCollectionList.Add(parameters);
            this.PutStateObject();
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginExecuteNonQuery()
        {
            return this.BeginExecuteNonQuery(null, null);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginExecuteNonQuery(AsyncCallback callback, object stateObject)
        {
            IAsyncResult result2;
            SqlConnection.ExecutePermission.Demand();
            this._pendingCancel = false;
            this.ValidateAsyncCommand();
            SqlStatistics statistics = null;
            try
            {
                statistics = SqlStatistics.StartTimer(this.Statistics);
                ExecutionContext execContext = (callback == null) ? null : ExecutionContext.Capture();
                DbAsyncResult result = new DbAsyncResult(this, "EndExecuteNonQuery", callback, stateObject, execContext);
                try
                {
                    this.InternalExecuteNonQuery(result, "BeginExecuteNonQuery", false);
                }
                catch (Exception exception4)
                {
                    if (!ADP.IsCatchableOrSecurityExceptionType(exception4))
                    {
                        throw;
                    }
                    this.PutStateObject();
                    throw;
                }
                SNIHandle target = null;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    target = SqlInternalConnection.GetBestEffortCleanupTarget(this._activeConnection);
                    this.cachedAsyncState.SetActiveConnectionAndResult(result, this._activeConnection);
                    this._stateObj.ReadSni(result, this._stateObj);
                }
                catch (OutOfMemoryException exception3)
                {
                    this._activeConnection.Abort(exception3);
                    throw;
                }
                catch (StackOverflowException exception2)
                {
                    this._activeConnection.Abort(exception2);
                    throw;
                }
                catch (ThreadAbortException exception)
                {
                    this._activeConnection.Abort(exception);
                    SqlInternalConnection.BestEffortCleanup(target);
                    throw;
                }
                catch (Exception)
                {
                    if (this._cachedAsyncState != null)
                    {
                        this._cachedAsyncState.ResetAsyncState();
                    }
                    this.PutStateObject();
                    throw;
                }
                result2 = result;
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
            }
            return result2;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginExecuteReader()
        {
            return this.BeginExecuteReader(null, null, CommandBehavior.Default);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginExecuteReader(CommandBehavior behavior)
        {
            return this.BeginExecuteReader(null, null, behavior);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginExecuteReader(AsyncCallback callback, object stateObject)
        {
            return this.BeginExecuteReader(callback, stateObject, CommandBehavior.Default);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginExecuteReader(AsyncCallback callback, object stateObject, CommandBehavior behavior)
        {
            IAsyncResult result;
            SqlConnection.ExecutePermission.Demand();
            this._pendingCancel = false;
            SqlStatistics statistics = null;
            try
            {
                statistics = SqlStatistics.StartTimer(this.Statistics);
                result = this.InternalBeginExecuteReader(callback, stateObject, behavior);
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
            }
            return result;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginExecuteXmlReader()
        {
            return this.BeginExecuteXmlReader(null, null);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginExecuteXmlReader(AsyncCallback callback, object stateObject)
        {
            IAsyncResult result2;
            SqlConnection.ExecutePermission.Demand();
            this._pendingCancel = false;
            this.ValidateAsyncCommand();
            SqlStatistics statistics = null;
            try
            {
                statistics = SqlStatistics.StartTimer(this.Statistics);
                ExecutionContext execContext = (callback == null) ? null : ExecutionContext.Capture();
                DbAsyncResult result = new DbAsyncResult(this, "EndExecuteXmlReader", callback, stateObject, execContext);
                try
                {
                    this.RunExecuteReader(CommandBehavior.SequentialAccess, RunBehavior.ReturnImmediately, true, "BeginExecuteXmlReader", result);
                }
                catch (Exception exception4)
                {
                    if (!ADP.IsCatchableOrSecurityExceptionType(exception4))
                    {
                        throw;
                    }
                    this.PutStateObject();
                    throw;
                }
                SNIHandle target = null;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    target = SqlInternalConnection.GetBestEffortCleanupTarget(this._activeConnection);
                    this.cachedAsyncState.SetActiveConnectionAndResult(result, this._activeConnection);
                    this._stateObj.ReadSni(result, this._stateObj);
                }
                catch (OutOfMemoryException exception3)
                {
                    this._activeConnection.Abort(exception3);
                    throw;
                }
                catch (StackOverflowException exception2)
                {
                    this._activeConnection.Abort(exception2);
                    throw;
                }
                catch (ThreadAbortException exception)
                {
                    this._activeConnection.Abort(exception);
                    SqlInternalConnection.BestEffortCleanup(target);
                    throw;
                }
                catch (Exception)
                {
                    if (this._cachedAsyncState != null)
                    {
                        this._cachedAsyncState.ResetAsyncState();
                    }
                    this.PutStateObject();
                    throw;
                }
                result2 = result;
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
            }
            return result2;
        }

        private _SqlRPC BuildExecute(bool inSchema)
        {
            int startCount = 1;
            int num2 = this.CountSendableParameters(this._parameters);
            _SqlRPC rpc = null;
            this.GetRPCObject(num2 + startCount, ref rpc);
            rpc.ProcID = 12;
            rpc.rpcName = "sp_execute";
            SqlParameter parameter = new SqlParameter(null, SqlDbType.Int) {
                Value = this._prepareHandle
            };
            rpc.parameters[0] = parameter;
            this.SetUpRPCParameters(rpc, startCount, inSchema, this._parameters);
            return rpc;
        }

        private void BuildExecuteSql(CommandBehavior behavior, string commandText, SqlParameterCollection parameters, ref _SqlRPC rpc)
        {
            int num;
            int num2 = this.CountSendableParameters(parameters);
            if (num2 > 0)
            {
                num = 2;
            }
            else
            {
                num = 1;
            }
            this.GetRPCObject(num2 + num, ref rpc);
            rpc.ProcID = 10;
            rpc.rpcName = "sp_executesql";
            if (commandText == null)
            {
                commandText = this.GetCommandText(behavior);
            }
            SqlParameter parameter = new SqlParameter(null, ((commandText.Length << 1) <= 0x1f40) ? SqlDbType.NVarChar : SqlDbType.NText, commandText.Length) {
                Value = commandText
            };
            rpc.parameters[0] = parameter;
            if (num2 > 0)
            {
                string str = this.BuildParamList(this._stateObj.Parser, this.BatchRPCMode ? parameters : this._parameters);
                parameter = new SqlParameter(null, ((str.Length << 1) <= 0x1f40) ? SqlDbType.NVarChar : SqlDbType.NText, str.Length) {
                    Value = str
                };
                rpc.parameters[1] = parameter;
                bool inSchema = CommandBehavior.Default != (behavior & CommandBehavior.SchemaOnly);
                this.SetUpRPCParameters(rpc, num, inSchema, parameters);
            }
        }

        internal string BuildParamList(TdsParser parser, SqlParameterCollection parameters)
        {
            StringBuilder builder = new StringBuilder();
            bool flag = false;
            bool isYukonOrNewer = parser.IsYukonOrNewer;
            int count = 0;
            count = parameters.Count;
            for (int i = 0; i < count; i++)
            {
                SqlParameter p = parameters[i];
                p.Validate(i, System.Data.CommandType.StoredProcedure == this.CommandType);
                if (ShouldSendParameter(p))
                {
                    if (flag)
                    {
                        builder.Append(',');
                    }
                    builder.Append(p.ParameterNameFixed);
                    MetaType internalMetaType = p.InternalMetaType;
                    builder.Append(" ");
                    if (internalMetaType.SqlDbType == SqlDbType.Udt)
                    {
                        string udtTypeName = p.UdtTypeName;
                        if (ADP.IsEmpty(udtTypeName))
                        {
                            throw SQL.MustSetUdtTypeNameForUdtParams();
                        }
                        builder.Append(this.ParseAndQuoteIdentifier(udtTypeName, true));
                    }
                    else if (internalMetaType.SqlDbType == SqlDbType.Structured)
                    {
                        string typeName = p.TypeName;
                        if (ADP.IsEmpty(typeName))
                        {
                            throw SQL.MustSetTypeNameForParam(internalMetaType.TypeName, p.ParameterNameFixed);
                        }
                        builder.Append(this.ParseAndQuoteIdentifier(typeName, false));
                        builder.Append(" READONLY");
                    }
                    else
                    {
                        internalMetaType = p.ValidateTypeLengths(isYukonOrNewer);
                        builder.Append(internalMetaType.TypeName);
                    }
                    flag = true;
                    if (internalMetaType.SqlDbType == SqlDbType.Decimal)
                    {
                        byte actualPrecision = p.GetActualPrecision();
                        byte actualScale = p.GetActualScale();
                        builder.Append('(');
                        if (actualPrecision == 0)
                        {
                            if (this.IsShiloh)
                            {
                                actualPrecision = 0x1d;
                            }
                            else
                            {
                                actualPrecision = 0x1c;
                            }
                        }
                        builder.Append(actualPrecision);
                        builder.Append(',');
                        builder.Append(actualScale);
                        builder.Append(')');
                    }
                    else if (internalMetaType.IsVarTime)
                    {
                        byte num6 = p.GetActualScale();
                        builder.Append('(');
                        builder.Append(num6);
                        builder.Append(')');
                    }
                    else if (((!internalMetaType.IsFixed && !internalMetaType.IsLong) && ((internalMetaType.SqlDbType != SqlDbType.Timestamp) && (internalMetaType.SqlDbType != SqlDbType.Udt))) && (SqlDbType.Structured != internalMetaType.SqlDbType))
                    {
                        int size = p.Size;
                        builder.Append('(');
                        if (internalMetaType.IsAnsiType)
                        {
                            object coercedValue = p.GetCoercedValue();
                            string str = null;
                            if ((coercedValue != null) && (DBNull.Value != coercedValue))
                            {
                                str = coercedValue as string;
                                if (str == null)
                                {
                                    SqlString str4 = (coercedValue is SqlString) ? ((SqlString) coercedValue) : SqlString.Null;
                                    if (!str4.IsNull)
                                    {
                                        str = str4.Value;
                                    }
                                }
                            }
                            if (str != null)
                            {
                                int num4 = parser.GetEncodingCharLength(str, p.GetActualSize(), p.Offset, null);
                                if (num4 > size)
                                {
                                    size = num4;
                                }
                            }
                        }
                        if (size == 0)
                        {
                            size = internalMetaType.IsSizeInCharacters ? 0xfa0 : 0x1f40;
                        }
                        builder.Append(size);
                        builder.Append(')');
                    }
                    else if ((internalMetaType.IsPlp && (internalMetaType.SqlDbType != SqlDbType.Xml)) && (internalMetaType.SqlDbType != SqlDbType.Udt))
                    {
                        builder.Append("(max) ");
                    }
                    if (p.Direction != ParameterDirection.Input)
                    {
                        builder.Append(" output");
                    }
                }
            }
            return builder.ToString();
        }

        private _SqlRPC BuildPrepare(CommandBehavior behavior)
        {
            _SqlRPC rpc = null;
            this.GetRPCObject(3, ref rpc);
            rpc.ProcID = 11;
            rpc.rpcName = "sp_prepare";
            SqlParameter parameter = new SqlParameter(null, SqlDbType.Int) {
                Direction = ParameterDirection.Output
            };
            rpc.parameters[0] = parameter;
            rpc.paramoptions[0] = 1;
            string str2 = this.BuildParamList(this._stateObj.Parser, this._parameters);
            parameter = new SqlParameter(null, ((str2.Length << 1) <= 0x1f40) ? SqlDbType.NVarChar : SqlDbType.NText, str2.Length) {
                Value = str2
            };
            rpc.parameters[1] = parameter;
            string commandText = this.GetCommandText(behavior);
            parameter = new SqlParameter(null, ((commandText.Length << 1) <= 0x1f40) ? SqlDbType.NVarChar : SqlDbType.NText, commandText.Length) {
                Value = commandText
            };
            rpc.parameters[2] = parameter;
            return rpc;
        }

        private _SqlRPC BuildPrepExec(CommandBehavior behavior)
        {
            int startCount = 3;
            int num2 = this.CountSendableParameters(this._parameters);
            _SqlRPC rpc = null;
            this.GetRPCObject(num2 + startCount, ref rpc);
            rpc.ProcID = 13;
            rpc.rpcName = "sp_prepexec";
            SqlParameter parameter = new SqlParameter(null, SqlDbType.Int) {
                Direction = ParameterDirection.InputOutput,
                Value = this._prepareHandle
            };
            rpc.parameters[0] = parameter;
            rpc.paramoptions[0] = 1;
            string str2 = this.BuildParamList(this._stateObj.Parser, this._parameters);
            parameter = new SqlParameter(null, ((str2.Length << 1) <= 0x1f40) ? SqlDbType.NVarChar : SqlDbType.NText, str2.Length) {
                Value = str2
            };
            rpc.parameters[1] = parameter;
            string commandText = this.GetCommandText(behavior);
            parameter = new SqlParameter(null, ((commandText.Length << 1) <= 0x1f40) ? SqlDbType.NVarChar : SqlDbType.NText, commandText.Length) {
                Value = commandText
            };
            rpc.parameters[2] = parameter;
            this.SetUpRPCParameters(rpc, startCount, false, this._parameters);
            return rpc;
        }

        private void BuildRPC(bool inSchema, SqlParameterCollection parameters, ref _SqlRPC rpc)
        {
            int paramCount = this.CountSendableParameters(parameters);
            this.GetRPCObject(paramCount, ref rpc);
            rpc.rpcName = this.CommandText;
            this.SetUpRPCParameters(rpc, 0, inSchema, parameters);
        }

        private _SqlRPC BuildUnprepare()
        {
            _SqlRPC rpc = null;
            this.GetRPCObject(1, ref rpc);
            rpc.ProcID = 15;
            rpc.rpcName = "sp_unprepare";
            SqlParameter parameter = new SqlParameter(null, SqlDbType.Int) {
                Value = this._prepareHandle
            };
            rpc.parameters[0] = parameter;
            return rpc;
        }

        public override void Cancel()
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<sc.SqlCommand.Cancel|API> %d#", this.ObjectID);
            SqlStatistics statistics = null;
            try
            {
                statistics = SqlStatistics.StartTimer(this.Statistics);
                if (this._activeConnection != null)
                {
                    SqlInternalConnectionTds innerConnection = this._activeConnection.InnerConnection as SqlInternalConnectionTds;
                    if (innerConnection != null)
                    {
                        lock (innerConnection)
                        {
                            if ((innerConnection == (this._activeConnection.InnerConnection as SqlInternalConnectionTds)) && (innerConnection.Parser != null))
                            {
                                SNIHandle target = null;
                                RuntimeHelpers.PrepareConstrainedRegions();
                                try
                                {
                                    target = SqlInternalConnection.GetBestEffortCleanupTarget(this._activeConnection);
                                    if (!this._pendingCancel)
                                    {
                                        this._pendingCancel = true;
                                        TdsParserStateObject obj2 = this._stateObj;
                                        if (obj2 != null)
                                        {
                                            obj2.Cancel(this.ObjectID);
                                        }
                                        else
                                        {
                                            SqlDataReader reader = innerConnection.FindLiveReader(this);
                                            if (reader != null)
                                            {
                                                reader.Cancel(this.ObjectID);
                                            }
                                        }
                                    }
                                }
                                catch (OutOfMemoryException exception3)
                                {
                                    this._activeConnection.Abort(exception3);
                                    throw;
                                }
                                catch (StackOverflowException exception2)
                                {
                                    this._activeConnection.Abort(exception2);
                                    throw;
                                }
                                catch (ThreadAbortException exception)
                                {
                                    this._activeConnection.Abort(exception);
                                    SqlInternalConnection.BestEffortCleanup(target);
                                    throw;
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
                Bid.ScopeLeave(ref ptr);
            }
        }

        private void CheckNotificationStateAndAutoEnlist()
        {
            if (this.NotificationAutoEnlist && this._activeConnection.IsYukonOrNewer)
            {
                string str = SqlNotificationContext();
                if (!ADP.IsEmpty(str))
                {
                    SqlDependency dependencyEntry = SqlDependencyPerAppDomainDispatcher.SingletonInstance.LookupDependencyEntry(str);
                    if (dependencyEntry != null)
                    {
                        dependencyEntry.AddCommandDependency(this);
                    }
                }
            }
            if ((this.Notification != null) && (this._sqlDep != null))
            {
                if (this._sqlDep.Options == null)
                {
                    SqlDependency.IdentityUserNamePair identityUser = null;
                    SqlInternalConnectionTds innerConnection = this._activeConnection.InnerConnection as SqlInternalConnectionTds;
                    if (innerConnection.Identity != null)
                    {
                        identityUser = new SqlDependency.IdentityUserNamePair(innerConnection.Identity, null);
                    }
                    else
                    {
                        identityUser = new SqlDependency.IdentityUserNamePair(null, innerConnection.ConnectionOptions.UserID);
                    }
                    this.Notification.Options = SqlDependency.GetDefaultComposedOptions(this._activeConnection.DataSource, this.InternalTdsConnection.ServerProvidedFailOverPartner, identityUser, this._activeConnection.Database);
                }
                this.Notification.UserData = this._sqlDep.ComputeHashAndAddToDispatcher(this);
                this._sqlDep.AddToServerList(this._activeConnection.DataSource);
            }
        }

        private void CheckThrowSNIException()
        {
            if ((this._stateObj != null) && (this._stateObj._error != null))
            {
                this._stateObj.Parser.Errors.Add(this._stateObj._error);
                this._stateObj._error = null;
                this._stateObj.Parser.ThrowExceptionAndWarning();
            }
        }

        internal void ClearBatchCommand()
        {
            List<_SqlRPC> list = this._RPCList;
            if (list != null)
            {
                list.Clear();
            }
            if (this._parameterCollectionList != null)
            {
                this._parameterCollectionList.Clear();
            }
            this._SqlRPCBatchArray = null;
            this._currentlyExecutingBatch = 0;
        }

        public SqlCommand Clone()
        {
            SqlCommand command = new SqlCommand(this);
            Bid.Trace("<sc.SqlCommand.Clone|API> %d#, clone=%d#\n", this.ObjectID, command.ObjectID);
            return command;
        }

        private SqlDataReader CompleteAsyncExecuteReader()
        {
            SqlDataReader cachedAsyncReader = this.cachedAsyncState.CachedAsyncReader;
            bool flag = true;
            try
            {
                this.FinishExecuteReader(cachedAsyncReader, this.cachedAsyncState.CachedRunBehavior, this.cachedAsyncState.CachedSetOptions);
            }
            catch (Exception exception)
            {
                flag = ADP.IsCatchableExceptionType(exception);
                throw;
            }
            finally
            {
                if (flag)
                {
                    this.cachedAsyncState.ResetAsyncState();
                    this.PutStateObject();
                }
            }
            return cachedAsyncReader;
        }

        private object CompleteExecuteScalar(SqlDataReader ds, bool returnSqlValue)
        {
            object obj2 = null;
            try
            {
                if (!ds.Read() || (ds.FieldCount <= 0))
                {
                    return obj2;
                }
                if (returnSqlValue)
                {
                    return ds.GetSqlValue(0);
                }
                obj2 = ds.GetValue(0);
            }
            finally
            {
                ds.Close();
            }
            return obj2;
        }

        private XmlReader CompleteXmlReader(SqlDataReader ds)
        {
            XmlReader reader = null;
            SmiExtendedMetaData[] internalSmiMetaData = ds.GetInternalSmiMetaData();
            if (((internalSmiMetaData != null) && (internalSmiMetaData.Length == 1)) && (((internalSmiMetaData[0].SqlDbType == SqlDbType.NText) || (internalSmiMetaData[0].SqlDbType == SqlDbType.NVarChar)) || (internalSmiMetaData[0].SqlDbType == SqlDbType.Xml)))
            {
                try
                {
                    reader = new SqlStream(ds, true, internalSmiMetaData[0].SqlDbType != SqlDbType.Xml).ToXmlReader();
                }
                catch (Exception exception)
                {
                    if (ADP.IsCatchableExceptionType(exception))
                    {
                        ds.Close();
                    }
                    throw;
                }
            }
            if (reader == null)
            {
                ds.Close();
                throw SQL.NonXmlResult();
            }
            return reader;
        }

        private int CountSendableParameters(SqlParameterCollection parameters)
        {
            int num2 = 0;
            if (parameters != null)
            {
                int count = parameters.Count;
                for (int i = 0; i < count; i++)
                {
                    if (ShouldSendParameter(parameters[i]))
                    {
                        num2++;
                    }
                }
            }
            return num2;
        }

        protected override DbParameter CreateDbParameter()
        {
            return this.CreateParameter();
        }

        public SqlParameter CreateParameter()
        {
            return new SqlParameter();
        }

        internal void DeriveParameters()
        {
            string[] katmaiProcParamsNames;
            bool flag;
            object obj5;
            System.Data.CommandType commandType = this.CommandType;
            if (commandType == System.Data.CommandType.Text)
            {
                throw ADP.DeriveParametersNotSupported(this);
            }
            if (commandType != System.Data.CommandType.StoredProcedure)
            {
                if (commandType == System.Data.CommandType.TableDirect)
                {
                    throw ADP.DeriveParametersNotSupported(this);
                }
                throw ADP.InvalidCommandType(this.CommandType);
            }
            this.ValidateCommand("DeriveParameters", false);
            string[] strArray2 = MultipartIdentifier.ParseMultipartIdentifier(this.CommandText, "[\"", "]\"", "SQL_SqlCommandCommandText", false);
            if ((strArray2[3] == null) || ADP.IsEmpty(strArray2[3]))
            {
                throw ADP.NoStoredProcedureExists(this.CommandText);
            }
            SqlCommand command = null;
            StringBuilder builder = new StringBuilder();
            if (!ADP.IsEmpty(strArray2[0]))
            {
                SqlCommandSet.BuildStoredProcedureName(builder, strArray2[0]);
                builder.Append(".");
            }
            if (ADP.IsEmpty(strArray2[1]))
            {
                strArray2[1] = this.Connection.Database;
            }
            SqlCommandSet.BuildStoredProcedureName(builder, strArray2[1]);
            builder.Append(".");
            if (this.Connection.IsKatmaiOrNewer)
            {
                builder.Append("[sys].[").Append("sp_procedure_params_100_managed").Append("]");
                katmaiProcParamsNames = KatmaiProcParamsNames;
                flag = true;
            }
            else
            {
                if (this.Connection.IsYukonOrNewer)
                {
                    builder.Append("[sys].[").Append("sp_procedure_params_managed").Append("]");
                }
                else
                {
                    builder.Append(".[").Append("sp_procedure_params_rowset").Append("]");
                }
                katmaiProcParamsNames = PreKatmaiProcParamsNames;
                flag = false;
            }
            command = new SqlCommand(builder.ToString(), this.Connection, this.Transaction) {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            command.Parameters.Add(new SqlParameter("@procedure_name", SqlDbType.NVarChar, 0xff));
            command.Parameters[0].Value = UnquoteProcedureName(strArray2[3], out obj5);
            if (obj5 != null)
            {
                command.Parameters.Add(new SqlParameter("@group_number", SqlDbType.Int)).Value = obj5;
            }
            if (!ADP.IsEmpty(strArray2[2]))
            {
                command.Parameters.Add(new SqlParameter("@procedure_schema", SqlDbType.NVarChar, 0xff)).Value = UnquoteProcedurePart(strArray2[2]);
            }
            SqlDataReader reader = null;
            List<SqlParameter> list = new List<SqlParameter>();
            bool flag2 = true;
            try
            {
                reader = command.ExecuteReader();
                SqlParameter item = null;
                while (reader.Read())
                {
                    item = new SqlParameter {
                        ParameterName = (string) reader[katmaiProcParamsNames[0]]
                    };
                    if (flag)
                    {
                        item.SqlDbType = (SqlDbType) ((short) reader[katmaiProcParamsNames[3]]);
                        switch (item.SqlDbType)
                        {
                            case SqlDbType.Text:
                                item.SqlDbType = SqlDbType.VarChar;
                                break;

                            case SqlDbType.Timestamp:
                            case SqlDbType.Image:
                                item.SqlDbType = SqlDbType.VarBinary;
                                break;

                            case SqlDbType.NText:
                                item.SqlDbType = SqlDbType.NVarChar;
                                break;
                        }
                    }
                    else
                    {
                        item.SqlDbType = MetaType.GetSqlDbTypeFromOleDbType((short) reader[katmaiProcParamsNames[2]], ADP.IsNull(reader[katmaiProcParamsNames[9]]) ? ADP.StrEmpty : ((string) reader[katmaiProcParamsNames[9]]));
                    }
                    object obj4 = reader[katmaiProcParamsNames[4]];
                    if (obj4 is int)
                    {
                        int num = (int) obj4;
                        if ((num == 0) && (((item.SqlDbType == SqlDbType.NVarChar) || (item.SqlDbType == SqlDbType.VarBinary)) || (item.SqlDbType == SqlDbType.VarChar)))
                        {
                            num = -1;
                        }
                        item.Size = num;
                    }
                    item.Direction = this.ParameterDirectionFromOleDbDirection((short) reader[katmaiProcParamsNames[1]]);
                    if (item.SqlDbType == SqlDbType.Decimal)
                    {
                        item.ScaleInternal = (byte) (((short) reader[katmaiProcParamsNames[6]]) & 0xff);
                        item.PrecisionInternal = (byte) (((short) reader[katmaiProcParamsNames[5]]) & 0xff);
                    }
                    if (SqlDbType.Udt == item.SqlDbType)
                    {
                        string str;
                        if (flag)
                        {
                            str = (string) reader[katmaiProcParamsNames[9]];
                        }
                        else
                        {
                            str = (string) reader[katmaiProcParamsNames[13]];
                        }
                        item.UdtTypeName = string.Concat(new object[] { reader[katmaiProcParamsNames[7]], ".", reader[katmaiProcParamsNames[8]], ".", str });
                    }
                    if (SqlDbType.Structured == item.SqlDbType)
                    {
                        item.TypeName = string.Concat(new object[] { reader[katmaiProcParamsNames[7]], ".", reader[katmaiProcParamsNames[8]], ".", reader[katmaiProcParamsNames[9]] });
                    }
                    if (SqlDbType.Xml == item.SqlDbType)
                    {
                        object obj2 = reader[katmaiProcParamsNames[10]];
                        item.XmlSchemaCollectionDatabase = ADP.IsNull(obj2) ? string.Empty : ((string) obj2);
                        obj2 = reader[katmaiProcParamsNames[11]];
                        item.XmlSchemaCollectionOwningSchema = ADP.IsNull(obj2) ? string.Empty : ((string) obj2);
                        obj2 = reader[katmaiProcParamsNames[12]];
                        item.XmlSchemaCollectionName = ADP.IsNull(obj2) ? string.Empty : ((string) obj2);
                    }
                    if (MetaType._IsVarTime(item.SqlDbType))
                    {
                        object obj3 = reader[katmaiProcParamsNames[14]];
                        if (obj3 is int)
                        {
                            item.ScaleInternal = (byte) (((int) obj3) & 0xff);
                        }
                    }
                    list.Add(item);
                }
            }
            catch (Exception exception)
            {
                flag2 = ADP.IsCatchableExceptionType(exception);
                throw;
            }
            finally
            {
                if (flag2)
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }
                    command.Connection = null;
                }
            }
            if (list.Count == 0)
            {
                throw ADP.NoStoredProcedureExists(this.CommandText);
            }
            this.Parameters.Clear();
            foreach (SqlParameter parameter2 in list)
            {
                this._parameters.Add(parameter2);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._cachedMetaData = null;
            }
            base.Dispose(disposing);
        }

        private void DisposeSmiRequest()
        {
            if (this._smiRequest != null)
            {
                SmiRequestExecutor executor = this._smiRequest;
                this._smiRequest = null;
                this._smiRequestContext = null;
                executor.Close(this.EventSink);
                this.EventSink.ProcessMessagesAndThrow();
            }
        }

        public int EndExecuteNonQuery(IAsyncResult asyncResult)
        {
            SNIHandle target = null;
            SqlStatistics statistics = null;
            int num;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                target = SqlInternalConnection.GetBestEffortCleanupTarget(this._activeConnection);
                statistics = SqlStatistics.StartTimer(this.Statistics);
                this.VerifyEndExecuteState((DbAsyncResult) asyncResult, "EndExecuteNonQuery");
                this.WaitForAsyncResults(asyncResult);
                bool flag = true;
                try
                {
                    this.NotifyDependency();
                    this.CheckThrowSNIException();
                    if ((System.Data.CommandType.Text == this.CommandType) && (this.GetParameterCount(this._parameters) == 0))
                    {
                        try
                        {
                            this._stateObj.Parser.Run(RunBehavior.UntilDone, this, null, null, this._stateObj);
                            goto Label_00B3;
                        }
                        finally
                        {
                            this.cachedAsyncState.ResetAsyncState();
                        }
                    }
                    SqlDataReader reader = this.CompleteAsyncExecuteReader();
                    if (reader != null)
                    {
                        reader.Close();
                    }
                }
                catch (Exception exception4)
                {
                    flag = ADP.IsCatchableExceptionType(exception4);
                    throw;
                }
                finally
                {
                    if (flag)
                    {
                        this.PutStateObject();
                    }
                }
            Label_00B3:
                return this._rowsAffected;
            }
            catch (OutOfMemoryException exception3)
            {
                this._activeConnection.Abort(exception3);
                throw;
            }
            catch (StackOverflowException exception2)
            {
                this._activeConnection.Abort(exception2);
                throw;
            }
            catch (ThreadAbortException exception)
            {
                this._activeConnection.Abort(exception);
                SqlInternalConnection.BestEffortCleanup(target);
                throw;
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
            }
            return num;
        }

        public SqlDataReader EndExecuteReader(IAsyncResult asyncResult)
        {
            SqlStatistics statistics = null;
            SqlDataReader reader;
            try
            {
                statistics = SqlStatistics.StartTimer(this.Statistics);
                reader = this.InternalEndExecuteReader(asyncResult, "EndExecuteReader");
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
            }
            return reader;
        }

        public XmlReader EndExecuteXmlReader(IAsyncResult asyncResult)
        {
            return this.CompleteXmlReader(this.InternalEndExecuteReader(asyncResult, "EndExecuteXmlReader"));
        }

        internal int ExecuteBatchRPCCommand()
        {
            this._SqlRPCBatchArray = this._RPCList.ToArray();
            this._currentlyExecutingBatch = 0;
            return this.ExecuteNonQuery();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return this.ExecuteReader(behavior, "ExecuteReader");
        }

        public override int ExecuteNonQuery()
        {
            int num;
            IntPtr ptr;
            SqlConnection.ExecutePermission.Demand();
            this._pendingCancel = false;
            SqlStatistics statistics = null;
            Bid.ScopeEnter(out ptr, "<sc.SqlCommand.ExecuteNonQuery|API> %d#", this.ObjectID);
            try
            {
                statistics = SqlStatistics.StartTimer(this.Statistics);
                num = this.InternalExecuteNonQuery(null, "ExecuteNonQuery", false);
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
                Bid.ScopeLeave(ref ptr);
            }
            return num;
        }

        public SqlDataReader ExecuteReader()
        {
            SqlStatistics statistics = null;
            SqlDataReader reader;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<sc.SqlCommand.ExecuteReader|API> %d#", this.ObjectID);
            try
            {
                statistics = SqlStatistics.StartTimer(this.Statistics);
                reader = this.ExecuteReader(CommandBehavior.Default, "ExecuteReader");
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
                Bid.ScopeLeave(ref ptr);
            }
            return reader;
        }

        public SqlDataReader ExecuteReader(CommandBehavior behavior)
        {
            SqlDataReader reader;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<sc.SqlCommand.ExecuteReader|API> %d#, behavior=%d{ds.CommandBehavior}", this.ObjectID, (int) behavior);
            try
            {
                reader = this.ExecuteReader(behavior, "ExecuteReader");
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return reader;
        }

        internal SqlDataReader ExecuteReader(CommandBehavior behavior, string method)
        {
            SqlDataReader reader;
            SqlConnection.ExecutePermission.Demand();
            this._pendingCancel = false;
            SqlStatistics statistics = null;
            SNIHandle target = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                target = SqlInternalConnection.GetBestEffortCleanupTarget(this._activeConnection);
                statistics = SqlStatistics.StartTimer(this.Statistics);
                return this.RunExecuteReader(behavior, RunBehavior.ReturnImmediately, true, method);
            }
            catch (OutOfMemoryException exception3)
            {
                this._activeConnection.Abort(exception3);
                throw;
            }
            catch (StackOverflowException exception2)
            {
                this._activeConnection.Abort(exception2);
                throw;
            }
            catch (ThreadAbortException exception)
            {
                this._activeConnection.Abort(exception);
                SqlInternalConnection.BestEffortCleanup(target);
                throw;
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
            }
            return reader;
        }

        public override object ExecuteScalar()
        {
            object obj2;
            IntPtr ptr;
            SqlConnection.ExecutePermission.Demand();
            this._pendingCancel = false;
            SqlStatistics statistics = null;
            Bid.ScopeEnter(out ptr, "<sc.SqlCommand.ExecuteScalar|API> %d#", this.ObjectID);
            try
            {
                statistics = SqlStatistics.StartTimer(this.Statistics);
                SqlDataReader ds = this.RunExecuteReader(CommandBehavior.Default, RunBehavior.ReturnImmediately, true, "ExecuteScalar");
                obj2 = this.CompleteExecuteScalar(ds, false);
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
                Bid.ScopeLeave(ref ptr);
            }
            return obj2;
        }

        internal void ExecuteToPipe(SmiContext pipeContext)
        {
            IntPtr ptr;
            SqlConnection.ExecutePermission.Demand();
            this._pendingCancel = false;
            SqlStatistics statistics = null;
            Bid.ScopeEnter(out ptr, "<sc.SqlCommand.ExecuteToPipe|INFO> %d#", this.ObjectID);
            try
            {
                statistics = SqlStatistics.StartTimer(this.Statistics);
                this.InternalExecuteNonQuery(null, "ExecuteNonQuery", true);
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
                Bid.ScopeLeave(ref ptr);
            }
        }

        public XmlReader ExecuteXmlReader()
        {
            XmlReader reader;
            IntPtr ptr;
            SqlConnection.ExecutePermission.Demand();
            this._pendingCancel = false;
            SqlStatistics statistics = null;
            Bid.ScopeEnter(out ptr, "<sc.SqlCommand.ExecuteXmlReader|API> %d#", this.ObjectID);
            try
            {
                statistics = SqlStatistics.StartTimer(this.Statistics);
                SqlDataReader ds = this.RunExecuteReader(CommandBehavior.SequentialAccess, RunBehavior.ReturnImmediately, true, "ExecuteXmlReader");
                reader = this.CompleteXmlReader(ds);
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
                Bid.ScopeLeave(ref ptr);
            }
            return reader;
        }

        private void FinishExecuteReader(SqlDataReader ds, RunBehavior runBehavior, string resetOptionsString)
        {
            this.NotifyDependency();
            if (runBehavior == RunBehavior.UntilDone)
            {
                try
                {
                    this._stateObj.Parser.Run(RunBehavior.UntilDone, this, ds, null, this._stateObj);
                }
                catch (Exception exception2)
                {
                    if (ADP.IsCatchableExceptionType(exception2))
                    {
                        if (this._inPrepare)
                        {
                            this._inPrepare = false;
                            this.IsDirty = true;
                            this._execType = EXECTYPE.PREPAREPENDING;
                        }
                        if (ds != null)
                        {
                            ds.Close();
                        }
                    }
                    throw;
                }
            }
            if (ds != null)
            {
                ds.Bind(this._stateObj);
                this._stateObj = null;
                ds.ResetOptionsString = resetOptionsString;
                this._activeConnection.AddWeakReference(ds, 1);
                try
                {
                    this._cachedMetaData = ds.MetaData;
                    ds.IsInitialized = true;
                }
                catch (Exception exception)
                {
                    if (ADP.IsCatchableExceptionType(exception))
                    {
                        if (this._inPrepare)
                        {
                            this._inPrepare = false;
                            this.IsDirty = true;
                            this._execType = EXECTYPE.PREPAREPENDING;
                        }
                        ds.Close();
                    }
                    throw;
                }
            }
        }

        private string GetCommandText(CommandBehavior behavior)
        {
            return (this.GetSetOptionsString(behavior) + this.CommandText);
        }

        private SqlParameterCollection GetCurrentParameterCollection()
        {
            if (!this.BatchRPCMode)
            {
                return this._parameters;
            }
            if (this._parameterCollectionList.Count > this._currentlyExecutingBatch)
            {
                return this._parameterCollectionList[this._currentlyExecutingBatch];
            }
            return null;
        }

        internal SqlException GetErrors(int commandIndex)
        {
            SqlException exception = null;
            int num3 = this._SqlRPCBatchArray[commandIndex].errorsIndexEnd - this._SqlRPCBatchArray[commandIndex].errorsIndexStart;
            if (0 >= num3)
            {
                return exception;
            }
            SqlErrorCollection errorCollection = new SqlErrorCollection();
            for (int i = this._SqlRPCBatchArray[commandIndex].errorsIndexStart; i < this._SqlRPCBatchArray[commandIndex].errorsIndexEnd; i++)
            {
                errorCollection.Add(this._SqlRPCBatchArray[commandIndex].errors[i]);
            }
            for (int j = this._SqlRPCBatchArray[commandIndex].warningsIndexStart; j < this._SqlRPCBatchArray[commandIndex].warningsIndexEnd; j++)
            {
                errorCollection.Add(this._SqlRPCBatchArray[commandIndex].warnings[j]);
            }
            return SqlException.CreateException(errorCollection, this.Connection.ServerVersion);
        }

        private int GetParameterCount(SqlParameterCollection parameters)
        {
            if (parameters == null)
            {
                return 0;
            }
            return parameters.Count;
        }

        private SqlParameter GetParameterForOutputValueExtraction(SqlParameterCollection parameters, string paramName, int paramCount)
        {
            SqlParameter parameter = null;
            bool flag = false;
            if (paramName == null)
            {
                for (int i = 0; i < paramCount; i++)
                {
                    parameter = parameters[i];
                    if (parameter.Direction == ParameterDirection.ReturnValue)
                    {
                        flag = true;
                        break;
                    }
                }
            }
            else
            {
                for (int j = 0; j < paramCount; j++)
                {
                    parameter = parameters[j];
                    if (((parameter.Direction != ParameterDirection.Input) && (parameter.Direction != ParameterDirection.ReturnValue)) && (paramName == parameter.ParameterNameFixed))
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if (flag)
            {
                return parameter;
            }
            return null;
        }

        internal int? GetRecordsAffected(int commandIndex)
        {
            return this._SqlRPCBatchArray[commandIndex].recordsAffected;
        }

        private string GetResetOptionsString(CommandBehavior behavior)
        {
            string str = null;
            if (CommandBehavior.SchemaOnly == (behavior & CommandBehavior.SchemaOnly))
            {
                str = str + " SET FMTONLY OFF;";
            }
            if (CommandBehavior.KeyInfo == (behavior & CommandBehavior.KeyInfo))
            {
                str = str + " SET NO_BROWSETABLE OFF;";
            }
            return str;
        }

        private void GetRPCObject(int paramCount, ref _SqlRPC rpc)
        {
            if (rpc == null)
            {
                if (this._rpcArrayOf1 == null)
                {
                    this._rpcArrayOf1 = new _SqlRPC[] { new _SqlRPC() };
                }
                rpc = this._rpcArrayOf1[0];
            }
            rpc.ProcID = 0;
            rpc.rpcName = null;
            rpc.options = 0;
            rpc.recordsAffected = null;
            rpc.cumulativeRecordsAffected = -1;
            rpc.errorsIndexStart = 0;
            rpc.errorsIndexEnd = 0;
            rpc.errors = null;
            rpc.warningsIndexStart = 0;
            rpc.warningsIndexEnd = 0;
            rpc.warnings = null;
            if ((rpc.parameters == null) || (rpc.parameters.Length < paramCount))
            {
                rpc.parameters = new SqlParameter[paramCount];
            }
            else if (rpc.parameters.Length > paramCount)
            {
                rpc.parameters[paramCount] = null;
            }
            if ((rpc.paramoptions == null) || (rpc.paramoptions.Length < paramCount))
            {
                rpc.paramoptions = new byte[paramCount];
            }
            else
            {
                for (int i = 0; i < paramCount; i++)
                {
                    rpc.paramoptions[i] = 0;
                }
            }
        }

        private string GetSetOptionsString(CommandBehavior behavior)
        {
            string str = null;
            if ((CommandBehavior.SchemaOnly == (behavior & CommandBehavior.SchemaOnly)) || (CommandBehavior.KeyInfo == (behavior & CommandBehavior.KeyInfo)))
            {
                str = " SET FMTONLY OFF;";
                if (CommandBehavior.KeyInfo == (behavior & CommandBehavior.KeyInfo))
                {
                    str = str + " SET NO_BROWSETABLE ON;";
                }
                if (CommandBehavior.SchemaOnly == (behavior & CommandBehavior.SchemaOnly))
                {
                    str = str + " SET FMTONLY ON;";
                }
            }
            return str;
        }

        private void GetStateObject()
        {
            if (this._pendingCancel)
            {
                this._pendingCancel = false;
                throw SQL.OperationCancelled();
            }
            TdsParserStateObject session = this._activeConnection.Parser.GetSession(this);
            session.StartSession(this.ObjectID);
            this._stateObj = session;
            if (this._pendingCancel)
            {
                this._pendingCancel = false;
                throw SQL.OperationCancelled();
            }
        }

        private IAsyncResult InternalBeginExecuteReader(AsyncCallback callback, object stateObject, CommandBehavior behavior)
        {
            ExecutionContext execContext = (callback == null) ? null : ExecutionContext.Capture();
            DbAsyncResult result = new DbAsyncResult(this, "EndExecuteReader", callback, stateObject, execContext);
            this.ValidateAsyncCommand();
            try
            {
                this.RunExecuteReader(behavior, RunBehavior.ReturnImmediately, true, "BeginExecuteReader", result);
            }
            catch (Exception exception4)
            {
                if (!ADP.IsCatchableOrSecurityExceptionType(exception4))
                {
                    throw;
                }
                this.PutStateObject();
                throw;
            }
            SNIHandle target = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                target = SqlInternalConnection.GetBestEffortCleanupTarget(this._activeConnection);
                this.cachedAsyncState.SetActiveConnectionAndResult(result, this._activeConnection);
                this._stateObj.ReadSni(result, this._stateObj);
            }
            catch (OutOfMemoryException exception3)
            {
                this._activeConnection.Abort(exception3);
                throw;
            }
            catch (StackOverflowException exception2)
            {
                this._activeConnection.Abort(exception2);
                throw;
            }
            catch (ThreadAbortException exception)
            {
                this._activeConnection.Abort(exception);
                SqlInternalConnection.BestEffortCleanup(target);
                throw;
            }
            catch (Exception)
            {
                if (this._cachedAsyncState != null)
                {
                    this._cachedAsyncState.ResetAsyncState();
                }
                this.PutStateObject();
                throw;
            }
            return result;
        }

        private SqlDataReader InternalEndExecuteReader(IAsyncResult asyncResult, string endMethod)
        {
            SqlDataReader reader;
            this.VerifyEndExecuteState((DbAsyncResult) asyncResult, endMethod);
            this.WaitForAsyncResults(asyncResult);
            this.CheckThrowSNIException();
            SNIHandle target = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                target = SqlInternalConnection.GetBestEffortCleanupTarget(this._activeConnection);
                reader = this.CompleteAsyncExecuteReader();
            }
            catch (OutOfMemoryException exception3)
            {
                this._activeConnection.Abort(exception3);
                throw;
            }
            catch (StackOverflowException exception2)
            {
                this._activeConnection.Abort(exception2);
                throw;
            }
            catch (ThreadAbortException exception)
            {
                this._activeConnection.Abort(exception);
                SqlInternalConnection.BestEffortCleanup(target);
                throw;
            }
            return reader;
        }

        private int InternalExecuteNonQuery(DbAsyncResult result, string methodName, bool sendToPipe)
        {
            int num;
            bool async = null != result;
            SqlStatistics statistics = this.Statistics;
            this._rowsAffected = -1;
            SNIHandle target = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                target = SqlInternalConnection.GetBestEffortCleanupTarget(this._activeConnection);
                this.ValidateCommand(methodName, null != result);
                this.CheckNotificationStateAndAutoEnlist();
                if (this._activeConnection.IsContextConnection)
                {
                    if (statistics != null)
                    {
                        statistics.SafeIncrement(ref statistics._unpreparedExecs);
                    }
                    this.RunExecuteNonQuerySmi(sendToPipe);
                }
                else if ((!this.BatchRPCMode && (System.Data.CommandType.Text == this.CommandType)) && (this.GetParameterCount(this._parameters) == 0))
                {
                    if (statistics != null)
                    {
                        if (!this.IsDirty && this.IsPrepared)
                        {
                            statistics.SafeIncrement(ref statistics._preparedExecs);
                        }
                        else
                        {
                            statistics.SafeIncrement(ref statistics._unpreparedExecs);
                        }
                    }
                    this.RunExecuteNonQueryTds(methodName, async);
                }
                else
                {
                    Bid.Trace("<sc.SqlCommand.ExecuteNonQuery|INFO> %d#, Command executed as RPC.\n", this.ObjectID);
                    SqlDataReader reader = this.RunExecuteReader(CommandBehavior.Default, RunBehavior.UntilDone, false, methodName, result);
                    if (reader != null)
                    {
                        reader.Close();
                    }
                }
                num = this._rowsAffected;
            }
            catch (OutOfMemoryException exception3)
            {
                this._activeConnection.Abort(exception3);
                throw;
            }
            catch (StackOverflowException exception2)
            {
                this._activeConnection.Abort(exception2);
                throw;
            }
            catch (ThreadAbortException exception)
            {
                this._activeConnection.Abort(exception);
                SqlInternalConnection.BestEffortCleanup(target);
                throw;
            }
            return num;
        }

        private SqlDataReader InternalPrepare(CommandBehavior behavior)
        {
            SqlDataReader dataStream = null;
            if (this.IsDirty)
            {
                this.Unprepare(false);
                this.IsDirty = false;
            }
            if (this._activeConnection.IsShiloh)
            {
                this._execType = EXECTYPE.PREPAREPENDING;
            }
            else
            {
                this.BuildPrepare(behavior);
                this._inPrepare = true;
                dataStream = new SqlDataReader(this, behavior);
                try
                {
                    this._stateObj.Parser.TdsExecuteRPC(this._rpcArrayOf1, this.CommandTimeout, false, null, this._stateObj, System.Data.CommandType.StoredProcedure == this.CommandType);
                    this._stateObj.Parser.Run(RunBehavior.UntilDone, this, dataStream, null, this._stateObj);
                }
                catch
                {
                    this._inPrepare = false;
                    throw;
                }
                dataStream.Bind(this._stateObj);
                this._execType = EXECTYPE.PREPARED;
                Bid.Trace("<sc.SqlCommand.Prepare|INFO> %d#, Command prepared.\n", this.ObjectID);
            }
            if (this.Statistics != null)
            {
                this.Statistics.SafeIncrement(ref this.Statistics._prepares);
            }
            this._activeConnection.AddPreparedCommand(this);
            return dataStream;
        }

        private void InternalUnprepare(bool isClosing)
        {
            if (this.IsShiloh)
            {
                this._execType = EXECTYPE.PREPAREPENDING;
                if (isClosing)
                {
                    this._prepareHandle = -1;
                }
            }
            else
            {
                if (this._prepareHandle != -1)
                {
                    this.BuildUnprepare();
                    this._stateObj.Parser.TdsExecuteRPC(this._rpcArrayOf1, this.CommandTimeout, false, null, this._stateObj, System.Data.CommandType.StoredProcedure == this.CommandType);
                    this._stateObj.Parser.Run(RunBehavior.UntilDone, this, null, null, this._stateObj);
                    this._prepareHandle = -1;
                }
                this._execType = EXECTYPE.UNPREPARED;
            }
            this._cachedMetaData = null;
            if (!isClosing)
            {
                this._activeConnection.RemovePreparedCommand(this);
            }
            Bid.Trace("<sc.SqlCommand.Prepare|INFO> %d#, Command unprepared.\n", this.ObjectID);
        }

        private void NotifyDependency()
        {
            if (this._sqlDep != null)
            {
                this._sqlDep.StartTimer(this.Notification);
            }
        }

        internal void OnDoneProc()
        {
            if (this.BatchRPCMode)
            {
                this._SqlRPCBatchArray[this._currentlyExecutingBatch].cumulativeRecordsAffected = this._rowsAffected;
                this._SqlRPCBatchArray[this._currentlyExecutingBatch].recordsAffected = new int?(((0 < this._currentlyExecutingBatch) && (0 <= this._rowsAffected)) ? (this._rowsAffected - Math.Max(this._SqlRPCBatchArray[this._currentlyExecutingBatch - 1].cumulativeRecordsAffected, 0)) : this._rowsAffected);
                this._SqlRPCBatchArray[this._currentlyExecutingBatch].errorsIndexStart = (0 < this._currentlyExecutingBatch) ? this._SqlRPCBatchArray[this._currentlyExecutingBatch - 1].errorsIndexEnd : 0;
                this._SqlRPCBatchArray[this._currentlyExecutingBatch].errorsIndexEnd = this._stateObj.Parser.Errors.Count;
                this._SqlRPCBatchArray[this._currentlyExecutingBatch].errors = this._stateObj.Parser.Errors;
                this._SqlRPCBatchArray[this._currentlyExecutingBatch].warningsIndexStart = (0 < this._currentlyExecutingBatch) ? this._SqlRPCBatchArray[this._currentlyExecutingBatch - 1].warningsIndexEnd : 0;
                this._SqlRPCBatchArray[this._currentlyExecutingBatch].warningsIndexEnd = this._stateObj.Parser.Warnings.Count;
                this._SqlRPCBatchArray[this._currentlyExecutingBatch].warnings = this._stateObj.Parser.Warnings;
                this._currentlyExecutingBatch++;
            }
        }

        internal void OnParameterAvailableSmi(SmiParameterMetaData metaData, ITypedGettersV3 parameterValues, int ordinal)
        {
            if (ParameterDirection.Input != metaData.Direction)
            {
                string paramName = null;
                if (ParameterDirection.ReturnValue != metaData.Direction)
                {
                    paramName = metaData.Name;
                }
                SqlParameterCollection currentParameterCollection = this.GetCurrentParameterCollection();
                int parameterCount = this.GetParameterCount(currentParameterCollection);
                SqlParameter parameter = this.GetParameterForOutputValueExtraction(currentParameterCollection, paramName, parameterCount);
                if (parameter != null)
                {
                    object obj2;
                    parameter.LocaleId = (int) metaData.LocaleId;
                    parameter.CompareInfo = metaData.CompareOptions;
                    SqlBuffer targetBuffer = new SqlBuffer();
                    if (this._activeConnection.IsKatmaiOrNewer)
                    {
                        obj2 = ValueUtilsSmi.GetOutputParameterV200Smi(this.OutParamEventSink, (SmiTypedGetterSetter) parameterValues, ordinal, metaData, this._smiRequestContext, targetBuffer);
                    }
                    else
                    {
                        obj2 = ValueUtilsSmi.GetOutputParameterV3Smi(this.OutParamEventSink, parameterValues, ordinal, metaData, this._smiRequestContext, targetBuffer);
                    }
                    if (obj2 != null)
                    {
                        parameter.Value = obj2;
                    }
                    else
                    {
                        parameter.SetSqlBuffer(targetBuffer);
                    }
                }
            }
        }

        internal void OnParametersAvailableSmi(SmiParameterMetaData[] paramMetaData, ITypedGettersV3 parameterValues)
        {
            for (int i = 0; i < paramMetaData.Length; i++)
            {
                this.OnParameterAvailableSmi(paramMetaData[i], parameterValues, i);
            }
        }

        internal void OnReturnStatus(int status)
        {
            if (!this._inPrepare)
            {
                SqlParameterCollection parameters = this._parameters;
                if (this.BatchRPCMode)
                {
                    if (this._parameterCollectionList.Count > this._currentlyExecutingBatch)
                    {
                        parameters = this._parameterCollectionList[this._currentlyExecutingBatch];
                    }
                    else
                    {
                        parameters = null;
                    }
                }
                int parameterCount = this.GetParameterCount(parameters);
                for (int i = 0; i < parameterCount; i++)
                {
                    SqlParameter parameter = parameters[i];
                    if (parameter.Direction == ParameterDirection.ReturnValue)
                    {
                        object obj2 = parameter.Value;
                        if ((obj2 != null) && (obj2.GetType() == typeof(SqlInt32)))
                        {
                            parameter.Value = new SqlInt32(status);
                            return;
                        }
                        parameter.Value = status;
                        return;
                    }
                }
            }
        }

        internal void OnReturnValue(SqlReturnValue rec)
        {
            if (this._inPrepare)
            {
                if (!rec.value.IsNull)
                {
                    this._prepareHandle = rec.value.Int32;
                }
                this._inPrepare = false;
            }
            else
            {
                SqlParameterCollection currentParameterCollection = this.GetCurrentParameterCollection();
                int parameterCount = this.GetParameterCount(currentParameterCollection);
                SqlParameter parameter = this.GetParameterForOutputValueExtraction(currentParameterCollection, rec.parameter, parameterCount);
                if (parameter != null)
                {
                    object obj1 = parameter.Value;
                    if (SqlDbType.Udt == parameter.SqlDbType)
                    {
                        object byteArray = null;
                        try
                        {
                            SqlConnection.CheckGetExtendedUDTInfo(rec, true);
                            if (rec.value.IsNull)
                            {
                                byteArray = DBNull.Value;
                            }
                            else
                            {
                                byteArray = rec.value.ByteArray;
                            }
                            parameter.Value = this.Connection.GetUdtValue(byteArray, rec, false);
                        }
                        catch (FileNotFoundException exception2)
                        {
                            parameter.SetUdtLoadError(exception2);
                        }
                        catch (FileLoadException exception)
                        {
                            parameter.SetUdtLoadError(exception);
                        }
                    }
                    else
                    {
                        parameter.SetSqlBuffer(rec.value);
                        MetaType metaTypeFromSqlDbType = MetaType.GetMetaTypeFromSqlDbType(rec.type, rec.isMultiValued);
                        if (rec.type == SqlDbType.Decimal)
                        {
                            parameter.ScaleInternal = rec.scale;
                            parameter.PrecisionInternal = rec.precision;
                        }
                        else if (metaTypeFromSqlDbType.IsVarTime)
                        {
                            parameter.ScaleInternal = rec.scale;
                        }
                        else if (rec.type == SqlDbType.Xml)
                        {
                            SqlCachedBuffer buffer = parameter.Value as SqlCachedBuffer;
                            if (buffer != null)
                            {
                                parameter.Value = buffer.ToString();
                            }
                        }
                        if (rec.collation != null)
                        {
                            parameter.Collation = rec.collation;
                        }
                    }
                }
            }
        }

        internal void OnStatementCompleted(int recordCount)
        {
            if (0 <= recordCount)
            {
                StatementCompletedEventHandler handler = this._statementCompletedEventHandler;
                if (handler != null)
                {
                    try
                    {
                        Bid.Trace("<sc.SqlCommand.OnStatementCompleted|INFO> %d#, recordCount=%d\n", this.ObjectID, recordCount);
                        handler(this, new StatementCompletedEventArgs(recordCount));
                    }
                    catch (Exception exception)
                    {
                        if (!ADP.IsCatchableOrSecurityExceptionType(exception))
                        {
                            throw;
                        }
                        ADP.TraceExceptionWithoutRethrow(exception);
                    }
                }
            }
        }

        private ParameterDirection ParameterDirectionFromOleDbDirection(short oledbDirection)
        {
            switch (oledbDirection)
            {
                case 2:
                    return ParameterDirection.InputOutput;

                case 3:
                    return ParameterDirection.Output;

                case 4:
                    return ParameterDirection.ReturnValue;
            }
            return ParameterDirection.Input;
        }

        private string ParseAndQuoteIdentifier(string identifier, bool isUdtTypeName)
        {
            string[] strArray = SqlParameter.ParseTypeName(identifier, isUdtTypeName);
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < strArray.Length; i++)
            {
                if (0 < builder.Length)
                {
                    builder.Append('.');
                }
                if ((strArray[i] != null) && (strArray[i].Length != 0))
                {
                    builder.Append(ADP.BuildQuotedString("[", "]", strArray[i]));
                }
            }
            return builder.ToString();
        }

        public override void Prepare()
        {
            SqlConnection.ExecutePermission.Demand();
            this._pendingCancel = false;
            if ((this._activeConnection == null) || !this._activeConnection.IsContextConnection)
            {
                IntPtr ptr;
                SqlStatistics statistics = null;
                SqlDataReader reader = null;
                Bid.ScopeEnter(out ptr, "<sc.SqlCommand.Prepare|API> %d#", this.ObjectID);
                statistics = SqlStatistics.StartTimer(this.Statistics);
                if ((this.IsPrepared && !this.IsDirty) || ((this.CommandType == System.Data.CommandType.StoredProcedure) || ((System.Data.CommandType.Text == this.CommandType) && (this.GetParameterCount(this._parameters) == 0))))
                {
                    if (this.Statistics != null)
                    {
                        this.Statistics.SafeIncrement(ref this.Statistics._prepares);
                    }
                    this._hiddenPrepare = false;
                }
                else
                {
                    bool flag = true;
                    SNIHandle target = null;
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try
                    {
                        target = SqlInternalConnection.GetBestEffortCleanupTarget(this._activeConnection);
                        this.ValidateCommand("Prepare", false);
                        this.GetStateObject();
                        if (this._parameters != null)
                        {
                            int count = this._parameters.Count;
                            for (int i = 0; i < count; i++)
                            {
                                this._parameters[i].Prepare(this);
                            }
                        }
                        reader = this.InternalPrepare(CommandBehavior.Default);
                    }
                    catch (OutOfMemoryException exception4)
                    {
                        flag = false;
                        this._activeConnection.Abort(exception4);
                        throw;
                    }
                    catch (StackOverflowException exception3)
                    {
                        flag = false;
                        this._activeConnection.Abort(exception3);
                        throw;
                    }
                    catch (ThreadAbortException exception2)
                    {
                        flag = false;
                        this._activeConnection.Abort(exception2);
                        SqlInternalConnection.BestEffortCleanup(target);
                        throw;
                    }
                    catch (Exception exception)
                    {
                        flag = ADP.IsCatchableExceptionType(exception);
                        throw;
                    }
                    finally
                    {
                        if (flag)
                        {
                            this._hiddenPrepare = false;
                            if (reader != null)
                            {
                                this._cachedMetaData = reader.MetaData;
                                reader.Close();
                            }
                            this.PutStateObject();
                        }
                    }
                }
                SqlStatistics.StopTimer(statistics);
                Bid.ScopeLeave(ref ptr);
            }
        }

        private void PropertyChanging()
        {
            this.IsDirty = true;
        }

        private void PutStateObject()
        {
            TdsParserStateObject obj2 = this._stateObj;
            this._stateObj = null;
            if (obj2 != null)
            {
                obj2.CloseSession();
            }
        }

        public void ResetCommandTimeout()
        {
            if (30 != this._commandTimeout)
            {
                this.PropertyChanging();
                this._commandTimeout = 30;
            }
        }

        private void RunExecuteNonQuerySmi(bool sendToPipe)
        {
            SqlInternalConnectionSmi internalSmiConnection = this.InternalSmiConnection;
            try
            {
                SmiExecuteType toPipe;
                this.SetUpSmiRequest(internalSmiConnection);
                if (sendToPipe)
                {
                    toPipe = SmiExecuteType.ToPipe;
                }
                else
                {
                    toPipe = SmiExecuteType.NonQuery;
                }
                SmiEventStream stream = null;
                bool flag = true;
                try
                {
                    long num;
                    System.Transactions.Transaction transaction;
                    internalSmiConnection.GetCurrentTransactionPair(out num, out transaction);
                    if (Bid.AdvancedOn)
                    {
                        Bid.Trace("<sc.SqlCommand.RunExecuteNonQuerySmi|ADV> %d#, innerConnection=%d#, transactionId=0x%I64x, cmdBehavior=%d.\n", this.ObjectID, internalSmiConnection.ObjectID, num, 0);
                    }
                    if (SmiContextFactory.Instance.NegotiatedSmiVersion >= 210L)
                    {
                        stream = this._smiRequest.Execute(internalSmiConnection.SmiConnection, num, transaction, CommandBehavior.Default, toPipe);
                    }
                    else
                    {
                        stream = this._smiRequest.Execute(internalSmiConnection.SmiConnection, num, CommandBehavior.Default, toPipe);
                    }
                    while (stream.HasEvents)
                    {
                        stream.ProcessEvent(this.EventSink);
                    }
                }
                catch (Exception exception2)
                {
                    flag = ADP.IsCatchableExceptionType(exception2);
                    throw;
                }
                finally
                {
                    if ((stream != null) && flag)
                    {
                        stream.Close(this.EventSink);
                    }
                }
                this.EventSink.ProcessMessagesAndThrow();
            }
            catch (Exception exception)
            {
                if (!ADP.IsCatchableOrSecurityExceptionType(exception))
                {
                    throw;
                }
                this.DisposeSmiRequest();
                throw;
            }
        }

        private void RunExecuteNonQueryTds(string methodName, bool async)
        {
            bool flag = true;
            try
            {
                this.GetStateObject();
                Bid.Trace("<sc.SqlCommand.ExecuteNonQuery|INFO> %d#, Command executed as SQLBATCH.\n", this.ObjectID);
                this._stateObj.Parser.TdsExecuteSQLBatch(this.CommandText, this.CommandTimeout, this.Notification, this._stateObj);
                this.NotifyDependency();
                if (async)
                {
                    this._activeConnection.GetOpenTdsConnection(methodName).IncrementAsyncCount();
                }
                else
                {
                    this._stateObj.Parser.Run(RunBehavior.UntilDone, this, null, null, this._stateObj);
                }
            }
            catch (Exception exception)
            {
                flag = ADP.IsCatchableExceptionType(exception);
                throw;
            }
            finally
            {
                if (flag && !async)
                {
                    this.PutStateObject();
                }
            }
        }

        internal SqlDataReader RunExecuteReader(CommandBehavior cmdBehavior, RunBehavior runBehavior, bool returnStream, string method)
        {
            return this.RunExecuteReader(cmdBehavior, runBehavior, returnStream, method, null);
        }

        internal SqlDataReader RunExecuteReader(CommandBehavior cmdBehavior, RunBehavior runBehavior, bool returnStream, string method, DbAsyncResult result)
        {
            SqlDataReader reader;
            bool async = null != result;
            this._rowsAffected = -1;
            if ((CommandBehavior.SingleRow & cmdBehavior) != CommandBehavior.Default)
            {
                cmdBehavior |= CommandBehavior.SingleResult;
            }
            this.ValidateCommand(method, null != result);
            this.CheckNotificationStateAndAutoEnlist();
            SNIHandle target = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                target = SqlInternalConnection.GetBestEffortCleanupTarget(this._activeConnection);
                SqlStatistics statistics = this.Statistics;
                if (statistics != null)
                {
                    if (((!this.IsDirty && this.IsPrepared) && !this._hiddenPrepare) || (this.IsPrepared && (this._execType == EXECTYPE.PREPAREPENDING)))
                    {
                        statistics.SafeIncrement(ref statistics._preparedExecs);
                    }
                    else
                    {
                        statistics.SafeIncrement(ref statistics._unpreparedExecs);
                    }
                }
                if (this._activeConnection.IsContextConnection)
                {
                    return this.RunExecuteReaderSmi(cmdBehavior, runBehavior, returnStream);
                }
                reader = this.RunExecuteReaderTds(cmdBehavior, runBehavior, returnStream, async);
            }
            catch (OutOfMemoryException exception3)
            {
                this._activeConnection.Abort(exception3);
                throw;
            }
            catch (StackOverflowException exception2)
            {
                this._activeConnection.Abort(exception2);
                throw;
            }
            catch (ThreadAbortException exception)
            {
                this._activeConnection.Abort(exception);
                SqlInternalConnection.BestEffortCleanup(target);
                throw;
            }
            return reader;
        }

        private SqlDataReader RunExecuteReaderSmi(CommandBehavior cmdBehavior, RunBehavior runBehavior, bool returnStream)
        {
            SqlInternalConnectionSmi internalSmiConnection = this.InternalSmiConnection;
            SmiEventStream eventStream = null;
            SqlDataReader reader = null;
            try
            {
                long num;
                System.Transactions.Transaction transaction;
                this.SetUpSmiRequest(internalSmiConnection);
                internalSmiConnection.GetCurrentTransactionPair(out num, out transaction);
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<sc.SqlCommand.RunExecuteReaderSmi|ADV> %d#, innerConnection=%d#, transactionId=0x%I64x, commandBehavior=%d.\n", this.ObjectID, internalSmiConnection.ObjectID, num, (int) cmdBehavior);
                }
                if (SmiContextFactory.Instance.NegotiatedSmiVersion >= 210L)
                {
                    eventStream = this._smiRequest.Execute(internalSmiConnection.SmiConnection, num, transaction, cmdBehavior, SmiExecuteType.Reader);
                }
                else
                {
                    eventStream = this._smiRequest.Execute(internalSmiConnection.SmiConnection, num, cmdBehavior, SmiExecuteType.Reader);
                }
                if ((runBehavior & RunBehavior.UntilDone) != ((RunBehavior) 0))
                {
                    while (eventStream.HasEvents)
                    {
                        eventStream.ProcessEvent(this.EventSink);
                    }
                    eventStream.Close(this.EventSink);
                }
                if (returnStream)
                {
                    reader = new SqlDataReaderSmi(eventStream, this, cmdBehavior, internalSmiConnection, this.EventSink);
                    reader.NextResult();
                    this._activeConnection.AddWeakReference(reader, 1);
                }
                this.EventSink.ProcessMessagesAndThrow();
            }
            catch (Exception exception)
            {
                if (!ADP.IsCatchableOrSecurityExceptionType(exception))
                {
                    throw;
                }
                if (eventStream != null)
                {
                    eventStream.Close(this.EventSink);
                }
                this.DisposeSmiRequest();
                throw;
            }
            return reader;
        }

        private SqlDataReader RunExecuteReaderTds(CommandBehavior cmdBehavior, RunBehavior runBehavior, bool returnStream, bool async)
        {
            bool inSchema = CommandBehavior.Default != (cmdBehavior & CommandBehavior.SchemaOnly);
            SqlDataReader ds = null;
            _SqlRPC rpc = null;
            string text = null;
            bool flag2 = true;
            try
            {
                this.GetStateObject();
                if (this.BatchRPCMode)
                {
                    this._stateObj.Parser.TdsExecuteRPC(this._SqlRPCBatchArray, this.CommandTimeout, inSchema, this.Notification, this._stateObj, System.Data.CommandType.StoredProcedure == this.CommandType);
                }
                else if ((System.Data.CommandType.Text == this.CommandType) && (this.GetParameterCount(this._parameters) == 0))
                {
                    if (returnStream)
                    {
                        Bid.Trace("<sc.SqlCommand.ExecuteReader|INFO> %d#, Command executed as SQLBATCH.\n", this.ObjectID);
                    }
                    string str2 = this.GetCommandText(cmdBehavior) + this.GetResetOptionsString(cmdBehavior);
                    this._stateObj.Parser.TdsExecuteSQLBatch(str2, this.CommandTimeout, this.Notification, this._stateObj);
                }
                else if (System.Data.CommandType.Text == this.CommandType)
                {
                    if (this.IsDirty)
                    {
                        if (this._execType == EXECTYPE.PREPARED)
                        {
                            this._hiddenPrepare = true;
                        }
                        this.InternalUnprepare(false);
                        this.IsDirty = false;
                    }
                    if (this._execType == EXECTYPE.PREPARED)
                    {
                        rpc = this.BuildExecute(inSchema);
                    }
                    else if (this._execType == EXECTYPE.PREPAREPENDING)
                    {
                        rpc = this.BuildPrepExec(cmdBehavior);
                        this._execType = EXECTYPE.PREPARED;
                        this._activeConnection.AddPreparedCommand(this);
                        this._inPrepare = true;
                    }
                    else
                    {
                        this.BuildExecuteSql(cmdBehavior, null, this._parameters, ref rpc);
                    }
                    if (this._activeConnection.IsShiloh)
                    {
                        rpc.options = 2;
                    }
                    if (returnStream)
                    {
                        Bid.Trace("<sc.SqlCommand.ExecuteReader|INFO> %d#, Command executed as RPC.\n", this.ObjectID);
                    }
                    this._stateObj.Parser.TdsExecuteRPC(this._rpcArrayOf1, this.CommandTimeout, inSchema, this.Notification, this._stateObj, System.Data.CommandType.StoredProcedure == this.CommandType);
                }
                else
                {
                    this.BuildRPC(inSchema, this._parameters, ref rpc);
                    text = this.GetSetOptionsString(cmdBehavior);
                    if (returnStream)
                    {
                        Bid.Trace("<sc.SqlCommand.ExecuteReader|INFO> %d#, Command executed as RPC.\n", this.ObjectID);
                    }
                    if (text != null)
                    {
                        this._stateObj.Parser.TdsExecuteSQLBatch(text, this.CommandTimeout, this.Notification, this._stateObj);
                        this._stateObj.Parser.Run(RunBehavior.UntilDone, this, null, null, this._stateObj);
                        text = this.GetResetOptionsString(cmdBehavior);
                    }
                    this._activeConnection.CheckSQLDebug();
                    this._stateObj.Parser.TdsExecuteRPC(this._rpcArrayOf1, this.CommandTimeout, inSchema, this.Notification, this._stateObj, System.Data.CommandType.StoredProcedure == this.CommandType);
                }
                if (returnStream)
                {
                    ds = new SqlDataReader(this, cmdBehavior);
                }
                if (async)
                {
                    this._activeConnection.GetOpenTdsConnection().IncrementAsyncCount();
                    this.cachedAsyncState.SetAsyncReaderState(ds, runBehavior, text);
                    return ds;
                }
                this.FinishExecuteReader(ds, runBehavior, text);
                return ds;
            }
            catch (Exception exception)
            {
                flag2 = ADP.IsCatchableExceptionType(exception);
                throw;
            }
            finally
            {
                if (flag2 && !async)
                {
                    this.PutStateObject();
                }
            }
            return ds;
        }

        private void SetUpRPCParameters(_SqlRPC rpc, int startCount, bool inSchema, SqlParameterCollection parameters)
        {
            int parameterCount = this.GetParameterCount(parameters);
            int index = startCount;
            bool isYukonOrNewer = this._activeConnection.Parser.IsYukonOrNewer;
            for (int i = 0; i < parameterCount; i++)
            {
                SqlParameter p = parameters[i];
                p.Validate(i, System.Data.CommandType.StoredProcedure == this.CommandType);
                p.ValidateTypeLengths(isYukonOrNewer);
                if (ShouldSendParameter(p))
                {
                    rpc.parameters[index] = p;
                    if ((p.Direction == ParameterDirection.InputOutput) || (p.Direction == ParameterDirection.Output))
                    {
                        rpc.paramoptions[index] = 1;
                    }
                    if (((p.Direction != ParameterDirection.Output) && (p.Value == null)) && (!inSchema || (SqlDbType.Structured == p.SqlDbType)))
                    {
                        rpc.paramoptions[index] = (byte) (rpc.paramoptions[index] | 2);
                    }
                    index++;
                }
            }
        }

        private void SetUpSmiRequest(SqlInternalConnectionSmi innerConnection)
        {
            this.DisposeSmiRequest();
            if (this.Notification != null)
            {
                throw SQL.NotificationsNotAvailableOnContextConnection();
            }
            SmiParameterMetaData[] parameterMetaData = null;
            ParameterPeekAheadValue[] valueArray = null;
            int parameterCount = this.GetParameterCount(this.Parameters);
            if (0 < parameterCount)
            {
                parameterMetaData = new SmiParameterMetaData[parameterCount];
                valueArray = new ParameterPeekAheadValue[parameterCount];
                for (int j = 0; j < parameterCount; j++)
                {
                    SqlParameter parameter2 = this.Parameters[j];
                    parameter2.Validate(j, System.Data.CommandType.StoredProcedure == this.CommandType);
                    parameterMetaData[j] = parameter2.MetaDataForSmi(out valueArray[j]);
                    if (!innerConnection.IsKatmaiOrNewer)
                    {
                        MetaType metaTypeFromSqlDbType = MetaType.GetMetaTypeFromSqlDbType(parameterMetaData[j].SqlDbType, parameterMetaData[j].IsMultiValued);
                        if (!metaTypeFromSqlDbType.Is90Supported)
                        {
                            throw ADP.VersionDoesNotSupportDataType(metaTypeFromSqlDbType.TypeName);
                        }
                    }
                }
            }
            System.Data.CommandType commandType = this.CommandType;
            this._smiRequestContext = innerConnection.InternalContext;
            this._smiRequest = this._smiRequestContext.CreateRequestExecutor(this.CommandText, commandType, parameterMetaData, this.EventSink);
            this.EventSink.ProcessMessagesAndThrow();
            for (int i = 0; i < parameterCount; i++)
            {
                if ((ParameterDirection.Output == parameterMetaData[i].Direction) || (ParameterDirection.ReturnValue == parameterMetaData[i].Direction))
                {
                    continue;
                }
                SqlParameter parameter = this.Parameters[i];
                object coercedValue = parameter.GetCoercedValue();
                ExtendedClrTypeCode typeCode = MetaDataUtilsSmi.DetermineExtendedTypeCodeForUseWithSqlDbType(parameterMetaData[i].SqlDbType, parameterMetaData[i].IsMultiValued, coercedValue, null, SmiContextFactory.Instance.NegotiatedSmiVersion);
                if ((System.Data.CommandType.StoredProcedure == commandType) && (ExtendedClrTypeCode.Empty == typeCode))
                {
                    this._smiRequest.SetDefault(i);
                    continue;
                }
                int size = parameter.Size;
                if (((size != 0) && (size != -1L)) && !parameter.SizeInferred)
                {
                    switch (parameterMetaData[i].SqlDbType)
                    {
                        case SqlDbType.NText:
                            if (size != 0x3fffffff)
                            {
                                throw SQL.ParameterSizeRestrictionFailure(i);
                            }
                            break;

                        case SqlDbType.NVarChar:
                            if (((size > 0) && (size != 0x3fffffff)) && (parameterMetaData[i].MaxLength == -1L))
                            {
                                throw SQL.ParameterSizeRestrictionFailure(i);
                            }
                            break;

                        case SqlDbType.Text:
                        case SqlDbType.Image:
                            goto Label_01E8;

                        case SqlDbType.Timestamp:
                            if (size < SmiMetaData.DefaultTimestamp.MaxLength)
                            {
                                throw SQL.ParameterSizeRestrictionFailure(i);
                            }
                            break;

                        case SqlDbType.VarBinary:
                        case SqlDbType.VarChar:
                            goto Label_020C;

                        case SqlDbType.Variant:
                            goto Label_0271;

                        case SqlDbType.Xml:
                            if ((coercedValue != null) && (ExtendedClrTypeCode.SqlXml != typeCode))
                            {
                                throw SQL.ParameterSizeRestrictionFailure(i);
                            }
                            break;
                    }
                }
                goto Label_02CF;
            Label_01E8:
                if (size == 0x7fffffff)
                {
                    goto Label_02CF;
                }
                throw SQL.ParameterSizeRestrictionFailure(i);
            Label_020C:
                if (((size <= 0) || (size == 0x7fffffff)) || (parameterMetaData[i].MaxLength != -1L))
                {
                    goto Label_02CF;
                }
                throw SQL.ParameterSizeRestrictionFailure(i);
            Label_0271:
                if (coercedValue != null)
                {
                    MetaType metaTypeFromValue = MetaType.GetMetaTypeFromValue(coercedValue);
                    if (((metaTypeFromValue.IsNCharType && (size < 0xfa0L)) || (metaTypeFromValue.IsBinType && (size < 0x1f40L))) || (metaTypeFromValue.IsAnsiType && (size < 0x1f40L)))
                    {
                        throw SQL.ParameterSizeRestrictionFailure(i);
                    }
                }
            Label_02CF:
                if (innerConnection.IsKatmaiOrNewer)
                {
                    ValueUtilsSmi.SetCompatibleValueV200(this.EventSink, this._smiRequest, i, parameterMetaData[i], coercedValue, typeCode, parameter.Offset, parameter.Size, valueArray[i]);
                }
                else
                {
                    ValueUtilsSmi.SetCompatibleValue(this.EventSink, this._smiRequest, i, parameterMetaData[i], coercedValue, typeCode, parameter.Offset);
                }
            }
        }

        private static bool ShouldSendParameter(SqlParameter p)
        {
            switch (p.Direction)
            {
                case ParameterDirection.Input:
                case ParameterDirection.Output:
                case ParameterDirection.InputOutput:
                    return true;

                case ParameterDirection.ReturnValue:
                    return false;
            }
            return false;
        }

        private bool ShouldSerializeCommandTimeout()
        {
            return (30 != this._commandTimeout);
        }

        [SecurityPermission(SecurityAction.Assert, Infrastructure=true)]
        internal static string SqlNotificationContext()
        {
            return (CallContext.GetData("MS.SqlDependencyCookie") as string);
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        internal void Unprepare(bool isClosing)
        {
            if (!this._activeConnection.IsContextConnection)
            {
                bool flag2 = false;
                bool flag = true;
                try
                {
                    if (this._stateObj == null)
                    {
                        this.GetStateObject();
                        flag2 = true;
                    }
                    this.InternalUnprepare(isClosing);
                }
                catch (Exception exception)
                {
                    flag = ADP.IsCatchableExceptionType(exception);
                    throw;
                }
                finally
                {
                    if (flag && flag2)
                    {
                        this.PutStateObject();
                    }
                }
            }
        }

        private static string UnquoteProcedureName(string name, out object groupNumber)
        {
            groupNumber = null;
            string part = name;
            if (part == null)
            {
                return part;
            }
            if (char.IsDigit(part[part.Length - 1]))
            {
                int length = part.LastIndexOf(';');
                if (length != -1)
                {
                    string s = part.Substring(length + 1);
                    int result = 0;
                    if (int.TryParse(s, out result))
                    {
                        groupNumber = result;
                        part = part.Substring(0, length);
                    }
                }
            }
            return UnquoteProcedurePart(part);
        }

        private static string UnquoteProcedurePart(string part)
        {
            if (((part != null) && (2 <= part.Length)) && (('[' == part[0]) && (']' == part[part.Length - 1])))
            {
                part = part.Substring(1, part.Length - 2);
                part = part.Replace("]]", "]");
            }
            return part;
        }

        private void ValidateAsyncCommand()
        {
            if (this.cachedAsyncState.PendingAsyncOperation)
            {
                if (this.cachedAsyncState.IsActiveConnectionValid(this._activeConnection))
                {
                    throw SQL.PendingBeginXXXExists();
                }
                this._stateObj = null;
                this.cachedAsyncState.ResetAsyncState();
            }
        }

        private void ValidateCommand(string method, bool async)
        {
            if (this._activeConnection == null)
            {
                throw ADP.ConnectionRequired(method);
            }
            SqlInternalConnectionTds innerConnection = this._activeConnection.InnerConnection as SqlInternalConnectionTds;
            if ((innerConnection != null) && (innerConnection.Parser.State != TdsParserState.OpenLoggedIn))
            {
                if (innerConnection.Parser.State == TdsParserState.Closed)
                {
                    throw ADP.OpenConnectionRequired(method, ConnectionState.Closed);
                }
                throw ADP.OpenConnectionRequired(method, ConnectionState.Broken);
            }
            this.ValidateAsyncCommand();
            SNIHandle target = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                target = SqlInternalConnection.GetBestEffortCleanupTarget(this._activeConnection);
                this._activeConnection.ValidateConnectionForExecute(method, this);
            }
            catch (OutOfMemoryException exception3)
            {
                this._activeConnection.Abort(exception3);
                throw;
            }
            catch (StackOverflowException exception2)
            {
                this._activeConnection.Abort(exception2);
                throw;
            }
            catch (ThreadAbortException exception)
            {
                this._activeConnection.Abort(exception);
                SqlInternalConnection.BestEffortCleanup(target);
                throw;
            }
            if ((this._transaction != null) && (this._transaction.Connection == null))
            {
                this._transaction = null;
            }
            if (this._activeConnection.HasLocalTransactionFromAPI && (this._transaction == null))
            {
                throw ADP.TransactionRequired(method);
            }
            if ((this._transaction != null) && (this._activeConnection != this._transaction.Connection))
            {
                throw ADP.TransactionConnectionMismatch();
            }
            if (ADP.IsEmpty(this.CommandText))
            {
                throw ADP.CommandTextRequired(method);
            }
            if ((this.Notification != null) && !this._activeConnection.IsYukonOrNewer)
            {
                throw SQL.NotificationsRequireYukon();
            }
            if (async && !this._activeConnection.Asynchronous)
            {
                throw SQL.AsyncConnectionRequired();
            }
        }

        private void VerifyEndExecuteState(DbAsyncResult dbAsyncResult, string endMethod)
        {
            if (dbAsyncResult == null)
            {
                throw ADP.ArgumentNull("asyncResult");
            }
            if (dbAsyncResult.EndMethodName != endMethod)
            {
                throw ADP.MismatchedAsyncResult(dbAsyncResult.EndMethodName, endMethod);
            }
            if (!this.cachedAsyncState.IsActiveConnectionValid(this._activeConnection))
            {
                throw ADP.CommandAsyncOperationCompleted();
            }
            dbAsyncResult.CompareExchangeOwner(this, endMethod);
        }

        private void WaitForAsyncResults(IAsyncResult asyncResult)
        {
            DbAsyncResult result = (DbAsyncResult) asyncResult;
            if (!asyncResult.IsCompleted)
            {
                asyncResult.AsyncWaitHandle.WaitOne();
            }
            result.Reset();
            this._activeConnection.GetOpenTdsConnection().DecrementAsyncCount();
        }

        internal bool BatchRPCMode
        {
            get
            {
                return this._batchRPCMode;
            }
            set
            {
                this._batchRPCMode = value;
                if (!this._batchRPCMode)
                {
                    this.ClearBatchCommand();
                }
                else
                {
                    if (this._RPCList == null)
                    {
                        this._RPCList = new List<_SqlRPC>();
                    }
                    if (this._parameterCollectionList == null)
                    {
                        this._parameterCollectionList = new List<SqlParameterCollection>();
                    }
                }
            }
        }

        private CachedAsyncState cachedAsyncState
        {
            get
            {
                if (this._cachedAsyncState == null)
                {
                    this._cachedAsyncState = new CachedAsyncState();
                }
                return this._cachedAsyncState;
            }
        }

        [System.Data.ResDescription("DbCommand_CommandText"), Editor("Microsoft.VSDesigner.Data.SQL.Design.SqlCommandTextEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultValue(""), System.Data.ResCategory("DataCategory_Data"), RefreshProperties(RefreshProperties.All)]
        public override string CommandText
        {
            get
            {
                string str = this._commandText;
                if (str == null)
                {
                    return ADP.StrEmpty;
                }
                return str;
            }
            set
            {
                if (Bid.TraceOn)
                {
                    Bid.Trace("<sc.SqlCommand.set_CommandText|API> %d#, '", this.ObjectID);
                    Bid.PutStr(value);
                    Bid.Trace("'\n");
                }
                if (ADP.SrcCompare(this._commandText, value) != 0)
                {
                    this.PropertyChanging();
                    this._commandText = value;
                }
            }
        }

        [System.Data.ResCategory("DataCategory_Data"), System.Data.ResDescription("DbCommand_CommandTimeout")]
        public override int CommandTimeout
        {
            get
            {
                return this._commandTimeout;
            }
            set
            {
                Bid.Trace("<sc.SqlCommand.set_CommandTimeout|API> %d#, %d\n", this.ObjectID, value);
                if (value < 0)
                {
                    throw ADP.InvalidCommandTimeout(value);
                }
                if (value != this._commandTimeout)
                {
                    this.PropertyChanging();
                    this._commandTimeout = value;
                }
            }
        }

        [System.Data.ResCategory("DataCategory_Data"), RefreshProperties(RefreshProperties.All), DefaultValue(1), System.Data.ResDescription("DbCommand_CommandType")]
        public override System.Data.CommandType CommandType
        {
            get
            {
                System.Data.CommandType type = this._commandType;
                if (type == ((System.Data.CommandType) 0))
                {
                    return System.Data.CommandType.Text;
                }
                return type;
            }
            set
            {
                Bid.Trace("<sc.SqlCommand.set_CommandType|API> %d#, %d{ds.CommandType}\n", this.ObjectID, (int) value);
                if (this._commandType != value)
                {
                    System.Data.CommandType type = value;
                    if ((type != System.Data.CommandType.Text) && (type != System.Data.CommandType.StoredProcedure))
                    {
                        if (type == System.Data.CommandType.TableDirect)
                        {
                            throw SQL.NotSupportedCommandType(value);
                        }
                        throw ADP.InvalidCommandType(value);
                    }
                    this.PropertyChanging();
                    this._commandType = value;
                }
            }
        }

        [DefaultValue((string) null), System.Data.ResCategory("DataCategory_Data"), System.Data.ResDescription("DbCommand_Connection"), Editor("Microsoft.VSDesigner.Data.Design.DbConnectionEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public SqlConnection Connection
        {
            get
            {
                return this._activeConnection;
            }
            set
            {
                if (((this._activeConnection != value) && (this._activeConnection != null)) && this.cachedAsyncState.PendingAsyncOperation)
                {
                    throw SQL.CannotModifyPropertyAsyncOperationInProgress("Connection");
                }
                if ((this._transaction != null) && (this._transaction.Connection == null))
                {
                    this._transaction = null;
                }
                this._activeConnection = value;
                Bid.Trace("<sc.SqlCommand.set_Connection|API> %d#, %d#\n", this.ObjectID, (value != null) ? value.ObjectID : -1);
            }
        }

        protected override System.Data.Common.DbConnection DbConnection
        {
            get
            {
                return this.Connection;
            }
            set
            {
                this.Connection = (SqlConnection) value;
            }
        }

        protected override System.Data.Common.DbParameterCollection DbParameterCollection
        {
            get
            {
                return this.Parameters;
            }
        }

        protected override System.Data.Common.DbTransaction DbTransaction
        {
            get
            {
                return this.Transaction;
            }
            set
            {
                this.Transaction = (SqlTransaction) value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DesignOnly(true), DefaultValue(true), Browsable(false)]
        public override bool DesignTimeVisible
        {
            get
            {
                return !this._designTimeInvisible;
            }
            set
            {
                this._designTimeInvisible = !value;
                TypeDescriptor.Refresh(this);
            }
        }

        private CommandEventSink EventSink
        {
            get
            {
                if (this._smiEventSink == null)
                {
                    this._smiEventSink = new CommandEventSink(this);
                }
                this._smiEventSink.Parent = this.InternalSmiConnection.CurrentEventSink;
                return this._smiEventSink;
            }
        }

        internal int InternalRecordsAffected
        {
            get
            {
                return this._rowsAffected;
            }
            set
            {
                if (-1 == this._rowsAffected)
                {
                    this._rowsAffected = value;
                }
                else if (0 < value)
                {
                    this._rowsAffected += value;
                }
            }
        }

        private SqlInternalConnectionSmi InternalSmiConnection
        {
            get
            {
                return (SqlInternalConnectionSmi) this._activeConnection.InnerConnection;
            }
        }

        private SqlInternalConnectionTds InternalTdsConnection
        {
            get
            {
                return (SqlInternalConnectionTds) this._activeConnection.InnerConnection;
            }
        }

        internal bool IsDirty
        {
            get
            {
                if (!this.IsPrepared)
                {
                    return false;
                }
                return (this._dirty || ((this._parameters != null) && this._parameters.IsDirty));
            }
            set
            {
                this._dirty = value ? this.IsPrepared : false;
                if (this._parameters != null)
                {
                    this._parameters.IsDirty = this._dirty;
                }
                this._cachedMetaData = null;
            }
        }

        private bool IsPrepared
        {
            get
            {
                return (this._execType != EXECTYPE.UNPREPARED);
            }
        }

        private bool IsShiloh
        {
            get
            {
                if (this._activeConnection == null)
                {
                    return false;
                }
                return this._activeConnection.IsShiloh;
            }
        }

        private bool IsUserPrepared
        {
            get
            {
                return ((this.IsPrepared && !this._hiddenPrepare) && !this.IsDirty);
            }
        }

        internal _SqlMetaDataSet MetaData
        {
            get
            {
                return this._cachedMetaData;
            }
        }

        [System.Data.ResCategory("DataCategory_Notification"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), System.Data.ResDescription("SqlCommand_Notification")]
        public SqlNotificationRequest Notification
        {
            get
            {
                return this._notification;
            }
            set
            {
                Bid.Trace("<sc.SqlCommand.set_Notification|API> %d#\n", this.ObjectID);
                this._sqlDep = null;
                this._notification = value;
            }
        }

        [System.Data.ResCategory("DataCategory_Notification"), System.Data.ResDescription("SqlCommand_NotificationAutoEnlist"), DefaultValue(true)]
        public bool NotificationAutoEnlist
        {
            get
            {
                return this._notificationAutoEnlist;
            }
            set
            {
                this._notificationAutoEnlist = value;
            }
        }

        private SmiEventSink_DeferedProcessing OutParamEventSink
        {
            get
            {
                if (this._outParamEventSink == null)
                {
                    this._outParamEventSink = new SmiEventSink_DeferedProcessing(this.EventSink);
                }
                else
                {
                    this._outParamEventSink.Parent = this.EventSink;
                }
                return this._outParamEventSink;
            }
        }

        [System.Data.ResDescription("DbCommand_Parameters"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), System.Data.ResCategory("DataCategory_Data")]
        public SqlParameterCollection Parameters
        {
            get
            {
                if (this._parameters == null)
                {
                    this._parameters = new SqlParameterCollection();
                }
                return this._parameters;
            }
        }

        internal SqlStatistics Statistics
        {
            get
            {
                if ((this._activeConnection != null) && this._activeConnection.StatisticsEnabled)
                {
                    return this._activeConnection.Statistics;
                }
                return null;
            }
        }

        [System.Data.ResDescription("DbCommand_Transaction"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SqlTransaction Transaction
        {
            get
            {
                if ((this._transaction != null) && (this._transaction.Connection == null))
                {
                    this._transaction = null;
                }
                return this._transaction;
            }
            set
            {
                if (((this._transaction != value) && (this._activeConnection != null)) && this.cachedAsyncState.PendingAsyncOperation)
                {
                    throw SQL.CannotModifyPropertyAsyncOperationInProgress("Transaction");
                }
                Bid.Trace("<sc.SqlCommand.set_Transaction|API> %d#\n", this.ObjectID);
                this._transaction = value;
            }
        }

        [System.Data.ResDescription("DbCommand_UpdatedRowSource"), System.Data.ResCategory("DataCategory_Update"), DefaultValue(3)]
        public override UpdateRowSource UpdatedRowSource
        {
            get
            {
                return this._updatedRowSource;
            }
            set
            {
                switch (value)
                {
                    case UpdateRowSource.None:
                    case UpdateRowSource.OutputParameters:
                    case UpdateRowSource.FirstReturnedRecord:
                    case UpdateRowSource.Both:
                        this._updatedRowSource = value;
                        return;
                }
                throw ADP.InvalidUpdateRowSource(value);
            }
        }

        private class CachedAsyncState
        {
            private int _cachedAsyncCloseCount = -1;
            private SqlConnection _cachedAsyncConnection;
            private SqlDataReader _cachedAsyncReader;
            private DbAsyncResult _cachedAsyncResult;
            private RunBehavior _cachedRunBehavior = RunBehavior.ReturnImmediately;
            private string _cachedSetOptions;

            internal CachedAsyncState()
            {
            }

            internal bool IsActiveConnectionValid(SqlConnection activeConnection)
            {
                return ((this._cachedAsyncConnection == activeConnection) && (this._cachedAsyncCloseCount == activeConnection.CloseCount));
            }

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            internal void ResetAsyncState()
            {
                this._cachedAsyncCloseCount = -1;
                this._cachedAsyncResult = null;
                if (this._cachedAsyncConnection != null)
                {
                    this._cachedAsyncConnection.AsycCommandInProgress = false;
                    this._cachedAsyncConnection = null;
                }
                this._cachedAsyncReader = null;
                this._cachedRunBehavior = RunBehavior.ReturnImmediately;
                this._cachedSetOptions = null;
            }

            internal void SetActiveConnectionAndResult(DbAsyncResult result, SqlConnection activeConnection)
            {
                this._cachedAsyncCloseCount = activeConnection.CloseCount;
                this._cachedAsyncResult = result;
                if (((activeConnection != null) && !activeConnection.Parser.MARSOn) && activeConnection.AsycCommandInProgress)
                {
                    throw SQL.MARSUnspportedOnConnection();
                }
                this._cachedAsyncConnection = activeConnection;
                this._cachedAsyncConnection.AsycCommandInProgress = true;
            }

            internal void SetAsyncReaderState(SqlDataReader ds, RunBehavior runBehavior, string optionSettings)
            {
                this._cachedAsyncReader = ds;
                this._cachedRunBehavior = runBehavior;
                this._cachedSetOptions = optionSettings;
            }

            internal SqlDataReader CachedAsyncReader
            {
                get
                {
                    return this._cachedAsyncReader;
                }
            }

            internal RunBehavior CachedRunBehavior
            {
                get
                {
                    return this._cachedRunBehavior;
                }
            }

            internal string CachedSetOptions
            {
                get
                {
                    return this._cachedSetOptions;
                }
            }

            internal bool PendingAsyncOperation
            {
                get
                {
                    return (null != this._cachedAsyncResult);
                }
            }
        }

        private sealed class CommandEventSink : SmiEventSink_Default
        {
            private SqlCommand _command;

            internal CommandEventSink(SqlCommand command)
            {
                this._command = command;
            }

            internal override void BatchCompleted()
            {
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<sc.SqlCommand.CommandEventSink.BatchCompleted|ADV> %d#.\n", this._command.ObjectID);
                }
            }

            internal override void ParameterAvailable(SmiParameterMetaData metaData, SmiTypedGetterSetter parameterValues, int ordinal)
            {
                if (Bid.AdvancedOn && (metaData != null))
                {
                    Bid.Trace("<sc.SqlCommand.CommandEventSink.ParameterAvailable|ADV> %d#, metaData[%d] is %ls%ls\n", this._command.ObjectID, ordinal, metaData.GetType().ToString(), metaData.TraceString());
                }
                this._command.OnParameterAvailableSmi(metaData, parameterValues, ordinal);
            }

            internal override void ParametersAvailable(SmiParameterMetaData[] metaData, ITypedGettersV3 parameterValues)
            {
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<sc.SqlCommand.CommandEventSink.ParametersAvailable|ADV> %d# metaData.Length=%d.\n", this._command.ObjectID, (metaData != null) ? metaData.Length : -1);
                    if (metaData != null)
                    {
                        for (int i = 0; i < metaData.Length; i++)
                        {
                            Bid.Trace("<sc.SqlCommand.CommandEventSink.ParametersAvailable|ADV> %d#, metaData[%d] is %ls%ls\n", this._command.ObjectID, i, metaData[i].GetType().ToString(), metaData[i].TraceString());
                        }
                    }
                }
                this._command.OnParametersAvailableSmi(metaData, parameterValues);
            }

            internal override void StatementCompleted(int rowsAffected)
            {
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<sc.SqlCommand.CommandEventSink.StatementCompleted|ADV> %d#, rowsAffected=%d.\n", this._command.ObjectID, rowsAffected);
                }
                this._command.InternalRecordsAffected = rowsAffected;
            }
        }

        private enum EXECTYPE
        {
            UNPREPARED,
            PREPAREPENDING,
            PREPARED
        }
    }
}

