namespace System.Data.SqlClient
{
    using Microsoft.SqlServer.Server;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Data.SqlTypes;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class SqlDataReader : DbDataReader, IDataReader, IDisposable, IDataRecord
    {
        private _SqlMetaDataSetCollection _altMetaDataSetCollection;
        private ALTROWSTATUS _altRowStatus;
        private bool _browseModeInfoConsumed;
        private long _columnDataBytesRead;
        private long _columnDataBytesRemaining;
        private char[] _columnDataChars;
        private long _columnDataCharsRead;
        private SqlCommand _command;
        private CommandBehavior _commandBehavior;
        private SqlConnection _connection;
        private SqlBuffer[] _data;
        private bool _dataReady;
        private int _defaultLCID;
        private FieldNameLookup _fieldNameLookup;
        private bool _haltRead;
        private bool _hasRows;
        private bool _isClosed;
        private bool _isInitialized;
        private _SqlMetaDataSet _metaData;
        private bool _metaDataConsumed;
        private int _nextColumnDataToRead;
        private int _nextColumnHeaderToRead;
        private static int _objectTypeCount;
        private TdsParser _parser;
        private int _recordsAffected = -1;
        private string _resetOptionsString;
        private Exception _rowException;
        private TdsParserStateObject _stateObj;
        private SqlStatistics _statistics;
        private SqlStreamingXml _streamingXml;
        private MultiPartTableName[] _tableNames;
        private int _timeoutSeconds;
        private SqlConnectionString.TypeSystem _typeSystem;
        internal readonly int ObjectID = Interlocked.Increment(ref _objectTypeCount);

        internal SqlDataReader(SqlCommand command, CommandBehavior behavior)
        {
            this._command = command;
            this._commandBehavior = behavior;
            if (this._command != null)
            {
                this._timeoutSeconds = command.CommandTimeout;
                this._connection = command.Connection;
                if (this._connection != null)
                {
                    this._statistics = this._connection.Statistics;
                    this._typeSystem = this._connection.TypeSystem;
                }
            }
            this._dataReady = false;
            this._metaDataConsumed = false;
            this._hasRows = false;
            this._browseModeInfoConsumed = false;
        }

        internal void Bind(TdsParserStateObject stateObj)
        {
            stateObj.Owner = this;
            this._stateObj = stateObj;
            this._parser = stateObj.Parser;
            this._defaultLCID = this._parser.DefaultLCID;
        }

        internal DataTable BuildSchemaTable()
        {
            _SqlMetaDataSet metaData = this.MetaData;
            DataTable table = new DataTable("SchemaTable") {
                Locale = CultureInfo.InvariantCulture,
                MinimumCapacity = metaData.Length
            };
            DataColumn column31 = new DataColumn(SchemaTableColumn.ColumnName, typeof(string));
            DataColumn column9 = new DataColumn(SchemaTableColumn.ColumnOrdinal, typeof(int));
            DataColumn column2 = new DataColumn(SchemaTableColumn.ColumnSize, typeof(int));
            DataColumn column8 = new DataColumn(SchemaTableColumn.NumericPrecision, typeof(short));
            DataColumn column3 = new DataColumn(SchemaTableColumn.NumericScale, typeof(short));
            DataColumn column30 = new DataColumn(SchemaTableColumn.DataType, typeof(Type));
            DataColumn column29 = new DataColumn(SchemaTableOptionalColumn.ProviderSpecificDataType, typeof(Type));
            DataColumn column28 = new DataColumn(SchemaTableColumn.NonVersionedProviderType, typeof(int));
            DataColumn column = new DataColumn(SchemaTableColumn.ProviderType, typeof(int));
            DataColumn column7 = new DataColumn(SchemaTableColumn.IsLong, typeof(bool));
            DataColumn column27 = new DataColumn(SchemaTableColumn.AllowDBNull, typeof(bool));
            DataColumn column26 = new DataColumn(SchemaTableOptionalColumn.IsReadOnly, typeof(bool));
            DataColumn column6 = new DataColumn(SchemaTableOptionalColumn.IsRowVersion, typeof(bool));
            DataColumn column5 = new DataColumn(SchemaTableColumn.IsUnique, typeof(bool));
            DataColumn column25 = new DataColumn(SchemaTableColumn.IsKey, typeof(bool));
            DataColumn column24 = new DataColumn(SchemaTableOptionalColumn.IsAutoIncrement, typeof(bool));
            DataColumn column23 = new DataColumn(SchemaTableOptionalColumn.IsHidden, typeof(bool));
            DataColumn column22 = new DataColumn(SchemaTableOptionalColumn.BaseCatalogName, typeof(string));
            DataColumn column21 = new DataColumn(SchemaTableColumn.BaseSchemaName, typeof(string));
            DataColumn column20 = new DataColumn(SchemaTableColumn.BaseTableName, typeof(string));
            DataColumn column4 = new DataColumn(SchemaTableColumn.BaseColumnName, typeof(string));
            DataColumn column19 = new DataColumn(SchemaTableOptionalColumn.BaseServerName, typeof(string));
            DataColumn column18 = new DataColumn(SchemaTableColumn.IsAliased, typeof(bool));
            DataColumn column17 = new DataColumn(SchemaTableColumn.IsExpression, typeof(bool));
            DataColumn column16 = new DataColumn("IsIdentity", typeof(bool));
            DataColumn column15 = new DataColumn("DataTypeName", typeof(string));
            DataColumn column14 = new DataColumn("UdtAssemblyQualifiedName", typeof(string));
            DataColumn column13 = new DataColumn("XmlSchemaCollectionDatabase", typeof(string));
            DataColumn column12 = new DataColumn("XmlSchemaCollectionOwningSchema", typeof(string));
            DataColumn column11 = new DataColumn("XmlSchemaCollectionName", typeof(string));
            DataColumn column10 = new DataColumn("IsColumnSet", typeof(bool));
            column9.DefaultValue = 0;
            column7.DefaultValue = false;
            DataColumnCollection columns = table.Columns;
            columns.Add(column31);
            columns.Add(column9);
            columns.Add(column2);
            columns.Add(column8);
            columns.Add(column3);
            columns.Add(column5);
            columns.Add(column25);
            columns.Add(column19);
            columns.Add(column22);
            columns.Add(column4);
            columns.Add(column21);
            columns.Add(column20);
            columns.Add(column30);
            columns.Add(column27);
            columns.Add(column);
            columns.Add(column18);
            columns.Add(column17);
            columns.Add(column16);
            columns.Add(column24);
            columns.Add(column6);
            columns.Add(column23);
            columns.Add(column7);
            columns.Add(column26);
            columns.Add(column29);
            columns.Add(column15);
            columns.Add(column13);
            columns.Add(column12);
            columns.Add(column11);
            columns.Add(column14);
            columns.Add(column28);
            columns.Add(column10);
            for (int i = 0; i < metaData.Length; i++)
            {
                _SqlMetaData data = metaData[i];
                DataRow row = table.NewRow();
                row[column31] = data.column;
                row[column9] = data.ordinal;
                row[column2] = (data.metaType.IsSizeInCharacters && (data.length != 0x7fffffff)) ? (data.length / 2) : data.length;
                row[column30] = this.GetFieldTypeInternal(data);
                row[column29] = this.GetProviderSpecificFieldTypeInternal(data);
                row[column28] = (int) data.type;
                row[column15] = this.GetDataTypeNameInternal(data);
                if ((this._typeSystem <= SqlConnectionString.TypeSystem.SQLServer2005) && data.IsNewKatmaiDateTimeType)
                {
                    row[column] = SqlDbType.NVarChar;
                    switch (data.type)
                    {
                        case SqlDbType.Date:
                            row[column2] = 10;
                            break;

                        case SqlDbType.Time:
                            row[column2] = TdsEnums.WHIDBEY_TIME_LENGTH[(0xff != data.scale) ? data.scale : data.metaType.Scale];
                            break;

                        case SqlDbType.DateTime2:
                            row[column2] = TdsEnums.WHIDBEY_DATETIME2_LENGTH[(0xff != data.scale) ? data.scale : data.metaType.Scale];
                            break;

                        case SqlDbType.DateTimeOffset:
                            row[column2] = TdsEnums.WHIDBEY_DATETIMEOFFSET_LENGTH[(0xff != data.scale) ? data.scale : data.metaType.Scale];
                            break;
                    }
                }
                else if ((this._typeSystem <= SqlConnectionString.TypeSystem.SQLServer2005) && data.IsLargeUdt)
                {
                    if (this._typeSystem == SqlConnectionString.TypeSystem.SQLServer2005)
                    {
                        row[column] = SqlDbType.VarBinary;
                    }
                    else
                    {
                        row[column] = SqlDbType.Image;
                    }
                }
                else if (this._typeSystem != SqlConnectionString.TypeSystem.SQLServer2000)
                {
                    row[column] = (int) data.type;
                    if (data.type == SqlDbType.Udt)
                    {
                        row[column14] = data.udtAssemblyQualifiedName;
                    }
                    else if (data.type == SqlDbType.Xml)
                    {
                        row[column13] = data.xmlSchemaCollectionDatabase;
                        row[column12] = data.xmlSchemaCollectionOwningSchema;
                        row[column11] = data.xmlSchemaCollectionName;
                    }
                }
                else
                {
                    row[column] = this.GetVersionedMetaType(data.metaType).SqlDbType;
                }
                if (0xff != data.precision)
                {
                    row[column8] = data.precision;
                }
                else
                {
                    row[column8] = data.metaType.Precision;
                }
                if ((this._typeSystem <= SqlConnectionString.TypeSystem.SQLServer2005) && data.IsNewKatmaiDateTimeType)
                {
                    row[column3] = MetaType.MetaNVarChar.Scale;
                }
                else if (0xff != data.scale)
                {
                    row[column3] = data.scale;
                }
                else
                {
                    row[column3] = data.metaType.Scale;
                }
                row[column27] = data.isNullable;
                if (this._browseModeInfoConsumed)
                {
                    row[column18] = data.isDifferentName;
                    row[column25] = data.isKey;
                    row[column23] = data.isHidden;
                    row[column17] = data.isExpression;
                }
                row[column16] = data.isIdentity;
                row[column24] = data.isIdentity;
                row[column7] = data.metaType.IsLong;
                if (SqlDbType.Timestamp == data.type)
                {
                    row[column5] = true;
                    row[column6] = true;
                }
                else
                {
                    row[column5] = false;
                    row[column6] = false;
                }
                row[column26] = 0 == data.updatability;
                row[column10] = data.isColumnSet;
                if (!ADP.IsEmpty(data.serverName))
                {
                    row[column19] = data.serverName;
                }
                if (!ADP.IsEmpty(data.catalogName))
                {
                    row[column22] = data.catalogName;
                }
                if (!ADP.IsEmpty(data.schemaName))
                {
                    row[column21] = data.schemaName;
                }
                if (!ADP.IsEmpty(data.tableName))
                {
                    row[column20] = data.tableName;
                }
                if (!ADP.IsEmpty(data.baseColumn))
                {
                    row[column4] = data.baseColumn;
                }
                else if (!ADP.IsEmpty(data.column))
                {
                    row[column4] = data.column;
                }
                table.Rows.Add(row);
                row.AcceptChanges();
            }
            foreach (DataColumn column32 in columns)
            {
                column32.ReadOnly = true;
            }
            return table;
        }

        internal void Cancel(int objectID)
        {
            TdsParserStateObject obj2 = this._stateObj;
            if (obj2 != null)
            {
                obj2.Cancel(objectID);
            }
        }

        private void CleanPartialRead()
        {
            if (this._nextColumnHeaderToRead == 0)
            {
                this._stateObj.Parser.SkipRow(this._metaData, this._stateObj);
            }
            else
            {
                this.ResetBlobState();
                this._stateObj.Parser.SkipRow(this._metaData, this._nextColumnHeaderToRead, this._stateObj);
            }
        }

        private void ClearMetaData()
        {
            this._metaData = null;
            this._tableNames = null;
            this._fieldNameLookup = null;
            this._metaDataConsumed = false;
            this._browseModeInfoConsumed = false;
        }

        public override void Close()
        {
            SqlStatistics statistics = null;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<sc.SqlDataReader.Close|API> %d#", this.ObjectID);
            try
            {
                statistics = SqlStatistics.StartTimer(this.Statistics);
                if (!this.IsClosed)
                {
                    this.SetTimeout();
                    this.CloseInternal(true);
                }
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
                Bid.ScopeLeave(ref ptr);
            }
        }

        private void CloseInternal(bool closeReader)
        {
            TdsParser parser = this._parser;
            TdsParserStateObject stateObj = this._stateObj;
            bool flag2 = this.IsCommandBehavior(CommandBehavior.CloseConnection);
            this._parser = null;
            bool flag = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                if (((parser != null) && (stateObj != null)) && (stateObj._pendingData && (parser.State == TdsParserState.OpenLoggedIn)))
                {
                    if (this._altRowStatus == ALTROWSTATUS.AltRow)
                    {
                        this._dataReady = true;
                    }
                    if (this._dataReady)
                    {
                        this.CleanPartialRead();
                    }
                    parser.Run(RunBehavior.Clean, this._command, this, null, stateObj);
                }
                this.RestoreServerSettings(parser, stateObj);
            }
            catch (OutOfMemoryException exception6)
            {
                this._isClosed = true;
                flag = true;
                if (this._connection != null)
                {
                    this._connection.Abort(exception6);
                }
                throw;
            }
            catch (StackOverflowException exception5)
            {
                this._isClosed = true;
                flag = true;
                if (this._connection != null)
                {
                    this._connection.Abort(exception5);
                }
                throw;
            }
            catch (ThreadAbortException exception4)
            {
                this._isClosed = true;
                flag = true;
                if (this._connection != null)
                {
                    this._connection.Abort(exception4);
                }
                throw;
            }
            finally
            {
                if (flag)
                {
                    this._isClosed = true;
                    this._command = null;
                    this._connection = null;
                    this._statistics = null;
                }
                else if (closeReader)
                {
                    this._stateObj = null;
                    this._data = null;
                    if (this.Connection != null)
                    {
                        this.Connection.RemoveWeakReference(this);
                    }
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try
                    {
                        if ((this._command != null) && (stateObj != null))
                        {
                            stateObj.CloseSession();
                        }
                    }
                    catch (OutOfMemoryException exception3)
                    {
                        this._isClosed = true;
                        flag = true;
                        if (this._connection != null)
                        {
                            this._connection.Abort(exception3);
                        }
                        throw;
                    }
                    catch (StackOverflowException exception2)
                    {
                        this._isClosed = true;
                        flag = true;
                        if (this._connection != null)
                        {
                            this._connection.Abort(exception2);
                        }
                        throw;
                    }
                    catch (ThreadAbortException exception)
                    {
                        this._isClosed = true;
                        flag = true;
                        if (this._connection != null)
                        {
                            this._connection.Abort(exception);
                        }
                        throw;
                    }
                    this.SetMetaData(null, false);
                    this._dataReady = false;
                    this._isClosed = true;
                    this._fieldNameLookup = null;
                    if (flag2 && (this.Connection != null))
                    {
                        this.Connection.Close();
                    }
                    if (this._command != null)
                    {
                        this._recordsAffected = this._command.InternalRecordsAffected;
                    }
                    this._command = null;
                    this._connection = null;
                    this._statistics = null;
                }
            }
        }

