namespace System.Data.SqlClient
{
    using Microsoft.SqlServer.Server;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlTypes;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Xml;

    [TypeConverter(typeof(SqlParameter.SqlParameterConverter))]
    public sealed class SqlParameter : DbParameter, IDbDataParameter, IDataParameter, ICloneable
    {
        private object _coercedValue;
        private SqlCollation _collation;
        private ParameterDirection _direction;
        private bool _hasScale;
        private MetaType _internalMetaType;
        private bool _isNullable;
        private bool _isSqlParameterSqlType;
        private MetaType _metaType;
        private int _offset;
        private string _parameterName;
        private object _parent;
        private byte _precision;
        private byte _scale;
        private int _size;
        private string _sourceColumn;
        private bool _sourceColumnNullMapping;
        private DataRowVersion _sourceVersion;
        private SqlBuffer _sqlBufferReturnValue;
        private string _typeName;
        private Exception _udtLoadError;
        private Type _udtType;
        private string _udtTypeName;
        private object _value;
        private string _xmlSchemaCollectionDatabase;
        private string _xmlSchemaCollectionName;
        private string _xmlSchemaCollectionOwningSchema;

        public SqlParameter()
        {
        }

        private SqlParameter(SqlParameter source) : this()
        {
            ADP.CheckArgumentNull(source, "source");
            source.CloneHelper(this);
            ICloneable cloneable = this._value as ICloneable;
            if (cloneable != null)
            {
                this._value = cloneable.Clone();
            }
        }

        public SqlParameter(string parameterName, System.Data.SqlDbType dbType) : this()
        {
            this.ParameterName = parameterName;
            this.SqlDbType = dbType;
        }

        public SqlParameter(string parameterName, object value) : this()
        {
            this.ParameterName = parameterName;
            this.Value = value;
        }

        public SqlParameter(string parameterName, System.Data.SqlDbType dbType, int size) : this()
        {
            this.ParameterName = parameterName;
            this.SqlDbType = dbType;
            this.Size = size;
        }

