namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using System.Xml;

    internal class CanonicalXmlEntityReference : XmlEntityReference, ICanonicalizableNode
    {
        private bool m_isInNodeSet;

        public CanonicalXmlEntityReference(string name, XmlDocument doc, bool defaultNodeSetInclusionState) : base(name, doc)
        {
            this.m_isInNodeSet = defaultNodeSetInclusionState;
        }

        public void Write(StringBuilder strBuilder, DocPosition docPos, AncestralNamespaceContextManager anc)
        {
            if (this.IsInNodeSet)
            {
                CanonicalizationDispatcher.WriteGenericNode(this, strBuilder, docPos, anc);
            }
        }

        public void WriteHash(HashAlgorithm hash, DocPosition docPos, AncestralNamespaceContextManager anc)
        {
            if (this.IsInNodeSet)
            {
                CanonicalizationDispatcher.WriteHashGenericNode(this, hash, docPos, anc);
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

