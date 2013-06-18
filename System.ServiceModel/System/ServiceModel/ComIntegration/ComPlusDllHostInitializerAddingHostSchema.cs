namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.Serialization;
    using System.Xml;

    [DataContract(Name="ComPlusDllHostInitializerAddingHost")]
    internal class ComPlusDllHostInitializerAddingHostSchema : ComPlusDllHostInitializerSchema
    {
        [DataMember(Name="Address")]
        private string address;
        [DataMember(Name="BehaviorConfiguration")]
        private string behaviorConfiguration;
        [DataMember(Name="BindingConfiguration")]
        private string bindingConfiguration;
        [DataMember(Name="BindingName")]
        private string bindingName;
        [DataMember(Name="BindingNamespace")]
        private string bindingNamespace;
        [DataMember(Name="BindingSectionName")]
        private string bindingSectionName;
        [DataMember(Name="clsid")]
        private Guid clsid;
        [DataMember(Name="ContractType")]
        private string contractType;
        private const string schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusDllHostInitializerAddingHostTraceRecord";
        [DataMember(Name="ServiceType")]
        private string serviceType;

        public ComPlusDllHostInitializerAddingHostSchema(Guid appid, Guid clsid, string behaviorConfiguration, string serviceType, string address, string bindingConfiguration, string bindingName, string bindingNamespace, string bindingSectionName, string contractType) : base(appid)
        {
            this.clsid = clsid;
            this.behaviorConfiguration = behaviorConfiguration;
            this.serviceType = serviceType;
            this.address = address;
            this.bindingConfiguration = bindingConfiguration;
            this.bindingName = bindingName;
            this.bindingNamespace = bindingNamespace;
            this.bindingSectionName = bindingSectionName;
            this.contractType = contractType;
        }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusDllHostInitializerAddingHostTraceRecord";
            }
        }
    }
}

