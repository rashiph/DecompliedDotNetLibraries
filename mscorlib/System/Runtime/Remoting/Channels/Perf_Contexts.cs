namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Perf_Contexts
    {
        internal int cRemoteCalls;
        internal int cChannels;
    }
}

