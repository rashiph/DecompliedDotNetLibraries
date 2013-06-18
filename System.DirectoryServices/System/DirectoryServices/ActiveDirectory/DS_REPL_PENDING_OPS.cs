namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class DS_REPL_PENDING_OPS
    {
        public long ftimeCurrentOpStarted;
        public int cNumPendingOps;
    }
}

