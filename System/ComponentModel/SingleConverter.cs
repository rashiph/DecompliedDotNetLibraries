namespace System.ComponentModel
{
    using System;
    using System.Globalization;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class SingleConverter : BaseNumberConverter
    {
        internal override object FromString(string value, CultureInfo culture)
        {
            return float.Parse(value, culture);
        }

        internal override object FromString(string value, NumberFormatInfo formatInfo)
        {
            return float.Parse(value, NumberStyles.Float, (IFormatProvider) formatInfo);
        }

        internal override object FromString(string value, int radix)
        {
            return Convert.ToSingle(value, CultureInfo.CurrentCulture);
        }

        internal override string ToString(object value, NumberFormatInfo formatInfo)
        {
            float num = (float) value;
            return num.ToString("R", formatInfo);
        }

        internal override bool AllowHex
        {
            get
            {
                return false;
            }
        }

        internal override Type TargetType
        {
            get
            {
                return typeof(float);
            }
        }
    }
}

