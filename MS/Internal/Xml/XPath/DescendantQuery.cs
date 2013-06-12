namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml.XPath;

    internal class DescendantQuery : DescendantBaseQuery
    {
        private XPathNodeIterator nodeIterator;

        public DescendantQuery(DescendantQuery other) : base(other)
        {
            this.nodeIterator = Query.Clone(other.nodeIterator);
        }

        internal DescendantQuery(Query qyParent, string Name, string Prefix, XPathNodeType Type, bool matchSelf, bool abbrAxis) : base(qyParent, Name, Prefix, Type, matchSelf, abbrAxis)
        {
        }

        public override XPathNavigator Advance()
        {
            while (true)
            {
                if (this.nodeIterator == null)
                {
                    base.position = 0;
                    XPathNavigator navigator = base.qyInput.Advance();
                    if (navigator == null)
                    {
                        return null;
                    }
                    if (base.NameTest)
                    {
                        if (base.TypeTest == XPathNodeType.ProcessingInstruction)
                        {
                            this.nodeIterator = new IteratorFilter(navigator.SelectDescendants(base.TypeTest, base.matchSelf), base.Name);
                        }
                        else
                        {
                            this.nodeIterator = navigator.SelectDescendants(base.Name, base.Namespace, base.matchSelf);
                        }
                    }
                    else
                    {
                        this.nodeIterator = navigator.SelectDescendants(base.TypeTest, base.matchSelf);
                    }
                }
                if (this.nodeIterator.MoveNext())
                {
                    base.position++;
                    base.currentNode = this.nodeIterator.Current;
                    return base.currentNode;
                }
                this.nodeIterator = null;
            }
        }

        public override XPathNodeIterator Clone()
        {
            return new DescendantQuery(this);
        }

        public override void Reset()
        {
            this.nodeIterator = null;
            base.Reset();
        }
    }
}

