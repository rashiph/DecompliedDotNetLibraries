namespace System.Net.Sockets
{
    using System;

    public enum SocketOptionLevel
    {
        IP = 0,
        IPv6 = 0x29,
        Socket = 0xffff,
        Tcp = 6,
        Udp = 0x11
    }
}

