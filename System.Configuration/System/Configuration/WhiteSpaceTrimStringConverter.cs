namespace System.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    public sealed class WhiteSpaceTrimStringConverter : ConfigurationConverterBase
    {
        public override object ConvertFrom(ITypeDescriptorContext ctx, CultureInfo ci, object data)
        {
            return ((string) data).Trim();
        }

        public override object ConvertTo(ITypeDescriptorContext ctx, CultureInfo ci, object value, Type type)
        {
            base.ValidateType(value, typeof(string));
            if (value == null)
            {
                return string.Empty;
            }
            return ((string) value).Trim();
        }
    }
}

