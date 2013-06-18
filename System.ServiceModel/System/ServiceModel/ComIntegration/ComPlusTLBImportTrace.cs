namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Diagnostics;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal static class ComPlusTLBImportTrace
    {
        public static void Trace(TraceEventType type, int traceCode, string description, Guid iid, Guid typeLibraryID)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                ComPlusTLBImportSchema schema = new ComPlusTLBImportSchema(iid, typeLibraryID);
                TraceUtility.TraceEvent(type, traceCode, System.ServiceModel.SR.GetString(description), (TraceRecord) schema);
            }
        }

        public static void Trace(TraceEventType type, int traceCode, string description, Guid iid, Guid typeLibraryID, string assembly)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                ComPlusTLBImportFromAssemblySchema schema = new ComPlusTLBImportFromAssemblySchema(iid, typeLibraryID, assembly);
                TraceUtility.TraceEvent(type, traceCode, System.ServiceModel.SR.GetString(description), (TraceRecord) schema);
            }
        }

        public static void Trace(TraceEventType type, int traceCode, string description, Guid iid, Guid typeLibraryID, ImporterEventKind eventKind, int eventCode, string eventMsg)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                ComPlusTLBImportConverterEventSchema schema = new ComPlusTLBImportConverterEventSchema(iid, typeLibraryID, eventKind, eventCode, eventMsg);
                TraceUtility.TraceEvent(type, traceCode, System.ServiceModel.SR.GetString(description), (TraceRecord) schema);
            }
        }
    }
}

