namespace System.ServiceModel.Security
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Text;
    using System.Xml;

    internal static class XmlHelper
    {
        internal static void AddNamespaceDeclaration(XmlDictionaryWriter writer, string prefix, XmlDictionaryString ns)
        {
            string str = writer.LookupPrefix(ns.Value);
            if ((str == null) || (str != prefix))
            {
                writer.WriteXmlnsAttribute(prefix, ns);
            }
        }

        internal static string EnsureNamespaceDefined(XmlDictionaryWriter writer, XmlDictionaryString ns, string defaultPrefix)
        {
            string prefix = writer.LookupPrefix(ns.Value);
            if (prefix == null)
            {
                writer.WriteXmlnsAttribute(defaultPrefix, ns);
                prefix = defaultPrefix;
            }
            return prefix;
        }

        private static UniqueId GetAttributeAsUniqueId(XmlDictionaryReader reader, string name, string ns)
        {
            if (!reader.MoveToAttribute(name, ns))
            {
                return null;
            }
            UniqueId id = reader.ReadContentAsUniqueId();
            reader.MoveToElement();
            return id;
        }

        internal static UniqueId GetAttributeAsUniqueId(XmlDictionaryReader reader, XmlDictionaryString localName, XmlDictionaryString ns)
        {
            return GetAttributeAsUniqueId(reader, localName.Value, (ns != null) ? ns.Value : null);
        }

        internal static XmlQualifiedName GetAttributeValueAsQName(XmlReader reader, string attributeName)
        {
            string attribute = reader.GetAttribute(attributeName);
            if (attribute == null)
            {
                return null;
            }
            return GetValueAsQName(reader, attribute);
        }

        internal static XmlElement GetChildElement(XmlElement parent)
        {
            if (parent == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parent");
            }
            XmlElement element = null;
            for (int i = 0; i < parent.ChildNodes.Count; i++)
            {
                System.Xml.XmlNode n = parent.ChildNodes[i];
                if ((n.NodeType != XmlNodeType.Whitespace) && (n.NodeType != XmlNodeType.Comment))
                {
                    if ((n.NodeType == XmlNodeType.Element) && (element == null))
                    {
                        element = (XmlElement) n;
                    }
                    else
                    {
                        OnUnexpectedChildNodeError(parent, n);
                    }
                }
            }
            if (element == null)
            {
                OnChildNodeTypeMissing(parent, XmlNodeType.Element);
            }
            return element;
        }

        internal static XmlElement GetChildElement(XmlElement parent, string childLocalName, string childNamespace)
        {
            if (parent == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parent");
            }
            for (int i = 0; i < parent.ChildNodes.Count; i++)
            {
                System.Xml.XmlNode n = parent.ChildNodes[i];
                if ((n.NodeType != XmlNodeType.Whitespace) && (n.NodeType != XmlNodeType.Comment))
                {
                    if (n.NodeType == XmlNodeType.Element)
                    {
                        if ((n.LocalName == childLocalName) && (n.NamespaceURI == childNamespace))
                        {
                            return (XmlElement) n;
                        }
                    }
                    else
                    {
                        OnUnexpectedChildNodeError(parent, n);
                    }
                }
            }
            return null;
        }

        internal static byte[] GetRequiredBase64Attribute(XmlDictionaryReader reader, XmlDictionaryString name, XmlDictionaryString ns)
        {
            if (!reader.MoveToAttribute(name.Value, (ns == null) ? null : ns.Value))
            {
                OnRequiredAttributeMissing(name.Value, (ns == null) ? null : ns.Value);
            }
            byte[] buffer = reader.ReadContentAsBase64();
            if ((buffer == null) || (buffer.Length == 0))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("EmptyBase64Attribute", new object[] { name, ns })));
            }
            return buffer;
        }

        internal static string GetRequiredNonEmptyAttribute(XmlDictionaryReader reader, XmlDictionaryString name, XmlDictionaryString ns)
        {
            string attribute = reader.GetAttribute(name, ns);
            if ((attribute == null) || (attribute.Length == 0))
            {
                OnRequiredAttributeMissing(name.Value, (reader == null) ? null : reader.Name);
            }
            return attribute;
        }

        internal static XmlQualifiedName GetValueAsQName(XmlReader reader, string value)
        {
            string str;
            string str2;
            SplitIntoPrefixAndName(value, out str, out str2);
            string ns = reader.LookupNamespace(str);
            if ((ns == null) && (str.Length > 0))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("CouldNotFindNamespaceForPrefix", new object[] { str })));
            }
            return new XmlQualifiedName(str2, ns);
        }

        internal static string GetWhiteSpace(XmlReader reader)
        {
            string str = null;
            StringBuilder builder = null;
            while ((reader.NodeType == XmlNodeType.Whitespace) || (reader.NodeType == XmlNodeType.SignificantWhitespace))
            {
                if (builder != null)
                {
                    builder.Append(reader.Value);
                }
                else if (str != null)
                {
                    builder = new StringBuilder(str);
                    builder.Append(reader.Value);
                    str = null;
                }
                else
                {
                    str = reader.Value;
                }
                if (!reader.Read())
                {
                    break;
                }
            }
            if (builder == null)
            {
                return str;
            }
            return builder.ToString();
        }

        internal static bool IsWhitespaceOrComment(XmlReader reader)
        {
            return ((reader.NodeType == XmlNodeType.Comment) || (reader.NodeType == XmlNodeType.Whitespace));
        }

        internal static void OnChildNodeTypeMissing(string parentName, XmlNodeType expectedNodeType)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("ChildNodeTypeMissing", new object[] { parentName, expectedNodeType })));
        }

        internal static void OnChildNodeTypeMissing(XmlElement parent, XmlNodeType expectedNodeType)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("ChildNodeTypeMissing", new object[] { parent.Name, expectedNodeType })));
        }

        internal static void OnEmptyElementError(XmlElement e)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("EmptyXmlElementError", new object[] { e.Name })));
        }

        internal static void OnEmptyElementError(XmlReader r)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("EmptyXmlElementError", new object[] { r.Name })));
        }

        internal static void OnEOF()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("UnexpectedEndOfFile")));
        }

        internal static void OnNamespaceMissing(string prefix)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("CouldNotFindNamespaceForPrefix", new object[] { prefix })));
        }

        internal static void OnRequiredAttributeMissing(string attrName, string elementName)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("RequiredAttributeMissing", new object[] { attrName, elementName })));
        }

        internal static void OnRequiredElementMissing(string elementName, string elementNamespace)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("ExpectedElementMissing", new object[] { elementName, elementNamespace })));
        }

        internal static void OnUnexpectedChildNodeError(string parentName, XmlReader r)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("UnexpectedXmlChildNode", new object[] { r.Name, r.NodeType, parentName })));
        }

        internal static void OnUnexpectedChildNodeError(XmlElement parent, System.Xml.XmlNode n)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("UnexpectedXmlChildNode", new object[] { n.Name, n.NodeType, parent.Name })));
        }

        public static long ReadElementContentAsInt64(XmlDictionaryReader reader)
        {
            reader.ReadFullStartElement();
            long num = reader.ReadContentAsLong();
            reader.ReadEndElement();
            return num;
        }

        public static UniqueId ReadElementStringAsUniqueId(XmlDictionaryReader reader)
        {
            if (reader.IsStartElement() && reader.IsEmptyElement)
            {
                reader.Read();
                return new UniqueId(string.Empty);
            }
            reader.ReadStartElement();
            UniqueId id = reader.ReadContentAsUniqueId();
            reader.ReadEndElement();
            return id;
        }

        public static UniqueId ReadElementStringAsUniqueId(XmlDictionaryReader reader, XmlDictionaryString localName, XmlDictionaryString ns)
        {
            if (reader.IsStartElement(localName, ns) && reader.IsEmptyElement)
            {
                reader.Read();
                return new UniqueId(string.Empty);
            }
            reader.ReadStartElement(localName, ns);
            UniqueId id = reader.ReadContentAsUniqueId();
            reader.ReadEndElement();
            return id;
        }

        internal static string ReadEmptyElementAndRequiredAttribute(XmlDictionaryReader reader, XmlDictionaryString name, XmlDictionaryString namespaceUri, XmlDictionaryString attributeName, out string prefix)
        {
            reader.MoveToStartElement(name, namespaceUri);
            prefix = reader.Prefix;
            bool isEmptyElement = reader.IsEmptyElement;
            string attribute = reader.GetAttribute(attributeName, null);
            if (attribute == null)
            {
                OnRequiredAttributeMissing(attributeName.Value, null);
            }
            reader.Read();
            if (!isEmptyElement)
            {
                reader.ReadEndElement();
            }
            return attribute;
        }

        internal static string ReadTextElementAsTrimmedString(XmlElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            using (XmlReader reader = new XmlNodeReader(element))
            {
                reader.MoveToContent();
                return XmlUtil.Trim(reader.ReadElementContentAsString());
            }
        }

        public static UniqueId ReadTextElementAsUniqueId(XmlElement element)
        {
            return new UniqueId(ReadTextElementAsTrimmedString(element));
        }

        internal static void SplitIntoPrefixAndName(string qName, out string prefix, out string name)
        {
            string[] strArray = qName.Split(new char[] { ':' });
            if (strArray.Length > 2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("InvalidQName"));
            }
            if (strArray.Length == 2)
            {
                prefix = strArray[0].Trim();
                name = strArray[1].Trim();
            }
            else
            {
                prefix = string.Empty;
                name = qName.Trim();
            }
        }

        internal static void ValidateIdPrefix(string idPrefix)
        {
            if (idPrefix == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("idPrefix"));
            }
            if (idPrefix.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("idPrefix", System.ServiceModel.SR.GetString("ValueMustBeGreaterThanZero")));
            }
            if (!char.IsLetter(idPrefix[0]) && (idPrefix[0] != '_'))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("idPrefix", System.ServiceModel.SR.GetString("InValidateIdPrefix", new object[] { idPrefix[0] })));
            }
            for (int i = 1; i < idPrefix.Length; i++)
            {
                char c = idPrefix[i];
                if (((!char.IsLetter(c) && !char.IsNumber(c)) && ((c != '.') && (c != '_'))) && (c != '-'))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("idPrefix", System.ServiceModel.SR.GetString("InValidateId", new object[] { idPrefix[i] })));
                }
            }
        }

        public static void WriteAttributeStringAsUniqueId(XmlDictionaryWriter writer, string prefix, XmlDictionaryString localName, XmlDictionaryString ns, UniqueId id)
        {
            writer.WriteStartAttribute(prefix, localName, ns);
            writer.WriteValue(id);
            writer.WriteEndAttribute();
        }

        public static void WriteElementContentAsInt64(XmlDictionaryWriter writer, XmlDictionaryString localName, XmlDictionaryString ns, long value)
        {
            writer.WriteStartElement(localName, ns);
            writer.WriteValue(value);
            writer.WriteEndElement();
        }

        public static void WriteElementStringAsUniqueId(XmlWriter writer, string localName, UniqueId id)
        {
            writer.WriteStartElement(localName);
            writer.WriteValue(id);
            writer.WriteEndElement();
        }

        public static void WriteElementStringAsUniqueId(XmlDictionaryWriter writer, XmlDictionaryString localName, XmlDictionaryString ns, UniqueId id)
        {
            writer.WriteStartElement(localName, ns);
            writer.WriteValue(id);
            writer.WriteEndElement();
        }

        public static void WriteStringAsUniqueId(XmlDictionaryWriter writer, UniqueId id)
        {
            writer.WriteValue(id);
        }
    }
}

