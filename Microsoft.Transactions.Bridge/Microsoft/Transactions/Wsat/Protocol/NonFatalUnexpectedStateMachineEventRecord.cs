namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Diagnostics;
    using System.ServiceModel.Diagnostics;

    internal static class NonFatalUnexpectedStateMachineEventRecord
    {
        public static void TraceAndLog(Guid enlistmentId, string transactionId, string stateMachine, string currentState, StateMachineHistory history, string eventName, string eventDetails)
        {
            using (Activity.CreateActivity(enlistmentId))
            {
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Warning, EventLogCategory.StateMachine, (EventLogEventId) (-1073545203), new string[] { transactionId, stateMachine, currentState, (history == null) ? string.Empty : history.ToString(), eventName, string.IsNullOrEmpty(eventDetails) ? string.Empty : eventDetails });
            }
        }
    }
}

