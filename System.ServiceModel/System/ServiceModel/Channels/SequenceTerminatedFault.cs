namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal sealed class SequenceTerminatedFault : WsrmHeaderFault
    {
        private SequenceTerminatedFault(bool isSenderFault, UniqueId sequenceID, string faultReason, string exceptionMessage) : base(isSenderFault, "SequenceTerminated", faultReason, exceptionMessage, sequenceID, true, true)
        {
        }

        public SequenceTerminatedFault(FaultCode code, FaultReason reason, XmlDictionaryReader detailReader, ReliableMessagingVersion reliableMessagingVersion) : base(code, "SequenceTerminated", reason, detailReader, reliableMessagingVersion, true, true)
        {
        }

        public static WsrmFault CreateCommunicationFault(UniqueId sequenceID, string faultReason, string exceptionMessage)
        {
            return new SequenceTerminatedFault(false, sequenceID, faultReason, exceptionMessage);
        }

        public static WsrmFault CreateMaxRetryCountExceededFault(UniqueId sequenceId)
        {
            return CreateCommunicationFault(sequenceId, System.ServiceModel.SR.GetString("SequenceTerminatedMaximumRetryCountExceeded"), null);
        }

        public static WsrmFault CreateProtocolFault(UniqueId sequenceID, string faultReason, string exceptionMessage)
        {
            return new SequenceTerminatedFault(true, sequenceID, faultReason, exceptionMessage);
        }

        public static WsrmFault CreateQuotaExceededFault(UniqueId sequenceID)
        {
            return CreateProtocolFault(sequenceID, System.ServiceModel.SR.GetString("SequenceTerminatedQuotaExceededException"), null);
        }
    }
}

