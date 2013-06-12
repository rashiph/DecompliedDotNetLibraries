namespace System.Net.Sockets
{
    using System;

    [Flags]
    public enum TransmitFileOptions
    {
        Disconnect = 1,
        ReuseSocket = 2,
        UseDefaultWorkerThread = 0,
        UseKernelApc = 0x20,
        UseSystemThread = 0x10,
        WriteBehind = 4
    }
}

