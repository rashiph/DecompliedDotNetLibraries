namespace System.Web.Services.Protocols
{
    using System;
    using System.Web.Services.Description;
    using System.Xml.Serialization;

    internal class SoapClientMethod
    {
        internal string action;
        internal object[] extensionInitializers;
        internal SoapReflectedExtension[] extensions;
        internal SoapHeaderMapping[] inHeaderMappings;
        internal XmlSerializer inHeaderSerializer;
        internal LogicalMethodInfo methodInfo;
        internal bool oneWay;
        internal SoapHeaderMapping[] outHeaderMappings;
        internal XmlSerializer outHeaderSerializer;
        internal XmlSerializer parameterSerializer;
        internal SoapParameterStyle paramStyle;
        internal XmlSerializer returnSerializer;
        internal bool rpc;
        internal SoapBindingUse use;
    }
}

