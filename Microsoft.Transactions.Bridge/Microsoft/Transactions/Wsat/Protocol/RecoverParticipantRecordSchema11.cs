namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [DataContract(Name="RecoverParticipant11")]
    internal class RecoverParticipantRecordSchema11 : RecoverParticipantRecordSchema
    {
        private const string id = "http://schemas.microsoft.com/2006/08/ServiceModel/RecoverParticipant11TraceRecord";
        [DataMember(Name="ParticipantService")]
        private EndpointAddress10 participantService;

        public RecoverParticipantRecordSchema11(string transactionId, Guid enlistmentId, EndpointAddress participantService) : base(transactionId, enlistmentId)
        {
            base.schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/RecoverParticipant11TraceRecord";
            if (participantService != null)
            {
                this.participantService = EndpointAddress10.FromEndpointAddress(participantService);
            }
        }
    }
}

