namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.Diagnostics;
    using System.Runtime.Serialization;
    using System.Xml;

    [DataContract(Name="ComPlusMexBuilderMetadataRetrieved")]
    internal class ComPlusMexBuilderMetadataRetrievedSchema : TraceRecord
    {
        [DataMember(Name="bindingNamespaces")]
        private ComPlusMexBuilderMetadataRetrievedEndpoint[] endpoints;
        private const string schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusMexBuilderMetadataRetrievedTraceRecord";

        public ComPlusMexBuilderMetadataRetrievedSchema(ComPlusMexBuilderMetadataRetrievedEndpoint[] endpoints)
        {
            this.endpoints = endpoints;
        }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusMexBuilderMetadataRetrievedTraceRecord";
            }
        }
    }
}

