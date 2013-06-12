namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml.XPath;

    internal class XPathSingletonIterator : ResetableIterator
    {
        private XPathNavigator nav;
        private int position;

        public XPathSingletonIterator(XPathSingletonIterator it)
        {
            this.nav = it.nav.Clone();
            this.position = it.position;
        }

        public XPathSingletonIterator(XPathNavigator nav)
        {
            this.nav = nav;
        }

        public XPathSingletonIterator(XPathNavigator nav, bool moved) : this(nav)
        {
            if (moved)
            {
                this.position = 1;
            }
        }

        public override XPathNodeIterator Clone()
        {
            return new XPathSingletonIterator(this);
        }

        public override bool MoveNext()
        {
            if (this.position == 0)
            {
                this.position = 1;
                return true;
            }
            return false;
        }

        public override void Reset()
        {
            this.position = 0;
        }

        public override int Count
        {
            get
            {
                return 1;
            }
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
    }
}

