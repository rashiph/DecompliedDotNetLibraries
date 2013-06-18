namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    public class DataGridPreferredColumnWidthTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            if (!(sourceType == typeof(string)) && !(sourceType == typeof(int)))
            {
                return false;
            }
            return true;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value.GetType() == typeof(string))
            {
                string s = value.ToString();
                if (s.Equals("AutoColumnResize (-1)"))
                {
                    return -1;
                }
                return int.Parse(s, CultureInfo.CurrentCulture);
            }
            if (value.GetType() != typeof(int))
            {
                throw base.GetConvertFromException(value);
            }
            return (int) value;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
        {
            if (!(destinationType == typeof(string)))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
            if (!(value.GetType() == typeof(int)))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
            int num = (int) value;
            if (num == -1)
            {
                return "AutoColumnResize (-1)";
            }
            return num.ToString(CultureInfo.CurrentCulture);
        }
    }
}

