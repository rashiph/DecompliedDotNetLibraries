namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Wsat.Messaging;
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [DataContract(Name="RegistrationCoordinatorResponseInvalidMetadata10")]
    internal class RegistrationCoordinatorResponseInvalidMetadataSchema10 : RegistrationCoordinatorResponseInvalidMetadataSchema
    {
        [DataMember(Name="CoordinatorService", IsRequired=true)]
        private EndpointAddressAugust2004 coordinatorService;
        private const string id = "http://schemas.microsoft.com/2006/08/ServiceModel/RegistrationCoordinatorResponseInvalidMetadataTraceRecord";

        public RegistrationCoordinatorResponseInvalidMetadataSchema10(CoordinationContext context, ControlProtocol protocol, EndpointAddress coordinatorService) : base(context, protocol)
        {
            base.schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/RegistrationCoordinatorResponseInvalidMetadataTraceRecord";
            if (coordinatorService != null)
            {
                this.coordinatorService = EndpointAddressAugust2004.FromEndpointAddress(coordinatorService);
            }
        }
    }
}

