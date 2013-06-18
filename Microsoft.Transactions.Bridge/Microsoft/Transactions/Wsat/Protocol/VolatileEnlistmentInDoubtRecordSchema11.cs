namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [DataContract(Name="VolatileEnlistmentInDoubt11")]
    internal class VolatileEnlistmentInDoubtRecordSchema11 : VolatileEnlistmentInDoubtRecordSchema
    {
        private const string id = "http://schemas.microsoft.com/2006/08/ServiceModel/VolatileEnlistmentInDoubt11TraceRecord";
        [DataMember(Name="ReplyTo")]
        private EndpointAddress10 replyTo;

        public VolatileEnlistmentInDoubtRecordSchema11(Guid enlistmentId, EndpointAddress replyTo) : base(enlistmentId)
        {
            base.schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/VolatileEnlistmentInDoubt11TraceRecord";
            if (replyTo != null)
            {
                this.replyTo = EndpointAddress10.FromEndpointAddress(replyTo);
            }
        }
    }
}

