namespace System.ServiceModel.XamlIntegration
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.ServiceModel.Dispatcher;
    using System.Windows.Markup;

    public class XPathMessageContextTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((typeof(MarkupExtension) == sourceType) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ((typeof(MarkupExtension) == destinationType) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is XPathMessageContextMarkupExtension)
            {
                return ((MarkupExtension) value).ProvideValue(null);
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            XPathMessageContext context2 = value as XPathMessageContext;
            if ((context2 != null) && (typeof(MarkupExtension) == destinationType))
            {
                return new XPathMessageContextMarkupExtension(context2);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

