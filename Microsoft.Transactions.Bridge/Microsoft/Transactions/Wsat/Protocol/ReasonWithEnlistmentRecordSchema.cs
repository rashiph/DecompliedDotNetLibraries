namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions;
    using System;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Runtime.Serialization;
    using System.Xml;

    [DataContract(Name="ReasonWithEnlistment")]
    internal class ReasonWithEnlistmentRecordSchema : TraceRecord
    {
        [DataMember(Name="EnlistmentId", IsRequired=true)]
        private Guid enlistmentId;
        [DataMember(Name="Reason", IsRequired=true)]
        private string reason;
        private const string schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/ReasonWithEnlistmentTraceRecord";
        [DataMember(Name="TransactionId", IsRequired=true)]
        private string transactionId;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ReasonWithEnlistmentRecordSchema(string transactionId, Guid enlistmentId, string reason)
        {
            this.transactionId = transactionId;
            this.enlistmentId = enlistmentId;
            this.reason = reason;
        }

        public override string ToString()
        {
            return Microsoft.Transactions.SR.GetString("ReasonWithEnlistmentRecordSchema", new object[] { this.transactionId, this.enlistmentId, this.reason });
        }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            TransactionTraceRecord.SerializeRecord(xmlWriter, this);
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/ReasonWithEnlistmentTraceRecord";
            }
        }
    }
}

