namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.Diagnostics;
    using System.Runtime.Serialization;
    using System.ServiceModel.Description;
    using System.Xml;

    [DataContract(Name="ComPlusMexBuilderMetadataRetrievedEndpoint")]
    internal class ComPlusMexBuilderMetadataRetrievedEndpoint : TraceRecord
    {
        [DataMember(Name="Address")]
        private string address;
        [DataMember(Name="Binding")]
        private string binding;
        [DataMember(Name="BindingNamespace")]
        private string bindingNamespace;
        [DataMember(Name="Contract")]
        private string contract;
        [DataMember(Name="ContractNamespace")]
        private string contractNamespace;
        private const string schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusMexBuilderMetadataRetrievedEndpointTraceRecord";

        public ComPlusMexBuilderMetadataRetrievedEndpoint(ServiceEndpoint endpoint)
        {
            this.binding = endpoint.Binding.Name;
            this.bindingNamespace = endpoint.Binding.Namespace;
            this.address = endpoint.Address.ToString();
            this.contract = endpoint.Contract.Name;
            this.contractNamespace = endpoint.Contract.Namespace;
        }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusMexBuilderMetadataRetrievedEndpointTraceRecord";
            }
        }
    }
}

