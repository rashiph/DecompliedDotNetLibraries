namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.Serialization;
    using System.Xml;

    [DataContract(Name="ComPlusServiceHostCreatedServiceEndpoint")]
    internal class ComPlusServiceHostCreatedServiceEndpointSchema : ComPlusServiceHostSchema
    {
        [DataMember(Name="Address")]
        private Uri address;
        [DataMember(Name="Binding")]
        private string binding;
        [DataMember(Name="Contract")]
        private string contract;

        public ComPlusServiceHostCreatedServiceEndpointSchema(Guid appid, Guid clsid, string contract, Uri address, string binding) : base(appid, clsid)
        {
            this.contract = contract;
            this.address = address;
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
                return base.BuildEventId("ComPlusServiceHostCreatedServiceEndpoint");
            }
        }
    }
}

