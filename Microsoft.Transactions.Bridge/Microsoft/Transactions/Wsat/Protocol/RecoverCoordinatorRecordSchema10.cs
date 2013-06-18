namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [DataContract(Name="RecoverCoordinator10")]
    internal class RecoverCoordinatorRecordSchema10 : RecoverCoordinatorRecordSchema
    {
        [DataMember(Name="CoordinatorService")]
        private EndpointAddressAugust2004 coordinatorService;
        private const string id = "http://schemas.microsoft.com/2006/08/ServiceModel/RecoverCoordinatorTraceRecord";

        public RecoverCoordinatorRecordSchema10(string transactionId, EndpointAddress coordinatorService) : base(transactionId)
        {
            base.schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/RecoverCoordinatorTraceRecord";
            if (coordinatorService != null)
            {
                this.coordinatorService = EndpointAddressAugust2004.FromEndpointAddress(coordinatorService);
            }
        }
    }
}

