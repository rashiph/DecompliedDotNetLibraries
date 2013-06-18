namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [DataContract(Name="RecoverParticipant10")]
    internal class RecoverParticipantRecordSchema10 : RecoverParticipantRecordSchema
    {
        private const string id = "http://schemas.microsoft.com/2006/08/ServiceModel/RecoverParticipantTraceRecord";
        [DataMember(Name="ParticipantService")]
        private EndpointAddressAugust2004 participantService;

        public RecoverParticipantRecordSchema10(string transactionId, Guid enlistmentId, EndpointAddress participantService) : base(transactionId, enlistmentId)
        {
            base.schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/RecoverParticipantTraceRecord";
            if (participantService != null)
            {
                this.participantService = EndpointAddressAugust2004.FromEndpointAddress(participantService);
            }
        }
    }
}

