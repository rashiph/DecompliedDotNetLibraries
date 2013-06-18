namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class DS_REPSYNCALL_ERRINFO
    {
        public IntPtr pszSvrId;
        public SyncFromAllServersErrorCategory error;
        public int dwWin32Err;
        public IntPtr pszSrcId;
    }
}

