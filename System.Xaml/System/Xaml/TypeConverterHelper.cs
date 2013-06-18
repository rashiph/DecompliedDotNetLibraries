namespace System.Xaml
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Xaml.Replacements;

    internal static class TypeConverterHelper
    {
        private static CultureInfo invariantEnglishUS = CultureInfo.ReadOnly(new CultureInfo("en-us", false));

        internal static Type GetConverterType(Type type)
        {
            Type converterType = null;
            string typeConverterAttributeData = ReflectionHelper.GetTypeConverterAttributeData(type, out converterType);
            if (converterType == null)
            {
                converterType = GetConverterTypeFromName(typeConverterAttributeData);
            }
            return converterType;
        }

        private static Type GetConverterTypeFromName(string converterName)
        {
            Type qualifiedType = null;
            if (!string.IsNullOrEmpty(converterName))
            {
                qualifiedType = ReflectionHelper.GetQualifiedType(converterName);
                if ((qualifiedType != null) && !ReflectionHelper.IsPublicType(qualifiedType))
                {
                    qualifiedType = null;
                }
            }
            return qualifiedType;
        }

        private static TypeConverter GetCoreConverterFromCoreType(Type type)
        {
            TypeConverter converter = null;
            if (type == typeof(int))
            {
                return new Int32Converter();
            }
            if (type == typeof(short))
            {
                return new Int16Converter();
            }
            if (type == typeof(long))
            {
                return new Int64Converter();
            }
            if (type == typeof(uint))
            {
                return new UInt32Converter();
            }
            if (type == typeof(ushort))
            {
                return new UInt16Converter();
            }
            if (type == typeof(ulong))
            {
                return new UInt64Converter();
            }
            if (type == typeof(bool))
            {
                return new BooleanConverter();
            }
            if (type == typeof(double))
            {
                return new DoubleConverter();
            }
            if (type == typeof(float))
            {
                return new SingleConverter();
            }
            if (type == typeof(byte))
            {
                return new ByteConverter();
            }
            if (type == typeof(sbyte))
            {
                return new SByteConverter();
            }
            if (type == typeof(char))
            {
                return new CharConverter();
            }
            if (type == typeof(decimal))
            {
                return new DecimalConverter();
            }
            if (type == typeof(TimeSpan))
            {
                return new TimeSpanConverter();
            }
            if (type == typeof(Guid))
            {
                return new GuidConverter();
            }
            if (type == typeof(string))
            {
                return new StringConverter();
            }
            if (type == typeof(CultureInfo))
            {
                return new CultureInfoConverter();
            }
            if (type == typeof(Type))
            {
                return new TypeTypeConverter();
            }
            if (type == typeof(DateTime))
            {
                return new DateTimeConverter2();
            }
            if (ReflectionHelper.IsNullableType(type))
            {
                converter = new NullableConverter(type);
            }
            return converter;
        }

        internal static TypeConverter GetCoreConverterFromCustomType(Type type)
        {
            TypeConverter converter = null;
            if (type.IsEnum)
            {
                return new EnumConverter(type);
            }
            if (typeof(int).IsAssignableFrom(type))
            {
                return new Int32Converter();
            }
            if (typeof(short).IsAssignableFrom(type))
            {
                return new Int16Converter();
            }
            if (typeof(long).IsAssignableFrom(type))
            {
                return new Int64Converter();
            }
            if (typeof(uint).IsAssignableFrom(type))
            {
                return new UInt32Converter();
            }
            if (typeof(ushort).IsAssignableFrom(type))
            {
                return new UInt16Converter();
            }
            if (typeof(ulong).IsAssignableFrom(type))
            {
                return new UInt64Converter();
            }
            if (typeof(bool).IsAssignableFrom(type))
            {
                return new BooleanConverter();
            }
            if (typeof(double).IsAssignableFrom(type))
            {
                return new DoubleConverter();
            }
            if (typeof(float).IsAssignableFrom(type))
            {
                return new SingleConverter();
            }
            if (typeof(byte).IsAssignableFrom(type))
            {
                return new ByteConverter();
            }
            if (typeof(sbyte).IsAssignableFrom(type))
            {
                return new SByteConverter();
            }
            if (typeof(char).IsAssignableFrom(type))
            {
                return new CharConverter();
            }
            if (typeof(decimal).IsAssignableFrom(type))
            {
                return new DecimalConverter();
            }
            if (typeof(TimeSpan).IsAssignableFrom(type))
            {
                return new TimeSpanConverter();
            }
            if (typeof(Guid).IsAssignableFrom(type))
            {
                return new GuidConverter();
            }
            if (typeof(string).IsAssignableFrom(type))
            {
                return new StringConverter();
            }
            if (typeof(CultureInfo).IsAssignableFrom(type))
            {
                return new CultureInfoConverter();
            }
            if (type == typeof(Type))
            {
                return new TypeTypeConverter();
            }
            if (typeof(DateTime).IsAssignableFrom(type))
            {
                converter = new DateTimeConverter2();
            }
            return converter;
        }

        internal static TypeConverter GetTypeConverter(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            TypeConverter coreConverterFromCoreType = GetCoreConverterFromCoreType(type);
            if (coreConverterFromCoreType == null)
            {
                Type converterType = GetConverterType(type);
                if (converterType != null)
                {
                    coreConverterFromCoreType = Activator.CreateInstance(converterType, BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance, null, null, InvariantEnglishUS) as TypeConverter;
                }
                else
                {
                    coreConverterFromCoreType = GetCoreConverterFromCustomType(type);
                }
                if (coreConverterFromCoreType == null)
                {
                    coreConverterFromCoreType = new TypeConverter();
                }
            }
            return coreConverterFromCoreType;
        }

        internal static CultureInfo InvariantEnglishUS
        {
            get
            {
                return invariantEnglishUS;
            }
        }
    }
}