        internal void CloseReaderFromConnection()
        {
            this.Close();
        }

        private void ConsumeMetaData()
        {
            while (((this._parser != null) && (this._stateObj != null)) && (this._stateObj._pendingData && !this._metaDataConsumed))
            {
                this._parser.Run(RunBehavior.ReturnImmediately, this._command, this, null, this._stateObj);
            }
            if (this._metaData != null)
            {
                this._metaData.visibleColumns = 0;
                int[] numArray = new int[this._metaData.Length];
                for (int i = 0; i < numArray.Length; i++)
                {
                    numArray[i] = this._metaData.visibleColumns;
                    if (!this._metaData[i].isHidden)
                    {
                        this._metaData.visibleColumns++;
                    }
                }
                this._metaData.indexMap = numArray;
            }
        }

        public override bool GetBoolean(int i)
        {
            this.ReadColumn(i);
            return this._data[i].Boolean;
        }

        public override byte GetByte(int i)
        {
            this.ReadColumn(i);
            return this._data[i].Byte;
        }

        public override long GetBytes(int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
        {
            SqlStatistics statistics = null;
            long num = 0L;
            if ((this.MetaData == null) || !this._dataReady)
            {
                throw SQL.InvalidRead();
            }
            MetaType metaType = this._metaData[i].metaType;
            if ((!metaType.IsLong && !metaType.IsBinType) || (SqlDbType.Xml == metaType.SqlDbType))
            {
                throw SQL.NonBlobColumn(this._metaData[i].column);
            }
            try
            {
                statistics = SqlStatistics.StartTimer(this.Statistics);
                this.SetTimeout();
                num = this.GetBytesInternal(i, dataIndex, buffer, bufferIndex, length);
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
            }
            return num;
        }

        internal virtual long GetBytesInternal(int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
        {
            long num3;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                byte[] unicodeBytes;
                int maxLen = 0;
                if (this.IsCommandBehavior(CommandBehavior.SequentialAccess))
                {
                    if ((0 > i) || (i >= this._metaData.Length))
                    {
                        throw new IndexOutOfRangeException();
                    }
                    if (this._nextColumnDataToRead > i)
                    {
                        throw ADP.NonSequentialColumnAccess(i, this._nextColumnDataToRead);
                    }
                    if (this._nextColumnHeaderToRead <= i)
                    {
                        this.ReadColumnHeader(i);
                    }
                    if ((this._data[i] != null) && this._data[i].IsNull)
                    {
                        throw new SqlNullValueException();
                    }
                    if (0L == this._columnDataBytesRemaining)
                    {
                        return 0L;
                    }
                    if (buffer == null)
                    {
                        if (this._metaData[i].metaType.IsPlp)
                        {
                            return (long) this._parser.PlpBytesTotalLength(this._stateObj);
                        }
                        return this._columnDataBytesRemaining;
                    }
                    if (dataIndex < 0L)
                    {
                        throw ADP.NegativeParameter("dataIndex");
                    }
                    if (dataIndex < this._columnDataBytesRead)
                    {
                        throw ADP.NonSeqByteAccess(dataIndex, this._columnDataBytesRead, "GetBytes");
                    }
                    long num = dataIndex - this._columnDataBytesRead;
                    if ((num > this._columnDataBytesRemaining) && !this._metaData[i].metaType.IsPlp)
                    {
                        return 0L;
                    }
                    if ((bufferIndex < 0) || (bufferIndex >= buffer.Length))
                    {
                        throw ADP.InvalidDestinationBufferIndex(buffer.Length, bufferIndex, "bufferIndex");
                    }
                    if ((length + bufferIndex) > buffer.Length)
                    {
                        throw ADP.InvalidBufferSizeOrIndex(length, bufferIndex);
                    }
                    if (length < 0)
                    {
                        throw ADP.InvalidDataLength((long) length);
                    }
                    if (this._metaData[i].metaType.IsPlp)
                    {
                        if (num > 0L)
                        {
                            num = (long) this._parser.SkipPlpValue((ulong) num, this._stateObj);
                            this._columnDataBytesRead += num;
                        }
                        num = this._stateObj.ReadPlpBytes(ref buffer, bufferIndex, length);
                        this._columnDataBytesRead += num;
                        this._columnDataBytesRemaining = (long) this._parser.PlpBytesLeft(this._stateObj);
                        return num;
                    }
                    if (num > 0L)
                    {
                        this._parser.SkipLongBytes((ulong) num, this._stateObj);
                        this._columnDataBytesRead += num;
                        this._columnDataBytesRemaining -= num;
                    }
                    num = (this._columnDataBytesRemaining < length) ? this._columnDataBytesRemaining : ((long) length);
                    this._stateObj.ReadByteArray(buffer, bufferIndex, (int) num);
                    this._columnDataBytesRead += num;
                    this._columnDataBytesRemaining -= num;
                    return num;
                }
                if (dataIndex < 0L)
                {
                    throw ADP.NegativeParameter("dataIndex");
                }
                if (dataIndex > 0x7fffffffL)
                {
                    throw ADP.InvalidSourceBufferIndex(maxLen, dataIndex, "dataIndex");
                }
                int sourceIndex = (int) dataIndex;
                if (this._metaData[i].metaType.IsBinType)
                {
                    unicodeBytes = this.GetSqlBinary(i).Value;
                }
                else
                {
                    SqlString sqlString = this.GetSqlString(i);
                    if (this._metaData[i].metaType.IsNCharType)
                    {
                        unicodeBytes = sqlString.GetUnicodeBytes();
                    }
                    else
                    {
                        unicodeBytes = sqlString.GetNonUnicodeBytes();
                    }
                }
                maxLen = unicodeBytes.Length;
                if (buffer == null)
                {
                    return (long) maxLen;
                }
                if ((sourceIndex < 0) || (sourceIndex >= maxLen))
                {
                    return 0L;
                }
                try
                {
                    if (sourceIndex < maxLen)
                    {
                        if ((sourceIndex + length) > maxLen)
                        {
                            maxLen -= sourceIndex;
                        }
                        else
                        {
                            maxLen = length;
                        }
                    }
                    Array.Copy(unicodeBytes, sourceIndex, buffer, bufferIndex, maxLen);
                }
                catch (Exception exception4)
                {
                    if (!ADP.IsCatchableExceptionType(exception4))
                    {
                        throw;
                    }
                    maxLen = unicodeBytes.Length;
                    if (length < 0)
                    {
                        throw ADP.InvalidDataLength((long) length);
                    }
                    if ((bufferIndex < 0) || (bufferIndex >= buffer.Length))
                    {
                        throw ADP.InvalidDestinationBufferIndex(buffer.Length, bufferIndex, "bufferIndex");
                    }
                    if ((maxLen + bufferIndex) > buffer.Length)
                    {
                        throw ADP.InvalidBufferSizeOrIndex(maxLen, bufferIndex);
                    }
                    throw;
                }
                num3 = maxLen;
            }
            catch (OutOfMemoryException exception3)
            {
                this._isClosed = true;
                if (this._connection != null)
                {
                    this._connection.Abort(exception3);
                }
                throw;
            }
            catch (StackOverflowException exception2)
            {
                this._isClosed = true;
                if (this._connection != null)
                {
                    this._connection.Abort(exception2);
                }
                throw;
            }
            catch (ThreadAbortException exception)
            {
                this._isClosed = true;
                if (this._connection != null)
                {
                    this._connection.Abort(exception);
                }
                throw;
            }
            return num3;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override char GetChar(int i)
        {
            throw ADP.NotSupported();
        }

        public override long GetChars(int i, long dataIndex, char[] buffer, int bufferIndex, int length)
        {
            long num3;
            SqlStatistics statistics = null;
            if ((this.MetaData == null) || !this._dataReady)
            {
                throw SQL.InvalidRead();
            }
            if ((0 > i) || (i >= this._metaData.Length))
            {
                throw new IndexOutOfRangeException();
            }
            try
            {
                statistics = SqlStatistics.StartTimer(this.Statistics);
                this.SetTimeout();
                if (this._metaData[i].metaType.IsPlp && this.IsCommandBehavior(CommandBehavior.SequentialAccess))
                {
                    if (length < 0)
                    {
                        throw ADP.InvalidDataLength((long) length);
                    }
                    if ((bufferIndex < 0) || ((buffer != null) && (bufferIndex >= buffer.Length)))
                    {
                        throw ADP.InvalidDestinationBufferIndex(buffer.Length, bufferIndex, "bufferIndex");
                    }
                    if ((buffer != null) && ((length + bufferIndex) > buffer.Length))
                    {
                        throw ADP.InvalidBufferSizeOrIndex(length, bufferIndex);
                    }
                    if (this._metaData[i].type == SqlDbType.Xml)
                    {
                        return this.GetStreamingXmlChars(i, dataIndex, buffer, bufferIndex, length);
                    }
                    return this.GetCharsFromPlpData(i, dataIndex, buffer, bufferIndex, length);
                }
                if (((this._nextColumnDataToRead == (i + 1)) && (this._nextColumnHeaderToRead == (i + 1))) && (this._columnDataChars != null))
                {
                    if (this.IsCommandBehavior(CommandBehavior.SequentialAccess) && (dataIndex < this._columnDataCharsRead))
                    {
                        throw ADP.NonSeqByteAccess(dataIndex, this._columnDataCharsRead, "GetChars");
                    }
                }
                else
                {
                    this._columnDataChars = this.GetSqlString(i).Value.ToCharArray();
                    this._columnDataCharsRead = 0L;
                }
                int maxLen = this._columnDataChars.Length;
                if (dataIndex > 0x7fffffffL)
                {
                    throw ADP.InvalidSourceBufferIndex(maxLen, dataIndex, "dataIndex");
                }
                int sourceIndex = (int) dataIndex;
                if (buffer == null)
                {
                    return (long) maxLen;
                }
                if ((sourceIndex < 0) || (sourceIndex >= maxLen))
                {
                    return 0L;
                }
                try
                {
                    if (sourceIndex < maxLen)
                    {
                        if ((sourceIndex + length) > maxLen)
                        {
                            maxLen -= sourceIndex;
                        }
                        else
                        {
                            maxLen = length;
                        }
                    }
                    Array.Copy(this._columnDataChars, sourceIndex, buffer, bufferIndex, maxLen);
                    this._columnDataCharsRead += maxLen;
                }
                catch (Exception exception)
                {
                    if (!ADP.IsCatchableExceptionType(exception))
                    {
                        throw;
                    }
                    maxLen = this._columnDataChars.Length;
                    if (length < 0)
                    {
                        throw ADP.InvalidDataLength((long) length);
                    }
                    if ((bufferIndex < 0) || (bufferIndex >= buffer.Length))
                    {
                        throw ADP.InvalidDestinationBufferIndex(buffer.Length, bufferIndex, "bufferIndex");
                    }
                    if ((maxLen + bufferIndex) > buffer.Length)
                    {
                        throw ADP.InvalidBufferSizeOrIndex(maxLen, bufferIndex);
                    }
                    throw;
                }
                num3 = maxLen;
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
            }
            return num3;
        }

        private long GetCharsFromPlpData(int i, long dataIndex, char[] buffer, int bufferIndex, int length)
        {
            long num2;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                long num;
                if ((this.MetaData == null) || !this._dataReady)
                {
                    throw SQL.InvalidRead();
                }
                if (this._nextColumnDataToRead > i)
                {
                    throw ADP.NonSequentialColumnAccess(i, this._nextColumnDataToRead);
                }
                if (!this._metaData[i].metaType.IsCharType)
                {
                    throw SQL.NonCharColumn(this._metaData[i].column);
                }
                if (this._nextColumnHeaderToRead <= i)
                {
                    this.ReadColumnHeader(i);
                }
                if ((this._data[i] != null) && this._data[i].IsNull)
                {
                    throw new SqlNullValueException();
                }
                if (dataIndex < this._columnDataCharsRead)
                {
                    throw ADP.NonSeqByteAccess(dataIndex, this._columnDataCharsRead, "GetChars");
                }
                bool isNCharType = this._metaData[i].metaType.IsNCharType;
                if (0L == this._columnDataBytesRemaining)
                {
                    return 0L;
                }
                if (buffer == null)
                {
                    num = (long) this._parser.PlpBytesTotalLength(this._stateObj);
                    return ((isNCharType && (num > 0L)) ? (num >> 1) : num);
                }
                if (dataIndex > this._columnDataCharsRead)
                {
                    num = dataIndex - this._columnDataCharsRead;
                    num = isNCharType ? (num << 1) : num;
                    num = (long) this._parser.SkipPlpValue((ulong) num, this._stateObj);
                    this._columnDataBytesRead += num;
                    this._columnDataCharsRead += (isNCharType && (num > 0L)) ? (num >> 1) : num;
                }
                num = length;
                if (isNCharType)
                {
                    num = this._parser.ReadPlpUnicodeChars(ref buffer, bufferIndex, length, this._stateObj);
                    this._columnDataBytesRead += num << 1;
                }
                else
                {
                    num = this._parser.ReadPlpAnsiChars(ref buffer, bufferIndex, length, this._metaData[i], this._stateObj);
                    this._columnDataBytesRead += num << 1;
                }
                this._columnDataCharsRead += num;
                this._columnDataBytesRemaining = (long) this._parser.PlpBytesLeft(this._stateObj);
                num2 = num;
            }
            catch (OutOfMemoryException exception3)
            {
                this._isClosed = true;
                if (this._connection != null)
                {
                    this._connection.Abort(exception3);
                }
                throw;
            }
            catch (StackOverflowException exception2)
            {
                this._isClosed = true;
                if (this._connection != null)
                {
                    this._connection.Abort(exception2);
                }
                throw;
            }
            catch (ThreadAbortException exception)
            {
                this._isClosed = true;
                if (this._connection != null)
                {
                    this._connection.Abort(exception);
                }
                throw;
            }
            return num2;
        }

        public override string GetDataTypeName(int i)
        {
            SqlStatistics statistics = null;
            string dataTypeNameInternal;
            try
            {
                statistics = SqlStatistics.StartTimer(this.Statistics);
                if (this.MetaData == null)
                {
                    throw SQL.InvalidRead();
                }
                dataTypeNameInternal = this.GetDataTypeNameInternal(this._metaData[i]);
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
            }
            return dataTypeNameInternal;
        }

        private string GetDataTypeNameInternal(_SqlMetaData metaData)
        {
            if ((this._typeSystem <= SqlConnectionString.TypeSystem.SQLServer2005) && metaData.IsNewKatmaiDateTimeType)
            {
                return MetaType.MetaNVarChar.TypeName;
            }
            if ((this._typeSystem <= SqlConnectionString.TypeSystem.SQLServer2005) && metaData.IsLargeUdt)
            {
                if (this._typeSystem == SqlConnectionString.TypeSystem.SQLServer2005)
                {
                    return MetaType.MetaMaxVarBinary.TypeName;
                }
                return MetaType.MetaImage.TypeName;
            }
            if (this._typeSystem != SqlConnectionString.TypeSystem.SQLServer2000)
            {
                if (metaData.type == SqlDbType.Udt)
                {
                    return (metaData.udtDatabaseName + "." + metaData.udtSchemaName + "." + metaData.udtTypeName);
                }
                return metaData.metaType.TypeName;
            }
            return this.GetVersionedMetaType(metaData.metaType).TypeName;
        }

        public override DateTime GetDateTime(int i)
        {
            this.ReadColumn(i);
            DateTime dateTime = this._data[i].DateTime;
            if ((this._typeSystem <= SqlConnectionString.TypeSystem.SQLServer2005) && this._metaData[i].IsNewKatmaiDateTimeType)
            {
                dateTime = (DateTime) this._data[i].String;
            }
            return dateTime;
        }

        public virtual DateTimeOffset GetDateTimeOffset(int i)
        {
            this.ReadColumn(i);
            DateTimeOffset dateTimeOffset = this._data[i].DateTimeOffset;
            if (this._typeSystem <= SqlConnectionString.TypeSystem.SQLServer2005)
            {
                dateTimeOffset = (DateTimeOffset) this._data[i].String;
            }
            return dateTimeOffset;
        }

        public override decimal GetDecimal(int i)
        {
            this.ReadColumn(i);
            return this._data[i].Decimal;
        }

        public override double GetDouble(int i)
        {
            this.ReadColumn(i);
            return this._data[i].Double;
        }

        public override IEnumerator GetEnumerator()
        {
            return new DbEnumerator(this, this.IsCommandBehavior(CommandBehavior.CloseConnection));
        }

        public override Type GetFieldType(int i)
        {
            SqlStatistics statistics = null;
            Type fieldTypeInternal;
            try
            {
                statistics = SqlStatistics.StartTimer(this.Statistics);
                if (this.MetaData == null)
                {
                    throw SQL.InvalidRead();
                }
                fieldTypeInternal = this.GetFieldTypeInternal(this._metaData[i]);
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
            }
            return fieldTypeInternal;
        }

        private Type GetFieldTypeInternal(_SqlMetaData metaData)
        {
            if ((this._typeSystem <= SqlConnectionString.TypeSystem.SQLServer2005) && metaData.IsNewKatmaiDateTimeType)
            {
                return MetaType.MetaNVarChar.ClassType;
            }
            if ((this._typeSystem <= SqlConnectionString.TypeSystem.SQLServer2005) && metaData.IsLargeUdt)
            {
                if (this._typeSystem == SqlConnectionString.TypeSystem.SQLServer2005)
                {
                    return MetaType.MetaMaxVarBinary.ClassType;
                }
                return MetaType.MetaImage.ClassType;
            }
            if (this._typeSystem != SqlConnectionString.TypeSystem.SQLServer2000)
            {
                if (metaData.type == SqlDbType.Udt)
                {
                    SqlConnection.CheckGetExtendedUDTInfo(metaData, false);
                    return metaData.udtType;
                }
                return metaData.metaType.ClassType;
            }
            return this.GetVersionedMetaType(metaData.metaType).ClassType;
        }

        public override float GetFloat(int i)
        {
            this.ReadColumn(i);
            return this._data[i].Single;
        }

        public override Guid GetGuid(int i)
        {
            this.ReadColumn(i);
            return this._data[i].SqlGuid.Value;
        }

        public override short GetInt16(int i)
        {
            this.ReadColumn(i);
            return this._data[i].Int16;
        }

        public override int GetInt32(int i)
        {
            this.ReadColumn(i);
            return this._data[i].Int32;
        }

        public override long GetInt64(int i)
        {
            this.ReadColumn(i);
            return this._data[i].Int64;
        }

        internal virtual SmiExtendedMetaData[] GetInternalSmiMetaData()
        {
            SmiExtendedMetaData[] dataArray = null;
            _SqlMetaDataSet metaData = this.MetaData;
            if ((metaData != null) && (0 < metaData.Length))
            {
                dataArray = new SmiExtendedMetaData[metaData.visibleColumns];
                for (int i = 0; i < metaData.Length; i++)
                {
                    _SqlMetaData data = metaData[i];
                    if (!data.isHidden)
                    {
                        SqlCollation collation = data.collation;
                        string xmlSchemaCollectionDatabase = null;
                        string xmlSchemaCollectionOwningSchema = null;
                        string xmlSchemaCollectionName = null;
                        if (SqlDbType.Xml == data.type)
                        {
                            xmlSchemaCollectionDatabase = data.xmlSchemaCollectionDatabase;
                            xmlSchemaCollectionOwningSchema = data.xmlSchemaCollectionOwningSchema;
                            xmlSchemaCollectionName = data.xmlSchemaCollectionName;
                        }
                        else if (SqlDbType.Udt == data.type)
                        {
                            SqlConnection.CheckGetExtendedUDTInfo(data, true);
                            xmlSchemaCollectionDatabase = data.udtDatabaseName;
                            xmlSchemaCollectionOwningSchema = data.udtSchemaName;
                            xmlSchemaCollectionName = data.udtTypeName;
                        }
                        int length = data.length;
                        if (length > 0x1f40)
                        {
                            length = -1;
                        }
                        else if ((SqlDbType.NChar == data.type) || (SqlDbType.NVarChar == data.type))
                        {
                            length /= ADP.CharSize;
                        }
                        dataArray[i] = new SmiQueryMetaData(data.type, (long) length, data.precision, data.scale, (collation != null) ? ((long) collation.LCID) : ((long) this._defaultLCID), (collation != null) ? collation.SqlCompareOptions : SqlCompareOptions.None, data.udtType, false, null, null, data.column, xmlSchemaCollectionDatabase, xmlSchemaCollectionOwningSchema, xmlSchemaCollectionName, data.isNullable, data.serverName, data.catalogName, data.schemaName, data.tableName, data.baseColumn, data.isKey, data.isIdentity, 0 == data.updatability, data.isExpression, data.isDifferentName, data.isHidden);
                    }
                }
            }
            return dataArray;
        }

        internal virtual int GetLocaleId(int i)
        {
            _SqlMetaData data = this.MetaData[i];
            if (data.collation != null)
            {
                return data.collation.LCID;
            }
            return 0;
        }

        public override string GetName(int i)
        {
            if (this.MetaData == null)
            {
                throw SQL.InvalidRead();
            }
            return this._metaData[i].column;
        }

        public override int GetOrdinal(string name)
        {
            SqlStatistics statistics = null;
            int ordinal;
            try
            {
                statistics = SqlStatistics.StartTimer(this.Statistics);
                if (this._fieldNameLookup == null)
                {
                    if (this.MetaData == null)
                    {
                        throw SQL.InvalidRead();
                    }
                    this._fieldNameLookup = new FieldNameLookup(this, this._defaultLCID);
                }
                ordinal = this._fieldNameLookup.GetOrdinal(name);
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
            }
            return ordinal;
        }

        public override Type GetProviderSpecificFieldType(int i)
        {
            SqlStatistics statistics = null;
            Type providerSpecificFieldTypeInternal;
            try
            {
                statistics = SqlStatistics.StartTimer(this.Statistics);
                if (this.MetaData == null)
                {
                    throw SQL.InvalidRead();
                }
                providerSpecificFieldTypeInternal = this.GetProviderSpecificFieldTypeInternal(this._metaData[i]);
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
            }
            return providerSpecificFieldTypeInternal;
        }

        private Type GetProviderSpecificFieldTypeInternal(_SqlMetaData metaData)
        {
            if ((this._typeSystem <= SqlConnectionString.TypeSystem.SQLServer2005) && metaData.IsNewKatmaiDateTimeType)
            {
                return MetaType.MetaNVarChar.SqlType;
            }
            if ((this._typeSystem <= SqlConnectionString.TypeSystem.SQLServer2005) && metaData.IsLargeUdt)
            {
                if (this._typeSystem == SqlConnectionString.TypeSystem.SQLServer2005)
                {
                    return MetaType.MetaMaxVarBinary.SqlType;
                }
                return MetaType.MetaImage.SqlType;
            }
            if (this._typeSystem != SqlConnectionString.TypeSystem.SQLServer2000)
            {
                if (metaData.type == SqlDbType.Udt)
                {
                    SqlConnection.CheckGetExtendedUDTInfo(metaData, false);
                    return metaData.udtType;
                }
                return metaData.metaType.SqlType;
            }
            return this.GetVersionedMetaType(metaData.metaType).SqlType;
        }

        public override object GetProviderSpecificValue(int i)
        {
            return this.GetSqlValue(i);
        }

        public override int GetProviderSpecificValues(object[] values)
        {
            return this.GetSqlValues(values);
        }

        public override DataTable GetSchemaTable()
        {
            DataTable table;
            SqlStatistics statistics = null;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<sc.SqlDataReader.GetSchemaTable|API> %d#", this.ObjectID);
            try
            {
                statistics = SqlStatistics.StartTimer(this.Statistics);
                if (((this._metaData == null) || (this._metaData.schemaTable == null)) && (this.MetaData != null))
                {
                    this._metaData.schemaTable = this.BuildSchemaTable();
                }
                if (this._metaData != null)
                {
                    return this._metaData.schemaTable;
                }
                table = null;
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
                Bid.ScopeLeave(ref ptr);
            }
            return table;
        }

        public virtual SqlBinary GetSqlBinary(int i)
        {
            this.ReadColumn(i);
            return this._data[i].SqlBinary;
        }

        public virtual SqlBoolean GetSqlBoolean(int i)
        {
            this.ReadColumn(i);
            return this._data[i].SqlBoolean;
        }

        public virtual SqlByte GetSqlByte(int i)
        {
            this.ReadColumn(i);
            return this._data[i].SqlByte;
        }

        public virtual SqlBytes GetSqlBytes(int i)
        {
            if (this.MetaData == null)
            {
                throw SQL.InvalidRead();
            }
            this.ReadColumn(i);
            return new SqlBytes(this._data[i].SqlBinary);
        }

        public virtual SqlChars GetSqlChars(int i)
        {
            SqlString katmaiDateTimeSqlString;
            this.ReadColumn(i);
            if ((this._typeSystem <= SqlConnectionString.TypeSystem.SQLServer2005) && this._metaData[i].IsNewKatmaiDateTimeType)
            {
                katmaiDateTimeSqlString = this._data[i].KatmaiDateTimeSqlString;
            }
            else
            {
                katmaiDateTimeSqlString = this._data[i].SqlString;
            }
            return new SqlChars(katmaiDateTimeSqlString);
        }

        public virtual SqlDateTime GetSqlDateTime(int i)
        {
            this.ReadColumn(i);
            return this._data[i].SqlDateTime;
        }

        public virtual SqlDecimal GetSqlDecimal(int i)
        {
            this.ReadColumn(i);
            return this._data[i].SqlDecimal;
        }

        public virtual SqlDouble GetSqlDouble(int i)
        {
            this.ReadColumn(i);
            return this._data[i].SqlDouble;
        }

        public virtual SqlGuid GetSqlGuid(int i)
        {
            this.ReadColumn(i);
            return this._data[i].SqlGuid;
        }

        public virtual SqlInt16 GetSqlInt16(int i)
        {
            this.ReadColumn(i);
            return this._data[i].SqlInt16;
        }

        public virtual SqlInt32 GetSqlInt32(int i)
        {
            this.ReadColumn(i);
            return this._data[i].SqlInt32;
        }

        public virtual SqlInt64 GetSqlInt64(int i)
        {
            this.ReadColumn(i);
            return this._data[i].SqlInt64;
        }

        public virtual SqlMoney GetSqlMoney(int i)
        {
            this.ReadColumn(i);
            return this._data[i].SqlMoney;
        }

        public virtual SqlSingle GetSqlSingle(int i)
        {
            this.ReadColumn(i);
            return this._data[i].SqlSingle;
        }

        public virtual SqlString GetSqlString(int i)
        {
            this.ReadColumn(i);
            if ((this._typeSystem <= SqlConnectionString.TypeSystem.SQLServer2005) && this._metaData[i].IsNewKatmaiDateTimeType)
            {
                return this._data[i].KatmaiDateTimeSqlString;
            }
            return this._data[i].SqlString;
        }

        public virtual object GetSqlValue(int i)
        {
            SqlStatistics statistics = null;
            object sqlValueInternal;
            try
            {
                statistics = SqlStatistics.StartTimer(this.Statistics);
                if ((this.MetaData == null) || !this._dataReady)
                {
                    throw SQL.InvalidRead();
                }
                this.SetTimeout();
                sqlValueInternal = this.GetSqlValueInternal(i);
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
            }
            return sqlValueInternal;
        }

        private object GetSqlValueInternal(int i)
        {
            this.ReadColumn(i, false);
            if ((this._typeSystem <= SqlConnectionString.TypeSystem.SQLServer2005) && this._metaData[i].IsNewKatmaiDateTimeType)
            {
                return this._data[i].KatmaiDateTimeSqlString;
            }
            if ((this._typeSystem > SqlConnectionString.TypeSystem.SQLServer2005) || !this._metaData[i].IsLargeUdt)
            {
                if (this._typeSystem != SqlConnectionString.TypeSystem.SQLServer2000)
                {
                    if (this._metaData[i].type == SqlDbType.Udt)
                    {
                        SqlConnection.CheckGetExtendedUDTInfo(this._metaData[i], true);
                        return this.Connection.GetUdtValue(this._data[i].Value, this._metaData[i], false);
                    }
                    return this._data[i].SqlValue;
                }
                if (this._metaData[i].type == SqlDbType.Xml)
                {
                    return this._data[i].SqlString;
                }
            }
            return this._data[i].SqlValue;
        }

        public virtual int GetSqlValues(object[] values)
        {
            SqlStatistics statistics = null;
            int num3;
            try
            {
                statistics = SqlStatistics.StartTimer(this.Statistics);
                if ((this.MetaData == null) || !this._dataReady)
                {
                    throw SQL.InvalidRead();
                }
                if (values == null)
                {
                    throw ADP.ArgumentNull("values");
                }
                this.SetTimeout();
                int num2 = (values.Length < this._metaData.visibleColumns) ? values.Length : this._metaData.visibleColumns;
                for (int i = 0; i < num2; i++)
                {
                    values[this._metaData.indexMap[i]] = this.GetSqlValueInternal(i);
                }
                num3 = num2;
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
            }
            return num3;
        }

        public virtual SqlXml GetSqlXml(int i)
        {
            this.ReadColumn(i);
            SqlXml xml = null;
            if (this._typeSystem != SqlConnectionString.TypeSystem.SQLServer2000)
            {
                return (this._data[i].IsNull ? SqlXml.Null : this._data[i].SqlCachedBuffer.ToSqlXml());
            }
            xml = this._data[i].IsNull ? SqlXml.Null : this._data[i].SqlCachedBuffer.ToSqlXml();
            return (SqlXml) this._data[i].String;
        }

        internal long GetStreamingXmlChars(int i, long dataIndex, char[] buffer, int bufferIndex, int length)
        {
            SqlStreamingXml xml = null;
            if ((this._streamingXml != null) && (this._streamingXml.ColumnOrdinal != i))
            {
                this._streamingXml.Close();
                this._streamingXml = null;
            }
            if (this._streamingXml == null)
            {
                xml = new SqlStreamingXml(i, this);
            }
            else
            {
                xml = this._streamingXml;
            }
            long num = xml.GetChars(dataIndex, buffer, bufferIndex, length);
            if (this._streamingXml == null)
            {
                this._streamingXml = xml;
            }
            return num;
        }

        public override string GetString(int i)
        {
            this.ReadColumn(i);
            if ((this._typeSystem <= SqlConnectionString.TypeSystem.SQLServer2005) && this._metaData[i].IsNewKatmaiDateTimeType)
            {
                return this._data[i].KatmaiDateTimeString;
            }
            return this._data[i].String;
        }

        public virtual TimeSpan GetTimeSpan(int i)
        {
            this.ReadColumn(i);
            TimeSpan time = this._data[i].Time;
            if (this._typeSystem <= SqlConnectionString.TypeSystem.SQLServer2005)
            {
                time = (TimeSpan) this._data[i].String;
            }
            return time;
        }

        public override object GetValue(int i)
        {
            SqlStatistics statistics = null;
            object valueInternal;
            try
            {
                statistics = SqlStatistics.StartTimer(this.Statistics);
                if ((this.MetaData == null) || !this._dataReady)
                {
                    throw SQL.InvalidRead();
                }
                this.SetTimeout();
                valueInternal = this.GetValueInternal(i);
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
            }
            return valueInternal;
        }

        private object GetValueInternal(int i)
        {
            this.ReadColumn(i, false);
            if ((this._typeSystem <= SqlConnectionString.TypeSystem.SQLServer2005) && this._metaData[i].IsNewKatmaiDateTimeType)
            {
                if (this._data[i].IsNull)
                {
                    return DBNull.Value;
                }
                return this._data[i].KatmaiDateTimeString;
            }
            if (((this._typeSystem > SqlConnectionString.TypeSystem.SQLServer2005) || !this._metaData[i].IsLargeUdt) && (this._typeSystem != SqlConnectionString.TypeSystem.SQLServer2000))
            {
                if (this._metaData[i].type != SqlDbType.Udt)
                {
                    return this._data[i].Value;
                }
                SqlConnection.CheckGetExtendedUDTInfo(this._metaData[i], true);
                return this.Connection.GetUdtValue(this._data[i].Value, this._metaData[i], true);
            }
            return this._data[i].Value;
        }

        public override int GetValues(object[] values)
        {
            SqlStatistics statistics = null;
            int num3;
            try
            {
                statistics = SqlStatistics.StartTimer(this.Statistics);
                if ((this.MetaData == null) || !this._dataReady)
                {
                    throw SQL.InvalidRead();
                }
                if (values == null)
                {
                    throw ADP.ArgumentNull("values");
                }
                int num2 = (values.Length < this._metaData.visibleColumns) ? values.Length : this._metaData.visibleColumns;
                this.SetTimeout();
                for (int i = 0; i < num2; i++)
                {
                    values[this._metaData.indexMap[i]] = this.GetValueInternal(i);
                }
                if (this._rowException != null)
                {
                    throw this._rowException;
                }
                num3 = num2;
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
            }
            return num3;
        }

        private MetaType GetVersionedMetaType(MetaType actualMetaType)
        {
            if (actualMetaType == MetaType.MetaUdt)
            {
                return MetaType.MetaVarBinary;
            }
            if (actualMetaType == MetaType.MetaXml)
            {
                return MetaType.MetaNText;
            }
            if (actualMetaType == MetaType.MetaMaxVarBinary)
            {
                return MetaType.MetaImage;
            }
            if (actualMetaType == MetaType.MetaMaxVarChar)
            {
                return MetaType.MetaText;
            }
            if (actualMetaType == MetaType.MetaMaxNVarChar)
            {
                return MetaType.MetaNText;
            }
            return actualMetaType;
        }

        private bool HasMoreResults()
        {
            if (this._parser != null)
            {
                if (!this.HasMoreRows())
                {
                    while (this._stateObj._pendingData)
                    {
                        switch (this._stateObj.PeekByte())
                        {
                            case 0xd1:
                                return true;

                            case 0xd3:
                                if (this._altRowStatus == ALTROWSTATUS.Null)
                                {
                                    this._altMetaDataSetCollection.metaDataSet = this._metaData;
                                    this._metaData = null;
                                }
                                this._altRowStatus = ALTROWSTATUS.AltRow;
                                this._hasRows = true;
                                return true;

                            case 0xfd:
                                this._altRowStatus = ALTROWSTATUS.Null;
                                this._metaData = null;
                                this._altMetaDataSetCollection = null;
                                return true;

                            case 0x81:
                                return true;
                        }
                        this._parser.Run(RunBehavior.ReturnImmediately, this._command, this, null, this._stateObj);
                    }
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        private bool HasMoreRows()
        {
            if (this._parser != null)
            {
                byte num;
                if (this._dataReady)
                {
                    return true;
                }
                switch (this._altRowStatus)
                {
                    case ALTROWSTATUS.AltRow:
                        return true;

                    case ALTROWSTATUS.Done:
                        return false;

                    default:
                    {
                        if (!this._stateObj._pendingData)
                        {
                            goto Label_00E6;
                        }
                        num = this._stateObj.PeekByte();
                        bool flag = false;
                        while ((((((num == 0xfd) || (num == 0xfe)) || (num == 0xff)) || (!flag && (num == 0xa9))) || (!flag && (num == 170))) || (!flag && (num == 0xab)))
                        {
                            if (((num == 0xfd) || (num == 0xfe)) || (num == 0xff))
                            {
                                flag = true;
                            }
                            this._parser.Run(RunBehavior.ReturnImmediately, this._command, this, null, this._stateObj);
                            if (!this._stateObj._pendingData)
                            {
                                break;
                            }
                            num = this._stateObj.PeekByte();
                        }
                        break;
                    }
                }
                if (0xd1 == num)
                {
                    return true;
                }
            }
        Label_00E6:
            return false;
        }

        protected bool IsCommandBehavior(CommandBehavior condition)
        {
            return (condition == (condition & this._commandBehavior));
        }

        public override bool IsDBNull(int i)
        {
            this.SetTimeout();
            this.ReadColumnHeader(i);
            return this._data[i].IsNull;
        }

        public override bool NextResult()
        {
            bool flag;
            SqlStatistics statistics = null;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<sc.SqlDataReader.NextResult|API> %d#", this.ObjectID);
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                statistics = SqlStatistics.StartTimer(this.Statistics);
                this.SetTimeout();
                if (this.IsClosed)
                {
                    throw ADP.DataReaderClosed("NextResult");
                }
                this._fieldNameLookup = null;
                bool flag2 = false;
                this._hasRows = false;
                if (this.IsCommandBehavior(CommandBehavior.SingleResult))
                {
                    this.CloseInternal(false);
                    this.ClearMetaData();
                    return flag2;
                }
                if (this._parser != null)
                {
                    while (this.ReadInternal(false))
                    {
                    }
                }
                if (this._parser != null)
                {
                    if (this.HasMoreResults())
                    {
                        this._metaDataConsumed = false;
                        this._browseModeInfoConsumed = false;
                        switch (this._altRowStatus)
                        {
                            case ALTROWSTATUS.AltRow:
                            {
                                int altRowId = this._parser.GetAltRowId(this._stateObj);
                                _SqlMetaDataSet altMetaData = this._altMetaDataSetCollection.GetAltMetaData(altRowId);
                                if (altMetaData != null)
                                {
                                    this._metaData = altMetaData;
                                    this._metaData.indexMap = altMetaData.indexMap;
                                }
                                break;
                            }
                            case ALTROWSTATUS.Done:
                                this._metaData = this._altMetaDataSetCollection.metaDataSet;
                                this._altRowStatus = ALTROWSTATUS.Null;
                                break;

                            default:
                                this.ConsumeMetaData();
                                if (this._metaData == null)
                                {
                                    return false;
                                }
                                break;
                        }
                        return true;
                    }
                    this.CloseInternal(false);
                    this.SetMetaData(null, false);
                    return flag2;
                }
                this.ClearMetaData();
                return flag2;
            }
            catch (OutOfMemoryException exception3)
            {
                this._isClosed = true;
                if (this._connection != null)
                {
                    this._connection.Abort(exception3);
                }
                throw;
            }
            catch (StackOverflowException exception2)
            {
                this._isClosed = true;
                if (this._connection != null)
                {
                    this._connection.Abort(exception2);
                }
                throw;
            }
            catch (ThreadAbortException exception)
            {
                this._isClosed = true;
                if (this._connection != null)
                {
                    this._connection.Abort(exception);
                }
                throw;
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
                Bid.ScopeLeave(ref ptr);
            }
            return flag;
        }

        public override bool Read()
        {
            return this.ReadInternal(true);
        }

        private void ReadColumn(int i)
        {
            this.ReadColumn(i, true);
        }

        private void ReadColumn(int i, bool setTimeout)
        {
            if ((this.MetaData == null) || !this._dataReady)
            {
                throw SQL.InvalidRead();
            }
            if ((0 > i) || (i >= this._metaData.Length))
            {
                throw new IndexOutOfRangeException();
            }
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                if (setTimeout)
                {
                    this.SetTimeout();
                }
                if (this._nextColumnHeaderToRead <= i)
                {
                    this.ReadColumnHeader(i);
                }
                if (this._nextColumnDataToRead == i)
                {
                    this.ReadColumnData();
                }
                else if ((this._nextColumnDataToRead > i) && this.IsCommandBehavior(CommandBehavior.SequentialAccess))
                {
                    throw ADP.NonSequentialColumnAccess(i, this._nextColumnDataToRead);
                }
            }
            catch (OutOfMemoryException exception3)
            {
                this._isClosed = true;
                if (this._connection != null)
                {
                    this._connection.Abort(exception3);
                }
                throw;
            }
            catch (StackOverflowException exception2)
            {
                this._isClosed = true;
                if (this._connection != null)
                {
                    this._connection.Abort(exception2);
                }
                throw;
            }
            catch (ThreadAbortException exception)
            {
                this._isClosed = true;
                if (this._connection != null)
                {
                    this._connection.Abort(exception);
                }
                throw;
            }
        }

        private void ReadColumnData()
        {
            if (!this._data[this._nextColumnDataToRead].IsNull)
            {
                _SqlMetaData md = this._metaData[this._nextColumnDataToRead];
                this._parser.ReadSqlValue(this._data[this._nextColumnDataToRead], md, (int) this._columnDataBytesRemaining, this._stateObj);
                this._columnDataBytesRemaining = 0L;
            }
            this._nextColumnDataToRead++;
        }

        private void ReadColumnHeader(int i)
        {
            if (!this._dataReady)
            {
                throw SQL.InvalidRead();
            }
            if (i >= this._nextColumnDataToRead)
            {
                bool flag = this.IsCommandBehavior(CommandBehavior.SequentialAccess);
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    if (flag)
                    {
                        if (0 < this._nextColumnDataToRead)
                        {
                            this._data[this._nextColumnDataToRead - 1].Clear();
                        }
                    }
                    else if (this._nextColumnDataToRead < this._nextColumnHeaderToRead)
                    {
                        this.ReadColumnData();
                    }
                    while (this._nextColumnHeaderToRead <= i)
                    {
                        this.ResetBlobState();
                        if (flag)
                        {
                            flag = this._nextColumnHeaderToRead < i;
                        }
                        _SqlMetaData col = this._metaData[this._nextColumnHeaderToRead];
                        if (flag && col.metaType.IsPlp)
                        {
                            this._parser.SkipPlpValue(ulong.MaxValue, this._stateObj);
                            this._nextColumnDataToRead = this._nextColumnHeaderToRead;
                            this._nextColumnHeaderToRead++;
                            this._columnDataBytesRemaining = 0L;
                        }
                        else
                        {
                            bool isNull = false;
                            ulong num = this._parser.ProcessColumnHeader(col, this._stateObj, out isNull);
                            this._nextColumnDataToRead = this._nextColumnHeaderToRead;
                            this._nextColumnHeaderToRead++;
                            if (flag)
                            {
                                this._parser.SkipLongBytes(num, this._stateObj);
                                this._columnDataBytesRemaining = 0L;
                                continue;
                            }
                            if (isNull)
                            {
                                this._parser.GetNullSqlValue(this._data[this._nextColumnDataToRead], col);
                                this._columnDataBytesRemaining = 0L;
                                continue;
                            }
                            this._columnDataBytesRemaining = (long) num;
                            if (i > this._nextColumnDataToRead)
                            {
                                this.ReadColumnData();
                            }
                        }
                    }
                }
                catch (OutOfMemoryException exception3)
                {
                    this._isClosed = true;
                    if (this._connection != null)
                    {
                        this._connection.Abort(exception3);
                    }
                    throw;
                }
                catch (StackOverflowException exception2)
                {
                    this._isClosed = true;
                    if (this._connection != null)
                    {
                        this._connection.Abort(exception2);
                    }
                    throw;
                }
                catch (ThreadAbortException exception)
                {
                    this._isClosed = true;
                    if (this._connection != null)
                    {
                        this._connection.Abort(exception);
                    }
                    throw;
                }
            }
        }

        private bool ReadInternal(bool setTimeout)
        {
            bool flag;
            SqlStatistics statistics = null;
            IntPtr ptr;
            Bid.ScopeEnter(out ptr, "<sc.SqlDataReader.Read|API> %d#", this.ObjectID);
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                statistics = SqlStatistics.StartTimer(this.Statistics);
                if (this._parser == null)
                {
                    goto Label_017B;
                }
                if (setTimeout)
                {
                    this.SetTimeout();
                }
                if (this._dataReady)
                {
                    this.CleanPartialRead();
                }
                this._dataReady = false;
                SqlBuffer.Clear(this._data);
                this._nextColumnHeaderToRead = 0;
                this._nextColumnDataToRead = 0;
                this._columnDataBytesRemaining = -1L;
                if (this._haltRead)
                {
                    goto Label_016A;
                }
                if (this.HasMoreRows())
                {
                    while (this._stateObj._pendingData)
                    {
                        if (this._altRowStatus != ALTROWSTATUS.AltRow)
                        {
                            this._dataReady = this._parser.Run(RunBehavior.ReturnImmediately, this._command, this, null, this._stateObj);
                            if (!this._dataReady)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            this._altRowStatus = ALTROWSTATUS.Done;
                            this._dataReady = true;
                        }
                        break;
                    }
                    if (this._dataReady)
                    {
                        this._haltRead = this.IsCommandBehavior(CommandBehavior.SingleRow);
                        return true;
                    }
                }
                if (!this._stateObj._pendingData)
                {
                    this.CloseInternal(false);
                }
                goto Label_018E;
            Label_010E:
                this._dataReady = this._parser.Run(RunBehavior.ReturnImmediately, this._command, this, null, this._stateObj);
            Label_012E:
                if (this._stateObj._pendingData && !this._dataReady)
                {
                    goto Label_010E;
                }
                if (this._dataReady)
                {
                    this.CleanPartialRead();
                }
                this._dataReady = false;
                SqlBuffer.Clear(this._data);
                this._nextColumnHeaderToRead = 0;
            Label_016A:
                if (this.HasMoreRows())
                {
                    goto Label_012E;
                }
                this._haltRead = false;
                goto Label_018E;
            Label_017B:
                if (this.IsClosed)
                {
                    throw ADP.DataReaderClosed("Read");
                }
            Label_018E:
                return false;
            }
            catch (OutOfMemoryException exception3)
            {
                this._isClosed = true;
                SqlConnection connection3 = this._connection;
                if (connection3 != null)
                {
                    connection3.Abort(exception3);
                }
                throw;
            }
            catch (StackOverflowException exception2)
            {
                this._isClosed = true;
                SqlConnection connection2 = this._connection;
                if (connection2 != null)
                {
                    connection2.Abort(exception2);
                }
                throw;
            }
            catch (ThreadAbortException exception)
            {
                this._isClosed = true;
                SqlConnection connection = this._connection;
                if (connection != null)
                {
                    connection.Abort(exception);
                }
                throw;
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
                Bid.ScopeLeave(ref ptr);
            }
            return flag;
        }

