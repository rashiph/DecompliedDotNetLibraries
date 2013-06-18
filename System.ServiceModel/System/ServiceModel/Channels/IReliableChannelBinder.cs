namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal interface IReliableChannelBinder
    {
        event BinderExceptionHandler Faulted;

        event BinderExceptionHandler OnException;

        void Abort();
        IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state);
        IAsyncResult BeginClose(TimeSpan timeout, MaskingMode maskingMode, AsyncCallback callback, object state);
        IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state);
        IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state);
        IAsyncResult BeginSend(Message message, TimeSpan timeout, MaskingMode maskingMode, AsyncCallback callback, object state);
        IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state);
        IAsyncResult BeginTryReceive(TimeSpan timeout, MaskingMode maskingMode, AsyncCallback callback, object state);
        void Close(TimeSpan timeout);
        void Close(TimeSpan timeout, MaskingMode maskingMode);
        void EndClose(IAsyncResult result);
        void EndOpen(IAsyncResult result);
        void EndSend(IAsyncResult result);
        bool EndTryReceive(IAsyncResult result, out RequestContext requestContext);
        ISession GetInnerSession();
        void HandleException(Exception e);
        bool IsHandleable(Exception e);
        void Open(TimeSpan timeout);
        void Send(Message message, TimeSpan timeout);
        void Send(Message message, TimeSpan timeout, MaskingMode maskingMode);
        void SetMaskingMode(RequestContext context, MaskingMode maskingMode);
        bool TryReceive(TimeSpan timeout, out RequestContext requestContext);
        bool TryReceive(TimeSpan timeout, out RequestContext requestContext, MaskingMode maskingMode);
        RequestContext WrapRequestContext(RequestContext context);

        bool CanSendAsynchronously { get; }

        IChannel Channel { get; }

        bool Connected { get; }

        TimeSpan DefaultSendTimeout { get; }

        bool HasSession { get; }

        EndpointAddress LocalAddress { get; }

        EndpointAddress RemoteAddress { get; }

        CommunicationState State { get; }
    }
}

