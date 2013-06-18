namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Diagnostics;

    internal static class EnlistmentIdentityCheckFailedRecord
    {
        public static void Trace(Guid enlistmentId)
        {
            TxTraceUtility.Trace(TraceEventType.Information, 0xb0026, Microsoft.Transactions.SR.GetString("EnlistmentIdentityCheckFailed"), null, null, enlistmentId, null);
        }

        public static bool ShouldTrace
        {
            get
            {
                return DiagnosticUtility.ShouldTraceInformation;
            }
        }
    }
}

