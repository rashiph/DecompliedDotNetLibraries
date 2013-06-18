namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal class InputChannel : InputQueueChannel<Message>, IInputChannel, IChannel, ICommunicationObject
    {
        private EndpointAddress localAddress;

        public InputChannel(ChannelManagerBase channelManager, EndpointAddress localAddress) : base(channelManager)
        {
            this.localAddress = localAddress;
        }

        public virtual IAsyncResult BeginReceive(AsyncCallback callback, object state)
        {
            return this.BeginReceive(base.DefaultReceiveTimeout, callback, state);
        }

        public virtual IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", timeout, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
            }
            base.ThrowPending();
            return HelpBeginReceive(this, timeout, callback, state);
        }

        public virtual IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", timeout, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
            }
            base.ThrowPending();
            return base.BeginDequeue(timeout, callback, state);
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", timeout, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
            }
            base.ThrowPending();
            return base.BeginWaitForItem(timeout, callback, state);
        }

        private static Exception CreateReceiveTimedOutException(IInputChannel channel, TimeSpan timeout)
        {
            if (channel.LocalAddress != null)
            {
                return new TimeoutException(System.ServiceModel.SR.GetString("ReceiveTimedOut", new object[] { channel.LocalAddress.Uri.AbsoluteUri, timeout }));
            }
            return new TimeoutException(System.ServiceModel.SR.GetString("ReceiveTimedOutNoLocalAddress", new object[] { timeout }));
        }

        public Message EndReceive(IAsyncResult result)
        {
            return HelpEndReceive(result);
        }

        public virtual bool EndTryReceive(IAsyncResult result, out Message message)
        {
            return base.EndDequeue(result, out message);
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            return base.EndWaitForItem(result);
        }

        public override T GetProperty<T>() where T: class
        {
            if (typeof(T) == typeof(IInputChannel))
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

        internal static IAsyncResult HelpBeginReceive(IInputChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new HelpReceiveAsyncResult(channel, timeout, callback, state);
        }

        internal static Message HelpEndReceive(IAsyncResult result)
        {
            return HelpReceiveAsyncResult.End(result);
        }

        internal static Message HelpReceive(IInputChannel channel, TimeSpan timeout)
        {
            Message message;
            if (!channel.TryReceive(timeout, out message))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateReceiveTimedOutException(channel, timeout));
            }
            return message;
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

        public virtual Message Receive()
        {
            return this.Receive(base.DefaultReceiveTimeout);
        }

        public virtual Message Receive(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", timeout, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
            }
            base.ThrowPending();
            return HelpReceive(this, timeout);
        }

        public virtual bool TryReceive(TimeSpan timeout, out Message message)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", timeout, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
            }
            base.ThrowPending();
            return base.Dequeue(timeout, out message);
        }

        public bool WaitForMessage(TimeSpan timeout)
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

        private class HelpReceiveAsyncResult : AsyncResult
        {
            private IInputChannel channel;
            private Message message;
            private static AsyncCallback onReceive = Fx.ThunkCallback(new AsyncCallback(InputChannel.HelpReceiveAsyncResult.OnReceive));
            private TimeSpan timeout;

            public HelpReceiveAsyncResult(IInputChannel channel, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.channel = channel;
                this.timeout = timeout;
                IAsyncResult result = channel.BeginTryReceive(timeout, onReceive, this);
                if (result.CompletedSynchronously)
                {
                    this.HandleReceiveComplete(result);
                    base.Complete(true);
                }
            }

            public static Message End(IAsyncResult result)
            {
                return AsyncResult.End<InputChannel.HelpReceiveAsyncResult>(result).message;
            }

            private void HandleReceiveComplete(IAsyncResult result)
            {
                if (!this.channel.EndTryReceive(result, out this.message))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(InputChannel.CreateReceiveTimedOutException(this.channel, this.timeout));
                }
            }

            private static void OnReceive(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    InputChannel.HelpReceiveAsyncResult asyncState = (InputChannel.HelpReceiveAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        asyncState.HandleReceiveComplete(result);
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

