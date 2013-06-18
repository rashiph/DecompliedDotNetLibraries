namespace System.Data.OleDb
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlTypes;
    using System.Globalization;

    [TypeConverter(typeof(OleDbParameter.OleDbParameterConverter))]
    public sealed class OleDbParameter : DbParameter, ICloneable, IDbDataParameter, IDataParameter
    {
        private int _changeID;
        private object _coercedValue;
        private NativeDBType _coerceMetaType;
        private ParameterDirection _direction;
        private bool _hasScale;
        private bool _isNullable;
        private NativeDBType _metaType;
        private string _parameterName;
        private object _parent;
        private byte _precision;
        private byte _scale;
        private int _size;
        private string _sourceColumn;
        private bool _sourceColumnNullMapping;
        private DataRowVersion _sourceVersion;
        private object _value;

        public OleDbParameter()
        {
        }

        private OleDbParameter(OleDbParameter source) : this()
        {
            ADP.CheckArgumentNull(source, "source");
            source.CloneHelper(this);
            ICloneable cloneable = this._value as ICloneable;
            if (cloneable != null)
            {
                this._value = cloneable.Clone();
            }
        }

        public OleDbParameter(string name, System.Data.OleDb.OleDbType dataType) : this()
        {
            this.ParameterName = name;
            this.OleDbType = dataType;
        }

        public OleDbParameter(string name, object value) : this()
        {
            this.ParameterName = name;
            this.Value = value;
        }

        public OleDbParameter(string name, System.Data.OleDb.OleDbType dataType, int size) : this()
        {
            this.ParameterName = name;
            this.OleDbType = dataType;
            this.Size = size;
        }

        public OleDbParameter(string name, System.Data.OleDb.OleDbType dataType, int size, string srcColumn) : this()
        {
            this.ParameterName = name;
            this.OleDbType = dataType;
            this.Size = size;
            this.SourceColumn = srcColumn;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public OleDbParameter(string parameterName, System.Data.OleDb.OleDbType dbType, int size, ParameterDirection direction, bool isNullable, byte precision, byte scale, string srcColumn, DataRowVersion srcVersion, object value) : this()
        {
            this.ParameterName = parameterName;
            this.OleDbType = dbType;
            this.Size = size;
            this.Direction = direction;
            this.IsNullable = isNullable;
            this.PrecisionInternal = precision;
            this.ScaleInternal = scale;
            this.SourceColumn = srcColumn;
            this.SourceVersion = srcVersion;
            this.Value = value;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public OleDbParameter(string parameterName, System.Data.OleDb.OleDbType dbType, int size, ParameterDirection direction, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, bool sourceColumnNullMapping, object value) : this()
        {
            this.ParameterName = parameterName;
            this.OleDbType = dbType;
            this.Size = size;
            this.Direction = direction;
            this.PrecisionInternal = precision;
            this.ScaleInternal = scale;
            this.SourceColumn = sourceColumn;
            this.SourceVersion = sourceVersion;
            this.SourceColumnNullMapping = sourceColumnNullMapping;
            this.Value = value;
        }

        internal bool BindParameter(int index, Bindings bindings)
        {
            int size;
            int ptrSize;
            byte precisionInternal;
            byte scaleInternal;
            object obj2 = this.Value;
            NativeDBType bindType = this.GetBindType(obj2);
            if (bindType.enumOleDbType == System.Data.OleDb.OleDbType.Empty)
            {
                throw ODB.UninitializedParameters(index, bindType.enumOleDbType);
            }
            this._coerceMetaType = bindType;
            obj2 = CoerceValue(obj2, bindType);
            this.CoercedValue = obj2;
            ParameterDirection direction = this.Direction;
            if (this.ShouldSerializePrecision())
            {
                precisionInternal = this.PrecisionInternal;
            }
            else
            {
                precisionInternal = this.ValuePrecision(obj2);
            }
            if (precisionInternal == 0)
            {
                precisionInternal = bindType.maxpre;
            }
            if (this.ShouldSerializeScale())
            {
                scaleInternal = this.ScaleInternal;
            }
            else
            {
                scaleInternal = this.ValueScale(obj2);
            }
            int wType = bindType.wType;
            if (bindType.islong)
            {
                ptrSize = ADP.PtrSize;
                if (this.ShouldSerializeSize())
                {
                    size = this.Size;
                }
                else if (0x81 == bindType.dbType)
                {
                    size = 0x7fffffff;
                }
                else if (130 == bindType.dbType)
                {
                    size = 0x3fffffff;
                }
                else
                {
                    size = 0x7fffffff;
                }
                wType |= 0x4000;
            }
            else if (bindType.IsVariableLength)
            {
                bool flag;
                if (!this.ShouldSerializeSize() && ADP.IsDirection(this, ParameterDirection.Output))
                {
                    throw ADP.UninitializedParameterSize(index, this._coerceMetaType.dataType);
                }
                if (this.ShouldSerializeSize())
                {
                    size = this.Size;
                    flag = false;
                }
                else
                {
                    size = this.ValueSize(obj2);
                    flag = true;
                }
                if (0 >= size)
                {
                    if (size != 0)
                    {
                        if (-1 != size)
                        {
                            throw ADP.InvalidSizeValue(size);
                        }
                        ptrSize = ADP.PtrSize;
                        wType |= 0x4000;
                    }
                    else if (130 == wType)
                    {
                        ptrSize = 2;
                    }
                    else
                    {
                        ptrSize = 0;
                    }
                }
                else
                {
                    if (130 == bindType.wType)
                    {
                        ptrSize = (Math.Min(size, 0x3ffffffe) * 2) + 2;
                    }
                    else
                    {
                        ptrSize = size;
                    }
                    if (flag && (0x81 == bindType.dbType))
                    {
                        size = Math.Min(size, 0x3ffffffe) * 2;
                    }
                    if (0x2000 < ptrSize)
                    {
                        ptrSize = ADP.PtrSize;
                        wType |= 0x4000;
                    }
                }
            }
            else
            {
                ptrSize = bindType.fixlen;
                size = ptrSize;
            }
            bindings.CurrentIndex = index;
            bindings.DataSourceType = bindType.dbString.DangerousGetHandle();
            bindings.Name = ADP.PtrZero;
            bindings.ParamSize = new IntPtr(size);
            bindings.Flags = GetBindFlags(direction);
            bindings.Ordinal = (IntPtr) (index + 1);
            bindings.Part = bindType.dbPart;
            bindings.ParamIO = GetBindDirection(direction);
            bindings.Precision = precisionInternal;
            bindings.Scale = scaleInternal;
            bindings.DbType = wType;
            bindings.MaxLen = ptrSize;
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oledb.struct.tagDBPARAMBINDINFO|INFO|ADV> index=%d, parameterName='%ls'\n", index, this.ParameterName);
                Bid.Trace("<oledb.struct.tagDBBINDING|INFO|ADV>\n");
            }
            return this.IsParameterComputed();
        }

        private void CloneHelper(OleDbParameter destination)
        {
            this.CloneHelperCore(destination);
            destination._metaType = this._metaType;
            destination._parameterName = this._parameterName;
            destination._precision = this._precision;
            destination._scale = this._scale;
            destination._hasScale = this._hasScale;
        }

        private void CloneHelperCore(OleDbParameter destination)
        {
            destination._value = this._value;
            destination._direction = this._direction;
            destination._size = this._size;
            destination._sourceColumn = this._sourceColumn;
            destination._sourceVersion = this._sourceVersion;
            destination._sourceColumnNullMapping = this._sourceColumnNullMapping;
            destination._isNullable = this._isNullable;
        }

        private static object CoerceValue(object value, NativeDBType destinationType)
        {
            if (((value != null) && (DBNull.Value != value)) && (typeof(object) != destinationType.dataType))
            {
                Type type = value.GetType();
                if (!(type != destinationType.dataType))
                {
                    return value;
                }
                try
                {
                    if ((typeof(string) == destinationType.dataType) && (typeof(char[]) == type))
                    {
                        return value;
                    }
                    if ((6 == destinationType.dbType) && (typeof(string) == type))
                    {
                        value = decimal.Parse((string) value, NumberStyles.Currency, null);
                        return value;
                    }
                    value = Convert.ChangeType(value, destinationType.dataType, null);
                }
                catch (Exception exception)
                {
                    if (!ADP.IsCatchableExceptionType(exception))
                    {
                        throw;
                    }
                    throw ADP.ParameterConversionFailed(value, destinationType.dataType, exception);
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
            this.CloneHelper((OleDbParameter) destination);
        }

        private static int GetBindDirection(ParameterDirection direction)
        {
            return (int) (ParameterDirection.InputOutput & direction);
        }

        private static int GetBindFlags(ParameterDirection direction)
        {
            return (int) (ParameterDirection.InputOutput & direction);
        }

        private NativeDBType GetBindType(object value)
        {
            NativeDBType type = this._metaType;
            if (type != null)
            {
                return type;
            }
            if (ADP.IsNull(value))
            {
                return NativeDBType.Default;
            }
            return NativeDBType.FromSystemType(value);
        }

        internal object GetCoercedValue()
        {
            object coercedValue = this.CoercedValue;
            if (coercedValue == null)
            {
                coercedValue = CoerceValue(this.Value, this._coerceMetaType);
                this.CoercedValue = coercedValue;
            }
            return coercedValue;
        }

        internal bool IsParameterComputed()
        {
            NativeDBType type = this._metaType;
            if ((type != null) && (this.ShouldSerializeSize() || !type.IsVariableLength))
            {
                if (14 == type.dbType)
                {
                    return true;
                }
                if (0x83 != type.dbType)
                {
                    return false;
                }
                if (this.ShouldSerializeScale())
                {
                    return !this.ShouldSerializePrecision();
                }
            }
            return true;
        }

        internal void Prepare(OleDbCommand cmd)
        {
            if (this._metaType == null)
            {
                throw ADP.PrepareParameterType(cmd);
            }
            if (!this.ShouldSerializeSize() && this._metaType.IsVariableLength)
            {
                throw ADP.PrepareParameterSize(cmd);
            }
            if ((!this.ShouldSerializePrecision() && !this.ShouldSerializeScale()) && ((14 == this._metaType.wType) || (0x83 == this._metaType.wType)))
            {
                throw ADP.PrepareParameterScale(cmd, this._metaType.wType.ToString("G", CultureInfo.InvariantCulture));
            }
        }

        private void PropertyChanging()
        {
            this._changeID++;
        }

        private void PropertyTypeChanging()
        {
            this.PropertyChanging();
            this._coerceMetaType = null;
            this.CoercedValue = null;
        }

        public override void ResetDbType()
        {
            this.ResetOleDbType();
        }

        public void ResetOleDbType()
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

        private bool ShouldSerializeOleDbType()
        {
            return (null != this._metaType);
        }

        private bool ShouldSerializePrecision()
        {
            return (0 != this._precision);
        }

        private bool ShouldSerializeScale()
        {
            return this.ShouldSerializeScale(this._scale);
        }

        private bool ShouldSerializeScale(byte scale)
        {
            if (!this._hasScale)
            {
                return false;
            }
            if (scale == 0)
            {
                return this.ShouldSerializePrecision();
            }
            return true;
        }

        private bool ShouldSerializeSize()
        {
            return (0 != this._size);
        }

        object ICloneable.Clone()
        {
            return new OleDbParameter(this);
        }

        public override string ToString()
        {
            return this.ParameterName;
        }

        private byte ValuePrecision(object value)
        {
            return this.ValuePrecisionCore(value);
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
            return this.ValueScaleCore(value);
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
            return this.ValueSizeCore(value);
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

        internal int ChangeID
        {
            get
            {
                return this._changeID;
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

        public override System.Data.DbType DbType
        {
            get
            {
                return this.GetBindType(this.Value).enumDbType;
            }
            set
            {
                NativeDBType type = this._metaType;
                if ((type == null) || (type.enumDbType != value))
                {
                    this.PropertyTypeChanging();
                    this._metaType = NativeDBType.FromDbType(value);
                }
            }
        }

        [RefreshProperties(RefreshProperties.All), ResDescription("DbParameter_Direction"), ResCategory("DataCategory_Data")]
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

        internal int Offset
        {
            get
            {
                return 0;
            }
        }

        [ResCategory("DataCategory_Data"), RefreshProperties(RefreshProperties.All), ResDescription("OleDbParameter_OleDbType"), DbProviderSpecificTypeProperty(true)]
        public System.Data.OleDb.OleDbType OleDbType
        {
            get
            {
                return this.GetBindType(this.Value).enumOleDbType;
            }
            set
            {
                NativeDBType type = this._metaType;
                if ((type == null) || (type.enumOleDbType != value))
                {
                    this.PropertyTypeChanging();
                    this._metaType = NativeDBType.FromDataType(value);
                }
            }
        }

        [ResCategory("DataCategory_Data"), ResDescription("DbParameter_ParameterName")]
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
                if (this._parameterName != value)
                {
                    this.PropertyChanging();
                    this._parameterName = value;
                }
            }
        }

        [ResDescription("DbDataParameter_Precision"), DefaultValue((byte) 0), ResCategory("DataCategory_Data")]
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
                if (num == 0)
                {
                    num = this.ValuePrecision(this.Value);
                }
                return num;
            }
            set
            {
                if (this._precision != value)
                {
                    this.PropertyChanging();
                    this._precision = value;
                }
            }
        }

        [DefaultValue((byte) 0), ResCategory("DataCategory_Data"), ResDescription("DbDataParameter_Scale")]
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
                byte scale = this._scale;
                if (!this.ShouldSerializeScale(scale))
                {
                    scale = this.ValueScale(this.Value);
                }
                return scale;
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

        [ResDescription("DbParameter_Size"), ResCategory("DataCategory_Data")]
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

        [ResCategory("DataCategory_Update"), ResDescription("DbParameter_SourceColumn")]
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

        [ResCategory("DataCategory_Update"), ResDescription("DbParameter_SourceVersion")]
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

        [RefreshProperties(RefreshProperties.All), ResCategory("DataCategory_Data"), TypeConverter(typeof(StringConverter)), ResDescription("DbParameter_Value")]
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

        internal sealed class OleDbParameterConverter : ExpandableObjectConverter
        {
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return ((typeof(InstanceDescriptor) == destinationType) || base.CanConvertTo(context, destinationType));
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (null == destinationType)
                {
                    throw ADP.ArgumentNull("destinationType");
                }
                if ((typeof(InstanceDescriptor) == destinationType) && (value is OleDbParameter))
                {
                    return this.ConvertToInstanceDescriptor(value as OleDbParameter);
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }

            private InstanceDescriptor ConvertToInstanceDescriptor(OleDbParameter p)
            {
                object[] objArray3;
                Type[] typeArray3;
                int num = 0;
                if (p.ShouldSerializeOleDbType())
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
                if (p.SourceColumnNullMapping)
                {
                    num |= 0x20;
                }
                switch (num)
                {
                    case 0:
                    case 1:
                        typeArray3 = new Type[] { typeof(string), typeof(OleDbType) };
                        objArray3 = new object[] { p.ParameterName, p.OleDbType };
                        break;

                    case 2:
                    case 3:
                        typeArray3 = new Type[] { typeof(string), typeof(OleDbType), typeof(int) };
                        objArray3 = new object[] { p.ParameterName, p.OleDbType, p.Size };
                        break;

                    case 4:
                    case 5:
                    case 6:
                    case 7:
                        typeArray3 = new Type[] { typeof(string), typeof(OleDbType), typeof(int), typeof(string) };
                        objArray3 = new object[] { p.ParameterName, p.OleDbType, p.Size, p.SourceColumn };
                        break;

                    case 8:
                        typeArray3 = new Type[] { typeof(string), typeof(object) };
                        objArray3 = new object[] { p.ParameterName, p.Value };
                        break;

                    default:
                        if ((0x20 & num) == 0)
                        {
                            typeArray3 = new Type[] { typeof(string), typeof(OleDbType), typeof(int), typeof(ParameterDirection), typeof(bool), typeof(byte), typeof(byte), typeof(string), typeof(DataRowVersion), typeof(object) };
                            objArray3 = new object[] { p.ParameterName, p.OleDbType, p.Size, p.Direction, p.IsNullable, p.PrecisionInternal, p.ScaleInternal, p.SourceColumn, p.SourceVersion, p.Value };
                        }
                        else
                        {
                            typeArray3 = new Type[] { typeof(string), typeof(OleDbType), typeof(int), typeof(ParameterDirection), typeof(byte), typeof(byte), typeof(string), typeof(DataRowVersion), typeof(bool), typeof(object) };
                            objArray3 = new object[] { p.ParameterName, p.OleDbType, p.Size, p.Direction, p.PrecisionInternal, p.ScaleInternal, p.SourceColumn, p.SourceVersion, p.SourceColumnNullMapping, p.Value };
                        }
                        break;
                }
                return new InstanceDescriptor(typeof(OleDbParameter).GetConstructor(typeArray3), objArray3);
            }
        }
    }
}

