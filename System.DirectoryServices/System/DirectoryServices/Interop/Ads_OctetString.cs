namespace System.DirectoryServices.Interop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Ads_OctetString
    {
        public int length;
        public IntPtr value;
    }
}

