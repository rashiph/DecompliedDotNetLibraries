namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using System.Xml;

    internal class CanonicalXmlDocument : XmlDocument, ICanonicalizableNode
    {
        private bool m_defaultNodeSetInclusionState;
        private bool m_includeComments;
        private bool m_isInNodeSet;

        public CanonicalXmlDocument(bool defaultNodeSetInclusionState, bool includeComments)
        {
            base.PreserveWhitespace = true;
            this.m_includeComments = includeComments;
            this.m_isInNodeSet = this.m_defaultNodeSetInclusionState = defaultNodeSetInclusionState;
        }

        public override XmlAttribute CreateAttribute(string prefix, string localName, string namespaceURI)
        {
            return new CanonicalXmlAttribute(prefix, localName, namespaceURI, this, this.m_defaultNodeSetInclusionState);
        }

        public override XmlCDataSection CreateCDataSection(string data)
        {
            return new CanonicalXmlCDataSection(data, this, this.m_defaultNodeSetInclusionState);
        }

        public override XmlComment CreateComment(string data)
        {
            return new CanonicalXmlComment(data, this, this.m_defaultNodeSetInclusionState, this.m_includeComments);
        }

        protected override XmlAttribute CreateDefaultAttribute(string prefix, string localName, string namespaceURI)
        {
            return new CanonicalXmlAttribute(prefix, localName, namespaceURI, this, this.m_defaultNodeSetInclusionState);
        }

        public override XmlElement CreateElement(string prefix, string localName, string namespaceURI)
        {
            return new CanonicalXmlElement(prefix, localName, namespaceURI, this, this.m_defaultNodeSetInclusionState);
        }

        public override XmlEntityReference CreateEntityReference(string name)
        {
            return new CanonicalXmlEntityReference(name, this, this.m_defaultNodeSetInclusionState);
        }

        public override XmlProcessingInstruction CreateProcessingInstruction(string target, string data)
        {
            return new CanonicalXmlProcessingInstruction(target, data, this, this.m_defaultNodeSetInclusionState);
        }

        public override XmlSignificantWhitespace CreateSignificantWhitespace(string text)
        {
            return new CanonicalXmlSignificantWhitespace(text, this, this.m_defaultNodeSetInclusionState);
        }

        public override XmlText CreateTextNode(string text)
        {
            return new CanonicalXmlText(text, this, this.m_defaultNodeSetInclusionState);
        }

        public override XmlWhitespace CreateWhitespace(string prefix)
        {
            return new CanonicalXmlWhitespace(prefix, this, this.m_defaultNodeSetInclusionState);
        }

        public void Write(StringBuilder strBuilder, DocPosition docPos, AncestralNamespaceContextManager anc)
        {
            docPos = DocPosition.BeforeRootElement;
            foreach (XmlNode node in this.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    CanonicalizationDispatcher.Write(node, strBuilder, DocPosition.InRootElement, anc);
                    docPos = DocPosition.AfterRootElement;
                }
                else
                {
                    CanonicalizationDispatcher.Write(node, strBuilder, docPos, anc);
                }
            }
        }

        public void WriteHash(HashAlgorithm hash, DocPosition docPos, AncestralNamespaceContextManager anc)
        {
            docPos = DocPosition.BeforeRootElement;
            foreach (XmlNode node in this.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    CanonicalizationDispatcher.WriteHash(node, hash, DocPosition.InRootElement, anc);
                    docPos = DocPosition.AfterRootElement;
                }
                else
                {
                    CanonicalizationDispatcher.WriteHash(node, hash, docPos, anc);
                }
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

