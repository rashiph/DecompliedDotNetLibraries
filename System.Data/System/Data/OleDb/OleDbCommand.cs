namespace System.Data.OleDb
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    [ToolboxItem(true), Designer("Microsoft.VSDesigner.Data.VS.OleDbCommandDesigner, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultEvent("RecordsAffected")]
    public sealed class OleDbCommand : DbCommand, ICloneable, IDbCommand, IDisposable
    {
        private int _changeID;
        private string _commandText;
        private int _commandTimeout;
        private System.Data.CommandType _commandType;
        private OleDbConnection _connection;
        private Bindings _dbBindings;
        private bool _designTimeInvisible;
        private bool _executeQuery;
        private bool _hasDataReader;
        private UnsafeNativeMethods.ICommandText _icommandText;
        private bool _isPrepared;
        private int _lastChangeID;
        private static int _objectTypeCount;
        private OleDbParameterCollection _parameters;
        private IntPtr _recordsAffected;
        private bool _trackingForClose;
        private OleDbTransaction _transaction;
        private UpdateRowSource _updatedRowSource;
        internal bool canceling;
        private CommandBehavior commandBehavior;
        internal readonly int ObjectID;

        public OleDbCommand()
        {
            this._commandTimeout = 30;
            this._updatedRowSource = UpdateRowSource.Both;
            this.ObjectID = Interlocked.Increment(ref _objectTypeCount);
            GC.SuppressFinalize(this);
        }

        private OleDbCommand(OleDbCommand from) : this()
        {
            this.CommandText = from.CommandText;
            this.CommandTimeout = from.CommandTimeout;
            this.CommandType = from.CommandType;
            this.Connection = from.Connection;
            this.DesignTimeVisible = from.DesignTimeVisible;
            this.UpdatedRowSource = from.UpdatedRowSource;
            this.Transaction = from.Transaction;
            OleDbParameterCollection parameters = this.Parameters;
            foreach (object obj2 in from.Parameters)
            {
                parameters.Add((obj2 is ICloneable) ? (obj2 as ICloneable).Clone() : obj2);
            }
        }

        public OleDbCommand(string cmdText) : this()
        {
            this.CommandText = cmdText;
        }

        public OleDbCommand(string cmdText, OleDbConnection connection) : this()
        {
            this.CommandText = cmdText;
            this.Connection = connection;
        }

        public OleDbCommand(string cmdText, OleDbConnection connection, OleDbTransaction transaction) : this()
        {
            this.CommandText = cmdText;
            this.Connection = connection;
            this.Transaction = transaction;
        }

        private void ApplyParameterBindings(System.Data.Common.UnsafeNativeMethods.ICommandWithParameters commandWithParameters, tagDBPARAMBINDINFO[] bindInfo)
        {
            IntPtr[] rgParamOrdinals = new IntPtr[bindInfo.Length];
            for (int i = 0; i < rgParamOrdinals.Length; i++)
            {
                rgParamOrdinals[i] = (IntPtr) (i + 1);
            }
            Bid.Trace("<oledb.ICommandWithParameters.SetParameterInfo|API|OLEDB> %d#\n", this.ObjectID);
            OleDbHResult result = commandWithParameters.SetParameterInfo((IntPtr) bindInfo.Length, rgParamOrdinals, bindInfo);
            Bid.Trace("<oledb.ICommandWithParameters.SetParameterInfo|API|OLEDB|RET> %08X{HRESULT}\n", result);
            if (result < OleDbHResult.S_OK)
            {
                this.ProcessResults(result);
            }
        }

        public override void Cancel()
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<oledb.OleDbCommand.Cancel|API> %d#\n", this.ObjectID);
            try
            {
                this._changeID++;
                UnsafeNativeMethods.ICommandText text = this._icommandText;
                if (text != null)
                {
                    OleDbHResult result = OleDbHResult.S_OK;
                    lock (text)
                    {
                        if (text == this._icommandText)
                        {
                            Bid.Trace("<oledb.ICommandText.Cancel|API|OLEDB> %d#\n", this.ObjectID);
                            result = text.Cancel();
                            Bid.Trace("<oledb.ICommandText.Cancel|API|OLEDB|RET> %08X{HRESULT}\n", result);
                        }
                    }
                    if (OleDbHResult.DB_E_CANTCANCEL != result)
                    {
                        this.canceling = true;
                    }
                    this.ProcessResultsNoReset(result);
                }
                else
                {
                    this.canceling = true;
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        public OleDbCommand Clone()
        {
            OleDbCommand command = new OleDbCommand(this);
            Bid.Trace("<oledb.OleDbCommand.Clone|API> %d#, clone=%d#\n", this.ObjectID, command.ObjectID);
            return command;
        }

        internal void CloseCommandFromConnection(bool canceling)
        {
            this.canceling = canceling;
            this.CloseInternal();
            this._trackingForClose = false;
            this._transaction = null;
        }

        internal void CloseFromDataReader(Bindings bindings)
        {
            if (bindings != null)
            {
                if (this.canceling)
                {
                    bindings.Dispose();
                }
                else
                {
                    bindings.ApplyOutputParameters();
                    this.ParameterBindings = bindings;
                }
            }
            this._hasDataReader = false;
        }

        internal void CloseInternal()
        {
            this.CloseInternalParameters();
            this.CloseInternalCommand();
        }

        private void CloseInternalCommand()
        {
            this._changeID++;
            this.commandBehavior = CommandBehavior.Default;
            this._isPrepared = false;
            UnsafeNativeMethods.ICommandText o = Interlocked.Exchange<UnsafeNativeMethods.ICommandText>(ref this._icommandText, null);
            if (o != null)
            {
                lock (o)
                {
                    Marshal.ReleaseComObject(o);
                }
            }
        }

        private void CloseInternalParameters()
        {
            Bindings bindings = this._dbBindings;
            this._dbBindings = null;
            if (bindings != null)
            {
                bindings.Dispose();
            }
        }

        private DBPropSet CommandPropertySets()
        {
            DBPropSet set = null;
            bool flag = CommandBehavior.Default != (CommandBehavior.KeyInfo & this.commandBehavior);
            int num = this._executeQuery ? (flag ? 4 : 2) : 1;
            if (0 < num)
            {
                set = new DBPropSet(1);
                tagDBPROP[] properties = new tagDBPROP[num];
                properties[0] = new tagDBPROP(0x22, false, this.CommandTimeout);
                if (this._executeQuery)
                {
                    properties[1] = new tagDBPROP(0xe7, false, 2);
                    if (flag)
                    {
                        properties[2] = new tagDBPROP(0xee, false, flag);
                        properties[3] = new tagDBPROP(0x7b, false, true);
                    }
                }
                set.SetPropertySet(0, OleDbPropertySetGuid.Rowset, properties);
            }
            return set;
        }

        private void CreateAccessor()
        {
            System.Data.Common.UnsafeNativeMethods.ICommandWithParameters commandWithParameters = this.ICommandWithParameters();
            OleDbParameterCollection parameters = this._parameters;
            OleDbParameter[] array = new OleDbParameter[parameters.Count];
            parameters.CopyTo(array, 0);
            Bindings bindings = new Bindings(array, parameters.ChangeID);
            for (int i = 0; i < array.Length; i++)
            {
                bindings.ForceRebind |= array[i].BindParameter(i, bindings);
            }
            bindings.AllocateForAccessor(null, 0, 0);
            this.ApplyParameterBindings(commandWithParameters, bindings.BindInfo);
            System.Data.Common.UnsafeNativeMethods.IAccessor iaccessor = this.IAccessor();
            OleDbHResult hr = bindings.CreateAccessor(iaccessor, 4);
            if (hr < OleDbHResult.S_OK)
            {
                this.ProcessResults(hr);
            }
            this._dbBindings = bindings;
        }

        protected override DbParameter CreateDbParameter()
        {
            return this.CreateParameter();
        }

        public OleDbParameter CreateParameter()
        {
            return new OleDbParameter();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._changeID++;
                this.ResetConnection();
                this._transaction = null;
                this._parameters = null;
                this.CommandText = null;
            }
            base.Dispose(disposing);
        }

        private int ExecuteCommand(CommandBehavior behavior, out object executeResult)
        {
            if (!this.InitializeCommand(behavior, false))
            {
                return this.ExecuteTableDirect(behavior, out executeResult);
            }
            if ((CommandBehavior.SchemaOnly & this.commandBehavior) != CommandBehavior.Default)
            {
                executeResult = null;
                return 3;
            }
            return this.ExecuteCommandText(out executeResult);
        }

        private int ExecuteCommandText(out object executeResult)
        {
            tagDBPARAMS dbParams = null;
            RowBinding binding = null;
            int num;
            Bindings parameterBindings = this.ParameterBindings;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                if (parameterBindings != null)
                {
                    binding = parameterBindings.RowBinding();
                    binding.DangerousAddRef(ref success);
                    parameterBindings.ApplyInputParameters();
                    dbParams = new tagDBPARAMS {
                        pData = binding.DangerousGetDataPtr(),
                        cParamSets = 1,
                        hAccessor = binding.DangerousGetAccessorHandle()
                    };
                }
                if (((CommandBehavior.SingleResult & this.commandBehavior) == CommandBehavior.Default) && this._connection.SupportMultipleResults())
                {
                    return this.ExecuteCommandTextForMultpleResults(dbParams, out executeResult);
                }
                if (((CommandBehavior.SingleRow & this.commandBehavior) == CommandBehavior.Default) || !this._executeQuery)
                {
                    return this.ExecuteCommandTextForSingleResult(dbParams, out executeResult);
                }
                num = this.ExecuteCommandTextForSingleRow(dbParams, out executeResult);
            }
            finally
            {
                if (success)
                {
                    binding.DangerousRelease();
                }
            }
            return num;
        }

        private void ExecuteCommandTextErrorHandling(OleDbHResult hr)
        {
            Exception e = OleDbConnection.ProcessResults(hr, this._connection, this);
            if (e != null)
            {
                throw this.ExecuteCommandTextSpecialErrorHandling(hr, e);
            }
        }

        private int ExecuteCommandTextForMultpleResults(tagDBPARAMS dbParams, out object executeResult)
        {
            Bid.Trace("<oledb.ICommandText.Execute|API|OLEDB> %d#, IID_IMultipleResults\n", this.ObjectID);
            OleDbHResult result = this._icommandText.Execute(ADP.PtrZero, ref ODB.IID_IMultipleResults, dbParams, out this._recordsAffected, out executeResult);
            Bid.Trace("<oledb.ICommandText.Execute|API|OLEDB|RET> %08X{HRESULT}, RecordsAffected=%Id\n", result, this._recordsAffected);
            if (OleDbHResult.E_NOINTERFACE != result)
            {
                this.ExecuteCommandTextErrorHandling(result);
                return 0;
            }
            SafeNativeMethods.Wrapper.ClearErrorInfo();
            return this.ExecuteCommandTextForSingleResult(dbParams, out executeResult);
        }

        private int ExecuteCommandTextForSingleResult(tagDBPARAMS dbParams, out object executeResult)
        {
            OleDbHResult result;
            if (this._executeQuery)
            {
                Bid.Trace("<oledb.ICommandText.Execute|API|OLEDB> %d#, IID_IRowset\n", this.ObjectID);
                result = this._icommandText.Execute(ADP.PtrZero, ref ODB.IID_IRowset, dbParams, out this._recordsAffected, out executeResult);
                Bid.Trace("<oledb.ICommandText.Execute|API|OLEDB|RET> %08X{HRESULT}, RecordsAffected=%Id\n", result, this._recordsAffected);
            }
            else
            {
                Bid.Trace("<oledb.ICommandText.Execute|API|OLEDB> %d#, IID_NULL\n", this.ObjectID);
                result = this._icommandText.Execute(ADP.PtrZero, ref ODB.IID_NULL, dbParams, out this._recordsAffected, out executeResult);
                Bid.Trace("<oledb.ICommandText.Execute|API|OLEDB|RET> %08X{HRESULT}, RecordsAffected=%Id\n", result, this._recordsAffected);
            }
            this.ExecuteCommandTextErrorHandling(result);
            return 1;
        }

        private int ExecuteCommandTextForSingleRow(tagDBPARAMS dbParams, out object executeResult)
        {
            if (this._connection.SupportIRow(this))
            {
                Bid.Trace("<oledb.ICommandText.Execute|API|OLEDB> %d#, IID_IRow\n", this.ObjectID);
                OleDbHResult result = this._icommandText.Execute(ADP.PtrZero, ref ODB.IID_IRow, dbParams, out this._recordsAffected, out executeResult);
                Bid.Trace("<oledb.ICommandText.Execute|API|OLEDB|RET> %08X{HRESULT}, RecordsAffected=%Id\n", result, this._recordsAffected);
                if (OleDbHResult.DB_E_NOTFOUND == result)
                {
                    SafeNativeMethods.Wrapper.ClearErrorInfo();
                    return 2;
                }
                if (OleDbHResult.E_NOINTERFACE != result)
                {
                    this.ExecuteCommandTextErrorHandling(result);
                    return 2;
                }
            }
            SafeNativeMethods.Wrapper.ClearErrorInfo();
            return this.ExecuteCommandTextForSingleResult(dbParams, out executeResult);
        }

        private Exception ExecuteCommandTextSpecialErrorHandling(OleDbHResult hr, Exception e)
        {
            if (((OleDbHResult.DB_E_ERRORSOCCURRED == hr) || (OleDbHResult.DB_E_BADBINDINFO == hr)) && (this._dbBindings != null))
            {
                StringBuilder builder = new StringBuilder();
                this.ParameterBindings.ParameterStatus(builder);
                e = ODB.CommandParameterStatus(builder.ToString(), e);
            }
            return e;
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return this.ExecuteReader(behavior);
        }

        public override int ExecuteNonQuery()
        {
            int num;
            IntPtr ptr;
            OleDbConnection.ExecutePermission.Demand();
            Bid.ScopeEnter(out ptr, "<oledb.OleDbCommand.ExecuteNonQuery|API> %d#\n", this.ObjectID);
            try
            {
                this._executeQuery = false;
                this.ExecuteReaderInternal(CommandBehavior.Default, "ExecuteNonQuery");
                num = ADP.IntPtrToInt32(this._recordsAffected);
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return num;
        }

        public OleDbDataReader ExecuteReader()
        {
            return this.ExecuteReader(CommandBehavior.Default);
        }

        public OleDbDataReader ExecuteReader(CommandBehavior behavior)
        {
            OleDbDataReader reader;
            IntPtr ptr;
            OleDbConnection.ExecutePermission.Demand();
            Bid.ScopeEnter(out ptr, "<oledb.OleDbCommand.ExecuteReader|API> %d#, behavior=%d{ds.CommandBehavior}\n", this.ObjectID, (int) behavior);
            try
            {
                this._executeQuery = true;
                reader = this.ExecuteReaderInternal(behavior, "ExecuteReader");
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return reader;
        }

        private OleDbDataReader ExecuteReaderInternal(CommandBehavior behavior, string method)
        {
            OleDbDataReader dataReader = null;
            OleDbException previous = null;
            int num2 = 0;
            try
            {
                object obj2;
                int num;
                this.ValidateConnectionAndTransaction(method);
                if ((CommandBehavior.SingleRow & behavior) != CommandBehavior.Default)
                {
                    behavior |= CommandBehavior.SingleResult;
                }
                switch (this.CommandType)
                {
                    case ((System.Data.CommandType) 0):
                    case System.Data.CommandType.Text:
                    case System.Data.CommandType.StoredProcedure:
                        num = this.ExecuteCommand(behavior, out obj2);
                        break;

                    case System.Data.CommandType.TableDirect:
                        num = this.ExecuteTableDirect(behavior, out obj2);
                        break;

                    default:
                        throw ADP.InvalidCommandType(this.CommandType);
                }
                if (this._executeQuery)
                {
                    try
                    {
                        dataReader = new OleDbDataReader(this._connection, this, 0, this.commandBehavior);
                        switch (num)
                        {
                            case 0:
                                dataReader.InitializeIMultipleResults(obj2);
                                dataReader.NextResult();
                                break;

                            case 1:
                                dataReader.InitializeIRowset(obj2, ChapterHandle.DB_NULL_HCHAPTER, this._recordsAffected);
                                dataReader.BuildMetaInfo();
                                dataReader.HasRowsRead();
                                break;

                            case 2:
                                dataReader.InitializeIRow(obj2, this._recordsAffected);
                                dataReader.BuildMetaInfo();
                                break;

                            case 3:
                                if (!this._isPrepared)
                                {
                                    this.PrepareCommandText(2);
                                }
                                OleDbDataReader.GenerateSchemaTable(dataReader, this._icommandText, behavior);
                                break;
                        }
                        obj2 = null;
                        this._hasDataReader = true;
                        this._connection.AddWeakReference(dataReader, 2);
                        num2 = 1;
                        return dataReader;
                    }
                    finally
                    {
                        if (1 != num2)
                        {
                            this.canceling = true;
                            if (dataReader != null)
                            {
                                dataReader.Dispose();
                                dataReader = null;
                            }
                        }
                    }
                }
                try
                {
                    if (num == 0)
                    {
                        UnsafeNativeMethods.IMultipleResults imultipleResults = (UnsafeNativeMethods.IMultipleResults) obj2;
                        previous = OleDbDataReader.NextResults(imultipleResults, this._connection, this, out this._recordsAffected);
                    }
                }
                finally
                {
                    try
                    {
                        if (obj2 != null)
                        {
                            Marshal.ReleaseComObject(obj2);
                            obj2 = null;
                        }
                        this.CloseFromDataReader(this.ParameterBindings);
                    }
                    catch (Exception exception3)
                    {
                        if (!ADP.IsCatchableExceptionType(exception3))
                        {
                            throw;
                        }
                        if (previous == null)
                        {
                            throw;
                        }
                        previous = new OleDbException(previous, exception3);
                    }
                }
            }
            finally
            {
                try
                {
                    if ((dataReader == null) && (1 != num2))
                    {
                        this.ParameterCleanup();
                    }
                }
                catch (Exception exception2)
                {
                    if (!ADP.IsCatchableExceptionType(exception2))
                    {
                        throw;
                    }
                    if (previous == null)
                    {
                        throw;
                    }
                    previous = new OleDbException(previous, exception2);
                }
                if (previous != null)
                {
                    throw previous;
                }
            }
            return dataReader;
        }

        public override object ExecuteScalar()
        {
            object obj3;
            IntPtr ptr;
            OleDbConnection.ExecutePermission.Demand();
            Bid.ScopeEnter(out ptr, "<oledb.OleDbCommand.ExecuteScalar|API> %d#\n", this.ObjectID);
            try
            {
                object obj2 = null;
                this._executeQuery = true;
                using (OleDbDataReader reader = this.ExecuteReaderInternal(CommandBehavior.Default, "ExecuteScalar"))
                {
                    if (reader.Read() && (0 < reader.FieldCount))
                    {
                        obj2 = reader.GetValue(0);
                    }
                }
                obj3 = obj2;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return obj3;
        }

        private int ExecuteTableDirect(CommandBehavior behavior, out object executeResult)
        {
            this.commandBehavior = behavior;
            executeResult = null;
            OleDbHResult result = OleDbHResult.S_OK;
            StringMemHandle handle = null;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                handle = new StringMemHandle(this.ExpandCommandText());
                handle.DangerousAddRef(ref success);
                if (success)
                {
                    tagDBID pTableID = new tagDBID {
                        uGuid = Guid.Empty,
                        eKind = 2,
                        ulPropid = handle.DangerousGetHandle()
                    };
                    using (IOpenRowsetWrapper wrapper = this._connection.IOpenRowset())
                    {
                        using (DBPropSet set = this.CommandPropertySets())
                        {
                            if (set != null)
                            {
                                Bid.Trace("<oledb.IOpenRowset.OpenRowset|API|OLEDB> %d#, IID_IRowset\n", this.ObjectID);
                                bool flag2 = false;
                                RuntimeHelpers.PrepareConstrainedRegions();
                                try
                                {
                                    set.DangerousAddRef(ref flag2);
                                    result = wrapper.Value.OpenRowset(ADP.PtrZero, pTableID, ADP.PtrZero, ref ODB.IID_IRowset, set.PropertySetCount, set.DangerousGetHandle(), out executeResult);
                                }
                                finally
                                {
                                    if (flag2)
                                    {
                                        set.DangerousRelease();
                                    }
                                }
                                Bid.Trace("<oledb.IOpenRowset.OpenRowset|API|OLEDB|RET> %08X{HRESULT}", result);
                                if (OleDbHResult.DB_E_ERRORSOCCURRED == result)
                                {
                                    Bid.Trace("<oledb.IOpenRowset.OpenRowset|API|OLEDB> %d#, IID_IRowset\n", this.ObjectID);
                                    result = wrapper.Value.OpenRowset(ADP.PtrZero, pTableID, ADP.PtrZero, ref ODB.IID_IRowset, 0, IntPtr.Zero, out executeResult);
                                    Bid.Trace("<oledb.IOpenRowset.OpenRowset|API|OLEDB|RET> %08X{HRESULT}", result);
                                }
                            }
                            else
                            {
                                Bid.Trace("<oledb.IOpenRowset.OpenRowset|API|OLEDB> %d#, IID_IRowset\n", this.ObjectID);
                                result = wrapper.Value.OpenRowset(ADP.PtrZero, pTableID, ADP.PtrZero, ref ODB.IID_IRowset, 0, IntPtr.Zero, out executeResult);
                                Bid.Trace("<oledb.IOpenRowset.OpenRowset|API|OLEDB|RET> %08X{HRESULT}", result);
                            }
                        }
                    }
                }
            }
            finally
            {
                if (success)
                {
                    handle.DangerousRelease();
                }
            }
            this.ProcessResults(result);
            this._recordsAffected = ADP.RecordsUnaffected;
            return 1;
        }

        private string ExpandCommandText()
        {
            string commandText = this.CommandText;
            if (ADP.IsEmpty(commandText))
            {
                return ADP.StrEmpty;
            }
            System.Data.CommandType commandType = this.CommandType;
            System.Data.CommandType type = commandType;
            if (type != System.Data.CommandType.Text)
            {
                if (type == System.Data.CommandType.StoredProcedure)
                {
                    return this.ExpandStoredProcedureToText(commandText);
                }
                if (type != System.Data.CommandType.TableDirect)
                {
                    throw ADP.InvalidCommandType(commandType);
                }
                return commandText;
            }
            return commandText;
        }

        private string ExpandOdbcMaximumToText(string sproctext, int parameterCount)
        {
            StringBuilder builder = new StringBuilder();
            if ((0 < parameterCount) && (ParameterDirection.ReturnValue == this.Parameters[0].Direction))
            {
                parameterCount--;
                builder.Append("{ ? = CALL ");
            }
            else
            {
                builder.Append("{ CALL ");
            }
            builder.Append(sproctext);
            switch (parameterCount)
            {
                case 0:
                    builder.Append(" }");
                    break;

                case 1:
                    builder.Append("( ? ) }");
                    break;

                default:
                    builder.Append("( ?, ?");
                    for (int i = 2; i < parameterCount; i++)
                    {
                        builder.Append(", ?");
                    }
                    builder.Append(" ) }");
                    break;
            }
            return builder.ToString();
        }

        private string ExpandOdbcMinimumToText(string sproctext, int parameterCount)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("exec ");
            builder.Append(sproctext);
            if (0 < parameterCount)
            {
                builder.Append(" ?");
                for (int i = 1; i < parameterCount; i++)
                {
                    builder.Append(", ?");
                }
            }
            return builder.ToString();
        }

        private string ExpandStoredProcedureToText(string sproctext)
        {
            int parameterCount = (this._parameters != null) ? this._parameters.Count : 0;
            if ((1 & this._connection.SqlSupport()) == 0)
            {
                return this.ExpandOdbcMinimumToText(sproctext, parameterCount);
            }
            return this.ExpandOdbcMaximumToText(sproctext, parameterCount);
        }

        internal object GetPropertyValue(Guid propertySet, int propertyID)
        {
            tagDBPROP[] gdbpropArray;
            if (this._icommandText == null)
            {
                return OleDbPropertyStatus.NotSupported;
            }
            System.Data.Common.UnsafeNativeMethods.ICommandProperties properties = this.ICommandProperties();
            using (PropertyIDSet set2 = new PropertyIDSet(propertySet, propertyID))
            {
                OleDbHResult result;
                using (DBPropSet set = new DBPropSet(properties, set2, out result))
                {
                    if (result < OleDbHResult.S_OK)
                    {
                        SafeNativeMethods.Wrapper.ClearErrorInfo();
                    }
                    gdbpropArray = set.GetPropertySet(0, out propertySet);
                }
            }
            if (gdbpropArray[0].dwStatus == OleDbPropertyStatus.Ok)
            {
                return gdbpropArray[0].vValue;
            }
            return gdbpropArray[0].dwStatus;
        }

        private bool HasParameters()
        {
            OleDbParameterCollection parameters = this._parameters;
            return ((parameters != null) && (0 < parameters.Count));
        }

        private System.Data.Common.UnsafeNativeMethods.IAccessor IAccessor()
        {
            Bid.Trace("<oledb.IUnknown.QueryInterface|API|OLEDB|command> %d#, IAccessor\n", this.ObjectID);
            return (System.Data.Common.UnsafeNativeMethods.IAccessor) this._icommandText;
        }

        private System.Data.Common.UnsafeNativeMethods.ICommandPrepare ICommandPrepare()
        {
            Bid.Trace("<oledb.IUnknown.QueryInterface|API|OLEDB|command> %d#, ICommandPrepare\n", this.ObjectID);
            return (this._icommandText as System.Data.Common.UnsafeNativeMethods.ICommandPrepare);
        }

        internal System.Data.Common.UnsafeNativeMethods.ICommandProperties ICommandProperties()
        {
            Bid.Trace("<oledb.IUnknown.QueryInterface|API|OLEDB|command> %d#, ICommandProperties\n", this.ObjectID);
            return (System.Data.Common.UnsafeNativeMethods.ICommandProperties) this._icommandText;
        }

        private System.Data.Common.UnsafeNativeMethods.ICommandWithParameters ICommandWithParameters()
        {
            Bid.Trace("<oledb.IUnknown.QueryInterface|API|OLEDB|command> %d#, ICommandWithParameters\n", this.ObjectID);
            System.Data.Common.UnsafeNativeMethods.ICommandWithParameters parameters = this._icommandText as System.Data.Common.UnsafeNativeMethods.ICommandWithParameters;
            if (parameters == null)
            {
                throw ODB.NoProviderSupportForParameters(this._connection.Provider, null);
            }
            return parameters;
        }

        private bool InitializeCommand(CommandBehavior behavior, bool throwifnotsupported)
        {
            int num = this._changeID;
            if (((CommandBehavior.KeyInfo & (this.commandBehavior ^ behavior)) != CommandBehavior.Default) || (this._lastChangeID != num))
            {
                this.CloseInternalParameters();
                this.CloseInternalCommand();
            }
            this.commandBehavior = behavior;
            num = this._changeID;
            if (!this.PropertiesOnCommand(false))
            {
                return false;
            }
            if ((this._dbBindings != null) && this._dbBindings.AreParameterBindingsInvalid(this._parameters))
            {
                this.CloseInternalParameters();
            }
            if ((this._dbBindings == null) && this.HasParameters())
            {
                this.CreateAccessor();
            }
            if (this._lastChangeID != num)
            {
                string str = this.ExpandCommandText();
                if (Bid.TraceOn)
                {
                    Bid.Trace("<oledb.ICommandText.SetCommandText|API|OLEDB> %d#, DBGUID_DEFAULT, CommandText='", this.ObjectID);
                    Bid.PutStr(str);
                    Bid.Trace("'\n");
                }
                OleDbHResult result = this._icommandText.SetCommandText(ref ODB.DBGUID_DEFAULT, str);
                Bid.Trace("<oledb.ICommandText.SetCommandText|API|OLEDB|RET> %08X{HRESULT}\n", result);
                if (result < OleDbHResult.S_OK)
                {
                    this.ProcessResults(result);
                }
            }
            this._lastChangeID = num;
            return true;
        }

        private void ParameterCleanup()
        {
            Bindings parameterBindings = this.ParameterBindings;
            if (parameterBindings != null)
            {
                parameterBindings.CleanupBindings();
            }
        }

        public override void Prepare()
        {
            IntPtr ptr;
            OleDbConnection.ExecutePermission.Demand();
            Bid.ScopeEnter(out ptr, "<oledb.OleDbCommand.Prepare|API> %d#\n", this.ObjectID);
            try
            {
                if (System.Data.CommandType.TableDirect != this.CommandType)
                {
                    this.ValidateConnectionAndTransaction("Prepare");
                    this._isPrepared = false;
                    if (System.Data.CommandType.TableDirect != this.CommandType)
                    {
                        this.InitializeCommand(CommandBehavior.Default, true);
                        this.PrepareCommandText(1);
                    }
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        private void PrepareCommandText(int expectedExecutionCount)
        {
            OleDbParameterCollection parameters = this._parameters;
            if (parameters != null)
            {
                foreach (OleDbParameter parameter in parameters)
                {
                    if (parameter.IsParameterComputed())
                    {
                        parameter.Prepare(this);
                    }
                }
            }
            System.Data.Common.UnsafeNativeMethods.ICommandPrepare prepare = this.ICommandPrepare();
            if (prepare != null)
            {
                Bid.Trace("<oledb.ICommandPrepare.Prepare|API|OLEDB> %d#, expectedExecutionCount=%d\n", this.ObjectID, expectedExecutionCount);
                OleDbHResult result = prepare.Prepare(expectedExecutionCount);
                Bid.Trace("<oledb.ICommandPrepare.Prepare|API|OLEDB|RET> %08X{HRESULT}\n", result);
                this.ProcessResults(result);
            }
            this._isPrepared = true;
        }

        private void ProcessResults(OleDbHResult hr)
        {
            Exception exception = OleDbConnection.ProcessResults(hr, this._connection, this);
            if (exception != null)
            {
                throw exception;
            }
        }

        private void ProcessResultsNoReset(OleDbHResult hr)
        {
            Exception exception = OleDbConnection.ProcessResults(hr, null, this);
            if (exception != null)
            {
                throw exception;
            }
        }

        private bool PropertiesOnCommand(bool throwNotSupported)
        {
            if (this._icommandText == null)
            {
                OleDbConnection connection = this._connection;
                if (connection == null)
                {
                    connection.CheckStateOpen("Properties");
                }
                if (!this._trackingForClose)
                {
                    this._trackingForClose = true;
                    connection.AddWeakReference(this, 1);
                }
                this._icommandText = connection.ICommandText();
                if (this._icommandText == null)
                {
                    if (throwNotSupported || this.HasParameters())
                    {
                        throw ODB.CommandTextNotSupported(connection.Provider, null);
                    }
                    return false;
                }
                using (DBPropSet set = this.CommandPropertySets())
                {
                    if (set != null)
                    {
                        System.Data.Common.UnsafeNativeMethods.ICommandProperties properties = this.ICommandProperties();
                        Bid.Trace("<oledb.ICommandProperties.SetProperties|API|OLEDB> %d#\n", this.ObjectID);
                        OleDbHResult result = properties.SetProperties(set.PropertySetCount, set);
                        Bid.Trace("<oledb.ICommandProperties.SetProperties|API|OLEDB|RET> %08X{HRESULT}\n", result);
                        if (result < OleDbHResult.S_OK)
                        {
                            SafeNativeMethods.Wrapper.ClearErrorInfo();
                        }
                    }
                }
            }
            return true;
        }

        private void PropertyChanging()
        {
            this._changeID++;
        }

        public void ResetCommandTimeout()
        {
            if (30 != this._commandTimeout)
            {
                this.PropertyChanging();
                this._commandTimeout = 30;
            }
        }

        private void ResetConnection()
        {
            OleDbConnection connection = this._connection;
            if (connection != null)
            {
                this.PropertyChanging();
                this.CloseInternal();
                if (this._trackingForClose)
                {
                    connection.RemoveWeakReference(this);
                    this._trackingForClose = false;
                }
            }
            this._connection = null;
        }

        private bool ShouldSerializeCommandTimeout()
        {
            return (30 != this._commandTimeout);
        }

        IDataReader IDbCommand.ExecuteReader()
        {
            return this.ExecuteReader(CommandBehavior.Default);
        }

        IDataReader IDbCommand.ExecuteReader(CommandBehavior behavior)
        {
            return this.ExecuteReader(behavior);
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        internal Bindings TakeBindingOwnerShip()
        {
            Bindings bindings = this._dbBindings;
            this._dbBindings = null;
            return bindings;
        }

        private void ValidateConnection(string method)
        {
            if (this._connection == null)
            {
                throw ADP.ConnectionRequired(method);
            }
            this._connection.CheckStateOpen(method);
            if (this._hasDataReader)
            {
                if (this._connection.HasLiveReader(this))
                {
                    throw ADP.OpenReaderExists();
                }
                this._hasDataReader = false;
            }
        }

        private void ValidateConnectionAndTransaction(string method)
        {
            this.ValidateConnection(method);
            this._transaction = this._connection.ValidateTransaction(this.Transaction, method);
            this.canceling = false;
        }

        [DefaultValue(""), ResDescription("DbCommand_CommandText"), ResCategory("DataCategory_Data"), RefreshProperties(RefreshProperties.All), Editor("Microsoft.VSDesigner.Data.ADO.Design.OleDbCommandTextEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
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
                    Bid.Trace("<oledb.OleDbCommand.set_CommandText|API> %d#, '", this.ObjectID);
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

        [ResCategory("DataCategory_Data"), ResDescription("DbCommand_CommandTimeout")]
        public override int CommandTimeout
        {
            get
            {
                return this._commandTimeout;
            }
            set
            {
                Bid.Trace("<oledb.OleDbCommand.set_CommandTimeout|API> %d#, %d\n", this.ObjectID, value);
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

        [DefaultValue(1), RefreshProperties(RefreshProperties.All), ResCategory("DataCategory_Data"), ResDescription("DbCommand_CommandType")]
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
                if (((type != System.Data.CommandType.Text) && (type != System.Data.CommandType.StoredProcedure)) && (type != System.Data.CommandType.TableDirect))
                {
                    throw ADP.InvalidCommandType(value);
                }
                this.PropertyChanging();
                this._commandType = value;
            }
        }

        [Editor("Microsoft.VSDesigner.Data.Design.DbConnectionEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultValue((string) null), ResCategory("DataCategory_Data"), ResDescription("DbCommand_Connection")]
        public OleDbConnection Connection
        {
            get
            {
                return this._connection;
            }
            set
            {
                OleDbConnection connection = this._connection;
                if (value != connection)
                {
                    this.PropertyChanging();
                    this.ResetConnection();
                    this._connection = value;
                    Bid.Trace("<oledb.OleDbCommand.set_Connection|API> %d#\n", this.ObjectID);
                    if (value != null)
                    {
                        this._transaction = OleDbTransaction.TransactionUpdate(this._transaction);
                    }
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
                this.Connection = (OleDbConnection) value;
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
                this.Transaction = (OleDbTransaction) value;
            }
        }

        [DesignOnly(true), DefaultValue(true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        private Bindings ParameterBindings
        {
            get
            {
                return this._dbBindings;
            }
            set
            {
                Bindings bindings = this._dbBindings;
                this._dbBindings = value;
                if ((bindings != null) && (value != bindings))
                {
                    bindings.Dispose();
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), ResDescription("DbCommand_Parameters"), ResCategory("DataCategory_Data")]
        public OleDbParameterCollection Parameters
        {
            get
            {
                OleDbParameterCollection parameters = this._parameters;
                if (parameters == null)
                {
                    parameters = new OleDbParameterCollection();
                    this._parameters = parameters;
                }
                return parameters;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), ResDescription("DbCommand_Transaction")]
        public OleDbTransaction Transaction
        {
            get
            {
                OleDbTransaction parent = this._transaction;
                while ((parent != null) && (parent.Connection == null))
                {
                    parent = parent.Parent;
                    this._transaction = parent;
                }
                return parent;
            }
            set
            {
                this._transaction = value;
                Bid.Trace("<oledb.OleDbCommand.set_Transaction|API> %d#\n", this.ObjectID);
            }
        }

        [ResCategory("DataCategory_Update"), ResDescription("DbCommand_UpdatedRowSource"), DefaultValue(3)]
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

