namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using System;
    using System.Diagnostics;
    using System.ServiceModel;

    internal static class RegisterCoordinatorRecord
    {
        private static Type type = typeof(RegisterCoordinatorRecord);

        private static int GetCode(ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, type, "GetCode");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return 0xb000c;

                case ProtocolVersion.Version11:
                    return 0xb002c;
            }
            return 0;
        }

        public static void Trace(Guid enlistmentId, CoordinationContext context, ControlProtocol protocol, EndpointAddress coordinatorService, ProtocolVersion protocolVersion)
        {
            RegisterCoordinatorRecordSchema extendedData = RegisterCoordinatorRecordSchema.Instance(context, protocol, coordinatorService, protocolVersion);
            TxTraceUtility.Trace(TraceEventType.Information, GetCode(protocolVersion), Microsoft.Transactions.SR.GetString("RegisterCoordinator"), extendedData, null, enlistmentId, null);
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

