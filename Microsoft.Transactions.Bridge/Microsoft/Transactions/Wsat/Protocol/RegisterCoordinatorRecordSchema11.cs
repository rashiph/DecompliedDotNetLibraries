namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Wsat.Messaging;
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [DataContract(Name="RegisterCoordinator11")]
    internal class RegisterCoordinatorRecordSchema11 : RegisterCoordinatorRecordSchema
    {
        [DataMember(Name="CoordinatorService", IsRequired=true)]
        private EndpointAddress10 coordinatorService;
        private const string id = "http://schemas.microsoft.com/2006/08/ServiceModel/RegisterCoordinator11TraceRecord";

        public RegisterCoordinatorRecordSchema11(CoordinationContext context, ControlProtocol protocol, EndpointAddress coordinatorService) : base(context, protocol)
        {
            base.schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/RegisterCoordinator11TraceRecord";
            if (coordinatorService != null)
            {
                this.coordinatorService = EndpointAddress10.FromEndpointAddress(coordinatorService);
            }
        }
    }
}

