namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    internal abstract class LayeredChannelFactory<TChannel> : ChannelFactoryBase<TChannel>
    {
        private IChannelFactory innerChannelFactory;

        public LayeredChannelFactory(IDefaultCommunicationTimeouts timeouts, IChannelFactory innerChannelFactory) : base(timeouts)
        {
            this.innerChannelFactory = innerChannelFactory;
        }

        public override T GetProperty<T>() where T: class
        {
            if (typeof(T) == typeof(IChannelFactory<TChannel>))
            {
                return (T) this;
            }
            T property = base.GetProperty<T>();
            if (property != null)
            {
                return property;
            }
            return this.innerChannelFactory.GetProperty<T>();
        }

        protected override void OnAbort()
        {
            base.OnAbort();
            this.innerChannelFactory.Abort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedCloseAsyncResult(timeout, callback, state, new ChainedBeginHandler(this.OnBeginClose), new ChainedEndHandler(this.OnEndClose), new ICommunicationObject[] { this.innerChannelFactory });
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerChannelFactory.BeginOpen(timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            base.OnClose(helper.RemainingTime());
            this.innerChannelFactory.Close(helper.RemainingTime());
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            this.innerChannelFactory.EndOpen(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            this.innerChannelFactory.Open(timeout);
        }

        protected IChannelFactory InnerChannelFactory
        {
            get
            {
                return this.innerChannelFactory;
            }
        }
    }
}

