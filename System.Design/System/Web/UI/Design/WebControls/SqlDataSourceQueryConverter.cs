namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Design;
    using System.Globalization;

    internal class SqlDataSourceQueryConverter : TypeConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return System.Design.SR.GetString("SqlDataSourceQueryConverter_Text");
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            return null;
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return false;
        }
    }
}

