namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml.XPath;

    internal class XPathSelectionIterator : ResetableIterator
    {
        private XPathNavigator nav;
        private int position;
        private Query query;

        protected XPathSelectionIterator(XPathSelectionIterator it)
        {
            this.nav = it.nav.Clone();
            this.query = (Query) it.query.Clone();
            this.position = it.position;
        }

        internal XPathSelectionIterator(XPathNavigator nav, Query query)
        {
            this.nav = nav.Clone();
            this.query = query;
        }

        public override XPathNodeIterator Clone()
        {
            return new XPathSelectionIterator(this);
        }

        public override bool MoveNext()
        {
            XPathNavigator other = this.query.Advance();
            if (other == null)
            {
                return false;
            }
            this.position++;
            if (!this.nav.MoveTo(other))
            {
                this.nav = other.Clone();
            }
            return true;
        }

        public override void Reset()
        {
            this.query.Reset();
        }

        public override int Count
        {
            get
            {
                return this.query.Count;
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

