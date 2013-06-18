namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Diagnostics;

    internal static class VolatileOutcomeTimeoutRecord
    {
        public static void Trace(Guid enlistmentId, string transactionId, TransactionOutcome outcome, TimeSpan timeout)
        {
            EnlistmentTimeoutRecordSchema extendedData = new EnlistmentTimeoutRecordSchema(transactionId, enlistmentId, outcome, timeout);
            TxTraceUtility.Trace(TraceEventType.Warning, 0xb0004, Microsoft.Transactions.SR.GetString("VolatileOutcomeTimeout"), extendedData, null, enlistmentId, null);
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

