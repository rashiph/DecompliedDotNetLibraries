namespace System
{
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    [FriendAccessAllowed]
    internal class Number
    {
        private const int Int32Precision = 10;
        private const int Int64Precision = 0x13;
        private const int NumberMaxDigits = 50;
        private const int UInt32Precision = 10;
        private const int UInt64Precision = 20;

        private Number()
        {
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern string FormatDecimal(decimal value, string format, NumberFormatInfo info);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern string FormatDouble(double value, string format, NumberFormatInfo info);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern string FormatInt32(int value, string format, NumberFormatInfo info);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern string FormatInt64(long value, string format, NumberFormatInfo info);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical, FriendAccessAllowed]
        internal static extern unsafe string FormatNumberBuffer(byte* number, string format, NumberFormatInfo info);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern string FormatSingle(float value, string format, NumberFormatInfo info);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern string FormatUInt32(uint value, string format, NumberFormatInfo info);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern string FormatUInt64(ulong value, string format, NumberFormatInfo info);
        private static bool HexNumberToInt32(ref NumberBuffer number, ref int value)
        {
            uint num = 0;
            bool flag = HexNumberToUInt32(ref number, ref num);
            value = (int) num;
            return flag;
        }

        private static bool HexNumberToInt64(ref NumberBuffer number, ref long value)
        {
            ulong num = 0L;
            bool flag = HexNumberToUInt64(ref number, ref num);
            value = (long) num;
            return flag;
        }

        [SecuritySafeCritical]
        private static unsafe bool HexNumberToUInt32(ref NumberBuffer number, ref uint value)
        {
            int scale = number.scale;
            if ((scale > 10) || (scale < number.precision))
            {
                return false;
            }
            char* digits = number.digits;
            uint num2 = 0;
            while (--scale >= 0)
            {
                if (num2 > 0xfffffff)
                {
                    return false;
                }
                num2 *= 0x10;
                if (digits[0] != '\0')
                {
                    uint num3 = num2;
                    if (digits[0] != '\0')
                    {
                        if ((digits[0] >= '0') && (digits[0] <= '9'))
                        {
                            num3 += digits[0] - '0';
                        }
                        else if ((digits[0] >= 'A') && (digits[0] <= 'F'))
                        {
                            num3 += (uint) ((digits[0] - 'A') + 10);
                        }
                        else
                        {
                            num3 += (uint) ((digits[0] - 'a') + 10);
                        }
                        digits++;
                    }
                    if (num3 < num2)
                    {
                        return false;
                    }
                    num2 = num3;
                }
            }
            value = num2;
            return true;
        }

        [SecuritySafeCritical]
        private static unsafe bool HexNumberToUInt64(ref NumberBuffer number, ref ulong value)
        {
            int scale = number.scale;
            if ((scale > 20) || (scale < number.precision))
            {
                return false;
            }
            char* digits = number.digits;
            ulong num2 = 0L;
            while (--scale >= 0)
            {
                if (num2 > 0xfffffffffffffffL)
                {
                    return false;
                }
                num2 *= (ulong) 0x10L;
                if (digits[0] != '\0')
                {
                    ulong num3 = num2;
                    if (digits[0] != '\0')
                    {
                        if ((digits[0] >= '0') && (digits[0] <= '9'))
                        {
                            num3 += digits[0] - '0';
                        }
                        else if ((digits[0] >= 'A') && (digits[0] <= 'F'))
                        {
                            num3 += (digits[0] - 'A') + 10;
                        }
                        else
                        {
                            num3 += (digits[0] - 'a') + 10;
                        }
                        digits++;
                    }
                    if (num3 < num2)
                    {
                        return false;
                    }
                    num2 = num3;
                }
            }
            value = num2;
            return true;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        private static bool IsWhite(char ch)
        {
            return ((ch == ' ') || ((ch >= '\t') && (ch <= '\r')));
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecurityCritical]
        private static unsafe char* MatchChars(char* p, string str)
        {
            fixed (char* str2 = ((char*) str))
            {
                char* chPtr = str2;
                return MatchChars(p, chPtr);
            }
        }

        [SecurityCritical]
        private static unsafe char* MatchChars(char* p, char* str)
        {
            if (str[0] != '\0')
            {
                while (str[0] != '\0')
                {
                    if ((p[0] != str[0]) && ((str[0] != '\x00a0') || (p[0] != ' ')))
                    {
                        return null;
                    }
                    p++;
                    str++;
                }
                return p;
            }
            return null;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern unsafe bool NumberBufferToDecimal(byte* number, ref decimal value);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern unsafe bool NumberBufferToDouble(byte* number, ref double value);
        [SecuritySafeCritical]
        private static unsafe bool NumberToInt32(ref NumberBuffer number, ref int value)
        {
            int scale = number.scale;
            if ((scale > 10) || (scale < number.precision))
            {
                return false;
            }
            char* digits = number.digits;
            int num2 = 0;
            while (--scale >= 0)
            {
                if (num2 > 0xccccccc)
                {
                    return false;
                }
                num2 *= 10;
                if (digits[0] != '\0')
                {
                    digits++;
                    num2 += digits[0] - '0';
                }
            }
            if (number.sign)
            {
                num2 = -num2;
                if (num2 > 0)
                {
                    return false;
                }
            }
            else if (num2 < 0)
            {
                return false;
            }
            value = num2;
            return true;
        }

        [SecuritySafeCritical]
        private static unsafe bool NumberToInt64(ref NumberBuffer number, ref long value)
        {
            int scale = number.scale;
            if ((scale > 0x13) || (scale < number.precision))
            {
                return false;
            }
            char* digits = number.digits;
            long num2 = 0L;
            while (--scale >= 0)
            {
                if (num2 > 0xcccccccccccccccL)
                {
                    return false;
                }
                num2 *= 10L;
                if (digits[0] != '\0')
                {
                    digits++;
                    num2 += digits[0] - '0';
                }
            }
            if (number.sign)
            {
                num2 = -num2;
                if (num2 > 0L)
                {
                    return false;
                }
            }
            else if (num2 < 0L)
            {
                return false;
            }
            value = num2;
            return true;
        }

        [SecuritySafeCritical]
        private static unsafe bool NumberToUInt32(ref NumberBuffer number, ref uint value)
        {
            int scale = number.scale;
            if (((scale > 10) || (scale < number.precision)) || number.sign)
            {
                return false;
            }
            char* digits = number.digits;
            uint num2 = 0;
            while (--scale >= 0)
            {
                if (num2 > 0x19999999)
                {
                    return false;
                }
                num2 *= 10;
                if (digits[0] != '\0')
                {
                    digits++;
                    uint num3 = num2 + (digits[0] - '0');
                    if (num3 < num2)
                    {
                        return false;
                    }
                    num2 = num3;
                }
            }
            value = num2;
            return true;
        }

        [SecuritySafeCritical]
        private static unsafe bool NumberToUInt64(ref NumberBuffer number, ref ulong value)
        {
            int scale = number.scale;
            if (((scale > 20) || (scale < number.precision)) || number.sign)
            {
                return false;
            }
            char* digits = number.digits;
            ulong num2 = 0L;
            while (--scale >= 0)
            {
                if (num2 > 0x1999999999999999L)
                {
                    return false;
                }
                num2 *= (ulong) 10L;
                if (digits[0] != '\0')
                {
                    digits++;
                    ulong num3 = num2 + (digits[0] - '0');
                    if (num3 < num2)
                    {
                        return false;
                    }
                    num2 = num3;
                }
            }
            value = num2;
            return true;
        }

        [SecuritySafeCritical]
        internal static unsafe decimal ParseDecimal(string value, NumberStyles options, NumberFormatInfo numfmt)
        {
            byte* stackBuffer = stackalloc byte[0x72];
            NumberBuffer number = new NumberBuffer(stackBuffer);
            decimal num = 0M;
            StringToNumber(value, options, ref number, numfmt, true);
            if (!NumberBufferToDecimal(number.PackForNative(), ref num))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Decimal"));
            }
            return num;
        }

        [SecuritySafeCritical]
        internal static unsafe double ParseDouble(string value, NumberStyles options, NumberFormatInfo numfmt)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            byte* stackBuffer = stackalloc byte[0x72];
            NumberBuffer number = new NumberBuffer(stackBuffer);
            double num = 0.0;
            if (!TryStringToNumber(value, options, ref number, numfmt, false))
            {
                string str = value.Trim();
                if (str.Equals(numfmt.PositiveInfinitySymbol))
                {
                    return double.PositiveInfinity;
                }
                if (str.Equals(numfmt.NegativeInfinitySymbol))
                {
                    return double.NegativeInfinity;
                }
                if (!str.Equals(numfmt.NaNSymbol))
                {
                    throw new FormatException(Environment.GetResourceString("Format_InvalidString"));
                }
                return double.NaN;
            }
            if (!NumberBufferToDouble(number.PackForNative(), ref num))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Double"));
            }
            return num;
        }

        [SecuritySafeCritical]
        internal static unsafe int ParseInt32(string s, NumberStyles style, NumberFormatInfo info)
        {
            byte* stackBuffer = stackalloc byte[0x72];
            NumberBuffer number = new NumberBuffer(stackBuffer);
            int num = 0;
            StringToNumber(s, style, ref number, info, false);
            if ((style & NumberStyles.AllowHexSpecifier) != NumberStyles.None)
            {
                if (!HexNumberToInt32(ref number, ref num))
                {
                    throw new OverflowException(Environment.GetResourceString("Overflow_Int32"));
                }
                return num;
            }
            if (!NumberToInt32(ref number, ref num))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Int32"));
            }
            return num;
        }

        [SecuritySafeCritical]
        internal static unsafe long ParseInt64(string value, NumberStyles options, NumberFormatInfo numfmt)
        {
            byte* stackBuffer = stackalloc byte[0x72];
            NumberBuffer number = new NumberBuffer(stackBuffer);
            long num = 0L;
            StringToNumber(value, options, ref number, numfmt, false);
            if ((options & NumberStyles.AllowHexSpecifier) != NumberStyles.None)
            {
                if (!HexNumberToInt64(ref number, ref num))
                {
                    throw new OverflowException(Environment.GetResourceString("Overflow_Int64"));
                }
                return num;
            }
            if (!NumberToInt64(ref number, ref num))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Int64"));
            }
            return num;
        }

        [SecurityCritical]
        private static unsafe bool ParseNumber(ref char* str, NumberStyles options, ref NumberBuffer number, StringBuilder sb, NumberFormatInfo numfmt, bool parseDecimal)
        {
            string currencyDecimalSeparator;
            string currencyGroupSeparator;
            char* chPtr2;
            number.scale = 0;
            number.sign = false;
            string currencySymbol = null;
            string ansiCurrencySymbol = null;
            string numberDecimalSeparator = null;
            string numberGroupSeparator = null;
            bool flag = false;
            if ((options & NumberStyles.AllowCurrencySymbol) != NumberStyles.None)
            {
                currencySymbol = numfmt.CurrencySymbol;
                if (numfmt.ansiCurrencySymbol != null)
                {
                    ansiCurrencySymbol = numfmt.ansiCurrencySymbol;
                }
                numberDecimalSeparator = numfmt.NumberDecimalSeparator;
                numberGroupSeparator = numfmt.NumberGroupSeparator;
                currencyDecimalSeparator = numfmt.CurrencyDecimalSeparator;
                currencyGroupSeparator = numfmt.CurrencyGroupSeparator;
                flag = true;
            }
            else
            {
                currencyDecimalSeparator = numfmt.NumberDecimalSeparator;
                currencyGroupSeparator = numfmt.NumberGroupSeparator;
            }
            int num = 0;
            bool flag2 = false;
            bool flag3 = sb != null;
            bool flag4 = flag3 && ((options & NumberStyles.AllowHexSpecifier) != NumberStyles.None);
            int num2 = flag3 ? 0x7fffffff : 50;
            char* p = str;
            char ch = p[0];
            while (true)
            {
                if ((!IsWhite(ch) || ((options & NumberStyles.AllowLeadingWhite) == NumberStyles.None)) || (((num & 1) != 0) && (((num & 1) == 0) || (((num & 0x20) == 0) && (numfmt.numberNegativePattern != 2)))))
                {
                    if ((flag2 = ((options & NumberStyles.AllowLeadingSign) != NumberStyles.None) && ((num & 1) == 0)) && ((chPtr2 = MatchChars(p, numfmt.positiveSign)) != null))
                    {
                        num |= 1;
                        p = chPtr2 - 1;
                    }
                    else if (flag2 && ((chPtr2 = MatchChars(p, numfmt.negativeSign)) != null))
                    {
                        num |= 1;
                        number.sign = true;
                        p = chPtr2 - 1;
                    }
                    else if (((ch == '(') && ((options & NumberStyles.AllowParentheses) != NumberStyles.None)) && ((num & 1) == 0))
                    {
                        num |= 3;
                        number.sign = true;
                    }
                    else
                    {
                        if (((currencySymbol == null) || ((chPtr2 = MatchChars(p, currencySymbol)) == null)) && ((ansiCurrencySymbol == null) || ((chPtr2 = MatchChars(p, ansiCurrencySymbol)) == null)))
                        {
                            break;
                        }
                        num |= 0x20;
                        currencySymbol = null;
                        ansiCurrencySymbol = null;
                        p = chPtr2 - 1;
                    }
                }
                ch = *(++p);
            }
            int num3 = 0;
            int index = 0;
            while (true)
            {
                if (((ch >= '0') && (ch <= '9')) || (((options & NumberStyles.AllowHexSpecifier) != NumberStyles.None) && (((ch >= 'a') && (ch <= 'f')) || ((ch >= 'A') && (ch <= 'F')))))
                {
                    num |= 4;
                    if (((ch != '0') || ((num & 8) != 0)) || flag4)
                    {
                        if (num3 < num2)
                        {
                            if (flag3)
                            {
                                sb.Append(ch);
                            }
                            else
                            {
                                number.digits[num3++] = ch;
                            }
                            if ((ch != '0') || parseDecimal)
                            {
                                index = num3;
                            }
                        }
                        if ((num & 0x10) == 0)
                        {
                            number.scale++;
                        }
                        num |= 8;
                    }
                    else if ((num & 0x10) != 0)
                    {
                        number.scale--;
                    }
                }
                else if ((((options & NumberStyles.AllowDecimalPoint) != NumberStyles.None) && ((num & 0x10) == 0)) && (((chPtr2 = MatchChars(p, currencyDecimalSeparator)) != null) || ((flag && ((num & 0x20) == 0)) && ((chPtr2 = MatchChars(p, numberDecimalSeparator)) != null))))
                {
                    num |= 0x10;
                    p = chPtr2 - 1;
                }
                else
                {
                    if (((((options & NumberStyles.AllowThousands) == NumberStyles.None) || ((num & 4) == 0)) || ((num & 0x10) != 0)) || (((chPtr2 = MatchChars(p, currencyGroupSeparator)) == null) && ((!flag || ((num & 0x20) != 0)) || ((chPtr2 = MatchChars(p, numberGroupSeparator)) == null))))
                    {
                        break;
                    }
                    p = chPtr2 - 1;
                }
                ch = *(++p);
            }
            bool flag5 = false;
            number.precision = index;
            if (flag3)
            {
                sb.Append('\0');
            }
            else
            {
                number.digits[index] = '\0';
            }
            if ((num & 4) != 0)
            {
                if (((ch == 'E') || (ch == 'e')) && ((options & NumberStyles.AllowExponent) != NumberStyles.None))
                {
                    char* chPtr3 = p;
                    ch = *(++p);
                    chPtr2 = MatchChars(p, numfmt.positiveSign);
                    if (chPtr2 != null)
                    {
                        ch = *(p = chPtr2);
                    }
                    else
                    {
                        chPtr2 = MatchChars(p, numfmt.negativeSign);
                        if (chPtr2 != null)
                        {
                            ch = *(p = chPtr2);
                            flag5 = true;
                        }
                    }
                    if ((ch >= '0') && (ch <= '9'))
                    {
                        int num5 = 0;
                        do
                        {
                            num5 = (num5 * 10) + (ch - '0');
                            ch = *(++p);
                            if (num5 > 0x3e8)
                            {
                                num5 = 0x270f;
                                while ((ch >= '0') && (ch <= '9'))
                                {
                                    ch = *(++p);
                                }
                            }
                        }
                        while ((ch >= '0') && (ch <= '9'));
                        if (flag5)
                        {
                            num5 = -num5;
                        }
                        number.scale += num5;
                    }
                    else
                    {
                        p = chPtr3;
                        ch = p[0];
                    }
                }
                while (true)
                {
                    if (!IsWhite(ch) || ((options & NumberStyles.AllowTrailingWhite) == NumberStyles.None))
                    {
                        if ((flag2 = ((options & NumberStyles.AllowTrailingSign) != NumberStyles.None) && ((num & 1) == 0)) && ((chPtr2 = MatchChars(p, numfmt.positiveSign)) != null))
                        {
                            num |= 1;
                            p = chPtr2 - 1;
                        }
                        else if (flag2 && ((chPtr2 = MatchChars(p, numfmt.negativeSign)) != null))
                        {
                            num |= 1;
                            number.sign = true;
                            p = chPtr2 - 1;
                        }
                        else if ((ch == ')') && ((num & 2) != 0))
                        {
                            num &= -3;
                        }
                        else
                        {
                            if (((currencySymbol == null) || ((chPtr2 = MatchChars(p, currencySymbol)) == null)) && ((ansiCurrencySymbol == null) || ((chPtr2 = MatchChars(p, ansiCurrencySymbol)) == null)))
                            {
                                break;
                            }
                            currencySymbol = null;
                            ansiCurrencySymbol = null;
                            p = chPtr2 - 1;
                        }
                    }
                    ch = *(++p);
                }
                if ((num & 2) == 0)
                {
                    if ((num & 8) == 0)
                    {
                        if (!parseDecimal)
                        {
                            number.scale = 0;
                        }
                        if ((num & 0x10) == 0)
                        {
                            number.sign = false;
                        }
                    }
                    str = p;
                    return true;
                }
            }
            str = p;
            return false;
        }

        [SecuritySafeCritical]
        internal static unsafe float ParseSingle(string value, NumberStyles options, NumberFormatInfo numfmt)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            byte* stackBuffer = stackalloc byte[0x72];
            NumberBuffer number = new NumberBuffer(stackBuffer);
            double num = 0.0;
            if (!TryStringToNumber(value, options, ref number, numfmt, false))
            {
                string str = value.Trim();
                if (str.Equals(numfmt.PositiveInfinitySymbol))
                {
                    return float.PositiveInfinity;
                }
                if (str.Equals(numfmt.NegativeInfinitySymbol))
                {
                    return float.NegativeInfinity;
                }
                if (!str.Equals(numfmt.NaNSymbol))
                {
                    throw new FormatException(Environment.GetResourceString("Format_InvalidString"));
                }
                return float.NaN;
            }
            if (!NumberBufferToDouble(number.PackForNative(), ref num))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Single"));
            }
            float f = (float) num;
            if (float.IsInfinity(f))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Single"));
            }
            return f;
        }

        [SecuritySafeCritical]
        internal static unsafe uint ParseUInt32(string value, NumberStyles options, NumberFormatInfo numfmt)
        {
            byte* stackBuffer = stackalloc byte[0x72];
            NumberBuffer number = new NumberBuffer(stackBuffer);
            uint num = 0;
            StringToNumber(value, options, ref number, numfmt, false);
            if ((options & NumberStyles.AllowHexSpecifier) != NumberStyles.None)
            {
                if (!HexNumberToUInt32(ref number, ref num))
                {
                    throw new OverflowException(Environment.GetResourceString("Overflow_UInt32"));
                }
                return num;
            }
            if (!NumberToUInt32(ref number, ref num))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_UInt32"));
            }
            return num;
        }

        [SecuritySafeCritical]
        internal static unsafe ulong ParseUInt64(string value, NumberStyles options, NumberFormatInfo numfmt)
        {
            byte* stackBuffer = stackalloc byte[0x72];
            NumberBuffer number = new NumberBuffer(stackBuffer);
            ulong num = 0L;
            StringToNumber(value, options, ref number, numfmt, false);
            if ((options & NumberStyles.AllowHexSpecifier) != NumberStyles.None)
            {
                if (!HexNumberToUInt64(ref number, ref num))
                {
                    throw new OverflowException(Environment.GetResourceString("Overflow_UInt64"));
                }
                return num;
            }
            if (!NumberToUInt64(ref number, ref num))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_UInt64"));
            }
            return num;
        }

        [SecuritySafeCritical]
        private static unsafe void StringToNumber(string str, NumberStyles options, ref NumberBuffer number, NumberFormatInfo info, bool parseDecimal)
        {
            if (str == null)
            {
                throw new ArgumentNullException("String");
            }
            fixed (char* str2 = ((char*) str))
            {
                char* chPtr = str2;
                char* chPtr2 = chPtr;
                if (!ParseNumber(ref chPtr2, options, ref number, null, info, parseDecimal) || ((((long) ((chPtr2 - chPtr) / 2)) < str.Length) && !TrailingZeros(str, (int) ((long) ((chPtr2 - chPtr) / 2)))))
                {
                    throw new FormatException(Environment.GetResourceString("Format_InvalidString"));
                }
            }
        }

        private static bool TrailingZeros(string s, int index)
        {
            for (int i = index; i < s.Length; i++)
            {
                if (s[i] != '\0')
                {
                    return false;
                }
            }
            return true;
        }

        [SecuritySafeCritical]
        internal static unsafe bool TryParseDecimal(string value, NumberStyles options, NumberFormatInfo numfmt, out decimal result)
        {
            byte* stackBuffer = stackalloc byte[0x72];
            NumberBuffer number = new NumberBuffer(stackBuffer);
            result = 0M;
            if (!TryStringToNumber(value, options, ref number, numfmt, true))
            {
                return false;
            }
            if (!NumberBufferToDecimal(number.PackForNative(), ref result))
            {
                return false;
            }
            return true;
        }

        [SecuritySafeCritical]
        internal static unsafe bool TryParseDouble(string value, NumberStyles options, NumberFormatInfo numfmt, out double result)
        {
            byte* stackBuffer = stackalloc byte[0x72];
            NumberBuffer number = new NumberBuffer(stackBuffer);
            result = 0.0;
            if (!TryStringToNumber(value, options, ref number, numfmt, false))
            {
                return false;
            }
            if (!NumberBufferToDouble(number.PackForNative(), ref result))
            {
                return false;
            }
            return true;
        }

        [SecuritySafeCritical]
        internal static unsafe bool TryParseInt32(string s, NumberStyles style, NumberFormatInfo info, out int result)
        {
            byte* stackBuffer = stackalloc byte[0x72];
            NumberBuffer number = new NumberBuffer(stackBuffer);
            result = 0;
            if (!TryStringToNumber(s, style, ref number, info, false))
            {
                return false;
            }
            if ((style & NumberStyles.AllowHexSpecifier) != NumberStyles.None)
            {
                if (!HexNumberToInt32(ref number, ref result))
                {
                    return false;
                }
            }
            else if (!NumberToInt32(ref number, ref result))
            {
                return false;
            }
            return true;
        }

        [SecuritySafeCritical]
        internal static unsafe bool TryParseInt64(string s, NumberStyles style, NumberFormatInfo info, out long result)
        {
            byte* stackBuffer = stackalloc byte[0x72];
            NumberBuffer number = new NumberBuffer(stackBuffer);
            result = 0L;
            if (!TryStringToNumber(s, style, ref number, info, false))
            {
                return false;
            }
            if ((style & NumberStyles.AllowHexSpecifier) != NumberStyles.None)
            {
                if (!HexNumberToInt64(ref number, ref result))
                {
                    return false;
                }
            }
            else if (!NumberToInt64(ref number, ref result))
            {
                return false;
            }
            return true;
        }

        [SecuritySafeCritical]
        internal static unsafe bool TryParseSingle(string value, NumberStyles options, NumberFormatInfo numfmt, out float result)
        {
            byte* stackBuffer = stackalloc byte[0x72];
            NumberBuffer number = new NumberBuffer(stackBuffer);
            result = 0f;
            double num = 0.0;
            if (!TryStringToNumber(value, options, ref number, numfmt, false))
            {
                return false;
            }
            if (!NumberBufferToDouble(number.PackForNative(), ref num))
            {
                return false;
            }
            float f = (float) num;
            if (float.IsInfinity(f))
            {
                return false;
            }
            result = f;
            return true;
        }

        [SecuritySafeCritical]
        internal static unsafe bool TryParseUInt32(string s, NumberStyles style, NumberFormatInfo info, out uint result)
        {
            byte* stackBuffer = stackalloc byte[0x72];
            NumberBuffer number = new NumberBuffer(stackBuffer);
            result = 0;
            if (!TryStringToNumber(s, style, ref number, info, false))
            {
                return false;
            }
            if ((style & NumberStyles.AllowHexSpecifier) != NumberStyles.None)
            {
                if (!HexNumberToUInt32(ref number, ref result))
                {
                    return false;
                }
            }
            else if (!NumberToUInt32(ref number, ref result))
            {
                return false;
            }
            return true;
        }

        [SecuritySafeCritical]
        internal static unsafe bool TryParseUInt64(string s, NumberStyles style, NumberFormatInfo info, out ulong result)
        {
            byte* stackBuffer = stackalloc byte[0x72];
            NumberBuffer number = new NumberBuffer(stackBuffer);
            result = 0L;
            if (!TryStringToNumber(s, style, ref number, info, false))
            {
                return false;
            }
            if ((style & NumberStyles.AllowHexSpecifier) != NumberStyles.None)
            {
                if (!HexNumberToUInt64(ref number, ref result))
                {
                    return false;
                }
            }
            else if (!NumberToUInt64(ref number, ref result))
            {
                return false;
            }
            return true;
        }

        [SecuritySafeCritical]
        internal static bool TryStringToNumber(string str, NumberStyles options, ref NumberBuffer number, NumberFormatInfo numfmt, bool parseDecimal)
        {
            return TryStringToNumber(str, options, ref number, null, numfmt, parseDecimal);
        }

        [FriendAccessAllowed, SecuritySafeCritical]
        internal static unsafe bool TryStringToNumber(string str, NumberStyles options, ref NumberBuffer number, StringBuilder sb, NumberFormatInfo numfmt, bool parseDecimal)
        {
            if (str == null)
            {
                return false;
            }
            fixed (char* str2 = ((char*) str))
            {
                char* chPtr = str2;
                char* chPtr2 = chPtr;
                if (!ParseNumber(ref chPtr2, options, ref number, sb, numfmt, parseDecimal) || ((((long) ((chPtr2 - chPtr) / 2)) < str.Length) && !TrailingZeros(str, (int) ((long) ((chPtr2 - chPtr) / 2)))))
                {
                    return false;
                }
            }
            return true;
        }

        [StructLayout(LayoutKind.Sequential), FriendAccessAllowed]
        internal struct NumberBuffer
        {
            public const int NumberBufferBytes = 0x72;
            private unsafe byte* baseAddress;
            public unsafe char* digits;
            public int precision;
            public int scale;
            public bool sign;
            [SecurityCritical]
            public unsafe NumberBuffer(byte* stackBuffer)
            {
                this.baseAddress = stackBuffer;
                this.digits = (char*) (stackBuffer + 12);
                this.precision = 0;
                this.scale = 0;
                this.sign = false;
            }

            [SecurityCritical]
            public unsafe byte* PackForNative()
            {
                int* baseAddress = (int*) this.baseAddress;
                baseAddress[0] = this.precision;
                baseAddress[1] = this.scale;
                baseAddress[2] = this.sign ? 1 : 0;
                return this.baseAddress;
            }
        }
    }
}

