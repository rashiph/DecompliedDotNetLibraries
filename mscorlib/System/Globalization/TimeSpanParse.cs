namespace System.Globalization
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class TimeSpanParse
    {
        internal const int maxDays = 0xa2e3ff;
        internal const int maxFraction = 0x98967f;
        internal const int maxFractionDigits = 7;
        internal const int maxHours = 0x17;
        internal const int maxMinutes = 0x3b;
        internal const int maxSeconds = 0x3b;
        internal const int unlimitedDigits = -1;
        private static readonly TimeSpanToken zero = new TimeSpanToken(0);

        internal static TimeSpan Parse(string input, IFormatProvider formatProvider)
        {
            TimeSpanResult result = new TimeSpanResult();
            result.Init(TimeSpanThrowStyle.All);
            if (!TryParseTimeSpan(input, TimeSpanStandardStyles.Any, formatProvider, ref result))
            {
                throw result.GetTimeSpanParseException();
            }
            return result.parsedTimeSpan;
        }

        internal static TimeSpan ParseExact(string input, string format, IFormatProvider formatProvider, TimeSpanStyles styles)
        {
            TimeSpanResult result = new TimeSpanResult();
            result.Init(TimeSpanThrowStyle.All);
            if (!TryParseExactTimeSpan(input, format, formatProvider, styles, ref result))
            {
                throw result.GetTimeSpanParseException();
            }
            return result.parsedTimeSpan;
        }

        private static bool ParseExactDigits(ref TimeSpanTokenizer tokenizer, int minDigitLength, out int result)
        {
            result = 0;
            int zeroes = 0;
            int maxDigitLength = (minDigitLength == 1) ? 2 : minDigitLength;
            return ParseExactDigits(ref tokenizer, minDigitLength, maxDigitLength, out zeroes, out result);
        }

        private static bool ParseExactDigits(ref TimeSpanTokenizer tokenizer, int minDigitLength, int maxDigitLength, out int zeroes, out int result)
        {
            result = 0;
            zeroes = 0;
            int num = 0;
            while (num < maxDigitLength)
            {
                char nextChar = tokenizer.NextChar;
                if ((nextChar < '0') || (nextChar > '9'))
                {
                    tokenizer.BackOne();
                    break;
                }
                result = (result * 10) + (nextChar - '0');
                if (result == 0)
                {
                    zeroes++;
                }
                num++;
            }
            return (num >= minDigitLength);
        }

        private static bool ParseExactLiteral(ref TimeSpanTokenizer tokenizer, StringBuilder enquotedString)
        {
            for (int i = 0; i < enquotedString.Length; i++)
            {
                if (enquotedString[i] != tokenizer.NextChar)
                {
                    return false;
                }
            }
            return true;
        }

        internal static TimeSpan ParseExactMultiple(string input, string[] formats, IFormatProvider formatProvider, TimeSpanStyles styles)
        {
            TimeSpanResult result = new TimeSpanResult();
            result.Init(TimeSpanThrowStyle.All);
            if (!TryParseExactMultipleTimeSpan(input, formats, formatProvider, styles, ref result))
            {
                throw result.GetTimeSpanParseException();
            }
            return result.parsedTimeSpan;
        }

        private static bool ProcessTerminal_D(ref TimeSpanRawInfo raw, TimeSpanStandardStyles style, ref TimeSpanResult result)
        {
            if (((raw.SepCount != 2) || (raw.NumCount != 1)) || ((style & TimeSpanStandardStyles.RequireFull) != TimeSpanStandardStyles.None))
            {
                result.SetFailure(ParseFailureKind.Format, "Format_BadTimeSpan");
                return false;
            }
            bool flag = (style & TimeSpanStandardStyles.Invariant) != TimeSpanStandardStyles.None;
            bool flag2 = (style & TimeSpanStandardStyles.Localized) != TimeSpanStandardStyles.None;
            bool positive = false;
            bool flag4 = false;
            if (flag)
            {
                if (raw.FullDMatch(raw.PositiveInvariant))
                {
                    flag4 = true;
                    positive = true;
                }
                if (!flag4 && raw.FullDMatch(raw.NegativeInvariant))
                {
                    flag4 = true;
                    positive = false;
                }
            }
            if (flag2)
            {
                if (!flag4 && raw.FullDMatch(raw.PositiveLocalized))
                {
                    flag4 = true;
                    positive = true;
                }
                if (!flag4 && raw.FullDMatch(raw.NegativeLocalized))
                {
                    flag4 = true;
                    positive = false;
                }
            }
            long num = 0L;
            if (flag4)
            {
                if (!TryTimeToTicks(positive, raw.numbers[0], zero, zero, zero, zero, out num))
                {
                    result.SetFailure(ParseFailureKind.Overflow, "Overflow_TimeSpanElementTooLarge");
                    return false;
                }
                if (!positive)
                {
                    num = -num;
                    if (num > 0L)
                    {
                        result.SetFailure(ParseFailureKind.Overflow, "Overflow_TimeSpanElementTooLarge");
                        return false;
                    }
                }
                result.parsedTimeSpan._ticks = num;
                return true;
            }
            result.SetFailure(ParseFailureKind.Format, "Format_BadTimeSpan");
            return false;
        }

        private static bool ProcessTerminal_DHMSF(ref TimeSpanRawInfo raw, TimeSpanStandardStyles style, ref TimeSpanResult result)
        {
            if ((raw.SepCount != 6) || (raw.NumCount != 5))
            {
                result.SetFailure(ParseFailureKind.Format, "Format_BadTimeSpan");
                return false;
            }
            bool flag = (style & TimeSpanStandardStyles.Invariant) != TimeSpanStandardStyles.None;
            bool flag2 = (style & TimeSpanStandardStyles.Localized) != TimeSpanStandardStyles.None;
            bool positive = false;
            bool flag4 = false;
            if (flag)
            {
                if (raw.FullMatch(raw.PositiveInvariant))
                {
                    flag4 = true;
                    positive = true;
                }
                if (!flag4 && raw.FullMatch(raw.NegativeInvariant))
                {
                    flag4 = true;
                    positive = false;
                }
            }
            if (flag2)
            {
                if (!flag4 && raw.FullMatch(raw.PositiveLocalized))
                {
                    flag4 = true;
                    positive = true;
                }
                if (!flag4 && raw.FullMatch(raw.NegativeLocalized))
                {
                    flag4 = true;
                    positive = false;
                }
            }
            if (flag4)
            {
                long num;
                if (!TryTimeToTicks(positive, raw.numbers[0], raw.numbers[1], raw.numbers[2], raw.numbers[3], raw.numbers[4], out num))
                {
                    result.SetFailure(ParseFailureKind.Overflow, "Overflow_TimeSpanElementTooLarge");
                    return false;
                }
                if (!positive)
                {
                    num = -num;
                    if (num > 0L)
                    {
                        result.SetFailure(ParseFailureKind.Overflow, "Overflow_TimeSpanElementTooLarge");
                        return false;
                    }
                }
                result.parsedTimeSpan._ticks = num;
                return true;
            }
            result.SetFailure(ParseFailureKind.Format, "Format_BadTimeSpan");
            return false;
        }

        private static bool ProcessTerminal_HM(ref TimeSpanRawInfo raw, TimeSpanStandardStyles style, ref TimeSpanResult result)
        {
            if (((raw.SepCount != 3) || (raw.NumCount != 2)) || ((style & TimeSpanStandardStyles.RequireFull) != TimeSpanStandardStyles.None))
            {
                result.SetFailure(ParseFailureKind.Format, "Format_BadTimeSpan");
                return false;
            }
            bool flag = (style & TimeSpanStandardStyles.Invariant) != TimeSpanStandardStyles.None;
            bool flag2 = (style & TimeSpanStandardStyles.Localized) != TimeSpanStandardStyles.None;
            bool positive = false;
            bool flag4 = false;
            if (flag)
            {
                if (raw.FullHMMatch(raw.PositiveInvariant))
                {
                    flag4 = true;
                    positive = true;
                }
                if (!flag4 && raw.FullHMMatch(raw.NegativeInvariant))
                {
                    flag4 = true;
                    positive = false;
                }
            }
            if (flag2)
            {
                if (!flag4 && raw.FullHMMatch(raw.PositiveLocalized))
                {
                    flag4 = true;
                    positive = true;
                }
                if (!flag4 && raw.FullHMMatch(raw.NegativeLocalized))
                {
                    flag4 = true;
                    positive = false;
                }
            }
            long num = 0L;
            if (flag4)
            {
                if (!TryTimeToTicks(positive, zero, raw.numbers[0], raw.numbers[1], zero, zero, out num))
                {
                    result.SetFailure(ParseFailureKind.Overflow, "Overflow_TimeSpanElementTooLarge");
                    return false;
                }
                if (!positive)
                {
                    num = -num;
                    if (num > 0L)
                    {
                        result.SetFailure(ParseFailureKind.Overflow, "Overflow_TimeSpanElementTooLarge");
                        return false;
                    }
                }
                result.parsedTimeSpan._ticks = num;
                return true;
            }
            result.SetFailure(ParseFailureKind.Format, "Format_BadTimeSpan");
            return false;
        }

        private static bool ProcessTerminal_HM_S_D(ref TimeSpanRawInfo raw, TimeSpanStandardStyles style, ref TimeSpanResult result)
        {
            if (((raw.SepCount != 4) || (raw.NumCount != 3)) || ((style & TimeSpanStandardStyles.RequireFull) != TimeSpanStandardStyles.None))
            {
                result.SetFailure(ParseFailureKind.Format, "Format_BadTimeSpan");
                return false;
            }
            bool flag = (style & TimeSpanStandardStyles.Invariant) != TimeSpanStandardStyles.None;
            bool flag2 = (style & TimeSpanStandardStyles.Localized) != TimeSpanStandardStyles.None;
            bool positive = false;
            bool flag4 = false;
            bool flag5 = false;
            long num = 0L;
            if (flag)
            {
                if (raw.FullHMSMatch(raw.PositiveInvariant))
                {
                    positive = true;
                    flag4 = TryTimeToTicks(positive, zero, raw.numbers[0], raw.numbers[1], raw.numbers[2], zero, out num);
                    flag5 = flag5 || !flag4;
                }
                if (!flag4 && raw.FullDHMMatch(raw.PositiveInvariant))
                {
                    positive = true;
                    flag4 = TryTimeToTicks(positive, raw.numbers[0], raw.numbers[1], raw.numbers[2], zero, zero, out num);
                    flag5 = flag5 || !flag4;
                }
                if (!flag4 && raw.PartialAppCompatMatch(raw.PositiveInvariant))
                {
                    positive = true;
                    flag4 = TryTimeToTicks(positive, zero, raw.numbers[0], raw.numbers[1], zero, raw.numbers[2], out num);
                    flag5 = flag5 || !flag4;
                }
                if (!flag4 && raw.FullHMSMatch(raw.NegativeInvariant))
                {
                    positive = false;
                    flag4 = TryTimeToTicks(positive, zero, raw.numbers[0], raw.numbers[1], raw.numbers[2], zero, out num);
                    flag5 = flag5 || !flag4;
                }
                if (!flag4 && raw.FullDHMMatch(raw.NegativeInvariant))
                {
                    positive = false;
                    flag4 = TryTimeToTicks(positive, raw.numbers[0], raw.numbers[1], raw.numbers[2], zero, zero, out num);
                    flag5 = flag5 || !flag4;
                }
                if (!flag4 && raw.PartialAppCompatMatch(raw.NegativeInvariant))
                {
                    positive = false;
                    flag4 = TryTimeToTicks(positive, zero, raw.numbers[0], raw.numbers[1], zero, raw.numbers[2], out num);
                    flag5 = flag5 || !flag4;
                }
            }
            if (flag2)
            {
                if (!flag4 && raw.FullHMSMatch(raw.PositiveLocalized))
                {
                    positive = true;
                    flag4 = TryTimeToTicks(positive, zero, raw.numbers[0], raw.numbers[1], raw.numbers[2], zero, out num);
                    flag5 = flag5 || !flag4;
                }
                if (!flag4 && raw.FullDHMMatch(raw.PositiveLocalized))
                {
                    positive = true;
                    flag4 = TryTimeToTicks(positive, raw.numbers[0], raw.numbers[1], raw.numbers[2], zero, zero, out num);
                    flag5 = flag5 || !flag4;
                }
                if (!flag4 && raw.PartialAppCompatMatch(raw.PositiveLocalized))
                {
                    positive = true;
                    flag4 = TryTimeToTicks(positive, zero, raw.numbers[0], raw.numbers[1], zero, raw.numbers[2], out num);
                    flag5 = flag5 || !flag4;
                }
                if (!flag4 && raw.FullHMSMatch(raw.NegativeLocalized))
                {
                    positive = false;
                    flag4 = TryTimeToTicks(positive, zero, raw.numbers[0], raw.numbers[1], raw.numbers[2], zero, out num);
                    flag5 = flag5 || !flag4;
                }
                if (!flag4 && raw.FullDHMMatch(raw.NegativeLocalized))
                {
                    positive = false;
                    flag4 = TryTimeToTicks(positive, raw.numbers[0], raw.numbers[1], raw.numbers[2], zero, zero, out num);
                    flag5 = flag5 || !flag4;
                }
                if (!flag4 && raw.PartialAppCompatMatch(raw.NegativeLocalized))
                {
                    positive = false;
                    flag4 = TryTimeToTicks(positive, zero, raw.numbers[0], raw.numbers[1], zero, raw.numbers[2], out num);
                    flag5 = flag5 || !flag4;
                }
            }
            if (flag4)
            {
                if (!positive)
                {
                    num = -num;
                    if (num > 0L)
                    {
                        result.SetFailure(ParseFailureKind.Overflow, "Overflow_TimeSpanElementTooLarge");
                        return false;
                    }
                }
                result.parsedTimeSpan._ticks = num;
                return true;
            }
            if (flag5)
            {
                result.SetFailure(ParseFailureKind.Overflow, "Overflow_TimeSpanElementTooLarge");
                return false;
            }
            result.SetFailure(ParseFailureKind.Format, "Format_BadTimeSpan");
            return false;
        }

        private static bool ProcessTerminal_HMS_F_D(ref TimeSpanRawInfo raw, TimeSpanStandardStyles style, ref TimeSpanResult result)
        {
            if (((raw.SepCount != 5) || (raw.NumCount != 4)) || ((style & TimeSpanStandardStyles.RequireFull) != TimeSpanStandardStyles.None))
            {
                result.SetFailure(ParseFailureKind.Format, "Format_BadTimeSpan");
                return false;
            }
            bool flag = (style & TimeSpanStandardStyles.Invariant) != TimeSpanStandardStyles.None;
            bool flag2 = (style & TimeSpanStandardStyles.Localized) != TimeSpanStandardStyles.None;
            long num = 0L;
            bool positive = false;
            bool flag4 = false;
            bool flag5 = false;
            if (flag)
            {
                if (raw.FullHMSFMatch(raw.PositiveInvariant))
                {
                    positive = true;
                    flag4 = TryTimeToTicks(positive, zero, raw.numbers[0], raw.numbers[1], raw.numbers[2], raw.numbers[3], out num);
                    flag5 = flag5 || !flag4;
                }
                if (!flag4 && raw.FullDHMSMatch(raw.PositiveInvariant))
                {
                    positive = true;
                    flag4 = TryTimeToTicks(positive, raw.numbers[0], raw.numbers[1], raw.numbers[2], raw.numbers[3], zero, out num);
                    flag5 = flag5 || !flag4;
                }
                if (!flag4 && raw.FullAppCompatMatch(raw.PositiveInvariant))
                {
                    positive = true;
                    flag4 = TryTimeToTicks(positive, raw.numbers[0], raw.numbers[1], raw.numbers[2], zero, raw.numbers[3], out num);
                    flag5 = flag5 || !flag4;
                }
                if (!flag4 && raw.FullHMSFMatch(raw.NegativeInvariant))
                {
                    positive = false;
                    flag4 = TryTimeToTicks(positive, zero, raw.numbers[0], raw.numbers[1], raw.numbers[2], raw.numbers[3], out num);
                    flag5 = flag5 || !flag4;
                }
                if (!flag4 && raw.FullDHMSMatch(raw.NegativeInvariant))
                {
                    positive = false;
                    flag4 = TryTimeToTicks(positive, raw.numbers[0], raw.numbers[1], raw.numbers[2], raw.numbers[3], zero, out num);
                    flag5 = flag5 || !flag4;
                }
                if (!flag4 && raw.FullAppCompatMatch(raw.NegativeInvariant))
                {
                    positive = false;
                    flag4 = TryTimeToTicks(positive, raw.numbers[0], raw.numbers[1], raw.numbers[2], zero, raw.numbers[3], out num);
                    flag5 = flag5 || !flag4;
                }
            }
            if (flag2)
            {
                if (!flag4 && raw.FullHMSFMatch(raw.PositiveLocalized))
                {
                    positive = true;
                    flag4 = TryTimeToTicks(positive, zero, raw.numbers[0], raw.numbers[1], raw.numbers[2], raw.numbers[3], out num);
                    flag5 = flag5 || !flag4;
                }
                if (!flag4 && raw.FullDHMSMatch(raw.PositiveLocalized))
                {
                    positive = true;
                    flag4 = TryTimeToTicks(positive, raw.numbers[0], raw.numbers[1], raw.numbers[2], raw.numbers[3], zero, out num);
                    flag5 = flag5 || !flag4;
                }
                if (!flag4 && raw.FullAppCompatMatch(raw.PositiveLocalized))
                {
                    positive = true;
                    flag4 = TryTimeToTicks(positive, raw.numbers[0], raw.numbers[1], raw.numbers[2], zero, raw.numbers[3], out num);
                    flag5 = flag5 || !flag4;
                }
                if (!flag4 && raw.FullHMSFMatch(raw.NegativeLocalized))
                {
                    positive = false;
                    flag4 = TryTimeToTicks(positive, zero, raw.numbers[0], raw.numbers[1], raw.numbers[2], raw.numbers[3], out num);
                    flag5 = flag5 || !flag4;
                }
                if (!flag4 && raw.FullDHMSMatch(raw.NegativeLocalized))
                {
                    positive = false;
                    flag4 = TryTimeToTicks(positive, raw.numbers[0], raw.numbers[1], raw.numbers[2], raw.numbers[3], zero, out num);
                    flag5 = flag5 || !flag4;
                }
                if (!flag4 && raw.FullAppCompatMatch(raw.NegativeLocalized))
                {
                    positive = false;
                    flag4 = TryTimeToTicks(positive, raw.numbers[0], raw.numbers[1], raw.numbers[2], zero, raw.numbers[3], out num);
                    flag5 = flag5 || !flag4;
                }
            }
            if (flag4)
            {
                if (!positive)
                {
                    num = -num;
                    if (num > 0L)
                    {
                        result.SetFailure(ParseFailureKind.Overflow, "Overflow_TimeSpanElementTooLarge");
                        return false;
                    }
                }
                result.parsedTimeSpan._ticks = num;
                return true;
            }
            if (flag5)
            {
                result.SetFailure(ParseFailureKind.Overflow, "Overflow_TimeSpanElementTooLarge");
                return false;
            }
            result.SetFailure(ParseFailureKind.Format, "Format_BadTimeSpan");
            return false;
        }

        private static bool ProcessTerminalState(ref TimeSpanRawInfo raw, TimeSpanStandardStyles style, ref TimeSpanResult result)
        {
            if (raw.lastSeenTTT == TTT.Num)
            {
                TimeSpanToken tok = new TimeSpanToken {
                    ttt = TTT.Sep,
                    sep = string.Empty
                };
                if (!raw.ProcessToken(ref tok, ref result))
                {
                    result.SetFailure(ParseFailureKind.Format, "Format_BadTimeSpan");
                    return false;
                }
            }
            switch (raw.NumCount)
            {
                case 1:
                    return ProcessTerminal_D(ref raw, style, ref result);

                case 2:
                    return ProcessTerminal_HM(ref raw, style, ref result);

                case 3:
                    return ProcessTerminal_HM_S_D(ref raw, style, ref result);

                case 4:
                    return ProcessTerminal_HMS_F_D(ref raw, style, ref result);

                case 5:
                    return ProcessTerminal_DHMSF(ref raw, style, ref result);
            }
            result.SetFailure(ParseFailureKind.Format, "Format_BadTimeSpan");
            return false;
        }

        internal static bool TryParse(string input, IFormatProvider formatProvider, out TimeSpan result)
        {
            TimeSpanResult result2 = new TimeSpanResult();
            result2.Init(TimeSpanThrowStyle.None);
            if (TryParseTimeSpan(input, TimeSpanStandardStyles.Any, formatProvider, ref result2))
            {
                result = result2.parsedTimeSpan;
                return true;
            }
            result = new TimeSpan();
            return false;
        }

        private static bool TryParseByFormat(string input, string format, TimeSpanStyles styles, ref TimeSpanResult result)
        {
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            bool flag4 = false;
            bool flag5 = false;
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            int zeroes = 0;
            int num6 = 0;
            int pos = 0;
            int returnValue = 0;
            TimeSpanTokenizer tokenizer = new TimeSpanTokenizer();
            tokenizer.Init(input, -1);
            while (pos < format.Length)
            {
                int num9;
                char failureMessageFormatArgument = format[pos];
                switch (failureMessageFormatArgument)
                {
                    case '%':
                        num9 = DateTimeFormat.ParseNextChar(format, pos);
                        if ((num9 < 0) || (num9 == 0x25))
                        {
                            goto Label_0280;
                        }
                        returnValue = 1;
                        goto Label_02CA;

                    case '\'':
                    case '"':
                    {
                        StringBuilder builder = new StringBuilder();
                        if (!DateTimeParse.TryParseQuoteString(format, pos, builder, out returnValue))
                        {
                            result.SetFailure(ParseFailureKind.FormatWithParameter, "Format_BadQuote", failureMessageFormatArgument);
                            return false;
                        }
                        if (ParseExactLiteral(ref tokenizer, builder))
                        {
                            goto Label_02CA;
                        }
                        result.SetFailure(ParseFailureKind.Format, "Format_InvalidString");
                        return false;
                    }
                    case 'F':
                        returnValue = DateTimeFormat.ParseRepeatPattern(format, pos, failureMessageFormatArgument);
                        if ((returnValue > 7) || flag5)
                        {
                            result.SetFailure(ParseFailureKind.Format, "Format_InvalidString");
                            return false;
                        }
                        ParseExactDigits(ref tokenizer, returnValue, returnValue, out zeroes, out num6);
                        flag5 = true;
                        goto Label_02CA;

                    case 'm':
                        returnValue = DateTimeFormat.ParseRepeatPattern(format, pos, failureMessageFormatArgument);
                        if (((returnValue > 2) || flag3) || !ParseExactDigits(ref tokenizer, returnValue, out num3))
                        {
                            result.SetFailure(ParseFailureKind.Format, "Format_InvalidString");
                            return false;
                        }
                        flag3 = true;
                        goto Label_02CA;

                    case 's':
                        returnValue = DateTimeFormat.ParseRepeatPattern(format, pos, failureMessageFormatArgument);
                        if (((returnValue > 2) || flag4) || !ParseExactDigits(ref tokenizer, returnValue, out num4))
                        {
                            result.SetFailure(ParseFailureKind.Format, "Format_InvalidString");
                            return false;
                        }
                        flag4 = true;
                        goto Label_02CA;

                    case 'd':
                    {
                        returnValue = DateTimeFormat.ParseRepeatPattern(format, pos, failureMessageFormatArgument);
                        int num10 = 0;
                        if (((returnValue > 8) || flag) || !ParseExactDigits(ref tokenizer, (returnValue < 2) ? 1 : returnValue, (returnValue < 2) ? 8 : returnValue, out num10, out num))
                        {
                            result.SetFailure(ParseFailureKind.Format, "Format_InvalidString");
                            return false;
                        }
                        flag = true;
                        goto Label_02CA;
                    }
                    case 'f':
                        returnValue = DateTimeFormat.ParseRepeatPattern(format, pos, failureMessageFormatArgument);
                        if (((returnValue <= 7) && !flag5) && ParseExactDigits(ref tokenizer, returnValue, returnValue, out zeroes, out num6))
                        {
                            goto Label_0193;
                        }
                        result.SetFailure(ParseFailureKind.Format, "Format_InvalidString");
                        return false;

                    case 'h':
                        returnValue = DateTimeFormat.ParseRepeatPattern(format, pos, failureMessageFormatArgument);
                        if (((returnValue <= 2) && !flag2) && ParseExactDigits(ref tokenizer, returnValue, out num2))
                        {
                            break;
                        }
                        result.SetFailure(ParseFailureKind.Format, "Format_InvalidString");
                        return false;

                    case '\\':
                        num9 = DateTimeFormat.ParseNextChar(format, pos);
                        if ((num9 >= 0) && (tokenizer.NextChar == ((char) num9)))
                        {
                            returnValue = 2;
                            goto Label_02CA;
                        }
                        result.SetFailure(ParseFailureKind.Format, "Format_InvalidString");
                        return false;

                    default:
                        result.SetFailure(ParseFailureKind.Format, "Format_InvalidString");
                        return false;
                }
                flag2 = true;
                goto Label_02CA;
            Label_0193:
                flag5 = true;
                goto Label_02CA;
            Label_0280:
                result.SetFailure(ParseFailureKind.Format, "Format_InvalidString");
                return false;
            Label_02CA:
                pos += returnValue;
            }
            if (!tokenizer.EOL)
            {
                result.SetFailure(ParseFailureKind.Format, "Format_BadTimeSpan");
                return false;
            }
            long num11 = 0L;
            bool positive = (styles & TimeSpanStyles.AssumeNegative) == TimeSpanStyles.None;
            if (TryTimeToTicks(positive, new TimeSpanToken(num), new TimeSpanToken(num2), new TimeSpanToken(num3), new TimeSpanToken(num4), new TimeSpanToken(zeroes, num6), out num11))
            {
                if (!positive)
                {
                    num11 = -num11;
                }
                result.parsedTimeSpan._ticks = num11;
                return true;
            }
            result.SetFailure(ParseFailureKind.Overflow, "Overflow_TimeSpanElementTooLarge");
            return false;
        }

        internal static bool TryParseExact(string input, string format, IFormatProvider formatProvider, TimeSpanStyles styles, out TimeSpan result)
        {
            TimeSpanResult result2 = new TimeSpanResult();
            result2.Init(TimeSpanThrowStyle.None);
            if (TryParseExactTimeSpan(input, format, formatProvider, styles, ref result2))
            {
                result = result2.parsedTimeSpan;
                return true;
            }
            result = new TimeSpan();
            return false;
        }

        internal static bool TryParseExactMultiple(string input, string[] formats, IFormatProvider formatProvider, TimeSpanStyles styles, out TimeSpan result)
        {
            TimeSpanResult result2 = new TimeSpanResult();
            result2.Init(TimeSpanThrowStyle.None);
            if (TryParseExactMultipleTimeSpan(input, formats, formatProvider, styles, ref result2))
            {
                result = result2.parsedTimeSpan;
                return true;
            }
            result = new TimeSpan();
            return false;
        }

        private static bool TryParseExactMultipleTimeSpan(string input, string[] formats, IFormatProvider formatProvider, TimeSpanStyles styles, ref TimeSpanResult result)
        {
            if (input == null)
            {
                result.SetFailure(ParseFailureKind.ArgumentNull, "ArgumentNull_String", null, "input");
                return false;
            }
            if (formats == null)
            {
                result.SetFailure(ParseFailureKind.ArgumentNull, "ArgumentNull_String", null, "formats");
                return false;
            }
            if (input.Length == 0)
            {
                result.SetFailure(ParseFailureKind.Format, "Format_BadTimeSpan");
                return false;
            }
            if (formats.Length == 0)
            {
                result.SetFailure(ParseFailureKind.Format, "Format_BadFormatSpecifier");
                return false;
            }
            for (int i = 0; i < formats.Length; i++)
            {
                if ((formats[i] == null) || (formats[i].Length == 0))
                {
                    result.SetFailure(ParseFailureKind.Format, "Format_BadFormatSpecifier");
                    return false;
                }
                TimeSpanResult result2 = new TimeSpanResult();
                result2.Init(TimeSpanThrowStyle.None);
                if (TryParseExactTimeSpan(input, formats[i], formatProvider, styles, ref result2))
                {
                    result.parsedTimeSpan = result2.parsedTimeSpan;
                    return true;
                }
            }
            result.SetFailure(ParseFailureKind.Format, "Format_BadTimeSpan");
            return false;
        }

        private static bool TryParseExactTimeSpan(string input, string format, IFormatProvider formatProvider, TimeSpanStyles styles, ref TimeSpanResult result)
        {
            if (input == null)
            {
                result.SetFailure(ParseFailureKind.ArgumentNull, "ArgumentNull_String", null, "input");
                return false;
            }
            if (format == null)
            {
                result.SetFailure(ParseFailureKind.ArgumentNull, "ArgumentNull_String", null, "format");
                return false;
            }
            if (format.Length == 0)
            {
                result.SetFailure(ParseFailureKind.Format, "Format_BadFormatSpecifier");
                return false;
            }
            if (format.Length != 1)
            {
                return TryParseByFormat(input, format, styles, ref result);
            }
            TimeSpanStandardStyles none = TimeSpanStandardStyles.None;
            if (((format[0] == 'c') || (format[0] == 't')) || (format[0] == 'T'))
            {
                return TryParseTimeSpanConstant(input, ref result);
            }
            if (format[0] == 'g')
            {
                none = TimeSpanStandardStyles.Localized;
            }
            else if (format[0] == 'G')
            {
                none = TimeSpanStandardStyles.RequireFull | TimeSpanStandardStyles.Localized;
            }
            else
            {
                result.SetFailure(ParseFailureKind.Format, "Format_BadFormatSpecifier");
                return false;
            }
            return TryParseTimeSpan(input, none, formatProvider, ref result);
        }

        private static bool TryParseTimeSpan(string input, TimeSpanStandardStyles style, IFormatProvider formatProvider, ref TimeSpanResult result)
        {
            if (input == null)
            {
                result.SetFailure(ParseFailureKind.ArgumentNull, "ArgumentNull_String", null, "input");
                return false;
            }
            input = input.Trim();
            if (input == string.Empty)
            {
                result.SetFailure(ParseFailureKind.Format, "Format_BadTimeSpan");
                return false;
            }
            TimeSpanTokenizer tokenizer = new TimeSpanTokenizer();
            tokenizer.Init(input);
            TimeSpanRawInfo raw = new TimeSpanRawInfo();
            raw.Init(DateTimeFormatInfo.GetInstance(formatProvider));
            for (TimeSpanToken token = tokenizer.GetNextToken(); token.ttt != TTT.End; token = tokenizer.GetNextToken())
            {
                if (!raw.ProcessToken(ref token, ref result))
                {
                    result.SetFailure(ParseFailureKind.Format, "Format_BadTimeSpan");
                    return false;
                }
            }
            if (!tokenizer.EOL)
            {
                result.SetFailure(ParseFailureKind.Format, "Format_BadTimeSpan");
                return false;
            }
            if (!ProcessTerminalState(ref raw, style, ref result))
            {
                result.SetFailure(ParseFailureKind.Format, "Format_BadTimeSpan");
                return false;
            }
            return true;
        }

        private static bool TryParseTimeSpanConstant(string input, ref TimeSpanResult result)
        {
            StringParser parser2 = new StringParser();
            return parser2.TryParse(input, ref result);
        }

        private static bool TryTimeToTicks(bool positive, TimeSpanToken days, TimeSpanToken hours, TimeSpanToken minutes, TimeSpanToken seconds, TimeSpanToken fraction, out long result)
        {
            if ((days.IsInvalidNumber(0xa2e3ff, -1) || hours.IsInvalidNumber(0x17, -1)) || ((minutes.IsInvalidNumber(0x3b, -1) || seconds.IsInvalidNumber(0x3b, -1)) || fraction.IsInvalidNumber(0x98967f, 7)))
            {
                result = 0L;
                return false;
            }
            long num = (((((days.num * 0xe10L) * 0x18L) + (hours.num * 0xe10L)) + (minutes.num * 60L)) + seconds.num) * 0x3e8L;
            if ((num > 0x346dc5d638865L) || (num < -922337203685477L))
            {
                result = 0L;
                return false;
            }
            long num2 = fraction.num;
            if (num2 != 0L)
            {
                long num3 = 0xf4240L;
                if (fraction.zeroes > 0)
                {
                    long num4 = (long) Math.Pow(10.0, (double) fraction.zeroes);
                    num3 /= num4;
                }
                while (num2 < num3)
                {
                    num2 *= 10L;
                }
            }
            result = (num * 0x2710L) + num2;
            if (positive && (result < 0L))
            {
                result = 0L;
                return false;
            }
            return true;
        }

        internal static void ValidateStyles(TimeSpanStyles style, string parameterName)
        {
            if ((style != TimeSpanStyles.None) && (style != TimeSpanStyles.AssumeNegative))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidTimeSpanStyles"), parameterName);
            }
        }

        private enum ParseFailureKind
        {
            None,
            ArgumentNull,
            Format,
            FormatWithParameter,
            Overflow
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct StringParser
        {
            private string str;
            private char ch;
            private int pos;
            private int len;
            internal void NextChar()
            {
                if (this.pos < this.len)
                {
                    this.pos++;
                }
                this.ch = (this.pos < this.len) ? this.str[this.pos] : '\0';
            }

            internal char NextNonDigit()
            {
                for (int i = this.pos; i < this.len; i++)
                {
                    char ch = this.str[i];
                    if ((ch < '0') || (ch > '9'))
                    {
                        return ch;
                    }
                }
                return '\0';
            }

            internal bool TryParse(string input, ref TimeSpanParse.TimeSpanResult result)
            {
                long num;
                result.parsedTimeSpan._ticks = 0L;
                if (input == null)
                {
                    result.SetFailure(TimeSpanParse.ParseFailureKind.ArgumentNull, "ArgumentNull_String", null, "input");
                    return false;
                }
                this.str = input;
                this.len = input.Length;
                this.pos = -1;
                this.NextChar();
                this.SkipBlanks();
                bool flag = false;
                if (this.ch == '-')
                {
                    flag = true;
                    this.NextChar();
                }
                if (this.NextNonDigit() == ':')
                {
                    if (!this.ParseTime(out num, ref result))
                    {
                        return false;
                    }
                }
                else
                {
                    int num2;
                    if (!this.ParseInt(0xa2e3ff, out num2, ref result))
                    {
                        return false;
                    }
                    num = num2 * 0xc92a69c000L;
                    if (this.ch == '.')
                    {
                        long num3;
                        this.NextChar();
                        if (!this.ParseTime(out num3, ref result))
                        {
                            return false;
                        }
                        num += num3;
                    }
                }
                if (flag)
                {
                    num = -num;
                    if (num > 0L)
                    {
                        result.SetFailure(TimeSpanParse.ParseFailureKind.Overflow, "Overflow_TimeSpanElementTooLarge");
                        return false;
                    }
                }
                else if (num < 0L)
                {
                    result.SetFailure(TimeSpanParse.ParseFailureKind.Overflow, "Overflow_TimeSpanElementTooLarge");
                    return false;
                }
                this.SkipBlanks();
                if (this.pos < this.len)
                {
                    result.SetFailure(TimeSpanParse.ParseFailureKind.Format, "Format_BadTimeSpan");
                    return false;
                }
                result.parsedTimeSpan._ticks = num;
                return true;
            }

            internal bool ParseInt(int max, out int i, ref TimeSpanParse.TimeSpanResult result)
            {
                i = 0;
                int pos = this.pos;
                while ((this.ch >= '0') && (this.ch <= '9'))
                {
                    if ((((long) i) & 0xf0000000L) != 0L)
                    {
                        result.SetFailure(TimeSpanParse.ParseFailureKind.Overflow, "Overflow_TimeSpanElementTooLarge");
                        return false;
                    }
                    i = ((i * 10) + this.ch) - 0x30;
                    if (i < 0)
                    {
                        result.SetFailure(TimeSpanParse.ParseFailureKind.Overflow, "Overflow_TimeSpanElementTooLarge");
                        return false;
                    }
                    this.NextChar();
                }
                if (pos == this.pos)
                {
                    result.SetFailure(TimeSpanParse.ParseFailureKind.Format, "Format_BadTimeSpan");
                    return false;
                }
                if (i > max)
                {
                    result.SetFailure(TimeSpanParse.ParseFailureKind.Overflow, "Overflow_TimeSpanElementTooLarge");
                    return false;
                }
                return true;
            }

            internal bool ParseTime(out long time, ref TimeSpanParse.TimeSpanResult result)
            {
                int num;
                time = 0L;
                if (!this.ParseInt(0x17, out num, ref result))
                {
                    return false;
                }
                time = num * 0x861c46800L;
                if (this.ch != ':')
                {
                    result.SetFailure(TimeSpanParse.ParseFailureKind.Format, "Format_BadTimeSpan");
                    return false;
                }
                this.NextChar();
                if (!this.ParseInt(0x3b, out num, ref result))
                {
                    return false;
                }
                time += num * 0x23c34600L;
                if (this.ch == ':')
                {
                    this.NextChar();
                    if (this.ch != '.')
                    {
                        if (!this.ParseInt(0x3b, out num, ref result))
                        {
                            return false;
                        }
                        time += num * 0x989680L;
                    }
                    if (this.ch == '.')
                    {
                        this.NextChar();
                        int num2 = 0x989680;
                        while (((num2 > 1) && (this.ch >= '0')) && (this.ch <= '9'))
                        {
                            num2 /= 10;
                            time += (this.ch - '0') * num2;
                            this.NextChar();
                        }
                    }
                }
                return true;
            }

            internal void SkipBlanks()
            {
                while ((this.ch == ' ') || (this.ch == '\t'))
                {
                    this.NextChar();
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TimeSpanRawInfo
        {
            private const int MaxTokens = 11;
            private const int MaxLiteralTokens = 6;
            private const int MaxNumericTokens = 5;
            internal TimeSpanParse.TTT lastSeenTTT;
            internal int tokenCount;
            internal int SepCount;
            internal int NumCount;
            internal string[] literals;
            internal TimeSpanParse.TimeSpanToken[] numbers;
            private TimeSpanFormat.FormatLiterals m_posLoc;
            private TimeSpanFormat.FormatLiterals m_negLoc;
            private bool m_posLocInit;
            private bool m_negLocInit;
            private string m_fullPosPattern;
            private string m_fullNegPattern;
            internal TimeSpanFormat.FormatLiterals PositiveInvariant
            {
                get
                {
                    return TimeSpanFormat.PositiveInvariantFormatLiterals;
                }
            }
            internal TimeSpanFormat.FormatLiterals NegativeInvariant
            {
                get
                {
                    return TimeSpanFormat.NegativeInvariantFormatLiterals;
                }
            }
            internal TimeSpanFormat.FormatLiterals PositiveLocalized
            {
                get
                {
                    if (!this.m_posLocInit)
                    {
                        this.m_posLoc = new TimeSpanFormat.FormatLiterals();
                        this.m_posLoc.Init(this.m_fullPosPattern, false);
                        this.m_posLocInit = true;
                    }
                    return this.m_posLoc;
                }
            }
            internal TimeSpanFormat.FormatLiterals NegativeLocalized
            {
                get
                {
                    if (!this.m_negLocInit)
                    {
                        this.m_negLoc = new TimeSpanFormat.FormatLiterals();
                        this.m_negLoc.Init(this.m_fullNegPattern, false);
                        this.m_negLocInit = true;
                    }
                    return this.m_negLoc;
                }
            }
            internal bool FullAppCompatMatch(TimeSpanFormat.FormatLiterals pattern)
            {
                return (((((this.SepCount == 5) && (this.NumCount == 4)) && ((pattern.Start == this.literals[0]) && (pattern.DayHourSep == this.literals[1]))) && ((pattern.HourMinuteSep == this.literals[2]) && (pattern.AppCompatLiteral == this.literals[3]))) && (pattern.End == this.literals[4]));
            }

            internal bool PartialAppCompatMatch(TimeSpanFormat.FormatLiterals pattern)
            {
                return (((((this.SepCount == 4) && (this.NumCount == 3)) && ((pattern.Start == this.literals[0]) && (pattern.HourMinuteSep == this.literals[1]))) && (pattern.AppCompatLiteral == this.literals[2])) && (pattern.End == this.literals[3]));
            }

            internal bool FullMatch(TimeSpanFormat.FormatLiterals pattern)
            {
                return (((((this.SepCount == 6) && (this.NumCount == 5)) && ((pattern.Start == this.literals[0]) && (pattern.DayHourSep == this.literals[1]))) && (((pattern.HourMinuteSep == this.literals[2]) && (pattern.MinuteSecondSep == this.literals[3])) && (pattern.SecondFractionSep == this.literals[4]))) && (pattern.End == this.literals[5]));
            }

            internal bool FullDMatch(TimeSpanFormat.FormatLiterals pattern)
            {
                return ((((this.SepCount == 2) && (this.NumCount == 1)) && (pattern.Start == this.literals[0])) && (pattern.End == this.literals[1]));
            }

            internal bool FullHMMatch(TimeSpanFormat.FormatLiterals pattern)
            {
                return ((((this.SepCount == 3) && (this.NumCount == 2)) && ((pattern.Start == this.literals[0]) && (pattern.HourMinuteSep == this.literals[1]))) && (pattern.End == this.literals[2]));
            }

            internal bool FullDHMMatch(TimeSpanFormat.FormatLiterals pattern)
            {
                return (((((this.SepCount == 4) && (this.NumCount == 3)) && ((pattern.Start == this.literals[0]) && (pattern.DayHourSep == this.literals[1]))) && (pattern.HourMinuteSep == this.literals[2])) && (pattern.End == this.literals[3]));
            }

            internal bool FullHMSMatch(TimeSpanFormat.FormatLiterals pattern)
            {
                return (((((this.SepCount == 4) && (this.NumCount == 3)) && ((pattern.Start == this.literals[0]) && (pattern.HourMinuteSep == this.literals[1]))) && (pattern.MinuteSecondSep == this.literals[2])) && (pattern.End == this.literals[3]));
            }

            internal bool FullDHMSMatch(TimeSpanFormat.FormatLiterals pattern)
            {
                return (((((this.SepCount == 5) && (this.NumCount == 4)) && ((pattern.Start == this.literals[0]) && (pattern.DayHourSep == this.literals[1]))) && ((pattern.HourMinuteSep == this.literals[2]) && (pattern.MinuteSecondSep == this.literals[3]))) && (pattern.End == this.literals[4]));
            }

            internal bool FullHMSFMatch(TimeSpanFormat.FormatLiterals pattern)
            {
                return (((((this.SepCount == 5) && (this.NumCount == 4)) && ((pattern.Start == this.literals[0]) && (pattern.HourMinuteSep == this.literals[1]))) && ((pattern.MinuteSecondSep == this.literals[2]) && (pattern.SecondFractionSep == this.literals[3]))) && (pattern.End == this.literals[4]));
            }

            internal void Init(DateTimeFormatInfo dtfi)
            {
                this.lastSeenTTT = TimeSpanParse.TTT.None;
                this.tokenCount = 0;
                this.SepCount = 0;
                this.NumCount = 0;
                this.literals = new string[6];
                this.numbers = new TimeSpanParse.TimeSpanToken[5];
                this.m_fullPosPattern = dtfi.FullTimeSpanPositivePattern;
                this.m_fullNegPattern = dtfi.FullTimeSpanNegativePattern;
                this.m_posLocInit = false;
                this.m_negLocInit = false;
            }

            internal bool ProcessToken(ref TimeSpanParse.TimeSpanToken tok, ref TimeSpanParse.TimeSpanResult result)
            {
                if (tok.ttt == TimeSpanParse.TTT.NumOverflow)
                {
                    result.SetFailure(TimeSpanParse.ParseFailureKind.Overflow, "Overflow_TimeSpanElementTooLarge", null);
                    return false;
                }
                if ((tok.ttt != TimeSpanParse.TTT.Sep) && (tok.ttt != TimeSpanParse.TTT.Num))
                {
                    result.SetFailure(TimeSpanParse.ParseFailureKind.Format, "Format_BadTimeSpan", null);
                    return false;
                }
                switch (tok.ttt)
                {
                    case TimeSpanParse.TTT.Num:
                        if ((this.tokenCount != 0) || this.AddSep(string.Empty, ref result))
                        {
                            if (!this.AddNum(tok, ref result))
                            {
                                return false;
                            }
                            break;
                        }
                        return false;

                    case TimeSpanParse.TTT.Sep:
                        if (this.AddSep(tok.sep, ref result))
                        {
                            break;
                        }
                        return false;
                }
                this.lastSeenTTT = tok.ttt;
                return true;
            }

            private bool AddSep(string sep, ref TimeSpanParse.TimeSpanResult result)
            {
                if ((this.SepCount >= 6) || (this.tokenCount >= 11))
                {
                    result.SetFailure(TimeSpanParse.ParseFailureKind.Format, "Format_BadTimeSpan", null);
                    return false;
                }
                this.literals[this.SepCount++] = sep;
                this.tokenCount++;
                return true;
            }

            private bool AddNum(TimeSpanParse.TimeSpanToken num, ref TimeSpanParse.TimeSpanResult result)
            {
                if ((this.NumCount >= 5) || (this.tokenCount >= 11))
                {
                    result.SetFailure(TimeSpanParse.ParseFailureKind.Format, "Format_BadTimeSpan", null);
                    return false;
                }
                this.numbers[this.NumCount++] = num;
                this.tokenCount++;
                return true;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TimeSpanResult
        {
            internal TimeSpan parsedTimeSpan;
            internal TimeSpanParse.TimeSpanThrowStyle throwStyle;
            internal TimeSpanParse.ParseFailureKind m_failure;
            internal string m_failureMessageID;
            internal object m_failureMessageFormatArgument;
            internal string m_failureArgumentName;
            internal void Init(TimeSpanParse.TimeSpanThrowStyle canThrow)
            {
                this.parsedTimeSpan = new TimeSpan();
                this.throwStyle = canThrow;
            }

            internal void SetFailure(TimeSpanParse.ParseFailureKind failure, string failureMessageID)
            {
                this.SetFailure(failure, failureMessageID, null, null);
            }

            internal void SetFailure(TimeSpanParse.ParseFailureKind failure, string failureMessageID, object failureMessageFormatArgument)
            {
                this.SetFailure(failure, failureMessageID, failureMessageFormatArgument, null);
            }

            internal void SetFailure(TimeSpanParse.ParseFailureKind failure, string failureMessageID, object failureMessageFormatArgument, string failureArgumentName)
            {
                this.m_failure = failure;
                this.m_failureMessageID = failureMessageID;
                this.m_failureMessageFormatArgument = failureMessageFormatArgument;
                this.m_failureArgumentName = failureArgumentName;
                if (this.throwStyle != TimeSpanParse.TimeSpanThrowStyle.None)
                {
                    throw this.GetTimeSpanParseException();
                }
            }

            internal Exception GetTimeSpanParseException()
            {
                switch (this.m_failure)
                {
                    case TimeSpanParse.ParseFailureKind.ArgumentNull:
                        return new ArgumentNullException(this.m_failureArgumentName, Environment.GetResourceString(this.m_failureMessageID));

                    case TimeSpanParse.ParseFailureKind.Format:
                        return new FormatException(Environment.GetResourceString(this.m_failureMessageID));

                    case TimeSpanParse.ParseFailureKind.FormatWithParameter:
                        return new FormatException(Environment.GetResourceString(this.m_failureMessageID, new object[] { this.m_failureMessageFormatArgument }));

                    case TimeSpanParse.ParseFailureKind.Overflow:
                        return new OverflowException(Environment.GetResourceString(this.m_failureMessageID));
                }
                return new FormatException(Environment.GetResourceString("Format_InvalidString"));
            }
        }

        [Flags]
        private enum TimeSpanStandardStyles
        {
            None,
            Invariant,
            Localized,
            Any,
            RequireFull
        }

        private enum TimeSpanThrowStyle
        {
            None,
            All
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TimeSpanToken
        {
            internal TimeSpanParse.TTT ttt;
            internal int num;
            internal int zeroes;
            internal string sep;
            public TimeSpanToken(int number)
            {
                this.ttt = TimeSpanParse.TTT.Num;
                this.num = number;
                this.zeroes = 0;
                this.sep = null;
            }

            public TimeSpanToken(int leadingZeroes, int number)
            {
                this.ttt = TimeSpanParse.TTT.Num;
                this.num = number;
                this.zeroes = leadingZeroes;
                this.sep = null;
            }

            public bool IsInvalidNumber(int maxValue, int maxPrecision)
            {
                if (this.num > maxValue)
                {
                    return true;
                }
                if (maxPrecision == -1)
                {
                    return false;
                }
                return ((this.zeroes > maxPrecision) || (((this.num != 0) && (this.zeroes != 0)) && (this.num >= (((long) maxValue) / ((long) Math.Pow(10.0, (double) (this.zeroes - 1)))))));
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TimeSpanTokenizer
        {
            private int m_pos;
            private string m_value;
            internal void Init(string input)
            {
                this.Init(input, 0);
            }

            internal void Init(string input, int startPosition)
            {
                this.m_pos = startPosition;
                this.m_value = input;
            }

            internal TimeSpanParse.TimeSpanToken GetNextToken()
            {
                TimeSpanParse.TimeSpanToken token = new TimeSpanParse.TimeSpanToken();
                char currentChar = this.CurrentChar;
                if (currentChar == '\0')
                {
                    token.ttt = TimeSpanParse.TTT.End;
                    return token;
                }
                if ((currentChar >= '0') && (currentChar <= '9'))
                {
                    token.ttt = TimeSpanParse.TTT.Num;
                    token.num = 0;
                    token.zeroes = 0;
                    do
                    {
                        if ((token.num & 0xf0000000L) != 0L)
                        {
                            token.ttt = TimeSpanParse.TTT.NumOverflow;
                            return token;
                        }
                        token.num = ((token.num * 10) + currentChar) - 0x30;
                        if (token.num == 0)
                        {
                            token.zeroes++;
                        }
                        if (token.num < 0)
                        {
                            token.ttt = TimeSpanParse.TTT.NumOverflow;
                            return token;
                        }
                        currentChar = this.NextChar;
                    }
                    while ((currentChar >= '0') && (currentChar <= '9'));
                    return token;
                }
                token.ttt = TimeSpanParse.TTT.Sep;
                int pos = this.m_pos;
                int length = 0;
                while ((currentChar != '\0') && ((currentChar < '0') || ('9' < currentChar)))
                {
                    currentChar = this.NextChar;
                    length++;
                }
                token.sep = this.m_value.Substring(pos, length);
                return token;
            }

            internal bool EOL
            {
                get
                {
                    return (this.m_pos >= (this.m_value.Length - 1));
                }
            }
            internal void BackOne()
            {
                if (this.m_pos > 0)
                {
                    this.m_pos--;
                }
            }

            internal char NextChar
            {
                get
                {
                    this.m_pos++;
                    return this.CurrentChar;
                }
            }
            internal char CurrentChar
            {
                get
                {
                    if ((this.m_pos > -1) && (this.m_pos < this.m_value.Length))
                    {
                        return this.m_value[this.m_pos];
                    }
                    return '\0';
                }
            }
        }

        private enum TTT
        {
            None,
            End,
            Num,
            Sep,
            NumOverflow
        }
    }
}

