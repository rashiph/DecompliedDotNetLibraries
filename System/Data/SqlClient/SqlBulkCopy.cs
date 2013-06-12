namespace System.Data.SqlClient
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlTypes;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Xml;

    public sealed class SqlBulkCopy : IDisposable
    {
        private int _batchSize;
        private SqlBulkCopyColumnMappingCollection _columnMappings;
        private SqlConnection _connection;
        private SqlBulkCopyOptions _copyOptions;
        private DataRow _currentRow;
        private int _currentRowLength;
        private string _destinationTableName;
        private SqlTransaction _externalTransaction;
        private bool _insideRowsCopiedEvent;
        private SqlTransaction _internalTransaction;
        private SqlBulkCopyColumnMappingCollection _localColumnMappings;
        private int _notifyAfter;
        internal readonly int _objectID;
        private static int _objectTypeCount;
        private bool _ownConnection;
        private TdsParser _parser;
        private IEnumerator _rowEnumerator;
        private int _rowsCopied;
        private object _rowSource;
        private ValueSourceType _rowSourceType;
        private DataRowState _rowState;
        private int _rowsUntilNotification;
        private ArrayList _sortedColumnMappings;
        private SqlDataReader _SqlDataReaderRowSource;
        private TdsParserStateObject _stateObj;
        private int _timeout;
        private const int ColIdId = 0;
        private const int CollationId = 3;
        private const int CollationResultId = 2;
        private const int DefaultCommandTimeout = 30;
        private const int MetaDataResultId = 1;
        private const int NameId = 1;
        private const int Tds_CollationId = 2;
        private const int TranCountResultId = 0;
        private const int TranCountRowId = 0;
        private const int TranCountValueId = 0;

        public event SqlRowsCopiedEventHandler SqlRowsCopied;

        public SqlBulkCopy(SqlConnection connection)
        {
            this._timeout = 30;
            this._objectID = Interlocked.Increment(ref _objectTypeCount);
            if (connection == null)
            {
                throw ADP.ArgumentNull("connection");
            }
            this._connection = connection;
            this._columnMappings = new SqlBulkCopyColumnMappingCollection();
        }

        public SqlBulkCopy(string connectionString) : this(new SqlConnection(connectionString))
        {
            if (connectionString == null)
            {
                throw ADP.ArgumentNull("connectionString");
            }
            this._connection = new SqlConnection(connectionString);
            this._columnMappings = new SqlBulkCopyColumnMappingCollection();
            this._ownConnection = true;
        }

        public SqlBulkCopy(string connectionString, SqlBulkCopyOptions copyOptions) : this(connectionString)
        {
            this._copyOptions = copyOptions;
        }

        public SqlBulkCopy(SqlConnection connection, SqlBulkCopyOptions copyOptions, SqlTransaction externalTransaction) : this(connection)
        {
            this._copyOptions = copyOptions;
            if ((externalTransaction != null) && this.IsCopyOption(SqlBulkCopyOptions.UseInternalTransaction))
            {
                throw SQL.BulkLoadConflictingTransactionOption();
            }
            if (!this.IsCopyOption(SqlBulkCopyOptions.UseInternalTransaction))
            {
                this._externalTransaction = externalTransaction;
            }
        }

        private string AnalyzeTargetAndCreateUpdateBulkCommand(BulkCopySimpleResultSet internalResults)
        {
            bool hasLocalTransaction;
            this._sortedColumnMappings = new ArrayList();
            StringBuilder query = new StringBuilder();
            if (this._connection.IsShiloh && (internalResults[2].Count == 0))
            {
                throw SQL.BulkLoadNoCollation();
            }
            query.Append("insert bulk " + this.DestinationTableName + " (");
            int num3 = 0;
            int num6 = 0;
            if (this._parser.IsYukonOrNewer)
            {
                hasLocalTransaction = this._connection.HasLocalTransaction;
            }
            else
            {
                hasLocalTransaction = 0 < ((SqlInt32) internalResults[0][0][0]);
            }
            if (((hasLocalTransaction && (this._externalTransaction == null)) && ((this._internalTransaction == null) && (this._connection.Parser != null))) && ((this._connection.Parser.CurrentTransaction != null) && this._connection.Parser.CurrentTransaction.IsLocal))
            {
                throw SQL.BulkLoadExistingTransaction();
            }
            for (int i = 0; i < internalResults[1].MetaData.Length; i++)
            {
                _SqlMetaData metadata = internalResults[1].MetaData[i];
                bool flag2 = false;
                if ((metadata.type == SqlDbType.Timestamp) || (metadata.isIdentity && !this.IsCopyOption(SqlBulkCopyOptions.KeepIdentity)))
                {
                    internalResults[1].MetaData[i] = null;
                    flag2 = true;
                }
                int num = 0;
                while (num < this._localColumnMappings.Count)
                {
                    if ((this._localColumnMappings[num]._destinationColumnOrdinal == metadata.ordinal) || (this.UnquotedName(this._localColumnMappings[num]._destinationColumnName) == metadata.column))
                    {
                        if (flag2)
                        {
                            num6++;
                            break;
                        }
                        this._sortedColumnMappings.Add(new _ColumnMapping(this._localColumnMappings[num]._internalSourceColumnOrdinal, metadata));
                        num3++;
                        if (num3 > 1)
                        {
                            query.Append(", ");
                        }
                        if (metadata.type == SqlDbType.Variant)
                        {
                            this.AppendColumnNameAndTypeName(query, metadata.column, "sql_variant");
                        }
                        else if (metadata.type == SqlDbType.Udt)
                        {
                            this.AppendColumnNameAndTypeName(query, metadata.column, "varbinary");
                        }
                        else
                        {
                            this.AppendColumnNameAndTypeName(query, metadata.column, metadata.type.ToString());
                        }
                        switch (metadata.metaType.NullableType)
                        {
                            case 0x29:
                            case 0x2a:
                            case 0x2b:
                                query.Append("(" + metadata.scale.ToString((IFormatProvider) null) + ")");
                                break;

                            case 0x6a:
                            case 0x6c:
                                query.Append("(" + metadata.precision.ToString((IFormatProvider) null) + "," + metadata.scale.ToString((IFormatProvider) null) + ")");
                                break;

                            case 240:
                                if (metadata.IsLargeUdt)
                                {
                                    query.Append("(max)");
                                }
                                else
                                {
                                    query.Append("(" + metadata.length.ToString((IFormatProvider) null) + ")");
                                }
                                break;

                            default:
                                if (!metadata.metaType.IsFixed && !metadata.metaType.IsLong)
                                {
                                    int length = metadata.length;
                                    switch (metadata.metaType.NullableType)
                                    {
                                        case 0x63:
                                        case 0xe7:
                                        case 0xef:
                                            length /= 2;
                                            break;
                                    }
                                    query.Append("(" + length.ToString((IFormatProvider) null) + ")");
                                }
                                else if (metadata.metaType.IsPlp && (metadata.metaType.SqlDbType != SqlDbType.Xml))
                                {
                                    query.Append("(max)");
                                }
                                break;
                        }
                        if (!this._connection.IsShiloh)
                        {
                            break;
                        }
                        Result result = internalResults[2];
                        object obj2 = result[i][3];
                        if (obj2 == null)
                        {
                            break;
                        }
                        SqlString str = (SqlString) obj2;
                        if (str.IsNull)
                        {
                            break;
                        }
                        query.Append(" COLLATE " + str.ToString());
                        if ((this._SqlDataReaderRowSource == null) || (metadata.collation == null))
                        {
                            break;
                        }
                        int num9 = this._localColumnMappings[num]._internalSourceColumnOrdinal;
                        int lCID = metadata.collation.LCID;
                        int localeId = this._SqlDataReaderRowSource.GetLocaleId(num9);
                        if (localeId == lCID)
                        {
                            break;
                        }
                        throw SQL.BulkLoadLcidMismatch(localeId, this._SqlDataReaderRowSource.GetName(num9), lCID, metadata.column);
                    }
                    num++;
                }
                if (num == this._localColumnMappings.Count)
                {
                    internalResults[1].MetaData[i] = null;
                }
            }
            if ((num3 + num6) != this._localColumnMappings.Count)
            {
                throw SQL.BulkLoadNonMatchingColumnMapping();
            }
            query.Append(")");
            if ((this._copyOptions & (SqlBulkCopyOptions.FireTriggers | SqlBulkCopyOptions.KeepNulls | SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.CheckConstraints)) != SqlBulkCopyOptions.Default)
            {
                bool flag = false;
                query.Append(" with (");
                if (this.IsCopyOption(SqlBulkCopyOptions.KeepNulls))
                {
                    query.Append("KEEP_NULLS");
                    flag = true;
                }
                if (this.IsCopyOption(SqlBulkCopyOptions.TableLock))
                {
                    query.Append((flag ? ", " : "") + "TABLOCK");
                    flag = true;
                }
                if (this.IsCopyOption(SqlBulkCopyOptions.CheckConstraints))
                {
                    query.Append((flag ? ", " : "") + "CHECK_CONSTRAINTS");
                    flag = true;
                }
                if (this.IsCopyOption(SqlBulkCopyOptions.FireTriggers))
                {
                    query.Append((flag ? ", " : "") + "FIRE_TRIGGERS");
                    flag = true;
                }
                query.Append(")");
            }
            return query.ToString();
        }

        private void AppendColumnNameAndTypeName(StringBuilder query, string columnName, string typeName)
        {
            SqlServerEscapeHelper.EscapeIdentifier(query, columnName);
            query.Append(" ");
            query.Append(typeName);
        }

        public void Close()
        {
            if (this._insideRowsCopiedEvent)
            {
                throw SQL.InvalidOperationInsideEvent();
            }
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private object ConvertValue(object value, _SqlMetaData metadata)
        {
            object obj2;
            if (ADP.IsNull(value))
            {
                if (!metadata.isNullable)
                {
                    throw SQL.BulkLoadBulkLoadNotAllowDBNull(metadata.column);
                }
                return value;
            }
            MetaType metaType = metadata.metaType;
            try
            {
                MetaType metaTypeFromSqlDbType;
                SqlDecimal num2;
                switch (metaType.NullableType)
                {
                    case 0x22:
                    case 0x23:
                    case 0x24:
                    case 0x26:
                    case 40:
                    case 0x29:
                    case 0x2a:
                    case 0x2b:
                    case 50:
                    case 0x3a:
                    case 0x3b:
                    case 0x3d:
                    case 0x3e:
                    case 0x68:
                    case 0x6d:
                    case 110:
                    case 0x6f:
                    case 0xa5:
                    case 0xa7:
                    case 0xad:
                    case 0xaf:
                        metaTypeFromSqlDbType = MetaType.GetMetaTypeFromSqlDbType(metaType.SqlDbType, false);
                        value = SqlParameter.CoerceValue(value, metaTypeFromSqlDbType);
                        goto Label_0290;

                    case 0x62:
                        value = this.ValidateBulkCopyVariant(value);
                        goto Label_0290;

                    case 0x63:
                    case 0xef:
                    case 0xe7:
                    {
                        metaTypeFromSqlDbType = MetaType.GetMetaTypeFromSqlDbType(metaType.SqlDbType, false);
                        value = SqlParameter.CoerceValue(value, metaTypeFromSqlDbType);
                        int num3 = (value is string) ? ((string) value).Length : ((SqlString) value).Value.Length;
                        if (num3 > (metadata.length / 2))
                        {
                            throw SQL.BulkLoadStringTooLong();
                        }
                        goto Label_0290;
                    }
                    case 0x6a:
                    case 0x6c:
                        metaTypeFromSqlDbType = MetaType.GetMetaTypeFromSqlDbType(metaType.SqlDbType, false);
                        value = SqlParameter.CoerceValue(value, metaTypeFromSqlDbType);
                        if (!(value is SqlDecimal))
                        {
                            break;
                        }
                        num2 = (SqlDecimal) value;
                        goto Label_017D;

                    case 240:
                        if (value.GetType() != typeof(byte[]))
                        {
                            value = this._connection.GetBytes(value);
                        }
                        goto Label_0290;

                    case 0xf1:
                        if (value is XmlReader)
                        {
                            value = MetaType.GetStringFromXml((XmlReader) value);
                        }
                        goto Label_0290;

                    default:
                        throw SQL.BulkLoadCannotConvertValue(value.GetType(), metadata.metaType, null);
                }
                num2 = new SqlDecimal((decimal) value);
            Label_017D:
                if (num2.Scale != metadata.scale)
                {
                    num2 = TdsParser.AdjustSqlDecimalScale(num2, metadata.scale);
                    value = num2;
                }
                if (num2.Precision > metadata.precision)
                {
                    throw SQL.BulkLoadCannotConvertValue(value.GetType(), metaTypeFromSqlDbType, ADP.ParameterValueOutOfRange(num2));
                }
            Label_0290:
                obj2 = value;
            }
            catch (Exception exception)
            {
                if (!ADP.IsCatchableExceptionType(exception))
                {
                    throw;
                }
                throw SQL.BulkLoadCannotConvertValue(value.GetType(), metadata.metaType, exception);
            }
            return obj2;
        }

        private BulkCopySimpleResultSet CreateAndExecuteInitialQuery()
        {
            string[] strArray;
            try
            {
                strArray = MultipartIdentifier.ParseMultipartIdentifier(this.DestinationTableName, "[\"", "]\"", "SQL_BulkCopyDestinationTableName", true);
            }
            catch (Exception exception)
            {
                throw SQL.BulkLoadInvalidDestinationTable(this.DestinationTableName, exception);
            }
            if (ADP.IsEmpty(strArray[3]))
            {
                throw SQL.BulkLoadInvalidDestinationTable(this.DestinationTableName, null);
            }
            BulkCopySimpleResultSet bulkCopyHandler = new BulkCopySimpleResultSet();
            string str3 = "select @@trancount; SET FMTONLY ON select * from " + this.DestinationTableName + " SET FMTONLY OFF ";
            if (this._connection.IsShiloh)
            {
                string str5;
                if (this._connection.IsKatmaiOrNewer)
                {
                    str5 = "sp_tablecollations_100";
                }
                else if (this._connection.IsYukonOrNewer)
                {
                    str5 = "sp_tablecollations_90";
                }
                else
                {
                    str5 = "sp_tablecollations";
                }
                string str = strArray[3];
                bool flag = (str.Length > 0) && ('#' == str[0]);
                if (!ADP.IsEmpty(str))
                {
                    str = SqlServerEscapeHelper.EscapeIdentifier(SqlServerEscapeHelper.EscapeStringAsLiteral(str));
                }
                string str2 = strArray[2];
                if (!ADP.IsEmpty(str2))
                {
                    str2 = SqlServerEscapeHelper.EscapeIdentifier(SqlServerEscapeHelper.EscapeStringAsLiteral(str2));
                }
                string str4 = strArray[1];
                if (flag && ADP.IsEmpty(str4))
                {
                    str3 = str3 + string.Format(null, "exec tempdb..{0} N'{1}.{2}'", new object[] { str5, str2, str });
                }
                else
                {
                    if (!ADP.IsEmpty(str4))
                    {
                        str4 = SqlServerEscapeHelper.EscapeIdentifier(str4);
                    }
                    str3 = str3 + string.Format(null, "exec {0}..{1} N'{2}.{3}'", new object[] { str4, str5, str2, str });
                }
            }
            Bid.Trace("<sc.SqlBulkCopy.CreateAndExecuteInitialQuery|INFO> Initial Query: '%ls' \n", str3);
            this._parser.TdsExecuteSQLBatch(str3, this.BulkCopyTimeout, null, this._stateObj);
            this._parser.Run(RunBehavior.UntilDone, null, null, bulkCopyHandler, this._stateObj);
            return bulkCopyHandler;
        }

        private void CreateOrValidateConnection(string method)
        {
            if (this._connection == null)
            {
                throw ADP.ConnectionRequired(method);
            }
            if (this._connection.IsContextConnection)
            {
                throw SQL.NotAvailableOnContextConnection();
            }
            if (this._ownConnection && (this._connection.State != ConnectionState.Open))
            {
                this._connection.Open();
            }
            this._connection.ValidateConnectionForExecute(method, null);
            if ((this._externalTransaction != null) && (this._connection != this._externalTransaction.Connection))
            {
                throw ADP.TransactionConnectionMismatch();
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._columnMappings = null;
                this._parser = null;
                try
                {
                    if (this._internalTransaction != null)
                    {
                        this._internalTransaction.Rollback();
                        this._internalTransaction.Dispose();
                        this._internalTransaction = null;
                    }
                }
                catch (Exception exception)
                {
                    if (!ADP.IsCatchableExceptionType(exception))
                    {
                        throw;
                    }
                    ADP.TraceExceptionWithoutRethrow(exception);
                }
                finally
                {
                    if (this._connection != null)
                    {
                        if (this._ownConnection)
                        {
                            this._connection.Dispose();
                        }
                        this._connection = null;
                    }
                }
            }
        }

        private bool FireRowsCopiedEvent(long rowsCopied)
        {
            SqlRowsCopiedEventArgs args = new SqlRowsCopiedEventArgs(rowsCopied);
            try
            {
                this._insideRowsCopiedEvent = true;
                this.OnRowsCopied(args);
            }
            finally
            {
                this._insideRowsCopiedEvent = false;
            }
            return args.Abort;
        }

        private object GetValueFromSourceRow(int columnOrdinal, _SqlMetaData metadata, int[] UseSqlValue, int destRowIndex)
        {
            if (UseSqlValue[destRowIndex] == 0)
            {
                UseSqlValue[destRowIndex] = -1;
                if ((metadata.metaType.NullableType == 0x6a) || (metadata.metaType.NullableType == 0x6c))
                {
                    Type fieldType = null;
                    switch (this._rowSourceType)
                    {
                        case ValueSourceType.IDataReader:
                            if (this._SqlDataReaderRowSource != null)
                            {
                                fieldType = this._SqlDataReaderRowSource.GetFieldType(columnOrdinal);
                            }
                            break;

                        case ValueSourceType.DataTable:
                        case ValueSourceType.RowArray:
                            fieldType = this._currentRow.Table.Columns[columnOrdinal].DataType;
                            break;
                    }
                    if ((typeof(SqlDecimal) == fieldType) || (typeof(decimal) == fieldType))
                    {
                        UseSqlValue[destRowIndex] = 4;
                    }
                    else if ((typeof(SqlDouble) == fieldType) || (typeof(double) == fieldType))
                    {
                        UseSqlValue[destRowIndex] = 5;
                    }
                    else if ((typeof(SqlSingle) == fieldType) || (typeof(float) == fieldType))
                    {
                        UseSqlValue[destRowIndex] = 10;
                    }
                }
            }
            switch (this._rowSourceType)
            {
                case ValueSourceType.IDataReader:
                    if (this._SqlDataReaderRowSource == null)
                    {
                        return ((IDataReader) this._rowSource).GetValue(columnOrdinal);
                    }
                    switch (UseSqlValue[destRowIndex])
                    {
                        case 4:
                            return this._SqlDataReaderRowSource.GetSqlDecimal(columnOrdinal);

                        case 5:
                            return new SqlDecimal(this._SqlDataReaderRowSource.GetSqlDouble(columnOrdinal).Value);

                        case 10:
                            return new SqlDecimal((double) this._SqlDataReaderRowSource.GetSqlSingle(columnOrdinal).Value);
                    }
                    return this._SqlDataReaderRowSource.GetValue(columnOrdinal);

                case ValueSourceType.DataTable:
                case ValueSourceType.RowArray:
                {
                    object obj2 = this._currentRow[columnOrdinal];
                    if (((obj2 == null) || (DBNull.Value == obj2)) || (((10 != UseSqlValue[destRowIndex]) && (5 != UseSqlValue[destRowIndex])) && (4 != UseSqlValue[destRowIndex])))
                    {
                        return obj2;
                    }
                    INullable nullable = obj2 as INullable;
                    if ((nullable != null) && nullable.IsNull)
                    {
                        return obj2;
                    }
                    SqlBuffer.StorageType type2 = (SqlBuffer.StorageType) UseSqlValue[destRowIndex];
                    switch (type2)
                    {
                        case SqlBuffer.StorageType.Decimal:
                            if (nullable == null)
                            {
                                return new SqlDecimal((decimal) obj2);
                            }
                            return (SqlDecimal) obj2;

                        case SqlBuffer.StorageType.Double:
                        {
                            if (nullable == null)
                            {
                                double d = (double) obj2;
                                if (double.IsNaN(d))
                                {
                                    return obj2;
                                }
                                return new SqlDecimal(d);
                            }
                            SqlDouble num4 = (SqlDouble) obj2;
                            return new SqlDecimal(num4.Value);
                        }
                    }
                    if (type2 != SqlBuffer.StorageType.Single)
                    {
                        return obj2;
                    }
                    if (nullable != null)
                    {
                        SqlSingle num5 = (SqlSingle) obj2;
                        return new SqlDecimal((double) num5.Value);
                    }
                    float f = (float) obj2;
                    if (float.IsNaN(f))
                    {
                        return obj2;
                    }
                    return new SqlDecimal((double) f);
                }
            }
            throw ADP.NotSupported();
        }

        private bool IsCopyOption(SqlBulkCopyOptions copyOption)
        {
            return ((this._copyOptions & copyOption) == copyOption);
        }

        private void OnRowsCopied(SqlRowsCopiedEventArgs value)
        {
            SqlRowsCopiedEventHandler handler = this._rowsCopiedEventHandler;
            if (handler != null)
            {
                handler(this, value);
            }
        }

        private bool ReadFromRowSource()
        {
            switch (this._rowSourceType)
            {
                case ValueSourceType.IDataReader:
                    return ((IDataReader) this._rowSource).Read();

                case ValueSourceType.DataTable:
                case ValueSourceType.RowArray:
                    do
                    {
                        if (!this._rowEnumerator.MoveNext())
                        {
                            return false;
                        }
                        this._currentRow = (DataRow) this._rowEnumerator.Current;
                    }
                    while (((this._currentRow.RowState & DataRowState.Deleted) != 0) || ((this._rowState != 0) && ((this._currentRow.RowState & this._rowState) == 0)));
                    this._currentRowLength = this._currentRow.ItemArray.Length;
                    return true;
            }
            throw ADP.NotSupported();
        }

        private void SubmitUpdateBulkCommand(BulkCopySimpleResultSet internalResults, string TDSCommand)
        {
            this._parser.TdsExecuteSQLBatch(TDSCommand, this.BulkCopyTimeout, null, this._stateObj);
            this._parser.Run(RunBehavior.UntilDone, null, null, null, this._stateObj);
        }

        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private string UnquotedName(string name)
        {
            if (ADP.IsEmpty(name))
            {
                return null;
            }
            if (name[0] == '[')
            {
                int length = name.Length;
                name = name.Substring(1, length - 2);
            }
            return name;
        }

        private object ValidateBulkCopyVariant(object value)
        {
            switch (MetaType.GetMetaTypeFromValue(value).TDSType)
            {
                case 0xa5:
                case 0xa7:
                case 0xe7:
                case 0x7f:
                case 0x24:
                case 40:
                case 0x29:
                case 0x2a:
                case 0x2b:
                case 0x30:
                case 50:
                case 0x34:
                case 0x38:
                case 0x3b:
                case 60:
                case 0x3d:
                case 0x3e:
                case 0x6c:
                    if (value is INullable)
                    {
                        return MetaType.GetComValueFromSqlVariant(value);
                    }
                    return value;
            }
            throw SQL.BulkLoadInvalidVariantValue();
        }

        private void WriteMetaData(BulkCopySimpleResultSet internalResults)
        {
            this._stateObj.SetTimeoutSeconds(this.BulkCopyTimeout);
            _SqlMetaDataSet metaData = internalResults[1].MetaData;
            this._stateObj._outputMessageType = 7;
            this._parser.WriteBulkCopyMetaData(metaData, this._sortedColumnMappings.Count, this._stateObj);
        }

        private void WriteRowSourceToServer(int columnCount)
        {
            this.CreateOrValidateConnection("WriteToServer");
            bool flag = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            SNIHandle target = null;
            try
            {
                target = SqlInternalConnection.GetBestEffortCleanupTarget(this._connection);
                this._columnMappings.ReadOnly = true;
                this._localColumnMappings = this._columnMappings;
                if (this._localColumnMappings.Count > 0)
                {
                    this._localColumnMappings.ValidateCollection();
                    foreach (SqlBulkCopyColumnMapping mapping2 in this._localColumnMappings)
                    {
                        if (mapping2._internalSourceColumnOrdinal == -1)
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                else
                {
                    this._localColumnMappings = new SqlBulkCopyColumnMappingCollection();
                    this._localColumnMappings.CreateDefaultMapping(columnCount);
                }
                if (flag)
                {
                    int ordinal = -1;
                    flag = false;
                    if (this._localColumnMappings.Count > 0)
                    {
                        foreach (SqlBulkCopyColumnMapping mapping in this._localColumnMappings)
                        {
                            if (mapping._internalSourceColumnOrdinal != -1)
                            {
                                continue;
                            }
                            string name = this.UnquotedName(mapping.SourceColumn);
                            switch (this._rowSourceType)
                            {
                                case ValueSourceType.IDataReader:
                                    try
                                    {
                                        ordinal = ((IDataRecord) this._rowSource).GetOrdinal(name);
                                    }
                                    catch (IndexOutOfRangeException exception4)
                                    {
                                        throw SQL.BulkLoadNonMatchingColumnName(name, exception4);
                                    }
                                    break;

                                case ValueSourceType.DataTable:
                                    ordinal = ((DataTable) this._rowSource).Columns.IndexOf(name);
                                    break;

                                case ValueSourceType.RowArray:
                                    ordinal = ((DataRow[]) this._rowSource)[0].Table.Columns.IndexOf(name);
                                    break;
                            }
                            if (ordinal == -1)
                            {
                                throw SQL.BulkLoadNonMatchingColumnName(name);
                            }
                            mapping._internalSourceColumnOrdinal = ordinal;
                        }
                    }
                }
                this.WriteToServerInternal();
            }
            catch (OutOfMemoryException exception3)
            {
                this._connection.Abort(exception3);
                throw;
            }
            catch (StackOverflowException exception2)
            {
                this._connection.Abort(exception2);
                throw;
            }
            catch (ThreadAbortException exception)
            {
                this._connection.Abort(exception);
                SqlInternalConnection.BestEffortCleanup(target);
                throw;
            }
            finally
            {
                this._columnMappings.ReadOnly = false;
            }
        }

        public void WriteToServer(DataTable table)
        {
            this.WriteToServer(table, 0);
        }

        public void WriteToServer(IDataReader reader)
        {
            SqlConnection.ExecutePermission.Demand();
            SqlStatistics statistics = this.Statistics;
            try
            {
                statistics = SqlStatistics.StartTimer(this.Statistics);
                if (reader == null)
                {
                    throw new ArgumentNullException("reader");
                }
                this._rowSource = reader;
                this._SqlDataReaderRowSource = this._rowSource as SqlDataReader;
                this._rowSourceType = ValueSourceType.IDataReader;
                this.WriteRowSourceToServer(reader.FieldCount);
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
            }
        }

        public void WriteToServer(DataRow[] rows)
        {
            SqlConnection.ExecutePermission.Demand();
            SqlStatistics statistics = this.Statistics;
            try
            {
                statistics = SqlStatistics.StartTimer(this.Statistics);
                if (rows == null)
                {
                    throw new ArgumentNullException("rows");
                }
                if (rows.Length != 0)
                {
                    DataTable table = rows[0].Table;
                    this._rowState = 0;
                    this._rowSource = rows;
                    this._SqlDataReaderRowSource = null;
                    this._rowSourceType = ValueSourceType.RowArray;
                    this._rowEnumerator = rows.GetEnumerator();
                    this.WriteRowSourceToServer(table.Columns.Count);
                }
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
            }
        }

        public void WriteToServer(DataTable table, DataRowState rowState)
        {
            SqlConnection.ExecutePermission.Demand();
            SqlStatistics statistics = this.Statistics;
            try
            {
                statistics = SqlStatistics.StartTimer(this.Statistics);
                if (table == null)
                {
                    throw new ArgumentNullException("table");
                }
                this._rowState = rowState & ~DataRowState.Deleted;
                this._rowSource = table;
                this._SqlDataReaderRowSource = null;
                this._rowSourceType = ValueSourceType.DataTable;
                this._rowEnumerator = table.Rows.GetEnumerator();
                this.WriteRowSourceToServer(table.Columns.Count);
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
            }
        }

        private void WriteToServerInternal()
        {
            string tDSCommand = null;
            bool flag3 = false;
            bool flag2 = false;
            int[] useSqlValue = null;
            int num5 = this._batchSize;
            bool flag4 = false;
            if (this._batchSize > 0)
            {
                flag4 = true;
            }
            Exception inner = null;
            this._rowsCopied = 0;
            if (this._destinationTableName == null)
            {
                throw SQL.BulkLoadMissingDestinationTable();
            }
            if (this.ReadFromRowSource())
            {
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    bool flag = true;
                    this._parser = this._connection.Parser;
                    this._stateObj = this._parser.GetSession(this);
                    this._stateObj._bulkCopyOpperationInProgress = true;
                    try
                    {
                        BulkCopySimpleResultSet set;
                        this._stateObj.StartSession(this.ObjectID);
                        try
                        {
                            set = this.CreateAndExecuteInitialQuery();
                        }
                        catch (SqlException exception5)
                        {
                            throw SQL.BulkLoadInvalidDestinationTable(this._destinationTableName, exception5);
                        }
                        this._rowsUntilNotification = this._notifyAfter;
                        tDSCommand = this.AnalyzeTargetAndCreateUpdateBulkCommand(set);
                        if (this._sortedColumnMappings.Count == 0)
                        {
                            return;
                        }
                        this._stateObj.SniContext = SniContext.Snix_SendRows;
                    Label_00DD:
                        if (this.IsCopyOption(SqlBulkCopyOptions.UseInternalTransaction))
                        {
                            this._internalTransaction = this._connection.BeginTransaction();
                        }
                        this.SubmitUpdateBulkCommand(set, tDSCommand);
                        try
                        {
                            this.WriteMetaData(set);
                            object[] objArray = new object[this._sortedColumnMappings.Count];
                            if (useSqlValue == null)
                            {
                                useSqlValue = new int[objArray.Length];
                            }
                            int num3 = num5;
                            do
                            {
                                for (int i = 0; i < objArray.Length; i++)
                                {
                                    _ColumnMapping mapping = (_ColumnMapping) this._sortedColumnMappings[i];
                                    _SqlMetaData metadata = mapping._metadata;
                                    object obj2 = this.GetValueFromSourceRow(mapping._sourceColumnOrdinal, metadata, useSqlValue, i);
                                    objArray[i] = this.ConvertValue(obj2, metadata);
                                }
                                this._parser.WriteByte(0xd1, this._stateObj);
                                for (int j = 0; j < objArray.Length; j++)
                                {
                                    _ColumnMapping mapping2 = (_ColumnMapping) this._sortedColumnMappings[j];
                                    _SqlMetaData data = mapping2._metadata;
                                    if (data.type != SqlDbType.Variant)
                                    {
                                        this._parser.WriteBulkCopyValue(objArray[j], data, this._stateObj);
                                    }
                                    else
                                    {
                                        this._parser.WriteSqlVariantDataRowValue(objArray[j], this._stateObj);
                                    }
                                }
                                this._rowsCopied++;
                                if (((this._notifyAfter > 0) && (this._rowsUntilNotification > 0)) && (--this._rowsUntilNotification == 0))
                                {
                                    try
                                    {
                                        this._stateObj.BcpLock = true;
                                        flag2 = this.FireRowsCopiedEvent((long) this._rowsCopied);
                                        Bid.Trace("<sc.SqlBulkCopy.WriteToServerInternal|INFO> \n");
                                        if (ConnectionState.Open != this._connection.State)
                                        {
                                            goto Label_02F7;
                                        }
                                    }
                                    catch (Exception exception2)
                                    {
                                        if (!ADP.IsCatchableExceptionType(exception2))
                                        {
                                            throw;
                                        }
                                        inner = OperationAbortedException.Aborted(exception2);
                                        goto Label_02F7;
                                    }
                                    finally
                                    {
                                        this._stateObj.BcpLock = false;
                                    }
                                    if (flag2)
                                    {
                                        goto Label_02F7;
                                    }
                                    this._rowsUntilNotification = this._notifyAfter;
                                }
                                if (this._rowsUntilNotification > this._notifyAfter)
                                {
                                    this._rowsUntilNotification = this._notifyAfter;
                                }
                                flag3 = this.ReadFromRowSource();
                                if (flag4)
                                {
                                    num3--;
                                    if (num3 == 0)
                                    {
                                        goto Label_02F7;
                                    }
                                }
                            }
                            while (flag3);
                        }
                        catch (NullReferenceException)
                        {
                            this._stateObj.CancelRequest();
                            throw;
                        }
                        catch (Exception exception4)
                        {
                            if (ADP.IsCatchableExceptionType(exception4))
                            {
                                this._stateObj.CancelRequest();
                            }
                            throw;
                        }
                    Label_02F7:
                        if (ConnectionState.Open != this._connection.State)
                        {
                            throw ADP.OpenConnectionRequired("WriteToServer", this._connection.State);
                        }
                        this._parser.WriteBulkCopyDone(this._stateObj);
                        this._parser.Run(RunBehavior.UntilDone, null, null, null, this._stateObj);
                        if (flag2 || (inner != null))
                        {
                            throw OperationAbortedException.Aborted(inner);
                        }
                        if (this._internalTransaction != null)
                        {
                            this._internalTransaction.Commit();
                            this._internalTransaction = null;
                        }
                        if (flag3)
                        {
                            goto Label_00DD;
                        }
                        this._localColumnMappings = null;
                    }
                    catch (Exception exception3)
                    {
                        flag = ADP.IsCatchableExceptionType(exception3);
                        if (flag)
                        {
                            this._stateObj._internalTimeout = false;
                            if (this._internalTransaction != null)
                            {
                                if (!this._internalTransaction.IsZombied)
                                {
                                    this._internalTransaction.Rollback();
                                }
                                this._internalTransaction = null;
                            }
                        }
                        throw;
                    }
                    finally
                    {
                        if (flag && (this._stateObj != null))
                        {
                            this._stateObj.CloseSession();
                        }
                    }
                }
                finally
                {
                    if (this._stateObj != null)
                    {
                        this._stateObj._bulkCopyOpperationInProgress = false;
                        this._stateObj = null;
                    }
                }
            }
        }

        public int BatchSize
        {
            get
            {
                return this._batchSize;
            }
            set
            {
                if (value < 0)
                {
                    throw ADP.ArgumentOutOfRange("BatchSize");
                }
                this._batchSize = value;
            }
        }

        public int BulkCopyTimeout
        {
            get
            {
                return this._timeout;
            }
            set
            {
                if (value < 0)
                {
                    throw SQL.BulkLoadInvalidTimeout(value);
                }
                this._timeout = value;
            }
        }

        public SqlBulkCopyColumnMappingCollection ColumnMappings
        {
            get
            {
                return this._columnMappings;
            }
        }

        public string DestinationTableName
        {
            get
            {
                return this._destinationTableName;
            }
            set
            {
                if (value == null)
                {
                    throw ADP.ArgumentNull("DestinationTableName");
                }
                if (value.Length == 0)
                {
                    throw ADP.ArgumentOutOfRange("DestinationTableName");
                }
                this._destinationTableName = value;
            }
        }

        public int NotifyAfter
        {
            get
            {
                return this._notifyAfter;
            }
            set
            {
                if (value < 0)
                {
                    throw ADP.ArgumentOutOfRange("NotifyAfter");
                }
                this._notifyAfter = value;
            }
        }

        internal int ObjectID
        {
            get
            {
                return this._objectID;
            }
        }

        internal SqlStatistics Statistics
        {
            get
            {
                if ((this._connection != null) && this._connection.StatisticsEnabled)
                {
                    return this._connection.Statistics;
                }
                return null;
            }
        }

        private enum ValueSourceType
        {
            Unspecified,
            IDataReader,
            DataTable,
            RowArray
        }
    }
}

