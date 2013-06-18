namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [DataContract(Name="RecoverCoordinator11")]
    internal class RecoverCoordinatorRecordSchema11 : RecoverCoordinatorRecordSchema
    {
        [DataMember(Name="CoordinatorService")]
        private EndpointAddress10 coordinatorService;
        private const string id = "http://schemas.microsoft.com/2006/08/ServiceModel/RecoverCoordinator11TraceRecord";

        public RecoverCoordinatorRecordSchema11(string transactionId, EndpointAddress coordinatorService) : base(transactionId)
        {
            base.schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/RecoverCoordinator11TraceRecord";
            if (coordinatorService != null)
            {
                this.coordinatorService = EndpointAddress10.FromEndpointAddress(coordinatorService);
            }
        }
    }
}

