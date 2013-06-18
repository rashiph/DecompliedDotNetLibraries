namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Diagnostics;
    using System.ServiceModel;

    internal static class ParticipantRecoveredRecord
    {
        private static Type type = typeof(ParticipantRecoveredRecord);

        private static int GetCode(ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, type, "GetCode");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return 0xb0024;

                case ProtocolVersion.Version11:
                    return 0xb002e;
            }
            return 0;
        }

        public static void Trace(Guid enlistmentId, string transactionId, EndpointAddress protocolService, ProtocolVersion protocolVersion)
        {
            RecoverParticipantRecordSchema extendedData = RecoverParticipantRecordSchema.Instance(transactionId, enlistmentId, protocolService, protocolVersion);
            TxTraceUtility.Trace(TraceEventType.Information, GetCode(protocolVersion), Microsoft.Transactions.SR.GetString("ParticipantRecovered"), extendedData, null, enlistmentId, null);
        }

        public static bool ShouldTrace
        {
            get
            {
                return Microsoft.Transactions.Bridge.DiagnosticUtility.ShouldTraceInformation;
            }
        }
    }
}

