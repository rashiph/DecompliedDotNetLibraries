namespace System.Windows.Forms.ComponentModel.Com2Interop
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;

    internal class Com2ExtendedTypeConverter : TypeConverter
    {
        private TypeConverter innerConverter;

        public Com2ExtendedTypeConverter(TypeConverter innerConverter)
        {
            this.innerConverter = innerConverter;
        }

        public Com2ExtendedTypeConverter(Type baseType)
        {
            this.innerConverter = TypeDescriptor.GetConverter(baseType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (this.innerConverter != null)
            {
                return this.innerConverter.CanConvertFrom(context, sourceType);
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (this.innerConverter != null)
            {
                return this.innerConverter.CanConvertTo(context, destinationType);
            }
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (this.innerConverter != null)
            {
                return this.innerConverter.ConvertFrom(context, culture, value);
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (this.innerConverter != null)
            {
                return this.innerConverter.ConvertTo(context, culture, value, destinationType);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            if (this.innerConverter != null)
            {
                return this.innerConverter.CreateInstance(context, propertyValues);
            }
            return base.CreateInstance(context, propertyValues);
        }

        public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
        {
            if (this.innerConverter != null)
            {
                return this.innerConverter.GetCreateInstanceSupported(context);
            }
            return base.GetCreateInstanceSupported(context);
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            if (this.innerConverter != null)
            {
                return this.innerConverter.GetProperties(context, value, attributes);
            }
            return base.GetProperties(context, value, attributes);
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            if (this.innerConverter != null)
            {
                return this.innerConverter.GetPropertiesSupported(context);
            }
            return base.GetPropertiesSupported(context);
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (this.innerConverter != null)
            {
                return this.innerConverter.GetStandardValues(context);
            }
            return base.GetStandardValues(context);
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            if (this.innerConverter != null)
            {
                return this.innerConverter.GetStandardValuesExclusive(context);
            }
            return base.GetStandardValuesExclusive(context);
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            if (this.innerConverter != null)
            {
                return this.innerConverter.GetStandardValuesSupported(context);
            }
            return base.GetStandardValuesSupported(context);
        }

        public TypeConverter GetWrappedConverter(Type t)
        {
            for (TypeConverter converter = this.innerConverter; converter != null; converter = ((Com2ExtendedTypeConverter) converter).InnerConverter)
            {
                if (t.IsInstanceOfType(converter))
                {
                    return converter;
                }
                if (!(converter is Com2ExtendedTypeConverter))
                {
                    break;
                }
            }
            return null;
        }

        public override bool IsValid(ITypeDescriptorContext context, object value)
        {
            if (this.innerConverter != null)
            {
                return this.innerConverter.IsValid(context, value);
            }
            return base.IsValid(context, value);
        }

        public TypeConverter InnerConverter
        {
            get
            {
                return this.innerConverter;
            }
        }
    }
}

