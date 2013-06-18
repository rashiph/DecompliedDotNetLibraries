namespace Microsoft.VisualBasic.CompilerServices
{
    using Microsoft.VisualBasic;
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime;
    using System.Text;
    using System.Threading;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class StringType
    {
        private const string GENERAL_FORMAT = "G";

        private StringType()
        {
        }

        private static int AsteriskSkip(string Pattern, string Source, int SourceEndIndex, CompareMethod CompareOption, CompareInfo ci)
        {
            int num2;
            int num4;
            int num3 = Strings.Len(Pattern);
            while (num4 < num3)
            {
                bool flag;
                bool flag2;
                bool flag3;
                switch (Pattern[num4])
                {
                    case '-':
                        if (Pattern[num4 + 1] == ']')
                        {
                            flag2 = true;
                        }
                        break;

                    case '!':
                        if (Pattern[num4 + 1] == ']')
                        {
                            flag2 = true;
                        }
                        else
                        {
                            flag3 = true;
                        }
                        break;

                    case '[':
                        if (flag)
                        {
                            flag2 = true;
                        }
                        else
                        {
                            flag = true;
                        }
                        break;

                    case ']':
                        if (flag2 || !flag)
                        {
                            num2++;
                            flag3 = true;
                        }
                        flag2 = false;
                        flag = false;
                        break;

                    case '*':
                        if (num2 > 0)
                        {
                            CompareOptions ordinal;
                            if (flag3)
                            {
                                num2 = MultipleAsteriskSkip(Pattern, Source, num2, CompareOption);
                                return (SourceEndIndex - num2);
                            }
                            string str = Pattern.Substring(0, num4);
                            if (CompareOption == CompareMethod.Binary)
                            {
                                ordinal = CompareOptions.Ordinal;
                            }
                            else
                            {
                                ordinal = CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase;
                            }
                            return ci.LastIndexOf(Source, str, ordinal);
                        }
                        break;

                    case '?':
                    case '#':
                        if (flag)
                        {
                            flag2 = true;
                        }
                        else
                        {
                            num2++;
                            flag3 = true;
                        }
                        break;

                    default:
                        if (flag)
                        {
                            flag2 = true;
                        }
                        else
                        {
                            num2++;
                        }
                        break;
                }
                num4++;
            }
            return (SourceEndIndex - num2);
        }

        public static string FromBoolean(bool Value)
        {
            if (Value)
            {
                return bool.TrueString;
            }
            return bool.FalseString;
        }

        public static string FromByte(byte Value)
        {
            return Value.ToString(null, null);
        }

        public static string FromChar(char Value)
        {
            return Value.ToString();
        }

        public static string FromDate(DateTime Value)
        {
            long ticks = Value.TimeOfDay.Ticks;
            if ((ticks == Value.Ticks) || (((Value.Year == 0x76b) && (Value.Month == 12)) && (Value.Day == 30)))
            {
                return Value.ToString("T", null);
            }
            if (ticks == 0L)
            {
                return Value.ToString("d", null);
            }
            return Value.ToString("G", null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static string FromDecimal(decimal Value)
        {
            return FromDecimal(Value, null);
        }

        public static string FromDecimal(decimal Value, NumberFormatInfo NumberFormat)
        {
            return Value.ToString("G", NumberFormat);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static string FromDouble(double Value)
        {
            return FromDouble(Value, null);
        }

        public static string FromDouble(double Value, NumberFormatInfo NumberFormat)
        {
            return Value.ToString("G", NumberFormat);
        }

        public static string FromInteger(int Value)
        {
            return Value.ToString(null, null);
        }

        public static string FromLong(long Value)
        {
            return Value.ToString(null, null);
        }

        public static string FromObject(object Value)
        {
            if (Value == null)
            {
                return null;
            }
            string str2 = Value as string;
            if (str2 != null)
            {
                return str2;
            }
            IConvertible convertible = Value as IConvertible;
            if (convertible != null)
            {
                switch (convertible.GetTypeCode())
                {
                    case TypeCode.Boolean:
                        return FromBoolean(convertible.ToBoolean(null));

                    case TypeCode.Char:
                        return FromChar(convertible.ToChar(null));

                    case TypeCode.Byte:
                        return FromByte(convertible.ToByte(null));

                    case TypeCode.Int16:
                        return FromShort(convertible.ToInt16(null));

                    case TypeCode.Int32:
                        return FromInteger(convertible.ToInt32(null));

                    case TypeCode.Int64:
                        return FromLong(convertible.ToInt64(null));

                    case TypeCode.Single:
                        return FromSingle(convertible.ToSingle(null));

                    case TypeCode.Double:
                        return FromDouble(convertible.ToDouble(null));

                    case TypeCode.Decimal:
                        return FromDecimal(convertible.ToDecimal(null));

                    case TypeCode.DateTime:
                        return FromDate(convertible.ToDateTime(null));

                    case TypeCode.String:
                        return convertible.ToString(null);
                }
            }
            else
            {
                char[] chArray = Value as char[];
                if ((chArray != null) && (chArray.Rank == 1))
                {
                    return new string(CharArrayType.FromObject(Value));
                }
            }
            throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(Value), "String" }));
        }

        public static string FromShort(short Value)
        {
            return Value.ToString(null, (IFormatProvider) null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static string FromSingle(float Value)
        {
            return FromSingle(Value, null);
        }

        public static string FromSingle(float Value, NumberFormatInfo NumberFormat)
        {
            return Value.ToString(null, NumberFormat);
        }

        public static void MidStmtStr(ref string sDest, int StartPosition, int MaxInsertLength, string sInsert)
        {
            int length;
            int num3;
            if (sDest != null)
            {
                length = sDest.Length;
            }
            if (sInsert != null)
            {
                num3 = sInsert.Length;
            }
            StartPosition--;
            if ((StartPosition < 0) || (StartPosition >= length))
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Start" }));
            }
            if (MaxInsertLength < 0)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Length" }));
            }
            if (num3 > MaxInsertLength)
            {
                num3 = MaxInsertLength;
            }
            if (num3 > (length - StartPosition))
            {
                num3 = length - StartPosition;
            }
            if (num3 != 0)
            {
                StringBuilder builder = new StringBuilder(length);
                if (StartPosition > 0)
                {
                    builder.Append(sDest, 0, StartPosition);
                }
                builder.Append(sInsert, 0, num3);
                int count = length - (StartPosition + num3);
                if (count > 0)
                {
                    builder.Append(sDest, StartPosition + num3, count);
                }
                sDest = builder.ToString();
            }
        }

        private static int MultipleAsteriskSkip(string Pattern, string Source, int Count, CompareMethod CompareOption)
        {
            int num2 = Strings.Len(Source);
            while (Count < num2)
            {
                bool flag;
                string source = Source.Substring(num2 - Count);
                try
                {
                    flag = StrLike(source, Pattern, CompareOption);
                }
                catch (StackOverflowException exception)
                {
                    throw exception;
                }
                catch (OutOfMemoryException exception2)
                {
                    throw exception2;
                }
                catch (ThreadAbortException exception3)
                {
                    throw exception3;
                }
                catch (Exception)
                {
                    flag = false;
                }
                if (flag)
                {
                    return Count;
                }
                Count++;
            }
            return Count;
        }

        public static int StrCmp(string sLeft, string sRight, bool TextCompare)
        {
            if (sLeft == sRight)
            {
                return 0;
            }
            if (sLeft == null)
            {
                if (sRight.Length == 0)
                {
                    return 0;
                }
                return -1;
            }
            if (sRight == null)
            {
                if (sLeft.Length == 0)
                {
                    return 0;
                }
                return 1;
            }
            if (TextCompare)
            {
                return Utils.GetCultureInfo().CompareInfo.Compare(sLeft, sRight, CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreCase);
            }
            return string.CompareOrdinal(sLeft, sRight);
        }

        public static bool StrLike(string Source, string Pattern, CompareMethod CompareOption)
        {
            if (CompareOption == CompareMethod.Binary)
            {
                return StrLikeBinary(Source, Pattern);
            }
            return StrLikeText(Source, Pattern);
        }

        public static bool StrLikeBinary(string Source, string Pattern)
        {
            bool flag;
            int length;
            int num2;
            char ch3;
            int num4;
            int num5;
            bool flag2 = false;
            if (Pattern == null)
            {
                length = 0;
            }
            else
            {
                length = Pattern.Length;
            }
            if (Source == null)
            {
                num4 = 0;
            }
            else
            {
                num4 = Source.Length;
            }
            if (num5 < num4)
            {
                ch3 = Source[num5];
            }
            while (num2 < length)
            {
                char p = Pattern[num2];
                if ((p == '*') && !flag)
                {
                    int num3 = AsteriskSkip(Pattern.Substring(num2 + 1), Source.Substring(num5), num4 - num5, CompareMethod.Binary, Strings.m_InvariantCompareInfo);
                    if (num3 < 0)
                    {
                        return false;
                    }
                    if (num3 > 0)
                    {
                        num5 += num3;
                        if (num5 < num4)
                        {
                            ch3 = Source[num5];
                        }
                    }
                }
                else if ((p == '?') && !flag)
                {
                    num5++;
                    if (num5 < num4)
                    {
                        ch3 = Source[num5];
                    }
                }
                else if ((p == '#') && !flag)
                {
                    if (!char.IsDigit(ch3))
                    {
                        break;
                    }
                    num5++;
                    if (num5 < num4)
                    {
                        ch3 = Source[num5];
                    }
                }
                else
                {
                    bool flag4;
                    bool flag5;
                    if ((((p == '-') && flag) && (flag5 && !flag2)) && (!flag4 && (((num2 + 1) >= length) || (Pattern[num2 + 1] != ']'))))
                    {
                        flag4 = true;
                    }
                    else
                    {
                        bool flag3;
                        bool flag6;
                        if (((p == '!') && flag) && !flag6)
                        {
                            flag6 = true;
                            flag3 = true;
                        }
                        else
                        {
                            char ch;
                            char ch4;
                            if ((p == '[') && !flag)
                            {
                                flag = true;
                                ch4 = '\0';
                                ch = '\0';
                                flag5 = false;
                            }
                            else if ((p == ']') && flag)
                            {
                                flag = false;
                                if (flag5)
                                {
                                    if (!flag3)
                                    {
                                        break;
                                    }
                                    num5++;
                                    if (num5 < num4)
                                    {
                                        ch3 = Source[num5];
                                    }
                                }
                                else if (flag4)
                                {
                                    if (!flag3)
                                    {
                                        break;
                                    }
                                }
                                else if (flag6)
                                {
                                    if ('!' != ch3)
                                    {
                                        break;
                                    }
                                    num5++;
                                    if (num5 < num4)
                                    {
                                        ch3 = Source[num5];
                                    }
                                }
                                flag3 = false;
                                flag5 = false;
                                flag6 = false;
                                flag4 = false;
                            }
                            else
                            {
                                flag5 = true;
                                flag2 = false;
                                if (flag)
                                {
                                    if (flag4)
                                    {
                                        flag4 = false;
                                        flag2 = true;
                                        ch = p;
                                        if (ch4 > ch)
                                        {
                                            throw ExceptionUtils.VbMakeException(0x5d);
                                        }
                                        if ((flag6 && flag3) || (!flag6 && !flag3))
                                        {
                                            flag3 = (ch3 > ch4) && (ch3 <= ch);
                                            if (flag6)
                                            {
                                                flag3 = !flag3;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ch4 = p;
                                        flag3 = StrLikeCompareBinary(flag6, flag3, p, ch3);
                                    }
                                }
                                else
                                {
                                    if ((p != ch3) && !flag6)
                                    {
                                        break;
                                    }
                                    flag6 = false;
                                    num5++;
                                    if (num5 < num4)
                                    {
                                        ch3 = Source[num5];
                                    }
                                    else if (num5 > num4)
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
                num2++;
            }
            if (flag)
            {
                if (num4 != 0)
                {
                    throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Pattern" }));
                }
                return false;
            }
            return ((num2 == length) && (num5 == num4));
        }

        private static bool StrLikeCompare(CompareInfo ci, bool SeenNot, bool Match, char p, char s, CompareOptions Options)
        {
            if (SeenNot && Match)
            {
                if (Options == CompareOptions.Ordinal)
                {
                    return (p != s);
                }
                return (ci.Compare(Conversions.ToString(p), Conversions.ToString(s), Options) != 0);
            }
            if (SeenNot || Match)
            {
                return Match;
            }
            if (Options == CompareOptions.Ordinal)
            {
                return (p == s);
            }
            return (ci.Compare(Conversions.ToString(p), Conversions.ToString(s), Options) == 0);
        }

        private static bool StrLikeCompareBinary(bool SeenNot, bool Match, char p, char s)
        {
            if (SeenNot && Match)
            {
                return (p != s);
            }
            if (!SeenNot && !Match)
            {
                return (p == s);
            }
            return Match;
        }

        public static bool StrLikeText(string Source, string Pattern)
        {
            bool flag;
            int length;
            int num2;
            char ch3;
            int num4;
            int num5;
            bool flag2 = false;
            if (Pattern == null)
            {
                length = 0;
            }
            else
            {
                length = Pattern.Length;
            }
            if (Source == null)
            {
                num4 = 0;
            }
            else
            {
                num4 = Source.Length;
            }
            if (num5 < num4)
            {
                ch3 = Source[num5];
            }
            CompareInfo compareInfo = Utils.GetCultureInfo().CompareInfo;
            CompareOptions options = CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase;
            while (num2 < length)
            {
                char p = Pattern[num2];
                if ((p == '*') && !flag)
                {
                    int num3 = AsteriskSkip(Pattern.Substring(num2 + 1), Source.Substring(num5), num4 - num5, CompareMethod.Text, compareInfo);
                    if (num3 < 0)
                    {
                        return false;
                    }
                    if (num3 > 0)
                    {
                        num5 += num3;
                        if (num5 < num4)
                        {
                            ch3 = Source[num5];
                        }
                    }
                }
                else if ((p == '?') && !flag)
                {
                    num5++;
                    if (num5 < num4)
                    {
                        ch3 = Source[num5];
                    }
                }
                else if ((p == '#') && !flag)
                {
                    if (!char.IsDigit(ch3))
                    {
                        break;
                    }
                    num5++;
                    if (num5 < num4)
                    {
                        ch3 = Source[num5];
                    }
                }
                else
                {
                    bool flag4;
                    bool flag5;
                    if ((((p == '-') && flag) && (flag5 && !flag2)) && (!flag4 && (((num2 + 1) >= length) || (Pattern[num2 + 1] != ']'))))
                    {
                        flag4 = true;
                    }
                    else
                    {
                        bool flag3;
                        bool flag6;
                        if (((p == '!') && flag) && !flag6)
                        {
                            flag6 = true;
                            flag3 = true;
                        }
                        else
                        {
                            char ch;
                            char ch4;
                            if ((p == '[') && !flag)
                            {
                                flag = true;
                                ch4 = '\0';
                                ch = '\0';
                                flag5 = false;
                            }
                            else if ((p == ']') && flag)
                            {
                                flag = false;
                                if (flag5)
                                {
                                    if (!flag3)
                                    {
                                        break;
                                    }
                                    num5++;
                                    if (num5 < num4)
                                    {
                                        ch3 = Source[num5];
                                    }
                                }
                                else if (flag4)
                                {
                                    if (!flag3)
                                    {
                                        break;
                                    }
                                }
                                else if (flag6)
                                {
                                    if (compareInfo.Compare("!", Conversions.ToString(ch3)) != 0)
                                    {
                                        break;
                                    }
                                    num5++;
                                    if (num5 < num4)
                                    {
                                        ch3 = Source[num5];
                                    }
                                }
                                flag3 = false;
                                flag5 = false;
                                flag6 = false;
                                flag4 = false;
                            }
                            else
                            {
                                flag5 = true;
                                flag2 = false;
                                if (flag)
                                {
                                    if (flag4)
                                    {
                                        flag4 = false;
                                        flag2 = true;
                                        ch = p;
                                        if (ch4 > ch)
                                        {
                                            throw ExceptionUtils.VbMakeException(0x5d);
                                        }
                                        if ((flag6 && flag3) || (!flag6 && !flag3))
                                        {
                                            if (options == CompareOptions.Ordinal)
                                            {
                                                flag3 = (ch3 > ch4) && (ch3 <= ch);
                                            }
                                            else
                                            {
                                                flag3 = (compareInfo.Compare(Conversions.ToString(ch4), Conversions.ToString(ch3), options) < 0) && (compareInfo.Compare(Conversions.ToString(ch), Conversions.ToString(ch3), options) >= 0);
                                            }
                                            if (flag6)
                                            {
                                                flag3 = !flag3;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ch4 = p;
                                        flag3 = StrLikeCompare(compareInfo, flag6, flag3, p, ch3, options);
                                    }
                                }
                                else
                                {
                                    if (options == CompareOptions.Ordinal)
                                    {
                                        if ((p != ch3) && !flag6)
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        string str = Conversions.ToString(p);
                                        string str2 = Conversions.ToString(ch3);
                                        while (((num2 + 1) < length) && ((UnicodeCategory.ModifierSymbol == char.GetUnicodeCategory(Pattern[num2 + 1])) || (UnicodeCategory.NonSpacingMark == char.GetUnicodeCategory(Pattern[num2 + 1]))))
                                        {
                                            str = str + Conversions.ToString(Pattern[num2 + 1]);
                                            num2++;
                                        }
                                        while (((num5 + 1) < num4) && ((UnicodeCategory.ModifierSymbol == char.GetUnicodeCategory(Source[num5 + 1])) || (UnicodeCategory.NonSpacingMark == char.GetUnicodeCategory(Source[num5 + 1]))))
                                        {
                                            str2 = str2 + Conversions.ToString(Source[num5 + 1]);
                                            num5++;
                                        }
                                        if ((compareInfo.Compare(str, str2, CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreCase) != 0) && !flag6)
                                        {
                                            break;
                                        }
                                    }
                                    flag6 = false;
                                    num5++;
                                    if (num5 < num4)
                                    {
                                        ch3 = Source[num5];
                                    }
                                    else if (num5 > num4)
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
                num2++;
            }
            if (flag)
            {
                if (num4 != 0)
                {
                    throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Pattern" }));
                }
                return false;
            }
            return ((num2 == length) && (num5 == num4));
        }
    }
}

