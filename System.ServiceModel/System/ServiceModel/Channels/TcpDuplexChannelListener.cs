namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal class TcpDuplexChannelListener : TcpChannelListener<IDuplexSessionChannel, InputQueueChannelAcceptor<IDuplexSessionChannel>>, ISessionPreambleHandler
    {
        private InputQueueChannelAcceptor<IDuplexSessionChannel> duplexAcceptor;

        public TcpDuplexChannelListener(TcpTransportBindingElement bindingElement, BindingContext context) : base(bindingElement, context)
        {
            this.duplexAcceptor = new InputQueueChannelAcceptor<IDuplexSessionChannel>(this);
        }

        void ISessionPreambleHandler.HandleServerSessionPreamble(ServerSessionPreambleConnectionReader preambleReader, ConnectionDemuxer connectionDemuxer)
        {
            IDuplexSessionChannel channel = preambleReader.CreateDuplexSessionChannel(this, new EndpointAddress(this.Uri, new AddressHeader[0]), base.ExposeConnectionProperty, connectionDemuxer);
            this.duplexAcceptor.EnqueueAndDispatch(channel, preambleReader.ConnectionDequeuedCallback);
        }

        protected override InputQueueChannelAcceptor<IDuplexSessionChannel> ChannelAcceptor
        {
            get
            {
                return this.duplexAcceptor;
            }
        }
    }
}

