namespace System.EnterpriseServices.CompensatingResourceManager
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    internal struct _BLOB
    {
        public int cbSize;
        public IntPtr pBlobData;
    }
}

