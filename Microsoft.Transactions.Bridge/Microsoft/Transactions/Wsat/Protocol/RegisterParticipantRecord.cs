namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using System;
    using System.Diagnostics;
    using System.ServiceModel;

    internal static class RegisterParticipantRecord
    {
        private static Type type = typeof(RegisterParticipantRecord);

        private static int GetCode(ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, type, "GetCode");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return 0xb000d;

                case ProtocolVersion.Version11:
                    return 0xb002d;
            }
            return 0;
        }

        public static void Trace(Guid enlistmentId, string transactionId, ControlProtocol protocol, EndpointAddress participantService, ProtocolVersion protocolVersion)
        {
            RegisterParticipantRecordSchema extendedData = RegisterParticipantRecordSchema.Instance(transactionId, enlistmentId, protocol, participantService, protocolVersion);
            TxTraceUtility.Trace(TraceEventType.Information, GetCode(protocolVersion), Microsoft.Transactions.SR.GetString("RegisterParticipant"), extendedData, null, enlistmentId, null);
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

