namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Web.Services.Description;

    internal static class ComPlusServiceMonikerTrace
    {
        public static void Trace(TraceEventType type, int traceCode, string description, Dictionary<MonikerHelper.MonikerAttribute, string> propertyTable)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                string str = null;
                string str2 = null;
                string str3 = null;
                string str4 = null;
                string str5 = null;
                string str6 = null;
                string str7 = null;
                string str8 = null;
                string str9 = null;
                string str10 = null;
                string str11 = null;
                string str12 = null;
                string str13 = null;
                string str14 = null;
                string str15 = null;
                string str16 = null;
                ServiceDescription wsdl = null;
                propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Wsdl, out str8);
                propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Contract, out str2);
                propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Address, out str);
                propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Binding, out str3);
                propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.BindingConfiguration, out str4);
                propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.SpnIdentity, out str5);
                propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.UpnIdentity, out str6);
                propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.DnsIdentity, out str7);
                propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.MexAddress, out str9);
                propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.MexBinding, out str10);
                propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.MexBindingConfiguration, out str11);
                propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.MexSpnIdentity, out str12);
                propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.MexUpnIdentity, out str13);
                propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.MexDnsIdentity, out str14);
                propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.ContractNamespace, out str15);
                propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.BindingNamespace, out str16);
                if (!string.IsNullOrEmpty(str8))
                {
                    TextReader textReader = new StringReader(str8);
                    wsdl = ServiceDescription.Read(textReader);
                }
                ComPlusServiceMonikerSchema schema = new ComPlusServiceMonikerSchema(str, str2, str15, wsdl, str5, str6, str7, str3, str4, str16, str9, str10, str11, str12, str13, str14);
                TraceUtility.TraceEvent(type, traceCode, System.ServiceModel.SR.GetString(description), (TraceRecord) schema);
            }
        }
    }
}

