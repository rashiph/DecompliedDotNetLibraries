namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml.XPath;

    internal class XPathAncestorIterator : XPathAxisIterator
    {
        public XPathAncestorIterator(XPathAncestorIterator other) : base(other)
        {
        }

        public XPathAncestorIterator(XPathNavigator nav, XPathNodeType type, bool matchSelf) : base(nav, type, matchSelf)
        {
        }

        public XPathAncestorIterator(XPathNavigator nav, string name, string namespaceURI, bool matchSelf) : base(nav, name, namespaceURI, matchSelf)
        {
        }

        public override XPathNodeIterator Clone()
        {
            return new XPathAncestorIterator(this);
        }

        public override bool MoveNext()
        {
            if (base.first)
            {
                base.first = false;
                if (base.matchSelf && this.Matches)
                {
                    base.position = 1;
                    return true;
                }
            }
            while (base.nav.MoveToParent())
            {
                if (this.Matches)
                {
                    base.position++;
                    return true;
                }
            }
            return false;
        }
    }
}

