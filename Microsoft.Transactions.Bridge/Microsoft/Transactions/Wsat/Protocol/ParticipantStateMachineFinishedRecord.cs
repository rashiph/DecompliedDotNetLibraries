namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Diagnostics;

    internal static class ParticipantStateMachineFinishedRecord
    {
        public static void Trace(Guid enlistmentId, string transactionId, TransactionOutcome outcome)
        {
            ParticipantOutcomeRecordSchema extendedData = new ParticipantOutcomeRecordSchema(transactionId, enlistmentId, outcome);
            TxTraceUtility.Trace(TraceEventType.Verbose, 0xb0027, Microsoft.Transactions.SR.GetString("ParticipantStateMachineFinished"), extendedData, null, enlistmentId, null);
        }

        public static bool ShouldTrace
        {
            get
            {
                return DiagnosticUtility.ShouldTraceVerbose;
            }
        }
    }
}

