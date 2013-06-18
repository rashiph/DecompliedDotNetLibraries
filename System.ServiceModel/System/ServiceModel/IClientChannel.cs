namespace System.ServiceModel
{
    using System;
    using System.ServiceModel.Channels;

    public interface IClientChannel : IContextChannel, IChannel, ICommunicationObject, IExtensibleObject<IContextChannel>, IDisposable
    {
        event EventHandler<UnknownMessageReceivedEventArgs> UnknownMessageReceived;

        IAsyncResult BeginDisplayInitializationUI(AsyncCallback callback, object state);
        void DisplayInitializationUI();
        void EndDisplayInitializationUI(IAsyncResult result);

        bool AllowInitializationUI { get; set; }

        bool DidInteractiveInitialization { get; }

        Uri Via { get; }
    }
}

