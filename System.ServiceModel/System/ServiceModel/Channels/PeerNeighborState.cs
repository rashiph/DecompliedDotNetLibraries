namespace System.ServiceModel.Channels
{
    using System;

    internal enum PeerNeighborState
    {
        Created,
        Opened,
        Authenticated,
        Connecting,
        Connected,
        Disconnecting,
        Disconnected,
        Faulted,
        Closed
    }
}

