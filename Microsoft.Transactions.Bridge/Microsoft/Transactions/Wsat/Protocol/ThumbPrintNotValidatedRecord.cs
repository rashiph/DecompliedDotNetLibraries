namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Diagnostics;
    using System.ServiceModel.Diagnostics;

    internal static class ThumbPrintNotValidatedRecord
    {
        public static void TraceAndLog(string thumbPrint)
        {
            DiagnosticUtility.EventLog.LogEvent(TraceEventType.Information, EventLogCategory.Wsat, (EventLogEventId) (-1073545198), new string[] { thumbPrint });
        }
    }
}

