namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml.XPath;

    internal sealed class PrecedingQuery : BaseAxisQuery
    {
        private ClonableStack<XPathNavigator> ancestorStk;
        private XPathNodeIterator workIterator;

        private PrecedingQuery(PrecedingQuery other) : base((BaseAxisQuery) other)
        {
            this.workIterator = Query.Clone(other.workIterator);
            this.ancestorStk = other.ancestorStk.Clone();
        }

        public PrecedingQuery(Query qyInput, string name, string prefix, XPathNodeType typeTest) : base(qyInput, name, prefix, typeTest)
        {
            this.ancestorStk = new ClonableStack<XPathNavigator>();
        }

        public override XPathNavigator Advance()
        {
            if (this.workIterator == null)
            {
                XPathNavigator other = base.qyInput.Advance();
                if (other == null)
                {
                    return null;
                }
                XPathNavigator navigator = other.Clone();
                do
                {
                    navigator.MoveTo(other);
                }
                while ((other = base.qyInput.Advance()) != null);
                if ((navigator.NodeType == XPathNodeType.Attribute) || (navigator.NodeType == XPathNodeType.Namespace))
                {
                    navigator.MoveToParent();
                }
                do
                {
                    this.ancestorStk.Push(navigator.Clone());
                }
                while (navigator.MoveToParent());
                this.workIterator = navigator.SelectDescendants(XPathNodeType.All, true);
            }
            while (this.workIterator.MoveNext())
            {
                base.currentNode = this.workIterator.Current;
                if (base.currentNode.IsSamePosition(this.ancestorStk.Peek()))
                {
                    this.ancestorStk.Pop();
                    if (this.ancestorStk.Count == 0)
                    {
                        base.currentNode = null;
                        this.workIterator = null;
                        return null;
                    }
                }
                else if (this.matches(base.currentNode))
                {
                    base.position++;
                    return base.currentNode;
                }
            }
            return null;
        }

        public override XPathNodeIterator Clone()
        {
            return new PrecedingQuery(this);
        }

        public override void Reset()
        {
            this.workIterator = null;
            this.ancestorStk.Clear();
            base.Reset();
        }

        public override QueryProps Properties
        {
            get
            {
                return (base.Properties | QueryProps.Reverse);
            }
        }
    }
}

