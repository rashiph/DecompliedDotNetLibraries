namespace System.ComponentModel
{
    using System;
    using System.Globalization;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class UInt16Converter : BaseNumberConverter
    {
        internal override object FromString(string value, CultureInfo culture)
        {
            return ushort.Parse(value, culture);
        }

        internal override object FromString(string value, NumberFormatInfo formatInfo)
        {
            return ushort.Parse(value, NumberStyles.Integer, (IFormatProvider) formatInfo);
        }

        internal override object FromString(string value, int radix)
        {
            return Convert.ToUInt16(value, radix);
        }

        internal override string ToString(object value, NumberFormatInfo formatInfo)
        {
            ushort num = (ushort) value;
            return num.ToString("G", formatInfo);
        }

        internal override Type TargetType
        {
            get
            {
                return typeof(ushort);
            }
        }
    }
}

