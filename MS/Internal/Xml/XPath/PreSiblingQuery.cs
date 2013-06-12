namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Collections.Generic;
    using System.Xml.XPath;

    internal class PreSiblingQuery : CacheAxisQuery
    {
        protected PreSiblingQuery(PreSiblingQuery other) : base(other)
        {
        }

        public PreSiblingQuery(Query qyInput, string name, string prefix, XPathNodeType typeTest) : base(qyInput, name, prefix, typeTest)
        {
        }

        public override XPathNodeIterator Clone()
        {
            return new PreSiblingQuery(this);
        }

        public override object Evaluate(XPathNodeIterator context)
        {
            base.Evaluate(context);
            List<XPathNavigator> parentStk = new List<XPathNavigator>();
            Stack<XPathNavigator> stack = new Stack<XPathNavigator>();
            while ((base.currentNode = base.qyInput.Advance()) != null)
            {
                stack.Push(base.currentNode.Clone());
            }
            while (stack.Count != 0)
            {
                XPathNavigator nav = stack.Pop();
                if (((nav.NodeType != XPathNodeType.Attribute) && (nav.NodeType != XPathNodeType.Namespace)) && this.NotVisited(nav, parentStk))
                {
                    XPathNavigator e = nav.Clone();
                    if (e.MoveToParent())
                    {
                        e.MoveToFirstChild();
                        while (!e.IsSamePosition(nav))
                        {
                            if (this.matches(e))
                            {
                                base.Insert(base.outputBuffer, e);
                            }
                            if (e.MoveToNext())
                            {
                            }
                        }
                    }
                }
            }
            return this;
        }

        private bool NotVisited(XPathNavigator nav, List<XPathNavigator> parentStk)
        {
            XPathNavigator item = nav.Clone();
            item.MoveToParent();
            for (int i = 0; i < parentStk.Count; i++)
            {
                if (item.IsSamePosition(parentStk[i]))
                {
                    return false;
                }
            }
            parentStk.Add(item);
            return true;
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

