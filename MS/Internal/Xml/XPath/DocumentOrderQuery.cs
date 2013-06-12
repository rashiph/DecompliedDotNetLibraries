namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml.XPath;

    internal sealed class DocumentOrderQuery : CacheOutputQuery
    {
        private DocumentOrderQuery(DocumentOrderQuery other) : base((CacheOutputQuery) other)
        {
        }

        public DocumentOrderQuery(Query qyParent) : base(qyParent)
        {
        }

        public override XPathNodeIterator Clone()
        {
            return new DocumentOrderQuery(this);
        }

        public override object Evaluate(XPathNodeIterator context)
        {
            XPathNavigator navigator;
            base.Evaluate(context);
            while ((navigator = base.input.Advance()) != null)
            {
                base.Insert(base.outputBuffer, navigator);
            }
            return this;
        }

        public override XPathNavigator MatchNode(XPathNavigator context)
        {
            return base.input.MatchNode(context);
        }
    }
}

