namespace System.Messaging.Design
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Messaging;

    internal class SizeConverter : TypeConverter
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
            if ((strA.Length != 0) && (string.Compare(strA, Res.GetString("InfiniteValue"), true, CultureInfo.CurrentCulture) != 0))
            {
                return Convert.ToInt64(strA, culture);
            }
            return (long) 0xffffffffL;
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
            if (((long) value) == 0xffffffffL)
            {
                return Res.GetString("InfiniteValue");
            }
            return value.ToString();
        }
    }
}

