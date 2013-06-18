namespace System.ServiceModel
{
    using System;
    using System.ServiceModel.Channels;

    public interface IDuplexContextChannel : IContextChannel, IChannel, ICommunicationObject, IExtensibleObject<IContextChannel>
    {
        IAsyncResult BeginCloseOutputSession(TimeSpan timeout, AsyncCallback callback, object state);
        void CloseOutputSession(TimeSpan timeout);
        void EndCloseOutputSession(IAsyncResult result);

        bool AutomaticInputSessionShutdown { get; set; }

        InstanceContext CallbackInstance { get; set; }
    }
}

