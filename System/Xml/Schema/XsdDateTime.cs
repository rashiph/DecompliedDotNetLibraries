namespace System.Xml.Schema
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;

    [StructLayout(LayoutKind.Sequential)]
    internal struct XsdDateTime
    {
        private const uint TypeMask = 0xff000000;
        private const uint KindMask = 0xff0000;
        private const uint ZoneHourMask = 0xff00;
        private const uint ZoneMinuteMask = 0xff;
        private const int TypeShift = 0x18;
        private const int KindShift = 0x10;
        private const int ZoneHourShift = 8;
        private const short maxFractionDigits = 7;
        private DateTime dt;
        private uint extra;
        private static readonly int Lzyyyy;
        private static readonly int Lzyyyy_;
        private static readonly int Lzyyyy_MM;
        private static readonly int Lzyyyy_MM_;
        private static readonly int Lzyyyy_MM_dd;
        private static readonly int Lzyyyy_MM_ddT;
        private static readonly int LzHH;
        private static readonly int LzHH_;
        private static readonly int LzHH_mm;
        private static readonly int LzHH_mm_;
        private static readonly int LzHH_mm_ss;
        private static readonly int Lz_;
        private static readonly int Lz_zz;
        private static readonly int Lz_zz_;
        private static readonly int Lz_zz_zz;
        private static readonly int Lz__;
        private static readonly int Lz__mm;
        private static readonly int Lz__mm_;
        private static readonly int Lz__mm__;
        private static readonly int Lz__mm_dd;
        private static readonly int Lz___;
        private static readonly int Lz___dd;
        private static readonly XmlTypeCode[] typeCodes;
        public XsdDateTime(string text) : this(text, XsdDateTimeFlags.AllXsd)
        {
        }

        public XsdDateTime(string text, XsdDateTimeFlags kinds)
        {
            this = new XsdDateTime();
            Parser parser = new Parser();
            if (!parser.Parse(text, kinds))
            {
                throw new FormatException(Res.GetString("XmlConvert_BadFormat", new object[] { text, kinds }));
            }
            this.InitiateXsdDateTime(parser);
        }

        private XsdDateTime(Parser parser)
        {
            this = new XsdDateTime();
            this.InitiateXsdDateTime(parser);
        }

        private void InitiateXsdDateTime(Parser parser)
        {
            this.dt = new DateTime(parser.year, parser.month, parser.day, parser.hour, parser.minute, parser.second);
            if (parser.fraction != 0)
            {
                this.dt = this.dt.AddTicks((long) parser.fraction);
            }
            this.extra = (uint) ((((((int) parser.typeCode) << 0x18) | (((int) parser.kind) << 0x10)) | (parser.zoneHour << 8)) | parser.zoneMinute);
        }

        internal static bool TryParse(string text, XsdDateTimeFlags kinds, out XsdDateTime result)
        {
            Parser parser = new Parser();
            if (!parser.Parse(text, kinds))
            {
                result = new XsdDateTime();
                return false;
            }
            result = new XsdDateTime(parser);
            return true;
        }

        public XsdDateTime(DateTime dateTime, XsdDateTimeFlags kinds)
        {
            XsdDateTimeKind unspecified;
            this.dt = dateTime;
            DateTimeTypeCode code = (DateTimeTypeCode) (Bits.LeastPosition((uint) kinds) - 1);
            int hours = 0;
            int minutes = 0;
            switch (dateTime.Kind)
            {
                case DateTimeKind.Unspecified:
                    unspecified = XsdDateTimeKind.Unspecified;
                    break;

                case DateTimeKind.Utc:
                    unspecified = XsdDateTimeKind.Zulu;
                    break;

                default:
                {
                    TimeSpan utcOffset = TimeZoneInfo.Local.GetUtcOffset(dateTime);
                    if (utcOffset.Ticks < 0L)
                    {
                        unspecified = XsdDateTimeKind.LocalWestOfZulu;
                        hours = -utcOffset.Hours;
                        minutes = -utcOffset.Minutes;
                    }
                    else
                    {
                        unspecified = XsdDateTimeKind.LocalEastOfZulu;
                        hours = utcOffset.Hours;
                        minutes = utcOffset.Minutes;
                    }
                    break;
                }
            }
            this.extra = (uint) ((((((int) code) << 0x18) | (((int) unspecified) << 0x10)) | (hours << 8)) | minutes);
        }

        public XsdDateTime(DateTimeOffset dateTimeOffset) : this(dateTimeOffset, XsdDateTimeFlags.DateTime)
        {
        }

        public XsdDateTime(DateTimeOffset dateTimeOffset, XsdDateTimeFlags kinds)
        {
            XsdDateTimeKind localWestOfZulu;
            this.dt = dateTimeOffset.DateTime;
            TimeSpan offset = dateTimeOffset.Offset;
            DateTimeTypeCode code = (DateTimeTypeCode) (Bits.LeastPosition((uint) kinds) - 1);
            if (offset.TotalMinutes < 0.0)
            {
                offset = offset.Negate();
                localWestOfZulu = XsdDateTimeKind.LocalWestOfZulu;
            }
            else if (offset.TotalMinutes > 0.0)
            {
                localWestOfZulu = XsdDateTimeKind.LocalEastOfZulu;
            }
            else
            {
                localWestOfZulu = XsdDateTimeKind.Zulu;
            }
            this.extra = (uint) ((((((int) code) << 0x18) | (((int) localWestOfZulu) << 0x10)) | (offset.Hours << 8)) | offset.Minutes);
        }

        private DateTimeTypeCode InternalTypeCode
        {
            get
            {
                return (DateTimeTypeCode) ((this.extra & -16777216) >> 0x18);
            }
        }
        private XsdDateTimeKind InternalKind
        {
            get
            {
                return (XsdDateTimeKind) ((this.extra & 0xff0000) >> 0x10);
            }
        }
        public XmlTypeCode TypeCode
        {
            get
            {
                return typeCodes[(int) this.InternalTypeCode];
            }
        }
        public DateTimeKind Kind
        {
            get
            {
                switch (this.InternalKind)
                {
                    case XsdDateTimeKind.Unspecified:
                        return DateTimeKind.Unspecified;

                    case XsdDateTimeKind.Zulu:
                        return DateTimeKind.Utc;
                }
                return DateTimeKind.Local;
            }
        }
        public int Year
        {
            get
            {
                return this.dt.Year;
            }
        }
        public int Month
        {
            get
            {
                return this.dt.Month;
            }
        }
        public int Day
        {
            get
            {
                return this.dt.Day;
            }
        }
        public int Hour
        {
            get
            {
                return this.dt.Hour;
            }
        }
        public int Minute
        {
            get
            {
                return this.dt.Minute;
            }
        }
        public int Second
        {
            get
            {
                return this.dt.Second;
            }
        }
        public int Fraction
        {
            get
            {
                DateTime time = new DateTime(this.dt.Year, this.dt.Month, this.dt.Day, this.dt.Hour, this.dt.Minute, this.dt.Second);
                return (int) (this.dt.Ticks - time.Ticks);
            }
        }
        public int ZoneHour
        {
            get
            {
                uint num = (uint) ((this.extra & 0xff00) >> 8);
                return (int) num;
            }
        }
        public int ZoneMinute
        {
            get
            {
                uint num = this.extra & 0xff;
                return (int) num;
            }
        }
        public DateTime ToZulu()
        {
            switch (this.InternalKind)
            {
                case XsdDateTimeKind.Zulu:
                    return new DateTime(this.dt.Ticks, DateTimeKind.Utc);

                case XsdDateTimeKind.LocalWestOfZulu:
                    return new DateTime(this.dt.Add(new TimeSpan(this.ZoneHour, this.ZoneMinute, 0)).Ticks, DateTimeKind.Utc);

                case XsdDateTimeKind.LocalEastOfZulu:
                    return new DateTime(this.dt.Subtract(new TimeSpan(this.ZoneHour, this.ZoneMinute, 0)).Ticks, DateTimeKind.Utc);
            }
            return this.dt;
        }

        public static implicit operator DateTime(XsdDateTime xdt)
        {
            DateTime dt;
            switch (xdt.InternalTypeCode)
            {
                case DateTimeTypeCode.GDay:
                case DateTimeTypeCode.GMonth:
                    dt = new DateTime(DateTime.Now.Year, xdt.Month, xdt.Day);
                    break;

                case DateTimeTypeCode.Time:
                {
                    DateTime now = DateTime.Now;
                    TimeSpan span = (TimeSpan) (new DateTime(now.Year, now.Month, now.Day) - new DateTime(xdt.Year, xdt.Month, xdt.Day));
                    dt = xdt.dt.Add(span);
                    break;
                }
                default:
                    dt = xdt.dt;
                    break;
            }
            switch (xdt.InternalKind)
            {
                case XsdDateTimeKind.Zulu:
                    return new DateTime(dt.Ticks, DateTimeKind.Utc);

                case XsdDateTimeKind.LocalWestOfZulu:
                {
                    try
                    {
                        dt = dt.Add(new TimeSpan(xdt.ZoneHour, xdt.ZoneMinute, 0));
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        return new DateTime(DateTime.MaxValue.Ticks, DateTimeKind.Local);
                    }
                    DateTime time8 = new DateTime(dt.Ticks, DateTimeKind.Utc);
                    return time8.ToLocalTime();
                }
                case XsdDateTimeKind.LocalEastOfZulu:
                {
                    try
                    {
                        dt = dt.Subtract(new TimeSpan(xdt.ZoneHour, xdt.ZoneMinute, 0));
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        return new DateTime(DateTime.MinValue.Ticks, DateTimeKind.Local);
                    }
                    DateTime time6 = new DateTime(dt.Ticks, DateTimeKind.Utc);
                    return time6.ToLocalTime();
                }
            }
            return dt;
        }

        public static implicit operator DateTimeOffset(XsdDateTime xdt)
        {
            DateTime dt;
            switch (xdt.InternalTypeCode)
            {
                case DateTimeTypeCode.GDay:
                case DateTimeTypeCode.GMonth:
                    dt = new DateTime(DateTime.Now.Year, xdt.Month, xdt.Day);
                    break;

                case DateTimeTypeCode.Time:
                {
                    DateTime now = DateTime.Now;
                    TimeSpan span = (TimeSpan) (new DateTime(now.Year, now.Month, now.Day) - new DateTime(xdt.Year, xdt.Month, xdt.Day));
                    dt = xdt.dt.Add(span);
                    break;
                }
                default:
                    dt = xdt.dt;
                    break;
            }
            switch (xdt.InternalKind)
            {
                case XsdDateTimeKind.Zulu:
                    return new DateTimeOffset(dt, new TimeSpan(0L));

                case XsdDateTimeKind.LocalWestOfZulu:
                    return new DateTimeOffset(dt, new TimeSpan(-xdt.ZoneHour, -xdt.ZoneMinute, 0));

                case XsdDateTimeKind.LocalEastOfZulu:
                    return new DateTimeOffset(dt, new TimeSpan(xdt.ZoneHour, xdt.ZoneMinute, 0));
            }
            return new DateTimeOffset(dt, TimeZoneInfo.Local.GetUtcOffset(dt));
        }

        public static int Compare(XsdDateTime left, XsdDateTime right)
        {
            if (left.extra == right.extra)
            {
                return DateTime.Compare(left.dt, right.dt);
            }
            if (left.InternalTypeCode != right.InternalTypeCode)
            {
                throw new ArgumentException(Res.GetString("Sch_XsdDateTimeCompare", new object[] { left.TypeCode, right.TypeCode }));
            }
            return DateTime.Compare(left.GetZuluDateTime(), right.GetZuluDateTime());
        }

        public int CompareTo(object value)
        {
            if (value == null)
            {
                return 1;
            }
            return Compare(this, (XsdDateTime) value);
        }

        public override string ToString()
        {
            char[] chArray;
            StringBuilder sb = new StringBuilder(0x40);
            switch (this.InternalTypeCode)
            {
                case DateTimeTypeCode.DateTime:
                    this.PrintDate(sb);
                    sb.Append('T');
                    this.PrintTime(sb);
                    break;

                case DateTimeTypeCode.Time:
                    this.PrintTime(sb);
                    break;

                case DateTimeTypeCode.Date:
                    this.PrintDate(sb);
                    break;

                case DateTimeTypeCode.GYearMonth:
                    chArray = new char[Lzyyyy_MM];
                    this.IntToCharArray(chArray, 0, this.Year, 4);
                    chArray[Lzyyyy] = '-';
                    this.ShortToCharArray(chArray, Lzyyyy_, this.Month);
                    sb.Append(chArray);
                    break;

                case DateTimeTypeCode.GYear:
                    chArray = new char[Lzyyyy];
                    this.IntToCharArray(chArray, 0, this.Year, 4);
                    sb.Append(chArray);
                    break;

                case DateTimeTypeCode.GMonthDay:
                    chArray = new char[Lz__mm_dd];
                    chArray[0] = '-';
                    chArray[Lz_] = '-';
                    this.ShortToCharArray(chArray, Lz__, this.Month);
                    chArray[Lz__mm] = '-';
                    this.ShortToCharArray(chArray, Lz__mm_, this.Day);
                    sb.Append(chArray);
                    break;

                case DateTimeTypeCode.GDay:
                    chArray = new char[Lz___dd];
                    chArray[0] = '-';
                    chArray[Lz_] = '-';
                    chArray[Lz__] = '-';
                    this.ShortToCharArray(chArray, Lz___, this.Day);
                    sb.Append(chArray);
                    break;

                case DateTimeTypeCode.GMonth:
                    chArray = new char[Lz__mm__];
                    chArray[0] = '-';
                    chArray[Lz_] = '-';
                    this.ShortToCharArray(chArray, Lz__, this.Month);
                    chArray[Lz__mm] = '-';
                    chArray[Lz__mm_] = '-';
                    sb.Append(chArray);
                    break;
            }
            this.PrintZone(sb);
            return sb.ToString();
        }

        private void PrintDate(StringBuilder sb)
        {
            char[] text = new char[Lzyyyy_MM_dd];
            this.IntToCharArray(text, 0, this.Year, 4);
            text[Lzyyyy] = '-';
            this.ShortToCharArray(text, Lzyyyy_, this.Month);
            text[Lzyyyy_MM] = '-';
            this.ShortToCharArray(text, Lzyyyy_MM_, this.Day);
            sb.Append(text);
        }

        private void PrintTime(StringBuilder sb)
        {
            char[] text = new char[LzHH_mm_ss];
            this.ShortToCharArray(text, 0, this.Hour);
            text[LzHH] = ':';
            this.ShortToCharArray(text, LzHH_, this.Minute);
            text[LzHH_mm] = ':';
            this.ShortToCharArray(text, LzHH_mm_, this.Second);
            sb.Append(text);
            int fraction = this.Fraction;
            if (fraction != 0)
            {
                int digits = 7;
                while ((fraction % 10) == 0)
                {
                    digits--;
                    fraction /= 10;
                }
                text = new char[digits + 1];
                text[0] = '.';
                this.IntToCharArray(text, 1, fraction, digits);
                sb.Append(text);
            }
        }

        private void PrintZone(StringBuilder sb)
        {
            char[] chArray;
            switch (this.InternalKind)
            {
                case XsdDateTimeKind.Zulu:
                    sb.Append('Z');
                    return;

                case XsdDateTimeKind.LocalWestOfZulu:
                    chArray = new char[Lz_zz_zz];
                    chArray[0] = '-';
                    this.ShortToCharArray(chArray, Lz_, this.ZoneHour);
                    chArray[Lz_zz] = ':';
                    this.ShortToCharArray(chArray, Lz_zz_, this.ZoneMinute);
                    sb.Append(chArray);
                    return;

                case XsdDateTimeKind.LocalEastOfZulu:
                    chArray = new char[Lz_zz_zz];
                    chArray[0] = '+';
                    this.ShortToCharArray(chArray, Lz_, this.ZoneHour);
                    chArray[Lz_zz] = ':';
                    this.ShortToCharArray(chArray, Lz_zz_, this.ZoneMinute);
                    sb.Append(chArray);
                    return;
            }
        }

        private void IntToCharArray(char[] text, int start, int value, int digits)
        {
            while (digits-- != 0)
            {
                text[start + digits] = (char) ((value % 10) + 0x30);
                value /= 10;
            }
        }

        private void ShortToCharArray(char[] text, int start, int value)
        {
            text[start] = (char) ((value / 10) + 0x30);
            text[start + 1] = (char) ((value % 10) + 0x30);
        }

        private DateTime GetZuluDateTime()
        {
            switch (this.InternalKind)
            {
                case XsdDateTimeKind.Zulu:
                    return this.dt;

                case XsdDateTimeKind.LocalWestOfZulu:
                    return this.dt.Add(new TimeSpan(this.ZoneHour, this.ZoneMinute, 0));

                case XsdDateTimeKind.LocalEastOfZulu:
                    return this.dt.Subtract(new TimeSpan(this.ZoneHour, this.ZoneMinute, 0));
            }
            return this.dt.ToUniversalTime();
        }

        static XsdDateTime()
        {
            Lzyyyy = "yyyy".Length;
            Lzyyyy_ = "yyyy-".Length;
            Lzyyyy_MM = "yyyy-MM".Length;
            Lzyyyy_MM_ = "yyyy-MM-".Length;
            Lzyyyy_MM_dd = "yyyy-MM-dd".Length;
            Lzyyyy_MM_ddT = "yyyy-MM-ddT".Length;
            LzHH = "HH".Length;
            LzHH_ = "HH:".Length;
            LzHH_mm = "HH:mm".Length;
            LzHH_mm_ = "HH:mm:".Length;
            LzHH_mm_ss = "HH:mm:ss".Length;
            Lz_ = "-".Length;
            Lz_zz = "-zz".Length;
            Lz_zz_ = "-zz:".Length;
            Lz_zz_zz = "-zz:zz".Length;
            Lz__ = "--".Length;
            Lz__mm = "--MM".Length;
            Lz__mm_ = "--MM-".Length;
            Lz__mm__ = "--MM--".Length;
            Lz__mm_dd = "--MM-dd".Length;
            Lz___ = "---".Length;
            Lz___dd = "---dd".Length;
            typeCodes = new XmlTypeCode[] { XmlTypeCode.DateTime, XmlTypeCode.Time, XmlTypeCode.Date, XmlTypeCode.GYearMonth, XmlTypeCode.GYear, XmlTypeCode.GMonthDay, XmlTypeCode.GDay, XmlTypeCode.GMonth };
        }
        private enum DateTimeTypeCode
        {
            DateTime,
            Time,
            Date,
            GYearMonth,
            GYear,
            GMonthDay,
            GDay,
            GMonth,
            XdrDateTime
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Parser
        {
            private const int leapYear = 0x770;
            private const int firstMonth = 1;
            private const int firstDay = 1;
            public XsdDateTime.DateTimeTypeCode typeCode;
            public int year;
            public int month;
            public int day;
            public int hour;
            public int minute;
            public int second;
            public int fraction;
            public XsdDateTime.XsdDateTimeKind kind;
            public int zoneHour;
            public int zoneMinute;
            private string text;
            private int length;
            private static int[] Power10;
            public bool Parse(string text, XsdDateTimeFlags kinds)
            {
                this.text = text;
                this.length = text.Length;
                int start = 0;
                while ((start < this.length) && char.IsWhiteSpace(text[start]))
                {
                    start++;
                }
                if (Test(kinds, XsdDateTimeFlags.XdrDateTime | XsdDateTimeFlags.XdrDateTimeNoTz | XsdDateTimeFlags.Date | XsdDateTimeFlags.DateTime) && this.ParseDate(start))
                {
                    if ((Test(kinds, XsdDateTimeFlags.DateTime) && this.ParseChar(start + XsdDateTime.Lzyyyy_MM_dd, 'T')) && this.ParseTimeAndZoneAndWhitespace(start + XsdDateTime.Lzyyyy_MM_ddT))
                    {
                        this.typeCode = XsdDateTime.DateTimeTypeCode.DateTime;
                        return true;
                    }
                    if (Test(kinds, XsdDateTimeFlags.Date) && this.ParseZoneAndWhitespace(start + XsdDateTime.Lzyyyy_MM_dd))
                    {
                        this.typeCode = XsdDateTime.DateTimeTypeCode.Date;
                        return true;
                    }
                    if (Test(kinds, XsdDateTimeFlags.XdrDateTime) && (this.ParseZoneAndWhitespace(start + XsdDateTime.Lzyyyy_MM_dd) || (this.ParseChar(start + XsdDateTime.Lzyyyy_MM_dd, 'T') && this.ParseTimeAndZoneAndWhitespace(start + XsdDateTime.Lzyyyy_MM_ddT))))
                    {
                        this.typeCode = XsdDateTime.DateTimeTypeCode.XdrDateTime;
                        return true;
                    }
                    if (Test(kinds, XsdDateTimeFlags.XdrDateTimeNoTz))
                    {
                        if (!this.ParseChar(start + XsdDateTime.Lzyyyy_MM_dd, 'T'))
                        {
                            this.typeCode = XsdDateTime.DateTimeTypeCode.XdrDateTime;
                            return true;
                        }
                        if (this.ParseTimeAndWhitespace(start + XsdDateTime.Lzyyyy_MM_ddT))
                        {
                            this.typeCode = XsdDateTime.DateTimeTypeCode.XdrDateTime;
                            return true;
                        }
                    }
                }
                if (Test(kinds, XsdDateTimeFlags.Time) && this.ParseTimeAndZoneAndWhitespace(start))
                {
                    this.year = 0x770;
                    this.month = 1;
                    this.day = 1;
                    this.typeCode = XsdDateTime.DateTimeTypeCode.Time;
                    return true;
                }
                if (Test(kinds, XsdDateTimeFlags.XdrTimeNoTz) && this.ParseTimeAndWhitespace(start))
                {
                    this.year = 0x770;
                    this.month = 1;
                    this.day = 1;
                    this.typeCode = XsdDateTime.DateTimeTypeCode.Time;
                    return true;
                }
                if ((Test(kinds, XsdDateTimeFlags.GYear | XsdDateTimeFlags.GYearMonth) && this.Parse4Dig(start, ref this.year)) && (1 <= this.year))
                {
                    if (((Test(kinds, XsdDateTimeFlags.GYearMonth) && this.ParseChar(start + XsdDateTime.Lzyyyy, '-')) && (this.Parse2Dig(start + XsdDateTime.Lzyyyy_, ref this.month) && (1 <= this.month))) && ((this.month <= 12) && this.ParseZoneAndWhitespace(start + XsdDateTime.Lzyyyy_MM)))
                    {
                        this.day = 1;
                        this.typeCode = XsdDateTime.DateTimeTypeCode.GYearMonth;
                        return true;
                    }
                    if (Test(kinds, XsdDateTimeFlags.GYear) && this.ParseZoneAndWhitespace(start + XsdDateTime.Lzyyyy))
                    {
                        this.month = 1;
                        this.day = 1;
                        this.typeCode = XsdDateTime.DateTimeTypeCode.GYear;
                        return true;
                    }
                }
                if (((Test(kinds, XsdDateTimeFlags.GMonth | XsdDateTimeFlags.GMonthDay) && this.ParseChar(start, '-')) && (this.ParseChar(start + XsdDateTime.Lz_, '-') && this.Parse2Dig(start + XsdDateTime.Lz__, ref this.month))) && ((1 <= this.month) && (this.month <= 12)))
                {
                    if (((Test(kinds, XsdDateTimeFlags.GMonthDay) && this.ParseChar(start + XsdDateTime.Lz__mm, '-')) && (this.Parse2Dig(start + XsdDateTime.Lz__mm_, ref this.day) && (1 <= this.day))) && ((this.day <= DateTime.DaysInMonth(0x770, this.month)) && this.ParseZoneAndWhitespace(start + XsdDateTime.Lz__mm_dd)))
                    {
                        this.year = 0x770;
                        this.typeCode = XsdDateTime.DateTimeTypeCode.GMonthDay;
                        return true;
                    }
                    if (Test(kinds, XsdDateTimeFlags.GMonth) && (this.ParseZoneAndWhitespace(start + XsdDateTime.Lz__mm) || ((this.ParseChar(start + XsdDateTime.Lz__mm, '-') && this.ParseChar(start + XsdDateTime.Lz__mm_, '-')) && this.ParseZoneAndWhitespace(start + XsdDateTime.Lz__mm__))))
                    {
                        this.year = 0x770;
                        this.day = 1;
                        this.typeCode = XsdDateTime.DateTimeTypeCode.GMonth;
                        return true;
                    }
                }
                if (((Test(kinds, XsdDateTimeFlags.GDay) && this.ParseChar(start, '-')) && (this.ParseChar(start + XsdDateTime.Lz_, '-') && this.ParseChar(start + XsdDateTime.Lz__, '-'))) && ((this.Parse2Dig(start + XsdDateTime.Lz___, ref this.day) && (1 <= this.day)) && ((this.day <= DateTime.DaysInMonth(0x770, 1)) && this.ParseZoneAndWhitespace(start + XsdDateTime.Lz___dd))))
                {
                    this.year = 0x770;
                    this.month = 1;
                    this.typeCode = XsdDateTime.DateTimeTypeCode.GDay;
                    return true;
                }
                return false;
            }

            private bool ParseDate(int start)
            {
                return (((((this.Parse4Dig(start, ref this.year) && (1 <= this.year)) && (this.ParseChar(start + XsdDateTime.Lzyyyy, '-') && this.Parse2Dig(start + XsdDateTime.Lzyyyy_, ref this.month))) && (((1 <= this.month) && (this.month <= 12)) && (this.ParseChar(start + XsdDateTime.Lzyyyy_MM, '-') && this.Parse2Dig(start + XsdDateTime.Lzyyyy_MM_, ref this.day)))) && (1 <= this.day)) && (this.day <= DateTime.DaysInMonth(this.year, this.month)));
            }

            private bool ParseTimeAndZoneAndWhitespace(int start)
            {
                return (this.ParseTime(ref start) && this.ParseZoneAndWhitespace(start));
            }

            private bool ParseTimeAndWhitespace(int start)
            {
                if (!this.ParseTime(ref start))
                {
                    return false;
                }
                while (start < this.length)
                {
                    start++;
                }
                return (start == this.length);
            }

            private bool ParseTime(ref int start)
            {
                if (((!this.Parse2Dig(start, ref this.hour) || (this.hour >= 0x18)) || (!this.ParseChar(start + XsdDateTime.LzHH, ':') || !this.Parse2Dig(start + XsdDateTime.LzHH_, ref this.minute))) || (((this.minute >= 60) || !this.ParseChar(start + XsdDateTime.LzHH_mm, ':')) || (!this.Parse2Dig(start + XsdDateTime.LzHH_mm_, ref this.second) || (this.second >= 60))))
                {
                    this.hour = 0;
                    return false;
                }
                start += XsdDateTime.LzHH_mm_ss;
                if (this.ParseChar(start, '.'))
                {
                    this.fraction = 0;
                    int num = 0;
                    int num2 = 0;
                    while (++start < this.length)
                    {
                        int num3 = this.text[start] - '0';
                        if (9 < num3)
                        {
                            break;
                        }
                        if (num < 7)
                        {
                            this.fraction = (this.fraction * 10) + num3;
                        }
                        else if (num == 7)
                        {
                            if (5 < num3)
                            {
                                num2 = 1;
                            }
                            else if (num3 == 5)
                            {
                                num2 = -1;
                            }
                        }
                        else if ((num2 < 0) && (num3 != 0))
                        {
                            num2 = 1;
                        }
                        num++;
                    }
                    if (num < 7)
                    {
                        if (num == 0)
                        {
                            return false;
                        }
                        this.fraction *= Power10[7 - num];
                    }
                    else
                    {
                        if (num2 < 0)
                        {
                            num2 = this.fraction & 1;
                        }
                        this.fraction += num2;
                    }
                }
                return true;
            }

            private bool ParseZoneAndWhitespace(int start)
            {
                if (start < this.length)
                {
                    char ch = this.text[start];
                    switch (ch)
                    {
                        case 'Z':
                        case 'z':
                            this.kind = XsdDateTime.XsdDateTimeKind.Zulu;
                            start++;
                            goto Label_00C4;
                    }
                    if (((((start + 5) < this.length) && this.Parse2Dig(start + XsdDateTime.Lz_, ref this.zoneHour)) && ((this.zoneHour <= 0x63) && this.ParseChar(start + XsdDateTime.Lz_zz, ':'))) && (this.Parse2Dig(start + XsdDateTime.Lz_zz_, ref this.zoneMinute) && (this.zoneMinute <= 0x63)))
                    {
                        switch (ch)
                        {
                            case '-':
                                this.kind = XsdDateTime.XsdDateTimeKind.LocalWestOfZulu;
                                start += XsdDateTime.Lz_zz_zz;
                                break;

                            case '+':
                                this.kind = XsdDateTime.XsdDateTimeKind.LocalEastOfZulu;
                                start += XsdDateTime.Lz_zz_zz;
                                break;
                        }
                    }
                }
            Label_00C4:
                while ((start < this.length) && char.IsWhiteSpace(this.text[start]))
                {
                    start++;
                }
                return (start == this.length);
            }

            private bool Parse4Dig(int start, ref int num)
            {
                if ((start + 3) < this.length)
                {
                    int num2 = this.text[start] - '0';
                    int num3 = this.text[start + 1] - '0';
                    int num4 = this.text[start + 2] - '0';
                    int num5 = this.text[start + 3] - '0';
                    if ((((0 <= num2) && (num2 < 10)) && ((0 <= num3) && (num3 < 10))) && (((0 <= num4) && (num4 < 10)) && ((0 <= num5) && (num5 < 10))))
                    {
                        num = (((((num2 * 10) + num3) * 10) + num4) * 10) + num5;
                        return true;
                    }
                }
                return false;
            }

            private bool Parse2Dig(int start, ref int num)
            {
                if ((start + 1) < this.length)
                {
                    int num2 = this.text[start] - '0';
                    int num3 = this.text[start + 1] - '0';
                    if (((0 <= num2) && (num2 < 10)) && ((0 <= num3) && (num3 < 10)))
                    {
                        num = (num2 * 10) + num3;
                        return true;
                    }
                }
                return false;
            }

            private bool ParseChar(int start, char ch)
            {
                return ((start < this.length) && (this.text[start] == ch));
            }

            private static bool Test(XsdDateTimeFlags left, XsdDateTimeFlags right)
            {
                return ((left & right) != 0);
            }

            static Parser()
            {
                Power10 = new int[] { -1, 10, 100, 0x3e8, 0x2710, 0x186a0, 0xf4240 };
            }
        }

        private enum XsdDateTimeKind
        {
            Unspecified,
            Zulu,
            LocalWestOfZulu,
            LocalEastOfZulu
        }
    }
}

