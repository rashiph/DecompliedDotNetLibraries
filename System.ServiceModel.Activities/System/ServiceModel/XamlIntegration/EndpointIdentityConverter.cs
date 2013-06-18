namespace System.ServiceModel.XamlIntegration
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.ServiceModel;
    using System.Windows.Markup;

    public class EndpointIdentityConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ((destinationType == typeof(MarkupExtension)) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value == null)
            {
                return null;
            }
            if (!(destinationType == typeof(MarkupExtension)) || !(value is EndpointIdentity))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
            if (value is SpnEndpointIdentity)
            {
                return new SpnEndpointIdentityExtension((SpnEndpointIdentity) value);
            }
            if (value is UpnEndpointIdentity)
            {
                return new UpnEndpointIdentityExtension((UpnEndpointIdentity) value);
            }
            return new EndpointIdentityExtension((EndpointIdentity) value);
        }
    }
}

