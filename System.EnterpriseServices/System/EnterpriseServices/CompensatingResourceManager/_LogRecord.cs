namespace System.EnterpriseServices.CompensatingResourceManager
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    internal struct _LogRecord
    {
        public int dwCrmFlags;
        public int dwSequenceNumber;
        public _BLOB blobUserData;
    }
}

