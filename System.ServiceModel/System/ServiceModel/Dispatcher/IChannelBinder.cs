namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal interface IChannelBinder
    {
        void Abort();
        IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state);
        IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state);
        IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state);
        IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state);
        void CloseAfterFault(TimeSpan timeout);
        Message EndRequest(IAsyncResult result);
        void EndSend(IAsyncResult result);
        bool EndTryReceive(IAsyncResult result, out RequestContext requestContext);
        bool EndWaitForMessage(IAsyncResult result);
        Message Request(Message message, TimeSpan timeout);
        void Send(Message message, TimeSpan timeout);
        bool TryReceive(TimeSpan timeout, out RequestContext requestContext);
        bool WaitForMessage(TimeSpan timeout);

        IChannel Channel { get; }

        bool HasSession { get; }

        Uri ListenUri { get; }

        EndpointAddress LocalAddress { get; }

        EndpointAddress RemoteAddress { get; }
    }
}

