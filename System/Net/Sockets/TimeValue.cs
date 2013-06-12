namespace System.Net.Sockets
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct TimeValue
    {
        public int Seconds;
        public int Microseconds;
    }
}

