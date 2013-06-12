namespace System.Net
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    internal static class HttpDateParse
    {
        private const int BASE_DEC = 10;
        private const int DATE_1123_INDEX_DAY = 1;
        private const int DATE_1123_INDEX_HRS = 4;
        private const int DATE_1123_INDEX_MINS = 5;
        private const int DATE_1123_INDEX_MONTH = 2;
        private const int DATE_1123_INDEX_SECS = 6;
        private const int DATE_1123_INDEX_YEAR = 3;
        private const int DATE_ANSI_INDEX_DAY = 2;
        private const int DATE_ANSI_INDEX_HRS = 3;
        private const int DATE_ANSI_INDEX_MINS = 4;
        private const int DATE_ANSI_INDEX_MONTH = 1;
        private const int DATE_ANSI_INDEX_SECS = 5;
        private const int DATE_ANSI_INDEX_YEAR = 6;
        private const int DATE_INDEX_DAY_OF_WEEK = 0;
        private const int DATE_INDEX_LAST = 7;
        private const int DATE_INDEX_TZ = 7;
        private const int DATE_TOKEN_APRIL = 4;
        private const int DATE_TOKEN_AUGUST = 8;
        private const int DATE_TOKEN_DECEMBER = 12;
        private const int DATE_TOKEN_ERROR = -999;
        private const int DATE_TOKEN_FEBRUARY = 2;
        private const int DATE_TOKEN_FRIDAY = 5;
        private const int DATE_TOKEN_GMT = -1000;
        private const int DATE_TOKEN_JANUARY = 1;
        private const int DATE_TOKEN_JULY = 7;
        private const int DATE_TOKEN_JUNE = 6;
        private const int DATE_TOKEN_LAST = -1000;
        private const int DATE_TOKEN_LAST_DAY = 7;
        private const int DATE_TOKEN_LAST_MONTH = 13;
        private const int DATE_TOKEN_MARCH = 3;
        private const int DATE_TOKEN_MAY = 5;
        private const int DATE_TOKEN_MONDAY = 1;
        private const int DATE_TOKEN_NOVEMBER = 11;
        private const int DATE_TOKEN_OCTOBER = 10;
        private const int DATE_TOKEN_SATURDAY = 6;
        private const int DATE_TOKEN_SEPTEMBER = 9;
        private const int DATE_TOKEN_SUNDAY = 0;
        private const int DATE_TOKEN_THURSDAY = 4;
        private const int DATE_TOKEN_TUESDAY = 2;
        private const int DATE_TOKEN_WEDNESDAY = 3;
        private const int MAX_FIELD_DATE_ENTRIES = 8;

        private static char MAKE_UPPER(char c)
        {
            return char.ToUpper(c, CultureInfo.InvariantCulture);
        }

        private static int MapDayMonthToDword(char[] lpszDay, int index)
        {
            switch (MAKE_UPPER(lpszDay[index]))
            {
                case 'A':
                    switch (MAKE_UPPER(lpszDay[index + 1]))
                    {
                        case 'P':
                            return 4;

                        case 'U':
                            return 8;
                    }
                    return -999;

                case 'D':
                    return 12;

                case 'F':
                {
                    char ch3 = MAKE_UPPER(lpszDay[index + 1]);
                    if (ch3 == 'E')
                    {
                        return 2;
                    }
                    if (ch3 != 'R')
                    {
                        return -999;
                    }
                    return 5;
                }
                case 'G':
                    return -1000;

                case 'J':
                    switch (MAKE_UPPER(lpszDay[index + 1]))
                    {
                        case 'A':
                            return 1;

                        case 'U':
                            switch (MAKE_UPPER(lpszDay[index + 2]))
                            {
                                case 'L':
                                    return 7;

                                case 'N':
                                    return 6;
                            }
                            break;
                    }
                    return -999;

                case 'M':
                {
                    char ch4 = MAKE_UPPER(lpszDay[index + 1]);
                    if (ch4 == 'A')
                    {
                        switch (MAKE_UPPER(lpszDay[index + 2]))
                        {
                            case 'R':
                                return 3;

                            case 'Y':
                                return 5;
                        }
                        break;
                    }
                    if (ch4 != 'O')
                    {
                        break;
                    }
                    return 1;
                }
                case 'N':
                    return 11;

                case 'O':
                    return 10;

                case 'S':
                    switch (MAKE_UPPER(lpszDay[index + 1]))
                    {
                        case 'A':
                            return 6;

                        case 'E':
                            return 9;

                        case 'U':
                            return 0;
                    }
                    return -999;

                case 'T':
                {
                    char ch9 = MAKE_UPPER(lpszDay[index + 1]);
                    if (ch9 == 'H')
                    {
                        return 4;
                    }
                    if (ch9 != 'U')
                    {
                        return -999;
                    }
                    return 2;
                }
                case 'U':
                    return -1000;

                case 'W':
                    return 3;

                default:
                    return -999;
            }
            return -999;
        }

        public static bool ParseHttpDate(string DateString, out DateTime dtOut)
        {
            int num4;
            int num5;
            int num6;
            int num7;
            int num8;
            int num9;
            int index = 0;
            int num2 = 0;
            int num3 = -1;
            bool flag = false;
            int[] numArray = new int[8];
            char[] lpszDay = DateString.ToCharArray();
            dtOut = new DateTime();
            while ((index < DateString.Length) && (num2 < 8))
            {
                if ((lpszDay[index] >= '0') && (lpszDay[index] <= '9'))
                {
                    numArray[num2] = 0;
                    do
                    {
                        numArray[num2] *= 10;
                        numArray[num2] += lpszDay[index] - '0';
                        index++;
                    }
                    while (((index < DateString.Length) && (lpszDay[index] >= '0')) && (lpszDay[index] <= '9'));
                    num2++;
                }
                else
                {
                    if (((lpszDay[index] >= 'A') && (lpszDay[index] <= 'Z')) || ((lpszDay[index] >= 'a') && (lpszDay[index] <= 'z')))
                    {
                        numArray[num2] = MapDayMonthToDword(lpszDay, index);
                        num3 = num2;
                        if ((numArray[num2] == -999) && (!flag || (num2 != 6)))
                        {
                            return false;
                        }
                        if (num2 == 1)
                        {
                            flag = true;
                        }
                        do
                        {
                            index++;
                        }
                        while ((index < DateString.Length) && (((lpszDay[index] >= 'A') && (lpszDay[index] <= 'Z')) || ((lpszDay[index] >= 'a') && (lpszDay[index] <= 'z'))));
                        num2++;
                        continue;
                    }
                    index++;
                }
            }
            int millisecond = 0;
            if (flag)
            {
                num6 = numArray[2];
                num5 = numArray[1];
                num7 = numArray[3];
                num8 = numArray[4];
                num9 = numArray[5];
                if (num3 != 6)
                {
                    num4 = numArray[6];
                }
                else
                {
                    num4 = numArray[7];
                }
            }
            else
            {
                num6 = numArray[1];
                num5 = numArray[2];
                num4 = numArray[3];
                num7 = numArray[4];
                num8 = numArray[5];
                num9 = numArray[6];
            }
            if (num4 < 100)
            {
                num4 += (num4 < 80) ? 0x7d0 : 0x76c;
            }
            if (((num2 < 4) || (num6 > 0x1f)) || (((num7 > 0x17) || (num8 > 0x3b)) || (num9 > 0x3b)))
            {
                return false;
            }
            dtOut = new DateTime(num4, num5, num6, num7, num8, num9, millisecond);
            if (num3 == 6)
            {
                dtOut = dtOut.ToUniversalTime();
            }
            if ((num2 > 7) && (numArray[7] != -1000))
            {
                double num11 = numArray[7];
                dtOut.AddHours(num11);
            }
            dtOut = dtOut.ToLocalTime();
            return true;
        }
    }
}

