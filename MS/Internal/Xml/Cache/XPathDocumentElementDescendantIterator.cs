namespace MS.Internal.Xml.Cache
{
    using System;
    using System.Xml.XPath;

    internal class XPathDocumentElementDescendantIterator : XPathDocumentBaseIterator
    {
        private XPathDocumentNavigator end;
        private string localName;
        private bool matchSelf;
        private string namespaceUri;

        public XPathDocumentElementDescendantIterator(XPathDocumentElementDescendantIterator iter) : base(iter)
        {
            this.end = iter.end;
            this.localName = iter.localName;
            this.namespaceUri = iter.namespaceUri;
            this.matchSelf = iter.matchSelf;
        }

        public XPathDocumentElementDescendantIterator(XPathDocumentNavigator root, string name, string namespaceURI, bool matchSelf) : base(root)
        {
            if (namespaceURI == null)
            {
                throw new ArgumentNullException("namespaceURI");
            }
            this.localName = root.NameTable.Get(name);
            this.namespaceUri = namespaceURI;
            this.matchSelf = matchSelf;
            if (root.NodeType != XPathNodeType.Root)
            {
                this.end = new XPathDocumentNavigator(root);
                this.end.MoveToNonDescendant();
            }
        }

        public override XPathNodeIterator Clone()
        {
            return new XPathDocumentElementDescendantIterator(this);
        }

        public override bool MoveNext()
        {
            if (this.matchSelf)
            {
                this.matchSelf = false;
                if (base.ctxt.IsElementMatch(this.localName, this.namespaceUri))
                {
                    base.pos++;
                    return true;
                }
            }
            if (!base.ctxt.MoveToFollowing(this.localName, this.namespaceUri, this.end))
            {
                return false;
            }
            base.pos++;
            return true;
        }
    }
}

