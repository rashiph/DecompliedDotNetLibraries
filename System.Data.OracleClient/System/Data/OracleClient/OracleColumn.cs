namespace System.Data.OracleClient
{
    using System;
    using System.Data.Common;
    using System.IO;
    using System.Runtime.InteropServices;

    internal sealed class OracleColumn
    {
        private bool _bindAsUTF16;
        private int _byteSize;
        private OCI.Callback.OCICallbackDefine _callback;
        private string _columnName;
        private OracleConnection _connection;
        private int _connectionCloseCount;
        private OciParameterDescriptor _describeHandle;
        private int _indicatorOffset;
        private bool _isNullable;
        private int _lengthOffset;
        private OciLobLocator _lobLocator;
        private NativeBuffer_LongColumnData _longBuffer;
        private int _longLength;
        private MetaType _metaType;
        private int _ordinal;
        private byte _precision;
        private NativeBuffer_RowBuffer _rowBuffer;
        private byte _scale;
        private int _valueOffset;

        internal OracleColumn(OciStatementHandle statementHandle, int ordinal, OciErrorHandle errorHandle, OracleConnection connection)
        {
            this._ordinal = ordinal;
            this._describeHandle = statementHandle.GetDescriptor(this._ordinal, errorHandle);
            this._connection = connection;
            this._connectionCloseCount = connection.CloseCount;
        }

        private int _callback_GetColumnPiecewise(IntPtr octxp, IntPtr defnp, uint iter, IntPtr bufpp, IntPtr alenp, IntPtr piecep, IntPtr indpp, IntPtr rcodep)
        {
            IntPtr ptr;
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<oc._callback_GetColumnPiecewise|ADV|OCI> octxp=0x%-07Ix defnp=0x%-07Ix iter=%-2d bufpp=0x%-07Ix alenp=0x%-07Ix piecep=0x%-07Ix indpp=0x%-07Ix rcodep=0x%-07Ix\n", octxp, defnp, (int) iter, bufpp, alenp, piecep, indpp, rcodep);
            }
            IntPtr val = (-1 != this._indicatorOffset) ? this._rowBuffer.DangerousGetDataPtr(this._indicatorOffset) : IntPtr.Zero;
            IntPtr chunk = this._longBuffer.GetChunk(out ptr);
            Marshal.WriteIntPtr(bufpp, chunk);
            Marshal.WriteIntPtr(indpp, val);
            Marshal.WriteIntPtr(alenp, ptr);
            Marshal.WriteInt32(ptr, NativeBuffer_LongColumnData.MaxChunkSize);
            GC.KeepAlive(this);
            return -24200;
        }

        internal void Bind(OciStatementHandle statementHandle, NativeBuffer_RowBuffer buffer, OciErrorHandle errorHandle, int rowBufferLength)
        {
            OciDefineHandle defnp = null;
            int num3;
            OCI.MODE mode = OCI.MODE.OCI_DEFAULT;
            OCI.DATATYPE ociType = this._metaType.OciType;
            this._rowBuffer = buffer;
            if (this._metaType.IsLong)
            {
                mode = OCI.MODE.OCI_DATA_AT_EXEC;
                num3 = 0x7fffffff;
            }
            else
            {
                num3 = this._byteSize;
            }
            IntPtr zero = IntPtr.Zero;
            IntPtr rlenp = IntPtr.Zero;
            IntPtr dataPtr = this._rowBuffer.DangerousGetDataPtr(this._valueOffset);
            if (-1 != this._indicatorOffset)
            {
                zero = this._rowBuffer.DangerousGetDataPtr(this._indicatorOffset);
            }
            if ((-1 != this._lengthOffset) && !this._metaType.IsLong)
            {
                rlenp = this._rowBuffer.DangerousGetDataPtr(this._lengthOffset);
            }
            try
            {
                IntPtr ptr3;
                int rc = TracedNativeMethods.OCIDefineByPos(statementHandle, out ptr3, errorHandle, ((uint) this._ordinal) + 1, dataPtr, num3, ociType, zero, rlenp, IntPtr.Zero, mode);
                if (rc != 0)
                {
                    this._connection.CheckError(errorHandle, rc);
                }
                defnp = new OciDefineHandle(statementHandle, ptr3);
                if (rowBufferLength != 0)
                {
                    uint pvskip = (uint) rowBufferLength;
                    uint indskip = (-1 != this._indicatorOffset) ? pvskip : 0;
                    uint rlskip = ((-1 != this._lengthOffset) && !this._metaType.IsLong) ? pvskip : 0;
                    rc = TracedNativeMethods.OCIDefineArrayOfStruct(defnp, errorHandle, pvskip, indskip, rlskip, 0);
                    if (rc != 0)
                    {
                        this._connection.CheckError(errorHandle, rc);
                    }
                }
                if (this._metaType.UsesNationalCharacterSet)
                {
                    defnp.SetAttribute(OCI.ATTR.OCI_ATTR_CHARSET_FORM, 2, errorHandle);
                }
                if (!this._connection.UnicodeEnabled && this._bindAsUTF16)
                {
                    defnp.SetAttribute(OCI.ATTR.OCI_ATTR_CHARSET_ID, 0x3e8, errorHandle);
                }
                if (this._metaType.IsLong)
                {
                    this._rowBuffer.WriteIntPtr(this._valueOffset, IntPtr.Zero);
                    this._callback = new OCI.Callback.OCICallbackDefine(this._callback_GetColumnPiecewise);
                    rc = TracedNativeMethods.OCIDefineDynamic(defnp, errorHandle, IntPtr.Zero, this._callback);
                    if (rc != 0)
                    {
                        this._connection.CheckError(errorHandle, rc);
                    }
                }
            }
            finally
            {
                NativeBuffer.SafeDispose(ref this._longBuffer);
                OciHandle.SafeDispose(ref defnp);
            }
        }

        internal bool Describe(ref int offset, OracleConnection connection, OciErrorHandle errorHandle)
        {
            byte num;
            short num3;
            bool flag = false;
            bool flag2 = false;
            this._describeHandle.GetAttribute(OCI.ATTR.OCI_ATTR_SQLCODE, out this._columnName, errorHandle, this._connection);
            this._describeHandle.GetAttribute(OCI.ATTR.OCI_ATTR_OBJECT, out num3, errorHandle);
            this._describeHandle.GetAttribute(OCI.ATTR.OCI_ATTR_SESSION, out num, errorHandle);
            this._isNullable = 0 != num;
            OCI.DATATYPE ociType = (OCI.DATATYPE) num3;
            switch (ociType)
            {
                case OCI.DATATYPE.VARCHAR2:
                case OCI.DATATYPE.CHAR:
                {
                    int num2;
                    this._describeHandle.GetAttribute(OCI.ATTR.OCI_ATTR_FNCODE, out this._byteSize, errorHandle);
                    this._describeHandle.GetAttribute(OCI.ATTR.OCI_ATTR_CHARSET_FORM, out num, errorHandle);
                    OCI.CHARSETFORM charsetform = (OCI.CHARSETFORM) num;
                    this._bindAsUTF16 = connection.ServerVersionAtLeastOracle8;
                    if (connection.ServerVersionAtLeastOracle9i && OCI.ClientVersionAtLeastOracle9i)
                    {
                        this._describeHandle.GetAttribute(OCI.ATTR.OCI_ATTR_CHAR_SIZE, out num3, errorHandle);
                        num2 = num3;
                    }
                    else
                    {
                        num2 = this._byteSize;
                    }
                    if (charsetform == OCI.CHARSETFORM.SQLCS_NCHAR)
                    {
                        this._metaType = MetaType.GetMetaTypeForType((OCI.DATATYPE.CHAR == ociType) ? System.Data.OracleClient.OracleType.NChar : System.Data.OracleClient.OracleType.NVarChar);
                    }
                    else
                    {
                        this._metaType = MetaType.GetMetaTypeForType((OCI.DATATYPE.CHAR == ociType) ? System.Data.OracleClient.OracleType.Char : System.Data.OracleClient.OracleType.VarChar);
                        if (this._bindAsUTF16)
                        {
                            this._byteSize *= System.Data.Common.ADP.CharSize;
                        }
                    }
                    this._byteSize = Math.Max(this._byteSize, num2 * System.Data.Common.ADP.CharSize);
                    flag = true;
                    break;
                }
                case OCI.DATATYPE.NUMBER:
                    this._metaType = MetaType.GetMetaTypeForType(System.Data.OracleClient.OracleType.Number);
                    this._byteSize = this._metaType.BindSize;
                    this._describeHandle.GetAttribute(OCI.ATTR.OCI_ATTR_ENV, out this._precision, errorHandle);
                    this._describeHandle.GetAttribute(OCI.ATTR.OCI_ATTR_SERVER, out this._scale, errorHandle);
                    break;

                case OCI.DATATYPE.LONG:
                    this._metaType = MetaType.GetMetaTypeForType(System.Data.OracleClient.OracleType.LongVarChar);
                    this._byteSize = this._metaType.BindSize;
                    flag = true;
                    flag2 = true;
                    this._bindAsUTF16 = connection.ServerVersionAtLeastOracle8;
                    break;

                case OCI.DATATYPE.ROWID:
                case OCI.DATATYPE.ROWID_DESC:
                case OCI.DATATYPE.UROWID:
                    this._metaType = MetaType.GetMetaTypeForType(System.Data.OracleClient.OracleType.RowId);
                    this._byteSize = this._metaType.BindSize;
                    if (connection.UnicodeEnabled)
                    {
                        this._bindAsUTF16 = true;
                        this._byteSize *= System.Data.Common.ADP.CharSize;
                    }
                    flag = true;
                    break;

                case OCI.DATATYPE.DATE:
                    this._metaType = MetaType.GetMetaTypeForType(System.Data.OracleClient.OracleType.DateTime);
                    this._byteSize = this._metaType.BindSize;
                    flag = true;
                    break;

                case OCI.DATATYPE.RAW:
                    this._metaType = MetaType.GetMetaTypeForType(System.Data.OracleClient.OracleType.Raw);
                    this._describeHandle.GetAttribute(OCI.ATTR.OCI_ATTR_FNCODE, out this._byteSize, errorHandle);
                    flag = true;
                    break;

                case OCI.DATATYPE.LONGRAW:
                    this._metaType = MetaType.GetMetaTypeForType(System.Data.OracleClient.OracleType.LongRaw);
                    this._byteSize = this._metaType.BindSize;
                    flag = true;
                    flag2 = true;
                    break;

                case OCI.DATATYPE.CLOB:
                    this._describeHandle.GetAttribute(OCI.ATTR.OCI_ATTR_CHARSET_FORM, out num, errorHandle);
                    this._metaType = MetaType.GetMetaTypeForType((2 == num) ? System.Data.OracleClient.OracleType.NClob : System.Data.OracleClient.OracleType.Clob);
                    this._byteSize = this._metaType.BindSize;
                    flag2 = true;
                    break;

                case OCI.DATATYPE.BLOB:
                    this._metaType = MetaType.GetMetaTypeForType(System.Data.OracleClient.OracleType.Blob);
                    this._byteSize = this._metaType.BindSize;
                    flag2 = true;
                    break;

                case OCI.DATATYPE.BFILE:
                    this._metaType = MetaType.GetMetaTypeForType(System.Data.OracleClient.OracleType.BFile);
                    this._byteSize = this._metaType.BindSize;
                    flag2 = true;
                    break;

                case OCI.DATATYPE.TIMESTAMP:
                    this._metaType = MetaType.GetMetaTypeForType(System.Data.OracleClient.OracleType.Timestamp);
                    this._byteSize = this._metaType.BindSize;
                    flag = true;
                    break;

                case OCI.DATATYPE.TIMESTAMP_TZ:
                    this._metaType = MetaType.GetMetaTypeForType(System.Data.OracleClient.OracleType.TimestampWithTZ);
                    this._byteSize = this._metaType.BindSize;
                    flag = true;
                    break;

                case OCI.DATATYPE.INTERVAL_YM:
                    this._metaType = MetaType.GetMetaTypeForType(System.Data.OracleClient.OracleType.IntervalYearToMonth);
                    this._byteSize = this._metaType.BindSize;
                    break;

                case OCI.DATATYPE.INTERVAL_DS:
                    this._metaType = MetaType.GetMetaTypeForType(System.Data.OracleClient.OracleType.IntervalDayToSecond);
                    this._byteSize = this._metaType.BindSize;
                    break;

                case OCI.DATATYPE.TIMESTAMP_LTZ:
                    this._metaType = MetaType.GetMetaTypeForType(System.Data.OracleClient.OracleType.TimestampLocal);
                    this._byteSize = this._metaType.BindSize;
                    flag = true;
                    break;

                default:
                    throw System.Data.Common.ADP.TypeNotSupported(ociType);
            }
            if (this._isNullable)
            {
                this._indicatorOffset = offset;
                offset += IntPtr.Size;
            }
            else
            {
                this._indicatorOffset = -1;
            }
            if (flag)
            {
                this._lengthOffset = offset;
                offset += IntPtr.Size;
            }
            else
            {
                this._lengthOffset = -1;
            }
            this._valueOffset = offset;
            if ((OCI.DATATYPE.LONG == ociType) || (OCI.DATATYPE.LONGRAW == ociType))
            {
                offset += IntPtr.Size;
            }
            else
            {
                offset += this._byteSize;
            }
            offset = (offset + (IntPtr.Size - 1)) & ~(IntPtr.Size - 1);
            OciHandle.SafeDispose(ref this._describeHandle);
            return flag2;
        }

        internal void Dispose()
        {
            NativeBuffer.SafeDispose(ref this._longBuffer);
            OciLobLocator.SafeDispose(ref this._lobLocator);
            OciHandle.SafeDispose(ref this._describeHandle);
            this._columnName = null;
            this._metaType = null;
            this._callback = null;
            this._connection = null;
        }

        internal void FixupLongValueLength(NativeBuffer buffer)
        {
            if ((this._longBuffer != null) && (-1 == this._longLength))
            {
                this._longLength = this._longBuffer.TotalLengthInBytes;
                if (this._bindAsUTF16)
                {
                    this._longLength /= 2;
                }
                buffer.WriteInt32(this._lengthOffset, this._longLength);
            }
        }

        internal long GetBytes(NativeBuffer_RowBuffer buffer, long fieldOffset, byte[] destinationBuffer, int destinationOffset, int length)
        {
            int num;
            if (length < 0)
            {
                throw System.Data.Common.ADP.InvalidDataLength((long) length);
            }
            if ((destinationOffset < 0) || ((destinationBuffer != null) && (destinationOffset >= destinationBuffer.Length)))
            {
                throw System.Data.Common.ADP.InvalidDestinationBufferIndex(destinationBuffer.Length, destinationOffset, "bufferoffset");
            }
            if ((0L > fieldOffset) || (0xffffffffL < fieldOffset))
            {
                throw System.Data.Common.ADP.InvalidSourceOffset("fieldOffset", 0L, 0xffffffffL);
            }
            if (this.IsLob)
            {
                System.Data.OracleClient.OracleType oracleType = this._metaType.OracleType;
                if ((System.Data.OracleClient.OracleType.Blob != oracleType) && (System.Data.OracleClient.OracleType.BFile != oracleType))
                {
                    throw System.Data.Common.ADP.InvalidCast();
                }
                if (this.IsDBNull(buffer))
                {
                    throw System.Data.Common.ADP.DataReaderNoData();
                }
                using (OracleLob lob = new OracleLob(this._lobLocator))
                {
                    uint num3 = (uint) lob.Length;
                    uint num2 = (uint) fieldOffset;
                    if (num2 > num3)
                    {
                        throw System.Data.Common.ADP.InvalidSourceBufferIndex((int) num3, (long) num2, "fieldOffset");
                    }
                    num = (int) (num3 - num2);
                    if (destinationBuffer != null)
                    {
                        num = Math.Min(num, length);
                        if (0 < num)
                        {
                            lob.Seek((long) num2, SeekOrigin.Begin);
                            lob.Read(destinationBuffer, destinationOffset, num);
                        }
                    }
                    goto Label_0155;
                }
            }
            if ((System.Data.OracleClient.OracleType.Raw != this.OracleType) && (System.Data.OracleClient.OracleType.LongRaw != this.OracleType))
            {
                throw System.Data.Common.ADP.InvalidCast();
            }
            if (this.IsDBNull(buffer))
            {
                throw System.Data.Common.ADP.DataReaderNoData();
            }
            this.FixupLongValueLength(buffer);
            int num5 = OracleBinary.GetLength(buffer, this._lengthOffset, this._metaType);
            int sourceOffset = (int) fieldOffset;
            num = num5 - sourceOffset;
            if (destinationBuffer != null)
            {
                num = Math.Min(num, length);
                if (0 < num)
                {
                    OracleBinary.GetBytes(buffer, this._valueOffset, this._metaType, sourceOffset, destinationBuffer, destinationOffset, num);
                }
            }
        Label_0155:
            return (long) Math.Max(0, num);
        }

        internal long GetChars(NativeBuffer_RowBuffer buffer, long fieldOffset, char[] destinationBuffer, int destinationOffset, int length)
        {
            int num;
            if (length < 0)
            {
                throw System.Data.Common.ADP.InvalidDataLength((long) length);
            }
            if ((destinationOffset < 0) || ((destinationBuffer != null) && (destinationOffset >= destinationBuffer.Length)))
            {
                throw System.Data.Common.ADP.InvalidDestinationBufferIndex(destinationBuffer.Length, destinationOffset, "bufferoffset");
            }
            if ((0L > fieldOffset) || (0xffffffffL < fieldOffset))
            {
                throw System.Data.Common.ADP.InvalidSourceOffset("fieldOffset", 0L, 0xffffffffL);
            }
            if (this.IsLob)
            {
                System.Data.OracleClient.OracleType oracleType = this._metaType.OracleType;
                if (((System.Data.OracleClient.OracleType.Clob != oracleType) && (System.Data.OracleClient.OracleType.NClob != oracleType)) && (System.Data.OracleClient.OracleType.BFile != oracleType))
                {
                    throw System.Data.Common.ADP.InvalidCast();
                }
                if (this.IsDBNull(buffer))
                {
                    throw System.Data.Common.ADP.DataReaderNoData();
                }
                using (OracleLob lob = new OracleLob(this._lobLocator))
                {
                    string str = (string) lob.Value;
                    int maxLen = str.Length;
                    int startIndex = (int) fieldOffset;
                    if (startIndex < 0)
                    {
                        throw System.Data.Common.ADP.InvalidSourceBufferIndex(maxLen, (long) startIndex, "fieldOffset");
                    }
                    num = maxLen - startIndex;
                    if (destinationBuffer != null)
                    {
                        num = Math.Min(num, length);
                        if (0 < num)
                        {
                            Buffer.BlockCopy(str.ToCharArray(startIndex, num), 0, destinationBuffer, destinationOffset, num);
                        }
                    }
                    goto Label_0198;
                }
            }
            if ((((System.Data.OracleClient.OracleType.Char != this.OracleType) && (System.Data.OracleClient.OracleType.VarChar != this.OracleType)) && ((System.Data.OracleClient.OracleType.LongVarChar != this.OracleType) && (System.Data.OracleClient.OracleType.NChar != this.OracleType))) && (System.Data.OracleClient.OracleType.NVarChar != this.OracleType))
            {
                throw System.Data.Common.ADP.InvalidCast();
            }
            if (this.IsDBNull(buffer))
            {
                throw System.Data.Common.ADP.DataReaderNoData();
            }
            this.FixupLongValueLength(buffer);
            int num5 = OracleString.GetLength(buffer, this._lengthOffset, this._metaType);
            int sourceOffset = (int) fieldOffset;
            num = num5 - sourceOffset;
            if (destinationBuffer != null)
            {
                num = Math.Min(num, length);
                if (0 < num)
                {
                    OracleString.GetChars(buffer, this._valueOffset, this._lengthOffset, this._metaType, this._connection, this._bindAsUTF16, sourceOffset, destinationBuffer, destinationOffset, num);
                }
            }
        Label_0198:
            return (long) Math.Max(0, num);
        }

        internal string GetDataTypeName()
        {
            return this._metaType.DataTypeName;
        }

        internal DateTime GetDateTime(NativeBuffer_RowBuffer buffer)
        {
            if (this.IsDBNull(buffer))
            {
                throw System.Data.Common.ADP.DataReaderNoData();
            }
            if (typeof(DateTime) != this._metaType.BaseType)
            {
                throw System.Data.Common.ADP.InvalidCast();
            }
            return OracleDateTime.MarshalToDateTime(buffer, this._valueOffset, this._lengthOffset, this._metaType, this._connection);
        }

        internal decimal GetDecimal(NativeBuffer_RowBuffer buffer)
        {
            if (typeof(decimal) != this._metaType.BaseType)
            {
                throw System.Data.Common.ADP.InvalidCast();
            }
            if (this.IsDBNull(buffer))
            {
                throw System.Data.Common.ADP.DataReaderNoData();
            }
            return OracleNumber.MarshalToDecimal(buffer, this._valueOffset, this._connection);
        }

        internal double GetDouble(NativeBuffer_RowBuffer buffer)
        {
            if (typeof(decimal) != this._metaType.BaseType)
            {
                throw System.Data.Common.ADP.InvalidCast();
            }
            if (this.IsDBNull(buffer))
            {
                throw System.Data.Common.ADP.DataReaderNoData();
            }
            return (double) OracleNumber.MarshalToDecimal(buffer, this._valueOffset, this._connection);
        }

        internal Type GetFieldOracleType()
        {
            return this._metaType.NoConvertType;
        }

        internal Type GetFieldType()
        {
            return this._metaType.BaseType;
        }

        internal float GetFloat(NativeBuffer_RowBuffer buffer)
        {
            if (typeof(decimal) != this._metaType.BaseType)
            {
                throw System.Data.Common.ADP.InvalidCast();
            }
            if (this.IsDBNull(buffer))
            {
                throw System.Data.Common.ADP.DataReaderNoData();
            }
            return (float) OracleNumber.MarshalToDecimal(buffer, this._valueOffset, this._connection);
        }

        internal int GetInt32(NativeBuffer_RowBuffer buffer)
        {
            if ((typeof(int) != this._metaType.BaseType) && (typeof(decimal) != this._metaType.BaseType))
            {
                throw System.Data.Common.ADP.InvalidCast();
            }
            if (this.IsDBNull(buffer))
            {
                throw System.Data.Common.ADP.DataReaderNoData();
            }
            if (typeof(int) == this._metaType.BaseType)
            {
                return OracleMonthSpan.MarshalToInt32(buffer, this._valueOffset);
            }
            return OracleNumber.MarshalToInt32(buffer, this._valueOffset, this._connection);
        }

        internal long GetInt64(NativeBuffer_RowBuffer buffer)
        {
            if (typeof(decimal) != this._metaType.BaseType)
            {
                throw System.Data.Common.ADP.InvalidCast();
            }
            if (this.IsDBNull(buffer))
            {
                throw System.Data.Common.ADP.DataReaderNoData();
            }
            return OracleNumber.MarshalToInt64(buffer, this._valueOffset, this._connection);
        }

        internal OracleBFile GetOracleBFile(NativeBuffer_RowBuffer buffer)
        {
            if (typeof(OracleBFile) != this._metaType.NoConvertType)
            {
                throw System.Data.Common.ADP.InvalidCast();
            }
            if (this.IsDBNull(buffer))
            {
                return OracleBFile.Null;
            }
            return new OracleBFile(this._lobLocator);
        }

        internal OracleBinary GetOracleBinary(NativeBuffer_RowBuffer buffer)
        {
            if (typeof(OracleBinary) != this._metaType.NoConvertType)
            {
                throw System.Data.Common.ADP.InvalidCast();
            }
            this.FixupLongValueLength(buffer);
            if (this.IsDBNull(buffer))
            {
                return OracleBinary.Null;
            }
            return new OracleBinary(buffer, this._valueOffset, this._lengthOffset, this._metaType);
        }

        internal OracleDateTime GetOracleDateTime(NativeBuffer_RowBuffer buffer)
        {
            if (typeof(OracleDateTime) != this._metaType.NoConvertType)
            {
                throw System.Data.Common.ADP.InvalidCast();
            }
            if (this.IsDBNull(buffer))
            {
                return OracleDateTime.Null;
            }
            return new OracleDateTime(buffer, this._valueOffset, this._lengthOffset, this._metaType, this._connection);
        }

        internal OracleLob GetOracleLob(NativeBuffer_RowBuffer buffer)
        {
            if (typeof(OracleLob) != this._metaType.NoConvertType)
            {
                throw System.Data.Common.ADP.InvalidCast();
            }
            if (this.IsDBNull(buffer))
            {
                return OracleLob.Null;
            }
            return new OracleLob(this._lobLocator);
        }

        internal OracleMonthSpan GetOracleMonthSpan(NativeBuffer_RowBuffer buffer)
        {
            if (typeof(OracleMonthSpan) != this._metaType.NoConvertType)
            {
                throw System.Data.Common.ADP.InvalidCast();
            }
            if (this.IsDBNull(buffer))
            {
                return OracleMonthSpan.Null;
            }
            return new OracleMonthSpan(buffer, this._valueOffset);
        }

        internal OracleNumber GetOracleNumber(NativeBuffer_RowBuffer buffer)
        {
            if (typeof(OracleNumber) != this._metaType.NoConvertType)
            {
                throw System.Data.Common.ADP.InvalidCast();
            }
            if (this.IsDBNull(buffer))
            {
                return OracleNumber.Null;
            }
            return new OracleNumber(buffer, this._valueOffset);
        }

        internal OracleString GetOracleString(NativeBuffer_RowBuffer buffer)
        {
            if (typeof(OracleString) != this._metaType.NoConvertType)
            {
                throw System.Data.Common.ADP.InvalidCast();
            }
            if (this.IsDBNull(buffer))
            {
                return OracleString.Null;
            }
            this.FixupLongValueLength(buffer);
            return new OracleString(buffer, this._valueOffset, this._lengthOffset, this._metaType, this._connection, this._bindAsUTF16, false);
        }

        internal OracleTimeSpan GetOracleTimeSpan(NativeBuffer_RowBuffer buffer)
        {
            if (typeof(OracleTimeSpan) != this._metaType.NoConvertType)
            {
                throw System.Data.Common.ADP.InvalidCast();
            }
            if (this.IsDBNull(buffer))
            {
                return OracleTimeSpan.Null;
            }
            return new OracleTimeSpan(buffer, this._valueOffset);
        }

        internal object GetOracleValue(NativeBuffer_RowBuffer buffer)
        {
            switch (this._metaType.OciType)
            {
                case OCI.DATATYPE.VARNUM:
                    return this.GetOracleNumber(buffer);

                case OCI.DATATYPE.LONG:
                case OCI.DATATYPE.VARCHAR2:
                case OCI.DATATYPE.CHAR:
                    return this.GetOracleString(buffer);

                case OCI.DATATYPE.RAW:
                case OCI.DATATYPE.LONGRAW:
                    return this.GetOracleBinary(buffer);

                case OCI.DATATYPE.DATE:
                case OCI.DATATYPE.INT_TIMESTAMP:
                case OCI.DATATYPE.INT_TIMESTAMP_TZ:
                case OCI.DATATYPE.INT_TIMESTAMP_LTZ:
                    return this.GetOracleDateTime(buffer);

                case OCI.DATATYPE.CLOB:
                case OCI.DATATYPE.BLOB:
                    return this.GetOracleLob(buffer);

                case OCI.DATATYPE.BFILE:
                    return this.GetOracleBFile(buffer);

                case OCI.DATATYPE.INT_INTERVAL_YM:
                    return this.GetOracleMonthSpan(buffer);

                case OCI.DATATYPE.INT_INTERVAL_DS:
                    return this.GetOracleTimeSpan(buffer);
            }
            throw System.Data.Common.ADP.TypeNotSupported(this._metaType.OciType);
        }

        internal string GetString(NativeBuffer_RowBuffer buffer)
        {
            if (this.IsLob)
            {
                System.Data.OracleClient.OracleType oracleType = this._metaType.OracleType;
                if (((System.Data.OracleClient.OracleType.Clob != oracleType) && (System.Data.OracleClient.OracleType.NClob != oracleType)) && (System.Data.OracleClient.OracleType.BFile != oracleType))
                {
                    throw System.Data.Common.ADP.InvalidCast();
                }
                if (this.IsDBNull(buffer))
                {
                    throw System.Data.Common.ADP.DataReaderNoData();
                }
                using (OracleLob lob = new OracleLob(this._lobLocator))
                {
                    return (string) lob.Value;
                }
            }
            if (typeof(string) != this._metaType.BaseType)
            {
                throw System.Data.Common.ADP.InvalidCast();
            }
            if (this.IsDBNull(buffer))
            {
                throw System.Data.Common.ADP.DataReaderNoData();
            }
            this.FixupLongValueLength(buffer);
            return OracleString.MarshalToString(buffer, this._valueOffset, this._lengthOffset, this._metaType, this._connection, this._bindAsUTF16, false);
        }

        internal TimeSpan GetTimeSpan(NativeBuffer_RowBuffer buffer)
        {
            if (typeof(TimeSpan) != this._metaType.BaseType)
            {
                throw System.Data.Common.ADP.InvalidCast();
            }
            if (this.IsDBNull(buffer))
            {
                throw System.Data.Common.ADP.DataReaderNoData();
            }
            return OracleTimeSpan.MarshalToTimeSpan(buffer, this._valueOffset);
        }

        internal object GetValue(NativeBuffer_RowBuffer buffer)
        {
            if (this.IsDBNull(buffer))
            {
                return DBNull.Value;
            }
            switch (this._metaType.OciType)
            {
                case OCI.DATATYPE.VARNUM:
                    return this.GetDecimal(buffer);

                case OCI.DATATYPE.LONG:
                case OCI.DATATYPE.VARCHAR2:
                case OCI.DATATYPE.CHAR:
                    return this.GetString(buffer);

                case OCI.DATATYPE.RAW:
                case OCI.DATATYPE.LONGRAW:
                {
                    long num = this.GetBytes(buffer, 0L, null, 0, 0);
                    byte[] destinationBuffer = new byte[num];
                    this.GetBytes(buffer, 0L, destinationBuffer, 0, (int) num);
                    return destinationBuffer;
                }
                case OCI.DATATYPE.DATE:
                case OCI.DATATYPE.INT_TIMESTAMP:
                case OCI.DATATYPE.INT_TIMESTAMP_TZ:
                case OCI.DATATYPE.INT_TIMESTAMP_LTZ:
                    return this.GetDateTime(buffer);

                case OCI.DATATYPE.CLOB:
                case OCI.DATATYPE.BLOB:
                {
                    using (OracleLob lob = this.GetOracleLob(buffer))
                    {
                        return lob.Value;
                    }
                }
                case OCI.DATATYPE.BFILE:
                {
                    using (OracleBFile file = this.GetOracleBFile(buffer))
                    {
                        return file.Value;
                    }
                }
                case OCI.DATATYPE.INT_INTERVAL_YM:
                    return this.GetInt32(buffer);

                case OCI.DATATYPE.INT_INTERVAL_DS:
                    return this.GetTimeSpan(buffer);
            }
            throw System.Data.Common.ADP.TypeNotSupported(this._metaType.OciType);
        }

        internal bool IsDBNull(NativeBuffer_RowBuffer buffer)
        {
            return (this._isNullable && (buffer.ReadInt16(this._indicatorOffset) == -1));
        }

        internal void Rebind(OracleConnection connection, ref bool mustRelease, ref SafeHandle handleToBind)
        {
            handleToBind = null;
            switch (this._metaType.OciType)
            {
                case OCI.DATATYPE.CLOB:
                case OCI.DATATYPE.BLOB:
                case OCI.DATATYPE.BFILE:
                    OciLobLocator.SafeDispose(ref this._lobLocator);
                    this._lobLocator = new OciLobLocator(connection, this._metaType.OracleType);
                    handleToBind = this._lobLocator.Descriptor;
                    break;

                case OCI.DATATYPE.LONGRAW:
                case OCI.DATATYPE.LONG:
                    this._rowBuffer.WriteInt32(this._lengthOffset, 0);
                    this._longLength = -1;
                    if (this._longBuffer != null)
                    {
                        this._longBuffer.Reset();
                    }
                    else
                    {
                        this._longBuffer = new NativeBuffer_LongColumnData();
                    }
                    handleToBind = this._longBuffer;
                    break;
            }
            if (handleToBind != null)
            {
                handleToBind.DangerousAddRef(ref mustRelease);
                this._rowBuffer.WriteIntPtr(this._valueOffset, handleToBind.DangerousGetHandle());
            }
        }

        internal string ColumnName
        {
            get
            {
                return this._columnName;
            }
        }

        internal bool IsLob
        {
            get
            {
                return this._metaType.IsLob;
            }
        }

        internal bool IsLong
        {
            get
            {
                return this._metaType.IsLong;
            }
        }

        internal bool IsNullable
        {
            get
            {
                return this._isNullable;
            }
        }

        internal System.Data.OracleClient.OracleType OracleType
        {
            get
            {
                return this._metaType.OracleType;
            }
        }

        internal int Ordinal
        {
            get
            {
                return this._ordinal;
            }
        }

        internal byte Precision
        {
            get
            {
                return this._precision;
            }
        }

        internal byte Scale
        {
            get
            {
                return this._scale;
            }
        }

        internal int SchemaTableSize
        {
            get
            {
                if (this._bindAsUTF16 && !this._metaType.IsLong)
                {
                    return (this._byteSize / 2);
                }
                return this._byteSize;
            }
        }
    }
}

