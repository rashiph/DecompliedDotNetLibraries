namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    internal class SecurityAlgorithmSuiteConverter : TypeConverter
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
                case "Default":
                    return SecurityAlgorithmSuite.Default;

                case "Basic256":
                    return SecurityAlgorithmSuite.Basic256;

                case "Basic192":
                    return SecurityAlgorithmSuite.Basic192;

                case "Basic128":
                    return SecurityAlgorithmSuite.Basic128;

                case "TripleDes":
                    return SecurityAlgorithmSuite.TripleDes;

                case "Basic256Rsa15":
                    return SecurityAlgorithmSuite.Basic256Rsa15;

                case "Basic192Rsa15":
                    return SecurityAlgorithmSuite.Basic192Rsa15;

                case "Basic128Rsa15":
                    return SecurityAlgorithmSuite.Basic128Rsa15;

                case "TripleDesRsa15":
                    return SecurityAlgorithmSuite.TripleDesRsa15;

                case "Basic256Sha256":
                    return SecurityAlgorithmSuite.Basic256Sha256;

                case "Basic192Sha256":
                    return SecurityAlgorithmSuite.Basic192Sha256;

                case "Basic128Sha256":
                    return SecurityAlgorithmSuite.Basic128Sha256;

                case "TripleDesSha256":
                    return SecurityAlgorithmSuite.TripleDesSha256;

                case "Basic256Sha256Rsa15":
                    return SecurityAlgorithmSuite.Basic256Sha256Rsa15;

                case "Basic192Sha256Rsa15":
                    return SecurityAlgorithmSuite.Basic192Sha256Rsa15;

                case "Basic128Sha256Rsa15":
                    return SecurityAlgorithmSuite.Basic128Sha256Rsa15;

                case "TripleDesSha256Rsa15":
                    return SecurityAlgorithmSuite.TripleDesSha256Rsa15;
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", System.ServiceModel.SR.GetString("ConfigInvalidClassFactoryValue", new object[] { str, typeof(SecurityAlgorithmSuite).FullName })));
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (!(typeof(string) == destinationType) || !(value is SecurityAlgorithmSuite))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
            SecurityAlgorithmSuite suite = (SecurityAlgorithmSuite) value;
            if (suite == SecurityAlgorithmSuite.Default)
            {
                return "Default";
            }
            if (suite == SecurityAlgorithmSuite.Basic256)
            {
                return "Basic256";
            }
            if (suite == SecurityAlgorithmSuite.Basic192)
            {
                return "Basic192";
            }
            if (suite == SecurityAlgorithmSuite.Basic128)
            {
                return "Basic128";
            }
            if (suite == SecurityAlgorithmSuite.TripleDes)
            {
                return "TripleDes";
            }
            if (suite == SecurityAlgorithmSuite.Basic256Rsa15)
            {
                return "Basic256Rsa15";
            }
            if (suite == SecurityAlgorithmSuite.Basic192Rsa15)
            {
                return "Basic192Rsa15";
            }
            if (suite == SecurityAlgorithmSuite.Basic128Rsa15)
            {
                return "Basic128Rsa15";
            }
            if (suite == SecurityAlgorithmSuite.TripleDesRsa15)
            {
                return "TripleDesRsa15";
            }
            if (suite == SecurityAlgorithmSuite.Basic256Sha256)
            {
                return "Basic256Sha256";
            }
            if (suite == SecurityAlgorithmSuite.Basic192Sha256)
            {
                return "Basic192Sha256";
            }
            if (suite == SecurityAlgorithmSuite.Basic128Sha256)
            {
                return "Basic128Sha256";
            }
            if (suite == SecurityAlgorithmSuite.TripleDesSha256)
            {
                return "TripleDesSha256";
            }
            if (suite == SecurityAlgorithmSuite.Basic256Sha256Rsa15)
            {
                return "Basic256Sha256Rsa15";
            }
            if (suite == SecurityAlgorithmSuite.Basic192Sha256Rsa15)
            {
                return "Basic192Sha256Rsa15";
            }
            if (suite == SecurityAlgorithmSuite.Basic128Sha256Rsa15)
            {
                return "Basic128Sha256Rsa15";
            }
            if (suite != SecurityAlgorithmSuite.TripleDesSha256Rsa15)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", System.ServiceModel.SR.GetString("ConfigInvalidClassInstanceValue", new object[] { typeof(SecurityAlgorithmSuite).FullName })));
            }
            return "TripleDesSha256Rsa15";
        }
    }
}

