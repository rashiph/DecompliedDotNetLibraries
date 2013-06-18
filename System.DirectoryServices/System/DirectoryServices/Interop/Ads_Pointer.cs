namespace System.DirectoryServices.Interop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Ads_Pointer
    {
        public IntPtr value;
    }
}

