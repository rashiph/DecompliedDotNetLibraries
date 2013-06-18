namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal interface IPeerMaintainer
    {
        event MaintainerClosedHandler MaintainerClosed;

        event NeighborClosedHandler NeighborClosed;

        event NeighborConnectedHandler NeighborConnected;

        event ReferralsAddedHandler ReferralsAdded;

        IAsyncResult BeginOpenNeighbor(PeerNodeAddress to, TimeSpan timeout, AsyncCallback callback, object asyncState);
        void CloseNeighbor(IPeerNeighbor neighbor, PeerCloseReason closeReason);
        IPeerNeighbor EndOpenNeighbor(IAsyncResult result);
        IPeerNeighbor FindDuplicateNeighbor(PeerNodeAddress address);
        IPeerNeighbor GetLeastUsefulNeighbor();
        PeerNodeAddress GetListenAddress();

        int ConnectedNeighborCount { get; }

        bool IsOpen { get; }

        int NonClosingNeighborCount { get; }
    }
}

