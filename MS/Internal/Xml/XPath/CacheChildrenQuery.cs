namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml;
    using System.Xml.XPath;

    internal sealed class CacheChildrenQuery : ChildrenQuery
    {
        private ClonableStack<XPathNavigator> elementStk;
        private bool needInput;
        private XPathNavigator nextInput;
        private ClonableStack<int> positionStk;

        private CacheChildrenQuery(CacheChildrenQuery other) : base(other)
        {
            this.nextInput = Query.Clone(other.nextInput);
            this.elementStk = other.elementStk.Clone();
            this.positionStk = other.positionStk.Clone();
            this.needInput = other.needInput;
        }

        public CacheChildrenQuery(Query qyInput, string name, string prefix, XPathNodeType type) : base(qyInput, name, prefix, type)
        {
            this.elementStk = new ClonableStack<XPathNavigator>();
            this.positionStk = new ClonableStack<int>();
            this.needInput = true;
        }

        public override XPathNavigator Advance()
        {
        Label_0000:
            if (this.needInput)
            {
                if (this.elementStk.Count == 0)
                {
                    base.currentNode = this.GetNextInput();
                    if (base.currentNode == null)
                    {
                        return null;
                    }
                    if (!base.currentNode.MoveToFirstChild())
                    {
                        goto Label_0000;
                    }
                    base.position = 0;
                }
                else
                {
                    base.currentNode = this.elementStk.Pop();
                    base.position = this.positionStk.Pop();
                    if (!this.DecideNextNode())
                    {
                        goto Label_0000;
                    }
                }
                this.needInput = false;
            }
            else if (!base.currentNode.MoveToNext() || !this.DecideNextNode())
            {
                this.needInput = true;
                goto Label_0000;
            }
            if (!this.matches(base.currentNode))
            {
                goto Label_0000;
            }
            base.position++;
            return base.currentNode;
        }

        public override XPathNodeIterator Clone()
        {
            return new CacheChildrenQuery(this);
        }

        private bool DecideNextNode()
        {
            this.nextInput = this.GetNextInput();
            if ((this.nextInput != null) && (Query.CompareNodes(base.currentNode, this.nextInput) == XmlNodeOrder.After))
            {
                this.elementStk.Push(base.currentNode);
                this.positionStk.Push(base.position);
                base.currentNode = this.nextInput;
                this.nextInput = null;
                if (!base.currentNode.MoveToFirstChild())
                {
                    return false;
                }
                base.position = 0;
            }
            return true;
        }

        private XPathNavigator GetNextInput()
        {
            XPathNavigator nextInput;
            if (this.nextInput != null)
            {
                nextInput = this.nextInput;
                this.nextInput = null;
                return nextInput;
            }
            nextInput = base.qyInput.Advance();
            if (nextInput != null)
            {
                nextInput = nextInput.Clone();
            }
            return nextInput;
        }

        public override void Reset()
        {
            this.nextInput = null;
            this.elementStk.Clear();
            this.positionStk.Clear();
            this.needInput = true;
            base.Reset();
        }
    }
}

