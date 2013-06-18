namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    public abstract class ChannelListenerBase : ChannelManagerBase, IChannelListener, ICommunicationObject
    {
        private TimeSpan closeTimeout;
        private TimeSpan openTimeout;
        private TimeSpan receiveTimeout;
        private TimeSpan sendTimeout;

        protected ChannelListenerBase()
        {
            this.closeTimeout = ServiceDefaults.CloseTimeout;
            this.openTimeout = ServiceDefaults.OpenTimeout;
            this.receiveTimeout = ServiceDefaults.ReceiveTimeout;
            this.sendTimeout = ServiceDefaults.SendTimeout;
        }

        protected ChannelListenerBase(IDefaultCommunicationTimeouts timeouts)
        {
            this.closeTimeout = ServiceDefaults.CloseTimeout;
            this.openTimeout = ServiceDefaults.OpenTimeout;
            this.receiveTimeout = ServiceDefaults.ReceiveTimeout;
            this.sendTimeout = ServiceDefaults.SendTimeout;
            if (timeouts != null)
            {
                this.closeTimeout = timeouts.CloseTimeout;
                this.openTimeout = timeouts.OpenTimeout;
                this.receiveTimeout = timeouts.ReceiveTimeout;
                this.sendTimeout = timeouts.SendTimeout;
            }
        }

        public IAsyncResult BeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            base.ThrowIfNotOpened();
            base.ThrowPending();
            return this.OnBeginWaitForChannel(timeout, callback, state);
        }

        public bool EndWaitForChannel(IAsyncResult result)
        {
            return this.OnEndWaitForChannel(result);
        }

        public virtual T GetProperty<T>() where T: class
        {
            if (typeof(T) == typeof(IChannelListener))
            {
                return (T) this;
            }
            return default(T);
        }

        protected abstract IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state);
        protected abstract bool OnEndWaitForChannel(IAsyncResult result);
        protected abstract bool OnWaitForChannel(TimeSpan timeout);
        public bool WaitForChannel(TimeSpan timeout)
        {
            base.ThrowIfNotOpened();
            base.ThrowPending();
            return this.OnWaitForChannel(timeout);
        }

        protected override TimeSpan DefaultCloseTimeout
        {
            get
            {
                return this.closeTimeout;
            }
        }

        protected override TimeSpan DefaultOpenTimeout
        {
            get
            {
                return this.openTimeout;
            }
        }

        protected override TimeSpan DefaultReceiveTimeout
        {
            get
            {
                return this.receiveTimeout;
            }
        }

        protected override TimeSpan DefaultSendTimeout
        {
            get
            {
                return this.sendTimeout;
            }
        }

        public abstract System.Uri Uri { get; }
    }
}

