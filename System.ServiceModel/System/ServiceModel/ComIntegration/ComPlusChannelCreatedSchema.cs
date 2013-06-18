namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.Diagnostics;
    using System.Runtime.Serialization;
    using System.Xml;

    [DataContract(Name="ComPlusChannelCreated")]
    internal class ComPlusChannelCreatedSchema : TraceRecord
    {
        [DataMember(Name="Address")]
        private Uri address;
        [DataMember(Name="Contract")]
        private string contract;
        private const string schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusChannelCreatedTraceRecord";

        public ComPlusChannelCreatedSchema(Uri address, string contract)
        {
            this.address = address;
            this.contract = contract;
        }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusChannelCreatedTraceRecord";
            }
        }
    }
}

