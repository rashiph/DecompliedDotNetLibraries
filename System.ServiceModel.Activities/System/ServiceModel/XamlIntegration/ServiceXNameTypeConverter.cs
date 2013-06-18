namespace System.ServiceModel.XamlIntegration
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime;
    using System.Xml.Linq;

    public class ServiceXNameTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return XNameTypeConverterHelper.CanConvertFrom(sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return XNameTypeConverterHelper.CanConvertTo(destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string str = value as string;
            if (!string.IsNullOrEmpty(str) && !this.IsQualifiedName(str))
            {
                return XName.Get(str);
            }
            return (XNameTypeConverterHelper.ConvertFrom(context, value) ?? base.ConvertFrom(context, culture, value));
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            XName name = value as XName;
            if ((destinationType == typeof(string)) && (name != null))
            {
                if (name.Namespace == XNamespace.None)
                {
                    return name.LocalName;
                }
                string str = (string) (XNameTypeConverterHelper.ConvertTo(context, value, destinationType) ?? base.ConvertTo(context, culture, value, destinationType));
                if (this.IsQualifiedName(str))
                {
                    return str;
                }
                return name.ToString().Replace("{", "{{").Replace("}", "}}");
            }
            return (XNameTypeConverterHelper.ConvertTo(context, value, destinationType) ?? base.ConvertTo(context, culture, value, destinationType));
        }

        private bool IsQualifiedName(string name)
        {
            return ((name.IndexOf(':') >= 1) || ((name.Length > 0) && (name[0] == '{')));
        }
    }
}

