namespace System.Messaging.Design
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Messaging;

    internal class TimeoutConverter : TypeConverter
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
            string strA = ((string) value).Trim();
            if ((strA.Length == 0) || (string.Compare(strA, Res.GetString("InfiniteValue"), true, CultureInfo.CurrentCulture) == 0))
            {
                return TimeSpan.FromSeconds(4294967295);
            }
            double num = Convert.ToDouble(strA, culture);
            if (num > 4294967295)
            {
                num = 4294967295;
            }
            return TimeSpan.FromSeconds(num);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if (!(destinationType == typeof(string)) || (value == null))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
            TimeSpan span = (TimeSpan) value;
            double totalSeconds = span.TotalSeconds;
            if (totalSeconds >= 4294967295)
            {
                return Res.GetString("InfiniteValue");
            }
            uint num2 = (uint) totalSeconds;
            return num2.ToString(culture);
        }
    }
}

