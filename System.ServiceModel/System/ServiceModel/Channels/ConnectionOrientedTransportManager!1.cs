namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal abstract class ConnectionOrientedTransportManager<TChannelListener> : TransportManager where TChannelListener: ConnectionOrientedTransportChannelListener
    {
        private UriPrefixTable<TChannelListener> addressTable;
        private TimeSpan channelInitializationTimeout;
        private int connectionBufferSize;
        private TimeSpan idleTimeout;
        private TimeSpan maxOutputDelay;
        private int maxPendingAccepts;
        private int maxPendingConnections;
        private int maxPooledConnections;
        private Action messageReceivedCallback;

        protected ConnectionOrientedTransportManager()
        {
            this.addressTable = new UriPrefixTable<TChannelListener>();
        }

        internal void ApplyListenerSettings(IConnectionOrientedListenerSettings listenerSettings)
        {
            this.connectionBufferSize = listenerSettings.ConnectionBufferSize;
            this.channelInitializationTimeout = listenerSettings.ChannelInitializationTimeout;
            this.maxPendingConnections = listenerSettings.MaxPendingConnections;
            this.maxOutputDelay = listenerSettings.MaxOutputDelay;
            this.maxPendingAccepts = listenerSettings.MaxPendingAccepts;
            this.idleTimeout = listenerSettings.IdleTimeout;
            this.maxPooledConnections = listenerSettings.MaxPooledConnections;
        }

        private TChannelListener GetChannelListener(Uri via)
        {
            TChannelListener item = default(TChannelListener);
            if (!this.AddressTable.TryLookupUri(via, HostNameComparisonMode.StrongWildcard, out item))
            {
                if (this.AddressTable.TryLookupUri(via, HostNameComparisonMode.Exact, out item))
                {
                    return item;
                }
                this.AddressTable.TryLookupUri(via, HostNameComparisonMode.WeakWildcard, out item);
            }
            return item;
        }

        internal bool IsCompatible(ConnectionOrientedTransportChannelListener channelListener)
        {
            return (channelListener.InheritBaseAddressSettings || (((((this.ChannelInitializationTimeout == channelListener.ChannelInitializationTimeout) && (this.ConnectionBufferSize == channelListener.ConnectionBufferSize)) && ((this.MaxPendingConnections == channelListener.MaxPendingConnections) && (this.MaxOutputDelay == channelListener.MaxOutputDelay))) && ((this.MaxPendingAccepts == channelListener.MaxPendingAccepts) && (this.idleTimeout == channelListener.IdleTimeout))) && (this.maxPooledConnections == channelListener.MaxPooledConnections)));
        }

        internal void OnDemuxerError(Exception exception)
        {
            lock (base.ThisLock)
            {
                base.Fault<TChannelListener>(this.AddressTable, exception);
            }
        }

        internal ISingletonChannelListener OnGetSingletonMessageHandler(ServerSingletonPreambleConnectionReader serverSingletonPreambleReader)
        {
            Uri via = serverSingletonPreambleReader.Via;
            TChannelListener channelListener = this.GetChannelListener(via);
            if (channelListener != null)
            {
                if (channelListener is IChannelListener<IReplyChannel>)
                {
                    channelListener.RaiseMessageReceived();
                    return (ISingletonChannelListener) channelListener;
                }
                serverSingletonPreambleReader.SendFault("http://schemas.microsoft.com/ws/2006/05/framing/faults/UnsupportedMode");
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("FramingModeNotSupported", new object[] { FramingMode.Singleton })));
            }
            serverSingletonPreambleReader.SendFault("http://schemas.microsoft.com/ws/2006/05/framing/faults/EndpointNotFound");
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new EndpointNotFoundException(System.ServiceModel.SR.GetString("EndpointNotFound", new object[] { via })));
        }

        internal IConnectionOrientedTransportFactorySettings OnGetTransportFactorySettings(Uri via)
        {
            return this.GetChannelListener(via);
        }

        internal void OnHandleServerSessionPreamble(ServerSessionPreambleConnectionReader serverSessionPreambleReader, ConnectionDemuxer connectionDemuxer)
        {
            Uri via = serverSessionPreambleReader.Via;
            TChannelListener channelListener = this.GetChannelListener(via);
            if (channelListener != null)
            {
                ISessionPreambleHandler handler = channelListener as ISessionPreambleHandler;
                if ((handler != null) && (channelListener is IChannelListener<IDuplexSessionChannel>))
                {
                    handler.HandleServerSessionPreamble(serverSessionPreambleReader, connectionDemuxer);
                    return;
                }
                serverSessionPreambleReader.SendFault("http://schemas.microsoft.com/ws/2006/05/framing/faults/UnsupportedMode");
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(System.ServiceModel.SR.GetString("FramingModeNotSupported", new object[] { FramingMode.Duplex })));
            }
            serverSessionPreambleReader.SendFault("http://schemas.microsoft.com/ws/2006/05/framing/faults/EndpointNotFound");
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new EndpointNotFoundException(System.ServiceModel.SR.GetString("DuplexSessionListenerNotFound", new object[] { via.ToString() })));
        }

        private void OnMessageReceived()
        {
            Action messageReceivedCallback = this.messageReceivedCallback;
            if (messageReceivedCallback != null)
            {
                messageReceivedCallback();
            }
        }

        internal override void Register(TransportChannelListener channelListener)
        {
            this.AddressTable.RegisterUri(channelListener.Uri, channelListener.HostNameComparisonModeInternal, (TChannelListener) channelListener);
            channelListener.SetMessageReceivedCallback(new Action(this.OnMessageReceived));
        }

        internal void SetMessageReceivedCallback(Action messageReceivedCallback)
        {
            this.messageReceivedCallback = messageReceivedCallback;
        }

        internal override void Unregister(TransportChannelListener channelListener)
        {
            TransportManager.EnsureRegistered<TChannelListener>(this.AddressTable, (TChannelListener) channelListener, channelListener.HostNameComparisonModeInternal);
            this.AddressTable.UnregisterUri(channelListener.Uri, channelListener.HostNameComparisonModeInternal);
            channelListener.SetMessageReceivedCallback(null);
        }

        private UriPrefixTable<TChannelListener> AddressTable
        {
            get
            {
                return this.addressTable;
            }
        }

        protected TimeSpan ChannelInitializationTimeout
        {
            get
            {
                return this.channelInitializationTimeout;
            }
        }

        internal int ConnectionBufferSize
        {
            get
            {
                return this.connectionBufferSize;
            }
        }

        internal TimeSpan IdleTimeout
        {
            get
            {
                return this.idleTimeout;
            }
        }

        internal TimeSpan MaxOutputDelay
        {
            get
            {
                return this.maxOutputDelay;
            }
        }

        internal int MaxPendingAccepts
        {
            get
            {
                return this.maxPendingAccepts;
            }
        }

        internal int MaxPendingConnections
        {
            get
            {
                return this.maxPendingConnections;
            }
        }

        internal int MaxPooledConnections
        {
            get
            {
                return this.maxPooledConnections;
            }
        }
    }
}

