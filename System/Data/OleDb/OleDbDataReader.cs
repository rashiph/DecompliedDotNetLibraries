namespace System.Data.OleDb
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public sealed class OleDbDataReader : DbDataReader
    {
        private Bindings[] _bindings;
        private ChapterHandle _chapterHandle = ChapterHandle.DB_NULL_HCHAPTER;
        private OleDbCommand _command;
        private CommandBehavior _commandBehavior;
        private OleDbConnection _connection;
        private int _currentRow;
        private DataTable _dbSchemaTable;
        private int _depth;
        private FieldNameLookup _fieldNameLookup;
        private bool _hasRows;
        private bool _hasRowsReadCheck;
        private UnsafeNativeMethods.IMultipleResults _imultipleResults;
        private System.Data.Common.UnsafeNativeMethods.IRow _irow;
        private System.Data.Common.UnsafeNativeMethods.IRowset _irowset;
        private bool _isClosed;
        private bool _isRead;
        private System.Data.OleDb.MetaData[] _metadata;
        private int _nextAccessorForRetrieval;
        private int _nextValueForRetrieval;
        private static int _objectTypeCount;
        private Bindings _parameterBindings;
        private IntPtr _recordsAffected = ADP.RecordsUnaffected;
        private IntPtr _rowFetchedCount;
        private IntPtr _rowHandleFetchCount;
        private RowHandleBuffer _rowHandleNativeBuffer;
        private bool _sequentialAccess;
        private long _sequentialBytesRead;
        private int _sequentialOrdinal;
        private bool _singleRow;
        private bool _useIColumnsRowset;
        private int _visibleFieldCount;
        internal readonly int ObjectID = Interlocked.Increment(ref _objectTypeCount);

        internal OleDbDataReader(OleDbConnection connection, OleDbCommand command, int depth, CommandBehavior commandBehavior)
        {
            this._connection = connection;
            this._command = command;
            this._commandBehavior = commandBehavior;
            if ((command != null) && (this._depth == 0))
            {
                this._parameterBindings = command.TakeBindingOwnerShip();
            }
            this._depth = depth;
        }

        private static IntPtr AddRecordsAffected(IntPtr recordsAffected, IntPtr affected)
        {
            if (0 > ((int) affected))
            {
                return recordsAffected;
            }
            if (0 <= ((int) recordsAffected))
            {
                return (IntPtr) (((int) recordsAffected) + ((int) affected));
            }
            return affected;
        }

        private static bool AllowDBNull(int flags)
        {
            return (0 != (0x20 & flags));
        }

        private static bool AllowDBNullMaybeNull(int flags)
        {
            return (0 != (0x60 & flags));
        }

        private void AppendSchemaInfo()
        {
            if (this._metadata.Length > 0)
            {
                int num4 = 0;
                for (int i = 0; i < this._metadata.Length; i++)
                {
                    if (this._metadata[i].isKeyColumn && !this._metadata[i].isHidden)
                    {
                        num4++;
                    }
                }
                if (num4 == 0)
                {
                    string strA = null;
                    string str2 = null;
                    string baseTableName = null;
                    for (int j = 0; j < this._metadata.Length; j++)
                    {
                        System.Data.OleDb.MetaData data = this._metadata[j];
                        if ((data.baseTableName != null) && (0 < data.baseTableName.Length))
                        {
                            string strB = (data.baseCatalogName != null) ? data.baseCatalogName : "";
                            string str8 = (data.baseSchemaName != null) ? data.baseSchemaName : "";
                            if (baseTableName == null)
                            {
                                strA = str8;
                                str2 = strB;
                                baseTableName = data.baseTableName;
                            }
                            else if (((ADP.SrcCompare(baseTableName, data.baseTableName) != 0) || (ADP.SrcCompare(str2, strB) != 0)) || (ADP.SrcCompare(strA, str8) != 0))
                            {
                                baseTableName = null;
                                break;
                            }
                        }
                    }
                    if (baseTableName != null)
                    {
                        str2 = ADP.IsEmpty(str2) ? null : str2;
                        strA = ADP.IsEmpty(strA) ? null : strA;
                        if ((this._connection != null) && (4 == this._connection.QuotedIdentifierCase()))
                        {
                            string quoteSuffix = null;
                            string quotePrefix = null;
                            this._connection.GetLiteralQuotes("GetSchemaTable", out quotePrefix, out quoteSuffix);
                            if (quotePrefix == null)
                            {
                                quotePrefix = "";
                            }
                            if (quoteSuffix == null)
                            {
                                quoteSuffix = "";
                            }
                            baseTableName = quotePrefix + baseTableName + quoteSuffix;
                        }
                        Hashtable baseColumnNames = new Hashtable(this._metadata.Length * 2);
                        for (int k = this._metadata.Length - 1; 0 <= k; k--)
                        {
                            string baseColumnName = this._metadata[k].baseColumnName;
                            if (!ADP.IsEmpty(baseColumnName))
                            {
                                baseColumnNames[baseColumnName] = k;
                            }
                        }
                        for (int m = 0; m < this._metadata.Length; m++)
                        {
                            string str = this._metadata[m].baseColumnName;
                            if (!ADP.IsEmpty(str))
                            {
                                str = str.ToLower(CultureInfo.InvariantCulture);
                                if (!baseColumnNames.Contains(str))
                                {
                                    baseColumnNames[str] = m;
                                }
                            }
                        }
                        if (this._connection.SupportSchemaRowset(OleDbSchemaGuid.Primary_Keys))
                        {
                            object[] restrictions = new object[] { str2, strA, baseTableName };
                            num4 = this.AppendSchemaPrimaryKey(baseColumnNames, restrictions);
                        }
                        if ((num4 == 0) && this._connection.SupportSchemaRowset(OleDbSchemaGuid.Indexes))
                        {
                            object[] objArray = new object[5];
                            objArray[0] = str2;
                            objArray[1] = strA;
                            objArray[4] = baseTableName;
                            object[] objArray3 = objArray;
                            this.AppendSchemaUniqueIndexAsKey(baseColumnNames, objArray3);
                        }
                    }
                }
            }
        }

        private int AppendSchemaPrimaryKey(Hashtable baseColumnNames, object[] restrictions)
        {
            int num2 = 0;
            bool flag = false;
            DataTable schemaRowset = null;
            try
            {
                schemaRowset = this._connection.GetSchemaRowset(OleDbSchemaGuid.Primary_Keys, restrictions);
            }
            catch (Exception exception)
            {
                if (!ADP.IsCatchableExceptionType(exception))
                {
                    throw;
                }
                ADP.TraceExceptionWithoutRethrow(exception);
            }
            if (schemaRowset != null)
            {
                DataColumnCollection columns = schemaRowset.Columns;
                int index = columns.IndexOf("COLUMN_NAME");
                if (-1 != index)
                {
                    DataColumn column = columns[index];
                    foreach (DataRow row in schemaRowset.Rows)
                    {
                        string name = (string) row[column, DataRowVersion.Default];
                        int num3 = this.IndexOf(baseColumnNames, name);
                        if (0 <= num3)
                        {
                            System.Data.OleDb.MetaData data = this._metadata[num3];
                            data.isKeyColumn = true;
                            data.flags &= -33;
                            num2++;
                        }
                        else
                        {
                            flag = true;
                            break;
                        }
                    }
                }
            }
            if (!flag)
            {
                return num2;
            }
            for (int i = 0; i < this._metadata.Length; i++)
            {
                this._metadata[i].isKeyColumn = false;
            }
            return -1;
        }

        private void AppendSchemaUniqueIndexAsKey(Hashtable baseColumnNames, object[] restrictions)
        {
            bool flag3 = false;
            DataTable schemaRowset = null;
            try
            {
                schemaRowset = this._connection.GetSchemaRowset(OleDbSchemaGuid.Indexes, restrictions);
            }
            catch (Exception exception)
            {
                if (!ADP.IsCatchableExceptionType(exception))
                {
                    throw;
                }
                ADP.TraceExceptionWithoutRethrow(exception);
            }
            if (schemaRowset != null)
            {
                DataColumnCollection columns = schemaRowset.Columns;
                int index = columns.IndexOf("INDEX_NAME");
                int num7 = columns.IndexOf("PRIMARY_KEY");
                int num6 = columns.IndexOf("UNIQUE");
                int num5 = columns.IndexOf("COLUMN_NAME");
                int num4 = columns.IndexOf("NULLS");
                if (((-1 != index) && (-1 != num7)) && ((-1 != num6) && (-1 != num5)))
                {
                    DataColumn column4 = columns[index];
                    DataColumn column3 = columns[num7];
                    DataColumn column2 = columns[num6];
                    DataColumn column5 = columns[num5];
                    DataColumn column = (-1 != num4) ? columns[num4] : null;
                    bool[] flagArray2 = new bool[this._metadata.Length];
                    bool[] flagArray = new bool[this._metadata.Length];
                    string str = null;
                    foreach (DataRow row in schemaRowset.Rows)
                    {
                        bool flag = !row.IsNull(column3, DataRowVersion.Default) && ((bool) row[column3, DataRowVersion.Default]);
                        bool flag2 = !row.IsNull(column2, DataRowVersion.Default) && ((bool) row[column2, DataRowVersion.Default]);
                        if ((column != null) && !row.IsNull(column, DataRowVersion.Default))
                        {
                            Convert.ToInt32(row[column, DataRowVersion.Default], CultureInfo.InvariantCulture);
                        }
                        if (flag || flag2)
                        {
                            string name = (string) row[column5, DataRowVersion.Default];
                            int num3 = this.IndexOf(baseColumnNames, name);
                            if (0 <= num3)
                            {
                                if (flag)
                                {
                                    flagArray2[num3] = true;
                                }
                                if (flag2 && (flagArray != null))
                                {
                                    flagArray[num3] = true;
                                    string str2 = (string) row[column4, DataRowVersion.Default];
                                    if (str == null)
                                    {
                                        str = str2;
                                    }
                                    else if (str2 != str)
                                    {
                                        flagArray = null;
                                    }
                                }
                            }
                            else
                            {
                                if (flag)
                                {
                                    flag3 = true;
                                    break;
                                }
                                if (str != null)
                                {
                                    string str3 = (string) row[column4, DataRowVersion.Default];
                                    if (str3 != str)
                                    {
                                        flagArray = null;
                                    }
                                }
                            }
                        }
                    }
                    if (flag3)
                    {
                        for (int i = 0; i < this._metadata.Length; i++)
                        {
                            this._metadata[i].isKeyColumn = false;
                        }
                    }
                    else if (flagArray != null)
                    {
                        for (int j = 0; j < this._metadata.Length; j++)
                        {
                            this._metadata[j].isKeyColumn = flagArray[j];
                        }
                    }
                }
            }
        }

        internal void BuildMetaInfo()
        {
            if (this._irowset != null)
            {
                if (this._useIColumnsRowset)
                {
                    this.BuildSchemaTableRowset(this._irowset);
                }
                else
                {
                    this.BuildSchemaTableInfo(this._irowset, false, false);
                }
                if ((this._metadata != null) && (0 < this._metadata.Length))
                {
                    this.CreateAccessors(true);
                }
            }
            else if (this._irow != null)
            {
                this.BuildSchemaTableInfo(this._irow, false, false);
                if ((this._metadata != null) && (0 < this._metadata.Length))
                {
                    this.CreateBindingsFromMetaData(true);
                }
            }
            if (this._metadata == null)
            {
                this._hasRows = false;
                this._visibleFieldCount = 0;
                this._metadata = new System.Data.OleDb.MetaData[0];
            }
        }

        private DataTable BuildSchemaTable(System.Data.OleDb.MetaData[] metadata)
        {
            DataTable table = new DataTable("SchemaTable") {
                Locale = CultureInfo.InvariantCulture,
                MinimumCapacity = metadata.Length
            };
            DataColumn column19 = new DataColumn("ColumnName", typeof(string));
            DataColumn column3 = new DataColumn("ColumnOrdinal", typeof(int));
            DataColumn column18 = new DataColumn("ColumnSize", typeof(int));
            DataColumn column17 = new DataColumn("NumericPrecision", typeof(short));
            DataColumn column16 = new DataColumn("NumericScale", typeof(short));
            DataColumn column15 = new DataColumn("DataType", typeof(Type));
            DataColumn column14 = new DataColumn("ProviderType", typeof(int));
            DataColumn column2 = new DataColumn("IsLong", typeof(bool));
            DataColumn column = new DataColumn("AllowDBNull", typeof(bool));
            DataColumn column13 = new DataColumn("IsReadOnly", typeof(bool));
            DataColumn column12 = new DataColumn("IsRowVersion", typeof(bool));
            DataColumn column11 = new DataColumn("IsUnique", typeof(bool));
            DataColumn column10 = new DataColumn("IsKey", typeof(bool));
            DataColumn column9 = new DataColumn("IsAutoIncrement", typeof(bool));
            DataColumn column8 = new DataColumn("IsHidden", typeof(bool));
            DataColumn column7 = new DataColumn("BaseSchemaName", typeof(string));
            DataColumn column6 = new DataColumn("BaseCatalogName", typeof(string));
            DataColumn column5 = new DataColumn("BaseTableName", typeof(string));
            DataColumn column4 = new DataColumn("BaseColumnName", typeof(string));
            column3.DefaultValue = 0;
            column2.DefaultValue = false;
            DataColumnCollection columns = table.Columns;
            columns.Add(column19);
            columns.Add(column3);
            columns.Add(column18);
            columns.Add(column17);
            columns.Add(column16);
            columns.Add(column15);
            columns.Add(column14);
            columns.Add(column2);
            columns.Add(column);
            columns.Add(column13);
            columns.Add(column12);
            columns.Add(column11);
            columns.Add(column10);
            columns.Add(column9);
            if (this._visibleFieldCount < metadata.Length)
            {
                columns.Add(column8);
            }
            columns.Add(column7);
            columns.Add(column6);
            columns.Add(column5);
            columns.Add(column4);
            for (int i = 0; i < metadata.Length; i++)
            {
                System.Data.OleDb.MetaData data = metadata[i];
                DataRow row = table.NewRow();
                row[column19] = data.columnName;
                row[column3] = i;
                row[column18] = (data.type.enumOleDbType != OleDbType.BSTR) ? data.size : -1;
                row[column17] = data.precision;
                row[column16] = data.scale;
                row[column15] = data.type.dataType;
                row[column14] = data.type.enumOleDbType;
                row[column2] = IsLong(data.flags);
                if (data.isKeyColumn)
                {
                    row[column] = AllowDBNull(data.flags);
                }
                else
                {
                    row[column] = AllowDBNullMaybeNull(data.flags);
                }
                row[column13] = IsReadOnly(data.flags);
                row[column12] = IsRowVersion(data.flags);
                row[column11] = data.isUnique;
                row[column10] = data.isKeyColumn;
                row[column9] = data.isAutoIncrement;
                if (this._visibleFieldCount < metadata.Length)
                {
                    row[column8] = data.isHidden;
                }
                if (data.baseSchemaName != null)
                {
                    row[column7] = data.baseSchemaName;
                }
                if (data.baseCatalogName != null)
                {
                    row[column6] = data.baseCatalogName;
                }
                if (data.baseTableName != null)
                {
                    row[column5] = data.baseTableName;
                }
                if (data.baseColumnName != null)
                {
                    row[column4] = data.baseColumnName;
                }
                table.Rows.Add(row);
                row.AcceptChanges();
            }
            int count = columns.Count;
            for (int j = 0; j < count; j++)
            {
                columns[j].ReadOnly = true;
            }
            this._dbSchemaTable = table;
            return table;
        }

        private void BuildSchemaTableInfo(object handle, bool filterITypeInfo, bool filterChapters)
        {
            Bid.Trace("<oledb.IUnknown.QueryInterface|API|OLEDB|rowset_row> %d#, IColumnsInfo\n", this.ObjectID);
            UnsafeNativeMethods.IColumnsInfo columnsInfo = handle as UnsafeNativeMethods.IColumnsInfo;
            if (columnsInfo == null)
            {
                Bid.Trace("<oledb.IUnknown.QueryInterface|API|OLEDB|RET> %08X{HRESULT}\n", OleDbHResult.E_NOINTERFACE);
                this._dbSchemaTable = null;
            }
            else
            {
                OleDbHResult result;
                IntPtr ptrZero = ADP.PtrZero;
                IntPtr columnInfos = ADP.PtrZero;
                using (new DualCoTaskMem(columnsInfo, out ptrZero, out columnInfos, out result))
                {
                    if (result < OleDbHResult.S_OK)
                    {
                        this.ProcessResults(result);
                    }
                    if (0 < ((int) ptrZero))
                    {
                        this.BuildSchemaTableInfoTable(ptrZero.ToInt32(), columnInfos, filterITypeInfo, filterChapters);
                    }
                }
            }
        }

        private void BuildSchemaTableInfoTable(int columnCount, IntPtr columnInfos, bool filterITypeInfo, bool filterChapters)
        {
            int index = 0;
            System.Data.OleDb.MetaData[] dataArray = new System.Data.OleDb.MetaData[columnCount];
            tagDBCOLUMNINFO structure = new tagDBCOLUMNINFO();
            int num4 = 0;
            for (int i = 0; num4 < columnCount; i += ODB.SizeOf_tagDBCOLUMNINFO)
            {
                Marshal.PtrToStructure(ADP.IntPtrOffset(columnInfos, i), structure);
                if ((0 >= ((int) structure.iOrdinal)) || DoColumnDropFilter(structure.dwFlags))
                {
                    goto Label_01EC;
                }
                if (structure.pwszName == null)
                {
                    structure.pwszName = "";
                }
                if ((filterITypeInfo && ("DBCOLUMN_TYPEINFO" == structure.pwszName)) || (filterChapters && (0x88 == structure.wType)))
                {
                    goto Label_01EC;
                }
                bool isLong = IsLong(structure.dwFlags);
                bool isFixed = IsFixed(structure.dwFlags);
                NativeDBType type = NativeDBType.FromDBType(structure.wType, isLong, isFixed);
                System.Data.OleDb.MetaData data = new System.Data.OleDb.MetaData {
                    columnName = structure.pwszName,
                    type = type,
                    ordinal = structure.iOrdinal,
                    size = (int) structure.ulColumnSize,
                    flags = structure.dwFlags,
                    precision = structure.bPrecision,
                    scale = structure.bScale,
                    kind = structure.columnid.eKind
                };
                switch (structure.columnid.eKind)
                {
                    case 0:
                    case 1:
                    case 6:
                        data.guid = structure.columnid.uGuid;
                        break;

                    default:
                        data.guid = Guid.Empty;
                        break;
                }
                switch (structure.columnid.eKind)
                {
                    case 0:
                    case 2:
                        if (!(ADP.PtrZero != structure.columnid.ulPropid))
                        {
                            break;
                        }
                        data.idname = Marshal.PtrToStringUni(structure.columnid.ulPropid);
                        goto Label_01E3;

                    case 1:
                    case 5:
                        data.propid = structure.columnid.ulPropid;
                        goto Label_01E3;

                    default:
                        data.propid = ADP.PtrZero;
                        goto Label_01E3;
                }
                data.idname = null;
            Label_01E3:
                dataArray[index] = data;
                index++;
            Label_01EC:
                num4++;
            }
            if (index < columnCount)
            {
                System.Data.OleDb.MetaData[] dataArray2 = new System.Data.OleDb.MetaData[index];
                for (int j = 0; j < index; j++)
                {
                    dataArray2[j] = dataArray[j];
                }
                dataArray = dataArray2;
            }
            this._visibleFieldCount = index;
            this._metadata = dataArray;
        }

        private void BuildSchemaTableRowset(object handle)
        {
            Bid.Trace("<oledb.IUnknown.QueryInterface|API|OLEDB|rowset_row> %d, IColumnsRowset\n", this.ObjectID);
            UnsafeNativeMethods.IColumnsRowset icolumnsRowset = handle as UnsafeNativeMethods.IColumnsRowset;
            if (icolumnsRowset != null)
            {
                OleDbHResult result;
                IntPtr ptr;
                System.Data.Common.UnsafeNativeMethods.IRowset ppColRowset = null;
                using (DualCoTaskMem mem = new DualCoTaskMem(icolumnsRowset, out ptr, out result))
                {
                    Bid.Trace("<oledb.IColumnsRowset.GetColumnsRowset|API|OLEDB> %d#, IID_IRowset\n", this.ObjectID);
                    result = icolumnsRowset.GetColumnsRowset(ADP.PtrZero, ptr, mem, ref ODB.IID_IRowset, 0, ADP.PtrZero, out ppColRowset);
                    Bid.Trace("<oledb.IColumnsRowset.GetColumnsRowset|API|OLEDB|RET> %08X{HRESULT}\n", result);
                }
                if (result < OleDbHResult.S_OK)
                {
                    this.ProcessResults(result);
                }
                this.DumpToSchemaTable(ppColRowset);
                if (ppColRowset != null)
                {
                    Marshal.ReleaseComObject(ppColRowset);
                }
            }
            else
            {
                Bid.Trace("<oledb.IUnknown.QueryInterface|API|OLEDB|RET> %08X{HRESULT}\n", OleDbHResult.E_NOINTERFACE);
                this._useIColumnsRowset = false;
                this.BuildSchemaTableInfo(handle, false, false);
            }
        }

        public override void Close()
        {
            IntPtr ptr2;
            Bid.ScopeEnter(out ptr2, "<oledb.OleDbDataReader.Close|API> %d#\n", this.ObjectID);
            try
            {
                OleDbConnection connection = this._connection;
                OleDbCommand command = this._command;
                Bindings bindings = this._parameterBindings;
                this._connection = null;
                this._command = null;
                this._parameterBindings = null;
                this._isClosed = true;
                this.DisposeOpenResults();
                this._hasRows = false;
                if ((command != null) && command.canceling)
                {
                    this.DisposeNativeMultipleResults();
                    if (bindings != null)
                    {
                        bindings.CloseFromConnection();
                        bindings = null;
                    }
                }
                else
                {
                    UnsafeNativeMethods.IMultipleResults imultipleResults = this._imultipleResults;
                    this._imultipleResults = null;
                    if (imultipleResults != null)
                    {
                        try
                        {
                            if ((command != null) && !command.canceling)
                            {
                                IntPtr zero = IntPtr.Zero;
                                OleDbException exception = NextResults(imultipleResults, null, command, out zero);
                                this._recordsAffected = AddRecordsAffected(this._recordsAffected, zero);
                                if (exception != null)
                                {
                                    throw exception;
                                }
                            }
                        }
                        finally
                        {
                            if (imultipleResults != null)
                            {
                                Marshal.ReleaseComObject(imultipleResults);
                            }
                        }
                    }
                }
                if ((command != null) && (this._depth == 0))
                {
                    command.CloseFromDataReader(bindings);
                }
                if (connection != null)
                {
                    connection.RemoveWeakReference(this);
                    if (this.IsCommandBehavior(CommandBehavior.CloseConnection))
                    {
                        connection.Close();
                    }
                }
                RowHandleBuffer buffer = this._rowHandleNativeBuffer;
                this._rowHandleNativeBuffer = null;
                if (buffer != null)
                {
                    buffer.Dispose();
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr2);
            }
        }

        internal void CloseReaderFromConnection(bool canceling)
        {
            if (this._command != null)
            {
                this._command.canceling = canceling;
            }
            this._connection = null;
            this.Close();
        }

        private void CreateAccessors(bool allowMultipleAccessor)
        {
            Bindings[] bindingsArray = this.CreateBindingsFromMetaData(allowMultipleAccessor);
            System.Data.Common.UnsafeNativeMethods.IAccessor iaccessor = this.IAccessor();
            for (int i = 0; i < bindingsArray.Length; i++)
            {
                OleDbHResult hr = bindingsArray[i].CreateAccessor(iaccessor, 2);
                if (hr < OleDbHResult.S_OK)
                {
                    this.ProcessResults(hr);
                }
            }
            if (IntPtr.Zero == this._rowHandleFetchCount)
            {
                this._rowHandleFetchCount = new IntPtr(1);
                object propertyValue = this.GetPropertyValue(0x49);
                if (propertyValue is int)
                {
                    this._rowHandleFetchCount = new IntPtr((int) propertyValue);
                    if ((ADP.PtrZero == this._rowHandleFetchCount) || (20 <= ((int) this._rowHandleFetchCount)))
                    {
                        this._rowHandleFetchCount = new IntPtr(20);
                    }
                }
                else if (propertyValue is long)
                {
                    this._rowHandleFetchCount = new IntPtr((long) propertyValue);
                    if ((ADP.PtrZero == this._rowHandleFetchCount) || (20L <= ((long) this._rowHandleFetchCount)))
                    {
                        this._rowHandleFetchCount = new IntPtr(20);
                    }
                }
            }
            if (this._rowHandleNativeBuffer == null)
            {
                this._rowHandleNativeBuffer = new RowHandleBuffer(this._rowHandleFetchCount);
            }
        }

        private Bindings[] CreateBindingsFromMetaData(bool allowMultipleAccessor)
        {
            int count = 0;
            int num8 = 0;
            System.Data.OleDb.MetaData[] dataArray = this._metadata;
            int[] numArray = new int[dataArray.Length];
            int[] numArray2 = new int[dataArray.Length];
            if (allowMultipleAccessor)
            {
                if (this._irowset != null)
                {
                    for (int k = 0; k < numArray.Length; k++)
                    {
                        numArray[k] = count;
                        numArray2[k] = num8;
                        num8++;
                    }
                    if (0 < num8)
                    {
                        count++;
                    }
                }
                else if (this._irow != null)
                {
                    for (int m = 0; m < numArray.Length; m++)
                    {
                        numArray[m] = m;
                        numArray2[m] = 0;
                    }
                    count = dataArray.Length;
                }
            }
            else
            {
                for (int n = 0; n < numArray.Length; n++)
                {
                    numArray[n] = 0;
                    numArray2[n] = n;
                }
                count = 1;
            }
            Bindings[] bindingsArray = new Bindings[count];
            count = 0;
            for (int i = 0; i < dataArray.Length; i++)
            {
                Bindings bindings = bindingsArray[numArray[i]];
                if (bindings == null)
                {
                    count = 0;
                    for (int num12 = i; (num12 < dataArray.Length) && (count == numArray2[num12]); num12++)
                    {
                        count++;
                    }
                    bindingsArray[numArray[i]] = bindings = new Bindings(this, null != this._irowset, count);
                }
                System.Data.OleDb.MetaData data = dataArray[i];
                int fixlen = data.type.fixlen;
                short wType = data.type.wType;
                if (-1 != data.size)
                {
                    if (data.type.islong)
                    {
                        fixlen = ADP.PtrSize;
                        wType = (short) (((ushort) wType) | 0x4000);
                    }
                    else if (-1 == fixlen)
                    {
                        if (0x2000 < data.size)
                        {
                            fixlen = ADP.PtrSize;
                            wType = (short) (((ushort) wType) | 0x4000);
                        }
                        else if ((130 == wType) && (-1 != data.size))
                        {
                            fixlen = (data.size * 2) + 2;
                        }
                        else
                        {
                            fixlen = data.size;
                        }
                    }
                }
                else if (fixlen < 0)
                {
                    fixlen = ADP.PtrSize;
                    wType = (short) (((ushort) wType) | 0x4000);
                }
                num8 = numArray2[i];
                bindings.CurrentIndex = num8;
                bindings.Ordinal = data.ordinal;
                bindings.Part = data.type.dbPart;
                bindings.Precision = data.precision;
                bindings.Scale = data.scale;
                bindings.DbType = wType;
                bindings.MaxLen = fixlen;
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<oledb.struct.tagDBBINDING|INFO|ADV> index=%d, columnName='%ls'\n", i, data.columnName);
                }
            }
            int index = 0;
            int indexStart = 0;
            for (int j = 0; j < bindingsArray.Length; j++)
            {
                indexStart = bindingsArray[j].AllocateForAccessor(this, indexStart, j);
                ColumnBinding[] bindingArray = bindingsArray[j].ColumnBindings();
                for (int num10 = 0; num10 < bindingArray.Length; num10++)
                {
                    dataArray[index].columnBinding = bindingArray[num10];
                    dataArray[index].bindings = bindingsArray[j];
                    index++;
                }
            }
            this._bindings = bindingsArray;
            return bindingsArray;
        }

        private void DisposeManagedRowset()
        {
            this._isRead = false;
            this._hasRowsReadCheck = false;
            this._nextAccessorForRetrieval = 0;
            this._nextValueForRetrieval = 0;
            Bindings[] bindingsArray = this._bindings;
            this._bindings = null;
            if (bindingsArray != null)
            {
                for (int i = 0; i < bindingsArray.Length; i++)
                {
                    if (bindingsArray[i] != null)
                    {
                        bindingsArray[i].Dispose();
                    }
                }
            }
            this._currentRow = 0;
            this._rowFetchedCount = IntPtr.Zero;
            this._dbSchemaTable = null;
            this._visibleFieldCount = 0;
            this._metadata = null;
            this._fieldNameLookup = null;
        }

        private void DisposeNativeMultipleResults()
        {
            UnsafeNativeMethods.IMultipleResults o = this._imultipleResults;
            this._imultipleResults = null;
            if (o != null)
            {
                Marshal.ReleaseComObject(o);
            }
        }

        private void DisposeNativeRow()
        {
            System.Data.Common.UnsafeNativeMethods.IRow o = this._irow;
            this._irow = null;
            if (o != null)
            {
                Marshal.ReleaseComObject(o);
            }
        }

        private void DisposeNativeRowset()
        {
            System.Data.Common.UnsafeNativeMethods.IRowset o = this._irowset;
            this._irowset = null;
            ChapterHandle handle = this._chapterHandle;
            this._chapterHandle = ChapterHandle.DB_NULL_HCHAPTER;
            if (ChapterHandle.DB_NULL_HCHAPTER != handle)
            {
                handle.Dispose();
            }
            if (o != null)
            {
                Marshal.ReleaseComObject(o);
            }
        }

        private void DisposeOpenResults()
        {
            this.DisposeManagedRowset();
            this.DisposeNativeRow();
            this.DisposeNativeRowset();
        }

        private static bool DoColumnDropFilter(int flags)
        {
            return (0 != (1 & flags));
        }

        private ColumnBinding DoSequentialCheck(int ordinal, long dataIndex, string method)
        {
            ColumnBinding columnBinding = this.GetColumnBinding(ordinal);
            if (dataIndex > 0x7fffffffL)
            {
                throw ADP.InvalidSourceBufferIndex(0, dataIndex, "dataIndex");
            }
            if (this._sequentialOrdinal != ordinal)
            {
                this._sequentialOrdinal = ordinal;
                this._sequentialBytesRead = 0L;
                return columnBinding;
            }
            if (this._sequentialAccess && (this._sequentialBytesRead < dataIndex))
            {
                throw ADP.NonSeqByteAccess(dataIndex, this._sequentialBytesRead, method);
            }
            return columnBinding;
        }

        private System.Data.OleDb.MetaData DoValueCheck(int ordinal)
        {
            if (!this._isRead)
            {
                throw ADP.DataReaderNoData();
            }
            if (this._sequentialAccess && (ordinal < this._nextValueForRetrieval))
            {
                throw ADP.NonSequentialColumnAccess(ordinal, this._nextValueForRetrieval);
            }
            return this._metadata[ordinal];
        }

        internal void DumpToSchemaTable(System.Data.Common.UnsafeNativeMethods.IRowset rowset)
        {
            List<System.Data.OleDb.MetaData> list = new List<System.Data.OleDb.MetaData>();
            object propertyValue = null;
            using (OleDbDataReader reader = new OleDbDataReader(this._connection, this._command, -2147483648, CommandBehavior.Default))
            {
                reader.InitializeIRowset(rowset, ChapterHandle.DB_NULL_HCHAPTER, IntPtr.Zero);
                reader.BuildSchemaTableInfo(rowset, true, false);
                propertyValue = this.GetPropertyValue(0x102);
                if (reader.FieldCount == 0)
                {
                    return;
                }
                FieldNameLookup lookup = new FieldNameLookup(reader, -1);
                reader._fieldNameLookup = lookup;
                System.Data.OleDb.MetaData data20 = reader.FindMetaData("DBCOLUMN_IDNAME");
                System.Data.OleDb.MetaData data19 = reader.FindMetaData("DBCOLUMN_GUID");
                System.Data.OleDb.MetaData data18 = reader.FindMetaData("DBCOLUMN_PROPID");
                System.Data.OleDb.MetaData data17 = reader.FindMetaData("DBCOLUMN_NAME");
                System.Data.OleDb.MetaData data12 = reader.FindMetaData("DBCOLUMN_NUMBER");
                System.Data.OleDb.MetaData data16 = reader.FindMetaData("DBCOLUMN_TYPE");
                System.Data.OleDb.MetaData data11 = reader.FindMetaData("DBCOLUMN_COLUMNSIZE");
                System.Data.OleDb.MetaData data15 = reader.FindMetaData("DBCOLUMN_PRECISION");
                System.Data.OleDb.MetaData data14 = reader.FindMetaData("DBCOLUMN_SCALE");
                System.Data.OleDb.MetaData data13 = reader.FindMetaData("DBCOLUMN_FLAGS");
                System.Data.OleDb.MetaData data10 = reader.FindMetaData("DBCOLUMN_BASESCHEMANAME");
                System.Data.OleDb.MetaData data9 = reader.FindMetaData("DBCOLUMN_BASECATALOGNAME");
                System.Data.OleDb.MetaData data8 = reader.FindMetaData("DBCOLUMN_BASETABLENAME");
                System.Data.OleDb.MetaData data7 = reader.FindMetaData("DBCOLUMN_BASECOLUMNNAME");
                System.Data.OleDb.MetaData data6 = reader.FindMetaData("DBCOLUMN_ISAUTOINCREMENT");
                System.Data.OleDb.MetaData data5 = reader.FindMetaData("DBCOLUMN_ISUNIQUE");
                System.Data.OleDb.MetaData data4 = reader.FindMetaData("DBCOLUMN_KEYCOLUMN");
                reader.CreateAccessors(false);
                while (reader.ReadRowset())
                {
                    reader.GetRowDataFromHandle();
                    System.Data.OleDb.MetaData item = new System.Data.OleDb.MetaData();
                    ColumnBinding columnBinding = data20.columnBinding;
                    if (!columnBinding.IsValueNull())
                    {
                        item.idname = (string) columnBinding.Value();
                        item.kind = 2;
                    }
                    columnBinding = data19.columnBinding;
                    if (!columnBinding.IsValueNull())
                    {
                        item.guid = columnBinding.Value_GUID();
                        item.kind = (2 == item.kind) ? 0 : 6;
                    }
                    columnBinding = data18.columnBinding;
                    if (!columnBinding.IsValueNull())
                    {
                        item.propid = new IntPtr((long) columnBinding.Value_UI4());
                        item.kind = (6 == item.kind) ? 1 : 5;
                    }
                    columnBinding = data17.columnBinding;
                    if (!columnBinding.IsValueNull())
                    {
                        item.columnName = (string) columnBinding.Value();
                    }
                    else
                    {
                        item.columnName = "";
                    }
                    if (4 == ADP.PtrSize)
                    {
                        item.ordinal = (IntPtr) data12.columnBinding.Value_UI4();
                    }
                    else
                    {
                        item.ordinal = (IntPtr) data12.columnBinding.Value_UI8();
                    }
                    short dbType = (short) data16.columnBinding.Value_UI2();
                    if (4 == ADP.PtrSize)
                    {
                        item.size = (int) data11.columnBinding.Value_UI4();
                    }
                    else
                    {
                        item.size = ADP.IntPtrToInt32((IntPtr) data11.columnBinding.Value_UI8());
                    }
                    columnBinding = data15.columnBinding;
                    if (!columnBinding.IsValueNull())
                    {
                        item.precision = (byte) columnBinding.Value_UI2();
                    }
                    columnBinding = data14.columnBinding;
                    if (!columnBinding.IsValueNull())
                    {
                        item.scale = (byte) columnBinding.Value_I2();
                    }
                    item.flags = (int) data13.columnBinding.Value_UI4();
                    bool isLong = IsLong(item.flags);
                    bool isFixed = IsFixed(item.flags);
                    NativeDBType type = NativeDBType.FromDBType(dbType, isLong, isFixed);
                    item.type = type;
                    if (data6 != null)
                    {
                        columnBinding = data6.columnBinding;
                        if (!columnBinding.IsValueNull())
                        {
                            item.isAutoIncrement = columnBinding.Value_BOOL();
                        }
                    }
                    if (data5 != null)
                    {
                        columnBinding = data5.columnBinding;
                        if (!columnBinding.IsValueNull())
                        {
                            item.isUnique = columnBinding.Value_BOOL();
                        }
                    }
                    if (data4 != null)
                    {
                        columnBinding = data4.columnBinding;
                        if (!columnBinding.IsValueNull())
                        {
                            item.isKeyColumn = columnBinding.Value_BOOL();
                        }
                    }
                    if (data10 != null)
                    {
                        columnBinding = data10.columnBinding;
                        if (!columnBinding.IsValueNull())
                        {
                            item.baseSchemaName = columnBinding.ValueString();
                        }
                    }
                    if (data9 != null)
                    {
                        columnBinding = data9.columnBinding;
                        if (!columnBinding.IsValueNull())
                        {
                            item.baseCatalogName = columnBinding.ValueString();
                        }
                    }
                    if (data8 != null)
                    {
                        columnBinding = data8.columnBinding;
                        if (!columnBinding.IsValueNull())
                        {
                            item.baseTableName = columnBinding.ValueString();
                        }
                    }
                    if (data7 != null)
                    {
                        columnBinding = data7.columnBinding;
                        if (!columnBinding.IsValueNull())
                        {
                            item.baseColumnName = columnBinding.ValueString();
                        }
                    }
                    list.Add(item);
                }
            }
            int count = list.Count;
            if (propertyValue is int)
            {
                count -= (int) propertyValue;
            }
            bool flag = false;
            for (int i = list.Count - 1; count <= i; i--)
            {
                System.Data.OleDb.MetaData data3 = list[i];
                data3.isHidden = true;
                if (flag)
                {
                    data3.isKeyColumn = false;
                }
                else if (data3.guid.Equals(ODB.DBCOL_SPECIALCOL))
                {
                    data3.isKeyColumn = false;
                    flag = true;
                    for (int k = list.Count - 1; i < k; k--)
                    {
                        list[k].isKeyColumn = false;
                    }
                }
            }
            for (int j = count - 1; 0 <= j; j--)
            {
                System.Data.OleDb.MetaData data2 = list[j];
                if (flag)
                {
                    data2.isKeyColumn = false;
                }
                if (data2.guid.Equals(ODB.DBCOL_SPECIALCOL))
                {
                    data2.isHidden = true;
                    count--;
                }
                else if (0 >= ((int) data2.ordinal))
                {
                    data2.isHidden = true;
                    count--;
                }
                else if (DoColumnDropFilter(data2.flags))
                {
                    data2.isHidden = true;
                    count--;
                }
            }
            list.Sort();
            this._visibleFieldCount = count;
            this._metadata = list.ToArray();
        }

        private System.Data.OleDb.MetaData FindMetaData(string name)
        {
            int index = this._fieldNameLookup.IndexOfName(name);
            if (-1 == index)
            {
                return null;
            }
            return this._metadata[index];
        }

        internal static void GenerateSchemaTable(OleDbDataReader dataReader, object handle, CommandBehavior behavior)
        {
            if ((CommandBehavior.KeyInfo & behavior) != CommandBehavior.Default)
            {
                dataReader.BuildSchemaTableRowset(handle);
                dataReader.AppendSchemaInfo();
            }
            else
            {
                dataReader.BuildSchemaTableInfo(handle, false, false);
            }
            System.Data.OleDb.MetaData[] metaData = dataReader.MetaData;
            if ((metaData != null) && (0 < metaData.Length))
            {
                dataReader.BuildSchemaTable(metaData);
            }
        }

        public override bool GetBoolean(int ordinal)
        {
            return this.GetColumnBinding(ordinal).ValueBoolean();
        }

        public override byte GetByte(int ordinal)
        {
            return this.GetColumnBinding(ordinal).ValueByte();
        }

        public override long GetBytes(int ordinal, long dataIndex, byte[] buffer, int bufferIndex, int length)
        {
            byte[] src = this.DoSequentialCheck(ordinal, dataIndex, "GetBytes").ValueByteArray();
            if (buffer == null)
            {
                return (long) src.Length;
            }
            int srcOffset = (int) dataIndex;
            int count = Math.Min(src.Length - srcOffset, length);
            if (srcOffset < 0)
            {
                throw ADP.InvalidSourceBufferIndex(src.Length, (long) srcOffset, "dataIndex");
            }
            if ((bufferIndex < 0) || (bufferIndex >= buffer.Length))
            {
                throw ADP.InvalidDestinationBufferIndex(buffer.Length, bufferIndex, "bufferIndex");
            }
            if (0 < count)
            {
                Buffer.BlockCopy(src, srcOffset, buffer, bufferIndex, count);
                this._sequentialBytesRead = srcOffset + count;
            }
            else
            {
                if (length < 0)
                {
                    throw ADP.InvalidDataLength((long) length);
                }
                count = 0;
            }
            return (long) count;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override char GetChar(int ordinal)
        {
            throw ADP.NotSupported();
        }

        public override long GetChars(int ordinal, long dataIndex, char[] buffer, int bufferIndex, int length)
        {
            string str = this.DoSequentialCheck(ordinal, dataIndex, "GetChars").ValueString();
            if (buffer == null)
            {
                return (long) str.Length;
            }
            int sourceIndex = (int) dataIndex;
            int count = Math.Min(str.Length - sourceIndex, length);
            if (sourceIndex < 0)
            {
                throw ADP.InvalidSourceBufferIndex(str.Length, (long) sourceIndex, "dataIndex");
            }
            if ((bufferIndex < 0) || (bufferIndex >= buffer.Length))
            {
                throw ADP.InvalidDestinationBufferIndex(buffer.Length, bufferIndex, "bufferIndex");
            }
            if (0 < count)
            {
                str.CopyTo(sourceIndex, buffer, bufferIndex, count);
                this._sequentialBytesRead = sourceIndex + count;
            }
            else
            {
                if (length < 0)
                {
                    throw ADP.InvalidDataLength((long) length);
                }
                count = 0;
            }
            return (long) count;
        }

        private ColumnBinding GetColumnBinding(int ordinal)
        {
            System.Data.OleDb.MetaData info = this.DoValueCheck(ordinal);
            return this.GetValueBinding(info);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public OleDbDataReader GetData(int ordinal)
        {
            return this.GetColumnBinding(ordinal).ValueChapter();
        }

        private OleDbDataReader GetDataForReader(IntPtr ordinal, RowBinding rowbinding, int valueOffset)
        {
            System.Data.Common.UnsafeNativeMethods.IRowset rowset;
            System.Data.Common.UnsafeNativeMethods.IRowsetInfo info = this.IRowsetInfo();
            Bid.Trace("<oledb.IRowsetInfo.GetReferencedRowset|API|OLEDB> %d#, ColumnOrdinal=%Id\n", this.ObjectID, ordinal);
            OleDbHResult result = info.GetReferencedRowset(ordinal, ref ODB.IID_IRowset, out rowset);
            Bid.Trace("<oledb.IRowsetInfo.GetReferencedRowset|API|OLEDB|RET> %08X{HRESULT}\n", result);
            this.ProcessResults(result);
            OleDbDataReader reader = null;
            if (rowset != null)
            {
                ChapterHandle chapterHandle = ChapterHandle.CreateChapterHandle(rowset, rowbinding, valueOffset);
                reader = new OleDbDataReader(this._connection, this._command, 1 + this.Depth, this._commandBehavior & ~CommandBehavior.CloseConnection);
                reader.InitializeIRowset(rowset, chapterHandle, ADP.RecordsUnaffected);
                reader.BuildMetaInfo();
                reader.HasRowsRead();
                if (this._connection != null)
                {
                    this._connection.AddWeakReference(reader, 2);
                }
            }
            return reader;
        }

        public override string GetDataTypeName(int index)
        {
            if (this._metadata == null)
            {
                throw ADP.DataReaderNoData();
            }
            return this._metadata[index].type.dataSourceType;
        }

        public override DateTime GetDateTime(int ordinal)
        {
            return this.GetColumnBinding(ordinal).ValueDateTime();
        }

        protected override DbDataReader GetDbDataReader(int ordinal)
        {
            return this.GetData(ordinal);
        }

        public override decimal GetDecimal(int ordinal)
        {
            return this.GetColumnBinding(ordinal).ValueDecimal();
        }

        public override double GetDouble(int ordinal)
        {
            return this.GetColumnBinding(ordinal).ValueDouble();
        }

        public override IEnumerator GetEnumerator()
        {
            return new DbEnumerator(this, this.IsCommandBehavior(CommandBehavior.CloseConnection));
        }

        public override Type GetFieldType(int index)
        {
            if (this._metadata == null)
            {
                throw ADP.DataReaderNoData();
            }
            return this._metadata[index].type.dataType;
        }

        public override float GetFloat(int ordinal)
        {
            return this.GetColumnBinding(ordinal).ValueSingle();
        }

        public override Guid GetGuid(int ordinal)
        {
            return this.GetColumnBinding(ordinal).ValueGuid();
        }

        public override short GetInt16(int ordinal)
        {
            return this.GetColumnBinding(ordinal).ValueInt16();
        }

        public override int GetInt32(int ordinal)
        {
            return this.GetColumnBinding(ordinal).ValueInt32();
        }

        public override long GetInt64(int ordinal)
        {
            return this.GetColumnBinding(ordinal).ValueInt64();
        }

        public override string GetName(int index)
        {
            if (this._metadata == null)
            {
                throw ADP.DataReaderNoData();
            }
            return this._metadata[index].columnName;
        }

        public override int GetOrdinal(string name)
        {
            if (this._fieldNameLookup == null)
            {
                if (this._metadata == null)
                {
                    throw ADP.DataReaderNoData();
                }
                this._fieldNameLookup = new FieldNameLookup(this, -1);
            }
            return this._fieldNameLookup.GetOrdinal(name);
        }

        private object GetPropertyOnRowset(Guid propertySet, int propertyID)
        {
            tagDBPROP[] gdbpropArray;
            System.Data.Common.UnsafeNativeMethods.IRowsetInfo properties = this.IRowsetInfo();
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

        private object GetPropertyValue(int propertyId)
        {
            if (this._irowset != null)
            {
                return this.GetPropertyOnRowset(OleDbPropertySetGuid.Rowset, propertyId);
            }
            if (this._command != null)
            {
                return this._command.GetPropertyValue(OleDbPropertySetGuid.Rowset, propertyId);
            }
            return OleDbPropertyStatus.NotSupported;
        }

        private void GetRowDataFromHandle()
        {
            OleDbHResult result = OleDbHResult.S_OK;
            System.Data.Common.UnsafeNativeMethods.IRowset rowset = this.IRowset();
            IntPtr rowHandle = this._rowHandleNativeBuffer.GetRowHandle(this._currentRow);
            RowBinding binding = this._bindings[this._nextAccessorForRetrieval].RowBinding();
            IntPtr accessorHandle = binding.DangerousGetAccessorHandle();
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                binding.DangerousAddRef(ref success);
                binding.StartDataBlock();
                IntPtr dataPtr = binding.DangerousGetDataPtr();
                Bid.Trace("<oledb.IRowset.GetData|API|OLEDB> %d#, RowHandle=%Id, AccessorHandle=%Id\n", this.ObjectID, rowHandle, accessorHandle);
                result = rowset.GetData(rowHandle, accessorHandle, dataPtr);
                Bid.Trace("<oledb.IRowset.GetData|API|OLEDB|RET> %08X{HRESULT}\n", result);
            }
            finally
            {
                if (success)
                {
                    binding.DangerousRelease();
                }
            }
            this._nextAccessorForRetrieval++;
            if (result < OleDbHResult.S_OK)
            {
                this.ProcessResults(result);
            }
        }

        private void GetRowHandles()
        {
            OleDbHResult result = OleDbHResult.S_OK;
            RowHandleBuffer buffer = this._rowHandleNativeBuffer;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                buffer.DangerousAddRef(ref success);
                IntPtr handle = buffer.DangerousGetHandle();
                System.Data.Common.UnsafeNativeMethods.IRowset rowset = this.IRowset();
                try
                {
                    Bid.Trace("<oledb.IRowset.GetNextRows|API|OLEDB> %d#, Chapter=%Id, RowsRequested=%Id\n", this.ObjectID, this._chapterHandle.HChapter, this._rowHandleFetchCount);
                    result = rowset.GetNextRows(this._chapterHandle.HChapter, IntPtr.Zero, this._rowHandleFetchCount, out this._rowFetchedCount, ref handle);
                    Bid.Trace("<oledb.IRowset.GetNextRows|API|OLEDB|RET> %08X{HRESULT}, RowsObtained=%Id\n", result, this._rowFetchedCount);
                }
                catch (InvalidCastException exception)
                {
                    throw ODB.ThreadApartmentState(exception);
                }
            }
            finally
            {
                if (success)
                {
                    buffer.DangerousRelease();
                }
            }
            if (result < OleDbHResult.S_OK)
            {
                this.ProcessResults(result);
            }
            this._isRead = (OleDbHResult.DB_S_ENDOFROWSET != result) || (0 < ((int) this._rowFetchedCount));
            this._rowFetchedCount = (IntPtr) Math.Max((int) this._rowFetchedCount, 0);
        }

        private void GetRowValue()
        {
            Bindings bindings = this._bindings[this._nextAccessorForRetrieval];
            ColumnBinding[] bindingArray = bindings.ColumnBindings();
            RowBinding binding = bindings.RowBinding();
            bool success = false;
            bool[] flagArray = new bool[bindingArray.Length];
            StringMemHandle[] handleArray = new StringMemHandle[bindingArray.Length];
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                for (int i = 0; i < bindingArray.Length; i++)
                {
                    bindings.CurrentIndex = i;
                    handleArray[i] = null;
                    System.Data.OleDb.MetaData data = this._metadata[bindingArray[i].Index];
                    if ((data.kind == 0) || (2 == data.kind))
                    {
                        handleArray[i] = new StringMemHandle(data.idname);
                        bindingArray[i]._sptr = handleArray[i];
                    }
                    handleArray[i].DangerousAddRef(ref flagArray[i]);
                    IntPtr propid = (handleArray[i] != null) ? handleArray[i].DangerousGetHandle() : data.propid;
                    bindings.GuidKindName(data.guid, data.kind, propid);
                }
                tagDBCOLUMNACCESS[] dBColumnAccess = bindings.DBColumnAccess;
                binding.DangerousAddRef(ref success);
                binding.StartDataBlock();
                System.Data.Common.UnsafeNativeMethods.IRow row = this.IRow();
                Bid.Trace("<oledb.IRow.GetColumns|API|OLEDB> %d#\n", this.ObjectID);
                OleDbHResult columns = row.GetColumns((IntPtr) dBColumnAccess.Length, dBColumnAccess);
                Bid.Trace("<oledb.IRow.GetColumns|API|OLEDB|RET> %08X{HRESULT}\n", columns);
            }
            finally
            {
                if (success)
                {
                    binding.DangerousRelease();
                }
                for (int j = 0; j < flagArray.Length; j++)
                {
                    if (flagArray[j])
                    {
                        handleArray[j].DangerousRelease();
                    }
                }
            }
            this._nextAccessorForRetrieval++;
        }

        public override DataTable GetSchemaTable()
        {
            DataTable table2;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<oledb.OleDbDataReader.GetSchemaTable|API> %d#\n", this.ObjectID);
            try
            {
                DataTable table = this._dbSchemaTable;
                if (table == null)
                {
                    System.Data.OleDb.MetaData[] metaData = this.MetaData;
                    if ((metaData != null) && (0 < metaData.Length))
                    {
                        if (((0 < metaData.Length) && this._useIColumnsRowset) && (this._connection != null))
                        {
                            this.AppendSchemaInfo();
                        }
                        table = this.BuildSchemaTable(metaData);
                    }
                    else if (this.IsClosed)
                    {
                        throw ADP.DataReaderClosed("GetSchemaTable");
                    }
                }
                table2 = table;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return table2;
        }

        public override string GetString(int ordinal)
        {
            return this.GetColumnBinding(ordinal).ValueString();
        }

        public TimeSpan GetTimeSpan(int ordinal)
        {
            return (TimeSpan) this.GetValue(ordinal);
        }

        public override object GetValue(int ordinal)
        {
            return this.GetColumnBinding(ordinal).Value();
        }

        private ColumnBinding GetValueBinding(System.Data.OleDb.MetaData info)
        {
            ColumnBinding columnBinding = info.columnBinding;
            for (int i = this._nextAccessorForRetrieval; i <= columnBinding.IndexForAccessor; i++)
            {
                if (this._sequentialAccess)
                {
                    if (this._nextValueForRetrieval != columnBinding.Index)
                    {
                        this._metadata[this._nextValueForRetrieval].columnBinding.ResetValue();
                    }
                    this._nextAccessorForRetrieval = columnBinding.IndexForAccessor;
                }
                if (this._irowset != null)
                {
                    this.GetRowDataFromHandle();
                }
                else
                {
                    if (this._irow == null)
                    {
                        throw ADP.DataReaderNoData();
                    }
                    this.GetRowValue();
                }
            }
            this._nextValueForRetrieval = columnBinding.Index;
            return columnBinding;
        }

        public override int GetValues(object[] values)
        {
            if (values == null)
            {
                throw ADP.ArgumentNull("values");
            }
            this.DoValueCheck(0);
            int num2 = Math.Min(values.Length, this._visibleFieldCount);
            for (int i = 0; (i < this._metadata.Length) && (i < num2); i++)
            {
                values[i] = this.GetValueBinding(this._metadata[i]).Value();
            }
            return num2;
        }

        internal void HasRowsRead()
        {
            bool flag = this.Read();
            this._hasRows = flag;
            this._hasRowsReadCheck = true;
            this._isRead = false;
        }

        private System.Data.Common.UnsafeNativeMethods.IAccessor IAccessor()
        {
            Bid.Trace("<oledb.IUnknown.QueryInterface|API|OLEDB|rowset> %d#, IAccessor\n", this.ObjectID);
            return (System.Data.Common.UnsafeNativeMethods.IAccessor) this.IRowset();
        }

        private int IndexOf(Hashtable hash, string name)
        {
            object obj2 = hash[name];
            if (obj2 == null)
            {
                string str = name.ToLower(CultureInfo.InvariantCulture);
                obj2 = hash[str];
                if (obj2 == null)
                {
                    return -1;
                }
            }
            return (int) obj2;
        }

        private void Initialize()
        {
            CommandBehavior behavior = this._commandBehavior;
            this._useIColumnsRowset = CommandBehavior.Default != (CommandBehavior.KeyInfo & behavior);
            this._sequentialAccess = CommandBehavior.Default != (CommandBehavior.SequentialAccess & behavior);
            if (this._depth == 0)
            {
                this._singleRow = CommandBehavior.Default != (CommandBehavior.SingleRow & behavior);
            }
        }

        internal void InitializeIMultipleResults(object result)
        {
            this.Initialize();
            this._imultipleResults = (UnsafeNativeMethods.IMultipleResults) result;
        }

        internal void InitializeIRow(object result, IntPtr recordsAffected)
        {
            this.Initialize();
            this._singleRow = true;
            this._recordsAffected = recordsAffected;
            this._irow = (System.Data.Common.UnsafeNativeMethods.IRow) result;
            this._hasRows = null != this._irow;
        }

        internal void InitializeIRowset(object result, ChapterHandle chapterHandle, IntPtr recordsAffected)
        {
            if ((this._connection == null) || (ChapterHandle.DB_NULL_HCHAPTER != chapterHandle))
            {
                this._rowHandleFetchCount = new IntPtr(1);
            }
            this.Initialize();
            this._recordsAffected = recordsAffected;
            this._irowset = (System.Data.Common.UnsafeNativeMethods.IRowset) result;
            this._chapterHandle = chapterHandle;
        }

        private System.Data.Common.UnsafeNativeMethods.IRow IRow()
        {
            System.Data.Common.UnsafeNativeMethods.IRow row = this._irow;
            if (row == null)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            return row;
        }

        private System.Data.Common.UnsafeNativeMethods.IRowset IRowset()
        {
            System.Data.Common.UnsafeNativeMethods.IRowset rowset = this._irowset;
            if (rowset == null)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            return rowset;
        }

        private System.Data.Common.UnsafeNativeMethods.IRowsetInfo IRowsetInfo()
        {
            Bid.Trace("<oledb.IUnknown.QueryInterface|API|OLEDB|rowset> %d#, IRowsetInfo\n", this.ObjectID);
            return (System.Data.Common.UnsafeNativeMethods.IRowsetInfo) this.IRowset();
        }

        private bool IsCommandBehavior(CommandBehavior condition)
        {
            return (condition == (condition & this._commandBehavior));
        }

        public override bool IsDBNull(int ordinal)
        {
            return this.GetColumnBinding(ordinal).IsValueNull();
        }

        private static bool IsFixed(int flags)
        {
            return (0 != (0x10 & flags));
        }

        private static bool IsLong(int flags)
        {
            return (0 != (0x80 & flags));
        }

        private static bool IsReadOnly(int flags)
        {
            return (0 == (12 & flags));
        }

        private static bool IsRowVersion(int flags)
        {
            return (0 != (0x300 & flags));
        }

        public override bool NextResult()
        {
            bool flag2;
            IntPtr ptr2;
            Bid.ScopeEnter(out ptr2, "<oledb.OleDbDataReader.NextResult|API> %d#\n", this.ObjectID);
            try
            {
                bool flag = false;
                if (this.IsClosed)
                {
                    throw ADP.DataReaderClosed("NextResult");
                }
                this._fieldNameLookup = null;
                OleDbCommand command = this._command;
                UnsafeNativeMethods.IMultipleResults results = this._imultipleResults;
                if (results != null)
                {
                    this.DisposeOpenResults();
                    this._hasRows = false;
                    do
                    {
                        IntPtr ptr;
                        object ppRowset = null;
                        if ((command != null) && command.canceling)
                        {
                            this.Close();
                            goto Label_0116;
                        }
                        Bid.Trace("<oledb.IMultipleResults.GetResult|API|OLEDB> %d#, IID_IRowset\n", this.ObjectID);
                        OleDbHResult result = results.GetResult(ADP.PtrZero, ODB.DBRESULTFLAG_DEFAULT, ref ODB.IID_IRowset, out ptr, out ppRowset);
                        Bid.Trace("<oledb.IMultipleResults.GetResult|API|OLEDB|RET> %08X{HRESULT}, RecordAffected=%Id\n", result, ptr);
                        if ((OleDbHResult.S_OK <= result) && (ppRowset != null))
                        {
                            Bid.Trace("<oledb.IUnknown.QueryInterface|API|OLEDB|RowSet> %d#, IRowset\n", this.ObjectID);
                            this._irowset = (System.Data.Common.UnsafeNativeMethods.IRowset) ppRowset;
                        }
                        this._recordsAffected = AddRecordsAffected(this._recordsAffected, ptr);
                        if (OleDbHResult.DB_S_NORESULT == result)
                        {
                            this.DisposeNativeMultipleResults();
                            goto Label_0116;
                        }
                        this.ProcessResults(result);
                    }
                    while (this._irowset == null);
                    this.BuildMetaInfo();
                    this.HasRowsRead();
                    flag = true;
                }
                else
                {
                    this.DisposeOpenResults();
                    this._hasRows = false;
                }
            Label_0116:
                flag2 = flag;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr2);
            }
            return flag2;
        }

        internal static OleDbException NextResults(UnsafeNativeMethods.IMultipleResults imultipleResults, OleDbConnection connection, OleDbCommand command, out IntPtr recordsAffected)
        {
            recordsAffected = ADP.RecordsUnaffected;
            List<OleDbException> exceptions = null;
            if (imultipleResults != null)
            {
                int num = 0;
                while (true)
                {
                    IntPtr ptr;
                    object obj2;
                    if ((command != null) && command.canceling)
                    {
                        break;
                    }
                    Bid.Trace("<oledb.IMultipleResults.GetResult|API|OLEDB> DBRESULTFLAG_DEFAULT, IID_NULL\n");
                    OleDbHResult result = imultipleResults.GetResult(ADP.PtrZero, ODB.DBRESULTFLAG_DEFAULT, ref ODB.IID_NULL, out ptr, out obj2);
                    Bid.Trace("<oledb.IMultipleResults.GetResult|API|OLEDB|RET> %08X{HRESULT}, RecordAffected=%Id\n", result, ptr);
                    if ((OleDbHResult.DB_S_NORESULT == result) || (OleDbHResult.E_NOINTERFACE == result))
                    {
                        break;
                    }
                    if (connection != null)
                    {
                        Exception exception = OleDbConnection.ProcessResults(result, connection, command);
                        if (exception != null)
                        {
                            OleDbException item = exception as OleDbException;
                            if (item == null)
                            {
                                throw exception;
                            }
                            if (exceptions == null)
                            {
                                exceptions = new List<OleDbException>();
                            }
                            exceptions.Add(item);
                        }
                    }
                    else if (result < OleDbHResult.S_OK)
                    {
                        SafeNativeMethods.Wrapper.ClearErrorInfo();
                        break;
                    }
                    recordsAffected = AddRecordsAffected(recordsAffected, ptr);
                    if (((int) ptr) != 0)
                    {
                        num = 0;
                    }
                    else if (0x7d0 <= num)
                    {
                        NextResultsInfinite();
                        break;
                    }
                    num++;
                }
            }
            if (exceptions != null)
            {
                return OleDbException.CombineExceptions(exceptions);
            }
            return null;
        }

        private static void NextResultsInfinite()
        {
            Bid.Trace("<oledb.OleDbDataReader.NextResultsInfinite|INFO> System.Data.OleDb.OleDbDataReader: 2000 IMultipleResult.GetResult(NULL, DBRESULTFLAG_DEFAULT, IID_NULL, NULL, NULL) iterations with 0 records affected. Stopping suspect infinite loop. To work-around try using ExecuteReader() and iterating through results with NextResult().\n");
        }

        private void ProcessResults(OleDbHResult hr)
        {
            Exception exception;
            if (this._command != null)
            {
                exception = OleDbConnection.ProcessResults(hr, this._connection, this._command);
            }
            else
            {
                exception = OleDbConnection.ProcessResults(hr, this._connection, this._connection);
            }
            if (exception != null)
            {
                throw exception;
            }
        }

        public override bool Read()
        {
            bool flag2;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<oledb.OleDbDataReader.Read|API> %d#\n", this.ObjectID);
            try
            {
                bool flag = false;
                OleDbCommand command = this._command;
                if ((command != null) && command.canceling)
                {
                    this.DisposeOpenResults();
                }
                else if (this._irowset != null)
                {
                    if (this._hasRowsReadCheck)
                    {
                        this._isRead = flag = this._hasRows;
                        this._hasRowsReadCheck = false;
                    }
                    else if (this._singleRow && this._isRead)
                    {
                        this.DisposeOpenResults();
                    }
                    else
                    {
                        flag = this.ReadRowset();
                    }
                }
                else if (this._irow != null)
                {
                    flag = this.ReadRow();
                }
                else if (this.IsClosed)
                {
                    throw ADP.DataReaderClosed("Read");
                }
                flag2 = flag;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return flag2;
        }

        private bool ReadRow()
        {
            if (this._isRead)
            {
                this._isRead = false;
                this.DisposeNativeRow();
                this._sequentialOrdinal = -1;
                return false;
            }
            this._isRead = true;
            return (0 < this._metadata.Length);
        }

        private bool ReadRowset()
        {
            this.ReleaseCurrentRow();
            this._sequentialOrdinal = -1;
            if (IntPtr.Zero == this._rowFetchedCount)
            {
                this.GetRowHandles();
            }
            return ((this._currentRow <= ((int) this._rowFetchedCount)) && this._isRead);
        }

        private void ReleaseCurrentRow()
        {
            if (0 < ((int) this._rowFetchedCount))
            {
                Bindings[] bindingsArray = this._bindings;
                for (int i = 0; (i < bindingsArray.Length) && (i < this._nextAccessorForRetrieval); i++)
                {
                    bindingsArray[i].CleanupBindings();
                }
                this._nextAccessorForRetrieval = 0;
                this._nextValueForRetrieval = 0;
                this._currentRow++;
                if (this._currentRow == ((int) this._rowFetchedCount))
                {
                    this.ReleaseRowHandles();
                }
            }
        }

        private void ReleaseRowHandles()
        {
            System.Data.Common.UnsafeNativeMethods.IRowset rowset = this.IRowset();
            Bid.Trace("<oledb.IRowset.ReleaseRows|API|OLEDB> %d#, Request=%Id\n", this.ObjectID, this._rowFetchedCount);
            OleDbHResult result = rowset.ReleaseRows(this._rowFetchedCount, this._rowHandleNativeBuffer, ADP.PtrZero, ADP.PtrZero, ADP.PtrZero);
            Bid.Trace("<oledb.IRowset.ReleaseRows|API|OLEDB|RET> %08X{HRESULT}\n", result);
            if (result < OleDbHResult.S_OK)
            {
                SafeNativeMethods.Wrapper.ClearErrorInfo();
            }
            this._rowFetchedCount = IntPtr.Zero;
            this._currentRow = 0;
            this._isRead = false;
        }

        internal OleDbDataReader ResetChapter(int bindingIndex, int index, RowBinding rowbinding, int valueOffset)
        {
            return this.GetDataForReader(this._metadata[bindingIndex + index].ordinal, rowbinding, valueOffset);
        }

        internal OleDbCommand Command
        {
            get
            {
                return this._command;
            }
        }

        public override int Depth
        {
            get
            {
                Bid.Trace("<oledb.OleDbDataReader.get_Depth|API> %d#\n", this.ObjectID);
                if (this.IsClosed)
                {
                    throw ADP.DataReaderClosed("Depth");
                }
                return this._depth;
            }
        }

        public override int FieldCount
        {
            get
            {
                Bid.Trace("<oledb.OleDbDataReader.get_FieldCount|API> %d#\n", this.ObjectID);
                if (this.IsClosed)
                {
                    throw ADP.DataReaderClosed("FieldCount");
                }
                System.Data.OleDb.MetaData[] metaData = this.MetaData;
                if (metaData == null)
                {
                    return 0;
                }
                return metaData.Length;
            }
        }

        public override bool HasRows
        {
            get
            {
                Bid.Trace("<oledb.OleDbDataReader.get_HasRows|API> %d#\n", this.ObjectID);
                if (this.IsClosed)
                {
                    throw ADP.DataReaderClosed("HasRows");
                }
                return this._hasRows;
            }
        }

        public override bool IsClosed
        {
            get
            {
                Bid.Trace("<oledb.OleDbDataReader.get_IsClosed|API> %d#\n", this.ObjectID);
                return this._isClosed;
            }
        }

        public override object this[int index]
        {
            get
            {
                return this.GetValue(index);
            }
        }

        public override object this[string name]
        {
            get
            {
                int ordinal = this.GetOrdinal(name);
                return this.GetValue(ordinal);
            }
        }

        private System.Data.OleDb.MetaData[] MetaData
        {
            get
            {
                return this._metadata;
            }
        }

        public override int RecordsAffected
        {
            get
            {
                Bid.Trace("<oledb.OleDbDataReader.get_RecordsAffected|API> %d#\n", this.ObjectID);
                return ADP.IntPtrToInt32(this._recordsAffected);
            }
        }

        public override int VisibleFieldCount
        {
            get
            {
                Bid.Trace("<oledb.OleDbDataReader.get_VisibleFieldCount|API> %d#\n", this.ObjectID);
                if (this.IsClosed)
                {
                    throw ADP.DataReaderClosed("VisibleFieldCount");
                }
                return this._visibleFieldCount;
            }
        }
    }
}

