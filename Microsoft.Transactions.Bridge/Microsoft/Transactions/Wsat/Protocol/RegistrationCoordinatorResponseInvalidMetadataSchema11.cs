namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Wsat.Messaging;
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [DataContract(Name="RegistrationCoordinatorResponseInvalidMetadata11")]
    internal class RegistrationCoordinatorResponseInvalidMetadataSchema11 : RegistrationCoordinatorResponseInvalidMetadataSchema
    {
        [DataMember(Name="CoordinatorService", IsRequired=true)]
        private EndpointAddress10 coordinatorService;
        private const string id = "http://schemas.microsoft.com/2006/08/ServiceModel/RegistrationCoordinatorResponseInvalidMetadata11TraceRecord";

        public RegistrationCoordinatorResponseInvalidMetadataSchema11(CoordinationContext context, ControlProtocol protocol, EndpointAddress coordinatorService) : base(context, protocol)
        {
            base.schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/RegistrationCoordinatorResponseInvalidMetadata11TraceRecord";
            if (coordinatorService != null)
            {
                this.coordinatorService = EndpointAddress10.FromEndpointAddress(coordinatorService);
            }
        }
    }
}

