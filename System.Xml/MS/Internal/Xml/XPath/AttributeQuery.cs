namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml.XPath;

    internal sealed class AttributeQuery : BaseAxisQuery
    {
        private bool onAttribute;

        private AttributeQuery(AttributeQuery other) : base((BaseAxisQuery) other)
        {
            this.onAttribute = other.onAttribute;
        }

        public AttributeQuery(Query qyParent, string Name, string Prefix, XPathNodeType Type) : base(qyParent, Name, Prefix, Type)
        {
        }

        public override XPathNavigator Advance()
        {
            do
            {
                if (!this.onAttribute)
                {
                    base.currentNode = base.qyInput.Advance();
                    if (base.currentNode == null)
                    {
                        return null;
                    }
                    base.position = 0;
                    base.currentNode = base.currentNode.Clone();
                    this.onAttribute = base.currentNode.MoveToFirstAttribute();
                }
                else
                {
                    this.onAttribute = base.currentNode.MoveToNextAttribute();
                }
            }
            while (!this.onAttribute || !this.matches(base.currentNode));
            base.position++;
            return base.currentNode;
        }

        public override XPathNodeIterator Clone()
        {
            return new AttributeQuery(this);
        }

        public override XPathNavigator MatchNode(XPathNavigator context)
        {
            if (((context != null) && (context.NodeType == XPathNodeType.Attribute)) && this.matches(context))
            {
                XPathNavigator current = context.Clone();
                if (current.MoveToParent())
                {
                    return base.qyInput.MatchNode(current);
                }
            }
            return null;
        }

        public override void Reset()
        {
            this.onAttribute = false;
            base.Reset();
        }
    }
}

