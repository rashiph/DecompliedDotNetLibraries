namespace System.Runtime.Remoting.Channels.Tcp
{
    using System;

    internal static class TcpHeaders
    {
        internal const ushort CloseConnection = 5;
        internal const ushort ContentType = 6;
        internal const ushort Custom = 1;
        internal const ushort EndOfHeaders = 0;
        internal const ushort RequestUri = 4;
        internal const ushort StatusCode = 2;
        internal const ushort StatusPhrase = 3;
    }
}

