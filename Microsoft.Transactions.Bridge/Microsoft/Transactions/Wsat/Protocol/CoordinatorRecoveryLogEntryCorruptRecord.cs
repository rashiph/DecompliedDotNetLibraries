namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Diagnostics;
    using System.ServiceModel.Diagnostics;

    internal static class CoordinatorRecoveryLogEntryCorruptRecord
    {
        public static void TraceAndLog(Guid localTransactionId, string transactionId, byte[] recoveryData, Exception e)
        {
            using (Activity.CreateActivity(localTransactionId))
            {
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Critical, EventLogCategory.StateMachine, (EventLogEventId) (-1073545212), new string[] { transactionId, Convert.ToBase64String(recoveryData), e.ToString() });
            }
        }
    }
}

