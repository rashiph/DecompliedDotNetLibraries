namespace System.Web.Services.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    internal class TypeAndNameConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                return new TypeAndName((string) value);
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (!(destinationType == typeof(string)))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
            TypeAndName name = (TypeAndName) value;
            if (name.name != null)
            {
                return name.name;
            }
            return name.type.AssemblyQualifiedName;
        }
    }
}