        private void ResetBlobState()
        {
            int num = this._nextColumnHeaderToRead - 1;
            if ((num >= 0) && this._metaData[num].metaType.IsPlp)
            {
                if (this._stateObj._longlen != 0L)
                {
                    this._stateObj.Parser.SkipPlpValue(ulong.MaxValue, this._stateObj);
                }
                if (this._streamingXml != null)
                {
                    SqlStreamingXml xml = this._streamingXml;
                    this._streamingXml = null;
                    xml.Close();
                }
            }
            else if (0L < this._columnDataBytesRemaining)
            {
                this._stateObj.Parser.SkipLongBytes((ulong) this._columnDataBytesRemaining, this._stateObj);
            }
            this._columnDataBytesRemaining = -1L;
            this._columnDataBytesRead = 0L;
            this._columnDataCharsRead = 0L;
            this._columnDataChars = null;
        }

        private void RestoreServerSettings(TdsParser parser, TdsParserStateObject stateObj)
        {
            if ((parser != null) && (this._resetOptionsString != null))
            {
                if (parser.State == TdsParserState.OpenLoggedIn)
                {
                    parser.TdsExecuteSQLBatch(this._resetOptionsString, (this._command != null) ? this._command.CommandTimeout : 0, null, stateObj);
                    parser.Run(RunBehavior.UntilDone, this._command, this, null, stateObj);
                }
                this._resetOptionsString = null;
            }
        }

