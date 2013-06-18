namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Diagnostics;
    using System.ServiceModel.Diagnostics;

    internal static class ParticipantRecoveryLogEntryCreationFailureRecord
    {
        public static void TraceAndLog(Guid enlistmentId, string transactionId, string reason, Exception e)
        {
            using (Activity.CreateActivity(enlistmentId))
            {
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error, EventLogCategory.StateMachine, (EventLogEventId) (-1073545210), new string[] { transactionId, enlistmentId.ToString(), reason, e.ToString() });
            }
        }
    }
}

