namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using System;
    using System.Diagnostics;
    using System.ServiceModel.Channels;

    internal static class RegistrationCoordinatorFaultedRecord
    {
        public static void Trace(Guid enlistmentId, CoordinationContext context, ControlProtocol protocol, MessageFault fault)
        {
            RegistrationCoordinatorFaultedSchema extendedData = new RegistrationCoordinatorFaultedSchema(context, protocol, fault);
            TxTraceUtility.Trace(TraceEventType.Warning, 0xb0006, Microsoft.Transactions.SR.GetString("RegistrationCoordinatorFaulted"), extendedData, null, enlistmentId, null);
        }

        public static bool ShouldTrace
        {
            get
            {
                return DiagnosticUtility.ShouldTraceWarning;
            }
        }
    }
}

