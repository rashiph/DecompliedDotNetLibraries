namespace System.ServiceModel.Channels
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.ServiceModel;

    internal sealed class ExclusiveTcpTransportManager : TcpTransportManager, ISocketListenerSettings
    {
        private bool closed;
        private ConnectionDemuxer connectionDemuxer;
        private IConnectionListener connectionListener;
        private System.Net.IPAddress ipAddress;
        private int listenBacklog;
        private Socket listenSocket;
        private ExclusiveTcpTransportManagerRegistration registration;

        public ExclusiveTcpTransportManager(ExclusiveTcpTransportManagerRegistration registration, TcpChannelListener channelListener, System.Net.IPAddress ipAddressAny, UriHostNameType ipHostNameType)
        {
            base.ApplyListenerSettings(channelListener);
            this.listenSocket = channelListener.GetListenSocket(ipHostNameType);
            if (this.listenSocket != null)
            {
                this.ipAddress = ((IPEndPoint) this.listenSocket.LocalEndPoint).Address;
            }
            else if (channelListener.Uri.HostNameType == ipHostNameType)
            {
                this.ipAddress = System.Net.IPAddress.Parse(channelListener.Uri.DnsSafeHost);
            }
            else
            {
                this.ipAddress = ipAddressAny;
            }
            this.listenBacklog = channelListener.ListenBacklog;
            this.registration = registration;
        }

        private void Cleanup()
        {
            lock (base.ThisLock)
            {
                if (this.closed)
                {
                    return;
                }
                this.closed = true;
            }
            if (this.connectionDemuxer != null)
            {
                this.connectionDemuxer.Dispose();
            }
            if (this.connectionListener != null)
            {
                this.connectionListener.Dispose();
            }
            this.registration.OnClose(this);
        }

        internal override void OnAbort()
        {
            this.Cleanup();
            base.OnAbort();
        }

        internal override void OnClose(TimeSpan timeout)
        {
            this.Cleanup();
        }

        internal override void OnOpen()
        {
            SocketConnectionListener connectionListener = null;
            if (this.listenSocket != null)
            {
                connectionListener = new SocketConnectionListener(this.listenSocket, this, false);
                this.listenSocket = null;
            }
            else
            {
                int port = this.registration.ListenUri.Port;
                if (port == -1)
                {
                    port = 0x328;
                }
                connectionListener = new SocketConnectionListener(new IPEndPoint(this.ipAddress, port), this, false);
            }
            this.connectionListener = new BufferedConnectionListener(connectionListener, base.MaxOutputDelay, base.ConnectionBufferSize);
            if (DiagnosticUtility.ShouldUseActivity)
            {
                this.connectionListener = new TracingConnectionListener(this.connectionListener, this.registration.ListenUri.ToString(), false);
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

        public System.Net.IPAddress IPAddress
        {
            get
            {
                return this.ipAddress;
            }
        }

        public int ListenBacklog
        {
            get
            {
                return this.listenBacklog;
            }
        }

        int ISocketListenerSettings.BufferSize
        {
            get
            {
                return base.ConnectionBufferSize;
            }
        }

        int ISocketListenerSettings.ListenBacklog
        {
            get
            {
                return this.ListenBacklog;
            }
        }

        bool ISocketListenerSettings.TeredoEnabled
        {
            get
            {
                return this.registration.TeredoEnabled;
            }
        }
    }
}

