namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal abstract class DuplexChannel : InputQueueChannel<Message>, IDuplexChannel, IInputChannel, IOutputChannel, IChannel, ICommunicationObject
    {
        private EndpointAddress localAddress;

        protected DuplexChannel(ChannelManagerBase channelManager, EndpointAddress localAddress) : base(channelManager)
        {
            this.localAddress = localAddress;
        }

        protected virtual void AddHeadersTo(Message message)
        {
        }

        public IAsyncResult BeginReceive(AsyncCallback callback, object state)
        {
            return this.BeginReceive(base.DefaultReceiveTimeout, callback, state);
        }

        public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", timeout, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
            }
            base.ThrowPending();
            return InputChannel.HelpBeginReceive(this, timeout, callback, state);
        }

        public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
        {
            return this.BeginSend(message, base.DefaultSendTimeout, callback, state);
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", timeout, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
            }
            base.ThrowIfDisposedOrNotOpen();
            this.AddHeadersTo(message);
            return this.OnBeginSend(message, timeout, callback, state);
        }

        public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
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

        public Message EndReceive(IAsyncResult result)
        {
            return InputChannel.HelpEndReceive(result);
        }

        public void EndSend(IAsyncResult result)
        {
            this.OnEndSend(result);
        }

        public bool EndTryReceive(IAsyncResult result, out Message message)
        {
            return base.EndDequeue(result, out message);
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            return base.EndWaitForItem(result);
        }

        public override T GetProperty<T>() where T: class
        {
            if (typeof(T) == typeof(IDuplexChannel))
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

        protected virtual IAsyncResult OnBeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.OnSend(message, timeout);
            return new CompletedAsyncResult(callback, state);
        }

        protected virtual void OnEndSend(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected abstract void OnSend(Message message, TimeSpan timeout);
        public Message Receive()
        {
            return this.Receive(base.DefaultReceiveTimeout);
        }

        public Message Receive(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", timeout, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
            }
            base.ThrowPending();
            return InputChannel.HelpReceive(this, timeout);
        }

        public void Send(Message message)
        {
            this.Send(message, base.DefaultSendTimeout);
        }

        public void Send(Message message, TimeSpan timeout)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", timeout, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
            }
            base.ThrowIfDisposedOrNotOpen();
            this.AddHeadersTo(message);
            this.OnSend(message, timeout);
        }

        public bool TryReceive(TimeSpan timeout, out Message message)
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

        public virtual EndpointAddress LocalAddress
        {
            get
            {
                return this.localAddress;
            }
        }

        public abstract EndpointAddress RemoteAddress { get; }

        public abstract Uri Via { get; }
    }
}

