namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal interface IConnectionOrientedTransportChannelFactorySettings : IConnectionOrientedTransportFactorySettings, ITransportFactorySettings, IDefaultCommunicationTimeouts, IConnectionOrientedConnectionSettings
    {
        string ConnectionPoolGroupName { get; }

        int MaxOutboundConnectionsPerEndpoint { get; }
    }
}

