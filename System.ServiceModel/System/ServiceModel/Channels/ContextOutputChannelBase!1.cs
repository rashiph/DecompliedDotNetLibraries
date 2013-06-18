namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    internal abstract class ContextOutputChannelBase<TChannel> : LayeredChannel<TChannel> where TChannel: class, IOutputChannel
    {
        protected ContextOutputChannelBase(ChannelManagerBase channelManager, TChannel innerChannel) : base(channelManager, innerChannel)
        {
        }

        public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
        {
            return this.BeginSend(message, base.DefaultSendTimeout, callback, state);
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new SendAsyncResult<TChannel>(message, (ContextOutputChannelBase<TChannel>) this, this.ContextProtocol, timeout, callback, state);
        }

        public void EndSend(IAsyncResult result)
        {
            SendAsyncResult<TChannel>.End(result);
        }

        public override T GetProperty<T>() where T: class
        {
            if (typeof(T) == typeof(IContextManager))
            {
                return (T) this.ContextProtocol;
            }
            return base.GetProperty<T>();
        }

        public void Send(Message message)
        {
            this.Send(message, base.DefaultSendTimeout);
        }

        public void Send(Message message, TimeSpan timeout)
        {
            CorrelationCallbackMessageProperty property = null;
            TimeoutHelper helper = new TimeoutHelper(timeout);
            Message message2 = message;
            if (message != null)
            {
                this.ContextProtocol.OnOutgoingMessage(message, null);
                if (CorrelationCallbackMessageProperty.TryGet(message, out property))
                {
                    ContextExchangeCorrelationHelper.AddOutgoingCorrelationCallbackData(property, message, this.IsClient);
                    if (property.IsFullyDefined)
                    {
                        message2 = property.FinalizeCorrelation(message, helper.RemainingTime());
                    }
                }
            }
            try
            {
                base.InnerChannel.Send(message2, helper.RemainingTime());
            }
            finally
            {
                if ((message != null) && !object.ReferenceEquals(message, message2))
                {
                    message2.Close();
                }
            }
        }

        protected abstract System.ServiceModel.Channels.ContextProtocol ContextProtocol { get; }

        protected abstract bool IsClient { get; }

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

        private class SendAsyncResult : AsyncResult
        {
            private ContextOutputChannelBase<TChannel> channel;
            private CorrelationCallbackMessageProperty correlationCallback;
            private Message message;
            private static AsyncCallback onFinalizeCorrelation;
            private static AsyncCallback onSend;
            private Message sendMessage;
            private TimeoutHelper timeoutHelper;

            static SendAsyncResult()
            {
                ContextOutputChannelBase<TChannel>.SendAsyncResult.onFinalizeCorrelation = Fx.ThunkCallback(new AsyncCallback(ContextOutputChannelBase<TChannel>.SendAsyncResult.OnFinalizeCorrelationCompletedCallback));
                ContextOutputChannelBase<TChannel>.SendAsyncResult.onSend = Fx.ThunkCallback(new AsyncCallback(ContextOutputChannelBase<TChannel>.SendAsyncResult.OnSendCompletedCallback));
            }

            public SendAsyncResult(Message message, ContextOutputChannelBase<TChannel> channel, ContextProtocol contextProtocol, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.channel = channel;
                this.message = this.sendMessage = message;
                this.timeoutHelper = new TimeoutHelper(timeout);
                bool flag = true;
                if (message != null)
                {
                    contextProtocol.OnOutgoingMessage(message, null);
                    if (CorrelationCallbackMessageProperty.TryGet(message, out this.correlationCallback))
                    {
                        ContextExchangeCorrelationHelper.AddOutgoingCorrelationCallbackData(this.correlationCallback, message, this.channel.IsClient);
                        if (this.correlationCallback.IsFullyDefined)
                        {
                            IAsyncResult result = this.correlationCallback.BeginFinalizeCorrelation(this.message, this.timeoutHelper.RemainingTime(), ContextOutputChannelBase<TChannel>.SendAsyncResult.onFinalizeCorrelation, this);
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
                    IAsyncResult result2 = this.channel.InnerChannel.BeginSend(this.message, this.timeoutHelper.RemainingTime(), ContextOutputChannelBase<TChannel>.SendAsyncResult.onSend, this);
                    if (result2.CompletedSynchronously)
                    {
                        this.OnSendCompleted(result2);
                        base.Complete(true);
                    }
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<ContextOutputChannelBase<TChannel>.SendAsyncResult>(result);
            }

            private bool OnFinalizeCorrelationCompleted(IAsyncResult result)
            {
                IAsyncResult result2;
                this.sendMessage = this.correlationCallback.EndFinalizeCorrelation(result);
                bool flag = true;
                try
                {
                    result2 = this.channel.InnerChannel.BeginSend(this.sendMessage, this.timeoutHelper.RemainingTime(), ContextOutputChannelBase<TChannel>.SendAsyncResult.onSend, this);
                    flag = false;
                }
                finally
                {
                    if ((flag && (this.message != null)) && !object.ReferenceEquals(this.message, this.sendMessage))
                    {
                        this.sendMessage.Close();
                    }
                }
                if (result2.CompletedSynchronously)
                {
                    this.OnSendCompleted(result2);
                    return true;
                }
                return false;
            }

            private static void OnFinalizeCorrelationCompletedCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    bool flag;
                    ContextOutputChannelBase<TChannel>.SendAsyncResult asyncState = (ContextOutputChannelBase<TChannel>.SendAsyncResult) result.AsyncState;
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

            private void OnSendCompleted(IAsyncResult result)
            {
                try
                {
                    this.channel.InnerChannel.EndSend(result);
                }
                finally
                {
                    if ((this.message != null) && !object.ReferenceEquals(this.message, this.sendMessage))
                    {
                        this.sendMessage.Close();
                    }
                }
            }

            private static void OnSendCompletedCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ContextOutputChannelBase<TChannel>.SendAsyncResult asyncState = (ContextOutputChannelBase<TChannel>.SendAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        asyncState.OnSendCompleted(result);
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

