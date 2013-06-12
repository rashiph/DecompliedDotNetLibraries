namespace System
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    internal static class DateTimeParse
    {
        private static DS[][] dateParsingStates;
        internal const string GMTName = "GMT";
        internal static MatchNumberDelegate m_hebrewNumberParser = new MatchNumberDelegate(DateTimeParse.MatchHebrewDigits);
        internal const int MaxDateTimeNumberDigits = 8;
        private const int ORDER_DM = 7;
        private const int ORDER_DMY = 2;
        private const int ORDER_MD = 6;
        private const int ORDER_MDY = 1;
        private const int ORDER_MY = 5;
        private const int ORDER_YDM = 3;
        private const int ORDER_YM = 4;
        private const int ORDER_YMD = 0;
        internal const string ZuluName = "Z";

        static DateTimeParse()
        {
            DS[][] dsArray = new DS[20][];
            DS[] dsArray2 = new DS[0x12];
            dsArray2[1] = DS.ERROR;
            dsArray2[2] = DS.TX_N;
            dsArray2[3] = DS.N;
            dsArray2[4] = DS.D_Nd;
            dsArray2[5] = DS.T_Nt;
            dsArray2[6] = DS.ERROR;
            dsArray2[7] = DS.D_M;
            dsArray2[8] = DS.D_M;
            dsArray2[9] = DS.D_S;
            dsArray2[10] = DS.T_S;
            dsArray2[12] = DS.D_Y;
            dsArray2[13] = DS.D_Y;
            dsArray2[14] = DS.ERROR;
            dsArray2[0x11] = DS.ERROR;
            dsArray[0] = dsArray2;
            dsArray[1] = new DS[] { 
                DS.ERROR, DS.DX_NN, DS.ERROR, DS.NN, DS.D_NNd, DS.ERROR, DS.DX_NM, DS.D_NM, DS.D_MNd, DS.D_NDS, DS.ERROR, DS.N, DS.D_YN, DS.D_YNd, DS.DX_YN, DS.N, 
                DS.N, DS.ERROR
             };
            dsArray[2] = new DS[] { 
                DS.DX_NN, DS.DX_NNN, DS.TX_N, DS.DX_NNN, DS.ERROR, DS.T_Nt, DS.DX_MNN, DS.DX_MNN, DS.ERROR, DS.ERROR, DS.T_S, DS.NN, DS.DX_NNY, DS.ERROR, DS.DX_NNY, DS.NN, 
                DS.NN, DS.ERROR
             };
            dsArray[3] = new DS[] { 
                DS.ERROR, DS.DX_NN, DS.ERROR, DS.D_NN, DS.D_NNd, DS.ERROR, DS.DX_NM, DS.D_MN, DS.D_MNd, DS.ERROR, DS.ERROR, DS.D_Nd, DS.D_YN, DS.D_YNd, DS.DX_YN, DS.ERROR, 
                DS.D_Nd, DS.ERROR
             };
            dsArray[4] = new DS[] { 
                DS.DX_NN, DS.DX_NNN, DS.TX_N, DS.DX_NNN, DS.ERROR, DS.T_Nt, DS.DX_MNN, DS.DX_MNN, DS.ERROR, DS.DX_DS, DS.T_S, DS.D_NN, DS.DX_NNY, DS.ERROR, DS.DX_NNY, DS.ERROR, 
                DS.D_NN, DS.ERROR
             };
            dsArray[5] = new DS[] { 
                DS.ERROR, DS.DX_NNN, DS.DX_NNN, DS.DX_NNN, DS.ERROR, DS.ERROR, DS.DX_MNN, DS.DX_MNN, DS.ERROR, DS.DX_DS, DS.ERROR, DS.D_NNd, DS.DX_NNY, DS.ERROR, DS.DX_NNY, DS.ERROR, 
                DS.D_NNd, DS.ERROR
             };
            dsArray[6] = new DS[] { 
                DS.ERROR, DS.DX_MN, DS.ERROR, DS.D_MN, DS.D_MNd, DS.ERROR, DS.ERROR, DS.ERROR, DS.ERROR, DS.ERROR, DS.ERROR, DS.D_M, DS.D_YM, DS.D_YMd, DS.DX_YM, DS.ERROR, 
                DS.D_M, DS.ERROR
             };
            dsArray[7] = new DS[] { 
                DS.DX_MN, DS.DX_MNN, DS.DX_MNN, DS.DX_MNN, DS.ERROR, DS.T_Nt, DS.ERROR, DS.ERROR, DS.ERROR, DS.DX_DS, DS.T_S, DS.D_MN, DS.DX_YMN, DS.ERROR, DS.DX_YMN, DS.ERROR, 
                DS.D_MN, DS.ERROR
             };
            dsArray[8] = new DS[] { 
                DS.DX_NM, DS.DX_MNN, DS.DX_MNN, DS.DX_MNN, DS.ERROR, DS.T_Nt, DS.ERROR, DS.ERROR, DS.ERROR, DS.DX_DS, DS.T_S, DS.D_NM, DS.DX_YMN, DS.ERROR, DS.DX_YMN, DS.ERROR, 
                DS.D_NM, DS.ERROR
             };
            dsArray[9] = new DS[] { 
                DS.ERROR, DS.DX_MNN, DS.ERROR, DS.DX_MNN, DS.ERROR, DS.ERROR, DS.ERROR, DS.ERROR, DS.ERROR, DS.ERROR, DS.ERROR, DS.D_MNd, DS.DX_YMN, DS.ERROR, DS.DX_YMN, DS.ERROR, 
                DS.D_MNd, DS.ERROR
             };
            dsArray[10] = new DS[] { 
                DS.DX_NDS, DS.DX_NNDS, DS.DX_NNDS, DS.DX_NNDS, DS.ERROR, DS.T_Nt, DS.ERROR, DS.ERROR, DS.ERROR, DS.D_NDS, DS.T_S, DS.D_NDS, DS.ERROR, DS.ERROR, DS.ERROR, DS.ERROR, 
                DS.D_NDS, DS.ERROR
             };
            dsArray[11] = new DS[] { 
                DS.ERROR, DS.DX_YN, DS.ERROR, DS.D_YN, DS.D_YNd, DS.ERROR, DS.DX_YM, DS.D_YM, DS.D_YMd, DS.D_YM, DS.ERROR, DS.D_Y, DS.ERROR, DS.ERROR, DS.ERROR, DS.ERROR, 
                DS.D_Y, DS.ERROR
             };
            dsArray[12] = new DS[] { 
                DS.DX_YN, DS.DX_YNN, DS.DX_YNN, DS.DX_YNN, DS.ERROR, DS.ERROR, DS.DX_YMN, DS.DX_YMN, DS.ERROR, DS.ERROR, DS.ERROR, DS.D_YN, DS.ERROR, DS.ERROR, DS.ERROR, DS.ERROR, 
                DS.D_YN, DS.ERROR
             };
            dsArray[13] = new DS[] { 
                DS.ERROR, DS.DX_YNN, DS.DX_YNN, DS.DX_YNN, DS.ERROR, DS.ERROR, DS.DX_YMN, DS.DX_YMN, DS.ERROR, DS.ERROR, DS.ERROR, DS.D_YN, DS.ERROR, DS.ERROR, DS.ERROR, DS.ERROR, 
                DS.D_YN, DS.ERROR
             };
            dsArray[14] = new DS[] { 
                DS.DX_YM, DS.DX_YMN, DS.DX_YMN, DS.DX_YMN, DS.ERROR, DS.ERROR, DS.ERROR, DS.ERROR, DS.ERROR, DS.ERROR, DS.ERROR, DS.D_YM, DS.ERROR, DS.ERROR, DS.ERROR, DS.ERROR, 
                DS.D_YM, DS.ERROR
             };
            dsArray[15] = new DS[] { 
                DS.ERROR, DS.DX_YMN, DS.DX_YMN, DS.DX_YMN, DS.ERROR, DS.ERROR, DS.ERROR, DS.ERROR, DS.ERROR, DS.ERROR, DS.ERROR, DS.D_YM, DS.ERROR, DS.ERROR, DS.ERROR, DS.ERROR, 
                DS.D_YM, DS.ERROR
             };
            dsArray[0x10] = new DS[] { 
                DS.DX_DS, DS.DX_DSN, DS.TX_N, DS.T_Nt, DS.ERROR, DS.T_Nt, DS.ERROR, DS.ERROR, DS.ERROR, DS.D_S, DS.T_S, DS.D_S, DS.ERROR, DS.ERROR, DS.ERROR, DS.ERROR, 
                DS.D_S, DS.ERROR
             };
            dsArray[0x11] = new DS[] { 
                DS.TX_TS, DS.TX_TS, DS.TX_TS, DS.T_Nt, DS.D_Nd, DS.ERROR, DS.ERROR, DS.ERROR, DS.ERROR, DS.D_S, DS.T_S, DS.T_S, DS.ERROR, DS.ERROR, DS.ERROR, DS.T_S, 
                DS.T_S, DS.ERROR
             };
            dsArray[0x12] = new DS[] { 
                DS.ERROR, DS.TX_NN, DS.TX_NN, DS.TX_NN, DS.ERROR, DS.T_NNt, DS.DX_NM, DS.D_NM, DS.ERROR, DS.ERROR, DS.T_S, DS.ERROR, DS.ERROR, DS.ERROR, DS.ERROR, DS.T_Nt, 
                DS.T_Nt, DS.TX_NN
             };
            dsArray[0x13] = new DS[] { 
                DS.ERROR, DS.TX_NNN, DS.TX_NNN, DS.TX_NNN, DS.ERROR, DS.ERROR, DS.ERROR, DS.ERROR, DS.ERROR, DS.ERROR, DS.T_S, DS.T_NNt, DS.ERROR, DS.ERROR, DS.ERROR, DS.T_NNt, 
                DS.T_NNt, DS.TX_NNN
             };
            dateParsingStates = dsArray;
        }

        private static bool AdjustHour(ref int hour, TM timeMark)
        {
            if (timeMark != TM.NotSet)
            {
                if (timeMark == TM.AM)
                {
                    if ((hour < 0) || (hour > 12))
                    {
                        return false;
                    }
                    hour = (hour == 12) ? 0 : hour;
                }
                else
                {
                    if ((hour < 0) || (hour > 0x17))
                    {
                        return false;
                    }
                    if (hour < 12)
                    {
                        hour += 12;
                    }
                }
            }
            return true;
        }

        private static void AdjustTimeMark(DateTimeFormatInfo dtfi, ref DateTimeRawInfo raw)
        {
            if (((raw.timeMark == TM.NotSet) && (dtfi.AMDesignator != null)) && (dtfi.PMDesignator != null))
            {
                if ((dtfi.AMDesignator.Length == 0) && (dtfi.PMDesignator.Length != 0))
                {
                    raw.timeMark = TM.AM;
                }
                if ((dtfi.PMDesignator.Length == 0) && (dtfi.AMDesignator.Length != 0))
                {
                    raw.timeMark = TM.PM;
                }
            }
        }

        private static bool AdjustTimeZoneToLocal(ref DateTimeResult result, bool bTimeOnly)
        {
            long ticks = result.parsedDate.Ticks;
            TimeZoneInfo local = TimeZoneInfo.Local;
            bool isAmbiguousLocalDst = false;
            if (ticks < 0xc92a69c000L)
            {
                ticks -= result.timeZoneOffset.Ticks;
                ticks += local.GetUtcOffset(bTimeOnly ? DateTime.Now : result.parsedDate, TimeZoneInfoOptions.NoThrowOnInvalidTime).Ticks;
                if (ticks < 0L)
                {
                    ticks += 0xc92a69c000L;
                }
            }
            else
            {
                ticks -= result.timeZoneOffset.Ticks;
                if ((ticks < 0L) || (ticks > 0x2bca2875f4373fffL))
                {
                    ticks += local.GetUtcOffset(result.parsedDate, TimeZoneInfoOptions.NoThrowOnInvalidTime).Ticks;
                }
                else
                {
                    DateTime time = new DateTime(ticks, DateTimeKind.Utc);
                    bool isDaylightSavings = false;
                    ticks += TimeZoneInfo.GetUtcOffsetFromUtc(time, TimeZoneInfo.Local, out isDaylightSavings, out isAmbiguousLocalDst).Ticks;
                }
            }
            if ((ticks < 0L) || (ticks > 0x2bca2875f4373fffL))
            {
                result.parsedDate = DateTime.MinValue;
                result.SetFailure(System.ParseFailureKind.Format, "Format_DateOutOfRange", null);
                return false;
            }
            result.parsedDate = new DateTime(ticks, DateTimeKind.Local, isAmbiguousLocalDst);
            return true;
        }

        private static bool AdjustTimeZoneToUniversal(ref DateTimeResult result)
        {
            long ticks = result.parsedDate.Ticks - result.timeZoneOffset.Ticks;
            if (ticks < 0L)
            {
                ticks += 0xc92a69c000L;
            }
            if ((ticks < 0L) || (ticks > 0x2bca2875f4373fffL))
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_DateOutOfRange", null);
                return false;
            }
            result.parsedDate = new DateTime(ticks, DateTimeKind.Utc);
            return true;
        }

        private static int AdjustYear(ref DateTimeResult result, int year)
        {
            if (year < 100)
            {
                year = result.calendar.ToFourDigitYear(year);
            }
            return year;
        }

        private static bool CheckDefaultDateTime(ref DateTimeResult result, ref Calendar cal, DateTimeStyles styles)
        {
            if (((((result.flags & ParseFlags.CaptureOffset) != 0) && ((result.Month != -1) || (result.Day != -1))) && ((result.Year == -1) || ((result.flags & ParseFlags.YearDefault) != 0))) && ((result.flags & ParseFlags.TimeZoneUsed) != 0))
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_MissingIncompleteDate", null);
                return false;
            }
            if (((result.Year == -1) || (result.Month == -1)) || (result.Day == -1))
            {
                DateTime dateTimeNow = GetDateTimeNow(ref result, ref styles);
                if ((result.Month == -1) && (result.Day == -1))
                {
                    if (result.Year == -1)
                    {
                        if ((styles & DateTimeStyles.NoCurrentDateDefault) != DateTimeStyles.None)
                        {
                            cal = GregorianCalendar.GetDefaultInstance();
                            result.Year = result.Month = result.Day = 1;
                        }
                        else
                        {
                            result.Year = cal.GetYear(dateTimeNow);
                            result.Month = cal.GetMonth(dateTimeNow);
                            result.Day = cal.GetDayOfMonth(dateTimeNow);
                        }
                    }
                    else
                    {
                        result.Month = 1;
                        result.Day = 1;
                    }
                }
                else
                {
                    if (result.Year == -1)
                    {
                        result.Year = cal.GetYear(dateTimeNow);
                    }
                    if (result.Month == -1)
                    {
                        result.Month = 1;
                    }
                    if (result.Day == -1)
                    {
                        result.Day = 1;
                    }
                }
            }
            if (result.Hour == -1)
            {
                result.Hour = 0;
            }
            if (result.Minute == -1)
            {
                result.Minute = 0;
            }
            if (result.Second == -1)
            {
                result.Second = 0;
            }
            if (result.era == -1)
            {
                result.era = 0;
            }
            return true;
        }

        private static bool CheckNewValue(ref int currentValue, int newValue, char patternChar, ref DateTimeResult result)
        {
            if (currentValue == -1)
            {
                currentValue = newValue;
                return true;
            }
            if (newValue != currentValue)
            {
                result.SetFailure(System.ParseFailureKind.FormatWithParameter, "Format_RepeatDateTimePattern", patternChar);
                return false;
            }
            return true;
        }

        private static bool DateTimeOffsetTimeZonePostProcessing(ref DateTimeResult result, DateTimeStyles styles)
        {
            if ((result.flags & ParseFlags.TimeZoneUsed) == 0)
            {
                if ((styles & DateTimeStyles.AssumeUniversal) != DateTimeStyles.None)
                {
                    result.timeZoneOffset = TimeSpan.Zero;
                }
                else
                {
                    result.timeZoneOffset = TimeZoneInfo.Local.GetUtcOffset(result.parsedDate, TimeZoneInfoOptions.NoThrowOnInvalidTime);
                }
            }
            long ticks = result.timeZoneOffset.Ticks;
            long num2 = result.parsedDate.Ticks - ticks;
            if ((num2 < 0L) || (num2 > 0x2bca2875f4373fffL))
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_UTCOutOfRange", null);
                return false;
            }
            if ((ticks < -504000000000L) || (ticks > 0x7558bdb000L))
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_OffsetOutOfRange", null);
                return false;
            }
            if ((styles & DateTimeStyles.AdjustToUniversal) != DateTimeStyles.None)
            {
                if (((result.flags & ParseFlags.TimeZoneUsed) == 0) && ((styles & DateTimeStyles.AssumeUniversal) == DateTimeStyles.None))
                {
                    bool flag = AdjustTimeZoneToUniversal(ref result);
                    result.timeZoneOffset = TimeSpan.Zero;
                    return flag;
                }
                result.parsedDate = new DateTime(num2, DateTimeKind.Utc);
                result.timeZoneOffset = TimeSpan.Zero;
            }
            return true;
        }

        private static bool DetermineTimeZoneAdjustments(ref DateTimeResult result, DateTimeStyles styles, bool bTimeOnly)
        {
            if ((result.flags & ParseFlags.CaptureOffset) != 0)
            {
                return DateTimeOffsetTimeZonePostProcessing(ref result, styles);
            }
            if ((result.flags & ParseFlags.TimeZoneUsed) == 0)
            {
                if ((styles & DateTimeStyles.AssumeLocal) != DateTimeStyles.None)
                {
                    if ((styles & DateTimeStyles.AdjustToUniversal) == DateTimeStyles.None)
                    {
                        result.parsedDate = DateTime.SpecifyKind(result.parsedDate, DateTimeKind.Local);
                        return true;
                    }
                    result.flags |= ParseFlags.TimeZoneUsed;
                    result.timeZoneOffset = TimeZoneInfo.Local.GetUtcOffset(result.parsedDate, TimeZoneInfoOptions.NoThrowOnInvalidTime);
                }
                else
                {
                    if ((styles & DateTimeStyles.AssumeUniversal) == DateTimeStyles.None)
                    {
                        return true;
                    }
                    if ((styles & DateTimeStyles.AdjustToUniversal) != DateTimeStyles.None)
                    {
                        result.parsedDate = DateTime.SpecifyKind(result.parsedDate, DateTimeKind.Utc);
                        return true;
                    }
                    result.flags |= ParseFlags.TimeZoneUsed;
                    result.timeZoneOffset = TimeSpan.Zero;
                }
            }
            if (((styles & DateTimeStyles.RoundtripKind) != DateTimeStyles.None) && ((result.flags & ParseFlags.TimeZoneUtc) != 0))
            {
                result.parsedDate = DateTime.SpecifyKind(result.parsedDate, DateTimeKind.Utc);
                return true;
            }
            if ((styles & DateTimeStyles.AdjustToUniversal) != DateTimeStyles.None)
            {
                return AdjustTimeZoneToUniversal(ref result);
            }
            return AdjustTimeZoneToLocal(ref result, bTimeOnly);
        }

        [SecuritySafeCritical]
        private static bool DoStrictParse(string s, string formatParam, DateTimeStyles styles, DateTimeFormatInfo dtfi, ref DateTimeResult result)
        {
            ParsingInfo parseInfo = new ParsingInfo();
            parseInfo.Init();
            parseInfo.calendar = dtfi.Calendar;
            parseInfo.fAllowInnerWhite = (styles & DateTimeStyles.AllowInnerWhite) != DateTimeStyles.None;
            parseInfo.fAllowTrailingWhite = (styles & DateTimeStyles.AllowTrailingWhite) != DateTimeStyles.None;
            if (formatParam.Length == 1)
            {
                if (((result.flags & ParseFlags.CaptureOffset) != 0) && (formatParam[0] == 'U'))
                {
                    result.SetFailure(System.ParseFailureKind.Format, "Format_BadFormatSpecifier", null);
                    return false;
                }
                formatParam = ExpandPredefinedFormat(formatParam, ref dtfi, ref parseInfo, ref result);
            }
            bool bTimeOnly = false;
            result.calendar = parseInfo.calendar;
            if (parseInfo.calendar.ID == 8)
            {
                parseInfo.parseNumberDelegate = m_hebrewNumberParser;
                parseInfo.fCustomNumberParser = true;
            }
            result.Hour = result.Minute = result.Second = -1;
            __DTString format = new __DTString(formatParam, dtfi, false);
            __DTString str = new __DTString(s, dtfi, false);
            if (parseInfo.fAllowTrailingWhite)
            {
                format.TrimTail();
                format.RemoveTrailingInQuoteSpaces();
                str.TrimTail();
            }
            if ((styles & DateTimeStyles.AllowLeadingWhite) != DateTimeStyles.None)
            {
                format.SkipWhiteSpaces();
                format.RemoveLeadingInQuoteSpaces();
                str.SkipWhiteSpaces();
            }
            while (format.GetNext())
            {
                if (parseInfo.fAllowInnerWhite)
                {
                    str.SkipWhiteSpaces();
                }
                if (!ParseByFormat(ref str, ref format, ref parseInfo, dtfi, ref result))
                {
                    return false;
                }
            }
            if (str.Index < (str.Value.Length - 1))
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                return false;
            }
            if (parseInfo.fUseTwoDigitYear && ((dtfi.FormatFlags & DateTimeFormatFlags.UseHebrewRule) == DateTimeFormatFlags.None))
            {
                if (result.Year >= 100)
                {
                    result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                    return false;
                }
                result.Year = parseInfo.calendar.ToFourDigitYear(result.Year);
            }
            if (parseInfo.fUseHour12)
            {
                if (parseInfo.timeMark == TM.NotSet)
                {
                    parseInfo.timeMark = TM.AM;
                }
                if (result.Hour > 12)
                {
                    result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                    return false;
                }
                if (parseInfo.timeMark == TM.AM)
                {
                    if (result.Hour == 12)
                    {
                        result.Hour = 0;
                    }
                }
                else
                {
                    result.Hour = (result.Hour == 12) ? 12 : (result.Hour + 12);
                }
            }
            else if (((parseInfo.timeMark == TM.AM) && (result.Hour >= 12)) || ((parseInfo.timeMark == TM.PM) && (result.Hour < 12)))
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                return false;
            }
            bTimeOnly = ((result.Year == -1) && (result.Month == -1)) && (result.Day == -1);
            if (!CheckDefaultDateTime(ref result, ref parseInfo.calendar, styles))
            {
                return false;
            }
            if ((!bTimeOnly && dtfi.HasYearMonthAdjustment) && !dtfi.YearMonthAdjustment(ref result.Year, ref result.Month, (result.flags & ParseFlags.ParsedMonthName) != 0))
            {
                result.SetFailure(System.ParseFailureKind.FormatBadDateTimeCalendar, "Format_BadDateTimeCalendar", null);
                return false;
            }
            if (!parseInfo.calendar.TryToDateTime(result.Year, result.Month, result.Day, result.Hour, result.Minute, result.Second, 0, result.era, out result.parsedDate))
            {
                result.SetFailure(System.ParseFailureKind.FormatBadDateTimeCalendar, "Format_BadDateTimeCalendar", null);
                return false;
            }
            if (result.fraction > 0.0)
            {
                result.parsedDate = result.parsedDate.AddTicks((long) Math.Round((double) (result.fraction * 10000000.0)));
            }
            if ((parseInfo.dayOfWeek != -1) && (parseInfo.dayOfWeek != parseInfo.calendar.GetDayOfWeek(result.parsedDate)))
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDayOfWeek", null);
                return false;
            }
            if (!DetermineTimeZoneAdjustments(ref result, styles, bTimeOnly))
            {
                return false;
            }
            return true;
        }

        [Conditional("_LOGGING")]
        internal static void DTFITrace(DateTimeFormatInfo dtfi)
        {
        }

        private static string ExpandPredefinedFormat(string format, ref DateTimeFormatInfo dtfi, ref ParsingInfo parseInfo, ref DateTimeResult result)
        {
            switch (format[0])
            {
                case 'o':
                case 'O':
                    parseInfo.calendar = GregorianCalendar.GetDefaultInstance();
                    dtfi = DateTimeFormatInfo.InvariantInfo;
                    break;

                case 'r':
                case 'R':
                    parseInfo.calendar = GregorianCalendar.GetDefaultInstance();
                    dtfi = DateTimeFormatInfo.InvariantInfo;
                    if ((result.flags & ParseFlags.CaptureOffset) != 0)
                    {
                        result.flags |= ParseFlags.Rfc1123Pattern;
                    }
                    break;

                case 's':
                    dtfi = DateTimeFormatInfo.InvariantInfo;
                    parseInfo.calendar = GregorianCalendar.GetDefaultInstance();
                    break;

                case 'u':
                    parseInfo.calendar = GregorianCalendar.GetDefaultInstance();
                    dtfi = DateTimeFormatInfo.InvariantInfo;
                    if ((result.flags & ParseFlags.CaptureOffset) != 0)
                    {
                        result.flags |= ParseFlags.UtcSortPattern;
                    }
                    break;

                case 'U':
                    parseInfo.calendar = GregorianCalendar.GetDefaultInstance();
                    result.flags |= ParseFlags.TimeZoneUsed;
                    result.timeZoneOffset = new TimeSpan(0L);
                    result.flags |= ParseFlags.TimeZoneUtc;
                    if (dtfi.Calendar.GetType() != typeof(GregorianCalendar))
                    {
                        dtfi = (DateTimeFormatInfo) dtfi.Clone();
                        dtfi.Calendar = GregorianCalendar.GetDefaultInstance();
                    }
                    break;
            }
            return DateTimeFormat.GetRealFormat(format, dtfi);
        }

        private static bool GetDateOfDSN(ref DateTimeResult result, ref DateTimeRawInfo raw)
        {
            if ((raw.numCount != 1) || (result.Day != -1))
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                return false;
            }
            result.Day = raw.GetNumber(0);
            return true;
        }

        private static bool GetDateOfNDS(ref DateTimeResult result, ref DateTimeRawInfo raw)
        {
            if (result.Month == -1)
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                return false;
            }
            if (result.Year != -1)
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                return false;
            }
            result.Year = AdjustYear(ref result, raw.GetNumber(0));
            result.Day = 1;
            return true;
        }

        private static bool GetDateOfNNDS(ref DateTimeResult result, ref DateTimeRawInfo raw, DateTimeFormatInfo dtfi)
        {
            if ((result.flags & ParseFlags.HaveYear) != 0)
            {
                if ((((result.flags & ParseFlags.HaveMonth) == 0) && ((result.flags & ParseFlags.HaveDay) == 0)) && SetDateYMD(ref result, result.Year = AdjustYear(ref result, raw.year), raw.GetNumber(0), raw.GetNumber(1)))
                {
                    return true;
                }
            }
            else if ((((result.flags & ParseFlags.HaveMonth) != 0) && ((result.flags & ParseFlags.HaveYear) == 0)) && ((result.flags & ParseFlags.HaveDay) == 0))
            {
                int num;
                if (!GetYearMonthDayOrder(dtfi.ShortDatePattern, dtfi, out num))
                {
                    result.SetFailure(System.ParseFailureKind.FormatWithParameter, "Format_BadDatePattern", dtfi.ShortDatePattern);
                    return false;
                }
                if (num == 0)
                {
                    if (SetDateYMD(ref result, AdjustYear(ref result, raw.GetNumber(0)), result.Month, raw.GetNumber(1)))
                    {
                        return true;
                    }
                }
                else if (SetDateYMD(ref result, AdjustYear(ref result, raw.GetNumber(1)), result.Month, raw.GetNumber(0)))
                {
                    return true;
                }
            }
            result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
            return false;
        }

        private static DateTime GetDateTimeNow(ref DateTimeResult result, ref DateTimeStyles styles)
        {
            if ((result.flags & ParseFlags.CaptureOffset) != 0)
            {
                if ((result.flags & ParseFlags.TimeZoneUsed) != 0)
                {
                    return new DateTime(DateTime.UtcNow.Ticks + result.timeZoneOffset.Ticks, DateTimeKind.Unspecified);
                }
                if ((styles & DateTimeStyles.AssumeUniversal) != DateTimeStyles.None)
                {
                    return DateTime.UtcNow;
                }
            }
            return DateTime.Now;
        }

        private static Exception GetDateTimeParseException(ref DateTimeResult result)
        {
            switch (result.failure)
            {
                case System.ParseFailureKind.ArgumentNull:
                    return new ArgumentNullException(result.failureArgumentName, Environment.GetResourceString(result.failureMessageID));

                case System.ParseFailureKind.Format:
                    return new FormatException(Environment.GetResourceString(result.failureMessageID));

                case System.ParseFailureKind.FormatWithParameter:
                    return new FormatException(Environment.GetResourceString(result.failureMessageID, new object[] { result.failureMessageFormatArgument }));

                case System.ParseFailureKind.FormatBadDateTimeCalendar:
                    return new FormatException(Environment.GetResourceString(result.failureMessageID, new object[] { result.calendar }));
            }
            return null;
        }

        private static bool GetDayOfMN(ref DateTimeResult result, ref DateTimeStyles styles, ref DateTimeRawInfo raw, DateTimeFormatInfo dtfi)
        {
            int num;
            if ((result.flags & ParseFlags.HaveDate) != 0)
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                return false;
            }
            if (!GetMonthDayOrder(dtfi.MonthDayPattern, dtfi, out num))
            {
                result.SetFailure(System.ParseFailureKind.FormatWithParameter, "Format_BadDatePattern", dtfi.MonthDayPattern);
                return false;
            }
            if (num == 7)
            {
                int num2;
                if (!GetYearMonthOrder(dtfi.YearMonthPattern, dtfi, out num2))
                {
                    result.SetFailure(System.ParseFailureKind.FormatWithParameter, "Format_BadDatePattern", dtfi.YearMonthPattern);
                    return false;
                }
                if (num2 == 5)
                {
                    if (!SetDateYMD(ref result, AdjustYear(ref result, raw.GetNumber(0)), raw.month, 1))
                    {
                        result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                        return false;
                    }
                    return true;
                }
            }
            GetDefaultYear(ref result, ref styles);
            if (!SetDateYMD(ref result, result.Year, raw.month, raw.GetNumber(0)))
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                return false;
            }
            return true;
        }

        private static bool GetDayOfMNN(ref DateTimeResult result, ref DateTimeRawInfo raw, DateTimeFormatInfo dtfi)
        {
            int num3;
            int num4;
            if ((result.flags & ParseFlags.HaveDate) != 0)
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                return false;
            }
            int number = raw.GetNumber(0);
            int year = raw.GetNumber(1);
            if (!GetYearMonthDayOrder(dtfi.ShortDatePattern, dtfi, out num3))
            {
                result.SetFailure(System.ParseFailureKind.FormatWithParameter, "Format_BadDatePattern", dtfi.ShortDatePattern);
                return false;
            }
            if (num3 == 1)
            {
                if (result.calendar.IsValidDay(num4 = AdjustYear(ref result, year), raw.month, number, result.era))
                {
                    result.SetDate(num4, raw.month, number);
                    result.flags |= ParseFlags.HaveDate;
                    return true;
                }
                if (result.calendar.IsValidDay(num4 = AdjustYear(ref result, number), raw.month, year, result.era))
                {
                    result.SetDate(num4, raw.month, year);
                    result.flags |= ParseFlags.HaveDate;
                    return true;
                }
            }
            else if (num3 == 0)
            {
                if (result.calendar.IsValidDay(num4 = AdjustYear(ref result, number), raw.month, year, result.era))
                {
                    result.SetDate(num4, raw.month, year);
                    result.flags |= ParseFlags.HaveDate;
                    return true;
                }
                if (result.calendar.IsValidDay(num4 = AdjustYear(ref result, year), raw.month, number, result.era))
                {
                    result.SetDate(num4, raw.month, number);
                    result.flags |= ParseFlags.HaveDate;
                    return true;
                }
            }
            else if (num3 == 2)
            {
                if (result.calendar.IsValidDay(num4 = AdjustYear(ref result, year), raw.month, number, result.era))
                {
                    result.SetDate(num4, raw.month, number);
                    result.flags |= ParseFlags.HaveDate;
                    return true;
                }
                if (result.calendar.IsValidDay(num4 = AdjustYear(ref result, number), raw.month, year, result.era))
                {
                    result.SetDate(num4, raw.month, year);
                    result.flags |= ParseFlags.HaveDate;
                    return true;
                }
            }
            result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
            return false;
        }

        private static bool GetDayOfNM(ref DateTimeResult result, ref DateTimeStyles styles, ref DateTimeRawInfo raw, DateTimeFormatInfo dtfi)
        {
            int num;
            if ((result.flags & ParseFlags.HaveDate) != 0)
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                return false;
            }
            if (!GetMonthDayOrder(dtfi.MonthDayPattern, dtfi, out num))
            {
                result.SetFailure(System.ParseFailureKind.FormatWithParameter, "Format_BadDatePattern", dtfi.MonthDayPattern);
                return false;
            }
            if (num == 6)
            {
                int num2;
                if (!GetYearMonthOrder(dtfi.YearMonthPattern, dtfi, out num2))
                {
                    result.SetFailure(System.ParseFailureKind.FormatWithParameter, "Format_BadDatePattern", dtfi.YearMonthPattern);
                    return false;
                }
                if (num2 == 4)
                {
                    if (!SetDateYMD(ref result, AdjustYear(ref result, raw.GetNumber(0)), raw.month, 1))
                    {
                        result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                        return false;
                    }
                    return true;
                }
            }
            GetDefaultYear(ref result, ref styles);
            if (!SetDateYMD(ref result, result.Year, raw.month, raw.GetNumber(0)))
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                return false;
            }
            return true;
        }

        private static bool GetDayOfNN(ref DateTimeResult result, ref DateTimeStyles styles, ref DateTimeRawInfo raw, DateTimeFormatInfo dtfi)
        {
            int num3;
            if ((result.flags & ParseFlags.HaveDate) != 0)
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                return false;
            }
            int number = raw.GetNumber(0);
            int day = raw.GetNumber(1);
            GetDefaultYear(ref result, ref styles);
            if (!GetMonthDayOrder(dtfi.MonthDayPattern, dtfi, out num3))
            {
                result.SetFailure(System.ParseFailureKind.FormatWithParameter, "Format_BadDatePattern", dtfi.MonthDayPattern);
                return false;
            }
            if (num3 == 6)
            {
                if (SetDateYMD(ref result, result.Year, number, day))
                {
                    result.flags |= ParseFlags.HaveDate;
                    return true;
                }
            }
            else if (SetDateYMD(ref result, result.Year, day, number))
            {
                result.flags |= ParseFlags.HaveDate;
                return true;
            }
            result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
            return false;
        }

        private static bool GetDayOfNNN(ref DateTimeResult result, ref DateTimeRawInfo raw, DateTimeFormatInfo dtfi)
        {
            int num4;
            if ((result.flags & ParseFlags.HaveDate) != 0)
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                return false;
            }
            int number = raw.GetNumber(0);
            int month = raw.GetNumber(1);
            int day = raw.GetNumber(2);
            if (!GetYearMonthDayOrder(dtfi.ShortDatePattern, dtfi, out num4))
            {
                result.SetFailure(System.ParseFailureKind.FormatWithParameter, "Format_BadDatePattern", dtfi.ShortDatePattern);
                return false;
            }
            if (num4 == 0)
            {
                if (SetDateYMD(ref result, AdjustYear(ref result, number), month, day))
                {
                    result.flags |= ParseFlags.HaveDate;
                    return true;
                }
            }
            else if (num4 == 1)
            {
                if (SetDateMDY(ref result, number, month, AdjustYear(ref result, day)))
                {
                    result.flags |= ParseFlags.HaveDate;
                    return true;
                }
            }
            else if (num4 == 2)
            {
                if (SetDateDMY(ref result, number, month, AdjustYear(ref result, day)))
                {
                    result.flags |= ParseFlags.HaveDate;
                    return true;
                }
            }
            else if ((num4 == 3) && SetDateYDM(ref result, AdjustYear(ref result, number), month, day))
            {
                result.flags |= ParseFlags.HaveDate;
                return true;
            }
            result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
            return false;
        }

        private static bool GetDayOfNNY(ref DateTimeResult result, ref DateTimeRawInfo raw, DateTimeFormatInfo dtfi)
        {
            int num3;
            if ((result.flags & ParseFlags.HaveDate) != 0)
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                return false;
            }
            int number = raw.GetNumber(0);
            int day = raw.GetNumber(1);
            if (!GetYearMonthDayOrder(dtfi.ShortDatePattern, dtfi, out num3))
            {
                result.SetFailure(System.ParseFailureKind.FormatWithParameter, "Format_BadDatePattern", dtfi.ShortDatePattern);
                return false;
            }
            if ((num3 == 1) || (num3 == 0))
            {
                if (SetDateYMD(ref result, raw.year, number, day))
                {
                    result.flags |= ParseFlags.HaveDate;
                    return true;
                }
            }
            else if (SetDateYMD(ref result, raw.year, day, number))
            {
                result.flags |= ParseFlags.HaveDate;
                return true;
            }
            result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
            return false;
        }

        private static bool GetDayOfYM(ref DateTimeResult result, ref DateTimeRawInfo raw, DateTimeFormatInfo dtfi)
        {
            if ((result.flags & ParseFlags.HaveDate) != 0)
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                return false;
            }
            if (SetDateYMD(ref result, raw.year, raw.month, 1))
            {
                result.flags |= ParseFlags.HaveDate;
                return true;
            }
            result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
            return false;
        }

        private static bool GetDayOfYMN(ref DateTimeResult result, ref DateTimeRawInfo raw, DateTimeFormatInfo dtfi)
        {
            if ((result.flags & ParseFlags.HaveDate) != 0)
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                return false;
            }
            if (SetDateYMD(ref result, raw.year, raw.month, raw.GetNumber(0)))
            {
                result.flags |= ParseFlags.HaveDate;
                return true;
            }
            result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
            return false;
        }

        private static bool GetDayOfYN(ref DateTimeResult result, ref DateTimeRawInfo raw, DateTimeFormatInfo dtfi)
        {
            if ((result.flags & ParseFlags.HaveDate) != 0)
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                return false;
            }
            if (SetDateYMD(ref result, raw.year, raw.GetNumber(0), 1))
            {
                result.flags |= ParseFlags.HaveDate;
                return true;
            }
            result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
            return false;
        }

        private static bool GetDayOfYNN(ref DateTimeResult result, ref DateTimeRawInfo raw, DateTimeFormatInfo dtfi)
        {
            int num3;
            if ((result.flags & ParseFlags.HaveDate) != 0)
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                return false;
            }
            int number = raw.GetNumber(0);
            int month = raw.GetNumber(1);
            if (GetYearMonthDayOrder(dtfi.ShortDatePattern, dtfi, out num3) && (num3 == 3))
            {
                if (SetDateYMD(ref result, raw.year, month, number))
                {
                    result.flags |= ParseFlags.HaveDate;
                    return true;
                }
            }
            else if (SetDateYMD(ref result, raw.year, number, month))
            {
                result.flags |= ParseFlags.HaveDate;
                return true;
            }
            result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
            return false;
        }

        private static void GetDefaultYear(ref DateTimeResult result, ref DateTimeStyles styles)
        {
            result.Year = result.calendar.GetYear(GetDateTimeNow(ref result, ref styles));
            result.flags |= ParseFlags.YearDefault;
        }

        private static bool GetHebrewDayOfNM(ref DateTimeResult result, ref DateTimeRawInfo raw, DateTimeFormatInfo dtfi)
        {
            int num;
            if (!GetMonthDayOrder(dtfi.MonthDayPattern, dtfi, out num))
            {
                result.SetFailure(System.ParseFailureKind.FormatWithParameter, "Format_BadDatePattern", dtfi.MonthDayPattern);
                return false;
            }
            result.Month = raw.month;
            if (((num == 7) || (num == 6)) && result.calendar.IsValidDay(result.Year, result.Month, raw.GetNumber(0), result.era))
            {
                result.Day = raw.GetNumber(0);
                return true;
            }
            result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
            return false;
        }

        private static bool GetMonthDayOrder(string pattern, DateTimeFormatInfo dtfi, out int order)
        {
            int num = -1;
            int num2 = -1;
            int num3 = 0;
            bool flag = false;
            for (int i = 0; (i < pattern.Length) && (num3 < 2); i++)
            {
                char ch = pattern[i];
                switch (ch)
                {
                    case '\\':
                    case '%':
                    {
                        i++;
                        continue;
                    }
                    case '\'':
                    case '"':
                        flag = !flag;
                        break;
                }
                if (!flag)
                {
                    switch (ch)
                    {
                        case 'd':
                        {
                            int num5 = 1;
                            while (((i + 1) < pattern.Length) && (pattern[i + 1] == 'd'))
                            {
                                num5++;
                                i++;
                            }
                            if (num5 <= 2)
                            {
                                num2 = num3++;
                            }
                            break;
                        }
                        case 'M':
                            num = num3++;
                            while (((i + 1) < pattern.Length) && (pattern[i + 1] == 'M'))
                            {
                                i++;
                            }
                            break;
                    }
                }
            }
            if ((num == 0) && (num2 == 1))
            {
                order = 6;
                return true;
            }
            if ((num2 == 0) && (num == 1))
            {
                order = 7;
                return true;
            }
            order = -1;
            return false;
        }

        private static bool GetTimeOfN(DateTimeFormatInfo dtfi, ref DateTimeResult result, ref DateTimeRawInfo raw)
        {
            if ((result.flags & ParseFlags.HaveTime) != 0)
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                return false;
            }
            if (raw.timeMark == TM.NotSet)
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                return false;
            }
            result.Hour = raw.GetNumber(0);
            result.flags |= ParseFlags.HaveTime;
            return true;
        }

        private static bool GetTimeOfNN(DateTimeFormatInfo dtfi, ref DateTimeResult result, ref DateTimeRawInfo raw)
        {
            if ((result.flags & ParseFlags.HaveTime) != 0)
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                return false;
            }
            result.Hour = raw.GetNumber(0);
            result.Minute = raw.GetNumber(1);
            result.flags |= ParseFlags.HaveTime;
            return true;
        }

        private static bool GetTimeOfNNN(DateTimeFormatInfo dtfi, ref DateTimeResult result, ref DateTimeRawInfo raw)
        {
            if ((result.flags & ParseFlags.HaveTime) != 0)
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                return false;
            }
            result.Hour = raw.GetNumber(0);
            result.Minute = raw.GetNumber(1);
            result.Second = raw.GetNumber(2);
            result.flags |= ParseFlags.HaveTime;
            return true;
        }

        private static bool GetTimeZoneName(ref __DTString str)
        {
            return (MatchWord(ref str, "GMT") || MatchWord(ref str, "Z"));
        }

        private static bool GetYearMonthDayOrder(string datePattern, DateTimeFormatInfo dtfi, out int order)
        {
            int num = -1;
            int num2 = -1;
            int num3 = -1;
            int num4 = 0;
            bool flag = false;
            for (int i = 0; (i < datePattern.Length) && (num4 < 3); i++)
            {
                char ch = datePattern[i];
                switch (ch)
                {
                    case '\\':
                    case '%':
                    {
                        i++;
                        continue;
                    }
                    case '\'':
                    case '"':
                        flag = !flag;
                        break;
                }
                if (!flag)
                {
                    switch (ch)
                    {
                        case 'y':
                            num = num4++;
                            while (((i + 1) < datePattern.Length) && (datePattern[i + 1] == 'y'))
                            {
                                i++;
                            }
                            break;

                        case 'M':
                            num2 = num4++;
                            while (((i + 1) < datePattern.Length) && (datePattern[i + 1] == 'M'))
                            {
                                i++;
                            }
                            break;

                        case 'd':
                        {
                            int num6 = 1;
                            while (((i + 1) < datePattern.Length) && (datePattern[i + 1] == 'd'))
                            {
                                num6++;
                                i++;
                            }
                            if (num6 <= 2)
                            {
                                num3 = num4++;
                            }
                            break;
                        }
                    }
                }
            }
            if (((num == 0) && (num2 == 1)) && (num3 == 2))
            {
                order = 0;
                return true;
            }
            if (((num2 == 0) && (num3 == 1)) && (num == 2))
            {
                order = 1;
                return true;
            }
            if (((num3 == 0) && (num2 == 1)) && (num == 2))
            {
                order = 2;
                return true;
            }
            if (((num == 0) && (num3 == 1)) && (num2 == 2))
            {
                order = 3;
                return true;
            }
            order = -1;
            return false;
        }

        private static bool GetYearMonthOrder(string pattern, DateTimeFormatInfo dtfi, out int order)
        {
            int num = -1;
            int num2 = -1;
            int num3 = 0;
            bool flag = false;
            for (int i = 0; (i < pattern.Length) && (num3 < 2); i++)
            {
                char ch = pattern[i];
                switch (ch)
                {
                    case '\\':
                    case '%':
                    {
                        i++;
                        continue;
                    }
                    case '\'':
                    case '"':
                        flag = !flag;
                        break;
                }
                if (!flag)
                {
                    switch (ch)
                    {
                        case 'y':
                            num = num3++;
                            while (((i + 1) < pattern.Length) && (pattern[i + 1] == 'y'))
                            {
                                i++;
                            }
                            break;

                        case 'M':
                            num2 = num3++;
                            while (((i + 1) < pattern.Length) && (pattern[i + 1] == 'M'))
                            {
                                i++;
                            }
                            break;
                    }
                }
            }
            if ((num == 0) && (num2 == 1))
            {
                order = 4;
                return true;
            }
            if ((num2 == 0) && (num == 1))
            {
                order = 5;
                return true;
            }
            order = -1;
            return false;
        }

        private static bool HandleTimeZone(ref __DTString str, ref DateTimeResult result)
        {
            if (str.Index < (str.len - 1))
            {
                char c = str.Value[str.Index];
                int num = 0;
                while (char.IsWhiteSpace(c) && ((str.Index + num) < (str.len - 1)))
                {
                    num++;
                    c = str.Value[str.Index + num];
                }
                switch (c)
                {
                    case '+':
                    case '-':
                        str.Index += num;
                        if ((result.flags & ParseFlags.TimeZoneUsed) != 0)
                        {
                            result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                            return false;
                        }
                        result.flags |= ParseFlags.TimeZoneUsed;
                        if (!ParseTimeZone(ref str, ref result.timeZoneOffset))
                        {
                            result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                            return false;
                        }
                        break;
                }
            }
            return true;
        }

        internal static bool IsDigit(char ch)
        {
            return ((ch >= '0') && (ch <= '9'));
        }

        [SecuritySafeCritical]
        private static bool Lex(DS dps, ref __DTString str, ref DateTimeToken dtok, ref DateTimeRawInfo raw, ref DateTimeResult result, ref DateTimeFormatInfo dtfi)
        {
            TokenType type;
            int num;
            int num2;
            char ch;
            TokenType type2;
            dtok.dtt = DTT.Unk;
            str.GetRegularToken(out type, out num, dtfi);
            switch (type)
            {
                case TokenType.NumberToken:
                case TokenType.YearNumberToken:
                    if ((raw.numCount != 3) && (num != -1))
                    {
                        if ((dps == DS.T_NNt) && (str.Index < (str.len - 1)))
                        {
                            char ch2 = str.Value[str.Index];
                            if (ch2 == '.')
                            {
                                ParseFraction(ref str, out raw.fraction);
                            }
                        }
                        if (((dps != DS.T_NNt) && (dps != DS.T_Nt)) || ((str.Index >= (str.len - 1)) || HandleTimeZone(ref str, ref result)))
                        {
                            dtok.num = num;
                            if (type != TokenType.YearNumberToken)
                            {
                                switch ((type2 = str.GetSeparatorToken(dtfi, out num2, out ch)))
                                {
                                    case TokenType.SEP_End:
                                        dtok.dtt = DTT.NumEnd;
                                        raw.AddNumber(dtok.num);
                                        goto Label_09A0;

                                    case TokenType.SEP_Space:
                                        dtok.dtt = DTT.NumSpace;
                                        raw.AddNumber(dtok.num);
                                        goto Label_09A0;

                                    case TokenType.SEP_Am:
                                    case TokenType.SEP_Pm:
                                        if (raw.timeMark == TM.NotSet)
                                        {
                                            raw.timeMark = (type2 == TokenType.SEP_Am) ? TM.AM : TM.PM;
                                            dtok.dtt = DTT.NumAmpm;
                                            raw.AddNumber(dtok.num);
                                        }
                                        else
                                        {
                                            result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                                            goto Label_09A0;
                                        }
                                        if (((dps != DS.T_NNt) && (dps != DS.T_Nt)) || HandleTimeZone(ref str, ref result))
                                        {
                                            goto Label_09A0;
                                        }
                                        return false;

                                    case TokenType.SEP_Time:
                                        dtok.dtt = DTT.NumTimesep;
                                        raw.AddNumber(dtok.num);
                                        goto Label_09A0;

                                    case TokenType.SEP_YearSuff:
                                        dtok.num = dtfi.Calendar.ToFourDigitYear(num);
                                        dtok.dtt = DTT.NumDatesuff;
                                        dtok.suffix = type2;
                                        goto Label_09A0;

                                    case TokenType.SEP_Date:
                                        dtok.dtt = DTT.NumDatesep;
                                        raw.AddNumber(dtok.num);
                                        goto Label_09A0;

                                    case TokenType.SEP_MonthSuff:
                                    case TokenType.SEP_DaySuff:
                                        dtok.dtt = DTT.NumDatesuff;
                                        dtok.suffix = type2;
                                        goto Label_09A0;

                                    case TokenType.SEP_HourSuff:
                                    case TokenType.SEP_MinuteSuff:
                                    case TokenType.SEP_SecondSuff:
                                        dtok.dtt = DTT.NumTimesuff;
                                        dtok.suffix = type2;
                                        goto Label_09A0;

                                    case TokenType.SEP_LocalTimeMark:
                                        dtok.dtt = DTT.NumLocalTimeMark;
                                        raw.AddNumber(dtok.num);
                                        goto Label_09A0;

                                    case TokenType.SEP_DateOrOffset:
                                        if ((dateParsingStates[(int) dps][4] == DS.ERROR) && (dateParsingStates[(int) dps][3] > DS.ERROR))
                                        {
                                            str.Index = num2;
                                            str.m_current = ch;
                                            dtok.dtt = DTT.NumSpace;
                                        }
                                        else
                                        {
                                            dtok.dtt = DTT.NumDatesep;
                                        }
                                        raw.AddNumber(dtok.num);
                                        goto Label_09A0;
                                }
                                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                                return false;
                            }
                            if (raw.year != -1)
                            {
                                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                                return false;
                            }
                            raw.year = num;
                            switch ((type2 = str.GetSeparatorToken(dtfi, out num2, out ch)))
                            {
                                case TokenType.SEP_Pm:
                                case TokenType.SEP_Am:
                                    if (raw.timeMark == TM.NotSet)
                                    {
                                        raw.timeMark = (type2 == TokenType.SEP_Am) ? TM.AM : TM.PM;
                                        dtok.dtt = DTT.YearSpace;
                                    }
                                    else
                                    {
                                        result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                                    }
                                    goto Label_029E;

                                case TokenType.SEP_Date:
                                    dtok.dtt = DTT.YearDateSep;
                                    goto Label_029E;

                                case TokenType.SEP_YearSuff:
                                case TokenType.SEP_MonthSuff:
                                case TokenType.SEP_DaySuff:
                                    dtok.dtt = DTT.NumDatesuff;
                                    dtok.suffix = type2;
                                    goto Label_029E;

                                case TokenType.SEP_End:
                                    dtok.dtt = DTT.YearEnd;
                                    goto Label_029E;

                                case TokenType.SEP_Space:
                                    dtok.dtt = DTT.YearSpace;
                                    goto Label_029E;

                                case TokenType.SEP_HourSuff:
                                case TokenType.SEP_MinuteSuff:
                                case TokenType.SEP_SecondSuff:
                                    dtok.dtt = DTT.NumTimesuff;
                                    dtok.suffix = type2;
                                    goto Label_029E;

                                case TokenType.SEP_DateOrOffset:
                                    if ((dateParsingStates[(int) dps][13] == DS.ERROR) && (dateParsingStates[(int) dps][12] > DS.ERROR))
                                    {
                                        str.Index = num2;
                                        str.m_current = ch;
                                        dtok.dtt = DTT.YearSpace;
                                    }
                                    else
                                    {
                                        dtok.dtt = DTT.YearDateSep;
                                    }
                                    goto Label_029E;
                            }
                            result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                        }
                        return false;
                    }
                    result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                    return false;

                case TokenType.Am:
                case TokenType.Pm:
                    if (raw.timeMark != TM.NotSet)
                    {
                        result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                        return false;
                    }
                    raw.timeMark = (TM) num;
                    goto Label_09A0;

                case TokenType.MonthToken:
                {
                    if (raw.month != -1)
                    {
                        result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                        return false;
                    }
                    TokenType type8 = type2 = str.GetSeparatorToken(dtfi, out num2, out ch);
                    if (type8 > TokenType.SEP_Space)
                    {
                        switch (type8)
                        {
                            case TokenType.SEP_Date:
                                dtok.dtt = DTT.MonthDatesep;
                                goto Label_0786;

                            case TokenType.SEP_DateOrOffset:
                                if ((dateParsingStates[(int) dps][8] == DS.ERROR) && (dateParsingStates[(int) dps][7] > DS.ERROR))
                                {
                                    str.Index = num2;
                                    str.m_current = ch;
                                    dtok.dtt = DTT.MonthSpace;
                                }
                                else
                                {
                                    dtok.dtt = DTT.MonthDatesep;
                                }
                                goto Label_0786;
                        }
                    }
                    else
                    {
                        switch (type8)
                        {
                            case TokenType.SEP_End:
                                dtok.dtt = DTT.MonthEnd;
                                goto Label_0786;

                            case TokenType.SEP_Space:
                                dtok.dtt = DTT.MonthSpace;
                                goto Label_0786;
                        }
                    }
                    result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                    return false;
                }
                case TokenType.EndOfString:
                    dtok.dtt = DTT.End;
                    goto Label_09A0;

                case TokenType.DayOfWeekToken:
                    if (raw.dayOfWeek != -1)
                    {
                        result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                        return false;
                    }
                    raw.dayOfWeek = num;
                    dtok.dtt = DTT.DayOfWeek;
                    goto Label_09A0;

                case TokenType.TimeZoneToken:
                    if ((result.flags & ParseFlags.TimeZoneUsed) == 0)
                    {
                        dtok.dtt = DTT.TimeZone;
                        result.flags |= ParseFlags.TimeZoneUsed;
                        result.timeZoneOffset = new TimeSpan(0L);
                        result.flags |= ParseFlags.TimeZoneUtc;
                        goto Label_09A0;
                    }
                    result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                    return false;

                case TokenType.EraToken:
                    if (result.era == -1)
                    {
                        result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                        return false;
                    }
                    result.era = num;
                    dtok.dtt = DTT.Era;
                    goto Label_09A0;

                case TokenType.UnknownToken:
                    if (!char.IsLetter(str.m_current))
                    {
                        if (Environment.GetCompatibilityFlag(CompatibilityFlag.DateTimeParseIgnorePunctuation) && ((result.flags & ParseFlags.CaptureOffset) == 0))
                        {
                            str.GetNext();
                            return true;
                        }
                        if (((str.m_current == '-') || (str.m_current == '+')) && ((result.flags & ParseFlags.TimeZoneUsed) == 0))
                        {
                            int index = str.Index;
                            if (ParseTimeZone(ref str, ref result.timeZoneOffset))
                            {
                                result.flags |= ParseFlags.TimeZoneUsed;
                                return true;
                            }
                            str.Index = index;
                        }
                        if (VerifyValidPunctuation(ref str))
                        {
                            return true;
                        }
                        result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                        return false;
                    }
                    result.SetFailure(System.ParseFailureKind.FormatWithParameter, "Format_UnknowDateTimeWord", str.Index);
                    return false;

                case TokenType.HebrewNumber:
                    if (num < 100)
                    {
                        dtok.num = num;
                        raw.AddNumber(dtok.num);
                        switch ((type2 = str.GetSeparatorToken(dtfi, out num2, out ch)))
                        {
                            case TokenType.SEP_Date:
                            case TokenType.SEP_Space:
                                dtok.dtt = DTT.NumDatesep;
                                goto Label_09A0;

                            case TokenType.SEP_DateOrOffset:
                                if ((dateParsingStates[(int) dps][4] == DS.ERROR) && (dateParsingStates[(int) dps][3] > DS.ERROR))
                                {
                                    str.Index = num2;
                                    str.m_current = ch;
                                    dtok.dtt = DTT.NumSpace;
                                }
                                else
                                {
                                    dtok.dtt = DTT.NumDatesep;
                                }
                                goto Label_09A0;

                            case TokenType.SEP_End:
                                dtok.dtt = DTT.NumEnd;
                                goto Label_09A0;
                        }
                        result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                        return false;
                    }
                    if (raw.year != -1)
                    {
                        result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                        return false;
                    }
                    raw.year = num;
                    switch ((type2 = str.GetSeparatorToken(dtfi, out num2, out ch)))
                    {
                        case TokenType.SEP_End:
                            dtok.dtt = DTT.YearEnd;
                            goto Label_09A0;

                        case TokenType.SEP_Space:
                            dtok.dtt = DTT.YearSpace;
                            goto Label_09A0;

                        case TokenType.SEP_DateOrOffset:
                            if (dateParsingStates[(int) dps][12] > DS.ERROR)
                            {
                                str.Index = num2;
                                str.m_current = ch;
                                dtok.dtt = DTT.YearSpace;
                                goto Label_09A0;
                            }
                            break;
                    }
                    result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                    return false;

                case TokenType.JapaneseEraToken:
                    result.calendar = JapaneseCalendar.GetDefaultInstance();
                    dtfi = DateTimeFormatInfo.GetJapaneseCalendarDTFI();
                    if (result.era == -1)
                    {
                        result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                        return false;
                    }
                    result.era = num;
                    dtok.dtt = DTT.Era;
                    goto Label_09A0;

                case TokenType.TEraToken:
                    result.calendar = TaiwanCalendar.GetDefaultInstance();
                    dtfi = DateTimeFormatInfo.GetTaiwanCalendarDTFI();
                    if (result.era == -1)
                    {
                        result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                        return false;
                    }
                    result.era = num;
                    dtok.dtt = DTT.Era;
                    goto Label_09A0;

                default:
                    goto Label_09A0;
            }
        Label_029E:
            return true;
        Label_0786:
            raw.month = num;
        Label_09A0:
            return true;
        }

        [Conditional("_LOGGING")]
        internal static void LexTraceExit(string message, DS dps)
        {
        }

        private static bool MatchAbbreviatedDayName(ref __DTString str, DateTimeFormatInfo dtfi, ref int result)
        {
            int num = 0;
            result = -1;
            if (str.GetNext())
            {
                for (DayOfWeek week = DayOfWeek.Sunday; week <= DayOfWeek.Saturday; week += 1)
                {
                    string abbreviatedDayName = dtfi.GetAbbreviatedDayName(week);
                    int length = abbreviatedDayName.Length;
                    if ((dtfi.HasSpacesInDayNames ? str.MatchSpecifiedWords(abbreviatedDayName, false, ref length) : str.MatchSpecifiedWord(abbreviatedDayName)) && (length > num))
                    {
                        num = length;
                        result = (int) week;
                    }
                }
            }
            if (result >= 0)
            {
                str.Index += num - 1;
                return true;
            }
            return false;
        }

        private static bool MatchAbbreviatedMonthName(ref __DTString str, DateTimeFormatInfo dtfi, ref int result)
        {
            int maxMatchStrLen = 0;
            result = -1;
            if (str.GetNext())
            {
                int num2 = (dtfi.GetMonthName(13).Length == 0) ? 12 : 13;
                for (int i = 1; i <= num2; i++)
                {
                    string abbreviatedMonthName = dtfi.GetAbbreviatedMonthName(i);
                    int length = abbreviatedMonthName.Length;
                    if ((dtfi.HasSpacesInMonthNames ? str.MatchSpecifiedWords(abbreviatedMonthName, false, ref length) : str.MatchSpecifiedWord(abbreviatedMonthName)) && (length > maxMatchStrLen))
                    {
                        maxMatchStrLen = length;
                        result = i;
                    }
                }
                if ((dtfi.FormatFlags & DateTimeFormatFlags.UseLeapYearMonth) != DateTimeFormatFlags.None)
                {
                    int num5 = str.MatchLongestWords(dtfi.internalGetLeapYearMonthNames(), ref maxMatchStrLen);
                    if (num5 >= 0)
                    {
                        result = num5 + 1;
                    }
                }
            }
            if (result > 0)
            {
                str.Index += maxMatchStrLen - 1;
                return true;
            }
            return false;
        }

        private static bool MatchAbbreviatedTimeMark(ref __DTString str, DateTimeFormatInfo dtfi, ref TM result)
        {
            if (str.GetNext())
            {
                if (str.GetChar() == dtfi.AMDesignator[0])
                {
                    result = TM.AM;
                    return true;
                }
                if (str.GetChar() == dtfi.PMDesignator[0])
                {
                    result = TM.PM;
                    return true;
                }
            }
            return false;
        }

        private static bool MatchDayName(ref __DTString str, DateTimeFormatInfo dtfi, ref int result)
        {
            int num = 0;
            result = -1;
            if (str.GetNext())
            {
                for (DayOfWeek week = DayOfWeek.Sunday; week <= DayOfWeek.Saturday; week += 1)
                {
                    string dayName = dtfi.GetDayName(week);
                    int length = dayName.Length;
                    if ((dtfi.HasSpacesInDayNames ? str.MatchSpecifiedWords(dayName, false, ref length) : str.MatchSpecifiedWord(dayName)) && (length > num))
                    {
                        num = length;
                        result = (int) week;
                    }
                }
            }
            if (result >= 0)
            {
                str.Index += num - 1;
                return true;
            }
            return false;
        }

        private static bool MatchEraName(ref __DTString str, DateTimeFormatInfo dtfi, ref int result)
        {
            if (str.GetNext())
            {
                int[] eras = dtfi.Calendar.Eras;
                if (eras != null)
                {
                    for (int i = 0; i < eras.Length; i++)
                    {
                        string eraName = dtfi.GetEraName(eras[i]);
                        if (str.MatchSpecifiedWord(eraName))
                        {
                            str.Index += eraName.Length - 1;
                            result = eras[i];
                            return true;
                        }
                        eraName = dtfi.GetAbbreviatedEraName(eras[i]);
                        if (str.MatchSpecifiedWord(eraName))
                        {
                            str.Index += eraName.Length - 1;
                            result = eras[i];
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        internal static bool MatchHebrewDigits(ref __DTString str, int digitLen, out int number)
        {
            number = 0;
            HebrewNumberParsingContext context = new HebrewNumberParsingContext(0);
            HebrewNumberParsingState continueParsing = HebrewNumberParsingState.ContinueParsing;
            while ((continueParsing == HebrewNumberParsingState.ContinueParsing) && str.GetNext())
            {
                continueParsing = HebrewNumber.ParseByChar(str.GetChar(), ref context);
            }
            if (continueParsing == HebrewNumberParsingState.FoundEndOfHebrewNumber)
            {
                number = context.result;
                return true;
            }
            return false;
        }

        private static bool MatchMonthName(ref __DTString str, DateTimeFormatInfo dtfi, ref int result)
        {
            int maxMatchStrLen = 0;
            result = -1;
            if (str.GetNext())
            {
                int num2 = (dtfi.GetMonthName(13).Length == 0) ? 12 : 13;
                for (int i = 1; i <= num2; i++)
                {
                    string monthName = dtfi.GetMonthName(i);
                    int length = monthName.Length;
                    if ((dtfi.HasSpacesInMonthNames ? str.MatchSpecifiedWords(monthName, false, ref length) : str.MatchSpecifiedWord(monthName)) && (length > maxMatchStrLen))
                    {
                        maxMatchStrLen = length;
                        result = i;
                    }
                }
                if ((dtfi.FormatFlags & DateTimeFormatFlags.UseGenitiveMonth) != DateTimeFormatFlags.None)
                {
                    int num5 = str.MatchLongestWords(dtfi.MonthGenitiveNames, ref maxMatchStrLen);
                    if (num5 >= 0)
                    {
                        result = num5 + 1;
                    }
                }
                if ((dtfi.FormatFlags & DateTimeFormatFlags.UseLeapYearMonth) != DateTimeFormatFlags.None)
                {
                    int num6 = str.MatchLongestWords(dtfi.internalGetLeapYearMonthNames(), ref maxMatchStrLen);
                    if (num6 >= 0)
                    {
                        result = num6 + 1;
                    }
                }
            }
            if (result > 0)
            {
                str.Index += maxMatchStrLen - 1;
                return true;
            }
            return false;
        }

        private static bool MatchTimeMark(ref __DTString str, DateTimeFormatInfo dtfi, ref TM result)
        {
            result = TM.NotSet;
            if (dtfi.AMDesignator.Length == 0)
            {
                result = TM.AM;
            }
            if (dtfi.PMDesignator.Length == 0)
            {
                result = TM.PM;
            }
            if (str.GetNext())
            {
                string aMDesignator = dtfi.AMDesignator;
                if ((aMDesignator.Length > 0) && str.MatchSpecifiedWord(aMDesignator))
                {
                    str.Index += aMDesignator.Length - 1;
                    result = TM.AM;
                    return true;
                }
                aMDesignator = dtfi.PMDesignator;
                if ((aMDesignator.Length > 0) && str.MatchSpecifiedWord(aMDesignator))
                {
                    str.Index += aMDesignator.Length - 1;
                    result = TM.PM;
                    return true;
                }
                str.Index--;
            }
            return (result != TM.NotSet);
        }

        private static bool MatchWord(ref __DTString str, string target)
        {
            int length = target.Length;
            if (length > (str.Value.Length - str.Index))
            {
                return false;
            }
            if (str.CompareInfo.Compare(str.Value, str.Index, length, target, 0, length, CompareOptions.IgnoreCase) != 0)
            {
                return false;
            }
            int num2 = str.Index + target.Length;
            if (num2 < str.Value.Length)
            {
                char c = str.Value[num2];
                if (char.IsLetter(c))
                {
                    return false;
                }
            }
            str.Index = num2;
            if (str.Index < str.len)
            {
                str.m_current = str.Value[str.Index];
            }
            return true;
        }

        internal static DateTime Parse(string s, DateTimeFormatInfo dtfi, DateTimeStyles styles)
        {
            DateTimeResult result = new DateTimeResult();
            result.Init();
            if (!TryParse(s, dtfi, styles, ref result))
            {
                throw GetDateTimeParseException(ref result);
            }
            return result.parsedDate;
        }

        internal static DateTime Parse(string s, DateTimeFormatInfo dtfi, DateTimeStyles styles, out TimeSpan offset)
        {
            DateTimeResult result = new DateTimeResult();
            result.Init();
            result.flags |= ParseFlags.CaptureOffset;
            if (!TryParse(s, dtfi, styles, ref result))
            {
                throw GetDateTimeParseException(ref result);
            }
            offset = result.timeZoneOffset;
            return result.parsedDate;
        }

        private static bool ParseByFormat(ref __DTString str, ref __DTString format, ref ParsingInfo parseInfo, DateTimeFormatInfo dtfi, ref DateTimeResult result)
        {
            bool flag;
            int returnValue = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            int num5 = 0;
            int num6 = 0;
            int num7 = 0;
            int num8 = 0;
            double num9 = 0.0;
            TM aM = TM.AM;
            char failureMessageFormatArgument = format.GetChar();
            switch (failureMessageFormatArgument)
            {
                case '%':
                    if ((format.Index < (format.Value.Length - 1)) && (format.Value[format.Index + 1] != '%'))
                    {
                        goto Label_0A5A;
                    }
                    result.SetFailure(System.ParseFailureKind.Format, "Format_BadFormatSpecifier", null);
                    return false;

                case '\'':
                case '"':
                {
                    StringBuilder builder = new StringBuilder();
                    if (!TryParseQuoteString(format.Value, format.Index, builder, out returnValue))
                    {
                        result.SetFailure(System.ParseFailureKind.FormatWithParameter, "Format_BadQuote", failureMessageFormatArgument);
                        return false;
                    }
                    format.Index += returnValue - 1;
                    string str2 = builder.ToString();
                    for (int i = 0; i < str2.Length; i++)
                    {
                        if ((str2[i] == ' ') && parseInfo.fAllowInnerWhite)
                        {
                            str.SkipWhiteSpaces();
                        }
                        else if (!str.Match(str2[i]))
                        {
                            result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                            return false;
                        }
                    }
                    if ((result.flags & ParseFlags.CaptureOffset) != 0)
                    {
                        if (((result.flags & ParseFlags.Rfc1123Pattern) != 0) && (str2 == "GMT"))
                        {
                            result.flags |= ParseFlags.TimeZoneUsed;
                            result.timeZoneOffset = TimeSpan.Zero;
                        }
                        else if (((result.flags & ParseFlags.UtcSortPattern) != 0) && (str2 == "Z"))
                        {
                            result.flags |= ParseFlags.TimeZoneUsed;
                            result.timeZoneOffset = TimeSpan.Zero;
                        }
                    }
                    goto Label_0A5A;
                }
                case '.':
                    if (!str.Match(failureMessageFormatArgument))
                    {
                        if (!format.GetNext() || !format.Match('F'))
                        {
                            result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                            return false;
                        }
                        format.GetRepeatCount();
                    }
                    goto Label_0A5A;

                case '/':
                    if (str.Match(dtfi.DateSeparator))
                    {
                        goto Label_0A5A;
                    }
                    result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                    return false;

                case ':':
                    if (str.Match(dtfi.TimeSeparator))
                    {
                        goto Label_0A5A;
                    }
                    result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                    return false;

                case 'F':
                case 'f':
                    returnValue = format.GetRepeatCount();
                    if (returnValue <= 7)
                    {
                        if (!ParseFractionExact(ref str, returnValue, ref num9) && (failureMessageFormatArgument == 'f'))
                        {
                            result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                            return false;
                        }
                        if (result.fraction >= 0.0)
                        {
                            if (num9 != result.fraction)
                            {
                                result.SetFailure(System.ParseFailureKind.FormatWithParameter, "Format_RepeatDateTimePattern", failureMessageFormatArgument);
                                return false;
                            }
                        }
                        else
                        {
                            result.fraction = num9;
                        }
                        goto Label_0A5A;
                    }
                    result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                    return false;

                case 'H':
                    returnValue = format.GetRepeatCount();
                    if (ParseDigits(ref str, (returnValue < 2) ? 1 : 2, out num6))
                    {
                        if (!CheckNewValue(ref result.Hour, num6, failureMessageFormatArgument, ref result))
                        {
                            return false;
                        }
                        goto Label_0A5A;
                    }
                    result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                    return false;

                case 'K':
                    if (!str.Match('Z'))
                    {
                        if (str.Match('+') || str.Match('-'))
                        {
                            str.Index--;
                            TimeSpan span2 = new TimeSpan(0L);
                            if (!ParseTimeZoneOffset(ref str, 3, ref span2))
                            {
                                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                                return false;
                            }
                            if (((result.flags & ParseFlags.TimeZoneUsed) != 0) && (span2 != result.timeZoneOffset))
                            {
                                result.SetFailure(System.ParseFailureKind.FormatWithParameter, "Format_RepeatDateTimePattern", 'K');
                                return false;
                            }
                            result.timeZoneOffset = span2;
                            result.flags |= ParseFlags.TimeZoneUsed;
                        }
                        goto Label_0A5A;
                    }
                    if (((result.flags & ParseFlags.TimeZoneUsed) == 0) || !(result.timeZoneOffset != TimeSpan.Zero))
                    {
                        result.flags |= ParseFlags.TimeZoneUsed;
                        result.timeZoneOffset = new TimeSpan(0L);
                        result.flags |= ParseFlags.TimeZoneUtc;
                        goto Label_0A5A;
                    }
                    result.SetFailure(System.ParseFailureKind.FormatWithParameter, "Format_RepeatDateTimePattern", 'K');
                    return false;

                case 'M':
                    returnValue = format.GetRepeatCount();
                    if (returnValue > 2)
                    {
                        if (returnValue == 3)
                        {
                            if (!MatchAbbreviatedMonthName(ref str, dtfi, ref num3))
                            {
                                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                                return false;
                            }
                        }
                        else if (!MatchMonthName(ref str, dtfi, ref num3))
                        {
                            result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                            return false;
                        }
                        result.flags |= ParseFlags.ParsedMonthName;
                        goto Label_0223;
                    }
                    if (ParseDigits(ref str, returnValue, out num3) || (parseInfo.fCustomNumberParser && parseInfo.parseNumberDelegate(ref str, returnValue, out num3)))
                    {
                        goto Label_0223;
                    }
                    result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                    return false;

                case 'Z':
                    if (((result.flags & ParseFlags.TimeZoneUsed) == 0) || !(result.timeZoneOffset != TimeSpan.Zero))
                    {
                        result.flags |= ParseFlags.TimeZoneUsed;
                        result.timeZoneOffset = new TimeSpan(0L);
                        result.flags |= ParseFlags.TimeZoneUtc;
                        str.Index++;
                        if (!GetTimeZoneName(ref str))
                        {
                            result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                            return false;
                        }
                        str.Index--;
                        goto Label_0A5A;
                    }
                    result.SetFailure(System.ParseFailureKind.FormatWithParameter, "Format_RepeatDateTimePattern", 'Z');
                    return false;

                case '\\':
                    if (!format.GetNext())
                    {
                        result.SetFailure(System.ParseFailureKind.Format, "Format_BadFormatSpecifier", null);
                        return false;
                    }
                    if (str.Match(format.GetChar()))
                    {
                        goto Label_0A5A;
                    }
                    result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                    return false;

                case 'd':
                    returnValue = format.GetRepeatCount();
                    if (returnValue > 2)
                    {
                        if (returnValue == 3)
                        {
                            if (!MatchAbbreviatedDayName(ref str, dtfi, ref num5))
                            {
                                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                                return false;
                            }
                        }
                        else if (!MatchDayName(ref str, dtfi, ref num5))
                        {
                            result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                            return false;
                        }
                        if (!CheckNewValue(ref parseInfo.dayOfWeek, num5, failureMessageFormatArgument, ref result))
                        {
                            return false;
                        }
                        goto Label_0A5A;
                    }
                    if (ParseDigits(ref str, returnValue, out num4) || (parseInfo.fCustomNumberParser && parseInfo.parseNumberDelegate(ref str, returnValue, out num4)))
                    {
                        if (!CheckNewValue(ref result.Day, num4, failureMessageFormatArgument, ref result))
                        {
                            return false;
                        }
                        goto Label_0A5A;
                    }
                    result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                    return false;

                case 'g':
                    returnValue = format.GetRepeatCount();
                    if (MatchEraName(ref str, dtfi, ref result.era))
                    {
                        goto Label_0A5A;
                    }
                    result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                    return false;

                case 'h':
                    parseInfo.fUseHour12 = true;
                    returnValue = format.GetRepeatCount();
                    if (ParseDigits(ref str, (returnValue < 2) ? 1 : 2, out num6))
                    {
                        if (!CheckNewValue(ref result.Hour, num6, failureMessageFormatArgument, ref result))
                        {
                            return false;
                        }
                        goto Label_0A5A;
                    }
                    result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                    return false;

                case 's':
                    returnValue = format.GetRepeatCount();
                    if (ParseDigits(ref str, (returnValue < 2) ? 1 : 2, out num8))
                    {
                        if (!CheckNewValue(ref result.Second, num8, failureMessageFormatArgument, ref result))
                        {
                            return false;
                        }
                        goto Label_0A5A;
                    }
                    result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                    return false;

                case 't':
                    if (format.GetRepeatCount() != 1)
                    {
                        if (!MatchTimeMark(ref str, dtfi, ref aM))
                        {
                            result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                            return false;
                        }
                        goto Label_04DE;
                    }
                    if (MatchAbbreviatedTimeMark(ref str, dtfi, ref aM))
                    {
                        goto Label_04DE;
                    }
                    result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                    return false;

                case 'm':
                    returnValue = format.GetRepeatCount();
                    if (!ParseDigits(ref str, (returnValue < 2) ? 1 : 2, out num7))
                    {
                        result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                        return false;
                    }
                    if (CheckNewValue(ref result.Minute, num7, failureMessageFormatArgument, ref result))
                    {
                        goto Label_0A5A;
                    }
                    return false;

                case 'y':
                    returnValue = format.GetRepeatCount();
                    if (!dtfi.HasForceTwoDigitYears)
                    {
                        if (returnValue <= 2)
                        {
                            parseInfo.fUseTwoDigitYear = true;
                        }
                        flag = ParseDigits(ref str, returnValue, out num2);
                        break;
                    }
                    flag = ParseDigits(ref str, 1, 4, out num2);
                    break;

                case 'z':
                {
                    returnValue = format.GetRepeatCount();
                    TimeSpan span = new TimeSpan(0L);
                    if (ParseTimeZoneOffset(ref str, returnValue, ref span))
                    {
                        if (((result.flags & ParseFlags.TimeZoneUsed) != 0) && (span != result.timeZoneOffset))
                        {
                            result.SetFailure(System.ParseFailureKind.FormatWithParameter, "Format_RepeatDateTimePattern", 'z');
                            return false;
                        }
                        result.timeZoneOffset = span;
                        result.flags |= ParseFlags.TimeZoneUsed;
                        goto Label_0A5A;
                    }
                    result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                    return false;
                }
                default:
                    if (failureMessageFormatArgument == ' ')
                    {
                        if (!parseInfo.fAllowInnerWhite && !str.Match(failureMessageFormatArgument))
                        {
                            if ((parseInfo.fAllowTrailingWhite && format.GetNext()) && ParseByFormat(ref str, ref format, ref parseInfo, dtfi, ref result))
                            {
                                return true;
                            }
                            result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                            return false;
                        }
                    }
                    else if (format.MatchSpecifiedWord("GMT"))
                    {
                        format.Index += "GMT".Length - 1;
                        result.flags |= ParseFlags.TimeZoneUsed;
                        result.timeZoneOffset = TimeSpan.Zero;
                        if (!str.Match("GMT"))
                        {
                            result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                            return false;
                        }
                    }
                    else if (!str.Match(failureMessageFormatArgument))
                    {
                        result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                        return false;
                    }
                    goto Label_0A5A;
            }
            if (!flag && parseInfo.fCustomNumberParser)
            {
                flag = parseInfo.parseNumberDelegate(ref str, returnValue, out num2);
            }
            if (!flag)
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                return false;
            }
            if (CheckNewValue(ref result.Year, num2, failureMessageFormatArgument, ref result))
            {
                goto Label_0A5A;
            }
            return false;
        Label_0223:
            if (CheckNewValue(ref result.Month, num3, failureMessageFormatArgument, ref result))
            {
                goto Label_0A5A;
            }
            return false;
        Label_04DE:
            if (parseInfo.timeMark == TM.NotSet)
            {
                parseInfo.timeMark = aM;
            }
            else if (parseInfo.timeMark != aM)
            {
                result.SetFailure(System.ParseFailureKind.FormatWithParameter, "Format_RepeatDateTimePattern", failureMessageFormatArgument);
                return false;
            }
        Label_0A5A:
            return true;
        }

        internal static bool ParseDigits(ref __DTString str, int digitLen, out int result)
        {
            if (digitLen == 1)
            {
                return ParseDigits(ref str, 1, 2, out result);
            }
            return ParseDigits(ref str, digitLen, digitLen, out result);
        }

        internal static bool ParseDigits(ref __DTString str, int minDigitLen, int maxDigitLen, out int result)
        {
            result = 0;
            int index = str.Index;
            int num2 = 0;
            while (num2 < maxDigitLen)
            {
                if (!str.GetNextDigit())
                {
                    str.Index--;
                    break;
                }
                result = (result * 10) + str.GetDigit();
                num2++;
            }
            if (num2 < minDigitLen)
            {
                str.Index = index;
                return false;
            }
            return true;
        }

        internal static DateTime ParseExact(string s, string format, DateTimeFormatInfo dtfi, DateTimeStyles style)
        {
            DateTimeResult result = new DateTimeResult();
            result.Init();
            if (!TryParseExact(s, format, dtfi, style, ref result))
            {
                throw GetDateTimeParseException(ref result);
            }
            return result.parsedDate;
        }

        internal static DateTime ParseExact(string s, string format, DateTimeFormatInfo dtfi, DateTimeStyles style, out TimeSpan offset)
        {
            DateTimeResult result = new DateTimeResult();
            offset = TimeSpan.Zero;
            result.Init();
            result.flags |= ParseFlags.CaptureOffset;
            if (!TryParseExact(s, format, dtfi, style, ref result))
            {
                throw GetDateTimeParseException(ref result);
            }
            offset = result.timeZoneOffset;
            return result.parsedDate;
        }

        internal static DateTime ParseExactMultiple(string s, string[] formats, DateTimeFormatInfo dtfi, DateTimeStyles style)
        {
            DateTimeResult result = new DateTimeResult();
            result.Init();
            if (!TryParseExactMultiple(s, formats, dtfi, style, ref result))
            {
                throw GetDateTimeParseException(ref result);
            }
            return result.parsedDate;
        }

        internal static DateTime ParseExactMultiple(string s, string[] formats, DateTimeFormatInfo dtfi, DateTimeStyles style, out TimeSpan offset)
        {
            DateTimeResult result = new DateTimeResult();
            offset = TimeSpan.Zero;
            result.Init();
            result.flags |= ParseFlags.CaptureOffset;
            if (!TryParseExactMultiple(s, formats, dtfi, style, ref result))
            {
                throw GetDateTimeParseException(ref result);
            }
            offset = result.timeZoneOffset;
            return result.parsedDate;
        }

        private static bool ParseFraction(ref __DTString str, out double result)
        {
            char ch;
            result = 0.0;
            double num = 0.1;
            int num2 = 0;
            while (str.GetNext() && IsDigit(ch = str.m_current))
            {
                result += (ch - '0') * num;
                num *= 0.1;
                num2++;
            }
            return (num2 > 0);
        }

        [SecuritySafeCritical]
        private static bool ParseFractionExact(ref __DTString str, int maxDigitLen, ref double result)
        {
            if (!str.GetNextDigit())
            {
                str.Index--;
                return false;
            }
            result = str.GetDigit();
            int num = 1;
            while (num < maxDigitLen)
            {
                if (!str.GetNextDigit())
                {
                    str.Index--;
                    break;
                }
                result = (result * 10.0) + str.GetDigit();
                num++;
            }
            result = ((double) result) / Math.Pow(10.0, (double) num);
            return (num == maxDigitLen);
        }

        [SecuritySafeCritical]
        private static bool ParseISO8601(ref DateTimeRawInfo raw, ref __DTString str, DateTimeStyles styles, ref DateTimeResult result)
        {
            int num;
            int num2;
            DateTime time;
            if ((raw.year >= 0) && (raw.GetNumber(0) >= 0))
            {
                raw.GetNumber(1);
            }
            str.Index--;
            int num3 = 0;
            double num4 = 0.0;
            str.SkipWhiteSpaces();
            if (!ParseDigits(ref str, 2, out num))
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                return false;
            }
            str.SkipWhiteSpaces();
            if (!str.Match(':'))
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                return false;
            }
            str.SkipWhiteSpaces();
            if (!ParseDigits(ref str, 2, out num2))
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                return false;
            }
            str.SkipWhiteSpaces();
            if (str.Match(':'))
            {
                str.SkipWhiteSpaces();
                if (!ParseDigits(ref str, 2, out num3))
                {
                    result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                    return false;
                }
                if (str.Match('.'))
                {
                    if (!ParseFraction(ref str, out num4))
                    {
                        result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                        return false;
                    }
                    str.Index--;
                }
                str.SkipWhiteSpaces();
            }
            if (str.GetNext())
            {
                switch (str.GetChar())
                {
                    case '+':
                    case '-':
                        result.flags |= ParseFlags.TimeZoneUsed;
                        if (!ParseTimeZone(ref str, ref result.timeZoneOffset))
                        {
                            result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                            return false;
                        }
                        break;

                    case 'Z':
                    case 'z':
                        result.flags |= ParseFlags.TimeZoneUsed;
                        result.timeZoneOffset = TimeSpan.Zero;
                        result.flags |= ParseFlags.TimeZoneUtc;
                        break;

                    default:
                        str.Index--;
                        break;
                }
                str.SkipWhiteSpaces();
                if (str.Match('#'))
                {
                    if (!VerifyValidPunctuation(ref str))
                    {
                        result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                        return false;
                    }
                    str.SkipWhiteSpaces();
                }
                if (str.Match('\0') && !VerifyValidPunctuation(ref str))
                {
                    result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                    return false;
                }
                if (str.GetNext())
                {
                    result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                    return false;
                }
            }
            if (!GregorianCalendar.GetDefaultInstance().TryToDateTime(raw.year, raw.GetNumber(0), raw.GetNumber(1), num, num2, num3, 0, result.era, out time))
            {
                result.SetFailure(System.ParseFailureKind.FormatBadDateTimeCalendar, "Format_BadDateTimeCalendar", null);
                return false;
            }
            time = time.AddTicks((long) Math.Round((double) (num4 * 10000000.0)));
            result.parsedDate = time;
            if (!DetermineTimeZoneAdjustments(ref result, styles, false))
            {
                return false;
            }
            return true;
        }

        private static bool ParseSign(ref __DTString str, ref bool result)
        {
            if (str.GetNext())
            {
                switch (str.GetChar())
                {
                    case '+':
                        result = true;
                        return true;

                    case '-':
                        result = false;
                        return true;
                }
            }
            return false;
        }

        private static bool ParseTimeZone(ref __DTString str, ref TimeSpan result)
        {
            int hours = 0;
            int minutes = 0;
            DTSubString subString = str.GetSubString();
            if (subString.length != 1)
            {
                return false;
            }
            char ch = subString[0];
            if ((ch != '+') && (ch != '-'))
            {
                return false;
            }
            str.ConsumeSubString(subString);
            subString = str.GetSubString();
            if (subString.type != DTSubStringType.Number)
            {
                return false;
            }
            int num3 = subString.value;
            int length = subString.length;
            switch (length)
            {
                case 1:
                case 2:
                    hours = num3;
                    str.ConsumeSubString(subString);
                    subString = str.GetSubString();
                    if ((subString.length == 1) && (subString[0] == ':'))
                    {
                        str.ConsumeSubString(subString);
                        subString = str.GetSubString();
                        if (((subString.type != DTSubStringType.Number) || (subString.length < 1)) || (subString.length > 2))
                        {
                            return false;
                        }
                        minutes = subString.value;
                        str.ConsumeSubString(subString);
                    }
                    break;

                default:
                    if ((length != 3) && (length != 4))
                    {
                        return false;
                    }
                    hours = num3 / 100;
                    minutes = num3 % 100;
                    str.ConsumeSubString(subString);
                    break;
            }
            if ((minutes < 0) || (minutes >= 60))
            {
                return false;
            }
            result = new TimeSpan(hours, minutes, 0);
            if (ch == '-')
            {
                result = result.Negate();
            }
            return true;
        }

        private static bool ParseTimeZoneOffset(ref __DTString str, int len, ref TimeSpan result)
        {
            int num;
            bool flag = true;
            int num2 = 0;
            switch (len)
            {
                case 1:
                case 2:
                    if (ParseSign(ref str, ref flag))
                    {
                        if (ParseDigits(ref str, len, out num))
                        {
                            break;
                        }
                        return false;
                    }
                    return false;

                default:
                    if (!ParseSign(ref str, ref flag))
                    {
                        return false;
                    }
                    if (!ParseDigits(ref str, 1, out num))
                    {
                        return false;
                    }
                    if (str.Match(":"))
                    {
                        if (!ParseDigits(ref str, 2, out num2))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        str.Index--;
                        if (!ParseDigits(ref str, 2, out num2))
                        {
                            return false;
                        }
                    }
                    break;
            }
            if ((num2 < 0) || (num2 >= 60))
            {
                return false;
            }
            result = new TimeSpan(num, num2, 0);
            if (!flag)
            {
                result = result.Negate();
            }
            return true;
        }

        private static bool ProcessDateTimeSuffix(ref DateTimeResult result, ref DateTimeRawInfo raw, ref DateTimeToken dtok)
        {
            switch (dtok.suffix)
            {
                case TokenType.SEP_HourSuff:
                    if ((result.flags & ParseFlags.HaveHour) != 0)
                    {
                        return false;
                    }
                    result.flags |= ParseFlags.HaveHour;
                    result.Hour = dtok.num;
                    break;

                case TokenType.SEP_MinuteSuff:
                    if ((result.flags & ParseFlags.HaveMinute) != 0)
                    {
                        return false;
                    }
                    result.flags |= ParseFlags.HaveMinute;
                    result.Minute = dtok.num;
                    break;

                case TokenType.SEP_SecondSuff:
                    if ((result.flags & ParseFlags.HaveSecond) != 0)
                    {
                        return false;
                    }
                    result.flags |= ParseFlags.HaveSecond;
                    result.Second = dtok.num;
                    break;

                case TokenType.SEP_YearSuff:
                    if ((result.flags & ParseFlags.HaveYear) != 0)
                    {
                        return false;
                    }
                    result.flags |= ParseFlags.HaveYear;
                    result.Year = raw.year = dtok.num;
                    break;

                case TokenType.SEP_MonthSuff:
                    if ((result.flags & ParseFlags.HaveMonth) != 0)
                    {
                        return false;
                    }
                    result.flags |= ParseFlags.HaveMonth;
                    result.Month = raw.month = dtok.num;
                    break;

                case TokenType.SEP_DaySuff:
                    if ((result.flags & ParseFlags.HaveDay) != 0)
                    {
                        return false;
                    }
                    result.flags |= ParseFlags.HaveDay;
                    result.Day = dtok.num;
                    break;
            }
            return true;
        }

        internal static bool ProcessHebrewTerminalState(DS dps, ref DateTimeResult result, ref DateTimeStyles styles, ref DateTimeRawInfo raw, DateTimeFormatInfo dtfi)
        {
            switch (dps)
            {
                case DS.DX_MN:
                case DS.DX_NM:
                    GetDefaultYear(ref result, ref styles);
                    if (dtfi.YearMonthAdjustment(ref result.Year, ref raw.month, true))
                    {
                        if (!GetHebrewDayOfNM(ref result, ref raw, dtfi))
                        {
                            return false;
                        }
                        break;
                    }
                    result.SetFailure(System.ParseFailureKind.FormatBadDateTimeCalendar, "Format_BadDateTimeCalendar", null);
                    return false;

                case DS.DX_MNN:
                    raw.year = raw.GetNumber(1);
                    if (dtfi.YearMonthAdjustment(ref raw.year, ref raw.month, true))
                    {
                        if (!GetDayOfMNN(ref result, ref raw, dtfi))
                        {
                            return false;
                        }
                        break;
                    }
                    result.SetFailure(System.ParseFailureKind.FormatBadDateTimeCalendar, "Format_BadDateTimeCalendar", null);
                    return false;

                case DS.DX_YMN:
                    if (dtfi.YearMonthAdjustment(ref raw.year, ref raw.month, true))
                    {
                        if (!GetDayOfYMN(ref result, ref raw, dtfi))
                        {
                            return false;
                        }
                        break;
                    }
                    result.SetFailure(System.ParseFailureKind.FormatBadDateTimeCalendar, "Format_BadDateTimeCalendar", null);
                    return false;

                case DS.DX_YM:
                    if (dtfi.YearMonthAdjustment(ref raw.year, ref raw.month, true))
                    {
                        if (!GetDayOfYM(ref result, ref raw, dtfi))
                        {
                            return false;
                        }
                        break;
                    }
                    result.SetFailure(System.ParseFailureKind.FormatBadDateTimeCalendar, "Format_BadDateTimeCalendar", null);
                    return false;

                case DS.TX_N:
                    if (GetTimeOfN(dtfi, ref result, ref raw))
                    {
                        break;
                    }
                    return false;

                case DS.TX_NN:
                    if (GetTimeOfNN(dtfi, ref result, ref raw))
                    {
                        break;
                    }
                    return false;

                case DS.TX_NNN:
                    if (GetTimeOfNNN(dtfi, ref result, ref raw))
                    {
                        break;
                    }
                    return false;

                default:
                    result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                    return false;
            }
            if (dps > DS.ERROR)
            {
                raw.numCount = 0;
            }
            return true;
        }

        internal static bool ProcessTerminaltState(DS dps, ref DateTimeResult result, ref DateTimeStyles styles, ref DateTimeRawInfo raw, DateTimeFormatInfo dtfi)
        {
            bool dateOfDSN = true;
            switch (dps)
            {
                case DS.DX_NN:
                    dateOfDSN = GetDayOfNN(ref result, ref styles, ref raw, dtfi);
                    break;

                case DS.DX_NNN:
                    dateOfDSN = GetDayOfNNN(ref result, ref raw, dtfi);
                    break;

                case DS.DX_MN:
                    dateOfDSN = GetDayOfMN(ref result, ref styles, ref raw, dtfi);
                    break;

                case DS.DX_NM:
                    dateOfDSN = GetDayOfNM(ref result, ref styles, ref raw, dtfi);
                    break;

                case DS.DX_MNN:
                    dateOfDSN = GetDayOfMNN(ref result, ref raw, dtfi);
                    break;

                case DS.DX_DS:
                    dateOfDSN = true;
                    break;

                case DS.DX_DSN:
                    dateOfDSN = GetDateOfDSN(ref result, ref raw);
                    break;

                case DS.DX_NDS:
                    dateOfDSN = GetDateOfNDS(ref result, ref raw);
                    break;

                case DS.DX_NNDS:
                    dateOfDSN = GetDateOfNNDS(ref result, ref raw, dtfi);
                    break;

                case DS.DX_YNN:
                    dateOfDSN = GetDayOfYNN(ref result, ref raw, dtfi);
                    break;

                case DS.DX_YMN:
                    dateOfDSN = GetDayOfYMN(ref result, ref raw, dtfi);
                    break;

                case DS.DX_YN:
                    dateOfDSN = GetDayOfYN(ref result, ref raw, dtfi);
                    break;

                case DS.DX_YM:
                    dateOfDSN = GetDayOfYM(ref result, ref raw, dtfi);
                    break;

                case DS.TX_N:
                    dateOfDSN = GetTimeOfN(dtfi, ref result, ref raw);
                    break;

                case DS.TX_NN:
                    dateOfDSN = GetTimeOfNN(dtfi, ref result, ref raw);
                    break;

                case DS.TX_NNN:
                    dateOfDSN = GetTimeOfNNN(dtfi, ref result, ref raw);
                    break;

                case DS.TX_TS:
                    dateOfDSN = true;
                    break;

                case DS.DX_NNY:
                    dateOfDSN = GetDayOfNNY(ref result, ref raw, dtfi);
                    break;
            }
            if (!dateOfDSN)
            {
                return false;
            }
            if (dps > DS.ERROR)
            {
                raw.numCount = 0;
            }
            return true;
        }

        [Conditional("_LOGGING")]
        internal static void PTSTraceExit(DS dps, bool passed)
        {
        }

        private static bool SetDateDMY(ref DateTimeResult result, int day, int month, int year)
        {
            return SetDateYMD(ref result, year, month, day);
        }

        private static bool SetDateMDY(ref DateTimeResult result, int month, int day, int year)
        {
            return SetDateYMD(ref result, year, month, day);
        }

        private static bool SetDateYDM(ref DateTimeResult result, int year, int day, int month)
        {
            return SetDateYMD(ref result, year, month, day);
        }

        private static bool SetDateYMD(ref DateTimeResult result, int year, int month, int day)
        {
            if (result.calendar.IsValidDay(year, month, day, result.era))
            {
                result.SetDate(year, month, day);
                return true;
            }
            return false;
        }

        [Conditional("_LOGGING")]
        internal static void TPTraceExit(string message, DS dps)
        {
        }

        internal static bool TryParse(string s, DateTimeFormatInfo dtfi, DateTimeStyles styles, out DateTime result)
        {
            result = DateTime.MinValue;
            DateTimeResult result2 = new DateTimeResult();
            result2.Init();
            if (TryParse(s, dtfi, styles, ref result2))
            {
                result = result2.parsedDate;
                return true;
            }
            return false;
        }

        [SecuritySafeCritical]
        internal static unsafe bool TryParse(string s, DateTimeFormatInfo dtfi, DateTimeStyles styles, ref DateTimeResult result)
        {
            DateTime time;
            if (s == null)
            {
                result.SetFailure(System.ParseFailureKind.ArgumentNull, "ArgumentNull_String", null, "s");
                return false;
            }
            if (s.Length == 0)
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                return false;
            }
            DS bEGIN = DS.BEGIN;
            bool flag = false;
            DateTimeToken dtok = new DateTimeToken {
                suffix = TokenType.SEP_Unk
            };
            DateTimeRawInfo raw = new DateTimeRawInfo();
            int* numberBuffer = (int*) stackalloc byte[(((IntPtr) 3) * 4)];
            raw.Init(numberBuffer);
            result.calendar = dtfi.Calendar;
            result.era = 0;
            __DTString str = new __DTString(s, dtfi);
            str.GetNext();
            do
            {
                if (!Lex(bEGIN, ref str, ref dtok, ref raw, ref result, ref dtfi))
                {
                    return false;
                }
                if (dtok.dtt != DTT.Unk)
                {
                    if (dtok.suffix != TokenType.SEP_Unk)
                    {
                        if (!ProcessDateTimeSuffix(ref result, ref raw, ref dtok))
                        {
                            result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                            return false;
                        }
                        dtok.suffix = TokenType.SEP_Unk;
                    }
                    if (dtok.dtt == DTT.NumLocalTimeMark)
                    {
                        switch (bEGIN)
                        {
                            case DS.D_YNd:
                            case DS.D_YN:
                                return ParseISO8601(ref raw, ref str, styles, ref result);
                        }
                        result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                        return false;
                    }
                    bEGIN = dateParsingStates[(int) bEGIN][(int) dtok.dtt];
                    if (bEGIN == DS.ERROR)
                    {
                        result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                        return false;
                    }
                    if (bEGIN > DS.ERROR)
                    {
                        if ((dtfi.FormatFlags & DateTimeFormatFlags.UseHebrewRule) != DateTimeFormatFlags.None)
                        {
                            if (!ProcessHebrewTerminalState(bEGIN, ref result, ref styles, ref raw, dtfi))
                            {
                                return false;
                            }
                        }
                        else if (!ProcessTerminaltState(bEGIN, ref result, ref styles, ref raw, dtfi))
                        {
                            return false;
                        }
                        flag = true;
                        bEGIN = DS.BEGIN;
                    }
                }
            }
            while (((dtok.dtt != DTT.End) && (dtok.dtt != DTT.NumEnd)) && (dtok.dtt != DTT.MonthEnd));
            if (!flag)
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                return false;
            }
            AdjustTimeMark(dtfi, ref raw);
            if (!AdjustHour(ref result.Hour, raw.timeMark))
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                return false;
            }
            bool bTimeOnly = ((result.Year == -1) && (result.Month == -1)) && (result.Day == -1);
            if (!CheckDefaultDateTime(ref result, ref result.calendar, styles))
            {
                return false;
            }
            if (!result.calendar.TryToDateTime(result.Year, result.Month, result.Day, result.Hour, result.Minute, result.Second, 0, result.era, out time))
            {
                result.SetFailure(System.ParseFailureKind.FormatBadDateTimeCalendar, "Format_BadDateTimeCalendar", null);
                return false;
            }
            if (raw.fraction > 0.0)
            {
                time = time.AddTicks((long) Math.Round((double) (raw.fraction * 10000000.0)));
            }
            if ((raw.dayOfWeek != -1) && (raw.dayOfWeek != result.calendar.GetDayOfWeek(time)))
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDayOfWeek", null);
                return false;
            }
            result.parsedDate = time;
            if (!DetermineTimeZoneAdjustments(ref result, styles, bTimeOnly))
            {
                return false;
            }
            return true;
        }

        internal static bool TryParse(string s, DateTimeFormatInfo dtfi, DateTimeStyles styles, out DateTime result, out TimeSpan offset)
        {
            result = DateTime.MinValue;
            offset = TimeSpan.Zero;
            DateTimeResult result2 = new DateTimeResult();
            result2.Init();
            result2.flags |= ParseFlags.CaptureOffset;
            if (TryParse(s, dtfi, styles, ref result2))
            {
                result = result2.parsedDate;
                offset = result2.timeZoneOffset;
                return true;
            }
            return false;
        }

        internal static bool TryParseExact(string s, string format, DateTimeFormatInfo dtfi, DateTimeStyles style, out DateTime result)
        {
            result = DateTime.MinValue;
            DateTimeResult result2 = new DateTimeResult();
            result2.Init();
            if (TryParseExact(s, format, dtfi, style, ref result2))
            {
                result = result2.parsedDate;
                return true;
            }
            return false;
        }

        internal static bool TryParseExact(string s, string format, DateTimeFormatInfo dtfi, DateTimeStyles style, ref DateTimeResult result)
        {
            if (s == null)
            {
                result.SetFailure(System.ParseFailureKind.ArgumentNull, "ArgumentNull_String", null, "s");
                return false;
            }
            if (format == null)
            {
                result.SetFailure(System.ParseFailureKind.ArgumentNull, "ArgumentNull_String", null, "format");
                return false;
            }
            if (s.Length == 0)
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                return false;
            }
            if (format.Length == 0)
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadFormatSpecifier", null);
                return false;
            }
            return DoStrictParse(s, format, style, dtfi, ref result);
        }

        internal static bool TryParseExact(string s, string format, DateTimeFormatInfo dtfi, DateTimeStyles style, out DateTime result, out TimeSpan offset)
        {
            result = DateTime.MinValue;
            offset = TimeSpan.Zero;
            DateTimeResult result2 = new DateTimeResult();
            result2.Init();
            result2.flags |= ParseFlags.CaptureOffset;
            if (TryParseExact(s, format, dtfi, style, ref result2))
            {
                result = result2.parsedDate;
                offset = result2.timeZoneOffset;
                return true;
            }
            return false;
        }

        internal static bool TryParseExactMultiple(string s, string[] formats, DateTimeFormatInfo dtfi, DateTimeStyles style, out DateTime result)
        {
            result = DateTime.MinValue;
            DateTimeResult result2 = new DateTimeResult();
            result2.Init();
            if (TryParseExactMultiple(s, formats, dtfi, style, ref result2))
            {
                result = result2.parsedDate;
                return true;
            }
            return false;
        }

        internal static bool TryParseExactMultiple(string s, string[] formats, DateTimeFormatInfo dtfi, DateTimeStyles style, ref DateTimeResult result)
        {
            if (s == null)
            {
                result.SetFailure(System.ParseFailureKind.ArgumentNull, "ArgumentNull_String", null, "s");
                return false;
            }
            if (formats == null)
            {
                result.SetFailure(System.ParseFailureKind.ArgumentNull, "ArgumentNull_String", null, "formats");
                return false;
            }
            if (s.Length == 0)
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
                return false;
            }
            if (formats.Length == 0)
            {
                result.SetFailure(System.ParseFailureKind.Format, "Format_BadFormatSpecifier", null);
                return false;
            }
            for (int i = 0; i < formats.Length; i++)
            {
                if ((formats[i] == null) || (formats[i].Length == 0))
                {
                    result.SetFailure(System.ParseFailureKind.Format, "Format_BadFormatSpecifier", null);
                    return false;
                }
                DateTimeResult result2 = new DateTimeResult();
                result2.Init();
                result2.flags = result.flags;
                if (TryParseExact(s, formats[i], dtfi, style, ref result2))
                {
                    result.parsedDate = result2.parsedDate;
                    result.timeZoneOffset = result2.timeZoneOffset;
                    return true;
                }
            }
            result.SetFailure(System.ParseFailureKind.Format, "Format_BadDateTime", null);
            return false;
        }

        internal static bool TryParseExactMultiple(string s, string[] formats, DateTimeFormatInfo dtfi, DateTimeStyles style, out DateTime result, out TimeSpan offset)
        {
            result = DateTime.MinValue;
            offset = TimeSpan.Zero;
            DateTimeResult result2 = new DateTimeResult();
            result2.Init();
            result2.flags |= ParseFlags.CaptureOffset;
            if (TryParseExactMultiple(s, formats, dtfi, style, ref result2))
            {
                result = result2.parsedDate;
                offset = result2.timeZoneOffset;
                return true;
            }
            return false;
        }

        internal static bool TryParseQuoteString(string format, int pos, StringBuilder result, out int returnValue)
        {
            returnValue = 0;
            int length = format.Length;
            int num2 = pos;
            char ch = format[pos++];
            bool flag = false;
            while (pos < length)
            {
                char ch2 = format[pos++];
                if (ch2 == ch)
                {
                    flag = true;
                    break;
                }
                if (ch2 == '\\')
                {
                    if (pos >= length)
                    {
                        return false;
                    }
                    result.Append(format[pos++]);
                }
                else
                {
                    result.Append(ch2);
                }
            }
            if (!flag)
            {
                return false;
            }
            returnValue = pos - num2;
            return true;
        }

        private static bool VerifyValidPunctuation(ref __DTString str)
        {
            char c = str.Value[str.Index];
            if (c == '#')
            {
                bool flag = false;
                bool flag2 = false;
                for (int j = 0; j < str.len; j++)
                {
                    c = str.Value[j];
                    if (c == '#')
                    {
                        if (flag)
                        {
                            if (flag2)
                            {
                                return false;
                            }
                            flag2 = true;
                        }
                        else
                        {
                            flag = true;
                        }
                    }
                    else if (c == '\0')
                    {
                        if (!flag2)
                        {
                            return false;
                        }
                    }
                    else if (!char.IsWhiteSpace(c) && (!flag || flag2))
                    {
                        return false;
                    }
                }
                if (!flag2)
                {
                    return false;
                }
                str.GetNext();
                return true;
            }
            if (c != '\0')
            {
                return false;
            }
            for (int i = str.Index; i < str.len; i++)
            {
                if (str.Value[i] != '\0')
                {
                    return false;
                }
            }
            str.Index = str.len;
            return true;
        }

        internal enum DS
        {
            BEGIN,
            N,
            NN,
            D_Nd,
            D_NN,
            D_NNd,
            D_M,
            D_MN,
            D_NM,
            D_MNd,
            D_NDS,
            D_Y,
            D_YN,
            D_YNd,
            D_YM,
            D_YMd,
            D_S,
            T_S,
            T_Nt,
            T_NNt,
            ERROR,
            DX_NN,
            DX_NNN,
            DX_MN,
            DX_NM,
            DX_MNN,
            DX_DS,
            DX_DSN,
            DX_NDS,
            DX_NNDS,
            DX_YNN,
            DX_YMN,
            DX_YN,
            DX_YM,
            TX_N,
            TX_NN,
            TX_NNN,
            TX_TS,
            DX_NNY
        }

        internal enum DTT
        {
            End,
            NumEnd,
            NumAmpm,
            NumSpace,
            NumDatesep,
            NumTimesep,
            MonthEnd,
            MonthSpace,
            MonthDatesep,
            NumDatesuff,
            NumTimesuff,
            DayOfWeek,
            YearSpace,
            YearDateSep,
            YearEnd,
            TimeZone,
            Era,
            NumUTCTimeMark,
            Unk,
            NumLocalTimeMark,
            Max
        }

        internal delegate bool MatchNumberDelegate(ref __DTString str, int digitLen, out int result);

        internal enum TM
        {
            AM = 0,
            NotSet = -1,
            PM = 1
        }
    }
}

