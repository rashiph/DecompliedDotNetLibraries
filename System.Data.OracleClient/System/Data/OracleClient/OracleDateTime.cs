namespace System.Data.OracleClient
{
    using System;
    using System.Data.Common;
    using System.Data.SqlTypes;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct OracleDateTime : IComparable, INullable
    {
        private const int MaxOracleFSecPrecision = 9;
        private const byte x_DATE_Length = 7;
        private const byte x_TIMESTAMP_Length = 11;
        private const byte x_TIMESTAMP_WITH_TIMEZONE_Length = 13;
        private const int FractionalSecondsPerTick = 100;
        private byte[] _value;
        public static readonly OracleDateTime MaxValue;
        public static readonly OracleDateTime MinValue;
        public static readonly OracleDateTime Null;
        private OracleDateTime(bool isNull)
        {
            this._value = null;
        }

        public OracleDateTime(DateTime dt)
        {
            this._value = new byte[11];
            Pack(this._value, dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, ((int) (dt.Ticks % 0x989680L)) * 100);
        }

        public OracleDateTime(long ticks)
        {
            this._value = new byte[11];
            DateTime time = new DateTime(ticks);
            Pack(this._value, time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second, ((int) (time.Ticks % 0x989680L)) * 100);
        }

        public OracleDateTime(int year, int month, int day) : this(year, month, day, 0, 0, 0, 0)
        {
        }

        public OracleDateTime(int year, int month, int day, Calendar calendar) : this(year, month, day, 0, 0, 0, 0, calendar)
        {
        }

        public OracleDateTime(int year, int month, int day, int hour, int minute, int second) : this(year, month, day, hour, minute, second, 0)
        {
        }

        public OracleDateTime(int year, int month, int day, int hour, int minute, int second, Calendar calendar) : this(year, month, day, hour, minute, second, 0, calendar)
        {
        }

        public OracleDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond)
        {
            this._value = new byte[11];
            new DateTime((year < 0) ? 0 : year, month, (year < 0) ? 1 : day, hour, minute, second, millisecond);
            Pack(this._value, year, month, day, hour, minute, second, ((int) (millisecond * 0x2710L)) * 100);
        }

        public OracleDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, Calendar calendar)
        {
            this._value = new byte[11];
            DateTime time = new DateTime(year, month, day, hour, minute, second, millisecond, calendar);
            Pack(this._value, time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second, ((int) (time.Ticks % 0x989680L)) * 100);
        }

        public OracleDateTime(OracleDateTime from)
        {
            this._value = new byte[from._value.Length];
            from._value.CopyTo(this._value, 0);
        }

        internal OracleDateTime(NativeBuffer buffer, int valueOffset, int lengthOffset, MetaType metaType, OracleConnection connection)
        {
            this._value = GetBytesFromBuffer(buffer, valueOffset, lengthOffset, metaType, connection);
        }

        internal OracleDateTime(OciDateTimeDescriptor dateTimeDescriptor, MetaType metaType, OracleConnection connection)
        {
            this._value = GetBytesFromDescriptor(dateTimeDescriptor, metaType, connection);
        }

        private static void Pack(byte[] dateval, int year, int month, int day, int hour, int minute, int second, int fsecs)
        {
            dateval[0] = (byte) ((year / 100) + 100);
            dateval[1] = (byte) ((year % 100) + 100);
            dateval[2] = (byte) month;
            dateval[3] = (byte) day;
            dateval[4] = (byte) (hour + 1);
            dateval[5] = (byte) (minute + 1);
            dateval[6] = (byte) (second + 1);
            dateval[7] = (byte) (fsecs >> 0x18);
            dateval[8] = (byte) ((fsecs >> 0x10) & 0xff);
            dateval[9] = (byte) ((fsecs >> 8) & 0xff);
            dateval[10] = (byte) (fsecs & 0xff);
        }

        private static int Unpack(byte[] dateval, out int year, out int month, out int day, out int hour, out int minute, out int second, out int fsec)
        {
            int num;
            int num2;
            year = ((dateval[0] - 100) * 100) + (dateval[1] - 100);
            month = dateval[2];
            day = dateval[3];
            hour = dateval[4] - 1;
            minute = dateval[5] - 1;
            second = dateval[6] - 1;
            if (7 == dateval.Length)
            {
                fsec = num = num2 = 0;
            }
            else
            {
                fsec = (((dateval[7] << 0x18) | (dateval[8] << 0x10)) | (dateval[9] << 8)) | dateval[10];
                if (11 == dateval.Length)
                {
                    num = num2 = 0;
                }
                else
                {
                    num = dateval[11] - 20;
                    num2 = dateval[12] - 60;
                }
            }
            if (13 == dateval.Length)
            {
                DateTime time = new DateTime(year, month, day, hour, minute, second) + new TimeSpan(num, num2, 0);
                year = time.Year;
                month = time.Month;
                day = time.Day;
                hour = time.Hour;
                minute = time.Minute;
            }
            return dateval.Length;
        }

        public bool IsNull
        {
            get
            {
                return (null == this._value);
            }
        }
        public DateTime Value
        {
            get
            {
                if (this.IsNull)
                {
                    throw System.Data.Common.ADP.DataIsNull();
                }
                return ToDateTime(this._value);
            }
        }
        public int Year
        {
            get
            {
                int num;
                int num2;
                int num3;
                int num4;
                int num5;
                int num6;
                int num7;
                if (this.IsNull)
                {
                    throw System.Data.Common.ADP.DataIsNull();
                }
                Unpack(this._value, out num, out num7, out num6, out num5, out num4, out num3, out num2);
                return num;
            }
        }
        public int Month
        {
            get
            {
                int num;
                int num2;
                int num3;
                int num4;
                int num5;
                int num6;
                int num7;
                if (this.IsNull)
                {
                    throw System.Data.Common.ADP.DataIsNull();
                }
                Unpack(this._value, out num7, out num, out num6, out num5, out num4, out num3, out num2);
                return num;
            }
        }
        public int Day
        {
            get
            {
                int num;
                int num2;
                int num3;
                int num4;
                int num5;
                int num6;
                int num7;
                if (this.IsNull)
                {
                    throw System.Data.Common.ADP.DataIsNull();
                }
                Unpack(this._value, out num7, out num6, out num, out num5, out num4, out num3, out num2);
                return num;
            }
        }
        public int Hour
        {
            get
            {
                int num;
                int num2;
                int num3;
                int num4;
                int num5;
                int num6;
                int num7;
                if (this.IsNull)
                {
                    throw System.Data.Common.ADP.DataIsNull();
                }
                Unpack(this._value, out num7, out num6, out num5, out num, out num4, out num3, out num2);
                return num;
            }
        }
        public int Minute
        {
            get
            {
                int num;
                int num2;
                int num3;
                int num4;
                int num5;
                int num6;
                int num7;
                if (this.IsNull)
                {
                    throw System.Data.Common.ADP.DataIsNull();
                }
                Unpack(this._value, out num7, out num6, out num5, out num4, out num, out num3, out num2);
                return num;
            }
        }
        public int Second
        {
            get
            {
                int num;
                int num2;
                int num3;
                int num4;
                int num5;
                int num6;
                int num7;
                if (this.IsNull)
                {
                    throw System.Data.Common.ADP.DataIsNull();
                }
                Unpack(this._value, out num7, out num6, out num5, out num4, out num3, out num, out num2);
                return num;
            }
        }
        public int Millisecond
        {
            get
            {
                int num2;
                int num3;
                int num4;
                int num5;
                int num6;
                int num7;
                int num8;
                if (this.IsNull)
                {
                    throw System.Data.Common.ADP.DataIsNull();
                }
                Unpack(this._value, out num8, out num7, out num6, out num5, out num4, out num3, out num2);
                return (int) (((long) (num2 / 100)) / 0x2710L);
            }
        }
        public int CompareTo(object obj)
        {
            int num2;
            int num3;
            int num4;
            int num5;
            int num6;
            int num7;
            int num8;
            int num9;
            int num10;
            int num11;
            int num12;
            int num13;
            int num14;
            int num15;
            if (!(obj.GetType() == typeof(OracleDateTime)))
            {
                throw System.Data.Common.ADP.WrongType(obj.GetType(), typeof(OracleDateTime));
            }
            OracleDateTime time = (OracleDateTime) obj;
            if (this.IsNull)
            {
                if (!time.IsNull)
                {
                    return -1;
                }
                return 0;
            }
            if (time.IsNull)
            {
                return 1;
            }
            Unpack(this._value, out num15, out num14, out num13, out num12, out num11, out num10, out num9);
            Unpack(time._value, out num8, out num7, out num6, out num5, out num4, out num3, out num2);
            int num = num15 - num8;
            if (num != 0)
            {
                return num;
            }
            num = num14 - num7;
            if (num != 0)
            {
                return num;
            }
            num = num13 - num6;
            if (num != 0)
            {
                return num;
            }
            num = num12 - num5;
            if (num != 0)
            {
                return num;
            }
            num = num11 - num4;
            if (num != 0)
            {
                return num;
            }
            num = num10 - num3;
            if (num != 0)
            {
                return num;
            }
            num = num9 - num2;
            if (num != 0)
            {
                return num;
            }
            return 0;
        }

        public override bool Equals(object value)
        {
            if (value is OracleDateTime)
            {
                OracleBoolean flag = this == ((OracleDateTime) value);
                return flag.Value;
            }
            return false;
        }

        internal static byte[] GetBytesFromDescriptor(OciDateTimeDescriptor dateTimeDescriptor, MetaType metaType, OracleConnection connection)
        {
            uint num2;
            OCI.DATATYPE ociType = metaType.OciType;
            OCI.DATATYPE datatype2 = ociType;
            if (datatype2 == OCI.DATATYPE.INT_TIMESTAMP)
            {
                num2 = 11;
            }
            else if (datatype2 == OCI.DATATYPE.INT_TIMESTAMP_LTZ)
            {
                num2 = 13;
            }
            else
            {
                num2 = 13;
            }
            byte[] outarray = new byte[num2];
            uint len = num2;
            OciIntervalDescriptor reftz = new OciIntervalDescriptor(connection.EnvironmentHandle);
            int rc = System.Data.Common.UnsafeNativeMethods.OCIDateTimeToArray(connection.EnvironmentHandle, connection.ErrorHandle, dateTimeDescriptor, reftz, outarray, ref len, 9);
            if (rc != 0)
            {
                connection.CheckError(connection.ErrorHandle, rc);
            }
            if (OCI.DATATYPE.INT_TIMESTAMP_LTZ == ociType)
            {
                TimeSpan serverTimeZoneAdjustmentToUTC = connection.ServerTimeZoneAdjustmentToUTC;
                outarray[11] = (byte) (serverTimeZoneAdjustmentToUTC.Hours + 20);
                outarray[12] = (byte) (serverTimeZoneAdjustmentToUTC.Minutes + 60);
                return outarray;
            }
            if (OCI.DATATYPE.INT_TIMESTAMP_TZ == ociType)
            {
                sbyte num3;
                sbyte num4;
                rc = System.Data.Common.UnsafeNativeMethods.OCIDateTimeGetTimeZoneOffset(connection.EnvironmentHandle, connection.ErrorHandle, dateTimeDescriptor, out num4, out num3);
                if (rc != 0)
                {
                    connection.CheckError(connection.ErrorHandle, rc);
                }
                outarray[11] = (byte) (num4 + 20);
                outarray[12] = (byte) (num3 + 60);
            }
            return outarray;
        }

        internal static byte[] GetBytesFromBuffer(NativeBuffer buffer, int valueOffset, int lengthOffset, MetaType metaType, OracleConnection connection)
        {
            uint num2;
            OCI.DATATYPE ociType = metaType.OciType;
            short length = buffer.ReadInt16(lengthOffset);
            OCI.DATATYPE datatype = ociType;
            if (datatype == OCI.DATATYPE.DATE)
            {
                num2 = 7;
            }
            else if (datatype == OCI.DATATYPE.INT_TIMESTAMP)
            {
                num2 = 11;
            }
            else if (datatype == OCI.DATATYPE.INT_TIMESTAMP_LTZ)
            {
                num2 = 13;
            }
            else
            {
                num2 = 13;
            }
            byte[] destination = new byte[num2];
            buffer.ReadBytes(valueOffset, destination, 0, length);
            if (OCI.DATATYPE.INT_TIMESTAMP_LTZ == ociType)
            {
                TimeSpan serverTimeZoneAdjustmentToUTC = connection.ServerTimeZoneAdjustmentToUTC;
                destination[11] = (byte) (serverTimeZoneAdjustmentToUTC.Hours + 20);
                destination[12] = (byte) (serverTimeZoneAdjustmentToUTC.Minutes + 60);
                return destination;
            }
            if ((OCI.DATATYPE.INT_TIMESTAMP_TZ == ociType) && (0x80 < destination[11]))
            {
                sbyte num3;
                sbyte num4;
                OciIntervalDescriptor reftz = new OciIntervalDescriptor(connection.EnvironmentHandle);
                OciDateTimeDescriptor datetime = new OciDateTimeDescriptor(connection.EnvironmentHandle, OCI.HTYPE.OCI_DTYPE_TIMESTAMP_TZ);
                int rc = System.Data.Common.UnsafeNativeMethods.OCIDateTimeFromArray(connection.EnvironmentHandle, connection.ErrorHandle, destination, num2, 0xbc, datetime, reftz, 0);
                if (rc != 0)
                {
                    connection.CheckError(connection.ErrorHandle, rc);
                }
                rc = System.Data.Common.UnsafeNativeMethods.OCIDateTimeGetTimeZoneOffset(connection.EnvironmentHandle, connection.ErrorHandle, datetime, out num4, out num3);
                if (rc != 0)
                {
                    connection.CheckError(connection.ErrorHandle, rc);
                }
                destination[11] = (byte) (num4 + 20);
                destination[12] = (byte) (num3 + 60);
            }
            return destination;
        }

        public override int GetHashCode()
        {
            return (this.IsNull ? 0 : this._value.GetHashCode());
        }

        internal static DateTime MarshalToDateTime(NativeBuffer buffer, int valueOffset, int lengthOffset, MetaType metaType, OracleConnection connection)
        {
            return ToDateTime(GetBytesFromBuffer(buffer, valueOffset, lengthOffset, metaType, connection));
        }

        internal static int MarshalDateToNative(object value, NativeBuffer buffer, int offset, OCI.DATATYPE ociType, OracleConnection connection)
        {
            byte[] buffer2;
            if (value is OracleDateTime)
            {
                buffer2 = ((OracleDateTime) value)._value;
            }
            else
            {
                DateTime time = (DateTime) value;
                buffer2 = new byte[11];
                Pack(buffer2, time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second, 0);
            }
            int length = 7;
            buffer.WriteBytes(offset, buffer2, 0, length);
            return length;
        }

        internal static DateTime MarshalTimestampToDateTime(OciDateTimeDescriptor dateTimeDescriptor, MetaType metaType, OracleConnection connection)
        {
            return ToDateTime(GetBytesFromDescriptor(dateTimeDescriptor, metaType, connection));
        }

        internal static OciDateTimeDescriptor CreateEmptyDescriptor(OCI.DATATYPE ociType, OracleConnection connection)
        {
            OCI.HTYPE htype;
            switch (ociType)
            {
                case OCI.DATATYPE.INT_TIMESTAMP:
                    htype = OCI.HTYPE.OCI_DTYPE_TIMESTAMP;
                    break;

                case OCI.DATATYPE.INT_TIMESTAMP_TZ:
                    htype = OCI.HTYPE.OCI_DTYPE_TIMESTAMP_TZ;
                    break;

                default:
                    htype = OCI.HTYPE.OCI_DTYPE_TIMESTAMP_LTZ;
                    break;
            }
            return new OciDateTimeDescriptor(connection.EnvironmentHandle, htype);
        }

        internal static OciDateTimeDescriptor CreateDescriptor(OCI.DATATYPE ociType, OracleConnection connection, object value)
        {
            byte[] buffer;
            OCI.DATATYPE tIMESTAMP;
            if (value is OracleDateTime)
            {
                buffer = ((OracleDateTime) value)._value;
            }
            else
            {
                DateTime dt = (DateTime) value;
                OracleDateTime time2 = new OracleDateTime(dt);
                buffer = time2._value;
            }
            switch (ociType)
            {
                case OCI.DATATYPE.INT_TIMESTAMP:
                    tIMESTAMP = OCI.DATATYPE.TIMESTAMP;
                    break;

                case OCI.DATATYPE.INT_TIMESTAMP_LTZ:
                    tIMESTAMP = OCI.DATATYPE.TIMESTAMP_LTZ;
                    break;

                default:
                {
                    tIMESTAMP = OCI.DATATYPE.TIMESTAMP_TZ;
                    TimeSpan serverTimeZoneAdjustmentToUTC = connection.ServerTimeZoneAdjustmentToUTC;
                    if (buffer.Length < 13)
                    {
                        byte[] dst = new byte[13];
                        Buffer.BlockCopy(buffer, 0, dst, 0, buffer.Length);
                        buffer = dst;
                        buffer[11] = (byte) (20 + serverTimeZoneAdjustmentToUTC.Hours);
                        buffer[12] = (byte) (60 + serverTimeZoneAdjustmentToUTC.Minutes);
                    }
                    break;
                }
            }
            OciDateTimeDescriptor datetime = CreateEmptyDescriptor(ociType, connection);
            OciIntervalDescriptor reftz = new OciIntervalDescriptor(connection.EnvironmentHandle);
            int rc = System.Data.Common.UnsafeNativeMethods.OCIDateTimeFromArray(connection.EnvironmentHandle, connection.ErrorHandle, buffer, (uint) buffer.Length, (byte) tIMESTAMP, datetime, reftz, 9);
            if (rc != 0)
            {
                connection.CheckError(connection.ErrorHandle, rc);
            }
            return datetime;
        }

        internal bool HasTimeZoneInfo
        {
            get
            {
                return ((this._value != null) && (this._value.Length >= 13));
            }
        }
        internal bool HasTimeInfo
        {
            get
            {
                return ((this._value != null) && (this._value.Length >= 11));
            }
        }
        public static OracleDateTime Parse(string s)
        {
            return new OracleDateTime(DateTime.Parse(s, null));
        }

        private static DateTime ToDateTime(byte[] rawValue)
        {
            int num;
            int num3;
            int num4;
            int num5;
            int num6;
            int num7;
            int num8;
            int num2 = Unpack(rawValue, out num8, out num7, out num6, out num5, out num4, out num3, out num);
            DateTime time = new DateTime(num8, num7, num6, num5, num4, num3);
            if ((num2 > 7) && (num > 100))
            {
                time = time.AddTicks(((long) num) / 100L);
            }
            return time;
        }

        public override string ToString()
        {
            if (this.IsNull)
            {
                return System.Data.Common.ADP.NullString;
            }
            return this.Value.ToString((IFormatProvider) null);
        }

        public static OracleBoolean Equals(OracleDateTime x, OracleDateTime y)
        {
            return (x == y);
        }

        public static OracleBoolean GreaterThan(OracleDateTime x, OracleDateTime y)
        {
            return (x > y);
        }

        public static OracleBoolean GreaterThanOrEqual(OracleDateTime x, OracleDateTime y)
        {
            return (x >= y);
        }

        public static OracleBoolean LessThan(OracleDateTime x, OracleDateTime y)
        {
            return (x < y);
        }

        public static OracleBoolean LessThanOrEqual(OracleDateTime x, OracleDateTime y)
        {
            return (x <= y);
        }

        public static OracleBoolean NotEquals(OracleDateTime x, OracleDateTime y)
        {
            return (x != y);
        }

        public static explicit operator DateTime(OracleDateTime x)
        {
            if (x.IsNull)
            {
                throw System.Data.Common.ADP.DataIsNull();
            }
            return x.Value;
        }

        public static explicit operator OracleDateTime(string x)
        {
            return Parse(x);
        }

        public static OracleBoolean operator ==(OracleDateTime x, OracleDateTime y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) == 0);
            }
            return OracleBoolean.Null;
        }

        public static OracleBoolean operator >(OracleDateTime x, OracleDateTime y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) > 0);
            }
            return OracleBoolean.Null;
        }

        public static OracleBoolean operator >=(OracleDateTime x, OracleDateTime y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) >= 0);
            }
            return OracleBoolean.Null;
        }

        public static OracleBoolean operator <(OracleDateTime x, OracleDateTime y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) < 0);
            }
            return OracleBoolean.Null;
        }

        public static OracleBoolean operator <=(OracleDateTime x, OracleDateTime y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) <= 0);
            }
            return OracleBoolean.Null;
        }

        public static OracleBoolean operator !=(OracleDateTime x, OracleDateTime y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new OracleBoolean(x.CompareTo(y) != 0);
            }
            return OracleBoolean.Null;
        }

        static OracleDateTime()
        {
            MaxValue = new OracleDateTime(DateTime.MaxValue);
            MinValue = new OracleDateTime(DateTime.MinValue);
            Null = new OracleDateTime(true);
        }
    }
}

