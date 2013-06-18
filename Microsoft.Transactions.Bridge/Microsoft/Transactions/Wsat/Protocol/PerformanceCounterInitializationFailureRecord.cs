namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Diagnostics;
    using System.ServiceModel.Diagnostics;

    internal static class PerformanceCounterInitializationFailureRecord
    {
        public static void TraceAndLog(Guid protocolId, string counterName, Exception e)
        {
            using (Activity.CreateActivity(protocolId))
            {
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Warning, EventLogCategory.PerformanceCounter, (EventLogEventId) (-1073545202), new string[] { counterName, e.ToString() });
            }
        }
    }
}

