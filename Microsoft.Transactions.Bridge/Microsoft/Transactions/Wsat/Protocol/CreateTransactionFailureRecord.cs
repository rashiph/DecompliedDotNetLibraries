namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Diagnostics;

    internal static class CreateTransactionFailureRecord
    {
        public static void Trace(Guid enlistmentId, string reason)
        {
            ReasonRecordSchema extendedData = new ReasonRecordSchema(reason);
            TxTraceUtility.Trace(TraceEventType.Warning, 0xb0001, Microsoft.Transactions.SR.GetString("CreateTransactionFailure"), extendedData, null, enlistmentId, null);
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

