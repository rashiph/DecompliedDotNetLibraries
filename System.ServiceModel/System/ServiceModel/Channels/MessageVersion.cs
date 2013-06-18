namespace System.ServiceModel.Channels
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.ServiceModel;
    using System.ServiceModel.Configuration;

    [TypeConverter(typeof(MessageVersionConverter))]
    public sealed class MessageVersion
    {
        private AddressingVersion addressing;
        private EnvelopeVersion envelope;
        private static MessageVersion none = new MessageVersion(EnvelopeVersion.None, AddressingVersion.None);
        private static MessageVersion soap11 = new MessageVersion(EnvelopeVersion.Soap11, AddressingVersion.None);
        private static MessageVersion soap11Addressing10 = new MessageVersion(EnvelopeVersion.Soap11, AddressingVersion.WSAddressing10);
        private static MessageVersion soap11Addressing200408 = new MessageVersion(EnvelopeVersion.Soap11, AddressingVersion.WSAddressingAugust2004);
        private static MessageVersion soap12 = new MessageVersion(EnvelopeVersion.Soap12, AddressingVersion.None);
        private static MessageVersion soap12Addressing10 = new MessageVersion(EnvelopeVersion.Soap12, AddressingVersion.WSAddressing10);
        private static MessageVersion soap12Addressing200408 = new MessageVersion(EnvelopeVersion.Soap12, AddressingVersion.WSAddressingAugust2004);

        private MessageVersion(EnvelopeVersion envelopeVersion, AddressingVersion addressingVersion)
        {
            this.envelope = envelopeVersion;
            this.addressing = addressingVersion;
        }

        public static MessageVersion CreateVersion(EnvelopeVersion envelopeVersion)
        {
            return CreateVersion(envelopeVersion, AddressingVersion.WSAddressing10);
        }

        public static MessageVersion CreateVersion(EnvelopeVersion envelopeVersion, AddressingVersion addressingVersion)
        {
            if (envelopeVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("envelopeVersion");
            }
            if (addressingVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("addressingVersion");
            }
            if (envelopeVersion == EnvelopeVersion.Soap12)
            {
                if (addressingVersion == AddressingVersion.WSAddressing10)
                {
                    return soap12Addressing10;
                }
                if (addressingVersion == AddressingVersion.WSAddressingAugust2004)
                {
                    return soap12Addressing200408;
                }
                if (addressingVersion != AddressingVersion.None)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("addressingVersion", System.ServiceModel.SR.GetString("AddressingVersionNotSupported", new object[] { addressingVersion }));
                }
                return soap12;
            }
            if (envelopeVersion == EnvelopeVersion.Soap11)
            {
                if (addressingVersion == AddressingVersion.WSAddressing10)
                {
                    return soap11Addressing10;
                }
                if (addressingVersion == AddressingVersion.WSAddressingAugust2004)
                {
                    return soap11Addressing200408;
                }
                if (addressingVersion != AddressingVersion.None)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("addressingVersion", System.ServiceModel.SR.GetString("AddressingVersionNotSupported", new object[] { addressingVersion }));
                }
                return soap11;
            }
            if (envelopeVersion != EnvelopeVersion.None)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("envelopeVersion", System.ServiceModel.SR.GetString("EnvelopeVersionNotSupported", new object[] { envelopeVersion }));
            }
            if (addressingVersion != AddressingVersion.None)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("addressingVersion", System.ServiceModel.SR.GetString("AddressingVersionNotSupported", new object[] { addressingVersion }));
            }
            return none;
        }

        public override bool Equals(object obj)
        {
            return (this == obj);
        }

        public override int GetHashCode()
        {
            int num = 0;
            if (this.Envelope == EnvelopeVersion.Soap11)
            {
                num++;
            }
            if (this.Addressing == AddressingVersion.WSAddressingAugust2004)
            {
                num += 2;
            }
            return num;
        }

        internal bool IsMatch(MessageVersion messageVersion)
        {
            if (messageVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("messageVersion");
            }
            if (this.addressing == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "MessageVersion.Addressing cannot be null", new object[0])));
            }
            if (this.envelope != messageVersion.Envelope)
            {
                return false;
            }
            if (this.addressing.Namespace != messageVersion.Addressing.Namespace)
            {
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            return System.ServiceModel.SR.GetString("MessageVersionToStringFormat", new object[] { this.envelope.ToString(), this.addressing.ToString() });
        }

        public AddressingVersion Addressing
        {
            get
            {
                return this.addressing;
            }
        }

        public static MessageVersion Default
        {
            get
            {
                return soap12Addressing10;
            }
        }

        public EnvelopeVersion Envelope
        {
            get
            {
                return this.envelope;
            }
        }

        public static MessageVersion None
        {
            get
            {
                return none;
            }
        }

        public static MessageVersion Soap11
        {
            get
            {
                return soap11;
            }
        }

        public static MessageVersion Soap11WSAddressing10
        {
            get
            {
                return soap11Addressing10;
            }
        }

        public static MessageVersion Soap11WSAddressingAugust2004
        {
            get
            {
                return soap11Addressing200408;
            }
        }

        public static MessageVersion Soap12
        {
            get
            {
                return soap12;
            }
        }

        public static MessageVersion Soap12WSAddressing10
        {
            get
            {
                return soap12Addressing10;
            }
        }

        public static MessageVersion Soap12WSAddressingAugust2004
        {
            get
            {
                return soap12Addressing200408;
            }
        }
    }
}

