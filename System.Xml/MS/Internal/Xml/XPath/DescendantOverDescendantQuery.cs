namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml.XPath;

    internal sealed class DescendantOverDescendantQuery : DescendantBaseQuery
    {
        private int level;

        private DescendantOverDescendantQuery(DescendantOverDescendantQuery other) : base(other)
        {
            this.level = other.level;
        }

        public DescendantOverDescendantQuery(Query qyParent, bool matchSelf, string name, string prefix, XPathNodeType typeTest, bool abbrAxis) : base(qyParent, name, prefix, typeTest, matchSelf, abbrAxis)
        {
        }

        public override XPathNavigator Advance()
        {
        Label_0000:
            while (this.level == 0)
            {
                base.currentNode = base.qyInput.Advance();
                base.position = 0;
                if (base.currentNode == null)
                {
                    return null;
                }
                if (base.matchSelf && this.matches(base.currentNode))
                {
                    base.position = 1;
                    return base.currentNode;
                }
                base.currentNode = base.currentNode.Clone();
                if (this.MoveToFirstChild())
                {
                    goto Label_0071;
                }
            }
            if (!this.MoveUpUntillNext())
            {
                goto Label_0000;
            }
        Label_0071:
            if (this.matches(base.currentNode))
            {
                base.position++;
                return base.currentNode;
            }
            if (this.MoveToFirstChild())
            {
                goto Label_0071;
            }
            goto Label_0000;
        }

        public override XPathNodeIterator Clone()
        {
            return new DescendantOverDescendantQuery(this);
        }

        private bool MoveToFirstChild()
        {
            if (base.currentNode.MoveToFirstChild())
            {
                this.level++;
                return true;
            }
            return false;
        }

        private bool MoveUpUntillNext()
        {
            while (!base.currentNode.MoveToNext())
            {
                this.level--;
                if (this.level == 0)
                {
                    return false;
                }
                base.currentNode.MoveToParent();
            }
            return true;
        }

        public override void Reset()
        {
            this.level = 0;
            base.Reset();
        }
    }
}

