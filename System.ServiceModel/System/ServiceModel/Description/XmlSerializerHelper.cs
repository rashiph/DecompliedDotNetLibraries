namespace System.ServiceModel.Description
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.ServiceModel;
    using System.Xml;
    using System.Xml.Serialization;

    internal static class XmlSerializerHelper
    {
        internal static XmlReflectionMember GetXmlReflectionMember(MessagePartDescription part, bool isRpc, bool isEncoded, bool isWrapped)
        {
            string ns = isRpc ? null : part.Namespace;
            ICustomAttributeProvider additionalAttributesProvider = null;
            if (isEncoded || (part.AdditionalAttributesProvider is MemberInfo))
            {
                additionalAttributesProvider = part.AdditionalAttributesProvider;
            }
            System.ServiceModel.Description.XmlName memberName = string.IsNullOrEmpty(part.UniquePartName) ? null : new System.ServiceModel.Description.XmlName(part.UniquePartName, true);
            System.ServiceModel.Description.XmlName xmlName = part.XmlName;
            return GetXmlReflectionMember(memberName, xmlName, ns, part.Type, additionalAttributesProvider, part.Multiple, isEncoded, isWrapped);
        }

        internal static XmlReflectionMember GetXmlReflectionMember(System.ServiceModel.Description.XmlName memberName, System.ServiceModel.Description.XmlName elementName, string ns, Type type, ICustomAttributeProvider additionalAttributesProvider, bool isMultiple, bool isEncoded, bool isWrapped)
        {
            if (isEncoded && isMultiple)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxMultiplePartsNotAllowedInEncoded", new object[] { elementName.DecodedName, ns })));
            }
            XmlReflectionMember member = new XmlReflectionMember {
                MemberName = (memberName ?? elementName).DecodedName,
                MemberType = type
            };
            if (member.MemberType.IsByRef)
            {
                member.MemberType = member.MemberType.GetElementType();
            }
            if (isMultiple)
            {
                member.MemberType = member.MemberType.MakeArrayType();
            }
            if (additionalAttributesProvider != null)
            {
                if (isEncoded)
                {
                    member.SoapAttributes = new SoapAttributes(additionalAttributesProvider);
                }
                else
                {
                    member.XmlAttributes = new XmlAttributes(additionalAttributesProvider);
                }
            }
            if (isEncoded)
            {
                if (member.SoapAttributes == null)
                {
                    member.SoapAttributes = new SoapAttributes();
                }
                else
                {
                    Type type2 = null;
                    if (member.SoapAttributes.SoapAttribute != null)
                    {
                        type2 = typeof(SoapAttributeAttribute);
                    }
                    else if (member.SoapAttributes.SoapIgnore)
                    {
                        type2 = typeof(SoapIgnoreAttribute);
                    }
                    else if (member.SoapAttributes.SoapType != null)
                    {
                        type2 = typeof(SoapTypeAttribute);
                    }
                    if (type2 != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxInvalidSoapAttribute", new object[] { type2, elementName.DecodedName })));
                    }
                }
                if (member.SoapAttributes.SoapElement == null)
                {
                    member.SoapAttributes.SoapElement = new SoapElementAttribute(elementName.DecodedName);
                }
                return member;
            }
            if (member.XmlAttributes == null)
            {
                member.XmlAttributes = new XmlAttributes();
            }
            else
            {
                Type type3 = null;
                if (member.XmlAttributes.XmlAttribute != null)
                {
                    type3 = typeof(XmlAttributeAttribute);
                }
                else if ((member.XmlAttributes.XmlAnyAttribute != null) && !isWrapped)
                {
                    type3 = typeof(XmlAnyAttributeAttribute);
                }
                else if (member.XmlAttributes.XmlChoiceIdentifier != null)
                {
                    type3 = typeof(XmlChoiceIdentifierAttribute);
                }
                else if (member.XmlAttributes.XmlIgnore)
                {
                    type3 = typeof(XmlIgnoreAttribute);
                }
                else if (member.XmlAttributes.Xmlns)
                {
                    type3 = typeof(XmlNamespaceDeclarationsAttribute);
                }
                else if (member.XmlAttributes.XmlText != null)
                {
                    type3 = typeof(XmlTextAttribute);
                }
                else if (member.XmlAttributes.XmlEnum != null)
                {
                    type3 = typeof(XmlEnumAttribute);
                }
                if (type3 != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString(isWrapped ? "SFxInvalidXmlAttributeInWrapped" : "SFxInvalidXmlAttributeInBare", new object[] { type3, elementName.DecodedName })));
                }
                if ((member.XmlAttributes.XmlArray != null) && isMultiple)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxXmlArrayNotAllowedForMultiple", new object[] { elementName.DecodedName, ns })));
                }
            }
            bool isArray = member.MemberType.IsArray;
            if (((isArray && !isMultiple) && (member.MemberType != typeof(byte[]))) || (((!isArray && typeof(IEnumerable).IsAssignableFrom(member.MemberType)) && ((member.MemberType != typeof(string)) && !typeof(System.Xml.XmlNode).IsAssignableFrom(member.MemberType))) && !typeof(IXmlSerializable).IsAssignableFrom(member.MemberType)))
            {
                if (member.XmlAttributes.XmlArray != null)
                {
                    if (member.XmlAttributes.XmlArray.ElementName == string.Empty)
                    {
                        member.XmlAttributes.XmlArray.ElementName = elementName.DecodedName;
                    }
                    if (member.XmlAttributes.XmlArray.Namespace == null)
                    {
                        member.XmlAttributes.XmlArray.Namespace = ns;
                    }
                    return member;
                }
                if (HasNoXmlParameterAttributes(member.XmlAttributes))
                {
                    member.XmlAttributes.XmlArray = new XmlArrayAttribute();
                    member.XmlAttributes.XmlArray.ElementName = elementName.DecodedName;
                    member.XmlAttributes.XmlArray.Namespace = ns;
                }
                return member;
            }
            if ((member.XmlAttributes.XmlElements == null) || (member.XmlAttributes.XmlElements.Count == 0))
            {
                if (HasNoXmlParameterAttributes(member.XmlAttributes))
                {
                    XmlElementAttribute attribute = new XmlElementAttribute {
                        ElementName = elementName.DecodedName,
                        Namespace = ns
                    };
                    member.XmlAttributes.XmlElements.Add(attribute);
                }
                return member;
            }
            foreach (XmlElementAttribute attribute2 in member.XmlAttributes.XmlElements)
            {
                if (attribute2.ElementName == string.Empty)
                {
                    attribute2.ElementName = elementName.DecodedName;
                }
                if (attribute2.Namespace == null)
                {
                    attribute2.Namespace = ns;
                }
            }
            return member;
        }

        private static bool HasNoXmlParameterAttributes(XmlAttributes xmlAttributes)
        {
            return ((((xmlAttributes.XmlAnyAttribute == null) && ((xmlAttributes.XmlAnyElements == null) || (xmlAttributes.XmlAnyElements.Count == 0))) && ((((xmlAttributes.XmlArray == null) && (xmlAttributes.XmlAttribute == null)) && (!xmlAttributes.XmlIgnore && (xmlAttributes.XmlText == null))) && ((xmlAttributes.XmlChoiceIdentifier == null) && ((xmlAttributes.XmlElements == null) || (xmlAttributes.XmlElements.Count == 0))))) && !xmlAttributes.Xmlns);
        }
    }
}

