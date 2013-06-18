namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal interface IReliableFactorySettings
    {
        TimeSpan AcknowledgementInterval { get; }

        bool FlowControlEnabled { get; }

        TimeSpan InactivityTimeout { get; }

        int MaxPendingChannels { get; }

        int MaxRetryCount { get; }

        int MaxTransferWindowSize { get; }

        System.ServiceModel.Channels.MessageVersion MessageVersion { get; }

        bool Ordered { get; }

        System.ServiceModel.ReliableMessagingVersion ReliableMessagingVersion { get; }

        TimeSpan SendTimeout { get; }
    }
}

