namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml.XPath;

    internal abstract class XPathAxisIterator : XPathNodeIterator
    {
        internal bool first;
        internal bool matchSelf;
        internal string name;
        internal XPathNavigator nav;
        internal int position;
        internal XPathNodeType type;
        internal string uri;

        public XPathAxisIterator(XPathAxisIterator it)
        {
            this.first = true;
            this.nav = it.nav.Clone();
            this.type = it.type;
            this.name = it.name;
            this.uri = it.uri;
            this.position = it.position;
            this.matchSelf = it.matchSelf;
            this.first = it.first;
        }

        public XPathAxisIterator(XPathNavigator nav, bool matchSelf)
        {
            this.first = true;
            this.nav = nav;
            this.matchSelf = matchSelf;
        }

        public XPathAxisIterator(XPathNavigator nav, XPathNodeType type, bool matchSelf) : this(nav, matchSelf)
        {
            this.type = type;
        }

        public XPathAxisIterator(XPathNavigator nav, string name, string namespaceURI, bool matchSelf) : this(nav, matchSelf)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (namespaceURI == null)
            {
                throw new ArgumentNullException("namespaceURI");
            }
            this.name = name;
            this.uri = namespaceURI;
        }

        public override XPathNavigator Current
        {
            get
            {
                return this.nav;
            }
        }

        public override int CurrentPosition
        {
            get
            {
                return this.position;
            }
        }

        protected virtual bool Matches
        {
            get
            {
                if (this.name == null)
                {
                    if ((this.type != this.nav.NodeType) && (this.type != XPathNodeType.All))
                    {
                        if (this.type != XPathNodeType.Text)
                        {
                            return false;
                        }
                        if (this.nav.NodeType != XPathNodeType.Whitespace)
                        {
                            return (this.nav.NodeType == XPathNodeType.SignificantWhitespace);
                        }
                    }
                    return true;
                }
                if ((this.nav.NodeType != XPathNodeType.Element) || ((this.name.Length != 0) && !(this.name == this.nav.LocalName)))
                {
                    return false;
                }
                return (this.uri == this.nav.NamespaceURI);
            }
        }
    }
}

