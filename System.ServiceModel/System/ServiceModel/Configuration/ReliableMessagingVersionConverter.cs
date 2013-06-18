namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.ServiceModel;

    internal class ReliableMessagingVersionConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((typeof(string) == sourceType) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ((typeof(InstanceDescriptor) == destinationType) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string str = value as string;
            switch (str)
            {
                case "Default":
                    return ReliableMessagingVersion.Default;

                case "WSReliableMessaging11":
                    return ReliableMessagingVersion.WSReliableMessaging11;

                case "WSReliableMessagingFebruary2005":
                    return ReliableMessagingVersion.WSReliableMessagingFebruary2005;

                case null:
                    return base.ConvertFrom(context, culture, value);
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("ConfigInvalidReliableMessagingVersionValue", new object[] { str }));
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (!(typeof(string) == destinationType) || !(value is ReliableMessagingVersion))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
            ReliableMessagingVersion version = (ReliableMessagingVersion) value;
            if (version == ReliableMessagingVersion.Default)
            {
                return "Default";
            }
            if (version == ReliableMessagingVersion.WSReliableMessaging11)
            {
                return "WSReliableMessaging11";
            }
            if (version != ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", System.ServiceModel.SR.GetString("ConfigInvalidClassInstanceValue", new object[] { typeof(ReliableMessagingVersion).FullName })));
            }
            return "WSReliableMessagingFebruary2005";
        }
    }
}

