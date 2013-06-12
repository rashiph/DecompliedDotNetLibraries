namespace System.ComponentModel
{
    using System;
    using System.Globalization;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class Int16Converter : BaseNumberConverter
    {
        internal override object FromString(string value, CultureInfo culture)
        {
            return short.Parse(value, culture);
        }

        internal override object FromString(string value, NumberFormatInfo formatInfo)
        {
            return short.Parse(value, NumberStyles.Integer, (IFormatProvider) formatInfo);
        }

        internal override object FromString(string value, int radix)
        {
            return Convert.ToInt16(value, radix);
        }

        internal override string ToString(object value, NumberFormatInfo formatInfo)
        {
            short num = (short) value;
            return num.ToString("G", (IFormatProvider) formatInfo);
        }

        internal override Type TargetType
        {
            get
            {
                return typeof(short);
            }
        }
    }
}

