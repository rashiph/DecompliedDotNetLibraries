namespace System.Runtime
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal static class TypeHelper
    {
        public static readonly Type ArrayType = typeof(Array);
        public static readonly Type BoolType = typeof(bool);
        public static readonly Type ByteType = typeof(byte);
        public static readonly Type CharType = typeof(char);
        public static readonly Type DecimalType = typeof(decimal);
        public static readonly Type DoubleType = typeof(double);
        public static readonly Type ExceptionType = typeof(Exception);
        public static readonly Type FloatType = typeof(float);
        public static readonly Type GenericCollectionType = typeof(ICollection<>);
        public static readonly Type IntType = typeof(int);
        public static readonly Type LongType = typeof(long);
        public static readonly Type NullableType = typeof(Nullable<>);
        public static readonly Type ObjectType = typeof(object);
        public static readonly Type SByteType = typeof(sbyte);
        public static readonly Type ShortType = typeof(short);
        public static readonly Type StringType = typeof(string);
        public static readonly Type TypeType = typeof(Type);
        public static readonly Type UIntType = typeof(uint);
        public static readonly Type ULongType = typeof(ulong);
        public static readonly Type UShortType = typeof(ushort);
        public static readonly Type VoidType = typeof(void);

        public static bool AreReferenceTypesCompatible(Type sourceType, Type destinationType)
        {
            return (object.ReferenceEquals(sourceType, destinationType) || IsImplicitReferenceConversion(sourceType, destinationType));
        }

        public static bool AreTypesCompatible(object source, Type destinationType)
        {
            if (source != null)
            {
                return AreTypesCompatible(source.GetType(), destinationType);
            }
            if (destinationType.IsValueType)
            {
                return IsNullableType(destinationType);
            }
            return true;
        }

        public static bool AreTypesCompatible(Type sourceType, Type destinationType)
        {
            if (!object.ReferenceEquals(sourceType, destinationType) && ((!IsImplicitNumericConversion(sourceType, destinationType) && !IsImplicitReferenceConversion(sourceType, destinationType)) && !IsImplicitBoxingConversion(sourceType, destinationType)))
            {
                return IsImplicitNullableConversion(sourceType, destinationType);
            }
            return true;
        }

        public static bool ContainsCompatibleType(IEnumerable<Type> enumerable, Type targetType)
        {
            foreach (Type type in enumerable)
            {
                if (AreTypesCompatible(type, targetType))
                {
                    return true;
                }
            }
            return false;
        }

        public static T Convert<T>(object source)
        {
            T local;
            if (source is T)
            {
                return (T) source;
            }
            if (source == null)
            {
                if (typeof(T).IsValueType && !IsNullableType(typeof(T)))
                {
                    throw Fx.Exception.AsError(new InvalidCastException(SRCore.CannotConvertObject(source, typeof(T))));
                }
                return default(T);
            }
            if (!TryNumericConversion<T>(source, out local))
            {
                throw Fx.Exception.AsError(new InvalidCastException(SRCore.CannotConvertObject(source, typeof(T))));
            }
            return local;
        }

        public static IEnumerable<Type> GetCompatibleTypes(IEnumerable<Type> enumerable, Type targetType)
        {
            foreach (Type iteratorVariable0 in enumerable)
            {
                if (AreTypesCompatible(iteratorVariable0, targetType))
                {
                    yield return iteratorVariable0;
                }
            }
        }

        public static object GetDefaultValueForType(Type type)
        {
            if (!type.IsValueType)
            {
                return null;
            }
            if (type.IsEnum)
            {
                Array values = Enum.GetValues(type);
                if (values.Length > 0)
                {
                    return values.GetValue(0);
                }
            }
            return Activator.CreateInstance(type);
        }

        public static IEnumerable<Type> GetImplementedTypes(Type type)
        {
            HashSet<Type> typesEncountered = new HashSet<Type>();
            GetImplementedTypesHelper(type, typesEncountered);
            return typesEncountered;
        }

        private static void GetImplementedTypesHelper(Type type, HashSet<Type> typesEncountered)
        {
            if (!typesEncountered.Contains(type))
            {
                typesEncountered.Add(type);
                Type[] interfaces = type.GetInterfaces();
                for (int i = 0; i < interfaces.Length; i++)
                {
                    GetImplementedTypesHelper(interfaces[i], typesEncountered);
                }
                for (Type type2 = type.BaseType; (type2 != null) && (type2 != ObjectType); type2 = type2.BaseType)
                {
                    GetImplementedTypesHelper(type2, typesEncountered);
                }
            }
        }

        private static bool IsImplicitBoxingConversion(Type sourceType, Type destinationType)
        {
            return ((sourceType.IsValueType && ((destinationType == ObjectType) || (destinationType == typeof(ValueType)))) || (sourceType.IsEnum && (destinationType == typeof(Enum))));
        }

        private static bool IsImplicitNullableConversion(Type sourceType, Type destinationType)
        {
            if (!IsNullableType(destinationType))
            {
                return false;
            }
            destinationType = destinationType.GetGenericArguments()[0];
            if (IsNullableType(sourceType))
            {
                sourceType = sourceType.GetGenericArguments()[0];
            }
            return AreTypesCompatible(sourceType, destinationType);
        }

        private static bool IsImplicitNumericConversion(Type source, Type destination)
        {
            TypeCode typeCode = Type.GetTypeCode(source);
            TypeCode code2 = Type.GetTypeCode(destination);
            switch (typeCode)
            {
                case TypeCode.Char:
                    switch (code2)
                    {
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.SByte:
                    switch (code2)
                    {
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    break;

                case TypeCode.Byte:
                    switch (code2)
                    {
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.Int16:
                    switch (code2)
                    {
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.UInt16:
                    switch (code2)
                    {
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.Int32:
                    switch (code2)
                    {
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.UInt32:
                    switch (code2)
                    {
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.Int64:
                case TypeCode.UInt64:
                    switch (code2)
                    {
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.Single:
                    return (code2 == TypeCode.Double);

                default:
                    return false;
            }
            return false;
        }

        private static bool IsImplicitReferenceConversion(Type sourceType, Type destinationType)
        {
            return destinationType.IsAssignableFrom(sourceType);
        }

        public static bool IsNonNullableValueType(Type type)
        {
            if (!type.IsValueType)
            {
                return false;
            }
            if (type.IsGenericType)
            {
                return false;
            }
            return (type != StringType);
        }

        private static bool IsNullableType(Type type)
        {
            return (type.IsGenericType && (type.GetGenericTypeDefinition() == NullableType));
        }

        public static bool IsNullableValueType(Type type)
        {
            return (type.IsValueType && IsNullableType(type));
        }

        public static bool ShouldFilterProperty(PropertyDescriptor property, Attribute[] attributes)
        {
            if ((attributes != null) && (attributes.Length != 0))
            {
                for (int i = 0; i < attributes.Length; i++)
                {
                    Attribute attribute = attributes[i];
                    Attribute attribute2 = property.Attributes[attribute.GetType()];
                    if (attribute2 == null)
                    {
                        if (!attribute.IsDefaultAttribute())
                        {
                            return true;
                        }
                    }
                    else if (!attribute.Match(attribute2))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool TryNumericConversion<T>(object source, out T result)
        {
            TypeCode typeCode = Type.GetTypeCode(source.GetType());
            TypeCode code2 = Type.GetTypeCode(typeof(T));
            switch (typeCode)
            {
                case TypeCode.Char:
                {
                    char ch = (char) source;
                    switch (code2)
                    {
                        case TypeCode.UInt16:
                            result = (T) ch;
                            return true;

                        case TypeCode.Int32:
                            result = (T) ch;
                            return true;

                        case TypeCode.UInt32:
                            result = (T) ch;
                            return true;

                        case TypeCode.Int64:
                            result = (T) ((long) ch);
                            return true;

                        case TypeCode.UInt64:
                            result = (T) ch;
                            return true;

                        case TypeCode.Single:
                            result = (T) ((float) ch);
                            return true;

                        case TypeCode.Double:
                            result = (T) ((double) ch);
                            return true;

                        case TypeCode.Decimal:
                            result = (T) ch;
                            return true;
                    }
                    break;
                }
                case TypeCode.SByte:
                {
                    sbyte num = (sbyte) source;
                    switch (code2)
                    {
                        case TypeCode.Int16:
                            result = (T) num;
                            return true;

                        case TypeCode.Int32:
                            result = (T) num;
                            return true;

                        case TypeCode.Int64:
                            result = (T) num;
                            return true;

                        case TypeCode.Single:
                            result = (T) num;
                            return true;

                        case TypeCode.Double:
                            result = (T) num;
                            return true;

                        case TypeCode.Decimal:
                            result = (T) num;
                            return true;
                    }
                    break;
                }
                case TypeCode.Byte:
                {
                    byte num2 = (byte) source;
                    switch (code2)
                    {
                        case TypeCode.Int16:
                            result = (T) num2;
                            return true;

                        case TypeCode.UInt16:
                            result = (T) num2;
                            return true;

                        case TypeCode.Int32:
                            result = (T) num2;
                            return true;

                        case TypeCode.UInt32:
                            result = (T) num2;
                            return true;

                        case TypeCode.Int64:
                            result = (T) num2;
                            return true;

                        case TypeCode.UInt64:
                            result = (T) num2;
                            return true;

                        case TypeCode.Single:
                            result = (T) num2;
                            return true;

                        case TypeCode.Double:
                            result = (T) num2;
                            return true;

                        case TypeCode.Decimal:
                            result = (T) num2;
                            return true;
                    }
                    break;
                }
                case TypeCode.Int16:
                {
                    short num3 = (short) source;
                    switch (code2)
                    {
                        case TypeCode.Int32:
                            result = (T) num3;
                            return true;

                        case TypeCode.Int64:
                            result = (T) num3;
                            return true;

                        case TypeCode.Single:
                            result = (T) num3;
                            return true;

                        case TypeCode.Double:
                            result = (T) num3;
                            return true;

                        case TypeCode.Decimal:
                            result = (T) num3;
                            return true;
                    }
                    break;
                }
                case TypeCode.UInt16:
                {
                    ushort num4 = (ushort) source;
                    switch (code2)
                    {
                        case TypeCode.Int32:
                            result = (T) num4;
                            return true;

                        case TypeCode.UInt32:
                            result = (T) num4;
                            return true;

                        case TypeCode.Int64:
                            result = (T) num4;
                            return true;

                        case TypeCode.UInt64:
                            result = (T) num4;
                            return true;

                        case TypeCode.Single:
                            result = (T) num4;
                            return true;

                        case TypeCode.Double:
                            result = (T) num4;
                            return true;

                        case TypeCode.Decimal:
                            result = (T) num4;
                            return true;
                    }
                    break;
                }
                case TypeCode.Int32:
                {
                    int num5 = (int) source;
                    switch (code2)
                    {
                        case TypeCode.Int64:
                            result = (T) num5;
                            return true;

                        case TypeCode.Single:
                            result = (T) num5;
                            return true;

                        case TypeCode.Double:
                            result = (T) num5;
                            return true;

                        case TypeCode.Decimal:
                            result = (T) num5;
                            return true;
                    }
                    break;
                }
                case TypeCode.UInt32:
                {
                    uint num6 = (uint) source;
                    switch (code2)
                    {
                        case TypeCode.UInt32:
                            result = (T) num6;
                            return true;

                        case TypeCode.Int64:
                            result = (T) num6;
                            return true;

                        case TypeCode.UInt64:
                            result = (T) num6;
                            return true;

                        case TypeCode.Single:
                            result = (T) num6;
                            return true;

                        case TypeCode.Double:
                            result = (T) num6;
                            return true;

                        case TypeCode.Decimal:
                            result = (T) num6;
                            return true;
                    }
                    break;
                }
                case TypeCode.Int64:
                {
                    long num7 = (long) source;
                    switch (code2)
                    {
                        case TypeCode.Single:
                            result = (T) num7;
                            return true;

                        case TypeCode.Double:
                            result = (T) num7;
                            return true;

                        case TypeCode.Decimal:
                            result = (T) num7;
                            return true;
                    }
                    break;
                }
                case TypeCode.UInt64:
                {
                    ulong num8 = (ulong) source;
                    switch (code2)
                    {
                        case TypeCode.Single:
                            result = (T) num8;
                            return true;

                        case TypeCode.Double:
                            result = (T) num8;
                            return true;

                        case TypeCode.Decimal:
                            result = (T) num8;
                            return true;
                    }
                    break;
                }
                case TypeCode.Single:
                    if (code2 != TypeCode.Double)
                    {
                        break;
                    }
                    result = (T) ((float) source);
                    return true;
            }
            result = default(T);
            return false;
        }

    }
}

