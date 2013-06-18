namespace System.Data.OracleClient
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlTypes;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    internal sealed class OracleParameterBinding
    {
        private bool _bindAsUCS2;
        private OciBindHandle _bindHandle;
        private MetaType _bindingMetaType;
        private int _bindSize;
        private int _bufferLength;
        private object _coercedValue;
        private OracleCommand _command;
        private OciDateTimeDescriptor _dateTimeDescriptor;
        private OciStatementHandle _descriptor;
        private bool _freeTemporaryLob;
        private int _indicatorOffset;
        private int _lengthOffset;
        private OciLobLocator _locator;
        private OracleParameter _parameter;
        private int _valueOffset;

        internal OracleParameterBinding(OracleCommand command, OracleParameter parameter)
        {
            this._command = command;
            this._parameter = parameter;
        }

        internal void Bind(OciStatementHandle statementHandle, NativeBuffer parameterBuffer, OracleConnection connection, ref bool mustRelease, ref SafeHandle handleToBind)
        {
            if (IsDirection(this.Parameter, ParameterDirection.Output) || (this.Parameter.Value != null))
            {
                int num2;
                IntPtr ptr2;
                string parameterName = this.Parameter.ParameterName;
                OciErrorHandle errorHandle = connection.ErrorHandle;
                OciServiceContextHandle serviceContextHandle = connection.ServiceContextHandle;
                int num = 0;
                OCI.INDICATOR oK = OCI.INDICATOR.OK;
                OCI.DATATYPE ociType = this._bindingMetaType.OciType;
                IntPtr dataPtr = parameterBuffer.DangerousGetDataPtr(this._indicatorOffset);
                IntPtr alenp = parameterBuffer.DangerousGetDataPtr(this._lengthOffset);
                IntPtr valuep = parameterBuffer.DangerousGetDataPtr(this._valueOffset);
                OciHandle.SafeDispose(ref this._dateTimeDescriptor);
                if (IsDirection(this.Parameter, ParameterDirection.Input))
                {
                    if (System.Data.Common.ADP.IsNull(this._coercedValue))
                    {
                        oK = OCI.INDICATOR.ISNULL;
                        switch (ociType)
                        {
                            case OCI.DATATYPE.INT_TIMESTAMP:
                            case OCI.DATATYPE.INT_TIMESTAMP_TZ:
                            case OCI.DATATYPE.INT_TIMESTAMP_LTZ:
                                this._dateTimeDescriptor = OracleDateTime.CreateEmptyDescriptor(ociType, connection);
                                handleToBind = this._dateTimeDescriptor;
                                break;
                        }
                    }
                    else
                    {
                        num = this.PutOracleValue(this._coercedValue, parameterBuffer, this._valueOffset, this._bindingMetaType, connection, ref handleToBind);
                    }
                }
                else
                {
                    if (this._bindingMetaType.IsVariableLength)
                    {
                        num = 0;
                    }
                    else
                    {
                        num = this._bufferLength;
                    }
                    OciLobLocator.SafeDispose(ref this._locator);
                    OciHandle.SafeDispose(ref this._descriptor);
                    switch (ociType)
                    {
                        case OCI.DATATYPE.CLOB:
                        case OCI.DATATYPE.BLOB:
                        case OCI.DATATYPE.BFILE:
                            this._locator = new OciLobLocator(connection, this._bindingMetaType.OracleType);
                            handleToBind = this._locator.Descriptor;
                            break;

                        case OCI.DATATYPE.RSET:
                            this._descriptor = new OciStatementHandle(serviceContextHandle);
                            handleToBind = this._descriptor;
                            break;

                        case OCI.DATATYPE.INT_TIMESTAMP:
                        case OCI.DATATYPE.INT_TIMESTAMP_TZ:
                        case OCI.DATATYPE.INT_TIMESTAMP_LTZ:
                            this._dateTimeDescriptor = OracleDateTime.CreateEmptyDescriptor(ociType, connection);
                            handleToBind = this._dateTimeDescriptor;
                            break;
                    }
                }
                if (handleToBind != null)
                {
                    handleToBind.DangerousAddRef(ref mustRelease);
                    parameterBuffer.WriteIntPtr(this._valueOffset, handleToBind.DangerousGetHandle());
                }
                parameterBuffer.WriteInt16(this._indicatorOffset, (short) oK);
                if ((OCI.DATATYPE.LONGVARCHAR == ociType) || (OCI.DATATYPE.LONGVARRAW == ociType))
                {
                    alenp = IntPtr.Zero;
                }
                else if (this._bindAsUCS2)
                {
                    parameterBuffer.WriteInt32(this._lengthOffset, num / System.Data.Common.ADP.CharSize);
                }
                else
                {
                    parameterBuffer.WriteInt32(this._lengthOffset, num);
                }
                if (IsDirection(this.Parameter, ParameterDirection.Output))
                {
                    num2 = this._bufferLength;
                }
                else
                {
                    num2 = num;
                }
                OCI.DATATYPE dty = ociType;
                switch (ociType)
                {
                    case OCI.DATATYPE.INT_TIMESTAMP:
                        dty = OCI.DATATYPE.TIMESTAMP;
                        break;

                    case OCI.DATATYPE.INT_TIMESTAMP_TZ:
                        dty = OCI.DATATYPE.TIMESTAMP_TZ;
                        break;

                    case OCI.DATATYPE.INT_TIMESTAMP_LTZ:
                        dty = OCI.DATATYPE.TIMESTAMP_LTZ;
                        break;
                }
                int rc = TracedNativeMethods.OCIBindByName(statementHandle, out ptr2, errorHandle, parameterName, parameterName.Length, valuep, num2, dty, dataPtr, alenp, OCI.MODE.OCI_DEFAULT);
                if (rc != 0)
                {
                    this._command.Connection.CheckError(errorHandle, rc);
                }
                this._bindHandle = new OciBindHandle(statementHandle, ptr2);
                if (this._bindingMetaType.IsCharacterType)
                {
                    if (OCI.ClientVersionAtLeastOracle9i && IsDirection(this.Parameter, ParameterDirection.Output))
                    {
                        this._bindHandle.SetAttribute(OCI.ATTR.OCI_ATTR_MAXCHAR_SIZE, this._bindSize, errorHandle);
                    }
                    if ((num2 > (this._bindingMetaType.MaxBindSize / System.Data.Common.ADP.CharSize)) || (!OCI.ClientVersionAtLeastOracle9i && this._bindingMetaType.UsesNationalCharacterSet))
                    {
                        this._bindHandle.SetAttribute(OCI.ATTR.OCI_ATTR_MAXDATA_SIZE, this._bindingMetaType.MaxBindSize, errorHandle);
                    }
                    if (this._bindingMetaType.UsesNationalCharacterSet)
                    {
                        this._bindHandle.SetAttribute(OCI.ATTR.OCI_ATTR_CHARSET_FORM, 2, errorHandle);
                    }
                    if (this._bindAsUCS2)
                    {
                        this._bindHandle.SetAttribute(OCI.ATTR.OCI_ATTR_CHARSET_ID, 0x3e8, errorHandle);
                    }
                }
                GC.KeepAlive(parameterBuffer);
            }
        }

        private OracleLob CreateTemporaryLobForValue(OracleConnection connection, OracleType oracleType, object value)
        {
            switch (oracleType)
            {
                case OracleType.BFile:
                    oracleType = OracleType.Blob;
                    break;

                case OracleType.Blob:
                case OracleType.Clob:
                case OracleType.NClob:
                    break;

                default:
                    throw System.Data.Common.ADP.InvalidLobType(oracleType);
            }
            OracleLob stream = new OracleLob(connection, oracleType);
            byte[] buffer = value as byte[];
            if (buffer != null)
            {
                stream.Write(buffer, 0, buffer.Length);
                return stream;
            }
            Encoding encoding = new UnicodeEncoding(false, false);
            stream.Seek(0L, SeekOrigin.Begin);
            StreamWriter writer = new StreamWriter(stream, encoding);
            writer.Write(value);
            writer.Flush();
            return stream;
        }

        internal void Dispose()
        {
            OciHandle.SafeDispose(ref this._bindHandle);
            if (this._freeTemporaryLob)
            {
                OracleLob lob = this._coercedValue as OracleLob;
                if (lob != null)
                {
                    lob.Free();
                }
            }
        }

        internal object GetOutputValue(NativeBuffer parameterBuffer, OracleConnection connection, bool needCLSType)
        {
            object obj2;
            if (parameterBuffer.ReadInt16(this._indicatorOffset) == -1)
            {
                return DBNull.Value;
            }
            switch (this._bindingMetaType.OciType)
            {
                case OCI.DATATYPE.VARCHAR2:
                case OCI.DATATYPE.LONG:
                case OCI.DATATYPE.LONGVARCHAR:
                case OCI.DATATYPE.CHAR:
                {
                    obj2 = new OracleString(parameterBuffer, this._valueOffset, this._lengthOffset, this._bindingMetaType, connection, this._bindAsUCS2, true);
                    int size = this._parameter.Size;
                    if (size != 0)
                    {
                        OracleString str4 = (OracleString) obj2;
                        if (size < str4.Length)
                        {
                            OracleString str3 = (OracleString) obj2;
                            string s = str3.Value.Substring(0, size);
                            if (needCLSType)
                            {
                                return s;
                            }
                            return new OracleString(s);
                        }
                    }
                    if (needCLSType)
                    {
                        OracleString str2 = (OracleString) obj2;
                        obj2 = str2.Value;
                    }
                    return obj2;
                }
                case OCI.DATATYPE.INTEGER:
                case OCI.DATATYPE.FLOAT:
                case OCI.DATATYPE.UNSIGNEDINT:
                    return parameterBuffer.PtrToStructure(this._valueOffset, this._bindingMetaType.BaseType);

                case OCI.DATATYPE.VARNUM:
                    obj2 = new OracleNumber(parameterBuffer, this._valueOffset);
                    if (needCLSType)
                    {
                        OracleNumber number = (OracleNumber) obj2;
                        obj2 = number.Value;
                    }
                    return obj2;

                case OCI.DATATYPE.DATE:
                    obj2 = new OracleDateTime(parameterBuffer, this._valueOffset, this._lengthOffset, this._bindingMetaType, connection);
                    if (needCLSType)
                    {
                        OracleDateTime time2 = (OracleDateTime) obj2;
                        obj2 = time2.Value;
                    }
                    return obj2;

                case OCI.DATATYPE.RAW:
                case OCI.DATATYPE.LONGRAW:
                case OCI.DATATYPE.LONGVARRAW:
                    obj2 = new OracleBinary(parameterBuffer, this._valueOffset, this._lengthOffset, this._bindingMetaType);
                    if (needCLSType)
                    {
                        OracleBinary binary = (OracleBinary) obj2;
                        obj2 = binary.Value;
                    }
                    return obj2;

                case OCI.DATATYPE.CLOB:
                case OCI.DATATYPE.BLOB:
                    return new OracleLob(this._locator);

                case OCI.DATATYPE.BFILE:
                    return new OracleBFile(this._locator);

                case OCI.DATATYPE.RSET:
                    return new OracleDataReader(connection, this._descriptor);

                case OCI.DATATYPE.INT_TIMESTAMP:
                case OCI.DATATYPE.INT_TIMESTAMP_TZ:
                case OCI.DATATYPE.INT_TIMESTAMP_LTZ:
                    obj2 = new OracleDateTime(this._dateTimeDescriptor, this._bindingMetaType, connection);
                    if (needCLSType)
                    {
                        OracleDateTime time = (OracleDateTime) obj2;
                        obj2 = time.Value;
                    }
                    return obj2;

                case OCI.DATATYPE.INT_INTERVAL_YM:
                    obj2 = new OracleMonthSpan(parameterBuffer, this._valueOffset);
                    if (needCLSType)
                    {
                        OracleMonthSpan span2 = (OracleMonthSpan) obj2;
                        obj2 = span2.Value;
                    }
                    return obj2;

                case OCI.DATATYPE.INT_INTERVAL_DS:
                    obj2 = new OracleTimeSpan(parameterBuffer, this._valueOffset);
                    if (needCLSType)
                    {
                        OracleTimeSpan span = (OracleTimeSpan) obj2;
                        obj2 = span.Value;
                    }
                    return obj2;
            }
            throw System.Data.Common.ADP.TypeNotSupported(this._bindingMetaType.OciType);
        }

        internal static bool IsDirection(IDataParameter value, ParameterDirection condition)
        {
            return (condition == (condition & value.Direction));
        }

        private bool IsEmpty(object value)
        {
            bool flag = false;
            if (value is string)
            {
                flag = 0 == ((string) value).Length;
            }
            if (value is OracleString)
            {
                OracleString str = (OracleString) value;
                flag = 0 == str.Length;
            }
            if (value is char[])
            {
                flag = 0 == ((char[]) value).Length;
            }
            if (value is byte[])
            {
                flag = 0 == ((byte[]) value).Length;
            }
            if (value is OracleBinary)
            {
                OracleBinary binary = (OracleBinary) value;
                flag = 0 == binary.Length;
            }
            return flag;
        }

        internal void PostExecute(NativeBuffer parameterBuffer, OracleConnection connection)
        {
            OracleParameter parameter = this.Parameter;
            if (IsDirection(parameter, ParameterDirection.Output) || IsDirection(parameter, ParameterDirection.ReturnValue))
            {
                bool needCLSType = true;
                if (IsDirection(parameter, ParameterDirection.Input) && (parameter.Value is INullable))
                {
                    needCLSType = false;
                }
                parameter.Value = this.GetOutputValue(parameterBuffer, connection, needCLSType);
            }
        }

        internal void PrepareForBind(OracleConnection connection, ref int offset)
        {
            OracleParameter parameter = this.Parameter;
            bool flag = false;
            object obj2 = parameter.Value;
            if (!IsDirection(parameter, ParameterDirection.Output) && (obj2 == null))
            {
                this._bufferLength = 0;
            }
            else
            {
                this._bindingMetaType = parameter.GetMetaType(obj2);
                if ((OCI.DATATYPE.RSET == this._bindingMetaType.OciType) && System.Data.Common.ADP.IsDirection(parameter.Direction, ParameterDirection.Input))
                {
                    throw System.Data.Common.ADP.InputRefCursorNotSupported(parameter.ParameterName);
                }
                parameter.SetCoercedValueInternal(obj2, this._bindingMetaType);
                this._coercedValue = parameter.GetCoercedValueInternal();
                switch (this._bindingMetaType.OciType)
                {
                    case OCI.DATATYPE.CLOB:
                    case OCI.DATATYPE.BLOB:
                    case OCI.DATATYPE.BFILE:
                        if ((!System.Data.Common.ADP.IsNull(this._coercedValue) && !(this._coercedValue is OracleLob)) && !(this._coercedValue is OracleBFile))
                        {
                            if (!connection.HasTransaction)
                            {
                                this._bindingMetaType = MetaType.GetMetaTypeForType(this._bindingMetaType.DbType);
                                flag = true;
                                break;
                            }
                            this._freeTemporaryLob = true;
                            this._coercedValue = this.CreateTemporaryLobForValue(connection, this._bindingMetaType.OracleType, this._coercedValue);
                        }
                        break;
                }
                this._bindSize = this._bindingMetaType.BindSize;
                if (((IsDirection(parameter, ParameterDirection.Output) && this._bindingMetaType.IsVariableLength) || ((this._bindSize == 0) && !System.Data.Common.ADP.IsNull(this._coercedValue))) || (this._bindSize > 0x7fff))
                {
                    int bindSize = parameter.BindSize;
                    if (bindSize != 0)
                    {
                        this._bindSize = bindSize;
                    }
                    if (((this._bindSize == 0) || (0x7fffffff == this._bindSize)) && !this.IsEmpty(this._coercedValue))
                    {
                        throw System.Data.Common.ADP.ParameterSizeIsMissing(parameter.ParameterName, this._bindingMetaType.BaseType);
                    }
                }
                this._bufferLength = this._bindSize;
                if (this._bindingMetaType.IsCharacterType && connection.ServerVersionAtLeastOracle8)
                {
                    this._bindAsUCS2 = true;
                    this._bufferLength *= System.Data.Common.ADP.CharSize;
                }
                if (!System.Data.Common.ADP.IsNull(this._coercedValue) && ((this._bindSize > this._bindingMetaType.MaxBindSize) || flag))
                {
                    switch (this._bindingMetaType.OciType)
                    {
                        case OCI.DATATYPE.RAW:
                        case OCI.DATATYPE.LONGRAW:
                            this._bindingMetaType = MetaType.oracleTypeMetaType_LONGVARRAW;
                            break;

                        case OCI.DATATYPE.CHAR:
                        case OCI.DATATYPE.VARCHAR2:
                        case OCI.DATATYPE.LONG:
                            this._bindingMetaType = this._bindingMetaType.UsesNationalCharacterSet ? MetaType.oracleTypeMetaType_LONGNVARCHAR : MetaType.oracleTypeMetaType_LONGVARCHAR;
                            break;
                    }
                    this._bufferLength += 4;
                }
                if (0 > this._bufferLength)
                {
                    throw System.Data.Common.ADP.ParameterSizeIsTooLarge(parameter.ParameterName);
                }
                this._indicatorOffset = offset;
                offset += IntPtr.Size;
                this._lengthOffset = offset;
                offset += IntPtr.Size;
                this._valueOffset = offset;
                offset += this._bufferLength;
                offset = (offset + (IntPtr.Size - 1)) & ~(IntPtr.Size - 1);
            }
        }

        internal int PutOracleValue(object value, NativeBuffer buffer, int bufferOffset, MetaType metaType, OracleConnection connection, ref SafeHandle handleToBind)
        {
            handleToBind = null;
            OCI.DATATYPE ociType = metaType.OciType;
            OracleParameter parameter = this.Parameter;
            switch (ociType)
            {
                case OCI.DATATYPE.VARCHAR2:
                case OCI.DATATYPE.LONG:
                case OCI.DATATYPE.LONGVARCHAR:
                case OCI.DATATYPE.CHAR:
                    return OracleString.MarshalToNative(value, parameter.Offset, parameter.GetActualSize(), buffer, bufferOffset, ociType, this._bindAsUCS2);

                case OCI.DATATYPE.INTEGER:
                case OCI.DATATYPE.FLOAT:
                case OCI.DATATYPE.UNSIGNEDINT:
                    buffer.StructureToPtr(bufferOffset, value);
                    return metaType.BindSize;

                case OCI.DATATYPE.VARNUM:
                    return OracleNumber.MarshalToNative(value, buffer, bufferOffset, connection);

                case OCI.DATATYPE.DATE:
                    return OracleDateTime.MarshalDateToNative(value, buffer, bufferOffset, ociType, connection);

                case OCI.DATATYPE.RAW:
                case OCI.DATATYPE.LONGRAW:
                case OCI.DATATYPE.LONGVARRAW:
                {
                    int num;
                    byte[] buffer2;
                    if (this._coercedValue is OracleBinary)
                    {
                        OracleBinary binary = (OracleBinary) this._coercedValue;
                        buffer2 = binary.Value;
                    }
                    else
                    {
                        buffer2 = (byte[]) this._coercedValue;
                    }
                    int num2 = buffer2.Length - parameter.Offset;
                    int actualSize = parameter.GetActualSize();
                    if (actualSize != 0)
                    {
                        num2 = Math.Min(num2, actualSize);
                    }
                    if (OCI.DATATYPE.LONGVARRAW == ociType)
                    {
                        buffer.WriteInt32(bufferOffset, num2);
                        bufferOffset += 4;
                        num = num2 + 4;
                    }
                    else
                    {
                        num = num2;
                    }
                    buffer.WriteBytes(bufferOffset, buffer2, parameter.Offset, num2);
                    return num;
                }
                case OCI.DATATYPE.CLOB:
                case OCI.DATATYPE.BLOB:
                    if (!(value is OracleLob))
                    {
                        throw System.Data.Common.ADP.BadBindValueType(value.GetType(), metaType.OracleType);
                    }
                    handleToBind = ((OracleLob) value).Descriptor;
                    return IntPtr.Size;

                case OCI.DATATYPE.BFILE:
                    if (!(value is OracleBFile))
                    {
                        throw System.Data.Common.ADP.BadBindValueType(value.GetType(), metaType.OracleType);
                    }
                    handleToBind = ((OracleBFile) value).Descriptor;
                    return IntPtr.Size;

                case OCI.DATATYPE.INT_TIMESTAMP:
                case OCI.DATATYPE.INT_TIMESTAMP_LTZ:
                    if (value is OracleDateTime)
                    {
                        OracleDateTime time = (OracleDateTime) value;
                        if (!time.HasTimeInfo)
                        {
                            throw System.Data.Common.ADP.UnsupportedOracleDateTimeBinding(metaType.OracleType);
                        }
                    }
                    this._dateTimeDescriptor = OracleDateTime.CreateDescriptor(ociType, connection, value);
                    handleToBind = this._dateTimeDescriptor;
                    return IntPtr.Size;

                case OCI.DATATYPE.INT_TIMESTAMP_TZ:
                    if (value is OracleDateTime)
                    {
                        OracleDateTime time2 = (OracleDateTime) value;
                        if (!time2.HasTimeZoneInfo)
                        {
                            throw System.Data.Common.ADP.UnsupportedOracleDateTimeBinding(OracleType.TimestampWithTZ);
                        }
                    }
                    this._dateTimeDescriptor = OracleDateTime.CreateDescriptor(ociType, connection, value);
                    handleToBind = this._dateTimeDescriptor;
                    return IntPtr.Size;

                case OCI.DATATYPE.INT_INTERVAL_YM:
                    return OracleMonthSpan.MarshalToNative(value, buffer, bufferOffset);

                case OCI.DATATYPE.INT_INTERVAL_DS:
                    return OracleTimeSpan.MarshalToNative(value, buffer, bufferOffset);
            }
            throw System.Data.Common.ADP.TypeNotSupported(ociType);
        }

        internal OracleParameter Parameter
        {
            get
            {
                return this._parameter;
            }
        }
    }
}

