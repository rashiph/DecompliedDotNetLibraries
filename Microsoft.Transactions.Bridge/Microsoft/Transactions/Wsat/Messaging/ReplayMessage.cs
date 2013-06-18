namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal class ReplayMessage : NotificationMessage
    {
        public ReplayMessage(MessageVersion version, ProtocolVersion protocolVersion) : base(AtomicTransactionStrings.Version(protocolVersion).ReplayAction, version, protocolVersion)
        {
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            base.WriteTo(writer, base.atomicTransactionStrings.Prefix, base.atomicTransactionXmlDictionaryStrings.Replay, base.atomicTransactionXmlDictionaryStrings.Namespace);
        }

        public static void ReadFrom(Message message, ProtocolVersion protocolVersion)
        {
            AtomicTransactionXmlDictionaryStrings strings = AtomicTransactionXmlDictionaryStrings.Version(protocolVersion);
            NotificationMessage.ReadFrom(message, strings.Replay, strings.Namespace);
        }

        public static void ReadFrom(XmlDictionaryReader reader, ProtocolVersion protocolVersion)
        {
            AtomicTransactionXmlDictionaryStrings strings = AtomicTransactionXmlDictionaryStrings.Version(protocolVersion);
            NotificationMessage.ReadFrom(reader, strings.Replay, strings.Namespace);
        }
    }
}

