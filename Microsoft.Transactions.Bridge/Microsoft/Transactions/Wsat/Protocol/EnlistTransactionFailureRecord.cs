namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using System;
    using System.Diagnostics;

    internal static class EnlistTransactionFailureRecord
    {
        public static void Trace(Guid enlistmentId, CoordinationContext context, string reason)
        {
            ReasonRecordSchema extendedData = new ReasonRecordSchema(reason);
            TxTraceUtility.Trace(TraceEventType.Warning, 0xb0002, Microsoft.Transactions.SR.GetString("EnlistTransactionFailure"), extendedData, null, enlistmentId, null);
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

