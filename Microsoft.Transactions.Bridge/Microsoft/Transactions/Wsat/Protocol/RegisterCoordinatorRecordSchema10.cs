namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Wsat.Messaging;
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [DataContract(Name="RegisterCoordinator10")]
    internal class RegisterCoordinatorRecordSchema10 : RegisterCoordinatorRecordSchema
    {
        [DataMember(Name="CoordinatorService", IsRequired=true)]
        private EndpointAddressAugust2004 coordinatorService;
        private const string id = "http://schemas.microsoft.com/2006/08/ServiceModel/RegisterCoordinatorTraceRecord";

        public RegisterCoordinatorRecordSchema10(CoordinationContext context, ControlProtocol protocol, EndpointAddress coordinatorService) : base(context, protocol)
        {
            base.schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/RegisterCoordinatorTraceRecord";
            if (coordinatorService != null)
            {
                this.coordinatorService = EndpointAddressAugust2004.FromEndpointAddress(coordinatorService);
            }
        }
    }
}

