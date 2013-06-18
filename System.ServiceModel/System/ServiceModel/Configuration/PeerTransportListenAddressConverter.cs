namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Net;

    internal class PeerTransportListenAddressConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((typeof(string) == sourceType) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ((typeof(IPAddress) == destinationType) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                return IPAddress.Parse(value as string);
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if ((typeof(string) == destinationType) && (value is IPAddress))
            {
                return ((IPAddress) value).ToString();
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

