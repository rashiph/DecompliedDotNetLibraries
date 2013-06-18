namespace System.Workflow.Runtime.DebugEngine
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("e5e93adb-a6fe-435e-8640-31ae310d812f")]
    internal interface IWDEProgramNode
    {
        void Attach(ref Guid programId, int attachTimeout, int detachPingInterval, [MarshalAs(UnmanagedType.BStr)] out string hostName, [MarshalAs(UnmanagedType.BStr)] out string uri, out int controllerThreadId, [MarshalAs(UnmanagedType.Bool)] out bool isSynchronousAttach);
    }
}

