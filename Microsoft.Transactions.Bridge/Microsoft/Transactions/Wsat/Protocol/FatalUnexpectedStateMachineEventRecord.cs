namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Diagnostics;
    using System.ServiceModel.Diagnostics;

    internal static class FatalUnexpectedStateMachineEventRecord
    {
        public static void TraceAndLog(Guid enlistmentId, string transactionId, string stateMachineName, string currentState, StateMachineHistory history, string eventName, string eventDetails)
        {
            DiagnosticUtility.EventLog.LogEvent(TraceEventType.Critical, EventLogCategory.StateMachine, (EventLogEventId) (-1073545214), new string[] { transactionId, stateMachineName, currentState, (history == null) ? string.Empty : history.ToString(), eventName, string.IsNullOrEmpty(eventDetails) ? string.Empty : eventDetails });
        }
    }
}

