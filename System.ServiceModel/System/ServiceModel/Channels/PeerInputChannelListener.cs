namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal sealed class PeerInputChannelListener : PeerChannelListener<IInputChannel, PeerInputChannelAcceptor>
    {
        private PeerInputChannelAcceptor inputAcceptor;

        public PeerInputChannelListener(PeerTransportBindingElement bindingElement, BindingContext context, PeerResolver peerResolver) : base(bindingElement, context, peerResolver)
        {
        }

        protected override void CreateAcceptor()
        {
            this.inputAcceptor = new PeerInputChannelAcceptor(base.InnerNode, base.Registration, this, new EndpointAddress(this.Uri, new AddressHeader[0]), this.Uri);
        }

        protected override PeerInputChannelAcceptor ChannelAcceptor
        {
            get
            {
                return this.inputAcceptor;
            }
        }
    }
}

