namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    public interface IChannelListener<TChannel> : IChannelListener, ICommunicationObject where TChannel: class, IChannel
    {
        TChannel AcceptChannel();
        TChannel AcceptChannel(TimeSpan timeout);
        IAsyncResult BeginAcceptChannel(AsyncCallback callback, object state);
        IAsyncResult BeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state);
        TChannel EndAcceptChannel(IAsyncResult result);
    }
}

