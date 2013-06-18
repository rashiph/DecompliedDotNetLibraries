namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.Diagnostics;
    using System.Runtime.Serialization;
    using System.Xml;

    [DataContract(Name="ComPlusMexChannelBuilder")]
    internal class ComPlusMexChannelBuilderSchema : TraceRecord
    {
        [DataMember(Name="Address")]
        private string address;
        [DataMember(Name="Binding")]
        private string binding;
        [DataMember(Name="bindingNamespace")]
        private string bindingNamespace;
        [DataMember(Name="Contract")]
        private string contract;
        [DataMember(Name="contractNamespace")]
        private string contractNamespace;
        private const string schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusMexChannelBuilderTraceRecord";

        public ComPlusMexChannelBuilderSchema(string contract, string contractNamespace, string binding, string bindingNamespace, string address)
        {
            this.contract = contract;
            this.binding = binding;
            this.contractNamespace = contractNamespace;
            this.bindingNamespace = bindingNamespace;
            this.address = address;
        }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusMexChannelBuilderTraceRecord";
            }
        }
    }
}

