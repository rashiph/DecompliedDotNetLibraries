namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml.XPath;

    internal class XPathDescendantIterator : XPathAxisIterator
    {
        private int level;

        public XPathDescendantIterator(XPathDescendantIterator it) : base(it)
        {
            this.level = it.level;
        }

        public XPathDescendantIterator(XPathNavigator nav, XPathNodeType type, bool matchSelf) : base(nav, type, matchSelf)
        {
        }

        public XPathDescendantIterator(XPathNavigator nav, string name, string namespaceURI, bool matchSelf) : base(nav, name, namespaceURI, matchSelf)
        {
        }

        public override XPathNodeIterator Clone()
        {
            return new XPathDescendantIterator(this);
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
        Label_0028:
            if (base.nav.MoveToFirstChild())
            {
                this.level++;
                goto Label_0078;
            }
        Label_0045:
            if (this.level == 0)
            {
                return false;
            }
            if (!base.nav.MoveToNext())
            {
                base.nav.MoveToParent();
                this.level--;
                goto Label_0045;
            }
        Label_0078:
            if (!this.Matches)
            {
                goto Label_0028;
            }
            base.position++;
            return true;
        }
    }
}

