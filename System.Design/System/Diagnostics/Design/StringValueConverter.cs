namespace System.Diagnostics.Design
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    internal class StringValueConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (!(value is string))
            {
                return base.ConvertFrom(context, culture, value);
            }
            string str = ((string) value).Trim();
            if (str == string.Empty)
            {
                str = null;
            }
            return str;
        }
    }
}

