namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    internal class ContextChannelRequestContext : RequestContext
    {
        private ContextProtocol contextProtocol;
        private TimeSpan defaultSendTimeout;
        private RequestContext innerContext;

        public ContextChannelRequestContext(RequestContext innerContext, ContextProtocol contextProtocol, TimeSpan defaultSendTimeout)
        {
            if (innerContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("innerContext");
            }
            if (contextProtocol == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contextProtocol");
            }
            this.innerContext = innerContext;
            this.contextProtocol = contextProtocol;
            this.defaultSendTimeout = defaultSendTimeout;
        }

        public override void Abort()
        {
            this.innerContext.Abort();
        }

        public override IAsyncResult BeginReply(Message message, AsyncCallback callback, object state)
        {
            return this.BeginReply(message, this.defaultSendTimeout, callback, state);
        }

        public override IAsyncResult BeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ReplyAsyncResult(message, this, timeout, callback, state);
        }

        public override void Close()
        {
            this.innerContext.Close();
        }

        public override void Close(TimeSpan timeout)
        {
            this.innerContext.Close(timeout);
        }

        public override void EndReply(IAsyncResult result)
        {
            ReplyAsyncResult.End(result);
        }

        public override void Reply(Message message)
        {
            this.Reply(message, this.defaultSendTimeout);
        }

        public override void Reply(Message message, TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            Message message2 = message;
            if (message != null)
            {
                CorrelationCallbackMessageProperty property;
                this.contextProtocol.OnOutgoingMessage(message, this);
                if (CorrelationCallbackMessageProperty.TryGet(message, out property))
                {
                    ContextExchangeCorrelationHelper.AddOutgoingCorrelationCallbackData(property, message, false);
                    if (property.IsFullyDefined)
                    {
                        message2 = property.FinalizeCorrelation(message, helper.RemainingTime());
                        message2.Properties.Remove(CorrelationCallbackMessageProperty.Name);
                    }
                }
            }
            try
            {
                this.innerContext.Reply(message2, helper.RemainingTime());
            }
            finally
            {
                if ((message != null) && !object.ReferenceEquals(message, message2))
                {
                    message2.Close();
                }
            }
        }

        public override Message RequestMessage
        {
            get
            {
                return this.innerContext.RequestMessage;
            }
        }

        private class ReplyAsyncResult : AsyncResult
        {
            private ContextChannelRequestContext context;
            private CorrelationCallbackMessageProperty correlationCallback;
            private Message message;
            private static AsyncCallback onFinalizeCorrelation = Fx.ThunkCallback(new AsyncCallback(ContextChannelRequestContext.ReplyAsyncResult.OnFinalizeCorrelationCompletedCallback));
            private static AsyncCallback onReply = Fx.ThunkCallback(new AsyncCallback(ContextChannelRequestContext.ReplyAsyncResult.OnReplyCompletedCallback));
            private Message replyMessage;
            private TimeoutHelper timeoutHelper;

            public ReplyAsyncResult(Message message, ContextChannelRequestContext context, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.context = context;
                this.message = this.replyMessage = message;
                this.timeoutHelper = new TimeoutHelper(timeout);
                bool flag = true;
                if (message != null)
                {
                    this.context.contextProtocol.OnOutgoingMessage(message, this.context);
                    if (CorrelationCallbackMessageProperty.TryGet(message, out this.correlationCallback))
                    {
                        ContextExchangeCorrelationHelper.AddOutgoingCorrelationCallbackData(this.correlationCallback, message, false);
                        if (this.correlationCallback.IsFullyDefined)
                        {
                            IAsyncResult result = this.correlationCallback.BeginFinalizeCorrelation(this.message, this.timeoutHelper.RemainingTime(), onFinalizeCorrelation, this);
                            if (result.CompletedSynchronously && this.OnFinalizeCorrelationCompleted(result))
                            {
                                base.Complete(true);
                            }
                            flag = false;
                        }
                    }
                }
                if (flag)
                {
                    IAsyncResult result2 = this.context.innerContext.BeginReply(this.message, this.timeoutHelper.RemainingTime(), onReply, this);
                    if (result2.CompletedSynchronously)
                    {
                        this.OnReplyCompleted(result2);
                        base.Complete(true);
                    }
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<ContextChannelRequestContext.ReplyAsyncResult>(result);
            }

            private bool OnFinalizeCorrelationCompleted(IAsyncResult result)
            {
                IAsyncResult result2;
                this.replyMessage = this.correlationCallback.EndFinalizeCorrelation(result);
                bool flag = true;
                try
                {
                    result2 = this.context.innerContext.BeginReply(this.replyMessage, this.timeoutHelper.RemainingTime(), onReply, this);
                    flag = false;
                }
                finally
                {
                    if ((flag && (this.message != null)) && !object.ReferenceEquals(this.message, this.replyMessage))
                    {
                        this.replyMessage.Close();
                    }
                }
                if (result2.CompletedSynchronously)
                {
                    this.OnReplyCompleted(result2);
                    return true;
                }
                return false;
            }

            private static void OnFinalizeCorrelationCompletedCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    bool flag;
                    ContextChannelRequestContext.ReplyAsyncResult asyncState = (ContextChannelRequestContext.ReplyAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        flag = asyncState.OnFinalizeCorrelationCompleted(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                        flag = true;
                    }
                    if (flag)
                    {
                        asyncState.Complete(false, exception);
                    }
                }
            }

            private void OnReplyCompleted(IAsyncResult result)
            {
                try
                {
                    this.context.innerContext.EndReply(result);
                }
                finally
                {
                    if ((this.message != null) && !object.ReferenceEquals(this.message, this.replyMessage))
                    {
                        this.replyMessage.Close();
                    }
                }
            }

            private static void OnReplyCompletedCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ContextChannelRequestContext.ReplyAsyncResult asyncState = (ContextChannelRequestContext.ReplyAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        asyncState.OnReplyCompleted(result);
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

