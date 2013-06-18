namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class DS_REPSYNCALL_SYNC
    {
        public IntPtr pszSrcId;
        public IntPtr pszDstId;
    }
}

