namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;

    internal abstract class ConnectionOrientedTransportChannelFactory<TChannel> : TransportChannelFactory<TChannel>, IConnectionOrientedTransportChannelFactorySettings, IConnectionOrientedTransportFactorySettings, ITransportFactorySettings, IDefaultCommunicationTimeouts, IConnectionOrientedConnectionSettings
    {
        private int connectionBufferSize;
        private IConnectionInitiator connectionInitiator;
        private ConnectionPool connectionPool;
        private string connectionPoolGroupName;
        private bool exposeConnectionProperty;
        private bool flowIdentity;
        private TimeSpan idleTimeout;
        private int maxBufferSize;
        private int maxOutboundConnectionsPerEndpoint;
        private TimeSpan maxOutputDelay;
        private ISecurityCapabilities securityCapabilities;
        private System.ServiceModel.TransferMode transferMode;
        private StreamUpgradeProvider upgrade;

        internal ConnectionOrientedTransportChannelFactory(ConnectionOrientedTransportBindingElement bindingElement, BindingContext context, string connectionPoolGroupName, TimeSpan idleTimeout, int maxOutboundConnectionsPerEndpoint, bool supportsImpersonationDuringAsyncOpen) : base(bindingElement, context)
        {
            if ((bindingElement.TransferMode == System.ServiceModel.TransferMode.Buffered) && (bindingElement.MaxReceivedMessageSize > 0x7fffffffL))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("bindingElement.MaxReceivedMessageSize", System.ServiceModel.SR.GetString("MaxReceivedMessageSizeMustBeInIntegerRange")));
            }
            this.connectionBufferSize = bindingElement.ConnectionBufferSize;
            this.connectionPoolGroupName = connectionPoolGroupName;
            this.exposeConnectionProperty = bindingElement.ExposeConnectionProperty;
            this.idleTimeout = idleTimeout;
            this.maxBufferSize = bindingElement.MaxBufferSize;
            this.maxOutboundConnectionsPerEndpoint = maxOutboundConnectionsPerEndpoint;
            this.maxOutputDelay = bindingElement.MaxOutputDelay;
            this.transferMode = bindingElement.TransferMode;
            Collection<StreamUpgradeBindingElement> collection = context.BindingParameters.FindAll<StreamUpgradeBindingElement>();
            if (collection.Count > 1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MultipleStreamUpgradeProvidersInParameters")));
            }
            if ((collection.Count == 1) && this.SupportsUpgrade(collection[0]))
            {
                this.upgrade = collection[0].BuildClientStreamUpgradeProvider(context);
                context.BindingParameters.Remove<StreamUpgradeBindingElement>();
                this.securityCapabilities = collection[0].GetProperty<ISecurityCapabilities>(context);
                this.flowIdentity = supportsImpersonationDuringAsyncOpen;
            }
        }

        internal abstract IConnectionInitiator GetConnectionInitiator();
        internal abstract ConnectionPool GetConnectionPool();
        public override T GetProperty<T>() where T: class
        {
            if (typeof(T) == typeof(ISecurityCapabilities))
            {
                return (T) this.securityCapabilities;
            }
            T property = base.GetProperty<T>();
            if ((property == null) && (this.upgrade != null))
            {
                property = this.upgrade.GetProperty<T>();
            }
            return property;
        }

        private bool GetUpgradeAndConnectionPool(out StreamUpgradeProvider upgradeCopy, out ConnectionPool poolCopy)
        {
            if ((this.upgrade != null) || (this.connectionPool != null))
            {
                lock (base.ThisLock)
                {
                    if ((this.upgrade != null) || (this.connectionPool != null))
                    {
                        upgradeCopy = this.upgrade;
                        poolCopy = this.connectionPool;
                        this.upgrade = null;
                        this.connectionPool = null;
                        return true;
                    }
                }
            }
            upgradeCopy = null;
            poolCopy = null;
            return false;
        }

        protected override void OnAbort()
        {
            StreamUpgradeProvider provider;
            ConnectionPool pool;
            if (this.GetUpgradeAndConnectionPool(out provider, out pool))
            {
                if (pool != null)
                {
                    this.ReleaseConnectionPool(pool, TimeSpan.Zero);
                }
                if (provider != null)
                {
                    provider.Abort();
                }
            }
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseAsyncResult<TChannel>((ConnectionOrientedTransportChannelFactory<TChannel>) this, timeout, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OpenAsyncResult<TChannel>(this.Upgrade, timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            StreamUpgradeProvider provider;
            ConnectionPool pool;
            if (this.GetUpgradeAndConnectionPool(out provider, out pool))
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                if (pool != null)
                {
                    this.ReleaseConnectionPool(pool, helper.RemainingTime());
                }
                if (provider != null)
                {
                    provider.Close(helper.RemainingTime());
                }
            }
        }

        protected override TChannel OnCreateChannel(EndpointAddress address, Uri via)
        {
            base.ValidateScheme(via);
            if (this.TransferMode == System.ServiceModel.TransferMode.Buffered)
            {
                return (TChannel) new ClientFramingDuplexSessionChannel(this, this, address, via, this.ConnectionInitiator, this.connectionPool, this.exposeConnectionProperty, this.flowIdentity);
            }
            return (TChannel) new StreamedFramingRequestChannel(this, this, address, via, this.ConnectionInitiator, this.connectionPool);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CloseAsyncResult<TChannel>.End(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            OpenAsyncResult<TChannel>.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            StreamUpgradeProvider upgrade = this.Upgrade;
            if (upgrade != null)
            {
                upgrade.Open(timeout);
            }
        }

        protected override void OnOpening()
        {
            base.OnOpening();
            this.connectionPool = this.GetConnectionPool();
        }

        internal abstract void ReleaseConnectionPool(ConnectionPool pool, TimeSpan timeout);
        protected virtual bool SupportsUpgrade(StreamUpgradeBindingElement upgradeBindingElement)
        {
            return true;
        }

        public int ConnectionBufferSize
        {
            get
            {
                return this.connectionBufferSize;
            }
        }

        internal IConnectionInitiator ConnectionInitiator
        {
            get
            {
                if (this.connectionInitiator == null)
                {
                    lock (base.ThisLock)
                    {
                        if (this.connectionInitiator == null)
                        {
                            this.connectionInitiator = this.GetConnectionInitiator();
                            if (DiagnosticUtility.ShouldUseActivity)
                            {
                                this.connectionInitiator = new TracingConnectionInitiator(this.connectionInitiator, (ServiceModelActivity.Current != null) && (ServiceModelActivity.Current.ActivityType == ActivityType.OpenClient));
                            }
                        }
                    }
                }
                return this.connectionInitiator;
            }
        }

        public string ConnectionPoolGroupName
        {
            get
            {
                return this.connectionPoolGroupName;
            }
        }

        public TimeSpan IdleTimeout
        {
            get
            {
                return this.idleTimeout;
            }
        }

        public int MaxBufferSize
        {
            get
            {
                return this.maxBufferSize;
            }
        }

        public int MaxOutboundConnectionsPerEndpoint
        {
            get
            {
                return this.maxOutboundConnectionsPerEndpoint;
            }
        }

        public TimeSpan MaxOutputDelay
        {
            get
            {
                return this.maxOutputDelay;
            }
        }

        ServiceSecurityAuditBehavior IConnectionOrientedTransportFactorySettings.AuditBehavior
        {
            get
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("SecurityAuditNotSupportedOnChannelFactory")));
            }
        }

        int IConnectionOrientedTransportFactorySettings.MaxBufferSize
        {
            get
            {
                return this.MaxBufferSize;
            }
        }

        System.ServiceModel.TransferMode IConnectionOrientedTransportFactorySettings.TransferMode
        {
            get
            {
                return this.TransferMode;
            }
        }

        StreamUpgradeProvider IConnectionOrientedTransportFactorySettings.Upgrade
        {
            get
            {
                return this.Upgrade;
            }
        }

        public System.ServiceModel.TransferMode TransferMode
        {
            get
            {
                return this.transferMode;
            }
        }

        public StreamUpgradeProvider Upgrade
        {
            get
            {
                StreamUpgradeProvider upgrade = this.upgrade;
                base.ThrowIfDisposed();
                return upgrade;
            }
        }

        private class CloseAsyncResult : AsyncResult
        {
            private ConnectionPool connectionPool;
            private static AsyncCallback onCloseComplete;
            private static Action<object> onReleaseConnectionPoolScheduled;
            private ConnectionOrientedTransportChannelFactory<TChannel> parent;
            private TimeoutHelper timeoutHelper;
            private StreamUpgradeProvider upgradeProvider;

            static CloseAsyncResult()
            {
                ConnectionOrientedTransportChannelFactory<TChannel>.CloseAsyncResult.onCloseComplete = Fx.ThunkCallback(new AsyncCallback(ConnectionOrientedTransportChannelFactory<TChannel>.CloseAsyncResult.OnCloseComplete));
            }

            public CloseAsyncResult(ConnectionOrientedTransportChannelFactory<TChannel> parent, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.parent = parent;
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.parent.GetUpgradeAndConnectionPool(out this.upgradeProvider, out this.connectionPool);
                if (this.connectionPool == null)
                {
                    if (this.HandleReleaseConnectionPoolComplete())
                    {
                        base.Complete(true);
                    }
                }
                else
                {
                    if (ConnectionOrientedTransportChannelFactory<TChannel>.CloseAsyncResult.onReleaseConnectionPoolScheduled == null)
                    {
                        ConnectionOrientedTransportChannelFactory<TChannel>.CloseAsyncResult.onReleaseConnectionPoolScheduled = new Action<object>(ConnectionOrientedTransportChannelFactory<TChannel>.CloseAsyncResult.OnReleaseConnectionPoolScheduled);
                    }
                    ActionItem.Schedule(ConnectionOrientedTransportChannelFactory<TChannel>.CloseAsyncResult.onReleaseConnectionPoolScheduled, this);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<ConnectionOrientedTransportChannelFactory<TChannel>.CloseAsyncResult>(result);
            }

            private bool HandleReleaseConnectionPoolComplete()
            {
                if (this.upgradeProvider == null)
                {
                    return true;
                }
                IAsyncResult result = this.upgradeProvider.BeginClose(this.timeoutHelper.RemainingTime(), ConnectionOrientedTransportChannelFactory<TChannel>.CloseAsyncResult.onCloseComplete, this);
                if (result.CompletedSynchronously)
                {
                    this.upgradeProvider.EndClose(result);
                    return true;
                }
                return false;
            }

            private static void OnCloseComplete(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ConnectionOrientedTransportChannelFactory<TChannel>.CloseAsyncResult asyncState = (ConnectionOrientedTransportChannelFactory<TChannel>.CloseAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        asyncState.upgradeProvider.EndClose(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    asyncState.Complete(false, exception);
                }
            }

            private bool OnReleaseConnectionPoolScheduled()
            {
                this.parent.ReleaseConnectionPool(this.connectionPool, this.timeoutHelper.RemainingTime());
                return this.HandleReleaseConnectionPoolComplete();
            }

            private static void OnReleaseConnectionPoolScheduled(object state)
            {
                bool flag;
                ConnectionOrientedTransportChannelFactory<TChannel>.CloseAsyncResult result = (ConnectionOrientedTransportChannelFactory<TChannel>.CloseAsyncResult) state;
                Exception exception = null;
                try
                {
                    flag = result.OnReleaseConnectionPoolScheduled();
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    flag = true;
                    exception = exception2;
                }
                if (flag)
                {
                    result.Complete(false, exception);
                }
            }
        }

        private class OpenAsyncResult : AsyncResult
        {
            private ICommunicationObject communicationObject;
            private static AsyncCallback onOpenComplete;

            static OpenAsyncResult()
            {
                ConnectionOrientedTransportChannelFactory<TChannel>.OpenAsyncResult.onOpenComplete = Fx.ThunkCallback(new AsyncCallback(ConnectionOrientedTransportChannelFactory<TChannel>.OpenAsyncResult.OnOpenComplete));
            }

            public OpenAsyncResult(ICommunicationObject communicationObject, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.communicationObject = communicationObject;
                if (this.communicationObject == null)
                {
                    base.Complete(true);
                }
                else
                {
                    IAsyncResult result = this.communicationObject.BeginOpen(timeout, ConnectionOrientedTransportChannelFactory<TChannel>.OpenAsyncResult.onOpenComplete, this);
                    if (result.CompletedSynchronously)
                    {
                        this.communicationObject.EndOpen(result);
                        base.Complete(true);
                    }
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<ConnectionOrientedTransportChannelFactory<TChannel>.OpenAsyncResult>(result);
            }

            private static void OnOpenComplete(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ConnectionOrientedTransportChannelFactory<TChannel>.OpenAsyncResult asyncState = (ConnectionOrientedTransportChannelFactory<TChannel>.OpenAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        asyncState.communicationObject.EndOpen(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    asyncState.Complete(false, exception);
                }
            }
        }
    }
}

