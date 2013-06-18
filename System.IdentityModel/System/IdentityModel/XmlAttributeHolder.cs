namespace System.IdentityModel
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;

    [StructLayout(LayoutKind.Sequential)]
    internal struct XmlAttributeHolder
    {
        private string prefix;
        private string ns;
        private string localName;
        private string value;
        public static XmlAttributeHolder[] emptyArray;
        public XmlAttributeHolder(string prefix, string localName, string ns, string value)
        {
            this.prefix = prefix;
            this.localName = localName;
            this.ns = ns;
            this.value = value;
        }

        public string Prefix
        {
            get
            {
                return this.prefix;
            }
        }
        public string NamespaceUri
        {
            get
            {
                return this.ns;
            }
        }
        public string LocalName
        {
            get
            {
                return this.localName;
            }
        }
        public string Value
        {
            get
            {
                return this.value;
            }
        }
        public void WriteTo(XmlWriter writer)
        {
            writer.WriteStartAttribute(this.prefix, this.localName, this.ns);
            writer.WriteString(this.value);
            writer.WriteEndAttribute();
        }

        public static void WriteAttributes(XmlAttributeHolder[] attributes, XmlWriter writer)
        {
            for (int i = 0; i < attributes.Length; i++)
            {
                attributes[i].WriteTo(writer);
            }
        }

        public static XmlAttributeHolder[] ReadAttributes(XmlDictionaryReader reader)
        {
            int maxSizeOfHeaders = 0x7fffffff;
            return ReadAttributes(reader, ref maxSizeOfHeaders);
        }

        public static XmlAttributeHolder[] ReadAttributes(XmlDictionaryReader reader, ref int maxSizeOfHeaders)
        {
            if (reader.AttributeCount == 0)
            {
                return emptyArray;
            }
            XmlAttributeHolder[] holderArray = new XmlAttributeHolder[reader.AttributeCount];
            reader.MoveToFirstAttribute();
            for (int i = 0; i < holderArray.Length; i++)
            {
                string namespaceURI = reader.NamespaceURI;
                string localName = reader.LocalName;
                string prefix = reader.Prefix;
                string s = string.Empty;
                while (reader.ReadAttributeValue())
                {
                    if (s.Length == 0)
                    {
                        s = reader.Value;
                    }
                    else
                    {
                        s = s + reader.Value;
                    }
                }
                Deduct(prefix, ref maxSizeOfHeaders);
                Deduct(localName, ref maxSizeOfHeaders);
                Deduct(namespaceURI, ref maxSizeOfHeaders);
                Deduct(s, ref maxSizeOfHeaders);
                holderArray[i] = new XmlAttributeHolder(prefix, localName, namespaceURI, s);
                reader.MoveToNextAttribute();
            }
            reader.MoveToElement();
            return holderArray;
        }

        private static void Deduct(string s, ref int maxSizeOfHeaders)
        {
            int num = s.Length * 2;
            if (num > maxSizeOfHeaders)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("XmlBufferQuotaExceeded")));
            }
            maxSizeOfHeaders -= num;
        }

        public static string GetAttribute(XmlAttributeHolder[] attributes, string localName, string ns)
        {
            for (int i = 0; i < attributes.Length; i++)
            {
                if ((attributes[i].LocalName == localName) && (attributes[i].NamespaceUri == ns))
                {
                    return attributes[i].Value;
                }
            }
            return null;
        }

        static XmlAttributeHolder()
        {
            emptyArray = new XmlAttributeHolder[0];
        }
    }
}

