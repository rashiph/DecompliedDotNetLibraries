namespace System.ServiceModel.Channels
{
    using System;

    internal class PeerFlooder : PeerFlooderSimple
    {
        private PeerFlooder(PeerNodeConfig config, PeerNeighborManager neighborManager) : base(config, neighborManager)
        {
        }

        public static PeerFlooder CreateFlooder(PeerNodeConfig config, PeerNeighborManager neighborManager, IPeerNodeMessageHandling messageHandler)
        {
            return new PeerFlooder(config, neighborManager) { messageHandler = messageHandler };
        }
    }
}

