namespace System.Numerics
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    internal static class BigNumber
    {
        private const NumberStyles InvalidNumberStyles = ~(NumberStyles.HexNumber | NumberStyles.AllowCurrencySymbol | NumberStyles.AllowExponent | NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint | NumberStyles.AllowParentheses | NumberStyles.AllowTrailingSign | NumberStyles.AllowLeadingSign);

        [SecuritySafeCritical]
        internal static unsafe string FormatBigInteger(BigInteger value, string format, NumberFormatInfo info)
        {
            int num3;
            int num9;
            int digits = 0;
            char ch = ParseFormatSpecifier(format, out digits);
            switch (ch)
            {
                case 'x':
                case 'X':
                    return FormatBigIntegerToHexString(value, ch, digits, info);
            }
            bool flag = ((((ch == 'g') || (ch == 'G')) || ((ch == 'd') || (ch == 'D'))) || (ch == 'r')) || (ch == 'R');
            if (value._bits == null)
            {
                switch (ch)
                {
                    case 'g':
                    case 'G':
                    case 'r':
                    case 'R':
                        if (digits > 0)
                        {
                            format = string.Format(CultureInfo.InvariantCulture, "D{0}", new object[] { digits.ToString(CultureInfo.InvariantCulture) });
                        }
                        else
                        {
                            format = "D";
                        }
                        break;
                }
                return value._sign.ToString(format, info);
            }
            int num2 = BigInteger.Length(value._bits);
            try
            {
                num3 = ((num2 * 10) / 9) + 2;
            }
            catch (OverflowException exception)
            {
                throw new FormatException(SR.GetString("Format_TooLarge"), exception);
            }
            uint[] numArray = new uint[num3];
            int num4 = 0;
            int index = num2;
            while (--index >= 0)
            {
                uint uLo = value._bits[index];
                for (int k = 0; k < num4; k++)
                {
                    ulong num8 = NumericsHelpers.MakeUlong(numArray[k], uLo);
                    numArray[k] = (uint) (num8 % ((ulong) 0x3b9aca00L));
                    uLo = (uint) (num8 / ((ulong) 0x3b9aca00L));
                }
                if (uLo != 0)
                {
                    numArray[num4++] = uLo % 0x3b9aca00;
                    uLo /= 0x3b9aca00;
                    if (uLo != 0)
                    {
                        numArray[num4++] = uLo;
                    }
                }
            }
            try
            {
                num9 = num4 * 9;
            }
            catch (OverflowException exception2)
            {
                throw new FormatException(SR.GetString("Format_TooLarge"), exception2);
            }
            if (flag)
            {
                if ((digits > 0) && (digits > num9))
                {
                    num9 = digits;
                }
                if (value._sign < 0)
                {
                    try
                    {
                        num9 += info.NegativeSign.Length;
                    }
                    catch (OverflowException exception3)
                    {
                        throw new FormatException(SR.GetString("Format_TooLarge"), exception3);
                    }
                }
            }
            char[] chArray = new char[num9];
            int startIndex = num9;
            for (int i = 0; i < (num4 - 1); i++)
            {
                uint num12 = numArray[i];
                int num13 = 9;
                while (--num13 >= 0)
                {
                    chArray[--startIndex] = (char) (0x30 + (num12 % 10));
                    num12 /= 10;
                }
            }
            for (uint j = numArray[num4 - 1]; j != 0; j /= 10)
            {
                chArray[--startIndex] = (char) (0x30 + (j % 10));
            }
            if (!flag)
            {
                byte* stackBuffer = stackalloc byte[0x72];
                Number.NumberBuffer buffer = new Number.NumberBuffer(stackBuffer) {
                    sign = value._sign < 0,
                    precision = 0x1d
                };
                buffer.digits[0] = '\0';
                buffer.scale = num9 - startIndex;
                int num15 = Math.Min(startIndex + 50, num9);
                for (int m = startIndex; m < num15; m++)
                {
                    buffer.digits[m - startIndex] = chArray[m];
                }
                return Number.FormatNumberBuffer(buffer.PackForNative(), format, info);
            }
            int num17 = num9 - startIndex;
            while ((digits > 0) && (digits > num17))
            {
                chArray[--startIndex] = '0';
                digits--;
            }
            if (value._sign < 0)
            {
                string negativeSign = info.NegativeSign;
                for (int n = info.NegativeSign.Length - 1; n > -1; n--)
                {
                    chArray[--startIndex] = info.NegativeSign[n];
                }
            }
            return new string(chArray, startIndex, num9 - startIndex);
        }

        private static string FormatBigIntegerToHexString(BigInteger value, char format, int digits, NumberFormatInfo info)
        {
            StringBuilder builder = new StringBuilder();
            byte[] buffer = value.ToByteArray();
            string str = null;
            int index = buffer.Length - 1;
            if (index > -1)
            {
                bool flag = false;
                byte num2 = buffer[index];
                if (num2 > 0xf7)
                {
                    num2 = (byte) (num2 - 240);
                    flag = true;
                }
                if ((num2 < 8) || flag)
                {
                    str = string.Format(CultureInfo.InvariantCulture, "{0}1", new object[] { format });
                    builder.Append(num2.ToString(str, info));
                    index--;
                }
            }
            if (index > -1)
            {
                str = string.Format(CultureInfo.InvariantCulture, "{0}2", new object[] { format });
                while (index > -1)
                {
                    builder.Append(buffer[index--].ToString(str, info));
                }
            }
            if ((digits > 0) && (digits > builder.Length))
            {
                builder.Insert(0, (value._sign >= 0) ? "0" : ((format == 'x') ? "f" : "F"), digits - builder.Length);
            }
            return builder.ToString();
        }

        private static bool HexNumberToBigInteger(ref BigNumberBuffer number, ref BigInteger value)
        {
            if ((number.digits == null) || (number.digits.Length == 0))
            {
                return false;
            }
            int num = number.digits.Length - 1;
            byte[] buffer = new byte[(num / 2) + (num % 2)];
            bool flag = false;
            bool flag2 = false;
            int index = 0;
            for (int i = num - 1; i > -1; i--)
            {
                byte num4;
                char ch = number.digits[i];
                if ((ch >= '0') && (ch <= '9'))
                {
                    num4 = (byte) (ch - '0');
                }
                else if ((ch >= 'A') && (ch <= 'F'))
                {
                    num4 = (byte) ((ch - 'A') + 10);
                }
                else
                {
                    num4 = (byte) ((ch - 'a') + 10);
                }
                if ((i == 0) && ((num4 & 8) == 8))
                {
                    flag2 = true;
                }
                if (flag)
                {
                    buffer[index] = (byte) (buffer[index] | (num4 << 4));
                    index++;
                }
                else
                {
                    buffer[index] = flag2 ? ((byte) (num4 | 240)) : num4;
                }
                flag = !flag;
            }
            value = new BigInteger(buffer);
            return true;
        }

        private static bool NumberToBigInteger(ref BigNumberBuffer number, ref BigInteger value)
        {
            int scale = number.scale;
            int num2 = 0;
            value = 0;
            while (--scale >= 0)
            {
                value *= 10;
                if (number.digits[num2] != '\0')
                {
                    value += number.digits[num2++] - '0';
                }
            }
            while (number.digits[num2] != '\0')
            {
                if (number.digits[num2++] != '0')
                {
                    return false;
                }
            }
            if (number.sign)
            {
                value = -value;
            }
            return true;
        }

        internal static BigInteger ParseBigInteger(string value, NumberStyles style, NumberFormatInfo info)
        {
            ArgumentException exception;
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (!TryValidateParseStyleInteger(style, out exception))
            {
                throw exception;
            }
            BigInteger zero = BigInteger.Zero;
            if (!TryParseBigInteger(value, style, info, out zero))
            {
                throw new FormatException(SR.GetString("Overflow_ParseBigInteger"));
            }
            return zero;
        }

        internal static char ParseFormatSpecifier(string format, out int digits)
        {
            digits = -1;
            if (string.IsNullOrEmpty(format))
            {
                return 'R';
            }
            int num = 0;
            char ch = format[num];
            if (((ch >= 'A') && (ch <= 'Z')) || ((ch >= 'a') && (ch <= 'z')))
            {
                num++;
                int num2 = -1;
                if (((num < format.Length) && (format[num] >= '0')) && (format[num] <= '9'))
                {
                    num2 = format[num++] - '0';
                    while (((num < format.Length) && (format[num] >= '0')) && (format[num] <= '9'))
                    {
                        num2 = (num2 * 10) + (format[num++] - '0');
                        if (num2 >= 10)
                        {
                            break;
                        }
                    }
                }
                if ((num >= format.Length) || (format[num] == '\0'))
                {
                    digits = num2;
                    return ch;
                }
            }
            return '\0';
        }

        [SecuritySafeCritical]
        internal static unsafe bool TryParseBigInteger(string value, NumberStyles style, NumberFormatInfo info, out BigInteger result)
        {
            ArgumentException exception;
            result = BigInteger.Zero;
            if (!TryValidateParseStyleInteger(style, out exception))
            {
                throw exception;
            }
            BigNumberBuffer number = BigNumberBuffer.Create();
            byte* stackBuffer = stackalloc byte[0x72];
            Number.NumberBuffer buffer2 = new Number.NumberBuffer(stackBuffer);
            result = 0;
            if (!Number.TryStringToNumber(value, style, ref buffer2, number.digits, info, false))
            {
                return false;
            }
            number.precision = buffer2.precision;
            number.scale = buffer2.scale;
            number.sign = buffer2.sign;
            if ((style & NumberStyles.AllowHexSpecifier) != NumberStyles.None)
            {
                if (!HexNumberToBigInteger(ref number, ref result))
                {
                    return false;
                }
            }
            else if (!NumberToBigInteger(ref number, ref result))
            {
                return false;
            }
            return true;
        }

        internal static bool TryValidateParseStyleInteger(NumberStyles style, out ArgumentException e)
        {
            if ((style & ~(NumberStyles.HexNumber | NumberStyles.AllowCurrencySymbol | NumberStyles.AllowExponent | NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint | NumberStyles.AllowParentheses | NumberStyles.AllowTrailingSign | NumberStyles.AllowLeadingSign)) != NumberStyles.None)
            {
                e = new ArgumentException(SR.GetString("Argument_InvalidNumberStyles", new object[] { "style" }));
                return false;
            }
            if (((style & NumberStyles.AllowHexSpecifier) != NumberStyles.None) && ((style & ~NumberStyles.HexNumber) != NumberStyles.None))
            {
                e = new ArgumentException(SR.GetString("Argument_InvalidHexStyle"));
                return false;
            }
            e = null;
            return true;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct BigNumberBuffer
        {
            public StringBuilder digits;
            public int precision;
            public int scale;
            public bool sign;
            public static BigNumber.BigNumberBuffer Create()
            {
                return new BigNumber.BigNumberBuffer { digits = new StringBuilder() };
            }
        }
    }
}

