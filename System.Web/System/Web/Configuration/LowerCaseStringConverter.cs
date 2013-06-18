namespace System.Web.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    public sealed class LowerCaseStringConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext ctx, Type type)
        {
            return (type == typeof(string));
        }

        public override bool CanConvertTo(ITypeDescriptorContext ctx, Type type)
        {
            return (type == typeof(string));
        }

        public override object ConvertFrom(ITypeDescriptorContext ctx, CultureInfo ci, object data)
        {
            return ((string) data).ToLower(CultureInfo.InvariantCulture);
        }

        public override object ConvertTo(ITypeDescriptorContext ctx, CultureInfo ci, object value, Type type)
        {
            if (value == null)
            {
                return string.Empty;
            }
            return ((string) value).ToLower(CultureInfo.InvariantCulture);
        }
    }
}

