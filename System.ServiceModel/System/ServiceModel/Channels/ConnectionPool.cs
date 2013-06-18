namespace System.ServiceModel.Channels
{
    using System;

    internal abstract class ConnectionPool : IdlingCommunicationPool<string, IConnection>
    {
        private int connectionBufferSize;
        private TimeSpan maxOutputDelay;
        private string name;

        protected ConnectionPool(IConnectionOrientedTransportChannelFactorySettings settings, TimeSpan leaseTimeout) : base(settings.MaxOutboundConnectionsPerEndpoint, settings.IdleTimeout, leaseTimeout)
        {
            this.connectionBufferSize = settings.ConnectionBufferSize;
            this.maxOutputDelay = settings.MaxOutputDelay;
            this.name = settings.ConnectionPoolGroupName;
        }

        protected override void AbortItem(IConnection item)
        {
            item.Abort();
        }

        protected override void CloseItem(IConnection item, TimeSpan timeout)
        {
            item.Close(timeout, false);
        }

        protected override void CloseItemAsync(IConnection item, TimeSpan timeout)
        {
            item.Close(timeout, true);
        }

        public virtual bool IsCompatible(IConnectionOrientedTransportChannelFactorySettings settings)
        {
            return ((((this.name == settings.ConnectionPoolGroupName) && (this.connectionBufferSize == settings.ConnectionBufferSize)) && ((base.MaxIdleConnectionPoolCount == settings.MaxOutboundConnectionsPerEndpoint) && (base.IdleTimeout == settings.IdleTimeout))) && (this.maxOutputDelay == settings.MaxOutputDelay));
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }
    }
}

