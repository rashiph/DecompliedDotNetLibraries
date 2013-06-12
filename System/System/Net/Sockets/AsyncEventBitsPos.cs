namespace System.Net.Sockets
{
    using System;

    internal enum AsyncEventBitsPos
    {
        FdReadBit,
        FdWriteBit,
        FdOobBit,
        FdAcceptBit,
        FdConnectBit,
        FdCloseBit,
        FdQosBit,
        FdGroupQosBit,
        FdRoutingInterfaceChangeBit,
        FdAddressListChangeBit,
        FdMaxEvents
    }
}

