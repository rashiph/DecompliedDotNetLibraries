namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Runtime.Serialization;
    using System.Xml;

    [DataContract(Name="Reason")]
    internal class ReasonRecordSchema : TraceRecord
    {
        [DataMember(Name="Reason", IsRequired=true)]
        private string reason;
        private const string schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/ReasonTraceRecord";

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ReasonRecordSchema(string reason)
        {
            this.reason = reason;
        }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            TransactionTraceRecord.SerializeRecord(xmlWriter, this);
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/ReasonTraceRecord";
            }
        }
    }
}

