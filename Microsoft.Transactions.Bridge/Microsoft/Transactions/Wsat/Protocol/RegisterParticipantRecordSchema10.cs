namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Wsat.Messaging;
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [DataContract(Name="RegisterParticipant10")]
    internal class RegisterParticipantRecordSchema10 : RegisterParticipantRecordSchema
    {
        private const string id = "http://schemas.microsoft.com/2006/08/ServiceModel/RegisterParticipantTraceRecord";
        [DataMember(Name="ParticipantService", IsRequired=true)]
        private EndpointAddressAugust2004 participantService;

        public RegisterParticipantRecordSchema10(string transactionId, Guid enlistmentId, ControlProtocol protocol, EndpointAddress participantService) : base(transactionId, enlistmentId, protocol)
        {
            base.schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/RegisterParticipantTraceRecord";
            if (participantService != null)
            {
                this.participantService = EndpointAddressAugust2004.FromEndpointAddress(participantService);
            }
        }
    }
}

