namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Diagnostics;
    using System.ServiceModel.Diagnostics;

    internal static class ProtocolStopFailureRecord
    {
        public static void TraceAndLog(Guid protocolId, string protocolName, Exception e)
        {
            using (Activity.CreateActivity(protocolId))
            {
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error, EventLogCategory.Wsat, (EventLogEventId) (-1073545204), new string[] { protocolId.ToString(), protocolName, e.ToString() });
            }
        }
    }
}

