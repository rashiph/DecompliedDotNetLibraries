namespace Microsoft.VisualBasic.CompilerServices
{
    using Microsoft.VisualBasic;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Dynamic;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class Conversions
    {
        private Conversions()
        {
        }

        internal static bool CanUserDefinedConvert(object Expression, Type TargetType)
        {
            Type sourceType = Expression.GetType();
            if (((ConversionResolution.ClassifyPredefinedConversion(TargetType, sourceType) == ConversionResolution.ConversionClass.None) && (Symbols.IsClassOrValueType(sourceType) || Symbols.IsClassOrValueType(TargetType))) && (!Symbols.IsIntrinsicType(sourceType) || !Symbols.IsIntrinsicType(TargetType)))
            {
                Symbols.Method operatorMethod = null;
                ConversionResolution.ConversionClass class2 = ConversionResolution.ClassifyUserDefinedConversion(TargetType, sourceType, ref operatorMethod);
                return (operatorMethod != null);
            }
            return false;
        }

        private static object CastByteEnum(byte Expression, Type TargetType)
        {
            if (Symbols.IsEnum(TargetType))
            {
                return Enum.ToObject(TargetType, Expression);
            }
            return Expression;
        }

        private static object CastInt16Enum(short Expression, Type TargetType)
        {
            if (Symbols.IsEnum(TargetType))
            {
                return Enum.ToObject(TargetType, Expression);
            }
            return Expression;
        }

        private static object CastInt32Enum(int Expression, Type TargetType)
        {
            if (Symbols.IsEnum(TargetType))
            {
                return Enum.ToObject(TargetType, Expression);
            }
            return Expression;
        }

        private static object CastInt64Enum(long Expression, Type TargetType)
        {
            if (Symbols.IsEnum(TargetType))
            {
                return Enum.ToObject(TargetType, Expression);
            }
            return Expression;
        }

        private static object CastSByteEnum(sbyte Expression, Type TargetType)
        {
            if (Symbols.IsEnum(TargetType))
            {
                return Enum.ToObject(TargetType, Expression);
            }
            return Expression;
        }

        private static object CastUInt16Enum(ushort Expression, Type TargetType)
        {
            if (Symbols.IsEnum(TargetType))
            {
                return Enum.ToObject(TargetType, Expression);
            }
            return Expression;
        }

        private static object CastUInt32Enum(uint Expression, Type TargetType)
        {
            if (Symbols.IsEnum(TargetType))
            {
                return Enum.ToObject(TargetType, Expression);
            }
            return Expression;
        }

        private static object CastUInt64Enum(ulong Expression, Type TargetType)
        {
            if (Symbols.IsEnum(TargetType))
            {
                return Enum.ToObject(TargetType, Expression);
            }
            return Expression;
        }

        private static object ChangeIntrinsicType(object Expression, Type TargetType)
        {
            switch (Symbols.GetTypeCode(TargetType))
            {
                case TypeCode.Boolean:
                    return ToBoolean(Expression);

                case TypeCode.Char:
                    return ToChar(Expression);

                case TypeCode.SByte:
                    return CastSByteEnum(ToSByte(Expression), TargetType);

                case TypeCode.Byte:
                    return CastByteEnum(ToByte(Expression), TargetType);

                case TypeCode.Int16:
                    return CastInt16Enum(ToShort(Expression), TargetType);

                case TypeCode.UInt16:
                    return CastUInt16Enum(ToUShort(Expression), TargetType);

                case TypeCode.Int32:
                    return CastInt32Enum(ToInteger(Expression), TargetType);

                case TypeCode.UInt32:
                    return CastUInt32Enum(ToUInteger(Expression), TargetType);

                case TypeCode.Int64:
                    return CastInt64Enum(ToLong(Expression), TargetType);

                case TypeCode.UInt64:
                    return CastUInt64Enum(ToULong(Expression), TargetType);

                case TypeCode.Single:
                    return ToSingle(Expression);

                case TypeCode.Double:
                    return ToDouble(Expression);

                case TypeCode.Decimal:
                    return ToDecimal(Expression);

                case TypeCode.DateTime:
                    return ToDate(Expression);

                case TypeCode.String:
                    return ToString(Expression);
            }
            throw new Exception();
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static object ChangeType(object Expression, Type TargetType)
        {
            return ChangeType(Expression, TargetType, false);
        }

        [SecuritySafeCritical]
        internal static object ChangeType(object Expression, Type TargetType, bool Dynamic)
        {
            if (TargetType == null)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidNullValue1", new string[] { "TargetType" }));
            }
            if (Expression == null)
            {
                if (Symbols.IsValueType(TargetType))
                {
                    new ReflectionPermission(ReflectionPermissionFlag.NoFlags).Demand();
                    return Activator.CreateInstance(TargetType);
                }
                return null;
            }
            Type type = Expression.GetType();
            if (TargetType.IsByRef)
            {
                TargetType = TargetType.GetElementType();
            }
            if ((TargetType == type) || Symbols.IsRootObjectType(TargetType))
            {
                return Expression;
            }
            if (Symbols.IsIntrinsicType(Symbols.GetTypeCode(TargetType)) && Symbols.IsIntrinsicType(Symbols.GetTypeCode(type)))
            {
                return ChangeIntrinsicType(Expression, TargetType);
            }
            if (TargetType.IsInstanceOfType(Expression))
            {
                return Expression;
            }
            if (Symbols.IsCharArrayRankOne(TargetType) && Symbols.IsStringType(type))
            {
                return ToCharArrayRankOne((string) Expression);
            }
            if (Symbols.IsStringType(TargetType) && Symbols.IsCharArrayRankOne(type))
            {
                return new string((char[]) Expression);
            }
            if (Dynamic)
            {
                IDynamicMetaObjectProvider expression = IDOUtils.TryCastToIDMOP(Expression);
                if (expression != null)
                {
                    return IDOBinder.UserDefinedConversion(expression, TargetType);
                }
            }
            return ObjectUserDefinedConversion(Expression, TargetType);
        }

        [DebuggerHidden, DebuggerStepThrough, Obsolete("do not use this method", true), TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static object FallbackUserDefinedConversion(object Expression, Type TargetType)
        {
            return ObjectUserDefinedConversion(Expression, TargetType);
        }

        internal static object ForceValueCopy(object Expression, Type TargetType)
        {
            IConvertible convertible = Expression as IConvertible;
            if (convertible != null)
            {
                switch (convertible.GetTypeCode())
                {
                    case TypeCode.Empty:
                    case TypeCode.Object:
                    case TypeCode.DBNull:
                    case (TypeCode.DateTime | TypeCode.Object):
                    case TypeCode.String:
                        return Expression;

                    case TypeCode.Boolean:
                        return convertible.ToBoolean(null);

                    case TypeCode.Char:
                        return convertible.ToChar(null);

                    case TypeCode.SByte:
                        return CastSByteEnum(convertible.ToSByte(null), TargetType);

                    case TypeCode.Byte:
                        return CastByteEnum(convertible.ToByte(null), TargetType);

                    case TypeCode.Int16:
                        return CastInt16Enum(convertible.ToInt16(null), TargetType);

                    case TypeCode.UInt16:
                        return CastUInt16Enum(convertible.ToUInt16(null), TargetType);

                    case TypeCode.Int32:
                        return CastInt32Enum(convertible.ToInt32(null), TargetType);

                    case TypeCode.UInt32:
                        return CastUInt32Enum(convertible.ToUInt32(null), TargetType);

                    case TypeCode.Int64:
                        return CastInt64Enum(convertible.ToInt64(null), TargetType);

                    case TypeCode.UInt64:
                        return CastUInt64Enum(convertible.ToUInt64(null), TargetType);

                    case TypeCode.Single:
                        return convertible.ToSingle(null);

                    case TypeCode.Double:
                        return convertible.ToDouble(null);

                    case TypeCode.Decimal:
                        return convertible.ToDecimal(null);

                    case TypeCode.DateTime:
                        return convertible.ToDateTime(null);
                }
            }
            return Expression;
        }

        public static string FromCharAndCount(char Value, int Count)
        {
            return new string(Value, Count);
        }

        public static string FromCharArray(char[] Value)
        {
            return new string(Value);
        }

        public static string FromCharArraySubset(char[] Value, int StartIndex, int Length)
        {
            return new string(Value, StartIndex, Length);
        }

        private static NumberFormatInfo GetNormalizedNumberFormat(NumberFormatInfo InNumberFormat)
        {
            NumberFormatInfo info2;
            NumberFormatInfo info3 = InNumberFormat;
            if (((((info3.CurrencyDecimalSeparator != null) && (info3.NumberDecimalSeparator != null)) && ((info3.CurrencyGroupSeparator != null) && (info3.NumberGroupSeparator != null))) && (((info3.CurrencyDecimalSeparator.Length == 1) && (info3.NumberDecimalSeparator.Length == 1)) && ((info3.CurrencyGroupSeparator.Length == 1) && (info3.NumberGroupSeparator.Length == 1)))) && (((info3.CurrencyDecimalSeparator[0] == info3.NumberDecimalSeparator[0]) && (info3.CurrencyGroupSeparator[0] == info3.NumberGroupSeparator[0])) && (info3.CurrencyDecimalDigits == info3.NumberDecimalDigits)))
            {
                return InNumberFormat;
            }
            info3 = null;
            NumberFormatInfo info4 = InNumberFormat;
            if ((((info4.CurrencyDecimalSeparator != null) && (info4.NumberDecimalSeparator != null)) && ((info4.CurrencyDecimalSeparator.Length == info4.NumberDecimalSeparator.Length) && (info4.CurrencyGroupSeparator != null))) && ((info4.NumberGroupSeparator != null) && (info4.CurrencyGroupSeparator.Length == info4.NumberGroupSeparator.Length)))
            {
                int num;
                int num2 = info4.CurrencyDecimalSeparator.Length - 1;
                for (num = 0; num <= num2; num++)
                {
                    if (info4.CurrencyDecimalSeparator[num] != info4.NumberDecimalSeparator[num])
                    {
                        goto Label_018E;
                    }
                }
                int num3 = info4.CurrencyGroupSeparator.Length - 1;
                for (num = 0; num <= num3; num++)
                {
                    if (info4.CurrencyGroupSeparator[num] != info4.NumberGroupSeparator[num])
                    {
                        goto Label_018E;
                    }
                }
                return InNumberFormat;
            }
            info4 = null;
        Label_018E:
            info2 = (NumberFormatInfo) InNumberFormat.Clone();
            NumberFormatInfo info5 = info2;
            info5.CurrencyDecimalSeparator = info5.NumberDecimalSeparator;
            info5.CurrencyGroupSeparator = info5.NumberGroupSeparator;
            info5.CurrencyDecimalDigits = info5.NumberDecimalDigits;
            info5 = null;
            return info2;
        }

        [DebuggerStepThrough, DebuggerHidden]
        private static object ObjectUserDefinedConversion(object Expression, Type TargetType)
        {
            Type sourceType = Expression.GetType();
            if (((ConversionResolution.ClassifyPredefinedConversion(TargetType, sourceType) == ConversionResolution.ConversionClass.None) && (Symbols.IsClassOrValueType(sourceType) || Symbols.IsClassOrValueType(TargetType))) && (!Symbols.IsIntrinsicType(sourceType) || !Symbols.IsIntrinsicType(TargetType)))
            {
                Symbols.Method operatorMethod = null;
                ConversionResolution.ConversionClass class2 = ConversionResolution.ClassifyUserDefinedConversion(TargetType, sourceType, ref operatorMethod);
                if (operatorMethod != null)
                {
                    Symbols.Container container = new Symbols.Container(operatorMethod.DeclaringType);
                    return ChangeType(container.InvokeMethod(operatorMethod, new object[] { Expression }, null, BindingFlags.InvokeMethod), TargetType);
                }
                if (class2 == ConversionResolution.ConversionClass.Ambiguous)
                {
                    throw new InvalidCastException(Utils.GetResourceString("AmbiguousCast2", new string[] { Utils.VBFriendlyName(sourceType), Utils.VBFriendlyName(TargetType) }));
                }
            }
            throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(sourceType), Utils.VBFriendlyName(TargetType) }));
        }

        private static decimal ParseDecimal(string Value, NumberFormatInfo NumberFormat)
        {
            decimal num;
            CultureInfo cultureInfo = Utils.GetCultureInfo();
            if (NumberFormat == null)
            {
                NumberFormat = cultureInfo.NumberFormat;
            }
            NumberFormatInfo normalizedNumberFormat = GetNormalizedNumberFormat(NumberFormat);
            Value = Utils.ToHalfwidthNumbers(Value, cultureInfo);
            try
            {
                num = decimal.Parse(Value, NumberStyles.Any, normalizedNumberFormat);
            }
            catch when (?)
            {
                num = decimal.Parse(Value, NumberStyles.Any, NumberFormat);
            }
            catch (Exception exception2)
            {
                throw exception2;
            }
            return num;
        }

        private static double ParseDouble(string Value)
        {
            return ParseDouble(Value, null);
        }

        private static double ParseDouble(string Value, NumberFormatInfo NumberFormat)
        {
            double num;
            CultureInfo cultureInfo = Utils.GetCultureInfo();
            if (NumberFormat == null)
            {
                NumberFormat = cultureInfo.NumberFormat;
            }
            NumberFormatInfo normalizedNumberFormat = GetNormalizedNumberFormat(NumberFormat);
            Value = Utils.ToHalfwidthNumbers(Value, cultureInfo);
            try
            {
                num = double.Parse(Value, NumberStyles.Any, (IFormatProvider) normalizedNumberFormat);
            }
            catch when (?)
            {
                num = double.Parse(Value, NumberStyles.Any, (IFormatProvider) NumberFormat);
            }
            catch (Exception exception2)
            {
                throw exception2;
            }
            return num;
        }

        public static bool ToBoolean(object Value)
        {
            if (Value == null)
            {
                return false;
            }
            IConvertible convertible = Value as IConvertible;
            if (convertible != null)
            {
                switch (convertible.GetTypeCode())
                {
                    case TypeCode.Boolean:
                        if (Value is bool)
                        {
                            return (bool) Value;
                        }
                        return convertible.ToBoolean(null);

                    case TypeCode.SByte:
                        if (Value is sbyte)
                        {
                            return (((sbyte) Value) > 0);
                        }
                        return (convertible.ToSByte(null) > 0);

                    case TypeCode.Byte:
                        if (Value is byte)
                        {
                            return (((byte) Value) > 0);
                        }
                        return (convertible.ToByte(null) > 0);

                    case TypeCode.Int16:
                        if (Value is short)
                        {
                            return (((short) Value) > 0);
                        }
                        return (convertible.ToInt16(null) > 0);

                    case TypeCode.UInt16:
                        if (Value is ushort)
                        {
                            return (((ushort) Value) > 0);
                        }
                        return (convertible.ToUInt16(null) > 0);

                    case TypeCode.Int32:
                        if (Value is int)
                        {
                            return (((int) Value) > 0);
                        }
                        return (convertible.ToInt32(null) > 0);

                    case TypeCode.UInt32:
                        if (Value is uint)
                        {
                            return (((uint) Value) > 0);
                        }
                        return (convertible.ToUInt32(null) > 0);

                    case TypeCode.Int64:
                        if (Value is long)
                        {
                            return (((long) Value) > 0L);
                        }
                        return (convertible.ToInt64(null) > 0L);

                    case TypeCode.UInt64:
                        if (Value is ulong)
                        {
                            return (((ulong) Value) > 0L);
                        }
                        return (convertible.ToUInt64(null) > 0L);

                    case TypeCode.Single:
                        if (Value is float)
                        {
                            return !(((float) Value) == 0f);
                        }
                        return !(convertible.ToSingle(null) == 0f);

                    case TypeCode.Double:
                        if (Value is double)
                        {
                            return !(((double) Value) == 0.0);
                        }
                        return !(convertible.ToDouble(null) == 0.0);

                    case TypeCode.Decimal:
                        if (Value is decimal)
                        {
                            return convertible.ToBoolean(null);
                        }
                        return Convert.ToBoolean(convertible.ToDecimal(null));

                    case TypeCode.String:
                    {
                        string str = Value as string;
                        if (str != null)
                        {
                            return ToBoolean(str);
                        }
                        return ToBoolean(convertible.ToString(null));
                    }
                }
            }
            throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(Value), "Boolean" }));
        }

        public static bool ToBoolean(string Value)
        {
            bool flag;
            if (Value == null)
            {
                Value = "";
            }
            try
            {
                long num;
                CultureInfo cultureInfo = Utils.GetCultureInfo();
                if (string.Compare(Value, bool.FalseString, true, cultureInfo) == 0)
                {
                    return false;
                }
                if (string.Compare(Value, bool.TrueString, true, cultureInfo) == 0)
                {
                    return true;
                }
                if (Utils.IsHexOrOctValue(Value, ref num))
                {
                    return (num > 0L);
                }
                flag = !(ParseDouble(Value) == 0.0);
            }
            catch (FormatException exception)
            {
                throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", new string[] { Strings.Left(Value, 0x20), "Boolean" }), exception);
            }
            return flag;
        }

        public static byte ToByte(object Value)
        {
            if (Value == null)
            {
                return 0;
            }
            IConvertible convertible = Value as IConvertible;
            if (convertible != null)
            {
                switch (convertible.GetTypeCode())
                {
                    case TypeCode.Boolean:
                        if (Value is bool)
                        {
                            return (byte) -(((bool) Value) > false);
                        }
                        return (byte) -(convertible.ToBoolean(null) > false);

                    case TypeCode.SByte:
                        if (Value is sbyte)
                        {
                            return (byte) ((sbyte) Value);
                        }
                        return (byte) convertible.ToSByte(null);

                    case TypeCode.Byte:
                        if (Value is byte)
                        {
                            return (byte) Value;
                        }
                        return convertible.ToByte(null);

                    case TypeCode.Int16:
                        if (Value is short)
                        {
                            return (byte) ((short) Value);
                        }
                        return (byte) convertible.ToInt16(null);

                    case TypeCode.UInt16:
                        if (Value is ushort)
                        {
                            return (byte) ((ushort) Value);
                        }
                        return (byte) convertible.ToUInt16(null);

                    case TypeCode.Int32:
                        if (Value is int)
                        {
                            return (byte) ((int) Value);
                        }
                        return (byte) convertible.ToInt32(null);

                    case TypeCode.UInt32:
                        if (Value is uint)
                        {
                            return (byte) ((uint) Value);
                        }
                        return (byte) convertible.ToUInt32(null);

                    case TypeCode.Int64:
                        if (Value is long)
                        {
                            return (byte) ((long) Value);
                        }
                        return (byte) convertible.ToInt64(null);

                    case TypeCode.UInt64:
                        if (Value is ulong)
                        {
                            return (byte) ((ulong) Value);
                        }
                        return (byte) convertible.ToUInt64(null);

                    case TypeCode.Single:
                        if (Value is float)
                        {
                            return (byte) Math.Round((double) ((float) Value));
                        }
                        return (byte) Math.Round((double) convertible.ToSingle(null));

                    case TypeCode.Double:
                        if (Value is double)
                        {
                            return (byte) Math.Round((double) Value);
                        }
                        return (byte) Math.Round(convertible.ToDouble(null));

                    case TypeCode.Decimal:
                        if (Value is decimal)
                        {
                            return convertible.ToByte(null);
                        }
                        return Convert.ToByte(convertible.ToDecimal(null));

                    case TypeCode.String:
                    {
                        string str = Value as string;
                        if (str != null)
                        {
                            return ToByte(str);
                        }
                        return ToByte(convertible.ToString(null));
                    }
                }
            }
            throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(Value), "Byte" }));
        }

        public static byte ToByte(string Value)
        {
            byte num;
            if (Value == null)
            {
                return 0;
            }
            try
            {
                long num2;
                if (Utils.IsHexOrOctValue(Value, ref num2))
                {
                    return (byte) num2;
                }
                num = (byte) Math.Round(ParseDouble(Value));
            }
            catch (FormatException exception)
            {
                throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", new string[] { Strings.Left(Value, 0x20), "Byte" }), exception);
            }
            return num;
        }

        public static char ToChar(object Value)
        {
            if (Value == null)
            {
                return '\0';
            }
            IConvertible convertible = Value as IConvertible;
            if (convertible != null)
            {
                switch (convertible.GetTypeCode())
                {
                    case TypeCode.Char:
                        if (Value is char)
                        {
                            return (char) Value;
                        }
                        return convertible.ToChar(null);

                    case TypeCode.String:
                    {
                        string str = Value as string;
                        if (str == null)
                        {
                            return ToChar(convertible.ToString(null));
                        }
                        return ToChar(str);
                    }
                }
            }
            throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(Value), "Char" }));
        }

        public static char ToChar(string Value)
        {
            if ((Value != null) && (Value.Length != 0))
            {
                return Value[0];
            }
            return '\0';
        }

        public static char[] ToCharArrayRankOne(object Value)
        {
            if (Value == null)
            {
                return "".ToCharArray();
            }
            char[] chArray = Value as char[];
            if ((chArray != null) && (chArray.Rank == 1))
            {
                return chArray;
            }
            IConvertible convertible = Value as IConvertible;
            if ((convertible == null) || (convertible.GetTypeCode() != TypeCode.String))
            {
                throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(Value), "Char()" }));
            }
            return convertible.ToString(null).ToCharArray();
        }

        public static char[] ToCharArrayRankOne(string Value)
        {
            if (Value == null)
            {
                Value = "";
            }
            return Value.ToCharArray();
        }

        public static DateTime ToDate(object Value)
        {
            if (Value == null)
            {
                DateTime time;
                return time;
            }
            IConvertible convertible = Value as IConvertible;
            if (convertible != null)
            {
                switch (convertible.GetTypeCode())
                {
                    case TypeCode.DateTime:
                        if (Value is DateTime)
                        {
                            return (DateTime) Value;
                        }
                        return convertible.ToDateTime(null);

                    case TypeCode.String:
                    {
                        string str = Value as string;
                        if (str == null)
                        {
                            return ToDate(convertible.ToString(null));
                        }
                        return ToDate(str);
                    }
                }
            }
            throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(Value), "Date" }));
        }

        public static DateTime ToDate(string Value)
        {
            DateTime time;
            if (!TryParseDate(Value, ref time))
            {
                throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", new string[] { Strings.Left(Value, 0x20), "Date" }));
            }
            return time;
        }

        public static decimal ToDecimal(bool Value)
        {
            if (Value)
            {
                return decimal.MinusOne;
            }
            return decimal.Zero;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static decimal ToDecimal(object Value)
        {
            return ToDecimal(Value, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static decimal ToDecimal(string Value)
        {
            return ToDecimal(Value, null);
        }

        internal static decimal ToDecimal(object Value, NumberFormatInfo NumberFormat)
        {
            if (Value == null)
            {
                return decimal.Zero;
            }
            IConvertible convertible = Value as IConvertible;
            if (convertible != null)
            {
                switch (convertible.GetTypeCode())
                {
                    case TypeCode.Boolean:
                        if (Value is bool)
                        {
                            return ToDecimal((bool) Value);
                        }
                        return ToDecimal(convertible.ToBoolean(null));

                    case TypeCode.SByte:
                        if (Value is sbyte)
                        {
                            return new decimal((sbyte) Value);
                        }
                        return new decimal(convertible.ToSByte(null));

                    case TypeCode.Byte:
                        if (Value is byte)
                        {
                            return new decimal((byte) Value);
                        }
                        return new decimal(convertible.ToByte(null));

                    case TypeCode.Int16:
                        if (Value is short)
                        {
                            return new decimal((short) Value);
                        }
                        return new decimal(convertible.ToInt16(null));

                    case TypeCode.UInt16:
                        if (Value is ushort)
                        {
                            return new decimal((ushort) Value);
                        }
                        return new decimal(convertible.ToUInt16(null));

                    case TypeCode.Int32:
                        if (Value is int)
                        {
                            return new decimal((int) Value);
                        }
                        return new decimal(convertible.ToInt32(null));

                    case TypeCode.UInt32:
                        if (Value is uint)
                        {
                            return new decimal((uint) Value);
                        }
                        return new decimal(convertible.ToUInt32(null));

                    case TypeCode.Int64:
                        if (Value is long)
                        {
                            return new decimal((long) Value);
                        }
                        return new decimal(convertible.ToInt64(null));

                    case TypeCode.UInt64:
                        if (Value is ulong)
                        {
                            return new decimal((ulong) Value);
                        }
                        return new decimal(convertible.ToUInt64(null));

                    case TypeCode.Single:
                        if (Value is float)
                        {
                            return new decimal((float) Value);
                        }
                        return new decimal(convertible.ToSingle(null));

                    case TypeCode.Double:
                        if (Value is double)
                        {
                            return new decimal((double) Value);
                        }
                        return new decimal(convertible.ToDouble(null));

                    case TypeCode.Decimal:
                        return convertible.ToDecimal(null);

                    case TypeCode.String:
                        return ToDecimal(convertible.ToString(null), NumberFormat);
                }
            }
            throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(Value), "Decimal" }));
        }

        internal static decimal ToDecimal(string Value, NumberFormatInfo NumberFormat)
        {
            decimal num;
            if (Value == null)
            {
                return decimal.Zero;
            }
            try
            {
                long num2;
                if (Utils.IsHexOrOctValue(Value, ref num2))
                {
                    return new decimal(num2);
                }
                num = ParseDecimal(Value, NumberFormat);
            }
            catch (OverflowException)
            {
                throw ExceptionUtils.VbMakeException(6);
            }
            catch (FormatException)
            {
                throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", new string[] { Strings.Left(Value, 0x20), "Decimal" }));
            }
            return num;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static double ToDouble(object Value)
        {
            return ToDouble(Value, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static double ToDouble(string Value)
        {
            return ToDouble(Value, null);
        }

        internal static double ToDouble(object Value, NumberFormatInfo NumberFormat)
        {
            if (Value == null)
            {
                return 0.0;
            }
            IConvertible convertible = Value as IConvertible;
            if (convertible != null)
            {
                switch (convertible.GetTypeCode())
                {
                    case TypeCode.Boolean:
                        if (Value is bool)
                        {
                            return (double) -(((bool) Value) > false);
                        }
                        return (double) -(convertible.ToBoolean(null) > false);

                    case TypeCode.SByte:
                        if (Value is sbyte)
                        {
                            return (double) ((sbyte) Value);
                        }
                        return (double) convertible.ToSByte(null);

                    case TypeCode.Byte:
                        if (Value is byte)
                        {
                            return (double) ((byte) Value);
                        }
                        return (double) convertible.ToByte(null);

                    case TypeCode.Int16:
                        if (Value is short)
                        {
                            return (double) ((short) Value);
                        }
                        return (double) convertible.ToInt16(null);

                    case TypeCode.UInt16:
                        if (Value is ushort)
                        {
                            return (double) ((ushort) Value);
                        }
                        return (double) convertible.ToUInt16(null);

                    case TypeCode.Int32:
                        if (Value is int)
                        {
                            return (double) ((int) Value);
                        }
                        return (double) convertible.ToInt32(null);

                    case TypeCode.UInt32:
                        if (Value is uint)
                        {
                            return (double) ((uint) Value);
                        }
                        return (double) convertible.ToUInt32(null);

                    case TypeCode.Int64:
                        if (Value is long)
                        {
                            return (double) ((long) Value);
                        }
                        return (double) convertible.ToInt64(null);

                    case TypeCode.UInt64:
                        if (Value is ulong)
                        {
                            return (double) ((ulong) Value);
                        }
                        return (double) convertible.ToUInt64(null);

                    case TypeCode.Single:
                        if (Value is float)
                        {
                            return (double) ((float) Value);
                        }
                        return (double) convertible.ToSingle(null);

                    case TypeCode.Double:
                        if (Value is double)
                        {
                            return (double) Value;
                        }
                        return convertible.ToDouble(null);

                    case TypeCode.Decimal:
                        if (Value is decimal)
                        {
                            return convertible.ToDouble(null);
                        }
                        return Convert.ToDouble(convertible.ToDecimal(null));

                    case TypeCode.String:
                        return ToDouble(convertible.ToString(null), NumberFormat);
                }
            }
            throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(Value), "Double" }));
        }

        internal static double ToDouble(string Value, NumberFormatInfo NumberFormat)
        {
            double num;
            if (Value == null)
            {
                return 0.0;
            }
            try
            {
                long num2;
                if (Utils.IsHexOrOctValue(Value, ref num2))
                {
                    return (double) num2;
                }
                num = ParseDouble(Value, NumberFormat);
            }
            catch (FormatException exception)
            {
                throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", new string[] { Strings.Left(Value, 0x20), "Double" }), exception);
            }
            return num;
        }

        public static T ToGenericParameter<T>(object Value)
        {
            if (Value == null)
            {
                return default(T);
            }
            Type type = typeof(T);
            switch (Symbols.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return (T) ToBoolean(Value);

                case TypeCode.Char:
                    return (T) ToChar(Value);

                case TypeCode.SByte:
                    return (T) ToSByte(Value);

                case TypeCode.Byte:
                    return (T) ToByte(Value);

                case TypeCode.Int16:
                    return (T) ToShort(Value);

                case TypeCode.UInt16:
                    return (T) ToUShort(Value);

                case TypeCode.Int32:
                    return (T) ToInteger(Value);

                case TypeCode.UInt32:
                    return (T) ToUInteger(Value);

                case TypeCode.Int64:
                    return (T) ToLong(Value);

                case TypeCode.UInt64:
                    return (T) ToULong(Value);

                case TypeCode.Single:
                    return (T) ToSingle(Value);

                case TypeCode.Double:
                    return (T) ToDouble(Value);

                case TypeCode.Decimal:
                    return (T) ToDecimal(Value);

                case TypeCode.DateTime:
                    return (T) ToDate(Value);

                case TypeCode.String:
                    return (T) ToString(Value);
            }
            return (T) Value;
        }

        public static int ToInteger(object Value)
        {
            if (Value == null)
            {
                return 0;
            }
            IConvertible convertible = Value as IConvertible;
            if (convertible != null)
            {
                switch (convertible.GetTypeCode())
                {
                    case TypeCode.Boolean:
                        if (Value is bool)
                        {
                            return (int) -(((bool) Value) > false);
                        }
                        return (int) -(convertible.ToBoolean(null) > false);

                    case TypeCode.SByte:
                        if (Value is sbyte)
                        {
                            return (sbyte) Value;
                        }
                        return convertible.ToSByte(null);

                    case TypeCode.Byte:
                        if (Value is byte)
                        {
                            return (byte) Value;
                        }
                        return convertible.ToByte(null);

                    case TypeCode.Int16:
                        if (Value is short)
                        {
                            return (short) Value;
                        }
                        return convertible.ToInt16(null);

                    case TypeCode.UInt16:
                        if (Value is ushort)
                        {
                            return (ushort) Value;
                        }
                        return convertible.ToUInt16(null);

                    case TypeCode.Int32:
                        if (Value is int)
                        {
                            return (int) Value;
                        }
                        return convertible.ToInt32(null);

                    case TypeCode.UInt32:
                        if (Value is uint)
                        {
                            return (int) ((uint) Value);
                        }
                        return (int) convertible.ToUInt32(null);

                    case TypeCode.Int64:
                        if (Value is long)
                        {
                            return (int) ((long) Value);
                        }
                        return (int) convertible.ToInt64(null);

                    case TypeCode.UInt64:
                        if (Value is ulong)
                        {
                            return (int) ((ulong) Value);
                        }
                        return (int) convertible.ToUInt64(null);

                    case TypeCode.Single:
                        if (Value is float)
                        {
                            return (int) Math.Round((double) ((float) Value));
                        }
                        return (int) Math.Round((double) convertible.ToSingle(null));

                    case TypeCode.Double:
                        if (Value is double)
                        {
                            return (int) Math.Round((double) Value);
                        }
                        return (int) Math.Round(convertible.ToDouble(null));

                    case TypeCode.Decimal:
                        if (Value is decimal)
                        {
                            return convertible.ToInt32(null);
                        }
                        return Convert.ToInt32(convertible.ToDecimal(null));

                    case TypeCode.String:
                    {
                        string str = Value as string;
                        if (str != null)
                        {
                            return ToInteger(str);
                        }
                        return ToInteger(convertible.ToString(null));
                    }
                }
            }
            throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(Value), "Integer" }));
        }

        public static int ToInteger(string Value)
        {
            int num;
            if (Value == null)
            {
                return 0;
            }
            try
            {
                long num2;
                if (Utils.IsHexOrOctValue(Value, ref num2))
                {
                    return (int) num2;
                }
                num = (int) Math.Round(ParseDouble(Value));
            }
            catch (FormatException exception)
            {
                throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", new string[] { Strings.Left(Value, 0x20), "Integer" }), exception);
            }
            return num;
        }

        public static long ToLong(object Value)
        {
            if (Value == null)
            {
                return 0L;
            }
            IConvertible convertible = Value as IConvertible;
            if (convertible != null)
            {
                switch (convertible.GetTypeCode())
                {
                    case TypeCode.Boolean:
                        if (Value is bool)
                        {
                            return (long) -(((bool) Value) > false);
                        }
                        return (long) -(convertible.ToBoolean(null) > false);

                    case TypeCode.SByte:
                        if (Value is sbyte)
                        {
                            return (long) ((sbyte) Value);
                        }
                        return (long) convertible.ToSByte(null);

                    case TypeCode.Byte:
                        if (Value is byte)
                        {
                            return (long) ((byte) Value);
                        }
                        return (long) convertible.ToByte(null);

                    case TypeCode.Int16:
                        if (Value is short)
                        {
                            return (long) ((short) Value);
                        }
                        return (long) convertible.ToInt16(null);

                    case TypeCode.UInt16:
                        if (Value is ushort)
                        {
                            return (long) ((ushort) Value);
                        }
                        return (long) convertible.ToUInt16(null);

                    case TypeCode.Int32:
                        if (Value is int)
                        {
                            return (long) ((int) Value);
                        }
                        return (long) convertible.ToInt32(null);

                    case TypeCode.UInt32:
                        if (Value is uint)
                        {
                            return (long) ((uint) Value);
                        }
                        return (long) convertible.ToUInt32(null);

                    case TypeCode.Int64:
                        if (Value is long)
                        {
                            return (long) Value;
                        }
                        return convertible.ToInt64(null);

                    case TypeCode.UInt64:
                        if (Value is ulong)
                        {
                            return (long) ((ulong) Value);
                        }
                        return (long) convertible.ToUInt64(null);

                    case TypeCode.Single:
                        if (Value is float)
                        {
                            return (long) Math.Round((double) ((float) Value));
                        }
                        return (long) Math.Round((double) convertible.ToSingle(null));

                    case TypeCode.Double:
                        if (Value is double)
                        {
                            return (long) Math.Round((double) Value);
                        }
                        return (long) Math.Round(convertible.ToDouble(null));

                    case TypeCode.Decimal:
                        if (Value is decimal)
                        {
                            return convertible.ToInt64(null);
                        }
                        return Convert.ToInt64(convertible.ToDecimal(null));

                    case TypeCode.String:
                    {
                        string str = Value as string;
                        if (str != null)
                        {
                            return ToLong(str);
                        }
                        return ToLong(convertible.ToString(null));
                    }
                }
            }
            throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(Value), "Long" }));
        }

        public static long ToLong(string Value)
        {
            long num;
            if (Value == null)
            {
                return 0L;
            }
            try
            {
                long num2;
                if (Utils.IsHexOrOctValue(Value, ref num2))
                {
                    return num2;
                }
                num = Convert.ToInt64(ParseDecimal(Value, null));
            }
            catch (FormatException exception)
            {
                throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", new string[] { Strings.Left(Value, 0x20), "Long" }), exception);
            }
            return num;
        }

        [CLSCompliant(false)]
        public static sbyte ToSByte(object Value)
        {
            if (Value == null)
            {
                return 0;
            }
            IConvertible convertible = Value as IConvertible;
            if (convertible != null)
            {
                switch (convertible.GetTypeCode())
                {
                    case TypeCode.Boolean:
                        if (Value is bool)
                        {
                            return (sbyte) -(((bool) Value) > false);
                        }
                        return (sbyte) -(convertible.ToBoolean(null) > false);

                    case TypeCode.SByte:
                        if (Value is sbyte)
                        {
                            return (sbyte) Value;
                        }
                        return convertible.ToSByte(null);

                    case TypeCode.Byte:
                        if (Value is byte)
                        {
                            return (sbyte) ((byte) Value);
                        }
                        return (sbyte) convertible.ToByte(null);

                    case TypeCode.Int16:
                        if (Value is short)
                        {
                            return (sbyte) ((short) Value);
                        }
                        return (sbyte) convertible.ToInt16(null);

                    case TypeCode.UInt16:
                        if (Value is ushort)
                        {
                            return (sbyte) ((ushort) Value);
                        }
                        return (sbyte) convertible.ToUInt16(null);

                    case TypeCode.Int32:
                        if (Value is int)
                        {
                            return (sbyte) ((int) Value);
                        }
                        return (sbyte) convertible.ToInt32(null);

                    case TypeCode.UInt32:
                        if (Value is uint)
                        {
                            return (sbyte) ((uint) Value);
                        }
                        return (sbyte) convertible.ToUInt32(null);

                    case TypeCode.Int64:
                        if (Value is long)
                        {
                            return (sbyte) ((long) Value);
                        }
                        return (sbyte) convertible.ToInt64(null);

                    case TypeCode.UInt64:
                        if (Value is ulong)
                        {
                            return (sbyte) ((ulong) Value);
                        }
                        return (sbyte) convertible.ToUInt64(null);

                    case TypeCode.Single:
                        if (Value is float)
                        {
                            return (sbyte) Math.Round((double) ((float) Value));
                        }
                        return (sbyte) Math.Round((double) convertible.ToSingle(null));

                    case TypeCode.Double:
                        if (Value is double)
                        {
                            return (sbyte) Math.Round((double) Value);
                        }
                        return (sbyte) Math.Round(convertible.ToDouble(null));

                    case TypeCode.Decimal:
                        if (Value is decimal)
                        {
                            return convertible.ToSByte(null);
                        }
                        return Convert.ToSByte(convertible.ToDecimal(null));

                    case TypeCode.String:
                    {
                        string str = Value as string;
                        if (str != null)
                        {
                            return ToSByte(str);
                        }
                        return ToSByte(convertible.ToString(null));
                    }
                }
            }
            throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(Value), "SByte" }));
        }

        [CLSCompliant(false)]
        public static sbyte ToSByte(string Value)
        {
            sbyte num;
            if (Value == null)
            {
                return 0;
            }
            try
            {
                long num2;
                if (Utils.IsHexOrOctValue(Value, ref num2))
                {
                    return (sbyte) num2;
                }
                num = (sbyte) Math.Round(ParseDouble(Value));
            }
            catch (FormatException exception)
            {
                throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", new string[] { Strings.Left(Value, 0x20), "SByte" }), exception);
            }
            return num;
        }

        public static short ToShort(object Value)
        {
            if (Value == null)
            {
                return 0;
            }
            IConvertible convertible = Value as IConvertible;
            if (convertible != null)
            {
                switch (convertible.GetTypeCode())
                {
                    case TypeCode.Boolean:
                        if (Value is bool)
                        {
                            return (short) -(((bool) Value) > false);
                        }
                        return (short) -(convertible.ToBoolean(null) > false);

                    case TypeCode.SByte:
                        if (Value is sbyte)
                        {
                            return (sbyte) Value;
                        }
                        return convertible.ToSByte(null);

                    case TypeCode.Byte:
                        if (Value is byte)
                        {
                            return (byte) Value;
                        }
                        return convertible.ToByte(null);

                    case TypeCode.Int16:
                        if (Value is short)
                        {
                            return (short) Value;
                        }
                        return convertible.ToInt16(null);

                    case TypeCode.UInt16:
                        if (Value is ushort)
                        {
                            return (short) ((ushort) Value);
                        }
                        return (short) convertible.ToUInt16(null);

                    case TypeCode.Int32:
                        if (Value is int)
                        {
                            return (short) ((int) Value);
                        }
                        return (short) convertible.ToInt32(null);

                    case TypeCode.UInt32:
                        if (Value is uint)
                        {
                            return (short) ((uint) Value);
                        }
                        return (short) convertible.ToUInt32(null);

                    case TypeCode.Int64:
                        if (Value is long)
                        {
                            return (short) ((long) Value);
                        }
                        return (short) convertible.ToInt64(null);

                    case TypeCode.UInt64:
                        if (Value is ulong)
                        {
                            return (short) ((ulong) Value);
                        }
                        return (short) convertible.ToUInt64(null);

                    case TypeCode.Single:
                        if (Value is float)
                        {
                            return (short) Math.Round((double) ((float) Value));
                        }
                        return (short) Math.Round((double) convertible.ToSingle(null));

                    case TypeCode.Double:
                        if (Value is double)
                        {
                            return (short) Math.Round((double) Value);
                        }
                        return (short) Math.Round(convertible.ToDouble(null));

                    case TypeCode.Decimal:
                        if (Value is decimal)
                        {
                            return convertible.ToInt16(null);
                        }
                        return Convert.ToInt16(convertible.ToDecimal(null));

                    case TypeCode.String:
                    {
                        string str = Value as string;
                        if (str != null)
                        {
                            return ToShort(str);
                        }
                        return ToShort(convertible.ToString(null));
                    }
                }
            }
            throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(Value), "Short" }));
        }

        public static short ToShort(string Value)
        {
            short num;
            if (Value == null)
            {
                return 0;
            }
            try
            {
                long num2;
                if (Utils.IsHexOrOctValue(Value, ref num2))
                {
                    return (short) num2;
                }
                num = (short) Math.Round(ParseDouble(Value));
            }
            catch (FormatException exception)
            {
                throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", new string[] { Strings.Left(Value, 0x20), "Short" }), exception);
            }
            return num;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static float ToSingle(object Value)
        {
            return ToSingle(Value, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static float ToSingle(string Value)
        {
            return ToSingle(Value, null);
        }

        internal static float ToSingle(object Value, NumberFormatInfo NumberFormat)
        {
            if (Value == null)
            {
                return 0f;
            }
            IConvertible convertible = Value as IConvertible;
            if (convertible != null)
            {
                switch (convertible.GetTypeCode())
                {
                    case TypeCode.Boolean:
                        if (Value is bool)
                        {
                            return (float) -(((bool) Value) > false);
                        }
                        return (float) -(convertible.ToBoolean(null) > false);

                    case TypeCode.SByte:
                        if (Value is sbyte)
                        {
                            return (float) ((sbyte) Value);
                        }
                        return (float) convertible.ToSByte(null);

                    case TypeCode.Byte:
                        if (Value is byte)
                        {
                            return (float) ((byte) Value);
                        }
                        return (float) convertible.ToByte(null);

                    case TypeCode.Int16:
                        if (Value is short)
                        {
                            return (float) ((short) Value);
                        }
                        return (float) convertible.ToInt16(null);

                    case TypeCode.UInt16:
                        if (Value is ushort)
                        {
                            return (float) ((ushort) Value);
                        }
                        return (float) convertible.ToUInt16(null);

                    case TypeCode.Int32:
                        if (Value is int)
                        {
                            return (float) ((int) Value);
                        }
                        return (float) convertible.ToInt32(null);

                    case TypeCode.UInt32:
                        if (Value is uint)
                        {
                            return (float) ((uint) Value);
                        }
                        return (float) convertible.ToUInt32(null);

                    case TypeCode.Int64:
                        if (Value is long)
                        {
                            return (float) ((long) Value);
                        }
                        return (float) convertible.ToInt64(null);

                    case TypeCode.UInt64:
                        if (Value is ulong)
                        {
                            return (float) ((ulong) Value);
                        }
                        return (float) convertible.ToUInt64(null);

                    case TypeCode.Single:
                        if (Value is float)
                        {
                            return (float) Value;
                        }
                        return convertible.ToSingle(null);

                    case TypeCode.Double:
                        if (Value is double)
                        {
                            return (float) ((double) Value);
                        }
                        return (float) convertible.ToDouble(null);

                    case TypeCode.Decimal:
                        if (Value is decimal)
                        {
                            return convertible.ToSingle(null);
                        }
                        return Convert.ToSingle(convertible.ToDecimal(null));

                    case TypeCode.String:
                        return ToSingle(convertible.ToString(null), NumberFormat);
                }
            }
            throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(Value), "Single" }));
        }

        internal static float ToSingle(string Value, NumberFormatInfo NumberFormat)
        {
            float num;
            if (Value == null)
            {
                return 0f;
            }
            try
            {
                long num2;
                if (Utils.IsHexOrOctValue(Value, ref num2))
                {
                    return (float) num2;
                }
                double d = ParseDouble(Value, NumberFormat);
                if (((d < -3.4028234663852886E+38) || (d > 3.4028234663852886E+38)) && !double.IsInfinity(d))
                {
                    throw new OverflowException();
                }
                num = (float) d;
            }
            catch (FormatException exception)
            {
                throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", new string[] { Strings.Left(Value, 0x20), "Single" }), exception);
            }
            return num;
        }

        public static string ToString(bool Value)
        {
            if (Value)
            {
                return bool.TrueString;
            }
            return bool.FalseString;
        }

        public static string ToString(byte Value)
        {
            return Value.ToString(null, null);
        }

        public static string ToString(char Value)
        {
            return Value.ToString();
        }

        public static string ToString(DateTime Value)
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
        public static string ToString(decimal Value)
        {
            return ToString(Value, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static string ToString(double Value)
        {
            return ToString(Value, null);
        }

        public static string ToString(short Value)
        {
            return Value.ToString(null, (IFormatProvider) null);
        }

        public static string ToString(int Value)
        {
            return Value.ToString(null, null);
        }

        public static string ToString(long Value)
        {
            return Value.ToString(null, null);
        }

        public static string ToString(object Value)
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
                        return ToString(convertible.ToBoolean(null));

                    case TypeCode.Char:
                        return ToString(convertible.ToChar(null));

                    case TypeCode.SByte:
                        return ToString((int) convertible.ToSByte(null));

                    case TypeCode.Byte:
                        return ToString(convertible.ToByte(null));

                    case TypeCode.Int16:
                        return ToString((int) convertible.ToInt16(null));

                    case TypeCode.UInt16:
                        return ToString((uint) convertible.ToUInt16(null));

                    case TypeCode.Int32:
                        return ToString(convertible.ToInt32(null));

                    case TypeCode.UInt32:
                        return ToString(convertible.ToUInt32(null));

                    case TypeCode.Int64:
                        return ToString(convertible.ToInt64(null));

                    case TypeCode.UInt64:
                        return ToString(convertible.ToUInt64(null));

                    case TypeCode.Single:
                        return ToString(convertible.ToSingle(null));

                    case TypeCode.Double:
                        return ToString(convertible.ToDouble(null));

                    case TypeCode.Decimal:
                        return ToString(convertible.ToDecimal(null));

                    case TypeCode.DateTime:
                        return ToString(convertible.ToDateTime(null));

                    case TypeCode.String:
                        return convertible.ToString(null);
                }
            }
            else
            {
                char[] chArray = Value as char[];
                if (chArray != null)
                {
                    return new string(chArray);
                }
            }
            throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(Value), "String" }));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static string ToString(float Value)
        {
            return ToString(Value, null);
        }

        [CLSCompliant(false)]
        public static string ToString(uint Value)
        {
            return Value.ToString(null, null);
        }

        [CLSCompliant(false)]
        public static string ToString(ulong Value)
        {
            return Value.ToString(null, null);
        }

        public static string ToString(decimal Value, NumberFormatInfo NumberFormat)
        {
            return Value.ToString("G", NumberFormat);
        }

        public static string ToString(double Value, NumberFormatInfo NumberFormat)
        {
            return Value.ToString("G", NumberFormat);
        }

        public static string ToString(float Value, NumberFormatInfo NumberFormat)
        {
            return Value.ToString(null, NumberFormat);
        }

        [CLSCompliant(false)]
        public static uint ToUInteger(object Value)
        {
            if (Value == null)
            {
                return 0;
            }
            IConvertible convertible = Value as IConvertible;
            if (convertible != null)
            {
                switch (convertible.GetTypeCode())
                {
                    case TypeCode.Boolean:
                        if (Value is bool)
                        {
                            return (uint) -(((bool) Value) > false);
                        }
                        return (uint) -(convertible.ToBoolean(null) > false);

                    case TypeCode.SByte:
                        if (Value is sbyte)
                        {
                            return (uint) ((sbyte) Value);
                        }
                        return (uint) convertible.ToSByte(null);

                    case TypeCode.Byte:
                        if (Value is byte)
                        {
                            return (byte) Value;
                        }
                        return convertible.ToByte(null);

                    case TypeCode.Int16:
                        if (Value is short)
                        {
                            return (uint) ((short) Value);
                        }
                        return (uint) convertible.ToInt16(null);

                    case TypeCode.UInt16:
                        if (Value is ushort)
                        {
                            return (ushort) Value;
                        }
                        return convertible.ToUInt16(null);

                    case TypeCode.Int32:
                        if (Value is int)
                        {
                            return (uint) ((int) Value);
                        }
                        return (uint) convertible.ToInt32(null);

                    case TypeCode.UInt32:
                        if (Value is uint)
                        {
                            return (uint) Value;
                        }
                        return convertible.ToUInt32(null);

                    case TypeCode.Int64:
                        if (Value is long)
                        {
                            return (uint) ((long) Value);
                        }
                        return (uint) convertible.ToInt64(null);

                    case TypeCode.UInt64:
                        if (Value is ulong)
                        {
                            return (uint) ((ulong) Value);
                        }
                        return (uint) convertible.ToUInt64(null);

                    case TypeCode.Single:
                        if (Value is float)
                        {
                            return (uint) Math.Round((double) ((float) Value));
                        }
                        return (uint) Math.Round((double) convertible.ToSingle(null));

                    case TypeCode.Double:
                        if (Value is double)
                        {
                            return (uint) Math.Round((double) Value);
                        }
                        return (uint) Math.Round(convertible.ToDouble(null));

                    case TypeCode.Decimal:
                        if (Value is decimal)
                        {
                            return convertible.ToUInt32(null);
                        }
                        return Convert.ToUInt32(convertible.ToDecimal(null));

                    case TypeCode.String:
                    {
                        string str = Value as string;
                        if (str != null)
                        {
                            return ToUInteger(str);
                        }
                        return ToUInteger(convertible.ToString(null));
                    }
                }
            }
            throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(Value), "UInteger" }));
        }

        [CLSCompliant(false)]
        public static uint ToUInteger(string Value)
        {
            uint num;
            if (Value == null)
            {
                return 0;
            }
            try
            {
                long num2;
                if (Utils.IsHexOrOctValue(Value, ref num2))
                {
                    return (uint) num2;
                }
                num = (uint) Math.Round(ParseDouble(Value));
            }
            catch (FormatException exception)
            {
                throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", new string[] { Strings.Left(Value, 0x20), "UInteger" }), exception);
            }
            return num;
        }

        [CLSCompliant(false)]
        public static ulong ToULong(object Value)
        {
            if (Value == null)
            {
                return 0L;
            }
            IConvertible convertible = Value as IConvertible;
            if (convertible != null)
            {
                switch (convertible.GetTypeCode())
                {
                    case TypeCode.Boolean:
                        if (Value is bool)
                        {
                            return (ulong) ((long) -(((bool) Value) > false));
                        }
                        return (ulong) ((long) -(convertible.ToBoolean(null) > false));

                    case TypeCode.SByte:
                        if (Value is sbyte)
                        {
                            return (ulong) ((sbyte) Value);
                        }
                        return (ulong) convertible.ToSByte(null);

                    case TypeCode.Byte:
                        if (Value is byte)
                        {
                            return (ulong) ((byte) Value);
                        }
                        return (ulong) convertible.ToByte(null);

                    case TypeCode.Int16:
                        if (Value is short)
                        {
                            return (ulong) ((short) Value);
                        }
                        return (ulong) convertible.ToInt16(null);

                    case TypeCode.UInt16:
                        if (Value is ushort)
                        {
                            return (ulong) ((ushort) Value);
                        }
                        return (ulong) convertible.ToUInt16(null);

                    case TypeCode.Int32:
                        if (Value is int)
                        {
                            return (ulong) ((int) Value);
                        }
                        return (ulong) convertible.ToInt32(null);

                    case TypeCode.UInt32:
                        if (Value is uint)
                        {
                            return (ulong) ((uint) Value);
                        }
                        return (ulong) convertible.ToUInt32(null);

                    case TypeCode.Int64:
                        if (Value is long)
                        {
                            return (ulong) ((long) Value);
                        }
                        return (ulong) convertible.ToInt64(null);

                    case TypeCode.UInt64:
                        if (Value is ulong)
                        {
                            return (ulong) Value;
                        }
                        return convertible.ToUInt64(null);

                    case TypeCode.Single:
                        if (Value is float)
                        {
                            return (ulong) Math.Round((double) ((float) Value));
                        }
                        return (ulong) Math.Round((double) convertible.ToSingle(null));

                    case TypeCode.Double:
                        if (Value is double)
                        {
                            return (ulong) Math.Round((double) Value);
                        }
                        return (ulong) Math.Round(convertible.ToDouble(null));

                    case TypeCode.Decimal:
                        if (Value is decimal)
                        {
                            return convertible.ToUInt64(null);
                        }
                        return Convert.ToUInt64(convertible.ToDecimal(null));

                    case TypeCode.String:
                    {
                        string str = Value as string;
                        if (str != null)
                        {
                            return ToULong(str);
                        }
                        return ToULong(convertible.ToString(null));
                    }
                }
            }
            throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(Value), "ULong" }));
        }

        [CLSCompliant(false)]
        public static ulong ToULong(string Value)
        {
            ulong num;
            if (Value == null)
            {
                return 0L;
            }
            try
            {
                ulong num2;
                if (Utils.IsHexOrOctValue(Value, ref num2))
                {
                    return num2;
                }
                num = Convert.ToUInt64(ParseDecimal(Value, null));
            }
            catch (FormatException exception)
            {
                throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", new string[] { Strings.Left(Value, 0x20), "ULong" }), exception);
            }
            return num;
        }

        [CLSCompliant(false)]
        public static ushort ToUShort(object Value)
        {
            if (Value == null)
            {
                return 0;
            }
            IConvertible convertible = Value as IConvertible;
            if (convertible != null)
            {
                switch (convertible.GetTypeCode())
                {
                    case TypeCode.Boolean:
                        if (Value is bool)
                        {
                            return (ushort) -(((bool) Value) > false);
                        }
                        return (ushort) -(convertible.ToBoolean(null) > false);

                    case TypeCode.SByte:
                        if (Value is sbyte)
                        {
                            return (ushort) ((sbyte) Value);
                        }
                        return (ushort) convertible.ToSByte(null);

                    case TypeCode.Byte:
                        if (Value is byte)
                        {
                            return (byte) Value;
                        }
                        return convertible.ToByte(null);

                    case TypeCode.Int16:
                        if (Value is short)
                        {
                            return (ushort) ((short) Value);
                        }
                        return (ushort) convertible.ToInt16(null);

                    case TypeCode.UInt16:
                        if (Value is ushort)
                        {
                            return (ushort) Value;
                        }
                        return convertible.ToUInt16(null);

                    case TypeCode.Int32:
                        if (Value is int)
                        {
                            return (ushort) ((int) Value);
                        }
                        return (ushort) convertible.ToInt32(null);

                    case TypeCode.UInt32:
                        if (Value is uint)
                        {
                            return (ushort) ((uint) Value);
                        }
                        return (ushort) convertible.ToUInt32(null);

                    case TypeCode.Int64:
                        if (Value is long)
                        {
                            return (ushort) ((long) Value);
                        }
                        return (ushort) convertible.ToInt64(null);

                    case TypeCode.UInt64:
                        if (Value is ulong)
                        {
                            return (ushort) ((ulong) Value);
                        }
                        return (ushort) convertible.ToUInt64(null);

                    case TypeCode.Single:
                        if (Value is float)
                        {
                            return (ushort) Math.Round((double) ((float) Value));
                        }
                        return (ushort) Math.Round((double) convertible.ToSingle(null));

                    case TypeCode.Double:
                        if (Value is double)
                        {
                            return (ushort) Math.Round((double) Value);
                        }
                        return (ushort) Math.Round(convertible.ToDouble(null));

                    case TypeCode.Decimal:
                        if (Value is decimal)
                        {
                            return convertible.ToUInt16(null);
                        }
                        return Convert.ToUInt16(convertible.ToDecimal(null));

                    case TypeCode.String:
                    {
                        string str = Value as string;
                        if (str != null)
                        {
                            return ToUShort(str);
                        }
                        return ToUShort(convertible.ToString(null));
                    }
                }
            }
            throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(Value), "UShort" }));
        }

        [CLSCompliant(false)]
        public static ushort ToUShort(string Value)
        {
            ushort num;
            if (Value == null)
            {
                return 0;
            }
            try
            {
                long num2;
                if (Utils.IsHexOrOctValue(Value, ref num2))
                {
                    return (ushort) num2;
                }
                num = (ushort) Math.Round(ParseDouble(Value));
            }
            catch (FormatException exception)
            {
                throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromStringTo", new string[] { Strings.Left(Value, 0x20), "UShort" }), exception);
            }
            return num;
        }

        internal static bool TryParseDate(string Value, ref DateTime Result)
        {
            CultureInfo cultureInfo = Utils.GetCultureInfo();
            return DateTime.TryParse(Utils.ToHalfwidthNumbers(Value, cultureInfo), cultureInfo, DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AllowWhiteSpaces, out Result);
        }

        internal static bool TryParseDouble(string Value, ref double Result)
        {
            bool flag;
            CultureInfo cultureInfo = Utils.GetCultureInfo();
            NumberFormatInfo numberFormat = cultureInfo.NumberFormat;
            NumberFormatInfo normalizedNumberFormat = GetNormalizedNumberFormat(numberFormat);
            Value = Utils.ToHalfwidthNumbers(Value, cultureInfo);
            if (numberFormat == normalizedNumberFormat)
            {
                return double.TryParse(Value, NumberStyles.Any, (IFormatProvider) normalizedNumberFormat, out Result);
            }
            try
            {
                Result = double.Parse(Value, NumberStyles.Any, (IFormatProvider) normalizedNumberFormat);
                flag = true;
            }
            catch (FormatException)
            {
                try
                {
                    flag = double.TryParse(Value, NumberStyles.Any, (IFormatProvider) numberFormat, out Result);
                }
                catch (ArgumentException)
                {
                    flag = false;
                }
            }
            catch (StackOverflowException exception3)
            {
                throw exception3;
            }
            catch (OutOfMemoryException exception4)
            {
                throw exception4;
            }
            catch (ThreadAbortException exception5)
            {
                throw exception5;
            }
            catch (Exception)
            {
                flag = false;
            }
            return flag;
        }
    }
}

