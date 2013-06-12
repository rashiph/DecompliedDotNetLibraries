namespace System.Data.Odbc
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlTypes;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;

    [TypeConverter(typeof(OdbcParameter.OdbcParameterConverter))]
    public sealed class OdbcParameter : DbParameter, ICloneable, IDbDataParameter, IDataParameter
    {
        private TypeMap _bindtype;
        private IntPtr _boundBuffer;
        private IntPtr _boundIntbuffer;
        private ODBC32.SQL_TYPE _boundParameterType;
        private int _boundScale;
        private int _boundSize;
        private ODBC32.SQL_C _boundSqlCType;
        private object _coercedValue;
        private ParameterDirection _direction;
        private bool _hasChanged;
        private bool _hasScale;
        private ParameterDirection _internalDirection;
        private int _internalOffset;
        private byte _internalPrecision;
        private byte _internalScale;
        private bool _internalShouldSerializeSize;
        private int _internalSize;
        internal bool _internalUserSpecifiedType;
        private object _internalValue;
        private bool _isNullable;
        private TypeMap _originalbindtype;
        private string _parameterName;
        private object _parent;
        private byte _precision;
        private ODBC32.SQL_C _prepared_Sql_C_Type;
        private int _preparedBufferSize;
        private int _preparedIntOffset;
        private int _preparedOffset;
        private int _preparedSize;
        private object _preparedValue;
        private int _preparedValueOffset;
        private byte _scale;
        private int _size;
        private string _sourceColumn;
        private bool _sourceColumnNullMapping;
        private DataRowVersion _sourceVersion;
        private TypeMap _typemap;
        private bool _userSpecifiedType;
        private object _value;

        public OdbcParameter()
        {
        }

        private OdbcParameter(OdbcParameter source) : this()
        {
            ADP.CheckArgumentNull(source, "source");
            source.CloneHelper(this);
            ICloneable cloneable = this._value as ICloneable;
            if (cloneable != null)
            {
                this._value = cloneable.Clone();
            }
        }

        public OdbcParameter(string name, System.Data.Odbc.OdbcType type) : this()
        {
            this.ParameterName = name;
            this.OdbcType = type;
        }

        public OdbcParameter(string name, object value) : this()
        {
            this.ParameterName = name;
            this.Value = value;
        }

        public OdbcParameter(string name, System.Data.Odbc.OdbcType type, int size) : this()
        {
            this.ParameterName = name;
            this.OdbcType = type;
            this.Size = size;
        }

        public OdbcParameter(string name, System.Data.Odbc.OdbcType type, int size, string sourcecolumn) : this()
        {
            this.ParameterName = name;
            this.OdbcType = type;
            this.Size = size;
            this.SourceColumn = sourcecolumn;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public OdbcParameter(string parameterName, System.Data.Odbc.OdbcType odbcType, int size, ParameterDirection parameterDirection, bool isNullable, byte precision, byte scale, string srcColumn, DataRowVersion srcVersion, object value) : this()
        {
            this.ParameterName = parameterName;
            this.OdbcType = odbcType;
            this.Size = size;
            this.Direction = parameterDirection;
            this.IsNullable = isNullable;
            this.PrecisionInternal = precision;
            this.ScaleInternal = scale;
            this.SourceColumn = srcColumn;
            this.SourceVersion = srcVersion;
            this.Value = value;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public OdbcParameter(string parameterName, System.Data.Odbc.OdbcType odbcType, int size, ParameterDirection parameterDirection, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, bool sourceColumnNullMapping, object value) : this()
        {
            this.ParameterName = parameterName;
            this.OdbcType = odbcType;
            this.Size = size;
            this.Direction = parameterDirection;
            this.PrecisionInternal = precision;
            this.ScaleInternal = scale;
            this.SourceColumn = sourceColumn;
            this.SourceVersion = sourceVersion;
            this.SourceColumnNullMapping = sourceColumnNullMapping;
            this.Value = value;
        }

        internal void Bind(OdbcStatementHandle hstmt, OdbcCommand command, short ordinal, CNativeBuffer parameterBuffer, bool allowReentrance)
        {
            ODBC32.SQL_C sql_c = this._prepared_Sql_C_Type;
            ODBC32.SQL_PARAM sql_param = this.SqlDirectionFromParameterDirection();
            int offset = this._preparedOffset;
            int sizeorprecision = this._preparedSize;
            object obj2 = this._preparedValue;
            int valueSize = this.GetValueSize(obj2, offset);
            int num4 = this.GetColumnSize(obj2, offset, ordinal);
            byte parameterPrecision = this.GetParameterPrecision(obj2);
            byte parameterScale = this.GetParameterScale(obj2);
            HandleRef buffer = parameterBuffer.PtrOffset(this._preparedValueOffset, this._preparedBufferSize);
            HandleRef intbuffer = parameterBuffer.PtrOffset(this._preparedIntOffset, IntPtr.Size);
            if (ODBC32.SQL_C.NUMERIC == sql_c)
            {
                if (((ODBC32.SQL_PARAM.INPUT_OUTPUT == sql_param) && (obj2 is decimal)) && (parameterScale < this._internalScale))
                {
                    while (parameterScale < this._internalScale)
                    {
                        obj2 = ((decimal) obj2) * 10M;
                        parameterScale = (byte) (parameterScale + 1);
                    }
                }
                this.SetInputValue(obj2, sql_c, valueSize, parameterPrecision, 0, parameterBuffer);
                if (ODBC32.SQL_PARAM.INPUT != sql_param)
                {
                    parameterBuffer.WriteInt16(this._preparedValueOffset, (short) ((parameterScale << 8) | parameterPrecision));
                }
            }
            else
            {
                this.SetInputValue(obj2, sql_c, valueSize, sizeorprecision, offset, parameterBuffer);
            }
            if (((this._hasChanged || (this._boundSqlCType != sql_c)) || ((this._boundParameterType != this._bindtype._sql_type) || (this._boundSize != num4))) || (((this._boundScale != parameterScale) || (this._boundBuffer != buffer.Handle)) || (this._boundIntbuffer != intbuffer.Handle)))
            {
                ODBC32.RetCode retcode = hstmt.BindParameter(ordinal, (short) sql_param, sql_c, this._bindtype._sql_type, (IntPtr) num4, (IntPtr) parameterScale, buffer, (IntPtr) this._preparedBufferSize, intbuffer);
                if (retcode != ODBC32.RetCode.SUCCESS)
                {
                    if ("07006" == command.GetDiagSqlState())
                    {
                        Bid.Trace("<odbc.OdbcParameter.Bind|ERR> Call to BindParameter returned errorcode [07006]\n");
                        command.Connection.FlagRestrictedSqlBindType(this._bindtype._sql_type);
                        if (allowReentrance)
                        {
                            this.Bind(hstmt, command, ordinal, parameterBuffer, false);
                            return;
                        }
                    }
                    command.Connection.HandleError(hstmt, retcode);
                }
                this._hasChanged = false;
                this._boundSqlCType = sql_c;
                this._boundParameterType = this._bindtype._sql_type;
                this._boundSize = num4;
                this._boundScale = parameterScale;
                this._boundBuffer = buffer.Handle;
                this._boundIntbuffer = intbuffer.Handle;
                if (ODBC32.SQL_C.NUMERIC == sql_c)
                {
                    OdbcDescriptorHandle descriptorHandle = command.GetDescriptorHandle(ODBC32.SQL_ATTR.APP_PARAM_DESC);
                    retcode = descriptorHandle.SetDescriptionField1(ordinal, ODBC32.SQL_DESC.TYPE, (IntPtr) 2L);
                    if (retcode != ODBC32.RetCode.SUCCESS)
                    {
                        command.Connection.HandleError(hstmt, retcode);
                    }
                    int num2 = parameterPrecision;
                    retcode = descriptorHandle.SetDescriptionField1(ordinal, ODBC32.SQL_DESC.PRECISION, (IntPtr) num2);
                    if (retcode != ODBC32.RetCode.SUCCESS)
                    {
                        command.Connection.HandleError(hstmt, retcode);
                    }
                    num2 = parameterScale;
                    retcode = descriptorHandle.SetDescriptionField1(ordinal, ODBC32.SQL_DESC.SCALE, (IntPtr) num2);
                    if (retcode != ODBC32.RetCode.SUCCESS)
                    {
                        command.Connection.HandleError(hstmt, retcode);
                    }
                    retcode = descriptorHandle.SetDescriptionField2(ordinal, ODBC32.SQL_DESC.DATA_PTR, buffer);
                    if (retcode != ODBC32.RetCode.SUCCESS)
                    {
                        command.Connection.HandleError(hstmt, retcode);
                    }
                }
            }
        }

        internal void ClearBinding()
        {
            if (!this._userSpecifiedType)
            {
                this._typemap = null;
            }
            this._bindtype = null;
        }

        private void CloneHelper(OdbcParameter destination)
        {
            this.CloneHelperCore(destination);
            destination._userSpecifiedType = this._userSpecifiedType;
            destination._typemap = this._typemap;
            destination._parameterName = this._parameterName;
            destination._precision = this._precision;
            destination._scale = this._scale;
            destination._hasScale = this._hasScale;
        }

        private void CloneHelperCore(OdbcParameter destination)
        {
            destination._value = this._value;
            destination._direction = this._direction;
            destination._size = this._size;
            destination._sourceColumn = this._sourceColumn;
            destination._sourceVersion = this._sourceVersion;
            destination._sourceColumnNullMapping = this._sourceColumnNullMapping;
            destination._isNullable = this._isNullable;
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

        private void CopyParameterInternal()
        {
            this._internalValue = this.Value;
            this._internalPrecision = this.ShouldSerializePrecision() ? this.PrecisionInternal : this.ValuePrecision(this._internalValue);
            this._internalShouldSerializeSize = this.ShouldSerializeSize();
            this._internalSize = this._internalShouldSerializeSize ? this.Size : this.ValueSize(this._internalValue);
            this._internalDirection = this.Direction;
            this._internalScale = this.ShouldSerializeScale() ? this.ScaleInternal : this.ValueScale(this._internalValue);
            this._internalOffset = this.Offset;
            this._internalUserSpecifiedType = this.UserSpecifiedType;
        }

        internal void CopyTo(DbParameter destination)
        {
            ADP.CheckArgumentNull(destination, "destination");
            this.CloneHelper((OdbcParameter) destination);
        }

        private int GetColumnSize(object value, int offset, int ordinal)
        {
            if ((ODBC32.SQL_C.NUMERIC == this._bindtype._sql_c) && (this._internalPrecision != 0))
            {
                return Math.Min(this._internalPrecision, 0x1d);
            }
            int maxByteCount = this._bindtype._columnSize;
            if (0 < maxByteCount)
            {
                return maxByteCount;
            }
            if (ODBC32.SQL_C.NUMERIC == this._typemap._sql_c)
            {
                return 0x3e;
            }
            maxByteCount = this._internalSize;
            if ((this._internalShouldSerializeSize && (0x3fffffff > maxByteCount)) && (maxByteCount >= 0))
            {
                return maxByteCount;
            }
            if (!this._internalShouldSerializeSize && ((ParameterDirection.Output & this._internalDirection) != ((ParameterDirection) 0)))
            {
                throw ADP.UninitializedParameterSize(ordinal, this._bindtype._type);
            }
            if ((value == null) || Convert.IsDBNull(value))
            {
                maxByteCount = 0;
            }
            else if (value is string)
            {
                maxByteCount = ((string) value).Length - offset;
                if (((ParameterDirection.Output & this._internalDirection) != ((ParameterDirection) 0)) && (0x3fffffff <= this._internalSize))
                {
                    maxByteCount = Math.Max(maxByteCount, 0x1000);
                }
                if (((ODBC32.SQL_TYPE.CHAR == this._bindtype._sql_type) || (ODBC32.SQL_TYPE.VARCHAR == this._bindtype._sql_type)) || (ODBC32.SQL_TYPE.LONGVARCHAR == this._bindtype._sql_type))
                {
                    maxByteCount = Encoding.Default.GetMaxByteCount(maxByteCount);
                }
            }
            else if (value is char[])
            {
                maxByteCount = ((char[]) value).Length - offset;
                if (((ParameterDirection.Output & this._internalDirection) != ((ParameterDirection) 0)) && (0x3fffffff <= this._internalSize))
                {
                    maxByteCount = Math.Max(maxByteCount, 0x1000);
                }
                if (((ODBC32.SQL_TYPE.CHAR == this._bindtype._sql_type) || (ODBC32.SQL_TYPE.VARCHAR == this._bindtype._sql_type)) || (ODBC32.SQL_TYPE.LONGVARCHAR == this._bindtype._sql_type))
                {
                    maxByteCount = Encoding.Default.GetMaxByteCount(maxByteCount);
                }
            }
            else if (value is byte[])
            {
                maxByteCount = ((byte[]) value).Length - offset;
                if (((ParameterDirection.Output & this._internalDirection) != ((ParameterDirection) 0)) && (0x3fffffff <= this._internalSize))
                {
                    maxByteCount = Math.Max(maxByteCount, 0x2000);
                }
            }
            return Math.Max(2, maxByteCount);
        }

        internal void GetOutputValue(CNativeBuffer parameterBuffer)
        {
            if (!this._hasChanged && ((this._bindtype != null) && (this._internalDirection != ParameterDirection.Input)))
            {
                TypeMap map = this._bindtype;
                this._bindtype = null;
                int cb = (int) parameterBuffer.ReadIntPtr(this._preparedIntOffset);
                if (-1 == cb)
                {
                    this.Value = DBNull.Value;
                }
                else if ((0 <= cb) || (cb == -3))
                {
                    this.Value = parameterBuffer.MarshalToManaged(this._preparedValueOffset, this._boundSqlCType, cb);
                    if (((this._boundSqlCType == ODBC32.SQL_C.CHAR) && (this.Value != null)) && !Convert.IsDBNull(this.Value))
                    {
                        CultureInfo info = new CultureInfo(CultureInfo.CurrentCulture.LCID);
                        this.Value = Encoding.GetEncoding(info.TextInfo.ANSICodePage).GetString((byte[]) this.Value);
                    }
                    if (((map != this._typemap) && (this.Value != null)) && (!Convert.IsDBNull(this.Value) && (this.Value.GetType() != this._typemap._type)))
                    {
                        this.Value = decimal.Parse((string) this.Value, CultureInfo.CurrentCulture);
                    }
                }
            }
        }

        private byte GetParameterPrecision(object value)
        {
            if ((this._internalPrecision != 0) && (value is decimal))
            {
                if (this._internalPrecision >= 0x1d)
                {
                    return 0x1d;
                }
                if (this._internalPrecision != 0)
                {
                    SqlDecimal num2 = (decimal) value;
                    byte precision = num2.Precision;
                    this._internalPrecision = Math.Max(this._internalPrecision, precision);
                }
                return this._internalPrecision;
            }
            if (((value != null) && !(value is decimal)) && !Convert.IsDBNull(value))
            {
                return 0;
            }
            return 0x1c;
        }

        private byte GetParameterScale(object value)
        {
            if (!(value is decimal))
            {
                return this._internalScale;
            }
            byte num = (byte) ((decimal.GetBits((decimal) value)[3] & 0xff0000) >> 0x10);
            if ((this._internalScale > 0) && (this._internalScale < num))
            {
                return this._internalScale;
            }
            return num;
        }

        private int GetParameterSize(object value, int offset, int ordinal)
        {
            int length = this._bindtype._bufferSize;
            if (0 >= length)
            {
                if (ODBC32.SQL_C.NUMERIC == this._typemap._sql_c)
                {
                    return 0x206;
                }
                length = this._internalSize;
                if ((!this._internalShouldSerializeSize || (0x3fffffff <= length)) || (length < 0))
                {
                    if ((length <= 0) && ((ParameterDirection.Output & this._internalDirection) != ((ParameterDirection) 0)))
                    {
                        throw ADP.UninitializedParameterSize(ordinal, this._bindtype._type);
                    }
                    if ((value == null) || Convert.IsDBNull(value))
                    {
                        if (this._bindtype._sql_c == ODBC32.SQL_C.WCHAR)
                        {
                            length = 2;
                        }
                        else
                        {
                            length = 0;
                        }
                    }
                    else if (value is string)
                    {
                        length = ((((string) value).Length - offset) * 2) + 2;
                    }
                    else if (value is char[])
                    {
                        length = ((((char[]) value).Length - offset) * 2) + 2;
                    }
                    else if (value is byte[])
                    {
                        length = ((byte[]) value).Length - offset;
                    }
                    if (((ParameterDirection.Output & this._internalDirection) != ((ParameterDirection) 0)) && (0x3fffffff <= this._internalSize))
                    {
                        length = Math.Max(length, 0x2000);
                    }
                    return length;
                }
                if (ODBC32.SQL_C.WCHAR == this._bindtype._sql_c)
                {
                    if (((value is string) && (length < ((string) value).Length)) && (this._bindtype == this._originalbindtype))
                    {
                        length = ((string) value).Length;
                    }
                    return ((length * 2) + 2);
                }
                if (((value is byte[]) && (length < ((byte[]) value).Length)) && (this._bindtype == this._originalbindtype))
                {
                    length = ((byte[]) value).Length;
                }
            }
            return length;
        }

        private int GetValueSize(object value, int offset)
        {
            if ((ODBC32.SQL_C.NUMERIC == this._bindtype._sql_c) && (this._internalPrecision != 0))
            {
                return Math.Min(this._internalPrecision, 0x1d);
            }
            int num = this._bindtype._columnSize;
            if (0 >= num)
            {
                bool flag = false;
                if (value is string)
                {
                    num = ((string) value).Length - offset;
                    flag = true;
                }
                else if (value is char[])
                {
                    num = ((char[]) value).Length - offset;
                    flag = true;
                }
                else if (value is byte[])
                {
                    num = ((byte[]) value).Length - offset;
                }
                else
                {
                    num = 0;
                }
                if ((this._internalShouldSerializeSize && (this._internalSize >= 0)) && ((this._internalSize < num) && (this._bindtype == this._originalbindtype)))
                {
                    num = this._internalSize;
                }
                if (flag)
                {
                    num *= 2;
                }
            }
            return num;
        }

        internal void PrepareForBind(OdbcCommand command, short ordinal, ref int parameterBufferSize)
        {
            this.CopyParameterInternal();
            object bytes = this.ProcessAndGetParameterValue();
            int offset = this._internalOffset;
            int length = this._internalSize;
            if (offset > 0)
            {
                if (bytes is string)
                {
                    if (offset > ((string) bytes).Length)
                    {
                        throw ADP.OffsetOutOfRangeException();
                    }
                }
                else if (bytes is char[])
                {
                    if (offset > ((char[]) bytes).Length)
                    {
                        throw ADP.OffsetOutOfRangeException();
                    }
                }
                else if (bytes is byte[])
                {
                    if (offset > ((byte[]) bytes).Length)
                    {
                        throw ADP.OffsetOutOfRangeException();
                    }
                }
                else
                {
                    offset = 0;
                }
            }
            switch (this._bindtype._sql_type)
            {
                case ODBC32.SQL_TYPE.WLONGVARCHAR:
                case ODBC32.SQL_TYPE.WVARCHAR:
                case ODBC32.SQL_TYPE.WCHAR:
                    if (bytes is char)
                    {
                        bytes = bytes.ToString();
                        length = ((string) bytes).Length;
                        offset = 0;
                    }
                    if (!command.Connection.TestTypeSupport(this._bindtype._sql_type))
                    {
                        if (ODBC32.SQL_TYPE.WCHAR == this._bindtype._sql_type)
                        {
                            this._bindtype = TypeMap._Char;
                        }
                        else if (ODBC32.SQL_TYPE.WVARCHAR == this._bindtype._sql_type)
                        {
                            this._bindtype = TypeMap._VarChar;
                        }
                        else if (ODBC32.SQL_TYPE.WLONGVARCHAR == this._bindtype._sql_type)
                        {
                            this._bindtype = TypeMap._Text;
                        }
                    }
                    break;

                case ODBC32.SQL_TYPE.BIGINT:
                    if (!command.Connection.IsV3Driver)
                    {
                        this._bindtype = TypeMap._VarChar;
                        if ((bytes != null) && !Convert.IsDBNull(bytes))
                        {
                            bytes = ((long) bytes).ToString(CultureInfo.CurrentCulture);
                            length = ((string) bytes).Length;
                            offset = 0;
                        }
                    }
                    break;

                case ODBC32.SQL_TYPE.NUMERIC:
                case ODBC32.SQL_TYPE.DECIMAL:
                    if ((!command.Connection.IsV3Driver || !command.Connection.TestTypeSupport(ODBC32.SQL_TYPE.NUMERIC)) || command.Connection.TestRestrictedSqlBindType(this._bindtype._sql_type))
                    {
                        this._bindtype = TypeMap._VarChar;
                        if ((bytes != null) && !Convert.IsDBNull(bytes))
                        {
                            bytes = ((decimal) bytes).ToString(CultureInfo.CurrentCulture);
                            length = ((string) bytes).Length;
                            offset = 0;
                        }
                    }
                    break;
            }
            ODBC32.SQL_C cHAR = this._bindtype._sql_c;
            if (!command.Connection.IsV3Driver && (cHAR == ODBC32.SQL_C.WCHAR))
            {
                cHAR = ODBC32.SQL_C.CHAR;
                if (((bytes != null) && !Convert.IsDBNull(bytes)) && (bytes is string))
                {
                    CultureInfo info = new CultureInfo(CultureInfo.CurrentCulture.LCID);
                    bytes = Encoding.GetEncoding(info.TextInfo.ANSICodePage).GetBytes(bytes.ToString());
                    length = ((byte[]) bytes).Length;
                }
            }
            int num2 = this.GetParameterSize(bytes, offset, ordinal);
            switch (this._bindtype._sql_type)
            {
                case ODBC32.SQL_TYPE.WVARCHAR:
                    if (num2 > 0xfa0)
                    {
                        this._bindtype = TypeMap._NText;
                    }
                    break;

                case ODBC32.SQL_TYPE.VARBINARY:
                    if (num2 > 0x1f40)
                    {
                        this._bindtype = TypeMap._Image;
                    }
                    break;

                case ODBC32.SQL_TYPE.VARCHAR:
                    if (num2 > 0x1f40)
                    {
                        this._bindtype = TypeMap._Text;
                    }
                    break;
            }
            this._prepared_Sql_C_Type = cHAR;
            this._preparedOffset = offset;
            this._preparedSize = length;
            this._preparedValue = bytes;
            this._preparedBufferSize = num2;
            this._preparedIntOffset = parameterBufferSize;
            this._preparedValueOffset = this._preparedIntOffset + IntPtr.Size;
            parameterBufferSize += num2 + IntPtr.Size;
        }

        private object ProcessAndGetParameterValue()
        {
            object obj2 = this._internalValue;
            if (this._internalUserSpecifiedType)
            {
                if ((obj2 != null) && !Convert.IsDBNull(obj2))
                {
                    Type type = obj2.GetType();
                    if (!type.IsArray)
                    {
                        if (!(type != this._typemap._type))
                        {
                            goto Label_00D5;
                        }
                        try
                        {
                            obj2 = Convert.ChangeType(obj2, this._typemap._type, null);
                            goto Label_00D5;
                        }
                        catch (Exception exception)
                        {
                            if (!ADP.IsCatchableExceptionType(exception))
                            {
                                throw;
                            }
                            throw ADP.ParameterConversionFailed(obj2, this._typemap._type, exception);
                        }
                    }
                    if (type == typeof(char[]))
                    {
                        obj2 = new string((char[]) obj2);
                    }
                }
            }
            else if (this._typemap == null)
            {
                if ((obj2 == null) || Convert.IsDBNull(obj2))
                {
                    this._typemap = TypeMap._NVarChar;
                }
                else
                {
                    Type dataType = obj2.GetType();
                    this._typemap = TypeMap.FromSystemType(dataType);
                }
            }
        Label_00D5:
            this._originalbindtype = this._bindtype = this._typemap;
            return obj2;
        }

        private void PropertyChanging()
        {
            this._hasChanged = true;
        }

        private void PropertyTypeChanging()
        {
            this.PropertyChanging();
        }

        public override void ResetDbType()
        {
            this.ResetOdbcType();
        }

        public void ResetOdbcType()
        {
            this.PropertyTypeChanging();
            this._typemap = null;
            this._userSpecifiedType = false;
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

        internal void SetInputValue(object value, ODBC32.SQL_C sql_c_type, int cbsize, int sizeorprecision, int offset, CNativeBuffer parameterBuffer)
        {
            if ((ParameterDirection.Input != this._internalDirection) && (ParameterDirection.InputOutput != this._internalDirection))
            {
                this._internalValue = null;
                parameterBuffer.WriteIntPtr(this._preparedIntOffset, (IntPtr) (-1));
            }
            else if (value == null)
            {
                parameterBuffer.WriteIntPtr(this._preparedIntOffset, (IntPtr) (-5));
            }
            else if (Convert.IsDBNull(value))
            {
                parameterBuffer.WriteIntPtr(this._preparedIntOffset, (IntPtr) (-1));
            }
            else
            {
                switch (sql_c_type)
                {
                    case ODBC32.SQL_C.WCHAR:
                    case ODBC32.SQL_C.BINARY:
                    case ODBC32.SQL_C.CHAR:
                        parameterBuffer.WriteIntPtr(this._preparedIntOffset, (IntPtr) cbsize);
                        break;

                    default:
                        parameterBuffer.WriteIntPtr(this._preparedIntOffset, IntPtr.Zero);
                        break;
                }
                parameterBuffer.MarshalToNative(this._preparedValueOffset, value, sql_c_type, sizeorprecision, offset);
            }
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

        private ODBC32.SQL_PARAM SqlDirectionFromParameterDirection()
        {
            switch (this._internalDirection)
            {
                case ParameterDirection.Input:
                    return ODBC32.SQL_PARAM.INPUT;

                case ParameterDirection.Output:
                case ParameterDirection.ReturnValue:
                    return ODBC32.SQL_PARAM.OUTPUT;

                case ParameterDirection.InputOutput:
                    return ODBC32.SQL_PARAM.INPUT_OUTPUT;
            }
            return ODBC32.SQL_PARAM.INPUT;
        }

        object ICloneable.Clone()
        {
            return new OdbcParameter(this);
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
                if (this._userSpecifiedType)
                {
                    return this._typemap._dbType;
                }
                return TypeMap._NVarChar._dbType;
            }
            set
            {
                if ((this._typemap == null) || (this._typemap._dbType != value))
                {
                    this.PropertyTypeChanging();
                    this._typemap = TypeMap.FromDbType(value);
                    this._userSpecifiedType = true;
                }
            }
        }

        [ResCategory("DataCategory_Data"), RefreshProperties(RefreshProperties.All), ResDescription("DbParameter_Direction")]
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

        internal bool HasChanged
        {
            set
            {
                this._hasChanged = value;
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

        [DbProviderSpecificTypeProperty(true), DefaultValue(11), ResCategory("DataCategory_Data"), RefreshProperties(RefreshProperties.All), ResDescription("OdbcParameter_OdbcType")]
        public System.Data.Odbc.OdbcType OdbcType
        {
            get
            {
                if (this._userSpecifiedType)
                {
                    return this._typemap._odbcType;
                }
                return TypeMap._NVarChar._odbcType;
            }
            set
            {
                if ((this._typemap == null) || (this._typemap._odbcType != value))
                {
                    this.PropertyTypeChanging();
                    this._typemap = TypeMap.FromOdbcType(value);
                    this._userSpecifiedType = true;
                }
            }
        }

        internal int Offset
        {
            get
            {
                return 0;
            }
        }

        [ResDescription("DbParameter_ParameterName"), ResCategory("DataCategory_Data")]
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

        [ResDescription("DbDataParameter_Precision"), ResCategory("DataCategory_Data"), DefaultValue((byte) 0)]
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

        [ResCategory("DataCategory_Data"), ResDescription("DbDataParameter_Scale"), DefaultValue((byte) 0)]
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

        [ResDescription("DbParameter_SourceColumn"), ResCategory("DataCategory_Update")]
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

        [ResDescription("DbParameter_SourceVersion"), ResCategory("DataCategory_Update")]
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

        internal bool UserSpecifiedType
        {
            get
            {
                return this._userSpecifiedType;
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

        internal sealed class OdbcParameterConverter : ExpandableObjectConverter
        {
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return ((destinationType == typeof(InstanceDescriptor)) || base.CanConvertTo(context, destinationType));
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == null)
                {
                    throw ADP.ArgumentNull("destinationType");
                }
                if ((destinationType == typeof(InstanceDescriptor)) && (value is OdbcParameter))
                {
                    object[] objArray3;
                    Type[] typeArray3;
                    OdbcParameter parameter = (OdbcParameter) value;
                    int num = 0;
                    if (OdbcType.NChar != parameter.OdbcType)
                    {
                        num |= 1;
                    }
                    if (parameter.ShouldSerializeSize())
                    {
                        num |= 2;
                    }
                    if (!ADP.IsEmpty(parameter.SourceColumn))
                    {
                        num |= 4;
                    }
                    if (parameter.Value != null)
                    {
                        num |= 8;
                    }
                    if (((ParameterDirection.Input != parameter.Direction) || parameter.IsNullable) || ((parameter.ShouldSerializePrecision() || parameter.ShouldSerializeScale()) || (DataRowVersion.Current != parameter.SourceVersion)))
                    {
                        num |= 0x10;
                    }
                    if (parameter.SourceColumnNullMapping)
                    {
                        num |= 0x20;
                    }
                    switch (num)
                    {
                        case 0:
                        case 1:
                            typeArray3 = new Type[] { typeof(string), typeof(OdbcType) };
                            objArray3 = new object[] { parameter.ParameterName, parameter.OdbcType };
                            break;

                        case 2:
                        case 3:
                            typeArray3 = new Type[] { typeof(string), typeof(OdbcType), typeof(int) };
                            objArray3 = new object[] { parameter.ParameterName, parameter.OdbcType, parameter.Size };
                            break;

                        case 4:
                        case 5:
                        case 6:
                        case 7:
                            typeArray3 = new Type[] { typeof(string), typeof(OdbcType), typeof(int), typeof(string) };
                            objArray3 = new object[] { parameter.ParameterName, parameter.OdbcType, parameter.Size, parameter.SourceColumn };
                            break;

                        case 8:
                            typeArray3 = new Type[] { typeof(string), typeof(object) };
                            objArray3 = new object[] { parameter.ParameterName, parameter.Value };
                            break;

                        default:
                            if ((0x20 & num) == 0)
                            {
                                typeArray3 = new Type[] { typeof(string), typeof(OdbcType), typeof(int), typeof(ParameterDirection), typeof(bool), typeof(byte), typeof(byte), typeof(string), typeof(DataRowVersion), typeof(object) };
                                objArray3 = new object[] { parameter.ParameterName, parameter.OdbcType, parameter.Size, parameter.Direction, parameter.IsNullable, parameter.PrecisionInternal, parameter.ScaleInternal, parameter.SourceColumn, parameter.SourceVersion, parameter.Value };
                            }
                            else
                            {
                                typeArray3 = new Type[] { typeof(string), typeof(OdbcType), typeof(int), typeof(ParameterDirection), typeof(byte), typeof(byte), typeof(string), typeof(DataRowVersion), typeof(bool), typeof(object) };
                                objArray3 = new object[] { parameter.ParameterName, parameter.OdbcType, parameter.Size, parameter.Direction, parameter.PrecisionInternal, parameter.ScaleInternal, parameter.SourceColumn, parameter.SourceVersion, parameter.SourceColumnNullMapping, parameter.Value };
                            }
                            break;
                    }
                    ConstructorInfo constructor = typeof(OdbcParameter).GetConstructor(typeArray3);
                    if (null != constructor)
                    {
                        return new InstanceDescriptor(constructor, objArray3);
                    }
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}

