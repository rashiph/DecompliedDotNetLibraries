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
    internal struct CreateCoordinationContextResponse
    {
        public Microsoft.Transactions.Wsat.Messaging.CoordinationContext CoordinationContext;
        public RequestSecurityTokenResponse IssuedToken;
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
        public CreateCoordinationContextResponse(Microsoft.Transactions.Wsat.Protocol.ProtocolVersion protocolVersion)
        {
            this.CoordinationContext = null;
            this.IssuedToken = null;
            this.protocolVersion = protocolVersion;
            this.coordinationStrings = CoordinationStrings.Version(protocolVersion);
            this.coordinationXmlDictionaryStrings = CoordinationXmlDictionaryStrings.Version(protocolVersion);
        }

        public CreateCoordinationContextResponse(Message message, Microsoft.Transactions.Wsat.Protocol.ProtocolVersion protocolVersion) : this(protocolVersion)
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
            try
            {
                this.IssuedToken = CoordinationServiceSecurity.GetIssuedToken(message, this.CoordinationContext.Identifier, protocolVersion);
            }
            catch (XmlException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidMessageException(exception2.Message, exception2));
            }
        }

        public CreateCoordinationContextResponse(XmlDictionaryReader reader, Microsoft.Transactions.Wsat.Protocol.ProtocolVersion protocolVersion) : this(protocolVersion)
        {
            this.ReadFrom(reader);
        }

        public void WriteTo(XmlDictionaryWriter writer)
        {
            writer.WriteStartElement(this.coordinationStrings.Prefix, this.coordinationXmlDictionaryStrings.CreateCoordinationContextResponse, this.coordinationXmlDictionaryStrings.Namespace);
            this.CoordinationContext.WriteTo(writer, this.coordinationXmlDictionaryStrings.CoordinationContext, this.coordinationXmlDictionaryStrings.Namespace);
            writer.WriteEndElement();
        }

        private void ReadFrom(XmlDictionaryReader reader)
        {
            try
            {
                reader.ReadFullStartElement(this.coordinationXmlDictionaryStrings.CreateCoordinationContextResponse, this.coordinationXmlDictionaryStrings.Namespace);
                this.CoordinationContext = Microsoft.Transactions.Wsat.Messaging.CoordinationContext.ReadFrom(reader, this.coordinationXmlDictionaryStrings.CoordinationContext, this.coordinationXmlDictionaryStrings.Namespace, this.protocolVersion);
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
            catch (InvalidCoordinationContextException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidMessageException(Microsoft.Transactions.SR.GetString("InvalidMessageBody"), exception2));
            }
        }
    }
}

