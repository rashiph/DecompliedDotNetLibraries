namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using System.Xml;

    internal class CanonicalXmlProcessingInstruction : XmlProcessingInstruction, ICanonicalizableNode
    {
        private bool m_isInNodeSet;

        public CanonicalXmlProcessingInstruction(string target, string data, XmlDocument doc, bool defaultNodeSetInclusionState) : base(target, data, doc)
        {
            this.m_isInNodeSet = defaultNodeSetInclusionState;
        }

        public void Write(StringBuilder strBuilder, DocPosition docPos, AncestralNamespaceContextManager anc)
        {
            if (this.IsInNodeSet)
            {
                if (docPos == DocPosition.AfterRootElement)
                {
                    strBuilder.Append('\n');
                }
                strBuilder.Append("<?");
                strBuilder.Append(this.Name);
                if ((this.Value != null) && (this.Value.Length > 0))
                {
                    strBuilder.Append(" " + this.Value);
                }
                strBuilder.Append("?>");
                if (docPos == DocPosition.BeforeRootElement)
                {
                    strBuilder.Append('\n');
                }
            }
        }

        public void WriteHash(HashAlgorithm hash, DocPosition docPos, AncestralNamespaceContextManager anc)
        {
            if (this.IsInNodeSet)
            {
                byte[] bytes;
                UTF8Encoding encoding = new UTF8Encoding(false);
                if (docPos == DocPosition.AfterRootElement)
                {
                    bytes = encoding.GetBytes("(char) 10");
                    hash.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
                }
                bytes = encoding.GetBytes("<?");
                hash.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
                bytes = encoding.GetBytes(this.Name);
                hash.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
                if ((this.Value != null) && (this.Value.Length > 0))
                {
                    bytes = encoding.GetBytes(" " + this.Value);
                    hash.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
                }
                bytes = encoding.GetBytes("?>");
                hash.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
                if (docPos == DocPosition.BeforeRootElement)
                {
                    bytes = encoding.GetBytes("(char) 10");
                    hash.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
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

