namespace System.Globalization
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Resources;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Threading;

    internal class CultureData
    {
        private bool bFramework;
        private bool bNeutral;
        private bool bUseOverrides;
        private bool bWin32Installed;
        private CalendarData[] calendars;
        private string fontSignature;
        private int iCurrency;
        private int iCurrencyDigits;
        private int iDefaultAnsiCodePage = -1;
        private int iDefaultEbcdicCodePage = -1;
        private int iDefaultMacCodePage = -1;
        private int iDefaultOemCodePage = -1;
        private int iDigits;
        private int iDigitSubstitution;
        private int iFirstDayOfWeek = -1;
        private int iFirstWeekOfYear = -1;
        private int iGeoId = -1;
        private int iInputLanguageHandle = -1;
        private int iLanguage;
        private int iLeadingZeros;
        private int iMeasure = -1;
        private int iNegativeCurrency;
        private int iNegativeNumber;
        private int iNegativePercent = -1;
        private static readonly bool init = Init();
        internal static CultureData Invariant;
        private int iPositivePercent = -1;
        private int iReadingLayout = -1;
        private const uint LOCALE_FONTSIGNATURE = 0x58;
        private const uint LOCALE_ICALENDARTYPE = 0x1009;
        private const uint LOCALE_ICENTURY = 0x24;
        private const uint LOCALE_ICOUNTRY = 5;
        private const uint LOCALE_ICURRDIGITS = 0x19;
        private const uint LOCALE_ICURRENCY = 0x1b;
        private const uint LOCALE_IDATE = 0x21;
        private const uint LOCALE_IDAYLZERO = 0x26;
        private const uint LOCALE_IDEFAULTANSICODEPAGE = 0x1004;
        private const uint LOCALE_IDEFAULTCODEPAGE = 11;
        private const uint LOCALE_IDEFAULTCOUNTRY = 10;
        private const uint LOCALE_IDEFAULTEBCDICCODEPAGE = 0x1012;
        private const uint LOCALE_IDEFAULTLANGUAGE = 9;
        private const uint LOCALE_IDEFAULTMACCODEPAGE = 0x1011;
        private const uint LOCALE_IDIGITS = 0x11;
        private const uint LOCALE_IDIGITSUBSTITUTION = 0x1014;
        private const uint LOCALE_IFIRSTDAYOFWEEK = 0x100c;
        private const uint LOCALE_IFIRSTWEEKOFYEAR = 0x100d;
        private const uint LOCALE_IGEOID = 0x5b;
        private const uint LOCALE_IINTLCURRDIGITS = 0x1a;
        private const uint LOCALE_ILDATE = 0x22;
        private const uint LOCALE_ILZERO = 0x12;
        private const uint LOCALE_IMEASURE = 13;
        private const uint LOCALE_IMONLZERO = 0x27;
        private const uint LOCALE_INEGATIVEPERCENT = 0x74;
        private const uint LOCALE_INEGCURR = 0x1c;
        private const uint LOCALE_INEGNUMBER = 0x1010;
        private const uint LOCALE_INEGSEPBYSPACE = 0x57;
        private const uint LOCALE_INEGSIGNPOSN = 0x53;
        private const uint LOCALE_INEGSYMPRECEDES = 0x56;
        private const uint LOCALE_INEUTRAL = 0x71;
        private const uint LOCALE_IOPTIONALCALENDAR = 0x100b;
        private const uint LOCALE_IPAPERSIZE = 0x100a;
        private const uint LOCALE_IPOSITIVEPERCENT = 0x75;
        private const uint LOCALE_IPOSSEPBYSPACE = 0x55;
        private const uint LOCALE_IPOSSIGNPOSN = 0x52;
        private const uint LOCALE_IPOSSYMPRECEDES = 0x54;
        private const uint LOCALE_IREADINGLAYOUT = 0x70;
        private const uint LOCALE_ITIME = 0x23;
        private const uint LOCALE_ITIMEMARKPOSN = 0x1005;
        private const uint LOCALE_ITLZERO = 0x25;
        private const uint LOCALE_NOUSEROVERRIDE = 0x80000000;
        private const uint LOCALE_RETURN_GENITIVE_NAMES = 0x10000000;
        private const uint LOCALE_RETURN_NUMBER = 0x20000000;
        private const uint LOCALE_S1159 = 40;
        private const uint LOCALE_S2359 = 0x29;
        private const uint LOCALE_SABBREVCTRYNAME = 7;
        private const uint LOCALE_SABBREVDAYNAME1 = 0x31;
        private const uint LOCALE_SABBREVDAYNAME2 = 50;
        private const uint LOCALE_SABBREVDAYNAME3 = 0x33;
        private const uint LOCALE_SABBREVDAYNAME4 = 0x34;
        private const uint LOCALE_SABBREVDAYNAME5 = 0x35;
        private const uint LOCALE_SABBREVDAYNAME6 = 0x36;
        private const uint LOCALE_SABBREVDAYNAME7 = 0x37;
        private const uint LOCALE_SABBREVLANGNAME = 3;
        private const uint LOCALE_SABBREVMONTHNAME1 = 0x44;
        private const uint LOCALE_SABBREVMONTHNAME10 = 0x4d;
        private const uint LOCALE_SABBREVMONTHNAME11 = 0x4e;
        private const uint LOCALE_SABBREVMONTHNAME12 = 0x4f;
        private const uint LOCALE_SABBREVMONTHNAME13 = 0x100f;
        private const uint LOCALE_SABBREVMONTHNAME2 = 0x45;
        private const uint LOCALE_SABBREVMONTHNAME3 = 70;
        private const uint LOCALE_SABBREVMONTHNAME4 = 0x47;
        private const uint LOCALE_SABBREVMONTHNAME5 = 0x48;
        private const uint LOCALE_SABBREVMONTHNAME6 = 0x49;
        private const uint LOCALE_SABBREVMONTHNAME7 = 0x4a;
        private const uint LOCALE_SABBREVMONTHNAME8 = 0x4b;
        private const uint LOCALE_SABBREVMONTHNAME9 = 0x4c;
        private const uint LOCALE_SCONSOLEFALLBACKNAME = 110;
        private const uint LOCALE_SCURRENCY = 20;
        private const uint LOCALE_SDATE = 0x1d;
        private const uint LOCALE_SDAYNAME1 = 0x2a;
        private const uint LOCALE_SDAYNAME2 = 0x2b;
        private const uint LOCALE_SDAYNAME3 = 0x2c;
        private const uint LOCALE_SDAYNAME4 = 0x2d;
        private const uint LOCALE_SDAYNAME5 = 0x2e;
        private const uint LOCALE_SDAYNAME6 = 0x2f;
        private const uint LOCALE_SDAYNAME7 = 0x30;
        private const uint LOCALE_SDECIMAL = 14;
        private const uint LOCALE_SDURATION = 0x5d;
        private const uint LOCALE_SENGCURRNAME = 0x1007;
        private const uint LOCALE_SENGLISHCOUNTRYNAME = 0x1002;
        private const uint LOCALE_SENGLISHDISPLAYNAME = 0x72;
        private const uint LOCALE_SENGLISHLANGUAGENAME = 0x1001;
        private const uint LOCALE_SGROUPING = 0x10;
        private const uint LOCALE_SINTLSYMBOL = 0x15;
        private const uint LOCALE_SISO3166CTRYNAME = 90;
        private const uint LOCALE_SISO3166CTRYNAME2 = 0x68;
        private const uint LOCALE_SISO639LANGNAME = 0x59;
        private const uint LOCALE_SISO639LANGNAME2 = 0x67;
        private const uint LOCALE_SKEYBOARDSTOINSTALL = 0x5e;
        private const uint LOCALE_SLIST = 12;
        private const uint LOCALE_SLOCALIZEDCOUNTRYNAME = 6;
        private const uint LOCALE_SLOCALIZEDDISPLAYNAME = 2;
        private const uint LOCALE_SLOCALIZEDLANGUAGENAME = 0x6f;
        private const uint LOCALE_SLONGDATE = 0x20;
        private const uint LOCALE_SMONDECIMALSEP = 0x16;
        private const uint LOCALE_SMONGROUPING = 0x18;
        private const uint LOCALE_SMONTHDAY = 120;
        private const uint LOCALE_SMONTHNAME1 = 0x38;
        private const uint LOCALE_SMONTHNAME10 = 0x41;
        private const uint LOCALE_SMONTHNAME11 = 0x42;
        private const uint LOCALE_SMONTHNAME12 = 0x43;
        private const uint LOCALE_SMONTHNAME13 = 0x100e;
        private const uint LOCALE_SMONTHNAME2 = 0x39;
        private const uint LOCALE_SMONTHNAME3 = 0x3a;
        private const uint LOCALE_SMONTHNAME4 = 0x3b;
        private const uint LOCALE_SMONTHNAME5 = 60;
        private const uint LOCALE_SMONTHNAME6 = 0x3d;
        private const uint LOCALE_SMONTHNAME7 = 0x3e;
        private const uint LOCALE_SMONTHNAME8 = 0x3f;
        private const uint LOCALE_SMONTHNAME9 = 0x40;
        private const uint LOCALE_SMONTHOUSANDSEP = 0x17;
        private const uint LOCALE_SNAME = 0x5c;
        private const uint LOCALE_SNAN = 0x69;
        private const uint LOCALE_SNATIVECOUNTRYNAME = 8;
        private const uint LOCALE_SNATIVECURRNAME = 0x1008;
        private const uint LOCALE_SNATIVEDIGITS = 0x13;
        private const uint LOCALE_SNATIVEDISPLAYNAME = 0x73;
        private const uint LOCALE_SNATIVELANGUAGENAME = 4;
        private const uint LOCALE_SNEGATIVESIGN = 0x51;
        private const uint LOCALE_SNEGINFINITY = 0x6b;
        private const uint LOCALE_SOPENTYPELANGUAGETAG = 0x7a;
        private const uint LOCALE_SPARENT = 0x6d;
        private const uint LOCALE_SPERCENT = 0x76;
        private const uint LOCALE_SPERMILLE = 0x77;
        private const uint LOCALE_SPOSINFINITY = 0x6a;
        private const uint LOCALE_SPOSITIVESIGN = 80;
        private const uint LOCALE_SSCRIPTS = 0x6c;
        private const uint LOCALE_SSHORTDATE = 0x1f;
        private const uint LOCALE_SSHORTESTDAYNAME1 = 0x60;
        private const uint LOCALE_SSHORTESTDAYNAME2 = 0x61;
        private const uint LOCALE_SSHORTESTDAYNAME3 = 0x62;
        private const uint LOCALE_SSHORTESTDAYNAME4 = 0x63;
        private const uint LOCALE_SSHORTESTDAYNAME5 = 100;
        private const uint LOCALE_SSHORTESTDAYNAME6 = 0x65;
        private const uint LOCALE_SSHORTESTDAYNAME7 = 0x66;
        private const uint LOCALE_SSHORTTIME = 0x79;
        private const uint LOCALE_SSORTLOCALE = 0x7b;
        private const uint LOCALE_SSORTNAME = 0x1013;
        private const uint LOCALE_STHOUSAND = 15;
        private const uint LOCALE_STIME = 30;
        private const uint LOCALE_STIMEFORMAT = 0x1003;
        private const uint LOCALE_SYEARMONTH = 0x1006;
        internal static ResourceSet MscorlibResourceSet;
        private static Dictionary<string, string> RegionNames;
        private static Dictionary<string, CultureData> s_cachedCultures;
        private static Dictionary<string, CultureData> s_cachedRegions;
        private static string s_RegionKey = @"System\CurrentControlSet\Control\Nls\RegionMapping";
        internal static string[] s_replacementCultureNames;
        private static readonly Version s_win7Version = new Version(6, 1);
        private string sAbbrevCountry;
        private string sAbbrevLang;
        private string[] saDurationFormats;
        private string[] saLongTimes;
        private string sAM1159;
        private string[] saNativeDigits;
        private string[] saShortTimes;
        private string sCompareInfo;
        private string sConsoleFallbackName;
        private string sCurrency;
        private string sDecimalSeparator;
        private string sEnglishCountry;
        private string sEnglishCurrency;
        private string sEnglishDisplayName;
        private string sEnglishLanguage;
        private string sIntlMonetarySymbol;
        private string sISO3166CountryName;
        private string sISO3166CountryName2;
        private string sISO639Language;
        private string sISO639Language2;
        private string sKeyboardsToInstall;
        private string sListSeparator;
        private string sLocalizedCountry;
        private string sLocalizedDisplayName;
        private string sLocalizedLanguage;
        private string sMonetaryDecimal;
        private string sMonetaryThousand;
        private string sName;
        private string sNaN;
        private string sNativeCountry;
        private string sNativeCurrency;
        private string sNativeDisplayName;
        private string sNativeLanguage;
        private string sNegativeInfinity;
        private string sNegativeSign;
        private string sParent;
        internal static CultureInfo[] specificCultures;
        private string sPercent;
        private string sPerMille;
        private string sPM2359;
        private string sPositiveInfinity;
        private string sPositiveSign;
        private string sRealName;
        private string sRegionName;
        private string sScripts;
        private string sSpecificCulture;
        private string sTextInfo;
        private string sThousandSeparator;
        private string sWindowsName;
        internal const uint TIME_NOSECONDS = 2;
        private const int undef = -1;
        private int[] waCalendars;
        private int[] waGrouping;
        private int[] waMonetaryGrouping;

        static CultureData()
        {
            CultureData cultureData = new CultureData {
                bUseOverrides = false,
                sRealName = ""
            };
            nativeInitCultureData(cultureData);
            cultureData.bUseOverrides = false;
            cultureData.sRealName = "";
            cultureData.sWindowsName = "";
            cultureData.sName = "";
            cultureData.sParent = "";
            cultureData.bNeutral = false;
            cultureData.bFramework = true;
            cultureData.sEnglishDisplayName = "Invariant Language (Invariant Country)";
            cultureData.sNativeDisplayName = "Invariant Language (Invariant Country)";
            cultureData.sSpecificCulture = "";
            cultureData.sISO639Language = "iv";
            cultureData.sLocalizedLanguage = "Invariant Language";
            cultureData.sEnglishLanguage = "Invariant Language";
            cultureData.sNativeLanguage = "Invariant Language";
            cultureData.sRegionName = "IV";
            cultureData.iGeoId = 0xf4;
            cultureData.sEnglishCountry = "Invariant Country";
            cultureData.sNativeCountry = "Invariant Country";
            cultureData.sISO3166CountryName = "IV";
            cultureData.sPositiveSign = "+";
            cultureData.sNegativeSign = "-";
            cultureData.saNativeDigits = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            cultureData.iDigitSubstitution = 1;
            cultureData.iLeadingZeros = 1;
            cultureData.iDigits = 2;
            cultureData.iNegativeNumber = 1;
            cultureData.waGrouping = new int[] { 3 };
            cultureData.sDecimalSeparator = ".";
            cultureData.sThousandSeparator = ",";
            cultureData.sNaN = "NaN";
            cultureData.sPositiveInfinity = "Infinity";
            cultureData.sNegativeInfinity = "-Infinity";
            cultureData.iNegativePercent = 0;
            cultureData.iPositivePercent = 0;
            cultureData.sPercent = "%";
            cultureData.sPerMille = "‰";
            cultureData.sCurrency = "\x00a4";
            cultureData.sIntlMonetarySymbol = "XDR";
            cultureData.sEnglishCurrency = "International Monetary Fund";
            cultureData.sNativeCurrency = "International Monetary Fund";
            cultureData.iCurrencyDigits = 2;
            cultureData.iCurrency = 0;
            cultureData.iNegativeCurrency = 0;
            cultureData.waMonetaryGrouping = new int[] { 3 };
            cultureData.sMonetaryDecimal = ".";
            cultureData.sMonetaryThousand = ",";
            cultureData.iMeasure = 0;
            cultureData.sListSeparator = ",";
            cultureData.sAM1159 = "AM";
            cultureData.sPM2359 = "PM";
            cultureData.saLongTimes = new string[] { "HH:mm:ss" };
            cultureData.saShortTimes = new string[] { "HH:mm", "hh:mm tt", "H:mm", "h:mm tt" };
            cultureData.saDurationFormats = new string[] { "HH:mm:ss" };
            cultureData.iFirstDayOfWeek = 0;
            cultureData.iFirstWeekOfYear = 0;
            cultureData.waCalendars = new int[] { 1 };
            cultureData.calendars = new CalendarData[0x17];
            cultureData.calendars[0] = CalendarData.Invariant;
            cultureData.iReadingLayout = 0;
            cultureData.sTextInfo = "";
            cultureData.sCompareInfo = "";
            cultureData.sScripts = "Latn;";
            cultureData.iLanguage = 0x7f;
            cultureData.iDefaultAnsiCodePage = 0x4e4;
            cultureData.iDefaultOemCodePage = 0x1b5;
            cultureData.iDefaultMacCodePage = 0x2710;
            cultureData.iDefaultEbcdicCodePage = 0x25;
            cultureData.sAbbrevLang = "IVL";
            cultureData.sAbbrevCountry = "IVC";
            cultureData.sISO639Language2 = "ivl";
            cultureData.sISO3166CountryName2 = "ivc";
            cultureData.iInputLanguageHandle = 0x7f;
            cultureData.sConsoleFallbackName = "";
            cultureData.sKeyboardsToInstall = "0409:00000409";
            Invariant = cultureData;
        }

        internal string[] AbbrevEraNames(int calendarId)
        {
            return this.GetCalendar(calendarId).saAbbrevEraNames;
        }

        internal string[] AbbreviatedDayNames(int calendarId)
        {
            return this.GetCalendar(calendarId).saAbbrevDayNames;
        }

        internal string[] AbbreviatedEnglishEraNames(int calendarId)
        {
            return this.GetCalendar(calendarId).saAbbrevEnglishEraNames;
        }

        internal string[] AbbreviatedGenitiveMonthNames(int calendarId)
        {
            return this.GetCalendar(calendarId).saAbbrevMonthGenitiveNames;
        }

        internal string[] AbbreviatedMonthNames(int calendarId)
        {
            return this.GetCalendar(calendarId).saAbbrevMonthNames;
        }

        private string[] AdjustShortTimesForMac(string[] shortTimes)
        {
            return shortTimes;
        }

        internal static string AnsiToLower(string testString)
        {
            StringBuilder builder = new StringBuilder(testString.Length);
            for (int i = 0; i < testString.Length; i++)
            {
                char ch = testString[i];
                builder.Append(((ch <= 'Z') && (ch >= 'A')) ? ((char) ((ch - 'A') + 0x61)) : ch);
            }
            return builder.ToString();
        }

        internal string CalendarName(int calendarId)
        {
            return this.GetCalendar(calendarId).sNativeName;
        }

        internal static void ClearCachedData()
        {
            s_cachedCultures = null;
            s_cachedRegions = null;
            s_replacementCultureNames = null;
        }

        private static int ConvertFirstDayOfWeekMonToSun(int iTemp)
        {
            iTemp++;
            if (iTemp > 6)
            {
                iTemp = 0;
            }
            return iTemp;
        }

        private static int[] ConvertWin32GroupString(string win32Str)
        {
            int[] numArray;
            if (((win32Str == null) || (win32Str.Length == 0)) || (win32Str[0] == '0'))
            {
                return new int[] { 3 };
            }
            if (win32Str[win32Str.Length - 1] == '0')
            {
                numArray = new int[win32Str.Length / 2];
            }
            else
            {
                numArray = new int[(win32Str.Length / 2) + 2];
                numArray[numArray.Length - 1] = 0;
            }
            int num = 0;
            for (int i = 0; (num < win32Str.Length) && (i < numArray.Length); i++)
            {
                if ((win32Str[num] < '1') || (win32Str[num] > '9'))
                {
                    return new int[] { 3 };
                }
                numArray[i] = win32Str[num] - '0';
                num += 2;
            }
            return numArray;
        }

        private static CultureData CreateCultureData(string cultureName, bool useUserOverride)
        {
            CultureData data = new CultureData {
                bUseOverrides = useUserOverride,
                sRealName = cultureName
            };
            if ((!data.InitCultureData() && !data.InitCompatibilityCultureData()) && !data.InitLegacyAlternateSortData())
            {
                return null;
            }
            return data;
        }

        internal int CurrentEra(int calendarId)
        {
            return this.GetCalendar(calendarId).iCurrentEra;
        }

        internal string DateSeparator(int calendarId)
        {
            return GetDateSeparator(this.ShortDates(calendarId)[0]);
        }

        internal string[] DayNames(int calendarId)
        {
            return this.GetCalendar(calendarId).saDayNames;
        }

        private string[] DeriveShortTimesFromLong()
        {
            string[] strArray = new string[this.LongTimes.Length];
            for (int i = 0; i < this.LongTimes.Length; i++)
            {
                strArray[i] = StripSecondsFromPattern(this.LongTimes[i]);
            }
            return strArray;
        }

        private string[] DoEnumShortTimeFormats()
        {
            return ReescapeWin32Strings(nativeEnumTimeFormats(this.sWindowsName, 2, this.UseUserOverride));
        }

        private string[] DoEnumTimeFormats()
        {
            return ReescapeWin32Strings(nativeEnumTimeFormats(this.sWindowsName, 0, this.UseUserOverride));
        }

        [SecurityCritical]
        private string DoGetLocaleInfo(uint lctype)
        {
            return this.DoGetLocaleInfo(this.sWindowsName, lctype);
        }

        [SecurityCritical]
        private string DoGetLocaleInfo(string localeName, uint lctype)
        {
            if (!this.UseUserOverride)
            {
                lctype |= 0x80000000;
            }
            string str = CultureInfo.nativeGetLocaleInfoEx(localeName, lctype);
            if (str == null)
            {
                str = string.Empty;
            }
            return str;
        }

        private int DoGetLocaleInfoInt(uint lctype)
        {
            if (!this.UseUserOverride)
            {
                lctype |= 0x80000000;
            }
            return CultureInfo.nativeGetLocaleInfoExInt(this.sWindowsName, lctype);
        }

        internal string[] EraNames(int calendarId)
        {
            return this.GetCalendar(calendarId).saEraNames;
        }

        internal string[] GenitiveMonthNames(int calendarId)
        {
            return this.GetCalendar(calendarId).saMonthGenitiveNames;
        }

        internal CalendarData GetCalendar(int calendarId)
        {
            int index = calendarId - 1;
            if (this.calendars == null)
            {
                this.calendars = new CalendarData[0x17];
            }
            CalendarData data = this.calendars[index];
            if ((data == null) || this.UseUserOverride)
            {
                data = new CalendarData(this.sWindowsName, calendarId, this.UseUserOverride);
                if ((IsOsWin7OrPrior() && !this.IsUserCustomCulture) && !this.IsReplacementCulture)
                {
                    data.FixupWin7MonthDaySemicolonBug();
                }
                this.calendars[index] = data;
            }
            return data;
        }

        internal static CultureData GetCultureData(int culture, bool bUseUserOverride)
        {
            string str = null;
            CultureData cultureData = null;
            if (CompareInfo.IsLegacy20SortingBehaviorRequested)
            {
                switch (culture)
                {
                    case 0x10411:
                        str = "ja-JP_unicod";
                        break;

                    case 0x10412:
                        str = "ko-KR_unicod";
                        break;

                    case 0x20c04:
                        str = "zh-HK_stroke";
                        break;
                }
            }
            if (str == null)
            {
                str = LCIDToLocaleName(culture);
            }
            if (string.IsNullOrEmpty(str))
            {
                if (culture == 0x7f)
                {
                    return Invariant;
                }
            }
            else
            {
                string str2 = str;
                if (str2 != null)
                {
                    if (!(str2 == "zh-Hans"))
                    {
                        if (str2 == "zh-Hant")
                        {
                            str = "zh-CHT";
                        }
                    }
                    else
                    {
                        str = "zh-CHS";
                    }
                }
                cultureData = GetCultureData(str, bUseUserOverride);
            }
            if (cultureData == null)
            {
                throw new CultureNotFoundException("culture", culture, Environment.GetResourceString("Argument_CultureNotSupported"));
            }
            return cultureData;
        }

        internal static CultureData GetCultureData(string cultureName, bool useUserOverride)
        {
            if (string.IsNullOrEmpty(cultureName))
            {
                return Invariant;
            }
            string key = AnsiToLower(useUserOverride ? cultureName : (cultureName + '*'));
            Dictionary<string, CultureData> dictionary = s_cachedCultures;
            if (dictionary == null)
            {
                dictionary = new Dictionary<string, CultureData>();
            }
            else
            {
                CultureData data;
                lock (((ICollection) dictionary).SyncRoot)
                {
                    dictionary.TryGetValue(key, out data);
                }
                if (data != null)
                {
                    return data;
                }
            }
            CultureData data2 = CreateCultureData(cultureName, useUserOverride);
            if (data2 == null)
            {
                return null;
            }
            lock (((ICollection) dictionary).SyncRoot)
            {
                dictionary[key] = data2;
            }
            s_cachedCultures = dictionary;
            return data2;
        }

        [SecurityCritical]
        internal static CultureData GetCultureDataForRegion(string cultureName, bool useUserOverride)
        {
            if (string.IsNullOrEmpty(cultureName))
            {
                return Invariant;
            }
            CultureData cultureData = GetCultureData(cultureName, useUserOverride);
            if ((cultureData != null) && !cultureData.IsNeutralCulture)
            {
                return cultureData;
            }
            CultureData data2 = cultureData;
            string str = AnsiToLower(useUserOverride ? cultureName : (cultureName + '*'));
            Dictionary<string, CultureData> dictionary = s_cachedRegions;
            if (dictionary == null)
            {
                dictionary = new Dictionary<string, CultureData>();
            }
            else
            {
                lock (((ICollection) dictionary).SyncRoot)
                {
                    dictionary.TryGetValue(str, out cultureData);
                }
                if (cultureData != null)
                {
                    return cultureData;
                }
            }
            try
            {
                RegistryKey key = Registry.LocalMachine.InternalOpenSubKey(s_RegionKey, false);
                if (key != null)
                {
                    try
                    {
                        object obj2 = key.InternalGetValue(cultureName, null, false, false);
                        if (obj2 != null)
                        {
                            cultureData = GetCultureData(obj2.ToString(), useUserOverride);
                        }
                    }
                    finally
                    {
                        key.Close();
                    }
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (ArgumentException)
            {
            }
            if (((cultureData == null) || cultureData.IsNeutralCulture) && RegionNames.ContainsKey(cultureName))
            {
                cultureData = GetCultureData(RegionNames[cultureName], useUserOverride);
            }
            if ((cultureData == null) || cultureData.IsNeutralCulture)
            {
                CultureInfo[] specificCultures = SpecificCultures;
                for (int i = 0; i < specificCultures.Length; i++)
                {
                    if (string.Compare(specificCultures[i].m_cultureData.SREGIONNAME, cultureName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        cultureData = specificCultures[i].m_cultureData;
                        break;
                    }
                }
            }
            if ((cultureData != null) && !cultureData.IsNeutralCulture)
            {
                lock (((ICollection) dictionary).SyncRoot)
                {
                    dictionary[str] = cultureData;
                }
                s_cachedRegions = dictionary;
                return cultureData;
            }
            return data2;
        }

        [SecuritySafeCritical]
        internal static CultureInfo[] GetCultures(CultureTypes types)
        {
            if ((types <= 0) || ((types & ~(CultureTypes.FrameworkCultures | CultureTypes.WindowsOnlyCultures | CultureTypes.ReplacementCultures | CultureTypes.UserCustomCulture | CultureTypes.AllCultures)) != 0))
            {
                throw new ArgumentOutOfRangeException("types", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Range"), new object[] { CultureTypes.NeutralCultures, CultureTypes.FrameworkCultures }));
            }
            if ((types & CultureTypes.WindowsOnlyCultures) != 0)
            {
                types &= ~CultureTypes.WindowsOnlyCultures;
            }
            string[] o = null;
            if (nativeEnumCultureNames((int) types, JitHelpers.GetObjectHandleOnStack<string[]>(ref o)) == 0)
            {
                return new CultureInfo[0];
            }
            int length = o.Length;
            if ((types & (CultureTypes.FrameworkCultures | CultureTypes.NeutralCultures)) != 0)
            {
                length += 2;
            }
            CultureInfo[] infoArray = new CultureInfo[length];
            for (int i = 0; i < o.Length; i++)
            {
                infoArray[i] = new CultureInfo(o[i]);
            }
            if ((types & (CultureTypes.FrameworkCultures | CultureTypes.NeutralCultures)) != 0)
            {
                infoArray[o.Length] = new CultureInfo("zh-CHS");
                infoArray[o.Length + 1] = new CultureInfo("zh-CHT");
            }
            return infoArray;
        }

        private static string GetDateSeparator(string format)
        {
            return GetSeparator(format, "dyM");
        }

        private static int GetIndexOfNextTokenAfterSeconds(string time, int index, out bool containsSpace)
        {
            bool flag = false;
            containsSpace = false;
            while (index < time.Length)
            {
                switch (time[index])
                {
                    case ' ':
                        containsSpace = true;
                        break;

                    case '\'':
                        flag = !flag;
                        break;

                    case 'H':
                    case 'm':
                    case 't':
                    case 'h':
                        if (!flag)
                        {
                            return index;
                        }
                        break;

                    case '\\':
                        index++;
                        if (time[index] == ' ')
                        {
                            containsSpace = true;
                        }
                        break;
                }
                index++;
            }
            containsSpace = false;
            return index;
        }

        [SecurityCritical]
        internal void GetNFIValues(NumberFormatInfo nfi)
        {
            if (this.IsInvariantCulture)
            {
                nfi.positiveSign = this.sPositiveSign;
                nfi.negativeSign = this.sNegativeSign;
                nfi.nativeDigits = this.saNativeDigits;
                nfi.digitSubstitution = this.iDigitSubstitution;
                nfi.numberGroupSeparator = this.sThousandSeparator;
                nfi.numberDecimalSeparator = this.sDecimalSeparator;
                nfi.numberDecimalDigits = this.iDigits;
                nfi.numberNegativePattern = this.iNegativeNumber;
                nfi.currencySymbol = this.sCurrency;
                nfi.currencyGroupSeparator = this.sMonetaryThousand;
                nfi.currencyDecimalSeparator = this.sMonetaryDecimal;
                nfi.currencyDecimalDigits = this.iCurrencyDigits;
                nfi.currencyNegativePattern = this.iNegativeCurrency;
                nfi.currencyPositivePattern = this.iCurrency;
                nfi.percentNegativePattern = this.INEGATIVEPERCENT;
                nfi.percentPositivePattern = this.IPOSITIVEPERCENT;
                nfi.percentSymbol = this.SPERCENT;
                nfi.perMilleSymbol = this.SPERMILLE;
                nfi.negativeInfinitySymbol = this.SNEGINFINITY;
                nfi.positiveInfinitySymbol = this.SPOSINFINITY;
                nfi.nanSymbol = this.SNAN;
            }
            else
            {
                nativeGetNumberFormatInfoValues(this.sWindowsName, nfi, this.UseUserOverride);
            }
            nfi.numberGroupSizes = this.WAGROUPING;
            nfi.currencyGroupSizes = this.WAMONGROUPING;
            nfi.percentDecimalDigits = nfi.numberDecimalDigits;
            nfi.percentDecimalSeparator = nfi.numberDecimalSeparator;
            nfi.percentGroupSizes = nfi.numberGroupSizes;
            nfi.percentGroupSeparator = nfi.numberGroupSeparator;
            if ((nfi.positiveSign == null) || (nfi.positiveSign.Length == 0))
            {
                nfi.positiveSign = "+";
            }
            if ((nfi.currencyDecimalSeparator == null) || (nfi.currencyDecimalSeparator.Length == 0))
            {
                nfi.currencyDecimalSeparator = nfi.numberDecimalSeparator;
            }
            if ((0x3a4 == this.IDEFAULTANSICODEPAGE) || (0x3b5 == this.IDEFAULTANSICODEPAGE))
            {
                nfi.ansiCurrencySymbol = @"\";
            }
        }

        private static string GetSeparator(string format, string timeParts)
        {
            int num = IndexOfTimePart(format, 0, timeParts);
            if (num != -1)
            {
                char ch = format[num];
                do
                {
                    num++;
                }
                while ((num < format.Length) && (format[num] == ch));
                int startIndex = num;
                if (startIndex < format.Length)
                {
                    int num3 = IndexOfTimePart(format, startIndex, timeParts);
                    if (num3 != -1)
                    {
                        return UnescapeNlsString(format, startIndex, num3 - 1);
                    }
                }
            }
            return string.Empty;
        }

        private static string GetTimeSeparator(string format)
        {
            return GetSeparator(format, "Hhms");
        }

        private static int IndexOfTimePart(string format, int startIndex, string timeParts)
        {
            bool flag = false;
            for (int i = startIndex; i < format.Length; i++)
            {
                if (!flag && (timeParts.IndexOf(format[i]) != -1))
                {
                    return i;
                }
                char ch = format[i];
                if (ch != '\'')
                {
                    if ((ch == '\\') && ((i + 1) < format.Length))
                    {
                        i++;
                        char ch2 = format[i];
                        if ((ch2 != '\'') && (ch2 != '\\'))
                        {
                            i--;
                        }
                    }
                }
                else
                {
                    flag = !flag;
                }
            }
            return -1;
        }

        private static bool Init()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("029", "en-029");
            dictionary.Add("AE", "ar-AE");
            dictionary.Add("AF", "prs-AF");
            dictionary.Add("AL", "sq-AL");
            dictionary.Add("AM", "hy-AM");
            dictionary.Add("AR", "es-AR");
            dictionary.Add("AT", "de-AT");
            dictionary.Add("AU", "en-AU");
            dictionary.Add("AZ", "az-Cyrl-AZ");
            dictionary.Add("BA", "bs-Latn-BA");
            dictionary.Add("BD", "bn-BD");
            dictionary.Add("BE", "nl-BE");
            dictionary.Add("BG", "bg-BG");
            dictionary.Add("BH", "ar-BH");
            dictionary.Add("BN", "ms-BN");
            dictionary.Add("BO", "es-BO");
            dictionary.Add("BR", "pt-BR");
            dictionary.Add("BY", "be-BY");
            dictionary.Add("BZ", "en-BZ");
            dictionary.Add("CA", "en-CA");
            dictionary.Add("CH", "it-CH");
            dictionary.Add("CL", "es-CL");
            dictionary.Add("CN", "zh-CN");
            dictionary.Add("CO", "es-CO");
            dictionary.Add("CR", "es-CR");
            dictionary.Add("CS", "sr-Cyrl-CS");
            dictionary.Add("CZ", "cs-CZ");
            dictionary.Add("DE", "de-DE");
            dictionary.Add("DK", "da-DK");
            dictionary.Add("DO", "es-DO");
            dictionary.Add("DZ", "ar-DZ");
            dictionary.Add("EC", "es-EC");
            dictionary.Add("EE", "et-EE");
            dictionary.Add("EG", "ar-EG");
            dictionary.Add("ES", "es-ES");
            dictionary.Add("ET", "am-ET");
            dictionary.Add("FI", "fi-FI");
            dictionary.Add("FO", "fo-FO");
            dictionary.Add("FR", "fr-FR");
            dictionary.Add("GB", "en-GB");
            dictionary.Add("GE", "ka-GE");
            dictionary.Add("GL", "kl-GL");
            dictionary.Add("GR", "el-GR");
            dictionary.Add("GT", "es-GT");
            dictionary.Add("HK", "zh-HK");
            dictionary.Add("HN", "es-HN");
            dictionary.Add("HR", "hr-HR");
            dictionary.Add("HU", "hu-HU");
            dictionary.Add("ID", "id-ID");
            dictionary.Add("IE", "en-IE");
            dictionary.Add("IL", "he-IL");
            dictionary.Add("IN", "hi-IN");
            dictionary.Add("IQ", "ar-IQ");
            dictionary.Add("IR", "fa-IR");
            dictionary.Add("IS", "is-IS");
            dictionary.Add("IT", "it-IT");
            dictionary.Add("IV", "");
            dictionary.Add("JM", "en-JM");
            dictionary.Add("JO", "ar-JO");
            dictionary.Add("JP", "ja-JP");
            dictionary.Add("KE", "sw-KE");
            dictionary.Add("KG", "ky-KG");
            dictionary.Add("KH", "km-KH");
            dictionary.Add("KR", "ko-KR");
            dictionary.Add("KW", "ar-KW");
            dictionary.Add("KZ", "kk-KZ");
            dictionary.Add("LA", "lo-LA");
            dictionary.Add("LB", "ar-LB");
            dictionary.Add("LI", "de-LI");
            dictionary.Add("LK", "si-LK");
            dictionary.Add("LT", "lt-LT");
            dictionary.Add("LU", "lb-LU");
            dictionary.Add("LV", "lv-LV");
            dictionary.Add("LY", "ar-LY");
            dictionary.Add("MA", "ar-MA");
            dictionary.Add("MC", "fr-MC");
            dictionary.Add("ME", "sr-Latn-ME");
            dictionary.Add("MK", "mk-MK");
            dictionary.Add("MN", "mn-MN");
            dictionary.Add("MO", "zh-MO");
            dictionary.Add("MT", "mt-MT");
            dictionary.Add("MV", "dv-MV");
            dictionary.Add("MX", "es-MX");
            dictionary.Add("MY", "ms-MY");
            dictionary.Add("NG", "ig-NG");
            dictionary.Add("NI", "es-NI");
            dictionary.Add("NL", "nl-NL");
            dictionary.Add("NO", "nn-NO");
            dictionary.Add("NP", "ne-NP");
            dictionary.Add("NZ", "en-NZ");
            dictionary.Add("OM", "ar-OM");
            dictionary.Add("PA", "es-PA");
            dictionary.Add("PE", "es-PE");
            dictionary.Add("PH", "en-PH");
            dictionary.Add("PK", "ur-PK");
            dictionary.Add("PL", "pl-PL");
            dictionary.Add("PR", "es-PR");
            dictionary.Add("PT", "pt-PT");
            dictionary.Add("PY", "es-PY");
            dictionary.Add("QA", "ar-QA");
            dictionary.Add("RO", "ro-RO");
            dictionary.Add("RS", "sr-Latn-RS");
            dictionary.Add("RU", "ru-RU");
            dictionary.Add("RW", "rw-RW");
            dictionary.Add("SA", "ar-SA");
            dictionary.Add("SE", "sv-SE");
            dictionary.Add("SG", "zh-SG");
            dictionary.Add("SI", "sl-SI");
            dictionary.Add("SK", "sk-SK");
            dictionary.Add("SN", "wo-SN");
            dictionary.Add("SV", "es-SV");
            dictionary.Add("SY", "ar-SY");
            dictionary.Add("TH", "th-TH");
            dictionary.Add("TJ", "tg-Cyrl-TJ");
            dictionary.Add("TM", "tk-TM");
            dictionary.Add("TN", "ar-TN");
            dictionary.Add("TR", "tr-TR");
            dictionary.Add("TT", "en-TT");
            dictionary.Add("TW", "zh-TW");
            dictionary.Add("UA", "uk-UA");
            dictionary.Add("US", "en-US");
            dictionary.Add("UY", "es-UY");
            dictionary.Add("UZ", "uz-Cyrl-UZ");
            dictionary.Add("VE", "es-VE");
            dictionary.Add("VN", "vi-VN");
            dictionary.Add("YE", "ar-YE");
            dictionary.Add("ZA", "af-ZA");
            dictionary.Add("ZW", "en-ZW");
            RegionNames = dictionary;
            return true;
        }

        private bool InitCompatibilityCultureData()
        {
            string str2;
            string str3;
            string str4 = AnsiToLower(this.sRealName);
            if (str4 != null)
            {
                if (!(str4 == "zh-chs"))
                {
                    if (str4 == "zh-cht")
                    {
                        str2 = "zh-Hant";
                        str3 = "zh-CHT";
                        goto Label_004B;
                    }
                }
                else
                {
                    str2 = "zh-Hans";
                    str3 = "zh-CHS";
                    goto Label_004B;
                }
            }
            return false;
        Label_004B:
            this.sRealName = str2;
            if (!this.InitCultureData())
            {
                return false;
            }
            this.sName = str3;
            this.sParent = str2;
            this.bFramework = true;
            return true;
        }

        [SecuritySafeCritical]
        private bool InitCultureData()
        {
            if (!nativeInitCultureData(this))
            {
                return false;
            }
            if (CultureInfo.IsTaiwanSku)
            {
                this.TreatTaiwanParentChainAsHavingTaiwanAsSpecific();
            }
            return true;
        }

        private bool InitLegacyAlternateSortData()
        {
            string str;
            string str2;
            if (CompareInfo.IsLegacy20SortingBehaviorRequested && ((str2 = AnsiToLower(this.sRealName)) != null))
            {
                if (!(str2 == "ko-kr_unicod"))
                {
                    if (str2 == "ja-jp_unicod")
                    {
                        str = "ja-JP_unicod";
                        this.sRealName = "ja-JP";
                        this.iLanguage = 0x10411;
                        goto Label_00A2;
                    }
                    if (str2 == "zh-hk_stroke")
                    {
                        str = "zh-HK_stroke";
                        this.sRealName = "zh-HK";
                        this.iLanguage = 0x20c04;
                        goto Label_00A2;
                    }
                }
                else
                {
                    str = "ko-KR_unicod";
                    this.sRealName = "ko-KR";
                    this.iLanguage = 0x10412;
                    goto Label_00A2;
                }
            }
            return false;
        Label_00A2:
            if (!nativeInitCultureData(this))
            {
                return false;
            }
            this.sRealName = str;
            this.sCompareInfo = str;
            this.bFramework = true;
            return true;
        }

        internal static bool IsCustomCultureId(int cultureId)
        {
            if ((cultureId != 0xc00) && (cultureId != 0x1000))
            {
                return false;
            }
            return true;
        }

        private bool IsIncorrectNativeLanguageForSinhala()
        {
            if (!IsOsWin7OrPrior() || (!(this.sName == "si-LK") && !(this.sName == "si")))
            {
                return false;
            }
            return !this.IsReplacementCulture;
        }

        private bool IsNeutralInParentChainOfTaiwan()
        {
            if (!(this.sRealName == "zh"))
            {
                return (this.sRealName == "zh-Hant");
            }
            return true;
        }

        private static bool IsOsPriorToWin7()
        {
            return ((Environment.OSVersion.Platform == PlatformID.Win32NT) && (Environment.OSVersion.Version < s_win7Version));
        }

        private static bool IsOsWin7OrPrior()
        {
            return ((Environment.OSVersion.Platform == PlatformID.Win32NT) && (Environment.OSVersion.Version < new Version(6, 2)));
        }

        [SecuritySafeCritical]
        private static bool IsReplacementCultureName(string name)
        {
            string[] o = s_replacementCultureNames;
            if (o == null)
            {
                if (nativeEnumCultureNames(0x10, JitHelpers.GetObjectHandleOnStack<string[]>(ref o)) == 0)
                {
                    return false;
                }
                Array.Sort<string>(o);
                s_replacementCultureNames = o;
            }
            return (Array.BinarySearch<string>(o, name) >= 0);
        }

        [SecurityCritical]
        private static bool IsResourcePresent(string resourceKey)
        {
            if (MscorlibResourceSet == null)
            {
                MscorlibResourceSet = new ResourceSet(typeof(Environment).Assembly.GetManifestResourceStream("mscorlib.resources"));
            }
            return (MscorlibResourceSet.GetString(resourceKey) != null);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        internal static extern string LCIDToLocaleName(int lcid);
        internal string[] LeapYearMonthNames(int calendarId)
        {
            return this.GetCalendar(calendarId).saLeapYearMonthNames;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        internal static extern int LocaleNameToLCID(string localeName);
        internal string[] LongDates(int calendarId)
        {
            return this.GetCalendar(calendarId).saLongDates;
        }

        internal string MonthDay(int calendarId)
        {
            return this.GetCalendar(calendarId).sMonthDay;
        }

        internal string[] MonthNames(int calendarId)
        {
            return this.GetCalendar(calendarId).saMonthNames;
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern int nativeEnumCultureNames(int cultureTypes, ObjectHandleOnStack retStringArray);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        private static extern string[] nativeEnumTimeFormats(string localeName, uint dwFlags, bool useUserOverride);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        internal static extern bool nativeGetNumberFormatInfoValues(string localeName, NumberFormatInfo nfi, bool useUserOverride);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        internal static extern bool nativeInitCultureData(CultureData cultureData);
        internal static string ReescapeWin32String(string str)
        {
            if (str == null)
            {
                return null;
            }
            StringBuilder builder = null;
            bool flag = false;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '\'')
                {
                    if (flag)
                    {
                        if (((i + 1) < str.Length) && (str[i + 1] == '\''))
                        {
                            if (builder == null)
                            {
                                builder = new StringBuilder(str, 0, i, str.Length * 2);
                            }
                            builder.Append(@"\'");
                            i++;
                            continue;
                        }
                        flag = false;
                    }
                    else
                    {
                        flag = true;
                    }
                }
                else if (str[i] == '\\')
                {
                    if (builder == null)
                    {
                        builder = new StringBuilder(str, 0, i, str.Length * 2);
                    }
                    builder.Append(@"\\");
                    continue;
                }
                if (builder != null)
                {
                    builder.Append(str[i]);
                }
            }
            if (builder == null)
            {
                return str;
            }
            return builder.ToString();
        }

        internal static string[] ReescapeWin32Strings(string[] array)
        {
            if (array != null)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = ReescapeWin32String(array[i]);
                }
            }
            return array;
        }

        internal string[] ShortDates(int calendarId)
        {
            return this.GetCalendar(calendarId).saShortDates;
        }

        private static string StripSecondsFromPattern(string time)
        {
            bool flag = false;
            int num = -1;
            for (int i = 0; i < time.Length; i++)
            {
                if (time[i] == '\'')
                {
                    flag = !flag;
                }
                else if (time[i] == '\\')
                {
                    i++;
                }
                else if (!flag)
                {
                    switch (time[i])
                    {
                        case 'm':
                        case 'H':
                        case 'h':
                            goto Label_00D6;

                        case 's':
                        {
                            bool flag2;
                            if (((((i - num) <= 4) && ((i - num) > 1)) && ((time[num + 1] != '\'') && (time[i - 1] != '\''))) && (num >= 0))
                            {
                                i = num + 1;
                            }
                            int startIndex = GetIndexOfNextTokenAfterSeconds(time, i, out flag2);
                            StringBuilder builder = new StringBuilder(time.Substring(0, i));
                            if (flag2)
                            {
                                builder.Append(' ');
                            }
                            builder.Append(time.Substring(startIndex));
                            time = builder.ToString();
                            break;
                        }
                    }
                }
                continue;
            Label_00D6:
                num = i;
            }
            return time;
        }

        internal string[] SuperShortDayNames(int calendarId)
        {
            return this.GetCalendar(calendarId).saSuperShortDayNames;
        }

        [SecuritySafeCritical]
        private void TreatTaiwanParentChainAsHavingTaiwanAsSpecific()
        {
            if ((this.IsNeutralInParentChainOfTaiwan() && IsOsPriorToWin7()) && !this.IsReplacementCulture)
            {
                string sNATIVELANGUAGE = this.SNATIVELANGUAGE;
                string sENGLISHLANGUAGE = this.SENGLISHLANGUAGE;
                string sLOCALIZEDLANGUAGE = this.SLOCALIZEDLANGUAGE;
                string sTEXTINFO = this.STEXTINFO;
                string sCOMPAREINFO = this.SCOMPAREINFO;
                string fONTSIGNATURE = this.FONTSIGNATURE;
                int iDEFAULTANSICODEPAGE = this.IDEFAULTANSICODEPAGE;
                int iDEFAULTOEMCODEPAGE = this.IDEFAULTOEMCODEPAGE;
                int iDEFAULTMACCODEPAGE = this.IDEFAULTMACCODEPAGE;
                this.sSpecificCulture = "zh-TW";
                this.sWindowsName = "zh-TW";
            }
        }

        private static string UnescapeNlsString(string str, int start, int end)
        {
            StringBuilder builder = null;
            for (int i = start; (i < str.Length) && (i <= end); i++)
            {
                switch (str[i])
                {
                    case '\'':
                        if (builder == null)
                        {
                            builder = new StringBuilder(str, start, i - start, str.Length);
                        }
                        break;

                    case '\\':
                        if (builder == null)
                        {
                            builder = new StringBuilder(str, start, i - start, str.Length);
                        }
                        i++;
                        if (i < str.Length)
                        {
                            builder.Append(str[i]);
                        }
                        break;

                    default:
                        if (builder != null)
                        {
                            builder.Append(str[i]);
                        }
                        break;
                }
            }
            if (builder == null)
            {
                return str.Substring(start, (end - start) + 1);
            }
            return builder.ToString();
        }

        internal string[] YearMonths(int calendarId)
        {
            return this.GetCalendar(calendarId).saYearMonths;
        }

        internal int[] CalendarIds
        {
            get
            {
                if (this.waCalendars == null)
                {
                    int[] calendars = new int[0x17];
                    int length = CalendarData.nativeGetCalendars(this.sWindowsName, this.bUseOverrides, calendars);
                    if (length == 0)
                    {
                        this.waCalendars = Invariant.waCalendars;
                    }
                    else
                    {
                        if (this.sWindowsName == "zh-TW")
                        {
                            bool flag = false;
                            for (int i = 0; i < length; i++)
                            {
                                if (calendars[i] == 4)
                                {
                                    flag = true;
                                    break;
                                }
                            }
                            if (!flag)
                            {
                                length++;
                                Array.Copy(calendars, 1, calendars, 2, 0x15);
                                calendars[1] = 4;
                            }
                        }
                        int[] destinationArray = new int[length];
                        Array.Copy(calendars, destinationArray, length);
                        this.waCalendars = destinationArray;
                    }
                }
                return this.waCalendars;
            }
        }

        internal string CultureName
        {
            get
            {
                string str;
                if (((str = this.sName) == null) || (!(str == "zh-CHS") && !(str == "zh-CHT")))
                {
                    return this.sRealName;
                }
                return this.sName;
            }
        }

        internal Calendar DefaultCalendar
        {
            get
            {
                int calType = this.DoGetLocaleInfoInt(0x1009);
                if (calType == 0)
                {
                    calType = this.CalendarIds[0];
                }
                return CultureInfo.GetCalendarInstance(calType);
            }
        }

        private string FONTSIGNATURE
        {
            [SecuritySafeCritical]
            get
            {
                if (this.fontSignature == null)
                {
                    this.fontSignature = this.DoGetLocaleInfo(0x58);
                }
                return this.fontSignature;
            }
        }

        internal int ICOUNTRY
        {
            get
            {
                return this.DoGetLocaleInfoInt(5);
            }
        }

        internal int IDEFAULTANSICODEPAGE
        {
            get
            {
                if (this.iDefaultAnsiCodePage == -1)
                {
                    this.iDefaultAnsiCodePage = this.DoGetLocaleInfoInt(0x1004);
                }
                return this.iDefaultAnsiCodePage;
            }
        }

        private int IDEFAULTCOUNTRY
        {
            [SecuritySafeCritical]
            get
            {
                return this.DoGetLocaleInfoInt(10);
            }
        }

        internal int IDEFAULTEBCDICCODEPAGE
        {
            get
            {
                if (this.iDefaultEbcdicCodePage == -1)
                {
                    this.iDefaultEbcdicCodePage = this.DoGetLocaleInfoInt(0x1012);
                }
                return this.iDefaultEbcdicCodePage;
            }
        }

        internal int IDEFAULTMACCODEPAGE
        {
            get
            {
                if (this.iDefaultMacCodePage == -1)
                {
                    this.iDefaultMacCodePage = this.DoGetLocaleInfoInt(0x1011);
                }
                return this.iDefaultMacCodePage;
            }
        }

        internal int IDEFAULTOEMCODEPAGE
        {
            get
            {
                if (this.iDefaultOemCodePage == -1)
                {
                    this.iDefaultOemCodePage = this.DoGetLocaleInfoInt(11);
                }
                return this.iDefaultOemCodePage;
            }
        }

        internal int IFIRSTDAYOFWEEK
        {
            get
            {
                if ((this.iFirstDayOfWeek == -1) || this.UseUserOverride)
                {
                    this.iFirstDayOfWeek = ConvertFirstDayOfWeekMonToSun(this.DoGetLocaleInfoInt(0x100c));
                }
                return this.iFirstDayOfWeek;
            }
        }

        internal int IFIRSTWEEKOFYEAR
        {
            get
            {
                if ((this.iFirstWeekOfYear == -1) || this.UseUserOverride)
                {
                    this.iFirstWeekOfYear = this.DoGetLocaleInfoInt(0x100d);
                }
                return this.iFirstWeekOfYear;
            }
        }

        internal int IGEOID
        {
            get
            {
                if (this.iGeoId == -1)
                {
                    this.iGeoId = this.DoGetLocaleInfoInt(0x5b);
                }
                return this.iGeoId;
            }
        }

        internal int IINPUTLANGUAGEHANDLE
        {
            get
            {
                if (this.iInputLanguageHandle == -1)
                {
                    if (this.IsUserCustomCulture)
                    {
                        this.iInputLanguageHandle = 0x409;
                    }
                    else
                    {
                        this.iInputLanguageHandle = this.ILANGUAGE;
                    }
                }
                return this.iInputLanguageHandle;
            }
        }

        internal int ILANGUAGE
        {
            get
            {
                if (this.iLanguage == 0)
                {
                    this.iLanguage = LocaleNameToLCID(this.sRealName);
                }
                return this.iLanguage;
            }
        }

        private bool ILEADINGZEROS
        {
            [SecuritySafeCritical]
            get
            {
                return (this.DoGetLocaleInfoInt(0x12) == 1);
            }
        }

        internal int IMEASURE
        {
            get
            {
                if ((this.iMeasure == -1) || this.UseUserOverride)
                {
                    this.iMeasure = this.DoGetLocaleInfoInt(13);
                }
                return this.iMeasure;
            }
        }

        internal int INEGATIVEPERCENT
        {
            get
            {
                if ((this.iNegativePercent == -1) || this.UseUserOverride)
                {
                    this.iNegativePercent = this.DoGetLocaleInfoInt(0x74);
                }
                return this.iNegativePercent;
            }
        }

        private int IPAPERSIZE
        {
            [SecuritySafeCritical]
            get
            {
                return this.DoGetLocaleInfoInt(0x100a);
            }
        }

        internal int IPOSITIVEPERCENT
        {
            get
            {
                if ((this.iPositivePercent == -1) || this.UseUserOverride)
                {
                    this.iPositivePercent = this.DoGetLocaleInfoInt(0x75);
                }
                return this.iPositivePercent;
            }
        }

        private int IREADINGLAYOUT
        {
            get
            {
                if ((this.iReadingLayout == -1) || this.UseUserOverride)
                {
                    this.iReadingLayout = this.DoGetLocaleInfoInt(0x70);
                }
                return this.iReadingLayout;
            }
        }

        internal bool IsFramework
        {
            get
            {
                return this.bFramework;
            }
        }

        internal bool IsInvariantCulture
        {
            get
            {
                return string.IsNullOrEmpty(this.SNAME);
            }
        }

        internal bool IsNeutralCulture
        {
            get
            {
                return this.bNeutral;
            }
        }

        internal bool IsReplacementCulture
        {
            get
            {
                return IsReplacementCultureName(this.SNAME);
            }
        }

        internal bool IsRightToLeft
        {
            get
            {
                return (this.IREADINGLAYOUT == 1);
            }
        }

        internal bool IsUserCustomCulture
        {
            get
            {
                return IsCustomCultureId(this.ILANGUAGE);
            }
        }

        internal bool IsWin32Installed
        {
            get
            {
                return this.bWin32Installed;
            }
        }

        internal string[] LongTimes
        {
            get
            {
                if ((this.saLongTimes == null) || this.UseUserOverride)
                {
                    string[] strArray = this.DoEnumTimeFormats();
                    if ((strArray == null) || (strArray.Length == 0))
                    {
                        this.saLongTimes = Invariant.saLongTimes;
                    }
                    else
                    {
                        this.saLongTimes = strArray;
                    }
                }
                return this.saLongTimes;
            }
        }

        internal string SABBREVCTRYNAME
        {
            [SecurityCritical]
            get
            {
                if (this.sAbbrevCountry == null)
                {
                    this.sAbbrevCountry = this.DoGetLocaleInfo(7);
                }
                return this.sAbbrevCountry;
            }
        }

        internal string SABBREVLANGNAME
        {
            [SecurityCritical]
            get
            {
                if (this.sAbbrevLang == null)
                {
                    this.sAbbrevLang = this.DoGetLocaleInfo(3);
                }
                return this.sAbbrevLang;
            }
        }

        internal string[] SADURATION
        {
            [SecurityCritical]
            get
            {
                if ((this.saDurationFormats == null) || this.UseUserOverride)
                {
                    string str = this.DoGetLocaleInfo(0x5d);
                    this.saDurationFormats = new string[] { ReescapeWin32String(str) };
                }
                return this.saDurationFormats;
            }
        }

        internal string SAM1159
        {
            [SecurityCritical]
            get
            {
                if ((this.sAM1159 == null) || this.UseUserOverride)
                {
                    this.sAM1159 = this.DoGetLocaleInfo(40);
                }
                return this.sAM1159;
            }
        }

        internal string SCOMPAREINFO
        {
            [SecuritySafeCritical]
            get
            {
                if (this.sCompareInfo == null)
                {
                    if (this.IsUserCustomCulture)
                    {
                        this.sCompareInfo = this.DoGetLocaleInfo(0x7b);
                    }
                    if (this.sCompareInfo == null)
                    {
                        this.sCompareInfo = this.sWindowsName;
                    }
                }
                return this.sCompareInfo;
            }
        }

        internal string SCONSOLEFALLBACKNAME
        {
            [SecurityCritical]
            get
            {
                if (this.sConsoleFallbackName == null)
                {
                    string str = this.DoGetLocaleInfo(110);
                    if (str == "es-ES_tradnl")
                    {
                        str = "es-ES";
                    }
                    this.sConsoleFallbackName = str;
                }
                return this.sConsoleFallbackName;
            }
        }

        internal string SCURRENCY
        {
            [SecurityCritical]
            get
            {
                if ((this.sCurrency == null) || this.UseUserOverride)
                {
                    this.sCurrency = this.DoGetLocaleInfo(20);
                }
                return this.sCurrency;
            }
        }

        internal string SENGCOUNTRY
        {
            [SecurityCritical]
            get
            {
                if (this.sEnglishCountry == null)
                {
                    this.sEnglishCountry = this.DoGetLocaleInfo(0x1002);
                }
                return this.sEnglishCountry;
            }
        }

        internal string SENGDISPLAYNAME
        {
            [SecurityCritical]
            get
            {
                if (this.sEnglishDisplayName == null)
                {
                    if (this.IsNeutralCulture)
                    {
                        string str;
                        this.sEnglishDisplayName = this.SENGLISHLANGUAGE;
                        if (((str = this.sName) != null) && ((str == "zh-CHS") || (str == "zh-CHT")))
                        {
                            this.sEnglishDisplayName = this.sEnglishDisplayName + " Legacy";
                        }
                    }
                    else
                    {
                        this.sEnglishDisplayName = this.DoGetLocaleInfo(0x72);
                        if (string.IsNullOrEmpty(this.sEnglishDisplayName))
                        {
                            if (this.SENGLISHLANGUAGE.EndsWith(')'))
                            {
                                this.sEnglishDisplayName = this.SENGLISHLANGUAGE.Substring(0, this.sEnglishLanguage.Length - 1) + ", " + this.SENGCOUNTRY + ")";
                            }
                            else
                            {
                                this.sEnglishDisplayName = this.SENGLISHLANGUAGE + " (" + this.SENGCOUNTRY + ")";
                            }
                        }
                    }
                }
                return this.sEnglishDisplayName;
            }
        }

        internal string SENGLISHCURRENCY
        {
            [SecurityCritical]
            get
            {
                if (this.sEnglishCurrency == null)
                {
                    this.sEnglishCurrency = this.DoGetLocaleInfo(0x1007);
                }
                return this.sEnglishCurrency;
            }
        }

        internal string SENGLISHLANGUAGE
        {
            [SecurityCritical]
            get
            {
                if (this.sEnglishLanguage == null)
                {
                    this.sEnglishLanguage = this.DoGetLocaleInfo(0x1001);
                }
                return this.sEnglishLanguage;
            }
        }

        internal string[] ShortTimes
        {
            get
            {
                if ((this.saShortTimes == null) || this.UseUserOverride)
                {
                    string[] shortTimes = null;
                    shortTimes = this.DoEnumShortTimeFormats();
                    if ((shortTimes == null) || (shortTimes.Length == 0))
                    {
                        shortTimes = this.DeriveShortTimesFromLong();
                    }
                    shortTimes = this.AdjustShortTimesForMac(shortTimes);
                    this.saShortTimes = shortTimes;
                }
                return this.saShortTimes;
            }
        }

        internal string SINTLSYMBOL
        {
            [SecurityCritical]
            get
            {
                if ((this.sIntlMonetarySymbol == null) || this.UseUserOverride)
                {
                    this.sIntlMonetarySymbol = this.DoGetLocaleInfo(0x15);
                }
                return this.sIntlMonetarySymbol;
            }
        }

        internal string SISO3166CTRYNAME
        {
            [SecurityCritical]
            get
            {
                if (this.sISO3166CountryName == null)
                {
                    this.sISO3166CountryName = this.DoGetLocaleInfo(90);
                }
                return this.sISO3166CountryName;
            }
        }

        internal string SISO3166CTRYNAME2
        {
            [SecurityCritical]
            get
            {
                if (this.sISO3166CountryName2 == null)
                {
                    this.sISO3166CountryName2 = this.DoGetLocaleInfo(0x68);
                }
                return this.sISO3166CountryName2;
            }
        }

        internal string SISO639LANGNAME
        {
            [SecurityCritical]
            get
            {
                if (this.sISO639Language == null)
                {
                    this.sISO639Language = this.DoGetLocaleInfo(0x59);
                }
                return this.sISO639Language;
            }
        }

        internal string SISO639LANGNAME2
        {
            [SecurityCritical]
            get
            {
                if (this.sISO639Language2 == null)
                {
                    this.sISO639Language2 = this.DoGetLocaleInfo(0x67);
                }
                return this.sISO639Language2;
            }
        }

        private string SKEYBOARDSTOINSTALL
        {
            [SecuritySafeCritical]
            get
            {
                return this.DoGetLocaleInfo(0x5e);
            }
        }

        internal string SLIST
        {
            [SecurityCritical]
            get
            {
                if ((this.sListSeparator == null) || this.UseUserOverride)
                {
                    this.sListSeparator = this.DoGetLocaleInfo(12);
                }
                return this.sListSeparator;
            }
        }

        internal string SLOCALIZEDCOUNTRY
        {
            [SecurityCritical]
            get
            {
                if (this.sLocalizedCountry == null)
                {
                    if (this.IsUserCustomCulture && !IsReplacementCultureName(this.SNAME))
                    {
                        this.sLocalizedCountry = this.SNATIVECOUNTRY;
                    }
                    else
                    {
                        string resourceKey = "Globalization.ri_" + this.SREGIONNAME;
                        if (IsResourcePresent(resourceKey))
                        {
                            this.sLocalizedCountry = Environment.GetResourceString(resourceKey);
                        }
                    }
                    if (string.IsNullOrEmpty(this.sLocalizedCountry))
                    {
                        if (CultureInfo.UserDefaultUICulture.Name.Equals(Thread.CurrentThread.CurrentUICulture.Name))
                        {
                            this.sLocalizedCountry = this.DoGetLocaleInfo(6);
                        }
                        if (string.IsNullOrEmpty(this.sLocalizedDisplayName))
                        {
                            this.sLocalizedCountry = this.SNATIVECOUNTRY;
                        }
                    }
                }
                return this.sLocalizedCountry;
            }
        }

        internal string SLOCALIZEDDISPLAYNAME
        {
            [SecurityCritical]
            get
            {
                if (this.sLocalizedDisplayName == null)
                {
                    if (this.IsUserCustomCulture && !IsReplacementCultureName(this.SNAME))
                    {
                        if (this.IsNeutralCulture)
                        {
                            this.sLocalizedDisplayName = this.SNATIVELANGUAGE;
                        }
                        else
                        {
                            this.sLocalizedDisplayName = this.SNATIVEDISPLAYNAME;
                        }
                    }
                    else
                    {
                        string resourceKey = "Globalization.ci_" + this.sName;
                        if (IsResourcePresent(resourceKey))
                        {
                            this.sLocalizedDisplayName = Environment.GetResourceString(resourceKey);
                        }
                    }
                    if (string.IsNullOrEmpty(this.sLocalizedDisplayName))
                    {
                        if (this.IsNeutralCulture)
                        {
                            this.sLocalizedDisplayName = this.SLOCALIZEDLANGUAGE;
                        }
                        else
                        {
                            if (CultureInfo.UserDefaultUICulture.Name.Equals(Thread.CurrentThread.CurrentUICulture.Name))
                            {
                                this.sLocalizedDisplayName = this.DoGetLocaleInfo(2);
                            }
                            if (string.IsNullOrEmpty(this.sLocalizedDisplayName))
                            {
                                this.sLocalizedDisplayName = this.SNATIVEDISPLAYNAME;
                            }
                        }
                    }
                }
                return this.sLocalizedDisplayName;
            }
        }

        internal string SLOCALIZEDLANGUAGE
        {
            [SecurityCritical]
            get
            {
                if (this.sLocalizedLanguage == null)
                {
                    if (CultureInfo.UserDefaultUICulture.Name.Equals(Thread.CurrentThread.CurrentUICulture.Name))
                    {
                        this.sLocalizedLanguage = this.DoGetLocaleInfo(0x6f);
                    }
                    if (string.IsNullOrEmpty(this.sLocalizedLanguage))
                    {
                        this.sLocalizedLanguage = this.SNATIVELANGUAGE;
                    }
                }
                return this.sLocalizedLanguage;
            }
        }

        internal string SNAME
        {
            get
            {
                if (this.sName == null)
                {
                    this.sName = string.Empty;
                }
                return this.sName;
            }
        }

        internal string SNAN
        {
            [SecurityCritical]
            get
            {
                if ((this.sNaN == null) || this.UseUserOverride)
                {
                    this.sNaN = this.DoGetLocaleInfo(0x69);
                }
                return this.sNaN;
            }
        }

        internal string SNATIVECOUNTRY
        {
            [SecurityCritical]
            get
            {
                if (this.sNativeCountry == null)
                {
                    this.sNativeCountry = this.DoGetLocaleInfo(8);
                }
                return this.sNativeCountry;
            }
        }

        internal string SNATIVECURRENCY
        {
            [SecurityCritical]
            get
            {
                if (this.sNativeCurrency == null)
                {
                    this.sNativeCurrency = this.DoGetLocaleInfo(0x1008);
                }
                return this.sNativeCurrency;
            }
        }

        internal string SNATIVEDISPLAYNAME
        {
            [SecurityCritical]
            get
            {
                if (this.sNativeDisplayName == null)
                {
                    if (!this.IsNeutralCulture)
                    {
                        if (this.IsIncorrectNativeLanguageForSinhala())
                        {
                            this.sNativeDisplayName = "සිංහල (ශ්‍රී ලංකා)";
                        }
                        else
                        {
                            this.sNativeDisplayName = this.DoGetLocaleInfo(0x73);
                        }
                        if (string.IsNullOrEmpty(this.sNativeDisplayName))
                        {
                            this.sNativeDisplayName = this.SNATIVELANGUAGE + " (" + this.SNATIVECOUNTRY + ")";
                        }
                    }
                    else
                    {
                        this.sNativeDisplayName = this.SNATIVELANGUAGE;
                        string sName = this.sName;
                        if (sName != null)
                        {
                            if (!(sName == "zh-CHS"))
                            {
                                if (sName == "zh-CHT")
                                {
                                    this.sNativeDisplayName = this.sNativeDisplayName + " 舊版";
                                }
                            }
                            else
                            {
                                this.sNativeDisplayName = this.sNativeDisplayName + " 旧版";
                            }
                        }
                    }
                }
                return this.sNativeDisplayName;
            }
        }

        internal string SNATIVELANGUAGE
        {
            [SecurityCritical]
            get
            {
                if (this.sNativeLanguage == null)
                {
                    if (this.IsIncorrectNativeLanguageForSinhala())
                    {
                        this.sNativeLanguage = "සිංහල";
                    }
                    else
                    {
                        this.sNativeLanguage = this.DoGetLocaleInfo(4);
                    }
                }
                return this.sNativeLanguage;
            }
        }

        internal string SNEGINFINITY
        {
            [SecurityCritical]
            get
            {
                if ((this.sNegativeInfinity == null) || this.UseUserOverride)
                {
                    this.sNegativeInfinity = this.DoGetLocaleInfo(0x6b);
                }
                return this.sNegativeInfinity;
            }
        }

        private string SOPENTYPELANGUAGETAG
        {
            [SecuritySafeCritical]
            get
            {
                return this.DoGetLocaleInfo(0x7a);
            }
        }

        internal string SPARENT
        {
            [SecurityCritical]
            get
            {
                if (this.sParent == null)
                {
                    this.sParent = this.DoGetLocaleInfo(this.sRealName, 0x6d);
                    string sParent = this.sParent;
                    if (sParent != null)
                    {
                        if (!(sParent == "zh-Hans"))
                        {
                            if (sParent == "zh-Hant")
                            {
                                this.sParent = "zh-CHT";
                            }
                        }
                        else
                        {
                            this.sParent = "zh-CHS";
                        }
                    }
                }
                return this.sParent;
            }
        }

        private static CultureInfo[] SpecificCultures
        {
            get
            {
                if (specificCultures == null)
                {
                    specificCultures = GetCultures(CultureTypes.SpecificCultures);
                }
                return specificCultures;
            }
        }

        internal string SPERCENT
        {
            [SecurityCritical]
            get
            {
                if ((this.sPercent == null) || this.UseUserOverride)
                {
                    this.sPercent = this.DoGetLocaleInfo(0x76);
                }
                return this.sPercent;
            }
        }

        internal string SPERMILLE
        {
            [SecurityCritical]
            get
            {
                if ((this.sPerMille == null) || this.UseUserOverride)
                {
                    this.sPerMille = this.DoGetLocaleInfo(0x77);
                }
                return this.sPerMille;
            }
        }

        internal string SPM2359
        {
            [SecurityCritical]
            get
            {
                if ((this.sPM2359 == null) || this.UseUserOverride)
                {
                    this.sPM2359 = this.DoGetLocaleInfo(0x29);
                }
                return this.sPM2359;
            }
        }

        internal string SPOSINFINITY
        {
            [SecurityCritical]
            get
            {
                if ((this.sPositiveInfinity == null) || this.UseUserOverride)
                {
                    this.sPositiveInfinity = this.DoGetLocaleInfo(0x6a);
                }
                return this.sPositiveInfinity;
            }
        }

        internal string SREGIONNAME
        {
            [SecurityCritical]
            get
            {
                if (this.sRegionName == null)
                {
                    this.sRegionName = this.DoGetLocaleInfo(90);
                }
                return this.sRegionName;
            }
        }

        private string SSCRIPTS
        {
            [SecuritySafeCritical]
            get
            {
                if (this.sScripts == null)
                {
                    this.sScripts = this.DoGetLocaleInfo(0x6c);
                }
                return this.sScripts;
            }
        }

        internal string SSPECIFICCULTURE
        {
            get
            {
                return this.sSpecificCulture;
            }
        }

        internal string STEXTINFO
        {
            [SecuritySafeCritical]
            get
            {
                if (this.sTextInfo == null)
                {
                    if (this.IsNeutralCulture || this.IsUserCustomCulture)
                    {
                        string cultureName = this.DoGetLocaleInfo(0x7b);
                        this.sTextInfo = GetCultureData(cultureName, this.bUseOverrides).SNAME;
                    }
                    if (this.sTextInfo == null)
                    {
                        this.sTextInfo = this.SNAME;
                    }
                }
                return this.sTextInfo;
            }
        }

        internal string TimeSeparator
        {
            [SecuritySafeCritical]
            get
            {
                string str = ReescapeWin32String(this.DoGetLocaleInfo(0x1003));
                if (string.IsNullOrEmpty(str))
                {
                    str = this.LongTimes[0];
                }
                return GetTimeSeparator(str);
            }
        }

        internal bool UseUserOverride
        {
            get
            {
                return this.bUseOverrides;
            }
        }

        internal int[] WAGROUPING
        {
            [SecurityCritical]
            get
            {
                if ((this.waGrouping == null) || this.UseUserOverride)
                {
                    this.waGrouping = ConvertWin32GroupString(this.DoGetLocaleInfo(0x10));
                }
                return this.waGrouping;
            }
        }

        internal int[] WAMONGROUPING
        {
            [SecurityCritical]
            get
            {
                if ((this.waMonetaryGrouping == null) || this.UseUserOverride)
                {
                    this.waMonetaryGrouping = ConvertWin32GroupString(this.DoGetLocaleInfo(0x18));
                }
                return this.waMonetaryGrouping;
            }
        }
    }
}

