namespace System.ServiceModel.Channels
{
    using System;

    internal class PeerMaintainer : PeerMaintainerBase<ConnectAlgorithms>
    {
        public PeerMaintainer(PeerNodeConfig config, PeerNeighborManager neighborManager, PeerFlooder flooder) : base(config, neighborManager, flooder)
        {
        }
    }
}