        internal void SetAltMetaDataSet(_SqlMetaDataSet metaDataSet, bool metaDataConsumed)
        {
            if (this._altMetaDataSetCollection == null)
            {
                this._altMetaDataSetCollection = new _SqlMetaDataSetCollection();
            }
            this._altMetaDataSetCollection.SetAltMetaData(metaDataSet);
            this._metaDataConsumed = metaDataConsumed;
            if (this._metaDataConsumed && (this._parser != null))
            {
                byte num = this._stateObj.PeekByte();
                if (0xa9 == num)
                {
                    this._parser.Run(RunBehavior.ReturnImmediately, this._command, this, null, this._stateObj);
                    num = this._stateObj.PeekByte();
                }
                this._hasRows = 0xd1 == num;
            }
            if ((metaDataSet != null) && ((this._data == null) || (this._data.Length < metaDataSet.Length)))
            {
                this._data = SqlBuffer.CreateBufferArray(metaDataSet.Length);
            }
        }

        internal void SetMetaData(_SqlMetaDataSet metaData, bool moreInfo)
        {
            this._metaData = metaData;
            this._tableNames = null;
            if (this._metaData != null)
            {
                this._metaData.schemaTable = null;
                this._data = SqlBuffer.CreateBufferArray(metaData.Length);
            }
            this._fieldNameLookup = null;
            if (metaData != null)
            {
                if (!moreInfo)
                {
                    this._metaDataConsumed = true;
                    if (this._parser != null)
                    {
                        byte num = this._stateObj.PeekByte();
                        if (num == 0xa9)
                        {
                            this._parser.Run(RunBehavior.ReturnImmediately, null, null, null, this._stateObj);
                            num = this._stateObj.PeekByte();
                        }
                        this._hasRows = 0xd1 == num;
                        if (0x88 == num)
                        {
                            this._metaDataConsumed = false;
                        }
                    }
                }
            }
            else
            {
                this._metaDataConsumed = false;
            }
            this._browseModeInfoConsumed = false;
        }

