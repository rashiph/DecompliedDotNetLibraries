namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class berval
    {
        public int bv_len;
        public IntPtr bv_val = IntPtr.Zero;
    }
}

