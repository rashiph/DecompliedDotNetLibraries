namespace System.Runtime.InteropServices
{
    using System;
    using System.Reflection;

    [ComVisible(true), Guid("F1C3BF76-C3E4-11d3-88E7-00902754C43A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ITypeLibImporterNotifySink
    {
        void ReportEvent(ImporterEventKind eventKind, int eventCode, string eventMsg);
        Assembly ResolveRef([MarshalAs(UnmanagedType.Interface)] object typeLib);
    }
}

