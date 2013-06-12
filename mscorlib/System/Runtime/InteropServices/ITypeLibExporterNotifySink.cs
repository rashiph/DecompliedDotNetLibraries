namespace System.Runtime.InteropServices
{
    using System;
    using System.Reflection;

    [Guid("F1C3BF77-C3E4-11d3-88E7-00902754C43A"), ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ITypeLibExporterNotifySink
    {
        void ReportEvent(ExporterEventKind eventKind, int eventCode, string eventMsg);
        [return: MarshalAs(UnmanagedType.Interface)]
        object ResolveRef(Assembly assembly);
    }
}

