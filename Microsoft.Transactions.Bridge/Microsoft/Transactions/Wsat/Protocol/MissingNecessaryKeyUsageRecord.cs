namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Diagnostics;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel.Diagnostics;

    internal static class MissingNecessaryKeyUsageRecord
    {
        public static void TraceAndLog(string subject, string thumbPrint, X509KeyUsageFlags keyUsage)
        {
            DiagnosticUtility.EventLog.LogEvent(TraceEventType.Information, EventLogCategory.Wsat, (EventLogEventId) (-1073545195), new string[] { subject, thumbPrint, keyUsage.ToString() });
        }
    }
}

