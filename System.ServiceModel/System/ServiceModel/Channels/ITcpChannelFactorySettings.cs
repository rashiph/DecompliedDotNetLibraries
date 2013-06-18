namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal interface ITcpChannelFactorySettings : IConnectionOrientedTransportChannelFactorySettings, IConnectionOrientedTransportFactorySettings, ITransportFactorySettings, IDefaultCommunicationTimeouts, IConnectionOrientedConnectionSettings
    {
        TimeSpan LeaseTimeout { get; }
    }
}

