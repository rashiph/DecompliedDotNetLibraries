namespace System
{
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public struct Char : IComparable, IConvertible, IComparable<char>, IEquatable<char>
    {
        public const char MaxValue = '￿';
        public const char MinValue = '\0';
        internal const int UNICODE_PLANE00_END = 0xffff;
        internal const int UNICODE_PLANE01_START = 0x10000;
        internal const int UNICODE_PLANE16_END = 0x10ffff;
        internal const int HIGH_SURROGATE_START = 0xd800;
        internal const int LOW_SURROGATE_END = 0xdfff;
        internal char m_value;
        private static readonly byte[] categoryForLatin1;
        private static bool IsLatin1(char ch)
        {
            return (ch <= '\x00ff');
        }

        private static bool IsAscii(char ch)
        {
            return (ch <= '\x007f');
        }

        private static UnicodeCategory GetLatin1UnicodeCategory(char ch)
        {
            return (UnicodeCategory) categoryForLatin1[ch];
        }

        public override int GetHashCode()
        {
            return (this | (this << 0x10));
        }

        public override bool Equals(object obj)
        {
            return ((obj is char) && (this == ((char) obj)));
        }

        public bool Equals(char obj)
        {
            return (this == obj);
        }

        public int CompareTo(object value)
        {
            if (value == null)
            {
                return 1;
            }
            if (!(value is char))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeChar"));
            }
            return (this - ((char) value));
        }

        public int CompareTo(char value)
        {
            return (this - value);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public override string ToString()
        {
            return ToString(this);
        }

        public string ToString(IFormatProvider provider)
        {
            return ToString(this);
        }

        public static string ToString(char c)
        {
            return new string(c, 1);
        }

        public static char Parse(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            if (s.Length != 1)
            {
                throw new FormatException(Environment.GetResourceString("Format_NeedSingleChar"));
            }
            return s[0];
        }

        public static bool TryParse(string s, out char result)
        {
            result = '\0';
            if (s == null)
            {
                return false;
            }
            if (s.Length != 1)
            {
                return false;
            }
            result = s[0];
            return true;
        }

        public static bool IsDigit(char c)
        {
            if (!IsLatin1(c))
            {
                return (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.DecimalDigitNumber);
            }
            return ((c >= '0') && (c <= '9'));
        }

        internal static bool CheckLetter(UnicodeCategory uc)
        {
            switch (uc)
            {
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.TitlecaseLetter:
                case UnicodeCategory.ModifierLetter:
                case UnicodeCategory.OtherLetter:
                    return true;
            }
            return false;
        }

        public static bool IsLetter(char c)
        {
            if (!IsLatin1(c))
            {
                return CheckLetter(CharUnicodeInfo.GetUnicodeCategory(c));
            }
            if (!IsAscii(c))
            {
                return CheckLetter(GetLatin1UnicodeCategory(c));
            }
            c = (char) (c | ' ');
            return ((c >= 'a') && (c <= 'z'));
        }

        private static bool IsWhiteSpaceLatin1(char c)
        {
            if (((c != ' ') && ((c < '\t') || (c > '\r'))) && ((c != '\x00a0') && (c != '\x0085')))
            {
                return false;
            }
            return true;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool IsWhiteSpace(char c)
        {
            if (IsLatin1(c))
            {
                return IsWhiteSpaceLatin1(c);
            }
            return CharUnicodeInfo.IsWhiteSpace(c);
        }

        public static bool IsUpper(char c)
        {
            if (!IsLatin1(c))
            {
                return (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.UppercaseLetter);
            }
            if (!IsAscii(c))
            {
                return (GetLatin1UnicodeCategory(c) == UnicodeCategory.UppercaseLetter);
            }
            return ((c >= 'A') && (c <= 'Z'));
        }

        public static bool IsLower(char c)
        {
            if (!IsLatin1(c))
            {
                return (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.LowercaseLetter);
            }
            if (!IsAscii(c))
            {
                return (GetLatin1UnicodeCategory(c) == UnicodeCategory.LowercaseLetter);
            }
            return ((c >= 'a') && (c <= 'z'));
        }

        internal static bool CheckPunctuation(UnicodeCategory uc)
        {
            switch (uc)
            {
                case UnicodeCategory.ConnectorPunctuation:
                case UnicodeCategory.DashPunctuation:
                case UnicodeCategory.OpenPunctuation:
                case UnicodeCategory.ClosePunctuation:
                case UnicodeCategory.InitialQuotePunctuation:
                case UnicodeCategory.FinalQuotePunctuation:
                case UnicodeCategory.OtherPunctuation:
                    return true;
            }
            return false;
        }

        public static bool IsPunctuation(char c)
        {
            if (IsLatin1(c))
            {
                return CheckPunctuation(GetLatin1UnicodeCategory(c));
            }
            return CheckPunctuation(CharUnicodeInfo.GetUnicodeCategory(c));
        }

        internal static bool CheckLetterOrDigit(UnicodeCategory uc)
        {
            switch (uc)
            {
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.TitlecaseLetter:
                case UnicodeCategory.ModifierLetter:
                case UnicodeCategory.OtherLetter:
                case UnicodeCategory.DecimalDigitNumber:
                    return true;
            }
            return false;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool IsLetterOrDigit(char c)
        {
            if (IsLatin1(c))
            {
                return CheckLetterOrDigit(GetLatin1UnicodeCategory(c));
            }
            return CheckLetterOrDigit(CharUnicodeInfo.GetUnicodeCategory(c));
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static char ToUpper(char c, CultureInfo culture)
        {
            if (culture == null)
            {
                throw new ArgumentNullException("culture");
            }
            return culture.TextInfo.ToUpper(c);
        }

        public static char ToUpper(char c)
        {
            return ToUpper(c, CultureInfo.CurrentCulture);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static char ToUpperInvariant(char c)
        {
            return ToUpper(c, CultureInfo.InvariantCulture);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static char ToLower(char c, CultureInfo culture)
        {
            if (culture == null)
            {
                throw new ArgumentNullException("culture");
            }
            return culture.TextInfo.ToLower(c);
        }

        public static char ToLower(char c)
        {
            return ToLower(c, CultureInfo.CurrentCulture);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static char ToLowerInvariant(char c)
        {
            return ToLower(c, CultureInfo.InvariantCulture);
        }

        public TypeCode GetTypeCode()
        {
            return TypeCode.Char;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", new object[] { "Char", "Boolean" }));
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            return this;
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(this);
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(this);
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(this);
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(this);
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return Convert.ToInt32(this);
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(this);
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(this);
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(this);
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", new object[] { "Char", "Single" }));
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", new object[] { "Char", "Double" }));
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", new object[] { "Char", "Decimal" }));
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", new object[] { "Char", "DateTime" }));
        }

        object IConvertible.ToType(Type type, IFormatProvider provider)
        {
            return Convert.DefaultToType(this, type, provider);
        }

        public static bool IsControl(char c)
        {
            if (IsLatin1(c))
            {
                return (GetLatin1UnicodeCategory(c) == UnicodeCategory.Control);
            }
            return (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.Control);
        }

        [SecuritySafeCritical]
        public static bool IsControl(string s, int index)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            if (index >= s.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            char ch = s[index];
            if (IsLatin1(ch))
            {
                return (GetLatin1UnicodeCategory(ch) == UnicodeCategory.Control);
            }
            return (CharUnicodeInfo.GetUnicodeCategory(s, index) == UnicodeCategory.Control);
        }

        [SecuritySafeCritical]
        public static bool IsDigit(string s, int index)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            if (index >= s.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            char ch = s[index];
            if (!IsLatin1(ch))
            {
                return (CharUnicodeInfo.GetUnicodeCategory(s, index) == UnicodeCategory.DecimalDigitNumber);
            }
            return ((ch >= '0') && (ch <= '9'));
        }

        [SecuritySafeCritical]
        public static bool IsLetter(string s, int index)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            if (index >= s.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            char ch = s[index];
            if (!IsLatin1(ch))
            {
                return CheckLetter(CharUnicodeInfo.GetUnicodeCategory(s, index));
            }
            if (!IsAscii(ch))
            {
                return CheckLetter(GetLatin1UnicodeCategory(ch));
            }
            ch = (char) (ch | ' ');
            return ((ch >= 'a') && (ch <= 'z'));
        }

        [SecuritySafeCritical]
        public static bool IsLetterOrDigit(string s, int index)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            if (index >= s.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            char ch = s[index];
            if (IsLatin1(ch))
            {
                return CheckLetterOrDigit(GetLatin1UnicodeCategory(ch));
            }
            return CheckLetterOrDigit(CharUnicodeInfo.GetUnicodeCategory(s, index));
        }

        [SecuritySafeCritical]
        public static bool IsLower(string s, int index)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            if (index >= s.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            char ch = s[index];
            if (!IsLatin1(ch))
            {
                return (CharUnicodeInfo.GetUnicodeCategory(s, index) == UnicodeCategory.LowercaseLetter);
            }
            if (!IsAscii(ch))
            {
                return (GetLatin1UnicodeCategory(ch) == UnicodeCategory.LowercaseLetter);
            }
            return ((ch >= 'a') && (ch <= 'z'));
        }

        internal static bool CheckNumber(UnicodeCategory uc)
        {
            switch (uc)
            {
                case UnicodeCategory.DecimalDigitNumber:
                case UnicodeCategory.LetterNumber:
                case UnicodeCategory.OtherNumber:
                    return true;
            }
            return false;
        }

        public static bool IsNumber(char c)
        {
            if (!IsLatin1(c))
            {
                return CheckNumber(CharUnicodeInfo.GetUnicodeCategory(c));
            }
            if (!IsAscii(c))
            {
                return CheckNumber(GetLatin1UnicodeCategory(c));
            }
            return ((c >= '0') && (c <= '9'));
        }

        [SecuritySafeCritical]
        public static bool IsNumber(string s, int index)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            if (index >= s.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            char ch = s[index];
            if (!IsLatin1(ch))
            {
                return CheckNumber(CharUnicodeInfo.GetUnicodeCategory(s, index));
            }
            if (!IsAscii(ch))
            {
                return CheckNumber(GetLatin1UnicodeCategory(ch));
            }
            return ((ch >= '0') && (ch <= '9'));
        }

        [SecuritySafeCritical]
        public static bool IsPunctuation(string s, int index)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            if (index >= s.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            char ch = s[index];
            if (IsLatin1(ch))
            {
                return CheckPunctuation(GetLatin1UnicodeCategory(ch));
            }
            return CheckPunctuation(CharUnicodeInfo.GetUnicodeCategory(s, index));
        }

        internal static bool CheckSeparator(UnicodeCategory uc)
        {
            switch (uc)
            {
                case UnicodeCategory.SpaceSeparator:
                case UnicodeCategory.LineSeparator:
                case UnicodeCategory.ParagraphSeparator:
                    return true;
            }
            return false;
        }

        private static bool IsSeparatorLatin1(char c)
        {
            if (c != ' ')
            {
                return (c == '\x00a0');
            }
            return true;
        }

        public static bool IsSeparator(char c)
        {
            if (IsLatin1(c))
            {
                return IsSeparatorLatin1(c);
            }
            return CheckSeparator(CharUnicodeInfo.GetUnicodeCategory(c));
        }

        [SecuritySafeCritical]
        public static bool IsSeparator(string s, int index)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            if (index >= s.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            char ch = s[index];
            if (IsLatin1(ch))
            {
                return IsSeparatorLatin1(ch);
            }
            return CheckSeparator(CharUnicodeInfo.GetUnicodeCategory(s, index));
        }

        public static bool IsSurrogate(char c)
        {
            return ((c >= 0xd800) && (c <= 0xdfff));
        }

        public static bool IsSurrogate(string s, int index)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            if (index >= s.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            return IsSurrogate(s[index]);
        }

        internal static bool CheckSymbol(UnicodeCategory uc)
        {
            switch (uc)
            {
                case UnicodeCategory.MathSymbol:
                case UnicodeCategory.CurrencySymbol:
                case UnicodeCategory.ModifierSymbol:
                case UnicodeCategory.OtherSymbol:
                    return true;
            }
            return false;
        }

        public static bool IsSymbol(char c)
        {
            if (IsLatin1(c))
            {
                return CheckSymbol(GetLatin1UnicodeCategory(c));
            }
            return CheckSymbol(CharUnicodeInfo.GetUnicodeCategory(c));
        }

        [SecuritySafeCritical]
        public static bool IsSymbol(string s, int index)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            if (index >= s.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (IsLatin1(s[index]))
            {
                return CheckSymbol(GetLatin1UnicodeCategory(s[index]));
            }
            return CheckSymbol(CharUnicodeInfo.GetUnicodeCategory(s, index));
        }

        [SecuritySafeCritical]
        public static bool IsUpper(string s, int index)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            if (index >= s.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            char ch = s[index];
            if (!IsLatin1(ch))
            {
                return (CharUnicodeInfo.GetUnicodeCategory(s, index) == UnicodeCategory.UppercaseLetter);
            }
            if (!IsAscii(ch))
            {
                return (GetLatin1UnicodeCategory(ch) == UnicodeCategory.UppercaseLetter);
            }
            return ((ch >= 'A') && (ch <= 'Z'));
        }

        [SecuritySafeCritical]
        public static bool IsWhiteSpace(string s, int index)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            if (index >= s.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (IsLatin1(s[index]))
            {
                return IsWhiteSpaceLatin1(s[index]);
            }
            return CharUnicodeInfo.IsWhiteSpace(s, index);
        }

        [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static UnicodeCategory GetUnicodeCategory(char c)
        {
            if (IsLatin1(c))
            {
                return GetLatin1UnicodeCategory(c);
            }
            return CharUnicodeInfo.InternalGetUnicodeCategory(c);
        }

        [SecuritySafeCritical]
        public static UnicodeCategory GetUnicodeCategory(string s, int index)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            if (index >= s.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (IsLatin1(s[index]))
            {
                return GetLatin1UnicodeCategory(s[index]);
            }
            return CharUnicodeInfo.InternalGetUnicodeCategory(s, index);
        }

        public static double GetNumericValue(char c)
        {
            return CharUnicodeInfo.GetNumericValue(c);
        }

        [SecuritySafeCritical]
        public static double GetNumericValue(string s, int index)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            if (index >= s.Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            return CharUnicodeInfo.GetNumericValue(s, index);
        }

        public static bool IsHighSurrogate(char c)
        {
            return ((c >= 0xd800) && (c <= 0xdbff));
        }

        public static bool IsHighSurrogate(string s, int index)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            if ((index < 0) || (index >= s.Length))
            {
                throw new ArgumentOutOfRangeException("index");
            }
            return IsHighSurrogate(s[index]);
        }

        public static bool IsLowSurrogate(char c)
        {
            return ((c >= 0xdc00) && (c <= 0xdfff));
        }

        public static bool IsLowSurrogate(string s, int index)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            if ((index < 0) || (index >= s.Length))
            {
                throw new ArgumentOutOfRangeException("index");
            }
            return IsLowSurrogate(s[index]);
        }

        public static bool IsSurrogatePair(string s, int index)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            if ((index < 0) || (index >= s.Length))
            {
                throw new ArgumentOutOfRangeException("index");
            }
            return (((index + 1) < s.Length) && IsSurrogatePair(s[index], s[index + 1]));
        }

        public static bool IsSurrogatePair(char highSurrogate, char lowSurrogate)
        {
            if ((highSurrogate < 0xd800) || (highSurrogate > 0xdbff))
            {
                return false;
            }
            return ((lowSurrogate >= 0xdc00) && (lowSurrogate <= 0xdfff));
        }

        public static string ConvertFromUtf32(int utf32)
        {
            if (((utf32 < 0) || (utf32 > 0x10ffff)) || ((utf32 >= 0xd800) && (utf32 <= 0xdfff)))
            {
                throw new ArgumentOutOfRangeException("utf32", Environment.GetResourceString("ArgumentOutOfRange_InvalidUTF32"));
            }
            if (utf32 < 0x10000)
            {
                return ToString((char) utf32);
            }
            utf32 -= 0x10000;
            return new string(new char[] { (char) ((utf32 / 0x400) + 0xd800), (char) ((utf32 % 0x400) + 0xdc00) });
        }

        public static int ConvertToUtf32(char highSurrogate, char lowSurrogate)
        {
            if (!IsHighSurrogate(highSurrogate))
            {
                throw new ArgumentOutOfRangeException("highSurrogate", Environment.GetResourceString("ArgumentOutOfRange_InvalidHighSurrogate"));
            }
            if (!IsLowSurrogate(lowSurrogate))
            {
                throw new ArgumentOutOfRangeException("lowSurrogate", Environment.GetResourceString("ArgumentOutOfRange_InvalidLowSurrogate"));
            }
            return ((((highSurrogate - 0xd800) * 0x400) + (lowSurrogate - 0xdc00)) + 0x10000);
        }

        public static int ConvertToUtf32(string s, int index)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            if ((index < 0) || (index >= s.Length))
            {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            int num = s[index] - 0xd800;
            if ((num < 0) || (num > 0x7ff))
            {
                return s[index];
            }
            if (num > 0x3ff)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidLowSurrogate", new object[] { index }), "s");
            }
            if (index >= (s.Length - 1))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidHighSurrogate", new object[] { index }), "s");
            }
            int num2 = s[index + 1] - 0xdc00;
            if ((num2 < 0) || (num2 > 0x3ff))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidHighSurrogate", new object[] { index }), "s");
            }
            return (((num * 0x400) + num2) + 0x10000);
        }

        static Char()
        {
            categoryForLatin1 = new byte[] { 
                14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 
                14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 
                11, 0x18, 0x18, 0x18, 0x1a, 0x18, 0x18, 0x18, 20, 0x15, 0x18, 0x19, 0x18, 0x13, 0x18, 0x18, 
                8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 0x18, 0x18, 0x19, 0x19, 0x19, 0x18, 
                0x18, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 20, 0x18, 0x15, 0x1b, 0x12, 
                0x1b, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 20, 0x19, 0x15, 0x19, 14, 
                14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 
                14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 
                11, 0x18, 0x1a, 0x1a, 0x1a, 0x1a, 0x1c, 0x1c, 0x1b, 0x1c, 1, 0x16, 0x19, 0x13, 0x1c, 0x1b, 
                0x1c, 0x19, 10, 10, 0x1b, 1, 0x1c, 0x18, 0x1b, 10, 1, 0x17, 10, 10, 10, 0x18, 
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
                0, 0, 0, 0, 0, 0, 0, 0x19, 0, 0, 0, 0, 0, 0, 0, 1, 
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 
                1, 1, 1, 1, 1, 1, 1, 0x19, 1, 1, 1, 1, 1, 1, 1, 1
             };
        }
    }
}

