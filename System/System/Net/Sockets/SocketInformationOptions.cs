namespace System.Net.Sockets
{
    using System;

    [Flags]
    public enum SocketInformationOptions
    {
        Connected = 2,
        Listening = 4,
        NonBlocking = 1,
        UseOnlyOverlappedIO = 8
    }
}

