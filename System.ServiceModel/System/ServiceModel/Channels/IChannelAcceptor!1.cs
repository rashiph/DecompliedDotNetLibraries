namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal interface IChannelAcceptor<TChannel> : ICommunicationObject where TChannel: class, IChannel
    {
        TChannel AcceptChannel(TimeSpan timeout);
        IAsyncResult BeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state);
        IAsyncResult BeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state);
        TChannel EndAcceptChannel(IAsyncResult result);
        bool EndWaitForChannel(IAsyncResult result);
        bool WaitForChannel(TimeSpan timeout);
    }
}

