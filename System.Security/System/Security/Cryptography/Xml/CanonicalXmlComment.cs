namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using System.Xml;

    internal class CanonicalXmlComment : XmlComment, ICanonicalizableNode
    {
        private bool m_includeComments;
        private bool m_isInNodeSet;

        public CanonicalXmlComment(string comment, XmlDocument doc, bool defaultNodeSetInclusionState, bool includeComments) : base(comment, doc)
        {
            this.m_isInNodeSet = defaultNodeSetInclusionState;
            this.m_includeComments = includeComments;
        }

        public void Write(StringBuilder strBuilder, DocPosition docPos, AncestralNamespaceContextManager anc)
        {
            if (this.IsInNodeSet && this.IncludeComments)
            {
                if (docPos == DocPosition.AfterRootElement)
                {
                    strBuilder.Append('\n');
                }
                strBuilder.Append("<!--");
                strBuilder.Append(this.Value);
                strBuilder.Append("-->");
                if (docPos == DocPosition.BeforeRootElement)
                {
                    strBuilder.Append('\n');
                }
            }
        }

        public void WriteHash(HashAlgorithm hash, DocPosition docPos, AncestralNamespaceContextManager anc)
        {
            if (this.IsInNodeSet && this.IncludeComments)
            {
                UTF8Encoding encoding = new UTF8Encoding(false);
                byte[] bytes = encoding.GetBytes("(char) 10");
                if (docPos == DocPosition.AfterRootElement)
                {
                    hash.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
                }
                bytes = encoding.GetBytes("<!--");
                hash.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
                bytes = encoding.GetBytes(this.Value);
                hash.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
                bytes = encoding.GetBytes("-->");
                hash.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
                if (docPos == DocPosition.BeforeRootElement)
                {
                    bytes = encoding.GetBytes("(char) 10");
                    hash.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
                }
            }
        }

        public bool IncludeComments
        {
            get
            {
                return this.m_includeComments;
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

