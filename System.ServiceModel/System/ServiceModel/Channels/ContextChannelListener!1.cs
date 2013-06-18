namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal class ContextChannelListener<TChannel> : LayeredChannelListener<TChannel> where TChannel: class, IChannel
    {
        private ContextExchangeMechanism contextExchangeMechanism;
        private Uri listenBaseAddress;

        public ContextChannelListener(BindingContext context, ContextExchangeMechanism contextExchangeMechanism) : base((context == null) ? null : context.Binding, (context == null) ? null : context.BuildInnerChannelListener<TChannel>())
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (!ContextExchangeMechanismHelper.IsDefined(contextExchangeMechanism))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("contextExchangeMechanism"));
            }
            this.contextExchangeMechanism = contextExchangeMechanism;
            this.listenBaseAddress = context.ListenUriBaseAddress;
        }

        private TChannel InternalAcceptChannel(TChannel innerChannel)
        {
            if (innerChannel == null)
            {
                return innerChannel;
            }
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0xf0004, System.ServiceModel.SR.GetString("TraceCodeContextChannelListenerChannelAccepted"), this);
            }
            if (typeof(TChannel) == typeof(IInputChannel))
            {
                return (TChannel) new ContextInputChannel(this, (IInputChannel) innerChannel, this.contextExchangeMechanism);
            }
            if (typeof(TChannel) == typeof(IInputSessionChannel))
            {
                return (TChannel) new ContextInputSessionChannel(this, (IInputSessionChannel) innerChannel, this.contextExchangeMechanism);
            }
            if (typeof(TChannel) == typeof(IReplyChannel))
            {
                return (TChannel) new ContextReplyChannel(this, (IReplyChannel) innerChannel, this.contextExchangeMechanism);
            }
            if (typeof(TChannel) == typeof(IReplySessionChannel))
            {
                return (TChannel) new ContextReplySessionChannel(this, (IReplySessionChannel) innerChannel, this.contextExchangeMechanism);
            }
            return (TChannel) new ContextDuplexSessionChannel(this, (IDuplexSessionChannel) innerChannel, this.contextExchangeMechanism);
        }

        protected override TChannel OnAcceptChannel(TimeSpan timeout)
        {
            return this.InternalAcceptChannel(((IChannelListener<TChannel>) this.InnerChannelListener).AcceptChannel(timeout));
        }

        protected override IAsyncResult OnBeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return ((IChannelListener<TChannel>) this.InnerChannelListener).BeginAcceptChannel(timeout, callback, state);
        }

        protected override IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.InnerChannelListener.BeginWaitForChannel(timeout, callback, state);
        }

        protected override TChannel OnEndAcceptChannel(IAsyncResult result)
        {
            return this.InternalAcceptChannel(((IChannelListener<TChannel>) this.InnerChannelListener).EndAcceptChannel(result));
        }

        protected override bool OnEndWaitForChannel(IAsyncResult result)
        {
            return this.InnerChannelListener.EndWaitForChannel(result);
        }

        protected override bool OnWaitForChannel(TimeSpan timeout)
        {
            return this.InnerChannelListener.WaitForChannel(timeout);
        }
    }
}

