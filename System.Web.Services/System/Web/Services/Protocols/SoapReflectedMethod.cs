namespace System.Web.Services.Protocols
{
    using System;
    using System.Web.Services;
    using System.Web.Services.Description;
    using System.Xml;
    using System.Xml.Serialization;

    internal class SoapReflectedMethod
    {
        internal string action;
        internal WebServiceBindingAttribute binding;
        internal SoapReflectedExtension[] extensions;
        internal SoapReflectedHeader[] headers;
        internal XmlMembersMapping inHeaderMappings;
        internal LogicalMethodInfo methodInfo;
        internal string name;
        internal bool oneWay;
        internal XmlMembersMapping outHeaderMappings;
        internal SoapParameterStyle paramStyle;
        internal XmlQualifiedName portType;
        internal XmlQualifiedName requestElementName;
        internal XmlMembersMapping requestMappings;
        internal XmlMembersMapping responseMappings;
        internal bool rpc;
        internal SoapBindingUse use;

        internal bool IsClaimsConformance
        {
            get
            {
                return ((this.binding != null) && (this.binding.ConformsTo == WsiProfiles.BasicProfile1_1));
            }
        }
    }
}

