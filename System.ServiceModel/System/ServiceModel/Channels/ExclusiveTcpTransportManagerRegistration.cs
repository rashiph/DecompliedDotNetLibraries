namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;

    internal class ExclusiveTcpTransportManagerRegistration : TransportManagerRegistration
    {
        private TimeSpan channelInitializationTimeout;
        private int connectionBufferSize;
        private TimeSpan idleTimeout;
        private ExclusiveTcpTransportManager ipv4TransportManager;
        private ExclusiveTcpTransportManager ipv6TransportManager;
        private int listenBacklog;
        private TimeSpan maxOutputDelay;
        private int maxPendingAccepts;
        private int maxPendingConnections;
        private int maxPooledConnections;
        private bool teredoEnabled;

        public ExclusiveTcpTransportManagerRegistration(Uri listenUri, TcpChannelListener channelListener) : base(listenUri, channelListener.HostNameComparisonMode)
        {
            this.connectionBufferSize = channelListener.ConnectionBufferSize;
            this.channelInitializationTimeout = channelListener.ChannelInitializationTimeout;
            this.teredoEnabled = channelListener.TeredoEnabled;
            this.listenBacklog = channelListener.ListenBacklog;
            this.maxOutputDelay = channelListener.MaxOutputDelay;
            this.maxPendingConnections = channelListener.MaxPendingConnections;
            this.maxPendingAccepts = channelListener.MaxPendingAccepts;
            this.idleTimeout = channelListener.IdleTimeout;
            this.maxPooledConnections = channelListener.MaxPooledConnections;
        }

        private bool IsCompatible(TcpChannelListener channelListener, bool useIPv4, bool useIPv6)
        {
            if (channelListener.InheritBaseAddressSettings)
            {
                return true;
            }
            if (useIPv6 && !channelListener.IsScopeIdCompatible(base.HostNameComparisonMode, base.ListenUri))
            {
                return false;
            }
            return ((((!channelListener.PortSharingEnabled && (useIPv4 || useIPv6)) && ((((this.channelInitializationTimeout == channelListener.ChannelInitializationTimeout) && (this.idleTimeout == channelListener.IdleTimeout)) && ((this.maxPooledConnections == channelListener.MaxPooledConnections) && (this.connectionBufferSize == channelListener.ConnectionBufferSize))) && (!useIPv6 || (this.teredoEnabled == channelListener.TeredoEnabled)))) && (((this.listenBacklog == channelListener.ListenBacklog) && (this.maxPendingConnections == channelListener.MaxPendingConnections)) && (this.maxOutputDelay == channelListener.MaxOutputDelay))) && (this.maxPendingAccepts == channelListener.MaxPendingAccepts));
        }

        public void OnClose(TcpTransportManager manager)
        {
            if (manager == this.ipv4TransportManager)
            {
                this.ipv4TransportManager = null;
            }
            else if (manager == this.ipv6TransportManager)
            {
                this.ipv6TransportManager = null;
            }
            if ((this.ipv4TransportManager == null) && (this.ipv6TransportManager == null))
            {
                TcpChannelListener.StaticTransportManagerTable.UnregisterUri(base.ListenUri, base.HostNameComparisonMode);
            }
        }

        private void ProcessSelection(TcpChannelListener channelListener, IPAddress ipAddressAny, UriHostNameType ipHostNameType, ref ExclusiveTcpTransportManager transportManager, IList<TransportManager> result)
        {
            if (transportManager == null)
            {
                transportManager = new ExclusiveTcpTransportManager(this, channelListener, ipAddressAny, ipHostNameType);
            }
            result.Add(transportManager);
        }

        public override IList<TransportManager> Select(TransportChannelListener channelListener)
        {
            bool flag = (base.ListenUri.HostNameType != UriHostNameType.IPv6) && Socket.OSSupportsIPv4;
            bool flag2 = (base.ListenUri.HostNameType != UriHostNameType.IPv4) && Socket.OSSupportsIPv6;
            TcpChannelListener listener = (TcpChannelListener) channelListener;
            if (!this.IsCompatible(listener, flag, flag2))
            {
                return null;
            }
            IList<TransportManager> result = new List<TransportManager>();
            if (flag)
            {
                this.ProcessSelection(listener, IPAddress.Any, UriHostNameType.IPv4, ref this.ipv4TransportManager, result);
            }
            if (flag2)
            {
                this.ProcessSelection(listener, IPAddress.IPv6Any, UriHostNameType.IPv6, ref this.ipv6TransportManager, result);
            }
            return result;
        }

        public bool TeredoEnabled
        {
            get
            {
                return this.teredoEnabled;
            }
        }
    }
}

