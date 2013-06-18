namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;

    internal delegate SocketHandler SocketHandlerFactory(Socket socket, SocketCache socketCache, string machineAndPort);
}

