namespace System.Net
{
    using System;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct AddressInfo
    {
        internal AddressInfoHints ai_flags;
        internal AddressFamily ai_family;
        internal SocketType ai_socktype;
        internal ProtocolFamily ai_protocol;
        internal int ai_addrlen;
        internal unsafe sbyte* ai_canonname;
        internal unsafe byte* ai_addr;
        internal unsafe AddressInfo* ai_next;
    }
}