        private void SetTimeout()
        {
            TdsParserStateObject obj2 = this._stateObj;
            if (obj2 != null)
            {
                obj2.SetTimeoutSeconds(this._timeoutSeconds);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        IDataReader IDataRecord.GetData(int i)
        {
            throw ADP.NotSupported();
        }

        internal bool BrowseModeInfoConsumed
        {
            set
            {
                this._browseModeInfoConsumed = value;
            }
        }

        internal SqlCommand Command
        {
            get
            {
                return this._command;
            }
        }

        protected SqlConnection Connection
        {
            get
            {
                return this._connection;
            }
        }

        public override int Depth
        {
            get
            {
                if (this.IsClosed)
                {
                    throw ADP.DataReaderClosed("Depth");
                }
                return 0;
            }
        }

        public override int FieldCount
        {
            get
            {
                if (this.IsClosed)
                {
                    throw ADP.DataReaderClosed("FieldCount");
                }
                if (this.MetaData == null)
                {
                    return 0;
                }
                return this._metaData.Length;
            }
        }

        public override bool HasRows
        {
            get
            {
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
                return this._isClosed;
            }
        }

        internal bool IsInitialized
        {
            get
            {
                return this._isInitialized;
            }
            set
            {
                this._isInitialized = value;
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
                return this.GetValue(this.GetOrdinal(name));
            }
        }

        internal _SqlMetaDataSet MetaData
        {
            get
            {
                if (this.IsClosed)
                {
                    throw ADP.DataReaderClosed("MetaData");
                }
                if ((this._metaData == null) && !this._metaDataConsumed)
                {
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try
                    {
                        this.ConsumeMetaData();
                    }
                    catch (OutOfMemoryException exception3)
                    {
                        this._isClosed = true;
                        if (this._connection != null)
                        {
                            this._connection.Abort(exception3);
                        }
                        throw;
                    }
                    catch (StackOverflowException exception2)
                    {
                        this._isClosed = true;
                        if (this._connection != null)
                        {
                            this._connection.Abort(exception2);
                        }
                        throw;
                    }
                    catch (ThreadAbortException exception)
                    {
                        this._isClosed = true;
                        if (this._connection != null)
                        {
                            this._connection.Abort(exception);
                        }
                        throw;
                    }
                }
                return this._metaData;
            }
        }

        public override int RecordsAffected
        {
            get
            {
                if (this._command != null)
                {
                    return this._command.InternalRecordsAffected;
                }
                return this._recordsAffected;
            }
        }

        internal string ResetOptionsString
        {
            set
            {
                this._resetOptionsString = value;
            }
        }

        private SqlStatistics Statistics
        {
            get
            {
                return this._statistics;
            }
        }

        internal MultiPartTableName[] TableNames
        {
            get
            {
                return this._tableNames;
            }
            set
            {
                this._tableNames = value;
            }
        }

        public override int VisibleFieldCount
        {
            get
            {
                if (this.IsClosed)
                {
                    throw ADP.DataReaderClosed("VisibleFieldCount");
                }
                _SqlMetaDataSet metaData = this.MetaData;
                if (metaData == null)
                {
                    return 0;
                }
                return metaData.visibleColumns;
            }
        }

        private enum ALTROWSTATUS
        {
            Null,
            AltRow,
            Done
        }
    }
}

