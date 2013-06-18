namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    public interface IInputChannel : IChannel, ICommunicationObject
    {
        IAsyncResult BeginReceive(AsyncCallback callback, object state);
        IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state);
        IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state);
        IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state);
        Message EndReceive(IAsyncResult result);
        bool EndTryReceive(IAsyncResult result, out Message message);
        bool EndWaitForMessage(IAsyncResult result);
        Message Receive();
        Message Receive(TimeSpan timeout);
        bool TryReceive(TimeSpan timeout, out Message message);
        bool WaitForMessage(TimeSpan timeout);

        EndpointAddress LocalAddress { get; }
    }
}

