namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct WSABuffer
    {
        internal int Length;
        internal IntPtr Pointer;
    }
}

