namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml.XPath;

    internal class ForwardPositionQuery : CacheOutputQuery
    {
        protected ForwardPositionQuery(ForwardPositionQuery other) : base((CacheOutputQuery) other)
        {
        }

        public ForwardPositionQuery(Query input) : base(input)
        {
        }

        public override XPathNodeIterator Clone()
        {
            return new ForwardPositionQuery(this);
        }

        public override object Evaluate(XPathNodeIterator context)
        {
            XPathNavigator navigator;
            base.Evaluate(context);
            while ((navigator = base.input.Advance()) != null)
            {
                base.outputBuffer.Add(navigator.Clone());
            }
            return this;
        }

        public override XPathNavigator MatchNode(XPathNavigator context)
        {
            return base.input.MatchNode(context);
        }
    }
}

