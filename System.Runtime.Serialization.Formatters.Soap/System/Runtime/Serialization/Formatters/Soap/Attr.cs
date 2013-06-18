namespace System.Runtime.Serialization.Formatters.Soap
{
    using System;
    using System.Reflection;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Metadata;

    internal static class Attr
    {
        internal static SoapAttributeInfo GetMemberAttributeInfo(MemberInfo memberInfo, string name, Type type)
        {
            SoapAttributeInfo attributeInfo = new SoapAttributeInfo();
            ProcessTypeAttribute(type, attributeInfo);
            ProcessMemberInfoAttribute(memberInfo, attributeInfo);
            return attributeInfo;
        }

        internal static void ProcessMemberInfoAttribute(MemberInfo memberInfo, SoapAttributeInfo attributeInfo)
        {
            SoapAttribute cachedSoapAttribute = InternalRemotingServices.GetCachedSoapAttribute(memberInfo);
            if (cachedSoapAttribute.Embedded)
            {
                attributeInfo.m_attributeType |= SoapAttributeType.Embedded;
            }
            if (cachedSoapAttribute is SoapFieldAttribute)
            {
                SoapFieldAttribute attribute2 = (SoapFieldAttribute) cachedSoapAttribute;
                if (attribute2.UseAttribute)
                {
                    attributeInfo.m_attributeType |= SoapAttributeType.XmlAttribute;
                    attributeInfo.m_elementName = attribute2.XmlElementName;
                    attributeInfo.m_nameSpace = attribute2.XmlNamespace;
                }
                else if (attribute2.IsInteropXmlElement())
                {
                    attributeInfo.m_attributeType |= SoapAttributeType.XmlElement;
                    attributeInfo.m_elementName = attribute2.XmlElementName;
                    attributeInfo.m_nameSpace = attribute2.XmlNamespace;
                }
            }
        }

        internal static void ProcessTypeAttribute(Type type, SoapAttributeInfo attributeInfo)
        {
            string str;
            string str2;
            SoapTypeAttribute cachedSoapAttribute = (SoapTypeAttribute) InternalRemotingServices.GetCachedSoapAttribute(type);
            if (cachedSoapAttribute.Embedded)
            {
                attributeInfo.m_attributeType |= SoapAttributeType.Embedded;
            }
            if (SoapServices.GetXmlElementForInteropType(type, out str, out str2))
            {
                attributeInfo.m_attributeType |= SoapAttributeType.XmlElement;
                attributeInfo.m_elementName = str;
                attributeInfo.m_nameSpace = str2;
            }
            if (SoapServices.GetXmlTypeForInteropType(type, out str, out str2))
            {
                attributeInfo.m_attributeType |= SoapAttributeType.XmlType;
                attributeInfo.m_typeName = str;
                attributeInfo.m_typeNamespace = str2;
            }
        }
    }
}

