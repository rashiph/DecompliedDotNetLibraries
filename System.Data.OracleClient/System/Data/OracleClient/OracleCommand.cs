namespace System.Data.OracleClient
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    [Designer("Microsoft.VSDesigner.Data.VS.OracleCommandDesigner, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ToolboxItem(true), Obsolete("OracleCommand has been deprecated. http://go.microsoft.com/fwlink/?LinkID=144260", false), DefaultEvent("RecordsAffected")]
    public sealed class OracleCommand : DbCommand, ICloneable
    {
        private string _commandText;
        private System.Data.CommandType _commandType;
        private OracleConnection _connection;
        private bool _designTimeInvisible;
        internal readonly int _objectID;
        private static int _objectTypeCount;
        private OracleParameterCollection _parameterCollection;
        private int _preparedAtCloseCount;
        private OciStatementHandle _preparedStatementHandle;
        private OCI.STMT _statementType;
        private OracleTransaction _transaction;
        private UpdateRowSource _updatedRowSource;

        public OracleCommand()
        {
            this._objectID = Interlocked.Increment(ref _objectTypeCount);
            this._updatedRowSource = UpdateRowSource.Both;
            GC.SuppressFinalize(this);
        }

        private OracleCommand(OracleCommand command) : this()
        {
            this.CommandText = command.CommandText;
            this.CommandType = command.CommandType;
            this.Connection = command.Connection;
            this.DesignTimeVisible = command.DesignTimeVisible;
            this.UpdatedRowSource = command.UpdatedRowSource;
            this.Transaction = command.Transaction;
            if ((command._parameterCollection != null) && (0 < command._parameterCollection.Count))
            {
                OracleParameterCollection parameters = this.Parameters;
                foreach (ICloneable cloneable in command.Parameters)
                {
                    parameters.Add(cloneable.Clone());
                }
            }
        }

        public OracleCommand(string commandText) : this()
        {
            this.CommandText = commandText;
        }

        public OracleCommand(string commandText, OracleConnection connection) : this()
        {
            this.CommandText = commandText;
            this.Connection = connection;
        }

        public OracleCommand(string commandText, OracleConnection connection, OracleTransaction tx) : this()
        {
            this.CommandText = commandText;
            this.Connection = connection;
            this.Transaction = tx;
        }

        public override void Cancel()
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ora.OracleCommand.Cancel|API> %d#\n", this.ObjectID);
            try
            {
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        public object Clone()
        {
            OracleCommand command = new OracleCommand(this);
            Bid.Trace("<ora.OracleCommand.Clone|API> %d#, clone=%d#\n", this.ObjectID, command.ObjectID);
            return command;
        }

        protected override DbParameter CreateDbParameter()
        {
            return this.CreateParameter();
        }

        public OracleParameter CreateParameter()
        {
            return new OracleParameter();
        }

        internal string Execute(OciStatementHandle statementHandle, CommandBehavior behavior, out ArrayList resultParameterOrdinals)
        {
            OciRowidDescriptor descriptor;
            return this.Execute(statementHandle, behavior, false, out descriptor, out resultParameterOrdinals);
        }

        internal string Execute(OciStatementHandle statementHandle, CommandBehavior behavior, bool needRowid, out OciRowidDescriptor rowidDescriptor, out ArrayList resultParameterOrdinals)
        {
            if (this.ConnectionIsClosed)
            {
                throw System.Data.Common.ADP.ClosedConnectionError();
            }
            if ((this._transaction == null) && (this.Connection.Transaction != null))
            {
                throw System.Data.Common.ADP.TransactionRequired();
            }
            if (((this._transaction != null) && (this._transaction.Connection != null)) && (this.Connection != this._transaction.Connection))
            {
                throw System.Data.Common.ADP.TransactionConnectionMismatch();
            }
            rowidDescriptor = null;
            this.Connection.RollbackDeadTransaction();
            int rc = 0;
            NativeBuffer parameterBuffer = null;
            bool success = false;
            bool[] flagArray = null;
            SafeHandle[] handleArray = null;
            OracleParameterBinding[] bindingArray = null;
            string stmt = null;
            resultParameterOrdinals = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                int num8;
                short num13;
                if (this._preparedStatementHandle != statementHandle)
                {
                    stmt = this.StatementText;
                    rc = TracedNativeMethods.OCIStmtPrepare(statementHandle, this.ErrorHandle, stmt, OCI.SYNTAX.OCI_NTV_SYNTAX, OCI.MODE.OCI_DEFAULT, this.Connection);
                    if (rc != 0)
                    {
                        this.Connection.CheckError(this.ErrorHandle, rc);
                    }
                }
                statementHandle.GetAttribute(OCI.ATTR.OCI_ATTR_STMT_TYPE, out num13, this.ErrorHandle);
                this._statementType = (OCI.STMT) num13;
                if (OCI.STMT.OCI_STMT_SELECT != this._statementType)
                {
                    num8 = 1;
                }
                else
                {
                    num8 = 0;
                    if (CommandBehavior.SingleRow != behavior)
                    {
                        statementHandle.SetAttribute(OCI.ATTR.OCI_ATTR_PREFETCH_ROWS, 0, this.ErrorHandle);
                        statementHandle.SetAttribute(OCI.ATTR.OCI_ATTR_PREFETCH_MEMORY, 0, this.ErrorHandle);
                    }
                }
                OCI.MODE mode = OCI.MODE.OCI_DEFAULT;
                if (num8 == 0)
                {
                    if (IsBehavior(behavior, CommandBehavior.SchemaOnly))
                    {
                        mode |= OCI.MODE.OCI_DESCRIBE_ONLY;
                    }
                }
                else if (this._connection.TransactionState == TransactionState.AutoCommit)
                {
                    mode |= OCI.MODE.OCI_COMMIT_ON_SUCCESS;
                }
                else if (TransactionState.GlobalStarted != this._connection.TransactionState)
                {
                    this._connection.TransactionState = TransactionState.LocalStarted;
                }
                if ((((mode & OCI.MODE.OCI_DESCRIBE_ONLY) == OCI.MODE.OCI_DEFAULT) && (this._parameterCollection != null)) && (this._parameterCollection.Count > 0))
                {
                    int offset = 0;
                    int count = this._parameterCollection.Count;
                    flagArray = new bool[count];
                    handleArray = new SafeHandle[count];
                    bindingArray = new OracleParameterBinding[count];
                    for (int i = 0; i < count; i++)
                    {
                        bindingArray[i] = new OracleParameterBinding(this, this._parameterCollection[i]);
                        bindingArray[i].PrepareForBind(this._connection, ref offset);
                        if ((OracleType.Cursor == this._parameterCollection[i].OracleType) || (0 < this._parameterCollection[i].CommandSetResult))
                        {
                            if (resultParameterOrdinals == null)
                            {
                                resultParameterOrdinals = new ArrayList();
                            }
                            resultParameterOrdinals.Add(i);
                        }
                    }
                    parameterBuffer = new NativeBuffer_ParameterBuffer(offset);
                    parameterBuffer.DangerousAddRef(ref success);
                    for (int j = 0; j < count; j++)
                    {
                        bindingArray[j].Bind(statementHandle, parameterBuffer, this._connection, ref flagArray[j], ref handleArray[j]);
                    }
                }
                rc = TracedNativeMethods.OCIStmtExecute(this.ServiceContextHandle, statementHandle, this.ErrorHandle, num8, mode);
                if (rc != 0)
                {
                    this.Connection.CheckError(this.ErrorHandle, rc);
                }
                if (bindingArray != null)
                {
                    int length = bindingArray.Length;
                    for (int k = 0; k < length; k++)
                    {
                        bindingArray[k].PostExecute(parameterBuffer, this._connection);
                        bindingArray[k].Dispose();
                        bindingArray[k] = null;
                    }
                    bindingArray = null;
                }
                if (!needRowid || ((mode & OCI.MODE.OCI_DESCRIBE_ONLY) != OCI.MODE.OCI_DEFAULT))
                {
                    return stmt;
                }
                switch (this._statementType)
                {
                    case OCI.STMT.OCI_STMT_UPDATE:
                    case OCI.STMT.OCI_STMT_DELETE:
                    case OCI.STMT.OCI_STMT_INSERT:
                        rowidDescriptor = statementHandle.GetRowid(this.EnvironmentHandle, this.ErrorHandle);
                        return stmt;
                }
                rowidDescriptor = null;
            }
            finally
            {
                if (success)
                {
                    parameterBuffer.DangerousRelease();
                }
                if (parameterBuffer != null)
                {
                    parameterBuffer.Dispose();
                    parameterBuffer = null;
                }
                if (bindingArray != null)
                {
                    int num10 = bindingArray.Length;
                    for (int m = 0; m < num10; m++)
                    {
                        if (bindingArray[m] != null)
                        {
                            bindingArray[m].Dispose();
                            bindingArray[m] = null;
                        }
                    }
                    bindingArray = null;
                }
                if ((flagArray != null) && (handleArray != null))
                {
                    int num9 = flagArray.Length;
                    for (int n = 0; n < num9; n++)
                    {
                        if (flagArray[n])
                        {
                            handleArray[n].DangerousRelease();
                        }
                    }
                }
            }
            return stmt;
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return this.ExecuteReader(behavior);
        }

        public override int ExecuteNonQuery()
        {
            int num;
            IntPtr ptr;
            OracleConnection.ExecutePermission.Demand();
            Bid.ScopeEnter(out ptr, "<ora.OracleCommand.ExecuteNonQuery|API> %d#\n", this.ObjectID);
            try
            {
                OciRowidDescriptor rowidDescriptor = null;
                int num2 = this.ExecuteNonQueryInternal(false, out rowidDescriptor);
                OciHandle.SafeDispose(ref rowidDescriptor);
                num = num2;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return num;
        }

        private int ExecuteNonQueryInternal(bool needRowid, out OciRowidDescriptor rowidDescriptor)
        {
            OciStatementHandle statementHandle = null;
            int num = -1;
            try
            {
                try
                {
                    ArrayList resultParameterOrdinals = new ArrayList();
                    statementHandle = this.GetStatementHandle();
                    this.Execute(statementHandle, CommandBehavior.Default, needRowid, out rowidDescriptor, out resultParameterOrdinals);
                    if (resultParameterOrdinals != null)
                    {
                        num = 0;
                        foreach (int num2 in resultParameterOrdinals)
                        {
                            OracleParameter parameter = this._parameterCollection[num2];
                            if (OracleType.Cursor != parameter.OracleType)
                            {
                                num += (int) parameter.Value;
                            }
                        }
                        return num;
                    }
                    if (OCI.STMT.OCI_STMT_SELECT != this._statementType)
                    {
                        statementHandle.GetAttribute(OCI.ATTR.OCI_ATTR_ROW_COUNT, out num, this.ErrorHandle);
                    }
                    return num;
                }
                finally
                {
                    if (statementHandle != null)
                    {
                        this.ReleaseStatementHandle(statementHandle);
                    }
                }
            }
            catch
            {
                throw;
            }
            return num;
        }

        public int ExecuteOracleNonQuery(out OracleString rowid)
        {
            int num;
            IntPtr ptr;
            OracleConnection.ExecutePermission.Demand();
            Bid.ScopeEnter(out ptr, "<ora.OracleCommand.ExecuteOracleNonQuery|API> %d#\n", this.ObjectID);
            try
            {
                OciRowidDescriptor rowidDescriptor = null;
                int num2 = this.ExecuteNonQueryInternal(true, out rowidDescriptor);
                rowid = GetPersistedRowid(this.Connection, rowidDescriptor);
                OciHandle.SafeDispose(ref rowidDescriptor);
                num = num2;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return num;
        }

        public object ExecuteOracleScalar()
        {
            object obj2;
            IntPtr ptr;
            OracleConnection.ExecutePermission.Demand();
            Bid.ScopeEnter(out ptr, "<ora.OracleCommand.ExecuteOracleScalar|API> %d#", this.ObjectID);
            try
            {
                OciRowidDescriptor rowidDescriptor = null;
                object obj3 = this.ExecuteScalarInternal(false, false, out rowidDescriptor);
                OciHandle.SafeDispose(ref rowidDescriptor);
                obj2 = obj3;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return obj2;
        }

        public OracleDataReader ExecuteReader()
        {
            return this.ExecuteReader(CommandBehavior.Default);
        }

        public OracleDataReader ExecuteReader(CommandBehavior behavior)
        {
            OracleDataReader reader2;
            IntPtr ptr;
            OracleConnection.ExecutePermission.Demand();
            Bid.ScopeEnter(out ptr, "<ora.OracleCommand.ExecuteReader|API> %d#, behavior=%d{ds.CommandBehavior}\n", this.ObjectID, (int) behavior);
            try
            {
                OciStatementHandle statementHandle = null;
                OracleDataReader reader = null;
                ArrayList resultParameterOrdinals = null;
                try
                {
                    statementHandle = this.GetStatementHandle();
                    string statementText = this.Execute(statementHandle, behavior, out resultParameterOrdinals);
                    if (statementHandle == this._preparedStatementHandle)
                    {
                        this._preparedStatementHandle = null;
                    }
                    if (resultParameterOrdinals == null)
                    {
                        reader = new OracleDataReader(this, statementHandle, statementText, behavior);
                    }
                    else
                    {
                        reader = new OracleDataReader(this, resultParameterOrdinals, statementText, behavior);
                    }
                }
                finally
                {
                    if ((statementHandle != null) && ((reader == null) || (resultParameterOrdinals != null)))
                    {
                        this.ReleaseStatementHandle(statementHandle);
                    }
                }
                reader2 = reader;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return reader2;
        }

        public override object ExecuteScalar()
        {
            object obj2;
            IntPtr ptr;
            OracleConnection.ExecutePermission.Demand();
            Bid.ScopeEnter(out ptr, "<ora.OracleCommand.ExecuteScalar|API> %d#\n", this.ObjectID);
            try
            {
                OciRowidDescriptor descriptor;
                object obj3 = this.ExecuteScalarInternal(true, false, out descriptor);
                OciHandle.SafeDispose(ref descriptor);
                obj2 = obj3;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return obj2;
        }

        private object ExecuteScalarInternal(bool needCLStype, bool needRowid, out OciRowidDescriptor rowidDescriptor)
        {
            OciStatementHandle statementHandle = null;
            object oracleValue = null;
            int rc = 0;
            try
            {
                statementHandle = this.GetStatementHandle();
                ArrayList resultParameterOrdinals = new ArrayList();
                this.Execute(statementHandle, CommandBehavior.Default, needRowid, out rowidDescriptor, out resultParameterOrdinals);
                if (OCI.STMT.OCI_STMT_SELECT != this._statementType)
                {
                    return oracleValue;
                }
                OracleColumn column = new OracleColumn(statementHandle, 0, this.ErrorHandle, this._connection);
                int offset = 0;
                bool success = false;
                bool mustRelease = false;
                SafeHandle handleToBind = null;
                column.Describe(ref offset, this._connection, this.ErrorHandle);
                NativeBuffer_RowBuffer buffer = new NativeBuffer_RowBuffer(offset, 1);
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    buffer.DangerousAddRef(ref success);
                    column.Bind(statementHandle, buffer, this.ErrorHandle, 0);
                    column.Rebind(this._connection, ref mustRelease, ref handleToBind);
                    rc = TracedNativeMethods.OCIStmtFetch(statementHandle, this.ErrorHandle, 1, OCI.FETCH.OCI_FETCH_NEXT, OCI.MODE.OCI_DEFAULT);
                    if (100 != rc)
                    {
                        if (rc != 0)
                        {
                            this.Connection.CheckError(this.ErrorHandle, rc);
                        }
                        if (needCLStype)
                        {
                            oracleValue = column.GetValue(buffer);
                        }
                        else
                        {
                            oracleValue = column.GetOracleValue(buffer);
                        }
                    }
                }
                finally
                {
                    if (mustRelease)
                    {
                        handleToBind.DangerousRelease();
                    }
                    if (success)
                    {
                        buffer.DangerousRelease();
                    }
                }
                GC.KeepAlive(column);
            }
            finally
            {
                if (statementHandle != null)
                {
                    this.ReleaseStatementHandle(statementHandle);
                }
            }
            return oracleValue;
        }

        internal static OracleString GetPersistedRowid(OracleConnection connection, OciRowidDescriptor rowidHandle)
        {
            OracleString @null = OracleString.Null;
            if (rowidHandle != null)
            {
                OciErrorHandle errorHandle = connection.ErrorHandle;
                NativeBuffer scratchBuffer = connection.GetScratchBuffer(0xf82);
                bool success = false;
                bool flag = false;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    int num;
                    scratchBuffer.DangerousAddRef(ref success);
                    if (OCI.ClientVersionAtLeastOracle9i)
                    {
                        int length = scratchBuffer.Length;
                        num = TracedNativeMethods.OCIRowidToChar(rowidHandle, scratchBuffer, ref length, errorHandle);
                        if (num != 0)
                        {
                            connection.CheckError(errorHandle, num);
                        }
                        return new OracleString(scratchBuffer.PtrToStringAnsi(0, length));
                    }
                    rowidHandle.DangerousAddRef(ref flag);
                    OciServiceContextHandle serviceContextHandle = connection.ServiceContextHandle;
                    OciStatementHandle stmtp = new OciStatementHandle(serviceContextHandle);
                    string stmt = "begin :rowid := :rdesc; end;";
                    int offset = 0;
                    int num6 = 4;
                    int num5 = 8;
                    int num3 = 12;
                    int num2 = 0x10;
                    int num4 = 20;
                    try
                    {
                        IntPtr ptr;
                        IntPtr ptr2;
                        num = TracedNativeMethods.OCIStmtPrepare(stmtp, errorHandle, stmt, OCI.SYNTAX.OCI_NTV_SYNTAX, OCI.MODE.OCI_DEFAULT, connection);
                        if (num != 0)
                        {
                            connection.CheckError(errorHandle, num);
                        }
                        scratchBuffer.WriteIntPtr(num5, rowidHandle.DangerousGetHandle());
                        scratchBuffer.WriteInt32(offset, 0);
                        scratchBuffer.WriteInt32(num6, 4);
                        scratchBuffer.WriteInt32(num3, 0);
                        scratchBuffer.WriteInt32(num2, 0xf6e);
                        num = TracedNativeMethods.OCIBindByName(stmtp, out ptr2, errorHandle, "rowid", 5, scratchBuffer.DangerousGetDataPtr(num4), 0xf6e, OCI.DATATYPE.VARCHAR2, scratchBuffer.DangerousGetDataPtr(num3), scratchBuffer.DangerousGetDataPtr(num2), OCI.MODE.OCI_DEFAULT);
                        if (num != 0)
                        {
                            connection.CheckError(errorHandle, num);
                        }
                        num = TracedNativeMethods.OCIBindByName(stmtp, out ptr, errorHandle, "rdesc", 5, scratchBuffer.DangerousGetDataPtr(num5), 4, OCI.DATATYPE.ROWID_DESC, scratchBuffer.DangerousGetDataPtr(offset), scratchBuffer.DangerousGetDataPtr(num6), OCI.MODE.OCI_DEFAULT);
                        if (num != 0)
                        {
                            connection.CheckError(errorHandle, num);
                        }
                        num = TracedNativeMethods.OCIStmtExecute(serviceContextHandle, stmtp, errorHandle, 1, OCI.MODE.OCI_DEFAULT);
                        if (num != 0)
                        {
                            connection.CheckError(errorHandle, num);
                        }
                        if (scratchBuffer.ReadInt16(num3) == -1)
                        {
                            return @null;
                        }
                        @null = new OracleString(scratchBuffer, num4, num2, MetaType.GetMetaTypeForType(OracleType.RowId), connection, false, true);
                        GC.KeepAlive(rowidHandle);
                    }
                    finally
                    {
                        OciHandle.SafeDispose(ref stmtp);
                    }
                }
                finally
                {
                    if (flag)
                    {
                        rowidHandle.DangerousRelease();
                    }
                    if (success)
                    {
                        scratchBuffer.DangerousRelease();
                    }
                }
            }
            return @null;
        }

        private OciStatementHandle GetStatementHandle()
        {
            if (this.ConnectionIsClosed)
            {
                throw System.Data.Common.ADP.ClosedConnectionError();
            }
            if (this._preparedStatementHandle != null)
            {
                if (this._connection.CloseCount == this._preparedAtCloseCount)
                {
                    return this._preparedStatementHandle;
                }
                this._preparedStatementHandle.Dispose();
                this._preparedStatementHandle = null;
            }
            return new OciStatementHandle(this.ServiceContextHandle);
        }

        internal static bool IsBehavior(CommandBehavior value, CommandBehavior condition)
        {
            return (condition == (condition & value));
        }

        public override void Prepare()
        {
            IntPtr ptr;
            OracleConnection.ExecutePermission.Demand();
            Bid.ScopeEnter(out ptr, "<ora.OracleCommand.Prepare|API> %d#\n", this.ObjectID);
            try
            {
                if (this.ConnectionIsClosed)
                {
                    throw System.Data.Common.ADP.ClosedConnectionError();
                }
                if (System.Data.CommandType.Text == this.CommandType)
                {
                    short num2;
                    OciStatementHandle statementHandle = this.GetStatementHandle();
                    int closeCount = this._connection.CloseCount;
                    string statementText = this.StatementText;
                    int rc = TracedNativeMethods.OCIStmtPrepare(statementHandle, this.ErrorHandle, statementText, OCI.SYNTAX.OCI_NTV_SYNTAX, OCI.MODE.OCI_DEFAULT, this.Connection);
                    if (rc != 0)
                    {
                        this.Connection.CheckError(this.ErrorHandle, rc);
                    }
                    statementHandle.GetAttribute(OCI.ATTR.OCI_ATTR_STMT_TYPE, out num2, this.ErrorHandle);
                    this._statementType = (OCI.STMT) num2;
                    if (OCI.STMT.OCI_STMT_SELECT == this._statementType)
                    {
                        rc = TracedNativeMethods.OCIStmtExecute(this._connection.ServiceContextHandle, statementHandle, this.ErrorHandle, 0, OCI.MODE.OCI_DESCRIBE_ONLY);
                        if (rc != 0)
                        {
                            this.Connection.CheckError(this.ErrorHandle, rc);
                        }
                    }
                    if (statementHandle != this._preparedStatementHandle)
                    {
                        OciHandle.SafeDispose(ref this._preparedStatementHandle);
                    }
                    this._preparedStatementHandle = statementHandle;
                    this._preparedAtCloseCount = closeCount;
                }
                else if (this._preparedStatementHandle != null)
                {
                    OciHandle.SafeDispose(ref this._preparedStatementHandle);
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        private void PropertyChanging()
        {
            if (this._preparedStatementHandle != null)
            {
                this._preparedStatementHandle.Dispose();
                this._preparedStatementHandle = null;
            }
        }

        private void ReleaseStatementHandle(OciStatementHandle statementHandle)
        {
            if ((this.Connection.State != ConnectionState.Closed) && (this._preparedStatementHandle != statementHandle))
            {
                OciHandle.SafeDispose(ref statementHandle);
            }
        }

        public void ResetCommandTimeout()
        {
        }

        private bool ShouldSerializeCommandTimeout()
        {
            return false;
        }

        [DefaultValue(""), RefreshProperties(RefreshProperties.All), System.Data.OracleClient.ResCategory("OracleCategory_Data"), Editor("Microsoft.VSDesigner.Data.Oracle.Design.OracleCommandTextEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), System.Data.OracleClient.ResDescription("DbCommand_CommandText")]
        public override string CommandText
        {
            get
            {
                string str = this._commandText;
                if (str == null)
                {
                    return System.Data.Common.ADP.StrEmpty;
                }
                return str;
            }
            set
            {
                if (Bid.TraceOn)
                {
                    Bid.Trace("<ora.OracleCommand.set_CommandText|API> %d#, '", this.ObjectID);
                    Bid.PutStr(value);
                    Bid.Trace("'\n");
                }
                if (System.Data.Common.ADP.SrcCompare(this._commandText, value) != 0)
                {
                    this.PropertyChanging();
                    this._commandText = value;
                }
            }
        }

        [System.Data.OracleClient.ResCategory("OracleCategory_Data"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never), System.Data.OracleClient.ResDescription("DbCommand_CommandTimeout")]
        public override int CommandTimeout
        {
            get
            {
                return 0;
            }
            set
            {
            }
        }

        [RefreshProperties(RefreshProperties.All), DefaultValue(1), System.Data.OracleClient.ResDescription("DbCommand_CommandType"), System.Data.OracleClient.ResCategory("OracleCategory_Data")]
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
                if (this._commandType != value)
                {
                    System.Data.CommandType type = value;
                    if ((type != System.Data.CommandType.Text) && (type != System.Data.CommandType.StoredProcedure))
                    {
                        if (type == System.Data.CommandType.TableDirect)
                        {
                            throw System.Data.Common.ADP.NoOptimizedDirectTableAccess();
                        }
                        throw System.Data.Common.ADP.InvalidCommandType(value);
                    }
                    this.PropertyChanging();
                    this._commandType = value;
                }
            }
        }

        [System.Data.OracleClient.ResCategory("OracleCategory_Behavior"), DefaultValue((string) null), System.Data.OracleClient.ResDescription("DbCommand_Connection"), Editor("Microsoft.VSDesigner.Data.Design.DbConnectionEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public OracleConnection Connection
        {
            get
            {
                return this._connection;
            }
            set
            {
                if (this._connection != value)
                {
                    this.PropertyChanging();
                    this._connection = value;
                }
            }
        }

        private bool ConnectionIsClosed
        {
            get
            {
                OracleConnection connection = this.Connection;
                if (connection != null)
                {
                    return (ConnectionState.Closed == connection.State);
                }
                return true;
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
                this.Connection = (OracleConnection) value;
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
                this.Transaction = (OracleTransaction) value;
            }
        }

        [Browsable(false), DefaultValue(true), EditorBrowsable(EditorBrowsableState.Never), DesignOnly(true)]
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

        private OciEnvironmentHandle EnvironmentHandle
        {
            get
            {
                return this._connection.EnvironmentHandle;
            }
        }

        private OciErrorHandle ErrorHandle
        {
            get
            {
                return this._connection.ErrorHandle;
            }
        }

        internal int ObjectID
        {
            get
            {
                return this._objectID;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), System.Data.OracleClient.ResDescription("DbCommand_Parameters"), System.Data.OracleClient.ResCategory("OracleCategory_Data")]
        public OracleParameterCollection Parameters
        {
            get
            {
                if (this._parameterCollection == null)
                {
                    this._parameterCollection = new OracleParameterCollection();
                }
                return this._parameterCollection;
            }
        }

        private OciServiceContextHandle ServiceContextHandle
        {
            get
            {
                return this._connection.ServiceContextHandle;
            }
        }

        internal string StatementText
        {
            get
            {
                string str2 = null;
                string commandText = this.CommandText;
                if (System.Data.Common.ADP.IsEmpty(commandText))
                {
                    throw System.Data.Common.ADP.NoCommandText();
                }
                System.Data.CommandType commandType = this.CommandType;
                if (commandType == System.Data.CommandType.Text)
                {
                    return commandText;
                }
                if (commandType != System.Data.CommandType.StoredProcedure)
                {
                    return str2;
                }
                StringBuilder builder = new StringBuilder();
                builder.Append("begin ");
                int count = this.Parameters.Count;
                int num3 = 0;
                for (int i = 0; i < count; i++)
                {
                    OracleParameter parameter2 = this.Parameters[i];
                    if (System.Data.Common.ADP.IsDirection(parameter2, ParameterDirection.ReturnValue))
                    {
                        builder.Append(":");
                        builder.Append(parameter2.ParameterName);
                        builder.Append(" := ");
                    }
                }
                builder.Append(commandText);
                string str3 = "(";
                for (int j = 0; j < count; j++)
                {
                    OracleParameter parameter = this.Parameters[j];
                    if ((!System.Data.Common.ADP.IsDirection(parameter, ParameterDirection.ReturnValue) && (System.Data.Common.ADP.IsDirection(parameter, ParameterDirection.Output) || (parameter.Value != null))) && ((parameter.Value != null) || System.Data.Common.ADP.IsDirection(parameter, ParameterDirection.Output)))
                    {
                        builder.Append(str3);
                        str3 = ", ";
                        num3++;
                        builder.Append(parameter.ParameterName);
                        builder.Append("=>:");
                        builder.Append(parameter.ParameterName);
                    }
                }
                if (num3 != 0)
                {
                    builder.Append("); end;");
                }
                else
                {
                    builder.Append("; end;");
                }
                return builder.ToString();
            }
        }

        internal OCI.STMT StatementType
        {
            get
            {
                return this._statementType;
            }
        }

        [System.Data.OracleClient.ResDescription("DbCommand_Transaction"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public OracleTransaction Transaction
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
                this._transaction = value;
            }
        }

        [System.Data.OracleClient.ResCategory("DataCategory_Update"), System.Data.OracleClient.ResDescription("DbCommand_UpdatedRowSource"), DefaultValue(3)]
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
                throw System.Data.Common.ADP.InvalidUpdateRowSource(value);
            }
        }
    }
}

