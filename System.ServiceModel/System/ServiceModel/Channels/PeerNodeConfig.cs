namespace System.ServiceModel.Channels
{
    using System;
    using System.Net;
    using System.ServiceModel;
    using System.Xml;

    internal class PeerNodeConfig
    {
        private int connectTimeout;
        private System.ServiceModel.Channels.MessageEncoder encoder;
        private int idealNeighbors;
        private PeerNodeAddress listenAddress;
        private IPAddress listenIPAddress;
        private Uri listenUri;
        private int maintainerInterval;
        private TimeSpan maintainerRetryInterval;
        private TimeSpan maintainerTimeout;
        private long maxBufferPoolSize;
        private int maxConcurrentSessions = 0x40;
        private int maxIncomingConcurrentCalls = 0x80;
        private int maxNeighbors;
        private long maxReceivedMessageSize;
        private int maxReferralCacheSize;
        private int maxReferrals;
        private int maxResolveAddresses;
        private int maxSendQueueSize = 0x80;
        private string meshId;
        private PeerMessagePropagationFilter messagePropagationFilter;
        private int minNeighbors;
        private ulong nodeId;
        private int port;
        private XmlDictionaryReaderQuotas readerQuotas = new XmlDictionaryReaderQuotas();
        private PeerResolver resolver;
        private PeerSecurityManager securityManager;
        private TimeSpan unregisterTimeout;

        public PeerNodeConfig(string meshId, ulong nodeId, PeerResolver resolver, PeerMessagePropagationFilter messagePropagationFilter, System.ServiceModel.Channels.MessageEncoder encoder, Uri listenUri, IPAddress listenIPAddress, int port, long maxReceivedMessageSize, int minNeighbors, int idealNeighbors, int maxNeighbors, int maxReferrals, int connectTimeout, int maintainerInterval, PeerSecurityManager securityManager, XmlDictionaryReaderQuotas readerQuotas, long maxBufferPool, int maxSendQueueSize, int maxReceiveQueueSize)
        {
            this.connectTimeout = connectTimeout;
            this.listenIPAddress = listenIPAddress;
            this.listenUri = listenUri;
            this.maxReceivedMessageSize = maxReceivedMessageSize;
            this.minNeighbors = minNeighbors;
            this.idealNeighbors = idealNeighbors;
            this.maxNeighbors = maxNeighbors;
            this.maxReferrals = maxReferrals;
            this.maxReferralCacheSize = 50;
            this.maxResolveAddresses = 3;
            this.meshId = meshId;
            this.encoder = encoder;
            this.messagePropagationFilter = messagePropagationFilter;
            this.nodeId = nodeId;
            this.port = port;
            this.resolver = resolver;
            this.maintainerInterval = maintainerInterval;
            this.maintainerRetryInterval = new TimeSpan(0x5f5e100L);
            this.maintainerTimeout = new TimeSpan(0x47868c00L);
            this.unregisterTimeout = new TimeSpan(0x47868c00L);
            this.securityManager = securityManager;
            readerQuotas.CopyTo(this.readerQuotas);
            this.maxBufferPoolSize = maxBufferPool;
            this.maxIncomingConcurrentCalls = maxReceiveQueueSize;
            this.maxSendQueueSize = maxSendQueueSize;
        }

        private static Uri BuildUri(string host, int port, Guid guid)
        {
            UriBuilder uriBuilder = new UriBuilder {
                Host = host
            };
            if (port > 0)
            {
                uriBuilder.Port = port;
            }
            uriBuilder.Path = "PeerChannelEndpoints" + '/' + guid;
            uriBuilder.Scheme = Uri.UriSchemeNetTcp;
            TcpChannelListener.FixIpv6Hostname(uriBuilder, uriBuilder.Uri);
            return uriBuilder.Uri;
        }

        public PeerNodeAddress GetListenAddress(bool maskScopeId)
        {
            PeerNodeAddress listenAddress = this.listenAddress;
            return new PeerNodeAddress(listenAddress.EndpointAddress, PeerIPHelper.CloneAddresses(listenAddress.IPAddresses, maskScopeId));
        }

        public Uri GetMeshUri()
        {
            UriBuilder builder = new UriBuilder {
                Host = this.meshId,
                Scheme = "net.p2p"
            };
            return builder.Uri;
        }

        public Uri GetSelfUri()
        {
            Guid guid = Guid.NewGuid();
            if (this.listenIPAddress == null)
            {
                return BuildUri(DnsCache.MachineName, this.port, guid);
            }
            return BuildUri(this.listenIPAddress.ToString(), this.port, guid);
        }

        public void SetListenAddress(PeerNodeAddress address)
        {
            this.listenAddress = address;
        }

        public int ConnectTimeout
        {
            get
            {
                return this.connectTimeout;
            }
        }

        public int IdealNeighbors
        {
            get
            {
                return this.idealNeighbors;
            }
        }

        public int ListenerPort
        {
            get
            {
                return this.listenAddress.EndpointAddress.Uri.Port;
            }
        }

        public IPAddress ListenIPAddress
        {
            get
            {
                return this.listenIPAddress;
            }
        }

        public Uri ListenUri
        {
            get
            {
                return this.listenUri;
            }
        }

        public int MaintainerInterval
        {
            get
            {
                return this.maintainerInterval;
            }
        }

        public TimeSpan MaintainerRetryInterval
        {
            get
            {
                return this.maintainerRetryInterval;
            }
        }

        public TimeSpan MaintainerTimeout
        {
            get
            {
                return this.maintainerTimeout;
            }
        }

        public long MaxBufferPoolSize
        {
            get
            {
                return this.maxBufferPoolSize;
            }
        }

        public int MaxConcurrentSessions
        {
            get
            {
                return this.maxConcurrentSessions;
            }
        }

        public int MaxNeighbors
        {
            get
            {
                return this.maxNeighbors;
            }
        }

        public int MaxPendingIncomingCalls
        {
            get
            {
                return this.maxIncomingConcurrentCalls;
            }
        }

        public int MaxPendingOutgoingCalls
        {
            get
            {
                return this.maxSendQueueSize;
            }
        }

        public long MaxReceivedMessageSize
        {
            get
            {
                return this.maxReceivedMessageSize;
            }
        }

        public int MaxReferralCacheSize
        {
            get
            {
                return this.maxReferralCacheSize;
            }
        }

        public int MaxReferrals
        {
            get
            {
                return this.maxReferrals;
            }
        }

        public int MaxResolveAddresses
        {
            get
            {
                return this.maxResolveAddresses;
            }
        }

        public string MeshId
        {
            get
            {
                return this.meshId;
            }
        }

        public System.ServiceModel.Channels.MessageEncoder MessageEncoder
        {
            get
            {
                return this.encoder;
            }
        }

        public PeerMessagePropagationFilter MessagePropagationFilter
        {
            get
            {
                return this.messagePropagationFilter;
            }
        }

        public int MinNeighbors
        {
            get
            {
                return this.minNeighbors;
            }
        }

        public ulong NodeId
        {
            get
            {
                return this.nodeId;
            }
        }

        public int Port
        {
            get
            {
                return this.port;
            }
        }

        public XmlDictionaryReaderQuotas ReaderQuotas
        {
            get
            {
                return this.readerQuotas;
            }
        }

        public PeerResolver Resolver
        {
            get
            {
                return this.resolver;
            }
        }

        internal PeerSecurityManager SecurityManager
        {
            get
            {
                return this.securityManager;
            }
        }

        public TimeSpan UnregisterTimeout
        {
            get
            {
                return this.unregisterTimeout;
            }
        }
    }
}

