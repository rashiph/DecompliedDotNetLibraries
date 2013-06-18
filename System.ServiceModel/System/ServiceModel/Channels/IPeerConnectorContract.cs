namespace System.ServiceModel.Channels
{
    using System;

    internal interface IPeerConnectorContract
    {
        void Connect(IPeerNeighbor neighbor, ConnectInfo connectInfo);
        void Disconnect(IPeerNeighbor neighbor, DisconnectInfo disconnectInfo);
        void Refuse(IPeerNeighbor neighbor, RefuseInfo refuseInfo);
        void Welcome(IPeerNeighbor neighbor, WelcomeInfo welcomeInfo);
    }
}

