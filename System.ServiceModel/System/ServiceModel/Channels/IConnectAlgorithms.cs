namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;

    internal interface IConnectAlgorithms : IDisposable
    {
        void Connect(TimeSpan timeout);
        void Initialize(IPeerMaintainer maintainer, PeerNodeConfig config, int wantedConnectedNeighbors, Dictionary<EndpointAddress, Referral> referralCache);
        void PruneConnections();
        void UpdateEndpointsCollection(ICollection<PeerNodeAddress> src);
    }
}

