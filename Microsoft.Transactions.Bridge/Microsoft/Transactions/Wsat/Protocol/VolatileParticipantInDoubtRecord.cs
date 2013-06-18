namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Diagnostics;
    using System.ServiceModel;

    internal static class VolatileParticipantInDoubtRecord
    {
        private static Type type = typeof(VolatileParticipantInDoubtRecord);

        private static int GetCode(ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, type, "GetCode");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return 0xb0029;

                case ProtocolVersion.Version11:
                    return 0xb002a;
            }
            return 0;
        }

        public static void Trace(Guid enlistmentId, EndpointAddress replyTo, ProtocolVersion protocolVersion)
        {
            VolatileEnlistmentInDoubtRecordSchema extendedData = VolatileEnlistmentInDoubtRecordSchema.Instance(enlistmentId, replyTo, protocolVersion);
            TxTraceUtility.Trace(TraceEventType.Warning, GetCode(protocolVersion), Microsoft.Transactions.SR.GetString("VolatileParticipantInDoubt"), extendedData, null, enlistmentId, null);
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

