namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml.XPath;

    internal sealed class XPathSelfQuery : BaseAxisQuery
    {
        private XPathSelfQuery(XPathSelfQuery other) : base((BaseAxisQuery) other)
        {
        }

        public XPathSelfQuery(Query qyInput, string Name, string Prefix, XPathNodeType Type) : base(qyInput, Name, Prefix, Type)
        {
        }

        public override XPathNavigator Advance()
        {
            while ((base.currentNode = base.qyInput.Advance()) != null)
            {
                if (this.matches(base.currentNode))
                {
                    base.position = 1;
                    return base.currentNode;
                }
            }
            return null;
        }

        public override XPathNodeIterator Clone()
        {
            return new XPathSelfQuery(this);
        }
    }
}

