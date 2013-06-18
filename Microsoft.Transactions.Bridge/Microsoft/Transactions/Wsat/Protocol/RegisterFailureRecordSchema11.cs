namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Wsat.Messaging;
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [DataContract(Name="RegisterFailure11")]
    internal class RegisterFailureRecordSchema11 : RegisterFailureRecordSchema
    {
        private const string id = "http://schemas.microsoft.com/2006/08/ServiceModel/RegisterFailure11TraceRecord";
        [DataMember(Name="ProtocolService", IsRequired=true)]
        private EndpointAddress10 protocolService;

        public RegisterFailureRecordSchema11(string transactionId, ControlProtocol protocol, EndpointAddress protocolService, string reason) : base(transactionId, protocol, reason)
        {
            base.schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/RegisterFailure11TraceRecord";
            if (protocolService != null)
            {
                this.protocolService = EndpointAddress10.FromEndpointAddress(protocolService);
            }
        }
    }
}

