namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    public abstract class ChannelFactoryBase<TChannel> : ChannelFactoryBase, IChannelFactory<TChannel>, IChannelFactory, ICommunicationObject
    {
        private CommunicationObjectManager<IChannel> channels;

        protected ChannelFactoryBase() : this(null)
        {
        }

        protected ChannelFactoryBase(IDefaultCommunicationTimeouts timeouts) : base(timeouts)
        {
            this.channels = new CommunicationObjectManager<IChannel>(base.ThisLock);
        }

        public TChannel CreateChannel(EndpointAddress address)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }
            return this.InternalCreateChannel(address, address.Uri);
        }

        public TChannel CreateChannel(EndpointAddress address, Uri via)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }
            if (via == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("via");
            }
            return this.InternalCreateChannel(address, via);
        }

        private TChannel InternalCreateChannel(EndpointAddress address, Uri via)
        {
            this.ValidateCreateChannel();
            TChannel local = this.OnCreateChannel(address, via);
            bool flag = false;
            try
            {
                this.channels.Add((IChannel) local);
                flag = true;
            }
            finally
            {
                if (!flag)
                {
                    ((IChannel) local).Abort();
                }
            }
            return local;
        }

        protected override void OnAbort()
        {
            foreach (IChannel channel in this.channels.ToArray())
            {
                channel.Abort();
            }
            this.channels.Abort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedCloseAsyncResult(timeout, callback, state, new ChainedBeginHandler(this.channels.BeginClose), new ChainedEndHandler(this.channels.EndClose), this.channels.ToArray());
        }

        protected override void OnClose(TimeSpan timeout)
        {
            IChannel[] channelArray = this.channels.ToArray();
            TimeoutHelper helper = new TimeoutHelper(timeout);
            foreach (IChannel channel in channelArray)
            {
                channel.Close(helper.RemainingTime());
            }
            this.channels.Close(helper.RemainingTime());
        }

        protected abstract TChannel OnCreateChannel(EndpointAddress address, Uri via);
        protected override void OnEndClose(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        protected void ValidateCreateChannel()
        {
            base.ThrowIfDisposed();
            if (base.State != CommunicationState.Opened)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ChannelFactoryCannotBeUsedToCreateChannels", new object[] { base.GetType().ToString() })));
            }
        }
    }
}

