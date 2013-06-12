namespace System.Net.Sockets
{
    using System;

    [Flags]
    public enum SocketFlags
    {
        Broadcast = 0x400,
        ControlDataTruncated = 0x200,
        DontRoute = 4,
        MaxIOVectorLength = 0x10,
        Multicast = 0x800,
        None = 0,
        OutOfBand = 1,
        Partial = 0x8000,
        Peek = 2,
        Truncated = 0x100
    }
}

