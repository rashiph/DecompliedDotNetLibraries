namespace System.IdentityModel.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IdentityModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Diagnostics;

    internal static class TraceUtility
    {
        private static Dictionary<int, string> traceCodes;

        static TraceUtility()
        {
            Dictionary<int, string> dictionary = new Dictionary<int, string>(3);
            dictionary.Add(0xc0000, "IdentityModel");
            dictionary.Add(0xc0002, "AuthorizationContextCreated");
            dictionary.Add(0xc0003, "AuthorizationPolicyEvaluated");
            traceCodes = dictionary;
        }

        internal static void TraceEvent(TraceEventType severity, int traceCode, string traceDescription)
        {
            TraceEvent(severity, traceCode, traceDescription, null, null, null);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void TraceEvent(TraceEventType severity, int traceCode, string traceDescription, TraceRecord extendedData, object source, Exception exception)
        {
            if (DiagnosticUtility.ShouldTrace(severity))
            {
                Guid activityId = System.Runtime.Diagnostics.DiagnosticTrace.ActivityId;
                string msdnTraceCode = System.ServiceModel.Diagnostics.DiagnosticTrace.GenerateMsdnTraceCode("System.IdentityModel", traceCodes[traceCode]);
                DiagnosticUtility.DiagnosticTrace.TraceEvent(severity, traceCode, msdnTraceCode, traceDescription, extendedData, exception, activityId, source);
            }
        }
    }
}

