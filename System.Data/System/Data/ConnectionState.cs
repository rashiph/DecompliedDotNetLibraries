namespace System.Data
{
    using System;

    [Flags]
    public enum ConnectionState
    {
        Broken = 0x10,
        Closed = 0,
        Connecting = 2,
        Executing = 4,
        Fetching = 8,
        Open = 1
    }
}

