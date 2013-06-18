namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal sealed class LastMessageNumberExceededFault : WsrmHeaderFault
    {
        public LastMessageNumberExceededFault(UniqueId sequenceID) : base(true, "LastMessageNumberExceeded", System.ServiceModel.SR.GetString("LastMessageNumberExceededFaultReason"), System.ServiceModel.SR.GetString("LastMessageNumberExceeded"), sequenceID, false, true)
        {
        }

        public LastMessageNumberExceededFault(FaultCode code, FaultReason reason, XmlDictionaryReader detailReader, ReliableMessagingVersion reliableMessagingVersion) : base(code, "LastMessageNumberExceeded", reason, detailReader, reliableMessagingVersion, false, true)
        {
        }
    }
}