        public SqlParameter(string parameterName, System.Data.SqlDbType dbType, int size, string sourceColumn) : this()
        {
            this.ParameterName = parameterName;
            this.SqlDbType = dbType;
            this.Size = size;
            this.SourceColumn = sourceColumn;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public SqlParameter(string parameterName, System.Data.SqlDbType dbType, int size, ParameterDirection direction, bool isNullable, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, object value) : this()
        {
            this.ParameterName = parameterName;
            this.SqlDbType = dbType;
            this.Size = size;
            this.Direction = direction;
            this.IsNullable = isNullable;
            this.PrecisionInternal = precision;
            this.ScaleInternal = scale;
            this.SourceColumn = sourceColumn;
            this.SourceVersion = sourceVersion;
            this.Value = value;
        }

        public SqlParameter(string parameterName, System.Data.SqlDbType dbType, int size, ParameterDirection direction, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, bool sourceColumnNullMapping, object value, string xmlSchemaCollectionDatabase, string xmlSchemaCollectionOwningSchema, string xmlSchemaCollectionName)
        {
            this.ParameterName = parameterName;
            this.SqlDbType = dbType;
            this.Size = size;
            this.Direction = direction;
            this.PrecisionInternal = precision;
            this.ScaleInternal = scale;
            this.SourceColumn = sourceColumn;
            this.SourceVersion = sourceVersion;
            this.SourceColumnNullMapping = sourceColumnNullMapping;
            this.Value = value;
            this._xmlSchemaCollectionDatabase = xmlSchemaCollectionDatabase;
            this._xmlSchemaCollectionOwningSchema = xmlSchemaCollectionOwningSchema;
            this._xmlSchemaCollectionName = xmlSchemaCollectionName;
        }

        private void CloneHelper(SqlParameter destination)
        {
            this.CloneHelperCore(destination);
            destination._metaType = this._metaType;
            destination._collation = this._collation;
            destination._xmlSchemaCollectionDatabase = this._xmlSchemaCollectionDatabase;
            destination._xmlSchemaCollectionOwningSchema = this._xmlSchemaCollectionOwningSchema;
            destination._xmlSchemaCollectionName = this._xmlSchemaCollectionName;
            destination._udtTypeName = this._udtTypeName;
            destination._typeName = this._typeName;
            destination._udtLoadError = this._udtLoadError;
            destination._parameterName = this._parameterName;
            destination._precision = this._precision;
            destination._scale = this._scale;
            destination._sqlBufferReturnValue = this._sqlBufferReturnValue;
            destination._isSqlParameterSqlType = this._isSqlParameterSqlType;
            destination._internalMetaType = this._internalMetaType;
            destination.CoercedValue = this.CoercedValue;
        }

        private void CloneHelperCore(SqlParameter destination)
        {
            destination._value = this._value;
            destination._direction = this._direction;
            destination._size = this._size;
            destination._offset = this._offset;
            destination._sourceColumn = this._sourceColumn;
            destination._sourceVersion = this._sourceVersion;
            destination._sourceColumnNullMapping = this._sourceColumnNullMapping;
            destination._isNullable = this._isNullable;
        }

        internal static object CoerceValue(object value, MetaType destinationType)
        {
            if ((value != null) && (DBNull.Value != value))
            {
                Type c = value.GetType();
                bool flag = true;
                if ((value is INullable) && ((INullable) value).IsNull)
                {
                    flag = false;
                }
                if (!flag || !(typeof(object) != destinationType.ClassType))
                {
                    return value;
                }
                if (((c == destinationType.ClassType) || (c == destinationType.SqlType)) && (System.Data.SqlDbType.Xml != destinationType.SqlDbType))
                {
                    return value;
                }
                try
                {
                    if (typeof(string) == destinationType.ClassType)
                    {
                        if (typeof(SqlXml) == c)
                        {
                            value = MetaType.GetStringFromXml(((SqlXml) value).CreateReader());
                            return value;
                        }
                        if (typeof(SqlString) != c)
                        {
                            if (typeof(XmlReader).IsAssignableFrom(c))
                            {
                                value = MetaType.GetStringFromXml((XmlReader) value);
                                return value;
                            }
                            if (typeof(char[]) == c)
                            {
                                value = new string((char[]) value);
                                return value;
                            }
                            if (typeof(SqlChars) == c)
                            {
                                SqlChars chars = (SqlChars) value;
                                value = new string(chars.Value);
                                return value;
                            }
                            value = Convert.ChangeType(value, destinationType.ClassType, null);
                        }
                        return value;
                    }
                    if ((System.Data.DbType.Currency == destinationType.DbType) && (typeof(string) == c))
                    {
                        value = decimal.Parse((string) value, NumberStyles.Currency, null);
                        return value;
                    }
                    if ((typeof(SqlBytes) == c) && (typeof(byte[]) == destinationType.ClassType))
                    {
                        SqlBytes bytes1 = (SqlBytes) value;
                        return value;
                    }
                    if ((typeof(string) == c) && (System.Data.SqlDbType.Time == destinationType.SqlDbType))
                    {
                        value = TimeSpan.Parse((string) value);
                        return value;
                    }
                    if ((typeof(string) == c) && (System.Data.SqlDbType.DateTimeOffset == destinationType.SqlDbType))
                    {
                        value = DateTimeOffset.Parse((string) value, null);
                        return value;
                    }
                    if ((typeof(DateTime) == c) && (System.Data.SqlDbType.DateTimeOffset == destinationType.SqlDbType))
                    {
                        value = new DateTimeOffset((DateTime) value);
                        return value;
                    }
                    if ((0xf3 == destinationType.TDSType) && (((value is DataTable) || (value is DbDataReader)) || (value is IEnumerable<SqlDataRecord>)))
                    {
                        return value;
                    }
                    value = Convert.ChangeType(value, destinationType.ClassType, null);
                }
                catch (Exception exception)
                {
                    if (!ADP.IsCatchableExceptionType(exception))
                    {
                        throw;
                    }
                    throw ADP.ParameterConversionFailed(value, destinationType.ClassType, exception);
                }
            }
            return value;
        }

        internal object CompareExchangeParent(object value, object comparand)
        {
            object obj2 = this._parent;
            if (comparand == obj2)
            {
                this._parent = value;
            }
            return obj2;
        }

        internal void CopyTo(DbParameter destination)
        {
            ADP.CheckArgumentNull(destination, "destination");
            this.CloneHelper((SqlParameter) destination);
        }

        private void GetActualFieldsAndProperties(out List<SmiExtendedMetaData> fields, out SmiMetaDataPropertyCollection props, out ParameterPeekAheadValue peekAhead)
        {
            fields = null;
            props = null;
            peekAhead = null;
            object coercedValue = this.GetCoercedValue();
            if (coercedValue is DataTable)
            {
                DataTable parent = coercedValue as DataTable;
                if (parent.Columns.Count <= 0)
                {
                    throw SQL.NotEnoughColumnsInStructuredType();
                }
                fields = new List<SmiExtendedMetaData>(parent.Columns.Count);
                bool[] collection = new bool[parent.Columns.Count];
                bool flag = false;
                if ((parent.PrimaryKey != null) && (0 < parent.PrimaryKey.Length))
                {
                    foreach (DataColumn column in parent.PrimaryKey)
                    {
                        collection[column.Ordinal] = true;
                        flag = true;
                    }
                }
                for (int i = 0; i < parent.Columns.Count; i++)
                {
                    fields.Add(MetaDataUtilsSmi.SmiMetaDataFromDataColumn(parent.Columns[i], parent));
                    if (!flag && parent.Columns[i].Unique)
                    {
                        collection[i] = true;
                        flag = true;
                    }
                }
                if (flag)
                {
                    props = new SmiMetaDataPropertyCollection();
                    props[SmiPropertySelector.UniqueKey] = new SmiUniqueKeyProperty(new List<bool>(collection));
                }
            }
            else if (coercedValue is SqlDataReader)
            {
                fields = new List<SmiExtendedMetaData>(((SqlDataReader) coercedValue).GetInternalSmiMetaData());
                if (fields.Count <= 0)
                {
                    throw SQL.NotEnoughColumnsInStructuredType();
                }
                bool[] flagArray6 = new bool[fields.Count];
                bool flag5 = false;
                for (int j = 0; j < fields.Count; j++)
                {
                    SmiQueryMetaData data2 = fields[j] as SmiQueryMetaData;
                    if (((data2 != null) && !data2.IsKey.IsNull) && data2.IsKey.Value)
                    {
                        flagArray6[j] = true;
                        flag5 = true;
                    }
                }
                if (flag5)
                {
                    props = new SmiMetaDataPropertyCollection();
                    props[SmiPropertySelector.UniqueKey] = new SmiUniqueKeyProperty(new List<bool>(flagArray6));
                }
            }
            else
            {
                if (coercedValue is IEnumerable<SqlDataRecord>)
                {
                    IEnumerator<SqlDataRecord> enumerator = ((IEnumerable<SqlDataRecord>) coercedValue).GetEnumerator();
                    SqlDataRecord current = null;
                    try
                    {
                        if (!enumerator.MoveNext())
                        {
                            throw SQL.IEnumerableOfSqlDataRecordHasNoRows();
                        }
                        current = enumerator.Current;
                        int fieldCount = current.FieldCount;
                        if (0 >= fieldCount)
                        {
                            throw SQL.NotEnoughColumnsInStructuredType();
                        }
                        bool[] flagArray5 = new bool[fieldCount];
                        bool[] flagArray4 = new bool[fieldCount];
                        bool[] flagArray = new bool[fieldCount];
                        int sortOrdinal = -1;
                        bool flag4 = false;
                        bool flag3 = false;
                        int num7 = 0;
                        SmiOrderProperty.SmiColumnOrder[] orderArray = new SmiOrderProperty.SmiColumnOrder[fieldCount];
                        fields = new List<SmiExtendedMetaData>(fieldCount);
                        for (int k = 0; k < fieldCount; k++)
                        {
                            SqlMetaData sqlMetaData = current.GetSqlMetaData(k);
                            fields.Add(MetaDataUtilsSmi.SqlMetaDataToSmiExtendedMetaData(sqlMetaData));
                            if (sqlMetaData.IsUniqueKey)
                            {
                                flagArray5[k] = true;
                                flag4 = true;
                            }
                            if (sqlMetaData.UseServerDefault)
                            {
                                flagArray4[k] = true;
                                flag3 = true;
                            }
                            orderArray[k].Order = sqlMetaData.SortOrder;
                            if (SortOrder.Unspecified != sqlMetaData.SortOrder)
                            {
                                if (fieldCount <= sqlMetaData.SortOrdinal)
                                {
                                    throw SQL.SortOrdinalGreaterThanFieldCount(k, sqlMetaData.SortOrdinal);
                                }
                                if (flagArray[sqlMetaData.SortOrdinal])
                                {
                                    throw SQL.DuplicateSortOrdinal(sqlMetaData.SortOrdinal);
                                }
                                orderArray[k].SortOrdinal = sqlMetaData.SortOrdinal;
                                flagArray[sqlMetaData.SortOrdinal] = true;
                                if (sqlMetaData.SortOrdinal > sortOrdinal)
                                {
                                    sortOrdinal = sqlMetaData.SortOrdinal;
                                }
                                num7++;
                            }
                        }
                        if (flag4)
                        {
                            props = new SmiMetaDataPropertyCollection();
                            props[SmiPropertySelector.UniqueKey] = new SmiUniqueKeyProperty(new List<bool>(flagArray5));
                        }
                        if (flag3)
                        {
                            if (props == null)
                            {
                                props = new SmiMetaDataPropertyCollection();
                            }
                            props[SmiPropertySelector.DefaultFields] = new SmiDefaultFieldsProperty(new List<bool>(flagArray4));
                        }
                        if (0 < num7)
                        {
                            if (sortOrdinal >= num7)
                            {
                                int index = 0;
                                while (index < num7)
                                {
                                    if (!flagArray[index])
                                    {
                                        break;
                                    }
                                    index++;
                                }
                                throw SQL.MissingSortOrdinal(index);
                            }
                            if (props == null)
                            {
                                props = new SmiMetaDataPropertyCollection();
                            }
                            props[SmiPropertySelector.SortOrder] = new SmiOrderProperty(new List<SmiOrderProperty.SmiColumnOrder>(orderArray));
                        }
                        peekAhead = new ParameterPeekAheadValue();
                        peekAhead.Enumerator = enumerator;
                        peekAhead.FirstRecord = current;
                        enumerator = null;
                        return;
                    }
                    finally
                    {
                        if (enumerator != null)
                        {
                            enumerator.Dispose();
                        }
                    }
                }
                if (coercedValue is DbDataReader)
                {
                    DataTable schemaTable = ((DbDataReader) coercedValue).GetSchemaTable();
                    if (schemaTable.Rows.Count <= 0)
                    {
                        throw SQL.NotEnoughColumnsInStructuredType();
                    }
                    int count = schemaTable.Rows.Count;
                    fields = new List<SmiExtendedMetaData>(count);
                    bool[] flagArray3 = new bool[count];
                    bool flag2 = false;
                    int ordinal = schemaTable.Columns[SchemaTableColumn.IsKey].Ordinal;
                    int columnIndex = schemaTable.Columns[SchemaTableColumn.ColumnOrdinal].Ordinal;
                    for (int m = 0; m < count; m++)
                    {
                        DataRow schemaRow = schemaTable.Rows[m];
                        SmiExtendedMetaData item = MetaDataUtilsSmi.SmiMetaDataFromSchemaTableRow(schemaRow);
                        int num2 = m;
                        if (!schemaRow.IsNull(columnIndex))
                        {
                            num2 = (int) schemaRow[columnIndex];
                        }
                        if ((num2 < count) && (num2 >= 0))
                        {
                            goto Label_04B7;
                        }
                        throw SQL.InvalidSchemaTableOrdinals();
                    Label_04AF:
                        fields.Add(null);
                    Label_04B7:
                        if (num2 > fields.Count)
                        {
                            goto Label_04AF;
                        }
                        if (fields.Count == num2)
                        {
                            fields.Add(item);
                        }
                        else
                        {
                            if (fields[num2] != null)
                            {
                                throw SQL.InvalidSchemaTableOrdinals();
                            }
                            fields[num2] = item;
                        }
                        if (!schemaRow.IsNull(ordinal) && ((bool) schemaRow[ordinal]))
                        {
                            flagArray3[num2] = true;
                            flag2 = true;
                        }
                    }
                    if (flag2)
                    {
                        props = new SmiMetaDataPropertyCollection();
                        props[SmiPropertySelector.UniqueKey] = new SmiUniqueKeyProperty(new List<bool>(flagArray3));
                    }
                }
            }
        }

        internal byte GetActualPrecision()
        {
            if (!this.ShouldSerializePrecision())
            {
                return this.ValuePrecision(this.CoercedValue);
            }
            return this.PrecisionInternal;
        }

        internal byte GetActualScale()
        {
            if (this.ShouldSerializeScale())
            {
                return this.ScaleInternal;
            }
            if (this.GetMetaTypeOnly().IsVarTime)
            {
                return 7;
            }
            return this.ValueScale(this.CoercedValue);
        }

        internal int GetActualSize()
        {
            int num = 0;
            MetaType internalMetaType = this.InternalMetaType;
            System.Data.SqlDbType sqlDbType = internalMetaType.SqlDbType;
            object coercedValue = this.GetCoercedValue();
            bool flag = false;
            if (ADP.IsNull(coercedValue) && !internalMetaType.IsVarTime)
            {
                return 0;
            }
            if (sqlDbType == System.Data.SqlDbType.Variant)
            {
                internalMetaType = MetaType.GetMetaTypeFromValue(coercedValue);
                sqlDbType = MetaType.GetSqlDataType(internalMetaType.TDSType, 0, 0).SqlDbType;
                flag = true;
            }
            if (internalMetaType.IsFixed)
            {
                return internalMetaType.FixedLength;
            }
            int length = 0;
            switch (sqlDbType)
            {
                case System.Data.SqlDbType.Binary:
                case System.Data.SqlDbType.Image:
                case System.Data.SqlDbType.Timestamp:
                case System.Data.SqlDbType.VarBinary:
                    length = this.ValueSize(coercedValue);
                    num = this.ShouldSerializeSize() ? this.Size : 0;
                    num = (this.ShouldSerializeSize() && (num <= length)) ? num : length;
                    if (num == -1)
                    {
                        num = length;
                    }
                    break;

                case System.Data.SqlDbType.Char:
                case System.Data.SqlDbType.Text:
                case System.Data.SqlDbType.VarChar:
                    length = this.ValueSize(coercedValue);
                    num = this.ShouldSerializeSize() ? this.Size : 0;
                    num = (this.ShouldSerializeSize() && (num <= length)) ? num : length;
                    if (num == -1)
                    {
                        num = length;
                    }
                    break;

                case System.Data.SqlDbType.NChar:
                case System.Data.SqlDbType.NText:
                case System.Data.SqlDbType.NVarChar:
                case System.Data.SqlDbType.Xml:
                    length = this.ValueSize(coercedValue);
                    num = this.ShouldSerializeSize() ? this.Size : 0;
                    num = (this.ShouldSerializeSize() && (num <= length)) ? num : length;
                    if (num == -1)
                    {
                        num = length;
                    }
                    num = num << 1;
                    break;

                case System.Data.SqlDbType.Udt:
                    if (!ADP.IsNull(coercedValue))
                    {
                        length = AssemblyCache.GetLength(coercedValue);
                    }
                    break;

                case System.Data.SqlDbType.Structured:
                    length = -1;
                    break;

                case System.Data.SqlDbType.Time:
                    num = flag ? 5 : MetaType.GetTimeSizeFromScale(this.GetActualScale());
                    break;

                case System.Data.SqlDbType.DateTime2:
                    num = 3 + (flag ? 5 : MetaType.GetTimeSizeFromScale(this.GetActualScale()));
                    break;

                case System.Data.SqlDbType.DateTimeOffset:
                    num = 5 + (flag ? 5 : MetaType.GetTimeSizeFromScale(this.GetActualScale()));
                    break;
            }
            if (flag && (length > 0x1f40))
            {
                throw SQL.ParameterInvalidVariant(this.ParameterName);
            }
            return num;
        }

        internal object GetCoercedValue()
        {
            object coercedValue = this.CoercedValue;
            if (coercedValue == null)
            {
                coercedValue = CoerceValue(this.Value, this._internalMetaType);
                this.CoercedValue = coercedValue;
            }
            return coercedValue;
        }

        private System.Data.SqlDbType GetMetaSqlDbTypeOnly()
        {
            MetaType defaultMetaType = this._metaType;
            if (defaultMetaType == null)
            {
                defaultMetaType = MetaType.GetDefaultMetaType();
            }
            return defaultMetaType.SqlDbType;
        }

        private MetaType GetMetaTypeOnly()
        {
            if (this._metaType != null)
            {
                return this._metaType;
            }
            if ((this._value != null) && (DBNull.Value != this._value))
            {
                Type dataType = this._value.GetType();
                if (typeof(char) == dataType)
                {
                    this._value = this._value.ToString();
                    dataType = typeof(string);
                }
                else if (typeof(char[]) == dataType)
                {
                    this._value = new string((char[]) this._value);
                    dataType = typeof(string);
                }
                return MetaType.GetMetaTypeFromType(dataType);
            }
            if (this._sqlBufferReturnValue != null)
            {
                Type typeFromStorageType = this._sqlBufferReturnValue.GetTypeFromStorageType(this._isSqlParameterSqlType);
                if (null != typeFromStorageType)
                {
                    return MetaType.GetMetaTypeFromType(typeFromStorageType);
                }
            }
            return MetaType.GetDefaultMetaType();
        }

        internal int GetParameterSize()
        {
            if (!this.ShouldSerializeSize())
            {
                return this.ValueSize(this.CoercedValue);
            }
            return this.Size;
        }

        internal SmiParameterMetaData MetaDataForSmi(out ParameterPeekAheadValue peekAhead)
        {
            string xmlSchemaCollectionDatabase;
            SqlCompareOptions compareOptions;
            peekAhead = null;
            MetaType type = this.ValidateTypeLengths(true);
            long actualSize = this.GetActualSize();
            long size = this.Size;
            if (!type.IsLong)
            {
                if ((System.Data.SqlDbType.NChar == type.SqlDbType) || (System.Data.SqlDbType.NVarChar == type.SqlDbType))
                {
                    actualSize /= 2L;
                }
                if (actualSize > size)
                {
                    size = actualSize;
                }
            }
            if (0L == size)
            {
                if ((System.Data.SqlDbType.Binary == type.SqlDbType) || (System.Data.SqlDbType.VarBinary == type.SqlDbType))
                {
                    size = 0x1f40L;
                }
                else if ((System.Data.SqlDbType.Char == type.SqlDbType) || (System.Data.SqlDbType.VarChar == type.SqlDbType))
                {
                    size = 0x1f40L;
                }
                else if ((System.Data.SqlDbType.NChar == type.SqlDbType) || (System.Data.SqlDbType.NVarChar == type.SqlDbType))
                {
                    size = 0xfa0L;
                }
            }
            else if ((((size > 0x1f40L) && ((System.Data.SqlDbType.Binary == type.SqlDbType) || (System.Data.SqlDbType.VarBinary == type.SqlDbType))) || ((size > 0x1f40L) && ((System.Data.SqlDbType.Char == type.SqlDbType) || (System.Data.SqlDbType.VarChar == type.SqlDbType)))) || ((size > 0xfa0L) && ((System.Data.SqlDbType.NChar == type.SqlDbType) || (System.Data.SqlDbType.NVarChar == type.SqlDbType))))
            {
                size = -1L;
            }
            int localeId = this.LocaleId;
            if ((localeId == 0) && type.IsCharType)
            {
                object coercedValue = this.GetCoercedValue();
                if (coercedValue is SqlString)
                {
                    SqlString str7 = (SqlString) coercedValue;
                    if (!str7.IsNull)
                    {
                        SqlString str6 = (SqlString) coercedValue;
                        localeId = str6.LCID;
                        goto Label_0156;
                    }
                }
                localeId = CultureInfo.CurrentCulture.LCID;
            }
        Label_0156:
            compareOptions = this.CompareInfo;
            if ((compareOptions == SqlCompareOptions.None) && type.IsCharType)
            {
                object obj2 = this.GetCoercedValue();
                if (obj2 is SqlString)
                {
                    SqlString str5 = (SqlString) obj2;
                    if (!str5.IsNull)
                    {
                        SqlString str4 = (SqlString) obj2;
                        compareOptions = str4.SqlCompareOptions;
                        goto Label_01B3;
                    }
                }
                compareOptions = SmiMetaData.GetDefaultForType(type.SqlDbType).CompareOptions;
            }
        Label_01B3:
            xmlSchemaCollectionDatabase = null;
            string xmlSchemaCollectionOwningSchema = null;
            string xmlSchemaCollectionName = null;
            if (System.Data.SqlDbType.Xml == type.SqlDbType)
            {
                xmlSchemaCollectionDatabase = this.XmlSchemaCollectionDatabase;
                xmlSchemaCollectionOwningSchema = this.XmlSchemaCollectionOwningSchema;
                xmlSchemaCollectionName = this.XmlSchemaCollectionName;
            }
            else if ((System.Data.SqlDbType.Udt == type.SqlDbType) || ((System.Data.SqlDbType.Structured == type.SqlDbType) && !ADP.IsEmpty(this.TypeName)))
            {
                string[] strArray;
                if (System.Data.SqlDbType.Udt == type.SqlDbType)
                {
                    strArray = ParseTypeName(this.UdtTypeName, true);
                }
                else
                {
                    strArray = ParseTypeName(this.TypeName, false);
                }
                if (1 == strArray.Length)
                {
                    xmlSchemaCollectionName = strArray[0];
                }
                else if (2 == strArray.Length)
                {
                    xmlSchemaCollectionOwningSchema = strArray[0];
                    xmlSchemaCollectionName = strArray[1];
                }
                else
                {
                    if (3 != strArray.Length)
                    {
                        throw ADP.ArgumentOutOfRange("names");
                    }
                    xmlSchemaCollectionDatabase = strArray[0];
                    xmlSchemaCollectionOwningSchema = strArray[1];
                    xmlSchemaCollectionName = strArray[2];
                }
                if (((!ADP.IsEmpty(xmlSchemaCollectionDatabase) && (0xff < xmlSchemaCollectionDatabase.Length)) || (!ADP.IsEmpty(xmlSchemaCollectionOwningSchema) && (0xff < xmlSchemaCollectionOwningSchema.Length))) || (!ADP.IsEmpty(xmlSchemaCollectionName) && (0xff < xmlSchemaCollectionName.Length)))
                {
                    throw ADP.ArgumentOutOfRange("names");
                }
            }
            byte actualPrecision = this.GetActualPrecision();
            byte actualScale = this.GetActualScale();
            if ((System.Data.SqlDbType.Decimal == type.SqlDbType) && (actualPrecision == 0))
            {
                actualPrecision = 0x1d;
            }
            List<SmiExtendedMetaData> fields = null;
            SmiMetaDataPropertyCollection props = null;
            if (System.Data.SqlDbType.Structured == type.SqlDbType)
            {
                this.GetActualFieldsAndProperties(out fields, out props, out peekAhead);
            }
            return new SmiParameterMetaData(type.SqlDbType, size, actualPrecision, actualScale, (long) localeId, compareOptions, null, System.Data.SqlDbType.Structured == type.SqlDbType, fields, props, this.ParameterNameFixed, xmlSchemaCollectionDatabase, xmlSchemaCollectionOwningSchema, xmlSchemaCollectionName, this.Direction);
        }

        internal static string[] ParseTypeName(string typeName, bool isUdtTypeName)
        {
            string[] strArray;
            try
            {
                string str;
                if (isUdtTypeName)
                {
                    str = "SQL_UDTTypeName";
                }
                else
                {
                    str = "SQL_TypeName";
                }
                strArray = MultipartIdentifier.ParseMultipartIdentifier(typeName, "[\"", "]\"", '.', 3, true, str, true);
            }
            catch (ArgumentException)
            {
                if (isUdtTypeName)
                {
                    throw SQL.InvalidUdt3PartNameFormat();
                }
                throw SQL.InvalidParameterTypeNameFormat();
            }
            return strArray;
        }

        internal void Prepare(SqlCommand cmd)
        {
            if (this._metaType == null)
            {
                throw ADP.PrepareParameterType(cmd);
            }
            if (!this.ShouldSerializeSize() && !this._metaType.IsFixed)
            {
                throw ADP.PrepareParameterSize(cmd);
            }
            if ((!this.ShouldSerializePrecision() && !this.ShouldSerializeScale()) && (this._metaType.SqlDbType == System.Data.SqlDbType.Decimal))
            {
                throw ADP.PrepareParameterScale(cmd, this.SqlDbType.ToString());
            }
        }

        private void PropertyChanging()
        {
            this._internalMetaType = null;
        }

        private void PropertyTypeChanging()
        {
            this.PropertyChanging();
            this.CoercedValue = null;
        }

        public override void ResetDbType()
        {
            this.ResetSqlDbType();
        }

        internal void ResetParent()
        {
            this._parent = null;
        }

        private void ResetSize()
        {
            if (this._size != 0)
            {
                this.PropertyChanging();
                this._size = 0;
            }
        }

        public void ResetSqlDbType()
        {
            if (this._metaType != null)
            {
                this.PropertyTypeChanging();
                this._metaType = null;
            }
        }

        internal void SetSqlBuffer(SqlBuffer buff)
        {
            this._sqlBufferReturnValue = buff;
            this._value = null;
            this._coercedValue = null;
            this._udtLoadError = null;
        }

        internal void SetUdtLoadError(Exception e)
        {
            this._udtLoadError = e;
        }

        private bool ShouldSerializePrecision()
        {
            return (0 != this._precision);
        }

        private bool ShouldSerializeScale()
        {
            return (0 != this._scale);
        }

        private bool ShouldSerializeSize()
        {
            return (0 != this._size);
        }

        private bool ShouldSerializeSqlDbType()
        {
            return (null != this._metaType);
        }

        object ICloneable.Clone()
        {
            return new SqlParameter(this);
        }

        public override string ToString()
        {
            return this.ParameterName;
        }

        internal void Validate(int index, bool isCommandProc)
        {
            MetaType metaTypeOnly = this.GetMetaTypeOnly();
            this._internalMetaType = metaTypeOnly;
            if ((((ADP.IsDirection(this, ParameterDirection.Output) && !ADP.IsDirection(this, ParameterDirection.ReturnValue)) && (!metaTypeOnly.IsFixed && !this.ShouldSerializeSize())) && ((this._value == null) || Convert.IsDBNull(this._value))) && (((this.SqlDbType != System.Data.SqlDbType.Timestamp) && (this.SqlDbType != System.Data.SqlDbType.Udt)) && ((this.SqlDbType != System.Data.SqlDbType.Xml) && !metaTypeOnly.IsVarTime)))
            {
                throw ADP.UninitializedParameterSize(index, metaTypeOnly.ClassType);
            }
            if ((metaTypeOnly.SqlDbType != System.Data.SqlDbType.Udt) && (this.Direction != ParameterDirection.Output))
            {
                this.GetCoercedValue();
            }
            if (metaTypeOnly.SqlDbType == System.Data.SqlDbType.Udt)
            {
                if (ADP.IsEmpty(this.UdtTypeName))
                {
                    throw SQL.MustSetUdtTypeNameForUdtParams();
                }
            }
            else if (!ADP.IsEmpty(this.UdtTypeName))
            {
                throw SQL.UnexpectedUdtTypeNameForNonUdtParams();
            }
            if (metaTypeOnly.SqlDbType == System.Data.SqlDbType.Structured)
            {
                if (!isCommandProc && ADP.IsEmpty(this.TypeName))
                {
                    throw SQL.MustSetTypeNameForParam(metaTypeOnly.TypeName, this.ParameterName);
                }
                if (ParameterDirection.Input != this.Direction)
                {
                    throw SQL.UnsupportedTVPOutputParameter(this.Direction, this.ParameterName);
                }
                if (DBNull.Value == this.GetCoercedValue())
                {
                    throw SQL.DBNullNotSupportedForTVPValues(this.ParameterName);
                }
            }
            else if (!ADP.IsEmpty(this.TypeName))
            {
                throw SQL.UnexpectedTypeNameForNonStructParams(this.ParameterName);
            }
        }

        internal MetaType ValidateTypeLengths(bool yukonOrNewer)
        {
            MetaType internalMetaType = this.InternalMetaType;
            if (((System.Data.SqlDbType.Udt != internalMetaType.SqlDbType) && !internalMetaType.IsFixed) && !internalMetaType.IsLong)
            {
                long actualSize = this.GetActualSize();
                long size = this.Size;
                long num3 = 0L;
                if (internalMetaType.IsNCharType && yukonOrNewer)
                {
                    num3 = ((size * 2L) > actualSize) ? (size * 2L) : actualSize;
                }
                else
                {
                    num3 = (size > actualSize) ? size : actualSize;
                }
                if (((num3 <= 0x1f40L) && (size != -1L)) && (actualSize != -1L))
                {
                    return internalMetaType;
                }
                if (yukonOrNewer)
                {
                    internalMetaType = MetaType.GetMaxMetaTypeFromMetaType(internalMetaType);
                    this._metaType = internalMetaType;
                    this.InternalMetaType = internalMetaType;
                    if (!internalMetaType.IsPlp)
                    {
                        if (internalMetaType.SqlDbType == System.Data.SqlDbType.Xml)
                        {
                            throw ADP.InvalidMetaDataValue();
                        }
                        if (((internalMetaType.SqlDbType != System.Data.SqlDbType.NVarChar) && (internalMetaType.SqlDbType != System.Data.SqlDbType.VarChar)) && (internalMetaType.SqlDbType != System.Data.SqlDbType.VarBinary))
                        {
                            return internalMetaType;
                        }
                        this.Size = -1;
                    }
                    return internalMetaType;
                }
                switch (internalMetaType.SqlDbType)
                {
                    case System.Data.SqlDbType.Binary:
                    case System.Data.SqlDbType.VarBinary:
                        internalMetaType = MetaType.GetMetaTypeFromSqlDbType(System.Data.SqlDbType.Image, false);
                        this._metaType = internalMetaType;
                        this.InternalMetaType = internalMetaType;
                        return internalMetaType;

                    case System.Data.SqlDbType.Bit:
                        return internalMetaType;

                    case System.Data.SqlDbType.Char:
                    case System.Data.SqlDbType.VarChar:
                        internalMetaType = MetaType.GetMetaTypeFromSqlDbType(System.Data.SqlDbType.Text, false);
                        this._metaType = internalMetaType;
                        this.InternalMetaType = internalMetaType;
                        return internalMetaType;

                    case System.Data.SqlDbType.NChar:
                    case System.Data.SqlDbType.NVarChar:
                        internalMetaType = MetaType.GetMetaTypeFromSqlDbType(System.Data.SqlDbType.NText, false);
                        this._metaType = internalMetaType;
                        this.InternalMetaType = internalMetaType;
                        return internalMetaType;

                    case System.Data.SqlDbType.NText:
                        return internalMetaType;
                }
            }
            return internalMetaType;
        }

        private byte ValuePrecision(object value)
        {
            if (!(value is SqlDecimal))
            {
                return this.ValuePrecisionCore(value);
            }
            SqlDecimal num2 = (SqlDecimal) value;
            if (num2.IsNull)
            {
                return 0;
            }
            SqlDecimal num = (SqlDecimal) value;
            return num.Precision;
        }

        private byte ValuePrecisionCore(object value)
        {
            if (value is decimal)
            {
                SqlDecimal num = (decimal) value;
                return num.Precision;
            }
            return 0;
        }

        private byte ValueScale(object value)
        {
            if (!(value is SqlDecimal))
            {
                return this.ValueScaleCore(value);
            }
            SqlDecimal num2 = (SqlDecimal) value;
            if (num2.IsNull)
            {
                return 0;
            }
            SqlDecimal num = (SqlDecimal) value;
            return num.Scale;
        }

        private byte ValueScaleCore(object value)
        {
            if (value is decimal)
            {
                return (byte) ((decimal.GetBits((decimal) value)[3] & 0xff0000) >> 0x10);
            }
            return 0;
        }

        private int ValueSize(object value)
        {
            if (value is SqlString)
            {
                SqlString str2 = (SqlString) value;
                if (str2.IsNull)
                {
                    return 0;
                }
                SqlString str = (SqlString) value;
                return str.Value.Length;
            }
            if (value is SqlChars)
            {
                if (((SqlChars) value).IsNull)
                {
                    return 0;
                }
                return ((SqlChars) value).Value.Length;
            }
            if (value is SqlBinary)
            {
                SqlBinary binary2 = (SqlBinary) value;
                if (binary2.IsNull)
                {
                    return 0;
                }
                SqlBinary binary = (SqlBinary) value;
                return binary.Length;
            }
            if (!(value is SqlBytes))
            {
                return this.ValueSizeCore(value);
            }
            if (((SqlBytes) value).IsNull)
            {
                return 0;
            }
            return (int) ((SqlBytes) value).Length;
        }

        private int ValueSizeCore(object value)
        {
            if (!ADP.IsNull(value))
            {
                string str = value as string;
                if (str != null)
                {
                    return str.Length;
                }
                byte[] buffer = value as byte[];
                if (buffer != null)
                {
                    return buffer.Length;
                }
                char[] chArray = value as char[];
                if (chArray != null)
                {
                    return chArray.Length;
                }
                if ((value is byte) || (value is char))
                {
                    return 1;
                }
            }
            return 0;
        }

        private object CoercedValue
        {
            get
            {
                return this._coercedValue;
            }
            set
            {
                this._coercedValue = value;
            }
        }

        internal SqlCollation Collation
        {
            get
            {
                return this._collation;
            }
            set
            {
                this._collation = value;
            }
        }

        [Browsable(false)]
        public SqlCompareOptions CompareInfo
        {
            get
            {
                SqlCollation collation = this._collation;
                if (collation != null)
                {
                    return collation.SqlCompareOptions;
                }
                return SqlCompareOptions.None;
            }
            set
            {
                SqlCollation collation = this._collation;
                if (collation == null)
                {
                    this._collation = collation = new SqlCollation();
                }
                if ((value & SqlString.x_iValidSqlCompareOptionMask) != value)
                {
                    throw ADP.ArgumentOutOfRange("CompareInfo");
                }
                collation.SqlCompareOptions = value;
            }
        }

        public override System.Data.DbType DbType
        {
            get
            {
                return this.GetMetaTypeOnly().DbType;
            }
            set
            {
                MetaType type = this._metaType;
                if (((type == null) || (type.DbType != value)) || ((value == System.Data.DbType.Date) || (value == System.Data.DbType.Time)))
                {
                    this.PropertyTypeChanging();
                    this._metaType = MetaType.GetMetaTypeFromDbType(value);
                }
            }
        }

        [System.Data.ResDescription("DbParameter_Direction"), RefreshProperties(RefreshProperties.All), System.Data.ResCategory("DataCategory_Data")]
        public override ParameterDirection Direction
        {
            get
            {
                ParameterDirection direction = this._direction;
                if (direction == ((ParameterDirection) 0))
                {
                    return ParameterDirection.Input;
                }
                return direction;
            }
            set
            {
                if (this._direction != value)
                {
                    switch (value)
                    {
                        case ParameterDirection.Input:
                        case ParameterDirection.Output:
                        case ParameterDirection.InputOutput:
                        case ParameterDirection.ReturnValue:
                            this.PropertyChanging();
                            this._direction = value;
                            return;
                    }
                    throw ADP.InvalidParameterDirection(value);
                }
            }
        }

        internal MetaType InternalMetaType
        {
            get
            {
                return this._internalMetaType;
            }
            set
            {
                this._internalMetaType = value;
            }
        }

        public override bool IsNullable
        {
            get
            {
                return this._isNullable;
            }
            set
            {
                this._isNullable = value;
            }
        }

        [Browsable(false)]
        public int LocaleId
        {
            get
            {
                SqlCollation collation = this._collation;
                if (collation != null)
                {
                    return collation.LCID;
                }
                return 0;
            }
            set
            {
                SqlCollation collation = this._collation;
                if (collation == null)
                {
                    this._collation = collation = new SqlCollation();
                }
                if (value != (0xfffffL & value))
                {
                    throw ADP.ArgumentOutOfRange("LocaleId");
                }
                collation.LCID = value;
            }
        }

        private SqlMetaData MetaData
        {
            get
            {
                long fixedLength;
                MetaType metaTypeOnly = this.GetMetaTypeOnly();
                if (metaTypeOnly.IsFixed)
                {
                    fixedLength = metaTypeOnly.FixedLength;
                }
                else if ((this.Size > 0) || (this.Size < 0))
                {
                    fixedLength = this.Size;
                }
                else
                {
                    fixedLength = SmiMetaData.GetDefaultForType(metaTypeOnly.SqlDbType).MaxLength;
                }
                return new SqlMetaData(this.ParameterName, metaTypeOnly.SqlDbType, fixedLength, this.GetActualPrecision(), this.GetActualScale(), (long) this.LocaleId, this.CompareInfo, this.XmlSchemaCollectionDatabase, this.XmlSchemaCollectionOwningSchema, this.XmlSchemaCollectionName, metaTypeOnly.IsPlp, this._udtType);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced), System.Data.ResCategory("DataCategory_Data"), System.Data.ResDescription("DbParameter_Offset")]
        public int Offset
        {
            get
            {
                return this._offset;
            }
            set
            {
                if (value < 0)
                {
                    throw ADP.InvalidOffsetValue(value);
                }
                this._offset = value;
            }
        }

        internal bool ParamaterIsSqlType
        {
            get
            {
                return this._isSqlParameterSqlType;
            }
            set
            {
                this._isSqlParameterSqlType = value;
            }
        }

        [System.Data.ResDescription("SqlParameter_ParameterName"), System.Data.ResCategory("DataCategory_Data")]
        public override string ParameterName
        {
            get
            {
                string str = this._parameterName;
                if (str == null)
                {
                    return ADP.StrEmpty;
                }
                return str;
            }
            set
            {
                if ((!ADP.IsEmpty(value) && (value.Length >= 0x80)) && (('@' != value[0]) || (value.Length > 0x80)))
                {
                    throw SQL.InvalidParameterNameLength(value);
                }
                if (this._parameterName != value)
                {
                    this.PropertyChanging();
                    this._parameterName = value;
                }
            }
        }

        internal string ParameterNameFixed
        {
            get
            {
                string parameterName = this.ParameterName;
                if ((0 < parameterName.Length) && ('@' != parameterName[0]))
                {
                    parameterName = "@" + parameterName;
                }
                return parameterName;
            }
        }

        [System.Data.ResDescription("DbDataParameter_Precision"), DefaultValue((byte) 0), System.Data.ResCategory("DataCategory_Data")]
        public byte Precision
        {
            get
            {
                return this.PrecisionInternal;
            }
            set
            {
                this.PrecisionInternal = value;
            }
        }

        internal byte PrecisionInternal
        {
            get
            {
                byte num = this._precision;
                System.Data.SqlDbType metaSqlDbTypeOnly = this.GetMetaSqlDbTypeOnly();
                if ((num == 0) && (System.Data.SqlDbType.Decimal == metaSqlDbTypeOnly))
                {
                    num = this.ValuePrecision(this.SqlValue);
                }
                return num;
            }
            set
            {
                if ((this.SqlDbType == System.Data.SqlDbType.Decimal) && (value > 0x26))
                {
                    throw SQL.PrecisionValueOutOfRange(value);
                }
                if (this._precision != value)
                {
                    this.PropertyChanging();
                    this._precision = value;
                }
            }
        }

        [DefaultValue((byte) 0), System.Data.ResDescription("DbDataParameter_Scale"), System.Data.ResCategory("DataCategory_Data")]
        public byte Scale
        {
            get
            {
                return this.ScaleInternal;
            }
            set
            {
                this.ScaleInternal = value;
            }
        }

        internal byte ScaleInternal
        {
            get
            {
                byte num = this._scale;
                System.Data.SqlDbType metaSqlDbTypeOnly = this.GetMetaSqlDbTypeOnly();
                if ((num == 0) && (System.Data.SqlDbType.Decimal == metaSqlDbTypeOnly))
                {
                    num = this.ValueScale(this.SqlValue);
                }
                return num;
            }
            set
            {
                if ((this._scale != value) || !this._hasScale)
                {
                    this.PropertyChanging();
                    this._scale = value;
                    this._hasScale = true;
                }
            }
        }

        [System.Data.ResCategory("DataCategory_Data"), System.Data.ResDescription("DbParameter_Size")]
        public override int Size
        {
            get
            {
                int num = this._size;
                if (num == 0)
                {
                    num = this.ValueSize(this.Value);
                }
                return num;
            }
            set
            {
                if (this._size != value)
                {
                    if (value < -1)
                    {
                        throw ADP.InvalidSizeValue(value);
                    }
                    this.PropertyChanging();
                    this._size = value;
                }
            }
        }

        internal bool SizeInferred
        {
            get
            {
                return (0 == this._size);
            }
        }

        [System.Data.ResDescription("DbParameter_SourceColumn"), System.Data.ResCategory("DataCategory_Update")]
        public override string SourceColumn
        {
            get
            {
                string str = this._sourceColumn;
                if (str == null)
                {
                    return ADP.StrEmpty;
                }
                return str;
            }
            set
            {
                this._sourceColumn = value;
            }
        }

        public override bool SourceColumnNullMapping
        {
            get
            {
                return this._sourceColumnNullMapping;
            }
            set
            {
                this._sourceColumnNullMapping = value;
            }
        }

        [System.Data.ResDescription("DbParameter_SourceVersion"), System.Data.ResCategory("DataCategory_Update")]
        public override DataRowVersion SourceVersion
        {
            get
            {
                DataRowVersion version = this._sourceVersion;
                if (version == ((DataRowVersion) 0))
                {
                    return DataRowVersion.Current;
                }
                return version;
            }
            set
            {
                DataRowVersion version = value;
                if (version <= DataRowVersion.Current)
                {
                    switch (version)
                    {
                        case DataRowVersion.Original:
                        case DataRowVersion.Current:
                            goto Label_002C;
                    }
                    goto Label_0034;
                }
                if ((version != DataRowVersion.Proposed) && (version != DataRowVersion.Default))
                {
                    goto Label_0034;
                }
            Label_002C:
                this._sourceVersion = value;
                return;
            Label_0034:
                throw ADP.InvalidDataRowVersion(value);
            }
        }

        [System.Data.ResDescription("SqlParameter_SqlDbType"), RefreshProperties(RefreshProperties.All), System.Data.ResCategory("DataCategory_Data"), DbProviderSpecificTypeProperty(true)]
        public System.Data.SqlDbType SqlDbType
        {
            get
            {
                return this.GetMetaTypeOnly().SqlDbType;
            }
            set
            {
                MetaType type = this._metaType;
                if ((System.Data.SqlDbType.SmallInt | System.Data.SqlDbType.Int) == value)
                {
                    throw SQL.InvalidSqlDbType(value);
                }
                if ((type == null) || (type.SqlDbType != value))
                {
                    this.PropertyTypeChanging();
                    this._metaType = MetaType.GetMetaTypeFromSqlDbType(value, value == System.Data.SqlDbType.Structured);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public object SqlValue
        {
            get
            {
                if (this._udtLoadError != null)
                {
                    throw this._udtLoadError;
                }
                if (this._value != null)
                {
                    if (this._value == DBNull.Value)
                    {
                        return MetaType.GetNullSqlValue(this.GetMetaTypeOnly().SqlType);
                    }
                    if (this._value is INullable)
                    {
                        return this._value;
                    }
                    if (this._value is DateTime)
                    {
                        switch (this.GetMetaTypeOnly().SqlDbType)
                        {
                            case System.Data.SqlDbType.Date:
                            case System.Data.SqlDbType.DateTime2:
                                return this._value;
                        }
                    }
                    return MetaType.GetSqlValueFromComVariant(this._value);
                }
                if (this._sqlBufferReturnValue != null)
                {
                    return this._sqlBufferReturnValue.SqlValue;
                }
                return null;
            }
            set
            {
                this.Value = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
        public string TypeName
        {
            get
            {
                string str = this._typeName;
                if (str == null)
                {
                    return ADP.StrEmpty;
                }
                return str;
            }
            set
            {
                this._typeName = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public string UdtTypeName
        {
            get
            {
                string str = this._udtTypeName;
                if (str == null)
                {
                    return ADP.StrEmpty;
                }
                return str;
            }
            set
            {
                this._udtTypeName = value;
            }
        }

        [TypeConverter(typeof(StringConverter)), System.Data.ResCategory("DataCategory_Data"), RefreshProperties(RefreshProperties.All), System.Data.ResDescription("DbParameter_Value")]
        public override object Value
        {
            get
            {
                if (this._udtLoadError != null)
                {
                    throw this._udtLoadError;
                }
                if (this._value != null)
                {
                    return this._value;
                }
                if (this._sqlBufferReturnValue == null)
                {
                    return null;
                }
                if (this.ParamaterIsSqlType)
                {
                    return this._sqlBufferReturnValue.SqlValue;
                }
                return this._sqlBufferReturnValue.Value;
            }
            set
            {
                this._value = value;
                this._sqlBufferReturnValue = null;
                this._coercedValue = null;
                this._isSqlParameterSqlType = this._value is INullable;
                this._udtLoadError = null;
            }
        }

        [System.Data.ResCategory("DataCategory_Xml"), System.Data.ResDescription("SqlParameter_XmlSchemaCollectionDatabase")]
        public string XmlSchemaCollectionDatabase
        {
            get
            {
                string str = this._xmlSchemaCollectionDatabase;
                if (str == null)
                {
                    return ADP.StrEmpty;
                }
                return str;
            }
            set
            {
                this._xmlSchemaCollectionDatabase = value;
            }
        }

        [System.Data.ResCategory("DataCategory_Xml"), System.Data.ResDescription("SqlParameter_XmlSchemaCollectionName")]
        public string XmlSchemaCollectionName
        {
            get
            {
                string str = this._xmlSchemaCollectionName;
                if (str == null)
                {
                    return ADP.StrEmpty;
                }
                return str;
            }
            set
            {
                this._xmlSchemaCollectionName = value;
            }
        }

        [System.Data.ResDescription("SqlParameter_XmlSchemaCollectionOwningSchema"), System.Data.ResCategory("DataCategory_Xml")]
        public string XmlSchemaCollectionOwningSchema
        {
            get
            {
                string str = this._xmlSchemaCollectionOwningSchema;
                if (str == null)
                {
                    return ADP.StrEmpty;
                }
                return str;
            }
            set
            {
                this._xmlSchemaCollectionOwningSchema = value;
            }
        }

        internal sealed class SqlParameterConverter : ExpandableObjectConverter
        {
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return ((typeof(InstanceDescriptor) == destinationType) || base.CanConvertTo(context, destinationType));
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == null)
                {
                    throw ADP.ArgumentNull("destinationType");
                }
                if ((typeof(InstanceDescriptor) == destinationType) && (value is SqlParameter))
                {
                    return this.ConvertToInstanceDescriptor(value as SqlParameter);
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }

            private InstanceDescriptor ConvertToInstanceDescriptor(SqlParameter p)
            {
                object[] objArray3;
                Type[] typeArray3;
                int num = 0;
                if (p.ShouldSerializeSqlDbType())
                {
                    num |= 1;
                }
                if (p.ShouldSerializeSize())
                {
                    num |= 2;
                }
                if (!ADP.IsEmpty(p.SourceColumn))
                {
                    num |= 4;
                }
                if (p.Value != null)
                {
                    num |= 8;
                }
                if (((ParameterDirection.Input != p.Direction) || p.IsNullable) || ((p.ShouldSerializePrecision() || p.ShouldSerializeScale()) || (DataRowVersion.Current != p.SourceVersion)))
                {
                    num |= 0x10;
                }
                if ((p.SourceColumnNullMapping || !ADP.IsEmpty(p.XmlSchemaCollectionDatabase)) || (!ADP.IsEmpty(p.XmlSchemaCollectionOwningSchema) || !ADP.IsEmpty(p.XmlSchemaCollectionName)))
                {
                    num |= 0x20;
                }
                switch (num)
                {
                    case 0:
                    case 1:
                        typeArray3 = new Type[] { typeof(string), typeof(SqlDbType) };
                        objArray3 = new object[] { p.ParameterName, p.SqlDbType };
                        break;

                    case 2:
                    case 3:
                        typeArray3 = new Type[] { typeof(string), typeof(SqlDbType), typeof(int) };
                        objArray3 = new object[] { p.ParameterName, p.SqlDbType, p.Size };
                        break;

                    case 4:
                    case 5:
                    case 6:
                    case 7:
                        typeArray3 = new Type[] { typeof(string), typeof(SqlDbType), typeof(int), typeof(string) };
                        objArray3 = new object[] { p.ParameterName, p.SqlDbType, p.Size, p.SourceColumn };
                        break;

                    case 8:
                        typeArray3 = new Type[] { typeof(string), typeof(object) };
                        objArray3 = new object[] { p.ParameterName, p.Value };
                        break;

                    default:
                        if ((0x20 & num) == 0)
                        {
                            typeArray3 = new Type[] { typeof(string), typeof(SqlDbType), typeof(int), typeof(ParameterDirection), typeof(bool), typeof(byte), typeof(byte), typeof(string), typeof(DataRowVersion), typeof(object) };
                            objArray3 = new object[] { p.ParameterName, p.SqlDbType, p.Size, p.Direction, p.IsNullable, p.PrecisionInternal, p.ScaleInternal, p.SourceColumn, p.SourceVersion, p.Value };
                        }
                        else
                        {
                            typeArray3 = new Type[] { typeof(string), typeof(SqlDbType), typeof(int), typeof(ParameterDirection), typeof(byte), typeof(byte), typeof(string), typeof(DataRowVersion), typeof(bool), typeof(object), typeof(string), typeof(string), typeof(string) };
                            objArray3 = new object[] { p.ParameterName, p.SqlDbType, p.Size, p.Direction, p.PrecisionInternal, p.ScaleInternal, p.SourceColumn, p.SourceVersion, p.SourceColumnNullMapping, p.Value, p.XmlSchemaCollectionDatabase, p.XmlSchemaCollectionOwningSchema, p.XmlSchemaCollectionName };
                        }
                        break;
                }
                return new InstanceDescriptor(typeof(SqlParameter).GetConstructor(typeArray3), objArray3);
            }
        }
    }
}

