namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Diagnostics;
    using System.ServiceModel.Diagnostics;

    internal static class SslNoAccessiblePrivateKeyRecord
    {
        public static void TraceAndLog(string subject, string thumbPrint)
        {
            DiagnosticUtility.EventLog.LogEvent(TraceEventType.Information, EventLogCategory.Wsat, (EventLogEventId) (-1073545196), new string[] { subject, thumbPrint });
        }
    }
}

