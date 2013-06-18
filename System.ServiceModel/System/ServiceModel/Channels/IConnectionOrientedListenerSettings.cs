namespace System.ServiceModel.Channels
{
    using System;

    internal interface IConnectionOrientedListenerSettings : IConnectionOrientedConnectionSettings
    {
        TimeSpan ChannelInitializationTimeout { get; }

        int MaxPendingAccepts { get; }

        int MaxPendingConnections { get; }

        int MaxPooledConnections { get; }
    }
}

