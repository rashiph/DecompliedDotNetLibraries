namespace System.Data.OracleClient
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlTypes;
    using System.Globalization;

    [TypeConverter(typeof(OracleParameter.OracleParameterConverter))]
    public sealed class OracleParameter : DbParameter, ICloneable, IDbDataParameter, IDataParameter
    {
        private MetaType _coercedMetaType;
        private object _coercedValue;
        private int _commandSetResult;
        private ParameterDirection _direction;
        private bool _hasScale;
        private bool _isNullable;
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
        private object _value;

        public OracleParameter()
        {
        }

        private OracleParameter(OracleParameter source) : this()
        {
            System.Data.Common.ADP.CheckArgumentNull(source, "source");
            source.CloneHelper(this);
            ICloneable cloneable = this._value as ICloneable;
            if (cloneable != null)
            {
                this._value = cloneable.Clone();
            }
        }

        public OracleParameter(string name, System.Data.OracleClient.OracleType oracleType) : this()
        {
            this.ParameterName = name;
            this.OracleType = oracleType;
        }

        public OracleParameter(string name, object value)
        {
            this.ParameterName = name;
            this.Value = value;
        }

        public OracleParameter(string name, System.Data.OracleClient.OracleType oracleType, int size) : this()
        {
            this.ParameterName = name;
            this.OracleType = oracleType;
            this.Size = size;
        }

        public OracleParameter(string name, System.Data.OracleClient.OracleType oracleType, int size, string srcColumn) : this()
        {
            this.ParameterName = name;
            this.OracleType = oracleType;
            this.Size = size;
            this.SourceColumn = srcColumn;
        }

        public OracleParameter(string name, System.Data.OracleClient.OracleType oracleType, int size, ParameterDirection direction, string sourceColumn, DataRowVersion sourceVersion, bool sourceColumnNullMapping, object value) : this()
        {
            this.ParameterName = name;
            this.OracleType = oracleType;
            this.Size = size;
            this.Direction = direction;
            this.SourceColumn = sourceColumn;
            this.SourceVersion = sourceVersion;
            this.SourceColumnNullMapping = sourceColumnNullMapping;
            this.Value = value;
        }

        public OracleParameter(string name, System.Data.OracleClient.OracleType oracleType, int size, ParameterDirection direction, bool isNullable, byte precision, byte scale, string srcColumn, DataRowVersion srcVersion, object value) : this()
        {
            this.ParameterName = name;
            this.OracleType = oracleType;
            this.Size = size;
            this.Direction = direction;
            this.IsNullable = isNullable;
            this.PrecisionInternal = precision;
            this.ScaleInternal = scale;
            this.SourceColumn = srcColumn;
            this.SourceVersion = srcVersion;
            this.Value = value;
        }

        private void CloneHelper(OracleParameter destination)
        {
            this.CloneHelperCore(destination);
            destination._metaType = this._metaType;
            destination._parameterName = this._parameterName;
            destination._precision = this._precision;
            destination._scale = this._scale;
            destination._hasScale = this._hasScale;
        }

        private void CloneHelperCore(OracleParameter destination)
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

        private static object CoerceValue(object value, MetaType destinationType)
        {
            if (((value != null) && !Convert.IsDBNull(value)) && (typeof(object) != destinationType.BaseType))
            {
                Type type = value.GetType();
                if (!(type != destinationType.BaseType) || !(type != destinationType.NoConvertType))
                {
                    return value;
                }
                try
                {
                    if ((typeof(string) == destinationType.BaseType) && (typeof(char[]) == type))
                    {
                        value = new string((char[]) value);
                        return value;
                    }
                    if ((System.Data.DbType.Currency == destinationType.DbType) && (typeof(string) == type))
                    {
                        value = decimal.Parse((string) value, NumberStyles.Currency, null);
                        return value;
                    }
                    value = Convert.ChangeType(value, destinationType.BaseType, null);
                }
                catch (Exception exception)
                {
                    if (!System.Data.Common.ADP.IsCatchableExceptionType(exception))
                    {
                        throw;
                    }
                    throw System.Data.Common.ADP.ParameterConversionFailed(value, destinationType.BaseType, exception);
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
            System.Data.Common.ADP.CheckArgumentNull(destination, "destination");
            this.CloneHelper((OracleParameter) destination);
        }

        internal int GetActualSize()
        {
            if (!this.ShouldSerializeSize())
            {
                return this.ValueSize(this.CoercedValue);
            }
            return this.Size;
        }

        internal object GetCoercedValueInternal()
        {
            object coercedValue = this.CoercedValue;
            if (coercedValue == null)
            {
                coercedValue = CoerceValue(this.Value, this._coercedMetaType);
                this.CoercedValue = coercedValue;
            }
            return coercedValue;
        }

        private MetaType GetMetaType()
        {
            return this.GetMetaType(this.Value);
        }

        internal MetaType GetMetaType(object value)
        {
            MetaType metaTypeForObject = this._metaType;
            if (metaTypeForObject == null)
            {
                if ((value != null) && !Convert.IsDBNull(value))
                {
                    metaTypeForObject = MetaType.GetMetaTypeForObject(value);
                }
                else
                {
                    metaTypeForObject = MetaType.GetDefaultMetaType();
                }
                this._metaType = metaTypeForObject;
            }
            return metaTypeForObject;
        }

        private void PropertyChanging()
        {
        }

        private void PropertyTypeChanging()
        {
            this.PropertyChanging();
            this.CoercedValue = null;
        }

        public override void ResetDbType()
        {
            this.ResetOracleType();
        }

        public void ResetOracleType()
        {
            if (this._metaType != null)
            {
                this.PropertyTypeChanging();
                this._metaType = null;
            }
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

        internal void SetCoercedValueInternal(object value, MetaType metaType)
        {
            this._coercedMetaType = metaType;
            this.CoercedValue = CoerceValue(value, metaType);
        }

        private bool ShouldSerializeOracleType()
        {
            return (null != this._metaType);
        }

        private bool ShouldSerializePrecision()
        {
            return (0 != this._precision);
        }

        private bool ShouldSerializeScale()
        {
            return this._hasScale;
        }

        private bool ShouldSerializeSize()
        {
            return (0 != this._size);
        }

        object ICloneable.Clone()
        {
            return new OracleParameter(this);
        }

        public override string ToString()
        {
            return this.ParameterName;
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
            if (value is OracleString)
            {
                OracleString str = (OracleString) value;
                return str.Length;
            }
            if (value is string)
            {
                return ((string) value).Length;
            }
            if (value is char[])
            {
                return ((char[]) value).Length;
            }
            if (value is OracleBinary)
            {
                OracleBinary binary = (OracleBinary) value;
                return binary.Length;
            }
            return this.ValueSizeCore(value);
        }

        private int ValueSizeCore(object value)
        {
            if (!System.Data.Common.ADP.IsNull(value))
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

        internal int BindSize
        {
            get
            {
                int actualSize = this.GetActualSize();
                if ((0x7fff < actualSize) && (ParameterDirection.Input == this.Direction))
                {
                    actualSize = this.ValueSize(this.GetCoercedValueInternal());
                }
                return actualSize;
            }
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

        internal int CommandSetResult
        {
            get
            {
                return this._commandSetResult;
            }
            set
            {
                this._commandSetResult = value;
            }
        }

        public override System.Data.DbType DbType
        {
            get
            {
                return this.GetMetaType().DbType;
            }
            set
            {
                if ((this._metaType == null) || (this._metaType.DbType != value))
                {
                    this.PropertyTypeChanging();
                    this._metaType = MetaType.GetMetaTypeForType(value);
                }
            }
        }

        [System.Data.OracleClient.ResCategory("DataCategory_Data"), RefreshProperties(RefreshProperties.All), System.Data.OracleClient.ResDescription("DbParameter_Direction")]
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
                    throw System.Data.Common.ADP.InvalidParameterDirection(value);
                }
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

        [System.Data.OracleClient.ResCategory("DataCategory_Data"), System.Data.OracleClient.ResDescription("DbParameter_Offset"), EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
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
                    throw System.Data.Common.ADP.InvalidOffsetValue(value);
                }
                this._offset = value;
            }
        }

        [DbProviderSpecificTypeProperty(true), RefreshProperties(RefreshProperties.All), DefaultValue(0x16), System.Data.OracleClient.ResDescription("OracleParameter_OracleType"), System.Data.OracleClient.ResCategory("OracleCategory_Data")]
        public System.Data.OracleClient.OracleType OracleType
        {
            get
            {
                return this.GetMetaType().OracleType;
            }
            set
            {
                MetaType type = this._metaType;
                if ((type == null) || (type.OracleType != value))
                {
                    this.PropertyTypeChanging();
                    this._metaType = MetaType.GetMetaTypeForType(value);
                }
            }
        }

        [System.Data.OracleClient.ResDescription("DbParameter_ParameterName"), System.Data.OracleClient.ResCategory("DataCategory_Data")]
        public override string ParameterName
        {
            get
            {
                string str = this._parameterName;
                if (str == null)
                {
                    return System.Data.Common.ADP.StrEmpty;
                }
                return str;
            }
            set
            {
                if (this._parameterName != value)
                {
                    this._parameterName = value;
                }
            }
        }

        [Obsolete("Precision has been deprecated.  Use the Math classes to explicitly set the precision of a decimal.  http://go.microsoft.com/fwlink/?linkid=14202"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        private byte PrecisionInternal
        {
            get
            {
                return this._precision;
            }
            set
            {
                if (this._precision != value)
                {
                    this._precision = value;
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), Obsolete("Scale has been deprecated.  Use the Math classes to explicitly set the scale of a decimal.  http://go.microsoft.com/fwlink/?linkid=14202")]
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

        private byte ScaleInternal
        {
            get
            {
                return this._scale;
            }
            set
            {
                if ((this._scale != value) || !this._hasScale)
                {
                    this._scale = value;
                    this._hasScale = true;
                }
            }
        }

        [System.Data.OracleClient.ResDescription("DbParameter_Size"), System.Data.OracleClient.ResCategory("DataCategory_Data")]
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
                        throw System.Data.Common.ADP.InvalidSizeValue(value);
                    }
                    this.PropertyChanging();
                    this._size = value;
                }
            }
        }

        [System.Data.OracleClient.ResDescription("DbParameter_SourceColumn"), System.Data.OracleClient.ResCategory("DataCategory_Update")]
        public override string SourceColumn
        {
            get
            {
                string str = this._sourceColumn;
                if (str == null)
                {
                    return System.Data.Common.ADP.StrEmpty;
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

        [System.Data.OracleClient.ResDescription("DbParameter_SourceVersion"), System.Data.OracleClient.ResCategory("DataCategory_Update")]
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
                throw System.Data.Common.ADP.InvalidDataRowVersion(value);
            }
        }

        [System.Data.OracleClient.ResDescription("DbParameter_Value"), System.Data.OracleClient.ResCategory("DataCategory_Data"), TypeConverter(typeof(StringConverter)), RefreshProperties(RefreshProperties.All)]
        public override object Value
        {
            get
            {
                return this._value;
            }
            set
            {
                this._coercedValue = null;
                this._value = value;
            }
        }

        internal sealed class OracleParameterConverter : ExpandableObjectConverter
        {
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return ((destinationType == typeof(InstanceDescriptor)) || base.CanConvertTo(context, destinationType));
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == null)
                {
                    throw System.Data.Common.ADP.ArgumentNull("destinationType");
                }
                if ((destinationType == typeof(InstanceDescriptor)) && (value is OracleParameter))
                {
                    return this.ConvertToInstanceDescriptor(value as OracleParameter);
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }

            private InstanceDescriptor ConvertToInstanceDescriptor(OracleParameter p)
            {
                object[] objArray3;
                Type[] typeArray3;
                int num = 0;
                if (p.ShouldSerializeOracleType())
                {
                    num |= 1;
                }
                if (p.ShouldSerializeSize())
                {
                    num |= 2;
                }
                if (!System.Data.Common.ADP.IsEmpty(p.SourceColumn))
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
                if (p.SourceColumnNullMapping)
                {
                    num |= 0x20;
                }
                switch (num)
                {
                    case 0:
                    case 1:
                        typeArray3 = new Type[] { typeof(string), typeof(OracleType) };
                        objArray3 = new object[] { p.ParameterName, p.OracleType };
                        break;

                    case 2:
                    case 3:
                        typeArray3 = new Type[] { typeof(string), typeof(OracleType), typeof(int) };
                        objArray3 = new object[] { p.ParameterName, p.OracleType, p.Size };
                        break;

                    case 4:
                    case 5:
                    case 6:
                    case 7:
                        typeArray3 = new Type[] { typeof(string), typeof(OracleType), typeof(int), typeof(string) };
                        objArray3 = new object[] { p.ParameterName, p.OracleType, p.Size, p.SourceColumn };
                        break;

                    case 8:
                        typeArray3 = new Type[] { typeof(string), typeof(object) };
                        objArray3 = new object[] { p.ParameterName, p.Value };
                        break;

                    default:
                        if ((0x20 & num) == 0)
                        {
                            typeArray3 = new Type[] { typeof(string), typeof(OracleType), typeof(int), typeof(ParameterDirection), typeof(bool), typeof(byte), typeof(byte), typeof(string), typeof(DataRowVersion), typeof(object) };
                            objArray3 = new object[] { p.ParameterName, p.OracleType, p.Size, p.Direction, p.IsNullable, p.PrecisionInternal, p.ScaleInternal, p.SourceColumn, p.SourceVersion, p.Value };
                        }
                        else
                        {
                            typeArray3 = new Type[] { typeof(string), typeof(OracleType), typeof(int), typeof(ParameterDirection), typeof(string), typeof(DataRowVersion), typeof(bool), typeof(object) };
                            objArray3 = new object[] { p.ParameterName, p.OracleType, p.Size, p.Direction, p.SourceColumn, p.SourceVersion, p.SourceColumnNullMapping, p.Value };
                        }
                        break;
                }
                return new InstanceDescriptor(typeof(OracleParameter).GetConstructor(typeArray3), objArray3);
            }
        }
    }
}

