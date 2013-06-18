namespace System.IdentityModel
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;

    internal static class XmlHelper
    {
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

        internal static void OnRequiredAttributeMissing(string attrName, string elementName)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.IdentityModel.SR.GetString("RequiredAttributeMissing", new object[] { attrName, elementName })));
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
            XmlReader reader = new XmlNodeReader(element);
            reader.MoveToContent();
            return XmlUtil.Trim(reader.ReadElementContentAsString());
        }
    }
}

