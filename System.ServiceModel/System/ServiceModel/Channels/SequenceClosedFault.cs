namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal sealed class SequenceClosedFault : WsrmHeaderFault
    {
        public SequenceClosedFault(UniqueId sequenceID) : base(true, "SequenceClosed", System.ServiceModel.SR.GetString("SequenceClosedFaultString"), null, sequenceID, false, true)
        {
        }

        public SequenceClosedFault(FaultCode code, FaultReason reason, XmlDictionaryReader detailReader, ReliableMessagingVersion reliableMessagingVersion) : base(code, "SequenceClosed", reason, detailReader, reliableMessagingVersion, false, true)
        {
        }
    }
}

