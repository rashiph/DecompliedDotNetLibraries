namespace System.Runtime.Remoting.Channels.Tcp
{
    using System;

    internal static class TcpContentDelimiter
    {
        internal const ushort Chunked = 1;
        internal const ushort ContentLength = 0;
    }
}

