namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal class ContextChannelFactory<TChannel> : LayeredChannelFactory<TChannel>
    {
        private Uri callbackAddress;
        private ContextExchangeMechanism contextExchangeMechanism;
        private bool contextManagementEnabled;

        public ContextChannelFactory(BindingContext context, ContextExchangeMechanism contextExchangeMechanism, Uri callbackAddress, bool contextManagementEnabled) : base((context == null) ? null : context.Binding, (context == null) ? null : context.BuildInnerChannelFactory<TChannel>())
        {
            if (!ContextExchangeMechanismHelper.IsDefined(contextExchangeMechanism))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("contextExchangeMechanism"));
            }
            this.contextExchangeMechanism = contextExchangeMechanism;
            this.callbackAddress = callbackAddress;
            this.contextManagementEnabled = contextManagementEnabled;
        }

        protected override TChannel OnCreateChannel(EndpointAddress address, Uri via)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                string content = System.ServiceModel.SR.GetString("ContextChannelFactoryChannelCreatedDetail", new object[] { address, via });
                TraceUtility.TraceEvent(TraceEventType.Information, 0xf0003, System.ServiceModel.SR.GetString("TraceCodeContextChannelFactoryChannelCreated"), new StringTraceRecord("ChannelDetail", content), this, null);
            }
            if (typeof(TChannel) == typeof(IOutputChannel))
            {
                return (TChannel) new ContextOutputChannel(this, ((IChannelFactory<IOutputChannel>) base.InnerChannelFactory).CreateChannel(address, via), this.contextExchangeMechanism, this.callbackAddress, this.contextManagementEnabled);
            }
            if (typeof(TChannel) == typeof(IOutputSessionChannel))
            {
                return (TChannel) new ContextOutputSessionChannel(this, ((IChannelFactory<IOutputSessionChannel>) base.InnerChannelFactory).CreateChannel(address, via), this.contextExchangeMechanism, this.callbackAddress, this.contextManagementEnabled);
            }
            if (typeof(TChannel) == typeof(IRequestChannel))
            {
                return (TChannel) new ContextRequestChannel(this, ((IChannelFactory<IRequestChannel>) base.InnerChannelFactory).CreateChannel(address, via), this.contextExchangeMechanism, this.callbackAddress, this.contextManagementEnabled);
            }
            if (typeof(TChannel) == typeof(IRequestSessionChannel))
            {
                return (TChannel) new ContextRequestSessionChannel(this, ((IChannelFactory<IRequestSessionChannel>) base.InnerChannelFactory).CreateChannel(address, via), this.contextExchangeMechanism, this.callbackAddress, this.contextManagementEnabled);
            }
            return (TChannel) new ContextDuplexSessionChannel(this, ((IChannelFactory<IDuplexSessionChannel>) base.InnerChannelFactory).CreateChannel(address, via), this.contextExchangeMechanism, via, this.callbackAddress, this.contextManagementEnabled);
        }
    }
}

