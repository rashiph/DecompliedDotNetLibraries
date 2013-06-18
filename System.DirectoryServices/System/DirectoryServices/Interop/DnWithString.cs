namespace System.DirectoryServices.Interop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class DnWithString
    {
        public IntPtr pszStringValue;
        public IntPtr pszDNString;
    }
}

