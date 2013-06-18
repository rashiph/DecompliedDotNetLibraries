namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using System;
    using System.Diagnostics;
    using System.ServiceModel;

    internal static class RegisterParticipantFailureRecord
    {
        private static Type type = typeof(RegisterParticipantFailureRecord);

        private static int GetCode(ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, type, "GetCode");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return 0xb0003;

                case ProtocolVersion.Version11:
                    return 0xb002b;
            }
            return 0;
        }

        public static void Trace(Guid enlistmentId, string transactionId, ControlProtocol protocol, EndpointAddress protocolService, string reason, ProtocolVersion protocolVersion)
        {
            RegisterFailureRecordSchema extendedData = RegisterFailureRecordSchema.Instance(transactionId, protocol, protocolService, reason, protocolVersion);
            TxTraceUtility.Trace(TraceEventType.Warning, GetCode(protocolVersion), Microsoft.Transactions.SR.GetString("RegisterParticipantFailure"), extendedData, null, enlistmentId, null);
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

