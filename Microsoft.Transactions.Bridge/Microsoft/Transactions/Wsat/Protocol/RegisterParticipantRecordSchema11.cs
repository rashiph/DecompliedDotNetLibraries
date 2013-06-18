namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Wsat.Messaging;
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [DataContract(Name="RegisterParticipant11")]
    internal class RegisterParticipantRecordSchema11 : RegisterParticipantRecordSchema
    {
        private const string id = "http://schemas.microsoft.com/2006/08/ServiceModel/RegisterParticipant11TraceRecord";
        [DataMember(Name="ParticipantService", IsRequired=true)]
        private EndpointAddress10 participantService;

        public RegisterParticipantRecordSchema11(string transactionId, Guid enlistmentId, ControlProtocol protocol, EndpointAddress participantService) : base(transactionId, enlistmentId, protocol)
        {
            base.schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/RegisterParticipant11TraceRecord";
            if (participantService != null)
            {
                this.participantService = EndpointAddress10.FromEndpointAddress(participantService);
            }
        }
    }
}

