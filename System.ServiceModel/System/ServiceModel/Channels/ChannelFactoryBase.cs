namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    public abstract class ChannelFactoryBase : ChannelManagerBase, IChannelFactory, ICommunicationObject
    {
        private TimeSpan closeTimeout;
        private TimeSpan openTimeout;
        private TimeSpan receiveTimeout;
        private TimeSpan sendTimeout;

        protected ChannelFactoryBase()
        {
            this.closeTimeout = ServiceDefaults.CloseTimeout;
            this.openTimeout = ServiceDefaults.OpenTimeout;
            this.receiveTimeout = ServiceDefaults.ReceiveTimeout;
            this.sendTimeout = ServiceDefaults.SendTimeout;
        }

        protected ChannelFactoryBase(IDefaultCommunicationTimeouts timeouts)
        {
            this.closeTimeout = ServiceDefaults.CloseTimeout;
            this.openTimeout = ServiceDefaults.OpenTimeout;
            this.receiveTimeout = ServiceDefaults.ReceiveTimeout;
            this.sendTimeout = ServiceDefaults.SendTimeout;
            this.InitializeTimeouts(timeouts);
        }

        public virtual T GetProperty<T>() where T: class
        {
            if (typeof(T) == typeof(IChannelFactory))
            {
                return (T) this;
            }
            return default(T);
        }

        private void InitializeTimeouts(IDefaultCommunicationTimeouts timeouts)
        {
            if (timeouts != null)
            {
                this.closeTimeout = timeouts.CloseTimeout;
                this.openTimeout = timeouts.OpenTimeout;
                this.receiveTimeout = timeouts.ReceiveTimeout;
                this.sendTimeout = timeouts.SendTimeout;
            }
        }

        protected override void OnAbort()
        {
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
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
    }
}

