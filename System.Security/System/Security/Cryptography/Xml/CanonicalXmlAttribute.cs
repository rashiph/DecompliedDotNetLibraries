namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using System.Xml;

    internal class CanonicalXmlAttribute : XmlAttribute, ICanonicalizableNode
    {
        private bool m_isInNodeSet;

        public CanonicalXmlAttribute(string prefix, string localName, string namespaceURI, XmlDocument doc, bool defaultNodeSetInclusionState) : base(prefix, localName, namespaceURI, doc)
        {
            this.IsInNodeSet = defaultNodeSetInclusionState;
        }

        public void Write(StringBuilder strBuilder, DocPosition docPos, AncestralNamespaceContextManager anc)
        {
            strBuilder.Append(" " + this.Name + "=\"");
            strBuilder.Append(System.Security.Cryptography.Xml.Utils.EscapeAttributeValue(this.Value));
            strBuilder.Append("\"");
        }

        public void WriteHash(HashAlgorithm hash, DocPosition docPos, AncestralNamespaceContextManager anc)
        {
            UTF8Encoding encoding = new UTF8Encoding(false);
            byte[] bytes = encoding.GetBytes(" " + this.Name + "=\"");
            hash.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
            bytes = encoding.GetBytes(System.Security.Cryptography.Xml.Utils.EscapeAttributeValue(this.Value));
            hash.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
            bytes = encoding.GetBytes("\"");
            hash.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
        }

        public bool IsInNodeSet
        {
            get
            {
                return this.m_isInNodeSet;
            }
            set
            {
                this.m_isInNodeSet = value;
            }
        }
    }
}

