namespace System.ComponentModel
{
    using System;
    using System.Collections;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class NullableConverter : TypeConverter
    {
        private Type nullableType;
        private Type simpleType;
        private TypeConverter simpleTypeConverter;

        public NullableConverter(Type type)
        {
            this.nullableType = type;
            this.simpleType = Nullable.GetUnderlyingType(type);
            if (this.simpleType == null)
            {
                throw new ArgumentException(SR.GetString("NullableConverterBadCtorArg"), "type");
            }
            this.simpleTypeConverter = TypeDescriptor.GetConverter(this.simpleType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == this.simpleType)
            {
                return true;
            }
            if (this.simpleTypeConverter != null)
            {
                return this.simpleTypeConverter.CanConvertFrom(context, sourceType);
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == this.simpleType)
            {
                return true;
            }
            if (destinationType == typeof(InstanceDescriptor))
            {
                return true;
            }
            if (this.simpleTypeConverter != null)
            {
                return this.simpleTypeConverter.CanConvertTo(context, destinationType);
            }
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if ((value == null) || (value.GetType() == this.simpleType))
            {
                return value;
            }
            if ((value is string) && string.IsNullOrEmpty(value as string))
            {
                return null;
            }
            if (this.simpleTypeConverter != null)
            {
                return this.simpleTypeConverter.ConvertFrom(context, culture, value);
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if ((destinationType == this.simpleType) && this.nullableType.IsInstanceOfType(value))
            {
                return value;
            }
            if (destinationType == typeof(InstanceDescriptor))
            {
                return new InstanceDescriptor(this.nullableType.GetConstructor(new Type[] { this.simpleType }), new object[] { value }, true);
            }
            if (value == null)
            {
                if (destinationType == typeof(string))
                {
                    return string.Empty;
                }
            }
            else if (this.simpleTypeConverter != null)
            {
                return this.simpleTypeConverter.ConvertTo(context, culture, value, destinationType);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            if (this.simpleTypeConverter != null)
            {
                return this.simpleTypeConverter.CreateInstance(context, propertyValues);
            }
            return base.CreateInstance(context, propertyValues);
        }

        public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
        {
            if (this.simpleTypeConverter != null)
            {
                return this.simpleTypeConverter.GetCreateInstanceSupported(context);
            }
            return base.GetCreateInstanceSupported(context);
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            if (this.simpleTypeConverter != null)
            {
                object obj2 = value;
                return this.simpleTypeConverter.GetProperties(context, obj2, attributes);
            }
            return base.GetProperties(context, value, attributes);
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            if (this.simpleTypeConverter != null)
            {
                return this.simpleTypeConverter.GetPropertiesSupported(context);
            }
            return base.GetPropertiesSupported(context);
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (this.simpleTypeConverter != null)
            {
                TypeConverter.StandardValuesCollection standardValues = this.simpleTypeConverter.GetStandardValues(context);
                if (this.GetStandardValuesSupported(context) && (standardValues != null))
                {
                    object[] values = new object[standardValues.Count + 1];
                    int num = 0;
                    values[num++] = null;
                    foreach (object obj2 in standardValues)
                    {
                        values[num++] = obj2;
                    }
                    return new TypeConverter.StandardValuesCollection(values);
                }
            }
            return base.GetStandardValues(context);
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            if (this.simpleTypeConverter != null)
            {
                return this.simpleTypeConverter.GetStandardValuesExclusive(context);
            }
            return base.GetStandardValuesExclusive(context);
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            if (this.simpleTypeConverter != null)
            {
                return this.simpleTypeConverter.GetStandardValuesSupported(context);
            }
            return base.GetStandardValuesSupported(context);
        }

        public override bool IsValid(ITypeDescriptorContext context, object value)
        {
            if (this.simpleTypeConverter == null)
            {
                return base.IsValid(context, value);
            }
            object obj2 = value;
            return ((obj2 == null) || this.simpleTypeConverter.IsValid(context, obj2));
        }

        public Type NullableType
        {
            get
            {
                return this.nullableType;
            }
        }

        public Type UnderlyingType
        {
            get
            {
                return this.simpleType;
            }
        }

        public TypeConverter UnderlyingTypeConverter
        {
            get
            {
                return this.simpleTypeConverter;
            }
        }
    }
}

