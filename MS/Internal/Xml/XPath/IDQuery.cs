namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml;
    using System.Xml.XPath;

    internal sealed class IDQuery : CacheOutputQuery
    {
        private IDQuery(IDQuery other) : base((CacheOutputQuery) other)
        {
        }

        public IDQuery(Query arg) : base(arg)
        {
        }

        public override XPathNodeIterator Clone()
        {
            return new IDQuery(this);
        }

        public override object Evaluate(XPathNodeIterator context)
        {
            object obj2 = base.Evaluate(context);
            XPathNavigator contextNode = context.Current.Clone();
            switch (base.GetXPathType(obj2))
            {
                case XPathResultType.Number:
                    this.ProcessIds(contextNode, StringFunctions.toString((double) obj2));
                    break;

                case XPathResultType.String:
                    this.ProcessIds(contextNode, (string) obj2);
                    break;

                case XPathResultType.Boolean:
                    this.ProcessIds(contextNode, StringFunctions.toString((bool) obj2));
                    break;

                case XPathResultType.NodeSet:
                    XPathNavigator navigator2;
                    while ((navigator2 = base.input.Advance()) != null)
                    {
                        this.ProcessIds(contextNode, navigator2.Value);
                    }
                    break;

                case ((XPathResultType) 4):
                    this.ProcessIds(contextNode, ((XPathNavigator) obj2).Value);
                    break;
            }
            return this;
        }

        public override XPathNavigator MatchNode(XPathNavigator context)
        {
            XPathNavigator navigator;
            this.Evaluate(new XPathSingletonIterator(context, true));
            while ((navigator = this.Advance()) != null)
            {
                if (navigator.IsSamePosition(context))
                {
                    return context;
                }
            }
            return null;
        }

        private void ProcessIds(XPathNavigator contextNode, string val)
        {
            string[] strArray = XmlConvert.SplitString(val);
            for (int i = 0; i < strArray.Length; i++)
            {
                if (contextNode.MoveToId(strArray[i]))
                {
                    base.Insert(base.outputBuffer, contextNode);
                }
            }
        }
    }
}

