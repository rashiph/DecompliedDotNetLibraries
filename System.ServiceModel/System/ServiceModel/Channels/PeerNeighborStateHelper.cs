namespace System.ServiceModel.Channels
{
    using System;

    internal static class PeerNeighborStateHelper
    {
        public static bool IsAuthenticatedOrClosed(PeerNeighborState state)
        {
            if ((state != PeerNeighborState.Authenticated) && (state != PeerNeighborState.Faulted))
            {
                return (state == PeerNeighborState.Closed);
            }
            return true;
        }

        public static bool IsConnected(PeerNeighborState state)
        {
            return (state == PeerNeighborState.Connected);
        }

        public static bool IsSettable(PeerNeighborState state)
        {
            if (((state != PeerNeighborState.Authenticated) && (state != PeerNeighborState.Connecting)) && ((state != PeerNeighborState.Connected) && (state != PeerNeighborState.Disconnecting)))
            {
                return (state == PeerNeighborState.Disconnected);
            }
            return true;
        }
    }
}

