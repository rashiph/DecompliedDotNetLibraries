namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Wsat.Messaging;
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [DataContract(Name="RegisterFailure10")]
    internal class RegisterFailureRecordSchema10 : RegisterFailureRecordSchema
    {
        private const string id = "http://schemas.microsoft.com/2006/08/ServiceModel/RegisterFailureTraceRecord";
        [DataMember(Name="ProtocolService", IsRequired=true)]
        private EndpointAddressAugust2004 protocolService;

        public RegisterFailureRecordSchema10(string transactionId, ControlProtocol protocol, EndpointAddress protocolService, string reason) : base(transactionId, protocol, reason)
        {
            base.schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/RegisterFailureTraceRecord";
            if (protocolService != null)
            {
                this.protocolService = EndpointAddressAugust2004.FromEndpointAddress(protocolService);
            }
        }
    }
}

