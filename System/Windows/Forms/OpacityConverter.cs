namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    public class OpacityConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (!(value is string))
            {
                return base.ConvertFrom(context, culture, value);
            }
            string s = ((string) value).Replace('%', ' ').Trim();
            double num = double.Parse(s, CultureInfo.CurrentCulture);
            if (((((string) value).IndexOf("%") > 0) && (num >= 0.0)) && (num <= 1.0))
            {
                s = (num / 100.0).ToString(CultureInfo.CurrentCulture);
            }
            double num3 = 1.0;
            try
            {
                num3 = (double) TypeDescriptor.GetConverter(typeof(double)).ConvertFrom(context, culture, s);
                if (num3 > 1.0)
                {
                    num3 /= 100.0;
                }
            }
            catch (FormatException exception)
            {
                throw new FormatException(System.Windows.Forms.SR.GetString("InvalidBoundArgument", new object[] { "Opacity", s, "0%", "100%" }), exception);
            }
            if ((num3 < 0.0) || (num3 > 1.0))
            {
                throw new FormatException(System.Windows.Forms.SR.GetString("InvalidBoundArgument", new object[] { "Opacity", s, "0%", "100%" }));
            }
            return num3;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if (destinationType == typeof(string))
            {
                double num = (double) value;
                int num2 = (int) (num * 100.0);
                return (num2.ToString(CultureInfo.CurrentCulture) + "%");
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

