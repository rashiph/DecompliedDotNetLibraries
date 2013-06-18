namespace System.ServiceModel.Channels
{
    using System;
    using System.Globalization;
    using System.ServiceModel;

    internal class TcpConnectionPoolRegistry : ConnectionPoolRegistry
    {
        protected override ConnectionPool CreatePool(IConnectionOrientedTransportChannelFactorySettings settings)
        {
            return new TcpConnectionPool((ITcpChannelFactorySettings) settings);
        }

        private class TcpConnectionPool : ConnectionPool
        {
            public TcpConnectionPool(ITcpChannelFactorySettings settings) : base(settings, settings.LeaseTimeout)
            {
            }

            protected override string GetPoolKey(EndpointAddress address, Uri via)
            {
                int port = via.Port;
                if (port == -1)
                {
                    port = 0x328;
                }
                string str = via.DnsSafeHost.ToUpperInvariant();
                return string.Format(CultureInfo.InvariantCulture, "[{0}, {1}]", new object[] { str, port });
            }

            public override bool IsCompatible(IConnectionOrientedTransportChannelFactorySettings settings)
            {
                ITcpChannelFactorySettings settings2 = (ITcpChannelFactorySettings) settings;
                return ((base.LeaseTimeout == settings2.LeaseTimeout) && base.IsCompatible(settings));
            }
        }
    }
}

