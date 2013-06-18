namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel;

    public interface IInteractiveChannelInitializer
    {
        IAsyncResult BeginDisplayInitializationUI(IClientChannel channel, AsyncCallback callback, object state);
        void EndDisplayInitializationUI(IAsyncResult result);
    }
}

