namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Runtime.Serialization;
    using System.Xml;

    [DataContract(Name="CoordinatorRetryMessage")]
    internal class CoordinatorRetryMessageRecordSchema : TraceRecord
    {
        [DataMember(Name="RetryCount", IsRequired=true)]
        private int count;
        private const string schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/CoordinatorRetryMessageTraceRecord";
        [DataMember(Name="TransactionId", IsRequired=true)]
        private string transactionId;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CoordinatorRetryMessageRecordSchema(string transactionId, int count)
        {
            this.transactionId = transactionId;
            this.count = count;
        }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            TransactionTraceRecord.SerializeRecord(xmlWriter, this);
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/CoordinatorRetryMessageTraceRecord";
            }
        }
    }
}

