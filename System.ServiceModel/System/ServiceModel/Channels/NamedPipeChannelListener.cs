namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;

    internal abstract class NamedPipeChannelListener : ConnectionOrientedTransportChannelListener
    {
        private List<SecurityIdentifier> allowedUsers;
        private static UriPrefixTable<ITransportManagerRegistration> transportManagerTable = new UriPrefixTable<ITransportManagerRegistration>();

        protected NamedPipeChannelListener(NamedPipeTransportBindingElement bindingElement, BindingContext context) : base(bindingElement, context)
        {
            base.SetIdleTimeout(bindingElement.ConnectionPoolSettings.IdleTimeout);
            base.SetMaxPooledConnections(bindingElement.ConnectionPoolSettings.MaxOutboundConnectionsPerEndpoint);
        }

        internal override ITransportManagerRegistration CreateTransportManagerRegistration(Uri listenUri)
        {
            return new ExclusiveNamedPipeTransportManager(listenUri, this);
        }

        protected override bool SupportsUpgrade(StreamUpgradeBindingElement upgradeBindingElement)
        {
            return !(upgradeBindingElement is SslStreamSecurityBindingElement);
        }

        internal List<SecurityIdentifier> AllowedUsers
        {
            get
            {
                return this.allowedUsers;
            }
            set
            {
                lock (base.ThisLock)
                {
                    base.ThrowIfDisposedOrImmutable();
                    this.allowedUsers = value;
                }
            }
        }

        public override string Scheme
        {
            get
            {
                return Uri.UriSchemeNetPipe;
            }
        }

        internal static UriPrefixTable<ITransportManagerRegistration> StaticTransportManagerTable
        {
            get
            {
                return transportManagerTable;
            }
        }

        internal override UriPrefixTable<ITransportManagerRegistration> TransportManagerTable
        {
            get
            {
                return transportManagerTable;
            }
        }
    }
}

