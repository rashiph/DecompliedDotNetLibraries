namespace System.Web.Services.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    internal class TypeTypeConverter : TypeAndNameConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                TypeAndName name = (TypeAndName) base.ConvertFrom(context, culture, value);
                return name.type;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                TypeAndName name = new TypeAndName((Type) value);
                return base.ConvertTo(context, culture, name, destinationType);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

