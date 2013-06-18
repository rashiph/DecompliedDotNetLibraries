namespace System.Web.UI
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    internal class MinimizableAttributeTypeConverter : BooleanConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string a = value as string;
            if (a == null)
            {
                return base.ConvertFrom(context, culture, value);
            }
            return ((a.Length > 0) && !string.Equals(a, "false", StringComparison.OrdinalIgnoreCase));
        }
    }
}

