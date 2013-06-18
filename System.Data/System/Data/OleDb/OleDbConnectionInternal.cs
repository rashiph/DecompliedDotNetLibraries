namespace System.Data.OleDb
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;
    using System.Transactions;

    internal sealed class OleDbConnectionInternal : DbConnectionInternal, IDisposable
    {
        private readonly DataSourceWrapper _datasrcwrp;
        private readonly SessionWrapper _sessionwrp;
        private bool _unEnlistDuringDeactivate;
        internal readonly OleDbConnectionString ConnectionString;
        private static object dataInitializeLock = new object();
        private static volatile OleDbServicesWrapper idataInitialize;
        private WeakReference weakTransaction;

        internal OleDbConnectionInternal(OleDbConnectionString constr, OleDbConnection connection)
        {
            this.ConnectionString = constr;
            if (constr.PossiblePrompt && !Environment.UserInteractive)
            {
                throw ODB.PossiblePromptNotUserInteractive();
            }
            try
            {
                OleDbServicesWrapper objectPool = GetObjectPool();
                this._datasrcwrp = new DataSourceWrapper();
                objectPool.GetDataSource(constr, ref this._datasrcwrp);
                if (connection != null)
                {
                    this._sessionwrp = new SessionWrapper();
                    OleDbHResult hresult = this._datasrcwrp.InitializeAndCreateSession(constr, ref this._sessionwrp);
                    if ((OleDbHResult.S_OK > hresult) || this._sessionwrp.IsInvalid)
                    {
                        throw OleDbConnection.ProcessResults(hresult, null, null);
                    }
                    OleDbConnection.ProcessResults(hresult, connection, connection);
                }
            }
            catch
            {
                if (this._sessionwrp != null)
                {
                    this._sessionwrp.Dispose();
                    this._sessionwrp = null;
                }
                if (this._datasrcwrp != null)
                {
                    this._datasrcwrp.Dispose();
                    this._datasrcwrp = null;
                }
                throw;
            }
        }

        protected override void Activate(Transaction transaction)
        {
            throw ADP.NotSupported();
        }

        internal bool AddInfoKeywordsToTable(DataTable table, DataColumn keyword)
        {
            using (IDBInfoWrapper wrapper = this.IDBInfo())
            {
                string str;
                System.Data.Common.UnsafeNativeMethods.IDBInfo info = wrapper.Value;
                if (info == null)
                {
                    return false;
                }
                Bid.Trace("<oledb.IDBInfo.GetKeywords|API|OLEDB> %d#\n", base.ObjectID);
                OleDbHResult keywords = info.GetKeywords(out str);
                Bid.Trace("<oledb.IDBInfo.GetKeywords|API|OLEDB|RET> %08X{HRESULT}\n", keywords);
                if (keywords < OleDbHResult.S_OK)
                {
                    this.ProcessResults(keywords);
                }
                if (str != null)
                {
                    string[] strArray = str.Split(new char[] { ',' });
                    for (int i = 0; i < strArray.Length; i++)
                    {
                        DataRow row = table.NewRow();
                        row[keyword] = strArray[i];
                        table.Rows.Add(row);
                        row.AcceptChanges();
                    }
                }
                return true;
            }
        }

        public override DbTransaction BeginTransaction(System.Data.IsolationLevel isolationLevel)
        {
            OleDbTransaction transaction;
            OleDbConnection.ExecutePermission.Demand();
            OleDbConnection connection = this.Connection;
            if (this.LocalTransaction != null)
            {
                throw ADP.ParallelTransactionsNotSupported(connection);
            }
            object o = null;
            try
            {
                transaction = new OleDbTransaction(connection, null, isolationLevel);
                Bid.Trace("<oledb.IUnknown.QueryInterface|API|OLEDB|session> %d#, ITransactionLocal\n", base.ObjectID);
                o = this._sessionwrp.ComWrapper();
                System.Data.Common.UnsafeNativeMethods.ITransactionLocal local = o as System.Data.Common.UnsafeNativeMethods.ITransactionLocal;
                if (local == null)
                {
                    throw ODB.TransactionsNotSupported(this.Provider, null);
                }
                transaction.BeginInternal(local);
            }
            finally
            {
                if (o != null)
                {
                    Marshal.ReleaseComObject(o);
                }
            }
            this.LocalTransaction = transaction;
            return transaction;
        }

        internal DataTable BuildInfoKeywords()
        {
            DataTable table = new DataTable("DbInfoKeywords") {
                Locale = CultureInfo.InvariantCulture
            };
            DataColumn column = new DataColumn("Keyword", typeof(string));
            table.Columns.Add(column);
            if (!this.AddInfoKeywordsToTable(table, column))
            {
                table = null;
            }
            return table;
        }

        internal DataTable BuildInfoLiterals()
        {
            using (IDBInfoWrapper wrapper = this.IDBInfo())
            {
                OleDbHResult result;
                System.Data.Common.UnsafeNativeMethods.IDBInfo dbInfo = wrapper.Value;
                if (dbInfo == null)
                {
                    return null;
                }
                DataTable table = new DataTable("DbInfoLiterals") {
                    Locale = CultureInfo.InvariantCulture
                };
                DataColumn column6 = new DataColumn("LiteralName", typeof(string));
                DataColumn column5 = new DataColumn("LiteralValue", typeof(string));
                DataColumn column4 = new DataColumn("InvalidChars", typeof(string));
                DataColumn column3 = new DataColumn("InvalidStartingChars", typeof(string));
                DataColumn column2 = new DataColumn("Literal", typeof(int));
                DataColumn column = new DataColumn("Maxlen", typeof(int));
                table.Columns.Add(column6);
                table.Columns.Add(column5);
                table.Columns.Add(column4);
                table.Columns.Add(column3);
                table.Columns.Add(column2);
                table.Columns.Add(column);
                int literalCount = 0;
                IntPtr ptrZero = ADP.PtrZero;
                using (new DualCoTaskMem(dbInfo, null, out literalCount, out ptrZero, out result))
                {
                    if (OleDbHResult.DB_E_ERRORSOCCURRED != result)
                    {
                        long num2 = ptrZero.ToInt64();
                        tagDBLITERALINFO structure = new tagDBLITERALINFO();
                        int num = 0;
                        while (num < literalCount)
                        {
                            Marshal.PtrToStructure((IntPtr) num2, structure);
                            DataRow row = table.NewRow();
                            row[column6] = ((OleDbLiteral) structure.it).ToString();
                            row[column5] = structure.pwszLiteralValue;
                            row[column4] = structure.pwszInvalidChars;
                            row[column3] = structure.pwszInvalidStartingChars;
                            row[column2] = structure.it;
                            row[column] = structure.cchMaxLen;
                            table.Rows.Add(row);
                            row.AcceptChanges();
                            num++;
                            num2 += ODB.SizeOf_tagDBLITERALINFO;
                        }
                        if (result < OleDbHResult.S_OK)
                        {
                            this.ProcessResults(result);
                        }
                    }
                    else
                    {
                        SafeNativeMethods.Wrapper.ClearErrorInfo();
                    }
                }
                return table;
            }
        }

        internal DataTable BuildSchemaGuids()
        {
            DataTable table = new DataTable("SchemaGuids") {
                Locale = CultureInfo.InvariantCulture
            };
            DataColumn column2 = new DataColumn("Schema", typeof(Guid));
            DataColumn column = new DataColumn("RestrictionSupport", typeof(int));
            table.Columns.Add(column2);
            table.Columns.Add(column);
            SchemaSupport[] schemaRowsetInformation = this.GetSchemaRowsetInformation();
            if (schemaRowsetInformation != null)
            {
                object[] values = new object[2];
                table.BeginLoadData();
                for (int i = 0; i < schemaRowsetInformation.Length; i++)
                {
                    values[0] = schemaRowsetInformation[i]._schemaRowset;
                    values[1] = schemaRowsetInformation[i]._restrictions;
                    table.LoadDataRow(values, LoadOption.OverwriteChanges);
                }
                table.EndLoadData();
            }
            return table;
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
        private static object CreateInstanceDataLinks()
        {
            return Activator.CreateInstance(Type.GetTypeFromCLSID(ODB.CLSID_DataLinks, true), BindingFlags.Public | BindingFlags.Instance, null, null, CultureInfo.InvariantCulture, null);
        }

        protected override DbReferenceCollection CreateReferenceCollection()
        {
            return new OleDbReferenceCollection();
        }

        protected override void Deactivate()
        {
            base.NotifyWeakReference(0);
            if (this._unEnlistDuringDeactivate)
            {
                this.EnlistTransactionInternal(null);
            }
            OleDbTransaction localTransaction = this.LocalTransaction;
            if (localTransaction != null)
            {
                this.LocalTransaction = null;
                localTransaction.Dispose();
            }
        }

        public override void Dispose()
        {
            if (this._sessionwrp != null)
            {
                this._sessionwrp.Dispose();
            }
            if (this._datasrcwrp != null)
            {
                this._datasrcwrp.Dispose();
            }
            base.Dispose();
        }

        public override void EnlistTransaction(Transaction transaction)
        {
            OleDbConnection connection = this.Connection;
            if (this.LocalTransaction != null)
            {
                throw ADP.LocalTransactionPresent();
            }
            this.EnlistTransactionInternal(transaction);
        }

        internal void EnlistTransactionInternal(Transaction transaction)
        {
            IntPtr ptr;
            IDtcTransaction oletxTransaction = ADP.GetOletxTransaction(transaction);
            Bid.ScopeEnter(out ptr, "<oledb.ITransactionJoin.JoinTransaction|API|OLEDB> %d#\n", base.ObjectID);
            try
            {
                using (ITransactionJoinWrapper wrapper = this.ITransactionJoin())
                {
                    if (wrapper.Value == null)
                    {
                        throw ODB.TransactionsNotSupported(this.Provider, null);
                    }
                    wrapper.Value.JoinTransaction(oletxTransaction, -1, 0, IntPtr.Zero);
                    this._unEnlistDuringDeactivate = null != transaction;
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            base.EnlistedTransaction = transaction;
        }

        internal object GetDataSourcePropertyValue(Guid propertySet, int propertyID)
        {
            tagDBPROP[] gdbpropArray;
            using (IDBPropertiesWrapper wrapper = this.IDBProperties())
            {
                using (PropertyIDSet set2 = new PropertyIDSet(propertySet, propertyID))
                {
                    OleDbHResult result;
                    using (DBPropSet set = new DBPropSet(wrapper.Value, set2, out result))
                    {
                        if (result < OleDbHResult.S_OK)
                        {
                            SafeNativeMethods.Wrapper.ClearErrorInfo();
                        }
                        gdbpropArray = set.GetPropertySet(0, out propertySet);
                    }
                }
            }
            if (gdbpropArray[0].dwStatus == OleDbPropertyStatus.Ok)
            {
                return gdbpropArray[0].vValue;
            }
            return gdbpropArray[0].dwStatus;
        }

        internal object GetDataSourceValue(Guid propertySet, int propertyID)
        {
            object dataSourcePropertyValue = this.GetDataSourcePropertyValue(propertySet, propertyID);
            if (!(dataSourcePropertyValue is OleDbPropertyStatus) && !Convert.IsDBNull(dataSourcePropertyValue))
            {
                return dataSourcePropertyValue;
            }
            return null;
        }

        internal string GetLiteralInfo(int literal)
        {
            using (IDBInfoWrapper wrapper = this.IDBInfo())
            {
                OleDbHResult result;
                System.Data.Common.UnsafeNativeMethods.IDBInfo dbInfo = wrapper.Value;
                if (dbInfo == null)
                {
                    return null;
                }
                string str = null;
                IntPtr ptrZero = ADP.PtrZero;
                int literalCount = 0;
                using (new DualCoTaskMem(dbInfo, new int[] { literal }, out literalCount, out ptrZero, out result))
                {
                    if (OleDbHResult.DB_E_ERRORSOCCURRED != result)
                    {
                        if ((1 == literalCount) && (Marshal.ReadInt32(ptrZero, ODB.OffsetOf_tagDBLITERALINFO_it) == literal))
                        {
                            str = Marshal.PtrToStringUni(Marshal.ReadIntPtr(ptrZero, 0));
                        }
                        if (result < OleDbHResult.S_OK)
                        {
                            this.ProcessResults(result);
                        }
                    }
                    else
                    {
                        SafeNativeMethods.Wrapper.ClearErrorInfo();
                    }
                }
                return str;
            }
        }

        private static OleDbServicesWrapper GetObjectPool()
        {
            OleDbServicesWrapper idataInitialize = OleDbConnectionInternal.idataInitialize;
            if (idataInitialize == null)
            {
                lock (dataInitializeLock)
                {
                    object obj2;
                    idataInitialize = OleDbConnectionInternal.idataInitialize;
                    if (idataInitialize != null)
                    {
                        return idataInitialize;
                    }
                    VersionCheck();
                    try
                    {
                        obj2 = CreateInstanceDataLinks();
                    }
                    catch (Exception exception)
                    {
                        if (!ADP.IsCatchableExceptionType(exception))
                        {
                            throw;
                        }
                        throw ODB.MDACNotAvailable(exception);
                    }
                    if (obj2 == null)
                    {
                        throw ODB.MDACNotAvailable(null);
                    }
                    idataInitialize = new OleDbServicesWrapper(obj2);
                    OleDbConnectionInternal.idataInitialize = idataInitialize;
                }
            }
            return idataInitialize;
        }

        internal Dictionary<string, OleDbPropertyInfo> GetPropertyInfo(Guid[] propertySets)
        {
            bool hasSession = this.HasSession;
            if (propertySets == null)
            {
                propertySets = new Guid[0];
            }
            using (PropertyIDSet set2 = new PropertyIDSet(propertySets))
            {
                using (IDBPropertiesWrapper wrapper = this.IDBProperties())
                {
                    using (PropertyInfoSet set = new PropertyInfoSet(wrapper.Value, set2))
                    {
                        return set.GetValues();
                    }
                }
            }
        }

        internal DataTable GetSchemaRowset(Guid schema, object[] restrictions)
        {
            DataTable table2;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<oledb.OleDbConnectionInternal.GetSchemaRowset|INFO> %d#, schema=%ls, restrictions\n", base.ObjectID, schema);
            try
            {
                if (restrictions == null)
                {
                    restrictions = new object[0];
                }
                DataTable table = null;
                using (IDBSchemaRowsetWrapper wrapper = this.IDBSchemaRowset())
                {
                    System.Data.Common.UnsafeNativeMethods.IDBSchemaRowset rowset2 = wrapper.Value;
                    if (rowset2 == null)
                    {
                        throw ODB.SchemaRowsetsNotSupported(this.Provider);
                    }
                    System.Data.Common.UnsafeNativeMethods.IRowset ppRowset = null;
                    Bid.Trace("<oledb.IDBSchemaRowset.GetRowset|API|OLEDB> %d#\n", base.ObjectID);
                    OleDbHResult result = rowset2.GetRowset(ADP.PtrZero, ref schema, restrictions.Length, restrictions, ref ODB.IID_IRowset, 0, ADP.PtrZero, out ppRowset);
                    Bid.Trace("<oledb.IDBSchemaRowset.GetRowset|API|OLEDB|RET> %08X{HRESULT}\n", result);
                    if (result < OleDbHResult.S_OK)
                    {
                        this.ProcessResults(result);
                    }
                    if (ppRowset != null)
                    {
                        using (OleDbDataReader reader = new OleDbDataReader(this.Connection, null, 0, CommandBehavior.Default))
                        {
                            reader.InitializeIRowset(ppRowset, ChapterHandle.DB_NULL_HCHAPTER, IntPtr.Zero);
                            reader.BuildMetaInfo();
                            reader.HasRowsRead();
                            table = new DataTable {
                                Locale = CultureInfo.InvariantCulture,
                                TableName = OleDbSchemaGuid.GetTextFromValue(schema)
                            };
                            OleDbDataAdapter.FillDataTable(reader, new DataTable[] { table });
                        }
                    }
                    return table;
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return table2;
        }

        internal SchemaSupport[] GetSchemaRowsetInformation()
        {
            OleDbConnectionString connectionString = this.ConnectionString;
            SchemaSupport[] schemaSupport = connectionString.SchemaSupport;
            if (schemaSupport != null)
            {
                return schemaSupport;
            }
            using (IDBSchemaRowsetWrapper wrapper = this.IDBSchemaRowset())
            {
                OleDbHResult result;
                System.Data.Common.UnsafeNativeMethods.IDBSchemaRowset dbSchemaRowset = wrapper.Value;
                if (dbSchemaRowset == null)
                {
                    return null;
                }
                int schemaCount = 0;
                IntPtr ptrZero = ADP.PtrZero;
                IntPtr schemaRestrictions = ADP.PtrZero;
                using (new DualCoTaskMem(dbSchemaRowset, out schemaCount, out ptrZero, out schemaRestrictions, out result))
                {
                    dbSchemaRowset = null;
                    if (result < OleDbHResult.S_OK)
                    {
                        this.ProcessResults(result);
                    }
                    schemaSupport = new SchemaSupport[schemaCount];
                    if (ADP.PtrZero != ptrZero)
                    {
                        int index = 0;
                        for (int i = 0; index < schemaSupport.Length; i += ODB.SizeOf_Guid)
                        {
                            IntPtr ptr = ADP.IntPtrOffset(ptrZero, index * ODB.SizeOf_Guid);
                            schemaSupport[index]._schemaRowset = (Guid) Marshal.PtrToStructure(ptr, typeof(Guid));
                            index++;
                        }
                    }
                    if (ADP.PtrZero != schemaRestrictions)
                    {
                        for (int j = 0; j < schemaSupport.Length; j++)
                        {
                            schemaSupport[j]._restrictions = Marshal.ReadInt32(schemaRestrictions, j * 4);
                        }
                    }
                }
                connectionString.SchemaSupport = schemaSupport;
                return schemaSupport;
            }
        }

        internal bool HasLiveReader(OleDbCommand cmd)
        {
            DbReferenceCollection referenceCollection = base.ReferenceCollection;
            if (referenceCollection != null)
            {
                foreach (object obj2 in referenceCollection.Filter(2))
                {
                    OleDbDataReader reader = (OleDbDataReader) obj2;
                    if ((reader != null) && (cmd == reader.Command))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal System.Data.Common.UnsafeNativeMethods.ICommandText ICommandText()
        {
            object icommandText = null;
            OleDbHResult hr = this._sessionwrp.CreateCommand(ref icommandText);
            if (hr < OleDbHResult.S_OK)
            {
                if (OleDbHResult.E_NOINTERFACE != hr)
                {
                    this.ProcessResults(hr);
                }
                else
                {
                    SafeNativeMethods.Wrapper.ClearErrorInfo();
                }
            }
            return (System.Data.Common.UnsafeNativeMethods.ICommandText) icommandText;
        }

        private IDBInfoWrapper IDBInfo()
        {
            return this._datasrcwrp.IDBInfo(this);
        }

        internal IDBPropertiesWrapper IDBProperties()
        {
            return this._datasrcwrp.IDBProperties(this);
        }

        internal IDBSchemaRowsetWrapper IDBSchemaRowset()
        {
            return this._sessionwrp.IDBSchemaRowset(this);
        }

        internal IOpenRowsetWrapper IOpenRowset()
        {
            return this._sessionwrp.IOpenRowset(this);
        }

        internal ITransactionJoinWrapper ITransactionJoin()
        {
            return this._sessionwrp.ITransactionJoin(this);
        }

        private void ProcessResults(OleDbHResult hr)
        {
            OleDbConnection connection = this.Connection;
            Exception exception = OleDbConnection.ProcessResults(hr, connection, connection);
            if (exception != null)
            {
                throw exception;
            }
        }

        public static void ReleaseObjectPool()
        {
            idataInitialize = null;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void SetMTAApartmentState()
        {
            Thread.CurrentThread.SetApartmentState(ApartmentState.MTA);
        }

        internal bool SupportSchemaRowset(Guid schema)
        {
            SchemaSupport[] schemaRowsetInformation = this.GetSchemaRowsetInformation();
            if (schemaRowsetInformation != null)
            {
                for (int i = 0; i < schemaRowsetInformation.Length; i++)
                {
                    if (schema == schemaRowsetInformation[i]._schemaRowset)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal OleDbTransaction ValidateTransaction(OleDbTransaction transaction, string method)
        {
            if (this.weakTransaction != null)
            {
                OleDbTransaction target = (OleDbTransaction) this.weakTransaction.Target;
                if ((target != null) && this.weakTransaction.IsAlive)
                {
                    target = OleDbTransaction.TransactionUpdate(target);
                }
                if (target != null)
                {
                    if (transaction == null)
                    {
                        throw ADP.TransactionRequired(method);
                    }
                    OleDbTransaction transaction3 = OleDbTransaction.TransactionLast(target);
                    if (transaction3 == transaction)
                    {
                        return transaction;
                    }
                    if (transaction3.Connection != transaction.Connection)
                    {
                        throw ADP.TransactionConnectionMismatch();
                    }
                    throw ADP.TransactionCompleted();
                }
                this.weakTransaction = null;
            }
            else if ((transaction != null) && (transaction.Connection != null))
            {
                throw ADP.TransactionConnectionMismatch();
            }
            return null;
        }

        private static void VersionCheck()
        {
            if (ApartmentState.Unknown == Thread.CurrentThread.GetApartmentState())
            {
                SetMTAApartmentState();
            }
            ADP.CheckVersionMDAC(false);
        }

        internal OleDbConnection Connection
        {
            get
            {
                return (OleDbConnection) base.Owner;
            }
        }

        internal bool HasSession
        {
            get
            {
                return (null != this._sessionwrp);
            }
        }

        internal OleDbTransaction LocalTransaction
        {
            get
            {
                OleDbTransaction target = null;
                if (this.weakTransaction != null)
                {
                    target = (OleDbTransaction) this.weakTransaction.Target;
                }
                return target;
            }
            set
            {
                this.weakTransaction = null;
                if (value != null)
                {
                    this.weakTransaction = new WeakReference(value);
                }
            }
        }

        private string Provider
        {
            get
            {
                return this.ConnectionString.Provider;
            }
        }

        public override string ServerVersion
        {
            get
            {
                return Convert.ToString(this.GetDataSourceValue(OleDbPropertySetGuid.DataSourceInfo, 0x29), CultureInfo.InvariantCulture);
            }
        }
    }
}

