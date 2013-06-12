namespace System.Globalization
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class DateTimeFormatInfoScanner
    {
        internal const string ChineseHourSuff = "时";
        internal const string CJKDaySuff = "日";
        internal const string CJKHourSuff = "時";
        internal const string CJKMinuteSuff = "分";
        internal const string CJKMonthSuff = "月";
        internal const string CJKSecondSuff = "秒";
        internal const string CJKYearSuff = "年";
        internal const char IgnorableSymbolChar = '';
        internal const string KoreanDaySuff = "일";
        internal const string KoreanHourSuff = "시";
        internal const string KoreanMinuteSuff = "분";
        internal const string KoreanMonthSuff = "월";
        internal const string KoreanSecondSuff = "초";
        internal const string KoreanYearSuff = "년";
        internal List<string> m_dateWords = new List<string>();
        private FoundDatePattern m_ymdFlags;
        internal const char MonthPostfixChar = '';
        private static Dictionary<string, string> s_knownWords;

        internal void AddDateWordOrPostfix(string formatPostfix, string str)
        {
            if (str.Length > 0)
            {
                if (str.Equals("."))
                {
                    this.AddIgnorableSymbols(".");
                }
                else
                {
                    string str2;
                    if (!KnownWords.TryGetValue(str, out str2))
                    {
                        if (this.m_dateWords == null)
                        {
                            this.m_dateWords = new List<string>();
                        }
                        if (formatPostfix == "MMMM")
                        {
                            string item = ((char) 0xe000) + str;
                            if (!this.m_dateWords.Contains(item))
                            {
                                this.m_dateWords.Add(item);
                            }
                        }
                        else
                        {
                            if (!this.m_dateWords.Contains(str))
                            {
                                this.m_dateWords.Add(str);
                            }
                            if (str[str.Length - 1] == '.')
                            {
                                string str4 = str.Substring(0, str.Length - 1);
                                if (!this.m_dateWords.Contains(str4))
                                {
                                    this.m_dateWords.Add(str4);
                                }
                            }
                        }
                    }
                }
            }
        }

        internal int AddDateWords(string pattern, int index, string formatPostfix)
        {
            int num = SkipWhiteSpacesAndNonLetter(pattern, index);
            if ((num != index) && (formatPostfix != null))
            {
                formatPostfix = null;
            }
            index = num;
            StringBuilder builder = new StringBuilder();
            while (index < pattern.Length)
            {
                char c = pattern[index];
                switch (c)
                {
                    case '\'':
                        this.AddDateWordOrPostfix(formatPostfix, builder.ToString());
                        index++;
                        return index;

                    case '\\':
                    {
                        index++;
                        if (index < pattern.Length)
                        {
                            builder.Append(pattern[index]);
                            index++;
                        }
                        continue;
                    }
                }
                if (char.IsWhiteSpace(c))
                {
                    this.AddDateWordOrPostfix(formatPostfix, builder.ToString());
                    if (formatPostfix != null)
                    {
                        formatPostfix = null;
                    }
                    builder.Length = 0;
                    index++;
                }
                else
                {
                    builder.Append(c);
                    index++;
                }
            }
            return index;
        }

        internal void AddIgnorableSymbols(string text)
        {
            if (this.m_dateWords == null)
            {
                this.m_dateWords = new List<string>();
            }
            string item = ((char) 0xe001) + text;
            if (!this.m_dateWords.Contains(item))
            {
                this.m_dateWords.Add(item);
            }
        }

        private static bool ArrayElementsBeginWithDigit(string[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (((array[i].Length > 0) && (array[i][0] >= '0')) && (array[i][0] <= '9'))
                {
                    int num2 = 1;
                    while (((num2 < array[i].Length) && (array[i][num2] >= '0')) && (array[i][num2] <= '9'))
                    {
                        num2++;
                    }
                    if (num2 == array[i].Length)
                    {
                        return false;
                    }
                    if (num2 == (array[i].Length - 1))
                    {
                        switch (array[i][num2])
                        {
                            case '月':
                            case 0xc6d4:
                                return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        private static bool ArrayElementsHaveSpace(string[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                for (int j = 0; j < array[i].Length; j++)
                {
                    if (char.IsWhiteSpace(array[i][j]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool EqualStringArrays(string[] array1, string[] array2)
        {
            if (array1 != array2)
            {
                if (array1.Length != array2.Length)
                {
                    return false;
                }
                for (int i = 0; i < array1.Length; i++)
                {
                    if (!array1[i].Equals(array2[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal string[] GetDateWordsOfDTFI(DateTimeFormatInfo dtfi)
        {
            int num;
            string[] allDateTimePatterns = dtfi.GetAllDateTimePatterns('D');
            for (num = 0; num < allDateTimePatterns.Length; num++)
            {
                this.ScanDateWord(allDateTimePatterns[num]);
            }
            allDateTimePatterns = dtfi.GetAllDateTimePatterns('d');
            for (num = 0; num < allDateTimePatterns.Length; num++)
            {
                this.ScanDateWord(allDateTimePatterns[num]);
            }
            allDateTimePatterns = dtfi.GetAllDateTimePatterns('y');
            for (num = 0; num < allDateTimePatterns.Length; num++)
            {
                this.ScanDateWord(allDateTimePatterns[num]);
            }
            this.ScanDateWord(dtfi.MonthDayPattern);
            allDateTimePatterns = dtfi.GetAllDateTimePatterns('T');
            for (num = 0; num < allDateTimePatterns.Length; num++)
            {
                this.ScanDateWord(allDateTimePatterns[num]);
            }
            allDateTimePatterns = dtfi.GetAllDateTimePatterns('t');
            for (num = 0; num < allDateTimePatterns.Length; num++)
            {
                this.ScanDateWord(allDateTimePatterns[num]);
            }
            string[] strArray2 = null;
            if ((this.m_dateWords != null) && (this.m_dateWords.Count > 0))
            {
                strArray2 = new string[this.m_dateWords.Count];
                for (num = 0; num < this.m_dateWords.Count; num++)
                {
                    strArray2[num] = this.m_dateWords[num];
                }
            }
            return strArray2;
        }

        internal static FORMATFLAGS GetFormatFlagGenitiveMonth(string[] monthNames, string[] genitveMonthNames, string[] abbrevMonthNames, string[] genetiveAbbrevMonthNames)
        {
            if (EqualStringArrays(monthNames, genitveMonthNames) && EqualStringArrays(abbrevMonthNames, genetiveAbbrevMonthNames))
            {
                return FORMATFLAGS.None;
            }
            return FORMATFLAGS.UseGenitiveMonth;
        }

        internal static FORMATFLAGS GetFormatFlagUseHebrewCalendar(int calID)
        {
            if (calID != 8)
            {
                return FORMATFLAGS.None;
            }
            return (FORMATFLAGS.UseHebrewParsing | FORMATFLAGS.UseLeapYearMonth);
        }

        internal static FORMATFLAGS GetFormatFlagUseSpaceInDayNames(string[] dayNames, string[] abbrevDayNames)
        {
            if (!ArrayElementsHaveSpace(dayNames) && !ArrayElementsHaveSpace(abbrevDayNames))
            {
                return FORMATFLAGS.None;
            }
            return FORMATFLAGS.UseSpacesInDayNames;
        }

        internal static FORMATFLAGS GetFormatFlagUseSpaceInMonthNames(string[] monthNames, string[] genitveMonthNames, string[] abbrevMonthNames, string[] genetiveAbbrevMonthNames)
        {
            FORMATFLAGS none = FORMATFLAGS.None;
            none |= ((ArrayElementsBeginWithDigit(monthNames) || ArrayElementsBeginWithDigit(genitveMonthNames)) || (ArrayElementsBeginWithDigit(abbrevMonthNames) || ArrayElementsBeginWithDigit(genetiveAbbrevMonthNames))) ? FORMATFLAGS.UseDigitPrefixInTokens : FORMATFLAGS.None;
            return (none | (((ArrayElementsHaveSpace(monthNames) || ArrayElementsHaveSpace(genitveMonthNames)) || (ArrayElementsHaveSpace(abbrevMonthNames) || ArrayElementsHaveSpace(genetiveAbbrevMonthNames))) ? FORMATFLAGS.UseSpacesInMonthNames : FORMATFLAGS.None));
        }

        internal void ScanDateWord(string pattern)
        {
            this.m_ymdFlags = FoundDatePattern.None;
            int index = 0;
            while (index < pattern.Length)
            {
                int num2;
                char c = pattern[index];
                switch (c)
                {
                    case '\\':
                    {
                        index += 2;
                        continue;
                    }
                    case 'd':
                    {
                        index = ScanRepeatChar(pattern, 'd', index, out num2);
                        if (num2 <= 2)
                        {
                            this.m_ymdFlags |= FoundDatePattern.FoundDayPatternFlag;
                        }
                        continue;
                    }
                    case 'y':
                    {
                        index = ScanRepeatChar(pattern, 'y', index, out num2);
                        this.m_ymdFlags |= FoundDatePattern.FoundYearPatternFlag;
                        continue;
                    }
                    case '\'':
                    {
                        index = this.AddDateWords(pattern, index + 1, null);
                        continue;
                    }
                    case '.':
                    {
                        if (this.m_ymdFlags == FoundDatePattern.FoundYMDPatternFlag)
                        {
                            this.AddIgnorableSymbols(".");
                            this.m_ymdFlags = FoundDatePattern.None;
                        }
                        index++;
                        continue;
                    }
                    case 'M':
                    {
                        index = ScanRepeatChar(pattern, 'M', index, out num2);
                        if (((num2 >= 4) && (index < pattern.Length)) && (pattern[index] == '\''))
                        {
                            index = this.AddDateWords(pattern, index + 1, "MMMM");
                        }
                        this.m_ymdFlags |= FoundDatePattern.FoundMonthPatternFlag;
                        continue;
                    }
                }
                if ((this.m_ymdFlags == FoundDatePattern.FoundYMDPatternFlag) && !char.IsWhiteSpace(c))
                {
                    this.m_ymdFlags = FoundDatePattern.None;
                }
                index++;
            }
        }

        internal static int ScanRepeatChar(string pattern, char ch, int index, out int count)
        {
            count = 1;
            while ((++index < pattern.Length) && (pattern[index] == ch))
            {
                count++;
            }
            return index;
        }

        internal static int SkipWhiteSpacesAndNonLetter(string pattern, int currentIndex)
        {
            while (currentIndex < pattern.Length)
            {
                char c = pattern[currentIndex];
                if (c == '\\')
                {
                    currentIndex++;
                    if (currentIndex >= pattern.Length)
                    {
                        return currentIndex;
                    }
                    c = pattern[currentIndex];
                    if (c == '\'')
                    {
                        continue;
                    }
                }
                if ((char.IsLetter(c) || (c == '\'')) || (c == '.'))
                {
                    return currentIndex;
                }
                currentIndex++;
            }
            return currentIndex;
        }

        private static Dictionary<string, string> KnownWords
        {
            get
            {
                if (s_knownWords == null)
                {
                    Dictionary<string, string> dictionary = new Dictionary<string, string>();
                    dictionary.Add("/", string.Empty);
                    dictionary.Add("-", string.Empty);
                    dictionary.Add(".", string.Empty);
                    dictionary.Add("年", string.Empty);
                    dictionary.Add("月", string.Empty);
                    dictionary.Add("日", string.Empty);
                    dictionary.Add("년", string.Empty);
                    dictionary.Add("월", string.Empty);
                    dictionary.Add("일", string.Empty);
                    dictionary.Add("시", string.Empty);
                    dictionary.Add("분", string.Empty);
                    dictionary.Add("초", string.Empty);
                    dictionary.Add("時", string.Empty);
                    dictionary.Add("时", string.Empty);
                    dictionary.Add("分", string.Empty);
                    dictionary.Add("秒", string.Empty);
                    s_knownWords = dictionary;
                }
                return s_knownWords;
            }
        }

        private enum FoundDatePattern
        {
            FoundDayPatternFlag = 4,
            FoundMonthPatternFlag = 2,
            FoundYearPatternFlag = 1,
            FoundYMDPatternFlag = 7,
            None = 0
        }
    }
}

