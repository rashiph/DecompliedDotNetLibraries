namespace System.Web.Util
{
    using System;
    using System.Globalization;
    using System.Web;

    internal static class HttpDate
    {
        private static readonly string[] s_days = new string[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
        private static readonly sbyte[] s_monthIndexTable = new sbyte[] { 
            -1, 0x41, 2, 12, -1, -1, -1, 8, -1, -1, -1, -1, 7, -1, 0x4e, -1, 
            9, -1, 0x52, -1, 10, -1, 11, -1, -1, 5, -1, -1, -1, -1, -1, -1, 
            -1, 0x41, 2, 12, -1, -1, -1, 8, -1, -1, -1, -1, 7, -1, 0x4e, -1, 
            9, -1, 0x52, -1, 10, -1, 11, -1, -1, 5, -1, -1, -1, -1, -1, -1
         };
        private static readonly string[] s_months = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        private static readonly int[] s_tensDigit = new int[] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90 };

        private static int atoi2(string s, int startIndex)
        {
            int num3;
            try
            {
                int index = s[startIndex] - '0';
                int num2 = s[1 + startIndex] - '0';
                num3 = s_tensDigit[index] + num2;
            }
            catch
            {
                throw new FormatException(System.Web.SR.GetString("Atio2BadString", new object[] { s, startIndex }));
            }
            return num3;
        }

        private static int make_month(string s, int startIndex)
        {
            int index = (s[2 + startIndex] - '@') & 0x3f;
            sbyte num2 = s_monthIndexTable[index];
            if (num2 >= 13)
            {
                if (num2 != 0x4e)
                {
                    if (num2 != 0x52)
                    {
                        throw new FormatException(System.Web.SR.GetString("MakeMonthBadString", new object[] { s, startIndex }));
                    }
                    if (s_monthIndexTable[(s[1 + startIndex] - '@') & 0x3f] == 0x41)
                    {
                        num2 = 3;
                    }
                    else
                    {
                        num2 = 4;
                    }
                }
                else if (s_monthIndexTable[(s[1 + startIndex] - '@') & 0x3f] == 0x41)
                {
                    num2 = 1;
                }
                else
                {
                    num2 = 6;
                }
            }
            string str = s_months[num2 - 1];
            if ((((s[startIndex] != str[0]) || (s[1 + startIndex] != str[1])) || (s[2 + startIndex] != str[2])) && (((char.ToUpper(s[startIndex], CultureInfo.InvariantCulture) != str[0]) || (char.ToLower(s[1 + startIndex], CultureInfo.InvariantCulture) != str[1])) || (char.ToLower(s[2 + startIndex], CultureInfo.InvariantCulture) != str[2])))
            {
                throw new FormatException(System.Web.SR.GetString("MakeMonthBadString", new object[] { s, startIndex }));
            }
            return num2;
        }

        internal static DateTime UtcParse(string time)
        {
            int num2;
            int num3;
            int num4;
            int num5;
            int num6;
            int num7;
            if (time == null)
            {
                throw new ArgumentNullException("time");
            }
            int index = time.IndexOf(',');
            if (index != -1)
            {
                int num8 = time.Length - index;
                while ((--num8 > 0) && (time[++index] == ' '))
                {
                }
                if (time[index + 2] == '-')
                {
                    if (num8 < 0x12)
                    {
                        throw new FormatException(System.Web.SR.GetString("UtilParseDateTimeBad", new object[] { time }));
                    }
                    num4 = atoi2(time, index);
                    num3 = make_month(time, index + 3);
                    num2 = atoi2(time, index + 7);
                    if (num2 < 50)
                    {
                        num2 += 0x7d0;
                    }
                    else
                    {
                        num2 += 0x76c;
                    }
                    num5 = atoi2(time, index + 10);
                    num6 = atoi2(time, index + 13);
                    num7 = atoi2(time, index + 0x10);
                }
                else
                {
                    if (num8 < 20)
                    {
                        throw new FormatException(System.Web.SR.GetString("UtilParseDateTimeBad", new object[] { time }));
                    }
                    num4 = atoi2(time, index);
                    num3 = make_month(time, index + 3);
                    num2 = (atoi2(time, index + 7) * 100) + atoi2(time, index + 9);
                    num5 = atoi2(time, index + 12);
                    num6 = atoi2(time, index + 15);
                    num7 = atoi2(time, index + 0x12);
                }
            }
            else
            {
                index = -1;
                int num9 = time.Length + 1;
                while ((--num9 > 0) && (time[++index] == ' '))
                {
                }
                if (num9 < 0x18)
                {
                    throw new FormatException(System.Web.SR.GetString("UtilParseDateTimeBad", new object[] { time }));
                }
                num4 = atoi2(time, index + 8);
                num3 = make_month(time, index + 4);
                num2 = (atoi2(time, index + 20) * 100) + atoi2(time, index + 0x16);
                num5 = atoi2(time, index + 11);
                num6 = atoi2(time, index + 14);
                num7 = atoi2(time, index + 0x11);
            }
            return new DateTime(num2, num3, num4, num5, num6, num7);
        }
    }
}

