namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.Serialization;
    using System.Xml;

    [DataContract(Name="ComPlusServiceHostCreatedServiceContract")]
    internal class ComPlusServiceHostCreatedServiceContractSchema : ComPlusServiceHostSchema
    {
        [DataMember(Name="Contract")]
        private string contract;
        [DataMember(Name="ContractQName")]
        private XmlQualifiedName contractQname;

        public ComPlusServiceHostCreatedServiceContractSchema(Guid appid, Guid clsid, XmlQualifiedName contractQname, string contract) : base(appid, clsid)
        {
            this.contractQname = contractQname;
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
                return base.BuildEventId("ComPlusServiceHostCreatedServiceContract");
            }
        }
    }
}

