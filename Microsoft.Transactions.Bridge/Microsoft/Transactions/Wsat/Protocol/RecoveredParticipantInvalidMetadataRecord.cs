namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Diagnostics;
    using System.ServiceModel;

    internal static class RecoveredParticipantInvalidMetadataRecord
    {
        private static Type type = typeof(RecoveredParticipantInvalidMetadataRecord);

        private static int GetCode(ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, type, "GetCode");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return 0xb000a;

                case ProtocolVersion.Version11:
                    return 0xb0031;
            }
            return 0;
        }

        public static void Trace(Guid enlistmentId, string transactionId, EndpointAddress participantService, ProtocolVersion protocolVersion)
        {
            RecoverParticipantRecordSchema extendedData = RecoverParticipantRecordSchema.Instance(transactionId, enlistmentId, participantService, protocolVersion);
            TxTraceUtility.Trace(TraceEventType.Warning, GetCode(protocolVersion), Microsoft.Transactions.SR.GetString("RecoveredParticipantInvalidMetadata"), extendedData, null, enlistmentId, null);
        }

        public static bool ShouldTrace
        {
            get
            {
                return Microsoft.Transactions.Bridge.DiagnosticUtility.ShouldTraceWarning;
            }
        }
    }
}

