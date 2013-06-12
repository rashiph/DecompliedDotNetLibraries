namespace MS.Internal.Xml.Cache
{
    using System;
    using System.Xml.XPath;

    internal class XPathDocumentElementChildIterator : XPathDocumentBaseIterator
    {
        private string localName;
        private string namespaceUri;

        public XPathDocumentElementChildIterator(XPathDocumentElementChildIterator iter) : base(iter)
        {
            this.localName = iter.localName;
            this.namespaceUri = iter.namespaceUri;
        }

        public XPathDocumentElementChildIterator(XPathDocumentNavigator parent, string name, string namespaceURI) : base(parent)
        {
            if (namespaceURI == null)
            {
                throw new ArgumentNullException("namespaceURI");
            }
            this.localName = parent.NameTable.Get(name);
            this.namespaceUri = namespaceURI;
        }

        public override XPathNodeIterator Clone()
        {
            return new XPathDocumentElementChildIterator(this);
        }

        public override bool MoveNext()
        {
            if (base.pos == 0)
            {
                if (!base.ctxt.MoveToChild(this.localName, this.namespaceUri))
                {
                    return false;
                }
            }
            else if (!base.ctxt.MoveToNext(this.localName, this.namespaceUri))
            {
                return false;
            }
            base.pos++;
            return true;
        }
    }
}

