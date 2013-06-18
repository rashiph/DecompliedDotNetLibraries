namespace System.ServiceModel.Channels
{
    using System;

    internal interface IConnectionListener : IDisposable
    {
        IAsyncResult BeginAccept(AsyncCallback callback, object state);
        IConnection EndAccept(IAsyncResult result);
        void Listen();
    }
}

