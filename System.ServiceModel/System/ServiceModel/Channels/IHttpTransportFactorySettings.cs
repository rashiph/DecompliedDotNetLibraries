namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal interface IHttpTransportFactorySettings : ITransportFactorySettings, IDefaultCommunicationTimeouts
    {
        int MaxBufferSize { get; }

        System.ServiceModel.TransferMode TransferMode { get; }
    }
}

