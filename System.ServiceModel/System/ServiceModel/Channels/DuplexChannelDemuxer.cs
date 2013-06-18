namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal class DuplexChannelDemuxer : DatagramChannelDemuxer<IDuplexChannel, Message>
    {
        public DuplexChannelDemuxer(BindingContext context) : base(context)
        {
        }

        protected override void AbortItem(Message message)
        {
            TypedChannelDemuxer.AbortMessage(message);
        }

        protected override IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return base.InnerChannel.BeginReceive(timeout, callback, state);
        }

        protected override LayeredChannelListener<IDuplexChannel> CreateListener<IDuplexChannel>(ChannelDemuxerFilter filter) where IDuplexChannel: class, IChannel
        {
            SingletonChannelListener<IDuplexChannel, DuplexChannel, Message> listener;
            return new SingletonChannelListener<IDuplexChannel, DuplexChannel, Message>(filter, this) { Acceptor = (IChannelAcceptor<IDuplexChannel>) new DuplexChannelAcceptor(listener, this) };
        }

        protected override void Dispatch(IChannelListener listener)
        {
            ((SingletonChannelListener<IDuplexChannel, DuplexChannel, Message>) listener).Dispatch();
        }

        protected override void EndpointNotFound(Message message)
        {
            if (base.DemuxFailureHandler != null)
            {
                base.DemuxFailureHandler.HandleDemuxFailure(message);
            }
            this.AbortItem(message);
        }

        protected override Message EndReceive(IAsyncResult result)
        {
            return base.InnerChannel.EndReceive(result);
        }

        protected override void EnqueueAndDispatch(IChannelListener listener, Exception exception, Action dequeuedCallback, bool canDispatchOnThisThread)
        {
            ((SingletonChannelListener<IDuplexChannel, DuplexChannel, Message>) listener).EnqueueAndDispatch(exception, dequeuedCallback, canDispatchOnThisThread);
        }

        protected override void EnqueueAndDispatch(IChannelListener listener, Message message, Action dequeuedCallback, bool canDispatchOnThisThread)
        {
            ((SingletonChannelListener<IDuplexChannel, DuplexChannel, Message>) listener).EnqueueAndDispatch(message, dequeuedCallback, canDispatchOnThisThread);
        }

        protected override Message GetMessage(Message message)
        {
            return message;
        }

        private class DuplexChannelAcceptor : SingletonChannelAcceptor<IDuplexChannel, DuplexChannel, Message>
        {
            private DuplexChannelDemuxer demuxer;

            public DuplexChannelAcceptor(ChannelManagerBase channelManager, DuplexChannelDemuxer demuxer) : base(channelManager)
            {
                this.demuxer = demuxer;
            }

            protected override DuplexChannel OnCreateChannel()
            {
                return new DuplexChannelDemuxer.DuplexChannelWrapper(base.ChannelManager, this.demuxer.InnerChannel);
            }

            protected override void OnTraceMessageReceived(Message message)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    TraceUtility.TraceEvent(TraceEventType.Information, 0x40013, System.ServiceModel.SR.GetString("TraceCodeMessageReceived"), MessageTransmitTraceRecord.CreateReceiveTraceRecord(message), this, null);
                }
            }
        }

        private class DuplexChannelWrapper : DuplexChannel
        {
            private IDuplexChannel innerChannel;

            public DuplexChannelWrapper(ChannelManagerBase channelManager, IDuplexChannel innerChannel) : base(channelManager, innerChannel.LocalAddress)
            {
                this.innerChannel = innerChannel;
            }

            protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new CompletedAsyncResult(callback, state);
            }

            protected override IAsyncResult OnBeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.innerChannel.BeginSend(message, timeout, callback, state);
            }

            protected override void OnEndOpen(IAsyncResult result)
            {
                CompletedAsyncResult.End(result);
            }

            protected override void OnEndSend(IAsyncResult result)
            {
                this.innerChannel.EndSend(result);
            }

            protected override void OnOpen(TimeSpan timeout)
            {
            }

            protected override void OnSend(Message message, TimeSpan timeout)
            {
                this.innerChannel.Send(message, timeout);
            }

            public override EndpointAddress RemoteAddress
            {
                get
                {
                    return this.innerChannel.RemoteAddress;
                }
            }

            public override Uri Via
            {
                get
                {
                    return this.innerChannel.Via;
                }
            }
        }
    }
}

