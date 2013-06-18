namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    internal class PolicyVersionConverter : TypeConverter
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
            if (!(value is string))
            {
                return base.ConvertFrom(context, culture, value);
            }
            string str = (string) value;
            switch (str)
            {
                case "Policy12":
                    return PolicyVersion.Policy12;

                case "Policy15":
                    return PolicyVersion.Policy15;

                case "Default":
                    return PolicyVersion.Default;
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", System.ServiceModel.SR.GetString("ConfigInvalidClassFactoryValue", new object[] { str, typeof(PolicyVersion).FullName })));
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (!(typeof(string) == destinationType) || !(value is PolicyVersion))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
            PolicyVersion version = (PolicyVersion) value;
            if (version == PolicyVersion.Default)
            {
                return "Default";
            }
            if (version == PolicyVersion.Policy12)
            {
                return "Policy12";
            }
            if (version != PolicyVersion.Policy15)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", System.ServiceModel.SR.GetString("ConfigInvalidClassInstanceValue", new object[] { typeof(PolicyVersion).FullName })));
            }
            return "Policy15";
        }
    }
}

