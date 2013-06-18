namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    public interface IReplyChannel : IChannel, ICommunicationObject
    {
        IAsyncResult BeginReceiveRequest(AsyncCallback callback, object state);
        IAsyncResult BeginReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state);
        IAsyncResult BeginTryReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state);
        IAsyncResult BeginWaitForRequest(TimeSpan timeout, AsyncCallback callback, object state);
        RequestContext EndReceiveRequest(IAsyncResult result);
        bool EndTryReceiveRequest(IAsyncResult result, out RequestContext context);
        bool EndWaitForRequest(IAsyncResult result);
        RequestContext ReceiveRequest();
        RequestContext ReceiveRequest(TimeSpan timeout);
        bool TryReceiveRequest(TimeSpan timeout, out RequestContext context);
        bool WaitForRequest(TimeSpan timeout);

        EndpointAddress LocalAddress { get; }
    }
}

