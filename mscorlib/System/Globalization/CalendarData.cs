namespace System.Globalization
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    internal class CalendarData
    {
        internal bool bUseUserOverrides;
        internal int iCurrentEra;
        internal static CalendarData Invariant;
        internal int iTwoDigitYearMax;
        internal const int MAX_CALENDARS = 0x17;
        internal string[] saAbbrevDayNames;
        internal string[] saAbbrevEnglishEraNames;
        internal string[] saAbbrevEraNames;
        internal string[] saAbbrevMonthGenitiveNames;
        internal string[] saAbbrevMonthNames;
        internal string[] saDayNames;
        internal string[] saEraNames;
        internal string[] saLeapYearMonthNames;
        internal string[] saLongDates;
        internal string[] saMonthGenitiveNames;
        internal string[] saMonthNames;
        internal string[] saShortDates;
        internal string[] saSuperShortDayNames;
        internal string[] saYearMonths;
        internal string sMonthDay;
        internal string sNativeName;

        static CalendarData()
        {
            CalendarData data;
            data = new CalendarData {
                sNativeName = "Gregorian Calendar",
                iTwoDigitYearMax = 0x7ed,
                iCurrentEra = 1,
                saShortDates = new string[] { "MM/dd/yyyy", "yyyy-MM-dd" },
                saLongDates = new string[] { "dddd, dd MMMM yyyy" },
                saYearMonths = new string[] { "yyyy MMMM" },
                sMonthDay = "MMMM dd",
                saEraNames = new string[] { "A.D." },
                saAbbrevEraNames = new string[] { "AD" },
                saAbbrevEnglishEraNames = new string[] { "AD" },
                saDayNames = new string[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" },
                saAbbrevDayNames = new string[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" },
                saSuperShortDayNames = new string[] { "Su", "Mo", "Tu", "We", "Th", "Fr", "Sa" },
                saMonthNames = new string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December", string.Empty },
                saAbbrevMonthNames = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", string.Empty },
                saMonthGenitiveNames = data.saMonthNames,
                saAbbrevMonthGenitiveNames = data.saAbbrevMonthNames,
                saLeapYearMonthNames = data.saMonthNames,
                bUseUserOverrides = false
            };
            Invariant = data;
        }

        private CalendarData()
        {
            this.iTwoDigitYearMax = 0x7ed;
        }

        internal CalendarData(string localeName, int calendarId, bool bUseUserOverrides)
        {
            this.iTwoDigitYearMax = 0x7ed;
            this.bUseUserOverrides = bUseUserOverrides;
            if (!nativeGetCalendarData(this, localeName, calendarId))
            {
                if (this.sNativeName == null)
                {
                    this.sNativeName = string.Empty;
                }
                if (this.saShortDates == null)
                {
                    this.saShortDates = Invariant.saShortDates;
                }
                if (this.saYearMonths == null)
                {
                    this.saYearMonths = Invariant.saYearMonths;
                }
                if (this.saLongDates == null)
                {
                    this.saLongDates = Invariant.saLongDates;
                }
                if (this.sMonthDay == null)
                {
                    this.sMonthDay = Invariant.sMonthDay;
                }
                if (this.saEraNames == null)
                {
                    this.saEraNames = Invariant.saEraNames;
                }
                if (this.saAbbrevEraNames == null)
                {
                    this.saAbbrevEraNames = Invariant.saAbbrevEraNames;
                }
                if (this.saAbbrevEnglishEraNames == null)
                {
                    this.saAbbrevEnglishEraNames = Invariant.saAbbrevEnglishEraNames;
                }
                if (this.saDayNames == null)
                {
                    this.saDayNames = Invariant.saDayNames;
                }
                if (this.saAbbrevDayNames == null)
                {
                    this.saAbbrevDayNames = Invariant.saAbbrevDayNames;
                }
                if (this.saSuperShortDayNames == null)
                {
                    this.saSuperShortDayNames = Invariant.saSuperShortDayNames;
                }
                if (this.saMonthNames == null)
                {
                    this.saMonthNames = Invariant.saMonthNames;
                }
                if (this.saAbbrevMonthNames == null)
                {
                    this.saAbbrevMonthNames = Invariant.saAbbrevMonthNames;
                }
            }
            this.saShortDates = CultureData.ReescapeWin32Strings(this.saShortDates);
            this.saLongDates = CultureData.ReescapeWin32Strings(this.saLongDates);
            this.saYearMonths = CultureData.ReescapeWin32Strings(this.saYearMonths);
            this.sMonthDay = CultureData.ReescapeWin32String(this.sMonthDay);
            if (((ushort) calendarId) == 4)
            {
                if (CultureInfo.IsTaiwanSku)
                {
                    this.sNativeName = "中華民國曆";
                }
                else
                {
                    this.sNativeName = string.Empty;
                }
            }
            if ((this.saMonthGenitiveNames == null) || string.IsNullOrEmpty(this.saMonthGenitiveNames[0]))
            {
                this.saMonthGenitiveNames = this.saMonthNames;
            }
            if ((this.saAbbrevMonthGenitiveNames == null) || string.IsNullOrEmpty(this.saAbbrevMonthGenitiveNames[0]))
            {
                this.saAbbrevMonthGenitiveNames = this.saAbbrevMonthNames;
            }
            if ((this.saLeapYearMonthNames == null) || string.IsNullOrEmpty(this.saLeapYearMonthNames[0]))
            {
                this.saLeapYearMonthNames = this.saMonthNames;
            }
            this.InitializeEraNames(localeName, calendarId);
            this.InitializeAbbreviatedEraNames(localeName, calendarId);
            if (calendarId == 3)
            {
                this.saAbbrevEnglishEraNames = JapaneseCalendar.EnglishEraNames();
            }
            else
            {
                this.saAbbrevEnglishEraNames = new string[] { "" };
            }
            this.iCurrentEra = this.saEraNames.Length;
        }

        private static string CalendarIdToCultureName(int calendarId)
        {
            switch (calendarId)
            {
                case 2:
                    return "fa-IR";

                case 3:
                    return "ja-JP";

                case 4:
                    return "zh-TW";

                case 5:
                    return "ko-KR";

                case 6:
                case 10:
                case 0x17:
                    return "ar-SA";

                case 7:
                    return "th-TH";

                case 8:
                    return "he-IL";

                case 9:
                    return "ar-DZ";

                case 11:
                case 12:
                    return "ar-IQ";
            }
            return "en-US";
        }

        private static int FindUnescapedCharacter(string s, char charToFind)
        {
            bool flag = false;
            int length = s.Length;
            for (int i = 0; i < length; i++)
            {
                char ch = s[i];
                switch (ch)
                {
                    case '\'':
                        flag = !flag;
                        break;

                    case '\\':
                        i++;
                        break;

                    default:
                        if (!flag && (charToFind == ch))
                        {
                            return i;
                        }
                        break;
                }
            }
            return -1;
        }

        internal void FixupWin7MonthDaySemicolonBug()
        {
            int length = FindUnescapedCharacter(this.sMonthDay, ';');
            if (length > 0)
            {
                this.sMonthDay = this.sMonthDay.Substring(0, length);
            }
        }

        internal static CalendarData GetCalendarData(int calendarId)
        {
            return CultureInfo.GetCultureInfo(CalendarIdToCultureName(calendarId)).m_cultureData.GetCalendar(calendarId);
        }

        private void InitializeAbbreviatedEraNames(string localeName, int calendarId)
        {
            switch (((CalendarId) ((ushort) calendarId)))
            {
                case CalendarId.GREGORIAN:
                    if (((this.saAbbrevEraNames == null) || (this.saAbbrevEraNames.Length == 0)) || string.IsNullOrEmpty(this.saAbbrevEraNames[0]))
                    {
                        this.saAbbrevEraNames = new string[] { "AD" };
                    }
                    return;

                case CalendarId.GREGORIAN_US:
                case CalendarId.JULIAN:
                    this.saAbbrevEraNames = new string[] { "AD" };
                    return;

                case CalendarId.JAPAN:
                case CalendarId.JAPANESELUNISOLAR:
                    this.saAbbrevEraNames = JapaneseCalendar.AbbrevEraNames();
                    return;

                case CalendarId.TAIWAN:
                    this.saAbbrevEraNames = new string[1];
                    if (this.saEraNames[0].Length != 4)
                    {
                        this.saAbbrevEraNames[0] = this.saEraNames[0];
                        return;
                    }
                    this.saAbbrevEraNames[0] = this.saEraNames[0].Substring(2, 2);
                    return;

                case CalendarId.HIJRI:
                case CalendarId.UMALQURA:
                    if (localeName == "dv-MV")
                    {
                        this.saAbbrevEraNames = new string[] { "ހ." };
                        return;
                    }
                    this.saAbbrevEraNames = new string[] { "هـ" };
                    return;
            }
            this.saAbbrevEraNames = this.saEraNames;
        }

        private void InitializeEraNames(string localeName, int calendarId)
        {
            switch (((CalendarId) ((ushort) calendarId)))
            {
                case CalendarId.GREGORIAN:
                    if (((this.saEraNames == null) || (this.saEraNames.Length == 0)) || string.IsNullOrEmpty(this.saEraNames[0]))
                    {
                        this.saEraNames = new string[] { "A.D." };
                    }
                    return;

                case CalendarId.GREGORIAN_US:
                case CalendarId.JULIAN:
                    this.saEraNames = new string[] { "A.D." };
                    return;

                case CalendarId.JAPAN:
                case CalendarId.JAPANESELUNISOLAR:
                    this.saEraNames = JapaneseCalendar.EraNames();
                    return;

                case CalendarId.TAIWAN:
                    if (!CultureInfo.IsTaiwanSku)
                    {
                        this.saEraNames = new string[] { string.Empty };
                        return;
                    }
                    this.saEraNames = new string[] { "中華民國" };
                    return;

                case CalendarId.KOREA:
                    this.saEraNames = new string[] { "단기" };
                    return;

                case CalendarId.HIJRI:
                case CalendarId.UMALQURA:
                    if (!(localeName == "dv-MV"))
                    {
                        this.saEraNames = new string[] { "بعد الهجرة" };
                        return;
                    }
                    this.saEraNames = new string[] { "ހިޖްރީ" };
                    return;

                case CalendarId.THAI:
                    this.saEraNames = new string[] { "พ.ศ." };
                    return;

                case CalendarId.HEBREW:
                    this.saEraNames = new string[] { "C.E." };
                    return;

                case CalendarId.GREGORIAN_ME_FRENCH:
                    this.saEraNames = new string[] { "ap. J.-C." };
                    return;

                case CalendarId.GREGORIAN_ARABIC:
                case CalendarId.GREGORIAN_XLIT_ENGLISH:
                case CalendarId.GREGORIAN_XLIT_FRENCH:
                    this.saEraNames = new string[] { "م" };
                    return;
            }
            this.saEraNames = Invariant.saEraNames;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        private static extern bool nativeGetCalendarData(CalendarData data, string localeName, int calendar);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        internal static extern int nativeGetCalendars(string localeName, bool useUserOverride, [In, Out] int[] calendars);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern int nativeGetTwoDigitYearMax(int calID);
    }
}

