namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal class CommitMessage : NotificationMessage
    {
        public CommitMessage(MessageVersion version, ProtocolVersion protocolVersion) : base(AtomicTransactionStrings.Version(protocolVersion).CommitAction, version, protocolVersion)
        {
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            base.WriteTo(writer, base.atomicTransactionStrings.Prefix, base.atomicTransactionXmlDictionaryStrings.Commit, base.atomicTransactionXmlDictionaryStrings.Namespace);
        }

        public static void ReadFrom(Message message, ProtocolVersion protocolVersion)
        {
            AtomicTransactionXmlDictionaryStrings strings = AtomicTransactionXmlDictionaryStrings.Version(protocolVersion);
            NotificationMessage.ReadFrom(message, strings.Commit, strings.Namespace);
        }

        public static void ReadFrom(XmlDictionaryReader reader, ProtocolVersion protocolVersion)
        {
            AtomicTransactionXmlDictionaryStrings strings = AtomicTransactionXmlDictionaryStrings.Version(protocolVersion);
            NotificationMessage.ReadFrom(reader, strings.Commit, strings.Namespace);
        }
    }
}

