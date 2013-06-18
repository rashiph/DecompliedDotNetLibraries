namespace Microsoft.Transactions.Bridge
{
    using System;
    using System.Diagnostics;
    using System.Globalization;

    internal static class PropagationProtocolsTracing
    {
        private static DateTimeFormatInfo dateTimeFormatInfo = DateTimeFormatInfo.InvariantInfo;
        private static TraceSwitch pplTraceSwitch = new TraceSwitch("Microsoft.Transactions.Bridge", "MSTB PPL Tracing");

        public static void TraceError(string message)
        {
            Trace.WriteLineIf(pplTraceSwitch.TraceError, DateTime.Now.ToString("u", dateTimeFormatInfo) + " [PPL] : " + message);
        }

        public static void TraceVerbose(string message)
        {
            Trace.WriteLineIf(pplTraceSwitch.TraceVerbose, DateTime.Now.ToString("u", dateTimeFormatInfo) + " [PPL] : " + message);
        }
    }
}

