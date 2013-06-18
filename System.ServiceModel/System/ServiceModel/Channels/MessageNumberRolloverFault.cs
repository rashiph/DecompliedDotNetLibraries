namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal sealed class MessageNumberRolloverFault : WsrmHeaderFault
    {
        public MessageNumberRolloverFault(UniqueId sequenceID) : base(true, "MessageNumberRollover", System.ServiceModel.SR.GetString("MessageNumberRolloverFaultReason"), System.ServiceModel.SR.GetString("MessageNumberRollover"), sequenceID, true, true)
        {
        }

        public MessageNumberRolloverFault(FaultCode code, FaultReason reason, XmlDictionaryReader detailReader, ReliableMessagingVersion reliableMessagingVersion) : base(code, "MessageNumberRollover", reason, true, true)
        {
            try
            {
                base.SequenceID = WsrmUtilities.ReadIdentifier(detailReader, reliableMessagingVersion);
                if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
                {
                    ulong num;
                    detailReader.ReadStartElement(DXD.Wsrm11Dictionary.MaxMessageNumber, WsrmIndex.GetNamespace(reliableMessagingVersion));
                    if (!ulong.TryParse(detailReader.ReadContentAsString(), out num) || (num <= 0L))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("InvalidSequenceNumber", new object[] { num })));
                    }
                    detailReader.ReadEndElement();
                }
            }
            finally
            {
                detailReader.Close();
            }
        }

        protected override void OnWriteDetailContents(XmlDictionaryWriter writer)
        {
            ReliableMessagingVersion reliableMessagingVersion = base.GetReliableMessagingVersion();
            WsrmUtilities.WriteIdentifier(writer, reliableMessagingVersion, base.SequenceID);
            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                writer.WriteStartElement("r", DXD.Wsrm11Dictionary.MaxMessageNumber, WsrmIndex.GetNamespace(reliableMessagingVersion));
                writer.WriteValue((long) 0x7fffffffffffffffL);
                writer.WriteEndElement();
            }
        }
    }
}

