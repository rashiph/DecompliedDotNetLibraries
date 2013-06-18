namespace System.DirectoryServices.Interop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class DnWithBinary
    {
        public int dwLength;
        public IntPtr lpBinaryValue;
        public IntPtr pszDNString;
    }
}

