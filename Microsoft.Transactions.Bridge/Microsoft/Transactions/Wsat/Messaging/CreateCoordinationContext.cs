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
    using System.Transactions;
    using System.Xml;

    [StructLayout(LayoutKind.Sequential)]
    internal struct CreateCoordinationContext
    {
        private CoordinationXmlDictionaryStrings coordinationXmlDictionaryStrings;
        private AtomicTransactionXmlDictionaryStrings atomicTransactionXmlDictionaryStrings;
        private AtomicTransactionStrings atomicTransactionStrings;
        private CoordinationStrings coordinationStrings;
        private Microsoft.Transactions.Wsat.Protocol.ProtocolVersion protocolVersion;
        private bool expiresPresent;
        private uint expiration;
        public CoordinationContext CurrentContext;
        public RequestSecurityTokenResponse IssuedToken;
        public System.Transactions.IsolationLevel IsolationLevel;
        public Microsoft.Transactions.Wsat.Protocol.ProtocolVersion ProtocolVersion
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.protocolVersion;
            }
        }
        public bool ExpiresPresent
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.expiresPresent;
            }
        }
        public uint Expires
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.expiration;
            }
            set
            {
                this.expiration = value;
                this.expiresPresent = true;
            }
        }
        public CreateCoordinationContext(Microsoft.Transactions.Wsat.Protocol.ProtocolVersion protocolVersion)
        {
            this.expiration = 0;
            this.expiresPresent = false;
            this.CurrentContext = null;
            this.IssuedToken = null;
            this.IsolationLevel = System.Transactions.IsolationLevel.Serializable;
            this.protocolVersion = protocolVersion;
            this.coordinationXmlDictionaryStrings = CoordinationXmlDictionaryStrings.Version(protocolVersion);
            this.atomicTransactionXmlDictionaryStrings = AtomicTransactionXmlDictionaryStrings.Version(protocolVersion);
            this.atomicTransactionStrings = AtomicTransactionStrings.Version(protocolVersion);
            this.coordinationStrings = CoordinationStrings.Version(protocolVersion);
        }

        public CreateCoordinationContext(Message message, Microsoft.Transactions.Wsat.Protocol.ProtocolVersion protocolVersion) : this(protocolVersion)
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
            if (this.CurrentContext != null)
            {
                try
                {
                    this.IssuedToken = CoordinationServiceSecurity.GetIssuedToken(message, this.CurrentContext.Identifier, protocolVersion);
                }
                catch (XmlException exception2)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidMessageException(exception2.Message, exception2));
                }
            }
        }

        public CreateCoordinationContext(XmlDictionaryReader reader, Microsoft.Transactions.Wsat.Protocol.ProtocolVersion protocolVersion) : this(protocolVersion)
        {
            this.ReadFrom(reader);
        }

        public void WriteTo(XmlDictionaryWriter writer)
        {
            writer.WriteStartElement(this.coordinationStrings.Prefix, this.coordinationXmlDictionaryStrings.CreateCoordinationContext, this.coordinationXmlDictionaryStrings.Namespace);
            if (this.expiresPresent)
            {
                writer.WriteStartElement(this.coordinationXmlDictionaryStrings.Expires, this.coordinationXmlDictionaryStrings.Namespace);
                writer.WriteValue((long) this.Expires);
                writer.WriteEndElement();
            }
            if (this.CurrentContext != null)
            {
                this.CurrentContext.WriteTo(writer, this.coordinationXmlDictionaryStrings.CurrentContext, this.coordinationXmlDictionaryStrings.Namespace);
            }
            writer.WriteStartElement(this.coordinationXmlDictionaryStrings.CoordinationType, this.coordinationXmlDictionaryStrings.Namespace);
            writer.WriteString(this.atomicTransactionXmlDictionaryStrings.Namespace);
            writer.WriteEndElement();
            if (this.IsolationLevel != System.Transactions.IsolationLevel.Serializable)
            {
                writer.WriteStartElement("mstx", XD.DotNetAtomicTransactionExternalDictionary.IsolationLevel, XD.DotNetAtomicTransactionExternalDictionary.Namespace);
                writer.WriteValue((int) this.IsolationLevel);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        private void ReadFrom(XmlDictionaryReader reader)
        {
            try
            {
                reader.ReadFullStartElement(this.coordinationXmlDictionaryStrings.CreateCoordinationContext, this.coordinationXmlDictionaryStrings.Namespace);
                if (reader.IsStartElement(this.coordinationXmlDictionaryStrings.Expires, this.coordinationXmlDictionaryStrings.Namespace))
                {
                    int num = reader.ReadElementContentAsInt();
                    if (num < 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidMessageException(Microsoft.Transactions.SR.GetString("InvalidMessageBody")));
                    }
                    this.expiration = (uint) num;
                    this.expiresPresent = true;
                }
                if (reader.IsStartElement(this.coordinationXmlDictionaryStrings.CurrentContext, this.coordinationXmlDictionaryStrings.Namespace))
                {
                    this.CurrentContext = CoordinationContext.ReadFrom(reader, this.coordinationXmlDictionaryStrings.CurrentContext, this.coordinationXmlDictionaryStrings.Namespace, this.protocolVersion);
                }
                reader.MoveToStartElement(this.coordinationXmlDictionaryStrings.CoordinationType, this.coordinationXmlDictionaryStrings.Namespace);
                if (reader.ReadElementContentAsString().Trim() != this.atomicTransactionStrings.Namespace)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidMessageException(Microsoft.Transactions.SR.GetString("InvalidMessageBody")));
                }
                if (!reader.IsStartElement(XD.DotNetAtomicTransactionExternalDictionary.IsolationLevel, XD.DotNetAtomicTransactionExternalDictionary.Namespace))
                {
                    goto Label_016B;
                }
                this.IsolationLevel = (System.Transactions.IsolationLevel) reader.ReadElementContentAsInt();
                if (((this.IsolationLevel >= System.Transactions.IsolationLevel.Serializable) && (this.IsolationLevel <= System.Transactions.IsolationLevel.Unspecified)) && (this.IsolationLevel != System.Transactions.IsolationLevel.Snapshot))
                {
                    goto Label_016B;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidMessageException(Microsoft.Transactions.SR.GetString("InvalidMessageBody")));
            Label_0165:
                reader.Skip();
            Label_016B:
                if (reader.IsStartElement())
                {
                    goto Label_0165;
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

