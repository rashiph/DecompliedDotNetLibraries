namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SecurityHandle
    {
        public IntPtr Lower;
        public IntPtr Upper;
    }
}

