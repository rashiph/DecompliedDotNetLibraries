namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using System;
    using System.Diagnostics;

    internal static class RegistrationCoordinatorFailedRecord
    {
        public static void Trace(Guid enlistmentId, CoordinationContext context, ControlProtocol protocol, Exception e)
        {
            RegistrationCoordinatorFailedSchema extendedData = new RegistrationCoordinatorFailedSchema(context, protocol);
            TxTraceUtility.Trace(TraceEventType.Warning, 0xb0007, Microsoft.Transactions.SR.GetString("RegistrationCoordinatorFailed"), extendedData, null, enlistmentId, null);
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

