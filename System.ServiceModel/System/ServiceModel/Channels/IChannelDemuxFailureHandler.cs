namespace System.ServiceModel.Channels
{
    using System;

    internal interface IChannelDemuxFailureHandler
    {
        IAsyncResult BeginHandleDemuxFailure(Message message, IOutputChannel faultContext, AsyncCallback callback, object state);
        IAsyncResult BeginHandleDemuxFailure(Message message, RequestContext faultContext, AsyncCallback callback, object state);
        void EndHandleDemuxFailure(IAsyncResult result);
        void HandleDemuxFailure(Message message);
    }
}

