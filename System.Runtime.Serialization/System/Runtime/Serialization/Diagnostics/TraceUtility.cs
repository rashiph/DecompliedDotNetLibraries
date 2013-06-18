namespace System.Runtime.Serialization.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Runtime.Serialization;
    using System.ServiceModel.Diagnostics;

    internal static class TraceUtility
    {
        private static Dictionary<int, string> traceCodes;

        static TraceUtility()
        {
            Dictionary<int, string> dictionary = new Dictionary<int, string>(0x12);
            dictionary.Add(0x30001, "WriteObjectBegin");
            dictionary.Add(0x30002, "WriteObjectEnd");
            dictionary.Add(0x30003, "WriteObjectContentBegin");
            dictionary.Add(0x30004, "WriteObjectContentEnd");
            dictionary.Add(0x30005, "ReadObjectBegin");
            dictionary.Add(0x30006, "ReadObjectEnd");
            dictionary.Add(0x30007, "ElementIgnored");
            dictionary.Add(0x30008, "XsdExportBegin");
            dictionary.Add(0x30009, "XsdExportEnd");
            dictionary.Add(0x3000a, "XsdImportBegin");
            dictionary.Add(0x3000b, "XsdImportEnd");
            dictionary.Add(0x3000c, "XsdExportError");
            dictionary.Add(0x3000d, "XsdImportError");
            dictionary.Add(0x3000e, "XsdExportAnnotationFailed");
            dictionary.Add(0x3000f, "XsdImportAnnotationFailed");
            dictionary.Add(0x30010, "XsdExportDupItems");
            dictionary.Add(0x30011, "FactoryTypeNotFound");
            dictionary.Add(0x30012, "ObjectWithLargeDepth");
            traceCodes = dictionary;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal static void Trace(TraceEventType severity, int traceCode, string traceDescription)
        {
            Trace(severity, traceCode, traceDescription, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal static void Trace(TraceEventType severity, int traceCode, string traceDescription, TraceRecord record)
        {
            Trace(severity, traceCode, traceDescription, record, null);
        }

        internal static void Trace(TraceEventType severity, int traceCode, string traceDescription, TraceRecord record, Exception exception)
        {
            string msdnTraceCode = System.ServiceModel.Diagnostics.DiagnosticTrace.GenerateMsdnTraceCode("System.Runtime.Serialization", traceCodes[traceCode]);
            DiagnosticUtility.DiagnosticTrace.TraceEvent(severity, traceCode, msdnTraceCode, traceDescription, record, exception, null);
        }
    }
}

