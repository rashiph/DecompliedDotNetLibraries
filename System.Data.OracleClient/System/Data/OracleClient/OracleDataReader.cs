namespace System.Data.OracleClient
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    public sealed class OracleDataReader : DbDataReader
    {
        private NativeBuffer_RowBuffer _buffer;
        private bool _closeConnectionToo;
        private OracleColumn[] _columnInfo;
        private CommandBehavior _commandBehavior;
        private OracleConnection _connection;
        private int _connectionCloseCount;
        private bool _endOfData;
        private System.Data.ProviderBase.FieldNameLookup _fieldNameLookup;
        private byte _hasRows;
        private bool _isLastBuffer;
        private bool _keyInfoRequested;
        private int _nextRefCursor;
        private static int _objectTypeCount;
        private const int _prefetchMemory = 0x10000;
        private int _recordsAffected;
        private OracleDataReader[] _refCursorDataReaders;
        private int _rowBufferLength;
        private int _rowsToPrefetch;
        private int _rowsTotal;
        private DataTable _schemaTable;
        private OciStatementHandle _statementHandle;
        private string _statementText;
        internal readonly int ObjectID;
        private const byte x_hasRows_False = 1;
        private const byte x_hasRows_True = 2;
        private const byte x_hasRows_Unknown = 0;

        internal OracleDataReader(OracleConnection connection, OciStatementHandle statementHandle)
        {
            this.ObjectID = Interlocked.Increment(ref _objectTypeCount);
            this._commandBehavior = CommandBehavior.Default;
            this._statementHandle = statementHandle;
            this._connection = connection;
            this._connectionCloseCount = this._connection.CloseCount;
            this._recordsAffected = -1;
            this.FillColumnInfo();
        }

        internal OracleDataReader(OracleCommand command, ArrayList refCursorParameterOrdinals, string statementText, CommandBehavior commandBehavior)
        {
            this.ObjectID = Interlocked.Increment(ref _objectTypeCount);
            this._commandBehavior = commandBehavior;
            this._statementText = statementText;
            this._closeConnectionToo = this.IsCommandBehavior(CommandBehavior.CloseConnection);
            if (CommandType.Text == command.CommandType)
            {
                this._keyInfoRequested = this.IsCommandBehavior(CommandBehavior.KeyInfo);
            }
            ArrayList list = new ArrayList();
            int num2 = 0;
            OracleDataReader reader = null;
            for (int i = 0; i < refCursorParameterOrdinals.Count; i++)
            {
                int num3 = (int) refCursorParameterOrdinals[i];
                OracleParameter parameter = command.Parameters[num3];
                if (OracleType.Cursor == parameter.OracleType)
                {
                    reader = (OracleDataReader) parameter.Value;
                    reader._recordsAffected = num2;
                    list.Add(reader);
                    parameter.Value = DBNull.Value;
                }
                else
                {
                    num2 += (int) parameter.Value;
                }
            }
            this._refCursorDataReaders = new OracleDataReader[list.Count];
            list.CopyTo(this._refCursorDataReaders);
            this._nextRefCursor = 0;
            this.NextResultInternal();
        }

        internal OracleDataReader(OracleCommand command, OciStatementHandle statementHandle, string statementText, CommandBehavior commandBehavior)
        {
            this.ObjectID = Interlocked.Increment(ref _objectTypeCount);
            this._commandBehavior = commandBehavior;
            this._statementHandle = statementHandle;
            this._connection = command.Connection;
            this._connectionCloseCount = this._connection.CloseCount;
            this._columnInfo = null;
            if (OCI.STMT.OCI_STMT_SELECT == command.StatementType)
            {
                this.FillColumnInfo();
                this._recordsAffected = -1;
                if (this.IsCommandBehavior(CommandBehavior.SchemaOnly))
                {
                    this._endOfData = true;
                }
            }
            else
            {
                this._statementHandle.GetAttribute(OCI.ATTR.OCI_ATTR_ROW_COUNT, out this._recordsAffected, this.ErrorHandle);
                this._endOfData = true;
                this._hasRows = 1;
            }
            this._statementText = statementText;
            this._closeConnectionToo = this.IsCommandBehavior(CommandBehavior.CloseConnection);
            if (CommandType.Text == command.CommandType)
            {
                this._keyInfoRequested = this.IsCommandBehavior(CommandBehavior.KeyInfo);
            }
        }

        private void AssertReaderHasColumns()
        {
            if (0 >= this.FieldCount)
            {
                throw System.Data.Common.ADP.DataReaderNoData();
            }
        }

        private void AssertReaderHasData()
        {
            if (!this.IsValidRow)
            {
                throw System.Data.Common.ADP.DataReaderNoData();
            }
        }

        private void AssertReaderIsOpen()
        {
            if ((this._connection != null) && (this._connectionCloseCount != this._connection.CloseCount))
            {
                this.Close();
            }
            if (this._statementHandle == null)
            {
                throw System.Data.Common.ADP.ClosedDataReaderError();
            }
            if ((this._connection == null) || (ConnectionState.Open != this._connection.State))
            {
                throw System.Data.Common.ADP.ClosedConnectionError();
            }
        }

        private void AssertReaderIsOpen(string methodName)
        {
            if (this.IsClosed)
            {
                throw System.Data.Common.ADP.DataReaderClosed(methodName);
            }
        }

        private void Cleanup()
        {
            if (this._buffer != null)
            {
                this._buffer.Dispose();
                this._buffer = null;
            }
            if (this._columnInfo != null)
            {
                if (this._refCursorDataReaders == null)
                {
                    int length = this._columnInfo.Length;
                    while (--length >= 0)
                    {
                        if (this._columnInfo[length] != null)
                        {
                            this._columnInfo[length].Dispose();
                            this._columnInfo[length] = null;
                        }
                    }
                }
                this._columnInfo = null;
            }
        }

        public override void Close()
        {
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ora.OracleDataReader.Close|API> %d#\n", this.ObjectID);
            try
            {
                OciHandle.SafeDispose(ref this._statementHandle);
                this.Cleanup();
                if (this._refCursorDataReaders != null)
                {
                    int length = this._refCursorDataReaders.Length;
                    while (--length >= 0)
                    {
                        OracleDataReader reader = this._refCursorDataReaders[length];
                        this._refCursorDataReaders[length] = null;
                        if (reader != null)
                        {
                            reader.Dispose();
                        }
                    }
                    this._refCursorDataReaders = null;
                }
                if (this._closeConnectionToo && (this._connection != null))
                {
                    this._connection.Close();
                }
                this._connection = null;
                this._fieldNameLookup = null;
                this._schemaTable = null;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        private DataTable CreateSchemaTable(int columnCount)
        {
            DataTable table = new DataTable("SchemaTable") {
                Locale = CultureInfo.InvariantCulture,
                MinimumCapacity = columnCount
            };
            DataColumn column16 = new DataColumn(SchemaTableColumn.ColumnName, typeof(string));
            DataColumn column2 = new DataColumn(SchemaTableColumn.ColumnOrdinal, typeof(int));
            DataColumn column15 = new DataColumn(SchemaTableColumn.ColumnSize, typeof(int));
            DataColumn column14 = new DataColumn(SchemaTableColumn.NumericPrecision, typeof(short));
            DataColumn column13 = new DataColumn(SchemaTableColumn.NumericScale, typeof(short));
            DataColumn column12 = new DataColumn(SchemaTableColumn.DataType, typeof(Type));
            DataColumn column11 = new DataColumn(SchemaTableColumn.ProviderType, typeof(int));
            DataColumn column = new DataColumn(SchemaTableColumn.IsLong, typeof(bool));
            DataColumn column10 = new DataColumn(SchemaTableColumn.AllowDBNull, typeof(bool));
            DataColumn column9 = new DataColumn(SchemaTableColumn.IsAliased, typeof(bool));
            DataColumn column8 = new DataColumn(SchemaTableColumn.IsExpression, typeof(bool));
            DataColumn column7 = new DataColumn(SchemaTableColumn.IsKey, typeof(bool));
            DataColumn column6 = new DataColumn(SchemaTableColumn.IsUnique, typeof(bool));
            DataColumn column5 = new DataColumn(SchemaTableColumn.BaseSchemaName, typeof(string));
            DataColumn column4 = new DataColumn(SchemaTableColumn.BaseTableName, typeof(string));
            DataColumn column3 = new DataColumn(SchemaTableColumn.BaseColumnName, typeof(string));
            column2.DefaultValue = 0;
            column.DefaultValue = false;
            DataColumnCollection columns = table.Columns;
            columns.Add(column16);
            columns.Add(column2);
            columns.Add(column15);
            columns.Add(column14);
            columns.Add(column13);
            columns.Add(column12);
            columns.Add(column11);
            columns.Add(column);
            columns.Add(column10);
            columns.Add(column9);
            columns.Add(column8);
            columns.Add(column7);
            columns.Add(column6);
            columns.Add(column5);
            columns.Add(column4);
            columns.Add(column3);
            for (int i = 0; i < columns.Count; i++)
            {
                columns[i].ReadOnly = true;
            }
            return table;
        }

        internal void FillColumnInfo()
        {
            int num2;
            bool flag = false;
            this._statementHandle.GetAttribute(OCI.ATTR.OCI_ATTR_PARAM_COUNT, out num2, this.ErrorHandle);
            this._columnInfo = new OracleColumn[num2];
            this._rowBufferLength = 0;
            for (int i = 0; i < num2; i++)
            {
                this._columnInfo[i] = new OracleColumn(this._statementHandle, i, this.ErrorHandle, this._connection);
                if (this._columnInfo[i].Describe(ref this._rowBufferLength, this._connection, this.ErrorHandle))
                {
                    flag = true;
                }
            }
            if (flag || (this._rowBufferLength == 0))
            {
                this._rowsToPrefetch = 1;
            }
            else
            {
                this._rowsToPrefetch = ((0x10000 + this._rowBufferLength) - 1) / this._rowBufferLength;
            }
        }

        private void FillSchemaTable(DataTable schemaTable)
        {
            DataColumn column3 = new DataColumn(SchemaTableOptionalColumn.ProviderSpecificDataType, typeof(Type));
            schemaTable.Columns.Add(column3);
            int fieldCount = this.FieldCount;
            DbSqlParserColumnCollection columns = null;
            int count = 0;
            if (this._keyInfoRequested)
            {
                OracleSqlParser parser = new OracleSqlParser();
                parser.Parse(this._statementText, this._connection);
                columns = parser.Columns;
                count = columns.Count;
            }
            for (int i = 0; i < fieldCount; i++)
            {
                OracleColumn column = this._columnInfo[i];
                DataRow row = schemaTable.NewRow();
                row[SchemaTableColumn.ColumnName] = column.ColumnName;
                row[SchemaTableColumn.ColumnOrdinal] = column.Ordinal;
                if (column.IsLong | column.IsLob)
                {
                    row[SchemaTableColumn.ColumnSize] = 0x7fffffff;
                }
                else
                {
                    row[SchemaTableColumn.ColumnSize] = column.SchemaTableSize;
                }
                row[SchemaTableColumn.NumericPrecision] = column.Precision;
                row[SchemaTableColumn.NumericScale] = column.Scale;
                row[SchemaTableColumn.DataType] = column.GetFieldType();
                row[column3] = column.GetFieldOracleType();
                row[SchemaTableColumn.ProviderType] = column.OracleType;
                row[SchemaTableColumn.IsLong] = column.IsLong | column.IsLob;
                row[SchemaTableColumn.AllowDBNull] = column.IsNullable;
                if (this._keyInfoRequested && (count == fieldCount))
                {
                    DbSqlParserColumn column2 = columns[i];
                    row[SchemaTableColumn.IsAliased] = column2.IsAliased;
                    row[SchemaTableColumn.IsExpression] = column2.IsExpression;
                    row[SchemaTableColumn.IsKey] = column2.IsKey;
                    row[SchemaTableColumn.IsUnique] = column2.IsUnique;
                    row[SchemaTableColumn.BaseSchemaName] = this.SetSchemaValue(OracleSqlParser.CatalogCase(column2.SchemaName));
                    row[SchemaTableColumn.BaseTableName] = this.SetSchemaValue(OracleSqlParser.CatalogCase(column2.TableName));
                    row[SchemaTableColumn.BaseColumnName] = this.SetSchemaValue(OracleSqlParser.CatalogCase(column2.ColumnName));
                }
                else
                {
                    row[SchemaTableColumn.IsAliased] = DBNull.Value;
                    row[SchemaTableColumn.IsExpression] = DBNull.Value;
                    row[SchemaTableColumn.IsKey] = DBNull.Value;
                    row[SchemaTableColumn.IsUnique] = DBNull.Value;
                    row[SchemaTableColumn.BaseSchemaName] = DBNull.Value;
                    row[SchemaTableColumn.BaseTableName] = DBNull.Value;
                    row[SchemaTableColumn.BaseColumnName] = DBNull.Value;
                }
                schemaTable.Rows.Add(row);
                row.AcceptChanges();
            }
        }

        public override bool GetBoolean(int i)
        {
            throw System.Data.Common.ADP.NotSupported();
        }

        public override byte GetByte(int i)
        {
            throw System.Data.Common.ADP.NotSupported();
        }

        public override long GetBytes(int i, long fieldOffset, byte[] buffer2, int bufferoffset, int length)
        {
            this.AssertReaderIsOpen();
            this.AssertReaderHasData();
            return this._columnInfo[i].GetBytes(this._buffer, fieldOffset, buffer2, bufferoffset, length);
        }

        public override char GetChar(int i)
        {
            throw System.Data.Common.ADP.NotSupported();
        }

        public override long GetChars(int i, long fieldOffset, char[] buffer2, int bufferoffset, int length)
        {
            this.AssertReaderIsOpen();
            this.AssertReaderHasData();
            return this._columnInfo[i].GetChars(this._buffer, fieldOffset, buffer2, bufferoffset, length);
        }

        public override string GetDataTypeName(int i)
        {
            this.AssertReaderIsOpen();
            if (this._columnInfo == null)
            {
                throw System.Data.Common.ADP.NoData();
            }
            return this._columnInfo[i].GetDataTypeName();
        }

        public override DateTime GetDateTime(int i)
        {
            this.AssertReaderIsOpen();
            this.AssertReaderHasData();
            return this._columnInfo[i].GetDateTime(this._buffer);
        }

        public override decimal GetDecimal(int i)
        {
            this.AssertReaderIsOpen();
            this.AssertReaderHasData();
            return this._columnInfo[i].GetDecimal(this._buffer);
        }

        public override double GetDouble(int i)
        {
            this.AssertReaderIsOpen();
            this.AssertReaderHasData();
            return this._columnInfo[i].GetDouble(this._buffer);
        }

        public override IEnumerator GetEnumerator()
        {
            return new DbEnumerator(this, this.IsCommandBehavior(CommandBehavior.CloseConnection));
        }

        public override Type GetFieldType(int i)
        {
            if (this._columnInfo == null)
            {
                this.AssertReaderIsOpen();
                throw System.Data.Common.ADP.NoData();
            }
            return this._columnInfo[i].GetFieldType();
        }

        public override float GetFloat(int i)
        {
            this.AssertReaderIsOpen();
            this.AssertReaderHasData();
            return this._columnInfo[i].GetFloat(this._buffer);
        }

        public override Guid GetGuid(int i)
        {
            throw System.Data.Common.ADP.NotSupported();
        }

        public override short GetInt16(int i)
        {
            throw System.Data.Common.ADP.NotSupported();
        }

        public override int GetInt32(int i)
        {
            this.AssertReaderIsOpen();
            this.AssertReaderHasData();
            return this._columnInfo[i].GetInt32(this._buffer);
        }

        public override long GetInt64(int i)
        {
            this.AssertReaderIsOpen();
            this.AssertReaderHasData();
            return this._columnInfo[i].GetInt64(this._buffer);
        }

        public override string GetName(int i)
        {
            if (this._columnInfo == null)
            {
                this.AssertReaderIsOpen();
                throw System.Data.Common.ADP.NoData();
            }
            return this._columnInfo[i].ColumnName;
        }

        public OracleBFile GetOracleBFile(int i)
        {
            this.AssertReaderIsOpen();
            this.AssertReaderHasData();
            return this._columnInfo[i].GetOracleBFile(this._buffer);
        }

        public OracleBinary GetOracleBinary(int i)
        {
            this.AssertReaderIsOpen();
            this.AssertReaderHasData();
            return this._columnInfo[i].GetOracleBinary(this._buffer);
        }

        public OracleDateTime GetOracleDateTime(int i)
        {
            this.AssertReaderIsOpen();
            this.AssertReaderHasData();
            return this._columnInfo[i].GetOracleDateTime(this._buffer);
        }

        public OracleLob GetOracleLob(int i)
        {
            this.AssertReaderIsOpen();
            this.AssertReaderHasData();
            return this._columnInfo[i].GetOracleLob(this._buffer);
        }

        public OracleMonthSpan GetOracleMonthSpan(int i)
        {
            this.AssertReaderIsOpen();
            this.AssertReaderHasData();
            return this._columnInfo[i].GetOracleMonthSpan(this._buffer);
        }

        public OracleNumber GetOracleNumber(int i)
        {
            this.AssertReaderIsOpen();
            this.AssertReaderHasData();
            return this._columnInfo[i].GetOracleNumber(this._buffer);
        }

        public OracleString GetOracleString(int i)
        {
            this.AssertReaderIsOpen();
            this.AssertReaderHasData();
            return this._columnInfo[i].GetOracleString(this._buffer);
        }

        public OracleTimeSpan GetOracleTimeSpan(int i)
        {
            this.AssertReaderIsOpen();
            this.AssertReaderHasData();
            return this._columnInfo[i].GetOracleTimeSpan(this._buffer);
        }

        public object GetOracleValue(int i)
        {
            this.AssertReaderIsOpen();
            this.AssertReaderHasData();
            return this._columnInfo[i].GetOracleValue(this._buffer);
        }

        public int GetOracleValues(object[] values)
        {
            if (values == null)
            {
                throw System.Data.Common.ADP.ArgumentNull("values");
            }
            this.AssertReaderIsOpen();
            this.AssertReaderHasData();
            int num2 = Math.Min(values.Length, this.FieldCount);
            for (int i = 0; i < num2; i++)
            {
                values[i] = this.GetOracleValue(i);
            }
            return num2;
        }

        public override int GetOrdinal(string name)
        {
            this.AssertReaderIsOpen("GetOrdinal");
            this.AssertReaderHasColumns();
            if (this._fieldNameLookup == null)
            {
                this._fieldNameLookup = new System.Data.ProviderBase.FieldNameLookup(this, -1);
            }
            return this._fieldNameLookup.GetOrdinal(name);
        }

        public override Type GetProviderSpecificFieldType(int i)
        {
            if (this._columnInfo == null)
            {
                this.AssertReaderIsOpen();
                throw System.Data.Common.ADP.NoData();
            }
            return this._columnInfo[i].GetFieldOracleType();
        }

        public override object GetProviderSpecificValue(int i)
        {
            return this.GetOracleValue(i);
        }

        public override int GetProviderSpecificValues(object[] values)
        {
            return this.GetOracleValues(values);
        }

        public override DataTable GetSchemaTable()
        {
            DataTable schemaTable = this._schemaTable;
            if (schemaTable == null)
            {
                this.AssertReaderIsOpen("GetSchemaTable");
                if (0 < this.FieldCount)
                {
                    schemaTable = this.CreateSchemaTable(this.FieldCount);
                    this.FillSchemaTable(schemaTable);
                    this._schemaTable = schemaTable;
                    return schemaTable;
                }
                if (0 > this.FieldCount)
                {
                    throw System.Data.Common.ADP.DataReaderNoData();
                }
            }
            return schemaTable;
        }

        public override string GetString(int i)
        {
            this.AssertReaderIsOpen();
            this.AssertReaderHasData();
            return this._columnInfo[i].GetString(this._buffer);
        }

        public TimeSpan GetTimeSpan(int i)
        {
            this.AssertReaderIsOpen();
            this.AssertReaderHasData();
            return this._columnInfo[i].GetTimeSpan(this._buffer);
        }

        public override object GetValue(int i)
        {
            this.AssertReaderIsOpen();
            this.AssertReaderHasData();
            return this._columnInfo[i].GetValue(this._buffer);
        }

        public override int GetValues(object[] values)
        {
            if (values == null)
            {
                throw System.Data.Common.ADP.ArgumentNull("values");
            }
            this.AssertReaderIsOpen();
            this.AssertReaderHasData();
            int num2 = Math.Min(values.Length, this.FieldCount);
            for (int i = 0; i < num2; i++)
            {
                values[i] = this._columnInfo[i].GetValue(this._buffer);
            }
            return num2;
        }

        private bool IsCommandBehavior(CommandBehavior condition)
        {
            return (condition == (condition & this._commandBehavior));
        }

        public override bool IsDBNull(int i)
        {
            this.AssertReaderIsOpen();
            this.AssertReaderHasData();
            return this._columnInfo[i].IsDBNull(this._buffer);
        }

        public override bool NextResult()
        {
            bool flag;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ora.OracleDataReader.NextResult|API> %d#\n", this.ObjectID);
            try
            {
                this.AssertReaderIsOpen("NextResult");
                this._fieldNameLookup = null;
                this._schemaTable = null;
                flag = this.NextResultInternal();
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return flag;
        }

        private bool NextResultInternal()
        {
            this.Cleanup();
            if ((this._refCursorDataReaders == null) || (this._nextRefCursor >= this._refCursorDataReaders.Length))
            {
                this._endOfData = true;
                this._hasRows = 1;
                return false;
            }
            if (this._nextRefCursor > 0)
            {
                this._refCursorDataReaders[this._nextRefCursor - 1].Dispose();
                this._refCursorDataReaders[this._nextRefCursor - 1] = null;
            }
            OciStatementHandle handle = this._statementHandle;
            this._statementHandle = this._refCursorDataReaders[this._nextRefCursor]._statementHandle;
            OciHandle.SafeDispose(ref handle);
            this._connection = this._refCursorDataReaders[this._nextRefCursor]._connection;
            this._connectionCloseCount = this._refCursorDataReaders[this._nextRefCursor]._connectionCloseCount;
            this._hasRows = this._refCursorDataReaders[this._nextRefCursor]._hasRows;
            this._recordsAffected = this._refCursorDataReaders[this._nextRefCursor]._recordsAffected;
            this._columnInfo = this._refCursorDataReaders[this._nextRefCursor]._columnInfo;
            this._rowBufferLength = this._refCursorDataReaders[this._nextRefCursor]._rowBufferLength;
            this._rowsToPrefetch = this._refCursorDataReaders[this._nextRefCursor]._rowsToPrefetch;
            this._nextRefCursor++;
            this._endOfData = false;
            this._isLastBuffer = false;
            this._rowsTotal = 0;
            return true;
        }

        public override bool Read()
        {
            bool flag2;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<ora.OracleDataReader.Read|API> %d#\n", this.ObjectID);
            try
            {
                this.AssertReaderIsOpen("Read");
                bool flag = this.ReadInternal();
                if (flag)
                {
                    this._hasRows = 2;
                }
                flag2 = flag;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
            return flag2;
        }

        private bool ReadInternal()
        {
            int num;
            bool flag;
            if (this._endOfData)
            {
                return false;
            }
            int length = this._columnInfo.Length;
            NativeBuffer_RowBuffer buffer = this._buffer;
            bool success = false;
            bool[] flagArray = new bool[length];
            SafeHandle[] handleArray = new SafeHandle[length];
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                if (buffer == null)
                {
                    int rowBufferLength = (this._rowsToPrefetch > 1) ? this._rowBufferLength : 0;
                    buffer = new NativeBuffer_RowBuffer(this._rowBufferLength, this._rowsToPrefetch);
                    buffer.DangerousAddRef(ref success);
                    for (num = 0; num < length; num++)
                    {
                        this._columnInfo[num].Bind(this._statementHandle, buffer, this.ErrorHandle, rowBufferLength);
                    }
                    this._buffer = buffer;
                }
                else
                {
                    buffer.DangerousAddRef(ref success);
                }
                if (buffer.MoveNext())
                {
                    return true;
                }
                if (this._isLastBuffer)
                {
                    this._endOfData = true;
                    return false;
                }
                buffer.MoveFirst();
                if (1 == this._rowsToPrefetch)
                {
                    for (num = 0; num < length; num++)
                    {
                        this._columnInfo[num].Rebind(this._connection, ref flagArray[num], ref handleArray[num]);
                    }
                }
                int rc = TracedNativeMethods.OCIStmtFetch(this._statementHandle, this.ErrorHandle, this._rowsToPrefetch, OCI.FETCH.OCI_FETCH_NEXT, OCI.MODE.OCI_DEFAULT);
                int num5 = this._rowsTotal;
                this._statementHandle.GetAttribute(OCI.ATTR.OCI_ATTR_ROW_COUNT, out this._rowsTotal, this.ErrorHandle);
                if (rc == 0)
                {
                    return true;
                }
                if (1 == rc)
                {
                    this._connection.CheckError(this.ErrorHandle, rc);
                    return true;
                }
                if (100 == rc)
                {
                    int num4 = this._rowsTotal - num5;
                    if (num4 == 0)
                    {
                        if (this._rowsTotal == 0)
                        {
                            this._hasRows = 1;
                        }
                        this._endOfData = true;
                        return false;
                    }
                    buffer.NumberOfRows = num4;
                    this._isLastBuffer = true;
                    return true;
                }
                this._endOfData = true;
                this._connection.CheckError(this.ErrorHandle, rc);
                flag = false;
            }
            finally
            {
                if (1 == this._rowsToPrefetch)
                {
                    for (num = 0; num < length; num++)
                    {
                        if (flagArray[num])
                        {
                            handleArray[num].DangerousRelease();
                        }
                    }
                }
                if (success)
                {
                    buffer.DangerousRelease();
                }
            }
            return flag;
        }

        private object SetSchemaValue(string value)
        {
            if (System.Data.Common.ADP.IsEmpty(value))
            {
                return DBNull.Value;
            }
            return value;
        }

        public override int Depth
        {
            get
            {
                this.AssertReaderIsOpen("Depth");
                return 0;
            }
        }

        private OciErrorHandle ErrorHandle
        {
            get
            {
                return this._connection.ErrorHandle;
            }
        }

        public override int FieldCount
        {
            get
            {
                this.AssertReaderIsOpen();
                if (this._columnInfo == null)
                {
                    return 0;
                }
                return this._columnInfo.Length;
            }
        }

        public override bool HasRows
        {
            get
            {
                this.AssertReaderIsOpen();
                bool flag = 2 == this._hasRows;
                if (this._hasRows == 0)
                {
                    flag = this.ReadInternal();
                    if (this._buffer != null)
                    {
                        this._buffer.MovePrevious();
                    }
                    this._hasRows = flag ? ((byte) 2) : ((byte) 1);
                }
                return flag;
            }
        }

        public override bool IsClosed
        {
            get
            {
                if ((this._statementHandle != null) && (this._connection != null))
                {
                    return (this._connectionCloseCount != this._connection.CloseCount);
                }
                return true;
            }
        }

        private bool IsValidRow
        {
            get
            {
                return ((!this._endOfData && (this._buffer != null)) && this._buffer.CurrentPositionIsValid);
            }
        }

        public override object this[int i]
        {
            get
            {
                return this.GetValue(i);
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

        public override int RecordsAffected
        {
            get
            {
                return this._recordsAffected;
            }
        }
    }
}

