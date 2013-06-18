namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Diagnostics;

    internal static class DurableParticipantReplayWhilePreparingRecord
    {
        public static void Trace(Guid enlistmentId, string transactionId)
        {
            EnlistmentRecordSchema extendedData = new EnlistmentRecordSchema(transactionId, enlistmentId);
            TxTraceUtility.Trace(TraceEventType.Warning, 0xb0005, Microsoft.Transactions.SR.GetString("DurableParticipantReplayWhilePreparing"), extendedData, null, enlistmentId, null);
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

