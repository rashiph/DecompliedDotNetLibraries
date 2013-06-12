namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml.XPath;

    internal sealed class FollowingQuery : BaseAxisQuery
    {
        private XPathNavigator input;
        private XPathNodeIterator iterator;

        private FollowingQuery(FollowingQuery other) : base((BaseAxisQuery) other)
        {
            this.input = Query.Clone(other.input);
            this.iterator = Query.Clone(other.iterator);
        }

        public FollowingQuery(Query qyInput, string name, string prefix, XPathNodeType typeTest) : base(qyInput, name, prefix, typeTest)
        {
        }

        public override XPathNavigator Advance()
        {
            if (this.iterator == null)
            {
                XPathNavigator navigator;
                this.input = base.qyInput.Advance();
                if (this.input == null)
                {
                    return null;
                }
                do
                {
                    navigator = this.input.Clone();
                    this.input = base.qyInput.Advance();
                }
                while (navigator.IsDescendant(this.input));
                this.input = navigator;
                this.iterator = XPathEmptyIterator.Instance;
            }
            while (!this.iterator.MoveNext())
            {
                if ((this.input.NodeType != XPathNodeType.Attribute) && (this.input.NodeType != XPathNodeType.Namespace))
                {
                    goto Label_00A3;
                }
                this.input.MoveToParent();
                bool matchSelf = false;
                goto Label_00B2;
            Label_0094:
                if (!this.input.MoveToParent())
                {
                    return null;
                }
            Label_00A3:
                if (!this.input.MoveToNext())
                {
                    goto Label_0094;
                }
                matchSelf = true;
            Label_00B2:
                if (base.NameTest)
                {
                    this.iterator = this.input.SelectDescendants(base.Name, base.Namespace, matchSelf);
                }
                else
                {
                    this.iterator = this.input.SelectDescendants(base.TypeTest, matchSelf);
                }
            }
            base.position++;
            base.currentNode = this.iterator.Current;
            return base.currentNode;
        }

        public override XPathNodeIterator Clone()
        {
            return new FollowingQuery(this);
        }

        public override void Reset()
        {
            this.iterator = null;
            base.Reset();
        }
    }
}

