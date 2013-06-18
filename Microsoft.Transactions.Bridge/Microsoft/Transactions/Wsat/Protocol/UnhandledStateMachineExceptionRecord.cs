namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Diagnostics;
    using System.ServiceModel.Diagnostics;

    internal static class UnhandledStateMachineExceptionRecord
    {
        public static void TraceAndLog(Guid enlistmentId, string transactionId, string stateMachineName, string currentState, StateMachineHistory history, Exception e)
        {
            DiagnosticUtility.EventLog.LogEvent(TraceEventType.Critical, EventLogCategory.StateMachine, (EventLogEventId) (-1073545215), new string[] { transactionId, stateMachineName, currentState, (history == null) ? string.Empty : history.ToString(), enlistmentId.ToString(), e.ToString() });
        }
    }
}

