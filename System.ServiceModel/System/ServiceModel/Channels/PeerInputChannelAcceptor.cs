namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal sealed class PeerInputChannelAcceptor : SingletonChannelAcceptor<IInputChannel, PeerInputChannel, Message>
    {
        private PeerMessageDispatcher<IInputChannel, PeerInputChannel> dispatcher;
        private EndpointAddress localAddress;
        private PeerNodeImplementation peerNode;
        private PeerNodeImplementation.Registration registration;
        private Uri via;

        public PeerInputChannelAcceptor(PeerNodeImplementation peerNode, PeerNodeImplementation.Registration registration, ChannelManagerBase channelManager, EndpointAddress localAddress, Uri via) : base(channelManager)
        {
            this.registration = registration;
            this.peerNode = peerNode;
            this.localAddress = localAddress;
            this.via = via;
            PeerMessageDispatcher<IInputChannel, PeerInputChannel>.PeerMessageQueueAdapter queueHandler = new PeerMessageDispatcher<IInputChannel, PeerInputChannel>.PeerMessageQueueAdapter(this);
            this.dispatcher = new PeerMessageDispatcher<IInputChannel, PeerInputChannel>(queueHandler, peerNode, base.ChannelManager, localAddress, via);
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

        protected override PeerInputChannel OnCreateChannel()
        {
            return new PeerInputChannel(this.peerNode, this.registration, base.ChannelManager, this.localAddress, this.via);
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

