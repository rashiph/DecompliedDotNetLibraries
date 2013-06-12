namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml.XPath;

    internal class ChildrenQuery : BaseAxisQuery
    {
        private XPathNodeIterator iterator;

        protected ChildrenQuery(ChildrenQuery other) : base((BaseAxisQuery) other)
        {
            this.iterator = XPathEmptyIterator.Instance;
            this.iterator = Query.Clone(other.iterator);
        }

        public ChildrenQuery(Query qyInput, string name, string prefix, XPathNodeType type) : base(qyInput, name, prefix, type)
        {
            this.iterator = XPathEmptyIterator.Instance;
        }

        public override XPathNavigator Advance()
        {
            while (!this.iterator.MoveNext())
            {
                XPathNavigator navigator = base.qyInput.Advance();
                if (navigator == null)
                {
                    return null;
                }
                if (base.NameTest)
                {
                    if (base.TypeTest == XPathNodeType.ProcessingInstruction)
                    {
                        this.iterator = new IteratorFilter(navigator.SelectChildren(base.TypeTest), base.Name);
                    }
                    else
                    {
                        this.iterator = navigator.SelectChildren(base.Name, base.Namespace);
                    }
                }
                else
                {
                    this.iterator = navigator.SelectChildren(base.TypeTest);
                }
                base.position = 0;
            }
            base.position++;
            base.currentNode = this.iterator.Current;
            return base.currentNode;
        }

        public override XPathNodeIterator Clone()
        {
            return new ChildrenQuery(this);
        }

        public sealed override XPathNavigator MatchNode(XPathNavigator context)
        {
            if ((context != null) && this.matches(context))
            {
                XPathNavigator current = context.Clone();
                if ((current.NodeType != XPathNodeType.Attribute) && current.MoveToParent())
                {
                    return base.qyInput.MatchNode(current);
                }
            }
            return null;
        }

        public override void Reset()
        {
            this.iterator = XPathEmptyIterator.Instance;
            base.Reset();
        }
    }
}

