namespace System
{
    using System.Collections;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    [Serializable, ComVisible(true)]
    public abstract class Enum : ValueType, IComparable, IFormattable, IConvertible
    {
        private const string enumSeperator = ", ";
        private static readonly char[] enumSeperatorCharArray;
        private static Hashtable fieldInfoHash;
        private const int maxHashElements = 100;

        static Enum()
        {
            enumSeperatorCharArray = new char[] { ',' };
            fieldInfoHash = Hashtable.Synchronized(new Hashtable());
        }

        protected Enum()
        {
        }

        [SecuritySafeCritical]
        public int CompareTo(object target)
        {
            if (this == 0)
            {
                throw new NullReferenceException();
            }
            int num = InternalCompareTo(this, target);
            if (num < 2)
            {
                return num;
            }
            if (num == 2)
            {
                Type type = base.GetType();
                Type type2 = target.GetType();
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumAndObjectMustBeSameType", new object[] { type2.ToString(), type.ToString() }));
            }
            throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_UnknownEnumType"));
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public override extern bool Equals(object obj);
        [ComVisible(true)]
        public static string Format(Type enumType, object value, string format)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException("enumType");
            }
            if (!enumType.IsEnum)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (format == null)
            {
                throw new ArgumentNullException("format");
            }
            RuntimeType eT = enumType as RuntimeType;
            if (eT == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "enumType");
            }
            Type type = value.GetType();
            Type underlyingType = GetUnderlyingType(enumType);
            if (type.IsEnum)
            {
                Type type4 = GetUnderlyingType(type);
                if (!type.IsEquivalentTo(enumType))
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_EnumAndObjectMustBeSameType", new object[] { type.ToString(), enumType.ToString() }));
                }
                type = type4;
                value = ((Enum) value).GetValue();
            }
            else if (type != underlyingType)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumFormatUnderlyingTypeAndObjectMustBeSameType", new object[] { type.ToString(), underlyingType.ToString() }));
            }
            if (format.Length != 1)
            {
                throw new FormatException(Environment.GetResourceString("Format_InvalidEnumFormatSpecification"));
            }
            char ch = format[0];
            switch (ch)
            {
                case 'D':
                case 'd':
                    return value.ToString();

                case 'X':
                case 'x':
                    return InternalFormattedHexString(value);

                case 'G':
                case 'g':
                    return InternalFormat(eT, value);
            }
            if ((ch != 'F') && (ch != 'f'))
            {
                throw new FormatException(Environment.GetResourceString("Format_InvalidEnumFormatSpecification"));
            }
            return InternalFlagsFormat(eT, value);
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetEnumValues(RuntimeTypeHandle enumType, ObjectHandleOnStack values, ObjectHandleOnStack names);
        public override int GetHashCode()
        {
            return this.GetValue().GetHashCode();
        }

        [SecuritySafeCritical]
        private static HashEntry GetHashEntry(RuntimeType enumType)
        {
            HashEntry entry = (HashEntry) fieldInfoHash[enumType];
            if (entry == null)
            {
                if (fieldInfoHash.Count > 100)
                {
                    fieldInfoHash.Clear();
                }
                ulong[] o = null;
                string[] strArray = null;
                GetEnumValues(enumType.GetTypeHandleInternal(), JitHelpers.GetObjectHandleOnStack<ulong[]>(ref o), JitHelpers.GetObjectHandleOnStack<string[]>(ref strArray));
                entry = new HashEntry(strArray, o);
                fieldInfoHash[enumType] = entry;
            }
            return entry;
        }

        [ComVisible(true)]
        public static string GetName(Type enumType, object value)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException("enumType");
            }
            return enumType.GetEnumName(value);
        }

        [ComVisible(true)]
        public static string[] GetNames(Type enumType)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException("enumType");
            }
            return enumType.GetEnumNames();
        }

        public TypeCode GetTypeCode()
        {
            Type underlyingType = GetUnderlyingType(base.GetType());
            if (underlyingType == typeof(int))
            {
                return TypeCode.Int32;
            }
            if (underlyingType == typeof(sbyte))
            {
                return TypeCode.SByte;
            }
            if (underlyingType == typeof(short))
            {
                return TypeCode.Int16;
            }
            if (underlyingType == typeof(long))
            {
                return TypeCode.Int64;
            }
            if (underlyingType == typeof(uint))
            {
                return TypeCode.UInt32;
            }
            if (underlyingType == typeof(byte))
            {
                return TypeCode.Byte;
            }
            if (underlyingType == typeof(ushort))
            {
                return TypeCode.UInt16;
            }
            if (underlyingType != typeof(ulong))
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_UnknownEnumType"));
            }
            return TypeCode.UInt64;
        }

        [SecuritySafeCritical, ComVisible(true)]
        public static Type GetUnderlyingType(Type enumType)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException("enumType");
            }
            return enumType.GetEnumUnderlyingType();
        }

        [SecuritySafeCritical]
        internal object GetValue()
        {
            return this.InternalGetValue();
        }

        [ComVisible(true)]
        public static Array GetValues(Type enumType)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException("enumType");
            }
            return enumType.GetEnumValues();
        }

        public bool HasFlag(Enum flag)
        {
            if (!base.GetType().IsEquivalentTo(flag.GetType()))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EnumTypeDoesNotMatch", new object[] { flag.GetType(), base.GetType() }));
            }
            ulong num = ToUInt64(flag.GetValue());
            return ((ToUInt64(this.GetValue()) & num) == num);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern object InternalBoxEnum(RuntimeType enumType, long value);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern int InternalCompareTo(object o1, object o2);
        private static string InternalFlagsFormat(RuntimeType eT, object value)
        {
            ulong num = ToUInt64(value);
            HashEntry hashEntry = GetHashEntry(eT);
            string[] names = hashEntry.names;
            ulong[] values = hashEntry.values;
            int index = values.Length - 1;
            StringBuilder builder = new StringBuilder();
            bool flag = true;
            ulong num3 = num;
            while (index >= 0)
            {
                if ((index == 0) && (values[index] == 0L))
                {
                    break;
                }
                if ((num & values[index]) == values[index])
                {
                    num -= values[index];
                    if (!flag)
                    {
                        builder.Insert(0, ", ");
                    }
                    builder.Insert(0, names[index]);
                    flag = false;
                }
                index--;
            }
            if (num != 0L)
            {
                return value.ToString();
            }
            if (num3 != 0L)
            {
                return builder.ToString();
            }
            if ((values.Length > 0) && (values[0] == 0L))
            {
                return names[0];
            }
            return "0";
        }

        private static string InternalFormat(RuntimeType eT, object value)
        {
            if (eT.IsDefined(typeof(FlagsAttribute), false))
            {
                return InternalFlagsFormat(eT, value);
            }
            string name = GetName(eT, value);
            if (name == null)
            {
                return value.ToString();
            }
            return name;
        }

        private static string InternalFormattedHexString(object value)
        {
            switch (Convert.GetTypeCode(value))
            {
                case TypeCode.SByte:
                {
                    byte num = (byte) ((sbyte) value);
                    return num.ToString("X2", null);
                }
                case TypeCode.Byte:
                {
                    byte num2 = (byte) value;
                    return num2.ToString("X2", null);
                }
                case TypeCode.Int16:
                {
                    ushort num3 = (ushort) ((short) value);
                    return num3.ToString("X4", null);
                }
                case TypeCode.UInt16:
                {
                    ushort num4 = (ushort) value;
                    return num4.ToString("X4", null);
                }
                case TypeCode.Int32:
                {
                    uint num6 = (uint) ((int) value);
                    return num6.ToString("X8", null);
                }
                case TypeCode.UInt32:
                {
                    uint num5 = (uint) value;
                    return num5.ToString("X8", null);
                }
                case TypeCode.Int64:
                {
                    ulong num8 = (ulong) ((long) value);
                    return num8.ToString("X16", null);
                }
                case TypeCode.UInt64:
                {
                    ulong num7 = (ulong) value;
                    return num7.ToString("X16", null);
                }
            }
            throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_UnknownEnumType"));
        }

        internal static string[] InternalGetNames(RuntimeType enumType)
        {
            return GetHashEntry(enumType).names;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        internal static extern RuntimeType InternalGetUnderlyingType(RuntimeType enumType);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern object InternalGetValue();
        internal static ulong[] InternalGetValues(RuntimeType enumType)
        {
            return GetHashEntry(enumType).values;
        }

        [ComVisible(true)]
        public static bool IsDefined(Type enumType, object value)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException("enumType");
            }
            return enumType.IsEnumDefined(value);
        }

        [ComVisible(true)]
        public static object Parse(Type enumType, string value)
        {
            return Parse(enumType, value, false);
        }

        [ComVisible(true)]
        public static object Parse(Type enumType, string value, bool ignoreCase)
        {
            EnumResult parseResult = new EnumResult();
            parseResult.Init(true);
            if (!TryParseEnum(enumType, value, ignoreCase, ref parseResult))
            {
                throw parseResult.GetEnumParseException();
            }
            return parseResult.parsedEnum;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return Convert.ToBoolean(this.GetValue(), CultureInfo.CurrentCulture);
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(this.GetValue(), CultureInfo.CurrentCulture);
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            return Convert.ToChar(this.GetValue(), CultureInfo.CurrentCulture);
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            throw new InvalidCastException(Environment.GetResourceString("InvalidCast_FromTo", new object[] { "Enum", "DateTime" }));
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return Convert.ToDecimal(this.GetValue(), CultureInfo.CurrentCulture);
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(this.GetValue(), CultureInfo.CurrentCulture);
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(this.GetValue(), CultureInfo.CurrentCulture);
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return Convert.ToInt32(this.GetValue(), CultureInfo.CurrentCulture);
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(this.GetValue(), CultureInfo.CurrentCulture);
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(this.GetValue(), CultureInfo.CurrentCulture);
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return Convert.ToSingle(this.GetValue(), CultureInfo.CurrentCulture);
        }

        object IConvertible.ToType(Type type, IFormatProvider provider)
        {
            return Convert.DefaultToType(this, type, provider);
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(this.GetValue(), CultureInfo.CurrentCulture);
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(this.GetValue(), CultureInfo.CurrentCulture);
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(this.GetValue(), CultureInfo.CurrentCulture);
        }

        [SecuritySafeCritical, ComVisible(true)]
        public static object ToObject(Type enumType, byte value)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException("enumType");
            }
            if (!enumType.IsEnum)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
            }
            RuntimeType type = enumType as RuntimeType;
            if (type == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "enumType");
            }
            return InternalBoxEnum(type, (long) value);
        }

        [ComVisible(true), SecuritySafeCritical]
        public static object ToObject(Type enumType, short value)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException("enumType");
            }
            if (!enumType.IsEnum)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
            }
            RuntimeType type = enumType as RuntimeType;
            if (type == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "enumType");
            }
            return InternalBoxEnum(type, (long) value);
        }

        [ComVisible(true), SecuritySafeCritical]
        public static object ToObject(Type enumType, int value)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException("enumType");
            }
            if (!enumType.IsEnum)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
            }
            RuntimeType type = enumType as RuntimeType;
            if (type == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "enumType");
            }
            return InternalBoxEnum(type, (long) value);
        }

        [ComVisible(true), SecuritySafeCritical]
        public static object ToObject(Type enumType, long value)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException("enumType");
            }
            if (!enumType.IsEnum)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
            }
            RuntimeType type = enumType as RuntimeType;
            if (type == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "enumType");
            }
            return InternalBoxEnum(type, value);
        }

        [ComVisible(true)]
        public static object ToObject(Type enumType, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            switch (Convert.GetTypeCode(value))
            {
                case TypeCode.SByte:
                    return ToObject(enumType, (sbyte) value);

                case TypeCode.Byte:
                    return ToObject(enumType, (byte) value);

                case TypeCode.Int16:
                    return ToObject(enumType, (short) value);

                case TypeCode.UInt16:
                    return ToObject(enumType, (ushort) value);

                case TypeCode.Int32:
                    return ToObject(enumType, (int) value);

                case TypeCode.UInt32:
                    return ToObject(enumType, (uint) value);

                case TypeCode.Int64:
                    return ToObject(enumType, (long) value);

                case TypeCode.UInt64:
                    return ToObject(enumType, (ulong) value);
            }
            throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnumBaseTypeOrEnum"), "value");
        }

        [ComVisible(true), SecuritySafeCritical, CLSCompliant(false)]
        public static object ToObject(Type enumType, sbyte value)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException("enumType");
            }
            if (!enumType.IsEnum)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
            }
            RuntimeType type = enumType as RuntimeType;
            if (type == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "enumType");
            }
            return InternalBoxEnum(type, (long) value);
        }

        [CLSCompliant(false), ComVisible(true), SecuritySafeCritical]
        public static object ToObject(Type enumType, ushort value)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException("enumType");
            }
            if (!enumType.IsEnum)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
            }
            RuntimeType type = enumType as RuntimeType;
            if (type == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "enumType");
            }
            return InternalBoxEnum(type, (long) value);
        }

        [SecuritySafeCritical, CLSCompliant(false), ComVisible(true)]
        public static object ToObject(Type enumType, uint value)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException("enumType");
            }
            if (!enumType.IsEnum)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
            }
            RuntimeType type = enumType as RuntimeType;
            if (type == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "enumType");
            }
            return InternalBoxEnum(type, (long) value);
        }

        [CLSCompliant(false), ComVisible(true), SecuritySafeCritical]
        public static object ToObject(Type enumType, ulong value)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException("enumType");
            }
            if (!enumType.IsEnum)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
            }
            RuntimeType type = enumType as RuntimeType;
            if (type == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "enumType");
            }
            return InternalBoxEnum(type, (long) value);
        }

        public override string ToString()
        {
            return InternalFormat((RuntimeType) base.GetType(), this.GetValue());
        }

        [Obsolete("The provider argument is not used. Please use ToString().")]
        public string ToString(IFormatProvider provider)
        {
            return this.ToString();
        }

        [SecuritySafeCritical]
        public string ToString(string format)
        {
            if ((format == null) || (format.Length == 0))
            {
                format = "G";
            }
            if (string.Compare(format, "G", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return this.ToString();
            }
            if (string.Compare(format, "D", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return this.GetValue().ToString();
            }
            if (string.Compare(format, "X", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return InternalFormattedHexString(this.GetValue());
            }
            if (string.Compare(format, "F", StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw new FormatException(Environment.GetResourceString("Format_InvalidEnumFormatSpecification"));
            }
            return InternalFlagsFormat((RuntimeType) base.GetType(), this.GetValue());
        }

        [Obsolete("The provider argument is not used. Please use ToString(String).")]
        public string ToString(string format, IFormatProvider provider)
        {
            return this.ToString(format);
        }

        internal static ulong ToUInt64(object value)
        {
            switch (Convert.GetTypeCode(value))
            {
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return (ulong) Convert.ToInt64(value, CultureInfo.InvariantCulture);

                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return Convert.ToUInt64(value, CultureInfo.InvariantCulture);
            }
            throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_UnknownEnumType"));
        }

        [SecuritySafeCritical]
        public static bool TryParse<TEnum>(string value, out TEnum result) where TEnum: struct
        {
            return TryParse<TEnum>(value, false, out result);
        }

        [SecuritySafeCritical]
        public static bool TryParse<TEnum>(string value, bool ignoreCase, out TEnum result) where TEnum: struct
        {
            bool flag;
            result = default(TEnum);
            EnumResult parseResult = new EnumResult();
            parseResult.Init(false);
            if (flag = TryParseEnum(typeof(TEnum), value, ignoreCase, ref parseResult))
            {
                result = (TEnum) parseResult.parsedEnum;
            }
            return flag;
        }

        [SecuritySafeCritical]
        private static bool TryParseEnum(Type enumType, string value, bool ignoreCase, ref EnumResult parseResult)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException("enumType");
            }
            RuntimeType type = enumType as RuntimeType;
            if (type == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "enumType");
            }
            if (!enumType.IsEnum)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeEnum"), "enumType");
            }
            if (value == null)
            {
                parseResult.SetFailure(ParseFailureKind.ArgumentNull, "value");
                return false;
            }
            value = value.Trim();
            if (value.Length == 0)
            {
                parseResult.SetFailure(ParseFailureKind.Argument, "Arg_MustContainEnumInfo", null);
                return false;
            }
            ulong num = 0L;
            if ((char.IsDigit(value[0]) || (value[0] == '-')) || (value[0] == '+'))
            {
                Type underlyingType = GetUnderlyingType(enumType);
                try
                {
                    object obj2 = Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
                    parseResult.parsedEnum = ToObject(enumType, obj2);
                    return true;
                }
                catch (FormatException)
                {
                }
                catch (Exception exception)
                {
                    if (parseResult.canThrow)
                    {
                        throw;
                    }
                    parseResult.SetFailure(exception);
                    return false;
                }
            }
            string[] strArray = value.Split(enumSeperatorCharArray);
            HashEntry hashEntry = GetHashEntry(type);
            string[] names = hashEntry.names;
            for (int i = 0; i < strArray.Length; i++)
            {
                strArray[i] = strArray[i].Trim();
                bool flag = false;
                for (int j = 0; j < names.Length; j++)
                {
                    ulong num4;
                    if (ignoreCase)
                    {
                        if (string.Compare(names[j], strArray[i], StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            goto Label_0157;
                        }
                        continue;
                    }
                    if (!names[j].Equals(strArray[i]))
                    {
                        continue;
                    }
                Label_0157:
                    num4 = hashEntry.values[j];
                    num |= num4;
                    flag = true;
                    break;
                }
                if (!flag)
                {
                    parseResult.SetFailure(ParseFailureKind.ArgumentWithParameter, "Arg_EnumValueNotFound", value);
                    return false;
                }
            }
            try
            {
                parseResult.parsedEnum = ToObject(enumType, num);
                return true;
            }
            catch (Exception exception2)
            {
                if (parseResult.canThrow)
                {
                    throw;
                }
                parseResult.SetFailure(exception2);
                return false;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct EnumResult
        {
            internal object parsedEnum;
            internal bool canThrow;
            internal Enum.ParseFailureKind m_failure;
            internal string m_failureMessageID;
            internal string m_failureParameter;
            internal object m_failureMessageFormatArgument;
            internal Exception m_innerException;
            internal void Init(bool canMethodThrow)
            {
                this.parsedEnum = 0;
                this.canThrow = canMethodThrow;
            }

            internal void SetFailure(Exception unhandledException)
            {
                this.m_failure = Enum.ParseFailureKind.UnhandledException;
                this.m_innerException = unhandledException;
            }

            internal void SetFailure(Enum.ParseFailureKind failure, string failureParameter)
            {
                this.m_failure = failure;
                this.m_failureParameter = failureParameter;
                if (this.canThrow)
                {
                    throw this.GetEnumParseException();
                }
            }

            internal void SetFailure(Enum.ParseFailureKind failure, string failureMessageID, object failureMessageFormatArgument)
            {
                this.m_failure = failure;
                this.m_failureMessageID = failureMessageID;
                this.m_failureMessageFormatArgument = failureMessageFormatArgument;
                if (this.canThrow)
                {
                    throw this.GetEnumParseException();
                }
            }

            internal Exception GetEnumParseException()
            {
                switch (this.m_failure)
                {
                    case Enum.ParseFailureKind.Argument:
                        return new ArgumentException(Environment.GetResourceString(this.m_failureMessageID));

                    case Enum.ParseFailureKind.ArgumentNull:
                        return new ArgumentNullException(this.m_failureParameter);

                    case Enum.ParseFailureKind.ArgumentWithParameter:
                        return new ArgumentException(Environment.GetResourceString(this.m_failureMessageID, new object[] { this.m_failureMessageFormatArgument }));

                    case Enum.ParseFailureKind.UnhandledException:
                        return this.m_innerException;
                }
                return new ArgumentException(Environment.GetResourceString("Arg_EnumValueNotFound"));
            }
        }

        private class HashEntry
        {
            public string[] names;
            public ulong[] values;

            public HashEntry(string[] names, ulong[] values)
            {
                this.names = names;
                this.values = values;
            }
        }

        private enum ParseFailureKind
        {
            None,
            Argument,
            ArgumentNull,
            ArgumentWithParameter,
            UnhandledException
        }
    }
}

