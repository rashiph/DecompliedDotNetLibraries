namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Diagnostics;
    using System.ServiceModel.Diagnostics;

    internal static class MissingNecessaryEnhancedKeyUsageRecord
    {
        public static void TraceAndLog(string subject, string thumbPrint, string keyUsage)
        {
            DiagnosticUtility.EventLog.LogEvent(TraceEventType.Information, EventLogCategory.Wsat, (EventLogEventId) (-1073545194), new string[] { subject, thumbPrint, keyUsage });
        }
    }
}

