namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=4)]
    internal sealed class DS_REPL_OP
    {
        public long ftimeEnqueued;
        public int ulSerialNumber;
        public int ulPriority;
        public ReplicationOperationType OpType;
        public int ulOptions;
        public IntPtr pszNamingContext;
        public IntPtr pszDsaDN;
        public IntPtr pszDsaAddress;
        public Guid uuidNamingContextObjGuid;
        public Guid uuidDsaObjGuid;
    }
}

