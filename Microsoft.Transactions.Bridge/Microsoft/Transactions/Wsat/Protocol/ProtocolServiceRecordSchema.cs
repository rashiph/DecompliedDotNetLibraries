namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions;
    using System;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Runtime.Serialization;
    using System.Xml;

    [DataContract(Name="ProtocolService")]
    internal class ProtocolServiceRecordSchema : TraceRecord
    {
        [DataMember(Name="ProtocolIdentifier", IsRequired=true)]
        private Guid protocolId;
        [DataMember(Name="ProtocolName", IsRequired=true)]
        private string protocolName;
        private const string schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/ProtocolServiceTraceRecord";

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ProtocolServiceRecordSchema(string protocolName, Guid protocolId)
        {
            this.protocolName = protocolName;
            this.protocolId = protocolId;
        }

        public override string ToString()
        {
            return Microsoft.Transactions.SR.GetString("ProtocolServiceRecordSchema", new object[] { this.protocolName, this.protocolId.ToString() });
        }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            TransactionTraceRecord.SerializeRecord(xmlWriter, this);
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/ProtocolServiceTraceRecord";
            }
        }
    }
}

