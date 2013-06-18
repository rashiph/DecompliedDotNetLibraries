namespace System.ServiceModel.Activation.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal static class TraceUtility
    {
        private static Dictionary<int, string> traceCodes;

        static TraceUtility()
        {
            Dictionary<int, string> dictionary = new Dictionary<int, string>(7);
            dictionary.Add(0x90001, "WebHostFailedToCompile");
            dictionary.Add(0x90002, "WebHostServiceActivated");
            dictionary.Add(0x90003, "WebHostFailedToActivateService");
            dictionary.Add(0x90004, "WebHostCompilation");
            dictionary.Add(0x90005, "WebHostDebugRequest");
            dictionary.Add(0x90006, "WebHostProtocolMisconfigured");
            dictionary.Add(0x90007, "WebHostServiceCloseFailed");
            dictionary.Add(0x90008, "WebHostNoCBTSupport");
            traceCodes = dictionary;
        }

        internal static string CreateSourceString(object source)
        {
            return (source.GetType().ToString() + "/" + source.GetHashCode().ToString(CultureInfo.CurrentCulture));
        }

        internal static void TraceEvent(TraceEventType severity, int traceCode, string traceDescription, object source)
        {
            TraceEvent(severity, traceCode, traceDescription, null, source, null);
        }

        internal static void TraceEvent(TraceEventType severity, int traceCode, string traceDescription, object source, Exception exception)
        {
            TraceEvent(severity, traceCode, traceDescription, null, source, exception);
        }

        internal static void TraceEvent(TraceEventType severity, int traceCode, string traceDescription, TraceRecord record, object source, Exception exception)
        {
            string msdnTraceCode = System.ServiceModel.Diagnostics.DiagnosticTrace.GenerateMsdnTraceCode("System.ServiceModel.Activation", traceCodes[traceCode]);
            DiagnosticUtility.DiagnosticTrace.TraceEvent(severity, traceCode, msdnTraceCode, traceDescription, record, exception, source);
        }
    }
}

