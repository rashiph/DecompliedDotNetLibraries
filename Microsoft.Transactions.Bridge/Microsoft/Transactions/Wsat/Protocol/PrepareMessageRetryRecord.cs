namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Diagnostics;

    internal static class PrepareMessageRetryRecord
    {
        public static void Trace(Guid enlistmentId, string transactionId, int count)
        {
            ParticipantRetryMessageRecordSchema extendedData = new ParticipantRetryMessageRecordSchema(transactionId, enlistmentId, count);
            TxTraceUtility.Trace(TraceEventType.Information, 0xb0020, Microsoft.Transactions.SR.GetString("PrepareMessageRetry"), extendedData, null, enlistmentId, null);
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

