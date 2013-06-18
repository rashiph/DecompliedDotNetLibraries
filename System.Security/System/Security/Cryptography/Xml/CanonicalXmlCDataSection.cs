namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using System.Xml;

    internal class CanonicalXmlCDataSection : XmlCDataSection, ICanonicalizableNode
    {
        private bool m_isInNodeSet;

        public CanonicalXmlCDataSection(string data, XmlDocument doc, bool defaultNodeSetInclusionState) : base(data, doc)
        {
            this.m_isInNodeSet = defaultNodeSetInclusionState;
        }

        public void Write(StringBuilder strBuilder, DocPosition docPos, AncestralNamespaceContextManager anc)
        {
            if (this.IsInNodeSet)
            {
                strBuilder.Append(System.Security.Cryptography.Xml.Utils.EscapeCData(this.Data));
            }
        }

        public void WriteHash(HashAlgorithm hash, DocPosition docPos, AncestralNamespaceContextManager anc)
        {
            if (this.IsInNodeSet)
            {
                byte[] bytes = new UTF8Encoding(false).GetBytes(System.Security.Cryptography.Xml.Utils.EscapeCData(this.Data));
                hash.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
            }
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

