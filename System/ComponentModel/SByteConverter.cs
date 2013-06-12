namespace System.ComponentModel
{
    using System;
    using System.Globalization;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class SByteConverter : BaseNumberConverter
    {
        internal override object FromString(string value, CultureInfo culture)
        {
            return sbyte.Parse(value, culture);
        }

        internal override object FromString(string value, NumberFormatInfo formatInfo)
        {
            return sbyte.Parse(value, NumberStyles.Integer, (IFormatProvider) formatInfo);
        }

        internal override object FromString(string value, int radix)
        {
            return Convert.ToSByte(value, radix);
        }

        internal override string ToString(object value, NumberFormatInfo formatInfo)
        {
            sbyte num = (sbyte) value;
            return num.ToString("G", (IFormatProvider) formatInfo);
        }

        internal override Type TargetType
        {
            get
            {
                return typeof(sbyte);
            }
        }
    }
}

