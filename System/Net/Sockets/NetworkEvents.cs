namespace System.Net.Sockets
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct NetworkEvents
    {
        public AsyncEventBits Events;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=10)]
        public int[] ErrorCodes;
    }
}

