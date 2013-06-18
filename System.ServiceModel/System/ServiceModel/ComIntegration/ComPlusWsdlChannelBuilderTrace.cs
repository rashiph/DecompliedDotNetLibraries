namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.Web.Services.Description;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    internal static class ComPlusWsdlChannelBuilderTrace
    {
        public static void Trace(TraceEventType type, int traceCode, string description, XmlQualifiedName bindingQname, XmlQualifiedName contractQname, System.Web.Services.Description.ServiceDescription wsdl, ContractDescription contract, System.ServiceModel.Channels.Binding binding, XmlSchemas schemas)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                string name = "Service";
                if (wsdl.Name != null)
                {
                    name = wsdl.Name;
                }
                System.Type contractType = contract.ContractType;
                XmlQualifiedName serviceQname = new XmlQualifiedName(name, wsdl.TargetNamespace);
                foreach (System.Xml.Schema.XmlSchema schema in schemas)
                {
                    ComPlusWsdlChannelBuilderSchema schema2 = new ComPlusWsdlChannelBuilderSchema(bindingQname, contractQname, serviceQname, (contractType != null) ? contractType.ToString() : null, (binding != null) ? binding.GetType().ToString() : null, schema);
                    TraceUtility.TraceEvent(type, traceCode, System.ServiceModel.SR.GetString(description), (TraceRecord) schema2);
                }
            }
        }
    }
}

