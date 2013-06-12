namespace System.ComponentModel
{
    using System;
    using System.Globalization;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class ByteConverter : BaseNumberConverter
    {
        internal override object FromString(string value, CultureInfo culture)
        {
            return byte.Parse(value, culture);
        }

        internal override object FromString(string value, NumberFormatInfo formatInfo)
        {
            return byte.Parse(value, NumberStyles.Integer, (IFormatProvider) formatInfo);
        }

        internal override object FromString(string value, int radix)
        {
            return Convert.ToByte(value, radix);
        }

        internal override string ToString(object value, NumberFormatInfo formatInfo)
        {
            byte num = (byte) value;
            return num.ToString("G", formatInfo);
        }

        internal override Type TargetType
        {
            get
            {
                return typeof(byte);
            }
        }
    }
}

