namespace MS.Internal.Xml.Cache
{
    using System;
    using System.Text;
    using System.Xml.XPath;

    internal sealed class XPathNodeInfoAtom
    {
        private string baseUri;
        private XPathDocument doc;
        private int hashCode;
        private int lineNumBase;
        private int linePosBase;
        private string localName;
        private int localNameHash;
        private string namespaceUri;
        private XPathNodeInfoAtom next;
        private XPathNodePageInfo pageInfo;
        private XPathNode[] pageParent;
        private XPathNode[] pageSibling;
        private XPathNode[] pageSimilar;
        private string prefix;

        public XPathNodeInfoAtom(XPathNodePageInfo pageInfo)
        {
            this.pageInfo = pageInfo;
        }

        public XPathNodeInfoAtom(string localName, string namespaceUri, string prefix, string baseUri, XPathNode[] pageParent, XPathNode[] pageSibling, XPathNode[] pageSimilar, XPathDocument doc, int lineNumBase, int linePosBase)
        {
            this.Init(localName, namespaceUri, prefix, baseUri, pageParent, pageSibling, pageSimilar, doc, lineNumBase, linePosBase);
        }

        public override bool Equals(object other)
        {
            XPathNodeInfoAtom atom = other as XPathNodeInfoAtom;
            return (((((this.GetHashCode() == atom.GetHashCode()) && (this.localName == atom.localName)) && ((this.pageSibling == atom.pageSibling) && (this.namespaceUri == atom.namespaceUri))) && (((this.pageParent == atom.pageParent) && (this.pageSimilar == atom.pageSimilar)) && ((this.prefix == atom.prefix) && (this.baseUri == atom.baseUri)))) && ((this.lineNumBase == atom.lineNumBase) && (this.linePosBase == atom.linePosBase)));
        }

        public override int GetHashCode()
        {
            if (this.hashCode == 0)
            {
                int localNameHash = this.localNameHash;
                if (this.pageSibling != null)
                {
                    localNameHash += (localNameHash << 7) ^ this.pageSibling[0].PageInfo.PageNumber;
                }
                if (this.pageParent != null)
                {
                    localNameHash += (localNameHash << 7) ^ this.pageParent[0].PageInfo.PageNumber;
                }
                if (this.pageSimilar != null)
                {
                    localNameHash += (localNameHash << 7) ^ this.pageSimilar[0].PageInfo.PageNumber;
                }
                this.hashCode = (localNameHash == 0) ? 1 : localNameHash;
            }
            return this.hashCode;
        }

        public void Init(string localName, string namespaceUri, string prefix, string baseUri, XPathNode[] pageParent, XPathNode[] pageSibling, XPathNode[] pageSimilar, XPathDocument doc, int lineNumBase, int linePosBase)
        {
            this.localName = localName;
            this.namespaceUri = namespaceUri;
            this.prefix = prefix;
            this.baseUri = baseUri;
            this.pageParent = pageParent;
            this.pageSibling = pageSibling;
            this.pageSimilar = pageSimilar;
            this.doc = doc;
            this.lineNumBase = lineNumBase;
            this.linePosBase = linePosBase;
            this.next = null;
            this.pageInfo = null;
            this.hashCode = 0;
            this.localNameHash = 0;
            for (int i = 0; i < this.localName.Length; i++)
            {
                this.localNameHash += (this.localNameHash << 7) ^ this.localName[i];
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("hash=");
            builder.Append(this.GetHashCode());
            builder.Append(", ");
            if (this.localName.Length != 0)
            {
                builder.Append('{');
                builder.Append(this.namespaceUri);
                builder.Append('}');
                if (this.prefix.Length != 0)
                {
                    builder.Append(this.prefix);
                    builder.Append(':');
                }
                builder.Append(this.localName);
                builder.Append(", ");
            }
            if (this.pageParent != null)
            {
                builder.Append("parent=");
                builder.Append(this.pageParent[0].PageInfo.PageNumber);
                builder.Append(", ");
            }
            if (this.pageSibling != null)
            {
                builder.Append("sibling=");
                builder.Append(this.pageSibling[0].PageInfo.PageNumber);
                builder.Append(", ");
            }
            if (this.pageSimilar != null)
            {
                builder.Append("similar=");
                builder.Append(this.pageSimilar[0].PageInfo.PageNumber);
                builder.Append(", ");
            }
            builder.Append("lineNum=");
            builder.Append(this.lineNumBase);
            builder.Append(", ");
            builder.Append("linePos=");
            builder.Append(this.linePosBase);
            return builder.ToString();
        }

        public string BaseUri
        {
            get
            {
                return this.baseUri;
            }
        }

        public XPathDocument Document
        {
            get
            {
                return this.doc;
            }
        }

        public int LineNumberBase
        {
            get
            {
                return this.lineNumBase;
            }
        }

        public int LinePositionBase
        {
            get
            {
                return this.linePosBase;
            }
        }

        public string LocalName
        {
            get
            {
                return this.localName;
            }
        }

        public int LocalNameHashCode
        {
            get
            {
                return this.localNameHash;
            }
        }

        public string NamespaceUri
        {
            get
            {
                return this.namespaceUri;
            }
        }

        public XPathNodeInfoAtom Next
        {
            get
            {
                return this.next;
            }
            set
            {
                this.next = value;
            }
        }

        public XPathNodePageInfo PageInfo
        {
            get
            {
                return this.pageInfo;
            }
        }

        public XPathNode[] ParentPage
        {
            get
            {
                return this.pageParent;
            }
        }

        public string Prefix
        {
            get
            {
                return this.prefix;
            }
        }

        public XPathNode[] SiblingPage
        {
            get
            {
                return this.pageSibling;
            }
        }

        public XPathNode[] SimilarElementPage
        {
            get
            {
                return this.pageSimilar;
            }
        }
    }
}

