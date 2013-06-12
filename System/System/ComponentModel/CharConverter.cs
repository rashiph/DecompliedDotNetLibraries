namespace System.ComponentModel
{
    using System;
    using System.Globalization;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class CharConverter : TypeConverter
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
            string str = (string) value;
            if (str.Length > 1)
            {
                str = str.Trim();
            }
            if ((str == null) || (str.Length <= 0))
            {
                return '\0';
            }
            if (str.Length != 1)
            {
                throw new FormatException(SR.GetString("ConvertInvalidPrimitive", new object[] { str, "Char" }));
            }
            return str[0];
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (((destinationType == typeof(string)) && (value is char)) && (((char) value) == '\0'))
            {
                return "";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

