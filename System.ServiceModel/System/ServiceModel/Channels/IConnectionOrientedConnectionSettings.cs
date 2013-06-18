namespace System.ServiceModel.Channels
{
    using System;

    internal interface IConnectionOrientedConnectionSettings
    {
        int ConnectionBufferSize { get; }

        TimeSpan IdleTimeout { get; }

        TimeSpan MaxOutputDelay { get; }
    }
}

