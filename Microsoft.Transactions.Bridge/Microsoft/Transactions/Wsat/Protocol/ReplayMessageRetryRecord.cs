namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Diagnostics;

    internal static class ReplayMessageRetryRecord
    {
        public static void Trace(Guid enlistmentId, string transactionId, int count)
        {
            CoordinatorRetryMessageRecordSchema extendedData = new CoordinatorRetryMessageRecordSchema(transactionId, count);
            TxTraceUtility.Trace(TraceEventType.Information, 0xb0023, Microsoft.Transactions.SR.GetString("ReplayMessageRetry"), extendedData, null, enlistmentId, null);
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

