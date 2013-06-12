namespace System.Net.Sockets
{
    using System;

    public enum IOControlCode : long
    {
        AbsorbRouterAlert = 0x98000005L,
        AddMulticastGroupOnInterface = 0x9800000aL,
        AddressListChange = 0x28000017L,
        AddressListQuery = 0x48000016L,
        AddressListSort = 0xc8000019L,
        AssociateHandle = 0x88000001L,
        AsyncIO = 0x8004667dL,
        BindToInterface = 0x98000008L,
        DataToRead = 0x4004667fL,
        DeleteMulticastGroupFromInterface = 0x9800000bL,
        EnableCircularQueuing = 0x28000002L,
        Flush = 0x28000004L,
        GetBroadcastAddress = 0x48000005L,
        GetExtensionFunctionPointer = 0xc8000006L,
        GetGroupQos = 0xc8000008L,
        GetQos = 0xc8000007L,
        KeepAliveValues = 0x98000004L,
        LimitBroadcasts = 0x98000007L,
        MulticastInterface = 0x98000009L,
        MulticastScope = 0x8800000aL,
        MultipointLoopback = 0x88000009L,
        NamespaceChange = 0x88000019L,
        NonBlockingIO = 0x8004667eL,
        OobDataRead = 0x40047307L,
        QueryTargetPnpHandle = 0x48000018L,
        ReceiveAll = 0x98000001L,
        ReceiveAllIgmpMulticast = 0x98000003L,
        ReceiveAllMulticast = 0x98000002L,
        RoutingInterfaceChange = 0x88000015L,
        RoutingInterfaceQuery = 0xc8000014L,
        SetGroupQos = 0x8800000cL,
        SetQos = 0x8800000bL,
        TranslateHandle = 0xc800000dL,
        UnicastInterface = 0x98000006L
    }
}

