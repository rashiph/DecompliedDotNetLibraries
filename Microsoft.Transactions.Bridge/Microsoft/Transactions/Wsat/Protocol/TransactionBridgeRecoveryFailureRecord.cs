namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Diagnostics;
    using System.ServiceModel.Diagnostics;

    internal static class TransactionBridgeRecoveryFailureRecord
    {
        public static void TraceAndLog(Exception e)
        {
            DiagnosticUtility.EventLog.LogEvent(TraceEventType.Critical, EventLogCategory.Wsat, (EventLogEventId) (-1073545205), new string[] { e.ToString() });
        }
    }
}

