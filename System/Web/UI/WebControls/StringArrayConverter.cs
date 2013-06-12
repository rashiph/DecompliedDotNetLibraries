namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    public class StringArrayConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(string));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (!(value is string))
            {
                throw base.GetConvertFromException(value);
            }
            if (((string) value).Length == 0)
            {
                return new string[0];
            }
            string[] strArray = ((string) value).Split(new char[] { ',' });
            for (int i = 0; i < strArray.Length; i++)
            {
                strArray[i] = strArray[i].Trim();
            }
            return strArray;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (!(destinationType == typeof(string)))
            {
                throw base.GetConvertToException(value, destinationType);
            }
            if (value == null)
            {
                return string.Empty;
            }
            return string.Join(",", (string[]) value);
        }
    }
}

