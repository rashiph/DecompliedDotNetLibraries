namespace System.Transactions.Oletx
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential), ComVisible(false)]
    internal struct OletxXactTransInfo
    {
        internal Guid uow;
        internal OletxTransactionIsolationLevel isoLevel;
        internal OletxTransactionIsoFlags isoFlags;
        internal int grfTCSupported;
        internal int grfRMSupported;
        internal int grfTCSupportedRetaining;
        internal int grfRMSupportedRetaining;
        internal OletxXactTransInfo(Guid guid, OletxTransactionIsolationLevel isoLevel)
        {
            this.uow = guid;
            this.isoLevel = isoLevel;
            this.isoFlags = OletxTransactionIsoFlags.ISOFLAG_NONE;
            this.grfTCSupported = 0;
            this.grfRMSupported = 0;
            this.grfTCSupportedRetaining = 0;
            this.grfRMSupportedRetaining = 0;
        }
    }
}

