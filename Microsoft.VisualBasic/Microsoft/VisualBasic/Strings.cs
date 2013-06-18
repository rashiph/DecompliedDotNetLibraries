namespace Microsoft.VisualBasic
{
    using Microsoft.VisualBasic.CompilerServices;
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    [StandardModule]
    public sealed class Strings
    {
        private const int CODEPAGE_SIMPLIFIED_CHINESE = 0x3a8;
        private const int CODEPAGE_TRADITIONAL_CHINESE = 950;
        private static readonly string[] CurrencyNegativeFormatStrings = new string[] { "('$'n)", "-'$'n", "'$'-n", "'$'n-", "(n'$')", "-n'$'", "n-'$'", "n'$'-", "-n '$'", "-'$' n", "n '$'-", "'$' n-", "'$'- n", "n- '$'", "('$' n)", "(n '$')" };
        private static readonly string[] CurrencyPositiveFormatStrings = new string[] { "'$'n", "n'$'", "'$' n", "n '$'" };
        private const int InvariantCultureID = 0x7f;
        private static string m_CachedOnOffFormatStyle;
        private static string m_CachedTrueFalseFormatStyle;
        private static string m_CachedYesNoFormatStyle;
        internal static readonly CompareInfo m_InvariantCompareInfo = CultureInfo.InvariantCulture.CompareInfo;
        private static CultureInfo m_LastUsedOnOffCulture;
        private static CultureInfo m_LastUsedTrueFalseCulture;
        private static CultureInfo m_LastUsedYesNoCulture;
        private static object m_SyncObject = new object();
        private const string NAMEDFORMAT_CURRENCY = "currency";
        private const string NAMEDFORMAT_FIXED = "fixed";
        private const string NAMEDFORMAT_GENERAL_DATE = "general date";
        private const string NAMEDFORMAT_GENERAL_NUMBER = "general number";
        private const string NAMEDFORMAT_LONG_DATE = "long date";
        private const string NAMEDFORMAT_LONG_TIME = "long time";
        private const string NAMEDFORMAT_MEDIUM_DATE = "medium date";
        private const string NAMEDFORMAT_MEDIUM_TIME = "medium time";
        private const string NAMEDFORMAT_ON_OFF = "on/off";
        private const string NAMEDFORMAT_PERCENT = "percent";
        private const string NAMEDFORMAT_SCIENTIFIC = "scientific";
        private const string NAMEDFORMAT_SHORT_DATE = "short date";
        private const string NAMEDFORMAT_SHORT_TIME = "short time";
        private const string NAMEDFORMAT_STANDARD = "standard";
        private const string NAMEDFORMAT_TRUE_FALSE = "true/false";
        private const string NAMEDFORMAT_YES_NO = "yes/no";
        private static readonly string[] NumberNegativeFormatStrings = new string[] { "(n)", "-n", "- n", "n-", "n -" };
        private const CompareOptions STANDARD_COMPARE_FLAGS = (CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreCase);

        public static int Asc(char String)
        {
            int num;
            int num2 = Convert.ToInt32(String);
            if (num2 < 0x80)
            {
                return num2;
            }
            try
            {
                byte[] buffer;
                Encoding fileIOEncoding = Utils.GetFileIOEncoding();
                char[] chars = new char[] { String };
                if (fileIOEncoding.IsSingleByte)
                {
                    buffer = new byte[1];
                    int num3 = fileIOEncoding.GetBytes(chars, 0, 1, buffer, 0);
                    return buffer[0];
                }
                buffer = new byte[2];
                if (fileIOEncoding.GetBytes(chars, 0, 1, buffer, 0) == 1)
                {
                    return buffer[0];
                }
                if (BitConverter.IsLittleEndian)
                {
                    byte num4 = buffer[0];
                    buffer[0] = buffer[1];
                    buffer[1] = num4;
                }
                num = BitConverter.ToInt16(buffer, 0);
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return num;
        }

        public static int Asc(string String)
        {
            if ((String == null) || (String.Length == 0))
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_LengthGTZero1", new string[] { "String" }));
            }
            char ch = String[0];
            return Asc(ch);
        }

        public static int AscW(char String)
        {
            return String;
        }

        public static int AscW(string String)
        {
            if ((String == null) || (String.Length == 0))
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_LengthGTZero1", new string[] { "String" }));
            }
            return String[0];
        }

        public static char Chr(int CharCode)
        {
            char ch;
            if ((CharCode < -32768) || (CharCode > 0xffff))
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_RangeTwoBytes1", new string[] { "CharCode" }));
            }
            if ((CharCode >= 0) && (CharCode <= 0x7f))
            {
                return Convert.ToChar(CharCode);
            }
            try
            {
                int num;
                Encoding encoding = Encoding.GetEncoding(Utils.GetLocaleCodePage());
                if (encoding.IsSingleByte && ((CharCode < 0) || (CharCode > 0xff)))
                {
                    throw ExceptionUtils.VbMakeException(5);
                }
                char[] chars = new char[2];
                byte[] bytes = new byte[2];
                System.Text.Decoder decoder = encoding.GetDecoder();
                if ((CharCode >= 0) && (CharCode <= 0xff))
                {
                    bytes[0] = (byte) (CharCode & 0xff);
                    num = decoder.GetChars(bytes, 0, 1, chars, 0);
                }
                else
                {
                    bytes[0] = (byte) ((CharCode & 0xff00) >> 8);
                    bytes[1] = (byte) (CharCode & 0xff);
                    num = decoder.GetChars(bytes, 0, 2, chars, 0);
                }
                ch = chars[0];
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return ch;
        }

        public static char ChrW(int CharCode)
        {
            if ((CharCode < -32768) || (CharCode > 0xffff))
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_RangeTwoBytes1", new string[] { "CharCode" }));
            }
            return Convert.ToChar((int) (CharCode & 0xffff));
        }

        public static string[] Filter(object[] Source, string Match, bool Include = true, [OptionCompare] CompareMethod Compare = 0)
        {
            int num = Information.UBound(Source, 1);
            string[] source = new string[num + 1];
            try
            {
                int num3 = num;
                for (int i = 0; i <= num3; i++)
                {
                    source[i] = Conversions.ToString(Source[i]);
                }
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
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValueType2", new string[] { "Source", "String" }));
            }
            return Filter(source, Match, Include, Compare);
        }

        public static string[] Filter(string[] Source, string Match, bool Include = true, [OptionCompare] CompareMethod Compare = 0)
        {
            string[] strArray;
            try
            {
                CompareOptions ignoreCase;
                int num2;
                if (Source.Rank != 1)
                {
                    throw new ArgumentException(Utils.GetResourceString("Argument_RankEQOne1"));
                }
                if ((Match == null) || (Match.Length == 0))
                {
                    return null;
                }
                int length = Source.Length;
                CompareInfo compareInfo = Utils.GetCultureInfo().CompareInfo;
                if (Compare == CompareMethod.Text)
                {
                    ignoreCase = CompareOptions.IgnoreCase;
                }
                string[] strArray2 = new string[(length - 1) + 1];
                int num4 = length - 1;
                for (int i = 0; i <= num4; i++)
                {
                    string source = Source[i];
                    if ((source != null) && ((compareInfo.IndexOf(source, Match, ignoreCase) >= 0) == Include))
                    {
                        strArray2[num2] = source;
                        num2++;
                    }
                }
                if (num2 == 0)
                {
                    return new string[0];
                }
                if (num2 == strArray2.Length)
                {
                    return strArray2;
                }
                strArray2 = (string[]) Utils.CopyArray((Array) strArray2, new string[(num2 - 1) + 1]);
                strArray = strArray2;
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return strArray;
        }

        public static string Format(object Expression, string Style = "")
        {
            string str;
            try
            {
                double num;
                float num2;
                IFormatProvider formatProvider = null;
                IFormattable formattable = null;
                if ((Expression == null) || (Expression.GetType() == null))
                {
                    return "";
                }
                if ((Style == null) || (Style.Length == 0))
                {
                    return Conversions.ToString(Expression);
                }
                IConvertible convertible = (IConvertible) Expression;
                TypeCode typeCode = convertible.GetTypeCode();
                if (Style.Length > 0)
                {
                    try
                    {
                        string returnValue = null;
                        if (FormatNamed(Expression, Style, ref returnValue))
                        {
                            return returnValue;
                        }
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
                        return Conversions.ToString(Expression);
                    }
                }
                formattable = Expression as IFormattable;
                if (formattable == null)
                {
                    typeCode = Convert.GetTypeCode(Expression);
                    if ((typeCode != TypeCode.String) && (typeCode != TypeCode.Boolean))
                    {
                        throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Expression" }));
                    }
                }
                switch (typeCode)
                {
                    case TypeCode.Empty:
                        return "";

                    case TypeCode.Object:
                    case TypeCode.Char:
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Decimal:
                    case TypeCode.DateTime:
                        return formattable.ToString(Style, formatProvider);

                    case TypeCode.DBNull:
                        return "";

                    case TypeCode.Boolean:
                        return string.Format(formatProvider, Style, new object[] { Conversions.ToString(convertible.ToBoolean(null)) });

                    case TypeCode.Single:
                        num2 = convertible.ToSingle(null);
                        if ((Style != null) && (Style.Length != 0))
                        {
                            goto Label_01C3;
                        }
                        return Conversions.ToString(num2);

                    case TypeCode.Double:
                        num = convertible.ToDouble(null);
                        if ((Style != null) && (Style.Length != 0))
                        {
                            break;
                        }
                        return Conversions.ToString(num);

                    case TypeCode.String:
                        return string.Format(formatProvider, Style, new object[] { Expression });

                    default:
                        goto Label_01F8;
                }
                if (num == 0.0)
                {
                    num = 0.0;
                }
                return num.ToString(Style, formatProvider);
            Label_01C3:
                if (num2 == 0f)
                {
                    num2 = 0f;
                }
                return num2.ToString(Style, formatProvider);
            Label_01F8:
                str = formattable.ToString(Style, formatProvider);
            }
            catch (Exception exception4)
            {
                throw exception4;
            }
            return str;
        }

        public static string FormatCurrency(object Expression, int NumDigitsAfterDecimal = -1, TriState IncludeLeadingDigit = -2, TriState UseParensForNegativeNumbers = -2, TriState GroupDigits = -2)
        {
            string str;
            IFormatProvider formatProvider = null;
            try
            {
                ValidateTriState(IncludeLeadingDigit);
                ValidateTriState(UseParensForNegativeNumbers);
                ValidateTriState(GroupDigits);
                if (NumDigitsAfterDecimal > 0x63)
                {
                    throw new ArgumentException(Utils.GetResourceString("Argument_Range0to99_1", new string[] { "NumDigitsAfterDecimal" }));
                }
                if (Expression == null)
                {
                    return "";
                }
                Type type = Expression.GetType();
                if (type == typeof(string))
                {
                    Expression = Conversions.ToDouble(Expression);
                }
                else if (!Symbols.IsNumericType(type))
                {
                    throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(type), "Currency" }));
                }
                IFormattable formattable = (IFormattable) Expression;
                if (IncludeLeadingDigit == TriState.False)
                {
                    double num = Conversions.ToDouble(Expression);
                    if ((num >= 1.0) || (num <= -1.0))
                    {
                        IncludeLeadingDigit = TriState.True;
                    }
                }
                string format = GetCurrencyFormatString(IncludeLeadingDigit, NumDigitsAfterDecimal, UseParensForNegativeNumbers, GroupDigits, ref formatProvider);
                str = formattable.ToString(format, formatProvider);
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return str;
        }

        public static string FormatDateTime(DateTime Expression, DateFormat NamedFormat = 0)
        {
            string str;
            try
            {
                string str2;
                switch (NamedFormat)
                {
                    case DateFormat.GeneralDate:
                        if (Expression.TimeOfDay.Ticks != Expression.Ticks)
                        {
                            break;
                        }
                        str2 = "T";
                        goto Label_0088;

                    case DateFormat.LongDate:
                        str2 = "D";
                        goto Label_0088;

                    case DateFormat.ShortDate:
                        str2 = "d";
                        goto Label_0088;

                    case DateFormat.LongTime:
                        str2 = "T";
                        goto Label_0088;

                    case DateFormat.ShortTime:
                        str2 = "HH:mm";
                        goto Label_0088;

                    default:
                        throw ExceptionUtils.VbMakeException(5);
                }
                if (Expression.TimeOfDay.Ticks == 0L)
                {
                    str2 = "d";
                }
                else
                {
                    str2 = "G";
                }
            Label_0088:
                str = Expression.ToString(str2, null);
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return str;
        }

        private static bool FormatNamed(object Expression, string Style, ref string ReturnValue)
        {
            int length = Style.Length;
            ReturnValue = null;
            switch (length)
            {
                case 5:
                {
                    char ch = Style[0];
                    if (((ch != 'f') && (ch != 'F')) || (string.Compare(Style, "fixed", StringComparison.OrdinalIgnoreCase) != 0))
                    {
                        break;
                    }
                    ReturnValue = Conversions.ToDouble(Expression).ToString("0.00", null);
                    return true;
                }
                case 6:
                {
                    char ch2 = Style[0];
                    if ((ch2 != 'y') && (ch2 != 'Y'))
                    {
                        if (((ch2 != 'o') && (ch2 != 'O')) || (string.Compare(Style, "on/off", StringComparison.OrdinalIgnoreCase) != 0))
                        {
                            break;
                        }
                        ReturnValue = ((int) -(Conversions.ToBoolean(Expression) > false)).ToString(CachedOnOffFormatStyle, null);
                        return true;
                    }
                    if (string.Compare(Style, "yes/no", StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        break;
                    }
                    ReturnValue = ((int) -(Conversions.ToBoolean(Expression) > false)).ToString(CachedYesNoFormatStyle, null);
                    return true;
                }
                case 7:
                {
                    char ch3 = Style[0];
                    if (((ch3 != 'p') && (ch3 != 'P')) || (string.Compare(Style, "percent", StringComparison.OrdinalIgnoreCase) != 0))
                    {
                        break;
                    }
                    ReturnValue = Conversions.ToDouble(Expression).ToString("0.00%", null);
                    return true;
                }
                case 8:
                {
                    char ch4 = Style[0];
                    if ((ch4 != 's') && (ch4 != 'S'))
                    {
                        if (((ch4 != 'c') && (ch4 != 'C')) || (string.Compare(Style, "currency", StringComparison.OrdinalIgnoreCase) != 0))
                        {
                            break;
                        }
                        ReturnValue = Conversions.ToDouble(Expression).ToString("C", null);
                        return true;
                    }
                    if (string.Compare(Style, "standard", StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        break;
                    }
                    ReturnValue = Conversions.ToDouble(Expression).ToString("N2", null);
                    return true;
                }
                case 9:
                {
                    char ch5 = Style[5];
                    if ((ch5 != 't') && (ch5 != 'T'))
                    {
                        if (((ch5 != 'd') && (ch5 != 'D')) || (string.Compare(Style, "long date", StringComparison.OrdinalIgnoreCase) != 0))
                        {
                            break;
                        }
                        ReturnValue = Conversions.ToDate(Expression).ToString("D", null);
                        return true;
                    }
                    if (string.Compare(Style, "long time", StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        break;
                    }
                    ReturnValue = Conversions.ToDate(Expression).ToString("T", null);
                    return true;
                }
                case 10:
                    switch (Style[6])
                    {
                        case 'A':
                        case 'a':
                            if (string.Compare(Style, "true/false", StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                break;
                            }
                            ReturnValue = ((int) -(Conversions.ToBoolean(Expression) > false)).ToString(CachedTrueFalseFormatStyle, null);
                            return true;

                        case 'D':
                        case 'd':
                            if (string.Compare(Style, "short date", StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                break;
                            }
                            ReturnValue = Conversions.ToDate(Expression).ToString("d", null);
                            return true;

                        case 'I':
                        case 'i':
                        {
                            if (string.Compare(Style, "scientific", StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                break;
                            }
                            double d = Conversions.ToDouble(Expression);
                            if (double.IsNaN(d) || double.IsInfinity(d))
                            {
                                ReturnValue = d.ToString("G", null);
                            }
                            else
                            {
                                ReturnValue = d.ToString("0.00E+00", null);
                            }
                            return true;
                        }
                        case 'T':
                        case 't':
                            if (string.Compare(Style, "short time", StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                break;
                            }
                            ReturnValue = Conversions.ToDate(Expression).ToString("t", null);
                            return true;
                    }
                    break;

                case 11:
                {
                    char ch7 = Style[7];
                    if ((ch7 != 't') && (ch7 != 'T'))
                    {
                        if (((ch7 == 'd') || (ch7 == 'D')) && (string.Compare(Style, "medium date", StringComparison.OrdinalIgnoreCase) == 0))
                        {
                            ReturnValue = Conversions.ToDate(Expression).ToString("D", null);
                            return true;
                        }
                        break;
                    }
                    if (string.Compare(Style, "medium time", StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        break;
                    }
                    ReturnValue = Conversions.ToDate(Expression).ToString("T", null);
                    return true;
                }
                case 12:
                {
                    char ch8 = Style[0];
                    if (((ch8 != 'g') && (ch8 != 'G')) || (string.Compare(Style, "general date", StringComparison.OrdinalIgnoreCase) != 0))
                    {
                        break;
                    }
                    ReturnValue = Conversions.ToDate(Expression).ToString("G", null);
                    return true;
                }
                case 14:
                {
                    char ch9 = Style[0];
                    if (((ch9 == 'g') || (ch9 == 'G')) && (string.Compare(Style, "general number", StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        ReturnValue = Conversions.ToDouble(Expression).ToString("G", null);
                        return true;
                    }
                    break;
                }
            }
            return false;
        }

        public static string FormatNumber(object Expression, int NumDigitsAfterDecimal = -1, TriState IncludeLeadingDigit = -2, TriState UseParensForNegativeNumbers = -2, TriState GroupDigits = -2)
        {
            string str;
            try
            {
                ValidateTriState(IncludeLeadingDigit);
                ValidateTriState(UseParensForNegativeNumbers);
                ValidateTriState(GroupDigits);
                if (Expression == null)
                {
                    return "";
                }
                Type type = Expression.GetType();
                if (type == typeof(string))
                {
                    Expression = Conversions.ToDouble(Expression);
                }
                else if (type == typeof(bool))
                {
                    if (Conversions.ToBoolean(Expression))
                    {
                        Expression = -1.0;
                    }
                    else
                    {
                        Expression = 0.0;
                    }
                }
                else if (!Symbols.IsNumericType(type))
                {
                    throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(type), "Currency" }));
                }
                str = ((IFormattable) Expression).ToString(GetNumberFormatString(NumDigitsAfterDecimal, IncludeLeadingDigit, UseParensForNegativeNumbers, GroupDigits), null);
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return str;
        }

        public static string FormatPercent(object Expression, int NumDigitsAfterDecimal = -1, TriState IncludeLeadingDigit = -2, TriState UseParensForNegativeNumbers = -2, TriState GroupDigits = -2)
        {
            ValidateTriState(IncludeLeadingDigit);
            ValidateTriState(UseParensForNegativeNumbers);
            ValidateTriState(GroupDigits);
            if (Expression == null)
            {
                return "";
            }
            Type type = Expression.GetType();
            if (type == typeof(string))
            {
                Expression = Conversions.ToDouble(Expression);
            }
            else if (!Symbols.IsNumericType(type))
            {
                throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(type), "numeric" }));
            }
            IFormattable formattable = (IFormattable) Expression;
            string format = GetFormatString(NumDigitsAfterDecimal, IncludeLeadingDigit, UseParensForNegativeNumbers, GroupDigits, FormatType.Percent);
            return formattable.ToString(format, null);
        }

        public static char GetChar(string str, int Index)
        {
            if (str == null)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_LengthGTZero1", new string[] { "String" }));
            }
            if (Index < 1)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_GEOne1", new string[] { "Index" }));
            }
            if (Index > str.Length)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_IndexLELength2", new string[] { "Index", "String" }));
            }
            return str[Index - 1];
        }

        internal static string GetCurrencyFormatString(TriState IncludeLeadingDigit, int NumDigitsAfterDecimal, TriState UseParensForNegativeNumbers, TriState GroupDigits, ref IFormatProvider formatProvider)
        {
            string str3;
            string str2 = "C";
            NumberFormatInfo format = (NumberFormatInfo) Utils.GetCultureInfo().GetFormat(typeof(NumberFormatInfo));
            format = (NumberFormatInfo) format.Clone();
            if (GroupDigits == TriState.False)
            {
                format.CurrencyGroupSizes = new int[] { 0 };
            }
            int currencyPositivePattern = format.CurrencyPositivePattern;
            int currencyNegativePattern = format.CurrencyNegativePattern;
            if (UseParensForNegativeNumbers != TriState.UseDefault)
            {
                if (UseParensForNegativeNumbers == TriState.False)
                {
                    switch (currencyNegativePattern)
                    {
                        case 0:
                            currencyNegativePattern = 1;
                            break;

                        case 4:
                            currencyNegativePattern = 5;
                            break;

                        case 14:
                            currencyNegativePattern = 9;
                            break;

                        case 15:
                            currencyNegativePattern = 10;
                            break;
                    }
                }
                else
                {
                    UseParensForNegativeNumbers = TriState.True;
                    switch (currencyNegativePattern)
                    {
                        case 1:
                        case 2:
                        case 3:
                            currencyNegativePattern = 0;
                            break;

                        case 5:
                        case 6:
                        case 7:
                            currencyNegativePattern = 4;
                            break;

                        case 8:
                        case 10:
                        case 13:
                            currencyNegativePattern = 15;
                            break;

                        case 9:
                        case 11:
                        case 12:
                            currencyNegativePattern = 14;
                            break;
                    }
                }
            }
            else
            {
                switch (currencyNegativePattern)
                {
                    case 0:
                    case 4:
                    case 14:
                    case 15:
                        UseParensForNegativeNumbers = TriState.True;
                        goto Label_0168;
                }
                UseParensForNegativeNumbers = TriState.False;
            }
        Label_0168:
            format.CurrencyNegativePattern = currencyNegativePattern;
            if (NumDigitsAfterDecimal == -1)
            {
                NumDigitsAfterDecimal = format.CurrencyDecimalDigits;
            }
            format.CurrencyDecimalDigits = NumDigitsAfterDecimal;
            formatProvider = new FormatInfoHolder(format);
            if (IncludeLeadingDigit != TriState.False)
            {
                return str2;
            }
            format.NumberGroupSizes = format.CurrencyGroupSizes;
            string str = CurrencyPositiveFormatStrings[currencyPositivePattern] + ";" + CurrencyNegativeFormatStrings[currencyNegativePattern];
            if (GroupDigits == TriState.False)
            {
                if (IncludeLeadingDigit == TriState.False)
                {
                    str3 = "#";
                }
                else
                {
                    str3 = "0";
                }
            }
            else if (IncludeLeadingDigit == TriState.False)
            {
                str3 = "#,###";
            }
            else
            {
                str3 = "#,##0";
            }
            if (NumDigitsAfterDecimal > 0)
            {
                str3 = str3 + "." + new string('0', NumDigitsAfterDecimal);
            }
            if (string.CompareOrdinal("$", format.CurrencySymbol) != 0)
            {
                str = str.Replace("$", format.CurrencySymbol.Replace("'", "''"));
            }
            return str.Replace("n", str3);
        }

        internal static string GetFormatString(int NumDigitsAfterDecimal, TriState IncludeLeadingDigit, TriState UseParensForNegativeNumbers, TriState GroupDigits, FormatType FormatTypeValue)
        {
            string str2;
            string str3;
            string str4;
            StringBuilder builder = new StringBuilder(30);
            NumberFormatInfo format = (NumberFormatInfo) Utils.GetCultureInfo().GetFormat(typeof(NumberFormatInfo));
            if (NumDigitsAfterDecimal < -1)
            {
                throw ExceptionUtils.VbMakeException(5);
            }
            if (NumDigitsAfterDecimal == -1)
            {
                if (FormatTypeValue == FormatType.Percent)
                {
                    NumDigitsAfterDecimal = format.NumberDecimalDigits;
                }
                else if (FormatTypeValue == FormatType.Number)
                {
                    NumDigitsAfterDecimal = format.NumberDecimalDigits;
                }
                else if (FormatTypeValue == FormatType.Currency)
                {
                    NumDigitsAfterDecimal = format.CurrencyDecimalDigits;
                }
            }
            if (GroupDigits == TriState.UseDefault)
            {
                GroupDigits = TriState.True;
                if (FormatTypeValue == FormatType.Percent)
                {
                    if (IsArrayEmpty(format.PercentGroupSizes))
                    {
                        GroupDigits = TriState.False;
                    }
                }
                else if (FormatTypeValue == FormatType.Number)
                {
                    if (IsArrayEmpty(format.NumberGroupSizes))
                    {
                        GroupDigits = TriState.False;
                    }
                }
                else if ((FormatTypeValue == FormatType.Currency) && IsArrayEmpty(format.CurrencyGroupSizes))
                {
                    GroupDigits = TriState.False;
                }
            }
            if (UseParensForNegativeNumbers == TriState.UseDefault)
            {
                UseParensForNegativeNumbers = TriState.False;
                if (FormatTypeValue == FormatType.Number)
                {
                    if (format.NumberNegativePattern == 0)
                    {
                        UseParensForNegativeNumbers = TriState.True;
                    }
                }
                else if ((FormatTypeValue == FormatType.Currency) && (format.CurrencyNegativePattern == 0))
                {
                    UseParensForNegativeNumbers = TriState.True;
                }
            }
            if (GroupDigits == TriState.True)
            {
                str3 = "#,##";
            }
            else
            {
                str3 = "";
            }
            if (IncludeLeadingDigit != TriState.False)
            {
                str4 = "0";
            }
            else
            {
                str4 = "#";
            }
            if (NumDigitsAfterDecimal > 0)
            {
                str2 = "." + new string('0', NumDigitsAfterDecimal);
            }
            else
            {
                str2 = "";
            }
            if (FormatTypeValue == FormatType.Currency)
            {
                builder.Append(format.CurrencySymbol);
            }
            builder.Append(str3);
            builder.Append(str4);
            builder.Append(str2);
            if (FormatTypeValue == FormatType.Percent)
            {
                builder.Append(format.PercentSymbol);
            }
            if (UseParensForNegativeNumbers == TriState.True)
            {
                string str5 = builder.ToString();
                builder.Append(";(");
                builder.Append(str5);
                builder.Append(")");
            }
            return builder.ToString();
        }

        internal static string GetNumberFormatString(int NumDigitsAfterDecimal, TriState IncludeLeadingDigit, TriState UseParensForNegativeNumbers, TriState GroupDigits)
        {
            string str3;
            NumberFormatInfo format = (NumberFormatInfo) Utils.GetCultureInfo().GetFormat(typeof(NumberFormatInfo));
            if (NumDigitsAfterDecimal == -1)
            {
                NumDigitsAfterDecimal = format.NumberDecimalDigits;
            }
            else if ((NumDigitsAfterDecimal > 0x63) || (NumDigitsAfterDecimal < -1))
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_Range0to99_1", new string[] { "NumDigitsAfterDecimal" }));
            }
            if (GroupDigits == TriState.UseDefault)
            {
                if ((format.NumberGroupSizes == null) || (format.NumberGroupSizes.Length == 0))
                {
                    GroupDigits = TriState.False;
                }
                else
                {
                    GroupDigits = TriState.True;
                }
            }
            int numberNegativePattern = format.NumberNegativePattern;
            if (UseParensForNegativeNumbers == TriState.UseDefault)
            {
                if (numberNegativePattern == 0)
                {
                    UseParensForNegativeNumbers = TriState.True;
                }
                else
                {
                    UseParensForNegativeNumbers = TriState.False;
                }
            }
            else if (UseParensForNegativeNumbers == TriState.False)
            {
                if (numberNegativePattern == 0)
                {
                    numberNegativePattern = 1;
                }
            }
            else
            {
                UseParensForNegativeNumbers = TriState.True;
                switch (numberNegativePattern)
                {
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                        numberNegativePattern = 0;
                        break;
                }
            }
            if (UseParensForNegativeNumbers == TriState.UseDefault)
            {
                UseParensForNegativeNumbers = TriState.True;
            }
            string expression = "n;" + NumberNegativeFormatStrings[numberNegativePattern];
            if (string.CompareOrdinal("-", format.NegativeSign) != 0)
            {
                expression = expression.Replace("-", "\"" + format.NegativeSign + "\"");
            }
            if (IncludeLeadingDigit != TriState.False)
            {
                str3 = "0";
            }
            else
            {
                str3 = "#";
            }
            if ((GroupDigits != TriState.False) && (format.NumberGroupSizes.Length != 0))
            {
                if (format.NumberGroupSizes.Length == 1)
                {
                    str3 = "#," + new string('#', format.NumberGroupSizes[0]) + str3;
                }
                else
                {
                    str3 = new string('#', format.NumberGroupSizes[0] - 1) + str3;
                    int upperBound = format.NumberGroupSizes.GetUpperBound(0);
                    for (int i = 1; i <= upperBound; i++)
                    {
                        str3 = "," + new string('#', format.NumberGroupSizes[i]) + "," + str3;
                    }
                }
            }
            if (NumDigitsAfterDecimal > 0)
            {
                str3 = str3 + "." + new string('0', NumDigitsAfterDecimal);
            }
            return Replace(expression, "n", str3, 1, -1, CompareMethod.Binary);
        }

        public static int InStr(string String1, string String2, [OptionCompare] CompareMethod Compare = 0)
        {
            if (Compare == CompareMethod.Binary)
            {
                return (InternalInStrBinary(0, String1, String2) + 1);
            }
            return (InternalInStrText(0, String1, String2) + 1);
        }

        public static int InStr(int Start, string String1, string String2, [OptionCompare] CompareMethod Compare = 0)
        {
            if (Start < 1)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_GTZero1", new string[] { "Start" }));
            }
            if (Compare == CompareMethod.Binary)
            {
                return (InternalInStrBinary(Start - 1, String1, String2) + 1);
            }
            return (InternalInStrText(Start - 1, String1, String2) + 1);
        }

        public static int InStrRev(string StringCheck, string StringMatch, int Start = -1, [OptionCompare] CompareMethod Compare = 0)
        {
            int num;
            try
            {
                int length;
                if ((Start == 0) || (Start < -1))
                {
                    throw new ArgumentException(Utils.GetResourceString("Argument_MinusOneOrGTZero1", new string[] { "Start" }));
                }
                if (StringCheck == null)
                {
                    length = 0;
                }
                else
                {
                    length = StringCheck.Length;
                }
                if (Start == -1)
                {
                    Start = length;
                }
                if ((Start > length) || (length == 0))
                {
                    return 0;
                }
                if ((StringMatch == null) || (StringMatch.Length == 0))
                {
                    return Start;
                }
                if (Compare == CompareMethod.Binary)
                {
                    return (m_InvariantCompareInfo.LastIndexOf(StringCheck, StringMatch, Start - 1, Start, CompareOptions.Ordinal) + 1);
                }
                num = Utils.GetCultureInfo().CompareInfo.LastIndexOf(StringCheck, StringMatch, Start - 1, Start, CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreCase) + 1;
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return num;
        }

        private static int InternalInStrBinary(int StartPos, string sSrc, string sFind)
        {
            int length;
            if (sSrc != null)
            {
                length = sSrc.Length;
            }
            else
            {
                length = 0;
            }
            if ((StartPos > length) || (length == 0))
            {
                return -1;
            }
            if ((sFind != null) && (sFind.Length != 0))
            {
                return m_InvariantCompareInfo.IndexOf(sSrc, sFind, StartPos, CompareOptions.Ordinal);
            }
            return StartPos;
        }

        private static int InternalInStrText(int lStartPos, string sSrc, string sFind)
        {
            int length;
            if (sSrc != null)
            {
                length = sSrc.Length;
            }
            else
            {
                length = 0;
            }
            if ((lStartPos > length) || (length == 0))
            {
                return -1;
            }
            if ((sFind != null) && (sFind.Length != 0))
            {
                return Utils.GetCultureInfo().CompareInfo.IndexOf(sSrc, sFind, lStartPos, CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreCase);
            }
            return lStartPos;
        }

        private static string InternalStrReverse(string Expression, int SrcIndex, int Length)
        {
            StringBuilder builder = new StringBuilder(Length) {
                Length = Length
            };
            TextElementEnumerator textElementEnumerator = StringInfo.GetTextElementEnumerator(Expression, SrcIndex);
            if (!textElementEnumerator.MoveNext())
            {
                return "";
            }
            int num2 = 0;
            int num = Length - 1;
            while (num2 < SrcIndex)
            {
                builder[num] = Expression[num2];
                num--;
                num2++;
            }
            int elementIndex = textElementEnumerator.ElementIndex;
            while (num >= 0)
            {
                SrcIndex = elementIndex;
                if (textElementEnumerator.MoveNext())
                {
                    elementIndex = textElementEnumerator.ElementIndex;
                }
                else
                {
                    elementIndex = Length;
                }
                for (num2 = elementIndex - 1; num2 >= SrcIndex; num2--)
                {
                    builder[num] = Expression[num2];
                    num--;
                }
            }
            return builder.ToString();
        }

        private static bool IsArrayEmpty(Array array)
        {
            return ((array == null) || (array.Length == 0));
        }

        internal static bool IsValidCodePage(int codepage)
        {
            bool flag = false;
            try
            {
                if (Encoding.GetEncoding(codepage) != null)
                {
                    flag = true;
                }
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
            }
            return flag;
        }

        public static string Join(object[] SourceArray, string Delimiter = " ")
        {
            int num2 = Information.UBound(SourceArray, 1);
            string[] sourceArray = new string[num2 + 1];
            try
            {
                int num3 = num2;
                for (int i = 0; i <= num3; i++)
                {
                    sourceArray[i] = Conversions.ToString(SourceArray[i]);
                }
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
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValueType2", new string[] { "SourceArray", "String" }));
            }
            return Join(sourceArray, Delimiter);
        }

        public static string Join(string[] SourceArray, string Delimiter = " ")
        {
            string str;
            try
            {
                if (IsArrayEmpty(SourceArray))
                {
                    return null;
                }
                if (SourceArray.Rank != 1)
                {
                    throw new ArgumentException(Utils.GetResourceString("Argument_RankEQOne1"));
                }
                str = string.Join(Delimiter, SourceArray);
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return str;
        }

        public static char LCase(char Value)
        {
            char ch;
            try
            {
                ch = Thread.CurrentThread.CurrentCulture.TextInfo.ToLower(Value);
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return ch;
        }

        public static string LCase(string Value)
        {
            string str;
            try
            {
                if (Value == null)
                {
                    return null;
                }
                str = Thread.CurrentThread.CurrentCulture.TextInfo.ToLower(Value);
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return str;
        }

        public static string Left(string str, int Length)
        {
            if (Length < 0)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_GEZero1", new string[] { "Length" }));
            }
            if ((Length == 0) || (str == null))
            {
                return "";
            }
            if (Length >= str.Length)
            {
                return str;
            }
            return str.Substring(0, Length);
        }

        public static int Len(bool Expression)
        {
            return 2;
        }

        public static int Len(byte Expression)
        {
            return 1;
        }

        public static int Len(char Expression)
        {
            return 2;
        }

        public static int Len(DateTime Expression)
        {
            return 8;
        }

        public static int Len(decimal Expression)
        {
            return 8;
        }

        public static int Len(double Expression)
        {
            return 8;
        }

        public static int Len(short Expression)
        {
            return 2;
        }

        public static int Len(int Expression)
        {
            return 4;
        }

        public static int Len(long Expression)
        {
            return 8;
        }

        [SecuritySafeCritical]
        public static int Len(object Expression)
        {
            if (Expression == null)
            {
                return 0;
            }
            IConvertible convertible = Expression as IConvertible;
            if (convertible != null)
            {
                switch (convertible.GetTypeCode())
                {
                    case TypeCode.Boolean:
                        return 2;

                    case TypeCode.Char:
                        return 2;

                    case TypeCode.SByte:
                        return 1;

                    case TypeCode.Byte:
                        return 1;

                    case TypeCode.Int16:
                        return 2;

                    case TypeCode.UInt16:
                        return 2;

                    case TypeCode.Int32:
                        return 4;

                    case TypeCode.UInt32:
                        return 4;

                    case TypeCode.Int64:
                        return 8;

                    case TypeCode.UInt64:
                        return 8;

                    case TypeCode.Single:
                        return 4;

                    case TypeCode.Double:
                        return 8;

                    case TypeCode.Decimal:
                        return 0x10;

                    case TypeCode.DateTime:
                        return 8;

                    case TypeCode.String:
                        return Expression.ToString().Length;
                }
            }
            else
            {
                char[] chArray = Expression as char[];
                if (chArray != null)
                {
                    return chArray.Length;
                }
            }
            if (!(Expression is ValueType))
            {
                throw ExceptionUtils.VbMakeException(13);
            }
            new ReflectionPermission(ReflectionPermissionFlag.MemberAccess).Assert();
            int recordLength = StructUtils.GetRecordLength(Expression, 1);
            PermissionSet.RevertAssert();
            return recordLength;
        }

        [CLSCompliant(false)]
        public static int Len(sbyte Expression)
        {
            return 1;
        }

        public static int Len(float Expression)
        {
            return 4;
        }

        public static int Len(string Expression)
        {
            if (Expression == null)
            {
                return 0;
            }
            return Expression.Length;
        }

        [CLSCompliant(false)]
        public static int Len(ushort Expression)
        {
            return 2;
        }

        [CLSCompliant(false)]
        public static int Len(uint Expression)
        {
            return 4;
        }

        [CLSCompliant(false)]
        public static int Len(ulong Expression)
        {
            return 8;
        }

        public static string LSet(string Source, int Length)
        {
            if (Length == 0)
            {
                return "";
            }
            if (Source == null)
            {
                return new string(' ', Length);
            }
            if (Length > Source.Length)
            {
                return Source.PadRight(Length);
            }
            return Source.Substring(0, Length);
        }

        public static string LTrim(string str)
        {
            if ((str == null) || (str.Length == 0))
            {
                return "";
            }
            char ch = str[0];
            if ((ch != ' ') && (ch != '　'))
            {
                return str;
            }
            return str.TrimStart(Utils.m_achIntlSpace);
        }

        public static string Mid(string str, int Start)
        {
            string str2;
            try
            {
                if (str == null)
                {
                    return null;
                }
                str2 = Mid(str, Start, str.Length);
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return str2;
        }

        public static string Mid(string str, int Start, int Length)
        {
            if (Start <= 0)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_GTZero1", new string[] { "Start" }));
            }
            if (Length < 0)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_GEZero1", new string[] { "Length" }));
            }
            if ((Length == 0) || (str == null))
            {
                return "";
            }
            int length = str.Length;
            if (Start > length)
            {
                return "";
            }
            if ((Start + Length) > length)
            {
                return str.Substring(Start - 1);
            }
            return str.Substring(Start - 1, Length);
        }

        private static int PRIMARYLANGID(int lcid)
        {
            return (lcid & 0x3ff);
        }

        private static string ProperCaseString(CultureInfo loc, int dwMapFlags, string sSrc)
        {
            int length;
            if (sSrc == null)
            {
                length = 0;
            }
            else
            {
                length = sSrc.Length;
            }
            if (length == 0)
            {
                return "";
            }
            StringBuilder builder = new StringBuilder(vbLCMapString(loc, dwMapFlags | 0x100, sSrc));
            return loc.TextInfo.ToTitleCase(builder.ToString());
        }

        public static string Replace(string Expression, string Find, string Replacement, int Start = 1, int Count = -1, [OptionCompare] CompareMethod Compare = 0)
        {
            string str;
            try
            {
                if (Count < -1)
                {
                    throw new ArgumentException(Utils.GetResourceString("Argument_GEMinusOne1", new string[] { "Count" }));
                }
                if (Start <= 0)
                {
                    throw new ArgumentException(Utils.GetResourceString("Argument_GTZero1", new string[] { "Start" }));
                }
                if ((Expression == null) || (Start > Expression.Length))
                {
                    return null;
                }
                if (Start != 1)
                {
                    Expression = Expression.Substring(Start - 1);
                }
                if (((Find == null) || (Find.Length == 0)) || (Count == 0))
                {
                    return Expression;
                }
                if (Count == -1)
                {
                    Count = Expression.Length;
                }
                str = ReplaceInternal(Expression, Find, Replacement, Count, Compare);
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return str;
        }

        private static string ReplaceInternal(string Expression, string Find, string Replacement, int Count, CompareMethod Compare)
        {
            CompareOptions ordinal;
            CompareInfo compareInfo;
            int num5;
            int length = Expression.Length;
            int num2 = Find.Length;
            StringBuilder builder = new StringBuilder(length);
            if (Compare == CompareMethod.Text)
            {
                compareInfo = Utils.GetCultureInfo().CompareInfo;
                ordinal = CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreCase;
            }
            else
            {
                compareInfo = m_InvariantCompareInfo;
                ordinal = CompareOptions.Ordinal;
            }
            while (num5 < length)
            {
                int num4;
                if (num4 == Count)
                {
                    builder.Append(Expression.Substring(num5));
                    break;
                }
                int num3 = compareInfo.IndexOf(Expression, Find, num5, ordinal);
                if (num3 < 0)
                {
                    builder.Append(Expression.Substring(num5));
                    break;
                }
                builder.Append(Expression.Substring(num5, num3 - num5));
                builder.Append(Replacement);
                num4++;
                num5 = num3 + num2;
            }
            return builder.ToString();
        }

        public static string Right(string str, int Length)
        {
            if (Length < 0)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_GEZero1", new string[] { "Length" }));
            }
            if ((Length == 0) || (str == null))
            {
                return "";
            }
            int length = str.Length;
            if (Length >= length)
            {
                return str;
            }
            return str.Substring(length - Length, Length);
        }

        public static string RSet(string Source, int Length)
        {
            if (Length == 0)
            {
                return "";
            }
            if (Source == null)
            {
                return new string(' ', Length);
            }
            if (Length > Source.Length)
            {
                return Source.PadLeft(Length);
            }
            return Source.Substring(0, Length);
        }

        public static string RTrim(string str)
        {
            string str2;
            try
            {
                if ((str == null) || (str.Length == 0))
                {
                    return "";
                }
                switch (str[str.Length - 1])
                {
                    case ' ':
                    case '　':
                        return str.TrimEnd(Utils.m_achIntlSpace);
                }
                str2 = str;
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return str2;
        }

        public static string Space(int Number)
        {
            if (Number < 0)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_GEZero1", new string[] { "Number" }));
            }
            return new string(' ', Number);
        }

        public static string[] Split(string Expression, string Delimiter = " ", int Limit = -1, [OptionCompare] CompareMethod Compare = 0)
        {
            string[] strArray;
            try
            {
                int length;
                if ((Expression == null) || (Expression.Length == 0))
                {
                    return new string[] { "" };
                }
                if (Limit == -1)
                {
                    Limit = Expression.Length + 1;
                }
                if (Delimiter == null)
                {
                    length = 0;
                }
                else
                {
                    length = Delimiter.Length;
                }
                if (length == 0)
                {
                    return new string[] { Expression };
                }
                strArray = SplitHelper(Expression, Delimiter, Limit, (int) Compare);
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return strArray;
        }

        private static string[] SplitHelper(string sSrc, string sFind, int cMaxSubStrings, int Compare)
        {
            CompareInfo invariantCompareInfo;
            int num2;
            CompareOptions ordinal;
            int length;
            int num5;
            int num6;
            if (sFind == null)
            {
                length = 0;
            }
            else
            {
                length = sFind.Length;
            }
            if (sSrc == null)
            {
                num6 = 0;
            }
            else
            {
                num6 = sSrc.Length;
            }
            if (length == 0)
            {
                return new string[] { sSrc };
            }
            if (num6 == 0)
            {
                return new string[] { sSrc };
            }
            int num = 20;
            if (num > cMaxSubStrings)
            {
                num = cMaxSubStrings;
            }
            string[] strArray = new string[num + 1];
            if (Compare == 0)
            {
                ordinal = CompareOptions.Ordinal;
                invariantCompareInfo = m_InvariantCompareInfo;
            }
            else
            {
                invariantCompareInfo = Utils.GetCultureInfo().CompareInfo;
                ordinal = CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreCase;
            }
            while (num5 < num6)
            {
                string str;
                int num4 = invariantCompareInfo.IndexOf(sSrc, sFind, num5, num6 - num5, ordinal);
                if ((num4 == -1) || ((num2 + 1) == cMaxSubStrings))
                {
                    str = sSrc.Substring(num5);
                    if (str == null)
                    {
                        str = "";
                    }
                    strArray[num2] = str;
                    break;
                }
                str = sSrc.Substring(num5, num4 - num5);
                if (str == null)
                {
                    str = "";
                }
                strArray[num2] = str;
                num5 = num4 + length;
                num2++;
                if (num2 > num)
                {
                    num += 20;
                    if (num > cMaxSubStrings)
                    {
                        num = cMaxSubStrings + 1;
                    }
                    strArray = (string[]) Utils.CopyArray((Array) strArray, new string[num + 1]);
                }
                strArray[num2] = "";
                if (num2 == cMaxSubStrings)
                {
                    str = sSrc.Substring(num5);
                    if (str == null)
                    {
                        str = "";
                    }
                    strArray[num2] = str;
                    break;
                }
            }
            if ((num2 + 1) == strArray.Length)
            {
                return strArray;
            }
            return (string[]) Utils.CopyArray((Array) strArray, new string[num2 + 1]);
        }

        public static int StrComp(string String1, string String2, [OptionCompare] CompareMethod Compare = 0)
        {
            int num;
            try
            {
                if (Compare == CompareMethod.Binary)
                {
                    return Operators.CompareString(String1, String2, false);
                }
                if (Compare != CompareMethod.Text)
                {
                    throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Compare" }));
                }
                num = Operators.CompareString(String1, String2, true);
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return num;
        }

        public static string StrConv(string str, VbStrConv Conversion, int LocaleID = 0)
        {
            string str2;
            try
            {
                int num;
                CultureInfo cultureInfo;
                if ((LocaleID == 0) || (LocaleID == 1))
                {
                    cultureInfo = Utils.GetCultureInfo();
                    LocaleID = cultureInfo.LCID;
                }
                else
                {
                    try
                    {
                        cultureInfo = new CultureInfo(LocaleID & 0xffff);
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
                        throw new ArgumentException(Utils.GetResourceString("Argument_LCIDNotSupported1", new string[] { Conversions.ToString(LocaleID) }));
                    }
                }
                int num2 = PRIMARYLANGID(LocaleID);
                if ((Conversion & ~(VbStrConv.LinguisticCasing | VbStrConv.TraditionalChinese | VbStrConv.SimplifiedChinese | VbStrConv.Hiragana | VbStrConv.Katakana | VbStrConv.Narrow | VbStrConv.Wide | VbStrConv.ProperCase)) != VbStrConv.None)
                {
                    throw new ArgumentException(Utils.GetResourceString("Argument_InvalidVbStrConv"));
                }
                switch ((((int) Conversion) & 0x300))
                {
                    case 0x300:
                        throw new ArgumentException(Utils.GetResourceString("Argument_StrConvSCandTC"));

                    case 0x100:
                        if (!IsValidCodePage(0x3a8) || !IsValidCodePage(950))
                        {
                            throw new ArgumentException(Utils.GetResourceString("Argument_SCNotSupported"));
                        }
                        num |= 0x2000000;
                        break;

                    case 0x200:
                        if (!IsValidCodePage(0x3a8) || !IsValidCodePage(950))
                        {
                            throw new ArgumentException(Utils.GetResourceString("Argument_TCNotSupported"));
                        }
                        num |= 0x4000000;
                        break;
                }
                switch ((Conversion & VbStrConv.ProperCase))
                {
                    case VbStrConv.None:
                        if ((Conversion & VbStrConv.LinguisticCasing) != VbStrConv.None)
                        {
                            throw new ArgumentException(Utils.GetResourceString("LinguisticRequirements"));
                        }
                        goto Label_0192;

                    case VbStrConv.Uppercase:
                        if (Conversion != VbStrConv.Uppercase)
                        {
                            break;
                        }
                        return cultureInfo.TextInfo.ToUpper(str);

                    case VbStrConv.Lowercase:
                        if (Conversion != VbStrConv.Lowercase)
                        {
                            goto Label_018A;
                        }
                        return cultureInfo.TextInfo.ToLower(str);

                    case VbStrConv.ProperCase:
                        num = 0;
                        goto Label_0192;

                    default:
                        goto Label_0192;
                }
                num |= 0x200;
                goto Label_0192;
            Label_018A:
                num |= 0x100;
            Label_0192:
                if (((Conversion & (VbStrConv.Hiragana | VbStrConv.Katakana)) != VbStrConv.None) && ((num2 != 0x11) || !ValidLCID(LocaleID)))
                {
                    throw new ArgumentException(Utils.GetResourceString("Argument_JPNNotSupported"));
                }
                if ((Conversion & (VbStrConv.Narrow | VbStrConv.Wide)) != VbStrConv.None)
                {
                    if (((num2 != 0x11) && (num2 != 0x12)) && (num2 != 4))
                    {
                        throw new ArgumentException(Utils.GetResourceString("Argument_WideNarrowNotApplicable"));
                    }
                    if (!ValidLCID(LocaleID))
                    {
                        throw new ArgumentException(Utils.GetResourceString("Argument_LocalNotSupported"));
                    }
                }
                switch ((Conversion & (VbStrConv.Narrow | VbStrConv.Wide)))
                {
                    case VbStrConv.Wide:
                        num |= 0x800000;
                        break;

                    case VbStrConv.Narrow:
                        num |= 0x400000;
                        break;

                    case (VbStrConv.Narrow | VbStrConv.Wide):
                        throw new ArgumentException(Utils.GetResourceString("Argument_IllegalWideNarrow"));
                }
                VbStrConv conv3 = Conversion & (VbStrConv.Hiragana | VbStrConv.Katakana);
                if (conv3 != VbStrConv.None)
                {
                    if (conv3 == (VbStrConv.Hiragana | VbStrConv.Katakana))
                    {
                        throw new ArgumentException(Utils.GetResourceString("Argument_IllegalKataHira"));
                    }
                    if (conv3 == VbStrConv.Katakana)
                    {
                        num |= 0x200000;
                    }
                    else if (conv3 == VbStrConv.Hiragana)
                    {
                        num |= 0x100000;
                    }
                }
                if ((Conversion & VbStrConv.ProperCase) == VbStrConv.ProperCase)
                {
                    return ProperCaseString(cultureInfo, num, str);
                }
                if (num != 0)
                {
                    return vbLCMapString(cultureInfo, num, str);
                }
                str2 = str;
            }
            catch (Exception exception4)
            {
                throw exception4;
            }
            return str2;
        }

        public static string StrDup(int Number, char Character)
        {
            if (Number < 0)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_GEZero1", new string[] { "Number" }));
            }
            return new string(Character, Number);
        }

        public static object StrDup(int Number, object Character)
        {
            char ch;
            if (Number < 0)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Number" }));
            }
            if (Character == null)
            {
                throw new ArgumentNullException(Utils.GetResourceString("Argument_InvalidNullValue1", new string[] { "Character" }));
            }
            string str = Character as string;
            if (str != null)
            {
                if (str.Length == 0)
                {
                    throw new ArgumentException(Utils.GetResourceString("Argument_LengthGTZero1", new string[] { "Character" }));
                }
                ch = str[0];
            }
            else
            {
                try
                {
                    ch = Conversions.ToChar(Character);
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
                    throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Character" }));
                }
            }
            return new string(ch, Number);
        }

        public static string StrDup(int Number, string Character)
        {
            if (Number < 0)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_GEZero1", new string[] { "Number" }));
            }
            if ((Character == null) || (Character.Length == 0))
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_LengthGTZero1", new string[] { "Character" }));
            }
            return new string(Character[0], Number);
        }

        public static string StrReverse(string Expression)
        {
            if (Expression == null)
            {
                return "";
            }
            int length = Expression.Length;
            if (length == 0)
            {
                return "";
            }
            int num3 = length - 1;
            for (int i = 0; i <= num3; i++)
            {
                char c = Expression[i];
                switch (char.GetUnicodeCategory(c))
                {
                    case UnicodeCategory.Surrogate:
                    case UnicodeCategory.NonSpacingMark:
                    case UnicodeCategory.SpacingCombiningMark:
                    case UnicodeCategory.EnclosingMark:
                        return InternalStrReverse(Expression, i, length);
                }
            }
            char[] array = Expression.ToCharArray();
            Array.Reverse(array);
            return new string(array);
        }

        public static string Trim(string str)
        {
            string str2;
            try
            {
                if ((str == null) || (str.Length == 0))
                {
                    return "";
                }
                switch (str[0])
                {
                    case ' ':
                    case '　':
                        return str.Trim(Utils.m_achIntlSpace);
                }
                switch (str[str.Length - 1])
                {
                    case ' ':
                    case '　':
                        return str.Trim(Utils.m_achIntlSpace);
                }
                str2 = str;
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return str2;
        }

        public static char UCase(char Value)
        {
            char ch;
            try
            {
                ch = Thread.CurrentThread.CurrentCulture.TextInfo.ToUpper(Value);
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return ch;
        }

        public static string UCase(string Value)
        {
            string str;
            try
            {
                if (Value == null)
                {
                    return "";
                }
                str = Thread.CurrentThread.CurrentCulture.TextInfo.ToUpper(Value);
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return str;
        }

        private static void ValidateTriState(TriState Param)
        {
            if (((Param != TriState.True) && (Param != TriState.False)) && (Param != TriState.UseDefault))
            {
                throw ExceptionUtils.VbMakeException(5);
            }
        }

        internal static bool ValidLCID(int LocaleID)
        {
            bool flag;
            try
            {
                CultureInfo info = new CultureInfo(LocaleID);
                flag = true;
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
            return flag;
        }

        [SecuritySafeCritical]
        internal static string vbLCMapString(CultureInfo loc, int dwMapFlags, string sSrc)
        {
            int num2;
            int length;
            if (sSrc == null)
            {
                length = 0;
            }
            else
            {
                length = sSrc.Length;
            }
            if (length == 0)
            {
                return "";
            }
            int lCID = loc.LCID;
            Encoding encoding = Encoding.GetEncoding(loc.TextInfo.ANSICodePage);
            if (!encoding.IsSingleByte)
            {
                string s = sSrc;
                byte[] bytes = encoding.GetBytes(s);
                num2 = Microsoft.VisualBasic.CompilerServices.UnsafeNativeMethods.LCMapStringA(lCID, dwMapFlags, bytes, bytes.Length, null, 0);
                byte[] buffer = new byte[(num2 - 1) + 1];
                num2 = Microsoft.VisualBasic.CompilerServices.UnsafeNativeMethods.LCMapStringA(lCID, dwMapFlags, bytes, bytes.Length, buffer, num2);
                return encoding.GetString(buffer);
            }
            string lpDestStr = new string(' ', length);
            num2 = Microsoft.VisualBasic.CompilerServices.UnsafeNativeMethods.LCMapString(lCID, dwMapFlags, ref sSrc, length, ref lpDestStr, length);
            return lpDestStr;
        }

        private static string CachedOnOffFormatStyle
        {
            get
            {
                CultureInfo cultureInfo = Utils.GetCultureInfo();
                object syncObject = m_SyncObject;
                ObjectFlowControl.CheckForSyncLockOnValueType(syncObject);
                lock (syncObject)
                {
                    if (m_LastUsedOnOffCulture != cultureInfo)
                    {
                        m_LastUsedOnOffCulture = cultureInfo;
                        m_CachedOnOffFormatStyle = Utils.GetResourceString("OnOffFormatStyle");
                    }
                    return m_CachedOnOffFormatStyle;
                }
            }
        }

        private static string CachedTrueFalseFormatStyle
        {
            get
            {
                CultureInfo cultureInfo = Utils.GetCultureInfo();
                object syncObject = m_SyncObject;
                ObjectFlowControl.CheckForSyncLockOnValueType(syncObject);
                lock (syncObject)
                {
                    if (m_LastUsedTrueFalseCulture != cultureInfo)
                    {
                        m_LastUsedTrueFalseCulture = cultureInfo;
                        m_CachedTrueFalseFormatStyle = Utils.GetResourceString("TrueFalseFormatStyle");
                    }
                    return m_CachedTrueFalseFormatStyle;
                }
            }
        }

        private static string CachedYesNoFormatStyle
        {
            get
            {
                CultureInfo cultureInfo = Utils.GetCultureInfo();
                object syncObject = m_SyncObject;
                ObjectFlowControl.CheckForSyncLockOnValueType(syncObject);
                lock (syncObject)
                {
                    if (m_LastUsedYesNoCulture != cultureInfo)
                    {
                        m_LastUsedYesNoCulture = cultureInfo;
                        m_CachedYesNoFormatStyle = Utils.GetResourceString("YesNoFormatStyle");
                    }
                    return m_CachedYesNoFormatStyle;
                }
            }
        }

        internal enum FormatType
        {
            Number,
            Percent,
            Currency
        }

        private enum NamedFormats
        {
            UNKNOWN,
            GENERAL_NUMBER,
            LONG_TIME,
            MEDIUM_TIME,
            SHORT_TIME,
            GENERAL_DATE,
            LONG_DATE,
            MEDIUM_DATE,
            SHORT_DATE,
            FIXED,
            STANDARD,
            PERCENT,
            SCIENTIFIC,
            CURRENCY,
            TRUE_FALSE,
            YES_NO,
            ON_OFF
        }
    }
}

