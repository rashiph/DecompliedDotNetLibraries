namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Diagnostics;
    using System.ServiceModel.Diagnostics;

    internal static class ProtocolRecoveryCompleteRecord
    {
        public static void TraceAndLog(Guid protocolId, string protocolName)
        {
            using (Activity.CreateActivity(protocolId))
            {
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Information, EventLogCategory.Wsat, (EventLogEventId) (-1073545201), new string[] { protocolId.ToString(), protocolName });
            }
        }
    }
}

