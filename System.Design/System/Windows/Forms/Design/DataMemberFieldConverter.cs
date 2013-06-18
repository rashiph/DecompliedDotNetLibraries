namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.Design;
    using System.Globalization;

    internal class DataMemberFieldConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if ((value != null) && value.Equals(System.Design.SR.GetString("None")))
            {
                return string.Empty;
            }
            return value;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (!(destinationType == typeof(string)) || ((value != null) && !value.Equals(string.Empty)))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
            return System.Design.SR.GetString("None_lc");
        }
    }
}

