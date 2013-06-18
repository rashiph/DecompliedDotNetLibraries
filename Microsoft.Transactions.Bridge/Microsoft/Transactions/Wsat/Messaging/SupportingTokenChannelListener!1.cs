namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal class SupportingTokenChannelListener<TChannel> : LayeredChannelListener<TChannel> where TChannel: class, IChannel
    {
        private IChannelListener<TChannel> innerChannelListener;
        private Microsoft.Transactions.Wsat.Protocol.ProtocolVersion protocolVersion;
        private SupportingTokenSecurityTokenResolver tokenResolver;

        public SupportingTokenChannelListener(SupportingTokenBindingElement bindingElement, BindingContext context, SupportingTokenSecurityTokenResolver tokenResolver) : base(context.Binding, context.BuildInnerChannelListener<TChannel>())
        {
            this.protocolVersion = bindingElement.ProtocolVersion;
            this.tokenResolver = tokenResolver;
        }

        protected override TChannel OnAcceptChannel(TimeSpan timeout)
        {
            TChannel innerChannel = this.innerChannelListener.AcceptChannel(timeout);
            if (innerChannel == null)
            {
                return default(TChannel);
            }
            return this.WrapChannel(innerChannel);
        }

        protected override IAsyncResult OnBeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerChannelListener.BeginAcceptChannel(timeout, callback, state);
        }

        protected override IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerChannelListener.BeginWaitForChannel(timeout, callback, state);
        }

        protected override TChannel OnEndAcceptChannel(IAsyncResult result)
        {
            TChannel innerChannel = this.innerChannelListener.EndAcceptChannel(result);
            if (innerChannel == null)
            {
                return default(TChannel);
            }
            return this.WrapChannel(innerChannel);
        }

        protected override bool OnEndWaitForChannel(IAsyncResult result)
        {
            return this.innerChannelListener.EndWaitForChannel(result);
        }

        protected override void OnOpening()
        {
            base.OnOpening();
            this.innerChannelListener = (IChannelListener<TChannel>) this.InnerChannelListener;
        }

        protected override bool OnWaitForChannel(TimeSpan timeout)
        {
            return this.innerChannelListener.WaitForChannel(timeout);
        }

        private TChannel WrapChannel(TChannel innerChannel)
        {
            if (DebugTrace.Verbose)
            {
                DebugTrace.Trace(TraceLevel.Verbose, "Creating new SupportingTokenChannel<{0}>", typeof(TChannel).Name);
            }
            if (typeof(TChannel) == typeof(IDuplexChannel))
            {
                return (TChannel) new SupportingTokenDuplexChannel(this, (IDuplexChannel) innerChannel, this.tokenResolver, this.protocolVersion);
            }
            DiagnosticUtility.FailFast("SupportingTokenListener does not support " + typeof(TChannel).Name);
            return default(TChannel);
        }

        public Microsoft.Transactions.Wsat.Protocol.ProtocolVersion ProtocolVersion
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.protocolVersion;
            }
        }
    }
}

