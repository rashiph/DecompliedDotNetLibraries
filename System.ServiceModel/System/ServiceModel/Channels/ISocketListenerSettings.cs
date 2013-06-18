namespace System.ServiceModel.Channels
{
    using System;

    internal interface ISocketListenerSettings
    {
        int BufferSize { get; }

        int ListenBacklog { get; }

        bool TeredoEnabled { get; }
    }
}

