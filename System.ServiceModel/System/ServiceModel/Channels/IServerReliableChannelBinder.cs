namespace System.ServiceModel.Channels
{
    using System;

    internal interface IServerReliableChannelBinder : IReliableChannelBinder
    {
        bool AddressResponse(Message request, Message response);
        IAsyncResult BeginWaitForRequest(TimeSpan timeout, AsyncCallback callback, object state);
        bool EndWaitForRequest(IAsyncResult result);
        bool UseNewChannel(IChannel channel);
        bool WaitForRequest(TimeSpan timeout);
    }
}

