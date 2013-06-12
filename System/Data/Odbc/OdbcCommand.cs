namespace System.Data.Odbc
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Runtime.CompilerServices;
    using System.Threading;

    [Designer("Microsoft.VSDesigner.Data.VS.OdbcCommandDesigner, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultEvent("RecordsAffected"), ToolboxItem(true)]
    public sealed class OdbcCommand : DbCommand, ICloneable
    {
        private CMDWrapper _cmdWrapper;
        private string _commandText;
        private int _commandTimeout;
        private System.Data.CommandType _commandType;
        private OdbcConnection _connection;
        private bool _designTimeInvisible;
        private bool _isPrepared;
        private static int _objectTypeCount;
        private OdbcParameterCollection _parameterCollection;
        private OdbcTransaction _transaction;
        private UpdateRowSource _updatedRowSource;
        private ConnectionState cmdState;
        internal readonly int ObjectID;
        private WeakReference weakDataReaderReference;

        public OdbcCommand()
        {
            this.ObjectID = Interlocked.Increment(ref _objectTypeCount);
            this._commandTimeout = 30;
            this._updatedRowSource = UpdateRowSource.Both;
            GC.SuppressFinalize(this);
        }

        public OdbcCommand(string cmdText) : this()
        {
            this.CommandText = cmdText;
        }

        public OdbcCommand(string cmdText, OdbcConnection connection) : this()
        {
            this.CommandText = cmdText;
            this.Connection = connection;
        }

        public OdbcCommand(string cmdText, OdbcConnection connection, OdbcTransaction transaction) : this()
        {
            this.CommandText = cmdText;
            this.Connection = connection;
            this.Transaction = transaction;
        }

        public override void Cancel()
        {
            CMDWrapper wrapper = this._cmdWrapper;
            if (wrapper != null)
            {
                wrapper.Canceling = true;
                OdbcStatementHandle statementHandle = wrapper.StatementHandle;
                if (statementHandle != null)
                {
                    lock (statementHandle)
                    {
                        ODBC32.RetCode retcode = statementHandle.Cancel();
                        switch (retcode)
                        {
                            case ODBC32.RetCode.SUCCESS:
                            case ODBC32.RetCode.SUCCESS_WITH_INFO:
                                return;
                        }
                        throw wrapper.Connection.HandleErrorNoThrow(statementHandle, retcode);
                    }
                }
            }
        }

        private void CloseCommandWrapper()
        {
            CMDWrapper wrapper = this._cmdWrapper;
            if (wrapper != null)
            {
                try
                {
                    wrapper.Dispose();
                    if (this._connection != null)
                    {
                        this._connection.RemoveWeakReference(this);
                    }
                }
                finally
                {
                    this._cmdWrapper = null;
                }
            }
        }

        internal void CloseFromConnection()
        {
            if (this._parameterCollection != null)
            {
                this._parameterCollection.RebindCollection = true;
            }
            this.DisposeDataReader();
            this.CloseCommandWrapper();
            this._isPrepared = false;
            this._transaction = null;
        }

        internal void CloseFromDataReader()
        {
            this.weakDataReaderReference = null;
            this.cmdState = ConnectionState.Closed;
        }

        protected override DbParameter CreateDbParameter()
        {
            return this.CreateParameter();
        }

        public OdbcParameter CreateParameter()
        {
            return new OdbcParameter();
        }

        internal void DisconnectFromDataReaderAndConnection()
        {
            OdbcDataReader reader = null;
            if (this.weakDataReaderReference != null)
            {
                OdbcDataReader target = (OdbcDataReader) this.weakDataReaderReference.Target;
                if (this.weakDataReaderReference.IsAlive)
                {
                    reader = target;
                }
            }
            if (reader != null)
            {
                reader.Command = null;
            }
            this._transaction = null;
            if (this._connection != null)
            {
                this._connection.RemoveWeakReference(this);
                this._connection = null;
            }
            if (reader == null)
            {
                this.CloseCommandWrapper();
            }
            this._cmdWrapper = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.DisconnectFromDataReaderAndConnection();
                this._parameterCollection = null;
                this.CommandText = null;
            }
            this._cmdWrapper = null;
            this._isPrepared = false;
            base.Dispose(disposing);
        }

        private void DisposeDataReader()
        {
            if (this.weakDataReaderReference != null)
            {
                IDisposable target = (IDisposable) this.weakDataReaderReference.Target;
                if ((target != null) && this.weakDataReaderReference.IsAlive)
                {
                    target.Dispose();
                }
                this.CloseFromDataReader();
            }
        }

        private void DisposeDeadDataReader()
        {
            if (((ConnectionState.Fetching == this.cmdState) && (this.weakDataReaderReference != null)) && !this.weakDataReaderReference.IsAlive)
            {
                if (this._cmdWrapper != null)
                {
                    this._cmdWrapper.FreeKeyInfoStatementHandle(ODBC32.STMT.CLOSE);
                    this._cmdWrapper.FreeStatementHandle(ODBC32.STMT.CLOSE);
                }
                this.CloseFromDataReader();
            }
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return this.ExecuteReader(behavior);
        }

        public override int ExecuteNonQuery()
        {
            OdbcConnection.ExecutePermission.Demand();
            using (OdbcDataReader reader = this.ExecuteReaderObject(CommandBehavior.Default, "ExecuteNonQuery", false))
            {
                reader.Close();
                return reader.RecordsAffected;
            }
        }

        public OdbcDataReader ExecuteReader()
        {
            return this.ExecuteReader(CommandBehavior.Default);
        }

        public OdbcDataReader ExecuteReader(CommandBehavior behavior)
        {
            OdbcConnection.ExecutePermission.Demand();
            return this.ExecuteReaderObject(behavior, "ExecuteReader", true);
        }

        internal OdbcDataReader ExecuteReaderFromSQLMethod(object[] methodArguments, ODBC32.SQL_API method)
        {
            return this.ExecuteReaderObject(CommandBehavior.Default, method.ToString(), true, methodArguments, method);
        }

        private OdbcDataReader ExecuteReaderObject(CommandBehavior behavior, string method, bool needReader)
        {
            if ((this.CommandText == null) || (this.CommandText.Length == 0))
            {
                throw ADP.CommandTextRequired(method);
            }
            return this.ExecuteReaderObject(behavior, method, needReader, null, ODBC32.SQL_API.SQLEXECDIRECT);
        }

        private OdbcDataReader ExecuteReaderObject(CommandBehavior behavior, string method, bool needReader, object[] methodArguments, ODBC32.SQL_API odbcApiMethod)
        {
            OdbcDataReader target = null;
            try
            {
                ODBC32.RetCode typeInfo;
                this.DisposeDeadDataReader();
                this.ValidateConnectionAndTransaction(method);
                if ((CommandBehavior.SingleRow & behavior) != CommandBehavior.Default)
                {
                    behavior |= CommandBehavior.SingleResult;
                }
                OdbcStatementHandle statementHandle = this.GetStatementHandle().StatementHandle;
                this._cmdWrapper.Canceling = false;
                if ((this.weakDataReaderReference != null) && this.weakDataReaderReference.IsAlive)
                {
                    object obj2 = this.weakDataReaderReference.Target;
                    if (((obj2 != null) && this.weakDataReaderReference.IsAlive) && !((OdbcDataReader) obj2).IsClosed)
                    {
                        throw ADP.OpenReaderExists();
                    }
                }
                target = new OdbcDataReader(this, this._cmdWrapper, behavior);
                if (!this.Connection.ProviderInfo.NoQueryTimeout)
                {
                    this.TrySetStatementAttribute(statementHandle, ODBC32.SQL_ATTR.QUERY_TIMEOUT, (IntPtr) this.CommandTimeout);
                }
                if ((needReader && this.Connection.IsV3Driver) && (!this.Connection.ProviderInfo.NoSqlSoptSSNoBrowseTable && !this.Connection.ProviderInfo.NoSqlSoptSSHiddenColumns))
                {
                    if (target.IsBehavior(CommandBehavior.KeyInfo))
                    {
                        if (!this._cmdWrapper._ssKeyInfoModeOn)
                        {
                            this.TrySetStatementAttribute(statementHandle, (ODBC32.SQL_ATTR) 0x4cc, (IntPtr) 1L);
                            this.TrySetStatementAttribute(statementHandle, ODBC32.SQL_ATTR.SQL_COPT_SS_TXN_ISOLATION, (IntPtr) 1L);
                            this._cmdWrapper._ssKeyInfoModeOff = false;
                            this._cmdWrapper._ssKeyInfoModeOn = true;
                        }
                    }
                    else if (!this._cmdWrapper._ssKeyInfoModeOff)
                    {
                        this.TrySetStatementAttribute(statementHandle, (ODBC32.SQL_ATTR) 0x4cc, (IntPtr) 0L);
                        this.TrySetStatementAttribute(statementHandle, ODBC32.SQL_ATTR.SQL_COPT_SS_TXN_ISOLATION, (IntPtr) 0L);
                        this._cmdWrapper._ssKeyInfoModeOff = true;
                        this._cmdWrapper._ssKeyInfoModeOn = false;
                    }
                }
                if (target.IsBehavior(CommandBehavior.KeyInfo) || target.IsBehavior(CommandBehavior.SchemaOnly))
                {
                    typeInfo = statementHandle.Prepare(this.CommandText);
                    if (typeInfo != ODBC32.RetCode.SUCCESS)
                    {
                        this._connection.HandleError(statementHandle, typeInfo);
                    }
                }
                bool success = false;
                CNativeBuffer parameterBuffer = this._cmdWrapper._nativeParameterBuffer;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    if ((this._parameterCollection != null) && (0 < this._parameterCollection.Count))
                    {
                        int initialSize = this._parameterCollection.CalcParameterBufferSize(this);
                        if ((parameterBuffer == null) || (parameterBuffer.Length < initialSize))
                        {
                            if (parameterBuffer != null)
                            {
                                parameterBuffer.Dispose();
                            }
                            parameterBuffer = new CNativeBuffer(initialSize);
                            this._cmdWrapper._nativeParameterBuffer = parameterBuffer;
                        }
                        else
                        {
                            parameterBuffer.ZeroMemory();
                        }
                        parameterBuffer.DangerousAddRef(ref success);
                        this._parameterCollection.Bind(this, this._cmdWrapper, parameterBuffer);
                    }
                    if (target.IsBehavior(CommandBehavior.SchemaOnly))
                    {
                        goto Label_0443;
                    }
                    if ((target.IsBehavior(CommandBehavior.KeyInfo) || target.IsBehavior(CommandBehavior.SchemaOnly)) && (this.CommandType != System.Data.CommandType.StoredProcedure))
                    {
                        short num2;
                        typeInfo = statementHandle.NumberOfResultColumns(out num2);
                        switch (typeInfo)
                        {
                            case ODBC32.RetCode.SUCCESS:
                            case ODBC32.RetCode.SUCCESS_WITH_INFO:
                                if (num2 > 0)
                                {
                                    target.GetSchemaTable();
                                }
                                goto Label_029A;
                        }
                        if (typeInfo != ODBC32.RetCode.NO_DATA)
                        {
                            this._connection.HandleError(statementHandle, typeInfo);
                        }
                    }
                Label_029A:
                    switch (odbcApiMethod)
                    {
                        case ODBC32.SQL_API.SQLEXECDIRECT:
                            if (target.IsBehavior(CommandBehavior.KeyInfo) || this._isPrepared)
                            {
                                typeInfo = statementHandle.Execute();
                            }
                            else
                            {
                                typeInfo = statementHandle.ExecuteDirect(this.CommandText);
                            }
                            break;

                        case ODBC32.SQL_API.SQLCOLUMNS:
                            typeInfo = statementHandle.Columns((string) methodArguments[0], (string) methodArguments[1], (string) methodArguments[2], (string) methodArguments[3]);
                            break;

                        case ODBC32.SQL_API.SQLSTATISTICS:
                            typeInfo = statementHandle.Statistics((string) methodArguments[0], (string) methodArguments[1], (string) methodArguments[2], (short) methodArguments[3], (short) methodArguments[4]);
                            break;

                        case ODBC32.SQL_API.SQLTABLES:
                            typeInfo = statementHandle.Tables((string) methodArguments[0], (string) methodArguments[1], (string) methodArguments[2], (string) methodArguments[3]);
                            break;

                        case ODBC32.SQL_API.SQLGETTYPEINFO:
                            typeInfo = statementHandle.GetTypeInfo((short) methodArguments[0]);
                            break;

                        case ODBC32.SQL_API.SQLPROCEDURECOLUMNS:
                            typeInfo = statementHandle.ProcedureColumns((string) methodArguments[0], (string) methodArguments[1], (string) methodArguments[2], (string) methodArguments[3]);
                            break;

                        case ODBC32.SQL_API.SQLPROCEDURES:
                            typeInfo = statementHandle.Procedures((string) methodArguments[0], (string) methodArguments[1], (string) methodArguments[2]);
                            break;

                        default:
                            throw ADP.InvalidOperation(method.ToString());
                    }
                    if ((typeInfo != ODBC32.RetCode.SUCCESS) && (ODBC32.RetCode.NO_DATA != typeInfo))
                    {
                        this._connection.HandleError(statementHandle, typeInfo);
                    }
                }
                finally
                {
                    if (success)
                    {
                        parameterBuffer.DangerousRelease();
                    }
                }
            Label_0443:
                this.weakDataReaderReference = new WeakReference(target);
                if (!target.IsBehavior(CommandBehavior.SchemaOnly))
                {
                    target.FirstResult();
                }
                this.cmdState = ConnectionState.Fetching;
            }
            finally
            {
                if (ConnectionState.Fetching != this.cmdState)
                {
                    if (target != null)
                    {
                        if (this._parameterCollection != null)
                        {
                            this._parameterCollection.ClearBindings();
                        }
                        target.Dispose();
                    }
                    if (this.cmdState != ConnectionState.Closed)
                    {
                        this.cmdState = ConnectionState.Closed;
                    }
                }
            }
            return target;
        }

        public override object ExecuteScalar()
        {
            OdbcConnection.ExecutePermission.Demand();
            object obj2 = null;
            using (IDataReader reader = this.ExecuteReaderObject(CommandBehavior.Default, "ExecuteScalar", false))
            {
                if (reader.Read() && (0 < reader.FieldCount))
                {
                    obj2 = reader.GetValue(0);
                }
                reader.Close();
            }
            return obj2;
        }

        internal OdbcDescriptorHandle GetDescriptorHandle(ODBC32.SQL_ATTR attribute)
        {
            return this._cmdWrapper.GetDescriptorHandle(attribute);
        }

        internal string GetDiagSqlState()
        {
            return this._cmdWrapper.GetDiagSqlState();
        }

        internal CMDWrapper GetStatementHandle()
        {
            if (this._cmdWrapper == null)
            {
                this._cmdWrapper = new CMDWrapper(this._connection);
                this._connection.AddWeakReference(this, 1);
            }
            if (this._cmdWrapper._dataReaderBuf == null)
            {
                this._cmdWrapper._dataReaderBuf = new CNativeBuffer(0x1000);
            }
            if (this._cmdWrapper.StatementHandle == null)
            {
                this._isPrepared = false;
                this._cmdWrapper.CreateStatementHandle();
            }
            else if ((this._parameterCollection != null) && this._parameterCollection.RebindCollection)
            {
                this._cmdWrapper.FreeStatementHandle(ODBC32.STMT.RESET_PARAMS);
            }
            return this._cmdWrapper;
        }

        public override void Prepare()
        {
            OdbcConnection.ExecutePermission.Demand();
            this.ValidateOpenConnection("Prepare");
            if ((ConnectionState.Fetching & this._connection.InternalState) != ConnectionState.Closed)
            {
                throw ADP.OpenReaderExists();
            }
            if (this.CommandType != System.Data.CommandType.TableDirect)
            {
                this.DisposeDeadDataReader();
                this.GetStatementHandle();
                OdbcStatementHandle statementHandle = this._cmdWrapper.StatementHandle;
                ODBC32.RetCode retcode = statementHandle.Prepare(this.CommandText);
                if (retcode != ODBC32.RetCode.SUCCESS)
                {
                    this._connection.HandleError(statementHandle, retcode);
                }
                this._isPrepared = true;
            }
        }

        private void PropertyChanging()
        {
            this._isPrepared = false;
        }

        internal bool RecoverFromConnection()
        {
            this.DisposeDeadDataReader();
            return (ConnectionState.Closed == this.cmdState);
        }

        public void ResetCommandTimeout()
        {
            if (30 != this._commandTimeout)
            {
                this.PropertyChanging();
                this._commandTimeout = 30;
            }
        }

        private bool ShouldSerializeCommandTimeout()
        {
            return (30 != this._commandTimeout);
        }

        object ICloneable.Clone()
        {
            OdbcCommand command = new OdbcCommand();
            Bid.Trace("<odbc.OdbcCommand.Clone|API> %d#, clone=%d#\n", this.ObjectID, command.ObjectID);
            command.CommandText = this.CommandText;
            command.CommandTimeout = this.CommandTimeout;
            command.CommandType = this.CommandType;
            command.Connection = this.Connection;
            command.Transaction = this.Transaction;
            command.UpdatedRowSource = this.UpdatedRowSource;
            if ((this._parameterCollection != null) && (0 < this.Parameters.Count))
            {
                OdbcParameterCollection parameters = command.Parameters;
                foreach (ICloneable cloneable in this.Parameters)
                {
                    parameters.Add(cloneable.Clone());
                }
            }
            return command;
        }

        private void TrySetStatementAttribute(OdbcStatementHandle stmt, ODBC32.SQL_ATTR stmtAttribute, IntPtr value)
        {
            if (stmt.SetStatementAttribute(stmtAttribute, value, ODBC32.SQL_IS.UINTEGER) == ODBC32.RetCode.ERROR)
            {
                string str;
                stmt.GetDiagnosticField(out str);
                switch (str)
                {
                    case "HYC00":
                    case "HY092":
                        this.Connection.FlagUnsupportedStmtAttr(stmtAttribute);
                        break;
                }
            }
        }

        private void ValidateConnectionAndTransaction(string method)
        {
            if (this._connection == null)
            {
                throw ADP.ConnectionRequired(method);
            }
            this._transaction = this._connection.SetStateExecuting(method, this.Transaction);
            this.cmdState = ConnectionState.Executing;
        }

        private void ValidateOpenConnection(string methodName)
        {
            OdbcConnection connection = this.Connection;
            if (connection == null)
            {
                throw ADP.ConnectionRequired(methodName);
            }
            ConnectionState state = connection.State;
            if (ConnectionState.Open != state)
            {
                throw ADP.OpenConnectionRequired(methodName, state);
            }
        }

        internal bool Canceling
        {
            get
            {
                return this._cmdWrapper.Canceling;
            }
        }

        [DefaultValue(""), RefreshProperties(RefreshProperties.All), ResDescription("DbCommand_CommandText"), Editor("Microsoft.VSDesigner.Data.Odbc.Design.OdbcCommandTextEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ResCategory("DataCategory_Data")]
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
                    Bid.Trace("<odbc.OdbcCommand.set_CommandText|API> %d#, '", this.ObjectID);
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

        [ResDescription("DbCommand_CommandTimeout"), ResCategory("DataCategory_Data")]
        public override int CommandTimeout
        {
            get
            {
                return this._commandTimeout;
            }
            set
            {
                Bid.Trace("<odbc.OdbcCommand.set_CommandTimeout|API> %d#, %d\n", this.ObjectID, value);
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

        [ResDescription("DbCommand_CommandType"), DefaultValue(1), ResCategory("DataCategory_Data"), RefreshProperties(RefreshProperties.All)]
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
                System.Data.CommandType type = value;
                if ((type != System.Data.CommandType.Text) && (type != System.Data.CommandType.StoredProcedure))
                {
                    if (type == System.Data.CommandType.TableDirect)
                    {
                        throw ODBC.NotSupportedCommandType(value);
                    }
                    throw ADP.InvalidCommandType(value);
                }
                this.PropertyChanging();
                this._commandType = value;
            }
        }

        [Editor("Microsoft.VSDesigner.Data.Design.DbConnectionEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ResDescription("DbCommand_Connection"), ResCategory("DataCategory_Behavior"), DefaultValue((string) null)]
        public OdbcConnection Connection
        {
            get
            {
                return this._connection;
            }
            set
            {
                if (value != this._connection)
                {
                    this.PropertyChanging();
                    this.DisconnectFromDataReaderAndConnection();
                    this._connection = value;
                }
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
                this.Connection = (OdbcConnection) value;
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
                this.Transaction = (OdbcTransaction) value;
            }
        }

        [Browsable(false), DefaultValue(true), DesignOnly(true), EditorBrowsable(EditorBrowsableState.Never)]
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

        internal bool HasParameters
        {
            get
            {
                return (null != this._parameterCollection);
            }
        }

        [ResCategory("DataCategory_Data"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), ResDescription("DbCommand_Parameters")]
        public OdbcParameterCollection Parameters
        {
            get
            {
                if (this._parameterCollection == null)
                {
                    this._parameterCollection = new OdbcParameterCollection();
                }
                return this._parameterCollection;
            }
        }

        [Browsable(false), ResDescription("DbCommand_Transaction"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public OdbcTransaction Transaction
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
                if (this._transaction != value)
                {
                    this.PropertyChanging();
                    this._transaction = value;
                }
            }
        }

        [ResDescription("DbCommand_UpdatedRowSource"), ResCategory("DataCategory_Update"), DefaultValue(3)]
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
    }
}

