namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal interface ICoordinationListener
    {
        EndpointAddress CreateEndpointReference(AddressHeader refParam);
        void Start();
        void Stop();
    }
}

