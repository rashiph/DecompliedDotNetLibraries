namespace System.ServiceModel
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal static class XmlUtil
    {
        public const string XmlNs = "http://www.w3.org/XML/1998/namespace";
        public const string XmlNsNs = "http://www.w3.org/2000/xmlns/";

        public static string GetXmlLangAttribute(XmlReader reader)
        {
            string str = null;
            if (reader.MoveToAttribute("lang", "http://www.w3.org/XML/1998/namespace"))
            {
                str = reader.Value;
                reader.MoveToElement();
            }
            if (str == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("XmlLangAttributeMissing")));
            }
            return str;
        }

        public static bool IsTrue(string booleanValue)
        {
            if (string.IsNullOrEmpty(booleanValue))
            {
                return false;
            }
            return XmlConvert.ToBoolean(booleanValue);
        }

        public static bool IsWhitespace(char ch)
        {
            if (((ch != ' ') && (ch != '\t')) && (ch != '\r'))
            {
                return (ch == '\n');
            }
            return true;
        }

        public static void ParseQName(XmlReader reader, string qname, out string localName, out string ns)
        {
            string str;
            int index = qname.IndexOf(':');
            if (index < 0)
            {
                str = "";
                localName = TrimStart(TrimEnd(qname));
            }
            else
            {
                if (index == (qname.Length - 1))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("InvalidXmlQualifiedName", new object[] { qname })));
                }
                str = TrimStart(qname.Substring(0, index));
                localName = TrimEnd(qname.Substring(index + 1));
            }
            ns = reader.LookupNamespace(str);
            if (ns == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("UnboundPrefixInQName", new object[] { qname })));
            }
        }

        public static void ReadContentAsQName(XmlReader reader, out string localName, out string ns)
        {
            ParseQName(reader, reader.ReadContentAsString(), out localName, out ns);
        }

        public static string Trim(string s)
        {
            int startIndex = 0;
            while ((startIndex < s.Length) && IsWhitespace(s[startIndex]))
            {
                startIndex++;
            }
            if (startIndex >= s.Length)
            {
                return string.Empty;
            }
            int length = s.Length;
            while ((length > 0) && IsWhitespace(s[length - 1]))
            {
                length--;
            }
            if ((startIndex == 0) && (length == s.Length))
            {
                return s;
            }
            return s.Substring(startIndex, length - startIndex);
        }

        public static string TrimEnd(string s)
        {
            int length = s.Length;
            while ((length > 0) && IsWhitespace(s[length - 1]))
            {
                length--;
            }
            if (length != s.Length)
            {
                return s.Substring(0, length);
            }
            return s;
        }

        public static string TrimStart(string s)
        {
            int startIndex = 0;
            while ((startIndex < s.Length) && IsWhitespace(s[startIndex]))
            {
                startIndex++;
            }
            if (startIndex != 0)
            {
                return s.Substring(startIndex);
            }
            return s;
        }
    }
}

