namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml.XPath;

    internal class ContextQuery : Query
    {
        protected XPathNavigator contextNode;

        public ContextQuery()
        {
            base.count = 0;
        }

        protected ContextQuery(ContextQuery other) : base(other)
        {
            this.contextNode = other.contextNode;
        }

        public override XPathNavigator Advance()
        {
            if (base.count == 0)
            {
                base.count = 1;
                return this.contextNode;
            }
            return null;
        }

        public override XPathNodeIterator Clone()
        {
            return new ContextQuery(this);
        }

        public override object Evaluate(XPathNodeIterator context)
        {
            this.contextNode = context.Current;
            base.count = 0;
            return this;
        }

        public override XPathNavigator MatchNode(XPathNavigator current)
        {
            return current;
        }

        public override void Reset()
        {
            base.count = 0;
        }

        public override int Count
        {
            get
            {
                return 1;
            }
        }

        public override XPathNavigator Current
        {
            get
            {
                return this.contextNode;
            }
        }

        public override int CurrentPosition
        {
            get
            {
                return base.count;
            }
        }

        public override QueryProps Properties
        {
            get
            {
                return (QueryProps.Merge | QueryProps.Cached | QueryProps.Count | QueryProps.Position);
            }
        }

        public override XPathResultType StaticType
        {
            get
            {
                return XPathResultType.NodeSet;
            }
        }
    }
}

