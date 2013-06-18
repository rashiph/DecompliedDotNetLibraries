namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.Xml;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Register
    {
        public ControlProtocol Protocol;
        public EndpointAddress ParticipantProtocolService;
        private CoordinationStrings coordinationStrings;
        private CoordinationXmlDictionaryStrings coordinationXmlDictionaryStrings;
        public Guid Loopback;
        public RequestSecurityTokenResponse SupportingToken;
        private Microsoft.Transactions.Wsat.Protocol.ProtocolVersion protocolVersion;
        public Microsoft.Transactions.Wsat.Protocol.ProtocolVersion ProtocolVersion
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.protocolVersion;
            }
        }
        public Register(Microsoft.Transactions.Wsat.Protocol.ProtocolVersion protocolVersion)
        {
            this.Protocol = ControlProtocol.None;
            this.ParticipantProtocolService = null;
            this.Loopback = Guid.Empty;
            this.SupportingToken = null;
            this.protocolVersion = protocolVersion;
            this.coordinationStrings = CoordinationStrings.Version(protocolVersion);
            this.coordinationXmlDictionaryStrings = CoordinationXmlDictionaryStrings.Version(protocolVersion);
        }

        public Register(Message message, Microsoft.Transactions.Wsat.Protocol.ProtocolVersion protocolVersion) : this(protocolVersion)
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

        public Register(XmlDictionaryReader reader, Microsoft.Transactions.Wsat.Protocol.ProtocolVersion protocolVersion) : this(protocolVersion)
        {
            this.ReadFrom(reader);
        }

        public void WriteTo(XmlDictionaryWriter writer)
        {
            writer.WriteStartElement(this.coordinationStrings.Prefix, this.coordinationXmlDictionaryStrings.Register, this.coordinationXmlDictionaryStrings.Namespace);
            XmlDictionaryString str = WSAtomicTransactionStrings.ProtocolToWellKnownName(this.Protocol, this.protocolVersion);
            writer.WriteStartElement(this.coordinationXmlDictionaryStrings.Protocol, this.coordinationXmlDictionaryStrings.Namespace);
            writer.WriteString(str);
            writer.WriteEndElement();
            this.ParticipantProtocolService.WriteTo(MessagingVersionHelper.AddressingVersion(this.protocolVersion), writer, this.coordinationXmlDictionaryStrings.ParticipantProtocolService, this.coordinationXmlDictionaryStrings.Namespace);
            if (this.Loopback != Guid.Empty)
            {
                writer.WriteStartElement("mstx", XD.DotNetAtomicTransactionExternalDictionary.Loopback, XD.DotNetAtomicTransactionExternalDictionary.Namespace);
                writer.WriteValue(this.Loopback);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        private void ReadFrom(XmlDictionaryReader reader)
        {
            try
            {
                reader.ReadFullStartElement(this.coordinationXmlDictionaryStrings.Register, this.coordinationXmlDictionaryStrings.Namespace);
                reader.MoveToStartElement(this.coordinationXmlDictionaryStrings.Protocol, this.coordinationXmlDictionaryStrings.Namespace);
                this.Protocol = WSAtomicTransactionStrings.WellKnownNameToProtocol(reader.ReadElementContentAsString().Trim(), this.protocolVersion);
                if (this.Protocol == ControlProtocol.None)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidMessageException(Microsoft.Transactions.SR.GetString("InvalidMessageBody")));
                }
                this.ParticipantProtocolService = EndpointAddress.ReadFrom(MessagingVersionHelper.AddressingVersion(this.protocolVersion), reader, this.coordinationXmlDictionaryStrings.ParticipantProtocolService, this.coordinationXmlDictionaryStrings.Namespace);
                if (reader.IsStartElement(XD.DotNetAtomicTransactionExternalDictionary.Loopback, XD.DotNetAtomicTransactionExternalDictionary.Namespace))
                {
                    this.Loopback = reader.ReadElementContentAsGuid();
                }
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

