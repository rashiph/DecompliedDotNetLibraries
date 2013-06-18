namespace System.ServiceModel.Channels
{
    using System;

    internal class SharedHttpsTransportManager : SharedHttpTransportManager
    {
        private static UriPrefixTable<ITransportManagerRegistration> transportManagerTable = new UriPrefixTable<ITransportManagerRegistration>(true);

        public SharedHttpsTransportManager(Uri listenUri, HttpChannelListener factory) : base(listenUri, factory)
        {
        }

        internal override string Scheme
        {
            get
            {
                return Uri.UriSchemeHttps;
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

