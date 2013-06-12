namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml.XPath;

    internal sealed class AbsoluteQuery : ContextQuery
    {
        public AbsoluteQuery()
        {
        }

        private AbsoluteQuery(AbsoluteQuery other) : base(other)
        {
        }

        public override XPathNodeIterator Clone()
        {
            return new AbsoluteQuery(this);
        }

        public override object Evaluate(XPathNodeIterator context)
        {
            base.contextNode = context.Current.Clone();
            base.contextNode.MoveToRoot();
            base.count = 0;
            return this;
        }

        public override XPathNavigator MatchNode(XPathNavigator context)
        {
            if ((context != null) && (context.NodeType == XPathNodeType.Root))
            {
                return context;
            }
            return null;
        }
    }
}

