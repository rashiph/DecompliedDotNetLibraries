namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    [StructLayout(LayoutKind.Sequential)]
    internal struct RegisterResponse
    {
        public EndpointAddress CoordinatorProtocolService;
        private CoordinationStrings coordinationStrings;
        private CoordinationXmlDictionaryStrings coordinationXmlDictionaryStrings;
        private Microsoft.Transactions.Wsat.Protocol.ProtocolVersion protocolVersion;
        public Microsoft.Transactions.Wsat.Protocol.ProtocolVersion ProtocolVersion
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.protocolVersion;
            }
        }
        public RegisterResponse(Microsoft.Transactions.Wsat.Protocol.ProtocolVersion protocolVersion)
        {
            this.CoordinatorProtocolService = null;
            this.protocolVersion = protocolVersion;
            this.coordinationStrings = CoordinationStrings.Version(protocolVersion);
            this.coordinationXmlDictionaryStrings = CoordinationXmlDictionaryStrings.Version(protocolVersion);
        }

        public RegisterResponse(Message message, Microsoft.Transactions.Wsat.Protocol.ProtocolVersion protocolVersion) : this(protocolVersion)
        {
            if (message.IsEmpty)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidMessageException(Microsoft.Transactions.SR.GetString("InvalidMessageBody")));
            }
            XmlDictionaryReader readerAtBodyContents = message.GetReaderAtBodyContents();
            using (readerAtBodyContents)
            {
                this.ReadFrom(readerAtBodyContents);
                try
                {
                    message.ReadFromBodyContentsToEnd(readerAtBodyContents);
                }
                catch (XmlException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidMessageException(Microsoft.Transactions.SR.GetString("InvalidMessageBody"), exception));
                }
            }
        }

        public RegisterResponse(XmlDictionaryReader reader, Microsoft.Transactions.Wsat.Protocol.ProtocolVersion protocolVersion) : this(protocolVersion)
        {
            this.ReadFrom(reader);
        }

        public void WriteTo(XmlDictionaryWriter writer)
        {
            writer.WriteStartElement(this.coordinationStrings.Prefix, this.coordinationXmlDictionaryStrings.RegisterResponse, this.coordinationXmlDictionaryStrings.Namespace);
            this.CoordinatorProtocolService.WriteTo(MessagingVersionHelper.AddressingVersion(this.protocolVersion), writer, this.coordinationXmlDictionaryStrings.CoordinatorProtocolService, this.coordinationXmlDictionaryStrings.Namespace);
            writer.WriteEndElement();
        }

        private void ReadFrom(XmlDictionaryReader reader)
        {
            try
            {
                reader.ReadFullStartElement(this.coordinationXmlDictionaryStrings.RegisterResponse, this.coordinationXmlDictionaryStrings.Namespace);
                this.CoordinatorProtocolService = EndpointAddress.ReadFrom(MessagingVersionHelper.AddressingVersion(this.protocolVersion), reader, this.coordinationXmlDictionaryStrings.CoordinatorProtocolService, this.coordinationXmlDictionaryStrings.Namespace);
                while (reader.IsStartElement())
                {
                    reader.Skip();
                }
                reader.ReadEndElement();
            }
            catch (XmlException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidMessageException(Microsoft.Transactions.SR.GetString("InvalidMessageBody"), exception));
            }
        }
    }
}

