namespace MS.Internal.Xml.Cache
{
    using System;
    using System.Xml.XPath;

    internal class XPathDocumentKindChildIterator : XPathDocumentBaseIterator
    {
        private XPathNodeType typ;

        public XPathDocumentKindChildIterator(XPathDocumentKindChildIterator iter) : base(iter)
        {
            this.typ = iter.typ;
        }

        public XPathDocumentKindChildIterator(XPathDocumentNavigator parent, XPathNodeType typ) : base(parent)
        {
            this.typ = typ;
        }

        public override XPathNodeIterator Clone()
        {
            return new XPathDocumentKindChildIterator(this);
        }

        public override bool MoveNext()
        {
            if (base.pos == 0)
            {
                if (!base.ctxt.MoveToChild(this.typ))
                {
                    return false;
                }
            }
            else if (!base.ctxt.MoveToNext(this.typ))
            {
                return false;
            }
            base.pos++;
            return true;
        }
    }
}

