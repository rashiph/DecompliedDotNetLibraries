namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal sealed class PeerDuplexChannelListener : PeerChannelListener<IDuplexChannel, PeerDuplexChannelAcceptor>
    {
        private PeerDuplexChannelAcceptor duplexAcceptor;

        public PeerDuplexChannelListener(PeerTransportBindingElement bindingElement, BindingContext context, PeerResolver peerResolver) : base(bindingElement, context, peerResolver)
        {
        }

        protected override void CreateAcceptor()
        {
            this.duplexAcceptor = new PeerDuplexChannelAcceptor(base.InnerNode, base.Registration, this, new EndpointAddress(this.Uri, new AddressHeader[0]), base.BaseUri);
        }

        protected override PeerDuplexChannelAcceptor ChannelAcceptor
        {
            get
            {
                return this.duplexAcceptor;
            }
        }
    }
}

