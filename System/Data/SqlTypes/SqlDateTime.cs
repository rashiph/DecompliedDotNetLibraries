namespace System.Data.SqlTypes
{
    using System;
    using System.Data.Common;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [Serializable, StructLayout(LayoutKind.Sequential), XmlSchemaProvider("GetXsdType")]
    public struct SqlDateTime : INullable, IComparable, IXmlSerializable
    {
        private const DateTimeStyles x_DateTimeStyle = DateTimeStyles.AllowWhiteSpaces;
        private bool m_fNotNull;
        private int m_day;
        private int m_time;
        private static readonly double SQLTicksPerMillisecond;
        public static readonly int SQLTicksPerSecond;
        public static readonly int SQLTicksPerMinute;
        public static readonly int SQLTicksPerHour;
        private static readonly int SQLTicksPerDay;
        private static readonly long TicksPerSecond;
        private static readonly DateTime SQLBaseDate;
        private static readonly long SQLBaseDateTicks;
        private static readonly int MinYear;
        private static readonly int MaxYear;
        private static readonly int MinDay;
        private static readonly int MaxDay;
        private static readonly int MinTime;
        private static readonly int MaxTime;
        private static readonly int DayBase;
        private static readonly int[] DaysToMonth365;
        private static readonly int[] DaysToMonth366;
        private static readonly DateTime MinDateTime;
        private static readonly DateTime MaxDateTime;
        private static readonly TimeSpan MinTimeSpan;
        private static readonly TimeSpan MaxTimeSpan;
        private static readonly string x_ISO8601_DateTimeFormat;
        private static readonly string[] x_DateTimeFormats;
        public static readonly SqlDateTime MinValue;
        public static readonly SqlDateTime MaxValue;
        public static readonly SqlDateTime Null;
        private SqlDateTime(bool fNull)
        {
            this.m_fNotNull = false;
            this.m_day = 0;
            this.m_time = 0;
        }

        public SqlDateTime(DateTime value)
        {
            this = FromDateTime(value);
        }

        public SqlDateTime(int year, int month, int day) : this(year, month, day, 0, 0, 0, (double) 0.0)
        {
        }

        public SqlDateTime(int year, int month, int day, int hour, int minute, int second) : this(year, month, day, hour, minute, second, (double) 0.0)
        {
        }

        public SqlDateTime(int year, int month, int day, int hour, int minute, int second, double millisecond)
        {
            if (((year >= MinYear) && (year <= MaxYear)) && ((month >= 1) && (month <= 12)))
            {
                int[] numArray = IsLeapYear(year) ? DaysToMonth366 : DaysToMonth365;
                if ((day >= 1) && (day <= (numArray[month] - numArray[month - 1])))
                {
                    int num2 = year - 1;
                    int dayTicks = ((((((num2 * 0x16d) + (num2 / 4)) - (num2 / 100)) + (num2 / 400)) + numArray[month - 1]) + day) - 1;
                    dayTicks -= DayBase;
                    if (((((dayTicks >= MinDay) && (dayTicks <= MaxDay)) && ((hour >= 0) && (hour < 0x18))) && (((minute >= 0) && (minute < 60)) && ((second >= 0) && (second < 60)))) && ((millisecond >= 0.0) && (millisecond < 1000.0)))
                    {
                        double num4 = (millisecond * SQLTicksPerMillisecond) + 0.5;
                        int timeTicks = (((hour * SQLTicksPerHour) + (minute * SQLTicksPerMinute)) + (second * SQLTicksPerSecond)) + ((int) num4);
                        if (timeTicks > MaxTime)
                        {
                            timeTicks = 0;
                            dayTicks++;
                        }
                        this = new SqlDateTime(dayTicks, timeTicks);
                        return;
                    }
                }
            }
            throw new SqlTypeException(SQLResource.InvalidDateTimeMessage);
        }

        public SqlDateTime(int year, int month, int day, int hour, int minute, int second, int bilisecond) : this(year, month, day, hour, minute, second, (double) (((double) bilisecond) / 1000.0))
        {
        }

        public SqlDateTime(int dayTicks, int timeTicks)
        {
            if (((dayTicks < MinDay) || (dayTicks > MaxDay)) || ((timeTicks < MinTime) || (timeTicks > MaxTime)))
            {
                this.m_fNotNull = false;
                throw new OverflowException(SQLResource.DateTimeOverflowMessage);
            }
            this.m_day = dayTicks;
            this.m_time = timeTicks;
            this.m_fNotNull = true;
        }

        internal SqlDateTime(double dblVal)
        {
            if ((dblVal < MinDay) || (dblVal >= (MaxDay + 1)))
            {
                throw new OverflowException(SQLResource.DateTimeOverflowMessage);
            }
            int dayTicks = (int) dblVal;
            int timeTicks = (int) ((dblVal - dayTicks) * SQLTicksPerDay);
            if (timeTicks < 0)
            {
                dayTicks--;
                timeTicks += SQLTicksPerDay;
            }
            else if (timeTicks >= SQLTicksPerDay)
            {
                dayTicks++;
                timeTicks -= SQLTicksPerDay;
            }
            this = new SqlDateTime(dayTicks, timeTicks);
        }

        public bool IsNull
        {
            get
            {
                return !this.m_fNotNull;
            }
        }
        private static TimeSpan ToTimeSpan(SqlDateTime value)
        {
            long num = (long) ((((double) value.m_time) / SQLTicksPerMillisecond) + 0.5);
            return new TimeSpan((value.m_day * 0xc92a69c000L) + (num * 0x2710L));
        }

        private static DateTime ToDateTime(SqlDateTime value)
        {
            return SQLBaseDate.Add(ToTimeSpan(value));
        }

        internal static DateTime ToDateTime(int daypart, int timepart)
        {
            if (((daypart < MinDay) || (daypart > MaxDay)) || ((timepart < MinTime) || (timepart > MaxTime)))
            {
                throw new OverflowException(SQLResource.DateTimeOverflowMessage);
            }
            long num2 = daypart * 0xc92a69c000L;
            long num = ((long) ((((double) timepart) / SQLTicksPerMillisecond) + 0.5)) * 0x2710L;
            return new DateTime((SQLBaseDateTicks + num2) + num);
        }

        private static SqlDateTime FromTimeSpan(TimeSpan value)
        {
            if ((value < MinTimeSpan) || (value > MaxTimeSpan))
            {
                throw new SqlTypeException(SQLResource.DateTimeOverflowMessage);
            }
            int days = value.Days;
            long num2 = value.Ticks - (days * 0xc92a69c000L);
            if (num2 < 0L)
            {
                days--;
                num2 += 0xc92a69c000L;
            }
            int timeTicks = (int) (((((double) num2) / 10000.0) * SQLTicksPerMillisecond) + 0.5);
            if (timeTicks > MaxTime)
            {
                timeTicks = 0;
                days++;
            }
            return new SqlDateTime(days, timeTicks);
        }

        private static SqlDateTime FromDateTime(DateTime value)
        {
            if (value == DateTime.MaxValue)
            {
                return MaxValue;
            }
            return FromTimeSpan(value.Subtract(SQLBaseDate));
        }

        public DateTime Value
        {
            get
            {
                if (!this.m_fNotNull)
                {
                    throw new SqlNullValueException();
                }
                return ToDateTime(this);
            }
        }
        public int DayTicks
        {
            get
            {
                if (!this.m_fNotNull)
                {
                    throw new SqlNullValueException();
                }
                return this.m_day;
            }
        }
        public int TimeTicks
        {
            get
            {
                if (!this.m_fNotNull)
                {
                    throw new SqlNullValueException();
                }
                return this.m_time;
            }
        }
        public static implicit operator SqlDateTime(DateTime value)
        {
            return new SqlDateTime(value);
        }

        public static explicit operator DateTime(SqlDateTime x)
        {
            return ToDateTime(x);
        }

        public override string ToString()
        {
            if (this.IsNull)
            {
                return SQLResource.NullString;
            }
            return ToDateTime(this).ToString((IFormatProvider) null);
        }

        public static SqlDateTime Parse(string s)
        {
            DateTime time;
            if (s == SQLResource.NullString)
            {
                return Null;
            }
            try
            {
                time = DateTime.Parse(s, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                DateTimeFormatInfo format = (DateTimeFormatInfo) Thread.CurrentThread.CurrentCulture.GetFormat(typeof(DateTimeFormatInfo));
                time = DateTime.ParseExact(s, x_DateTimeFormats, format, DateTimeStyles.AllowWhiteSpaces);
            }
            return new SqlDateTime(time);
        }

        public static SqlDateTime operator +(SqlDateTime x, TimeSpan t)
        {
            if (!x.IsNull)
            {
                return FromDateTime(ToDateTime(x) + t);
            }
            return Null;
        }

        public static SqlDateTime operator -(SqlDateTime x, TimeSpan t)
        {
            if (!x.IsNull)
            {
                return FromDateTime(ToDateTime(x) - t);
            }
            return Null;
        }

        public static SqlDateTime Add(SqlDateTime x, TimeSpan t)
        {
            return (x + t);
        }

        public static SqlDateTime Subtract(SqlDateTime x, TimeSpan t)
        {
            return (x - t);
        }

        public static explicit operator SqlDateTime(SqlString x)
        {
            if (!x.IsNull)
            {
                return Parse(x.Value);
            }
            return Null;
        }

        private static bool IsLeapYear(int year)
        {
            if ((year % 4) != 0)
            {
                return false;
            }
            if ((year % 100) == 0)
            {
                return ((year % 400) == 0);
            }
            return true;
        }

        public static SqlBoolean operator ==(SqlDateTime x, SqlDateTime y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean((x.m_day == y.m_day) && (x.m_time == y.m_time));
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator !=(SqlDateTime x, SqlDateTime y)
        {
            return !(x == y);
        }

        public static SqlBoolean operator <(SqlDateTime x, SqlDateTime y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean((x.m_day < y.m_day) || ((x.m_day == y.m_day) && (x.m_time < y.m_time)));
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator >(SqlDateTime x, SqlDateTime y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean((x.m_day > y.m_day) || ((x.m_day == y.m_day) && (x.m_time > y.m_time)));
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator <=(SqlDateTime x, SqlDateTime y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean((x.m_day < y.m_day) || ((x.m_day == y.m_day) && (x.m_time <= y.m_time)));
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean operator >=(SqlDateTime x, SqlDateTime y)
        {
            if (!x.IsNull && !y.IsNull)
            {
                return new SqlBoolean((x.m_day > y.m_day) || ((x.m_day == y.m_day) && (x.m_time >= y.m_time)));
            }
            return SqlBoolean.Null;
        }

        public static SqlBoolean Equals(SqlDateTime x, SqlDateTime y)
        {
            return (x == y);
        }

        public static SqlBoolean NotEquals(SqlDateTime x, SqlDateTime y)
        {
            return (x != y);
        }

        public static SqlBoolean LessThan(SqlDateTime x, SqlDateTime y)
        {
            return (x < y);
        }

        public static SqlBoolean GreaterThan(SqlDateTime x, SqlDateTime y)
        {
            return (x > y);
        }

        public static SqlBoolean LessThanOrEqual(SqlDateTime x, SqlDateTime y)
        {
            return (x <= y);
        }

        public static SqlBoolean GreaterThanOrEqual(SqlDateTime x, SqlDateTime y)
        {
            return (x >= y);
        }

        public SqlString ToSqlString()
        {
            return (SqlString) this;
        }

        public int CompareTo(object value)
        {
            if (!(value is SqlDateTime))
            {
                throw ADP.WrongType(value.GetType(), typeof(SqlDateTime));
            }
            SqlDateTime time = (SqlDateTime) value;
            return this.CompareTo(time);
        }

        public int CompareTo(SqlDateTime value)
        {
            if (this.IsNull)
            {
                if (!value.IsNull)
                {
                    return -1;
                }
                return 0;
            }
            if (value.IsNull)
            {
                return 1;
            }
            if (SqlBoolean.op_True(this < value))
            {
                return -1;
            }
            if (SqlBoolean.op_True(this > value))
            {
                return 1;
            }
            return 0;
        }

        public override bool Equals(object value)
        {
            if (!(value is SqlDateTime))
            {
                return false;
            }
            SqlDateTime time = (SqlDateTime) value;
            if (time.IsNull || this.IsNull)
            {
                return (time.IsNull && this.IsNull);
            }
            SqlBoolean flag = this == time;
            return flag.Value;
        }

        public override int GetHashCode()
        {
            if (!this.IsNull)
            {
                return this.Value.GetHashCode();
            }
            return 0;
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            string attribute = reader.GetAttribute("nil", "http://www.w3.org/2001/XMLSchema-instance");
            if ((attribute != null) && XmlConvert.ToBoolean(attribute))
            {
                reader.ReadElementString();
                this.m_fNotNull = false;
            }
            else
            {
                DateTime time = XmlConvert.ToDateTime(reader.ReadElementString(), XmlDateTimeSerializationMode.RoundtripKind);
                if (time.Kind != DateTimeKind.Unspecified)
                {
                    throw new SqlTypeException(SQLResource.TimeZoneSpecifiedMessage);
                }
                SqlDateTime time2 = FromDateTime(time);
                this.m_day = time2.DayTicks;
                this.m_time = time2.TimeTicks;
                this.m_fNotNull = true;
            }
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            if (this.IsNull)
            {
                writer.WriteAttributeString("xsi", "nil", "http://www.w3.org/2001/XMLSchema-instance", "true");
            }
            else
            {
                writer.WriteString(XmlConvert.ToString(this.Value, x_ISO8601_DateTimeFormat));
            }
        }

        public static XmlQualifiedName GetXsdType(XmlSchemaSet schemaSet)
        {
            return new XmlQualifiedName("dateTime", "http://www.w3.org/2001/XMLSchema");
        }

        static SqlDateTime()
        {
            SQLTicksPerMillisecond = 0.3;
            SQLTicksPerSecond = 300;
            SQLTicksPerMinute = SQLTicksPerSecond * 60;
            SQLTicksPerHour = SQLTicksPerMinute * 60;
            SQLTicksPerDay = SQLTicksPerHour * 0x18;
            TicksPerSecond = 0x989680L;
            SQLBaseDate = new DateTime(0x76c, 1, 1);
            SQLBaseDateTicks = SQLBaseDate.Ticks;
            MinYear = 0x6d9;
            MaxYear = 0x270f;
            MinDay = -53690;
            MaxDay = 0x2d247f;
            MinTime = 0;
            MaxTime = SQLTicksPerDay - 1;
            DayBase = 0xa955b;
            DaysToMonth365 = new int[] { 0, 0x1f, 0x3b, 90, 120, 0x97, 0xb5, 0xd4, 0xf3, 0x111, 0x130, 0x14e, 0x16d };
            DaysToMonth366 = new int[] { 0, 0x1f, 60, 0x5b, 0x79, 0x98, 0xb6, 0xd5, 0xf4, 0x112, 0x131, 0x14f, 0x16e };
            MinDateTime = new DateTime(0x6d9, 1, 1);
            MaxDateTime = DateTime.MaxValue;
            MinTimeSpan = MinDateTime.Subtract(SQLBaseDate);
            MaxTimeSpan = MaxDateTime.Subtract(SQLBaseDate);
            x_ISO8601_DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fff";
            x_DateTimeFormats = new string[] { "MMM d yyyy hh:mm:ss:ffftt", "MMM d yyyy hh:mm:ss:fff", "d MMM yyyy hh:mm:ss:ffftt", "d MMM yyyy hh:mm:ss:fff", "hh:mm:ss:ffftt", "hh:mm:ss:fff", "yyMMdd", "yyyyMMdd" };
            MinValue = new SqlDateTime(MinDay, 0);
            MaxValue = new SqlDateTime(MaxDay, MaxTime);
            Null = new SqlDateTime(true);
        }
    }
}

