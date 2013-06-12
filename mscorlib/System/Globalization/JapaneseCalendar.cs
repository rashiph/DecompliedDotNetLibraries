namespace System.Globalization
{
    using Microsoft.Win32;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, ComVisible(true)]
    public class JapaneseCalendar : Calendar
    {
        private const string c_japaneseErasHive = @"System\CurrentControlSet\Control\Nls\Calendars\Japanese\Eras";
        private const string c_japaneseErasHivePermissionList = @"HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Nls\Calendars\Japanese\Eras";
        internal static readonly DateTime calendarMinValue = new DateTime(0x74c, 9, 8);
        private const int DEFAULT_TWO_DIGIT_YEAR_MAX = 0x63;
        internal GregorianCalendarHelper helper;
        internal static EraInfo[] japaneseEraInfo;
        internal static Calendar s_defaultInstance;

        public JapaneseCalendar()
        {
            try
            {
                new CultureInfo("ja-JP");
            }
            catch (ArgumentException exception)
            {
                throw new TypeInitializationException(base.GetType().FullName, exception);
            }
            this.helper = new GregorianCalendarHelper(this, GetEraInfo());
        }

        internal static string[] AbbrevEraNames()
        {
            EraInfo[] eraInfo = GetEraInfo();
            string[] strArray = new string[eraInfo.Length];
            for (int i = 0; i < eraInfo.Length; i++)
            {
                strArray[i] = eraInfo[(eraInfo.Length - i) - 1].abbrevEraName;
            }
            return strArray;
        }

        public override DateTime AddMonths(DateTime time, int months)
        {
            return this.helper.AddMonths(time, months);
        }

        public override DateTime AddYears(DateTime time, int years)
        {
            return this.helper.AddYears(time, years);
        }

        private static int CompareEraRanges(EraInfo a, EraInfo b)
        {
            return b.ticks.CompareTo(a.ticks);
        }

        internal static string[] EnglishEraNames()
        {
            EraInfo[] eraInfo = GetEraInfo();
            string[] strArray = new string[eraInfo.Length];
            for (int i = 0; i < eraInfo.Length; i++)
            {
                strArray[i] = eraInfo[(eraInfo.Length - i) - 1].englishEraName;
            }
            return strArray;
        }

        internal static string[] EraNames()
        {
            EraInfo[] eraInfo = GetEraInfo();
            string[] strArray = new string[eraInfo.Length];
            for (int i = 0; i < eraInfo.Length; i++)
            {
                strArray[i] = eraInfo[(eraInfo.Length - i) - 1].eraName;
            }
            return strArray;
        }

        public override int GetDayOfMonth(DateTime time)
        {
            return this.helper.GetDayOfMonth(time);
        }

        public override DayOfWeek GetDayOfWeek(DateTime time)
        {
            return this.helper.GetDayOfWeek(time);
        }

        public override int GetDayOfYear(DateTime time)
        {
            return this.helper.GetDayOfYear(time);
        }

        public override int GetDaysInMonth(int year, int month, int era)
        {
            return this.helper.GetDaysInMonth(year, month, era);
        }

        public override int GetDaysInYear(int year, int era)
        {
            return this.helper.GetDaysInYear(year, era);
        }

        internal static Calendar GetDefaultInstance()
        {
            if (s_defaultInstance == null)
            {
                s_defaultInstance = new JapaneseCalendar();
            }
            return s_defaultInstance;
        }

        public override int GetEra(DateTime time)
        {
            return this.helper.GetEra(time);
        }

        private static EraInfo GetEraFromValue(string value, string data)
        {
            if ((value != null) && (data != null))
            {
                int num;
                int num2;
                int num3;
                if (value.Length != 10)
                {
                    return null;
                }
                if ((!Number.TryParseInt32(value.Substring(0, 4), NumberStyles.None, NumberFormatInfo.InvariantInfo, out num) || !Number.TryParseInt32(value.Substring(5, 2), NumberStyles.None, NumberFormatInfo.InvariantInfo, out num2)) || !Number.TryParseInt32(value.Substring(8, 2), NumberStyles.None, NumberFormatInfo.InvariantInfo, out num3))
                {
                    return null;
                }
                string[] strArray = data.Split(new char[] { '_' });
                if ((strArray.Length == 4) && (((strArray[0].Length != 0) && (strArray[1].Length != 0)) && ((strArray[2].Length != 0) && (strArray[3].Length != 0))))
                {
                    return new EraInfo(0, num, num2, num3, num - 1, 1, 0, strArray[0], strArray[1], strArray[3]);
                }
            }
            return null;
        }

        [SecuritySafeCritical]
        internal static EraInfo[] GetEraInfo()
        {
            if (japaneseEraInfo == null)
            {
                japaneseEraInfo = GetErasFromRegistry();
                if (japaneseEraInfo == null)
                {
                    japaneseEraInfo = new EraInfo[] { new EraInfo(4, 0x7c5, 1, 8, 0x7c4, 1, 0x1f4b, "平成", "平", "H"), new EraInfo(3, 0x786, 12, 0x19, 0x785, 1, 0x40, "昭和", "昭", "S"), new EraInfo(2, 0x778, 7, 30, 0x777, 1, 15, "大正", "大", "T"), new EraInfo(1, 0x74c, 1, 1, 0x74b, 1, 0x2d, "明治", "明", "M") };
                }
            }
            return japaneseEraInfo;
        }

        [SecuritySafeCritical]
        private static EraInfo[] GetErasFromRegistry()
        {
            int index = 0;
            EraInfo[] array = null;
            try
            {
                PermissionSet set = new PermissionSet(PermissionState.None);
                set.AddPermission(new RegistryPermission(RegistryPermissionAccess.Read, @"HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Nls\Calendars\Japanese\Eras"));
                set.Assert();
                RegistryKey key = RegistryKey.GetBaseKey(RegistryKey.HKEY_LOCAL_MACHINE).OpenSubKey(@"System\CurrentControlSet\Control\Nls\Calendars\Japanese\Eras", false);
                if (key == null)
                {
                    return null;
                }
                string[] valueNames = key.GetValueNames();
                if ((valueNames != null) && (valueNames.Length > 0))
                {
                    array = new EraInfo[valueNames.Length];
                    for (int j = 0; j < valueNames.Length; j++)
                    {
                        EraInfo eraFromValue = GetEraFromValue(valueNames[j], key.GetValue(valueNames[j]).ToString());
                        if (eraFromValue != null)
                        {
                            array[index] = eraFromValue;
                            index++;
                        }
                    }
                }
            }
            catch (SecurityException)
            {
                return null;
            }
            catch (IOException)
            {
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
            if (index < 4)
            {
                return null;
            }
            Array.Resize<EraInfo>(ref array, index);
            Array.Sort<EraInfo>(array, new Comparison<EraInfo>(JapaneseCalendar.CompareEraRanges));
            for (int i = 0; i < array.Length; i++)
            {
                array[i].era = array.Length - i;
                if (i == 0)
                {
                    array[0].maxEraYear = 0x270f - array[0].yearOffset;
                }
                else
                {
                    array[i].maxEraYear = (array[i - 1].yearOffset + 1) - array[i].yearOffset;
                }
            }
            return array;
        }

        [ComVisible(false)]
        public override int GetLeapMonth(int year, int era)
        {
            return this.helper.GetLeapMonth(year, era);
        }

        public override int GetMonth(DateTime time)
        {
            return this.helper.GetMonth(time);
        }

        public override int GetMonthsInYear(int year, int era)
        {
            return this.helper.GetMonthsInYear(year, era);
        }

        [ComVisible(false)]
        public override int GetWeekOfYear(DateTime time, CalendarWeekRule rule, DayOfWeek firstDayOfWeek)
        {
            return this.helper.GetWeekOfYear(time, rule, firstDayOfWeek);
        }

        public override int GetYear(DateTime time)
        {
            return this.helper.GetYear(time);
        }

        public override bool IsLeapDay(int year, int month, int day, int era)
        {
            return this.helper.IsLeapDay(year, month, day, era);
        }

        public override bool IsLeapMonth(int year, int month, int era)
        {
            return this.helper.IsLeapMonth(year, month, era);
        }

        public override bool IsLeapYear(int year, int era)
        {
            return this.helper.IsLeapYear(year, era);
        }

        internal override bool IsValidYear(int year, int era)
        {
            return this.helper.IsValidYear(year, era);
        }

        public override DateTime ToDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, int era)
        {
            return this.helper.ToDateTime(year, month, day, hour, minute, second, millisecond, era);
        }

        public override int ToFourDigitYear(int year)
        {
            if (year <= 0)
            {
                throw new ArgumentOutOfRangeException("year", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
            }
            if (year > this.helper.MaxYear)
            {
                throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 1, this.helper.MaxYear }));
            }
            return year;
        }

        [ComVisible(false)]
        public override CalendarAlgorithmType AlgorithmType
        {
            get
            {
                return CalendarAlgorithmType.SolarCalendar;
            }
        }

        public override int[] Eras
        {
            get
            {
                return this.helper.Eras;
            }
        }

        internal override int ID
        {
            get
            {
                return 3;
            }
        }

        [ComVisible(false)]
        public override DateTime MaxSupportedDateTime
        {
            get
            {
                return DateTime.MaxValue;
            }
        }

        [ComVisible(false)]
        public override DateTime MinSupportedDateTime
        {
            get
            {
                return calendarMinValue;
            }
        }

        public override int TwoDigitYearMax
        {
            get
            {
                if (base.twoDigitYearMax == -1)
                {
                    base.twoDigitYearMax = Calendar.GetSystemTwoDigitYearSetting(this.ID, 0x63);
                }
                return base.twoDigitYearMax;
            }
            set
            {
                base.VerifyWritable();
                if ((value < 0x63) || (value > this.helper.MaxYear))
                {
                    throw new ArgumentOutOfRangeException("year", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { 0x63, this.helper.MaxYear }));
                }
                base.twoDigitYearMax = value;
            }
        }
    }
}

