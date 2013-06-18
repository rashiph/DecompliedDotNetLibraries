namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal sealed class UnknownSequenceFault : WsrmHeaderFault
    {
        public UnknownSequenceFault(UniqueId sequenceID) : base(true, "UnknownSequence", System.ServiceModel.SR.GetString("UnknownSequenceFaultReason"), System.ServiceModel.SR.GetString("UnknownSequenceMessageReceived"), sequenceID, true, true)
        {
        }

        public UnknownSequenceFault(FaultCode code, FaultReason reason, XmlDictionaryReader detailReader, ReliableMessagingVersion reliableMessagingVersion) : base(code, "UnknownSequence", reason, detailReader, reliableMessagingVersion, true, true)
        {
        }

        public override CommunicationException CreateException()
        {
            string safeReasonText;
            if (base.IsRemote)
            {
                safeReasonText = FaultException.GetSafeReasonText(this.Reason);
                safeReasonText = System.ServiceModel.SR.GetString("UnknownSequenceFaultReceived", new object[] { safeReasonText });
            }
            else
            {
                safeReasonText = base.GetExceptionMessage();
            }
            return new CommunicationException(safeReasonText);
        }
    }
}

