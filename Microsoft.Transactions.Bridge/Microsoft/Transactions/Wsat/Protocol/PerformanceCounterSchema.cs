namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions;
    using System;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Runtime.Serialization;
    using System.Xml;

    [DataContract(Name="PerformanceCounter")]
    internal class PerformanceCounterSchema : TraceRecord
    {
        [DataMember(Name="Name", IsRequired=true)]
        private string counterName;
        private const string schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/PerformanceCounterTraceRecord";

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public PerformanceCounterSchema(string counterName)
        {
            this.counterName = counterName;
        }

        public override string ToString()
        {
            return Microsoft.Transactions.SR.GetString("PerformanceCounterSchema", new object[] { this.counterName });
        }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            TransactionTraceRecord.SerializeRecord(xmlWriter, this);
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/PerformanceCounterTraceRecord";
            }
        }
    }
}

