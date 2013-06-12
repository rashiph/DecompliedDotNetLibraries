namespace System.Data.SqlClient
{
    using Microsoft.SqlServer.Server;
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlTypes;
    using System.Diagnostics;
    using System.Text;

    internal class TdsValueSetter
    {
        private System.Text.Encoder _encoder;
        private bool _isPlp;
        private SmiMetaData _metaData;
        private bool _plpUnknownSent;
        private TdsParserStateObject _stateObj;
        private SmiMetaData _variantType;

        internal TdsValueSetter(TdsParserStateObject stateObj, SmiMetaData md)
        {
            this._stateObj = stateObj;
            this._metaData = md;
            this._isPlp = MetaDataUtilsSmi.IsPlpFormat(md);
            this._plpUnknownSent = false;
            this._encoder = null;
        }

        [Conditional("DEBUG")]
        private void CheckSettingOffset(long offset)
        {
        }

        internal void SetBoolean(bool value)
        {
            if (SqlDbType.Variant == this._metaData.SqlDbType)
            {
                this._stateObj.Parser.WriteSqlVariantHeader(3, 50, 0, this._stateObj);
            }
            else
            {
                this._stateObj.Parser.WriteByte((byte) this._metaData.MaxLength, this._stateObj);
            }
            if (value)
            {
                this._stateObj.Parser.WriteByte(1, this._stateObj);
            }
            else
            {
                this._stateObj.Parser.WriteByte(0, this._stateObj);
            }
        }

        internal void SetByte(byte value)
        {
            if (SqlDbType.Variant == this._metaData.SqlDbType)
            {
                this._stateObj.Parser.WriteSqlVariantHeader(3, 0x30, 0, this._stateObj);
            }
            else
            {
                this._stateObj.Parser.WriteByte((byte) this._metaData.MaxLength, this._stateObj);
            }
            this._stateObj.Parser.WriteByte(value, this._stateObj);
        }

        internal int SetBytes(long fieldOffset, byte[] buffer, int bufferOffset, int length)
        {
            this.SetBytesNoOffsetHandling(fieldOffset, buffer, bufferOffset, length);
            return length;
        }

        internal void SetBytesLength(long length)
        {
            if (0L == length)
            {
                if (this._isPlp)
                {
                    this._stateObj.Parser.WriteLong(0L, this._stateObj);
                    this._plpUnknownSent = true;
                }
                else
                {
                    if (SqlDbType.Variant == this._metaData.SqlDbType)
                    {
                        this._stateObj.Parser.WriteSqlVariantHeader(4, 0xa5, 2, this._stateObj);
                    }
                    this._stateObj.Parser.WriteShort(0, this._stateObj);
                }
            }
            if (this._plpUnknownSent)
            {
                this._stateObj.Parser.WriteInt(0, this._stateObj);
                this._plpUnknownSent = false;
            }
        }

        private void SetBytesNoOffsetHandling(long fieldOffset, byte[] buffer, int bufferOffset, int length)
        {
            if (this._isPlp)
            {
                if (!this._plpUnknownSent)
                {
                    this._stateObj.Parser.WriteUnsignedLong(18446744073709551614L, this._stateObj);
                    this._plpUnknownSent = true;
                }
                this._stateObj.Parser.WriteInt(length, this._stateObj);
                this._stateObj.Parser.WriteByteArray(buffer, length, bufferOffset, this._stateObj);
            }
            else
            {
                if (SqlDbType.Variant == this._metaData.SqlDbType)
                {
                    this._stateObj.Parser.WriteSqlVariantHeader(4 + length, 0xa5, 2, this._stateObj);
                }
                this._stateObj.Parser.WriteShort(length, this._stateObj);
                this._stateObj.Parser.WriteByteArray(buffer, length, bufferOffset, this._stateObj);
            }
        }

        internal int SetChars(long fieldOffset, char[] buffer, int bufferOffset, int length)
        {
            if (MetaDataUtilsSmi.IsAnsiType(this._metaData.SqlDbType))
            {
                if (this._encoder == null)
                {
                    this._encoder = this._stateObj.Parser._defaultEncoding.GetEncoder();
                }
                byte[] bytes = new byte[this._encoder.GetByteCount(buffer, bufferOffset, length, false)];
                this._encoder.GetBytes(buffer, bufferOffset, length, bytes, 0, false);
                this.SetBytesNoOffsetHandling(fieldOffset, bytes, 0, bytes.Length);
                return length;
            }
            if (this._isPlp)
            {
                if (!this._plpUnknownSent)
                {
                    this._stateObj.Parser.WriteUnsignedLong(18446744073709551614L, this._stateObj);
                    this._plpUnknownSent = true;
                }
                this._stateObj.Parser.WriteInt(length * ADP.CharSize, this._stateObj);
                this._stateObj.Parser.WriteCharArray(buffer, length, bufferOffset, this._stateObj);
                return length;
            }
            if (SqlDbType.Variant == this._metaData.SqlDbType)
            {
                this._stateObj.Parser.WriteSqlVariantValue(new string(buffer, bufferOffset, length), length, 0, this._stateObj);
                return length;
            }
            this._stateObj.Parser.WriteShort(length * ADP.CharSize, this._stateObj);
            this._stateObj.Parser.WriteCharArray(buffer, length, bufferOffset, this._stateObj);
            return length;
        }

        internal void SetCharsLength(long length)
        {
            if (0L == length)
            {
                if (this._isPlp)
                {
                    this._stateObj.Parser.WriteLong(0L, this._stateObj);
                    this._plpUnknownSent = true;
                }
                else
                {
                    this._stateObj.Parser.WriteShort(0, this._stateObj);
                }
            }
            if (this._plpUnknownSent)
            {
                this._stateObj.Parser.WriteInt(0, this._stateObj);
                this._plpUnknownSent = false;
            }
            this._encoder = null;
        }

        internal void SetDateTime(DateTime value)
        {
            if (SqlDbType.Variant == this._metaData.SqlDbType)
            {
                TdsDateTime time3 = MetaType.FromDateTime(value, 8);
                this._stateObj.Parser.WriteSqlVariantHeader(10, 0x3d, 0, this._stateObj);
                this._stateObj.Parser.WriteInt(time3.days, this._stateObj);
                this._stateObj.Parser.WriteInt(time3.time, this._stateObj);
            }
            else
            {
                this._stateObj.Parser.WriteByte((byte) this._metaData.MaxLength, this._stateObj);
                if (SqlDbType.SmallDateTime == this._metaData.SqlDbType)
                {
                    TdsDateTime time2 = MetaType.FromDateTime(value, (byte) this._metaData.MaxLength);
                    this._stateObj.Parser.WriteShort(time2.days, this._stateObj);
                    this._stateObj.Parser.WriteShort(time2.time, this._stateObj);
                }
                else if (SqlDbType.DateTime == this._metaData.SqlDbType)
                {
                    TdsDateTime time = MetaType.FromDateTime(value, (byte) this._metaData.MaxLength);
                    this._stateObj.Parser.WriteInt(time.days, this._stateObj);
                    this._stateObj.Parser.WriteInt(time.time, this._stateObj);
                }
                else
                {
                    int days = value.Subtract(DateTime.MinValue).Days;
                    if (SqlDbType.DateTime2 == this._metaData.SqlDbType)
                    {
                        long num = value.TimeOfDay.Ticks / TdsEnums.TICKS_FROM_SCALE[this._metaData.Scale];
                        this._stateObj.Parser.WriteByteArray(BitConverter.GetBytes(num), ((int) this._metaData.MaxLength) - 3, 0, this._stateObj);
                    }
                    this._stateObj.Parser.WriteByteArray(BitConverter.GetBytes(days), 3, 0, this._stateObj);
                }
            }
        }

        internal void SetDateTimeOffset(DateTimeOffset value)
        {
            byte fixedLength;
            byte scale;
            if (SqlDbType.Variant == this._metaData.SqlDbType)
            {
                scale = MetaType.MetaDateTimeOffset.Scale;
                fixedLength = (byte) MetaType.MetaDateTimeOffset.FixedLength;
                this._stateObj.Parser.WriteSqlVariantHeader(13, 0x2b, 1, this._stateObj);
                this._stateObj.Parser.WriteByte(scale, this._stateObj);
            }
            else
            {
                scale = this._metaData.Scale;
                fixedLength = (byte) this._metaData.MaxLength;
                this._stateObj.Parser.WriteByte(fixedLength, this._stateObj);
            }
            DateTime utcDateTime = value.UtcDateTime;
            long num5 = utcDateTime.TimeOfDay.Ticks / TdsEnums.TICKS_FROM_SCALE[scale];
            int days = utcDateTime.Subtract(DateTime.MinValue).Days;
            short totalMinutes = (short) value.Offset.TotalMinutes;
            this._stateObj.Parser.WriteByteArray(BitConverter.GetBytes(num5), fixedLength - 5, 0, this._stateObj);
            this._stateObj.Parser.WriteByteArray(BitConverter.GetBytes(days), 3, 0, this._stateObj);
            this._stateObj.Parser.WriteByte((byte) (totalMinutes & 0xff), this._stateObj);
            this._stateObj.Parser.WriteByte((byte) ((totalMinutes >> 8) & 0xff), this._stateObj);
        }

        internal void SetDBNull()
        {
            if (this._isPlp)
            {
                this._stateObj.Parser.WriteUnsignedLong(ulong.MaxValue, this._stateObj);
            }
            else
            {
                switch (this._metaData.SqlDbType)
                {
                    case SqlDbType.BigInt:
                    case SqlDbType.Bit:
                    case SqlDbType.DateTime:
                    case SqlDbType.Decimal:
                    case SqlDbType.Float:
                    case SqlDbType.Int:
                    case SqlDbType.Money:
                    case SqlDbType.Real:
                    case SqlDbType.UniqueIdentifier:
                    case SqlDbType.SmallDateTime:
                    case SqlDbType.SmallInt:
                    case SqlDbType.SmallMoney:
                    case SqlDbType.TinyInt:
                    case SqlDbType.Date:
                    case SqlDbType.Time:
                    case SqlDbType.DateTime2:
                    case SqlDbType.DateTimeOffset:
                        this._stateObj.Parser.WriteByte(0, this._stateObj);
                        return;

                    case SqlDbType.Binary:
                    case SqlDbType.Char:
                    case SqlDbType.Image:
                    case SqlDbType.NChar:
                    case SqlDbType.NText:
                    case SqlDbType.NVarChar:
                    case SqlDbType.Text:
                    case SqlDbType.Timestamp:
                    case SqlDbType.VarBinary:
                    case SqlDbType.VarChar:
                        this._stateObj.Parser.WriteShort(0xffff, this._stateObj);
                        return;

                    case SqlDbType.Variant:
                        this._stateObj.Parser.WriteInt(0, this._stateObj);
                        break;

                    case (SqlDbType.SmallInt | SqlDbType.Int):
                    case SqlDbType.Xml:
                    case (SqlDbType.Text | SqlDbType.Int):
                    case (SqlDbType.Xml | SqlDbType.Bit):
                    case (SqlDbType.TinyInt | SqlDbType.Int):
                    case SqlDbType.Udt:
                    case SqlDbType.Structured:
                        break;

                    default:
                        return;
                }
            }
        }

        internal void SetDouble(double value)
        {
            if (SqlDbType.Variant == this._metaData.SqlDbType)
            {
                this._stateObj.Parser.WriteSqlVariantHeader(10, 0x3e, 0, this._stateObj);
            }
            else
            {
                this._stateObj.Parser.WriteByte((byte) this._metaData.MaxLength, this._stateObj);
            }
            this._stateObj.Parser.WriteDouble(value, this._stateObj);
        }

        internal void SetGuid(Guid value)
        {
            byte[] b = value.ToByteArray();
            if (SqlDbType.Variant == this._metaData.SqlDbType)
            {
                this._stateObj.Parser.WriteSqlVariantHeader(0x12, 0x24, 0, this._stateObj);
            }
            else
            {
                this._stateObj.Parser.WriteByte((byte) this._metaData.MaxLength, this._stateObj);
            }
            this._stateObj.Parser.WriteByteArray(b, b.Length, 0, this._stateObj);
        }

        internal void SetInt16(short value)
        {
            if (SqlDbType.Variant == this._metaData.SqlDbType)
            {
                this._stateObj.Parser.WriteSqlVariantHeader(4, 0x34, 0, this._stateObj);
            }
            else
            {
                this._stateObj.Parser.WriteByte((byte) this._metaData.MaxLength, this._stateObj);
            }
            this._stateObj.Parser.WriteShort(value, this._stateObj);
        }

        internal void SetInt32(int value)
        {
            if (SqlDbType.Variant == this._metaData.SqlDbType)
            {
                this._stateObj.Parser.WriteSqlVariantHeader(6, 0x38, 0, this._stateObj);
            }
            else
            {
                this._stateObj.Parser.WriteByte((byte) this._metaData.MaxLength, this._stateObj);
            }
            this._stateObj.Parser.WriteInt(value, this._stateObj);
        }

        internal void SetInt64(long value)
        {
            if (SqlDbType.Variant == this._metaData.SqlDbType)
            {
                if (this._variantType == null)
                {
                    this._stateObj.Parser.WriteSqlVariantHeader(10, 0x7f, 0, this._stateObj);
                    this._stateObj.Parser.WriteLong(value, this._stateObj);
                }
                else
                {
                    this._stateObj.Parser.WriteSqlVariantHeader(10, 60, 0, this._stateObj);
                    this._stateObj.Parser.WriteInt((int) (value >> 0x20), this._stateObj);
                    this._stateObj.Parser.WriteInt((int) value, this._stateObj);
                    this._variantType = null;
                }
            }
            else
            {
                this._stateObj.Parser.WriteByte((byte) this._metaData.MaxLength, this._stateObj);
                if (SqlDbType.SmallMoney == this._metaData.SqlDbType)
                {
                    this._stateObj.Parser.WriteInt((int) value, this._stateObj);
                }
                else if (SqlDbType.Money == this._metaData.SqlDbType)
                {
                    this._stateObj.Parser.WriteInt((int) (value >> 0x20), this._stateObj);
                    this._stateObj.Parser.WriteInt((int) value, this._stateObj);
                }
                else
                {
                    this._stateObj.Parser.WriteLong(value, this._stateObj);
                }
            }
        }

        internal void SetSingle(float value)
        {
            if (SqlDbType.Variant == this._metaData.SqlDbType)
            {
                this._stateObj.Parser.WriteSqlVariantHeader(6, 0x3b, 0, this._stateObj);
            }
            else
            {
                this._stateObj.Parser.WriteByte((byte) this._metaData.MaxLength, this._stateObj);
            }
            this._stateObj.Parser.WriteFloat(value, this._stateObj);
        }

        internal void SetSqlDecimal(SqlDecimal value)
        {
            if (SqlDbType.Variant == this._metaData.SqlDbType)
            {
                this._stateObj.Parser.WriteSqlVariantHeader(0x15, 0x6c, 2, this._stateObj);
                this._stateObj.Parser.WriteByte(value.Precision, this._stateObj);
                this._stateObj.Parser.WriteByte(value.Scale, this._stateObj);
                this._stateObj.Parser.WriteSqlDecimal(value, this._stateObj);
            }
            else
            {
                this._stateObj.Parser.WriteByte((byte) MetaType.MetaDecimal.FixedLength, this._stateObj);
                this._stateObj.Parser.WriteSqlDecimal(SqlDecimal.ConvertToPrecScale(value, this._metaData.Precision, this._metaData.Scale), this._stateObj);
            }
        }

        internal void SetString(string value, int offset, int length)
        {
            if (MetaDataUtilsSmi.IsAnsiType(this._metaData.SqlDbType))
            {
                byte[] bytes;
                if ((offset == 0) && (value.Length <= length))
                {
                    bytes = this._stateObj.Parser._defaultEncoding.GetBytes(value);
                }
                else
                {
                    char[] chars = value.ToCharArray(offset, length);
                    bytes = this._stateObj.Parser._defaultEncoding.GetBytes(chars);
                }
                this.SetBytes(0L, bytes, 0, bytes.Length);
                this.SetBytesLength((long) bytes.Length);
            }
            else if (SqlDbType.Variant == this._metaData.SqlDbType)
            {
                SqlCollation collation = new SqlCollation {
                    LCID = (int) this._variantType.LocaleId,
                    SqlCompareOptions = this._variantType.CompareOptions
                };
                if ((length * ADP.CharSize) > 0x1f40)
                {
                    byte[] buffer;
                    if ((offset == 0) && (value.Length <= length))
                    {
                        buffer = this._stateObj.Parser._defaultEncoding.GetBytes(value);
                    }
                    else
                    {
                        buffer = this._stateObj.Parser._defaultEncoding.GetBytes(value.ToCharArray(offset, length));
                    }
                    this._stateObj.Parser.WriteSqlVariantHeader(9 + buffer.Length, 0xa7, 7, this._stateObj);
                    this._stateObj.Parser.WriteUnsignedInt(collation.info, this._stateObj);
                    this._stateObj.Parser.WriteByte(collation.sortId, this._stateObj);
                    this._stateObj.Parser.WriteShort(buffer.Length, this._stateObj);
                    this._stateObj.Parser.WriteByteArray(buffer, buffer.Length, 0, this._stateObj);
                }
                else
                {
                    this._stateObj.Parser.WriteSqlVariantHeader(9 + (length * ADP.CharSize), 0xe7, 7, this._stateObj);
                    this._stateObj.Parser.WriteUnsignedInt(collation.info, this._stateObj);
                    this._stateObj.Parser.WriteByte(collation.sortId, this._stateObj);
                    this._stateObj.Parser.WriteShort(length * ADP.CharSize, this._stateObj);
                    this._stateObj.Parser.WriteString(value, length, offset, this._stateObj);
                }
                this._variantType = null;
            }
            else if (this._isPlp)
            {
                this._stateObj.Parser.WriteLong((long) (length * ADP.CharSize), this._stateObj);
                this._stateObj.Parser.WriteInt(length * ADP.CharSize, this._stateObj);
                this._stateObj.Parser.WriteString(value, length, offset, this._stateObj);
                if (length != 0)
                {
                    this._stateObj.Parser.WriteInt(0, this._stateObj);
                }
            }
            else
            {
                this._stateObj.Parser.WriteShort(length * ADP.CharSize, this._stateObj);
                this._stateObj.Parser.WriteString(value, length, offset, this._stateObj);
            }
        }

        internal void SetTimeSpan(TimeSpan value)
        {
            byte maxLength;
            byte scale;
            if (SqlDbType.Variant == this._metaData.SqlDbType)
            {
                scale = SmiMetaData.DefaultTime.Scale;
                maxLength = (byte) SmiMetaData.DefaultTime.MaxLength;
                this._stateObj.Parser.WriteSqlVariantHeader(8, 0x29, 1, this._stateObj);
                this._stateObj.Parser.WriteByte(scale, this._stateObj);
            }
            else
            {
                scale = this._metaData.Scale;
                maxLength = (byte) this._metaData.MaxLength;
                this._stateObj.Parser.WriteByte(maxLength, this._stateObj);
            }
            long num3 = value.Ticks / TdsEnums.TICKS_FROM_SCALE[scale];
            this._stateObj.Parser.WriteByteArray(BitConverter.GetBytes(num3), maxLength, 0, this._stateObj);
        }

        internal void SetVariantType(SmiMetaData value)
        {
            this._variantType = value;
        }
    }
}

