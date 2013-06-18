namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.InteropServices;

    internal static class MonikerHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct KeywordInfo
        {
            internal string Name;
            internal MonikerHelper.MonikerAttribute Attrib;
            internal static readonly MonikerHelper.KeywordInfo[] KeywordCollection;
            internal KeywordInfo(string name, MonikerHelper.MonikerAttribute attrib)
            {
                this.Name = name;
                this.Attrib = attrib;
            }

            static KeywordInfo()
            {
                KeywordCollection = new MonikerHelper.KeywordInfo[] { 
                    new MonikerHelper.KeywordInfo("address", MonikerHelper.MonikerAttribute.Address), new MonikerHelper.KeywordInfo("contract", MonikerHelper.MonikerAttribute.Contract), new MonikerHelper.KeywordInfo("wsdl", MonikerHelper.MonikerAttribute.Wsdl), new MonikerHelper.KeywordInfo("spnidentity", MonikerHelper.MonikerAttribute.SpnIdentity), new MonikerHelper.KeywordInfo("upnidentity", MonikerHelper.MonikerAttribute.UpnIdentity), new MonikerHelper.KeywordInfo("dnsidentity", MonikerHelper.MonikerAttribute.DnsIdentity), new MonikerHelper.KeywordInfo("binding", MonikerHelper.MonikerAttribute.Binding), new MonikerHelper.KeywordInfo("bindingconfiguration", MonikerHelper.MonikerAttribute.BindingConfiguration), new MonikerHelper.KeywordInfo("mexaddress", MonikerHelper.MonikerAttribute.MexAddress), new MonikerHelper.KeywordInfo("mexbindingconfiguration", MonikerHelper.MonikerAttribute.MexBindingConfiguration), new MonikerHelper.KeywordInfo("mexbinding", MonikerHelper.MonikerAttribute.MexBinding), new MonikerHelper.KeywordInfo("bindingnamespace", MonikerHelper.MonikerAttribute.BindingNamespace), new MonikerHelper.KeywordInfo("contractnamespace", MonikerHelper.MonikerAttribute.ContractNamespace), new MonikerHelper.KeywordInfo("mexspnidentity", MonikerHelper.MonikerAttribute.MexSpnIdentity), new MonikerHelper.KeywordInfo("mexupnidentity", MonikerHelper.MonikerAttribute.MexUpnIdentity), new MonikerHelper.KeywordInfo("mexdnsidentity", MonikerHelper.MonikerAttribute.MexDnsIdentity), 
                    new MonikerHelper.KeywordInfo("serializer", MonikerHelper.MonikerAttribute.Serializer)
                 };
            }
        }

        internal enum MonikerAttribute
        {
            Address,
            Contract,
            Wsdl,
            SpnIdentity,
            UpnIdentity,
            DnsIdentity,
            Binding,
            BindingConfiguration,
            MexAddress,
            MexBinding,
            MexBindingConfiguration,
            BindingNamespace,
            ContractNamespace,
            MexSpnIdentity,
            MexUpnIdentity,
            MexDnsIdentity,
            Serializer
        }
    }
}

