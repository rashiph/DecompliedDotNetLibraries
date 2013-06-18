namespace System.Web.Management
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal class ConversionEventSink : ITypeLibExporterNotifySink
    {
        public void ReportEvent(ExporterEventKind eventKind, int eventCode, string eventMsg)
        {
        }

        public object ResolveRef(Assembly assemblyReference)
        {
            return null;
        }
    }
}

