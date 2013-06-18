namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;

    internal abstract class TransportManagerRegistration : ITransportManagerRegistration
    {
        private System.ServiceModel.HostNameComparisonMode hostNameComparisonMode;
        private Uri listenUri;

        protected TransportManagerRegistration(Uri listenUri, System.ServiceModel.HostNameComparisonMode hostNameComparisonMode)
        {
            this.listenUri = listenUri;
            this.hostNameComparisonMode = hostNameComparisonMode;
        }

        public abstract IList<TransportManager> Select(TransportChannelListener factory);

        public System.ServiceModel.HostNameComparisonMode HostNameComparisonMode
        {
            get
            {
                return this.hostNameComparisonMode;
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

