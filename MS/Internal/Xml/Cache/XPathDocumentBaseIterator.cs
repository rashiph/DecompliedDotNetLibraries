namespace MS.Internal.Xml.Cache
{
    using System;
    using System.Xml.XPath;

    internal abstract class XPathDocumentBaseIterator : XPathNodeIterator
    {
        protected XPathDocumentNavigator ctxt;
        protected int pos;

        protected XPathDocumentBaseIterator(XPathDocumentBaseIterator iter)
        {
            this.ctxt = new XPathDocumentNavigator(iter.ctxt);
            this.pos = iter.pos;
        }

        protected XPathDocumentBaseIterator(XPathDocumentNavigator ctxt)
        {
            this.ctxt = new XPathDocumentNavigator(ctxt);
        }

        public override XPathNavigator Current
        {
            get
            {
                return this.ctxt;
            }
        }

        public override int CurrentPosition
        {
            get
            {
                return this.pos;
            }
        }
    }
}

