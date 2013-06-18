namespace System.ServiceModel.Channels
{
    using System;
    using System.Net.Security;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Xml;

    internal class CallbackContextMessageHeader : MessageHeader
    {
        private EndpointAddress callbackAddress;
        public const string CallbackContextHeaderName = "CallbackContext";
        public const string CallbackContextHeaderNamespace = "http://schemas.microsoft.com/ws/2008/02/context";
        public const string CallbackEndpointReference = "CallbackEndpointReference";
        private static ChannelProtectionRequirements encryptAndSignChannelProtectionRequirements;
        private static ChannelProtectionRequirements signChannelProtectionRequirements;
        private AddressingVersion version;

        public CallbackContextMessageHeader(EndpointAddress callbackAddress, AddressingVersion version)
        {
            if (callbackAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackAddress");
            }
            if (version == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("version");
            }
            if (version != AddressingVersion.WSAddressing10)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("CallbackContextOnlySupportedInWSAddressing10", new object[] { version })));
            }
            this.callbackAddress = callbackAddress;
            this.version = version;
        }

        internal static ChannelProtectionRequirements GetChannelProtectionRequirements(ProtectionLevel protectionLevel)
        {
            if (protectionLevel == ProtectionLevel.EncryptAndSign)
            {
                if (encryptAndSignChannelProtectionRequirements == null)
                {
                    MessagePartSpecification parts = new MessagePartSpecification {
                        HeaderTypes = { new XmlQualifiedName("CallbackContext", "http://schemas.microsoft.com/ws/2008/02/context") }
                    };
                    ChannelProtectionRequirements requirements2 = new ChannelProtectionRequirements();
                    requirements2.IncomingSignatureParts.AddParts(parts);
                    requirements2.IncomingEncryptionParts.AddParts(parts);
                    requirements2.OutgoingSignatureParts.AddParts(parts);
                    requirements2.OutgoingEncryptionParts.AddParts(parts);
                    requirements2.MakeReadOnly();
                    encryptAndSignChannelProtectionRequirements = requirements2;
                }
                return encryptAndSignChannelProtectionRequirements;
            }
            if (protectionLevel != ProtectionLevel.Sign)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("protectionLevel"));
            }
            if (signChannelProtectionRequirements == null)
            {
                MessagePartSpecification specification2 = new MessagePartSpecification {
                    HeaderTypes = { new XmlQualifiedName("CallbackContext", "http://schemas.microsoft.com/ws/2008/02/context") }
                };
                ChannelProtectionRequirements requirements3 = new ChannelProtectionRequirements();
                requirements3.IncomingSignatureParts.AddParts(specification2);
                requirements3.OutgoingSignatureParts.AddParts(specification2);
                requirements3.MakeReadOnly();
                signChannelProtectionRequirements = requirements3;
            }
            return signChannelProtectionRequirements;
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            this.callbackAddress.WriteTo(this.version, writer, "CallbackEndpointReference", "http://schemas.microsoft.com/ws/2008/02/context");
        }

        internal static CallbackContextMessageProperty ParseCallbackContextHeader(XmlReader reader, AddressingVersion version)
        {
            CallbackContextMessageProperty property;
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            if (version != AddressingVersion.WSAddressing10)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("CallbackContextOnlySupportedInWSAddressing10", new object[] { version })));
            }
            try
            {
                reader.ReadStartElement("CallbackContext", "http://schemas.microsoft.com/ws/2008/02/context");
                EndpointAddress callbackAddress = EndpointAddress.ReadFrom(version, reader, "CallbackEndpointReference", "http://schemas.microsoft.com/ws/2008/02/context");
                reader.ReadEndElement();
                property = new CallbackContextMessageProperty(callbackAddress);
            }
            catch (XmlException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("XmlFormatViolationInCallbackContextHeader"), exception));
            }
            return property;
        }

        public override string Name
        {
            get
            {
                return "CallbackContext";
            }
        }

        public override string Namespace
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2008/02/context";
            }
        }
    }
}

