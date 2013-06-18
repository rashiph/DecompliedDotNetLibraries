namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.Diagnostics;
    using System.Runtime.Serialization;
    using System.Xml;

    [DataContract(Name="ComPlusTypedChannelBuilder")]
    internal class ComPlusTypedChannelBuilderSchema : TraceRecord
    {
        [DataMember(Name="Binding")]
        private string binding;
        [DataMember(Name="Contract")]
        private string contract;
        private const string schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusTypedChannelBuilderTraceRecord";

        public ComPlusTypedChannelBuilderSchema(string contract, string binding)
        {
            this.contract = contract;
            this.binding = binding;
        }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusTypedChannelBuilderTraceRecord";
            }
        }
    }
}

