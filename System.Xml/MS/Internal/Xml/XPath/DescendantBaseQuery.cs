namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml;
    using System.Xml.XPath;

    internal abstract class DescendantBaseQuery : BaseAxisQuery
    {
        protected bool abbrAxis;
        protected bool matchSelf;

        public DescendantBaseQuery(DescendantBaseQuery other) : base((BaseAxisQuery) other)
        {
            this.matchSelf = other.matchSelf;
            this.abbrAxis = other.abbrAxis;
        }

        public DescendantBaseQuery(Query qyParent, string Name, string Prefix, XPathNodeType Type, bool matchSelf, bool abbrAxis) : base(qyParent, Name, Prefix, Type)
        {
            this.matchSelf = matchSelf;
            this.abbrAxis = abbrAxis;
        }

        public override XPathNavigator MatchNode(XPathNavigator context)
        {
            if (context != null)
            {
                if (!this.abbrAxis)
                {
                    throw XPathException.Create("Xp_InvalidPattern");
                }
                XPathNavigator navigator = null;
                if (this.matches(context))
                {
                    if (this.matchSelf && ((navigator = base.qyInput.MatchNode(context)) != null))
                    {
                        return navigator;
                    }
                    XPathNavigator current = context.Clone();
                    while (current.MoveToParent())
                    {
                        navigator = base.qyInput.MatchNode(current);
                        if (navigator != null)
                        {
                            return navigator;
                        }
                    }
                }
            }
            return null;
        }

        public override void PrintQuery(XmlWriter w)
        {
            w.WriteStartElement(base.GetType().Name);
            if (this.matchSelf)
            {
                w.WriteAttributeString("self", "yes");
            }
            if (base.NameTest)
            {
                w.WriteAttributeString("name", (base.Prefix.Length != 0) ? (base.Prefix + ':' + base.Name) : base.Name);
            }
            if (base.TypeTest != XPathNodeType.Element)
            {
                w.WriteAttributeString("nodeType", base.TypeTest.ToString());
            }
            base.qyInput.PrintQuery(w);
            w.WriteEndElement();
        }
    }
}

