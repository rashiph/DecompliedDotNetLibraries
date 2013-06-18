namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    internal struct CRYPTOAPI_BLOB
    {
        public int cbData;
        public IntPtr pbData;
    }
}

