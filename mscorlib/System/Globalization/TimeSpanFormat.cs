namespace System.Globalization
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    internal static class TimeSpanFormat
    {
        internal static readonly FormatLiterals NegativeInvariantFormatLiterals = FormatLiterals.InitInvariant(true);
        internal static readonly FormatLiterals PositiveInvariantFormatLiterals = FormatLiterals.InitInvariant(false);

        internal static string Format(TimeSpan value, string format, IFormatProvider formatProvider)
        {
            Pattern minimum;
            if ((format == null) || (format.Length == 0))
            {
                format = "c";
            }
            if (format.Length != 1)
            {
                return FormatCustomized(value, format, DateTimeFormatInfo.GetInstance(formatProvider));
            }
            char ch = format[0];
            switch (ch)
            {
                case 'c':
                case 't':
                case 'T':
                    return FormatStandard(value, true, format, Pattern.Minimum);
            }
            if ((ch != 'g') && (ch != 'G'))
            {
                throw new FormatException(Environment.GetResourceString("Format_InvalidString"));
            }
            DateTimeFormatInfo instance = DateTimeFormatInfo.GetInstance(formatProvider);
            if (value._ticks < 0L)
            {
                format = instance.FullTimeSpanNegativePattern;
            }
            else
            {
                format = instance.FullTimeSpanPositivePattern;
            }
            if (ch == 'g')
            {
                minimum = Pattern.Minimum;
            }
            else
            {
                minimum = Pattern.Full;
            }
            return FormatStandard(value, false, format, minimum);
        }

        internal static string FormatCustomized(TimeSpan value, string format, DateTimeFormatInfo dtfi)
        {
            int num = (int) (value._ticks / 0xc92a69c000L);
            long num2 = value._ticks % 0xc92a69c000L;
            if (value._ticks < 0L)
            {
                num = -num;
                num2 = -num2;
            }
            int num3 = (int) ((num2 / 0x861c46800L) % 0x18L);
            int num4 = (int) ((num2 / 0x23c34600L) % 60L);
            int num5 = (int) ((num2 / 0x989680L) % 60L);
            int num6 = (int) (num2 % 0x989680L);
            long num7 = 0L;
            int pos = 0;
            StringBuilder outputBuffer = new StringBuilder();
            while (pos < format.Length)
            {
                int num9;
                int num10;
                char patternChar = format[pos];
                switch (patternChar)
                {
                    case '%':
                    {
                        num10 = DateTimeFormat.ParseNextChar(format, pos);
                        if ((num10 < 0) || (num10 == 0x25))
                        {
                            throw new FormatException(Environment.GetResourceString("Format_InvalidString"));
                        }
                        char ch3 = (char) num10;
                        outputBuffer.Append(FormatCustomized(value, ch3.ToString(), dtfi));
                        num9 = 2;
                        goto Label_035D;
                    }
                    case '\'':
                    case '"':
                    {
                        StringBuilder result = new StringBuilder();
                        num9 = DateTimeFormat.ParseQuoteString(format, pos, result);
                        outputBuffer.Append(result);
                        goto Label_035D;
                    }
                    case 'F':
                    {
                        num9 = DateTimeFormat.ParseRepeatPattern(format, pos, patternChar);
                        if (num9 > 7)
                        {
                            throw new FormatException(Environment.GetResourceString("Format_InvalidString"));
                        }
                        num7 = num6;
                        num7 /= (long) Math.Pow(10.0, (double) (7 - num9));
                        int num11 = num9;
                        while (num11 > 0)
                        {
                            if ((num7 % 10L) != 0L)
                            {
                                break;
                            }
                            num7 /= 10L;
                            num11--;
                        }
                        if (num11 > 0)
                        {
                            outputBuffer.Append(num7.ToString(DateTimeFormat.fixedNumberFormats[num11 - 1], CultureInfo.InvariantCulture));
                        }
                        goto Label_035D;
                    }
                    case 'm':
                        num9 = DateTimeFormat.ParseRepeatPattern(format, pos, patternChar);
                        if (num9 > 2)
                        {
                            throw new FormatException(Environment.GetResourceString("Format_InvalidString"));
                        }
                        DateTimeFormat.FormatDigits(outputBuffer, num4, num9);
                        goto Label_035D;

                    case 's':
                        num9 = DateTimeFormat.ParseRepeatPattern(format, pos, patternChar);
                        if (num9 > 2)
                        {
                            throw new FormatException(Environment.GetResourceString("Format_InvalidString"));
                        }
                        DateTimeFormat.FormatDigits(outputBuffer, num5, num9);
                        goto Label_035D;

                    case 'd':
                        num9 = DateTimeFormat.ParseRepeatPattern(format, pos, patternChar);
                        if (num9 > 8)
                        {
                            throw new FormatException(Environment.GetResourceString("Format_InvalidString"));
                        }
                        goto Label_02A7;

                    case 'f':
                        num9 = DateTimeFormat.ParseRepeatPattern(format, pos, patternChar);
                        if (num9 > 7)
                        {
                            throw new FormatException(Environment.GetResourceString("Format_InvalidString"));
                        }
                        goto Label_01B8;

                    case 'h':
                        num9 = DateTimeFormat.ParseRepeatPattern(format, pos, patternChar);
                        if (num9 > 2)
                        {
                            throw new FormatException(Environment.GetResourceString("Format_InvalidString"));
                        }
                        break;

                    case '\\':
                        num10 = DateTimeFormat.ParseNextChar(format, pos);
                        if (num10 < 0)
                        {
                            throw new FormatException(Environment.GetResourceString("Format_InvalidString"));
                        }
                        outputBuffer.Append((char) num10);
                        num9 = 2;
                        goto Label_035D;

                    default:
                        throw new FormatException(Environment.GetResourceString("Format_InvalidString"));
                }
                DateTimeFormat.FormatDigits(outputBuffer, num3, num9);
                goto Label_035D;
            Label_01B8:
                num7 = num6;
                outputBuffer.Append((num7 / ((long) Math.Pow(10.0, (double) (7 - num9)))).ToString(DateTimeFormat.fixedNumberFormats[num9 - 1], CultureInfo.InvariantCulture));
                goto Label_035D;
            Label_02A7:
                DateTimeFormat.FormatDigits(outputBuffer, num, num9, true);
            Label_035D:
                pos += num9;
            }
            return outputBuffer.ToString();
        }

        private static string FormatStandard(TimeSpan value, bool isInvariant, string format, Pattern pattern)
        {
            FormatLiterals negativeInvariantFormatLiterals;
            StringBuilder builder = new StringBuilder();
            int num = (int) (value._ticks / 0xc92a69c000L);
            long num2 = value._ticks % 0xc92a69c000L;
            if (value._ticks < 0L)
            {
                num = -num;
                num2 = -num2;
            }
            int n = (int) ((num2 / 0x861c46800L) % 0x18L);
            int num4 = (int) ((num2 / 0x23c34600L) % 60L);
            int num5 = (int) ((num2 / 0x989680L) % 60L);
            int num6 = (int) (num2 % 0x989680L);
            if (isInvariant)
            {
                if (value._ticks < 0L)
                {
                    negativeInvariantFormatLiterals = NegativeInvariantFormatLiterals;
                }
                else
                {
                    negativeInvariantFormatLiterals = PositiveInvariantFormatLiterals;
                }
            }
            else
            {
                negativeInvariantFormatLiterals = new FormatLiterals();
                negativeInvariantFormatLiterals.Init(format, pattern == Pattern.Full);
            }
            if (num6 != 0)
            {
                num6 = (int) (((long) num6) / ((long) Math.Pow(10.0, (double) (7 - negativeInvariantFormatLiterals.ff))));
            }
            builder.Append(negativeInvariantFormatLiterals.Start);
            if ((pattern == Pattern.Full) || (num != 0))
            {
                builder.Append(num);
                builder.Append(negativeInvariantFormatLiterals.DayHourSep);
            }
            builder.Append(IntToString(n, negativeInvariantFormatLiterals.hh));
            builder.Append(negativeInvariantFormatLiterals.HourMinuteSep);
            builder.Append(IntToString(num4, negativeInvariantFormatLiterals.mm));
            builder.Append(negativeInvariantFormatLiterals.MinuteSecondSep);
            builder.Append(IntToString(num5, negativeInvariantFormatLiterals.ss));
            if (isInvariant || (pattern != Pattern.Minimum))
            {
                if ((pattern == Pattern.Full) || (num6 != 0))
                {
                    builder.Append(negativeInvariantFormatLiterals.SecondFractionSep);
                    builder.Append(IntToString(num6, negativeInvariantFormatLiterals.ff));
                }
            }
            else
            {
                int ff = negativeInvariantFormatLiterals.ff;
                while (ff > 0)
                {
                    if ((num6 % 10) != 0)
                    {
                        break;
                    }
                    num6 /= 10;
                    ff--;
                }
                if (ff > 0)
                {
                    builder.Append(negativeInvariantFormatLiterals.SecondFractionSep);
                    builder.Append(num6.ToString(DateTimeFormat.fixedNumberFormats[ff - 1], CultureInfo.InvariantCulture));
                }
            }
            builder.Append(negativeInvariantFormatLiterals.End);
            return builder.ToString();
        }

        [SecuritySafeCritical]
        private static string IntToString(int n, int digits)
        {
            return ParseNumbers.IntToString(n, 10, digits, '0', 0);
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FormatLiterals
        {
            internal string AppCompatLiteral;
            internal int dd;
            internal int hh;
            internal int mm;
            internal int ss;
            internal int ff;
            private string[] literals;
            internal string Start
            {
                get
                {
                    return this.literals[0];
                }
            }
            internal string DayHourSep
            {
                get
                {
                    return this.literals[1];
                }
            }
            internal string HourMinuteSep
            {
                get
                {
                    return this.literals[2];
                }
            }
            internal string MinuteSecondSep
            {
                get
                {
                    return this.literals[3];
                }
            }
            internal string SecondFractionSep
            {
                get
                {
                    return this.literals[4];
                }
            }
            internal string End
            {
                get
                {
                    return this.literals[5];
                }
            }
            internal static TimeSpanFormat.FormatLiterals InitInvariant(bool isNegative)
            {
                return new TimeSpanFormat.FormatLiterals { literals = new string[] { isNegative ? "-" : string.Empty, ".", ":", ":", ".", string.Empty }, AppCompatLiteral = ":.", dd = 2, hh = 2, mm = 2, ss = 2, ff = 7 };
            }

            internal void Init(string format, bool useInvariantFieldLengths)
            {
                this.literals = new string[6];
                for (int i = 0; i < this.literals.Length; i++)
                {
                    this.literals[i] = string.Empty;
                }
                this.dd = 0;
                this.hh = 0;
                this.mm = 0;
                this.ss = 0;
                this.ff = 0;
                StringBuilder builder = new StringBuilder();
                bool flag = false;
                char ch = '\'';
                int index = 0;
                for (int j = 0; j < format.Length; j++)
                {
                    switch (format[j])
                    {
                        case '\'':
                        case '"':
                        {
                            if (flag && (ch == format[j]))
                            {
                                if ((index < 0) || (index > 5))
                                {
                                    return;
                                }
                                this.literals[index] = builder.ToString();
                                builder.Length = 0;
                                flag = false;
                            }
                            else if (!flag)
                            {
                                ch = format[j];
                                flag = true;
                            }
                            continue;
                        }
                        case 'F':
                        case 'f':
                        {
                            if (!flag)
                            {
                                index = 5;
                                this.ff++;
                            }
                            continue;
                        }
                        case 'm':
                        {
                            if (!flag)
                            {
                                index = 3;
                                this.mm++;
                            }
                            continue;
                        }
                        case 's':
                        {
                            if (!flag)
                            {
                                index = 4;
                                this.ss++;
                            }
                            continue;
                        }
                        case 'd':
                        {
                            if (!flag)
                            {
                                index = 1;
                                this.dd++;
                            }
                            continue;
                        }
                        case 'h':
                        {
                            if (!flag)
                            {
                                index = 2;
                                this.hh++;
                            }
                            continue;
                        }
                        case '\\':
                        {
                            if (flag)
                            {
                                break;
                            }
                            j++;
                            continue;
                        }
                    }
                    builder.Append(format[j]);
                }
                this.AppCompatLiteral = this.MinuteSecondSep + this.SecondFractionSep;
                if (useInvariantFieldLengths)
                {
                    this.dd = 2;
                    this.hh = 2;
                    this.mm = 2;
                    this.ss = 2;
                    this.ff = 7;
                }
                else
                {
                    if ((this.dd < 1) || (this.dd > 2))
                    {
                        this.dd = 2;
                    }
                    if ((this.hh < 1) || (this.hh > 2))
                    {
                        this.hh = 2;
                    }
                    if ((this.mm < 1) || (this.mm > 2))
                    {
                        this.mm = 2;
                    }
                    if ((this.ss < 1) || (this.ss > 2))
                    {
                        this.ss = 2;
                    }
                    if ((this.ff < 1) || (this.ff > 7))
                    {
                        this.ff = 7;
                    }
                }
            }
        }

        internal enum Pattern
        {
            None,
            Minimum,
            Full
        }
    }
}

