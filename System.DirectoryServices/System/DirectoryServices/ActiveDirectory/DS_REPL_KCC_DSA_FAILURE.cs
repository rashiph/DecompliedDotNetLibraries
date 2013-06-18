namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=4)]
    internal sealed class DS_REPL_KCC_DSA_FAILURE
    {
        public IntPtr pszDsaDN;
        public Guid uuidDsaObjGuid;
        public long ftimeFirstFailure;
        public int cNumFailures;
        public int dwLastResult;
    }
}

