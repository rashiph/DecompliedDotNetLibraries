namespace System.Data.SqlClient
{
    using System;
    using System.Data.SqlTypes;
    using System.Globalization;
    using System.Runtime.InteropServices;

    internal sealed class SqlBuffer
    {
        private static string[] __katmaiDateTime2FormatByScale = new string[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss.f", "yyyy-MM-dd HH:mm:ss.ff", "yyyy-MM-dd HH:mm:ss.fff", "yyyy-MM-dd HH:mm:ss.ffff", "yyyy-MM-dd HH:mm:ss.fffff", "yyyy-MM-dd HH:mm:ss.ffffff", "yyyy-MM-dd HH:mm:ss.fffffff" };
        private static string[] __katmaiDateTimeOffsetFormatByScale = new string[] { "yyyy-MM-dd HH:mm:ss zzz", "yyyy-MM-dd HH:mm:ss.f zzz", "yyyy-MM-dd HH:mm:ss.ff zzz", "yyyy-MM-dd HH:mm:ss.fff zzz", "yyyy-MM-dd HH:mm:ss.ffff zzz", "yyyy-MM-dd HH:mm:ss.fffff zzz", "yyyy-MM-dd HH:mm:ss.ffffff zzz", "yyyy-MM-dd HH:mm:ss.fffffff zzz" };
        private static string[] __katmaiTimeFormatByScale = new string[] { "HH:mm:ss", "HH:mm:ss.f", "HH:mm:ss.ff", "HH:mm:ss.fff", "HH:mm:ss.ffff", "HH:mm:ss.fffff", "HH:mm:ss.ffffff", "HH:mm:ss.fffffff" };
        private bool _isNull;
        private object _object;
        private StorageType _type;
        private Storage _value;

        internal SqlBuffer()
        {
        }

        private SqlBuffer(SqlBuffer value)
        {
            this._isNull = value._isNull;
            this._type = value._type;
            this._value = value._value;
            this._object = value._object;
        }

        internal void Clear()
        {
            this._isNull = false;
            this._type = StorageType.Empty;
            this._object = null;
        }

        internal static void Clear(SqlBuffer[] values)
        {
            if (values != null)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i].Clear();
                }
            }
        }

        internal static SqlBuffer[] CloneBufferArray(SqlBuffer[] values)
        {
            SqlBuffer[] bufferArray = new SqlBuffer[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                bufferArray[i] = new SqlBuffer(values[i]);
            }
            return bufferArray;
        }

        internal static SqlBuffer[] CreateBufferArray(int length)
        {
            SqlBuffer[] bufferArray = new SqlBuffer[length];
            for (int i = 0; i < bufferArray.Length; i++)
            {
                bufferArray[i] = new SqlBuffer();
            }
            return bufferArray;
        }

        private static void FillInTimeInfo(ref TimeInfo timeInfo, byte[] timeBytes, int length, byte scale)
        {
            long num = (timeBytes[0] + (timeBytes[1] << 8)) + (timeBytes[2] << 0x10);
            if (length > 3)
            {
                num += timeBytes[3] << 0x18;
            }
            if (length > 4)
            {
                num += timeBytes[4] << 0x20;
            }
            timeInfo.ticks = num * TdsEnums.TICKS_FROM_SCALE[scale];
            timeInfo.scale = scale;
        }

        private static int GetDateFromByteArray(byte[] buf, int offset)
        {
            return ((buf[offset] + (buf[offset + 1] << 8)) + (buf[offset + 2] << 0x10));
        }

        private static long GetTicksFromDateTime2Info(DateTime2Info dateTime2Info)
        {
            return ((dateTime2Info.date * 0xc92a69c000L) + dateTime2Info.timeInfo.ticks);
        }

        internal Type GetTypeFromStorageType(bool isSqlType)
        {
            if (isSqlType)
            {
                switch (this._type)
                {
                    case StorageType.Empty:
                        return null;

                    case StorageType.Boolean:
                        return typeof(System.Data.SqlTypes.SqlBoolean);

                    case StorageType.Byte:
                        return typeof(System.Data.SqlTypes.SqlByte);

                    case StorageType.DateTime:
                        return typeof(System.Data.SqlTypes.SqlDateTime);

                    case StorageType.Decimal:
                        return typeof(System.Data.SqlTypes.SqlDecimal);

                    case StorageType.Double:
                        return typeof(System.Data.SqlTypes.SqlDouble);

                    case StorageType.Int16:
                        return typeof(System.Data.SqlTypes.SqlInt16);

                    case StorageType.Int32:
                        return typeof(System.Data.SqlTypes.SqlInt32);

                    case StorageType.Int64:
                        return typeof(System.Data.SqlTypes.SqlInt64);

                    case StorageType.Money:
                        return typeof(System.Data.SqlTypes.SqlMoney);

                    case StorageType.Single:
                        return typeof(System.Data.SqlTypes.SqlSingle);

                    case StorageType.String:
                        return typeof(System.Data.SqlTypes.SqlString);

                    case StorageType.SqlBinary:
                        return typeof(object);

                    case StorageType.SqlCachedBuffer:
                        return typeof(System.Data.SqlTypes.SqlString);

                    case StorageType.SqlGuid:
                        return typeof(object);

                    case StorageType.SqlXml:
                        return typeof(System.Data.SqlTypes.SqlXml);
                }
            }
            else
            {
                switch (this._type)
                {
                    case StorageType.Empty:
                        return null;

                    case StorageType.Boolean:
                        return typeof(bool);

                    case StorageType.Byte:
                        return typeof(byte);

                    case StorageType.DateTime:
                        return typeof(System.DateTime);

                    case StorageType.Decimal:
                        return typeof(decimal);

                    case StorageType.Double:
                        return typeof(double);

                    case StorageType.Int16:
                        return typeof(short);

                    case StorageType.Int32:
                        return typeof(int);

                    case StorageType.Int64:
                        return typeof(long);

                    case StorageType.Money:
                        return typeof(decimal);

                    case StorageType.Single:
                        return typeof(float);

                    case StorageType.String:
                        return typeof(string);

                    case StorageType.SqlBinary:
                        return typeof(byte[]);

                    case StorageType.SqlCachedBuffer:
                        return typeof(string);

                    case StorageType.SqlGuid:
                        return typeof(System.Guid);

                    case StorageType.SqlXml:
                        return typeof(string);
                }
            }
            return null;
        }

        internal void SetToDate(byte[] bytes)
        {
            this._type = StorageType.Date;
            this._value._int32 = GetDateFromByteArray(bytes, 0);
            this._isNull = false;
        }

        internal void SetToDate(System.DateTime date)
        {
            this._type = StorageType.Date;
            this._value._int32 = date.Subtract(System.DateTime.MinValue).Days;
            this._isNull = false;
        }

        internal void SetToDateTime(int daypart, int timepart)
        {
            this._value._dateTimeInfo.daypart = daypart;
            this._value._dateTimeInfo.timepart = timepart;
            this._type = StorageType.DateTime;
            this._isNull = false;
        }

        internal void SetToDateTime2(System.DateTime dateTime, byte scale)
        {
            this._type = StorageType.DateTime2;
            this._value._dateTime2Info.timeInfo.ticks = dateTime.TimeOfDay.Ticks;
            this._value._dateTime2Info.timeInfo.scale = scale;
            this._value._dateTime2Info.date = dateTime.Subtract(System.DateTime.MinValue).Days;
            this._isNull = false;
        }

        internal void SetToDateTime2(byte[] bytes, int length, byte scale)
        {
            this._type = StorageType.DateTime2;
            FillInTimeInfo(ref this._value._dateTime2Info.timeInfo, bytes, length - 3, scale);
            this._value._dateTime2Info.date = GetDateFromByteArray(bytes, length - 3);
            this._isNull = false;
        }

        internal void SetToDateTimeOffset(System.DateTimeOffset dateTimeOffset, byte scale)
        {
            this._type = StorageType.DateTimeOffset;
            System.DateTime utcDateTime = dateTimeOffset.UtcDateTime;
            this._value._dateTimeOffsetInfo.dateTime2Info.timeInfo.ticks = utcDateTime.TimeOfDay.Ticks;
            this._value._dateTimeOffsetInfo.dateTime2Info.timeInfo.scale = scale;
            this._value._dateTimeOffsetInfo.dateTime2Info.date = utcDateTime.Subtract(System.DateTime.MinValue).Days;
            this._value._dateTimeOffsetInfo.offset = (short) dateTimeOffset.Offset.TotalMinutes;
            this._isNull = false;
        }

        internal void SetToDateTimeOffset(byte[] bytes, int length, byte scale)
        {
            this._type = StorageType.DateTimeOffset;
            FillInTimeInfo(ref this._value._dateTimeOffsetInfo.dateTime2Info.timeInfo, bytes, length - 5, scale);
            this._value._dateTimeOffsetInfo.dateTime2Info.date = GetDateFromByteArray(bytes, length - 5);
            this._value._dateTimeOffsetInfo.offset = (short) (bytes[length - 2] + (bytes[length - 1] << 8));
            this._isNull = false;
        }

        internal void SetToDecimal(byte precision, byte scale, bool positive, int[] bits)
        {
            this._value._numericInfo.precision = precision;
            this._value._numericInfo.scale = scale;
            this._value._numericInfo.positive = positive;
            this._value._numericInfo.data1 = bits[0];
            this._value._numericInfo.data2 = bits[1];
            this._value._numericInfo.data3 = bits[2];
            this._value._numericInfo.data4 = bits[3];
            this._type = StorageType.Decimal;
            this._isNull = false;
        }

        internal void SetToMoney(long value)
        {
            this._value._int64 = value;
            this._type = StorageType.Money;
            this._isNull = false;
        }

        internal void SetToNullOfType(StorageType storageType)
        {
            this._type = storageType;
            this._isNull = true;
            this._object = null;
        }

        internal void SetToString(string value)
        {
            this._object = value;
            this._type = StorageType.String;
            this._isNull = false;
        }

        internal void SetToTime(TimeSpan timeSpan, byte scale)
        {
            this._type = StorageType.Time;
            this._value._timeInfo.ticks = timeSpan.Ticks;
            this._value._timeInfo.scale = scale;
            this._isNull = false;
        }

        internal void SetToTime(byte[] bytes, int length, byte scale)
        {
            this._type = StorageType.Time;
            FillInTimeInfo(ref this._value._timeInfo, bytes, length, scale);
            this._isNull = false;
        }

        private void ThrowIfNull()
        {
            if (this.IsNull)
            {
                throw new SqlNullValueException();
            }
        }

        internal bool Boolean
        {
            get
            {
                this.ThrowIfNull();
                if (StorageType.Boolean == this._type)
                {
                    return this._value._boolean;
                }
                return (bool) this.Value;
            }
            set
            {
                this._value._boolean = value;
                this._type = StorageType.Boolean;
                this._isNull = false;
            }
        }

        internal byte Byte
        {
            get
            {
                this.ThrowIfNull();
                if (StorageType.Byte == this._type)
                {
                    return this._value._byte;
                }
                return (byte) this.Value;
            }
            set
            {
                this._value._byte = value;
                this._type = StorageType.Byte;
                this._isNull = false;
            }
        }

        internal byte[] ByteArray
        {
            get
            {
                this.ThrowIfNull();
                return this.SqlBinary.Value;
            }
        }

        internal System.DateTime DateTime
        {
            get
            {
                this.ThrowIfNull();
                if (StorageType.Date == this._type)
                {
                    return System.DateTime.MinValue.AddDays((double) this._value._int32);
                }
                if (StorageType.DateTime2 == this._type)
                {
                    return new System.DateTime(GetTicksFromDateTime2Info(this._value._dateTime2Info));
                }
                if (StorageType.DateTime == this._type)
                {
                    return System.Data.SqlTypes.SqlDateTime.ToDateTime(this._value._dateTimeInfo.daypart, this._value._dateTimeInfo.timepart);
                }
                return (System.DateTime) this.Value;
            }
        }

        internal System.DateTimeOffset DateTimeOffset
        {
            get
            {
                this.ThrowIfNull();
                if (StorageType.DateTimeOffset == this._type)
                {
                    TimeSpan offset = new TimeSpan(0, this._value._dateTimeOffsetInfo.offset, 0);
                    return new System.DateTimeOffset(GetTicksFromDateTime2Info(this._value._dateTimeOffsetInfo.dateTime2Info) + offset.Ticks, offset);
                }
                return (System.DateTimeOffset) this.Value;
            }
        }

        internal decimal Decimal
        {
            get
            {
                this.ThrowIfNull();
                if (StorageType.Decimal == this._type)
                {
                    if ((this._value._numericInfo.data4 != 0) || (this._value._numericInfo.scale > 0x1c))
                    {
                        throw new OverflowException(SQLResource.ConversionOverflowMessage);
                    }
                    return new decimal(this._value._numericInfo.data1, this._value._numericInfo.data2, this._value._numericInfo.data3, !this._value._numericInfo.positive, this._value._numericInfo.scale);
                }
                if (StorageType.Money != this._type)
                {
                    return (decimal) this.Value;
                }
                long num = this._value._int64;
                bool isNegative = false;
                if (num < 0L)
                {
                    isNegative = true;
                    num = -num;
                }
                return new decimal((int) (((ulong) num) & 0xffffffffL), (int) (num >> 0x20), 0, isNegative, 4);
            }
        }

        internal double Double
        {
            get
            {
                this.ThrowIfNull();
                if (StorageType.Double == this._type)
                {
                    return this._value._double;
                }
                return (double) this.Value;
            }
            set
            {
                this._value._double = value;
                this._type = StorageType.Double;
                this._isNull = false;
            }
        }

        internal System.Guid Guid
        {
            get
            {
                this.ThrowIfNull();
                return this.SqlGuid.Value;
            }
        }

        internal short Int16
        {
            get
            {
                this.ThrowIfNull();
                if (StorageType.Int16 == this._type)
                {
                    return this._value._int16;
                }
                return (short) this.Value;
            }
            set
            {
                this._value._int16 = value;
                this._type = StorageType.Int16;
                this._isNull = false;
            }
        }

        internal int Int32
        {
            get
            {
                this.ThrowIfNull();
                if (StorageType.Int32 == this._type)
                {
                    return this._value._int32;
                }
                return (int) this.Value;
            }
            set
            {
                this._value._int32 = value;
                this._type = StorageType.Int32;
                this._isNull = false;
            }
        }

        internal long Int64
        {
            get
            {
                this.ThrowIfNull();
                if (StorageType.Int64 == this._type)
                {
                    return this._value._int64;
                }
                return (long) this.Value;
            }
            set
            {
                this._value._int64 = value;
                this._type = StorageType.Int64;
                this._isNull = false;
            }
        }

        internal bool IsEmpty
        {
            get
            {
                return (StorageType.Empty == this._type);
            }
        }

        internal bool IsNull
        {
            get
            {
                return this._isNull;
            }
        }

        internal System.Data.SqlTypes.SqlString KatmaiDateTimeSqlString
        {
            get
            {
                if (((StorageType.Date != this._type) && (StorageType.Time != this._type)) && ((StorageType.DateTime2 != this._type) && (StorageType.DateTimeOffset != this._type)))
                {
                    return (System.Data.SqlTypes.SqlString) this.SqlValue;
                }
                if (this.IsNull)
                {
                    return System.Data.SqlTypes.SqlString.Null;
                }
                return new System.Data.SqlTypes.SqlString(this.KatmaiDateTimeString);
            }
        }

        internal string KatmaiDateTimeString
        {
            get
            {
                this.ThrowIfNull();
                if (StorageType.Date == this._type)
                {
                    return this.DateTime.ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);
                }
                if (StorageType.Time == this._type)
                {
                    byte scale = this._value._timeInfo.scale;
                    System.DateTime time2 = new System.DateTime(this._value._timeInfo.ticks);
                    return time2.ToString(__katmaiTimeFormatByScale[scale], DateTimeFormatInfo.InvariantInfo);
                }
                if (StorageType.DateTime2 == this._type)
                {
                    byte index = this._value._dateTime2Info.timeInfo.scale;
                    return this.DateTime.ToString(__katmaiDateTime2FormatByScale[index], DateTimeFormatInfo.InvariantInfo);
                }
                if (StorageType.DateTimeOffset == this._type)
                {
                    System.DateTimeOffset dateTimeOffset = this.DateTimeOffset;
                    byte num = this._value._dateTimeOffsetInfo.dateTime2Info.timeInfo.scale;
                    return dateTimeOffset.ToString(__katmaiDateTimeOffsetFormatByScale[num], DateTimeFormatInfo.InvariantInfo);
                }
                return (string) this.Value;
            }
        }

        internal float Single
        {
            get
            {
                this.ThrowIfNull();
                if (StorageType.Single == this._type)
                {
                    return this._value._single;
                }
                return (float) this.Value;
            }
            set
            {
                this._value._single = value;
                this._type = StorageType.Single;
                this._isNull = false;
            }
        }

        internal System.Data.SqlTypes.SqlBinary SqlBinary
        {
            get
            {
                if (StorageType.SqlBinary == this._type)
                {
                    return (System.Data.SqlTypes.SqlBinary) this._object;
                }
                return (System.Data.SqlTypes.SqlBinary) this.SqlValue;
            }
            set
            {
                this._object = value;
                this._type = StorageType.SqlBinary;
                this._isNull = value.IsNull;
            }
        }

        internal System.Data.SqlTypes.SqlBoolean SqlBoolean
        {
            get
            {
                if (StorageType.Boolean != this._type)
                {
                    return (System.Data.SqlTypes.SqlBoolean) this.SqlValue;
                }
                if (this.IsNull)
                {
                    return System.Data.SqlTypes.SqlBoolean.Null;
                }
                return new System.Data.SqlTypes.SqlBoolean(this._value._boolean);
            }
        }

        internal System.Data.SqlTypes.SqlByte SqlByte
        {
            get
            {
                if (StorageType.Byte != this._type)
                {
                    return (System.Data.SqlTypes.SqlByte) this.SqlValue;
                }
                if (this.IsNull)
                {
                    return System.Data.SqlTypes.SqlByte.Null;
                }
                return new System.Data.SqlTypes.SqlByte(this._value._byte);
            }
        }

        internal System.Data.SqlClient.SqlCachedBuffer SqlCachedBuffer
        {
            get
            {
                if (StorageType.SqlCachedBuffer != this._type)
                {
                    return (System.Data.SqlClient.SqlCachedBuffer) this.SqlValue;
                }
                if (this.IsNull)
                {
                    return System.Data.SqlClient.SqlCachedBuffer.Null;
                }
                return (System.Data.SqlClient.SqlCachedBuffer) this._object;
            }
            set
            {
                this._object = value;
                this._type = StorageType.SqlCachedBuffer;
                this._isNull = value.IsNull;
            }
        }

        internal System.Data.SqlTypes.SqlDateTime SqlDateTime
        {
            get
            {
                if (StorageType.DateTime != this._type)
                {
                    return (System.Data.SqlTypes.SqlDateTime) this.SqlValue;
                }
                if (this.IsNull)
                {
                    return System.Data.SqlTypes.SqlDateTime.Null;
                }
                return new System.Data.SqlTypes.SqlDateTime(this._value._dateTimeInfo.daypart, this._value._dateTimeInfo.timepart);
            }
        }

        internal System.Data.SqlTypes.SqlDecimal SqlDecimal
        {
            get
            {
                if (StorageType.Decimal != this._type)
                {
                    return (System.Data.SqlTypes.SqlDecimal) this.SqlValue;
                }
                if (this.IsNull)
                {
                    return System.Data.SqlTypes.SqlDecimal.Null;
                }
                return new System.Data.SqlTypes.SqlDecimal(this._value._numericInfo.precision, this._value._numericInfo.scale, this._value._numericInfo.positive, this._value._numericInfo.data1, this._value._numericInfo.data2, this._value._numericInfo.data3, this._value._numericInfo.data4);
            }
        }

        internal System.Data.SqlTypes.SqlDouble SqlDouble
        {
            get
            {
                if (StorageType.Double != this._type)
                {
                    return (System.Data.SqlTypes.SqlDouble) this.SqlValue;
                }
                if (this.IsNull)
                {
                    return System.Data.SqlTypes.SqlDouble.Null;
                }
                return new System.Data.SqlTypes.SqlDouble(this._value._double);
            }
        }

        internal System.Data.SqlTypes.SqlGuid SqlGuid
        {
            get
            {
                if (StorageType.SqlGuid == this._type)
                {
                    return (System.Data.SqlTypes.SqlGuid) this._object;
                }
                return (System.Data.SqlTypes.SqlGuid) this.SqlValue;
            }
            set
            {
                this._object = value;
                this._type = StorageType.SqlGuid;
                this._isNull = value.IsNull;
            }
        }

        internal System.Data.SqlTypes.SqlInt16 SqlInt16
        {
            get
            {
                if (StorageType.Int16 != this._type)
                {
                    return (System.Data.SqlTypes.SqlInt16) this.SqlValue;
                }
                if (this.IsNull)
                {
                    return System.Data.SqlTypes.SqlInt16.Null;
                }
                return new System.Data.SqlTypes.SqlInt16(this._value._int16);
            }
        }

        internal System.Data.SqlTypes.SqlInt32 SqlInt32
        {
            get
            {
                if (StorageType.Int32 != this._type)
                {
                    return (System.Data.SqlTypes.SqlInt32) this.SqlValue;
                }
                if (this.IsNull)
                {
                    return System.Data.SqlTypes.SqlInt32.Null;
                }
                return new System.Data.SqlTypes.SqlInt32(this._value._int32);
            }
        }

        internal System.Data.SqlTypes.SqlInt64 SqlInt64
        {
            get
            {
                if (StorageType.Int64 != this._type)
                {
                    return (System.Data.SqlTypes.SqlInt64) this.SqlValue;
                }
                if (this.IsNull)
                {
                    return System.Data.SqlTypes.SqlInt64.Null;
                }
                return new System.Data.SqlTypes.SqlInt64(this._value._int64);
            }
        }

        internal System.Data.SqlTypes.SqlMoney SqlMoney
        {
            get
            {
                if (StorageType.Money != this._type)
                {
                    return (System.Data.SqlTypes.SqlMoney) this.SqlValue;
                }
                if (this.IsNull)
                {
                    return System.Data.SqlTypes.SqlMoney.Null;
                }
                return new System.Data.SqlTypes.SqlMoney(this._value._int64, 1);
            }
        }

        internal System.Data.SqlTypes.SqlSingle SqlSingle
        {
            get
            {
                if (StorageType.Single != this._type)
                {
                    return (System.Data.SqlTypes.SqlSingle) this.SqlValue;
                }
                if (this.IsNull)
                {
                    return System.Data.SqlTypes.SqlSingle.Null;
                }
                return new System.Data.SqlTypes.SqlSingle(this._value._single);
            }
        }

        internal System.Data.SqlTypes.SqlString SqlString
        {
            get
            {
                if (StorageType.String == this._type)
                {
                    if (this.IsNull)
                    {
                        return System.Data.SqlTypes.SqlString.Null;
                    }
                    return new System.Data.SqlTypes.SqlString((string) this._object);
                }
                if (StorageType.SqlCachedBuffer != this._type)
                {
                    return (System.Data.SqlTypes.SqlString) this.SqlValue;
                }
                System.Data.SqlClient.SqlCachedBuffer buffer = (System.Data.SqlClient.SqlCachedBuffer) this._object;
                if (buffer.IsNull)
                {
                    return System.Data.SqlTypes.SqlString.Null;
                }
                return buffer.ToSqlString();
            }
        }

        internal object SqlValue
        {
            get
            {
                switch (this._type)
                {
                    case StorageType.Empty:
                        return DBNull.Value;

                    case StorageType.Boolean:
                        return this.SqlBoolean;

                    case StorageType.Byte:
                        return this.SqlByte;

                    case StorageType.DateTime:
                        return this.SqlDateTime;

                    case StorageType.Decimal:
                        return this.SqlDecimal;

                    case StorageType.Double:
                        return this.SqlDouble;

                    case StorageType.Int16:
                        return this.SqlInt16;

                    case StorageType.Int32:
                        return this.SqlInt32;

                    case StorageType.Int64:
                        return this.SqlInt64;

                    case StorageType.Money:
                        return this.SqlMoney;

                    case StorageType.Single:
                        return this.SqlSingle;

                    case StorageType.String:
                        return this.SqlString;

                    case StorageType.SqlBinary:
                    case StorageType.SqlGuid:
                        return this._object;

                    case StorageType.SqlCachedBuffer:
                    {
                        System.Data.SqlClient.SqlCachedBuffer buffer = (System.Data.SqlClient.SqlCachedBuffer) this._object;
                        if (!buffer.IsNull)
                        {
                            return buffer.ToSqlXml();
                        }
                        return System.Data.SqlTypes.SqlXml.Null;
                    }
                    case StorageType.SqlXml:
                        if (!this._isNull)
                        {
                            return (System.Data.SqlTypes.SqlXml) this._object;
                        }
                        return System.Data.SqlTypes.SqlXml.Null;

                    case StorageType.Date:
                    case StorageType.DateTime2:
                        if (!this._isNull)
                        {
                            return this.DateTime;
                        }
                        return DBNull.Value;

                    case StorageType.DateTimeOffset:
                        if (!this._isNull)
                        {
                            return this.DateTimeOffset;
                        }
                        return DBNull.Value;

                    case StorageType.Time:
                        if (!this._isNull)
                        {
                            return this.Time;
                        }
                        return DBNull.Value;
                }
                return null;
            }
        }

        internal System.Data.SqlTypes.SqlXml SqlXml
        {
            get
            {
                if (StorageType.SqlXml != this._type)
                {
                    return (System.Data.SqlTypes.SqlXml) this.SqlValue;
                }
                if (this.IsNull)
                {
                    return System.Data.SqlTypes.SqlXml.Null;
                }
                return (System.Data.SqlTypes.SqlXml) this._object;
            }
            set
            {
                this._object = value;
                this._type = StorageType.SqlXml;
                this._isNull = value.IsNull;
            }
        }

        internal string String
        {
            get
            {
                this.ThrowIfNull();
                if (StorageType.String == this._type)
                {
                    return (string) this._object;
                }
                if (StorageType.SqlCachedBuffer == this._type)
                {
                    return ((System.Data.SqlClient.SqlCachedBuffer) this._object).ToString();
                }
                return (string) this.Value;
            }
        }

        internal TimeSpan Time
        {
            get
            {
                this.ThrowIfNull();
                if (StorageType.Time == this._type)
                {
                    return new TimeSpan(this._value._timeInfo.ticks);
                }
                return (TimeSpan) this.Value;
            }
        }

        internal object Value
        {
            get
            {
                if (this.IsNull)
                {
                    return DBNull.Value;
                }
                switch (this._type)
                {
                    case StorageType.Empty:
                        return DBNull.Value;

                    case StorageType.Boolean:
                        return this.Boolean;

                    case StorageType.Byte:
                        return this.Byte;

                    case StorageType.DateTime:
                        return this.DateTime;

                    case StorageType.Decimal:
                        return this.Decimal;

                    case StorageType.Double:
                        return this.Double;

                    case StorageType.Int16:
                        return this.Int16;

                    case StorageType.Int32:
                        return this.Int32;

                    case StorageType.Int64:
                        return this.Int64;

                    case StorageType.Money:
                        return this.Decimal;

                    case StorageType.Single:
                        return this.Single;

                    case StorageType.String:
                        return this.String;

                    case StorageType.SqlBinary:
                        return this.ByteArray;

                    case StorageType.SqlCachedBuffer:
                        return ((System.Data.SqlClient.SqlCachedBuffer) this._object).ToString();

                    case StorageType.SqlGuid:
                        return this.Guid;

                    case StorageType.SqlXml:
                    {
                        System.Data.SqlTypes.SqlXml xml = (System.Data.SqlTypes.SqlXml) this._object;
                        return xml.Value;
                    }
                    case StorageType.Date:
                        return this.DateTime;

                    case StorageType.DateTime2:
                        return this.DateTime;

                    case StorageType.DateTimeOffset:
                        return this.DateTimeOffset;

                    case StorageType.Time:
                        return this.Time;
                }
                return null;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct DateTime2Info
        {
            internal int date;
            internal SqlBuffer.TimeInfo timeInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct DateTimeInfo
        {
            internal int daypart;
            internal int timepart;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct DateTimeOffsetInfo
        {
            internal SqlBuffer.DateTime2Info dateTime2Info;
            internal short offset;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NumericInfo
        {
            internal int data1;
            internal int data2;
            internal int data3;
            internal int data4;
            internal byte precision;
            internal byte scale;
            internal bool positive;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct Storage
        {
            [FieldOffset(0)]
            internal bool _boolean;
            [FieldOffset(0)]
            internal byte _byte;
            [FieldOffset(0)]
            internal SqlBuffer.DateTime2Info _dateTime2Info;
            [FieldOffset(0)]
            internal SqlBuffer.DateTimeInfo _dateTimeInfo;
            [FieldOffset(0)]
            internal SqlBuffer.DateTimeOffsetInfo _dateTimeOffsetInfo;
            [FieldOffset(0)]
            internal double _double;
            [FieldOffset(0)]
            internal short _int16;
            [FieldOffset(0)]
            internal int _int32;
            [FieldOffset(0)]
            internal long _int64;
            [FieldOffset(0)]
            internal SqlBuffer.NumericInfo _numericInfo;
            [FieldOffset(0)]
            internal float _single;
            [FieldOffset(0)]
            internal SqlBuffer.TimeInfo _timeInfo;
        }

        internal enum StorageType
        {
            Empty,
            Boolean,
            Byte,
            DateTime,
            Decimal,
            Double,
            Int16,
            Int32,
            Int64,
            Money,
            Single,
            String,
            SqlBinary,
            SqlCachedBuffer,
            SqlGuid,
            SqlXml,
            Date,
            DateTime2,
            DateTimeOffset,
            Time
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct TimeInfo
        {
            internal long ticks;
            internal byte scale;
        }
    }
}

