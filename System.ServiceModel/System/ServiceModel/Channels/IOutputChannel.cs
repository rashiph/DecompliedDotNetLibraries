namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    public interface IOutputChannel : IChannel, ICommunicationObject
    {
        IAsyncResult BeginSend(Message message, AsyncCallback callback, object state);
        IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state);
        void EndSend(IAsyncResult result);
        void Send(Message message);
        void Send(Message message, TimeSpan timeout);

        EndpointAddress RemoteAddress { get; }

        Uri Via { get; }
    }
}

