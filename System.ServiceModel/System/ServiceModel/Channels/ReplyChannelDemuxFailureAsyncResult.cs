namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    internal class ReplyChannelDemuxFailureAsyncResult : AsyncResult
    {
        private IChannelDemuxFailureHandler demuxFailureHandler;
        private static AsyncCallback demuxFailureHandlerCallback = Fx.ThunkCallback(new AsyncCallback(ReplyChannelDemuxFailureAsyncResult.DemuxFailureHandlerCallback));
        private RequestContext requestContext;

        public ReplyChannelDemuxFailureAsyncResult(IChannelDemuxFailureHandler demuxFailureHandler, RequestContext requestContext, AsyncCallback callback, object state) : base(callback, state)
        {
            if (demuxFailureHandler == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("demuxFailureHandler");
            }
            if (requestContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("requestContext");
            }
            this.demuxFailureHandler = demuxFailureHandler;
            this.requestContext = requestContext;
        }

        private static void DemuxFailureHandlerCallback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ReplyChannelDemuxFailureAsyncResult asyncState = (ReplyChannelDemuxFailureAsyncResult) result.AsyncState;
                bool flag = false;
                Exception exception = null;
                try
                {
                    asyncState.demuxFailureHandler.EndHandleDemuxFailure(result);
                    flag = asyncState.OnDemuxFailureHandled();
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    flag = true;
                    exception = exception2;
                }
                if (flag)
                {
                    asyncState.Complete(false, exception);
                }
            }
        }

        public static void End(IAsyncResult result)
        {
            AsyncResult.End<ReplyChannelDemuxFailureAsyncResult>(result);
        }

        protected virtual bool OnDemuxFailureHandled()
        {
            this.requestContext.Close();
            return true;
        }

        public void Start()
        {
            IAsyncResult result = this.demuxFailureHandler.BeginHandleDemuxFailure(this.requestContext.RequestMessage, this.requestContext, demuxFailureHandlerCallback, this);
            if (result.CompletedSynchronously)
            {
                this.demuxFailureHandler.EndHandleDemuxFailure(result);
                if (this.OnDemuxFailureHandled())
                {
                    base.Complete(true);
                }
            }
        }
    }
}

