namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Diagnostics;

    internal static class CoordinatorStateMachineFinishedRecord
    {
        public static void Trace(Guid enlistmentId, string transactionId, TransactionOutcome outcome)
        {
            CoordinatorOutcomeRecordSchema extendedData = new CoordinatorOutcomeRecordSchema(transactionId, outcome);
            TxTraceUtility.Trace(TraceEventType.Verbose, 0xb0028, Microsoft.Transactions.SR.GetString("CoordinatorStateMachineFinished"), extendedData, null, enlistmentId, null);
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

