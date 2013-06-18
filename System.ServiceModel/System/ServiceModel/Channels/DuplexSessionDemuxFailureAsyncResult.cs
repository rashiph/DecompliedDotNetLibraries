namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    internal class DuplexSessionDemuxFailureAsyncResult : AsyncResult
    {
        private IDuplexSessionChannel channel;
        private static AsyncCallback channelCloseCallback = Fx.ThunkCallback(new AsyncCallback(DuplexSessionDemuxFailureAsyncResult.ChannelCloseCallback));
        private IChannelDemuxFailureHandler demuxFailureHandler;
        private static AsyncCallback demuxFailureHandlerCallback = Fx.ThunkCallback(new AsyncCallback(DuplexSessionDemuxFailureAsyncResult.DemuxFailureHandlerCallback));
        private Message message;

        public DuplexSessionDemuxFailureAsyncResult(IChannelDemuxFailureHandler demuxFailureHandler, IDuplexSessionChannel channel, Message message, AsyncCallback callback, object state) : base(callback, state)
        {
            if (demuxFailureHandler == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("demuxFailureHandler");
            }
            if (channel == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("channel");
            }
            this.demuxFailureHandler = demuxFailureHandler;
            this.channel = channel;
            this.message = message;
        }

        private static void ChannelCloseCallback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                DuplexSessionDemuxFailureAsyncResult asyncState = (DuplexSessionDemuxFailureAsyncResult) result.AsyncState;
                Exception exception = null;
                try
                {
                    asyncState.channel.EndClose(result);
                    asyncState.message.Close();
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

        private static void DemuxFailureHandlerCallback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                DuplexSessionDemuxFailureAsyncResult asyncState = (DuplexSessionDemuxFailureAsyncResult) result.AsyncState;
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
            AsyncResult.End<DuplexSessionDemuxFailureAsyncResult>(result);
        }

        private bool OnDemuxFailureHandled()
        {
            IAsyncResult result = this.channel.BeginClose(channelCloseCallback, this);
            if (!result.CompletedSynchronously)
            {
                return false;
            }
            this.channel.EndClose(result);
            this.message.Close();
            return true;
        }

        public void Start()
        {
            IAsyncResult result = this.demuxFailureHandler.BeginHandleDemuxFailure(this.message, this.channel, demuxFailureHandlerCallback, this);
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

