namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Diagnostics;
    using System.ServiceModel;

    internal static class CoordinatorRecoveredRecord
    {
        private static Type type = typeof(CoordinatorRecoveredRecord);

        private static int GetCode(ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, type, "GetCode");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return 0xb0025;

                case ProtocolVersion.Version11:
                    return 0xb002f;
            }
            return 0;
        }

        public static void Trace(Guid enlistmentId, string transactionId, EndpointAddress protocolService, ProtocolVersion protocolVersion)
        {
            RecoverCoordinatorRecordSchema extendedData = RecoverCoordinatorRecordSchema.Instance(transactionId, protocolService, protocolVersion);
            TxTraceUtility.Trace(TraceEventType.Information, GetCode(protocolVersion), Microsoft.Transactions.SR.GetString("CoordinatorRecovered"), extendedData, null, enlistmentId, null);
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

