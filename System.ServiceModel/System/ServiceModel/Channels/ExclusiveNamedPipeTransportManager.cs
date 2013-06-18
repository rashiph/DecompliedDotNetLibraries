namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal sealed class ExclusiveNamedPipeTransportManager : NamedPipeTransportManager
    {
        private ConnectionDemuxer connectionDemuxer;
        private IConnectionListener connectionListener;

        public ExclusiveNamedPipeTransportManager(Uri listenUri, NamedPipeChannelListener channelListener) : base(listenUri)
        {
            base.ApplyListenerSettings(channelListener);
            base.SetHostNameComparisonMode(channelListener.HostNameComparisonMode);
            base.SetAllowedUsers(channelListener.AllowedUsers);
        }

        internal override void OnAbort()
        {
            if (this.connectionDemuxer != null)
            {
                this.connectionDemuxer.Dispose();
            }
            if (this.connectionListener != null)
            {
                this.connectionListener.Dispose();
            }
            base.OnAbort();
        }

        internal override void OnClose(TimeSpan timeout)
        {
            this.connectionDemuxer.Dispose();
            this.connectionListener.Dispose();
            base.OnClose(timeout);
        }

        internal override void OnOpen()
        {
            this.connectionListener = new BufferedConnectionListener(new PipeConnectionListener(base.ListenUri, base.HostNameComparisonMode, base.ConnectionBufferSize, base.AllowedUsers, true, 0x7fffffff), base.MaxOutputDelay, base.ConnectionBufferSize);
            if (DiagnosticUtility.ShouldUseActivity)
            {
                this.connectionListener = new TracingConnectionListener(this.connectionListener, base.ListenUri.ToString(), false);
            }
            this.connectionDemuxer = new ConnectionDemuxer(this.connectionListener, base.MaxPendingAccepts, base.MaxPendingConnections, base.ChannelInitializationTimeout, base.IdleTimeout, base.MaxPooledConnections, new TransportSettingsCallback(this.OnGetTransportFactorySettings), new SingletonPreambleDemuxCallback(this.OnGetSingletonMessageHandler), new ServerSessionPreambleDemuxCallback(this.OnHandleServerSessionPreamble), new System.ServiceModel.Channels.ErrorCallback(this.OnDemuxerError));
            bool flag = false;
            try
            {
                this.connectionDemuxer.StartDemuxing();
                flag = true;
            }
            finally
            {
                if (!flag)
                {
                    this.connectionDemuxer.Dispose();
                }
            }
        }
    }
}

