namespace System.ServiceModel.Channels
{
    using System;

    internal interface IClientReliableChannelBinder : IReliableChannelBinder
    {
        event EventHandler ConnectionLost;

        IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state);
        IAsyncResult BeginRequest(Message message, TimeSpan timeout, MaskingMode maskingMode, AsyncCallback callback, object state);
        Message EndRequest(IAsyncResult result);
        bool EnsureChannelForRequest();
        Message Request(Message message, TimeSpan timeout);
        Message Request(Message message, TimeSpan timeout, MaskingMode maskingMode);

        Uri Via { get; }
    }
}

