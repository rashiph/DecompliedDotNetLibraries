namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Collections.Generic;
    using System.Xml.XPath;

    internal sealed class FollSiblingQuery : BaseAxisQuery
    {
        private ClonableStack<XPathNavigator> elementStk;
        private XPathNavigator nextInput;
        private List<XPathNavigator> parentStk;

        private FollSiblingQuery(FollSiblingQuery other) : base((BaseAxisQuery) other)
        {
            this.elementStk = other.elementStk.Clone();
            this.parentStk = new List<XPathNavigator>(other.parentStk);
            this.nextInput = Query.Clone(other.nextInput);
        }

        public FollSiblingQuery(Query qyInput, string name, string prefix, XPathNodeType type) : base(qyInput, name, prefix, type)
        {
            this.elementStk = new ClonableStack<XPathNavigator>();
            this.parentStk = new List<XPathNavigator>();
        }

        public override XPathNavigator Advance()
        {
            while (true)
            {
                if (base.currentNode == null)
                {
                    if (this.nextInput == null)
                    {
                        this.nextInput = this.FetchInput();
                    }
                    if (this.elementStk.Count == 0)
                    {
                        if (this.nextInput == null)
                        {
                            return null;
                        }
                        base.currentNode = this.nextInput;
                        this.nextInput = this.FetchInput();
                    }
                    else
                    {
                        base.currentNode = this.elementStk.Pop();
                    }
                }
                while (base.currentNode.IsDescendant(this.nextInput))
                {
                    this.elementStk.Push(base.currentNode);
                    base.currentNode = this.nextInput;
                    this.nextInput = base.qyInput.Advance();
                    if (this.nextInput != null)
                    {
                        this.nextInput = this.nextInput.Clone();
                    }
                }
                while (base.currentNode.MoveToNext())
                {
                    if (this.matches(base.currentNode))
                    {
                        base.position++;
                        return base.currentNode;
                    }
                }
                base.currentNode = null;
            }
        }

        public override XPathNodeIterator Clone()
        {
            return new FollSiblingQuery(this);
        }

        private XPathNavigator FetchInput()
        {
            XPathNavigator navigator;
            do
            {
                navigator = base.qyInput.Advance();
                if (navigator == null)
                {
                    return null;
                }
            }
            while (this.Visited(navigator));
            return navigator.Clone();
        }

        public override void Reset()
        {
            this.elementStk.Clear();
            this.parentStk.Clear();
            this.nextInput = null;
            base.Reset();
        }

        private bool Visited(XPathNavigator nav)
        {
            XPathNavigator item = nav.Clone();
            item.MoveToParent();
            for (int i = 0; i < this.parentStk.Count; i++)
            {
                if (item.IsSamePosition(this.parentStk[i]))
                {
                    return true;
                }
            }
            this.parentStk.Add(item);
            return false;
        }
    }
}

