namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;

    internal class UniqueTransportManagerRegistration : TransportManagerRegistration
    {
        private List<TransportManager> list;

        public UniqueTransportManagerRegistration(TransportManager uniqueManager, Uri listenUri, HostNameComparisonMode hostNameComparisonMode) : base(listenUri, hostNameComparisonMode)
        {
            this.list = new List<TransportManager>();
            this.list.Add(uniqueManager);
        }

        public override IList<TransportManager> Select(TransportChannelListener channelListener)
        {
            return this.list;
        }
    }
}

