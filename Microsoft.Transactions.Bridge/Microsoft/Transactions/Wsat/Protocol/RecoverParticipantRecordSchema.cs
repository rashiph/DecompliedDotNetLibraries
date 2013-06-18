namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.Xml;

    [DataContract(Name="RecoverParticipant")]
    internal abstract class RecoverParticipantRecordSchema : TraceRecord
    {
        [DataMember(Name="EnlistmentId", IsRequired=true)]
        private Guid enlistmentId;
        protected string schemaId;
        [DataMember(Name="TransactionId", IsRequired=true)]
        private string transactionId;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected RecoverParticipantRecordSchema(string transactionId, Guid enlistmentId)
        {
            this.transactionId = transactionId;
            this.enlistmentId = enlistmentId;
        }

        public static RecoverParticipantRecordSchema Instance(string transactionId, Guid enlistmentId, EndpointAddress participantService, ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(RecoverParticipantRecordSchema), "Instance");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return new RecoverParticipantRecordSchema10(transactionId, enlistmentId, participantService);

                case ProtocolVersion.Version11:
                    return new RecoverParticipantRecordSchema11(transactionId, enlistmentId, participantService);
            }
            return null;
        }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            TransactionTraceRecord.SerializeRecord(xmlWriter, this);
        }

        internal override string EventId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.schemaId;
            }
        }
    }
}

