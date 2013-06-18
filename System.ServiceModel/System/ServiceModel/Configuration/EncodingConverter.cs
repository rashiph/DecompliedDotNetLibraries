namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;

    internal class EncodingConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            return ((typeof(string) == sourceType) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
        {
            return ((typeof(InstanceDescriptor) == destinationType) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            Encoding encoding;
            if (!(value is string))
            {
                return base.ConvertFrom(context, culture, value);
            }
            string strA = (string) value;
            if (string.Compare(strA, "utf-8", StringComparison.OrdinalIgnoreCase) == 0)
            {
                encoding = TextEncoderDefaults.Encoding;
            }
            else
            {
                encoding = Encoding.GetEncoding(strA);
            }
            if (encoding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", System.ServiceModel.SR.GetString("ConfigInvalidEncodingValue", new object[] { strA }));
            }
            return encoding;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
        {
            if ((typeof(string) == destinationType) && (value is Encoding))
            {
                Encoding encoding = (Encoding) value;
                return encoding.HeaderName;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

