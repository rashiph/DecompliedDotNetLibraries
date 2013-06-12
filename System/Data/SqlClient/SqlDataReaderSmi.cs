namespace System.Data.SqlClient
{
    using Microsoft.SqlServer.Server;
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Data.SqlTypes;
    using System.Globalization;
    using System.Reflection;

    internal sealed class SqlDataReaderSmi : SqlDataReader
    {
        private ITypedGetters _currentColumnValues;
        private ITypedGettersV3 _currentColumnValuesV3;
        private SqlInternalConnectionSmi _currentConnection;
        private SmiQueryMetaData[] _currentMetaData;
        private PositionState _currentPosition;
        private SmiEventStream _eventStream;
        private FieldNameLookup _fieldNameLookup;
        private bool _hasRows;
        private int[] _indexMap;
        private bool _isOpen;
        private ReaderEventSink _readerEventSink;
        private DataTable _schemaTable;
        private int _visibleColumnCount;

        internal SqlDataReaderSmi(SmiEventStream eventStream, SqlCommand parent, CommandBehavior behavior, SqlInternalConnectionSmi connection, SmiEventSink parentSink) : base(parent, behavior)
        {
            this._eventStream = eventStream;
            this._currentConnection = connection;
            this._readerEventSink = new ReaderEventSink(this, parentSink);
            this._currentPosition = PositionState.BeforeResults;
            this._isOpen = true;
            this._indexMap = null;
            this._visibleColumnCount = 0;
        }

        private void BatchCompleted()
        {
            this._currentPosition = PositionState.AfterResults;
            this._eventStream.Close(this._readerEventSink);
        }

        public override void Close()
        {
            IntPtr ptr;
            bool flag2 = base.IsCommandBehavior(CommandBehavior.CloseConnection);
            Bid.ScopeEnter(out ptr, "<sc.SqlDataReaderSmi.Close|API> %d#", base.ObjectID);
            bool flag = true;
            try
            {
                if (!this.IsClosed)
                {
                    this._hasRows = false;
                    while (this._eventStream.HasEvents)
                    {
                        this._eventStream.ProcessEvent(this._readerEventSink);
                        this._readerEventSink.ProcessMessagesAndThrow(true);
                    }
                }
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
                    this._isOpen = false;
                    if (flag2)
                    {
                        if (base.Connection != null)
                        {
                            base.Connection.Close();
                        }
                        Bid.ScopeLeave(ref ptr);
                    }
                }
            }
        }

        private void EnsureCanGetCol(string operationName, int ordinal)
        {
            this.EnsureOnRow(operationName);
        }

        internal void EnsureCanGetMetaData(string operationName)
        {
            this.ThrowIfClosed(operationName);
            if (this.FNotInResults())
            {
                throw SQL.InvalidRead();
            }
        }

        internal void EnsureOnRow(string operationName)
        {
            this.ThrowIfClosed(operationName);
            if (this._currentPosition != PositionState.OnRow)
            {
                throw SQL.InvalidRead();
            }
        }

        private bool FInResults()
        {
            return !this.FNotInResults();
        }

        private bool FNotInResults()
        {
            if (PositionState.AfterResults != this._currentPosition)
            {
                return (PositionState.BeforeResults == this._currentPosition);
            }
            return true;
        }

        public override bool GetBoolean(int ordinal)
        {
            this.EnsureCanGetCol("GetBoolean", ordinal);
            return ValueUtilsSmi.GetBoolean(this._readerEventSink, this._currentColumnValuesV3, ordinal, this._currentMetaData[ordinal]);
        }

        public override byte GetByte(int ordinal)
        {
            this.EnsureCanGetCol("GetByte", ordinal);
            return ValueUtilsSmi.GetByte(this._readerEventSink, this._currentColumnValuesV3, ordinal, this._currentMetaData[ordinal]);
        }

        public override long GetBytes(int ordinal, long fieldOffset, byte[] buffer, int bufferOffset, int length)
        {
            this.EnsureCanGetCol("GetBytes", ordinal);
            return ValueUtilsSmi.GetBytes(this._readerEventSink, this._currentColumnValuesV3, ordinal, this._currentMetaData[ordinal], fieldOffset, buffer, bufferOffset, length, true);
        }

        internal override long GetBytesInternal(int ordinal, long fieldOffset, byte[] buffer, int bufferOffset, int length)
        {
            this.EnsureCanGetCol("GetBytes", ordinal);
            return ValueUtilsSmi.GetBytesInternal(this._readerEventSink, this._currentColumnValuesV3, ordinal, this._currentMetaData[ordinal], fieldOffset, buffer, bufferOffset, length, false);
        }

        public override char GetChar(int ordinal)
        {
            throw ADP.NotSupported();
        }

        public override long GetChars(int ordinal, long fieldOffset, char[] buffer, int bufferOffset, int length)
        {
            this.EnsureCanGetCol("GetChars", ordinal);
            SmiExtendedMetaData metaData = this._currentMetaData[ordinal];
            if (base.IsCommandBehavior(CommandBehavior.SequentialAccess) && (metaData.SqlDbType == SqlDbType.Xml))
            {
                return base.GetStreamingXmlChars(ordinal, fieldOffset, buffer, bufferOffset, length);
            }
            return ValueUtilsSmi.GetChars(this._readerEventSink, this._currentColumnValuesV3, ordinal, metaData, fieldOffset, buffer, bufferOffset, length);
        }

        public override string GetDataTypeName(int ordinal)
        {
            this.EnsureCanGetMetaData("GetDataTypeName");
            SmiExtendedMetaData data = this._currentMetaData[ordinal];
            if (SqlDbType.Udt == data.SqlDbType)
            {
                return (data.TypeSpecificNamePart1 + "." + data.TypeSpecificNamePart2 + "." + data.TypeSpecificNamePart3);
            }
            return data.TypeName;
        }

        public override DateTime GetDateTime(int ordinal)
        {
            this.EnsureCanGetCol("GetDateTime", ordinal);
            return ValueUtilsSmi.GetDateTime(this._readerEventSink, this._currentColumnValuesV3, ordinal, this._currentMetaData[ordinal]);
        }

        public override DateTimeOffset GetDateTimeOffset(int ordinal)
        {
            this.EnsureCanGetCol("GetDateTimeOffset", ordinal);
            return ValueUtilsSmi.GetDateTimeOffset(this._readerEventSink, this._currentColumnValuesV3, ordinal, this._currentMetaData[ordinal], this._currentConnection.IsKatmaiOrNewer);
        }

        public override decimal GetDecimal(int ordinal)
        {
            this.EnsureCanGetCol("GetDecimal", ordinal);
            return ValueUtilsSmi.GetDecimal(this._readerEventSink, this._currentColumnValuesV3, ordinal, this._currentMetaData[ordinal]);
        }

        public override double GetDouble(int ordinal)
        {
            this.EnsureCanGetCol("GetDouble", ordinal);
            return ValueUtilsSmi.GetDouble(this._readerEventSink, this._currentColumnValuesV3, ordinal, this._currentMetaData[ordinal]);
        }

        public override Type GetFieldType(int ordinal)
        {
            this.EnsureCanGetMetaData("GetFieldType");
            if (SqlDbType.Udt == this._currentMetaData[ordinal].SqlDbType)
            {
                return this._currentMetaData[ordinal].Type;
            }
            return MetaType.GetMetaTypeFromSqlDbType(this._currentMetaData[ordinal].SqlDbType, this._currentMetaData[ordinal].IsMultiValued).ClassType;
        }

        public override float GetFloat(int ordinal)
        {
            this.EnsureCanGetCol("GetFloat", ordinal);
            return ValueUtilsSmi.GetSingle(this._readerEventSink, this._currentColumnValuesV3, ordinal, this._currentMetaData[ordinal]);
        }

        public override Guid GetGuid(int ordinal)
        {
            this.EnsureCanGetCol("GetGuid", ordinal);
            return ValueUtilsSmi.GetGuid(this._readerEventSink, this._currentColumnValuesV3, ordinal, this._currentMetaData[ordinal]);
        }

        public override short GetInt16(int ordinal)
        {
            this.EnsureCanGetCol("GetInt16", ordinal);
            return ValueUtilsSmi.GetInt16(this._readerEventSink, this._currentColumnValuesV3, ordinal, this._currentMetaData[ordinal]);
        }

        public override int GetInt32(int ordinal)
        {
            this.EnsureCanGetCol("GetInt32", ordinal);
            return ValueUtilsSmi.GetInt32(this._readerEventSink, this._currentColumnValuesV3, ordinal, this._currentMetaData[ordinal]);
        }

        public override long GetInt64(int ordinal)
        {
            this.EnsureCanGetCol("GetInt64", ordinal);
            return ValueUtilsSmi.GetInt64(this._readerEventSink, this._currentColumnValuesV3, ordinal, this._currentMetaData[ordinal]);
        }

        internal override SmiExtendedMetaData[] GetInternalSmiMetaData()
        {
            if ((this._currentMetaData == null) || (this._visibleColumnCount == this.InternalFieldCount))
            {
                return this._currentMetaData;
            }
            SmiExtendedMetaData[] dataArray = new SmiExtendedMetaData[this._visibleColumnCount];
            for (int i = 0; i < this._visibleColumnCount; i++)
            {
                dataArray[i] = this._currentMetaData[this._indexMap[i]];
            }
            return dataArray;
        }

        internal override int GetLocaleId(int ordinal)
        {
            this.EnsureCanGetMetaData("GetLocaleId");
            return (int) this._currentMetaData[ordinal].LocaleId;
        }

        public override string GetName(int ordinal)
        {
            this.EnsureCanGetMetaData("GetName");
            return this._currentMetaData[ordinal].Name;
        }

        public override int GetOrdinal(string name)
        {
            this.EnsureCanGetMetaData("GetOrdinal");
            if (this._fieldNameLookup == null)
            {
                this._fieldNameLookup = new FieldNameLookup(this, -1);
            }
            return this._fieldNameLookup.GetOrdinal(name);
        }

        public override Type GetProviderSpecificFieldType(int ordinal)
        {
            this.EnsureCanGetMetaData("GetProviderSpecificFieldType");
            if (SqlDbType.Udt == this._currentMetaData[ordinal].SqlDbType)
            {
                return this._currentMetaData[ordinal].Type;
            }
            return MetaType.GetMetaTypeFromSqlDbType(this._currentMetaData[ordinal].SqlDbType, this._currentMetaData[ordinal].IsMultiValued).SqlType;
        }

        public override DataTable GetSchemaTable()
        {
            this.ThrowIfClosed("GetSchemaTable");
            if ((this._schemaTable == null) && this.FInResults())
            {
                DataTable table = new DataTable("SchemaTable") {
                    Locale = CultureInfo.InvariantCulture,
                    MinimumCapacity = this.InternalFieldCount
                };
                DataColumn column31 = new DataColumn(SchemaTableColumn.ColumnName, typeof(string));
                DataColumn column9 = new DataColumn(SchemaTableColumn.ColumnOrdinal, typeof(int));
                DataColumn column30 = new DataColumn(SchemaTableColumn.ColumnSize, typeof(int));
                DataColumn column29 = new DataColumn(SchemaTableColumn.NumericPrecision, typeof(short));
                DataColumn column8 = new DataColumn(SchemaTableColumn.NumericScale, typeof(short));
                DataColumn column7 = new DataColumn(SchemaTableColumn.DataType, typeof(Type));
                DataColumn column6 = new DataColumn(SchemaTableOptionalColumn.ProviderSpecificDataType, typeof(Type));
                DataColumn column28 = new DataColumn(SchemaTableColumn.ProviderType, typeof(int));
                DataColumn column27 = new DataColumn(SchemaTableColumn.NonVersionedProviderType, typeof(int));
                DataColumn column5 = new DataColumn(SchemaTableColumn.IsLong, typeof(bool));
                DataColumn column26 = new DataColumn(SchemaTableColumn.AllowDBNull, typeof(bool));
                DataColumn column25 = new DataColumn(SchemaTableOptionalColumn.IsReadOnly, typeof(bool));
                DataColumn column4 = new DataColumn(SchemaTableOptionalColumn.IsRowVersion, typeof(bool));
                DataColumn column3 = new DataColumn(SchemaTableColumn.IsUnique, typeof(bool));
                DataColumn column24 = new DataColumn(SchemaTableColumn.IsKey, typeof(bool));
                DataColumn column23 = new DataColumn(SchemaTableOptionalColumn.IsAutoIncrement, typeof(bool));
                DataColumn column22 = new DataColumn(SchemaTableOptionalColumn.IsHidden, typeof(bool));
                DataColumn column21 = new DataColumn(SchemaTableOptionalColumn.BaseCatalogName, typeof(string));
                DataColumn column20 = new DataColumn(SchemaTableColumn.BaseSchemaName, typeof(string));
                DataColumn column19 = new DataColumn(SchemaTableColumn.BaseTableName, typeof(string));
                DataColumn column2 = new DataColumn(SchemaTableColumn.BaseColumnName, typeof(string));
                DataColumn column18 = new DataColumn(SchemaTableOptionalColumn.BaseServerName, typeof(string));
                DataColumn column17 = new DataColumn(SchemaTableColumn.IsAliased, typeof(bool));
                DataColumn column16 = new DataColumn(SchemaTableColumn.IsExpression, typeof(bool));
                DataColumn column15 = new DataColumn("IsIdentity", typeof(bool));
                DataColumn column = new DataColumn("DataTypeName", typeof(string));
                DataColumn column14 = new DataColumn("UdtAssemblyQualifiedName", typeof(string));
                DataColumn column13 = new DataColumn("XmlSchemaCollectionDatabase", typeof(string));
                DataColumn column12 = new DataColumn("XmlSchemaCollectionOwningSchema", typeof(string));
                DataColumn column11 = new DataColumn("XmlSchemaCollectionName", typeof(string));
                DataColumn column10 = new DataColumn("IsColumnSet", typeof(bool));
                column9.DefaultValue = 0;
                column5.DefaultValue = false;
                DataColumnCollection columns = table.Columns;
                columns.Add(column31);
                columns.Add(column9);
                columns.Add(column30);
                columns.Add(column29);
                columns.Add(column8);
                columns.Add(column3);
                columns.Add(column24);
                columns.Add(column18);
                columns.Add(column21);
                columns.Add(column2);
                columns.Add(column20);
                columns.Add(column19);
                columns.Add(column7);
                columns.Add(column26);
                columns.Add(column28);
                columns.Add(column17);
                columns.Add(column16);
                columns.Add(column15);
                columns.Add(column23);
                columns.Add(column4);
                columns.Add(column22);
                columns.Add(column5);
                columns.Add(column25);
                columns.Add(column6);
                columns.Add(column);
                columns.Add(column13);
                columns.Add(column12);
                columns.Add(column11);
                columns.Add(column14);
                columns.Add(column27);
                columns.Add(column10);
                for (int i = 0; i < this.InternalFieldCount; i++)
                {
                    SmiQueryMetaData data = this._currentMetaData[i];
                    long maxLength = data.MaxLength;
                    MetaType metaTypeFromSqlDbType = MetaType.GetMetaTypeFromSqlDbType(data.SqlDbType, data.IsMultiValued);
                    if (-1L == maxLength)
                    {
                        metaTypeFromSqlDbType = MetaType.GetMaxMetaTypeFromMetaType(metaTypeFromSqlDbType);
                        maxLength = (metaTypeFromSqlDbType.IsSizeInCharacters && !metaTypeFromSqlDbType.IsPlp) ? ((long) 0x3fffffff) : ((long) 0x7fffffff);
                    }
                    DataRow row = table.NewRow();
                    if (SqlDbType.Decimal == data.SqlDbType)
                    {
                        maxLength = 0x11L;
                    }
                    else if (SqlDbType.Variant == data.SqlDbType)
                    {
                        maxLength = 0x1f49L;
                    }
                    row[column31] = data.Name;
                    row[column9] = i;
                    row[column30] = maxLength;
                    row[column28] = (int) data.SqlDbType;
                    row[column27] = (int) data.SqlDbType;
                    if (data.SqlDbType != SqlDbType.Udt)
                    {
                        row[column7] = metaTypeFromSqlDbType.ClassType;
                        row[column6] = metaTypeFromSqlDbType.SqlType;
                    }
                    else
                    {
                        row[column14] = data.Type.AssemblyQualifiedName;
                        row[column7] = data.Type;
                        row[column6] = data.Type;
                    }
                    byte precision = 0xff;
                    switch (data.SqlDbType)
                    {
                        case SqlDbType.BigInt:
                        case SqlDbType.DateTime:
                        case SqlDbType.Decimal:
                        case SqlDbType.Int:
                        case SqlDbType.Money:
                        case SqlDbType.SmallDateTime:
                        case SqlDbType.SmallInt:
                        case SqlDbType.SmallMoney:
                        case SqlDbType.TinyInt:
                            precision = data.Precision;
                            break;

                        case SqlDbType.Float:
                            precision = 15;
                            break;

                        case SqlDbType.Real:
                            precision = 7;
                            break;

                        default:
                            precision = 0xff;
                            break;
                    }
                    row[column29] = precision;
                    if (((SqlDbType.Decimal == data.SqlDbType) || (SqlDbType.Time == data.SqlDbType)) || ((SqlDbType.DateTime2 == data.SqlDbType) || (SqlDbType.DateTimeOffset == data.SqlDbType)))
                    {
                        row[column8] = data.Scale;
                    }
                    else
                    {
                        row[column8] = MetaType.GetMetaTypeFromSqlDbType(data.SqlDbType, data.IsMultiValued).Scale;
                    }
                    row[column26] = data.AllowsDBNull;
                    if (!data.IsAliased.IsNull)
                    {
                        row[column17] = data.IsAliased.Value;
                    }
                    if (!data.IsKey.IsNull)
                    {
                        row[column24] = data.IsKey.Value;
                    }
                    if (!data.IsHidden.IsNull)
                    {
                        row[column22] = data.IsHidden.Value;
                    }
                    if (!data.IsExpression.IsNull)
                    {
                        row[column16] = data.IsExpression.Value;
                    }
                    row[column25] = data.IsReadOnly;
                    row[column15] = data.IsIdentity;
                    row[column10] = data.IsColumnSet;
                    row[column23] = data.IsIdentity;
                    row[column5] = metaTypeFromSqlDbType.IsLong;
                    if (SqlDbType.Timestamp == data.SqlDbType)
                    {
                        row[column3] = true;
                        row[column4] = true;
                    }
                    else
                    {
                        row[column3] = false;
                        row[column4] = false;
                    }
                    if (!ADP.IsEmpty(data.ColumnName))
                    {
                        row[column2] = data.ColumnName;
                    }
                    else if (!ADP.IsEmpty(data.Name))
                    {
                        row[column2] = data.Name;
                    }
                    if (!ADP.IsEmpty(data.TableName))
                    {
                        row[column19] = data.TableName;
                    }
                    if (!ADP.IsEmpty(data.SchemaName))
                    {
                        row[column20] = data.SchemaName;
                    }
                    if (!ADP.IsEmpty(data.CatalogName))
                    {
                        row[column21] = data.CatalogName;
                    }
                    if (!ADP.IsEmpty(data.ServerName))
                    {
                        row[column18] = data.ServerName;
                    }
                    if (SqlDbType.Udt == data.SqlDbType)
                    {
                        row[column] = data.TypeSpecificNamePart1 + "." + data.TypeSpecificNamePart2 + "." + data.TypeSpecificNamePart3;
                    }
                    else
                    {
                        row[column] = metaTypeFromSqlDbType.TypeName;
                    }
                    if (SqlDbType.Xml == data.SqlDbType)
                    {
                        row[column13] = data.TypeSpecificNamePart1;
                        row[column12] = data.TypeSpecificNamePart2;
                        row[column11] = data.TypeSpecificNamePart3;
                    }
                    table.Rows.Add(row);
                    row.AcceptChanges();
                }
                foreach (DataColumn column32 in columns)
                {
                    column32.ReadOnly = true;
                }
                this._schemaTable = table;
            }
            return this._schemaTable;
        }

        public override SqlBinary GetSqlBinary(int ordinal)
        {
            this.EnsureCanGetCol("GetSqlBinary", ordinal);
            return ValueUtilsSmi.GetSqlBinary(this._readerEventSink, this._currentColumnValuesV3, ordinal, this._currentMetaData[ordinal]);
        }

        public override SqlBoolean GetSqlBoolean(int ordinal)
        {
            this.EnsureCanGetCol("GetSqlBoolean", ordinal);
            return ValueUtilsSmi.GetSqlBoolean(this._readerEventSink, this._currentColumnValuesV3, ordinal, this._currentMetaData[ordinal]);
        }

        public override SqlByte GetSqlByte(int ordinal)
        {
            this.EnsureCanGetCol("GetSqlByte", ordinal);
            return ValueUtilsSmi.GetSqlByte(this._readerEventSink, this._currentColumnValuesV3, ordinal, this._currentMetaData[ordinal]);
        }

        public override SqlBytes GetSqlBytes(int ordinal)
        {
            this.EnsureCanGetCol("GetSqlBytes", ordinal);
            return ValueUtilsSmi.GetSqlBytes(this._readerEventSink, this._currentColumnValuesV3, ordinal, this._currentMetaData[ordinal], this._currentConnection.InternalContext);
        }

        public override SqlChars GetSqlChars(int ordinal)
        {
            this.EnsureCanGetCol("GetSqlChars", ordinal);
            return ValueUtilsSmi.GetSqlChars(this._readerEventSink, this._currentColumnValuesV3, ordinal, this._currentMetaData[ordinal], this._currentConnection.InternalContext);
        }

        public override SqlDateTime GetSqlDateTime(int ordinal)
        {
            this.EnsureCanGetCol("GetSqlDateTime", ordinal);
            return ValueUtilsSmi.GetSqlDateTime(this._readerEventSink, this._currentColumnValuesV3, ordinal, this._currentMetaData[ordinal]);
        }

        public override SqlDecimal GetSqlDecimal(int ordinal)
        {
            this.EnsureCanGetCol("GetSqlDecimal", ordinal);
            return ValueUtilsSmi.GetSqlDecimal(this._readerEventSink, this._currentColumnValuesV3, ordinal, this._currentMetaData[ordinal]);
        }

        public override SqlDouble GetSqlDouble(int ordinal)
        {
            this.EnsureCanGetCol("GetSqlDouble", ordinal);
            return ValueUtilsSmi.GetSqlDouble(this._readerEventSink, this._currentColumnValuesV3, ordinal, this._currentMetaData[ordinal]);
        }

        public override SqlGuid GetSqlGuid(int ordinal)
        {
            this.EnsureCanGetCol("GetSqlGuid", ordinal);
            return ValueUtilsSmi.GetSqlGuid(this._readerEventSink, this._currentColumnValuesV3, ordinal, this._currentMetaData[ordinal]);
        }

        public override SqlInt16 GetSqlInt16(int ordinal)
        {
            this.EnsureCanGetCol("GetSqlInt16", ordinal);
            return ValueUtilsSmi.GetSqlInt16(this._readerEventSink, this._currentColumnValuesV3, ordinal, this._currentMetaData[ordinal]);
        }

        public override SqlInt32 GetSqlInt32(int ordinal)
        {
            this.EnsureCanGetCol("GetSqlInt32", ordinal);
            return ValueUtilsSmi.GetSqlInt32(this._readerEventSink, this._currentColumnValuesV3, ordinal, this._currentMetaData[ordinal]);
        }

        public override SqlInt64 GetSqlInt64(int ordinal)
        {
            this.EnsureCanGetCol("GetSqlInt64", ordinal);
            return ValueUtilsSmi.GetSqlInt64(this._readerEventSink, this._currentColumnValuesV3, ordinal, this._currentMetaData[ordinal]);
        }

        public override SqlMoney GetSqlMoney(int ordinal)
        {
            this.EnsureCanGetCol("GetSqlMoney", ordinal);
            return ValueUtilsSmi.GetSqlMoney(this._readerEventSink, this._currentColumnValuesV3, ordinal, this._currentMetaData[ordinal]);
        }

        public override SqlSingle GetSqlSingle(int ordinal)
        {
            this.EnsureCanGetCol("GetSqlSingle", ordinal);
            return ValueUtilsSmi.GetSqlSingle(this._readerEventSink, this._currentColumnValuesV3, ordinal, this._currentMetaData[ordinal]);
        }

        public override SqlString GetSqlString(int ordinal)
        {
            this.EnsureCanGetCol("GetSqlString", ordinal);
            return ValueUtilsSmi.GetSqlString(this._readerEventSink, this._currentColumnValuesV3, ordinal, this._currentMetaData[ordinal]);
        }

        public override object GetSqlValue(int ordinal)
        {
            this.EnsureCanGetCol("GetSqlValue", ordinal);
            SmiMetaData metaData = this._currentMetaData[ordinal];
            if (this._currentConnection.IsKatmaiOrNewer)
            {
                return ValueUtilsSmi.GetSqlValue200(this._readerEventSink, (SmiTypedGetterSetter) this._currentColumnValuesV3, ordinal, metaData, this._currentConnection.InternalContext);
            }
            return ValueUtilsSmi.GetSqlValue(this._readerEventSink, this._currentColumnValuesV3, ordinal, metaData, this._currentConnection.InternalContext);
        }

        public override int GetSqlValues(object[] values)
        {
            this.EnsureCanGetCol("GetSqlValues", 0);
            if (values == null)
            {
                throw ADP.ArgumentNull("values");
            }
            int num2 = (values.Length < this._visibleColumnCount) ? values.Length : this._visibleColumnCount;
            for (int i = 0; i < num2; i++)
            {
                values[this._indexMap[i]] = this.GetSqlValue(i);
            }
            return num2;
        }

        public override SqlXml GetSqlXml(int ordinal)
        {
            this.EnsureCanGetCol("GetSqlXml", ordinal);
            return ValueUtilsSmi.GetSqlXml(this._readerEventSink, this._currentColumnValuesV3, ordinal, this._currentMetaData[ordinal], this._currentConnection.InternalContext);
        }

        public override string GetString(int ordinal)
        {
            this.EnsureCanGetCol("GetString", ordinal);
            return ValueUtilsSmi.GetString(this._readerEventSink, this._currentColumnValuesV3, ordinal, this._currentMetaData[ordinal]);
        }

        public override TimeSpan GetTimeSpan(int ordinal)
        {
            this.EnsureCanGetCol("GetTimeSpan", ordinal);
            return ValueUtilsSmi.GetTimeSpan(this._readerEventSink, this._currentColumnValuesV3, ordinal, this._currentMetaData[ordinal], this._currentConnection.IsKatmaiOrNewer);
        }

        public override object GetValue(int ordinal)
        {
            this.EnsureCanGetCol("GetValue", ordinal);
            SmiQueryMetaData metaData = this._currentMetaData[ordinal];
            if (this._currentConnection.IsKatmaiOrNewer)
            {
                return ValueUtilsSmi.GetValue200(this._readerEventSink, (SmiTypedGetterSetter) this._currentColumnValuesV3, ordinal, metaData, this._currentConnection.InternalContext);
            }
            return ValueUtilsSmi.GetValue(this._readerEventSink, this._currentColumnValuesV3, ordinal, metaData, this._currentConnection.InternalContext);
        }

        public override int GetValues(object[] values)
        {
            this.EnsureCanGetCol("GetValues", 0);
            if (values == null)
            {
                throw ADP.ArgumentNull("values");
            }
            int num2 = (values.Length < this._visibleColumnCount) ? values.Length : this._visibleColumnCount;
            for (int i = 0; i < num2; i++)
            {
                values[this._indexMap[i]] = this.GetValue(i);
            }
            return num2;
        }

        internal bool InternalNextResult(bool ignoreNonFatalMessages)
        {
            bool flag;
            IntPtr zero = IntPtr.Zero;
            if (Bid.AdvancedOn)
            {
                Bid.ScopeEnter(out zero, "<sc.SqlDataReaderSmi.InternalNextResult|ADV> %d#", base.ObjectID);
            }
            try
            {
                this._hasRows = false;
                if (PositionState.AfterResults != this._currentPosition)
                {
                    while (this.InternalRead(ignoreNonFatalMessages))
                    {
                    }
                    while ((this._currentMetaData == null) && this._eventStream.HasEvents)
                    {
                        this._eventStream.ProcessEvent(this._readerEventSink);
                        this._readerEventSink.ProcessMessagesAndThrow(ignoreNonFatalMessages);
                    }
                }
                flag = PositionState.AfterResults != this._currentPosition;
            }
            finally
            {
                if (Bid.AdvancedOn)
                {
                    Bid.ScopeLeave(ref zero);
                }
            }
            return flag;
        }

        internal bool InternalRead(bool ignoreNonFatalErrors)
        {
            bool flag;
            IntPtr zero = IntPtr.Zero;
            if (Bid.AdvancedOn)
            {
                Bid.ScopeEnter(out zero, "<sc.SqlDataReaderSmi.InternalRead|ADV> %d#", base.ObjectID);
            }
            try
            {
                if (this.FInResults())
                {
                    this._currentColumnValues = null;
                    this._currentColumnValuesV3 = null;
                    while (((this._currentColumnValues == null) && (this._currentColumnValuesV3 == null)) && ((this.FInResults() && (PositionState.AfterRows != this._currentPosition)) && this._eventStream.HasEvents))
                    {
                        this._eventStream.ProcessEvent(this._readerEventSink);
                        this._readerEventSink.ProcessMessagesAndThrow(ignoreNonFatalErrors);
                    }
                }
                flag = PositionState.OnRow == this._currentPosition;
            }
            finally
            {
                if (Bid.AdvancedOn)
                {
                    Bid.ScopeLeave(ref zero);
                }
            }
            return flag;
        }

        public override bool IsDBNull(int ordinal)
        {
            this.EnsureCanGetCol("IsDBNull", ordinal);
            return ValueUtilsSmi.IsDBNull(this._readerEventSink, this._currentColumnValuesV3, ordinal);
        }

        private bool IsReallyClosed()
        {
            return !this._isOpen;
        }

        private void MetaDataAvailable(SmiQueryMetaData[] md, bool nextEventIsRow)
        {
            this._currentMetaData = md;
            this._hasRows = nextEventIsRow;
            this._fieldNameLookup = null;
            this._currentPosition = PositionState.BeforeRows;
            this._indexMap = new int[this._currentMetaData.Length];
            int index = 0;
            for (int i = 0; i < this._currentMetaData.Length; i++)
            {
                if (!this._currentMetaData[i].IsHidden.IsTrue)
                {
                    this._indexMap[index] = i;
                    index++;
                }
            }
            this._visibleColumnCount = index;
        }

        public override bool NextResult()
        {
            this.ThrowIfClosed("NextResult");
            return this.InternalNextResult(false);
        }

        public override bool Read()
        {
            this.ThrowIfClosed("Read");
            return this.InternalRead(false);
        }

        private void RowAvailable(ITypedGetters row)
        {
            this._currentColumnValues = row;
            this._currentPosition = PositionState.OnRow;
        }

        private void RowAvailable(ITypedGettersV3 row)
        {
            this._currentColumnValuesV3 = row;
            this._currentPosition = PositionState.OnRow;
        }

        private void StatementCompleted()
        {
            this._currentMetaData = null;
            this._visibleColumnCount = 0;
            this._schemaTable = null;
            this._currentPosition = PositionState.AfterRows;
        }

        internal void ThrowIfClosed(string operationName)
        {
            if (this.IsClosed)
            {
                throw ADP.DataReaderClosed(operationName);
            }
        }

        public override int Depth
        {
            get
            {
                this.ThrowIfClosed("Depth");
                return 0;
            }
        }

        public override int FieldCount
        {
            get
            {
                this.ThrowIfClosed("FieldCount");
                return this.InternalFieldCount;
            }
        }

        public override bool HasRows
        {
            get
            {
                return this._hasRows;
            }
        }

        private int InternalFieldCount
        {
            get
            {
                if (this.FNotInResults())
                {
                    return 0;
                }
                return this._currentMetaData.Length;
            }
        }

        public override bool IsClosed
        {
            get
            {
                return this.IsReallyClosed();
            }
        }

        public override object this[int ordinal]
        {
            get
            {
                return this.GetValue(ordinal);
            }
        }

        public override object this[string strName]
        {
            get
            {
                return this.GetValue(this.GetOrdinal(strName));
            }
        }

        public override int RecordsAffected
        {
            get
            {
                return base.Command.InternalRecordsAffected;
            }
        }

        public override int VisibleFieldCount
        {
            get
            {
                this.ThrowIfClosed("VisibleFieldCount");
                if (this.FNotInResults())
                {
                    return 0;
                }
                return this._visibleColumnCount;
            }
        }

        internal enum PositionState
        {
            BeforeResults,
            BeforeRows,
            OnRow,
            AfterRows,
            AfterResults
        }

        private sealed class ReaderEventSink : SmiEventSink_Default
        {
            private readonly SqlDataReaderSmi reader;

            internal ReaderEventSink(SqlDataReaderSmi reader, SmiEventSink parent) : base(parent)
            {
                this.reader = reader;
            }

            internal override void BatchCompleted()
            {
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<sc.SqlDataReaderSmi.ReaderEventSink.BatchCompleted|ADV> %d#.\n", this.reader.ObjectID);
                }
                base.BatchCompleted();
                this.reader.BatchCompleted();
            }

            internal override void MetaDataAvailable(SmiQueryMetaData[] md, bool nextEventIsRow)
            {
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<sc.SqlDataReaderSmi.ReaderEventSink.MetaDataAvailable|ADV> %d#, md.Length=%d nextEventIsRow=%d.\n", this.reader.ObjectID, (md != null) ? md.Length : -1, nextEventIsRow);
                    if (md != null)
                    {
                        for (int i = 0; i < md.Length; i++)
                        {
                            Bid.Trace("<sc.SqlDataReaderSmi.ReaderEventSink.MetaDataAvailable|ADV> %d#, metaData[%d] is %ls%ls\n", this.reader.ObjectID, i, md[i].GetType().ToString(), md[i].TraceString());
                        }
                    }
                }
                this.reader.MetaDataAvailable(md, nextEventIsRow);
            }

            internal override void RowAvailable(ITypedGetters row)
            {
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<sc.SqlDataReaderSmi.ReaderEventSink.RowAvailable|ADV> %d# (v2).\n", this.reader.ObjectID);
                }
                this.reader.RowAvailable(row);
            }

            internal override void RowAvailable(ITypedGettersV3 row)
            {
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<sc.SqlDataReaderSmi.ReaderEventSink.RowAvailable|ADV> %d# (ITypedGettersV3).\n", this.reader.ObjectID);
                }
                this.reader.RowAvailable(row);
            }

            internal override void RowAvailable(SmiTypedGetterSetter rowData)
            {
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<sc.SqlDataReaderSmi.ReaderEventSink.RowAvailable|ADV> %d# (SmiTypedGetterSetter).\n", this.reader.ObjectID);
                }
                this.reader.RowAvailable(rowData);
            }

            internal override void StatementCompleted(int recordsAffected)
            {
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<sc.SqlDataReaderSmi.ReaderEventSink.StatementCompleted|ADV> %d# recordsAffected=%d.\n", this.reader.ObjectID, recordsAffected);
                }
                base.StatementCompleted(recordsAffected);
                this.reader.StatementCompleted();
            }
        }
    }
}

