namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    internal abstract class ContextRequestChannelBase<TChannel> : LayeredChannel<TChannel> where TChannel: class, IRequestChannel
    {
        private ContextProtocol contextProtocol;

        protected ContextRequestChannelBase(ChannelManagerBase channelManager, TChannel innerChannel, ContextExchangeMechanism contextExchangeMechanism, Uri callbackAddress, bool contextManagementEnabled) : base(channelManager, innerChannel)
        {
            this.contextProtocol = new ClientContextProtocol(contextExchangeMechanism, innerChannel.Via, this, callbackAddress, contextManagementEnabled);
        }

        public IAsyncResult BeginRequest(Message message, AsyncCallback callback, object state)
        {
            return this.BeginRequest(message, base.DefaultSendTimeout, callback, state);
        }

        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.contextProtocol.OnOutgoingMessage(message, null);
            return new RequestAsyncResult<TChannel>(message, base.InnerChannel, timeout, callback, state);
        }

        public Message EndRequest(IAsyncResult result)
        {
            Message message = RequestAsyncResult<TChannel>.End(result);
            if (message != null)
            {
                this.contextProtocol.OnIncomingMessage(message);
            }
            return message;
        }

        public override T GetProperty<T>() where T: class
        {
            if ((typeof(T) == typeof(IContextManager)) && (this.contextProtocol is IContextManager))
            {
                return (T) this.contextProtocol;
            }
            return base.GetProperty<T>();
        }

        public Message Request(Message message)
        {
            return this.Request(message, base.DefaultSendTimeout);
        }

        public Message Request(Message message, TimeSpan timeout)
        {
            CorrelationCallbackMessageProperty property = null;
            TimeoutHelper helper = new TimeoutHelper(timeout);
            Message message2 = message;
            this.contextProtocol.OnOutgoingMessage(message, null);
            if ((message != null) && CorrelationCallbackMessageProperty.TryGet(message, out property))
            {
                ContextExchangeCorrelationHelper.AddOutgoingCorrelationCallbackData(property, message, true);
                if (property.IsFullyDefined)
                {
                    message2 = property.FinalizeCorrelation(message, helper.RemainingTime());
                }
            }
            Message message3 = null;
            try
            {
                message3 = base.InnerChannel.Request(message2, timeout);
                if (message3 != null)
                {
                    this.contextProtocol.OnIncomingMessage(message3);
                }
            }
            finally
            {
                if ((message != null) && !object.ReferenceEquals(message, message2))
                {
                    message2.Close();
                }
            }
            return message3;
        }

        public EndpointAddress RemoteAddress
        {
            get
            {
                return base.InnerChannel.RemoteAddress;
            }
        }

        public Uri Via
        {
            get
            {
                return base.InnerChannel.Via;
            }
        }

        private class RequestAsyncResult : AsyncResult
        {
            private IRequestChannel channel;
            private CorrelationCallbackMessageProperty correlationCallback;
            private Message message;
            private static AsyncCallback onFinalizeCorrelation;
            private static AsyncCallback onRequest;
            private Message replyMessage;
            private Message requestMessage;
            private TimeoutHelper timeoutHelper;

            static RequestAsyncResult()
            {
                ContextRequestChannelBase<TChannel>.RequestAsyncResult.onFinalizeCorrelation = Fx.ThunkCallback(new AsyncCallback(ContextRequestChannelBase<TChannel>.RequestAsyncResult.OnFinalizeCorrelationCompletedCallback));
                ContextRequestChannelBase<TChannel>.RequestAsyncResult.onRequest = Fx.ThunkCallback(new AsyncCallback(ContextRequestChannelBase<TChannel>.RequestAsyncResult.OnRequestCompletedCallback));
            }

            public RequestAsyncResult(Message message, IRequestChannel channel, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.channel = channel;
                this.message = this.requestMessage = message;
                this.timeoutHelper = new TimeoutHelper(timeout);
                bool flag = true;
                if ((message != null) && CorrelationCallbackMessageProperty.TryGet(message, out this.correlationCallback))
                {
                    ContextExchangeCorrelationHelper.AddOutgoingCorrelationCallbackData(this.correlationCallback, message, true);
                    if (this.correlationCallback.IsFullyDefined)
                    {
                        IAsyncResult result = this.correlationCallback.BeginFinalizeCorrelation(this.message, this.timeoutHelper.RemainingTime(), ContextRequestChannelBase<TChannel>.RequestAsyncResult.onFinalizeCorrelation, this);
                        if (result.CompletedSynchronously && this.OnFinalizeCorrelationCompleted(result))
                        {
                            base.Complete(true);
                        }
                        flag = false;
                    }
                }
                if (flag)
                {
                    IAsyncResult result2 = this.channel.BeginRequest(this.message, this.timeoutHelper.RemainingTime(), ContextRequestChannelBase<TChannel>.RequestAsyncResult.onRequest, this);
                    if (result2.CompletedSynchronously)
                    {
                        this.OnRequestCompleted(result2);
                        base.Complete(true);
                    }
                }
            }

            public static Message End(IAsyncResult result)
            {
                return AsyncResult.End<ContextRequestChannelBase<TChannel>.RequestAsyncResult>(result).replyMessage;
            }

            private bool OnFinalizeCorrelationCompleted(IAsyncResult result)
            {
                IAsyncResult result2;
                this.requestMessage = this.correlationCallback.EndFinalizeCorrelation(result);
                this.requestMessage.Properties.Remove(CorrelationCallbackMessageProperty.Name);
                bool flag = true;
                try
                {
                    result2 = this.channel.BeginRequest(this.requestMessage, this.timeoutHelper.RemainingTime(), ContextRequestChannelBase<TChannel>.RequestAsyncResult.onRequest, this);
                    flag = false;
                }
                finally
                {
                    if ((flag && (this.message != null)) && !object.ReferenceEquals(this.message, this.requestMessage))
                    {
                        this.requestMessage.Close();
                    }
                }
                if (result2.CompletedSynchronously)
                {
                    this.OnRequestCompleted(result2);
                    return true;
                }
                return false;
            }

            private static void OnFinalizeCorrelationCompletedCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    bool flag;
                    ContextRequestChannelBase<TChannel>.RequestAsyncResult asyncState = (ContextRequestChannelBase<TChannel>.RequestAsyncResult) result.AsyncState;
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

            private void OnRequestCompleted(IAsyncResult result)
            {
                try
                {
                    this.replyMessage = this.channel.EndRequest(result);
                }
                finally
                {
                    if ((this.message != null) && !object.ReferenceEquals(this.message, this.requestMessage))
                    {
                        this.requestMessage.Close();
                    }
                }
            }

            private static void OnRequestCompletedCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ContextRequestChannelBase<TChannel>.RequestAsyncResult asyncState = (ContextRequestChannelBase<TChannel>.RequestAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        asyncState.OnRequestCompleted(result);
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

