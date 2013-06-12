namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct _CERT_CHAIN_ELEMENT
    {
        public uint cbSize;
        public IntPtr pCertContext;
    }
}

