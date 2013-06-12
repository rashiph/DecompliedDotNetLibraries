namespace System.ComponentModel
{
    using System;
    using System.Globalization;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class UInt64Converter : BaseNumberConverter
    {
        internal override object FromString(string value, CultureInfo culture)
        {
            return ulong.Parse(value, culture);
        }

        internal override object FromString(string value, NumberFormatInfo formatInfo)
        {
            return ulong.Parse(value, NumberStyles.Integer, formatInfo);
        }

        internal override object FromString(string value, int radix)
        {
            return Convert.ToUInt64(value, radix);
        }

        internal override string ToString(object value, NumberFormatInfo formatInfo)
        {
            ulong num = (ulong) value;
            return num.ToString("G", formatInfo);
        }

        internal override Type TargetType
        {
            get
            {
                return typeof(ulong);
            }
        }
    }
}

