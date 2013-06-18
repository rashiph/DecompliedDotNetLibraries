namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using System;
    using System.Diagnostics;
    using System.ServiceModel;

    internal static class RegistrationCoordinatorResponseInvalidMetadataRecord
    {
        private static Type type = typeof(RegistrationCoordinatorResponseInvalidMetadataRecord);

        private static int GetCode(ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, type, "GetCode");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return 0xb0008;

                case ProtocolVersion.Version11:
                    return 0xb0032;
            }
            return 0;
        }

        public static void Trace(Guid enlistmentId, CoordinationContext context, ControlProtocol protocol, EndpointAddress coordinatorService, Exception e, ProtocolVersion protocolVersion)
        {
            RegistrationCoordinatorResponseInvalidMetadataSchema extendedData = RegistrationCoordinatorResponseInvalidMetadataSchema.Instance(context, protocol, coordinatorService, protocolVersion);
            TxTraceUtility.Trace(TraceEventType.Warning, GetCode(protocolVersion), Microsoft.Transactions.SR.GetString("RegistrationCoordinatorResponseInvalidMetadata"), extendedData, e, enlistmentId, null);
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

