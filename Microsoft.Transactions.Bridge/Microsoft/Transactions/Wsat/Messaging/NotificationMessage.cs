namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal abstract class NotificationMessage : CoordinationMessage
    {
        protected AtomicTransactionStrings atomicTransactionStrings;
        protected AtomicTransactionXmlDictionaryStrings atomicTransactionXmlDictionaryStrings;

        protected NotificationMessage(string action, MessageVersion version, ProtocolVersion protocolVersion) : base(action, version)
        {
            this.atomicTransactionStrings = AtomicTransactionStrings.Version(protocolVersion);
            this.atomicTransactionXmlDictionaryStrings = AtomicTransactionXmlDictionaryStrings.Version(protocolVersion);
        }

        public static Message CreateRecoverMessage(MessageVersion version, ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(NotificationMessage), "CreateRecoverMessage");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return new ReplayMessage(version, protocolVersion);

                case ProtocolVersion.Version11:
                    return new PreparedMessage(version, protocolVersion);
            }
            return null;
        }

        protected static void ReadFrom(Message message, XmlDictionaryString localName, XmlDictionaryString ns)
        {
            if (message.IsEmpty)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidMessageException(Microsoft.Transactions.SR.GetString("InvalidMessageBody")));
            }
            using (XmlDictionaryReader reader = message.GetReaderAtBodyContents())
            {
                ReadFrom(reader, localName, ns);
                try
                {
                    message.ReadFromBodyContentsToEnd(reader);
                }
                catch (XmlException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidMessageException(Microsoft.Transactions.SR.GetString("InvalidMessageBody"), exception));
                }
            }
        }

        protected static void ReadFrom(XmlDictionaryReader reader, XmlDictionaryString localName, XmlDictionaryString ns)
        {
            try
            {
                if (!reader.IsStartElement(localName, ns))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidMessageException(Microsoft.Transactions.SR.GetString("InvalidMessageBody")));
                }
                bool isEmptyElement = reader.IsEmptyElement;
                reader.ReadStartElement();
                if (!isEmptyElement)
                {
                    while (reader.IsStartElement())
                    {
                        reader.Skip();
                    }
                    reader.ReadEndElement();
                }
            }
            catch (XmlException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidMessageException(Microsoft.Transactions.SR.GetString("InvalidMessageBody"), exception));
            }
        }

        protected void WriteTo(XmlDictionaryWriter writer, string prefix, XmlDictionaryString localName, XmlDictionaryString ns)
        {
            writer.WriteStartElement(prefix, localName, ns);
            writer.WriteEndElement();
        }
    }
}

