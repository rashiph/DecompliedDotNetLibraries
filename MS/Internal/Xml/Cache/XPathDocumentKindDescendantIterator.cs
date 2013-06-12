namespace MS.Internal.Xml.Cache
{
    using System;
    using System.Xml.XPath;

    internal class XPathDocumentKindDescendantIterator : XPathDocumentBaseIterator
    {
        private XPathDocumentNavigator end;
        private bool matchSelf;
        private XPathNodeType typ;

        public XPathDocumentKindDescendantIterator(XPathDocumentKindDescendantIterator iter) : base(iter)
        {
            this.end = iter.end;
            this.typ = iter.typ;
            this.matchSelf = iter.matchSelf;
        }

        public XPathDocumentKindDescendantIterator(XPathDocumentNavigator root, XPathNodeType typ, bool matchSelf) : base(root)
        {
            this.typ = typ;
            this.matchSelf = matchSelf;
            if (root.NodeType != XPathNodeType.Root)
            {
                this.end = new XPathDocumentNavigator(root);
                this.end.MoveToNonDescendant();
            }
        }

        public override XPathNodeIterator Clone()
        {
            return new XPathDocumentKindDescendantIterator(this);
        }

        public override bool MoveNext()
        {
            if (this.matchSelf)
            {
                this.matchSelf = false;
                if (base.ctxt.IsKindMatch(this.typ))
                {
                    base.pos++;
                    return true;
                }
            }
            if (!base.ctxt.MoveToFollowing(this.typ, this.end))
            {
                return false;
            }
            base.pos++;
            return true;
        }
    }
}

