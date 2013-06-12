namespace System
{
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;

    public static class Convert
    {
        internal static readonly char[] base64Table = new char[] { 
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 
            'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 
            'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 
            'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '+', '/', 
            '='
         };
        internal static readonly RuntimeType[] ConvertTypes = new RuntimeType[] { 
            ((RuntimeType) typeof(Empty)), ((RuntimeType) typeof(object)), ((RuntimeType) typeof(System.DBNull)), ((RuntimeType) typeof(bool)), ((RuntimeType) typeof(char)), ((RuntimeType) typeof(sbyte)), ((RuntimeType) typeof(byte)), ((RuntimeType) typeof(short)), ((RuntimeType) typeof(ushort)), ((RuntimeType) typeof(int)), ((RuntimeType) typeof(uint)), ((RuntimeType) typeof(long)), ((RuntimeType) typeof(ulong)), ((RuntimeType) typeof(float)), ((RuntimeType) typeof(double)), ((RuntimeType) typeof(decimal)), 
            ((RuntimeType) typeof(DateTime)), ((RuntimeType) typeof(object)), ((RuntimeType) typeof(string))
         };
        public static readonly object DBNull = System.DBNull.Value;
        private static readonly RuntimeType EnumType = ((RuntimeType) typeof(Enum));

        private static int CalculateOutputLength(int inputLength, bool insertLineBreaks)
        {
            int num = (inputLength / 3) * 4;
            num += ((inputLength % 3) != 0) ? 4 : 0;
            if (num == 0)
            {
                return num;
            }
            if (!insertLineBreaks)
            {
                return num;
            }
            int num2 = num / 0x4c;
            if ((num % 0x4c) == 0)
            {
                num2--;
            }
            return (num + (num2 * 2));
        }

        public static object ChangeType(object value, Type conversionType)
        {
            return ChangeType(value, conversionType, Thread.CurrentThread.CurrentCulture);
        }

        public static object ChangeType(object value, TypeCode typeCode)
        {
            return ChangeType(value, typeCode, Thread.CurrentThread.CurrentCulture);
        }

        public static object ChangeType(object value, Type conversionType, IFormatProvider provider)
        {
            if (conversionType == null)
            {
                throw new ArgumentNullException("conversionType");
            }
            if (value == null)
            {
                if (conversionType.IsValueType)
                {
                    throw new InvalidCastException(Environment.GetResourceString("InvalidCast_CannotCastNullToValueType"));
                }
                return null;
            }
            IConvertible convertible = value as IConvertible;
            if (convertible == null)
            {
                if (value.GetType() != conversionType)
                {
                    throw new InvalidCastException(Environment.GetResourceString("InvalidCast_IConvertible"));
                }
                return value;
            }
            RuntimeType type = conversionType as RuntimeType;
            if (type == ConvertTypes[3])
            {
                return convertible.ToBoolean(provider);
            }
            if (type == ConvertTypes[4])
            {
                return convertible.ToChar(provider);
            }
            if (type == ConvertTypes[5])
            {
                return convertible.ToSByte(provider);
            }
            if (type == ConvertTypes[6])
            {
                return convertible.ToByte(provider);
            }
            if (type == ConvertTypes[7])
            {
                return convertible.ToInt16(provider);
            }
            if (type == ConvertTypes[8])
            {
                return convertible.ToUInt16(provider);
            }
            if (type == ConvertTypes[9])
            {
                return convertible.ToInt32(provider);
            }
            if (type == ConvertTypes[10])
            {
                return convertible.ToUInt32(provider);
            }
            if (type == ConvertTypes[11])
            {
                return convertible.ToInt64(provider);
            }
            if (type == ConvertTypes[12])
            {
                return convertible.ToUInt64(provider);
            }
            if (type == ConvertTypes[13])
            {
                return convertible.ToSingle(provider);
            }
            if (type == ConvertTypes[14])
            {
                return convertible.ToDouble(provider);
            }
            if (type == ConvertTypes[15])
            {
                return convertible.ToDecimal(provider);
            }
            if (type == ConvertTypes[0x10])
            {
                return convertible.ToDateTime(provider);
            }
            if (type == ConvertTypes[0x12])
            {
                return convertible.ToString(provider);
            }
            if (type == ConvertTypes[1])
            {
                return value;
            }
            return convertible.ToType(conversionType, provider);
        }

        public static object ChangeType(object value, TypeCode typeCode, IFormatProvider provider)
        {
            if ((value == null) && (((typeCode == TypeCode.Empty) || (typeCode == TypeCode.String)) || (typeCode == TypeCode.Object)))
            {
                return null;
            }
            IConvertible convertible = value as IConvertible;
            if (convertible == null)
            {
                throw new InvalidCastException(Environment.GetResourceString("InvalidCast_IConvertible"));
            }
            switch (typeCode)
            {
                case TypeCode.Empty:
                    throw new InvalidCastException(Environment.GetResourceString("InvalidCast_Empty"));

                case TypeCode.Object:
                    return value;

                case TypeCode.DBNull:
                    throw new InvalidCastException(Environment.GetResourceString("InvalidCast_DBNull"));

                case TypeCode.Boolean:
                    return convertible.ToBoolean(provider);

                case TypeCode.Char:
                    return convertible.ToChar(provider);

                case TypeCode.SByte:
                    return convertible.ToSByte(provider);

                case TypeCode.Byte:
                    return convertible.ToByte(provider);

                case TypeCode.Int16:
                    return convertible.ToInt16(provider);

                case TypeCode.UInt16:
                    return convertible.ToUInt16(provider);

                case TypeCode.Int32:
                    return convertible.ToInt32(provider);

                case TypeCode.UInt32:
                    return convertible.ToUInt32(provider);

                case TypeCode.Int64:
                    return convertible.ToInt64(provider);

                case TypeCode.UInt64:
                    return convertible.ToUInt64(provider);

                case TypeCode.Single:
                    return convertible.ToSingle(provider);

                case TypeCode.Double:
                    return convertible.ToDouble(provider);

                case TypeCode.Decimal:
                    return convertible.ToDecimal(provider);

                case TypeCode.DateTime:
                    return convertible.ToDateTime(provider);

                case TypeCode.String:
                    return convertible.ToString(provider);
            }
            throw new ArgumentException(Environment.GetResourceString("Arg_UnknownTypeCode"));
        }

        [SecurityCritical]
        private static unsafe int ConvertToBase64Array(char* outChars, byte* inData, int offset, int length, bool insertLineBreaks)
        {
            int num = length % 3;
            int num2 = offset + (length - num);
            int index = 0;
            int num4 = 0;
            fixed (char* chRef = base64Table)
            {
                int num5;
                for (num5 = offset; num5 < num2; num5 += 3)
                {
                    if (insertLineBreaks)
                    {
                        if (num4 == 0x4c)
                        {
                            outChars[index++] = '\r';
                            outChars[index++] = '\n';
                            num4 = 0;
                        }
                        num4 += 4;
                    }
                    outChars[index] = chRef[(inData[num5] & 0xfc) >> 2];
                    outChars[index + 1] = chRef[((inData[num5] & 3) << 4) | ((inData[num5 + 1] & 240) >> 4)];
                    outChars[index + 2] = chRef[((inData[num5 + 1] & 15) << 2) | ((inData[num5 + 2] & 0xc0) >> 6)];
                    outChars[index + 3] = chRef[inData[num5 + 2] & 0x3f];
                    index += 4;
                }
                num5 = num2;
                if ((insertLineBreaks && (num != 0)) && (num4 == 0x4c))
                {
                    outChars[index++] = '\r';
                    outChars[index++] = '\n';
                }
                switch (num)
                {
                    case 1:
                        outChars[index] = chRef[(inData[num5] & 0xfc) >> 2];
                        outChars[index + 1] = chRef[(inData[num5] & 3) << 4];
                        outChars[index + 2] = chRef[0x40];
                        outChars[index + 3] = chRef[0x40];
                        index += 4;
                        break;

                    case 2:
                        outChars[index] = chRef[(inData[num5] & 0xfc) >> 2];
                        outChars[index + 1] = chRef[((inData[num5] & 3) << 4) | ((inData[num5 + 1] & 240) >> 4)];
                        outChars[index + 2] = chRef[(inData[num5 + 1] & 15) << 2];
                        outChars[index + 3] = chRef[0x40];
                        index += 4;
                        break;
                }
            }
            return index;
        }

        internal static object DefaultToType(IConvertible value, Type targetType, IFormatProvider provider)
        {
            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }
            RuntimeType type = targetType as RuntimeType;
            if (type != null)
            {
                if (value.GetType() == targetType)
                {
                    return value;
                }
                if (type == ConvertTypes[3])
                {
                    return value.ToBoolean(provider);
                }
                if (type == ConvertTypes[4])
                {
                    return value.ToChar(provider);
                }
                if (type == ConvertTypes[5])
                {
                    return value.ToSByte(provider);
                }
                if (type == ConvertTypes[6])
                {
                    return value.ToByte(provider);
                }
                if (type == ConvertTypes[7])
                {
                    return value.ToInt16(provider);
                }
                if (type == ConvertTypes[8])
                {
                    return value.ToUInt16(provider);
                }
                if (type == ConvertTypes[9])
                {
                    return value.ToInt32(provider);
                }
                if (type == ConvertTypes[10])
                {
                    return value.ToUInt32(provider);
                }
                if (type == ConvertTypes[11])
                {
                    return value.ToInt64(provider);
                }
                if (type == ConvertTypes[12])
                {
                    return value.ToUInt64(provider);
                }
                if (type == ConvertTypes[13])
                {
                    return value.ToSingle(provider);
                }
                if (type == ConvertTypes[14])
                {
                    return value.ToDouble(provider);
                }
                if (type == ConvertTypes[15])
                {
                    return value.ToDecimal(provider);
                }
                if (type == ConvertTypes[0x10])
                {
                    return value.ToDateTime(provider);
                }
                if (type == ConvertTypes[0x12])
                {
                    return value.ToString(provider);
                }
                if (type == ConvertTypes[1])
                {
                    return value;
                }
                if (type == EnumType)
                {
                    return (Enum) value;
                }
                if (type == ConvertTypes[2])
                {
                    throw new InvalidCastException(Environment.GetResourceString("InvalidCast_DBNull"));
                }
                if (type == ConvertTypes[0])
                {
                    throw new InvalidCastException(Environment.GetResourceString("InvalidCast_Empty"));
                }
            }
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", new object[] { value.GetType().FullName, targetType.FullName }));
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern byte[] FromBase64CharArray(char[] inArray, int offset, int length);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern byte[] FromBase64String(string s);
        public static TypeCode GetTypeCode(object value)
        {
            if (value == null)
            {
                return TypeCode.Empty;
            }
            IConvertible convertible = value as IConvertible;
            if (convertible != null)
            {
                return convertible.GetTypeCode();
            }
            return TypeCode.Object;
        }

        public static bool IsDBNull(object value)
        {
            if (value == System.DBNull.Value)
            {
                return true;
            }
            IConvertible convertible = value as IConvertible;
            if (convertible == null)
            {
                return false;
            }
            return (convertible.GetTypeCode() == TypeCode.DBNull);
        }

        public static int ToBase64CharArray(byte[] inArray, int offsetIn, int length, char[] outArray, int offsetOut)
        {
            return ToBase64CharArray(inArray, offsetIn, length, outArray, offsetOut, Base64FormattingOptions.None);
        }

        [SecuritySafeCritical, ComVisible(false)]
        public static unsafe int ToBase64CharArray(byte[] inArray, int offsetIn, int length, char[] outArray, int offsetOut, Base64FormattingOptions options)
        {
            if (inArray == null)
            {
                throw new ArgumentNullException("inArray");
            }
            if (outArray == null)
            {
                throw new ArgumentNullException("outArray");
            }
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if (offsetIn < 0)
            {
                throw new ArgumentOutOfRangeException("offsetIn", Environment.GetResourceString("ArgumentOutOfRange_GenericPositive"));
            }
            if (offsetOut < 0)
            {
                throw new ArgumentOutOfRangeException("offsetOut", Environment.GetResourceString("ArgumentOutOfRange_GenericPositive"));
            }
            if ((options < Base64FormattingOptions.None) || (options > Base64FormattingOptions.InsertLineBreaks))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", new object[] { (int) options }));
            }
            int num2 = inArray.Length;
            if (offsetIn > (num2 - length))
            {
                throw new ArgumentOutOfRangeException("offsetIn", Environment.GetResourceString("ArgumentOutOfRange_OffsetLength"));
            }
            if (num2 == 0)
            {
                return 0;
            }
            bool insertLineBreaks = options == Base64FormattingOptions.InsertLineBreaks;
            int num3 = outArray.Length;
            int num4 = CalculateOutputLength(length, insertLineBreaks);
            if (offsetOut > (num3 - num4))
            {
                throw new ArgumentOutOfRangeException("offsetOut", Environment.GetResourceString("ArgumentOutOfRange_OffsetOut"));
            }
            fixed (char* chRef = &(outArray[offsetOut]))
            {
                int num;
                fixed (byte* numRef = inArray)
                {
                    num = ConvertToBase64Array(chRef, numRef, offsetIn, length, insertLineBreaks);
                    chRef = null;
                }
                return num;
            }
        }

        public static string ToBase64String(byte[] inArray)
        {
            if (inArray == null)
            {
                throw new ArgumentNullException("inArray");
            }
            return ToBase64String(inArray, 0, inArray.Length, Base64FormattingOptions.None);
        }

        [ComVisible(false)]
        public static string ToBase64String(byte[] inArray, Base64FormattingOptions options)
        {
            if (inArray == null)
            {
                throw new ArgumentNullException("inArray");
            }
            return ToBase64String(inArray, 0, inArray.Length, options);
        }

        public static string ToBase64String(byte[] inArray, int offset, int length)
        {
            return ToBase64String(inArray, offset, length, Base64FormattingOptions.None);
        }

        [ComVisible(false), SecuritySafeCritical]
        public static unsafe string ToBase64String(byte[] inArray, int offset, int length, Base64FormattingOptions options)
        {
            if (inArray == null)
            {
                throw new ArgumentNullException("inArray");
            }
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_GenericPositive"));
            }
            if ((options < Base64FormattingOptions.None) || (options > Base64FormattingOptions.InsertLineBreaks))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", new object[] { (int) options }));
            }
            int num = inArray.Length;
            if (offset > (num - length))
            {
                throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_OffsetLength"));
            }
            if (num == 0)
            {
                return string.Empty;
            }
            bool insertLineBreaks = options == Base64FormattingOptions.InsertLineBreaks;
            string str = string.FastAllocateString(CalculateOutputLength(length, insertLineBreaks));
            fixed (char* str3 = ((char*) str))
            {
                char* outChars = str3;
                fixed (byte* numRef = inArray)
                {
                    ConvertToBase64Array(outChars, numRef, offset, length, insertLineBreaks);
                    return str;
                }
            }
        }

        public static bool ToBoolean(bool value)
        {
            return value;
        }

        public static bool ToBoolean(byte value)
        {
            return (value != 0);
        }

        public static bool ToBoolean(char value)
        {
            return ((IConvertible) value).ToBoolean(null);
        }

        public static bool ToBoolean(DateTime value)
        {
            return ((IConvertible) value).ToBoolean(null);
        }

        public static bool ToBoolean(decimal value)
        {
            return (value != 0M);
        }

        public static bool ToBoolean(double value)
        {
            return !(value == 0.0);
        }

        public static bool ToBoolean(short value)
        {
            return (value != 0);
        }

        public static bool ToBoolean(int value)
        {
            return (value != 0);
        }

        public static bool ToBoolean(long value)
        {
            return (value != 0L);
        }

        public static bool ToBoolean(object value)
        {
            return ((value != null) && ((IConvertible) value).ToBoolean(null));
        }

        [CLSCompliant(false)]
        public static bool ToBoolean(sbyte value)
        {
            return (value != 0);
        }

        public static bool ToBoolean(float value)
        {
            return !(value == 0f);
        }

        public static bool ToBoolean(string value)
        {
            if (value == null)
            {
                return false;
            }
            return bool.Parse(value);
        }

        [CLSCompliant(false)]
        public static bool ToBoolean(ushort value)
        {
            return (value != 0);
        }

        [CLSCompliant(false)]
        public static bool ToBoolean(uint value)
        {
            return (value != 0);
        }

        [CLSCompliant(false)]
        public static bool ToBoolean(ulong value)
        {
            return (value != 0L);
        }

        public static bool ToBoolean(object value, IFormatProvider provider)
        {
            return ((value != null) && ((IConvertible) value).ToBoolean(provider));
        }

        public static bool ToBoolean(string value, IFormatProvider provider)
        {
            if (value == null)
            {
                return false;
            }
            return bool.Parse(value);
        }

        public static byte ToByte(bool value)
        {
            if (!value)
            {
                return 0;
            }
            return 1;
        }

        public static byte ToByte(byte value)
        {
            return value;
        }

        public static byte ToByte(char value)
        {
            if (value > '\x00ff')
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Byte"));
            }
            return (byte) value;
        }

        public static byte ToByte(DateTime value)
        {
            return ((IConvertible) value).ToByte(null);
        }

        public static byte ToByte(decimal value)
        {
            return decimal.ToByte(decimal.Round(value, 0));
        }

        public static byte ToByte(double value)
        {
            return ToByte(ToInt32(value));
        }

        public static byte ToByte(short value)
        {
            if ((value < 0) || (value > 0xff))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Byte"));
            }
            return (byte) value;
        }

        public static byte ToByte(int value)
        {
            if ((value < 0) || (value > 0xff))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Byte"));
            }
            return (byte) value;
        }

        public static byte ToByte(long value)
        {
            if ((value < 0L) || (value > 0xffL))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Byte"));
            }
            return (byte) value;
        }

        public static byte ToByte(object value)
        {
            if (value != null)
            {
                return ((IConvertible) value).ToByte(null);
            }
            return 0;
        }

        [CLSCompliant(false)]
        public static byte ToByte(sbyte value)
        {
            if (value < 0)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Byte"));
            }
            return (byte) value;
        }

        public static byte ToByte(float value)
        {
            return ToByte((double) value);
        }

        public static byte ToByte(string value)
        {
            if (value == null)
            {
                return 0;
            }
            return byte.Parse(value, CultureInfo.CurrentCulture);
        }

        [CLSCompliant(false)]
        public static byte ToByte(ushort value)
        {
            if (value > 0xff)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Byte"));
            }
            return (byte) value;
        }

        [CLSCompliant(false)]
        public static byte ToByte(uint value)
        {
            if (value > 0xff)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Byte"));
            }
            return (byte) value;
        }

        [CLSCompliant(false)]
        public static byte ToByte(ulong value)
        {
            if (value > 0xffL)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Byte"));
            }
            return (byte) value;
        }

        public static byte ToByte(object value, IFormatProvider provider)
        {
            if (value != null)
            {
                return ((IConvertible) value).ToByte(provider);
            }
            return 0;
        }

        public static byte ToByte(string value, IFormatProvider provider)
        {
            if (value == null)
            {
                return 0;
            }
            return byte.Parse(value, NumberStyles.Integer, provider);
        }

        public static byte ToByte(string value, int fromBase)
        {
            if (((fromBase != 2) && (fromBase != 8)) && ((fromBase != 10) && (fromBase != 0x10)))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_InvalidBase"));
            }
            int num = ParseNumbers.StringToInt(value, fromBase, 0x1200);
            if ((num < 0) || (num > 0xff))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Byte"));
            }
            return (byte) num;
        }

        public static char ToChar(bool value)
        {
            return ((IConvertible) value).ToChar(null);
        }

        public static char ToChar(byte value)
        {
            return (char) value;
        }

        public static char ToChar(char value)
        {
            return value;
        }

        public static char ToChar(DateTime value)
        {
            return ((IConvertible) value).ToChar(null);
        }

        public static char ToChar(decimal value)
        {
            return ((IConvertible) value).ToChar(null);
        }

        public static char ToChar(double value)
        {
            return ((IConvertible) value).ToChar(null);
        }

        public static char ToChar(short value)
        {
            if (value < 0)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Char"));
            }
            return (char) ((ushort) value);
        }

        public static char ToChar(int value)
        {
            if ((value < 0) || (value > 0xffff))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Char"));
            }
            return (char) value;
        }

        public static char ToChar(long value)
        {
            if ((value < 0L) || (value > 0xffffL))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Char"));
            }
            return (char) ((ushort) value);
        }

        public static char ToChar(object value)
        {
            if (value != null)
            {
                return ((IConvertible) value).ToChar(null);
            }
            return '\0';
        }

        [CLSCompliant(false)]
        public static char ToChar(sbyte value)
        {
            if (value < 0)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Char"));
            }
            return (char) ((ushort) value);
        }

        public static char ToChar(float value)
        {
            return ((IConvertible) value).ToChar(null);
        }

        public static char ToChar(string value)
        {
            return ToChar(value, null);
        }

        [CLSCompliant(false)]
        public static char ToChar(ushort value)
        {
            return (char) value;
        }

        [CLSCompliant(false)]
        public static char ToChar(uint value)
        {
            if (value > 0xffff)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Char"));
            }
            return (char) value;
        }

        [CLSCompliant(false)]
        public static char ToChar(ulong value)
        {
            if (value > 0xffffL)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Char"));
            }
            return (char) value;
        }

        public static char ToChar(object value, IFormatProvider provider)
        {
            if (value != null)
            {
                return ((IConvertible) value).ToChar(provider);
            }
            return '\0';
        }

        public static char ToChar(string value, IFormatProvider provider)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (value.Length != 1)
            {
                throw new FormatException(Environment.GetResourceString("Format_NeedSingleChar"));
            }
            return value[0];
        }

        public static DateTime ToDateTime(bool value)
        {
            return ((IConvertible) value).ToDateTime(null);
        }

        public static DateTime ToDateTime(byte value)
        {
            return ((IConvertible) value).ToDateTime(null);
        }

        public static DateTime ToDateTime(char value)
        {
            return ((IConvertible) value).ToDateTime(null);
        }

        public static DateTime ToDateTime(DateTime value)
        {
            return value;
        }

        public static DateTime ToDateTime(decimal value)
        {
            return ((IConvertible) value).ToDateTime(null);
        }

        public static DateTime ToDateTime(double value)
        {
            return ((IConvertible) value).ToDateTime(null);
        }

        public static DateTime ToDateTime(short value)
        {
            return ((IConvertible) value).ToDateTime(null);
        }

        public static DateTime ToDateTime(int value)
        {
            return ((IConvertible) value).ToDateTime(null);
        }

        public static DateTime ToDateTime(long value)
        {
            return ((IConvertible) value).ToDateTime(null);
        }

        public static DateTime ToDateTime(object value)
        {
            if (value != null)
            {
                return ((IConvertible) value).ToDateTime(null);
            }
            return DateTime.MinValue;
        }

        [CLSCompliant(false)]
        public static DateTime ToDateTime(sbyte value)
        {
            return ((IConvertible) value).ToDateTime(null);
        }

        public static DateTime ToDateTime(float value)
        {
            return ((IConvertible) value).ToDateTime(null);
        }

        public static DateTime ToDateTime(string value)
        {
            if (value == null)
            {
                return new DateTime(0L);
            }
            return DateTime.Parse(value, CultureInfo.CurrentCulture);
        }

        [CLSCompliant(false)]
        public static DateTime ToDateTime(ushort value)
        {
            return ((IConvertible) value).ToDateTime(null);
        }

        [CLSCompliant(false)]
        public static DateTime ToDateTime(uint value)
        {
            return ((IConvertible) value).ToDateTime(null);
        }

        [CLSCompliant(false)]
        public static DateTime ToDateTime(ulong value)
        {
            return ((IConvertible) value).ToDateTime(null);
        }

        public static DateTime ToDateTime(object value, IFormatProvider provider)
        {
            if (value != null)
            {
                return ((IConvertible) value).ToDateTime(provider);
            }
            return DateTime.MinValue;
        }

        public static DateTime ToDateTime(string value, IFormatProvider provider)
        {
            if (value == null)
            {
                return new DateTime(0L);
            }
            return DateTime.Parse(value, provider);
        }

        public static decimal ToDecimal(bool value)
        {
            return (value ? 1 : 0);
        }

        public static decimal ToDecimal(byte value)
        {
            return value;
        }

        public static decimal ToDecimal(char value)
        {
            return ((IConvertible) value).ToDecimal(null);
        }

        public static decimal ToDecimal(DateTime value)
        {
            return ((IConvertible) value).ToDecimal(null);
        }

        public static decimal ToDecimal(decimal value)
        {
            return value;
        }

        public static decimal ToDecimal(double value)
        {
            return (decimal) value;
        }

        public static decimal ToDecimal(short value)
        {
            return value;
        }

        public static decimal ToDecimal(int value)
        {
            return value;
        }

        public static decimal ToDecimal(long value)
        {
            return value;
        }

        public static decimal ToDecimal(object value)
        {
            if (value != null)
            {
                return ((IConvertible) value).ToDecimal(null);
            }
            return 0M;
        }

        [CLSCompliant(false)]
        public static decimal ToDecimal(sbyte value)
        {
            return value;
        }

        public static decimal ToDecimal(float value)
        {
            return (decimal) value;
        }

        public static decimal ToDecimal(string value)
        {
            if (value == null)
            {
                return 0M;
            }
            return decimal.Parse(value, CultureInfo.CurrentCulture);
        }

        [CLSCompliant(false)]
        public static decimal ToDecimal(ushort value)
        {
            return value;
        }

        [CLSCompliant(false)]
        public static decimal ToDecimal(uint value)
        {
            return value;
        }

        [CLSCompliant(false)]
        public static decimal ToDecimal(ulong value)
        {
            return value;
        }

        public static decimal ToDecimal(object value, IFormatProvider provider)
        {
            if (value != null)
            {
                return ((IConvertible) value).ToDecimal(provider);
            }
            return 0M;
        }

        public static decimal ToDecimal(string value, IFormatProvider provider)
        {
            if (value == null)
            {
                return 0M;
            }
            return decimal.Parse(value, NumberStyles.Number, provider);
        }

        public static double ToDouble(bool value)
        {
            return (value ? ((double) 1) : ((double) 0));
        }

        public static double ToDouble(byte value)
        {
            return (double) value;
        }

        public static double ToDouble(char value)
        {
            return ((IConvertible) value).ToDouble(null);
        }

        public static double ToDouble(DateTime value)
        {
            return ((IConvertible) value).ToDouble(null);
        }

        public static double ToDouble(decimal value)
        {
            return (double) value;
        }

        public static double ToDouble(double value)
        {
            return value;
        }

        public static double ToDouble(short value)
        {
            return (double) value;
        }

        public static double ToDouble(int value)
        {
            return (double) value;
        }

        public static double ToDouble(long value)
        {
            return (double) value;
        }

        public static double ToDouble(object value)
        {
            if (value != null)
            {
                return ((IConvertible) value).ToDouble(null);
            }
            return 0.0;
        }

        [CLSCompliant(false)]
        public static double ToDouble(sbyte value)
        {
            return (double) value;
        }

        public static double ToDouble(float value)
        {
            return (double) value;
        }

        public static double ToDouble(string value)
        {
            if (value == null)
            {
                return 0.0;
            }
            return double.Parse(value, CultureInfo.CurrentCulture);
        }

        [CLSCompliant(false)]
        public static double ToDouble(ushort value)
        {
            return (double) value;
        }

        [CLSCompliant(false)]
        public static double ToDouble(uint value)
        {
            return (double) value;
        }

        [CLSCompliant(false)]
        public static double ToDouble(ulong value)
        {
            return (double) value;
        }

        public static double ToDouble(object value, IFormatProvider provider)
        {
            if (value != null)
            {
                return ((IConvertible) value).ToDouble(provider);
            }
            return 0.0;
        }

        public static double ToDouble(string value, IFormatProvider provider)
        {
            if (value == null)
            {
                return 0.0;
            }
            return double.Parse(value, NumberStyles.Float | NumberStyles.AllowThousands, provider);
        }

        public static short ToInt16(bool value)
        {
            if (!value)
            {
                return 0;
            }
            return 1;
        }

        public static short ToInt16(byte value)
        {
            return value;
        }

        public static short ToInt16(char value)
        {
            if (value > '翿')
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Int16"));
            }
            return (short) value;
        }

        public static short ToInt16(DateTime value)
        {
            return ((IConvertible) value).ToInt16(null);
        }

        public static short ToInt16(decimal value)
        {
            return decimal.ToInt16(decimal.Round(value, 0));
        }

        public static short ToInt16(double value)
        {
            return ToInt16(ToInt32(value));
        }

        public static short ToInt16(short value)
        {
            return value;
        }

        public static short ToInt16(int value)
        {
            if ((value < -32768) || (value > 0x7fff))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Int16"));
            }
            return (short) value;
        }

        public static short ToInt16(long value)
        {
            if ((value < -32768L) || (value > 0x7fffL))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Int16"));
            }
            return (short) value;
        }

        public static short ToInt16(object value)
        {
            if (value != null)
            {
                return ((IConvertible) value).ToInt16(null);
            }
            return 0;
        }

        [CLSCompliant(false)]
        public static short ToInt16(sbyte value)
        {
            return value;
        }

        public static short ToInt16(float value)
        {
            return ToInt16((double) value);
        }

        public static short ToInt16(string value)
        {
            if (value == null)
            {
                return 0;
            }
            return short.Parse(value, CultureInfo.CurrentCulture);
        }

        [CLSCompliant(false)]
        public static short ToInt16(ushort value)
        {
            if (value > 0x7fff)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Int16"));
            }
            return (short) value;
        }

        [CLSCompliant(false)]
        public static short ToInt16(uint value)
        {
            if (value > 0x7fffL)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Int16"));
            }
            return (short) value;
        }

        [CLSCompliant(false)]
        public static short ToInt16(ulong value)
        {
            if (value > 0x7fffL)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Int16"));
            }
            return (short) value;
        }

        public static short ToInt16(object value, IFormatProvider provider)
        {
            if (value != null)
            {
                return ((IConvertible) value).ToInt16(provider);
            }
            return 0;
        }

        public static short ToInt16(string value, IFormatProvider provider)
        {
            if (value == null)
            {
                return 0;
            }
            return short.Parse(value, NumberStyles.Integer, provider);
        }

        public static short ToInt16(string value, int fromBase)
        {
            if (((fromBase != 2) && (fromBase != 8)) && ((fromBase != 10) && (fromBase != 0x10)))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_InvalidBase"));
            }
            int num = ParseNumbers.StringToInt(value, fromBase, 0x1800);
            if (((fromBase == 10) || (num > 0xffff)) && ((num < -32768) || (num > 0x7fff)))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Int16"));
            }
            return (short) num;
        }

        public static int ToInt32(bool value)
        {
            if (!value)
            {
                return 0;
            }
            return 1;
        }

        public static int ToInt32(byte value)
        {
            return value;
        }

        public static int ToInt32(char value)
        {
            return value;
        }

        public static int ToInt32(DateTime value)
        {
            return ((IConvertible) value).ToInt32(null);
        }

        [SecuritySafeCritical]
        public static int ToInt32(decimal value)
        {
            return decimal.FCallToInt32(value);
        }

        public static int ToInt32(double value)
        {
            if (value >= 0.0)
            {
                if (value < 2147483647.5)
                {
                    int num = (int) value;
                    double num2 = value - num;
                    if ((num2 > 0.5) || ((num2 == 0.5) && ((num & 1) != 0)))
                    {
                        num++;
                    }
                    return num;
                }
            }
            else if (value >= -2147483648.5)
            {
                int num3 = (int) value;
                double num4 = value - num3;
                if ((num4 < -0.5) || ((num4 == -0.5) && ((num3 & 1) != 0)))
                {
                    num3--;
                }
                return num3;
            }
            throw new OverflowException(Environment.GetResourceString("Overflow_Int32"));
        }

        public static int ToInt32(short value)
        {
            return value;
        }

        public static int ToInt32(int value)
        {
            return value;
        }

        public static int ToInt32(long value)
        {
            if ((value < -2147483648L) || (value > 0x7fffffffL))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Int32"));
            }
            return (int) value;
        }

        public static int ToInt32(object value)
        {
            if (value != null)
            {
                return ((IConvertible) value).ToInt32(null);
            }
            return 0;
        }

        [CLSCompliant(false)]
        public static int ToInt32(sbyte value)
        {
            return value;
        }

        public static int ToInt32(float value)
        {
            return ToInt32((double) value);
        }

        public static int ToInt32(string value)
        {
            if (value == null)
            {
                return 0;
            }
            return int.Parse(value, CultureInfo.CurrentCulture);
        }

        [CLSCompliant(false)]
        public static int ToInt32(ushort value)
        {
            return value;
        }

        [CLSCompliant(false)]
        public static int ToInt32(uint value)
        {
            if (value > 0x7fffffff)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Int32"));
            }
            return (int) value;
        }

        [CLSCompliant(false)]
        public static int ToInt32(ulong value)
        {
            if (value > 0x7fffffffL)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Int32"));
            }
            return (int) value;
        }

        public static int ToInt32(object value, IFormatProvider provider)
        {
            if (value != null)
            {
                return ((IConvertible) value).ToInt32(provider);
            }
            return 0;
        }

        public static int ToInt32(string value, IFormatProvider provider)
        {
            if (value == null)
            {
                return 0;
            }
            return int.Parse(value, NumberStyles.Integer, provider);
        }

        public static int ToInt32(string value, int fromBase)
        {
            if (((fromBase != 2) && (fromBase != 8)) && ((fromBase != 10) && (fromBase != 0x10)))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_InvalidBase"));
            }
            return ParseNumbers.StringToInt(value, fromBase, 0x1000);
        }

        public static long ToInt64(bool value)
        {
            return (value ? ((long) 1) : ((long) 0));
        }

        public static long ToInt64(byte value)
        {
            return (long) value;
        }

        public static long ToInt64(char value)
        {
            return (long) value;
        }

        public static long ToInt64(DateTime value)
        {
            return ((IConvertible) value).ToInt64(null);
        }

        public static long ToInt64(decimal value)
        {
            return decimal.ToInt64(decimal.Round(value, 0));
        }

        [SecuritySafeCritical]
        public static long ToInt64(double value)
        {
            return (long) Math.Round(value);
        }

        public static long ToInt64(short value)
        {
            return (long) value;
        }

        public static long ToInt64(int value)
        {
            return (long) value;
        }

        public static long ToInt64(long value)
        {
            return value;
        }

        public static long ToInt64(object value)
        {
            if (value != null)
            {
                return ((IConvertible) value).ToInt64(null);
            }
            return 0L;
        }

        [CLSCompliant(false)]
        public static long ToInt64(sbyte value)
        {
            return (long) value;
        }

        public static long ToInt64(float value)
        {
            return ToInt64((double) value);
        }

        public static long ToInt64(string value)
        {
            if (value == null)
            {
                return 0L;
            }
            return long.Parse(value, CultureInfo.CurrentCulture);
        }

        [CLSCompliant(false)]
        public static long ToInt64(ushort value)
        {
            return (long) value;
        }

        [CLSCompliant(false)]
        public static long ToInt64(uint value)
        {
            return (long) value;
        }

        [CLSCompliant(false)]
        public static long ToInt64(ulong value)
        {
            if (value > 0x7fffffffffffffffL)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_Int64"));
            }
            return (long) value;
        }

        public static long ToInt64(object value, IFormatProvider provider)
        {
            if (value != null)
            {
                return ((IConvertible) value).ToInt64(provider);
            }
            return 0L;
        }

        public static long ToInt64(string value, IFormatProvider provider)
        {
            if (value == null)
            {
                return 0L;
            }
            return long.Parse(value, NumberStyles.Integer, provider);
        }

        public static long ToInt64(string value, int fromBase)
        {
            if (((fromBase != 2) && (fromBase != 8)) && ((fromBase != 10) && (fromBase != 0x10)))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_InvalidBase"));
            }
            return ParseNumbers.StringToLong(value, fromBase, 0x1000);
        }

        [CLSCompliant(false)]
        public static sbyte ToSByte(bool value)
        {
            if (!value)
            {
                return 0;
            }
            return 1;
        }

        [CLSCompliant(false)]
        public static sbyte ToSByte(byte value)
        {
            if (value > 0x7f)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_SByte"));
            }
            return (sbyte) value;
        }

        [CLSCompliant(false)]
        public static sbyte ToSByte(char value)
        {
            if (value > '\x007f')
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_SByte"));
            }
            return (sbyte) value;
        }

        [CLSCompliant(false)]
        public static sbyte ToSByte(DateTime value)
        {
            return ((IConvertible) value).ToSByte(null);
        }

        [CLSCompliant(false)]
        public static sbyte ToSByte(decimal value)
        {
            return decimal.ToSByte(decimal.Round(value, 0));
        }

        [CLSCompliant(false)]
        public static sbyte ToSByte(double value)
        {
            return ToSByte(ToInt32(value));
        }

        [CLSCompliant(false)]
        public static sbyte ToSByte(short value)
        {
            if ((value < -128) || (value > 0x7f))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_SByte"));
            }
            return (sbyte) value;
        }

        [CLSCompliant(false)]
        public static sbyte ToSByte(int value)
        {
            if ((value < -128) || (value > 0x7f))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_SByte"));
            }
            return (sbyte) value;
        }

        [CLSCompliant(false)]
        public static sbyte ToSByte(long value)
        {
            if ((value < -128L) || (value > 0x7fL))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_SByte"));
            }
            return (sbyte) value;
        }

        [CLSCompliant(false)]
        public static sbyte ToSByte(object value)
        {
            if (value != null)
            {
                return ((IConvertible) value).ToSByte(null);
            }
            return 0;
        }

        [CLSCompliant(false)]
        public static sbyte ToSByte(sbyte value)
        {
            return value;
        }

        [CLSCompliant(false)]
        public static sbyte ToSByte(float value)
        {
            return ToSByte((double) value);
        }

        [CLSCompliant(false)]
        public static sbyte ToSByte(string value)
        {
            if (value == null)
            {
                return 0;
            }
            return sbyte.Parse(value, CultureInfo.CurrentCulture);
        }

        [CLSCompliant(false)]
        public static sbyte ToSByte(ushort value)
        {
            if (value > 0x7f)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_SByte"));
            }
            return (sbyte) value;
        }

        [CLSCompliant(false)]
        public static sbyte ToSByte(uint value)
        {
            if (value > 0x7fL)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_SByte"));
            }
            return (sbyte) value;
        }

        [CLSCompliant(false)]
        public static sbyte ToSByte(ulong value)
        {
            if (value > 0x7fL)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_SByte"));
            }
            return (sbyte) value;
        }

        [CLSCompliant(false)]
        public static sbyte ToSByte(object value, IFormatProvider provider)
        {
            if (value != null)
            {
                return ((IConvertible) value).ToSByte(provider);
            }
            return 0;
        }

        [CLSCompliant(false)]
        public static sbyte ToSByte(string value, IFormatProvider provider)
        {
            return sbyte.Parse(value, NumberStyles.Integer, provider);
        }

        [CLSCompliant(false)]
        public static sbyte ToSByte(string value, int fromBase)
        {
            if (((fromBase != 2) && (fromBase != 8)) && ((fromBase != 10) && (fromBase != 0x10)))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_InvalidBase"));
            }
            int num = ParseNumbers.StringToInt(value, fromBase, 0x1400);
            if (((fromBase == 10) || (num > 0xff)) && ((num < -128) || (num > 0x7f)))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_SByte"));
            }
            return (sbyte) num;
        }

        public static float ToSingle(bool value)
        {
            return (value ? ((float) 1) : ((float) 0));
        }

        public static float ToSingle(byte value)
        {
            return (float) value;
        }

        public static float ToSingle(char value)
        {
            return ((IConvertible) value).ToSingle(null);
        }

        public static float ToSingle(DateTime value)
        {
            return ((IConvertible) value).ToSingle(null);
        }

        public static float ToSingle(decimal value)
        {
            return (float) value;
        }

        public static float ToSingle(double value)
        {
            return (float) value;
        }

        public static float ToSingle(short value)
        {
            return (float) value;
        }

        public static float ToSingle(int value)
        {
            return (float) value;
        }

        public static float ToSingle(long value)
        {
            return (float) value;
        }

        public static float ToSingle(object value)
        {
            if (value != null)
            {
                return ((IConvertible) value).ToSingle(null);
            }
            return 0f;
        }

        [CLSCompliant(false)]
        public static float ToSingle(sbyte value)
        {
            return (float) value;
        }

        public static float ToSingle(float value)
        {
            return value;
        }

        public static float ToSingle(string value)
        {
            if (value == null)
            {
                return 0f;
            }
            return float.Parse(value, CultureInfo.CurrentCulture);
        }

        [CLSCompliant(false)]
        public static float ToSingle(ushort value)
        {
            return (float) value;
        }

        [CLSCompliant(false)]
        public static float ToSingle(uint value)
        {
            return (float) value;
        }

        [CLSCompliant(false)]
        public static float ToSingle(ulong value)
        {
            return (float) value;
        }

        public static float ToSingle(object value, IFormatProvider provider)
        {
            if (value != null)
            {
                return ((IConvertible) value).ToSingle(provider);
            }
            return 0f;
        }

        public static float ToSingle(string value, IFormatProvider provider)
        {
            if (value == null)
            {
                return 0f;
            }
            return float.Parse(value, NumberStyles.Float | NumberStyles.AllowThousands, provider);
        }

        public static string ToString(bool value)
        {
            return value.ToString();
        }

        public static string ToString(byte value)
        {
            return value.ToString(CultureInfo.CurrentCulture);
        }

        public static string ToString(char value)
        {
            return char.ToString(value);
        }

        public static string ToString(DateTime value)
        {
            return value.ToString();
        }

        public static string ToString(decimal value)
        {
            return value.ToString(CultureInfo.CurrentCulture);
        }

        public static string ToString(double value)
        {
            return value.ToString(CultureInfo.CurrentCulture);
        }

        public static string ToString(short value)
        {
            return value.ToString(CultureInfo.CurrentCulture);
        }

        public static string ToString(int value)
        {
            return value.ToString(CultureInfo.CurrentCulture);
        }

        public static string ToString(long value)
        {
            return value.ToString(CultureInfo.CurrentCulture);
        }

        public static string ToString(object value)
        {
            return ToString(value, null);
        }

        [CLSCompliant(false)]
        public static string ToString(sbyte value)
        {
            return value.ToString(CultureInfo.CurrentCulture);
        }

        public static string ToString(float value)
        {
            return value.ToString(CultureInfo.CurrentCulture);
        }

        public static string ToString(string value)
        {
            return value;
        }

        [CLSCompliant(false)]
        public static string ToString(ushort value)
        {
            return value.ToString(CultureInfo.CurrentCulture);
        }

        [CLSCompliant(false)]
        public static string ToString(uint value)
        {
            return value.ToString(CultureInfo.CurrentCulture);
        }

        [CLSCompliant(false)]
        public static string ToString(ulong value)
        {
            return value.ToString(CultureInfo.CurrentCulture);
        }

        public static string ToString(bool value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        public static string ToString(byte value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        [SecuritySafeCritical]
        public static string ToString(byte value, int toBase)
        {
            if (((toBase != 2) && (toBase != 8)) && ((toBase != 10) && (toBase != 0x10)))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_InvalidBase"));
            }
            return ParseNumbers.IntToString(value, toBase, -1, ' ', 0x40);
        }

        public static string ToString(char value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        public static string ToString(DateTime value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        public static string ToString(decimal value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        public static string ToString(double value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        public static string ToString(short value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        [SecuritySafeCritical]
        public static string ToString(short value, int toBase)
        {
            if (((toBase != 2) && (toBase != 8)) && ((toBase != 10) && (toBase != 0x10)))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_InvalidBase"));
            }
            return ParseNumbers.IntToString(value, toBase, -1, ' ', 0x80);
        }

        public static string ToString(int value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        [SecuritySafeCritical]
        public static string ToString(int value, int toBase)
        {
            if (((toBase != 2) && (toBase != 8)) && ((toBase != 10) && (toBase != 0x10)))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_InvalidBase"));
            }
            return ParseNumbers.IntToString(value, toBase, -1, ' ', 0);
        }

        public static string ToString(long value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        [SecuritySafeCritical]
        public static string ToString(long value, int toBase)
        {
            if (((toBase != 2) && (toBase != 8)) && ((toBase != 10) && (toBase != 0x10)))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_InvalidBase"));
            }
            return ParseNumbers.LongToString(value, toBase, -1, ' ', 0);
        }

        public static string ToString(object value, IFormatProvider provider)
        {
            IConvertible convertible = value as IConvertible;
            if (convertible != null)
            {
                return convertible.ToString(provider);
            }
            IFormattable formattable = value as IFormattable;
            if (formattable != null)
            {
                return formattable.ToString(null, provider);
            }
            if (value != null)
            {
                return value.ToString();
            }
            return string.Empty;
        }

        [CLSCompliant(false)]
        public static string ToString(sbyte value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        public static string ToString(float value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        public static string ToString(string value, IFormatProvider provider)
        {
            return value;
        }

        [CLSCompliant(false)]
        public static string ToString(ushort value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        [CLSCompliant(false)]
        public static string ToString(uint value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        [CLSCompliant(false)]
        public static string ToString(ulong value, IFormatProvider provider)
        {
            return value.ToString(provider);
        }

        [CLSCompliant(false)]
        public static ushort ToUInt16(bool value)
        {
            if (!value)
            {
                return 0;
            }
            return 1;
        }

        [CLSCompliant(false)]
        public static ushort ToUInt16(byte value)
        {
            return value;
        }

        [CLSCompliant(false)]
        public static ushort ToUInt16(char value)
        {
            return value;
        }

        [CLSCompliant(false)]
        public static ushort ToUInt16(DateTime value)
        {
            return ((IConvertible) value).ToUInt16(null);
        }

        [CLSCompliant(false)]
        public static ushort ToUInt16(decimal value)
        {
            return decimal.ToUInt16(decimal.Round(value, 0));
        }

        [CLSCompliant(false)]
        public static ushort ToUInt16(double value)
        {
            return ToUInt16(ToInt32(value));
        }

        [CLSCompliant(false)]
        public static ushort ToUInt16(short value)
        {
            if (value < 0)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_UInt16"));
            }
            return (ushort) value;
        }

        [CLSCompliant(false)]
        public static ushort ToUInt16(int value)
        {
            if ((value < 0) || (value > 0xffff))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_UInt16"));
            }
            return (ushort) value;
        }

        [CLSCompliant(false)]
        public static ushort ToUInt16(long value)
        {
            if ((value < 0L) || (value > 0xffffL))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_UInt16"));
            }
            return (ushort) value;
        }

        [CLSCompliant(false)]
        public static ushort ToUInt16(object value)
        {
            if (value != null)
            {
                return ((IConvertible) value).ToUInt16(null);
            }
            return 0;
        }

        [CLSCompliant(false)]
        public static ushort ToUInt16(sbyte value)
        {
            if (value < 0)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_UInt16"));
            }
            return (ushort) value;
        }

        [CLSCompliant(false)]
        public static ushort ToUInt16(float value)
        {
            return ToUInt16((double) value);
        }

        [CLSCompliant(false)]
        public static ushort ToUInt16(string value)
        {
            if (value == null)
            {
                return 0;
            }
            return ushort.Parse(value, CultureInfo.CurrentCulture);
        }

        [CLSCompliant(false)]
        public static ushort ToUInt16(ushort value)
        {
            return value;
        }

        [CLSCompliant(false)]
        public static ushort ToUInt16(uint value)
        {
            if (value > 0xffff)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_UInt16"));
            }
            return (ushort) value;
        }

        [CLSCompliant(false)]
        public static ushort ToUInt16(ulong value)
        {
            if (value > 0xffffL)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_UInt16"));
            }
            return (ushort) value;
        }

        [CLSCompliant(false)]
        public static ushort ToUInt16(object value, IFormatProvider provider)
        {
            if (value != null)
            {
                return ((IConvertible) value).ToUInt16(provider);
            }
            return 0;
        }

        [CLSCompliant(false)]
        public static ushort ToUInt16(string value, IFormatProvider provider)
        {
            if (value == null)
            {
                return 0;
            }
            return ushort.Parse(value, NumberStyles.Integer, provider);
        }

        [CLSCompliant(false)]
        public static ushort ToUInt16(string value, int fromBase)
        {
            if (((fromBase != 2) && (fromBase != 8)) && ((fromBase != 10) && (fromBase != 0x10)))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_InvalidBase"));
            }
            int num = ParseNumbers.StringToInt(value, fromBase, 0x1200);
            if ((num < 0) || (num > 0xffff))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_UInt16"));
            }
            return (ushort) num;
        }

        [CLSCompliant(false)]
        public static uint ToUInt32(bool value)
        {
            if (!value)
            {
                return 0;
            }
            return 1;
        }

        [CLSCompliant(false)]
        public static uint ToUInt32(byte value)
        {
            return value;
        }

        [CLSCompliant(false)]
        public static uint ToUInt32(char value)
        {
            return value;
        }

        [CLSCompliant(false)]
        public static uint ToUInt32(DateTime value)
        {
            return ((IConvertible) value).ToUInt32(null);
        }

        [CLSCompliant(false)]
        public static uint ToUInt32(decimal value)
        {
            return decimal.ToUInt32(decimal.Round(value, 0));
        }

        [CLSCompliant(false)]
        public static uint ToUInt32(double value)
        {
            if ((value < -0.5) || (value >= 4294967295.5))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_UInt32"));
            }
            uint num = (uint) value;
            double num2 = value - num;
            if ((num2 > 0.5) || ((num2 == 0.5) && ((num & 1) != 0)))
            {
                num++;
            }
            return num;
        }

        [CLSCompliant(false)]
        public static uint ToUInt32(short value)
        {
            if (value < 0)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_UInt32"));
            }
            return (uint) value;
        }

        [CLSCompliant(false)]
        public static uint ToUInt32(int value)
        {
            if (value < 0)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_UInt32"));
            }
            return (uint) value;
        }

        [CLSCompliant(false)]
        public static uint ToUInt32(long value)
        {
            if ((value < 0L) || (value > 0xffffffffL))
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_UInt32"));
            }
            return (uint) value;
        }

        [CLSCompliant(false)]
        public static uint ToUInt32(object value)
        {
            if (value != null)
            {
                return ((IConvertible) value).ToUInt32(null);
            }
            return 0;
        }

        [CLSCompliant(false)]
        public static uint ToUInt32(sbyte value)
        {
            if (value < 0)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_UInt32"));
            }
            return (uint) value;
        }

        [CLSCompliant(false)]
        public static uint ToUInt32(float value)
        {
            return ToUInt32((double) value);
        }

        [CLSCompliant(false)]
        public static uint ToUInt32(string value)
        {
            if (value == null)
            {
                return 0;
            }
            return uint.Parse(value, CultureInfo.CurrentCulture);
        }

        [CLSCompliant(false)]
        public static uint ToUInt32(ushort value)
        {
            return value;
        }

        [CLSCompliant(false)]
        public static uint ToUInt32(uint value)
        {
            return value;
        }

        [CLSCompliant(false)]
        public static uint ToUInt32(ulong value)
        {
            if (value > 0xffffffffL)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_UInt32"));
            }
            return (uint) value;
        }

        [CLSCompliant(false)]
        public static uint ToUInt32(object value, IFormatProvider provider)
        {
            if (value != null)
            {
                return ((IConvertible) value).ToUInt32(provider);
            }
            return 0;
        }

        [CLSCompliant(false)]
        public static uint ToUInt32(string value, IFormatProvider provider)
        {
            if (value == null)
            {
                return 0;
            }
            return uint.Parse(value, NumberStyles.Integer, provider);
        }

        [CLSCompliant(false)]
        public static uint ToUInt32(string value, int fromBase)
        {
            if (((fromBase != 2) && (fromBase != 8)) && ((fromBase != 10) && (fromBase != 0x10)))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_InvalidBase"));
            }
            return (uint) ParseNumbers.StringToInt(value, fromBase, 0x1200);
        }

        [CLSCompliant(false)]
        public static ulong ToUInt64(bool value)
        {
            if (!value)
            {
                return 0L;
            }
            return 1L;
        }

        [CLSCompliant(false)]
        public static ulong ToUInt64(byte value)
        {
            return (ulong) value;
        }

        [CLSCompliant(false)]
        public static ulong ToUInt64(char value)
        {
            return (ulong) value;
        }

        [CLSCompliant(false)]
        public static ulong ToUInt64(DateTime value)
        {
            return ((IConvertible) value).ToUInt64(null);
        }

        [CLSCompliant(false)]
        public static ulong ToUInt64(decimal value)
        {
            return decimal.ToUInt64(decimal.Round(value, 0));
        }

        [SecuritySafeCritical, CLSCompliant(false)]
        public static ulong ToUInt64(double value)
        {
            return (ulong) Math.Round(value);
        }

        [CLSCompliant(false)]
        public static ulong ToUInt64(short value)
        {
            if (value < 0)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_UInt64"));
            }
            return (ulong) value;
        }

        [CLSCompliant(false)]
        public static ulong ToUInt64(int value)
        {
            if (value < 0)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_UInt64"));
            }
            return (ulong) value;
        }

        [CLSCompliant(false)]
        public static ulong ToUInt64(long value)
        {
            if (value < 0L)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_UInt64"));
            }
            return (ulong) value;
        }

        [CLSCompliant(false)]
        public static ulong ToUInt64(object value)
        {
            if (value != null)
            {
                return ((IConvertible) value).ToUInt64(null);
            }
            return 0L;
        }

        [CLSCompliant(false)]
        public static ulong ToUInt64(sbyte value)
        {
            if (value < 0)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_UInt64"));
            }
            return (ulong) value;
        }

        [CLSCompliant(false)]
        public static ulong ToUInt64(float value)
        {
            return ToUInt64((double) value);
        }

        [CLSCompliant(false)]
        public static ulong ToUInt64(string value)
        {
            if (value == null)
            {
                return 0L;
            }
            return ulong.Parse(value, CultureInfo.CurrentCulture);
        }

        [CLSCompliant(false)]
        public static ulong ToUInt64(ushort value)
        {
            return (ulong) value;
        }

        [CLSCompliant(false)]
        public static ulong ToUInt64(uint value)
        {
            return (ulong) value;
        }

        [CLSCompliant(false)]
        public static ulong ToUInt64(ulong value)
        {
            return value;
        }

        [CLSCompliant(false)]
        public static ulong ToUInt64(object value, IFormatProvider provider)
        {
            if (value != null)
            {
                return ((IConvertible) value).ToUInt64(provider);
            }
            return 0L;
        }

        [CLSCompliant(false)]
        public static ulong ToUInt64(string value, IFormatProvider provider)
        {
            if (value == null)
            {
                return 0L;
            }
            return ulong.Parse(value, NumberStyles.Integer, provider);
        }

        [CLSCompliant(false)]
        public static ulong ToUInt64(string value, int fromBase)
        {
            if (((fromBase != 2) && (fromBase != 8)) && ((fromBase != 10) && (fromBase != 0x10)))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_InvalidBase"));
            }
            return (ulong) ParseNumbers.StringToLong(value, fromBase, 0x1200);
        }
    }
}

