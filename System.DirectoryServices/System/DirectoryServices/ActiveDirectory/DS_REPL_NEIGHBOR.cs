namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class DS_REPL_NEIGHBOR
    {
        public IntPtr pszNamingContext;
        public IntPtr pszSourceDsaDN;
        public IntPtr pszSourceDsaAddress;
        public IntPtr pszAsyncIntersiteTransportDN;
        public int dwReplicaFlags;
        public int dwReserved;
        public Guid uuidNamingContextObjGuid;
        public Guid uuidSourceDsaObjGuid;
        public Guid uuidSourceDsaInvocationID;
        public Guid uuidAsyncIntersiteTransportObjGuid;
        public long usnLastObjChangeSynced;
        public long usnAttributeFilter;
        public long ftimeLastSyncSuccess;
        public long ftimeLastSyncAttempt;
        public int dwLastSyncResult;
        public int cNumConsecutiveSyncFailures;
    }
}

