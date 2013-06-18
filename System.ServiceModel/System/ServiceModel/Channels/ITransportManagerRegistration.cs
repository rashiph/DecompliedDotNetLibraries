namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;

    internal interface ITransportManagerRegistration
    {
        IList<TransportManager> Select(TransportChannelListener factory);

        System.ServiceModel.HostNameComparisonMode HostNameComparisonMode { get; }

        Uri ListenUri { get; }
    }
}

