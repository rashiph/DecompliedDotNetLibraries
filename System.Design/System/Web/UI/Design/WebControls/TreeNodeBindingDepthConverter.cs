namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    public class TreeNodeBindingDepthConverter : Int32Converter
    {
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string str = value as string;
            if ((str != null) && (str.Length == 0))
            {
                return -1;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if ((value is int) && (((int) value) == -1))
            {
                return string.Empty;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

