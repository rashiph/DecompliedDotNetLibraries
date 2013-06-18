namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;

    internal class ReplySessionDemuxFailureAsyncResult : ReplyChannelDemuxFailureAsyncResult
    {
        private IReplySessionChannel channel;
        private static AsyncCallback closeChannelCallback = Fx.ThunkCallback(new AsyncCallback(ReplySessionDemuxFailureAsyncResult.ChannelCloseCallback));

        public ReplySessionDemuxFailureAsyncResult(IChannelDemuxFailureHandler demuxFailureHandler, RequestContext requestContext, IReplySessionChannel channel, AsyncCallback callback, object state) : base(demuxFailureHandler, requestContext, callback, state)
        {
            this.channel = channel;
        }

        private static void ChannelCloseCallback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ReplySessionDemuxFailureAsyncResult asyncState = (ReplySessionDemuxFailureAsyncResult) result.AsyncState;
                Exception exception = null;
                try
                {
                    asyncState.channel.EndClose(result);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    exception = exception2;
                }
                asyncState.Complete(false, exception);
            }
        }

        public static void End(IAsyncResult result)
        {
            AsyncResult.End<ReplySessionDemuxFailureAsyncResult>(result);
        }

        protected override bool OnDemuxFailureHandled()
        {
            base.OnDemuxFailureHandled();
            IAsyncResult result = this.channel.BeginClose(closeChannelCallback, this);
            if (!result.CompletedSynchronously)
            {
                return false;
            }
            this.channel.EndClose(result);
            return true;
        }
    }
}

