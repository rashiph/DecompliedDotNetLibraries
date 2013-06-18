namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal sealed class PeerDuplexChannelAcceptor : SingletonChannelAcceptor<IDuplexChannel, PeerDuplexChannel, Message>
    {
        private PeerMessageDispatcher<IDuplexChannel, PeerDuplexChannel> dispatcher;
        private EndpointAddress localAddress;
        private PeerNodeImplementation peerNode;
        private PeerNodeImplementation.Registration registration;
        private Uri via;

        public PeerDuplexChannelAcceptor(PeerNodeImplementation peerNode, PeerNodeImplementation.Registration registration, ChannelManagerBase channelManager, EndpointAddress localAddress, Uri via) : base(channelManager)
        {
            this.registration = registration;
            this.peerNode = peerNode;
            this.localAddress = localAddress;
            this.via = via;
            PeerMessageDispatcher<IDuplexChannel, PeerDuplexChannel>.PeerMessageQueueAdapter queueHandler = new PeerMessageDispatcher<IDuplexChannel, PeerDuplexChannel>.PeerMessageQueueAdapter(this);
            this.dispatcher = new PeerMessageDispatcher<IDuplexChannel, PeerDuplexChannel>(queueHandler, peerNode, base.ChannelManager, localAddress, via);
        }

        private void CloseDispatcher()
        {
            if (this.dispatcher != null)
            {
                this.dispatcher.Unregister(true);
                this.dispatcher = null;
            }
        }

        protected override void OnClose(TimeSpan timeout)
        {
        }

        protected override void OnClosing()
        {
            this.CloseDispatcher();
            base.OnClosing();
        }

        protected override PeerDuplexChannel OnCreateChannel()
        {
            return new PeerDuplexChannel(this.peerNode, this.registration, base.ChannelManager, this.localAddress, this.via);
        }

        protected override void OnFaulted()
        {
            this.CloseDispatcher();
            base.OnFaulted();
        }

        protected override void OnTraceMessageReceived(Message message)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0x40013, System.ServiceModel.SR.GetString("TraceCodeMessageReceived"), MessageTransmitTraceRecord.CreateReceiveTraceRecord(message), this, null);
            }
        }
    }
}

