namespace System.ComponentModel
{
    using System;
    using System.Globalization;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class UInt32Converter : BaseNumberConverter
    {
        internal override object FromString(string value, CultureInfo culture)
        {
            return uint.Parse(value, culture);
        }

        internal override object FromString(string value, NumberFormatInfo formatInfo)
        {
            return uint.Parse(value, NumberStyles.Integer, formatInfo);
        }

        internal override object FromString(string value, int radix)
        {
            return Convert.ToUInt32(value, radix);
        }

        internal override string ToString(object value, NumberFormatInfo formatInfo)
        {
            uint num = (uint) value;
            return num.ToString("G", formatInfo);
        }

        internal override Type TargetType
        {
            get
            {
                return typeof(uint);
            }
        }
    }
}

