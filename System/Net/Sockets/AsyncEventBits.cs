namespace System.Net.Sockets
{
    using System;

    [Flags]
    internal enum AsyncEventBits
    {
        FdAccept = 8,
        FdAddressListChange = 0x200,
        FdAllEvents = 0x3ff,
        FdClose = 0x20,
        FdConnect = 0x10,
        FdGroupQos = 0x80,
        FdNone = 0,
        FdOob = 4,
        FdQos = 0x40,
        FdRead = 1,
        FdRoutingInterfaceChange = 0x100,
        FdWrite = 2
    }
}

