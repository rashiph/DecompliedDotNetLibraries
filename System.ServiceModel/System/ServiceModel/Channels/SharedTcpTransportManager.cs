namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;

    internal class SharedTcpTransportManager : TcpTransportManager, ITransportManagerRegistration
    {
        private ConnectionDemuxer connectionDemuxer;
        private bool demuxerCreated;
        private System.ServiceModel.HostNameComparisonMode hostNameComparisonMode;
        private SharedConnectionListener listener;
        private Uri listenUri;
        private Func<Uri, int> onDuplicatedViaCallback;
        private int queueId;
        private Guid token;

        protected SharedTcpTransportManager(Uri listenUri)
        {
            this.listenUri = listenUri;
        }

        public SharedTcpTransportManager(Uri listenUri, TcpChannelListener channelListener)
        {
            this.HostNameComparisonMode = channelListener.HostNameComparisonMode;
            this.listenUri = listenUri;
            base.ApplyListenerSettings(channelListener);
        }

        protected void CleanUp(bool aborting, TimeSpan timeout)
        {
            lock (base.ThisLock)
            {
                if (this.listener != null)
                {
                    if (!aborting)
                    {
                        this.listener.Stop(timeout);
                    }
                    else
                    {
                        this.listener.Abort();
                    }
                    this.listener = null;
                }
                if (this.connectionDemuxer != null)
                {
                    this.connectionDemuxer.Dispose();
                }
                this.demuxerCreated = false;
            }
        }

        private void CreateConnectionDemuxer()
        {
            IConnectionListener listener = new BufferedConnectionListener(this.listener, base.MaxOutputDelay, base.ConnectionBufferSize);
            if (DiagnosticUtility.ShouldUseActivity)
            {
                listener = new TracingConnectionListener(listener, this.ListenUri);
            }
            this.connectionDemuxer = new ConnectionDemuxer(listener, base.MaxPendingAccepts, base.MaxPendingConnections, base.ChannelInitializationTimeout, base.IdleTimeout, base.MaxPooledConnections, new TransportSettingsCallback(this.OnGetTransportFactorySettings), new SingletonPreambleDemuxCallback(this.OnGetSingletonMessageHandler), new ServerSessionPreambleDemuxCallback(this.OnHandleServerSessionPreamble), new System.ServiceModel.Channels.ErrorCallback(this.OnDemuxerError));
            this.connectionDemuxer.StartDemuxing(this.GetOnViaCallback());
        }

        protected virtual Action<Uri> GetOnViaCallback()
        {
            return null;
        }

        protected override bool IsCompatible(TcpChannelListener channelListener)
        {
            if ((channelListener.HostedVirtualPath == null) && !channelListener.PortSharingEnabled)
            {
                return false;
            }
            return base.IsCompatible(channelListener);
        }

        internal override void OnAbort()
        {
            this.CleanUp(true, TimeSpan.Zero);
            this.Unregister();
            base.OnAbort();
        }

        internal override void OnClose(TimeSpan timeout)
        {
            this.CleanUp(false, timeout);
            this.Unregister();
        }

        private int OnDuplicatedVia(Uri via)
        {
            Action<Uri> onViaCallback = this.GetOnViaCallback();
            if (onViaCallback != null)
            {
                onViaCallback(via);
            }
            if (!this.demuxerCreated)
            {
                lock (base.ThisLock)
                {
                    if (this.listener == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationObjectAbortedException(System.ServiceModel.SR.GetString("Sharing_ListenerProxyStopped")));
                    }
                    if (!this.demuxerCreated)
                    {
                        this.CreateConnectionDemuxer();
                        this.demuxerCreated = true;
                    }
                }
            }
            return base.ConnectionBufferSize;
        }

        internal override void OnOpen()
        {
            this.OnOpenInternal(0, Guid.Empty);
        }

        internal void OnOpenInternal(int queueId, Guid token)
        {
            lock (base.ThisLock)
            {
                this.queueId = queueId;
                this.token = token;
                BaseUriWithWildcard baseAddress = new BaseUriWithWildcard(this.ListenUri, this.HostNameComparisonMode);
                if (this.onDuplicatedViaCallback == null)
                {
                    this.onDuplicatedViaCallback = new Func<Uri, int>(this.OnDuplicatedVia);
                }
                this.listener = new SharedConnectionListener(baseAddress, queueId, token, this.onDuplicatedViaCallback);
            }
        }

        protected virtual void OnSelecting(TcpChannelListener channelListener)
        {
        }

        IList<TransportManager> ITransportManagerRegistration.Select(TransportChannelListener channelListener)
        {
            if (!channelListener.IsScopeIdCompatible(this.hostNameComparisonMode, this.listenUri))
            {
                return null;
            }
            this.OnSelecting((TcpChannelListener) channelListener);
            IList<TransportManager> list = null;
            if (this.IsCompatible((TcpChannelListener) channelListener))
            {
                list = new List<TransportManager> {
                    this
                };
            }
            return list;
        }

        private void Unregister()
        {
            TcpChannelListener.StaticTransportManagerTable.UnregisterUri(this.ListenUri, this.HostNameComparisonMode);
        }

        public System.ServiceModel.HostNameComparisonMode HostNameComparisonMode
        {
            get
            {
                return this.hostNameComparisonMode;
            }
            set
            {
                HostNameComparisonModeHelper.Validate(value);
                lock (base.ThisLock)
                {
                    base.ThrowIfOpen();
                    this.hostNameComparisonMode = value;
                }
            }
        }

        public Uri ListenUri
        {
            get
            {
                return this.listenUri;
            }
        }
    }
}

