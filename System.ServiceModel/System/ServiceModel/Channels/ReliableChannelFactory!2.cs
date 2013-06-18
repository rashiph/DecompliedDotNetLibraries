namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    internal class ReliableChannelFactory<TChannel, InnerChannel> : ChannelFactoryBase<TChannel>, IReliableFactorySettings where InnerChannel: class, IChannel
    {
        private TimeSpan acknowledgementInterval;
        private System.ServiceModel.Channels.FaultHelper faultHelper;
        private bool flowControlEnabled;
        private TimeSpan inactivityTimeout;
        private IChannelFactory<InnerChannel> innerChannelFactory;
        private int maxPendingChannels;
        private int maxRetryCount;
        private int maxTransferWindowSize;
        private System.ServiceModel.Channels.MessageVersion messageVersion;
        private bool ordered;
        private System.ServiceModel.ReliableMessagingVersion reliableMessagingVersion;

        public ReliableChannelFactory(ReliableSessionBindingElement settings, IChannelFactory<InnerChannel> innerChannelFactory, Binding binding) : base(binding)
        {
            this.acknowledgementInterval = settings.AcknowledgementInterval;
            this.flowControlEnabled = settings.FlowControlEnabled;
            this.inactivityTimeout = settings.InactivityTimeout;
            this.maxPendingChannels = settings.MaxPendingChannels;
            this.maxRetryCount = settings.MaxRetryCount;
            this.maxTransferWindowSize = settings.MaxTransferWindowSize;
            this.messageVersion = binding.MessageVersion;
            this.ordered = settings.Ordered;
            this.reliableMessagingVersion = settings.ReliableMessagingVersion;
            this.innerChannelFactory = innerChannelFactory;
            this.faultHelper = new SendFaultHelper(binding.SendTimeout, binding.CloseTimeout);
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
            this.faultHelper.Abort();
            this.innerChannelFactory.Abort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return OperationWithTimeoutComposer.BeginComposeAsyncOperations(timeout, new OperationWithTimeoutBeginCallback[] { new OperationWithTimeoutBeginCallback(this.OnBeginClose), new OperationWithTimeoutBeginCallback(this.faultHelper.BeginClose), new OperationWithTimeoutBeginCallback(this.innerChannelFactory.BeginClose) }, new OperationEndCallback[] { new OperationEndCallback(this.OnEndClose), new OperationEndCallback(this.faultHelper.EndClose), new OperationEndCallback(this.innerChannelFactory.EndClose) }, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerChannelFactory.BeginOpen(callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            base.OnClose(helper.RemainingTime());
            this.faultHelper.Close(helper.RemainingTime());
            this.innerChannelFactory.Close(helper.RemainingTime());
        }

        protected override TChannel OnCreateChannel(EndpointAddress address, Uri via)
        {
            LateBoundChannelParameterCollection channelParameters = new LateBoundChannelParameterCollection();
            IClientReliableChannelBinder binder = ClientReliableChannelBinder<InnerChannel>.CreateBinder(address, via, this.InnerChannelFactory, MaskingMode.All, TolerateFaultsMode.IfNotSecuritySession, channelParameters, this.DefaultCloseTimeout, this.DefaultSendTimeout);
            if (typeof(TChannel) == typeof(IOutputSessionChannel))
            {
                if (!(typeof(InnerChannel) == typeof(IDuplexChannel)) && !(typeof(InnerChannel) == typeof(IDuplexSessionChannel)))
                {
                    return (TChannel) new ReliableOutputSessionChannelOverRequest(this, this, binder, this.faultHelper, channelParameters);
                }
                return (TChannel) new ReliableOutputSessionChannelOverDuplex(this, this, binder, this.faultHelper, channelParameters);
            }
            if (typeof(TChannel) == typeof(IDuplexSessionChannel))
            {
                return (TChannel) new ClientReliableDuplexSessionChannel(this, this, binder, this.faultHelper, channelParameters, WsrmUtilities.NextSequenceId());
            }
            return (TChannel) new ReliableRequestSessionChannel(this, this, binder, this.faultHelper, channelParameters, WsrmUtilities.NextSequenceId());
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            OperationWithTimeoutComposer.EndComposeAsyncOperations(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            this.innerChannelFactory.EndOpen(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            this.innerChannelFactory.Open(timeout);
        }

        public TimeSpan AcknowledgementInterval
        {
            get
            {
                return this.acknowledgementInterval;
            }
        }

        public System.ServiceModel.Channels.FaultHelper FaultHelper
        {
            get
            {
                return this.faultHelper;
            }
        }

        public bool FlowControlEnabled
        {
            get
            {
                return this.flowControlEnabled;
            }
        }

        public TimeSpan InactivityTimeout
        {
            get
            {
                return this.inactivityTimeout;
            }
        }

        protected IChannelFactory<InnerChannel> InnerChannelFactory
        {
            get
            {
                return this.innerChannelFactory;
            }
        }

        public int MaxPendingChannels
        {
            get
            {
                return this.maxPendingChannels;
            }
        }

        public int MaxRetryCount
        {
            get
            {
                return this.maxRetryCount;
            }
        }

        public int MaxTransferWindowSize
        {
            get
            {
                return this.maxTransferWindowSize;
            }
        }

        public System.ServiceModel.Channels.MessageVersion MessageVersion
        {
            get
            {
                return this.messageVersion;
            }
        }

        public bool Ordered
        {
            get
            {
                return this.ordered;
            }
        }

        public System.ServiceModel.ReliableMessagingVersion ReliableMessagingVersion
        {
            get
            {
                return this.reliableMessagingVersion;
            }
        }

        public TimeSpan SendTimeout
        {
            get
            {
                return base.InternalSendTimeout;
            }
        }
    }
}

