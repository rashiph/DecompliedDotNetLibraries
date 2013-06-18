namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal class ReplyChannel : InputQueueChannel<RequestContext>, IReplyChannel, IChannel, ICommunicationObject
    {
        private EndpointAddress localAddress;

        public ReplyChannel(ChannelManagerBase channelManager, EndpointAddress localAddress) : base(channelManager)
        {
            this.localAddress = localAddress;
        }

        public IAsyncResult BeginReceiveRequest(AsyncCallback callback, object state)
        {
            return this.BeginReceiveRequest(base.DefaultReceiveTimeout, callback, state);
        }

        public IAsyncResult BeginReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", timeout, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
            }
            base.ThrowPending();
            return HelpBeginReceiveRequest(this, timeout, callback, state);
        }

        public IAsyncResult BeginTryReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", timeout, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
            }
            base.ThrowPending();
            return base.BeginDequeue(timeout, callback, state);
        }

        public IAsyncResult BeginWaitForRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", timeout, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
            }
            base.ThrowPending();
            return base.BeginWaitForItem(timeout, callback, state);
        }

        private static Exception CreateReceiveRequestTimedOutException(IReplyChannel channel, TimeSpan timeout)
        {
            if (channel.LocalAddress != null)
            {
                return new TimeoutException(System.ServiceModel.SR.GetString("ReceiveRequestTimedOut", new object[] { channel.LocalAddress.Uri.AbsoluteUri, timeout }));
            }
            return new TimeoutException(System.ServiceModel.SR.GetString("ReceiveRequestTimedOutNoLocalAddress", new object[] { timeout }));
        }

        public RequestContext EndReceiveRequest(IAsyncResult result)
        {
            return HelpEndReceiveRequest(result);
        }

        public bool EndTryReceiveRequest(IAsyncResult result, out RequestContext context)
        {
            return base.EndDequeue(result, out context);
        }

        public bool EndWaitForRequest(IAsyncResult result)
        {
            return base.EndWaitForItem(result);
        }

        public override T GetProperty<T>() where T: class
        {
            if (typeof(T) == typeof(IReplyChannel))
            {
                return (T) this;
            }
            T property = base.GetProperty<T>();
            if (property != null)
            {
                return property;
            }
            return default(T);
        }

        internal static IAsyncResult HelpBeginReceiveRequest(IReplyChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new HelpReceiveRequestAsyncResult(channel, timeout, callback, state);
        }

        internal static RequestContext HelpEndReceiveRequest(IAsyncResult result)
        {
            return HelpReceiveRequestAsyncResult.End(result);
        }

        internal static RequestContext HelpReceiveRequest(IReplyChannel channel, TimeSpan timeout)
        {
            RequestContext context;
            if (!channel.TryReceiveRequest(timeout, out context))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateReceiveRequestTimedOutException(channel, timeout));
            }
            return context;
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
        }

        public RequestContext ReceiveRequest()
        {
            return this.ReceiveRequest(base.DefaultReceiveTimeout);
        }

        public RequestContext ReceiveRequest(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", timeout, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
            }
            base.ThrowPending();
            return HelpReceiveRequest(this, timeout);
        }

        public bool TryReceiveRequest(TimeSpan timeout, out RequestContext context)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", timeout, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
            }
            base.ThrowPending();
            return base.Dequeue(timeout, out context);
        }

        public bool WaitForRequest(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", timeout, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
            }
            base.ThrowPending();
            return base.WaitForItem(timeout);
        }

        public EndpointAddress LocalAddress
        {
            get
            {
                return this.localAddress;
            }
        }

        private class HelpReceiveRequestAsyncResult : AsyncResult
        {
            private IReplyChannel channel;
            private static AsyncCallback onReceiveRequest = Fx.ThunkCallback(new AsyncCallback(ReplyChannel.HelpReceiveRequestAsyncResult.OnReceiveRequest));
            private RequestContext requestContext;
            private TimeSpan timeout;

            public HelpReceiveRequestAsyncResult(IReplyChannel channel, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.channel = channel;
                this.timeout = timeout;
                IAsyncResult result = channel.BeginTryReceiveRequest(timeout, onReceiveRequest, this);
                if (result.CompletedSynchronously)
                {
                    this.HandleReceiveRequestComplete(result);
                    base.Complete(true);
                }
            }

            public static RequestContext End(IAsyncResult result)
            {
                return AsyncResult.End<ReplyChannel.HelpReceiveRequestAsyncResult>(result).requestContext;
            }

            private void HandleReceiveRequestComplete(IAsyncResult result)
            {
                if (!this.channel.EndTryReceiveRequest(result, out this.requestContext))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ReplyChannel.CreateReceiveRequestTimedOutException(this.channel, this.timeout));
                }
            }

            private static void OnReceiveRequest(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ReplyChannel.HelpReceiveRequestAsyncResult asyncState = (ReplyChannel.HelpReceiveRequestAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        asyncState.HandleReceiveRequestComplete(result);
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
        }
    }
}

