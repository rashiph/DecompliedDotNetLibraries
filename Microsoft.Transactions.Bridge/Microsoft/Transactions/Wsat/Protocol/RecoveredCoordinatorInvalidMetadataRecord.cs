namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Diagnostics;
    using System.ServiceModel;

    internal static class RecoveredCoordinatorInvalidMetadataRecord
    {
        private static Type type = typeof(RecoveredCoordinatorInvalidMetadataRecord);

        private static int GetCode(ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, type, "GetCode");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return 0xb0009;

                case ProtocolVersion.Version11:
                    return 0xb0030;
            }
            return 0;
        }

        public static void Trace(Guid enlistmentId, string transactionId, EndpointAddress coordinatorService, ProtocolVersion protocolVersion)
        {
            RecoverCoordinatorRecordSchema extendedData = RecoverCoordinatorRecordSchema.Instance(transactionId, coordinatorService, protocolVersion);
            TxTraceUtility.Trace(TraceEventType.Warning, GetCode(protocolVersion), Microsoft.Transactions.SR.GetString("RecoveredCoordinatorInvalidMetadata"), extendedData, null, enlistmentId, null);
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

