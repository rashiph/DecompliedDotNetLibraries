namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class DS_REPSYNCALL_UPDATE
    {
        public SyncFromAllServersEvent eventType;
        public IntPtr pErrInfo;
        public IntPtr pSync;
    }
}

