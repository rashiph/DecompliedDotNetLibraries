namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.Diagnostics;
    using System.Runtime.Serialization;
    using System.Web.Services.Description;
    using System.Xml;

    [DataContract(Name="ComPlusServiceMoniker")]
    internal class ComPlusServiceMonikerSchema : TraceRecord
    {
        [DataMember(Name="Address")]
        private string address;
        [DataMember(Name="Binding")]
        private string binding;
        [DataMember(Name="BindingConfiguration")]
        private string bindingConfiguration;
        [DataMember(Name="BindingNamespace")]
        private string bindingNamespace;
        [DataMember(Name="Contract")]
        private string contract;
        [DataMember(Name="ContractNamespace")]
        private string contractNamespace;
        [DataMember(Name="DnsIdentity")]
        private string dnsIdentity;
        [DataMember(Name="mexAddress")]
        private string mexAddress;
        [DataMember(Name="mexBinding")]
        private string mexBinding;
        [DataMember(Name="mexBindingConfiguration")]
        private string mexBindingConfiguration;
        [DataMember(Name="mexDnsIdentity")]
        private string mexDnsIdentity;
        [DataMember(Name="mexSpnIdentity")]
        private string mexSpnIdentity;
        [DataMember(Name="mexUpnIdentity")]
        private string mexUpnIdentity;
        private const string schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusServiceMonikerTraceRecord";
        [DataMember(Name="SpnIdentity")]
        private string spnIdentity;
        [DataMember(Name="UpnIdentity")]
        private string upnIdentity;
        [DataMember(Name="Wsdl")]
        private WsdlWrapper wsdlWrapper;

        public ComPlusServiceMonikerSchema(string address, string contract, string contractNamespace, ServiceDescription wsdl, string spnIdentity, string upnIdentity, string dnsIdentity, string binding, string bindingConfiguration, string bindingNamespace, string mexAddress, string mexBinding, string mexBindingConfiguration, string mexSpnIdentity, string mexUpnIdentity, string mexDnsIdentity)
        {
            this.address = address;
            this.contract = contract;
            this.contractNamespace = contractNamespace;
            this.wsdlWrapper = new WsdlWrapper(wsdl);
            this.spnIdentity = spnIdentity;
            this.upnIdentity = spnIdentity;
            this.dnsIdentity = spnIdentity;
            this.binding = binding;
            this.bindingConfiguration = bindingConfiguration;
            this.bindingNamespace = bindingNamespace;
            this.mexSpnIdentity = mexSpnIdentity;
            this.mexUpnIdentity = mexUpnIdentity;
            this.mexDnsIdentity = mexDnsIdentity;
            this.mexAddress = mexAddress;
            this.mexBinding = mexBinding;
            this.mexBindingConfiguration = mexBindingConfiguration;
        }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusServiceMonikerTraceRecord";
            }
        }
    }
}

