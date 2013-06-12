namespace System.Globalization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Threading;

    [Serializable, ComVisible(true)]
    public sealed class DateTimeFormatInfo : ICloneable, IFormatProvider
    {
        internal string[] abbreviatedDayNames;
        internal string[] abbreviatedMonthNames;
        internal string[] allLongDatePatterns;
        internal string[] allLongTimePatterns;
        internal string[] allShortDatePatterns;
        internal string[] allShortTimePatterns;
        [OptionalField(VersionAdded=3)]
        private string[] allYearMonthPatterns;
        internal string amDesignator;
        [OptionalField(VersionAdded=1)]
        private bool bUseCalendarInfo;
        internal System.Globalization.Calendar calendar;
        internal int calendarWeekRule;
        internal const string ChineseHourSuff = "时";
        internal const string CJKDaySuff = "日";
        internal const string CJKHourSuff = "時";
        internal const string CJKMinuteSuff = "分";
        internal const string CJKMonthSuff = "月";
        internal const string CJKSecondSuff = "秒";
        internal const string CJKYearSuff = "年";
        [OptionalField(VersionAdded=1)]
        private int CultureID;
        [OptionalField(VersionAdded=1)]
        internal string dateSeparator;
        private const string dateSeparatorOrTimeZoneOffset = "-";
        [OptionalField(VersionAdded=2)]
        internal string dateTimeOffsetPattern;
        internal string[] dayNames;
        private const int DEFAULT_ALL_DATETIMES_SIZE = 0x84;
        internal const string EnglishLangName = "en";
        internal int firstDayOfWeek;
        [OptionalField(VersionAdded=2)]
        internal DateTimeFormatFlags formatFlags;
        [OptionalField(VersionAdded=1)]
        internal string fullDateTimePattern;
        [OptionalField(VersionAdded=1)]
        internal string generalLongTimePattern;
        [OptionalField(VersionAdded=1)]
        internal string generalShortTimePattern;
        [OptionalField(VersionAdded=2)]
        internal string[] genitiveMonthNames;
        internal const string IgnorableComma = ",";
        internal const string IgnorablePeriod = ".";
        internal const DateTimeStyles InvalidDateTimeStyles = ~(DateTimeStyles.RoundtripKind | DateTimeStyles.AssumeUniversal | DateTimeStyles.AssumeLocal | DateTimeStyles.AdjustToUniversal | DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AllowWhiteSpaces);
        private const string invariantDateSeparator = "/";
        private static DateTimeFormatInfo invariantInfo;
        private const string invariantTimeSeparator = ":";
        internal const string JapaneseLangName = "ja";
        internal const string KoreanDaySuff = "일";
        internal const string KoreanHourSuff = "시";
        internal const string KoreanLangName = "ko";
        internal const string KoreanMinuteSuff = "분";
        internal const string KoreanMonthSuff = "월";
        internal const string KoreanSecondSuff = "초";
        internal const string KoreanYearSuff = "년";
        [OptionalField(VersionAdded=2)]
        internal string[] leapYearMonthNames;
        internal const string LocalTimeMark = "T";
        internal string longDatePattern;
        internal string longTimePattern;
        internal string[] m_abbrevEnglishEraNames;
        internal string[] m_abbrevEraNames;
        [NonSerialized]
        private System.Globalization.CompareInfo m_compareInfo;
        [NonSerialized]
        private CultureData m_cultureData;
        [NonSerialized]
        private CultureInfo m_cultureInfo;
        [OptionalField(VersionAdded=1)]
        internal string[] m_dateWords;
        [NonSerialized]
        private TokenHashValue[] m_dtfiTokenHash;
        internal string[] m_eraNames;
        [NonSerialized]
        private string m_fullTimeSpanNegativePattern;
        [NonSerialized]
        private string m_fullTimeSpanPositivePattern;
        [OptionalField(VersionAdded=2)]
        internal string[] m_genitiveAbbreviatedMonthNames;
        [OptionalField(VersionAdded=2)]
        internal bool m_isDefaultCalendar;
        internal bool m_isReadOnly;
        [NonSerialized]
        private string m_langName;
        [OptionalField(VersionAdded=2)]
        internal string m_name;
        [OptionalField(VersionAdded=2)]
        internal string[] m_superShortDayNames;
        [OptionalField(VersionAdded=1)]
        private bool m_useUserOverride;
        internal string monthDayPattern;
        internal string[] monthNames;
        private static char[] MonthSpaces = new char[] { ' ', '\x00a0' };
        [OptionalField(VersionAdded=1)]
        private int nDataItem;
        internal int[] optionalCalendars;
        internal string pmDesignator;
        internal const string rfc1123Pattern = "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'";
        [OptionalField(VersionAdded=2)]
        private static Hashtable s_calendarNativeNames;
        private static DateTimeFormatInfo s_jajpDTFI;
        private static DateTimeFormatInfo s_zhtwDTFI;
        private const int SECOND_PRIME = 0xc5;
        internal string shortDatePattern;
        internal string shortTimePattern;
        internal const string sortableDateTimePattern = "yyyy'-'MM'-'dd'T'HH':'mm':'ss";
        [OptionalField(VersionAdded=1)]
        internal string timeSeparator;
        private const int TOKEN_HASH_SIZE = 0xc7;
        internal const string universalSortableDateTimePattern = "yyyy'-'MM'-'dd HH':'mm':'ss'Z'";
        internal string yearMonthPattern;

        public DateTimeFormatInfo() : this(CultureInfo.InvariantCulture.m_cultureData, GregorianCalendar.GetDefaultInstance())
        {
        }

        internal DateTimeFormatInfo(CultureData cultureData, System.Globalization.Calendar cal)
        {
            this.firstDayOfWeek = -1;
            this.calendarWeekRule = -1;
            this.formatFlags = ~DateTimeFormatFlags.None;
            this.m_cultureData = cultureData;
            this.Calendar = cal;
            this.InitializeOverridableProperties(cultureData, this.Calendar.ID);
        }

        private void AddMonthNames(TokenHashValue[] temp, string monthPostfix)
        {
            for (int i = 1; i <= 13; i++)
            {
                string monthName = this.GetMonthName(i);
                if (monthName.Length > 0)
                {
                    if (monthPostfix != null)
                    {
                        this.InsertHash(temp, monthName + monthPostfix, TokenType.MonthToken, i);
                    }
                    else
                    {
                        this.InsertHash(temp, monthName, TokenType.MonthToken, i);
                    }
                }
                monthName = this.GetAbbreviatedMonthName(i);
                this.InsertHash(temp, monthName, TokenType.MonthToken, i);
            }
        }

        private static void CheckNullValue(string[] values, int length)
        {
            for (int i = 0; i < length; i++)
            {
                if (values[i] == null)
                {
                    throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_ArrayValue"));
                }
            }
        }

        private void ClearTokenHashTable()
        {
            this.m_dtfiTokenHash = null;
            this.formatFlags = ~DateTimeFormatFlags.None;
        }

        [SecuritySafeCritical]
        public object Clone()
        {
            DateTimeFormatInfo info = (DateTimeFormatInfo) base.MemberwiseClone();
            info.calendar = (System.Globalization.Calendar) this.Calendar.Clone();
            info.m_isReadOnly = false;
            return info;
        }

        [SecurityCritical]
        internal TokenHashValue[] CreateTokenHashTable()
        {
            TokenHashValue[] dtfiTokenHash = this.m_dtfiTokenHash;
            if (dtfiTokenHash == null)
            {
                dtfiTokenHash = new TokenHashValue[0xc7];
                bool flag = this.LanguageName.Equals("ko");
                string str = this.TimeSeparator.Trim();
                if ("," != str)
                {
                    this.InsertHash(dtfiTokenHash, ",", TokenType.IgnorableSymbol, 0);
                }
                if ("." != str)
                {
                    this.InsertHash(dtfiTokenHash, ".", TokenType.IgnorableSymbol, 0);
                }
                if ((("시" != str) && ("時" != str)) && ("时" != str))
                {
                    this.InsertHash(dtfiTokenHash, this.TimeSeparator, TokenType.SEP_Time, 0);
                }
                this.InsertHash(dtfiTokenHash, this.AMDesignator, TokenType.SEP_Am | TokenType.Am, 0);
                this.InsertHash(dtfiTokenHash, this.PMDesignator, TokenType.SEP_Pm | TokenType.Pm, 1);
                if (this.LanguageName.Equals("sq"))
                {
                    this.InsertHash(dtfiTokenHash, "." + this.AMDesignator, TokenType.SEP_Am | TokenType.Am, 0);
                    this.InsertHash(dtfiTokenHash, "." + this.PMDesignator, TokenType.SEP_Pm | TokenType.Pm, 1);
                }
                this.InsertHash(dtfiTokenHash, "年", TokenType.SEP_YearSuff, 0);
                this.InsertHash(dtfiTokenHash, "년", TokenType.SEP_YearSuff, 0);
                this.InsertHash(dtfiTokenHash, "月", TokenType.SEP_MonthSuff, 0);
                this.InsertHash(dtfiTokenHash, "월", TokenType.SEP_MonthSuff, 0);
                this.InsertHash(dtfiTokenHash, "日", TokenType.SEP_DaySuff, 0);
                this.InsertHash(dtfiTokenHash, "일", TokenType.SEP_DaySuff, 0);
                this.InsertHash(dtfiTokenHash, "時", TokenType.SEP_HourSuff, 0);
                this.InsertHash(dtfiTokenHash, "时", TokenType.SEP_HourSuff, 0);
                this.InsertHash(dtfiTokenHash, "分", TokenType.SEP_MinuteSuff, 0);
                this.InsertHash(dtfiTokenHash, "秒", TokenType.SEP_SecondSuff, 0);
                if (flag)
                {
                    this.InsertHash(dtfiTokenHash, "시", TokenType.SEP_HourSuff, 0);
                    this.InsertHash(dtfiTokenHash, "분", TokenType.SEP_MinuteSuff, 0);
                    this.InsertHash(dtfiTokenHash, "초", TokenType.SEP_SecondSuff, 0);
                }
                if (this.LanguageName.Equals("ky"))
                {
                    this.InsertHash(dtfiTokenHash, "-", TokenType.IgnorableSymbol, 0);
                }
                else
                {
                    this.InsertHash(dtfiTokenHash, "-", TokenType.SEP_DateOrOffset, 0);
                }
                string[] strArray = null;
                DateTimeFormatInfoScanner scanner = null;
                scanner = new DateTimeFormatInfoScanner();
                this.m_dateWords = strArray = scanner.GetDateWordsOfDTFI(this);
                DateTimeFormatFlags formatFlags = this.FormatFlags;
                bool flag2 = false;
                string monthPostfix = null;
                if (strArray != null)
                {
                    for (int num = 0; num < strArray.Length; num++)
                    {
                        switch (strArray[num][0])
                        {
                            case 0xe000:
                                monthPostfix = strArray[num].Substring(1);
                                this.AddMonthNames(dtfiTokenHash, monthPostfix);
                                break;

                            case 0xe001:
                            {
                                string str3 = strArray[num].Substring(1);
                                this.InsertHash(dtfiTokenHash, str3, TokenType.IgnorableSymbol, 0);
                                if (this.DateSeparator.Trim(null).Equals(str3))
                                {
                                    flag2 = true;
                                }
                                break;
                            }
                            default:
                                this.InsertHash(dtfiTokenHash, strArray[num], TokenType.DateWordToken, 0);
                                if (this.LanguageName.Equals("eu"))
                                {
                                    this.InsertHash(dtfiTokenHash, "." + strArray[num], TokenType.DateWordToken, 0);
                                }
                                break;
                        }
                    }
                }
                if (!flag2)
                {
                    this.InsertHash(dtfiTokenHash, this.DateSeparator, TokenType.SEP_Date, 0);
                }
                this.AddMonthNames(dtfiTokenHash, null);
                for (int i = 1; i <= 13; i++)
                {
                    this.InsertHash(dtfiTokenHash, this.GetAbbreviatedMonthName(i), TokenType.MonthToken, i);
                }
                if ((this.FormatFlags & DateTimeFormatFlags.UseGenitiveMonth) != DateTimeFormatFlags.None)
                {
                    for (int num3 = 1; num3 <= 13; num3++)
                    {
                        string str4 = this.internalGetMonthName(num3, MonthNameStyles.Genitive, false);
                        this.InsertHash(dtfiTokenHash, str4, TokenType.MonthToken, num3);
                    }
                }
                if ((this.FormatFlags & DateTimeFormatFlags.UseLeapYearMonth) != DateTimeFormatFlags.None)
                {
                    for (int num4 = 1; num4 <= 13; num4++)
                    {
                        string str5 = this.internalGetMonthName(num4, MonthNameStyles.LeapYear, false);
                        this.InsertHash(dtfiTokenHash, str5, TokenType.MonthToken, num4);
                    }
                }
                for (int j = 0; j < 7; j++)
                {
                    string dayName = this.GetDayName((DayOfWeek) j);
                    this.InsertHash(dtfiTokenHash, dayName, TokenType.DayOfWeekToken, j);
                    dayName = this.GetAbbreviatedDayName((DayOfWeek) j);
                    this.InsertHash(dtfiTokenHash, dayName, TokenType.DayOfWeekToken, j);
                }
                int[] eras = this.calendar.Eras;
                for (int k = 1; k <= eras.Length; k++)
                {
                    this.InsertHash(dtfiTokenHash, this.GetEraName(k), TokenType.EraToken, k);
                    this.InsertHash(dtfiTokenHash, this.GetAbbreviatedEraName(k), TokenType.EraToken, k);
                }
                if (this.LanguageName.Equals("ja"))
                {
                    for (int num7 = 0; num7 < 7; num7++)
                    {
                        string str7 = "(" + this.GetAbbreviatedDayName((DayOfWeek) num7) + ")";
                        this.InsertHash(dtfiTokenHash, str7, TokenType.DayOfWeekToken, num7);
                    }
                    if (this.Calendar.GetType() != typeof(JapaneseCalendar))
                    {
                        DateTimeFormatInfo japaneseCalendarDTFI = GetJapaneseCalendarDTFI();
                        for (int num8 = 1; num8 <= japaneseCalendarDTFI.Calendar.Eras.Length; num8++)
                        {
                            this.InsertHash(dtfiTokenHash, japaneseCalendarDTFI.GetEraName(num8), TokenType.JapaneseEraToken, num8);
                            this.InsertHash(dtfiTokenHash, japaneseCalendarDTFI.GetAbbreviatedEraName(num8), TokenType.JapaneseEraToken, num8);
                            this.InsertHash(dtfiTokenHash, japaneseCalendarDTFI.AbbreviatedEnglishEraNames[num8 - 1], TokenType.JapaneseEraToken, num8);
                        }
                    }
                }
                else if (this.CultureName.Equals("zh-TW"))
                {
                    DateTimeFormatInfo taiwanCalendarDTFI = GetTaiwanCalendarDTFI();
                    for (int num9 = 1; num9 <= taiwanCalendarDTFI.Calendar.Eras.Length; num9++)
                    {
                        if (taiwanCalendarDTFI.GetEraName(num9).Length > 0)
                        {
                            this.InsertHash(dtfiTokenHash, taiwanCalendarDTFI.GetEraName(num9), TokenType.TEraToken, num9);
                        }
                    }
                }
                this.InsertHash(dtfiTokenHash, InvariantInfo.AMDesignator, TokenType.SEP_Am | TokenType.Am, 0);
                this.InsertHash(dtfiTokenHash, InvariantInfo.PMDesignator, TokenType.SEP_Pm | TokenType.Pm, 1);
                for (int m = 1; m <= 12; m++)
                {
                    string monthName = InvariantInfo.GetMonthName(m);
                    this.InsertHash(dtfiTokenHash, monthName, TokenType.MonthToken, m);
                    monthName = InvariantInfo.GetAbbreviatedMonthName(m);
                    this.InsertHash(dtfiTokenHash, monthName, TokenType.MonthToken, m);
                }
                for (int n = 0; n < 7; n++)
                {
                    string abbreviatedDayName = InvariantInfo.GetDayName((DayOfWeek) n);
                    this.InsertHash(dtfiTokenHash, abbreviatedDayName, TokenType.DayOfWeekToken, n);
                    abbreviatedDayName = InvariantInfo.GetAbbreviatedDayName((DayOfWeek) n);
                    this.InsertHash(dtfiTokenHash, abbreviatedDayName, TokenType.DayOfWeekToken, n);
                }
                for (int num12 = 0; num12 < this.AbbreviatedEnglishEraNames.Length; num12++)
                {
                    this.InsertHash(dtfiTokenHash, this.AbbreviatedEnglishEraNames[num12], TokenType.EraToken, num12 + 1);
                }
                this.InsertHash(dtfiTokenHash, "T", TokenType.SEP_LocalTimeMark, 0);
                this.InsertHash(dtfiTokenHash, "GMT", TokenType.TimeZoneToken, 0);
                this.InsertHash(dtfiTokenHash, "Z", TokenType.TimeZoneToken, 0);
                this.InsertHash(dtfiTokenHash, "/", TokenType.SEP_Date, 0);
                this.InsertHash(dtfiTokenHash, ":", TokenType.SEP_Time, 0);
                this.m_dtfiTokenHash = dtfiTokenHash;
            }
            return dtfiTokenHash;
        }

        public string GetAbbreviatedDayName(DayOfWeek dayofweek)
        {
            if ((dayofweek < DayOfWeek.Sunday) || (dayofweek > DayOfWeek.Saturday))
            {
                throw new ArgumentOutOfRangeException("dayofweek", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { DayOfWeek.Sunday, DayOfWeek.Saturday }));
            }
            return this.internalGetAbbreviatedDayOfWeekNames()[(int) dayofweek];
        }

        [SecuritySafeCritical]
        public string GetAbbreviatedEraName(int era)
        {
            if (this.AbbreviatedEraNames.Length == 0)
            {
                return this.GetEraName(era);
            }
            if (era == 0)
            {
                era = this.Calendar.CurrentEraValue;
            }
            if ((--era >= this.m_abbrevEraNames.Length) || (era < 0))
            {
                throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("ArgumentOutOfRange_InvalidEraValue"));
            }
            return this.m_abbrevEraNames[era];
        }

        public string GetAbbreviatedMonthName(int month)
        {
            if ((month < 1) || (month > 13))
            {
                throw new ArgumentOutOfRangeException("month", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { 1, 13 }));
            }
            return this.internalGetAbbreviatedMonthNames()[month - 1];
        }

        public string[] GetAllDateTimePatterns()
        {
            List<string> list = new List<string>(0x84);
            for (int i = 0; i < DateTimeFormat.allStandardFormats.Length; i++)
            {
                string[] allDateTimePatterns = this.GetAllDateTimePatterns(DateTimeFormat.allStandardFormats[i]);
                for (int j = 0; j < allDateTimePatterns.Length; j++)
                {
                    list.Add(allDateTimePatterns[j]);
                }
            }
            return list.ToArray();
        }

        public string[] GetAllDateTimePatterns(char format)
        {
            switch (format)
            {
                case 'D':
                    return this.AllLongDatePatterns;

                case 'F':
                case 'U':
                    return GetCombinedPatterns(this.AllLongDatePatterns, this.AllLongTimePatterns, " ");

                case 'G':
                    return GetCombinedPatterns(this.AllShortDatePatterns, this.AllLongTimePatterns, " ");

                case 'M':
                case 'm':
                    return new string[] { this.MonthDayPattern };

                case 'O':
                case 'o':
                    return new string[] { "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK" };

                case 'R':
                case 'r':
                    return new string[] { "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'" };

                case 'T':
                    return this.AllLongTimePatterns;

                case 'd':
                    return this.AllShortDatePatterns;

                case 'f':
                    return GetCombinedPatterns(this.AllLongDatePatterns, this.AllShortTimePatterns, " ");

                case 'g':
                    return GetCombinedPatterns(this.AllShortDatePatterns, this.AllShortTimePatterns, " ");

                case 'Y':
                case 'y':
                    return this.AllYearMonthPatterns;

                case 's':
                    return new string[] { "yyyy'-'MM'-'dd'T'HH':'mm':'ss" };

                case 't':
                    return this.AllShortTimePatterns;

                case 'u':
                    return new string[] { this.UniversalSortableDateTimePattern };
            }
            throw new ArgumentException(Environment.GetResourceString("Format_BadFormatSpecifier"), "format");
        }

        private static string[] GetCombinedPatterns(string[] patterns1, string[] patterns2, string connectString)
        {
            string[] strArray = new string[patterns1.Length * patterns2.Length];
            int num = 0;
            for (int i = 0; i < patterns1.Length; i++)
            {
                for (int j = 0; j < patterns2.Length; j++)
                {
                    strArray[num++] = patterns1[i] + connectString + patterns2[j];
                }
            }
            return strArray;
        }

        public string GetDayName(DayOfWeek dayofweek)
        {
            if ((dayofweek < DayOfWeek.Sunday) || (dayofweek > DayOfWeek.Saturday))
            {
                throw new ArgumentOutOfRangeException("dayofweek", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { DayOfWeek.Sunday, DayOfWeek.Saturday }));
            }
            return this.internalGetDayOfWeekNames()[(int) dayofweek];
        }

        [SecuritySafeCritical]
        public int GetEra(string eraName)
        {
            if (eraName == null)
            {
                throw new ArgumentNullException("eraName", Environment.GetResourceString("ArgumentNull_String"));
            }
            if (eraName.Length != 0)
            {
                for (int i = 0; i < this.EraNames.Length; i++)
                {
                    if ((this.m_eraNames[i].Length > 0) && (string.Compare(eraName, this.m_eraNames[i], this.Culture, CompareOptions.IgnoreCase) == 0))
                    {
                        return (i + 1);
                    }
                }
                for (int j = 0; j < this.AbbreviatedEraNames.Length; j++)
                {
                    if (string.Compare(eraName, this.m_abbrevEraNames[j], this.Culture, CompareOptions.IgnoreCase) == 0)
                    {
                        return (j + 1);
                    }
                }
                for (int k = 0; k < this.AbbreviatedEnglishEraNames.Length; k++)
                {
                    if (string.Compare(eraName, this.m_abbrevEnglishEraNames[k], StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        return (k + 1);
                    }
                }
            }
            return -1;
        }

        public string GetEraName(int era)
        {
            if (era == 0)
            {
                era = this.Calendar.CurrentEraValue;
            }
            if ((--era >= this.EraNames.Length) || (era < 0))
            {
                throw new ArgumentOutOfRangeException("era", Environment.GetResourceString("ArgumentOutOfRange_InvalidEraValue"));
            }
            return this.m_eraNames[era];
        }

        public object GetFormat(Type formatType)
        {
            if (!(formatType == typeof(DateTimeFormatInfo)))
            {
                return null;
            }
            return this;
        }

        [SecuritySafeCritical]
        public static DateTimeFormatInfo GetInstance(IFormatProvider provider)
        {
            CultureInfo info2 = provider as CultureInfo;
            if ((info2 != null) && !info2.m_isInherited)
            {
                return info2.DateTimeFormat;
            }
            DateTimeFormatInfo format = provider as DateTimeFormatInfo;
            if (format != null)
            {
                return format;
            }
            if (provider != null)
            {
                format = provider.GetFormat(typeof(DateTimeFormatInfo)) as DateTimeFormatInfo;
                if (format != null)
                {
                    return format;
                }
            }
            return CurrentInfo;
        }

        internal static DateTimeFormatInfo GetJapaneseCalendarDTFI()
        {
            DateTimeFormatInfo dateTimeFormat = s_jajpDTFI;
            if (dateTimeFormat == null)
            {
                dateTimeFormat = new CultureInfo("ja-JP", false).DateTimeFormat;
                dateTimeFormat.Calendar = JapaneseCalendar.GetDefaultInstance();
                s_jajpDTFI = dateTimeFormat;
            }
            return dateTimeFormat;
        }

        private static string[] GetMergedPatterns(string[] patterns, string defaultPattern)
        {
            string[] strArray;
            if (defaultPattern == patterns[0])
            {
                return (string[]) patterns.Clone();
            }
            int index = 0;
            while (index < patterns.Length)
            {
                if (defaultPattern == patterns[index])
                {
                    break;
                }
                index++;
            }
            if (index < patterns.Length)
            {
                strArray = (string[]) patterns.Clone();
                strArray[index] = strArray[0];
            }
            else
            {
                strArray = new string[patterns.Length + 1];
                Array.Copy(patterns, 0, strArray, 1, patterns.Length);
            }
            strArray[0] = defaultPattern;
            return strArray;
        }

        public string GetMonthName(int month)
        {
            if ((month < 1) || (month > 13))
            {
                throw new ArgumentOutOfRangeException("month", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { 1, 13 }));
            }
            return this.internalGetMonthNames()[month - 1];
        }

        [ComVisible(false)]
        public string GetShortestDayName(DayOfWeek dayOfWeek)
        {
            if ((dayOfWeek < DayOfWeek.Sunday) || (dayOfWeek > DayOfWeek.Saturday))
            {
                throw new ArgumentOutOfRangeException("dayOfWeek", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { DayOfWeek.Sunday, DayOfWeek.Saturday }));
            }
            return this.internalGetSuperShortDayNames()[(int) dayOfWeek];
        }

        internal static DateTimeFormatInfo GetTaiwanCalendarDTFI()
        {
            DateTimeFormatInfo dateTimeFormat = s_zhtwDTFI;
            if (dateTimeFormat == null)
            {
                dateTimeFormat = new CultureInfo("zh-TW", false).DateTimeFormat;
                dateTimeFormat.Calendar = TaiwanCalendar.GetDefaultInstance();
                s_zhtwDTFI = dateTimeFormat;
            }
            return dateTimeFormat;
        }

        [SecuritySafeCritical]
        private void InitializeOverridableProperties(CultureData cultureData, int calendarID)
        {
            if (this.firstDayOfWeek == -1)
            {
                this.firstDayOfWeek = cultureData.IFIRSTDAYOFWEEK;
            }
            if (this.calendarWeekRule == -1)
            {
                this.calendarWeekRule = cultureData.IFIRSTWEEKOFYEAR;
            }
            if (this.amDesignator == null)
            {
                this.amDesignator = cultureData.SAM1159;
            }
            if (this.pmDesignator == null)
            {
                this.pmDesignator = cultureData.SPM2359;
            }
            if (this.timeSeparator == null)
            {
                this.timeSeparator = cultureData.TimeSeparator;
            }
            if (this.dateSeparator == null)
            {
                this.dateSeparator = cultureData.DateSeparator(calendarID);
            }
            this.allLongTimePatterns = this.m_cultureData.LongTimes;
            this.allShortTimePatterns = this.m_cultureData.ShortTimes;
            this.allLongDatePatterns = cultureData.LongDates(calendarID);
            this.allShortDatePatterns = cultureData.ShortDates(calendarID);
            this.allYearMonthPatterns = cultureData.YearMonths(calendarID);
        }

        private void InsertAtCurrentHashNode(TokenHashValue[] hashTable, string str, char ch, TokenType tokenType, int tokenValue, int pos, int hashcode, int hashProbe)
        {
            TokenHashValue value2 = hashTable[hashcode];
            hashTable[hashcode] = new TokenHashValue(str, tokenType, tokenValue);
            while (++pos < 0xc7)
            {
                hashcode += hashProbe;
                if (hashcode >= 0xc7)
                {
                    hashcode -= 0xc7;
                }
                TokenHashValue value3 = hashTable[hashcode];
                if ((value3 == null) || (char.ToLower(value3.tokenString[0], this.Culture) == ch))
                {
                    hashTable[hashcode] = value2;
                    if (value3 == null)
                    {
                        return;
                    }
                    value2 = value3;
                }
            }
        }

        private void InsertHash(TokenHashValue[] hashTable, string str, TokenType tokenType, int tokenValue)
        {
            TokenHashValue value2;
            if ((str == null) || (str.Length == 0))
            {
                return;
            }
            int pos = 0;
            if (char.IsWhiteSpace(str[0]) || char.IsWhiteSpace(str[str.Length - 1]))
            {
                str = str.Trim(null);
                if (str.Length == 0)
                {
                    return;
                }
            }
            char ch = char.ToLower(str[0], this.Culture);
            int index = ch % '\x00c7';
            int hashProbe = 1 + (ch % '\x00c5');
        Label_0069:
            value2 = hashTable[index];
            if (value2 == null)
            {
                hashTable[index] = new TokenHashValue(str, tokenType, tokenValue);
            }
            else
            {
                if ((str.Length >= value2.tokenString.Length) && (string.Compare(str, 0, value2.tokenString, 0, value2.tokenString.Length, this.Culture, CompareOptions.IgnoreCase) == 0))
                {
                    if (str.Length > value2.tokenString.Length)
                    {
                        this.InsertAtCurrentHashNode(hashTable, str, ch, tokenType, tokenValue, pos, index, hashProbe);
                        return;
                    }
                    int num4 = (int) tokenType;
                    int num5 = (int) value2.tokenType;
                    if ((((num4 | num5) & 0xff) == num4) || (((num4 | num5) & 0xff00) == num4))
                    {
                        value2.tokenType |= tokenType;
                        if (tokenValue != 0)
                        {
                            value2.tokenValue = tokenValue;
                        }
                    }
                }
                pos++;
                index += hashProbe;
                if (index >= 0xc7)
                {
                    index -= 0xc7;
                }
                if (pos < 0xc7)
                {
                    goto Label_0069;
                }
            }
        }

        [SecuritySafeCritical]
        private string[] internalGetAbbreviatedDayOfWeekNames()
        {
            if (this.abbreviatedDayNames == null)
            {
                this.abbreviatedDayNames = this.m_cultureData.AbbreviatedDayNames(this.Calendar.ID);
            }
            return this.abbreviatedDayNames;
        }

        [SecuritySafeCritical]
        private string[] internalGetAbbreviatedMonthNames()
        {
            if (this.abbreviatedMonthNames == null)
            {
                this.abbreviatedMonthNames = this.m_cultureData.AbbreviatedMonthNames(this.Calendar.ID);
            }
            return this.abbreviatedMonthNames;
        }

        [SecuritySafeCritical]
        private string[] internalGetDayOfWeekNames()
        {
            if (this.dayNames == null)
            {
                this.dayNames = this.m_cultureData.DayNames(this.Calendar.ID);
            }
            return this.dayNames;
        }

        private string[] internalGetGenitiveMonthNames(bool abbreviated)
        {
            if (abbreviated)
            {
                if (this.m_genitiveAbbreviatedMonthNames == null)
                {
                    this.m_genitiveAbbreviatedMonthNames = this.m_cultureData.AbbreviatedGenitiveMonthNames(this.Calendar.ID);
                }
                return this.m_genitiveAbbreviatedMonthNames;
            }
            if (this.genitiveMonthNames == null)
            {
                this.genitiveMonthNames = this.m_cultureData.GenitiveMonthNames(this.Calendar.ID);
            }
            return this.genitiveMonthNames;
        }

        internal string[] internalGetLeapYearMonthNames()
        {
            if (this.leapYearMonthNames == null)
            {
                this.leapYearMonthNames = this.m_cultureData.LeapYearMonthNames(this.Calendar.ID);
            }
            return this.leapYearMonthNames;
        }

        internal string internalGetMonthName(int month, MonthNameStyles style, bool abbreviated)
        {
            string[] strArray = null;
            switch (style)
            {
                case MonthNameStyles.Genitive:
                    strArray = this.internalGetGenitiveMonthNames(abbreviated);
                    break;

                case MonthNameStyles.LeapYear:
                    strArray = this.internalGetLeapYearMonthNames();
                    break;

                default:
                    strArray = abbreviated ? this.internalGetAbbreviatedMonthNames() : this.internalGetMonthNames();
                    break;
            }
            if ((month < 1) || (month > strArray.Length))
            {
                throw new ArgumentOutOfRangeException("month", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { 1, strArray.Length }));
            }
            return strArray[month - 1];
        }

        [SecuritySafeCritical]
        private string[] internalGetMonthNames()
        {
            if (this.monthNames == null)
            {
                this.monthNames = this.m_cultureData.MonthNames(this.Calendar.ID);
            }
            return this.monthNames;
        }

        [SecuritySafeCritical]
        private string[] internalGetSuperShortDayNames()
        {
            if (this.m_superShortDayNames == null)
            {
                this.m_superShortDayNames = this.m_cultureData.SuperShortDayNames(this.Calendar.ID);
            }
            return this.m_superShortDayNames;
        }

        private static bool IsHebrewChar(char ch)
        {
            return ((ch >= '֐') && (ch <= '׿'));
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            if (this.m_name != null)
            {
                this.m_cultureData = CultureData.GetCultureData(this.m_name, this.m_useUserOverride);
                if (this.m_cultureData == null)
                {
                    throw new CultureNotFoundException("m_name", this.m_name, Environment.GetResourceString("Argument_CultureNotSupported"));
                }
            }
            else
            {
                this.m_cultureData = CultureData.GetCultureData(this.CultureID, this.m_useUserOverride);
            }
            if (this.calendar == null)
            {
                this.calendar = (System.Globalization.Calendar) GregorianCalendar.GetDefaultInstance().Clone();
                this.calendar.SetReadOnlyState(this.m_isReadOnly);
            }
            else
            {
                CultureInfo.CheckDomainSafetyObject(this.calendar, this);
            }
            this.InitializeOverridableProperties(this.m_cultureData, this.calendar.ID);
            bool isReadOnly = this.m_isReadOnly;
            this.m_isReadOnly = false;
            if (this.longDatePattern != null)
            {
                this.LongDatePattern = this.longDatePattern;
            }
            if (this.shortDatePattern != null)
            {
                this.ShortDatePattern = this.shortDatePattern;
            }
            if (this.yearMonthPattern != null)
            {
                this.YearMonthPattern = this.yearMonthPattern;
            }
            if (this.longTimePattern != null)
            {
                this.LongTimePattern = this.longTimePattern;
            }
            if (this.shortTimePattern != null)
            {
                this.ShortTimePattern = this.shortTimePattern;
            }
            this.m_isReadOnly = isReadOnly;
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext ctx)
        {
            this.CultureID = this.m_cultureData.ILANGUAGE;
            this.m_useUserOverride = this.m_cultureData.UseUserOverride;
            this.m_name = this.CultureName;
            if (s_calendarNativeNames == null)
            {
                s_calendarNativeNames = new Hashtable();
            }
            string longTimePattern = this.LongTimePattern;
            string longDatePattern = this.LongDatePattern;
            string shortTimePattern = this.ShortTimePattern;
            string shortDatePattern = this.ShortDatePattern;
            string yearMonthPattern = this.YearMonthPattern;
            string[] allLongTimePatterns = this.AllLongTimePatterns;
            string[] allLongDatePatterns = this.AllLongDatePatterns;
            string[] allShortTimePatterns = this.AllShortTimePatterns;
            string[] allShortDatePatterns = this.AllShortDatePatterns;
            string[] allYearMonthPatterns = this.AllYearMonthPatterns;
        }

        [SecuritySafeCritical]
        public static DateTimeFormatInfo ReadOnly(DateTimeFormatInfo dtfi)
        {
            if (dtfi == null)
            {
                throw new ArgumentNullException("dtfi", Environment.GetResourceString("ArgumentNull_Obj"));
            }
            if (dtfi.IsReadOnly)
            {
                return dtfi;
            }
            DateTimeFormatInfo info = (DateTimeFormatInfo) dtfi.MemberwiseClone();
            info.calendar = System.Globalization.Calendar.ReadOnly(dtfi.Calendar);
            info.m_isReadOnly = true;
            return info;
        }

        [ComVisible(false)]
        public void SetAllDateTimePatterns(string[] patterns, char format)
        {
            if (this.IsReadOnly)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
            }
            if (patterns == null)
            {
                throw new ArgumentNullException("patterns", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if (patterns.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_ArrayZeroError"), "patterns");
            }
            for (int i = 0; i < patterns.Length; i++)
            {
                if (patterns[i] == null)
                {
                    throw new ArgumentNullException(Environment.GetResourceString("ArgumentNull_ArrayValue"));
                }
            }
            switch (format)
            {
                case 'd':
                    this.allShortDatePatterns = patterns;
                    this.shortDatePattern = this.allShortDatePatterns[0];
                    break;

                case 't':
                    this.allShortTimePatterns = patterns;
                    this.shortTimePattern = this.allShortTimePatterns[0];
                    break;

                case 'y':
                case 'Y':
                    this.allYearMonthPatterns = patterns;
                    this.yearMonthPattern = this.allYearMonthPatterns[0];
                    break;

                case 'D':
                    this.allLongDatePatterns = patterns;
                    this.longDatePattern = this.allLongDatePatterns[0];
                    break;

                case 'T':
                    this.allLongTimePatterns = patterns;
                    this.longTimePattern = this.allLongTimePatterns[0];
                    break;

                default:
                    throw new ArgumentException(Environment.GetResourceString("Format_BadFormatSpecifier"), "format");
            }
            this.ClearTokenHashTable();
        }

        [SecurityCritical]
        internal bool Tokenize(TokenType TokenMask, out TokenType tokenType, out int tokenValue, ref __DTString str)
        {
            tokenType = TokenType.UnknownToken;
            tokenValue = 0;
            char current = str.m_current;
            bool flag = char.IsLetter(current);
            if (flag)
            {
                bool flag2;
                current = char.ToLower(current, this.Culture);
                if ((IsHebrewChar(current) && (TokenMask == TokenType.RegularTokenMask)) && TryParseHebrewNumber(ref str, out flag2, out tokenValue))
                {
                    if (flag2)
                    {
                        tokenType = TokenType.UnknownToken;
                        return false;
                    }
                    tokenType = TokenType.HebrewNumber;
                    return true;
                }
            }
            int index = current % '\x00c7';
            int num2 = 1 + (current % '\x00c5');
            int num3 = str.len - str.Index;
            int num4 = 0;
            TokenHashValue[] dtfiTokenHash = this.m_dtfiTokenHash;
            if (dtfiTokenHash == null)
            {
                dtfiTokenHash = this.CreateTokenHashTable();
            }
            do
            {
                TokenHashValue value2 = dtfiTokenHash[index];
                if (value2 == null)
                {
                    break;
                }
                if (((value2.tokenType & TokenMask) > ((TokenType) 0)) && (value2.tokenString.Length <= num3))
                {
                    if (string.Compare(str.Value, str.Index, value2.tokenString, 0, value2.tokenString.Length, this.Culture, CompareOptions.IgnoreCase) == 0)
                    {
                        int num5;
                        if (flag && ((num5 = str.Index + value2.tokenString.Length) < str.len))
                        {
                            char c = str.Value[num5];
                            if (char.IsLetter(c))
                            {
                                return false;
                            }
                        }
                        tokenType = value2.tokenType & TokenMask;
                        tokenValue = value2.tokenValue;
                        str.Advance(value2.tokenString.Length);
                        return true;
                    }
                    if ((value2.tokenType == TokenType.MonthToken) && this.HasSpacesInMonthNames)
                    {
                        int matchLength = 0;
                        if (str.MatchSpecifiedWords(value2.tokenString, true, ref matchLength))
                        {
                            tokenType = value2.tokenType & TokenMask;
                            tokenValue = value2.tokenValue;
                            str.Advance(matchLength);
                            return true;
                        }
                    }
                    else if ((value2.tokenType == TokenType.DayOfWeekToken) && this.HasSpacesInDayNames)
                    {
                        int num7 = 0;
                        if (str.MatchSpecifiedWords(value2.tokenString, true, ref num7))
                        {
                            tokenType = value2.tokenType & TokenMask;
                            tokenValue = value2.tokenValue;
                            str.Advance(num7);
                            return true;
                        }
                    }
                }
                num4++;
                index += num2;
                if (index >= 0xc7)
                {
                    index -= 0xc7;
                }
            }
            while (num4 < 0xc7);
            return false;
        }

        private static bool TryParseHebrewNumber(ref __DTString str, out bool badFormat, out int number)
        {
            HebrewNumberParsingState state;
            number = -1;
            badFormat = false;
            int index = str.Index;
            if (!HebrewNumber.IsDigit(str.Value[index]))
            {
                return false;
            }
            HebrewNumberParsingContext context = new HebrewNumberParsingContext(0);
            do
            {
                state = HebrewNumber.ParseByChar(str.Value[index++], ref context);
                switch (state)
                {
                    case HebrewNumberParsingState.InvalidHebrewNumber:
                    case HebrewNumberParsingState.NotHebrewDigit:
                        return false;
                }
            }
            while ((index < str.Value.Length) && (state != HebrewNumberParsingState.FoundEndOfHebrewNumber));
            if (state != HebrewNumberParsingState.FoundEndOfHebrewNumber)
            {
                return false;
            }
            str.Advance(index - str.Index);
            number = context.result;
            return true;
        }

        internal static void ValidateStyles(DateTimeStyles style, string parameterName)
        {
            if ((style & ~(DateTimeStyles.RoundtripKind | DateTimeStyles.AssumeUniversal | DateTimeStyles.AssumeLocal | DateTimeStyles.AdjustToUniversal | DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AllowWhiteSpaces)) != DateTimeStyles.None)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidDateTimeStyles"), parameterName);
            }
            if (((style & DateTimeStyles.AssumeLocal) != DateTimeStyles.None) && ((style & DateTimeStyles.AssumeUniversal) != DateTimeStyles.None))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_ConflictingDateTimeStyles"), parameterName);
            }
            if (((style & DateTimeStyles.RoundtripKind) != DateTimeStyles.None) && ((style & (DateTimeStyles.AssumeUniversal | DateTimeStyles.AssumeLocal | DateTimeStyles.AdjustToUniversal)) != DateTimeStyles.None))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_ConflictingDateTimeRoundtripStyles"), parameterName);
            }
        }

        internal bool YearMonthAdjustment(ref int year, ref int month, bool parsedMonthName)
        {
            if ((this.FormatFlags & DateTimeFormatFlags.UseHebrewRule) != DateTimeFormatFlags.None)
            {
                if (year < 0x3e8)
                {
                    year += 0x1388;
                }
                if ((year < this.Calendar.GetYear(this.Calendar.MinSupportedDateTime)) || (year > this.Calendar.GetYear(this.Calendar.MaxSupportedDateTime)))
                {
                    return false;
                }
                if (parsedMonthName && !this.Calendar.IsLeapYear(year))
                {
                    if (month >= 8)
                    {
                        month--;
                    }
                    else if (month == 7)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public string[] AbbreviatedDayNames
        {
            get
            {
                return (string[]) this.internalGetAbbreviatedDayOfWeekNames().Clone();
            }
            set
            {
                if (this.IsReadOnly)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_Array"));
                }
                if (value.Length != 7)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidArrayLength", new object[] { 7 }), "value");
                }
                CheckNullValue(value, value.Length);
                this.ClearTokenHashTable();
                this.abbreviatedDayNames = value;
            }
        }

        internal string[] AbbreviatedEnglishEraNames
        {
            get
            {
                if (this.m_abbrevEnglishEraNames == null)
                {
                    this.m_abbrevEnglishEraNames = this.m_cultureData.AbbreviatedEnglishEraNames(this.Calendar.ID);
                }
                return this.m_abbrevEnglishEraNames;
            }
        }

        internal string[] AbbreviatedEraNames
        {
            get
            {
                if (this.m_abbrevEraNames == null)
                {
                    this.m_abbrevEraNames = this.m_cultureData.AbbrevEraNames(this.Calendar.ID);
                }
                return this.m_abbrevEraNames;
            }
        }

        [ComVisible(false)]
        public string[] AbbreviatedMonthGenitiveNames
        {
            [SecuritySafeCritical]
            get
            {
                return (string[]) this.internalGetGenitiveMonthNames(true).Clone();
            }
            set
            {
                if (this.IsReadOnly)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_Array"));
                }
                if (value.Length != 13)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidArrayLength", new object[] { 13 }), "value");
                }
                CheckNullValue(value, value.Length - 1);
                this.ClearTokenHashTable();
                this.m_genitiveAbbreviatedMonthNames = value;
            }
        }

        public string[] AbbreviatedMonthNames
        {
            get
            {
                return (string[]) this.internalGetAbbreviatedMonthNames().Clone();
            }
            set
            {
                if (this.IsReadOnly)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_Array"));
                }
                if (value.Length != 13)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidArrayLength", new object[] { 13 }), "value");
                }
                CheckNullValue(value, value.Length - 1);
                this.ClearTokenHashTable();
                this.abbreviatedMonthNames = value;
            }
        }

        private string[] AllLongDatePatterns
        {
            get
            {
                return GetMergedPatterns(this.UnclonedLongDatePatterns, this.LongDatePattern);
            }
        }

        private string[] AllLongTimePatterns
        {
            get
            {
                return GetMergedPatterns(this.UnclonedLongTimePatterns, this.LongTimePattern);
            }
        }

        private string[] AllShortDatePatterns
        {
            get
            {
                return GetMergedPatterns(this.UnclonedShortDatePatterns, this.ShortDatePattern);
            }
        }

        private string[] AllShortTimePatterns
        {
            get
            {
                return GetMergedPatterns(this.UnclonedShortTimePatterns, this.ShortTimePattern);
            }
        }

        private string[] AllYearMonthPatterns
        {
            get
            {
                return GetMergedPatterns(this.UnclonedYearMonthPatterns, this.YearMonthPattern);
            }
        }

        public string AMDesignator
        {
            [SecuritySafeCritical]
            get
            {
                return this.amDesignator;
            }
            set
            {
                if (this.IsReadOnly)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_String"));
                }
                this.ClearTokenHashTable();
                this.amDesignator = value;
            }
        }

        public System.Globalization.Calendar Calendar
        {
            get
            {
                return this.calendar;
            }
            [SecuritySafeCritical]
            set
            {
                if (this.IsReadOnly)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_Obj"));
                }
                if (value != this.calendar)
                {
                    CultureInfo.CheckDomainSafetyObject(value, this);
                    for (int i = 0; i < this.OptionalCalendars.Length; i++)
                    {
                        if (this.OptionalCalendars[i] == value.ID)
                        {
                            if (this.calendar == null)
                            {
                                this.calendar = value;
                                return;
                            }
                            this.m_eraNames = null;
                            this.m_abbrevEraNames = null;
                            this.m_abbrevEnglishEraNames = null;
                            this.monthDayPattern = null;
                            this.dayNames = null;
                            this.abbreviatedDayNames = null;
                            this.m_superShortDayNames = null;
                            this.monthNames = null;
                            this.abbreviatedMonthNames = null;
                            this.genitiveMonthNames = null;
                            this.m_genitiveAbbreviatedMonthNames = null;
                            this.leapYearMonthNames = null;
                            this.formatFlags = ~DateTimeFormatFlags.None;
                            this.allShortDatePatterns = null;
                            this.allLongDatePatterns = null;
                            this.allYearMonthPatterns = null;
                            this.dateTimeOffsetPattern = null;
                            this.longDatePattern = null;
                            this.shortDatePattern = null;
                            this.yearMonthPattern = null;
                            this.fullDateTimePattern = null;
                            this.generalShortTimePattern = null;
                            this.generalLongTimePattern = null;
                            this.dateSeparator = null;
                            this.ClearTokenHashTable();
                            this.calendar = value;
                            this.InitializeOverridableProperties(this.m_cultureData, this.calendar.ID);
                            return;
                        }
                    }
                    throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("Argument_InvalidCalendar"));
                }
            }
        }

        public System.Globalization.CalendarWeekRule CalendarWeekRule
        {
            [SecuritySafeCritical]
            get
            {
                return (System.Globalization.CalendarWeekRule) this.calendarWeekRule;
            }
            set
            {
                if (this.IsReadOnly)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
                }
                if ((value < System.Globalization.CalendarWeekRule.FirstDay) || (value > System.Globalization.CalendarWeekRule.FirstFourDayWeek))
                {
                    throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { System.Globalization.CalendarWeekRule.FirstDay, System.Globalization.CalendarWeekRule.FirstFourDayWeek }));
                }
                this.calendarWeekRule = (int) value;
            }
        }

        internal System.Globalization.CompareInfo CompareInfo
        {
            get
            {
                if (this.m_compareInfo == null)
                {
                    this.m_compareInfo = System.Globalization.CompareInfo.GetCompareInfo(this.m_cultureData.SCOMPAREINFO);
                }
                return this.m_compareInfo;
            }
        }

        private CultureInfo Culture
        {
            get
            {
                if (this.m_cultureInfo == null)
                {
                    this.m_cultureInfo = CultureInfo.GetCultureInfo(this.CultureName);
                }
                return this.m_cultureInfo;
            }
        }

        private string CultureName
        {
            get
            {
                if (this.m_name == null)
                {
                    this.m_name = this.m_cultureData.CultureName;
                }
                return this.m_name;
            }
        }

        public static DateTimeFormatInfo CurrentInfo
        {
            [SecuritySafeCritical]
            get
            {
                CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
                if (!currentCulture.m_isInherited)
                {
                    DateTimeFormatInfo dateTimeInfo = currentCulture.dateTimeInfo;
                    if (dateTimeInfo != null)
                    {
                        return dateTimeInfo;
                    }
                }
                return (DateTimeFormatInfo) currentCulture.GetFormat(typeof(DateTimeFormatInfo));
            }
        }

        public string DateSeparator
        {
            get
            {
                return this.dateSeparator;
            }
            set
            {
                if (this.IsReadOnly)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_String"));
                }
                this.ClearTokenHashTable();
                this.dateSeparator = value;
            }
        }

        internal string DateTimeOffsetPattern
        {
            get
            {
                if (this.dateTimeOffsetPattern == null)
                {
                    this.dateTimeOffsetPattern = this.ShortDatePattern + " " + this.LongTimePattern;
                    bool flag = false;
                    bool flag2 = false;
                    char ch = '\'';
                    for (int i = 0; !flag && (i < this.LongTimePattern.Length); i++)
                    {
                        switch (this.LongTimePattern[i])
                        {
                            case '\\':
                            case '%':
                                i++;
                                break;

                            case 'z':
                                flag = !flag2;
                                break;

                            case '\'':
                            case '"':
                                if (flag2 && (ch == this.LongTimePattern[i]))
                                {
                                    flag2 = false;
                                }
                                else if (!flag2)
                                {
                                    ch = this.LongTimePattern[i];
                                    flag2 = true;
                                }
                                break;
                        }
                    }
                    if (!flag)
                    {
                        this.dateTimeOffsetPattern = this.dateTimeOffsetPattern + " zzz";
                    }
                }
                return this.dateTimeOffsetPattern;
            }
        }

        public string[] DayNames
        {
            get
            {
                return (string[]) this.internalGetDayOfWeekNames().Clone();
            }
            set
            {
                if (this.IsReadOnly)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_Array"));
                }
                if (value.Length != 7)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidArrayLength", new object[] { 7 }), "value");
                }
                CheckNullValue(value, value.Length);
                this.ClearTokenHashTable();
                this.dayNames = value;
            }
        }

        internal string[] EraNames
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_eraNames == null)
                {
                    this.m_eraNames = this.m_cultureData.EraNames(this.Calendar.ID);
                }
                return this.m_eraNames;
            }
        }

        public DayOfWeek FirstDayOfWeek
        {
            [SecuritySafeCritical]
            get
            {
                return (DayOfWeek) this.firstDayOfWeek;
            }
            set
            {
                if (this.IsReadOnly)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
                }
                if ((value < DayOfWeek.Sunday) || (value > DayOfWeek.Saturday))
                {
                    throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { DayOfWeek.Sunday, DayOfWeek.Saturday }));
                }
                this.firstDayOfWeek = (int) value;
            }
        }

        internal DateTimeFormatFlags FormatFlags
        {
            get
            {
                if (this.formatFlags == ~DateTimeFormatFlags.None)
                {
                    this.formatFlags = DateTimeFormatFlags.None;
                    this.formatFlags |= (DateTimeFormatFlags) ((int) DateTimeFormatInfoScanner.GetFormatFlagGenitiveMonth(this.MonthNames, this.internalGetGenitiveMonthNames(false), this.AbbreviatedMonthNames, this.internalGetGenitiveMonthNames(true)));
                    this.formatFlags |= (DateTimeFormatFlags) ((int) DateTimeFormatInfoScanner.GetFormatFlagUseSpaceInMonthNames(this.MonthNames, this.internalGetGenitiveMonthNames(false), this.AbbreviatedMonthNames, this.internalGetGenitiveMonthNames(true)));
                    this.formatFlags |= (DateTimeFormatFlags) ((int) DateTimeFormatInfoScanner.GetFormatFlagUseSpaceInDayNames(this.DayNames, this.AbbreviatedDayNames));
                    this.formatFlags |= (DateTimeFormatFlags) ((int) DateTimeFormatInfoScanner.GetFormatFlagUseHebrewCalendar(this.Calendar.ID));
                }
                return this.formatFlags;
            }
        }

        public string FullDateTimePattern
        {
            get
            {
                if (this.fullDateTimePattern == null)
                {
                    this.fullDateTimePattern = this.LongDatePattern + " " + this.LongTimePattern;
                }
                return this.fullDateTimePattern;
            }
            set
            {
                if (this.IsReadOnly)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_String"));
                }
                this.fullDateTimePattern = value;
            }
        }

        internal string FullTimeSpanNegativePattern
        {
            get
            {
                if (this.m_fullTimeSpanNegativePattern == null)
                {
                    this.m_fullTimeSpanNegativePattern = "'-'" + this.FullTimeSpanPositivePattern;
                }
                return this.m_fullTimeSpanNegativePattern;
            }
        }

        internal string FullTimeSpanPositivePattern
        {
            get
            {
                if (this.m_fullTimeSpanPositivePattern == null)
                {
                    CultureData cultureData;
                    if (this.m_cultureData.UseUserOverride)
                    {
                        cultureData = CultureData.GetCultureData(this.m_cultureData.CultureName, false);
                    }
                    else
                    {
                        cultureData = this.m_cultureData;
                    }
                    string numberDecimalSeparator = new NumberFormatInfo(cultureData).NumberDecimalSeparator;
                    this.m_fullTimeSpanPositivePattern = "d':'h':'mm':'ss'" + numberDecimalSeparator + "'FFFFFFF";
                }
                return this.m_fullTimeSpanPositivePattern;
            }
        }

        internal string GeneralLongTimePattern
        {
            get
            {
                if (this.generalLongTimePattern == null)
                {
                    this.generalLongTimePattern = this.ShortDatePattern + " " + this.LongTimePattern;
                }
                return this.generalLongTimePattern;
            }
        }

        internal string GeneralShortTimePattern
        {
            get
            {
                if (this.generalShortTimePattern == null)
                {
                    this.generalShortTimePattern = this.ShortDatePattern + " " + this.ShortTimePattern;
                }
                return this.generalShortTimePattern;
            }
        }

        internal bool HasForceTwoDigitYears
        {
            get
            {
                switch (this.calendar.ID)
                {
                    case 3:
                    case 4:
                        return true;
                }
                return false;
            }
        }

        internal bool HasSpacesInDayNames
        {
            get
            {
                return ((this.FormatFlags & DateTimeFormatFlags.UseSpacesInDayNames) != DateTimeFormatFlags.None);
            }
        }

        internal bool HasSpacesInMonthNames
        {
            get
            {
                return ((this.FormatFlags & DateTimeFormatFlags.UseSpacesInMonthNames) != DateTimeFormatFlags.None);
            }
        }

        internal bool HasYearMonthAdjustment
        {
            get
            {
                return ((this.FormatFlags & DateTimeFormatFlags.UseHebrewRule) != DateTimeFormatFlags.None);
            }
        }

        public static DateTimeFormatInfo InvariantInfo
        {
            get
            {
                if (invariantInfo == null)
                {
                    DateTimeFormatInfo info = new DateTimeFormatInfo();
                    info.Calendar.SetReadOnlyState(true);
                    info.m_isReadOnly = true;
                    invariantInfo = info;
                }
                return invariantInfo;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this.m_isReadOnly;
            }
        }

        private string LanguageName
        {
            [SecurityCritical]
            get
            {
                if (this.m_langName == null)
                {
                    this.m_langName = this.m_cultureData.SISO639LANGNAME;
                }
                return this.m_langName;
            }
        }

        public string LongDatePattern
        {
            [SecuritySafeCritical]
            get
            {
                if (this.longDatePattern == null)
                {
                    this.longDatePattern = this.UnclonedLongDatePatterns[0];
                }
                return this.longDatePattern;
            }
            [SecuritySafeCritical]
            set
            {
                if (this.IsReadOnly)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_String"));
                }
                this.longDatePattern = value;
                this.ClearTokenHashTable();
                this.fullDateTimePattern = null;
            }
        }

        public string LongTimePattern
        {
            [SecuritySafeCritical]
            get
            {
                if (this.longTimePattern == null)
                {
                    this.longTimePattern = this.UnclonedLongTimePatterns[0];
                }
                return this.longTimePattern;
            }
            [SecuritySafeCritical]
            set
            {
                if (this.IsReadOnly)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_String"));
                }
                this.longTimePattern = value;
                this.ClearTokenHashTable();
                this.fullDateTimePattern = null;
                this.generalLongTimePattern = null;
                this.dateTimeOffsetPattern = null;
            }
        }

        public string MonthDayPattern
        {
            [SecuritySafeCritical]
            get
            {
                if (this.monthDayPattern == null)
                {
                    this.monthDayPattern = this.m_cultureData.MonthDay(this.Calendar.ID);
                }
                return this.monthDayPattern;
            }
            set
            {
                if (this.IsReadOnly)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_String"));
                }
                this.monthDayPattern = value;
            }
        }

        [ComVisible(false)]
        public string[] MonthGenitiveNames
        {
            [SecuritySafeCritical]
            get
            {
                return (string[]) this.internalGetGenitiveMonthNames(false).Clone();
            }
            set
            {
                if (this.IsReadOnly)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_Array"));
                }
                if (value.Length != 13)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidArrayLength", new object[] { 13 }), "value");
                }
                CheckNullValue(value, value.Length - 1);
                this.genitiveMonthNames = value;
                this.ClearTokenHashTable();
            }
        }

        public string[] MonthNames
        {
            get
            {
                return (string[]) this.internalGetMonthNames().Clone();
            }
            set
            {
                if (this.IsReadOnly)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_Array"));
                }
                if (value.Length != 13)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidArrayLength", new object[] { 13 }), "value");
                }
                CheckNullValue(value, value.Length - 1);
                this.monthNames = value;
                this.ClearTokenHashTable();
            }
        }

        [ComVisible(false)]
        public string NativeCalendarName
        {
            get
            {
                return this.m_cultureData.CalendarName(this.Calendar.ID);
            }
        }

        private int[] OptionalCalendars
        {
            get
            {
                if (this.optionalCalendars == null)
                {
                    this.optionalCalendars = this.m_cultureData.CalendarIds;
                }
                return this.optionalCalendars;
            }
        }

        public string PMDesignator
        {
            [SecuritySafeCritical]
            get
            {
                return this.pmDesignator;
            }
            set
            {
                if (this.IsReadOnly)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_String"));
                }
                this.ClearTokenHashTable();
                this.pmDesignator = value;
            }
        }

        public string RFC1123Pattern
        {
            get
            {
                return "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'";
            }
        }

        public string ShortDatePattern
        {
            get
            {
                if (this.shortDatePattern == null)
                {
                    this.shortDatePattern = this.UnclonedShortDatePatterns[0];
                }
                return this.shortDatePattern;
            }
            set
            {
                if (this.IsReadOnly)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_String"));
                }
                this.shortDatePattern = value;
                this.ClearTokenHashTable();
                this.generalLongTimePattern = null;
                this.generalShortTimePattern = null;
                this.dateTimeOffsetPattern = null;
            }
        }

        [ComVisible(false)]
        public string[] ShortestDayNames
        {
            get
            {
                return (string[]) this.internalGetSuperShortDayNames().Clone();
            }
            set
            {
                if (this.IsReadOnly)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_Array"));
                }
                if (value.Length != 7)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidArrayLength", new object[] { 7 }), "value");
                }
                CheckNullValue(value, value.Length);
                this.m_superShortDayNames = value;
            }
        }

        public string ShortTimePattern
        {
            [SecuritySafeCritical]
            get
            {
                if (this.shortTimePattern == null)
                {
                    this.shortTimePattern = this.UnclonedShortTimePatterns[0];
                }
                return this.shortTimePattern;
            }
            [SecuritySafeCritical]
            set
            {
                if (this.IsReadOnly)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_String"));
                }
                this.shortTimePattern = value;
                this.ClearTokenHashTable();
                this.generalShortTimePattern = null;
            }
        }

        public string SortableDateTimePattern
        {
            get
            {
                return "yyyy'-'MM'-'dd'T'HH':'mm':'ss";
            }
        }

        public string TimeSeparator
        {
            get
            {
                return this.timeSeparator;
            }
            set
            {
                if (this.IsReadOnly)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_String"));
                }
                this.ClearTokenHashTable();
                this.timeSeparator = value;
            }
        }

        private string[] UnclonedLongDatePatterns
        {
            [SecuritySafeCritical]
            get
            {
                if (this.allLongDatePatterns == null)
                {
                    this.allLongDatePatterns = this.m_cultureData.LongDates(this.Calendar.ID);
                }
                return this.allLongDatePatterns;
            }
        }

        private string[] UnclonedLongTimePatterns
        {
            [SecuritySafeCritical]
            get
            {
                if (this.allLongTimePatterns == null)
                {
                    this.allLongTimePatterns = this.m_cultureData.LongTimes;
                }
                return this.allLongTimePatterns;
            }
        }

        private string[] UnclonedShortDatePatterns
        {
            [SecuritySafeCritical]
            get
            {
                if (this.allShortDatePatterns == null)
                {
                    this.allShortDatePatterns = this.m_cultureData.ShortDates(this.Calendar.ID);
                }
                return this.allShortDatePatterns;
            }
        }

        private string[] UnclonedShortTimePatterns
        {
            [SecuritySafeCritical]
            get
            {
                if (this.allShortTimePatterns == null)
                {
                    this.allShortTimePatterns = this.m_cultureData.ShortTimes;
                }
                return this.allShortTimePatterns;
            }
        }

        private string[] UnclonedYearMonthPatterns
        {
            [SecuritySafeCritical]
            get
            {
                if (this.allYearMonthPatterns == null)
                {
                    this.allYearMonthPatterns = this.m_cultureData.YearMonths(this.Calendar.ID);
                }
                return this.allYearMonthPatterns;
            }
        }

        public string UniversalSortableDateTimePattern
        {
            get
            {
                return "yyyy'-'MM'-'dd HH':'mm':'ss'Z'";
            }
        }

        public string YearMonthPattern
        {
            [SecuritySafeCritical]
            get
            {
                if (this.yearMonthPattern == null)
                {
                    this.yearMonthPattern = this.UnclonedYearMonthPatterns[0];
                }
                return this.yearMonthPattern;
            }
            [SecuritySafeCritical]
            set
            {
                if (this.IsReadOnly)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ReadOnly"));
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value", Environment.GetResourceString("ArgumentNull_String"));
                }
                this.yearMonthPattern = value;
                this.ClearTokenHashTable();
            }
        }
    }
}

