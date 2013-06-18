namespace System.Runtime.Remoting.Channels.Tcp
{
    using System;

    internal static class TcpHeaderFormat
    {
        internal const byte Byte = 2;
        internal const byte CountedString = 1;
        internal const byte Int32 = 4;
        internal const byte UInt16 = 3;
        internal const byte Void = 0;
    }
}

