namespace System.Xml
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Text;

    internal static class XmlConverter
    {
        private static System.Text.Base64Encoding base64Encoding;
        public const int MaxBoolChars = 5;
        public const int MaxDateTimeChars = 0x40;
        public const int MaxDecimalChars = 40;
        public const int MaxDoubleChars = 0x20;
        public const int MaxFloatChars = 0x10;
        public const int MaxInt32Chars = 0x10;
        public const int MaxInt64Chars = 0x20;
        public const int MaxPrimitiveChars = 0x40;
        public const int MaxUInt64Chars = 0x20;
        private static System.Text.UnicodeEncoding unicodeEncoding;
        private static System.Text.UTF8Encoding utf8Encoding;
        private static char[] whiteSpaceChars = new char[] { ' ', '\t', '\n', '\r' };

        [SecuritySafeCritical]
        private static unsafe bool IsNegativeZero(double value)
        {
            double num = 0.0;
            return (*(((long*) &value)) == *(((long*) &num)));
        }

        [SecuritySafeCritical]
        private static unsafe bool IsNegativeZero(float value)
        {
            float num = 0f;
            return (*(((int*) &value)) == *(((int*) &num)));
        }

        public static bool IsWhitespace(char ch)
        {
            if (ch > ' ')
            {
                return false;
            }
            if (((ch != ' ') && (ch != '\t')) && (ch != '\r'))
            {
                return (ch == '\n');
            }
            return true;
        }

        public static bool IsWhitespace(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                if (!IsWhitespace(s[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static string StripWhitespace(string s)
        {
            int length = s.Length;
            for (int i = 0; i < s.Length; i++)
            {
                if (IsWhitespace(s[i]))
                {
                    length--;
                }
            }
            if (length == s.Length)
            {
                return s;
            }
            char[] chArray = new char[length];
            length = 0;
            for (int j = 0; j < s.Length; j++)
            {
                char ch = s[j];
                if (!IsWhitespace(ch))
                {
                    chArray[length++] = ch;
                }
            }
            return new string(chArray);
        }

        private static int ToAsciiChars(string s, byte[] buffer, int offset)
        {
            for (int i = 0; i < s.Length; i++)
            {
                buffer[offset++] = (byte) s[i];
            }
            return s.Length;
        }

        public static bool ToBoolean(string value)
        {
            bool flag;
            try
            {
                flag = XmlConvert.ToBoolean(value);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "Boolean", exception));
            }
            catch (FormatException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "Boolean", exception2));
            }
            return flag;
        }

        public static bool ToBoolean(byte[] buffer, int offset, int count)
        {
            if (count == 1)
            {
                switch (buffer[offset])
                {
                    case 0x31:
                        return true;

                    case 0x30:
                        return false;
                }
            }
            return ToBoolean(ToString(buffer, offset, count));
        }

        public static byte[] ToBytes(string value)
        {
            byte[] bytes;
            try
            {
                bytes = UTF8Encoding.GetBytes(value);
            }
            catch (DecoderFallbackException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateEncodingException(value, exception));
            }
            return bytes;
        }

        public static int ToChars(bool value, byte[] buffer, int offset)
        {
            if (value)
            {
                buffer[offset] = 0x74;
                buffer[offset + 1] = 0x72;
                buffer[offset + 2] = 0x75;
                buffer[offset + 3] = 0x65;
                return 4;
            }
            buffer[offset] = 0x66;
            buffer[offset + 1] = 0x61;
            buffer[offset + 2] = 0x6c;
            buffer[offset + 3] = 0x73;
            buffer[offset + 4] = 0x65;
            return 5;
        }

        public static int ToChars(DateTime value, byte[] chars, int offset)
        {
            TimeSpan utcOffset;
            int num = offset;
            offset += ToCharsD4(value.Year, chars, offset);
            chars[offset++] = 0x2d;
            offset += ToCharsD2(value.Month, chars, offset);
            chars[offset++] = 0x2d;
            offset += ToCharsD2(value.Day, chars, offset);
            chars[offset++] = 0x54;
            offset += ToCharsD2(value.Hour, chars, offset);
            chars[offset++] = 0x3a;
            offset += ToCharsD2(value.Minute, chars, offset);
            chars[offset++] = 0x3a;
            offset += ToCharsD2(value.Second, chars, offset);
            int num2 = (int) (value.Ticks % 0x989680L);
            if (num2 != 0)
            {
                chars[offset++] = 0x2e;
                offset += ToCharsD7(num2, chars, offset);
            }
            switch (value.Kind)
            {
                case DateTimeKind.Unspecified:
                    goto Label_0168;

                case DateTimeKind.Utc:
                    chars[offset++] = 90;
                    goto Label_0168;

                case DateTimeKind.Local:
                    utcOffset = TimeZoneInfo.Local.GetUtcOffset(value);
                    if (utcOffset.Ticks >= 0L)
                    {
                        chars[offset++] = 0x2b;
                        break;
                    }
                    chars[offset++] = 0x2d;
                    break;

                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException());
            }
            offset += ToCharsD2(Math.Abs(utcOffset.Hours), chars, offset);
            chars[offset++] = 0x3a;
            offset += ToCharsD2(Math.Abs(utcOffset.Minutes), chars, offset);
        Label_0168:
            return (offset - num);
        }

        public static int ToChars(decimal value, byte[] buffer, int offset)
        {
            return ToAsciiChars(value.ToString(null, NumberFormatInfo.InvariantInfo), buffer, offset);
        }

        public static int ToChars(double value, byte[] buffer, int offset)
        {
            if (double.IsInfinity(value))
            {
                return ToInfinity(double.IsNegativeInfinity(value), buffer, offset);
            }
            if (value == 0.0)
            {
                return ToZero(IsNegativeZero(value), buffer, offset);
            }
            return ToAsciiChars(value.ToString("R", NumberFormatInfo.InvariantInfo), buffer, offset);
        }

        public static int ToChars(int value, byte[] chars, int offset)
        {
            int count = ToCharsR(value, chars, offset + 0x10);
            Buffer.BlockCopy(chars, (offset + 0x10) - count, chars, offset, count);
            return count;
        }

        public static int ToChars(long value, byte[] chars, int offset)
        {
            int count = ToCharsR(value, chars, offset + 0x20);
            Buffer.BlockCopy(chars, (offset + 0x20) - count, chars, offset, count);
            return count;
        }

        public static int ToChars(float value, byte[] buffer, int offset)
        {
            if (float.IsInfinity(value))
            {
                return ToInfinity(float.IsNegativeInfinity(value), buffer, offset);
            }
            if (value == 0.0)
            {
                return ToZero(IsNegativeZero(value), buffer, offset);
            }
            return ToAsciiChars(value.ToString("R", NumberFormatInfo.InvariantInfo), buffer, offset);
        }

        public static int ToChars(ulong value, byte[] buffer, int offset)
        {
            return ToAsciiChars(value.ToString(null, NumberFormatInfo.InvariantInfo), buffer, offset);
        }

        public static int ToChars(byte[] buffer, int offset, int count, char[] chars, int charOffset)
        {
            int num;
            try
            {
                num = UTF8Encoding.GetChars(buffer, offset, count, chars, charOffset);
            }
            catch (DecoderFallbackException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateEncodingException(buffer, offset, count, exception));
            }
            return num;
        }

        private static int ToCharsD2(int value, byte[] chars, int offset)
        {
            if (value < 10)
            {
                chars[offset] = 0x30;
                chars[offset + 1] = (byte) (0x30 + value);
            }
            else
            {
                int num = value / 10;
                chars[offset] = (byte) (0x30 + num);
                chars[offset + 1] = (byte) ((0x30 + value) - (num * 10));
            }
            return 2;
        }

        private static int ToCharsD4(int value, byte[] chars, int offset)
        {
            ToCharsD2(value / 100, chars, offset);
            ToCharsD2(value % 100, chars, offset + 2);
            return 4;
        }

        private static int ToCharsD7(int value, byte[] chars, int offset)
        {
            int num = 7 - ToCharsR(value, chars, offset + 7);
            for (int i = 0; i < num; i++)
            {
                chars[offset + i] = 0x30;
            }
            int num3 = 7;
            while ((num3 > 0) && (chars[(offset + num3) - 1] == 0x30))
            {
                num3--;
            }
            return num3;
        }

        public static int ToCharsR(int value, byte[] chars, int offset)
        {
            int num = 0;
            if (value < 0)
            {
                while (value <= -10)
                {
                    int num3 = value / 10;
                    num++;
                    chars[--offset] = (byte) (0x30 - (value - (num3 * 10)));
                    value = num3;
                }
                chars[--offset] = (byte) (0x30 - value);
                chars[--offset] = 0x2d;
                return (num + 2);
            }
            while (value >= 10)
            {
                int num2 = value / 10;
                num++;
                chars[--offset] = (byte) (0x30 + (value - (num2 * 10)));
                value = num2;
            }
            chars[--offset] = (byte) (0x30 + value);
            num++;
            return num;
        }

        public static int ToCharsR(long value, byte[] chars, int offset)
        {
            int num = 0;
            if (value < 0L)
            {
                while (value < -2147483648L)
                {
                    long num3 = value / 10L;
                    num++;
                    chars[--offset] = (byte) (0x30 - ((int) (value - (num3 * 10L))));
                    value = num3;
                }
            }
            else
            {
                while (value > 0x7fffffffL)
                {
                    long num2 = value / 10L;
                    num++;
                    chars[--offset] = (byte) (0x30 + ((int) (value - (num2 * 10L))));
                    value = num2;
                }
            }
            return (num + ToCharsR((int) value, chars, offset));
        }

        public static DateTime ToDateTime(long value)
        {
            DateTime time;
            try
            {
                time = DateTime.FromBinary(value);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(ToString(value), "DateTime", exception));
            }
            return time;
        }

        public static DateTime ToDateTime(string value)
        {
            DateTime time;
            try
            {
                time = XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.RoundtripKind);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "DateTime", exception));
            }
            catch (FormatException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "DateTime", exception2));
            }
            return time;
        }

        public static DateTime ToDateTime(byte[] buffer, int offset, int count)
        {
            DateTime time;
            if (TryParseDateTime(buffer, offset, count, out time))
            {
                return time;
            }
            return ToDateTime(ToString(buffer, offset, count));
        }

        public static decimal ToDecimal(string value)
        {
            decimal num;
            try
            {
                num = XmlConvert.ToDecimal(value);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "decimal", exception));
            }
            catch (FormatException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "decimal", exception2));
            }
            catch (OverflowException exception3)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "decimal", exception3));
            }
            return num;
        }

        public static decimal ToDecimal(byte[] buffer, int offset, int count)
        {
            return ToDecimal(ToString(buffer, offset, count));
        }

        public static double ToDouble(string value)
        {
            double num;
            try
            {
                num = XmlConvert.ToDouble(value);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "double", exception));
            }
            catch (FormatException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "double", exception2));
            }
            catch (OverflowException exception3)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "double", exception3));
            }
            return num;
        }

        public static double ToDouble(byte[] buffer, int offset, int count)
        {
            double num;
            if (TryParseDouble(buffer, offset, count, out num))
            {
                return num;
            }
            return ToDouble(ToString(buffer, offset, count));
        }

        public static Guid ToGuid(string value)
        {
            Guid guid;
            try
            {
                guid = Guid.Parse(Trim(value));
            }
            catch (FormatException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "Guid", exception));
            }
            catch (ArgumentException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "Guid", exception2));
            }
            catch (OverflowException exception3)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "Guid", exception3));
            }
            return guid;
        }

        public static Guid ToGuid(byte[] buffer, int offset, int count)
        {
            return ToGuid(ToString(buffer, offset, count));
        }

        private static int ToInfinity(bool isNegative, byte[] buffer, int offset)
        {
            if (isNegative)
            {
                buffer[offset] = 0x2d;
                buffer[offset + 1] = 0x49;
                buffer[offset + 2] = 0x4e;
                buffer[offset + 3] = 70;
                return 4;
            }
            buffer[offset] = 0x49;
            buffer[offset + 1] = 0x4e;
            buffer[offset + 2] = 70;
            return 3;
        }

        public static int ToInt32(string value)
        {
            int num;
            try
            {
                num = XmlConvert.ToInt32(value);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "Int32", exception));
            }
            catch (FormatException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "Int32", exception2));
            }
            catch (OverflowException exception3)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "Int32", exception3));
            }
            return num;
        }

        public static int ToInt32(byte[] buffer, int offset, int count)
        {
            int num;
            if (TryParseInt32(buffer, offset, count, out num))
            {
                return num;
            }
            return ToInt32(ToString(buffer, offset, count));
        }

        private static int ToInt32D2(byte[] chars, int offset)
        {
            byte num = (byte) (chars[offset] - 0x30);
            byte num2 = (byte) (chars[offset + 1] - 0x30);
            if ((num <= 9) && (num2 <= 9))
            {
                return ((10 * num) + num2);
            }
            return -1;
        }

        private static int ToInt32D4(byte[] chars, int offset, int count)
        {
            return ToInt32D7(chars, offset, count);
        }

        private static int ToInt32D7(byte[] chars, int offset, int count)
        {
            int num = 0;
            for (int i = 0; i < count; i++)
            {
                byte num3 = (byte) (chars[offset + i] - 0x30);
                if (num3 > 9)
                {
                    return -1;
                }
                num = (num * 10) + num3;
            }
            return num;
        }

        public static long ToInt64(string value)
        {
            long num;
            try
            {
                num = XmlConvert.ToInt64(value);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "Int64", exception));
            }
            catch (FormatException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "Int64", exception2));
            }
            catch (OverflowException exception3)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "Int64", exception3));
            }
            return num;
        }

        public static long ToInt64(byte[] buffer, int offset, int count)
        {
            long num;
            if (TryParseInt64(buffer, offset, count, out num))
            {
                return num;
            }
            return ToInt64(ToString(buffer, offset, count));
        }

        public static void ToQualifiedName(string qname, out string prefix, out string localName)
        {
            int index = qname.IndexOf(':');
            if (index < 0)
            {
                prefix = string.Empty;
                localName = Trim(qname);
            }
            else
            {
                if (index == (qname.Length - 1))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("XmlInvalidQualifiedName", new object[] { qname })));
                }
                prefix = Trim(qname.Substring(0, index));
                localName = Trim(qname.Substring(index + 1));
            }
        }

        public static float ToSingle(string value)
        {
            float num;
            try
            {
                num = XmlConvert.ToSingle(value);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "float", exception));
            }
            catch (FormatException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "float", exception2));
            }
            catch (OverflowException exception3)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "float", exception3));
            }
            return num;
        }

        public static float ToSingle(byte[] buffer, int offset, int count)
        {
            float num;
            if (TryParseSingle(buffer, offset, count, out num))
            {
                return num;
            }
            return ToSingle(ToString(buffer, offset, count));
        }

        public static string ToString(bool value)
        {
            if (!value)
            {
                return "false";
            }
            return "true";
        }

        public static string ToString(DateTime value)
        {
            byte[] chars = new byte[0x40];
            int count = ToChars(value, chars, 0);
            return ToString(chars, 0, count);
        }

        public static string ToString(decimal value)
        {
            return XmlConvert.ToString(value);
        }

        public static string ToString(double value)
        {
            return XmlConvert.ToString(value);
        }

        public static string ToString(Guid value)
        {
            return value.ToString();
        }

        public static string ToString(int value)
        {
            return XmlConvert.ToString(value);
        }

        public static string ToString(long value)
        {
            return XmlConvert.ToString(value);
        }

        public static string ToString(float value)
        {
            return XmlConvert.ToString(value);
        }

        public static string ToString(TimeSpan value)
        {
            return XmlConvert.ToString(value);
        }

        public static string ToString(ulong value)
        {
            return value.ToString(NumberFormatInfo.InvariantInfo);
        }

        public static string ToString(UniqueId value)
        {
            return value.ToString();
        }

        public static string ToString(object[] objects)
        {
            if (objects.Length == 0)
            {
                return string.Empty;
            }
            string str = ToString(objects[0]);
            if (objects.Length <= 1)
            {
                return str;
            }
            StringBuilder builder = new StringBuilder(str);
            for (int i = 1; i < objects.Length; i++)
            {
                builder.Append(' ');
                builder.Append(ToString(objects[i]));
            }
            return builder.ToString();
        }

        private static string ToString(object value)
        {
            if (value is int)
            {
                return ToString((int) value);
            }
            if (value is long)
            {
                return ToString((long) value);
            }
            if (value is float)
            {
                return ToString((float) value);
            }
            if (value is double)
            {
                return ToString((double) value);
            }
            if (value is decimal)
            {
                return ToString((decimal) value);
            }
            if (value is TimeSpan)
            {
                return ToString((TimeSpan) value);
            }
            if (value is UniqueId)
            {
                return ToString((UniqueId) value);
            }
            if (value is Guid)
            {
                return ToString((Guid) value);
            }
            if (value is ulong)
            {
                return ToString((ulong) value);
            }
            if (value is DateTime)
            {
                return ToString((DateTime) value);
            }
            if (value is bool)
            {
                return ToString((bool) value);
            }
            return value.ToString();
        }

        public static string ToString(byte[] buffer, int offset, int count)
        {
            string str;
            try
            {
                str = UTF8Encoding.GetString(buffer, offset, count);
            }
            catch (DecoderFallbackException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateEncodingException(buffer, offset, count, exception));
            }
            return str;
        }

        public static string ToStringUnicode(byte[] buffer, int offset, int count)
        {
            string str;
            try
            {
                str = UnicodeEncoding.GetString(buffer, offset, count);
            }
            catch (DecoderFallbackException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateEncodingException(buffer, offset, count, exception));
            }
            return str;
        }

        public static TimeSpan ToTimeSpan(string value)
        {
            TimeSpan span;
            try
            {
                span = XmlConvert.ToTimeSpan(value);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "TimeSpan", exception));
            }
            catch (FormatException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "TimeSpan", exception2));
            }
            catch (OverflowException exception3)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "TimeSpan", exception3));
            }
            return span;
        }

        public static TimeSpan ToTimeSpan(byte[] buffer, int offset, int count)
        {
            return ToTimeSpan(ToString(buffer, offset, count));
        }

        public static ulong ToUInt64(string value)
        {
            ulong num;
            try
            {
                num = ulong.Parse(value, NumberFormatInfo.InvariantInfo);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "UInt64", exception));
            }
            catch (FormatException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "UInt64", exception2));
            }
            catch (OverflowException exception3)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "UInt64", exception3));
            }
            return num;
        }

        public static ulong ToUInt64(byte[] buffer, int offset, int count)
        {
            return ToUInt64(ToString(buffer, offset, count));
        }

        public static UniqueId ToUniqueId(string value)
        {
            UniqueId id;
            try
            {
                id = new UniqueId(Trim(value));
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "UniqueId", exception));
            }
            catch (FormatException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value, "UniqueId", exception2));
            }
            return id;
        }

        public static UniqueId ToUniqueId(byte[] buffer, int offset, int count)
        {
            return ToUniqueId(ToString(buffer, offset, count));
        }

        private static int ToZero(bool isNegative, byte[] buffer, int offset)
        {
            if (isNegative)
            {
                buffer[offset] = 0x2d;
                buffer[offset + 1] = 0x30;
                return 2;
            }
            buffer[offset] = 0x30;
            return 1;
        }

        private static string Trim(string s)
        {
            int startIndex = 0;
            while ((startIndex < s.Length) && IsWhitespace(s[startIndex]))
            {
                startIndex++;
            }
            int length = s.Length;
            while ((length > 0) && IsWhitespace(s[length - 1]))
            {
                length--;
            }
            if ((startIndex == 0) && (length == s.Length))
            {
                return s;
            }
            if (length == 0)
            {
                return string.Empty;
            }
            return s.Substring(startIndex, length - startIndex);
        }

        private static bool TryParseDateTime(byte[] chars, int offset, int count, out DateTime result)
        {
            DateTime time;
            int num = offset + count;
            result = DateTime.MaxValue;
            if (count < 0x13)
            {
                return false;
            }
            if (((chars[offset + 4] != 0x2d) || (chars[offset + 7] != 0x2d)) || (((chars[offset + 10] != 0x54) || (chars[offset + 13] != 0x3a)) || (chars[offset + 0x10] != 0x3a)))
            {
                return false;
            }
            int year = ToInt32D4(chars, offset, 4);
            int month = ToInt32D2(chars, offset + 5);
            int day = ToInt32D2(chars, offset + 8);
            int hour = ToInt32D2(chars, offset + 11);
            int minute = ToInt32D2(chars, offset + 14);
            int second = ToInt32D2(chars, offset + 0x11);
            if ((((((year | month) | day) | hour) | minute) | second) < 0)
            {
                return false;
            }
            DateTimeKind unspecified = DateTimeKind.Unspecified;
            offset += 0x13;
            int num8 = 0;
            if ((offset < num) && (chars[offset] == 0x2e))
            {
                offset++;
                int num9 = offset;
                while (offset < num)
                {
                    byte num10 = chars[offset];
                    if ((num10 < 0x30) || (num10 > 0x39))
                    {
                        break;
                    }
                    offset++;
                }
                int num11 = offset - num9;
                if ((num11 < 1) || (num11 > 7))
                {
                    return false;
                }
                num8 = ToInt32D7(chars, num9, num11);
                if (num8 < 0)
                {
                    return false;
                }
                for (int i = num11; i < 7; i++)
                {
                    num8 *= 10;
                }
            }
            bool flag = false;
            int hours = 0;
            int minutes = 0;
            if (offset < num)
            {
                byte num15 = chars[offset];
                switch (num15)
                {
                    case 90:
                        offset++;
                        unspecified = DateTimeKind.Utc;
                        break;

                    case 0x2b:
                    case 0x2d:
                        offset++;
                        if (((offset + 5) > num) || (chars[offset + 2] != 0x3a))
                        {
                            return false;
                        }
                        unspecified = DateTimeKind.Utc;
                        flag = true;
                        hours = ToInt32D2(chars, offset);
                        minutes = ToInt32D2(chars, offset + 3);
                        if ((hours | minutes) < 0)
                        {
                            return false;
                        }
                        if (num15 == 0x2b)
                        {
                            hours = -hours;
                            minutes = -minutes;
                        }
                        offset += 5;
                        break;
                }
            }
            if (offset < num)
            {
                return false;
            }
            try
            {
                time = new DateTime(year, month, day, hour, minute, second, unspecified);
            }
            catch (ArgumentException)
            {
                return false;
            }
            if (num8 > 0)
            {
                time = time.AddTicks((long) num8);
            }
            if (flag)
            {
                try
                {
                    TimeSpan span = new TimeSpan(hours, minutes, 0);
                    if (((hours >= 0) && (time < (DateTime.MaxValue - span))) || ((hours < 0) && (time > (DateTime.MinValue - span))))
                    {
                        time = time.Add(span).ToLocalTime();
                    }
                    else
                    {
                        time = time.ToLocalTime().Add(span);
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    return false;
                }
            }
            result = time;
            return true;
        }

        private static bool TryParseDouble(byte[] chars, int offset, int count, out double result)
        {
            result = 0.0;
            int num = offset + count;
            bool flag = false;
            if ((offset < num) && (chars[offset] == 0x2d))
            {
                flag = true;
                offset++;
                count--;
            }
            if ((count < 1) || (count > 10))
            {
                return false;
            }
            int num2 = 0;
            while (offset < num)
            {
                int num3 = chars[offset] - 0x30;
                if (num3 == -2)
                {
                    offset++;
                    int num4 = 1;
                    while (offset < num)
                    {
                        num3 = chars[offset] - 0x30;
                        if (num3 >= 10)
                        {
                            return false;
                        }
                        num4 *= 10;
                        num2 = (num2 * 10) + num3;
                        offset++;
                    }
                    if (flag)
                    {
                        result = -((double) num2) / ((double) num4);
                    }
                    else
                    {
                        result = ((double) num2) / ((double) num4);
                    }
                    return true;
                }
                if (num3 >= 10)
                {
                    return false;
                }
                num2 = (num2 * 10) + num3;
                offset++;
            }
            if (count == 10)
            {
                return false;
            }
            if (flag)
            {
                result = -num2;
            }
            else
            {
                result = num2;
            }
            return true;
        }

        private static bool TryParseInt32(byte[] chars, int offset, int count, out int result)
        {
            result = 0;
            if (count == 0)
            {
                return false;
            }
            int num = 0;
            int num2 = offset + count;
            if (chars[offset] == 0x2d)
            {
                if (count == 1)
                {
                    return false;
                }
                for (int i = offset + 1; i < num2; i++)
                {
                    int num4 = chars[i] - 0x30;
                    if (num4 > 9)
                    {
                        return false;
                    }
                    if (num < -214748364)
                    {
                        return false;
                    }
                    num *= 10;
                    if (num < (-2147483648 + num4))
                    {
                        return false;
                    }
                    num -= num4;
                }
            }
            else
            {
                for (int j = offset; j < num2; j++)
                {
                    int num6 = chars[j] - 0x30;
                    if (num6 > 9)
                    {
                        return false;
                    }
                    if (num > 0xccccccc)
                    {
                        return false;
                    }
                    num *= 10;
                    if (num > (0x7fffffff - num6))
                    {
                        return false;
                    }
                    num += num6;
                }
            }
            result = num;
            return true;
        }

        private static bool TryParseInt64(byte[] chars, int offset, int count, out long result)
        {
            result = 0L;
            if (count < 11)
            {
                int num;
                if (!TryParseInt32(chars, offset, count, out num))
                {
                    return false;
                }
                result = num;
                return true;
            }
            long num2 = 0L;
            int num3 = offset + count;
            if (chars[offset] == 0x2d)
            {
                if (count == 1)
                {
                    return false;
                }
                for (int i = offset + 1; i < num3; i++)
                {
                    int num5 = chars[i] - 0x30;
                    if (num5 > 9)
                    {
                        return false;
                    }
                    if (num2 < -922337203685477580L)
                    {
                        return false;
                    }
                    num2 *= 10L;
                    if (num2 < (-9223372036854775808L + num5))
                    {
                        return false;
                    }
                    num2 -= num5;
                }
            }
            else
            {
                for (int j = offset; j < num3; j++)
                {
                    int num7 = chars[j] - 0x30;
                    if (num7 > 9)
                    {
                        return false;
                    }
                    if (num2 > 0xcccccccccccccccL)
                    {
                        return false;
                    }
                    num2 *= 10L;
                    if (num2 > (0x7fffffffffffffffL - num7))
                    {
                        return false;
                    }
                    num2 += num7;
                }
            }
            result = num2;
            return true;
        }

        private static bool TryParseSingle(byte[] chars, int offset, int count, out float result)
        {
            result = 0f;
            int num = offset + count;
            bool flag = false;
            if ((offset < num) && (chars[offset] == 0x2d))
            {
                flag = true;
                offset++;
                count--;
            }
            if ((count < 1) || (count > 10))
            {
                return false;
            }
            int num2 = 0;
            while (offset < num)
            {
                int num3 = chars[offset] - 0x30;
                if (num3 == -2)
                {
                    offset++;
                    int num4 = 1;
                    while (offset < num)
                    {
                        num3 = chars[offset] - 0x30;
                        if (num3 >= 10)
                        {
                            return false;
                        }
                        num4 *= 10;
                        num2 = (num2 * 10) + num3;
                        offset++;
                    }
                    if (count > 8)
                    {
                        result = (float) (((double) num2) / ((double) num4));
                    }
                    else
                    {
                        result = ((float) num2) / ((float) num4);
                    }
                    if (flag)
                    {
                        result = -result;
                    }
                    return true;
                }
                if (num3 >= 10)
                {
                    return false;
                }
                num2 = (num2 * 10) + num3;
                offset++;
            }
            if (count == 10)
            {
                return false;
            }
            if (flag)
            {
                result = -num2;
            }
            else
            {
                result = num2;
            }
            return true;
        }

        public static System.Text.Base64Encoding Base64Encoding
        {
            get
            {
                if (base64Encoding == null)
                {
                    base64Encoding = new System.Text.Base64Encoding();
                }
                return base64Encoding;
            }
        }

        private static System.Text.UnicodeEncoding UnicodeEncoding
        {
            get
            {
                if (unicodeEncoding == null)
                {
                    unicodeEncoding = new System.Text.UnicodeEncoding(false, false, true);
                }
                return unicodeEncoding;
            }
        }

        private static System.Text.UTF8Encoding UTF8Encoding
        {
            get
            {
                if (utf8Encoding == null)
                {
                    utf8Encoding = new System.Text.UTF8Encoding(false, true);
                }
                return utf8Encoding;
            }
        }
    }
}

