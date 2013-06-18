namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;

    internal interface IListenerBinder
    {
        IChannelBinder Accept(TimeSpan timeout);
        IAsyncResult BeginAccept(TimeSpan timeout, AsyncCallback callback, object state);
        IChannelBinder EndAccept(IAsyncResult result);

        IChannelListener Listener { get; }

        System.ServiceModel.Channels.MessageVersion MessageVersion { get; }
    }
}

