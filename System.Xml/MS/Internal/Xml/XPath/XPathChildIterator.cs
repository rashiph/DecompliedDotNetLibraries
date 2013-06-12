namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml.XPath;

    internal class XPathChildIterator : XPathAxisIterator
    {
        public XPathChildIterator(XPathChildIterator it) : base(it)
        {
        }

        public XPathChildIterator(XPathNavigator nav, XPathNodeType type) : base(nav, type, false)
        {
        }

        public XPathChildIterator(XPathNavigator nav, string name, string namespaceURI) : base(nav, name, namespaceURI, false)
        {
        }

        public override XPathNodeIterator Clone()
        {
            return new XPathChildIterator(this);
        }

        public override bool MoveNext()
        {
            while (base.first ? base.nav.MoveToFirstChild() : base.nav.MoveToNext())
            {
                base.first = false;
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

