namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Linger
    {
        internal ushort OnOff;
        internal ushort Time;
    }
}

