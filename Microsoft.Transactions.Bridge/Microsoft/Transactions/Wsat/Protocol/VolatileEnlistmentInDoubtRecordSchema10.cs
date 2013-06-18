namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [DataContract(Name="VolatileEnlistmentInDoubt10")]
    internal class VolatileEnlistmentInDoubtRecordSchema10 : VolatileEnlistmentInDoubtRecordSchema
    {
        private const string id = "http://schemas.microsoft.com/2006/08/ServiceModel/VolatileEnlistmentInDoubtTraceRecord";
        [DataMember(Name="ReplyTo")]
        private EndpointAddressAugust2004 replyTo;

        public VolatileEnlistmentInDoubtRecordSchema10(Guid enlistmentId, EndpointAddress replyTo) : base(enlistmentId)
        {
            base.schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/VolatileEnlistmentInDoubtTraceRecord";
            if (replyTo != null)
            {
                this.replyTo = EndpointAddressAugust2004.FromEndpointAddress(replyTo);
            }
        }
    }
}

