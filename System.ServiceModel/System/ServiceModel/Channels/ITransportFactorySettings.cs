namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal interface ITransportFactorySettings : IDefaultCommunicationTimeouts
    {
        System.ServiceModel.Channels.BufferManager BufferManager { get; }

        bool ManualAddressing { get; }

        long MaxReceivedMessageSize { get; }

        System.ServiceModel.Channels.MessageEncoderFactory MessageEncoderFactory { get; }

        System.ServiceModel.Channels.MessageVersion MessageVersion { get; }
    }
}

